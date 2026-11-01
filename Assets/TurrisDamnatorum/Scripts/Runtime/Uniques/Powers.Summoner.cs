using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== S01 Maska trzeciego głodu

    /// <summary>W formie Wcielenia mocny atak zużywa najbliższego sługę innego rodzaju i przejmuje jego zdolność (do końca formy).</summary>
    public sealed class ThirdHungerPower : UniquePower
    {
        ServantKind? borrowed;
        public ServantKind? Borrowed => borrowed;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "incarnation.heavy" || !(data is IncarnationStrike s)) return false;
            if (!borrowed.HasValue)
            {
                var other = Servant.Active.Where(x => x != null && x.Master == pc && !x.IsChimera && x.Kind != s.formKind)
                    .OrderBy(x => HuWorld.Flat(x.transform.position, pc.transform.position)).FirstOrDefault();
                if (other == null || HuWorld.Flat(other.transform.position, pc.transform.position) > 12f) { Count("brak drugiego sługi"); return false; }
                borrowed = other.Kind;
                other.Dismiss(); // drugi realny ubytek armii (bez naturalnego końca)
                CombatEvents.RaiseMessage($"TRZECI GŁÓD: {ClassFeatureCatalog.ServantName(other.Kind)}", new Color(0.45f, 1f, 0.65f));
                Count("zjedzeni słudzy");
            }
            var h = s.hit;
            float k = 2f * P("ability_share");
            h.physical *= k; h.magic *= k;
            var kind = borrowed.Value;
            if (kind == ServantKind.Imp) { h.element = Element.Fire; h.magic += h.physical; h.physical = 0f; h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Burn) }; }
            else if (kind == ServantKind.Hound) h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Bleed, 2) };
            else h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Stagger, 2) };
            Servant.Deliver(pc, kind, h, pc.transform, s.target, kind == ServantKind.Hound ? 1.6f : kind == ServantKind.Golem ? 1.9f : 9f, new Color(0.45f, 1f, 0.65f), 1.6f);
            Count("mocne ataki zdolnością");
            return true;
        }

        public override void Tick(float dt)
        {
            var inc = Feature<IncarnationFeature>();
            if (borrowed.HasValue && (inc == null || !inc.InForm)) borrowed = null; // koniec formy – koniec zdolności
        }

        public override void Death() { borrowed = null; }
        public override string Hud => borrowed.HasValue ? $"głód: {ClassFeatureCatalog.ServantName(borrowed.Value)}" : null;
    }

    // ====================================================================== S02 Serce szóstego żebra

    /// <summary>Chimera ma prawdziwe zdrowie z wkładów i przyciąga wrogów; ponowne scalenie zachowuje bieżące zdrowie (bez pełnego odnowienia).</summary>
    public sealed class SixthRibPower : UniquePower
    {
        Servant chimera; FeatureDecoy taunt;
        readonly HashSet<Servant.Part> known = new HashSet<Servant.Part>();
        float knownHealth, retaunt;
        public Servant Chimera => chimera != null && Servant.Active.Contains(chimera) ? chimera : null;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "chimera.spawned" || !(data is Servant ch)) return false;
            float hp = 0f;
            foreach (var p in ch.Parts)
                hp += known.Contains(p) ? 0f : p.remaining * P("hp_per_second");
            if (ch.Parts.Any(p => known.Contains(p))) hp += knownHealth; // wcześniejsza chimera wnosi bieżące zdrowie, nie pełne
            ch.MakeMortal(hp, P("max_seconds"));
            chimera = ch;
            known.Clear(); foreach (var p in ch.Parts) known.Add(p);
            knownHealth = ch.HealthCurrent;
            taunt?.End();
            taunt = FeatureDecoy.SpawnOn(pc, ch.transform, P("max_seconds"), 6, P("taunt_range"), h => ch != null ? ch.TakeDamage(h.Total) : 0f);
            Count("śmiertelne chimery"); Count("zdrowie chimer", hp);
            return false;
        }

        public override void ManualHeal(bool flask)
        {
            var c = Chimera;
            if (flask || c == null || HuWorld.Flat(c.transform.position, pc.transform.position) > 8f) return;
            float healed = c.Heal(c.HealthMax * 0.25f); // leczenie dzielone: czar leczy też bliską chimerę
            if (healed > 0f) { FxLibrary.Heal(c.transform, new Color(0.75f, 1f, 0.45f), 0.6f); Count("leczenie chimery", healed); }
        }

        public override void Tick(float dt)
        {
            var c = Chimera;
            if (c == null) { if (taunt != null) { taunt.End(); taunt = null; } chimera = null; return; }
            knownHealth = c.HealthCurrent;
            retaunt -= dt;
            if (retaunt <= 0f && taunt != null && taunt.Alive) { retaunt = 1.5f; taunt.Deceive(P("taunt_range")); }
        }

        public override void Death() { taunt?.End(); taunt = null; chimera = null; known.Clear(); }
        public override string Hud => Chimera != null ? $"chimera {Chimera.HealthCurrent:0}/{Chimera.HealthMax:0} PŻ" : null;
    }

    // ====================================================================== S03 Rękawice kościanego dyrygenta

    /// <summary>Sługi atakują tylko na komendę: każde ręczne trafienie właściciela wysyła kolejnego sługę (jedna komenda na akcję).</summary>
    public sealed class BoneConductorPower : UniquePower
    {
        int next, lastOrigin = -1;
        public override bool CommandsServants => true;

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (hit.featureEffect || hit.isStatusTick || hit.isReaction || hit.originId == lastOrigin) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            var mine = Servant.Active.Where(s => s != null && s.Master == pc).ToList();
            if (mine.Count == 0) return;
            lastOrigin = hit.originId;
            for (int i = 0; i < mine.Count; i++)
            {
                var s = mine[(next + i) % mine.Count];
                if (s.Command(P("min_interval_share"))) { next = (next + i + 1) % Mathf.Max(1, mine.Count); Count("komendy"); return; }
            }
            Count("komendy bez gotowego sługi");
        }
    }

    // ====================================================================== S04 Grymuar otwartych pysków

    /// <summary>Ręczny czar pociskowy wychodzi z kolejnego własnego sługi (rotacja); kosztuje sługę czas życia (śmiertelną chimerę – zdrowie).</summary>
    public sealed class OpenMawsPower : UniquePower
    {
        int next;
        public override void ModifyHuCast(SpellRuntime sk, ref Vector3 origin, ref Vector3 aim, ref Vector3 target)
        {
            if (!CastKinds.Offensive(sk) || !sk.instance.seals.Shape.HasComponent("Projectile")) return;
            var mine = Servant.Active.Where(s => s != null && s.Master == pc && HuWorld.Flat(s.transform.position, pc.transform.position) <= 14f).ToList();
            if (mine.Count == 0) return;
            var src = mine[next % mine.Count]; next++;
            var e = pc.LockTarget != null && !pc.LockTarget.IsDead ? pc.LockTarget : pc.AssistTarget != null ? pc.AssistTarget : HuWorld.Nearest(src.transform.position, 14f);
            Vector3 from = src.transform.position;
            Vector3 dir = e != null ? HuWorld.FlatDir(from, e.transform.position) : aim;
            target += from - origin; origin = from; aim = dir;
            src.SpendLife(P("servant_cost"), P("chimera_hp"));
            FxLibrary.Flash(from + Vector3.up, new Color(0.45f, 1f, 0.65f), 0.7f);
            Count("czary z pysków");
        }
    }

    // ====================================================================== S05 Pas dziedziczonej bestii

    /// <summary>Naturalny koniec sługi zostawia ślad; następne przywołanie innego rodzaju dziedziczy jego właściwość (jeden status).</summary>
    public sealed class InheritedBeastPower : UniquePower
    {
        ServantKind? trace; float until;
        protected override void OnBind() { Servant.Expired += OnExpired; }
        public override void Dispose() { Servant.Expired -= OnExpired; base.Dispose(); }

        void OnExpired(Servant s)
        {
            if (!Active || s == null || s.Master != pc || s.IsChimera) return;
            trace = s.Kind; until = pc.Clock + P("trace_seconds");
            Count("ślady dziedziczenia");
        }

        public override void CastFired(SpellRuntime sk, int origin)
        {
            if (!trace.HasValue || sk?.instance.seals == null || !sk.instance.seals.Shape.HasComponent("Summon")) return;
            if (pc.Clock > until) { trace = null; return; }
            var fresh = Servant.Active.Where(s => s != null && s.Master == pc && s.Age < 0.1f && !s.IsChimera).ToList();
            if (fresh.Count == 0 || fresh[0].Kind == trace.Value) { Count("ten sam rodzaj – bez dziedziczenia"); return; }
            var status = trace.Value == ServantKind.Imp ? StatusKind.Burn : trace.Value == ServantKind.Hound ? StatusKind.Bleed : StatusKind.Stagger;
            foreach (var s in fresh) s.Inherited = status;
            CombatEvents.RaiseMessage($"DZIEDZICTWO: {Names.Status(status)}", new Color(0.45f, 1f, 0.65f));
            trace = null;
            Count("dziedziczenia");
        }

        public override void Death() { trace = null; }
        public override string Hud => trace.HasValue && pc.Clock < until ? $"dziedzictwo: {ClassFeatureCatalog.ServantName(trace.Value)}" : null;
    }

    // ====================================================================== T01 Bandaże runicznego zapaśnika

    /// <summary>Puste dłonie są orężem Arkanów; ofensywne czary w zwarciu stają się uderzeniami (pocisk – wybuchający prosty, nova – obrót).</summary>
    public sealed class RunicWrestlerPower : UniquePower
    {
        protected override string EquipmentDormant() =>
            Run.equipment.Get(EquipSlot.MainHand) != null || Run.equipment.Get(EquipSlot.OffHand) != null ? "uśpiona: wymaga pustych dłoni" : null;

        public override bool FeatureEvent(string evt, ref float amount, object data) => evt == "arcana.fists";

        readonly Dictionary<AttackDefinition, AttackDefinition> fists = new Dictionary<AttackDefinition, AttackDefinition>();

        /// <summary>Bandaże: zwykłe ciosy pięścią zadają więcej (ta sama animacja i tempo; moc jest uśpiona z czymkolwiek w rękach).</summary>
        public override AttackDefinition AttackOverride(ActionType type, AttackDefinition atk)
        {
            if (atk == null || atk.delivery != AttackDelivery.Melee || (type != ActionType.LightAttack && type != ActionType.HeavyAttack)) return atk;
            if (!fists.TryGetValue(atk, out var strong))
            {
                strong = atk.Clone();
                strong.baseDamage *= P("fist_mult", 1f);
                fists[atk] = strong;
            }
            return strong;
        }

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || !CastKinds.Offensive(cast.spell) || Run == null || !Run.weaponArcanaActive) return false;
            Vector3 me = pc.transform.position;
            if (cast.shape.HasComponent("Projectile"))
            {
                Vector3 at = me + aim * P("punch_reach");
                var h = cast.Hit(1f); h.fromMeleeWeapon = true; h.explosionRadius = 0f;
                int n = HuWorld.Explode(pc, at, P("punch_radius"), h, cast.color);
                HellFx.Burst(at + Vector3.up, cast.family, P("punch_radius"));
                pc.PlayFeatureSwing(true);
                Count("proste z czaru"); Count("trafieni", n);
                return true;
            }
            if (CastKinds.IsNova(cast))
            {
                var h = cast.Hit(1f); h.fromMeleeWeapon = true;
                int n = HuWorld.Explode(pc, me, cast.Area("radius", 3f), h, cast.color);
                HellFx.Shockwave(me, cast.family, cast.color, cast.Area("radius", 3f), true);
                pc.PlayFeatureSwing(true);
                Count("obroty z novy"); Count("trafieni", n);
                return true;
            }
            return false; // pozostałe kształty działają normalnie
        }
    }

    // ====================================================================== T02 Miecz niedokończonej runy

    /// <summary>Czar z toru Dwóch taktów zostawia pieczęć; następny mocny atak tnie odcinek do niej zamiast łuku.</summary>
    public sealed class UnfinishedRunePower : UniquePower
    {
        Vector3 point; float until = -1f; GameObject mark;
        bool cutting;
        public bool Armed => until > pc.Clock;
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: miecz musi być w głównej ręce" : null;

        public override void ModifyHuCast(SpellRuntime sk, ref Vector3 origin, ref Vector3 aim, ref Vector3 target)
        {
            var beats = Feature<TwoBeatsFeature>();
            if (beats == null || !beats.Active || !CastKinds.Offensive(sk)) return;
            bool lane = !ReferenceEquals(pc.CurrentSpellRuntime, sk) || pc.Actions.Current != ActionType.Cast;
            if (!lane) return;
            var shape = sk.instance.seals.Shape;
            point = shape.Targeting == HuTargeting.Point ? HuWorld.Ground(target) : HuWorld.Clamp(origin, origin + aim * Mathf.Min(shape.Parameters.GetF(HuParams.Range, 8f), P("max_length")));
            until = pc.Clock + P("point_seconds");
            if (mark != null) Object.Destroy(mark);
            if (FxLibrary.Enabled) mark = VisualFx.GroundMarker(point, 0.7f, new Color(0.55f, 0.65f, 1f), HellFxElement.Air);
            Count("pieczęcie");
        }

        public override bool OverrideSweepHit(IHitReceiver target, AttackDefinition atk) => cutting && pc.Actions.Current == ActionType.HeavyAttack;

        public override void AttackStarted(ActionType type, AttackDefinition atk, int origin) => cutting = type == ActionType.HeavyAttack && Armed;

        public override void AttackActive(ActionType type, AttackDefinition atk, int origin)
        {
            if (type != ActionType.HeavyAttack || !cutting) return;
            Vector3 a = pc.transform.position;
            Vector3 b = point;
            if (HuWorld.Flat(a, b) > P("max_length")) b = a + HuWorld.FlatDir(a, b) * P("max_length");
            var h = pc.UniqueWeaponHit(atk, P("line_mult"), false);
            h.attackName = "Niedokończona runa"; h.featureEffect = false; h.uniqueEffect = true; h.heavy = true;
            int n = 0;
            foreach (var e in UniqueGeo.EnemiesOnSegment(a, b, P("width") * 0.5f))
                if (!UniqueGeo.Blocked(a, e.transform.position)) { Apply(e, h, true); n++; }
            LightningBolt.Strike(a + Vector3.up, b + Vector3.up, new Color(0.6f, 0.7f, 1f));
            until = -1f; cutting = false;
            if (mark != null) Object.Destroy(mark);
            Count("cięcia do pieczęci"); Count("trafieni", n);
        }

        public override void Death() { until = -1f; cutting = false; if (mark != null) Object.Destroy(mark); }
        public override string Hud => Armed ? $"pieczęć {until - pc.Clock:0.0} s" : null;
    }

    // ====================================================================== T03 Zbroja runożercy

    /// <summary>Własne pęknięcie pancerza obniża odporność na żywioły (za warstwę) zamiast fizycznej.</summary>
    public sealed class RuneEaterPower : UniquePower
    {
        public override float SunderMagicPerStack => P("magic_per_stack");
        public override void OutgoingLate(ref HitData hit)
        {
            if (hit.statuses != null && hit.statuses.Exists(s => s.kind == StatusKind.Sunder)) { hit.mods.sunderMagicPerStack = P("magic_per_stack"); Count("magiczne pęknięcia"); }
        }
    }

    // ====================================================================== T04 Obrączka parującej runy

    /// <summary>Po udanym parowaniu riposta zostaje zastąpiona czarem z wybranego slotu (normalna mana, premia riposty).</summary>
    public sealed class ParryingRunePower : UniquePower
    {
        float armedUntil = -1f;
        public int Slot => Mathf.Clamp(Item != null ? Item.powerSlot : PI("slot"), 0, RunState.SkillSlotCount - 1);

        public override void Defended(HitOutcome outcome, in HitData hit) { if (outcome == HitOutcome.Parried) armedUntil = pc.Clock + 3f; }

        public override bool OverrideRiposte(EnemyBrain target, float damage)
        {
            if (armedUntil < pc.Clock) return false;
            armedUntil = -1f;
            var sk = pc.Skill(Slot);
            if (sk == null || !sk.Def.IsSpell || sk.Def.IsAura) { Fail("brak czaru", $"Parująca runa: slot {Slot + 1} bez czaru – zwykła riposta"); return false; }
            if (!sk.instance.Ready || !sk.equipmentMet) { Fail("czar niegotowy", $"Parująca runa: {sk.Def.displayName} niegotowy – zwykła riposta"); return false; }
            if (pc.Mana.Current + 0.01f < sk.manaCost) { Fail("brak many", "Parująca runa: za mało many – zwykła riposta"); return false; }
            pc.Mana.TrySpend(sk.manaCost);
            sk.instance.TryUse();
            target.ReceiveRiposte(0f, pc); // okno riposty zużyte – brak fizycznej riposty równolegle
            var prevLock = pc.LockTarget;
            pc.LockTarget = target;
            pc.uniqueCastPower = 1f + pc.Build.stats[StatType.RiposteDamage] / 100f * P("riposte_bonus_share");
            pc.UniqueFireSpell(sk);
            pc.uniqueCastPower = 1f;
            pc.LockTarget = prevLock;
            CombatEvents.RaiseMessage($"RIPOSTA: {sk.Def.displayName}", new Color(0.55f, 0.65f, 1f));
            Count("riposty-czary");
            return true;
        }

        public override void Death() { armedUntil = -1f; }
    }

    // ====================================================================== T05 Grymuar złamanej różdżki

    /// <summary>Lekki atak różdżki to cięcie przywołanym ostrzem jej żywiołu; mocny pozostaje ostrzałem.</summary>
    public sealed class BrokenWandPower : UniquePower
    {
        readonly Dictionary<AttackDefinition, AttackDefinition> blades = new Dictionary<AttackDefinition, AttackDefinition>();

        protected override string EquipmentDormant()
        {
            var main = Run.equipment.Get(EquipSlot.MainHand);
            if (main == null || !main.definition.IsWeapon || main.definition.twoHanded || main.definition.weapon.light.delivery != AttackDelivery.Projectile) return "uśpiona: wymaga jednoręcznej różdżki w głównej ręce";
            return null;
        }

        public override AttackDefinition AttackOverride(ActionType type, AttackDefinition atk)
        {
            if (type != ActionType.LightAttack || atk == null || atk.delivery != AttackDelivery.Projectile || pc.AttackIsOffhand) return atk;
            if (!blades.TryGetValue(atk, out var b))
            {
                b = atk.Clone();
                b.delivery = AttackDelivery.Melee; b.reach = P("reach"); b.radius = P("radius");
                b.baseDamage = atk.baseDamage * P("blade_mult"); b.animation = AttackAnim.SlashRight; b.projectileCount = 1;
                b.name = "Ostrze różdżki";
                blades[atk] = b;
            }
            Count("cięcia ostrzem");
            return b;
        }
    }
}
