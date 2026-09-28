using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    public struct DodgeParams
    {
        public float staminaCost, distance, invulnStart, invulnDuration, totalDuration;
    }

    public class SpellRuntime
    {
        public SpellInstance instance;
        public float manaCost;
        public float power;            // obrażenia / leczenie / premia
        public bool requirementsMet;
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

        public readonly List<SpellRuntime> spells = new List<SpellRuntime>();

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
                   * mainHand.WeaponLevelFactor * weaponEffectiveness;
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
            snap.dodge = new DodgeParams
            {
                staminaCost = b.dodgeStaminaCost * (heavy ? b.heavyDodgeCostMult : 1f),
                distance = b.dodgeDistance * (heavy ? b.heavyDodgeDistanceMult : 1f),
                invulnStart = b.dodgeInvulnStart,
                invulnDuration = b.dodgeInvulnDuration * (heavy ? b.heavyDodgeInvulnMult : 1f),
                totalDuration = b.dodgeTotalDuration,
            };

            foreach (var s in run.attunedSpells) snap.spells.Add(ComputeSpell(s, snap.stats, b));
            return snap;
        }

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
            return new SpellRuntime
            {
                instance = s,
                manaCost = def.manaCost * Mathf.Max(0.4f, 1f - def.costReductionPerLevel * s.level),
                power = power,
                requirementsMet = met,
            };
        }
    }
}
