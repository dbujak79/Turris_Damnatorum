using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Sklep po piętrze (etap 6 Hell Unleashed): bronie z poziomami, perki i przedmioty z rzadkością i affixami.</summary>
    public partial class GameUI
    {
        /// <summary>Opis egzemplarza: rzadkość, jakość, premie bazowe i affixy (jedna linia na premię).</summary>
        public static string ItemLines(ItemInstance it)
        {
            var sb = new System.Text.StringBuilder();
            if (it.rarity != ItemRarity.Common || it.huQuality > 0)
                sb.Append($"<color=#{Hex(HellEconomy.RarityColor(it.rarity))}>{HellEconomy.RarityName(it.rarity)}</color>{(it.huQuality > 0 ? $", jakość {it.huQuality}" : "")}\n");
            if (it.definition.IsWeapon && it.definition.weapon?.light != null)
                sb.Append($"Lekki cios {it.definition.weapon.light.baseDamage * it.WeaponLevelFactor:0}, poziom {it.level + 1}/{HellEconomy.MaxLevelOf(it.definition) + 1}\n");
            foreach (var m in it.definition.modifiers) sb.Append(m.Scaled(it.QualityFactor)).Append('\n');
            for (int i = 0; i < it.affixes.Count; i++) sb.Append($"<color=#{Hex(HellEconomy.RarityColor(it.rarity))}>{it.affixes[i].Scaled(it.HuQualityFactor)}</color>\n");
            return sb.ToString().TrimEnd('\n');
        }

        static string PerkLines(BoonDefinition b, int owned) =>
            $"poziom {owned} → {owned + 1}/{b.maxStacks}: " + string.Join(", ", b.modifiers.Select(m => m.ToString()));

        /// <summary>Sekcja sklepu po piętrze w sklepie dusz.</summary>
        void DrawFloorShopSection()
        {
            var run = root.Run;
            var stock = root.FloorStock;
            if (stock == null || stock.Count == 0) return;
            var wrap = new GUIStyle(small) { wordWrap = true, richText = true };
            void Section(string title, FloorOffer.Kind[] kinds)
            {
                var list = stock.Where(o => kinds.Contains(o.kind)).ToList();
                if (list.Count == 0) return;
                GUILayout.Label($"<b>{title}</b>", label);
                foreach (var o in list)
                {
                    var offer = o;
                    string color = offer.item != null ? Hex(HellEconomy.RarityColor(offer.item.rarity)) : "C8D8FF";
                    GUILayout.Label($"<color=#{color}>{offer.Title}</color>", label);
                    string lines = offer.kind == FloorOffer.Kind.Perk ? PerkLines(offer.perk, run.BoonStacks(offer.perk))
                        : offer.kind == FloorOffer.Kind.WeaponLevel ? $"Obrażenia broni +20% (poziom {offer.item.level + 2}/{HellEconomy.MaxLevelOf(offer.item.definition) + 1})"
                        : ItemLines(offer.item).Replace("\n", " · ");
                    GUILayout.Label(lines, wrap);
                    GUI.enabled = !offer.sold && run.souls >= offer.price;
                    Btn(offer.sold ? "Kupione" : $"Kup ({offer.price} dusz)", button, () => root.BuyFloorOffer(offer), GUILayout.Height(38));
                    GUI.enabled = true;
                    GUILayout.Space(4);
                }
                GUILayout.Space(6);
            }
            Section("Bronie", new[] { FloorOffer.Kind.WeaponLevel, FloorOffer.Kind.Weapon });
            Section("Perki", new[] { FloorOffer.Kind.Perk });
            Section("Przedmioty", new[] { FloorOffer.Kind.Item });
        }
    }
}
