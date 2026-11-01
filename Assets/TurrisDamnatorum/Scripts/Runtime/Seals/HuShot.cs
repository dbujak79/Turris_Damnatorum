using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Pocisk skilla HU (port <c>ProjectileNode</c> dla gracza): przebicie, rykoszet, rozprysk, uderzenie skilla przy trafieniu
    /// albo na końcu lotu, orbita wokół bohatera, naprowadzanie, powrót do rzucającego i przejmowanie żywiołu z pól żaru.
    /// Każdy pocisk w locie jest kotwicą dla Przekaźnika. Zasady trafienia: najpierw kontakt (część <c>1 − explosion_share</c>),
    /// potem uderzenie skilla – najwyżej jedno na trafienie, nigdy w łańcuchu.
    /// </summary>
    public sealed class HuShot : MonoBehaviour, IHuAnchor
    {
        /// <summary>Wygląd pocisku: kula żywiołu, strzała (łuk) albo wirujące ostrze (topór, żużel wojownika).</summary>
        public enum Look { Orb, Arrow, Blade }

        public struct Motion
        {
            public Transform orbitCenter; public float orbitRadius, orbitSpeed, orbitAngle;
            public float homingTurn;
            public Transform returnTo; public float returnSeconds;
            public bool Orbits => orbitCenter != null && orbitRadius > 0f;
        }

        const float RicochetRadius = 7f, ChildDamage = 0.4f, ChildAngle = 25f, ChildLife = 1.5f, FlightHeight = 1.0f;
        static readonly Collider[] Overlaps = new Collider[16];

        HuCast cast;
        HitData hit;
        float speed, radius, life, contactShare, rehit;
        int pierce, ricochet, splits, generation;
        bool impactOnHit, repeatImpact, impacted, returning, active;
        Motion motion;
        Vector3 velocity;
        readonly HashSet<IHitReceiver> struck = new HashSet<IHitReceiver>();
        readonly HashSet<int> infusedBy = new HashSet<int>();
        GameObject fx;
        Transform arrow, blade;

        public Vector3 AnchorPosition => transform.position;
        public bool AnchorLive => active && this != null;

        // ------------------------------------------------------------------ Moce unikatów
        public static readonly List<HuShot> Live = new List<HuShot>();
        /// <summary>Pocisk skończył lot: (pocisk, miejsce, czy kogoś trafił).</summary>
        public static event System.Action<HuShot, Vector3, bool> Ended;
        public HuCast SourceCast => cast;
        public HitData ShotHit => hit;
        public Vector3 Velocity => velocity;
        public bool HasStruck => struck.Count > 0;
        public bool IsActive => active;
        /// <summary>Dodatkowy budżet końcowego wybuchu (Miech pożeracza stosów) – wypłacany raz przy uderzeniu.</summary>
        public float ImpactBonus { get; set; }
        public void Redirect(Vector3 dir, float speedMult)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f || motion.Orbits) return;
            speed *= speedMult;
            velocity = dir.normalized * speed;
            motion.homingTurn = 0f;
        }
        public void ConvertShare(Element e, float share)
        {
            float moved = hit.Total * Mathf.Clamp01(share);
            float p = Mathf.Min(hit.physical, moved); hit.physical -= p;
            hit.magic += p; hit.element = e;
        }
        /// <summary>Zdejmuje pocisk bez trafienia i bez wybuchu (zderzenie, węzeł) – budżet rozlicza moc.</summary>
        public void Kill() => Expire(false);

        public static HuShot Spawn(HuCast cast, Vector3 pos, Vector3 dir, HitData hit, float speed, float radius, float lifetime,
            int pierce, int ricochet, int splits, float contactShare, bool impactOnHit, bool repeatImpact, Motion motion, Look look)
        {
            if (!Application.isPlaying) return null;
            var go = new GameObject("HuShot");
            go.transform.position = pos;
            var s = go.AddComponent<HuShot>();
            s.cast = cast; s.hit = hit; s.hit.isProjectile = true; s.speed = Mathf.Max(0.5f, speed); s.radius = Mathf.Max(0.12f, radius);
            s.life = Mathf.Max(0.05f, lifetime); s.pierce = pierce; s.ricochet = ricochet; s.splits = splits;
            s.contactShare = contactShare; s.impactOnHit = impactOnHit; s.repeatImpact = repeatImpact; s.motion = motion;
            dir.y = 0; s.velocity = (dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.forward) * s.speed;
            s.active = true;
            Live.Add(s);
            if (motion.Orbits) go.transform.position = s.OrbitPoint();
            s.BuildLook(look);
            HuWorld.RegisterAnchor(s);
            return s;
        }

        void BuildLook(Look look)
        {
            if (look == Look.Blade)
            {
                // Wirujący topór: ostrze i drzewce kręcą się w locie, z cienką smugą stali.
                blade = new GameObject("Blade").transform;
                blade.SetParent(transform, false);
                PartBuilder.Add(blade, ProcMesh.Box(), Surface.Wood, new Color(0.35f, 0.25f, 0.16f), Vector3.zero, new Vector3(0.05f, 0.05f, 0.75f));
                PartBuilder.Add(blade, ProcMesh.Blade(), Surface.Metal, new Color(0.78f, 0.78f, 0.82f), new Vector3(0.18f, 0, 0.3f), new Vector3(0.32f, 0.04f, 0.3f), new Vector3(0, 90f, 0));
                PartBuilder.BakeAll(blade);
                blade.localScale = Vector3.one * Mathf.Clamp(radius / 0.4f, 0.8f, 2.2f);
                Projectile.AddTrail(gameObject, Color.Lerp(cast.color, Color.white, 0.5f), 0.12f, 0.12f);
                if (hit.element != Element.None) fx = HellFx.Trail(transform, cast.family, radius * 0.65f)?.gameObject;
                return;
            }
            if (look == Look.Arrow)
            {
                arrow = new GameObject("Arrow").transform;
                arrow.SetParent(transform, false);
                PartBuilder.Add(arrow, ProcMesh.Box(), Surface.Wood, new Color(0.45f, 0.32f, 0.2f), new Vector3(0, 0, -0.2f), new Vector3(0.018f, 0.018f, 0.7f));
                PartBuilder.Add(arrow, ProcMesh.Cone(), Surface.Metal, new Color(0.75f, 0.75f, 0.78f), new Vector3(0, 0, 0.17f), new Vector3(0.035f, 0.08f, 0.035f), new Vector3(90f, 0, 0));
                PartBuilder.BakeAll(arrow);
                Projectile.AddTrail(gameObject, Color.Lerp(cast.color, Color.white, 0.6f), 0.05f, 0.15f);
                arrow.rotation = Quaternion.LookRotation(velocity);
                if (hit.element != Element.None) fx = HellFx.Trail(transform, cast.family, radius * 0.65f)?.gameObject;
            }
            else fx = FxLibrary.ProjectileVisual(transform, cast.color, Mathf.Min(radius, 0.6f), hit.element, false, cast.family);
            FxLibrary.Flash(transform.position, cast.color, 0.5f);
        }

        /// <summary>Dziecko rozprysku: część obrażeń rodzica, krótszy lot, bez dalszego pękania.</summary>
        void SpawnChildren(int count, Vector3 baseDir)
        {
            for (int i = 0; i < count; i++)
            {
                float step = (i / 2 + 1) * ChildAngle * (i % 2 == 0 ? 1f : -1f);
                var h = hit; h.physical *= ChildDamage; h.magic *= ChildDamage;
                var child = Spawn(cast, transform.position, Quaternion.Euler(0, step, 0) * baseDir, h, speed, radius * 0.8f, ChildLife,
                    0, 0, 0, contactShare, impactOnHit, repeatImpact, default, arrow != null ? Look.Arrow : blade != null ? Look.Blade : Look.Orb);
                if (child == null) continue;
                child.generation = generation + 1;
                foreach (var r in struck) child.struck.Add(r);
            }
        }

        Vector3 OrbitPoint()
        {
            Vector3 c = motion.orbitCenter.position;
            return new Vector3(c.x + Mathf.Cos(motion.orbitAngle) * motion.orbitRadius, c.y + FlightHeight, c.z + Mathf.Sin(motion.orbitAngle) * motion.orbitRadius);
        }

        void Update() => Step(Time.deltaTime);

        public void Step(float dt)
        {
            if (!active || dt <= 0f) return;
            if (cast.caster == null || cast.caster.IsDead) { Expire(false); return; }
            life -= dt;
            if (life <= 0f)
            {
                if (StartReturn()) return;
                Expire(!returning);
                return;
            }

            Steer(dt);
            Infuse();
            Vector3 from = transform.position, to = from + velocity * dt;
            if (motion.Orbits)
            {
                if (motion.orbitCenter == null) { Expire(true); return; }
                motion.orbitAngle += motion.orbitSpeed * dt;
                to = OrbitPoint();
                velocity = (to - from) / Mathf.Max(dt, 1e-4f);
                // Dookoła: każdy demon może dostać ponownie dwa razy na sekundę.
                rehit -= dt;
                if (rehit <= 0f) { rehit = 0.5f; struck.Clear(); }
            }
            if (returning && motion.returnTo != null && HuWorld.Flat(from, motion.returnTo.position) < 0.9f) { Expire(false); return; }

            if (Cast(from, to, out var target, out var contact))
            {
                transform.position = contact;
                HandleHit(target, contact);
                if (!active) return;
            }
            else transform.position = to;
            if (arrow != null && velocity.sqrMagnitude > 0.01f) arrow.rotation = Quaternion.LookRotation(velocity);
            if (blade != null) blade.Rotate(0f, 900f * dt, 0f, Space.World);
            if (!motion.Orbits && Blocked(from, to) && !StartReturn()) Expire(!returning);
        }

        /// <summary>Najbliższy nowy wróg na odcinku lotu.</summary>
        bool Cast(Vector3 from, Vector3 to, out IHitReceiver target, out Vector3 contact)
        {
            target = null; contact = to;
            Vector3 step = to - from;
            float len = step.magnitude;
            int n = Physics.OverlapCapsuleNonAlloc(from, to, radius, Overlaps, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                var r = Overlaps[i].GetComponentInParent<IHitReceiver>();
                if (r == null || r.Faction == Faction.Player || r.IsDead || struck.Contains(r)) continue;
                float along = len > 1e-4f ? Mathf.Clamp(Vector3.Dot(r.Transform.position - from, step / len), 0f, len) : 0f;
                if (along < best) { best = along; target = r; contact = from + (len > 1e-4f ? step / len * along : Vector3.zero); }
            }
            return target != null;
        }

        /// <summary>Ściana na drodze (nie wróg).</summary>
        bool Blocked(Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            if (d.sqrMagnitude < 1e-6f) return false;
            var hits = Physics.RaycastAll(from, d.normalized, d.magnitude + radius, ~0, QueryTriggerInteraction.Ignore);
            foreach (var h in hits) if (h.collider.GetComponentInParent<IHitReceiver>() == null && !h.collider.isTrigger) return true;
            return false;
        }

        /// <summary>Naprowadzanie skręca w stronę najbliższego demona przed pociskiem; wracający skręca do rzucającego.</summary>
        void Steer(float dt)
        {
            if (motion.Orbits) return;
            Vector3? goal = null;
            if (returning && motion.returnTo != null) goal = motion.returnTo.position;
            else if (motion.homingTurn > 0f)
            {
                var prey = HuWorld.AimTarget(transform.position, velocity, 8f, 75f);
                if (prey != null) goal = prey.transform.position;
            }
            if (!goal.HasValue) return;
            Vector3 want = HuWorld.FlatDir(transform.position, goal.Value);
            float turn = returning ? 12f : motion.homingTurn;
            Vector3 now = velocity; now.y = 0;
            float sp = now.magnitude;
            now = Vector3.RotateTowards(now.normalized, want, turn * dt, 0f);
            velocity = now * sp;
        }

        bool StartReturn()
        {
            if (returning || motion.returnTo == null || motion.returnSeconds <= 0f) return false;
            returning = true;
            struck.Clear();
            life += motion.returnSeconds;
            velocity = HuWorld.FlatDir(transform.position, motion.returnTo.position) * Mathf.Max(velocity.magnitude, 8f);
            return true;
        }

        /// <summary>Pocisk przelatujący przez pole żaru przejmuje jego obrażenia i efekty – raz na pole.</summary>
        void Infuse()
        {
            var field = HuWorld.InfusionAt(transform.position, infusedBy);
            if (field == null) return;
            infusedBy.Add(field.id);
            hit.physical += field.physical;
            hit.magic += field.magic;
            if (field.element != Element.None && (hit.element == Element.None || field.magic > hit.magic * 0.5f)) hit.element = field.element;
            if (field.statuses != null)
            {
                var list = hit.statuses != null ? new List<StatusApplication>(hit.statuses) : new List<StatusApplication>();
                list.AddRange(field.statuses);
                hit.statuses = list;
            }
            if (fx != null) FxLibrary.DetachProjectileVisual(fx);
            fx = FxLibrary.ProjectileVisual(transform, field.color, radius * 1.2f, field.element, false, HellFx.For(field.element));
            FxLibrary.Sparks(transform.position, velocity.normalized, field.color);
        }

        void HandleHit(IHitReceiver target, Vector3 contact)
        {
            struck.Add(target);
            if (contactShare > 0f)
            {
                var h = hit; h.physical *= contactShare; h.magic *= contactShare;
                h.sourcePosition = transform.position - velocity.normalized * 0.5f;
                HitQuery.Apply(cast.caster, target, h, false);
            }
            if (impactOnHit) Detonate(contact);
            else HellFx.Impact(contact, cast.family, cast.color, 0.5f);

            if (splits > 0 && generation < 1) { splits--; SpawnChildren(2, velocity.normalized); }
            if (pierce > 0) { pierce--; return; }
            if (ricochet > 0)
            {
                var bounce = HuWorld.Nearest(contact, RicochetRadius, e => !struck.Contains(e));
                if (bounce != null)
                {
                    ricochet--;
                    velocity = HuWorld.FlatDir(contact, bounce.transform.position) * velocity.magnitude;
                    return;
                }
            }
            if (motion.Orbits) return; // satelity krążą dalej
            if (StartReturn()) return;
            Expire(false);
        }

        /// <summary>Najwyżej jedno uderzenie skilla na trafienie (chyba że skill powtarza je przy każdym).</summary>
        void Detonate(Vector3 point)
        {
            if (impacted && !repeatImpact) return;
            impacted = true;
            cast.Impact(point);
            if (ImpactBonus > 0f && cast.caster != null)
            {
                // Pożarte pola: jeden dodatkowy budżet w tym samym wybuchu (raz).
                var h = cast.Hit(0f); h.magic = ImpactBonus; h.physical = 0f; h.element = Element.Fire; h.uniqueEffect = true; h.featureEffect = true;
                HuWorld.Explode(cast.caster, point, Mathf.Max(1.5f, cast.Area("explosion_radius", 2f)), h, cast.color);
                ImpactBonus = 0f;
            }
        }

        void Expire(bool explode)
        {
            if (!active) return;
            active = false;
            Live.Remove(this);
            Ended?.Invoke(this, transform.position, struck.Count > 0);
            if (explode) Detonate(transform.position);
            if (fx != null) { FxLibrary.DetachProjectileVisual(fx); fx = null; }
            Destroy(gameObject);
        }

        /// <summary>Przekaźnik odpala pocisk: wybucha tam, gdzie jest.</summary>
        public void Trigger() => Expire(true);

        void OnDestroy()
        {
            Live.Remove(this);
            active = false;
            if (fx != null) FxLibrary.DetachProjectileVisual(fx);
        }
    }
}
