using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Krótkie, deterministyczne scenariusze botów dla każdej z 30 cech klasowych (rozdz. 15.1): prawdziwa scena Main, pierwsze
    /// piętro, ograniczony czas gry, wymuszony wariant. Zapisuje raport (naciśnięcia, użycia, odmowy, telemetria mechaniki) do
    /// TURRIS_FEATURE_BOTS (katalog) – bez zmiennej test jest pomijany, żeby nie wydłużać zwykłego PlayMode.
    /// To nie jest wielogodzinna seria (te pozostają wstrzymane decyzją autora).
    /// </summary>
    public class ClassFeatureBotScenarios
    {
        const float GameSeconds = 45f;

        static readonly string[] RangedClasses = { "class_rogue", "class_hu_archer", "class_hu_arcane_archer" };

        [UnityTest, Timeout(3 * 60 * 60 * 1000)]
        public IEnumerator AllThirtyFeatures_BotUsesEachInShortScenario()
        {
            string dir = Environment.GetEnvironmentVariable("TURRIS_FEATURE_BOTS");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_FEATURE_BOTS=<katalog>, aby uruchomić scenariusze botów cech.");
            Directory.CreateDirectory(dir);
            GameRoot.ProfileFileOverride = $"turris_feature_bots_{Guid.NewGuid():N}.json";
            EnemyBrain.DebugNoEvasion = false;
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            Assert.IsNotNull(root);
            Time.captureFramerate = 60;
            var rows = new List<string>();
            var unused = new List<string>();
            foreach (var cls in root.config.classes)
                foreach (var def in ClassFeatureCatalog.For(cls))
                {
                    HeroBot bot = RangedClasses.Contains(cls.id) || cls.Family.id == "class_mage" ? (HeroBot)root.Player.gameObject.AddComponent<MageBot>() : root.Player.gameObject.AddComponent<WarriorBot>();
                    var persona = PersonaProfile.For(BotPersona.SoulsSlasher);
                    bot.Init(root, persona, 4242 + def.id.Length);
                    HeroBot.ForcedFeatureId = def.id;
                    var plan = new LoadoutPlan { difficulty = root.config.difficulties[0] };
                    root.Meta.SelectClass(plan, cls);
                    bot.PreparePlan(plan, 0, new List<BotRunRecord>());
                    var record = new BotRunRecord { className = cls.displayName, classId = cls.id, persona = persona.persona.ToString(), run = 1 };
                    var rec = new BotRunRecorder(root, record);
                    bot.Begin(rec, 99);
                    Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
                    yield return null;
                    rec.FloorStarted();
                    float t = 0f;
                    while (t < GameSeconds && root.Screen == GameScreen.Playing) { t += Time.deltaTime; yield return null; }
                    bot.End();
                    rec.Dispose();
                    Object.Destroy(bot);
                    if (root.Screen == GameScreen.Playing) root.AbandonRun();
                    yield return null;
                    string mech = string.Join(", ", Enumerable.Range(0, record.featureStats.k.Count).Select(i => $"{record.featureStats.k[i]}: {record.featureStats.v[i]:0}"));
                    string rej = string.Join(", ", Enumerable.Range(0, record.featureRejections.k.Count).Select(i => $"{record.featureRejections.k[i]} ×{record.featureRejections.v[i]:0}"));
                    rows.Add($"| {cls.displayName} | {def.name} | {record.featurePresses} | {record.featureUses} | {rej} | {mech} |");
                    if (record.featureUses == 0) unused.Add(def.id);
                    Assert.AreEqual(def.id, record.featureId ?? def.id, "bot grał wymuszonym wariantem");
                }
            HeroBot.ForcedFeatureId = null;
            Time.captureFramerate = 0;
            var sb = new StringBuilder();
            sb.AppendLine("# Scenariusze botów – 30 cech klasowych");
            sb.AppendLine();
            sb.AppendLine($"Scena Main, piętro 1, {GameSeconds:0} s gry na wariant, osobowość: gracz H&S/souls. Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}.");
            sb.AppendLine();
            sb.AppendLine("| Klasa | Cecha | Naciśnięcia | Użycia | Odmowy (przyczyna) | Mechanika |");
            sb.AppendLine("|---|---|---|---|---|---|");
            foreach (var r in rows) sb.AppendLine(r);
            sb.AppendLine();
            sb.AppendLine(unused.Count == 0 ? "Każdy wariant został użyty co najmniej raz." : "Bez użycia (polityka bota albo brak warunku – nie dowód słabości): " + string.Join(", ", unused));
            File.WriteAllText(Path.Combine(dir, "RAPORT_CECHY_BOTY.md"), sb.ToString());
            Debug.Log(sb.ToString());
            Assert.LessOrEqual(unused.Count, 6, "większość wariantów używana przez boty: " + string.Join(", ", unused));
        }
    }
}
