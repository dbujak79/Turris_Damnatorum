#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Podmiana berserkera: stare klipy, dwa zestawy broni, deformacje i prawdziwa arena.</summary>
    public class BerserkerAnimePlayTests
    {
        const string Content="Assets/TurrisDamnatorum/Content/";
        const string VisualPath=Content+"Visuals/visual_berserker_anime.asset";
        readonly List<Object> cleanup=new List<Object>();
        string profile;
        static string Dir { get { string p=Path.Combine(Application.dataPath,"../TestResults/berserker-anime");Directory.CreateDirectory(p);return p; } }
        class Source : ICharacterAnimSource { public CharacterAnimState state;public CharacterAnimState GetAnimState()=>state; }
        class Forward : IPlayerDriver { public Vector3 MoveWorld=>Vector3.forward;public bool BlockHeld=>false;public bool SprintHeld=>true; }

        [SetUp] public void Setup()
        {
            profile="berserker_test_"+Guid.NewGuid().ToString("N")+".json";
            GameRoot.ProfileFileOverride=profile;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var obj in cleanup)if(obj!=null)Object.Destroy(obj);
            cleanup.Clear();Time.captureFramerate=0;GameRoot.ProfileFileOverride=null;
            string path=Path.Combine(Application.persistentDataPath,profile);if(File.Exists(path))File.Delete(path);
            var main=SceneManager.GetSceneByName("Main");
            if(main.IsValid()&&main.isLoaded) {
                SceneManager.SetActiveScene(SceneManager.CreateScene("AfterBerserkerTest"));
                yield return SceneManager.UnloadSceneAsync(main);
            }
            yield return null;
        }

        [UnityTest] public IEnumerator Berserker_Topor_StartBiegSeria()
        { yield return StartRun(0); }
        [UnityTest] public IEnumerator Berserker_Mlot_StartBiegSeria()
        { yield return StartRun(1); }

        IEnumerator StartRun(int kit)
        {
            SceneManager.LoadScene("Main");yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameRoot>();
            var definition=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            var berserker=root.config.classes.First(c=>c.id=="class_hu_berserker");
            var original=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Content+"Visuals/visual_hero_warrior_berserker.asset");
            Assert.AreSame(definition,berserker.Visual);
            Assert.AreSame(original.twoHandSet,definition.twoHandSet,"Te same klipy topora/młota");
            Assert.AreEqual("visual_mage_anime",root.config.classes.First(c=>c.id=="class_mage").Visual.name,"Mag ma osobną podmianę");
            Assert.AreEqual("visual_hero_warrior",root.config.classes.First(c=>c.id=="class_knight").Visual.name,"Wojownik bez podmiany");
            Assert.IsTrue(root.TryStartRun(new LoadoutPlan{classDef=berserker,loadoutIndex=kit,difficulty=root.config.difficulties[0]},out var reason),reason);
            yield return null;yield return null;
            var player=root.Player;
            var visual=player.GetComponent<CharacterVisual>();
            var animator=visual.Model.GetComponentInChildren<Animator>();
            Assert.IsTrue(animator.isHuman&&animator.avatar.isValid);
            Assert.IsInstanceOf<ClipAnimationDriver>(visual.Driver);
            Assert.AreSame(definition.twoHandSet,visual.ModelSet);
            Assert.IsNotNull(Util.FindDeep(visual.Model.transform,berserker.loadouts[kit].weapon.id));
            foreach(var enemy in root.Enemies)if(enemy!=null)enemy.gameObject.SetActive(false);
            root.enabled=false;
            Time.captureFramerate=60;
            for(int i=0;i<20;i++)yield return null;
            Shot(Camera.main,"game-"+kit+"-idle.png");
            var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            var controller=player.GetComponent<PlayerController>();
            var start=player.transform.position;controller.Driver=new Forward();
            for(int i=0;i<70;i++)yield return null;
            Assert.Greater(Vector3.Distance(start,player.transform.position),2f,"Berserker rzeczywiście biegnie");
            Shot(Camera.main,"game-"+kit+"-run.png");
            controller.Driver=null;for(int i=0;i<35;i++)yield return null;
            bool hit=false;float handTravel=0;var previous=hand.position;
            player.Request(ActionType.LightAttack);
            for(int i=0;i<140;i++) {
                var state=player.GetAnimState();
                hit|=state.action==AnimAction.Attack&&state.phase==ActionPhase.Active;
                handTravel+=Vector3.Distance(previous,hand.position);previous=hand.position;
                if(i==35||i==70)Shot(Camera.main,"game-"+kit+"-attack-"+i+".png");
                if(i==45)player.Request(ActionType.LightAttack);
                yield return null;
            }
            Assert.IsTrue(hit,"Atak osiąga fazę trafienia");
            Assert.Greater(handTravel,.4f,"Broń nie zostaje w pozie spoczynkowej");
            Assert.IsFalse(ShaderUtil.ShaderHasError(visual.Model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.shader));
        }

        [UnityTest] public IEnumerator Berserker_GeometriaWStarychAnimacjach()
        {
            var def=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            var cls=AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content+"Classes/class_hu_berserker.asset");
            var go=new GameObject("Berserker_Pozy");cleanup.Add(go);
            var visual=go.AddComponent<CharacterVisual>();visual.enabled=false;
            var source=new Source();var kit=cls.loadouts[0];
            visual.BuildModel(def,1,new RigLook{weapon=AnimResolve.ForItem(kit.weapon),weaponItem=kit.weapon});visual.Init(source);
            var animator=visual.Model.GetComponentInChildren<Animator>();
            var skin=visual.Model.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.Greater(skin.sharedMesh.vertexCount,1200000,"Pełna geometria źródłowa zachowana");
            Assert.IsTrue(skin.sharedMesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.002f));
            var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            var slot=Util.FindDeep(visual.Model.transform,"HandSlot.R");
            Assert.IsNotNull(slot);
            var slotRest=hand.InverseTransformPoint(slot.position);
            var baked=new Mesh();cleanup.Add(baked);
            var cameraGo=new GameObject("KameraBerserkera");cleanup.Add(cameraGo);
            var camera=cameraGo.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.15f,.17f,.21f);camera.fieldOfView=35;
            var lightGo=new GameObject("ŚwiatłoBerserkera");cleanup.Add(lightGo);
            var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.3f;
            lightGo.transform.rotation=Quaternion.Euler(40,-35,0);
            RenderSettings.ambientLight=new Color(.5f,.52f,.58f);
            var report=new System.Text.StringBuilder("poza\tminY\tmaxY\tszerokosc\tgniazdo_mm\n");
            var names=new[]{"idle","run","slash-right","slash-left","heavy","cast","dodge","death"};
            for(int pose=0;pose<names.Length;pose++) {
                source.state=pose<2?default:pose==5?new CharacterAnimState{action=AnimAction.Cast,phase=ActionPhase.Active,phaseProgress=.5f}:
                    pose==6?new CharacterAnimState{action=AnimAction.Dodge,actionTime=.3f}:
                    pose==7?new CharacterAnimState{action=AnimAction.Death,actionTime=.8f}:
                    new CharacterAnimState{action=AnimAction.Attack,attack=pose==2?AttackAnim.SlashRight:pose==3?AttackAnim.SlashLeft:AttackAnim.Overhead,phase=ActionPhase.Active,phaseProgress=.5f};
                for(int i=0;i<40;i++) {
                    if(pose==1)go.transform.position+=Vector3.forward*(4f/60);
                    visual.Tick(1f/60);
                }
                yield return null;yield return null;
                skin.BakeMesh(baked);
                var vertices=baked.vertices;
                Assert.IsFalse(vertices.Any(p=>!float.IsFinite(p.x)||!float.IsFinite(p.y)||!float.IsFinite(p.z)),names[pose]);
                Assert.Less(baked.bounds.size.magnitude,4.5f,names[pose]+": siatka nie eksploduje");
                float drift=Vector3.Distance(slotRest,hand.InverseTransformPoint(slot.position));
                Assert.Less(drift,.003f,"Gniazdo trzyma się dłoni");
                if(pose<5)Assert.Less(visual.GripError,.025f,"Druga zaciśnięta dłoń obejmuje drzewce");
                report.AppendLine(names[pose]+"\t"+baked.bounds.min.y+"\t"+baked.bounds.max.y+"\t"+baked.bounds.size.x+"\t"+drift*1000);
                camera.transform.position=go.transform.position+new Vector3(2.5f,1.6f,3.9f);
                camera.transform.LookAt(go.transform.position+Vector3.up*.95f);
                Shot(camera,"pose-"+names[pose]+".png");
                if(pose==0) {
                    foreach(var pair in new[]{new[]{"HandSlot.R","right"},new[]{"HandSlot.L","left"}}) {
                        var grip=Util.FindDeep(visual.Model.transform,pair[0]);
                        camera.transform.position=grip.position+new Vector3(.33f,.15f,.45f);
                        camera.transform.LookAt(grip.position);Shot(camera,"grip-"+pair[1]+".png");
                    }
                }
                if(pose==0) {
                    camera.transform.position=go.transform.position+new Vector3(-2.5f,1.6f,-3.9f);
                    camera.transform.LookAt(go.transform.position+Vector3.up*.95f);Shot(camera,"pose-back.png");
                }
            }
            File.WriteAllText(Path.Combine(Dir,"poses.tsv"),report.ToString());
        }

        static void Shot(Camera camera,string name)
        {
            Assert.IsNotNull(camera);
            var previous=RenderTexture.active;var previousTarget=camera.targetTexture;
            var target=RenderTexture.GetTemporary(1400,1000,24);var texture=new Texture2D(1400,1000,TextureFormat.RGB24,false);
            try {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,1400,1000),0,0);texture.Apply();
                File.WriteAllBytes(Path.Combine(Dir,name),texture.EncodeToPNG());
            } finally {camera.targetTexture=previousTarget;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.Destroy(texture);}
        }
    }
}
#endif
