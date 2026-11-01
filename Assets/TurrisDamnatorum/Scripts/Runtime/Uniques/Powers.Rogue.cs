using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== L01 Maska fałszywego króla

    /// <summary>Fałszywy świadek oszukuje wrogów, by atakowali wskazanego prawdziwego wroga; ich ciosy ranią go (znormalizowany budżet, limit).</summary>
    public sealed class FalseKingPower : UniquePower
    {
        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "witness.press" || !(data is WitnessPress w)) return false;
            var king = pc.FeatureTarget(w.range + 4f);
            if (king == null) return false;
            bool others = EnemyBrain.All.Any(e => e != null && !e.IsDead && e != king && HuWorld.Flat(e.transform.position, king.transform.position) <= P("range"));
            if (!others) { Count("samotny cel – zwykły wabik"); return false; }
            int hits = 0;
            float cap = pc.BasicDamage * P("hit_cap");
            var decoy = FeatureDecoy.SpawnOn(pc, king.transform, w.seconds, w.maxDeceived, P("range"), h =>
            {
                if (king == null || king.IsDead || hits >= PI("max_hits")) return 0f;
                hits++;
                float dmg = Mathf.Min(h.Total, cap);
                var ph = PowerHit("Cios oszukanego", dmg, 0f, Element.None, h.sourcePosition);
                ph.blockable = false;
                var r = Apply(king, ph);
                Count("ciosy w króla"); Count("obrażenia oszustwa", r.healthDamage);
                return r.healthDamage;
            }, king);
            w.decoy = decoy;
            HellFx.Shadow(king.transform, king.transform.position, true, w.seconds, 0.7f);
            CombatEvents.RaiseWorldText(king.AimPoint + Vector3.up, "FAŁSZYWY KRÓL", new Color(0.7f, 0.6f, 1f));
            return true;
        }
    }

    // ====================================================================== L02 Rękawice zawodowego oszusta

    /// <summary>Pułapka ze skradzionej sztuczki: uzbraja się, kontakt wroga wykonuje profil sztuczki w miejscu pułapki.</summary>
    public sealed class StolenTrap : IHuRuntime, IUniqueMine
    {
        readonly PlayerCombat pc; readonly TrickData trick; Vector3 at; readonly float arm, life, trigger; float t; bool done; GameObject ring;
        public StolenTrap(PlayerCombat pc, TrickData trick, Vector3 at, float arm, float life, float trigger)
        {
            this.pc = pc; this.trick = trick; this.at = HuWorld.Ground(at); this.arm = arm; this.life = life; this.trigger = trigger;
            if (FxLibrary.Enabled) ring = VisualFx.GroundMarker(this.at, trigger, new Color(0.9f, 0.75f, 0.4f), trick.element != Element.None ? HellFx.For(trick.element) : HellFxElement.Physical);
            HuWorld.RegisterAnchor(this); UniqueMines.Register(this);
        }
        public PlayerCombat Owner => pc;
        public bool Armed => t >= arm;
        public float Remaining => life - t;
        public Vector3 AnchorPosition => at;
        public bool AnchorLive => !done;
        public void MoveTo(Vector3 p) { at = HuWorld.Ground(p); if (ring != null) ring.transform.position = at; }
        public bool Tick(float dt)
        {
            if (done) return false;
            t += dt;
            if (t >= life || pc == null || pc.IsDead) { done = true; Free(); return false; }
            if (Armed && !UniqueMines.Held(this) && HuWorld.Nearest(at, trigger) != null) { Trigger(); return false; }
            return true;
        }
        public void Trigger()
        {
            if (done || pc == null || pc.IsDead) { done = true; Free(); return; }
            done = true; Free();
            var h = trick.hit; h.sourcePosition = at;
            var prey = HuWorld.Nearest(at, Mathf.Max(trigger, 6f));
            switch (trick.kind)
            {
                case StolenTrickFeature.TrickKind.Projectile:
                    for (int i = 0; i < Mathf.Max(1, trick.count); i++)
                    {
                        var hi = h; hi.physical /= Mathf.Max(1, trick.count); hi.magic /= Mathf.Max(1, trick.count);
                        Vector3 from = at + Vector3.up;
                        float ang = trick.count <= 1 ? 0f : Mathf.Lerp(-trick.spread, trick.spread, i / (float)(trick.count - 1));
                        Vector3 dir = prey != null ? prey.AimPoint - from : Vector3.forward;
                        Projectile.Spawn(from, Quaternion.Euler(0, ang, 0) * dir, 18f, hi, Faction.Player, pc, Names.ElementColor(trick.element), 0.25f, 2f, prey, prey != null ? 60f : 0f);
                    }
                    break;
                default:
                    float r = trick.kind == StolenTrickFeature.TrickKind.Melee ? trick.reach : trick.radius;
                    HuWorld.Explode(pc, at, r, h, Names.ElementColor(trick.element));
                    break;
            }
            pc.UniqueTelemetry.Add("L02: pułapki odpalone");
        }
        public void Stop() { done = true; Free(); }
        void Free() { if (ring != null) Object.Destroy(ring); }
    }

    public sealed class ProfessionalCheatPower : UniquePower
    {
        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "trick.execute" || !(data is TrickData t)) return false;
            var e = pc.FeatureTarget(P("place_range"));
            Vector3 at = e != null ? e.transform.position : pc.transform.position + pc.FeatureAim() * 4f;
            at = HuWorld.Clamp(pc.transform.position, at);
            HuWorld.Add(new StolenTrap(pc, t, at, P("arm_seconds"), P("trap_seconds"), P("trigger_radius")));
            Count("pułapki " + t.kind);
            return true;
        }
    }

    // ====================================================================== L03 Pas niewypałów

    /// <summary>Bomba siarkowa przykleja się do ofiary; własne Sidła na nosicielu albo ręczny cios w plecy odpalają ładunek.</summary>
    public sealed class DudsPower : UniquePower
    {
        sealed class Bomb { public EnemyBrain carrier; public HitData hit; public float radius, until; public HellParticles fx; }
        readonly List<Bomb> bombs = new List<Bomb>();
        public int Bombs => bombs.Count;

        public bool Stick(EnemyBrain e, HitData hit, float radius)
        {
            if (e == null || e.IsDead) return false;
            while (bombs.Count >= PI("max_bombs")) Remove(0);
            hit.explosionRadius = radius; // wybuch to nie nowy cios w plecy
            bombs.Add(new Bomb { carrier = e, hit = hit, radius = radius, until = pc.Clock + P("bomb_seconds"),
                fx = FxLibrary.Enabled ? HellFx.Ambient(e.transform, Vector3.up * e.Def.scale, HellFxElement.Fire, 0.3f * e.Def.scale, P("bomb_seconds")) : null });
            CombatEvents.RaiseWorldText(e.AimPoint + Vector3.up * 0.6f, "NIEWYPAŁ", new Color(1f, 0.6f, 0.2f));
            Count("przyklejone bomby");
            return true;
        }

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_sulfur_bomb") return false;
            var e = pc.HuAimTarget(cast.P.GetF(HuParams.Range, 8f), 50f, pc.transform.position, aim);
            if (e == null) return false; // bez celu – zwykła bomba
            return Stick(e, cast.Hit(cast.P.GetF("explosion_share", 1f)), cast.Area("explosion_radius", 2.4f));
        }

        public override float BackstabBonus(float bonus, in HitData hit, EnemyBrain target)
        {
            if (hit.featureEffect || hit.isStatusTick) return bonus;
            for (int i = bombs.Count - 1; i >= 0; i--) if (bombs[i].carrier == target) { Detonate(i, "cios w plecy"); }
            return bonus;
        }

        void Detonate(int i, string why)
        {
            var b = bombs[i];
            Remove(i);
            if (b.carrier == null) return;
            Vector3 at = b.carrier.transform.position;
            var h = b.hit; h.sourcePosition = at;
            int n = HuWorld.Explode(pc, at, b.radius, h, new Color(1f, 0.55f, 0.15f));
            HellFx.Burst(at + Vector3.up, HellFxElement.Fire, b.radius);
            Count("detonacje: " + why); Count("trafieni", n);
        }

        void Remove(int i) { if (bombs[i].fx != null) bombs[i].fx.StopAndRelease(); bombs.RemoveAt(i); }

        public override void Tick(float dt)
        {
            for (int i = bombs.Count - 1; i >= 0; i--)
            {
                var b = bombs[i];
                if (b.carrier == null || b.carrier.IsDead) { Count("bomby z martwym nosicielem"); Remove(i); continue; }
                if (pc.Clock > b.until) { Count("bomby wygasłe"); Remove(i); continue; }
                foreach (var z in DamageZone.Active)
                    if (z != null && ReferenceEquals(z.Owner, pc) && z.ZoneHit.attackName == SkillName("hu_snare") && HuWorld.Flat(z.transform.position, b.carrier.transform.position) <= z.Radius)
                    { Detonate(i, "Sidła"); break; }
            }
        }

        public override void Death() { for (int i = bombs.Count - 1; i >= 0; i--) Remove(i); }
        public override string Hud => bombs.Count > 0 ? $"niewypały: {bombs.Count}" : null;
    }

    // ====================================================================== L04 Sztylet niedokończonej ucieczki

    /// <summary>Pułapka bomby w punkcie startu Kroku cienia (kontakt: wybuch albo – z Pasem niewypałów – przyklejenie).</summary>
    public sealed class BombTrap : IHuRuntime, IUniqueMine
    {
        readonly HuCast cast; Vector3 at; float t; bool done; GameObject ring;
        const float Arm = 0.5f, Life = 10f, TriggerRadius = 1.6f;
        public BombTrap(HuCast cast, Vector3 at) { this.cast = cast; this.at = HuWorld.Ground(at); if (FxLibrary.Enabled) ring = VisualFx.GroundMarker(this.at, TriggerRadius, cast.color, cast.family); HuWorld.RegisterAnchor(this); UniqueMines.Register(this); }
        public PlayerCombat Owner => cast.caster;
        public bool Armed => t >= Arm;
        public float Remaining => Life - t;
        public Vector3 AnchorPosition => at;
        public bool AnchorLive => !done;
        public void MoveTo(Vector3 p) { at = HuWorld.Ground(p); if (ring != null) ring.transform.position = at; }
        public bool Tick(float dt)
        {
            if (done) return false;
            t += dt;
            if (t >= Life || cast.caster == null || cast.caster.IsDead) { done = true; Free(); return false; }
            if (Armed && !UniqueMines.Held(this) && HuWorld.Nearest(at, TriggerRadius) != null) { Trigger(); return false; }
            return true;
        }
        public void Trigger()
        {
            if (done) return;
            done = true; Free();
            var pc = cast.caster;
            if (pc == null || pc.IsDead) return;
            var victim = HuWorld.Nearest(at, TriggerRadius + 0.5f);
            var duds = pc.Uniques.Get<DudsPower>();
            if (duds != null && duds.Active && victim != null && duds.Stick(victim, cast.Hit(cast.P.GetF("explosion_share", 1f)), cast.Area("explosion_radius", 2.4f))) return;
            HuWorld.Explode(pc, at, cast.Area("explosion_radius", 2.4f), cast.Hit(cast.P.GetF("explosion_share", 1f)), cast.color);
            HellFx.Burst(at + Vector3.up, cast.family, 1.5f);
        }
        public void Stop() { done = true; Free(); }
        void Free() { if (ring != null) Object.Destroy(ring); }
    }

    public sealed class UnfinishedEscapePower : UniquePower
    {
        Vector3 start; bool haveStart;
        protected override string EquipmentDormant()
        {
            if (Run.equipment.Get(EquipSlot.MainHand) != Item && Run.equipment.Get(EquipSlot.OffHand) != Item) return "uśpiona: sztylet musi być w ręce";
            if (SlotSkill("hu_shadow_step") == null) return NeedSkill("hu_shadow_step");
            return SlotSkill("hu_snare", "hu_sulfur_bomb") == null ? NeedSkill("hu_snare", "hu_sulfur_bomb") : null;
        }

        public override void ModifyHuCast(SpellRuntime sk, ref Vector3 origin, ref Vector3 aim, ref Vector3 target)
        {
            if (IsSkill(sk, "hu_shadow_step")) { start = origin; haveStart = true; }
        }

        public override void CastFired(SpellRuntime sk, int origin)
        {
            if (!IsSkill(sk, "hu_shadow_step") || !haveStart) return;
            haveStart = false;
            SpellRuntime trap = null;
            for (int i = 0; i < RunState.SkillSlotCount; i++)
            {
                var s = pc.Skill(i);
                if (s == null || !IsSkill(s, "hu_snare", "hu_sulfur_bomb")) continue;
                if (!s.instance.Ready || !s.equipmentMet || (s.Def.IsSpell ? pc.Mana.Current + 0.01f < s.manaCost : pc.Stamina.Current <= 0f)) continue;
                trap = s; break;
            }
            if (trap == null) { Fail("pułapka niegotowa", "Niedokończona ucieczka: pułapka niegotowa – zwykły Krok"); return; }
            if (trap.Def.IsSpell) pc.Mana.TrySpend(trap.manaCost); else pc.Stamina.Drain(trap.staminaCost, pc.Config.balance.staminaRegenDelay);
            trap.instance.TryUse();
            var dir = pc.transform.forward;
            var cast = new HuCast(pc, trap, start, dir, start, 1f);
            if (trap.Def.id == "hu_sulfur_bomb") HuWorld.Add(new BombTrap(cast, start));
            else pc.UniqueFireHuCast(cast, dir, 0f, origin);
            Count("pułapki ucieczki: " + trap.Def.id);
        }
    }

    // ====================================================================== L05 Medalion sygnału z otchłani

    /// <summary>Własne miny i pułapki nie reagują na wejście wroga – odpala je ręczny nóż przelatujący obok znacznika.</summary>
    public sealed class AbyssSignalPower : UniquePower
    {
        protected override void OnBind() { UniqueMines.HoldContact = Holds; }
        bool Holds(IUniqueMine m) => pc != null && Active && m.Owner == pc;
        public override void Dispose() { if (UniqueMines.HoldContact == (System.Func<IUniqueMine, bool>)Holds) UniqueMines.HoldContact = null; base.Dispose(); }
        public override void FloorStart() { UniqueMines.HoldContact = Holds; }

        public override void Tick(float dt)
        {
            if (!Active) return;
            var mines = UniqueMines.Live(pc);
            if (mines.Count == 0) return;
            foreach (var s in HuShot.Live.ToArray())
            {
                if (s == null || !s.IsActive || s.SourceCast == null || s.SourceCast.caster != pc || s.SourceCast.Def.id != "hu_knife_throw") continue;
                foreach (var m in mines)
                    if (m.AnchorLive && HuWorld.Flat(m.AnchorPosition, s.transform.position) <= P("signal_radius")) { m.Trigger(); Count("sygnały"); }
            }
        }
    }

    // ====================================================================== K01 Rękawice tkacza cięciw

    /// <summary>Wyrwanie grotów łączy kotwice odcinkami; wrogowie na odcinkach dzielą sumę wyrwania (jeden budżet).</summary>
    public sealed class BowstringWeaverPower : UniquePower
    {
        Vector3? ground;
        protected override void OnBind() { Projectile.Expired += OnArrowEnd; }
        public override void Dispose() { Projectile.Expired -= OnArrowEnd; base.Dispose(); }

        void OnArrowEnd(Projectile p, Vector3 at)
        {
            if (!Active || p == null || !ReferenceEquals(p.Owner, pc) || p.Hit.nailShare <= 0f || ground.HasValue) return;
            ground = HuWorld.Ground(at); // jeden grot w podłożu – punkt terenowy
            Count("groty w podłożu");
        }

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "nails.rip" || !(data is HuntersNailsFeature f)) return false;
            var nails = f.NailBudgets().OrderBy(kv => HuWorld.Flat(kv.Key.transform.position, pc.transform.position)).Take(PI("max_anchors")).ToList();
            var points = nails.Select(kv => kv.Key.transform.position).ToList();
            if (ground.HasValue) points.Add(ground.Value);
            if (points.Count < 2) { ground = null; return false; } // jedna kotwica – zwykłe wyrwanie
            float pool = nails.Sum(kv => kv.Value) * amount;
            int segments = points.Count - 1;
            float perSegment = pool / segments;
            int hits = 0;
            for (int i = 0; i < segments; i++)
            {
                Vector3 a = points[i], b = points[i + 1];
                var victims = UniqueGeo.EnemiesOnSegment(a, b, P("width") * 0.5f);
                foreach (var kv in nails) if (!victims.Contains(kv.Key) && (kv.Key.transform.position == a || kv.Key.transform.position == b)) victims.Add(kv.Key);
                if (victims.Count == 0) continue;
                float each = perSegment / victims.Count;
                foreach (var e in victims) { Apply(e, PowerHit("Cięciwa", each, 0f, Element.None, e.transform.position)); hits++; }
                LightningBolt.Strike(a + Vector3.up, b + Vector3.up, new Color(0.95f, 0.9f, 0.7f));
            }
            ground = null;
            Count("sieci"); Count("trafienia cięciw", hits); Count("budżet sieci", pool);
            return true;
        }

        public override void Death() { ground = null; }
    }

    // ====================================================================== K02 Buty wygnanego strzelca

    /// <summary>Mocny strzał przy stanowisku wylatuje z ręki, potem gracz zamienia się miejscami ze stanowiskiem (osobne odnowienie).</summary>
    public sealed class ExiledShooterPower : UniquePower
    {
        FiringPost pending; float ready;
        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "post.shot" || !(data is FiringPost p) || pc.Actions.Current != ActionType.HeavyAttack || pc.Clock < ready) return false;
            pending = p;
            return true; // ten strzał z ręki (źródło sprzed zamiany)
        }

        public override void AttackActive(ActionType type, AttackDefinition atk, int origin)
        {
            if (type != ActionType.HeavyAttack || pending == null) return;
            var p = pending; pending = null;
            if (!p.Alive) return;
            if (HuWorld.Nearest(p.transform.position, 0.9f) != null) { Fail("zajęte", "Wygnany strzelec: stanowisko zajęte"); return; }
            Vector3 old = pc.transform.position;
            pc.HuTeleport(p.transform.position, new Color(0.95f, 0.8f, 0.45f));
            p.transform.position = HuWorld.Ground(old);
            ready = pc.Clock + P("swap_cooldown");
            Count("zamiany");
        }

        public override void Death() { pending = null; }
        public override string Hud => pc.Clock < ready ? $"zamiana {ready - pc.Clock:0.0} s" : null;
    }

    // ====================================================================== K03 Łuk grabarza

    /// <summary>Lekki atak to pchnięcie strzałą w zwarciu; po trafieniu strzała leci dalej przez ofiarę (bez ponownego trafienia jej).</summary>
    public sealed class GravediggersBowPower : UniquePower
    {
        readonly Dictionary<AttackDefinition, AttackDefinition> thrusts = new Dictionary<AttackDefinition, AttackDefinition>();
        AttackDefinition source; int passedOrigin = -1;
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: łuk musi być w rękach" : null;

        public override AttackDefinition AttackOverride(ActionType type, AttackDefinition atk)
        {
            if (type != ActionType.LightAttack || atk == null || atk.delivery != AttackDelivery.Projectile) return atk;
            if (!thrusts.TryGetValue(atk, out var t))
            {
                t = atk.Clone(); t.delivery = AttackDelivery.Melee; t.reach = P("reach"); t.radius = 0.7f; t.animation = AttackAnim.Thrust; t.name = "Pchnięcie strzałą";
                thrusts[atk] = t;
            }
            source = atk;
            return t;
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (!hit.basicAttack || hit.featureEffect || source == null || pc.Actions.Current != ActionType.LightAttack || hit.originId == passedOrigin || !(target is EnemyBrain e)) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            passedOrigin = hit.originId;
            Vector3 dir = pc.transform.forward; dir.y = 0f; dir.Normalize();
            Vector3 from = e.AimPoint + dir * (0.6f + 0.5f * e.Def.scale); // za ofiarą – bez ponownego trafienia jej
            var h = pc.UniqueWeaponHit(source, P("pass_share"), false);
            h.isProjectile = true; h.fromMeleeWeapon = false; h.attackName = "Strzała grabarza";
            Projectile.Spawn(from, dir, source.projectileSpeed, h, Faction.Player, pc, new Color(1f, 0.95f, 0.85f), 0.12f, 1.5f, null, 0f, true, new ProjectileOptions { pierce = 1 });
            Count("strzały przez ofiarę");
        }
    }

    // ====================================================================== K04 Pas niedokończonej salwy

    /// <summary>Salwa wbija rząd strzał w ziemię; następny mocny strzał wystrzeliwuje cały rząd raz w swoim kierunku.</summary>
    public sealed class UnfinishedVolleyPower : UniquePower
    {
        readonly List<(Vector3 at, HitData hit, GameObject fx)> row = new List<(Vector3, HitData, GameObject)>();
        float armedAt, until; Vector3 heavyDir;
        public int Planted => row.Count;
        protected override string EquipmentDormant() => NeedSkill("hu_volley");

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_volley") return false;
            Clear();
            int n = Mathf.Max(1, (int)cast.P.Get("count", 5));
            var hit = cast.Hit(1f, projectile: true);
            Vector3 side = new Vector3(cast.direction.z, 0f, -cast.direction.x);
            for (int i = 0; i < n; i++)
            {
                float off = (i - (n - 1) * 0.5f) * 0.7f;
                Vector3 at = HuWorld.Clamp(cast.origin, cast.origin + cast.direction * 2.2f + side * off);
                GameObject fx = FxLibrary.Enabled ? VisualFx.GroundMarker(HuWorld.Ground(at), 0.25f, new Color(0.9f, 0.85f, 0.7f), HellFxElement.Physical) : null;
                if (FxLibrary.Enabled) HuFallingArrow.Drop(HuWorld.Ground(at), new Color(0.95f, 0.9f, 0.75f)); // widoczna strzała wbijana w ziemię
                row.Add((at, hit, fx));
            }
            armedAt = pc.Clock + P("arm_seconds"); until = pc.Clock + P("row_seconds");
            Count("rzędy");
            return true;
        }

        public override bool CaptureShot(in HitData hit, Vector3 origin, Vector3 dir, float speed, Color c, int index, int count)
        {
            if (index == 0 && pc.Actions.Current == ActionType.HeavyAttack) heavyDir = dir;
            return false;
        }

        public override void AttackActive(ActionType type, AttackDefinition atk, int origin)
        {
            if (type != ActionType.HeavyAttack || row.Count == 0 || atk == null || atk.delivery != AttackDelivery.Projectile) return;
            if (pc.Clock < armedAt) { Fail("nieuzbrojony", "Rząd strzał jeszcze się uzbraja"); return; }
            Vector3 dir = heavyDir.sqrMagnitude > 0.01f ? heavyDir : pc.FeatureAim();
            foreach (var r in row)
            {
                var h = r.hit; h.sourcePosition = r.at;
                Projectile.Spawn(r.at + Vector3.up * 0.9f, dir, 22f, h, Faction.Player, pc, new Color(1f, 0.95f, 0.85f), 0.12f, 1.6f, null, 0f, true);
            }
            Count("wystrzelone rzędy"); Count("strzały rzędu", row.Count);
            Clear();
        }

        void Clear() { foreach (var r in row) if (r.fx != null) Object.Destroy(r.fx); row.Clear(); }
        public override void Tick(float dt) { if (row.Count > 0 && pc.Clock > until) { Count("rzędy wygasłe"); Clear(); } }
        public override void Death() { Clear(); }
        public override string Hud => row.Count > 0 ? $"rząd strzał: {row.Count}" : null;
    }

    // ====================================================================== K05 Kaptur odwróconego celownika

    /// <summary>Deszcz strzał podąża za ofiarą ostatniego podstawowego strzału (bez odnowienia czasu ani rytmu).</summary>
    public sealed class ReversedSightPower : UniquePower
    {
        DamageZone rain; EnemyBrain guide; float drop; int guideOrigin = -1;
        protected override string EquipmentDormant() => NeedSkill("hu_arrow_rain");

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_arrow_rain") return false;
            int before = DamageZone.Active.Count;
            HuSkillCaster.SpawnZone(cast, cast.target, cast.Area("zone_radius", 2f), cast.P.GetF("zone_seconds"), cast.P.GetF("zone_interval", 0.5f), cast.P.GetF("zone_share", 0.25f));
            rain = DamageZone.Active.Count > before ? DamageZone.Active[DamageZone.Active.Count - 1] : null;
            Count("deszcze");
            return true;
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (!hit.basicAttack || !hit.isProjectile || hit.featureEffect || hit.originId == guideOrigin || !(target is EnemyBrain e)) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            guideOrigin = hit.originId; // salwa: jeden przewodnik
            if (guide != e) Count("zmiany przewodnika");
            guide = e;
        }

        public override void Tick(float dt)
        {
            if (rain == null || rain.Remaining <= 0f) { rain = null; return; }
            if (guide != null && !guide.IsDead)
            {
                Vector3 p = rain.transform.position, to = guide.transform.position - p; to.y = 0f;
                float d = to.magnitude;
                if (d > 0.1f) rain.MoveTo(p + to / d * Mathf.Min(d, P("follow_speed") * dt));
            }
            drop -= dt;
            if (drop <= 0f && FxLibrary.Enabled)
            {
                drop = 0.07f;
                Vector2 c = Random.insideUnitCircle * rain.Radius;
                HuFallingArrow.Drop(HuWorld.Ground(rain.transform.position + new Vector3(c.x, 0f, c.y)), new Color(0.9f, 0.85f, 0.7f));
            }
        }

        public override void Death() { rain = null; guide = null; }
    }

    // ====================================================================== Z01 Całun własnego pogrzebu

    /// <summary>Przygwożdżony cień idzie przy właścicielu; ręczny atak, który trafi ofiarę przez cień, kosztuje życie (raz na akcję).</summary>
    public sealed class OwnFuneralPower : UniquePower
    {
        PinnedShadow following; int paidOrigin = -1;

        public override void Tick(float dt)
        {
            var s = Feature<PinnedShadowFeature>()?.Shadow;
            if (s != null && s != following) { s.Follow(pc.transform, new Vector3(0f, 0f, P("range") * 0.45f)); following = s; Count("cienie przy stopach"); }
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            var s = following;
            if (s == null || !s.Alive || !ReferenceEquals(target, s.Victim) || hit.featureEffect || hit.isStatusTick || hit.originId == paidOrigin) return;
            if (HuWorld.Flat(pc.transform.position, s.Victim.transform.position) <= P("range")) return; // ciało w zasięgu – to zwykły cios
            paidOrigin = hit.originId;
            float cost = pc.Health.Max * P("life_cost");
            pc.PayLife(cost);
            Count("ciosy przez cień"); Count("koszt życia", cost);
        }

        public override void Death() { following = null; }
    }

    // ====================================================================== Z02 Pierścień zwleczonego wyroku

    /// <summary>Część premii egzekucji staje się raną (krwawienie z tym budżetem) zamiast zwiększać trafienie.</summary>
    public sealed class DeferredVerdictPower : UniquePower
    {
        public override void OutgoingAgainst(ref HitData hit, EnemyBrain target)
        {
            if (target == null || hit.executeBonus <= 0f || !target.ExecuteQualified || hit.isStatusTick || hit.featureEffect) return;
            float part = hit.executeBonus * P("wound_share");
            float budget = hit.Total * part;
            hit.executeBonus -= part;
            target.Status.AddBleed(budget, P("wound_seconds"), pc.Config.balance);
            Count("rany wyroku", budget);
        }
    }

    // ====================================================================== Z03 Igła grobowej szwaczki

    /// <summary>Mocny cios wyciąga część krwawienia ofiary do pułapki w ziemi; kontakt innego wroga przenosi ranę na niego.</summary>
    public sealed class GraveNeedlePower : UniquePower
    {
        sealed class Trap { public Vector3 at; public float budget, remaining, until; public EnemyBrain source; public GameObject fx; }
        readonly List<Trap> traps = new List<Trap>();
        public int Traps => traps.Count;
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item && Run.equipment.Get(EquipSlot.OffHand) != Item ? "uśpiona: igła musi być w ręce" : null;

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (!hit.basicAttack || hit.featureEffect || pc.Actions.Current != ActionType.HeavyAttack || !(target is EnemyBrain e) || e.Status.BleedStacks == 0) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            float budget = e.Status.TakeBleed(P("pull_share"), out float rem); // atomowo: znika z ofiary
            if (budget <= 0.5f) return;
            while (traps.Count >= PI("max_traps")) Remove(0);
            var t = new Trap { at = HuWorld.Ground(e.transform.position), budget = budget, remaining = Mathf.Max(0.6f, rem), until = pc.Clock + P("trap_seconds"), source = e };
            if (FxLibrary.Enabled) t.fx = VisualFx.GroundMarker(t.at, P("trigger_radius"), new Color(0.7f, 0.1f, 0.12f), HellFxElement.Enemy);
            traps.Add(t);
            Count("pułapki rany"); Count("budżet wyciągnięty", budget);
        }

        void Remove(int i) { if (traps[i].fx != null) Object.Destroy(traps[i].fx); traps.RemoveAt(i); }

        public override void Tick(float dt)
        {
            for (int i = traps.Count - 1; i >= 0; i--)
            {
                var t = traps[i];
                if (pc.Clock > t.until) { Count("pułapki wygasłe", t.budget); Remove(i); continue; }
                var prey = HuWorld.Nearest(t.at, P("trigger_radius"), x => x != t.source);
                if (prey == null) continue;
                prey.Status.AddBleed(t.budget, t.remaining, pc.Config.balance);
                FxLibrary.Blood(prey.AimPoint, Vector3.up, 1.2f);
                Count("rany przeniesione", t.budget);
                Remove(i);
            }
        }

        public override void Death() { for (int i = traps.Count - 1; i >= 0; i--) Remove(i); }
        public override string Hud => traps.Count > 0 ? $"pułapki rany: {traps.Count}" : null;
    }

    // ====================================================================== Z04 Maska milczącego świadka

    /// <summary>Część premii za cios w plecy zostaje ładunkiem; następny Rzut nożami dzieli go między noże (bez drugiej Zasadzki).</summary>
    public sealed class SilentWitnessPower : UniquePower
    {
        float charge, until; int knifeOrigin = -1, knives = 1; float knifeUntil;
        public float Charge => charge;

        public override float BackstabBonus(float bonus, in HitData hit, EnemyBrain target)
        {
            if (hit.featureEffect || hit.isStatusTick) return bonus;
            float keep = bonus * P("store_share");
            charge += hit.Total * keep;
            until = pc.Clock + P("charge_seconds");
            Count("ładunki świadka", hit.Total * keep);
            return bonus - keep;
        }

        public override void CastFired(SpellRuntime sk, int origin)
        {
            if (!IsSkill(sk, "hu_knife_throw") || charge <= 0f) return;
            knifeOrigin = origin; knives = Mathf.Max(1, (int)sk.instance.seals.Shape.Parameters.Get("count", 3)); knifeUntil = pc.Clock + 1.5f;
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (charge <= 0f || hit.originId != knifeOrigin || hit.featureEffect || !hit.isProjectile) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            float give = Mathf.Min(charge, charge / Mathf.Max(1, knives) + 0.001f);
            give = Mathf.Min(give, charge);
            charge -= give; knives = Mathf.Max(1, knives - 1);
            var h = PowerHit("Milczący świadek", give, 0f, Element.None, target.AimPoint);
            h.explosionRadius = 0.01f; // dodatek nie dostaje drugiej Zasadzki
            Apply(target, h);
            Count("dostarczony ładunek", give);
        }

        public override void Tick(float dt)
        {
            if (charge > 0f && pc.Clock > until && pc.Clock > knifeUntil) { Count("ładunek wygasły", charge); charge = 0f; }
        }

        public override void Death() { charge = 0f; }
        public override string Hud => charge > 0f ? $"świadek: {charge:0}" : null;
    }

    // ====================================================================== Z05 Rękawice karminowego węzła

    /// <summary>Ofiary jednego Cięcia krzyżowego są zszyte; ręczne trafienie jednej wydaje część jej rany na podzieloną odpowiedź u innych.</summary>
    public sealed class CrimsonKnotPower : UniquePower
    {
        int crossOrigin = -1; float gather;
        readonly List<EnemyBrain> pending = new List<EnemyBrain>();
        readonly List<EnemyBrain> knot = new List<EnemyBrain>();
        float knotUntil, ready;
        public int Knotted => knot.Count;

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (!(target is EnemyBrain e) || hit.featureEffect || hit.isStatusTick || hit.isReaction) return;
            bool landed = result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken;
            // Stożek trafia w trakcie rzutu (przed CastFired): zbieranie ofiar startuje od pierwszego trafienia tej akcji.
            if (pc.Actions.Current == ActionType.Cast && IsSkill(pc.CurrentSpellRuntime, "hu_cross_cut"))
            {
                if (hit.originId != crossOrigin || gather <= 0f) { crossOrigin = hit.originId; pending.Clear(); gather = 0.15f; }
                if (landed && !pending.Contains(e)) pending.Add(e);
                return;
            }
            if (!landed || knot.Count < 2 || !knot.Contains(e) || pc.Clock < ready) return;
            float budget = e.Status.TakeBleed(P("spend_share"), out _);
            if (budget <= 0.5f) return;
            var others = knot.Where(x => x != null && !x.IsDead && x != e).ToList();
            if (others.Count == 0) return;
            ready = pc.Clock + P("interval");
            foreach (var o in others)
            {
                var h = PowerHit("Karminowy węzeł", budget / others.Count, 0f, Element.None, o.AimPoint);
                h.isReaction = true;
                Apply(o, h);
                FxLibrary.Lightning(e.AimPoint, o.AimPoint, new Color(0.8f, 0.1f, 0.15f));
            }
            Count("odpowiedzi węzła"); Count("budżet węzła", budget);
        }

        public override void Tick(float dt)
        {
            if (gather > 0f)
            {
                gather -= dt;
                if (gather <= 0f && pending.Count >= 2) { knot.Clear(); knot.AddRange(pending.Take(PI("max_links"))); knotUntil = pc.Clock + P("knot_seconds"); Count("węzły"); }
            }
            if (knot.Count > 0 && pc.Clock > knotUntil) knot.Clear();
            knot.RemoveAll(x => x == null || x.IsDead);
        }

        public override void Death() { knot.Clear(); pending.Clear(); }
        public override string Hud => knot.Count >= 2 ? $"węzeł: {knot.Count}" : null;
    }

    // ====================================================================== Q01 Łuk siódmej pieczęci

    /// <summary>Czar ze strzały zaklęcia rozkłada się pasem od miejsca strzału do trafienia; każdy w pasie dostaje część czaru raz.</summary>
    public sealed class SeventhSealPower : UniquePower
    {
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: łuk musi być w rękach" : null;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "spellarrow.release" || !(data is SpellArrowRelease r)) return false;
            var q4 = pc.Uniques.Get<UnfinishedIncantationPower>();
            if (q4 != null && q4.Active) return false; // runa przejmie payload i użyje pasa przy odpaleniu
            Strip(r.cast, r.from, r.at);
            return true;
        }

        public void Strip(HuCast cast, Vector3 from, Vector3 at)
        {
            from = HuWorld.Ground(from); at = HuWorld.Ground(at);
            float len = Mathf.Max(1f, HuWorld.Flat(from, at));
            float area = Mathf.Max(cast.Area("radius"), Mathf.Max(cast.Area("explosion_radius"), cast.Area("zone_radius", 2f)));
            float share = Mathf.Clamp(area * 2f / len, 0.35f, 1f); // skończona moc rozłożona na długości
            var h = cast.Hit(share);
            h.explosionRadius = 0f;
            int n = HuWorld.HitBand(pc, from, at, P("width") * 0.5f, h, new HitTracker());
            int steps = Mathf.Clamp(Mathf.CeilToInt(len / P("step")), 1, 16);
            for (int i = 0; i <= steps; i++) HellFx.Burst(Vector3.Lerp(from, at, i / (float)steps) + Vector3.up * 0.3f, cast.family, 0.6f);
            Count("pasy"); Count("trafieni", n);
        }
    }

    // ====================================================================== Q02 Kołczan skradzionych żywiołów

    /// <summary>Część zysku reakcji ze Strzały odpowiedzi zostaje ładunkiem; następny ręczny strzał niesie go do nowego celu.</summary>
    public sealed class StolenElementsPower : UniquePower
    {
        float charge, until; Element element; int carryOrigin = -1;
        public float Charge => charge;

        public override float ReactionStore(float gain, Element el)
        {
            if (gain <= 0f) return 0f;
            float take = gain * P("store_share");
            charge = Mathf.Min(charge + take, pc.BasicDamage * 3f);
            element = el; until = pc.Clock + P("charge_seconds");
            Count("odłożone reakcje", take);
            return take;
        }

        public override void AttackStarted(ActionType type, AttackDefinition atk, int origin)
        {
            if (carryOrigin >= 0 && charge > 0f) { Count("ładunek zmarnowany (pudło)", charge); charge = 0f; }
            if (charge > 0f && atk != null && atk.delivery == AttackDelivery.Projectile) carryOrigin = origin;
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (charge <= 0f || hit.originId != carryOrigin || hit.featureEffect || !hit.isProjectile) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            var h = PowerHit("Skradziony żywioł", element == Element.None ? charge : 0f, element == Element.None ? 0f : charge, element, target.AimPoint);
            h.isReaction = true; // nie ładuje nowego ładunku ani reakcji
            Apply(target, h);
            Count("przeniesione reakcje", charge);
            charge = 0f; carryOrigin = -1;
        }

        public override void Tick(float dt) { if (charge > 0f && pc.Clock > until) { Count("ładunek wygasły", charge); charge = 0f; carryOrigin = -1; } }
        public override void Death() { charge = 0f; }
        public override string Hud => charge > 0f ? $"skradziony żywioł: {charge:0}" : null;
    }

    // ====================================================================== Q03 Łuk heretyckiego chóru

    /// <summary>Dodatkowe strzały podstawowe przyjmują żywioł skilli z kolejnych slotów (część obrażeń), główna zostaje fizyczna.</summary>
    public sealed class HereticChoirPower : UniquePower
    {
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: łuk musi być w rękach" : null;

        public static Element SlotElement(PlayerCombat pc, int slot)
        {
            var s = pc.Skill(slot);
            if (s == null) return Element.None;
            if (s.instance.seals != null) return HuCast.ToTurris(s.instance.seals.Shape.Element);
            return s.Def.attack != null ? s.Def.attack.element : Element.None;
        }

        public override void BasicShot(ref HitData hit, int index, int count)
        {
            if (index <= 0) return;
            var el = SlotElement(pc, (index - 1) % RunState.SkillSlotCount);
            if (el == Element.None) return;
            float moved = hit.physical * P("convert_share");
            hit.physical -= moved;
            if (hit.split.active) hit.AddMagic(el, moved); // podział wielu żywiołów (affiksy) – dokładny składnik
            else
            {
                hit.magic += moved;
                if (hit.element != Element.None && hit.element != el) { hit.element2 = hit.element; hit.element2Share = 0.3f; }
                hit.element = el;
            }
            Count("strzały chóru");
        }
    }

    // ====================================================================== Q04 Amulet niedokończonej inkantacji

    /// <summary>Czar ze strzały zostaje runą; ręczny atak innym żywiołem w jej obszarze odpala go zmienionego na żywioł zapalnika.</summary>
    public sealed class UnfinishedIncantationPower : UniquePower
    {
        sealed class Rune { public HuCast cast; public Vector3 at, from; public Element element; public float until; public GameObject fx; }
        Rune rune;
        public bool HasRune => rune != null;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "spellarrow.release" || !(data is SpellArrowRelease r)) return false;
            Clear();
            rune = new Rune { cast = r.cast, at = r.at, from = r.from, element = r.cast.Hit(0f).element, until = pc.Clock + P("rune_seconds") };
            if (FxLibrary.Enabled) rune.fx = VisualFx.GroundMarker(r.at, P("radius"), r.cast.color, r.cast.family);
            Count("runy");
            return true;
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (rune == null || hit.featureEffect || hit.isStatusTick || hit.isReaction || target == null) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            if (HuWorld.Flat(target.Transform.position, rune.at) > P("radius") || hit.element == rune.element) return;
            var r = rune; rune = null;
            if (r.fx != null) Object.Destroy(r.fx);
            pc.uniqueElementOverride = hit.element; // przemiana na żywioł zapalnika (fizyczny – bez zmiany)
            var q1 = pc.Uniques.Get<SeventhSealPower>();
            if (q1 != null && q1.Active) q1.Strip(r.cast, r.from, r.at);
            else pc.UniqueFireHuCast(r.cast, r.cast.direction, r.cast.P.GetF(HuParams.Range, 8f), hit.originId);
            pc.uniqueElementOverride = Element.None;
            Count("odpalone runy");
        }

        void Clear() { if (rune?.fx != null) Object.Destroy(rune.fx); rune = null; }
        public override void Tick(float dt) { if (rune != null && pc.Clock > rune.until) { Count("runy wygasłe"); Clear(); } }
        public override void Death() { Clear(); }
        public override string Hud => rune != null ? $"runa inkantacji {rune.until - pc.Clock:0.0} s" : null;
    }

    // ====================================================================== Q05 Rękawice kołczanu między światami

    /// <summary>Dodatkowe strzały lekkiego ataku trafiają do kołczanu; mocny strzał najpierw uwalnia zgromadzone w swoim kierunku.</summary>
    public sealed class BetweenWorldsPower : UniquePower
    {
        struct Held { public HitData hit; public float speed, until, answerEnds; public Color color; }
        readonly List<Held> quiver = new List<Held>();
        public int Stored => quiver.Count;

        public override bool CaptureShot(in HitData hit, Vector3 origin, Vector3 dir, float speed, Color c, int index, int count)
        {
            var cur = pc.Actions.Current;
            if (cur == ActionType.HeavyAttack && index == 0 && quiver.Count > 0) { Release(origin, dir); return false; }
            if (cur != ActionType.LightAttack || index == 0 || count < 2) return false;
            if (quiver.Count >= PI("capacity")) return false;
            var ans = Feature<AnsweringArrowFeature>();
            quiver.Add(new Held { hit = hit, speed = speed, color = c, until = pc.Clock + P("hold_seconds"), answerEnds = ans != null ? ans.WindowEnds : 0f });
            Count("strzały w kołczanie");
            return true;
        }

        void Release(Vector3 origin, Vector3 dir)
        {
            for (int i = 0; i < quiver.Count; i++)
            {
                var q = quiver[i];
                var h = q.hit;
                if (h.answerCharge && pc.Clock > q.answerEnds) h.answerCharge = false; // odłożenie nie przedłuża okna cechy
                h.spellArrowToken = 0; // dzieci nie kopiują payloadu Strzały zaklęcia
                float ang = quiver.Count == 1 ? 0f : Mathf.Lerp(-10f, 10f, i / (float)(quiver.Count - 1));
                Projectile.Spawn(origin, Quaternion.Euler(0, ang, 0) * dir, q.speed, h, Faction.Player, pc, q.color, 0.12f, 3f, null, 0f, q.hit.element == Element.None);
            }
            Count("uwolnione strzały", quiver.Count);
            quiver.Clear();
        }

        public override void Tick(float dt) { int n = quiver.RemoveAll(q => pc.Clock > q.until); if (n > 0) Count("strzały wygasłe", n); }
        public override void Death() { quiver.Clear(); }
        public override string Hud => quiver.Count > 0 ? $"kołczan: {quiver.Count}/{PI("capacity")}" : null;
    }
}
