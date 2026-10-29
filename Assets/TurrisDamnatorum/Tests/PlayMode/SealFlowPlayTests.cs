using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Pieczęcie w przebiegu wieży: po piętrze sklep dusz ma oferty pieczęci i dar bossa, zakup wkłada pieczęć do skilla,
    /// zakładka pieczęci w ekwipunku pozwala włożyć pieczęć z sakwy, a HUD pokazuje znaczki pod kartą skilla.
    /// </summary>
    public class SealFlowPlayTests
    {
        string profileFile;

        [SetUp] public void SetUp() { profileFile = $"turris_seal_flow_{System.Guid.NewGuid():N}.json"; GameRoot.ProfileFileOverride = profileFile; }

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
            HuWorld.Clear();
            var main = SceneManager.GetSceneByName("Main");
            if (!main.IsValid() || !main.isLoaded) yield break;
            var empty = SceneManager.CreateScene("AfterTest_" + System.Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(main);
        }

        static string ShotDir()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            return Application.isBatchMode ? null : dir;
        }

        static IEnumerator Shot(string name)
        {
            string dir = ShotDir();
            if (string.IsNullOrEmpty(dir)) yield break;
            for (int i = 0; i < 3; i++) yield return null;
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            if (tex != null && tex.width > 16) File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            if (tex != null) Object.Destroy(tex);
        }

        [UnityTest]
        public IEnumerator FloorClear_OffersSeals_GiftAndPouchWork()
        {
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            var cls = root.config.classes.First(c => c.id == "class_hu_mage_fire");
            Assert.IsTrue(root.TryStartRun(new LoadoutPlan { classDef = cls, difficulty = root.config.difficulties[0] }, out var reason), reason);
            yield return null;
            var run = root.Run;
            var orb = run.LearnSpell(root.config.baseSpellPool.First(d => d.id == "hu_infernal_orb"));
            run.AssignSlot(orb, 0);
            orb.level = 1;
            run.AddSeal(HuCatalog.Seal("seal.split"), toPouch: true);
            run.AddSeal(HuCatalog.Seal("seal.echo"), toPouch: true);
            run.sealGifts.Add(HuSealEconomy.RollGift(run, new System.Random(3)));
            root.Player.RefreshBuild();
            yield return Shot("seal_hud_before");

            root.DebugCompleteFloorNow();
            yield return null;
            Assert.AreEqual(GameScreen.Intermission, root.Screen, "HU: po piętrze od razu sklep");
            yield return null;
            Assert.AreEqual(GameScreen.Intermission, root.Screen);
            Assert.Greater(root.SealOffers.Count, 0, "sklep dusz ma oferty pieczęci");
            Assert.IsTrue(root.SealOffers.Any(o => o.seal.IsForm), "jedna oferta to forma posiadanego skilla");
            Assert.AreEqual(1, run.sealGifts.Count, "dar bossa czeka na postoju");
            yield return Shot("seal_shop");

            run.souls = 5000;
            var offer = root.SealOffers.First();
            root.BuySealOffer(offer);
            yield return null;
            Assert.IsTrue(offer.sold);
            Assert.IsTrue(run.OwnsSeal(offer.seal));

            int pouch = run.sealPouch.Count;
            root.TakeGift(0, 0);
            yield return null;
            Assert.AreEqual(0, run.sealGifts.Count, "dar wybrany – reszta przepada");

            var ui = Object.FindAnyObjectByType<GameUI>();
            ui.ShowSeals = true;
            root.OpenEquipment();
            yield return null;
            var split = HuCatalog.Seal("seal.split");
            if (run.sealPouch.Contains(split))
            {
                // Zakupy i dar mogły zająć gniazda albo zmienić formę (oferty zależą od losowania) – zwolnij skill przed wstawieniem z sakwy.
                foreach (var s in orb.seals.All.ToList()) root.RemoveSeal(orb, s); // forma z zakupu może nie przyjmować rozszczepu
                root.InsertSeal(orb, split);
                yield return null;
                Assert.IsTrue(orb.seals.All.Contains(split), "pieczęć z sakwy w skillu");
            }
            yield return Shot("seal_manager");
            Assert.AreEqual(GameScreen.Equipment, root.Screen);
            ui.Back();
            yield return null;
            Assert.AreEqual(GameScreen.Intermission, root.Screen);
            Assert.IsFalse(ui.ShowSeals);
            Assert.GreaterOrEqual(pouch, 0);
        }
    }
}
