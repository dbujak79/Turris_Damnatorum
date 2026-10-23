using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Fala ostrza (Hell Unleashed: Cięcie wichru): pas o podanej szerokości leci od bohatera w kierunku celowania do zasięgu
    /// i przenika przez wszystko – każdy wróg dostaje cios raz na przelot i zostaje odrzucony wzdłuż fali.
    /// </summary>
    public class WaveEffect : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<WaveEffect> Active = new System.Collections.Generic.List<WaveEffect>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);
        IHitReceiver owner;
        HitData hit;
        Vector3 origin, dir;
        float speed, range, halfWidth, knockback, travelled;
        Color color;
        readonly HitTracker tracker = new HitTracker();
        HellParticles fx;

        public static WaveEffect Spawn(IHitReceiver owner, Vector3 origin, Vector3 dir, float speed, float range, float width, float knockback, HitData hit, Color color)
        {
            dir.y = 0f;
            var go = new GameObject("Wave");
            go.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(dir.sqrMagnitude > 1e-4f ? dir : Vector3.forward));
            var w = go.AddComponent<WaveEffect>();
            if (owner is EnemyBrain enemy) enemy.TrackOwnedEffect(w);
            w.owner = owner; w.hit = hit; w.origin = origin; w.dir = go.transform.forward; w.speed = Mathf.Max(1f, speed); w.range = range;
            w.halfWidth = Mathf.Max(0.3f, width * 0.5f); w.knockback = knockback; w.color = color;
            if (FxLibrary.Enabled)
            {
                w.fx = HellFx.Wave(go.transform, hit.visualElement ?? HellFx.For(hit.element), w.halfWidth, range / w.speed + 0.1f);
                Projectile.AddTrail(go, color, w.halfWidth * 2f, 0.25f);
            }
            return w;
        }

        void Update() => Step(Time.deltaTime);

        /// <summary>Przesunięcie fali (publiczne na potrzeby testów).</summary>
        public void Step(float dt)
        {
            if (dt <= 0) return;
            if (owner != null && owner.IsDead) { Destroy(gameObject); return; }
            float from = travelled;
            travelled = Mathf.Min(range, travelled + speed * dt);
            Vector3 a = origin + dir * Mathf.Max(0f, from - 0.35f) + Vector3.up;
            Vector3 b = origin + dir * travelled + Vector3.up;
            transform.position = origin + dir * travelled;
            HitQuery.Capsule(a, b, halfWidth, owner != null ? owner.Faction : Faction.Player, tracker, r =>
            {
                var h = hit;
                h.sourcePosition = r.Transform.position - dir;
                HitQuery.Apply(owner, r, h, true);
                if (knockback > 0f && r is EnemyBrain e) e.Pull(e.transform.position + dir * (knockback + 1f), knockback, 0.2f);
            });
            if (travelled >= range)
            {
                if (fx != null) fx.StopAndRelease();
                Destroy(gameObject);
            }
        }

        void OnDestroy() { if (fx != null) fx.StopAndRelease(); }
    }
}
