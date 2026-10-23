#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Demony z pierwszych trzech aren Hell Unleashed: modele i klipy, ataki specjalne, portale.</summary>
    public class DemonPlayTests
    {
        const string Enemies = "Assets/TurrisDamnatorum/Content/Enemies/";
        readonly List<Object> cleanup = new List<Object>();
        string profileFile;

        static readonly (string id, float height)[] Demons =
        {
            ("enemy_imp", 1.3f), ("enemy_imp_frost", 1.3f), ("enemy_imp_storm", 1.3f), ("enemy_imp_earth", 1.3f),
            ("enemy_hu_cultist", 1.85f), ("enemy_worm", 0.6f), ("enemy_minion", 1.0f), ("enemy_gatelord", 3.8f),
        };

        static EnemyDefinition Load(string id) => AssetDatabase.LoadAssetAtPath<EnemyDefinition>(Enemies + id + ".asset");

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            profileFile = $"turris_test_{System.Guid.NewGuid():N}.json";
            EnemyBrain.ResetLatchCount();
        }

        [TearDown]
        public void TearDown()
        {
            EnemyBrain.SummonRequested -= OnSummon;
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
            EnemyBrain.ResetLatchCount();
        }

        /// <summary>Obrys siatki w bieżącej pozie (wypieczonej – obrys skórowanej siatki bywa nieaktualny).</summary>
        static Bounds MeshBounds(GameObject go)
        {
            var b = new Bounds();
            bool first = true;
            var mesh = new Mesh();
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.BakeMesh(mesh, true);
                foreach (var v in mesh.vertices)
                {
                    var p = r.transform.TransformPoint(v);
                    if (first) { b = new Bounds(p, Vector3.zero); first = false; } else b.Encapsulate(p);
                }
            }
            Object.Destroy(mesh);
            return b;
        }

        [UnityTest]
        public IEnumerator AllDemons_ModelWithClips_HeightAsInHellUnleashed_FeetOnGround()
        {
            foreach (var (id, height) in Demons)
            {
                var def = Load(id);
                Assert.IsNotNull(def, "brak wroga " + id);
                Assert.IsNotNull(def.visual, id + ": brak modelu");
                Assert.IsTrue(def.visual.demon, id);
                Assert.IsNotNull(def.visual.idle, id + ": brak klipu spoczynku");
                Assert.IsNotNull(def.visual.walk, id + ": brak klipu chodu");
                Assert.IsNotNull(def.visual.Find(AnimAction.Attack, AttackAnim.Auto), id + ": brak klipu ataku");
                Assert.IsNotNull(def.visual.Find(AnimAction.Death, AttackAnim.Auto), id + ": brak klipu śmierci");

                var e = WorldBuilder.CreateEnemy(def, Vector3.zero, null);
                cleanup.Add(e.gameObject);
                var v = e.GetComponent<CharacterVisual>();
                var src = new StubAnimSource();
                v.Init(src);
                Assert.IsInstanceOf<DemonAnimationDriver>(v.Driver, id);
                for (int i = 0; i < 20; i++) v.Tick(1f / 60f);
                var b = MeshBounds(v.Model);
                Assert.That(b.size.y, Is.InRange(height * 0.7f, height * 1.3f), $"{id}: wysokość {b.size.y:0.00} m");
                if (id != "enemy_minion") Assert.That(b.min.y, Is.InRange(-0.15f * height, 0.2f * height), $"{id}: stopy {b.min.y:0.00}");
                Object.Destroy(e.gameObject);
                yield return null;
            }
        }

        /// <summary>
        /// Zamach jak w HU: klip dochodzi do szczytu i tam czeka, a w ostatnim ułamku zapowiedzi przyspiesza do klatki ciosu –
        /// w chwili fazy aktywnej klip stoi dokładnie na ciosie, niezależnie od długości zapowiedzi.
        /// </summary>
        [UnityTest]
        public IEnumerator Attack_ClipHeldAtTop_StrikesExactlyAtActivePhase()
        {
            var def = Load("enemy_imp");
            var e = WorldBuilder.CreateEnemy(def, Vector3.zero, null);
            cleanup.Add(e.gameObject);
            var v = e.GetComponent<CharacterVisual>();
            var src = new StubAnimSource();
            v.Init(src);
            var drv = (DemonAnimationDriver)v.Driver;
            var clip = def.visual.Find(AnimAction.Attack, AttackAnim.Auto);
            float top = clip.windupEnd * clip.clip.length, strike = clip.activeEnd * clip.clip.length;
            foreach (float windup in new[] { 0.3f, 1.2f })
            {
                src.state = new CharacterAnimState();
                for (int i = 0; i < 20; i++) v.Tick(1f / 60f);
                float at80 = -1f, at100 = -1f;
                int frames = Mathf.RoundToInt(windup * 60f);
                for (int i = 0; i <= frames; i++)
                {
                    src.state = new CharacterAnimState { action = AnimAction.Attack, attack = AttackAnim.Auto, phase = ActionPhase.Startup, phaseProgress = i / (float)frames, windupSeconds = windup };
                    v.Tick(1f / 60f);
                    if (i == Mathf.RoundToInt(frames * 0.5f)) at80 = drv.CurrentActionTime;
                    if (i == frames) at100 = drv.CurrentActionTime;
                }
                if (windup > 1f) Assert.AreEqual(top, at80, 0.03f, "w połowie długiej zapowiedzi klip czeka na szczycie zamachu");
                Assert.AreEqual(strike, at100, 0.02f, $"koniec zapowiedzi {windup} s = klatka ciosu");
                src.state = new CharacterAnimState { action = AnimAction.Attack, attack = AttackAnim.Auto, phase = ActionPhase.Active, phaseProgress = 0.5f, windupSeconds = windup };
                v.Tick(0.1f);
                Assert.AreEqual(strike + 0.1f, drv.CurrentActionTime, 0.02f, "po ciosie klip gra w czasie rzeczywistym");
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator Demon_Walks_AndRecoilsFromBlow()
        {
            var def = Load("enemy_hu_cultist");
            var e = WorldBuilder.CreateEnemy(def, Vector3.zero, null);
            cleanup.Add(e.gameObject);
            var v = e.GetComponent<CharacterVisual>();
            var src = new StubAnimSource();
            v.Init(src);
            var drv = (DemonAnimationDriver)v.Driver;
            for (int i = 0; i < 60; i++) { e.transform.position += Vector3.forward * def.moveSpeed / 60f; v.Tick(1f / 60f); yield return null; }
            Assert.Greater(drv.WalkWeight, 0.8f, "w ruchu gra klip chodu");
            for (int i = 0; i < 60; i++) { v.Tick(1f / 60f); yield return null; }
            Assert.Less(drv.WalkWeight, 0.1f, "w miejscu – spoczynek");
            v.Impact(Vector3.forward, 1f);
            float max = 0f;
            for (int i = 0; i < 30; i++) { v.Tick(1f / 60f); max = Mathf.Max(max, drv.TiltAngle); }
            Assert.Greater(max, 6f, "ciało odchyla się od ciosu");
            for (int i = 0; i < 90; i++) v.Tick(1f / 60f);
            Assert.Less(drv.TiltAngle, 1f, "i wraca");
        }

        /// <summary>Podgląd w bestiariuszu: animacja w tempie gry (było 2× – nietoperz machał skrzydłami za szybko).</summary>
        [UnityTest]
        public IEnumerator BestiaryPreview_AnimatesAtRealSpeed()
        {
            var stage = BestiaryStage.Create();
            cleanup.Add(stage.gameObject);
            stage.Preview(Load("enemy_minion"), true);
            Time.captureFramerate = 60;
            yield return null; yield return null;
            var drv = Object.FindObjectsByType<CharacterVisual>(FindObjectsSortMode.None).Select(v => v.Driver).OfType<DemonAnimationDriver>().First();
            var field = typeof(DemonAnimationDriver).GetField("idleTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            float t0 = (float)field.GetValue(drv);
            for (int i = 0; i < 12; i++) yield return null;
            float t1 = (float)field.GetValue(drv);
            Time.captureFramerate = 0;
            float len = Load("enemy_minion").visual.idle.length;
            float advanced = Mathf.Repeat(t1 - t0, len);
            Assert.AreEqual(12f / 60f, advanced, 0.03f, "klip spoczynku w tempie 1× (jak w Hell Unleashed)");
        }

        // ------------------------------------------------------------------ Walka

        const string ConfigPath = "Assets/TurrisDamnatorum/Content/GameConfig.asset";
        static GameConfig AssetConfig() => AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
        /// <summary>Wojownik z Hell Unleashed (model FBX) – nie proceduralny rycerz z treści w kodzie.</summary>
        static ClassDefinition Warrior(GameConfig cfg) => cfg.classes.First(c => c.id == "class_knight");

        (PlayerCombat pc, GameConfig cfg) Arena()
        {
            var cfg = AssetConfig();
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
            var plan = new LoadoutPlan { classDef = Warrior(cfg), difficulty = cfg.difficulties[0] };
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(RunFactory.Create(plan, cfg, 7), cfg);
            p.GetComponent<PlayerVisuals>().Refresh();
            Assert.IsTrue(p.GetComponent<CharacterVisual>().UsesModel, "bohater z modelem FBX");
            p.transform.position = new Vector3(0, 0.05f, 0);
            return (pc, cfg);
        }

        EnemyBrain Spawn(EnemyDefinition def, Vector3 pos, PlayerCombat pc, BalanceConfig b, bool elite = false)
        {
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, elite, b);
            cleanup.Add(e.gameObject);
            return e;
        }

        PlayerCombat summonTarget;
        BalanceConfig summonRules;
        readonly List<EnemyBrain> summoned = new List<EnemyBrain>();

        void OnSummon(EnemyBrain owner, EnemyDefinition def, Vector3 pos)
        {
            var m = Spawn(def, pos, summonTarget, summonRules);
            m.Owner = owner;
            summoned.Add(m);
        }

        /// <summary>
        /// Elitarny czerw skacze i wgryza się w bohatera: spowalnia go i co sekundę gryzie (krwawienie). Unik go strząsa.
        /// </summary>
        [UnityTest]
        public IEnumerator EliteWorm_LatchesOn_SlowsAndBites_DodgeShakesItOff()
        {
            var (pc, cfg) = Arena();
            var worm = Spawn(Load("enemy_worm"), new Vector3(0, 0.05f, 4f), pc, cfg.balance, elite: true);
            Assert.IsTrue(worm.IsElite);
            yield return null;
            int latch = worm.Def.attacks.FindIndex(a => a.special == EnemySpecial.Latch);
            worm.DebugStartAttack(latch);
            float t = 0f;
            while (worm.CurrentState != EnemyBrain.State.Latched && t < 2f) { t += Time.deltaTime; yield return null; }
            Assert.AreEqual(EnemyBrain.State.Latched, worm.CurrentState, "czerw wgryzł się w bohatera");
            Assert.AreEqual(1, EnemyBrain.LatchedCount);
            Assert.Greater(pc.Status.ExternalSlow, 0f, "uczepiony czerw spowalnia");
            float hp = pc.Health.Current;
            yield return new WaitForSeconds(1.3f);
            Assert.Less(pc.Health.Current, hp, "gryzie co sekundę");
            Assert.Less(Vector3.Distance(worm.transform.position, pc.transform.position), 1.2f, "jedzie na bohaterze");

            t = 0f;
            while (pc.Actions.Current != ActionType.Dodge && t < 1.5f) { pc.Request(ActionType.Dodge); t += Time.deltaTime; yield return null; }
            Assert.AreEqual(ActionType.Dodge, pc.Actions.Current, "bohater z czerwiem na plecach może zrobić unik");
            yield return null; yield return null;
            Assert.AreNotEqual(EnemyBrain.State.Latched, worm.CurrentState, "unik strząsa czerwia");
            Assert.AreEqual(0, EnemyBrain.LatchedCount);
            Assert.AreEqual(0f, pc.Status.ExternalSlow, 1e-4f);
        }

        /// <summary>Władca Bramy: każdy atak specjalny działa; od połowy życia przyzywa nietoperze, które giną razem z nim.</summary>
        [UnityTest]
        public IEnumerator Gatelord_Specials_Work_BatsDieWithHim()
        {
            var (pc, cfg) = Arena();
            pc.DebugInvulnerable = true;
            var b = cfg.balance;
            summonTarget = pc; summonRules = b;
            EnemyBrain.SummonRequested += OnSummon;
            var lord = Spawn(Load("enemy_gatelord"), new Vector3(0, 0.05f, 9f), pc, b);
            yield return null;

            // Szarża: zapowiedziana linia, przebiega wiele metrów i zostawia płonący ślad.
            int charge = lord.Def.attacks.FindIndex(a => a.special == EnemySpecial.Charge);
            pc.transform.position = new Vector3(0f, 0.05f, -9f); // daleko na linii – szarża (12,5 m) nie dobiega
            lord.transform.rotation = Quaternion.LookRotation(Vector3.back);
            Vector3 start = lord.transform.position;
            lord.DebugStartAttack(charge);
            int zonesBefore = DamageZone.Active.Count;
            yield return new WaitForSeconds(2.2f);
            Assert.Greater(Vector3.Distance(start, lord.transform.position), 5f, "szarża przebiegła kawał areny");
            Assert.Greater(DamageZone.Active.Count, zonesBefore, "płonący ślad po szarży");

            // Kręgi ognia wokół bohatera.
            int zones = lord.Def.attacks.FindIndex(a => a.special == EnemySpecial.FireZones);
            lord.DebugStartAttack(zones);
            float t = 0f;
            while (DelayedStrike.Active.Count < 4 && t < 2f) { t += Time.deltaTime; yield return null; }
            Assert.GreaterOrEqual(DelayedStrike.Active.Count, 4, "cztery zapowiedziane kręgi ognia");
            yield return new WaitForSeconds(1.5f);

            // Faza 2 (połowa życia): od razu wezwanie sfory nietoperzy.
            lord.Health.Drain(lord.Health.Max * 0.55f);
            t = 0f;
            while (summoned.Count == 0 && t < 3f) { t += Time.deltaTime; yield return null; }
            Assert.GreaterOrEqual(summoned.Count, 4, "wejście w fazę przyzywa nietoperze");
            Assert.IsTrue(summoned.All(m => m.Owner == lord && m.Def.isMinion));

            // Kule i skok (faza 2) – bez wyjątków, skok ląduje przy bohaterze.
            int orbs = lord.Def.attacks.FindIndex(a => a.special == EnemySpecial.Orbs);
            int before = Projectile.Active.Count;
            yield return new WaitForSeconds(1.2f);
            lord.DebugStartAttack(orbs);
            yield return new WaitForSeconds(0.7f);
            Assert.GreaterOrEqual(Projectile.Active.Count, before + 1, "piekielne kule lecą");
            int leap = lord.Def.attacks.FindIndex(a => a.special == EnemySpecial.Leap);
            yield return new WaitForSeconds(1f);
            lord.DebugStartAttack(leap);
            yield return new WaitForSeconds(1.2f);
            Assert.Less(Vector3.Distance(lord.transform.position, pc.transform.position), 4.5f, "skok na miejsce bohatera");

            lord.DebugKill();
            yield return null;
            Assert.IsTrue(summoned.All(m => m == null || m.IsDead), "nietoperze giną razem z Władcą Bramy");
            foreach (var z in DamageZone.Active.ToList()) Object.Destroy(z.gameObject);
            foreach (var p in Projectile.Active.ToList()) Object.Destroy(p.gameObject);
            foreach (var d in DelayedStrike.Active.ToList()) Object.Destroy(d.gameObject);
        }

        // ------------------------------------------------------------------ Portale

        GameRoot CreateGame(out GameConfig cfg)
        {
            // Konfiguracja z assetów: demony z modelami HU, bohater z modelem FBX. Kopia, by test mógł skrócić piętro.
            cfg = Object.Instantiate(AssetConfig());
            cfg.tower = Object.Instantiate(cfg.tower);
            cleanup.Add(cfg); cleanup.Add(cfg.tower);
            var go = new GameObject("TestGame");
            go.SetActive(false);
            var root = go.AddComponent<GameRoot>();
            root.config = cfg;
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

        /// <summary>
        /// Piętro I: portale otwierają się z dala od bohatera, demony wychodzą małymi grupami przez cały czas piętra,
        /// a piętro kończy się dopiero, gdy portale zgasną i wszystkie demony zginą.
        /// </summary>
        [UnityTest]
        public IEnumerator PortalFloor_SmallGroupsAwayFromHero_EndsWhenAllDead()
        {
            var root = CreateGame(out var cfg);
            var floor = cfg.tower.floors[0];
            Assert.IsTrue(floor.portals.enabled);
            floor.portals.duration = 5f;
            floor.portals.groupInterval = 1f;
            root.StartRun(new LoadoutPlan { classDef = Warrior(cfg), difficulty = cfg.difficulties[0] });
            root.Player.DebugInvulnerable = true;
            yield return null;
            Assert.AreEqual(0, root.Enemies.Count, "na starcie arena jest pusta");
            Assert.IsTrue(root.PortalsRunning);

            float t = 0f, minDist = float.MaxValue;
            int maxGroup = 0;
            while (root.OpenPortals.Count() == 0 && t < 2f) { t += Time.deltaTime; yield return null; }
            foreach (var p in root.OpenPortals) minDist = Mathf.Min(minDist, Vector3.Distance(Flat(p), Flat(root.Player.transform.position)));
            Assert.GreaterOrEqual(minDist, floor.portals.minDistance - 0.01f, "portal nie otwiera się tuż przy bohaterze");

            int seen = 0;
            while (root.PortalsRunning && t < 12f)
            {
                t += Time.deltaTime;
                int now = root.PortalSpawned;
                maxGroup = Mathf.Max(maxGroup, now - seen);
                seen = now;
                yield return null;
            }
            Assert.IsFalse(root.PortalsRunning, "portale zgasły po czasie piętra");
            Assert.That(root.PortalSpawned, Is.InRange(3, 12), "kilka grup w 5 s");
            Assert.LessOrEqual(maxGroup, 2, "małe grupy (1–2)");
            Assert.AreEqual(GameScreen.Playing, root.Screen, "żywe demony – piętro trwa");
            Assert.IsTrue(root.Enemies.All(e => e.Def.id.StartsWith("enemy_imp")), "na piętrze I tylko chochliki (w żywiołach)");

            root.DebugKillAllEnemies();
            yield return new WaitForSeconds(1.8f);
            Assert.AreEqual(GameScreen.Reward, root.Screen, "po zabiciu wszystkich piętro ukończone");
        }

        static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

        /// <summary>Galeria portalu: otwieranie, pełny portal, chochlik wychodzący z portalu, zamykanie; widok z boku i z kamery gry.</summary>
        [UnityTest]
        public IEnumerator PortalGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię portalu."); yield break; }
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(60, 1, 60);
            ground.GetComponent<Renderer>().material.color = new Color(0.22f, 0.2f, 0.2f);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 0.5f;
            lightGo.transform.rotation = Quaternion.Euler(50, 160, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.2f, 0.2f, 0.24f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.07f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 40;
            const int W = 480, H = 540;
            var times = new[] { 0.12f, 0.4f, 1.0f, 1.25f, 1.6f, 2.2f };
            var sheet = new Texture2D(W * times.Length, H * 2, TextureFormat.RGB24, false);
            var rt = new RenderTexture(W, H, 24);
            Time.captureFramerate = 60;
            var portal = DemonPortal.Open(Vector3.zero, Vector3.back, 2.4f, 2.0f, false);
            float t = 0f; int shot = 0; EnemyBrain imp = null;
            while (shot < times.Length && t < 4f)
            {
                yield return null;
                t += 1f / 60f;
                if (imp == null && t >= GameRoot.PortalDelay)
                {
                    imp = WorldBuilder.CreateEnemy(Load("enemy_imp"), Vector3.zero, null);
                    imp.transform.rotation = Quaternion.LookRotation(Vector3.back);
                    var v = imp.GetComponent<CharacterVisual>(); v.Init(new StubAnimSource());
                    cleanup.Add(imp.gameObject);
                    if (portal != null) portal.Invoke(nameof(DemonPortal.Close), 0.5f);
                }
                if (imp != null) imp.transform.position += Vector3.back * 1.1f / 0.45f / 60f * (t < GameRoot.PortalDelay + 0.45f ? 1f : 0f);
                if (t < times[shot]) continue;
                for (int k = 0; k < 2; k++)
                {
                    cam.transform.position = k == 0 ? new Vector3(1.6f, 1.4f, -5f) : new Vector3(0f, 9f, -7f);
                    cam.transform.LookAt(new Vector3(0f, k == 0 ? 1.2f : 0.5f, 0f));
                    cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                    sheet.ReadPixels(new Rect(0, 0, W, H), shot * W, (1 - k) * H);
                    RenderTexture.active = null; cam.targetTexture = null;
                }
                shot++;
            }
            Time.captureFramerate = 0;
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(dir, "portal.png"), sheet.EncodeToPNG());
            Object.Destroy(rt); Object.Destroy(sheet);
        }

        [UnityTest]
        public IEnumerator DemonGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię demonów."); yield break; }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(80, 1, 80);
            ground.GetComponent<Renderer>().material.color = new Color(0.35f, 0.34f, 0.33f);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.shadows = LightShadows.Soft; l.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(40, 160, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.48f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 35;

            // Rząd 1: spoczynek; rząd 2: szczyt zamachu; rząd 3: klatka ciosu (atak specjalny dla bossa).
            var states = new[]
            {
                new CharacterAnimState(),
                new CharacterAnimState { action = AnimAction.Attack, attack = AttackAnim.Auto, phase = ActionPhase.Startup, phaseProgress = 0.7f, windupSeconds = 0.8f },
                new CharacterAnimState { action = AnimAction.Attack, attack = AttackAnim.Special, phase = ActionPhase.Startup, phaseProgress = 1f, windupSeconds = 0.8f },
            };
            float x = -11f;
            for (int i = 0; i < Demons.Length; i++)
            {
                var def = Load(Demons[i].id);
                float w = Mathf.Max(1.4f, Demons[i].height * 0.9f);
                x += w * 0.5f;
                for (int r = 0; r < states.Length; r++)
                {
                    var e = WorldBuilder.CreateEnemy(def, new Vector3(x, 0, r * 5f), null);
                    e.transform.rotation = Quaternion.Euler(0, 180f, 0);
                    cleanup.Add(e.gameObject);
                    var v = e.GetComponent<CharacterVisual>();
                    var src = new StubAnimSource();
                    v.Init(src);
                    for (int k = 0; k < 40; k++)
                    {
                        var st = states[r];
                        st.phaseProgress = states[r].action == AnimAction.Attack ? Mathf.Min(1f, k / 39f) * states[r].phaseProgress : 0f;
                        src.state = st;
                        v.Tick(1f / 60f);
                    }
                }
                x += w * 0.5f + 0.4f;
            }
            yield return null;

            void Shot(string name, Vector3 pos, Vector3 look, int wpx = 2400, int hpx = 1000)
            {
                cam.transform.position = pos;
                cam.transform.LookAt(look);
                var rt = new RenderTexture(wpx, hpx, 24);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                var tex = new Texture2D(wpx, hpx, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, wpx, hpx), 0, 0); tex.Apply();
                cam.targetTexture = null; RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
                Object.Destroy(rt); Object.Destroy(tex);
            }
            float mid = (x - 11f) * 0.5f - 0.2f;
            Shot("demon_idle.png", new Vector3(mid, 1.6f, -16f), new Vector3(mid, 1.1f, 0));
            Shot("demon_idle_left.png", new Vector3(-6f, 1.0f, -6f), new Vector3(-6f, 0.8f, 0), 1800, 1000);
            Shot("demon_idle_right.png", new Vector3(mid + 5f, 1.6f, -8f), new Vector3(mid + 5f, 1.3f, 0), 1800, 1000);
            Shot("demon_top.png", new Vector3(mid, 1.6f, -11f), new Vector3(mid, 1.1f, 5f));
            Shot("demon_game.png", new Vector3(mid, 11f, -9f), new Vector3(mid, 0.5f, 5f));
        }
    }
}
#endif
