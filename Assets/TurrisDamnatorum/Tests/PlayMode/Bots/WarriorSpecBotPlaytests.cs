using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Porównanie specjalizacji wojownika: Berserker, Paladyn i Rębajło – ten sam bot wojownika, tester i gracz,
    /// po TURRIS_BOT_RUNS podejść.
    /// Uruchomienie: python tools/run_bots_parallel.py --preset wojownik-specjalizacje --out docs/raporty_botow/&lt;katalog&gt;
    /// </summary>
    public class WarriorSpecBotPlaytests
    {
        const int SeriesTimeout = 48 * 60 * 60 * 1000;

        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Berserker_SoulsSlasher() => BotPlaytestRunner.RunSeries<WarriorBot>("class_hu_berserker", BotPersona.SoulsSlasher);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Berserker_Tester() => BotPlaytestRunner.RunSeries<WarriorBot>("class_hu_berserker", BotPersona.Tester);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Paladin_SoulsSlasher() => BotPlaytestRunner.RunSeries<WarriorBot>("class_hu_paladin", BotPersona.SoulsSlasher);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Paladin_Tester() => BotPlaytestRunner.RunSeries<WarriorBot>("class_hu_paladin", BotPersona.Tester);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Dual_SoulsSlasher() => BotPlaytestRunner.RunSeries<WarriorBot>("class_hu_dual", BotPersona.SoulsSlasher);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Dual_Tester() => BotPlaytestRunner.RunSeries<WarriorBot>("class_hu_dual", BotPersona.Tester);
    }
}
