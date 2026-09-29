using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Szczegółowe areny z tysięcy części (posadzka z płyt, mury z bloków, przypory, blanki, brama, kolumny,
    /// pochodnie, sztandary, gruz, kości, posągi…), scalanych w kilka siatek.
    /// Kolizje są proste i niewidoczne (podłoga, pierścień murów, kolumny) – rozgrywka i kamera nie zależą od dekoracji.
    /// </summary>
    public static class ArenaBuilder
    {
        const float WallHeight = 5f;
        static System.Random rng;

        static float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        static Color Vary(Color c, int steps = 5)
        {
            // Kilka dyskretnych odcieni – ogranicza liczbę materiałów po scaleniu.
            int k = rng.Next(steps);
            return c * Mathf.Lerp(0.82f, 1.12f, k / (float)(steps - 1));
        }

        static void P(Transform t, Mesh m, Surface s, Color c, Vector3 pos, Vector3 scale, Vector3 euler = default)
            => PartBuilder.Add(t, m, s, c, pos, scale, euler);

        static Mesh Box => ProcMesh.Box();

        public static ArenaStyle StyleOf(ArenaDefinition a)
        {
            if (a.style != ArenaStyle.Auto) return a.style;
            string id = a.id ?? "";
            if (id.Contains("crypt")) return ArenaStyle.Crypt;
            if (id.Contains("summit")) return ArenaStyle.Summit;
            return ArenaStyle.Courtyard;
        }

        public static GameObject Build(ArenaDefinition a, Transform parent)
        {
            rng = new System.Random((a.id ?? "arena").GetHashCode());
            var style = StyleOf(a);
            var root = new GameObject("Arena_" + a.id);
            root.transform.SetParent(parent, false);
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            var props = new GameObject("Props").transform;
            props.SetParent(root.transform, false);

            BuildColliders(a, style, root.transform);
            Floor(a, style, visual);
            if (a.circular) CircularWall(a, style, visual, props);
            else SquareWall(a, style, visual, props);
            foreach (var p in a.pillars) Column(visual, p, style == ArenaStyle.Crypt ? 5.5f : 5f, a.wallColor * 1.08f, rng.NextDouble() < 0.25);
            Debris(a, visual);
            switch (style)
            {
                case ArenaStyle.Courtyard: CourtyardProps(a, visual, props); break;
                case ArenaStyle.Crypt: CryptProps(a, visual, props); break;
                case ArenaStyle.Summit: SummitProps(a, visual, props); break;
            }

            PartBuilder.BakeAll(visual);
            // Posągi mają własne scalone siatki; dekoracje nie rzucają się w oczy kolizjami.
            foreach (var r in visual.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            RenderSettings.fogStartDistance = style == ArenaStyle.Summit ? 30f : 14f;
            RenderSettings.fogEndDistance = style == ArenaStyle.Summit ? 110f : 55f;
            return root;
        }

        // ------------------------------------------------------------------ Kolizje (niewidoczne)

        static void BuildColliders(ArenaDefinition a, ArenaStyle style, Transform root)
        {
            float extent = a.size + 3f;
            var floor = new GameObject("FloorCollider");
            floor.transform.SetParent(root, false);
            floor.transform.localPosition = new Vector3(0, -0.5f, 0);
            floor.AddComponent<BoxCollider>().size = new Vector3(extent * 2f, 1f, extent * 2f);

            float h = style == ArenaStyle.Summit ? 1.3f : WallHeight;
            if (a.circular)
            {
                int n = Mathf.Max(24, Mathf.RoundToInt(a.size * 2.5f));
                float seg = 2f * Mathf.PI * (a.size + 0.5f) / n + 0.3f;
                for (int i = 0; i < n; i++)
                {
                    float ang = i * Mathf.PI * 2f / n;
                    var w = new GameObject("WallCollider");
                    w.transform.SetParent(root, false);
                    w.transform.localPosition = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * (a.size + 0.5f) + Vector3.up * 2.5f;
                    w.transform.localRotation = Quaternion.Euler(0, -ang * Mathf.Rad2Deg + 90f, 0);
                    w.AddComponent<BoxCollider>().size = new Vector3(seg, Mathf.Max(5f, h), 1f);
                }
            }
            else
            {
                float half = a.size * 0.5f;
                Vector3[] pos = { new Vector3(0, 0, half + 0.5f), new Vector3(0, 0, -half - 0.5f), new Vector3(half + 0.5f, 0, 0), new Vector3(-half - 0.5f, 0, 0) };
                Vector3[] size = { new Vector3(a.size + 2, 5, 1), new Vector3(a.size + 2, 5, 1), new Vector3(1, 5, a.size + 2), new Vector3(1, 5, a.size + 2) };
                for (int i = 0; i < 4; i++)
                {
                    var w = new GameObject("WallCollider");
                    w.transform.SetParent(root, false);
                    w.transform.localPosition = pos[i] + Vector3.up * 2.5f;
                    w.AddComponent<BoxCollider>().size = size[i];
                }
            }
            foreach (var p in a.pillars)
            {
                var c = new GameObject("PillarCollider");
                c.transform.SetParent(root, false);
                c.transform.localPosition = p + Vector3.up * 2.5f;
                c.AddComponent<BoxCollider>().size = new Vector3(1.3f, 5f, 1.3f);
            }
        }

        // ------------------------------------------------------------------ Posadzka

        static bool Inside(ArenaDefinition a, float x, float z, float margin)
        {
            if (a.circular) return x * x + z * z < (a.size - margin) * (a.size - margin);
            float half = a.size * 0.5f - margin;
            return Mathf.Abs(x) < half && Mathf.Abs(z) < half;
        }

        static void Floor(ArenaDefinition a, ArenaStyle style, Transform v)
        {
            Color fc = a.floorColor;
            float extent = a.size + 1.5f;
            // Fugi / ziemia pod płytami
            P(v, Box, Surface.Stone, fc * 0.45f, new Vector3(0, -0.2f, 0), new Vector3(extent * 2f, 0.2f, extent * 2f));

            float step = style == ArenaStyle.Crypt ? 1.6f : 1.25f;
            int n = Mathf.CeilToInt(extent / step);
            for (int ix = -n; ix <= n; ix++)
                for (int iz = -n; iz <= n; iz++)
                {
                    float x = ix * step + (iz % 2 == 0 ? 0f : step * 0.5f * (style == ArenaStyle.Crypt ? 0f : 1f)), z = iz * step;
                    if (!Inside(a, x, z, 0.35f)) continue;
                    if (rng.NextDouble() < 0.05)
                    {
                        // Brakująca płyta: dziura z gruzem
                        for (int k = 0; k < 3; k++)
                            P(v, ProcMesh.Sphere(), Surface.Stone, Vary(fc * 0.8f), new Vector3(x + R(-0.4f, 0.4f), -0.12f, z + R(-0.4f, 0.4f)), new Vector3(R(0.15f, 0.35f), R(0.08f, 0.15f), R(0.15f, 0.3f)), new Vector3(0, R(0, 180), 0));
                        continue;
                    }
                    bool crypt = style == ArenaStyle.Crypt;
                    Color tile = Vary(crypt && ((ix + iz) & 1) == 0 ? fc * 0.85f : fc);
                    float s = step - 0.07f;
                    P(v, ProcMesh.Frustum(0.96f, 0.96f), Surface.Stone, tile, new Vector3(x + R(-0.02f, 0.02f), -0.075f + R(-0.012f, 0.012f), z + R(-0.02f, 0.02f)),
                      new Vector3(s + R(-0.04f, 0.02f), 0.15f, s + R(-0.04f, 0.02f)), new Vector3(R(-0.8f, 0.8f), R(-2.5f, 2.5f), R(-0.8f, 0.8f)));
                    if (crypt && rng.NextDouble() < 0.12)
                    {
                        // Płyta nagrobna z krzyżem
                        P(v, Box, Surface.Stone, fc * 0.6f, new Vector3(x, 0.006f, z), new Vector3(0.08f, 0.01f, s * 0.6f));
                        P(v, Box, Surface.Stone, fc * 0.6f, new Vector3(x, 0.006f, z + s * 0.12f), new Vector3(s * 0.4f, 0.01f, 0.08f));
                    }
                }

            if (a.circular)
            {
                // Obwódka z łukowych bloków
                int ring = Mathf.RoundToInt(a.size * 4f);
                for (int i = 0; i < ring; i++)
                {
                    float ang = (i + 0.5f) / ring * 360f;
                    float rad = ang * Mathf.Deg2Rad;
                    float len = 2f * Mathf.PI * (a.size - 0.3f) / ring - 0.05f;
                    P(v, Box, Surface.Stone, Vary(a.wallColor * 0.95f), new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * (a.size - 0.3f) + Vector3.up * -0.03f, new Vector3(len, 0.2f, 0.6f), new Vector3(0, ang, 0));
                }
            }

            // Pęknięcia
            for (int i = 0; i < 18; i++)
            {
                float x = R(-a.size * 0.8f, a.size * 0.8f), z = R(-a.size * 0.8f, a.size * 0.8f);
                if (!Inside(a, x, z, 1f)) continue;
                float yaw = R(0, 180);
                for (int k = 0; k < 3; k++)
                {
                    yaw += R(-35f, 35f);
                    P(v, Box, Surface.Dark, fc * 0.3f, new Vector3(x, 0.004f, z), new Vector3(0.012f, 0.008f, R(0.3f, 0.6f)), new Vector3(0, yaw, 0));
                    x += Mathf.Sin(yaw * Mathf.Deg2Rad) * 0.35f; z += Mathf.Cos(yaw * Mathf.Deg2Rad) * 0.35f;
                }
            }

            // Mozaika w centrum (dziedziniec i szczyt)
            if (style != ArenaStyle.Crypt)
            {
                Color inlay = style == ArenaStyle.Summit ? new Color(0.45f, 0.08f, 0.08f) : new Color(0.55f, 0.45f, 0.3f);
                for (int r = 1; r <= 3; r++)
                    P(v, ProcMesh.Torus(0.06f), Surface.Stone, r == 2 ? inlay : fc * 0.6f, new Vector3(0, 0.005f, 0), new Vector3(r * 3.2f, 0.12f, r * 3.2f));
                for (int i = 0; i < 16; i++)
                    P(v, Box, Surface.Stone, i % 2 == 0 ? inlay : fc * 0.6f, new Vector3(0, 0.006f, 0), new Vector3(0.12f, 0.012f, i % 2 == 0 ? 9.4f : 6.2f), new Vector3(0, i * 22.5f, 0));
                for (int k = 0; k < 2; k++)
                    P(v, Box, Surface.Gold, new Color(0.7f, 0.56f, 0.25f), new Vector3(0, 0.008f, 0), new Vector3(1.4f, 0.014f, 1.4f), new Vector3(0, 45f * k, 0));
                P(v, ProcMesh.Dome(), Surface.Stone, inlay, new Vector3(0, 0f, 0), new Vector3(0.8f, 0.05f, 0.8f));
            }
        }

        // ------------------------------------------------------------------ Mury

        static void Masonry(Transform v, Color wall, float length, Vector3 center, float yaw, float height, float depth)
        {
            int rows = Mathf.RoundToInt(height / 0.55f);
            float rowH = height / rows;
            for (int r = 0; r < rows; r++)
            {
                float x = -length * 0.5f + (r % 2 == 0 ? 0f : -0.6f);
                while (x < length * 0.5f)
                {
                    float bl = R(0.9f, 1.6f);
                    float x0 = Mathf.Max(x, -length * 0.5f), x1 = Mathf.Min(x + bl, length * 0.5f);
                    if (x1 - x0 > 0.15f)
                    {
                        Vector3 local = new Vector3((x0 + x1) * 0.5f, r * rowH + rowH * 0.5f, R(-0.03f, 0.03f));
                        Vector3 world = center + Quaternion.Euler(0, yaw, 0) * local;
                        P(v, Box, Surface.Stone, Vary(wall), world, new Vector3(x1 - x0 - 0.05f, rowH - 0.05f, depth + R(-0.05f, 0.05f)), new Vector3(0, yaw, 0));
                    }
                    x += bl;
                }
            }
            // Zaprawa za blokami
            P(v, Box, Surface.Stone, wall * 0.35f, center + Vector3.up * height * 0.5f, new Vector3(length, height, depth * 0.55f), new Vector3(0, yaw, 0));
        }

        static void CircularWall(ArenaDefinition a, ArenaStyle style, Transform v, Transform props)
        {
            float rw = a.size + 0.9f;
            bool summit = style == ArenaStyle.Summit;
            float h = summit ? 1.25f : WallHeight;
            int segs = Mathf.RoundToInt(2f * Mathf.PI * rw / 2.4f);
            for (int i = 0; i < segs; i++)
            {
                float ang = (i + 0.5f) / segs * 360f;
                float rad = ang * Mathf.Deg2Rad;
                Vector3 c = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * rw;
                float len = 2f * Mathf.PI * rw / segs + 0.05f;
                // Ściana zwrócona do środka: yaw = ang + 180
                if (!summit && Mathf.Abs(Mathf.DeltaAngle(ang, 180f)) < 8f) continue; // miejsce na bramę
                Masonry(v, a.wallColor, len, c, ang + 180f, h, 0.9f);
                // Blanki
                if (i % 2 == 0)
                    P(v, Box, Surface.Stone, Vary(a.wallColor * 1.05f), c + Vector3.up * (h + 0.35f), new Vector3(len * 0.55f, 0.7f, 0.95f), new Vector3(0, ang, 0));
                P(v, Box, Surface.Stone, a.wallColor * 1.1f, c + Vector3.up * (h + 0.04f), new Vector3(len + 0.02f, 0.08f, 1.05f), new Vector3(0, ang, 0));
            }

            if (summit) return;

            // Przypory z pochodniami i sztandary
            int buttresses = 12;
            for (int i = 0; i < buttresses; i++)
            {
                float ang = i / (float)buttresses * 360f + 15f;
                if (Mathf.Abs(Mathf.DeltaAngle(ang, 180f)) < 20f) continue;
                float rad = ang * Mathf.Deg2Rad;
                Vector3 dirOut = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
                Vector3 c = dirOut * (rw - 0.55f);
                P(v, Box, Surface.Stone, Vary(a.wallColor * 1.08f), c + Vector3.up * (h + 0.6f) * 0.5f, new Vector3(1.0f, h + 0.6f, 0.7f), new Vector3(0, ang, 0));
                P(v, Box, Surface.Stone, a.wallColor * 1.15f, c + Vector3.up * 0.3f, new Vector3(1.3f, 0.6f, 1.0f), new Vector3(0, ang, 0));
                P(v, ProcMesh.Frustum(0.6f, 0.5f), Surface.Stone, a.wallColor * 1.1f, c + Vector3.up * (h + 0.85f), new Vector3(1.1f, 0.5f, 0.8f), new Vector3(0, ang, 0));
                for (int k = 1; k < 5; k++)
                    P(v, Box, Surface.Stone, a.wallColor * 0.6f, c - dirOut * 0.36f + Vector3.up * (k * 1.1f), new Vector3(1.02f, 0.04f, 0.04f), new Vector3(0, ang, 0));
                if (i % 2 == 0) Torch(v, props, c - dirOut * 0.45f + Vector3.up * 2.4f, -dirOut);
                else Banner(v, (c - dirOut * 0.38f) + Vector3.up * (h - 0.3f), -dirOut, ang, new Color(0.45f, 0.08f, 0.08f));
            }
            Gate(v, a, rw);
        }

        static void SquareWall(ArenaDefinition a, ArenaStyle style, Transform v, Transform props)
        {
            float half = a.size * 0.5f + 0.9f;
            for (int side = 0; side < 4; side++)
            {
                float yaw = side * 90f;
                Vector3 inward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
                Vector3 c = -inward * half;
                Masonry(v, a.wallColor, a.size + 2.4f, c, yaw, WallHeight, 0.9f);
                // Pilastry i nisze
                for (int k = -2; k <= 2; k++)
                {
                    Vector3 along = Quaternion.Euler(0, yaw, 0) * Vector3.right;
                    Vector3 p = c + along * (k * a.size / 5f) + inward * 0.45f;
                    P(v, Box, Surface.Stone, Vary(a.wallColor * 1.1f), p + Vector3.up * WallHeight * 0.5f, new Vector3(0.6f, WallHeight, 0.4f), new Vector3(0, yaw, 0));
                    P(v, Box, Surface.Stone, a.wallColor * 1.15f, p + Vector3.up * 0.25f, new Vector3(0.8f, 0.5f, 0.55f), new Vector3(0, yaw, 0));
                    P(v, Box, Surface.Stone, a.wallColor * 1.15f, p + Vector3.up * (WallHeight - 0.15f), new Vector3(0.8f, 0.3f, 0.55f), new Vector3(0, yaw, 0));
                    if (k < 2)
                    {
                        Vector3 mid = c + along * ((k + 0.5f) * a.size / 5f) + inward * 0.42f;
                        Arch(v, mid + Vector3.up * 2.2f, yaw, 1.2f, a.wallColor * 1.05f);
                        P(v, Box, Surface.Dark, new Color(0.03f, 0.03f, 0.035f), mid + Vector3.up * 1.4f - inward * 0.3f, new Vector3(2.0f, 2.8f, 0.05f), new Vector3(0, yaw, 0));
                        if ((k + side) % 2 == 0) Sarcophagus(v, mid + inward * 0.9f, yaw, a.wallColor);
                        else Chain(v, mid + Vector3.up * 3.6f + inward * 0.1f, 9);
                    }
                }
                P(v, Box, Surface.Stone, a.wallColor * 1.1f, c + Vector3.up * (WallHeight + 0.1f) + inward * 0.1f, new Vector3(a.size + 2.6f, 0.25f, 1.2f), new Vector3(0, yaw, 0));
            }
        }

        static void Arch(Transform v, Vector3 center, float yaw, float radius, Color c)
        {
            int n = 11;
            for (int i = 0; i < n; i++)
            {
                float t = (i + 0.5f) / n * 180f;
                float rad = t * Mathf.Deg2Rad;
                Vector3 local = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);
                P(v, Box, Surface.Stone, Vary(c), center + Quaternion.Euler(0, yaw, 0) * local, new Vector3(0.36f, 0.3f, 0.5f), new Vector3(0, yaw, t - 90f));
            }
            P(v, Box, Surface.Stone, c * 1.1f, center + Quaternion.Euler(0, yaw, 0) * new Vector3(0, radius, 0), new Vector3(0.3f, 0.42f, 0.56f), new Vector3(0, yaw, 0));
        }

        static void Gate(Transform v, ArenaDefinition a, float rw)
        {
            Vector3 c = new Vector3(0, 0, -rw);
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 p = c + new Vector3(s * 2.1f, 0, 0);
                P(v, Box, Surface.Stone, a.wallColor * 1.12f, p + Vector3.up * 3f, new Vector3(1.2f, 6f, 1.4f));
                P(v, Box, Surface.Stone, a.wallColor * 1.2f, p + Vector3.up * 0.35f, new Vector3(1.5f, 0.7f, 1.7f));
                P(v, ProcMesh.Frustum(0.5f, 0.5f), Surface.Stone, a.wallColor * 1.15f, p + Vector3.up * 6.3f, new Vector3(1.3f, 0.6f, 1.5f));
            }
            Arch(v, c + Vector3.up * 3.2f, 0f, 1.6f, a.wallColor * 1.1f);
            P(v, Box, Surface.Stone, a.wallColor, c + Vector3.up * 5.4f, new Vector3(3.2f, 1.6f, 1f));
            P(v, Box, Surface.Dark, new Color(0.02f, 0.02f, 0.025f), c + Vector3.up * 1.9f - Vector3.forward * 0.3f, new Vector3(3.2f, 3.8f, 0.1f));
            Color iron = new Color(0.18f, 0.18f, 0.2f);
            for (int i = 0; i < 8; i++) P(v, Box, Surface.DarkMetal, iron, c + new Vector3(-1.4f + i * 0.4f, 2.1f, 0.15f), new Vector3(0.06f, 4.2f, 0.06f));
            for (int k = 0; k < 5; k++) P(v, Box, Surface.DarkMetal, iron, c + new Vector3(0, 0.4f + k * 0.9f, 0.18f), new Vector3(3.2f, 0.06f, 0.06f));
            for (int i = 0; i < 8; i++) P(v, ProcMesh.Cone(), Surface.DarkMetal, iron, c + new Vector3(-1.4f + i * 0.4f, 0.05f, 0.15f), new Vector3(0.08f, 0.15f, 0.08f), new Vector3(180f, 0, 0));
        }

        // ------------------------------------------------------------------ Elementy

        static void Column(Transform v, Vector3 pos, float height, Color c, bool broken)
        {
            float h = broken ? height * R(0.45f, 0.65f) : height;
            P(v, Box, Surface.Stone, c * 1.05f, pos + Vector3.up * 0.2f, new Vector3(1.45f, 0.4f, 1.45f));
            P(v, Box, Surface.Stone, c * 1.1f, pos + Vector3.up * 0.46f, new Vector3(1.25f, 0.12f, 1.25f));
            P(v, ProcMesh.Torus(0.25f), Surface.Stone, c, pos + Vector3.up * 0.6f, new Vector3(1.15f, 0.8f, 1.15f));
            P(v, ProcMesh.Tube(0.9f), Surface.Stone, c, pos + Vector3.up * (0.6f + h * 0.5f), new Vector3(0.95f, h, 0.95f));
            for (int i = 0; i < 16; i++)
            {
                float ang = i / 16f * Mathf.PI * 2f;
                P(v, Box, Surface.Stone, c * 0.8f, pos + new Vector3(Mathf.Cos(ang) * 0.44f, 0.6f + h * 0.5f, Mathf.Sin(ang) * 0.44f), new Vector3(0.05f, h * 0.96f, 0.05f), new Vector3(0, -ang * Mathf.Rad2Deg, 0));
            }
            if (broken)
            {
                for (int k = 0; k < 4; k++)
                    P(v, Box, Surface.Stone, c * 0.9f, pos + new Vector3(R(-0.25f, 0.25f), 0.6f + h + R(-0.1f, 0.15f), R(-0.25f, 0.25f)), new Vector3(R(0.3f, 0.6f), R(0.15f, 0.4f), R(0.3f, 0.6f)), new Vector3(R(-30, 30), R(0, 90), R(-30, 30)));
                Vector3 dir = Quaternion.Euler(0, R(0, 360), 0) * Vector3.forward;
                for (int k = 0; k < 2; k++)
                    P(v, ProcMesh.Tube(0.95f), Surface.Stone, c * 0.95f, pos + dir * (1.4f + k * 1.1f) + Vector3.up * 0.45f, new Vector3(0.9f, 1f, 0.9f), new Vector3(90f, Vector3.SignedAngle(Vector3.forward, dir, Vector3.up), R(-10, 10)));
            }
            else
            {
                P(v, ProcMesh.Torus(0.25f), Surface.Stone, c, pos + Vector3.up * (0.6f + h - 0.1f), new Vector3(1.0f, 0.6f, 1.0f));
                P(v, ProcMesh.Frustum(1.4f, 1.4f), Surface.Stone, c * 1.05f, pos + Vector3.up * (0.6f + h + 0.2f), new Vector3(0.95f, 0.4f, 0.95f));
                P(v, Box, Surface.Stone, c * 1.1f, pos + Vector3.up * (0.6f + h + 0.5f), new Vector3(1.45f, 0.2f, 1.45f));
            }
        }

        static void Torch(Transform v, Transform props, Vector3 pos, Vector3 inward)
        {
            Color iron = new Color(0.2f, 0.19f, 0.2f);
            float yaw = Quaternion.LookRotation(inward).eulerAngles.y;
            P(v, Box, Surface.DarkMetal, iron, pos - inward * 0.05f, new Vector3(0.18f, 0.28f, 0.05f), new Vector3(0, yaw, 0));
            P(v, Box, Surface.DarkMetal, iron, pos + inward * 0.18f - Vector3.up * 0.05f, new Vector3(0.05f, 0.05f, 0.4f), new Vector3(-25f, yaw, 0));
            P(v, ProcMesh.Tube(1.4f), Surface.DarkMetal, iron, pos + inward * 0.35f + Vector3.up * 0.12f, new Vector3(0.12f, 0.12f, 0.12f));
            P(v, ProcMesh.Tube(0.8f), Surface.Wood, new Color(0.35f, 0.22f, 0.12f), pos + inward * 0.35f + Vector3.up * 0.28f, new Vector3(0.06f, 0.35f, 0.06f));
            P(v, ProcMesh.Sphere(), Surface.Dark, new Color(0.1f, 0.06f, 0.04f), pos + inward * 0.35f + Vector3.up * 0.47f, new Vector3(0.1f, 0.12f, 0.1f));
            FxLibrary.Fire(pos + inward * 0.35f + Vector3.up * 0.55f, 1.2f, props);
        }

        static void Banner(Transform v, Vector3 top, Vector3 inward, float ang, Color cloth)
        {
            Color gold = new Color(0.75f, 0.6f, 0.25f);
            float yaw = Quaternion.LookRotation(inward).eulerAngles.y;
            P(v, ProcMesh.Tube(1f), Surface.DarkMetal, new Color(0.25f, 0.24f, 0.25f), top + inward * 0.1f, new Vector3(0.05f, 1.3f, 0.05f), new Vector3(0, yaw, 90f));
            for (int s = -1; s <= 1; s += 2)
                P(v, ProcMesh.Sphere(), Surface.Gold, gold, top + inward * 0.1f + Quaternion.Euler(0, yaw, 0) * new Vector3(s * 0.66f, 0, 0), Vector3.one * 0.09f);
            Vector3 cpos = top + inward * 0.12f - Vector3.up * 1.2f;
            P(v, ProcMesh.Frustum(1.15f, 1f), Surface.Cloth, cloth, cpos, new Vector3(1.0f, 2.3f, 0.03f), new Vector3(0, yaw, 0));
            Vector3 side = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            for (int s = -1; s <= 1; s += 2)
                P(v, Box, Surface.Gold, gold, cpos + side * s * 0.53f, new Vector3(0.05f, 2.3f, 0.04f), new Vector3(0, yaw, 0));
            // Herb: krzyż i tarcza
            P(v, ProcMesh.Extrude("heater", ProcMesh.HeaterOutline()), Surface.Cloth, cloth * 0.6f, cpos + Vector3.up * 0.2f + inward * 0.02f, new Vector3(0.55f, 0.6f, 0.02f), new Vector3(0, yaw, 0));
            P(v, Box, Surface.Gold, gold, cpos + Vector3.up * 0.15f + inward * 0.035f, new Vector3(0.07f, 0.55f, 0.02f), new Vector3(0, yaw, 0));
            P(v, Box, Surface.Gold, gold, cpos + Vector3.up * 0.3f + inward * 0.035f, new Vector3(0.38f, 0.07f, 0.02f), new Vector3(0, yaw, 0));
            for (int i = 0; i < 5; i++)
                P(v, ProcMesh.Cone(), Surface.Cloth, cloth, cpos - Vector3.up * 1.22f + side * (-0.4f + i * 0.2f), new Vector3(0.2f, 0.25f, 0.03f), new Vector3(180f, yaw, 0));
        }

        static void Sarcophagus(Transform v, Vector3 pos, float yaw, Color c)
        {
            Quaternion q = Quaternion.Euler(0, yaw + 90f, 0);
            P(v, ProcMesh.Frustum(0.95f, 0.95f), Surface.Stone, c * 1.1f, pos + Vector3.up * 0.4f, new Vector3(0.9f, 0.8f, 2.2f), new Vector3(0, yaw + 90f, 0));
            P(v, ProcMesh.Frustum(0.85f, 0.95f), Surface.Stone, c * 1.2f, pos + Vector3.up * 0.88f, new Vector3(1.0f, 0.16f, 2.3f), new Vector3(0, yaw + 90f, 0));
            // Wyrzeźbiona postać na wieku
            P(v, ProcMesh.Sphere(), Surface.Stone, c * 1.3f, pos + Vector3.up * 1.05f + q * new Vector3(0, 0, 0.8f), new Vector3(0.22f, 0.2f, 0.25f));
            P(v, ProcMesh.Frustum(0.8f, 0.8f), Surface.Stone, c * 1.28f, pos + Vector3.up * 1.03f + q * new Vector3(0, 0, 0.05f), new Vector3(0.42f, 0.14f, 1.2f), new Vector3(0, yaw + 90f, 0));
            P(v, Box, Surface.Stone, c * 1.35f, pos + Vector3.up * 1.12f + q * new Vector3(0, 0, 0.2f), new Vector3(0.05f, 0.05f, 0.7f), new Vector3(0, yaw + 90f, 0));
            P(v, Box, Surface.Stone, c * 1.35f, pos + Vector3.up * 1.12f + q * new Vector3(0, 0, 0.35f), new Vector3(0.3f, 0.05f, 0.05f), new Vector3(0, yaw + 90f, 0));
            for (int k = -1; k <= 1; k += 2)
                P(v, Box, Surface.Stone, c * 0.8f, pos + Vector3.up * 0.4f + q * new Vector3(0.46f * k, 0, 0), new Vector3(0.02f, 0.5f, 1.8f), new Vector3(0, yaw + 90f, 0));
            // Świece na narożnikach
            for (int k = 0; k < 3; k++)
            {
                Vector3 cp = pos + q * new Vector3(R(-0.3f, 0.3f), 0.96f, R(-1f, -0.7f));
                float ch = R(0.1f, 0.25f);
                P(v, ProcMesh.Tube(0.95f), Surface.Bone, new Color(0.9f, 0.86f, 0.75f), cp + Vector3.up * ch * 0.5f, new Vector3(0.05f, ch, 0.05f));
                FxLibrary.Particles(cp + Vector3.up * (ch + 0.04f), new FxLibrary.Emit { rate = 12, duration = 1f, life = new Vector2(0.2f, 0.3f), speed = new Vector2(0.1f, 0.25f), size = new Vector2(0.05f, 0.08f), colorA = new Color(1f, 0.6f, 0.2f), colorB = new Color(1f, 0.9f, 0.5f), radius = 0.005f, sizeGrow = 0.3f }, v.parent.Find("Props")).LoopForever();
            }
            var candleLight = FxLibrary.PointLight(pos + Vector3.up * 1.4f, new Color(1f, 0.65f, 0.3f), 1.2f, 5f, 0f, v.parent.Find("Props"));
            if (candleLight != null) candleLight.gameObject.AddComponent<FxFlicker>().baseIntensity = 1.2f;
        }

        static void Chain(Transform v, Vector3 top, int links)
        {
            Color iron = new Color(0.22f, 0.21f, 0.22f);
            for (int i = 0; i < links; i++)
                P(v, ProcMesh.Torus(0.22f), Surface.DarkMetal, iron, top - Vector3.up * i * 0.13f, new Vector3(0.09f, 0.5f, 0.16f), new Vector3(90f, i % 2 == 0 ? 0f : 90f, 0));
            P(v, ProcMesh.Torus(0.2f), Surface.DarkMetal, iron, top - Vector3.up * links * 0.13f - Vector3.up * 0.05f, new Vector3(0.28f, 0.4f, 0.28f));
        }

        static void Skull(Transform v, Vector3 pos, float yaw)
        {
            Color bone = new Color(0.8f, 0.76f, 0.64f);
            P(v, ProcMesh.Sphere(), Surface.Bone, bone, pos + Vector3.up * 0.1f, new Vector3(0.18f, 0.17f, 0.21f), new Vector3(0, yaw, 0));
            Vector3 f = Quaternion.Euler(0, yaw, 0) * Vector3.forward, r = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            P(v, ProcMesh.Frustum(0.8f, 0.8f), Surface.Bone, bone * 0.95f, pos + Vector3.up * 0.035f + f * 0.05f, new Vector3(0.12f, 0.06f, 0.12f), new Vector3(0, yaw, 0));
            for (int s = -1; s <= 1; s += 2)
                P(v, ProcMesh.Sphere(), Surface.Dark, new Color(0.05f, 0.04f, 0.03f), pos + Vector3.up * 0.11f + f * 0.09f + r * s * 0.045f, new Vector3(0.05f, 0.045f, 0.03f));
        }

        static void Debris(ArenaDefinition a, Transform v)
        {
            for (int i = 0; i < 60; i++)
            {
                float ang = R(0, Mathf.PI * 2f);
                float dist = a.circular ? a.size - R(0.4f, 2.2f) : 0f;
                Vector3 p = a.circular ? new Vector3(Mathf.Cos(ang) * dist, 0, Mathf.Sin(ang) * dist)
                    : new Vector3(R(-a.size * 0.48f, a.size * 0.48f), 0, (rng.NextDouble() < 0.5 ? -1 : 1) * (a.size * 0.5f - R(0.3f, 1.8f)));
                if (!a.circular && rng.NextDouble() < 0.5) p = new Vector3(p.z, 0, p.x);
                float s = R(0.08f, 0.45f);
                P(v, rng.NextDouble() < 0.5 ? ProcMesh.Sphere() : Box, Surface.Stone, Vary(a.wallColor * 0.9f), p + Vector3.up * s * 0.3f, new Vector3(s, s * R(0.4f, 0.8f), s * R(0.6f, 1.2f)), new Vector3(R(-20, 20), R(0, 360), R(-20, 20)));
            }
            Color bone = new Color(0.78f, 0.74f, 0.62f);
            for (int i = 0; i < 10; i++)
            {
                Vector3 p = new Vector3(R(-a.size * 0.6f, a.size * 0.6f), 0.03f, R(-a.size * 0.6f, a.size * 0.6f));
                if (!Inside(a, p.x, p.z, 1.5f)) continue;
                Skull(v, p, R(0, 360));
                for (int k = 0; k < 3; k++)
                    P(v, ProcMesh.Limb(0.7f), Surface.Bone, bone, p + new Vector3(R(-0.5f, 0.5f), 0.03f, R(-0.5f, 0.5f)), new Vector3(0.05f, R(0.25f, 0.45f), 0.05f), new Vector3(90f, R(0, 360), 0));
            }
        }

        static void Statue(Transform v, Vector3 pos, float yaw, Color stone, WeaponModel weapon)
        {
            var holder = new GameObject("Statue");
            holder.transform.SetParent(v, false);
            holder.transform.localPosition = pos + Vector3.up * 1.01f;
            holder.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            P(v, Box, Surface.Stone, stone * 0.85f, pos + Vector3.up * 0.45f, new Vector3(1.6f, 0.9f, 1.6f), new Vector3(0, yaw, 0));
            P(v, Box, Surface.Stone, stone * 0.95f, pos + Vector3.up * 0.95f, new Vector3(1.75f, 0.12f, 1.75f), new Vector3(0, yaw, 0));
            var look = new RigLook { body = BodyGear.Plate, head = HeadGear.GreatHelm, cape = true, pauldrons = true, weapon = weapon, shield = ShieldModel.Heater, stone = true, stoneColor = stone };
            var rig = HumanoidRig.Create(holder.transform, look, 1.35f);
            var anim = new ProceduralHumanoidAnimator(rig, holder.transform);
            anim.Tick(new CharacterAnimState { action = AnimAction.Kneel, actionTime = 0f }, Vector3.zero, 1f);
        }

        // ------------------------------------------------------------------ Style

        static void CourtyardProps(ArenaDefinition a, Transform v, Transform props)
        {
            // Posągi klęczących rycerzy po bokach bramy
            for (int s = -1; s <= 1; s += 2)
                Statue(v, new Vector3(s * 4.5f, 0, -a.size + 1.8f), 0f, a.wallColor * 1.25f, WeaponModel.Sword);
            // Studnia / fontanna w centrum pola? – zostawiamy środek wolny do walki. Kosze z gruzem przy murach:
            for (int i = 0; i < 4; i++)
            {
                float ang = (i * 90f + 45f) * Mathf.Deg2Rad;
                Vector3 p = new Vector3(Mathf.Sin(ang), 0, Mathf.Cos(ang)) * (a.size - 1.6f);
                P(v, ProcMesh.Tube(1.1f), Surface.Wood, new Color(0.35f, 0.24f, 0.14f), p + Vector3.up * 0.4f, new Vector3(0.7f, 0.8f, 0.7f));
                for (int k = 0; k < 3; k++)
                    P(v, ProcMesh.Torus(0.12f), Surface.DarkMetal, new Color(0.25f, 0.24f, 0.25f), p + Vector3.up * (0.12f + k * 0.28f), new Vector3(0.74f + k * 0.03f, 0.3f, 0.74f + k * 0.03f));
                P(v, ProcMesh.Dome(), Surface.Stone, a.wallColor * 0.8f, p + Vector3.up * 0.8f, new Vector3(0.6f, 0.3f, 0.6f));
            }
        }

        static void CryptProps(ArenaDefinition a, Transform v, Transform props)
        {
            // Kandelabry w narożnikach
            float half = a.size * 0.5f - 1.3f;
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = new Vector3(i < 2 ? -half : half, 0, i % 2 == 0 ? -half : half);
                Color iron = new Color(0.2f, 0.19f, 0.2f);
                P(v, ProcMesh.Tube(0.3f), Surface.DarkMetal, iron, p + Vector3.up * 0.9f, new Vector3(0.12f, 1.8f, 0.12f));
                for (int k = 0; k < 3; k++)
                {
                    float ang = k * 120f;
                    P(v, Box, Surface.DarkMetal, iron, p + Quaternion.Euler(0, ang, 0) * new Vector3(0, 0.1f, 0.25f), new Vector3(0.05f, 0.05f, 0.55f), new Vector3(35f, ang, 0));
                }
                P(v, ProcMesh.Torus(0.12f), Surface.DarkMetal, iron, p + Vector3.up * 1.8f, new Vector3(0.7f, 0.3f, 0.7f));
                for (int k = 0; k < 5; k++)
                {
                    float ang = k * 72f * Mathf.Deg2Rad;
                    Vector3 cp = p + new Vector3(Mathf.Cos(ang) * 0.3f, 1.85f, Mathf.Sin(ang) * 0.3f);
                    float ch = R(0.12f, 0.28f);
                    P(v, ProcMesh.Tube(0.95f), Surface.Bone, new Color(0.9f, 0.86f, 0.75f), cp + Vector3.up * ch * 0.5f, new Vector3(0.06f, ch, 0.06f));
                    FxLibrary.Particles(cp + Vector3.up * (ch + 0.04f), new FxLibrary.Emit { rate = 12, duration = 1f, life = new Vector2(0.2f, 0.3f), speed = new Vector2(0.1f, 0.25f), size = new Vector2(0.05f, 0.09f), colorA = new Color(1f, 0.6f, 0.2f), colorB = new Color(1f, 0.9f, 0.5f), radius = 0.005f, sizeGrow = 0.3f }, props).LoopForever();
                }
                var l = FxLibrary.PointLight(p + Vector3.up * 2.3f, new Color(1f, 0.65f, 0.3f), 1.8f, 7f, 0f, props);
                if (l != null) l.gameObject.AddComponent<FxFlicker>().baseIntensity = 1.8f;
                // Pajęczyny
                for (int k = 0; k < 4; k++)
                    P(v, Box, Surface.Cloth, new Color(0.7f, 0.7f, 0.68f), new Vector3(Mathf.Sign(p.x) * (a.size * 0.5f + 0.35f), 4.4f - k * 0.1f, Mathf.Sign(p.z) * (a.size * 0.5f + 0.35f)), new Vector3(1.4f - k * 0.3f, 0.008f, 0.008f), new Vector3(0, 45f * Mathf.Sign(p.x * p.z), 0));
            }
        }

        static void SummitProps(ArenaDefinition a, Transform v, Transform props)
        {
            // Koksowniki
            for (int i = 0; i < 4; i++)
            {
                float ang = (i * 90f + 45f) * Mathf.Deg2Rad;
                Vector3 p = new Vector3(Mathf.Sin(ang), 0, Mathf.Cos(ang)) * (a.size - 2.4f);
                Color iron = new Color(0.16f, 0.15f, 0.16f);
                for (int k = 0; k < 3; k++)
                {
                    float la = k * 120f;
                    P(v, ProcMesh.Tube(0.6f), Surface.DarkMetal, iron, p + Quaternion.Euler(0, la, 0) * new Vector3(0, 0.55f, 0.25f), new Vector3(0.08f, 1.2f, 0.08f), new Vector3(-15f, la, 0));
                }
                P(v, ProcMesh.Shell(), Surface.DarkMetal, iron, p + Vector3.up * 1.1f, new Vector3(1.1f, 0.7f, 1.1f), new Vector3(180f, 0, 0));
                P(v, ProcMesh.Torus(0.1f), Surface.DarkMetal, iron * 1.3f, p + Vector3.up * 1.12f, new Vector3(1.12f, 0.4f, 1.12f));
                for (int k = 0; k < 7; k++)
                    P(v, ProcMesh.Sphere(), Surface.Glow, new Color(1f, 0.35f, 0.08f), p + new Vector3(R(-0.3f, 0.3f), 1.1f, R(-0.3f, 0.3f)), Vector3.one * R(0.12f, 0.22f));
                FxLibrary.Fire(p + Vector3.up * 1.2f, 2.2f, props);
            }
            // Obeliski z runami
            for (int i = 0; i < 8; i++)
            {
                float ang = (i * 45f + 22.5f) * Mathf.Deg2Rad;
                Vector3 p = new Vector3(Mathf.Sin(ang), 0, Mathf.Cos(ang)) * (a.size - 0.9f);
                P(v, Box, Surface.Stone, a.wallColor * 0.9f, p + Vector3.up * 0.3f, new Vector3(1.1f, 0.6f, 1.1f));
                P(v, ProcMesh.Frustum(0.55f, 0.55f), Surface.Stone, a.wallColor * 0.8f, p + Vector3.up * 2.5f, new Vector3(0.75f, 3.8f, 0.75f));
                P(v, ProcMesh.Cone(), Surface.Stone, a.wallColor * 0.85f, p + Vector3.up * 4.65f, new Vector3(0.42f, 0.5f, 0.42f));
                for (int k = 0; k < 4; k++)
                    P(v, Box, Surface.Glow, new Color(1f, 0.2f, 0.1f), p + Vector3.up * (1.2f + k * 0.7f) - p.normalized * 0.33f, new Vector3(0.12f, 0.12f, 0.02f), new Vector3(0, Mathf.Atan2(p.x, p.z) * Mathf.Rad2Deg, 45f));
            }
            // Posągi bezgłowych strażników
            for (int i = 0; i < 2; i++)
                Statue(v, new Vector3((i == 0 ? -1 : 1) * 6f, 0, a.size - 3f), 180f, a.wallColor * 1.4f, WeaponModel.GreatSword);
            // Rytualny krąg w centrum (trwały, powoli się obraca)
            var glyph = FxLibrary.Decal(Vector3.zero, FxMaterials.Glyph, new Color(0.8f, 0.12f, 0.08f), 9f, 9f, 0f, 6f);
            if (glyph != null) glyph.transform.SetParent(props, true);
            // Panorama: odległe iglice i księżyc
            for (int i = 0; i < 16; i++)
            {
                float ang = i / 16f * Mathf.PI * 2f + R(-0.1f, 0.1f);
                float dist = R(45f, 75f);
                Vector3 p = new Vector3(Mathf.Sin(ang), 0, Mathf.Cos(ang)) * dist;
                float h = R(18f, 45f);
                Color sil = new Color(0.08f, 0.04f, 0.05f);
                P(v, Box, Surface.Stone, sil, p + Vector3.up * (h * 0.5f - 12f), new Vector3(R(3f, 6f), h, R(3f, 6f)));
                P(v, ProcMesh.Cone(), Surface.Stone, sil, p + Vector3.up * (h - 12f + 4f), new Vector3(R(4f, 7f), 8f, R(4f, 7f)));
                for (int k = 0; k < 3; k++)
                    P(v, Box, Surface.Glow, new Color(1f, 0.55f, 0.2f), p + Vector3.up * R(0f, h - 16f) - p.normalized * 2.6f, new Vector3(0.4f, 0.8f, 0.1f), new Vector3(0, Mathf.Atan2(p.x, p.z) * Mathf.Rad2Deg, 0));
            }
            P(v, ProcMesh.Sphere(), Surface.Glow, new Color(0.95f, 0.8f, 0.7f), new Vector3(-25f, 38f, 70f), Vector3.one * 9f);
        }
    }

    public static class ParticleSystemExtensions
    {
        /// <summary>Zamienia jednorazową emisję w zapętloną (świece, pochodnie).</summary>
        public static ParticleSystem LoopForever(this ParticleSystem ps)
        {
            if (ps == null) return null;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;
            main.stopAction = ParticleSystemStopAction.None;
            ps.Play();
            return ps;
        }
    }
}
