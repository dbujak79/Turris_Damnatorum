using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Pełna ścieżka gry obsługiwana WYŁĄCZNIE wirtualnym padem (Input System):
    /// menu → przygotowanie → start → atak/pauza → nagroda → ekwipunek → kolejne piętro → śmierć → restart.
    /// </summary>
    public class GamepadPlayTests
    {
        Gamepad pad;
        string profileFile;

        [SetUp]
        public void SetUp()
        {
            profileFile = $"turris_pad_test_{System.Guid.NewGuid():N}.json";
            GameRoot.ProfileFileOverride = profileFile;
            pad = InputSystem.AddDevice<Gamepad>("TestGamepad");
            pad.MakeCurrent();
        }

        [TearDown]
        public void TearDown()
        {
            if (pad != null) InputSystem.RemoveDevice(pad);
            GameRoot.ProfileFileOverride = null;
            Time.timeScale = 1f;
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
        }

        IEnumerator Press(GamepadButton button)
        {
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(button));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
            yield return null;
        }

        /// <summary>IMGUI (OnGUI) nie działa w trybie -batchmode – wtedy test jest pomijany, a nie fałszywie oblany.</summary>
        static IEnumerator RequireOnGUI(GameUI ui)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            if (ui.RepaintCount == 0)
                Assert.Ignore("OnGUI nie jest wywoływane (tryb wsadowy bez okna gry). Uruchom w edytorze: Test Runner → PlayMode.");
        }

        IEnumerator PressTimes(GamepadButton button, int times)
        {
            for (int i = 0; i < times; i++) yield return Press(button);
        }

        [UnityTest]
        public IEnumerator FullFlow_WithGamepadOnly()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            var ui = root.GetComponent<GameUI>();
            Assert.AreEqual(GameScreen.MainMenu, root.Screen);

            yield return RequireOnGUI(ui);
            // Pierwsze A tylko pokazuje fokus, drugie zatwierdza "Nowe podejście".
            yield return Press(GamepadButton.South);
            Assert.IsTrue(ui.UsingPad);
            Assert.IsTrue(ui.Navigator.NavMode);
            yield return Press(GamepadButton.South);
            Assert.AreEqual(GameScreen.Loadout, root.Screen);

            // Przygotowanie: B wraca do menu, ponowne wejście, potem w dół do "Wróć" i w prawo do "Wejdź do wieży".
            yield return Press(GamepadButton.East);
            Assert.AreEqual(GameScreen.MainMenu, root.Screen);
            yield return Press(GamepadButton.South);
            Assert.AreEqual(GameScreen.Loadout, root.Screen);
            yield return PressTimes(GamepadButton.DpadDown, 4);
            yield return Press(GamepadButton.DpadRight);
            yield return Press(GamepadButton.South);
            Assert.AreEqual(GameScreen.Playing, root.Screen, "Start podejścia padem");
            Assert.AreEqual("class_knight", root.Run.startingClass.id);

            // Rozgrywka: RB = lekki atak.
            yield return new WaitForSeconds(0.2f);
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.RightShoulder));
            yield return null;
            yield return null;
            Assert.AreEqual(ActionType.LightAttack, root.Player.Actions.Current, "RB wykonuje lekki atak");
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return new WaitForSeconds(1f);

            // Start = pauza, B = wznów.
            yield return Press(GamepadButton.Start);
            Assert.AreEqual(GameScreen.Paused, root.Screen);
            yield return Press(GamepadButton.East);
            Assert.AreEqual(GameScreen.Playing, root.Screen);

            // Nagroda: fokus od razu widoczny, A wybiera pierwszą kartę.
            root.DebugCompleteFloorNow();
            yield return null;
            Assert.AreEqual(GameScreen.Reward, root.Screen);
            yield return null;
            yield return Press(GamepadButton.South);
            Assert.AreEqual(GameScreen.Intermission, root.Screen, "A wybiera nagrodę");

            // Ekwipunek: A otwiera (pierwszy przycisk), B zamyka.
            yield return Press(GamepadButton.South);
            Assert.AreEqual(GameScreen.Equipment, root.Screen);
            yield return PressTimes(GamepadButton.DpadDown, 2);
            yield return Press(GamepadButton.East);
            Assert.AreEqual(GameScreen.Intermission, root.Screen);

            // Rozwój cechy padem: pierwszy przycisk pod „Ekwipunek” to +1 Siła.
            Assert.AreEqual(1, root.Run.attributePoints, "Punkt cechy za ukończone piętro");
            int strength = root.Run.attributes.strength;
            yield return PressTimes(GamepadButton.DpadDown, 1);
            yield return Press(GamepadButton.South);
            Assert.AreEqual(strength + 1, root.Run.attributes.strength, "A rozwija wybraną cechę");
            Assert.AreEqual(0, root.Run.attributePoints);

            // W dół do "Wejdź wyżej".
            yield return PressTimes(GamepadButton.DpadDown, 4);
            yield return Press(GamepadButton.South);
            Assert.AreEqual(GameScreen.Playing, root.Screen);
            Assert.AreEqual(1, root.Run.floorIndex);

            // Śmierć i szybki restart przyciskiem Y.
            root.Player.DebugKill();
            yield return new WaitForSeconds(2.2f);
            Assert.AreEqual(GameScreen.Death, root.Screen);
            yield return Press(GamepadButton.North);
            Assert.AreEqual(GameScreen.Playing, root.Screen);
            Assert.AreEqual(0, root.Run.floorIndex);
        }

        [UnityTest]
        public IEnumerator Unlocks_CanBePurchasedWithGamepad()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            yield return RequireOnGUI(root.GetComponent<GameUI>());
            root.Meta.DebugAddAsh(200);
            int unlockedBefore = root.Meta.Profile.unlocked.Count;

            yield return Press(GamepadButton.South);           // pokaż fokus
            yield return PressTimes(GamepadButton.DpadDown, 1); // "Odblokowania" (brak "Szybkiego startu" przy pierwszym uruchomieniu)
            yield return Press(GamepadButton.South);
            Assert.AreEqual(GameScreen.Unlocks, root.Screen);
            yield return Press(GamepadButton.South);           // pierwszy dostępny przycisk "Odblokuj"
            Assert.AreEqual(unlockedBefore + 1, root.Meta.Profile.unlocked.Count, "Zakup odblokowania padem");
            yield return Press(GamepadButton.East);
            Assert.AreEqual(GameScreen.MainMenu, root.Screen);
        }
    }
}
