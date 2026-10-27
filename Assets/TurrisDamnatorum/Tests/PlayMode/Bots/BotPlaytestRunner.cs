using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Rozgrywa serię podejść botem danej klasy i osobowości na prawdziwej scenie Main.
    /// Wymaga TURRIS_BOT_DIR (katalog wyników); liczbę podejść ustawia TURRIS_BOT_RUNS (domyślnie 20).
    /// Po każdym podejściu zapisuje JSON serii i odświeża raport klasy (RAPORT_&lt;klasa&gt;.md) ze wszystkich serii w katalogu.
    /// </summary>
    public static class BotPlaytestRunner
    {
        /// <summary>Limit czasu gry na jedno piętro – po nim podejście jest przerywane i zgłaszane jako anomalia.</summary>
        public const float FloorTimeLimit = 360f;

        public static string OutputDir()
        {
            string dir = Environment.GetEnvironmentVariable("TURRIS_BOT_DIR");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_BOT_DIR (i opcjonalnie TURRIS_BOT_RUNS), aby boty rozegrały serię.");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static int RunCount()
        {
            string s = Environment.GetEnvironmentVariable("TURRIS_BOT_RUNS");
            return int.TryParse(s, out int n) && n > 0 ? n : 20;
        }

        static string KindName(RewardKind k) => k == RewardKind.Item ? "przedmiot" : k == RewardKind.Boon ? "wzmocnienie" : k == RewardKind.LearnSpell ? "nowy skill" : "ulepszenie";

        public static IEnumerator RunSeries<TBot>(string classId, BotPersona persona) where TBot : HeroBot
        {
            string dir = OutputDir();
            int runs = RunCount();
            string profile = $"turris_bot_{classId}_{persona}_{Guid.NewGuid():N}.json";
            GameRoot.ProfileFileOverride = profile;
            LogAssert.ignoreFailingMessages = true; // błędy z logu zbiera telemetria – nie przerywają serii
            TBot bot = null;
            try
            {
                SceneManager.LoadScene("Main");
                yield return null;
                yield return null;
                var root = Object.FindAnyObjectByType<GameRoot>();
                Assert.IsNotNull(root, "Scena Main bez GameRoot");
                var cls = root.config.classes.FirstOrDefault(c => c.id == classId);
                Assert.IsNotNull(cls, "Brak klasy " + classId);
                Time.captureFramerate = 60;

                var p = PersonaProfile.For(persona);
                bot = root.Player.gameObject.AddComponent<TBot>();
                bot.Init(root, p, 7919 * (int)persona + classId.Sum(ch => (int)ch));
                var series = new BotSeries
                {
                    className = cls.displayName, classId = classId, persona = persona.ToString(),
                    personaName = p.name, personaDescription = p.description,
                };

                for (int i = 0; i < runs; i++)
                {
                    var plan = new LoadoutPlan { difficulty = root.config.difficulties[0] };
                    root.Meta.SelectClass(plan, cls);
                    var bought = bot.MetaShopping(plan);
                    bot.PreparePlan(plan, i, series.runs);
                    var record = new BotRunRecord
                    {
                        className = cls.displayName, classId = classId, persona = persona.ToString(), run = i + 1,
                        loadout = HeroBot.LoadoutName(plan.Loadout), startSkill = plan.OpeningSkill != null ? plan.OpeningSkill.displayName : "–",
                    };
                    record.metaUnlocks.AddRange(bought);
                    var rec = new BotRunRecorder(root, record);
                    bot.Begin(rec, 1000 * i + (int)persona);
                    Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
                    record.seed = root.Run.seed;
                    yield return null;
                    rec.FloorStarted();

                    string result = null;
                    while (result == null)
                    {
                        switch (root.Screen)
                        {
                            case GameScreen.Playing:
                                if (rec.FloorTime > FloorTimeLimit)
                                {
                                    var left = root.Enemies.Where(e => e != null && !e.IsDead)
                                        .Select(e => $"{e.Def.displayName} [{e.CurrentState}] ({e.transform.position.x:0}; {e.transform.position.y:0}; {e.transform.position.z:0})").Take(4);
                                    record.Note("limit czasu piętra", $"piętro {root.Run.floorIndex + 1} „{root.CurrentFloor?.name}” > {FloorTimeLimit:0} s; pozostali: {string.Join(", ", left)}; w drodze: {root.IncomingEnemies}");
                                    result = "Limit czasu";
                                    root.AbandonRun();
                                }
                                break;
                            case GameScreen.Reward:
                                rec.FloorEnded(true);
                                foreach (var o in root.Rewards) record.rewardsSeen.Add($"{KindName(o.kind)}: {o.Title}");
                                if (root.Rewards.Count > 0)
                                {
                                    int idx = Mathf.Clamp(bot.ChooseReward(root.Rewards), 0, root.Rewards.Count - 1);
                                    record.rewards.Add($"{KindName(root.Rewards[idx].kind)}: {root.Rewards[idx].Title}");
                                    root.ChooseReward(idx);
                                }
                                else root.ChooseReward(0);
                                break;
                            case GameScreen.Intermission:
                                bot.Intermission();
                                root.NextFloor();
                                yield return null;
                                rec.FloorStarted();
                                break;
                            case GameScreen.Victory: result = "Zwycięstwo"; break;
                            case GameScreen.Death: result = "Śmierć"; break;
                            case GameScreen.MainMenu:
                                result = "Przerwane";
                                record.Note("przerwane podejście", $"gra wróciła do menu głównego na piętrze {record.floorReached} (czas piętra {rec.FloorTime:0} s)");
                                Debug.LogWarning("BOT: podejście przerwane – menu główne");
                                break;
                        }
                        if (result == null) yield return null;
                    }

                    rec.Finish(result);
                    rec.Dispose();
                    bot.End();
                    series.runs.Add(record);
                    series.generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    Debug.Log($"BOT {classId}/{persona} #{i + 1}: {result}, piętro {record.floorReached} ({record.floorName}), zabici {record.kills}, " +
                              $"czas gry {record.gameTime:0} s, czas rzeczywisty {record.wallTime:0} s" + (result == "Śmierć" ? $", zabójca {record.killer} – {record.killerAttack}" : ""));
                    File.WriteAllText(Path.Combine(dir, $"{classId}_{persona}.json"), JsonUtility.ToJson(series, true));
                    WriteReport(dir, classId);
                    yield return null;
                }
                Assert.AreEqual(runs, series.runs.Count);
            }
            finally
            {
                if (bot != null) bot.End();
                Time.captureFramerate = 0;
                Time.timeScale = 1f;
                GameRoot.ProfileFileOverride = null;
                LogAssert.ignoreFailingMessages = false;
                string path = Path.Combine(Application.persistentDataPath, profile);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        /// <summary>Raport klasy ze wszystkich serii zapisanych w katalogu (np. tester i gracz z osobnych testów).</summary>
        public static void WriteReport(string dir, string classId)
        {
            var all = new List<BotSeries>();
            foreach (var f in Directory.GetFiles(dir, classId + "_*.json").OrderBy(x => x))
            {
                try { all.Add(JsonUtility.FromJson<BotSeries>(File.ReadAllText(f))); }
                catch (Exception e) { Debug.LogWarning("Nie wczytano " + f + ": " + e.Message); }
            }
            all = all.Where(s => s != null && s.runs != null && s.runs.Count > 0).OrderBy(s => s.persona == BotPersona.Tester.ToString() ? 0 : 1).ToList();
            if (all.Count == 0) return;
            File.WriteAllText(Path.Combine(dir, $"RAPORT_{classId}.md"), BotReport.Build(all));
        }
    }
}
