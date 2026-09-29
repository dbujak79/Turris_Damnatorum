using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Class", fileName = "Class")]
    public class ClassDefinition : ContentDefinition
    {
        [Header("Cechy startowe")]
        [UnityEngine.Serialization.FormerlySerializedAs("vigor")] public int toughness = 10;
        public int strength = 10, dexterity = 10, intelligence = 10;
        [HideInInspector] public int endurance = 10, mind = 10; // wycofane (zostają tylko dla zgodności zapisanych assetów)
        public List<ItemDefinition> startingItems = new List<ItemDefinition>();
        public List<SpellDefinition> startingSpells = new List<SpellDefinition>();
        public List<BoonDefinition> startingTalents = new List<BoonDefinition>();
        public int healthFlasks = 3;
        public int manaFlasks = 1;
        public Color color = Color.white;
        [Tooltip("Opcjonalny model FBX z animacjami. Puste = proceduralny humanoid.")]
        public CharacterVisualDefinition visual;

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
