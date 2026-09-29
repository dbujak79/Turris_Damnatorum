using System.Linq;
using UnityEngine;

namespace Turris
{
    public enum Bone { Hips, Spine, Chest, Neck, Head, UpperArmL, ForearmL, HandL, UpperArmR, ForearmR, HandR, ThighL, ShinL, FootL, ThighR, ShinR, FootR }

    /// <summary>
    /// Proceduralny humanoid: hierarchia kości obłożona setkami części (siatki proceduralne + materiały PBR),
    /// scalanymi na koniec w jedną siatkę na kość.
    /// Konwencja: kości kończyn w pozycji spoczynkowej wskazują w dół (lokalne −Y), postać patrzy w +Z.
    /// Wysokość bazowa ≈ 1,8 m; skalę daje VisualRoot.localScale.
    /// </summary>
    public class HumanoidRig : MonoBehaviour
    {
        public Transform VisualRoot { get; private set; }
        public Transform Pivot { get; private set; }
        public Transform WeaponSocket { get; private set; }
        public Transform ShieldSocket { get; private set; }
        /// <summary>Gniazdo drugiej broni w lewej dłoni (noże).</summary>
        public Transform OffhandSocket { get; private set; }
        /// <summary>Żywa cięciwa łuku: dwa odcinki od końców ramion do punktu naciągu (null bez łuku).</summary>
        public Transform BowStringUpper { get; private set; }
        public Transform BowStringLower { get; private set; }
        /// <summary>Strzała nałożona na cięciwę w czasie naciągu (ukryta poza nim).</summary>
        public Transform NockedArrow { get; private set; }
        public Transform CapePivot { get; private set; }
        public Transform CapeLower { get; private set; }
        public RigLook Look { get; private set; }
        public readonly Transform[] bones = new Transform[17];

        public float HipsHeight { get; private set; } = 0.95f;
        public float UpperArmLength { get; private set; } = 0.29f;
        public float ForearmLength { get; private set; } = 0.29f;
        public float LegLength { get; private set; } = 0.85f;
        public float RestHunch { get; private set; }
        /// <summary>Liczba części, z których zbudowano postać (przed scaleniem).</summary>
        public int PartCount { get; private set; }

        readonly TintSet bodyTint = new TintSet();
        readonly TintSet weaponTint = new TintSet();
        GameObject weaponObject, shieldObject, flaskObject, offhandObject, quiverObject;

        public Transform this[Bone b] => bones[(int)b];

        public static HumanoidRig Create(Transform character, RigLook look, float scale)
        {
            var old = character.GetComponentInChildren<HumanoidRig>();
            if (old != null) Util.DestroyNow(old.gameObject);

            var go = new GameObject("Rig");
            go.transform.SetParent(character, false);
            var rig = go.AddComponent<HumanoidRig>();
            rig.VisualRoot = go.transform;
            rig.VisualRoot.localScale = Vector3.one * scale;
            rig.Build(look);
            return rig;
        }

        // ------------------------------------------------------------------ Pomocnicze

        Transform MakeBone(Bone b, Transform parent, Vector3 localPos)
        {
            var t = new GameObject(b.ToString()).transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            bones[(int)b] = t;
            return t;
        }

        static Transform Node(string name, Transform parent, Vector3 pos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            return t;
        }

        void P(Transform t, Mesh m, Surface s, Color c, Vector3 pos, Vector3 scale, Vector3 euler = default)
        {
            if (Look != null && Look.stone)
            {
                float lum = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                c = Look.stoneColor * Mathf.Lerp(0.75f, 1.2f, Mathf.Clamp01(lum * 1.5f));
                s = Surface.Stone;
            }
            PartBuilder.Add(t, m, s, c, pos, scale, euler);
            PartCount++;
        }

        static Mesh Box => ProcMesh.Box();
        static Mesh Sph => ProcMesh.Sphere();
        static Mesh Dome => ProcMesh.Dome();
        static Mesh Cone => ProcMesh.Cone();
        static Mesh Tube(float r = 1f) => ProcMesh.Tube(r);
        static Mesh Limb(float t) => ProcMesh.Limb(t);
        static Mesh Fr(float x, float z, float shift = 0f) => ProcMesh.Frustum(x, z, shift);
        static Mesh Bell(float top, float bulge = 0.06f) => ProcMesh.Bell(top, bulge);
        static Mesh Tor(float minor = 0.2f) => ProcMesh.Torus(minor);

        // ------------------------------------------------------------------ Budowa

        void Build(RigLook look)
        {
            Look = look;
            float w = look.bulk;
            UpperArmLength = 0.29f * look.armLength;
            ForearmLength = 0.29f * look.armLength;
            RestHunch = look.hunch;

            Pivot = Node("Pivot", VisualRoot, new Vector3(0, HipsHeight, 0));
            var hips = MakeBone(Bone.Hips, Pivot, Vector3.zero);
            var spine = MakeBone(Bone.Spine, hips, new Vector3(0, 0.12f, 0));
            var chest = MakeBone(Bone.Chest, spine, new Vector3(0, 0.2f, 0));
            var neck = MakeBone(Bone.Neck, chest, new Vector3(0, 0.25f, 0));
            MakeBone(Bone.Head, neck, new Vector3(0, 0.1f, 0));
            float sx = 0.22f * w;
            var uaL = MakeBone(Bone.UpperArmL, chest, new Vector3(-sx, 0.2f, 0));
            var faL = MakeBone(Bone.ForearmL, uaL, new Vector3(0, -UpperArmLength, 0));
            MakeBone(Bone.HandL, faL, new Vector3(0, -ForearmLength, 0));
            var uaR = MakeBone(Bone.UpperArmR, chest, new Vector3(sx, 0.2f, 0));
            var faR = MakeBone(Bone.ForearmR, uaR, new Vector3(0, -UpperArmLength, 0));
            MakeBone(Bone.HandR, faR, new Vector3(0, -ForearmLength, 0));
            var thL = MakeBone(Bone.ThighL, hips, new Vector3(-0.11f * w, -0.05f, 0));
            var shL = MakeBone(Bone.ShinL, thL, new Vector3(0, -0.43f, 0));
            MakeBone(Bone.FootL, shL, new Vector3(0, -0.42f, 0));
            var thR = MakeBone(Bone.ThighR, hips, new Vector3(0.11f * w, -0.05f, 0));
            var shR = MakeBone(Bone.ShinR, thR, new Vector3(0, -0.43f, 0));
            MakeBone(Bone.FootR, shR, new Vector3(0, -0.42f, 0));

            BuildTorso(look);
            BuildHead(look);
            BuildArm(look, true);
            BuildArm(look, false);
            BuildLeg(look, true);
            BuildLeg(look, false);
            if (look.cape) BuildCape(look);
            DetailPass(look);
            DetailPass2(look);

            var flaskNode = Node("Flask", this[Bone.HandL], new Vector3(0, -0.1f, 0.04f));
            P(flaskNode, Sph, Surface.Glow, new Color(0.95f, 0.5f, 0.12f), new Vector3(0, -0.02f, 0), new Vector3(0.065f, 0.08f, 0.065f));
            P(flaskNode, Tube(0.8f), Surface.Metal, new Color(0.7f, 0.72f, 0.75f), new Vector3(0, 0.035f, 0), new Vector3(0.028f, 0.04f, 0.028f));
            P(flaskNode, Tube(0.9f), Surface.Wood, new Color(0.45f, 0.3f, 0.18f), new Vector3(0, 0.062f, 0), new Vector3(0.022f, 0.02f, 0.022f));
            flaskObject = flaskNode.gameObject;

            PartBuilder.BakeAll(VisualRoot);
            flaskObject.SetActive(false);

            WeaponSocket = Node("WeaponSocket", VisualRoot, Vector3.zero);
            ShieldSocket = Node("ShieldSocket", VisualRoot, Vector3.zero);
            OffhandSocket = Node("OffhandSocket", VisualRoot, Vector3.zero);
            SetWeapon(look.weapon, look.weaponColor);
            SetShield(look.shield, look.shieldColor);
        }

        void CollectBodyTint()
        {
            bodyTint.Clear();
            var weaponRenderers = weaponObject != null ? weaponObject.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
            bodyTint.Collect(VisualRoot.GetComponentsInChildren<Renderer>(true).Where(r => !weaponRenderers.Contains(r)));
        }

        // ------------------------------------------------------------------ Tułów

        void BuildTorso(RigLook look)
        {
            float w = look.bulk;
            var hips = this[Bone.Hips];
            var spine = this[Bone.Spine];
            var chest = this[Bone.Chest];
            Color A = look.armor, Ch = look.armor * 0.85f, Cl = look.cloth, Cd = look.cloth * 0.7f, Tr = look.trim, Le = look.leather, Ld = look.leather * 0.7f, Sk = look.skin;
            Color dark = new Color(0.05f, 0.045f, 0.05f);

            // Pusta pochwa miecza po lewej i sztylet w pochwie po prawej (rycerze w kolczudze i zbroi).
            void Sidearms()
            {
                Color sc = Le * 0.8f;
                P(hips, Fr(0.75f, 0.8f), Surface.Leather, sc, new Vector3(-0.2f * w, -0.3f, -0.02f), new Vector3(0.05f, 0.62f, 0.028f), new Vector3(-20f, 0, -6f));
                P(hips, Box, Surface.Metal, look.trim, new Vector3(-0.2f * w, -0.03f, 0.075f), new Vector3(0.056f, 0.06f, 0.034f), new Vector3(-20f, 0, -6f));
                P(hips, Cone, Surface.Metal, look.trim, new Vector3(-0.2f * w, -0.6f, -0.13f), new Vector3(0.04f, 0.07f, 0.025f), new Vector3(160f, 0, -6f));
                for (int k = 0; k < 2; k++) P(hips, Box, Surface.Leather, Le * 0.6f, new Vector3(-0.2f * w, -0.1f - k * 0.18f, 0.05f - k * 0.065f), new Vector3(0.056f, 0.02f, 0.034f), new Vector3(-20f, 0, -6f));
                P(hips, Box, Surface.Leather, sc, new Vector3(0.19f * w, -0.1f, 0.06f), new Vector3(0.035f, 0.2f, 0.022f), new Vector3(-10f, 0, 8f));
                P(hips, Box, Surface.Gold, look.trim, new Vector3(0.188f * w, 0.02f, 0.08f), new Vector3(0.07f, 0.015f, 0.02f), new Vector3(-10f, 0, 8f));
                P(hips, Tube(0.9f), Surface.Leather, GripOf(Le), new Vector3(0.186f * w, 0.07f, 0.088f), new Vector3(0.022f, 0.08f, 0.022f), new Vector3(-10f, 0, 8f));
                P(hips, Sph, Surface.Gold, look.trim, new Vector3(0.184f * w, 0.12f, 0.097f), Vector3.one * 0.025f);
            }
            Color GripOf(Color c) => c * 0.6f;

            // Bogactwo szaty: spodnia szata w rozcięciu, haft na lamówce, frędzle, koraliki, fiolki, tuba na zwoje.
            void RobeDetails()
            {
                Color under = look.style == RigStyle.Heretic ? new Color(0.08f, 0.05f, 0.08f) : new Color(0.75f, 0.72f, 0.62f);
                P(hips, Fr(1.6f, 1f), Surface.Cloth, under, new Vector3(0, -0.45f, 0.2f), new Vector3(0.09f, 0.8f, 0.01f), new Vector3(-8f, 0, 0));
                P(chest, Fr(1f, 1f), Surface.Cloth, under, new Vector3(0, 0.2f, 0.121f), new Vector3(0.05f, 0.12f, 0.008f));
                for (int i = 0; i < 16; i++)
                {
                    float a = i / 16f * 360f;
                    float rad = a * Mathf.Deg2Rad;
                    P(hips, Box, Surface.Gold, Tr, new Vector3(Mathf.Sin(rad) * 0.228f * w, -0.8f, Mathf.Cos(rad) * 0.19f), new Vector3(0.025f, 0.025f, 0.006f), new Vector3(-6f, a, 45f));
                }
                for (int i = 0; i < 14; i++)
                {
                    float a = (i / 13f - 0.5f) * 250f;
                    float rad = a * Mathf.Deg2Rad;
                    P(chest, Box, Surface.Cloth, Tr * 0.9f, new Vector3(Mathf.Sin(rad) * 0.25f * w, 0.175f, Mathf.Cos(rad) * 0.16f), new Vector3(0.012f, 0.05f, 0.012f), new Vector3(0, a, 0));
                }
                for (int i = 0; i < 9; i++)
                {
                    float a = (i / 8f - 0.5f) * 140f * Mathf.Deg2Rad;
                    P(chest, Sph, Surface.Bone, i % 3 == 1 ? new Color(0.6f, 0.15f, 0.12f) : new Color(0.3f, 0.22f, 0.15f), new Vector3(Mathf.Sin(a) * 0.09f, 0.24f - Mathf.Cos(a) * 0.05f, 0.1f + Mathf.Cos(a) * 0.035f), Vector3.one * 0.02f);
                }
                Color[] potions = { new Color(0.9f, 0.2f, 0.15f), new Color(0.2f, 0.5f, 1f), new Color(0.3f, 0.9f, 0.4f) };
                for (int i = 0; i < 3; i++)
                {
                    Vector3 pos = new Vector3(-0.05f - i * 0.045f, -0.07f, 0.12f - i * 0.02f);
                    P(spine, Tube(0.7f), Surface.Glow, potions[i] * 0.8f, pos, new Vector3(0.028f, 0.06f, 0.028f));
                    P(spine, Tube(0.9f), Surface.Wood, new Color(0.5f, 0.35f, 0.2f), pos + new Vector3(0, 0.04f, 0), new Vector3(0.014f, 0.02f, 0.014f));
                }
                P(chest, Tube(1f), Surface.Leather, Le, new Vector3(0.08f, 0.1f, -0.15f), new Vector3(0.07f, 0.42f, 0.07f), new Vector3(0, 0, -25f));
                P(chest, Tube(1f), Surface.Gold, Tr, new Vector3(0.17f, 0.29f, -0.15f), new Vector3(0.075f, 0.03f, 0.075f), new Vector3(0, 0, -25f));
                P(chest, Tube(1f), Surface.Gold, Tr, new Vector3(-0.01f, -0.09f, -0.15f), new Vector3(0.075f, 0.03f, 0.075f), new Vector3(0, 0, -25f));
                P(chest, Box, Surface.Leather, Le * 0.7f, new Vector3(0, 0.12f, -0.125f), new Vector3(0.03f, 0.38f, 0.01f), new Vector3(0, 0, 40f));
            }

            void Belt(Color c)
            {
                P(spine, Tor(0.2f), Surface.Leather, c, new Vector3(0, -0.01f, 0), new Vector3(0.35f * w, 0.25f, 0.245f));
                P(spine, Box, Surface.Gold, Tr, new Vector3(0, -0.01f, 0.123f), new Vector3(0.06f, 0.05f, 0.012f));
                P(spine, Box, Surface.Dark, dark, new Vector3(0, -0.01f, 0.13f), new Vector3(0.035f, 0.028f, 0.006f));
                for (int s = -1; s <= 1; s += 2)
                {
                    P(spine, Fr(0.9f, 0.85f), Surface.Leather, Ld, new Vector3(s * 0.15f * w, -0.06f, 0.075f), new Vector3(0.07f, 0.08f, 0.045f), new Vector3(0, s * -20f, 0));
                    P(spine, Box, Surface.Leather, Le, new Vector3(s * 0.15f * w, -0.025f, 0.085f), new Vector3(0.072f, 0.03f, 0.05f), new Vector3(0, s * -20f, 0));
                    P(spine, Sph, Surface.Gold, Tr, new Vector3(s * 0.158f * w, -0.035f, 0.108f), Vector3.one * 0.014f);
                }
            }

            switch (look.body)
            {
                case BodyGear.Chain:
                case BodyGear.Tunic:
                case BodyGear.Leather:
                {
                    bool chain = look.body == BodyGear.Chain;
                    bool leatherBody = look.body == BodyGear.Leather;
                    Surface s = chain ? Surface.Chain : leatherBody ? Surface.Leather : Surface.Cloth;
                    Color c = chain ? Ch : leatherBody ? Le * 1.15f : Cd;
                    P(hips, Fr(1.1f, 1.05f), Surface.Leather, Ld, Vector3.zero, new Vector3(0.32f * w, 0.18f, 0.2f));
                    P(spine, Fr(1.18f, 1.08f), s, c, new Vector3(0, 0.1f, 0), new Vector3(0.31f * w, 0.22f, 0.2f));
                    P(chest, Fr(1.28f, 1.12f), s, c, new Vector3(0, 0.12f, 0), new Vector3(0.35f * w, 0.34f, 0.22f));
                    P(chest, Fr(0.6f, 0.8f), s, c, new Vector3(0, 0.31f, 0), new Vector3(0.44f * w, 0.06f, 0.21f));
                    P(chest, Tor(0.3f), Surface.Cloth, Cl * 0.55f, new Vector3(0, 0.33f, 0), new Vector3(0.2f, 0.4f, 0.17f));
                    if (chain) P(hips, Bell(0.8f, 0.02f), Surface.Chain, Ch * 0.95f, new Vector3(0, -0.14f, 0), new Vector3(0.36f * w, 0.3f, 0.26f));
                    else P(hips, Bell(0.85f, 0.02f), Surface.Cloth, Cd, new Vector3(0, -0.1f, 0), new Vector3(0.34f * w, 0.22f, 0.25f));

                    if (leatherBody)
                    {
                        // Kurtka skórzana: nabijane ćwieki, dwa pasy na krzyż z pochwami noży, krótka pelerynka z kapturem na ramionach.
                        for (int row = 0; row < 4; row++)
                            for (int col = -2; col <= 2; col++)
                                P(chest, Sph, Surface.Metal, new Color(0.55f, 0.5f, 0.42f), new Vector3(col * 0.055f * w, 0.02f + row * 0.07f, 0.128f), Vector3.one * 0.016f);
                        for (int d = -1; d <= 1; d += 2)
                        {
                            P(chest, Box, Surface.Leather, Le * 0.55f, new Vector3(0, 0.1f, 0.136f), new Vector3(0.035f, 0.46f, 0.01f), new Vector3(0, 0, d * 35f));
                            P(chest, Box, Surface.Leather, Le * 0.45f, new Vector3(d * 0.07f, 0.16f, 0.142f), new Vector3(0.03f, 0.12f, 0.012f), new Vector3(0, 0, d * 35f));
                            P(chest, Box, Surface.Metal, new Color(0.75f, 0.75f, 0.78f), new Vector3(d * 0.085f, 0.23f, 0.146f), new Vector3(0.012f, 0.05f, 0.006f), new Vector3(0, 0, d * 35f));
                        }
                        P(chest, Fr(1.25f, 0.9f), Surface.Cloth, Cl, new Vector3(0, 0.31f, 0), new Vector3(0.4f * w, 0.09f, 0.24f));
                        for (int i = 0; i < 6; i++)
                            P(hips, Box, Surface.Leather, Le * (i % 2 == 0 ? 0.9f : 0.75f), new Vector3((-0.125f + i * 0.05f) * w, -0.2f, 0.12f), new Vector3(0.045f * w, 0.18f, 0.012f), new Vector3(-6f, 0, 0));
                        Belt(Le);
                        break;
                    }

                    // Tabard z herbem (przód i tył)
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float z = side * 0.122f;
                        P(chest, Fr(1.1f, 1f), Surface.Cloth, Cl, new Vector3(0, 0.08f, z), new Vector3(0.27f * w, 0.36f, 0.015f));
                        P(spine, Box, Surface.Cloth, Cl, new Vector3(0, 0.1f, side * 0.108f), new Vector3(0.25f * w, 0.22f, 0.012f));
                        P(hips, Fr(1.05f, 1f), Surface.Cloth, Cl, new Vector3(0, -0.25f, side * 0.14f), new Vector3(0.25f * w, 0.44f, 0.015f), new Vector3(side * -6f, 0, 0));
                        P(hips, Box, Surface.Gold, Tr, new Vector3(0, -0.465f, side * 0.163f), new Vector3(0.25f * w, 0.02f, 0.016f), new Vector3(side * -6f, 0, 0));
                        for (int e = -1; e <= 1; e += 2)
                        {
                            P(chest, Box, Surface.Gold, Tr, new Vector3(e * 0.14f * w, 0.08f, z + side * 0.006f), new Vector3(0.012f, 0.36f, 0.01f));
                            P(hips, Box, Surface.Gold, Tr, new Vector3(e * 0.126f * w, -0.25f, side * 0.148f), new Vector3(0.012f, 0.44f, 0.01f), new Vector3(side * -6f, 0, 0));
                        }
                        P(chest, Box, Surface.Gold, Tr, new Vector3(0, 0.1f, z + side * 0.011f), new Vector3(0.035f, 0.2f, 0.01f));
                        P(chest, Box, Surface.Gold, Tr, new Vector3(0, 0.15f, z + side * 0.011f), new Vector3(0.13f, 0.035f, 0.01f));
                    }
                    P(chest, Box, Surface.Leather, Le, new Vector3(0, 0.1f, 0.136f), new Vector3(0.035f, 0.44f, 0.01f), new Vector3(0, 0, 38f));
                    P(chest, Box, Surface.Gold, Tr, new Vector3(0.02f, 0.13f, 0.142f), new Vector3(0.03f, 0.03f, 0.008f), new Vector3(0, 0, 38f));
                    Belt(Le);
                    if (chain) Sidearms();
                    break;
                }
                case BodyGear.Plate:
                {
                    Color Ad = A * 0.85f;
                    Surface ms = look.style == RigStyle.Castellan ? Surface.DarkMetal : Surface.Metal;
                    P(hips, Fr(1.1f, 1.05f), Surface.Leather, Ld, Vector3.zero, new Vector3(0.32f * w, 0.18f, 0.2f));
                    P(chest, Fr(1.25f, 1.1f), ms, Ad, new Vector3(0, 0.12f, -0.01f), new Vector3(0.36f * w, 0.34f, 0.22f));
                    P(chest, Sph, ms, A, new Vector3(0, 0.13f, 0.02f), new Vector3(0.42f * w, 0.38f, 0.29f));
                    P(chest, Box, ms, A * 1.12f, new Vector3(0, 0.13f, 0.155f), new Vector3(0.022f, 0.3f, 0.02f));
                    P(chest, Fr(1.1f, 1f), ms, Ad, new Vector3(0, 0.12f, -0.12f), new Vector3(0.38f * w, 0.32f, 0.03f));
                    for (int i = 0; i < 2; i++) P(chest, Box, ms, A, new Vector3(0, 0.02f + i * 0.12f, -0.14f), new Vector3(0.34f * w, 0.02f, 0.015f));
                    P(chest, Tube(0.8f), ms, A, new Vector3(0, 0.34f, 0), new Vector3(0.22f, 0.08f, 0.2f));
                    P(chest, Tor(0.2f), Surface.Gold, Tr, new Vector3(0, 0.3f, 0), new Vector3(0.23f, 0.2f, 0.21f));
                    for (int i = 0; i < 3; i++)
                        for (int s = -1; s <= 1; s += 2)
                            P(chest, Sph, Surface.Gold, Tr, new Vector3(s * 0.15f * w, 0.02f + i * 0.09f, 0.125f), Vector3.one * 0.016f);
                    P(spine, Fr(1.15f, 1.1f), ms, A, new Vector3(0, 0.11f, 0.02f), new Vector3(0.3f * w, 0.2f, 0.22f));
                    P(spine, Box, ms, A * 1.1f, new Vector3(0, 0.11f, 0.13f), new Vector3(0.02f, 0.18f, 0.015f));
                    Belt(Le);
                    if (look.style != RigStyle.Castellan) Sidearms();
                    for (int k = 0; k < 3; k++)
                        P(hips, Tube(1.08f), ms, k % 2 == 0 ? A : Ad, new Vector3(0, 0.02f - k * 0.055f, 0), new Vector3(0.35f * w + k * 0.02f, 0.055f, 0.25f + k * 0.015f));
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(hips, Fr(0.9f, 1f), ms, A, new Vector3(s * 0.1f * w, -0.23f, 0.11f), new Vector3(0.15f, 0.2f, 0.02f), new Vector3(-10f, 0, s * 8f));
                        P(hips, Box, Surface.Gold, Tr, new Vector3(s * 0.1f * w, -0.14f, 0.128f), new Vector3(0.13f, 0.012f, 0.01f), new Vector3(-10f, 0, s * 8f));
                        P(hips, Sph, Surface.Gold, Tr, new Vector3(s * 0.1f * w, -0.2f, 0.13f), Vector3.one * 0.015f);
                    }
                    P(hips, Bell(0.85f, 0.02f), Surface.Chain, Ch * 0.9f, new Vector3(0, -0.17f, 0), new Vector3(0.34f * w, 0.26f, 0.25f));
                    P(hips, Fr(1f, 1f), Surface.Cloth, Cl, new Vector3(0, -0.3f, 0.09f), new Vector3(0.2f * w, 0.34f, 0.012f), new Vector3(-5f, 0, 0));
                    P(hips, Fr(1f, 1f), Surface.Cloth, Cl, new Vector3(0, -0.3f, -0.1f), new Vector3(0.24f * w, 0.36f, 0.012f), new Vector3(5f, 0, 0));
                    P(hips, Box, Surface.Gold, Tr, new Vector3(0, -0.46f, 0.105f), new Vector3(0.2f * w, 0.018f, 0.014f), new Vector3(-5f, 0, 0));
                    break;
                }
                case BodyGear.Robe:
                {
                    Color stole = look.style == RigStyle.Heretic ? new Color(0.12f, 0.05f, 0.14f) : Tr * 0.85f;
                    P(hips, Fr(1.1f, 1.05f), Surface.Cloth, Cd, Vector3.zero, new Vector3(0.32f * w, 0.18f, 0.2f));
                    P(chest, Fr(1.2f, 1.1f), Surface.Cloth, Cl, new Vector3(0, 0.12f, 0), new Vector3(0.34f * w, 0.34f, 0.22f));
                    P(chest, Fr(0.55f, 0.6f), Surface.Cloth, Cd, new Vector3(0, 0.27f, 0), new Vector3(0.5f * w, 0.14f, 0.32f));
                    P(chest, Tor(0.08f), Surface.Gold, Tr, new Vector3(0, 0.2f, 0), new Vector3(0.5f * w, 0.12f, 0.32f));
                    P(chest, Tube(1.2f), Surface.Cloth, Cd, new Vector3(0, 0.34f, 0), new Vector3(0.19f, 0.07f, 0.17f));
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(chest, Box, Surface.Cloth, stole, new Vector3(s * 0.05f, 0.08f, 0.125f), new Vector3(0.05f, 0.34f, 0.012f));
                        P(chest, Box, Surface.Gold, Tr, new Vector3(s * 0.077f, 0.08f, 0.128f), new Vector3(0.006f, 0.34f, 0.01f));
                        P(hips, Box, Surface.Cloth, stole, new Vector3(s * 0.05f, -0.42f, 0.205f), new Vector3(0.05f, 0.76f, 0.012f), new Vector3(-8f, 0, 0));
                        P(hips, Box, Surface.Gold, Tr, new Vector3(s * 0.05f, -0.8f, 0.258f), new Vector3(0.05f, 0.03f, 0.014f), new Vector3(-8f, 0, 0));
                    }
                    P(chest, Tor(0.12f), Surface.Gold, Tr, new Vector3(0, 0.27f, 0.06f), new Vector3(0.2f, 0.12f, 0.16f), new Vector3(25f, 0, 0));
                    P(chest, Dome, Surface.Gold, Tr, new Vector3(0, 0.15f, 0.132f), new Vector3(0.055f, 0.03f, 0.065f), new Vector3(90f, 0, 0));
                    P(chest, Sph, Surface.Glow, look.style == RigStyle.Heretic ? new Color(0.7f, 0.2f, 1f) : new Color(0.3f, 0.6f, 1f), new Vector3(0, 0.15f, 0.145f), new Vector3(0.035f, 0.045f, 0.02f));
                    P(spine, Fr(1.1f, 1.05f), Surface.Cloth, Cl, new Vector3(0, 0.1f, 0), new Vector3(0.31f * w, 0.22f, 0.21f));
                    Color sash = look.style == RigStyle.Heretic ? new Color(0.35f, 0.3f, 0.2f) : Cd * 1.2f;
                    P(spine, Tor(0.35f), Surface.Cloth, sash, new Vector3(0, 0f, 0), new Vector3(0.34f * w, 0.3f, 0.25f));
                    P(spine, Sph, Surface.Cloth, sash, new Vector3(0.08f, -0.01f, 0.125f), new Vector3(0.05f, 0.045f, 0.03f));
                    P(spine, Box, Surface.Cloth, sash, new Vector3(0.07f, -0.15f, 0.13f), new Vector3(0.04f, 0.24f, 0.01f), new Vector3(0, 0, -6f));
                    P(spine, Box, Surface.Cloth, sash, new Vector3(0.11f, -0.13f, 0.12f), new Vector3(0.035f, 0.2f, 0.01f), new Vector3(0, 0, 10f));
                    P(spine, Fr(0.9f, 0.85f), Surface.Leather, Le, new Vector3(-0.16f * w, -0.06f, 0.06f), new Vector3(0.07f, 0.09f, 0.05f), new Vector3(0, 20f, 0));
                    P(spine, Box, Surface.Leather, new Color(0.35f, 0.08f, 0.06f), new Vector3(0.17f * w, -0.07f, 0.03f), new Vector3(0.035f, 0.13f, 0.1f));
                    P(spine, Box, Surface.Bone, new Color(0.85f, 0.8f, 0.68f), new Vector3(0.172f * w, -0.07f, 0.03f), new Vector3(0.033f, 0.12f, 0.085f));
                    P(spine, Box, Surface.Gold, Tr, new Vector3(0.19f * w, -0.07f, 0.03f), new Vector3(0.004f, 0.06f, 0.05f));

                    P(hips, Bell(0.62f, 0.06f), Surface.Cloth, Cl, new Vector3(0, -0.42f, 0), new Vector3(0.46f * w, 0.88f, 0.38f));
                    P(hips, Tor(0.08f), Surface.Gold, Tr, new Vector3(0, -0.855f, 0), new Vector3(0.465f * w, 0.4f, 0.385f));
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i / 10f * 360f + 18f;
                        float rad = a * Mathf.Deg2Rad;
                        var pos = new Vector3(Mathf.Sin(rad) * 0.19f * w, -0.5f, Mathf.Cos(rad) * 0.155f);
                        P(hips, Box, Surface.Cloth, Cd, pos, new Vector3(0.018f, 0.66f, 0.018f), new Vector3(-6f, a, 0));
                    }
                    if (look.style == RigStyle.Heretic)
                    {
                        for (int i = 0; i < 14; i++)
                        {
                            float a = i / 14f * 360f;
                            float rad = a * Mathf.Deg2Rad;
                            P(hips, Box, Surface.Cloth, Cl * 0.8f, new Vector3(Mathf.Sin(rad) * 0.225f * w, -0.89f, Mathf.Cos(rad) * 0.185f), new Vector3(0.06f, 0.06f, 0.008f), new Vector3(0, a, 45f));
                        }
                        for (int i = 0; i < 4; i++)
                            P(hips, Box, Surface.Glow, new Color(0.75f, 0.3f, 1f), new Vector3(0, -0.2f - i * 0.15f, 0.215f + i * 0.021f), new Vector3(0.03f, 0.03f, 0.006f), new Vector3(-8f, 0, 45f));
                    }
                    RobeDetails();
                    break;
                }
                case BodyGear.Rags:
                {
                    Color bone = Sk * 1.12f;
                    P(hips, Fr(1.1f, 1.05f), Surface.Skin, Sk * 0.9f, Vector3.zero, new Vector3(0.27f * w, 0.16f, 0.17f));
                    P(chest, Fr(1.15f, 1.05f), Surface.Skin, Sk, new Vector3(0, 0.1f, 0), new Vector3(0.3f * w, 0.32f, 0.19f));
                    for (int k = 0; k < 4; k++)
                        for (int s = -1; s <= 1; s += 2)
                        {
                            P(chest, Box, Surface.Bone, bone, new Vector3(s * 0.07f * w, 0.03f + k * 0.065f, 0.095f), new Vector3(0.11f, 0.02f, 0.03f), new Vector3(0, s * 12f, s * 14f));
                            P(chest, Box, Surface.Bone, bone * 0.95f, new Vector3(s * 0.14f * w, 0.02f + k * 0.065f, 0.02f), new Vector3(0.02f, 0.02f, 0.16f), new Vector3(0, 0, s * 10f));
                        }
                    P(chest, Box, Surface.Bone, bone, new Vector3(0, 0.13f, 0.105f), new Vector3(0.03f, 0.24f, 0.02f));
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(chest, Box, Surface.Bone, bone, new Vector3(s * 0.09f, 0.28f, 0.07f), new Vector3(0.15f, 0.022f, 0.028f), new Vector3(0, s * -18f, s * -8f));
                        P(chest, Fr(0.8f, 1f), Surface.Skin, Sk * 0.95f, new Vector3(s * 0.08f, 0.17f, -0.1f), new Vector3(0.09f, 0.13f, 0.02f));
                    }
                    for (int k = 0; k < 5; k++) P(chest, Sph, Surface.Bone, bone, new Vector3(0, -0.02f + k * 0.07f, -0.1f), new Vector3(0.04f, 0.035f, 0.03f));
                    P(chest, Box, Surface.Skin, new Color(0.35f, 0.08f, 0.06f), new Vector3(0.06f, 0.05f, 0.1f), new Vector3(0.05f, 0.03f, 0.01f), new Vector3(0, 0, 25f));
                    P(spine, Fr(1.1f, 1.05f), Surface.Skin, Sk * 0.88f, new Vector3(0, 0.1f, 0), new Vector3(0.23f * w, 0.22f, 0.15f));
                    for (int k = 0; k < 2; k++) P(spine, Sph, Surface.Bone, bone, new Vector3(0, 0.05f + k * 0.08f, -0.075f), new Vector3(0.035f, 0.03f, 0.03f));
                    P(spine, Tor(0.25f), Surface.Leather, new Color(0.4f, 0.33f, 0.2f), new Vector3(0, -0.01f, 0), new Vector3(0.28f, 0.25f, 0.2f));
                    P(hips, Fr(0.8f, 1f), Surface.Cloth, Cl, new Vector3(0, -0.16f, 0.1f), new Vector3(0.2f, 0.3f, 0.015f), new Vector3(-5f, 0, 0));
                    P(hips, Fr(0.8f, 1f), Surface.Cloth, Cl * 0.85f, new Vector3(0, -0.16f, -0.1f), new Vector3(0.22f, 0.28f, 0.015f), new Vector3(5f, 0, 0));
                    for (int i = 0; i < 4; i++)
                        P(hips, Box, Surface.Cloth, Cd, new Vector3(-0.08f + i * 0.055f, -0.33f - (i % 2) * 0.03f, 0.115f), new Vector3(0.025f, 0.08f, 0.01f), new Vector3(-5f, 0, (i - 1.5f) * 8f));
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ Druga warstwa detali

        void DetailPass(RigLook look)
        {
            float w = look.bulk;
            var hips = this[Bone.Hips];
            var spine = this[Bone.Spine];
            var chest = this[Bone.Chest];
            Color A = look.armor, Ch = look.armor * 0.85f, Cl = look.cloth, Tr = look.trim, Le = look.leather, Sk = look.skin;

            switch (look.body)
            {
                case BodyGear.Chain:
                    // Rzędy kółek kolczugi
                    for (int k = 0; k < 4; k++) P(chest, Tor(0.12f), Surface.Chain, Ch * 0.8f, new Vector3(0, 0.0f + k * 0.075f, 0), new Vector3(0.36f * w + k * 0.02f, 0.2f, 0.23f));
                    for (int k = 0; k < 2; k++) P(spine, Tor(0.12f), Surface.Chain, Ch * 0.8f, new Vector3(0, 0.05f + k * 0.08f, 0), new Vector3(0.32f * w, 0.2f, 0.21f));
                    for (int k = 0; k < 3; k++) P(hips, Tor(0.1f), Surface.Chain, Ch * 0.8f, new Vector3(0, -0.06f - k * 0.08f, 0), new Vector3(0.31f * w + k * 0.035f, 0.2f, 0.225f + k * 0.02f));
                    // Frędzle tabardu
                    for (int side = -1; side <= 1; side += 2)
                        for (int i = 0; i < 8; i++)
                            P(hips, Cone, Surface.Gold, Tr, new Vector3((-0.105f + i * 0.03f) * w, -0.49f, side * 0.166f), new Vector3(0.018f, 0.04f, 0.01f), new Vector3(side * -6f + 180f, 0, 0));
                    // Klamry pendentu
                    for (int i = 0; i < 2; i++) P(chest, Box, Surface.Gold, Tr, new Vector3(-0.07f + i * 0.14f, 0.02f + i * 0.17f, 0.141f), new Vector3(0.025f, 0.025f, 0.008f), new Vector3(0, 0, 38f));
                    break;
                case BodyGear.Plate:
                {
                    Surface ms = look.style == RigStyle.Castellan ? Surface.DarkMetal : Surface.Metal;
                    // Grawerunki na napierśniku
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(chest, Box, Surface.Gold, Tr, new Vector3(s * 0.08f, 0.14f, 0.152f), new Vector3(0.008f, 0.22f, 0.006f), new Vector3(0, s * 10f, s * -8f));
                        P(chest, Box, Surface.Gold, Tr, new Vector3(s * 0.11f, 0.26f, 0.13f), new Vector3(0.08f, 0.008f, 0.006f), new Vector3(0, s * 20f, 0));
                        P(chest, Box, Surface.Leather, Le, new Vector3(s * 0.2f * w, 0.1f, 0f), new Vector3(0.012f, 0.24f, 0.04f));
                        for (int k = 0; k < 2; k++)
                        {
                            P(chest, Box, Surface.Leather, Le * 0.8f, new Vector3(s * 0.205f * w, 0.02f + k * 0.14f, 0f), new Vector3(0.02f, 0.025f, 0.12f));
                            P(chest, Box, Surface.Gold, Tr, new Vector3(s * 0.212f * w, 0.02f + k * 0.14f, 0.05f), new Vector3(0.01f, 0.03f, 0.02f));
                        }
                    }
                    for (int k = 0; k < 2; k++) P(chest, Tube(0.85f), ms, A * 0.95f, new Vector3(0, 0.32f - k * 0.035f, 0), new Vector3(0.25f + k * 0.03f, 0.03f, 0.22f + k * 0.02f));
                    P(chest, Box, ms, A * 1.1f, new Vector3(0, 0.12f, -0.14f), new Vector3(0.02f, 0.3f, 0.02f));
                    for (int k = 0; k < 2; k++) P(hips, Tube(1.08f), ms, A * 0.92f, new Vector3(0, -0.145f - k * 0.045f, 0), new Vector3(0.4f * w + k * 0.02f, 0.04f, 0.29f + k * 0.012f));
                    if (look.style == RigStyle.Warden)
                    {
                        // Futrzany kołnierz i ciemny tabard
                        Color fur = new Color(0.3f, 0.24f, 0.18f);
                        P(chest, Bell(0.45f, 0.03f), Surface.Hair, fur, new Vector3(0, 0.3f, 0), new Vector3(0.56f * w, 0.14f, 0.4f));
                        for (int i = 0; i < 18; i++)
                        {
                            float a = i / 18f * 360f;
                            float rad = a * Mathf.Deg2Rad;
                            P(chest, Cone, Surface.Hair, fur * (i % 2 == 0 ? 1f : 0.8f), new Vector3(Mathf.Sin(rad) * 0.27f * w, 0.24f, Mathf.Cos(rad) * 0.19f), new Vector3(0.06f, 0.09f, 0.04f), new Vector3(180f - 20f, a, 0));
                        }
                        P(chest, Fr(1.05f, 1f), Surface.Cloth, Cl, new Vector3(0, 0.02f, 0.17f), new Vector3(0.22f * w, 0.26f, 0.01f));
                        P(hips, Fr(1.05f, 1f), Surface.Cloth, Cl, new Vector3(0, -0.3f, 0.16f), new Vector3(0.22f * w, 0.42f, 0.012f), new Vector3(-6f, 0, 0));
                        P(chest, Box, Surface.Gold, Tr, new Vector3(0, 0.04f, 0.178f), new Vector3(0.1f, 0.1f, 0.006f), new Vector3(0, 0, 45f));
                    }
                    if (look.style == RigStyle.Castellan)
                    {
                        // Łańcuch z czaszkami przy pasie i pęknięcia żarzące się pod pancerzem
                        Color iron = new Color(0.25f, 0.2f, 0.2f), bone = new Color(0.75f, 0.7f, 0.58f);
                        for (int i = 0; i < 14; i++)
                        {
                            float a = (i / 13f - 0.5f) * 200f * Mathf.Deg2Rad;
                            P(spine, Tor(0.25f), Surface.DarkMetal, iron, new Vector3(Mathf.Sin(a) * 0.2f * w, -0.07f - Mathf.Cos(a) * 0.03f, Mathf.Cos(a) * 0.14f), new Vector3(0.035f, 0.3f, 0.05f), new Vector3(90f, a * Mathf.Rad2Deg + (i % 2) * 90f, 0));
                        }
                        for (int k = 0; k < 3; k++)
                        {
                            Vector3 sp = new Vector3((-0.12f + k * 0.12f) * w, -0.14f, 0.15f);
                            P(spine, Sph, Surface.Bone, bone, sp, new Vector3(0.07f, 0.065f, 0.075f));
                            P(spine, Fr(0.8f, 0.8f), Surface.Bone, bone * 0.95f, sp + new Vector3(0, -0.03f, 0.012f), new Vector3(0.045f, 0.025f, 0.045f));
                            for (int e = -1; e <= 1; e += 2) P(spine, Sph, Surface.Dark, new Color(0.05f, 0.03f, 0.02f), sp + new Vector3(e * 0.017f, 0.005f, 0.033f), Vector3.one * 0.018f);
                        }
                        for (int i = 0; i < 6; i++)
                            P(chest, Box, Surface.Glow, new Color(1f, 0.35f, 0.1f), new Vector3((-0.12f + i * 0.05f) * w, 0.05f + (i % 3) * 0.07f, 0.16f), new Vector3(0.006f, 0.08f, 0.004f), new Vector3(0, 0, (i - 2.5f) * 20f));
                        for (int s = -1; s <= 1; s += 2)
                            P(chest, Cone, Surface.DarkMetal, A * 1.3f, new Vector3(s * 0.14f * w, 0.24f, -0.14f), new Vector3(0.04f, 0.12f, 0.04f), new Vector3(-60f, 0, s * -20f));
                    }
                    break;
                }
                case BodyGear.Robe:
                {
                    // Dodatkowe fałdy, haft na plecach, frędzle przy szarfie, futrzana obszywka kaptura
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i / 10f * 360f;
                        float rad = a * Mathf.Deg2Rad;
                        P(hips, Box, Surface.Cloth, Cl * 0.82f, new Vector3(Mathf.Sin(rad) * 0.21f * w, -0.62f, Mathf.Cos(rad) * 0.172f), new Vector3(0.014f, 0.46f, 0.014f), new Vector3(-7f, a, 0));
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        float a = i / 6f * Mathf.PI * 2f;
                        P(chest, Box, look.style == RigStyle.Heretic ? Surface.Glow : Surface.Gold, look.style == RigStyle.Heretic ? new Color(0.6f, 0.25f, 0.9f) : Tr, new Vector3(Mathf.Cos(a) * 0.06f, 0.14f + Mathf.Sin(a) * 0.06f, -0.12f), new Vector3(0.05f, 0.01f, 0.006f), new Vector3(0, 0, a * Mathf.Rad2Deg));
                    }
                    P(chest, Tor(0.08f), Surface.Gold, Tr, new Vector3(0, 0.14f, -0.118f), new Vector3(0.16f, 0.1f, 0.16f), new Vector3(90f, 0, 0));
                    for (int i = 0; i < 4; i++)
                        P(spine, Cone, Surface.Gold, Tr, new Vector3(0.06f + i * 0.02f, -0.29f + (i % 2) * 0.02f, 0.13f), new Vector3(0.018f, 0.05f, 0.018f), new Vector3(180f, 0, 0));
                    P(spine, Box, Surface.Gold, Tr, new Vector3(0.19f * w, -0.03f, 0.03f), new Vector3(0.006f, 0.02f, 0.12f));
                    P(spine, Box, Surface.Gold, Tr, new Vector3(0.19f * w, -0.11f, 0.03f), new Vector3(0.006f, 0.02f, 0.12f));
                    for (int k = 0; k < 2; k++)
                        P(hips, Tor(0.06f), Surface.Gold, Tr, new Vector3(0, -0.8f + k * 0.05f, 0), new Vector3((0.44f - k * 0.015f) * w, 0.3f, 0.365f - k * 0.012f));
                    break;
                }
                case BodyGear.Rags:
                {
                    Color vein = new Color(0.2f, 0.25f, 0.18f);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(chest, Box, Surface.Bone, Sk * 1.1f, new Vector3(s * 0.07f * w, 0.29f, 0.093f), new Vector3(0.1f, 0.018f, 0.025f), new Vector3(0, s * 12f, s * 14f));
                        P(chest, Box, Surface.Bone, Sk * 1.1f, new Vector3(s * 0.065f * w, -0.03f, 0.09f), new Vector3(0.09f, 0.018f, 0.025f), new Vector3(0, s * 12f, s * 20f));
                        for (int k = 0; k < 3; k++)
                            P(chest, Tube(0.6f), Surface.Skin, vein, new Vector3(s * (0.04f + k * 0.02f), 0.05f + k * 0.03f, 0.1f), new Vector3(0.006f, 0.12f, 0.006f), new Vector3(0, 0, s * (20f + k * 15f)));
                    }
                    for (int k = 0; k < 3; k++) P(spine, Sph, Surface.Bone, Sk * 1.1f, new Vector3(0, 0.0f + k * 0.07f, -0.08f), new Vector3(0.03f, 0.028f, 0.028f));
                    for (int i = 0; i < 6; i++)
                        P(hips, Box, Surface.Cloth, Cl * 0.7f, new Vector3(-0.09f + i * 0.036f, -0.32f - (i % 3) * 0.02f, -0.11f), new Vector3(0.022f, 0.09f, 0.008f), new Vector3(5f, 0, (i - 2.5f) * 7f));
                    P(hips, Box, Surface.Skin, new Color(0.35f, 0.08f, 0.06f), new Vector3(-0.08f, 0.05f, 0.09f), new Vector3(0.04f, 0.02f, 0.01f), new Vector3(0, 0, -30f));
                    break;
                }
            }

            // Ręce i nogi – elementy wspólne
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                float sg = left ? -1f : 1f;
                var ua = this[left ? Bone.UpperArmL : Bone.UpperArmR];
                var fa = this[left ? Bone.ForearmL : Bone.ForearmR];
                var sh = this[left ? Bone.ShinL : Bone.ShinR];
                var ft = this[left ? Bone.FootL : Bone.FootR];
                switch (look.body)
                {
                    case BodyGear.Plate:
                    {
                        Surface ms = look.style == RigStyle.Castellan ? Surface.DarkMetal : Surface.Metal;
                        for (int k = 0; k < 2; k++) P(ua, Tube(0.95f), ms, A * 0.9f, new Vector3(0, -0.2f - k * 0.04f, 0), new Vector3(0.135f, 0.03f, 0.135f));
                        P(fa, Sph, Surface.Gold, Tr, new Vector3(sg * 0.055f, 0f, -0.02f), Vector3.one * 0.022f);
                        for (int k = 0; k < 3; k++) P(sh, Sph, Surface.Gold, Tr, new Vector3(sg * 0.07f, -0.1f - k * 0.1f, 0f), Vector3.one * 0.014f);
                        P(sh, Sph, Surface.Gold, Tr, new Vector3(0, 0.01f, 0.1f), Vector3.one * 0.022f);
                        for (int k = 0; k < 2; k++) P(ft, Fr(0.9f, 0.9f), ms, A * 0.95f, new Vector3(0, -0.018f, 0.2f + k * 0.035f), new Vector3(0.1f - k * 0.01f, 0.05f, 0.04f));
                        P(ft, Tor(0.2f), Surface.Gold, Tr, new Vector3(0, 0.0f, -0.08f), new Vector3(0.04f, 0.2f, 0.04f), new Vector3(90f, 0, 0));
                        break;
                    }
                    case BodyGear.Chain:
                    case BodyGear.Tunic:
                    case BodyGear.Leather:
                        // Sznurowanie butów, sprzączka, ostroga
                        for (int k = 0; k < 4; k++)
                            for (int d = -1; d <= 1; d += 2)
                                P(sh, Box, Surface.Leather, Le * 0.5f, new Vector3(0, -0.22f - k * 0.045f, 0.07f), new Vector3(0.045f, 0.006f, 0.006f), new Vector3(0, 0, d * 30f));
                        P(ft, Box, Surface.Gold, Tr, new Vector3(sg * 0.056f, -0.02f, 0.02f), new Vector3(0.008f, 0.03f, 0.04f));
                        if (look.body == BodyGear.Chain)
                        {
                            P(ft, Box, Surface.Metal, Tr, new Vector3(0, -0.02f, -0.1f), new Vector3(0.012f, 0.012f, 0.06f));
                            P(ft, Tor(0.3f), Surface.Metal, Tr, new Vector3(0, -0.02f, -0.135f), new Vector3(0.035f, 0.3f, 0.035f), new Vector3(0, 0, 90f));
                            P(ua, Tor(0.2f), Surface.Chain, Ch * 0.8f, new Vector3(0, -0.2f, 0), new Vector3(0.13f, 0.2f, 0.13f));
                        }
                        break;
                    case BodyGear.Robe:
                        for (int k = 0; k < 2; k++) P(fa, Tor(0.1f), Surface.Gold, Tr, new Vector3(0, -0.2f - k * 0.03f, 0), new Vector3(0.18f + k * 0.01f, 0.25f, 0.18f + k * 0.01f));
                        P(ua, Box, Surface.Gold, Tr, new Vector3(sg * 0.07f, -0.13f, 0), new Vector3(0.005f, 0.2f, 0.02f));
                        break;
                    case BodyGear.Rags:
                        for (int k = 0; k < 2; k++)
                            P(fa, Tube(0.6f), Surface.Skin, new Color(0.2f, 0.25f, 0.18f), new Vector3(sg * 0.02f, -0.1f - k * 0.08f, 0.04f), new Vector3(0.006f, 0.1f, 0.006f), new Vector3(0, 0, sg * 12f));
                        P(ua, Sph, Surface.Bone, Sk * 1.1f, new Vector3(sg * 0.05f, -0.02f, 0), Vector3.one * 0.045f);
                        break;
                }
            }

            // Peleryna: fałdy i frędzle
            if (CapePivot != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    P(CapePivot, Box, Surface.Cloth, Cl * 0.6f, new Vector3((-0.16f + i * 0.08f) * w, -0.22f, -0.014f), new Vector3(0.012f, 0.42f, 0.012f));
                    P(CapeLower, Box, Surface.Cloth, Cl * 0.6f, new Vector3((-0.18f + i * 0.09f) * w, -0.2f, -0.014f), new Vector3(0.012f, 0.38f, 0.012f));
                }
                for (int i = 0; i < 9; i++)
                    P(CapeLower, Cone, look.style == RigStyle.Castellan ? Surface.Cloth : Surface.Gold, look.style == RigStyle.Castellan ? Cl * 0.6f : Tr, new Vector3((-0.22f + i * 0.055f) * w, -0.425f, 0), new Vector3(0.02f, 0.045f, 0.012f), new Vector3(180f, 0, 0));
            }

            // Kaptur: futrzana obszywka (mag) / postrzępiony brzeg (heretyk)
            if (look.head == HeadGear.Hood)
            {
                var head = this[Bone.Head];
                for (int i = 0; i < 16; i++)
                {
                    float a = i / 16f * Mathf.PI * 2f;
                    Vector3 pos = new Vector3(Mathf.Cos(a) * 0.138f, 0.115f + Mathf.Sin(a) * 0.152f, 0.078f);
                    if (look.style == RigStyle.Mage) P(head, Sph, Surface.Hair, new Color(0.8f, 0.78f, 0.72f), pos, new Vector3(0.035f, 0.035f, 0.03f));
                    else P(head, Cone, Surface.Cloth, Cl * 0.7f, pos, new Vector3(0.025f, 0.04f, 0.01f), new Vector3(0, 0, a * Mathf.Rad2Deg - 90f));
                }
            }
        }

        void DetailPass2(RigLook look)
        {
            float w = look.bulk;
            var hips = this[Bone.Hips];
            var spine = this[Bone.Spine];
            var chest = this[Bone.Chest];
            var head = this[Bone.Head];
            Color A = look.armor, Ch = look.armor * 0.85f, Cl = look.cloth, Tr = look.trim, Le = look.leather, Sk = look.skin;
            Color dark = new Color(0.05f, 0.045f, 0.05f);

            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                float sg = left ? -1f : 1f;
                var ua = this[left ? Bone.UpperArmL : Bone.UpperArmR];
                var fa = this[left ? Bone.ForearmL : Bone.ForearmR];
                var th = this[left ? Bone.ThighL : Bone.ThighR];
                var sh = this[left ? Bone.ShinL : Bone.ShinR];
                var ft = this[left ? Bone.FootL : Bone.FootR];
                switch (look.body)
                {
                    case BodyGear.Chain:
                        // Pikowana przeszywanica pod kolczugą i rzędy kółek na rękawach
                        for (int k = 0; k < 4; k++) P(ua, Tor(0.18f), Surface.Cloth, Cl * 0.5f, new Vector3(0, -0.03f - k * 0.06f, 0), new Vector3(0.128f, 0.2f, 0.128f));
                        for (int k = 0; k < 3; k++) P(fa, Tor(0.12f), Surface.Chain, Ch * 0.8f, new Vector3(0, -0.02f - k * 0.04f, 0), new Vector3(0.112f, 0.2f, 0.112f));
                        for (int k = 0; k < 3; k++) P(th, Tor(0.12f), Surface.Chain, Ch * 0.8f, new Vector3(0, -0.12f - k * 0.1f, 0), new Vector3(0.158f, 0.2f, 0.158f));
                        for (int k = 0; k < 2; k++) P(ft, Box, Surface.Leather, Le * 0.55f, new Vector3(0, -0.005f, 0.09f + k * 0.04f), new Vector3(0.1f, 0.006f, 0.006f));
                        break;
                    case BodyGear.Tunic:
                        for (int k = 0; k < 3; k++) P(ua, Tor(0.18f), Surface.Cloth, Cl * 0.6f, new Vector3(0, -0.04f - k * 0.06f, 0), new Vector3(0.14f, 0.2f, 0.14f));
                        break;
                    case BodyGear.Leather:
                        // Karwasze na przedramionach (łucznik, nożownik).
                        for (int k = 0; k < 3; k++) P(fa, Tor(0.2f), Surface.Leather, Le * 0.6f, new Vector3(0, -0.06f - k * 0.05f, 0), new Vector3(0.12f, 0.2f, 0.12f));
                        P(fa, Tube(0.9f), Surface.Leather, Le * 0.85f, new Vector3(0, -0.11f, 0), new Vector3(0.108f, 0.14f, 0.108f));
                        break;
                    case BodyGear.Plate:
                    {
                        Surface ms = look.style == RigStyle.Castellan ? Surface.DarkMetal : Surface.Metal;
                        for (int k = 0; k < 6; k++)
                        {
                            float a = k / 6f * Mathf.PI * 2f;
                            P(ua, Sph, Surface.Gold, Tr, new Vector3(sg * 0.02f + Mathf.Cos(a) * 0.1f, 0.03f, Mathf.Sin(a) * 0.1f), Vector3.one * 0.014f);
                        }
                        for (int k = 0; k < 3; k++) P(th, Sph, Surface.Gold, Tr, new Vector3(sg * 0.08f, -0.1f - k * 0.09f, 0.02f), Vector3.one * 0.014f);
                        P(sh, Tor(0.15f), Surface.Chain, Ch, new Vector3(0, 0.03f, 0), new Vector3(0.14f, 0.2f, 0.14f));
                        if (look.style == RigStyle.Castellan)
                        {
                            for (int k = 0; k < 2; k++) P(fa, Cone, Surface.DarkMetal, A * 1.3f, new Vector3(sg * 0.06f, -0.1f - k * 0.08f, -0.02f), new Vector3(0.025f, 0.08f, 0.025f), new Vector3(0, 0, sg * -80f));
                            P(sh, Cone, Surface.DarkMetal, A * 1.3f, new Vector3(0, 0.02f, 0.09f), new Vector3(0.035f, 0.1f, 0.035f), new Vector3(80f, 0, 0));
                            for (int k = 0; k < 3; k++) P(sh, Box, Surface.Glow, new Color(1f, 0.35f, 0.1f), new Vector3(0, -0.12f - k * 0.07f, 0.075f), new Vector3(0.03f, 0.03f, 0.004f), new Vector3(0, 0, 45f));
                            for (int k = 0; k < 2; k++) P(this[left ? Bone.HandL : Bone.HandR], Cone, Surface.DarkMetal, A * 1.3f, new Vector3(sg * 0.025f, -0.05f - k * 0.03f, 0.03f - k * 0.03f), new Vector3(0.015f, 0.04f, 0.015f), new Vector3(0, 0, sg * -90f));
                        }
                        break;
                    }
                    case BodyGear.Robe:
                        for (int k = 0; k < 6; k++)
                        {
                            float a = k / 6f * 360f;
                            float rad = a * Mathf.Deg2Rad;
                            P(fa, Box, Surface.Cloth, Cl * 0.8f, new Vector3(Mathf.Sin(rad) * 0.075f, -0.15f, Mathf.Cos(rad) * 0.075f), new Vector3(0.01f, 0.2f, 0.01f), new Vector3(-8f, a, 0));
                        }
                        P(ua, Tor(0.1f), Surface.Gold, Tr, new Vector3(0, -0.2f, 0), new Vector3(0.145f, 0.25f, 0.145f));
                        break;
                    case BodyGear.Rags:
                        for (int k = 0; k < 2; k++)
                            P(th, Tube(0.6f), Surface.Skin, new Color(0.25f, 0.3f, 0.2f), new Vector3((k == 0 ? -1 : 1) * 0.04f, -0.2f, 0.05f), new Vector3(0.008f, 0.3f, 0.008f));
                        for (int k = 0; k < 2; k++)
                            P(sh, Tube(0.6f), Surface.Skin, new Color(0.25f, 0.3f, 0.2f), new Vector3((k == 0 ? -1 : 1) * 0.03f, -0.2f, -0.04f), new Vector3(0.008f, 0.3f, 0.008f));
                        for (int k = 0; k < 2; k++)
                            P(ft, Box, Surface.Skin, Sk * 0.95f, new Vector3((k == 0 ? -1 : 1) * 0.042f, -0.045f, 0.15f), new Vector3(0.018f, 0.022f, 0.04f));
                        P(ua, Box, Surface.Skin, new Color(0.3f, 0.08f, 0.06f), new Vector3(sg * 0.055f, -0.12f, 0), new Vector3(0.01f, 0.08f, 0.02f), new Vector3(0, 0, sg * 10f));
                        P(th, Box, Surface.Cloth, Cl * 0.7f, new Vector3(sg * 0.06f, -0.05f, 0), new Vector3(0.012f, 0.12f, 0.06f));
                        break;
                }
            }

            switch (look.body)
            {
                case BodyGear.Chain:
                    // Szwy wzdłuż lamówki tabardu, kółka na czepcu, sprzączka pasa
                    for (int i = 0; i < 12; i++)
                        P(chest, Sph, Surface.Gold, Tr * 0.8f, new Vector3((i % 2 == 0 ? -1 : 1) * 0.125f * w, -0.08f + (i / 2) * 0.06f, 0.131f), Vector3.one * 0.01f);
                    P(spine, Box, Surface.Leather, Le * 0.6f, new Vector3(0.05f, -0.035f, 0.125f), new Vector3(0.05f, 0.014f, 0.008f));
                    for (int k = 0; k < 3; k++) P(head, Tor(0.12f), Surface.Chain, Ch * 0.8f, new Vector3(0, 0.02f + k * 0.07f, -0.035f), new Vector3(0.245f - Mathf.Abs(k - 1) * 0.02f, 0.2f, 0.255f), new Vector3(8f, 0, 0));
                    break;
                case BodyGear.Plate:
                    if (look.style == RigStyle.Warden)
                    {
                        for (int k = 0; k < 12; k++)
                        {
                            float a = k / 12f * Mathf.PI * 2f;
                            P(hips, Sph, Surface.Metal, A * 1.2f, new Vector3(Mathf.Sin(a) * 0.19f * w, 0.02f, Mathf.Cos(a) * 0.132f), Vector3.one * 0.016f);
                        }
                        for (int k = 0; k < 4; k++) P(head, Box, Surface.DarkMetal, A * 0.5f, new Vector3(-0.045f + k * 0.03f, 0.1f, 0.146f), new Vector3(0.008f, 0.08f, 0.008f));
                        for (int s2 = -1; s2 <= 1; s2 += 2) P(chest, Box, Surface.Gold, Tr, new Vector3(s2 * 0.11f * w, 0.02f, 0.176f), new Vector3(0.008f, 0.26f, 0.006f));
                        // Nity na naramiennikach i napierśniku, łańcuch na piersi, klamry pasów
                        for (int k = 0; k < 8; k++)
                            P(chest, Sph, Surface.Metal, A * 1.2f, new Vector3((-0.14f + k * 0.04f) * w, 0.27f, 0.12f), Vector3.one * 0.014f);
                        for (int k = 0; k < 8; k++)
                            P(chest, Tor(0.25f), Surface.DarkMetal, A * 0.5f, new Vector3((-0.14f + k * 0.04f) * w, 0.2f - Mathf.Sin(k / 7f * Mathf.PI) * 0.06f, 0.16f), new Vector3(0.03f, 0.3f, 0.045f), new Vector3(90f, k % 2 == 0 ? 0f : 90f, 0));
                        for (int k = 0; k < 4; k++)
                            P(spine, Box, Surface.Gold, Tr, new Vector3((-0.12f + k * 0.08f) * w, -0.01f, 0.126f), new Vector3(0.02f, 0.03f, 0.008f));
                    }
                    if (look.style == RigStyle.Castellan)
                    {
                        // Czaszka na napierśniku, tatry peleryny, obręcze na rogach
                        P(chest, Sph, Surface.Bone, new Color(0.75f, 0.7f, 0.58f), new Vector3(0, 0.16f, 0.17f), new Vector3(0.08f, 0.08f, 0.04f));
                        for (int e = -1; e <= 1; e += 2) P(chest, Sph, Surface.Glow, new Color(1f, 0.3f, 0.1f), new Vector3(e * 0.018f, 0.165f, 0.188f), Vector3.one * 0.016f);
                        P(chest, Fr(0.8f, 0.8f), Surface.Bone, new Color(0.7f, 0.65f, 0.55f), new Vector3(0, 0.12f, 0.175f), new Vector3(0.05f, 0.03f, 0.02f));
                        for (int e = -1; e <= 1; e += 2)
                            for (int k = 0; k < 2; k++)
                                P(head, Tor(0.25f), Surface.Gold, Tr, new Vector3(e * (0.16f + k * 0.045f), 0.24f + k * 0.07f, 0), new Vector3(0.065f, 0.3f, 0.065f), new Vector3(0, 0, e * -35f));
                        if (CapeLower != null)
                            for (int i = 0; i < 6; i++)
                                P(CapeLower, Box, Surface.Cloth, Cl * 0.5f, new Vector3((-0.2f + i * 0.08f) * w, -0.3f + (i % 2) * 0.05f, -0.015f), new Vector3(0.03f, 0.05f, 0.01f), new Vector3(0, 0, 30f * (i % 2 == 0 ? 1 : -1)));
                    }
                    break;
                case BodyGear.Robe:
                {
                    // Fałdy pelerynki, wzór na szarfie, dodatkowy haft na spódnicy, szwy kaptura, koraliki
                    for (int i = 0; i < 12; i++)
                    {
                        float a = (i / 11f - 0.5f) * 300f;
                        float rad = a * Mathf.Deg2Rad;
                        P(chest, Box, Surface.Cloth, Cl * 0.55f, new Vector3(Mathf.Sin(rad) * 0.2f * w, 0.26f, Mathf.Cos(rad) * 0.13f), new Vector3(0.01f, 0.12f, 0.01f), new Vector3(-30f, a, 0));
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i / 8f * Mathf.PI * 2f;
                        P(spine, Box, Surface.Gold, Tr, new Vector3(Mathf.Sin(a) * 0.172f * w, 0f, Mathf.Cos(a) * 0.126f), new Vector3(0.02f, 0.02f, 0.005f), new Vector3(0, a * Mathf.Rad2Deg, 45f));
                    }
                    for (int i = 0; i < 20; i++)
                    {
                        float a = i / 20f * 360f + 9f;
                        float rad = a * Mathf.Deg2Rad;
                        P(hips, Box, look.style == RigStyle.Heretic ? Surface.Glow : Surface.Gold, look.style == RigStyle.Heretic ? new Color(0.5f, 0.2f, 0.7f) : Tr * 0.8f, new Vector3(Mathf.Sin(rad) * 0.2f * w, -0.62f, Mathf.Cos(rad) * 0.168f), new Vector3(0.018f, 0.018f, 0.004f), new Vector3(-7f, a, 45f));
                    }
                    for (int k = 0; k < 4; k++)
                        P(head, Box, Surface.Cloth, Cl * 0.6f, new Vector3(0, 0.12f + k * 0.012f, -0.02f - k * 0.045f), new Vector3(0.008f, 0.01f, 0.05f), new Vector3(-20f - k * 15f, 0, 0));
                    for (int i = 0; i < 8; i++)
                    {
                        float a = (i / 7f - 0.5f) * 120f * Mathf.Deg2Rad;
                        P(chest, Sph, Surface.Bone, i % 2 == 0 ? new Color(0.2f, 0.4f, 0.3f) : new Color(0.55f, 0.45f, 0.3f), new Vector3(Mathf.Sin(a) * 0.075f, 0.2f - Mathf.Cos(a) * 0.035f, 0.115f), Vector3.one * 0.016f);
                    }
                    P(chest, Box, Surface.Leather, Le * 0.8f, new Vector3(-0.06f, 0.05f, 0.125f), new Vector3(0.022f, 0.35f, 0.008f), new Vector3(0, 0, -30f));
                    if (look.style == RigStyle.Heretic)
                    {
                        Color bone = new Color(0.75f, 0.7f, 0.58f);
                        for (int k = 0; k < 2; k++)
                        {
                            Vector3 sp = new Vector3(-0.13f + k * 0.26f, -0.08f, 0.13f);
                            P(spine, Sph, Surface.Bone, bone, sp, new Vector3(0.055f, 0.05f, 0.06f));
                            P(spine, Fr(0.8f, 0.8f), Surface.Bone, bone * 0.9f, sp + new Vector3(0, -0.025f, 0.01f), new Vector3(0.035f, 0.02f, 0.035f));
                            for (int e = -1; e <= 1; e += 2) P(spine, Sph, Surface.Glow, new Color(0.7f, 0.3f, 1f), sp + new Vector3(e * 0.013f, 0.004f, 0.026f), Vector3.one * 0.012f);
                            P(spine, Tube(1f), Surface.Leather, Le * 0.6f, sp + new Vector3(0, 0.05f, 0), new Vector3(0.006f, 0.06f, 0.006f));
                        }
                        for (int i = 0; i < 8; i++)
                            P(chest, Box, Surface.Glow, new Color(0.65f, 0.25f, 0.95f), new Vector3(0.1f * Mathf.Cos(i * 0.8f), 0.05f + i * 0.025f, 0.118f), new Vector3(0.012f, 0.012f, 0.004f), new Vector3(0, 0, 45f));
                    }
                    break;
                }
                case BodyGear.Rags:
                {
                    Color bone = Sk * 1.12f;
                    for (int s2 = -1; s2 <= 1; s2 += 2)
                        for (int k = 0; k < 3; k++)
                            P(chest, Box, Surface.Bone, bone * 0.9f, new Vector3(s2 * 0.035f, 0.03f + k * 0.065f, 0.1f), new Vector3(0.025f, 0.012f, 0.02f), new Vector3(0, 0, s2 * 30f));
                    for (int s2 = -1; s2 <= 1; s2 += 2) P(chest, Sph, Surface.Bone, bone, new Vector3(s2 * 0.2f * w, 0.28f, 0f), new Vector3(0.05f, 0.04f, 0.05f));
                    for (int k = 0; k < 4; k++) P(hips, Box, Surface.Leather, new Color(0.4f, 0.33f, 0.2f), new Vector3(-0.06f + k * 0.04f, -0.08f - (k % 2) * 0.03f, 0.105f), new Vector3(0.006f, 0.1f, 0.006f), new Vector3(0, 0, (k - 1.5f) * 10f));
                    for (int i = 0; i < 6; i++) P(chest, Box, Surface.Dark, new Color(0.25f, 0.12f, 0.1f), new Vector3(-0.1f + i * 0.04f, -0.02f + (i % 3) * 0.1f, -0.098f), new Vector3(0.04f, 0.006f, 0.004f), new Vector3(0, 0, (i - 2.5f) * 25f));
                    for (int i = 0; i < 8; i++) P(head, Box, Surface.Hair, new Color(0.22f, 0.22f, 0.2f), new Vector3(-0.08f + i * 0.023f, 0.17f - Mathf.Abs(i - 3.5f) * 0.01f, -0.05f - (i % 2) * 0.02f), new Vector3(0.006f, 0.11f, 0.006f), new Vector3(-50f + (i % 3) * 10f, 0, (i - 3.5f) * 9f));
                    for (int k = 0; k < 4; k++) P(spine, Sph, Surface.Bone, bone, new Vector3(0, -0.03f + k * 0.055f, -0.085f), new Vector3(0.028f, 0.025f, 0.025f));
                    for (int i = 0; i < 4; i++) P(hips, Box, Surface.Cloth, Cl * 0.6f, new Vector3(-0.08f + i * 0.05f, -0.34f, 0.1f), new Vector3(0.02f, 0.06f, 0.008f), new Vector3(-5f, 0, (i - 1.5f) * 15f));
                    // Obojczyki, mostek, łopatki, kręgi szyi, strzępy skóry, kolce kręgosłupa
                    for (int s2 = -1; s2 <= 1; s2 += 2)
                    {
                        for (int k = 0; k < 3; k++) P(chest, Box, Surface.Skin, Sk * 0.85f, new Vector3(s2 * (0.12f + k * 0.02f) * w, 0.05f + k * 0.08f, -0.095f), new Vector3(0.03f, 0.02f, 0.008f), new Vector3(0, 0, s2 * 25f));
                        P(chest, Tube(0.6f), Surface.Skin, new Color(0.25f, 0.3f, 0.2f), new Vector3(s2 * 0.05f, 0.3f, 0.06f), new Vector3(0.01f, 0.12f, 0.01f), new Vector3(0, 0, s2 * 60f));
                    }
                    for (int k = 0; k < 3; k++) P(this[Bone.Neck], Sph, Surface.Bone, bone, new Vector3(0, -0.01f + k * 0.035f, -0.045f), new Vector3(0.028f, 0.022f, 0.022f));
                    for (int k = 0; k < 5; k++) P(chest, Cone, Surface.Bone, bone, new Vector3(0, 0.0f + k * 0.07f, -0.12f), new Vector3(0.018f, 0.04f, 0.018f), new Vector3(-90f, 0, 0));
                    for (int k = 0; k < 4; k++) P(hips, Box, Surface.Skin, Sk * 0.8f, new Vector3(-0.1f + k * 0.065f, -0.02f, -0.09f), new Vector3(0.03f, 0.04f, 0.006f), new Vector3(0, 0, (k - 1.5f) * 20f));
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ Głowa

        void Face(Transform head, RigLook look, float forward, bool hair)
        {
            Color Sk = look.skin;
            P(head, Sph, Surface.Skin, Sk, new Vector3(0, 0.1f, 0.005f + forward), new Vector3(0.19f, 0.23f, 0.21f));
            P(head, Fr(0.85f, 0.9f), Surface.Skin, Sk, new Vector3(0, 0.035f, 0.03f + forward), new Vector3(0.15f, 0.08f, 0.13f));
            P(head, Fr(0.6f, 0.5f, 0.01f), Surface.Skin, Sk * 0.95f, new Vector3(0, 0.085f, 0.105f + forward), new Vector3(0.03f, 0.05f, 0.03f), new Vector3(-10f, 0, 0));
            P(head, Box, Surface.Skin, Sk * 0.88f, new Vector3(0, 0.127f, 0.093f + forward), new Vector3(0.13f, 0.02f, 0.03f));
            P(head, Box, Surface.Dark, new Color(0.25f, 0.1f, 0.08f), new Vector3(0, 0.045f, 0.094f + forward), new Vector3(0.05f, 0.008f, 0.01f));
            for (int s = -1; s <= 1; s += 2)
            {
                P(head, Sph, look.glowingEyes ? Surface.Glow : Surface.Dark, look.eyes, new Vector3(s * 0.035f, 0.105f, 0.097f + forward), new Vector3(0.028f, 0.018f, 0.012f));
                P(head, Sph, Surface.Skin, Sk * 0.95f, new Vector3(s * 0.097f, 0.095f, forward), new Vector3(0.025f, 0.05f, 0.035f));
                P(head, Box, Surface.Skin, Sk * 0.9f, new Vector3(s * 0.035f, 0.116f, 0.096f + forward), new Vector3(0.034f, 0.008f, 0.012f), new Vector3(0, 0, s * -6f));
                P(head, Sph, Surface.Skin, Sk * 1.02f, new Vector3(s * 0.058f, 0.075f, 0.075f + forward), new Vector3(0.05f, 0.035f, 0.04f));
                P(head, Sph, Surface.Dark, new Color(0.2f, 0.1f, 0.08f), new Vector3(s * 0.008f, 0.066f, 0.118f + forward), new Vector3(0.008f, 0.006f, 0.006f));
                P(head, Sph, Surface.Skin, Sk * 0.93f, new Vector3(s * 0.098f, 0.1f, 0.012f + forward), new Vector3(0.012f, 0.028f, 0.018f));
            }
            P(head, Box, Surface.Skin, Sk * 0.75f, new Vector3(0, 0.051f, 0.097f + forward), new Vector3(0.042f, 0.006f, 0.01f));
            P(head, Box, Surface.Skin, Sk * 0.8f, new Vector3(0, 0.039f, 0.095f + forward), new Vector3(0.036f, 0.007f, 0.01f));
            P(head, Sph, Surface.Skin, Sk, new Vector3(0, 0.012f, 0.085f + forward), new Vector3(0.045f, 0.035f, 0.035f));
            if (hair)
            {
                Color h = new Color(0.22f, 0.14f, 0.09f);
                P(head, Sph, Surface.Hair, h, new Vector3(0, 0.145f, -0.015f), new Vector3(0.205f, 0.18f, 0.215f));
                P(head, Box, Surface.Hair, h, new Vector3(0, 0.19f, 0.085f), new Vector3(0.15f, 0.03f, 0.03f), new Vector3(20f, 0, 0));
                P(head, Fr(0.8f, 0.8f), Surface.Hair, h, new Vector3(0, 0.02f, 0.07f), new Vector3(0.13f, 0.06f, 0.08f));
            }
        }

        void BuildHead(RigLook look)
        {
            var head = this[Bone.Head];
            var neck = this[Bone.Neck];
            Color A = look.armor, Ch = look.armor * 0.85f, Tr = look.trim;
            Color dark = new Color(0.04f, 0.04f, 0.045f);
            P(neck, Tube(0.9f), Surface.Skin, look.skin * 0.95f, new Vector3(0, 0.03f, 0), new Vector3(0.1f, 0.1f, 0.1f));

            switch (look.head)
            {
                case HeadGear.GreatHelm:
                case HeadGear.HornedHelm:
                {
                    bool horned = look.head == HeadGear.HornedHelm;
                    Surface ms = horned ? Surface.DarkMetal : Surface.Metal;
                    P(head, Fr(0.94f, 0.96f), ms, A, new Vector3(0, 0.1f, 0.01f), new Vector3(0.25f, 0.24f, 0.27f));
                    P(head, Dome, ms, A, new Vector3(0, 0.22f, 0.01f), new Vector3(0.235f, 0.16f, 0.255f));
                    P(head, Box, ms, A * 1.12f, new Vector3(0, 0.137f, 0.14f), new Vector3(0.24f, 0.03f, 0.012f));
                    if (horned)
                        P(head, Box, Surface.Glow, look.eyes, new Vector3(0, 0.115f, 0.142f), new Vector3(0.18f, 0.018f, 0.01f));
                    else
                        for (int s = -1; s <= 1; s += 2) P(head, Box, Surface.Dark, dark, new Vector3(s * 0.058f, 0.115f, 0.143f), new Vector3(0.078f, 0.015f, 0.01f));
                    P(head, Box, ms, A * 1.1f, new Vector3(0, 0.08f, 0.146f), new Vector3(0.022f, 0.1f, 0.012f));
                    for (int i = 0; i < 6; i++)
                        P(head, Sph, Surface.Dark, dark, new Vector3(0.05f + (i % 2) * 0.024f, 0.07f - (i / 2) * 0.022f, 0.141f), new Vector3(0.011f, 0.011f, 0.006f));
                    P(head, Tor(0.15f), ms, A * 0.9f, new Vector3(0, -0.012f, 0.01f), new Vector3(0.268f, 0.2f, 0.288f));
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i / 10f * Mathf.PI * 2f;
                        P(head, Sph, Surface.Gold, Tr, new Vector3(Mathf.Sin(a) * 0.132f, -0.012f, 0.01f + Mathf.Cos(a) * 0.142f), Vector3.one * 0.014f);
                    }
                    P(head, Box, Surface.Gold, Tr, new Vector3(0, 0.285f, 0.01f), new Vector3(0.02f, 0.035f, 0.23f));
                    P(head, Tube(1.15f), Surface.Chain, Ch, new Vector3(0, -0.055f, -0.02f), new Vector3(0.24f, 0.08f, 0.24f));
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(head, Box, ms, A * 0.8f, new Vector3(s * 0.127f, 0.1f, 0.06f), new Vector3(0.012f, 0.05f, 0.03f));
                        P(head, Sph, Surface.Gold, Tr, new Vector3(s * 0.13f, 0.1f, 0.06f), Vector3.one * 0.016f);
                        P(head, Box, Surface.Gold, Tr, new Vector3(s * 0.06f, 0.16f, 0.141f), new Vector3(0.012f, 0.03f, 0.008f));
                    }
                    P(head, Box, Surface.Gold, Tr, new Vector3(0, 0.05f, 0.148f), new Vector3(0.05f, 0.012f, 0.008f));
                    if (!horned && look.style != RigStyle.Warden)
                    {
                        // Pióropusz
                        Color plume = look.cloth * 1.1f;
                        P(head, Tube(0.8f), Surface.Gold, Tr, new Vector3(0, 0.3f, -0.03f), new Vector3(0.03f, 0.06f, 0.03f));
                        for (int i = 0; i < 7; i++)
                        {
                            float t = i / 6f;
                            P(head, Limb(0.5f), Surface.Cloth, i % 2 == 0 ? plume : plume * 0.8f, new Vector3(0, 0.34f + Mathf.Sin(t * 2.4f) * 0.08f, -0.03f - t * 0.2f), new Vector3(0.035f, 0.16f, 0.07f), new Vector3(-30f - t * 70f, 0, 0));
                        }
                    }
                    if (horned)
                    {
                        Color hc = new Color(0.8f, 0.72f, 0.55f);
                        for (int s = -1; s <= 1; s += 2)
                        {
                            P(head, Tube(0.72f), Surface.Bone, hc, new Vector3(s * 0.14f, 0.22f, 0), new Vector3(0.065f, 0.13f, 0.065f), new Vector3(0, 0, s * -42f));
                            P(head, Tube(0.62f), Surface.Bone, hc * 0.95f, new Vector3(s * 0.205f, 0.31f, 0), new Vector3(0.048f, 0.11f, 0.048f), new Vector3(0, 0, s * -18f));
                            P(head, Cone, Surface.Bone, hc * 0.9f, new Vector3(s * 0.225f, 0.405f, 0), new Vector3(0.032f, 0.1f, 0.032f), new Vector3(0, 0, s * 5f));
                        }
                        for (int i = 0; i < 5; i++)
                            P(head, Cone, Surface.DarkMetal, A * 1.2f, new Vector3((i - 2) * 0.045f, 0.3f, 0.04f - Mathf.Abs(i - 2) * 0.02f), new Vector3(0.025f, 0.07f, 0.025f));
                    }
                    break;
                }
                case HeadGear.Coif:
                    Face(head, look, 0.01f, false);
                    P(head, Sph, Surface.Chain, Ch, new Vector3(0, 0.11f, -0.035f), new Vector3(0.24f, 0.27f, 0.25f));
                    P(head, Tube(1.3f), Surface.Chain, Ch, new Vector3(0, -0.02f, -0.01f), new Vector3(0.22f, 0.1f, 0.22f));
                    P(head, Tor(0.2f), Surface.Cloth, look.cloth * 0.5f, new Vector3(0, 0.09f, 0.085f), new Vector3(0.17f, 0.2f, 0.21f), new Vector3(90f, 0, 0));
                    P(head, Tor(0.16f), Surface.Chain, Ch * 1.1f, new Vector3(0, -0.07f, -0.005f), new Vector3(0.28f, 0.2f, 0.27f));
                    break;
                case HeadGear.Hood:
                {
                    Color hc = look.cloth * 0.85f;
                    // Twarz w cieniu kaptura; kaptur to otwarta powłoka z otworem z przodu i spiczastym tyłem.
                    var shaded = new RigLook { skin = look.skin * 0.7f, eyes = look.eyes, glowingEyes = look.glowingEyes };
                    Face(head, shaded, -0.015f, false);
                    P(head, ProcMesh.Shell(), Surface.Cloth, hc, new Vector3(0, 0.115f, 0.075f), new Vector3(0.28f, 0.44f, 0.31f), new Vector3(-90f, 0, 0));
                    P(head, Cone, Surface.Cloth, hc, new Vector3(0, 0.16f, -0.19f), new Vector3(0.14f, 0.2f, 0.13f), new Vector3(-62f, 0, 0));
                    P(head, Tor(0.14f), Surface.Cloth, look.cloth * 0.6f, new Vector3(0, 0.115f, 0.075f), new Vector3(0.28f, 0.3f, 0.31f), new Vector3(90f, 0, 0));
                    P(head, Bell(0.55f, 0.02f), Surface.Cloth, hc, new Vector3(0, -0.06f, -0.01f), new Vector3(0.34f, 0.14f, 0.3f));
                    if (look.style == RigStyle.Mage)
                    {
                        Color beard = new Color(0.72f, 0.7f, 0.66f);
                        P(head, Cone, Surface.Hair, beard, new Vector3(0, -0.04f, 0.085f), new Vector3(0.12f, 0.2f, 0.06f), new Vector3(170f, 0, 0));
                        P(head, Sph, Surface.Hair, beard, new Vector3(0, 0.03f, 0.085f), new Vector3(0.13f, 0.08f, 0.06f));
                        for (int s = -1; s <= 1; s += 2)
                            P(head, Box, Surface.Hair, beard, new Vector3(s * 0.03f, 0.058f, 0.1f), new Vector3(0.05f, 0.014f, 0.016f), new Vector3(0, 0, s * -18f));
                        P(head, Box, Surface.Hair, beard * 0.95f, new Vector3(0, 0.13f, 0.098f), new Vector3(0.13f, 0.018f, 0.02f));
                    }
                    break;
                }
                case HeadGear.Skull:
                {
                    Color Sk = look.skin, bone = Sk * 1.15f;
                    Color teeth = new Color(0.85f, 0.8f, 0.6f);
                    P(head, Sph, Surface.Skin, Sk, new Vector3(0, 0.11f, 0.01f), new Vector3(0.2f, 0.22f, 0.23f));
                    P(head, Box, Surface.Bone, bone, new Vector3(0, 0.14f, 0.1f), new Vector3(0.15f, 0.025f, 0.035f));
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(head, Sph, Surface.Dark, new Color(0.05f, 0.03f, 0.02f), new Vector3(s * 0.045f, 0.112f, 0.1f), new Vector3(0.05f, 0.045f, 0.03f));
                        P(head, Sph, Surface.Glow, look.eyes, new Vector3(s * 0.045f, 0.112f, 0.112f), new Vector3(0.018f, 0.018f, 0.01f));
                        P(head, Sph, Surface.Dark, new Color(0.15f, 0.12f, 0.08f), new Vector3(s * 0.075f, 0.06f, 0.085f), new Vector3(0.035f, 0.04f, 0.02f));
                        P(head, Cone, Surface.Skin, Sk, new Vector3(s * 0.105f, 0.13f, 0), new Vector3(0.03f, 0.08f, 0.035f), new Vector3(0, 0, s * -62f));
                    }
                    P(head, Fr(0.4f, 0.6f), Surface.Dark, new Color(0.05f, 0.03f, 0.02f), new Vector3(0, 0.075f, 0.115f), new Vector3(0.03f, 0.03f, 0.015f));
                    P(head, Fr(0.8f, 0.8f), Surface.Skin, Sk * 0.95f, new Vector3(0, 0.015f, 0.05f), new Vector3(0.13f, 0.06f, 0.12f));
                    for (int i = 0; i < 6; i++)
                        P(head, Box, Surface.Bone, teeth, new Vector3(-0.035f + i * 0.014f, 0.048f, 0.113f), new Vector3(0.009f, 0.02f, 0.008f));
                    for (int i = 0; i < 4; i++)
                        P(head, Box, Surface.Bone, teeth, new Vector3(-0.021f + i * 0.014f, 0.03f, 0.108f), new Vector3(0.009f, 0.018f, 0.008f));
                    for (int i = 0; i < 4; i++)
                        P(head, Box, Surface.Hair, new Color(0.2f, 0.2f, 0.18f), new Vector3(-0.05f + i * 0.035f, 0.19f, -0.07f), new Vector3(0.008f, 0.14f, 0.008f), new Vector3(-35f, 0, (i - 1.5f) * 12f));
                    break;
                }
                default:
                    Face(head, look, 0f, true);
                    break;
            }
        }

        // ------------------------------------------------------------------ Ręce

        void BuildArm(RigLook look, bool left)
        {
            float sg = left ? -1f : 1f;
            float t = look.limbThickness;
            var ua = this[left ? Bone.UpperArmL : Bone.UpperArmR];
            var fa = this[left ? Bone.ForearmL : Bone.ForearmR];
            var hand = this[left ? Bone.HandL : Bone.HandR];
            Color A = look.armor, Ad = look.armor * 0.85f, Ch = look.armor * 0.85f, Cl = look.cloth, Tr = look.trim, Le = look.leather, Sk = look.skin;
            float UA = UpperArmLength, FA = ForearmLength;

            Surface limbS; Color limbC; Surface handS; Color handC;
            switch (look.body)
            {
                case BodyGear.Plate: limbS = Surface.Chain; limbC = Ch; handS = Surface.Metal; handC = A * 0.9f; break;
                case BodyGear.Chain: limbS = Surface.Chain; limbC = Ch; handS = Surface.Leather; handC = Le; break;
                case BodyGear.Leather: limbS = Surface.Leather; limbC = Le * 0.95f; handS = Surface.Leather; handC = Le * 0.7f; break;
                case BodyGear.Robe: limbS = Surface.Cloth; limbC = Cl; handS = Surface.Skin; handC = Sk; break;
                case BodyGear.Rags: limbS = Surface.Skin; limbC = Sk; handS = Surface.Skin; handC = Sk; break;
                default: limbS = Surface.Cloth; limbC = Cl * 0.8f; handS = Surface.Leather; handC = Le; break;
            }

            P(ua, Sph, limbS, limbC, new Vector3(0, -0.02f, 0), Vector3.one * 0.13f * t);
            P(ua, Limb(0.8f), limbS, limbC, new Vector3(0, -UA * 0.5f, 0), new Vector3(0.115f * t, UA, 0.115f * t));
            P(fa, Sph, limbS, limbC, Vector3.zero, Vector3.one * 0.1f * t);
            P(fa, Limb(0.75f), look.body == BodyGear.Tunic ? Surface.Skin : limbS, look.body == BodyGear.Tunic ? Sk : limbC, new Vector3(0, -FA * 0.5f, 0), new Vector3(0.1f * t, FA, 0.1f * t));

            // Dłoń: śródręcze, palce, kciuk, kostki
            P(hand, Fr(0.9f, 0.85f), handS, handC, new Vector3(0, -0.045f, 0), new Vector3(0.04f, 0.08f, 0.085f));
            for (int f = 0; f < 4; f++)
            {
                float fz = -0.03f + f * 0.02f;
                float len = f == 1 || f == 2 ? 1f : 0.85f;
                P(hand, Sph, handS, handC * 0.92f, new Vector3(0, -0.086f, fz), new Vector3(0.024f, 0.02f, 0.02f));
                P(hand, Limb(0.85f), handS, handC, new Vector3(0, -0.107f, fz + 0.002f), new Vector3(0.019f, 0.042f * len, 0.018f), new Vector3(12f, 0, 0));
                P(hand, Limb(0.8f), handS, handC * 0.96f, new Vector3(0, -0.137f * len - 0.005f, fz + 0.012f), new Vector3(0.017f, 0.034f * len, 0.016f), new Vector3(38f, 0, 0));
            }
            P(hand, Limb(0.85f), handS, handC * 0.95f, new Vector3(-sg * 0.004f, -0.04f, 0.045f), new Vector3(0.022f, 0.04f, 0.022f), new Vector3(-40f, 0, sg * 15f));
            P(hand, Limb(0.8f), handS, handC * 0.95f, new Vector3(-sg * 0.006f, -0.06f, 0.068f), new Vector3(0.019f, 0.032f, 0.019f), new Vector3(-10f, 0, sg * 10f));

            switch (look.body)
            {
                case BodyGear.Plate:
                {
                    Surface ms = look.style == RigStyle.Castellan ? Surface.DarkMetal : Surface.Metal;
                    P(ua, Tube(0.85f), ms, A, new Vector3(0, -0.15f, 0), new Vector3(0.13f * t, 0.19f, 0.13f * t));
                    P(ua, Dome, ms, A, new Vector3(sg * 0.02f, 0.0f, 0), new Vector3(0.23f * look.bulk, 0.15f, 0.23f), new Vector3(0, 0, sg * -12f));
                    for (int k = 0; k < 2; k++)
                        P(ua, Tube(0.95f), ms, k == 0 ? Ad : A, new Vector3(sg * 0.01f, -0.035f - k * 0.035f, 0), new Vector3(0.21f - 0.025f * k, 0.035f, 0.21f - 0.025f * k));
                    P(ua, Tor(0.12f), Surface.Gold, Tr, new Vector3(sg * 0.02f, 0.0f, 0), new Vector3(0.23f * look.bulk, 0.1f, 0.23f), new Vector3(0, 0, sg * -12f));
                    P(fa, Dome, ms, A, new Vector3(0, 0f, -0.04f), new Vector3(0.1f, 0.08f, 0.1f), new Vector3(-90f, 0, 0));
                    P(fa, Box, ms, A, new Vector3(sg * 0.05f, 0f, -0.02f), new Vector3(0.015f, 0.085f, 0.07f));
                    P(fa, Tube(0.82f), ms, A, new Vector3(0, -0.15f, 0), new Vector3(0.118f * t, 0.19f, 0.118f * t));
                    P(fa, Tube(1.35f), ms, A * 0.95f, new Vector3(0, -0.26f, 0), new Vector3(0.1f, 0.07f, 0.1f));
                    P(hand, Box, ms, A, new Vector3(sg * 0.023f, -0.05f, 0), new Vector3(0.012f, 0.07f, 0.08f));
                    if (look.style == RigStyle.Castellan)
                        for (int k = 0; k < 2; k++)
                            P(ua, Cone, Surface.DarkMetal, A * 1.3f, new Vector3(sg * (0.07f + k * 0.03f), 0.05f - k * 0.03f, k * 0.04f - 0.02f), new Vector3(0.04f, 0.12f, 0.04f), new Vector3(0, 0, sg * -55f));
                    break;
                }
                case BodyGear.Chain:
                    P(ua, Dome, Surface.Leather, Le, new Vector3(sg * 0.01f, 0.0f, 0), new Vector3(0.17f, 0.1f, 0.17f), new Vector3(0, 0, sg * -10f));
                    for (int k = 0; k < 3; k++) P(ua, Sph, Surface.Gold, Tr, new Vector3(sg * 0.06f, 0.0f, -0.04f + k * 0.04f), Vector3.one * 0.013f);
                    P(fa, Tube(0.85f), Surface.Leather, Le, new Vector3(0, -0.17f, 0), new Vector3(0.112f, 0.16f, 0.112f));
                    for (int k = 0; k < 2; k++) P(fa, Tor(0.25f), Surface.Leather, Le * 0.65f, new Vector3(0, -0.12f - k * 0.08f, 0), new Vector3(0.115f, 0.15f, 0.115f));
                    P(fa, Tube(1.3f), Surface.Leather, Le * 0.85f, new Vector3(0, -0.265f, 0), new Vector3(0.1f, 0.06f, 0.1f));
                    break;
                case BodyGear.Robe:
                    P(ua, Tube(1.12f), Surface.Cloth, Cl, new Vector3(0, -0.13f, 0), new Vector3(0.14f, 0.26f, 0.14f));
                    P(fa, Bell(0.55f, 0.02f), Surface.Cloth, Cl, new Vector3(0, -0.15f, 0), new Vector3(0.2f, 0.25f, 0.2f));
                    P(fa, Tor(0.12f), Surface.Gold, Tr, new Vector3(0, -0.272f, 0), new Vector3(0.2f, 0.3f, 0.2f));
                    P(fa, Tube(0.9f), Surface.Cloth, Cl * 0.6f, new Vector3(0, -0.2f, 0), new Vector3(0.075f, 0.12f, 0.075f));
                    P(fa, Tor(0.1f), Surface.Cloth, look.style == RigStyle.Heretic ? Cl * 0.4f : new Color(0.75f, 0.72f, 0.62f), new Vector3(0, -0.265f, 0), new Vector3(0.18f, 0.3f, 0.18f));
                    P(ua, Tor(0.12f), Surface.Gold, Tr, new Vector3(0, -0.05f, 0), new Vector3(0.15f, 0.3f, 0.15f));
                    P(hand, Tor(0.35f), Surface.Gold, Tr, new Vector3(0, -0.1f, 0.03f), new Vector3(0.03f, 0.3f, 0.03f), new Vector3(90f, 0, 0));
                    P(hand, Sph, Surface.Glow, left ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.3f, 0.6f, 1f), new Vector3(0, -0.1f, 0.045f), Vector3.one * 0.012f);
                    break;
                case BodyGear.Rags:
                    P(fa, Sph, Surface.Bone, Sk * 1.15f, new Vector3(0, 0f, -0.03f), Vector3.one * 0.05f);
                    P(fa, Tor(0.35f), Surface.Cloth, look.cloth, new Vector3(0, -0.24f, 0), new Vector3(0.085f, 0.25f, 0.085f));
                    for (int i = -1; i <= 1; i++)
                    {
                        P(hand, Limb(0.5f), Surface.Bone, look.weaponColor, new Vector3(i * 0.013f, -0.165f, 0.012f), new Vector3(0.013f, 0.075f, 0.013f), new Vector3(15f, 0, 0));
                        P(hand, Cone, Surface.Bone, look.weaponColor * 0.9f, new Vector3(i * 0.013f, -0.215f, 0.032f), new Vector3(0.011f, 0.045f, 0.011f), new Vector3(150f, 0, 0));
                    }
                    break;
                default:
                    P(ua, Tube(1.05f), Surface.Cloth, Cl * 0.8f, new Vector3(0, -0.1f, 0), new Vector3(0.135f, 0.2f, 0.135f));
                    P(fa, Tube(0.85f), Surface.Leather, Le, new Vector3(0, -0.18f, 0), new Vector3(0.11f, 0.15f, 0.11f));
                    break;
            }
        }

        // ------------------------------------------------------------------ Nogi

        void BuildLeg(RigLook look, bool left)
        {
            float sg = left ? -1f : 1f;
            float t = look.limbThickness;
            var th = this[left ? Bone.ThighL : Bone.ThighR];
            var sh = this[left ? Bone.ShinL : Bone.ShinR];
            var ft = this[left ? Bone.FootL : Bone.FootR];
            Color A = look.armor, Ad = look.armor * 0.85f, Ch = look.armor * 0.85f, Le = look.leather, Ld = look.leather * 0.7f, Sk = look.skin, Tr = look.trim;
            Color sole = new Color(0.12f, 0.08f, 0.05f);

            if (look.body == BodyGear.Robe)
            {
                // Szata zakrywa nogi – widać tylko trzewiki.
                P(ft, Fr(0.9f, 0.85f), Surface.Leather, Le, new Vector3(0, -0.025f, 0.045f), new Vector3(0.1f, 0.06f, 0.22f));
                P(ft, Sph, Surface.Leather, Le, new Vector3(0, -0.03f, 0.15f), new Vector3(0.1f, 0.06f, 0.09f));
                P(ft, Box, Surface.Leather, sole, new Vector3(0, -0.06f, 0.05f), new Vector3(0.105f, 0.015f, 0.26f));
                return;
            }

            Surface ls; Color lc;
            switch (look.body)
            {
                case BodyGear.Plate:
                case BodyGear.Chain: ls = Surface.Chain; lc = Ch; break;
                case BodyGear.Rags: ls = Surface.Skin; lc = Sk; break;
                default: ls = Surface.Cloth; lc = look.cloth * 0.55f; break;
            }
            P(th, Sph, ls, lc, new Vector3(0, -0.01f, 0), Vector3.one * 0.15f * t);
            P(th, Limb(0.8f), ls, lc, new Vector3(0, -0.215f, 0), new Vector3(0.16f * t, 0.43f, 0.16f * t));
            P(sh, Sph, ls, lc, Vector3.zero, Vector3.one * 0.12f * t);
            P(sh, Limb(0.72f), ls, lc, new Vector3(0, -0.21f, 0), new Vector3(0.13f * t, 0.42f, 0.13f * t));

            if (look.body == BodyGear.Rags)
            {
                P(sh, Sph, Surface.Bone, Sk * 1.15f, new Vector3(0, 0, 0.04f), Vector3.one * 0.06f);
                P(sh, Tor(0.35f), Surface.Cloth, look.cloth, new Vector3(0, -0.36f, 0), new Vector3(0.09f, 0.25f, 0.09f));
                P(ft, Fr(0.8f, 0.8f), Surface.Skin, Sk, new Vector3(0, -0.03f, 0.05f), new Vector3(0.09f, 0.05f, 0.2f));
                for (int i = -1; i <= 1; i++)
                    P(ft, Box, Surface.Skin, Sk * 0.95f, new Vector3(i * 0.028f, -0.045f, 0.165f), new Vector3(0.022f, 0.025f, 0.05f));
                return;
            }

            if (look.body == BodyGear.Plate)
            {
                Surface ms = look.style == RigStyle.Castellan ? Surface.DarkMetal : Surface.Metal;
                P(th, Tube(0.85f), ms, A, new Vector3(0, -0.2f, 0.01f), new Vector3(0.17f * t, 0.3f, 0.17f * t));
                P(th, Box, ms, A * 1.1f, new Vector3(0, -0.2f, 0.09f * t), new Vector3(0.018f, 0.28f, 0.015f));
                P(sh, Dome, ms, A, new Vector3(0, 0f, 0.05f), new Vector3(0.12f, 0.08f, 0.12f), new Vector3(90f, 0, 0));
                P(sh, Box, ms, Ad, new Vector3(sg * 0.055f, 0f, 0.03f), new Vector3(0.015f, 0.075f, 0.06f));
                P(sh, Tube(0.8f), ms, A, new Vector3(0, -0.2f, 0), new Vector3(0.14f * t, 0.34f, 0.14f * t));
                P(sh, Box, ms, A * 1.1f, new Vector3(0, -0.2f, 0.07f * t), new Vector3(0.016f, 0.32f, 0.014f));
                for (int k = 0; k < 3; k++)
                    P(ft, Fr(0.9f, 0.9f), ms, k % 2 == 0 ? A : Ad, new Vector3(0, -0.02f, -0.01f + k * 0.055f), new Vector3(0.115f, 0.065f - 0.008f * k, 0.085f));
                P(ft, Sph, ms, A, new Vector3(0, -0.03f, 0.17f), new Vector3(0.1f, 0.06f, 0.085f));
                P(ft, Box, Surface.Leather, sole, new Vector3(0, -0.065f, 0.05f), new Vector3(0.115f, 0.018f, 0.27f));
                return;
            }

            // Buty z cholewą (kolczuga, tunika)
            P(sh, Tube(1.05f), Surface.Leather, Le, new Vector3(0, -0.3f, 0), new Vector3(0.14f, 0.22f, 0.14f));
            P(sh, Tor(0.25f), Surface.Leather, Ld, new Vector3(0, -0.19f, 0), new Vector3(0.15f, 0.3f, 0.15f));
            for (int k = 0; k < 2; k++) P(sh, Tor(0.2f), Surface.Leather, Ld * 0.8f, new Vector3(0, -0.26f - k * 0.08f, 0), new Vector3(0.143f, 0.2f, 0.143f));
            P(sh, Sph, Surface.Gold, Tr, new Vector3(sg * 0.07f, -0.26f, 0), Vector3.one * 0.015f);
            P(ft, Fr(0.9f, 0.85f), Surface.Leather, Le, new Vector3(0, -0.025f, 0.045f), new Vector3(0.11f, 0.07f, 0.22f));
            P(ft, Sph, Surface.Leather, Le, new Vector3(0, -0.03f, 0.155f), new Vector3(0.11f, 0.07f, 0.1f));
            P(ft, Box, Surface.Leather, sole, new Vector3(0, -0.065f, 0.05f), new Vector3(0.115f, 0.018f, 0.27f));
            P(ft, Box, Surface.Leather, sole, new Vector3(0, -0.055f, -0.06f), new Vector3(0.1f, 0.03f, 0.06f));
            if (look.body == BodyGear.Chain)
                P(sh, Dome, Surface.Leather, Ld, new Vector3(0, 0f, 0.045f), new Vector3(0.11f, 0.07f, 0.11f), new Vector3(90f, 0, 0));
        }

        // ------------------------------------------------------------------ Peleryna

        void BuildCape(RigLook look)
        {
            float w = look.bulk;
            var chest = this[Bone.Chest];
            Color c = look.cloth * 0.75f, Tr = look.trim;
            CapePivot = Node("CapePivot", chest, new Vector3(0, 0.27f, -0.15f));
            P(CapePivot, Box, Surface.Cloth, look.cloth * 0.6f, new Vector3(0, 0f, 0.02f), new Vector3(0.42f * w, 0.05f, 0.05f));
            P(CapePivot, Fr(0.85f, 1f), Surface.Cloth, c, new Vector3(0, -0.22f, 0), new Vector3(0.44f * w, 0.44f, 0.022f));
            CapeLower = Node("CapeLower", CapePivot, new Vector3(0, -0.44f, 0));
            P(CapeLower, Fr(0.95f, 1f), Surface.Cloth, c * 0.97f, new Vector3(0, -0.2f, 0), new Vector3(0.49f * w, 0.4f, 0.022f));
            P(CapeLower, Box, Surface.Gold, Tr, new Vector3(0, -0.4f, 0), new Vector3(0.49f * w, 0.02f, 0.026f));
            if (look.style == RigStyle.Castellan)
                for (int i = 0; i < 6; i++)
                    P(CapeLower, Box, Surface.Cloth, c * 0.9f, new Vector3(-0.2f * w + i * 0.08f * w, -0.43f - (i % 2) * 0.03f, 0), new Vector3(0.05f, 0.05f, 0.02f), new Vector3(0, 0, 45f));
            for (int s = -1; s <= 1; s += 2)
                P(chest, Sph, Surface.Gold, Tr, new Vector3(s * 0.16f * w, 0.27f, 0.09f), new Vector3(0.05f, 0.05f, 0.03f));
            P(chest, Box, Surface.Gold, Tr * 0.9f, new Vector3(0, 0.26f, 0.105f), new Vector3(0.3f * w, 0.01f, 0.01f));
        }

        // ------------------------------------------------------------------ Broń i tarcza

        public void SetWeapon(WeaponModel model, Color color)
        {
            if (weaponObject != null) Util.DestroyNow(weaponObject);
            if (offhandObject != null) Util.DestroyNow(offhandObject);
            Look.weapon = model;
            // Noże: po jednym ostrzu w każdej dłoni.
            if (BowStringUpper != null) { Util.DestroyNow(BowStringUpper.gameObject); Util.DestroyNow(BowStringLower.gameObject); BowStringUpper = BowStringLower = null; }
            if (NockedArrow != null) { Util.DestroyNow(NockedArrow.gameObject); NockedArrow = null; }
            if (quiverObject != null) { Util.DestroyNow(quiverObject); quiverObject = null; }
            weaponObject = GearBuilder.Weapon(WeaponSocket, model == WeaponModel.Knives ? WeaponModel.Dagger : model, color, bowString: model != WeaponModel.Bow);
            if (model == WeaponModel.Bow)
            {
                var stringColor = new Color(0.85f, 0.82f, 0.7f);
                BowStringUpper = PartBuilder.Add(VisualRoot, ProcMesh.Box(), Surface.Cloth, stringColor, Vector3.zero, Vector3.one).transform;
                BowStringLower = PartBuilder.Add(VisualRoot, ProcMesh.Box(), Surface.Cloth, stringColor, Vector3.zero, Vector3.one).transform;
                BowStringUpper.name = "BowStringUpper"; BowStringLower.name = "BowStringLower";
                foreach (var st in new[] { BowStringUpper, BowStringLower })
                    st.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                // Strzała: węzeł w (0,0,0) = nasadka na cięciwie, grot wzdłuż +Z.
                var arrow = new GameObject("NockedArrow").transform;
                arrow.SetParent(VisualRoot, false);
                PartBuilder.Add(arrow, ProcMesh.Box(), Surface.Wood, new Color(0.45f, 0.32f, 0.2f), new Vector3(0, 0, 0.36f), new Vector3(0.016f, 0.016f, 0.72f));
                PartBuilder.Add(arrow, ProcMesh.Cone(), Surface.Metal, new Color(0.75f, 0.75f, 0.78f), new Vector3(0, 0, 0.75f), new Vector3(0.03f, 0.07f, 0.03f), new Vector3(90f, 0, 0));
                for (int i = 0; i < 3; i++)
                    PartBuilder.Add(arrow, ProcMesh.Box(), Surface.Cloth, new Color(0.8f, 0.2f, 0.15f), new Vector3(0, 0, 0.06f), new Vector3(0.004f, 0.05f, 0.09f), new Vector3(0, 0, i * 60f));
                PartBuilder.BakeAll(arrow);
                arrow.gameObject.SetActive(false);
                NockedArrow = arrow;
                quiverObject = BuildQuiver();
            }
            if (model == WeaponModel.Knives && OffhandSocket != null) offhandObject = GearBuilder.Weapon(OffhandSocket, WeaponModel.Dagger, color);
            weaponTint.Clear();
            weaponTint.Collect(weaponObject.GetComponentsInChildren<Renderer>(true));
            if (offhandObject != null) weaponTint.Collect(offhandObject.GetComponentsInChildren<Renderer>(true));
            CollectBodyTint();
        }

        /// <summary>Kołczan na plecach (skos od prawego barku) z wystającymi strzałami.</summary>
        GameObject BuildQuiver()
        {
            var chest = this[Bone.Chest];
            var q = new GameObject("Quiver").transform;
            q.SetParent(chest, false);
            q.localPosition = new Vector3(0.1f, 0.14f, -0.17f);
            q.localRotation = Quaternion.Euler(0, 0, -22f);
            var leather = Look.leather;
            PartBuilder.Add(q, ProcMesh.Tube(0.85f), Surface.Leather, leather * 0.8f, Vector3.zero, new Vector3(0.1f, 0.42f, 0.1f));
            PartBuilder.Add(q, ProcMesh.Torus(0.25f), Surface.Leather, leather * 0.55f, new Vector3(0, 0.2f, 0), new Vector3(0.11f, 0.25f, 0.11f));
            PartBuilder.Add(q, ProcMesh.Torus(0.25f), Surface.Leather, leather * 0.55f, new Vector3(0, -0.12f, 0), new Vector3(0.11f, 0.25f, 0.11f));
            PartBuilder.Add(q, ProcMesh.Sphere(), Surface.Leather, leather * 0.6f, new Vector3(0, -0.21f, 0), new Vector3(0.1f, 0.04f, 0.1f));
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                var off = new Vector3(Mathf.Cos(a) * 0.025f, 0, Mathf.Sin(a) * 0.025f);
                PartBuilder.Add(q, ProcMesh.Box(), Surface.Wood, new Color(0.45f, 0.32f, 0.2f), off + new Vector3(0, 0.26f, 0), new Vector3(0.008f, 0.16f, 0.008f));
                PartBuilder.Add(q, ProcMesh.Box(), Surface.Cloth, new Color(0.8f, 0.2f, 0.15f), off + new Vector3(0, 0.32f, 0), new Vector3(0.004f, 0.06f, 0.035f), new Vector3(0, i * 30f, 0));
            }
            PartBuilder.BakeAll(q);
            return q.gameObject;
        }

        public void SetShield(ShieldModel model, Color color)
        {
            if (shieldObject != null) Util.DestroyNow(shieldObject);
            Look.shield = model;
            shieldObject = GearBuilder.Shield(ShieldSocket, model, color);
            CollectBodyTint();
        }

        public void ShowFlask(bool on) { if (flaskObject != null && flaskObject.activeSelf != on) flaskObject.SetActive(on); }

        // ------------------------------------------------------------------ Kolory (czytelność)

        /// <summary>Zabarwia postać (błysk parowania, trafienie).</summary>
        public void SetTint(Color c, float amount) => bodyTint.SetTint(c, amount);

        /// <summary>Poświata broni – telegraf ataku. Postać bez broni (pazury) świeci całym ciałem, delikatnie.</summary>
        public void SetWeaponGlow(Color c, float intensity)
        {
            if (Look.weapon == WeaponModel.Claws || Look.weapon == WeaponModel.None)
            {
                if (intensity > 0f) bodyTint.SetTint(c, Mathf.Clamp01(intensity) * 0.3f);
                return;
            }
            weaponTint.SetGlow(c, intensity);
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
                    case WeaponModel.Spear: len = 1.7f; break;
                    case WeaponModel.Scythe: len = 1.45f; break;
                    case WeaponModel.Hammer: len = 1.1f; break;
                    case WeaponModel.Mace: len = 0.72f; break;
                    case WeaponModel.Knives: len = 0.4f; break;
                    case WeaponModel.Bow: len = 0.65f; break;
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
