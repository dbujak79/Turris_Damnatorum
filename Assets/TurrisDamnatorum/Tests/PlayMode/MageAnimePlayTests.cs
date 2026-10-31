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
using Object=UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Nowy mag: stare animacje, różdżka, kostur, czar, szata i zamknięty chwyt.</summary>
    public class MageAnimePlayTests
    {
        const string Content="Assets/TurrisDamnatorum/Content/";
        readonly List<Object> cleanup=new List<Object>();
        string profile;
        class Source:ICharacterAnimSource {public CharacterAnimState state;public CharacterAnimState GetAnimState()=>state;}
        [SetUp]public void Setup() {profile="mage_anime_"+Guid.NewGuid().ToString("N")+".json";GameRoot.ProfileFileOverride=profile;}
        [UnityTearDown]public IEnumerator Cleanup() {
            foreach(var o in cleanup)if(o!=null)Object.Destroy(o);cleanup.Clear();GameRoot.ProfileFileOverride=null;
            string p=Path.Combine(Application.persistentDataPath,profile);if(File.Exists(p))File.Delete(p);
            var main=SceneManager.GetSceneByName("Main");if(main.IsValid()&&main.isLoaded) {
                SceneManager.SetActiveScene(SceneManager.CreateScene("AfterMageAnime"));yield return SceneManager.UnloadSceneAsync(main);
            }
            yield return null;
        }
        [UnityTest]public IEnumerator Mage_StartsRealRunWithExistingWandAndGrimoire() {
            SceneManager.LoadScene("Main");yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameRoot>();var cls=root.config.classes.First(c=>c.id=="class_mage");
            var def=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Content+"Visuals/visual_mage_anime.asset");
            var old=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Content+"Visuals/visual_mage_witch.asset");
            Assert.AreSame(def,cls.Visual);Assert.AreSame(old.staffSet,def.staffSet);Assert.AreSame(old.run,def.run);
            Assert.AreEqual("visual_hero_warrior",root.config.classes.First(c=>c.id=="class_knight").Visual.name);
            Assert.IsTrue(root.TryStartRun(new LoadoutPlan{classDef=cls,difficulty=root.config.difficulties[0]},out var reason),reason);
            yield return null;yield return null;
            var visual=root.Player.GetComponent<CharacterVisual>();
            Assert.IsTrue(visual.UsesModel);Assert.IsTrue(visual.Model.GetComponentInChildren<Animator>().isHuman);
            Assert.IsNotNull(Util.FindDeep(visual.Model.transform,cls.loadouts[0].weapon.id));
            foreach(var enemy in root.Enemies)if(enemy!=null)enemy.gameObject.SetActive(false);root.enabled=false;
            for(int i=0;i<25;i++)yield return null;
            Shot(Camera.main,"game-mage.png");
            Assert.IsNotNull(root.Player.Skill(0),"Grymuar nadal daje podstawowy czar");
            bool cast=false;root.Player.RequestSkill(0);
            for(int i=0;i<90;i++) {
                cast|=root.Player.GetAnimState().action==AnimAction.Cast;
                if(i==15)Shot(Camera.main,"game-mage-cast.png");yield return null;
            }
            Assert.IsTrue(cast,"Mag wykonuje rzeczywisty czar z istniejącego grymuaru");
        }
        [UnityTest]public IEnumerator Mage_OldClipsAnimateNewMesh_WandAndStaff() {
            var def=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Content+"Visuals/visual_mage_anime.asset");
            var cls=AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content+"Classes/class_mage.asset");
            foreach(bool staff in new[]{false,true}) {
                var go=new GameObject("Mag_Pozy");cleanup.Add(go);
                var source=new Source();var visual=go.AddComponent<CharacterVisual>();visual.enabled=false;
                visual.BuildModel(def,1,new RigLook{weapon=staff?WeaponModel.Staff:AnimResolve.ForItem(cls.loadouts[0].weapon),
                    weaponItem=staff?null:cls.loadouts[0].weapon,offhandItem=staff?null:cls.loadouts[0].offHand});visual.Init(source);
                var animator=visual.Model.GetComponentInChildren<Animator>();var skin=visual.Model.GetComponentInChildren<SkinnedMeshRenderer>();
                Assert.IsTrue(animator.avatar.isValid&&animator.avatar.isHuman);Assert.Greater(skin.sharedMesh.vertexCount,1800000);
                Assert.IsTrue(skin.sharedMesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.002f));
                var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);var slot=Util.FindDeep(visual.Model.transform,"HandSlot.R");
                var slotRest=hand.InverseTransformPoint(slot.position);
                var mesh=new Mesh();cleanup.Add(mesh);var camGo=new GameObject("KameraMaga");cleanup.Add(camGo);
                // Pierwsze Evaluate grafu Humanoid aktualizuje również kompensację jednostek FBX.
                // Punktem odniesienia jest ustabilizowana pozycja idle, a nie nieoceniona hierarchia importera.
                for(int settle=0;settle<40;settle++)visual.Tick(1f/60);
                yield return null;yield return null;
                // FBX ma kompensację skali jednostek. Obie próbki muszą być po BakeMesh,
                // inaczej samo przeliczenie centymetrów na metry wygląda jak rozciągnięcie ×100.
                skin.BakeMesh(mesh);
                var rest=mesh.vertices;var triangles=skin.sharedMesh.triangles;
                var edges=new List<(int a,int b,float length)>();
                for(int i=0;i+1<triangles.Length;i+=741) {
                    int a=triangles[i],b=triangles[i+1];float length=Vector3.Distance(rest[a],rest[b]);
                    if(length>.0005f&&length<.03f)edges.Add((a,b,length));
                }
                var camera=camGo.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.15f,.17f,.21f);camera.fieldOfView=35;
                var lightGo=new GameObject("ŚwiatłoMaga");cleanup.Add(lightGo);var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.3f;
                lightGo.transform.rotation=Quaternion.Euler(40,-35,0);RenderSettings.ambientLight=new Color(.5f,.52f,.58f);
                var labels=new[]{"idle","run","cast","attack","dodge","death"};
                for(int pose=0;pose<labels.Length;pose++) {
                    source.state=pose<2?default:pose==2?new CharacterAnimState{action=AnimAction.Cast,phase=ActionPhase.Active,phaseProgress=.5f}:
                        pose==3?new CharacterAnimState{action=AnimAction.Attack,attack=AttackAnim.Auto,phase=ActionPhase.Active,phaseProgress=.5f}:
                        pose==4?new CharacterAnimState{action=AnimAction.Dodge,actionTime=.3f}:new CharacterAnimState{action=AnimAction.Death,actionTime=.7f};
                    for(int i=0;i<40;i++){if(pose==1)go.transform.position+=Vector3.forward*(4f/60);visual.Tick(1f/60);}
                    yield return null;yield return null;skin.BakeMesh(mesh);
                    Assert.Less(mesh.bounds.size.magnitude,5f,"Brak eksplozji szaty: "+labels[pose]);
                    var points=mesh.vertices;
                    Assert.IsFalse(points.Any(p=>!float.IsFinite(p.x)||!float.IsFinite(p.y)||!float.IsFinite(p.z)));
                    var stretches=edges.Select(e=>Vector3.Distance(points[e.a],points[e.b])/e.length).OrderBy(v=>v).ToArray();
                    if(stretches.Length>0) {
                        float p99=stretches[(int)((stretches.Length-1)*.99f)];
                        Debug.Log("MAGE_STRETCH "+(staff?"staff":"wand")+" "+labels[pose]+" p99="+p99+" max="+stretches.Last());
                        Assert.Less(p99,5f,"Brak masowego rozciągania krawędzi szaty: "+labels[pose]);
                    }
                    Assert.Less(Vector3.Distance(slotRest,hand.InverseTransformPoint(slot.position)),.003f,"Gniazdo w dłoni");
                    camera.transform.position=go.transform.position+new Vector3(2.6f,1.7f,4.2f);camera.transform.LookAt(go.transform.position+Vector3.up*1.05f);
                    Shot(camera,(staff?"staff-":"wand-")+labels[pose]+".png");
                    if(pose==0) {
                        camera.transform.position=slot.position+new Vector3(.3f,.15f,.45f);camera.transform.LookAt(slot.position);Shot(camera,(staff?"staff-":"wand-")+"grip.png");
                    }
                }
                Object.Destroy(go);yield return null;
            }
        }
        static void Shot(Camera camera,string name) {
            string dir=Path.Combine(Application.dataPath,"../TestResults/mage-anime");Directory.CreateDirectory(dir);
            var previous=RenderTexture.active;var target=RenderTexture.GetTemporary(1400,1000,24);var texture=new Texture2D(1400,1000,TextureFormat.RGB24,false);
            try {camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1400,1000),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(dir,name),texture.EncodeToPNG());}
            finally {camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.Destroy(texture);}
        }
    }
}
#endif
