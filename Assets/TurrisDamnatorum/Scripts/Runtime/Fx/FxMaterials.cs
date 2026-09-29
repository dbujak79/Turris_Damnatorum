using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Materiały i tekstury efektów generowane w kodzie (bez zewnętrznych assetów).
    /// W buildzie materiały pochodzą z Resources (tworzy je Turris → Setup), w edytorze – z Shader.Find.
    /// </summary>
    public static class FxMaterials
    {
        static Material additive, alpha, glyphAdd, ringAdd;
        static Texture2D soft, glyph, ring, smoke;

        public const string AdditiveShader = "Legacy Shaders/Particles/Additive";
        public const string AlphaShader = "Legacy Shaders/Particles/Alpha Blended";

        public static Texture2D SoftTexture => soft != null ? soft : (soft = MakeSoft(64, 1.5f));
        public static Texture2D SmokeTexture => smoke != null ? smoke : (smoke = MakeSmoke(64));
        public static Texture2D RingTexture => ring != null ? ring : (ring = MakeRing(128));
        public static Texture2D GlyphTexture => glyph != null ? glyph : (glyph = MakeGlyph(256));

        /// <summary>Świecenie addytywne (cząsteczki, poświaty).</summary>
        public static Material Additive => additive != null ? additive : (additive = Make("TurrisFxAdditive", AdditiveShader, SoftTexture));
        /// <summary>Dym, kurz, popiół.</summary>
        public static Material AlphaBlended => alpha != null ? alpha : (alpha = Make("TurrisFxAlpha", AlphaShader, SmokeTexture));
        /// <summary>Runiczny krąg na ziemi.</summary>
        public static Material Glyph => glyphAdd != null ? glyphAdd : (glyphAdd = Make("TurrisFxGlyph", AdditiveShader, GlyphTexture));
        /// <summary>Pierścień fali uderzeniowej.</summary>
        public static Material Ring => ringAdd != null ? ringAdd : (ringAdd = Make("TurrisFxRing", AdditiveShader, RingTexture));

        static Material Make(string resourceName, string shaderName, Texture2D tex)
        {
            var fromResources = Resources.Load<Material>(resourceName);
            Material m;
            if (fromResources != null) m = new Material(fromResources);
            else
            {
                var shader = Shader.Find(shaderName);
                if (shader == null) shader = Shader.Find("Sprites/Default");
                m = new Material(shader);
            }
            m.name = resourceName;
            m.mainTexture = tex;
            if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", new Color(0.8f, 0.8f, 0.8f, 0.8f));
            return m;
        }

        // ------------------------------------------------------------------ Tekstury

        static Texture2D NewTex(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        }

        static Texture2D MakeSoft(int size, float power)
        {
            var t = NewTex(size);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                    float d = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    float a = Mathf.Pow(d, power);
                    px[y * size + x] = new Color(1, 1, 1, a) * new Color(a, a, a, 1f);
                }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        static Texture2D MakeSmoke(int size)
        {
            var t = NewTex(size);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float n = Mathf.PerlinNoise(x * 0.12f, y * 0.12f) * 0.6f + Mathf.PerlinNoise(x * 0.3f + 7f, y * 0.3f) * 0.4f;
                    float a = Mathf.Clamp01((1f - d) * 1.4f) * Mathf.Lerp(0.4f, 1f, n);
                    px[y * size + x] = new Color(1, 1, 1, a * a);
                }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        static Texture2D MakeRing(int size)
        {
            var t = NewTex(size);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Exp(-Mathf.Pow((d - 0.85f) / 0.07f, 2f)) + 0.25f * Mathf.Exp(-Mathf.Pow((d - 0.7f) / 0.15f, 2f));
                    a = Mathf.Clamp01(a) * (d < 1f ? 1f : 0f);
                    px[y * size + x] = new Color(a, a, a, a);
                }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        /// <summary>Runiczny krąg: podwójne okręgi, podziałka, heksagram i „runy” w pierścieniu.</summary>
        static Texture2D MakeGlyph(int size)
        {
            var t = NewTex(size);
            var a = new float[size * size];
            void Plot(float fx, float fy, float w)
            {
                int cx = Mathf.RoundToInt((fx * 0.5f + 0.5f) * (size - 1));
                int cy = Mathf.RoundToInt((fy * 0.5f + 0.5f) * (size - 1));
                int r = Mathf.CeilToInt(w * size * 0.5f) + 1;
                for (int oy = -r; oy <= r; oy++)
                    for (int ox = -r; ox <= r; ox++)
                    {
                        int px = cx + ox, py = cy + oy;
                        if (px < 0 || py < 0 || px >= size || py >= size) continue;
                        float dist = Mathf.Sqrt(ox * ox + oy * oy) / (w * size * 0.5f + 0.5f);
                        float v = Mathf.Clamp01(1f - dist);
                        a[py * size + px] = Mathf.Max(a[py * size + px], v);
                    }
            }
            void Line(Vector2 p0, Vector2 p1, float w)
            {
                int steps = Mathf.CeilToInt(Vector2.Distance(p0, p1) * size);
                for (int i = 0; i <= steps; i++) { var p = Vector2.Lerp(p0, p1, i / (float)Mathf.Max(1, steps)); Plot(p.x, p.y, w); }
            }
            void Circle(float r, float w)
            {
                int steps = Mathf.CeilToInt(r * size * 4f);
                for (int i = 0; i < steps; i++) { float ang = i / (float)steps * Mathf.PI * 2f; Plot(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, w); }
            }
            float lw = 3.4f / size;
            Circle(0.95f, lw * 1.6f);
            Circle(0.88f, lw);
            Circle(0.6f, lw * 1.2f);
            Circle(0.2f, lw);
            for (int i = 0; i < 48; i++)
            {
                float ang = i / 48f * Mathf.PI * 2f;
                float r0 = i % 4 == 0 ? 0.84f : 0.88f;
                Line(new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r0, new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 0.95f, lw);
            }
            for (int tri = 0; tri < 2; tri++)
                for (int k = 0; k < 3; k++)
                {
                    float a0 = (k / 3f + tri / 6f) * Mathf.PI * 2f + Mathf.PI / 2f, a1 = ((k + 1) / 3f + tri / 6f) * Mathf.PI * 2f + Mathf.PI / 2f;
                    Line(new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * 0.6f, new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * 0.6f, lw * 1.2f);
                }
            var rng = new System.Random(7);
            for (int i = 0; i < 12; i++)
            {
                float ang = (i + 0.5f) / 12f * Mathf.PI * 2f;
                Vector2 c = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 0.74f;
                for (int s = 0; s < 3; s++)
                {
                    Vector2 p0 = c + new Vector2((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f) * 0.09f;
                    Vector2 p1 = c + new Vector2((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f) * 0.09f;
                    Line(p0, p1, lw);
                }
            }
            var px2 = new Color[size * size];
            for (int i = 0; i < px2.Length; i++) px2[i] = new Color(a[i], a[i], a[i], a[i]);
            t.SetPixels(px2);
            t.Apply();
            return t;
        }
    }
}
