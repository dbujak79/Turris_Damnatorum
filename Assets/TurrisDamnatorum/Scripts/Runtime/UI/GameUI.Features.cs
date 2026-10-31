using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Cechy klasowe w interfejsie: dwie równorzędne karty na ekranie przygotowania (mysz i pad – fokus nie zmienia wyboru,
    /// wybrana karta ma osobne, złote oznaczenie), podsumowanie startu, okienko cechy w HUD i drugi zestaw Arsenału w ekwipunku.
    /// „Cecha klasowa” to nowy wybór; bazowa właściwość klasy nazywa się „Mechanika klasy”.
    /// </summary>
    public partial class GameUI
    {
        /// <summary>Tekst cechy z aktualnym przyciskiem klasy (po remapowaniu i zmianie urządzenia).</summary>
        string FeatureText(string s) => s.Replace("{G}", K("Signature"));

        void DrawFeatureCards(LoadoutPlan plan)
        {
            var c = plan.classDef;
            var defs = ClassFeatureCatalog.For(c);
            if (defs.Count == 0) return;
            var t = root.config.classFeatures ?? new ClassFeatureTuning();
            GUILayout.Space(6);
            GUILayout.Label($"<b>Cecha klasowa</b> <size=14>– wybierz jedną na całe podejście (bez kosztu); działa pod {K("Signature")}</size>", label);
            var chosen = plan.Feature;
            foreach (var d in defs)
            {
                var def = d;
                bool on = chosen == def;
                string head = $"  {(def.variant == ClassFeatureVariant.A ? "A" : "B")}. {def.name}" + (on ? "   <color=#ffd060>● WYBRANA</color>" : "");
                Tog(on, head, button, now => { if (now) root.Meta.SelectFeature(plan, def.id); }, GUILayout.Height(38));
                string body = $"<size=14><color=#e8c070>{def.Rule(t)}</color>\n" +
                              $"<b>Korzyść:</b> {def.Benefit(t)}\n<b>Koszt/ryzyko:</b> {def.Cost(t)}\n<b>Użycie:</b> {FeatureText(def.Usage(t))}\n" +
                              $"<b>Nietypowy build:</b> {def.exampleBuild} <color=#aaaaaa>(start: {def.coreBuild}; rozwinięcie: {def.developedBuild})</color>\n" +
                              $"<color=#bbbbbb>{def.passiveHint}</color></size>";
                if (on || defs.Count <= 2) GUILayout.Label(body, small);
            }
            // Arsenał: wybór drugiego zestawu startowego.
            if (chosen != null && chosen.id == "class_feature.warrior.arsenal" && c.loadouts.Count > 0)
            {
                GUILayout.Label("Drugi zestaw (Arsenał):", small);
                var second = plan.ArsenalLoadout;
                for (int li = 0; li < c.loadouts.Count; li++)
                {
                    int idx = li; var l = c.loadouts[li];
                    string text = (string.IsNullOrEmpty(l.label) ? "" : l.label + ": ") + (l.weapon != null ? l.weapon.displayName : "–") + (l.offHand != null ? " + " + l.offHand.displayName : "");
                    Tog(second == l, "  II: " + text, button, now => { if (now) root.Meta.SelectArsenalLoadout(plan, idx); }, GUILayout.Height(34));
                }
            }
            if (plan.featureSkills.Count > 0)
                GUILayout.Label("<size=14>Cecha dokłada do wyboru startowego: " + string.Join(", ", plan.featureSkills.Select(s => s.displayName)) + " (zwykły skill, także dla innych klas).</size>", small);
        }

        /// <summary>
        /// Opis klasy z assetu bez zdania o dawnej sygnaturze („Skill klasy: …”) – przycisk klasy obsługuje teraz cecha klasowa.
        /// Asset zostaje bez zmian (bez regeneracji treści).
        /// </summary>
        static string ClassDescription(ClassDefinition c)
        {
            string d = c.description ?? "";
            if (ClassFeatureCatalog.For(c).Count == 0) return d;
            int i = d.IndexOf("Skill klasy:", System.StringComparison.Ordinal);
            return i >= 0 ? d.Substring(0, i).TrimEnd() : d;
        }

        /// <summary>Podsumowanie startu: klasa, wygląd, cecha, broń, skille, trudność.</summary>
        string LoadoutSummary(LoadoutPlan plan)
        {
            if (plan.classDef == null) return "";
            var (main, off) = plan.StartingHands;
            var skills = plan.StartingSkills.Take(3).Select(s => s.displayName);
            return $"<size=15><b>{plan.classDef.displayName}</b> · wygląd: {(plan.appearance != null ? plan.appearance.displayName : "klasy")} · cecha: <color=#ffd060>{plan.Feature?.name ?? "—"}</color>" +
                   $" · broń: {main?.displayName ?? "—"}{(off != null ? " + " + off.displayName : "")} · skille: {string.Join(", ", skills)} · trudność: {plan.difficulty?.displayName}</size>";
        }

        /// <summary>Okienko cechy w HUD (obok slotów): przycisk, nazwa, stan i pasek.</summary>
        void DrawFeatureSlot(PlayerCombat pc, Rect r)
        {
            var hud = pc.FeatureHud;
            if (hud == null || pc.Feature == null) return;
            var h = hud.Value;
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(r, white);
            var c = h.color; c.a = 1f;
            GUI.color = c * (h.state == ClassFeatureState.Blocked || h.state == ClassFeatureState.Cooldown ? 0.5f : 1f);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - 6, r.width * Mathf.Clamp01(h.fill), 6), white);
            GUI.DrawTexture(new Rect(r.x, r.y, 6, r.height - 6), white);
            GUI.color = Color.white;
            string state = h.state == ClassFeatureState.Ready ? "<color=#99dd99>gotowa</color>" : h.state == ClassFeatureState.Cooldown ? "odnowienie"
                : h.state == ClassFeatureState.Active ? "<color=#ffdd77>działa</color>" : h.state == ClassFeatureState.Preparing ? "<color=#aaccff>przygotowanie</color>"
                : h.state == ClassFeatureState.Stored ? "<color=#ffcc66>zasób</color>" : h.state == ClassFeatureState.Target ? "<color=#ff9966>cel</color>"
                : h.state == ClassFeatureState.SecondPhase ? $"<color=#ffaaff>{K("Signature")} – druga faza</color>" : "<color=#ff8866>niedostępna</color>";
            GUI.Label(new Rect(r.x + 14, r.y + 4, r.width - 18, 30), $"<b>{K("Signature")}</b> <b>{pc.Feature.Def.name}</b>", rich);
            GUI.Label(new Rect(r.x + 14, r.y + 32, r.width - 18, 28), $"<size=14>{state} · {h.label}</size>", rich);
        }

        /// <summary>Czerwona linia: zarezerwowana część paska życia i bieżąca redukcja.</summary>
        void DrawHealthReservation(PlayerCombat pc, Rect bar)
        {
            if (pc.Feature is RedLineFeature rl && rl.Active)
            {
                float line = Mathf.Clamp01(pc.Health.Ceiling / Mathf.Max(1f, pc.Health.Max));
                var reserved = new Rect(bar.x + 8 + (bar.width - 16) * line, bar.y, (bar.width - 16) * (1f - line) + 8, bar.height);
                GUI.color = new Color(0.25f, 0f, 0f, 0.85f);
                GUI.DrawTexture(reserved, white);
                GUI.color = new Color(1f, 0.2f, 0.1f);
                GUI.DrawTexture(new Rect(reserved.x - 2, bar.y - 3, 3, bar.height + 6), white);
                GUI.color = Color.white;
                GUI.Label(new Rect(bar.xMax + 10, bar.y - 4, 420, 30), $"<b><color=#ff4433>linia {line * 100f:0}% · skóra −{rl.CurrentReduction * 100f:0}% · {rl.Remaining:0.0} s</color></b>", rich);
            }
            if (pc.Feature is ArcaneDebtFeature debt && debt.Debt > 0f)
                GUI.Label(new Rect(bar.xMax + 10, bar.y + 22, 560, 22), $"<size=15><color=#ff77aa>Dług many: {debt.Debt:0}/{debt.Limit:0}{(debt.Active ? "" : " – czary zablokowane do spłaty")}</color></size>", rich);
        }

        /// <summary>Arsenał w ekwipunku: drugi zestaw, przenoszenie z plecaka i zdejmowanie.</summary>
        void DrawArsenal(RunState run, PlayerCombat pc)
        {
            if (!run.HasFeature("class_feature.warrior.arsenal")) return;
            GUILayout.Space(8);
            GUILayout.Label("Arsenał – zestaw II", header);
            foreach (var it in new[] { run.arsenalMain, run.arsenalOff })
            {
                if (it == null) continue;
                var item = it;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{(item == run.arsenalMain ? "Ręka główna" : "Druga ręka")}: <b>{item.Name}</b>", label);
                GUI.enabled = !run.BackpackFull;
                Btn("Do plecaka", button, () => { run.TakeFromArsenal(item); pc.RefreshBuild(); }, GUILayout.Width(150));
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            if (run.arsenalMain == null && run.arsenalOff == null) GUILayout.Label("Pusty – przenieś broń z plecaka.", small);
            Btn("Zamień zestawy teraz", button, () => { if (run.SwapArsenal()) pc.RefreshBuild(); });
            GUILayout.Label($"<size=14>Oba zestawy obciążają postać; premie daje tylko trzymany. W walce: {K("Signature")}.</size>", small);
        }

        void ArsenalInventoryButton(RunState run, PlayerCombat pc, ItemInstance it)
        {
            if (!run.HasFeature("class_feature.warrior.arsenal")) return;
            var d = it.definition;
            if (!(d.IsWeapon || d.IsShield || d.kind == ItemKind.OffHand)) return;
            Btn("→ zestaw II", button, () => { if (run.MoveToArsenal(it)) pc.RefreshBuild(); });
        }
    }
}
