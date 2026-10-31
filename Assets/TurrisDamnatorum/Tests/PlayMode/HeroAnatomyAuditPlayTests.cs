#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Audyt opt-in. Wynik testu potwierdza zebranie danych, nie zdrowe kostki.</summary>
    public class HeroAnatomyAuditPlayTests
    {
        const string Content="Assets/TurrisDamnatorum/Content/";
        public const string EnabledKey="Turris.AnatomyAudit.Enabled";
        static readonly string[] ClassIds={"class_hu_berserker","class_hu_dual","class_hu_battlemage","class_rogue","class_hu_archer","class_hu_arcane_archer","class_hu_assassin","class_hu_mage_fire","class_hu_mage_water","class_hu_mage_air","class_hu_mage_earth","class_hu_summoner","class_mage","class_knight","class_hu_paladin"};
        readonly List<Object> cleanup=new List<Object>();
        static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,SessionState.GetBool("Turris.FeetRepair.Run",false)?"../TestResults/hero-feet-repair/after":"../TestResults/hero-anatomy"));
        Camera camera;
        string tempProfile;
        class Source:ICharacterAnimSource {public CharacterAnimState state;public CharacterAnimState GetAnimState()=>state;}
        class Forward:IPlayerDriver {public Vector3 MoveWorld=>Vector3.forward;public bool BlockHeld=>false;public bool SprintHeld=>true;}
        class Hero {public GameObject go;public CharacterVisual visual;public Animator anim;public ClipAnimationDriver driver;public Source source;public Foot[] feet;}
        class Foot {public Transform hip,knee,ankle,toe;public float restYaw;public Quaternion restRotation;public Vector3 restForward,restUp;public Marker[] markers;public SkinnedMeshRenderer skin;public Transform[] bones;public Matrix4x4[] bindposes;public float bakeError;public int skinMode;}
        struct Marker {public int index;public Vector3 local;public BoneWeight weight;public BoneWeight1[] influences;}
        [Serializable]class Manifest {public string classId,visual,prefab,avatar,profile,weapon,offhand,set,flags;public float walkSpeed,runSpeed;public string[] clips;public int[] footMarkers;public float[] skinValidationError;}
        static string N(float x)=>float.IsFinite(x)?x.ToString("G7",CultureInfo.InvariantCulture):"NaN";
        static string V(Vector3 v)=>N(v.x)+","+N(v.y)+","+N(v.z);
        static string Safe(string s)=>string.Concat(s.Select(c=>char.IsLetterOrDigit(c)||c=='-'||c=='_'?c:'_'));
        [SetUp]public void Setup() {if(!SessionState.GetBool(EnabledKey,false)&&!SessionState.GetBool("Turris.FeetRepair.Run",false))Assert.Ignore("Audyt ręczny: Turris/Audyt anatomii/Uruchom pełny audyt");Directory.CreateDirectory(Root);}
        [UnityTearDown]public IEnumerator Cleanup() {
            foreach(var obj in cleanup)if(obj!=null)Object.Destroy(obj);cleanup.Clear();Time.captureFramerate=0;
            if(tempProfile!=null) {GameRoot.ProfileFileOverride=null;string p=Path.Combine(Application.persistentDataPath,tempProfile);if(File.Exists(p))File.Delete(p);}
            var main=SceneManager.GetSceneByName("Main");if(main.IsValid()&&main.isLoaded) {SceneManager.SetActiveScene(SceneManager.CreateScene("PoAudycieAnatomii"));yield return SceneManager.UnloadSceneAsync(main);}
            yield return null;
        }
        Hero Build(CharacterVisualDefinition def,RigLook look) {
            var go=new GameObject("Audyt "+def.name);cleanup.Add(go);var visual=go.AddComponent<CharacterVisual>();visual.enabled=false;
            visual.BuildModel(def,1,look);var source=new Source();visual.Init(source);
            var h=new Hero{go=go,visual=visual,source=source,anim=visual.Model.GetComponentInChildren<Animator>(),driver=visual.Driver as ClipAnimationDriver};
            Assert.IsNotNull(h.driver);Assert.IsTrue(h.anim.isHuman&&h.anim.avatar.isValid,def.name);
            h.feet=new[]{FootFor(h,true),FootFor(h,false)};
            foreach(var t in visual.Model.GetComponentsInChildren<Transform>())t.gameObject.layer=28;
            return h;
        }
        Foot FootFor(Hero h,bool left) {
            Transform Bone(HumanBodyBones a,HumanBodyBones b)=>h.anim.GetBoneTransform(left?a:b);
            var f=new Foot{hip=Bone(HumanBodyBones.LeftUpperLeg,HumanBodyBones.RightUpperLeg),knee=Bone(HumanBodyBones.LeftLowerLeg,HumanBodyBones.RightLowerLeg),ankle=Bone(HumanBodyBones.LeftFoot,HumanBodyBones.RightFoot),toe=Bone(HumanBodyBones.LeftToes,HumanBodyBones.RightToes)};
            Assert.IsNotNull(f.toe,"Brak palców — wymagana osobna ocena siatki");
            f.restForward=f.ankle.InverseTransformDirection(f.toe.position-f.ankle.position);f.restRotation=f.ankle.rotation;
            f.restUp=f.ankle.InverseTransformDirection(h.go.transform.up);
            f.restYaw=Yaw(h.go.transform.InverseTransformDirection(f.toe.position-f.ankle.position));
            var candidates=new List<(SkinnedMeshRenderer skin,List<Marker> markers)>();
            foreach(var skin in h.visual.Model.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                int fi=Array.IndexOf(skin.bones,f.ankle),ti=Array.IndexOf(skin.bones,f.toe);if(fi<0)continue;
                var weights=skin.sharedMesh.boneWeights;Vector3[] vertices;
                if(skin.sharedMesh.isReadable)vertices=skin.sharedMesh.vertices;
                else {using(var data=MeshUtility.AcquireReadOnlyMeshData(skin.sharedMesh))using(var positions=new NativeArray<Vector3>(skin.sharedMesh.vertexCount,Allocator.Temp)){data[0].GetVertices(positions);vertices=positions.ToArray();}}
                var points=new List<Marker>();
                for(int i=0;i<weights.Length;i++) {var w=weights[i];float total=0;for(int j=0;j<4;j++) {int b=Index(w,j);if(b==fi||b==ti)total+=Weight(w,j);}if(total>.65f)points.Add(new Marker{index=i,local=vertices[i],weight=w});}
                if(points.Count>0)candidates.Add((skin,points));
            }
            if(candidates.Count==0){f.markers=Array.Empty<Marker>();return f;}
            var best=candidates.OrderByDescending(x=>x.markers.Count).First();f.skin=best.skin;f.bones=f.skin.bones;f.bindposes=f.skin.sharedMesh.bindposes;
            var bake=new Mesh();f.skin.BakeMesh(bake);var baked=bake.vertices;
            float Height(Marker m)=>f.skin.transform.TransformPoint(baked[m.index]).y;
            float min=best.markers.Min(Height),max=best.markers.Max(Height);
            var sole=best.markers.Where(m=>Height(m)<min+Mathf.Max(.018f,(max-min)*.09f)).ToList();
            f.markers=sole.Where((m,i)=>i%Mathf.Max(1,sole.Count/48)==0).Take(64).ToArray();
            using(var counts=f.skin.sharedMesh.GetBonesPerVertex())using(var all=f.skin.sharedMesh.GetAllBoneWeights()) {
                var offsets=new int[counts.Length];int offset=0;for(int i=0;i<counts.Length;i++){offsets[i]=offset;offset+=counts[i];}
                for(int i=0;i<f.markers.Length;i++){var m=f.markers[i];m.influences=new BoneWeight1[counts[m.index]];for(int j=0;j<m.influences.Length;j++)m.influences[j]=all[offsets[m.index]+j];f.markers[i]=m;}
            }
            // BakeMesh stanowi kontrolę niezależną od sposobu obcięcia/normalizacji wag renderera.
            float bestError=float.PositiveInfinity;int bestMode=0;
            for(int mode=0;mode<3;mode++){f.skinMode=mode;float error=f.markers.Select(m=>Vector3.Distance(Skin(f,m),f.skin.transform.TransformPoint(baked[m.index]))).DefaultIfEmpty(float.PositiveInfinity).Max();if(error<bestError){bestError=error;bestMode=mode;}}
            f.skinMode=bestMode;f.bakeError=bestError;Object.Destroy(bake);
            return f;
        }
        static int Index(BoneWeight w,int i)=>i==0?w.boneIndex0:i==1?w.boneIndex1:i==2?w.boneIndex2:w.boneIndex3;
        static float Weight(BoneWeight w,int i)=>i==0?w.weight0:i==1?w.weight1:i==2?w.weight2:w.weight3;
        static Vector3 Skin(Foot f,Marker m) {Vector3 p=Vector3.zero;float sum=0;if(f.skinMode==2&&m.influences!=null){foreach(var w in m.influences){p+=f.bones[w.boneIndex].localToWorldMatrix.MultiplyPoint3x4(f.bindposes[w.boneIndex].MultiplyPoint3x4(m.local))*w.weight;}return p;}for(int j=0;j<4;j++){float w=Weight(m.weight,j);if(w<=0)continue;int b=Index(m.weight,j);p+=f.bones[b].localToWorldMatrix.MultiplyPoint3x4(f.bindposes[b].MultiplyPoint3x4(m.local))*w;sum+=w;}return f.skinMode==1&&sum>0?p/sum:p;}
        static float Yaw(Vector3 v)=>Mathf.Atan2(v.x,v.z)*Mathf.Rad2Deg;
        static float Bend(Foot f)=>180-Vector3.Angle(f.hip.position-f.knee.position,f.ankle.position-f.knee.position);
        static float Twist(Foot f) {if(Bend(f)<25)return float.NaN;Vector3 shin=(f.knee.position-f.ankle.position).normalized;Vector3 plane=Vector3.ProjectOnPlane(f.knee.position-f.hip.position,shin);Vector3 forward=Vector3.ProjectOnPlane(f.toe.position-f.ankle.position,shin);if(plane.sqrMagnitude<1e-6f||forward.sqrMagnitude<1e-6f)return float.NaN;return Vector3.SignedAngle(plane,forward,shin);}
        void Tick(Hero h,Vector3 local,float dt,CharacterAnimState state=default) {h.source.state=state;h.go.transform.position+=h.go.transform.TransformDirection(local)*dt;h.visual.Tick(dt);h.go.transform.position+=h.go.transform.forward*h.driver.ConsumeAdvance();}
        void Row(StreamWriter w,Hero h,string stage,int frame,float dt) {
            for(int i=0;i<2;i++) {var f=h.feet[i];var dir=h.go.transform.InverseTransformDirection(f.toe.position-f.ankle.position);var hips=h.anim.GetBoneTransform(HumanBodyBones.Hips);var neck=h.anim.GetBoneTransform(HumanBodyBones.Neck);Vector3 spine=h.go.transform.InverseTransformDirection(neck.position-hips.position);
                Vector3 sole=Vector3.zero;float min=float.NaN,max=float.NaN;
                if(stage=="final"&&f.markers.Length>0&&f.bakeError<.003f) {min=float.PositiveInfinity;max=float.NegativeInfinity;foreach(var m in f.markers){var p=Skin(f,m);sole+=p;min=Mathf.Min(min,p.y);max=Mathf.Max(max,p.y);}sole/=f.markers.Length;}
                Vector3 up=h.go.transform.InverseTransformDirection(f.ankle.TransformDirection(f.restUp));
                w.WriteLine(frame+","+N(frame*dt)+","+N(h.driver.AuditStepPhase)+","+stage+","+(i==0?"L":"R")+","+V(f.ankle.position)+","+V(h.go.transform.InverseTransformPoint(f.ankle.position))+","+V(f.toe.position)+","+N(Yaw(dir))+","+N(Mathf.DeltaAngle(f.restYaw,Yaw(dir)))+","+N(Mathf.Atan2(dir.y,new Vector2(dir.x,dir.z).magnitude)*Mathf.Rad2Deg)+","+N(Bend(f))+","+N(Twist(f))+","+N(min)+","+N(max)+","+V(sole)+","+N(Quaternion.Angle(f.restRotation,f.ankle.rotation))+","+N(Mathf.Atan2(spine.z,spine.y)*Mathf.Rad2Deg)+","+N(Mathf.Atan2(up.x,up.y)*Mathf.Rad2Deg));
            }
        }
        const string Header="frame,time,phase,stage,side,world_x,world_y,world_z,local_x,local_y,local_z,toe_x,toe_y,toe_z,yaw,rest_delta,pitch,knee_bend,knee_twist,sole_min,sole_max,sole_x,sole_y,sole_z,rotation_delta,torso_pitch,foot_roll";
        static RigLook Look(StartingLoadout kit)=>new RigLook{weapon=AnimResolve.ForItem(kit.weapon),weaponItem=kit.weapon,offhandItem=kit.offHand,shield=AnimResolve.ForShield(kit.offHand)};
        void Studio() {
            var camGo=new GameObject("Kamera audytu");cleanup.Add(camGo);camera=camGo.AddComponent<Camera>();camera.enabled=false;camera.fieldOfView=30;camera.cullingMask=1<<28;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.19f,.21f,.25f);
            var lightGo=new GameObject("Światło audytu");cleanup.Add(lightGo);var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.3f;lightGo.transform.rotation=Quaternion.Euler(40,-35,0);
            var plane=GameObject.CreatePrimitive(PrimitiveType.Cube);cleanup.Add(plane);plane.name="Płaszczyzna kontrolna y=0";plane.layer=28;plane.transform.position=Vector3.down*.025f;plane.transform.localScale=new Vector3(200,.05f,200);
        }
        void Shot(Hero h,string path,bool close,string view,bool skeleton=false) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));Vector3 center=close?(h.feet[0].ankle.position+h.feet[1].ankle.position)*.5f+Vector3.up*.06f:h.go.transform.position+Vector3.up*1f;float distance=close?2.2f:4.4f;
            Vector3 offset=view=="front"?Vector3.forward:view=="back"?Vector3.back:view=="left"?Vector3.left:Vector3.right;
            camera.transform.position=center+offset*distance+Vector3.up*(close?.12f:.15f);camera.transform.LookAt(center);
            var lines=new List<GameObject>();
            if(skeleton)foreach(var f in h.feet) {foreach(var pair in new[]{(f.hip,f.knee),(f.knee,f.ankle),(f.ankle,f.toe)}) {var go=new GameObject("Kość");go.layer=28;var line=go.AddComponent<LineRenderer>();line.material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));line.material.color=f==h.feet[0]?Color.cyan:Color.magenta;line.startWidth=line.endWidth=.012f;line.positionCount=2;line.SetPositions(new[]{pair.Item1.position,pair.Item2.position});lines.Add(go);}}
            Capture(camera,path,768,768);foreach(var line in lines){Object.Destroy(line.GetComponent<LineRenderer>().material);Object.DestroyImmediate(line);}
        }
        static void Capture(Camera cam,string path,int width,int height) {var previous=cam.targetTexture;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);try {cam.targetTexture=rt;cam.Render();cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}finally{cam.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}}
        void IsolatedFeet(Hero h,string folder,int phase) {
            var hidden=h.visual.Model.GetComponentsInChildren<Renderer>();foreach(var r in hidden)r.forceRenderingOff=true;
            var objects=new List<GameObject>();var meshes=new List<Mesh>();
            try {
                foreach(var skin in h.feet.Select(f=>f.skin).Where(s=>s!=null).Distinct()) {
                    var b=new Mesh();skin.BakeMesh(b);meshes.Add(b);var vertices=b.vertices;
                    foreach(var f in h.feet.Where(f=>f.skin==skin)) {float error=f.markers.Select(m=>Vector3.Distance(Skin(f,m),skin.transform.TransformPoint(vertices[m.index]))).DefaultIfEmpty(float.NaN).Max();File.AppendAllText(Path.Combine(folder,"bake-validation.csv"),phase+","+(f==h.feet[0]?"L":"R")+","+f.skinMode+","+N(error)+"\n");}
                    var ids=new HashSet<int>();foreach(var f in h.feet.Where(f=>f.skin==skin))foreach(var bone in new[]{f.knee,f.ankle,f.toe})ids.Add(Array.IndexOf(skin.bones,bone));
                    var weights=skin.sharedMesh.boneWeights;var selected=new bool[weights.Length];for(int i=0;i<weights.Length;i++){float total=0;for(int j=0;j<4;j++)if(ids.Contains(Index(weights[i],j)))total+=Weight(weights[i],j);selected[i]=total>.55f;}
                    for(int sub=0;sub<b.subMeshCount;sub++){var indices=b.GetTriangles(sub);var result=new List<int>();for(int i=0;i<indices.Length;i+=3)if(selected[indices[i]]&&selected[indices[i+1]]&&selected[indices[i+2]]){result.Add(indices[i]);result.Add(indices[i+1]);result.Add(indices[i+2]);}b.SetTriangles(result,sub,false);}
                    var go=new GameObject("Diagnostyczny wycinek nóg — bez zapisu assetu");objects.Add(go);go.layer=28;go.transform.SetParent(skin.transform.parent,false);go.transform.SetLocalPositionAndRotation(skin.transform.localPosition,skin.transform.localRotation);go.transform.localScale=skin.transform.localScale;
                    go.AddComponent<MeshFilter>().sharedMesh=b;go.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;
                }
                Shot(h,Path.Combine(folder,$"p{phase}-isolated-front.png"),true,"front");Shot(h,Path.Combine(folder,$"p{phase}-isolated-side.png"),true,"left");
                foreach(var go in objects)go.SetActive(false);Shot(h,Path.Combine(folder,$"p{phase}-bones-only.png"),true,"front",true);
            }finally{foreach(var go in objects)Object.DestroyImmediate(go);foreach(var mesh in meshes)Object.DestroyImmediate(mesh);foreach(var r in hidden)r.forceRenderingOff=false;}
        }
        IEnumerator Case(Hero h,string dir,string name,Vector3 velocity,float dt,bool pictures,bool stages=true,bool dense=false) {
            string folder=Path.Combine(dir,name+(dt>.02f?"-30fps":"-60fps"));Directory.CreateDirectory(folder);
            for(int i=0;i<120;i++)Tick(h,velocity,dt);
            int frame=0,wraps=0;float prev=h.driver.AuditStepPhase;int[] shotAt={-1,-1,-1,-1};
            using(var w=new StreamWriter(Path.Combine(folder,"frames.csv"),false,Encoding.UTF8)) {
                w.WriteLine(Header);
                if(stages)h.driver.PoseAudit=(stage,anim)=>{if(dense||frame%4==0)Row(w,h,stage,frame,dt);};
                int limit=Mathf.CeilToInt(20/dt),minimum=Mathf.CeilToInt(3/dt);
                while(frame<limit&&(velocity.sqrMagnitude>.01f?wraps<4:frame<minimum)) {
                    Tick(h,velocity,dt);Row(w,h,"final",frame,dt);float phase=h.driver.AuditStepPhase;if(phase<prev-.5f)wraps++;prev=phase;
                    int quarter=Mathf.Min(3,(int)(phase*4));
                    if(dense&&wraps==2){yield return null;Shot(h,Path.Combine(folder,$"sequence-front-{frame:0000}.png"),true,"front");Shot(h,Path.Combine(folder,$"sequence-side-{frame:0000}.png"),true,"left");}
                    if(pictures&&wraps==2&&shotAt[quarter]<0) {shotAt[quarter]=frame;yield return null;foreach(string view in new[]{"front","back","left","right"}) {Shot(h,Path.Combine(folder,$"p{quarter}-{view}.png"),false,view);Shot(h,Path.Combine(folder,$"p{quarter}-{view}-feet.png"),true,view);}
                        Shot(h,Path.Combine(folder,$"p{quarter}-bones.png"),true,"front",true);if(dense)IsolatedFeet(h,folder,quarter);}
                    frame++;if(frame%120==0){w.Flush();yield return null;}
                }
                h.driver.PoseAudit=null;
                if(pictures&&velocity.sqrMagnitude<.01f){yield return null;Shot(h,Path.Combine(folder,"idle-front.png"),false,"front");Shot(h,Path.Combine(folder,"idle-feet.png"),true,"front");}
            }
            File.WriteAllText(Path.Combine(folder,"sampling.json"),"{\"frames\":"+frame+",\"wraps\":"+wraps+",\"dt\":"+N(dt)+",\"minimumCompleteCycles\":"+Mathf.Max(0,wraps-1)+"}");
            Assert.Greater(frame,25,name);if(velocity.sqrMagnitude>.01f)Assert.GreaterOrEqual(wraps,4,name+": 3 pełne cykle");
        }
        IEnumerator Actions(Hero h,string dir) {
            string folder=Path.Combine(dir,"actions-transitions");Directory.CreateDirectory(folder);int frame=0;
            using(var w=new StreamWriter(Path.Combine(folder,"frames.csv"),false,Encoding.UTF8)) {
                w.WriteLine(Header);
                void Step(Vector3 v,CharacterAnimState s=default) {Tick(h,v,1f/60,s);Row(w,h,"final",frame++,1f/60);}
                // Początek ruchu, zatrzymanie, zwroty 90/180, okrążanie i odwrócenie kierunku.
                for(int i=0;i<600;i++) {Vector3 v=i<60?Vector3.zero:i<180?Vector3.forward*3.9f:i<240?Vector3.zero:i<300?Vector3.right*3.9f:i<360?Vector3.back*3.9f:i<480?Vector3.left*3.9f:Vector3.forward*3.9f;
                    if(i==240||i==300)h.go.transform.Rotate(0,i==240?90:180,0);if(i>=360&&i<480)h.go.transform.Rotate(0,.8f,0);Step(v);if(i%120==0)yield return null;}
                foreach(var attack in new[]{AttackAnim.SlashRight,AttackAnim.SlashLeft,AttackAnim.Thrust,AttackAnim.Overhead,AttackAnim.BowDraw}) {
                    foreach(var phase in new[]{ActionPhase.Startup,ActionPhase.Active,ActionPhase.Recovery}) {int count=phase==ActionPhase.Active?12:24;for(int i=0;i<count;i++)Step(Vector3.zero,new CharacterAnimState{action=AnimAction.Attack,attack=attack,phase=phase,phaseProgress=(i+1f)/count,actionTime=i/60f,windupSeconds=.4f,activeSeconds=.2f,hasShield=true});}
                    yield return null;Shot(h,Path.Combine(folder,"attack-"+attack+".png"),true,"front");
                }
                foreach(var action in new[]{AnimAction.Block,AnimAction.Parry,AnimAction.Cast,AnimAction.Dodge,AnimAction.Backstep,AnimAction.Flinch}) {
                    for(int i=0;i<60;i++)Step(Vector3.zero,new CharacterAnimState{action=action,phase=i<20?ActionPhase.Startup:i<35?ActionPhase.Active:ActionPhase.Recovery,phaseProgress=(i%20)/20f,actionTime=i/60f,actionDuration=1,rollDuration=.65f,dodgeDirection=Vector3.forward,hasShield=true});
                    for(int i=0;i<45;i++)Step(Vector3.forward*3.9f);yield return null;
                }
                for(int i=0;i<240;i++)Step(Vector3.zero);
            }
        }
        IEnumerable<(string name,RigLook look)> Profiles(ClassDefinition cls) {
            for(int i=0;i<cls.loadouts.Count;i++)yield return ("native-"+i,Look(cls.loadouts[i]));
            yield return ("profile-onehand",new RigLook{weapon=WeaponModel.Sword});yield return ("profile-shield",new RigLook{weapon=WeaponModel.Sword,shield=ShieldModel.Heater});
            yield return ("profile-twohand",new RigLook{weapon=WeaponModel.GreatSword});yield return ("profile-bow",new RigLook{weapon=WeaponModel.Bow});yield return ("profile-staff",new RigLook{weapon=WeaponModel.Staff});
            yield return ("profile-dual",new RigLook{weapon=WeaponModel.Knives});
        }
        IEnumerator Sweep(string[] ids) {
            Studio();
            foreach(string id in ids) {
                var cls=AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content+"Classes/"+id+".asset");Assert.IsNotNull(cls,id);Assert.IsNotNull(cls.Visual,id);
                foreach(var profile in Profiles(cls)) {
                    var h=Build(cls.Visual,profile.look);var set=h.visual.ModelSet;float walk=set!=null?set.walkSpeed:cls.Visual.walkSpeed,run=set!=null?set.runSpeed:cls.Visual.runSpeed;
                    string dir=Path.Combine(Root,id,cls.Visual.name,profile.name);Directory.CreateDirectory(dir);
                    File.WriteAllText(Path.Combine(dir,"manifest.json"),JsonUtility.ToJson(new Manifest{classId=id,visual=cls.Visual.name,prefab=AssetDatabase.GetAssetPath(cls.Visual.modelPrefab),avatar=AssetDatabase.GetAssetPath(h.anim.avatar),profile=profile.name,weapon=profile.look.weaponItem!=null?profile.look.weaponItem.id:profile.look.weapon.ToString(),offhand=profile.look.offhandItem!=null?profile.look.offhandItem.id:profile.look.shield.ToString(),set=set!=null?set.name:"model",walkSpeed=walk,runSpeed=run,flags=$"rigid={cls.Visual.rigidBootToes};sagittal={cls.Visual.sagittalRunFeet};mage={cls.Visual.balancedMageStance};grip={cls.Visual.anatomicalGripSlots}",clips=Enumerable.Range(0,9).Select(i=>h.driver.AuditLocoClip(i)).ToArray(),footMarkers=h.feet.Select(f=>f.markers.Length).ToArray(),skinValidationError=h.feet.Select(f=>f.bakeError).ToArray()},true));
                    bool native=profile.name.StartsWith("native");
                    yield return Case(h,dir,"idle",Vector3.zero,1f/60,native);
                    for(int mode=0;mode<2;mode++)for(int d=0;d<8;d++) {var direction=Quaternion.Euler(0,d*45,0)*Vector3.forward;yield return Case(h,dir,(mode==0?"walk":"run")+"-"+d,direction*(mode==0?walk:run),1f/60,native&&d==0&&mode==1);}
                    // Takie same fazy i pełne cykle przy 30 fps, również dla innych rigów.
                    yield return Case(h,dir,"run-0",Vector3.forward*run,1f/30,native);
                    yield return Actions(h,dir);
                    Object.Destroy(h.go);yield return null;
                    File.WriteAllText(Path.Combine(Root,"progress.txt"),DateTime.Now.ToString("s")+" "+id+" "+profile.name);
                }
            }
        }
        [UnityTest,Timeout(3600000)]public IEnumerator A_Berserker_PelneCykleIWyposazenie() {yield return Sweep(new[]{ClassIds[0]});}
        [UnityTest,Timeout(3600000)]public IEnumerator B_PozostalePostacie_PelneCykleIWyposazenie() {yield return Sweep(ClassIds.Skip(1).ToArray());}

        [UnityTest,Timeout(3600000)]public IEnumerator C_Arena_RzeczywisteModeleIKamera() {
            tempProfile="anatomy_audit_"+Guid.NewGuid().ToString("N")+".json";GameRoot.ProfileFileOverride=tempProfile;SceneManager.LoadScene("Main");yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameRoot>();Assert.IsNotNull(root);Time.captureFramerate=60;
            foreach(string id in ClassIds) {var cls=root.config.classes.First(c=>c.id==id);int kits=id==ClassIds[0]?cls.loadouts.Count:1;
                for(int kit=0;kit<kits;kit++) {
                    Assert.IsTrue(root.TryStartRun(new LoadoutPlan{classDef=cls,loadoutIndex=kit,difficulty=root.config.difficulties[0]},out var reason),reason);yield return null;yield return null;
                    foreach(var enemy in root.Enemies)if(enemy!=null)enemy.gameObject.SetActive(false);root.enabled=false;
                    var player=root.Player;var visual=player.GetComponent<CharacterVisual>();var anim=visual.Model.GetComponentInChildren<Animator>();var controller=player.GetComponent<PlayerController>();
                    string dir=Path.Combine(Root,id,cls.Visual.name,"arena-"+kit);Directory.CreateDirectory(dir);
                    for(int i=0;i<60;i++)yield return null;Capture(Camera.main,Path.Combine(dir,"idle.png"),1280,720);
                    controller.Driver=new Forward();using(var w=new StreamWriter(Path.Combine(dir,"feet.csv"))) {w.WriteLine("frame,side,ankle_y,collider_y,visible_floor_y,run_phase");for(int i=0;i<180;i++) {
                        foreach(var pair in new[]{(HumanBodyBones.LeftFoot,"L"),(HumanBodyBones.RightFoot,"R")}) {var foot=anim.GetBoneTransform(pair.Item1);float floor=float.NaN;foreach(var hit in Physics.RaycastAll(foot.position+Vector3.up*2,Vector3.down,5))if(!hit.transform.IsChildOf(player.transform)&&(float.IsNaN(floor)||hit.point.y>floor))floor=hit.point.y;
                            w.WriteLine(i+","+pair.Item2+","+N(foot.position.y)+","+N(floor)+",NaN,"+N(((ClipAnimationDriver)visual.Driver).AuditStepPhase));}
                        if(i==40||i==80||i==120||i==160)Capture(Camera.main,Path.Combine(dir,"run-"+i+".png"),1280,720);yield return null;
                    }}
                    controller.Driver=null;
                    for(int i=0;i<120;i++) {if(i%40==0)player.Request(ActionType.LightAttack);if(i==35||i==75)Capture(Camera.main,Path.Combine(dir,"attack-"+i+".png"),1280,720);yield return null;}
                    root.enabled=true;
                }
            }
        }

        [UnityTest,Timeout(3600000)]public IEnumerator D_Zrodla_SiatkaPodeszwyISekwencje() {
            Studio();
            foreach(string id in ClassIds) {
                var cls=AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content+"Classes/"+id+".asset");
                var variants=new List<(string label,CharacterVisualDefinition def)>{("active",cls.Visual)};
                string old=id=="class_hu_berserker"?"visual_hero_warrior_berserker":id=="class_mage"?"visual_mage_witch":id=="class_hu_paladin"?"visual_paladin":null;
                if(old!=null){var def=Object.Instantiate(AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Content+"Visuals/"+old+".asset"));cleanup.Add(def);def.oneHandSet=cls.Visual.oneHandSet;def.twoHandSet=cls.Visual.twoHandSet;def.bowSet=cls.Visual.bowSet;def.staffSet=cls.Visual.staffSet;variants.Add(("old-rig-same-sets",def));}
                foreach(var variant in variants) {
                    var h=Build(variant.def,Look(cls.loadouts[0]));var set=h.visual.ModelSet;float speed=set!=null?set.runSpeed:variant.def.runSpeed;
                    string dir=Path.Combine(Root,id,cls.Visual.name,"diagnostic-"+variant.label);Directory.CreateDirectory(dir);
                    File.WriteAllText(Path.Combine(dir,"reference.json"),"{\"model\":\""+AssetDatabase.GetAssetPath(variant.def.modelPrefab)+"\",\"restYawL\":"+N(h.feet[0].restYaw)+",\"restYawR\":"+N(h.feet[1].restYaw)+",\"bakeErrorL\":"+N(h.feet[0].bakeError)+",\"bakeErrorR\":"+N(h.feet[1].bakeError)+",\"skinModeL\":"+h.feet[0].skinMode+",\"skinModeR\":"+h.feet[1].skinMode+"}");
                    yield return null;Shot(h,Path.Combine(dir,"rest-reference.png"),true,"front");
                    yield return Case(h,dir,"idle",Vector3.zero,1f/60,false);
                    yield return Case(h,dir,"run-0",Vector3.forward*speed,1f/60,true,true,true);
                    yield return Case(h,dir,"run-0",Vector3.forward*speed,1f/30,false,true,true);
                    Object.Destroy(h.go);yield return null;
                }
            }
        }

        void BakeSoles(Hero h,StreamWriter writer,int frame,float dt,PhysicsScene? surface=null) {
            foreach(var skin in h.feet.Select(f=>f.skin).Where(s=>s!=null).Distinct()) {
                var baked=new Mesh();skin.BakeMesh(baked);var vertices=baked.vertices;
                foreach(var f in h.feet.Where(f=>f.skin==skin))foreach(var marker in f.markers) {
                    Vector3 p=skin.transform.TransformPoint(vertices[marker.index]);float floor=0;
                    if(surface.HasValue)floor=surface.Value.Raycast(p+Vector3.up*2,Vector3.down,out var hit,5)?hit.point.y:float.NaN;
                    writer.WriteLine(frame+","+N(frame*dt)+","+N(h.driver.AuditStepPhase)+","+(f==h.feet[0]?"L":"R")+","+marker.index+","+V(p)+","+N(floor)+","+N(f.ankle.position.y)+","+N(Bend(f))+","+N(Twist(f)));
                }
                Object.DestroyImmediate(baked);
            }
        }
        const string SoleHeader="frame,time,phase,side,marker,world_x,world_y,world_z,surface_y,ankle_y,knee_bend,knee_twist";
        [UnityTest,Timeout(3600000)]public IEnumerator E_Podparcie_BakeMeshCoKlatke() {
            foreach(string id in ClassIds) {
                var cls=AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content+"Classes/"+id+".asset");var h=Build(cls.Visual,Look(cls.loadouts[0]));var set=h.visual.ModelSet;float speed=set!=null?set.runSpeed:cls.Visual.runSpeed;
                string dir=Path.Combine(Root,id,cls.Visual.name,"baked-contact");Directory.CreateDirectory(dir);
                for(int i=0;i<120;i++)Tick(h,Vector3.forward*speed,1f/60);
                using(var writer=new StreamWriter(Path.Combine(dir,"markers.csv"))) {
                    writer.WriteLine(SoleHeader);int wraps=0,frame=0;float previous=h.driver.AuditStepPhase;
                    while(wraps<4&&frame<1200) {Tick(h,Vector3.forward*speed,1f/60);BakeSoles(h,writer,frame,1f/60);float phase=h.driver.AuditStepPhase;if(phase<previous-.5f)wraps++;previous=phase;if(frame++%12==0)yield return null;}
                    Assert.GreaterOrEqual(wraps,4,id);File.WriteAllText(Path.Combine(dir,"sampling.json"),"{\"frames\":"+frame+",\"wraps\":"+wraps+",\"dt\":0.016666667,\"method\":\"BakeMesh every frame\"}");
                }
                Object.Destroy(h.go);yield return null;
            }
        }

        [UnityTest,Timeout(3600000)]public IEnumerator F_NatywneZestawy_AkcjeIPowierzchniaAreny() {
            tempProfile="anatomy_native_"+Guid.NewGuid().ToString("N")+".json";GameRoot.ProfileFileOverride=tempProfile;SceneManager.LoadScene("Main");yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameRoot>();Assert.IsNotNull(root);root.enabled=false;
            var probeScene=SceneManager.CreateScene("Pomiar widocznej posadzki",new CreateSceneParameters(LocalPhysicsMode.Physics3D));var probe=probeScene.GetPhysicsScene();bool copied=false;
            foreach(string id in ClassIds.Where(id=>!SessionState.GetBool("Turris.AnatomyAudit.BerserkerOnly",false)||id=="class_hu_berserker")) {
                var cls=root.config.classes.First(c=>c.id==id);
                for(int kit=0;kit<cls.loadouts.Count;kit++) {
                    Assert.IsTrue(root.TryStartRun(new LoadoutPlan{classDef=cls,loadoutIndex=kit,difficulty=root.config.difficulties[0]},out var reason),reason);yield return null;yield return null;
                    foreach(var enemy in root.Enemies)if(enemy!=null)enemy.gameObject.SetActive(false);root.enabled=false;
                    if(!copied) {
                        foreach(var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name.StartsWith("GotyckiDziedziniec_")||f.name=="Rozeta_Intarsja"||f.name=="CiemnePodloze")) {
                            var go=new GameObject("Powierzchnia kontrolna");SceneManager.MoveGameObjectToScene(go,probeScene);go.transform.SetPositionAndRotation(filter.transform.position,filter.transform.rotation);go.transform.localScale=filter.transform.lossyScale;
                            var mesh=Object.Instantiate(filter.sharedMesh);cleanup.Add(mesh);go.AddComponent<MeshCollider>().sharedMesh=mesh;
                        }
                        Physics.SyncTransforms();copied=true;
                    }
                    var player=root.Player;player.enabled=false;player.GetComponent<PlayerController>().enabled=false;var visual=player.GetComponent<CharacterVisual>();visual.enabled=false;
                    var h=new Hero{go=player.gameObject,visual=visual,anim=visual.Model.GetComponentInChildren<Animator>(),driver=(ClipAnimationDriver)visual.Driver};h.feet=new[]{FootFor(h,true),FootFor(h,false)};
                    string dir=Path.Combine(Root,id,cls.Visual.name,"native-arena-"+kit);Directory.CreateDirectory(dir);
                    using(var states=new StreamWriter(Path.Combine(dir,"states.csv")))using(var feet=new StreamWriter(Path.Combine(dir,"bones.csv")))using(var soles=new StreamWriter(Path.Combine(dir,"markers.csv"))) {
                        states.WriteLine("frame,request,action,attack,phase,progress,clip");feet.WriteLine(Header);soles.WriteLine(SoleHeader);int frame=0;
                        void Step(string request) {player.Tick(1f/60);visual.Tick(1f/60);h.go.transform.position+=h.go.transform.forward*h.driver.ConsumeAdvance();var state=player.GetAnimState();states.WriteLine(frame+","+request+","+state.action+","+state.attack+","+state.phase+","+N(state.phaseProgress)+","+(h.driver.CurrentActionClip?.clip!=null?h.driver.CurrentActionClip.clip.name:"none"));Row(feet,h,"final",frame,1f/60);if(frame%4==0)BakeSoles(h,soles,frame,1f/60,probe);frame++;}
                        for(int i=0;i<90;i++)Step("warmup");
                        int starts=0;bool wasStartup=false;var captured=new HashSet<int>();
                        for(int i=0;i<480;i++) {var state=player.GetAnimState();bool startup=state.action==AnimAction.Attack&&state.phase==ActionPhase.Startup;if(startup&&!wasStartup)starts++;wasStartup=startup;
                            if(starts<3&&(state.action==AnimAction.None||state.action==AnimAction.Attack&&state.phase==ActionPhase.Recovery&&state.phaseProgress>.85f)){player.RestoreAll();player.Request(ActionType.LightAttack);}
                            Step("combo");if(state.action==AnimAction.Attack&&state.phase==ActionPhase.Active&&captured.Add(starts)){yield return null;Capture(Camera.main,Path.Combine(dir,"combo-active-"+starts+".png"),1280,720);}if(i%60==0)yield return null;}
                        Assert.GreaterOrEqual(starts,3,id+" zestaw "+kit+": trzy rozpoczęte ciosy rzeczywistej serii");
                        foreach(var action in new[]{ActionType.HeavyAttack,ActionType.Block,ActionType.Parry,ActionType.Dodge}) {
                            player.RestoreAll();if(action==ActionType.Block)player.SetBlockHeld(true);player.Request(action);
                            bool shot=false;for(int i=0;i<150;i++){if(action==ActionType.Block&&i==50)player.SetBlockHeld(false);Step(action.ToString());if(!shot&&player.GetAnimState().action!=AnimAction.None&&player.GetAnimState().phase==ActionPhase.Active){shot=true;yield return null;Capture(Camera.main,Path.Combine(dir,action+"-active.png"),1280,720);}if(i%60==0)yield return null;}
                            yield return null;Capture(Camera.main,Path.Combine(dir,action+".png"),1280,720);
                        }
                        foreach(int slot in new[]{0,PlayerCombat.SignatureSlot}) {player.RestoreAll();player.RequestSkill(slot);for(int i=0;i<150;i++){Step("skill-"+slot);if(i%60==0)yield return null;}}
                    }
                    // Następny StartRun odtwarza cały natywny model i wyposażenie; nie zapisuje assetów.
                }
            }
            yield return SceneManager.UnloadSceneAsync(probeScene);
        }
    }
}
#endif
