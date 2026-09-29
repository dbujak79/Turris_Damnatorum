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
    public class EnemyBrain : MonoBehaviour, IHitReceiver, IRiposteTarget, ICharacterAnimSource
    {
        public static readonly List<EnemyBrain> All = new List<EnemyBrain>();

        public enum State { Dormant, Moving, Attacking, Guarding, Evading, Staggered, Recoil, PoiseBroken, Dead }

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

        /// <summary>Efekty trwające (krwawienie, podpalenie, chłód, porażenie, zamrożenie).</summary>
        public readonly StatusEffects Status = new StatusEffects();
        readonly StatusFx statusFx = new StatusFx();

        // Skalowanie
        float damageMult = 1f, aggressionMult = 1f;
        bool bossExtra;
        BalanceConfig rules;

        // Stan
        float stateTimer;
        float nextAttackTime;
        float[] cooldowns;
        EnemyAttackEntry current;
        ActionPhase attackPhase;
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
        float stateEnterTime, stateLength;
        int comboIndex;

        public EnemyAttackEntry CurrentAttack => CurrentState == State.Attacking ? current : null;
        public bool IsTelegraphing => CurrentState == State.Attacking && attackPhase == ActionPhase.Startup;

        float SpeedMult => PhaseIndex > 0 && PhaseIndex - 1 < Def.phases.Count ? Def.phases[PhaseIndex - 1].speedMultiplier : 1f;
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

            float hpMult = floorScale * (difficulty != null ? difficulty.enemyHealthMultiplier : 1f) * (elite ? 1.3f : 1f);
            damageMult = floorScale * (difficulty != null ? difficulty.enemyDamageMultiplier : 1f) * (elite ? 1.1f : 1f);
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
            All.Remove(this);
            ClearMarker();
            statusFx.Clear();
        }

        // ------------------------------------------------------------------ Pętla

        void Update()
        {
            if (Def == null) return;
            if (player == null) { UpdateVisuals(); return; } // bez celu (np. podgląd) – tylko wygląd
            float dt = Time.deltaTime;
            if (dt <= 0) return;

            if (CurrentState == State.Dead)
            {
                // Najpierw animacja upadku, potem ciało powoli zapada się pod podłogę.
                if (Time.time - stateEnterTime > 1.6f) transform.position += Vector3.down * 0.5f * dt;
                return;
            }

            // Efekty trwające: tyknięcia obrażeń, a zamrożony wróg stoi (bieżący stan czeka).
            float statusDamage = Status.Tick(dt, cc.velocity.sqrMagnitude > 0.25f, rules);
            if (statusDamage > 0f) { TakeStatusDamage(statusDamage); if (IsDead) return; }
            statusFx.Update(Status, transform, Def.scale, dt);
            if (Status.Frozen) { ApplyMove(Vector3.zero, dt); UpdateVisuals(); return; }

            // Regeneracja postawy i wytrzymałości
            if (poiseTimer > 0) poiseTimer -= dt; else Poise.Restore(Def.poiseRegen * dt);
            if (CurrentState != State.Guarding) Stamina.Tick(dt, 25f);
            for (int i = 0; i < cooldowns.Length; i++) cooldowns[i] -= dt;

            CheckPhase();

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

        void ApplyMove(Vector3 horizontal, float dt)
        {
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
                FxLibrary.Shockwave(transform.position, new Color(1f, 0.2f, 0.15f), 6f);
                FxLibrary.Flash(AimPoint, new Color(1f, 0.3f, 0.15f), 2.5f);
                Poise.SetCurrent(Poise.Max);
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
            float speed = Def.moveSpeed * SpeedMult * Status.MoveMultiplier(rules);

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
            cooldowns[idx] = current.cooldown;
            attackT = 0;
            attackPhase = ActionPhase.Startup;
            tracker.Reset();
            var a = current.attack;
            lungeSpeed = a.lunge / Mathf.Max(0.05f, (a.windup + a.active));
            CurrentState = State.Attacking;
            ClearMarker();
            if (a.delivery == AttackDelivery.AreaAroundSelf)
            {
                var c = TelegraphColors.For(a.Telegraph); c.a = 1f;
                marker = VisualFx.GroundMarker(transform.position, a.radius, c);
            }
            if (a.delivery == AttackDelivery.Projectile && visual != null)
                FxLibrary.Gather(visual.RightHand != null ? visual.RightHand : transform, TelegraphColors.For(a.Telegraph), a.windup / Mathf.Max(0.1f, SpeedMult));
        }

        Vector3 TickAttack(float dt)
        {
            var a = current.attack;
            float speed = SpeedMult * Status.ActionSpeedMultiplier(rules); // chłód spowalnia także zamach
            attackT += dt * speed;
            Vector3 move = Vector3.zero;

            switch (attackPhase)
            {
                case ActionPhase.Startup:
                    if (attackT < a.windup * 0.75f) FacePlayer(a.tracking, dt);
                    move = transform.forward * lungeSpeed * speed * LungeGate();
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
                    if (a.delivery == AttackDelivery.Melee) SweepMelee();
                    if (attackT >= a.windup + a.active) attackPhase = ActionPhase.Recovery;
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
            float dmg = a.baseDamage * damageMult;
            switch (a.delivery)
            {
                case AttackDelivery.Projectile:
                {
                    Vector3 origin = transform.position + Vector3.up * 1.3f * Def.scale + transform.forward * (0.8f * Def.scale);
                    Vector3 aim = player.AimPoint - origin;
                    int count = Mathf.Max(1, a.projectileCount);
                    for (int i = 0; i < count; i++)
                    {
                        float ang = count == 1 ? 0 : Mathf.Lerp(-a.spreadAngle, a.spreadAngle, i / (float)(count - 1));
                        Vector3 dir = Quaternion.Euler(0, ang, 0) * aim.normalized;
                        var hit = HitData.FromAttack(a, dmg, origin, this);
                        Projectile.Spawn(origin, dir, a.projectileSpeed, hit, Faction.Enemy, this, TelegraphColors.For(a.Telegraph) * 0.8f + new Color(0.2f, 0.1f, 0.4f), 0.3f);
                    }
                    break;
                }
                case AttackDelivery.AreaAroundSelf:
                {
                    ClearMarker();
                    var hit = HitData.FromAttack(a, dmg, transform.position, this);
                    HitQuery.Sphere(transform.position + Vector3.up, a.radius, Faction.Enemy, tracker, r => HitQuery.Apply(this, r, hit, false));
                    VisualFx.Ring(transform.position, a.radius, TelegraphColors.For(a.Telegraph), 0.4f);
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
            HitQuery.Capsule(from, to, a.radius, Faction.Enemy, tracker, r =>
            {
                var hit = HitData.FromAttack(a, a.baseDamage * damageMult, transform.position, this);
                HitQuery.Apply(this, r, hit, true);
            });
        }

        void FinishAttack()
        {
            var finished = current;
            int f = finished.followUp;
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
            nextAttackTime = Time.time + Random.Range(Def.attackInterval.x, Def.attackInterval.y) / agg;
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
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(to), degPerSec * dt);
        }

        float FlatDistance(Vector3 p)
        {
            Vector3 d = p - transform.position; d.y = 0;
            return d.magnitude;
        }

        // ------------------------------------------------------------------ Trafienia

        public DefenseState CurrentDefense() => new DefenseState
        {
            position = transform.position,
            forward = transform.forward,
            guarding = CurrentState == State.Guarding,
            guard = Def.guard,
            stamina = Stamina.Current,
            physicalResist = Def.physicalResist,
            magicResist = Def.magicResist,
            dead = IsDead,
        };

        public HitResult ReceiveHit(HitData hit)
        {
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
            float instant = Status.OnLanded(hit, r, rules, out var landedReaction, out bool froze, out float shatter);
            if (shatter > 0) Shatter(shatter, hit);
            if (hit.mods.conductionJump && Status.LastConductionDamage > 0) JumpConduction(Status.LastConductionDamage * 0.5f, hit);
            r.healthDamage += instant;
            reaction = reaction ?? landedReaction;
            if (reaction != null) CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.7f, reaction, Names.ElementColor(hit.element));
            Health.Drain(r.healthDamage);
            if (r.staminaDamage > 0) Stamina.Drain(r.staminaDamage, 1f);
            if (froze && Health.Current > 0)
            {
                // Zamrożenie przerywa atak (także z pancerzem) – krócej na bossach i elitach.
                // Na czas zamrożenia pętla stoi, więc po odmrożeniu zostaje tylko krótkie drgnięcie.
                Interrupt(State.Staggered, 0.2f);
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.7f, "ZAMROŻONY", Names.StatusColor(StatusKind.Frozen));
            }

            if (Health.Current <= 0)
            {
                CombatEvents.RaiseHit(AimPoint, r, false);
                Die();
                return r;
            }

            if (CurrentState == State.Dormant) CurrentState = State.Moving;

            switch (r.outcome)
            {
                case HitOutcome.Hit:
                    Poise.Drain(r.poiseDamage);
                    poiseTimer = Def.poiseRegenDelay;
                    if (Poise.Current <= 0)
                    {
                        bool armored = CurrentState == State.Attacking && current != null && current.hyperArmor;
                        if (!armored && CurrentState != State.PoiseBroken)
                        {
                            Interrupt(State.Staggered, 0.75f);
                            Poise.SetCurrent(Poise.Max);
                        }
                    }
                    break;
                case HitOutcome.GuardBroken:
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
            ClearMarker();
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
                Interrupt(State.Recoil, 0.7f);
            }
        }

        public void OnHitResolved(IHitReceiver target, in HitData hit, in HitResult result) { }

        public void ReceiveRiposte(float damage, IHitReceiver attacker)
        {
            if (!CanBeRiposted) return;
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
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.7f, "ZAMROŻONY", Names.StatusColor(StatusKind.Frozen));
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
            FxLibrary.Blood(AimPoint, Vector3.up, 2.5f);
            if (Health.Current <= 0) Die();
            return dmg;
        }

        /// <summary>Tyknięcie efektu (krwawienie, podpalenie): już po pancerzu, bez postawy i drgnięcia.</summary>
        void TakeStatusDamage(float dmg)
        {
            Health.Drain(dmg);
            CombatEvents.RaiseWorldText(AimPoint, $"{dmg:0}", Status.BleedStacks > 0 ? Names.StatusColor(StatusKind.Bleed) : Names.StatusColor(StatusKind.Burn));
            if (Health.Current <= 0) Die();
        }

        void Die()
        {
            ClearMarker();
            Status.Clear();
            statusFx.Clear();
            CurrentState = State.Dead;
            MarkState(0f);
            current = null;
            cc.enabled = false;
            if (visual != null) { visual.SetWeaponGlow(Color.black, 0f); visual.SetTint(Color.black, 0.45f); }
            FxLibrary.DeathAsh(transform.position, Def.scale);
            All.Remove(this);
            CombatEvents.RaiseDied(this);
            Died?.Invoke(this);
            Destroy(gameObject, 3f);
        }

        /// <summary>Tylko do testów.</summary>
        public void DebugKill() { Health.Drain(Health.Current); Die(); }

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
                actionTime = Status.Frozen ? 0.12f : Time.time - stateEnterTime, // zamrożony – poza zatrzymana
                actionDuration = stateLength,
                hasShield = Def != null && Def.guardChance > 0,
            };
            switch (CurrentState)
            {
                case State.Attacking when current != null:
                {
                    var a = current.attack;
                    st.action = AnimAction.Attack;
                    st.attack = AnimResolve.ForAttack(a, comboIndex, weaponModel);
                    st.phase = attackPhase;
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
