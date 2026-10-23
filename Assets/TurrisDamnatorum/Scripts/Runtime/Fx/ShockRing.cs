using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Ściana pyłu fali uderzeniowej: pochylony walec (Turris/FxShockRing) rozszerzający się z wyhamowaniem
    /// do promienia fali, rosnący i rzednący. Jedna siatka zamiast chmury cząsteczek – mało nakładania warstw.
    /// </summary>
    public sealed class ShockRing : MonoBehaviour
    {
        static readonly int FadeId = Shader.PropertyToID("_Fade"), AgeId = Shader.PropertyToID("_Age");
        static Mesh mesh;
        static Material material;
        MaterialPropertyBlock block;
        MeshRenderer rend;
        float radius, height, life, age;
        static readonly System.Collections.Generic.Stack<ShockRing> pool = new System.Collections.Generic.Stack<ShockRing>();

        public static ShockRing Spawn(Vector3 ground, float radius, float height = 0.9f, float life = 0.75f)
        {
            ShockRing fx = null;
            while (fx == null && pool.Count > 0) fx = pool.Pop();
            if (fx == null)
            {
                var go = new GameObject("ShockRing");
                go.AddComponent<MeshFilter>().sharedMesh = Mesh;
                fx = go.AddComponent<ShockRing>();
                fx.rend = go.AddComponent<MeshRenderer>();
                fx.rend.sharedMaterial = Material;
                fx.rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; fx.rend.receiveShadows = false;
                fx.block = new MaterialPropertyBlock();
            }
            fx.transform.position = ground;
            fx.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            fx.radius = radius; fx.height = height; fx.life = life; fx.age = 0f;
            fx.gameObject.SetActive(true);
            fx.Apply(0f);
            return fx;
        }

        void Update()
        {
            age += Time.deltaTime;
            if (age >= life) { gameObject.SetActive(false); pool.Push(this); return; }
            Apply(age / life);
        }

        void Apply(float t)
        {
            float spread = 1f - Mathf.Pow(1f - t, 3f);
            float r = Mathf.Lerp(0.2f, 1f, spread) * radius;
            transform.localScale = new Vector3(r, height * Mathf.Lerp(0.5f, 1.1f, spread), r);
            float fade = Mathf.Clamp01(t / 0.06f) * (1f - t * t);
            block.SetFloat(FadeId, fade); block.SetFloat(AgeId, t);
            rend.SetPropertyBlock(block);
        }

        static Material Material
        {
            get
            {
                if (material != null) return material;
                material = new Material(Resources.Load<Shader>("HellVfx/FxShockRing")) { name = "HU_ShockRing", hideFlags = HideFlags.DontSave };
                material.SetTexture("_NoiseTex", HellVfxMaterials.Texture("noise_tile"));
                material.renderQueue = 3050;
                return material;
            }
        }

        /// <summary>Otwarty walec: promień 1 przy ziemi, 1,25 u góry, wysokość 1; u – obwód, v – wysokość.</summary>
        static Mesh Mesh
        {
            get
            {
                if (mesh != null) return mesh;
                const int seg = 48;
                var v = new Vector3[(seg + 1) * 2]; var uv = new Vector2[v.Length]; var tri = new int[seg * 6];
                for (int i = 0; i <= seg; i++)
                {
                    float a = i * Mathf.PI * 2f / seg; float c = Mathf.Cos(a), s = Mathf.Sin(a);
                    v[i * 2] = new Vector3(c, 0, s); v[i * 2 + 1] = new Vector3(c * 1.25f, 1, s * 1.25f);
                    uv[i * 2] = new Vector2(i / (float)seg, 0); uv[i * 2 + 1] = new Vector2(i / (float)seg, 1);
                    if (i == seg) break;
                    int b = i * 2, k = i * 6;
                    tri[k] = b; tri[k + 1] = b + 1; tri[k + 2] = b + 3; tri[k + 3] = b; tri[k + 4] = b + 3; tri[k + 5] = b + 2;
                }
                mesh = new Mesh { name = "ShockRing", hideFlags = HideFlags.DontSave, vertices = v, uv = uv, triangles = tri };
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
