using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Affiksy specjalizacyjne gracza (2026-11-01): rzeczywiste dodane żywioły broni, postawa ciężkiej broni, szybkość rzucania
    /// i migawka obrażeń sług. Matematyka w <see cref="AffixMath"/>; tu tylko miejsca w ścieżkach trafień i akcji.
    /// </summary>
    public partial class PlayerCombat
    {
        /// <summary>Mnożnik premii fizycznej zawarty w obrażeniach broni (<see cref="BuildSnapshot.WeaponDamage"/>).</summary>
        public float PhysicalFactor => Mathf.Max(0.01f, 1f + Build.stats[StatType.PhysicalDamage] / 100f);

        /// <summary>
        /// Dodane żywioły broni jako osobne składniki magiczne: dodatek[e] = B × suma[e] / 100. <paramref name="weaponBase"/> to B –
        /// obrażenia tego ataku (z mnożnikiem ciosu/techniki, poziomem broni, wymaganiami) bez premii fizycznej. Wołane raz, przy
        /// tworzeniu trafienia ataku bronią, przed ruchem (mnożnik ciosu ładowanego wchodzi raz) i przed <c>PrepareOutgoing</c>
        /// (konwersje i premie żywiołu działają na dodatek raz; premia fizyczna już nie). Samo dodanie nie nakłada efektów.
        /// </summary>
        public void AddWeaponElements(ref HitData hit, float weaponBase)
        {
            if (Build == null || weaponBase <= 0f) return;
            var st = Build.stats;
            hit.AddMagic(Element.Fire, AffixMath.AddedElement(weaponBase, st[StatType.AddedWeaponFire]));
            hit.AddMagic(Element.Frost, AffixMath.AddedElement(weaponBase, st[StatType.AddedWeaponFrost]));
            hit.AddMagic(Element.Lightning, AffixMath.AddedElement(weaponBase, st[StatType.AddedWeaponLightning]));
            hit.AddMagic(Element.Earth, AffixMath.AddedElement(weaponBase, st[StatType.AddedWeaponEarth]));
        }

        /// <summary>Dodane żywioły riposty: składniki z premią danego żywiołu i przebiciem; odporność celu liczy <see cref="EnemyBrain.ReceiveRiposte"/>.</summary>
        HitData RiposteElements(float weaponBase)
        {
            var hit = new HitData { mods = StatusModifiers.None };
            AddWeaponElements(ref hit, weaponBase);
            if (hit.magic <= 0f) return hit;
            var st = Build.stats; var fx = Build.effects;
            hit.ScaleMagicByElement(e => e == Element.None ? 1f : 1f + st[Names.ElementDamageStat(e)] / 100f);
            hit.penFire = fx[PassiveEffectType.PenetrationFire] / 100f;
            hit.penFrost = fx[PassiveEffectType.PenetrationFrost] / 100f;
            hit.penLightning = fx[PassiveEffectType.PenetrationLightning] / 100f;
            hit.penEarth = fx[PassiveEffectType.PenetrationEarth] / 100f;
            return hit;
        }

        /// <summary>Ciężka broń wręcz w ręce głównej (rodzina z <see cref="WeaponFamilies"/>; łuk i kostur się nie kwalifikują).</summary>
        public bool HoldsHeavyMelee => Build != null && Build.mainHand != null && WeaponFamilies.IsHeavyMelee(Build.mainHand.definition);

        /// <summary>
        /// Premia postawy ciężkiej broni: mnoży <see cref="HitData.poiseDamage"/> ciosu wykonanego ciężką bronią (zwykły cios,
        /// technika wręcz). Bez riposty, efektów wtórnych cech/unikatów i źródeł z zerową postawą. Mnożniki ruchu zostają.
        /// </summary>
        public void ApplyHeavyPoise(ref HitData hit, bool offhandStrike)
        {
            if (offhandStrike || !HoldsHeavyMelee || hit.poiseDamage <= 0f || hit.isRiposte || hit.featureEffect || hit.uniqueEffect) return;
            float bonus = Build.stats[StatType.HeavyPoiseDamage];
            if (bonus > 0f) hit.poiseDamage *= AffixMath.PoiseMultiplier(bonus);
        }

        /// <summary>
        /// Efektywne przygotowanie ręcznie rzucanego czaru: tempo gracza, potem szybkość rzucania. Techniki, flaszki, parowanie,
        /// unik bez zmian (tylko <see cref="SpellDefinition.IsSpell"/>).
        /// </summary>
        public float EffectiveCastTime(SpellDefinition def)
        {
            float t = ScaleStartup(def.castTime);
            return def.IsSpell && Build != null ? AffixMath.CastTime(t, Build.stats[StatType.CastSpeed]) : t;
        }

        /// <summary>Mnożnik obrażeń sługi przyzwanego teraz (migawka – zmiana sprzętu działa od następnego przyzwania).</summary>
        public float MinionSnapshotMultiplier => Build != null ? AffixMath.MinionMultiplier(Build.stats[StatType.MinionDamage]) : 1f;
    }
}
