using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    // ====================================================================== Wojownik A – Walka w gardzie

    /// <summary>
    /// Okno, w którym trzymany blok osłania z przodu również w trakcie własnego zwykłego ataku i techniki bronią (podstawowy strzał
    /// różdżki/łuku też – to zwykły atak). Czary ze slotów nie dostają gardy. Wytrzymałość nie odrasta: atak, blok i unik
    /// zjadają jedną pulę. Ochrona to zwykła garda (kąt, redukcja, koszt) – przełamanie przerywa cios, plecy są odsłonięte.
    /// </summary>
    public sealed class GuardFeature : ClassFeatureController
    {
        float active;
        public bool Active => active > 0f;
        protected override void OnBind() { color = new Color(0.8f, 0.85f, 1f); }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Walka w gardzie już trwa"); return; }
            if (pc.Build == null || pc.Build.guard == null) { Fail("brak gardy", "Walka w gardzie: brak gardy (tarcza albo broń, która blokuje)"); return; }
            active = T.guardSeconds;
            Used(T.guardCooldown, "WALKA W GARDZIE");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Physical, 0.5f, T.guardSeconds);
            Sfx.Play(SoundId.Block, pc.AimPoint, 0.7f, 0.8f);
        }

        public override void Tick(float dt) { base.Tick(dt); if (active > 0f) active -= dt; }
        public override bool GuardsDuringAttack => Active;
        public override bool SuppressStaminaRegen => Active;
        public override void FloorStart() { base.FloorStart(); active = 0f; }
        public override void Death() { active = 0f; }
        public override ClassFeatureHud Hud => Active ? Timed(ClassFeatureState.Active, $"garda w ataku {active:0.0} s", active, T.guardSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Wojownik B – Arsenał weterana

    /// <summary>
    /// Dwa zestawy główna/druga ręka; G zamienia je atomowo (RunState.SwapArsenal + przeliczenie buildu). Legalne okno: spoczynek,
    /// blok albo wybrzmienie ciosu (po fazie aktywnej) – etap serii zostaje, a wyzwolony atak ma już zapisane źródło i obrażenia.
    /// Najkrótszy odstęp zmian chroni przed usuwaniem powrotów ataku spamem.
    /// </summary>
    public sealed class ArsenalFeature : ClassFeatureController
    {
        float lastSwap = -99f;
        public int Swaps { get; private set; }
        protected override void OnBind() { color = new Color(0.85f, 0.8f, 0.7f); }

        protected override void OnPress()
        {
            if (Run.arsenalMain == null && Run.arsenalOff == null) { Fail("brak drugiej broni", "Arsenał: drugi zestaw jest pusty (ekwipunek → zestaw II)"); return; }
            var a = pc.Actions;
            bool inSwing = (a.Current == ActionType.LightAttack || a.Current == ActionType.HeavyAttack) && a.Phase != ActionPhase.Recovery;
            if (inSwing || (a.Current != ActionType.None && a.Current != ActionType.Block && a.Current != ActionType.LightAttack && a.Current != ActionType.HeavyAttack))
            { Fail("nie teraz", "Arsenał: zmiana dopiero po wyprowadzeniu ciosu"); return; }
            if (pc.Clock - lastSwap < T.arsenalSwapInterval) { Fail("za szybko", "Arsenał: za szybko"); return; }
            bool chain = a.Current == ActionType.LightAttack || a.Current == ActionType.HeavyAttack;
            if (!Run.SwapArsenal()) { Fail("brak drugiej broni", "Arsenał: drugi zestaw jest pusty"); return; }
            lastSwap = pc.Clock;
            Swaps++;
            Telemetry.Add("arsenał: zmiany");
            if (chain) Telemetry.Add("arsenał: kontynuacje serii");
            pc.FeatureRefresh();
            if (a.IsBlocking && !pc.Build.CanBlock) a.EndBlock();
            Used(0f);
            CombatEvents.RaiseMessage($"Zestaw: {pc.Build.mainHand.Name}" + (Run.equipment.Get(EquipSlot.OffHand) != null ? " + " + Run.equipment.Get(EquipSlot.OffHand).Name : ""), color);
            Sfx.Play(SoundId.SwingLight, pc.AimPoint, 0.6f, 1.3f);
        }

        public override ClassFeatureHud Hud
        {
            get
            {
                string second = Run.arsenalMain != null ? Run.arsenalMain.Name : Run.arsenalOff != null ? Run.arsenalOff.Name : "pusty";
                bool cd = pc.Clock - lastSwap < T.arsenalSwapInterval;
                return new ClassFeatureHud { state = Run.arsenalMain == null && Run.arsenalOff == null ? ClassFeatureState.Blocked : cd ? ClassFeatureState.Cooldown : ClassFeatureState.Ready,
                    label = $"zestaw II: {second}", fill = 1f, color = color };
            }
        }
        public override void FloorStart() { base.FloorStart(); lastSwap = -99f; }
    }

    // ====================================================================== Paladyn A – Rozgrzeszenie

    /// <summary>
    /// Nadleczenie (po limitach źródła, ponad pełne życie) zasila Łaskę ze współczynnikiem źródła; G wydaje całą Łaskę na falę
    /// osądu wokół paladyna. Fala to efekt cechy: nie kradnie życia, nie leczy za zabójstwo i nie tworzy Łaski (brak pętli).
    /// </summary>
    public sealed class AbsolutionFeature : ClassFeatureController
    {
        public float Grace { get; private set; }
        public float Capacity => pc.Health.Max * T.graceCapacity;
        protected override void OnBind() { color = new Color(1f, 0.85f, 0.45f); }

        public override void OnOverheal(float amount, HealSource source)
        {
            if (pc.IsDead || amount <= 0f) return;
            float k = source == HealSource.Regen ? T.graceFromRegen : source == HealSource.Leech ? T.graceFromLeech
                : source == HealSource.Kill ? T.graceFromKill : source == HealSource.Flask ? T.graceFromFlask : T.graceFromOther;
            float add = amount * k;
            float room = Mathf.Max(0f, Capacity - Grace);
            Telemetry.Add("Łaska: nadleczenie po limitach", amount);
            if (add > room) Telemetry.Add("Łaska: zmarnowana (pełny magazyn)", add - room);
            Grace += Mathf.Min(room, add);
            Telemetry.Max("Łaska: największy magazyn", Grace);
        }

        protected override void OnPress()
        {
            if (Grace < 1f) { Fail("brak Łaski", "Rozgrzeszenie: brak Łaski – lecz się przy pełnym życiu"); return; }
            float dmg = Grace * T.graceToDamage;
            var h = pc.FeatureHit("Fala osądu", 0f, dmg, Element.Fire, pc.transform.position);
            h.knockback = 1.2f; h.poiseDamage = 30f; h.visualElement = HellFxElement.Fire;
            var tr = new HitTracker();
            float r = T.graceWaveRadius;
            int hits = 0;
            HitQuery.Sphere(pc.transform.position + Vector3.up, r, Faction.Player, tr, x => { HitQuery.Apply(pc, x, h, false); hits++; });
            Telemetry.Add("Łaska: wydana", Grace);
            Telemetry.Add("Łaska: obrażenia fali", dmg * hits);
            Grace = 0f;
            Used(T.graceCooldown, "ROZGRZESZENIE");
            HellFx.Shockwave(pc.transform.position, HellFxElement.Fire, new Color(1f, 0.85f, 0.45f), r, true);
            FxLibrary.Flash(pc.AimPoint, new Color(1f, 0.9f, 0.6f), 1.6f);
            Sfx.Play(SoundId.Shockwave, pc.transform.position, 1f, 0.9f);
        }

        public override void FloorStart() { base.FloorStart(); Grace = 0f; }
        public override void Death() { Grace = 0f; }
        public override ClassFeatureHud Hud => Cooldown > 0f ? ReadyOrCooldown()
            : new ClassFeatureHud { state = Grace >= 1f ? ClassFeatureState.Stored : ClassFeatureState.Blocked, label = $"Łaska {Grace:0}/{Capacity:0}", fill = Capacity > 0 ? Grace / Capacity : 0f, color = color };
    }

    // ====================================================================== Paladyn B – Sąd przysięgi

    /// <summary>
    /// Odwraca rzeczywiście aktywne aury (Pobożność: do dwóch) i przenosi ich środek na oznaczonego wroga: premie znikają z paladyna
    /// (rezerwacja many zostaje), wróg dostaje odwrotne działanie każdej aury. Po końcu albo śmierci celu aury wracają bez
    /// ponownego kosztu włączenia; wyłączona w trakcie aura po prostu nie wraca, a włączona w trakcie działa normalnie.
    /// </summary>
    public sealed class JudgementFeature : ClassFeatureController
    {
        float active;
        EnemyBrain target;
        JudgementEffect effect;
        readonly List<AuraEffect> moved = new List<AuraEffect>();
        public EnemyBrain Target => active > 0f ? target : null;
        public JudgementEffect Effect => effect;
        protected override void OnBind() { color = new Color(1f, 0.7f, 0.35f); }

        protected override void OnPress()
        {
            if (active > 0f) { Fail("już działa", "Sąd przysięgi już trwa"); return; }
            var auras = pc.Auras.FindAll(a => a != null && a.Skill != null);
            if (auras.Count == 0) { Fail("brak aury", "Sąd przysięgi: włącz aurę"); return; }
            var e = pc.FeatureTarget(T.judgementRange);
            if (e == null) { Fail("brak celu", "Sąd przysięgi: brak celu w zasięgu"); return; }
            target = e;
            effect = new JudgementEffect { owner = pc, remaining = T.judgementSeconds };
            foreach (var a in auras)
            {
                string id = a.Skill.id;
                effect.auraIds.Add(id);
                switch (id)
                {
                    case "hu_aura_defiance": effect.armourCut = Mathf.Min(T.judgementDefianceArmour, e.Def.physicalResist + 0.25f); break;
                    case "hu_aura_purity": effect.resistCut = T.judgementPurityResist; break;
                    case "hu_aura_zeal": effect.speedMult = T.judgementZealSlow; break;
                    case "hu_aura_might": effect.damageMult = T.judgementMightWeaken; break;
                    case "hu_aura_renewal":
                    {
                        float regen = 0f;
                        foreach (var m in a.HuModifiers()) if (m.stat == StatType.HealthRegen && m.mode == ModifierMode.Flat) regen += m.value;
                        effect.renewalDps = Mathf.Max(1f, regen) * T.judgementRenewalPerRegen;
                        break;
                    }
                }
                // Środek aury na celu (Święty ogień pali wokół niego; pierścień aury pokazuje, gdzie działa).
                moved.Add(a);
                a.transform.SetParent(e.transform, false);
                a.transform.localPosition = Vector3.zero;
            }
            e.Judgement = effect;
            active = T.judgementSeconds;
            pc.AuraStrengthChanged();
            Telemetry.Add("Sąd: odwrócone aury", auras.Count);
            Used(T.judgementCooldown, "SĄD PRZYSIĘGI");
            FxLibrary.Lightning(pc.AimPoint + Vector3.up, e.AimPoint, new Color(1f, 0.8f, 0.4f));
            HellFx.Shockwave(e.transform.position, HellFxElement.Fire, new Color(1f, 0.75f, 0.35f), 2.5f, false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (active <= 0f) return;
            active -= dt;
            Telemetry.Add("Sąd: czas na celu", dt);
            if (active <= 0f || target == null || target.IsDead) Return();
        }

        void Return()
        {
            active = 0f;
            if (target != null && target.Judgement == effect) target.Judgement = null;
            foreach (var a in moved)
            {
                if (a == null || pc == null) continue;
                a.transform.SetParent(pc.transform, false);
                a.transform.localPosition = Vector3.zero;
            }
            moved.Clear();
            target = null; effect = null;
            if (pc != null && !pc.IsDead) pc.AuraStrengthChanged();
        }

        public override bool SuppressesAura(AuraEffect aura) => active > 0f && moved.Contains(aura);
        public override void FloorStart() { base.FloorStart(); Return(); }
        public override void Death() { Return(); }
        public override ClassFeatureHud Hud => active > 0f ? Timed(ClassFeatureState.Target, $"sąd: {target?.DisplayName} {active:0.0} s", active, T.judgementSeconds)
            : pc.Auras.Exists(a => a != null) ? ReadyOrCooldown() : new ClassFeatureHud { state = ClassFeatureState.Blocked, label = "włącz aurę", color = color * 0.6f };
    }

    // ====================================================================== Berserker A – Czerwona linia: Żelazna skóra

    /// <summary>
    /// Trans: dostępne życie ograniczone do linii (ułamek normalnego maksimum) – nadwyżka jest odcinana jako jawny koszt bez
    /// wyzwalaczy trafienia; niższe życie nie rośnie. Dynamiczna Żelazna skóra R = R_max × (1 − PŻ/normalne maks.) liczona z życia
    /// sprzed rozstrzygnięcia trafienia, jeden mnożnik w potoku (zamiast dawnych stałych 80%). Leczenie najwyżej do linii,
    /// bierne – o połowę (jak dawna Żelazna skóra). Po końcu linia znika, ale życie samo nie wraca.
    /// </summary>
    public sealed class RedLineFeature : ClassFeatureController
    {
        float active;
        public bool Active => active > 0f;
        public float Remaining => active;
        /// <summary>Krzywa Żelaznej skóry dla rzeczywistego życia względem normalnego maksimum (0–1).</summary>
        public static float Reduction(float lifeFraction, float rmax) => Mathf.Clamp01(rmax) * (1f - Mathf.Clamp01(lifeFraction));
        public float CurrentReduction => Active ? Reduction(pc.Health.Current / Mathf.Max(1f, pc.Health.Max), T.redlineMaxReduction) : 0f;
        public float Line => pc.Health.Max * T.redlineCap;
        protected override void OnBind() { color = new Color(1f, 0.2f, 0.12f); }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Czerwona linia już trwa"); return; }
            active = T.redlineSeconds;
            float cut = pc.Health.SetCeiling(Line);
            Telemetry.Add("czerwona linia: zarezerwowane PŻ", cut);
            // Ryk ze starej Żelaznej skóry: odpycha i zachwiewa, daje furię – raz.
            var h = pc.FeatureHit("Ryk czerwonej linii", T.redlineRoarDamage, 0f, Element.None, pc.transform.position);
            h.knockback = T.redlineRoarKnockback; h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Stagger) };
            h.featureEffect = true;
            var tr = new HitTracker();
            HitQuery.Sphere(pc.transform.position + Vector3.up, T.redlineRoarRadius, Faction.Player, tr, x => HitQuery.Apply(pc, x, h, false));
            pc.AddFury(T.redlineRoarFury);
            Used(T.redlineCooldown, "CZERWONA LINIA!");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Enemy, 0.6f, T.redlineSeconds);
            HellFx.Shockwave(pc.transform.position, HellFxElement.Enemy, new Color(1f, 0.15f, 0.05f), T.redlineRoarRadius, true);
            Sfx.Play(SoundId.Warcry, pc.AimPoint); Sfx.Play(SoundId.BossRoar, pc.AimPoint, 0.4f, 1.4f);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (!Active) return;
            active -= dt;
            // Zmiana maks. życia w trakcie: linia zawsze względem normalnego maksimum.
            if (active > 0f) pc.Health.SetCeiling(Line);
            else End();
            Telemetry.Max("czerwona linia: najwyższa redukcja (%)", CurrentReduction * 100f);
        }

        void End()
        {
            active = 0f;
            pc.Health.ClearCeiling(); // bez zwrotu odciętego życia
        }

        public override float IncomingMultiplier(bool statusTick) => Active ? 1f - CurrentReduction : 1f;
        public override float PassiveHealMultiplier => Active ? 0.5f : 1f;
        public override void FloorStart() { base.FloorStart(); End(); }
        public override void Death() { Telemetry.Add(Active ? "czerwona linia: zgony w transie" : "czerwona linia: zgony poza transem"); active = 0f; pc.Health.ClearCeiling(); }
        public override ClassFeatureHud Hud => Active ? Timed(ClassFeatureState.Active, $"trans {active:0.0} s · skóra −{CurrentReduction * 100f:0}%", active, T.redlineSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Berserker B – Spalenie furii

    /// <summary>
    /// G w szale kończy go i zamienia pozostałe pełne odcinki na Żetony Gniewu (limit). Żeton opłaca manę albo wytrzymałość
    /// następnego poprawnie rozpoczętego skilla ze slotu – po walidacji wymagań; zwrot przy anulowaniu przed wyzwoleniem.
    /// Żetony gasną i nie przechodzą przez piętra; kopie i wyzwalacze pieczęci ich nie zużywają (nie przechodzą przez rzut ręczny).
    /// </summary>
    public sealed class FurySpendFeature : ClassFeatureController
    {
        public int Tokens { get; private set; }
        float life;
        protected override void OnBind() { color = new Color(1f, 0.35f, 0.15f); }

        protected override void OnPress()
        {
            if (!pc.Raging) { Fail("brak szału", "Spalenie furii: tylko w szale"); return; }
            float left = pc.RageTime;
            int n = Mathf.Min(T.furyMaxTokens, Mathf.FloorToInt(left / Mathf.Max(0.1f, T.furySecondsPerToken) + 1e-4f));
            if (n <= 0) { Fail("za mało szału", $"Spalenie furii: zostało za mało szału (min. {T.furySecondsPerToken:0.#} s)"); return; }
            pc.EndRage();
            Tokens = Mathf.Min(T.furyMaxTokens, Tokens + n);
            life = T.furyTokenLife;
            Telemetry.Add("furia: oddane sekundy szału", left);
            Telemetry.Add("furia: żetony", n);
            Used(0f, $"SPALENIE FURII: {n} × żeton");
            HellFx.Shockwave(pc.transform.position, HellFxElement.Fire, new Color(1f, 0.3f, 0.1f), 1.8f, false);
        }

        public override bool TryPay(SpellRuntime skill, int slot, out CastReceipt receipt)
        {
            receipt = default;
            if (Tokens <= 0 || skill == null || (skill.manaCost <= 0f && skill.staminaCost <= 0f)) return false;
            Tokens--;
            receipt = new CastReceipt { token = true, active = true };
            Telemetry.Add("furia: opłacone skille");
            return true;
        }

        public override void Refund(in CastReceipt receipt) { if (receipt.token) { Tokens = Mathf.Min(T.furyMaxTokens, Tokens + 1); Telemetry.Add("furia: zwrócone żetony"); } }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (Tokens > 0) { life -= dt; if (life <= 0f) { Telemetry.Add("furia: zmarnowane żetony", Tokens); Tokens = 0; } }
        }

        public override void FloorStart() { base.FloorStart(); Tokens = 0; life = 0f; }
        public override void Death() { Tokens = 0; }
        public override ClassFeatureHud Hud => Tokens > 0 ? Timed(ClassFeatureState.Stored, $"żetony gniewu {Tokens} ({life:0} s)", life, T.furyTokenLife)
            : pc.Raging ? new ClassFeatureHud { state = ClassFeatureState.Ready, label = $"szał → {Mathf.Min(T.furyMaxTokens, Mathf.FloorToInt(pc.RageTime / T.furySecondsPerToken))} żet.", fill = 1f, color = color }
            : new ClassFeatureHud { state = ClassFeatureState.Blocked, label = "wymaga szału", fill = pc.Fury / 100f, color = color * 0.6f };
    }

    // ====================================================================== Rębajło A – Niezgodna para

    /// <summary>
    /// W oknie zwykły lekki atak to wspólne uderzenie dwiema broniami jednoręcznymi: dwa pakiety w ustalonej kolejności (główna,
    /// potem druga), każdy z własnym żywiołem, efektami i częścią obrażeń. Tempo – średnia obu broni. Perki zwykłego ataku i ogólne
    /// moce wyposażenia działają raz (tylko pakiet głównej ręki).
    /// </summary>
    public sealed class MismatchedPairFeature : ClassFeatureController
    {
        float active;
        public bool Active => active > 0f;
        protected override void OnBind() { color = new Color(0.9f, 0.4f, 0.4f); }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Niezgodna para już trwa"); return; }
            if (pc.Build == null || pc.Build.offWeapon == null) { Fail("brak drugiej broni", "Niezgodna para: potrzebne dwie bronie jednoręczne"); return; }
            active = T.pairSeconds;
            Used(T.pairCooldown, "NIEZGODNA PARA");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Physical, 0.45f, T.pairSeconds);
        }

        bool Ready => Active && pc.Build != null && pc.Build.offWeapon != null;
        public override bool AllowsOffhandAlternation => !Ready;

        public override float AttackTimeScale(ActionType type, AttackDefinition atk)
        {
            if (!Ready || type != ActionType.LightAttack || atk == null) return 1f;
            var off = pc.Build.offWeapon.definition.weapon.light;
            float main = atk.windup + atk.active + atk.recovery, second = off.windup + off.active + off.recovery;
            return main > 0.01f ? (main + second) * 0.5f / main : 1f;
        }

        public override bool OverrideSweepHit(IHitReceiver target, AttackDefinition atk)
        {
            if (!Ready || pc.Actions.Current != ActionType.LightAttack) return false;
            var b = pc.Build;
            var main = pc.PacketHit(b.mainHand, atk, T.pairMainShare, false, target);
            var off = pc.PacketHit(b.offWeapon, b.offWeapon.definition.weapon.light, T.pairOffShare, true, target);
            off.featureEffect = true; // drugi pakiet: bez powtórnych kradzieży życia i ogólnych mocy wyposażenia
            off.originId = main.originId;
            HitQuery.Apply(pc, target, main, true);
            if (!target.IsDead) HitQuery.Apply(pc, target, off, true);
            Telemetry.Add("para: wspólne uderzenia");
            return true;
        }

        public override void Tick(float dt) { base.Tick(dt); if (active > 0f) active -= dt; }
        public override void FloorStart() { base.FloorStart(); active = 0f; }
        public override void Death() { active = 0f; }
        public override ClassFeatureHud Hud => Active ? Timed(ClassFeatureState.Active, $"para {active:0.0} s", active, T.pairSeconds)
            : pc.Build != null && pc.Build.offWeapon == null ? new ClassFeatureHud { state = ClassFeatureState.Blocked, label = "brak drugiej broni", color = color * 0.6f } : ReadyOrCooldown();
    }

    // ====================================================================== Rębajło B – Taniec bez przerwy

    /// <summary>
    /// W oknie unik nie kasuje rozpoczętego ciosu: ta sama oś czasu (zamach → faza aktywna), ten sam rejestr trafionych i to samo
    /// pochodzenie; hitbox idzie z postacią, więc ruch może sprawić pudło. Drugi unik nie restartuje cięcia. Drgnięcie, przełamanie,
    /// zamrożenie i śmierć kasują przeniesiony cios.
    /// </summary>
    public sealed class UnbrokenDanceFeature : ClassFeatureController
    {
        float active;
        AttackDefinition carried;
        bool carriedOffhand;
        int carriedOrigin;
        float elapsed, windup, activeTime;
        bool swung;
        readonly HitTracker carriedTracker = new HitTracker();
        public bool Active => active > 0f;
        public bool Carrying => carried != null;
        public int CarriedHits => carriedTracker.Count;
        protected override void OnBind() { color = new Color(0.8f, 0.3f, 0.35f); }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Taniec bez przerwy już trwa"); return; }
            active = T.danceSeconds;
            Used(T.danceCooldown, "TANIEC BEZ PRZERWY");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Physical, 0.4f, T.danceSeconds);
        }

        public override void DodgeStarting(ActionType interrupted, ActionPhase phase)
        {
            if (!Active || carried != null) return; // drugi unik: cios już przeniesiony, bez restartu
            if (interrupted != ActionType.LightAttack && interrupted != ActionType.HeavyAttack) return;
            if (phase != ActionPhase.Startup && phase != ActionPhase.Active) return;
            var atk = pc.CurrentAttackDef;
            if (atk == null || atk.delivery == AttackDelivery.Projectile) return;
            carried = atk; carriedOffhand = pc.AttackIsOffhand; carriedOrigin = pc.ActionOrigin;
            elapsed = pc.Actions.Elapsed; windup = pc.AttackWindup; activeTime = pc.AttackActive;
            swung = elapsed >= windup;
            carriedTracker.CopyFrom(pc.AttackTracker);
            Telemetry.Add("taniec: przeniesione ciosy");
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (active > 0f) active -= dt;
            if (carried == null) return;
            elapsed += dt * pc.Status.ActionSpeedMultiplier(pc.Config.balance);
            if (elapsed >= windup && !swung) { swung = true; pc.PlayFeatureSwing(carried.heavy); }
            if (elapsed >= windup && elapsed <= windup + activeTime)
            {
                int before = carriedTracker.Count;
                pc.SweepCarried(carried, carriedTracker, carriedOffhand, carriedOrigin);
                if (carriedTracker.Count > before) Telemetry.Add("taniec: trafienia w uniku", carriedTracker.Count - before);
            }
            if (elapsed > windup + activeTime) { if (carriedTracker.Count == 0) Telemetry.Add("taniec: pudła"); carried = null; }
        }

        public override void ForcedInterrupt() { carried = null; }
        public override void FloorStart() { base.FloorStart(); active = 0f; carried = null; }
        public override void Death() { active = 0f; carried = null; }
        public override ClassFeatureHud Hud => Active ? Timed(carried != null ? ClassFeatureState.SecondPhase : ClassFeatureState.Active,
            carried != null ? "cios w uniku" : $"taniec {active:0.0} s", active, T.danceSeconds) : ReadyOrCooldown();
    }
}
