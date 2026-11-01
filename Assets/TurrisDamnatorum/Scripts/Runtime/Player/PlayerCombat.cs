using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wspólny system walki gracza. Nie zna pojęcia "klasy": wszystkie akcje i ich parametry
    /// pochodzą z <see cref="BuildSnapshot"/> (wyposażenie + umiejętności w slotach + wzmocnienia).
    ///
    /// Akcje: szybki atak, mocny atak, blok, parowanie, unik, flaszki oraz trzy sloty umiejętności
    /// (czary za manę albo techniki bronią za wytrzymałość), każdy z własnym odnowieniem.
    /// </summary>
    public partial class PlayerCombat : MonoBehaviour, IHitReceiver, ICharacterAnimSource, IHitStop
    {
        public GameConfig Config { get; private set; }
        public RunState Run { get; private set; }
        public BuildSnapshot Build { get; private set; }
        /// <summary>Mnożnik prędkości pocisków bohatera (HU ProjectileSpeed z przedmiotów i affixów).</summary>
        public float ProjectileSpeedMult => Build == null ? 1f : 1f + Build.effects[PassiveEffectType.ProjectileSpeed] / 100f;

        public ResourcePool Health { get; private set; }
        public ResourcePool Stamina { get; private set; }
        public ResourcePool Mana { get; private set; }
        public readonly ActionController Actions = new ActionController();

        public float WeaponBuffTime { get; private set; }
        /// <summary>Osłona (umiejętność Barrier): pochłania obrażenia przed utratą życia.</summary>
        public float BarrierAmount { get; private set; }
        public float BarrierTime { get; private set; }
        /// <summary>Prędkość narzucana przez umiejętność (szarża). Zero = brak.</summary>
        public Vector3 SkillMotion { get; private set; }
        /// <summary>Efekty trwające na bohaterze (krwawienie, podpalenie, chłód, porażenie, zamrożenie).</summary>
        public readonly StatusEffects Status = new StatusEffects();
        readonly StatusFx statusFx = new StatusFx();
        /// <summary>Żywioł i efekty zaklętej broni (np. płomienne ostrze).</summary>
        Element weaponBuffElement;
        List<StatusApplication> weaponBuffStatuses;
        /// <summary>Buty burzy: po uniku następny cios bronią poraża.</summary>
        public bool ShockCharged { get; private set; }
        /// <summary>Okrzyk wojenny: premia do obrażeń (%) i czas jej trwania.</summary>
        public float DamageBuffPct { get; private set; }
        public float DamageBuffTime { get; private set; }
        /// <summary>Krwawy pakt: liczba kolejnych czarów bez kosztu many.</summary>
        public int FreeCasts { get; private set; }
        /// <summary>Mroźna zbroja: czas odwetu chłodem na atakujących wręcz.</summary>
        public float FrostArmorTime { get; private set; }
        /// <summary>Ciężki rzut: broń jest w locie – ataki pięściami, bez bloku bronią.</summary>
        public bool WeaponThrown { get; private set; }
        public Color WeaponBuffColor { get; private set; } = new Color(0.45f, 0.65f, 1f);
        /// <summary>Dłoń do efektów (z humanoida, a gdy go brak – sama postać).</summary>
        Transform Hand
        {
            get
            {
                var v = GetComponent<CharacterVisual>();
                return v != null && v.RightHand != null ? v.RightHand : transform;
            }
        }
        float weaponBuffDamage;

        public Faction Faction => Faction.Player;
        public bool IsDead => Actions.IsDead;
        public Transform Transform => transform;
        public Vector3 AimPoint => transform.position + Vector3.up * 1.0f;

        /// <summary>Aktualny cel namierzania (ustawiany przez PlayerController).</summary>
        public EnemyBrain LockTarget { get; set; }
        /// <summary>Wróg wybrany przez pomoc w celowaniu dla bieżącej akcji (ustawia PlayerController; null – brak).</summary>
        public EnemyBrain AssistTarget { get; set; }
        /// <summary>Kierunek wejścia względem kamery (dla uniku).</summary>
        public Vector3 MoveIntent { get; set; }

        public event Action Died;
        public event Action BuildChanged;

        // Stan bieżącej akcji
        AttackDefinition currentAttack;
        SpellRuntime currentSpell;
        HellParticles gatherFx, barrierFx;
        int requestedSlot;
        bool castWasFree;
        /// <summary>Co cecha pobrała za bieżący rzut (żeton, dług) – zwrot przy anulowaniu przed wyzwoleniem.</summary>
        CastReceipt castReceipt;
        float skillActiveStart, skillHitTimer;
        int flurryIndex;
        Vector3 chargeDir, lastChargePos;
        bool flaskIsMana;
        EnemyBrain riposteTarget;
        public Vector3 DodgeDirection { get; private set; }
        /// <summary>Sekundy od końca uniku (0 w trakcie) – wrogowie wtedy wolniej się obracają.</summary>
        public float TimeSinceDodge { get; private set; } = 99f;
        readonly HitTracker tracker = new HitTracker();

        // Odczucie trafień: hitstop, odrzut, kierunek ostatniego ciosu (upadek).
        float hitStop;
        Vector3 pushVelocity;
        float pushTime;
        Vector3 lastHitDir;
        float lastHitWeight;

        /// <summary>Trwa hitstop – ruch i akcje stoją.</summary>
        public bool InHitStop => hitStop > 0f;

        /// <summary>Kierunek, do którego bohater w bloku ma się obrócić po ciosie z boku (świat, poziomo); null – brak.</summary>
        public Vector3? GuardFace => guardFaceTime > 0f && Actions.Current == ActionType.Block ? guardFaceDir : (Vector3?)null;
        Vector3 guardFaceDir;
        float guardFaceTime;

        /// <summary>Cios w blok z kąta do <c>guardAutoTurnAngle</c> od przodu: zwraca kierunek do napastnika i zleca obrót.</summary>
        bool GuardTurnTo(Vector3 source, out Vector3 dir)
        {
            dir = source - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) return false;
            dir.Normalize();
            Vector3 fwd = transform.forward; fwd.y = 0f;
            if (Vector3.Angle(fwd, dir) > B.guardAutoTurnAngle) return false;
            guardFaceDir = dir;
            guardFaceTime = 0.35f;
            return true;
        }
        /// <summary>Prędkość odrzutu po otrzymanym ciosie (zero, gdy brak).</summary>
        public Vector3 PushVelocity => pushTime > 0f && hitStop <= 0f ? pushVelocity : Vector3.zero;

        public void HitStop(float seconds)
        {
            if (IsDead) return;
            hitStop = Mathf.Max(hitStop, seconds);
            var v = GetComponent<CharacterVisual>();
            if (v != null) v.Hold(seconds);
        }

        /// <summary>Odrzut bohatera (po hitstopie) – krótki poślizg w kierunku ciosu.</summary>
        public void Knockback(Vector3 dir, float distance, float time = 0.2f)
        {
            dir.y = 0f;
            if (IsDead || distance <= 0f || dir.sqrMagnitude < 1e-4f) return;
            pushVelocity = dir.normalized * (distance / time);
            pushTime = time;
        }

        /// <summary>Model broni w dłoni (pięści w czasie rzutu).</summary>
        WeaponModel HeldWeaponModel => WeaponThrown || Build.mainHand == null ? WeaponModel.None : AnimResolve.ForItem(Build.mainHand.definition);

        // Bufor wejścia
        ActionType buffered = ActionType.None;
        float bufferedTime;
        float clock; // własny zegar symulacji (deterministyczny w testach)
        float deathTime;

        struct TimedRestore { public bool mana; public float perSecond; public float remaining; }
        readonly List<TimedRestore> restores = new List<TimedRestore>();

        BalanceConfig B => Config.balance;

        // ------------------------------------------------------------------ Tempo akcji

        float Speed => Mathf.Max(0.1f, B.playerActionSpeed);
        float RecoveryScale => Mathf.Clamp(B.playerRecoveryScale, 0.1f, 1f);
        /// <summary>Czas przygotowania/fazy aktywnej akcji gracza po uwzględnieniu tempa.</summary>
        public float ScaleStartup(float t) => t / Speed;
        /// <summary>Czas fazy zakończenia (i punktu przerwania) akcji gracza po uwzględnieniu tempa.</summary>
        public float ScaleRecovery(float t) => float.IsInfinity(t) ? t : t / Speed * RecoveryScale;

        /// <summary>Faktyczne czasy bieżącego ataku bronią (dla wypadu i obrotu w PlayerController).</summary>
        public float AttackWindup { get; private set; }
        public float AttackActive { get; private set; }

        public void Init(RunState run, GameConfig cfg)
        {
            Run = run;
            Config = cfg;
            Actions.Reset();
            deathTime = 0f;
            Actions.PhaseChanged -= OnPhaseChanged;
            Actions.PhaseChanged += OnPhaseChanged;
            Build = BuildCalculator.Compute(run, cfg);
            Status.BurnImmune = Build.effects[PassiveEffectType.BurnImmunity] > 0;
            Health = new ResourcePool(Build.MaxHealth);
            Stamina = new ResourcePool(Build.MaxStamina);
            Mana = new ResourcePool(Build.MaxMana);
            Health.SetCurrent(Build.MaxHealth * run.healthFraction);
            Mana.SetCurrent(Build.MaxMana * run.manaFraction);
            restores.Clear();
            WeaponBuffTime = 0;
            weaponBuffElement = Element.None;
            weaponBuffStatuses = null;
            FeatureInit();
            UniquesInit();
            PerksInit();
            OnFloorStart();
        }

        /// <summary>Nowe piętro: pełne ładunki umiejętności, bez osłony.</summary>
        public void OnFloorStart()
        {
            if (Run != null) foreach (var s in Run.knownSpells) s.ResetCooldown();
            EndBarrier(false);
            SkillMotion = Vector3.zero;
            Status.Clear();
            statusFx.Clear();
            DamageBuffTime = 0; FreeCasts = 0; FrostArmorTime = 0; ShockCharged = false;
            foreach (var tw in ThrownWeapon.Active.ToArray()) if (tw != null) Destroy(tw.gameObject);
            CatchWeapon();
            UniquesFloorStart();
            PerksFloorStart();
            HeroFloorStart();
        }

        /// <summary>Przelicza build po zmianie wyposażenia/wzmocnień. Obecne wartości są przycinane do nowych maksimów.</summary>
        public void RefreshBuild()
        {
            Run.SyncGrantedSkills(); // grymuar / sztylet do rzucania dają skill, dopóki są założone
            Run.RefreshBonds();      // więzi pieczęci między skillami w ręce
            foreach (var found in Run.newlyDiscovered)
                CombatEvents.RaiseMessage($"ODKRYCIE: {(found.kind == SealKind.Bond ? "więź" : "zespolenie")} {found.name}", new Color(0.85f, 0.6f, 1f));
            Run.newlyDiscovered.Clear();
            Build = BuildCalculator.Compute(Run, Config);
            Status.BurnImmune = Build.effects[PassiveEffectType.BurnImmunity] > 0;
            Health.SetMax(Build.MaxHealth, false);
            Stamina.SetMax(Build.MaxStamina, false);
            Mana.SetMax(Build.MaxMana, false);
            UniquesSync();
            BuildChanged?.Invoke();
        }

        public void RestoreAll()
        {
            Health.SetCurrent(Health.Max);
            // Zobowiązanie przed odnowieniem: dług many spłaca odnowienie piętra (filtr puli), dopiero reszta zostaje maną.
            Mana.Restore(Mana.Max + (Run != null ? Run.manaDebt : 0f));
            Stamina.SetCurrent(Stamina.Max);
        }

        /// <summary>Umiejętność w slocie 0–2 (null = pusty slot).</summary>
        public SpellRuntime Skill(int slot) => Build == null ? null : slot == SignatureSlot ? Build.signature
            : slot >= 0 && slot < Build.skills.Length ? Build.skills[slot] : null;

        /// <summary>Umiejętność wykonywana w tej chwili (null, gdy postać nie używa umiejętności).</summary>
        public SpellRuntime ActiveSkill => Actions.Current == ActionType.Cast ? currentSpell : null;

        /// <summary>Czy umiejętność w slocie da się teraz użyć (odnowienie, zasób, wyposażenie) – dla HUD.</summary>
        public bool SkillUsable(int slot)
        {
            var sk = Skill(slot);
            if (sk == null || !sk.equipmentMet || !sk.instance.Ready || Stamina.Current <= 0) return false;
            return !sk.Def.IsSpell || Mana.Current + 0.01f >= sk.manaCost;
        }

        // ------------------------------------------------------------------ Żądania akcji

        public void Request(ActionType type)
        {
            if (IsDead) return;
            if (!TryExecute(type))
            {
                buffered = type;
                bufferedTime = clock;
            }
            else buffered = ActionType.None;
        }

        /// <summary>Użycie umiejętności ze slotu 0–2 (przyciski umiejętności 1/2/3).</summary>
        public void RequestSkill(int slot)
        {
            if (IsDead) return;
            // Przycisk klasy obsługuje wybrana cecha klasowa (zastępuje dawną sygnaturę).
            if (slot == SignatureSlot && Feature != null) { Feature.Press(); return; }
            // Druga faza cechy może przejąć przycisk slotu (np. wybór czaru do strzały) – bez zwykłego rzutu.
            if (Feature != null && slot >= 0 && slot < RunState.SkillSlotCount && Feature.InterceptSkillRequest(slot)) return;
            requestedSlot = slot;
            Request(ActionType.Cast);
        }

        public void RequestFlask(bool mana)
        {
            flaskIsMana = mana;
            Request(ActionType.Flask);
        }

        bool TryExecute(ActionType type)
        {
            switch (type)
            {
                case ActionType.LightAttack:
                {
                    if (Feature != null && Feature.InterceptAttack(ActionType.LightAttack)) return true;
                    if (UniquesInterceptAttack(ActionType.LightAttack)) return true;
                    var target = FindRiposteTarget();
                    if (target != null && Actions.CanStart(ActionType.Riposte))
                    {
                        riposteTarget = target;
                        return Actions.TryStart(ActionType.Riposte, ScaleStartup(B.riposteStartup), 0.05f, ScaleRecovery(B.riposteRecovery));
                    }
                    return StartAttack(ActionType.LightAttack, CurrentWeapon.light);
                }
                case ActionType.HeavyAttack:
                    if (Feature != null && Feature.InterceptAttack(ActionType.HeavyAttack)) return true;
                    if (UniquesInterceptAttack(ActionType.HeavyAttack)) return true;
                    return StartAttack(ActionType.HeavyAttack, CurrentWeapon.heavy);
                case ActionType.Parry:
                {
                    if (Feature != null && Feature.BlocksGuard) { CombatEvents.RaiseMessage("W tej formie nie da się parować", Color.gray); return true; }
                    var p = Build.parry;
                    if (p == null) { CombatEvents.RaiseMessage("Brak broni/tarczy zdolnej do parowania", Color.gray); return true; }
                    if (Stamina.Current <= 0 || !Actions.CanStart(ActionType.Parry)) return false;
                    // Okno parowania nie jest skracane przez tempo – tylko przygotowanie i zakończenie.
                    Actions.TryStart(ActionType.Parry, ScaleStartup(p.startup), p.activeWindow, ScaleRecovery(p.recovery));
                    Stamina.Drain(p.staminaCost, B.staminaRegenDelay);
                    return true;
                }
                case ActionType.Dodge:
                {
                    var d = Build.dodge;
                    if (Status.Rooted || Stamina.Current <= 0 || !Actions.CanStart(ActionType.Dodge)) return false;
                    Vector3 dir = MoveIntent.sqrMagnitude > 0.01f ? MoveIntent.normalized : -transform.forward;
                    var interrupted = Actions.Current;
                    var interruptedPhase = Actions.Phase;
                    Feature?.DodgeStarting(interrupted, interruptedPhase);
                    UniquesDodge(transform.position, dir);
                    DodgeDirection = dir;
                    Actions.TryStart(ActionType.Dodge, d.invulnStart, d.invulnDuration, Mathf.Max(0.05f, d.totalDuration - d.invulnStart - d.invulnDuration), B.dodgeCancelAfter);
                    if (Build.effects[PassiveEffectType.DodgeShockCharge] > 0) ShockCharged = true;
                    // Unik przerwał inkantację przed wypuszczeniem czaru – mana wraca.
                    if (interrupted == ActionType.Cast && interruptedPhase == ActionPhase.Startup) CancelCast();
                    FxLibrary.Dust(transform.position);
                    Sfx.Play(SoundId.Dodge, transform.position);
                    Stamina.Drain(d.staminaCost, B.staminaRegenDelay);
                    HeroDodgeStarted();
                    HuTrigger("dodge");
                    return true;
                }
                case ActionType.Cast:
                {
                    var skill = Skill(requestedSlot);
                    if (skill == null) { CombatEvents.RaiseMessage(requestedSlot == SignatureSlot ? "Ta klasa nie ma skilla klasy" : $"Slot {requestedSlot + 1} jest pusty", Color.gray); return true; }
                    var def = skill.Def;
                    if (requestedSlot != SignatureSlot && Feature != null)
                    {
                        string blocked = Feature.BlockCast(skill, requestedSlot);
                        if (blocked != null) { CombatEvents.RaiseMessage(blocked, new Color(0.6f, 0.6f, 0.9f)); return true; }
                    }
                    if (def.IsAura) return ToggleAura(skill);
                    if (requestedSlot != SignatureSlot && OverheatTime > 0f) { CombatEvents.RaiseMessage("Sloty przegrzane", new Color(0.8f, 0.5f, 1f)); return true; }
                    if (!skill.equipmentMet) { CombatEvents.RaiseMessage($"{def.displayName}: {Names.Requirement(def.requiredTags)}", Color.gray); return true; }
                    if (def.IsSpell && !skill.requirementsMet && B.spellRequirementsAreHard)
                    {
                        CombatEvents.RaiseMessage($"{def.displayName}: za mało Inteligencji", new Color(0.5f, 0.6f, 1f));
                        return true;
                    }
                    if (!def.IsSpell && WeaponThrown) { CombatEvents.RaiseMessage("Broń jeszcze w locie", Color.gray); return true; }
                    // Dwa takty: czar idzie osobnym torem – ciało zostaje wolne dla broni.
                    bool lane = requestedSlot != SignatureSlot && Feature != null && Feature.RoutesCastToLane(skill, requestedSlot);
                    if (!lane && !Actions.CanStart(ActionType.Cast)) return false;
                    if (!skill.instance.Ready) return false; // odnowienie – żądanie czeka w buforze
                    float lifeCost = HuLifeCost(skill);
                    if (lifeCost > 0f && Health.Current <= lifeCost) { CombatEvents.RaiseMessage("Za mało zdrowia", new Color(1f, 0.35f, 0.25f)); return true; }
                    bool free = lifeCost > 0f || (def.IsSpell && FreeCasts > 0 && def.kind != SpellKind.BloodPact);
                    // Cecha może opłacić koszt (żeton gniewu) albo pokryć brak many długiem – walidacja jest twarda, bez częściowej opłaty.
                    var receipt = default(CastReceipt);
                    bool featurePaid = !free && requestedSlot != SignatureSlot && Feature != null && Feature.TryPay(skill, requestedSlot, out receipt);
                    if (receipt.active && !featurePaid) return true; // cecha odrzuciła (komunikat podała)
                    if (def.IsSpell && !free && !featurePaid && Mana.Current + 0.01f < skill.manaCost) { CombatEvents.RaiseMessage("Za mało many", new Color(0.4f, 0.6f, 1f)); FeatureTelemetry.Reject("brak many"); return true; }
                    if (lifeCost <= 0f && !featurePaid && Stamina.Current <= 0) return false;
                    if (lane)
                    {
                        skill.instance.TryUse();
                        if (featurePaid) { } else if (lifeCost > 0f) Health.Drain(Mathf.Min(lifeCost, Health.Current - 1f));
                        else if (free) FreeCasts--;
                        else if (def.IsSpell) Mana.TrySpend(skill.manaCost);
                        (Feature as TwoBeatsFeature)?.QueueLane(skill, EffectiveCastTime(def), receipt, free && lifeCost <= 0f, lifeCost > 0f);
                        return true;
                    }
                    castReceipt = receipt;
                    currentSpell = skill;
                    NewOrigin();
                    float castTime = EffectiveCastTime(def); // szybkość rzucania: tylko przygotowanie ręcznego czaru
                    Actions.TryStart(ActionType.Cast, castTime, SkillActiveTime(def), ScaleRecovery(def.recovery), ScaleRecovery(def.cancelAfter));
                    skill.instance.TryUse();
                    tracker.Reset();
                    overloadThisCast = OverloadReady && requestedSlot != SignatureSlot && def.kind != SpellKind.Overload;
                    if (overloadThisCast) OverloadReady = false;
                    gatherFx = def.IsSpell ? HellFx.Gather(Hand, HellSkillFx.Family(def), castTime) : null;
                    if (def.IsSpell && castTime > 0.15f) Sfx.Play(SoundId.CastCharge, AimPoint, 0.8f, Mathf.Clamp(0.6f / castTime, 0.8f, 1.4f));
                    if (lifeCost > 0f)
                    {
                        // Krew płacona prosto z puli: bez pancerza i osłony – i nigdy ostatnia kropla.
                        Health.Drain(Mathf.Min(lifeCost, Health.Current - 1f));
                        CombatEvents.RaiseWorldText(AimPoint + Vector3.up, "-" + lifeCost.ToString("0"), new Color(1f, 0.3f, 0.2f));
                    }
                    else if (featurePaid) { } // cecha pobrała swoją opłatę (żeton/dług)
                    else if (free) FreeCasts--;
                    else if (def.IsSpell) Mana.TrySpend(skill.manaCost);
                    castWasFree = free && lifeCost <= 0f;
                    if (lifeCost <= 0f && !(featurePaid && receipt.token)) Stamina.Drain(skill.staminaCost, B.staminaRegenDelay);
                    return true;
                }
                case ActionType.Flask:
                {
                    int charges = flaskIsMana ? Run.manaFlasks : Run.healthFlasks;
                    if (charges <= 0) { CombatEvents.RaiseMessage("Flaszka pusta", Color.gray); return true; }
                    return Actions.TryStart(ActionType.Flask, ScaleStartup(B.flaskDrinkTime), 0.05f, ScaleRecovery(B.flaskRecovery));
                }
            }
            return false;
        }

        /// <summary>Dane broni do ataków: w czasie ciężkiego rzutu – pięści.</summary>
        WeaponData CurrentWeapon => WeaponThrown && Config.unarmed != null ? Config.unarmed.weapon : Build.weapon;

        bool StartAttack(ActionType type, AttackDefinition atk)
        {
            if (Stamina.Current <= 0 || !Actions.CanStart(type)) return false;
            if (type == ActionType.LightAttack && Actions.Current == ActionType.LightAttack && Actions.ComboIndex + 1 >= CurrentWeapon.lightComboLength)
                return false; // koniec serii – trzeba poczekać
            // Dwie bronie: lekkie ciosy na zmianę – prawa, lewa, prawa…
            attackOffhand = type == ActionType.LightAttack && Build.offWeapon != null && Actions.Current == ActionType.LightAttack && Actions.ComboIndex % 2 == 0
                            && (Feature == null || Feature.AllowsOffhandAlternation) && !OffhandAway;
            if (type == ActionType.LightAttack && Build.offWeapon != null && !OffhandAway && OffhandFightsAlone) attackOffhand = true;
            if (attackOffhand) atk = Build.offWeapon.definition.weapon.light;
            else atk = ChooseMove(type, atk); // topór/młot: cios z biegu, po uniku
            atk = UniquesAttackOverride(type, atk); // moce unikatów (np. lekki atak różdżki jako cięcie)
            currentAttack = atk;
            // Zręczność przyspiesza ataki bronią; broń bez wymaganych cech – wolniejsza.
            // Rozpęd (perk HU): seria zamachów bez trafienia zeruje warstwy.
            if (swingOpen && !swingLanded) momentumStacks = 0;
            swingOpen = type == ActionType.LightAttack || type == ActionType.HeavyAttack;
            swingLanded = false;
            // Pojedynkowicz: dodatek do tej samej sumy tempa (nie mnożnik), tylko kwalifikowany zwykły atak.
            float aspd = Mathf.Max(0.3f, Build.attackSpeed + MomentumBonus + DuelistBonus(type, atk, attackOffhand));
            float timeScale = Feature != null ? Feature.AttackTimeScale(type, atk) : 1f;
            AttackWindup = ScaleStartup(atk.windup) / aspd * timeScale;
            AttackActive = ScaleStartup(atk.active) / aspd * timeScale;
            Actions.TryStart(type, AttackWindup, AttackActive, ScaleRecovery(atk.recovery) / aspd * timeScale, ScaleRecovery(atk.cancelAfter) / aspd * timeScale);
            Stamina.Drain(AttackStaminaCost(type, atk), B.staminaRegenDelay);
            NewOrigin();
            Feature?.AttackStarted(type, atk, ActionOrigin);
            UniquesAttackStarted(type, atk, ActionOrigin);
            return true;
        }

        bool attackOffhand;

        // Rozpęd (perk HU): każdy zamach wręcz, który trafił, dodaje warstwę (do 5); pudło albo 1,5 s bez trafienia zeruje.
        int momentumStacks;
        float momentumLastHit;
        bool swingOpen, swingLanded;

        /// <summary>Premia rozpędu do mnożnika szybkości ataku.</summary>
        public float MomentumBonus => clock - momentumLastHit > PerkMechanics.MomentumTimeout ? 0f
            : momentumStacks * Build.effects[PassiveEffectType.MomentumAttackSpeed] / 100f;

        /// <summary>Perki zwykłego ataku bronią (HU zakres „broń główna”): konwersja, efekty trafienia, odrzut.</summary>
        void BasicAttackPerks(ref HitData hit, bool melee, IHitReceiver target)
        {
            var fx = Build.effects;
            if (melee)
            {
                float conv = Mathf.Min(1f, fx[PassiveEffectType.ConvertPhysicalToFire]);
                if (conv > 0f && hit.physical > 0f && (hit.element == Element.None || hit.element == Element.Fire))
                {
                    float moved = hit.physical * conv;
                    hit.physical -= moved; hit.magic += moved; hit.element = Element.Fire;
                }
                if (fx[PassiveEffectType.BleedOnHit] > 0f) AddStatus(ref hit, StatusKind.Bleed);
                float kb = fx[PassiveEffectType.MeleeKnockback];
                if (kb > 0f && target is EnemyBrain e) hit.knockback += kb * (e.Def.isBoss ? 0.25f : e.IsElite ? 0.5f : 1f);
            }
            else if (fx[PassiveEffectType.MeleeKnockback] > 0f) hit.knockback += fx[PassiveEffectType.MeleeKnockback]; // HU: odrzut zwykłego ataku także z pocisku (różdżka powietrza); bossy słabiej – EnemyBrain
            if (fx[PassiveEffectType.BurnOnHit] > 0f) AddStatus(ref hit, StatusKind.Burn);
            if (fx[PassiveEffectType.ChillOnHit] > 0f) AddStatus(ref hit, StatusKind.Chill);
            if (fx[PassiveEffectType.SoakedOnHit] > 0f) AddStatus(ref hit, StatusKind.Soaked);
            if (fx[PassiveEffectType.StaggerOnHit] > 0f) AddStatus(ref hit, StatusKind.Stagger);
        }

        /// <summary>Dokłada efekt do trafienia, jeśli atak go jeszcze nie nakłada (HU: perk nie dubluje efektu broni).</summary>
        static void AddStatus(ref HitData hit, StatusKind kind)
        {
            if (hit.statuses != null && hit.statuses.Exists(s => s.kind == kind)) return;
            var merged = hit.statuses != null ? new List<StatusApplication>(hit.statuses) : new List<StatusApplication>();
            merged.Add(new StatusApplication(kind));
            hit.statuses = merged;
        }

        /// <summary>Czas fazy aktywnej umiejętności: szarża i młynek trwają, reszta działa w jednej chwili.</summary>
        float SkillActiveTime(SpellDefinition def)
        {
            if (currentSpell != null && currentSpell.Def == def && IsHu(currentSpell)) return HuActiveTime(currentSpell);
            switch (def.kind)
            {
                case SpellKind.Charge:
                case SpellKind.Flurry:
                case SpellKind.Counter: return ScaleStartup(Mathf.Max(0.1f, def.duration));
                case SpellKind.Whirlwind: return ScaleStartup(Mathf.Max(0.1f, def.duration + (currentSpell != null && currentSpell.Def == def ? F(currentSpell, LevelFeatureKind.DurationBonus) : 0f)));
                case SpellKind.Rupture: return ScaleStartup(Mathf.Max(0.05f, def.attack.active));
                case SpellKind.Dash: return Mathf.Max(0.05f, def.dashTime);
                case SpellKind.Cleave:
                case SpellKind.Quake:
                case SpellKind.ShieldBash: return ScaleStartup(Mathf.Max(0.05f, def.attack.active));
                default: return 0.05f;
            }
        }

        /// <summary>Unik przerwał umiejętność przed jej wyzwoleniem – wraca mana i ładunek (wytrzymałość przepada).</summary>
        void CancelCast()
        {
            if (currentSpell != null)
            {
                if (castReceipt.active) Feature?.Refund(castReceipt);
                else if (castWasFree) FreeCasts++;
                else if (currentSpell.Def.IsSpell) Mana.Restore(currentSpell.manaCost);
                currentSpell.instance.Refund();
            }
            castReceipt = default;
            currentSpell = null;
            if (gatherFx != null) gatherFx.StopAndRelease();
            gatherFx = null;
        }

        EnemyBrain FindRiposteTarget()
        {
            EnemyBrain best = null; float bestD = float.MaxValue;
            foreach (var e in EnemyBrain.All)
            {
                if (!e.CanBeRiposted) continue;
                Vector3 to = e.transform.position - transform.position; to.y = 0;
                float d = to.magnitude;
                if (d > B.riposteRange) continue;
                if (Vector3.Angle(transform.forward, to) > B.riposteAngle * 0.5f) continue;
                if (d < bestD) { bestD = d; best = e; }
            }
            return best;
        }

        // ------------------------------------------------------------------ Fazy akcji

        void OnPhaseChanged(ActionType type, ActionPhase phase)
        {
            switch (type)
            {
                case ActionType.LightAttack:
                case ActionType.HeavyAttack:
                    if (phase == ActionPhase.Startup) tracker.Reset();
                    if (phase == ActionPhase.Active && currentAttack != null && currentAttack.delivery != AttackDelivery.Projectile) PlaySwing(currentAttack.heavy);
                    // Broń dystansowa (łuk): w fazie aktywnej wypuszcza strzały zamiast cięcia.
                    if (phase == ActionPhase.Active && currentAttack != null && currentAttack.delivery == AttackDelivery.Projectile) FireArrows(currentAttack);
                    if (phase == ActionPhase.Active && currentAttack != null) { Feature?.AttackActive(type, currentAttack, ActionOrigin); UniquesAttackActive(type, currentAttack, ActionOrigin); }
                    if (phase == ActionPhase.Recovery) MoveShockwave();
                    break;
                case ActionType.Cast:
                    if (phase == ActionPhase.Active) { skillActiveStart = clock; FireSpell(currentSpell); castReceipt = default; Feature?.CastFired(currentSpell, ActionOrigin); UniquesCastFired(currentSpell, ActionOrigin); }
                    if (IsHu(currentSpell))
                    {
                        if (phase == ActionPhase.Recovery || phase == ActionPhase.Finished) { SkillMotion = Vector3.zero; huDash?.EndNow(); huDash = null; }
                        if (phase == ActionPhase.Finished) { ConsumeOverload(); huSpin?.Stop(); huSpin = null; }
                        break;
                    }
                    if (phase == ActionPhase.Recovery && currentSpell != null && currentSpell.Def.kind == SpellKind.Dash) DashEnd(currentSpell);
                    if (phase == ActionPhase.Recovery && currentSpell != null && currentSpell.Def.kind == SpellKind.Whirlwind && currentSpell.Def.finalSlashShare > 0f)
                    {
                        // Taniec ostrzy: szerokie cięcie na koniec wiru.
                        var sk = currentSpell;
                        var t2 = new HitTracker();
                        float fr = Radius(sk, sk.Def.finalRadius > 0 ? sk.Def.finalRadius : sk.Def.attack.radius);
                        HitQuery.Sphere(transform.position + Vector3.up, fr, Faction.Player, t2, t =>
                        {
                            var h = SkillHitData(sk, transform.position);
                            h.physical *= sk.Def.finalSlashShare; h.magic *= sk.Def.finalSlashShare;
                            HitQuery.Apply(this, t, h, true);
                        });
                        HellSkillFx.Shockwave(sk.Def, transform.position, fr, false);
                        PlaySwing(true);
                    }
                    if (phase == ActionPhase.Finished) ConsumeOverload();
                    if (phase == ActionPhase.Recovery && currentSpell != null && currentSpell.Def.kind == SpellKind.Whirlwind && F(currentSpell, LevelFeatureKind.FinalSlash) > 0)
                    {
                        // Młynek na wysokim poziomie kończy się mocnym cięciem dookoła.
                        var sk = currentSpell;
                        float mult = F(sk, LevelFeatureKind.FinalSlash);
                        tracker.Reset();
                        HitQuery.Sphere(transform.position + Vector3.up, Radius(sk, sk.Def.attack.radius), Faction.Player, tracker, t =>
                        {
                            var h = SkillHitData(sk, transform.position);
                            h.physical *= mult; h.magic *= mult; h.poiseDamage *= 3f;
                            HitQuery.Apply(this, t, h, true);
                        });
                        HellSkillFx.Shockwave(sk.Def, transform.position, Radius(sk, sk.Def.attack.radius), false);
                        PlaySwing(true);
                    }
                    else if (phase == ActionPhase.Recovery || phase == ActionPhase.Finished)
                    {
                        if (SkillMotion != Vector3.zero) FxLibrary.Dust(transform.position, 1.3f);
                        SkillMotion = Vector3.zero;
                    }
                    break;
                case ActionType.Flask:
                    if (phase == ActionPhase.Active) DrinkFlask();
                    break;
                case ActionType.Riposte:
                    if (phase == ActionPhase.Active) PerformRiposte();
                    break;
            }
        }

        void DrinkFlask()
        {
            float potency = 1f + Build.stats[StatType.FlaskPotency] / 100f;
            if (flaskIsMana)
            {
                if (Run.manaFlasks <= 0) return;
                Run.manaFlasks--;
                FxLibrary.Drink(transform, new Color(0.35f, 0.6f, 1f));
                Sfx.Play(SoundId.Flask, AimPoint, 1f, 1.1f);
                restores.Add(new TimedRestore { mana = true, remaining = B.flaskEffectDuration, perSecond = Mana.Max * B.manaFlaskFraction * potency / B.flaskEffectDuration });
            }
            else
            {
                if (Run.healthFlasks <= 0) return;
                Run.healthFlasks--;
                UniquesManualHeal(true);
                FxLibrary.Drink(transform, new Color(1f, 0.55f, 0.15f));
                Sfx.Play(SoundId.Flask, AimPoint);
                restores.Add(new TimedRestore { mana = false, remaining = B.flaskEffectDuration, perSecond = Health.Max * B.healthFlaskFraction * potency / B.flaskEffectDuration });
            }
        }

        void PerformRiposte()
        {
            if (riposteTarget == null || !riposteTarget.CanBeRiposted) return;
            // Riposta omija zwykły potok trafienia: premia ogólna i dodane żywioły wchodzą tu jawnie, bez egzekucji i bufów ciosu.
            float strike = Build.weapon.riposteMultiplier * (1f + Build.stats[StatType.RiposteDamage] / 100f) * (1f + Build.stats[StatType.GlobalDamage] / 100f);
            float dmg = Build.WeaponDamage(Build.weapon.light) * strike;
            var elements = RiposteElements(Build.WeaponBase(Build.weapon.light) * strike);
            var target = riposteTarget;
            NewOrigin(); // riposta to nowa ręczna akcja (pochodzenie trafienia)
            if (UniquesOverrideRiposte(target, dmg + elements.magic)) return; // moc unikatu: fala, czar zamiast riposty (wyłom – w skutku głównym)
            float breach = ConsumeBreachRiposte(target); // Wykorzystaj wyłom: jeden zastępowany skutek, riposta zawsze trafia
            dmg *= breach;
            if (breach != 1f) elements.ScaleMagicByElement(_ => breach);
            float hpBefore = target.Health.Current;
            target.ReceiveRiposte(dmg, this, elements);
            float dealt = hpBefore - target.Health.Current;
            // Riposta omija HitQuery.Apply – reakcja ciała, hitstop i warstwy dźwięku przez to samo zdarzenie.
            CombatEvents.RaiseImpact(target, new HitData
            {
                physical = dmg, heavy = true, isRiposte = true, fromMeleeWeapon = true, weapon = HeldWeaponModel,
                sourcePosition = transform.position, attacker = this, attackName = "Riposta",
            }, new HitResult { outcome = HitOutcome.Hit, healthDamage = dealt });
            FxLibrary.Flash(riposteTarget.AimPoint, new Color(1f, 0.8f, 0.35f), 1.4f);
            FxLibrary.Blood(riposteTarget.AimPoint, transform.forward, 2f);
            Sfx.Play(SoundId.Riposte, riposteTarget.AimPoint);
            float heal = Build.effects[PassiveEffectType.RiposteHeal];
            if (heal > 0) Health.Restore(heal);
            CombatEvents.RaiseMessage("RIPOSTA!", new Color(1f, 0.85f, 0.3f));
        }

        void FireSpell(SpellRuntime spell)
        {
            if (spell == null) return;
            var def = spell.Def;
            float refund = def.IsSpell ? Build.effects[PassiveEffectType.SpellStaminaRefund] : 0f;
            if (refund > 0) Stamina.Restore(refund);
            PlaySkillSound(def);
            HellSkillFx.Cast(def, Hand.position, AimDirection());
            if (IsHu(spell) && !def.IsAura) { FireHu(spell); return; }
            if (FireHeroSpell(spell)) return;

            switch (def.kind)
            {
                case SpellKind.Projectile:
                {
                    Vector3 origin = transform.position + Vector3.up * 1.3f + transform.forward * 0.8f;
                    Vector3 aim = LockTarget != null ? (LockTarget.AimPoint - origin) : transform.forward;
                    aim.y = LockTarget != null ? aim.y : 0f;
                    int count = Mathf.Max(1, def.attack.projectileCount + Mathf.RoundToInt(F(spell, LevelFeatureKind.ExtraTargets)));
                    float spread = def.attack.spreadAngle > 0 ? def.attack.spreadAngle : (count > 1 ? 8f : 0f);
                    // Pełny pierścień (wachlarz ostrzy): pociski równo dookoła; wtedy bez naprowadzania.
                    bool ring = spread >= 179f;
                    var opts = new ProjectileOptions
                    {
                        pierce = def.pierce + Mathf.RoundToInt(F(spell, LevelFeatureKind.ExtraTargets) * (def.pierce > 0 ? 1f : 0f)),
                        split = def.split, fullExplosion = def.fullExplosion, knockback = def.knockback,
                        zoneRadius = def.zoneRadius, zoneDuration = def.zoneDuration, zoneInterval = def.zoneInterval, zoneShare = def.zoneShare,
                    };
                    for (int i = 0; i < count; i++)
                    {
                        float angle = count == 1 ? 0 : ring ? i * 360f / count : Mathf.Lerp(-spread, spread, i / (float)(count - 1));
                        Vector3 dir = Quaternion.Euler(0, angle, 0) * aim.normalized;
                        var hit = SkillHitData(spell, origin);
                        bool arrow = (def.requiredTags & BuildTag.Ranged) != 0 || !def.IsSpell; // strzały z łuku, noże
                        Vector3 from = ring ? transform.position + Vector3.up * 0.9f + dir * 0.7f : origin;
                        Projectile.Spawn(from, dir, def.attack.projectileSpeed * ProjectileSpeedMult, hit, Faction.Player, this, def.color, ring ? 0.3f : def.IsSpell ? 0.22f : 0.12f,
                            def.projectileLifetime > 0 ? def.projectileLifetime : 3f,
                            ring ? null : LockTarget, LockTarget != null && !ring ? (arrow ? 60f : 90f) : 0f, arrow, opts);
                    }
                    break;
                }
                case SpellKind.Nova:
                {
                    var hit = SkillHitData(spell, transform.position);
                    tracker.Reset();
                    float novaR = Radius(spell, def.attack.radius);
                    HitQuery.Sphere(transform.position + Vector3.up, novaR, Faction.Player, tracker,
                        r => HitQuery.Apply(this, r, hit, false));
                    HellSkillFx.Shockwave(def, transform.position, novaR);
                    break;
                }
                case SpellKind.Heal:
                    UniquesManualHeal(false);
                    restores.Add(new TimedRestore { mana = false, remaining = def.duration, perSecond = spell.power / Mathf.Max(0.1f, def.duration) });
                    FxLibrary.Heal(transform, def.color, def.duration);
                    break;
                case SpellKind.WeaponBuff:
                    WeaponBuffTime = def.duration;
                    weaponBuffDamage = spell.power;
                    WeaponBuffColor = def.color;
                    weaponBuffElement = def.attack.element;
                    weaponBuffStatuses = def.attack.statuses != null && def.attack.statuses.Count > 0 ? def.attack.statuses : null;
                    HellSkillFx.Shockwave(def, transform.position, 1.2f, false);
                    CombatEvents.RaiseMessage($"{def.displayName}: +{spell.power:0} obrażeń {(weaponBuffElement != Element.None ? "(" + Names.Element(weaponBuffElement) + ")" : "magicznych")} na cios", def.color);
                    break;
                case SpellKind.Cone:
                    ArcHit(spell, Radius(spell, def.attack.reach), def.arcAngle);
                    HellFx.Cone(transform.position + Vector3.up * 1.2f + transform.forward * 0.5f, transform.forward, HellSkillFx.Family(def), Radius(spell, def.attack.reach), def.arcAngle);
                    break;
                case SpellKind.Chain:
                    ChainLightning(spell);
                    break;
                case SpellKind.Zone:
                {
                    Vector3 center = transform.position + transform.forward * def.attack.reach * 0.6f;
                    if (LockTarget != null && Vector3.Distance(LockTarget.transform.position, transform.position) <= def.attack.reach)
                        center = LockTarget.transform.position;
                    center.y = transform.position.y;
                    float zs = ManifestationScale(def, false); // Gwałtowna manifestacja: ten sam harmonogram w krótszym czasie
                    CountManifestation(Duration(spell, def.duration), zs);
                    var zone = DamageZone.Spawn(center, Radius(spell, def.attack.radius), Duration(spell, def.duration), def.tickInterval, SkillHitData(spell, center), Faction.Player, this, def.color, zs);
                    if (ManifestationQualifies(def, false)) zone.MeasuredBy = this;
                    break;
                }
                case SpellKind.Flurry:
                    skillHitTimer = 0f;
                    flurryIndex = -1;
                    break;
                case SpellKind.Warcry:
                    DamageBuffPct = def.amount * (1f + def.powerPerLevel * spell.instance.level);
                    DamageBuffTime = Duration(spell, def.duration);
                    Stamina.Restore(Stamina.Max * 0.4f);
                    HellSkillFx.Shockwave(def, transform.position, 3f);
                    CombatEvents.RaiseMessage($"{def.displayName}: +{DamageBuffPct:0}% obrażeń", def.color);
                    break;
                case SpellKind.Meteor:
                {
                    Vector3 c = TargetPoint(def.attack.reach);
                    float mr = Radius(spell, def.attack.radius);
                    DamageZone.Settings? zone = def.attack.radius > 0 && def.tickInterval > 0 && def.duration > 0
                        ? new DamageZone.Settings { radius = mr * 0.7f, duration = Duration(spell, def.duration), interval = def.tickInterval, hit = ZoneHit(spell, c), timeScale = ManifestationScale(def, false), measuredBy = ManifestationQualifies(def, false) ? this : null }
                        : (DamageZone.Settings?)null;
                    DelayedStrike.Spawn(c, mr, 1.0f, SkillHitData(spell, c), Faction.Player, this, def.color, zone);
                    break;
                }
                case SpellKind.Storm:
                {
                    var h = SkillHitData(spell, transform.position);
                    StormEffect.Spawn(transform, Radius(spell, def.attack.radius), Duration(spell, def.duration), def.tickInterval, h, this, def.color);
                    HellSkillFx.Shockwave(def, transform.position, 2f, false);
                    break;
                }
                case SpellKind.BloodPact:
                {
                    float cost = Mathf.Min(Health.Max * def.amount / 100f, Health.Current - 1f);
                    if (cost > 0) Health.Drain(cost);
                    FreeCasts = Mathf.Max(FreeCasts, Mathf.Max(1, def.attack.projectileCount));
                    FxLibrary.Blood(AimPoint, Vector3.up, 2f);
                    HellSkillFx.Shockwave(def, transform.position, 1.5f, false);
                    CombatEvents.RaiseMessage($"{def.displayName}: {FreeCasts} czary bez many", def.color);
                    break;
                }
                case SpellKind.FrostArmor:
                    EndBarrier(false);
                    BarrierAmount = spell.power;
                    BarrierTime = def.duration;
                    FrostArmorTime = def.duration;
                    barrierFx = HellFx.Ambient(transform, Vector3.up, HellFxElement.Ice, 0.7f);
                    HellSkillFx.Shockwave(def, transform.position, 1.6f, false);
                    CombatEvents.RaiseMessage($"{def.displayName}: osłona {spell.power:0}, odwet chłodem", def.color);
                    break;
                case SpellKind.Pull:
                {
                    Vector3 c = transform.position + transform.forward * 1.5f;
                    var h = SkillHitData(spell, transform.position);
                    tracker.Reset();
                    HitQuery.Sphere(transform.position + Vector3.up, def.attack.radius, Faction.Player, tracker, t =>
                    {
                        if (t is EnemyBrain e) { e.Pull(c, def.attack.reach); FxLibrary.Lightning(Hand.position, e.AimPoint, def.color); }
                        HitQuery.Apply(this, t, h, false);
                    });
                    HellSkillFx.Shockwave(def, transform.position, def.attack.radius, false);
                    break;
                }
                case SpellKind.Counter:
                    FxLibrary.Flash(transform.position + Vector3.up * 1.2f + transform.forward * 0.6f, def.color, 0.8f);
                    break;
                case SpellKind.WeaponThrow:
                {
                    // Broń leci do przodu i wraca; w locie rani każdego na drodze (w obie strony).
                    WeaponThrown = true;
                    var vis = GetComponent<CharacterVisual>();
                    if (vis != null) vis.SetWeaponVisible(false);
                    var sk = spell;
                    Vector3 origin = transform.position + Vector3.up * 1.2f + transform.forward * 0.6f;
                    Vector3 dir = LockTarget != null ? (LockTarget.AimPoint - origin) : transform.forward;
                    var model = AnimResolve.ForItem(Build.mainHand.definition);
                    if (model == WeaponModel.Knives) model = WeaponModel.Dagger;
                    ThrownWeapon.Spawn(this, origin, dir, Radius(sk, def.attack.reach), 16f, def.attack.radius, model,
                        Build.mainHand.definition.color, () => SkillHitData(sk, transform.position), CatchWeapon);
                    FxLibrary.Dust(transform.position, 1f);
                    break;
                }
                case SpellKind.Rupture:
                {
                    Vector3 fwd = transform.forward;
                    var rt = Build.effects;
                    HitQuery.Sphere(transform.position + Vector3.up, def.attack.reach, Faction.Player, tracker, t =>
                    {
                        Vector3 to = t.Transform.position - transform.position; to.y = 0;
                        if (to.sqrMagnitude > 0.04f && Vector3.Angle(fwd, to) > def.arcAngle * 0.5f) return;
                        TechniqueHit(t, spell);
                        if (t is EnemyBrain e) e.RuptureBleed(def.amount * (1f + def.powerPerLevel * spell.instance.level));
                    });
                    break;
                }
                case SpellKind.Barrier:
                    EndBarrier(false);
                    BarrierAmount = spell.power;
                    BarrierTime = def.duration;
                    barrierFx = HellFx.Ambient(transform, Vector3.up, def.attack.element != Element.None ? HellFx.For(def.attack.element) : HellFxElement.Earth, 0.7f);
                    HellSkillFx.Shockwave(def, transform.position, 1.6f, false);
                    CombatEvents.RaiseMessage($"{def.displayName}: osłona {spell.power:0}", def.color);
                    break;

                // ---- Techniki bronią
                case SpellKind.Cleave:
                case SpellKind.ShieldBash:
                    ArcHit(spell, Radius(spell, def.attack.reach), def.arcAngle);
                    if (def.kind == SpellKind.ShieldBash) FxLibrary.Flash(transform.position + Vector3.up + transform.forward * 1.1f, def.color, 1.2f);
                    else HellSkillFx.Shockwave(def, transform.position + transform.forward * 0.8f, def.attack.reach * 0.8f, false);
                    break;
                case SpellKind.Quake:
                {
                    Vector3 center = transform.position + transform.forward * def.attack.reach;
                    HitQuery.Sphere(center + Vector3.up * 0.5f, Radius(spell, def.attack.radius), Faction.Player, tracker, t => TechniqueHit(t, spell));
                    HellSkillFx.Shockwave(def, center, Radius(spell, def.attack.radius));
                    FxLibrary.Dust(center, 2f);
                    break;
                }
                case SpellKind.Charge:
                    chargeDir = transform.forward;
                    lastChargePos = transform.position;
                    FxLibrary.Dust(transform.position, 1.2f);
                    break;
                case SpellKind.Whirlwind:
                    skillHitTimer = 0f;
                    FxLibrary.Dust(transform.position, 1.2f);
                    break;
            }
        }

        /// <summary>Trwające umiejętności (szarża, młynek) – wywoływane co klatkę w fazie aktywnej.</summary>
        void SkillActiveTick(float dt)
        {
            var sk = currentSpell;
            if (sk == null) return;
            if (IsHu(sk)) { HuActiveTick(); return; }
            var def = sk.Def;
            switch (def.kind)
            {
                case SpellKind.Dash:
                    DashTick(sk);
                    break;
                case SpellKind.Charge:
                {
                    // Dystans = attack.reach w czasie fazy aktywnej; trafia każdego wroga na drodze raz.
                    SkillMotion = chargeDir * (def.attack.reach / Mathf.Max(0.05f, ScaleStartup(def.duration)));
                    Vector3 up = Vector3.up * 1.0f;
                    HitQuery.Capsule(lastChargePos + up, transform.position + up + chargeDir * 0.6f, def.attack.radius, Faction.Player, tracker, t => TechniqueHit(t, sk));
                    lastChargePos = transform.position;
                    break;
                }
                case SpellKind.Whirlwind:
                {
                    skillHitTimer -= dt;
                    if (skillHitTimer > 0) break;
                    skillHitTimer = Mathf.Max(0.1f, def.tickInterval);
                    tracker.Reset();
                    PlaySwing(false);
                    HitQuery.Sphere(transform.position + Vector3.up, Radius(sk, def.attack.radius), Faction.Player, tracker, t => TechniqueHit(t, sk));
                    break;
                }
                case SpellKind.Flurry:
                {
                    // Seria cięć na przemian z prawej i lewej; każde cięcie może trafić ten sam cel ponownie.
                    skillHitTimer -= dt;
                    if (skillHitTimer > 0) break;
                    skillHitTimer = Mathf.Max(0.08f, ScaleStartup(def.tickInterval));
                    flurryIndex++;
                    tracker.Reset();
                    PlaySwing(false);
                    ArcHit(sk, def.attack.reach, def.arcAngle);
                    break;
                }
            }
        }

        /// <summary>Trafienia w łuku przed postacią (rozpłatanie, uderzenie tarczą).</summary>
        void ArcHit(SpellRuntime sk, float reach, float arc)
        {
            Vector3 fwd = transform.forward;
            HitQuery.Sphere(transform.position + Vector3.up, reach, Faction.Player, tracker, t =>
            {
                Vector3 to = t.Transform.position - transform.position; to.y = 0;
                if (to.sqrMagnitude > 0.04f && Vector3.Angle(fwd, to) > arc * 0.5f) return;
                TechniqueHit(t, sk);
            });
        }

        /// <summary>
        /// Łańcuch błyskawic: cel namierzony (albo najbliższy przed postacią), potem skoki do kolejnych wrogów
        /// w zasięgu <c>attack.radius</c>; każdy skok słabszy o 20%. Liczba celów = <c>attack.projectileCount</c>.
        /// </summary>
        void ChainLightning(SpellRuntime sk)
        {
            var def = sk.Def;
            Vector3 from = Hand.position;
            IHitReceiver target = LockTarget != null && !LockTarget.IsDead && Vector3.Distance(LockTarget.transform.position, transform.position) <= def.attack.reach
                ? LockTarget : NearestEnemy(transform.position, def.attack.reach, transform.forward, 80f, null);
            if (target == null)
            {
                FxLibrary.Lightning(from, transform.position + Vector3.up + transform.forward * def.attack.reach, def.color);
                Sfx.Play(SoundId.LightningZap, AimPoint);
                return;
            }
            var visited = new List<IHitReceiver>();
            float mult = 1f;
            int count = Mathf.Max(1, def.attack.projectileCount + Mathf.RoundToInt(F(sk, LevelFeatureKind.ExtraTargets)));
            for (int i = 0; i < count && target != null; i++)
            {
                FxLibrary.Lightning(from, target.AimPoint, def.color);
                Sfx.Play(SoundId.LightningZap, target.AimPoint, i == 0 ? 1f : 0.7f, 1f + i * 0.06f);
                var hit = SkillHitData(sk, from);
                hit.physical *= mult; hit.magic *= mult;
                HitQuery.Apply(this, target, hit, false);
                visited.Add(target);
                from = target.AimPoint;
                mult *= 0.8f;
                target = NearestEnemy(target.Transform.position, def.attack.radius, Vector3.zero, 360f, visited);
            }
        }

        static IHitReceiver NearestEnemy(Vector3 origin, float range, Vector3 forward, float maxAngle, List<IHitReceiver> exclude)
        {
            IHitReceiver best = null; float bestD = float.MaxValue;
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead || (exclude != null && exclude.Contains(e))) continue;
                Vector3 to = e.transform.position - origin; to.y = 0;
                float d = to.magnitude;
                if (d > range || d >= bestD) continue;
                if (forward != Vector3.zero && d > 0.3f && Vector3.Angle(forward, to) > maxAngle) continue;
                best = e; bestD = d;
            }
            return best;
        }

        /// <summary>Wartość cechy poziomu umiejętności (0, gdy nieodblokowana).</summary>
        static float F(SpellRuntime sk, LevelFeatureKind k) => sk == null ? 0f : sk.Def.Feature(k, sk.instance.level);
        /// <summary>Promień umiejętności: cecha poziomu i Szeroki zasięg (perk HU).</summary>
        float Radius(SpellRuntime sk, float r) => r * (1f + F(sk, LevelFeatureKind.RadiusBonus)) * AreaFactor;
        float AreaFactor => 1f + Build.effects[PassiveEffectType.AreaSize] / 100f;
        static float Duration(SpellRuntime sk, float d) => d + F(sk, LevelFeatureKind.DurationBonus);

        /// <summary>Broń wraca do dłoni po ciężkim rzucie.</summary>
        void CatchWeapon()
        {
            if (!WeaponThrown) return;
            WeaponThrown = false;
            var vis = GetComponent<CharacterVisual>();
            if (vis != null) vis.SetWeaponVisible(true);
        }

        /// <summary>Punkt celowania: namierzony wróg w zasięgu, inaczej przed postacią.</summary>
        Vector3 TargetPoint(float reach)
        {
            Vector3 c = transform.position + transform.forward * reach * 0.6f;
            if (LockTarget != null && Vector3.Distance(LockTarget.transform.position, transform.position) <= reach) c = LockTarget.transform.position;
            c.y = transform.position.y;
            return c;
        }

        /// <summary>Tyknięcie strefy po meteorze: 15% obrażeń meteoru na tyknięcie, bez wybuchu.</summary>
        HitData ZoneHit(SpellRuntime sk, Vector3 c)
        {
            var h = SkillHitData(sk, c);
            h.physical *= 0.15f; h.magic *= 0.15f; h.explosionRadius = 0; h.poiseDamage *= 0.1f;
            return h;
        }

        /// <summary>Obrażenia umiejętności: czar – moc czaru; technika – lekki atak broni × mnożnik.</summary>
        public float SkillDamage(SpellRuntime sk) => sk.Def.IsSpell || sk.Def.flatDamage || BuildCalculator.IsHeroKind(sk.Def.kind)
            ? sk.power : Build.WeaponDamage(Build.weapon.light) * sk.power;

        /// <summary>
        /// Dane trafienia umiejętności. Technika z żywiołem dzieli obrażenia pół na pół (fizyczne + żywioł),
        /// a zaklęta broń dokłada swój żywioł i efekty do technik bronią.
        /// </summary>
        HitData SkillHitData(SpellRuntime sk, Vector3 source)
        {
            var def = sk.Def;
            float dmg = SkillDamage(sk);
            if (overloadThisCast) dmg *= OverloadPower;
            var hit = HitData.FromAttack(def.attack, dmg, source, this);
            hit.attackName = def.displayName;
            hit.visualElement = HellSkillFx.Family(def);
            hit.knockback = def.knockback;
            if (!def.IsSpell)
            {
                if (def.attack.element != Element.None && def.attack.elementShare <= 0f) { hit.physical = dmg * 0.5f; hit.magic = dmg * 0.5f; }
                // Technika (Turris i HU): obrażenia oparte na broni – dodane żywioły raz, od bazy bez premii fizycznej.
                // Aura (tyknięcia) i przywołanie (trafienia sług) nie są uderzeniem bronią – bez dodatku i premii postawy.
                bool strike = def.kind != SpellKind.Aura && def.kind != SpellKind.Summon;
                if (strike) AddWeaponElements(ref hit, dmg / PhysicalFactor);
                if (def.kind != SpellKind.Projectile) { hit.fromMeleeWeapon = true; ApplyWeaponBuff(ref hit); if (strike) ApplyHeavyPoise(ref hit, false); }
            }
            int extraStacks = Mathf.RoundToInt(F(sk, LevelFeatureKind.ExtraStatusStacks));
            if (extraStacks > 0 && hit.statuses != null)
            {
                var more = new List<StatusApplication>();
                foreach (var st in hit.statuses) more.Add(new StatusApplication(st.kind, st.stacks + extraStacks, st.chance));
                hit.statuses = more;
            }
            if (hit.explosionRadius > 0) hit.explosionRadius = Radius(sk, hit.explosionRadius);
            hit.leaveZoneDuration = F(sk, LevelFeatureKind.LeaveZone);
            PrepareOutgoing(ref hit);
            return hit;
        }

        /// <summary>
        /// Wszystkie trafienia gracza przechodzą tędy: premie do żywiołu i krwawienia z wyposażenia, „płonąc zadajesz więcej”,
        /// ładunek porażenia po uniku (buty burzy) i modyfikatory efektów przenoszone do celu.
        /// </summary>
        void PrepareOutgoing(ref HitData hit)
        {
            var st = Build.stats; var fx = Build.effects;
            // Przemiana w parę (perk HU): ogień staje się mrozem – przed premiami do żywiołu.
            // Obejmuje też dodany ogień z affiksów (dodatek powstał wcześniej, przy tworzeniu trafienia broni).
            if (fx[PassiveEffectType.ConvertFireToFrost] >= 0.5f) hit.ConvertElement(Element.Fire, Element.Frost);
            if (hit.originId == 0 && !hit.isStatusTick && !hit.isReaction) hit.originId = ActionOrigin;
            // Cecha klasowa (rodziny podpaleń, Żar…) – przed premiami żywiołu i ogólnymi, które mnożą dodane składniki.
            Feature?.Outgoing(ref hit);
            UniquesOutgoing(ref hit);
            HeroOutgoing(ref hit);
            GearOutgoing(ref hit);
            if (hit.fromMeleeWeapon && fx[PassiveEffectType.SunderOnHit] > 0f) AddStatus(ref hit, StatusKind.Sunder);
            hit.physicalPenetration = fx[PassiveEffectType.PenetrationPhysical] / 100f;
            hit.accuracy = Build.accuracy;
            // Przebicie każdego żywiołu osobno – liczone przy odporności celu na jego składniku (bez uśredniania).
            hit.penFire = fx[PassiveEffectType.PenetrationFire] / 100f;
            hit.penFrost = fx[PassiveEffectType.PenetrationFrost] / 100f;
            hit.penLightning = fx[PassiveEffectType.PenetrationLightning] / 100f;
            hit.penEarth = fx[PassiveEffectType.PenetrationEarth] / 100f;
            if (hit.Elemental) hit.ScaleMagicByElement(e => e == Element.None ? 1f : 1f + st[Names.ElementDamageStat(e)] / 100f);
            if (DamageBuffTime > 0) { float m = 1f + DamageBuffPct / 100f; hit.physical *= m; hit.magic *= m; }
            if (Status.Burning && fx[PassiveEffectType.BurningDamageBonus] > 0)
            {
                float m = 1f + fx[PassiveEffectType.BurningDamageBonus] / 100f;
                hit.physical *= m; hit.magic *= m;
            }
            hit.mods = new StatusModifiers
            {
                burnDurationBonus = fx[PassiveEffectType.BurnDurationBonus],
                bleedStackBonus = Mathf.RoundToInt(fx[PassiveEffectType.BleedMaxStacksBonus]),
                bleedMovingBonus = fx[PassiveEffectType.BleedMovingBonus],
                bleedDamageMult = 1f + st[StatType.BleedDamage] / 100f,
                freezeReduction = Mathf.RoundToInt(fx[PassiveEffectType.FreezeStacksReduction]),
                conductionMult = 1f + fx[PassiveEffectType.ConductionBonus] / 100f,
                conductionJump = fx[PassiveEffectType.ConductionJump] > 0,
            };
            Feature?.OutgoingLate(ref hit);
            UniquesOutgoingLate(ref hit);
            if (ShockCharged && hit.fromMeleeWeapon)
            {
                ShockCharged = false;
                var merged = hit.statuses != null ? new List<StatusApplication>(hit.statuses) : new List<StatusApplication>();
                merged.Add(new StatusApplication(StatusKind.Shock));
                hit.statuses = merged;
            }
        }

        /// <summary>Zaklęta broń: dodatkowe obrażenia magiczne, a przy żywiole – także jego efekty.</summary>
        void ApplyWeaponBuff(ref HitData hit)
        {
            if (WeaponBuffTime <= 0) return;
            // Zaklęcie innego żywiołu niż broń (albo trafienie z dodanymi żywiołami): osobny składnik, nic nie przejmuje cudzego żywiołu.
            if (hit.split.active || (weaponBuffElement != Element.None && hit.element != Element.None && hit.element != weaponBuffElement && hit.magic > 0f))
                hit.AddMagic(weaponBuffElement, weaponBuffDamage);
            else hit.magic += weaponBuffDamage;
            if (weaponBuffElement == Element.None) return;
            if (hit.element == Element.None) hit.element = weaponBuffElement;
            if (weaponBuffStatuses == null) return;
            var merged = hit.statuses != null ? new List<StatusApplication>(hit.statuses) : new List<StatusApplication>();
            merged.AddRange(weaponBuffStatuses);
            hit.statuses = merged;
        }

        /// <summary>Obrażenia techniki = lekki atak broni × moc techniki (+ zaklęte ostrze). Liczy się jako cios bronią.</summary>
        void TechniqueHit(IHitReceiver target, SpellRuntime sk)
        {
            HitQuery.Apply(this, target, SkillHitData(sk, transform.position), !sk.Def.IsSpell);
        }

        void EndBarrier(bool broken)
        {
            if (broken) { CombatEvents.RaiseMessage("Osłona rozbita", new Color(0.8f, 0.7f, 0.5f)); Sfx.Play(SoundId.Shatter, AimPoint, 0.6f); barrierBroken = true; }
            BarrierAmount = 0;
            BarrierTime = 0;
            if (barrierFx != null)
            {
                barrierFx.StopAndRelease();
                barrierFx = null;
            }
        }

        // ------------------------------------------------------------------ Pętla

        void Update() => Tick(Time.deltaTime);

        /// <summary>Jeden krok symulacji (publiczny na potrzeby testów).</summary>
        public void Tick(float dt)
        {
            if (Run == null) return;
            if (IsDead) { deathTime += dt; return; }
            if (hitStop > 0f) { hitStop -= dt; return; }
            if (pushTime > 0f) pushTime -= dt;
            if (guardFaceTime > 0f) guardFaceTime -= dt;
            clock += dt;
            TimeSinceDodge = Actions.Current == ActionType.Dodge ? 0f : TimeSinceDodge + dt;

            // Efekty trwające: tyknięcia (osłona też je pochłania), szron/ogień/iskry na postaci.
            float statusDamage = Status.Tick(dt, MoveIntent.sqrMagnitude > 0.01f || Actions.Current == ActionType.Dodge, B);
            if (statusDamage > 0f) { TakeStatusDamage(statusDamage); if (IsDead) return; }
            statusFx.Update(Status, transform, 1f, dt);

            // Bufor wejścia – spróbuj ponownie wykonać zaległą akcję.
            if (buffered != ActionType.None)
            {
                if (clock - bufferedTime > B.inputBuffer) buffered = ActionType.None;
                else if (TryExecute(buffered)) buffered = ActionType.None;
            }

            // Chłód spowalnia także akcje (zamach, unik); zamrożenie trwa jako wymuszone drgnięcie.
            // Ładowany ciężki cios stoi z toporem cofniętym w tył i w dół, póki przycisk trzymany.
            chargingNow = ChargeHolding(dt);
            if (!chargingNow) Actions.Tick(Status.Incapacitated ? dt : dt * Status.ActionSpeedMultiplier(B));

            // Aktywna faza ataku bronią: przeciągnij hitbox (każdy cel najwyżej raz na zamach).
            if ((Actions.Current == ActionType.LightAttack || Actions.Current == ActionType.HeavyAttack) && Actions.Phase == ActionPhase.Active)
                SweepWeapon();
            if (Actions.Current == ActionType.Cast && Actions.Phase == ActionPhase.Active) SkillActiveTick(dt);
            else SkillMotion = Vector3.zero;

            // Odnowienia umiejętności (całej kolekcji – przełożenie do innego slotu niczego nie zeruje) i osłona.
            foreach (var s in Run.knownSpells) s.Tick(dt * CooldownSpeed);
            HeroTick(dt);
            GearTick(dt);
            UniquesTick(dt);
            if (chargingNow) UniquesChargeTick(ChargeTime, dt);
            if (BarrierTime > 0) { BarrierTime -= dt; if (BarrierTime <= 0) EndBarrier(false); }
            if (DamageBuffTime > 0) DamageBuffTime -= dt;
            if (FrostArmorTime > 0) FrostArmorTime -= dt;

            // Regeneracja
            var cur = Actions.Current;
            bool staminaRegenAllowed = cur == ActionType.None || cur == ActionType.Block || cur == ActionType.Flask;
            float stRegen = Build.stats[StatType.StaminaRegen];
            if (cur == ActionType.Block && Build.guard != null) stRegen *= Build.guard.staminaRegenMultiplier;
            if (Sprinting) { stRegen = 0; Stamina.Drain(B.sprintStaminaPerSecond * dt, 0.3f); }
            if (Feature != null && Feature.SuppressStaminaRegen) stRegen = 0f; // Walka w gardzie: atak, blok i unik na jednej puli
            Feature?.Tick(dt);
            Stamina.Tick(dt, staminaRegenAllowed ? stRegen : 0f);
            healSource = HealSource.Regen;
            Health.Tick(dt, Build.stats[StatType.HealthRegen]);
            healSource = HealSource.Other;
            Mana.Tick(dt, ManaRegenBlocked ? 0f : Build.stats[StatType.ManaRegen]); // Weksel lichwiarza: regeneracja nie spłaca długu

            for (int i = restores.Count - 1; i >= 0; i--)
            {
                var r = restores[i];
                float step = Mathf.Min(dt, r.remaining);
                if (r.mana) Mana.Restore(r.perSecond * step); else HealFrom(r.perSecond * step, HealSource.Flask);
                r.remaining -= dt;
                if (r.remaining <= 0) restores.RemoveAt(i); else restores[i] = r;
            }

            if (WeaponBuffTime > 0) WeaponBuffTime -= dt;
        }

        public void SetBlockHeld(bool held)
        {
            BlockHeld = held;
            if (IsDead) return;
            if (held && Feature != null && Feature.BlocksGuard) return;
            // W czasie rzutu blok bronią jest niemożliwy (tarcza – tak).
            if (held && Build.CanBlock && !(WeaponThrown && !Build.guardIsShield))
            {
                if (Actions.IsIdle) Actions.TryStartBlock();
            }
            else if (Actions.IsBlocking) Actions.EndBlock();
        }

        /// <summary>Strzały z łuku: celują w namierzonego wroga (lekkie naprowadzanie), inaczej prosto przed siebie.</summary>
        void FireArrows(AttackDefinition atk)
        {
            Sfx.Play(SoundId.BowRelease, AimPoint);
            Vector3 origin = transform.position + Vector3.up * 1.3f + transform.forward * 0.7f;
            Vector3 aim = LockTarget != null ? (LockTarget.AimPoint - origin) : transform.forward;
            if (LockTarget == null) aim.y = 0f;
            IHitReceiver homing = LockTarget;
            Feature?.ShotOrigin(ref origin, ref aim, ref homing);
            var fx = Build.effects;
            // Rozszczepienie (perk HU): łącznie 2/3/5/8 pocisków – wachlarz co 15°, od 8 pierścień; boczne za 40%.
            int volley = fx[PassiveEffectType.ProjectileVolley] > 0f ? PerkMechanics.VolleyCount(fx[PassiveEffectType.ProjectileVolley]) : 0;
            int count = Mathf.Max(1, Mathf.Max(atk.projectileCount, volley));
            float explosionShare = PerkMechanics.ExplosionShare(fx[PassiveEffectType.ProjectileExplosion]);
            var opts = new ProjectileOptions
            {
                pierce = Mathf.RoundToInt(fx[PassiveEffectType.ProjectilePierce]),
                ricochet = Mathf.RoundToInt(fx[PassiveEffectType.ProjectileRicochet]),
                split = fx[PassiveEffectType.ProjectileSplit] > 0f,
                explosionShare = explosionShare,
                explosionRadius = explosionShare > 0f ? Mathf.Max(atk.explosionRadius, PerkMechanics.MinExplosionRadius) * AreaFactor : 0f,
                residue = fx[PassiveEffectType.ProjectileResidue] > 0f,
            };
            for (int i = 0; i < count; i++)
            {
                float angle, share = 1f;
                if (volley > 0)
                {
                    angle = count >= 8 ? i * 360f / count : (i == 0 ? 0f : ((i + 1) / 2) * 15f * (i % 2 == 1 ? 1f : -1f));
                    if (i > 0) share = PerkMechanics.VolleySideShare;
                }
                else angle = count == 1 ? 0 : Mathf.Lerp(-atk.spreadAngle, atk.spreadAngle, i / (float)(count - 1));
                Vector3 dir = Quaternion.Euler(0, angle, 0) * aim.normalized;
                var hit = HitData.FromAttack(atk, Build.WeaponDamage(atk) * share, origin, this);
                hit.weapon = HeldWeaponModel; hit.basicAttack = true;
                AddWeaponElements(ref hit, Build.WeaponDamage(atk) * share / PhysicalFactor);
                if (hit.explosionRadius > 0f) hit.explosionRadius *= AreaFactor;
                ApplyWeaponBuff(ref hit);
                BasicAttackPerks(ref hit, false, null);
                Feature?.BasicShot(ref hit, i, count);
                UniquesBasicShot(ref hit, i, count);
                PrepareOutgoing(ref hit);
                // Łuk – strzały; różdżka (pocisk magiczny) – świecący pocisk w barwie żywiołu.
                bool arrow = atk.damageType == DamageType.Physical;
                Color col = WeaponBuffTime > 0 ? WeaponBuffColor : arrow ? new Color(1f, 0.95f, 0.85f) : Names.ElementColor(atk.element);
                if (UniquesCaptureShot(hit, origin, dir, atk.projectileSpeed * ProjectileSpeedMult, col, i, count)) continue;
                if (Feature != null && Feature.CaptureOwnShot(hit, dir, atk.projectileSpeed * ProjectileSpeedMult, col)) continue;
                Projectile.Spawn(origin, dir, atk.projectileSpeed * ProjectileSpeedMult, hit, Faction.Player, this, col,
                    arrow ? 0.12f : 0.18f, 3f, i == 0 ? homing : null, i == 0 && homing != null ? (arrow ? 60f : 90f) : 0f, arrow, opts);
            }
        }

        /// <summary>Świst broni: ciężka broń dwuręczna i ciężkie ataki – niski, lekkie – krótki i wysoki; pięści – krótki.</summary>
        void PlaySwing(bool heavyAttack)
        {
            var model = HeldWeaponModel;
            bool heavyWeapon = model == WeaponModel.GreatAxe || model == WeaponModel.GreatSword || model == WeaponModel.Hammer
                               || model == WeaponModel.Halberd || model == WeaponModel.Scythe;
            float pitch = model == WeaponModel.None ? 1.35f : model == WeaponModel.Dagger || model == WeaponModel.Knives ? 1.2f : 1f;
            Sfx.Play(heavyAttack || heavyWeapon ? SoundId.SwingHeavy : SoundId.SwingLight, AimPoint, 1f, pitch);
        }

        /// <summary>Dźwięk wypuszczenia umiejętności (rodzaj + żywioł).</summary>
        void PlaySkillSound(SpellDefinition def)
        {
            var el = def.attack != null ? def.attack.element : Element.None;
            bool arrow = (def.requiredTags & BuildTag.Ranged) != 0;
            switch (def.kind)
            {
                case SpellKind.Projectile: Sfx.Play(arrow ? SoundId.BowRelease : Sfx.ForElement(el, SoundId.ArcaneBolt), AimPoint); break;
                case SpellKind.Nova: Sfx.Play(SoundId.Shockwave, transform.position); if (el != Element.None) Sfx.Play(Sfx.ImpactFor(el), AimPoint, 0.7f); break;
                case SpellKind.Heal: Sfx.Play(SoundId.Heal, AimPoint); break;
                case SpellKind.WeaponBuff: Sfx.Play(SoundId.Enchant, AimPoint); if (el != Element.None) Sfx.Play(Sfx.ForElement(el, SoundId.Enchant), AimPoint, 0.5f); break;
                case SpellKind.Barrier: Sfx.Play(SoundId.Barrier, AimPoint); break;
                case SpellKind.FrostArmor: Sfx.Play(SoundId.Barrier, AimPoint); Sfx.Play(SoundId.FrostCast, AimPoint, 0.6f); break;
                case SpellKind.Cone: Sfx.Play(Sfx.ForElement(el, SoundId.FrostCast), AimPoint); break;
                case SpellKind.Zone: Sfx.Play(Sfx.ForElement(el, SoundId.FireCast), AimPoint); break;
                case SpellKind.Warcry: Sfx.Play(SoundId.Warcry, AimPoint); break;
                case SpellKind.Meteor: Sfx.Play(SoundId.FireCast, AimPoint, 0.8f, 0.7f); break;
                case SpellKind.Storm: Sfx.Play(SoundId.Thunder, AimPoint, 0.8f); break;
                case SpellKind.BloodPact: Sfx.Play(SoundId.BloodMagic, AimPoint); break;
                case SpellKind.Pull: Sfx.Play(SoundId.Chains, AimPoint); break;
                case SpellKind.Rupture: Sfx.Play(SoundId.BloodMagic, AimPoint); PlaySwing(false); break;
                case SpellKind.WeaponThrow: PlaySwing(true); break;
                case SpellKind.Cleave: PlaySwing(true); break;
                case SpellKind.ShieldBash: Sfx.Play(SoundId.Block, AimPoint + transform.forward, 0.8f, 0.85f); break;
                case SpellKind.Quake: Sfx.Play(SoundId.Quake, transform.position + transform.forward * def.attack.reach); break;
                case SpellKind.Charge: Sfx.Play(SoundId.Dodge, transform.position, 1f, 0.8f); Sfx.Play(SoundId.Warcry, AimPoint, 0.4f, 1.2f); break;
                case SpellKind.Counter: Sfx.Play(SoundId.Enchant, AimPoint, 0.5f, 1.3f); break;
                case SpellKind.Wave: PlaySwing(true); Sfx.Play(Sfx.ForElement(el, SoundId.Whiff), AimPoint, 0.6f); break;
                case SpellKind.Dash: Sfx.Play(SoundId.Dodge, transform.position, 1f, 0.8f); break;
                case SpellKind.Summon: Sfx.Play(SoundId.BloodMagic, AimPoint); break;
                case SpellKind.Overload: Sfx.Play(SoundId.CastCharge, AimPoint, 1f, 1.3f); break;
                case SpellKind.Undying: Sfx.Play(SoundId.Warcry, AimPoint); Sfx.Play(SoundId.BossRoar, AimPoint, 0.4f, 1.4f); break;
            }
        }

        void SweepWeapon()
        {
            var atk = currentAttack;
            if (atk == null || atk.delivery == AttackDelivery.Projectile) return;
            Vector3 from = transform.position + Vector3.up * 1.0f + transform.forward * 0.3f;
            float reach = atk.reach * (1f + Build.effects[PassiveEffectType.MeleeRange] / 100f); // Długie ostrze (perk HU)
            Vector3 to = transform.position + Vector3.up * 1.0f + transform.forward * reach;
            HitQuery.Capsule(from, to, atk.radius, Faction.Player, tracker, target =>
            {
                if (Feature != null && Feature.OverrideSweepHit(target, atk)) return;
                if (UniquesOverrideSweep(target, atk)) return;
                float dmg = attackOffhand ? Build.OffWeaponDamage(atk) : Build.WeaponDamage(atk);
                var hit = HitData.FromAttack(atk, dmg, transform.position, this);
                hit.fromMeleeWeapon = true; hit.basicAttack = true;
                hit.weapon = HeldWeaponModel;
                AddWeaponElements(ref hit, dmg / PhysicalFactor); // baza tej ręki, bez premii fizycznej
                ApplyWeaponBuff(ref hit);
                BasicAttackPerks(ref hit, true, target);
                ApplyMove(ref hit);
                ApplyHeavyPoise(ref hit, attackOffhand);
                PrepareOutgoing(ref hit);
                HitQuery.Apply(this, target, hit, true);
            });
        }

        public void OnHitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            HeroOnHitResolved(target, hit, result);
            Feature?.HitResolved(target, hit, result);
            UniquesHitResolved(target, hit, result);
            BreachResolved(target, hit, result);
            if (hit.fromMeleeWeapon && hit.weapon != WeaponModel.Auto && !swingLanded && swingOpen
                && (result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken || result.outcome == HitOutcome.Blocked))
            {
                swingLanded = true;
                if (clock - momentumLastHit > PerkMechanics.MomentumTimeout) momentumStacks = 0;
                momentumStacks = Mathf.Min(PerkMechanics.MomentumMaxStacks, momentumStacks + 1);
                momentumLastHit = clock;
            }
            if (result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken) GearOnLanded(target, hit, result);
            if (hit.fromMeleeWeapon && (result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken || result.outcome == HitOutcome.Blocked))
            {
                float m = Build.effects[PassiveEffectType.ManaOnMeleeHit];
                if (m > 0) Mana.Restore(m);
            }
            // Kosa: trafienie bronią krwawiącego wroga leczy.
            if (hit.fromMeleeWeapon && result.outcome == HitOutcome.Hit && target is EnemyBrain bleeding && bleeding.Status.BleedStacks > 0)
            {
                float heal = Build.effects[PassiveEffectType.BleedingHitHeal];
                if (heal > 0) PassiveHeal(heal);
            }
            if (target.IsDead)
            {
                float h = Build.effects[PassiveEffectType.HealOnKill];
                if (h > 0 && !hit.featureEffect) PassiveHeal(h, HealSource.Kill); // efekt cechy nie leczy za zabójstwo
                float mk = Build.effects[PassiveEffectType.ManaOnKill];
                if (mk > 0) Mana.Restore(mk);
                if (!hit.featureEffect) GearOnKill(target); // efekt cechy: bez żniwa i wybuchu zwłok (brak pętli zasobów)
                if (!hit.huTriggered) HuTrigger("kill", target.Transform.position);
            }
        }

        public void OnAttackParried(float poiseDamage)
        {
            // Przeciwnicy nie parują w prototypie; gdyby parowali – odrzut.
            Actions.Force(ActionType.Flinch, 0.8f);
        }

        // ------------------------------------------------------------------ Przyjmowanie trafień

        public DefenseState CurrentDefense()
        {
            var cur = Actions.Current;
            return new DefenseState
            {
                position = transform.position,
                forward = transform.forward,
                invulnerable = (cur == ActionType.Dodge && Actions.Phase == ActionPhase.Active) || cur == ActionType.Riposte || HeroInvulnerable,
                parryActive = cur == ActionType.Parry && Actions.Phase == ActionPhase.Active,
                parry = Build.parry,
                guarding = cur == ActionType.Block || GuardingDuringAttack,
                guard = Build.guard,
                stamina = Stamina.Current,
                physicalResist = Build.PhysicalReduction(B),
                magicResist = Build.MagicReduction(B),
                dead = IsDead,
            };
        }

        float shieldReadyAt;

        /// <summary>Walka w gardzie: trzymany blok osłania z przodu w trakcie własnego zwykłego ataku (nie czaru ze slotu).</summary>
        bool GuardingDuringAttack => Feature != null && Feature.GuardsDuringAttack && BlockHeld && Build != null && Build.guard != null
                                     && (Actions.Current == ActionType.LightAttack || Actions.Current == ActionType.HeavyAttack);

        /// <summary>Bezpośredni cios zabrał życie (nie tyknięcie) – dojrzewające dusze tracą część (HU).</summary>
        public event System.Action<float> HealthHit;

        /// <summary>Tylko testy: bez losowego bloku biernego tarczy (deterministyczne trafienia).</summary>
        public bool DebugNoPassiveBlock { get; set; }

        public HitResult ReceiveHit(HitData hit)
        {
            // Kontra: w aktywnym oknie cios wroga (który da się sparować) jest zatrzymany, a wróg traci postawę i dostaje cios.
            var counter = ActiveSkill;
            if (counter != null && counter.Def.kind == SpellKind.Counter && Actions.Phase == ActionPhase.Active && hit.parryable && hit.attacker is EnemyBrain countered && !hit.isStatusTick)
            {
                TechniqueHit(countered, counter);
                FxLibrary.Parry(AimPoint + transform.forward * 0.5f);
                CombatEvents.RaiseMessage("KONTRA!", counter.Def.color);
                // Utratę postawy napastnikowi zadaje HitQuery.Apply (jak przy parowaniu) – z tego wyniku.
                var cr = new HitResult { outcome = HitOutcome.Parried, attackerPoiseDamage = counter.Def.attack.poiseDamage };
                CombatEvents.RaiseHit(AimPoint, cr, true);
                return cr;
            }

            // Przewrót w cień: chwilę po uniku bezpośrednie ciosy chybiają.
            if (HeroEvades(hit))
            {
                var hidden = new HitResult { outcome = HitOutcome.Dodged };
                CombatEvents.RaiseHit(AimPoint, hidden, true);
                HuTrigger("defend");
                return hidden;
            }

            // Zręczność: bezpośrednie trafienie może chybić (bez efektów, bez utraty postawy). Tyknięcia i reakcje – zawsze trafiają.
            if (!hit.isStatusTick && !hit.isReaction && hit.dodgeable && hit.attacker != null && !IsDead
                && Build.evasionChance > 0 && UnityEngine.Random.value < Build.evasionChance)
            {
                var miss = new HitResult { outcome = HitOutcome.Dodged };
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.5f, "UCHYLENIE", new Color(0.7f, 0.95f, 1f));
                CombatEvents.RaiseHit(AimPoint, miss, true);
                HuTrigger("defend");
                return miss;
            }

            // Bezpośredni cios zawsze zostawia część surowych obrażeń (HU balans 2.0) – liczoną od ciosu przed obroną.
            float minimumDamage = !hit.isStatusTick && !hit.isReaction && hit.attacker != null ? hit.Total * B.directHitMinimumDamage : 0f;
            // Blok bierny z tarczą (HU): szansa na zatrzymanie całego bezpośredniego ciosu; ładunek odnawia się 1 / 0,92 / 0,85 s.
            // Kolejność HU: unik (niewrażliwość) i parowanie mają pierwszeństwo – blok bierny nie zabiera im ciosu.
            var cur = Actions.Current;
            bool covered = cur == ActionType.Block || cur == ActionType.Parry || cur == ActionType.Riposte
                           || (cur == ActionType.Dodge && Actions.Phase == ActionPhase.Active) || HeroInvulnerable;
            if (!hit.isStatusTick && !hit.isReaction && hit.blockable && hit.attacker != null && !IsDead && Build.passiveBlockChance > 0f && !covered
                && !DebugNoPassiveBlock && clock >= shieldReadyAt && UnityEngine.Random.value < Build.passiveBlockChance)
            {
                shieldReadyAt = clock + PerkMechanics.ShieldBlockCooldown(Build.shieldLevel);
                var blocked = new HitResult { outcome = HitOutcome.Blocked };
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.5f, "BLOK", new Color(0.85f, 0.85f, 0.95f));
                CombatEvents.RaiseHit(AimPoint, blocked, true);
                Sfx.Play(SoundId.Block, AimPoint, 0.6f);
                HuTrigger("defend");
                return blocked;
            }

            // Odporność na żywioł redukuje część z żywiołem (pełna do 40%, dalej połowa, limit 65%).
            if (hit.Elemental)
                hit.ScaleMagicByElement(e => e == Element.None ? 1f : 1f - B.HeroResist(Build.stats[Names.ElementResistStat(e)]) / 100f);
            string reaction = Status.ModifyIncoming(ref hit, B);
            var defense = CurrentDefense();
            // Blok: cios z boku (nie z pleców) – bohater błyskawicznie obraca się do napastnika i zasłania go tarczą/bronią.
            if (defense.guarding && defense.guard != null && hit.blockable && !hit.isStatusTick && GuardTurnTo(hit.sourcePosition, out var toAttacker))
                defense.forward = toAttacker;
            var r = DamageResolver.Resolve(hit, defense, B);
            if (r.outcome == HitOutcome.Ignored) return r;
            if (!hit.isStatusTick)
            {
                Vector3 d = transform.position - hit.sourcePosition; d.y = 0f;
                lastHitDir = d.sqrMagnitude > 1e-4f ? d.normalized : -transform.forward;
                lastHitWeight = hit.heavy ? 0.7f : 0.35f;
            }

            // Pancerz w ataku sprawdzony przed obrażeniami – HeroMitigate i drgnięcie widzą ten sam stan.
            bool hyperArmor = HyperArmor;
            // Redukcja obrażeń klasy/okrzyku/szału, arkanowa osłona i nieśmiertelność.
            if (r.healthDamage > 0) r.healthDamage = HeroMitigate(r.healthDamage, false, r.outcome == HitOutcome.Hit ? minimumDamage : 0f);
            // Płaszcz many (cecha): po redukcjach i rodzimej osłonie, przed barierą.
            if (r.healthDamage > 0 && Feature != null) r.healthDamage = Feature.AbsorbIncoming(r.healthDamage, false);

            // Osłona pochłania obrażenia przed utratą życia; w pełni pochłonięty cios nie wywołuje drgnięcia.
            bool absorbedAll = false;
            if (BarrierAmount > 0 && r.healthDamage > 0)
            {
                float absorbed = Mathf.Min(BarrierAmount, r.healthDamage);
                if (!hit.isStatusTick && hit.attacker is EnemyBrain) UniquesBarrier(BarrierAmount, BarrierAmount - absorbed, hit);
                BarrierAmount -= absorbed;
                r.healthDamage -= absorbed;
                absorbedAll = r.healthDamage <= 0.01f;
                if (BarrierAmount <= 0.01f) EndBarrier(true);
            }

            if (r.healthDamage > 0 && TryCheatDeath(r.healthDamage)) r.healthDamage = 0f;
            if (!hit.isStatusTick && hit.attacker is EnemyBrain && (r.outcome == HitOutcome.Hit || r.outcome == HitOutcome.GuardBroken)) UniquesStruck(hit, r, Feature is RedLineFeature rl ? 1f - rl.CurrentReduction : 1f);
            Health.Drain(r.healthDamage);
            if (r.outcome == HitOutcome.Hit && r.healthDamage > 0f && !hit.isStatusTick) HealthHit?.Invoke(r.healthDamage);
            // Ręczny blok: moc unikatu może przejąć część kosztu (Pawęż – mana zamiast wytrzymałości).
            if (r.staminaDamage > 0 && r.outcome == HitOutcome.Blocked && !hit.isStatusTick && (cur == ActionType.Block || GuardingDuringAttack))
                r.staminaDamage = UniquesBlockCost(r.staminaDamage, hit);
            if (r.staminaDamage > 0) Stamina.Drain(r.staminaDamage, B.staminaRegenDelay);
            if (!hit.isStatusTick && hit.attacker is EnemyBrain && ((r.outcome == HitOutcome.Blocked && (cur == ActionType.Block || GuardingDuringAttack)) || r.outcome == HitOutcome.Parried))
                UniquesDefended(r.outcome, hit);
            GearOnStruck(hit, r);

            switch (r.outcome)
            {
                case HitOutcome.Parried:
                {
                    var fx = Build.effects;
                    if (fx[PassiveEffectType.ParryRestoreStamina] > 0) Stamina.Restore(fx[PassiveEffectType.ParryRestoreStamina]);
                    if (fx[PassiveEffectType.ParryRestoreMana] > 0) Mana.Restore(fx[PassiveEffectType.ParryRestoreMana]);
                    if (fx[PassiveEffectType.ParryHeal] > 0) Health.Restore(fx[PassiveEffectType.ParryHeal]);
                    CombatEvents.RaiseMessage("PAROWANIE!", new Color(1f, 0.95f, 0.5f));
                    HuTrigger("defend");
                    break;
                }
                case HitOutcome.Blocked:
                {
                    float m = Build.effects[PassiveEffectType.BlockManaGain];
                    if (m > 0) Mana.Restore(m);
                    if (!hit.isStatusTick) HuTrigger("defend");
                    break;
                }
                case HitOutcome.GuardBroken:
                    Feature?.ForcedInterrupt(); UniquesForcedInterrupt();
                    Actions.Force(ActionType.GuardBroken, B.guardBreakStun);
                    CombatEvents.RaiseMessage("GARDA PRZEŁAMANA!", new Color(1f, 0.4f, 0.2f));
                    break;
                case HitOutcome.Hit:
                    // Szał i pancerz w ataku (broń dwuręczna): ciosy nie wybijają z rytmu.
                    if (!absorbedAll && !Raging && !hyperArmor && r.healthDamage > 0.01f) { Feature?.ForcedInterrupt(); UniquesForcedInterrupt(); Actions.Force(ActionType.Flinch, B.flinchDuration); }
                    break;
            }

            // Efekty trafienia (tylko gdy cios naprawdę doszedł – osłona w pełni pochłaniająca je zatrzymuje).
            if (!absorbedAll)
            {
                float instant = Status.OnLanded(hit, r, B, out var landedReaction, out bool froze);
                if (instant > 0) { Health.Drain(instant); r.healthDamage += instant; }
                reaction = reaction ?? landedReaction;
                if (froze)
                {
                    bool frozen = Status.Frozen;
                    Feature?.ForcedInterrupt(); UniquesForcedInterrupt();
                    Actions.Force(ActionType.Flinch, frozen ? Status.FrozenRemaining : Status.StunRemaining);
                    CombatEvents.RaiseMessage(frozen ? "ZAMROŻENIE!" : "OGŁUSZENIE!", Names.StatusColor(frozen ? StatusKind.Frozen : StatusKind.Stun));
                    Sfx.Play(frozen ? SoundId.Freeze : SoundId.Quake, AimPoint);
                }
            }
            if (reaction != null) { CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.7f, reaction, Names.ElementColor(hit.element)); Sfx.Play(Sfx.ImpactFor(hit.element), AimPoint, 0.8f); }

            // Mroźna zbroja: napastnik z bliska dostaje chłód.
            if (FrostArmorTime > 0 && hit.attacker is EnemyBrain striker && !hit.isStatusTick &&
                Vector3.Distance(striker.transform.position, transform.position) < 4f)
                striker.ApplyStatus(StatusKind.Chill, 0f, 10f);

            CombatEvents.RaiseHit(AimPoint, r, true);
            if (Health.Current <= 0 && !IsDead) Die();
            else if (!hit.isStatusTick && r.healthDamage > 0.01f && (r.outcome == HitOutcome.Hit || r.outcome == HitOutcome.GuardBroken)) HuTrigger("hurt");
            return r;
        }

        /// <summary>Tyknięcie efektu: już po pancerzu; osłona je pochłania, drgnięcia nie ma.</summary>
        void TakeStatusDamage(float dmg)
        {
            dmg = HeroMitigate(dmg, true);
            if (dmg > 0 && Feature != null) dmg = Feature.AbsorbIncoming(dmg, true);
            if (BarrierAmount > 0)
            {
                float absorbed = Mathf.Min(BarrierAmount, dmg);
                BarrierAmount -= absorbed;
                dmg -= absorbed;
                if (BarrierAmount <= 0.01f) EndBarrier(true);
            }
            if (dmg <= 0) return;
            if (TryCheatDeath(dmg)) return;
            Health.Drain(dmg);
            CombatEvents.RaiseWorldText(AimPoint, $"{dmg:0}", Status.BleedStacks > 0 ? Names.StatusColor(StatusKind.Bleed) : Names.StatusColor(StatusKind.Burn));
            Sfx.Play(Status.BleedStacks > 0 ? SoundId.BleedTick : SoundId.BurnTick, AimPoint);
            if (Health.Current <= 0 && !IsDead) Die();
        }

        /// <summary>Poligon umiejętności: bohater nie ginie (zdrowie wraca do pełna zamiast śmierci).</summary>
        public bool Immortal { get; set; }

        void Die()
        {
            if (Immortal) { Health.SetCurrent(Health.Max); return; }
            Actions.Force(ActionType.Dead, 0f);
            statusFx.Clear();
            Feature?.Death();
            UniquesDeath();
            HuClear();
            CombatEvents.RaiseMessage("POLEGŁEŚ", new Color(0.8f, 0.1f, 0.1f));
            Died?.Invoke();
        }

        // ------------------------------------------------------------------ Animacja

        public CharacterAnimState GetAnimState()
        {
            var a = Actions;
            var s = new CharacterAnimState
            {
                phase = a.Phase,
                phaseProgress = a.PhaseProgress,
                actionTime = a.Elapsed,
                actionDuration = a.FiniteDuration,
                windupSeconds = a.Current == ActionType.LightAttack || a.Current == ActionType.HeavyAttack ? AttackWindup : 0f,
                activeSeconds = a.Current == ActionType.LightAttack || a.Current == ActionType.HeavyAttack ? AttackActive : 0f,
                dodgeDirection = DodgeDirection,
                hitDirection = lastHitDir,
                hitWeight = lastHitWeight,
                rollDuration = Build != null ? Build.dodge.rollDuration : 0f,
                hasShield = Build != null && Build.guardIsShield,
                weaponGuard = Build != null && Build.guard != null && !Build.guardIsShield,
            };
            if (Build == null) return s;
            var weaponModel = AnimResolve.ForItem(Build.mainHand.definition);
            switch (a.Current)
            {
                case ActionType.LightAttack:
                    s.action = AnimAction.Attack;
                    s.attack = attackOffhand ? AttackAnim.SlashLeft : AnimResolve.ForAttack(currentAttack ?? Build.weapon.light, a.ComboIndex, weaponModel);
                    s.offhandAttack = attackOffhand;
                    break;
                case ActionType.HeavyAttack:
                    s.action = AnimAction.Attack;
                    s.attack = AnimResolve.ForAttack(currentAttack ?? Build.weapon.heavy, a.ComboIndex, weaponModel);
                    s.charge = chargingNow ? Mathf.Max(0.05f, ChargeTime / ChargeMax) : 0f;
                    break;
                case ActionType.Riposte: s.action = AnimAction.Attack; s.attack = AttackAnim.Thrust; break;
                case ActionType.Block: s.action = AnimAction.Block; break;
                case ActionType.Parry: s.action = AnimAction.Parry; break;
                case ActionType.Dodge: s.action = DodgeKind == DodgeStyle.ShoulderCharge ? AnimResolve.ChargeFor(transform, DodgeDirection) : AnimAction.Dodge; break;
                case ActionType.Cast:
                {
                    var def = currentSpell?.Def;
                    if (def == null || def.IsSpell) { s.action = AnimAction.Cast; break; }
                    // Techniki używają póz ataku bronią (lub wypchnięcia tarczy), zsynchronizowanych z fazami umiejętności.
                    s.action = AnimAction.Attack;
                    switch (def.kind)
                    {
                        case SpellKind.Charge: s.attack = AttackAnim.Thrust; break;
                        case SpellKind.Projectile: s.attack = AttackAnim.Thrust; break; // rzut
                        case SpellKind.Flurry:
                            s.attack = flurryIndex % 2 == 0 ? AttackAnim.SlashRight : AttackAnim.SlashLeft;
                            if (a.Phase == ActionPhase.Active)
                            {
                                float iv = Mathf.Max(0.08f, ScaleStartup(def.tickInterval));
                                s.phaseProgress = Mathf.Clamp01(1f - skillHitTimer / iv); // każde cięcie od zamachu do wybrzmienia
                            }
                            break;
                        case SpellKind.Quake: s.attack = AttackAnim.Slam; break;
                        case SpellKind.ShieldBash: s.action = AnimAction.Parry; break;
                        case SpellKind.Counter: s.action = a.Phase == ActionPhase.Active ? AnimAction.Block : AnimAction.Parry; break;
                        case SpellKind.Warcry: s.attack = AttackAnim.Burst; break;
                        case SpellKind.Undying: s.attack = AttackAnim.Burst; break;
                        case SpellKind.Dash: s.attack = AttackAnim.Thrust; break;
                        case SpellKind.Wave: s.attack = AttackAnim.SlashRight; break;
                        case SpellKind.Cone: s.attack = AttackAnim.SlashRight; break;
                        case SpellKind.Nova: s.attack = def.attack.element == Element.Earth || def.attack.element == Element.None ? AttackAnim.Slam : AttackAnim.Burst; break;
                        case SpellKind.Rupture: s.attack = AttackAnim.SlashLeft; break;
                        case SpellKind.WeaponThrow: s.attack = AttackAnim.Overhead; break;
                        case SpellKind.Whirlwind:
                            s.attack = AttackAnim.SlashRight;
                            if (a.Phase == ActionPhase.Active)
                            {
                                s.phaseProgress = 0.55f; // ramię wyprostowane, ciało wiruje
                                s.spinAngle = -(clock - skillActiveStart) * 900f;
                            }
                            break;
                        default: s.attack = AttackAnim.SlashRight; break;
                    }
                    // Animacja wskazana w danych umiejętności ma pierwszeństwo (np. Cięcie z wyskoku → skok).
                    if (s.action == AnimAction.Attack && def.attack.animation != AttackAnim.Auto && def.kind != SpellKind.Flurry) s.attack = def.attack.animation;
                    break;
                }
                case ActionType.Flask: s.action = AnimAction.Drink; break;
                case ActionType.Flinch: s.action = AnimAction.Flinch; break;
                case ActionType.GuardBroken: s.action = AnimAction.GuardBroken; break;
                case ActionType.Dead: s.action = AnimAction.Death; s.actionTime = deathTime; break;
            }
            return s;
        }

        /// <summary>Tylko do testów/debugowania.</summary>
        /// <summary>Kolor bieżącej umiejętności (poświata broni).</summary>
        public Color ActiveSkillColor => currentSpell != null ? currentSpell.Def.color : new Color(0.5f, 0.6f, 1f);

        public void DebugKill() { Health.Drain(Health.Current); Die(); }
    }
}
