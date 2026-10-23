using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Kryształy lodu z siatek: wyrastają z ziemi z lekkim przestrzeleniem rozmiaru, stoją chwilę,
    /// potem rozpadają się (rozpuszczanie z jasną krawędzią w Turris/FxIce) i lekko zapadają. Tylko wygląd.
    /// </summary>
    public sealed class IceCrystals : MonoBehaviour
    {
        const float Grow = 0.12f, Shatter = 0.38f;
        static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        static Mesh mesh;
        static Material material;

        struct Crystal { public Transform t; public Renderer r; public Vector3 scale; public float delay; }
        readonly List<Crystal> crystals = new List<Crystal>();
        static readonly Stack<IceCrystals> pool = new Stack<IceCrystals>();
        MaterialPropertyBlock block;
        float age, hold;
        int used;

        /// <summary>Wieniec kryształów wokół punktu na ziemi, odchylonych na zewnątrz (nova, wybuch lodu).</summary>
        public static IceCrystals Ring(Vector3 ground, float radius, int count, float size, float hold = 0.8f)
        {
            var fx = Create(hold);
            for (int i = 0; i < count; i++)
            {
                float angle = (i + Random.Range(-0.3f, 0.3f)) / count * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 at = ground + dir * radius * Random.Range(0.25f, 0.85f);
                fx.Add(at, Quaternion.AngleAxis(Random.Range(18f, 42f), Vector3.Cross(Vector3.up, dir)), size * Random.Range(0.6f, 1.3f), i * 0.012f);
            }
            return fx;
        }

        /// <summary>Kryształy w stożku przed rzucającym, coraz większe z odległością (podmuch mrozu).</summary>
        public static IceCrystals Arc(Vector3 ground, Vector3 forward, float range, float angle, int count, float size, float hold = 0.7f)
        {
            var fx = Create(hold);
            forward.y = 0; forward = forward.sqrMagnitude < 0.001f ? Vector3.forward : forward.normalized;
            for (int i = 0; i < count; i++)
            {
                float k = (i + 1f) / count;
                Vector3 dir = Quaternion.AngleAxis(Random.Range(-0.5f, 0.5f) * angle, Vector3.up) * forward;
                Vector3 at = ground + dir * range * Mathf.Lerp(0.25f, 0.95f, k);
                fx.Add(at, Quaternion.AngleAxis(Random.Range(15f, 35f), Vector3.Cross(Vector3.up, dir)), size * Mathf.Lerp(0.6f, 1.3f, k) * Random.Range(0.85f, 1.15f), k * 0.12f);
            }
            return fx;
        }

        static IceCrystals Create(float hold)
        {
            IceCrystals fx = null;
            while (fx == null && pool.Count > 0) fx = pool.Pop();
            if (fx == null) { fx = new GameObject("IceCrystals").AddComponent<IceCrystals>(); fx.block = new MaterialPropertyBlock(); }
            fx.hold = hold; fx.age = 0f; fx.used = 0;
            foreach (var c in fx.crystals) c.t.gameObject.SetActive(false);
            fx.gameObject.SetActive(true);
            return fx;
        }

        void Add(Vector3 at, Quaternion tilt, float size, float delay)
        {
            Crystal crystal;
            if (used < crystals.Count) crystal = crystals[used];
            else
            {
                var go = new GameObject("Crystal");
                go.transform.SetParent(transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = Mesh;
                var r = go.AddComponent<MeshRenderer>();
                r.sharedMaterial = Material; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                crystal = new Crystal { t = go.transform, r = r };
                crystals.Add(crystal);
            }
            crystal.t.position = at - Vector3.up * 0.05f * size;
            crystal.t.rotation = tilt * Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            crystal.t.localScale = Vector3.zero;
            crystal.scale = new Vector3(size * Random.Range(0.35f, 0.5f), size * Random.Range(1.1f, 1.6f), size * Random.Range(0.35f, 0.5f));
            crystal.delay = delay;
            crystal.t.gameObject.SetActive(true);
            crystals[used++] = crystal;
        }

        void Update()
        {
            age += Time.deltaTime;
            bool alive = false;
            for (int i = 0; i < used; i++)
            {
                var c = crystals[i];
                float t = age - c.delay;
                if (t < 0) { alive = true; continue; }
                float dissolve = Mathf.Clamp01((t - Grow - hold) / Shatter);
                if (dissolve < 1f) alive = true;
                // Wzrost z przestrzeleniem (ease-out back), potem lekkie zapadanie podczas rozpadu.
                float g = Mathf.Clamp01(t / Grow);
                float grow = 1f + 2.2f * Mathf.Pow(g - 1f, 3) + 1.2f * Mathf.Pow(g - 1f, 2);
                c.t.localScale = c.scale * grow * (1f - 0.25f * dissolve);
                block.SetFloat(DissolveId, dissolve);
                c.r.SetPropertyBlock(block);
            }
            if (!alive) { gameObject.SetActive(false); pool.Push(this); }
        }

        static Material Material
        {
            get
            {
                if (material != null) return material;
                material = new Material(Resources.Load<Shader>("HellVfx/FxIce")) { name = "HU_IceCrystal", hideFlags = HideFlags.DontSave };
                material.SetTexture("_NoiseTex", HellVfxMaterials.Texture("noise_tile"));
                return material;
            }
        }

        /// <summary>Sześcioboczny kryształ (wysokość 1, promień 0,5) z płaskimi fasetami; uv.y = wysokość.</summary>
        static Mesh Mesh
        {
            get
            {
                if (mesh != null) return mesh;
                const int sides = 6;
                // Pierścienie: wąska podstawa w ziemi, najszerszy brzuch, zwężenie i ostry czubek.
                float[] h = { 0f, 0.18f, 0.72f, 1f };
                float[] rad = { 0.32f, 0.5f, 0.38f, 0f };
                var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
                for (int ring = 0; ring < h.Length - 1; ring++)
                    for (int s = 0; s < sides; s++)
                    {
                        float a0 = s * Mathf.PI * 2f / sides, a1 = (s + 1) * Mathf.PI * 2f / sides;
                        Vector3 p00 = new Vector3(Mathf.Cos(a0) * rad[ring], h[ring], Mathf.Sin(a0) * rad[ring]);
                        Vector3 p01 = new Vector3(Mathf.Cos(a1) * rad[ring], h[ring], Mathf.Sin(a1) * rad[ring]);
                        Vector3 p10 = new Vector3(Mathf.Cos(a0) * rad[ring + 1], h[ring + 1], Mathf.Sin(a0) * rad[ring + 1]);
                        Vector3 p11 = new Vector3(Mathf.Cos(a1) * rad[ring + 1], h[ring + 1], Mathf.Sin(a1) * rad[ring + 1]);
                        int b = verts.Count;
                        verts.Add(p00); verts.Add(p10); verts.Add(p11); verts.Add(p01);
                        uvs.Add(new Vector2(0, h[ring])); uvs.Add(new Vector2(0, h[ring + 1])); uvs.Add(new Vector2(1, h[ring + 1])); uvs.Add(new Vector2(1, h[ring]));
                        tris.Add(b); tris.Add(b + 1); tris.Add(b + 2); tris.Add(b); tris.Add(b + 2); tris.Add(b + 3);
                    }
                mesh = new Mesh { name = "IceCrystal", hideFlags = HideFlags.DontSave };
                mesh.SetVertices(verts); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals(); mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
