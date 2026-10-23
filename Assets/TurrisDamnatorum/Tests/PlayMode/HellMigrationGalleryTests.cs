#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    public class HellMigrationGalleryTests
    {
        class Pose : ICharacterAnimSource { public CharacterAnimState state; public CharacterAnimState GetAnimState() => state; }
        readonly List<Object> cleanup = new List<Object>();
        Camera camera;
        string folder;
        [SetUp] public void Setup()
        {
            folder = Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(folder)) Assert.Ignore("Galeria: ustaw TURRIS_SHOT_DIR.");
            Directory.CreateDirectory(folder); Time.timeScale = 1;
            var go = new GameObject("P1_Camera"); cleanup.Add(go); camera = go.AddComponent<Camera>();
            camera.enabled = false; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.045f, 0.045f, 0.055f); camera.fieldOfView = 35;
            var light = new GameObject("P1_Light"); cleanup.Add(light); var sun = light.AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.4f;
            light.transform.rotation = Quaternion.Euler(45, -35, 0);
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.5f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in cleanup) if (go != null) Object.Destroy(go);
            foreach (var pool in Object.FindObjectsByType<HellFxPool>()) Object.Destroy(pool.gameObject);
            yield return null;
            // OnDestroy modeli odczepia dogasające cząsteczki. Następna galeria musi mieć pustą scenę.
            foreach (var fx in Object.FindObjectsByType<HellParticles>()) Object.Destroy(fx.gameObject);
            foreach (var fx in Object.FindObjectsByType<FxDecal>()) Object.Destroy(fx.gameObject);
            foreach (var fx in FxFade.Active.ToArray()) if (fx != null) Object.Destroy(fx.gameObject);
            yield return null;
        }
        Texture2D Shot(Vector3 position, Vector3 target, int width = 400, int height = 400)
        {
            camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
            var rt = new RenderTexture(width, height, 24); camera.targetTexture = rt; camera.Render();
            var old = RenderTexture.active; RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, width, height), 0, 0); tex.Apply();
            RenderTexture.active = old; camera.targetTexture = null; rt.Release(); Object.Destroy(rt); return tex;
        }
        [UnityTest]
        public IEnumerator WeaponPosesGallery_GripBowAndSurfaceLayers()
        {
            var ids = AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/TurrisDamnatorum/Content/Items" });
            var items = ids.Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(i => i.id.StartsWith("hu_") && i.IsWeapon).OrderBy(i => i.id).ToArray();
            var warrior = AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/class_knight.asset");
            var archer = AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/class_hu_archer.asset");
            var mage = AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/class_mage.asset");
            for (int start = 0; start < items.Length; start += 6)
            {
                int rows = Mathf.Min(6, items.Length - start); var sheet = new Texture2D(1600, rows * 400, TextureFormat.RGB24, false);
                var labels = new List<string>();
                for (int row = 0; row < rows; row++)
                {
                    var item = items[start + row]; labels.Add(item.id);
                    var look = new RigLook { weapon = item.weaponModel, weaponItem = item, shield = ShieldModel.None };
                    if (item.id == "hu_dagger") look.offhandItem = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/TurrisDamnatorum/Content/Items/hu_poniard.asset");
                    var cls = item.weaponModel == WeaponModel.Bow ? archer : item.weaponModel == WeaponModel.Staff ? mage : warrior;
                    var go = new GameObject(item.id); cleanup.Add(go); go.transform.rotation = Quaternion.Euler(0, 180, 0);
                    var visual = go.AddComponent<CharacterVisual>(); visual.BuildModel(cls.Visual, 1, look); var pose = new Pose(); visual.Init(pose);
                    for (int column = 0; column < 4; column++)
                    {
                        pose.state = column == 0 ? default : new CharacterAnimState { action = AnimAction.Attack, attack = AnimResolve.ForAttack(column == 3 ? item.weapon.heavy : item.weapon.light, 0, item.weaponModel),
                            phase = column == 1 ? ActionPhase.Startup : ActionPhase.Active, phaseProgress = column == 1 ? 0.98f : 0.5f, offhandAttack = column == 2 && look.offhandItem != null };
                        for (int i = 0; i < 30; i++) visual.Tick(1f / 60);
                        if (AnimResolve.GripFor(item.weaponModel) == GripStyle.TwoHand) Assert.Less(visual.GripError, 0.08f, item.id + ": chwyt, kolumna " + column);
                        if (item.weaponModel == WeaponModel.Bow && column == 1) Assert.Greater(visual.BowDraw, 0.95f, "cięciwa");
                        var image = Shot(new Vector3(2.2f, 1.6f, -4.2f), Vector3.up * 0.95f);
                        sheet.SetPixels(column * 400, (rows - row - 1) * 400, 400, 400, image.GetPixels()); Object.Destroy(image);
                    }
                    Object.Destroy(go); yield return null;
                }
                sheet.Apply(); File.WriteAllBytes(Path.Combine(folder, $"p1_weapons_{start / 6}.png"), sheet.EncodeToPNG()); Object.Destroy(sheet);
                File.WriteAllLines(Path.Combine(folder, $"p1_weapons_{start / 6}.txt"), new[] { "Kolumny: spoczynek, zamach 98%, trafienie (lewa ręka dla sztyletów), ciężki cios." }.Concat(labels));
            }
        }
        [UnityTest]
        public IEnumerator DemonGallery_AllSourceModels()
        {
            var rows = HellEnemyCatalog.Rows;
            for (int start = 0; start < rows.Length; start += 8)
            {
                var sheet = new Texture2D(1600, 800, TextureFormat.RGB24, false); var labels = new List<string>();
                for (int i = start; i < Mathf.Min(start + 8, rows.Length); i++)
                {
                    var row = rows[i]; labels.Add(row.targetId);
                    var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>($"Assets/TurrisDamnatorum/Content/Enemies/{row.targetId}.asset");
                    var visual = WorldBuilder.CreateEnemyVisual(def, Vector3.zero, null); cleanup.Add(visual.gameObject); visual.transform.rotation = Quaternion.Euler(0, 180, 0);
                    var source = new Pose(); visual.Init(source); for (int k = 0; k < 30; k++) visual.Tick(1f / 60);
                    var image = Shot(new Vector3(row.height * 1.2f, row.height * 0.8f, -row.height * 2.5f), Vector3.up * row.height * 0.48f);
                    int cell = i - start; sheet.SetPixels((cell % 4) * 400, (1 - cell / 4) * 400, 400, 400, image.GetPixels()); Object.Destroy(image); Object.Destroy(visual.gameObject); yield return null;
                }
                sheet.Apply(); File.WriteAllBytes(Path.Combine(folder, $"p1_demons_{start / 8}.png"), sheet.EncodeToPNG()); Object.Destroy(sheet);
                File.WriteAllLines(Path.Combine(folder, $"p1_demons_{start / 8}.txt"), labels);
            }
        }
        [UnityTest]
        public IEnumerator ThirtyDemons_RenderAndMeasureCameraCost()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); cleanup.Add(floor); floor.transform.position = Vector3.down * 0.5f; floor.transform.localScale = new Vector3(45,1,45);
            var floorMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.12f, 0.11f, 0.1f) }; cleanup.Add(floorMaterial); floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
            var names = new[] { "hound", "brute", "legionary", "frostcaster", "cultist", "imp" };
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/TurrisDamnatorum/Content/GameConfig.asset");
            var playerObject = new GameObject("P0_Target"); cleanup.Add(playerObject); var playerCollider = playerObject.AddComponent<CharacterController>(); playerCollider.height = 1.8f; playerCollider.center = Vector3.up * 0.9f;
            var player = playerObject.AddComponent<PlayerCombat>(); player.Init(RunFactory.Create(new LoadoutPlan { classDef = config.classes[0], difficulty = config.difficulties[0] }, config, 7), config); player.Health.SetMax(1000000, true);
            for (int i = 0; i < 30; i++)
            {
                var row = HellEnemyCatalog.Rows.First(r => r.model == names[i % names.Length]);
                var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>($"Assets/TurrisDamnatorum/Content/Enemies/{row.targetId}.asset");
                var enemy = WorldBuilder.CreateEnemy(def, new Vector3((i % 6 - 2.5f) * 2.5f, 0, 3 + i / 6 * 2.6f), null); enemy.Setup(def, player, 1, null, false, config.balance); cleanup.Add(enemy.gameObject);
            }
            camera.transform.position = new Vector3(0, 9, -10); camera.transform.LookAt(new Vector3(0, 1, 5)); camera.enabled = true;
            var warmup = System.Diagnostics.Stopwatch.StartNew(); while (warmup.Elapsed.TotalSeconds < 2) yield return null;
            var target = new RenderTexture(1600, 900, 24); camera.targetTexture = target;
            var frames = new List<float>(); var renders = new List<float>();
            for (int i = 0; i < 120; i++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew(); camera.Render(); renders.Add((float)watch.Elapsed.TotalMilliseconds);
                yield return null; frames.Add(Time.unscaledDeltaTime * 1000);
            }
            camera.targetTexture = null; target.Release(); Object.Destroy(target); renders.Sort();
            frames.Sort();
            var image = Shot(camera.transform.position, new Vector3(0, 1, 5), 1600, 900); File.WriteAllBytes(Path.Combine(folder, "p1_30_demons.png"), image.EncodeToPNG()); Object.Destroy(image);
            var systems = Object.FindObjectsByType<ParticleSystem>();
            File.WriteAllText(Path.Combine(folder, "p1_metrics.txt"), $"Unity {Application.unityVersion}; {SystemInfo.graphicsDeviceName}; {SystemInfo.graphicsDeviceType}; edytor gfx, 30 demonów z AI, fizyką, animacją i VFX; kamera 1600×900.\n120 klatek po 2 s rozgrzania: mediana {frames[60]:0.00} ms; p95 {frames[114]:0.00} ms.\nWywołanie Camera.Render (CPU): mediana {renders[60]:0.00} ms; p95 {renders[114]:0.00} ms.\nParticleSystem: {systems.Length}; cząsteczki: {systems.Sum(p => p.particleCount)}; pamięć Unity: {Profiler.GetTotalAllocatedMemoryLong()/1048576} MiB.\nPomiar edytora z ręcznym renderem każdej próbki, nie benchmark standalone ani osobny czas GPU.\n");
        }
        [UnityTest]
        public IEnumerator CocoonGallery_RootShellOnImportedHero()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/TurrisDamnatorum/Content/GameConfig.asset");
            var cls = config.classes[0];
            var go = new GameObject("P1_RootedHero"); cleanup.Add(go);
            var player = go.AddComponent<PlayerCombat>(); player.enabled = false;
            player.Init(RunFactory.Create(new LoadoutPlan { classDef = cls, difficulty = config.difficulties[0] }, config, 7), config);
            var visual = go.AddComponent<CharacterVisual>(); visual.BuildModel(cls.Visual, 1, RigLook.ForPlayer(player.Run, config)); visual.Init(player);
            go.transform.rotation = Quaternion.Euler(0, 180, 0); visual.Tick(1f / 60);
            var sheet = new Texture2D(1200, 600, TextureFormat.RGB24, false);
            for (int i = 0; i < 2; i++)
            {
                if (i == 1) { player.Status.TryRoot(1.2f); HellRootShell.Show(player); }
                yield return null;
                var image = Shot(new Vector3(2.4f, 1.7f, -4), Vector3.up, 600, 600);
                sheet.SetPixels(i * 600, 0, 600, 600, image.GetPixels()); Object.Destroy(image);
            }
            sheet.Apply(); File.WriteAllBytes(Path.Combine(folder, "p1_cocoon.png"), sheet.EncodeToPNG()); Object.Destroy(sheet);
        }
        [UnityTest]
        public IEnumerator SurfaceGallery_ShadowWetEmberEliteAndHitFlash()
        {
            var row = HellEnemyCatalog.Rows.First(r => r.model == "brute");
            var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>($"Assets/TurrisDamnatorum/Content/Enemies/{row.targetId}.asset");
            var visual = WorldBuilder.CreateEnemyVisual(def, Vector3.zero, null); cleanup.Add(visual.gameObject); visual.transform.rotation = Quaternion.Euler(0, 180, 0); visual.Init(new IdleAnimSource());
            var layer = new HellBodySurface(visual.Model); var sheet = new Texture2D(2000, 400, TextureFormat.RGB24, false);
            for (int i = 0; i < 5; i++)
            {
                visual.Tick(1f / 60); layer.Set(i == 1 ? 1 : 0, i == 2 ? 1 : 0, i == 3 ? 1 : 0, i == 4 ? 1 : 0);
                if (i == 4) visual.Impact(Vector3.forward, 0.4f, 0.7f);
                Assert.IsTrue(visual.Model.GetComponentInChildren<Renderer>().sharedMaterial.shader.isSupported);
                var image = Shot(new Vector3(2.8f, 1.8f, -6), Vector3.up * 1.2f); sheet.SetPixels(i * 400, 0, 400, 400, image.GetPixels()); Object.Destroy(image);
            }
            sheet.Apply(); File.WriteAllBytes(Path.Combine(folder, "p1_surfaces.png"), sheet.EncodeToPNG()); Object.Destroy(sheet);
            yield return null;
        }
    }
}
#endif
