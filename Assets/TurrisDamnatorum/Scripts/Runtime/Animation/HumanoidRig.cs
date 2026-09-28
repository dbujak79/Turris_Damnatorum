using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    public enum Bone { Hips, Spine, Chest, Neck, Head, UpperArmL, ForearmL, HandL, UpperArmR, ForearmR, HandR, ThighL, ShinL, FootL, ThighR, ShinR, FootR }

    /// <summary>
    /// Proceduralny humanoid złożony z brył zawieszonych na hierarchii kości.
    /// Konwencja: kości kończyn w pozycji spoczynkowej wskazują w dół (lokalne −Y), postać patrzy w +Z.
    /// Wysokość bazowa ≈ 1,8 m; skalę daje VisualRoot.localScale.
    /// </summary>
    public class HumanoidRig : MonoBehaviour
    {
        public Transform VisualRoot { get; private set; }
        public Transform Pivot { get; private set; }
        public Transform WeaponSocket { get; private set; }
        public Transform ShieldSocket { get; private set; }
        public Transform CapePivot { get; private set; }
        public RigLook Look { get; private set; }
        public readonly Transform[] bones = new Transform[17];

        public float HipsHeight { get; private set; } = 0.95f;
        public float UpperArmLength { get; private set; } = 0.29f;
        public float ForearmLength { get; private set; } = 0.29f;
        public float LegLength { get; private set; } = 0.85f;
        public float RestHunch { get; private set; }

        readonly List<Renderer> bodyRenderers = new List<Renderer>();
        readonly List<Color> bodyColors = new List<Color>();
        readonly List<Renderer> weaponRenderers = new List<Renderer>();
        readonly List<Color> weaponColors = new List<Color>();
        readonly List<Renderer> glowRenderers = new List<Renderer>();
        GameObject weaponObject, shieldObject, flaskObject;

        public Transform this[Bone b] => bones[(int)b];

        public static HumanoidRig Create(Transform character, RigLook look, float scale)
        {
            var old = character.GetComponentInChildren<HumanoidRig>();
            if (old != null) Util.DestroySafe(old.gameObject);

            var go = new GameObject("Rig");
            go.transform.SetParent(character, false);
            var rig = go.AddComponent<HumanoidRig>();
            rig.VisualRoot = go.transform;
            rig.VisualRoot.localScale = Vector3.one * scale;
            rig.Build(look);
            return rig;
        }

        // ------------------------------------------------------------------ Budowa

        Transform MakeBone(Bone b, Transform parent, Vector3 localPos)
        {
            var t = new GameObject(b.ToString()).transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            bones[(int)b] = t;
            return t;
        }

        GameObject Seg(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Color color, Vector3 euler = default, bool glow = false)
        {
            var go = GameObject.CreatePrimitive(type);
            Util.DestroySafe(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            if (Application.isPlaying) r.material.color = color;
            bodyRenderers.Add(r);
            bodyColors.Add(color);
            if (glow) { glowRenderers.Add(r); if (Application.isPlaying) VisualFx.SetEmission(r, color * 1.5f); }
            return go;
        }

        void Build(RigLook look)
        {
            Look = look;
            float w = look.bulk;
            float t = look.limbThickness;
            UpperArmLength = 0.29f * look.armLength;
            ForearmLength = 0.29f * look.armLength;
            RestHunch = look.hunch;
            bool robe = look.body == BodyGear.Robe;
            bool rags = look.body == BodyGear.Rags;
            Color torsoColor = look.body == BodyGear.Plate ? look.armor : look.body == BodyGear.Chain ? look.armor : robe ? look.cloth : rags ? look.skin : look.cloth;
            Color limbColor = look.body == BodyGear.Plate ? look.armor : rags ? look.skin : look.body == BodyGear.Chain ? look.armor * 0.9f : look.cloth * 0.85f;
            Color legColor = look.body == BodyGear.Plate ? look.armor : rags ? look.skin : look.leather;
            limbColor.a = legColor.a = torsoColor.a = 1f;

            Pivot = new GameObject("Pivot").transform;
            Pivot.SetParent(VisualRoot, false);
            Pivot.localPosition = new Vector3(0, HipsHeight, 0);

            var hips = MakeBone(Bone.Hips, Pivot, Vector3.zero);
            var spine = MakeBone(Bone.Spine, hips, new Vector3(0, 0.12f, 0));
            var chest = MakeBone(Bone.Chest, spine, new Vector3(0, 0.2f, 0));
            var neck = MakeBone(Bone.Neck, chest, new Vector3(0, 0.25f, 0));
            var head = MakeBone(Bone.Head, neck, new Vector3(0, 0.1f, 0));
            float sx = 0.22f * w;
            var uaL = MakeBone(Bone.UpperArmL, chest, new Vector3(-sx, 0.2f, 0));
            var faL = MakeBone(Bone.ForearmL, uaL, new Vector3(0, -UpperArmLength, 0));
            var hL = MakeBone(Bone.HandL, faL, new Vector3(0, -ForearmLength, 0));
            var uaR = MakeBone(Bone.UpperArmR, chest, new Vector3(sx, 0.2f, 0));
            var faR = MakeBone(Bone.ForearmR, uaR, new Vector3(0, -UpperArmLength, 0));
            var hR = MakeBone(Bone.HandR, faR, new Vector3(0, -ForearmLength, 0));
            var thL = MakeBone(Bone.ThighL, hips, new Vector3(-0.11f * w, -0.05f, 0));
            var shL = MakeBone(Bone.ShinL, thL, new Vector3(0, -0.43f, 0));
            var ftL = MakeBone(Bone.FootL, shL, new Vector3(0, -0.42f, 0));
            var thR = MakeBone(Bone.ThighR, hips, new Vector3(0.11f * w, -0.05f, 0));
            var shR = MakeBone(Bone.ShinR, thR, new Vector3(0, -0.43f, 0));
            var ftR = MakeBone(Bone.FootR, shR, new Vector3(0, -0.42f, 0));

            // Tułów
            Seg(hips, PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(0.34f * w, 0.18f, 0.21f), rags ? look.cloth : legColor);
            Seg(spine, PrimitiveType.Cube, new Vector3(0, 0.1f, 0), new Vector3(0.3f * w, 0.22f, 0.19f), torsoColor);
            Seg(chest, PrimitiveType.Cube, new Vector3(0, 0.13f, 0), new Vector3(0.44f * w, 0.32f, 0.25f), torsoColor);
            Seg(neck, PrimitiveType.Cylinder, new Vector3(0, 0.03f, 0), new Vector3(0.1f, 0.05f, 0.1f), look.skin);

            if (look.body == BodyGear.Plate)
            {
                Seg(chest, PrimitiveType.Cube, new Vector3(0, 0.12f, 0.02f), new Vector3(0.4f * w, 0.28f, 0.26f), look.armor * 1.15f);
                Seg(chest, PrimitiveType.Cube, new Vector3(0, 0.12f, 0.14f), new Vector3(0.05f, 0.24f, 0.02f), look.trim);
                Seg(spine, PrimitiveType.Cube, new Vector3(0, 0.0f, 0.0f), new Vector3(0.36f * w, 0.05f, 0.23f), look.leather);
            }
            if (look.body == BodyGear.Chain || look.body == BodyGear.Plate || look.body == BodyGear.Tunic)
            {
                // Tabard / tunika w barwach właściciela
                Seg(chest, PrimitiveType.Cube, new Vector3(0, 0.05f, 0.135f), new Vector3(0.26f * w, 0.3f, 0.02f), look.cloth);
                Seg(hips, PrimitiveType.Cube, new Vector3(0, -0.2f, 0.115f), new Vector3(0.26f * w, 0.36f, 0.02f), look.cloth);
                Seg(hips, PrimitiveType.Cube, new Vector3(0, -0.2f, -0.115f), new Vector3(0.26f * w, 0.32f, 0.02f), look.cloth * 0.85f);
                Seg(spine, PrimitiveType.Cube, new Vector3(0, -0.02f, 0), new Vector3(0.35f * w, 0.05f, 0.22f), look.leather);
            }
            if (robe)
            {
                Seg(hips, PrimitiveType.Cube, new Vector3(0, -0.36f, 0), new Vector3(0.42f, 0.74f, 0.32f), look.cloth);
                Seg(hips, PrimitiveType.Cube, new Vector3(0, -0.72f, 0), new Vector3(0.48f, 0.06f, 0.38f), look.trim);
                Seg(chest, PrimitiveType.Cube, new Vector3(0, 0.13f, 0.13f), new Vector3(0.08f, 0.3f, 0.02f), look.trim);
                Seg(spine, PrimitiveType.Cube, new Vector3(0, 0.0f, 0), new Vector3(0.32f, 0.05f, 0.21f), look.leather);
            }
            if (rags)
            {
                Seg(hips, PrimitiveType.Cube, new Vector3(0, -0.14f, 0.02f), new Vector3(0.36f, 0.22f, 0.23f), look.cloth);
                // Wystające żebra
                for (int i = 0; i < 3; i++) Seg(chest, PrimitiveType.Cube, new Vector3(0, 0.05f + i * 0.08f, 0.12f), new Vector3(0.36f, 0.025f, 0.03f), look.skin * 0.8f);
            }
            if (look.cape)
            {
                // Peleryna na zawiasie przy barkach – animator kołysze nią w rytm ruchu.
                CapePivot = new GameObject("CapePivot").transform;
                CapePivot.SetParent(chest, false);
                CapePivot.localPosition = new Vector3(0, 0.27f, -0.15f);
                var cape = Seg(CapePivot, PrimitiveType.Cube, new Vector3(0, -0.42f, 0), new Vector3(0.38f * w, 0.84f, 0.025f), look.cloth * 0.75f);
                cape.name = "Cape";
                Seg(CapePivot, PrimitiveType.Cube, new Vector3(0, 0.0f, 0.02f), new Vector3(0.42f * w, 0.06f, 0.05f), look.cloth * 0.6f);
            }

            // Głowa
            BuildHead(head, look);

            // Ręce
            BuildArm(uaL, faL, hL, look, limbColor, t, true);
            BuildArm(uaR, faR, hR, look, limbColor, t, false);

            // Nogi
            BuildLeg(thL, shL, ftL, look, legColor, t);
            BuildLeg(thR, shR, ftR, look, legColor, t);

            // Gniazda broni i tarczy – pozycjonowane co klatkę przez animator (orientacja w przestrzeni postaci).
            WeaponSocket = new GameObject("WeaponSocket").transform;
            WeaponSocket.SetParent(VisualRoot, false);
            ShieldSocket = new GameObject("ShieldSocket").transform;
            ShieldSocket.SetParent(VisualRoot, false);
            SetWeapon(look.weapon, look.weaponColor);
            SetShield(look.shield, look.shieldColor);

            var flask = Seg(hL, PrimitiveType.Cylinder, new Vector3(0, -0.08f, 0.04f), new Vector3(0.07f, 0.07f, 0.07f), new Color(0.9f, 0.55f, 0.15f), default, true);
            flaskObject = flask;
            flaskObject.SetActive(false);
        }

        void BuildHead(Transform head, RigLook look)
        {
            switch (look.head)
            {
                case HeadGear.GreatHelm:
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.1f, 0.01f), new Vector3(0.25f, 0.29f, 0.27f), look.armor * 1.1f);
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.12f, 0.14f), new Vector3(0.2f, 0.025f, 0.01f), new Color(0.05f, 0.05f, 0.05f));
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.05f, 0.14f), new Vector3(0.025f, 0.12f, 0.012f), new Color(0.08f, 0.08f, 0.08f));
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.26f, 0), new Vector3(0.03f, 0.04f, 0.24f), look.trim);
                    break;
                case HeadGear.HornedHelm:
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.1f, 0.01f), new Vector3(0.27f, 0.3f, 0.28f), look.armor);
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.11f, 0.145f), new Vector3(0.2f, 0.03f, 0.01f), look.eyes, default, look.glowingEyes);
                    Seg(head, PrimitiveType.Capsule, new Vector3(0.17f, 0.25f, 0), new Vector3(0.06f, 0.14f, 0.06f), look.trim * 0.8f, new Vector3(0, 0, -35f));
                    Seg(head, PrimitiveType.Capsule, new Vector3(-0.17f, 0.25f, 0), new Vector3(0.06f, 0.14f, 0.06f), look.trim * 0.8f, new Vector3(0, 0, 35f));
                    break;
                case HeadGear.Hood:
                    Seg(head, PrimitiveType.Sphere, new Vector3(0, 0.09f, 0), new Vector3(0.21f, 0.23f, 0.22f), look.skin);
                    Seg(head, PrimitiveType.Sphere, new Vector3(0, 0.12f, -0.02f), new Vector3(0.28f, 0.3f, 0.29f), look.cloth * 0.85f);
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.09f, 0.1f), new Vector3(0.16f, 0.16f, 0.06f), new Color(0.04f, 0.03f, 0.05f));
                    if (look.glowingEyes)
                    {
                        Seg(head, PrimitiveType.Cube, new Vector3(0.04f, 0.1f, 0.135f), new Vector3(0.03f, 0.015f, 0.01f), look.eyes, default, true);
                        Seg(head, PrimitiveType.Cube, new Vector3(-0.04f, 0.1f, 0.135f), new Vector3(0.03f, 0.015f, 0.01f), look.eyes, default, true);
                    }
                    break;
                case HeadGear.Coif:
                    Seg(head, PrimitiveType.Sphere, new Vector3(0, 0.1f, -0.005f), new Vector3(0.25f, 0.27f, 0.26f), look.armor * 0.95f);
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.08f, 0.1f), new Vector3(0.13f, 0.14f, 0.06f), look.skin);
                    Seg(head, PrimitiveType.Cube, new Vector3(0.035f, 0.1f, 0.13f), new Vector3(0.025f, 0.015f, 0.01f), look.eyes);
                    Seg(head, PrimitiveType.Cube, new Vector3(-0.035f, 0.1f, 0.13f), new Vector3(0.025f, 0.015f, 0.01f), look.eyes);
                    Seg(head, PrimitiveType.Cylinder, new Vector3(0, -0.04f, 0), new Vector3(0.24f, 0.05f, 0.24f), look.armor * 0.9f);
                    break;
                case HeadGear.Skull:
                    Seg(head, PrimitiveType.Sphere, new Vector3(0, 0.08f, 0.02f), new Vector3(0.2f, 0.22f, 0.24f), look.skin);
                    Seg(head, PrimitiveType.Cube, new Vector3(0, 0.0f, 0.1f), new Vector3(0.13f, 0.06f, 0.08f), look.skin * 0.8f);
                    Seg(head, PrimitiveType.Cube, new Vector3(0.05f, 0.1f, 0.12f), new Vector3(0.045f, 0.03f, 0.02f), look.eyes, default, true);
                    Seg(head, PrimitiveType.Cube, new Vector3(-0.05f, 0.1f, 0.12f), new Vector3(0.045f, 0.03f, 0.02f), look.eyes, default, true);
                    break;
                default:
                    Seg(head, PrimitiveType.Sphere, new Vector3(0, 0.09f, 0), new Vector3(0.21f, 0.24f, 0.22f), look.skin);
                    Seg(head, PrimitiveType.Sphere, new Vector3(0, 0.14f, -0.02f), new Vector3(0.22f, 0.16f, 0.22f), new Color(0.2f, 0.14f, 0.1f));
                    Seg(head, PrimitiveType.Cube, new Vector3(0.045f, 0.1f, 0.105f), new Vector3(0.03f, 0.02f, 0.01f), look.eyes);
                    Seg(head, PrimitiveType.Cube, new Vector3(-0.045f, 0.1f, 0.105f), new Vector3(0.03f, 0.02f, 0.01f), look.eyes);
                    break;
            }
        }

        void BuildArm(Transform ua, Transform fa, Transform hand, RigLook look, Color color, float t, bool left)
        {
            float d = 0.11f * t;
            Seg(ua, PrimitiveType.Capsule, new Vector3(0, -UpperArmLength * 0.5f, 0), new Vector3(d, UpperArmLength * 0.55f, d), color);
            Seg(fa, PrimitiveType.Capsule, new Vector3(0, -ForearmLength * 0.5f, 0), new Vector3(d * 0.9f, ForearmLength * 0.55f, d * 0.9f), color);
            bool gauntlet = look.body == BodyGear.Plate || look.body == BodyGear.Chain;
            Seg(hand, PrimitiveType.Cube, new Vector3(0, -0.05f, 0), new Vector3(0.085f, 0.11f, 0.09f), gauntlet ? look.armor * 0.9f : look.skin);
            if (gauntlet) Seg(fa, PrimitiveType.Cylinder, new Vector3(0, -ForearmLength * 0.75f, 0), new Vector3(d * 1.15f, 0.06f, d * 1.15f), look.armor);
            if (look.pauldrons)
                Seg(ua, PrimitiveType.Sphere, new Vector3(left ? -0.03f : 0.03f, 0.0f, 0), new Vector3(0.2f * look.bulk, 0.15f, 0.2f), look.armor * 1.1f);
            if (look.body == BodyGear.Robe)
                Seg(fa, PrimitiveType.Cylinder, new Vector3(0, -ForearmLength * 0.35f, 0), new Vector3(0.15f, ForearmLength * 0.3f, 0.15f), look.cloth);
            if (look.weapon == WeaponModel.Claws)
            {
                for (int i = -1; i <= 1; i++)
                    Seg(hand, PrimitiveType.Cube, new Vector3(i * 0.028f, -0.15f, 0.02f), new Vector3(0.015f, 0.14f, 0.015f), look.weaponColor, new Vector3(-15f, 0, 0));
            }
        }

        void BuildLeg(Transform th, Transform sh, Transform ft, RigLook look, Color color, float t)
        {
            float d = 0.15f * t;
            if (look.body == BodyGear.Robe)
            {
                // Szata zakrywa nogi – widać tylko stopy.
                Seg(ft, PrimitiveType.Cube, new Vector3(0, -0.02f, 0.05f), new Vector3(0.1f, 0.07f, 0.24f), look.leather);
                return;
            }
            Seg(th, PrimitiveType.Capsule, new Vector3(0, -0.215f, 0), new Vector3(d, 0.24f, d), color);
            Seg(sh, PrimitiveType.Capsule, new Vector3(0, -0.21f, 0), new Vector3(d * 0.82f, 0.23f, d * 0.82f), color);
            Color boot = look.body == BodyGear.Plate ? look.armor * 0.9f : look.body == BodyGear.Rags ? look.skin * 0.85f : look.leather;
            boot.a = 1f;
            Seg(sh, PrimitiveType.Cylinder, new Vector3(0, -0.3f, 0), new Vector3(d * 0.95f, 0.1f, d * 0.95f), boot);
            Seg(ft, PrimitiveType.Cube, new Vector3(0, -0.02f, 0.05f), new Vector3(0.1f, 0.07f, 0.24f), boot);
            if (look.body == BodyGear.Plate) Seg(sh, PrimitiveType.Sphere, new Vector3(0, 0f, 0.03f), new Vector3(0.12f, 0.1f, 0.1f), look.armor * 1.1f);
        }

        // ------------------------------------------------------------------ Broń i tarcza

        public void SetWeapon(WeaponModel model, Color color)
        {
            if (weaponObject != null) Util.DestroySafe(weaponObject);
            weaponRenderers.Clear(); weaponColors.Clear();
            Look.weapon = model;
            weaponObject = GearBuilder.Weapon(WeaponSocket, model, color, (r, c, glow) =>
            {
                Register(r, c, glow);
                weaponRenderers.Add(r); weaponColors.Add(c);
            });
        }

        public void SetShield(ShieldModel model, Color color)
        {
            if (shieldObject != null) Util.DestroySafe(shieldObject);
            Look.shield = model;
            shieldObject = GearBuilder.Shield(ShieldSocket, model, color, (r, c, glow) => Register(r, c, glow));
        }

        void Register(Renderer r, Color c, bool glow)
        {
            bodyRenderers.Add(r);
            bodyColors.Add(c);
            if (glow) glowRenderers.Add(r);
        }

        public void ShowFlask(bool on) { if (flaskObject != null && flaskObject.activeSelf != on) flaskObject.SetActive(on); }

        // ------------------------------------------------------------------ Kolory (czytelność)

        /// <summary>Zabarwia całą postać (np. błysk parowania, trafienie).</summary>
        public void SetTint(Color c, float amount)
        {
            if (!Application.isPlaying) return;
            for (int i = 0; i < bodyRenderers.Count; i++)
            {
                if (bodyRenderers[i] == null) continue;
                var baseC = bodyColors[i];
                var col = amount <= 0f ? baseC : Color.Lerp(baseC, c, amount);
                bodyRenderers[i].material.color = col;
            }
        }

        /// <summary>Poświata broni – telegraf ataku.</summary>
        public void SetWeaponGlow(Color c, float intensity)
        {
            if (!Application.isPlaying) return;
            for (int i = 0; i < weaponRenderers.Count; i++)
            {
                var r = weaponRenderers[i];
                if (r == null) continue;
                r.material.color = intensity <= 0 ? weaponColors[i] : Color.Lerp(weaponColors[i], c, Mathf.Clamp01(intensity));
                VisualFx.SetEmission(r, intensity <= 0 ? Color.black : c * intensity * 2f);
            }
            if (Look.weapon == WeaponModel.Claws || Look.weapon == WeaponModel.None)
            {
                // Bez broni świecą dłonie i pazury.
                foreach (var b in new[] { Bone.HandL, Bone.HandR })
                    foreach (var r in this[b].GetComponentsInChildren<Renderer>())
                        VisualFx.SetEmission(r, intensity <= 0 ? Color.black : c * intensity * 2f);
            }
        }

        /// <summary>Punkt na końcu broni (do testów i efektów).</summary>
        public Vector3 WeaponTip
        {
            get
            {
                float len;
                switch (Look.weapon)
                {
                    case WeaponModel.GreatSword: len = 1.6f; break;
                    case WeaponModel.Halberd: len = 1.65f; break;
                    case WeaponModel.GreatAxe: len = 1.0f; break;
                    case WeaponModel.Staff: len = 1.2f; break;
                    case WeaponModel.Dagger: len = 0.4f; break;
                    case WeaponModel.None: case WeaponModel.Claws: return this[Bone.HandR].position;
                    default: len = 0.95f; break;
                }
                return WeaponSocket.position + WeaponSocket.forward * len * VisualRoot.lossyScale.x;
            }
        }
    }
}
