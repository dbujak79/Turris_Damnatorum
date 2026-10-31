using System;
using System.Collections.Generic;

namespace Turris
{
    /// <summary>Wariant cechy klasowej: A albo B (kolejność kart na ekranie przygotowania, nie tożsamość).</summary>
    public enum ClassFeatureVariant { A, B }

    /// <summary>Jak cecha korzysta z przycisku klasy (G / View).</summary>
    public enum ClassFeatureActivation
    {
        /// <summary>Jedno naciśnięcie otwiera okno działania.</summary>
        Window,
        /// <summary>Pierwsze naciśnięcie przygotowuje/zbiera, drugie wyzwala (druga faza).</summary>
        TwoPhase,
        /// <summary>Natychmiastowe działanie (np. wydanie zasobu, zmiana zestawu).</summary>
        Instant,
    }

    /// <summary>
    /// Definicja cechy klasowej – tylko dane (bez stanu walki). Stan żyje w <see cref="ClassFeatureController"/> konkretnej postaci.
    /// Tożsamość to <see cref="id"/> (np. <c>class_feature.berserker.redline</c>) – nie indeks karty, nazwa ani wygląd.
    /// Teksty z liczbami są funkcjami parametrów (<see cref="ClassFeatureTuning"/>), żeby opis nie rozjeżdżał się z danymi.
    /// Cecha należy wyłącznie do <see cref="classId"/>: specjalizacje nie dziedziczą cech typu bohatera i odwrotnie.
    /// </summary>
    public sealed class ClassFeatureDefinition
    {
        public string id;
        public string classId;
        public ClassFeatureVariant variant;
        public string name;
        public ClassFeatureActivation activation;
        /// <summary>Reguła wyłączna (czego nie potrafi nikt inny).</summary>
        public Func<ClassFeatureTuning, string> rule;
        public Func<ClassFeatureTuning, string> benefit;
        public Func<ClassFeatureTuning, string> cost;
        /// <summary>Sposób użycia; „{G}” zastępuje UI aktualnym przyciskiem klasy.</summary>
        public Func<ClassFeatureTuning, string> usage;
        /// <summary>Nietypowy build, który cecha uzasadnia (przykład na karcie).</summary>
        public string exampleBuild;
        /// <summary>Rdzeń dostępny od startu (prawdziwe id/nazwy z gry).</summary>
        public string coreBuild;
        /// <summary>Rozwinięty build z co najmniej dwiema dalszymi synergiami.</summary>
        public string developedBuild;
        /// <summary>Krótki opis części pasywnej poza walką – czemu broń/statystyka staje się ciekawa.</summary>
        public string passiveHint;
        /// <summary>
        /// Zwykłe skille, które wariant jawnie dokłada do wyboru startowego (pojawiają się na liście umiejętności przygotowania,
        /// zajmują zwykły slot albo kolekcję – bez ukrytego czwartego slotu). Te same skille są dostępne innym klasom w sklepie.
        /// </summary>
        public readonly List<string> startingSkillIds = new List<string>();
        /// <summary>Zasada resetu na nowym piętrze (jawny kontrakt z rozdz. 5.11).</summary>
        public string floorReset;

        public string Rule(ClassFeatureTuning t) => rule != null ? rule(t) : "";
        public string Benefit(ClassFeatureTuning t) => benefit != null ? benefit(t) : "";
        public string Cost(ClassFeatureTuning t) => cost != null ? cost(t) : "";
        public string Usage(ClassFeatureTuning t) => usage != null ? usage(t) : "";

        public override string ToString() => id;
    }
}
