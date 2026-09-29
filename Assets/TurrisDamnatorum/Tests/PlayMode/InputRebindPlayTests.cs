using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Przemapowanie przycisków (plan rozwoju, etap E1).</summary>
    public class InputRebindPlayTests
    {
        GameObject go;
        PlayerInputReader reader;
        Keyboard kb;
        Gamepad pad;

        [SetUp]
        public void SetUp()
        {
            kb = InputSystem.AddDevice<Keyboard>("RebindTestKeyboard");
            pad = InputSystem.AddDevice<Gamepad>("RebindTestGamepad");
            go = new GameObject("InputTest");
            reader = go.AddComponent<PlayerInputReader>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(go);
            InputSystem.RemoveDevice(kb);
            InputSystem.RemoveDevice(pad);
        }

        [Test]
        public void Defaults_AreShownPerDevice()
        {
            Assert.AreEqual("Q", reader.Display("Skill1", false));
            Assert.AreEqual("E", reader.Display("Skill2", false));
            Assert.AreEqual("<Gamepad>/buttonSouth", reader.EffectivePath("Skill1", true));
            Assert.AreEqual("LPM", reader.Display("Light", false), "Polskie nazwy przycisków myszy");
            Assert.AreEqual("D-pad ↑", reader.Display("FlaskHealth", true), "Polskie nazwy przycisków pada");
            Assert.AreEqual("L3", reader.Display("Sprint", true));
        }

        [Test]
        public void Rebind_SwapsConflicts_SavesLoadsAndResets()
        {
            reader.ApplyRebind("Skill1", false, "<Keyboard>/e");
            Assert.AreEqual("E", reader.Display("Skill1", false));
            Assert.AreEqual("Q", reader.Display("Skill2", false), "Zajęty klawisz – zamiana miejscami");

            string json = reader.SaveOverrides();
            var other = new GameObject("Other").AddComponent<PlayerInputReader>();
            other.LoadOverrides(json);
            Assert.AreEqual("E", other.Display("Skill1", false), "Przypisania wracają z profilu");
            Object.Destroy(other.gameObject);

            reader.ResetOverrides();
            Assert.AreEqual("Q", reader.Display("Skill1", false));
            Assert.AreEqual("E", reader.Display("Skill2", false));
        }

        [UnityTest]
        public IEnumerator ReboundKey_TriggersAction()
        {
            reader.SetGameplayEnabled(true);
            reader.ApplyRebind("Skill1", false, "<Keyboard>/g");
            int fired = -1;
            reader.SkillPressed += s => fired = s;
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.G));
            yield return null;
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return null;
            Assert.AreEqual(0, fired, "Nowy klawisz uruchamia umiejętność 1");
        }

        [UnityTest]
        public IEnumerator InteractiveRebind_WithGamepad()
        {
            bool? result = null;
            reader.StartRebind("Skill1", true, ok => result = ok);
            Assert.IsTrue(reader.IsRebinding);
            yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.North));
            yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState());
            float t = 0;
            while (result == null && t < 2f) { t += Time.unscaledDeltaTime; yield return null; }
            Assert.IsTrue(result == true, "Przypisano naciśnięty przycisk");
            Assert.AreEqual("<Gamepad>/buttonNorth", reader.EffectivePath("Skill1", true));
            Assert.AreEqual("<Gamepad>/buttonSouth", reader.EffectivePath("Skill3", true), "Umiejętność 3 dostała zwolniony przycisk");
            Assert.IsFalse(reader.IsRebinding);
        }
    }
}
