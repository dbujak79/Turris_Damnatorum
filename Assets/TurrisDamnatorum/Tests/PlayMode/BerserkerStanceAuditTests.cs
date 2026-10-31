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
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>
    /// Audyt postawy berserkera anime (opt-in, TURRIS_SHOT_DIR): skąd pochylenie tułowia i ułożenie nóg w spoczynku.
    /// Pomiary po każdej warstwie ClipAnimationDriver (PoseAudit) i po HellEquipmentPose, oraz zrzuty z 4 stron ze szkieletem:
    /// poza wiązania (Animator wyłączony – sam rig i skóra), klip spoczynku bez postawy topora, stan obecny gry.
    /// Test nie ocenia – zbiera materiał.
    /// </summary>
    public class BerserkerStanceAuditTests
    {
        const float Dt = 1f / 60f;
        readonly List<Object> cleanup = new List<Object>();
        class Source : ICharacterAnimSource { public CharacterAnimState state; public CharacterAnimState GetAnimState() => state; }

        [UnityTearDown]
        public IEnumerator Cleanup() { foreach (var o in cleanup) if (o != null) Object.Destroy(o); cleanup.Clear(); yield return null; }

        static string N(float x) => x.ToString("0.0", CultureInfo.InvariantCulture);
        static string V3(Vector3 v) => "(" + v.x.ToString("0.00", CultureInfo.InvariantCulture) + "; " + v.y.ToString("0.00", CultureInfo.InvariantCulture) + "; " + v.z.ToString("0.00", CultureInfo.InvariantCulture) + ")";

        /// <summary>Wskaźniki postawy w przestrzeni postaci (+Z przód, +X prawo, +Y góra); kąty w stopniach, wysokości w cm.</summary>
        static string Metrics(Transform ch, Animator a)
        {
            Transform B(HumanBodyBones b) => a.GetBoneTransform(b);
            Vector3 L(Transform t) => ch.InverseTransformPoint(t.position);
            Vector3 Ld(Vector3 d) => ch.InverseTransformDirection(d);
            var hips = B(HumanBodyBones.Hips); var neck = B(HumanBodyBones.Neck) ?? B(HumanBodyBones.Head); var head = B(HumanBodyBones.Head);
            Vector3 spine = Ld(neck.position - hips.position);
            float leanF = Mathf.Atan2(spine.z, spine.y) * Mathf.Rad2Deg, leanS = Mathf.Atan2(spine.x, spine.y) * Mathf.Rad2Deg;
            Vector3 pel = Ld(B(HumanBodyBones.RightUpperLeg).position - B(HumanBodyBones.LeftUpperLeg).position);
            Vector3 sh = Ld(B(HumanBodyBones.RightUpperArm).position - B(HumanBodyBones.LeftUpperArm).position);
            float pelRoll = Mathf.Atan2(pel.y, new Vector2(pel.x, pel.z).magnitude) * Mathf.Rad2Deg, pelYaw = Mathf.Atan2(-pel.z, pel.x) * Mathf.Rad2Deg;
            float shRoll = Mathf.Atan2(sh.y, new Vector2(sh.x, sh.z).magnitude) * Mathf.Rad2Deg, shYaw = Mathf.Atan2(-sh.z, sh.x) * Mathf.Rad2Deg;
            var sb = new StringBuilder();
            var chest = B(HumanBodyBones.UpperChest) ?? B(HumanBodyBones.Chest);
            float cz = L(chest).z;
            sb.Append($"dłonie przed klatką: P {N((L(B(HumanBodyBones.RightHand)).z - cz) * 100)} cm, L {N((L(B(HumanBodyBones.LeftHand)).z - cz) * 100)} cm (przód tułowia ≈ 22–26 cm) | ");
            float upper = Vector3.Distance(B(HumanBodyBones.LeftUpperArm).position, B(HumanBodyBones.LeftLowerArm).position);
            float fore = Vector3.Distance(B(HumanBodyBones.LeftLowerArm).position, B(HumanBodyBones.LeftHand).position);
            sb.Append($"ramię {N(upper * 100)} cm, przedramię {N(fore * 100)} cm, bark L {V3(L(B(HumanBodyBones.LeftUpperArm)))} P {V3(L(B(HumanBodyBones.RightUpperArm)))} | ");
            var all = a.GetComponentsInChildren<Transform>(true);
            var sL = all.FirstOrDefault(t => t.name == "HandSlot.L"); var sock = all.FirstOrDefault(t => t.name == "WeaponSocket") ?? all.FirstOrDefault(t => t.name == "HandSlot.R");
            if (sL != null && sock != null)
            {
                Vector3 d = sL.position - sock.position; float along = Vector3.Dot(d, sock.up);
                float off = (d - sock.up * along).magnitude;
                sb.Append($"lewa dłoń: wzdłuż trzonka {N(-along * 100)} cm, od osi {N(off * 100)} cm; chwyt P {V3(L(sock))}, L {V3(L(sL))}, trzonek {V3(Ld(sock.up))} | ");
            }
            sb.Append($"tułów: przód {N(leanF)}° bok {N(leanS)}° | miednica: przechył {N(pelRoll)}° skręt {N(pelYaw)}° | barki: przechył {N(shRoll)}° skręt {N(shYaw)}° | biodra {N(L(hips).y * 100)} cm");
            foreach (var side in new[] { "L", "P" })
            {
                bool l = side == "L";
                var up = B(l ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg); var kn = B(l ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg);
                var an = B(l ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot); var toe = B(l ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes);
                float bend = 180f - Vector3.Angle(up.position - kn.position, an.position - kn.position);
                Vector3 foot = Ld(toe.position - an.position); float footYaw = Mathf.Atan2(foot.x, foot.z) * Mathf.Rad2Deg;
                // kierunek kolana: odchylenie kolana od prostej biodro–kostka (w poziomie) względem kierunku stopy
                Vector3 mid = (up.position + an.position) * 0.5f; Vector3 kd = Ld(kn.position - mid); kd.y = 0;
                float kneeYaw = kd.magnitude > 0.005f ? Mathf.Atan2(kd.x, kd.z) * Mathf.Rad2Deg : float.NaN;
                Vector3 legLine = Ld(an.position - up.position);
                float legSide = Mathf.Atan2(legLine.x, -legLine.y) * Mathf.Rad2Deg, legFwd = Mathf.Atan2(legLine.z, -legLine.y) * Mathf.Rad2Deg;
                sb.Append($" || noga {side}: zgięcie kolana {N(bend)}°, kolano→ {N(kneeYaw)}° (stopa {N(footYaw)}°), linia biodro–kostka bok {N(legSide)}° przód {N(legFwd)}°, kostka {N(L(an).y * 100)} cm, x {N(L(an).x * 100)} z {N(L(an).z * 100)}");
            }
            return sb.ToString();
        }

        Camera cam;
        void Studio()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); cleanup.Add(ground);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(40, 1, 40);
            ground.GetComponent<Renderer>().material.color = new Color(0.42f, 0.40f, 0.38f);
            var lg = new GameObject("L"); cleanup.Add(lg); var l = lg.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.25f; lg.transform.rotation = Quaternion.Euler(40, -45, 0);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.52f);
            var cg = new GameObject("Cam"); cleanup.Add(cg); cam = cg.AddComponent<Camera>(); cam.enabled = false; cam.fieldOfView = 26f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.42f, 0.36f);
        }

        /// <summary>Chwyt z bliska: wzdłuż trzonka (od końca i od głowicy) oraz z boku – czy trzonek przechodzi przez pięść.</summary>
        void GripShots(Transform ch, string path)
        {
            var sock = Util.FindDeep(ch, "WeaponSocket"); if (sock == null) return;
            const int W = 420, H = 420;
            var sheet = new Texture2D(W * 4, H, TextureFormat.RGB24, false); var rt = new RenderTexture(W, H, 24);
            float fov = cam.fieldOfView; cam.fieldOfView = 30f; cam.nearClipPlane = 0.02f;
            var shots = new (Vector3 at, Vector3 from)[]
            {
                (sock.position, sock.position - sock.up * 0.45f),                       // od końca trzonka
                (sock.position, sock.position + ch.forward * 0.4f),                     // z przodu
                (sock.position, sock.position + Vector3.up * 0.4f + ch.forward * 0.05f),// z góry
                (sock.position, sock.position - ch.forward * 0.4f + Vector3.up * 0.05f),  // od strony ciała
            };
            for (int i = 0; i < shots.Length; i++)
            {
                cam.transform.position = shots[i].from; cam.transform.LookAt(shots[i].at, Vector3.up);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; sheet.ReadPixels(new Rect(0, 0, W, H), i * W, 0); RenderTexture.active = null; cam.targetTexture = null;
            }
            File.WriteAllBytes(path, sheet.EncodeToPNG());
            // to samo bez broni – kształt dłoni
            var rends = sock.GetComponentsInChildren<Renderer>(); foreach (var r in rends) r.enabled = false;
            for (int i = 0; i < shots.Length; i++)
            {
                cam.transform.position = shots[i].from; cam.transform.LookAt(shots[i].at, Vector3.up);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; sheet.ReadPixels(new Rect(0, 0, W, H), i * W, 0); RenderTexture.active = null; cam.targetTexture = null;
            }
            foreach (var r in rends) r.enabled = true;
            File.WriteAllBytes(path.Replace(".png", "_bez-broni.png"), sheet.EncodeToPNG());
            cam.fieldOfView = fov; cam.nearClipPlane = 0.3f;
            Object.Destroy(sheet); rt.Release();
        }
        void Shots(Animator a, Transform ch, string path, float centerY = 0.95f, float dist = 4.6f, Vector3 offset = default)
        {
            // szkielet: linie kości nóg, kręgosłupa i rąk (zawsze na wierzchu – osobna kamera nie jest potrzebna, linie są grube i jasne)
            var pairs = new (HumanBodyBones a, HumanBodyBones b, Color c)[]
            {
                (HumanBodyBones.Hips, HumanBodyBones.Spine, Color.yellow), (HumanBodyBones.Spine, HumanBodyBones.Chest, Color.yellow), (HumanBodyBones.Chest, HumanBodyBones.Neck, Color.yellow), (HumanBodyBones.Neck, HumanBodyBones.Head, Color.yellow),
                (HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, Color.cyan), (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, Color.cyan), (HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes, Color.cyan),
                (HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, Color.magenta), (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, Color.magenta), (HumanBodyBones.RightFoot, HumanBodyBones.RightToes, Color.magenta),
                (HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg, Color.white),
            };
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit")); mat.renderQueue = 4000; mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            var lines = new List<GameObject>();
            foreach (var p in pairs)
            {
                var ta = a.GetBoneTransform(p.a); var tb = a.GetBoneTransform(p.b); if (ta == null || tb == null) continue;
                var go = new GameObject("kosc"); var lr = go.AddComponent<LineRenderer>(); lr.material = mat; lr.startColor = lr.endColor = p.c;
                lr.material.color = p.c; lr.startWidth = lr.endWidth = 0.018f; lr.positionCount = 2; lr.SetPositions(new[] { ta.position, tb.position }); lines.Add(go);
            }
            // pion przez biodra – odniesienie dla pochylenia
            var v = new GameObject("pion"); var vl = v.AddComponent<LineRenderer>(); vl.material = new Material(mat); vl.material.color = Color.green; vl.startWidth = vl.endWidth = 0.008f;
            vl.positionCount = 2; Vector3 h = a.GetBoneTransform(HumanBodyBones.Hips).position; vl.SetPositions(new[] { new Vector3(h.x, 0, h.z), new Vector3(h.x, 2.0f, h.z) }); lines.Add(v);
            const int W = 420, H = 620;
            var sheet = new Texture2D(W * 4, H, TextureFormat.RGB24, false); var rt = new RenderTexture(W, H, 24);
            var dirs = new[] { ch.forward, ch.right, -ch.forward, -ch.right }; // przód, prawy bok, tył, lewy bok
            for (int i = 0; i < 4; i++)
            {
                Vector3 c = ch.position + Vector3.up * centerY + ch.TransformVector(offset);
                cam.transform.position = c + dirs[i] * dist + Vector3.up * 0.15f; cam.transform.LookAt(c);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; sheet.ReadPixels(new Rect(0, 0, W, H), i * W, 0); RenderTexture.active = null; cam.targetTexture = null;
            }
            sheet.Apply(); File.WriteAllBytes(path, sheet.EncodeToPNG());
            Object.Destroy(sheet); Object.Destroy(rt); foreach (var g in lines) Object.DestroyImmediate(g);
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator Berserker_PostawaWSpoczynku_WhenRequested()
        {
            string dir = Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            Directory.CreateDirectory(dir);
            Studio();
            var cls = AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/class_hu_berserker.asset");
            var k = cls.loadouts[0];
            var look = new RigLook { weapon = AnimResolve.ForItem(k.weapon), weaponItem = k.weapon, offhandItem = k.offHand, shield = AnimResolve.ForShield(k.offHand) };
            var report = new StringBuilder($"Wygląd {cls.Visual.name}, broń {k.weapon.id}\n");

            foreach (var variant in new[] { "1-poza-wiazania", "2-klip-bez-postawy-topora", "3-stan-gry" })
            {
                var go = new GameObject("Berserker " + variant); cleanup.Add(go);
                var vis = go.AddComponent<CharacterVisual>(); vis.enabled = false; var src = new Source();
                vis.BuildModel(cls.Visual, 1f, look); vis.Init(src);
                var drv = (ClipAnimationDriver)vis.Driver; var anim = vis.Model.GetComponentInChildren<Animator>();
                report.AppendLine("== " + variant);
                if (variant.StartsWith("1"))
                {
                    anim.enabled = false;
                    // poza wiązania z siatki: kości w położeniu z bindposes
                    var smr = vis.Model.GetComponentsInChildren<SkinnedMeshRenderer>().OrderByDescending(s => s.sharedMesh.vertexCount).First();
                    var root = smr.transform;
                    for (int i = 0; i < smr.bones.Length; i++)
                    {
                        var m = root.localToWorldMatrix * smr.sharedMesh.bindposes[i].inverse;
                        smr.bones[i].SetPositionAndRotation(m.GetColumn(3), m.rotation);
                    }
                    yield return null;
                    report.AppendLine("final: " + Metrics(go.transform, anim));
                }
                else
                {
                    if (variant.StartsWith("2") && drv.GreatSwing != null) drv.GreatSwing.Guard = false;
                    var stages = new List<string>();
                    for (int i = 0; i < 150; i++)
                    {
                        if (i == 149) drv.PoseAudit = (stage, a) => stages.Add(stage + ": " + Metrics(go.transform, a));
                        src.state = default; vis.Tick(Dt); go.transform.position += go.transform.forward * drv.ConsumeAdvance();
                    }
                    drv.PoseAudit = null;
                    foreach (var s in stages) report.AppendLine(s);
                    report.AppendLine("final (po HellEquipmentPose): " + Metrics(go.transform, anim));
                    var sock = Util.FindDeep(go.transform, "WeaponSocket");
                    if (sock != null)
                    {
                        float lo = 1e9f, hi = -1e9f;
                        foreach (var mf in sock.GetComponentsInChildren<MeshFilter>())
                            for (int c8 = 0; c8 < 8; c8++)
                            {
                                var b = mf.sharedMesh.bounds;
                                var v = b.center + Vector3.Scale(b.extents, new Vector3((c8 & 1) == 0 ? -1 : 1, (c8 & 2) == 0 ? -1 : 1, (c8 & 4) == 0 ? -1 : 1));
                                float t = Vector3.Dot(mf.transform.TransformPoint(v) - sock.position, sock.up); lo = Mathf.Min(lo, t); hi = Mathf.Max(hi, t); }
                        foreach (var mf in sock.GetComponentsInChildren<MeshFilter>())
                        {
                            var b = mf.sharedMesh.bounds;
                            report.AppendLine($"  część {mf.name}: środek w gnieździe {sock.InverseTransformPoint(mf.transform.TransformPoint(b.center)).ToString("F3")}, rozmiar {b.size.ToString("F3")}, lokalnie {mf.transform.localPosition.ToString("F3")} {mf.transform.localEulerAngles.ToString("F0")}, skala {mf.transform.lossyScale.ToString("F2")}");
                        }
                        report.AppendLine($"broń wzdłuż trzonka od gniazda prawej dłoni: od {lo * 100:F1} cm do {hi * 100:F1} cm (rozstaw dłoni {HellEquipmentPose.HeavyGripSpan * 100:F0} cm)");
                    }
                    report.AppendLine("klip spoczynku: " + (drv.AuditLocoClip(0) ?? "?"));
                    yield return null;
                }
                Shots(anim, go.transform, Path.Combine(dir, "postawa_" + variant + ".png"));
                Shots(anim, go.transform, Path.Combine(dir, "nogi_" + variant + ".png"), 0.42f, 2.2f);
                // zbliżenie chwytu: czy dłonie zaciskają się na trzonku
                Shots(anim, go.transform, Path.Combine(dir, "dlonie_" + variant + ".png"), 0.98f, 1.7f, new Vector3(0.1f, 0, 0.3f));
                GripShots(go.transform, Path.Combine(dir, "chwyt_" + variant + ".png"));
                foreach (var (hand, fore, tag) in new[] { (HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, "P"), (HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, "L") })
                {
                    var h = anim.GetBoneTransform(hand); var f = anim.GetBoneTransform(fore);
                    var rel = Quaternion.Inverse(f.rotation) * h.rotation;
                    float bend = Vector3.Angle(f.up, h.up);
                    // skręt wokół osi przedramienia: składowa „twist” obrotu względnego
                    var r = new Vector3(rel.x, rel.y, rel.z); var proj = Vector3.Project(r, Vector3.up);
                    var twist = new Quaternion(proj.x, proj.y, proj.z, rel.w); float tw = 0;
                    if (twist.w * twist.w + proj.sqrMagnitude > 1e-8f) { twist = Quaternion.Normalize(twist); twist.ToAngleAxis(out tw, out var ax); if (Vector3.Dot(ax, Vector3.up) < 0) tw = -tw; if (tw > 180) tw -= 360; }
                    report.AppendLine($"nadgarstek {tag} ({variant}): zgięcie {bend:F0}°, skręt {tw:F0}°");
                }
                foreach (var (bone, tag) in new[] { (HumanBodyBones.RightHand, "P"), (HumanBodyBones.LeftHand, "L") })
                {
                    var hb = anim.GetBoneTransform(bone);
                    var c = go.transform.InverseTransformPoint(hb.position + hb.up * 0.06f);
                    Shots(anim, go.transform, Path.Combine(dir, "dlon" + tag + "_" + variant + ".png"), 0f, 0.55f, c);
                }
                go.SetActive(false);
                yield return null;
            }
            File.WriteAllText(Path.Combine(dir, "postawa.txt"), report.ToString());
            Debug.Log("DETAIL postawa\n" + report);
        }
    }
}
#endif
