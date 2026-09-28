using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Testy integracyjne w działającym silniku (fizyka, AI, przepływ gry, zapis na dysk).</summary>
    public class GameFlowPlayTests
    {
        readonly List<Object> cleanup = new List<Object>();
        string profileFile;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            profileFile = $"turris_test_{System.Guid.NewGuid():N}.json";
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureFramerate = 0;
            Time.timeScale = 1f;
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
            Cursor.lockState = CursorLockMode.None;
        }

        GameRoot CreateGame(out DefaultContent.Bundle bundle)
        {
            bundle = DefaultContent.Create();
            var go = new GameObject("TestGame");
            go.SetActive(false);
            var root = go.AddComponent<GameRoot>();
            root.config = bundle.config;
            root.profileFileName = profileFile;
            go.SetActive(true);
            cleanup.Add(go);
            cleanup.Add(root.Player.gameObject);
            cleanup.Add(GameObject.Find("World"));
            cleanup.Add(root.CameraRig.gameObject);
            var sun = Object.FindAnyObjectByType<Light>();
            if (sun != null) cleanup.Add(sun.gameObject);
            return root;
        }

        [UnityTest]
        public IEnumerator FullRun_FiveFloors_Victory_AshPersistsAfterRestart()
        {
            var root = CreateGame(out var bundle);
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("class_knight"), difficulty = bundle.config.difficulties[0] };
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
            yield return null;

            for (int floor = 0; floor < 5; floor++)
            {
                Assert.AreEqual(GameScreen.Playing, root.Screen);
                Assert.AreEqual(floor, root.Run.floorIndex);
                Assert.AreEqual(bundle.config.tower.floors[floor].enemies.Count, root.Enemies.Count);
                yield return new WaitForSeconds(0.3f); // chwila prawdziwej symulacji
                root.DebugCompleteFloorNow();
                yield return null;
                if (floor == 4) break;
                Assert.AreEqual(GameScreen.Reward, root.Screen);
                Assert.AreEqual(3, root.Rewards.Count);
                root.ChooseReward(0);
                Assert.AreEqual(GameScreen.Intermission, root.Screen);
                root.NextFloor();
                yield return null;
            }

            Assert.AreEqual(GameScreen.Victory, root.Screen);
            int ash = root.Meta.Profile.ash;
            Assert.Greater(ash, 0);
            Assert.AreEqual(ash, root.Run.ashEarned);

            // "Ponowne uruchomienie gry": nowy serwis czyta ten sam plik z dysku.
            var reloaded = new MetaService(bundle.config, new FileProfileStorage(root.ProfilePath));
            Assert.AreEqual(ash, reloaded.Profile.ash);
            Assert.AreEqual(5, reloaded.Profile.bestFloor);
            Assert.Contains(0, reloaded.Profile.victoryTiers);
        }

        [UnityTest]
        public IEnumerator HigherDifficulty_GivesMoreAshForSameProgress()
        {
            var root = CreateGame(out var bundle);
            root.Meta.DebugAddAsh(500);
            root.Meta.RecordReachedFloor(4);
            Assert.IsTrue(root.Meta.Purchase(bundle.Get<UnlockDefinition>("unlock_diff1")));
            int startAsh = root.Meta.Profile.ash;

            var p0 = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("class_mage"), difficulty = bundle.config.difficulties[0] };
            root.StartRun(p0);
            yield return null;
            root.DebugCompleteFloorNow();
            int ash0 = root.Run.ashEarned;

            var p1 = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("class_mage"), difficulty = bundle.config.difficulties[1] };
            root.StartRun(p1);
            yield return null;
            Assert.IsTrue(root.Enemies[0].IsElite, "Trudność 1 dodaje elity w pojedynkach");
            root.DebugCompleteFloorNow();
            int ash1 = root.Run.ashEarned;
            Assert.Greater(ash1, ash0);
            Assert.AreEqual(startAsh + ash0 + ash1, root.Meta.Profile.ash);
        }

        [UnityTest]
        public IEnumerator Death_LosesRunState_QuickRetryStartsFresh()
        {
            var root = CreateGame(out var bundle);
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("class_mage"), difficulty = bundle.config.difficulties[0] };
            root.StartRun(plan);
            yield return null;
            root.DebugCompleteFloorNow();
            root.ChooseReward(0);
            root.NextFloor();
            yield return null;
            int ashKept = root.Meta.Profile.ash;
            Assert.Greater(root.Run.souls, 0);

            root.Player.DebugKill();
            yield return new WaitForSeconds(2.2f);
            Assert.AreEqual(GameScreen.Death, root.Screen);
            Assert.AreEqual(1, root.Meta.Profile.totalDeaths);

            root.RetrySameLoadout();
            yield return null;
            Assert.AreEqual(GameScreen.Playing, root.Screen);
            Assert.AreEqual(0, root.Run.floorIndex);
            Assert.AreEqual(0, root.Run.souls, "Dusze przepadają");
            Assert.AreEqual(0, root.Run.boons.Count + root.Run.inventory.Count, "Tymczasowe nagrody przepadają");
            Assert.AreEqual(ashKept, root.Meta.Profile.ash, "Popiół zostaje");
            Assert.IsFalse(root.Player.IsDead);
        }

        // ------------------------------------------------------------------ Walka w silniku

        (PlayerCombat pc, CameraRig rig) CreateArenaWithPlayer(DefaultContent.Bundle bundle, string classId)
        {
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
            ctrl.InputEnabled = false;
            rig.target = p.transform;
            rig.player = ctrl;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0] };
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(RunFactory.Create(plan, bundle.config, 7), bundle.config);
            p.GetComponent<PlayerVisuals>().Refresh();
            return (pc, rig);
        }

        EnemyBrain SpawnEnemy(EnemyDefinition def, Vector3 pos, PlayerCombat pc, BalanceConfig b)
        {
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(-pos.normalized);
            e.Setup(def, pc, 1f, null, false, b);
            cleanup.Add(e.gameObject);
            return e;
        }

        [UnityTest]
        public IEnumerator MeleeSwing_HitsEachTargetOnlyOnce()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var dummy = ScriptableObject.CreateInstance<EnemyDefinition>();
            dummy.displayName = "Manekin"; dummy.maxHealth = 1000; dummy.maxPoise = 10000; dummy.scale = 1f; dummy.moveSpeed = 0f; dummy.strafeChance = 0f;
            var e = SpawnEnemy(dummy, new Vector3(0, 0.05f, 1.6f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);

            float expected = pc.Build.WeaponDamage(pc.Build.weapon.light);
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(1000f - expected, e.Health.Current, 0.5f, "Jedno uderzenie = jedno trafienie, mimo wielu klatek fazy aktywnej");
        }

        [UnityTest]
        public IEnumerator EnemyAI_AttacksBlockingKnight_ShieldAbsorbsPhysical()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var ghoul = bundle.Get<EnemyDefinition>("enemy_ghoul");
            SpawnEnemy(ghoul, new Vector3(0, 0.05f, 3f), pc, bundle.config.balance);

            var outcomes = new List<HitOutcome>();
            System.Action<Vector3, HitResult, bool> onHit = (p, r, isPlayer) => { if (isPlayer) outcomes.Add(r.outcome); };
            CombatEvents.HitResolved += onHit;
            Time.timeScale = 2f;
            float t = 0;
            while (t < 8f && outcomes.Count < 3)
            {
                pc.SetBlockHeld(true);
                pc.Stamina.Restore(100f); // test samej ochrony, nie kondycji
                t += Time.deltaTime;
                yield return null;
            }
            CombatEvents.HitResolved -= onHit;
            Assert.Greater(outcomes.Count, 0, "AI zaatakowało w ciągu 8 s");
            Assert.Contains(HitOutcome.Blocked, outcomes);
        }

        [UnityTest]
        public IEnumerator TimedParry_AgainstRealEnemy_OpensRiposte()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var ghoul = bundle.Get<EnemyDefinition>("enemy_ghoul");
            ghoul.attacks[2].weight = 0; // bez skoku z dystansu – test ciosu wręcz
            ghoul.evadeChance = 0;
            ghoul.strafeChance = 0;
            var e = SpawnEnemy(ghoul, new Vector3(0, 0.05f, 2.0f), pc, bundle.config.balance);
            Time.captureFramerate = 60;

            bool parried = false;
            System.Action<Vector3, HitResult, bool> onHit = (p, r, isPlayer) => { if (isPlayer && r.outcome == HitOutcome.Parried) parried = true; };
            CombatEvents.HitResolved += onHit;

            var parry = pc.Build.parry;
            for (int attempt = 0; attempt < 6 && !parried; attempt++)
            {
                float wait = 0;
                while (!e.IsTelegraphing && wait < 5f) { wait += Time.deltaTime; yield return null; }
                if (!e.IsTelegraphing) break;
                float windup = e.CurrentAttack.attack.windup;
                // Uruchom parowanie tak, by aktywne okno objęło koniec zamachu.
                float delay = windup - parry.startup - parry.activeWindow * 0.5f;
                float waited = 0;
                while (waited < delay) { waited += Time.deltaTime; yield return null; }
                pc.Request(ActionType.Parry);
                float after = 0;
                while (after < 1.2f && !parried) { after += Time.deltaTime; yield return null; }
                pc.Stamina.Restore(100f);
                pc.Health.Restore(1000f);
            }
            CombatEvents.HitResolved -= onHit;
            Assert.IsTrue(parried, "Parowanie w oknie zatrzymało atak");
            Assert.IsTrue(e.CanBeRiposted, "Ghul (niska postawa) po parowaniu jest otwarty na ripostę");

            float hp = e.Health.Current;
            while (!pc.Actions.IsIdle) yield return null;
            pc.Request(ActionType.LightAttack);
            Assert.AreEqual(ActionType.Riposte, pc.Actions.Current);
            yield return new WaitForSeconds(0.6f);
            Assert.Less(e.Health.Current, hp - pc.Build.WeaponDamage(pc.Build.weapon.light) * 2f, "Riposta zadaje zwielokrotnione obrażenia");
        }

        [UnityTest]
        public IEnumerator Boss_ChangesPhase_BelowHalfHealth()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var boss = SpawnEnemy(bundle.Get<EnemyDefinition>("enemy_castellan"), new Vector3(0, 0.05f, 8f), pc, bundle.config.balance);
            yield return null;
            Assert.AreEqual(0, boss.PhaseIndex);
            boss.Health.Drain(boss.Health.Max * 0.55f);
            yield return null;
            Assert.AreEqual(1, boss.PhaseIndex);
        }
    }
}
