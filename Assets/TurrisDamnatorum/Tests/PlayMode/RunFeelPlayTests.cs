using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Bieg prawdziwego gracza (scena Main, wirtualny pad, 60 kl./s): stopa podparcia nie może „teleportować się”
    /// po ziemi – przesunięcie stopy stojącej na ziemi w jednej klatce ma być małe. Wypisuje przebieg klatka po klatce.
    /// </summary>
    public class RunFeelPlayTests
    {
        Gamepad pad;
        string profileFile;

        [SetUp]
        public void SetUp()
        {
            profileFile = $"turris_run_test_{System.Guid.NewGuid():N}.json"; GameRoot.ProfileFileOverride = profileFile;
            pad = InputSystem.AddDevice<Gamepad>("RunTestGamepad");
        }

        [TearDown]
        public void TearDown()
        {
            if (pad != null) InputSystem.RemoveDevice(pad);
            Time.captureFramerate = 0;
            GameRoot.ProfileFileOverride = null;
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>Scena Main nie może zostać po teście: jej GameUI rysowałby HUD kamerą zniszczoną przez kolejne testy.</summary>
        [UnityTearDown]
        public IEnumerator UnloadMain()
        {
            var main = SceneManager.GetSceneByName("Main");
            if (!main.IsValid() || !main.isLoaded) yield break;
            var empty = SceneManager.CreateScene("AfterTest_" + System.Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(main);
        }

        [UnityTest]
        public IEnumerator PlayerRun_PlantedFootDoesNotJump()
        {
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            var plan = new LoadoutPlan { classDef = root.config.classes[0], difficulty = root.config.difficulties[0] };
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
            foreach (var e in root.Enemies) if (e != null) e.gameObject.SetActive(false);
            Time.captureFramerate = 60;
            for (int i = 0; i < 30; i++) yield return null;
            var visual = root.Player.GetComponentInChildren<CharacterVisual>();
            var animator = visual.GetComponentInChildren<Animator>();
            var feet = new[] { animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.RightFoot) };
            var drv = visual.Driver;
            var phaseField = drv.GetType().GetField("stepPhase", BindingFlags.Instance | BindingFlags.NonPublic);

            var rows = new List<string>();
            var stanceJumps = new List<float>();
            var turnJumps = new List<float>();
            float phaseStart = -1f, phaseEnd = 0f; int phaseFrames = 0;
            Vector3[] last = { feet[0].position, feet[1].position };
            float ground = root.Player.transform.position.y;
            for (int i = 0; i < 150; i++)
            {
                // Bieg na wprost, w połowie zwrot w bok (zmiana kierunku jak w grze).
                Vector2 stick = i < 90 ? new Vector2(0, 1) : new Vector2(1, 0);
                InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = stick });
                yield return null;
                if (i < 40) { last[0] = feet[0].position; last[1] = feet[1].position; continue; }
                var sb = new System.Text.StringBuilder();
                sb.Append($"{i} v={root.Player.GetComponent<CharacterController>().velocity.magnitude:F2} ph={(phaseField != null ? (float)phaseField.GetValue(drv) : -1):F3}");
                for (int k = 0; k < 2; k++)
                {
                    Vector3 p = feet[k].position;
                    float h = p.y - ground;
                    Vector3 d = p - last[k]; d.y = 0;
                    sb.Append($" {(k == 0 ? "L" : "P")}: h={h * 100f:F0} d={d.magnitude * 100f:F1}");
                    if (h < 0.16f) (i < 88 ? stanceJumps : turnJumps).Add(d.magnitude);
                    last[k] = p;
                }
                rows.Add(sb.ToString());
                if (phaseField != null && i < 88) { float ph = (float)phaseField.GetValue(drv); if (phaseStart < 0f) phaseStart = ph; else phaseEnd += Mathf.Repeat(ph - phaseStart, 1f); phaseStart = ph; phaseFrames++; }
            }
            InputSystem.QueueStateEvent(pad, new GamepadState());
            Debug.Log("DETAIL run frames:\n" + string.Join("\n", rows));
            var sorted = stanceJumps.OrderBy(x => x).ToList();
            float median = sorted[sorted.Count / 2], p95 = sorted[(int)(sorted.Count * 0.95f)], max = sorted[sorted.Count - 1];
            float cycle = phaseEnd > 0f ? (phaseFrames - 1) / 60f / phaseEnd : 0f;
            float turnMax = turnJumps.Count > 0 ? turnJumps.Max() : 0f;
            Debug.Log($"DETAIL bieg na wprost – stopa na ziemi, przesunięcie w klatce: mediana {median * 100f:F1} cm, p95 {p95 * 100f:F1} cm, max {max * 100f:F1} cm; cykl {cycle:F2} s; zwrot – max {turnMax * 100f:F1} cm");
            Assert.Less(p95, 0.05f, "Stopa podporowa ślizga się w biegu na wprost");
            Assert.Greater(cycle, 0.5f, "Cykl biegu krótszy niż 0,5 s – nogi przeskakują duże odcinki między klatkami");
        }
    }
}
