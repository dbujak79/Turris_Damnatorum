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

        public float Radius => radius;

        /// <summary>Parametry strefy tworzonej po czymś innym (np. po wybuchu meteoru).</summary>
        public struct Settings { public float radius, duration, interval; public HitData hit; }

        public static DamageZone Spawn(Vector3 center, float radius, float duration, float interval, HitData hit, Faction faction, IHitReceiver owner, Color color)
        {
            var go = new GameObject("DamageZone");
            go.transform.position = center;
            var z = go.AddComponent<DamageZone>();
            z.hit = hit; z.faction = faction; z.owner = owner;
            z.radius = radius; z.interval = Mathf.Max(0.1f, interval); z.life = duration;
            FxLibrary.BurningGround(center, radius, color, duration);
            return z;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            life -= dt;
            if (life <= 0) { Destroy(gameObject); return; }
            timer -= dt;
            if (timer > 0) return;
            timer = interval;
            tracker.Reset();
            hit.sourcePosition = transform.position;
            HitQuery.Sphere(transform.position + Vector3.up * 0.6f, radius, faction, tracker, r => HitQuery.Apply(owner, r, hit, false));
        }
    }
}
