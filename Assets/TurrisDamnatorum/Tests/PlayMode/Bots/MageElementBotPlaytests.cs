using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Porównanie magów czterech żywiołów: specjalizacje ognia (Meteor), wody (Wir głębin), powietrza (Nawałnica) i ziemi
    /// (Trzęsienie ziemi) – ten sam bot maga, tester i gracz, po TURRIS_BOT_RUNS podejść.
    /// Uruchomienie: TURRIS_BOT_DIR=&lt;katalog&gt; python tools/run_tests.py PlayMode batch Turris.Tests.Bots.MageElementBotPlaytests
    /// </summary>
    public class MageElementBotPlaytests
    {
        const int SeriesTimeout = 48 * 60 * 60 * 1000;

        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Fire_SoulsSlasher() => BotPlaytestRunner.RunSeries<MageBot>("class_hu_mage_fire", BotPersona.SoulsSlasher);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Fire_Tester() => BotPlaytestRunner.RunSeries<MageBot>("class_hu_mage_fire", BotPersona.Tester);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Water_SoulsSlasher() => BotPlaytestRunner.RunSeries<MageBot>("class_hu_mage_water", BotPersona.SoulsSlasher);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Water_Tester() => BotPlaytestRunner.RunSeries<MageBot>("class_hu_mage_water", BotPersona.Tester);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Air_SoulsSlasher() => BotPlaytestRunner.RunSeries<MageBot>("class_hu_mage_air", BotPersona.SoulsSlasher);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Air_Tester() => BotPlaytestRunner.RunSeries<MageBot>("class_hu_mage_air", BotPersona.Tester);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Earth_SoulsSlasher() => BotPlaytestRunner.RunSeries<MageBot>("class_hu_mage_earth", BotPersona.SoulsSlasher);
        [UnityTest, Timeout(SeriesTimeout)] public IEnumerator Earth_Tester() => BotPlaytestRunner.RunSeries<MageBot>("class_hu_mage_earth", BotPersona.Tester);
    }
}
