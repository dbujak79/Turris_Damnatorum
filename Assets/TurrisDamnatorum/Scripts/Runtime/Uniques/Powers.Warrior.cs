using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== W01 Pawęż ostatniego zakonnika

    /// <summary>W oknie Walki w gardzie koszt ręcznego bloku najpierw z many (przelicznik), reszta z wytrzymałości – bez podwójnej opłaty.</summary>
    public sealed class LastPavisePower : UniquePower
    {
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.OffHand) != Item ? "uśpiona: pawęż musi być w drugiej ręce" : null;

        public override float BlockStaminaCost(float cost, in HitData hit)
        {
            var guard = Feature<GuardFeature>();
            if (guard == null || !guard.Active || cost <= 0f) return cost;
            float ratio = Mathf.Max(0.01f, P("mana_per_stamina"));
            float pay = Mathf.Min(pc.Mana.Current, cost * ratio);
            if (pay <= 0f) { Count("blok bez many"); return cost; }
            pc.Mana.TrySpend(pay);
            float covered = pay / ratio;
            Count("mana za blok", pay);
            Count("wytrzymałość oszczędzona", covered);
            return Mathf.Max(0f, cost - covered);
        }
    }

    // ====================================================================== W02 Pas dwóch przysiąg

    /// <summary>
    /// Pierwszy celny ręczny atak po zmianie zestawu (Arsenał) wywołuje jeden ślad poprzedniej broni: jej lekki atak × udział,
    /// z jej żywiołem i efektami. Uzbrojenie wymaga trafienia zestawem, który właśnie schowano (spam G nic nie ładuje).
    /// </summary>
    public sealed class TwoOathsPower : UniquePower
    {
        int seenSwaps = -1;
        bool landedWithCurrent;
        ItemInstance echoOf;
        public bool Armed => echoOf != null;
        public ItemInstance EchoWeapon => echoOf;

        public override void Tick(float dt)
        {
            var arsenal = Feature<ArsenalFeature>();
            if (arsenal == null) { seenSwaps = -1; echoOf = null; return; }
            if (seenSwaps < 0) { seenSwaps = arsenal.Swaps; return; }
            if (arsenal.Swaps == seenSwaps) return;
            seenSwaps = arsenal.Swaps;
            // Schowany zestaw jest teraz w arsenale.
            var previous = Run.arsenalMain;
            if (landedWithCurrent && previous != null && previous.definition.IsWeapon) { echoOf = previous; Count("uzbrojenia"); }
            else { echoOf = null; Count("zmiana bez trafienia"); }
            landedWithCurrent = false;
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (!hit.basicAttack || hit.featureEffect || hit.isStatusTick) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            landedWithCurrent = true;
            if (echoOf == null || target == null || target.IsDead) return;
            var w = echoOf; echoOf = null;
            var atk = w.definition.weapon.light;
            var h = pc.UniqueWeaponHit(atk, P("echo_share"), false);
            h.attackName = Card.name;
            h.weapon = AnimResolve.ForItem(w.definition);
            Apply(target, h, true);
            FxLibrary.Flash(target.AimPoint, w.definition.color, 0.9f);
            CombatEvents.RaiseWorldText(target.AimPoint + Vector3.up * 0.6f, "ŚLAD PRZYSIĘGI", new Color(0.9f, 0.8f, 0.6f));
            Count("ślady");
        }

        public override void Death() { echoOf = null; landedWithCurrent = false; }
        public override void FloorStart() { Death(); seenSwaps = -1; }
        public override string Hud => echoOf != null ? $"ślad: {echoOf.Name}" : null;
    }

    // ====================================================================== W03 Hełm dzwonnika

    /// <summary>Riposta jako fala w stożku przed postacią: każdy wróg dostaje udział tego samego budżetu riposty (jeden opłacony atak).</summary>
    public sealed class BellringerPower : UniquePower
    {
        public override bool OverrideRiposte(EnemyBrain target, float damage)
        {
            float share = P("wave_share"), range = P("wave_range"), arc = P("wave_arc");
            Vector3 origin = pc.transform.position, fwd = pc.transform.forward;
            int n = 0;
            foreach (var e in EnemyBrain.All.ToArray())
            {
                if (e == null || e.IsDead || !UniqueGeo.InCone(origin, fwd, e.transform.position, range, arc)) continue;
                if (UniqueGeo.Blocked(origin, e.transform.position)) continue;
                float dmg = damage * share;
                if (e == target) { e.ReceiveRiposte(dmg * pc.ConsumeBreachRiposte(e), pc); n++; continue; }
                var h = new HitData
                {
                    physical = dmg, heavy = true, isRiposte = true, fromMeleeWeapon = true, blockable = false, parryable = false, dodgeable = false,
                    sourcePosition = origin, attacker = pc, attackName = Card.name, poiseDamage = 30f, mods = StatusModifiers.None,
                    featureEffect = true, uniqueEffect = true, originId = pc.ActionOrigin,
                };
                HitQuery.Apply(pc, e, h, true);
                n++;
            }
            HellFx.Cone(origin + Vector3.up * 1.1f + fwd * 0.4f, fwd, HellFxElement.Physical, range, arc);
            Sfx.Play(SoundId.Riposte, pc.AimPoint);
            Sfx.Play(SoundId.Shockwave, pc.AimPoint, 0.6f, 0.8f);
            CombatEvents.RaiseMessage($"RIPOSTA – FALA ({n})", new Color(1f, 0.85f, 0.3f));
            float heal = pc.Build.effects[PassiveEffectType.RiposteHeal];
            if (heal > 0) pc.Health.Restore(heal);
            Count("fale"); Count("trafieni", n);
            return true;
        }
    }

    // ====================================================================== W04 Karwasze pozostawionej warty

    /// <summary>
    /// Ręczny blok/parowanie zapisuje punkt warty; następna ręczna technika bronią (skill HU bez doskoku) rodzi się w nim.
    /// Młynek wiruje w punkcie warty. Jedna warta, normalny koszt, bez teleportu gracza i bez powtórzeń.
    /// </summary>
    public sealed class LeftWatchPower : UniquePower
    {
        Vector3 point, facing; float until = -1f;
        GameObject marker;
        public bool Armed => until > pc.Clock;
        public Vector3 Point => point;

        public override void Defended(HitOutcome outcome, in HitData hit)
        {
            point = pc.transform.position; facing = pc.transform.forward;
            until = pc.Clock + P("watch_seconds");
            ShowMarker();
            Count("warty");
        }

        void ShowMarker()
        {
            if (!FxLibrary.Enabled) return;
            if (marker != null) Object.Destroy(marker);
            var d = FxLibrary.Decal(HuWorld.Ground(point), HellVfxMaterials.Ground(HellFxElement.Physical, false), new Color(0.85f, 0.85f, 1f), 1.6f, 1.6f, P("watch_seconds"));
            marker = d != null ? d.gameObject : null;
        }

        static bool Compatible(SpellRuntime sk) => sk != null && sk.instance.seals != null && !sk.Def.IsSpell && !sk.instance.seals.Shape.HasComponent("Dash") && !sk.Def.IsAura;

        public override void ModifyHuCast(SpellRuntime sk, ref Vector3 origin, ref Vector3 aim, ref Vector3 target)
        {
            if (!Armed || !Compatible(sk) || pc.CurrentSpellRuntime != sk) return;
            if (HuWorld.Flat(point, pc.transform.position) > P("watch_range")) { Fail("za daleko", "Warta: za daleko od punktu"); return; }
            var lockTarget = pc.LockTarget != null && !pc.LockTarget.IsDead ? pc.LockTarget : pc.AssistTarget;
            Vector3 dir = lockTarget != null ? HuWorld.FlatDir(point, lockTarget.transform.position) : facing;
            Vector3 shift = point - origin;
            origin = point; aim = dir; target += shift;
            until = -1f;
            if (marker != null) Object.Destroy(marker);
            Count("techniki z warty");
        }

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            // Młynek: wirowanie w punkcie warty (HuSpin trzyma się ciała – tu stały środek).
            if (cast.copy || cast.Def.IsSpell || !cast.shape.HasComponent("Spin") || HuWorld.Flat(cast.origin, pc.transform.position) < 0.3f) return false;
            Vector3 c = cast.origin;
            var hit = cast.Hit();
            float radius = cast.Area("radius", 2f), interval = Mathf.Max(0.1f, cast.P.GetF("hit_interval", 0.3f)), duration = cast.P.GetF("duration_seconds", 2f);
            var last = new Dictionary<IHitReceiver, float>();
            float t = 0f, swing = 0f;
            HuWorld.Add(new UniqueRuntime(dt =>
            {
                if (pc == null || pc.IsDead) return false;
                t += dt;
                if (t >= duration) return false;
                HuWorld.HitCircle(pc, c, radius, hit, null, r => { if (last.TryGetValue(r, out float w) && t - w < interval) return false; last[r] = t; return true; });
                swing -= dt;
                if (swing <= 0f) { swing = 0.25f; FxLibrary.Dust(c, 0.8f); Sfx.Play(SoundId.SwingLight, c + Vector3.up, 0.6f); }
                return true;
            }));
            if (FxLibrary.Enabled) HuWorld.Add(new HuTelegraphRuntime(VisualFx.Ring(HuWorld.Ground(c), radius, new Color(0.85f, 0.85f, 1f), duration), duration));
            Count("młynek z warty");
            return true;
        }

        public override void Death() { until = -1f; if (marker != null) Object.Destroy(marker); }
        public override string Hud => Armed ? $"warta {until - pc.Clock:0.0} s" : null;
    }

    // ====================================================================== W05 Medalion pustego frontu

    /// <summary>Szarża (hu_charge) biegnie od celu; wrogowie na trasie dostają trafienie szarży raz, bez końcowego wybuchu.</summary>
    public sealed class EmptyFrontPower : UniquePower
    {
        protected override string EquipmentDormant() => NeedSkill(TurrisSkills.ChargeTarget);

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (cast.copy || cast.relayed || !IsSkill(cast.spell, TurrisSkills.ChargeTarget)) return false;
            Vector3 back = -cast.direction;
            var retreat = new HuCast(pc, cast.spell, cast.origin, back, cast.origin + back * range, cast.power);
            pc.HuStartDash(new HuDash(retreat, false));
            var hit = cast.Hit(P("dash_share", 1f));
            hit.knockdown = 0f; hit.knockdownChance = 0f;
            var tracker = new HitTracker();
            Vector3 last = pc.transform.position;
            float t = 0f, seconds = Mathf.Max(0.05f, cast.P.GetF("dash_seconds", 0.2f)) + 0.1f;
            HuWorld.Add(new UniqueRuntime(dt =>
            {
                if (pc == null || pc.IsDead) return false;
                t += dt;
                Vector3 now = pc.transform.position;
                // Taran osłania przód: trafia ścigających po stronie celu (za plecami biegu), każdego raz.
                HuWorld.HitBand(pc, last - back * 0.8f, now - back * 0.8f, 1.1f, hit, tracker);
                last = now;
                return t < seconds;
            }));
            FxLibrary.Dust(pc.transform.position, 1.2f);
            Count("odwroty");
            return true;
        }
    }
}
