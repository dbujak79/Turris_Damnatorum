#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    public class FlailPlayTests
    {
        class Source : ICharacterAnimSource
        {
            public CharacterAnimState state;
            public CharacterAnimState GetAnimState() => state;
        }

        [Test]
        public void ShortCut_WithFlailTiming_DoesNotPlayInSlowMotion()
        {
            var visual = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>("Assets/TurrisDamnatorum/Content/Visuals/visual_paladin_anime.asset");
            var cut = visual.oneHandSet.actions.First(c => c.action == AnimAction.Attack && c.attack == AttackAnim.SlashLeft);
            var weapon = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/TurrisDamnatorum/Content/Items/hu_flail.asset");
            var s = new CharacterAnimState { action = AnimAction.Attack, attack = AttackAnim.SlashLeft,
                windupSeconds = weapon.weapon.light.windup / 1.1f, activeSeconds = weapon.weapon.light.active / 1.1f };
            s.actionDuration = s.windupSeconds + s.activeSeconds + weapon.weapon.light.recovery * 0.8f / 1.1f;
            s.phase = ActionPhase.Active;
            s.phaseProgress = 0;
            float a = ClipAnimationDriver.NormalizedTime(cut,s,cut.clip.length);
            // Po żądanym wcześniejszym zakończeniu klipu reszta akcji jest powrotem do gardy.
            // Tempo mierzymy na nieukończonym cięciu, przed ograniczeniem kursora do końca wycinka.
            float sample = Mathf.Min(0.5f,(1f-a)*cut.clip.length/s.activeSeconds*0.8f);
            s.phaseProgress = sample;
            float b = ClipAnimationDriver.NormalizedTime(cut,s,cut.clip.length);
            float rate = (b-a)*cut.clip.length/(s.activeSeconds*sample);
            Debug.Log($"Korbacz: tempo krótkiego cięcia {rate:F3}x nagrania");
            Assert.GreaterOrEqual(rate,0.99f);
            s.phase = ActionPhase.Startup; s.phaseProgress = 1;
            Assert.AreEqual(a,ClipAnimationDriver.NormalizedTime(cut,s,cut.clip.length),0.0001f,"ciągłość na granicy trafienia");
        }

        [UnityTest]
        public IEnumerator EquippedFlail_SwingsAndDeformsMesh_Gallery()
        {
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>("Assets/TurrisDamnatorum/Content/Visuals/visual_paladin_anime.asset");
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/TurrisDamnatorum/Content/Items/hu_flail.asset");
            var go = new GameObject("PróbaKorbacza");
            var visual = go.AddComponent<CharacterVisual>(); visual.enabled = false;
            var source = new Source();
            var camGo = new GameObject("KameraKorbacza");
            var lightGo = new GameObject("ŚwiatłoKorbacza");
            var mesh = new Mesh();
            try
            {
                visual.BuildModel(def,1,new RigLook {weapon=WeaponModel.Mace,weaponItem=item,shield=ShieldModel.Heater});
                visual.Init(source);
                yield return null;
                var model = go.GetComponentsInChildren<Transform>().First(t => t.name == item.id).gameObject;
                var chain = HellFlailChain.Find(model);
                Assert.IsNotNull(chain);
                var renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
                var camera = camGo.AddComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(0.12f,0.14f,0.18f); camera.fieldOfView=35;
                var light = lightGo.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.5f;
                lightGo.transform.rotation=Quaternion.Euler(40,-35,0);
                for(int i=0;i<120;i++)visual.Tick(1f/60);
                renderer.BakeMesh(mesh); var before=mesh.vertices;
                Vector3 first=chain.HeadPosition;
                float movement=0;
                string dir="TestResults/flail";Directory.CreateDirectory(dir);
                for(int frame=0;frame<180;frame++)
                {
                    int local=frame%60;
                    source.state=new CharacterAnimState {action=AnimAction.Attack,attack=frame<60?AttackAnim.SlashRight:AttackAnim.SlashLeft,
                        phase=local<24?ActionPhase.Startup:local<35?ActionPhase.Active:ActionPhase.Recovery,
                        phaseProgress=local<24?local/24f:local<35?(local-24)/11f:(local-35)/25f,
                        windupSeconds=0.4f,activeSeconds=11f/60,actionDuration=1f};
                    visual.Tick(1f/60);
                    movement=Mathf.Max(movement,Vector3.Distance(first,chain.HeadPosition));
                    Assert.LessOrEqual(Vector3.Distance(chain.Anchor,chain.HeadPosition),chain.Length+0.005f,"łańcuch nie rozciąga się");
                    if(frame%45==15)
                    {
                        yield return null; yield return null;
                        camera.transform.position=go.transform.position+new Vector3(2.4f,1.6f,3);
                        camera.transform.LookAt(go.transform.position+Vector3.up);
                        var rt=new RenderTexture(1200,900,24);var tex=new Texture2D(1200,900,TextureFormat.RGB24,false);
                        var old=RenderTexture.active;
                        camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                        tex.ReadPixels(new Rect(0,0,1200,900),0,0);tex.Apply();
                        File.WriteAllBytes(Path.Combine(dir,"flail-"+frame+".png"),tex.EncodeToPNG());
                        camera.targetTexture=null;RenderTexture.active=old;Object.Destroy(rt);Object.Destroy(tex);
                    }
                }
                renderer.BakeMesh(mesh); var after=mesh.vertices;
                float deformation=before.Zip(after,(a,b)=>Vector3.Distance(a,b)).Max();
                Debug.Log($"Korbacz: ruch kuli {movement:F3} m, zmiana siatki {deformation:F3} m, długość {chain.Length:F3} m");
                Assert.Greater(movement,0.1f);Assert.Greater(deformation,0.05f,"obracają się wagi siatki, nie same kości");
                Vector3 paused=chain.HeadPosition;chain.Update(0);Assert.Less(Vector3.Distance(paused,chain.HeadPosition),0.001f);
            }
            finally {Object.Destroy(go);Object.Destroy(camGo);Object.Destroy(lightGo);Object.Destroy(mesh);}
        }
    }
}
#endif
