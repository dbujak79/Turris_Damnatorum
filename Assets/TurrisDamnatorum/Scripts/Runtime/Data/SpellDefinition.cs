using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Cecha odblokowywana poziomem umiejętności (poza zwykłym wzrostem mocy).</summary>
    public enum LevelFeatureKind
    {
        ExtraTargets,      // +N pocisków / celów łańcucha
        RadiusBonus,       // +X (ułamek) promienia i zasięgu
        DurationBonus,     // +X s czasu trwania (strefa, burza, młynek, wzmocnienia)
        ExtraStatusStacks, // +N warstw nakładanych efektów
        LeaveZone,         // wybuch pocisku zostawia płonącą ziemię na X s
        FinalSlash,        // młynek kończy się cięciem za X × moc
    }

    [Serializable]
    public class LevelFeature
    {
        [Tooltip("Poziom umiejętności (+N), od którego cecha działa.")]
        public int level = 2;
        public LevelFeatureKind kind;
        public float value = 1f;

        public LevelFeature() { }
        public LevelFeature(int level, LevelFeatureKind kind, float value) { this.level = level; this.kind = kind; this.value = value; }
    }

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

        [Header("Skille z Hell Unleashed")]
        [Tooltip("Technika z własnymi obrażeniami (attack.baseDamage × premie), a nie z obrażeń broni.")]
        public bool flatDamage;
        [Tooltip("Nova/Warcry/Undying/Judgement: liczba impulsów, odstęp i opóźnienie pierwszego (telegraf).")]
        public int pulses = 1;
        public float pulseInterval = 0.45f;
        public float telegraph;
        [Tooltip("Nova: impuls idzie za bohaterem (zamiast zostać w miejscu rzucenia).")]
        public bool followCaster;
        [Tooltip("Odrzut trafionych (m); ujemny = przyciąganie do bohatera.")]
        public float knockback;
        [Tooltip("Pocisk: ilu kolejnych wrogów przebija.")]
        public int pierce;
        [Tooltip("Pocisk pęka na dwie drzazgi przy pierwszym trafieniu.")]
        public bool split;
        [Tooltip("Pocisk: czas lotu (s); 0 = 3 s.")]
        public float projectileLifetime;
        [Tooltip("Pocisk wybucha całymi obrażeniami (attack.explosionRadius), także na końcu lotu.")]
        public bool fullExplosion;
        [Tooltip("Strefa po wybuchu/nowie: promień, czas trwania, odstęp tyknięć, część obrażeń na tyknięcie.")]
        public float zoneRadius, zoneDuration, zoneInterval = 0.5f, zoneShare;
        [Tooltip("Leczenie przy rzuceniu: ułamek maks. życia (Boski osąd).")]
        public float healPercent;
        [Tooltip("Okrzyk: redukcja obrażeń (ułamek) na czas trwania (duration).")]
        public float damageReductionBuff;
        [Tooltip("Furia dodawana przy rzuceniu (cecha Furia/Żądza krwi).")]
        public float furyGain;
        [Tooltip("Summon: rodzaj, liczba sług i odstęp ich ataków.")]
        public ServantKind servant;
        public int servantCount = 1;
        public float servantAttackInterval = 1f;
        [Tooltip("Aura obronna: modyfikatory bohatera, dopóki aura trwa.")]
        public List<StatModifier> auraModifiers = new List<StatModifier>();
        [Tooltip("Whirlwind: końcowe cięcie – część obrażeń i promień (0 = brak).")]
        public float finalSlashShare, finalRadius;
        [Tooltip("Wave: szerokość pasa i prędkość fali; druga fala – część obrażeń i opóźnienie (0 = brak).")]
        public float waveWidth = 1.6f, waveSpeed = 16f, extraWaveShare, extraWaveDelay = 0.25f;
        [Tooltip("Dash: dystans doskoku (attack.reach) pokonywany w tym czasie; zatrzymuje się przy wrogu.")]
        public float dashTime = 0.16f;

        [Tooltip("Najwcześniej po tylu ukończonych piętrach skill pojawia się w nagrodach i sklepie (HU: pierwsza fala w sklepie).")]
        public int minFloor;

        [Header("Ulepszenia")]
        [Tooltip("Cechy dochodzące z poziomem (np. od +2 większy promień, od +4 płonąca ziemia).")]
        public List<LevelFeature> levelFeatures = new List<LevelFeature>();
        public int maxLevel = 4;
        public float powerPerLevel = 0.15f;
        public float costReductionPerLevel = 0.06f;
        public Color color = new Color(0.4f, 0.6f, 1f);

        public bool IsSpell => category == SkillCategory.Spell;

        [NonSerialized] HuSkillData huData; [NonSerialized] bool huLooked;
        /// <summary>Źródło skilla z Hell Unleashed (komponenty, poziomy, pieczęcie) albo null dla skilli Turris.</summary>
        public HuSkillData HuData { get { if (!huLooked) { huLooked = true; huData = HuCatalog.ForTarget(id); } return huData; } }

        /// <summary>Suma wartości cechy dostępnej na danym poziomie.</summary>
        public float Feature(LevelFeatureKind k, int level)
        {
            float v = 0;
            foreach (var f in levelFeatures) if (f != null && f.kind == k && level >= f.level) v += f.value;
            return v;
        }

        public int MaxCharges(int level) => Mathf.Max(1, charges + (extraChargeAtLevel > 0 && level >= extraChargeAtLevel ? 1 : 0));
        public float CooldownAt(int level) => cooldown * Mathf.Max(0.4f, 1f - cooldownReductionPerLevel * level);
        /// <summary>Aura: włączana i gaszona tym samym przyciskiem, działa do odwołania.</summary>
        public bool IsAura => kind == SpellKind.Aura;
        public float CostFactor(int level) => Mathf.Max(0.4f, 1f - costReductionPerLevel * level);
    }
}
