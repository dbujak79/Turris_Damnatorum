using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Strefa na ziemi (np. płonąca ziemia): co <c>interval</c> sekund trafia każdego wroga w promieniu.
    /// Trafienia liczone zegarem, nie klatkami. Efekty (podpalenie) pochodzą z danych trafienia.
    /// </summary>
    public class DamageZone : MonoBehaviour
    {
        public static readonly List<DamageZone> Active = new List<DamageZone>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        HitData hit;
        Faction faction;
        IHitReceiver owner;
        float radius, interval, life, timer;
        readonly HitTracker tracker = new HitTracker();
        GameObject visual;

        public float Radius => radius;
        public Faction Faction => faction;
        public IHitReceiver Owner => owner;
        /// <summary>Prąd głębin: strefę wolno przesuwać (tylko strefy z Spawn – nie aury, totemy, miny, ściany ani stanowiska).</summary>
        public bool Relocatable { get; private set; } = true;
        /// <summary>Ile razy strefa została przesunięta (Prąd głębin – telemetria; czas i tyknięcia bez zmian).</summary>
        public int Moves { get; private set; }
        public int Ticks { get; private set; }
        Vector3 glideFrom, glideTo; float glideTime, glideLeft;

        /// <summary>
        /// Przesuwa strefę do punktu w podanym czasie, zachowując pozostały czas, rytm tyknięć, pochodzenie i statusy;
        /// bez ponownego wybuchu początkowego.
        /// </summary>
        public void GlideTo(Vector3 to, float seconds)
        {
            glideFrom = transform.position; glideTo = to; glideTime = Mathf.Max(0.05f, seconds); glideLeft = glideTime;
            Moves++;
        }
        /// <summary>Sekundy do zgaśnięcia strefy.</summary>
        public float Remaining => life;

        /// <summary>Parametry strefy tworzonej po czymś innym (np. po wybuchu meteoru).</summary>
        public struct Settings { public float radius, duration, interval; public HitData hit; }

        public static DamageZone Spawn(Vector3 center, float radius, float duration, float interval, HitData hit, Faction faction, IHitReceiver owner, Color color)
        {
            var go = new GameObject("DamageZone");
            go.transform.position = center;
            var z = go.AddComponent<DamageZone>();
            if (owner is EnemyBrain enemy) enemy.TrackOwnedEffect(z);
            z.hit = hit; z.hit.isGroundHazard = true; z.faction = faction; z.owner = owner;
            z.radius = radius; z.interval = Mathf.Max(0.1f, interval); z.life = duration;
            z.visual = FxLibrary.ElementGround(center, radius, hit.element, color, duration, hit.visualElement);
            Sfx.Play(Sfx.ForElement(hit.element, SoundId.Chains), center, 0.8f, 0.8f);
            return z;
        }

        void Update() => Step(Time.deltaTime);
        public void Step(float dt)
        {
            if (dt <= 0) return;
            if (owner != null && owner.IsDead) { Destroy(gameObject); return; }
            if (glideLeft > 0f)
            {
                glideLeft = Mathf.Max(0f, glideLeft - dt);
                transform.position = Vector3.Lerp(glideFrom, glideTo, 1f - glideLeft / glideTime);
                if (visual != null) visual.transform.position = transform.position;
            }
            float advance = Mathf.Min(dt, life);
            float next = timer;
            while (next <= advance && next < life)
            {
                tracker.Reset(); hit.sourcePosition = transform.position; Ticks++;
                HitQuery.Sphere(transform.position + Vector3.up * 0.6f, radius, faction, tracker, r => HitQuery.Apply(owner, r, hit, false));
                next += interval;
            }
            timer = next - advance;
            life -= dt;
            if (life <= 0) Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (visual == null) return;
            var particles = visual.GetComponentInChildren<HellParticles>();
            if (particles != null) particles.StopAndRelease();
            Destroy(visual);
        }
    }
}
