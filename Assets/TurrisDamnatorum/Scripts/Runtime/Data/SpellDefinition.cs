using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Umiejętność: czar (mana, Inteligencja) albo technika bronią (wytrzymałość, obrażenia broni).
    /// Nazwa klasy zostaje historyczna ("Spell"), bo odwołują się do niej zapisane assety.
    /// Gracz ma trzy sloty umiejętności, każdy pod osobnym przyciskiem.
    /// </summary>
    [CreateAssetMenu(menuName = "Turris/Skill (czar lub technika)", fileName = "Skill")]
    public class SpellDefinition : ContentDefinition
    {
        public SpellKind kind;
        public SkillCategory category = SkillCategory.Spell;
        public float manaCost = 15f;
        public float castTime = 0.45f;
        public float recovery = 0.4f;
        public float cancelAfter = 0.2f;
        public float staminaCost = 5f;
        public List<AttributeRequirement> requirements = new List<AttributeRequirement>();
        [Tooltip("Wymagane wyposażenie (np. Shield dla uderzenia tarczą). Bez niego umiejętności nie da się użyć.")]
        public BuildTag requiredTags = BuildTag.None;
        [Tooltip("% mocy za punkt Inteligencji (czary).")]
        public float intelligenceScaling = 2.5f;
        [Tooltip("Właściwości trafienia. Projectile/Nova: obrażenia bazowe; techniki: zasięg (reach), promień, postawa, obciążenie gardy.")]
        public AttackDefinition attack = new AttackDefinition { damageType = DamageType.Magic };
        [Tooltip("Heal: leczenie łączne; WeaponBuff: dodatkowe obrażenia magiczne na cios; Barrier: pojemność osłony.")]
        public float amount = 60f;
        [Tooltip("Heal/WeaponBuff/Barrier: czas trwania; Charge/Whirlwind: czas fazy aktywnej (szarży, wirowania).")]
        public float duration = 3f;

        [Header("Odnowienie")]
        [Tooltip("Sekundy odnowienia jednego ładunku. Liczone zegarem gry – pauza je zatrzymuje.")]
        public float cooldown = 1f;
        [Tooltip("Liczba ładunków na poziomie 0.")]
        public int charges = 1;
        [Tooltip("Od tego poziomu umiejętność ma dodatkowy ładunek (0 = nigdy).")]
        public int extraChargeAtLevel;
        public float cooldownReductionPerLevel = 0.06f;

        [Header("Techniki bronią")]
        [Tooltip("Obrażenia = obrażenia lekkiego ataku broni × ten mnożnik (na trafienie).")]
        public float weaponMultiplier = 1.5f;
        [Tooltip("Cleave/ShieldBash: kąt łuku trafienia (stopnie).")]
        public float arcAngle = 150f;
        [Tooltip("Whirlwind: co ile sekund ten sam cel może zostać trafiony ponownie.")]
        public float tickInterval = 0.35f;
        [Tooltip("Whirlwind: mnożnik szybkości ruchu w trakcie wirowania.")]
        public float moveMultiplier = 0.5f;

        [Header("Ulepszenia")]
        public int maxLevel = 4;
        public float powerPerLevel = 0.15f;
        public float costReductionPerLevel = 0.06f;
        public Color color = new Color(0.4f, 0.6f, 1f);

        public bool IsSpell => category == SkillCategory.Spell;

        public int MaxCharges(int level) => Mathf.Max(1, charges + (extraChargeAtLevel > 0 && level >= extraChargeAtLevel ? 1 : 0));
        public float CooldownAt(int level) => cooldown * Mathf.Max(0.4f, 1f - cooldownReductionPerLevel * level);
        public float CostFactor(int level) => Mathf.Max(0.4f, 1f - costReductionPerLevel * level);
    }
}
