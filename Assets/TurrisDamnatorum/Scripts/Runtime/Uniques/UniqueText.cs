using System.Linq;
using System.Text;

namespace Turris
{
    /// <summary>
    /// Opisy natywnych unikatów dla gracza: statystyki (osobno), legendarna reguła, koszt/ograniczenie, warunek, stan uśpienia
    /// i miejsce zdobycia. Liczby pochodzą z parametrów karty (te same, które czyta runtime).
    /// </summary>
    public static class UniqueText
    {
        public const string Color = "#d9733a";

        /// <summary>Linie opisu karty; <paramref name="pc"/> (opcjonalnie) – stan mocy u właściciela.</summary>
        public static string Lines(ItemInstance it, PlayerCombat pc = null)
        {
            var card = UniqueRegistry.CardOf(it?.definition);
            if (card == null) return "";
            var sb = new StringBuilder();
            sb.Append($"<color={Color}><b>UNIKAT {card.code}{(card.isNew ? " · NOWY" : "")}</b> – {card.promise}</color>\n");
            sb.Append($"<b>Moc:</b> {card.Rule}");
            if (card.code == "T04" && it != null) sb.Append($" (wybrany slot: {it.powerSlot + 1})");
            sb.Append('\n');
            sb.Append($"<b>Cena:</b> {card.Cost}\n");
            sb.Append($"<b>Warunek:</b> {card.condition}\n");
            string state = State(it, pc);
            if (state != null) sb.Append(state).Append('\n');
            sb.Append($"<size=13><color=#a0a0a0>Klasa: {ClassName(card.classId)} · szukaj: {card.Where} · jakość skaluje tylko statystyki</color></size>");
            return sb.ToString();
        }

        /// <summary>Stan mocy u właściciela (działa / uśpiona: przyczyna) – null, gdy przedmiot nie jest założony.</summary>
        public static string State(ItemInstance it, PlayerCombat pc)
        {
            if (pc == null || it == null || pc.Run == null || !pc.Run.equipment.IsEquipped(it)) return null;
            var p = pc.Uniques.Get(it.definition.uniquePowerId);
            if (p == null) return "<color=#ff8866>Moc: brak kontrolera</color>";
            if (p.Item != it) return "<color=#a0a0a0>Druga kopia: działa tylko jedna legenda (pierwszy slot); statystyki obu się sumują.</color>";
            string why = p.Dormant();
            return why == null ? "<color=#99dd99>Moc: aktywna</color>" + (p.Hud != null ? $" · {p.Hud}" : "") : $"<color=#ff9966>Moc {why}</color>";
        }

        public static string ClassName(string classId)
        {
            switch (classId)
            {
                case "class_knight": return "Wojownik";
                case "class_hu_paladin": return "Paladyn";
                case "class_hu_berserker": return "Berserker";
                case "class_hu_dual": return "Rębajło";
                case "class_mage": return "Mag";
                case "class_hu_mage_fire": return "Mag ognia";
                case "class_hu_mage_water": return "Mag wody";
                case "class_hu_mage_air": return "Mag powietrza";
                case "class_hu_mage_earth": return "Mag ziemi";
                case "class_hu_summoner": return "Przywoływacz";
                case "class_hu_battlemage": return "Mag bitewny";
                case "class_rogue": return "Łotrzyk";
                case "class_hu_archer": return "Łucznik";
                case "class_hu_assassin": return "Zabójca";
                case "class_hu_arcane_archer": return "Łucznik arkanów";
                default: return classId;
            }
        }

        /// <summary>Magazyny mocy do HUD (tylko gdy coś przechowują).</summary>
        public static string Hud(PlayerCombat pc)
        {
            if (pc == null) return null;
            var parts = pc.Uniques.All.Where(p => p.Active).Select(p => p.Hud).Where(h => h != null).ToList();
            return parts.Count == 0 ? null : string.Join(" · ", parts);
        }
    }
}
