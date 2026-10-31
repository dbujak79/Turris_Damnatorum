using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Stan cechy do HUD (rozdz. 6): gotowość, przygotowanie, działanie, zasób, cel, druga faza, odnowienie, blokada.</summary>
    public enum ClassFeatureState { Ready, Preparing, Active, Stored, Target, SecondPhase, Cooldown, Blocked }

    /// <summary>Migawka cechy dla HUD i botów.</summary>
    public struct ClassFeatureHud
    {
        public ClassFeatureState state;
        /// <summary>Krótka etykieta stanu („gotowa”, „trans 4,2 s”, „Łaska 38/80”…).</summary>
        public string label;
        /// <summary>Wypełnienie paska 0–1 (czas działania, zasób albo odnowienie).</summary>
        public float fill;
        public Color color;
    }

    /// <summary>
    /// Liczniki cechy w podejściu (rozdz. 15.2): próby, użycia, odrzucenia z przyczyną, koszty i zasoby. Wspólne dla gracza i botów.
    /// </summary>
    public class ClassFeatureTelemetry
    {
        public readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public readonly Dictionary<string, int> rejections = new Dictionary<string, int>();
        public int presses, uses;

        public void Add(string key, float v = 1f) { values.TryGetValue(key, out var x); values[key] = x + v; }
        public void Max(string key, float v) { values.TryGetValue(key, out var x); values[key] = Mathf.Max(x, v); }
        public float Get(string key) => values.TryGetValue(key, out var x) ? x : 0f;
        public void Reject(string reason) { rejections.TryGetValue(reason, out var n); rejections[reason] = n + 1; }
        public int Rejected(string reason) => rejections.TryGetValue(reason, out var n) ? n : 0;
        public void Clear() { values.Clear(); rejections.Clear(); presses = 0; uses = 0; }
    }

    /// <summary>
    /// Kontroler wybranej cechy klasowej jednej postaci: cały stan runtime (liczniki, cele, magazyny) – nigdy w definicji.
    /// <see cref="PlayerCombat"/> woła hooki w jednym, opisanym miejscu potoku walki; domyślnie wszystkie są neutralne,
    /// więc klasa bez danego hooka nie zmienia wyniku. Przycisk klasy (G / View) trafia do <see cref="Press"/>.
    /// </summary>
    public abstract class ClassFeatureController
    {
        public ClassFeatureDefinition Def { get; private set; }
        protected PlayerCombat pc;
        protected ClassFeatureTuning T => pc.FeatureTuning;
        protected RunState Run => pc.Run;
        public ClassFeatureTelemetry Telemetry => pc.FeatureTelemetry;

        /// <summary>Pozostałe odnowienie (s) i jego pełna długość.</summary>
        public float Cooldown { get; protected set; }
        protected float cooldownLength = 1f;
        protected Color color = new Color(1f, 0.8f, 0.4f);

        public void Bind(PlayerCombat owner, ClassFeatureDefinition def) { pc = owner; Def = def; OnBind(); }
        protected virtual void OnBind() { }

        /// <summary>Druga faza aktywacji czeka na naciśnięcie – odnowienie jej nie blokuje.</summary>
        protected virtual bool AwaitingSecondPress => false;

        /// <summary>Przycisk klasy. Walidacja przed kosztem: odmowa nie pobiera zasobu i nie rusza odnowienia.</summary>
        public void Press()
        {
            Telemetry.presses++;
            if (pc.IsDead) return;
            if (Cooldown > 0f && !AwaitingSecondPress) { Fail("odnowienie", $"{Def.name}: odnowienie {Cooldown:0.0} s"); return; }
            OnPress();
        }

        protected abstract void OnPress();

        /// <summary>Odmowa z komunikatem po polsku (bez kosztu i odnowienia) – zapisana w telemetrii.</summary>
        protected bool Fail(string reason, string message)
        {
            Telemetry.Reject(reason);
            CombatEvents.RaiseMessage(message, new Color(0.75f, 0.75f, 0.75f));
            return false;
        }

        /// <summary>Udane użycie: telemetria, odnowienie (gdy podane) i komunikat.</summary>
        protected void Used(float cooldown, string message = null)
        {
            Telemetry.uses++;
            if (cooldown > 0f) { Cooldown = cooldown; cooldownLength = cooldown; }
            if (message != null) CombatEvents.RaiseMessage(message, color);
        }

        public virtual void Tick(float dt) { if (Cooldown > 0f) Cooldown = Mathf.Max(0f, Cooldown - dt); }

        /// <summary>Nowe piętro: zgodnie z kartą (zwykły reset odnowień piętra nie daje dodatkowego wariantu).</summary>
        public virtual void FloorStart() { Cooldown = 0f; }

        /// <summary>Śmierć: usuń byty i stan bez nowej ofensywy.</summary>
        public virtual void Death() { }

        /// <summary>Kontroler przestaje istnieć (nowe podejście, inna postać).</summary>
        public virtual void Dispose() { Death(); }

        public abstract ClassFeatureHud Hud { get; }

        protected ClassFeatureHud ReadyOrCooldown(string readyLabel = "gotowa")
        {
            if (Cooldown > 0f) return new ClassFeatureHud { state = ClassFeatureState.Cooldown, label = $"{Cooldown:0.0} s", fill = 1f - Cooldown / Mathf.Max(0.01f, cooldownLength), color = color * 0.6f };
            return new ClassFeatureHud { state = ClassFeatureState.Ready, label = readyLabel, fill = 1f, color = color };
        }

        protected ClassFeatureHud Timed(ClassFeatureState state, string label, float remaining, float total) =>
            new ClassFeatureHud { state = state, label = label, fill = Mathf.Clamp01(remaining / Mathf.Max(0.01f, total)), color = color };

        // ================================================================== Hooki potoku walki (neutralne)

        /// <summary>Przycisk slotu 0–2: true – cecha go przejęła (np. wybór czaru do strzały), zwykły rzut się nie odbywa.</summary>
        public virtual bool InterceptSkillRequest(int slot) => false;
        /// <summary>Zwykły atak (lekki/ciężki): true – cecha wykonała własny atak (forma Wcielenia).</summary>
        public virtual bool InterceptAttack(ActionType type) => false;
        /// <summary>Blok/parowanie niedostępne (forma).</summary>
        public virtual bool BlocksGuard => false;
        /// <summary>Walidacja rzutu skilla ze slotu (przed kosztem). Komunikat ≠ null – odmowa.</summary>
        public virtual string BlockCast(SpellRuntime skill, int slot) => null;
        /// <summary>
        /// Płatność rzutu: cecha może przejąć koszt many/wytrzymałości (żeton) albo pokryć brak many długiem.
        /// Zwraca true, gdy płatność rozliczyła cecha (wtedy <paramref name="receipt"/> opisuje zwrot przy anulowaniu).
        /// </summary>
        public virtual bool TryPay(SpellRuntime skill, int slot, out CastReceipt receipt) { receipt = default; return false; }
        /// <summary>Anulowanie rzutu przed wyzwoleniem: zwrot tego, co cecha pobrała.</summary>
        public virtual void Refund(in CastReceipt receipt) { }
        /// <summary>Czar ze slotu ma iść osobnym torem (Dwa takty).</summary>
        public virtual bool RoutesCastToLane(SpellRuntime skill, int slot) => false;

        /// <summary>Mnożnik obrażeń po ogólnej redukcji i minimum ciosu (Czerwona linia). Życie sprzed rozstrzygnięcia trafienia.</summary>
        public virtual float IncomingMultiplier(bool statusTick) => 1f;
        /// <summary>Po pełnej mitygacji i rodzimej osłonie, przed barierą (Płaszcz many). Zwraca to, co idzie dalej.</summary>
        public virtual float AbsorbIncoming(float damage, bool statusTick) => damage;
        /// <summary>Nadleczenie (po limitach źródła) z jego źródłem.</summary>
        public virtual void OnOverheal(float amount, HealSource source) { }
        /// <summary>Garda w trakcie własnego ataku (Walka w gardzie).</summary>
        public virtual bool GuardsDuringAttack => false;
        /// <summary>Regeneracja wytrzymałości zatrzymana (Walka w gardzie).</summary>
        public virtual bool SuppressStaminaRegen => false;
        /// <summary>Mnożnik biernego leczenia (Czerwona linia zachowuje osłabienie dawnej Żelaznej skóry).</summary>
        public virtual float PassiveHealMultiplier => 1f;
        /// <summary>Premie aury wyłączone, bo aura działa odwrócona na wrogu (Sąd przysięgi).</summary>
        public virtual bool SuppressesAura(AuraEffect aura) => false;

        /// <summary>Wszystkie trafienia gracza (PrepareOutgoing, przed premiami żywiołu): rodziny podpaleń, Żar.</summary>
        public virtual void Outgoing(ref HitData hit) { }
        /// <summary>Koniec potoku wyjścia – po ustawieniu modyfikatorów efektów (rodziny podpaleń).</summary>
        public virtual void OutgoingLate(ref HitData hit) { }
        /// <summary>Trafienie gracza w konkretnego wroga, przed obroną (Strzała odpowiedzi, Stanowisko, Tętnica).</summary>
        public virtual void OutgoingAgainst(ref HitData hit, EnemyBrain target) { }
        /// <summary>Po rozstrzygnięciu trafienia gracza (groty, pęknięcie ściany, strzała zaklęcia).</summary>
        public virtual void HitResolved(IHitReceiver target, in HitData hit, in HitResult result) { }

        /// <summary>Start zwykłego ataku bronią (lekki/ciężki) z id akcji.</summary>
        public virtual void AttackStarted(ActionType type, AttackDefinition atk, int origin) { }
        /// <summary>Faza aktywna zwykłego ataku (wypuszczenie płyty przy mocnym ataku).</summary>
        public virtual void AttackActive(ActionType type, AttackDefinition atk, int origin) { }
        /// <summary>Ręczny rzut skilla ze slotu wszedł w fazę aktywną (Żar, rodzina podpalenia).</summary>
        public virtual void CastFired(SpellRuntime skill, int origin) { }
        /// <summary>Przeciągnięcie hitboxa ciosu: true – cecha rozstrzygnęła trafienie (Niezgodna para).</summary>
        public virtual bool OverrideSweepHit(IHitReceiver target, AttackDefinition atk) => false;
        /// <summary>Tempo zwykłego ataku (mnożnik czasu) – Niezgodna para liczy obie bronie.</summary>
        public virtual float AttackTimeScale(ActionType type, AttackDefinition atk) => 1f;
        /// <summary>Lekki atak drugą ręką na zmianę – cecha może to wyłączyć.</summary>
        public virtual bool AllowsOffhandAlternation => true;
        /// <summary>Unik startuje i przerwie bieżącą akcję – cecha może przenieść atak (Taniec bez przerwy).</summary>
        public virtual void DodgeStarting(ActionType interrupted, ActionPhase phase) { }
        /// <summary>Wymuszone przerwanie (drgnięcie, przełamanie gardy, zamrożenie, śmierć).</summary>
        public virtual void ForcedInterrupt() { }

        /// <summary>Podstawowy strzał: punkt wylotu i kierunek (Stanowisko strzeleckie).</summary>
        public virtual void ShotOrigin(ref Vector3 origin, ref Vector3 aim, ref IHitReceiver homing) { }
        /// <summary>Podstawowy pocisk tuż przed wylotem: true – trafił do magazynu (Oko ciszy) zamiast w świat.</summary>
        public virtual bool CaptureOwnShot(in HitData hit, Vector3 dir, float speed, Color color) => false;
        /// <summary>Podstawowy strzał – ładunki (Strzała zaklęcia, odpowiedzi, groty) i ich podział w salwie.</summary>
        public virtual void BasicShot(ref HitData hit, int index, int count) { }

        /// <summary>Furia może nie otwierać szału (nie dotyczy kart bez zmiany).</summary>
        public virtual void RageEnded() { }
    }

    /// <summary>Źródło leczenia – współczynniki Łaski (Rozgrzeszenie).</summary>
    public enum HealSource { Other, Regen, Leech, Kill, Flask, Skill }

    /// <summary>Co cecha pobrała przy rzucie (zwrot przy anulowaniu przed wyzwoleniem – dokładnie to, nic więcej).</summary>
    public struct CastReceipt
    {
        public bool token;
        public float mana, debt;
        public bool active;
    }
}
