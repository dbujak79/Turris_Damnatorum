using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Para botów Wojownika: tester QA i gracz H&S/souls-like, po TURRIS_BOT_RUNS podejść (domyślnie 20).
    /// Uruchomienie: TURRIS_BOT_DIR=&lt;katalog&gt; python tools/run_tests.py PlayMode batch Turris.Tests.Bots.WarriorBotPlaytests
    /// Bez TURRIS_BOT_DIR testy są pomijane (nie spowalniają zwykłego przebiegu PlayMode).
    /// </summary>
    public class WarriorBotPlaytests
    {
        const int SeriesTimeout = 48 * 60 * 60 * 1000; // w czasie rzeczywistym (TURRIS_BOT_REALTIME) seria trwa wiele godzin

        [UnityTest, Timeout(SeriesTimeout)]
        public IEnumerator Warrior_Tester() => BotPlaytestRunner.RunSeries<WarriorBot>(WarriorBot.ClassId, BotPersona.Tester);

        [UnityTest, Timeout(SeriesTimeout)]
        public IEnumerator Warrior_SoulsSlasher() => BotPlaytestRunner.RunSeries<WarriorBot>(WarriorBot.ClassId, BotPersona.SoulsSlasher);
    }
}

namespace Turris.Tests.Bots
{
    /// <summary>Przegenerowanie raportów z zapisanych serii (TURRIS_BOT_DIR) bez ponownego grania – po zmianie reguł raportu.</summary>
    public class BotReportRegenerate
    {
        [Test]
        public void Regenerate_FromSavedSeries()
        {
            string dir = BotPlaytestRunner.OutputDir();
            var classes = System.IO.Directory.GetFiles(dir, "*_*.json")
                .Select(f => System.IO.Path.GetFileNameWithoutExtension(f))
                .Select(n => n.Substring(0, n.LastIndexOf('_'))).Distinct().ToList();
            if (classes.Count == 0) Assert.Ignore("Brak zapisanych serii w " + dir);
            foreach (var c in classes) BotPlaytestRunner.WriteReport(dir, c);
        }
    }
}
