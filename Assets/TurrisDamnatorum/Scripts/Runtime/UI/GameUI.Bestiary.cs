using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Bestiariusz (jak w Hell Unleashed): siatka kart z portretami wrogów, a obok duży, obracający się podgląd wybranego
    /// z opisem, liczbą zabitych oraz mocnymi i słabymi stronami. Niespotkani – czarna sylwetka i „???”; słabości odkrywa się,
    /// zabijając (<see cref="Bestiary.RevealKills"/>). Zakładki: demony z Hell Unleashed i wrogowie wieży.
    /// </summary>
    public partial class GameUI
    {
        BestiaryStage stage;
        int bestiaryTab;
        EnemyDefinition bestiarySelected;
        readonly ScrollState scrollBestiary = new ScrollState();
        GUIStyle bestiaryTitle, bestiaryName, bestiaryCardName, bestiaryStrong, bestiaryWeak, bestiaryMuted, bestiaryCard, bestiaryCardSel, bestiaryPanel;

        static readonly string[] BestiaryTabs = { "Demony", "Wrogowie wieży" };

        void InitBestiaryStyles()
        {
            if (bestiaryTitle != null) return;
            bestiaryTitle = new GUIStyle(title) { fontSize = 58 };
            bestiaryTitle.normal.textColor = new Color(0.86f, 0.33f, 0.24f);
            bestiaryName = new GUIStyle(header) { fontSize = 34, alignment = TextAnchor.MiddleCenter };
            bestiaryName.normal.textColor = new Color(0.9f, 0.36f, 0.26f);
            bestiaryCardName = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, wordWrap = false, clipping = TextClipping.Clip };
            bestiaryStrong = new GUIStyle(header) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
            bestiaryStrong.normal.textColor = new Color(0.9f, 0.36f, 0.26f);
            bestiaryWeak = new GUIStyle(bestiaryStrong);
            bestiaryWeak.normal.textColor = new Color(0.45f, 0.85f, 0.45f);
            bestiaryMuted = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter };
            bestiaryMuted.normal.textColor = new Color(0.62f, 0.6f, 0.62f);
            bestiaryCard = new GUIStyle(GUI.skin.box);
            bestiaryCard.normal.background = MakeTex(new Color(0.16f, 0.11f, 0.14f, 0.95f));
            bestiaryCardSel = new GUIStyle(bestiaryCard);
            bestiaryCardSel.normal.background = MakeTex(new Color(0.85f, 0.33f, 0.2f, 1f));
            bestiaryPanel = new GUIStyle(box);
            bestiaryPanel.normal.background = MakeTex(new Color(0.07f, 0.05f, 0.07f, 0.95f));
        }

        public void OpenBestiary()
        {
            if (stage == null) stage = BestiaryStage.Create();
            bestiarySelected = null;
            root.ShowBestiary();
        }

        public void CloseBestiary()
        {
            if (stage != null) Destroy(stage.gameObject);
            stage = null;
            root.ShowMainMenu();
        }

        List<BestiaryEntry> BestiaryList(int tab) =>
            Bestiary.Entries(root.config).Where(e => e.demon == (tab == 0)).OrderBy(e => e.def.isBoss).ThenBy(e => e.floor == 0 ? 99 : e.floor).ToList();

        void DrawBestiary()
        {
            InitBestiaryStyles();
            if (stage == null) stage = BestiaryStage.Create();
            var meta = root.Meta;
            var all = Bestiary.Entries(root.config);
            int seen = all.Count(e => meta.Seen(e.def));
            int known = all.Count(e => meta.Kills(e.def) >= Bestiary.RevealAt(e.def));

            // Ciemne tło jak w HU – arena z menu nie prześwituje.
            var prev = GUI.color;
            GUI.color = new Color(0.07f, 0.05f, 0.07f, 0.97f);
            GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture);
            GUI.color = prev;
            GUI.Label(new Rect(0, 18, W, 70), "BESTIARIUSZ", bestiaryTitle);
            GUI.Label(new Rect(0, 86, W, 30), $"Spotkani wrogowie: {seen}/{all.Count}  ·  poznane słabości: {known}  (po {Bestiary.RevealKills} zabitych, boss – po pierwszym)", bestiaryMuted);

            // Zakładki
            float tabW = 260f, tabX = W / 2 - (tabW * BestiaryTabs.Length + 20f * (BestiaryTabs.Length - 1)) / 2;
            for (int t = 0; t < BestiaryTabs.Length; t++)
            {
                int tab = t;
                BtnRect(new Rect(tabX + t * (tabW + 20f), 124, tabW, 44), BestiaryTabs[t], bestiaryTab == t ? SelectedStyle(button) : button,
                    () => { bestiaryTab = tab; bestiarySelected = null; scrollBestiary.pos = Vector2.zero; });
            }

            var list = BestiaryList(bestiaryTab);
            if (bestiarySelected == null || !list.Any(e => e.def == bestiarySelected)) bestiarySelected = list.Count > 0 ? list[0].def : null;

            // Siatka kart
            float top = 186f, bottom = H - 110f;
            float gridW = Mathf.Min(W * 0.5f, 1000f);
            const int cols = 5;
            float cardW = (gridW - 40f) / cols - 10f, cardH = cardW + 34f;
            GUILayout.BeginArea(new Rect(40, top, gridW, bottom - top));
            BeginScroll(scrollBestiary, bottom - top);
            for (int i = 0; i < list.Count; i += cols)
            {
                GUILayout.BeginHorizontal();
                for (int c = 0; c < cols && i + c < list.Count; c++)
                {
                    var e = list[i + c];
                    Rect r = GUILayoutUtility.GetRect(cardW, cardH, GUILayout.Width(cardW), GUILayout.Height(cardH));
                    DrawBestiaryCard(r, e);
                    GUILayout.Space(10);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            EndScroll();
            GUILayout.EndArea();

            // Panel szczegółów
            var panel = new Rect(40 + gridW + 20, top, W - gridW - 100, bottom - top);
            GUI.Box(panel, GUIContent.none, bestiaryPanel);
            if (Event.current.type == EventType.Repaint) Frame(panel, new Color(0.42f, 0.24f, 0.3f), 2f);
            if (bestiarySelected != null) DrawBestiaryDetails(panel, bestiarySelected);

            BtnRect(new Rect(W / 2 - 170, H - 92, 340, 52), "Wróć", button, CloseBestiary);
        }

        static void Frame(Rect r, Color c, float t)
        {
            var prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, t), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.yMax - t, r.width, t), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, t, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.xMax - t, r.y, t, r.height), Texture2D.whiteTexture);
            GUI.color = prev;
        }

        void DrawBestiaryCard(Rect r, BestiaryEntry e)
        {
            bool seen = root.Meta.Seen(e.def);
            bool selected = e.def == bestiarySelected;
            if (Event.current.type == EventType.Repaint)
            {
                (selected ? bestiaryCardSel : bestiaryCard).Draw(r, false, false, false, false);
                var inner = new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8);
                // Model zawsze w kolorach (jak w HU); do spotkania ukryte są tylko nazwa i opis.
                GUI.DrawTexture(new Rect(inner.x, inner.y, inner.width, inner.width), stage.Portrait(e.def, true), ScaleMode.ScaleToFit);
                Frame(r, selected ? new Color(1f, 0.45f, 0.3f) : new Color(0.42f, 0.24f, 0.3f), selected ? 3f : 2f);
                if (e.def.isBoss && seen)
                    GUI.Label(new Rect(inner.x + 4, inner.y + 2, inner.width, 22), "<color=#ffcc55>BOSS</color>", small);
            }
            GUI.Label(new Rect(r.x, r.yMax - 32, r.width, 28), seen ? e.def.displayName : "???", bestiaryCardName);
            int id = nav.Register(r);
            DrawFocus(id, r);
            bool clicked = Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition);
            if (clicked) Event.current.Use();
            if (clicked || nav.ConsumeActivation(id)) { var def = e.def; nav.Enqueue(() => bestiarySelected = def); Sfx.Ui(SoundId.UiConfirm); }
            // Fokus padem też wybiera (podgląd idzie za kursorem).
            if (nav.IsFocused(id) && nav.NavMode && bestiarySelected != e.def) bestiarySelected = e.def;
        }

        void DrawBestiaryDetails(Rect panel, EnemyDefinition d)
        {
            var meta = root.Meta;
            bool seen = meta.Seen(d);
            int kills = meta.Kills(d);
            float pad = 16f;
            float previewH = Mathf.Min(panel.height * 0.52f, panel.width - 2 * pad);
            var pr = new Rect(panel.x + pad, panel.y + pad, panel.width - 2 * pad, previewH);
            GUI.DrawTexture(pr, Texture2D.blackTexture);
            BtnRect(new Rect(panel.x + pad, panel.yMax - 48, panel.width - 2 * pad, 36), "Próba walki (bez nagród i zapisu)", button, () => root.StartEnemyTrial(d));
            if (Event.current.type == EventType.Repaint) GUI.DrawTexture(pr, stage.Preview(d, true), ScaleMode.ScaleToFit);

            float y = pr.yMax + 10f, x = panel.x + pad, w = panel.width - 2 * pad;
            void Line(string text, GUIStyle st, float h)
            {
                float hh = Mathf.Max(h, st.CalcHeight(new GUIContent(text), w));
                GUI.Label(new Rect(x, y, w, hh), text, st);
                y += hh + 2f;
            }
            if (!seen)
            {
                Line("???", bestiaryName, 42);
                Line("Jeszcze nie spotkany. Pojawi się wyżej w wieży.", bestiaryMuted, 26);
                return;
            }
            var entry = Bestiary.Entries(root.config).FirstOrDefault(e => e.def == d);
            Line(d.displayName, bestiaryName, 42);
            Line(Bestiary.Summary(d) + (entry != null && entry.floor > 0 ? $"  ·  od piętra {entry.floor}" : ""), bestiaryMuted, 24);
            var desc = d.description;
            if (entry != null && entry.variants.Count > 0) desc += "  Odmiany: " + string.Join(", ", entry.variants.Select(v => v.displayName)) + ".";
            Line(desc, new GUIStyle(small) { alignment = TextAnchor.MiddleCenter }, 24);
            Line($"Zabitych: {kills}", new GUIStyle(small) { alignment = TextAnchor.MiddleCenter }, 24);

            var strong = Bestiary.Strengths(d);
            if (strong.Count > 0)
            {
                Line("Mocne strony", bestiaryStrong, 30);
                foreach (var s in strong.Take(6)) Line("•  " + s, new GUIStyle(small) { alignment = TextAnchor.MiddleCenter }, 22);
            }
            Line("Słabe strony", bestiaryWeak, 30);
            int need = Bestiary.RevealAt(d);
            if (kills >= need)
                foreach (var s in Bestiary.Weaknesses(d).Take(6)) Line("•  " + s, new GUIStyle(small) { alignment = TextAnchor.MiddleCenter }, 22);
            else
                Line($"Zabij jeszcze {need - kills}, by poznać słabości.", bestiaryMuted, 22);
        }
    }
}
