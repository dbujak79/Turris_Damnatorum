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
        }

        void OnDisable()
        {
            CombatEvents.HitResolved -= OnHit;
            CombatEvents.Message -= OnMessage;
        }

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
            if (root.Screen != GameScreen.Playing) HandleMenuInput(dt);

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
            bool hasBack = s == GameScreen.Unlocks || s == GameScreen.Loadout || s == GameScreen.Victory || s == GameScreen.Equipment || s == GameScreen.Paused;
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
                    string costText = u.kind == UnlockKind.Difficulty ? "" : $"  (koszt w budżecie: {u.loadoutCost})";
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
                case UnlockKind.Spell: return "Czary (dla każdej postaci)";
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
            GUILayout.Label("<size=15>Klasa określa tylko atrybuty, wyposażenie i czary na start. Później każda postać może używać dowolnej broni, pancerza i czarów.</size>", label);
            foreach (var c in cfg.classes)
                Tog(plan.classDef == c, $"  {c.displayName}", button, on => { if (on) plan.classDef = c; }, GUILayout.Height(44));
            if (plan.classDef != null)
            {
                var c = plan.classDef;
                GUILayout.Space(8);
                GUILayout.Label(c.description, small);
                GUILayout.Label($"Witalność {c.vigor} · Kondycja {c.endurance} · Umysł {c.mind}\nSiła {c.strength} · Zręczność {c.dexterity} · Inteligencja {c.intelligence}", small);
                GUILayout.Label("Wyposażenie: " + string.Join(", ", c.startingItems.Select(i => i.displayName)), small);
                if (c.startingSpells.Count > 0) GUILayout.Label("Czary: " + string.Join(", ", c.startingSpells.Select(s => s.displayName)), small);
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
            GUILayout.Label("<size=15>Wybierz odblokowane przedmioty, czary i talenty. Budżet nie pozwala zabrać wszystkiego naraz.</size>", label);
            BeginScroll(scrollC, H - 340);
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

        static string KindShort(UnlockKind k) => k == UnlockKind.StartingItem ? "przedmiot" : k == UnlockKind.Spell ? "czar" : "talent";

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

            // Flaszki i czar
            GUI.Label(new Rect(40, H - 140, 600, 30), $"<b>{Prompt("[1]", "(X)")}</b> Flaszka życia: {run.healthFlasks}/{run.MaxHealthFlasks}    <b>{Prompt("[2]", "(Y)")}</b> Flaszka many: {run.manaFlasks}/{run.MaxManaFlasks}", rich);
            var spell = pc.CurrentSpell;
            string spellText = spell == null ? "brak czaru" : $"{spell.instance.Name} ({spell.manaCost:0} many){(spell.requirementsMet ? "" : " – niespełnione wymagania")}";
            GUI.Label(new Rect(40, H - 108, 900, 30), $"<b>{Prompt("[R]", "(A)")}</b> Czar: {spellText}   <b>{Prompt("[X]", "(D-pad →)")}</b> zmień ({pc.Build.spells.Count})", rich);
            string defense = pc.Build.CanBlock ? (pc.Build.guardIsShield ? "Blok: tarcza" : "Blok: broń") : "Blok: brak";
            defense += pc.Build.CanParry ? " · Parowanie: tak" : " · Parowanie: brak";
            GUI.Label(new Rect(40, H - 76, 900, 30), $"Broń: {pc.Build.mainHand.Name}{(pc.Build.weaponEffectiveness < 1 ? " (niespełnione wymagania)" : "")} · {defense}" +
                                                     (pc.WeaponBuffTime > 0 ? $" · Zaklęte ostrze {pc.WeaponBuffTime:0}s" : ""), small);

            // Piętro, dusze
            var floor = root.CurrentFloor;
            GUI.Label(new Rect(W - 520, 30, 480, 30), $"{floor?.name}  ({run.floorIndex + 1}/{root.config.tower.floors.Count})", header);
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
                    GUI.Label(new Rect(sx - 200, sy - 60, 400, 30), "RIPOSTA – " + Prompt("[LPM]", "(RB)") + " z bliska", st);
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
                GUI.Label(new Rect(W / 2 - 400, H - 200, 800, 30), boss.DisplayName + (boss.PhaseIndex > 0 ? "  – faza II" : ""), center);
                Bar(new Rect(W / 2 - 400, H - 170, 800, 18), boss.Health.Fraction, new Color(0.65f, 0.08f, 0.08f), null);
                Bar(new Rect(W / 2 - 400, H - 148, 800, 5), boss.Poise.Fraction, new Color(0.85f, 0.75f, 0.3f), null);
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
                string controls = UsingPad
                    ? "<b>Sterowanie – pad</b> [Select ukryj]\nLewa gałka ruch · Prawa gałka kamera · L3 bieg\nRB lekki atak / riposta · RT ciężki atak\nLB blok (tarcza lub broń) · LT parowanie\nB unik · A czar · D-pad → zmiana czaru\nX flaszka życia · Y flaszka many\nR3 namierzanie · prawa gałka / D-pad ← zmiana celu\nStart pauza"
                    : "<b>Sterowanie</b> [F1 ukryj]\nWASD ruch · Mysz kamera · Shift bieg\nLPM lekki atak / riposta · F ciężki atak\nPPM blok (tarcza lub broń) · Q parowanie\nSpacja unik · R czar · X zmiana czaru\n1 flaszka życia · 2 flaszka many\nTab / ŚPM namierzanie · Z/C lub ruch myszą – zmiana celu\nEsc pauza";
                GUI.Label(new Rect(W - 470, H - 330, 450, 320),
                    "<size=15>" + controls + "\n\n<b>Sygnały ataków</b>\n<color=#e6e6e6>biały</color> zwykły · <color=#ff8c1a>pomarańczowy</color> ciężki\n<color=#bf59ff>fiolet</color> nie do sparowania · <color=#ff1a1a>czerwony</color> nie do zablokowania\n<color=#ff33cc>różowy</color> obszarowy – unik nie chroni, uciekaj lub blokuj</size>", rich);
            }
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
            GUILayout.BeginArea(new Rect(W / 2 - 220, H / 2 - 150, 440, 300), box);
            GUILayout.Label("Pauza", header);
            Btn("Wznów", button, root.TogglePause, GUILayout.Height(50));
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
        }

        void DrawIntermission()
        {
            var run = root.Run;
            var pc = root.Player;
            var r = new Rect(W / 2 - 380, 140, 760, 700);
            GUILayout.BeginArea(r, box);
            GUILayout.Label(root.AtRestPoint ? "Kapliczka – punkt odpoczynku" : "Między piętrami", header);
            GUILayout.Label($"Życie {pc.Health.Current:0}/{pc.Health.Max:0} · Mana {pc.Mana.Current:0}/{pc.Mana.Max:0} · Flaszki: życia {run.healthFlasks}/{run.MaxHealthFlasks}, many {run.manaFlasks}/{run.MaxManaFlasks}", label);
            GUILayout.Label(run.difficulty.restoreOnlyAtRest && !root.AtRestPoint
                ? "<color=#ff9966>Na tej trudności życie i flaszki odnawiają się tylko w kapliczkach.</color>"
                : "Życie, mana i flaszki zostały odnowione.", label);
            GUILayout.Label($"Dusze: {run.souls}", label);
            GUILayout.Space(10);
            Btn("Ekwipunek, czary i statystyki", button, root.OpenEquipment, GUILayout.Height(54));

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
            GUILayout.Label($"Czary przygotowane: {run.attunedSpells.Count}/{b.maxAttunedSpells}", header);
            foreach (var s in run.knownSpells)
            {
                Tog(run.attunedSpells.Contains(s), $"  {s.Name}", button, _ => { run.ToggleAttune(s, b.maxAttunedSpells); pc.RefreshBuild(); });
                GUILayout.Label("<size=14>" + Describe.Spell(s.definition, s.level, snap.stats, b).Replace("\n", " · ") + "</size>", label);
            }
            if (run.knownSpells.Count == 0) GUILayout.Label("Brak poznanych czarów – można je zdobyć jako nagrodę.", small);
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
            GUILayout.Label($"Witalność {st[StatType.Vigor]:0} · Kondycja {st[StatType.Endurance]:0} · Umysł {st[StatType.Mind]:0}\nSiła {st[StatType.Strength]:0} · Zręczność {st[StatType.Dexterity]:0} · Inteligencja {st[StatType.Intelligence]:0}", label);
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
