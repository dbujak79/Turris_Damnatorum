#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Porównanie tych samych póz i świateł na prawdziwej arenie; bez zmian profilu użytkownika.</summary>
    public class AnimeHeroStylePlayTests
    {
        string profile;
        bool asyncCompilation;
        readonly List<Object> cleanup=new List<Object>();
        static string Dir {
            get {string p=Path.Combine(Application.dataPath,"../TestResults/hero-style-v2");Directory.CreateDirectory(p);return p;}
        }

        [SetUp] public void Setup() {
            profile="hero_style_"+Guid.NewGuid().ToString("N")+".json";
            GameRoot.ProfileFileOverride=profile;
            asyncCompilation=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
        }

        [UnityTearDown] public IEnumerator Cleanup() {
            foreach(var obj in cleanup)if(obj!=null)Object.Destroy(obj);
            cleanup.Clear();GameRoot.ProfileFileOverride=null;Time.captureFramerate=0;
            ShaderUtil.allowAsyncCompilation=asyncCompilation;
            string path=Path.Combine(Application.persistentDataPath,profile);if(File.Exists(path))File.Delete(path);
            yield return UnloadMain();
        }

        static IEnumerator UnloadMain() {
            var main=SceneManager.GetSceneByName("Main");
            if(main.IsValid()&&main.isLoaded) {
                SceneManager.SetActiveScene(SceneManager.CreateScene("AfterHeroStyle"));
                yield return SceneManager.UnloadSceneAsync(main);
            }
            yield return null;
        }

        [UnityTest] public IEnumerator Korekta_MagIBerserker_TeSameKadryISwiatlo()
        {
            foreach(string key in new[]{"mage","berserker"}) {
                SceneManager.LoadScene("Main");yield return null;yield return null;
                var root=Object.FindAnyObjectByType<GameRoot>();
                var cls=root.config.classes.First(c=>c.id==(key=="mage"?"class_mage":"class_hu_berserker"));
                Assert.IsTrue(root.TryStartRun(new LoadoutPlan{classDef=cls,difficulty=root.config.difficulties[0]},out var reason),reason);
                yield return null;yield return null;
                foreach(var enemy in root.Enemies)if(enemy!=null)enemy.gameObject.SetActive(false);
                root.enabled=false;Time.captureFramerate=60;
                for(int i=0;i<30;i++)yield return null;
                var player=root.Player;
                var visual=player.GetComponent<CharacterVisual>();
                var skins=visual.Model.GetComponentsInChildren<SkinnedMeshRenderer>().Where(s=>s.sharedMesh.vertexCount>100000).ToArray();
                Assert.IsNotEmpty(skins);
                string folder="Assets/TurrisDamnatorum/Models/PoC/"+(key=="mage"?"MageAnime":"BerserkerAnime");
                var baseline=AssetDatabase.LoadAssetAtPath<Material>(folder+"/"+key+"_anime.mat");
                var corrected=AssetDatabase.LoadAssetAtPath<Material>(folder+"/"+key+"_anime_painterly_v2.mat");
                Assert.IsNotNull(baseline);Assert.IsNotNull(corrected);
                Assert.AreEqual("Turris/AnimeHeroPainterly",skins[0].sharedMaterial.shader.name,"Korekta aktywna w rzeczywistym modelu klasy");
                Assert.AreSame(baseline.GetTexture("_BaseMap"),corrected.GetTexture("_BaseMap"),"Oryginalna mapa 4K i UV zachowane");
                Assert.IsFalse(ShaderUtil.ShaderHasError(corrected.shader));

                visual.enabled=false;player.enabled=false;
                player.GetComponent<PlayerController>().enabled=false;
                foreach(var skin in skins)skin.SetPropertyBlock(null);
                var camera=Camera.main;
                Pair(camera,skins,baseline,corrected,key+"-game");
                // Oceniaj własny podshader, nie awaryjny przebieg wybrany przed inicjalizacją URP.
                var own=ShaderUtil.GetShaderData(corrected.shader).GetSubshader(0);
                var passes=Enumerable.Range(0,own.PassCount).Select(p=>own.GetPass(p).Name).ToArray();
                Debug.Log("HERO_STYLE_PASSES "+string.Join(",",passes));
                foreach(string pass in new[]{"AnimeForward","ShadowCaster","DepthOnly","DepthNormals"})
                    Assert.IsTrue(passes.Any(p=>string.Equals(p,pass,StringComparison.OrdinalIgnoreCase)),pass+": "+string.Join(",",passes));
                for(int pass=0;pass<own.PassCount;pass++)foreach(var stage in new[]{ShaderType.Vertex,ShaderType.Fragment}) {
                    var compiled=own.GetPass(pass).CompileVariant(stage,Array.Empty<string>(),ShaderCompilerPlatform.D3D,BuildTarget.StandaloneWindows64);
                    Assert.IsTrue(compiled.Success,passes[pass]+"/"+stage+": "+string.Join(";",compiled.Messages.Select(m=>m.message)));
                }
                var camGo=new GameObject("Porównanie stylu");cleanup.Add(camGo);
                var review=camGo.AddComponent<Camera>();review.CopyFrom(camera);review.enabled=false;
                review.fieldOfView=32;review.nearClipPlane=.05f;
                foreach(string angle in new[]{"front","back","face"}) {
                    Vector3 center=angle=="face"?visual.Model.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Head).position+Vector3.up*.04f:
                        player.transform.position+Vector3.up;
                    Vector3 offset=angle=="front"?new Vector3(2.1f,.5f,3.7f):angle=="back"?new Vector3(-2.1f,.5f,-3.7f):new Vector3(.5f,-.06f,.95f);
                    review.transform.position=center+offset;review.transform.LookAt(center);
                    Pair(review,skins,baseline,corrected,key+"-arena-"+angle);
                }

                // Rzeczywiste dodatkowe światło zmienia obraz: nie wypiekamy pomarańczowej obwódki w albedo.
                review.transform.position=player.transform.position+new Vector3(2.1f,1.5f,3.7f);
                review.transform.LookAt(player.transform.position+Vector3.up);
                Assign(skins,corrected);
                // W tej próbie oglądamy wyłącznie ciało: zmiana podłogi albo broni nie zaliczy testu świateł shadera.
                var bodyLayers=skins.Select(s=>(skin:s,layer:s.gameObject.layer)).ToArray();
                int arenaMask=review.cullingMask;var arenaClear=review.clearFlags;var arenaBackground=review.backgroundColor;
                foreach(var entry in bodyLayers)entry.skin.gameObject.layer=29;
                review.cullingMask=1<<29;review.clearFlags=CameraClearFlags.SolidColor;review.backgroundColor=new Color(.18f,.20f,.25f);
                var without=Capture(review,key+"-torch-off.png");
                var lightGo=new GameObject("Próba ciepłego światła");cleanup.Add(lightGo);
                var light=lightGo.AddComponent<Light>();light.type=LightType.Point;
                light.color=new Color(1,.49f,.16f);light.range=5;light.intensity=5;
                lightGo.transform.position=player.transform.position+new Vector3(-.8f,1.5f,1);
                var with=Capture(review,key+"-torch-on.png");
                Assert.Greater(Difference(without,with),.0001f,"Dodatkowe światło URP wpływa na shader");
                Debug.Log("HERO_STYLE_POINT_LIGHT "+key+" rgb_delta="+Difference(without,with));
                foreach(var entry in bodyLayers)entry.skin.gameObject.layer=entry.layer;
                review.cullingMask=arenaMask;review.clearFlags=arenaClear;review.backgroundColor=arenaBackground;
                Object.Destroy(lightGo);

                // Powtarzalna galeria w neutralnym oświetleniu, niezależna od palety areny.
                var lights=Object.FindObjectsByType<Light>().Where(l=>l.enabled&&l!=light).ToArray();
                foreach(var old in lights)old.enabled=false;
                var ambient=RenderSettings.ambientMode;var ambientColor=RenderSettings.ambientLight;
                RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.45f,.48f,.55f);
                var neutralGo=new GameObject("Neutralne światło bohatera");cleanup.Add(neutralGo);
                var neutral=neutralGo.AddComponent<Light>();neutral.type=LightType.Directional;neutral.intensity=1.2f;neutral.color=Color.white;
                neutralGo.transform.rotation=Quaternion.Euler(38,-30,0);
                int oldMask=review.cullingMask;review.cullingMask=1<<29;
                var layers=visual.Model.GetComponentsInChildren<Transform>().Select(t=>(t,t.gameObject.layer)).ToArray();
                foreach(var entry in layers)entry.t.gameObject.layer=29;
                review.clearFlags=CameraClearFlags.SolidColor;review.backgroundColor=new Color(.18f,.20f,.25f);
                Pair(review,skins,baseline,corrected,key+"-neutral-front");
                review.transform.position=player.transform.position+new Vector3(-2.1f,1.5f,-3.7f);
                review.transform.LookAt(player.transform.position+Vector3.up);
                Pair(review,skins,baseline,corrected,key+"-neutral-back");
                foreach(var entry in layers)entry.t.gameObject.layer=entry.layer;
                review.cullingMask=oldMask;RenderSettings.ambientMode=ambient;RenderSettings.ambientLight=ambientColor;
                foreach(var old in lights)if(old!=null)old.enabled=true;
                Object.Destroy(neutralGo);Object.Destroy(camGo);
                yield return UnloadMain();
            }
        }

        static void Assign(SkinnedMeshRenderer[] skins,Material material) {
            foreach(var skin in skins) {
                skin.sharedMaterials=skin.sharedMaterials.Select(_=>material).ToArray();
                skin.SetPropertyBlock(null);
            }
        }
        static void Pair(Camera camera,SkinnedMeshRenderer[] skins,Material baseline,Material corrected,string name) {
            Assign(skins,baseline);var before=Capture(camera,name+"-before.png");
            Assign(skins,corrected);var after=Capture(camera,name+"-after.png");
            float difference=Difference(before,after);
            Assert.Greater(difference,.0001f,"Korekta widoczna w obrazie: "+name);
            if(!name.EndsWith("face",StringComparison.Ordinal)) {
                double background=0;
                for(int y=1080;y<1200;y++)for(int x=0;x<160;x++) {
                    int p=y*1600+x;background+=Math.Abs(before[p].r-after[p].r)+Math.Abs(before[p].g-after[p].g)+Math.Abs(before[p].b-after[p].b);
                }
                Assert.Less(background/(120*160*3*255),.005,"Stałe tło i światło w porównaniu: "+name);
            }
            File.AppendAllText(Path.Combine(Dir,"comparison.tsv"),name+"\t"+difference.ToString(System.Globalization.CultureInfo.InvariantCulture)+"\n");
        }
        static float Difference(Color32[] a,Color32[] b) {
            double sum=0;for(int i=0;i<a.Length;i++)sum+=Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b);
            return (float)(sum/(a.Length*3*255));
        }
        static Color32[] Capture(Camera camera,string name) {
            var oldActive=RenderTexture.active;var oldTarget=camera.targetTexture;
            var target=RenderTexture.GetTemporary(1600,1200,24);var texture=new Texture2D(1600,1200,TextureFormat.RGB24,false);
            try {
                // Stabilizujemy także proporcje render targetu oraz cienie URP na tej samej kamerze.
                camera.targetTexture=target;camera.Render();camera.Render();camera.Render();RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,1600,1200),0,0);texture.Apply();
                var pixels=texture.GetPixels32();
                int magenta=pixels.Count(c=>c.r>220&&c.g<40&&c.b>220);
                File.WriteAllBytes(Path.Combine(Dir,name),texture.EncodeToPNG());
                Assert.Less(magenta,pixels.Length/500,"Brak różowego materiału: "+name);return pixels;
            }
            finally {camera.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(target);Object.Destroy(texture);}
        }
    }
}
#endif
