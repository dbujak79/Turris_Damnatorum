using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    public class StubAnimSource : ICharacterAnimSource
    {
        public CharacterAnimState state;
        public CharacterAnimState GetAnimState() => state;
    }

    /// <summary>Animacja humanoida: synchronizacja z fazami walki, poprawność IK, sterownik klipów, galeria póz.</summary>
    public class AnimationPlayTests
    {
        readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        (CharacterVisual visual, StubAnimSource src) Dummy(RigLook look, Vector3 pos, float yaw = 180f, float scale = 1f)
        {
            var go = new GameObject("Dummy");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, yaw, 0);
            var v = go.AddComponent<CharacterVisual>();
            v.BuildProcedural(look, scale);
            var src = new StubAnimSource();
            v.Init(src);
            cleanup.Add(go);
            return (v, src);
        }

        static CharacterAnimState Attack(AttackAnim a, ActionPhase phase, float p) =>
            new CharacterAnimState { action = AnimAction.Attack, attack = a, phase = phase, phaseProgress = p };

        static RigLook Knight() => new RigLook { body = BodyGear.Chain, head = HeadGear.GreatHelm, cape = true, weapon = WeaponModel.Sword, shield = ShieldModel.Heater };

        [UnityTest]
        public IEnumerator SlashRight_BladeTravelsFromBehindRightToFrontLeft()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            Transform ch = v.transform;

            src.state = Attack(AttackAnim.SlashRight, ActionPhase.Startup, 1f);
            v.Tick(1f / 60f);
            Vector3 windTip = ch.InverseTransformPoint(v.WeaponTip);

            src.state = Attack(AttackAnim.SlashRight, ActionPhase.Active, 0.5f);
            v.Tick(1f / 60f);
            Vector3 hitTip = ch.InverseTransformPoint(v.WeaponTip);

            src.state = Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f);
            v.Tick(1f / 60f);
            Vector3 followTip = ch.InverseTransformPoint(v.WeaponTip);
            yield return null;

            Assert.Greater(windTip.x, 0.2f, "Zamach: ostrze po prawej stronie");
            Assert.Less(windTip.z, hitTip.z, "Zamach: ostrze cofnięte względem trafienia");
            Assert.Greater(hitTip.z, 0.9f, "Trafienie: ostrze wyraźnie przed postacią");
            Assert.Less(followTip.x, -0.2f, "Wybrzmienie: ostrze po lewej stronie");
        }

        [UnityTest]
        public IEnumerator Overhead_BladeGoesFromAboveHeadToLowFront()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = Attack(AttackAnim.Overhead, ActionPhase.Startup, 1f);
            v.Tick(1f / 60f);
            Vector3 wind = v.transform.InverseTransformPoint(v.WeaponTip);
            src.state = Attack(AttackAnim.Overhead, ActionPhase.Active, 1f);
            v.Tick(1f / 60f);
            Vector3 follow = v.transform.InverseTransformPoint(v.WeaponTip);
            yield return null;
            Assert.Greater(wind.y, 1.6f, "Szczyt zamachu nad głową");
            Assert.Less(follow.y, 0.9f, "Cios kończy się nisko");
            Assert.Greater(follow.z, 0.5f, "…i przed postacią");
        }

        [UnityTest]
        public IEnumerator IK_HandReachesTarget()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = new CharacterAnimState { action = AnimAction.Block };
            for (int i = 0; i < 60; i++) v.Tick(1f / 60f);
            yield return null;
            var rig = v.Rig;
            var pose = ((ProceduralHumanoidAnimator)v.Driver).Current;
            Vector3 want = rig.VisualRoot.TransformPoint(pose.handL);
            float reach = (rig.UpperArmLength + rig.ForearmLength);
            float dist = Vector3.Distance(rig[Bone.UpperArmL].position, want);
            if (dist < reach * 0.98f)
                Assert.Less(Vector3.Distance(rig[Bone.HandL].position, want), 0.02f, "IK doprowadza dłoń do celu");
            foreach (var b in rig.bones) Assert.IsFalse(float.IsNaN(b.position.x) || float.IsNaN(b.rotation.x), $"NaN w kości {b.name}");
        }

        [UnityTest]
        public IEnumerator Dodge_RollsWholeBody_AndDeathEndsOnGround()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = new CharacterAnimState { action = AnimAction.Dodge, actionTime = 0.23f, actionDuration = 0.75f, dodgeDirection = Vector3.forward };
            v.Tick(1f / 60f);
            float midRollHeadY = v.Rig[Bone.Head].position.y;
            src.state = new CharacterAnimState { action = AnimAction.Death, actionTime = 2f };
            v.Tick(1f / 60f);
            float deadHeadY = v.Rig[Bone.Head].position.y;
            yield return null;
            Assert.Less(midRollHeadY, 1.0f, "W połowie przewrotu głowa jest nisko");
            Assert.Less(deadHeadY, 0.5f, "Po śmierci ciało leży na ziemi");
        }

        [UnityTest]
        public IEnumerator PlayerCombat_DrivesRigInSyncWithActionPhases()
        {
            var bundle = DefaultContent.Create();
            var p = WorldBuilder.CreatePlayer();
            cleanup.Add(p);
            p.GetComponent<PlayerController>().enabled = false;
            var pc = p.GetComponent<PlayerCombat>();
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("class_knight"), difficulty = bundle.config.difficulties[0] };
            pc.Init(RunFactory.Create(plan, bundle.config, 1), bundle.config);
            p.GetComponent<PlayerVisuals>().Refresh();
            var v = p.GetComponent<CharacterVisual>();
            Assert.IsNotNull(v.Rig, "Gracz ma proceduralnego humanoida");
            Assert.AreEqual(WeaponModel.Sword, v.Rig.Look.weapon);
            Assert.AreEqual(ShieldModel.Heater, v.Rig.Look.shield);
            Assert.AreEqual(HeadGear.Coif, v.Rig.Look.head, "Rycerz bez hełmu nosi kaptur kolczy pod kolczugą");

            pc.Request(ActionType.LightAttack);
            var st = pc.GetAnimState();
            Assert.AreEqual(AnimAction.Attack, st.action);
            Assert.AreEqual(AttackAnim.SlashRight, st.attack);
            yield return null;

            // Zmiana sprzętu przebudowuje wygląd: topór zamiast miecza.
            var run = pc.Run;
            var axe = new ItemInstance(bundle.Get<ItemDefinition>("weapon_axe"));
            run.inventory.Add(axe);
            run.EquipFromInventory(axe, EquipSlot.MainHand);
            pc.RefreshBuild();
            Assert.AreEqual(WeaponModel.Axe, v.Rig.Look.weapon);
        }

        [UnityTest]
        public IEnumerator Enemies_UseStyledHumanoids()
        {
            var bundle = DefaultContent.Create();
            var ids = new[] { "enemy_ghoul", "enemy_heretic", "enemy_warden", "enemy_castellan" };
            var expected = new[] { RigStyle.Ghoul, RigStyle.Heretic, RigStyle.Warden, RigStyle.Castellan };
            for (int i = 0; i < ids.Length; i++)
            {
                var def = bundle.Get<EnemyDefinition>(ids[i]);
                var e = WorldBuilder.CreateEnemy(def, new Vector3(i * 3, 0, 10), null);
                cleanup.Add(e.gameObject);
                var v = e.GetComponent<CharacterVisual>();
                Assert.IsNotNull(v.Rig);
                Assert.AreEqual(expected[i], v.Rig.Look.style);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClipDriver_ScrubsActionClipByCombatPhase()
        {
            // Model „Generic” z jedną kością i klipami wygenerowanymi w kodzie – sprawdza mapowanie faz na czas klipu.
            var model = new GameObject("GenericModel");
            cleanup.Add(model);
            var arm = new GameObject("Arm").transform;
            arm.SetParent(model.transform, false);
            model.AddComponent<Animator>();

            AnimationClip Clip(string name, float from, float to, float len)
            {
                var c = new AnimationClip { name = name };
                c.SetCurve("Arm", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, from, len, to));
                c.SetCurve("Arm", typeof(Transform), "localPosition.y", AnimationCurve.Constant(0, len, 0));
                c.SetCurve("Arm", typeof(Transform), "localPosition.z", AnimationCurve.Constant(0, len, 0));
                return c;
            }
            var def = ScriptableObject.CreateInstance<CharacterVisualDefinition>();
            def.idle = Clip("idle", 0, 0, 1f);
            def.actions.Add(new ActionClip { action = AnimAction.Attack, clip = Clip("attack", 0f, 10f, 2f), windupEnd = 0.4f, activeEnd = 0.6f });

            var driver = new ClipAnimationDriver(model.GetComponent<Animator>(), def, model.transform);
            try
            {
                driver.Tick(Attack(AttackAnim.SlashRight, ActionPhase.Startup, 0.5f), Vector3.zero, 0.016f);
                Assert.AreEqual(2f * 0.2f, driver.CurrentActionTime, 1e-3f, "Połowa zamachu = 0.5 × windupEnd");
                Assert.AreEqual(2f, arm.localPosition.x, 0.05f);

                driver.Tick(Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f), Vector3.zero, 0.016f);
                Assert.AreEqual(2f * 0.6f, driver.CurrentActionTime, 1e-3f, "Koniec fazy aktywnej = activeEnd");
                Assert.AreEqual(6f, arm.localPosition.x, 0.05f);
            }
            finally { driver.Dispose(); }
            yield return null;
        }

        // ------------------------------------------------------------------ Galeria (zrzuty do przeglądu)

        [UnityTest]
        public IEnumerator PoseGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię póz."); yield break; }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(60, 1, 60);
            ground.GetComponent<Renderer>().material.color = new Color(0.35f, 0.34f, 0.33f);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(45, 150, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.42f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 40;

            var knight = Knight();
            var states = new List<(string, CharacterAnimState)>
            {
                ("ready", new CharacterAnimState()),
                ("slashR-wind", Attack(AttackAnim.SlashRight, ActionPhase.Startup, 1f)),
                ("slashR-hit", Attack(AttackAnim.SlashRight, ActionPhase.Active, 0.5f)),
                ("slashR-follow", Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f)),
                ("overhead-wind", Attack(AttackAnim.Overhead, ActionPhase.Startup, 1f)),
                ("overhead-follow", Attack(AttackAnim.Overhead, ActionPhase.Active, 1f)),
                ("thrust-hit", Attack(AttackAnim.Thrust, ActionPhase.Active, 0.5f)),
                ("block", new CharacterAnimState { action = AnimAction.Block }),
                ("parry", new CharacterAnimState { action = AnimAction.Parry, phase = ActionPhase.Active, phaseProgress = 0.6f }),
            };
            var row2 = new List<(RigLook, float, CharacterAnimState)>
            {
                (new RigLook { body = BodyGear.Robe, head = HeadGear.Hood, style = RigStyle.Mage, weapon = WeaponModel.Staff, cloth = new Color(0.2f,0.25f,0.6f) }, 1f, new CharacterAnimState()),
                (new RigLook { body = BodyGear.Robe, head = HeadGear.Hood, style = RigStyle.Mage, weapon = WeaponModel.Staff, cloth = new Color(0.2f,0.25f,0.6f) }, 1f, Attack(AttackAnim.Cast, ActionPhase.Active, 1f)),
                (RigLook.ForEnemy(DefaultContent.Create().Get<EnemyDefinition>("enemy_ghoul")), 0.95f, new CharacterAnimState()),
                (RigLook.ForEnemy(DefaultContent.Create().Get<EnemyDefinition>("enemy_ghoul")), 0.95f, Attack(AttackAnim.Claw, ActionPhase.Startup, 1f)),
                (RigLook.ForEnemy(DefaultContent.Create().Get<EnemyDefinition>("enemy_heretic")), 1f, Attack(AttackAnim.Cast, ActionPhase.Startup, 1f)),
                (RigLook.ForEnemy(DefaultContent.Create().Get<EnemyDefinition>("enemy_warden")), 1.25f, new CharacterAnimState { action = AnimAction.Block }),
                (RigLook.ForEnemy(DefaultContent.Create().Get<EnemyDefinition>("enemy_warden")), 1.25f, Attack(AttackAnim.Overhead, ActionPhase.Startup, 1f)),
                (RigLook.ForEnemy(DefaultContent.Create().Get<EnemyDefinition>("enemy_castellan")), 1.6f, Attack(AttackAnim.SlashRight, ActionPhase.Startup, 1f)),
                (knight, 1f, new CharacterAnimState { action = AnimAction.Kneel, actionTime = 1f }),
            };

            float spacing = 2.2f;
            var all = new List<(CharacterVisual, StubAnimSource, CharacterAnimState)>();
            for (int i = 0; i < states.Count; i++)
            {
                var (v, s) = Dummy(knight, new Vector3((i - 4) * spacing, 0, 0));
                all.Add((v, s, states[i].Item2));
            }
            for (int i = 0; i < row2.Count; i++)
            {
                var (look, sc, st) = row2[i];
                var (v, s) = Dummy(look, new Vector3((i - 4) * spacing * 1.1f, 0, 5f), 180f, sc);
                all.Add((v, s, st));
            }
            foreach (var (v, s, st) in all) { s.state = st; for (int k = 0; k < 90; k++) v.Tick(1f / 60f); }
            yield return null;

            void Shot(string name, Vector3 pos, Vector3 look)
            {
                cam.transform.position = pos;
                cam.transform.LookAt(look);
                var rt = new RenderTexture(1920, 1080, 24);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); tex.Apply();
                cam.targetTexture = null; RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
                Object.Destroy(rt); Object.Destroy(tex);
            }
            // Postacie patrzą w −Z (yaw 180), kamera przed nimi.
            Shot("pose_row1_front.png", new Vector3(0, 1.6f, -14f), new Vector3(0, 1.0f, 0));
            Shot("pose_row1_side.png", new Vector3(-16f, 1.5f, -2f), new Vector3(0, 1.0f, 0));
            Shot("pose_close_slash.png", new Vector3(-8.2f, 1.8f, -3.6f), new Vector3(-6.2f, 1.1f, 0));
            Shot("pose_close_block.png", new Vector3(5.5f, 1.8f, -3.6f), new Vector3(6.6f, 1.1f, 0));
            for (int i = 0; i < states.Count; i++) all[i].Item1.gameObject.SetActive(false);
            Shot("pose_row2_front.png", new Vector3(0, 2.2f, -7f), new Vector3(0, 1.2f, 5f));
        }
    }
}
