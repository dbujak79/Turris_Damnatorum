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
                case StatType.FireResist: return "Odporność na ogień (%)";
                case StatType.FrostResist: return "Odporność na mróz (%)";
                case StatType.LightningResist: return "Odporność na błyskawice (%)";
                case StatType.FireDamage: return "Obrażenia od ognia";
                case StatType.FrostDamage: return "Obrażenia od mrozu";
                case StatType.LightningDamage: return "Obrażenia od błyskawic";
                case StatType.BleedDamage: return "Obrażenia krwawienia";
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

        public static string LevelFeature(LevelFeature f)
        {
            switch (f.kind)
            {
                case LevelFeatureKind.ExtraTargets: return $"+{f.value:0} pocisk/cel";
                case LevelFeatureKind.RadiusBonus: return $"+{f.value * 100:0}% promienia i zasięgu";
                case LevelFeatureKind.DurationBonus: return $"+{f.value:0.#} s czasu trwania";
                case LevelFeatureKind.ExtraStatusStacks: return $"+{f.value:0} warstwa efektu";
                case LevelFeatureKind.LeaveZone: return $"wybuch zostawia płonącą ziemię ({f.value:0.#} s)";
                default: return $"końcowe cięcie za {f.value * 100:0}% mocy";
            }
        }

        public static string Element(Element e)
        {
            switch (e)
            {
                case Turris.Element.Fire: return "ogień";
                case Turris.Element.Frost: return "mróz";
                case Turris.Element.Lightning: return "błyskawica";
                default: return "";
            }
        }

        public static string Status(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.Bleed: return "krwawienie";
                case StatusKind.Burn: return "podpalenie";
                case StatusKind.Chill: return "chłód";
                case StatusKind.Shock: return "porażenie";
                default: return "zamrożenie";
            }
        }

        public static StatType ElementDamageStat(Element e) => e == Turris.Element.Fire ? StatType.FireDamage : e == Turris.Element.Frost ? StatType.FrostDamage : StatType.LightningDamage;
        public static StatType ElementResistStat(Element e) => e == Turris.Element.Fire ? StatType.FireResist : e == Turris.Element.Frost ? StatType.FrostResist : StatType.LightningResist;

        public static UnityEngine.Color ElementColor(Element e)
        {
            switch (e)
            {
                case Turris.Element.Fire: return new UnityEngine.Color(1f, 0.5f, 0.15f);
                case Turris.Element.Frost: return new UnityEngine.Color(0.55f, 0.85f, 1f);
                case Turris.Element.Lightning: return new UnityEngine.Color(0.85f, 0.85f, 1f);
                default: return UnityEngine.Color.white;
            }
        }

        public static UnityEngine.Color StatusColor(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.Bleed: return new UnityEngine.Color(0.9f, 0.15f, 0.15f);
                case StatusKind.Burn: return ElementColor(Turris.Element.Fire);
                case StatusKind.Chill: return ElementColor(Turris.Element.Frost);
                case StatusKind.Shock: return new UnityEngine.Color(0.95f, 0.95f, 0.45f);
                default: return new UnityEngine.Color(0.7f, 0.95f, 1f);
            }
        }

        /// <summary>Opis listy efektów ataku, np. "krwawienie ×2 (50%)".</summary>
        public static string Statuses(System.Collections.Generic.List<StatusApplication> list)
        {
            if (list == null || list.Count == 0) return "";
            var parts = new System.Collections.Generic.List<string>();
            foreach (var s in list)
                parts.Add(Status(s.kind) + (s.stacks > 1 ? $" ×{s.stacks}" : "") + (s.chance < 0.999f ? $" ({s.chance * 100:0}%)" : ""));
            return string.Join(", ", parts);
        }

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
                case PassiveEffectType.BurnDurationBonus: return $"Podpalenie trwa +{v:0.#} s dłużej";
                case PassiveEffectType.BleedMaxStacksBonus: return $"+{v:0} maks. warstw krwawienia";
                case PassiveEffectType.BleedMovingBonus: return $"Krwawienie w ruchu: ×{2 + v:0.#} zamiast ×2";
                case PassiveEffectType.FreezeStacksReduction: return $"Do zamrożenia potrzeba o {v:0} warstw chłodu mniej";
                case PassiveEffectType.ConductionBonus: return $"Przewodzenie: +{v:0}% obrażeń";
                case PassiveEffectType.ConductionJump: return "Przewodzenie przeskakuje na najbliższego wroga";
                case PassiveEffectType.DodgeShockCharge: return "Po uniku następny cios bronią poraża";
                case PassiveEffectType.BurnImmunity: return "Odporność na podpalenie";
                case PassiveEffectType.BurningDamageBonus: return $"Płonąc, zadajesz +{v:0}% obrażeń";
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
