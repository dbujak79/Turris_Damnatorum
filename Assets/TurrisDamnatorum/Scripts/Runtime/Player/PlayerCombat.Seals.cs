using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Skille Hell Unleashed wykonywane z komponentów i pieczęci (<see cref="HuSkillCaster"/>): budowa trafienia z parametrów
    /// HU, doskok i młynek, które niosą bohatera, bariera, hart, przeskok, wyzwalacze oraz koszt w zdrowiu.
    /// Jedno źródło obrażeń: parametry przerobionego kształtu × 2,5 (skala Turris) × cechy/moc czarów (albo premia fizyczna).
    /// </summary>
    public partial class PlayerCombat
    {
        /// <summary>Obrażenia HU → Turris (jak przy przeniesionych skillach i demonach).</summary>
        public const float HuDamageScale = 2.5f;
        /// <summary>Zdrowie HU → Turris (bariery, leczenie płaskie): zdrowie bohatera Turris to ok. 2× HU.</summary>
        public float HuHealthScale => 2f;

        HuSpin huSpin;
        HuDash huDash;
        bool barrierBroken;

        /// <summary>Młynek HU trwa: bohater może się poruszać z tym mnożnikiem; null – brak.</summary>
        public float? SkillMoveMultiplier => huSpin != null && Actions.Current == ActionType.Cast ? huSpin.moveMultiplier : (float?)null;

        bool IsHu(SpellRuntime sk) => sk != null && sk.instance.seals != null;

        /// <summary>Czas fazy aktywnej skilla HU: doskok i młynek trwają, reszta działa w jednej chwili.</summary>
        float HuActiveTime(SpellRuntime sk)
        {
            var shape = sk.instance.seals.Shape;
            if (shape.HasComponent("Dash")) return Mathf.Max(0.05f, shape.Parameters.GetF("dash_seconds", 0.2f));
            if (shape.HasComponent("Spin")) return Mathf.Max(0.1f, shape.Parameters.GetF("duration_seconds", 2f));
            return 0.05f;
        }

        /// <summary>Rzut skilla HU w fazie aktywnej: celowanie, przeciążenie i wykonanie komponentów.</summary>
        void FireHu(SpellRuntime spell)
        {
            var shape = spell.instance.seals.Shape;
            Vector3 aim = AimDirection();
            float range = shape.Parameters.GetF(HuParams.Range, 8f);
            Vector3 target = shape.Targeting == HuTargeting.Point ? HuPointTarget(aim, range) : transform.position + aim * range;
            float power = overloadThisCast ? OverloadPower : 1f;
            HuSkillCaster.CastByHand(this, spell, aim, target, power);
        }

        /// <summary>Punkt dla skilla na miejsce: namierzony (albo wskazany pomocą celowania) wróg w zasięgu, inaczej średnia odległość.</summary>
        Vector3 HuPointTarget(Vector3 aim, float range)
        {
            var e = HuAimTarget(range, 60f, transform.position, aim);
            if (e != null) return HuWorld.Ground(e.transform.position);
            return HuWorld.Clamp(transform.position, transform.position + aim * Mathf.Min(range, 6f));
        }

        public EnemyBrain HuAimTarget(float range, float angle, Vector3 origin, Vector3 dir)
        {
            if (LockTarget != null && !LockTarget.IsDead && HuWorld.Flat(LockTarget.transform.position, origin) <= range) return LockTarget;
            if (AssistTarget != null && !AssistTarget.IsDead && HuWorld.Flat(AssistTarget.transform.position, origin) <= range) return AssistTarget;
            return HuWorld.AimTarget(origin, dir, range, angle);
        }

        /// <summary>Trafienie skilla HU z parametrów przerobionego kształtu (część <paramref name="share"/> mocy rzutu).</summary>
        public HitData HuHit(HuCast cast, float share, bool projectile, bool plain)
        {
            var p = cast.P;
            float scale = HuDamageScale * cast.spell.huScale * cast.power * share;
            float phys = (float)p.Damage(HuElement.Physical) * scale;
            // Część żywiołowa: główny żywioł i drugi z udziałem (HU: osobne składowe, np. strzała żywiołów ogień + powietrze).
            HuElement dominant = HuElement.Physical, second = HuElement.Physical; double best = 0, next = 0;
            foreach (HuElement e in new[] { HuElement.Fire, HuElement.Water, HuElement.Air, HuElement.Earth })
            {
                double d = p.Damage(e);
                if (d > best) { next = best; second = dominant; best = d; dominant = e; }
                else if (d > next) { next = d; second = e; }
            }
            float mag = (float)(p.TotalDamage - p.Damage(HuElement.Physical)) * scale;
            var def = cast.Def;
            var hit = new HitData
            {
                physical = phys, magic = mag, element = HuCast.ToTurris(dominant),
                element2 = next > 0 ? HuCast.ToTurris(second) : Element.None, element2Share = best + next > 0 ? (float)(next / (best + next)) : 0f,
                guardLoad = def.attack.guardLoad, poiseDamage = def.attack.poiseDamage * Mathf.Clamp(share * cast.power, 0.2f, 2f),
                blockable = true, parryable = false, dodgeable = true, heavy = def.attack.heavy,
                sourcePosition = cast.origin, attacker = this, attackName = def.displayName,
                visualElement = cast.family, mods = StatusModifiers.None,
                isProjectile = projectile, huTriggered = cast.triggered,
            };
            if (!plain)
            {
                hit.knockback = p.GetF(HuParams.Knockback);
                hit.knockdown = p.GetF("down_seconds");
                hit.knockdownChance = p.GetF("down_chance");
                // Szarża z tarczą kończy się uderzeniem tarczą: powala zawsze, na dłużej, i mocniej łamie postawę.
                if (p.On("shield_bash_down") && HasShield)
                {
                    hit.knockdown = p.GetF("shield_bash_down");
                    hit.knockdownChance = 0f;
                    hit.poiseDamage *= 1.5f;
                }
                hit.bonusVsBleeding = p.GetF("bonus_vs_bleeding");
                hit.bonusVsChilled = p.GetF("bonus_vs_soaked");
                hit.statuses = HuStatuses(p);
            }
            if (!def.IsSpell && !projectile && !cast.Data.tags.Contains("spell") && cast.Data.tags.Contains("melee")) { hit.fromMeleeWeapon = true; ApplyWeaponBuff(ref hit); }
            PrepareOutgoing(ref hit);
            return hit;
        }

        /// <summary>Efekty HU → Turris: podpalenie → podpalenie, przemoczenie i spowolnienie osobno (bez zamrożenia), reszta bez zmian.</summary>
        static List<StatusApplication> HuStatuses(HuParams p)
        {
            List<StatusApplication> list = null;
            foreach (var kv in p.Effects)
            {
                StatusKind? kind = null;
                switch (kv.Key)
                {
                    case "Bleed": kind = StatusKind.Bleed; break;
                    case "Ignite": kind = StatusKind.Burn; break;
                    case "Soaked": kind = StatusKind.Soaked; break;
                    case "Slow": kind = StatusKind.Slow; break;
                    case "Stagger": kind = StatusKind.Stagger; break;
                    case "Stun": kind = StatusKind.Stun; break;
                    case "Sunder": kind = StatusKind.Sunder; break;
                }
                if (kind.HasValue) (list ?? (list = new List<StatusApplication>())).Add(new StatusApplication(kind.Value, kv.Value));
            }
            return list;
        }

        /// <summary>Płaci za skill odpalony wyzwalaczem: część many/wytrzymałości albo cały koszt w zdrowiu. false – brak środków.</summary>
        public bool HuPay(SpellRuntime sk, float share)
        {
            float life = Health.Max * sk.instance.seals.Shape.Parameters.GetF(HuParams.LifeCost);
            if (life > 0f)
            {
                if (Health.Current <= life) return false;
                Health.Drain(Mathf.Min(life, Health.Current - 1f));
                return true;
            }
            if (sk.Def.IsSpell)
            {
                float cost = sk.manaCost * share;
                if (Mana.Current + 0.01f < cost) return false;
                Mana.TrySpend(cost);
                return true;
            }
            if (Stamina.Current <= 0f) return false;
            Stamina.Drain(sk.staminaCost * share, B.staminaRegenDelay);
            return true;
        }

        /// <summary>Koszt rzutu w zdrowiu (Ofiara krwi, Pakt z diabłem) – zamiast many i wytrzymałości; ostatniego punktu nie zabiera.</summary>
        float HuLifeCost(SpellRuntime sk) => IsHu(sk) ? Health.Max * sk.instance.seals.Shape.Parameters.GetF(HuParams.LifeCost) : 0f;

        public void HuCastFx(SpellRuntime sk, Vector3 aim)
        {
            PlaySkillSound(sk.Def);
            HellSkillFx.Cast(sk.Def, Hand.position, aim);
        }

        public void HuSwing(bool heavy) => PlaySwing(heavy);

        public void HuTeleport(Vector3 point, Color color)
        {
            if (IsDead) return;
            HellFx.BlinkSparks(transform.position, color, true);
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.position = HuWorld.Ground(point);
            if (cc != null) cc.enabled = true;
            HellFx.BlinkSparks(transform.position, color, false);
        }

        public void RaiseBarrier(float amount, float seconds, Color color)
        {
            if (amount <= 0f) return;
            if (BarrierAmount <= 0f && FxLibrary.Enabled) barrierFx = HellFx.Ambient(transform, Vector3.up, HellFx.FromColor(color), 0.7f);
            BarrierAmount = Mathf.Max(BarrierAmount, amount);
            BarrierTime = Mathf.Max(BarrierTime, seconds);
            barrierBroken = false;
        }

        /// <summary>Czy osłona właśnie pękła od ciosu (raz – odczyt zeruje).</summary>
        public bool ConsumeBarrierBroken() { bool b = barrierBroken; barrierBroken = false; return b; }

        /// <summary>Hart: przez chwilę mniej otrzymanych obrażeń (dzieli licznik z okrzykiem – liczy się mocniejszy).</summary>
        public void GrantReduction(float fraction, float seconds)
        {
            cryReduction = Mathf.Max(cryReduction, fraction);
            cryTime = Mathf.Max(cryTime, seconds);
        }

        public void HuStartSpin(HuSpin spin) { huSpin = spin; HuWorld.Add(spin); }
        public void HuStartDash(HuDash dash) { huDash = dash; HuWorld.Add(dash); }
        /// <summary>Czy bohater trzyma tarczę (garda tarczą).</summary>
        public bool HasShield => Build != null && Build.guardIsShield;
        /// <summary>Redukcja obrażeń w trakcie doskoku (<c>dash_reduction</c>, np. Szarża 50%).</summary>
        public float DashReduction => huDash != null && huDash.Running ? huDash.Reduction : 0f;

        /// <summary>Faza aktywna skilla HU: doskok niesie bohatera, młynek pozwala iść wolniej.</summary>
        void HuActiveTick()
        {
            if (huDash != null && Actions.Phase == ActionPhase.Active) SkillMotion = huDash.MotionFor(Time.deltaTime);
            else SkillMotion = Vector3.zero;
        }

        readonly List<Collider> ignoredEnemyColliders = new List<Collider>();

        /// <summary>Doskok HU: kontroler bohatera nie zderza się z ciałami wrogów (ściany dalej zatrzymują).</summary>
        public void SetPassThroughEnemies(bool on)
        {
            var cc = GetComponent<CharacterController>();
            if (cc == null) return;
            if (!on)
            {
                foreach (var c in ignoredEnemyColliders) if (c != null) Physics.IgnoreCollision(cc, c, false);
                ignoredEnemyColliders.Clear();
                return;
            }
            foreach (var e in EnemyBrain.All)
            {
                if (e == null) continue;
                foreach (var c in e.GetComponentsInChildren<Collider>())
                {
                    if (c == null || c.isTrigger || ignoredEnemyColliders.Contains(c)) continue;
                    Physics.IgnoreCollision(cc, c, true);
                    ignoredEnemyColliders.Add(c);
                }
            }
        }

        /// <summary>Koniec akcji skilla HU: doskok i młynek przestają nieść bohatera.</summary>
        void HuActionEnded()
        {
            huDash?.Stop(); huDash = null;
            if (huSpin != null && Actions.Current != ActionType.Cast) { huSpin.Stop(); huSpin = null; }
        }

        public void HuWarcry(HuCast cast)
        {
            GrantReduction(cast.P.GetF("reduction", 0.2f), cast.P.GetF("duration_seconds", 5f));
            AddFury(cast.P.GetF("fury", 35f));
            CombatEvents.RaiseMessage("OKRZYK!", cast.color);
        }

        public void HuOverload(HuCast cast)
        {
            OverloadReady = true;
            CombatEvents.RaiseMessage("PRZECIĄŻENIE", cast.Def.color);
            HellSkillFx.Shockwave(cast.Def, transform.position, 1.4f, false);
        }

        public void HuUndying(HuCast cast)
        {
            IronSkinTime = Mathf.Max(IronSkinTime, cast.P.GetF("duration_seconds", 4f));
            AddFury(cast.P.GetF("fury", 0f));
            CombatEvents.RaiseMessage("ŻELAZNA SKÓRA!", new Color(1f, 0.2f, 0.1f));
            HellFx.Ambient(transform, Vector3.up, HellFxElement.Enemy, 0.6f, IronSkinTime);
        }

        /// <summary>Wyzwalacze pieczęci: unik, otrzymany cios, zabicie, obrona (blok, parowanie, uchylenie).</summary>
        void HuTrigger(string kind, Vector3? at = null) => HuSkillCaster.OnTrigger(this, kind, at);

        /// <summary>Nowe piętro albo śmierć: to, co skille HU zostawiły w świecie, znika.</summary>
        void HuClear()
        {
            SetPassThroughEnemies(false);
            HuWorld.Clear();
            HuSkillCaster.Clear();
            huSpin = null; huDash = null;
        }
    }
}
