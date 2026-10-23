using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Współdzielone materiały i oryginalne tekstury HU. Resources zapewnia obecność w buildzie.</summary>
    public static class HellVfxMaterials
    {
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        public static Texture2D Texture(string name)
        {
            if (!textures.TryGetValue(name, out var texture))
            {
                texture = Resources.Load<Texture2D>("HellVfx/" + name);
                textures[name] = texture;
                if (texture == null) Debug.LogError("Brak tekstury HU: " + name);
            }
            return texture;
        }

        public static Material Sprite(string name, bool additive = true)
        {
            string key = name + (additive ? "+" : "a");
            if (materials.TryGetValue(key, out var material) && material != null) return material;
            // Shader w Resources pozostaje w buildzie także bez referencji z assetu sceny.
            var shader = Resources.Load<Shader>("HellVfx/HellParticle");
            material = new Material(shader) { name = "HU_" + key, hideFlags = HideFlags.DontSave };
            material.mainTexture = Texture(name);
            material.SetFloat("_Intensity", additive ? 0.6f : 1f);
            material.SetInt("_DstBlend", additive ? (int)UnityEngine.Rendering.BlendMode.One : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.renderQueue = additive ? 3100 : 3000;
            materials[key] = material; return material;
        }

        public static Material Rock
        {
            get
            {
                const string key = "rock";
                if (!materials.TryGetValue(key, out var material) || material == null)
                {
                    material = new Material(Shader.Find("Standard")) { name = "HU_Rock", color = new Color(0.16f, 0.13f, 0.1f), hideFlags = HideFlags.DontSave };
                    material.SetFloat("_Glossiness", 0.05f);
                    material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", new Color(0.14f, 0.048f, 0.008f));
                    materials[key] = material;
                }
                return material;
            }
        }

        public static Material Ground(HellFxElement element, bool telegraph = false)
        {
            string key = "ground_" + element + (telegraph ? "_telegraph" : "");
            if (materials.TryGetValue(key, out var material) && material != null) return material;
            material = new Material(Resources.Load<Shader>("HellVfx/HellGround")) { name = "HU_" + key, hideFlags = HideFlags.DontSave };
            material.SetTexture("_NoiseTex", Texture("noise_tile"));
            material.SetFloat("_Element", (int)element); material.SetFloat("_Telegraph", telegraph ? 1 : 0);
            materials[key] = material; return material;
        }
    }
}
