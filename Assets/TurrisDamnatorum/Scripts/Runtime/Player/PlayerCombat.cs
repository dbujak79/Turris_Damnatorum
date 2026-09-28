using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wspólny system walki gracza. Nie zna pojęcia "klasy": wszystkie akcje i ich parametry
    /// pochodzą z <see cref="BuildSnapshot"/> (wyposażenie + poznane czary + wzmocnienia).
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

        public int SelectedSpell { get; private set; }
        public float WeaponBuffTime { get; private set; }
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
            SelectedSpell = 0;
        }

        /// <summary>Przelicza build po zmianie wyposażenia/wzmocnień. Obecne wartości są przycinane do nowych maksimów.</summary>
        public void RefreshBuild()
        {
            Build = BuildCalculator.Compute(Run, Config);
            Health.SetMax(Build.MaxHealth, false);
            Stamina.SetMax(Build.MaxStamina, false);
            Mana.SetMax(Build.MaxMana, false);
            if (SelectedSpell >= Build.spells.Count) SelectedSpell = 0;
            BuildChanged?.Invoke();
        }

        public void RestoreAll()
        {
            Health.SetCurrent(Health.Max);
            Mana.SetCurrent(Mana.Max);
            Stamina.SetCurrent(Stamina.Max);
        }

        public SpellRuntime CurrentSpell => Build.spells.Count > 0 ? Build.spells[Mathf.Clamp(SelectedSpell, 0, Build.spells.Count - 1)] : null;

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

        public void CycleSpell()
        {
            if (Build.spells.Count == 0) return;
            SelectedSpell = (SelectedSpell + 1) % Build.spells.Count;
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
                        return Actions.TryStart(ActionType.Riposte, B.riposteStartup, 0.05f, B.riposteRecovery);
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
                    Actions.TryStart(ActionType.Parry, p.startup, p.activeWindow, p.recovery);
                    Stamina.Drain(p.staminaCost, B.staminaRegenDelay);
                    return true;
                }
                case ActionType.Dodge:
                {
                    var d = Build.dodge;
                    if (Stamina.Current <= 0 || !Actions.CanStart(ActionType.Dodge)) return false;
                    Vector3 dir = MoveIntent.sqrMagnitude > 0.01f ? MoveIntent.normalized : -transform.forward;
                    DodgeDirection = dir;
                    Actions.TryStart(ActionType.Dodge, d.invulnStart, d.invulnDuration, Mathf.Max(0.05f, d.totalDuration - d.invulnStart - d.invulnDuration));
                    Stamina.Drain(d.staminaCost, B.staminaRegenDelay);
                    return true;
                }
                case ActionType.Cast:
                {
                    var spell = CurrentSpell;
                    if (spell == null) { CombatEvents.RaiseMessage("Brak przygotowanego czaru", Color.gray); return true; }
                    if (!Actions.CanStart(ActionType.Cast)) return false;
                    if (Mana.Current + 0.01f < spell.manaCost) { CombatEvents.RaiseMessage("Za mało many", new Color(0.4f, 0.6f, 1f)); return true; }
                    if (Stamina.Current <= 0) return false;
                    currentSpell = spell;
                    Actions.TryStart(ActionType.Cast, spell.Def.castTime, 0.05f, spell.Def.recovery, spell.Def.cancelAfter);
                    Mana.TrySpend(spell.manaCost);
                    Stamina.Drain(spell.Def.staminaCost, B.staminaRegenDelay);
                    return true;
                }
                case ActionType.Flask:
                {
                    int charges = flaskIsMana ? Run.manaFlasks : Run.healthFlasks;
                    if (charges <= 0) { CombatEvents.RaiseMessage("Flaszka pusta", Color.gray); return true; }
                    return Actions.TryStart(ActionType.Flask, B.flaskDrinkTime, 0.05f, B.flaskRecovery);
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
            Actions.TryStart(type, atk.windup, atk.active, atk.recovery, atk.cancelAfter);
            Stamina.Drain(atk.staminaCost, B.staminaRegenDelay);
            return true;
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
                    if (phase == ActionPhase.Active) FireSpell(currentSpell);
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
                restores.Add(new TimedRestore { mana = true, remaining = B.flaskEffectDuration, perSecond = Mana.Max * B.manaFlaskFraction * potency / B.flaskEffectDuration });
            }
            else
            {
                if (Run.healthFlasks <= 0) return;
                Run.healthFlasks--;
                restores.Add(new TimedRestore { mana = false, remaining = B.flaskEffectDuration, perSecond = Health.Max * B.healthFlaskFraction * potency / B.flaskEffectDuration });
            }
        }

        void PerformRiposte()
        {
            if (riposteTarget == null || !riposteTarget.CanBeRiposted) return;
            float dmg = Build.WeaponDamage(Build.weapon.light) * Build.weapon.riposteMultiplier * (1f + Build.stats[StatType.RiposteDamage] / 100f);
            riposteTarget.ReceiveRiposte(dmg, this);
            float heal = Build.effects[PassiveEffectType.RiposteHeal];
            if (heal > 0) Health.Restore(heal);
            CombatEvents.RaiseMessage("RIPOSTA!", new Color(1f, 0.85f, 0.3f));
        }

        void FireSpell(SpellRuntime spell)
        {
            if (spell == null) return;
            var def = spell.Def;
            float refund = Build.effects[PassiveEffectType.SpellStaminaRefund];
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
                    VisualFx.Ring(transform.position, def.attack.radius, def.color, 0.35f);
                    break;
                }
                case SpellKind.Heal:
                    restores.Add(new TimedRestore { mana = false, remaining = def.duration, perSecond = spell.power / Mathf.Max(0.1f, def.duration) });
                    VisualFx.Ring(transform.position, 1.5f, def.color, 0.5f);
                    break;
                case SpellKind.WeaponBuff:
                    WeaponBuffTime = def.duration;
                    weaponBuffDamage = spell.power;
                    CombatEvents.RaiseMessage($"{def.displayName}: +{spell.power:0} obrażeń magicznych na cios", def.color);
                    break;
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
                    VisualFx.Flash(transform.position + transform.forward * 0.7f + Vector3.up * 1.2f, Color.white, 0.4f);
                    break;
                }
                case HitOutcome.Blocked:
                {
                    float m = Build.effects[PassiveEffectType.BlockManaGain];
                    if (m > 0) Mana.Restore(m);
                    VisualFx.Flash(transform.position + transform.forward * 0.6f + Vector3.up * 1.1f, new Color(1f, 0.7f, 0.3f), 0.2f);
                    break;
                }
                case HitOutcome.GuardBroken:
                    Actions.Force(ActionType.GuardBroken, B.guardBreakStun);
                    CombatEvents.RaiseMessage("GARDA PRZEŁAMANA!", new Color(1f, 0.4f, 0.2f));
                    break;
                case HitOutcome.Hit:
                    Actions.Force(ActionType.Flinch, B.flinchDuration);
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
                case ActionType.Cast: s.action = AnimAction.Cast; break;
                case ActionType.Flask: s.action = AnimAction.Drink; break;
                case ActionType.Flinch: s.action = AnimAction.Flinch; break;
                case ActionType.GuardBroken: s.action = AnimAction.GuardBroken; break;
                case ActionType.Dead: s.action = AnimAction.Death; s.actionTime = deathTime; break;
            }
            return s;
        }

        /// <summary>Tylko do testów/debugowania.</summary>
        public void DebugKill() { Health.Drain(Health.Current); Die(); }
    }
}
