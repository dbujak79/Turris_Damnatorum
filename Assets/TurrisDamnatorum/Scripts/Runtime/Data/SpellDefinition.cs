using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Spell", fileName = "Spell")]
    public class SpellDefinition : ContentDefinition
    {
        public SpellKind kind;
        public float manaCost = 15f;
        public float castTime = 0.45f;
        public float recovery = 0.4f;
        public float cancelAfter = 0.2f;
        public float staminaCost = 5f;
        public List<AttributeRequirement> requirements = new List<AttributeRequirement>();
        [Tooltip("% mocy za punkt Inteligencji.")]
        public float intelligenceScaling = 2.5f;
        [Tooltip("Właściwości trafienia (Projectile / Nova). Czasy brane są z castTime/recovery.")]
        public AttackDefinition attack = new AttackDefinition { damageType = DamageType.Magic };
        [Tooltip("Heal: leczenie łączne; WeaponBuff: dodatkowe obrażenia magiczne na cios.")]
        public float amount = 60f;
        [Tooltip("Heal: czas leczenia; WeaponBuff: czas trwania.")]
        public float duration = 3f;
        [Header("Ulepszenia")]
        public int maxLevel = 4;
        public float powerPerLevel = 0.15f;
        public float costReductionPerLevel = 0.06f;
        public Color color = new Color(0.4f, 0.6f, 1f);
    }
}
