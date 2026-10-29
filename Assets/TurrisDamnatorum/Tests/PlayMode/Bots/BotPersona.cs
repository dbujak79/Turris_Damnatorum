namespace Turris.Tests.Bots
{
    /// <summary>Osobowość bota: kto siedzi przy padzie.</summary>
    public enum BotPersona
    {
        /// <summary>Tester QA: sprawdza reguły, pokrycie mechanik i szuka błędów; gra poprawnie, ale celowo eksperymentuje.</summary>
        Tester,
        /// <summary>Gracz lubiący hack and slash i souls-like: agresywny, ale czyta telegrafy, paruje, karze otwarcia.</summary>
        SoulsSlasher,
    }

    /// <summary>Reakcja na nadchodzący cios.</summary>
    public enum DefResponse { None, Block, Parry, Dodge, StepOut }

    /// <summary>Parametry „ludzkiego” sterowania danej osobowości.</summary>
    public class PersonaProfile
    {
        public BotPersona persona;
        public string name, description;
        /// <summary>Czas reakcji na początek zamachu wroga (s) i jego rozrzut.</summary>
        public float reaction, reactionJitter;
        /// <summary>Odchylenie standardowe błędu wyczucia parowania/uniku (s).</summary>
        public float timingError;
        /// <summary>Szansa, że zwykły cios do sparowania zostanie sparowany (gdy broń na to pozwala).</summary>
        public float parryChance;
        /// <summary>Skłonność do atakowania poza wyraźnym otwarciem (0–1).</summary>
        public float aggression;
        /// <summary>Próg życia, poniżej którego pije flaszkę.</summary>
        public float flaskThreshold;
        /// <summary>Poniżej tego ułamka wytrzymałości odpuszcza atak i robi dystans.</summary>
        public float lowStamina;
        /// <summary>Długość serii ciosów przed odskokiem.</summary>
        public int comboMin, comboMax;
        /// <summary>Ułamek ciosów, na które tester reaguje celowo „eksperymentalnie” (rotacja wszystkich reakcji).</summary>
        public float experimentRate;
        /// <summary>Czy między podejściami kupuje odblokowania za popiół (meta-progresja).</summary>
        public bool metaProgression;
        /// <summary>Skłonność do używania skilli przy każdej okazji (hack and slash).</summary>
        public float skillEagerness;

        public static PersonaProfile For(BotPersona p)
        {
            switch (p)
            {
                case BotPersona.Tester:
                    return new PersonaProfile
                    {
                        persona = p, name = "Tester QA",
                        description = "Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją " +
                                      "(blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, " +
                                      "używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu " +
                                      "najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).",
                        reaction = 0.30f, reactionJitter = 0.08f, timingError = 0.06f, parryChance = 0.35f, aggression = 0.75f,
                        flaskThreshold = 0.35f, lowStamina = 0.15f, comboMin = 2, comboMax = 4, experimentRate = 0.30f,
                        metaProgression = false, skillEagerness = 1f,
                    };
                default:
                    return new PersonaProfile
                    {
                        persona = p, name = "Gracz H&S / souls-like",
                        description = "Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia " +
                                      "seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. " +
                                      "Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; " +
                                      "układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. " +
                                      "Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.",
                        reaction = 0.20f, reactionJitter = 0.06f, timingError = 0.045f, parryChance = 0.65f, aggression = 0.55f,
                        flaskThreshold = 0.42f, lowStamina = 0.25f, comboMin = 2, comboMax = 3, experimentRate = 0f,
                        metaProgression = true, skillEagerness = 0.85f,
                    };
            }
        }
    }
}
