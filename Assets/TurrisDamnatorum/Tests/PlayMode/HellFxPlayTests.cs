using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Import HU: poprawne atlasy, współdzielenie materiałów, ponowne użycie, pauza i dogasanie.</summary>
    public class HellFxPlayTests
    {
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            foreach (var fx in Object.FindObjectsByType<HellParticles>(FindObjectsInactive.Include)) if (fx != null) Object.Destroy(fx.gameObject);
            foreach (var fx in Object.FindObjectsByType<HellFxPool>()) if (fx != null) Object.Destroy(fx.gameObject);
            foreach (var fx in Object.FindObjectsByType<FxDecal>()) if (fx != null) Object.Destroy(fx.gameObject);
            foreach (var fx in Object.FindObjectsByType<Projectile>()) if (fx != null) Object.Destroy(fx.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ImportedAtlases_KeepRandomPicturesSeparateFromAnimation()
        {
            var fx = HellFx.Burst(Vector3.up, HellFxElement.Fire, 2.4f);
            var layers = fx.GetComponentsInChildren<ParticleSystem>();
            var flame = layers.First(p => p.name == "flame_sheet");
            var puffs = layers.First(p => p.name == "fire_puffs_atlas");
            Assert.AreEqual(8, flame.textureSheetAnimation.numTilesX);
            Assert.AreEqual(8, flame.textureSheetAnimation.numTilesY);
            Assert.Greater(flame.textureSheetAnimation.frameOverTime.curve.Evaluate(1),0.9f);
            Assert.AreEqual(4,puffs.textureSheetAnimation.numTilesX);
            Assert.AreEqual(0,puffs.textureSheetAnimation.frameOverTime.curve.Evaluate(1),0.001f,"Atlas puchów nie jest animacją");
            Assert.AreEqual(38, Resources.LoadAll<Texture2D>("HellVfx").Length);
            Assert.IsTrue(flame.GetComponent<Renderer>().sharedMaterial.shader.isSupported);
            Assert.IsTrue(HellVfxMaterials.Ground(HellFxElement.Fire).shader.isSupported);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BurstPool_IsBounded_ClearsOldParticlesAndSharesMaterials()
        {
            var first = HellFx.Burst(new Vector3(100,1,0),HellFxElement.Fire,1);
            foreach(var ps in first.GetComponentsInChildren<ParticleSystem>()) ps.Simulate(0.1f,false,true);
            for(int i=1;i<HellFxPool.BurstsPerElement;i++) HellFx.Burst(Vector3.up,HellFxElement.Fire,1);
            var recycled = HellFx.Burst(Vector3.up,HellFxElement.Fire,1);
            Assert.AreSame(first,recycled);
            var systems = recycled.GetComponentsInChildren<ParticleSystem>();
            var data = new ParticleSystem.Particle[256];
            foreach(var ps in systems)
            {
                ps.Simulate(0.03f,false,true);
                int count=ps.GetParticles(data);
                for(int i=0;i<count;i++) Assert.Less(Vector3.Distance(data[i].position,Vector3.up),3,"Brak cząsteczek z poprzedniego miejsca");
            }
            var another = HellFx.Burst(Vector3.up,HellFxElement.Fire,1);
            Assert.AreSame(systems[0].GetComponent<Renderer>().sharedMaterial,another.GetComponentsInChildren<ParticleSystem>()[0].GetComponent<Renderer>().sharedMaterial);
            Assert.AreEqual(HellFxPool.BurstsPerElement,HellFxPool.Instance.GetComponentsInChildren<HellParticles>(true).Count(f => f.name == "HU_Burst_Fire"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TimedGround_PausesAndStopsEmitting_ThenSmokeDiesOut()
        {
            var ground=HellFx.Ground(Vector3.zero,2,HellFxElement.Fire,Names.ElementColor(Element.Fire),0.3f);
            var fx=ground.GetComponentInChildren<HellParticles>();
            Time.timeScale=0;
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.IsTrue(fx.Emitting,"Pauza zatrzymuje czas emisji");
            Time.timeScale=1;
            yield return new WaitForSeconds(0.4f);
            Assert.IsFalse(fx.Emitting);
            Assert.IsNull(fx.transform.parent,"Dym dogasa niezależnie od strefy");
            yield return new WaitForSeconds(2.5f);
            Assert.IsTrue(fx==null); Assert.IsTrue(ground==null);
        }

        [UnityTest]
        public IEnumerator DestroyingProjectile_ReleasesItsTrail()
        {
            var p=Projectile.Spawn(new Vector3(0,10,0),Vector3.forward,2,new HitData { element=Element.Fire },Faction.Player,null,Color.red);
            var fx=p.GetComponentInChildren<HellParticles>();
            yield return new WaitForSeconds(0.15f);
            Object.Destroy(p.gameObject);
            yield return null; yield return null;
            Assert.IsNotNull(fx);
            Assert.IsNull(fx.transform.parent);
            Assert.IsFalse(fx.Emitting);
            yield return new WaitForSeconds(2);
            Assert.IsTrue(fx==null);
        }

        [UnityTest]
        public IEnumerator Gallery_WhenRequested()
        {
            string dir=System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if(string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię HU."); yield break; }
            Directory.CreateDirectory(dir);
            var cameraGo=new GameObject("HU_GalleryCamera"); var camera=cameraGo.AddComponent<Camera>();
            cameraGo.transform.position=new Vector3(0,7,-11); cameraGo.transform.LookAt(new Vector3(0,0.8f,1));
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(0.045f,0.04f,0.05f); camera.fieldOfView=50;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube); floor.transform.position=new Vector3(0,-0.2f,0); floor.transform.localScale=new Vector3(30,0.2f,30);
            var floorMaterial=new Material(Shader.Find(MaterialLibrary.LitShader)) { color=new Color(0.06f,0.055f,0.065f) };
            floor.GetComponent<Renderer>().sharedMaterial=floorMaterial;
            RenderSettings.fog=false; RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(0.16f,0.14f,0.18f);
            var lightGo=new GameObject("HU_GalleryLight"); var light=lightGo.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=0.6f;
            lightGo.transform.rotation=Quaternion.Euler(50,-30,0);
            var families=new[] { HellFxElement.Fire,HellFxElement.Water,HellFxElement.Air,HellFxElement.Earth,HellFxElement.Ice };
            try
            {
                foreach(var family in families)
                {
                    var zone=HellFx.Ground(new Vector3(-2.5f,0,1),1.7f,family,Color.white,2.5f);
                    var carrier=new GameObject("HU_GalleryBolt"); carrier.transform.position=new Vector3(0,1.4f,0);
                    var trail=HellFx.Trail(carrier.transform,family,0.3f);
                    yield return new WaitForSeconds(0.6f);
                    HellFx.Burst(new Vector3(2.5f,0.4f,1),family,2);
                    yield return new WaitForSeconds(0.12f);
                    Shot(camera,Path.Combine(dir,"hu_fx_"+family+".png"));
                    Object.Destroy(carrier); Object.Destroy(zone);
                    yield return new WaitForSeconds(2.5f);
                }
                HellFx.Blood(new Vector3(-2,1,0),Vector3.right,2); HellFx.Shadow(null,new Vector3(2,0,0));
                yield return new WaitForSeconds(0.15f); Shot(camera,Path.Combine(dir,"hu_fx_blood_shadow.png"));
            }
            finally { Object.Destroy(cameraGo); Object.Destroy(floor); Object.Destroy(lightGo); Object.Destroy(floorMaterial); }
        }

        static void Shot(Camera camera,string path)
        {
            var previous=RenderTexture.active; var rt=new RenderTexture(1280,800,24); var texture=new Texture2D(1280,800,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=rt; camera.Render(); RenderTexture.active=rt;
                texture.ReadPixels(new Rect(0,0,1280,800),0,0); texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG());
            }
            finally { camera.targetTexture=null; RenderTexture.active=previous; Object.Destroy(rt); Object.Destroy(texture); }
        }
    }
}
