using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Świetlna smuga za ostrzem w fazie aktywnej ataku (czytelność toru ciosu). Wstęga z historii
    /// położeń nasady i czubka broni; każdy segment gaśnie w czasie. Działa dla gracza i przeciwników.
    /// </summary>
    public class SwingTrail : MonoBehaviour
    {
        struct Sample { public Vector3 a, b; public float t; }

        public float sampleLife = 0.16f;
        public Color color = new Color(0.9f, 0.95f, 1f);

        CharacterVisual visual;
        ICharacterAnimSource source;
        readonly List<Sample> samples = new List<Sample>();
        Mesh mesh;
        MeshRenderer rend;
        GameObject holder;
        readonly List<Vector3> verts = new List<Vector3>();
        readonly List<Color> colors = new List<Color>();
        readonly List<Vector2> uvs = new List<Vector2>();
        readonly List<int> tris = new List<int>();

        public void Init(CharacterVisual v, ICharacterAnimSource src)
        {
            visual = v;
            source = src;
            if (holder != null) return;
            holder = new GameObject("SwingTrail");
            holder.transform.SetParent(null, false);
            mesh = new Mesh { name = "SwingTrail" };
            mesh.MarkDynamic();
            holder.AddComponent<MeshFilter>().sharedMesh = mesh;
            rend = holder.AddComponent<MeshRenderer>();
            rend.sharedMaterial = FxMaterials.Additive;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }

        void OnDestroy()
        {
            if (holder != null) Destroy(holder);
        }

        void LateUpdate()
        {
            if (!Application.isPlaying || visual == null || source == null || mesh == null) return;
            float now = Time.time;
            var st = source.GetAnimState();
            bool swinging = st.action == AnimAction.Attack && st.phase == ActionPhase.Active && visual.Rig != null
                            && visual.Rig.Look.weapon != WeaponModel.None && visual.Rig.Look.weapon != WeaponModel.Claws;
            if (swinging)
            {
                Vector3 tip = visual.WeaponTip;
                Vector3 socket = visual.Rig.WeaponSocket.position;
                Vector3 baseP = Vector3.Lerp(socket, tip, 0.25f);
                samples.Add(new Sample { a = baseP, b = tip, t = now });
            }
            samples.RemoveAll(s => now - s.t > sampleLife);
            BuildMesh(now);
        }

        void BuildMesh(float now)
        {
            verts.Clear(); colors.Clear(); uvs.Clear(); tris.Clear();
            if (samples.Count >= 2)
            {
                for (int i = 0; i < samples.Count; i++)
                {
                    var s = samples[i];
                    float k = Mathf.Clamp01(1f - (now - s.t) / sampleLife);
                    Color c = color * (k * k);
                    Color cBase = c * 0.15f;
                    verts.Add(s.a); colors.Add(cBase); uvs.Add(new Vector2(0.5f, 0.1f));
                    verts.Add(s.b); colors.Add(c); uvs.Add(new Vector2(0.5f, 0.5f));
                    if (i > 0)
                    {
                        int b = i * 2;
                        tris.AddRange(new[] { b - 2, b - 1, b, b, b - 1, b + 1 });
                        tris.AddRange(new[] { b - 2, b, b - 1, b, b + 1, b - 1 }); // dwustronnie
                    }
                }
            }
            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
        }
    }

    /// <summary>
    /// Efekty trafień w jednym miejscu: słucha zdarzeń walki i dobiera efekt do wyniku
    /// (krew, iskry bloku, gwiazda parowania, odłamki przełamania gardy, podmuch uniku).
    /// </summary>
    public class CombatFxDirector : MonoBehaviour
    {
        void OnEnable() => CombatEvents.HitResolved += OnHit;
        void OnDisable() => CombatEvents.HitResolved -= OnHit;

        void OnHit(Vector3 pos, HitResult r, bool targetIsPlayer)
        {
            var cam = Camera.main;
            Vector3 toCam = cam != null ? (cam.transform.position - pos).normalized : Vector3.up;
            switch (r.outcome)
            {
                case HitOutcome.Hit:
                    FxLibrary.Blood(pos, toCam + Vector3.up * 0.5f, Mathf.Clamp(r.healthDamage / 40f, 0.5f, 2f));
                    FxLibrary.Particles(pos, new FxLibrary.Emit { burst = 1, life = new Vector2(0.08f, 0.1f), size = new Vector2(0.6f, 0.7f), colorA = targetIsPlayer ? new Color(1f, 0.3f, 0.25f) : new Color(1f, 0.9f, 0.8f), sizeGrow = 1.5f });
                    break;
                case HitOutcome.Blocked:
                    FxLibrary.Sparks(pos, toCam, new Color(1f, 0.65f, 0.25f));
                    break;
                case HitOutcome.Parried:
                    FxLibrary.Parry(pos);
                    break;
                case HitOutcome.GuardBroken:
                    FxLibrary.GuardBreak(pos);
                    break;
                case HitOutcome.Dodged:
                    FxLibrary.Particles(pos, new FxLibrary.Emit { burst = 12, life = new Vector2(0.3f, 0.5f), speed = new Vector2(0.5f, 1.5f), size = new Vector2(0.15f, 0.3f), colorA = new Color(0.5f, 0.85f, 1f), noise = 0.5f, sizeGrow = 1.6f });
                    break;
            }
        }
    }
}
