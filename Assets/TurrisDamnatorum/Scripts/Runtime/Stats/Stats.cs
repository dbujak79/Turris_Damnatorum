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
        public int toughness, strength, dexterity, intelligence;

        public static AttributeBlock From(ClassDefinition c) => new AttributeBlock
        {
            toughness = c.toughness, strength = c.strength, dexterity = c.dexterity, intelligence = c.intelligence
        };

        public int Get(AttributeType a)
        {
            switch (a)
            {
                case AttributeType.Toughness: return toughness;
                case AttributeType.Strength: return strength;
                case AttributeType.Dexterity: return dexterity;
                default: return intelligence;
            }
        }

        /// <summary>Rozwój cechy o <paramref name="amount"/> (punkt po piętrze).</summary>
        public void Add(AttributeType a, int amount = 1)
        {
            switch (a)
            {
                case AttributeType.Toughness: toughness += amount; break;
                case AttributeType.Strength: strength += amount; break;
                case AttributeType.Dexterity: dexterity += amount; break;
                default: intelligence += amount; break;
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

            // Wycofane atrybuty ze starych assetów: Kondycja liczy się jako Wytrzymałość, Umysł jako Inteligencja.
            flat[(int)StatType.Toughness] += flat[(int)StatType.LegacyEndurance];
            pct[(int)StatType.Toughness] += pct[(int)StatType.LegacyEndurance];
            flat[(int)StatType.Intelligence] += flat[(int)StatType.LegacyMind];
            pct[(int)StatType.Intelligence] += pct[(int)StatType.LegacyMind];

            sheet[StatType.Toughness] = Final(StatType.Toughness, attributes.toughness);
            sheet[StatType.Strength] = Final(StatType.Strength, attributes.strength);
            sheet[StatType.Dexterity] = Final(StatType.Dexterity, attributes.dexterity);
            sheet[StatType.Intelligence] = Final(StatType.Intelligence, attributes.intelligence);

            // Wytrzymałość: życie, pula wytrzymałości i udźwig; Inteligencja: mana.
            sheet[StatType.MaxHealth] = Final(StatType.MaxHealth, b.baseHealth + b.healthPerToughness * sheet[StatType.Toughness]);
            sheet[StatType.MaxStamina] = Final(StatType.MaxStamina, b.baseStamina + b.staminaPerToughness * sheet[StatType.Toughness]);
            sheet[StatType.MaxMana] = Final(StatType.MaxMana, b.baseMana + b.manaPerIntelligence * sheet[StatType.Intelligence]);
            sheet[StatType.StaminaRegen] = Final(StatType.StaminaRegen, b.baseStaminaRegen);
            sheet[StatType.EquipLoad] = Final(StatType.EquipLoad, b.baseEquipLoad + b.equipLoadPerToughness * sheet[StatType.Toughness]);

            StatType[] plain = { StatType.HealthRegen, StatType.ManaRegen, StatType.PhysicalDefense, StatType.MagicDefense, StatType.ParryWindow };
            foreach (var s in plain) sheet[s] = Final(s, 0f);

            // Statystyki, których wartość sama jest premią procentową: modyfikatory płaskie i procentowe sumują się
            // (np. "+15% mocy czarów" z kostura i "+12%" ze wzmocnienia = 27%).
            StatType[] percentStats = { StatType.PhysicalDamage, StatType.SpellPower, StatType.FlaskPotency, StatType.RiposteDamage, StatType.MoveSpeed,
                StatType.FireResist, StatType.FrostResist, StatType.LightningResist, StatType.FireDamage, StatType.FrostDamage, StatType.LightningDamage, StatType.BleedDamage };
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
