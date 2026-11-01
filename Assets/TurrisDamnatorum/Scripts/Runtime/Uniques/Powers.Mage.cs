using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Wspólne testy rzutów HU dla mocy magów.</summary>
    static class CastKinds
    {
        public static bool Manual(HuCast c) => c != null && !c.copy && !c.relayed && !c.triggered;
        public static bool IsNova(HuCast c) => c.shape.HasComponent("Nova");
        public static bool Offensive(SpellRuntime sk) => sk != null && sk.Def.IsSpell && !sk.Def.IsAura && sk.instance.seals != null
            && !sk.instance.seals.Shape.HasComponent("Summon") && !sk.instance.seals.Shape.HasComponent("Heal") && !sk.instance.seals.Shape.HasComponent("Barrier");
        /// <summary>Punkt działania rzutu (cel, samo miejsce albo przed postacią).</summary>
        public static Vector3 Point(HuCast c, float range)
        {
            if (c.shape.Targeting == HuTargeting.Point) return HuWorld.Ground(c.target);
            if (c.shape.Targeting == HuTargeting.Self) return HuWorld.Ground(c.origin);
            return HuWorld.Clamp(c.origin, c.origin + c.direction * Mathf.Min(range, 8f));
        }
    }

    // ====================================================================== M01 Weksel lichwiarza

    /// <summary>Ręczne trafienia bronią spłacają dług arkanów (przelicznik, limit na akcję); regeneracja many podczas długu stoi.</summary>
    public sealed class MoneylendersNotePower : UniquePower
    {
        readonly Dictionary<int, float> repaidPerOrigin = new Dictionary<int, float>();
        public override bool BlocksManaRegen => Run != null && Run.manaDebt > 0f;

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (Run == null || Run.manaDebt <= 0f || !hit.basicAttack || hit.featureEffect || result.healthDamage <= 0f) return;
            repaidPerOrigin.TryGetValue(hit.originId, out float done);
            float pay = Mathf.Min(result.healthDamage * P("repay_ratio"), P("repay_cap") - done, Run.manaDebt);
            if (pay <= 0f) { Count("spłata ponad limit akcji"); return; }
            Run.manaDebt -= pay; // spłata wprost z długu – bez tworzenia many
            if (Run.manaDebt < 0.001f) Run.manaDebt = 0f;
            repaidPerOrigin[hit.originId] = done + pay;
            if (repaidPerOrigin.Count > 32) repaidPerOrigin.Clear();
            Count("dług spłacony ciosami", pay);
        }

        public override void Death() { repaidPerOrigin.Clear(); }
    }

    // ====================================================================== M02 Szata zamkniętego źródła

    /// <summary>Mana wydana Płaszczem many na ochronę zostawia runy; następna ręczna nova wydaje je na mocniejsze wyładowanie (jedna konsumpcja).</summary>
    public sealed class SealedSourcePower : UniquePower
    {
        float pool; int runes;
        public int Runes => runes;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "cloak.paid" || amount <= 0f) return false;
            pool += amount;
            float per = Mathf.Max(1f, P("rune_mana"));
            while (pool >= per && runes < PI("max_runes")) { pool -= per; runes++; Count("runy"); }
            if (runes >= PI("max_runes")) pool = Mathf.Min(pool, per);
            return false;
        }

        public override HuCast ReplaceHuCast(HuCast cast)
        {
            if (runes <= 0 || !CastKinds.Manual(cast) || !CastKinds.IsNova(cast)) return null;
            float k = 1f + runes * P("rune_share");
            Count("runy wydane", runes);
            CombatEvents.RaiseWorldText(pc.AimPoint + Vector3.up, $"RUNY ×{runes}", new Color(0.5f, 0.7f, 1f));
            runes = 0;
            return new HuCast(pc, cast.spell, cast.origin, cast.direction, cast.target, cast.power * k);
        }

        public override void Death() { runes = 0; pool = 0f; }
        public override string Hud => runes > 0 ? $"runy źródła: {runes}" : null;
    }

    // ====================================================================== M03 Grymuar uśpionych kręgów

    /// <summary>Mina z novy: uzbraja się, wybucha całą novą (z formami) po wejściu wroga; przekaźnik może ją odpalić.</summary>
    public sealed class SleepingCircleMine : IHuRuntime, IUniqueMine
    {
        readonly HuCast cast; Vector3 at; readonly float arm, life, trigger; float t; bool done;
        readonly GameObject ring;
        public SleepingCircleMine(HuCast cast, Vector3 at, float arm, float life, float trigger)
        {
            this.cast = cast; this.at = HuWorld.Ground(at); this.arm = arm; this.life = life; this.trigger = trigger;
            if (FxLibrary.Enabled) ring = VisualFx.GroundMarker(this.at, trigger, cast.color, cast.family);
            HuWorld.RegisterAnchor(this);
            UniqueMines.Register(this);
        }
        public PlayerCombat Owner => cast.caster;
        public bool Armed => t >= arm;
        public float Remaining => life - t;
        public Vector3 AnchorPosition => at;
        public bool AnchorLive => !done;
        public void MoveTo(Vector3 p) { at = HuWorld.Ground(p); if (ring != null) ring.transform.position = at; }
        public bool Tick(float dt)
        {
            if (done) return false;
            t += dt;
            if (ring != null) { var d = ring.GetComponent<FxDecal>(); if (d != null) d.Progress = Armed ? 0.5f + 0.5f * Mathf.Sin(t * 7f) : 0.15f; }
            if (t >= life) { done = true; Free(); cast.caster?.UniqueTelemetry.Add("M03: miny wygasłe"); return false; } // bez darmowego wybuchu
            if (Armed && !UniqueMines.Held(this) && HuWorld.Nearest(at, trigger) != null) { Trigger(); return false; }
            return true;
        }
        public void Trigger()
        {
            if (done || cast.caster == null || cast.caster.IsDead) { done = true; Free(); return; }
            done = true; Free();
            var blast = cast.Variant(at, cast.direction, at, 1f, true);
            HuSkillCaster.Fire(blast, cast.direction, 0f);
            cast.caster.UniqueTelemetry.Add("M03: miny odpalone");
        }
        public void Stop() { done = true; Free(); }
        void Free() { if (ring != null) Object.Destroy(ring); }
    }

    public sealed class SleepingCirclesPower : UniquePower
    {
        readonly List<SleepingCircleMine> mines = new List<SleepingCircleMine>();
        public int Mines { get { mines.RemoveAll(m => !m.AnchorLive); return mines.Count; } }

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || !CastKinds.IsNova(cast)) return false;
            var e = pc.HuAimTarget(P("place_range"), 60f, pc.transform.position, aim);
            Vector3 at = e != null ? e.transform.position : pc.transform.position + aim * Mathf.Min(5f, P("place_range"));
            at = HuWorld.Clamp(pc.transform.position, at);
            mines.RemoveAll(m => !m.AnchorLive);
            while (mines.Count >= PI("max_mines")) { mines[0].Stop(); mines.RemoveAt(0); Count("najstarsza mina zgaszona"); }
            var mine = new SleepingCircleMine(cast, at, P("arm_seconds"), P("mine_seconds"), P("trigger_radius"));
            mines.Add(mine);
            HuWorld.Add(mine);
            Count("miny");
            return true;
        }

        public override void Death() { foreach (var m in mines) m.Stop(); mines.Clear(); }
        public override string Hud => Mines > 0 ? $"uśpione kręgi: {Mines}" : null;
    }

    // ====================================================================== M04 Rękawice związanych zaklęć

    /// <summary>Dwa różne czary zostawiają punkty runiczne (wstrzymując część mocy); trzeci inny domyka odcinek, który rani za wstrzymany budżet.</summary>
    public sealed class BoundSpellsPower : UniquePower
    {
        sealed class Rune { public string spell; public Vector3 at; public HitData held; public float until; public GameObject mark; }
        readonly List<Rune> runes = new List<Rune>();
        public int Points => runes.Count;

        public override HuCast ReplaceHuCast(HuCast cast)
        {
            if (!CastKinds.Manual(cast) || !CastKinds.Offensive(cast.spell)) return null;
            runes.RemoveAll(r => { bool old = pc.Clock > r.until; if (old && r.mark != null) Object.Destroy(r.mark); return old; });
            string id = cast.Def.id;
            if (runes.Count == 2)
            {
                if (runes[0].spell == id || runes[1].spell == id) { Count("zła kolejność"); Clear(); return null; }
                Release(cast);
                return null;
            }
            if (runes.Count == 1 && runes[0].spell == id) { Count("zła kolejność"); Clear(); return null; }
            float hold = P("hold_share");
            var held = cast.Hit(hold);
            var r = new Rune { spell = id, at = CastKinds.Point(cast, cast.P.GetF(HuParams.Range, 8f)), held = held, until = pc.Clock + P("link_seconds") };
            if (FxLibrary.Enabled) r.mark = VisualFx.GroundMarker(r.at, 0.8f, cast.color, cast.family);
            runes.Add(r);
            Count("punkty runiczne");
            return new HuCast(pc, cast.spell, cast.origin, cast.direction, cast.target, cast.power * (1f - hold));
        }

        void Release(HuCast third)
        {
            var a = runes[0]; var b = runes[1];
            var h = a.held;
            h.physical += b.held.physical; h.magic += b.held.magic;
            float m = P("line_mult", 1f);
            h.physical *= m; h.magic *= m;
            var el = third.Hit(0f).element;
            if (el != Element.None) { h.magic += h.physical; h.physical = 0f; h.element = el; }
            h.uniqueEffect = true; h.featureEffect = true; h.attackName = "Związane zaklęcia"; h.explosionRadius = 0f;
            int n = HuWorld.HitBand(pc, a.at, b.at, P("line_width") * 0.5f, h, new HitTracker());
            LightningBolt.Strike(a.at + Vector3.up, b.at + Vector3.up, third.color);
            HellFx.Burst(a.at, third.family, 1f); HellFx.Burst(b.at, third.family, 1f);
            Count("połączenia"); Count("trafieni linią", n);
            Clear();
        }

        void Clear() { foreach (var r in runes) if (r.mark != null) Object.Destroy(r.mark); runes.Clear(); }
        public override void Death() { Clear(); }
        public override string Hud => runes.Count > 0 ? $"punkty runiczne: {runes.Count}/2" : null;
    }

    // ====================================================================== M05 Buty spóźnionej obecności

    /// <summary>Po uniku ślad źródła w miejscu startu; następny ręczny czar ofensywny wychodzi ze śladu.</summary>
    public sealed class LatePresencePower : UniquePower
    {
        Vector3 trace; float until = -1f; GameObject mark;
        public bool Armed => until > pc.Clock;

        public override void DodgeStarted(Vector3 from, Vector3 dir)
        {
            trace = from; until = pc.Clock + P("trace_seconds");
            if (mark != null) Object.Destroy(mark);
            if (FxLibrary.Enabled) { var d = FxLibrary.Decal(HuWorld.Ground(from), HellVfxMaterials.Ground(HellFxElement.Soul, false), new Color(0.6f, 0.5f, 1f), 1.2f, 1.2f, P("trace_seconds")); mark = d != null ? d.gameObject : null; }
            Count("ślady");
        }

        public override void ModifyHuCast(SpellRuntime sk, ref Vector3 origin, ref Vector3 aim, ref Vector3 target)
        {
            if (!Armed || !CastKinds.Offensive(sk)) return;
            var e = pc.LockTarget != null && !pc.LockTarget.IsDead ? pc.LockTarget : pc.AssistTarget;
            Vector3 dir = e != null ? HuWorld.FlatDir(trace, e.transform.position) : aim;
            if (sk.instance.seals.Shape.Targeting != HuTargeting.Point) target += trace - origin;
            origin = trace; aim = dir;
            until = -1f;
            if (mark != null) Object.Destroy(mark);
            Count("czary ze śladu");
        }

        public override void Death() { until = -1f; if (mark != null) Object.Destroy(mark); }
        public override string Hud => Armed ? $"ślad obecności {until - pc.Clock:0.0} s" : null;
    }

    // ====================================================================== F01 Miech pożeracza stosów

    /// <summary>Lecąca Piekielna kula pożera własne strefy ognia (pozostały budżet × udział) i dokłada je do końcowego wybuchu.</summary>
    public sealed class PyreDevourerPower : UniquePower
    {
        readonly Dictionary<HuShot, int> eaten = new Dictionary<HuShot, int>();
        protected override string EquipmentDormant() => NeedSkill("hu_infernal_orb");

        public override void Tick(float dt)
        {
            if (!Active) return;
            foreach (var shot in HuShot.Live.ToArray())
            {
                if (shot == null || !shot.IsActive || shot.SourceCast == null || shot.SourceCast.caster != pc || shot.SourceCast.Def.id != "hu_infernal_orb") continue;
                eaten.TryGetValue(shot, out int n);
                if (n >= PI("max_fields")) continue;
                foreach (var z in DamageZone.Active.ToArray())
                {
                    if (z == null || !ReferenceEquals(z.Owner, pc) || z.ZoneHit.element != Element.Fire || z.Remaining <= 0.05f) continue;
                    if (HuWorld.Flat(z.transform.position, shot.transform.position) > z.Radius + P("eat_radius")) continue;
                    float budget = z.RemainingBudget * P("field_share");
                    shot.ImpactBonus += budget;
                    HuWorld.RemoveInfusionsAt(z.transform.position, z.Radius); // zjedzony Krąg żaru nie zaklina dalej
                    z.Expire();
                    n++;
                    HellFx.Burst(shot.transform.position, HellFxElement.Fire, 1f);
                    Count("pożarte pola"); Count("budżet pól", budget);
                    if (n >= PI("max_fields")) break;
                }
                eaten[shot] = n;
            }
            if (eaten.Count > 16) foreach (var k in eaten.Keys.Where(s => s == null || !s.IsActive).ToList()) eaten.Remove(k);
        }

        public override void Death() { eaten.Clear(); }
    }

    // ====================================================================== F02 Mankiety popielnych marionetek

    /// <summary>Ofiara z własnymi rodzinami podpalenia staje się płomieniem przenoszącym pozostałe rany (i odroczony budżet) na nowy cel.</summary>
    public sealed class AshPuppetsPower : UniquePower
    {
        int alive;
        protected override void OnBind() { EnemyBrain.Dying += OnDied; }
        public override void Dispose() { EnemyBrain.Dying -= OnDied; base.Dispose(); }

        public override void OutgoingLate(ref HitData hit)
        {
            if (hit.statuses == null || !hit.statuses.Exists(s => s.kind == StatusKind.Burn)) return;
            hit.mods.burnDurationBonus += pc.Config.balance.burnDuration * (P("burn_mult") - 1f);
        }

        void OnDied(EnemyBrain e)
        {
            if (!Active || alive >= PI("max_puppets")) return;
            var burns = e.Status.TakeBurns();
            float deferred = e.Status.TakeDeferredBurn();
            float budget = 0f; foreach (var b in burns) budget += b.dps * b.remaining;
            if (budget + deferred <= 1f) return; // pusty budżet nie tworzy marionetki
            Vector3 pos = e.transform.position;
            alive++;
            GameObject flame = null;
            if (Application.isPlaying) { flame = new GameObject("PopielnaMarionetka"); flame.transform.position = pos + Vector3.up; HellFx.Ambient(flame.transform, Vector3.zero, HellFxElement.Fire, 0.4f); }
            var tf = pc.FeatureTuning;
            int families = tf != null ? Mathf.Max(1, tf.greatFireFamilies) : 3;
            float t = 0f, speed = P("puppet_speed");
            EnemyBrain prey = null;
            Count("marionetki"); Count("budżet niesiony", budget + deferred);
            HuWorld.Add(new UniqueRuntime(dt =>
            {
                t += dt;
                if (pc == null || pc.IsDead || t > 6f) return false;
                if (prey == null || prey.IsDead) prey = HuWorld.Nearest(pos, P("seek_range"), x => x != e);
                if (prey == null) return t < 1.5f;
                Vector3 to = prey.transform.position - pos; to.y = 0f;
                float d = to.magnitude;
                if (d > 0.6f) { pos += to / d * Mathf.Min(d, speed * dt); if (flame != null) flame.transform.position = pos + Vector3.up; return true; }
                foreach (var b in burns) prey.Status.AddBurnFamily(b.key, b.dps, b.remaining - t, families); // ta sama rodzina, pozostały czas
                if (deferred > 0f) prey.Status.AddDeferredBurn(deferred, 4f);
                HellFx.Burst(prey.AimPoint, HellFxElement.Fire, 0.9f);
                Count("rany przeniesione");
                return false;
            }, () => { alive = Mathf.Max(0, alive - 1); if (flame != null) Object.Destroy(flame); }));
        }

        public override void Death() { }
        public override string Hud => alive > 0 ? $"marionetki: {alive}" : null;
    }

    // ====================================================================== F03 Młot pożyczonego słońca

    /// <summary>Mocny cios młotem zabiera Żar i tworzy palenisko na ofierze; kolejne trafienia bronią w nią wypłacają porcje iskier.</summary>
    public sealed class BorrowedSunPower : UniquePower
    {
        float pending; int pendingOrigin = -1;
        EnemyBrain hearth; float budget, until;
        public float Hearth => hearth != null ? budget : 0f;
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: młot musi być w głównej ręce" : null;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "furnace.attack" || !(data is ActionType t) || t != ActionType.HeavyAttack) return false;
            pending = amount * P("ember_mult");
            Count("Żar do paleniska", pending);
            return true;
        }

        public override void AttackStarted(ActionType type, AttackDefinition atk, int origin) { if (type == ActionType.HeavyAttack && pending > 0f) pendingOrigin = origin; }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (!hit.basicAttack || hit.featureEffect || !(target is EnemyBrain e)) return;
            bool landed = result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken;
            if (pending > 0f && hit.originId == pendingOrigin)
            {
                if (!landed) return;
                hearth = e; budget = initial = pending; until = pc.Clock + P("hearth_seconds"); pending = 0f; pendingOrigin = -1;
                HellFx.Ambient(e.transform, Vector3.up * e.Def.scale, HellFxElement.Fire, 0.5f * e.Def.scale, P("hearth_seconds"));
                Count("paleniska");
                return;
            }
            if (!landed || e != hearth || budget <= 0f || hit.originId == pendingOrigin) return;
            float give = Mathf.Min(budget, initial * P("portion"));
            budget -= give;
            var h = PowerHit("Iskry paleniska", 0f, give, Element.Fire, e.AimPoint);
            Apply(e, h);
            HellFx.Impact(e.AimPoint, HellFxElement.Fire, new Color(1f, 0.6f, 0.2f), 0.8f);
            Count("iskry", give);
            if (budget <= 0.01f) hearth = null;
        }

        float initial;

        public override void Tick(float dt)
        {
            if (hearth != null && (hearth.IsDead || pc.Clock > until)) { Count("palenisko zgasło", budget); hearth = null; budget = 0f; initial = 0f; }
            if (pending > 0f && pendingOrigin >= 0 && pc.Actions.Current != ActionType.HeavyAttack) { Count("Żar zmarnowany (pudło)", pending); pending = 0f; pendingOrigin = -1; }
        }

        public override void Death() { hearth = null; budget = 0f; pending = 0f; initial = 0f; }
        public override string Hud => hearth != null ? $"palenisko {budget:0}" : null;
    }

    // ====================================================================== F04 Szata odroczonego pogorzeliska

    /// <summary>Własne podpalenia wrogów nie tykają, gromadzą budżet; ręczny mocny atak ściska go w słup ognia.</summary>
    public sealed class DeferredWildfirePower : UniquePower
    {
        public override void OutgoingLate(ref HitData hit)
        {
            if (hit.statuses == null || !hit.statuses.Exists(s => s.kind == StatusKind.Burn)) return;
            hit.mods.deferBurn = true; hit.mods.deferCapSeconds = P("cap_seconds");
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (!hit.basicAttack || hit.featureEffect || pc.Actions.Current != ActionType.HeavyAttack || !(target is EnemyBrain e)) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            float budget = e.Status.TakeDeferredBurn() * P("pillar_mult", 1f);
            if (budget <= 0.5f) return;
            Vector3 at = e.transform.position;
            Apply(e, PowerHit("Słup pogorzeliska", 0f, budget, Element.Fire, at));
            var splash = PowerHit("Słup pogorzeliska", 0f, budget * 0.5f, Element.Fire, at);
            foreach (var x in UniqueGeo.EnemiesIn(at, P("radius"), x => x != e)) Apply(x, splash);
            HellFx.Burst(at + Vector3.up, HellFxElement.Fire, P("radius"));
            Sfx.Play(SoundId.FireCast, at, 0.9f, 0.7f);
            Count("słupy"); Count("budżet słupów", budget);
        }
    }

    // ====================================================================== F05 Diadem pieca pod niebem

    /// <summary>Podstawowy strzał z wnętrza własnego Kręgu żaru spada z nieba w celowany punkt (jedno uderzenie na akcję, salwa dzieli budżet).</summary>
    public sealed class SkyFurnacePower : UniquePower
    {
        HitData strike; bool pendingStrike;
        protected override string EquipmentDormant() => NeedSkill("hu_ember_ring");

        public override bool CaptureShot(in HitData hit, Vector3 origin, Vector3 dir, float speed, Color c, int index, int count)
        {
            if (HuWorld.InfusionAt(pc.transform.position, new HashSet<int>()) == null) return false;
            if (index == 0)
            {
                strike = hit; pendingStrike = true;
                var e = pc.HuAimTarget(P("range"), 50f, pc.transform.position, dir);
                Vector3 at = e != null ? e.transform.position : pc.transform.position + new Vector3(dir.x, 0f, dir.z).normalized * Mathf.Min(8f, P("range"));
                at = HuWorld.Clamp(pc.transform.position, at);
                var mark = VisualFx.GroundMarker(at, P("radius"), new Color(1f, 0.5f, 0.15f), HellFxElement.Fire);
                HuWorld.Add(new HuTelegraphRuntime(mark, P("delay")));
                HuWorld.After(P("delay"), () =>
                {
                    if (pc == null || pc.IsDead || !pendingStrike) return;
                    pendingStrike = false;
                    var h = strike; h.sourcePosition = at; h.isProjectile = false; h.explosionRadius = 0f;
                    int n = HuWorld.Explode(pc, at, P("radius"), h, new Color(1f, 0.5f, 0.15f));
                    HellFx.Burst(at + Vector3.up * 0.5f, HellFxElement.Fire, P("radius"));
                    Count("uderzenia z nieba"); Count("trafieni", n);
                });
            }
            else { strike.physical += hit.physical; strike.magic += hit.magic; } // salwa: jeden budżet
            return true;
        }

        public override void Death() { pendingStrike = false; }
    }
}
