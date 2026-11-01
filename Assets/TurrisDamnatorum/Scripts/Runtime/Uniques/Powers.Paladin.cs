using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== P01 Relikwiarz głodnego świętego

    /// <summary>
    /// Wydana Łaska (Rozgrzeszenie) nie tworzy fali: dzieli się na ładunki przypisywane kolejnym ręcznym atakom bronią/podstawowym
    /// strzałom. Ładunek przypisany do ataku przepada przy pudle; salwa dzieli ładunek na pociski. Uwolnienie nie leczy (efekt cechy).
    /// </summary>
    public sealed class HungryReliquaryPower : UniquePower
    {
        readonly Queue<float> charges = new Queue<float>();
        float expires;
        int origin = -1; float budget, perShot; bool ranged;
        public int Charges => charges.Count;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "absolution.release" || amount <= 0f) return false;
            int n = Mathf.Max(1, PI("charges"));
            float each = amount * P("conversion") / n;
            charges.Clear();
            for (int i = 0; i < n; i++) charges.Enqueue(each);
            expires = pc.Clock + P("charge_seconds");
            Count("Łaska w ładunkach", amount * P("conversion"));
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Fire, 0.4f, P("charge_seconds"));
            return true;
        }

        public override void AttackStarted(ActionType type, AttackDefinition atk, int originId)
        {
            if (charges.Count == 0 || (type != ActionType.LightAttack && type != ActionType.HeavyAttack)) return;
            if (budget > 0f) Count("ładunki zmarnowane (pudło)");
            budget = charges.Dequeue(); origin = originId; ranged = atk != null && atk.delivery == AttackDelivery.Projectile; perShot = budget;
        }

        public override void BasicShot(ref HitData hit, int index, int count) { if (index == 0 && budget > 0f) perShot = budget / Mathf.Max(1, count); }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (budget <= 0f || hit.originId != origin || !hit.basicAttack || hit.featureEffect) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            float give = ranged ? Mathf.Min(perShot, budget) : budget;
            budget -= give;
            var h = PowerHit("Łaska w broni", 0f, give, Element.Fire, target.AimPoint);
            h.visualElement = HellFxElement.Fire;
            Apply(target, h);
            FxLibrary.Flash(target.AimPoint, new Color(1f, 0.85f, 0.45f), 0.9f);
            Count("Łaska wydana w ciosach", give);
        }

        public override void Tick(float dt)
        {
            if (charges.Count > 0 && pc.Clock > expires) { Count("ładunki wygasłe", charges.Count); charges.Clear(); }
        }

        public override void Death() { charges.Clear(); budget = 0f; origin = -1; }
        public override string Hud => charges.Count > 0 ? $"Łaska: {charges.Count} ład. ({expires - pc.Clock:0} s)" : null;
    }

    // ====================================================================== P02 Mitra rozdwojonej litanii

    /// <summary>Sąd przysięgi rozdziela aury: każde naciśnięcie w oknie przenosi kolejną aurę na inny cel (logika w JudgementFeature).</summary>
    public sealed class DividedLitanyPower : UniquePower
    {
        protected override string EquipmentDormant() => pc.Auras.Count(a => a != null && a.Skill != null) < 2 ? "do rozdziału potrzebne dwie aktywne aury" : null;

        // Działa także z jedną aurą (wtedy jak zwykły Sąd) – uśpienie jest tylko informacją w opisie.
        public override string Dormant() => Card.requiredFeature != null && !HasFeature(Card.requiredFeature) ? base.Dormant() : null;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "judgement.split") return false;
            amount = P("assign_seconds");
            Count("rozdziały");
            return true;
        }
    }

    // ====================================================================== P03 Habit pustego klasztoru

    /// <summary>Aury wspierające obejmują własne sługi (Zapał – tempo, Męstwo – obrażenia, Odnowa – leczenie śmiertelnych), właściciel – słabiej.</summary>
    public sealed class EmptyMonasteryPower : UniquePower
    {
        float AuraStat(StatType stat)
        {
            float v = 0f;
            foreach (var a in pc.Auras)
            {
                if (a == null || a.Skill == null || (pc.Feature != null && pc.Feature.SuppressesAura(a))) continue;
                foreach (var m in a.HuModifiers()) if (m.stat == stat && m.mode == ModifierMode.Flat) v += m.value;
            }
            return v;
        }

        bool AnyAura => pc.Auras.Exists(a => a != null && a.Skill != null);
        public override float AuraOwnerScale => AnyAura ? P("owner_share") : 1f;
        public override float ServantSpeedMult => 1f / (1f + AuraStat(StatType.AttackSpeed) / 100f * P("servant_share"));
        public override float ServantDamageMult => 1f + AuraStat(StatType.GlobalDamage) / 100f * P("servant_share");

        public override void Tick(float dt)
        {
            if (!Active) return;
            float regen = AuraStat(StatType.HealthRegen) * P("servant_share");
            if (regen <= 0f) return;
            foreach (var s in Servant.Active)
            {
                if (s == null || !s.Mortal || s.Master != pc) continue;
                float healed = s.Heal(regen * dt);
                if (healed > 0f) Count("leczenie sług", healed);
            }
        }
    }

    // ====================================================================== P04 Karwasze dzwonów pokutnych

    /// <summary>Aura świętego ognia nie tyka: jej impulsy narastają (limit czasu), udane parowanie uwalnia je jedną falą ze środka aury.</summary>
    public sealed class PenitentBellsPower : UniquePower
    {
        HitData stored; int pulses; AuraEffect source;
        public int Pulses => pulses;
        protected override string EquipmentDormant() => pc.Auras.Exists(a => a != null && a.Skill != null && a.Skill.id == "hu_aura_holy_fire") ? null : "uśpiona: włącz Aurę świętego ognia";

        public override bool AuraPulse(AuraEffect aura, in HitData hit)
        {
            if (aura == null || aura.Skill == null || aura.Skill.id != "hu_aura_holy_fire") return false;
            int cap = Mathf.Max(1, Mathf.RoundToInt(P("cap_seconds") / Mathf.Max(0.1f, aura.PulseEvery)));
            if (pulses == 0) stored = hit;
            source = aura;
            if (pulses < cap) pulses++;
            else Count("impulsy ponad limit");
            return true;
        }

        public override void Defended(HitOutcome outcome, in HitData hit)
        {
            if (outcome != HitOutcome.Parried) return;
            if (pulses <= 0 || source == null) { Count("puste parowania"); return; }
            var h = stored;
            float k = pulses * P("tick_dps_share", 1f);
            h.physical *= k; h.magic *= k;
            h.isGroundHazard = false; h.uniqueEffect = true; h.featureEffect = true; h.attackName = Card.name;
            Vector3 c = source.CenterPoint;
            h.sourcePosition = c;
            int n = HuWorld.Explode(pc, c, P("radius"), h, new Color(1f, 0.6f, 0.25f));
            HellFx.Shockwave(c, HellFxElement.Fire, new Color(1f, 0.75f, 0.35f), P("radius"), true);
            Sfx.Play(SoundId.Shockwave, c, 0.9f, 0.85f);
            CombatEvents.RaiseMessage($"DZWONY POKUTY ({pulses} imp.)", new Color(1f, 0.75f, 0.35f));
            Count("uwolnione impulsy", pulses); Count("trafieni", n);
            pulses = 0;
        }

        public override void Death() { pulses = 0; source = null; }
        public override string Hud => pulses > 0 ? $"dzwony: {pulses} imp." : null;
    }

    // ====================================================================== P05 Pastorał długiej pokuty

    /// <summary>Część obrażeń technik bronią odkłada się jako wina na ofierze; ręczne leczenie (czar, flaszka) rozlicza całą winę naraz.</summary>
    public sealed class LongPenancePower : UniquePower
    {
        readonly Dictionary<EnemyBrain, float> guilt = new Dictionary<EnemyBrain, float>();
        float lastAdded;
        public float GuiltOn(EnemyBrain e) => e != null && guilt.TryGetValue(e, out var g) ? g : 0f;
        public float TotalGuilt => guilt.Values.Sum();

        protected override string EquipmentDormant() =>
            Run.equipment.Get(EquipSlot.MainHand) != Item && Run.equipment.Get(EquipSlot.OffHand) != Item ? "uśpiona: pastorał musi być w ręce" : null;

        public override void OutgoingAgainst(ref HitData hit, EnemyBrain target)
        {
            if (target == null || !hit.fromMeleeWeapon || hit.basicAttack || hit.featureEffect || hit.isStatusTick || hit.isReaction) return;
            float share = P("guilt_share");
            float moved = hit.Total * share;
            float cap = pc.BasicDamage * P("guilt_cap");
            float have = GuiltOn(target);
            moved = Mathf.Min(moved, Mathf.Max(0f, cap - have));
            if (moved <= 0f) return;
            float k = hit.Total > 0f ? 1f - moved / hit.Total : 1f;
            hit.physical *= k; hit.magic *= k;
            guilt[target] = have + moved;
            lastAdded = pc.Clock;
            Count("wina odłożona", moved);
        }

        public override void ManualHeal(bool flask)
        {
            if (guilt.Count == 0) return;
            float range = P("range");
            int n = 0; float total = 0f;
            foreach (var kv in guilt.ToList())
            {
                var e = kv.Key;
                if (e == null || e.IsDead || HuWorld.Flat(e.transform.position, pc.transform.position) > range) continue;
                var h = PowerHit("Wyrok pokuty", kv.Value, 0f, Element.None, e.AimPoint);
                h.blockable = false;
                Apply(e, h);
                LightningBolt.Strike(e.AimPoint + Vector3.up * 6f, e.AimPoint, new Color(1f, 0.85f, 0.5f));
                n++; total += kv.Value;
            }
            guilt.Clear();
            if (n > 0) CombatEvents.RaiseMessage($"WYROK POKUTY ({n})", new Color(1f, 0.85f, 0.5f));
            Count(flask ? "wyroki z flaszki" : "wyroki z czaru"); Count("wina rozliczona", total);
        }

        public override void Tick(float dt)
        {
            if (guilt.Count > 0 && pc.Clock - lastAdded > P("guilt_seconds")) { Count("wina przedawniona", TotalGuilt); guilt.Clear(); }
            if (guilt.Count > 0) foreach (var k in guilt.Keys.Where(e => e == null || e.IsDead).ToList()) guilt.Remove(k);
        }

        public override void Death() { guilt.Clear(); }
        public override string Hud => guilt.Count > 0 ? $"wina {TotalGuilt:0} ({guilt.Count})" : null;
    }
}
