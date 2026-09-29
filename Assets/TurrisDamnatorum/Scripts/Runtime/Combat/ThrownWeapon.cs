using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Ciężki rzut (etap F2 planu): broń leci po prostej do <c>distance</c> (albo do ściany), potem wraca do rzucającego.
    /// Rani każdego na drodze raz w każdą stronę. Model to ta sama broń co w dłoni, wirująca w locie.
    /// </summary>
    public class ThrownWeapon : MonoBehaviour
    {
        public static readonly List<ThrownWeapon> Active = new List<ThrownWeapon>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        PlayerCombat owner;
        Func<HitData> makeHit;
        Action onReturn;
        Vector3 dir, last;
        float speed, distance, travelled, radius, life = 4f;
        bool returning;
        Transform model;
        readonly HitTracker tracker = new HitTracker();

        public bool Returning => returning;

        public static ThrownWeapon Spawn(PlayerCombat owner, Vector3 origin, Vector3 dir, float distance, float speed, float radius,
                                         WeaponModel weaponModel, Color color, Func<HitData> makeHit, Action onReturn)
        {
            var go = new GameObject("ThrownWeapon");
            go.transform.position = origin;
            var t = go.AddComponent<ThrownWeapon>();
            t.owner = owner; t.makeHit = makeHit; t.onReturn = onReturn;
            t.dir = dir.normalized; t.distance = distance; t.speed = speed; t.radius = radius; t.last = origin;
            if (Application.isPlaying)
            {
                var pivot = new GameObject("Spin").transform;
                pivot.SetParent(go.transform, false);
                t.model = pivot;
                var w = GearBuilder.Weapon(pivot, weaponModel, color);
                w.transform.localPosition = new Vector3(0, 0, -0.45f); // obrót wokół środka broni, nie rękojeści
                go.transform.rotation = Quaternion.LookRotation(t.dir);
                // Smuga za lecącą bronią – czytelny tor lotu także z bliska.
                Projectile.AddTrail(go, Color.Lerp(color, Color.white, 0.5f), 0.35f, 0.3f);
                FxLibrary.Aura(go.transform, Vector3.zero, Color.Lerp(color, Color.white, 0.5f), 40f, 0.25f, 0.08f);
            }
            return t;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            life -= dt;
            if (owner == null) { Destroy(gameObject); return; }
            if (model != null) model.Rotate(Vector3.right, 1080f * dt, Space.Self);

            Vector3 pos = transform.position;
            Vector3 next;
            if (!returning)
            {
                float step = speed * dt;
                // Ściana zatrzymuje broń – od razu zawraca.
                if (Physics.Raycast(pos, dir, out var wall, step + radius, ~0, QueryTriggerInteraction.Ignore) && wall.collider.GetComponentInParent<IHitReceiver>() == null)
                {
                    step = Mathf.Max(0f, wall.distance - radius);
                    BeginReturn();
                }
                next = pos + dir * step;
                travelled += step;
                if (travelled >= distance || life < 2f) BeginReturn();
            }
            else
            {
                Vector3 target = owner.AimPoint;
                Vector3 to = target - pos;
                float step = speed * 1.2f * dt;
                if (to.magnitude <= Mathf.Max(0.6f, step) || life <= 0f)
                {
                    onReturn?.Invoke();
                    FxLibrary.Sparks(target, -to.normalized, new Color(0.9f, 0.85f, 0.7f));
                    Destroy(gameObject);
                    return;
                }
                next = pos + to.normalized * step;
            }
            HitQuery.Capsule(last, next, radius, Faction.Player, tracker, r => HitQuery.Apply(owner, r, makeHit(), true));
            last = next;
            transform.position = next;
        }

        void BeginReturn()
        {
            if (returning) return;
            returning = true;
            tracker.Reset(); // w drodze powrotnej może trafić tych samych wrogów jeszcze raz
        }
    }
}
