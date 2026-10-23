using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Zestaw broni do wyboru na start (Hell Unleashed: „Broń na start”): broń, druga ręka i żywioł karty.</summary>
    [Serializable]
    public class StartingLoadout
    {
        [Tooltip("Etykieta karty, np. Ogień. Puste = nazwa broni.")]
        public string label;
        public Element element;
        public ItemDefinition weapon;
        [Tooltip("Tarcza, grymuar, sztylet do rzucania albo druga broń jednoręczna (walka dwiema brońmi).")]
        public ItemDefinition offHand;
    }

    /// <summary>
    /// Klasa postaci. Typ bohatera (Wojownik, Mag, Łotrzyk) ma <see cref="parent"/> = null; specjalizacja wskazuje swój typ
    /// i ma własne cechy, broń startową, cechę klasy, unik i skill klasy (jak w Hell Unleashed).
    /// </summary>
    [CreateAssetMenu(menuName = "Turris/Class", fileName = "Class")]
    public class ClassDefinition : ContentDefinition
    {
        [Header("Typ i specjalizacja")]
        [Tooltip("Typ bohatera, którego to jest specjalizacja. Puste = typ bohatera (\"Bez specjalizacji\").")]
        public ClassDefinition parent;
        [Tooltip("Krótki podpis pod nazwą, np. „Miecz i pancerz”.")]
        public string tagline;
        public ClassTrait trait;
        public DodgeStyle dodgeStyle;
        [Tooltip("Skill klasy pod osobnym przyciskiem (nie zajmuje slotu).")]
        public SpellDefinition signatureSkill;
        [Tooltip("Startowe skille do wyboru (gracz bierze jeden), jeśli broń startowa nie daje własnego.")]
        public List<SpellDefinition> startingSkillChoices = new List<SpellDefinition>();
        [Tooltip("Zestawy broni na start; pierwszy = domyślny. Puste = startingItems/alternativeWeapons.")]
        public List<StartingLoadout> loadouts = new List<StartingLoadout>();
        [Tooltip("Mistrzostwo klasy: modyfikatory działające od początku podejścia.")]
        public List<StatModifier> classModifiers = new List<StatModifier>();
        [Tooltip("Model bohatera dla tej klasy; puste = model typu bohatera.")]
        public CharacterVisualDefinition lookOverride;

        [Header("Cechy startowe")]
        [UnityEngine.Serialization.FormerlySerializedAs("vigor")] public int toughness = 10;
        public int strength = 10, dexterity = 10, intelligence = 10;
        [HideInInspector] public int endurance = 10, mind = 10; // wycofane (zostają tylko dla zgodności zapisanych assetów)
        public List<ItemDefinition> startingItems = new List<ItemDefinition>();
        [Tooltip("Inne bronie do wyboru na start zamiast broni z wyposażenia (bez kosztu w budżecie), np. łuk dla łotrzyka.")]
        public List<ItemDefinition> alternativeWeapons = new List<ItemDefinition>();
        public List<SpellDefinition> startingSpells = new List<SpellDefinition>();
        public List<BoonDefinition> startingTalents = new List<BoonDefinition>();
        public int healthFlasks = 3;
        public int manaFlasks = 1;
        public Color color = Color.white;
        [Tooltip("Opcjonalny model FBX z animacjami. Puste = proceduralny humanoid.")]
        public CharacterVisualDefinition visual;

        public bool IsSpecialization => parent != null;
        /// <summary>Typ bohatera (dla specjalizacji – rodzic).</summary>
        public ClassDefinition Family => parent != null ? parent : this;
        /// <summary>Wygląd: własny, a gdy go brak – typu bohatera.</summary>
        public CharacterVisualDefinition Visual => lookOverride != null ? lookOverride : visual != null ? visual : parent != null ? parent.Visual : null;

        /// <summary>Broń z wyposażenia startowego (pierwsza broń na liście).</summary>
        public ItemDefinition DefaultWeapon => startingItems.Find(i => i != null && i.IsWeapon);

        /// <summary>Bronie do wyboru na start: domyślna i alternatywne.</summary>
        public List<ItemDefinition> WeaponChoices
        {
            get
            {
                var list = new List<ItemDefinition>();
                if (DefaultWeapon != null) list.Add(DefaultWeapon);
                foreach (var w in alternativeWeapons) if (w != null && !list.Contains(w)) list.Add(w);
                return list;
            }
        }

        public int GetAttribute(AttributeType a)
        {
            switch (a)
            {
                case AttributeType.Toughness: return toughness;
                case AttributeType.Strength: return strength;
                case AttributeType.Dexterity: return dexterity;
                default: return intelligence;
            }
        }
    }
}
