using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Cechy klasowe (docs/PROMPT_WDROZENIE_30_CECH_KLASOWYCH.md): przycisk klasy (G / View) obsługuje kontroler wybranej cechy
    /// zamiast dawnej sygnatury. Tu: utworzenie kontrolera, kontekst leczenia, id akcji (pochodzenie trafień) i wspólne
    /// operacje dla kontrolerów – cel, kierunek, trafienie, zakończenie szału, zużycie osłony.
    ///
    /// Kolejność obrony bohatera (dokumentacja potoku): odporność na żywioł → reakcje → pancerz/garda (DamageResolver) →
    /// ogólna redukcja z miękkim limitem → minimum ciosu → Czerwona linia (cecha) → furia Berserkera → szarża → pancerz w ataku →
    /// Arkanowa osłona → Płaszcz many (cecha) → bariera → życie. Koszty życia i rezerwacja nie przechodzą przez ten potok.
    /// </summary>
    public partial class PlayerCombat
    {
        /// <summary>Kontroler wybranej cechy klasowej (null – brak cechy: klasa spoza katalogu albo test zgodności z dawną sygnaturą).</summary>
        public ClassFeatureController Feature { get; private set; }
        public ClassFeatureTuning FeatureTuning => Config != null && Config.classFeatures != null ? Config.classFeatures : defaultTuning;
        static readonly ClassFeatureTuning defaultTuning = new ClassFeatureTuning();
        public readonly ClassFeatureTelemetry FeatureTelemetry = new ClassFeatureTelemetry();

        /// <summary>Źródło bieżącego leczenia (współczynniki Łaski).</summary>
        HealSource healSource = HealSource.Other;
        /// <summary>Id bieżącej ręcznej akcji – dziedziczone przez trafienia, kopie i strefy.</summary>
        public int ActionOrigin { get; private set; }
        static int originCounter;
        internal int NewOrigin() => ActionOrigin = ++originCounter;

        /// <summary>Gracz trzyma przycisk bloku (Walka w gardzie: garda w trakcie ataku).</summary>
        public bool BlockHeld { get; private set; }

        void FeatureInit()
        {
            Feature?.Dispose();
            Feature = null;
            FeatureTelemetry.Clear();
            Health.Overflow = a => Feature?.OnOverheal(a, healSource);
            var def = Run != null ? Run.ClassFeature : null;
            if (def == null) return;
            Feature = ClassFeatureFactory.Create(def);
            Feature?.Bind(this, def);
        }

        /// <summary>Przycisk klasy.</summary>
        public void PressClassFeature()
        {
            if (Feature != null) Feature.Press();
        }

        /// <summary>Stan cechy do HUD (null – brak cechy).</summary>
        public ClassFeatureHud? FeatureHud => Feature?.Hud;

        // ------------------------------------------------------------------ Wspólne operacje kontrolerów

        /// <summary>Kierunek celowania (namierzony wróg, pomoc w celowaniu, przód postaci).</summary>
        internal Vector3 FeatureAim()
        {
            var e = LockTarget != null && !LockTarget.IsDead ? LockTarget : AssistTarget != null && !AssistTarget.IsDead ? AssistTarget : null;
            Vector3 d = e != null ? e.transform.position - transform.position : transform.forward;
            d.y = 0f;
            return d.sqrMagnitude > 1e-4f ? d.normalized : transform.forward;
        }

        /// <summary>Cel cechy: namierzony → pomoc w celowaniu → najbliższy w stożku 70° przed postacią, w zasięgu. Deterministyczny.</summary>
        internal EnemyBrain FeatureTarget(float range) => HuAimTarget(range, 70f, transform.position, FeatureAim());

        /// <summary>Trafienie efektu cechy (bez kradzieży życia i bez wytwarzania zasobów cech), przez zwykły potok wyjścia.</summary>
        internal HitData FeatureHit(string name, float physical, float magic, Element element, Vector3 source)
        {
            var h = new HitData
            {
                physical = physical, magic = magic, element = element, guardLoad = 25f, poiseDamage = 20f,
                blockable = true, parryable = false, dodgeable = true, sourcePosition = source, attacker = this, attackName = name,
                mods = StatusModifiers.None, featureEffect = true, originId = ActionOrigin,
            };
            PrepareOutgoing(ref h);
            return h;
        }

        internal HitResult FeatureApply(IHitReceiver target, HitData hit, bool melee = false) => HitQuery.Apply(this, target, hit, melee);

        /// <summary>Zwykły atak bronią: obrażenia lekkiego ataku głównej broni (wzorzec budżetów cech).</summary>
        internal float BasicDamage => Build != null ? Build.WeaponDamage(Build.weapon.light) : 0f;

        /// <summary>Kończy szał (Spalenie furii): bez szybkości i ochrony; furia nie zostaje ukryta.</summary>
        internal float EndRage()
        {
            float left = RageTime;
            RageTime = 0f; Fury = 0f;
            UpdateTempModifiers();
            return left;
        }

        /// <summary>Zużywa niewydaną Arkanową osłonę (rusza normalne opóźnienie odrastania). Zwraca zużytą ilość.</summary>
        internal float ConsumeWard()
        {
            float w = Ward;
            Ward = 0f;
            wardDelay = WardDelay;
            return w;
        }

        /// <summary>Tylko testy: trafienie efektu cechy (przez zwykły potok wyjścia).</summary>
        public HitData FeatureHitForTest(float physical, float magic = 0f, Element element = Element.None) => FeatureHit("Test", physical, magic, element, transform.position);
        /// <summary>Tylko testy: hook wyjścia cechy (przed premiami).</summary>
        public void FeatureOutgoingForTest(ref HitData hit) => Feature?.Outgoing(ref hit);
        /// <summary>Tylko testy: hook końca potoku wyjścia cechy (modyfikatory efektów).</summary>
        public void FeatureOutgoingLateForTest(ref HitData hit) => Feature?.OutgoingLate(ref hit);

        /// <summary>Tylko testy: zużywa Arkanową osłonę (bez cechy).</summary>
        public float ConsumeWardForTest() => ConsumeWard();

        /// <summary>Zużywa barierę (Kamienna osłona, Mroźna zbroja…). Zwraca zużytą ilość.</summary>
        internal float ConsumeBarrier()
        {
            float b = BarrierAmount;
            if (b > 0f) EndBarrier(false);
            return b;
        }

        /// <summary>Leczenie ze źródłem (Łaska liczy współczynnik źródła z nadwyżki).</summary>
        public float HealFrom(float amount, HealSource src)
        {
            var prev = healSource; healSource = src;
            float g = Health.Restore(amount);
            healSource = prev;
            return g;
        }

        /// <summary>Koszt życia (cecha): prosto z puli – bez pancerza, osłon, wyzwalaczy „otrzymano cios” i furii. Może zabić.</summary>
        internal void PayLife(float amount)
        {
            if (amount <= 0f || IsDead) return;
            Health.Drain(amount);
            if (Health.Current <= 0f && !IsDead) Die();
        }

        /// <summary>Samoobrażenia cechy (Żywy piec): odporność na ogień i potok obrony (bez furii, cierni i zemsty). Zwraca utracone PŻ.</summary>
        internal float SelfFire(float raw)
        {
            if (raw <= 0f || IsDead) return 0f;
            if (Status.BurnImmune) return 0f;
            float dmg = raw * (1f - B.HeroResist(Build.stats[StatType.FireResist]) / 100f);
            // Własny płomień płonie pod Arkanową osłoną (nie zdejmuje jej) – paliwo ogranicza odporność na ogień i bariera.
            dmg = HeroMitigate(dmg, true, 0f, true);
            if (BarrierAmount > 0f && dmg > 0f)
            {
                float absorbed = Mathf.Min(BarrierAmount, dmg);
                BarrierAmount -= absorbed; dmg -= absorbed;
                if (BarrierAmount <= 0.01f) EndBarrier(true);
            }
            if (dmg <= 0f) return 0f;
            float before = Health.Current;
            if (TryCheatDeath(dmg)) return 0f;
            Health.Drain(dmg);
            float lost = before - Health.Current;
            if (Health.Current <= 0f && !IsDead) Die();
            return lost;
        }

        /// <summary>Strzał pociskiem cechy (płyta, salwa oka, sztuczka) – pocisk gracza z podanym trafieniem.</summary>
        internal Projectile FeatureProjectile(Vector3 from, Vector3 dir, float speed, HitData hit, Color color, float radius, IHitReceiver homing = null)
            => Projectile.Spawn(from, dir, speed, hit, Faction.Player, this, color, radius, 2.5f, homing, homing != null ? 80f : 0f, false);

        /// <summary>Czar ze slotu w torze równoległym (Dwa takty): wyzwala czar bez akcji ciała. Zwraca false, gdy kind wymaga akcji.</summary>
        public static bool LaneCompatible(SpellRuntime sk)
        {
            if (sk == null || !sk.Def.IsSpell || sk.Def.IsAura) return false;
            if (sk.instance.seals != null)
            {
                var shape = sk.instance.seals.Shape;
                return !shape.HasComponent("Dash") && !shape.HasComponent("Spin");
            }
            switch (sk.Def.kind)
            {
                case SpellKind.Projectile: case SpellKind.Meteor: case SpellKind.Zone: case SpellKind.Chain: case SpellKind.Storm: return true;
                default: return false;
            }
        }

        internal void FireLaneSpell(SpellRuntime sk)
        {
            if (sk == null || IsDead) return;
            int prev = ActionOrigin;
            NewOrigin();
            FireSpell(sk);
            Feature?.CastFired(sk, ActionOrigin);
            ActionOrigin = prev;
        }

        /// <summary>Taniec bez przerwy: przeciągnięcie hitboxa przeniesionego ciosu z jego własną listą trafionych.</summary>
        internal void SweepCarried(AttackDefinition atk, HitTracker carriedTracker, bool offhand, int origin, float scale = 1f)
        {
            if (atk == null || atk.delivery == AttackDelivery.Projectile) return;
            Vector3 from = transform.position + Vector3.up * 1.0f + transform.forward * 0.3f;
            float reach = atk.reach * (1f + Build.effects[PassiveEffectType.MeleeRange] / 100f);
            Vector3 to = transform.position + Vector3.up * 1.0f + transform.forward * reach;
            int prev = ActionOrigin; ActionOrigin = origin;
            HitQuery.Capsule(from, to, atk.radius, Faction.Player, carriedTracker, target =>
            {
                float dmg = (offhand ? Build.OffWeaponDamage(atk) : Build.WeaponDamage(atk)) * scale;
                var hit = HitData.FromAttack(atk, dmg, transform.position, this);
                hit.fromMeleeWeapon = true; hit.basicAttack = true;
                hit.weapon = HeldWeaponModel;
                AddWeaponElements(ref hit, dmg / PhysicalFactor); // przeniesiony zwykły cios – własna baza
                ApplyWeaponBuff(ref hit);
                ApplyHeavyPoise(ref hit, offhand);
                BasicAttackPerks(ref hit, true, target);
                PrepareOutgoing(ref hit);
                HitQuery.Apply(this, target, hit, true);
            });
            ActionOrigin = prev;
        }

        internal void PlayFeatureSwing(bool heavy) => PlaySwing(heavy);
        internal AttackDefinition CurrentAttackDef => currentAttack;
        internal bool AttackIsOffhand => attackOffhand;
        internal HitTracker AttackTracker => tracker;
        internal float Clock => clock;
        internal float WardDelayRemaining => wardDelay;

        /// <summary>Druga broń w ręce (Niezgodna para): obrażenia jej własnego ataku.</summary>
        internal HitData PacketHit(ItemInstance item, AttackDefinition atk, float share, bool offhand, IHitReceiver target)
        {
            float dmg = (offhand ? Build.OffWeaponDamage(atk) : Build.WeaponDamage(atk)) * share;
            var hit = HitData.FromAttack(atk, dmg, transform.position, this);
            hit.fromMeleeWeapon = true; hit.basicAttack = true;
            hit.weapon = item != null ? AnimResolve.ForItem(item.definition) : HeldWeaponModel;
            AddWeaponElements(ref hit, dmg / PhysicalFactor); // atak drugiej broni – baza tej ręki
            ApplyWeaponBuff(ref hit);
            if (!offhand) BasicAttackPerks(ref hit, true, target);
            ApplyMove(ref hit);
            ApplyHeavyPoise(ref hit, offhand || (item != null && item != Build.mainHand));
            PrepareOutgoing(ref hit);
            return hit;
        }

        /// <summary>Zmiana broni w rękach (Arsenał, Wcielenie): build przeliczany atomowo; aktywna akcja już ma swoje dane.</summary>
        internal void FeatureRefresh() => RefreshBuild();
    }
}
