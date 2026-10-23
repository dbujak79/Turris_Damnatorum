using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Poligon umiejętności: manekiny bez AI, lista rodziny klasy, sloty, odnowienia, wstawanie celów, nieśmiertelność.</summary>
    public class SkillRangePlayTests
    {
        string profileFile;

        [SetUp] public void SetUp() { profileFile = $"turris_range_test_{System.Guid.NewGuid():N}.json"; GameRoot.ProfileFileOverride = profileFile; }

        [TearDown]
        public void TearDown()
        {
            GameRoot.ProfileFileOverride = null;
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
        }

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
        public IEnumerator Range_DummiesStand_SkillsSwapSlots_NoDeath_TargetsStandUp()
        {
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            var mage = root.config.classes.First(c => c.id == "class_mage");
            root.StartSkillRange(mage);
            yield return null;
            var range = root.Range;
            Assert.IsNotNull(range, "Poligon uruchomiony");
            Assert.AreEqual(GameScreen.Playing, root.Screen);
            Assert.AreEqual(7, root.Enemies.Count(e => e != null && !e.IsDead), "Gromada: 7 manekinów");
            Assert.IsTrue(root.Enemies.All(e => e.TrainingDummy), "Manekiny bez AI");
            var headers = range.Entries.Select(e => e.header).Distinct().ToList();
            Assert.AreEqual(mage.displayName, headers.First(), "Najpierw klasa bazowa");
            Assert.Contains("Wspólne", headers, "Grupa wspólnych umiejętności");
            Assert.Greater(range.Entries.Count, 5, "Lista umiejętności rodziny");

            // Manekiny stoją: po 2 s żaden się nie ruszył i nie zaatakował.
            var before = root.Enemies.Select(e => e.transform.position).ToList();
            float hp = root.Player.Health.Current;
            float t = 0; while (t < 2f) { t += Time.deltaTime; yield return null; }
            for (int i = 0; i < before.Count; i++) Assert.Less(Vector3.Distance(before[i], root.Enemies[i].transform.position), 0.35f, "Manekin stoi");
            Assert.GreaterOrEqual(root.Player.Health.Current, hp, "Manekiny nie biją");

            // Wybór: nowa umiejętność pod Q, poprzednia pod E.
            var first = range.SelectedSkill;
            range.Step(1);
            var second = range.SelectedSkill;
            Assert.AreNotSame(first, second);
            Assert.AreSame(second, root.Run.skillSlots[0].definition, "Wybrana pod Q");
            Assert.AreSame(first, root.Run.skillSlots[1].definition, "Poprzednia pod E");
            range.SetLevel(2);
            Assert.AreEqual(Mathf.Min(2, second.maxLevel), root.Run.skillSlots[0].level, "Poziom ustawiony");

            // Odnowienie wraca od razu.
            root.Run.skillSlots[0].TryUse();
            yield return null;
            Assert.IsTrue(root.Run.skillSlots[0].Ready, "Bez odnowień na poligonie");

            // Bohater nie ginie.
            root.Player.DebugKill();
            yield return null;
            Assert.IsFalse(root.Player.IsDead, "Bohater nie ginie na poligonie");

            // Cel pada i wstaje w tym samym miejscu; piętro się nie kończy.
            var victim = root.Enemies.First(e => !e.IsDead);
            Vector3 spot = victim.DummySpot;
            victim.ReceiveHit(new HitData { physical = 1e6f, sourcePosition = root.Player.transform.position, attacker = root.Player });
            Assert.IsTrue(victim.IsDead, "Cel padł");
            t = 0; while (t < 2.2f) { t += Time.deltaTime; yield return null; }
            Assert.AreEqual(GameScreen.Playing, root.Screen, "Poligon trwa – bez nagród i następnego piętra");
            Assert.IsTrue(root.Enemies.Any(e => e != null && !e.IsDead && Vector3.Distance(e.DummySpot, spot) < 0.01f), "Cel wstał na swoim miejscu");

            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (!string.IsNullOrEmpty(dir) && !Application.isBatchMode)
            {
                Directory.CreateDirectory(dir);
                for (int i = 0; i < 3; i++) yield return null;
                yield return new WaitForEndOfFrame();
                var tex = ScreenCapture.CaptureScreenshotAsTexture();
                if (tex != null && tex.width > 16) File.WriteAllBytes(Path.Combine(dir, "range_panel.png"), tex.EncodeToPNG());
                if (tex != null) Object.Destroy(tex);
            }

            root.ShowMainMenu();
            yield return null;
            Assert.IsNull(root.Range, "Wyjście z poligonu");
            Assert.IsFalse(root.Player.Immortal, "Nieśmiertelność tylko na poligonie");
        }
    }
}
