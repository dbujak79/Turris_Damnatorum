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
    /// Zrzuty interfejsu cech klasowych (opt-in: TURRIS_SHOT_DIR, tylko tryb window – OnGUI): karty A/B w przygotowaniu,
    /// wybór Arsenału, podsumowanie, okienko cechy w HUD, rezerwacja Czerwonej linii, dług many i zestaw II w ekwipunku.
    /// </summary>
    public class ClassFeatureUiShots
    {
        string profileFile, dir;

        [SetUp]
        public void SetUp()
        {
            dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać zrzuty interfejsu cech.");
            if (Application.isBatchMode) Assert.Ignore("Zrzuty interfejsu tylko w trybie window (OnGUI).");
            dir = Path.Combine(dir, "class-features-ui");
            Directory.CreateDirectory(dir);
            profileFile = $"turris_cf_ui_{System.Guid.NewGuid():N}.json";
            GameRoot.ProfileFileOverride = profileFile;
        }

        [TearDown]
        public void TearDown()
        {
            GameRoot.ProfileFileOverride = null;
            Time.timeScale = 1f;
            if (profileFile == null) return;
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
        }

        static IEnumerator UiShot(string path)
        {
            for (int i = 0; i < 4; i++) yield return null;
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            if (tex != null && tex.width > 16) File.WriteAllBytes(path, tex.EncodeToPNG());
            if (tex != null) Object.Destroy(tex);
        }

        static IEnumerator Wait(float s) { float t = 0; while (t < s) { t += Time.deltaTime; yield return null; } }

        ClassDefinition Cls(GameRoot root, string id) => root.config.classes.First(c => c.id == id);

        [UnityTest]
        public IEnumerator Capture_FeatureUi()
        {
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            Assert.IsNotNull(root);

            // Przygotowanie: karty A/B (wybrany A), potem B; Arsenał z drugim zestawem; mag i łucznik arkanów.
            root.ShowLoadout();
            root.Meta.SelectClass(root.Plan, Cls(root, "class_hu_berserker"));
            yield return UiShot(Path.Combine(dir, "ui_1_loadout_berserker_A.png"));
            root.Meta.SelectFeature(root.Plan, "class_feature.berserker.fury_spend");
            yield return UiShot(Path.Combine(dir, "ui_2_loadout_berserker_B.png"));
            root.Meta.SelectClass(root.Plan, Cls(root, "class_knight"));
            root.Meta.SelectFeature(root.Plan, "class_feature.warrior.arsenal");
            yield return UiShot(Path.Combine(dir, "ui_3_loadout_warrior_arsenal.png"));
            root.Meta.SelectClass(root.Plan, Cls(root, "class_hu_arcane_archer"));
            yield return UiShot(Path.Combine(dir, "ui_4_loadout_arcane_archer.png"));

            // HUD: Czerwona linia – rezerwacja paska życia i bieżąca redukcja.
            var plan = new LoadoutPlan { classDef = Cls(root, "class_hu_berserker"), difficulty = root.config.difficulties[0], classFeatureId = "class_feature.berserker.redline" };
            root.StartRun(plan);
            root.Player.DebugInvulnerable = true;
            yield return Wait(1f);
            yield return UiShot(Path.Combine(dir, "ui_5_hud_redline_ready.png"));
            root.Player.Health.SetCurrent(root.Player.Health.Max * 0.6f);
            root.Player.RequestSkill(PlayerCombat.SignatureSlot);
            yield return Wait(0.6f);
            yield return UiShot(Path.Combine(dir, "ui_6_hud_redline_trance.png"));
            root.AbandonRun();
            yield return null;

            // HUD: dług arkanów.
            plan = new LoadoutPlan { classDef = Cls(root, "class_mage"), difficulty = root.config.difficulties[0], classFeatureId = "class_feature.mage.arcane_debt" };
            root.StartRun(plan);
            root.Player.DebugInvulnerable = true;
            yield return Wait(0.8f);
            var pc = root.Player;
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            pc.Mana.SetCurrent(0f);
            int slot = Enumerable.Range(0, RunState.SkillSlotCount).First(s => pc.Skill(s) != null && pc.Skill(s).Def.IsSpell);
            pc.RequestSkill(slot);
            yield return Wait(0.5f);
            yield return UiShot(Path.Combine(dir, "ui_7_hud_arcane_debt_active.png"));
            yield return Wait(root.config.classFeatures.debtSeconds);
            yield return UiShot(Path.Combine(dir, "ui_8_hud_arcane_debt_blocked.png"));
            root.AbandonRun();
            yield return null;

            // Ekwipunek: Arsenał – zestaw II.
            plan = new LoadoutPlan { classDef = Cls(root, "class_knight"), difficulty = root.config.difficulties[0], classFeatureId = "class_feature.warrior.arsenal" };
            root.StartRun(plan);
            root.Player.DebugInvulnerable = true;
            yield return Wait(0.8f);
            yield return UiShot(Path.Combine(dir, "ui_9_hud_arsenal.png"));
            root.OpenEquipment();
            yield return UiShot(Path.Combine(dir, "ui_10_equipment_arsenal.png"));
            root.CloseEquipment();
            root.AbandonRun();
        }
    }
}
