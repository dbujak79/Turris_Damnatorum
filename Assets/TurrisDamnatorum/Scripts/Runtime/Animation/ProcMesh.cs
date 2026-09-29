using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Generator prostych siatek (wszystkie odczytywalne, więc można je łączyć w jedną siatkę na kość).
    /// Konwencje: siatki jednostkowe (≈1 m), środek w (0,0,0), oś długa wzdłuż Y, chyba że napisano inaczej.
    /// </summary>
    public static class ProcMesh
    {
        static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();

        static Mesh Cached(string key, System.Func<Mesh> build)
        {
            if (Cache.TryGetValue(key, out var m) && m != null) return m;
            m = build();
            m.name = key;
            Cache[key] = m;
            return m;
        }

        // ------------------------------------------------------------------ Bryły o płaskich ścianach

        /// <summary>Prostopadłościan 1×1×1 (płaskie cieniowanie).</summary>
        public static Mesh Box() => Frustum(1f, 1f);

        /// <summary>
        /// Ścięty ostrosłup: podstawa 1×1 na y = −0,5, góra (topX × topZ) na y = +0,5, przesunięta o shiftZ.
        /// Dobre na tułów, hełmy, płyty zbroi, podeszwy.
        /// </summary>
        public static Mesh Frustum(float topX, float topZ, float shiftZ = 0f)
        {
            return Cached($"frustum_{topX:0.###}_{topZ:0.###}_{shiftZ:0.###}", () =>
            {
                float bx = 0.5f, bz = 0.5f, tx = 0.5f * topX, tz = 0.5f * topZ;
                var c = new[]
                {
                    new Vector3(-bx, -0.5f, -bz), new Vector3(bx, -0.5f, -bz), new Vector3(bx, -0.5f, bz), new Vector3(-bx, -0.5f, bz),
                    new Vector3(-tx, 0.5f, -tz + shiftZ), new Vector3(tx, 0.5f, -tz + shiftZ), new Vector3(tx, 0.5f, tz + shiftZ), new Vector3(-tx, 0.5f, tz + shiftZ),
                };
                return FromQuads(c, new[,]
                {
                    { 0, 1, 2, 3 }, // dół
                    { 7, 6, 5, 4 }, // góra
                    { 0, 4, 5, 1 }, // tył (−Z)
                    { 2, 6, 7, 3 }, // przód (+Z)
                    { 1, 5, 6, 2 }, // prawo
                    { 3, 7, 4, 0 }, // lewo
                });
            });
        }

        static Mesh FromQuads(Vector3[] corners, int[,] quads)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            for (int q = 0; q < quads.GetLength(0); q++)
            {
                int b = v.Count;
                for (int k = 0; k < 4; k++) v.Add(corners[quads[q, k]]);
                t.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 });
            }
            var m = new Mesh();
            m.SetVertices(v);
            m.SetTriangles(t, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // ------------------------------------------------------------------ Bryły obrotowe

        /// <summary>
        /// Bryła obrotowa: profil (promień, y) od dołu do góry, obracany wokół osi Y.
        /// Promień 0 na końcach zamyka bryłę; w przeciwnym razie końce są otwarte, chyba że caps = true.
        /// </summary>
        public static Mesh Lathe(string key, Vector2[] profile, int segments = 14, bool caps = true, bool doubleSided = false)
        {
            return Cached("lathe_" + key + "_" + segments + (doubleSided ? "_ds" : ""), () =>
            {
                var v = new List<Vector3>();
                var tris = new List<int>();
                int rings = profile.Length;
                for (int i = 0; i < rings; i++)
                    for (int s = 0; s <= segments; s++)
                    {
                        float a = s / (float)segments * Mathf.PI * 2f;
                        v.Add(new Vector3(Mathf.Cos(a) * profile[i].x, profile[i].y, Mathf.Sin(a) * profile[i].x));
                    }
                int row = segments + 1;
                for (int i = 0; i < rings - 1; i++)
                    for (int s = 0; s < segments; s++)
                    {
                        int a = i * row + s, b = a + 1, c = a + row, d = c + 1;
                        tris.AddRange(new[] { a, c, b, b, c, d });
                    }
                if (caps)
                {
                    AddCap(v, tris, profile[0], segments, false);
                    AddCap(v, tris, profile[rings - 1], segments, true);
                }
                if (doubleSided)
                {
                    // Wewnętrzna powierzchnia (otwarte szaty, rękawy, kaptur widoczne także od środka).
                    int n = v.Count, tc = tris.Count;
                    for (int i = 0; i < n; i++) v.Add(v[i]);
                    for (int i = 0; i < tc; i += 3) tris.AddRange(new[] { tris[i] + n, tris[i + 2] + n, tris[i + 1] + n });
                }
                var m = new Mesh();
                m.SetVertices(v);
                m.SetTriangles(tris, 0);
                m.RecalculateNormals();
                m.RecalculateBounds();
                return m;
            });
        }

        static void AddCap(List<Vector3> v, List<int> tris, Vector2 p, int segments, bool top)
        {
            if (p.x <= 1e-4f) return;
            int center = v.Count;
            v.Add(new Vector3(0, p.y, 0));
            int start = v.Count;
            for (int s = 0; s <= segments; s++)
            {
                float a = s / (float)segments * Mathf.PI * 2f;
                v.Add(new Vector3(Mathf.Cos(a) * p.x, p.y, Mathf.Sin(a) * p.x));
            }
            for (int s = 0; s < segments; s++)
            {
                if (top) tris.AddRange(new[] { center, start + s + 1, start + s });
                else tris.AddRange(new[] { center, start + s, start + s + 1 });
            }
        }

        /// <summary>Kula o średnicy 1.</summary>
        public static Mesh Sphere(int rings = 10, int segments = 14)
        {
            var p = new Vector2[rings + 1];
            for (int i = 0; i <= rings; i++)
            {
                float a = -Mathf.PI / 2f + Mathf.PI * i / rings;
                p[i] = new Vector2(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f);
            }
            p[0].x = 0; p[rings].x = 0;
            return Lathe($"sphere{rings}", p, segments, false);
        }

        /// <summary>Półkula (kopuła) o średnicy 1, podstawa na y = 0, szczyt na y = 0,5.</summary>
        public static Mesh Dome(int rings = 6, int segments = 14)
        {
            var p = new Vector2[rings + 1];
            for (int i = 0; i <= rings; i++)
            {
                float a = Mathf.PI / 2f * i / rings;
                p[i] = new Vector2(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f);
            }
            p[rings].x = 0;
            return Lathe($"dome{rings}", p, segments, true);
        }

        /// <summary>Rura/walec o wysokości 1 i średnicy dołu 1; góra ma średnicę topRatio.</summary>
        public static Mesh Tube(float topRatio = 1f, int segments = 14)
        {
            return Lathe($"tube{topRatio:0.###}", new[] { new Vector2(0.5f, -0.5f), new Vector2(0.5f * topRatio, 0.5f) }, segments, true);
        }

        /// <summary>Kończyna: lekko wybrzuszony, zwężający się walec z zaokrąglonymi końcami (wysokość 1, średnica 1).</summary>
        public static Mesh Limb(float taper = 0.8f)
        {
            float t = taper;
            return Lathe($"limb{t:0.###}", new[]
            {
                new Vector2(0f, -0.5f), new Vector2(0.3f * t, -0.47f), new Vector2(0.45f * t, -0.38f), new Vector2(0.5f * Mathf.Lerp(t, 1f, 0.5f), -0.05f),
                new Vector2(0.5f, 0.25f), new Vector2(0.45f, 0.4f), new Vector2(0.3f, 0.48f), new Vector2(0f, 0.5f),
            }, 12, false);
        }

        /// <summary>Stożek: podstawa średnicy 1 na y = −0,5, szpic na y = +0,5.</summary>
        public static Mesh Cone(int segments = 12) => Lathe("cone", new[] { new Vector2(0.5f, -0.5f), new Vector2(0f, 0.5f) }, segments, true);

        /// <summary>Dzwon (szata, spódnica kolczugi, płaszcz na barki): góra średnicy topRatio na y = 0,5, dół średnicy 1 na y = −0,5.</summary>
        public static Mesh Bell(float topRatio, float bulge = 0.08f, int segments = 18)
        {
            return Lathe($"bell{topRatio:0.###}_{bulge:0.###}", new[]
            {
                new Vector2(0.5f, -0.5f), new Vector2(0.5f - 0.02f, -0.35f),
                new Vector2(Mathf.Lerp(0.5f, 0.5f * topRatio, 0.45f) + bulge * 0.5f, 0f),
                new Vector2(Mathf.Lerp(0.5f, 0.5f * topRatio, 0.8f) + bulge * 0.3f, 0.3f),
                new Vector2(0.5f * topRatio, 0.5f),
            }, segments, false, true);
        }

        /// <summary>Otwarta półsfera (powłoka, dwustronna) o średnicy 1: otwór na y = 0, szczyt na y = 0,5. Kaptury, pióropusze.</summary>
        public static Mesh Shell(int rings = 6, int segments = 16)
        {
            var p = new Vector2[rings + 1];
            for (int i = 0; i <= rings; i++)
            {
                float a = Mathf.PI / 2f * i / rings;
                p[i] = new Vector2(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f);
            }
            p[rings].x = 0;
            return Lathe($"shell{rings}", p, segments, false, true);
        }

        /// <summary>Pierścień (torus) w płaszczyźnie XZ, średnica 1, grubość minor (ułamek średnicy).</summary>
        public static Mesh Torus(float minor = 0.12f, int segments = 18, int sides = 6)
        {
            return Cached($"torus_{minor:0.###}_{segments}_{sides}", () =>
            {
                var v = new List<Vector3>();
                var tris = new List<int>();
                float R = 0.5f - minor * 0.5f, r = minor * 0.5f;
                for (int s = 0; s <= segments; s++)
                {
                    float a = s / (float)segments * Mathf.PI * 2f;
                    Vector3 c = new Vector3(Mathf.Cos(a) * R, 0, Mathf.Sin(a) * R);
                    Vector3 outDir = c.normalized;
                    for (int k = 0; k <= sides; k++)
                    {
                        float b = k / (float)sides * Mathf.PI * 2f;
                        v.Add(c + outDir * Mathf.Cos(b) * r + Vector3.up * Mathf.Sin(b) * r);
                    }
                }
                int row = sides + 1;
                for (int s = 0; s < segments; s++)
                    for (int k = 0; k < sides; k++)
                    {
                        int a = s * row + k, b2 = a + 1, c2 = a + row, d = c2 + 1;
                        tris.AddRange(new[] { a, b2, c2, b2, d, c2 });
                    }
                var m = new Mesh();
                m.SetVertices(v);
                m.SetTriangles(tris, 0);
                m.RecalculateNormals();
                m.RecalculateBounds();
                return m;
            });
        }

        /// <summary>Płaski czworokąt 1×1 leżący na ziemi (normalna +Y), z UV – dla kręgów i fal.</summary>
        public static Mesh GroundQuad()
        {
            return Cached("groundquad", () =>
            {
                var m = new Mesh();
                m.SetVertices(new[] { new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(0.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f) });
                m.SetUVs(0, new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
                m.SetTriangles(new[] { 0, 2, 1, 0, 3, 2 }, 0);
                m.RecalculateNormals();
                m.RecalculateBounds();
                return m;
            });
        }

        // ------------------------------------------------------------------ Kształty specjalne

        /// <summary>
        /// Ostrze: przekrój rombowy (szerokość wzdłuż X, grubość wzdłuż Y), od z = 0 do z = 1,
        /// zwężające się do szpica na ostatnich tipFraction długości.
        /// </summary>
        public static Mesh Blade(float tipFraction = 0.18f, float taper = 0.85f)
        {
            return Cached($"blade_{tipFraction:0.###}_{taper:0.###}", () =>
            {
                float zt = 1f - tipFraction;
                Vector3[] ring0 = { new Vector3(0.5f, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(-0.5f, 0, 0), new Vector3(0, -0.5f, 0) };
                var ring1 = new Vector3[4];
                for (int i = 0; i < 4; i++) ring1[i] = new Vector3(ring0[i].x * taper, ring0[i].y * taper, zt);
                Vector3 tip = new Vector3(0, 0, 1f);
                var v = new List<Vector3>();
                var t = new List<int>();
                for (int i = 0; i < 4; i++)
                {
                    int j = (i + 1) % 4;
                    int b = v.Count;
                    v.Add(ring0[i]); v.Add(ring0[j]); v.Add(ring1[j]); v.Add(ring1[i]);
                    t.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 });
                    b = v.Count;
                    v.Add(ring1[i]); v.Add(ring1[j]); v.Add(tip);
                    t.AddRange(new[] { b, b + 1, b + 2 });
                }
                int c0 = v.Count;
                for (int i = 0; i < 4; i++) v.Add(ring0[i]);
                t.AddRange(new[] { c0, c0 + 2, c0 + 1, c0, c0 + 3, c0 + 2 });
                var m = new Mesh();
                m.SetVertices(v);
                m.SetTriangles(t, 0);
                m.RecalculateNormals();
                m.RecalculateBounds();
                return m;
            });
        }

        /// <summary>Wyciągnięcie wielokąta (XY, gwiaździstego względem środka) na głębokość 1 wzdłuż Z (od −0,5 do +0,5).</summary>
        public static Mesh Extrude(string key, Vector2[] outline)
        {
            return Cached("extrude_" + key, () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                Vector2 c = Vector2.zero;
                foreach (var p in outline) c += p;
                c /= outline.Length;
                int n = outline.Length;
                foreach (float z in new[] { 0.5f, -0.5f })
                {
                    int center = v.Count;
                    v.Add(new Vector3(c.x, c.y, z));
                    for (int i = 0; i < n; i++) v.Add(new Vector3(outline[i].x, outline[i].y, z));
                    for (int i = 0; i < n; i++)
                    {
                        int a = center + 1 + i, b = center + 1 + (i + 1) % n;
                        if (z > 0) t.AddRange(new[] { center, b, a }); else t.AddRange(new[] { center, a, b });
                    }
                }
                for (int i = 0; i < n; i++)
                {
                    int j = (i + 1) % n;
                    int b = v.Count;
                    v.Add(new Vector3(outline[i].x, outline[i].y, 0.5f));
                    v.Add(new Vector3(outline[j].x, outline[j].y, 0.5f));
                    v.Add(new Vector3(outline[j].x, outline[j].y, -0.5f));
                    v.Add(new Vector3(outline[i].x, outline[i].y, -0.5f));
                    t.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 });
                }
                var m = new Mesh();
                m.SetVertices(v);
                m.SetTriangles(t, 0);
                m.RecalculateNormals();
                m.RecalculateBounds();
                return m;
            });
        }

        /// <summary>Obrys tarczy herbowej (szerokość 1, wysokość ≈ 1,2, szpic na dole).</summary>
        public static Vector2[] HeaterOutline()
        {
            var pts = new List<Vector2>();
            pts.Add(new Vector2(-0.5f, 0.45f));
            pts.Add(new Vector2(0.5f, 0.45f));
            for (int i = 0; i <= 8; i++)
            {
                float a = i / 8f;
                // prawa krawędź łukiem do szpica
                float x = 0.5f * Mathf.Cos(a * Mathf.PI * 0.5f);
                float y = 0.45f - 0.35f * a - 0.75f * Mathf.Sin(a * Mathf.PI * 0.5f);
                pts.Add(new Vector2(x, y));
            }
            for (int i = 7; i >= 1; i--)
            {
                float a = i / 8f;
                float x = -0.5f * Mathf.Cos(a * Mathf.PI * 0.5f);
                float y = 0.45f - 0.35f * a - 0.75f * Mathf.Sin(a * Mathf.PI * 0.5f);
                pts.Add(new Vector2(x, y));
            }
            return pts.ToArray();
        }

        /// <summary>Obrys ostrza topora (półksiężyc) – ostrze w +Y, osadzenie przy y ≈ 0. Kolejność zgodna z ruchem wskazówek zegara.</summary>
        public static Vector2[] AxeOutline()
        {
            var outline = new List<Vector2> { new Vector2(-0.1f, 0.05f) };
            for (int i = 0; i <= 10; i++)
            {
                float a = Mathf.Lerp(-1.1f, 1.1f, i / 10f);
                outline.Add(new Vector2(0.5f * Mathf.Sin(a), 0.5f + 0.5f * Mathf.Cos(a)));
            }
            outline.Add(new Vector2(0.1f, 0.05f));
            return outline.ToArray();
        }
    }
}
