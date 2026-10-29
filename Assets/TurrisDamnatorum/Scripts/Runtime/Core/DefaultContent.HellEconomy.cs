using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    public static partial class DefaultContent
    {
        /// <summary>Id przedmiotu Turris dla przedmiotu HU (plik źródłowy, np. body_heavy_t2 → hu_item_body_heavy_t2).</summary>
        public static string HuItemId(HellEconomy.ItemRow r) => "hu_item_" + r.file;

        /// <summary>Id wzmocnienia (perka) Turris dla perka HU (upgrade.damage.fire → hu_perk_damage_fire).</summary>
        public static string HuPerkId(HellEconomy.UpgradeRow r) => "hu_perk_" + r.id.Replace("upgrade.", "").Replace('.', '_');

        /// <summary>Perk HU ma odpowiednik w Turris: każda jego statystyka, konwersja i efekt trafienia (<see cref="PerkContent"/>).</summary>
        public static bool PerkSupported(HellEconomy.UpgradeRow r) => PerkContent(r, out _, out _);

        /// <summary>
        /// Statystyki i efekty perka HU na poziom (warstwę wzmocnienia). Statystyki z odpowiednikiem idą przez <see cref="HellEconomy.Map(string)"/>,
        /// mechaniki (pociski, przebicie, unik, blok bierny, odrzut, zasięg, rozpęd, odbicie, konwersje, efekty trafienia) – jako efekty pasywne.
        /// Fałsz, gdy czegokolwiek nie da się przenieść.
        /// </summary>
        public static bool PerkContent(HellEconomy.UpgradeRow r, out List<StatModifier> mods, out List<PassiveEffect> effects)
        {
            mods = new List<StatModifier>(); effects = new List<PassiveEffect>();
            foreach (var m in r.Modifiers)
            {
                var p = m.Split('|');
                if (p.Length < 3) return false;
                float v = float.Parse(p[2].Split(',')[0], System.Globalization.CultureInfo.InvariantCulture);
                PassiveEffectType fx;
                switch (p[0])
                {
                    case "AreaSize": fx = PassiveEffectType.AreaSize; v *= 100f; break;
                    case "Evasion": fx = PassiveEffectType.Evasion; v *= 100f; break;
                    case "PassiveBlockChance": fx = PassiveEffectType.PassiveBlockChance; v *= 100f; break;
                    case "BlockCostReduction": fx = PassiveEffectType.BlockCostReduction; v *= 100f; break;
                    case "Knockback": fx = PassiveEffectType.MeleeKnockback; break;
                    case "MeleeRange": fx = PassiveEffectType.MeleeRange; v *= 100f; break;
                    case "MomentumAttackSpeed": fx = PassiveEffectType.MomentumAttackSpeed; v *= 100f; break;
                    case "PhysicalPenetration": fx = PassiveEffectType.PenetrationPhysical; v *= 100f; break;
                    case "FirePenetration": fx = PassiveEffectType.PenetrationFire; v *= 100f; break;
                    case "WaterPenetration": fx = PassiveEffectType.PenetrationFrost; v *= 100f; break;
                    case "AirPenetration": fx = PassiveEffectType.PenetrationLightning; v *= 100f; break;
                    case "EarthPenetration": fx = PassiveEffectType.PenetrationEarth; v *= 100f; break;
                    case "ProjectilePierce": fx = PassiveEffectType.ProjectilePierce; break;
                    case "ProjectileRicochet": fx = PassiveEffectType.ProjectileRicochet; break;
                    case "ProjectileVolley": fx = PassiveEffectType.ProjectileVolley; v = 1f; break;      // tabela 2/3/5/8 wg poziomu
                    case "ProjectileSplit": fx = PassiveEffectType.ProjectileSplit; break;
                    case "ProjectileExplosionShare": fx = PassiveEffectType.ProjectileExplosion; v = 1f; break; // tabela 30/45/60%
                    case "ProjectileResidue": fx = PassiveEffectType.ProjectileResidue; break;
                    case "ParryReflect": fx = PassiveEffectType.ParryReflect; break;
                    case "BleedMaxStacks": fx = PassiveEffectType.BleedMaxStacksBonus; break;
                    default:
                        if (m.Contains(",")) return false;
                        var mm = HellEconomy.Map(m);
                        if (!mm.HasValue) return false;
                        mods.Add(mm.Value);
                        continue;
                }
                effects.Add(new PassiveEffect(fx, v));
            }
            if (!string.IsNullOrEmpty(r.Conversion))
            {
                var c = r.Conversion.Split('|');
                float ratio = c.Length > 2 ? float.Parse(c[2], System.Globalization.CultureInfo.InvariantCulture) : 1f;
                if (c[0] == "Fire" && c[1] == "Water") effects.Add(new PassiveEffect(PassiveEffectType.ConvertFireToFrost, ratio));
                else if (c[0] == "Physical" && c[1] == "Fire") effects.Add(new PassiveEffect(PassiveEffectType.ConvertPhysicalToFire, ratio));
                else return false;
            }
            foreach (var e in r.OnHitEffects)
            {
                switch (e)
                {
                    case "Sunder": effects.Add(new PassiveEffect(PassiveEffectType.SunderOnHit, 1f)); break;
                    case "Bleed": effects.Add(new PassiveEffect(PassiveEffectType.BleedOnHit, 1f)); break;
                    case "Ignite": effects.Add(new PassiveEffect(PassiveEffectType.BurnOnHit, 1f)); break;
                    case "Soaked": effects.Add(new PassiveEffect(PassiveEffectType.ChillOnHit, 1f)); break;
                    case "Stagger": effects.Add(new PassiveEffect(PassiveEffectType.StaggerOnHit, 1f)); break;
                    default: return false;
                }
            }
            return mods.Count + effects.Count > 0;
        }

        /// <summary>
        /// Etap 6 Hell Unleashed: zbroje, biżuteria i unikaty HU jako przedmioty Turris (statystyki przeliczone przez
        /// <see cref="HellEconomy.Map(string)"/>), a perki z odpowiednikiem statystyk jako wielopoziomowe wzmocnienia (poziom = warstwa).
        /// Rzadkość i affixy losuje się na egzemplarzu (<see cref="HellEconomy.Roll"/>).
        /// </summary>
        static void AddHellEconomy(GameConfig cfg)
        {
            foreach (var r in HellEconomy.Items)
            {
                ItemKind kind;
                switch (r.KindName)
                {
                    case "Head": kind = ItemKind.Head; break;
                    case "Body": kind = ItemKind.Body; break;
                    case "Hands": kind = ItemKind.Hands; break;
                    case "Feet": kind = ItemKind.Feet; break;
                    case "Amulet": kind = ItemKind.Amulet; break;
                    case "Ring": kind = ItemKind.Ring; break;
                    default: continue;
                }
                string desc = string.IsNullOrEmpty(r.desc) ? "" : r.desc;
                var it = Make<ItemDefinition>(HuItemId(r), r.name, desc, r.Tags.Contains("robe") ? BuildTag.Magic : r.Tags.Contains("heavy") ? BuildTag.Heavy : BuildTag.Agile);
                it.kind = kind;
                it.weight = r.Load;
                it.huSourceId = r.id;
                it.minFloor = MinFloorFor(r.MinWave);
                it.offerWeight = r.Rarity == 4 ? 0.35f : r.Weight;
                foreach (var m in r.Modifiers) { var mm = HellEconomy.Map(m); if (mm.HasValue) it.modifiers.Add(mm.Value); }
                it.color = r.Tags.Contains("heavy") ? new Color(0.6f, 0.62f, 0.68f) : r.Tags.Contains("robe") ? new Color(0.45f, 0.35f, 0.65f)
                    : r.Tags.Contains("light") ? new Color(0.45f, 0.36f, 0.25f) : new Color(0.75f, 0.68f, 0.45f);
                cfg.baseItemPool.Add(it);
            }
            foreach (var r in HellEconomy.Upgrades)
            {
                if (!PerkContent(r, out var mods, out var effects)) continue;
                var b = Make<BoonDefinition>(HuPerkId(r), r.name, r.desc ?? "", BuildTag.None);
                b.maxStacks = Mathf.Max(1, r.MaxLevel);
                b.modifiers.AddRange(mods);
                b.effects.AddRange(effects);
                cfg.baseBoonPool.Add(b);
            }
        }
    }
}
