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
        public float inputBuffer = 0.25f;

        [Header("Unik")]
        public float dodgeStaminaCost = 18f;
        public float dodgeDistance = 4.2f;
        public float dodgeInvulnStart = 0.04f;
        public float dodgeInvulnDuration = 0.28f;
        public float dodgeTotalDuration = 0.75f;
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
        public float moveSpeed = 4.6f;
        public float sprintMultiplier = 1.55f;
        public float guardMoveMultiplier = 0.55f;

        [Header("Budżet przygotowania")]
        public int loadoutBudget = 4;
        public int maxAttunedSpells = 3;

        [Header("Nagrody")]
        [Range(0, 1)] public float rewardExplorationChance = 0.3f;
        [Range(0, 1)] public float learnNewSpellChance = 0.35f;
        public int weaponUpgradeSoulCost = 150;
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
