using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Wspólne materiały sylwetek HU. Tekstury i hit-flash pozostają w tych samych property blocks.</summary>
    public sealed class HellBodySurface
    {
        static readonly Dictionary<Material, Material> materials = new Dictionary<Material, Material>();
        static readonly int Surface = Shader.PropertyToID("_HellSurface");
        readonly Renderer[] renderers;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        Vector4 last = Vector4.one * -1;
        public HellBodySurface(GameObject model)
        {
            renderers = model.GetComponentsInChildren<Renderer>();
            var shader = Shader.Find("Turris/HellBody");
            foreach (var r in renderers)
            {
                var list = r.sharedMaterials;
                for (int i = 0; i < list.Length; i++)
                {
                    var original = list[i];
                    if (original == null || original.shader.name != MaterialLibrary.LitShader || shader == null) continue;
                    if (!materials.TryGetValue(original, out var copy) || copy == null)
                    {
                        copy = new Material(original) { name = original.name + "_HellBody", shader = shader };
                        materials[original] = copy;
                    }
                    list[i] = copy;
                }
                r.sharedMaterials = list;
            }
        }
        public void Set(float shadow, float wet, float ember, float elite)
        {
            var value = new Vector4(shadow, wet, ember, elite);
            if (value == last) return;
            last = value;
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    renderer.GetPropertyBlock(block, i); block.SetVector(Surface, value); renderer.SetPropertyBlock(block, i);
                }
            }
        }
    }
}
