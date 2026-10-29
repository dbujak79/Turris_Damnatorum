using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Para botów Maga: tester QA i gracz H&amp;S/souls-like, po TURRIS_BOT_RUNS podejść (domyślnie 20).
    /// Uruchomienie: TURRIS_BOT_DIR=&lt;katalog&gt; python tools/run_tests.py PlayMode batch Turris.Tests.Bots.MageBotPlaytests
    /// Bez TURRIS_BOT_DIR testy są pomijane.
    /// </summary>
    public class MageBotPlaytests
    {
        const int SeriesTimeout = 48 * 60 * 60 * 1000;

        [UnityTest, Timeout(SeriesTimeout)]
        public IEnumerator Mage_Tester() => BotPlaytestRunner.RunSeries<MageBot>(MageBot.ClassId, BotPersona.Tester);

        [UnityTest, Timeout(SeriesTimeout)]
        public IEnumerator Mage_SoulsSlasher() => BotPlaytestRunner.RunSeries<MageBot>(MageBot.ClassId, BotPersona.SoulsSlasher);
    }
}
