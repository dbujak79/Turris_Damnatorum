using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Natywne unikaty Turris (docs/PROMPT_WDROZENIE_75_UNIKATOW_KLASOWYCH.md): zestaw mocy założonych przedmiotów i ich haki
    /// w potoku walki. Kolejność haków: najpierw cecha klasowa, potem moce unikatów w kolejności kodu karty – wynik nie zależy
    /// od kolejności zakładania. Trafienia wtórne mocy (<c>uniqueEffect</c>) nie wywołują haków mocy ponownie.
    /// </summary>
    public partial class PlayerCombat
    {
        public readonly UniquePowerSet Uniques = new UniquePowerSet();
        /// <summary>Tylko testy/pomiary: unikaty dają statystyki, ale ich moce nie działają (porównanie „z mocą / bez mocy”).</summary>
        public bool DebugNoUniquePowers;
        public readonly UniqueTelemetry UniqueTelemetry = new UniqueTelemetry();

        void UniquesSync()
        {
            if (Run == null || DebugNoUniquePowers) { Uniques.Clear(); return; }
            Uniques.Sync(this, Run.equipment);
        }

        /// <summary>Postać znika (koniec testu, nowa scena): moce zamykają swoje byty (widma, znaczniki, wiry) – bez wycieku.</summary>
        void OnDestroy() { Uniques.Clear(); PerksDestroy(); }

        void UniquesInit()
        {
            Uniques.Clear();
            UniqueTelemetry.Clear();
            UniquesSync();
        }

        bool UniquesInterceptAttack(ActionType type)
        {
            foreach (var p in Uniques.Active) if (p.InterceptAttack(type)) return true;
            return false;
        }

        AttackDefinition UniquesAttackOverride(ActionType type, AttackDefinition atk)
        {
            foreach (var p in Uniques.Active) atk = p.AttackOverride(type, atk);
            return atk;
        }

        void UniquesAttackStarted(ActionType type, AttackDefinition atk, int origin) { foreach (var p in Uniques.Active) p.AttackStarted(type, atk, origin); }
        void UniquesAttackActive(ActionType type, AttackDefinition atk, int origin) { foreach (var p in Uniques.Active) p.AttackActive(type, atk, origin); }
        void UniquesChargeTick(float seconds, float dt) { foreach (var p in Uniques.Active) p.ChargeTick(seconds, dt); }
        void UniquesOutgoing(ref HitData hit) { if (hit.uniqueEffect) return; foreach (var p in Uniques.Active) p.Outgoing(ref hit); }
        void UniquesOutgoingLate(ref HitData hit) { if (hit.uniqueEffect) return; foreach (var p in Uniques.Active) p.OutgoingLate(ref hit); }
        void UniquesOutgoingAgainst(ref HitData hit, EnemyBrain target) { if (hit.uniqueEffect) return; foreach (var p in Uniques.Active) p.OutgoingAgainst(ref hit, target); }

        void UniquesHitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (hit.uniqueEffect) return;
            foreach (var p in Uniques.Active) p.HitResolved(target, hit, result);
        }

        float UniquesBlockCost(float cost, in HitData hit)
        {
            foreach (var p in Uniques.Active) cost = p.BlockStaminaCost(cost, hit);
            return cost;
        }

        void UniquesDefended(HitOutcome outcome, in HitData hit) { foreach (var p in Uniques.Active) p.Defended(outcome, hit); }

        bool UniquesOverrideRiposte(EnemyBrain target, float damage)
        {
            foreach (var p in Uniques.Active) if (p.OverrideRiposte(target, damage)) return true;
            return false;
        }

        void UniquesStruck(in HitData hit, in HitResult r, float featureMultiplier) { foreach (var p in Uniques.Active) p.Struck(hit, r, featureMultiplier); }
        void UniquesDodge(Vector3 from, Vector3 dir) { foreach (var p in Uniques.Active) p.DodgeStarted(from, dir); }
        void UniquesCastFired(SpellRuntime sk, int origin) { foreach (var p in Uniques.Active) p.CastFired(sk, origin); }
        void UniquesBasicShot(ref HitData hit, int i, int count) { foreach (var p in Uniques.Active) p.BasicShot(ref hit, i, count); }

        bool UniquesCaptureShot(in HitData hit, Vector3 origin, Vector3 dir, float speed, Color col, int i, int count)
        {
            foreach (var p in Uniques.Active) if (p.CaptureShot(hit, origin, dir, speed, col, i, count)) return true;
            return false;
        }

        void UniquesManualHeal(bool flask) { foreach (var p in Uniques.Active) p.ManualHeal(flask); }
        /// <summary>Ręczny czar leczenia HU (komponent Heal z ręki, nie kopia i nie wyzwalacz).</summary>
        internal void NotifyManualHeal() => UniquesManualHeal(false);

        float UniquesBackstab(float bonus, in HitData hit, EnemyBrain target)
        {
            if (hit.uniqueEffect) return bonus;
            foreach (var p in Uniques.Active) bonus = p.BackstabBonus(bonus, hit, target);
            return bonus;
        }

        /// <summary>Rzut skilla HU z ręki: zmiana źródła i przejęcie wykonania przez moc (koszt już pobrany).</summary>
        internal void UniquesModifyHuCast(SpellRuntime sk, ref Vector3 origin, ref Vector3 aim, ref Vector3 target)
        {
            foreach (var p in Uniques.Active) p.ModifyHuCast(sk, ref origin, ref aim, ref target);
        }

        internal bool UniquesOverrideHuCast(ref HuCast cast, Vector3 aim, float range)
        {
            foreach (var p in Uniques.Active)
            {
                var replaced = p.ReplaceHuCast(cast);
                if (replaced != null) cast = replaced;
                if (p.OverrideHuCast(cast, aim, range)) return true;
            }
            return false;
        }

        internal bool UniquesSuppressKnockback(HuCast cast) { foreach (var p in Uniques.Active) if (p.SuppressKnockback(cast)) return true; return false; }
        internal int UniquesChainNodes(HuCast cast) { int n = 0; foreach (var p in Uniques.Active) n += p.ChainNodes(cast); return n; }
        internal Vector3? UniquesChainNode(Vector3 from, float jump, HashSet<object> used)
        {
            foreach (var p in Uniques.Active) { var v = p.ChainNode(from, jump, used); if (v.HasValue) return v; }
            return null;
        }
        bool ManaRegenBlocked { get { foreach (var p in Uniques.Active) if (p.BlocksManaRegen) return true; return false; } }
        void UniquesBarrier(float before, float after, in HitData hit) { foreach (var p in Uniques.Active) p.BarrierAbsorbed(before, after, hit); }
        bool UniquesOverrideSweep(IHitReceiver target, AttackDefinition atk) { foreach (var p in Uniques.Active) if (p.OverrideSweepHit(target, atk)) return true; return false; }
        /// <summary>Mnożnik mocy następnego rzutu HU (czar zamiast riposty z premią riposty) – ustawiany na czas jednego rzutu.</summary>
        internal float uniqueCastPower = 1f;
        /// <summary>Przemiana żywiołu odpalanego zapisanego czaru (runa inkantacji) – na czas jednego wykonania.</summary>
        internal Element uniqueElementOverride = Element.None;

        /// <summary>Punkt przemiany cechy klasowej – true: moc unikatu przejęła zdarzenie.</summary>
        internal bool UniquesFeatureEvent(string evt, ref float amount, object data = null)
        {
            foreach (var p in Uniques.Active) if (p.FeatureEvent(evt, ref amount, data)) return true;
            return false;
        }
        internal bool UniquesFeatureEvent(string evt, object data = null) { float a = 0f; return UniquesFeatureEvent(evt, ref a, data); }

        internal bool UniquesAuraPulse(AuraEffect aura, in HitData hit)
        {
            foreach (var p in Uniques.Active) if (p.AuraPulse(aura, hit)) return true;
            return false;
        }

        float UniquesAuraOwnerScale { get { float k = 1f; foreach (var p in Uniques.Active) k *= p.AuraOwnerScale; return k; } }
        /// <summary>Mnożnik odstępu ataków sług (mniejszy – szybciej) i obrażeń (Habit pustego klasztoru).</summary>
        public float ServantSpeedMult { get { float k = 1f; foreach (var p in Uniques.Active) k *= p.ServantSpeedMult; return k; } }
        public float ServantDamageMult { get { float k = 1f; foreach (var p in Uniques.Active) k *= p.ServantDamageMult; return k; } }
        /// <summary>Sługi atakują tylko na komendę (Rękawice kościanego dyrygenta).</summary>
        public bool ServantsCommanded { get { foreach (var p in Uniques.Active) if (p.CommandsServants) return true; return false; } }

        bool UniquesSuppressRageSpeed { get { foreach (var p in Uniques.Active) if (p.SuppressesRageSpeed) return true; return false; } }

        /// <summary>Zużywa sekundy szału (Pierścień nierozładowanego szału) – tych sekund nie wyda już Spalenie furii.</summary>
        internal bool ConsumeRage(float seconds)
        {
            if (RageTime < seconds) return false;
            RageTime -= seconds;
            if (RageTime <= 0.001f) { RageTime = 0f; UpdateTempModifiers(); }
            return true;
        }

        void UniquesForcedInterrupt() { foreach (var p in Uniques.Active) p.ForcedInterrupt(); }

        /// <summary>Lewa broń poza ręką (orbita Pasa żonglera) – nie atakuje na zmianę i nie liczy się do pary.</summary>
        public bool OffhandAway { get { foreach (var p in Uniques.Active) if (p.HoldsOffhand) return true; return false; } }
        public bool OffhandFightsAlone { get { foreach (var p in Uniques.Active) if (p.OffhandFightsAlone) return true; return false; } }

        /// <summary>Zysk z reakcji (Strzała odpowiedzi): moc może odłożyć część – zwraca odjętą kwotę.</summary>
        internal float UniquesReactionStore(float gain, Element element)
        {
            float taken = 0f;
            foreach (var p in Uniques.Active) taken += p.ReactionStore(gain - taken, element);
            return Mathf.Min(gain, taken);
        }

        // Indeksem, nie foreach: moc może w swoim kroku zmienić wyposażenie/build (Sync), a pętla ma to przetrwać.
        void UniquesTick(float dt) { var a = Uniques.All; for (int i = 0; i < a.Count; i++) a[i].Tick(dt); }
        void UniquesFloorStart() { var a = Uniques.All; for (int i = 0; i < a.Count; i++) a[i].FloorStart(); }
        void UniquesDeath() { var a = Uniques.All; for (int i = 0; i < a.Count; i++) a[i].Death(); }

        /// <summary>Wróg jest odwrócony plecami do bohatera (prawdziwa relacja właściciel/ofiara, jak Zasadzka).</summary>
        internal bool IsBehind(EnemyBrain target)
        {
            if (target == null) return false;
            Vector3 toHero = transform.position - target.transform.position; toHero.y = 0f;
            return toHero.sqrMagnitude > 1e-4f && Vector3.Dot(target.transform.forward, toHero.normalized) < -0.25f;
        }

        // ------------------------------------------------------------------ Usługi dla mocy

        /// <summary>Tylko testy: pełny potok wyjścia gracza (cecha, moce unikatów, premie) dla ręcznie zbudowanego trafienia.</summary>
        public void PrepareOutgoingForTest(ref HitData hit) => PrepareOutgoing(ref hit);
        /// <summary>Tylko testy: koszt życia (bez trafienia wroga).</summary>
        public void PayLifeForTest(float amount) => PayLife(amount);
        internal SpellRuntime CurrentSpellRuntime => currentSpell;
        /// <summary>Zbroja runożercy: pęknięcie pancerza właściciela jest magiczne (ułamek odporności żywiołów za warstwę).</summary>
        internal float UniqueSunderMagic { get { foreach (var p in Uniques.Active) if (p.SunderMagicPerStack > 0f) return p.SunderMagicPerStack; return 0f; } }
        internal float ChargeSeconds => ChargeTime;
        internal bool Charging => chargingNow;
        internal void ForceWeaponThrown(bool thrown)
        {
            WeaponThrown = thrown;
            var vis = GetComponent<CharacterVisual>();
            if (vis != null) vis.SetWeaponVisible(!thrown);
        }
        internal void UniquePushSelf(Vector3 dir, float distance, float seconds) => Knockback(dir, distance, Mathf.Max(0.05f, seconds));
        internal HitData UniqueWeaponHit(AttackDefinition atk, float share, bool offhand)
        {
            float dmg = (offhand ? Build.OffWeaponDamage(atk) : Build.WeaponDamage(atk)) * share;
            var hit = HitData.FromAttack(atk, dmg, transform.position, this);
            hit.fromMeleeWeapon = atk.delivery == AttackDelivery.Melee; hit.weapon = HeldWeaponModel;
            ApplyWeaponBuff(ref hit);
            PrepareOutgoing(ref hit);
            hit.uniqueEffect = true; hit.featureEffect = true;
            return hit;
        }
        /// <summary>Rzut skilla HU poza akcją (moc wykonuje zapisany rzut gdzie indziej) – bez kosztu, z podanym pochodzeniem.</summary>
        internal void UniqueFireHuCast(HuCast cast, Vector3 aim, float range, int origin)
        {
            int prev = ActionOrigin; ActionOrigin = origin;
            HuSkillCaster.Fire(cast, aim, range);
            ActionOrigin = prev;
        }
        /// <summary>Wykonuje czar ze slotu w torze (bez akcji ciała) – dla mocy zastępujących ripostę (koszt pobiera moc).</summary>
        internal void UniqueFireSpell(SpellRuntime sk) => FireLaneSpell(sk);
    }
}
