using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Turris.Tests
{
    public class FlailChainTests
    {
        GameObject Model(string item)
        {
            var def = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/TurrisDamnatorum/Content/Items/"+item+".asset");
            var go = Object.Instantiate(def.model); go.transform.position = Vector3.up*2;
            return go;
        }

        [TestCase("hu_flail")]
        [TestCase("hu_flail2h")]
        public void Head_HangsUnderGravity_AndLagsBehindMovingHaft(string item)
        {
            var go = Model(item);
            try
            {
                var chain = HellFlailChain.Find(go); Assert.IsNotNull(chain);
                var joints = go.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("chain") || t.name=="head").ToArray();
                var localPositions = joints.Select(t=>t.localPosition).ToArray();
                for (int i = 0; i < 600; i++) chain.Update(1f/120);
                Assert.Less(Vector3.Angle(chain.HeadPosition-chain.Anchor,Vector3.down),8f,"Kula zwisa, zamiast być sztywna względem trzonka");
                Vector3 before = chain.HeadPosition;
                go.transform.position += Vector3.right*0.25f;
                chain.Update(1f/60);
                float movement = Vector3.Distance(before,chain.HeadPosition);
                Debug.Log(item+": ruch kuli po przesunięciu trzonka 25 cm: "+movement*100+" cm");
                Assert.Less(movement,0.20f,"Ciężka kula zachowuje bezwładność");
                for (int i = 0; i < joints.Length; i++)
                    Assert.Less(Vector3.Distance(localPositions[i],joints[i].localPosition),0.00001f,"Ogniwa pozostają połączone");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCase("hu_flail")]
        [TestCase("hu_flail2h")]
        public void PauseHitchAndTeleport_KeepChainFiniteAndConnected(string item)
        {
            var go = Model(item);
            try
            {
                var chain = HellFlailChain.Find(go);
                for (int i = 0; i < 60; i++) chain.Update(1f/60);
                Vector3 paused = chain.HeadPosition;
                for (int i = 0; i < 20; i++) chain.Update(0);
                Assert.Less(Vector3.Distance(paused,chain.HeadPosition),0.00001f,"Pauza nie symuluje ruchu");
                foreach (float dt in new[] { 1f/30,1f/144,0.8f,1f/60 })
                {
                    go.transform.position += Vector3.right*0.1f;
                    chain.Update(dt);
                    Assert.IsFalse(float.IsNaN(chain.HeadPosition.x));
                    Assert.LessOrEqual(Vector3.Distance(chain.Anchor,chain.HeadPosition),chain.Length+0.001f);
                }
                go.transform.position += Vector3.right*20;
                chain.Update(1f/60);
                Assert.IsFalse(float.IsNaN(chain.HeadPosition.x));
                Assert.LessOrEqual(Vector3.Distance(chain.Anchor,chain.HeadPosition),chain.Length+0.001f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test] public void OrdinarySword_HasNoChainSimulation()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TurrisDamnatorum/Models/Weapons/knight_sword/knight_sword.fbx");
            Assert.IsNull(HellFlailChain.Find(model));
        }
    }
}
