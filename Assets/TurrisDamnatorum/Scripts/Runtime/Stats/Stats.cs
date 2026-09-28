using System;
using System.Collections.Generic;

namespace Turris
{
    /// <summary>Niezmienny po obliczeniu zestaw wartości statystyk.</summary>
    public class StatSheet
    {
        static readonly int Count = Enum.GetValues(typeof(StatType)).Length;
        readonly float[] values = new float[Count];

        public float this[StatType s]
        {
            get => values[(int)s];
            internal set => values[(int)s] = value;
        }

        public float Get(StatType s) => values[(int)s];
    }

    public class EffectTotals
    {
        static readonly int Count = Enum.GetValues(typeof(PassiveEffectType)).Length;
        readonly float[] values = new float[Count];
        public float this[PassiveEffectType t] => values[(int)t];
        public void Add(PassiveEffect e) { if (e.type != PassiveEffectType.None) values[(int)e.type] += e.value; }
    }

    [Serializable]
    public struct AttributeBlock
    {
        public int vigor, endurance, mind, strength, dexterity, intelligence;

        public static AttributeBlock From(ClassDefinition c) => new AttributeBlock
        {
            vigor = c.vigor, endurance = c.endurance, mind = c.mind,
            strength = c.strength, dexterity = c.dexterity, intelligence = c.intelligence
        };

        public int Get(AttributeType a)
        {
            switch (a)
            {
                case AttributeType.Vigor: return vigor;
                case AttributeType.Endurance: return endurance;
                case AttributeType.Mind: return mind;
                case AttributeType.Strength: return strength;
                case AttributeType.Dexterity: return dexterity;
                default: return intelligence;
            }
        }
    }

    /// <summary>
    /// Statystyki są zawsze liczone od zera z aktualnych źródeł (atrybuty bazowe + założone przedmioty + wzmocnienia).
    /// Dzięki temu zdjęcie przedmiotu nie może zostawić "osieroconej" premii.
    /// Kolejność: atrybuty (baza + płaskie) × (1 + %), potem statystyki pochodne z atrybutów + płaskie, × (1 + %).
    /// </summary>
    public static class StatCalculator
    {
        static readonly int Count = Enum.GetValues(typeof(StatType)).Length;

        public static StatSheet Compute(AttributeBlock attributes, IEnumerable<StatModifier> modifiers, BalanceConfig b)
        {
            var flat = new float[Count];
            var pct = new float[Count];
            foreach (var m in modifiers)
            {
                if (m.mode == ModifierMode.Flat) flat[(int)m.stat] += m.value;
                else pct[(int)m.stat] += m.value;
            }

            var sheet = new StatSheet();
            float Final(StatType s, float baseValue) => (baseValue + flat[(int)s]) * (1f + pct[(int)s] / 100f);

            sheet[StatType.Vigor] = Final(StatType.Vigor, attributes.vigor);
            sheet[StatType.Endurance] = Final(StatType.Endurance, attributes.endurance);
            sheet[StatType.Mind] = Final(StatType.Mind, attributes.mind);
            sheet[StatType.Strength] = Final(StatType.Strength, attributes.strength);
            sheet[StatType.Dexterity] = Final(StatType.Dexterity, attributes.dexterity);
            sheet[StatType.Intelligence] = Final(StatType.Intelligence, attributes.intelligence);

            sheet[StatType.MaxHealth] = Final(StatType.MaxHealth, b.baseHealth + b.healthPerVigor * sheet[StatType.Vigor]);
            sheet[StatType.MaxStamina] = Final(StatType.MaxStamina, b.baseStamina + b.staminaPerEndurance * sheet[StatType.Endurance]);
            sheet[StatType.MaxMana] = Final(StatType.MaxMana, b.baseMana + b.manaPerMind * sheet[StatType.Mind]);
            sheet[StatType.StaminaRegen] = Final(StatType.StaminaRegen, b.baseStaminaRegen);
            sheet[StatType.EquipLoad] = Final(StatType.EquipLoad, b.baseEquipLoad + b.equipLoadPerEndurance * sheet[StatType.Endurance]);

            StatType[] plain = { StatType.HealthRegen, StatType.ManaRegen, StatType.PhysicalDefense, StatType.MagicDefense, StatType.ParryWindow };
            foreach (var s in plain) sheet[s] = Final(s, 0f);

            // Statystyki, których wartość sama jest premią procentową: modyfikatory płaskie i procentowe sumują się
            // (np. "+15% mocy czarów" z kostura i "+12%" ze wzmocnienia = 27%).
            StatType[] percentStats = { StatType.PhysicalDamage, StatType.SpellPower, StatType.FlaskPotency, StatType.RiposteDamage, StatType.MoveSpeed };
            foreach (var s in percentStats) sheet[s] = flat[(int)s] + pct[(int)s];
            return sheet;
        }

        public static float DefenseToReduction(float defense, BalanceConfig b)
            => defense <= 0 ? 0f : defense / (defense + b.defenseConstant);

        public static bool RequirementsMet(IList<AttributeRequirement> reqs, StatSheet stats)
        {
            if (reqs == null) return true;
            foreach (var r in reqs)
                if (stats[(StatType)(int)r.attribute] + 0.001f < r.value) return false;
            return true;
        }
    }
}
