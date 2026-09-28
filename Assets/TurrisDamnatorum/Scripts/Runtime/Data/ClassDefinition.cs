using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Class", fileName = "Class")]
    public class ClassDefinition : ContentDefinition
    {
        public int vigor = 10, endurance = 10, mind = 10, strength = 10, dexterity = 10, intelligence = 10;
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
                case AttributeType.Vigor: return vigor;
                case AttributeType.Endurance: return endurance;
                case AttributeType.Mind: return mind;
                case AttributeType.Strength: return strength;
                case AttributeType.Dexterity: return dexterity;
                default: return intelligence;
            }
        }
    }
}
