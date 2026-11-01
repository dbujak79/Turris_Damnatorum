using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Turris
{
    /// <summary>
    /// Karta natywnego unikatu Turris (docs/PROMPT_WDROZENIE_75_UNIKATOW_KLASOWYCH.md) – wyłącznie dane, bez stanu walki.
    /// Tożsamość to <see cref="id"/> (np. <c>item.unique.turris.warrior.last_pavise</c>), niezależna od nazwy i kodu karty.
    /// Teksty z liczbami są szablonami <c>{klucz}</c>, wypełnianymi z <see cref="param"/> – opis i runtime czytają te same liczby.
    /// Parametry mocy są strukturalne: jakość i poziom egzemplarza skalują wyłącznie statystyki.
    /// </summary>
    public sealed class UniqueCard
    {
        /// <summary>Stabilne id definicji przedmiotu.</summary>
        public string id;
        /// <summary>Kod karty (W01…Q05) – nazwa ikony i sortowanie.</summary>
        public string code;
        /// <summary>Klasa, z którą karta projektuje synergię (kierunek, nie wyłączność – wyłączność daje <see cref="requiredFeature"/>).</summary>
        public string classId;
        public string name;
        public ItemKind kind;
        /// <summary>Broń/tarcza/grymuar: id przedmiotu bazowego z puli (model, ataki, chwyt). Pancerz i biżuteria: null.</summary>
        public string baseItemId;
        /// <summary>Cecha klasowa wymagana do działania mocy (A/B) – null dla mocy ogólnej (wspólny skill albo legalna akcja).</summary>
        public string requiredFeature;
        /// <summary>Skille, których moc dotyczy (id treści) – pusta lista: moc działa na akcjach bez skilla.</summary>
        public readonly List<string> skills = new List<string>();
        /// <summary>Warunek działania (tekst dla gracza).</summary>
        public string condition;
        /// <summary>Jednozdaniowa obietnica mocy.</summary>
        public string promise;
        /// <summary>Legendarna reguła (szablon z parametrami).</summary>
        public string rule;
        /// <summary>Koszt i ograniczenia (szablon z parametrami).</summary>
        public string cost;
        /// <summary>Osiągalny rdzeń buildu i rozwinięty build (prawdziwe id i nazwy).</summary>
        public string coreBuild, developedBuild;
        /// <summary>Świadomy koszt: co gracz oddaje i kiedy przeciwnik nadal zagraża.</summary>
        public string tradeoff;
        public readonly List<StatModifier> stats = new List<StatModifier>();
        public readonly Dictionary<string, float> param = new Dictionary<string, float>();
        /// <summary>Nowy pomysł (pozycje 04–05) z rozszerzenia katalogu.</summary>
        public bool isNew;

        // ------------------------------------------------------------------ Łup
        /// <summary>Najwcześniej po tylu ukończonych piętrach.</summary>
        public int minFloor;
        /// <summary>Waga w puli unikatów (przed preferencją klasy i poszukiwania).</summary>
        public float weight = 1f;
        /// <summary>Wypada tylko z bossa (bronie) – inaczej z elit i bossów.</summary>
        public bool bossOnly;
        /// <summary>Cena bazowa (dusze przed rzadkością i skalą).</summary>
        public float basePrice = 60f;
        /// <summary>Gdzie szukać (katalog).</summary>
        public string Where => bossOnly ? $"bossowie od piętra {minFloor + 1}" : $"elity i bossowie od piętra {minFloor + 1}";

        public float P(string key, float fallback = 0f) => param.TryGetValue(key, out var v) ? v : fallback;
        public int PI(string key, int fallback = 0) => param.TryGetValue(key, out var v) ? (int)Math.Round(v) : fallback;

        /// <summary>Wypełnia szablon: <c>{k}</c> – liczba, <c>{k%}</c> – procent, <c>{k s}</c> – sekundy, <c>{k m}</c> – metry.</summary>
        public string Fill(string template)
        {
            if (string.IsNullOrEmpty(template)) return "";
            return Regex.Replace(template, @"\{([a-zA-Z_0-9]+)(%| s| m)?\}", m =>
            {
                string key = m.Groups[1].Value, unit = m.Groups[2].Value;
                if (!param.TryGetValue(key, out var v)) return m.Value;
                switch (unit)
                {
                    case "%": return (v * 100f).ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',') + "%";
                    case " s": return v.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',') + " s";
                    case " m": return v.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',') + " m";
                    default: return v.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
                }
            });
        }

        public string Rule => Fill(rule);
        public string Cost => Fill(cost);

        public override string ToString() => id;
    }
}
