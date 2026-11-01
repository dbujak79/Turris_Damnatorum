using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Bohaterowie z Hell Unleashed: skill klasy (osobny przycisk), cechy klas (Furia, Arkanowa osłona, Zasadzka, Żądza krwi,
    /// Pobożność, Dwie klingi, Klinga zaklęć, Sokole oko, Więź dusz), style uniku (szarża barkiem, mrugnięcie, zniknięcie)
    /// oraz rodzaje umiejętności: aury, sługi, fala, doskok z wybuchem, przeciążenie, nieśmiertelność i impulsy novy.
    /// </summary>
    public partial class PlayerCombat
    {
        /// <summary>Indeks „slotu” skilla klasy w żądaniach (po trzech zwykłych slotach).</summary>
        public const int SignatureSlot = RunState.SkillSlotCount;

        ClassDefinition HeroClass => Run != null ? Run.startingClass : null;
        ClassTrait Trait => HeroClass != null ? HeroClass.trait : ClassTrait.None;
        DodgeStyle DodgeKind => HeroClass != null ? HeroClass.dodgeStyle : DodgeStyle.Roll;

        // ------------------------------------------------------------------ Stałe cech (Hell Unleashed, ClassTraitSettings)

        const float FuryPerHitDealt = 5f, FuryPerHitTaken = 12f, FuryDrainDelay = 3f, FuryDrainPerSecond = 15f;
        const float RageSeconds = 6f, RageAttackSpeed = 0.35f, RageReduction = 0.15f;
        /// <summary>Furia Berserkera (Żądza krwi): przy pełnej furii (i w szale) do 20% redukcji obrażeń i 10 życia/s, proporcjonalnie.</summary>
        public const float BerserkerFuryReduction = 0.2f, BerserkerFuryRegen = 10f;
        const float WardShare = 0.2f, WardDelay = 4f, WardRegenShare = 0.5f;
        const float AmbushBackMultiplier = 1.6f, AmbushDodgeMultiplier = 1.35f, AmbushWindow = 1.5f, VanishSeconds = 0.3f;
        const float BloodlustMax = 0.6f, HawkeyeMax = 0.45f, HawkeyeNear = 3f, HawkeyeFar = 11f;
        const float SoulBondPerServant = 0.03f, SoulBondMax = 0.15f;
        const float OverloadPower = 1.6f, OverloadRadius = 1.2f, OverheatSeconds = 2.5f;

        /// <summary>Pasek furii 0–100 (Furia, Żądza krwi).</summary>
        public float Fury { get; private set; }
        public float RageTime { get; private set; }
        public bool Raging => RageTime > 0f;
        /// <summary>
        /// Pancerz w ataku (hyper armour jak w Dark Souls): broń dwuręczna od zamachu do końca uderzenia – cios wroga nie wybija
        /// z ataku (bez drgnięcia), a obrażenia są o połowę mniejsze. Odzyskanie po ciosie jest już odsłonięte.
        /// </summary>
        public bool HyperArmor => Build != null && Build.twoHandedMelee && !IsDead
            && (Actions.Current == ActionType.LightAttack || Actions.Current == ActionType.HeavyAttack || Actions.Current == ActionType.Riposte)
            && (Actions.Phase == ActionPhase.Startup || Actions.Phase == ActionPhase.Active);
        public const float HyperArmorReduction = 0.5f;
        /// <summary>Furia jako ułamek 0–1; w szale liczy się jak pełna (furia spada wtedy do zera).</summary>
        public float FuryFraction => Raging ? 1f : Fury / 100f;
        float lastFightTime = -99f;

        /// <summary>Arkanowa osłona: warstwa pochłaniająca ciosy przed życiem; odrasta po kilku sekundach spokoju.</summary>
        public float Ward { get; private set; }
        public float WardMax => Trait == ClassTrait.ArcaneWard && Build != null ? Build.MaxHealth * WardShare : 0f;
        float wardDelay;

        /// <summary>Żelazna skóra (skill klasy Berserkera): 80% mniej obrażeń przez czas działania, poza miękkim limitem redukcji.</summary>
        public float IronSkinTime { get; private set; }
        public const float IronSkinReduction = 0.8f;
        /// <summary>Zniknięcie (przewrót w cień): bezpośrednie ciosy chybiają; potem okno zasadzki.</summary>
        float hiddenTime, ambushWindow;
        /// <summary>Mrugnięcie: niewrażliwość przez cały odskok.</summary>
        float blinkInvuln;
        float blinkFxRemaining;
        readonly HitTracker dodgeTracker = new HitTracker();

        /// <summary>Przeciążenie: następny skill ze slotu mocniejszy, potem sloty przegrzane.</summary>
        public bool OverloadReady { get; private set; }
        public float OverheatTime { get; private set; }
        bool overloadThisCast;

        /// <summary>Redukcja obrażeń z okrzyku (ułamek) i jej czas.</summary>
        float cryReduction, cryTime;

        /// <summary>Aury aktywne (Pobożność: dwie).</summary>
        public readonly List<AuraEffect> Auras = new List<AuraEffect>();
        int AuraSlots => Trait == ClassTrait.Devotion ? 2 : 1;

        struct PendingPulse { public float time; public SpellRuntime sk; public Vector3 center; public bool follow; public float mult; }
        readonly List<PendingPulse> pulses = new List<PendingPulse>();
        float heroClock;

        // ------------------------------------------------------------------ Start, piętra, tyknięcie

        /// <summary>Stan bohatera na początku piętra: furia i przegrzanie od zera, osłona pełna, sługi odchodzą (aury zostają).</summary>
        void HeroFloorStart()
        {
            Fury = 0f; RageTime = 0f; IronSkinTime = 0f; hiddenTime = 0f; ambushWindow = 0f; blinkInvuln = 0f;
            blinkFxRemaining = 0f;
            OverloadReady = false; OverheatTime = 0f; cryReduction = 0f; cryTime = 0f;
            pulses.Clear();
            HuClear();
            GearFloorStart();
            Run?.signatureSkill?.ResetCooldown();
            Servant.DismissAll();
            Ward = WardMax;
            Feature?.FloorStart();
            RespawnAuras();
            UpdateTempModifiers();
        }

        /// <summary>Po zmianie sceny/postaci obiekty aur trzeba odtworzyć przy bohaterze.</summary>
        void RespawnAuras()
        {
            var defs = new List<SpellDefinition>();
            foreach (var a in Auras) if (a != null) { defs.Add(a.Skill); a.Stop(); }
            Auras.Clear();
            foreach (var d in defs)
            {
                var sk = FindRuntime(d);
                if (sk != null) StartAura(sk, false);
            }
        }

        void HeroTick(float dt)
        {
            heroClock += dt;
            Run?.signatureSkill?.Tick(dt * CooldownSpeed);
            if (RageTime > 0f) { RageTime -= dt; if (RageTime <= 0f) { RageTime = 0f; UpdateTempModifiers(); } }
            if (UsesFury && !Raging && Fury > 0f && clock - lastFightTime > FuryDrainDelay) Fury = Mathf.Max(0f, Fury - FuryDrainPerSecond * dt);
            if (IronSkinTime > 0f) IronSkinTime -= dt;
            // Berserker: furia leczy (regeneracja, poza limitem kradzieży życia).
            if (Trait == ClassTrait.Bloodlust && Health != null && !IsDead && FuryFraction > 0f && Health.Fraction < 1f)
                Health.Restore(BerserkerFuryRegen * FuryFraction * dt);
            if (hiddenTime > 0f)
            {
                hiddenTime -= dt;
                if (hiddenTime <= 0f) HellFx.Shadow(null,transform.position,scale:0.5f);
            }
            if (blinkFxRemaining > 0f)
            {
                blinkFxRemaining -= dt;
                if (blinkFxRemaining <= 0f) HellFx.BlinkSparks(transform.position,Names.ElementColor(Build.weapon.light.element),false);
            }
            if (ambushWindow > 0f && Actions.Current != ActionType.Dodge) ambushWindow -= dt;
            if (blinkInvuln > 0f) blinkInvuln -= dt;
            if (OverheatTime > 0f) OverheatTime -= dt;
            if (cryTime > 0f) { cryTime -= dt; if (cryTime <= 0f) { cryReduction = 0f; } }

            // Arkanowa osłona odrasta po chwili bez trafień.
            if (Trait == ClassTrait.ArcaneWard)
            {
                if (wardDelay > 0f) wardDelay -= dt;
                else if (Ward < WardMax) Ward = Mathf.Min(WardMax, Ward + WardMax * WardRegenShare * dt);
            }

            // Szarża barkiem: w fazie uniku rani wrogów na drodze.
            if (Actions.Current == ActionType.Dodge && DodgeKind == DodgeStyle.ShoulderCharge && Actions.Phase != ActionPhase.Recovery)
            {
                var atk = Build.weapon.light;
                Vector3 c = transform.position + Vector3.up + DodgeDirection * 0.6f;
                HitQuery.Sphere(c, 0.75f, Faction.Player, dodgeTracker, t =>
                {
                    var h = HitData.FromAttack(atk, Build.WeaponDamage(atk) * 0.5f, transform.position, this);
                    h.element = Element.None; h.magic = 0f; h.split = default; h.physical = Build.WeaponDamage(atk) * 0.5f;
                    h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Stagger) };
                    h.knockback = 2.2f; h.fromMeleeWeapon = true; h.attackName = "Szarża barkiem";
                    PrepareOutgoing(ref h);
                    HitQuery.Apply(this, t, h, true);
                    AddFury(FuryPerHitDealt);
                });
            }

            // Zaplanowane impulsy novy (telegraf, kolejne fale).
            for (int i = pulses.Count - 1; i >= 0; i--)
            {
                var p = pulses[i];
                if (heroClock < p.time) continue;
                pulses.RemoveAt(i);
                NovaPulse(p.sk, p.follow ? transform.position : p.center, p.mult);
            }
        }

        /// <summary>Tempo odnowień: skrócenie z wyposażenia i klasy (Przywoływacz).</summary>
        float CooldownSpeed => 1f + Mathf.Max(0f, Build != null ? Build.stats[StatType.CooldownReduction] : 0f) / 100f;

        bool UsesFury => Trait == ClassTrait.Fury || Trait == ClassTrait.Bloodlust;

        public void AddFury(float amount)
        {
            if (!UsesFury || IsDead) return;
            lastFightTime = clock;
            if (Raging) return;
            Fury = Mathf.Min(100f, Fury + amount);
            if (Fury >= 100f)
            {
                Fury = 0f;
                RageTime = RageSeconds;
                CombatEvents.RaiseMessage("SZAŁ!", new Color(1f, 0.25f, 0.1f));
                Sfx.Play(SoundId.Warcry, AimPoint, 0.7f, 0.9f);
                FxLibrary.Shockwave(transform.position, new Color(1f, 0.2f, 0.05f), 2.5f, false);
                UpdateTempModifiers();
            }
        }

        /// <summary>Premie chwilowe do buildu: aury obronne (z mocą Pobożności) i szał.</summary>
        void UpdateTempModifiers()
        {
            if (Run == null) return;
            Run.tempModifiers.Clear();
            for (int i = 0; i < Auras.Count; i++)
            {
                var a = Auras[i];
                if (a == null || a.Skill == null) continue;
                float power = AuraPower(i);
                // Sąd przysięgi: aura działa odwrócona na wrogu – premie znikają, rezerwacja zostaje.
                bool judged = Feature != null && Feature.SuppressesAura(a);
                if (judged) { }
                else
                {
                    float own = UniquesAuraOwnerScale; // Habit pustego klasztoru: właściciel dostaje słabszą część
                    if (a.IsHu) foreach (var m in a.HuModifiers()) Run.tempModifiers.Add(m.Scaled(own));
                    else foreach (var m in a.Skill.auraModifiers) Run.tempModifiers.Add(new StatModifier(m.stat, m.value * power * own, m.mode));
                }
                // Koszt aury: Pobożność rezerwuje manę (20% pierwsza, 30% druga – jak w HU); inne klasy po 20% wytrzymałości.
                if (Trait == ClassTrait.Devotion) Run.tempModifiers.Add(new StatModifier(StatType.MaxMana, i == 0 ? -AuraManaReserveFirst : -AuraManaReserveSecond, ModifierMode.Percent));
                else Run.tempModifiers.Add(new StatModifier(StatType.MaxStamina, -AuraStaminaReserve, ModifierMode.Percent));
            }
            if (Raging && !UniquesSuppressRageSpeed) Run.tempModifiers.Add(new StatModifier(StatType.AttackSpeed, RageAttackSpeed * 100f));
            if (Build != null) RefreshBuild();
        }

        /// <summary>Rezerwacja zasobu przez aktywną aurę (%) – aura nie jest darmowa (audyt balansu).</summary>
        public const float AuraManaReserveFirst = 20f, AuraManaReserveSecond = 30f, AuraStaminaReserve = 20f;

        /// <summary>Forma aury HU zmieniła jej siłę (sztandar, tłum) – premie przeliczane od nowa.</summary>
        public void AuraStrengthChanged() => UpdateTempModifiers();

        /// <summary>Moc aury: Pobożność – pierwsza ×1,2, druga ×0,75; bez niej ×1.</summary>
        float AuraPower(int index) => Trait != ClassTrait.Devotion ? 1f : index == 0 ? 1.2f : 0.75f;

        // ------------------------------------------------------------------ Trafienia: cechy zależne od celu

        /// <summary>Wywoływane przez wroga przy każdym trafieniu od gracza (przed obroną): zasadzka, sokole oko, żądza krwi, egzekucja.</summary>
        public void OutgoingAgainst(ref HitData hit, EnemyBrain target)
        {
            float m = 1f;
            if ((Trait == ClassTrait.Ambush) && !hit.isReaction && hit.explosionRadius <= 0f)
            {
                Vector3 toHero = transform.position - target.transform.position; toHero.y = 0f;
                bool behind = toHero.sqrMagnitude > 1e-4f && Vector3.Dot(target.transform.forward, toHero.normalized) < -0.25f;
                if (behind) { m *= 1f + UniquesBackstab(AmbushBackMultiplier - 1f, hit, target); CombatEvents.RaiseWorldText(target.AimPoint + Vector3.up * 0.5f, "Z ZASKOCZENIA", new Color(1f, 0.8f, 0.3f)); }
                else if (ambushWindow > 0f) { m *= AmbushDodgeMultiplier; ambushWindow = 0f; CombatEvents.RaiseWorldText(target.AimPoint + Vector3.up * 0.5f, "Z ZASKOCZENIA", new Color(1f, 0.8f, 0.3f)); }
            }
            if (Trait == ClassTrait.Bloodlust && Health != null) m *= 1f + BloodlustMax * (1f - Health.Fraction);
            Feature?.OutgoingAgainst(ref hit, target);
            UniquesOutgoingAgainst(ref hit, target);
            BreachOutgoing(ref hit, target);
            if (Trait == ClassTrait.Hawkeye && hit.isProjectile)
            {
                // Stanowisko strzeleckie: odległość od punktu wylotu, nie od łucznika.
                float d = Vector3.Distance(hit.hasLaunchPoint ? hit.launchPoint : transform.position, target.transform.position);
                m *= 1f + HawkeyeMax * Mathf.Clamp01((d - HawkeyeNear) / (HawkeyeFar - HawkeyeNear));
            }
            hit.physical *= m; hit.magic *= m;
        }

        /// <summary>Premie wspólne dla wszystkich trafień (dopisywane w PrepareOutgoing).</summary>
        void HeroOutgoing(ref HitData hit)
        {
            if (Build == null) return;
            float g = 1f + Build.stats[StatType.GlobalDamage] / 100f;
            hit.physical *= g; hit.magic *= g;
            float ex = Build.stats[StatType.ExecuteDamage] / 100f;
            if (ex > 0f) hit.executeBonus += ex;
        }

        /// <summary>Po rozstrzygniętym trafieniu gracza: furia, kradzież życia, mana klingi zaklęć.</summary>
        void HeroOnHitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            bool landed = result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken;
            if (!landed || hit.isStatusTick) return;
            if (hit.weapon != WeaponModel.Auto && hit.fromMeleeWeapon) AddFury(FuryPerHitDealt);
            float leech = Build.stats[StatType.LifeLeech] / 100f;
            if (leech > 0f && result.healthDamage > 0f && !hit.featureEffect) PassiveHeal(result.healthDamage * leech * LeechWeight(hit), HealSource.Leech);
            if (Trait == ClassTrait.Spellblade && hit.fromMeleeWeapon && hit.weapon != WeaponModel.Auto) Mana.Restore(2f);
        }

        // ------------------------------------------------------------------ Leczenie bierne (HU PassiveHealingWindow)

        readonly Queue<(float time, float amount)> passiveHeals = new Queue<(float, float)>();
        float passiveHealSum;

        /// <summary>Współczynnik kradzieży życia wg rodzaju trafienia (HU): cios/pocisk 1, obszar 0,25, tyknięcie i reakcja 0.</summary>
        static float LeechWeight(in HitData hit) => hit.isStatusTick || hit.isReaction ? 0f : hit.explosionRadius > 0f ? 0.25f : 1f;

        /// <summary>
        /// Kradzież życia, życie za zabójstwo i podobne – razem najwyżej <see cref="BalanceConfig.passiveHealingPerSecond"/> maks. życia
        /// w ruchomej sekundzie; podczas Żelaznej skóry połowa. Skille leczące są poza tym limitem. Zwraca faktycznie przywrócone życie.
        /// </summary>
        public float PassiveHeal(float amount, HealSource source = HealSource.Leech)
        {
            if (amount <= 0f || IsDead) return 0f;
            while (passiveHeals.Count > 0 && clock - passiveHeals.Peek().time >= 1f) passiveHealSum -= passiveHeals.Dequeue().amount;
            if (IronSkinTime > 0f) amount *= 0.5f;
            if (Feature != null) amount *= Feature.PassiveHealMultiplier; // Czerwona linia: jak dawna Żelazna skóra
            float give = Mathf.Min(amount, Health.Max * B.passiveHealingPerSecond - passiveHealSum);
            if (give <= 0f) return 0f;
            passiveHeals.Enqueue((clock, give));
            passiveHealSum += give;
            // Limit źródła rozliczony – nadwyżka ponad pełne życie trafia do Łaski (Rozgrzeszenie) przez Overflow.
            return HealFrom(give, source);
        }

        // ------------------------------------------------------------------ Przyjmowanie trafień

        /// <summary>Przed rozstrzygnięciem ciosu: zniknięcie (ciosy chybiają). Zwraca wynik, gdy cios ma się nie liczyć.</summary>
        bool HeroEvades(in HitData hit)
        {
            if (hiddenTime > 0f && !hit.isStatusTick && hit.attacker != null)
            {
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.5f, "W CIENIU", new Color(0.6f, 0.6f, 0.9f));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Redukcja obrażeń: klasa, okrzyk, szał, więź dusz – sumowane i przez miękki limit HU (pełna do 25%, dalej połowa, maks. 45%);
        /// potem minimum bezpośredniego ciosu, Żelazna skóra (−80%), furia Berserkera (do −20%) – obie poza limitem – i arkanowa osłona.
        /// </summary>
        float HeroMitigate(float damage, bool statusTick, float minimum = 0f, bool skipWard = false)
        {
            damage *= 1f - CurrentDamageReduction;
            if (minimum > 0f && damage < minimum) damage = minimum;
            if (IronSkinTime > 0f) damage *= 1f - IronSkinReduction; // dawna sygnatura (tylko zgodność wstecz)
            // Cecha klasowa: dynamiczna Żelazna skóra Czerwonej linii – z życia sprzed rozstrzygnięcia tego trafienia.
            if (Feature != null) damage *= Feature.IncomingMultiplier(statusTick);
            // Furia Berserkera: osobny mnożnik, poza miękkim limitem redukcji (20% przy pełnej furii to zawsze 20%).
            if (Trait == ClassTrait.Bloodlust) damage *= 1f - BerserkerFuryReduction * FuryFraction;
            // Szarża: osobny mnożnik w trakcie doskoku, też poza limitem.
            damage *= 1f - DashReduction;
            // Pancerz w ataku (broń dwuręczna): osobny mnożnik, poza limitem.
            if (HyperArmor) damage *= 1f - HyperArmorReduction;
            if (Trait == ClassTrait.ArcaneWard && damage > 0f && !skipWard)
            {
                wardDelay = WardDelay;
                float absorbed = Mathf.Min(Ward, damage);
                Ward -= absorbed;
                damage -= absorbed;
            }
            if (!statusTick) { lastFightTime = clock; if (damage > 0f) AddFury(FuryPerHitTaken); }
            return damage;
        }

        /// <summary>Skuteczna redukcja obrażeń bohatera (0–1) ze wszystkich źródeł naraz.</summary>
        public float CurrentDamageReduction
        {
            get
            {
                float dr = Build.stats[StatType.DamageReduction] / 100f + cryReduction + (Raging ? RageReduction : 0f);
                if (Trait == ClassTrait.SoulBond) dr += Mathf.Min(SoulBondMax, SoulBondPerServant * Servant.Active.Count);
                return B.HeroDamageReduction(dr);
            }
        }

        bool HeroInvulnerable => blinkInvuln > 0f || DebugInvulnerable;
        /// <summary>Tylko testy: bohater nie otrzymuje obrażeń (np. przy sprawdzaniu ataków bossa).</summary>
        public bool DebugInvulnerable { get; set; }

        // ------------------------------------------------------------------ Unik klasy

        /// <summary>Unik według stylu klasy (po rozpoczęciu zwykłego uniku).</summary>
        void HeroDodgeStarted()
        {
            dodgeTracker.Reset();
            switch (DodgeKind)
            {
                case DodgeStyle.Blink:
                {
                    // Odskok widma: niewrażliwość przez cały unik, wybuch w miejscu startu.
                    blinkInvuln = Build.dodge.totalDuration + 0.08f;
                    blinkFxRemaining = Build.dodge.totalDuration * 0.5f;
                    HellFx.BlinkSparks(transform.position,Names.ElementColor(Build.weapon.light.element),true);
                    var atk = Build.weapon.light;
                    var h = HitData.FromAttack(atk, Build.WeaponDamage(atk) * 0.6f, transform.position, this);
                    h.knockback = 1.2f; h.attackName = "Odskok widma";
                    PrepareOutgoing(ref h);
                    var t = new HitTracker();
                    HitQuery.Sphere(transform.position + Vector3.up, 1.6f, Faction.Player, t, r => HitQuery.Apply(this, r, h, false));
                    FxLibrary.Shockwave(transform.position, new Color(0.6f, 0.5f, 1f), 1.6f, false);
                    FxLibrary.Flash(AimPoint, new Color(0.6f, 0.5f, 1f), 1f);
                    break;
                }
                case DodgeStyle.Vanish:
                    hiddenTime = VanishSeconds;
                    ambushWindow = AmbushWindow;
                    HellFx.Shadow(null, transform.position);
                    HellFx.Shadow(transform, transform.position, true, VanishSeconds);
                    break;
                case DodgeStyle.ShoulderCharge:
                    Sfx.Play(SoundId.Warcry, AimPoint, 0.35f, 1.2f);
                    break;
            }
            if (Trait == ClassTrait.Ambush && DodgeKind != DodgeStyle.Vanish) ambushWindow = AmbushWindow;
        }

        // ------------------------------------------------------------------ Skill klasy i aury

        SpellRuntime FindRuntime(SpellDefinition d)
        {
            if (Build == null || d == null) return null;
            if (Build.signature != null && Build.signature.Def == d) return Build.signature;
            foreach (var s in Build.skills) if (s != null && s.Def == d) return s;
            return null;
        }

        /// <summary>Włącza albo gasi aurę (bez kosztu, odnowienie 1 s). Zwraca true – żądanie obsłużone.</summary>
        bool ToggleAura(SpellRuntime sk)
        {
            var existing = Auras.Find(a => a != null && a.Skill == sk.Def);
            if (existing != null)
            {
                existing.Stop();
                Auras.Remove(existing);
                CombatEvents.RaiseMessage($"{sk.Def.displayName} – wygaszona", Color.gray);
                UpdateTempModifiers();
                return true;
            }
            if (!sk.instance.Ready) return false;
            sk.instance.TryUse();
            StartAura(sk, true);
            return true;
        }

        void StartAura(SpellRuntime sk, bool announce)
        {
            while (Auras.Count >= AuraSlots) { Auras[0]?.Stop(); Auras.RemoveAt(0); }
            int index = Auras.Count;
            float power = AuraPower(index);
            var def = sk.Def;
            if (IsHu(sk))
            {
                var hu = AuraEffect.SpawnHu(this, sk, power);
                Auras.Add(hu);
                UpdateTempModifiers();
                if (announce)
                {
                    HellFx.Shockwave(transform.position, HellSkillFx.Family(def), def.color, hu.Radius);
                    Sfx.Play(SoundId.Enchant, AimPoint, 0.8f);
                    CombatEvents.RaiseMessage(def.displayName, def.color);
                }
                return;
            }
            float radius = def.attack.radius * Mathf.Sqrt(power);
            bool offensive = def.attack.baseDamage > 0f;
            var aura = AuraEffect.Spawn(transform, this, def, radius, def.tickInterval, offensive ? () =>
            {
                var h = SkillHitData(sk, transform.position);
                h.physical *= power; h.magic *= power;
                return h;
            } : (System.Func<HitData>)null, def.color);
            Auras.Add(aura);
            UpdateTempModifiers();
            if (announce)
            {
                HellSkillFx.Shockwave(def, transform.position, radius, false);
                Sfx.Play(SoundId.Enchant, AimPoint, 0.8f);
                CombatEvents.RaiseMessage(def.displayName, def.color);
            }
        }

        // ------------------------------------------------------------------ Rodzaje umiejętności Hell Unleashed

        /// <summary>Nova w stylu Hell Unleashed: telegraf, impulsy, podążanie, odrzut, leczenie albo strefa.</summary>
        static bool IsHeroNova(SpellDefinition d) =>
            d.pulses > 1 || d.telegraph > 0f || d.followCaster || d.knockback != 0f || d.healPercent > 0f || d.zoneDuration > 0f || d.damageReductionBuff > 0f;

        /// <summary>Wykonanie rodzajów z Hell Unleashed. Zwraca false dla pozostałych (obsłuży je zwykła ścieżka).</summary>
        bool FireHeroSpell(SpellRuntime spell)
        {
            var def = spell.Def;
            float mult = overloadThisCast ? OverloadPower : 1f;
            switch (def.kind)
            {
                case SpellKind.Nova when IsHeroNova(def):
                case SpellKind.Warcry when def.damageReductionBuff > 0f:
                case SpellKind.Undying:
                {
                    for (int i = 0; i < Mathf.Max(1, def.pulses); i++)
                        pulses.Add(new PendingPulse { time = heroClock + def.telegraph + i * def.pulseInterval, sk = spell, center = transform.position, follow = def.followCaster, mult = mult });
                    if (def.telegraph > 0.05f)
                    {
                        HellSkillFx.Telegraph(def, transform);
                    }
                    if (def.kind == SpellKind.Warcry)
                    {
                        cryReduction = def.damageReductionBuff;
                        cryTime = def.duration;
                        CombatEvents.RaiseMessage("OKRZYK!", def.color);
                    }
                    if (def.kind == SpellKind.Undying)
                    {
                        IronSkinTime = def.duration;
                        CombatEvents.RaiseMessage("ŻELAZNA SKÓRA!", new Color(1f, 0.2f, 0.1f));
                        HellFx.Ambient(transform, Vector3.up, HellFxElement.Enemy, 0.6f, def.duration);
                    }
                    if (def.healPercent > 0f) Health.Restore(Health.Max * def.healPercent * mult);
                    if (def.furyGain > 0f) AddFury(def.furyGain);
                    return true;
                }
                case SpellKind.Aura:
                    return true; // włączanie/gaszenie w TryExecute
                case SpellKind.Summon:
                {
                    var snapshot = SkillHitData(spell, transform.position);
                    for (int i = 0; i < Mathf.Max(1, def.servantCount); i++)
                        Servant.Spawn(this, def.servant, def.duration, def.servantAttackInterval, () => snapshot, def.color);
                    HellSkillFx.Shockwave(def, transform.position, 2f, false);
                    return true;
                }
                case SpellKind.Wave:
                {
                    var h = SkillHitData(spell, transform.position);
                    Vector3 dir = AimDirection();
                    float width = def.waveWidth * (1f + F(spell, LevelFeatureKind.RadiusBonus));
                    WaveEffect.Spawn(this, transform.position + dir * 0.5f, dir, def.waveSpeed, def.attack.reach, width, def.knockback, h, def.color);
                    if (def.extraWaveShare > 0f)
                    {
                        var h2 = h; h2.physical *= def.extraWaveShare; h2.magic *= def.extraWaveShare;
                        StartCoroutine(Delayed(def.extraWaveDelay, () => WaveEffect.Spawn(this, transform.position + dir * 0.5f, dir, def.waveSpeed, def.attack.reach, width, def.knockback, h2, def.color)));
                    }
                    return true;
                }
                case SpellKind.Dash:
                    chargeDir = AimDirection();
                    lastChargePos = transform.position;
                    dashStopped = false;
                    FxLibrary.Dust(transform.position, 1.2f);
                    return true;
                case SpellKind.Overload:
                    OverloadReady = true;
                    CombatEvents.RaiseMessage("PRZECIĄŻENIE", def.color);
                    HellSkillFx.Shockwave(def, transform.position, 1.4f, false);
                    return true;
            }
            return false;
        }

        bool dashStopped;

        /// <summary>Doskok: szybki ruch w kierunku celowania; zatrzymuje się przy wrogu, a na końcu wybucha.</summary>
        void DashTick(SpellRuntime sk)
        {
            var def = sk.Def;
            if (dashStopped) { SkillMotion = Vector3.zero; return; }
            SkillMotion = chargeDir * (def.attack.reach / Mathf.Max(0.05f, def.dashTime));
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead) continue;
                if (Vector3.Distance(e.transform.position, transform.position) < 1.4f) { dashStopped = true; SkillMotion = Vector3.zero; break; }
            }
        }

        /// <summary>Koniec doskoku: wybuch wokół bohatera.</summary>
        void DashEnd(SpellRuntime sk)
        {
            var def = sk.Def;
            SkillMotion = Vector3.zero;
            var h = SkillHitData(sk, transform.position);
            var t = new HitTracker();
            float r = Radius(sk, def.attack.explosionRadius > 0 ? def.attack.explosionRadius : def.attack.radius);
            HitQuery.Sphere(transform.position + Vector3.up, r, Faction.Player, t, x => HitQuery.Apply(this, x, h, !def.IsSpell));
            HellSkillFx.Shockwave(def, transform.position, r, false);
            FxLibrary.Blood(AimPoint + transform.forward * 0.6f, transform.forward, 1.5f);
            PlaySwing(true);
        }

        /// <summary>Jeden impuls novy (także okrzyku, nieśmiertelności, boskiego osądu).</summary>
        void NovaPulse(SpellRuntime sk, Vector3 center, float mult)
        {
            if (sk == null || IsDead) return;
            var def = sk.Def;
            // Przeciążenie liczone przez mult (impuls przychodzi po rzuceniu) – bez drugiego naliczenia w SkillHitData.
            bool ov = overloadThisCast;
            overloadThisCast = false;
            var h = SkillHitData(sk, center);
            overloadThisCast = ov;
            h.physical *= mult; h.magic *= mult;
            h.knockback = def.knockback;
            float r = Radius(sk, def.attack.radius) * (mult > 1f ? OverloadRadius : 1f);
            var t = new HitTracker();
            HitQuery.Sphere(center + Vector3.up, r, Faction.Player, t, x => HitQuery.Apply(this, x, h, false));
            HellSkillFx.Shockwave(def, center, r);
            if (def.attack.element == Element.Earth) FxLibrary.Dust(center, r * 0.6f);
            if (def.zoneDuration > 0f && def.zoneRadius > 0f)
            {
                var zh = h; zh.physical *= def.zoneShare; zh.magic *= def.zoneShare; zh.knockback = 0f; zh.knockdown = 0f; zh.poiseDamage *= 0.1f;
                DamageZone.Spawn(center, def.zoneRadius, def.zoneDuration, def.zoneInterval, zh, Faction.Player, this, def.color);
            }
            Sfx.Play(def.kind == SpellKind.Nova ? Sfx.ForElement(def.attack.element, SoundId.Shockwave) : SoundId.Shockwave, center, 0.8f);
        }

        /// <summary>Kierunek celowania: namierzony wróg, inaczej przód postaci.</summary>
        Vector3 AimDirection()
        {
            Vector3 d = LockTarget != null && !LockTarget.IsDead ? LockTarget.transform.position - transform.position : transform.forward;
            d.y = 0f;
            return d.sqrMagnitude > 1e-4f ? d.normalized : transform.forward;
        }

        static System.Collections.IEnumerator Delayed(float seconds, System.Action a)
        {
            yield return new WaitForSeconds(seconds);
            a();
        }

        /// <summary>Po rzuceniu skilla ze slotu: zużyte przeciążenie przegrzewa sloty.</summary>
        void ConsumeOverload()
        {
            if (!overloadThisCast) return;
            overloadThisCast = false;
            OverheatTime = OverheatSeconds;
            CombatEvents.RaiseMessage("WYŁADOWANIE!", new Color(0.8f, 0.5f, 1f));
        }
    }
}
