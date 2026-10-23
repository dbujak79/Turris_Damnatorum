using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Błyskawica między dwoma punktami: poszarpany kanał (przemieszczenie punktów środkowych) z odgałęzieniami,
    /// szeroka poświata + cienki rdzeń HDR. Kształt losuje się na nowo co kilka klatek (ponowne wyładowania),
    /// a całość gaśnie z migotaniem. Tylko wygląd – obrażenia liczy wywołujący.
    /// </summary>
    public sealed class LightningBolt : MonoBehaviour
    {
        const float Life = 0.28f;
        const float Restrike = 0.045f;
        const int MaxBranches = 3;

        Vector3 from, to;
        Color color;
        float age, nextStrike;
        LineRenderer glow, core;
        readonly LineRenderer[] branchGlow = new LineRenderer[MaxBranches], branchCore = new LineRenderer[MaxBranches];
        readonly List<Vector3> points = new List<Vector3>(64);
        readonly List<Vector3> branch = new List<Vector3>(32);
        static readonly Vector3[] buffer = new Vector3[64];
        static readonly Stack<LightningBolt> pool = new Stack<LightningBolt>();

        public static LightningBolt Strike(Vector3 from, Vector3 to, Color color)
        {
            LightningBolt bolt = null;
            while (bolt == null && pool.Count > 0) bolt = pool.Pop();
            if (bolt == null)
            {
                bolt = new GameObject("Lightning").AddComponent<LightningBolt>();
                bolt.glow = bolt.Line("Glow");
                bolt.core = bolt.Line("Core");
                for (int i = 0; i < MaxBranches; i++) { bolt.branchGlow[i] = bolt.Line("BranchGlow"); bolt.branchCore[i] = bolt.Line("BranchCore"); }
            }
            bolt.from = from; bolt.to = to; bolt.color = color; bolt.age = 0f; bolt.nextStrike = 0f;
            bolt.gameObject.SetActive(true);
            bolt.Shape();
            bolt.Apply(0f);
            return bolt;
        }

        LineRenderer Line(string name)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = HellVfxMaterials.Beam;
            lr.alignment = LineAlignment.View; lr.textureMode = LineTextureMode.Stretch;
            lr.numCapVertices = 2; lr.numCornerVertices = 1;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; lr.receiveShadows = false;
            lr.useWorldSpace = true;
            return lr;
        }

        void Update()
        {
            age += Time.deltaTime;
            if (age >= Life) { gameObject.SetActive(false); pool.Push(this); return; }
            if (age >= nextStrike && age < Life * 0.6f) Shape();
            Apply(age / Life);
        }

        void Shape()
        {
            nextStrike = age + Restrike * Random.Range(0.7f, 1.3f);
            float length = Vector3.Distance(from, to);
            Jagged(from, to, Mathf.Clamp(length * 0.16f, 0.12f, 1.4f), points);
            Set(glow, points); Set(core, points);
            int count = length > 2.5f ? Random.Range(1, MaxBranches + 1) : Random.Range(0, 2);
            for (int i = 0; i < MaxBranches; i++)
            {
                bool on = i < count && points.Count > 4;
                branchGlow[i].enabled = branchCore[i].enabled = on;
                if (!on) continue;
                int start = Random.Range(1, points.Count - 2);
                Vector3 a = points[start];
                Vector3 dir = (to - from).normalized;
                Vector3 side = Vector3.Cross(dir, Random.onUnitSphere).normalized;
                Vector3 b = a + (dir * Random.Range(0.3f, 0.7f) + side * Random.Range(0.4f, 0.9f)) * length * Random.Range(0.18f, 0.32f);
                Jagged(a, b, Vector3.Distance(a, b) * 0.22f, branch);
                Set(branchGlow[i], branch); Set(branchCore[i], branch);
            }
        }

        /// <summary>Przemieszczenie punktów środkowych prostopadle do kanału; końce zostają w miejscu.</summary>
        static void Jagged(Vector3 a, Vector3 b, float amplitude, List<Vector3> result)
        {
            result.Clear(); result.Add(a); result.Add(b);
            Vector3 dir = (b - a).normalized;
            Vector3 u = Vector3.Cross(dir, Mathf.Abs(dir.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
            Vector3 v = Vector3.Cross(dir, u);
            float amp = amplitude;
            for (int level = 0; level < 5 && result.Count * 2 - 1 <= buffer.Length; level++)
            {
                for (int i = result.Count - 1; i > 0; i--)
                {
                    Vector3 mid = (result[i - 1] + result[i]) * 0.5f + (u * Random.Range(-1f, 1f) + v * Random.Range(-1f, 1f)) * amp;
                    result.Insert(i, mid);
                }
                amp *= 0.5f;
            }
        }

        static void Set(LineRenderer lr, List<Vector3> pts)
        {
            int n = Mathf.Min(pts.Count, buffer.Length);
            for (int i = 0; i < n; i++) buffer[i] = pts[i];
            lr.positionCount = n; lr.SetPositions(buffer);
        }

        void Apply(float t)
        {
            // Pierwsze wyładowanie najjaśniejsze, potem gaśnięcie z nierównym migotaniem.
            float fade = Mathf.Pow(1f - t, 1.4f) * (t < 0.08f ? 1f : Random.Range(0.55f, 1f));
            // Biały rdzeń i nasycona, chłodniejsza otoczka – rozdzielone barwą, nie tylko szerokością.
            Color hot = Color.Lerp(color, Color.white, 0.75f);
            Color halo = Color.Lerp(color, new Color(0.35f, 0.55f, 1f), 0.5f);
            Style(glow, halo, 0.42f, 0.6f * fade);
            Style(core, hot, 0.085f, fade);
            for (int i = 0; i < MaxBranches; i++)
            {
                if (!branchGlow[i].enabled) continue;
                Style(branchGlow[i], halo, 0.24f, 0.45f * fade);
                Style(branchCore[i], hot, 0.05f, 0.8f * fade);
            }
        }

        static void Style(LineRenderer lr, Color c, float width, float alpha)
        {
            lr.startWidth = width; lr.endWidth = width * 0.6f;
            c.a = Mathf.Clamp01(alpha);
            lr.startColor = lr.endColor = c;
        }
    }
}
