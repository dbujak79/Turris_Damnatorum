using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Bastion ziemi (Mag ziemi A): krótka, prawdziwa ściana – widoczna bryła i kolizja tej samej wielkości. Blokuje ruch
    /// (CharacterController) i zwykłe pociski obu stron (pocisk rozbija się na kolizji bez odbiorcy). Wrogowie obchodzą ją
    /// najbliższym końcem albo burzą przy zetknięciu (bossy szybciej). Pęknięcie wzdłuż ściany wywołuje mocne trafienie
    /// gracza przy niej – raz; zniszczenie, wygaśnięcie i kolejny G nie duplikują końcowego efektu.
    /// </summary>
    public class EarthWall : MonoBehaviour
    {
        public static readonly List<EarthWall> Active = new List<EarthWall>();
        const int MaxWalls = 1;

        PlayerCombat owner;
        float life, durability, maxDurability;
        float length, thickness;
        bool ended, cracked;
        BoxCollider box;

        public float Durability => durability;
        public float Remaining => life;
        public bool Cracked => cracked;
        /// <summary>Obrażenia od wrogów (telemetria: obejścia/zniszczenia).</summary>
        public float DamageTaken { get; private set; }
        public Vector3 Axis => transform.right;
        public float Length => length;

        /// <summary>Sprawdza miejsce: granice areny, inni aktorzy i portale. Zwraca komunikat odmowy albo null.</summary>
        public static string Validate(Vector3 center, Quaternion rot, float length, float height, float thickness)
        {
            // Granice: oba końce ściany i jej środek muszą być wolne od geometrii areny (promień wzdłuż osi) i nad podłożem.
            Vector3 axis = rot * Vector3.right, mid = center + Vector3.up * 0.6f;
            foreach (var dir in new[] { axis, -axis })
                foreach (var h in Physics.RaycastAll(mid, dir, length * 0.5f + 0.3f, ~0, QueryTriggerInteraction.Ignore))
                    if (h.collider.GetComponentInParent<IHitReceiver>() == null && h.collider.GetComponentInParent<EarthWall>() == null) return "Brak miejsca: za blisko krawędzi areny";
            if (!Physics.Raycast(center + Vector3.up * 1.5f, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore)) return "Brak miejsca: brak podłoża";
            Vector3 half = new Vector3(length * 0.5f, height * 0.5f, thickness * 0.5f + 0.2f);
            var hits = Physics.OverlapBox(center + Vector3.up * (height * 0.5f + 0.15f), half, rot, ~0, QueryTriggerInteraction.Ignore);
            foreach (var c in hits)
            {
                if (c.GetComponentInParent<IHitReceiver>() != null) return "Brak miejsca: ktoś stoi na drodze ściany";
                if (c.GetComponentInParent<EarthWall>() != null) return "Brak miejsca: tu już stoi ściana";
            }
            if (Object.FindObjectsByType<DemonPortal>(FindObjectsInactive.Exclude).Length > 0)
                foreach (var p in Object.FindObjectsByType<DemonPortal>(FindObjectsInactive.Exclude))
                    if (HuWorld.Flat(p.transform.position, center) < length * 0.5f + 1.5f) return "Brak miejsca: nie wolno zamykać portalu";
            return null;
        }

        public static EarthWall Spawn(PlayerCombat owner, Vector3 center, Quaternion rot, ClassFeatureTuning t, float lengthOverride = 0f, float secondsOverride = 0f, int maxWalls = MaxWalls)
        {
            while (Active.Count >= Mathf.Max(1, maxWalls)) Active[0].End(false);
            float L = lengthOverride > 0f ? lengthOverride : t.wallLength;
            var go = new GameObject("EarthWall");
            go.transform.SetPositionAndRotation(HuWorld.Ground(center), rot);
            var w = go.AddComponent<EarthWall>();
            w.owner = owner; w.life = secondsOverride > 0f ? secondsOverride : t.wallSeconds; w.length = L; w.thickness = t.wallThickness;
            w.durability = w.maxDurability = Mathf.Max(50f, owner.Health.Max * t.wallDurability);
            w.box = go.AddComponent<BoxCollider>();
            w.box.size = new Vector3(L, t.wallHeight, t.wallThickness);
            w.box.center = Vector3.up * t.wallHeight * 0.5f;
            // Widoczna bryła = kolizja (te same wymiary), z kilku kamiennych bloków.
            int blocks = Mathf.Max(3, Mathf.RoundToInt(L / 0.75f));
            for (int i = 0; i < blocks; i++)
            {
                float x = Mathf.Lerp(-L * 0.5f, L * 0.5f, (i + 0.5f) / blocks);
                float h = t.wallHeight * (0.9f + 0.1f * Mathf.Sin(i * 1.7f));
                var part = PartBuilder.Add(go.transform, ProcMesh.Box(), Surface.Stone, new Color(0.42f, 0.36f, 0.3f),
                    new Vector3(x, h * 0.5f, 0f), new Vector3(L / blocks * 1.02f, h, t.wallThickness), new Vector3(0f, 0f, (i % 2 == 0 ? 2f : -2f)));
                var col = part.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
            if (Application.isPlaying)
            {
                FxLibrary.Dust(go.transform.position, L * 0.5f);
                HellFx.Shockwave(go.transform.position, HellFxElement.Earth, new Color(0.7f, 0.55f, 0.35f), L * 0.5f, true);
                Sfx.Play(SoundId.Quake, go.transform.position, 0.8f, 1.1f);
            }
            Active.Add(w);
            return w;
        }

        void Update() => Tick(Time.deltaTime);

        Transform follow; Vector3 followOffset;
        /// <summary>Buty ruchomego bastionu: płyta prowadzona z postacią (pozycja względem niej, z kolizją).</summary>
        public void Follow(Transform target, Vector3 localOffset) { follow = target; followOffset = localOffset; }

        public void Tick(float dt)
        {
            if (ended) return;
            if (follow != null)
            {
                Vector3 fwd = follow.forward; fwd.y = 0f;
                if (fwd.sqrMagnitude > 1e-4f)
                {
                    var rot = Quaternion.LookRotation(fwd.normalized);
                    // Płyty po bokach: oś ściany wzdłuż kierunku ruchu.
                    transform.SetPositionAndRotation(HuWorld.Ground(follow.position + rot * followOffset), rot * Quaternion.Euler(0f, 90f, 0f));
                }
            }
            life -= dt;
            if (life <= 0f || owner == null || owner.IsDead) End(false);
        }

        /// <summary>Wróg przy ścianie (w drodze do celu) bije w nią; bossy mocniej.</summary>
        public static void Touch(EnemyBrain e, float dt)
        {
            if (Active.Count == 0 || e == null) return;
            foreach (var w in Active)
            {
                if (w == null || w.ended) continue;
                float d = w.DistanceTo(e.transform.position);
                if (d > 0.9f * e.Def.scale + w.thickness * 0.5f) continue;
                float dps = Mathf.Max(8f, e.Def.attacks.Count > 0 ? e.Def.attacks[0].attack.baseDamage : 10f) * (e.Def.isBoss ? w.owner.FeatureTuning.wallBossDamage : 1f);
                w.Damage(dps * dt);
                break;
            }
        }

        /// <summary>Odległość punktu od odcinka ściany (płasko).</summary>
        public float DistanceTo(Vector3 p)
        {
            Vector3 local = transform.InverseTransformPoint(p);
            float x = Mathf.Clamp(local.x, -length * 0.5f, length * 0.5f);
            Vector2 d = new Vector2(local.x - x, local.z);
            return d.magnitude;
        }

        public void Damage(float amount)
        {
            if (ended || amount <= 0f) return;
            durability -= amount;
            DamageTaken += amount;
            if (durability <= 0f) End(true);
        }

        /// <summary>
        /// Omijanie: gdy ściana przecina drogę do celu, wróg idzie do bliższego końca (z marginesem). Bez ściany – kierunek bez zmian.
        /// </summary>
        public static Vector3 Steer(Vector3 from, Vector3 dir, Vector3 target)
        {
            if (Active.Count == 0) return dir;
            foreach (var w in Active)
            {
                if (w == null || w.ended) continue;
                if (!w.Blocks(from, target)) continue;
                Vector3 a = w.transform.position + w.Axis * (w.length * 0.5f + 0.9f);
                Vector3 b = w.transform.position - w.Axis * (w.length * 0.5f + 0.9f);
                Vector3 end = (HuWorld.Flat(from, a) + HuWorld.Flat(a, target)) < (HuWorld.Flat(from, b) + HuWorld.Flat(b, target)) ? a : b;
                Vector3 to = end - from; to.y = 0f;
                if (to.sqrMagnitude < 0.04f) return dir;
                return to.normalized;
            }
            return dir;
        }

        /// <summary>Czy odcinek od–do (płasko) przecina ścianę.</summary>
        public bool Blocks(Vector3 from, Vector3 to)
        {
            Vector3 lf = transform.InverseTransformPoint(from), lt = transform.InverseTransformPoint(to);
            if (Mathf.Sign(lf.z) == Mathf.Sign(lt.z)) return false; // po tej samej stronie
            float k = lf.z / (lf.z - lt.z);
            float x = Mathf.Lerp(lf.x, lt.x, k);
            return Mathf.Abs(x) <= length * 0.5f + 0.4f;
        }

        /// <summary>
        /// Pęknięcie: mocne trafienie gracza przy ścianie wysyła falę wzdłuż niej – jeden budżet z trafienia, raz na ścianę.
        /// Pęknięcie nie pobudza kolejnego (obrażenia pęknięcia mają featureEffect).
        /// </summary>
        public bool TryCrack(HitData source, Vector3 near)
        {
            if (ended || cracked || source.featureEffect || DistanceTo(near) > 2.2f) return false;
            cracked = true;
            float budget = source.Total * owner.FeatureTuning.wallCrackShare;
            var h = owner.FeatureHit("Pęknięcie bastionu", budget * 0.5f, budget * 0.5f, Element.Earth, transform.position);
            h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Stagger) };
            h.knockback = 1.5f;
            var tr = new HitTracker();
            Vector3 a = transform.position + Axis * length * 0.5f + Vector3.up * 0.6f, b = transform.position - Axis * length * 0.5f + Vector3.up * 0.6f;
            HitQuery.Capsule(a, b, 1.6f, Faction.Player, tr, r => HitQuery.Apply(owner, r, h, false));
            owner.FeatureTelemetry.Add("ziemia: pęknięcia");
            owner.FeatureTelemetry.Add("ziemia: obrażenia pęknięcia", budget);
            if (Application.isPlaying)
            {
                for (int i = 0; i < 4; i++) FxLibrary.Dust(Vector3.Lerp(a, b, i / 3f), 1.2f);
                HellFx.Shockwave(transform.position, HellFxElement.Earth, new Color(0.75f, 0.55f, 0.3f), length * 0.6f, true);
                Sfx.Play(SoundId.Shatter, transform.position, 0.9f, 0.8f);
            }
            End(false);
            return true;
        }

        public void End(bool destroyed)
        {
            if (ended) return;
            ended = true;
            Active.Remove(this);
            if (box != null) box.enabled = false; // bez niewidzialnej blokady po usunięciu
            if (owner != null) owner.FeatureTelemetry.Add(destroyed ? "ziemia: ściany zburzone" : "ziemia: ściany wygasłe");
            if (Application.isPlaying) FxLibrary.Dust(transform.position, length * 0.4f);
            Util.DestroySafe(gameObject);
        }

        void OnDestroy() { Active.Remove(this); }

        public static void ClearAll() { foreach (var w in Active.ToArray()) if (w != null) w.End(false); Active.Clear(); }
    }
}
