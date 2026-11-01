using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Liczniki mocy unikatów (rozdz. 27.3): próby, sukcesy, odmowy z przyczyną, przeniesiony/wydany/zmarnowany budżet.
    /// Klucze w postaci „KOD: zdarzenie”.
    /// </summary>
    public class UniqueTelemetry
    {
        public readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public readonly Dictionary<string, int> rejections = new Dictionary<string, int>();
        public void Add(string key, float v = 1f) { values.TryGetValue(key, out var x); values[key] = x + v; }
        public float Get(string key) => values.TryGetValue(key, out var x) ? x : 0f;
        public void Reject(string key) { rejections.TryGetValue(key, out var n); rejections[key] = n + 1; }
        public int Rejected(string key) => rejections.TryGetValue(key, out var n) ? n : 0;
        public void Clear() { values.Clear(); rejections.Clear(); }
    }

    /// <summary>
    /// Moc jednego natywnego unikatu u jednego właściciela: cały stan runtime (magazyny, znaki, ładunki) – nigdy w definicji.
    /// <see cref="PlayerCombat"/> woła haki w opisanych miejscach potoku; domyślnie wszystkie są neutralne. Moc działa tylko,
    /// gdy <see cref="Dormant"/> zwraca null (wymagana cecha należy do klasy właściciela i jest wybrana, warunki wyposażenia).
    ///
    /// Odwzorowanie potoku z rozdz. 6.1: walidacja wyposażenia i cechy (<see cref="Dormant"/>) → zamiana typu akcji
    /// (<see cref="OverrideRiposte"/>, <see cref="AttackOverride"/>) → geometria/dostarczenie (<see cref="ModifyHuCast"/>,
    /// <see cref="OverrideHuCast"/>) → magazyn/transport (stan mocy) → rozliczenie przez zwykły potok trafienia (HitQuery.Apply),
    /// z trafieniami wtórnymi oznaczonymi <c>uniqueEffect</c> (bez kradzieży życia, zasobów cech i kolejnych wyzwalaczy mocy).
    /// </summary>
    public abstract class UniquePower
    {
        public UniqueCard Card { get; private set; }
        public ItemInstance Item { get; private set; }
        protected PlayerCombat pc;
        protected RunState Run => pc.Run;
        protected UniqueTelemetry Telemetry => pc.UniqueTelemetry;
        protected float P(string key, float fallback = 0f) => Card.P(key, fallback);
        protected int PI(string key, int fallback = 0) => Card.PI(key, fallback);
        protected Color color = new Color(0.95f, 0.55f, 0.25f);

        public void Bind(PlayerCombat owner, UniqueCard card, ItemInstance item) { pc = owner; Card = card; Item = item; OnBind(); }
        internal void Rebind(ItemInstance item) { Item = item; }
        protected virtual void OnBind() { }

        /// <summary>Wybrana cecha właściciela należy do klasy postaci i jest tą, której wymaga karta.</summary>
        protected bool HasFeature(string featureId) =>
            Run != null && Run.classFeatureId == featureId && ClassFeatureCatalog.Owns(Run.startingClass, featureId) && pc.Feature != null && pc.Feature.Def.id == featureId;

        /// <summary>Kontroler wymaganej cechy (null – brak albo inna cecha).</summary>
        protected T Feature<T>() where T : ClassFeatureController => pc.Feature as T;

        /// <summary>null – moc działa; inaczej przyczyna uśpienia po polsku (tooltip, HUD, telemetria).</summary>
        public virtual string Dormant()
        {
            if (Card.requiredFeature != null && !HasFeature(Card.requiredFeature))
            {
                var f = ClassFeatureCatalog.Get(Card.requiredFeature);
                return $"uśpiona: wymaga cechy {(f != null ? f.name : Card.requiredFeature)}";
            }
            return EquipmentDormant();
        }

        /// <summary>Warunki wyposażenia/skilli karty (null – spełnione).</summary>
        protected virtual string EquipmentDormant() => null;

        public bool Active => pc != null && !pc.IsDead && Dormant() == null;

        /// <summary>Skill o danym id w jednym ze zwykłych slotów (null – brak).</summary>
        protected SpellRuntime SlotSkill(params string[] ids)
        {
            if (pc.Build == null) return null;
            foreach (var s in pc.Build.skills) if (s != null && ids.Contains(s.Def.id)) return s;
            return null;
        }

        protected string NeedSkill(params string[] ids) => SlotSkill(ids) != null ? null : "brak skilla: " + string.Join(" / ", ids.Select(SkillName));

        protected string SkillName(string id)
        {
            var map = pc.Config != null ? RunSnapshot.Index(pc.Config) : null;
            return map != null && map.TryGetValue(id, out var d) ? d.displayName : id;
        }

        protected bool IsSkill(SpellRuntime sk, params string[] ids) => sk != null && ids.Contains(sk.Def.id);

        /// <summary>Komunikat odmowy (bez pobrania zasobu) z wpisem w telemetrii.</summary>
        protected void Fail(string reason, string message)
        {
            Telemetry.Reject(Card.code + ": " + reason);
            if (message != null) CombatEvents.RaiseMessage(message, new Color(0.75f, 0.75f, 0.75f));
        }

        protected void Count(string what, float v = 1f) => Telemetry.Add(Card.code + ": " + what, v);

        /// <summary>Trafienie wtórne mocy: zwykły potok wyjścia, bez kradzieży życia, zasobów cech i ponownych wyzwalaczy mocy.</summary>
        protected HitData PowerHit(string name, float physical, float magic, Element element, Vector3 source)
        {
            var h = pc.FeatureHit(name, physical, magic, element, source);
            h.uniqueEffect = true;
            return h;
        }

        protected HitResult Apply(IHitReceiver target, HitData hit, bool melee = false) => pc.FeatureApply(target, hit, melee);

        // ================================================================== Cykl życia
        public virtual void Tick(float dt) { }
        public virtual void FloorStart() { Death(); }
        /// <summary>Śmierć właściciela: usuń byty bez nowych ataków.</summary>
        public virtual void Death() { }
        /// <summary>Zdjęcie przedmiotu: kontrolery się zamykają; opłacony budżet w locie kończy się swoją ścieżką.</summary>
        public virtual void Dispose() { Death(); }
        /// <summary>Stan magazynu do HUD (null – nie pokazuj).</summary>
        public virtual string Hud => null;

        // ================================================================== Haki potoku (neutralne)

        /// <summary>Zwykły atak (lekki/ciężki) – true: moc wykonała własną akcję zamiast ataku.</summary>
        public virtual bool InterceptAttack(ActionType type) => false;
        /// <summary>Podmiana definicji ataku (np. lekki atak różdżki → cięcie). Zwraca atak do użycia.</summary>
        public virtual AttackDefinition AttackOverride(ActionType type, AttackDefinition atk) => atk;
        public virtual void AttackStarted(ActionType type, AttackDefinition atk, int origin) { }
        public virtual void AttackActive(ActionType type, AttackDefinition atk, int origin) { }
        /// <summary>Ładowany ciężki cios trwa (sekundy ładowania).</summary>
        public virtual void ChargeTick(float chargeSeconds, float dt) { }
        /// <summary>Wszystkie trafienia gracza (przed premiami żywiołu).</summary>
        public virtual void Outgoing(ref HitData hit) { }
        /// <summary>Koniec potoku wyjścia – po ustawieniu modyfikatorów efektów (podpalenie, pęknięcie).</summary>
        public virtual void OutgoingLate(ref HitData hit) { }
        /// <summary>Trafienie w konkretnego wroga przed obroną.</summary>
        public virtual void OutgoingAgainst(ref HitData hit, EnemyBrain target) { }
        public virtual void HitResolved(IHitReceiver target, in HitData hit, in HitResult result) { }
        /// <summary>Koszt ręcznego bloku w wytrzymałości – zwraca część, która zostaje w wytrzymałości.</summary>
        public virtual float BlockStaminaCost(float cost, in HitData hit) => cost;
        /// <summary>Udana ręczna obrona (blok/parowanie) prawdziwego ciosu wroga.</summary>
        public virtual void Defended(HitOutcome outcome, in HitData hit) { }
        /// <summary>Riposta: true – moc wykonała własną wersję.</summary>
        public virtual bool OverrideRiposte(EnemyBrain target, float damage) => false;
        /// <summary>Otrzymany bezpośredni cios po obronie (r.healthDamage po redukcjach, multiplier – mnożnik cechy).</summary>
        public virtual void Struck(in HitData hit, in HitResult result, float featureMultiplier) { }
        public virtual void DodgeStarted(Vector3 from, Vector3 dir) { }
        /// <summary>Skill HU z ręki: zmiana źródła/kierunku/celu przed utworzeniem rzutu.</summary>
        public virtual void ModifyHuCast(SpellRuntime sk, ref Vector3 origin, ref Vector3 aim, ref Vector3 target) { }
        /// <summary>Skill HU z ręki: true – moc przejęła wykonanie (koszt już pobrany).</summary>
        public virtual bool OverrideHuCast(HuCast cast, Vector3 aim, float range) => false;
        /// <summary>Ręczny rzut skilla wszedł w fazę aktywną (po wykonaniu).</summary>
        public virtual void CastFired(SpellRuntime sk, int origin) { }
        public virtual void BasicShot(ref HitData hit, int index, int count) { }
        /// <summary>Podstawowy pocisk przed wylotem: true – moc go przejęła.</summary>
        public virtual bool CaptureShot(in HitData hit, Vector3 origin, Vector3 dir, float speed, Color color, int index, int count) => false;
        /// <summary>Ręczne leczenie: czar leczenia albo flaszka życia.</summary>
        public virtual void ManualHeal(bool flask) { }
        /// <summary>Bonus z pleców w trafieniu (dodatek, który dołożyła Zasadzka) – moc może go zatrzymać.</summary>
        public virtual float BackstabBonus(float bonus, in HitData hit, EnemyBrain target) => bonus;
        /// <summary>Skill HU z ręki: podmiana rzutu (np. wzmocnienie runami) – kolejne moce widzą nową wersję. null – bez zmian.</summary>
        public virtual HuCast ReplaceHuCast(HuCast cast) => null;
        public virtual bool SuppressKnockback(HuCast cast) => false;
        /// <summary>Łańcuch: ile skoków przez węzły może zużyć ten rzut.</summary>
        public virtual int ChainNodes(HuCast cast) => 0;
        public virtual Vector3? ChainNode(Vector3 from, float jump, HashSet<object> used) => null;
        public virtual bool BlocksManaRegen => false;
        public virtual float SunderMagicPerStack => 0f;
        /// <summary>Zysk reakcji – ile moc odkłada (odejmowane od ofiary).</summary>
        public virtual float ReactionStore(float gain, Element element) => 0f;
        /// <summary>Bariera pochłonęła część ciosu (przed/po).</summary>
        public virtual void BarrierAbsorbed(float before, float after, in HitData hit) { }
        /// <summary>Przeciągnięcie hitboxa zwykłego ataku: true – moc rozstrzyga trafienie (np. linia do pieczęci).</summary>
        public virtual bool OverrideSweepHit(IHitReceiver target, AttackDefinition atk) => false;
        /// <summary>
        /// Punkt przemiany w cesze klasowej (np. „absolution.release”, „fury.tokens”): moc może zmienić ilość albo przejąć zdarzenie
        /// (true – cecha nie wykonuje zwykłego efektu). Wołane tylko dla mocy aktywnych.
        /// </summary>
        public virtual bool FeatureEvent(string evt, ref float amount, object data) => false;
        /// <summary>Impuls aury HU (true – moc go przejęła, np. magazyn Aury świętego ognia).</summary>
        public virtual bool AuraPulse(AuraEffect aura, in HitData hit) => false;
        /// <summary>Mnożnik premii własnych aur dla właściciela (Habit: słabsza część).</summary>
        public virtual float AuraOwnerScale => 1f;
        /// <summary>Premie dla sług: mnożnik szybkości ataku i obrażeń (1 – bez zmian).</summary>
        public virtual float ServantSpeedMult => 1f;
        public virtual float ServantDamageMult => 1f;
        public virtual bool CommandsServants => false;
        public virtual bool SuppressesRageSpeed => false;
        /// <summary>Lewa broń jest poza ręką (orbita).</summary>
        public virtual bool HoldsOffhand => false;
        /// <summary>Główna broń działa osobno (Amulet zbłąkanej klingi): lekkie ciosy zadaje sama lewa broń, bez pięści.</summary>
        public virtual bool OffhandFightsAlone => false;
        /// <summary>Wymuszone przerwanie akcji (drgnięcie, przełamanie, zamrożenie).</summary>
        public virtual void ForcedInterrupt() { }
    }

    /// <summary>
    /// Zestaw mocy właściciela: jedna moc na definicję (dwa takie same pierścienie – jedna legenda, profil z pierwszego slotu).
    /// Odbudowywany przy zmianie wyposażenia; moce zdjętych przedmiotów są zamykane (<see cref="UniquePower.Dispose"/>).
    /// </summary>
    public class UniquePowerSet
    {
        readonly List<UniquePower> powers = new List<UniquePower>();
        public IReadOnlyList<UniquePower> All => powers;

        public T Get<T>() where T : UniquePower => powers.OfType<T>().FirstOrDefault();
        public UniquePower Get(string cardId) => powers.FirstOrDefault(p => p.Card.id == cardId);

        public void Sync(PlayerCombat pc, EquipmentSet equipment)
        {
            var wanted = new Dictionary<string, ItemInstance>();
            foreach (EquipSlot slot in System.Enum.GetValues(typeof(EquipSlot)))
            {
                var it = equipment.Get(slot);
                if (it == null || !it.definition.IsNativeUnique || wanted.ContainsKey(it.definition.uniquePowerId)) continue;
                wanted[it.definition.uniquePowerId] = it;
            }
            bool changed = false;
            for (int i = powers.Count - 1; i >= 0; i--)
            {
                if (wanted.TryGetValue(powers[i].Card.id, out var item)) { powers[i].Rebind(item); continue; }
                powers[i].Dispose();
                powers.RemoveAt(i);
                changed = true;
            }
            foreach (var kv in wanted)
            {
                if (powers.Any(p => p.Card.id == kv.Key)) continue;
                var card = UniqueCatalog.Get(kv.Key);
                var p = UniquePowerFactory.Create(card);
                if (p == null) continue;
                p.Bind(pc, card, kv.Value);
                powers.Add(p);
                changed = true;
            }
            // Deterministyczna kolejność haków: kod karty, nie kolejność zakładania. Tylko przy zmianie – samo odświeżenie buildu
            // (np. początek szału w trakcie Tick mocy) nie może unieważnić trwającego przeglądu listy.
            if (changed) powers.Sort((a, b) => string.CompareOrdinal(a.Card.code, b.Card.code));
        }

        public void Clear() { foreach (var p in powers) p.Dispose(); powers.Clear(); }

        public IEnumerable<UniquePower> Active { get { foreach (var p in powers) if (p.Active) yield return p; } }
    }
}
