using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wspólny system walki gracza. Nie zna pojęcia "klasy": wszystkie akcje i ich parametry
    /// pochodzą z <see cref="BuildSnapshot"/> (wyposażenie + umiejętności w slotach + wzmocnienia).
    ///
    /// Akcje: szybki atak, mocny atak, blok, parowanie, unik, flaszki oraz trzy sloty umiejętności
    /// (czary za manę albo techniki bronią za wytrzymałość), każdy z własnym odnowieniem.
    /// </summary>
    public class PlayerCombat : MonoBehaviour, IHitReceiver, ICharacterAnimSource
    {
        public GameConfig Config { get; private set; }
        public RunState Run { get; private set; }
        public BuildSnapshot Build { get; private set; }

        public ResourcePool Health { get; private set; }
        public ResourcePool Stamina { get; private set; }
        public ResourcePool Mana { get; private set; }
        public readonly ActionController Actions = new ActionController();

        public float WeaponBuffTime { get; private set; }
        /// <summary>Osłona (umiejętność Barrier): pochłania obrażenia przed utratą życia.</summary>
        public float BarrierAmount { get; private set; }
        public float BarrierTime { get; private set; }
        /// <summary>Prędkość narzucana przez umiejętność (szarża). Zero = brak.</summary>
        public Vector3 SkillMotion { get; private set; }
        public Color WeaponBuffColor { get; private set; } = new Color(0.45f, 0.65f, 1f);
        /// <summary>Dłoń do efektów (z humanoida, a gdy go brak – sama postać).</summary>
        Transform Hand
        {
            get
            {
                var v = GetComponent<CharacterVisual>();
                return v != null && v.RightHand != null ? v.RightHand : transform;
            }
        }
        float weaponBuffDamage;

        public Faction Faction => Faction.Player;
        public bool IsDead => Actions.IsDead;
        public Transform Transform => transform;
        public Vector3 AimPoint => transform.position + Vector3.up * 1.0f;

        /// <summary>Aktualny cel namierzania (ustawiany przez PlayerController).</summary>
        public EnemyBrain LockTarget { get; set; }
        /// <summary>Kierunek wejścia względem kamery (dla uniku).</summary>
        public Vector3 MoveIntent { get; set; }

        public event Action Died;
        public event Action BuildChanged;

        // Stan bieżącej akcji
        AttackDefinition currentAttack;
        SpellRuntime currentSpell;
        ParticleSystem gatherFx, barrierFx;
        int requestedSlot;
        float skillActiveStart, skillHitTimer;
        Vector3 chargeDir, lastChargePos;
        bool flaskIsMana;
        EnemyBrain riposteTarget;
        public Vector3 DodgeDirection { get; private set; }
        readonly HitTracker tracker = new HitTracker();

        // Bufor wejścia
        ActionType buffered = ActionType.None;
        float bufferedTime;
        float clock; // własny zegar symulacji (deterministyczny w testach)
        float deathTime;

        struct TimedRestore { public bool mana; public float perSecond; public float remaining; }
        readonly List<TimedRestore> restores = new List<TimedRestore>();

        BalanceConfig B => Config.balance;

        // ------------------------------------------------------------------ Tempo akcji

        float Speed => Mathf.Max(0.1f, B.playerActionSpeed);
        float RecoveryScale => Mathf.Clamp(B.playerRecoveryScale, 0.1f, 1f);
        /// <summary>Czas przygotowania/fazy aktywnej akcji gracza po uwzględnieniu tempa.</summary>
        public float ScaleStartup(float t) => t / Speed;
        /// <summary>Czas fazy zakończenia (i punktu przerwania) akcji gracza po uwzględnieniu tempa.</summary>
        public float ScaleRecovery(float t) => float.IsInfinity(t) ? t : t / Speed * RecoveryScale;

        /// <summary>Faktyczne czasy bieżącego ataku bronią (dla wypadu i obrotu w PlayerController).</summary>
        public float AttackWindup { get; private set; }
        public float AttackActive { get; private set; }

        public void Init(RunState run, GameConfig cfg)
        {
            Run = run;
            Config = cfg;
            Actions.Reset();
            deathTime = 0f;
            Actions.PhaseChanged -= OnPhaseChanged;
            Actions.PhaseChanged += OnPhaseChanged;
            Build = BuildCalculator.Compute(run, cfg);
            Health = new ResourcePool(Build.MaxHealth);
            Stamina = new ResourcePool(Build.MaxStamina);
            Mana = new ResourcePool(Build.MaxMana);
            Health.SetCurrent(Build.MaxHealth * run.healthFraction);
            Mana.SetCurrent(Build.MaxMana * run.manaFraction);
            restores.Clear();
            WeaponBuffTime = 0;
            OnFloorStart();
        }

        /// <summary>Nowe piętro: pełne ładunki umiejętności, bez osłony.</summary>
        public void OnFloorStart()
        {
            if (Run != null) foreach (var s in Run.knownSpells) s.ResetCooldown();
            EndBarrier(false);
            SkillMotion = Vector3.zero;
        }

        /// <summary>Przelicza build po zmianie wyposażenia/wzmocnień. Obecne wartości są przycinane do nowych maksimów.</summary>
        public void RefreshBuild()
        {
            Build = BuildCalculator.Compute(Run, Config);
            Health.SetMax(Build.MaxHealth, false);
            Stamina.SetMax(Build.MaxStamina, false);
            Mana.SetMax(Build.MaxMana, false);
            BuildChanged?.Invoke();
        }

        public void RestoreAll()
        {
            Health.SetCurrent(Health.Max);
            Mana.SetCurrent(Mana.Max);
            Stamina.SetCurrent(Stamina.Max);
        }

        /// <summary>Umiejętność w slocie 0–2 (null = pusty slot).</summary>
        public SpellRuntime Skill(int slot) => Build != null && slot >= 0 && slot < Build.skills.Length ? Build.skills[slot] : null;

        /// <summary>Umiejętność wykonywana w tej chwili (null, gdy postać nie używa umiejętności).</summary>
        public SpellRuntime ActiveSkill => Actions.Current == ActionType.Cast ? currentSpell : null;

        /// <summary>Czy umiejętność w slocie da się teraz użyć (odnowienie, zasób, wyposażenie) – dla HUD.</summary>
        public bool SkillUsable(int slot)
        {
            var sk = Skill(slot);
            if (sk == null || !sk.equipmentMet || !sk.instance.Ready || Stamina.Current <= 0) return false;
            return !sk.Def.IsSpell || Mana.Current + 0.01f >= sk.manaCost;
        }

        // ------------------------------------------------------------------ Żądania akcji

        public void Request(ActionType type)
        {
            if (IsDead) return;
            if (!TryExecute(type))
            {
                buffered = type;
                bufferedTime = clock;
            }
            else buffered = ActionType.None;
        }

        /// <summary>Użycie umiejętności ze slotu 0–2 (przyciski umiejętności 1/2/3).</summary>
        public void RequestSkill(int slot)
        {
            requestedSlot = slot;
            Request(ActionType.Cast);
        }

        public void RequestFlask(bool mana)
        {
            flaskIsMana = mana;
            Request(ActionType.Flask);
        }

        bool TryExecute(ActionType type)
        {
            switch (type)
            {
                case ActionType.LightAttack:
                {
                    var target = FindRiposteTarget();
                    if (target != null && Actions.CanStart(ActionType.Riposte))
                    {
                        riposteTarget = target;
                        return Actions.TryStart(ActionType.Riposte, ScaleStartup(B.riposteStartup), 0.05f, ScaleRecovery(B.riposteRecovery));
                    }
                    return StartAttack(ActionType.LightAttack, Build.weapon.light);
                }
                case ActionType.HeavyAttack:
                    return StartAttack(ActionType.HeavyAttack, Build.weapon.heavy);
                case ActionType.Parry:
                {
                    var p = Build.parry;
                    if (p == null) { CombatEvents.RaiseMessage("Brak broni/tarczy zdolnej do parowania", Color.gray); return true; }
                    if (Stamina.Current <= 0 || !Actions.CanStart(ActionType.Parry)) return false;
                    // Okno parowania nie jest skracane przez tempo – tylko przygotowanie i zakończenie.
                    Actions.TryStart(ActionType.Parry, ScaleStartup(p.startup), p.activeWindow, ScaleRecovery(p.recovery));
                    Stamina.Drain(p.staminaCost, B.staminaRegenDelay);
                    return true;
                }
                case ActionType.Dodge:
                {
                    var d = Build.dodge;
                    if (Stamina.Current <= 0 || !Actions.CanStart(ActionType.Dodge)) return false;
                    Vector3 dir = MoveIntent.sqrMagnitude > 0.01f ? MoveIntent.normalized : -transform.forward;
                    var interrupted = Actions.Current;
                    var interruptedPhase = Actions.Phase;
                    DodgeDirection = dir;
                    Actions.TryStart(ActionType.Dodge, d.invulnStart, d.invulnDuration, Mathf.Max(0.05f, d.totalDuration - d.invulnStart - d.invulnDuration), B.dodgeCancelAfter);
                    // Unik przerwał inkantację przed wypuszczeniem czaru – mana wraca.
                    if (interrupted == ActionType.Cast && interruptedPhase == ActionPhase.Startup) CancelCast();
                    FxLibrary.Dust(transform.position);
                    Stamina.Drain(d.staminaCost, B.staminaRegenDelay);
                    return true;
                }
                case ActionType.Cast:
                {
                    var skill = Skill(requestedSlot);
                    if (skill == null) { CombatEvents.RaiseMessage($"Slot {requestedSlot + 1} jest pusty", Color.gray); return true; }
                    var def = skill.Def;
                    if (!skill.equipmentMet) { CombatEvents.RaiseMessage($"{def.displayName}: {Names.Requirement(def.requiredTags)}", Color.gray); return true; }
                    if (!Actions.CanStart(ActionType.Cast)) return false;
                    if (!skill.instance.Ready) return false; // odnowienie – żądanie czeka w buforze
                    if (def.IsSpell && Mana.Current + 0.01f < skill.manaCost) { CombatEvents.RaiseMessage("Za mało many", new Color(0.4f, 0.6f, 1f)); return true; }
                    if (Stamina.Current <= 0) return false;
                    currentSpell = skill;
                    float castTime = ScaleStartup(def.castTime);
                    Actions.TryStart(ActionType.Cast, castTime, SkillActiveTime(def), ScaleRecovery(def.recovery), ScaleRecovery(def.cancelAfter));
                    skill.instance.TryUse();
                    tracker.Reset();
                    gatherFx = def.IsSpell ? FxLibrary.Gather(Hand, def.color, castTime) : null;
                    if (def.IsSpell) Mana.TrySpend(skill.manaCost);
                    Stamina.Drain(skill.staminaCost, B.staminaRegenDelay);
                    return true;
                }
                case ActionType.Flask:
                {
                    int charges = flaskIsMana ? Run.manaFlasks : Run.healthFlasks;
                    if (charges <= 0) { CombatEvents.RaiseMessage("Flaszka pusta", Color.gray); return true; }
                    return Actions.TryStart(ActionType.Flask, ScaleStartup(B.flaskDrinkTime), 0.05f, ScaleRecovery(B.flaskRecovery));
                }
            }
            return false;
        }

        bool StartAttack(ActionType type, AttackDefinition atk)
        {
            if (Stamina.Current <= 0 || !Actions.CanStart(type)) return false;
            if (type == ActionType.LightAttack && Actions.Current == ActionType.LightAttack && Actions.ComboIndex + 1 >= Build.weapon.lightComboLength)
                return false; // koniec serii – trzeba poczekać
            currentAttack = atk;
            AttackWindup = ScaleStartup(atk.windup);
            AttackActive = ScaleStartup(atk.active);
            Actions.TryStart(type, AttackWindup, AttackActive, ScaleRecovery(atk.recovery), ScaleRecovery(atk.cancelAfter));
            Stamina.Drain(atk.staminaCost, B.staminaRegenDelay);
            return true;
        }

        /// <summary>Czas fazy aktywnej umiejętności: szarża i młynek trwają, reszta działa w jednej chwili.</summary>
        float SkillActiveTime(SpellDefinition def)
        {
            switch (def.kind)
            {
                case SpellKind.Charge:
                case SpellKind.Whirlwind: return ScaleStartup(Mathf.Max(0.1f, def.duration));
                case SpellKind.Cleave:
                case SpellKind.Quake:
                case SpellKind.ShieldBash: return ScaleStartup(Mathf.Max(0.05f, def.attack.active));
                default: return 0.05f;
            }
        }

        /// <summary>Unik przerwał umiejętność przed jej wyzwoleniem – wraca mana i ładunek (wytrzymałość przepada).</summary>
        void CancelCast()
        {
            if (currentSpell != null)
            {
                if (currentSpell.Def.IsSpell) Mana.Restore(currentSpell.manaCost);
                currentSpell.instance.Refund();
            }
            currentSpell = null;
            if (gatherFx != null) gatherFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            gatherFx = null;
        }

        EnemyBrain FindRiposteTarget()
        {
            EnemyBrain best = null; float bestD = float.MaxValue;
            foreach (var e in EnemyBrain.All)
            {
                if (!e.CanBeRiposted) continue;
                Vector3 to = e.transform.position - transform.position; to.y = 0;
                float d = to.magnitude;
                if (d > B.riposteRange) continue;
                if (Vector3.Angle(transform.forward, to) > B.riposteAngle * 0.5f) continue;
                if (d < bestD) { bestD = d; best = e; }
            }
            return best;
        }

        // ------------------------------------------------------------------ Fazy akcji

        void OnPhaseChanged(ActionType type, ActionPhase phase)
        {
            switch (type)
            {
                case ActionType.LightAttack:
                case ActionType.HeavyAttack:
                    if (phase == ActionPhase.Startup) tracker.Reset();
                    break;
                case ActionType.Cast:
                    if (phase == ActionPhase.Active) { skillActiveStart = clock; FireSpell(currentSpell); }
                    else if (phase == ActionPhase.Recovery || phase == ActionPhase.Finished)
                    {
                        if (SkillMotion != Vector3.zero) FxLibrary.Dust(transform.position, 1.3f);
                        SkillMotion = Vector3.zero;
                    }
                    break;
                case ActionType.Flask:
                    if (phase == ActionPhase.Active) DrinkFlask();
                    break;
                case ActionType.Riposte:
                    if (phase == ActionPhase.Active) PerformRiposte();
                    break;
            }
        }

        void DrinkFlask()
        {
            float potency = 1f + Build.stats[StatType.FlaskPotency] / 100f;
            if (flaskIsMana)
            {
                if (Run.manaFlasks <= 0) return;
                Run.manaFlasks--;
                FxLibrary.Drink(transform, new Color(0.35f, 0.6f, 1f));
                restores.Add(new TimedRestore { mana = true, remaining = B.flaskEffectDuration, perSecond = Mana.Max * B.manaFlaskFraction * potency / B.flaskEffectDuration });
            }
            else
            {
                if (Run.healthFlasks <= 0) return;
                Run.healthFlasks--;
                FxLibrary.Drink(transform, new Color(1f, 0.55f, 0.15f));
                restores.Add(new TimedRestore { mana = false, remaining = B.flaskEffectDuration, perSecond = Health.Max * B.healthFlaskFraction * potency / B.flaskEffectDuration });
            }
        }

        void PerformRiposte()
        {
            if (riposteTarget == null || !riposteTarget.CanBeRiposted) return;
            float dmg = Build.WeaponDamage(Build.weapon.light) * Build.weapon.riposteMultiplier * (1f + Build.stats[StatType.RiposteDamage] / 100f);
            riposteTarget.ReceiveRiposte(dmg, this);
            FxLibrary.Flash(riposteTarget.AimPoint, new Color(1f, 0.8f, 0.35f), 1.4f);
            FxLibrary.Blood(riposteTarget.AimPoint, transform.forward, 2f);
            float heal = Build.effects[PassiveEffectType.RiposteHeal];
            if (heal > 0) Health.Restore(heal);
            CombatEvents.RaiseMessage("RIPOSTA!", new Color(1f, 0.85f, 0.3f));
        }

        void FireSpell(SpellRuntime spell)
        {
            if (spell == null) return;
            var def = spell.Def;
            float refund = def.IsSpell ? Build.effects[PassiveEffectType.SpellStaminaRefund] : 0f;
            if (refund > 0) Stamina.Restore(refund);

            switch (def.kind)
            {
                case SpellKind.Projectile:
                {
                    Vector3 origin = transform.position + Vector3.up * 1.3f + transform.forward * 0.8f;
                    Vector3 aim = LockTarget != null ? (LockTarget.AimPoint - origin) : transform.forward;
                    aim.y = LockTarget != null ? aim.y : 0f;
                    int count = Mathf.Max(1, def.attack.projectileCount);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = count == 1 ? 0 : Mathf.Lerp(-def.attack.spreadAngle, def.attack.spreadAngle, i / (float)(count - 1));
                        Vector3 dir = Quaternion.Euler(0, angle, 0) * aim.normalized;
                        var hit = HitData.FromAttack(def.attack, spell.power, origin, this);
                        Projectile.Spawn(origin, dir, def.attack.projectileSpeed, hit, Faction.Player, this, def.color, 0.22f, 3f,
                            LockTarget, LockTarget != null ? 90f : 0f);
                    }
                    break;
                }
                case SpellKind.Nova:
                {
                    var hit = HitData.FromAttack(def.attack, spell.power, transform.position, this);
                    tracker.Reset();
                    HitQuery.Sphere(transform.position + Vector3.up, def.attack.radius, Faction.Player, tracker,
                        r => HitQuery.Apply(this, r, hit, false));
                    FxLibrary.Shockwave(transform.position, def.color, def.attack.radius);
                    break;
                }
                case SpellKind.Heal:
                    restores.Add(new TimedRestore { mana = false, remaining = def.duration, perSecond = spell.power / Mathf.Max(0.1f, def.duration) });
                    FxLibrary.Heal(transform, def.color, def.duration);
                    break;
                case SpellKind.WeaponBuff:
                    WeaponBuffTime = def.duration;
                    weaponBuffDamage = spell.power;
                    WeaponBuffColor = def.color;
                    FxLibrary.Shockwave(transform.position, def.color, 1.2f, false);
                    CombatEvents.RaiseMessage($"{def.displayName}: +{spell.power:0} obrażeń magicznych na cios", def.color);
                    break;
                case SpellKind.Barrier:
                    EndBarrier(false);
                    BarrierAmount = spell.power;
                    BarrierTime = def.duration;
                    barrierFx = FxLibrary.Aura(transform, Vector3.up, def.color, 40f, 0.9f, 0.14f);
                    FxLibrary.Shockwave(transform.position, def.color, 1.6f, false);
                    CombatEvents.RaiseMessage($"{def.displayName}: osłona {spell.power:0}", def.color);
                    break;

                // ---- Techniki bronią
                case SpellKind.Cleave:
                case SpellKind.ShieldBash:
                    ArcHit(spell, def.attack.reach, def.arcAngle);
                    if (def.kind == SpellKind.ShieldBash) FxLibrary.Flash(transform.position + Vector3.up + transform.forward * 1.1f, def.color, 1.2f);
                    else FxLibrary.Shockwave(transform.position + transform.forward * 0.8f, def.color, def.attack.reach * 0.8f, false);
                    break;
                case SpellKind.Quake:
                {
                    Vector3 center = transform.position + transform.forward * def.attack.reach;
                    HitQuery.Sphere(center + Vector3.up * 0.5f, def.attack.radius, Faction.Player, tracker, t => TechniqueHit(t, spell));
                    FxLibrary.Shockwave(center, def.color, def.attack.radius);
                    FxLibrary.Dust(center, 2f);
                    break;
                }
                case SpellKind.Charge:
                    chargeDir = transform.forward;
                    lastChargePos = transform.position;
                    FxLibrary.Dust(transform.position, 1.2f);
                    break;
                case SpellKind.Whirlwind:
                    skillHitTimer = 0f;
                    FxLibrary.Dust(transform.position, 1.2f);
                    break;
            }
        }

        /// <summary>Trwające umiejętności (szarża, młynek) – wywoływane co klatkę w fazie aktywnej.</summary>
        void SkillActiveTick(float dt)
        {
            var sk = currentSpell;
            if (sk == null) return;
            var def = sk.Def;
            switch (def.kind)
            {
                case SpellKind.Charge:
                {
                    // Dystans = attack.reach w czasie fazy aktywnej; trafia każdego wroga na drodze raz.
                    SkillMotion = chargeDir * (def.attack.reach / Mathf.Max(0.05f, ScaleStartup(def.duration)));
                    Vector3 up = Vector3.up * 1.0f;
                    HitQuery.Capsule(lastChargePos + up, transform.position + up + chargeDir * 0.6f, def.attack.radius, Faction.Player, tracker, t => TechniqueHit(t, sk));
                    lastChargePos = transform.position;
                    break;
                }
                case SpellKind.Whirlwind:
                {
                    skillHitTimer -= dt;
                    if (skillHitTimer > 0) break;
                    skillHitTimer = Mathf.Max(0.1f, def.tickInterval);
                    tracker.Reset();
                    HitQuery.Sphere(transform.position + Vector3.up, def.attack.radius, Faction.Player, tracker, t => TechniqueHit(t, sk));
                    break;
                }
            }
        }

        /// <summary>Trafienia w łuku przed postacią (rozpłatanie, uderzenie tarczą).</summary>
        void ArcHit(SpellRuntime sk, float reach, float arc)
        {
            Vector3 fwd = transform.forward;
            HitQuery.Sphere(transform.position + Vector3.up, reach, Faction.Player, tracker, t =>
            {
                Vector3 to = t.Transform.position - transform.position; to.y = 0;
                if (to.sqrMagnitude > 0.04f && Vector3.Angle(fwd, to) > arc * 0.5f) return;
                TechniqueHit(t, sk);
            });
        }

        /// <summary>Obrażenia techniki = lekki atak broni × moc techniki (+ zaklęte ostrze). Liczy się jako cios bronią.</summary>
        void TechniqueHit(IHitReceiver target, SpellRuntime sk)
        {
            float dmg = Build.WeaponDamage(Build.weapon.light) * sk.power;
            var hit = HitData.FromAttack(sk.Def.attack, dmg, transform.position, this);
            hit.fromMeleeWeapon = true;
            hit.attackName = sk.Def.displayName;
            if (WeaponBuffTime > 0) hit.magic += weaponBuffDamage;
            HitQuery.Apply(this, target, hit, true);
        }

        void EndBarrier(bool broken)
        {
            if (broken) CombatEvents.RaiseMessage("Osłona rozbita", new Color(0.8f, 0.7f, 0.5f));
            BarrierAmount = 0;
            BarrierTime = 0;
            if (barrierFx != null)
            {
                barrierFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(barrierFx.gameObject, 1f);
                barrierFx = null;
            }
        }

        // ------------------------------------------------------------------ Pętla

        void Update() => Tick(Time.deltaTime);

        /// <summary>Jeden krok symulacji (publiczny na potrzeby testów).</summary>
        public void Tick(float dt)
        {
            if (Run == null) return;
            if (IsDead) { deathTime += dt; return; }
            clock += dt;

            // Bufor wejścia – spróbuj ponownie wykonać zaległą akcję.
            if (buffered != ActionType.None)
            {
                if (clock - bufferedTime > B.inputBuffer) buffered = ActionType.None;
                else if (TryExecute(buffered)) buffered = ActionType.None;
            }

            Actions.Tick(dt);

            // Aktywna faza ataku bronią: przeciągnij hitbox (każdy cel najwyżej raz na zamach).
            if ((Actions.Current == ActionType.LightAttack || Actions.Current == ActionType.HeavyAttack) && Actions.Phase == ActionPhase.Active)
                SweepWeapon();
            if (Actions.Current == ActionType.Cast && Actions.Phase == ActionPhase.Active) SkillActiveTick(dt);
            else SkillMotion = Vector3.zero;

            // Odnowienia umiejętności (całej kolekcji – przełożenie do innego slotu niczego nie zeruje) i osłona.
            foreach (var s in Run.knownSpells) s.Tick(dt);
            if (BarrierTime > 0) { BarrierTime -= dt; if (BarrierTime <= 0) EndBarrier(false); }

            // Regeneracja
            var cur = Actions.Current;
            bool staminaRegenAllowed = cur == ActionType.None || cur == ActionType.Block || cur == ActionType.Flask;
            float stRegen = Build.stats[StatType.StaminaRegen];
            if (cur == ActionType.Block && Build.guard != null) stRegen *= Build.guard.staminaRegenMultiplier;
            if (Sprinting) { stRegen = 0; Stamina.Drain(B.sprintStaminaPerSecond * dt, 0.3f); }
            Stamina.Tick(dt, staminaRegenAllowed ? stRegen : 0f);
            Health.Tick(dt, Build.stats[StatType.HealthRegen]);
            Mana.Tick(dt, Build.stats[StatType.ManaRegen]);

            for (int i = restores.Count - 1; i >= 0; i--)
            {
                var r = restores[i];
                float step = Mathf.Min(dt, r.remaining);
                if (r.mana) Mana.Restore(r.perSecond * step); else Health.Restore(r.perSecond * step);
                r.remaining -= dt;
                if (r.remaining <= 0) restores.RemoveAt(i); else restores[i] = r;
            }

            if (WeaponBuffTime > 0) WeaponBuffTime -= dt;
        }

        /// <summary>Ustawiane przez PlayerController.</summary>
        public bool Sprinting { get; set; }

        public void SetBlockHeld(bool held)
        {
            if (IsDead) return;
            if (held && Build.CanBlock)
            {
                if (Actions.IsIdle) Actions.TryStartBlock();
            }
            else if (Actions.IsBlocking) Actions.EndBlock();
        }

        void SweepWeapon()
        {
            var atk = currentAttack;
            if (atk == null) return;
            Vector3 from = transform.position + Vector3.up * 1.0f + transform.forward * 0.3f;
            Vector3 to = transform.position + Vector3.up * 1.0f + transform.forward * atk.reach;
            HitQuery.Capsule(from, to, atk.radius, Faction.Player, tracker, target =>
            {
                float dmg = Build.WeaponDamage(atk);
                var hit = HitData.FromAttack(atk, dmg, transform.position, this);
                hit.fromMeleeWeapon = true;
                if (WeaponBuffTime > 0) hit.magic += weaponBuffDamage;
                HitQuery.Apply(this, target, hit, true);
            });
        }

        public void OnHitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (hit.fromMeleeWeapon && (result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken || result.outcome == HitOutcome.Blocked))
            {
                float m = Build.effects[PassiveEffectType.ManaOnMeleeHit];
                if (m > 0) Mana.Restore(m);
            }
            if (target.IsDead)
            {
                float h = Build.effects[PassiveEffectType.HealOnKill];
                if (h > 0) Health.Restore(h);
            }
        }

        public void OnAttackParried(float poiseDamage)
        {
            // Przeciwnicy nie parują w prototypie; gdyby parowali – odrzut.
            Actions.Force(ActionType.Flinch, 0.8f);
        }

        // ------------------------------------------------------------------ Przyjmowanie trafień

        public DefenseState CurrentDefense()
        {
            var cur = Actions.Current;
            return new DefenseState
            {
                position = transform.position,
                forward = transform.forward,
                invulnerable = (cur == ActionType.Dodge && Actions.Phase == ActionPhase.Active) || cur == ActionType.Riposte,
                parryActive = cur == ActionType.Parry && Actions.Phase == ActionPhase.Active,
                parry = Build.parry,
                guarding = cur == ActionType.Block,
                guard = Build.guard,
                stamina = Stamina.Current,
                physicalResist = Build.PhysicalReduction(B),
                magicResist = Build.MagicReduction(B),
                dead = IsDead,
            };
        }

        public HitResult ReceiveHit(HitData hit)
        {
            var r = DamageResolver.Resolve(hit, CurrentDefense(), B);
            if (r.outcome == HitOutcome.Ignored) return r;

            // Osłona pochłania obrażenia przed utratą życia; w pełni pochłonięty cios nie wywołuje drgnięcia.
            bool absorbedAll = false;
            if (BarrierAmount > 0 && r.healthDamage > 0)
            {
                float absorbed = Mathf.Min(BarrierAmount, r.healthDamage);
                BarrierAmount -= absorbed;
                r.healthDamage -= absorbed;
                absorbedAll = r.healthDamage <= 0.01f;
                if (BarrierAmount <= 0.01f) EndBarrier(true);
            }

            Health.Drain(r.healthDamage);
            if (r.staminaDamage > 0) Stamina.Drain(r.staminaDamage, B.staminaRegenDelay);

            switch (r.outcome)
            {
                case HitOutcome.Parried:
                {
                    var fx = Build.effects;
                    if (fx[PassiveEffectType.ParryRestoreStamina] > 0) Stamina.Restore(fx[PassiveEffectType.ParryRestoreStamina]);
                    if (fx[PassiveEffectType.ParryRestoreMana] > 0) Mana.Restore(fx[PassiveEffectType.ParryRestoreMana]);
                    if (fx[PassiveEffectType.ParryHeal] > 0) Health.Restore(fx[PassiveEffectType.ParryHeal]);
                    CombatEvents.RaiseMessage("PAROWANIE!", new Color(1f, 0.95f, 0.5f));
                    break;
                }
                case HitOutcome.Blocked:
                {
                    float m = Build.effects[PassiveEffectType.BlockManaGain];
                    if (m > 0) Mana.Restore(m);
                    break;
                }
                case HitOutcome.GuardBroken:
                    Actions.Force(ActionType.GuardBroken, B.guardBreakStun);
                    CombatEvents.RaiseMessage("GARDA PRZEŁAMANA!", new Color(1f, 0.4f, 0.2f));
                    break;
                case HitOutcome.Hit:
                    if (!absorbedAll) Actions.Force(ActionType.Flinch, B.flinchDuration);
                    break;
            }

            CombatEvents.RaiseHit(AimPoint, r, true);
            if (Health.Current <= 0 && !IsDead) Die();
            return r;
        }

        void Die()
        {
            Actions.Force(ActionType.Dead, 0f);
            CombatEvents.RaiseMessage("POLEGŁEŚ", new Color(0.8f, 0.1f, 0.1f));
            Died?.Invoke();
        }

        // ------------------------------------------------------------------ Animacja

        public CharacterAnimState GetAnimState()
        {
            var a = Actions;
            var s = new CharacterAnimState
            {
                phase = a.Phase,
                phaseProgress = a.PhaseProgress,
                actionTime = a.Elapsed,
                actionDuration = a.FiniteDuration,
                dodgeDirection = DodgeDirection,
                rollDuration = Build != null ? Build.dodge.rollDuration : 0f,
                hasShield = Build != null && Build.guardIsShield,
                weaponGuard = Build != null && Build.guard != null && !Build.guardIsShield,
            };
            if (Build == null) return s;
            var weaponModel = AnimResolve.ForItem(Build.mainHand.definition);
            switch (a.Current)
            {
                case ActionType.LightAttack:
                    s.action = AnimAction.Attack;
                    s.attack = AnimResolve.ForAttack(currentAttack ?? Build.weapon.light, a.ComboIndex, weaponModel);
                    break;
                case ActionType.HeavyAttack:
                    s.action = AnimAction.Attack;
                    s.attack = AnimResolve.ForAttack(currentAttack ?? Build.weapon.heavy, 0, weaponModel);
                    break;
                case ActionType.Riposte: s.action = AnimAction.Attack; s.attack = AttackAnim.Thrust; break;
                case ActionType.Block: s.action = AnimAction.Block; break;
                case ActionType.Parry: s.action = AnimAction.Parry; break;
                case ActionType.Dodge: s.action = AnimAction.Dodge; break;
                case ActionType.Cast:
                {
                    var def = currentSpell?.Def;
                    if (def == null || def.IsSpell) { s.action = AnimAction.Cast; break; }
                    // Techniki używają póz ataku bronią (lub wypchnięcia tarczy), zsynchronizowanych z fazami umiejętności.
                    s.action = AnimAction.Attack;
                    switch (def.kind)
                    {
                        case SpellKind.Charge: s.attack = AttackAnim.Thrust; break;
                        case SpellKind.Quake: s.attack = AttackAnim.Slam; break;
                        case SpellKind.ShieldBash: s.action = AnimAction.Parry; break;
                        case SpellKind.Whirlwind:
                            s.attack = AttackAnim.SlashRight;
                            if (a.Phase == ActionPhase.Active)
                            {
                                s.phaseProgress = 0.55f; // ramię wyprostowane, ciało wiruje
                                s.spinAngle = -(clock - skillActiveStart) * 900f;
                            }
                            break;
                        default: s.attack = AttackAnim.SlashRight; break;
                    }
                    break;
                }
                case ActionType.Flask: s.action = AnimAction.Drink; break;
                case ActionType.Flinch: s.action = AnimAction.Flinch; break;
                case ActionType.GuardBroken: s.action = AnimAction.GuardBroken; break;
                case ActionType.Dead: s.action = AnimAction.Death; s.actionTime = deathTime; break;
            }
            return s;
        }

        /// <summary>Tylko do testów/debugowania.</summary>
        /// <summary>Kolor bieżącej umiejętności (poświata broni).</summary>
        public Color ActiveSkillColor => currentSpell != null ? currentSpell.Def.color : new Color(0.5f, 0.6f, 1f);

        public void DebugKill() { Health.Drain(Health.Current); Die(); }
    }
}
