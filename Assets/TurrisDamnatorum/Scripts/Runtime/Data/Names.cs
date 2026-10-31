namespace Turris
{
    /// <summary>Polskie etykiety do interfejsu.</summary>
    public static class Names
    {
        /// <summary>Poziom pieczęci zapisany rzymsko (I–III).</summary>
        public static string Roman(int n) => n == 1 ? "I" : n == 2 ? "II" : n == 3 ? "III" : n.ToString();

        public static string Stat(StatType s)
        {
            switch (s)
            {
                case StatType.Toughness: return "Wytrzymałość";
                case StatType.LegacyEndurance: return "Wytrzymałość";   // wycofana Kondycja – liczona jako Wytrzymałość
                case StatType.LegacyMind: return "Inteligencja";        // wycofany Umysł – liczony jako Inteligencja
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
                case StatType.EarthResist: return "Odporność na ziemię (%)";
                case StatType.EarthDamage: return "Obrażenia od ziemi";
                case StatType.GlobalDamage: return "Wszystkie obrażenia";
                case StatType.DamageReduction: return "Redukcja obrażeń (%)";
                case StatType.AttackSpeed: return "Szybkość ataku";
                case StatType.CooldownReduction: return "Skrócenie odnowień";
                case StatType.LifeLeech: return "Kradzież życia (%)";
                case StatType.ExecuteDamage: return "Obrażenia egzekucji";
            }
            return s.ToString();
        }

        public static string Attribute(AttributeType a) => Stat((StatType)(int)a);

        /// <summary>Krótki opis działania cechy (panel rozwoju).</summary>
        public static string AttributeHelp(AttributeType a)
        {
            switch (a)
            {
                case AttributeType.Strength: return "cięższe bronie, obrażenia fizyczne";
                case AttributeType.Dexterity: return "uchylenie, unik, szybkość ataku";
                case AttributeType.Intelligence: return "mana, moc czarów, mocniejsze czary";
                default: return "życie, udźwig, wytrzymałość";
            }
        }

        public static readonly AttributeType[] Attributes = { AttributeType.Strength, AttributeType.Dexterity, AttributeType.Intelligence, AttributeType.Toughness };

        /// <summary>Opis wymaganego wyposażenia umiejętności, np. "wymaga tarczy".</summary>
        public static string Requirement(BuildTag tags)
        {
            var parts = new System.Collections.Generic.List<string>();
            if ((tags & BuildTag.TwoHanded) != 0) parts.Add((tags & BuildTag.Shield) != 0 ? "broni dwuręcznej albo tarczy" : "broni dwuręcznej");
            else if ((tags & BuildTag.Shield) != 0) parts.Add("tarczy");
            if ((tags & BuildTag.Guard) != 0) parts.Add("gardy (tarczy lub broni blokującej)");
            if ((tags & BuildTag.Parry) != 0) parts.Add("możliwości parowania");
            if ((tags & BuildTag.Magic) != 0) parts.Add("znajomości czarów");
            if ((tags & BuildTag.Ranged) != 0) parts.Add("łuku");
            if ((tags & BuildTag.Blade) != 0) parts.Add("ostrza (miecz, szabla, sztylet)");
            if ((tags & BuildTag.HeavyWeapon) != 0) parts.Add("ciężkiej broni (topór, młot, buława, korbacz)");
            if ((tags & BuildTag.Dagger) != 0) parts.Add("sztyletu (lub szabli)");
            if ((tags & BuildTag.Focus) != 0) parts.Add("fokusu (różdżka, kostur, grymuar)");
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

        public static string Servant(ServantKind k)
        {
            switch (k)
            {
                case ServantKind.Imp: return "chochlik ognia";
                case ServantKind.Hound: return "widmowy ogar";
                default: return "kościany golem";
            }
        }

        public static string Trait(ClassTrait t)
        {
            switch (t)
            {
                case ClassTrait.Fury: return "Furia";
                case ClassTrait.ArcaneWard: return "Arkanowa osłona";
                case ClassTrait.Ambush: return "Zasadzka";
                case ClassTrait.Bloodlust: return "Żądza krwi";
                case ClassTrait.Devotion: return "Pobożność";
                case ClassTrait.TwinBlades: return "Dwie klingi";
                case ClassTrait.Spellblade: return "Klinga zaklęć";
                case ClassTrait.Hawkeye: return "Sokole oko";
                case ClassTrait.SoulBond: return "Więź dusz";
                default: return "—";
            }
        }

        public static string TraitHelp(ClassTrait t)
        {
            switch (t)
            {
                case ClassTrait.Fury: return "Ciosy i otrzymane trafienia ładują furię; pełna – szał na 6 s: +35% szybkości ataku, +15% redukcji obrażeń, bez drgnięć.";
                case ClassTrait.ArcaneWard: return "Osłona za 20% maks. życia pochłania ciosy przed życiem; odrasta po 4 s bez trafień.";
                case ClassTrait.Ambush: return "Cios w plecy +60%; pierwszy cios do 1,5 s po uniku +35%.";
                case ClassTrait.Bloodlust: return "Furia i szał jak u wojownika; im mniej życia, tym mocniejsze ciosy (do +60%).";
                case ClassTrait.Devotion: return "Dwie aury naraz: pierwsza o 20% silniejsza, druga na 75% siły.";
                case ClassTrait.TwinBlades: return "Broń w każdej ręce: ciosy na zmianę, +20% szybkości ataku.";
                case ClassTrait.Spellblade: return "Moc czarów działa na ciosy bronią; każdy trafiony wróg oddaje 2 many.";
                case ClassTrait.Hawkeye: return "Im dalej cel, tym mocniejszy pocisk (do +45% od 11 m).";
                case ClassTrait.SoulBond: return "Każdy żywy sługa: +3% redukcji obrażeń (do 15%).";
                default: return "";
            }
        }

        public static string Dodge(DodgeStyle d)
        {
            switch (d)
            {
                case DodgeStyle.ShoulderCharge: return "szarża barkiem (rani i zachwiewa)";
                case DodgeStyle.Blink: return "odskok widma (niewrażliwość, wybuch w miejscu startu)";
                case DodgeStyle.Vanish: return "przewrót w cień (ciosy chybiają, okno zasadzki)";
                default: return "przewrót";
            }
        }

        public static string Element(Element e)
        {
            switch (e)
            {
                case Turris.Element.Fire: return "ogień";
                case Turris.Element.Frost: return "mróz";
                case Turris.Element.Lightning: return "błyskawica";
                case Turris.Element.Earth: return "ziemia";
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
                case StatusKind.Stagger: return "zachwianie";
                case StatusKind.Stun: return "ogłuszenie";
                case StatusKind.Sunder: return "pęknięcie pancerza";
                case StatusKind.Soaked: return "przemoczenie";
                case StatusKind.Slow: return "spowolnienie";
                default: return "zamrożenie";
            }
        }

        public static StatType ElementDamageStat(Element e) => e == Turris.Element.Fire ? StatType.FireDamage : e == Turris.Element.Frost ? StatType.FrostDamage
            : e == Turris.Element.Earth ? StatType.EarthDamage : StatType.LightningDamage;
        public static StatType ElementResistStat(Element e) => e == Turris.Element.Fire ? StatType.FireResist : e == Turris.Element.Frost ? StatType.FrostResist
            : e == Turris.Element.Earth ? StatType.EarthResist : StatType.LightningResist;

        public static UnityEngine.Color ElementColor(Element e)
        {
            switch (e)
            {
                case Turris.Element.Fire: return new UnityEngine.Color(1f, 0.5f, 0.15f);
                case Turris.Element.Frost: return new UnityEngine.Color(0.55f, 0.85f, 1f);
                case Turris.Element.Lightning: return new UnityEngine.Color(0.85f, 0.85f, 1f);
                case Turris.Element.Earth: return new UnityEngine.Color(0.75f, 0.58f, 0.32f);
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
                case StatusKind.Stagger: return new UnityEngine.Color(0.85f, 0.68f, 0.38f);
                case StatusKind.Stun: return new UnityEngine.Color(1f, 0.85f, 0.3f);
                case StatusKind.Sunder: return new UnityEngine.Color(0.7f, 0.7f, 0.75f);
                case StatusKind.Soaked: return new UnityEngine.Color(0.25f, 0.5f, 0.95f);
                case StatusKind.Slow: return new UnityEngine.Color(0.6f, 0.55f, 0.75f);
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
                case PassiveEffectType.BleedingHitHeal: return $"Trafienie bronią krwawiącego celu: +{v:0} życia";
                case PassiveEffectType.CheatDeath: return $"Raz na piętro śmiertelny cios zostawia {v * 100:0}% życia";
                case PassiveEffectType.CorpseExplosion: return $"Zabity wróg wybucha ogniem za {v * 100:0}% swojego życia";
                case PassiveEffectType.EmberAura: return $"Wrogowie w promieniu 3 m płoną: {v:0} obrażeń na sekundę";
                case PassiveEffectType.Thorns: return $"Wróg trafiający wręcz dostaje {v:0} obrażeń";
                case PassiveEffectType.WeaponChainLightning: return $"Trafienie bronią przeskakuje na 2 wrogów za {v * 100:0}% obrażeń";
                case PassiveEffectType.FrostWard: return $"{v * 100:0}% szans: trafienie bohatera wychładza wrogów wokół";
                case PassiveEffectType.LowHealthDamage: return $"Poniżej 35% życia: +{v * 100:0}% obrażeń";
                case PassiveEffectType.SoulHarvest: return $"Zabicie wroga leczy {v * 100:0.#}% życia";
                case PassiveEffectType.ProjectilePierce: return $"Pociski broni przebijają {v:0} wrogów";
                case PassiveEffectType.AreaSize: return $"+{v:0}% obszaru umiejętności i wybuchów";
                case PassiveEffectType.Evasion: return $"+{v:0}% uchylenia";
                case PassiveEffectType.PassiveBlockChance: return $"+{v:0}% szansy bloku biernego z tarczą";
                case PassiveEffectType.BlockCostReduction: return $"Blok kosztuje {UnityEngine.Mathf.Min(100f, v):0}% mniej wytrzymałości";
                case PassiveEffectType.MeleeKnockback: return $"Zwykły cios wręcz odrzuca o {v:0.#} m";
                case PassiveEffectType.MeleeRange: return $"+{v:0}% zasięgu zwykłego ciosu wręcz";
                case PassiveEffectType.MomentumAttackSpeed: return $"Rozpęd: każdy celny cios wręcz +{v:0}% szybkości ataku (do 5 warstw, 1,5 s)";
                case PassiveEffectType.PenetrationPhysical: return $"Przebicie odporności fizycznej: {v:0}%";
                case PassiveEffectType.PenetrationFire: return $"Przebicie odporności na ogień: {v:0}%";
                case PassiveEffectType.PenetrationFrost: return $"Przebicie odporności na mróz: {v:0}%";
                case PassiveEffectType.PenetrationLightning: return $"Przebicie odporności na błyskawice: {v:0}%";
                case PassiveEffectType.PenetrationEarth: return $"Przebicie odporności na ziemię: {v:0}%";
                case PassiveEffectType.ProjectileRicochet: return $"Pociski broni odbijają się do {v:0} wrogów";
                case PassiveEffectType.ProjectileVolley: return $"Strzał broni: {PerkMechanics.VolleyCount(v)} pocisków (boczne za 40%)";
                case PassiveEffectType.ProjectileSplit: return "Pocisk broni pęka przy trafieniu na 2 drzazgi (40%)";
                case PassiveEffectType.ProjectileExplosion: return $"Pociski broni wybuchają za {PerkMechanics.ExplosionShare(v) * 100:0}% obrażeń";
                case PassiveEffectType.ProjectileResidue: return "Wybuch pocisku broni zostawia strefę na 2 s";
                case PassiveEffectType.ParryReflect: return $"Sparowany pocisk wraca do wroga za {v * 100:0}% obrażeń";
                case PassiveEffectType.SunderOnHit: return "Ciosy wręcz nakładają pęknięcie pancerza";
                case PassiveEffectType.BleedOnHit: return "Zwykłe ciosy wręcz nakładają krwawienie";
                case PassiveEffectType.BurnOnHit: return "Zwykłe ataki bronią podpalają";
                case PassiveEffectType.ChillOnHit: return "Zwykłe ataki bronią wychładzają";
                case PassiveEffectType.SoakedOnHit: return "Zwykłe ataki bronią przemaczają";
                case PassiveEffectType.SoulGain: return $"+{v:0.#}% dusz za zabójstwo";
                case PassiveEffectType.ManaOnKill: return $"Zabicie wroga: +{v:0.#} many";
                case PassiveEffectType.ProjectileSpeed: return $"+{v:0.#}% prędkości pocisków";
                case PassiveEffectType.Accuracy: return $"Celność: −{v:0.#} pkt uniku wrogów";
                case PassiveEffectType.StaggerOnHit: return "Zwykłe ataki bronią zachwiewają";
                case PassiveEffectType.ConvertFireToFrost: return $"{UnityEngine.Mathf.Min(1f, v) * 100:0}% ognia staje się mrozem";
                case PassiveEffectType.ConvertPhysicalToFire: return $"{UnityEngine.Mathf.Min(1f, v) * 100:0}% obrażeń fizycznych zwykłego ciosu wręcz staje się ogniem";
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
