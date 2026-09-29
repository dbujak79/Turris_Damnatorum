namespace Turris
{
    /// <summary>Polskie etykiety do interfejsu.</summary>
    public static class Names
    {
        public static string Stat(StatType s)
        {
            switch (s)
            {
                case StatType.Vigor: return "Witalność";
                case StatType.Endurance: return "Kondycja";
                case StatType.Mind: return "Umysł";
                case StatType.Strength: return "Siła";
                case StatType.Dexterity: return "Zręczność";
                case StatType.Intelligence: return "Inteligencja";
                case StatType.MaxHealth: return "Maks. życie";
                case StatType.MaxStamina: return "Maks. wytrzymałość";
                case StatType.MaxMana: return "Maks. mana";
                case StatType.HealthRegen: return "Regeneracja życia/s";
                case StatType.StaminaRegen: return "Regeneracja wytrzymałości/s";
                case StatType.ManaRegen: return "Regeneracja many/s";
                case StatType.PhysicalDefense: return "Obrona fizyczna";
                case StatType.MagicDefense: return "Obrona magiczna";
                case StatType.PhysicalDamage: return "Obrażenia broni";
                case StatType.SpellPower: return "Moc czarów";
                case StatType.FlaskPotency: return "Siła flaszek";
                case StatType.RiposteDamage: return "Obrażenia riposty";
                case StatType.ParryWindow: return "Okno parowania (s)";
                case StatType.EquipLoad: return "Udźwig";
                case StatType.MoveSpeed: return "Szybkość ruchu";
            }
            return s.ToString();
        }

        public static string Attribute(AttributeType a) => Stat((StatType)(int)a);

        /// <summary>Opis wymaganego wyposażenia umiejętności, np. "wymaga tarczy".</summary>
        public static string Requirement(BuildTag tags)
        {
            var parts = new System.Collections.Generic.List<string>();
            if ((tags & BuildTag.Shield) != 0) parts.Add("tarczy");
            if ((tags & BuildTag.Guard) != 0) parts.Add("gardy (tarczy lub broni blokującej)");
            if ((tags & BuildTag.Parry) != 0) parts.Add("możliwości parowania");
            if ((tags & BuildTag.Magic) != 0) parts.Add("znajomości czarów");
            return parts.Count == 0 ? "" : "wymaga " + string.Join(", ", parts);
        }

        public static string SkillCategory(SkillCategory c) => c == Turris.SkillCategory.Spell ? "czar" : "technika";

        public static string Slot(EquipSlot s)
        {
            switch (s)
            {
                case EquipSlot.MainHand: return "Główna ręka";
                case EquipSlot.OffHand: return "Druga ręka";
                case EquipSlot.Head: return "Głowa";
                case EquipSlot.Body: return "Korpus";
                case EquipSlot.Hands: return "Rękawice";
                case EquipSlot.Belt: return "Pas";
                case EquipSlot.Feet: return "Buty";
                case EquipSlot.Ring1: return "Pierścień 1";
                case EquipSlot.Ring2: return "Pierścień 2";
                default: return "Amulet";
            }
        }

        public static string Effect(PassiveEffectType t, float v)
        {
            switch (t)
            {
                case PassiveEffectType.ParryRestoreStamina: return $"Udane parowanie: +{v:0} wytrzymałości";
                case PassiveEffectType.ParryRestoreMana: return $"Udane parowanie: +{v:0} many";
                case PassiveEffectType.ParryHeal: return $"Udane parowanie: +{v:0} życia";
                case PassiveEffectType.ManaOnMeleeHit: return $"Trafienie bronią: +{v:0.#} many";
                case PassiveEffectType.HealOnKill: return $"Zabicie wroga: +{v:0} życia";
                case PassiveEffectType.RiposteHeal: return $"Riposta: +{v:0} życia";
                case PassiveEffectType.BlockManaGain: return $"Zablokowany cios: +{v:0.#} many";
                case PassiveEffectType.SpellStaminaRefund: return $"Rzucenie czaru: +{v:0} wytrzymałości";
            }
            return t.ToString();
        }

        public static string Telegraph(TelegraphKind k)
        {
            switch (k)
            {
                case TelegraphKind.Heavy: return "CIĘŻKI";
                case TelegraphKind.Unparryable: return "NIE DO SPAROWANIA";
                case TelegraphKind.Unblockable: return "NIE DO ZABLOKOWANIA";
                case TelegraphKind.NoDodge: return "UCIEKAJ Z ZASIĘGU";
                default: return "";
            }
        }

        public static string Damage(DamageType d) => d == DamageType.Physical ? "fizyczne" : "magiczne";
    }
}
