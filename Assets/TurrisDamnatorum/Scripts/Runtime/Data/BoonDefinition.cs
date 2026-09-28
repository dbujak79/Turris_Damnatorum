using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Boon", fileName = "Boon")]
    public class BoonDefinition : ContentDefinition
    {
        public int maxStacks = 3;
        public List<StatModifier> modifiers = new List<StatModifier>();
        public List<PassiveEffect> effects = new List<PassiveEffect>();
        [Tooltip("Liczba dodatkowych ładunków flaszek (życia, many) na stos.")]
        public int extraHealthFlasks;
        public int extraManaFlasks;
    }
}
