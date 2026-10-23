#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Regresja skręconej kostki w spoczynku: geometria kości i galeria z kamery gry.</summary>
    public class FootPosePlayTests
    {
        const string Visuals = "Assets/TurrisDamnatorum/Content/Visuals/";
        readonly List<Object> cleanup = new List<Object>();
        static readonly string[] Heroes = { "hero_warrior", "hero_warrior_dual", "paladin", "hero_warrior_berserker", "mage_witch", "hero_rogue_archer" };

        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            yield return null;
        }
        CharacterVisual Hero(string hero, out StubAnimSource source, bool? rigidToes = null)
        {
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_" + hero + ".asset");
            if (rigidToes.HasValue) { def = Object.Instantiate(def); cleanup.Add(def); def.rigidBootToes = rigidToes.Value; }
            var go = new GameObject("FootPose_" + hero); cleanup.Add(go);
            var visual = go.AddComponent<CharacterVisual>(); visual.enabled = false;
            source = new StubAnimSource();
            visual.BuildModel(def, 1, new RigLook { weapon = WeaponModel.Sword, shield = ShieldModel.None });
            visual.Init(source);
            return visual;
        }
        sealed class Foot
        {
            public readonly Transform thigh, knee, ankle, toe;
            readonly Vector3 restInKnee;
            public Foot(Animator animator, bool left)
            {
                thigh = animator.GetBoneTransform(left ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg);
                knee = animator.GetBoneTransform(left ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg);
                ankle = animator.GetBoneTransform(left ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
                toe = animator.GetBoneTransform(left ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes);
                restInKnee = knee.InverseTransformDirection(toe.position - ankle.position);
            }
            public float Twist()
            {
                var axis = (knee.position - ankle.position).normalized;
                return Vector3.SignedAngle(Vector3.ProjectOnPlane(knee.TransformDirection(restInKnee), axis), Vector3.ProjectOnPlane(toe.position - ankle.position, axis), axis);
            }
            public float Yaw() { var d = toe.position - ankle.position; return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg; }
        }
        [UnityTest]
        public IEnumerator IdleAndGuard_AnklesStayNearTheirRestTwist()
        {
            var report = new StringBuilder("hero\tstate\tleft_twist_max\tright_twist_max\tleft_yaw\tright_yaw\tyaw_max\n");
            var failures = new List<string>();
            foreach (var hero in Heroes)
            {
                var visual = Hero(hero, out var source);
                var animator = visual.Model.GetComponentInChildren<Animator>();
                var left = new Foot(animator, true); var right = new Foot(animator, false);
                foreach (var action in new[] { AnimAction.None, AnimAction.Block })
                {
                    source.state = new CharacterAnimState { action = action, hasShield = action == AnimAction.Block };
                    for (int frame = 0; frame < 60; frame++) visual.Tick(1f / 60);
                    float maxL = 0, maxR = 0, maxYaw = 0;
                    for (int frame = 0; frame < 120; frame++)
                    {
                        source.state.actionTime += 1f / 60; visual.Tick(1f / 60);
                        maxL = Mathf.Max(maxL, Mathf.Abs(left.Twist())); maxR = Mathf.Max(maxR, Mathf.Abs(right.Twist()));
                        maxYaw = Mathf.Max(maxYaw, Mathf.Abs(left.Yaw()), Mathf.Abs(right.Yaw()));
                    }
                    report.AppendLine($"{hero}\t{action}\t{maxL:F2}\t{maxR:F2}\t{left.Yaw():F2}\t{right.Yaw():F2}\t{maxYaw:F2}");
                    if (maxL > 30 || maxR > 30) failures.Add($"{hero}/{action}: skręt kostki L {maxL:F1}°, P {maxR:F1}°");
                    if (maxYaw > 45) failures.Add($"{hero}/{action}: stopa odwrócona bokiem ({maxYaw:F1}°)");
                }
                Object.Destroy(visual.gameObject); yield return null;
            }
            Directory.CreateDirectory("TestResults/foot"); File.WriteAllText("TestResults/foot/ankles.tsv", report.ToString());
            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        [UnityTest]
        public IEnumerator SteelBoots_ForefootMeshKeepsItsShape()
        {
            var report = new StringBuilder("hero\trigid\tmax_deformation_mm\n");
            foreach (string hero in new[] { "paladin", "hero_warrior" })
            foreach (bool rigid in new[] { false, true })
            {
                var visual = Hero(hero, out var source, rigid ? null : (bool?)false);
                var animator = visual.Model.GetComponentInChildren<Animator>();
                var samples = new List<(SkinnedMeshRenderer skin, Transform foot, Mesh mesh, List<int> indices, List<Vector3> points)>();
                foreach (bool left in new[] { true, false })
                {
                    var foot = animator.GetBoneTransform(left ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
                    var toe = animator.GetBoneTransform(left ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes);
                    Vector3 forward = foot.InverseTransformPoint(toe.position).normalized;
                    float toeDistance = Vector3.Distance(foot.position, toe.position);
                    foreach (var skin in visual.Model.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        var mesh = new Mesh(); cleanup.Add(mesh); skin.BakeMesh(mesh);
                        var indices = new List<int>(); var points = new List<Vector3>();
                        var vertices = mesh.vertices;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            Vector3 p = Quaternion.Inverse(foot.rotation) * (skin.transform.TransformPoint(vertices[i]) - foot.position);
                            if (Vector3.Dot(p, forward) <= toeDistance * 0.8f || p.magnitude > 0.3f
                                || Vector3.ProjectOnPlane(p, forward).magnitude > 0.1f) continue;
                            indices.Add(i); points.Add(p);
                        }
                        // Ostatnie 4 cm noska, bez przegubu cholewki ważonego do goleni.
                        float front = float.MinValue;
                        foreach (var p in points) front = Mathf.Max(front, Vector3.Dot(p, forward));
                        for (int i = points.Count - 1; i >= 0; i--)
                            if (Vector3.Dot(points[i], forward) < front - 0.04f) { points.RemoveAt(i); indices.RemoveAt(i); }
                        if (indices.Count > 0) samples.Add((skin, foot, mesh, indices, points));
                    }
                }
                Assert.IsNotEmpty(samples, hero + ": znaleziono wierzchołki stalowego noska");
                float worst = 0;
                string worstAt = "";
                for (int frame = 0; frame < 240; frame++)
                {
                    if (frame >= 60 && frame < 150) visual.transform.position += Vector3.forward * (3.87f / 60);
                    source.state = frame < 180 ? default : new CharacterAnimState
                    {
                        action = AnimAction.Attack, attack = AttackAnim.SlashRight, phase = ActionPhase.Active,
                        phaseProgress = (frame - 180) / 60f, actionTime = (frame - 180) / 60f
                    };
                    visual.Tick(1f / 60);
                    if (frame % 6 != 0) continue;
                    foreach (var sample in samples)
                    {
                        sample.skin.BakeMesh(sample.mesh); var vertices = sample.mesh.vertices;
                        Vector3 offset = Vector3.zero;
                        for (int i = 0; i < sample.indices.Count; i++)
                        {
                            Vector3 p = Quaternion.Inverse(sample.foot.rotation) * (sample.skin.transform.TransformPoint(vertices[sample.indices[i]]) - sample.foot.position);
                            offset += p - sample.points[i];
                        }
                        offset /= sample.indices.Count; // mierzymy deformację kształtu, niezależnie od przesunięcia całego fragmentu
                        for (int i = 0; i < sample.indices.Count; i++)
                        {
                            Vector3 p = Quaternion.Inverse(sample.foot.rotation) * (sample.skin.transform.TransformPoint(vertices[sample.indices[i]]) - sample.foot.position);
                            float error = (p - sample.points[i] - offset).magnitude;
                            if (error > worst) { worst = error; worstAt = $"klatka {frame}, {sample.foot.name}, punkt {sample.indices[i]}, {sample.points[i]} → {p}"; }
                        }
                    }
                }
                Debug.Log($"{hero} (sztywny: {rigid}): deformacja noska {worst * 1000:F2} mm (spoczynek, bieg, cios)");
                report.AppendLine($"{hero}\t{rigid}\t{worst * 1000:F3}");
                Directory.CreateDirectory("TestResults/boot"); File.WriteAllText("TestResults/boot/shape.tsv", report.ToString());
                if (rigid) Assert.Less(worst, 0.003f, hero + ": stalowy nosek nie zgina się podczas animacji; " + worstAt);
                else Assert.Greater(worst, 0.003f, hero + ": próba kontrolna wykrywa pierwotną deformację bez korekty");
                Object.Destroy(visual.gameObject); yield return null;
            }
        }

        [UnityTest]
        public IEnumerator FootGallery_WhenRequested()
        {
            string folder = Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(folder)) Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię stóp.");
            Directory.CreateDirectory(folder);
            var lightGo = new GameObject("FootPose_Light"); cleanup.Add(lightGo);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.3f;
            lightGo.transform.rotation = Quaternion.Euler(40, -30, 0);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); cleanup.Add(ground);
            ground.transform.position = Vector3.down * 0.52f; ground.transform.localScale = new Vector3(30, 1, 30);
            var material = new Material(Shader.Find("Standard")) { color = new Color(0.2f, 0.2f, 0.22f) }; cleanup.Add(material);
            ground.GetComponent<Renderer>().sharedMaterial = material;
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.44f);
            var cameraGo = new GameObject("FootPose_Camera"); cleanup.Add(cameraGo);
            var camera = cameraGo.AddComponent<Camera>(); camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.07f, 0.07f, 0.08f); camera.fieldOfView = 32;
            const int W = 400, H = 400;
            var sheet = new Texture2D(W * 3, H * Heroes.Length, TextureFormat.RGB24, false); cleanup.Add(sheet);
            var rt = new RenderTexture(W, H, 24); cleanup.Add(rt);
            for (int row = 0; row < Heroes.Length; row++)
            {
                var visual = Hero(Heroes[row], out _);
                for (int frame = 0; frame < 120; frame++) visual.Tick(1f / 60);
                yield return null;
                var views = new[] { new Vector3(2.3f, 1.6f, -3.4f), new Vector3(0.9f, 0.45f, -1.25f), new Vector3(0, 2.6f, 0.01f) };
                for (int column = 0; column < views.Length; column++)
                {
                    var at = column == 0 ? Vector3.up * 0.9f : Vector3.up * 0.12f;
                    camera.nearClipPlane = column == 2 ? 2.1f : 0.03f; // widok z góry: tułów nie zasłania butów
                    camera.transform.SetPositionAndRotation(views[column], Quaternion.LookRotation(at - views[column]));
                    var old = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                    sheet.ReadPixels(new Rect(0, 0, W, H), column * W, (Heroes.Length - row - 1) * H);
                    RenderTexture.active = old; camera.targetTexture = null;
                }
                Object.Destroy(visual.gameObject); yield return null;
            }
            sheet.Apply(); File.WriteAllBytes(Path.Combine(folder, "idle_feet.png"), sheet.EncodeToPNG());
            File.WriteAllLines(Path.Combine(folder, "idle_feet.txt"), Heroes);
        }
    }
}
#endif
