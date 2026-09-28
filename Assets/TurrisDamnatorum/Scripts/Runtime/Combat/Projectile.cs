using UnityEngine;

namespace Turris
{
    /// <summary>Pocisk: porusza się, trafia jeden cel (lub ścianę) i znika.</summary>
    public class Projectile : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<Projectile> Active = new System.Collections.Generic.List<Projectile>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        HitData hit;
        Faction faction;
        IHitReceiver owner;
        Vector3 velocity;
        float radius;
        float life;
        IHitReceiver homingTarget;
        float homing;
        readonly HitTracker tracker = new HitTracker();

        public static Projectile Spawn(Vector3 pos, Vector3 dir, float speed, HitData hit, Faction faction, IHitReceiver owner,
                                       Color color, float radius = 0.25f, float lifetime = 4f, IHitReceiver homingTarget = null, float homing = 0f)
        {
            if (!Application.isPlaying) return null;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Projectile";
            Util.DestroySafe(go.GetComponent<Collider>());
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * radius * 2f;
            var rend = go.GetComponent<Renderer>();
            rend.material.color = color;
            if (rend.material.HasProperty("_EmissionColor"))
            {
                rend.material.EnableKeyword("_EMISSION");
                rend.material.SetColor("_EmissionColor", color * 2f);
            }
            var p = go.AddComponent<Projectile>();
            p.hit = hit; p.faction = faction; p.owner = owner;
            p.velocity = dir.normalized * speed; p.radius = radius; p.life = lifetime;
            p.homingTarget = homingTarget; p.homing = homing;
            return p;
        }

        static readonly RaycastHit[] Hits = new RaycastHit[16];
        static readonly Collider[] Overlaps = new Collider[8];

        void Update()
        {
            float dt = Time.deltaTime;
            life -= dt;
            if (life <= 0) { Destroy(gameObject); return; }

            if (homingTarget != null && !homingTarget.IsDead && homing > 0)
            {
                Vector3 desired = (homingTarget.AimPoint - transform.position).normalized * velocity.magnitude;
                velocity = Vector3.RotateTowards(velocity, desired, homing * Mathf.Deg2Rad * dt, 0f);
            }

            // Cel tuż przy wylocie (SphereCast nie wykrywa kolizji, w których startuje).
            int o = Physics.OverlapSphereNonAlloc(transform.position, radius, Overlaps, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < o; i++)
            {
                var r = Overlaps[i].GetComponentInParent<IHitReceiver>();
                if (r == null || r.Faction == faction || r.IsDead) continue;
                if (tracker.TryRegister(r))
                {
                    hit.sourcePosition = transform.position - velocity.normalized * 0.5f;
                    HitQuery.Apply(owner, r, hit, false);
                }
                Destroy(gameObject);
                return;
            }

            Vector3 step = velocity * dt;
            int n = Physics.SphereCastNonAlloc(transform.position, radius, step.normalized, Hits, step.magnitude, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue; int bestIdx = -1;
            for (int i = 0; i < n; i++)
            {
                var r = Hits[i].collider.GetComponentInParent<IHitReceiver>();
                if (r != null && (r.Faction == faction || r.IsDead)) continue;
                if (Hits[i].distance < best) { best = Hits[i].distance; bestIdx = i; }
            }
            if (bestIdx >= 0)
            {
                var r = Hits[bestIdx].collider.GetComponentInParent<IHitReceiver>();
                if (r != null && tracker.TryRegister(r))
                {
                    hit.sourcePosition = transform.position - velocity.normalized * 0.5f;
                    HitQuery.Apply(owner, r, hit, false);
                }
                Destroy(gameObject);
                return;
            }
            transform.position += step;
        }
    }
}
