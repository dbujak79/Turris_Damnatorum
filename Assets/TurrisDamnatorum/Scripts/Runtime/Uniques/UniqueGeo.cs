using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Wspólna geometria mocy unikatów: przeszkody, stożek, odcinek, wrogowie w kręgu.</summary>
    public static class UniqueGeo
    {
        /// <summary>Ściana/przeszkoda między punktami (ciała postaci się nie liczą).</summary>
        public static bool Blocked(Vector3 from, Vector3 to)
        {
            from.y += 1f; to.y += 1f;
            Vector3 d = to - from;
            float len = d.magnitude;
            if (len < 0.05f) return false;
            var hits = Physics.RaycastAll(from, d / len, len, ~0, QueryTriggerInteraction.Ignore);
            foreach (var h in hits)
            {
                if (h.collider == null) continue;
                if (h.collider.GetComponentInParent<EnemyBrain>() != null || h.collider.GetComponentInParent<PlayerCombat>() != null) continue;
                if (h.collider.GetComponentInParent<Servant>() != null) continue;
                if (h.collider.GetComponent<CharacterController>() != null) continue;
                return true;
            }
            return false;
        }

        public static bool InCone(Vector3 origin, Vector3 fwd, Vector3 point, float range, float arc)
        {
            Vector3 to = point - origin; to.y = 0f;
            if (to.magnitude > range) return false;
            return to.sqrMagnitude < 0.04f || Vector3.Angle(fwd, to) <= arc * 0.5f;
        }

        /// <summary>Odległość punktu od odcinka (płasko).</summary>
        public static float SegmentDistance(Vector3 a, Vector3 b, Vector3 p)
        {
            a.y = b.y = p.y = 0f;
            Vector3 ab = b - a;
            float t = ab.sqrMagnitude < 1e-6f ? 0f : Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector3.Distance(a + ab * t, p);
        }

        public static List<EnemyBrain> EnemiesIn(Vector3 center, float radius, Func<EnemyBrain, bool> accept = null)
        {
            var list = new List<EnemyBrain>();
            foreach (var e in EnemyBrain.All)
                if (e != null && !e.IsDead && HuWorld.Flat(e.transform.position, center) <= radius && (accept == null || accept(e))) list.Add(e);
            return list;
        }

        public static List<EnemyBrain> EnemiesOnSegment(Vector3 a, Vector3 b, float halfWidth)
        {
            var list = new List<EnemyBrain>();
            foreach (var e in EnemyBrain.All)
                if (e != null && !e.IsDead && SegmentDistance(a, b, e.transform.position) <= halfWidth + 0.4f * e.Def.scale) list.Add(e);
            return list;
        }
    }

    /// <summary>Prosty runtime mocy w świecie HU (tyka czasem gry, znika z końcem piętra i śmiercią).</summary>
    public sealed class UniqueRuntime : IHuRuntime
    {
        readonly Func<float, bool> tick; readonly Action stop;
        bool done;
        public UniqueRuntime(Func<float, bool> tick, Action stop = null) { this.tick = tick; this.stop = stop; }
        public bool Tick(float dt) { if (done) return false; bool alive = tick(dt); if (!alive) done = true; return alive; }
        public void Stop() { if (!done) { done = true; } stop?.Invoke(); }
        public bool Done => done;
        public void Kill() { done = true; }
    }
}

namespace Turris
{
    /// <summary>Źródło animacji bytu mocy (widmo, figura) – stała postawa bojowa.</summary>
    public sealed class UniqueAnimSource : UnityEngine.MonoBehaviour, ICharacterAnimSource
    {
        public CharacterAnimState GetAnimState() => new CharacterAnimState();
    }
}
