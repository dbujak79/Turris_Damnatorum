using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Aura (Hell Unleashed): krąg wokół bohatera, który trwa, dopóki gracz jej nie zgasi. Aura ofensywna co
    /// <c>interval</c> sekund rani wszystkich wrogów w promieniu; aura obronna tylko pokazuje krąg – jej premie
    /// liczy build bohatera (<see cref="PlayerCombat"/>).
    /// </summary>
    public class AuraEffect : MonoBehaviour
    {
        public static readonly List<AuraEffect> Active = new List<AuraEffect>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        public SpellDefinition Skill { get; private set; }
        public float Radius { get; private set; }

        IHitReceiver owner;
        Func<HitData> makeHit;
        float interval, timer;
        Color color;
        readonly HitTracker tracker = new HitTracker();
        HellParticles particles;
        FxDecal ring;

        public static AuraEffect Spawn(Transform follow, IHitReceiver owner, SpellDefinition skill, float radius, float interval, Func<HitData> makeHit, Color color)
        {
            var go = new GameObject("Aura_" + (skill != null ? skill.id : ""));
            go.transform.SetParent(follow, false);
            var a = go.AddComponent<AuraEffect>();
            a.Skill = skill; a.owner = owner; a.Radius = radius; a.interval = Mathf.Max(0.2f, interval); a.makeHit = makeHit; a.color = color;
            a.timer = a.interval;
            if (FxLibrary.Enabled)
            {
                a.particles = HellFx.Ambient(go.transform, Vector3.up * 0.15f, HellSkillFx.Family(skill), radius * 0.9f);
                a.ring = FxLibrary.Decal(follow.position, FxMaterials.Ring, color * 0.8f, radius * 2f, radius * 2f, 0f, 25f);
                if (a.ring != null) a.ring.Follow(follow);
            }
            return a;
        }

        void OnDestroy()
        {
            if (particles != null) particles.StopAndRelease();
            if (ring != null) Util.DestroySafe(ring.gameObject);
        }

        public void Stop()
        {
            if (particles != null) particles.StopAndRelease();
            Util.DestroySafe(gameObject);
        }

        void Update()
        {
            if (makeHit == null) return;
            timer -= Time.deltaTime;
            if (timer > 0) return;
            timer = interval;
            Pulse();
        }

        /// <summary>Jedno tyknięcie aury (publiczne dla testów).</summary>
        public void Pulse()
        {
            if (makeHit == null || owner == null || owner.IsDead) return;
            tracker.Reset();
            var hit = makeHit();
            hit.sourcePosition = transform.position;
            bool any = false;
            HitQuery.Sphere(transform.position + Vector3.up * 0.8f, Radius, Faction.Player, tracker, r => { HitQuery.Apply(owner, r, hit, false); any = true; });
            if (any && FxLibrary.Enabled) HellSkillFx.Shockwave(Skill, transform.position, Radius);
        }
    }
}
