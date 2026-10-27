using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Turris
{
    public static class TelegraphColors
    {
        public static Color For(TelegraphKind k)
        {
            switch (k)
            {
                case TelegraphKind.Heavy: return new Color(1f, 0.55f, 0.1f);
                case TelegraphKind.Unparryable: return new Color(0.75f, 0.35f, 1f);
                case TelegraphKind.Unblockable: return new Color(1f, 0.1f, 0.1f);
                case TelegraphKind.NoDodge: return new Color(1f, 0.2f, 0.8f);
                default: return new Color(0.9f, 0.9f, 0.9f);
            }
        }
    }

    /// <summary>
    /// Wspólne AI wszystkich przeciwników. Archetyp (szybki, opancerzony, dystansowy, boss) wynika
    /// wyłącznie z danych w <see cref="EnemyDefinition"/> – bez podklas.
    /// Stany: Dormant → Moving ⇄ Attacking; reakcje: Guarding, Evading, Staggered, Recoil, PoiseBroken (okno riposty), Dead.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public partial class EnemyBrain : MonoBehaviour, IHitReceiver, IRiposteTarget, ICharacterAnimSource, IHitStop
    {
        public static readonly List<EnemyBrain> All = new List<EnemyBrain>();

        public enum State { Dormant, Moving, Attacking, Guarding, Evading, Staggered, Recoil, PoiseBroken, Dead,
            /// <summary>Czerw (Hell Unleashed) wgryziony w bohatera: jedzie na nim, spowalnia go i co sekundę rani.</summary>
            Latched }

        public EnemyDefinition Def { get; private set; }
        public State CurrentState { get; private set; } = State.Dormant;
        public ResourcePool Health { get; private set; }
        public ResourcePool Poise { get; private set; }
        public ResourcePool Stamina { get; private set; }
        public bool IsElite { get; private set; }
        public int PhaseIndex { get; private set; }
        public float StateTimer => stateTimer;

        public Faction Faction => Faction.Enemy;
        public bool IsDead => CurrentState == State.Dead;
        public Transform Transform => transform;
        public Vector3 AimPoint => transform.position + Vector3.up * (1.0f * Def.scale);
        public bool CanBeRiposted => CurrentState == State.PoiseBroken;

        public string DisplayName => IsElite ? $"{Def.displayName} (Elita)" : Def.displayName;

        public event Action<EnemyBrain> Died;

        /// <summary>Przywołanie sług (Władca Bramy): kto przyzywa, kogo i gdzie. Obsługuje je GameRoot (lista wrogów piętra).</summary>
        public static event Action<EnemyBrain, EnemyDefinition, Vector3> SummonRequested;
        /// <summary>Przyzywający – sługa ginie razem z nim.</summary>
        public EnemyBrain Owner { get; set; }
        /// <summary>Ile czerwi jest teraz uczepionych bohatera (najwyżej 3 naraz, jak w Hell Unleashed).</summary>
        public static int LatchedCount { get; private set; }
        public const int MaxLatched = 3;

        /// <summary>Efekty trwające (krwawienie, podpalenie, chłód, porażenie, zamrożenie).</summary>
        public readonly StatusEffects Status = new StatusEffects();
        readonly StatusFx statusFx = new StatusFx();

        // Skalowanie
        float damageMult = 1f, aggressionMult = 1f;
        float DamageMult => damageMult * (hellFrenzy > 0 ? 1.4f : 1f);
        bool bossExtra;
        BalanceConfig rules;

        // Stan
        float stateTimer;
        float nextAttackTime;
        float[] cooldowns;
        EnemyAttackEntry current;
        ActionPhase attackPhase;
        /// <summary>Faza bieżącego ataku (zamach / cios / powrót) – ważna, gdy stan to Attacking.</summary>
        public ActionPhase AttackPhase => attackPhase;
        float attackT;
        float lungeSpeed;
        readonly HitTracker tracker = new HitTracker();
        GameObject marker;
        Vector3 evadeDir;
        float evadeSpeed;
        float strafeDir = 1f, strafeTimer;
        float poiseTimer;
        bool reactedToPlayerAttack;
        float verticalVelocity;

        PlayerCombat player;
        CharacterController cc;
        CharacterVisual visual;
        WeaponModel weaponModel;
        RigStyle rigStyle;
        float stateEnterTime, stateLength;
        int comboIndex;

        // Odczucie trafień: hitstop, kierunek i ciężar ostatniego ciosu (upadek), odgłos padającego ciała.
        float hitStop;
        Vector3 lastHitDir;
        float lastHitWeight;
        bool bodyFallPlayed;

        /// <summary>Styl sylwetki (np. do doboru dźwięku trafienia w pancerz).</summary>
        public RigStyle Style => rigStyle;
        public bool InHitStop => hitStop > 0f;
        public WeaponModel Weapon => weaponModel;

        public EnemyAttackEntry CurrentAttack => CurrentState == State.Attacking ? current : null;
        public bool IsTelegraphing => CurrentState == State.Attacking && attackPhase == ActionPhase.Startup;

        float SpeedMult => PhaseIndex > 0 && PhaseIndex - 1 < Def.phases.Count ? Def.phases[PhaseIndex - 1].speedMultiplier : 1f;
        float GlobalMoveMult => rules != null ? rules.enemyMoveSpeedMultiplier : 1f;
        float AttackSpeedMult => Mathf.Min(HellMaxTempo(), SpeedMult * (rules != null ? rules.enemyAttackSpeedMultiplier : 1f)
            * (hellFrenzy > 0 ? 1.4f : 1f) * (HellEnraged ? Def.huEnrageTempo : 1f));
        public float AttackWindupDuration => current != null ? current.attack.windup / Mathf.Max(0.1f, AttackSpeedMult) : 0;
        float PhaseAggression => PhaseIndex > 0 && PhaseIndex - 1 < Def.phases.Count ? Def.phases[PhaseIndex - 1].aggressionMultiplier : 1f;

        public void Setup(EnemyDefinition def, PlayerCombat target, float floorScale, DifficultyDefinition difficulty, bool elite, BalanceConfig balance)
        {
            Def = def;
            player = target;
            rules = balance;
            IsElite = elite;
            cc = GetComponent<CharacterController>();
            visual = GetComponent<CharacterVisual>();
            if (visual != null) visual.Init(this);
            weaponModel = AnimResolve.ForEnemyWeapon(def);
            rigStyle = AnimResolve.ForEnemy(def);

            if (def.isMinion || def.isBoss || !def.huCanBeElite) IsElite = elite = false;
            float hpMult = floorScale * (difficulty != null ? difficulty.enemyHealthMultiplier : 1f) * (elite ? def.eliteHealthMultiplier : 1f);
            damageMult = floorScale * (difficulty != null ? difficulty.enemyDamageMultiplier : 1f) * (elite ? def.eliteDamageMultiplier : 1f);
            aggressionMult = (difficulty != null ? difficulty.enemyAggressionMultiplier : 1f) * (elite ? 1.25f : 1f);
            bossExtra = def.isBoss && difficulty != null && difficulty.bossExtraBehavior;

            Health = new ResourcePool(def.maxHealth * hpMult);
            Poise = new ResourcePool(def.maxPoise * (elite ? 1.5f : 1f));
            Stamina = new ResourcePool(def.maxStamina);
            cooldowns = new float[def.attacks.Count];
            nextAttackTime = Time.time + 1.2f;
            Status.Clear();
            if (def.element != Element.None && FxLibrary.Enabled)
                FxLibrary.Aura(transform, Vector3.up * 1.0f * def.scale, Names.ElementColor(def.element), 70f, 0.7f * def.scale, 0.2f);
            Status.ControlMultiplier = def.isBoss ? balance.bossControlMultiplier : elite ? balance.eliteControlMultiplier : 1f;
            Status.BleedMultiplier = def.bleedMultiplier;
            stateTimer = 1.0f;
            CurrentState = State.Dormant;

            All.Add(this);
        }

        void OnDestroy()
        {
            ClearHell();
            All.Remove(this);
            ReleaseLatch(false);
            ClearMarker();
            ClearSpecialMarkers();
            statusFx.Clear();
        }

        // ------------------------------------------------------------------ Pętla

        void Update() => Step(Time.deltaTime);

        public void Step(float dt)
        {
            if (Def == null) return;
            if (player == null) { UpdateVisuals(); return; } // bez celu (np. podgląd) – tylko wygląd
            if (dt <= 0) return;

            if (CurrentState == State.Dead)
            {
                if (hitStop > 0f) { hitStop -= dt; stateEnterTime += dt; return; }
                // Ciało uderza o ziemię (koniec animacji upadku): głuchy łomot i kurz.
                if (!bodyFallPlayed && Time.time - stateEnterTime > 0.72f)
                {
                    bodyFallPlayed = true;
                    bool armored = rigStyle == RigStyle.Knight || rigStyle == RigStyle.Warden || rigStyle == RigStyle.Castellan;
                    Sfx.Play(SoundId.BodyFall, transform.position, armored ? 0.7f : 1f, Def.scale > 1.3f ? 0.8f : 1f);
                    FxLibrary.Dust(transform.position + lastHitDir * 0.6f * Def.scale, Def.scale * 1.2f);
                }
                // Najpierw animacja upadku, potem ciało powoli zapada się pod podłogę.
                if (Time.time - stateEnterTime > FallTime) transform.position += Vector3.down * 0.5f * dt;
                return;
            }

            // Hitstop: wszystko stoi (także liczniki stanu i czas animacji), odrzut czeka na koniec zatrzymania.
            if (hitStop > 0f)
            {
                hitStop -= dt;
                stateEnterTime += dt;
                nextAttackTime += dt;
                UpdateVisuals();
                return;
            }

            // Efekty trwające: tyknięcia obrażeń, a zamrożony wróg stoi (bieżący stan czeka).
            float statusDamage = Status.Tick(dt, cc.velocity.sqrMagnitude > 0.25f, rules);
            if (statusDamage > 0f) { TakeStatusDamage(statusDamage); if (IsDead) return; }
            statusFx.Update(Status, transform, Def.scale, dt);
            if (Status.Incapacitated) { ApplyMove(Vector3.zero, dt); UpdateVisuals(); return; }

            // Regeneracja postawy i wytrzymałości
            if (poiseTimer > 0) poiseTimer -= dt; else Poise.Restore(Def.poiseRegen * dt);
            if (CurrentState != State.Guarding) Stamina.Tick(dt, 25f);
            for (int i = 0; i < cooldowns.Length; i++) cooldowns[i] -= dt;

            if (TrainingDummy) { TickDummy(dt); return; }
            CheckPhase();
            TickHell(dt);
            if (IsDead) return;

            Vector3 move = Vector3.zero;
            float dist = FlatDistance(player.transform.position);
            if (player.IsDead && CurrentState == State.Moving) { ApplyMove(Vector3.zero, dt); return; }

            switch (CurrentState)
            {
                case State.Dormant:
                    stateTimer -= dt;
                    FacePlayer(Def.turnSpeed, dt);
                    if (stateTimer <= 0) CurrentState = State.Moving;
                    break;

                case State.Moving:
                    move = ThinkMoving(dist, dt);
                    break;

                case State.Attacking:
                    move = TickAttack(dt);
                    break;

                case State.Guarding:
                    FacePlayer(Def.turnSpeed, dt);
                    stateTimer -= dt;
                    if (stateTimer <= 0) EnterMoving();
                    break;

                case State.Evading:
                    move = evadeDir * evadeSpeed * Status.MoveMultiplier(rules);
                    stateTimer -= dt;
                    if (stateTimer <= 0) EnterMoving();
                    break;

                case State.Latched:
                    TickLatch(dt);
                    UpdateVisuals();
                    return;

                case State.Staggered:
                case State.Recoil:
                case State.PoiseBroken:
                    stateTimer -= dt;
                    if (stateTimer <= 0)
                    {
                        if (CurrentState == State.PoiseBroken) Poise.SetCurrent(Poise.Max);
                        EnterMoving();
                    }
                    break;
            }

            if (displacementTime > 0) { move += displacement; displacementTime -= dt; }
            ApplyMove(move, dt);
            UpdateVisuals();
        }

        /// <summary>Manekin poligonu umiejętności: nie myśli i nie atakuje, przyjmuje trafienia, statusy i odrzut.</summary>
        public bool TrainingDummy;
        /// <summary>Miejsce manekina – po odrzucie wraca tu, gdy stoi na nogach.</summary>
        public Vector3 DummySpot;

        void TickDummy(float dt)
        {
            if (CurrentState == State.Staggered || CurrentState == State.Recoil || CurrentState == State.PoiseBroken)
            {
                stateTimer -= dt;
                if (stateTimer <= 0) { if (CurrentState == State.PoiseBroken) Poise.SetCurrent(Poise.Max); EnterMoving(); }
            }
            else if (CurrentState != State.Moving) EnterMoving();
            Vector3 move = Vector3.zero;
            if (CurrentState == State.Moving)
            {
                FacePlayer(Def.turnSpeed * 0.5f, dt);
                Vector3 back = DummySpot - transform.position; back.y = 0;
                float off = back.magnitude;
                if (off > 0.3f) move = back / off * Mathf.Min(off * 4f, 3f);
            }
            if (displacementTime > 0) { move += displacement; displacementTime -= dt; }
            ApplyMove(move, dt);
            UpdateVisuals();
        }

        void ApplyMove(Vector3 horizontal, float dt)
        {
            if (!cc.enabled) return;
            if (cc.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
            verticalVelocity += Physics.gravity.y * dt;
            cc.Move((horizontal + Vector3.up * verticalVelocity) * dt);
        }

        void CheckPhase()
        {
            if (PhaseIndex >= Def.phases.Count) return;
            float threshold = Def.phases[PhaseIndex].healthThreshold + (bossExtra ? 0.1f : 0f);
            if (Health.Fraction <= threshold)
            {
                PhaseIndex++;
                CombatEvents.RaiseMessage(Def.phases[PhaseIndex - 1].announcement, new Color(1f, 0.3f, 0.3f));
                Sfx.Play(Def.isBoss ? SoundId.BossRoar : SoundId.GhoulGrowl, AimPoint);
                FxLibrary.Shockwave(transform.position, new Color(1f, 0.2f, 0.15f), 6f);
                FxLibrary.Flash(AimPoint, new Color(1f, 0.3f, 0.15f), 2.5f);
                Poise.SetCurrent(Poise.Max);
                // Wejście w fazę może od razu otworzyć atak (np. Władca Bramy przyzywa nietoperze).
                int open = Def.phases[PhaseIndex - 1].openingAttack;
                if (open >= 0 && open < Def.attacks.Count && CurrentState != State.Latched)
                {
                    if (CurrentState == State.Attacking) EndSpecialActive();
                    StartAttack(open);
                }
            }
        }

        Vector3 ThinkMoving(float dist, float dt)
        {
            FacePlayer(Def.turnSpeed, dt);
            ReactToPlayerAttack(dist);
            if (CurrentState != State.Moving) return Vector3.zero;

            if (Time.time >= nextAttackTime)
            {
                int idx = ChooseAttack(dist);
                if (idx >= 0) { StartAttack(idx); return Vector3.zero; }
            }

            Vector3 to = player.transform.position - transform.position; to.y = 0;
            Vector3 dir = to.sqrMagnitude > 0.001f ? to.normalized : transform.forward;
            float speed = Def.moveSpeed * SpeedMult * GlobalMoveMult * Status.MoveMultiplier(rules) * (hellFrenzy > 0 ? 1.4f : 1f);

            if (HellSummonerSpent) return dist > 1.2f ? dir * speed : Vector3.zero;
            if (Def.retreatRange > 0 && dist < Def.retreatRange) return -dir * speed * 0.9f;
            if (dist > Def.preferredRange) return dir * speed;

            strafeTimer -= dt;
            if (strafeTimer <= 0)
            {
                strafeTimer = Random.Range(1.2f, 2.6f);
                strafeDir = Random.value < Def.strafeChance ? (Random.value < 0.5f ? -1f : 1f) : 0f;
            }
            return Vector3.Cross(Vector3.up, dir) * strafeDir * speed * 0.45f;
        }

        void ReactToPlayerAttack(float dist)
        {
            var pa = player.Actions;
            bool playerWinding = (pa.Current == ActionType.LightAttack || pa.Current == ActionType.HeavyAttack) && pa.Phase == ActionPhase.Startup;
            if (!playerWinding) { reactedToPlayerAttack = false; return; }
            if (reactedToPlayerAttack || dist > 4f) return;
            reactedToPlayerAttack = true;

            if (Def.guardChance > 0 && Stamina.Current > 0 && Random.value < Def.guardChance)
            {
                CurrentState = State.Guarding;
                stateTimer = 0.9f;
            }
            else if (Def.evadeChance > 0 && Random.value < Def.evadeChance)
            {
                StartEvade(-(player.transform.position - transform.position), 3.2f, 0.35f);
            }
        }

        void StartEvade(Vector3 dir, float distance, float time)
        {
            dir.y = 0;
            evadeDir = dir.sqrMagnitude > 0.001f ? dir.normalized : -transform.forward;
            evadeSpeed = distance / time;
            FxLibrary.Dust(transform.position, Def.scale);
            stateTimer = time;
            MarkState(time);
            CurrentState = State.Evading;
        }

        int ChooseAttack(float dist)
        {
            float total = 0;
            // Ataki z wagą 0 są dostępne wyłącznie jako kontynuacja kombinacji (followUp).
            for (int i = 0; i < Def.attacks.Count; i++) if (Def.attacks[i].weight > 0 && Usable(i, dist)) total += Def.attacks[i].weight;
            if (total <= 0) return -1;
            float roll = Random.value * total;
            for (int i = 0; i < Def.attacks.Count; i++)
            {
                if (Def.attacks[i].weight <= 0 || !Usable(i, dist)) continue;
                roll -= Def.attacks[i].weight;
                if (roll <= 0) return i;
            }
            return -1;
        }

        bool Usable(int i, float dist)
        {
            var e = Def.attacks[i];
            if (!HellUsable(e)) return false;
            if (cooldowns[i] > 0) return false;
            if (e.minPhase > PhaseIndex) return false;
            if (e.eliteOnly && !IsElite) return false;
            if (e.bossExtraOnly && !bossExtra) return false;
            return dist >= e.minRange && dist <= e.maxRange;
        }

        void StartAttack(int idx, bool chained = false)
        {
            comboIndex = chained ? comboIndex + 1 : 0;
            MarkState(0f);
            current = Def.attacks[idx];
            cooldowns[idx] = current.cooldown * (hellFrenzy > 0 ? 0.7f : 1f);
            attackT = 0;
            attackPhase = ActionPhase.Startup;
            tracker.Reset();
            var a = current.attack;
            lungeSpeed = a.lunge / Mathf.Max(0.05f, (a.windup + a.active));
            CurrentState = State.Attacking;
            ClearMarker();
            BeginSpecial(current);
            if (a.delivery == AttackDelivery.AreaAroundSelf)
            {
                var c = TelegraphColors.For(a.Telegraph); c.a = 1f;
                marker = VisualFx.GroundMarker(transform.position, a.radius, c);
            }
            // Zapowiedź dźwiękiem: cios, którego nie da się zablokować – dysonansowy dzwon; ciężki – niski gong.
            var tg = a.Telegraph;
            if (tg == TelegraphKind.Unblockable || tg == TelegraphKind.NoDodge) Sfx.Play(SoundId.TelegraphUnblockable, AimPoint);
            else if (tg != TelegraphKind.Normal) Sfx.Play(SoundId.TelegraphHeavy, AimPoint);
            else if (rigStyle == RigStyle.Ghoul && Random.value < 0.35f) Sfx.Play(SoundId.GhoulGrowl, AimPoint, 0.8f);
            if (a.delivery == AttackDelivery.Projectile && weaponModel != WeaponModel.Bow) Sfx.Play(SoundId.CastCharge, AimPoint, 0.7f);
            if (a.delivery == AttackDelivery.Projectile && visual != null && weaponModel != WeaponModel.Bow)
                FxLibrary.Gather(visual.RightHand != null ? visual.RightHand : transform, TelegraphColors.For(a.Telegraph), a.windup / Mathf.Max(0.1f, AttackSpeedMult));
        }

        Vector3 TickAttack(float dt)
        {
            var a = current.attack;
            float speed = AttackSpeedMult * Status.ActionSpeedMultiplier(rules); // chłód spowalnia także zamach
            attackT += dt * speed;
            Vector3 move = Vector3.zero;

            switch (attackPhase)
            {
                case ActionPhase.Startup:
                    if (attackT < a.windup * 0.75f) FacePlayer(a.tracking, dt);
                    move = transform.forward * lungeSpeed * speed * LungeGate();
                    move += SpecialWindup(a, speed);
                    if (marker != null)
                    {
                        marker.transform.position = transform.position + Vector3.up * 0.03f;
                        var decal = marker.GetComponent<FxDecal>();
                        if (decal != null) decal.Progress = Mathf.Clamp01(attackT / Mathf.Max(0.01f, a.windup));
                    }
                    if (attackT >= a.windup)
                    {
                        attackPhase = ActionPhase.Active;
                        OnAttackActive();
                    }
                    break;
                case ActionPhase.Active:
                    move = transform.forward * lungeSpeed * speed * LungeGate();
                    move += SpecialActive(a, speed, dt);
                    if (current == null || CurrentState != State.Attacking) return Vector3.zero; // sparowana szarża przerwała atak
                    if (a.delivery == AttackDelivery.Melee && (current.special == EnemySpecial.None || current.special == EnemySpecial.Latch)) SweepMelee();
                    if (CurrentState != State.Attacking) return Vector3.zero; // uczepienie przejęło stan
                    if (attackT >= a.windup + a.active) { attackPhase = ActionPhase.Recovery; EndSpecialActive(); }
                    break;
                case ActionPhase.Recovery:
                    if (attackT >= a.windup + a.active + a.recovery) FinishAttack();
                    break;
            }
            return move;
        }

        /// <summary>Wypad zatrzymuje się tuż przed graczem, by nie przepychać go.</summary>
        float LungeGate() => FlatDistance(player.transform.position) > 1.2f * Def.scale ? 1f : 0f;

        void OnAttackActive()
        {
            var a = current.attack;
            float dmg = a.baseDamage * DamageMult;
            if (SpecialStrike(a, dmg)) return;
            switch (a.delivery)
            {
                case AttackDelivery.Melee:
                    Sfx.Play(a.heavy || Def.scale > 1.3f ? SoundId.SwingHeavy : SoundId.SwingLight, AimPoint, 1f, Def.scale > 1.3f ? 0.85f : 1f);
                    break;
                case AttackDelivery.Projectile:
                {
                    Vector3 origin = transform.position + Vector3.up * 1.3f * Def.scale + transform.forward * (0.8f * Def.scale);
                    Vector3 aim = player.AimPoint - origin;
                    Sfx.Play(weaponModel == WeaponModel.Bow ? SoundId.BowRelease : Sfx.ForElement(a.element, SoundId.ArcaneBolt), origin);
                    int count = Mathf.Max(1, a.projectileCount);
                    for (int i = 0; i < count; i++)
                    {
                        float ang = count == 1 ? 0 : Mathf.Lerp(-a.spreadAngle, a.spreadAngle, i / (float)(count - 1));
                        Vector3 dir = Quaternion.Euler(0, ang, 0) * aim.normalized;
                        var hit = BasicHit(a, dmg, origin);
                        if (weaponModel == WeaponModel.Bow)
                            Projectile.Spawn(origin, dir, a.projectileSpeed, hit, Faction.Enemy, this, new Color(0.85f, 0.8f, 0.7f), 0.12f, 3f, arrow: true);
                        else
                            Projectile.Spawn(origin, dir, a.projectileSpeed, hit, Faction.Enemy, this, TelegraphColors.For(a.Telegraph) * 0.8f + new Color(0.2f, 0.1f, 0.4f), 0.3f);
                    }
                    break;
                }
                case AttackDelivery.AreaAroundSelf:
                {
                    ClearMarker();
                    var hit = BasicHit(a, dmg, transform.position);
                    HitQuery.Sphere(transform.position + Vector3.up, a.radius, Faction.Enemy, tracker, r => HitQuery.Apply(this, r, hit, false));
                    VisualFx.Ring(transform.position, a.radius, TelegraphColors.For(a.Telegraph), 0.4f);
                    Sfx.Play(a.damageType == DamageType.Magic ? SoundId.Shockwave : SoundId.Quake, transform.position);
                    break;
                }
            }
        }

        void SweepMelee()
        {
            var a = current.attack;
            float s = Def.scale;
            Vector3 from = transform.position + Vector3.up * 1.0f * s + transform.forward * 0.3f * s;
            Vector3 to = transform.position + Vector3.up * 1.0f * s + transform.forward * a.reach;
            var entry = current;
            HitQuery.Capsule(from, to, a.radius, Faction.Enemy, tracker, r =>
            {
                var hit = BasicHit(a, a.baseDamage * DamageMult, transform.position);
                hit.weapon = weaponModel == WeaponModel.Auto ? WeaponModel.None : weaponModel;
                var res = HitQuery.Apply(this, r, hit, true);
                // Wgryzienie: skok, który doszedł (nie zablokowany, nie uniknięty), kończy się uczepieniem.
                if (entry.special == EnemySpecial.Latch && res.outcome == HitOutcome.Hit && ReferenceEquals(r, player) && !IsDead)
                    BeginLatch(entry);
            });
        }

        void FinishAttack()
        {
            InterruptHell();
            var finished = current;
            int f = finished.followUp;
            // Krok serii dostępny dopiero w późniejszej fazie (np. dodatkowe cięcie w szale) jest pomijany – seria idzie dalej.
            for (int guard = 0; f >= 0 && f < Def.attacks.Count && (Def.attacks[f].minPhase > PhaseIndex || (Def.attacks[f].huEnragedOnly && !HellEnraged)) && guard < 8; guard++)
                f = Def.attacks[f].followUp;
            if (f >= 0 && f < Def.attacks.Count && !player.IsDead && Random.value < finished.followUpChance
                && Usable(f, Mathf.Min(FlatDistance(player.transform.position), Def.attacks[f].maxRange)))
            {
                StartAttack(f, true);
                return;
            }
            if (finished.retreatAfter > 0)
            {
                StartEvade(-(player.transform.position - transform.position), finished.retreatAfter, 0.45f);
                ScheduleNextAttack();
                return;
            }
            EnterMoving();
        }

        void EnterMoving()
        {
            if (CurrentState == State.Attacking || CurrentState == State.Staggered || CurrentState == State.Recoil || CurrentState == State.PoiseBroken)
                ScheduleNextAttack();
            CurrentState = State.Moving;
            current = null;
            ClearMarker();
        }

        void ScheduleNextAttack()
        {
            float agg = Mathf.Max(0.1f, aggressionMult * PhaseAggression);
            nextAttackTime = Time.time + Random.Range(Def.attackInterval.x, Def.attackInterval.y) / agg * (hellFrenzy > 0 ? 0.7f : 1f);
        }

        void ClearMarker()
        {
            if (marker != null) Destroy(marker);
            marker = null;
        }

        void FacePlayer(float degPerSec, float dt)
        {
            Vector3 to = player.transform.position - transform.position; to.y = 0;
            if (to.sqrMagnitude < 0.001f) return;
            degPerSec *= TurnMultiplier(to);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(to), degPerSec * dt);
        }

        /// <summary>
        /// Wróg „gubi” gracza, który przetacza się obok (w czasie uniku i chwilę po nim), i wolniej odwraca się do tego,
        /// kto zaszedł go od tyłu – dzięki temu unik na skos do przodu daje szansę na cios w plecy.
        /// </summary>
        public float TurnMultiplier(Vector3 toPlayer)
        {
            if (rules == null || player == null) return 1f;
            float m = 1f;
            if (player.TimeSinceDodge < rules.enemyTurnDodgeGrace) m *= rules.enemyTurnDuringDodge;
            if (Vector3.Angle(transform.forward, toPlayer) > 100f) m *= rules.enemyTurnBehind;
            return m;
        }

        float FlatDistance(Vector3 p)
        {
            Vector3 d = p - transform.position; d.y = 0;
            return d.magnitude;
        }

        // ------------------------------------------------------------------ Trafienia

        /// <summary>Hitstop lokalny: wróg (logika i animacja) zatrzymuje się na chwilę.</summary>
        public void HitStop(float seconds)
        {
            // Działa też na cios kończący: ciało zastyga na chwilę, zanim zacznie padać.
            hitStop = Mathf.Max(hitStop, seconds);
            if (visual != null) visual.Hold(seconds);
        }

        /// <summary>Odrzut od ciosu (po hitstopie). Bossy i elity słabiej, pancerz w zamachu – prawie wcale.</summary>
        public void Knockback(Vector3 dir, float distance, float time = 0.18f)
        {
            if (IsDead || distance <= 0f) return;
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) return;
            float m = Def.isBoss ? 0.2f : IsElite ? 0.5f : 1f;
            if (CurrentState == State.Attacking && current != null && current.hyperArmor) m *= 0.3f;
            displacement = dir.normalized * (distance * m / time);
            displacementTime = time;
        }

        void RecordHit(Vector3 source, float weight)
        {
            Vector3 d = transform.position - source; d.y = 0f;
            lastHitDir = d.sqrMagnitude > 1e-4f ? d.normalized : -transform.forward;
            lastHitWeight = weight;
        }

        public DefenseState CurrentDefense() => new DefenseState
        {
            position = transform.position,
            forward = transform.forward,
            guarding = CurrentState == State.Guarding || (Def.huArchetype == 9 && CurrentState == State.Moving && Stamina.Current > 0),
            guard = Def.guard,
            stamina = Stamina.Current,
            physicalResist = hellStoneTime > 0 || (Def.huArchetype == 11 && CurrentState == State.Dormant) ? 0.9f : Def.physicalResist,
            magicResist = Def.magicResist,
            dead = IsDead,
        };

        public HitResult ReceiveHit(HitData hit)
        {
            // Cechy klasy gracza zależne od celu (zasadzka z tyłu, sokole oko, żądza krwi, egzekucja).
            if (hit.attacker is PlayerCombat pc && !hit.isStatusTick) pc.OutgoingAgainst(ref hit, this);
            // Słabość/odporność dotyczy części z żywiołem; porażenie i szok termiczny – przed pancerzem.
            if (hit.element != Element.None && hit.magic > 0)
            {
                float m = Def.ElementMultiplier(hit.element);
                hit.magic *= m;
                if (m > 1.01f && !IsDead) CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.4f, "SŁABOŚĆ", Names.ElementColor(hit.element));
            }
            // Egzekucja: premia przeciw osłabionym (poniżej 30% życia) lub z przełamaną postawą.
            if (hit.executeBonus > 0 && (Health.Fraction < 0.3f || CurrentState == State.PoiseBroken))
            {
                hit.physical *= 1f + hit.executeBonus; hit.magic *= 1f + hit.executeBonus;
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.4f, "EGZEKUCJA", new Color(1f, 0.3f, 0.2f));
            }
            string reaction = Status.ModifyIncoming(ref hit, rules);

            var r = DamageResolver.Resolve(hit, CurrentDefense(), rules);
            if (r.outcome == HitOutcome.Ignored) return r;
            r.healthDamage = AbsorbHellWard(r.healthDamage);
            if (!hit.isStatusTick && !hit.isReaction && r.healthDamage > 0) hellDirectHits++;
            if (!hit.isStatusTick) RecordHit(hit.sourcePosition, ImpactDirector.Weight(hit)); // ten sam ciężar co odczucie ciosu
            float instant = Status.OnLanded(hit, r, rules, out var landedReaction, out bool froze, out float shatter);
            if (shatter > 0) Shatter(shatter, hit);
            if (hit.mods.conductionJump && Status.LastConductionDamage > 0) JumpConduction(Status.LastConductionDamage * 0.5f, hit);
            r.healthDamage += instant;
            reaction = reaction ?? landedReaction;
            if (reaction != null) CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.7f, reaction, Names.ElementColor(hit.element));
            if (reaction != null) Sfx.Play(Sfx.ImpactFor(hit.element), AimPoint, 0.8f);
            else if (hit.fromMeleeWeapon && hit.element != Element.None && hit.magic > 0 && r.outcome == HitOutcome.Hit) Sfx.Play(Sfx.ImpactFor(hit.element), AimPoint, 0.45f);
            Health.Drain(r.healthDamage);
            if (r.staminaDamage > 0) Stamina.Drain(r.staminaDamage, 1f);
            if (froze && Health.Current > 0)
            {
                // Zamrożenie przerywa atak (także z pancerzem) – krócej na bossach i elitach.
                // Na czas zamrożenia pętla stoi, więc po odmrożeniu zostaje tylko krótkie drgnięcie.
                Interrupt(State.Staggered, 0.2f);
                AnnounceControl();
            }

            if (Health.Current <= 0)
            {
                CombatEvents.RaiseHit(AimPoint, r, false);
                Die();
                return r;
            }

            if (CurrentState == State.Dormant) CurrentState = State.Moving;

            // Odrzut (albo przyciągnięcie) z umiejętności: od źródła trafienia.
            if (hit.knockback != 0f && (r.outcome == HitOutcome.Hit || r.outcome == HitOutcome.GuardBroken))
            {
                Vector3 away = transform.position - hit.sourcePosition; away.y = 0f;
                if (away.sqrMagnitude < 1e-4f) away = -transform.forward;
                away.Normalize();
                if (hit.knockback > 0f) Pull(transform.position + away * (hit.knockback + 1f), hit.knockback, 0.2f);
                else Pull(hit.sourcePosition, -hit.knockback, 0.25f);
            }

            switch (r.outcome)
            {
                case HitOutcome.Hit:
                    Poise.Drain(r.poiseDamage);
                    poiseTimer = Def.poiseRegenDelay;
                    if (Poise.Current <= 0)
                    {
                        bool armored = CurrentState == State.Attacking && current != null && current.hyperArmor;
                        if (!armored && CurrentState != State.PoiseBroken && CurrentState != State.Latched)
                        {
                            // Przełamana postawa – zachwianie widać jak po ciężkim ciosie, nawet gdy przelał czarę lekki.
                            lastHitWeight = Mathf.Max(lastHitWeight, 0.8f);
                            Interrupt(State.Staggered, 0.75f);
                            Poise.SetCurrent(Poise.Max);
                        }
                    }
                    break;
                case HitOutcome.GuardBroken:
                    lastHitWeight = Mathf.Max(lastHitWeight, 0.9f);
                    Interrupt(State.Staggered, 1.2f);
                    CombatEvents.RaiseMessage("Garda wroga przełamana!", new Color(1f, 0.8f, 0.3f));
                    break;
                case HitOutcome.Blocked:
                    break;
            }
            CombatEvents.RaiseHit(AimPoint, r, false);
            return r;
        }

        void Interrupt(State s, float time)
        {
            InterruptHell();
            ClearMarker();
            if (CurrentState == State.Attacking) EndSpecialActive();
            ReleaseLatch(false);
            current = null;
            CurrentState = s;
            stateTimer = time;
            MarkState(time);
        }

        public void OnAttackParried(float poiseDamage)
        {
            if (IsDead) return;
            Poise.Drain(poiseDamage);
            poiseTimer = Def.poiseRegenDelay;
            if (Poise.Current <= 0)
            {
                Interrupt(State.PoiseBroken, Def.riposteWindow);
                CombatEvents.RaiseMessage("Postawa przełamana – RIPOSTA!", new Color(1f, 0.9f, 0.4f));
            }
            else
            {
                // Odbity cios odrzuca napastnika do tyłu (od broni obrońcy).
                RecordHit(transform.position + transform.forward, 0.6f);
                Interrupt(State.Recoil, 0.7f);
            }
        }

        public void OnHitResolved(IHitReceiver target, in HitData hit, in HitResult result) { }

        public void ReceiveRiposte(float damage, IHitReceiver attacker)
        {
            if (!CanBeRiposted) return;
            if (attacker != null) RecordHit(attacker.Transform.position, 1f);
            float dmg = damage * (1f - Def.physicalResist * 0.5f);
            Health.Drain(dmg);
            CombatEvents.RaiseHit(AimPoint, new HitResult { outcome = HitOutcome.Hit, healthDamage = dmg }, false);
            if (Health.Current <= 0) { Die(); return; }
            Interrupt(State.Staggered, 1.3f);
            Poise.SetCurrent(Poise.Max);
        }

        /// <summary>Roztrzaskanie: lód pęka, wybuch mrozu rani innych wrogów wokół (bez kolejnych reakcji).</summary>
        void Shatter(float damage, in HitData source)
        {
            var splash = new HitData
            {
                magic = damage, poiseDamage = 20, guardLoad = 20, blockable = true, parryable = false, dodgeable = true,
                sourcePosition = transform.position, attacker = source.attacker, element = Element.Frost, isReaction = true,
                attackName = "Roztrzaskanie", mods = StatusModifiers.None,
            };
            var tr = new HitTracker();
            tr.TryRegister(this);
            var attacker = source.attacker as IHitReceiver;
            // Wybuch rani stronę przeciwną atakującemu (dla gracza – innych wrogów).
            HitQuery.Sphere(transform.position + Vector3.up, rules.shatterRadius, attacker != null ? attacker.Faction : Faction.Player, tr, r => HitQuery.Apply(attacker, r, splash, false));
            FxLibrary.Shockwave(transform.position, Names.StatusColor(StatusKind.Frozen), rules.shatterRadius, false);
            FxLibrary.Impact(AimPoint, Names.StatusColor(StatusKind.Frozen), 1.5f);
            Sfx.Play(SoundId.Shatter, AimPoint);
        }

        /// <summary>Przewodzenie przeskakuje na najbliższego innego wroga (przedmiot „Amulet przewodnika”).</summary>
        void JumpConduction(float damage, in HitData source)
        {
            EnemyBrain best = null; float bestD = 6f;
            foreach (var e in All)
            {
                if (e == this || e.IsDead) continue;
                float d = Vector3.Distance(e.transform.position, transform.position);
                if (d < bestD) { bestD = d; best = e; }
            }
            if (best == null) return;
            var jump = new HitData
            {
                magic = damage, blockable = true, parryable = false, dodgeable = true, sourcePosition = transform.position,
                attacker = source.attacker, element = Element.Lightning, isReaction = true, attackName = "Przewodzenie", mods = StatusModifiers.None,
            };
            FxLibrary.Lightning(AimPoint, best.AimPoint, Names.ElementColor(Element.Lightning));
            Sfx.Play(SoundId.LightningZap, best.AimPoint, 0.7f);
            HitQuery.Apply(source.attacker as IHitReceiver, best, jump, false);
        }

        // ------------------------------------------------------------------ Wpływy umiejętności gracza

        Vector3 displacement; float displacementTime;

        /// <summary>Nałożenie efektu poza zwykłym trafieniem (np. odwet mroźnej zbroi). Zamrożenie przerywa atak.</summary>
        public void ApplyStatus(StatusKind kind, float landedPhysical, float landedMagic)
        {
            if (IsDead) return;
            if (Status.Apply(kind, landedPhysical, landedMagic, rules) && kind != StatusKind.Bleed)
            {
                Interrupt(State.Staggered, 0.2f);
                AnnounceControl();
            }
        }

        /// <summary>Powalenie (Odtrącenie): wróg leży przez podany czas (bossów odtrącenie nie dotyczy).</summary>
        public void HuThrowDown(float seconds)
        {
            if (IsDead || Def.isBoss || seconds <= 0f) return;
            if (Status.KnockDown(seconds, rules)) { Interrupt(State.Staggered, Status.StunRemaining); AnnounceControl(); }
        }

        /// <summary>Napis i dźwięk po zamrożeniu albo ogłuszeniu.</summary>
        void AnnounceControl()
        {
            if (Status.Frozen)
            {
                Sfx.Play(SoundId.Freeze, AimPoint);
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.7f, "ZAMROŻONY", Names.StatusColor(StatusKind.Frozen));
            }
            else
            {
                Sfx.Play(SoundId.Quake, AimPoint, 0.6f, 1.4f);
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.7f, "OGŁUSZONY", Names.StatusColor(StatusKind.Stun));
            }
        }

        /// <summary>Przyciągnięcie (łańcuchy): przesunięcie w stronę punktu przez krótki czas; bossy ×0,25, elity ×0,5.</summary>
        public void Pull(Vector3 toward, float distance, float time = 0.3f)
        {
            if (IsDead) return;
            float m = Def.isBoss ? 0.25f : IsElite ? 0.5f : 1f;
            Vector3 d = toward - transform.position; d.y = 0;
            float len = Mathf.Min(d.magnitude - 0.8f, distance * m);
            if (len <= 0.05f) return;
            displacement = d.normalized * (len / time);
            displacementTime = time;
            if (CurrentState == State.Attacking && current != null && !current.hyperArmor) Interrupt(State.Staggered, time + 0.2f);
        }

        /// <summary>Rozdarcie ran: całe pozostałe krwawienie od razu (× mnożnik). Zwraca zadane obrażenia.</summary>
        public float RuptureBleed(float multiplier)
        {
            if (IsDead) return 0f;
            float dmg = Status.ConsumeBleed() * multiplier;
            if (dmg <= 0f) return 0f;
            Health.Drain(dmg);
            CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.7f, $"ROZDARCIE {dmg:0}", Names.StatusColor(StatusKind.Bleed));
            Sfx.Play(SoundId.HitFleshHeavy, AimPoint);
            FxLibrary.Blood(AimPoint, Vector3.up, 2.5f);
            if (Health.Current <= 0) Die();
            return dmg;
        }

        /// <summary>Tyknięcie efektu (krwawienie, podpalenie): już po pancerzu, bez postawy i drgnięcia.</summary>
        void TakeStatusDamage(float dmg)
        {
            Health.Drain(dmg);
            CombatEvents.RaiseWorldText(AimPoint, $"{dmg:0}", Status.BleedStacks > 0 ? Names.StatusColor(StatusKind.Bleed) : Names.StatusColor(StatusKind.Burn));
            Sfx.Play(Status.BleedStacks > 0 ? SoundId.BleedTick : SoundId.BurnTick, AimPoint);
            if (Health.Current <= 0) Die();
        }

        void Die()
        {
            if (IsDead) return;
            ClearHell();
            if (Def.huSplitCount > 0 && Def.huSummon != null)
            {
                SummonHell(Def.huSummon, Def.huSplitCount, Def.huSplitCount);
                foreach (var child in All) if (child.Owner == this && child.Def == Def.huSummon) child.Owner = Owner;
            }
            ReleaseLatch(false);
            ClearMarker();
            ClearSpecialMarkers();
            Status.Clear();
            statusFx.Clear();
            CurrentState = State.Dead;
            MarkState(0f);
            current = null;
            cc.enabled = false;
            if (visual != null) { visual.SetWeaponGlow(Color.black, 0f); visual.SetTint(Color.black, 0.45f); }
            FxLibrary.DeathAsh(transform.position, Def.scale);
            Sfx.Play(Def.isBoss ? SoundId.BossDeath : rigStyle == RigStyle.Ghoul ? SoundId.DeathGhoul : rigStyle == RigStyle.Archer ? SoundId.DeathBones : SoundId.DeathArmored, AimPoint);
            All.Remove(this);
            CombatEvents.RaiseDied(this);
            Died?.Invoke(this);
            Destroy(gameObject, FallTime + 1.4f);
            // Sługi giną razem z przyzywającym (Hell Unleashed).
            foreach (var m in All.ToArray())
                if (m != null && m.Owner == this && !m.IsDead) m.Die();
        }

        /// <summary>Czas upadku (klip śmierci modelu), zanim ciało zacznie się zapadać.</summary>
        float FallTime => visual != null ? visual.DeathFallTime : 1.6f;

        /// <summary>Tylko do testów.</summary>
        public void DebugKill() { Health.Drain(Health.Current); Die(); }

        /// <summary>Tylko do testów: natychmiast zaczyna wskazany atak (także specjalny).</summary>
        public void DebugStartAttack(int index) { if (!IsDead && index >= 0 && index < Def.attacks.Count) StartAttack(index); }

        // ------------------------------------------------------------------ Animacja i czytelność

        void MarkState(float length)
        {
            stateEnterTime = Time.time;
            stateLength = length;
        }

        public CharacterAnimState GetAnimState()
        {
            var st = new CharacterAnimState
            {
                actionTime = Status.Incapacitated ? 0.12f : Time.time - stateEnterTime, // zamrożony / ogłuszony – poza zatrzymana
                actionDuration = stateLength,
                hasShield = Def != null && Def.guardChance > 0,
                hitDirection = lastHitDir,
                hitWeight = lastHitWeight,
            };
            switch (CurrentState)
            {
                case State.Attacking when current != null:
                {
                    var a = current.attack;
                    st.action = AnimAction.Attack;
                    st.attack = AnimResolve.ForAttack(a, comboIndex, weaponModel);
                    st.phase = attackPhase;
                    st.windupSeconds = a.windup / Mathf.Max(0.1f, AttackSpeedMult);
                    switch (attackPhase)
                    {
                        case ActionPhase.Startup: st.phaseProgress = a.windup > 0 ? attackT / a.windup : 1f; break;
                        case ActionPhase.Active: st.phaseProgress = a.active > 0 ? (attackT - a.windup) / a.active : 1f; break;
                        default: st.phaseProgress = a.recovery > 0 ? (attackT - a.windup - a.active) / a.recovery : 1f; break;
                    }
                    st.phaseProgress = Mathf.Clamp01(st.phaseProgress);
                    break;
                }
                case State.Guarding: st.action = AnimAction.Block; break;
                case State.Evading: st.action = AnimAction.Backstep; break;
                case State.Staggered:
                case State.Recoil: st.action = AnimAction.Flinch; break;
                case State.PoiseBroken: st.action = AnimAction.Kneel; break;
                case State.Dead: st.action = AnimAction.Death; break;
                case State.Latched: st.action = AnimAction.Latched; break;
            }
            return st;
        }

        void UpdateVisuals()
        {
            if (visual == null) return;
            Color glow = Color.black;
            float glowAmount = 0f;
            Color tint = Color.white;
            float tintAmount = 0f;
            if (CurrentState == State.Attacking && current != null)
            {
                var a = current.attack;
                var tc = TelegraphColors.For(a.Telegraph);
                if (attackPhase == ActionPhase.Startup)
                {
                    float k = Mathf.Clamp01(attackT / Mathf.Max(0.01f, a.windup));
                    // Pulsowanie przyspiesza pod koniec zamachu – czytelny rytm do reakcji.
                    float pulse = 0.5f + 0.5f * Mathf.Sin(attackT * Mathf.Lerp(8f, 30f, k));
                    glow = tc; glowAmount = 0.35f + 0.65f * k * pulse;
                    if (a.Telegraph != TelegraphKind.Normal) { tint = tc; tintAmount = 0.12f + 0.2f * k * pulse; }
                }
                else if (attackPhase == ActionPhase.Active) { glow = tc; glowAmount = 1f; }
                visual.SetTrailColor(Color.Lerp(tc, Color.white, 0.3f));
            }
            else if (CurrentState == State.PoiseBroken) { tint = Color.yellow; tintAmount = 0.25f + 0.2f * Mathf.Sin(Time.time * 12f); }
            if ((Status.Frozen || tintAmount <= 0f) && StatusFx.Tint(Status, out var sc, out var sa)) { tint = sc; tintAmount = sa; }
            else if (CurrentState == State.Staggered || CurrentState == State.Recoil) { tint = Color.white; tintAmount = 0.35f; }
            if (IsElite && tintAmount <= 0f) { tint = new Color(0.9f, 0.7f, 0.2f); tintAmount = 0.18f; }
            if (Def.element != Element.None)
            {
                // Wariant żywiołu: stała poświata broni i lekkie zabarwienie w kolorze żywiołu.
                var ec = Names.ElementColor(Def.element);
                if (glowAmount <= 0f) { glow = ec; glowAmount = 0.6f; }
                if (tintAmount <= 0f) { tint = ec; tintAmount = 0.3f; }
            }
            visual.SetTint(tint, tintAmount);
            visual.SetWeaponGlow(glow, glowAmount);
        }
    }
}
