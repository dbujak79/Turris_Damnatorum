using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Portal, z którego wychodzą wrogowie – stojący, czerwony owal jak „town portal” z Diablo: wirująca tarcza ognia,
    /// jasna obwódka, iskry wciągane do środka, czerwone światło i plama blasku na ziemi. Otwiera się (rozciąga od
    /// szczeliny), trwa, aż wróg wyjdzie, i zamyka się z błyskiem. Elita – złoty portal.
    /// </summary>
    public class DemonPortal : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<DemonPortal> Active = new System.Collections.Generic.List<DemonPortal>();

        const float OpenTime = 0.35f, CloseTime = 0.4f;

        Transform disc, swirl, core, rim;
        Light glow;
        float age, life, height, width;
        bool closing;
        float closeAge;
        Color color;
        HellParticles embers;

        /// <summary>Wysokość portalu (m).</summary>
        public float Height => height;

        /// <summary>
        /// Otwiera portal w <paramref name="ground"/> (stopa owalu), licem w stronę <paramref name="facing"/>.
        /// Zamyka się sam po <paramref name="lifetime"/> sekundach.
        /// </summary>
        public static DemonPortal Open(Vector3 ground, Vector3 facing, float height, float lifetime, bool elite)
        {
            if (!FxLibrary.Enabled) return null;
            var go = new GameObject("DemonPortal");
            facing.y = 0f;
            if (facing.sqrMagnitude < 1e-4f) facing = Vector3.forward;
            go.transform.SetPositionAndRotation(ground, Quaternion.LookRotation(facing.normalized));
            var p = go.AddComponent<DemonPortal>();
            p.Build(height, lifetime, elite);
            return p;
        }

        void Build(float h, float lifetime, bool elite)
        {
            height = h;
            width = h * 0.58f;
            life = lifetime;
            color = elite ? new Color(1f, 0.72f, 0.2f) : new Color(1f, 0.12f, 0.06f);
            Color hot = elite ? new Color(1f, 0.95f, 0.65f) : new Color(1f, 0.55f, 0.3f);
            Vector3 center = Vector3.up * (h * 0.5f + 0.05f);
            // Warstwy (od tyłu): ciemna tarcza, wir, jasny środek, obwódka. Addytywne – dwustronne.
            disc = Layer("Glow", FxMaterials.Additive, color * 0.9f, center, 1.35f);
            swirl = Layer("Swirl", FxMaterials.Portal, color, center, 1f);
            core = Layer("Core", FxMaterials.Additive, hot, center + Vector3.forward * 0.01f, 0.45f);
            rim = Layer("Rim", FxMaterials.Ring, hot, center + Vector3.forward * 0.02f, 1.08f);
            glow = FxLibrary.PointLight(transform.position + transform.rotation * center + transform.forward * 0.6f, color, 3.2f, 7f, 0f, transform);
            // Iskry krążące po obwodzie i wciągane do środka, żar u stóp.
            FxLibrary.LoopEmit(transform, center, new FxLibrary.Emit
            {
                sprite = "embers_atlas", atlas = new Vector2Int(4,4),
                rate = 90, life = new Vector2(0.4f, 0.8f), speed = new Vector2(0.1f, 0.3f), size = new Vector2(0.08f, 0.16f),
                colorA = hot, colorB = color, shape = ParticleSystemShapeType.Circle, radius = width * 0.5f, radial = -0.6f,
                sizeGrow = 0.3f, localSpace = true,
            });
            FxLibrary.LoopEmit(transform, Vector3.up * 0.05f, new FxLibrary.Emit
            {
                rate = 18, life = new Vector2(0.6f, 1.1f), speed = new Vector2(0.2f, 0.6f), size = new Vector2(0.3f, 0.6f),
                colorA = new Color(0.15f, 0.04f, 0.04f, 0.45f), alpha = true, shape = ParticleSystemShapeType.Sphere, radius = width * 0.4f,
                gravity = -0.1f, sizeGrow = 1.6f, noise = 0.4f,
            });
            var floor = FxLibrary.Decal(transform.position, FxMaterials.Additive, color * 0.8f, width * 1.6f, width * 2.2f, lifetime + CloseTime, 0f, 0.3f);
            embers = HellFx.Flame(transform, Vector3.up * 0.12f, height * 0.12f);
            if (floor != null) floor.transform.SetParent(transform, true);
            Sfx.Play(SoundId.CastCharge, transform.position + center, 1f, 0.5f);
            Apply(0f);
            Active.Add(this);
        }

        Transform Layer(string name, Material mat, Color c, Vector3 localPos, float size)
        {
            var oval = new GameObject(name);
            oval.transform.SetParent(transform, false);
            oval.transform.localPosition = localPos;
            oval.transform.localScale = new Vector3(width * size, height * size, 1f);
            var go = new GameObject("Quad");
            go.transform.SetParent(oval.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = QuadMesh;
            var r = go.AddComponent<MeshRenderer>();
            r.material = mat;
            if (r.material.HasProperty("_TintColor")) r.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, 0.9f));
            else r.material.color = c;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return oval.transform;
        }

        static Mesh quad;
        /// <summary>Pionowy kwadrat 1×1 (środek w zerze, lico wzdłuż +Z i −Z).</summary>
        static Mesh QuadMesh
        {
            get
            {
                if (quad != null) return quad;
                quad = new Mesh { name = "PortalQuad" };
                quad.vertices = new[] { new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0) };
                quad.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
                quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
                quad.RecalculateNormals();
                quad.RecalculateBounds();
                return quad;
            }
        }

        /// <summary>Zamknięcie teraz (np. wróg już wyszedł).</summary>
        public void Close()
        {
            if (closing) return;
            closing = true;
            closeAge = 0f;
            if (embers != null) { embers.StopAndRelease(); embers = null; }
            FxLibrary.Flash(transform.position + Vector3.up * height * 0.5f, color, 1.4f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            if (!closing && age >= life) Close();
            float open;
            if (closing)
            {
                closeAge += dt;
                open = 1f - Mathf.SmoothStep(0f, 1f, closeAge / CloseTime);
                if (closeAge >= CloseTime) { Destroy(gameObject); return; }
            }
            else open = Mathf.SmoothStep(0f, 1f, age / OpenTime);
            Apply(open);
        }

        /// <summary>Otwarcie od pionowej szczeliny (szerokość rośnie szybciej niż wysokość), wir się kręci, światło pulsuje.</summary>
        void Apply(float open)
        {
            float w = Mathf.Pow(open, 0.6f), h = Mathf.Lerp(0.35f, 1f, open);
            float pulse = 1f + 0.06f * Mathf.Sin(age * 9f);
            void Scale(Transform t, float s) { if (t != null) t.localScale = new Vector3(width * s * w * pulse, height * s * h, 1f); }
            Scale(disc, 1.35f);
            Scale(swirl, 1f);
            Scale(core, 0.45f);
            Scale(rim, 1.08f);
            // Wir: obrót wokół osi patrzenia (owal zostaje owalem – kręci się tekstura w przeskalowanej warstwie).
            if (swirl != null) swirl.GetChild(0).localRotation = Quaternion.AngleAxis(-age * 220f, Vector3.forward);
            if (core != null) core.GetChild(0).localRotation = Quaternion.AngleAxis(age * 140f, Vector3.forward);
            if (glow != null) glow.intensity = 3.2f * open * (0.85f + 0.15f * Mathf.Sin(age * 13f));
        }

        void OnDestroy() => Active.Remove(this);
    }
}
