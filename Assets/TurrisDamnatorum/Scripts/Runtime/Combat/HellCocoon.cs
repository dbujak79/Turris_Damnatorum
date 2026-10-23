using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Kokon tkaczki: pojedyncze trafienie, po uniku leci dalej; na końcu zasięgu zostawia pajęczynę bez obrażeń.</summary>
    public class HellCocoon : MonoBehaviour
    {
        EnemyBrain owner;
        PlayerCombat target;
        HitData hit;
        Vector3 direction;
        float speed, range, root, webRadius, webSeconds;
        bool passed, finished;
        GameObject visual;
        public static HellCocoon Spawn(EnemyBrain owner, PlayerCombat target, HitData hit, float speed, float range, float root, float webRadius, float webSeconds)
        {
            var c = new GameObject("HU_Cocoon").AddComponent<HellCocoon>();
            c.owner = owner; c.target = target; c.hit = hit; c.hit.isProjectile = true;
            c.speed = speed; c.range = range; c.root = root; c.webRadius = webRadius; c.webSeconds = webSeconds;
            c.direction = Vector3.ProjectOnPlane(target.transform.position - owner.transform.position, Vector3.up).normalized;
            c.transform.position = owner.transform.position + Vector3.up * 0.9f + c.direction * 0.7f;
            c.visual = FxLibrary.ProjectileVisual(c.transform, new Color(0.85f, 0.88f, 0.7f), 0.3f, Element.Earth, true);
            owner.TrackOwnedEffect(c);
            return c;
        }
        void Update() => Step(Time.deltaTime);
        public void Step(float dt)
        {
            if (dt <= 0 || finished) return;
            if (owner == null || owner.IsDead || target == null || target.IsDead) { Stop(); return; }
            float distance = Mathf.Min(speed * dt, range);
            Vector3 start = transform.position, end = start + direction * distance;
            // Odcinek, nie tylko pozycja końcowa: kokon nie przeskoczy celu przy niskim FPS.
            var delta = Vector3.ProjectOnPlane(target.transform.position - start, Vector3.up);
            float along = Mathf.Clamp(Vector3.Dot(delta, direction), 0, distance);
            if (!passed && (delta - direction * along).sqrMagnitude <= 0.8f * 0.8f)
            {
                transform.position = start + direction * along;
                hit.sourcePosition = transform.position - direction * 0.5f;
                var result = HitQuery.Apply(owner, target, hit, false);
                passed = true;
                if (result.outcome != HitOutcome.Dodged)
                {
                    if (result.outcome == HitOutcome.Hit && target.Status.TryRoot(root) > 0)
                    {
                        CombatEvents.RaiseWorldText(target.AimPoint, "UNIERUCHOMIONY", new Color(0.85f, 0.88f, 0.7f));
                        HellRootShell.Show(target);
                    }
                    Stop(); return;
                }
            }
            transform.position = end; range -= distance;
            if (range <= 0)
            {
                HellWebPatch.Spawn(owner, target, new Vector3(end.x, 0.03f, end.z), webRadius, webSeconds);
                Stop();
            }
        }
        void Stop() { finished = true; Destroy(gameObject); }
        void OnDestroy() { if (visual != null) FxLibrary.DetachProjectileVisual(visual); }
    }

    public class HellWebPatch : MonoBehaviour
    {
        public static readonly List<HellWebPatch> Active = new List<HellWebPatch>();
        EnemyBrain owner;
        PlayerCombat target;
        float radius, remaining;
        bool finished;
        public static HellWebPatch Spawn(EnemyBrain owner, PlayerCombat target, Vector3 at, float radius, float seconds)
        {
            if (radius <= 0 || seconds <= 0 || Active.Count >= 32) return null;
            var p = new GameObject("HU_WebPatch").AddComponent<HellWebPatch>();
            p.owner = owner; p.target = target; p.transform.position = at; p.radius = radius; p.remaining = seconds;
            Active.Add(p); owner.TrackOwnedEffect(p);
            var marker = VisualFx.GroundMarker(at, radius, new Color(0.85f, 0.88f, 0.7f, 0.35f));
            if (marker != null) marker.transform.SetParent(p.transform, true);
            return p;
        }
        void OnDestroy() => Active.Remove(this);
        void Update() => Step(Time.deltaTime);
        public void Step(float dt)
        {
            if (dt <= 0 || finished) return;
            if (owner == null || owner.IsDead || target == null || target.IsDead || remaining <= 0) { finished = true; Destroy(gameObject); return; }
            if (Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up).sqrMagnitude <= radius * radius) target.Status.TouchWeb();
            remaining -= dt;
            if (remaining <= 0) { finished = true; Destroy(gameObject); }
        }
    }

    public class HellRootShell : MonoBehaviour
    {
        PlayerCombat target;
        public static void Show(PlayerCombat target)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "HU_CocoonShell";
            go.GetComponent<Collider>().enabled = false; Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(target.transform, false); go.transform.localPosition = Vector3.up * 0.9f;
            go.transform.localScale = new Vector3(1.1f, 0.95f, 1.1f);
            var renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = FxMaterials.Additive;
            var block = new MaterialPropertyBlock();
            var webColor = new Color(0.86f, 0.88f, 0.8f, 0.25f); // barwa WebLook HU; mniejsza alfa dla addytywnego materiału Unity
            block.SetColor("_TintColor", webColor); block.SetColor("_Color", webColor); renderer.SetPropertyBlock(block);
            go.AddComponent<HellRootShell>().target = target;
        }
        void Update() { if (target == null || target.IsDead || !target.Status.Rooted) Destroy(gameObject); }
    }
}
