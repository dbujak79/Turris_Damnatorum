using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Turris
{
    /// <summary>
    /// Mapa akcji Input System tworzona w kodzie (klawiatura+mysz i pad).
    /// Nie wymaga pliku .inputactions, więc prototyp działa bez ręcznej konfiguracji.
    /// Przyciski akcji można przemapować osobno dla klawiatury/myszy i pada (nadpisania zapisywane w profilu).
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        public const string KeyboardGroup = "KeyboardMouse", GamepadGroup = "Gamepad";

        /// <summary>Akcje, które gracz może przemapować (nazwa akcji, etykieta w ustawieniach).</summary>
        public static readonly (string action, string label)[] Rebindable =
        {
            ("Light", "Szybki atak / riposta"), ("Heavy", "Mocny atak"), ("Block", "Blok (trzymaj)"), ("Parry", "Parowanie"),
            ("Dodge", "Unik"), ("Skill1", "Umiejętność 1"), ("Skill2", "Umiejętność 2"), ("Skill3", "Umiejętność 3"),
            ("FlaskHealth", "Flaszka życia"), ("FlaskMana", "Flaszka many"), ("Sprint", "Bieg (trzymaj)"),
            ("LockOn", "Namierzanie"), ("SwitchLeft", "Zmiana celu w lewo"), ("SwitchRight", "Zmiana celu w prawo"),
        };

        InputActionMap map;
        InputActionRebindingExtensions.RebindingOperation rebinding;
        /// <summary>Trwa oczekiwanie na przycisk do przypisania (menu ignoruje wtedy wejście).</summary>
        public bool IsRebinding => rebinding != null;
        InputAction move, look, sprint, block, light, heavy, parry, dodge, skill1, skill2, skill3, flaskHp, flaskMp, lockOn, switchLeft, switchRight, pause;

        public Vector2 Move => move.ReadValue<Vector2>();
        public Vector2 LookDelta { get; private set; }
        public bool SprintHeld => sprint.IsPressed();
        public bool BlockHeld => block.IsPressed();

        public event Action LightPressed, HeavyPressed, ParryPressed, DodgePressed,
            FlaskHealthPressed, FlaskManaPressed, LockOnPressed, PausePressed;
        public event Action<int> SwitchTargetPressed;
        /// <summary>Przycisk umiejętności: 0, 1 lub 2.</summary>
        public event Action<int> SkillPressed;

        public float mouseSensitivity = 0.12f;
        public float stickSensitivity = 160f;

        void Awake() => EnsureMap();

        /// <summary>Tworzy mapę akcji (idempotentne – bezpieczne także przed Awake, np. w testach).</summary>
        public void EnsureMap()
        {
            if (map != null) return;
            map = new InputActionMap("Gameplay");
            move = map.AddAction("Move", InputActionType.Value);
            move.expectedControlType = "Vector2";
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick");

            look = map.AddAction("Look", InputActionType.Value);
            look.expectedControlType = "Vector2";
            look.AddBinding("<Mouse>/delta");
            var lookStick = map.AddAction("LookStick", InputActionType.Value);
            lookStick.expectedControlType = "Vector2";
            lookStick.AddBinding("<Gamepad>/rightStick");

            sprint = Button("Sprint", "<Keyboard>/leftShift", "<Gamepad>/leftStickPress");
            block = Button("Block", "<Mouse>/rightButton", "<Gamepad>/leftShoulder");
            light = Button("Light", "<Mouse>/leftButton", "<Gamepad>/rightShoulder");
            heavy = Button("Heavy", "<Keyboard>/f", "<Gamepad>/rightTrigger");
            // Parowanie pod małym palcem, żeby Q/E/R zostały dla umiejętności (plus boczny przycisk myszy).
            parry = Button("Parry", "<Keyboard>/leftCtrl", "<Gamepad>/leftTrigger");
            parry.AddBinding("<Mouse>/backButton", groups: KeyboardGroup);
            dodge = Button("Dodge", "<Keyboard>/space", "<Gamepad>/buttonEast");
            // Trzy sloty umiejętności: Q / E / R oraz A / X / Y.
            skill1 = Button("Skill1", "<Keyboard>/q", "<Gamepad>/buttonSouth");
            skill2 = Button("Skill2", "<Keyboard>/e", "<Gamepad>/buttonWest");
            skill3 = Button("Skill3", "<Keyboard>/r", "<Gamepad>/buttonNorth");
            flaskHp = Button("FlaskHealth", "<Keyboard>/1", "<Gamepad>/dpad/up");
            flaskMp = Button("FlaskMana", "<Keyboard>/2", "<Gamepad>/dpad/down");
            lockOn = Button("LockOn", "<Keyboard>/tab", "<Gamepad>/rightStickPress");
            lockOn.AddBinding("<Mouse>/middleButton", groups: KeyboardGroup);
            switchLeft = Button("SwitchLeft", "<Keyboard>/z", "<Gamepad>/dpad/left");
            switchRight = Button("SwitchRight", "<Keyboard>/c", "<Gamepad>/dpad/right");
            pause = Button("Pause", "<Keyboard>/escape", "<Gamepad>/start");

            light.performed += _ => LightPressed?.Invoke();
            heavy.performed += _ => HeavyPressed?.Invoke();
            parry.performed += _ => ParryPressed?.Invoke();
            dodge.performed += _ => DodgePressed?.Invoke();
            skill1.performed += _ => SkillPressed?.Invoke(0);
            skill2.performed += _ => SkillPressed?.Invoke(1);
            skill3.performed += _ => SkillPressed?.Invoke(2);
            flaskHp.performed += _ => FlaskHealthPressed?.Invoke();
            flaskMp.performed += _ => FlaskManaPressed?.Invoke();
            lockOn.performed += _ => LockOnPressed?.Invoke();
            switchLeft.performed += _ => SwitchTargetPressed?.Invoke(-1);
            switchRight.performed += _ => SwitchTargetPressed?.Invoke(1);
            pause.performed += _ => PausePressed?.Invoke();

            this.lookStick = lookStick;
        }

        InputAction lookStick;

        InputAction Button(string name, string kb, string pad)
        {
            var a = map.AddAction(name, InputActionType.Button);
            if (kb != null) a.AddBinding(kb, groups: KeyboardGroup);
            if (pad != null) a.AddBinding(pad, groups: GamepadGroup);
            return a;
        }

        // ------------------------------------------------------------------ Przemapowanie

        static int BindingIndex(InputAction a, bool pad)
        {
            string g = pad ? GamepadGroup : KeyboardGroup;
            for (int i = 0; i < a.bindings.Count; i++)
                if (!a.bindings[i].isComposite && !a.bindings[i].isPartOfComposite && a.bindings[i].groups != null && a.bindings[i].groups.Contains(g)) return i;
            return -1;
        }

        /// <summary>Czytelna nazwa przycisku akcji (np. „Q”, „LPM”, „A”, „RB”) albo „—”.</summary>
        public string Display(string action, bool pad)
        {
            EnsureMap();
            var a = map.FindAction(action);
            if (a == null) return "—";
            int i = BindingIndex(a, pad);
            if (i < 0 || string.IsNullOrEmpty(a.bindings[i].effectivePath)) return "—";
            return Polish(a.GetBindingDisplayString(i));
        }

        static string Polish(string s)
        {
            switch (s)
            {
                case "LMB": return "LPM";
                case "RMB": return "PPM";
                case "MMB": return "ŚPM";
                case "Space": return "Spacja";
                case "Left Ctrl": case "LCtrl": return "L-Ctrl";
                case "Left Shift": case "LShift": return "Shift";
                case "Back": return "Mysz 4";
                case "D-Pad/Up": case "D-Pad Up": return "D-pad ↑";
                case "D-Pad/Down": case "D-Pad Down": return "D-pad ↓";
                case "D-Pad/Left": case "D-Pad Left": return "D-pad ←";
                case "D-Pad/Right": case "D-Pad Right": return "D-pad →";
                case "Left Stick Press": case "LS": return "L3";
                case "Right Stick Press": case "RS": return "R3";
                default: return s;
            }
        }

        public string EffectivePath(string action, bool pad)
        {
            EnsureMap();
            var a = map.FindAction(action);
            int i = a != null ? BindingIndex(a, pad) : -1;
            return i >= 0 ? a.bindings[i].effectivePath : null;
        }

        /// <summary>
        /// Przypisuje przycisk akcji. Jeśli ten przycisk miała już inna akcja w tej samej grupie – dostaje stary przycisk (zamiana),
        /// więc nigdy dwie akcje nie dzielą jednego klawisza.
        /// </summary>
        public void ApplyRebind(string action, bool pad, string newPath)
        {
            EnsureMap();
            var a = map.FindAction(action);
            int i = a != null ? BindingIndex(a, pad) : -1;
            if (i < 0 || string.IsNullOrEmpty(newPath)) return;
            string oldPath = a.bindings[i].effectivePath;
            foreach (var (other, _) in Rebindable)
            {
                if (other == action) continue;
                var o = map.FindAction(other);
                int oi = BindingIndex(o, pad);
                if (oi >= 0 && string.Equals(o.bindings[oi].effectivePath, newPath, StringComparison.OrdinalIgnoreCase))
                    o.ApplyBindingOverride(oi, oldPath);
            }
            a.ApplyBindingOverride(i, newPath);
        }

        /// <summary>Czeka na naciśnięcie przycisku (klawiatura/mysz albo pad) i przypisuje go do akcji. Esc/Select – anuluj.</summary>
        public void StartRebind(string action, bool pad, Action<bool> done)
        {
            EnsureMap();
            if (rebinding != null) return;
            var a = map.FindAction(action);
            int i = a != null ? BindingIndex(a, pad) : -1;
            if (i < 0) { done?.Invoke(false); return; }
            bool wasEnabled = a.enabled;
            string before = a.bindings[i].effectivePath;
            a.Disable();
            var op = a.PerformInteractiveRebinding(i)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f);
            if (pad) op = op.WithControlsHavingToMatchPath("<Gamepad>").WithControlsExcluding("<Gamepad>/select").WithControlsExcluding("<Gamepad>/start");
            else op = op.WithControlsHavingToMatchPath("<Keyboard>").WithControlsHavingToMatchPath("<Mouse>")
                    .WithControlsExcluding("<Mouse>/position").WithControlsExcluding("<Mouse>/delta").WithControlsExcluding("<Mouse>/scroll");
            void Finish(bool ok, string path)
            {
                rebinding?.Dispose();
                rebinding = null;
                if (ok)
                {
                    // Operacja już zapisała nowy przycisk – cofamy do stanu sprzed i przypisujemy przez ApplyRebind (z zamianą).
                    a.ApplyBindingOverride(i, before);
                    ApplyRebind(action, pad, path);
                }
                if (wasEnabled) a.Enable();
                done?.Invoke(ok);
            }
            rebinding = op.OnComplete(o => Finish(true, a.bindings[i].overridePath ?? a.bindings[i].effectivePath))
                          .OnCancel(o => Finish(false, null));
            // Ścieżka zapisuje się w overridePath dopiero po zakończeniu – odczyt w OnComplete.
            rebinding.Start();
        }

        public void CancelRebind() => rebinding?.Cancel();

        // Własny zapis zamiast SaveBindingOverridesAsJson: mapa powstaje w kodzie, więc identyfikatory bindingów są
        // przy każdym uruchomieniu inne – dopasowanie musi iść po nazwie akcji i urządzeniu.
        [Serializable] class SavedBindings { public System.Collections.Generic.List<SavedBinding> entries = new System.Collections.Generic.List<SavedBinding>(); }
        [Serializable] class SavedBinding { public string action; public bool pad; public string path; }

        public string SaveOverrides()
        {
            EnsureMap();
            var saved = new SavedBindings();
            foreach (var (action, _) in Rebindable)
                foreach (bool pad in new[] { false, true })
                {
                    var a = map.FindAction(action);
                    int i = BindingIndex(a, pad);
                    if (i >= 0 && !string.IsNullOrEmpty(a.bindings[i].overridePath))
                        saved.entries.Add(new SavedBinding { action = action, pad = pad, path = a.bindings[i].overridePath });
                }
            return saved.entries.Count == 0 ? "" : JsonUtility.ToJson(saved);
        }

        public void LoadOverrides(string json)
        {
            EnsureMap();
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var saved = JsonUtility.FromJson<SavedBindings>(json);
                if (saved?.entries == null) return;
                foreach (var e in saved.entries)
                {
                    var a = map.FindAction(e.action);
                    int i = a != null ? BindingIndex(a, e.pad) : -1;
                    if (i >= 0 && !string.IsNullOrEmpty(e.path)) a.ApplyBindingOverride(i, e.path);
                }
            }
            catch (Exception ex) { Debug.LogWarning("[Turris] Nie wczytano przypisań przycisków: " + ex.Message); }
        }

        public void ResetOverrides() { EnsureMap(); map.RemoveAllBindingOverrides(); }

        void OnEnable() => map?.Enable();
        void OnDisable() => map?.Disable();
        void OnDestroy() { rebinding?.Dispose(); map?.Dispose(); }

        void Update()
        {
            Vector2 mouse = look.ReadValue<Vector2>() * mouseSensitivity;
            Vector2 stick = lookStick.ReadValue<Vector2>() * stickSensitivity * Time.unscaledDeltaTime;
            LookDelta = mouse + stick;
        }

        /// <summary>Czy wejście rozgrywki ma być przyjmowane (wyłączane w menu).</summary>
        public void SetGameplayEnabled(bool enabled)
        {
            if (map == null) return;
            if (enabled) map.Enable(); else map.Disable();
            if (enabled) pause.Enable();
        }

        public void EnablePauseOnly()
        {
            map.Disable();
            pause.Enable();
        }
    }
}
