#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Porównanie rozwiązania IK i naprawy stóp na aktywnych rigach.</summary>
    public class RetargetFootRepairPlayTests
    {
        readonly List<Object> cleanup=new List<Object>();
        const string Dir="TestResults/hero-feet-repair";
        class Source:ICharacterAnimSource{public CharacterAnimState state;public CharacterAnimState GetAnimState()=>state;}
        [SetUp]public void Setup(){if(TestContext.CurrentContext.Test.Name.StartsWith("A_")&&!SessionState.GetBool("Turris.FeetRepair.Run",false))Assert.Ignore("Porównanie diagnostyczne uruchamiane z menu naprawy stóp.");Directory.CreateDirectory(Dir);}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var x in cleanup)if(x!=null)Object.Destroy(x);cleanup.Clear();yield return null;}
        static string N(float x)=>float.IsFinite(x)?x.ToString("G7",CultureInfo.InvariantCulture):"NaN";
        static float Twist(Animator anim,bool left,out float bend){Transform B(HumanBodyBones l,HumanBodyBones r)=>anim.GetBoneTransform(left?l:r);var hip=B(HumanBodyBones.LeftUpperLeg,HumanBodyBones.RightUpperLeg).position;var knee=B(HumanBodyBones.LeftLowerLeg,HumanBodyBones.RightLowerLeg).position;var ankle=B(HumanBodyBones.LeftFoot,HumanBodyBones.RightFoot).position;var toe=B(HumanBodyBones.LeftToes,HumanBodyBones.RightToes).position;bend=180-Vector3.Angle(hip-knee,ankle-knee);var shin=(knee-ankle).normalized;var plane=Vector3.ProjectOnPlane(knee-hip,shin);var foot=Vector3.ProjectOnPlane(toe-ankle,shin);return bend<25||plane.sqrMagnitude<1e-6f||foot.sqrMagnitude<1e-6f?float.NaN:Vector3.SignedAngle(plane,foot,shin);}
        static void Shot(Camera cam,string path){var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(900,900,24);var image=new Texture2D(900,900,TextureFormat.RGB24,false);try{cam.targetTexture=rt;cam.Render();cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,900,900),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}finally{cam.targetTexture=null;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);}}
        [UnityTest]public IEnumerator A_ZrodloIK_Porownanie()
        {
            var cg=new GameObject("Kamera stóp");cleanup.Add(cg);var cam=cg.AddComponent<Camera>();cam.enabled=false;cam.fieldOfView=32;cam.cullingMask=1<<28;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.17f,.19f,.23f);
            var lg=new GameObject("Światło stóp");cleanup.Add(lg);var light=lg.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.3f;lg.transform.rotation=Quaternion.Euler(35,-30,0);
            using(var w=new StreamWriter(Dir+"/ik-comparison.csv")) {
                w.WriteLine("class,variant,frame,phase,side,bend,twist,ankle_y,yaw,pitch");
                foreach(string id in new[]{"class_hu_berserker","class_mage","class_hu_archer","class_hu_paladin","class_knight"})foreach(string variant in new[]{"current","anatomical"}) {
                    var cls=AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/"+id+".asset");var kit=cls.loadouts[0];
                    var definition=Object.Instantiate(cls.Visual);cleanup.Add(definition);definition.anatomicalFeet=variant=="anatomical"&&id!="class_knight";
                    var go=new GameObject("Próba "+id);cleanup.Add(go);var visual=go.AddComponent<CharacterVisual>();visual.enabled=false;visual.BuildModel(definition,1,new RigLook{weapon=AnimResolve.ForItem(kit.weapon),weaponItem=kit.weapon,offhandItem=kit.offHand,shield=AnimResolve.ForShield(kit.offHand)});visual.Init(new Source());
                    foreach(var t in visual.Model.GetComponentsInChildren<Transform>())t.gameObject.layer=28;
                    var anim=visual.Model.GetComponentInChildren<Animator>();var driver=(ClipAnimationDriver)visual.Driver;var type=typeof(ClipAnimationDriver);
                    var clips=(AnimationClipPlayable[])type.GetField("locoClips",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(driver);
                    if(variant.StartsWith("no-foot-ik"))foreach(var clip in clips)clip.SetApplyFootIK(false);
                    var cache=type.GetField("footYawCache",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null) as System.Collections.IDictionary;cache.Clear();
                    var corrections=(Vector2[])type.GetField("footYawFix",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(driver);
                    float speed=visual.ModelSet!=null?visual.ModelSet.runSpeed:cls.Visual.runSpeed;
                    for(int frame=0;frame<360;frame++) {
                        if(variant.EndsWith("no-yaw"))Array.Clear(corrections,0,corrections.Length);
                        go.transform.position+=Vector3.forward*(speed/60);visual.Tick(1f/60);driver.ConsumeAdvance();
                        if(frame<120)continue;
                        foreach(bool left in new[]{true,false}){float twist=Twist(anim,left,out float bend);var ankle=anim.GetBoneTransform(left?HumanBodyBones.LeftFoot:HumanBodyBones.RightFoot);var toe=anim.GetBoneTransform(left?HumanBodyBones.LeftToes:HumanBodyBones.RightToes);var dir=toe.position-ankle.position;w.WriteLine(id+","+variant+","+frame+","+N(driver.AuditStepPhase)+","+(left?"L":"R")+","+N(bend)+","+N(twist)+","+N(ankle.position.y)+","+N(Mathf.Atan2(dir.x,dir.z)*Mathf.Rad2Deg)+","+N(Mathf.Atan2(dir.y,new Vector2(dir.x,dir.z).magnitude)*Mathf.Rad2Deg));}
                        if(frame%60==0){yield return null;var center=(anim.GetBoneTransform(HumanBodyBones.LeftFoot).position+anim.GetBoneTransform(HumanBodyBones.RightFoot).position)*.5f+Vector3.up*.05f;cam.transform.position=center+Vector3.forward*2.2f+Vector3.up*.15f;cam.transform.LookAt(center);Shot(cam,Dir+"/"+id+"-"+variant+"-"+frame+".png");}
                    }
                    w.Flush();Object.Destroy(go);yield return null;
                }
            }
        }
        [UnityTest]public IEnumerator B_AktywneRigi_SkretICiaglosc()
        {
            var ids=new[]{"class_hu_berserker","class_hu_dual","class_hu_battlemage","class_rogue","class_hu_archer","class_hu_arcane_archer","class_hu_assassin","class_hu_mage_fire","class_hu_mage_water","class_hu_mage_air","class_hu_mage_earth","class_hu_summoner","class_mage","class_hu_paladin"};
            using(var report=new StreamWriter(Dir+"/hinge-regression.csv")) {
                report.WriteLine("class,fps,side,swing_samples,max_twist,max_rotation_per_frame");
                foreach(string id in ids)foreach(int fps in new[]{60,30}) {
                    var cls=AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/"+id+".asset");Assert.IsTrue(cls.Visual.anatomicalFeet,id);
                    var go=new GameObject(id+" test kostek");cleanup.Add(go);var visual=go.AddComponent<CharacterVisual>();visual.enabled=false;var kit=cls.loadouts[0];visual.BuildModel(cls.Visual,1,new RigLook{weapon=AnimResolve.ForItem(kit.weapon),weaponItem=kit.weapon,offhandItem=kit.offHand,shield=AnimResolve.ForShield(kit.offHand)});visual.Init(new Source());
                    var anim=visual.Model.GetComponentInChildren<Animator>();var driver=(ClipAnimationDriver)visual.Driver;float speed=visual.ModelSet!=null?visual.ModelSet.runSpeed:cls.Visual.runSpeed;
                    var feet=new[]{anim.GetBoneTransform(HumanBodyBones.LeftFoot),anim.GetBoneTransform(HumanBodyBones.RightFoot)};var previous=new[]{feet[0].rotation,feet[1].rotation};var max=new float[2];var jumps=new float[2];var counts=new int[2];
                    yield return null;
                    for(int frame=0;frame<fps*6;frame++) {
                        go.transform.position+=Vector3.forward*(speed/fps);visual.Tick(1f/fps);driver.ConsumeAdvance();
                        for(int s=0;s<2;s++){float delta=Quaternion.Angle(previous[s],feet[s].rotation);previous[s]=feet[s].rotation;if(frame<fps*2)continue;jumps[s]=Mathf.Max(jumps[s],delta);float twist=Twist(anim,s==0,out float bend);if(bend>60&&float.IsFinite(twist)){counts[s]++;max[s]=Mathf.Max(max[s],Mathf.Abs(twist));}}
                        if(frame%fps==0)yield return null;
                    }
                    for(int s=0;s<2;s++){report.WriteLine(id+","+fps+","+(s==0?"L":"R")+","+counts[s]+","+N(max[s])+","+N(jumps[s]));Assert.Greater(counts[s],3,id+": nie pomijaj brakującej nogi");Assert.Less(max[s],18,id+": skręt kostki "+(s==0?"L":"R"));Assert.Less(jumps[s],fps==60?55:90,id+": ciągłość obrotu stopy");}
                    report.Flush();Object.Destroy(go);yield return null;
                }
            }
        }
    }
}
#endif
