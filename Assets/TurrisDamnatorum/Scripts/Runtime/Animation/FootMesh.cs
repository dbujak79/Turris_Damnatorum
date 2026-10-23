using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Skraca stopy modelu na siatce, bez ruszania kości. Kości stopy Mixamo/Meshy mają skośne osie (oś stopy idzie od kostki
    /// w dół i do przodu), więc skalowanie kości przechyliłoby podeszwę i uniosło nosek. Zamiast tego wierzchołki ważone do stopy
    /// i palców są ściskane tylko w poziomie, wzdłuż kierunku kostka → palce, względem kostki – podeszwa zostaje na ziemi,
    /// a cholewka (częściowo ważona do goleni) przechodzi płynnie według wag. Kopia siatki jest wspólna dla wszystkich
    /// postaci z tym samym modelem i długością.
    /// </summary>
    public static class FootMesh
    {
        static readonly Dictionary<(Mesh, int), Mesh> cache = new Dictionary<(Mesh, int), Mesh>();

        public static void Shorten(GameObject model, Animator animator, float length)
        {
            if (Mathf.Abs(length - 1f) < 0.005f || animator == null || !animator.isHuman) return;
            foreach (var smr in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var src = smr.sharedMesh;
                if (src == null) continue;
                var key = (src, Mathf.RoundToInt(length * 1000f));
                if (!cache.TryGetValue(key, out var mesh) || mesh == null)
                {
                    mesh = Build(smr, animator, length);
                    if (mesh == null) continue;
                    cache[key] = mesh;
                }
                smr.sharedMesh = mesh;
            }
        }

        struct Side { public int foot, toe; public Vector3 ankle, forward; }

        static Mesh Build(SkinnedMeshRenderer smr, Animator animator, float length)
        {
            var src = smr.sharedMesh;
            if (!src.isReadable)
            {
                Debug.LogWarning($"FootMesh: siatka {src.name} bez Read/Write – stopy zostają w oryginalnej długości.");
                return null;
            }
            var bones = smr.bones;
            var bind = src.bindposes;
            int Index(HumanBodyBones b)
            {
                var t = animator.GetBoneTransform(b);
                return t == null ? -1 : System.Array.IndexOf(bones, t);
            }
            Vector3 BindPos(int i) => bind[i].inverse.MultiplyPoint3x4(Vector3.zero);
            bool MakeSide(HumanBodyBones shin, HumanBodyBones foot, HumanBodyBones toe, out Side s)
            {
                s = new Side { foot = Index(foot), toe = Index(toe) };
                int knee = Index(shin);
                if (s.foot < 0 || s.toe < 0 || knee < 0) return false;
                s.ankle = BindPos(s.foot);
                // Pion z goleni (w pozie spoczynkowej noga stoi prosto), poziomy kierunek stopy z kostki do palców.
                Vector3 up = (BindPos(knee) - s.ankle).normalized;
                s.forward = Vector3.ProjectOnPlane(BindPos(s.toe) - s.ankle, up).normalized;
                return s.forward.sqrMagnitude > 0.5f;
            }
            if (!MakeSide(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes, out var left) ||
                !MakeSide(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, HumanBodyBones.RightToes, out var right))
                return null;

            var mesh = Object.Instantiate(src);
            mesh.name = src.name + "_feet" + Mathf.RoundToInt(length * 100f);
            var verts = mesh.vertices;
            var normals = mesh.normals;
            var weights = mesh.boneWeights;
            if (weights.Length != verts.Length) { Object.Destroy(mesh); return null; }
            float cut = 1f - length;
            for (int v = 0; v < verts.Length; v++)
            {
                var bw = weights[v];
                float W(Side s) =>
                    (bw.boneIndex0 == s.foot || bw.boneIndex0 == s.toe ? bw.weight0 : 0f) +
                    (bw.boneIndex1 == s.foot || bw.boneIndex1 == s.toe ? bw.weight1 : 0f) +
                    (bw.boneIndex2 == s.foot || bw.boneIndex2 == s.toe ? bw.weight2 : 0f) +
                    (bw.boneIndex3 == s.foot || bw.boneIndex3 == s.toe ? bw.weight3 : 0f);
                float wl = W(left), wr = W(right);
                if (wl <= 0f && wr <= 0f) continue;
                var side = wl >= wr ? left : right;
                float w = Mathf.Max(wl, wr);
                float along = Vector3.Dot(verts[v] - side.ankle, side.forward);
                float k = 1f - cut * w;
                verts[v] -= side.forward * (along * (1f - k));
                // Ściśnięcie wzdłuż osi przechyla normalne w jej stronę (transformacja odwrotnie transponowana).
                if (normals.Length == verts.Length)
                {
                    var n = normals[v];
                    normals[v] = (n + side.forward * (Vector3.Dot(n, side.forward) * (1f / k - 1f))).normalized;
                }
            }
            mesh.vertices = verts;
            if (normals.Length == verts.Length) mesh.normals = normals;
            if (mesh.tangents.Length == verts.Length) mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
