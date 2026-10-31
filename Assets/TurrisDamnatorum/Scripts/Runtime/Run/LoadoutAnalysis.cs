using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Analiza buildu do sklepu (HU <c>LoadoutAnalysis</c>): siła zwykłego ataku i skilli w slotach (obrażenia na trafienie, tempo, DPS)
    /// oraz perki HU, które zasną po założeniu oferowanej broni.
    /// </summary>
    public static class LoadoutAnalysis
    {
        public struct Line
        {
            public string name;
            public float perHit, perSecond, dps;   // perSecond: trafienia (atak) albo rzuty (skill) na sekundę
        }

        /// <summary>Zwykły atak: lekkie ciosy serii z tempem z buildu (model czasu serii jak w katalogu broni HU).</summary>
        public static Line MainAttack(BuildSnapshot b)
        {
            var w = b.weapon;
            if (w?.light == null) return new Line { name = "Zwykły atak" };
            int combo = Mathf.Max(1, w.lightComboLength);
            float hits = combo / Mathf.Max(0.05f, HellWeaponCatalog.SeriesTime(w.light, combo)) * Mathf.Max(0.05f, b.attackSpeed);
            float perHit = b.WeaponDamage(w.light) * Mathf.Max(1, w.light.projectileCount);
            return new Line { name = b.mainHand?.Name ?? "Zwykły atak", perHit = perHit, perSecond = hits, dps = perHit * hits };
        }

        /// <summary>Skill w slocie: obrażenia jednego rzutu (bez pieczęci) i odnowienie z premią do odnowień.</summary>
        public static Line Skill(BuildSnapshot b, SpellRuntime rt)
        {
            var def = rt.Def;
            int level = rt.instance.level;
            float perHit, cd;
            var hu = HuCatalog.ForTarget(def.id);
            if (hu != null && hu.levels.Length > 0)
            {
                var p = hu.At(level + 1);
                perHit = (float)p.TotalDamage * PlayerCombat.HuDamageScale * rt.huScale;
                cd = (float)p.Get("cooldown", def.CooldownAt(level));
            }
            else
            {
                perHit = def.IsSpell ? rt.power : rt.power * (b.weapon?.light != null ? b.WeaponDamage(b.weapon.light) : 0f);
                cd = def.CooldownAt(level);
            }
            cd /= 1f + Mathf.Max(0f, b.stats[StatType.CooldownReduction]) / 100f;
            float rate = cd > 0.05f ? 1f / cd : 0f;
            return new Line { name = rt.instance.Name, perHit = perHit, perSecond = rate, dps = perHit * rate };
        }

        public static IEnumerable<Line> Skills(BuildSnapshot b) => b.skills.Where(s => s != null && s.instance != null).Select(s => Skill(b, s));

        public static string Format(Line l, bool attack) =>
            attack ? $"{l.name}: {l.perHit:0} na trafienie × {l.perSecond:0.0}/s = <b>{l.dps:0} DPS</b>"
                   : $"{l.name}: {l.perHit:0} na rzut, co {(l.perSecond > 0 ? 1f / l.perSecond : 0f):0.0} s = <b>{l.dps:0} DPS</b>";

        /// <summary>Perk nic nie da w tym buildzie (HU <c>Check: NoEffect</c>): same premie czarów, a w slotach nie ma czaru.</summary>
        public static bool PerkUseless(BoonDefinition perk, BuildSnapshot b)
        {
            if (perk == null || perk.modifiers.Count == 0 || perk.effects.Count > 0) return false;
            bool spellOnly = perk.modifiers.All(m => m.stat == StatType.SpellPower || m.stat == StatType.MaxMana || m.stat == StatType.ManaRegen);
            bool hasSpell = b.skills.Any(s => s != null && s.instance != null && s.Def.IsSpell) || (b.signature?.instance != null && b.signature.Def.IsSpell);
            return spellOnly && !hasSpell;
        }

        /// <summary>Posiadane perki HU, które działają teraz, a zasną po założeniu tej broni (HU <c>UpgradesLosingEffect</c>).</summary>
        public static List<BoonDefinition> PerksLosingEffect(RunState run, ItemDefinition weapon)
        {
            var result = new List<BoonDefinition>();
            if (weapon == null || !weapon.IsWeapon) return result;
            var off = weapon.twoHanded ? null : run.equipment.Get(EquipSlot.OffHand)?.definition;
            foreach (var b in run.boons)
            {
                if (b.definition == null || b.stacks <= 0 || !b.definition.id.StartsWith("hu_perk_")) continue;
                var row = FloorShop.PerkRow(b.definition);
                if (row != null && PerkMechanics.Active(row, run) && !PerkMechanics.ActiveWith(row, run, weapon, off)) result.Add(b.definition);
            }
            return result;
        }
    }
}
