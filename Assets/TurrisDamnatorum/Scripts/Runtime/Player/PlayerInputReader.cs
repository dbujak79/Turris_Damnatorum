using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Turris
{
    /// <summary>
    /// Mapa akcji Input System tworzona w kodzie (klawiatura+mysz i pad).
    /// Nie wymaga pliku .inputactions, więc prototyp działa bez ręcznej konfiguracji.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        InputActionMap map;
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

        void Awake()
        {
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
            parry.AddBinding("<Mouse>/backButton");
            dodge = Button("Dodge", "<Keyboard>/space", "<Gamepad>/buttonEast");
            // Trzy sloty umiejętności: Q / E / R oraz A / X / Y.
            skill1 = Button("Skill1", "<Keyboard>/q", "<Gamepad>/buttonSouth");
            skill2 = Button("Skill2", "<Keyboard>/e", "<Gamepad>/buttonWest");
            skill3 = Button("Skill3", "<Keyboard>/r", "<Gamepad>/buttonNorth");
            flaskHp = Button("FlaskHealth", "<Keyboard>/1", "<Gamepad>/dpad/up");
            flaskMp = Button("FlaskMana", "<Keyboard>/2", "<Gamepad>/dpad/down");
            lockOn = Button("LockOn", "<Keyboard>/tab", "<Gamepad>/rightStickPress");
            lockOn.AddBinding("<Mouse>/middleButton");
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
            if (kb != null) a.AddBinding(kb);
            if (pad != null) a.AddBinding(pad);
            return a;
        }

        void OnEnable() => map?.Enable();
        void OnDisable() => map?.Disable();
        void OnDestroy() => map?.Dispose();

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
