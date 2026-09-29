using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Areny i efekty: budowa bez błędów, rozsądna liczba rendererów, galeria zrzutów (opcjonalnie).</summary>
    public class ArenaFxPlayTests
    {
        readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        [UnityTest]
        public IEnumerator Arenas_BuildWithSimpleCollidersAndBakedVisuals()
        {
            var bundle = DefaultContent.Create();
            foreach (var id in new[] { "arena_courtyard", "arena_crypt", "arena_summit" })
            {
                var def = bundle.Get<ArenaDefinition>(id);
                var arena = ArenaBuilder.Build(def, null);
                cleanup.Add(arena);
                var renderers = arena.GetComponentsInChildren<MeshRenderer>();
                var colliders = arena.GetComponentsInChildren<Collider>();
                int verts = 0;
                foreach (var r in renderers) { var mf = r.GetComponent<MeshFilter>(); if (mf != null && mf.sharedMesh != null) verts += mf.sharedMesh.vertexCount; }
                var fx = arena.GetComponentsInChildren<ParticleSystem>();
                var lights = arena.GetComponentsInChildren<Light>();
                Debug.Log($"ARENA {id}: renderery {renderers.Length}, wierzchołki {verts}, kolizje {colliders.Length}, systemy cząsteczek {fx.Length}, światła {lights.Length}");
                Assert.Greater(verts, 50000, "Arena jest szczegółowa");
                Assert.Less(renderers.Length, 80, "Części scalone w niewiele rendererów");
                foreach (var c in colliders) Assert.IsNull(c.GetComponent<Renderer>(), "Kolizje są niewidoczne i proste");
                yield return null;
                Object.Destroy(arena);
            }
        }

        [UnityTest]
        public IEnumerator Effects_SpawnAndCleanUp()
        {
            FxLibrary.Shockwave(Vector3.zero, Color.cyan, 3f);
            FxLibrary.Parry(Vector3.up);
            FxLibrary.Sparks(Vector3.up, Vector3.forward, Color.yellow);
            FxLibrary.Blood(Vector3.up, Vector3.forward);
            FxLibrary.DeathAsh(Vector3.zero, 1f);
            var target = new GameObject("T");
            cleanup.Add(target);
            FxLibrary.Heal(target.transform, Color.yellow, 0.5f);
            var hit = new HitData { magic = 10, blockable = true, dodgeable = true };
            Projectile.Spawn(new Vector3(0, 1, 0), Vector3.forward, 10f, hit, Faction.Enemy, null, Color.magenta, 0.25f, 0.3f);
            yield return null;
            Assert.Greater(Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude).Length, 5);
            yield return new WaitForSeconds(3.5f);
            // Jednorazowe efekty same się usuwają (stopAction / AutoDestroy).
            Assert.Less(Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude).Length, 3);
        }

        [UnityTest]
        public IEnumerator ArenaAndFxGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię aren i efektów."); yield break; }
            var bundle = DefaultContent.Create();
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 55f; cam.farClipPlane = 300f;
            var sunGo = new GameObject("Sun"); var sun = sunGo.AddComponent<Light>(); sun.type = LightType.Directional; sun.shadows = LightShadows.Soft; sun.intensity = 0.7f;
            sunGo.transform.rotation = Quaternion.Euler(50, -30, 0); cleanup.Add(sunGo);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.28f, 0.26f, 0.3f);
            QualitySettings.pixelLightCount = 8;

            void Shot(string name)
            {
                var rt = new RenderTexture(1920, 1080, 24);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); tex.Apply();
                cam.targetTexture = null; RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
                Object.Destroy(rt); Object.Destroy(tex);
            }

            foreach (var id in new[] { "arena_courtyard", "arena_crypt", "arena_summit" })
            {
                var def = bundle.Get<ArenaDefinition>(id);
                var arena = ArenaBuilder.Build(def, null);
                cleanup.Add(arena);
                sun.color = def.lightColor;
                RenderSettings.fogColor = def.fogColor;
                cam.backgroundColor = def.fogColor;
                float r = def.circular ? def.size : def.size * 0.5f;
                yield return new WaitForSeconds(0.6f);
                cam.transform.position = new Vector3(0, 3.2f, -r * 0.8f);
                cam.transform.LookAt(new Vector3(0, 1.8f, r * 0.4f));
                Shot($"arena_{id}_wide.png");
                cam.transform.position = new Vector3(r * 0.3f, 2.2f, r * 0.1f);
                cam.transform.LookAt(new Vector3(-r * 0.6f, 2.0f, -r * 0.7f));
                Shot($"arena_{id}_walls.png");
                cam.transform.position = new Vector3(0, 12f, -r * 1.05f);
                cam.transform.LookAt(new Vector3(0, 0, 0));
                Shot($"arena_{id}_top.png");

                if (id == "arena_courtyard")
                {
                    cam.transform.position = new Vector3(0, 2.2f, -6f);
                    cam.transform.LookAt(new Vector3(0, 1.0f, 0));
                    var v = new GameObject("FxTarget"); cleanup.Add(v);
                    var hit = new HitData { magic = 1, dodgeable = true };
                    // Każdy efekt osobno, zrzut w szczycie efektu
                    FxLibrary.Shockwave(Vector3.zero, new Color(0.6f, 0.5f, 1f), 3.5f);
                    yield return new WaitForSeconds(0.12f); Shot("fx_nova.png");
                    yield return new WaitForSeconds(1f);
                    FxLibrary.Parry(new Vector3(0, 1.3f, 0));
                    yield return new WaitForSeconds(0.08f); Shot("fx_parry.png");
                    yield return new WaitForSeconds(1f);
                    FxLibrary.Sparks(new Vector3(-1f, 1.2f, 0), Vector3.back, new Color(1f, 0.65f, 0.25f));
                    FxLibrary.Blood(new Vector3(1f, 1.2f, 0), Vector3.back, 1.5f);
                    FxLibrary.GuardBreak(new Vector3(0, 1.8f, 1f));
                    yield return new WaitForSeconds(0.1f); Shot("fx_hits.png");
                    yield return new WaitForSeconds(1f);
                    VisualFx.GroundMarker(new Vector3(0, 0, 1.5f), 3f, TelegraphColors.For(TelegraphKind.NoDodge));
                    FxLibrary.Heal(v.transform, new Color(1f, 0.9f, 0.5f), 2f);
                    FxLibrary.Gather(v.transform, new Color(0.45f, 0.65f, 1f), 1f);
                    Projectile.Spawn(new Vector3(-3f, 1.3f, -1.5f), Vector3.right, 7f, hit, Faction.Player, null, new Color(0.45f, 0.65f, 1f), 0.22f, 2f);
                    Projectile.Spawn(new Vector3(3f, 1.6f, 1f), Vector3.left, 6f, hit, Faction.Enemy, null, new Color(0.8f, 0.3f, 1f), 0.3f, 2f);
                    yield return new WaitForSeconds(0.45f); Shot("fx_spells.png");
                    FxLibrary.DeathAsh(new Vector3(0, 0, 1f), 1f);
                    yield return new WaitForSeconds(0.5f); Shot("fx_death.png");
                    foreach (var d in Object.FindObjectsByType<FxDecal>(FindObjectsInactive.Exclude)) Object.Destroy(d.gameObject);
                    foreach (var pr in Object.FindObjectsByType<Projectile>(FindObjectsInactive.Exclude)) Object.Destroy(pr.gameObject);
                }
                Object.Destroy(arena);
                yield return null;
            }
        }
    }
}
