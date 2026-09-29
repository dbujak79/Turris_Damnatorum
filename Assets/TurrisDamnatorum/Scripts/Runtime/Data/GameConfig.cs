using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Parametry balansu edytowalne w inspektorze (bez zmian w kodzie).</summary>
    [Serializable]
    public class BalanceConfig
    {
        [Header("Atrybuty → statystyki")]
        public float baseHealth = 200f, healthPerVigor = 15f;
        public float baseStamina = 60f, staminaPerEndurance = 3f;
        public float baseMana = 20f, manaPerMind = 6f;
        public float baseStaminaRegen = 38f;
        public float baseEquipLoad = 30f, equipLoadPerEndurance = 1f;
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
        [Tooltip("Maksymalna odporność bohatera na żywioł.")]
        public float maxElementResist = 75f;
        [Tooltip("Czas kontroli (zamrożenie, spowolnienie) na bossach i elitach.")]
        public float bossControlMultiplier = 0.5f, eliteControlMultiplier = 0.7f;

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
        public float playerTurnSpeed = 1080f;

        [Header("Unik")]
        public float dodgeStaminaCost = 18f;
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

        [Header("Budżet przygotowania")]
        public int loadoutBudget = 4;

        [Header("Nagrody")]
        [Range(0, 1)] public float rewardExplorationChance = 0.3f;
        [Range(0, 1)] public float learnNewSpellChance = 0.35f;
        public int weaponUpgradeSoulCost = 150;
        [Header("Sklep dusz (między piętrami)")]
        [Tooltip("Ulepszenie umiejętności: koszt × (poziom + 1).")]
        public int skillUpgradeSoulCost = 60;
        public int skillOfferSoulCost = 110;
        public int rewardRerollSoulCost = 40;
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
