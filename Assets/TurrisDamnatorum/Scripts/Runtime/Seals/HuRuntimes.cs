using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Znak na ziemi odmierzający zapowiedź (zapalnik, mina, upadek), usuwany po czasie.</summary>
    public sealed class HuTelegraphRuntime : IHuRuntime
    {
        readonly GameObject mark; readonly float seconds; float t;
        public HuTelegraphRuntime(GameObject mark, float seconds) { this.mark = mark; this.seconds = Mathf.Max(0.05f, seconds); }
        public bool Tick(float dt)
        {
            t += dt;
            if (mark != null) { var d = mark.GetComponent<FxDecal>(); if (d != null) d.Progress = Mathf.Clamp01(t / seconds); }
            return t < seconds;
        }
        public void Stop() { if (mark != null) UnityEngine.Object.Destroy(mark); }
    }

    /// <summary>Grawitacja: przed uderzeniem punkt wciąga wszystko z promienia, potem uderzenie tam, gdzie zebrało demony.</summary>
    public sealed class HuImplode : IHuRuntime
    {
        readonly Vector3 at; readonly float radius, speed, seconds; readonly Action then; readonly GameObject mark;
        float t; bool done;
        public HuImplode(HuCast cast, Vector3 at, Action then)
        {
            this.at = HuWorld.Ground(at); this.then = then;
            radius = cast.Area("implode_radius", 4f);
            seconds = Mathf.Max(0.1f, cast.P.GetF("implode_seconds", 0.4f));
            speed = cast.P.GetF("implode_pull", 2.5f) / seconds;
            mark = VisualFx.GroundMarker(this.at, radius, new Color(0.45f, 0.2f, 0.7f), HellFxElement.Soul);
            Sfx.Play(SoundId.LightningZap, this.at, 0.6f, 0.6f);
        }
        public bool Tick(float dt)
        {
            if (done) return false;
            t += dt;
            HuWorld.PullEnemies(at, radius, speed * dt * 3f);
            if (mark != null) { float left = 1f - Mathf.Clamp01(t / seconds); mark.transform.localScale = Vector3.one * Mathf.Max(0.15f, left); }
            if (t < seconds) return true;
            done = true;
            if (mark != null) UnityEngine.Object.Destroy(mark);
            then();
            return false;
        }
        public void Stop() { done = true; if (mark != null) UnityEngine.Object.Destroy(mark); }
    }

    /// <summary>Wir: wciąga wrogów do punktu i rani co odstęp; może skończyć się wybuchem, który ich wyrzuca.</summary>
    public sealed class HuVortex : IHuRuntime
    {
        readonly HuCast cast; readonly Vector3 center; readonly float radius, interval, pullSpeed, burstShare, burstKnock;
        readonly HitData hit; readonly Transform follow;
        readonly GameObject ground; readonly HellParticles swirl;
        readonly HellFxElement look;
        float remaining, timer, ringTimer;
        public HuVortex(HuCast cast, Vector3 center, float radius, float seconds, float interval, float pull, HitData hit, float burstShare, float burstKnockback, Transform follow = null)
        {
            this.cast = cast; this.center = HuWorld.Ground(center); this.radius = radius; remaining = seconds;
            this.interval = Mathf.Max(0.1f, interval); timer = this.interval; pullSpeed = pull; this.hit = hit;
            this.burstShare = burstShare; burstKnock = burstKnockback; this.follow = follow;
            // Wir fizyczny (tornado z młynka) rysowany wiatrem – sam pył byłby niewidoczny.
            look = cast.family == HellFxElement.Physical ? HellFxElement.Air : cast.family;
            if (FxLibrary.Enabled)
            {
                ground = HellFx.Ground(this.center, radius, look, cast.color, seconds + 0.2f);
                swirl = HellFx.Ambient(follow, follow != null ? Vector3.up * 0.4f : this.center + Vector3.up * 0.4f, look, radius * 0.8f, seconds);
            }
            Sfx.Play(Sfx.ForElement(hit.element, SoundId.Shockwave), this.center, 0.6f, 0.7f);
        }
        Vector3 Center => follow != null ? HuWorld.Ground(follow.position) : center;
        public bool Tick(float dt)
        {
            remaining -= dt;
            var c = Center;
            if (pullSpeed > 0f) HuWorld.PullEnemies(c, radius, pullSpeed * dt * 2.5f);
            else if (pullSpeed < 0f) HuWorld.PushEnemies(c, radius, -pullSpeed * dt * 2.5f);
            timer -= dt;
            while (timer <= 0f) { timer += interval; HuWorld.HitCircle(cast.caster, c, radius, hit); }
            if (ground != null && follow != null) ground.transform.position = c;
            ringTimer -= dt;
            if (ringTimer <= 0f && FxLibrary.Enabled)
            {
                ringTimer = 0.35f;
                HellFx.Shockwave(c, look, cast.color, radius * 0.9f);
                if (look == HellFxElement.Air || look == HellFxElement.Earth) FxLibrary.Dust(c, radius * 0.5f);
            }
            if (remaining > 0f) return true;
            if (burstShare > 0f)
            {
                var b = hit; b.physical *= burstShare; b.magic *= burstShare; b.knockback = burstKnock;
                HuWorld.Explode(cast.caster, c, radius, b, cast.color);
            }
            return false;
        }
        public void Stop()
        {
            if (ground != null) UnityEngine.Object.Destroy(ground);
            if (swirl != null) swirl.StopAndRelease();
        }
    }

    /// <summary>
    /// Fala: pas leci od początku do zasięgu; każdy cel trafiony raz na przelot. Z udziałem powrotu wraca słabsza.
    /// Z kątem rozszerzania pas poszerza się w locie – zamiata stożek, nie korytarz.
    /// </summary>
    public sealed class HuWave : IHuRuntime
    {
        const float Front = 0.7f;
        readonly HuCast cast; readonly Vector3 origin, dir; readonly float start, range, halfWidth, spreadTan, speed, knockback;
        readonly float returnShare; HitData hit;
        readonly HitTracker tracker = new HitTracker();
        readonly GameObject visual; readonly HellParticles fx;
        float position, splash; bool returning;

        public HuWave(HuCast cast, Vector3 origin, Vector3 dir, float start, float end, float halfWidth, float speed, HitData hit, float share, float returnShare, float spreadDegrees)
        {
            this.cast = cast; this.origin = origin; this.dir = dir; this.start = start; range = Mathf.Max(start + 0.5f, end);
            this.halfWidth = Mathf.Max(0.3f, halfWidth); this.speed = Mathf.Max(1f, speed); this.returnShare = returnShare;
            spreadTan = Mathf.Tan(Mathf.Clamp(spreadDegrees, 0f, 150f) * 0.5f * Mathf.Deg2Rad);
            this.hit = hit; this.hit.physical *= share; this.hit.magic *= share; knockback = hit.knockback;
            position = start;
            visual = new GameObject("HuWave");
            visual.transform.SetPositionAndRotation(origin + dir * start + Vector3.up * 0.4f, Quaternion.LookRotation(dir));
            if (FxLibrary.Enabled)
            {
                fx = HellFx.Wave(visual.transform, cast.family, this.halfWidth, (range - start) / this.speed * (returnShare > 0 ? 2f : 1f) + 0.2f);
                Projectile.AddTrail(visual, cast.color, this.halfWidth * 2f, 0.25f);
            }
        }

        float HalfAt(float d) => halfWidth + Mathf.Max(0f, d - start) * spreadTan;
        Vector3 At(float d) => origin + dir * d;

        public bool Tick(float dt)
        {
            float prev = position, step = speed * dt;
            position = returning ? Mathf.Max(start, position - step) : Mathf.Min(range, position + step);
            float near = Mathf.Min(prev, position) - Front * 0.5f, far = Mathf.Max(prev, position) + Front * 0.5f;
            Vector3 push = returning ? -dir : dir;
            var h = hit; h.knockback = 0f;
            HuWorld.HitBand(cast.caster, At(Mathf.Max(0f, near)), At(far), HalfAt(far), h, tracker, null, push);
            if (knockback != 0f)
                foreach (var e in EnemyBrain.All)
                {
                    if (e == null || e.IsDead || !tracker.Contains(e)) continue;
                    float along = Vector3.Dot(e.transform.position - origin, dir);
                    if (along >= near && along <= far + 0.3f) e.Knockback(push, Mathf.Abs(knockback) * dt * 6f, 0.1f);
                }
            splash -= dt;
            if (splash <= 0f && FxLibrary.Enabled)
            {
                // Czoło fali: rozbryzgi w poprzek pasa, żeby fala była widoczna na całej szerokości.
                splash = 0.07f;
                Vector3 side = new Vector3(dir.z, 0f, -dir.x);
                float hw = HalfAt(position);
                for (int i = -1; i <= 1; i++) HellFx.Burst(At(position) + side * (i * hw * 0.7f), cast.family, Mathf.Max(0.6f, hw * 0.45f));
            }
            if (visual != null)
            {
                visual.transform.position = At(position) + Vector3.up * 0.4f;
                visual.transform.rotation = Quaternion.LookRotation(push);
                visual.transform.localScale = new Vector3(HalfAt(position) / halfWidth, 1f, 1f);
            }
            bool atEnd = returning ? position <= start + 0.001f : position >= range - 0.001f;
            if (!atEnd) return true;
            if (!returning && returnShare > 0f)
            {
                returning = true; hit.physical *= returnShare; hit.magic *= returnShare; tracker.Reset();
                return true;
            }
            return false;
        }

        public void Stop()
        {
            if (fx != null) fx.StopAndRelease();
            if (visual != null) UnityEngine.Object.Destroy(visual, 0.3f);
        }
    }

    /// <summary>Linia kolców: zapowiedź wzdłuż kierunku, potem erupcja; opcjonalnie druga erupcja po chwili.</summary>
    public sealed class HuLine : IHuRuntime
    {
        readonly HuCast cast; readonly Vector3 from, to; readonly float halfWidth, delay, secondDelay, secondShare; readonly HitData hit;
        readonly GameObject mark; float t; int eruptions;
        public HuLine(HuCast cast, Vector3 from, Vector3 to, float halfWidth, float delay, HitData hit, float secondDelay, float secondShare)
        {
            this.cast = cast; this.from = HuWorld.Ground(from); this.to = HuWorld.Clamp(from, to); this.halfWidth = halfWidth;
            this.delay = Mathf.Max(0.05f, delay); this.hit = hit; this.secondDelay = secondDelay; this.secondShare = secondShare;
            if (FxLibrary.Enabled)
            {
                Vector3 mid = (this.from + this.to) * 0.5f;
                float len = HuWorld.Flat(this.from, this.to);
                mark = VisualFx.GroundMarker(mid, Mathf.Max(halfWidth, 0.5f), cast.color, cast.family);
                if (mark != null)
                {
                    mark.transform.rotation = Quaternion.LookRotation(HuWorld.FlatDir(this.from, this.to));
                    mark.transform.localScale = new Vector3(1f, 1f, Mathf.Max(1f, len / Mathf.Max(0.5f, halfWidth * 2f)));
                }
            }
        }
        public bool Tick(float dt)
        {
            t += dt;
            float due = eruptions == 0 ? delay : delay + secondDelay;
            if (mark != null) { var d = mark.GetComponent<FxDecal>(); if (d != null) d.Progress = Mathf.Clamp01(t / due); }
            if (t < due) return true;
            Erupt(eruptions == 0 ? 1f : secondShare);
            eruptions++;
            return eruptions == 1 && secondDelay > 0f && secondShare > 0f;
        }
        void Erupt(float share)
        {
            var h = hit; h.physical *= share; h.magic *= share;
            HuWorld.HitBand(cast.caster, from, to, halfWidth, h);
            float len = HuWorld.Flat(from, to);
            int spikes = Mathf.Max(2, (int)(len / 1.1f));
            for (int i = 0; i <= spikes; i += 2)
            {
                Vector3 p = Vector3.Lerp(from, to, i / (float)spikes);
                if (cast.family == HellFxElement.Earth) { HellFx.SlamDust(p, halfWidth * 1.2f); ShockRing.Spawn(p, halfWidth * 1.1f, 0.7f, 0.5f); }
                else HellFx.Burst(p, cast.family, halfWidth * 1.2f);
            }
            Sfx.Play(SoundId.Quake, Vector3.Lerp(from, to, 0.5f), 0.7f, 1.2f);
        }
        public void Stop() { if (mark != null) UnityEngine.Object.Destroy(mark); }
    }

    /// <summary>Smuga za doskokiem bohatera: rani, co w niej stoi, aż zgaśnie.</summary>
    public sealed class HuTrail : IHuRuntime
    {
        const float Segment = 0.8f;
        readonly HuCast cast; readonly Transform follow; readonly float followSeconds, trailSeconds, halfWidth, interval; readonly HitData hit;
        readonly List<(Vector3 a, Vector3 b, float until, GameObject fx)> segments = new List<(Vector3, Vector3, float, GameObject)>();
        float t, timer; Vector3 last; bool dropped;
        public HuTrail(HuCast cast, Transform follow, float followSeconds, float trailSeconds, float halfWidth, float interval, HitData hit)
        {
            this.cast = cast; this.follow = follow; this.followSeconds = followSeconds; this.trailSeconds = trailSeconds;
            this.halfWidth = halfWidth; this.interval = Mathf.Max(0.1f, interval); this.hit = hit;
            last = follow != null ? follow.position : Vector3.zero;
        }
        public bool Tick(float dt)
        {
            t += dt;
            if (t <= followSeconds + 0.05f && follow != null)
            {
                Vector3 cur = follow.position;
                if (HuWorld.Flat(last, cur) >= Segment || (t >= followSeconds && HuWorld.Flat(last, cur) > 0.1f)) { Drop(last, cur); last = cur; dropped = true; }
                else if (t >= followSeconds && !dropped) { Drop(cur - follow.forward * 0.3f, cur + follow.forward * 0.3f); dropped = true; } // zablokowany doskok: ślad tam, gdzie bohater stoi
            }
            for (int i = segments.Count - 1; i >= 0; i--)
                if (HuWorld.Clock >= segments[i].until) { if (segments[i].fx != null) UnityEngine.Object.Destroy(segments[i].fx); segments.RemoveAt(i); HuWorld.LiveZones--; }
            timer -= dt;
            if (timer <= 0f && segments.Count > 0)
            {
                timer += interval;
                var tracker = new HitTracker();
                foreach (var s in segments) HuWorld.HitBand(cast.caster, s.a, s.b, halfWidth, hit, tracker);
            }
            return t <= followSeconds + 0.05f || segments.Count > 0;
        }
        void Drop(Vector3 a, Vector3 b)
        {
            if (HuWorld.LiveZones >= HuWorld.MaxZones) return;
            HuWorld.LiveZones++;
            GameObject fx = FxLibrary.Enabled ? HellFx.Ground(HuWorld.Ground((a + b) * 0.5f), Mathf.Max(halfWidth, HuWorld.Flat(a, b) * 0.6f), cast.family, cast.color, trailSeconds + 0.3f) : null;
            segments.Add((HuWorld.Ground(a), HuWorld.Ground(b), HuWorld.Clock + trailSeconds, fx));
        }
        public void Stop()
        {
            foreach (var s in segments) { if (s.fx != null) UnityEngine.Object.Destroy(s.fx); HuWorld.LiveZones--; }
            segments.Clear();
        }
    }

    /// <summary>Mina: czeka na ziemi; uzbraja się, wybucha, gdy podejdzie demon, albo po czasie. Jest kotwicą.</summary>
    public sealed class HuMine : IHuRuntime, IHuAnchor
    {
        readonly HuCast cast; readonly Vector3 at; readonly float armAt, endAt, trigger;
        readonly GameObject ring; readonly HellParticles core;
        float t; bool done;
        public HuMine(HuCast cast, Vector3 at)
        {
            this.cast = cast; this.at = HuWorld.Ground(at);
            armAt = cast.P.GetF("arm_seconds", 0.4f); endAt = cast.P.GetF("mine_seconds", 8f); trigger = cast.Area("trigger_radius", 1.6f);
            if (FxLibrary.Enabled)
            {
                ring = VisualFx.GroundMarker(this.at, trigger, cast.color, cast.family);
                core = HellFx.Ambient(null, this.at + Vector3.up * 0.25f, cast.family, 0.35f);
                HellFx.Burst(this.at, cast.family, 0.8f);
            }
            HuWorld.RegisterAnchor(this);
        }
        public Vector3 AnchorPosition => at;
        public bool AnchorLive => !done;
        public bool Tick(float dt)
        {
            if (done) return false;
            t += dt;
            bool armed = t >= armAt;
            if (ring != null) { var d = ring.GetComponent<FxDecal>(); if (d != null) d.Progress = armed ? 0.5f + 0.5f * Mathf.Sin(t * 9f) : 0.15f; }
            if (t >= endAt || (armed && HuWorld.Nearest(at, trigger) != null)) { Trigger(); return false; }
            return true;
        }
        public void Trigger()
        {
            if (done) return;
            done = true; Free();
            cast.Impact(at);
        }
        public void Stop() { done = true; Free(); }
        void Free()
        {
            if (ring != null) UnityEngine.Object.Destroy(ring);
            if (core != null) core.StopAndRelease();
        }
    }

    /// <summary>
    /// Słup ofiarny: kamienny słup rzuca skill sam co odstęp w najbliższego demona, z częścią mocy. Jego rzuty to kopie
    /// z miejsca słupa (nie przenoszą bohatera). Jest kotwicą – nova z przekaźnikiem wybucha też przy nim (i go burzy).
    /// </summary>
    public sealed class HuTotem : IHuRuntime, IHuAnchor
    {
        readonly HuCast cast; readonly Vector3 at; readonly float range, interval, seconds, share, reach;
        readonly GameObject idol; float t, next; bool done;
        public HuTotem(HuCast cast, Vector3 at, float range)
        {
            this.cast = cast; this.at = HuWorld.Ground(at); this.range = range;
            interval = Mathf.Max(0.25f, cast.P.GetF("totem_interval", 1.2f)); seconds = cast.P.GetF("totem_seconds", 8f);
            share = cast.P.GetF("totem_share", 0.6f); reach = cast.P.GetF("totem_reach", 11f); next = 0.35f;
            idol = BuildIdol(this.at, cast.color);
            HellFx.Burst(this.at, cast.family, 1.4f);
            HuWorld.RegisterAnchor(this);
        }
        public Vector3 AnchorPosition => at;
        public bool AnchorLive => !done;
        public void Trigger() => Stop();
        public bool Tick(float dt)
        {
            if (done) return false;
            t += dt;
            if (t >= seconds || cast.caster == null || cast.caster.IsDead) { Stop(); return false; }
            if (t < next) return true;
            next = t + interval;
            var prey = HuWorld.Nearest(at, reach);
            if (prey == null) return true;
            Vector3 heading = HuWorld.FlatDir(at, prey.transform.position);
            Vector3 target = cast.shape.Targeting == HuTargeting.Point ? prey.transform.position : cast.shape.Targeting == HuTargeting.Self ? at : at + heading * range;
            var copy = cast.Variant(at, heading, target, share, true);
            FxLibrary.Lightning(at + Vector3.up * 1.7f, at + Vector3.up * 2.3f, cast.color);
            HuSkillCaster.Fire(copy, heading, range);
            return true;
        }
        public void Stop()
        {
            if (done) return;
            done = true;
            HellFx.Burst(at, cast.family, 1.2f);
            if (idol != null) UnityEngine.Object.Destroy(idol);
        }

        static GameObject BuildIdol(Vector3 at, Color color)
        {
            var go = new GameObject("HuTotem");
            go.transform.position = at;
            PartBuilder.Add(go.transform, ProcMesh.Tube(0.62f), Surface.Stone, new Color(0.16f, 0.13f, 0.12f), new Vector3(0, 0.85f, 0), new Vector3(0.6f, 0.85f, 0.6f));
            PartBuilder.Add(go.transform, ProcMesh.Sphere(), Surface.Stone, Color.Lerp(color, Color.white, 0.3f), new Vector3(0, 1.85f, 0), Vector3.one * 0.4f);
            PartBuilder.BakeAll(go.transform);
            if (FxLibrary.Enabled) FxLibrary.PointLight(at + Vector3.up * 1.9f, color, 1.6f, 4f, 30f, go.transform);
            return go;
        }
    }

    /// <summary>Kula w locie, która co odstęp parzy (i wciąga) wokół siebie, albo zostawia za sobą pas żywiołu (Smuga).</summary>
    public sealed class HuShotPulse : IHuRuntime
    {
        readonly HuCast cast; readonly HuShot shot; readonly float radius, interval, pull, trailEvery; readonly HitData hit;
        readonly HellParticles swirl;
        float timer, trailTimer;
        public HuShotPulse(HuCast cast, HuShot shot)
        {
            this.cast = cast; this.shot = shot;
            trailEvery = cast.P.GetF("trail_zone_interval"); trailTimer = trailEvery * 0.5f;
            radius = cast.Area("pulse_radius", 1.5f); interval = Mathf.Max(0.1f, cast.P.GetF("pulse_interval", 0.4f));
            pull = cast.Length("pulse_pull"); hit = cast.Hit(cast.P.GetF("pulse_share", 0.3f), plain: true); timer = interval * 0.5f;
            // Pocisk pulsujący (tornado, kula błyskawic, toczący się żar) widać w całym jego zasięgu, nie tylko jądro.
            if (cast.P.On("pulse_radius") && FxLibrary.Enabled && shot != null)
                swirl = HellFx.Ambient(shot.transform, Vector3.zero, cast.family == HellFxElement.Physical ? HellFxElement.Air : cast.family, radius * 0.7f);
        }
        public bool Tick(float dt)
        {
            if (shot == null || !shot.AnchorLive) return false;
            Vector3 at = shot.AnchorPosition;
            if (pull > 0f) HuWorld.PullEnemies(at, radius * 1.6f, pull * dt * 2.5f);
            timer -= dt;
            if (cast.P.On("pulse_radius") && timer <= 0f)
            {
                timer += interval;
                HuWorld.HitCircle(cast.caster, at, radius, hit);
                HellFx.Burst(at, cast.family == HellFxElement.Physical ? HellFxElement.Air : cast.family, radius * 0.6f);
            }
            if (trailEvery > 0f)
            {
                trailTimer -= dt;
                if (trailTimer <= 0f)
                {
                    trailTimer += trailEvery;
                    HuSkillCaster.SpawnZone(cast, HuWorld.Ground(at), cast.Area("trail_zone_radius", 1.2f), cast.P.GetF("trail_zone_seconds", 2.5f), 0.5f, cast.P.GetF("trail_zone_share", 0.15f));
                }
            }
            return true;
        }
        public void Stop() { if (swirl != null) swirl.StopAndRelease(); }
    }

    /// <summary>Młynek: trafia wokół bohatera co odstęp, gdy ten się porusza; na końcu opcjonalne cięcie dookoła.</summary>
    public sealed class HuSpin : IHuRuntime
    {
        readonly HuCast cast; readonly float radius, interval, duration; readonly HitData hit;
        readonly Dictionary<IHitReceiver, float> lastHit = new Dictionary<IHitReceiver, float>();
        float t, swing; bool interrupted, done;
        public readonly float moveMultiplier;
        public HuSpin(HuCast cast)
        {
            this.cast = cast; hit = cast.Hit(); radius = cast.Area("radius", 2f);
            interval = Mathf.Max(0.1f, cast.P.GetF("hit_interval", 0.3f)); duration = cast.P.GetF("duration_seconds", 2f);
            moveMultiplier = cast.P.GetF("move_multiplier", 0.75f);
            FxLibrary.Dust(cast.caster.transform.position, 1.2f);
        }
        public float Duration => duration;
        public void Interrupt() { interrupted = true; }
        public bool Tick(float dt)
        {
            if (done) return false;
            if (cast.caster == null || cast.caster.IsDead || interrupted) { done = true; return false; }
            t += dt;
            Vector3 c = cast.caster.transform.position;
            if (t >= duration)
            {
                float fs = cast.P.GetF("final_slash_share");
                if (fs > 0f)
                {
                    float fr = cast.Area("final_radius", radius);
                    var h = hit; h.physical *= fs; h.magic *= fs; h.poiseDamage *= 2f;
                    HuWorld.Explode(cast.caster, c, fr, h, cast.color);
                    cast.caster.HuSwing(true);
                }
                done = true; return false;
            }
            HuWorld.HitCircle(cast.caster, c, radius, hit, null, r =>
            {
                if (lastHit.TryGetValue(r, out float when) && t - when < interval) return false;
                lastHit[r] = t; return true;
            });
            swing -= dt;
            if (swing <= 0f) { swing = 0.25f; cast.caster.HuSwing(false); }
            return true;
        }
        public void Stop() { done = true; }
    }

    /// <summary>Doskok w kierunku celowania; ściana zatrzymuje, wróg zatrzymuje, gdy skill tak mówi. Koniec doskoku to uderzenie.</summary>
    public sealed class HuDash : IHuRuntime
    {
        readonly HuCast cast; readonly float seconds; readonly bool stopOnEnemy, impactOnEnd, cutsThrough;
        readonly HitTracker cutTracker = new HitTracker();
        float t, travelled; int stuck; Vector3 lastPos; bool ended;
        readonly float distance;
        public readonly Vector3 velocity;
        public HuDash(HuCast cast, bool impactOnEnd)
        {
            this.cast = cast; this.impactOnEnd = impactOnEnd;
            seconds = Mathf.Max(0.05f, cast.P.GetF("dash_seconds", 0.2f));
            stopOnEnemy = cast.P.On("stop_on_enemy");
            cutsThrough = cast.P.On("dash_cut_share");
            distance = cast.P.GetF("distance", 4f);
            velocity = cast.direction * (distance / seconds);
            lastPos = cast.caster.transform.position;
            // Doskok przechodzi przez ciała wrogów (zatrzymują go ściany i – gdy skill tak mówi – pierwszy wróg).
            cast.caster.SetPassThroughEnemies(true);
        }

        /// <summary>Koniec akcji przed końcem doskoku: doskok kończy się teraz – z uderzeniem, jeśli je ma.</summary>
        public void EndNow() { if (!ended) End(); }
        public float Seconds => seconds;
        public float LeapHeight => cast.P.GetF("leap_height");
        public float Progress => Mathf.Clamp01(t / seconds);
        /// <summary>Prędkość na tę klatkę: nigdy dalej niż do końca dystansu (przy niskim klatkażu ruch nie przelatuje celu).</summary>
        public Vector3 MotionFor(float dt)
        {
            float left = distance - travelled;
            if (ended || left <= 0.05f || dt <= 0f) return Vector3.zero;
            float speed = velocity.magnitude;
            return speed * dt > left ? velocity.normalized * (left / dt) : velocity;
        }
        public bool Tick(float dt)
        {
            if (ended) return false;
            t += dt;
            var pc = cast.caster;
            if (pc == null || pc.IsDead) { ended = true; return false; }
            Vector3 pos = pc.transform.position;
            float moved = HuWorld.Flat(pos, lastPos);
            travelled += moved;
            stuck = t > dt * 1.5f && moved < velocity.magnitude * dt * 0.15f ? stuck + 1 : 0;
            if (cutsThrough)
            {
                var h = cast.Hit(cast.P.GetF("dash_cut_share"));
                HuWorld.HitBand(pc, lastPos, pos + velocity.normalized * 0.6f, 0.9f, h, cutTracker);
            }
            lastPos = pos;
            bool hitEnemy = stopOnEnemy && HuWorld.Nearest(pos, 1.4f) != null;
            if (t >= seconds || hitEnemy || stuck >= 2) { End(); return false; }
            return true;
        }
        void End()
        {
            ended = true;
            cast.caster.SetPassThroughEnemies(false);
            if (impactOnEnd) cast.Impact(cast.caster.transform.position);
        }
        public void Stop() { ended = true; if (cast.caster != null) cast.caster.SetPassThroughEnemies(false); }
    }

    /// <summary>Kamienna osłona: bariera na bohaterze; pęknięcie to uderzenie (fala odrzutu, gdy skill ją ma).</summary>
    public sealed class HuBarrier : IHuRuntime
    {
        readonly HuCast cast; readonly HellParticles fx;
        public HuBarrier(HuCast cast)
        {
            this.cast = cast;
            float capacity = cast.P.GetF("capacity", 40f) * cast.caster.HuHealthScale * cast.power;
            cast.caster.RaiseBarrier(capacity, cast.P.GetF("duration_seconds", 6f), cast.color);
            fx = FxLibrary.Enabled ? HellFx.Ambient(cast.caster.transform, Vector3.up, cast.family, 0.8f, cast.P.GetF("duration_seconds", 6f)) : null;
        }
        public bool Tick(float dt)
        {
            var pc = cast.caster;
            if (pc == null || pc.IsDead) return false;
            if (pc.ConsumeBarrierBroken())
            {
                CombatEvents.RaiseWorldText(pc.AimPoint + Vector3.up * 0.6f, "OSŁONA PĘKŁA", cast.color);
                cast.Impact(pc.transform.position);
                return false;
            }
            return pc.BarrierAmount > 0f;
        }
        public void Stop() { if (fx != null) fx.StopAndRelease(); }
    }
}

namespace Turris
{
    /// <summary>
    /// Ściana ognia (komponent Turris <c>Wall</c>): odcinek w poprzek celowania albo pierścień wokół punktu; płonie przez
    /// <c>wall_seconds</c> i co <c>wall_interval</c> parzy wszystko na swojej linii. Z <c>wall_speed</c> rusza przed siebie.
    /// Liczy się jako jedna strefa skilla (limit <see cref="HuWorld.MaxZones"/>).
    /// </summary>
    public sealed class HuWall : IHuRuntime
    {
        // Gęstość ognia ściany/fali: płomienie co 0,73 m (dawniej 0,95) i żar ×1,3 – ok. +30% ognia na metr.
        const float FlameSpacing = 0.73f, RingSegments = 12, GroundDensity = 1.3f;
        readonly HuCast cast; readonly HitData hit; readonly Vector3 dir;
        readonly float seconds, interval, halfWidth, speed;
        readonly List<(Vector3 a, Vector3 b)> segments = new List<(Vector3, Vector3)>();
        readonly GameObject root;
        Vector3 offset; float t, timer;

        public HuWall(HuCast cast, Vector3 center, Vector3 dir, float length, float width, float seconds, float interval, float share, float speed, float ringRadius)
        {
            this.cast = cast; this.dir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.forward; this.speed = speed;
            this.seconds = Mathf.Max(0.5f, seconds); this.interval = Mathf.Max(0.1f, interval); halfWidth = Mathf.Max(0.3f, width * 0.5f);
            hit = cast.Hit(share * this.interval);
            hit.knockback = 0f; hit.poiseDamage *= 0.1f; hit.explosionRadius = 0f;
            center = HuWorld.Ground(center);
            if (ringRadius > 0f)
            {
                for (int i = 0; i < RingSegments; i++)
                {
                    float a0 = i / RingSegments * Mathf.PI * 2f, a1 = (i + 1) / RingSegments * Mathf.PI * 2f;
                    segments.Add((center + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * ringRadius, center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * ringRadius));
                }
            }
            else
            {
                Vector3 across = Vector3.Cross(Vector3.up, this.dir) * (length * 0.5f);
                segments.Add((center - across, center + across));
            }
            HuWorld.LiveZones++;
            root = new GameObject("HuWall");
            root.transform.position = center;
            if (!FxLibrary.Enabled) return;
            foreach (var (a, b) in segments) BuildSegment(a, b);
            HellFxPool.Instance.FlashLight(center + Vector3.up, cast.color, 3f, Mathf.Max(4f, length), 0.35f);
        }

        /// <summary>Płomienie co metr i żar na ziemi wzdłuż odcinka – podróżują razem z korzeniem ściany.</summary>
        void BuildSegment(Vector3 a, Vector3 b)
        {
            float len = HuWorld.Flat(a, b);
            var seg = new GameObject("Segment");
            seg.transform.SetParent(root.transform, false);
            seg.transform.SetPositionAndRotation(HuWorld.Ground((a + b) * 0.5f), Quaternion.LookRotation(Vector3.Cross(HuWorld.FlatDir(a, b), Vector3.up)));
            var ground = HellFx.Ambient(seg.transform, Vector3.up * 0.08f, cast.family, 1f, seconds);
            ground?.SetBox(len * 0.5f, halfWidth);
            if (cast.family == HellFxElement.Fire) ground?.ScaleAmount(GroundDensity);
            if (cast.family != HellFxElement.Fire) return;
            int flames = Mathf.Max(2, Mathf.RoundToInt(len / FlameSpacing));
            for (int i = 0; i < flames; i++)
            {
                float along = (i + 0.5f) / flames - 0.5f;
                var f = HellFx.Flame(seg.transform, new Vector3(along * len, 0.05f, UnityEngine.Random.Range(-0.15f, 0.15f)), UnityEngine.Random.Range(1.3f, 1.7f));
                if (f != null) f.Play(seconds);
            }
        }

        public bool Tick(float dt)
        {
            t += dt;
            if (speed > 0f) { offset += dir * speed * dt; if (root != null) root.transform.position += dir * speed * dt; }
            timer -= dt;
            if (timer <= 0f && t <= seconds)
            {
                timer += interval;
                var tracker = new HitTracker();
                foreach (var (a, b) in segments) HuWorld.HitBand(cast.caster, a + offset, b + offset, halfWidth + 0.3f, hit, tracker);
            }
            return t < seconds;
        }

        public void Stop()
        {
            if (root != null)
            {
                foreach (var p in root.GetComponentsInChildren<HellParticles>()) p.StopAndRelease();
                UnityEngine.Object.Destroy(root, 0.05f);
            }
            HuWorld.LiveZones = Mathf.Max(0, HuWorld.LiveZones - 1);
        }
    }
}
