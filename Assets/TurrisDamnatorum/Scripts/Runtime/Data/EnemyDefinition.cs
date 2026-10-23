using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Enemy", fileName = "Enemy")]
    public class EnemyDefinition : ContentDefinition
    {
        [Header("Migracja HU P1")]
        public string huSourceId, huSourceHash, huModel;
        public int huMigrationVersion;
        public int huArchetype = -1, huSplitCount, huSummonCount;
        public float huWakeRadius, huHatchSeconds, huEnrageHealth, huEnrageTempo = 1f;
        public bool huCanBeElite = true;
        public EnemyDefinition huSummon;
        public string archetype = "Wojownik";
        public float maxHealth = 300f;
        public float maxPoise = 40f;
        public float poiseRegenDelay = 2f;
        public float poiseRegen = 20f;
        public float maxStamina = 80f;
        [Range(0, 0.9f)] public float physicalResist;
        [Range(0, 0.9f)] public float magicResist;
        public float moveSpeed = 3.5f;
        public float turnSpeed = 360f;
        [Header("Żywioły (mnożnik obrażeń danego żywiołu: >1 słabość, <1 odporność)")]
        public float fireMultiplier = 1f;
        public float frostMultiplier = 1f;
        public float lightningMultiplier = 1f;
        [Tooltip("Mnożnik obrażeń krwawienia (0 = odporny).")]
        public float bleedMultiplier = 1f;
        public float earthMultiplier = 1f;

        [Tooltip("Żywioł wroga (wariant): stała aura i poświata broni w jego kolorze – od razu widać, z kim walczysz.")]
        public Element element = Element.None;
        [Header("Warianty żywiołów")]
        [Tooltip("Warianty (np. płonący ghul) losowane na piętrze zamiast tego wroga.")]
        public List<EnemyDefinition> variants = new List<EnemyDefinition>();
        [Range(0, 1)] public float variantChance = 0.4f;

        public float ElementMultiplier(Element e) => e == Element.Fire ? fireMultiplier : e == Element.Frost ? frostMultiplier : e == Element.Lightning ? lightningMultiplier : e == Element.Earth ? earthMultiplier : 1f;
        public float preferredRange = 2.2f;
        [Tooltip("Poniżej tej odległości przeciwnik próbuje się wycofać (przeciwnik dystansowy).")]
        public float retreatRange;
        [Tooltip("Losowy odstęp między atakami (s).")]
        public Vector2 attackInterval = new Vector2(0.6f, 1.4f);
        [Range(0, 1)] public float strafeChance = 0.4f;
        [Range(0, 1)] public float evadeChance;
        [Range(0, 1)] public float guardChance;
        public GuardData guard = new GuardData();
        public float riposteWindow = 1.8f;
        public List<EnemyAttackEntry> attacks = new List<EnemyAttackEntry>();
        public List<EnemyPhase> phases = new List<EnemyPhase>();
        public bool isBoss;
        public int soulReward = 50;
        public Color color = Color.red;
        public float scale = 1f;
        [Header("Wygląd")]
        [Tooltip("Sylwetka proceduralnego humanoida. Auto = na podstawie id.")]
        public RigStyle rigStyle = RigStyle.Auto;
        public WeaponModel weaponModel = WeaponModel.Auto;
        [Tooltip("Opcjonalny model FBX z animacjami. Puste = proceduralny humanoid.")]
        public CharacterVisualDefinition visual;

        [Header("Portale (Hell Unleashed)")]
        [Tooltip("Koszt w budżecie piętra z portalami (silniejszy demon kosztuje więcej).")]
        public float spawnCost = 1f;
        [Tooltip("Ile sztuk wychodzi z portalu za jeden koszt (stado czerwi – 4). Elitą może być tylko pierwsza.")]
        public int packSize = 1;
        [Tooltip("Mnożniki elity (Turris: ×1,3 zdrowia i ×1,1 obrażeń; złote elity Hell Unleashed: ×7 i ×1,6).")]
        public float eliteHealthMultiplier = 1.3f;
        public float eliteDamageMultiplier = 1.1f;
        [Tooltip("Sługa (np. nietoperz Władcy Bramy): nie może być elitą i ginie razem z przyzywającym.")]
        public bool isMinion;
    }
}
