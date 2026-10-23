using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Zapowiedziane uderzenie w punkt (np. Meteor): krąg na ziemi wypełnia się przez <c>delay</c>, potem wybuch w promieniu.
    /// Ten sam język zapowiedzi co ataki obszarowe wrogów – gracz, który zna krąg, wie, czego się spodziewać.
    /// </summary>
    public class DelayedStrike : MonoBehaviour
    {
        public static readonly List<DelayedStrike> Active = new List<DelayedStrike>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        HitData hit;
        Faction faction;
        IHitReceiver owner;
        float radius, delay, t;
        Color color;
        GameObject marker;
        DamageZone.Settings? afterZone;

        public static DelayedStrike Spawn(Vector3 center, float radius, float delay, HitData hit, Faction faction, IHitReceiver owner, Color color, DamageZone.Settings? afterZone = null)
        {
            var go = new GameObject("DelayedStrike");
            go.transform.position = center;
            var s = go.AddComponent<DelayedStrike>();
            if (owner is EnemyBrain enemy) enemy.TrackOwnedEffect(s);
            s.hit = hit; s.faction = faction; s.owner = owner; s.radius = radius; s.delay = Mathf.Max(0.05f, delay); s.color = color;
            s.afterZone = afterZone;
            s.marker = VisualFx.GroundMarker(center, radius, color, hit.visualElement ?? HellFx.For(hit.element));
            FxLibrary.Gather(go.transform, color, s.delay);
            Sfx.Play(SoundId.CastCharge, center, 0.8f, 0.6f);
            return s;
        }

        void Update() => Step(Time.deltaTime);
        public void Step(float dt)
        {
            if (dt <= 0) return;
            if (owner != null && owner.IsDead) { Destroy(gameObject); return; }
            t += dt;
            if (marker != null)
            {
                var decal = marker.GetComponent<FxDecal>();
                if (decal != null) decal.Progress = Mathf.Clamp01(t / delay);
            }
            if (t < delay) return;
            if (marker != null) Destroy(marker);
            var tracker = new HitTracker();
            hit.sourcePosition = transform.position;
            HitQuery.Sphere(transform.position + Vector3.up * 0.5f, radius, faction, tracker, r => HitQuery.Apply(owner, r, hit, false));
            HellFx.Shockwave(transform.position, hit.visualElement ?? HellFx.For(hit.element), color, radius);
            FxLibrary.Dust(transform.position, 2.5f);
            Sfx.Play(hit.element == Element.Fire ? SoundId.FireExplosion : SoundId.Quake, transform.position);
            Sfx.Play(SoundId.Quake, transform.position, 0.6f);
            if (afterZone.HasValue)
            {
                var z = afterZone.Value;
                DamageZone.Spawn(transform.position, z.radius, z.duration, z.interval, z.hit, faction, owner, color);
            }
            Destroy(gameObject);
        }

        void OnDestroy() { if (marker != null) Destroy(marker); }
    }

    /// <summary>
    /// Burza: przez czas trwania co <c>interval</c> piorun uderza w losowego wroga w promieniu wokół rzucającego.
    /// </summary>
    public class StormEffect : MonoBehaviour
    {
        public static readonly List<StormEffect> Active = new List<StormEffect>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        HitData hit;
        IHitReceiver owner;
        Transform center;
        float radius, interval, life, timer;
        Color color;

        public static StormEffect Spawn(Transform center, float radius, float duration, float interval, HitData hit, IHitReceiver owner, Color color)
        {
            var go = new GameObject("Storm");
            var s = go.AddComponent<StormEffect>();
            s.center = center; s.radius = radius; s.life = duration; s.interval = Mathf.Max(0.1f, interval);
            s.hit = hit; s.owner = owner; s.color = color;
            return s;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0) return;
            life -= dt;
            if (life <= 0 || center == null) { Destroy(gameObject); return; }
            timer -= dt;
            if (timer > 0) return;
            timer = interval;
            var candidates = new List<EnemyBrain>();
            foreach (var e in EnemyBrain.All)
                if (e != null && !e.IsDead && Vector3.Distance(e.transform.position, center.position) <= radius) candidates.Add(e);
            if (candidates.Count == 0) return;
            var target = candidates[Random.Range(0, candidates.Count)];
            FxLibrary.Lightning(target.transform.position + Vector3.up * 9f + Random.insideUnitSphere, target.AimPoint, color);
            Sfx.Play(SoundId.Thunder, target.AimPoint, 0.8f);
            hit.sourcePosition = target.transform.position + Vector3.up * 3f;
            HitQuery.Apply(owner, target, hit, false);
        }
    }
}
