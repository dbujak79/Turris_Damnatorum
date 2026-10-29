using System;
using UnityEngine;

namespace Turris
{
    /// <summary>Osobny zestaw modułów PoC; oryginalna definicja i generator areny pozostają dostępne.</summary>
    [CreateAssetMenu(menuName="Turris/PoC/Zestaw areny gotyckiej")]
    public sealed class GothicArenaSet : ScriptableObject
    {
        public GameObject[] modules;
        public Material stone, cloth, backdrop, ground, sky;
        public ArenaDefinition originalArena;
        public GameObject Module(string name) => Array.Find(modules,p=>p != null && p.name==name);
    }
}
