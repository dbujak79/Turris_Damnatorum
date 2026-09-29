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
        GameObject fx;
        Color color;
        float radiusVisual;
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
            var go = new GameObject("Projectile");
            go.transform.position = pos;
            // Jądro pocisku + oprawa (halo, smuga, krążące iskry, światło).
            var core = PartBuilder.Add(go.transform, ProcMesh.Sphere(), Surface.Glow, Color.Lerp(color, Color.white, 0.5f), Vector3.zero, Vector3.one * radius * 1.1f);
            core.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var p = go.AddComponent<Projectile>();
            p.fx = FxLibrary.ProjectileVisual(go.transform, color, radius);
            p.color = color;
            p.radiusVisual = radius;
            FxLibrary.Flash(pos, color, 0.5f);
            p.hit = hit; p.faction = faction; p.owner = owner;
            p.velocity = dir.normalized * speed; p.radius = radius; p.life = lifetime;
            p.homingTarget = homingTarget; p.homing = homing;
            return p;
        }

        void Explode(bool impact)
        {
            FxLibrary.DetachProjectileVisual(fx);
            fx = null;
            if (impact) FxLibrary.Impact(transform.position, color, Mathf.Clamp(radiusVisual * 4f, 0.7f, 1.6f));
            if (impact && hit.explosionRadius > 0f)
            {
                // Wybuch: pozostali w promieniu dostają część obrażeń (trafiony bezpośrednio – nie drugi raz).
                var splash = hit;
                splash.physical *= SplashShare; splash.magic *= SplashShare;
                splash.sourcePosition = transform.position;
                HitQuery.Sphere(transform.position, hit.explosionRadius, faction, tracker, r => HitQuery.Apply(owner, r, splash, false));
                FxLibrary.Shockwave(transform.position, color, hit.explosionRadius, false);
                FxLibrary.Impact(transform.position, color, 1.6f);
                if (hit.leaveZoneDuration > 0f)
                {
                    // Płonąca ziemia pod wybuchem: 15% obrażeń na tyknięcie, bez kolejnego wybuchu.
                    Vector3 ground = transform.position;
                    if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out var gh, 6f, ~0, QueryTriggerInteraction.Ignore)) ground = gh.point;
                    var zh = hit; zh.physical *= 0.15f; zh.magic *= 0.15f; zh.explosionRadius = 0; zh.leaveZoneDuration = 0; zh.poiseDamage *= 0.1f;
                    DamageZone.Spawn(ground, hit.explosionRadius * 0.7f, hit.leaveZoneDuration, 0.5f, zh, faction, owner, color);
                }
            }
            Destroy(gameObject);
        }

        /// <summary>Część obrażeń pocisku, jaką dostają cele w promieniu wybuchu.</summary>
        public const float SplashShare = 0.6f;

        static readonly RaycastHit[] Hits = new RaycastHit[16];
        static readonly Collider[] Overlaps = new Collider[8];

        void Update()
        {
            float dt = Time.deltaTime;
            life -= dt;
            if (life <= 0) { Explode(false); return; }

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
                Explode(true);
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
                transform.position += step.normalized * best;
                Explode(true);
                return;
            }
            transform.position += step;
        }
    }
}
