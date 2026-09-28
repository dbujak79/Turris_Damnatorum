using UnityEngine;

namespace Turris
{
    /// <summary>Modele broni i tarcz z prymitywów – wspólne dla humanoida proceduralnego i modeli FBX.</summary>
    public static class GearBuilder
    {
        public delegate void RendererSink(Renderer r, Color color, bool glow);

        /// <summary>Model broni z prymitywów. Ostrze wzdłuż lokalnego +Z, rękojeść w (0,0,0).</summary>
        public static GameObject Weapon(Transform parent, WeaponModel model, Color color, RendererSink sink)
        {
            var weaponObject = new GameObject("Weapon");
            weaponObject.transform.SetParent(parent, false);
            var p = weaponObject.transform;
            Color grip = new Color(0.25f, 0.17f, 0.1f), metal = color, gold = new Color(0.75f, 0.6f, 0.25f);
            switch (model)
            {
                case WeaponModel.Sword:
                    W(p, PrimitiveType.Cylinder, new Vector3(0, 0, 0.0f), new Vector3(0.035f, 0.08f, 0.035f), grip, new Vector3(90, 0, 0));
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.09f), new Vector3(0.2f, 0.035f, 0.04f), gold);
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.53f), new Vector3(0.06f, 0.014f, 0.86f), metal);
                    W(p, PrimitiveType.Sphere, new Vector3(0, 0, -0.1f), new Vector3(0.05f, 0.05f, 0.05f), gold);
                    break;
                case WeaponModel.GreatSword:
                    W(p, PrimitiveType.Cylinder, new Vector3(0, 0, 0.0f), new Vector3(0.045f, 0.16f, 0.045f), grip, new Vector3(90, 0, 0));
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.17f), new Vector3(0.34f, 0.05f, 0.06f), gold);
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.9f), new Vector3(0.11f, 0.02f, 1.4f), metal);
                    break;
                case WeaponModel.Axe:
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.25f), new Vector3(0.04f, 0.04f, 0.8f), grip);
                    W(p, PrimitiveType.Cube, new Vector3(0, 0.09f, 0.58f), new Vector3(0.025f, 0.2f, 0.2f), metal);
                    break;
                case WeaponModel.GreatAxe:
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.35f), new Vector3(0.05f, 0.05f, 1.3f), grip);
                    W(p, PrimitiveType.Cube, new Vector3(0, 0.14f, 0.9f), new Vector3(0.03f, 0.32f, 0.34f), metal);
                    W(p, PrimitiveType.Cube, new Vector3(0, -0.08f, 0.9f), new Vector3(0.03f, 0.14f, 0.18f), metal);
                    break;
                case WeaponModel.Dagger:
                    W(p, PrimitiveType.Cylinder, new Vector3(0, 0, 0f), new Vector3(0.03f, 0.06f, 0.03f), grip, new Vector3(90, 0, 0));
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.07f), new Vector3(0.12f, 0.025f, 0.03f), gold);
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.24f), new Vector3(0.04f, 0.012f, 0.32f), metal);
                    break;
                case WeaponModel.Staff:
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.3f), new Vector3(0.045f, 0.045f, 1.6f), grip * 1.3f);
                    W(p, PrimitiveType.Sphere, new Vector3(0, 0, 1.15f), new Vector3(0.14f, 0.14f, 0.14f), new Color(0.45f, 0.65f, 1f), default, true);
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 1.07f), new Vector3(0.1f, 0.1f, 0.03f), gold);
                    break;
                case WeaponModel.Halberd:
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 0.4f), new Vector3(0.05f, 0.05f, 2.0f), grip);
                    W(p, PrimitiveType.Cube, new Vector3(0, 0.13f, 1.25f), new Vector3(0.03f, 0.26f, 0.3f), metal);
                    W(p, PrimitiveType.Cube, new Vector3(0, 0, 1.5f), new Vector3(0.03f, 0.05f, 0.35f), metal);
                    break;
            }
            return weaponObject;

            void W(Transform par, PrimitiveType type, Vector3 pos, Vector3 scale, Color c, Vector3 euler = default, bool glow = false)
                => Part(par, type, pos, scale, c, euler, glow, sink);
        }

        /// <summary>Model tarczy; lico skierowane w lokalne +Z.</summary>
        public static GameObject Shield(Transform parent, ShieldModel model, Color color, RendererSink sink)
        {
            var shieldObject = new GameObject("Shield");
            shieldObject.transform.SetParent(parent, false);
            var p = shieldObject.transform;
            switch (model)
            {
                case ShieldModel.Heater:
                    Part(p, PrimitiveType.Cube, new Vector3(0, 0, 0.04f), new Vector3(0.5f, 0.62f, 0.05f), color, default, false, sink);
                    Part(p, PrimitiveType.Cube, new Vector3(0, -0.3f, 0.04f), new Vector3(0.33f, 0.33f, 0.05f), color, new Vector3(0, 0, 45f), false, sink);
                    Part(p, PrimitiveType.Cube, new Vector3(0, 0.02f, 0.07f), new Vector3(0.08f, 0.5f, 0.02f), new Color(0.8f, 0.65f, 0.25f), default, false, sink);
                    Part(p, PrimitiveType.Cube, new Vector3(0, 0.1f, 0.07f), new Vector3(0.36f, 0.07f, 0.02f), new Color(0.8f, 0.65f, 0.25f), default, false, sink);
                    break;
                case ShieldModel.Buckler:
                    Part(p, PrimitiveType.Cylinder, new Vector3(0, 0, 0.03f), new Vector3(0.4f, 0.02f, 0.4f), color, new Vector3(90, 0, 0), false, sink);
                    Part(p, PrimitiveType.Sphere, new Vector3(0, 0, 0.05f), new Vector3(0.12f, 0.12f, 0.08f), color * 1.2f, default, false, sink);
                    break;
                case ShieldModel.Tower:
                    Part(p, PrimitiveType.Cube, new Vector3(0, -0.1f, 0.05f), new Vector3(0.62f, 1.15f, 0.07f), color, default, false, sink);
                    Part(p, PrimitiveType.Cube, new Vector3(0, -0.1f, 0.09f), new Vector3(0.5f, 1.0f, 0.02f), color * 0.8f, default, false, sink);
                    break;
            }
            return shieldObject;
        }

        static void Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Color color, Vector3 euler, bool glow, RendererSink sink)
        {
            var go = GameObject.CreatePrimitive(type);
            Util.DestroySafe(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            if (Application.isPlaying)
            {
                r.material.color = color;
                if (glow) VisualFx.SetEmission(r, color * 1.5f);
            }
            sink?.Invoke(r, color, glow);
        }
    }
}
