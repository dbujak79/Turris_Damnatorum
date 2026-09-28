using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Enemy", fileName = "Enemy")]
    public class EnemyDefinition : ContentDefinition
    {
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
    }
}
