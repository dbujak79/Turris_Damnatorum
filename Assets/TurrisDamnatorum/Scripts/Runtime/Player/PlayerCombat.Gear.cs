using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Moce unikatów z Hell Unleashed (port <c>GearPowers</c>): ostatnia szansa, wybuch zwłok, aura żaru, ciernie,
    /// łańcuch błyskawic z ciosu bronią, mroźna osłona, pakt krwi i żniwo dusz. Łańcuchy i wybuchy czekają do następnej
    /// klatki – trafiają innych wrogów i nie mogą tego robić z wnętrza zapytania ani zabicia, które je wywołało.
    /// </summary>
    public partial class PlayerCombat
    {
        public const float BloodPactThreshold = 0.35f;
        const float ChainRange = 5f, EmberAuraRadius = 3f, EmberAuraInterval = 0.5f, CorpseRadius = 2.5f, FrostWardRadius = 3.2f;
        const int ChainTargets = 2;

        readonly List<Action> gearPending = new List<Action>();
        float emberTimer;
        bool chaining;
        /// <summary>Ostatnia szansa zużyta na tym piętrze.</summary>
        bool cheatDeathSpent;

        float Power(PassiveEffectType t) => Build != null ? Build.effects[t] : 0f;

        void GearTick(float dt)
        {
            if (gearPending.Count > 0)
            {
                var due = gearPending.ToArray();
                gearPending.Clear();
                foreach (var a in due) a();
            }
            float aura = Power(PassiveEffectType.EmberAura);
            if (aura <= 0f || IsDead) { emberTimer = 0f; return; }
            emberTimer += dt;
            if (emberTimer < EmberAuraInterval) return;
            emberTimer -= EmberAuraInterval;
            var heat = new HitData { magic = aura * EmberAuraInterval, element = Element.Fire, isReaction = true, isStatusTick = true, attacker = this, attackName = "Aura żaru", mods = StatusModifiers.None };
            HuWorld.HitCircle(this, transform.position, EmberAuraRadius, heat);
        }

        /// <summary>Pakt krwi: poniżej 35% życia ciosy bohatera są cięższe.</summary>
        void GearOutgoing(ref HitData hit)
        {
            if (hit.isStatusTick || Health == null) return;
            float pact = Power(PassiveEffectType.LowHealthDamage);
            if (pact > 0f && Health.Fraction < BloodPactThreshold) { hit.physical *= 1f + pact; hit.magic *= 1f + pact; }
        }

        /// <summary>Trafienie bohatera, które doszło: łańcuch błyskawic z ciosu bronią (zwykły atak).</summary>
        void GearOnLanded(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (hit.isStatusTick || hit.isReaction || result.healthDamage <= 0f) return;
            float chain = Power(PassiveEffectType.WeaponChainLightning);
            if (chain > 0f && !chaining && hit.weapon != WeaponModel.Auto)
            {
                float damage = result.healthDamage * chain;
                var from = target;
                gearPending.Add(() => Chain(from, damage));
            }
        }

        void Chain(IHitReceiver from, float damage)
        {
            if (from == null) return;
            Vector3 at = from.Transform.position;
            var near = new List<EnemyBrain>();
            foreach (var e in EnemyBrain.All) if (e != null && !e.IsDead && !ReferenceEquals(e, from) && HuWorld.Flat(e.transform.position, at) <= ChainRange) near.Add(e);
            near.Sort((a, b) => HuWorld.Flat(a.transform.position, at).CompareTo(HuWorld.Flat(b.transform.position, at)));
            chaining = true;
            int arcs = 0;
            foreach (var e in near)
            {
                if (arcs >= ChainTargets) break;
                var h = new HitData { magic = damage, element = Element.Lightning, isReaction = true, attacker = this, sourcePosition = at, attackName = "Łańcuch błyskawic", mods = StatusModifiers.None, dodgeable = true };
                LightningBolt.Strike(from.AimPoint, e.AimPoint, new Color(0.7f, 0.85f, 1f));
                HitQuery.Apply(this, e, h, false);
                arcs++;
            }
            chaining = false;
            if (arcs > 0) Sfx.Play(SoundId.LightningZap, at, 0.8f);
        }

        /// <summary>Wróg zabity przez bohatera: żniwo dusz i wybuch zwłok.</summary>
        void GearOnKill(IHitReceiver target)
        {
            float harvest = Power(PassiveEffectType.SoulHarvest);
            if (harvest > 0f && !IsDead) PassiveHeal(Health.Max * harvest);
            float corpse = Power(PassiveEffectType.CorpseExplosion);
            if (corpse > 0f && target is EnemyBrain enemy && enemy.Health != null)
            {
                float dmg = enemy.Health.Max * corpse;
                Vector3 where = enemy.transform.position;
                gearPending.Add(() =>
                {
                    var burst = new HitData { magic = dmg, element = Element.Fire, isReaction = true, knockback = 1f, attacker = this, attackName = "Wybuch zwłok", mods = StatusModifiers.None, dodgeable = true, visualElement = HellFxElement.Fire };
                    HuWorld.Explode(this, where, CorpseRadius, burst, new Color(1f, 0.5f, 0.15f));
                });
            }
        }

        /// <summary>Bohater dostał cios: ciernie odpowiadają napastnikowi wręcz, mroźna osłona wychładza krąg wokół.</summary>
        void GearOnStruck(in HitData hit, in HitResult r)
        {
            if (r.outcome != HitOutcome.Hit || hit.isStatusTick) return;
            float thorns = Power(PassiveEffectType.Thorns);
            if (thorns > 0f && !hit.isProjectile && hit.attacker is EnemyBrain striker && !striker.IsDead)
            {
                var spikes = new HitData { physical = thorns, isReaction = true, attacker = this, sourcePosition = transform.position, attackName = "Ciernie", mods = StatusModifiers.None, dodgeable = true };
                gearPending.Add(() => { if (striker != null && !striker.IsDead) HitQuery.Apply(this, striker, spikes, false); });
            }
            float frost = Power(PassiveEffectType.FrostWard);
            if (frost > 0f && UnityEngine.Random.value < frost)
            {
                var chill = new HitData { magic = 4f * HuDamageScale, element = Element.Frost, attacker = this, attackName = "Mroźna osłona", mods = StatusModifiers.None, dodgeable = true,
                    statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Chill) }, visualElement = HellFxElement.Water };
                gearPending.Add(() => HuWorld.Explode(this, transform.position, FrostWardRadius, chill, new Color(0.55f, 0.85f, 1f)));
            }
        }

        /// <summary>Śmiertelny cios: ostatnia szansa zostawia bohatera z częścią życia, raz na piętro. true – uratowany.</summary>
        bool TryCheatDeath(float incoming)
        {
            float share = Power(PassiveEffectType.CheatDeath);
            if (share <= 0f || cheatDeathSpent || incoming < Health.Current) return false;
            cheatDeathSpent = true;
            Health.SetCurrent(Health.Max * Mathf.Clamp(share, 0.05f, 1f));
            CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 1.2f, "OSTATNIA SZANSA", new Color(1f, 0.55f, 0.15f));
            HellFx.Shockwave(transform.position, HellFxElement.Fire, new Color(1f, 0.55f, 0.15f), 2.6f);
            HitStop(0.08f);
            return true;
        }

        void GearFloorStart() { gearPending.Clear(); emberTimer = 0f; cheatDeathSpent = false; }
    }
}
