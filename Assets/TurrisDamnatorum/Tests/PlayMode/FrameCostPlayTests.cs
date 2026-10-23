using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Koszt klatki prawdziwej gry (scena Main, piętro I, walka): czas pracy od pierwszego Update do końca renderu.
    /// Uśpienie edytora bez fokusu (dławienie do ~15 kl./s) wypada między klatkami, więc pomiar go nie zawiera.
    /// </summary>
    public class FrameCostPlayTests
    {
        [DefaultExecutionOrder(-32000)]
        class FrameStartProbe : MonoBehaviour
        {
            public double start;
            void Update() => start = Time.realtimeSinceStartupAsDouble;
        }

        string profileFile;

        [SetUp] public void SetUp() { profileFile = $"turris_cost_test_{System.Guid.NewGuid():N}.json"; GameRoot.ProfileFileOverride = profileFile; }

        [TearDown]
        public void TearDown()
        {
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
        public IEnumerator Floor1Combat_FrameWorkFitsBudget()
        {
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            var plan = new LoadoutPlan { classDef = root.config.classes[0], difficulty = root.config.difficulties[0] };
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
            var probe = new GameObject("FrameStartProbe").AddComponent<FrameStartProbe>();
            var end = new WaitForEndOfFrame();
            float t = 0;
            while (t < 3f) { t += Time.deltaTime; yield return null; }
            var work = new List<double>();
            for (int i = 0; i < 120; i++)
            {
                if (root.Player.Actions.IsIdle && i % 20 == 0) root.Player.Request(ActionType.LightAttack);
                yield return end;
                work.Add((Time.realtimeSinceStartupAsDouble - probe.start) * 1000.0);
                yield return null;
            }
            Object.Destroy(probe.gameObject);
            var sorted = work.OrderBy(x => x).ToList();
            double median = sorted[sorted.Count / 2], p95 = sorted[(int)(sorted.Count * 0.95f)], max = sorted[sorted.Count - 1];
            Debug.Log($"DETAIL frame work (Update→koniec renderu): mediana {median:F2} ms, p95 {p95:F2} ms, max {max:F2} ms, " +
                      $"wrogowie {root.Enemies.Count}, ekran {Screen.width}x{Screen.height}, dt gry {Time.deltaTime * 1000f:F1} ms");
            Assert.Less(median, 16.7, "Mediana pracy klatki mieści się w 60 kl./s");
        }
    }
}
