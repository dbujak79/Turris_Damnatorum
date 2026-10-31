#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Pełne nowe siatki na starych rigach w rzeczywistym sterowniku demonów.</summary>
    public class AnimeImpRigPlayTests
    {
        readonly List<Object> cleanup=new List<Object>();
        string profile;
        [SetUp] public void Setup(){profile="anime_imps_test_"+System.Guid.NewGuid().ToString("N")+".json";GameRoot.ProfileFileOverride=profile;}
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var item in cleanup)if(item!=null)Object.Destroy(item);cleanup.Clear();
            GameRoot.ProfileFileOverride=null;
            var main=SceneManager.GetSceneByName("Main");
            if(main.IsValid()&&main.isLoaded){var empty=SceneManager.CreateScene("AfterAnimeImpTest");SceneManager.SetActiveScene(empty);yield return SceneManager.UnloadSceneAsync(main);}
            string path=Path.Combine(Application.persistentDataPath,profile);if(File.Exists(path))File.Delete(path);
            yield return null;
        }
        [UnityTest] public IEnumerator Fire_OldRig_DrivesWalkAttackDeathAndNewTail(){yield return Verify("fire","enemy_imp",27);}
        [UnityTest] public IEnumerator Water_OldRig_DrivesWalkAttackDeathWingsAndTail(){yield return Verify("water","enemy_imp_frost",26);}

        IEnumerator Verify(string kind,string enemyId,int bones)
        {
            var definition=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>("Assets/TurrisDamnatorum/Content/Visuals/visual_imp_"+kind+"_anime.asset");
            Assert.IsNotNull(definition);
            var enemy=Object.Instantiate(AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/TurrisDamnatorum/Content/Enemies/"+enemyId+".asset"));cleanup.Add(enemy);
            enemy.visual=definition;
            var visual=WorldBuilder.CreateEnemyVisual(enemy,Vector3.zero,null);cleanup.Add(visual.gameObject);
            var driver=(DemonAnimationDriver)visual.Driver;
            Assert.IsNotNull(driver);
            visual.enabled=false;
            var render=visual.Model.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.AreEqual(bones,render.bones.Length);
            Assert.Greater(render.sharedMesh.vertexCount,400000,"Pełna geometria źródła pozostaje zachowana");
            Assert.AreEqual("Turris/AnimeCreatureMatte",render.sharedMaterial.shader.name);
            Assert.IsNull(visual.Model.GetComponentInChildren<Animator>().avatar,"Jawne krzywe Generic działają na hierarchii Transform");
            var bind=render.sharedMesh.boneWeights;
            Assert.IsTrue(bind.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.002f),"Wagi są kompletne i znormalizowane");
            var state=new CharacterAnimState();
            for(int i=0;i<30;i++)driver.Tick(state,Vector3.zero,1f/60);
            var tracked=render.bones.First(b=>b.name=="forearm.L");
            Quaternion idle=tracked.localRotation;
            var wing=render.bones.FirstOrDefault(b=>b.name=="wing.L");
            Quaternion wingStart=wing!=null?wing.localRotation:Quaternion.identity;float wingMotion=0;
            for(int i=0;i<30;i++){driver.Tick(state,Vector3.forward*definition.walkSpeed,1f/60);if(wing!=null)wingMotion=Mathf.Max(wingMotion,Quaternion.Angle(wingStart,wing.localRotation));}
            if(kind=="water")Assert.Greater(wingMotion,.2f,"Stary klip porusza skrzydłami nowej siatki");
            Assert.Greater(Quaternion.Angle(idle,tracked.localRotation),1f,"Chód porusza ręką zamiast pozostawiać pozę wiązania");
            var tail=render.bones.First(b=>b.name=="tail3");Quaternion initialTail=tail.localRotation;
            for(int i=0;i<23;i++)driver.Tick(state,Vector3.zero,1f/60);
            Assert.Greater(Quaternion.Angle(initialTail,tail.localRotation),.2f,"Ogon jest animowany");
            var clips=new[]{definition.idle,definition.walk}.Concat(definition.actions.Select(a=>a.clip)).Distinct().ToArray();
            var baked=new Mesh();cleanup.Add(baked);
            float lowest=float.MaxValue,highest=float.MinValue;
            foreach(var clip in clips)
            {
                for(int sample=0;sample<=8;sample++)
                {
                    clip.SampleAnimation(visual.Model,clip.length*sample/8f);
                    render.BakeMesh(baked);
                    var vertices=baked.vertices;
                    Assert.IsFalse(vertices.Any(p=>float.IsNaN(p.x)||float.IsInfinity(p.x)||float.IsNaN(p.y)||float.IsInfinity(p.y)||float.IsNaN(p.z)||float.IsInfinity(p.z)),kind+" "+clip.name);
                    Assert.Less(baked.bounds.size.magnitude,5f,"Siatka nie eksploduje w "+clip.name);
                    if(clip==definition.idle)
                    {
                        float ground=vertices.Min(p=>render.transform.TransformPoint(p).y);
                        lowest=Mathf.Min(lowest,ground);highest=Mathf.Max(highest,ground);
                    }
                }
            }
            Assert.Greater(lowest,-.20f,"Brak głębokiego zapadania w podłoże podczas bezczynności");
            Assert.Less(highest,.20f,"Stopy pozostają blisko podłoża");
            state=new CharacterAnimState{action=AnimAction.Attack,attack=AttackAnim.Auto,phase=ActionPhase.Startup,windupSeconds=.3f};
            for(int i=0;i<20;i++){state.phaseProgress=i/19f;state.actionTime=i/60f;driver.Tick(state,Vector3.zero,1f/60);}
            Assert.IsNotNull(driver.CurrentAction);
            state.phase=ActionPhase.Active;driver.Tick(state,Vector3.zero,1f/60);
            Assert.Greater(driver.CurrentActionTime,0);
            state=new CharacterAnimState{action=AnimAction.Death,actionTime=.5f};
            for(int i=0;i<15;i++)driver.Tick(state,Vector3.zero,1f/60);
            Assert.AreEqual(AnimAction.Death,driver.CurrentAction.action);
            yield return null;
        }

        class Still : ICharacterAnimSource { public CharacterAnimState GetAnimState()=>new CharacterAnimState(); }

        [UnityTest] public IEnumerator FirstArena_NewImpsUseRealLightingAndOriginalCombatData()
        {
            SceneManager.LoadScene("Main");yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameRoot>();
            var paladin=root.config.classes.First(c=>c.id=="class_hu_paladin");
            Assert.IsTrue(root.TryStartRun(new LoadoutPlan{classDef=paladin,difficulty=root.config.difficulties[0]},out var reason),reason);
            root.enabled=false;root.CameraRig.enabled=false;
            foreach(var enemy in root.Enemies)if(enemy!=null)enemy.gameObject.SetActive(false);
            var origin=root.Player.transform.position+Vector3.forward*3;
            if(Physics.Raycast(origin+Vector3.up*5,Vector3.down,out var ground,10))origin.y=ground.point.y+.015f;
            root.Player.gameObject.SetActive(false);
            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))canvas.enabled=false;
            foreach(var pair in new[]{new[]{"enemy_imp","fire"},new[]{"enemy_imp_frost","water"}})
            {
                var enemy=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/TurrisDamnatorum/Content/Enemies/"+pair[0]+".asset");
                var definition=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>("Assets/TurrisDamnatorum/Content/Visuals/visual_imp_"+pair[1]+"_anime.asset");
                Assert.AreSame(definition,enemy.visual);
                var visual=WorldBuilder.CreateEnemyVisual(enemy,origin+Vector3.right*(pair[1]=="fire"?-1.3f:1.3f),null);cleanup.Add(visual.gameObject);
                visual.Init(new Still());visual.enabled=false;
                for(int i=0;i<30;i++)visual.Tick(1f/60);
                Assert.IsFalse(ShaderUtil.ShaderHasError(visual.Model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.shader));
                if(FxLibrary.Enabled)Assert.Greater(visual.Model.GetComponentsInChildren<ParticleSystem>(true).Length,0,"Zachowane doczepienia VFX żywiołu");
            }
            var camera=Camera.main;camera.fieldOfView=48;
            camera.transform.position=origin+new Vector3(.4f,1.7f,6);camera.transform.LookAt(origin+Vector3.up*.7f);
            yield return null;yield return null;
            Shot(camera,"first-arena-imps.png");
            foreach(var kind in new[]{"fire","water"})
            {
                var center=origin+Vector3.right*(kind=="fire"?-1.3f:1.3f);
                camera.transform.position=center+new Vector3(.3f,1.05f,3.2f);camera.transform.LookAt(center+Vector3.up*.66f);camera.fieldOfView=38;
                yield return null;Shot(camera,kind+"-arena-close.png");
            }
        }

        static void Shot(Camera camera,string name)
        {
            string dir=Path.GetFullPath("TestResults/anime-imps");Directory.CreateDirectory(dir);
            var target=RenderTexture.GetTemporary(1920,1080,24);var previous=RenderTexture.active;
            var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Path.Combine(dir,name),image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.Destroy(image);}
        }
    }
}
#endif
