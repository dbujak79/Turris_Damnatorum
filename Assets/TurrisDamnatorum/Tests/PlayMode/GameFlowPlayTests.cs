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
            bundle = TestContent.Create();
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
        public IEnumerator FullRun_AllFloors_Victory_AshPersistsAfterRestart()
        {
            var root = CreateGame(out var bundle);
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_knight"), difficulty = bundle.config.difficulties[0] };
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
            yield return null;
            int last = bundle.config.tower.floors.Count - 1;

            for (int floor = 0; floor <= last; floor++)
            {
                Assert.AreEqual(GameScreen.Playing, root.Screen);
                Assert.AreEqual(floor, root.Run.floorIndex);
                // Wrogowie fali wychodzą z portali po kolei – liczą się ci na arenie i ci, którzy jeszcze nadchodzą.
                Assert.AreEqual(bundle.config.tower.floors[floor].enemies.Count, root.Enemies.Count + root.IncomingEnemies);
                yield return new WaitForSeconds(0.3f); // chwila prawdziwej symulacji
                root.DebugCompleteFloorNow();
                yield return null;
                if (floor == last) break;
                Assert.AreEqual(GameScreen.Intermission, root.Screen, "HU: po piętrze od razu sklep");
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
            Assert.AreEqual(last + 1, reloaded.Profile.bestFloor);
            Assert.Contains(0, reloaded.Profile.victoryTiers);
        }

        [UnityTest]
        public IEnumerator HigherDifficulty_GivesMoreAshForSameProgress()
        {
            var root = CreateGame(out var bundle);
            root.Meta.DebugAddAsh(500);
            root.Meta.RecordReachedFloor(14); // trudność 1: dotarcie na piętro 15 z 30
            Assert.IsTrue(root.Meta.Purchase(bundle.Get<UnlockDefinition>("unlock_diff1")));
            int startAsh = root.Meta.Profile.ash;

            var p0 = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_mage"), difficulty = bundle.config.difficulties[0] };
            root.StartRun(p0);
            yield return null;
            root.DebugCompleteFloorNow();
            int ash0 = root.Run.ashEarned;

            var p1 = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_mage"), difficulty = bundle.config.difficulties[1] };
            root.StartRun(p1);
            yield return null;
            root.DebugCompleteFloorNow();
            int ash1 = root.Run.ashEarned;
            Assert.Greater(ash1, ash0);
            Assert.AreEqual(startAsh + ash0 + ash1, root.Meta.Profile.ash);
        }

        [UnityTest]
        public IEnumerator Difficulty1_AddsElitesInDuels()
        {
            var root = CreateGame(out var bundle);
            root.Meta.DebugAddAsh(500);
            root.Meta.RecordReachedFloor(14); // trudność 1: dotarcie na piętro 15 z 30
            Assert.IsTrue(root.Meta.Purchase(bundle.Get<UnlockDefinition>("unlock_diff1")));
            // Wieża z fal HU nie ma pojedynków bez bossa – reguła dotyczy treści, która je ma (piętro ustawione w teście).
            var first = bundle.config.tower.floors[0];
            first.portals.enabled = false; first.isDuel = true; first.enemies.Clear(); first.enemies.Add(bundle.Get<EnemyDefinition>("enemy_ghoul"));
            root.StartRun(new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_mage"), difficulty = bundle.config.difficulties[1] });
            yield return null;
            float wait = 0f;
            while (root.Enemies.Count == 0 && wait < 5f) { wait += Time.deltaTime; yield return null; }
            Assert.IsTrue(root.Enemies[0].IsElite, "Trudność 1 dodaje elity w pojedynkach");
        }

        [UnityTest]
        public IEnumerator Death_LosesRunState_QuickRetryStartsFresh()
        {
            var root = CreateGame(out var bundle);
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_mage"), difficulty = bundle.config.difficulties[0] };
            root.StartRun(plan);
            yield return null;
            // Piętro I z portalami: pierwsza grupa wychodzi po chwili – jej zabicie daje dusze.
            yield return new WaitForSeconds(2.4f);
            Assert.Greater(root.Enemies.Count, 0, "z portalu nie wyszedł żaden demon");
            root.DebugCompleteFloorNow();
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
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
        public IEnumerator Impact_HitstopBothSides_KnockbackAndBodyReaction()
        {
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
            var director = new GameObject("Impact").AddComponent<ImpactDirector>();
            cleanup.Add(director.gameObject);
            var e = Dummy(new Vector3(0, 0.05f, 1.6f), pc, bundle.config.balance);
            yield return new WaitForSeconds(0.1f);
            Vector3 start = e.transform.position;
            var visual = e.GetComponent<CharacterVisual>();
            var anim = (ProceduralHumanoidAnimator)visual.Driver;
            // Krótki szczyt reakcji może cały wypaść między dwiema klatkami edytora (np. podczas kompilacji shadera).
            // Trafienie nadal pochodzi z normalnej walki; odpowiedź ciała próbkujemy stałym krokiem bez LateUpdate.
            visual.enabled = false;

            pc.Request(ActionType.LightAttack);
            float t = 0f;
            while (e.Health.Current >= e.Health.Max && t < 1.5f) { t += Time.deltaTime; yield return null; }
            Assert.Less(e.Health.Current, e.Health.Max, "Cios doszedł");
            Assert.IsTrue(e.InHitStop, "Trafiony wróg zastyga (hitstop)");
            Assert.IsTrue(pc.InHitStop, "Atakujący bronią też na chwilę zastyga");
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current, "Hitstop nie przerywa ataku");

            float maxReact = 0f;
            e.enabled = false;
            for (int frame = 0; frame < 60; frame++)
            {
                e.Step(1f / 120); visual.Tick(1f / 120);
                maxReact = Mathf.Max(maxReact, anim.ReactOffset.magnitude);
                yield return null;
            }
            Assert.IsFalse(e.InHitStop);
            Assert.Greater(maxReact, 0.2f, "Ciało wroga ustępuje pod ciosem");
            Assert.Greater(e.transform.position.z - start.z, 0.08f, "Wróg odrzucony od gracza");
            Assert.Greater(pc.GetComponent<CharacterVisual>().WeaponBlood, 0.05f, "Ostrze brudzi się krwią");
        }

        EnemyBrain Dummy(Vector3 pos, PlayerCombat pc, BalanceConfig b)
        {
            var d = ScriptableObject.CreateInstance<EnemyDefinition>();
            d.displayName = "Manekin"; d.maxHealth = 1000; d.maxPoise = 10000; d.scale = 1f; d.moveSpeed = 0f; d.strafeChance = 0f;
            cleanup.Add(d);
            return SpawnEnemy(d, pos, pc, b);
        }

        /// <summary>
        /// Blok a wróg z boku: bohater w bloku sam, szybko obraca się do zamachującego się wroga, zanim cios doleci,
        /// i cios jest zablokowany (wcześniej tarcza 140° zasłaniała tylko przód i cios z boku przechodził).
        /// </summary>
        [UnityTest]
        public IEnumerator Block_TurnsQuicklyToAttackerFromSide_AndBlocks()
        {
            var bundle = TestContent.Create();
            var b = bundle.config.balance;
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
            pc.transform.rotation = Quaternion.identity;
            var e = SpawnEnemy(bundle.Get<EnemyDefinition>("enemy_ghoul"), new Vector3(1.7f, 0.05f, 0f), pc, b); // 90° w prawo
            float hp = pc.Health.Current;
            float angleAtStrike = 180f;
            bool attacked = false;
            int blocked = 0;
            System.Action<Vector3, HitResult, bool> onHit = (p, r, toPlayer) => { if (toPlayer && r.outcome == HitOutcome.Blocked) blocked++; };
            CombatEvents.HitResolved += onHit;
            try
            {
                for (float t = 0f; t < 8f && blocked == 0; t += Time.deltaTime)
                {
                    pc.SetBlockHeld(true);
                    if (e.CurrentState == EnemyBrain.State.Attacking)
                    {
                        attacked = true;
                        if (e.AttackPhase == ActionPhase.Active)
                        {
                            Vector3 to = e.transform.position - pc.transform.position; to.y = 0f;
                            angleAtStrike = Mathf.Min(angleAtStrike, Vector3.Angle(pc.transform.forward, to));
                        }
                    }
                    yield return null;
                }
            }
            finally { CombatEvents.HitResolved -= onHit; }
            Assert.IsTrue(attacked, "Wróg zaatakował");
            Assert.Greater(blocked, 0, "Cios z boku zablokowany");
            Assert.Less(angleAtStrike, 30f, "W chwili ciosu bohater stoi już przodem do napastnika");
            Assert.AreEqual(hp, pc.Health.Current, 1f, "Tarcza zatrzymała obrażenia");
        }

        [UnityTest]
        public IEnumerator Enemy_TurnsSlower_DuringPlayerDodge_AndWhenPlayerIsBehind()
        {
            var bundle = TestContent.Create();
            var b = bundle.config.balance;
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_rogue");
            var e = Dummy(new Vector3(0, 0.05f, 2.5f), pc, b);
            yield return null;
            Vector3 front = pc.transform.position - e.transform.position;
            Assert.AreEqual(1f, e.TurnMultiplier(front), 0.001f, "Gracz z przodu, bez uniku – pełna szybkość obrotu");
            Assert.AreEqual(b.enemyTurnBehind, e.TurnMultiplier(-front), 0.001f, "Gracz za plecami – wolniej");

            pc.Request(ActionType.Dodge);
            yield return null;
            Assert.AreEqual(ActionType.Dodge, pc.Actions.Current);
            Assert.AreEqual(b.enemyTurnDuringDodge, e.TurnMultiplier(front), 0.001f, "W czasie uniku wróg gubi gracza");
            Assert.AreEqual(b.enemyTurnDuringDodge * b.enemyTurnBehind, e.TurnMultiplier(-front), 0.001f, "Unik za plecy – najwolniej");

            yield return new WaitForSeconds(2f);
            Assert.AreEqual(1f, e.TurnMultiplier(front), 0.001f, "Po chwili od uniku – znowu normalnie");
        }

        SpellInstance Equip(PlayerCombat pc, DefaultContent.Bundle bundle, string id, int slot)
        {
            TestWeapons.EnsureFor(pc.Run, bundle.Get<SpellDefinition>(id), w => bundle.Get<ItemDefinition>(w)); // skill wymaga rodzaju broni
            var s = pc.Run.LearnSpell(bundle.Get<SpellDefinition>(id));
            pc.Run.AssignSlot(s, slot);
            pc.RefreshBuild();
            pc.Mana.SetCurrent(pc.Mana.Max);
            return s;
        }

        [UnityTest]
        public IEnumerator Fireball_ExplodesOnImpact_AndBurns()
        {
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_mage");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_mage");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_mage");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_mage");
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
            // Wieża to fale HU (portale); fale po kolei zostają mechaniką treści – piętro z trzema falami ustawione w teście.
            var ghoul = bundle.Get<EnemyDefinition>("enemy_ghoul");
            var first = bundle.config.tower.floors[0];
            first.portals.enabled = false; first.isDuel = false;
            first.enemies.Clear(); first.enemies.Add(ghoul); first.enemies.Add(ghoul);
            first.extraWaves.Clear();
            for (int i = 0; i < 2; i++) { var w = new EnemyWave(); w.enemies.Add(ghoul); w.enemies.Add(ghoul); first.extraWaves.Add(w); }
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_knight"), difficulty = bundle.config.difficulties[0] };
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
            yield return null;
            var floor = root.CurrentFloor;
            Assert.GreaterOrEqual(floor.WaveCount, 3);
            for (int wave = 0; wave < floor.WaveCount; wave++)
            {
                Assert.AreEqual(wave, root.WaveIndex);
                Assert.AreEqual(GameScreen.Playing, root.Screen, $"Fala {wave + 1}: nadal walka");
                // Wrogowie fali wychodzą z portali po kolei (co StreamInterval s) – czekamy na całą falę i ją zabijamy.
                float w = 0f, nearest = float.MaxValue;
                while (root.IncomingEnemies > 0 && w < 15f)
                {
                    foreach (var pp in root.OpenPortals) nearest = Mathf.Min(nearest, Vector3.Distance(new Vector3(pp.x, 0, pp.z), new Vector3(root.Player.transform.position.x, 0, root.Player.transform.position.z)));
                    w += Time.deltaTime; yield return null;
                }
                var alive = root.Enemies.Where(e => e != null && !e.IsDead).ToList();
                Assert.Greater(alive.Count, 0, $"Fala {wave + 1} ma wrogów");
                if (nearest < float.MaxValue) Assert.Greater(nearest, 5f, "portale otwierają się z dala od bohatera");
                foreach (var e in alive) e.DebugKill();
                float t = 0;
                while (t < 4f && root.Screen == GameScreen.Playing && root.WaveIndex == wave) { t += Time.deltaTime; yield return null; }
            }
            Assert.AreEqual(GameScreen.Intermission, root.Screen, "Po ostatniej fali – sklep (HU, bez nagrody)");
        }

        [UnityTest]
        public IEnumerator Archer_ShootsArrowsAtPlayer_FromDistance()
        {
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_rogue");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_rogue");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_rogue");
            yield return null;
            var v = pc.GetComponent<CharacterVisual>();
            Assert.IsTrue(v.UsesModel, "Łotrzyk z modelem FBX");
            var right = Util.FindDeep(v.Model.transform, "WeaponSocket");
            var left = Util.FindDeep(v.Model.transform, "OffhandSocket");
            Assert.IsNotNull(right.GetComponentInChildren<MeshRenderer>(), "Nóż w prawej dłoni");
            Assert.IsNotNull(left.GetComponentInChildren<MeshRenderer>(), "Nóż w lewej dłoni");
            var handL = v.Model.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftHand);
            Assert.Less(Vector3.Distance(left.position, handL.position), 0.2f, "Drugi nóż trzymany lewą dłonią");
        }

        [UnityTest]
        public IEnumerator HeavyThrow_HitsOutAndBack_FistsMeanwhile()
        {
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_mage");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_mage");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_mage");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
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
            var bundle = TestContent.Create();
            var (pc, _) = CreateArenaWithPlayer(bundle, "legacy_knight");
            var boss = SpawnEnemy(bundle.Get<EnemyDefinition>("enemy_castellan"), new Vector3(0, 0.05f, 8f), pc, bundle.config.balance);
            yield return null;
            Assert.AreEqual(0, boss.PhaseIndex);
            boss.Health.Drain(boss.Health.Max * 0.55f);
            yield return null;
            Assert.AreEqual(1, boss.PhaseIndex);
        }
    }
}
