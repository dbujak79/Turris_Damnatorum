using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Test dymny prawdziwej sceny Main.unity z assetami utworzonymi przez Turris → Setup.</summary>
    public class MainScenePlayTests
    {
        string profileFile;

        [SetUp]
        public void SetUp()
        {
            profileFile = $"turris_scene_test_{System.Guid.NewGuid():N}.json";
            GameRoot.ProfileFileOverride = profileFile;
        }

        [TearDown]
        public void TearDown()
        {
            GameRoot.ProfileFileOverride = null;
            Time.timeScale = 1f;
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
        }

        [UnityTest]
        public IEnumerator MainScene_UsesAssetConfig_AndPlaysMageRunWithAI()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            Assert.IsNotNull(root, "Scena zawiera GameRoot");
            Assert.IsNotNull(root.config, "Konfiguracja przypisana w scenie");
            Assert.AreEqual(3, root.config.classes.Count(c => c.parent == null), "Trzy typy bohatera");
            Assert.AreEqual(15, root.config.classes.Count, "…i dwanaście specjalizacji");
            Assert.IsTrue(root.config.classes.All(c => c.Visual != null), "Każda klasa ma model bohatera");
            Assert.AreEqual(6, root.config.tower.floors.Count, "3 areny Hell Unleashed + 3 piętra Turris");
            Assert.AreEqual(GameScreen.MainMenu, root.Screen);

            var plan = new LoadoutPlan { classDef = root.config.classes[1], difficulty = root.config.difficulties[0] };
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);

            // Kilka sekund realnej walki: AI atakuje, gracz rzuca czary na zmianę z flaszką.
            float t = 0;
            float manaStart = root.Player.Mana.Current;
            root.Controller.ToggleLock();
            while (t < 4f)
            {
                if (root.Player.Actions.IsIdle) root.Player.RequestSkill(0);
                t += Time.deltaTime;
                yield return null;
            }
            Assert.Less(root.Player.Mana.Current, manaStart + 5f, "Czary zużywają manę");
            Assert.IsTrue(root.Screen == GameScreen.Playing || root.Screen == GameScreen.Reward || root.Screen == GameScreen.Death);
        }

        [UnityTest]
        public IEnumerator CaptureScreenshots_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać zrzuty."); yield break; }
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            Shot(root, Path.Combine(dir, "0_menu.png"));
            root.ShowControls();
            yield return UiShot(Path.Combine(dir, "0c_controls_ui.png"));
            root.CloseControls();
            root.ShowLoadout();
            yield return UiShot(Path.Combine(dir, "0b_loadout_ui.png"));
            root.Meta.SelectClass(root.Plan, root.config.classes.First(c => c.id == "class_rogue"));
            root.Meta.SelectLoadout(root.Plan, 1);
            yield return UiShot(Path.Combine(dir, "0d_loadout_rogue_ui.png"));
            root.Meta.SelectClass(root.Plan, root.config.classes.First(c => c.id == "class_hu_paladin"));
            yield return UiShot(Path.Combine(dir, "0f_loadout_paladin_ui.png"));
            root.Meta.SelectClass(root.Plan, root.config.classes.First(c => c.id == "class_hu_mage_earth"));
            yield return UiShot(Path.Combine(dir, "0g_loadout_mage_earth_ui.png"));
            root.Meta.SelectClass(root.Plan, root.config.classes[0]);
            root.ShowUnlocks();
            yield return UiShot(Path.Combine(dir, "0e_unlocks_ui.png"));
            root.ShowLoadout();

            var plan = new LoadoutPlan { classDef = root.config.classes[0], difficulty = root.config.difficulties[0] };
            root.StartRun(plan);
            float t = 0;
            while (t < 1.2f) { t += Time.deltaTime; yield return null; }
            Shot(root, Path.Combine(dir, "1_floor1_start.png"));
            root.Player.Request(ActionType.LightAttack);
            t = 0;
            while (t < 0.36f) { t += Time.deltaTime; yield return null; }
            Shot(root, Path.Combine(dir, "1b_player_slash.png"));
            t = 0;
            while (t < 0.9f) { t += Time.deltaTime; yield return null; }
            root.Player.Request(ActionType.HeavyAttack);
            t = 0;
            while (t < 0.6f) { t += Time.deltaTime; yield return null; }
            Shot(root, Path.Combine(dir, "1c_player_heavy_wind.png"));
            t = 0;
            while (t < 1.2f) { t += Time.deltaTime; yield return null; }
            root.Player.RequestSkill(PlayerCombat.SignatureSlot);
            t = 0;
            while (t < 0.3f) { t += Time.deltaTime; yield return null; }
            Shot(root, Path.Combine(dir, "1f_signature_warcry.png"));

            // Umiejętności: rozpłatanie (odnowienie w HUD), potem młynek w trzecim slocie.
            var whirl = root.Run.LearnSpell(root.config.unlocks.Select(u => u.target).OfType<SpellDefinition>().First(x => x.id == "skill_whirlwind"));
            root.Run.AssignSlot(whirl, 2);
            root.Player.RefreshBuild();
            t = 0;
            while (t < 0.9f) { t += Time.deltaTime; yield return null; }
            root.Player.RequestSkill(0);
            t = 0;
            while (t < 0.9f) { t += Time.deltaTime; yield return null; }
            root.Player.RequestSkill(2);
            t = 0;
            while (t < 0.55f) { t += Time.deltaTime; yield return null; }
            Shot(root, Path.Combine(dir, "1d_whirlwind.png"));
            yield return UiShot(Path.Combine(dir, "1e_hud_skills_ui.png"));

            // Ciężki rzut: broń w locie, bohater z pustą dłonią.
            var thr = root.Run.LearnSpell(root.config.baseSpellPool.First(x => x.id == "skill_throw"));
            root.Run.AssignSlot(thr, 1);
            root.Player.RefreshBuild();
            t = 0;
            while (t < 1.8f) { t += Time.deltaTime; yield return null; }
            root.Player.RequestSkill(1);
            t = 0;
            while (t < 0.5f) { t += Time.deltaTime; yield return null; }
            Shot(root, Path.Combine(dir, "1g_heavy_throw.png"));
            root.Controller.ToggleLock();
            bool shotTele = false;
            t = 0;
            while (t < 8f && !shotTele)
            {
                root.Player.SetBlockHeld(true);
                foreach (var e in root.Enemies)
                    if (e != null && e.IsTelegraphing) { shotTele = true; }
                t += Time.deltaTime;
                yield return null;
            }
            Shot(root, Path.Combine(dir, "2_floor1_telegraph.png"));

            root.DebugCompleteFloorNow();
            yield return UiShot(Path.Combine(dir, "1f_reward_ui.png"));
            root.ChooseReward(0);
            root.Run.souls = 420;
            yield return UiShot(Path.Combine(dir, "2a_intermission_shop_ui.png"));
            root.OpenEquipment();
            yield return UiShot(Path.Combine(dir, "2b_equipment_ui.png"));
            root.CloseEquipment();
            root.NextFloor();
            root.DebugCompleteFloorNow();
            root.ChooseReward(0);
            root.NextFloor();
            root.DebugCompleteFloorNow();
            root.ChooseReward(0);
            root.NextFloor();
            root.DebugCompleteFloorNow();
            root.ChooseReward(0);
            root.NextFloor();
            t = 0;
            root.Controller.ToggleLock();
            while (t < 2.5f) { t += Time.deltaTime; yield return null; }
            Shot(root, Path.Combine(dir, "3_boss.png"));
        }

        /// <summary>Bestiariusz: spotkani wrogowie z portretami i opisem, niespotkani – „???”; słabości po zabiciach.</summary>
        [UnityTest]
        public IEnumerator Bestiary_ShowsMetEnemies_WeaknessesAfterKills()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            var entries = Bestiary.Entries(root.config);
            var imp = entries.First(e => e.def.id == "enemy_imp");
            Assert.AreEqual(3, imp.variants.Count, "chochlik z trzema odmianami żywiołów w jednej karcie");
            Assert.IsTrue(entries.Any(e => e.def.id == "enemy_minion"), "przyzywane sługi też w bestiariuszu");
            // Spotkani: chochlik (odmiana liczy się do bazowego), kultysta, Władca Bramy.
            root.Meta.RecordSeen(imp.variants[0]);
            for (int i = 0; i < Bestiary.RevealKills; i++) root.Meta.RecordKill(imp.variants[i % 3]);
            var cultist = entries.First(e => e.def.id == "enemy_hu_cultist").def;
            root.Meta.RecordSeen(cultist);
            root.Meta.RecordKill(cultist);
            var lord = entries.First(e => e.def.id == "enemy_gatelord").def;
            root.Meta.RecordSeen(lord);
            root.Meta.RecordKill(lord);
            Assert.IsTrue(root.Meta.Seen(imp.def));
            Assert.AreEqual(Bestiary.RevealKills, root.Meta.Kills(imp.def));
            Assert.IsNotEmpty(Bestiary.Weaknesses(imp.def), "chochlik ma słabości");
            Assert.IsTrue(Bestiary.Strengths(lord).Any(s => s.Contains("szarża")), "Władca Bramy – szarża w mocnych stronach");

            var ui = root.GetComponent<GameUI>();
            ui.OpenBestiary();
            for (int i = 0; i < 20; i++) yield return null;
            Assert.AreEqual(GameScreen.Bestiary, root.Screen);
            var stage = Object.FindAnyObjectByType<BestiaryStage>();
            Assert.IsNotNull(stage);
            Assert.AreEqual(0, stage.PendingPortraits, "portrety kart wyrenderowane");
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (!string.IsNullOrEmpty(dir)) yield return UiShot(Path.Combine(dir, "bestiary_ui.png"));
            ui.Back();
            yield return null;
            Assert.AreEqual(GameScreen.MainMenu, root.Screen);
            Assert.IsNull(Object.FindAnyObjectByType<BestiaryStage>(), "scena podglądu sprzątnięta");
        }

        /// <summary>Zrzut całego ekranu gry razem z interfejsem IMGUI (działa tylko w oknie edytora).</summary>
        static IEnumerator UiShot(string path)
        {
            // W -batchmode WaitForEndOfFrame nigdy nie nadchodzi (i OnGUI nie działa) – zrzut z UI tylko w oknie.
            if (Application.isBatchMode) yield break;
            for (int i = 0; i < 3; i++) yield return null;
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            if (tex != null && tex.width > 16) File.WriteAllBytes(path, tex.EncodeToPNG());
            if (tex != null) Object.Destroy(tex);
        }

        static void Shot(GameRoot root, string path)
        {
            var cam = root.CameraRig.GetComponent<Camera>();
            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.Destroy(rt);
            Object.Destroy(tex);
        }
    }
}
