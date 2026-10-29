using UnityEngine;

namespace Turris
{
    /// <summary>Panel poligonu umiejętności – prawa krawędź ekranu, nad HUD-em walki.</summary>
    public partial class GameUI
    {
        Vector2 rangeScroll;

        void DrawRangePanel()
        {
            var range = root.Range;
            if (range == null) return;
            DrawRangeSeals();
            // Do wysokości 740 – niżej HUD rysuje listę sterowania (H − 330).
            GUILayout.BeginArea(new Rect(W - 460, 120, 440, 620), box);
            GUILayout.Label("Poligon umiejętności", header);

            GUILayout.BeginHorizontal();
            foreach (var family in range.Families)
            {
                var f = family;
                if (GUILayout.Button(f.displayName, f == range.Family ? SelectedStyle(small) : small) && f != range.Family)
                    nav.Enqueue(() => root.StartSkillRange(f));
            }
            GUILayout.EndHorizontal();

            rangeScroll = GUILayout.BeginScrollView(rangeScroll, GUILayout.Height(200));
            string group = null;
            for (int i = 0; i < range.Entries.Count; i++)
            {
                var e = range.Entries[i];
                if (e.header != group) { group = e.header; GUILayout.Label($"<color=#e0b860>{group}</color>", small); }
                int index = i;
                if (GUILayout.Button(e.skill.displayName, i == range.Selected ? SelectedStyle(small) : small)) nav.Enqueue(() => range.Select(index));
            }
            GUILayout.EndScrollView();

            var def = range.SelectedSkill;
            if (def != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Poziom:", small, GUILayout.Width(70));
                for (int l = 0; l <= def.maxLevel; l++)
                {
                    int level = l;
                    if (GUILayout.Button(l == 0 ? "baz." : "+" + l, l == range.Level ? SelectedStyle(small) : small, GUILayout.Width(52)))
                        nav.Enqueue(() => range.SetLevel(level));
                }
                GUILayout.EndHorizontal();

                GUILayout.Label($"<b>{def.displayName}</b>{(range.Level > 0 ? " +" + range.Level : "")}", label);
                if (!string.IsNullOrEmpty(def.description)) GUILayout.Label(def.description, small);
                var sk = range.Equipped;
                if (sk != null && sk.Def == def)
                {
                    string cost = def.IsSpell ? $"mana {sk.manaCost:0}" : $"wytrzymałość {sk.staminaCost:0}";
                    string element = def.attack != null && def.attack.element != Element.None ? $" · {def.attack.element}" : "";
                    GUILayout.Label($"odnowienie {sk.instance.Cooldown:0.#} s · ładunki {sk.instance.MaxCharges} · {cost}{element}", small);
                    if (!sk.equipmentMet) GUILayout.Label($"<color=#ff8866>{Names.Requirement(def.requiredTags)} – z obecną bronią się nie użyje.</color>", small);
                    else if (!sk.requirementsMet) GUILayout.Label("<color=#ffcc66>Za niskie atrybuty – obniżona skuteczność.</color>", small);
                }
            }

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cele:", small, GUILayout.Width(50));
            foreach (SkillRange.TargetSet set in System.Enum.GetValues(typeof(SkillRange.TargetSet)))
            {
                var s = set;
                string name = s == SkillRange.TargetSet.Pack ? "Gromada" : s == SkillRange.TargetSet.Heavy ? "Ciężcy" : s == SkillRange.TargetSet.Colossus ? "Kolos" : "Pusto";
                if (GUILayout.Button(name, s == range.Targets ? SelectedStyle(small) : small)) nav.Enqueue(() => range.PlaceTargets(s));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Rań cele", range.Bleed ? SelectedStyle(small) : small)) nav.Enqueue(() => range.Bleed = !range.Bleed);
            if (GUILayout.Button("Wychłódź cele", range.Chill ? SelectedStyle(small) : small)) nav.Enqueue(() => range.Chill = !range.Chill);
            if (GUILayout.Button("Pokaz", range.Show ? SelectedStyle(small) : small)) nav.Enqueue(() => range.SetShow(!range.Show));
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.Label(range.PointerMode
                ? "<color=#e0b860>Kursor wolny – kliknij w panel. Puść Alt, by wrócić do walki.</color>"
                : "Wybrana umiejętność pod Q, poprzednia pod E. [ ] – umiejętność, - = – poziom, T – cele, B – rań, V – wychłódź, " +
                  "P – pokaz, przytrzymaj Alt – kursor do panelu. Bez odnowień, bohater nie ginie, cele wstają. Esc – pauza i wyjście.", small);
            GUILayout.EndArea();
        }
    }
}
