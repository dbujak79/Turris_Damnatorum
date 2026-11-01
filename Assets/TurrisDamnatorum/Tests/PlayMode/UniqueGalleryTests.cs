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
    /// Galeria mocy unikatów (opt-in: TURRIS_SHOT_DIR). Świat (tryb gfx): byty mocy widoczne w miejscu działania – mina z novy,
    /// płyty ruchomego bastionu, orbita lewego ostrza, gniewne widma, zapowiedź uderzenia z nieba, rząd strzał, wir klingi,
    /// zakotwiczona broń. Interfejs (tylko tryb window – OnGUI): ekwipunek z unikatem i katalog.
    /// </summary>
    public class UniqueGalleryTests
    {
        readonly List<Object> cleanup = new List<Object>();
        DefaultContent.Bundle bundle;
        string dir;
        Camera cam;

        [SetUp]
        public void SetUp()
        {
            dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię unikatów.");
            dir = Path.Combine(dir, "unique-items");
            Directory.CreateDirectory(dir);
            bundle = TestContent.Create();
            EnemyBrain.DebugNoEvasion = true;
            UniqueRegistry.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Servant.DismissAll(); FeatureDecoy.ClearAll(); EarthWall.ClearAll(); HuWorld.Clear();
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            EnemyBrain.DebugNoEvasion = false; UniqueMines.HoldContact = null;
            GameRoot.ProfileFileOverride = null; Time.timeScale = 1f;
        }

        PlayerCombat Hero(string classId, string featureId, string[] uniques, string main, string off, params string[] skills)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0); floor.transform.localScale = new Vector3(60, 1, 60);
            floor.GetComponent<Renderer>().material.color = new Color(0.22f, 0.2f, 0.22f);
            cleanup.Add(floor);
            var light = new GameObject("Sun").AddComponent<Light>(); light.type = LightType.Directional; light.transform.rotation = Quaternion.Euler(50, -30, 0); light.intensity = 1.2f;
            cleanup.Add(light.gameObject);
            var camGo = new GameObject("GalleryCam"); cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.07f, 0.06f, 0.09f);
            var rig = camGo.AddComponent<CameraRig>(); rig.enabled = false;
            cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer(); cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>(); ctrl.cameraRig = rig; ctrl.InputEnabled = false;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0], classFeatureId = featureId };
            var run = RunFactory.Create(plan, bundle.config, 7);
            run.attributes.intelligence = 30; run.attributes.strength = 30; run.attributes.dexterity = 30;
            void Eq(string id, EquipSlot s) { if (id == null) return; var it = new ItemInstance(bundle.Get<ItemDefinition>(id)); run.inventory.Add(it); run.EquipFromInventory(it, s); }
            Eq(main, EquipSlot.MainHand); Eq(off, EquipSlot.OffHand);
            foreach (var u in uniques)
            {
                var it = HellEconomy.Roll(UniqueRegistry.Find(bundle.config, "item.unique.turris." + u), LootSource.Boss, 8, new System.Random(1));
                run.inventory.Add(it); run.EquipFromInventory(it, it.definition.DefaultSlot);
            }
            run.SyncGrantedSkills();
            for (int i = 0; i < skills.Length; i++) { var d = bundle.Get<SpellDefinition>(skills[i]); run.AssignSlot(run.FindSpell(d) ?? run.LearnSpell(d, true), i); }
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(run, bundle.config);
            pc.DebugInvulnerable = true; pc.DebugNoPassiveBlock = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            Look(new Vector3(5.5f, 5f, -5.5f), new Vector3(0, 0.5f, 2.5f));
            return pc;
        }

        void Look(Vector3 from, Vector3 at) { cam.transform.position = from; cam.transform.LookAt(at); }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, bool attacks = false)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = 50000; def.maxPoise = 100000; def.scale = 1f; def.moveSpeed = attacks ? 2f : 0f; def.strafeChance = 0f;
            if (attacks) def.attacks.Add(new EnemyAttackEntry { attack = new AttackDefinition { name = "Pazur", baseDamage = 5f, reach = 2f, windup = 0.4f }, maxRange = 3f });
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }

        void Shot(string name)
        {
            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            cam.targetTexture = null; RenderTexture.active = null;
            Object.Destroy(rt); Object.Destroy(tex);
        }

        void Clear() { TearDownWorld(); }
        void TearDownWorld()
        {
            Servant.DismissAll(); EarthWall.ClearAll(); HuWorld.Clear();
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        static void Face(PlayerCombat pc, EnemyBrain e) { Vector3 d = e.transform.position - pc.transform.position; d.y = 0; pc.transform.rotation = Quaternion.LookRotation(d); pc.LockTarget = e; }

        [UnityTest]
        public IEnumerator Gallery_UniquePowersInWorld()
        {
            // M03: nova jako mina w punkcie celu (znak na ziemi).
            var pc = Hero("class_mage", null, new[] { "mage.sleeping_circles" }, null, null, "hu_nova_fire");
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f));
            yield return new WaitForSeconds(0.1f);
            Face(pc, e); pc.LockTarget = null; pc.transform.rotation = Quaternion.identity;
            pc.RequestSkill(0);
            yield return new WaitForSeconds(0.6f);
            Shot("u_M03_mina_novy");
            Clear(); yield return null;

            // E01: dwie boczne płyty prowadzone z postacią.
            pc = Hero("class_hu_mage_earth", "class_feature.earth.bastion", new[] { "earth.moving_bastion" }, null, null);
            yield return new WaitForSeconds(0.1f);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(0.3f);
            Look(new Vector3(4f, 5f, -4.5f), new Vector3(0, 0.6f, 0.5f));
            Shot("u_E01_ruchomy_bastion");
            Clear(); yield return null;

            // R03: lewe ostrze na orbicie wokół Rębajły.
            pc = Hero("class_hu_dual", null, new[] { "dual.blade_juggler" }, "hu_sword", "hu_dagger");
            yield return new WaitForSeconds(0.1f);
            pc.Request(ActionType.HeavyAttack);
            yield return new WaitForSeconds(0.9f);
            Look(new Vector3(3.5f, 4f, -3.5f), new Vector3(0, 0.8f, 0f));
            Shot("u_R03_orbita_ostrza");
            Clear(); yield return null;

            // B02: gniewne widma ze Spalenia furii.
            pc = Hero("class_hu_berserker", "class_feature.berserker.fury_spend", new[] { "berserker.angry_spirits" }, "hu_greataxe", null);
            e = Enemy(pc, new Vector3(0, 0.05f, 4f));
            yield return new WaitForSeconds(0.1f);
            pc.AddFury(100f);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(0.7f);
            Look(new Vector3(5f, 4.5f, -3f), new Vector3(0, 0.8f, 2f));
            Shot("u_B02_gniewne_widma");
            Clear(); yield return null;

            // F05: strzał z Kręgu żaru – zapowiedź uderzenia z nieba w celu.
            pc = Hero("class_hu_mage_fire", null, new[] { "fire.sky_furnace" }, "hu_wand_fire", null, "hu_ember_ring");
            e = Enemy(pc, new Vector3(0, 0.05f, 7f));
            yield return new WaitForSeconds(0.1f);
            pc.LockTarget = null; pc.AssistTarget = null;
            var ringTarget = pc.transform.position;
            pc.RequestSkill(0); // krąg wokół celu = tu: przed postacią; ustawiamy postać w kręgu
            yield return new WaitForSeconds(0.7f);
            var z = DamageZone.Active.LastOrDefault();
            if (z != null) { var cc = pc.GetComponent<CharacterController>(); cc.enabled = false; pc.transform.position = z.transform.position; cc.enabled = true; }
            Face(pc, e);
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.5f);
            Look(new Vector3(5f, 6f, -2f), new Vector3(0, 0.3f, 4f));
            Shot("u_F05_uderzenie_z_nieba");
            Clear(); yield return null;

            // K04: rząd strzał wbity w ziemię.
            pc = Hero("class_hu_archer", null, new[] { "archer.unfinished_volley" }, "hu_bow", null, "hu_volley");
            yield return new WaitForSeconds(0.1f);
            pc.LockTarget = null; pc.transform.rotation = Quaternion.identity;
            pc.RequestSkill(0);
            yield return new WaitForSeconds(0.7f);
            Look(new Vector3(3.5f, 3.5f, -2.5f), new Vector3(0, 0.3f, 2f));
            Shot("u_K04_rzad_strzal");
            Clear(); yield return null;

            // R05: klinga wiruje w miejscu startu Młynka, Rębajło walczy lewą.
            pc = Hero("class_hu_dual", null, new[] { "dual.wandering_blade" }, "hu_sword", "hu_dagger", "hu_whirlwind");
            yield return new WaitForSeconds(0.1f);
            pc.RequestSkill(0);
            yield return new WaitForSeconds(0.3f);
            var cc2 = pc.GetComponent<CharacterController>(); cc2.enabled = false; pc.transform.position = new Vector3(2.5f, 0.05f, 1.5f); cc2.enabled = true;
            yield return new WaitForSeconds(0.2f);
            Look(new Vector3(4.5f, 4.5f, -4f), new Vector3(1f, 0.8f, 0.8f));
            Shot("u_R05_zblakana_klinga");
            Clear(); yield return null;

            // B04: broń wbita w ofierze po Ciężkim rzucie.
            pc = Hero("class_hu_berserker", null, new[] { "berserker.chained_butcher" }, "hu_greataxe", null, "skill_throw");
            e = Enemy(pc, new Vector3(0, 0.05f, 5f));
            yield return new WaitForSeconds(0.1f);
            Face(pc, e);
            pc.RequestSkill(0);
            yield return new WaitForSeconds(0.9f);
            Look(new Vector3(4f, 3.5f, 0f), new Vector3(0, 1f, 3.5f));
            Shot("u_B04_zakotwiczona_bron");
            Clear(); yield return null;
        }

        [UnityTest]
        public IEnumerator Ui_EquipmentWithUnique_AndCatalog()
        {
            if (Application.isBatchMode) Assert.Ignore("Zrzuty interfejsu tylko w trybie window (OnGUI).");
            string profile = $"turris_unique_ui_{System.Guid.NewGuid():N}.json";
            GameRoot.ProfileFileOverride = profile;
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            Assert.IsNotNull(root);
            var plan = new LoadoutPlan { classDef = root.config.classes.First(c => c.id == "class_mage"), difficulty = root.config.difficulties[0], classFeatureId = "class_feature.mage.arcane_debt" };
            root.StartRun(plan);
            root.Player.DebugInvulnerable = true;
            for (float t = 0; t < 0.8f; t += Time.deltaTime) yield return null;
            var run = root.Run;
            foreach (var id in new[] { "mage.moneylenders_note", "mage.late_presence", "warrior.bellringer" })
            {
                var it = HellEconomy.Roll(UniqueRegistry.Find(root.config, "item.unique.turris." + id), LootSource.Boss, 8, new System.Random(1));
                run.AddItem(it, id != "warrior.bellringer");
                root.Meta.DiscoverUnique(it.definition.id);
            }
            root.Player.RefreshBuild();
            root.Run.manaDebt = 20f;
            for (int i = 0; i < 20; i++) yield return null;
            yield return UiShot(Path.Combine(dir, "ui_unique_hud.png"));
            root.OpenEquipment();
            yield return UiShot(Path.Combine(dir, "ui_unique_equipment.png"));
            root.CloseEquipment();
            root.AbandonRun();
            yield return null;
            root.Meta.SetSoughtUnique("item.unique.turris.mage.late_presence");
            var ui = root.GetComponent<GameUI>();
            typeof(GameUI).GetField("showUniqueCatalog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ui, true);
            yield return UiShot(Path.Combine(dir, "ui_unique_catalog.png"));
            string path = Path.Combine(Application.persistentDataPath, profile);
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
    }
}
