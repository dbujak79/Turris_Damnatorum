using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Pomoc w celowaniu z Hell Unleashed: wybór celu (stożek, zasięg, remisy) i obrót ciosu; namierzanie ma pierwszeństwo.</summary>
    public class AimAssistPlayTests
    {
        readonly List<Object> cleanup = new List<Object>();

        class Stick : IPlayerDriver
        {
            public Vector3 move;
            public Vector3 MoveWorld => move;
            public bool BlockHeld => false;
            public bool SprintHeld => false;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        (PlayerCombat pc, PlayerController ctrl, Stick stick, DefaultContent.Bundle bundle) Arena()
        {
            var bundle = TestContent.Create();
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(60, 1, 60);
            cleanup.Add(floor);
            var camGo = new GameObject("TestCam");
            camGo.AddComponent<Camera>();
            var rig = camGo.AddComponent<CameraRig>();
            cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer();
            cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>();
            ctrl.cameraRig = rig;
            rig.target = p.transform;
            rig.player = ctrl;
            var stick = new Stick();
            ctrl.Driver = stick;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_knight"), difficulty = bundle.config.difficulties[0] };
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(RunFactory.Create(plan, bundle.config, 7), bundle.config);
            p.GetComponent<PlayerVisuals>().Refresh();
            p.transform.rotation = Quaternion.identity;
            return (pc, ctrl, stick, bundle);
        }

        EnemyBrain Dummy(Vector3 pos, PlayerCombat pc, BalanceConfig b)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            cleanup.Add(def);
            def.displayName = "Manekin"; def.maxHealth = 100000; def.maxPoise = 100000; def.scale = 1f; def.moveSpeed = 0f; def.strafeChance = 0f;
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.Setup(def, pc, 1f, null, false, b);
            cleanup.Add(e.gameObject);
            return e;
        }

        static float FacingDot(Transform t, EnemyBrain e)
        {
            Vector3 d = e.transform.position - t.position; d.y = 0f;
            return Vector3.Dot(t.forward, d.normalized);
        }

        [UnityTest]
        public IEnumerator Select_RespectsConeRangeAndPrefersNearest()
        {
            var (pc, _, _, bundle) = Arena();
            var b = bundle.config.balance;
            var ahead = Dummy(new Vector3(0, 0.05f, 4f), pc, b);
            var aheadFar = Dummy(new Vector3(0.5f, 0.05f, 6f), pc, b);
            var side = Dummy(new Vector3(3f, 0.05f, 0.5f), pc, b);      // ~80° od +Z, bliżej
            var behind = Dummy(new Vector3(0, 0.05f, -2f), pc, b);      // najbliższy, za plecami
            yield return null;
            var all = new List<EnemyBrain> { ahead, aheadFar, side, behind };
            Vector3 o = Vector3.zero;
            Assert.AreSame(ahead, AimAssist.Select(o, Vector3.forward, all, 8f, AimAssist.ExplicitConeDegrees), "w stożku 28° wygrywa najbliższy");
            Assert.AreSame(behind, AimAssist.Select(o, Vector3.zero, all, 8f, AimAssist.ExplicitConeDegrees), "bez kierunku – najbliższy w dowolną stronę");
            Assert.AreSame(behind, AimAssist.Select(o, Vector3.forward, all, 8f, 180f), "stożek 180° – najbliższy w dowolną stronę");
            Assert.IsNull(AimAssist.Select(o, Vector3.forward, all, 3f, AimAssist.ExplicitConeDegrees), "poza zasięgiem nic");
            Assert.IsNull(AimAssist.Select(o, Vector3.left, all, 8f, AimAssist.ExplicitConeDegrees), "w stożku nikogo – zostaje kierunek gracza");
        }

        [UnityTest]
        public IEnumerator Attack_WithoutInput_TurnsToNearestEnemy_EvenBehind()
        {
            var (pc, ctrl, _, bundle) = Arena();
            var e = Dummy(new Vector3(0, 0.05f, -2.2f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Assert.Less(FacingDot(pc.transform, e), -0.9f, "na starcie plecami do wroga");
            pc.Request(ActionType.LightAttack);
            yield return null;
            yield return null;
            Assert.AreSame(e, ctrl.AssistTarget);
            yield return new WaitForSeconds(pc.AttackWindup);
            Assert.Greater(FacingDot(pc.transform, e), 0.95f, "cios przyciągnięty do wroga za plecami");
        }

        [UnityTest]
        public IEnumerator Attack_WithAim_BendsOnlyInsideCone()
        {
            var (pc, ctrl, stick, bundle) = Arena();
            var b = bundle.config.balance;
            var wide = Dummy(new Vector3(2f, 0.05f, 1.2f), pc, b);   // ~59° od kierunku ruchu (+Z) – poza stożkiem
            yield return new WaitForSeconds(0.1f);
            stick.move = Vector3.forward;
            yield return null;
            pc.Request(ActionType.LightAttack);
            yield return null;
            yield return null;
            Assert.IsNull(ctrl.AssistTarget, "wróg 59° obok kierunku nie przyciąga ciosu");
            stick.move = Vector3.zero;
            yield return new WaitForSeconds(1.2f);

            ctrl.Teleport(Vector3.zero, Quaternion.identity);
            var near = Dummy(new Vector3(0.6f, 0.05f, 2.2f), pc, b); // ~15° – w stożku 28°, bliżej niż ten z boku
            yield return new WaitForSeconds(0.1f);
            stick.move = Vector3.forward;
            yield return null;
            pc.Request(ActionType.LightAttack);
            yield return null;
            yield return null;
            stick.move = Vector3.zero;
            Assert.AreSame(near, ctrl.AssistTarget, "wróg kilka stopni obok – pomoc zagina cios");
        }

        [UnityTest]
        public IEnumerator LockOn_Wins_OverAssist_AndAssistCanBeDisabled()
        {
            var (pc, ctrl, _, bundle) = Arena();
            var b = bundle.config.balance;
            var locked = Dummy(new Vector3(4f, 0.05f, 0f), pc, b);
            var closer = Dummy(new Vector3(0, 0.05f, -1.8f), pc, b);
            yield return new WaitForSeconds(0.1f);
            ctrl.LockOn(locked);
            pc.Request(ActionType.LightAttack);
            yield return null;
            yield return null;
            Assert.IsNull(ctrl.AssistTarget, "przy namierzaniu pomoc nie wybiera celu");
            yield return new WaitForSeconds(pc.AttackWindup);
            Assert.Greater(FacingDot(pc.transform, locked), 0.95f, "cios w namierzonego, nie w bliższego");
            yield return new WaitForSeconds(1.2f);

            ctrl.LockOn(null);
            ctrl.AimAssistEnabled = false;
            pc.transform.rotation = Quaternion.identity;
            yield return null;
            pc.Request(ActionType.LightAttack);
            yield return null;
            yield return null;
            Assert.IsNull(ctrl.AssistTarget, "wyłączona pomoc");
            yield return new WaitForSeconds(pc.AttackWindup);
            Assert.Less(FacingDot(pc.transform, closer), 0f, "bez pomocy postać nie obraca się do wroga za plecami");
        }
    }
}
