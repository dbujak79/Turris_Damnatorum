using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public ModifierMode mode;
        public float value;

        public StatModifier(StatType stat, float value, ModifierMode mode = ModifierMode.Flat)
        {
            this.stat = stat; this.value = value; this.mode = mode;
        }

        public StatModifier Scaled(float factor) => new StatModifier(stat, value * factor, mode);

        public override string ToString()
        {
            string sign = value >= 0 ? "+" : "";
            return mode == ModifierMode.Percent
                ? $"{sign}{value:0.#}% {Names.Stat(stat)}"
                : $"{sign}{value:0.##} {Names.Stat(stat)}";
        }
    }

    [Serializable]
    public struct PassiveEffect
    {
        public PassiveEffectType type;
        public float value;
        public PassiveEffect(PassiveEffectType type, float value) { this.type = type; this.value = value; }
        public override string ToString() => Names.Effect(type, value);
    }

    [Serializable]
    public struct AttributeRequirement
    {
        public AttributeType attribute;
        public int value;
        public AttributeRequirement(AttributeType a, int v) { attribute = a; value = v; }
    }

    /// <summary>Efekt nakładany przez atak, gdy trafienie dojdzie do celu (nie zablokowane, nie sparowane, nie uniknięte).</summary>
    [Serializable]
    public class StatusApplication
    {
        public StatusKind kind;
        [Min(1)] public int stacks = 1;
        [Range(0, 1)] public float chance = 1f;

        public StatusApplication() { }
        public StatusApplication(StatusKind kind, int stacks = 1, float chance = 1f) { this.kind = kind; this.stacks = stacks; this.chance = chance; }
    }

    /// <summary>
    /// Kompletny opis jednego ataku (gracza, przeciwnika lub czaru). Każda właściwość obronna jest niezależna:
    /// atak może być np. blokowalny, ale nie do sparowania.
    /// </summary>
    [Serializable]
    public class AttackDefinition
    {
        public string name = "Atak";
        public AttackDelivery delivery = AttackDelivery.Melee;
        public DamageType damageType = DamageType.Physical;
        public float baseDamage = 30f;

        [Header("Czas (s)")]
        public float windup = 0.35f;
        public float active = 0.15f;
        public float recovery = 0.45f;
        [Tooltip("Po ilu sekundach fazy regeneracji gracz może przerwać akcję (kombinacja, unik, blok).")]
        public float cancelAfter = 0.2f;

        [Header("Zasięg")]
        public float reach = 2.0f;
        public float radius = 0.9f;
        [Tooltip("Dystans, o jaki atakujący przesuwa się w przód podczas zamachu i fazy aktywnej.")]
        public float lunge = 0.5f;
        [Tooltip("Szybkość obrotu w stronę celu w trakcie zamachu (stopnie/s).")]
        public float tracking = 360f;

        [Header("Właściwości obronne")]
        public bool blockable = true;
        public bool parryable = true;
        [Tooltip("Czy okno niewrażliwości uniku chroni przed tym atakiem.")]
        public bool dodgeable = true;
        [Tooltip("Obciążenie gardy: ile wytrzymałości kosztuje obrońcę zablokowanie ciosu (przed mnożnikiem stabilności).")]
        public float guardLoad = 20f;
        public float poiseDamage = 15f;
        public bool heavy;

        [Header("Koszt")]
        public float staminaCost = 15f;

        [Header("Pociski")]
        public float projectileSpeed = 14f;
        public int projectileCount = 1;
        public float spreadAngle = 0f;

        [Tooltip("Pocisk: promień wybuchu przy trafieniu (0 = bez wybuchu).")]
        public float explosionRadius = 0f;

        [Header("Żywioły i efekty")]
        public Element element = Element.None;
        [Tooltip("Broń z żywiołem: ta część obrażeń fizycznych zamienia się w obrażenia żywiołu (0–1).")]
        [Range(0, 1)] public float elementShare = 0f;
        [Tooltip("Premia do obrażeń przeciw zamrożonym (0.5 = +50%). Rozbija lód.")]
        public float bonusVsFrozen = 0f;
        [Tooltip("Premia do obrażeń przeciw krwawiącym.")]
        public float bonusVsBleeding = 0f;
        [Tooltip("Egzekucja: premia przeciw celom poniżej 30% życia lub z przełamaną postawą.")]
        public float executeBonus = 0f;
        [Tooltip("Efekty nakładane przy trafieniu, które dotarło do celu.")]
        public List<StatusApplication> statuses = new List<StatusApplication>();

        [Header("Animacja")]
        [Tooltip("Styl ruchu ataku. Auto = wyliczany z parametrów (ciężki → z góry, pocisk → czar itd.).")]
        public AttackAnim animation = AttackAnim.Auto;

        public AttackDefinition Clone() => (AttackDefinition)MemberwiseClone();

        public float TotalDuration => windup + active + recovery;

        /// <summary>Czytelny sygnał wynikający bezpośrednio z właściwości ataku (bez osobnej, rozjeżdżającej się flagi).</summary>
        public TelegraphKind Telegraph
        {
            get
            {
                if (!dodgeable) return TelegraphKind.NoDodge;
                if (!blockable) return TelegraphKind.Unblockable;
                if (!parryable) return TelegraphKind.Unparryable;
                return heavy ? TelegraphKind.Heavy : TelegraphKind.Normal;
            }
        }
    }

    public enum TelegraphKind { Normal, Heavy, Unparryable, Unblockable, NoDodge }

    /// <summary>Parametry gardy (tarczy lub broni).</summary>
    [Serializable]
    public class GuardData
    {
        [Range(0, 1)] public float physicalReduction = 1f;
        [Range(0, 1)] public float magicReduction = 0.4f;
        [Tooltip("Mnożnik obciążenia gardy (niżej = stabilniej).")]
        public float stabilityMultiplier = 0.7f;
        [Tooltip("Całkowity kąt ochrony przed postacią (stopnie).")]
        public float blockAngle = 140f;
        [Tooltip("Mnożnik regeneracji wytrzymałości przy podniesionej gardzie.")]
        [Range(0, 1)] public float staminaRegenMultiplier = 0.3f;
    }

    /// <summary>Parametry parowania. Wszystkie czasy w sekundach.</summary>
    [Serializable]
    public class ParryData
    {
        public float startup = 0.08f;
        public float activeWindow = 0.2f;
        public float recovery = 0.5f;
        public float staminaCost = 15f;
        [Tooltip("Obrażenia postawy zadawane atakującemu przy udanym parowaniu.")]
        public float poiseDamage = 70f;
        public float angle = 150f;
    }

    [Serializable]
    public class WeaponData
    {
        public AttackDefinition light = new AttackDefinition();
        public AttackDefinition heavy = new AttackDefinition { name = "Ciężki atak", heavy = true };
        [Tooltip("Maksymalna liczba lekkich ciosów w serii.")]
        public int lightComboLength = 3;

        [Header("Skalowanie (% obrażeń za punkt atrybutu)")]
        public float strengthScaling = 1f;
        public float dexterityScaling = 1f;
        public float intelligenceScaling = 0f;

        public float riposteMultiplier = 3f;

        [Header("Obrona bronią")]
        public bool canBlock;
        public GuardData guard = new GuardData { physicalReduction = 0.6f, magicReduction = 0.2f, stabilityMultiplier = 1.2f, blockAngle = 100f, staminaRegenMultiplier = 0.3f };
        public bool canParry;
        public ParryData parry = new ParryData();
    }

    [Serializable]
    public class ShieldData
    {
        public GuardData guard = new GuardData();
        public bool canParry = true;
        public ParryData parry = new ParryData();
    }
}
