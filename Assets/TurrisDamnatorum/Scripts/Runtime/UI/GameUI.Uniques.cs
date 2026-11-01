using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Katalog natywnych unikatów (menu główne): 75 kart z ikoną, klasą i miejscem zdobycia; odkryte pokazują pełny opis i mogą
    /// zostać wskazane jako poszukiwane (umiarkowana preferencja w puli unikatów – UniqueLoot.SoughtFactor; bez nowej waluty).
    /// </summary>
    public partial class GameUI
    {
        bool showUniqueCatalog;
        readonly ScrollState scrollUniques = new ScrollState();

        void DrawUniqueCatalog()
        {
            var meta = root.Meta;
            var p = meta.Profile;
            var cards = UniqueCatalog.All;
            int found = cards.Count(c => p.discoveredUniques.Contains(c.id));
            GUI.Label(new Rect(0, 30, W, 60), "KATALOG UNIKATÓW", title);
            var sought = UniqueCatalog.Get(p.soughtUnique);
            GUI.Label(new Rect(0, 100, W, 30), $"Odkryte: {found}/{cards.Count}  ·  poszukiwany: {(sought != null ? sought.name : "brak")} (×{UniqueLoot.SoughtFactor:0} w puli unikatów od jego progu i źródła)", center);
            GUILayout.BeginArea(new Rect(60, 140, W - 120, H - 260), box);
            BeginScroll(scrollUniques, H - 300);
            string lastClass = null;
            foreach (var c in cards)
            {
                if (c.classId != lastClass) { lastClass = c.classId; GUILayout.Space(6); GUILayout.Label($"<b>{UniqueText.ClassName(c.classId)}</b>", header); }
                bool known = p.discoveredUniques.Contains(c.id);
                GUILayout.BeginHorizontal();
                var icon = UniqueRegistry.Icon(c);
                if (icon != null) { var prev = GUI.color; if (!known) GUI.color = new Color(0.35f, 0.35f, 0.35f); GUILayout.Label(icon, GUILayout.Width(52), GUILayout.Height(52)); GUI.color = prev; }
                string text = known
                    ? $"<b>{c.code} {c.name}</b>{(c.isNew ? " <color=#d9733a>NOWY</color>" : "")} – {c.promise}\n<size=14>{c.Rule}\nCena: {c.Cost}\nWarunek: {c.condition} · szukaj: {c.Where}</size>"
                    : $"<b>{c.code} ???</b> <size=14>({Names.Slot(KindSlot(c.kind))}) – nieodkryty · szukaj: {c.Where}</size>";
                GUILayout.Label(text, label, GUILayout.Width(W - 480));
                if (known)
                {
                    bool isSought = p.soughtUnique == c.id;
                    var id = c.id;
                    Btn(isSought ? "Poszukiwany ✓" : "Szukaj", isSought ? SelectedStyle(button) : button, () => meta.SetSoughtUnique(isSought ? "" : id), GUILayout.Width(200));
                }
                GUILayout.EndHorizontal();
            }
            EndScroll();
            GUILayout.EndArea();
            BtnRect(new Rect(W - 300, H - 100, 260, 60), "Wróć", button, () => showUniqueCatalog = false);
        }

        static EquipSlot KindSlot(ItemKind k)
        {
            switch (k)
            {
                case ItemKind.Weapon: return EquipSlot.MainHand;
                case ItemKind.Shield: case ItemKind.OffHand: return EquipSlot.OffHand;
                case ItemKind.Head: return EquipSlot.Head;
                case ItemKind.Body: return EquipSlot.Body;
                case ItemKind.Hands: return EquipSlot.Hands;
                case ItemKind.Belt: return EquipSlot.Belt;
                case ItemKind.Feet: return EquipSlot.Feet;
                case ItemKind.Ring: return EquipSlot.Ring1;
                default: return EquipSlot.Amulet;
            }
        }
    }
}
