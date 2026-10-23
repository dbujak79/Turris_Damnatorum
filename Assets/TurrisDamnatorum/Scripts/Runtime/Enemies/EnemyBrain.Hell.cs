using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    public partial class EnemyBrain
    {
        readonly List<GameObject> hellOwned = new List<GameObject>();
        readonly Queue<int> hellPhaseEvents = new Queue<int>();
        HellEnemyChannel hellChannel;
        Vector3 hellAim;
        int hellPhaseSeen;
        int hellDirectHits;
        int hellRaisedTotal;
        int HellRaisedAlive()
        {
            int count = 0;
            foreach (var other in All) if (other != null && !other.IsDead && other.Owner == this && other.Def == Def.huSummon) count++;
            return count;
        }
        bool HellSummonerSpent => Def.huArchetype == 7 && hellRaisedTotal >= Def.attacks[0].summonMax * 3;
        float hellAge, hellFrenzy, hellWard, hellWardLife, hellWardDaze, hellStoneTime;
        bool hellHatched, hellPetrified;
        HitData hellWardBurst;
        float hellWardRadius;
        public float WardRemaining => hellWard;
        public float FrenzyRemaining => hellFrenzy;
        public bool HasLiveTarget => player != null && !player.IsDead;
        bool HellEnraged => Def.huEnrageHealth > 0 && Health != null && Health.Fraction <= Def.huEnrageHealth;
        float HellMaxTempo()
        {
            if (current?.huAbility == null || string.IsNullOrEmpty(current.huAbility.name)) return float.PositiveInfinity;
            var ability = current.huAbility;
            float minimum = current.attack.dodgeable ? 0.45f : 0.6f;
            if (ability.name == "ComboStrike")
                minimum = (ability.argument == "slash" || ability.argument == "thrust" ? 0.38f : ability.argument == "sweep" ? 0.42f : ability.argument == "leap" ? 0.55f : 0.5f) + 0.15f;
            return current.attack.windup / minimum;
        }

        T Own<T>(T effect) where T : Component { TrackOwnedEffect(effect); return effect; }
        public void TrackOwnedEffect(Component effect) { if (effect != null && !hellOwned.Contains(effect.gameObject)) hellOwned.Add(effect.gameObject); }
        void ClearHell()
        {
            if (hellChannel != null) hellChannel.Cancel();
            hellChannel = null;
            foreach (var effect in hellOwned) if (effect != null) Destroy(effect);
            hellOwned.Clear(); hellWard = hellWardLife = 0;
        }
        void InterruptHell() { if (hellChannel != null) hellChannel.Cancel(); hellChannel = null; hellStoneTime = 0; }

        EnemyBrain RiteAlly(float range)
        {
            int frenzied = 0;
            foreach (var other in All) if (other != null && !other.IsDead && other.hellFrenzy > 0) frenzied++;
            if (frenzied >= 3) return null;
            EnemyBrain best = null; float worth = 0;
            foreach (var other in All)
            {
                if (other == null || other == this || other.IsDead || other.Def.isBoss || other.hellFrenzy > 0 || (other.Def.huArchetype == 11 && other.CurrentState == State.Dormant) || other.hellStoneTime > 0
                    || other.Def.huArchetype == 10 || other.Def.huArchetype == 13 || FlatDistance(other.transform.position) > range) continue;
                int type = other.Def.huArchetype;
                float score = other.Health.Max * (type == 1 || type == 6 || type == 7 ? 0.2f : 1);
                if (score > worth) { best = other; worth = score; }
            }
            return best;
        }

        void TickHell(float dt)
        {
            hellAge += dt; hellFrenzy = Mathf.Max(0, hellFrenzy - dt);
            hellStoneTime = Mathf.Max(0, hellStoneTime - dt);
            if (hellWard > 0 && (hellWardLife -= dt) <= 0)
            {
                hellWard = 0;
                Burst(transform.position, hellWardRadius, hellWardBurst);
            }
            hellOwned.RemoveAll(go => go == null);
            if (Def.huArchetype == 13 && !hellHatched && hellAge >= Def.huHatchSeconds)
            {
                hellHatched = true;
                SummonHell(Def.huSummon, Def.huSummonCount, Def.huSummonCount);
                // Wyklucie jest przemianą: nowe pająki nie należą do ginącego jaja.
                foreach (var child in All) if (child.Owner == this) child.Owner = Owner;
                Die(); return;
            }
            if (Def.huArchetype == 11 && CurrentState == State.Dormant && FlatDistance(player.transform.position) > Def.huWakeRadius)
                stateTimer = Mathf.Max(stateTimer, dt + 0.1f);
            if (PhaseIndex > hellPhaseSeen)
            {
                for (int i = 0; i < Def.attacks.Count; i++)
                    if (Def.attacks[i].huOnPhase > hellPhaseSeen && Def.attacks[i].huOnPhase <= PhaseIndex) hellPhaseEvents.Enqueue(i);
                hellPhaseSeen = PhaseIndex;
            }
            if (hellPhaseEvents.Count > 0 && CurrentState == State.Moving) StartAttack(hellPhaseEvents.Dequeue());
        }

        bool HellUsable(EnemyAttackEntry e)
        {
            if (Def.huArchetype == 7 && Def.huSummon != null)
            {
                bool canRaise = !HellSummonerSpent && HellRaisedAlive() < Def.attacks[0].summonMax;
                if (e.special == EnemySpecial.Summon && !canRaise) return false;
                if (e.special == EnemySpecial.None && string.IsNullOrEmpty(e.huAbility?.name) && canRaise) return false;
            }
            if (e.huAbility == null || string.IsNullOrEmpty(e.huAbility.name)) return true;
            if (e.huEnragedOnly && !HellEnraged) return false;
            if (e.huAbility.name == "BloodRite" && RiteAlly(e.huAbility.Get("range", 260) / 24f) == null) return false;
            if (e.huAbility.name == "Petrify" && (hellPetrified || Health.Fraction > e.huAbility.Get("below", 0.3f))) return false;
            return e.huOnPhase == 0;
        }

        void BeginHell(EnemyAttackEntry entry)
        {
            var h = entry.huAbility;
            if (h == null || string.IsNullOrEmpty(h.name)) return;
            hellAim = player.transform.position;
            // Zapowiedź zakotwiczona w miejscu celu: nie podąża za uciekającym graczem.
            if (h.name == "Zones" || h.name == "Rockfall" || h.name == "CycloneEye" || h.name == "Tether")
                specialMarkers.Add(VisualFx.GroundMarker(hellAim, entry.attack.radius, TelegraphColors.For(entry.attack.Telegraph)));
        }

        HitData HellHit(AttackDefinition a, float damage, HellEnemyAbility ability)
        {
            var hit = HitData.FromAttack(a, damage, transform.position, this);
            hit.visualElement = ability.Get("damage.water") > 0 || a.element == Element.Frost ? HellFxElement.Water : ability.Get("damage.air") > 0 ? HellFxElement.Air : HellFx.For(a.element);
            hit.knockback = ability.Get("knockback");
            return hit;
        }
        HitData BasicHit(AttackDefinition a, float damage, Vector3 at)
        {
            var hit = HitData.FromAttack(a, damage, at, this);
            // Bazowy pocisk/ręka też zachowuje wodę HU, mimo mechanicznych odporności mrozu Turris.
            if (!string.IsNullOrEmpty(Def.huSourceId) && a.element == Element.Frost) hit.visualElement = HellFxElement.Water;
            return hit;
        }

        bool HellStrike(AttackDefinition a, float dmg)
        {
            var entry = current;
            var h = entry.huAbility;
            if (h == null || string.IsNullOrEmpty(h.name)) return false;
            var hit = HellHit(a, dmg, h);
            Color color = TelegraphColors.For(a.Telegraph);
            var family = hit.visualElement.Value;
            ClearSpecialMarkers();
            void Strike(Vector3 at, float radius, HitData data) => Burst(at, radius, data);
            void ProjectileFan(bool orb)
            {
                int count = Mathf.Clamp(a.projectileCount, 1, 24);
                var origin = AimPoint + transform.forward * 0.7f;
                var aim = (player.AimPoint - origin).normalized;
                for (int i = 0; i < count; i++)
                {
                    float angle = count == 1 ? 0 : Mathf.Lerp(-a.spreadAngle / 2, a.spreadAngle / 2, i / (float)(count - 1));
                    var shot = hit;
                    if (orb) shot.explosionRadius = a.radius;
                    var options = new ProjectileOptions { fullExplosion = orb, zoneRadius = orb ? a.radius : 0, zoneDuration = h.Get("duration"), zoneInterval = h.Get("interval", 0.5f), zoneShare = h.Get("share", 0.25f) };
                    Own(Projectile.Spawn(origin, Quaternion.Euler(0, angle, 0) * aim, a.projectileSpeed, shot, Faction.Enemy, this, Names.ElementColor(a.element), orb ? 0.4f : 0.18f, 6, null, 0, false, options));
                }
            }
            void Channel(float duration, float interval, Action tick, bool persistent = false)
            {
                var c = Own(HellEnemyChannel.Run(this, duration / Mathf.Max(0.1f, AttackSpeedMult), interval / Mathf.Max(0.1f, AttackSpeedMult), tick));
                if (!persistent) hellChannel = c;
            }
            switch (h.name)
            {
                case "InfernalCharge": case "Latch": case "Summon": return false; // istniejące dostawy Turris
                case "Volley": ProjectileFan(false); return true;
                case "InfernalOrb": ProjectileFan(true); return true;
                case "Cocoon":
                    Own(HellCocoon.Spawn(this, player, hit, a.projectileSpeed, a.reach, h.Get("root", 1.2f), a.radius, h.Get("duration", 5)));
                    return true;
                case "Zones":
                    int count = Mathf.Clamp((int)h.Get("count", 1), 1, 16);
                    for (int i = 0; i < count; i++)
                    {
                        var at = hellAim + (i == 0 ? Vector3.zero : Quaternion.Euler(0, i * 360f / count, 0) * Vector3.forward * a.radius * 1.9f);
                        Own(DelayedStrike.Spawn(at, a.radius, 0.1f, hit, Faction.Enemy, this, color));
                    }
                    return true;
                case "Rockfall":
                    Channel(h.Get("duration", 4), h.Get("interval", 1), () =>
                    {
                        int n = Mathf.Clamp((int)h.Get("count", 2), 1, 8);
                        for (int i = 0; i < n; i++)
                        {
                            var at = player.transform.position + (i == 0 ? Vector3.zero : Quaternion.Euler(0, i * 360f / n, 0) * Vector3.forward * a.radius * 2.5f);
                            Own(DelayedStrike.Spawn(at, a.radius, h.Get("telegraph", 1.2f), hit, Faction.Enemy, this, color));
                        }
                    }, true); return true;
                case "TidalWave": case "Flood": case "StoneSpikes":
                    int waves = Mathf.Clamp((int)h.Get("count", 1), 1, 12);
                    for (int i = 0; i < waves; i++)
                    {
                        var dir = Quaternion.Euler(0, waves == 1 ? 0 : Mathf.Lerp(-h.Get("spread", 70) / 2, h.Get("spread", 70) / 2, i / (float)(waves - 1)), 0) * transform.forward;
                        float width = h.Get("width", 60) / 24f;
                        Own(WaveEffect.Spawn(this, transform.position, dir, h.Get("speed", h.name == "StoneSpikes" ? 240 : 230) / 24f, h.Get("range", 420) / 24f, width, 0, hit, color));
                    }
                    return true;
                case "CycloneEye": case "Whirlwind":
                    bool follows = h.name == "Whirlwind";
                    var center = follows ? transform.position : hellAim;
                    var fx = HellFx.Ground(center, a.radius, family, Names.ElementColor(a.element), h.Get("duration", 3));
                    if (fx != null) hellOwned.Add(fx);
                    Channel(h.Get("duration", 3), h.Get("interval", 0.4f), () =>
                    {
                        if (follows) center = transform.position;
                        Strike(center, a.radius, hit);
                        if (!follows && Flat(player.transform.position - center).magnitude <= a.radius * h.Get("share", 3))
                            player.Knockback((center - player.transform.position).normalized, h.Get("pull", 60) / 24f * h.Get("interval", 0.4f));
                    }); return true;
                case "WindStep": case "BlinkStrike":
                    var away = Flat(transform.position - player.transform.position).normalized;
                    if (away.sqrMagnitude < 0.01f) away = -player.transform.forward;
                    var destination = player.transform.position - (h.name == "BlinkStrike" ? player.transform.forward : away) * 1.5f;
                    HellFx.BlinkSparks(transform.position, color, true);
                    if (cc.enabled) cc.Move(destination - transform.position);
                    HellFx.BlinkSparks(transform.position, color, false);
                    if (h.name == "BlinkStrike") Own(DelayedStrike.Spawn(transform.position, Mathf.Max(1.8f, a.radius), 0.35f, hit, Faction.Enemy, this, color));
                    else Strike(transform.position, Mathf.Max(1.8f, a.radius), hit);
                    if (h.name == "WindStep") Own(DamageZone.Spawn(hellAim, h.Get("width", 44) / 24f, h.Get("duration", 2.5f), h.Get("interval", 0.4f), hit, Faction.Enemy, this, color));
                    return true;
                case "Quake":
                    Strike(transform.position, a.radius, hit);
                    var after = hit; after.physical *= h.Get("share", 0.6f); after.magic *= h.Get("share", 0.6f);
                    Own(DelayedStrike.Spawn(transform.position, a.radius * 1.7f, 0.7f, after, Faction.Enemy, this, color)); return true;
                case "StoneWard":
                    hellWard = Health.Max * h.Get("capacity", 0.08f); hellWardLife = h.Get("duration", 6);
                    hellWardDaze = h.Get("daze", 3); hellWardRadius = a.radius; hellWardBurst = hit;
                    CombatEvents.RaiseWorldText(AimPoint, "KAMIENNA OSŁONA", Names.ElementColor(Element.Earth));
                    return true;
                case "EggSac": SummonHell(entry.summon, (int)h.Get("count", 1), (int)h.Get("max", 3)); return true;
                case "BloodRite":
                    float range = h.Get("range", 260) / 24f;
                    var ally = RiteAlly(range);
                    if (ally == null) return true;
                    int hits = hellDirectHits;
                    hellChannel = Own(HellEnemyChannel.Run(this, h.Get("duration", 2) / Mathf.Max(0.1f, AttackSpeedMult), 0.2f, () =>
                    {
                        var direction = Flat(ally.transform.position - transform.position);
                        if (direction.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(direction);
                        FxLibrary.Lightning(AimPoint, ally.AimPoint, new Color(1, 0.12f, 0.06f));
                    }, () =>
                    {
                        ally.hellFrenzy = h.Get("frenzy", 8);
                        CombatEvents.RaiseWorldText(ally.AimPoint, "SZAŁ", new Color(1, 0.12f, 0.06f));
                        HellFx.Blood(ally.AimPoint, Vector3.up, 1.1f);
                    }, () => hellDirectHits == hits && ally != null && !ally.IsDead && FlatDistance(ally.transform.position) <= range * 1.4f,
                        () => Interrupt(State.Staggered, 0.5f)));
                    return true;
                case "Petrify":
                    hellPetrified = true;
                    hellStoneTime = h.Get("duration", 3);
                    Channel(h.Get("duration", 3), h.Get("duration", 3), () => { Health.Restore(Health.Max * h.Get("heal", 0.25f)); ProjectileFan(false); }, false);
                    return true;
                case "Tether":
                    Channel(h.Get("duration", 3), h.Get("interval", 0.5f), () =>
                    {
                        if (FlatDistance(player.transform.position) > h.Get("range", 210) / 24f) { InterruptHell(); return; }
                        FxLibrary.Lightning(AimPoint, player.AimPoint, Names.ElementColor(a.element));
                        var result = HitQuery.Apply(this, player, hit, false);
                        Health.Restore(result.healthDamage * h.Get("share", 1.5f));
                    }); return true;
                case "Explode": Strike(transform.position, a.radius, hit); Die(); return true;
                case "ShieldBash": case "Vomit": case "Swing": case "ComboStrike":
                    bool area = h.argument == "slam" || h.argument == "spin" || h.argument == "leap";
                    if (area) Strike(transform.position, a.radius, hit);
                    else
                    {
                        var tr = new HitTracker();
                        HitQuery.Sphere(transform.position + Vector3.up * 0.7f, Mathf.Max(a.reach, a.radius), Faction.Enemy, tr, receiver =>
                        {
                            var direction = Flat(receiver.Transform.position - transform.position);
                            if (Vector3.Angle(transform.forward, direction) <= h.Get("arc", 120) / 2) HitQuery.Apply(this, receiver, hit, true);
                        });
                    }
                    return true;
                default: throw new InvalidOperationException("Brak dostawy HU: " + h.name);
            }
        }

        void SummonHell(EnemyDefinition definition, int wanted, int limit)
        {
            if (definition == null) return;
            int count = 0;
            foreach (var child in All) if (child != null && !child.IsDead && child.Owner == this && child.Def == definition) count++;
            int add = Mathf.Min(wanted, Mathf.Max(0, limit - count));
            for (int i = 0; i < add; i++) SummonRequested?.Invoke(this, definition, transform.position + Quaternion.Euler(0, i * 360f / Mathf.Max(1, add), 0) * Vector3.forward * 2);
        }

        float AbsorbHellWard(float damage)
        {
            if (hellWard <= 0) return damage;
            float blocked = Mathf.Min(damage, hellWard); hellWard -= blocked;
            if (hellWard <= 0) { hellWardLife = 0; Interrupt(State.Staggered, hellWardDaze); }
            return damage - blocked;
        }
    }

    /// <summary>Kanał zależy od czasu gry. Przerwanie niszczy komponent, śmierć usuwa także trwałe zagrożenia fazy.</summary>
    public class HellEnemyChannel : MonoBehaviour
    {
        EnemyBrain owner;
        Action tick;
        Action finish, broken;
        Func<bool> valid;
        bool cancelled;
        float remaining, interval, accumulator;
        public static HellEnemyChannel Run(EnemyBrain owner, float duration, float interval, Action tick, Action finish = null, Func<bool> valid = null, Action broken = null)
        {
            var c = new GameObject("HellEnemyChannel").AddComponent<HellEnemyChannel>();
            c.owner = owner; c.tick = tick; c.remaining = duration; c.interval = Mathf.Max(0.1f, interval);
            c.finish = finish; c.valid = valid; c.broken = broken;
            return c;
        }
        public void Cancel() { cancelled = true; Destroy(gameObject); }
        void Update() => Step(Time.deltaTime);
        public void Step(float dt)
        {
            if (dt <= 0 || cancelled) return;
            if (owner == null || owner.IsDead || !owner.HasLiveTarget) { Cancel(); return; }
            if (valid != null && !valid()) { Cancel(); broken?.Invoke(); return; }
            if (owner.InHitStop) return;
            float advance = Mathf.Min(dt, remaining); remaining -= advance; accumulator += advance;
            while (accumulator + 0.00001f >= interval) { accumulator -= interval; tick?.Invoke(); if (cancelled || owner == null || owner.IsDead) return; }
            if (remaining <= 0) { finish?.Invoke(); Cancel(); }
        }
    }
}
