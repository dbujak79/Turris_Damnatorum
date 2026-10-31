#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Porównanie postawy przed/po z tymi samymi klipami, kontrolą stóp i zachowaniem czarów.</summary>
    public class MagePosturePlayTests
    {
        readonly List<Object> cleanup=new List<Object>();
        class Source:ICharacterAnimSource {public CharacterAnimState state;public CharacterAnimState GetAnimState()=>state;}
        class Hero {public GameObject go;public CharacterVisual visual;public Animator animator;public Source source;}
        static string Dir {get {string p=Path.Combine(Application.dataPath,"../TestResults/mage-posture");Directory.CreateDirectory(p);return p;}}

        [UnityTearDown]public IEnumerator Cleanup() {foreach(var obj in cleanup)if(obj!=null)Object.Destroy(obj);cleanup.Clear();yield return null;}

        Hero Build(bool correction,bool staff,Vector3 position)
        {
            var original=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>("Assets/TurrisDamnatorum/Content/Visuals/visual_mage_anime.asset");
            var def=Object.Instantiate(original);def.balancedMageStance=correction;cleanup.Add(def);
            var cls=AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/class_mage.asset");
            var go=new GameObject(correction?"Mag poprawiony":"Mag przed korektą");cleanup.Add(go);go.transform.position=position;
            var visual=go.AddComponent<CharacterVisual>();visual.enabled=false;
            var source=new Source();
            visual.BuildModel(def,1,new RigLook{weapon=staff?WeaponModel.Staff:AnimResolve.ForItem(cls.loadouts[0].weapon),
                weaponItem=staff?null:cls.loadouts[0].weapon,offhandItem=staff?null:cls.loadouts[0].offHand});visual.Init(source);
            return new Hero{go=go,visual=visual,animator=visual.Model.GetComponentInChildren<Animator>(),source=source};
        }

        [UnityTest]public IEnumerator Mag_PostawaIBiodra_StopyICzaryBezZmian()
        {
            var cameraGo=new GameObject("Kamera postawy maga");cleanup.Add(cameraGo);
            var camera=cameraGo.AddComponent<Camera>();camera.enabled=false;camera.fieldOfView=32;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.18f,.20f,.25f);
            int mask=1<<28;camera.cullingMask=mask;
            var lightGo=new GameObject("Światło postawy maga");cleanup.Add(lightGo);var light=lightGo.AddComponent<Light>();
            light.type=LightType.Directional;light.intensity=1.3f;lightGo.transform.rotation=Quaternion.Euler(35,-30,0);
            var report=new System.Text.StringBuilder("broń\tpoza\twariant\tpochylenie_stopnie\tbiodra_wzgledem_stop_m\twysokosc_bioder_m\n");
            foreach(bool staff in new[]{false,true}) {
                var before=Build(false,staff,Vector3.zero);var after=Build(true,staff,Vector3.right*4);
                foreach(var hero in new[]{before,after})foreach(var t in hero.visual.Model.GetComponentsInChildren<Transform>())t.gameObject.layer=28;
                var labels=new[]{"idle","walk","run","cast","attack","dodge"};
                for(int pose=0;pose<labels.Length;pose++) {
                    var state=pose<3?default:pose==3?new CharacterAnimState{action=AnimAction.Cast,phase=ActionPhase.Active,phaseProgress=.5f}:
                        pose==4?new CharacterAnimState{action=AnimAction.Attack,phase=ActionPhase.Active,phaseProgress=.5f}:
                        new CharacterAnimState{action=AnimAction.Dodge,actionTime=.3f};
                    before.source.state=after.source.state=state;
                    float speed=pose==1?1.4f:pose==2?3.87f:0;
                    for(int i=0;i<60;i++)foreach(var hero in new[]{before,after}) {
                        hero.go.transform.position+=Vector3.forward*(speed/60);
                        hero.visual.Tick(1f/60);
                    }
                    yield return null;yield return null;
                    foreach(var hero in new[]{before,after}) {
                        var hips=hero.animator.GetBoneTransform(HumanBodyBones.Hips);
                        var neck=hero.animator.GetBoneTransform(HumanBodyBones.Neck);
                        var support=(hero.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position+hero.animator.GetBoneTransform(HumanBodyBones.RightFoot).position)*.5f;
                        var axis=neck.position-hips.position;
                        float pitch=Mathf.Atan2(axis.z,axis.y)*Mathf.Rad2Deg;
                        report.AppendLine((staff?"staff":"wand")+"\t"+labels[pose]+"\t"+(hero==after?"after":"before")+"\t"+pitch+"\t"+(hips.position.z-support.z)+"\t"+(hips.position.y-support.y));
                        File.WriteAllText(Path.Combine(Dir,"posture.tsv"),report.ToString());
                        if(hero==after&&pose==0) {
                            Assert.Less(pitch,12,"Spokojna postawa bez garbienia");
                            Assert.Greater(hips.position.z-support.z,-.12f,"Biodra nie zostają daleko za stopami");
                        }
                        foreach(string view in new[]{"side","front","back"}) {
                            Vector3 center=hero.go.transform.position+Vector3.up*1.0f;
                            camera.transform.position=center+(view=="side"?new Vector3(4.2f,.15f,0):view=="front"?new Vector3(1.3f,.3f,4.2f):new Vector3(-1.3f,.3f,-4.2f));
                            camera.transform.LookAt(center);
                            // Drugi aktor jest poza kadrem i dodatkowo wyłączony na czas ujęcia.
                            var other=hero==before?after:before;
                            var hidden=other.visual.Model.GetComponentsInChildren<Renderer>();
                            foreach(var renderer in hidden)renderer.forceRenderingOff=true;
                            Shot(camera,(staff?"staff":"wand")+"-"+labels[pose]+"-"+view+"-"+(hero==after?"after":"before")+".png");
                            foreach(var renderer in hidden)renderer.forceRenderingOff=false;
                        }
                    }
                    if(pose==0||pose>=3)foreach(var bone in new[]{HumanBodyBones.LeftFoot,HumanBodyBones.RightFoot}) {
                        Vector3 a=before.go.transform.InverseTransformPoint(before.animator.GetBoneTransform(bone).position);
                        Vector3 b=after.go.transform.InverseTransformPoint(after.animator.GetBoneTransform(bone).position);
                        Assert.Less(Vector3.Distance(a,b),.006f,"Zachowana pozycja stopy: "+labels[pose]+" "+bone);
                    }
                    if(pose>=3)foreach(var bone in new[]{HumanBodyBones.Hips,HumanBodyBones.Neck,HumanBodyBones.RightHand}) {
                        Vector3 a=before.go.transform.InverseTransformPoint(before.animator.GetBoneTransform(bone).position);
                        Vector3 b=after.go.transform.InverseTransformPoint(after.animator.GetBoneTransform(bone).position);
                        Assert.Less(Vector3.Distance(a,b),.002f,"Akcja zachowuje pozę klipu: "+labels[pose]+" "+bone);
                    }
                    File.WriteAllText(Path.Combine(Dir,"posture.tsv"),report.ToString());
                }
                Object.Destroy(before.go);Object.Destroy(after.go);yield return null;
            }
            Debug.Log("MAGE_POSTURE_OK "+report);
        }
        static void Shot(Camera camera,string name) {
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            var target=RenderTexture.GetTemporary(1200,1000,24);var image=new Texture2D(1200,1000,TextureFormat.RGB24,false);
            try {camera.targetTexture=target;camera.Render();camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1200,1000),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Dir,name),image.EncodeToPNG());}
            finally {camera.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(target);Object.Destroy(image);}
        }
    }
}
#endif
