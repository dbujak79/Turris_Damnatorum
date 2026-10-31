using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Kiedy komponent działa: przy rzucie, przy rzucie z własnym punktem uderzenia (dostawa) albo w punkcie uderzenia.</summary>
    public enum HuPhase { Cast, Delivery, Impact }

    /// <summary>
    /// Wykonanie skilli Hell Unleashed z komponentów (port <c>SkillCaster</c>): rzut, kopie (echo, wachlarz, lustro,
    /// kaskada), przekaźnik, słupy, wyzwalacze i komponenty dostaw. Zachowanie zależy od komponentu, nigdy od nazwy skilla.
    ///
    /// Bezpieczniki z HU: wszystkie kopie jednego rzutu razem zadają najwyżej 1,5× jego obrażeń i jest ich najwyżej 24;
    /// skill odpalony wyzwalaczem nie odpali się ponownie przez 1,5 s i płaci połowę many/wytrzymałości.
    /// </summary>
    public static class HuSkillCaster
    {
        public const float TriggerCooldownSeconds = 1.5f, TriggerCostShare = 0.5f, SecondaryDamageBudget = 1.5f;
        public const int MaxSecondaryEvents = 24;

        static readonly Dictionary<string, HuPhase> Registry = new Dictionary<string, HuPhase>(StringComparer.OrdinalIgnoreCase)
        {
            ["Projectile"] = HuPhase.Delivery, ["Explosion"] = HuPhase.Impact, ["Zone"] = HuPhase.Impact, ["Rain"] = HuPhase.Impact,
            ["Barrage"] = HuPhase.Impact, ["Chain"] = HuPhase.Cast, ["Nova"] = HuPhase.Cast, ["Shove"] = HuPhase.Cast, ["Wave"] = HuPhase.Cast,
            ["Vortex"] = HuPhase.Cast, ["Line"] = HuPhase.Cast, ["Cone"] = HuPhase.Cast, ["Trail"] = HuPhase.Cast, ["Dash"] = HuPhase.Delivery,
            ["Spin"] = HuPhase.Cast, ["Barrier"] = HuPhase.Delivery, ["Warcry"] = HuPhase.Cast, ["Overload"] = HuPhase.Cast, ["Aura"] = HuPhase.Cast,
            ["Heal"] = HuPhase.Cast, ["Undying"] = HuPhase.Cast, ["Summon"] = HuPhase.Cast, ["Fall"] = HuPhase.Delivery, ["Mine"] = HuPhase.Delivery,
            ["Infuse"] = HuPhase.Impact, ["Wall"] = HuPhase.Impact,
        };

        public static bool IsKnown(string component) => Registry.ContainsKey(component);
        public static HuPhase PhaseOf(string component) => Registry.TryGetValue(component, out var p) ? p : HuPhase.Cast;

        static readonly List<HuMine> mines = new List<HuMine>();
        static readonly List<HuTotem> totems = new List<HuTotem>();
        static readonly Dictionary<SpellInstance, (int stacks, float last)> crescendo = new Dictionary<SpellInstance, (int, float)>();
        static readonly Dictionary<SpellInstance, float> lastTriggered = new Dictionary<SpellInstance, float>();

        /// <summary>Koniec piętra: miny, słupy, narastanie i blokady wyzwalaczy od zera (runtime'y czyści HuWorld).</summary>
        public static void Clear() { mines.Clear(); totems.Clear(); crescendo.Clear(); lastTriggered.Clear(); }

        // ------------------------------------------------------------------ Rzut

        /// <summary>
        /// Rzut bohatera (wołany w fazie aktywnej akcji): narastanie, premie przy rzucie, słup albo zwykłe odpalenie.
        /// Koszt i ładunek pobrał już PlayerCombat.
        /// </summary>
        public static void CastByHand(PlayerCombat pc, SpellRuntime spell, Vector3 aim, Vector3 target, float power)
        {
            var shape = spell.instance.seals.Shape;
            float range = shape.Parameters.GetF(HuParams.Range, 8f);
            power *= Crescendo(pc, spell.instance, shape);
            var cast = new HuCast(pc, spell, pc.transform.position, aim, target, power);
            OnCastBoons(cast);
            if (cast.P.On("totem_seconds")) PlantTotem(cast, range);
            else Fire(cast, aim, range);
        }

        /// <summary>Wszystko, co rzut wywołuje: przekaźnik na kotwice albo sam rzut, potem kopie i echo – z jednym budżetem.</summary>
        public static void Fire(HuCast cast, Vector3 aim, float range)
        {
            var anchors = cast.relayed || !cast.P.On("relay") ? new List<IHuAnchor>() : HuWorld.LiveAnchors(cast.target, Mathf.Max(1, (int)cast.P.Get("relay_max", 3)));
            float scale = SecondaryScale(cast.P, anchors);
            if (anchors.Count > 0) { Relay(cast, anchors, scale); return; }
            Execute(cast);
            ScheduleEcho(cast, aim, range, scale);
            Copies(cast, scale);
        }

        /// <summary>Wspólny budżet kopii: gdy udziały wszystkich kopii przekroczą 1,5×, wszystkie maleją jednakowo.</summary>
        static float SecondaryScale(HuParams p, List<IHuAnchor> anchors)
        {
            int fan = Mathf.Max(0, (int)p.Get("fan")), cascade = Mathf.Max(0, (int)p.Get("cascade"));
            bool mirror = p.On("mirror");
            float echo = Mathf.Max(0f, p.GetF("echo_share"));
            int perBurst = fan + cascade + (mirror ? 1 : 0) + (echo > 0f ? 1 : 0);
            float perBurstShare = fan * p.GetF("fan_share", 0.7f) + cascade * p.GetF("cascade_share", 0.6f) + (mirror ? p.GetF("mirror_share", 0.8f) : 0f) + echo;
            int maxBursts = Mathf.Max(1, (MaxSecondaryEvents + 1) / (1 + perBurst));
            if (anchors.Count > maxBursts) anchors.RemoveRange(maxBursts, anchors.Count - maxBursts);
            int bursts = Mathf.Max(1, anchors.Count);
            float burstShare = anchors.Count > 0 ? p.GetF("relay_share", 1f) : 1f;
            float total = (bursts - 1) * burstShare + bursts * burstShare * perBurstShare;
            return total > SecondaryDamageBudget ? SecondaryDamageBudget / total : 1f;
        }

        /// <summary>Wachlarz (na zmianę z obu stron), lustro (prosto za plecy) i kaskada (obok, co 0,09 s).</summary>
        static void Copies(HuCast cast, float scale)
        {
            int fan = (int)cast.P.Get("fan");
            float fanShare = cast.P.GetF("fan_share", 0.7f) * scale, fanStep = cast.P.GetF("fan_degrees", 30f);
            for (int i = 1; i <= fan; i++)
            {
                float angle = (i + 1) / 2 * fanStep * (i % 2 == 1 ? 1f : -1f);
                Execute(Turned(cast, angle, fanShare));
            }
            if (cast.P.On("mirror")) Execute(Turned(cast, 180f, cast.P.GetF("mirror_share", 0.8f) * scale));

            int cascade = (int)cast.P.Get("cascade");
            if (cascade <= 0) return;
            float share = cast.P.GetF("cascade_share", 0.6f) * scale;
            float spacing = cast.P.GetF("cascade_spacing", 2.6f) * cast.area;
            bool alongAim = cast.shape.Targeting != HuTargeting.Direction || cast.relayed;
            Vector3 side = new Vector3(cast.direction.z, 0f, -cast.direction.x);
            for (int i = 1; i <= cascade; i++)
            {
                float step = (i + 1) / 2 * spacing * (i % 2 == 1 ? 1f : -1f);
                Vector3 offset = alongAim ? cast.direction * step : side * step;
                var copy = cast.Variant(cast.origin + (alongAim && cast.shape.Targeting == HuTargeting.Point ? Vector3.zero : offset), cast.direction, cast.target + offset, share, true);
                HuWorld.After(0.09f * i, () => Execute(copy));
            }
        }

        static HuCast Turned(HuCast cast, float degrees, float share)
        {
            var rot = Quaternion.Euler(0, degrees, 0);
            return cast.Variant(cast.origin, rot * cast.direction, cast.origin + rot * (cast.target - cast.origin), share);
        }

        /// <summary>Narastanie: każde użycie w oknie od poprzedniego +krok mocy, do limitu; przerwa zeruje.</summary>
        static float Crescendo(PlayerCombat pc, SpellInstance skill, HuSkillShape shape)
        {
            float step = shape.Parameters.GetF("crescendo_step");
            if (step <= 0f) return 1f;
            float now = HuWorld.Clock;
            var (stacks, last) = crescendo.TryGetValue(skill, out var v) ? v : (0, float.NegativeInfinity);
            stacks = now - last <= shape.Parameters.GetF("crescendo_window", 4f) ? Mathf.Min(stacks + 1, (int)shape.Parameters.Get("crescendo_max", 5)) : 0;
            crescendo[skill] = (stacks, now);
            if (stacks > 0) CombatEvents.RaiseWorldText(pc.AimPoint + Vector3.up * 1.2f, $"×{stacks + 1}", HuCast.ColorFor(shape.Element, skill.definition.color));
            return 1f + step * stacks;
        }

        /// <summary>Co rzut daje poza sobą: barierę (<c>cast_barrier</c>) albo hart (<c>cast_reduction</c>).</summary>
        static void OnCastBoons(HuCast cast)
        {
            var pc = cast.caster;
            float barrier = cast.P.GetF("cast_barrier");
            if (barrier > 0f)
            {
                pc.RaiseBarrier(pc.Health.Max * barrier, cast.P.GetF("cast_barrier_seconds", 4f), HuCast.ColorFor(HuElement.Earth, cast.color));
                HellFx.Shockwave(pc.transform.position, HellFxElement.Earth, cast.color, 1.3f);
            }
            float reduction = cast.P.GetF("cast_reduction");
            if (reduction > 0f) pc.GrantReduction(reduction, cast.P.GetF("cast_reduction_seconds", 3f));
        }

        static void PlantTotem(HuCast cast, float range)
        {
            var pc = cast.caster;
            Vector3 at = cast.shape.Targeting == HuTargeting.Point ? cast.target : pc.transform.position + cast.direction * 1.6f;
            at = HuWorld.Clamp(pc.transform.position, at, 0.8f);
            totems.RemoveAll(t => !t.AnchorLive);
            int max = Mathf.Max(1, (int)cast.P.Get("max_totems", 1));
            while (totems.Count >= max) { totems[0].Stop(); totems.RemoveAt(0); }
            var totem = new HuTotem(cast, at, range);
            totems.Add(totem);
            HuWorld.Add(totem);
        }

        internal static void LayMine(HuMine mine, int max)
        {
            mines.RemoveAll(m => !m.AnchorLive);
            while (mines.Count >= max) { mines[0].Trigger(); mines.RemoveAt(0); }
            mines.Add(mine);
            HuWorld.Add(mine);
        }

        /// <summary>
        /// Wyzwalacz: zdarzenie walki (<c>dodge</c>, <c>hurt</c>, <c>kill</c>, <c>defend</c>) odpala każdy skill w ręce z
        /// <c>trigger_&lt;zdarzenie&gt;</c>, jeśli ma ładunek – w najbliższego demona. Najwyżej raz na 1,5 s, za ładunek,
        /// połowę many/wytrzymałości albo cały koszt w zdrowiu; to, co zabije, nie odpala dalszych wyzwalaczy.
        /// </summary>
        public static void OnTrigger(PlayerCombat pc, string kind, Vector3? at = null)
        {
            if (pc == null || pc.IsDead || pc.Run == null || pc.Build == null) return;
            string key = "trigger_" + kind;
            var armed = new List<SpellRuntime>();
            foreach (var s in pc.Build.skills) if (s != null && s.instance.seals != null) armed.Add(s);
            if (pc.Build.signature != null && pc.Build.signature.instance.seals != null) armed.Add(pc.Build.signature);
            foreach (var sk in armed)
            {
                var shape = sk.instance.seals.Shape;
                if (!shape.Parameters.On(key) || shape.HasComponent("Aura")) continue;
                float now = HuWorld.Clock;
                if (lastTriggered.TryGetValue(sk.instance, out float last) && now - last < TriggerCooldownSeconds) continue;
                if (!sk.instance.Ready) continue;
                if (!pc.HuPay(sk, TriggerCostShare)) continue;
                sk.instance.TryUse();
                lastTriggered[sk.instance] = now;
                float range = shape.Parameters.GetF(HuParams.Range, 8f);
                var prey = HuWorld.Nearest(pc.transform.position, Mathf.Max(range, 6f));
                Vector3 aim = prey != null ? HuWorld.FlatDir(pc.transform.position, prey.transform.position) : pc.transform.forward;
                Vector3 target = shape.Targeting == HuTargeting.Point ? at ?? (prey != null ? prey.transform.position : pc.transform.position + aim * range) : pc.transform.position + aim * range;
                var cast = new HuCast(pc, sk, pc.transform.position, aim, target, shape.Parameters.GetF("trigger_share", 1f), triggered: true);
                CombatEvents.RaiseWorldText(pc.AimPoint + Vector3.up * 1.4f, sk.Def.displayName, cast.color);
                pc.HuCastFx(sk, aim);
                OnCastBoons(cast);
                Fire(cast, aim, range);
            }
        }

        /// <summary>Przekaźnik: nova wybucha przy żywych kotwicach (najbliżej celownika) zamiast wokół bohatera i je odpala.</summary>
        static void Relay(HuCast cast, List<IHuAnchor> anchors, float scale)
        {
            float share = cast.P.GetF("relay_share", 1f);
            bool first = true;
            foreach (var a in anchors)
            {
                Vector3 at = a.AnchorPosition;
                var relayed = cast.Variant(at, cast.direction, at, first ? share : share * scale, true);
                first = false;
                FxLibrary.Lightning(cast.caster.AimPoint + Vector3.up * 0.3f, at + Vector3.up * 0.9f, cast.color);
                a.Trigger();
                Execute(relayed);
                ScheduleEcho(relayed, cast.direction, 0f, scale);
                Copies(relayed, scale);
            }
        }

        /// <summary>Echo: cały rzut jeszcze raz po chwili, z częścią mocy, tam, gdzie wtedy stoi bohater (albo w punkcie).</summary>
        static void ScheduleEcho(HuCast cast, Vector3 aim, float range, float scale)
        {
            float share = cast.P.GetF("echo_share") * scale;
            if (share <= 0f) return;
            HuWorld.After(cast.P.GetF("echo_delay", 0.45f), () =>
            {
                var pc = cast.caster;
                if (pc == null || pc.IsDead) return;
                Vector3 origin = cast.relayed ? cast.origin : pc.transform.position;
                Vector3 target = cast.relayed || cast.shape.Targeting == HuTargeting.Point ? cast.target : origin + aim * range;
                var echo = cast.Variant(origin, cast.direction, target, share);
                HellFx.Burst(origin, cast.family, 0.9f);
                Execute(echo);
            });
        }

        static void Execute(HuCast cast)
        {
            bool delivered = false;
            foreach (var name in cast.shape.Components)
            {
                var phase = PhaseOf(name);
                if (phase == HuPhase.Delivery) delivered = true;
                if (phase != HuPhase.Impact) RunComponent(name, cast, cast.shape.Targeting == HuTargeting.Self && !cast.relayed ? cast.origin : cast.target);
            }
            if (!delivered) cast.Impact(cast.shape.Targeting == HuTargeting.Self || cast.relayed ? cast.origin : cast.target);
        }

        // ------------------------------------------------------------------ Komponenty

        public static void RunComponent(string name, HuCast cast, Vector3 point)
        {
            switch (name.ToLowerInvariant())
            {
                case "projectile": Projectile(cast); break;
                case "explosion": Explosion(cast, point); break;
                case "zone": Zone(cast, point); break;
                case "rain": Rain(cast, point); break;
                case "barrage": Barrage(cast, point); break;
                case "chain": Chain(cast); break;
                case "nova": Nova(cast); break;
                case "shove": Shove(cast); break;
                case "wave": Wave(cast); break;
                case "vortex": Vortex(cast, point); break;
                case "line": Line(cast); break;
                case "cone": Cone(cast); break;
                case "trail": Trail(cast); break;
                case "dash": Dash(cast); break;
                case "spin": cast.caster.HuStartSpin(new HuSpin(cast)); break;
                case "barrier": HuWorld.Add(new HuBarrier(cast)); break;
                case "warcry": cast.caster.HuWarcry(cast); break;
                case "overload": cast.caster.HuOverload(cast); break;
                case "aura": break; // włączanie i gaszenie aury obsługuje PlayerCombat (przełącznik, nie rzut)
                case "heal": cast.caster.Health.Restore(cast.caster.Health.Max * cast.P.GetF("heal") * cast.power); break;
                case "undying": cast.caster.HuUndying(cast); break;
                case "summon": Summon(cast); break;
                case "fall": Fall(cast); break;
                case "mine": LayMine(new HuMine(cast, HuWorld.Clamp(cast.caster.transform.position, cast.target)), Mathf.Max(1, (int)cast.P.Get("max_mines", 3))); break;
                case "infuse": Infuse(cast, point); break;
                case "wall": Wall(cast, point); break;
                default: Debug.LogError($"[skill] {cast.Data.id}: nieznany komponent '{name}'"); break;
            }
        }

        /// <summary>Pocisk(i) wzdłuż celowania. Lądowanie – na ciele albo na końcu lotu – jest uderzeniem skilla.</summary>
        static void Projectile(HuCast cast)
        {
            var pc = cast.caster;
            bool explodes = cast.shape.HasComponent("Explosion");
            float contactShare = explodes ? 1f - Mathf.Clamp01(cast.P.GetF("explosion_share", 1f)) : 1f;
            bool burstAtEnd = cast.P.On("burst_at_end");
            bool pulses = cast.P.On("pulse_radius") || cast.P.On("trail_zone_interval");
            bool orbit = cast.P.On("orbit") && !cast.relayed;
            var motion = new HuShot.Motion
            {
                orbitCenter = orbit ? pc.transform : null,
                orbitRadius = orbit ? cast.Area("orbit_radius", 2.4f) : 0f,
                orbitSpeed = cast.P.GetF("orbit_speed", 200f) * Mathf.Deg2Rad,
                homingTurn = cast.P.GetF("homing") * Mathf.Deg2Rad,
                returnTo = cast.P.On("return_flight") && !cast.relayed ? pc.transform : null,
                returnSeconds = cast.P.GetF("lifetime", 1.5f) * 1.5f,
            };
            int count = Mathf.Max(1, (int)cast.P.Get("count", 1));
            float spread = cast.P.GetF("spread_degrees", count > 1 ? 30f : 0f);
            bool ring = spread >= 359.9f;
            var hit = cast.Hit(1f, projectile: true);
            // Strzały tylko z łuku łotrzyka (skill strzela od siebie); fizyczne pociski wojownika to wirujące ostrza; reszta – kule żywiołu.
            bool physical = !cast.shape.Tags.Contains("spell") && cast.shape.Element == HuElement.Physical && cast.Data.HasComponent("Projectile");
            var look = physical && cast.Data.classId == "rogue" && !cast.P.On("pulse_radius") ? HuShot.Look.Arrow
                : physical && cast.Data.classId == "warrior" && !cast.P.On("pulse_radius") ? HuShot.Look.Blade : HuShot.Look.Orb;
            float height = 0.9f;
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : ring ? i / (float)count : i / (float)(count - 1);
                float angle = (t - (ring ? 0f : 0.5f)) * spread;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * cast.direction;
                var m = motion;
                if (orbit) m.orbitAngle = Mathf.Atan2(dir.z, dir.x) + i * Mathf.PI * 2f / count;
                var shot = HuShot.Spawn(cast, cast.origin + Vector3.up * height + dir * 0.7f, dir, hit, cast.Length("speed", 9f) * cast.caster.ProjectileSpeedMult, cast.Length("projectile_radius", 0.4f),
                    cast.P.GetF("lifetime", 1.5f), (int)cast.P.Get("pierce"), (int)cast.P.Get("ricochet"), (int)cast.P.Get("splits"),
                    burstAtEnd ? 1f : contactShare, !burstAtEnd, cast.P.On("impact_every_hit"), m, look);
                if (shot != null && pulses) HuWorld.Add(new HuShotPulse(cast, shot));
            }
        }

        static void Explosion(HuCast cast, Vector3 point)
        {
            float r = cast.Area("explosion_radius");
            if (r <= 0f) return;
            HuWorld.Explode(cast.caster, point, r, cast.Hit(cast.P.GetF("explosion_share", 1f)), cast.color);
            if (cast.family == HellFxElement.Earth) HellFx.SlamDust(point, r * 0.7f);
        }

        /// <summary>Strefa w punkcie uderzenia (nova kładzie własną).</summary>
        static void Zone(HuCast cast, Vector3 point)
        {
            float seconds = cast.P.GetF("zone_seconds");
            if (seconds <= 0f || cast.shape.HasComponent("Nova")) return;
            float tick = cast.P.GetF("zone_interval", 0.5f);
            SpawnZone(cast, point, cast.Area("zone_radius", 2f), seconds, tick, cast.P.GetF("zone_share", 0.25f));
        }

        /// <summary>Strefa skilla: co tyknięcie <c>share × tick</c> trafienia (czyli <c>share</c> na sekundę), z limitem żywych stref.</summary>
        public static void SpawnZone(HuCast cast, Vector3 point, float radius, float seconds, float tick, float share)
        {
            if (HuWorld.LiveZones >= HuWorld.MaxZones) return;
            tick = Mathf.Max(0.1f, tick);
            var h = cast.Hit(share * tick, plain: false);
            h.knockback = 0f; h.poiseDamage *= 0.1f; h.explosionRadius = 0f;
            // Strefa fizyczna (strzały, ciernie, sidła): ciemny ślad na ziemi zamiast jasnej tarczy.
            Color zc = cast.shape.Element == HuElement.Physical && !cast.P.On("ice") ? new Color(0.32f, 0.26f, 0.2f) : cast.color;
            DamageZone.Spawn(HuWorld.Ground(point), radius, seconds, tick, h, Faction.Player, cast.caster, zc);
            HuWorld.LiveZones++;
            HuWorld.After(seconds, () => HuWorld.LiveZones = Mathf.Max(0, HuWorld.LiveZones - 1));
        }

        /// <summary>Strzały spadające na strefę – sam wygląd; obrażenia daje komponent Zone.</summary>
        static void Rain(HuCast cast, Vector3 point)
        {
            float seconds = cast.P.GetF("zone_seconds");
            if (seconds <= 0f || !FxLibrary.Enabled) return;
            float r = cast.Area("zone_radius", 2f);
            int n = Mathf.Clamp((int)(seconds * 14f), 8, 50);
            for (int i = 0; i < n; i++)
            {
                float at = i * seconds / n;
                HuWorld.After(at, () =>
                {
                    Vector2 c = UnityEngine.Random.insideUnitCircle * r;
                    HuFallingArrow.Drop(HuWorld.Ground(point + new Vector3(c.x, 0, c.y)), cast.color);
                });
            }
            Sfx.Play(SoundId.BowRelease, point, 0.7f, 0.8f);
        }

        /// <summary>
        /// Uderzenia z nieba we wskazany punkt: jeden wielki cios albo burza małych rozsypanych wokół. Każde zapowiedziane
        /// na ziemi i spadające tak, by wylądować w chwili wybuchu.
        /// </summary>
        static void Barrage(HuCast cast, Vector3 point)
        {
            var pc = cast.caster;
            int strikes = Mathf.Max(1, (int)cast.P.Get("strikes", 1));
            float r = cast.Area("strike_radius", 2f), spread = cast.Area("spread", 0f);
            float delay = cast.P.GetF("strike_delay", 0.8f), interval = cast.P.GetF("strike_interval", 0.15f);
            var hit = cast.Hit();
            bool lightning = cast.P.On("lightning");
            for (int i = 0; i < strikes; i++)
            {
                Vector3 c = point;
                if (spread > 0f && (strikes > 1 || i > 0))
                {
                    float a = UnityEngine.Random.value * Mathf.PI * 2f, reach = spread * Mathf.Sqrt(UnityEngine.Random.value);
                    c += new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * reach;
                }
                c = HuWorld.Clamp(point, c, 0.5f);
                float lands = delay + i * interval;
                var mark = VisualFx.GroundMarker(c, r, cast.color, cast.family);
                HuWorld.Add(new HuTelegraphRuntime(mark, lands));
                Vector3 at = c;
                HuWorld.After(lands, () =>
                {
                    if (pc == null || pc.IsDead) return;
                    if (lightning) LightningBolt.Strike(at + Vector3.up * 12f, at, cast.color);
                    HuWorld.Explode(pc, at, r, hit, cast.color);
                    if (cast.family == HellFxElement.Ice) IceCrystals.Ring(at, r * 0.8f, 5, 0.5f);
                    else if (cast.family == HellFxElement.Fire || cast.family == HellFxElement.Earth) HellFx.SlamDust(at, r * 0.6f);
                });
            }
            float after = cast.P.GetF("aftershock_share");
            if (after > 0f)
            {
                float at = delay + (strikes - 1) * interval + cast.P.GetF("aftershock_delay", 0.45f);
                var h = hit; h.physical *= after; h.magic *= after;
                HuWorld.ScheduleExplosion(pc, at, point, r * 1.7f, h, cast.color);
            }
        }

        /// <summary>Piorun skacze z ciała do ciała: od celu najbliżej celowania do kolejnych nietrafionych, słabnąc o <c>chain_falloff</c>.</summary>
        static void Chain(HuCast cast)
        {
            var pc = cast.caster;
            Vector3 hand = cast.origin + Vector3.up * 1.3f + cast.direction * 0.5f;
            float range = cast.Length("range", 9f), jump = cast.Area("chain_range", 4.5f);
            int chains = Mathf.Max(0, (int)cast.P.Get("chains", 3));
            float falloff = Mathf.Clamp(cast.P.GetF("chain_falloff", 0.15f), 0f, 0.9f);
            var hit = cast.Hit(1f, projectile: true);
            EnemyBrain current = cast.copy ? HuWorld.AimTarget(cast.origin, cast.direction, range, 50f) : pc.HuAimTarget(range, 50f, cast.origin, cast.direction);
            if (current == null)
            {
                LightningBolt.Strike(hand, HuWorld.Ground(cast.origin + cast.direction * Mathf.Min(range, 5f)), cast.color);
                return;
            }
            var struck = new HashSet<EnemyBrain>();
            Vector3 from = hand; float share = 1f, delay = 0f;
            for (int link = 0; link <= chains && current != null; link++)
            {
                var target = current;
                Vector3 to = target.AimPoint, src = from;
                var h = hit; h.physical *= share; h.magic *= share;
                float d = delay;
                HuWorld.After(d, () =>
                {
                    if (target == null || target.IsDead || pc == null) return;
                    LightningBolt.Strike(src, target.AimPoint, cast.color);
                    h.sourcePosition = src;
                    HitQuery.Apply(pc, target, h, false);
                });
                struck.Add(target);
                from = to; share *= 1f - falloff; delay += 0.07f;
                current = HuWorld.Nearest(target.transform.position, jump, e => !struck.Contains(e));
            }
            Sfx.Play(SoundId.LightningZap, pc.AimPoint, 1f, 1.2f);
        }

        /// <summary>Uderzenie obszarowe wokół bohatera po krótkiej zapowiedzi, w kilku impulsach; poziom III: strefa albo wir.</summary>
        static void Nova(HuCast cast)
        {
            var pc = cast.caster;
            float radius = cast.Area("radius", 3f);
            bool follow = cast.P.On("follow") && !cast.relayed;
            int pulses = Mathf.Max(1, Mathf.RoundToInt(cast.P.GetF("pulses", 1f)));
            float telegraph = cast.P.GetF("telegraph", 0.3f), interval = cast.P.GetF("pulse_interval", 0.45f);
            var hit = cast.Hit();
            float lastBlast = telegraph;
            if (telegraph > 0.05f && FxLibrary.Enabled)
            {
                var decal = FxLibrary.Decal(HuWorld.Ground(cast.origin), HellVfxMaterials.Ground(cast.family, true), cast.color, radius * 2, radius * 2, telegraph + 0.05f);
                decal.Progress = 0;
                decal.gameObject.AddComponent<HellTelegraph>().seconds = telegraph;
                if (follow) decal.Follow(pc.transform);
            }
            for (int i = 0; i < pulses; i++)
            {
                lastBlast = telegraph + i * interval;
                HuWorld.ScheduleExplosion(pc, lastBlast, HuWorld.Ground(cast.origin), radius, hit, cast.color, follow ? pc.transform : null);
            }
            Vector3 fixedAt = cast.origin;
            float zoneSeconds = cast.P.GetF("zone_seconds");
            if (zoneSeconds > 0f)
            {
                float tick = cast.P.GetF("zone_interval", 0.5f), zr = cast.Area("zone_radius", radius * 0.8f), zs = cast.P.GetF("zone_share", 0.2f);
                HuWorld.After(lastBlast, () => SpawnZone(cast, follow && pc != null ? pc.transform.position : fixedAt, zr, zoneSeconds, tick, zs));
            }
            float vortexSeconds = cast.P.GetF("vortex_seconds");
            if (vortexSeconds > 0f)
            {
                float vr = cast.Area("vortex_radius", 2.5f), vi = cast.P.GetF("vortex_interval", 0.5f), vp = cast.Length("vortex_pull", 3f);
                var vh = cast.Hit(cast.P.GetF("vortex_share", 0.25f), plain: true);
                HuWorld.After(lastBlast, () => HuWorld.Add(new HuVortex(cast, follow && pc != null ? pc.transform.position : fixedAt, vr, vortexSeconds, vi, vp, vh, 0f, 0f)));
            }
        }

        /// <summary>
        /// Odepchnięcie: każdy demon w zasięgu kroku (do krawędzi ciała) leci prosto od bohatera i leży na ziemi. Bossów nie rusza
        /// ani nie trafia. Wersja z obrotem trafia każdego w jego chwili obrotu.
        /// </summary>
        static void Shove(HuCast cast)
        {
            var pc = cast.caster;
            Vector3 me = pc.transform.position;
            float reach = cast.P.GetF("shove_radius", 1f), distance = cast.P.GetF("shove_distance", 2f), down = cast.P.GetF("down_seconds", 1.2f);
            bool spin = cast.P.On("spin");
            float turn = Mathf.Clamp(cast.P.GetF("spin_seconds", 0.3f), 0.1f, 1f);
            float startYaw = Mathf.Atan2(cast.direction.x, cast.direction.z);
            var hit = cast.Hit(); hit.knockback = 0f; hit.knockdown = 0f;
            int thrown = 0;
            foreach (var e in EnemyBrain.All.ToArray())
            {
                if (e == null || e.IsDead || e.Def.isBoss || HuWorld.Flat(e.transform.position, me) - 0.6f > reach) continue;
                Vector3 away = HuWorld.FlatDir(me, e.transform.position);
                thrown++;
                if (!spin) { Throw(pc, e, hit, me, away, distance, down); continue; }
                float angle = Mathf.Repeat(Mathf.Atan2(away.x, away.z) - startYaw, Mathf.PI * 2f);
                float at = 0.07f + angle / (Mathf.PI * 2f) * turn;
                Vector3 flung = (away + new Vector3(away.z, 0, -away.x) * 0.45f).normalized;
                var target = e;
                HuWorld.After(at, () => { if (target != null && !target.IsDead) { Throw(pc, target, hit, me, flung, distance, down); FxLibrary.Sparks(target.AimPoint, flung, cast.color); } });
            }
            if (spin) { HuWorld.After(0.07f + turn * 0.5f, () => HellFx.Burst(me, cast.family, reach + 0.9f)); pc.HuSwing(true); }
            else { HellFx.SlamDust(me, Mathf.Max(1.7f, reach + 0.7f)); ShockRing.Spawn(HuWorld.Ground(me), reach + distance, 0.6f, 0.5f); Sfx.Play(SoundId.Quake, me, 0.8f, 0.75f); }
        }

        static void Throw(PlayerCombat pc, EnemyBrain e, HitData hit, Vector3 from, Vector3 away, float distance, float down)
        {
            hit.sourcePosition = from;
            HitQuery.Apply(pc, e, hit, false);
            e.Knockback(away, distance, 0.25f);
            e.HuThrowDown(down);
        }

        static void Wave(HuCast cast)
        {
            Start(cast, 1f);
            float extra = cast.P.GetF("extra_wave_share");
            if (extra > 0f) HuWorld.After(cast.P.GetF("extra_wave_delay", 0.25f), () => Start(cast, extra));

            void Start(HuCast c, float share)
            {
                float range = c.P.Has("wave_range") ? c.Length("wave_range") : c.Length("range", 6f);
                float start = c.Length("wave_start");
                HuWorld.Add(new HuWave(c, HuWorld.Ground(c.origin), c.direction, start, start + range, c.Area("width", 3f) * 0.5f, c.Length("speed", 10f),
                    c.Hit(), share, c.P.GetF("return_share"), c.P.GetF("spread_degrees")));
            }
        }

        /// <summary>Ściana ognia (Turris): w poprzek celowania w punkcie trafienia albo pierścień wokół niego (<c>wall_ring</c>).</summary>
        static void Wall(HuCast cast, Vector3 point)
        {
            if (HuWorld.LiveZones >= HuWorld.MaxZones) return;
            float ring = cast.P.On("wall_ring") ? cast.Area("wall_ring_radius", 4f) : 0f;
            HuWorld.Add(new HuWall(cast, point, cast.direction, cast.Area("wall_length", 7f), cast.Area("wall_width", 1.2f), cast.P.GetF("wall_seconds", 4f),
                cast.P.GetF("wall_interval", 0.5f), cast.P.GetF("wall_share", 1f), cast.Length("wall_speed"), ring));
        }

        static void Vortex(HuCast cast, Vector3 point) =>
            HuWorld.Add(new HuVortex(cast, point, cast.Area("radius", 2.5f), cast.P.GetF("duration_seconds", 2.5f), cast.P.GetF("hit_interval", 0.4f),
                cast.Length("pull", 3f), cast.Hit(1f, plain: true), cast.P.GetF("burst_share"), cast.P.GetF("burst_knockback"),
                cast.shape.Targeting == HuTargeting.Self && !cast.relayed ? cast.caster.transform : null));

        static void Line(HuCast cast)
        {
            Vector3 from = cast.origin + cast.direction * 0.6f;
            HuWorld.Add(new HuLine(cast, from, from + cast.direction * cast.Length("length", 8f), cast.Area("width", 1.4f) * 0.5f,
                cast.P.GetF("delay_seconds", 0.45f), cast.Hit(), cast.P.GetF("second_delay"), cast.P.GetF("second_share")));
        }

        /// <summary>Ciężkie cięcie w stożku przed bohaterem.</summary>
        static void Cone(HuCast cast)
        {
            var pc = cast.caster;
            float range = cast.Length("range", 3f) * cast.area, arc = cast.P.GetF("arc_degrees", 120f);
            var hit = cast.Hit();
            Vector3 fwd = cast.direction;
            // Powalenie (down_seconds) nakłada samo trafienie (HitData.knockdown); bossów nie dotyczy.
            int hits = 0;
            HitQuery.Sphere(cast.origin + Vector3.up, range, Faction.Player, new HitTracker(), t =>
            {
                Vector3 to = t.Transform.position - cast.origin; to.y = 0;
                if (to.sqrMagnitude > 0.04f && Vector3.Angle(fwd, to) > arc * 0.5f) return;
                var h = hit; h.sourcePosition = cast.origin;
                HitQuery.Apply(pc, t, h, true); hits++;
            });
            HellFx.Cone(cast.origin + Vector3.up * 1.1f + fwd * 0.4f, fwd, cast.family, range, arc);
            if (!cast.copy) pc.HuSwing(true);
        }

        static void Trail(HuCast cast)
        {
            float seconds = cast.P.GetF("trail_seconds");
            if (seconds <= 0f || cast.copy) return;
            var h = cast.Hit(cast.P.GetF("trail_share", 0.3f), plain: true);
            h.statuses = null;
            HuWorld.Add(new HuTrail(cast, cast.caster.transform, cast.P.GetF("dash_seconds", 0.2f) + 0.05f, seconds, cast.Area("trail_width", 1.2f) * 0.5f,
                cast.P.GetF("hit_interval", 0.4f), h));
        }

        static void Dash(HuCast cast)
        {
            bool impact = cast.shape.HasComponent("Explosion") || cast.shape.HasComponent("Zone");
            if (cast.copy)
            {
                // Kopia nie przenosi bohatera: uderzenie tam, dokąd doskok by doniósł.
                if (impact) cast.Impact(HuWorld.Clamp(cast.origin, cast.origin + cast.direction * cast.Length("distance", 4f)));
                return;
            }
            cast.caster.HuStartDash(new HuDash(cast, impact));
        }

        static void Summon(HuCast cast)
        {
            int kind = Mathf.Clamp((int)cast.P.Get("servant"), 0, 2);
            var hit = cast.Hit(1f, projectile: kind == 0);
            int count = Mathf.Max(1, Mathf.RoundToInt(cast.P.GetF("count", 1f)));
            for (int i = 0; i < count; i++)
                Servant.Spawn(cast.caster, (ServantKind)kind, cast.P.GetF("duration_seconds", 12f), cast.P.GetF("attack_interval", 1f), () => hit, cast.color);
            HellFx.Shockwave(cast.caster.transform.position, cast.family, cast.color, 2f);
        }

        /// <summary>Kometa: skill spada z nieba we wskazany punkt; lądowanie jest uderzeniem.</summary>
        static void Fall(HuCast cast)
        {
            var pc = cast.caster;
            Vector3 target = HuWorld.Clamp(pc.transform.position, cast.target, 0.5f);
            float fall = Mathf.Max(0.2f, cast.P.GetF("fall_seconds", 0.8f));
            float r = cast.Area("explosion_radius", 2f);
            var mark = VisualFx.GroundMarker(target, r, cast.color, cast.family);
            HuWorld.Add(new HuTelegraphRuntime(mark, fall));
            if (mark != null) FxLibrary.Gather(mark.transform, cast.color, fall);
            HuWorld.After(fall, () =>
            {
                if (pc == null || pc.IsDead) return;
                if (cast.family == HellFxElement.Air) LightningBolt.Strike(target + Vector3.up * 12f, target, cast.color);
                else HellFx.Burst(target + Vector3.up * 0.5f, cast.family, r * 0.6f);
                cast.Impact(target);
            });
        }

        /// <summary>Krąg żaru: strefa jest też polem – każdy pocisk skilla bohatera przez nie przelatujący dostaje część obrażeń i efekty.</summary>
        static void Infuse(HuCast cast, Vector3 point)
        {
            float r = cast.Area("zone_radius", 2.5f), seconds = cast.P.GetF("zone_seconds", 4f);
            var h = cast.Hit(cast.P.GetF("infuse_share", 0.4f), plain: true);
            HuWorld.AddInfusion(HuWorld.Ground(point), r, seconds, h.physical, h.magic, h.element, h.statuses, cast.color);
            if (FxLibrary.Enabled) HuWorld.Add(new HuTelegraphRuntime(VisualFx.Ring(HuWorld.Ground(point), r, cast.color, seconds), seconds));
        }
    }
}
