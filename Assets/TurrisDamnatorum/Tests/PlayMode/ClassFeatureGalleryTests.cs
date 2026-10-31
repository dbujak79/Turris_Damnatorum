using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Galeria cech klasowych (opt-in: TURRIS_SHOT_DIR): widoczne działanie wariantów z bytami w świecie – ściana Bastionu,
    /// wabik, przygwożdżony cień, stanowisko, chimera, forma Wcielenia, trans Czerwonej linii, Oko ciszy, płyty i pola.
    /// Zrzuty z kamery (tryb gfx); interfejs (karty A/B, HUD) – w trybie window przez MainScene/Gamepad.
    /// </summary>
    public class ClassFeatureGalleryTests
    {
        readonly List<Object> cleanup = new List<Object>();
        DefaultContent.Bundle bundle;
        string dir;

        [SetUp]
        public void SetUp()
        {
            dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię cech.");
            dir = Path.Combine(dir, "class-features");
            Directory.CreateDirectory(dir);
            bundle = TestContent.Create();
            EnemyBrain.DebugNoEvasion = true;
        }

        [TearDown]
        public void TearDown()
        {
            Servant.DismissAll(); FeatureDecoy.ClearAll(); PinnedShadow.ClearAll(); EarthWall.ClearAll();
            foreach (var fp in Object.FindObjectsByType<FiringPost>(FindObjectsInactive.Exclude)) Object.Destroy(fp.gameObject);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            EnemyBrain.DebugNoEvasion = false;
        }

        Camera cam;

        PlayerCombat Hero(string classId, string featureId, int loadout = 0)
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
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0], loadoutIndex = loadout, classFeatureId = featureId };
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(RunFactory.Create(plan, bundle.config, 7), bundle.config);
            pc.DebugInvulnerable = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            Look(new Vector3(5.5f, 4.2f, -5.5f), new Vector3(0, 1f, 2.5f));
            return pc;
        }

        void Look(Vector3 from, Vector3 at) { cam.transform.position = from; cam.transform.LookAt(at); }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, bool attacks = false)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = 50000; def.maxPoise = 100000; def.scale = 1f; def.moveSpeed = attacks ? 2.5f : 0f; def.strafeChance = 0f;
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

        static void Press(PlayerCombat pc) => pc.RequestSkill(PlayerCombat.SignatureSlot);

        [UnityTest]
        public IEnumerator Gallery_WorldEntitiesOfFeatures()
        {
            // Bastion ziemi: widoczna ściana = kolizja.
            var pc = Hero("class_hu_mage_earth", "class_feature.earth.bastion");
            yield return new WaitForSeconds(0.3f);
            Press(pc);
            yield return new WaitForSeconds(0.4f);
            Shot("cf_bastion_wall");
            TearDown(); SetUp();

            // Fałszywy świadek: wabik i wróg, który w niego bije.
            pc = Hero("class_rogue", "class_feature.rogue.false_witness", 1);
            var e = Enemy(pc, new Vector3(0, 0.05f, 3f), attacks: true);
            yield return new WaitForSeconds(0.2f);
            Press(pc);
            pc.GetComponent<CharacterController>().enabled = false; pc.transform.position = new Vector3(-3f, 0f, -2f); pc.GetComponent<CharacterController>().enabled = true;
            yield return new WaitForSeconds(1.2f);
            Shot("cf_false_witness_decoy");
            TearDown(); SetUp();

            // Przygwożdżony cień: cień przy dawnym miejscu ofiary i nić do ciała.
            pc = Hero("class_hu_assassin", "class_feature.assassin.pinned_shadow");
            e = Enemy(pc, new Vector3(0, 0.05f, 3f));
            yield return new WaitForSeconds(0.2f);
            pc.LockTarget = e; Press(pc);
            e.transform.position = new Vector3(2.5f, 0.05f, 4.5f);
            yield return new WaitForSeconds(0.4f);
            Shot("cf_pinned_shadow");
            TearDown(); SetUp();

            // Stanowisko strzeleckie: punkt wylotu i strzała z niego.
            pc = Hero("class_hu_archer", "class_feature.archer.firing_position");
            e = Enemy(pc, new Vector3(4f, 0.05f, 9f));
            yield return new WaitForSeconds(0.2f);
            Press(pc); pc.LockTarget = e; pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.55f);
            Look(new Vector3(-5f, 4f, -4f), new Vector3(1.5f, 1f, 4f));
            Shot("cf_firing_post");
            TearDown(); SetUp();

            // Przywoływacz: chimera z dwóch chochlików, potem forma Wcielenia.
            pc = Hero("class_hu_summoner", "class_feature.summoner.chimera");
            yield return new WaitForSeconds(0.2f);
            pc.RequestSkill(pc.Run.SlotOf(pc.Run.FindSpell(bundle.Get<SpellDefinition>("hu_summon_imps"))));
            yield return new WaitForSeconds(0.9f);
            Press(pc); Press(pc);
            yield return new WaitForSeconds(0.6f);
            Shot("cf_chimera");
            TearDown(); SetUp();

            pc = Hero("class_hu_summoner", "class_feature.summoner.incarnation");
            e = Enemy(pc, new Vector3(0, 0.05f, 4f));
            yield return new WaitForSeconds(0.2f);
            pc.RequestSkill(pc.Run.SlotOf(pc.Run.FindSpell(bundle.Get<SpellDefinition>("hu_summon_imps"))));
            yield return new WaitForSeconds(0.9f);
            Press(pc); pc.LockTarget = e; pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.25f);
            Shot("cf_incarnation_imp_form");
            Press(pc);
            yield return new WaitForSeconds(0.3f);
            Shot("cf_incarnation_back_to_hero");
            TearDown(); SetUp();

            // Czerwona linia: trans, ryk.
            pc = Hero("class_hu_berserker", "class_feature.berserker.redline");
            Enemy(pc, new Vector3(0, 0.05f, 2.5f));
            yield return new WaitForSeconds(0.2f);
            Press(pc);
            yield return new WaitForSeconds(0.25f);
            Shot("cf_redline_trance");
            TearDown(); SetUp();

            // Oko ciszy: własne pociski w magazynie i salwa.
            pc = Hero("class_hu_mage_air", "class_feature.air.silent_eye");
            e = Enemy(pc, new Vector3(0, 0.05f, 7f));
            yield return new WaitForSeconds(0.2f);
            Press(pc); pc.LockTarget = e;
            for (int i = 0; i < 2; i++) { pc.Request(ActionType.LightAttack); yield return new WaitForSeconds(0.6f); }
            Shot("cf_silent_eye_loaded");
            Press(pc);
            yield return new WaitForSeconds(0.15f);
            Shot("cf_silent_eye_release");
        }
    }
}
