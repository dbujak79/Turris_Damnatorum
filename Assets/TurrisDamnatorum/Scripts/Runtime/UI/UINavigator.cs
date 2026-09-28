using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Stan przewijanej listy, którą nawigator może przewinąć do elementu z fokusem.</summary>
    public class ScrollState
    {
        public Vector2 pos;
        public float viewHeight;
    }

    /// <summary>
    /// Nawigacja padem/klawiaturą po interfejsie IMGUI.
    ///
    /// Każda kontrolka rejestruje się w kolejności rysowania i dostaje identyfikator. W zdarzeniu Repaint
    /// zapamiętywane są jej prostokąty ekranowe, na podstawie których nawigacja kierunkowa wybiera
    /// najbliższą kontrolkę w danym kierunku (działa w kolumnach, siatkach i listach bez ręcznej konfiguracji).
    ///
    /// Akcje (kliknięcia myszą i zatwierdzenia padem) są kolejkowane i wykonywane na końcu przebiegu OnGUI,
    /// dzięki czemu zmiana stanu nie rozspójnia układu GUILayout między zdarzeniami Layout i Repaint.
    /// </summary>
    public class UINavigator
    {
        /// <summary>Czy pokazywać fokus (ostatnio użyto pada lub strzałek, a nie myszy).</summary>
        public bool NavMode { get; set; }
        public int Focus { get; private set; }
        public int Count { get; private set; }

        int counter;
        List<Rect> rects = new List<Rect>();
        List<Rect> nextRects = new List<Rect>();
        readonly List<Action> pending = new List<Action>();
        readonly Stack<ScrollState> scrolls = new Stack<ScrollState>();
        bool activateRequested;
        bool scrollToFocus = true;

        /// <summary>Wywołać na początku każdego OnGUI.</summary>
        public void BeginGUI()
        {
            counter = 0;
            scrolls.Clear();
            if (Event.current.type == EventType.Repaint) nextRects.Clear();
        }

        /// <summary>Wywołać na końcu każdego OnGUI. Wykonuje zakolejkowane akcje.</summary>
        public void EndGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                var t = rects; rects = nextRects; nextRects = t;
                Count = counter;
                if (Count > 0 && Focus >= Count) Focus = Count - 1;
                activateRequested = false; // niewykorzystane zatwierdzenie nie przechodzi na kolejną klatkę
            }
            if (pending.Count == 0) return;
            var actions = pending.ToArray();
            pending.Clear();
            foreach (var a in actions) a();
        }

        public void Enqueue(Action a)
        {
            if (a != null) pending.Add(a);
        }

        /// <summary>Resetuje fokus (np. po zmianie ekranu).</summary>
        public void Reset(int focus = 0)
        {
            Focus = focus;
            scrollToFocus = true;
            activateRequested = false;
            rects.Clear();
            Count = 0;
        }

        public void RequestActivate() => activateRequested = true;

        public void PushScroll(ScrollState s) => scrolls.Push(s);
        public void PopScroll() { if (scrolls.Count > 0) scrolls.Pop(); }

        /// <summary>Rejestruje kontrolkę narysowaną w <paramref name="localRect"/> (współrzędne bieżącego GUI). Zwraca jej id.</summary>
        public int Register(Rect localRect)
        {
            int id = counter++;
            if (Event.current.type != EventType.Repaint) return id;

            nextRects.Add(GUIUtility.GUIToScreenRect(localRect));
            if (id == Focus && scrollToFocus)
            {
                if (scrolls.Count > 0)
                {
                    var s = scrolls.Peek();
                    const float margin = 20f;
                    if (localRect.yMax + margin > s.pos.y + s.viewHeight) s.pos.y = localRect.yMax + margin - s.viewHeight;
                    if (localRect.y - margin < s.pos.y) s.pos.y = Mathf.Max(0, localRect.y - margin);
                }
                scrollToFocus = false;
            }
            return id;
        }

        public bool IsFocused(int id) => NavMode && id == Focus;

        /// <summary>Czy kontrolka o danym id została zatwierdzona padem w tej klatce (tylko gdy jest aktywna).</summary>
        public bool ConsumeActivation(int id)
        {
            if (!NavMode || !activateRequested || id != Focus || !GUI.enabled) return false;
            if (Event.current.type != EventType.Repaint) return false;
            activateRequested = false;
            return true;
        }

        /// <summary>Przenosi fokus na najbliższą kontrolkę w kierunku <paramref name="dir"/> (oś Y w dół, jak w GUI).</summary>
        public void Move(Vector2 dir)
        {
            if (rects.Count == 0) return;
            if (Focus < 0 || Focus >= rects.Count) { Focus = 0; scrollToFocus = true; return; }
            int next = FindInDirection(rects, Focus, dir);
            if (next >= 0)
            {
                Focus = next;
                scrollToFocus = true;
            }
        }

        /// <summary>Wybór kontrolki w kierunku: odległość wzdłuż kierunku + kara za przesunięcie w poprzek.</summary>
        public static int FindInDirection(IList<Rect> rects, int from, Vector2 dir)
        {
            Vector2 cur = rects[from].center;
            int best = -1;
            float bestScore = float.MaxValue;
            for (int i = 0; i < rects.Count; i++)
            {
                if (i == from) continue;
                Vector2 d = rects[i].center - cur;
                float along = Vector2.Dot(d, dir);
                if (along <= 4f) continue;
                float across = Mathf.Abs(d.x * dir.y - d.y * dir.x);
                float score = along + across * 2.5f;
                if (score < bestScore) { bestScore = score; best = i; }
            }
            return best;
        }
    }
}
