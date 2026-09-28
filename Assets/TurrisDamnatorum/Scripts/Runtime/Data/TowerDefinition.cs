using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Tower", fileName = "Tower")]
    public class TowerDefinition : ContentDefinition
    {
        public List<FloorDefinition> floors = new List<FloorDefinition>();
        public int victoryAshBonus = 40;
    }
}
