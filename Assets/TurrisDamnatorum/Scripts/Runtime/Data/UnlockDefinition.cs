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
        [Tooltip("Klasy, które mogą wziąć to na start podejścia (dodatek lub stała umiejętność). Puste = ogólne, dla każdej klasy. " +
                 "W trakcie podejścia nagrody nie są ograniczone klasą.")]
        public List<ClassDefinition> classes = new List<ClassDefinition>();

        public bool IsGeneral => classes == null || classes.Count == 0;
        /// <summary>Dostępne dla klasy – także dla specjalizacji klasy wskazanej na liście (i odwrotnie: dodatek specjalizacji tylko dla niej).</summary>
        public bool AvailableFor(ClassDefinition c) => IsGeneral || c == null || classes.Contains(c) || (c.parent != null && classes.Contains(c.parent));
    }
}
