using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Dodatkowe zachowania pocisku (skille z Hell Unleashed): przebijanie kolejnych wrogów, rozszczep na drzazgi przy pierwszym
    /// trafieniu, wybuch zadający całe obrażenia (także na końcu lotu) i strefa po wybuchu.
    /// </summary>
    public struct ProjectileOptions
    {
        /// <summary>Ilu kolejnych wrogów pocisk przebija (0 = zatrzymuje się na pierwszym).</summary>
        public int pierce;
        /// <summary>Przy pierwszym trafieniu pęka na dwie drzazgi (±25°, 40% obrażeń, 1,5 s lotu).</summary>
        public bool split;
        /// <summary>Całe obrażenia w wybuchu o promieniu hit.explosionRadius – przy trafieniu albo na końcu lotu.</summary>
        public bool fullExplosion;
        /// <summary>Strefa po wybuchu: promień, czas, odstęp tyknięć i część obrażeń na tyknięcie.</summary>
        public float zoneRadius, zoneDuration, zoneInterval, zoneShare;
        /// <summary>Odrzut trafionego (m, ujemny = przyciąganie).</summary>
        public float knockback;
    }

    /// <summary>Pocisk: porusza się, trafia cel (lub ścianę) i znika – albo przebija kolejnych wrogów.</summary>
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
        Transform arrowModel;
        ProjectileOptions opts;
        int pierceLeft;
        float speedValue;

        /// <summary>Odczyt dla obserwatorów (boty testowe): prędkość, strona, właściciel i czy unik przed pociskiem chroni.</summary>
        public Vector3 Velocity => velocity;
        public Faction Side => faction;
        public IHitReceiver Owner => owner;
        public float HitRadius => radius;
        public bool Dodgeable => hit.dodgeable;
        public bool Blockable => hit.blockable;

        /// <summary>Smuga za lecącym obiektem (strzała, rzucona broń).</summary>
        public static TrailRenderer AddTrail(GameObject go, Color c, float width, float time)
        {
            var tr = go.AddComponent<TrailRenderer>();
            tr.material = FxMaterials.Additive;
            tr.time = time;
            tr.startWidth = width; tr.endWidth = 0f;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) }, new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            tr.colorGradient = g;
            tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            tr.minVertexDistance = 0.05f;
            return tr;
        }
        readonly HitTracker tracker = new HitTracker();

        public static Projectile Spawn(Vector3 pos, Vector3 dir, float speed, HitData hit, Faction faction, IHitReceiver owner,
                                       Color color, float radius, float lifetime, IHitReceiver homingTarget, float homing, bool arrow, ProjectileOptions options)
        {
            var p = Spawn(pos, dir, speed, hit, faction, owner, color, radius, lifetime, homingTarget, homing, arrow);
            if (p == null) return null;
            p.opts = options;
            p.pierceLeft = options.pierce;
            return p;
        }

        public static Projectile Spawn(Vector3 pos, Vector3 dir, float speed, HitData hit, Faction faction, IHitReceiver owner,
                                       Color color, float radius = 0.25f, float lifetime = 4f, IHitReceiver homingTarget = null, float homing = 0f, bool arrow = false)
        {
            if (!Application.isPlaying) return null;
            var go = new GameObject("Projectile");
            go.transform.position = pos;
            var p = go.AddComponent<Projectile>();
            if (owner is EnemyBrain enemy) enemy.TrackOwnedEffect(p);
            if (arrow)
            {
                // Strzała: drzewce, grot i lotki, zwrócona w kierunku lotu; cienka smuga zamiast świecącego jądra.
                p.arrowModel = new GameObject("Arrow").transform;
                p.arrowModel.SetParent(go.transform, false);
                var a = p.arrowModel;
                PartBuilder.Add(a, ProcMesh.Box(), Surface.Wood, new Color(0.45f, 0.32f, 0.2f), new Vector3(0, 0, -0.2f), new Vector3(0.018f, 0.018f, 0.7f));
                PartBuilder.Add(a, ProcMesh.Cone(), Surface.Metal, new Color(0.75f, 0.75f, 0.78f), new Vector3(0, 0, 0.17f), new Vector3(0.035f, 0.08f, 0.035f), new Vector3(90f, 0, 0));
                for (int i = 0; i < 3; i++)
                    PartBuilder.Add(a, ProcMesh.Box(), Surface.Cloth, new Color(0.8f, 0.2f, 0.15f), new Vector3(0, 0, -0.5f), new Vector3(0.004f, 0.06f, 0.1f), new Vector3(0, 0, i * 60f));
                PartBuilder.BakeAll(a);
                AddTrail(go, Color.Lerp(color, Color.white, 0.6f), 0.05f, 0.15f);
                a.rotation = Quaternion.LookRotation(dir);
                if (hit.element != Element.None)
                    p.fx = HellFx.Trail(go.transform, hit.visualElement ?? HellFx.For(hit.element), radius * 0.65f).gameObject;
            }
            else
            {
                p.fx = FxLibrary.ProjectileVisual(go.transform, color, radius, hit.element, faction == Faction.Enemy, hit.visualElement);
            }
            p.color = color;
            p.radiusVisual = radius;
            FxLibrary.Flash(pos, color, 0.5f);
            p.hit = hit; p.hit.isProjectile = true; p.faction = faction; p.owner = owner;
            p.velocity = dir.normalized * speed; p.radius = radius; p.life = lifetime; p.speedValue = speed;
            p.homingTarget = homingTarget; p.homing = homing;
            return p;
        }

        void Explode(bool impact)
        {
            FxLibrary.DetachProjectileVisual(fx);
            fx = null;
            if (opts.fullExplosion && hit.explosionRadius > 0f)
            {
                // Wybuch z całymi obrażeniami (przy trafieniu i na końcu lotu), potem ewentualna strefa.
                var boom = hit;
                boom.sourcePosition = transform.position;
                var t = new HitTracker();
                HitQuery.Sphere(transform.position, hit.explosionRadius, faction, t, r => { HitQuery.Apply(owner, r, boom, false); Push(r); });
                HellFx.Shockwave(transform.position, hit.visualElement ?? HellFx.For(hit.element), color, hit.explosionRadius);
                Sfx.Play(Sfx.ImpactFor(hit.element), transform.position);
                if (opts.zoneDuration > 0f && opts.zoneRadius > 0f)
                {
                    Vector3 ground = transform.position;
                    if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out var gh, 6f, ~0, QueryTriggerInteraction.Ignore)) ground = gh.point;
                    var zh = hit; zh.physical *= opts.zoneShare; zh.magic *= opts.zoneShare; zh.explosionRadius = 0; zh.leaveZoneDuration = 0; zh.poiseDamage *= 0.1f;
                    DamageZone.Spawn(ground, opts.zoneRadius, opts.zoneDuration, opts.zoneInterval > 0 ? opts.zoneInterval : 0.5f, zh, faction, owner, color);
                }
                Destroy(gameObject);
                return;
            }
            if (impact && hit.explosionRadius <= 0) HellFx.Impact(transform.position, hit.visualElement ?? (hit.element != Element.None ? HellFx.For(hit.element) : faction == Faction.Enemy ? HellFxElement.Enemy : HellFxElement.Physical),
                color, arrowModel != null ? 0.5f : Mathf.Clamp(radiusVisual * 4f, 0.7f, 1.6f));
            if (impact) Sfx.Play(arrowModel != null && hit.element == Element.None ? SoundId.ArrowHit : Sfx.ImpactFor(hit.element), transform.position,
                arrowModel != null ? 1f : 0.8f);
            if (impact && hit.explosionRadius > 0f)
            {
                // Wybuch: pozostali w promieniu dostają część obrażeń (trafiony bezpośrednio – nie drugi raz).
                var splash = hit;
                splash.physical *= SplashShare; splash.magic *= SplashShare;
                splash.sourcePosition = transform.position;
                HitQuery.Sphere(transform.position, hit.explosionRadius, faction, tracker, r => HitQuery.Apply(owner, r, splash, false));
                HellFx.Shockwave(transform.position, hit.visualElement ?? HellFx.For(hit.element), color, hit.explosionRadius);
                Sfx.Play(hit.element == Element.Fire ? SoundId.FireExplosion : SoundId.Shockwave, transform.position);
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

        /// <summary>Trafienie celu: obrażenia, odrzut i rozszczep. Zwraca true, gdy pocisk leci dalej (przebicie).</summary>
        bool Strike(IHitReceiver r)
        {
            if (opts.fullExplosion) return false; // całe obrażenia w wybuchu
            hit.sourcePosition = transform.position - velocity.normalized * 0.5f;
            HitQuery.Apply(owner, r, hit, false);
            Push(r);
            if (opts.split)
            {
                // Drzazgi lecą dalej pod kątem ±25°, każda za 40% obrażeń; trafionego już nie ranią.
                opts.split = false;
                foreach (float a in new[] { -25f, 25f })
                {
                    var h = hit; h.physical *= 0.4f; h.magic *= 0.4f;
                    var child = Spawn(transform.position, Quaternion.Euler(0, a, 0) * velocity, speedValue, h, faction, owner, color, radiusVisual * 0.8f, 1.5f, null, 0f, arrowModel != null);
                    if (child != null) child.tracker.TryRegister(r);
                }
            }
            if (pierceLeft > 0) { pierceLeft--; HellFx.Impact(transform.position, hit.visualElement ?? HellFx.For(hit.element), color, 0.5f); return true; }
            return false;
        }

        void Push(IHitReceiver r)
        {
            if (opts.knockback == 0f || !(r is EnemyBrain e) || e.IsDead) return;
            Vector3 d = velocity; d.y = 0;
            if (d.sqrMagnitude < 1e-4f) return;
            e.Pull(e.transform.position + d.normalized * (opts.knockback + Mathf.Sign(opts.knockback)), Mathf.Abs(opts.knockback), 0.2f);
        }

        static readonly RaycastHit[] Hits = new RaycastHit[16];
        static readonly Collider[] Overlaps = new Collider[8];

        void OnDestroy()
        {
            if (fx != null) FxLibrary.DetachProjectileVisual(fx);
        }

        void Update() => Step(Time.deltaTime);

        /// <summary>Jeden krok lotu. Przebicie przetwarza wszystkie trafienia odcinka, również w długiej klatce.</summary>
        public void Step(float dt)
        {
            if (dt <= 0f) return;
            life -= dt;
            if (life <= 0) { Explode(opts.fullExplosion); return; }

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
                if (!tracker.TryRegister(r)) continue; // przebity wcześniej – leci dalej
                if (Strike(r)) continue;
                Explode(true);
                return;
            }

            Vector3 step = velocity * dt;
            Vector3 start = transform.position;
            int n = Physics.SphereCastNonAlloc(transform.position, radius, step.normalized, Hits, step.magnitude, ~0, QueryTriggerInteraction.Ignore);
            // SphereCastNonAlloc nie sortuje wyników. Po przebiciu pierwszego celu wybieramy następny
            // z tego samego odcinka, zamiast przeskakiwać przez niego do końca klatki.
            for (int remaining = n; remaining > 0; remaining--)
            {
                float best = float.MaxValue; int bestIdx = -1;
                for (int i = 0; i < n; i++)
                {
                    if (Hits[i].collider == null) continue;
                    var receiver = Hits[i].collider.GetComponentInParent<IHitReceiver>();
                    if (receiver != null && (receiver.Faction == faction || receiver.IsDead || tracker.Contains(receiver))) continue;
                    if (Hits[i].distance < best) { best = Hits[i].distance; bestIdx = i; }
                }
                if (bestIdx < 0) break;
                var r = Hits[bestIdx].collider.GetComponentInParent<IHitReceiver>();
                transform.position = start + step.normalized * best;
                Hits[bestIdx] = default;
                if (r != null && tracker.TryRegister(r) && Strike(r))
                    continue;
                Explode(true);
                return;
            }
            transform.position = start + step;
            if (arrowModel != null && velocity.sqrMagnitude > 0.01f) arrowModel.rotation = Quaternion.LookRotation(velocity);
        }
    }
}
