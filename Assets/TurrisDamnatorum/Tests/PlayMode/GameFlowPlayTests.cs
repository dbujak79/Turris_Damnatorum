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

        EnemyBrain Dummy(Vector3 pos, PlayerCombat pc, BalanceConfig b)
        {
            var d = ScriptableObject.CreateInstance<EnemyDefinition>();
            d.displayName = "Manekin"; d.maxHealth = 1000; d.maxPoise = 10000; d.scale = 1f; d.moveSpeed = 0f; d.strafeChance = 0f;
            cleanup.Add(d);
            return SpawnEnemy(d, pos, pc, b);
        }

        SpellInstance Equip(PlayerCombat pc, DefaultContent.Bundle bundle, string id, int slot)
        {
            var s = pc.Run.LearnSpell(bundle.Get<SpellDefinition>(id));
            pc.Run.AssignSlot(s, slot);
            pc.RefreshBuild();
            pc.Mana.SetCurrent(pc.Mana.Max);
            return s;
        }

        [UnityTest]
        public IEnumerator Fireball_ExplodesOnImpact_AndBurns()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_mage");
            var a = Dummy(new Vector3(0, 0.05f, 6f), pc, bundle.config.balance);
            var b = Dummy(new Vector3(1.4f, 0.05f, 6.6f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "spell_fireball", 0);
            pc.RequestSkill(0);
            float t = 0;
            while (t < 1.2f && a.Health.Current >= 1000f) { t += Time.deltaTime; yield return null; }
            yield return null;
            Assert.Less(a.Health.Current, 1000f, "Kula trafiła cel");
            Assert.Less(b.Health.Current, 1000f, "Wybuch dosięgnął sąsiada");
            Assert.Less(1000f - b.Health.Current, 1000f - a.Health.Current, "Sąsiad dostaje mniej niż trafiony bezpośrednio");
            Assert.IsTrue(a.Status.Burning, "Kula ognia podpala");
            float hp = a.Health.Current;
            yield return new WaitForSeconds(1.0f);
            Assert.Less(a.Health.Current, hp, "Podpalenie pali w czasie");
        }

        [UnityTest]
        public IEnumerator ChainLightning_JumpsBetweenEnemies_AndShocks()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_mage");
            var e1 = Dummy(new Vector3(0, 0.05f, 4f), pc, bundle.config.balance);
            var e2 = Dummy(new Vector3(3f, 0.05f, 6f), pc, bundle.config.balance);
            var e3 = Dummy(new Vector3(6f, 0.05f, 7f), pc, bundle.config.balance);
            var far = Dummy(new Vector3(-14f, 0.05f, -10f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "spell_chain", 0);
            pc.RequestSkill(0);
            yield return new WaitForSeconds(0.8f);
            foreach (var e in new[] { e1, e2, e3 })
            {
                Assert.Less(e.Health.Current, 1000f, "Błyskawica przeskoczyła na kolejnego wroga");
                Assert.IsTrue(e.Status.Shocked);
            }
            Assert.AreEqual(1000f, far.Health.Current, "Daleki wróg poza zasięgiem skoku");
            Assert.Greater(1000f - e1.Health.Current, 1000f - e3.Health.Current, "Każdy skok słabszy");
        }

        [UnityTest]
        public IEnumerator Rend_AppliesBleed_ThatKeepsDamaging()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var e = Dummy(new Vector3(0, 0.05f, 1.8f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "skill_rend", 2);
            pc.RequestSkill(2);
            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(2, e.Status.BleedStacks, "Krwawe cięcie – dwie warstwy");
            float hp = e.Health.Current;
            yield return new WaitForSeconds(1.2f);
            Assert.Less(e.Health.Current, hp, "Krwawienie zadaje obrażenia po ciosie");
        }

        [UnityTest]
        public IEnumerator FrostNovaAndCone_FreezeEnemy()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_mage");
            var e = Dummy(new Vector3(0, 0.05f, 2.2f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "spell_frostnova", 0);
            Equip(pc, bundle, "spell_frostcone", 1);
            pc.RequestSkill(0);
            yield return new WaitForSeconds(1.0f);
            Assert.AreEqual(2, e.Status.ChillStacks, "Mroźna fala – dwie warstwy chłodu");
            pc.RequestSkill(1);
            float t = 0;
            while (t < 1.5f && !e.Status.Frozen) { t += Time.deltaTime; yield return null; }
            Assert.IsTrue(e.Status.Frozen, "Trzecia warstwa zamraża");
            Assert.AreEqual(EnemyBrain.State.Staggered, e.CurrentState, "Zamrożenie przerywa działanie wroga");
        }

        [UnityTest]
        public IEnumerator LevelFeatures_FireballLeavesZone_ChainHitsSix()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_mage");
            var targets = new List<EnemyBrain>();
            // Rząd od najbliższego celu – każdy skok (do 6 m) sięga następnego.
            for (int i = 0; i < 6; i++) targets.Add(Dummy(new Vector3(i * 2.2f, 0.05f, 5f + (i % 2) * 1.2f), pc, bundle.config.balance));
            yield return new WaitForSeconds(0.1f);

            var chain = Equip(pc, bundle, "spell_chain", 0);
            chain.level = 4;
            pc.RefreshBuild();
            pc.RequestSkill(0);
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual(6, targets.Count(t => t.Health.Current < 1000f), "Łańcuch +4 skacze po 6 celach");

            foreach (var z in DamageZone.Active.ToArray()) Object.Destroy(z.gameObject);
            yield return null;
            var fireball = Equip(pc, bundle, "spell_fireball", 1);
            fireball.level = 4;
            pc.RefreshBuild();
            pc.RequestSkill(1);
            float t2 = 0;
            while (t2 < 1.5f && DamageZone.Active.Count == 0) { t2 += Time.deltaTime; yield return null; }
            Assert.Greater(DamageZone.Active.Count, 0, "Kula ognia +4 zostawia płonącą ziemię");
        }

        [UnityTest]
        public IEnumerator Floor_WavesComeOneAfterAnother_ThenReward()
        {
            var root = CreateGame(out var bundle);
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("class_knight"), difficulty = bundle.config.difficulties[0] };
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
            yield return null;
            var floor = root.CurrentFloor;
            Assert.GreaterOrEqual(floor.WaveCount, 3);
            for (int wave = 0; wave < floor.WaveCount; wave++)
            {
                Assert.AreEqual(wave, root.WaveIndex);
                Assert.AreEqual(GameScreen.Playing, root.Screen, $"Fala {wave + 1}: nadal walka");
                var alive = root.Enemies.Where(e => e != null && !e.IsDead).ToList();
                Assert.Greater(alive.Count, 0, $"Fala {wave + 1} ma wrogów");
                foreach (var e in alive) e.DebugKill();
                float t = 0;
                while (t < 4f && root.Screen == GameScreen.Playing && root.WaveIndex == wave) { t += Time.deltaTime; yield return null; }
            }
            Assert.AreEqual(GameScreen.Reward, root.Screen, "Po ostatniej fali – nagroda");
        }

        [UnityTest]
        public IEnumerator Archer_ShootsArrowsAtPlayer_FromDistance()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var archer = SpawnEnemy(bundle.Get<EnemyDefinition>("enemy_archer"), new Vector3(0, 0.05f, 11f), pc, bundle.config.balance);
            float hp = pc.Health.Current;
            float t = 0;
            bool shot = false;
            while (t < 8f && pc.Health.Current >= hp) { shot |= Projectile.Active.Count > 0; t += Time.deltaTime; yield return null; }
            Assert.IsTrue(shot, "Łucznik strzelił");
            Assert.Less(pc.Health.Current, hp, "Strzała trafiła bohatera");
            Assert.Greater(Vector3.Distance(archer.transform.position, pc.transform.position), 4f, "Strzela z dystansu");
            Assert.IsNotNull(archer.GetComponent<CharacterVisual>().Rig.transform.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name == "Quiver"), "Kołczan na plecach");
        }

        [UnityTest]
        public IEnumerator FireArrow_Explodes_AndBurns()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_rogue");
            var bow = new ItemInstance(bundle.Get<ItemDefinition>("weapon_bow"));
            pc.Run.inventory.Add(bow);
            pc.Run.EquipFromInventory(bow, EquipSlot.MainHand);
            var e = Dummy(new Vector3(0, 0.05f, 8f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "skill_firearrow", 0);
            pc.RequestSkill(0);
            float t = 0;
            while (t < 2f && e.Health.Current >= 1000f) { t += Time.deltaTime; yield return null; }
            yield return null;
            Assert.Less(e.Health.Current, 1000f, "Ognista strzała trafiła");
            Assert.IsTrue(e.Status.Burning, "…i podpaliła");
        }

        [UnityTest]
        public IEnumerator Bow_ArrowHitsDistantEnemy()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_rogue");
            var bow = new ItemInstance(bundle.Get<ItemDefinition>("weapon_bow"));
            pc.Run.inventory.Add(bow);
            pc.Run.EquipFromInventory(bow, EquipSlot.MainHand);
            pc.RefreshBuild();
            var e = Dummy(new Vector3(0, 0.05f, 9f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.2f);
            float expected = pc.Build.WeaponDamage(pc.Build.weapon.light);
            pc.Request(ActionType.LightAttack);
            float t = 0;
            bool sawArrow = false;
            while (t < 1.5f && e.Health.Current >= 1000f) { sawArrow |= Projectile.Active.Count > 0; t += Time.deltaTime; yield return null; }
            Assert.IsTrue(sawArrow, "Wystrzelono strzałę");
            Assert.AreEqual(1000f - expected, e.Health.Current, 0.5f, "Strzała trafia cel 9 m dalej za obrażenia z łuku");
        }

        [UnityTest]
        public IEnumerator Rogue_HoldsKnifeInEachHand()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_rogue");
            yield return null;
            var rig = pc.GetComponent<CharacterVisual>().Rig;
            Assert.IsNotNull(rig.WeaponSocket.GetComponentInChildren<MeshRenderer>(), "Nóż w prawej dłoni");
            Assert.IsNotNull(rig.OffhandSocket.GetComponentInChildren<MeshRenderer>(), "Nóż w lewej dłoni");
            Assert.Less(Vector3.Distance(rig.OffhandSocket.position, rig[Bone.HandL].position), 0.2f, "Drugi nóż trzymany lewą dłonią");
        }

        [UnityTest]
        public IEnumerator HeavyThrow_HitsOutAndBack_FistsMeanwhile()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var e = Dummy(new Vector3(0, 0.05f, 5f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "skill_throw", 2);
            float oneHit = pc.Build.WeaponDamage(pc.Build.weapon.light) * pc.Skill(2).power;
            pc.RequestSkill(2);
            yield return new WaitForSeconds(0.45f);
            Assert.IsTrue(pc.WeaponThrown, "Broń w locie");
            Assert.AreEqual(1, ThrownWeapon.Active.Count);
            Assert.IsNotNull(ThrownWeapon.Active[0].GetComponent<TrailRenderer>(), "Smuga za lecącą bronią");

            // W tym czasie cios pięścią (krótszy zamach niż miecz) i brak bloku bronią… ale rycerz ma tarczę.
            pc.Actions.Reset();
            pc.Request(ActionType.LightAttack);
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current);
            Assert.Less(pc.AttackWindup, pc.ScaleStartup(pc.Build.weapon.light.windup) / pc.Build.attackSpeed + 1e-4f, "Pięści zamiast broni");

            float t = 0;
            while (pc.WeaponThrown && t < 4f) { t += Time.deltaTime; yield return null; }
            Assert.IsFalse(pc.WeaponThrown, "Broń wróciła do dłoni");
            Assert.AreEqual(1000f - 2f * oneHit, e.Health.Current, oneHit * 0.05f + 1f, "Trafienie w locie tam i z powrotem");
        }

        [UnityTest]
        public IEnumerator Shatter_LightningOnFrozenEnemy_ExplodesOnNeighbour()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_mage");
            var frozen = Dummy(new Vector3(0, 0.05f, 5f), pc, bundle.config.balance);
            var neighbour = Dummy(new Vector3(1.6f, 0.05f, 5.5f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            frozen.ApplyStatus(StatusKind.Frozen, 0, 0);
            Assert.IsTrue(frozen.Status.Frozen);
            Equip(pc, bundle, "spell_spark", 0);
            pc.RequestSkill(0);
            float t = 0;
            while (t < 1.2f && frozen.Status.Frozen) { t += Time.deltaTime; yield return null; }
            yield return null;
            Assert.IsFalse(frozen.Status.Frozen, "Iskra roztrzaskała lód");
            Assert.Less(neighbour.Health.Current, 1000f, "Wybuch lodu zranił sąsiada");
        }

        [UnityTest]
        public IEnumerator Counter_StopsEnemyBlow_AndStrikesBack()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var attacker = Dummy(new Vector3(0, 0.05f, 1.8f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "skill_counter", 2);
            pc.RequestSkill(2);
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(ActionPhase.Active, pc.Actions.Phase, "Postawa kontry");
            float hp = pc.Health.Current;
            var blow = new HitData { physical = 60, poiseDamage = 20, guardLoad = 20, blockable = true, parryable = true, dodgeable = true,
                sourcePosition = attacker.transform.position, attacker = attacker, mods = StatusModifiers.None };
            var r = HitQuery.Apply(attacker, pc, blow, true);
            Assert.AreEqual(HitOutcome.Parried, r.outcome);
            Assert.AreEqual(hp, pc.Health.Current, "Cios zatrzymany");
            Assert.Less(attacker.Health.Current, 1000f, "Kontra oddaje cios");
        }

        [UnityTest]
        public IEnumerator Meteor_StrikesAfterDelay_AndLeavesBurningGround()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_mage");
            var e = Dummy(new Vector3(0, 0.05f, 7.2f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "spell_meteor", 0);
            pc.RequestSkill(0);
            yield return new WaitForSeconds(1.0f);
            Assert.AreEqual(1000f, e.Health.Current, "Przed upływem zapowiedzi – bez obrażeń");
            yield return new WaitForSeconds(0.9f);
            Assert.Less(e.Health.Current, 1000f - 50f, "Meteor uderzył");
            Assert.IsTrue(e.Status.Burning);
            Assert.Greater(DamageZone.Active.Count, 0, "Płonąca ziemia po meteorze");
        }

        [UnityTest]
        public IEnumerator Storm_HitsEnemiesAround_Chains_PullEnemyIn()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_mage");
            var near = Dummy(new Vector3(3f, 0.05f, 3f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Equip(pc, bundle, "spell_storm", 0);
            pc.RequestSkill(0);
            yield return new WaitForSeconds(2.5f);
            Assert.Less(near.Health.Current, 1000f, "Piorun trafił wroga w zasięgu");
            Assert.IsTrue(near.Status.Shocked);

            Equip(pc, bundle, "spell_chains", 1);
            float before = Vector3.Distance(near.transform.position, pc.transform.position);
            pc.RequestSkill(1);
            yield return new WaitForSeconds(1.2f);
            Assert.Less(Vector3.Distance(near.transform.position, pc.transform.position), before - 1f, "Łańcuchy przyciągnęły wroga");
        }

        [UnityTest]
        public IEnumerator Techniques_ChargeMovesAndHits_CleaveScalesWithWeapon()
        {
            var bundle = DefaultContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "class_knight");
            var dummy = ScriptableObject.CreateInstance<EnemyDefinition>();
            dummy.displayName = "Manekin"; dummy.maxHealth = 1000; dummy.maxPoise = 10000; dummy.scale = 1f; dummy.moveSpeed = 0f; dummy.strafeChance = 0f;
            var e = SpawnEnemy(dummy, new Vector3(0, 0.05f, 3.6f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);

            // Szarża: zryw naprzód, trafia wroga na drodze.
            var charge = pc.Run.LearnSpell(bundle.Get<SpellDefinition>("skill_charge"));
            pc.Run.AssignSlot(charge, 2);
            pc.RefreshBuild();
            Vector3 start = pc.transform.position;
            float chargeDmg = pc.Build.WeaponDamage(pc.Build.weapon.light) * pc.Skill(2).power;
            pc.RequestSkill(2);
            yield return new WaitForSeconds(1.0f);
            Assert.Greater(Vector3.Distance(start, pc.transform.position), 1.5f, "Szarża przesuwa bohatera");
            Assert.AreEqual(1000f - chargeDmg, e.Health.Current, 0.5f, "Szarża trafia wroga na drodze dokładnie raz");

            // Rozpłatanie: jedno trafienie = lekki atak broni × mnożnik techniki.
            float hp = e.Health.Current;
            float cleaveDmg = pc.Build.WeaponDamage(pc.Build.weapon.light) * pc.Skill(0).power;
            pc.RequestSkill(0);
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current);
            yield return new WaitForSeconds(1.0f);
            Assert.AreEqual(hp - cleaveDmg, e.Health.Current, 0.5f);
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
                float delay = windup - pc.ScaleStartup(parry.startup) - parry.activeWindow * 0.5f;
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
