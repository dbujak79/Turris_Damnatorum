using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Szczegółowe modele broni i tarcz z siatek proceduralnych – wspólne dla humanoida proceduralnego i modeli FBX.
    /// Broń: rękojeść w (0,0,0), ostrze wzdłuż lokalnego +Z. Tarcza: lico skierowane w lokalne +Z.
    /// Części są scalane w jedną siatkę na obiekt.
    /// </summary>
    public static class GearBuilder
    {
        static readonly Color GripLeather = new Color(0.28f, 0.17f, 0.1f);
        static readonly Color Wood = new Color(0.4f, 0.27f, 0.16f);
        static readonly Color Gold = new Color(0.8f, 0.64f, 0.26f);
        static readonly Color Steel = new Color(0.72f, 0.73f, 0.76f);

        static void P(Transform t, Mesh m, Surface s, Color c, Vector3 pos, Vector3 scale, Vector3 euler = default)
            => PartBuilder.Add(t, m, s, c, pos, scale, euler);

        /// <summary>Kąty Eulera obracające oś +Y części na kierunek <paramref name="dir"/> (np. dla zakrzywionych szponów).</summary>
        static Vector3 AlongY(Vector3 dir) => Quaternion.FromToRotation(Vector3.up, dir.normalized).eulerAngles;

        static void Grip(Transform p, float z0, float length, float diameter, Color c)
        {
            P(p, ProcMesh.Tube(0.92f), Surface.Leather, c, new Vector3(0, 0, z0 + length * 0.5f), new Vector3(diameter, length, diameter), new Vector3(90f, 0, 0));
            int rings = Mathf.Max(3, Mathf.RoundToInt(length / 0.035f));
            for (int i = 0; i < rings; i++)
                P(p, ProcMesh.Torus(0.3f), Surface.Leather, c * 0.75f, new Vector3(0, 0, z0 + (i + 0.5f) * length / rings), new Vector3(diameter * 1.12f, 0.2f, diameter * 1.12f), new Vector3(90f, 0, 0));
        }

        public static GameObject Weapon(Transform parent, WeaponModel model, Color color)
        {
            var go = new GameObject("Weapon");
            go.transform.SetParent(parent, false);
            var p = go.transform;
            Color metal = color;
            switch (model)
            {
                case WeaponModel.Sword:
                    Grip(p, -0.08f, 0.17f, 0.032f, GripLeather);
                    P(p, ProcMesh.Sphere(), Surface.Gold, Gold, new Vector3(0, 0, -0.105f), new Vector3(0.048f, 0.048f, 0.055f));
                    P(p, ProcMesh.Cone(), Surface.Gold, Gold, new Vector3(0, 0, -0.14f), new Vector3(0.02f, 0.03f, 0.02f), new Vector3(-90f, 0, 0));
                    P(p, ProcMesh.Frustum(1f, 1f), Surface.Gold, Gold, new Vector3(0, 0, 0.1f), new Vector3(0.2f, 0.028f, 0.034f));
                    P(p, ProcMesh.Box(), Surface.Gold, Gold * 0.9f, new Vector3(0, 0, 0.11f), new Vector3(0.05f, 0.042f, 0.04f));
                    for (int s = -1; s <= 1; s += 2)
                        P(p, ProcMesh.Sphere(), Surface.Gold, Gold, new Vector3(s * 0.105f, 0, 0.098f), Vector3.one * 0.034f);
                    P(p, ProcMesh.Blade(0.16f, 0.88f), Surface.Metal, metal, new Vector3(0, 0, 0.125f), new Vector3(0.055f, 0.014f, 0.84f));
                    P(p, ProcMesh.Box(), Surface.DarkMetal, metal * 0.6f, new Vector3(0, 0, 0.44f), new Vector3(0.012f, 0.016f, 0.52f));
                    P(p, ProcMesh.Box(), Surface.Gold, Gold, new Vector3(0, 0, 0.16f), new Vector3(0.035f, 0.017f, 0.02f));
                    break;

                case WeaponModel.GreatSword:
                    Grip(p, -0.2f, 0.34f, 0.042f, GripLeather * 0.8f);
                    P(p, ProcMesh.Sphere(), Surface.DarkMetal, metal * 0.8f, new Vector3(0, 0, -0.23f), new Vector3(0.07f, 0.07f, 0.08f));
                    P(p, ProcMesh.Frustum(1f, 1f), Surface.DarkMetal, metal * 0.85f, new Vector3(0, 0, 0.17f), new Vector3(0.34f, 0.045f, 0.055f));
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(p, ProcMesh.Box(), Surface.DarkMetal, metal * 0.85f, new Vector3(s * 0.2f, 0, 0.21f), new Vector3(0.1f, 0.035f, 0.035f), new Vector3(0, s * 35f, 0));
                        P(p, ProcMesh.Cone(), Surface.DarkMetal, metal * 0.85f, new Vector3(s * 0.245f, 0, 0.25f), new Vector3(0.03f, 0.07f, 0.03f), new Vector3(90f, s * 35f, 0));
                    }
                    P(p, ProcMesh.Box(), Surface.Metal, metal * 0.9f, new Vector3(0, 0, 0.29f), new Vector3(0.085f, 0.024f, 0.2f));
                    P(p, ProcMesh.Blade(0.12f, 0.85f), Surface.Metal, metal, new Vector3(0, 0, 0.3f), new Vector3(0.11f, 0.022f, 1.32f));
                    P(p, ProcMesh.Box(), Surface.DarkMetal, metal * 0.55f, new Vector3(0, 0, 0.8f), new Vector3(0.022f, 0.025f, 0.9f));
                    for (int i = 0; i < 3; i++) P(p, ProcMesh.Box(), Surface.Glow, new Color(1f, 0.35f, 0.15f), new Vector3(0, 0.012f, 0.45f + i * 0.2f), new Vector3(0.03f, 0.004f, 0.05f), new Vector3(0, 0, 45f));
                    break;

                case WeaponModel.Axe:
                case WeaponModel.GreatAxe:
                {
                    bool great = model == WeaponModel.GreatAxe;
                    float len = great ? 1.35f : 0.85f;
                    float headZ = great ? 1.0f : 0.62f;
                    float hs = great ? 1.35f : 1f;
                    P(p, ProcMesh.Tube(0.85f), Surface.Wood, Wood, new Vector3(0, 0, len * 0.5f - 0.2f), new Vector3(0.042f * hs, len, 0.042f * hs), new Vector3(-90f, 0, 0));
                    Grip(p, -0.12f, great ? 0.4f : 0.2f, 0.046f * hs, GripLeather);
                    for (int i = 0; i < 3; i++)
                        P(p, ProcMesh.Torus(0.3f), Surface.Metal, metal * 0.8f, new Vector3(0, 0, headZ - 0.12f - i * 0.05f), new Vector3(0.05f * hs, 0.3f, 0.05f * hs), new Vector3(90f, 0, 0));
                    P(p, ProcMesh.Box(), Surface.Metal, metal * 0.85f, new Vector3(0, 0, headZ), new Vector3(0.06f * hs, 0.07f * hs, 0.11f * hs));
                    var axe = ProcMesh.Extrude("axe", ProcMesh.AxeOutline());
                    P(p, axe, Surface.Metal, metal, new Vector3(0, 0.03f * hs, headZ), new Vector3(0.24f * hs, 0.26f * hs, 0.018f * hs), new Vector3(0, 90f, 0));
                    P(p, axe, Surface.Metal, metal * 1.15f, new Vector3(0, 0.03f * hs, headZ), new Vector3(0.25f * hs, 0.275f * hs, 0.008f * hs), new Vector3(0, 90f, 0));
                    if (great)
                    {
                        P(p, axe, Surface.Metal, metal, new Vector3(0, -0.03f * hs, headZ), new Vector3(0.24f * hs, 0.26f * hs, 0.018f * hs), new Vector3(0, 90f, 180f));
                        P(p, ProcMesh.Cone(), Surface.Metal, metal, new Vector3(0, 0, headZ + 0.14f), new Vector3(0.04f, 0.16f, 0.04f), new Vector3(90f, 0, 0));
                    }
                    else P(p, ProcMesh.Cone(), Surface.Metal, metal * 0.9f, new Vector3(0, -0.08f, headZ), new Vector3(0.035f, 0.1f, 0.035f), new Vector3(180f, 0, 0));
                    P(p, ProcMesh.Sphere(), Surface.Metal, metal * 0.8f, new Vector3(0, 0, -0.2f), new Vector3(0.05f * hs, 0.05f * hs, 0.05f * hs));
                    break;
                }

                case WeaponModel.Dagger:
                    Grip(p, -0.06f, 0.11f, 0.028f, GripLeather);
                    P(p, ProcMesh.Sphere(), Surface.Gold, Gold, new Vector3(0, 0, -0.075f), Vector3.one * 0.035f);
                    P(p, ProcMesh.Frustum(1f, 1f), Surface.Gold, Gold, new Vector3(0, 0, 0.06f), new Vector3(0.12f, 0.022f, 0.025f));
                    for (int s = -1; s <= 1; s += 2)
                        P(p, ProcMesh.Sphere(), Surface.Gold, Gold, new Vector3(s * 0.065f, 0, 0.06f), Vector3.one * 0.022f);
                    P(p, ProcMesh.Blade(0.3f, 0.9f), Surface.Metal, metal, new Vector3(0, 0, 0.075f), new Vector3(0.038f, 0.011f, 0.32f));
                    P(p, ProcMesh.Box(), Surface.DarkMetal, metal * 0.6f, new Vector3(0, 0, 0.17f), new Vector3(0.008f, 0.013f, 0.16f));
                    break;

                case WeaponModel.Staff:
                {
                    P(p, ProcMesh.Tube(0.75f), Surface.Wood, Wood, new Vector3(0, 0, 0.3f), new Vector3(0.05f, 1.6f, 0.05f), new Vector3(90f, 0, 0));
                    for (int i = 0; i < 5; i++)
                        P(p, ProcMesh.Sphere(), Surface.Wood, Wood * 0.8f, new Vector3((i % 2 == 0 ? 1 : -1) * 0.012f, (i % 3 - 1) * 0.01f, -0.35f + i * 0.28f), new Vector3(0.06f, 0.055f, 0.07f));
                    Grip(p, -0.1f, 0.22f, 0.052f, new Color(0.35f, 0.12f, 0.1f));
                    P(p, ProcMesh.Cone(), Surface.Metal, Steel * 0.8f, new Vector3(0, 0, -0.52f), new Vector3(0.05f, 0.08f, 0.05f), new Vector3(-90f, 0, 0));
                    P(p, ProcMesh.Torus(0.3f), Surface.Gold, Gold, new Vector3(0, 0, 1.03f), new Vector3(0.07f, 0.3f, 0.07f), new Vector3(90f, 0, 0));
                    P(p, ProcMesh.Torus(0.3f), Surface.Gold, Gold, new Vector3(0, 0, 1.07f), new Vector3(0.06f, 0.3f, 0.06f), new Vector3(90f, 0, 0));
                    for (int i = 0; i < 3; i++)
                    {
                        float a = i / 3f * Mathf.PI * 2f;
                        Vector3 radial = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
                        // Szpony trzymające kulę: odchylone na zewnątrz, potem zagięte do środka.
                        P(p, ProcMesh.Limb(0.6f), Surface.Gold, Gold * 0.9f, new Vector3(0, 0, 1.12f) + radial * 0.05f, new Vector3(0.018f, 0.11f, 0.018f), AlongY(Vector3.forward + radial * 0.8f));
                        P(p, ProcMesh.Cone(), Surface.Gold, Gold * 0.9f, new Vector3(0, 0, 1.24f) + radial * 0.075f, new Vector3(0.016f, 0.07f, 0.016f), AlongY(Vector3.forward - radial * 0.7f));
                    }
                    P(p, ProcMesh.Sphere(), Surface.Glow, new Color(0.35f, 0.6f, 1f), new Vector3(0, 0, 1.19f), Vector3.one * 0.125f);
                    P(p, ProcMesh.Sphere(), Surface.Glow, new Color(0.85f, 0.95f, 1f), new Vector3(0, 0, 1.19f), Vector3.one * 0.06f);
                    P(p, ProcMesh.Box(), Surface.Cloth, new Color(0.35f, 0.12f, 0.1f), new Vector3(0.04f, 0, 0.93f), new Vector3(0.012f, 0.03f, 0.16f), new Vector3(0, 12f, 0));
                    P(p, ProcMesh.Box(), Surface.Cloth, new Color(0.3f, 0.1f, 0.08f), new Vector3(-0.035f, 0.01f, 0.9f), new Vector3(0.012f, 0.03f, 0.2f), new Vector3(0, -8f, 0));
                    break;
                }

                case WeaponModel.Halberd:
                {
                    P(p, ProcMesh.Tube(0.9f), Surface.Wood, Wood, new Vector3(0, 0, 0.4f), new Vector3(0.05f, 2.0f, 0.05f), new Vector3(90f, 0, 0));
                    Grip(p, -0.15f, 0.3f, 0.054f, GripLeather);
                    for (int i = 0; i < 4; i++)
                        P(p, ProcMesh.Torus(0.3f), Surface.Metal, metal * 0.8f, new Vector3(0, 0, 0.25f + i * 0.3f), new Vector3(0.058f, 0.3f, 0.058f), new Vector3(90f, 0, 0));
                    P(p, ProcMesh.Box(), Surface.Metal, metal * 0.85f, new Vector3(0, 0, 1.3f), new Vector3(0.065f, 0.07f, 0.22f));
                    var axe = ProcMesh.Extrude("axe", ProcMesh.AxeOutline());
                    P(p, axe, Surface.Metal, metal, new Vector3(0, 0.035f, 1.27f), new Vector3(0.36f, 0.3f, 0.022f), new Vector3(0, 90f, 0));
                    P(p, ProcMesh.Cone(), Surface.Metal, metal, new Vector3(0, -0.12f, 1.28f), new Vector3(0.04f, 0.18f, 0.04f), new Vector3(170f, 0, 0));
                    P(p, ProcMesh.Blade(0.35f, 0.9f), Surface.Metal, metal, new Vector3(0, 0, 1.4f), new Vector3(0.06f, 0.03f, 0.38f));
                    P(p, ProcMesh.Cone(), Surface.Cloth, new Color(0.55f, 0.1f, 0.08f), new Vector3(0, 0, 1.12f), new Vector3(0.09f, 0.14f, 0.09f), new Vector3(-90f, 0, 0));
                    P(p, ProcMesh.Cone(), Surface.Metal, metal * 0.8f, new Vector3(0, 0, -0.62f), new Vector3(0.05f, 0.1f, 0.05f), new Vector3(-90f, 0, 0));
                    break;
                }
            }
            PartBuilder.BakeAll(p);
            return go;
        }

        public static GameObject Shield(Transform parent, ShieldModel model, Color color)
        {
            var go = new GameObject("Shield");
            go.transform.SetParent(parent, false);
            var p = go.transform;
            switch (model)
            {
                case ShieldModel.Heater:
                {
                    var outline = ProcMesh.HeaterOutline();
                    var heater = ProcMesh.Extrude("heater", outline);
                    // Metalowe obramowanie (większy obrys z tyłu), drewniane lico, pole herbu, krzyż, umbo, nity.
                    P(p, heater, Surface.Metal, Steel * 0.85f, new Vector3(0, 0.02f, 0.025f), new Vector3(0.54f, 0.57f, 0.03f));
                    P(p, heater, Surface.Wood, color, new Vector3(0, 0.02f, 0.04f), new Vector3(0.5f, 0.53f, 0.03f));
                    P(p, heater, Surface.Cloth, color * 0.55f + new Color(0.25f, 0.02f, 0.02f), new Vector3(0.07f, 0.06f, 0.056f), new Vector3(0.22f, 0.26f, 0.004f));
                    P(p, ProcMesh.Box(), Surface.Gold, Gold, new Vector3(0, -0.02f, 0.06f), new Vector3(0.045f, 0.46f, 0.012f));
                    P(p, ProcMesh.Box(), Surface.Gold, Gold, new Vector3(0, 0.1f, 0.06f), new Vector3(0.4f, 0.045f, 0.012f));
                    P(p, ProcMesh.Dome(), Surface.Metal, Steel, new Vector3(0, 0.1f, 0.06f), new Vector3(0.1f, 0.05f, 0.1f), new Vector3(90f, 0, 0));
                    foreach (var pt in outline)
                    {
                        Vector2 o = pt * 0.9f;
                        P(p, ProcMesh.Sphere(), Surface.Metal, Steel, new Vector3(o.x * 0.5f, 0.02f + o.y * 0.53f, 0.058f), Vector3.one * 0.018f);
                    }
                    for (int s = -1; s <= 1; s += 2)
                        P(p, ProcMesh.Box(), Surface.Leather, GripLeather, new Vector3(s * 0.08f, 0.02f, 0.005f), new Vector3(0.035f, 0.3f, 0.015f));
                    P(p, ProcMesh.Box(), Surface.Leather, GripLeather * 0.8f, new Vector3(0, 0.05f, -0.005f), new Vector3(0.22f, 0.035f, 0.02f));
                    break;
                }
                case ShieldModel.Buckler:
                    P(p, ProcMesh.Dome(), Surface.Metal, color, new Vector3(0, 0, 0.02f), new Vector3(0.4f, 0.08f, 0.4f), new Vector3(90f, 0, 0));
                    P(p, ProcMesh.Torus(0.1f), Surface.Metal, color * 0.8f, new Vector3(0, 0, 0.025f), new Vector3(0.42f, 0.35f, 0.42f), new Vector3(90f, 0, 0));
                    P(p, ProcMesh.Sphere(), Surface.Metal, color * 1.15f, new Vector3(0, 0, 0.06f), new Vector3(0.1f, 0.1f, 0.07f));
                    P(p, ProcMesh.Cone(), Surface.Metal, color * 1.1f, new Vector3(0, 0, 0.1f), new Vector3(0.04f, 0.06f, 0.04f), new Vector3(90f, 0, 0));
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i / 10f * Mathf.PI * 2f;
                        P(p, ProcMesh.Sphere(), Surface.Gold, Gold, new Vector3(Mathf.Cos(a) * 0.17f, Mathf.Sin(a) * 0.17f, 0.04f), Vector3.one * 0.016f);
                    }
                    P(p, ProcMesh.Box(), Surface.Leather, GripLeather, new Vector3(0, 0, 0.0f), new Vector3(0.14f, 0.03f, 0.03f));
                    break;
                case ShieldModel.Tower:
                {
                    P(p, ProcMesh.Frustum(1f, 1f), Surface.Wood, color, new Vector3(0, -0.1f, 0.05f), new Vector3(0.6f, 1.12f, 0.05f));
                    P(p, ProcMesh.Frustum(0.9f, 1f), Surface.Wood, color * 0.85f, new Vector3(0, -0.1f, 0.078f), new Vector3(0.52f, 1.02f, 0.01f));
                    Color rim = Steel * 0.75f;
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(p, ProcMesh.Box(), Surface.Metal, rim, new Vector3(s * 0.3f, -0.1f, 0.06f), new Vector3(0.035f, 1.14f, 0.065f));
                        P(p, ProcMesh.Box(), Surface.Metal, rim, new Vector3(0, -0.1f + s * 0.56f, 0.06f), new Vector3(0.63f, 0.035f, 0.065f));
                    }
                    for (int k = 0; k < 2; k++) P(p, ProcMesh.Box(), Surface.Metal, rim, new Vector3(0, 0.12f - k * 0.45f, 0.086f), new Vector3(0.56f, 0.05f, 0.015f));
                    P(p, ProcMesh.Box(), Surface.Metal, rim * 1.1f, new Vector3(0, -0.1f, 0.088f), new Vector3(0.06f, 1.02f, 0.015f));
                    P(p, ProcMesh.Dome(), Surface.Metal, Steel, new Vector3(0, -0.02f, 0.09f), new Vector3(0.16f, 0.07f, 0.16f), new Vector3(90f, 0, 0));
                    P(p, ProcMesh.Cone(), Surface.Metal, Steel, new Vector3(0, -0.02f, 0.14f), new Vector3(0.05f, 0.08f, 0.05f), new Vector3(90f, 0, 0));
                    for (int i = 0; i < 8; i++)
                        for (int s = -1; s <= 1; s += 2)
                            P(p, ProcMesh.Sphere(), Surface.Metal, Steel, new Vector3(s * 0.3f, -0.62f + i * 0.145f, 0.095f), Vector3.one * 0.022f);
                    P(p, ProcMesh.Box(), Surface.Gold, Gold, new Vector3(0, 0.3f, 0.09f), new Vector3(0.16f, 0.16f, 0.01f), new Vector3(0, 0, 45f));
                    P(p, ProcMesh.Box(), Surface.Leather, GripLeather, new Vector3(0, 0f, 0.0f), new Vector3(0.25f, 0.04f, 0.03f));
                    break;
                }
            }
            PartBuilder.BakeAll(p);
            return go;
        }
    }
}
