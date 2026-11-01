using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Sklep po piętrze (etap 6 Hell Unleashed): bronie z poziomami, perki i przedmioty z rzadkością i affixami.</summary>
    public partial class GameUI
    {
        /// <summary>Założony natywny unikat: ikona, stan mocy (działa/uśpiona z przyczyną) i ustawienie mocy (slot czaru).</summary>
        void DrawUniqueEquipped(ItemInstance it, PlayerCombat pc, float colW)
        {
            var card = UniqueRegistry.CardOf(it.definition);
            GUILayout.BeginHorizontal();
            var icon = UniqueRegistry.Icon(card);
            if (icon != null) GUILayout.Label(icon, GUILayout.Width(40), GUILayout.Height(40));
            GUILayout.Label("<size=14>" + (UniqueText.State(it, pc) ?? "") + "</size>", small, GUILayout.Width(colW - 60));
            GUILayout.EndHorizontal();
            if (card != null && card.code == "T04")
            {
                int slot = it.powerSlot;
                Btn($"Czar zamiast riposty: slot {slot + 1} (zmień)", button, () => { it.powerSlot = (slot + 1) % RunState.SkillSlotCount; });
            }
        }

        /// <summary>Opis egzemplarza: rzadkość, jakość, premie bazowe i affixy (jedna linia na premię).</summary>
        public static string ItemLines(ItemInstance it)
        {
            var sb = new System.Text.StringBuilder();
            if (it.rarity != ItemRarity.Common || it.huQuality > 0)
                sb.Append($"<color=#{Hex(HellEconomy.RarityColor(it.rarity))}>{HellEconomy.RarityName(it.rarity)}</color>{(it.huQuality > 0 ? $", jakość {it.huQuality}" : "")}\n");
            if (it.definition.IsWeapon && it.definition.weapon?.light != null)
                sb.Append($"Lekki cios {it.definition.weapon.light.baseDamage * it.WeaponLevelFactor:0}, poziom {it.level + 1}/{HellEconomy.MaxLevelOf(it.definition) + 1}\n");
            foreach (var m in it.definition.modifiers) sb.Append(m.Scaled(it.QualityFactor)).Append('\n');
            bool legacyAdded = LegacyAddedElements(it);
            for (int i = 0; i < it.affixes.Count; i++)
            {
                var m = it.affixes[i].Scaled(it.HuQualityFactor);
                // Stary zapis: dawne „dodane żywioły” to premia fizyczna – opis bez sugerowania nieistniejącego żywiołu.
                string text = legacyAdded && m.stat == StatType.PhysicalDamage && m.mode == ModifierMode.Flat
                    ? $"+{m.value:0.#}% Premia obrażeń broni ze starego zapisu" : m.ToString();
                sb.Append($"<color=#{Hex(HellEconomy.RarityColor(it.rarity))}>{text}</color>\n");
            }
            foreach (var e in it.Effects) sb.Append(e).Append('\n'); // moce unikatów, uchylenie, dusze, affixy bez statystyki Turris
            return sb.ToString().TrimEnd('\n');
        }

        /// <summary>
        /// Egzemplarz sprzed pakietu affiksów (wersja 0) z dawnym „dodanym żywiołem”: jego premia jest fizyczna i taka zostaje.
        /// Rozpoznanie tylko do opisu – wartości nie są przeliczane ani odejmowane.
        /// </summary>
        public static bool LegacyAddedElements(ItemInstance it)
        {
            if (it.affixVersion >= HellEconomy.AffixVersion || it.affixNames == null) return false;
            foreach (var n in it.affixNames)
                if (n != null && (n.StartsWith("affix.fire_add") || n.StartsWith("affix.water_add") || n.StartsWith("affix.air_add") || n.StartsWith("affix.earth_add")))
                    return true;
            return false;
        }

        /// <summary>Podgląd oferty (HU OfferPreview): co jest teraz założone w slocie, który przedmiot zajmie.</summary>
        static string CurrentlyWorn(RunState run, ItemInstance offer)
        {
            var d = offer.definition;
            var slots = d.kind == ItemKind.Ring ? new[] { EquipSlot.Ring1, EquipSlot.Ring2 } : new[] { d.DefaultSlot };
            var worn = slots.Select(s => run.equipment.Get(s)).Where(i => i != null).ToList();
            if (worn.Count == 0) return "<color=#9fd49f>Teraz: pusty slot</color>";
            return string.Join("\n", worn.Select(i => $"<color=#a8a8a8>Teraz: {i.Name} – {ItemLines(i).Replace("\n", " · ")}</color>"));
        }

        static string PerkLines(BoonDefinition b, int owned, RunState run, GameConfig cfg)
        {
            if (!NativePerkCatalog.IsNative(b)) return PerkLines(b, owned);
            var t = NativePerkCatalog.Tuning(cfg);
            string dormant = NativePerkCatalog.Dormant(b.id, run);
            return $"poziom {owned} → {owned + 1}/{b.maxStacks}: {NativePerkCatalog.Benefit(b.id, t)} {NativePerkCatalog.Condition(b.id)}\n<color=#a0a0a0>Koszt alternatywny: {NativePerkCatalog.Tradeoff(b.id)}</color>"
                   + (dormant != null ? $"\n<color=#E07060>{dormant}</color>" : "");
        }

        static string PerkLines(BoonDefinition b, int owned) =>
            $"poziom {owned} → {owned + 1}/{b.maxStacks}: " + string.Join(", ", b.modifiers.Select(m => m.ToString()).Concat(b.effects.Select(e => e.ToString())));

        /// <summary>Sekcja sklepu po piętrze w sklepie dusz.</summary>
        void DrawFloorShopSection()
        {
            var run = root.Run;
            var stock = root.FloorStock;
            if (stock == null || stock.Count == 0) return;
            var wrap = new GUIStyle(small) { wordWrap = true, richText = true };
            // Siła buildu (HU LoadoutAnalysis): zwykły atak i skille w slotach – na trafienie, tempo, DPS.
            var snap = root.Player != null ? root.Player.Build : null;
            if (snap != null)
            {
                GUILayout.Label("<b>Twój build</b>", label);
                GUILayout.Label(LoadoutAnalysis.Format(LoadoutAnalysis.MainAttack(snap), true), wrap);
                foreach (var l in LoadoutAnalysis.Skills(snap)) GUILayout.Label(LoadoutAnalysis.Format(l, false), wrap);
                GUILayout.Space(6);
            }
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
                    string lines = offer.kind == FloorOffer.Kind.Perk ? PerkLines(offer.perk, run.BoonStacks(offer.perk), run, root.config)
                        : offer.kind == FloorOffer.Kind.WeaponLevel ? (offer.item.definition.IsShield ? "Blok bierny +2 pkt, szybszy ładunek bloku" : "Obrażenia broni +20%") + $" (poziom {offer.item.level + 2}/{HellEconomy.MaxLevelOf(offer.item.definition) + 1})"
                        : ItemLines(offer.item).Replace("\n", " · ");
                    GUILayout.Label(lines, wrap);
                    if (!offer.sold && (offer.kind == FloorOffer.Kind.Item || offer.kind == FloorOffer.Kind.Weapon)) GUILayout.Label(CurrentlyWorn(run, offer.item), wrap);
                    if (!offer.sold && offer.kind == FloorOffer.Kind.Weapon)
                    {
                        var losing = LoadoutAnalysis.PerksLosingEffect(run, offer.item.definition);
                        if (losing.Count > 0) GUILayout.Label($"<color=#E07060>Po założeniu przestaną działać: {string.Join(", ", losing.Select(p => p.displayName))}</color>", wrap);
                    }
                    else if (!offer.sold && offer.kind == FloorOffer.Kind.Item && offer.item?.definition != null)
                    {
                        var losing = LoadoutAnalysis.NativePerksLosingEffect(run, offer.item.definition);
                        if (losing.Count > 0) GUILayout.Label($"<color=#E07060>Po założeniu przestaną działać: {string.Join(", ", losing.Select(p => p.displayName))}</color>", wrap);
                    }
                    if (!offer.sold && offer.kind == FloorOffer.Kind.Perk && snap != null && LoadoutAnalysis.PerkUseless(offer.perk, snap))
                        GUILayout.Label("<color=#E07060>Nic nie da w tym buildzie (brak czaru w slotach)</color>", wrap);
                    bool fits = offer.item == null || offer.kind == FloorOffer.Kind.WeaponLevel || run.CanReceive(offer.item);
                    GUI.enabled = !offer.sold && run.souls >= offer.price && fits;
                    Btn(offer.sold ? "Kupione" : fits ? $"Kup ({offer.price} dusz)" : "Plecak pełny", button, () => root.BuyFloorOffer(offer), GUILayout.Height(38));
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
