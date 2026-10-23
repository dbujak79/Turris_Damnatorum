using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    public struct DodgeParams
    {
        public float staminaCost, distance, invulnStart, invulnDuration, totalDuration, rollDuration;
    }

    public class SpellRuntime
    {
        public SpellInstance instance;
        public float manaCost;
        public float staminaCost;
        public float power;            // czary: obrażenia / leczenie / premia; techniki: mnożnik obrażeń broni
        public bool requirementsMet;   // atrybuty (miękkie – obniżona skuteczność)
        public bool equipmentMet;      // wymagane wyposażenie (twarde – bez niego nie da się użyć)
        public SpellDefinition Def => instance.definition;
    }

    /// <summary>
    /// Wynik przeliczenia buildu: statystyki, efekty i ZESTAW DOSTĘPNYCH AKCJI.
    /// Akcje wynikają wyłącznie z wyposażenia i poznanych czarów – klasa startowa nie jest tu w ogóle czytana
    /// (poza atrybutami bazowymi skopiowanymi do RunState na starcie).
    /// </summary>
    public class BuildSnapshot
    {
        public StatSheet stats;
        public EffectTotals effects;

        public ItemInstance mainHand;       // nigdy null – przy pustej ręce: pięści
        public WeaponData weapon;
        public float weaponEffectiveness;   // < 1 gdy nie spełniono wymagań

        public GuardData guard;             // null = brak możliwości bloku
        public bool guardIsShield;
        public ParryData parry;             // null = brak możliwości parowania

        public DodgeParams dodge;
        public float loadRatio;
        /// <summary>Szansa (0–1), że bezpośrednie trafienie wroga chybi (Zręczność).</summary>
        public float evasionChance;
        /// <summary>Mnożnik szybkości ataków bronią (Zręczność, kara za niespełnione wymagania broni).</summary>
        public float attackSpeed = 1f;

        /// <summary>Umiejętności w slotach 1–3 (null = pusty slot).</summary>
        public readonly SpellRuntime[] skills = new SpellRuntime[RunState.SkillSlotCount];
        /// <summary>Skill klasy (osobny przycisk); null, gdy klasa go nie ma.</summary>
        public SpellRuntime signature;
        /// <summary>Druga broń w lewej ręce (walka dwiema brońmi) – null, gdy brak.</summary>
        public ItemInstance offWeapon;
        /// <summary>Klinga zaklęć: moc czarów działa też na ciosy bronią (1 = brak).</summary>
        public float spellbladeFactor = 1f;

        public bool CanBlock => guard != null;
        public bool CanParry => parry != null;

        public float MaxHealth => stats[StatType.MaxHealth];
        public float MaxStamina => stats[StatType.MaxStamina];
        public float MaxMana => stats[StatType.MaxMana];

        /// <summary>Obrażenia ataku bronią: baza × skalowanie atrybutów × premie × poziom × wymagania.</summary>
        public float WeaponDamage(AttackDefinition atk)
        {
            float scaling = (stats[StatType.Strength] * weapon.strengthScaling
                             + stats[StatType.Dexterity] * weapon.dexterityScaling
                             + stats[StatType.Intelligence] * weapon.intelligenceScaling) / 100f;
            return atk.baseDamage * (1f + scaling) * (1f + stats[StatType.PhysicalDamage] / 100f)
                   * mainHand.WeaponLevelFactor * weaponEffectiveness * spellbladeFactor;
        }

        /// <summary>Obrażenia ataku drugiej broni (walka dwiema brońmi).</summary>
        public float OffWeaponDamage(AttackDefinition atk)
        {
            if (offWeapon == null) return 0f;
            var w = offWeapon.definition.weapon;
            float scaling = (stats[StatType.Strength] * w.strengthScaling + stats[StatType.Dexterity] * w.dexterityScaling
                             + stats[StatType.Intelligence] * w.intelligenceScaling) / 100f;
            return atk.baseDamage * (1f + scaling) * (1f + stats[StatType.PhysicalDamage] / 100f) * offWeapon.WeaponLevelFactor * spellbladeFactor;
        }

        public float PhysicalReduction(BalanceConfig b) => StatCalculator.DefenseToReduction(stats[StatType.PhysicalDefense], b);
        public float MagicReduction(BalanceConfig b) => StatCalculator.DefenseToReduction(stats[StatType.MagicDefense], b);
    }

    public static class BuildCalculator
    {
        public static BuildSnapshot Compute(RunState run, GameConfig cfg)
        {
            var b = cfg.balance;
            var snap = new BuildSnapshot();
            snap.stats = StatCalculator.Compute(run.attributes, run.AllModifiers(), b);
            snap.effects = new EffectTotals();
            foreach (var e in run.AllEffects()) snap.effects.Add(e);

            // Broń: główna ręka lub pięści (zawsze istnieje atak bez many).
            var main = run.equipment.Get(EquipSlot.MainHand);
            snap.mainHand = main ?? new ItemInstance(cfg.unarmed);
            snap.weapon = snap.mainHand.definition.weapon;
            snap.weaponEffectiveness = StatCalculator.RequirementsMet(snap.mainHand.definition.requirements, snap.stats)
                ? 1f : b.unmetRequirementEffectiveness;

            // Garda: tarcza ma pierwszeństwo, w przeciwnym razie broń (jeśli potrafi blokować).
            var off = run.equipment.Get(EquipSlot.OffHand);
            if (off != null && off.definition.IsShield)
            {
                snap.guard = off.definition.shield.guard;
                snap.guardIsShield = true;
            }
            else if (snap.weapon.canBlock)
            {
                snap.guard = snap.weapon.guard;
            }

            // Parowanie: z tarczy, a gdy tarcza nie paruje lub jej brak – z broni.
            if (off != null && off.definition.IsShield && off.definition.shield.canParry) snap.parry = off.definition.shield.parry;
            else if (snap.weapon.canParry) snap.parry = snap.weapon.parry;

            if (snap.parry != null && snap.stats[StatType.ParryWindow] != 0f)
            {
                var p = new ParryData
                {
                    startup = snap.parry.startup, recovery = snap.parry.recovery, staminaCost = snap.parry.staminaCost,
                    poiseDamage = snap.parry.poiseDamage, angle = snap.parry.angle,
                    activeWindow = Mathf.Max(0.05f, snap.parry.activeWindow + snap.stats[StatType.ParryWindow])
                };
                snap.parry = p;
            }

            // Unik zależny od obciążenia.
            float capacity = Mathf.Max(1f, snap.stats[StatType.EquipLoad]);
            snap.loadRatio = run.equipment.TotalWeight / capacity;
            bool heavy = snap.loadRatio > b.heavyLoadThreshold;
            // Zręczność: dłuższy unik, uchylenie i szybsze ataki – z limitami („w granicach rozsądku”).
            float dex = snap.stats[StatType.Dexterity];
            float dexOver = Mathf.Max(0f, dex - 10f);
            snap.evasionChance = Mathf.Clamp((dex - b.evasionFromDexterity) * b.evasionPerDexterity, 0f, b.maxEvasion) / 100f;
            snap.attackSpeed = (1f + Mathf.Min(b.maxAttackSpeedBonus, dexOver * b.attackSpeedPerDexterity) + snap.stats[StatType.AttackSpeed] / 100f)
                               * (snap.weaponEffectiveness < 1f ? b.unmetRequirementAttackSpeed : 1f);
            // Druga broń jednoręczna w lewej ręce: ciosy na zmianę; cecha Dwie klingi – szybciej.
            if (off != null && off.definition.IsWeapon) snap.offWeapon = off;
            var cls = run.startingClass;
            if (cls != null && cls.trait == ClassTrait.TwinBlades && snap.offWeapon != null) snap.attackSpeed *= 1.2f;
            if (cls != null && cls.trait == ClassTrait.Spellblade) snap.spellbladeFactor = 1f + snap.stats[StatType.SpellPower] / 100f;
            float dodgeBonus = Mathf.Min(b.maxDodgeDistanceBonus, dexOver * b.dodgeDistancePerDexterity);
            snap.dodge = new DodgeParams
            {
                staminaCost = b.dodgeStaminaCost * (heavy ? b.heavyDodgeCostMult : 1f),
                distance = b.dodgeDistance * (heavy ? b.heavyDodgeDistanceMult : 1f) * (1f + dodgeBonus),
                invulnStart = b.dodgeInvulnStart,
                invulnDuration = b.dodgeInvulnDuration * (heavy ? b.heavyDodgeInvulnMult : 1f),
                totalDuration = b.dodgeTotalDuration,
                rollDuration = Mathf.Clamp(b.dodgeRollDuration, 0.2f, b.dodgeTotalDuration),
            };
            // Szarża barkiem (Hell Unleashed) jest krótsza od przewrotu.
            if (run.startingClass != null && run.startingClass.dodgeStyle == DodgeStyle.ShoulderCharge) snap.dodge.distance *= 0.805f;

            var tags = run.CurrentTags(snap);
            for (int i = 0; i < run.skillSlots.Length; i++)
            {
                var s = run.skillSlots[i];
                if (s == null) continue;
                var rt = ComputeSpell(s, snap.stats, b);
                rt.equipmentMet = (s.definition.requiredTags & tags) == s.definition.requiredTags;
                snap.skills[i] = rt;
            }
            if (run.signatureSkill != null)
            {
                snap.signature = ComputeSpell(run.signatureSkill, snap.stats, b);
                snap.signature.equipmentMet = (run.signatureSkill.definition.requiredTags & tags) == run.signatureSkill.definition.requiredTags;
            }
            return snap;
        }

        /// <summary>Rodzaje umiejętności z Hell Unleashed – zawsze z własnymi obrażeniami.</summary>
        public static bool IsHeroKind(SpellKind k) => k == SpellKind.Aura || k == SpellKind.Summon || k == SpellKind.Wave
                                                      || k == SpellKind.Dash || k == SpellKind.Overload || k == SpellKind.Undying;

        public static SpellRuntime ComputeSpell(SpellInstance s, StatSheet stats, BalanceConfig b)
        {
            var def = s.definition;
            bool met = StatCalculator.RequirementsMet(def.requirements, stats);
            float power = def.amount
                          * (1f + stats[StatType.Intelligence] * def.intelligenceScaling / 100f)
                          * (1f + stats[StatType.SpellPower] / 100f)
                          * (1f + def.powerPerLevel * s.level)
                          * (met ? 1f : b.unmetRequirementEffectiveness);
            if (def.kind == SpellKind.Projectile || def.kind == SpellKind.Nova)
                power = def.attack.baseDamage
                        * (1f + stats[StatType.Intelligence] * def.intelligenceScaling / 100f)
                        * (1f + stats[StatType.SpellPower] / 100f)
                        * (1f + def.powerPerLevel * s.level)
                        * (met ? 1f : b.unmetRequirementEffectiveness);
            float unmet = met ? 1f : b.unmetRequirementEffectiveness;
            if (def.flatDamage || IsHeroKind(def.kind))
            {
                // Skille z Hell Unleashed: własne obrażenia (attack.baseDamage) – czar skaluje z Inteligencją i mocą czarów,
                // technika z premią do obrażeń fizycznych.
                float scale = def.IsSpell
                    ? (1f + stats[StatType.Intelligence] * def.intelligenceScaling / 100f) * (1f + stats[StatType.SpellPower] / 100f)
                    : 1f + stats[StatType.PhysicalDamage] / 100f;
                return new SpellRuntime
                {
                    instance = s,
                    manaCost = def.IsSpell ? def.manaCost * def.CostFactor(s.level) : 0f,
                    staminaCost = def.staminaCost * (def.IsSpell ? 1f : def.CostFactor(s.level)),
                    power = def.attack.baseDamage * scale * (1f + def.powerPerLevel * s.level) * unmet,
                    requirementsMet = met,
                };
            }
            if (!def.IsSpell)
            {
                // Technika: mnożnik obrażeń broni (same obrażenia broni liczy PlayerCombat w chwili trafienia).
                return new SpellRuntime
                {
                    instance = s,
                    manaCost = 0f,
                    staminaCost = def.staminaCost * def.CostFactor(s.level),
                    power = def.weaponMultiplier * (1f + def.powerPerLevel * s.level) * unmet,
                    requirementsMet = met,
                };
            }
            return new SpellRuntime
            {
                instance = s,
                manaCost = def.manaCost * def.CostFactor(s.level),
                staminaCost = def.staminaCost,
                power = power,
                requirementsMet = met,
            };
        }
    }
}
