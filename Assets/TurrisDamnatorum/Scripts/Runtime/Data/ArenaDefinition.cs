using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Arena", fileName = "Arena")]
    public class ArenaDefinition : ContentDefinition
    {
        [Tooltip("Styl dekoracji areny. Auto = na podstawie id.")]
        public ArenaStyle style = ArenaStyle.Auto;
        public bool circular = true;
        public float size = 14f;
        public List<Vector3> pillars = new List<Vector3>();
        public Color floorColor = new Color(0.25f, 0.24f, 0.22f);
        public Color wallColor = new Color(0.35f, 0.33f, 0.3f);
        public Color lightColor = new Color(1f, 0.9f, 0.8f);
        public Color fogColor = new Color(0.08f, 0.07f, 0.09f);
    }
}
