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

        /// <summary>Wygląd sprite'a w uber-shaderze (Turris/HellParticle). Zera = dawny prosty wygląd.</summary>
        internal struct Look
        {
            public float hot, hotPower, erosion, erosionSoft, flow, softFade;
            public bool flipBlend;
        }

        // Wzmocnienie HDR trzymane nisko (szczyt ~1,5–3): wyżej ACES wybiela jądro do kremowej bieli, a poświatę daje bloom.
        // Profile per tekstura HU (lub nazwa wariantu z HellParticleLayer.Look). Dotyczą wszystkich rodzin,
        // które używają danej tekstury: iskry i żar świecą w HDR, dym i płomienie rozpadają się erozją.
        static readonly Dictionary<string, Look> looks = new Dictionary<string, Look>
        {
            ["fireball_sheet"] = new Look { hot = 1.2f, hotPower = 2f, erosion = 0.7f, erosionSoft = 0.25f, flow = 0.015f, softFade = 0.4f, flipBlend = true },
            ["flame_sheet"] = new Look { hot = 0.8f, hotPower = 2.5f, erosion = 0.8f, erosionSoft = 0.3f, flow = 0.02f, softFade = 0.35f, flipBlend = true },
            ["fire_puffs_atlas"] = new Look { hot = 0.5f, hotPower = 2.5f, erosion = 0.9f, erosionSoft = 0.3f, flow = 0.02f, softFade = 0.45f },
            ["embers_atlas"] = new Look { hot = 2.0f, hotPower = 1.5f },
            ["spark"] = new Look { hot = 2.0f, hotPower = 1.2f },
            ["smoke"] = new Look { erosion = 0.6f, erosionSoft = 0.35f, flow = 0.015f, softFade = 0.6f },
            ["glow"] = new Look { softFade = 0.5f },
            ["frost_mist"] = new Look { erosion = 0.6f, erosionSoft = 0.35f, flow = 0.015f, softFade = 0.6f },
            ["frost_rime"] = new Look { hot = 1.2f, hotPower = 2f },
            ["electric_sparks"] = new Look { hot = 2.0f, hotPower = 1.2f },
            ["lightning_arcs"] = new Look { hot = 2.0f, hotPower = 1.2f },
            // Błysk uderzenia ognia: krótkie, przesycone jądro (bloom), nie rozświetla sceny na długo.
            ["glow_hot"] = new Look { hot = 3f, hotPower = 1.2f, softFade = 0.3f },
        };

        public static Material Sprite(string name, bool additive = true, string look = null)
        {
            string key = (look ?? name) + (additive ? "+" : "a");
            if (materials.TryGetValue(key, out var material) && material != null) return material;
            if (look == "heat")
            {
                // Drganie powietrza: maska z tekstury, obraz z Opaque Texture (Turris/FxDistort).
                material = new Material(Resources.Load<Shader>("HellVfx/FxDistort")) { name = "HU_" + key, hideFlags = HideFlags.DontSave };
                material.mainTexture = Texture(name);
                material.SetTexture("_NoiseTex", Texture("noise_tile"));
                materials[key] = material; return material;
            }
            // Shader w Resources pozostaje w buildzie także bez referencji z assetu sceny.
            var shader = Resources.Load<Shader>("HellVfx/HellParticle");
            material = new Material(shader) { name = "HU_" + key, hideFlags = HideFlags.DontSave };
            material.mainTexture = Texture(name);
            material.SetFloat("_Intensity", additive ? 0.6f : 1f);
            material.SetInt("_DstBlend", additive ? (int)UnityEngine.Rendering.BlendMode.One : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.renderQueue = additive ? 3100 : 3000;
            material.SetTexture("_NoiseTex", Texture("noise_tile"));
            if (looks.TryGetValue(look ?? name, out var l))
            {
                material.SetFloat("_HotBoost", additive ? l.hot : 0f);
                material.SetFloat("_HotPower", l.hotPower > 0 ? l.hotPower : 2f);
                material.SetFloat("_Erosion", l.erosion);
                material.SetFloat("_ErosionSoft", l.erosionSoft > 0 ? l.erosionSoft : 0.2f);
                material.SetFloat("_Flow", l.flow);
                material.SetFloat("_SoftFade", l.softFade);
                material.SetFloat("_FlipBlend", l.flipBlend ? 1f : 0f);
            }
            materials[key] = material; return material;
        }

        /// <summary>Pęknięcia gruntu po uderzeniu (Turris/FxCracks) – FxDecal.</summary>
        public static Material Cracks
        {
            get
            {
                const string key = "cracks";
                if (materials.TryGetValue(key, out var material) && material != null) return material;
                material = new Material(Resources.Load<Shader>("HellVfx/FxCracks")) { name = "HU_Cracks", hideFlags = HideFlags.DontSave };
                material.SetTexture("_NoiseTex", Texture("noise_tile"));
                material.renderQueue = 2999;
                materials[key] = material; return material;
            }
        }

        /// <summary>Szron na ziemi (Turris/FxFrost) – FxDecal rosnący od środka.</summary>
        public static Material Frost
        {
            get
            {
                const string key = "frost";
                if (materials.TryGetValue(key, out var material) && material != null) return material;
                material = new Material(Resources.Load<Shader>("HellVfx/FxFrost")) { name = "HU_Frost", hideFlags = HideFlags.DontSave };
                material.SetTexture("_NoiseTex", Texture("noise_tile"));
                material.renderQueue = 2999;
                materials[key] = material; return material;
            }
        }

        static Texture2D beamTexture;

        /// <summary>Wiązka dla LineRenderer: jasny rdzeń w poprzek (V), stała wzdłuż (U). Uber-shader cząsteczek, HDR.</summary>
        public static Material Beam
        {
            get
            {
                const string key = "beam";
                if (materials.TryGetValue(key, out var material) && material != null) return material;
                if (beamTexture == null)
                {
                    beamTexture = new Texture2D(4, 64, TextureFormat.RGBA32, false, true) { name = "HU_BeamProfile", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                    var px = new Color[4 * 64];
                    for (int y = 0; y < 64; y++)
                    {
                        float v = Mathf.Abs((y + 0.5f) / 64f * 2f - 1f);
                        float a = Mathf.Exp(-v * v * 9f) * 0.75f + Mathf.Exp(-v * v * 60f) * 0.25f;
                        for (int x = 0; x < 4; x++) px[y * 4 + x] = new Color(1, 1, 1, a);
                    }
                    beamTexture.SetPixels(px); beamTexture.Apply(false, true);
                }
                material = new Material(Resources.Load<Shader>("HellVfx/HellParticle")) { name = "HU_Beam", hideFlags = HideFlags.DontSave };
                material.mainTexture = beamTexture;
                material.SetFloat("_Intensity", 1f);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_HotBoost", 2.5f); material.SetFloat("_HotPower", 2f);
                material.renderQueue = 3100;
                materials[key] = material; return material;
            }
        }

        static readonly List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>
        {
            ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Color,
            ParticleSystemVertexStream.UV, ParticleSystemVertexStream.Custom1XY,
            ParticleSystemVertexStream.UV2, ParticleSystemVertexStream.AnimBlend, ParticleSystemVertexStream.StableRandomX,
        };
        static AnimationCurve erosionCurve;

        /// <summary>
        /// Kontrakt strumieni uber-shadera (kolejność = układ TEXCOORD0/1 w shaderze) i krzywa erozji w Custom1.x:
        /// przez pierwszą połowę życia cząsteczka jest cała, potem rozpada się coraz szybciej.
        /// </summary>
        public static void ApplyStreams(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            renderer.SetActiveVertexStreams(streams);
            if (erosionCurve == null)
                erosionCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.45f, 0f, 0f, 0f), new Keyframe(1f, 1f, 2.6f, 0f));
            var custom = ps.customData;
            custom.enabled = true;
            custom.SetMode(ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Vector);
            custom.SetVectorComponentCount(ParticleSystemCustomData.Custom1, 2);
            custom.SetVector(ParticleSystemCustomData.Custom1, 0, new ParticleSystem.MinMaxCurve(1f, erosionCurve));
            custom.SetVector(ParticleSystemCustomData.Custom1, 1, new ParticleSystem.MinMaxCurve(0f));
        }

        /// <summary>
        /// Ślad na ziemi: ciemne wypalenie (alfa) albo żarzące się, stygnące pęknięcia (addytywnie).
        /// bolt = ślad pioruna: biało-niebieski żar stygnący do fioletu.
        /// </summary>
        public static Material Scorch(bool hot, bool bolt = false)
        {
            string key = (hot ? "scorch_hot" : "scorch") + (bolt && hot ? "_bolt" : "");
            if (materials.TryGetValue(key, out var material) && material != null) return material;
            material = new Material(Resources.Load<Shader>("HellVfx/FxScorch")) { name = "HU_" + key, hideFlags = HideFlags.DontSave };
            material.SetTexture("_NoiseTex", Texture("noise_tile"));
            material.SetFloat("_Hot", hot ? 1 : 0);
            if (bolt) { material.SetColor("_HotColor", new Color(0.75f, 0.9f, 1f)); material.SetColor("_CoolColor", new Color(0.25f, 0.12f, 0.6f)); }
            material.SetInt("_DstBlend", hot ? (int)UnityEngine.Rendering.BlendMode.One : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.renderQueue = hot ? 3001 : 2999;
            materials[key] = material; return material;
        }

        public static Material Rock
        {
            get
            {
                const string key = "rock";
                if (!materials.TryGetValue(key, out var material) || material == null)
                {
                    material = new Material(Shader.Find(MaterialLibrary.LitShader)) { name = "HU_Rock", color = new Color(0.16f, 0.13f, 0.1f), hideFlags = HideFlags.DontSave };
                    material.SetFloat("_Smoothness", 0.05f);
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
