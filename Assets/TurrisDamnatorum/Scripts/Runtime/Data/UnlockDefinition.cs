using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Unlock", fileName = "Unlock")]
    public class UnlockDefinition : ContentDefinition
    {
        public UnlockKind kind;
        public ContentDefinition target;
        public int ashCost = 20;
        [Tooltip("Koszt w budżecie przygotowania, jeśli wybierany na start podejścia.")]
        public int loadoutCost = 1;
        public bool unlockedByDefault;
        [Tooltip("Wymagane najwyższe osiągnięte piętro (1-5), 0 = brak.")]
        public int requiredBestFloor;
        [Tooltip("Wymagane zwycięstwo na tym poziomie trudności (-1 = brak).")]
        public int requiredVictoryTier = -1;
    }
}
