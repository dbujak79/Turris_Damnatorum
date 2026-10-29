using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Parametry balansu edytowalne w inspektorze (bez zmian w kodzie).</summary>
    [Serializable]
    public class BalanceConfig
    {
        [Header("Cechy → statystyki")]
        public float baseHealth = 200f;
        [UnityEngine.Serialization.FormerlySerializedAs("healthPerVigor")] public float healthPerToughness = 15f;
        public float baseStamina = 60f;
        [UnityEngine.Serialization.FormerlySerializedAs("staminaPerEndurance")] public float staminaPerToughness = 3f;
        public float baseMana = 20f;
        [UnityEngine.Serialization.FormerlySerializedAs("manaPerMind")] public float manaPerIntelligence = 6f;
        public float baseStaminaRegen = 38f;
        public float baseEquipLoad = 30f;
        [UnityEngine.Serialization.FormerlySerializedAs("equipLoadPerEndurance")] public float equipLoadPerToughness = 1f;
        [Tooltip("Zręczność: uchylenie (%/pkt) liczone od tej wartości w górę.")]
        public float evasionFromDexterity = 12f;
        public float evasionPerDexterity = 1.5f;
        [Tooltip("Limit szansy na uchylenie (%).")]
        public float maxEvasion = 25f;
        [Tooltip("Zręczność ponad 10: dłuższy unik (ułamek za punkt) i szybszy atak – z limitami.")]
        public float dodgeDistancePerDexterity = 0.02f, maxDodgeDistanceBonus = 0.25f;
        public float attackSpeedPerDexterity = 0.01f, maxAttackSpeedBonus = 0.2f;
        [Tooltip("Broń bez wymaganych cech atakuje wolniej (mnożnik szybkości).")]
        public float unmetRequirementAttackSpeed = 0.8f;
        [Tooltip("Czar bez wymaganej Inteligencji nie może zostać rzucony.")]
        public bool spellRequirementsAreHard = true;
        [Tooltip("Darmowe punkty cech po każdym ukończonym piętrze (Turris – cech nie kupuje się za dusze).")]
        public int attributePointsPerFloor = 1;
        [Tooltip("Obrona: redukcja = obrona / (obrona + stała).")]
        public float defenseConstant = 100f;

        [Header("Wytrzymałość")]
        public float staminaRegenDelay = 0.6f;
        public float sprintStaminaPerSecond = 12f;

        [Header("Wymagania")]
        [Tooltip("Skuteczność broni/czaru, gdy nie spełniono wymagań atrybutów (brak twardej blokady).")]
        [Range(0, 1)] public float unmetRequirementEffectiveness = 0.6f;

        [Header("Przełamanie gardy")]
        [Tooltip("Reguła: trafienie przełamujące gardę zadaje max(obrażenia po bloku, obrażenia bez bloku × ten współczynnik).")]
        [Range(0, 1)] public float guardBreakDamageFactor = 0.5f;
        public float guardBreakStun = 1.1f;

        [Header("Reakcje")]
        public float flinchDuration = 0.35f;
        public float inputBuffer = 0.3f;

        [Header("Efekty i żywioły")]
        [Tooltip("Krwawienie: łączne obrażenia warstwy = ułamek wylądowanych obrażeń fizycznych.")]
        public float bleedShare = 0.35f;
        public float bleedDuration = 3f;
        public int bleedMaxStacks = 5;
        [Tooltip("Mnożnik krwawienia, gdy cel się porusza.")]
        public float bleedMovingMultiplier = 2f;
        [Tooltip("Podpalenie: łączne obrażenia = ułamek wylądowanych obrażeń ognia.")]
        public float burnShare = 0.8f;
        public float burnDuration = 2.5f;
        public float chillDuration = 3f;
        [Tooltip("Spowolnienie ruchu i akcji za warstwę chłodu.")]
        public float chillSlowPerStack = 0.15f;
        [Tooltip("Tyle warstw chłodu zamraża cel.")]
        public int chillStacksToFreeze = 3;
        public float freezeDuration = 1.2f;
        [Tooltip("Po zamrożeniu cel jest przez chwilę odporny na kolejne.")]
        public float freezeImmunity = 2.5f;
        public float shockDuration = 4f;
        [Tooltip("Porażony cel otrzymuje tyle więcej obrażeń.")]
        public float shockDamageBonus = 0.2f;
        [Tooltip("Szok termiczny: ogień w wychłodzony cel – premia do części ognia; chłód znika.")]
        public float thermalShockBonus = 0.5f;
        [Tooltip("Przewodzenie: błyskawica w krwawiący cel – pozostałe obrażenia krwawienia od razu (× ten mnożnik).")]
        public float conductionMultiplier = 1f;
        public float statusTickInterval = 0.5f;
        [Tooltip("Roztrzaskanie: błyskawica w zamrożony cel – wybuch wokół celu za tę część obrażeń trafienia.")]
        public float shatterShare = 0.8f;
        public float shatterRadius = 3f;
        [Tooltip("Maksymalna odporność bohatera na żywioł (HU balans 2.0: 65%).")]
        public float maxElementResist = 65f;
        [Tooltip("Odporność bohatera powyżej progu liczy się w tej części (HU: pełna do 40%, dalej połowa – 60% surowej to 50%).")]
        public float resistSoftThreshold = 40f, resistSoftSlope = 0.5f;
        [Tooltip("Redukcja obrażeń bohatera ze wszystkich źródeł (klasa, przedmioty, okrzyk, szał, więź): pełna do progu, dalej w tej części, nie więcej niż limit (HU: 25% / 0,5 / 45%).")]
        public float damageReductionSoftThreshold = 25f, damageReductionSoftSlope = 0.5f, damageReductionCap = 45f;
        [Tooltip("Obrona fizyczna bohatera zdejmuje najwyżej tyle ciosu (HU: pancerz zostawia min. 45%).")]
        [Range(0, 1)] public float maxPhysicalReduction = 0.55f;
        [Tooltip("Bezpośredni cios w bohatera zadaje co najmniej tę część surowych obrażeń (unik, parowanie, blok i osłona dalej zerują).")]
        [Range(0, 1)] public float directHitMinimumDamage = 0.15f;
        [Tooltip("Kradzież życia i życie za zabójstwo razem leczą najwyżej tę część maks. życia w ruchomej sekundzie (HU: 8%).")]
        public float passiveHealingPerSecond = 0.08f;

        /// <summary>Wartość do progu bez zmian, ponad nim w części <paramref name="slope"/>, nigdy ponad <paramref name="cap"/> (HU <c>BalanceConfig.SoftCap</c>).</summary>
        public static float SoftCap(float value, float threshold, float slope, float cap) =>
            Mathf.Min(cap, value <= threshold ? value : threshold + (value - threshold) * slope);

        /// <summary>Skuteczna odporność bohatera na żywioł (%) z surowej: miękki próg i limit; ujemna (słabość) bez zmian, do −100%.</summary>
        public float HeroResist(float raw) => raw <= 0f ? Mathf.Max(-100f, raw) : SoftCap(raw, resistSoftThreshold, resistSoftSlope, maxElementResist);

        /// <summary>Skuteczna redukcja obrażeń bohatera (0–1) z sumy wszystkich źródeł (ułamek).</summary>
        public float HeroDamageReduction(float raw) =>
            SoftCap(Mathf.Max(0f, raw) * 100f, damageReductionSoftThreshold, damageReductionSoftSlope, damageReductionCap) / 100f;
        [Tooltip("Czas kontroli (zamrożenie, spowolnienie) na bossach i elitach.")]
        public float bossControlMultiplier = 0.5f, eliteControlMultiplier = 0.7f;
        [Tooltip("Zachwianie (ziemia, młoty): tyle warstw ogłusza; warstwa trwa staggerDuration, każda spowalnia ruch.")]
        public int staggerStacksToStun = 3;
        public float staggerDuration = 2.5f;
        public float staggerSlow = 0.15f;
        public float stunDuration = 0.8f;
        [Tooltip("Po ogłuszeniu cel jest przez chwilę odporny na kolejne.")]
        public float stunImmunity = 2f;
        [Tooltip("Pęknięcie pancerza: za warstwę cel otrzymuje tyle więcej obrażeń fizycznych.")]
        public float sunderPerStack = 0.04f;
        public int sunderMaxStacks = 5;
        public float sunderDuration = 4f;
        [Tooltip("Wybuch żaru: ziemia w płonący cel – dodatkowy ogień za tę część wylądowanych obrażeń ziemi.")]
        public float emberBurstShare = 0.5f;

        [Header("Tempo")]
        [Tooltip("Globalna szybkość rozgrywki (Time.timeScale w trakcie gry). 1 = czas rzeczywisty.")]
        [Range(0.5f, 2f)] public float gameSpeed = 1.1f;
        [Tooltip("Mnożnik szybkości akcji gracza (ataki, parowanie, czary, flaszki, riposta). >1 = szybciej. Okno parowania się nie skraca.")]
        [Range(0.5f, 2f)] public float playerActionSpeed = 1.1f;
        [Tooltip("Dodatkowe skrócenie fazy zakończenia akcji gracza (i punktu przerwania) – bohater szybciej odzyskuje kontrolę.")]
        [Range(0.3f, 1f)] public float playerRecoveryScale = 0.8f;
        [Tooltip("Przyspieszenie ruchu gracza (m/s²). Duże = responsywnie, małe = ślisko.")]
        public float moveAcceleration = 45f;
        [Tooltip("Hamowanie ruchu gracza (m/s²).")]
        public float moveDeceleration = 60f;
        [Tooltip("Szybkość obrotu bohatera przy swobodnym ruchu (stopnie/s).")]
        public float playerTurnSpeed = 540f;

        [Header("Odczucie trafień (bez wstrząsów kamery)")]
        [Tooltip("Hitstop: trafiona postać (i atakujący bronią) zatrzymuje się na chwilę – lekki cios.")]
        public float hitstopLight = 0.05f;
        [Tooltip("Hitstop ciężkiego ciosu, przełamania gardy i ciosu kończącego.")]
        public float hitstopHeavy = 0.1f;
        [Tooltip("Hitstop riposty.")]
        public float hitstopRiposte = 0.15f;
        [Tooltip("Hitstop bloku i parowania (obie strony).")]
        public float hitstopBlock = 0.06f;
        [Tooltip("Siła odchylenia ciała przy trafieniu (0 = bez reakcji, 1 = domyślnie).")]
        public float hitReactStrength = 1f;
        [Tooltip("Odrzut wroga przy lekkim i ciężkim ciosie (m). Bossy ×0,2, elity ×0,5, pancerz w zamachu ×0,3.")]
        public float knockbackLight = 0.25f, knockbackHeavy = 0.7f;
        [Tooltip("Odrzut bohatera po otrzymanym ciosie (m); blok – połowa.")]
        public float playerKnockback = 0.4f;

        [Header("Unik")]
        [Tooltip("HU 26.10: unik droższy o 20% (18 → 21,6).")]
        public float dodgeStaminaCost = 21.6f;
        public float dodgeDistance = 4.2f;
        public float dodgeInvulnStart = 0.04f;
        public float dodgeInvulnDuration = 0.28f;
        public float dodgeTotalDuration = 0.82f;
        [Tooltip("Czas samego przewrotu (animacja i przemieszczenie). Dłuższy = wolniejszy, bardziej naturalny przewrót.")]
        public float dodgeRollDuration = 0.6f;
        [Tooltip("Po ilu sekundach fazy zakończenia uniku można wykonać kolejną akcję (atak, unik, blok, parowanie, czar). " +
                 "Najlepiej tuż przed końcem przewrotu: dodgeInvulnStart + dodgeInvulnDuration + ta wartość ≈ dodgeRollDuration.")]
        public float dodgeCancelAfter = 0.24f;
        [Tooltip("Obciążenie powyżej tego progu (0-1) pogarsza unik.")]
        [Range(0, 1)] public float heavyLoadThreshold = 0.7f;
        public float heavyDodgeDistanceMult = 0.8f;
        public float heavyDodgeInvulnMult = 0.75f;
        public float heavyDodgeCostMult = 1.25f;

        [Header("Obracanie wrogów (zachodzenie od tyłu)")]
        [Tooltip("Mnożnik szybkości obrotu wroga w czasie uniku gracza i chwilę po nim – wróg „gubi” przetaczającego się gracza.")]
        [Range(0, 1)] public float enemyTurnDuringDodge = 0.35f;
        [Tooltip("Jak długo po uniku (s) wrogowie wciąż obracają się wolniej.")]
        public float enemyTurnDodgeGrace = 0.4f;
        [Tooltip("Mnożnik szybkości obrotu, gdy gracz jest za plecami wroga (ponad 100° od kierunku patrzenia).")]
        [Range(0, 1)] public float enemyTurnBehind = 0.6f;

        [Header("Tempo wrogów")]
        [Tooltip("Globalny mnożnik szybkości poruszania się wszystkich wrogów.")]
        public float enemyMoveSpeedMultiplier = 0.9f;
        [Tooltip("Globalny mnożnik tempa ataków wrogów (zamach, cios, powrót). <1 = wolniejsze ataki.")]
        public float enemyAttackSpeedMultiplier = 0.9f;

        [Header("Flaszki")]
        [Tooltip("Część maks. życia przywracana przez flaszkę życia.")]
        public float healthFlaskFraction = 0.4f;
        public float manaFlaskFraction = 0.55f;
        public float flaskEffectDuration = 1.2f;
        public float flaskDrinkTime = 0.55f;
        public float flaskRecovery = 0.55f;
        public float flaskMoveSpeedMultiplier = 0.4f;

        [Header("Riposta")]
        public float riposteRange = 2.6f;
        public float riposteAngle = 120f;
        public float riposteStartup = 0.3f;
        public float riposteRecovery = 0.6f;

        [Header("Ruch")]
        public float moveSpeed = 5.2f;
        public float sprintMultiplier = 1.55f;
        public float guardMoveMultiplier = 0.55f;
        [Tooltip("Obrót w bloku (stopnie/s) w stronę wroga, który zamachuje się lub uderza – blok zasłania kierunek ataku.")]
        public float guardTurnSpeed = 1800f;
        [Tooltip("Cios w blok z boku do tylu stopni od przodu bohatera: bohater obraca się do niego i blok jest ważny. Z większego kąta (plecy) – nie.")]
        [Range(0f, 180f)] public float guardAutoTurnAngle = 120f;

        [Header("Budżet przygotowania")]
        public int loadoutBudget = 4;

        [Header("Kapliczka")]
        public int weaponUpgradeSoulCost = 150;
        [Header("Sklep dusz (między piętrami)")]
        [Tooltip("Ulepszenie umiejętności: koszt × (poziom + 1).")]
        public int skillUpgradeSoulCost = 60;
        public int skillOfferSoulCost = 110;
        [Tooltip("Wzrost cen za każde ukończone piętro po pierwszym.")]
        public float shopInflationPerFloor = 0.15f;
        public int flaskUpgradeSoulCost = 200;
        public int maxItemLevel = 5;
    }

    [CreateAssetMenu(menuName = "Turris/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public BalanceConfig balance = new BalanceConfig();
        public List<ClassDefinition> classes = new List<ClassDefinition>();
        public List<DifficultyDefinition> difficulties = new List<DifficultyDefinition>();
        public TowerDefinition tower;
        [Tooltip("Pełny katalog demonów HU; dostępny w bestiariuszu i próbach, niezależnie od składu sześciu pięter.")]
        public List<EnemyDefinition> enemyCatalog = new List<EnemyDefinition>();
        [Tooltip("Treść zawsze dostępna w puli nagród.")]
        public List<ItemDefinition> baseItemPool = new List<ItemDefinition>();
        public List<BoonDefinition> baseBoonPool = new List<BoonDefinition>();
        public List<SpellDefinition> baseSpellPool = new List<SpellDefinition>();
        [Tooltip("Treść odblokowywana popiołem.")]
        public List<UnlockDefinition> unlocks = new List<UnlockDefinition>();
        [Tooltip("Pięści: broń zastępcza, gdy główna ręka jest pusta.")]
        public ItemDefinition unarmed;
    }
}
