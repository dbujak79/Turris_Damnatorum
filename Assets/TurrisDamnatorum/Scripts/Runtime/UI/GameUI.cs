using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Turris
{
    /// <summary>
    /// Cały interfejs prototypu w IMGUI (celowo prosty – bez prefabów i Canvasów).
    /// Skalowany do wysokości 1080 px. Wszystkie ekrany obsługują mysz, pada i strzałki klawiatury
    /// (fokus przestrzenny – patrz <see cref="UINavigator"/>).
    /// </summary>
    [RequireComponent(typeof(GameRoot))]
    public class GameUI : MonoBehaviour
    {
        GameRoot root;
        const float RefHeight = 1080f;
        float W => UnityEngine.Screen.width * RefHeight / UnityEngine.Screen.height;
        const float H = RefHeight;

        GUIStyle title, header, label, small, box, button, center, bigCenter, rich;
        Texture2D white;
        readonly ScrollState scrollA = new ScrollState(), scrollB = new ScrollState(), scrollC = new ScrollState();
        string status;
        bool showHelp = true;

        readonly UINavigator nav = new UINavigator();
        public UINavigator Navigator => nav;
        /// <summary>Ostatnio użyte urządzenie to pad (podpowiedzi przycisków w HUD i menu).</summary>
        public bool UsingPad { get; private set; }
        GameScreen lastScreen = (GameScreen)(-1);
        Vector2 heldDir;
        float repeatTimer;
        const float RepeatDelay = 0.35f, RepeatRate = 0.12f;

        struct Floating { public Vector3 pos; public string text; public Color color; public float t; }
        readonly List<Floating> floating = new List<Floating>();
        struct Msg { public string text; public Color color; public float t; }
        readonly List<Msg> messages = new List<Msg>();

        void Awake()
        {
            root = GetComponent<GameRoot>();
        }

        void OnEnable()
        {
            CombatEvents.HitResolved += OnHit;
            CombatEvents.Message += OnMessage;
            CombatEvents.WorldText += OnWorldText;
        }

        void OnDisable()
        {
            CombatEvents.HitResolved -= OnHit;
            CombatEvents.Message -= OnMessage;
            CombatEvents.WorldText -= OnWorldText;
        }

        void OnWorldText(Vector3 pos, string text, Color c) =>
            floating.Add(new Floating { pos = pos + UnityEngine.Random.insideUnitSphere * 0.3f, text = text, color = c, t = 0 });

        void OnHit(Vector3 pos, HitResult r, bool targetIsPlayer)
        {
            string text; Color c;
            switch (r.outcome)
            {
                case HitOutcome.Dodged: text = "unik"; c = new Color(0.6f, 0.9f, 1f); break;
                case HitOutcome.Parried: text = "SPAROWANO"; c = Color.white; break;
                case HitOutcome.Blocked: text = r.healthDamage > 0.5f ? $"blok −{r.healthDamage:0}" : "blok"; c = new Color(0.9f, 0.75f, 0.4f); break;
                case HitOutcome.GuardBroken: text = $"PRZEŁAMANIE −{r.healthDamage:0}"; c = new Color(1f, 0.5f, 0.2f); break;
                default: text = $"{r.healthDamage:0}"; c = targetIsPlayer ? new Color(1f, 0.35f, 0.3f) : Color.white; break;
            }
            floating.Add(new Floating { pos = pos + UnityEngine.Random.insideUnitSphere * 0.3f, text = text, color = c, t = 0 });
        }

        void OnMessage(string text, Color c)
        {
            messages.Add(new Msg { text = text, color = c, t = 0 });
            if (messages.Count > 4) messages.RemoveAt(0);
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            DetectDevice();
            if (root.Screen != lastScreen)
            {
                lastScreen = root.Screen;
                nav.Reset();
                nav.NavMode = UsingPad; // grając padem od razu widać fokus na nowym ekranie
                heldDir = Vector2.zero;
            }
            TickPendingRebind();
            bool listening = root.Input != null && (root.Input.IsRebinding || pendingRebind != null);
            if (root.Screen != GameScreen.Playing && !listening) HandleMenuInput(dt);

            for (int i = floating.Count - 1; i >= 0; i--)
            {
                var f = floating[i]; f.t += dt; f.pos += Vector3.up * dt * 0.8f; floating[i] = f;
                if (f.t > 1.1f) floating.RemoveAt(i);
            }
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                var m = messages[i]; m.t += dt; messages[i] = m;
                if (m.t > 2.2f) messages.RemoveAt(i);
            }
            var kb = Keyboard.current;
            if (listening) return;
            if (kb != null && kb.f1Key.wasPressedThisFrame) showHelp = !showHelp;
            if (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame) showHelp = !showHelp;
            if (kb != null && root.Screen == GameScreen.Reward)
            {
                if (kb.digit1Key.wasPressedThisFrame) root.ChooseReward(0);
                else if (kb.digit2Key.wasPressedThisFrame) root.ChooseReward(1);
                else if (kb.digit3Key.wasPressedThisFrame) root.ChooseReward(2);
            }
        }

        // ================================================================== Wejście w menu (pad / strzałki)

        void DetectDevice()
        {
            var g = Gamepad.current;
            if (g != null && PadActivity(g)) UsingPad = true;
            var kb = Keyboard.current;
            if (kb != null && kb.anyKey.wasPressedThisFrame) UsingPad = false;
            var m = Mouse.current;
            if (m != null && (m.delta.ReadValue().sqrMagnitude > 9f || m.leftButton.wasPressedThisFrame || m.rightButton.wasPressedThisFrame))
            {
                UsingPad = false;
                nav.NavMode = false;
            }
        }

        static bool PadActivity(Gamepad g)
        {
            return g.buttonSouth.wasPressedThisFrame || g.buttonEast.wasPressedThisFrame || g.buttonNorth.wasPressedThisFrame ||
                   g.buttonWest.wasPressedThisFrame || g.startButton.wasPressedThisFrame || g.selectButton.wasPressedThisFrame ||
                   g.leftShoulder.wasPressedThisFrame || g.rightShoulder.wasPressedThisFrame ||
                   g.leftTrigger.ReadValue() > 0.5f || g.rightTrigger.ReadValue() > 0.5f ||
                   g.dpad.ReadValue().sqrMagnitude > 0.25f ||
                   g.leftStick.ReadValue().sqrMagnitude > 0.2f || g.rightStick.ReadValue().sqrMagnitude > 0.2f;
        }

        /// <summary>Kierunek z D-pada, lewej gałki lub strzałek, zaokrąglony do jednej osi (Y w dół, jak w GUI).</summary>
        static Vector2 ReadMenuDirection()
        {
            Vector2 raw = Vector2.zero;
            var g = Gamepad.current;
            if (g != null)
            {
                Vector2 dp = g.dpad.ReadValue();
                Vector2 st = g.leftStick.ReadValue();
                raw = dp.sqrMagnitude > 0.1f ? dp : (st.magnitude > 0.55f ? st : Vector2.zero);
            }
            var kb = Keyboard.current;
            if (raw == Vector2.zero && kb != null)
            {
                if (kb.upArrowKey.isPressed) raw.y += 1;
                if (kb.downArrowKey.isPressed) raw.y -= 1;
                if (kb.leftArrowKey.isPressed) raw.x -= 1;
                if (kb.rightArrowKey.isPressed) raw.x += 1;
            }
            if (raw == Vector2.zero) return Vector2.zero;
            return Mathf.Abs(raw.x) > Mathf.Abs(raw.y) ? new Vector2(Mathf.Sign(raw.x), 0) : new Vector2(0, -Mathf.Sign(raw.y));
        }

        void HandleMenuInput(float dt)
        {
            var g = Gamepad.current;
            var kb = Keyboard.current;

            // Kierunek z powtarzaniem przy przytrzymaniu.
            Vector2 dir = ReadMenuDirection();
            if (dir != Vector2.zero)
            {
                bool fresh = dir != heldDir;
                if (fresh) repeatTimer = RepeatDelay;
                else repeatTimer -= dt;
                if (fresh || repeatTimer <= 0f)
                {
                    if (!fresh) repeatTimer = RepeatRate;
                    if (!nav.NavMode) nav.NavMode = true; // pierwsze wychylenie tylko pokazuje fokus
                    else nav.Move(dir);
                }
            }
            heldDir = dir;

            bool confirm = (g != null && g.buttonSouth.wasPressedThisFrame) ||
                           (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame));
            if (confirm)
            {
                if (!nav.NavMode) nav.NavMode = true; // pierwsze naciśnięcie pokazuje fokus, nie zatwierdza na ślepo
                else nav.RequestActivate();
            }

            bool back = (g != null && g.buttonEast.wasPressedThisFrame) || (kb != null && kb.backspaceKey.wasPressedThisFrame);
            if (back) Back();

            // Odpowiednik klawisza R na ekranie śmierci.
            if (g != null && root.Screen == GameScreen.Death && g.buttonNorth.wasPressedThisFrame) root.RetrySameLoadout();

            // Prawa gałka przewija panele z samym tekstem (opisy odblokowań, statystyki).
            if (g != null)
            {
                float sy = g.rightStick.ReadValue().y;
                if (Mathf.Abs(sy) > 0.2f)
                {
                    var target = root.Screen == GameScreen.Unlocks ? scrollA : root.Screen == GameScreen.Equipment ? scrollC : null;
                    if (target != null) target.pos.y = Mathf.Max(0, target.pos.y - sy * 900f * dt);
                }
            }
        }

        /// <summary>Przycisk "wstecz" (B / Backspace) zależnie od ekranu.</summary>
        public void Back()
        {
            switch (root.Screen)
            {
                case GameScreen.Unlocks:
                case GameScreen.Loadout:
                case GameScreen.Victory:
                    root.ShowMainMenu();
                    break;
                case GameScreen.Equipment:
                    root.CloseEquipment();
                    break;
                case GameScreen.Controls:
                    root.CloseControls();
                    break;
                case GameScreen.Paused:
                    root.TogglePause();
                    break;
            }
        }

        string Prompt(string keyboard, string pad) => UsingPad ? pad : keyboard;

        // ================================================================== Kontrolki z obsługą fokusu

        void Btn(string text, GUIStyle st, Action onClick, params GUILayoutOption[] opts)
        {
            bool clicked = GUILayout.Button(text, st, opts);
            Rect r = GUILayoutUtility.GetLastRect();
            int id = nav.Register(r);
            DrawFocus(id, r);
            if (clicked || nav.ConsumeActivation(id)) nav.Enqueue(onClick);
        }

        void BtnRect(Rect r, string text, GUIStyle st, Action onClick)
        {
            bool clicked = GUI.Button(r, text, st);
            int id = nav.Register(r);
            DrawFocus(id, r);
            if (clicked || nav.ConsumeActivation(id)) nav.Enqueue(onClick);
        }

        void Tog(bool value, string text, GUIStyle st, Action<bool> onChange, params GUILayoutOption[] opts)
        {
            bool now = GUILayout.Toggle(value, text, st, opts);
            Rect r = GUILayoutUtility.GetLastRect();
            int id = nav.Register(r);
            DrawFocus(id, r);
            if (now != value) nav.Enqueue(() => onChange(now));
            else if (nav.ConsumeActivation(id)) nav.Enqueue(() => onChange(!value));
        }

        void DrawFocus(int id, Rect r)
        {
            if (Event.current.type != EventType.Repaint || !nav.IsFocused(id)) return;
            var prev = GUI.color;
            GUI.color = new Color(1f, 0.82f, 0.3f);
            const float t = 3f;
            GUI.DrawTexture(new Rect(r.x - t, r.y - t, r.width + 2 * t, t), white);
            GUI.DrawTexture(new Rect(r.x - t, r.yMax, r.width + 2 * t, t), white);
            GUI.DrawTexture(new Rect(r.x - t, r.y, t, r.height), white);
            GUI.DrawTexture(new Rect(r.xMax, r.y, t, r.height), white);
            GUI.color = prev;
        }

        void BeginScroll(ScrollState s, float viewHeight)
        {
            s.viewHeight = viewHeight;
            nav.PushScroll(s);
            s.pos = GUILayout.BeginScrollView(s.pos);
        }

        void EndScroll()
        {
            GUILayout.EndScrollView();
            nav.PopScroll();
        }

        void DrawNavHints()
        {
            if (!UsingPad && !nav.NavMode) return;
            var s = root.Screen;
            bool hasBack = s == GameScreen.Unlocks || s == GameScreen.Loadout || s == GameScreen.Victory || s == GameScreen.Equipment || s == GameScreen.Paused || s == GameScreen.Controls;
            string text;
            if (UsingPad)
            {
                text = "D-pad / gałka: wybór   (A) zatwierdź" + (hasBack ? "   (B) wstecz" : "");
                if (s == GameScreen.Death) text += "   (Y) spróbuj ponownie";
                if (s == GameScreen.Unlocks || s == GameScreen.Equipment) text += "   prawa gałka: przewiń";
            }
            else text = "Strzałki: wybór   Enter: zatwierdź" + (hasBack ? "   Backspace: wstecz" : "");
            var st = new GUIStyle(center) { fontSize = 18 };
            st.normal.textColor = new Color(1f, 0.85f, 0.45f);
            GUI.Label(new Rect(0, H - 34, W, 30), text, st);
        }

        void InitStyles()
        {
            if (title != null) return;
            white = Texture2D.whiteTexture;
            title = new GUIStyle(GUI.skin.label) { fontSize = 64, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            title.normal.textColor = new Color(0.9f, 0.8f, 0.6f);
            header = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, wordWrap = true };
            header.normal.textColor = new Color(0.95f, 0.88f, 0.7f);
            label = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, richText = true };
            label.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            small = new GUIStyle(label) { fontSize = 16 };
            rich = new GUIStyle(label) { richText = true };
            box = new GUIStyle(GUI.skin.box) { padding = new RectOffset(14, 14, 12, 12) };
            box.normal.background = MakeTex(new Color(0.08f, 0.07f, 0.09f, 0.88f));
            button = new GUIStyle(GUI.skin.button) { fontSize = 20, wordWrap = true, padding = new RectOffset(10, 10, 8, 8) };
            center = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
            bigCenter = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = 40, fontStyle = FontStyle.Bold };
        }

        static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        /// <summary>Liczba przebiegów Repaint (diagnostyka/testy).</summary>
        public int RepaintCount { get; private set; }

        void OnGUI()
        {
            if (Event.current.type == EventType.Repaint) RepaintCount++;
            nav.BeginGUI();
            try { DrawScreens(); }
            finally { nav.EndGUI(); }
        }

        void DrawScreens()
        {
            InitStyles();
            float scale = UnityEngine.Screen.height / RefHeight;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));

            switch (root.Screen)
            {
                case GameScreen.MainMenu: DrawMainMenu(); break;
                case GameScreen.Unlocks: DrawUnlocks(); break;
                case GameScreen.Loadout: DrawLoadout(); break;
                case GameScreen.Playing: DrawHud(); break;
                case GameScreen.Paused: DrawHud(); DrawPause(); break;
                case GameScreen.Reward: DrawReward(); break;
                case GameScreen.Intermission: DrawIntermission(); break;
                case GameScreen.Equipment: DrawEquipment(); break;
                case GameScreen.Controls: DrawControls(); break;
                case GameScreen.Death: DrawHud(); DrawDeath(); break;
                case GameScreen.Victory: DrawVictory(); break;
            }
            if (root.Screen != GameScreen.Playing) DrawNavHints();
        }

        // ================================================================== Menu główne

        void DrawMainMenu()
        {
            var p = root.Meta.Profile;
            GUI.Label(new Rect(0, 90, W, 90), "TURRIS DAMNATORUM", title);
            GUI.Label(new Rect(0, 175, W, 40), "Wieża Potępionych – prototyp souls-like / rogue-lite", center);

            var r = new Rect(W / 2 - 300, 260, 600, 560);
            GUILayout.BeginArea(r, box);
            GUILayout.Label($"Popiół (waluta trwała): <b>{p.ash}</b>", label);
            GUILayout.Label($"Najwyższe piętro: {p.bestFloor}/{root.config.tower.floors.Count}   Podejścia: {p.totalRuns}   Zwycięstwa: {p.totalVictories}", small);
            if (root.Meta.Warning != null) GUILayout.Label("<color=#ff8866>" + root.Meta.Warning + "</color>", small);
            GUILayout.Space(16);
            Btn("Nowe podejście", button, root.ShowLoadout, GUILayout.Height(56));
            if (p.totalRuns > 0)
                Btn("Szybki start (ostatni zestaw)", button, () =>
                {
                    if (!root.TryStartRun(root.Meta.LastPlan(), out var reason)) status = reason;
                }, GUILayout.Height(48));
            Btn("Odblokowania", button, root.ShowUnlocks, GUILayout.Height(48));
            Btn("Sterowanie", button, root.ShowControls, GUILayout.Height(44));
            Btn("Wyjdź", button, Application.Quit, GUILayout.Height(40));
            GUILayout.Space(10);
            if (status != null) GUILayout.Label(status, small);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Zapis: {root.ProfilePath}", small);
            GUILayout.Label("[F9] deweloperskie +100 popiołu (do testów odblokowań)", small);
            GUILayout.EndArea();
        }

        // ================================================================== Odblokowania

        void DrawUnlocks()
        {
            var meta = root.Meta;
            GUI.Label(new Rect(40, 30, W - 80, 50), $"Odblokowania – popiół: {meta.Profile.ash}", header);
            GUILayout.BeginArea(new Rect(40, 90, W - 80, H - 200), box);
            BeginScroll(scrollA, H - 230);
            foreach (var group in root.config.unlocks.GroupBy(u => u.kind))
            {
                GUILayout.Label(KindTitle(group.Key), header);
                foreach (var u in group)
                {
                    GUILayout.BeginHorizontal();
                    bool unlocked = meta.IsUnlocked(u);
                    string costText = u.kind == UnlockKind.Difficulty ? "" : u.kind == UnlockKind.Spell ? "  (na stałe – w kolekcji każdego podejścia)" : $"  (koszt w budżecie: {u.loadoutCost})";
                    GUILayout.Label($"<b>{u.displayName}</b>{costText}\n<size=15>{UnlockDetails(u)}</size>", label, GUILayout.Width(W - 420));
                    if (unlocked) GUILayout.Label("<color=#88dd88>Odblokowane</color>", label, GUILayout.Width(250));
                    else
                    {
                        bool can = meta.CanPurchase(u, out var reason);
                        GUI.enabled = can;
                        Btn(can ? $"Odblokuj ({u.ashCost})" : reason, button, () => meta.Purchase(u), GUILayout.Width(250));
                        GUI.enabled = true;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(6);
                }
            }
            EndScroll();
            GUILayout.EndArea();
            BtnRect(new Rect(40, H - 100, 240, 50), "Wróć", button, root.ShowMainMenu);
        }

        static string KindTitle(UnlockKind k)
        {
            switch (k)
            {
                case UnlockKind.StartingItem: return "Przedmioty startowe (trafiają też do puli nagród)";
                case UnlockKind.Spell: return "Umiejętności – czary i techniki (na stałe, dla każdej postaci; przed odblokowaniem można je zdobyć tylko na jedno podejście)";
                case UnlockKind.Boon: return "Talenty i wzmocnienia";
                default: return "Poziomy trudności";
            }
        }

        string UnlockDetails(UnlockDefinition u)
        {
            var b = root.config.balance;
            switch (u.target)
            {
                case ItemDefinition i: return Describe.Item(new ItemInstance(i), null, b).Replace("\n", " · ");
                case SpellDefinition s: return Describe.Spell(s, 0, null, b).Replace("\n", " · ");
                case BoonDefinition bo: return Describe.Boon(bo, 0).Replace("\n", " · ");
                case DifficultyDefinition d: return string.Join(" · ", d.DescribeModifiers()) + " | Nagrody: " + string.Join(", ", d.DescribeRewards());
            }
            return u.description;
        }

        // ================================================================== Przygotowanie

        void DrawLoadout()
        {
            var plan = root.Plan;
            var meta = root.Meta;
            var cfg = root.config;
            GUI.Label(new Rect(40, 20, W - 80, 50), "Przygotowanie podejścia", header);

            float colW = (W - 120) / 3f;
            // --- Klasa
            GUILayout.BeginArea(new Rect(40, 80, colW, H - 200), box);
            GUILayout.Label("Klasa startowa", header);
            GUILayout.Label("<size=15>Klasa określa tylko atrybuty, wyposażenie i umiejętności na start. Później każda postać może używać dowolnej broni, pancerza i umiejętności.</size>", label);
            foreach (var c in cfg.classes)
                Tog(plan.classDef == c, $"  {c.displayName}", button, on => { if (on) plan.classDef = c; }, GUILayout.Height(44));
            if (plan.classDef != null)
            {
                var c = plan.classDef;
                GUILayout.Space(8);
                GUILayout.Label(c.description, small);
                GUILayout.Label($"Siła {c.strength} · Zręczność {c.dexterity} · Inteligencja {c.intelligence} · Wytrzymałość {c.toughness}\n<size=14>Po każdym piętrze: +1 do wybranej cechy.</size>", small);
                GUILayout.Label("Wyposażenie: " + string.Join(", ", c.startingItems.Select(i => i.displayName)), small);
                if (c.startingSpells.Count > 0) GUILayout.Label("Umiejętności klasy: " + string.Join(", ", c.startingSpells.Select(s => s.displayName)), small);
                GUILayout.Label($"Flaszki: życia {c.healthFlasks}, many {c.manaFlasks}", small);
            }
            GUILayout.EndArea();

            // --- Trudność
            GUILayout.BeginArea(new Rect(60 + colW, 80, colW, H - 200), box);
            GUILayout.Label("Poziom trudności", header);
            BeginScroll(scrollB, H - 280);
            foreach (var d in cfg.difficulties)
            {
                bool avail = meta.IsDifficultyAvailable(d);
                GUI.enabled = avail;
                Tog(plan.difficulty == d, $"  {d.tier}. {d.displayName}{(avail ? "" : " (zablokowany)")}", button,
                    on => { if (on && avail) plan.difficulty = d; }, GUILayout.Height(40));
                GUI.enabled = true;
                var mods = d.DescribeModifiers();
                GUILayout.Label("<size=15>" + d.description + "\nUtrudnienia: " + (mods.Count == 0 ? "brak" : string.Join("; ", mods)) +
                                "\n<color=#e8c070>Premie: " + string.Join("; ", d.DescribeRewards()) + "</color></size>", label);
                GUILayout.Space(6);
            }
            EndScroll();
            GUILayout.EndArea();

            // --- Dodatki
            GUILayout.BeginArea(new Rect(80 + colW * 2, 80, colW, H - 200), box);
            int budget = cfg.balance.loadoutBudget;
            GUILayout.Label($"Budżet przygotowania: {plan.Cost}/{budget}", header);
            GUILayout.Label("<size=15>Wybierz odblokowane przedmioty i talenty. Budżet nie pozwala zabrać wszystkiego naraz.</size>", label);
            BeginScroll(scrollC, H - 340);
            DrawLoadoutSkills(plan);
            GUILayout.Label("Dodatki", header);
            foreach (var u in meta.LoadoutOptions)
            {
                bool on = plan.extras.Contains(u);
                bool affordable = on || plan.Cost + u.loadoutCost <= budget;
                GUI.enabled = affordable;
                Tog(on, $"  [{u.loadoutCost}] {u.displayName} – {KindShort(u.kind)}", button, now =>
                {
                    if (now && !plan.extras.Contains(u) && plan.Cost + u.loadoutCost <= budget) plan.extras.Add(u);
                    else if (!now) plan.extras.Remove(u);
                });
                GUI.enabled = true;
            }
            EndScroll();
            GUILayout.EndArea();

            BtnRect(new Rect(40, H - 105, 240, 60), "Wróć", button, root.ShowMainMenu);
            BtnRect(new Rect(W - 400, H - 105, 360, 60), "Wejdź do wieży", button, () =>
            {
                if (!root.TryStartRun(plan, out var reason)) status = reason; else status = null;
            });
            if (status != null) GUI.Label(new Rect(300, H - 95, W - 720, 40), status, label);
        }

        static string KindShort(UnlockKind k) => k == UnlockKind.StartingItem ? "przedmiot" : k == UnlockKind.Spell ? "umiejętność" : "talent";

        /// <summary>Podpowiedź przycisku akcji z bieżącego układu: „[Q]” na klawiaturze, „(A)” na padzie.</summary>
        string K(string action) => root.Input == null ? "?" : UsingPad ? $"({root.Input.Display(action, true)})" : $"[{root.Input.Display(action, false)}]";
        string SkillButton(int slot) => root.Input == null ? (slot + 1).ToString() : root.Input.Display("Skill" + (slot + 1), UsingPad);
        /// <summary>Przycisk slotu: zaznaczony wyróżniony nawiasami, by było widać przypisanie.</summary>
        string SlotToggleText(int slot, bool on) => on ? $"[{SkillButton(slot)}]" : SkillButton(slot);

        /// <summary>Sloty umiejętności na ekranie przygotowania: umiejętności klasy + odblokowane na stałe.</summary>
        void DrawLoadoutSkills(LoadoutPlan plan)
        {
            var skills = plan.StartingSkills.ToList();
            GUILayout.Label("Umiejętności – 3 sloty", header);
            if (skills.Count == 0) { GUILayout.Label("Brak – odblokuj umiejętności za popiół.", small); return; }
            GUILayout.Label("<size=15>Klasowe i odblokowane na stałe. Przypisz do przycisku (ponownie – zdejmij); puste sloty wypełnią się same.</size>", label);
            for (int i = 0; i < RunState.SkillSlotCount; i++)
            {
                var s = plan.skillSlots[i];
                GUILayout.Label($"<b>[{SkillButton(i)}]</b> {(s != null && skills.Contains(s) ? s.displayName : "<color=#999999>automatycznie</color>")}", label);
            }
            foreach (var skill in skills)
            {
                var sk = skill;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{sk.displayName} <size=14>({Names.SkillCategory(sk.category)})</size>", label);
                for (int i = 0; i < RunState.SkillSlotCount; i++)
                {
                    int slot = i;
                    // Ponowny wybór tego samego przycisku zdejmuje umiejętność ze slotu.
                    Tog(plan.skillSlots[slot] == sk, SlotToggleText(slot, plan.skillSlots[slot] == sk), button, on => { if (on) plan.AssignSlot(sk, slot); else plan.ClearSlot(slot); }, GUILayout.Width(54));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
        }

        /// <summary>Sloty umiejętności w HUD: przycisk, nazwa, koszt, odnowienie i ładunki.</summary>
        void DrawSkillSlots(PlayerCombat pc, float x, float y)
        {
            const float w = 300, h = 66;
            for (int i = 0; i < RunState.SkillSlotCount; i++)
            {
                var r = new Rect(x + i * (w + 10), y, w, h);
                var sk = pc.Skill(i);
                GUI.color = new Color(0, 0, 0, 0.65f);
                GUI.DrawTexture(r, white);
                string head, sub = "";
                if (sk == null) head = $"<b>{K("Skill" + (i + 1))}</b> <color=#888888>pusty slot</color>";
                else
                {
                    var inst = sk.instance;
                    bool usable = pc.SkillUsable(i);
                    if (!inst.Ready)
                    {
                        // Odnowienie: szara zasłona cofająca się w miarę ładowania.
                        GUI.color = new Color(0.25f, 0.25f, 0.3f, 0.85f);
                        GUI.DrawTexture(new Rect(r.x, r.y, r.width * (1f - inst.RechargeProgress), r.height), white);
                    }
                    var c = sk.Def.color; if (!usable) c *= 0.45f; c.a = 1f;
                    GUI.color = c;
                    GUI.DrawTexture(new Rect(r.x, r.y, 6, r.height), white);
                    head = $"<b>{K("Skill" + (i + 1))}</b> <b>{inst.Name}</b>";
                    string cost = sk.Def.IsSpell ? $"{sk.manaCost:0} many" : $"{sk.staminaCost:0} wytrz.";
                    string charges = inst.MaxCharges > 1 ? $" · ładunki {inst.Charges}/{inst.MaxCharges}" : "";
                    string state = !sk.equipmentMet ? $"<color=#ff9966>{Names.Requirement(sk.Def.requiredTags)}</color>"
                        : inst.Ready ? "<color=#99dd99>gotowa</color>" : $"{inst.RechargeRemaining:0.0}s";
                    sub = $"<size=15>{cost} · {state}{charges}</size>";
                }
                GUI.color = Color.white;
                GUI.Label(new Rect(r.x + 14, r.y + 4, r.width - 18, 30), head, rich);
                GUI.Label(new Rect(r.x + 14, r.y + 34, r.width - 18, 28), sub, rich);
            }
        }

        // ================================================================== HUD

        void DrawHud()
        {
            var pc = root.Player;
            var run = root.Run;
            if (pc == null || run == null || pc.Build == null) return;
            var cam = root.CameraRig.Camera;

            // Paski zasobów
            float x = 40, y = 36;
            Bar(new Rect(x, y, 16 + pc.Health.Max * 0.9f, 22), pc.Health.Fraction, new Color(0.75f, 0.12f, 0.12f), $"{pc.Health.Current:0}/{pc.Health.Max:0}");
            Bar(new Rect(x, y + 28, 16 + pc.Mana.Max * 1.6f, 16), pc.Mana.Fraction, new Color(0.2f, 0.35f, 0.85f), $"{pc.Mana.Current:0}/{pc.Mana.Max:0}");
            Bar(new Rect(x, y + 50, 16 + pc.Stamina.Max * 2.5f, 16), pc.Stamina.Fraction, new Color(0.2f, 0.65f, 0.25f), null);
            if (pc.BarrierAmount > 0)
                Bar(new Rect(x, y + 72, 16 + pc.BarrierAmount * 1.5f, 14), 1f, new Color(0.8f, 0.66f, 0.42f), $"Osłona {pc.BarrierAmount:0} ({pc.BarrierTime:0}s)");
            StatusLabels(pc.Status, x, y + (pc.BarrierAmount > 0 ? 92 : 74), 18, false);
            var buffs = new List<string>();
            if (pc.DamageBuffTime > 0) buffs.Add($"<color=#ff7755>Okrzyk +{pc.DamageBuffPct:0}% ({pc.DamageBuffTime:0}s)</color>");
            if (pc.FreeCasts > 0) buffs.Add($"<color=#ff5566>Pakt: {pc.FreeCasts} czary bez many</color>");
            if (pc.FrostArmorTime > 0) buffs.Add($"<color=#99ddff>Mroźna zbroja ({pc.FrostArmorTime:0}s)</color>");
            if (pc.ShockCharged) buffs.Add("<color=#eeee88>Ładunek burzy</color>");
            if (pc.WeaponBuffTime > 0) buffs.Add($"<color=#{ColorUtility.ToHtmlStringRGB(pc.WeaponBuffColor)}>Zaklęta broń ({pc.WeaponBuffTime:0}s)</color>");
            if (buffs.Count > 0) GUI.Label(new Rect(x, y + (pc.BarrierAmount > 0 ? 116 : 98), 900, 26), "<size=16><b>" + string.Join("   ", buffs) + "</b></size>", rich);

            // Flaszki i sloty umiejętności
            GUI.Label(new Rect(40, H - 200, 900, 30), $"<b>{K("FlaskHealth")}</b> Flaszka życia: {run.healthFlasks}/{run.MaxHealthFlasks}    <b>{K("FlaskMana")}</b> Flaszka many: {run.manaFlasks}/{run.MaxManaFlasks}", rich);
            DrawSkillSlots(pc, 40, H - 164);
            string defense = pc.Build.CanBlock ? (pc.Build.guardIsShield ? "Blok: tarcza" : "Blok: broń") : "Blok: brak";
            defense += pc.Build.CanParry ? " · Parowanie: tak" : " · Parowanie: brak";
            GUI.Label(new Rect(40, H - 76, 900, 30), $"Broń: {pc.Build.mainHand.Name}{(pc.Build.weaponEffectiveness < 1 ? " (niespełnione wymagania)" : "")} · {defense}" +
                                                     (pc.WeaponBuffTime > 0 ? $" · Zaklęte ostrze {pc.WeaponBuffTime:0}s" : ""), small);

            // Piętro, dusze
            var floor = root.CurrentFloor;
            GUI.Label(new Rect(W - 520, 30, 480, 30), $"{floor?.name}  ({run.floorIndex + 1}/{root.config.tower.floors.Count})", header);
            if (floor != null && floor.WaveCount > 1)
                GUI.Label(new Rect(W - 520, 96, 480, 30), $"<b>Fala {root.WaveIndex + 1}/{floor.WaveCount}</b> · wrogowie: {root.Enemies.Count(e => e != null && !e.IsDead)}", rich);
            GUI.Label(new Rect(W - 520, 70, 480, 30), $"Dusze: {run.souls}   Popiół w podejściu: {run.ashEarned}   Trudność: {run.difficulty.displayName}", small);

            // Wrogowie: paski, telegrafy
            foreach (var e in root.Enemies)
            {
                if (e == null || e.IsDead) continue;
                Vector3 sp = cam.WorldToScreenPoint(e.transform.position + Vector3.up * (2.2f * e.Def.scale));
                if (sp.z <= 0) continue;
                float sx = sp.x / UnityEngine.Screen.height * RefHeight;
                float sy = RefHeight - sp.y / UnityEngine.Screen.height * RefHeight;
                if (!e.Def.isBoss)
                {
                    Bar(new Rect(sx - 60, sy, 120, 8), e.Health.Fraction, new Color(0.7f, 0.1f, 0.1f), null);
                    Bar(new Rect(sx - 60, sy + 10, 120, 4), e.Poise.Fraction, new Color(0.85f, 0.75f, 0.3f), null);
                    StatusLabels(e.Status, sx - 60, sy + 16, 15, true);
                }
                if (e.IsTelegraphing)
                {
                    var atk = e.CurrentAttack.attack;
                    var kind = atk.Telegraph;
                    var col = TelegraphColors.For(kind);
                    string tag = Names.Telegraph(kind);
                    var st = new GUIStyle(center) { fontSize = kind == TelegraphKind.Normal ? 22 : 26, fontStyle = FontStyle.Bold };
                    st.normal.textColor = col;
                    GUI.Label(new Rect(sx - 200, sy - 60, 400, 30), kind == TelegraphKind.Normal ? "!" : "!! " + tag + " !!", st);
                    var st2 = new GUIStyle(center) { fontSize = 16 };
                    st2.normal.textColor = col;
                    GUI.Label(new Rect(sx - 200, sy - 32, 400, 24), atk.name, st2);
                }
                if (e.CanBeRiposted)
                {
                    var st = new GUIStyle(center) { fontSize = 22, fontStyle = FontStyle.Bold };
                    st.normal.textColor = new Color(1f, 0.9f, 0.3f);
                    GUI.Label(new Rect(sx - 200, sy - 60, 400, 30), "RIPOSTA – " + K("Light") + " z bliska", st);
                }
                if (root.Controller.LockTarget == e)
                {
                    Vector3 lp = cam.WorldToScreenPoint(e.AimPoint);
                    if (lp.z > 0)
                    {
                        float lx = lp.x / UnityEngine.Screen.height * RefHeight, ly = RefHeight - lp.y / UnityEngine.Screen.height * RefHeight;
                        GUI.color = new Color(1f, 0.95f, 0.8f);
                        GUI.DrawTexture(new Rect(lx - 6, ly - 6, 12, 12), white);
                        GUI.color = Color.white;
                    }
                }
            }

            var boss = root.Enemies.FirstOrDefault(e => e != null && e.Def.isBoss && !e.IsDead);
            if (boss != null)
            {
                // Na górze ekranu – dół zajmują sloty umiejętności.
                GUI.Label(new Rect(W / 2 - 400, 104, 800, 30), boss.DisplayName + (boss.PhaseIndex > 0 ? "  – faza II" : ""), center);
                Bar(new Rect(W / 2 - 400, 134, 800, 18), boss.Health.Fraction, new Color(0.65f, 0.08f, 0.08f), null);
                Bar(new Rect(W / 2 - 400, 156, 800, 5), boss.Poise.Fraction, new Color(0.85f, 0.75f, 0.3f), null);
                StatusLabels(boss.Status, W / 2 - 400, 164, 17, false);
                string aff = Affinities(boss.Def);
                if (aff != null) GUI.Label(new Rect(W / 2 + 100, 104, 300, 30), $"<size=15>{aff}</size>", rich);
            }

            // Liczby obrażeń
            foreach (var f in floating)
            {
                Vector3 sp = cam.WorldToScreenPoint(f.pos);
                if (sp.z <= 0) continue;
                float sx = sp.x / UnityEngine.Screen.height * RefHeight;
                float sy = RefHeight - sp.y / UnityEngine.Screen.height * RefHeight;
                var st = new GUIStyle(center) { fontSize = 22, fontStyle = FontStyle.Bold };
                var c = f.color; c.a = 1f - f.t / 1.1f;
                st.normal.textColor = c;
                GUI.Label(new Rect(sx - 150, sy - 15, 300, 30), f.text, st);
            }

            // Komunikaty
            float my = 190;
            foreach (var m in messages)
            {
                var st = new GUIStyle(bigCenter);
                var c = m.color; c.a = Mathf.Clamp01(2.2f - m.t);
                st.normal.textColor = c;
                GUI.Label(new Rect(0, my, W, 50), m.text, st);
                my += 50;
            }

            if (showHelp)
            {
                string controls = $"<b>Sterowanie{(UsingPad ? " – pad" : "")}</b> [{(UsingPad ? "Select" : "F1")} ukryj · zmiana: menu → Sterowanie]\n" +
                    (UsingPad ? "Lewa gałka ruch · Prawa gałka kamera\n" : "WASD ruch · Mysz kamera\n") +
                    $"{K("Sprint")} bieg · {K("Light")} szybki atak / riposta · {K("Heavy")} mocny atak\n" +
                    $"{K("Block")} blok · {K("Parry")} parowanie · {K("Dodge")} unik (przerywa atak)\n" +
                    $"{K("Skill1")} {K("Skill2")} {K("Skill3")} umiejętności 1–3\n" +
                    $"{K("FlaskHealth")} flaszka życia · {K("FlaskMana")} flaszka many\n" +
                    $"{K("LockOn")} namierzanie · {K("SwitchLeft")} / {K("SwitchRight")} zmiana celu\n" +
                    (UsingPad ? "(Start) pauza" : "[Esc] pauza");
                GUI.Label(new Rect(W - 470, H - 330, 450, 320),
                    "<size=15>" + controls + "\n\n<b>Sygnały ataków</b>\n<color=#e6e6e6>biały</color> zwykły · <color=#ff8c1a>pomarańczowy</color> ciężki\n<color=#bf59ff>fiolet</color> nie do sparowania · <color=#ff1a1a>czerwony</color> nie do zablokowania\n<color=#ff33cc>różowy</color> obszarowy – unik nie chroni, uciekaj lub blokuj</size>", rich);
            }
        }

        /// <summary>Kolorowe etykiety aktywnych efektów (krwawienie, płonie, chłód, porażony, zamrożony).</summary>
        void StatusLabels(StatusEffects s, float x, float y, int size, bool compact)
        {
            if (s == null || !s.Any) return;
            var parts = s.Describe().Select(d => $"<color=#{ColorUtility.ToHtmlStringRGB(Names.StatusColor(d.kind))}>{(compact ? d.text : d.text.ToUpperInvariant())}</color>");
            GUI.Label(new Rect(x, y, 600, size + 10), $"<size={size}><b>{string.Join("  ", parts)}</b></size>", rich);
        }

        /// <summary>Słabości i odporności na żywioły, np. "słaby: ogień · odporny: mróz".</summary>
        static string Affinities(EnemyDefinition d)
        {
            var weak = new List<string>(); var strong = new List<string>();
            foreach (Element e in new[] { Element.Fire, Element.Frost, Element.Lightning })
            {
                float m = d.ElementMultiplier(e);
                if (m > 1.01f) weak.Add(Names.Element(e)); else if (m < 0.99f) strong.Add(Names.Element(e));
            }
            if (d.bleedMultiplier < 0.99f) strong.Add("krwawienie");
            if (weak.Count == 0 && strong.Count == 0) return null;
            string t = weak.Count > 0 ? "<color=#ffb070>słaby: " + string.Join(", ", weak) + "</color>" : "";
            if (strong.Count > 0) t += (t.Length > 0 ? " · " : "") + "<color=#a0c8ff>odporny: " + string.Join(", ", strong) + "</color>";
            return t;
        }

        void Bar(Rect r, float fraction, Color c, string text)
        {
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(r, white);
            GUI.color = c;
            GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, (r.width - 4) * Mathf.Clamp01(fraction), r.height - 4), white);
            GUI.color = Color.white;
            if (text != null) GUI.Label(new Rect(r.x + 8, r.y - 3, r.width, r.height + 6), text, small);
        }

        // ================================================================== Pauza / śmierć / zwycięstwo

        void DrawPause()
        {
            GUILayout.BeginArea(new Rect(W / 2 - 220, H / 2 - 170, 440, 340), box);
            GUILayout.Label("Pauza", header);
            Btn("Wznów", button, root.TogglePause, GUILayout.Height(50));
            Btn("Sterowanie", button, root.ShowControls, GUILayout.Height(50));
            Btn("Porzuć podejście (liczy się jak śmierć)", button, root.AbandonRun, GUILayout.Height(50));
            GUILayout.EndArea();
        }

        void DrawDeath()
        {
            var run = root.Run;
            GUILayout.BeginArea(new Rect(W / 2 - 330, H / 2 - 230, 660, 460), box);
            var st = new GUIStyle(bigCenter); st.normal.textColor = new Color(0.85f, 0.15f, 0.15f);
            GUILayout.Label("POLEGŁEŚ", st);
            GUILayout.Label($"Dotarłeś na piętro {run.floorIndex + 1}: {root.CurrentFloor?.name}. Pokonani wrogowie: {run.kills}.", label);
            GUILayout.Label($"<color=#e8c070>Zachowujesz: popiół zdobyty w podejściu ({run.ashEarned}) oraz wszystkie odblokowania.</color>", label);
            GUILayout.Label($"<color=#aaaaaa>Tracisz: dusze ({run.souls}), znalezione przedmioty, wzmocnienia i ulepszenia czarów.</color>", label);
            GUILayout.Space(12);
            Btn($"Spróbuj ponownie – ten sam zestaw  {Prompt("[R]", "(Y)")}", button, root.RetrySameLoadout, GUILayout.Height(60));
            Btn("Zmień przygotowanie", button, () => { root.ShowMainMenu(); root.ShowLoadout(); }, GUILayout.Height(46));
            Btn("Menu główne", button, root.ShowMainMenu, GUILayout.Height(40));
            GUILayout.EndArea();
        }

        void DrawVictory()
        {
            var run = root.Run;
            GUILayout.BeginArea(new Rect(W / 2 - 330, H / 2 - 220, 660, 440), box);
            var st = new GUIStyle(bigCenter); st.normal.textColor = new Color(0.95f, 0.8f, 0.4f);
            GUILayout.Label("WIEŻA ZDOBYTA", st);
            GUILayout.Label($"Trudność: {run.difficulty.displayName}. Pokonani wrogowie: {run.kills}.", label);
            GUILayout.Label($"Popiół zdobyty w podejściu: <b>{run.ashEarned}</b> (w tym premia za zwycięstwo).", label);
            GUILayout.Label($"Łącznie popiołu: {root.Meta.Profile.ash}. Wyższe poziomy trudności mogą być teraz dostępne w Odblokowaniach.", label);
            GUILayout.Space(12);
            Btn("Menu główne", button, root.ShowMainMenu, GUILayout.Height(56));
            Btn("Kolejne podejście", button, () => { root.ShowMainMenu(); root.ShowLoadout(); }, GUILayout.Height(46));
            GUILayout.EndArea();
        }

        // ================================================================== Nagrody i przerwa

        void DrawReward()
        {
            var run = root.Run;
            var pc = root.Player;
            GUI.Label(new Rect(0, 60, W, 50), root.LastFloorSummary, bigCenter);
            GUI.Label(new Rect(0, 115, W, 30), UsingPad ? "Wybierz jedną nagrodę (D-pad / gałka, A)" : "Wybierz jedną nagrodę (klawisze 1–3)", center);
            int n = root.Rewards.Count;
            float cardW = 460, gap = 30;
            float startX = W / 2 - (n * cardW + (n - 1) * gap) / 2;
            for (int i = 0; i < n; i++)
            {
                int idx = i;
                var o = root.Rewards[i];
                var r = new Rect(startX + i * (cardW + gap), 180, cardW, 620);
                GUILayout.BeginArea(r, box);
                GUILayout.Label($"<size=16><color=#e8c070>{o.Category}</color></size>", rich);
                GUILayout.Label(o.Title, header);
                GUILayout.Space(8);
                GUILayout.Label(Describe.Reward(o, run, pc.Build.stats, root.config.balance), small);
                GUILayout.FlexibleSpace();
                Btn(UsingPad ? "Wybierz" : $"Wybierz [{i + 1}]", button, () => root.ChooseReward(idx), GUILayout.Height(54));
                GUILayout.EndArea();
            }
            if (n == 0) BtnRect(new Rect(W / 2 - 150, 500, 300, 60), "Dalej", button, () => root.ChooseReward(-1));
            else
            {
                int cost = SoulShop.RerollCost(run, root.config.balance);
                GUI.enabled = root.CanReroll;
                BtnRect(new Rect(W / 2 - 220, 830, 440, 54), root.RerollUsed ? "Nagrody już przerzucone" : $"Przerzuć nagrody ({cost} dusz, masz {run.souls})", button, root.RerollRewards);
                GUI.enabled = true;
            }
        }

        void DrawIntermission()
        {
            var run = root.Run;
            var pc = root.Player;
            var r = new Rect(W / 2 - 700, 140, 760, 700);
            GUILayout.BeginArea(r, box);
            GUILayout.Label(root.AtRestPoint ? "Kapliczka – punkt odpoczynku" : "Między piętrami", header);
            GUILayout.Label($"Życie {pc.Health.Current:0}/{pc.Health.Max:0} · Mana {pc.Mana.Current:0}/{pc.Mana.Max:0} · Flaszki: życia {run.healthFlasks}/{run.MaxHealthFlasks}, many {run.manaFlasks}/{run.MaxManaFlasks}", label);
            GUILayout.Label(run.difficulty.restoreOnlyAtRest && !root.AtRestPoint
                ? "<color=#ff9966>Na tej trudności życie i flaszki odnawiają się tylko w kapliczkach.</color>"
                : "Życie, mana i flaszki zostały odnowione.", label);
            GUILayout.Label($"Dusze: {run.souls}", label);
            GUILayout.Space(10);
            Btn("Ekwipunek, umiejętności i statystyki", button, root.OpenEquipment, GUILayout.Height(54));
            if (run.attributePoints > 0)
            {
                // Rozwój cechy: +1 za każde ukończone piętro.
                GUILayout.Space(8);
                GUILayout.Label($"<b>Rozwój cechy</b> – punkty: {run.attributePoints}", label);
                foreach (var a in Names.Attributes)
                {
                    var attr = a;
                    Btn($"+1 {Names.Attribute(attr)} ({run.attributes.Get(attr)} → {run.attributes.Get(attr) + 1}) – {Names.AttributeHelp(attr)}", button,
                        () => root.SpendAttributePoint(attr), GUILayout.Height(38));
                }
            }

            if (root.AtRestPoint)
            {
                GUILayout.Space(10);
                GUILayout.Label("Kapliczka (waluta podejścia – dusze):", label);
                bool can = root.CanUpgradeWeapon(out var reason);
                GUI.enabled = can;
                var w = run.equipment.Get(EquipSlot.MainHand);
                Btn(can ? $"Ulepsz broń {w.Name} → +{w.level + 1}  ({root.WeaponUpgradeCost} dusz)" : $"Ulepszenie broni: {reason}", button, root.UpgradeWeapon, GUILayout.Height(46));
                GUI.enabled = run.bonusHealthFlasks < 2 && run.souls >= root.FlaskUpgradeCost;
                Btn(run.bonusHealthFlasks >= 2 ? "Dodatkowa flaszka: limit osiągnięty" : $"Dodatkowy ładunek flaszki życia ({root.FlaskUpgradeCost} dusz)", button, root.BuyFlask, GUILayout.Height(46));
                GUI.enabled = true;
            }
            GUILayout.FlexibleSpace();
            var next = root.config.tower.floors[run.floorIndex + 1];
            GUILayout.Label($"Następne: {next.name}{(next.isBoss ? " – BOSS" : next.isDuel ? " – pojedynek" : " – grupa przeciwników")}", label);
            Btn("Wejdź wyżej", button, root.NextFloor, GUILayout.Height(60));
            GUILayout.EndArea();
            // Sklep po panelu głównym – pierwszy fokus padem zostaje na „Ekwipunek”.
            DrawSoulShop(new Rect(W / 2 + 90, 140, 610, 700));
        }

        /// <summary>Sklep dusz między piętrami: ulepszenie umiejętności i losowa nowa umiejętność na to podejście.</summary>
        void DrawSoulShop(Rect r)
        {
            var run = root.Run;
            var b = root.config.balance;
            GUILayout.BeginArea(r, box);
            GUILayout.Label($"Sklep dusz – masz {run.souls}", header);
            GUILayout.Label($"<size=15>Ceny rosną z każdym piętrem (teraz ×{SoulShop.Inflation(run, b):0.00}).</size>", label);
            BeginScroll(scrollB, 560);
            var offer = root.ShopOffer;
            if (offer != null)
            {
                int oc = SoulShop.OfferCost(run, b);
                GUILayout.Label($"<b>Oferta:</b> {offer.displayName} <size=14>({Names.SkillCategory(offer.category)}, na to podejście)</size>", label);
                GUILayout.Label("<size=14>" + Describe.Spell(offer, 0, root.Player.Build.stats, b).Replace("\n", " · ") + "</size>", label);
                GUI.enabled = run.souls >= oc;
                Btn($"Kup ({oc} dusz)", button, root.BuyShopOffer, GUILayout.Height(44));
                GUI.enabled = true;
                GUILayout.Space(8);
            }
            GUILayout.Label("<b>Ulepszenie umiejętności</b>", label);
            foreach (var s in run.knownSpells.OrderBy(x => run.SlotOf(x) < 0 ? 9 : run.SlotOf(x)))
            {
                var sk = s;
                bool can = SoulShop.CanUpgrade(sk, run, b, out var why);
                string next = "";
                foreach (var f in sk.definition.levelFeatures) if (f.level == sk.level + 1) next = $" – nowa cecha: {Names.LevelFeature(f)}";
                GUI.enabled = can;
                Btn(sk.IsMaxLevel ? $"{sk.Name} – maks. poziom" : $"{sk.Name} → +{sk.level + 1} ({SoulShop.UpgradeCost(sk, run, b)} dusz){next}",
                    button, () => root.UpgradeSkill(sk), GUILayout.Height(40));
                GUI.enabled = true;
            }
            EndScroll();
            GUILayout.EndArea();
        }

        // ================================================================== Sterowanie

        (string action, bool pad)? pendingRebind;
        string listeningLabel;

        void DrawControls()
        {
            var inp = root.Input;
            GUI.Label(new Rect(40, 30, W - 80, 50), "Sterowanie – przypisanie przycisków", header);
            GUILayout.BeginArea(new Rect(W / 2 - 560, 90, 1120, H - 210), box);
            GUILayout.Label("<size=15>Wybierz przycisk i naciśnij nowy klawisz, przycisk myszy lub pada (Esc – anuluj). Jeśli inna akcja miała ten przycisk, zamienią się miejscami. Zapis w profilu.</size>", label);
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Akcja</b>", label, GUILayout.Width(440));
            GUILayout.Label("<b>Klawiatura / mysz</b>", label, GUILayout.Width(320));
            GUILayout.Label("<b>Pad</b>", label, GUILayout.Width(320));
            GUILayout.EndHorizontal();
            BeginScroll(scrollA, H - 360);
            foreach (var (action, lbl) in PlayerInputReader.Rebindable)
            {
                string act = action, name = lbl;
                GUILayout.BeginHorizontal();
                GUILayout.Label(name, label, GUILayout.Width(440));
                bool kbWait = listeningLabel == name && pendingOrActivePad == false;
                bool padWait = listeningLabel == name && pendingOrActivePad == true;
                Btn(kbWait ? "naciśnij…" : inp.Display(act, false), button, () => BeginListen(act, false, name), GUILayout.Width(320), GUILayout.Height(40));
                Btn(padWait ? "naciśnij…" : inp.Display(act, true), button, () => BeginListen(act, true, name), GUILayout.Width(320), GUILayout.Height(40));
                GUILayout.EndHorizontal();
            }
            EndScroll();
            GUILayout.EndArea();
            BtnRect(new Rect(40, H - 100, 240, 54), "Wróć", button, root.CloseControls);
            BtnRect(new Rect(W - 420, H - 100, 380, 54), "Przywróć domyślne", button, () => { inp.ResetOverrides(); root.SaveBindings(); });
            if (listeningLabel != null)
            {
                var st = new GUIStyle(bigCenter);
                st.normal.textColor = new Color(1f, 0.85f, 0.45f);
                GUI.Label(new Rect(0, H - 170, W, 50), $"Naciśnij nowy przycisk: {listeningLabel}  (Esc – anuluj)", st);
            }
        }

        bool? pendingOrActivePad;

        /// <summary>Nasłuch zaczyna się po puszczeniu przycisku, którym go wywołano – inaczej ten sam przycisk zostałby przypisany.</summary>
        void BeginListen(string action, bool pad, string label)
        {
            if (root.Input == null || root.Input.IsRebinding) return;
            pendingRebind = (action, pad);
            pendingOrActivePad = pad;
            listeningLabel = label;
        }

        void TickPendingRebind()
        {
            if (pendingRebind == null || root.Input == null) return;
            var g = Gamepad.current; var kb = Keyboard.current; var m = Mouse.current;
            bool held = (g != null && (g.buttonSouth.isPressed || g.buttonEast.isPressed)) ||
                        (kb != null && (kb.enterKey.isPressed || kb.spaceKey.isPressed)) || (m != null && m.leftButton.isPressed);
            if (held) return;
            var (action, pad) = pendingRebind.Value;
            pendingRebind = null;
            root.Input.StartRebind(action, pad, ok =>
            {
                listeningLabel = null;
                pendingOrActivePad = null;
                if (ok) root.SaveBindings();
            });
        }

        // ================================================================== Ekwipunek

        void DrawEquipment()
        {
            var run = root.Run;
            var pc = root.Player;
            var b = root.config.balance;
            // Podgląd statystyk liczony na bieżąco, by od razu pokazać efekt zmiany.
            var snap = BuildCalculator.Compute(run, root.config);
            GUI.Label(new Rect(40, 20, W - 80, 50), "Ekwipunek", header);

            float colW = (W - 120) / 3f;
            // Sloty
            GUILayout.BeginArea(new Rect(40, 80, colW, H - 190), box);
            GUILayout.Label("Założone", header);
            BeginScroll(scrollA, H - 260);
            foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
            {
                var it = run.equipment.Get(slot);
                GUILayout.BeginHorizontal();
                string name = it != null ? it.Name : (slot == EquipSlot.OffHand && run.equipment.Get(EquipSlot.MainHand)?.definition.twoHanded == true ? "(zajęta przez broń dwuręczną)" : "—");
                GUILayout.Label($"<b>{Names.Slot(slot)}:</b> {name}", label, GUILayout.Width(colW - 150));
                if (it != null) Btn("Zdejmij", button, () => { run.UnequipToInventory(slot); pc.RefreshBuild(); }, GUILayout.Width(110));
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            GUILayout.Label("Umiejętności – 3 sloty", header);
            for (int i = 0; i < RunState.SkillSlotCount; i++)
                GUILayout.Label($"<b>[{SkillButton(i)}]</b> {(run.skillSlots[i] != null ? run.skillSlots[i].Name : "<color=#999999>pusty</color>")}", label);
            GUILayout.Label("<size=14>Przypisz umiejętność do przycisku (ponownie – zdejmij). Odnowienie zostaje przy umiejętności.</size>", label);
            foreach (var s in run.knownSpells)
            {
                var sk = s;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>{sk.Name}</b> <size=14>({Names.SkillCategory(sk.definition.category)}, {(sk.permanent ? "stała" : "tylko to podejście")})</size>", label, GUILayout.Width(colW - 220));
                for (int i = 0; i < RunState.SkillSlotCount; i++)
                {
                    int slot = i;
                    Tog(run.SlotOf(sk) == slot, SlotToggleText(slot, run.SlotOf(sk) == slot), button, on => { if (on) run.AssignSlot(sk, slot); else run.ClearSlot(slot); pc.RefreshBuild(); }, GUILayout.Width(54));
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("<size=14>" + Describe.Spell(sk.definition, sk.level, snap.stats, b).Replace("\n", " · ") + "</size>", label);
            }
            if (run.knownSpells.Count == 0) GUILayout.Label("Brak umiejętności – można je zdobyć jako nagrodę lub odblokować na stałe.", small);
            EndScroll();
            GUILayout.EndArea();

            // Plecak
            GUILayout.BeginArea(new Rect(60 + colW, 80, colW, H - 190), box);
            GUILayout.Label("Plecak", header);
            BeginScroll(scrollB, H - 260);
            foreach (var it in run.inventory.ToList())
            {
                GUILayout.Label($"<b>{it.Name}</b>", label);
                GUILayout.Label(Describe.Item(it, snap.stats, b), small);
                GUILayout.BeginHorizontal();
                if (it.definition.kind == ItemKind.Ring)
                {
                    Btn("Załóż (P1)", button, () => { run.EquipFromInventory(it, EquipSlot.Ring1); pc.RefreshBuild(); });
                    Btn("Załóż (P2)", button, () => { run.EquipFromInventory(it, EquipSlot.Ring2); pc.RefreshBuild(); });
                }
                else Btn($"Załóż → {Names.Slot(it.definition.DefaultSlot)}", button, () => { run.EquipFromInventory(it, it.definition.DefaultSlot); pc.RefreshBuild(); });
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            if (run.inventory.Count == 0) GUILayout.Label("Pusto.", small);
            EndScroll();
            GUILayout.EndArea();

            // Statystyki (bez przycisków – przewijane prawą gałką)
            GUILayout.BeginArea(new Rect(80 + colW * 2, 80, colW, H - 190), box);
            GUILayout.Label("Statystyki", header);
            scrollC.pos = GUILayout.BeginScrollView(scrollC.pos);
            var st = snap.stats;
            GUILayout.Label($"Siła {st[StatType.Strength]:0} · Zręczność {st[StatType.Dexterity]:0} · Inteligencja {st[StatType.Intelligence]:0} · Wytrzymałość {st[StatType.Toughness]:0}", label);
            GUILayout.Label($"Zręczność: uchylenie {snap.evasionChance * 100:0.#}% · szybkość ataku ×{snap.attackSpeed:0.00} · unik {snap.dodge.distance:0.#} m", small);
            GUILayout.Label($"Życie {st[StatType.MaxHealth]:0} (reg. {st[StatType.HealthRegen]:0.#}/s)\nWytrzymałość {st[StatType.MaxStamina]:0} (reg. {st[StatType.StaminaRegen]:0}/s)\nMana {st[StatType.MaxMana]:0} (reg. {st[StatType.ManaRegen]:0.#}/s)", label);
            GUILayout.Label($"Redukcja fiz. {snap.PhysicalReduction(b) * 100:0}% · mag. {snap.MagicReduction(b) * 100:0}%", label);
            GUILayout.Label($"Broń: {snap.mainHand.Name} – lekki {snap.WeaponDamage(snap.weapon.light):0}, ciężki {snap.WeaponDamage(snap.weapon.heavy):0}" +
                            (snap.weaponEffectiveness < 1 ? $" <color=#ff9966>(wymagania niespełnione ×{snap.weaponEffectiveness:0.##})</color>" : ""), label);
            GUILayout.Label(snap.guard != null
                ? $"Blok ({(snap.guardIsShield ? "tarcza" : "broń")}): {snap.guard.physicalReduction * 100:0}% fiz. / {snap.guard.magicReduction * 100:0}% mag., stabilność ×{snap.guard.stabilityMultiplier:0.##}"
                : "Blok: niedostępny (brak tarczy i broni zdolnej do bloku)", label);
            GUILayout.Label(snap.parry != null ? $"Parowanie: okno {snap.parry.activeWindow:0.00}s, koszt {snap.parry.staminaCost:0}" : "Parowanie: niedostępne", label);
            GUILayout.Label($"Obciążenie {run.equipment.TotalWeight:0.#}/{st[StatType.EquipLoad]:0} ({snap.loadRatio * 100:0}%) – unik {snap.dodge.distance:0.#} m, niewrażliwość {snap.dodge.invulnDuration:0.00}s, koszt {snap.dodge.staminaCost:0}", label);
            GUILayout.Label($"Premie: obrażenia broni +{st[StatType.PhysicalDamage]:0}%, moc czarów +{st[StatType.SpellPower]:0}%, flaszki +{st[StatType.FlaskPotency]:0}%, riposta +{st[StatType.RiposteDamage]:0}%", small);
            GUILayout.Label($"Żywioły – obrażenia: ogień +{st[StatType.FireDamage]:0}%, mróz +{st[StatType.FrostDamage]:0}%, błyskawice +{st[StatType.LightningDamage]:0}%, krwawienie +{st[StatType.BleedDamage]:0}%", small);
            GUILayout.Label($"Odporności: ogień {Mathf.Min(st[StatType.FireResist], b.maxElementResist):0}%, mróz {Mathf.Min(st[StatType.FrostResist], b.maxElementResist):0}%, błyskawice {Mathf.Min(st[StatType.LightningResist], b.maxElementResist):0}%", small);
            var fxs = new List<string>();
            foreach (PassiveEffectType t in Enum.GetValues(typeof(PassiveEffectType)))
                if (t != PassiveEffectType.None && snap.effects[t] != 0) fxs.Add(Names.Effect(t, snap.effects[t]));
            if (fxs.Count > 0) GUILayout.Label("Efekty: " + string.Join(" · ", fxs), small);
            GUILayout.Space(8);
            GUILayout.Label("Wzmocnienia", header);
            foreach (var bs in run.boons) GUILayout.Label($"{bs.definition.displayName} ×{bs.stacks}", small);
            if (run.boons.Count == 0) GUILayout.Label("brak", small);
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            BtnRect(new Rect(W - 300, H - 100, 260, 60), "Gotowe", button, root.CloseEquipment);
        }
    }
}
