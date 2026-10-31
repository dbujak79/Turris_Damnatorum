#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Turris.Tests
{
    public class GothicArenaPlayTests
    {
        readonly List<Object> cleanup=new List<Object>();
        Material oldSky; Color oldAmbient,oldFog; float oldStart,oldEnd;
        string profile;
        const string Dir="TestResults/gothic-arena";
        [SetUp] public void Setup()
        {
            oldSky=RenderSettings.skybox; oldAmbient=RenderSettings.ambientLight; oldFog=RenderSettings.fogColor;
            oldStart=RenderSettings.fogStartDistance; oldEnd=RenderSettings.fogEndDistance;
            profile="gothic_arena_test_"+System.Guid.NewGuid().ToString("N")+".json";
            GameRoot.ProfileFileOverride=profile;
            Directory.CreateDirectory(Dir);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var item in cleanup) if(item!=null) Object.Destroy(item);
            cleanup.Clear(); GameRoot.ProfileFileOverride=null;
            string path=Path.Combine(Application.persistentDataPath,profile); if(File.Exists(path)) File.Delete(path);
            var main=SceneManager.GetSceneByName("Main");
            if(main.IsValid() && main.isLoaded)
            {
                var empty=SceneManager.CreateScene("AfterGothicTest"); SceneManager.SetActiveScene(empty);
                yield return SceneManager.UnloadSceneAsync(main);
            }
            RenderSettings.skybox=oldSky; RenderSettings.ambientLight=oldAmbient; RenderSettings.fogColor=oldFog;
            RenderSettings.fogStartDistance=oldStart; RenderSettings.fogEndDistance=oldEnd;
            yield return null;
        }

        [UnityTest] public IEnumerator NewArena_UsesDetailedModulesAndKeepsSimpleColliders()
        {
            var def=AssetDatabase.LoadAssetAtPath<ArenaDefinition>("Assets/TurrisDamnatorum/Content/Arenas/arena_courtyard_gothic.asset");
            var arena=WorldBuilder.BuildArena(def,null); cleanup.Add(arena);
            yield return null;
            var renderers=arena.GetComponentsInChildren<MeshRenderer>();
            int triangles=renderers.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3);
            Assert.Greater(triangles,90000,"Pełne modele i detale, bez zastąpienia architektury pudełkami");
            Assert.Less(triangles,500000); Assert.Less(renderers.Length,80,"Moduły statyczne scalone przestrzennie");
            Assert.AreEqual(0,arena.GetComponentsInChildren<MeshCollider>().Length);
            foreach(var collider in arena.GetComponentsInChildren<Collider>()) Assert.IsNull(collider.GetComponent<Renderer>());
            foreach(var p in new[]{WorldBuilder.PlayerSpawn(def),Vector3.zero,new Vector3(3,0,6)})
            {
                Assert.IsTrue(Physics.Raycast(p+Vector3.up*2,Vector3.down,out var hit,4));
                Assert.That(hit.point.y,Is.InRange(-.001f,.001f),"Płaska powierzchnia kolizji pomimo szczegółowej posadzki");
            }
            Assert.IsNotNull(arena.transform.Find("Zamek_Na_Skale"));
            Assert.Greater(arena.GetComponentsInChildren<ParticleSystem>().Length,0);
            Debug.Log($"Gotycka arena: renderery {renderers.Length}, trójkąty {triangles}, kolizje {arena.GetComponentsInChildren<Collider>().Length}");
            var legacy=WorldBuilder.BuildArena(def.gothicSet.originalArena,null); cleanup.Add(legacy);
            Assert.IsTrue(legacy.name.StartsWith("Arena_arena_courtyard"));
            Assert.IsNull(legacy.GetComponent<GothicArenaMeshes>(),"Stary generator nadal buduje oryginalną arenę");
        }

        [UnityTest] public IEnumerator Main_FirstFloorAndConceptGallery()
        {
            SceneManager.LoadScene("Main"); yield return null; yield return null;
            var root=Object.FindAnyObjectByType<GameRoot>();
            var paladin=root.config.classes.First(c=>c.id=="class_hu_paladin");
            Assert.IsTrue(root.TryStartRun(new LoadoutPlan { classDef=paladin,difficulty=root.config.difficulties[0] },out var reason),reason);
            for(int i=0;i<20;i++) yield return null;
            int index=0;
            foreach(var enemy in root.Enemies)
            {
                if(enemy==null) continue;
                if(index++==0) { enemy.enabled=false; enemy.transform.position=new Vector3(3,.1f,7); }
                else enemy.gameObject.SetActive(false);
            }
            var camera=Camera.main; Assert.AreEqual(CameraClearFlags.Skybox,camera.clearFlags);
            root.CameraRig.SnapBehindTarget(); root.CameraRig.enabled=false;
            var built=Object.FindAnyObjectByType<GothicArenaMeshes>(); Assert.IsNotNull(built);
            var fire=built.GetComponentsInChildren<ParticleSystem>();
            // W galerii pokazujemy ustabilizowane pochodnie, niezależnie od szybkości klatek edytora.
            foreach(var ps in fire) { ps.Simulate(.7f,false,true); ps.Play(false); }
            Assert.Greater(fire.Sum(p=>p.particleCount),0,"Pochodnie rzeczywiście emitują płomienie");
            yield return null; yield return null;
            Shot(camera,"00-warmup.png"); yield return null; yield return null;
            Shot(camera,"01-gameplay.png");
            camera.transform.position=new Vector3(-.7f,2.7f,-16.3f);
            camera.transform.LookAt(new Vector3(0,1.8f,8)); camera.fieldOfView=58;
            Shot(camera,"02-concept-angle.png");
            camera.transform.position=new Vector3(-10,4,-13); camera.transform.LookAt(new Vector3(2,3.7f,9)); camera.fieldOfView=65;
            Shot(camera,"03-architecture.png");
            camera.transform.position=new Vector3(2,1.7f,-4); camera.transform.LookAt(new Vector3(-1,0,2)); camera.fieldOfView=60;
            Shot(camera,"04-floor-detail.png");
            var torch=built.GetComponentsInChildren<Light>().First(l=>l.name=="Pochodnia_CiepleSwiatlo").transform;
            camera.transform.position=torch.position+new Vector3(-1.1f,.5f,-4);
            camera.transform.LookAt(torch.position); camera.fieldOfView=40;
            Shot(camera,"05-torch-detail.png");
            var flame=fire.First(p=>p.name=="flame_sheet");
            Debug.Log($"Pochodnia: cząstki {flame.particleCount}, emiter {flame.transform.position}, obrys {flame.GetComponent<ParticleSystemRenderer>().bounds}");
            Assert.IsFalse(ShaderUtil.ShaderHasError(root.config.tower.floors[0].arena.gothicSet.stone.shader));
        }

        [UnityTest] public IEnumerator Paladin_MaterialReviewOnFirstFloor()
        {
            SceneManager.LoadScene("Main"); yield return null; yield return null;
            var root=Object.FindAnyObjectByType<GameRoot>();
            var paladin=root.config.classes.First(c=>c.id=="class_hu_paladin");
            Assert.IsTrue(root.TryStartRun(new LoadoutPlan { classDef=paladin,difficulty=root.config.difficulties[0] },out var reason),reason);
            foreach(var enemy in root.Enemies) if(enemy!=null) enemy.gameObject.SetActive(false);
            root.CameraRig.SnapBehindTarget(); root.CameraRig.enabled=false;
            for(int i=0;i<20;i++) yield return null;
            var visual=root.Player.GetComponent<CharacterVisual>();
            visual.enabled=false; root.Player.enabled=false;
            var camera=Camera.main;
            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) canvas.enabled=false;
            string output="TestResults/paladin-look/current"; Directory.CreateDirectory(output);
            var position=root.Player.transform.position;
            // Model obejmuje też broń; weryfikujemy siatkę ciała z awatarem Humanoid.
            var animator=visual.Model.GetComponentInChildren<Animator>();
            var spine=animator.GetBoneTransform(HumanBodyBones.Spine);
            var material=visual.Model.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.bones.Contains(spine)).sharedMaterial;
            Assert.AreEqual("Turris/AnimeMatte",material.shader.name);
            Assert.IsFalse(ShaderUtil.ShaderHasError(material.shader));
            var offsets=new[]{new Vector3(-2.4f,1.65f,-3.5f),new Vector3(2.4f,1.65f,3.5f),new Vector3(3.5f,1.5f,-1.5f)};
            for(int i=0;i<offsets.Length;i++)
            {
                camera.transform.position=position+offsets[i]; camera.transform.LookAt(position+Vector3.up*.95f); camera.fieldOfView=35;
                Shot(camera,Path.GetFullPath(Path.Combine(output,"warmup.png"))); yield return null; yield return null;
                Shot(camera,Path.GetFullPath(Path.Combine(output,"paladin-"+i+".png")));
            }
            root.CameraRig.enabled=true; root.CameraRig.SnapBehindTarget(); root.CameraRig.enabled=false;
            camera.fieldOfView=58;
            Shot(camera,Path.GetFullPath(Path.Combine(output,"gameplay.png")));
        }

        [UnityTest] public IEnumerator DistanceFog_PreservesSilhouetteWhenDisabled()
        {
            var shader=Shader.Find("Turris/GothicStone");
            var material=new Material(shader); cleanup.Add(material);
            material.SetFloat("_Unlit",1); material.SetFloat("_FogWeight",0);
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad); cleanup.Add(quad);
            quad.layer=30; var mesh=Object.Instantiate(quad.GetComponent<MeshFilter>().sharedMesh); cleanup.Add(mesh);
            mesh.colors=Enumerable.Repeat(new Color(.4f,.5f,.65f),mesh.vertexCount).ToArray();
            quad.GetComponent<MeshFilter>().sharedMesh=mesh; quad.GetComponent<Renderer>().sharedMaterial=material;
            var go=new GameObject("KameraMgly"); cleanup.Add(go); var camera=go.AddComponent<Camera>();
            camera.cullingMask=1<<30; camera.transform.position=new Vector3(0,0,-3); camera.orthographic=true; camera.orthographicSize=.5f;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=Color.black;
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=0; RenderSettings.fogEndDistance=1; RenderSettings.fogColor=Color.black;
            yield return null; yield return null;
            Color Read()
            {
                var target=RenderTexture.GetTemporary(64,64,24); var previous=RenderTexture.active;
                var pixels=new Texture2D(64,64,TextureFormat.RGB24,false);
                try
                {
                    camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
                    pixels.ReadPixels(new Rect(0,0,64,64),0,0); pixels.Apply(); return pixels.GetPixel(32,32);
                }
                finally { camera.targetTexture=null; RenderTexture.active=previous; RenderTexture.ReleaseTemporary(target); Object.Destroy(pixels); }
            }
            var control=new Material(Shader.Find("Universal Render Pipeline/Unlit")); cleanup.Add(control);
            control.SetColor("_BaseColor",new Color(.4f,.5f,.65f)); quad.GetComponent<Renderer>().sharedMaterial=control;
            RenderSettings.fog=false;
            var reference=Read(); Assert.Greater(reference.b,.3f,"Kontrola dodatnia: kamera i powierzchnia rzeczywiście trafiają do próbki");
            RenderSettings.fog=true;
            quad.GetComponent<Renderer>().sharedMaterial=material;
            // Pierwszy render nowej konfiguracji może jeszcze korzystać z poprzedniego stanu renderera URP.
            // Rozgrzewamy go przed pomiarem, tak jak przed galerią nowej areny.
            Read(); yield return null; yield return null;
            var clear=Read(); material.SetFloat("_FogWeight",1); var fog=Read();
            Debug.Log($"Kontrola kamery: {reference}, materiał bez mgły {clear}, z mgłą {fog}");
            Assert.Greater(clear.grayscale,.15f,"Wyłączenie mgły zachowuje namalowaną sylwetę, zamiast zastępować ją mgłą");
            Assert.Greater(clear.b-clear.r,.05f,"Paleta odległego zamku zachowuje błękit");
            Assert.Less(fog.grayscale,clear.grayscale*.35f,"Pełna mgła rzeczywiście zasłania odległy obiekt");
            Debug.Log($"Mgła sylwety: wyłączona {clear}, pełna {fog}");
        }

        static void Shot(Camera camera,string name)
        {
            var target=RenderTexture.GetTemporary(1920,1080,24); var previous=RenderTexture.active;
            var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,1920,1080),0,0); texture.Apply();
                File.WriteAllBytes(Path.Combine(Dir,name),texture.EncodeToPNG());
            }
            finally {camera.targetTexture=null; RenderTexture.active=previous; RenderTexture.ReleaseTemporary(target); Object.Destroy(texture);}
        }
    }
}
#endif
