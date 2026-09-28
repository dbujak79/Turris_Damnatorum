using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Turris
{
    public enum Surface { Cloth, Leather, Metal, DarkMetal, Chain, Gold, Skin, Wood, Bone, Dark, Glow, Hair }

    /// <summary>Współdzielone materiały (po jednym na parę powierzchnia+kolor). Podświetlenia idą przez MaterialPropertyBlock.</summary>
    public static class MaterialLibrary
    {
        static Material template;
        static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        static Material Template()
        {
            if (template != null) return template;
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            template = new Material(tmp.GetComponent<Renderer>().sharedMaterial) { name = "TurrisTemplate" };
            Util.DestroyNow(tmp);
            return template;
        }

        public static Material Get(Surface s, Color c)
        {
            c.a = 1f;
            string key = $"{s}_{Mathf.RoundToInt(c.r * 63)}_{Mathf.RoundToInt(c.g * 63)}_{Mathf.RoundToInt(c.b * 63)}";
            if (Cache.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(Template()) { name = key, color = c };
            float metallic = 0f, gloss = 0.15f;
            switch (s)
            {
                case Surface.Metal: metallic = 0.8f; gloss = 0.62f; break;
                case Surface.DarkMetal: metallic = 0.75f; gloss = 0.45f; break;
                case Surface.Chain: metallic = 0.7f; gloss = 0.35f; break;
                case Surface.Gold: metallic = 0.9f; gloss = 0.7f; break;
                case Surface.Leather: gloss = 0.3f; break;
                case Surface.Skin: gloss = 0.28f; break;
                case Surface.Wood: gloss = 0.2f; break;
                case Surface.Bone: gloss = 0.35f; break;
                case Surface.Hair: gloss = 0.25f; break;
                case Surface.Dark: gloss = 0.05f; break;
                case Surface.Cloth: gloss = 0.08f; break;
            }
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", gloss);
            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", s == Surface.Glow ? c * 1.6f : Color.black);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            m.enableInstancing = true;
            Cache[key] = m;
            return m;
        }
    }

    /// <summary>Znacznik pojedynczej części przed scaleniem.</summary>
    public class RigPart : MonoBehaviour { }

    /// <summary>
    /// Buduje postać z wielu części (setki elementów), a potem scala części każdej kości w jedną siatkę
    /// z podsiatkami na materiały – szczegółowość bez setek wywołań rysowania.
    /// </summary>
    public static class PartBuilder
    {
        public static GameObject Add(Transform parent, Mesh mesh, Surface surface, Color color, Vector3 pos, Vector3 scale, Vector3 euler = default)
        {
            var go = new GameObject("Part");
            go.AddComponent<RigPart>();
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = MaterialLibrary.Get(surface, color);
            return go;
        }

        /// <summary>Scala części (bezpośrednie dzieci z RigPart) każdego węzła hierarchii pod <paramref name="root"/>.</summary>
        public static void BakeAll(Transform root)
        {
            var nodes = root.GetComponentsInChildren<Transform>(true).ToList();
            foreach (var t in nodes)
            {
                if (t == null || t.GetComponent<RigPart>() != null) continue;
                Bake(t);
            }
        }

        public static void Bake(Transform node)
        {
            var parts = new List<MeshFilter>();
            for (int i = 0; i < node.childCount; i++)
            {
                var c = node.GetChild(i);
                if (c.GetComponent<RigPart>() == null) continue;
                var mf = c.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) parts.Add(mf);
            }
            if (parts.Count == 0) return;

            Matrix4x4 toLocal = node.worldToLocalMatrix;
            var groups = parts.GroupBy(p => p.GetComponent<MeshRenderer>().sharedMaterial).ToList();
            var subMeshes = new List<CombineInstance>();
            var materials = new List<Material>();
            foreach (var g in groups)
            {
                var ci = g.Select(p => new CombineInstance { mesh = p.sharedMesh, transform = toLocal * p.transform.localToWorldMatrix }).ToArray();
                var sub = new Mesh { indexFormat = IndexFormat.UInt32 };
                sub.CombineMeshes(ci, true, true);
                subMeshes.Add(new CombineInstance { mesh = sub, transform = Matrix4x4.identity });
                materials.Add(g.Key);
            }
            var combined = new Mesh { name = "Baked_" + node.name, indexFormat = IndexFormat.UInt32 };
            combined.CombineMeshes(subMeshes.ToArray(), false, false);
            combined.RecalculateBounds();
            foreach (var s in subMeshes) Util.DestroyNow(s.mesh);

            var baked = new GameObject("Baked");
            baked.transform.SetParent(node, false);
            baked.AddComponent<MeshFilter>().sharedMesh = combined;
            var r = baked.AddComponent<MeshRenderer>();
            r.sharedMaterials = materials.ToArray();
            r.shadowCastingMode = ShadowCastingMode.On;
            foreach (var p in parts) Util.DestroyNow(p.gameObject);
        }
    }

    /// <summary>Zabarwienie i poświata przez MaterialPropertyBlock – bez tworzenia kopii materiałów.</summary>
    public class TintSet
    {
        struct Slot { public Renderer r; public int index; public Color color; public Color emission; }
        readonly List<Slot> slots = new List<Slot>();
        static MaterialPropertyBlock block;
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        Color lastTint; float lastAmount = -1f; Color lastGlow; float lastGlowAmount = -1f;

        public int Count => slots.Count;

        public void Collect(IEnumerable<Renderer> renderers)
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    slots.Add(new Slot
                    {
                        r = r, index = i,
                        color = m.HasProperty(ColorId) ? m.color : Color.white,
                        emission = m.HasProperty(EmissionId) ? m.GetColor(EmissionId) : Color.black,
                    });
                }
            }
            lastAmount = lastGlowAmount = -1f;
        }

        public void Clear() => slots.Clear();

        public void SetTint(Color c, float amount)
        {
            if (!Application.isPlaying) return;
            if (Mathf.Approximately(amount, lastAmount) && c == lastTint) return;
            lastTint = c; lastAmount = amount;
            if (block == null) block = new MaterialPropertyBlock();
            foreach (var s in slots)
            {
                if (s.r == null) continue;
                s.r.GetPropertyBlock(block, s.index);
                block.SetColor(ColorId, amount <= 0f ? s.color : Color.Lerp(s.color, c, amount));
                s.r.SetPropertyBlock(block, s.index);
            }
        }

        public void SetGlow(Color c, float intensity)
        {
            if (!Application.isPlaying) return;
            if (Mathf.Approximately(intensity, lastGlowAmount) && c == lastGlow) return;
            lastGlow = c; lastGlowAmount = intensity;
            if (block == null) block = new MaterialPropertyBlock();
            foreach (var s in slots)
            {
                if (s.r == null) continue;
                s.r.GetPropertyBlock(block, s.index);
                block.SetColor(EmissionId, intensity <= 0f ? s.emission : s.emission + c * intensity * 2f);
                block.SetColor(ColorId, intensity <= 0f ? s.color : Color.Lerp(s.color, c, Mathf.Clamp01(intensity) * 0.6f));
                s.r.SetPropertyBlock(block, s.index);
            }
        }
    }
}
