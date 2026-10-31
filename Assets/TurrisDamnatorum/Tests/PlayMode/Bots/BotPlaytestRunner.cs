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
        public const float FloorTimeLimit = 600f; // portale otwarte do 300 s (tempo wychodzenia ×0,5) + dobijanie
        /// <summary>Ile prób ma bot na jednym piętrze (po śmierci wraca na początek piętra z tym samym buildem).</summary>
        public const int AttemptsPerFloor = 3;

        public static string OutputDir()
        {
            string dir = Environment.GetEnvironmentVariable("TURRIS_BOT_DIR");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_BOT_DIR (i opcjonalnie TURRIS_BOT_RUNS), aby boty rozegrały serię.");
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// TURRIS_BOT_REALTIME=1: gra w normalnym czasie (bez stałego kroku 1/60 s, który w trybie wsadowym liczy klatki szybciej niż zegar).
        /// Czas gry = czas zegara, klatki jak u gracza (do 60/s) – seria trwa tyle, ile rozgrywka.
        /// </summary>
        public static bool RealTime => Environment.GetEnvironmentVariable("TURRIS_BOT_REALTIME") == "1";

        public static int RunCount()
        {
            string s = Environment.GetEnvironmentVariable("TURRIS_BOT_RUNS");
            return int.TryParse(s, out int n) && n > 0 ? n : 20;
        }


        public static IEnumerator RunSeries<TBot>(string classId, BotPersona persona) where TBot : HeroBot
        {
            string dir = OutputDir();
            int runs = RunCount();
            string profile = $"turris_bot_{classId}_{persona}_{Guid.NewGuid():N}.json";
            GameRoot.ProfileFileOverride = profile;
            EnemyBrain.DebugNoEvasion = false; // boty grają z unikiem demonów HU (testy go wyłączają)
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
                if (RealTime) { Time.captureFramerate = 0; Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0; }
                else Time.captureFramerate = 60;

                var p = PersonaProfile.For(persona);
                bot = root.Player.gameObject.AddComponent<TBot>();
                bot.Init(root, p, 7919 * (int)persona + classId.Sum(ch => (int)ch));
                var series = new BotSeries
                {
                    className = cls.displayName, classId = classId, persona = persona.ToString(),
                    personaName = p.name, personaDescription = p.description,
                };

                // Wyzwania: jedno na serię (bota) – z najdalszego piętra, na którym poległ; poprzednia runda tej osobowości znika.
                foreach (var old in RunSnapshot.ChallengeFiles().Where(f => Path.GetFileName(f).StartsWith($"{cls.id}_{persona}_"))) File.Delete(old);
                RunSnapshot challenge = null;
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
                    // Stan na początku piętra – po śmierci bot powtarza piętro (do AttemptsPerFloor prób), potem zapisuje wyzwanie dla gracza.
                    var floorSnap = RunSnapshot.Capture(root.Run);
                    int floorAttempt = 1;
                    var replay = new BotReplayRecorder(root); // widok z góry ostatecznej porażki na piętrze
                    replay.Begin(record, floorAttempt);

                    string result = null;
                    while (result == null)
                    {
                        switch (root.Screen)
                        {
                            case GameScreen.Playing:
                                replay.Tick();
                                if (rec.FloorTime > FloorTimeLimit)
                                {
                                    var left = root.Enemies.Where(e => e != null && !e.IsDead)
                                        .Select(e => $"{e.Def.displayName} [{e.CurrentState}] ({e.transform.position.x:0}; {e.transform.position.y:0}; {e.transform.position.z:0})").Take(4);
                                    record.Note("limit czasu piętra", $"piętro {root.Run.floorIndex + 1} „{root.CurrentFloor?.name}” > {FloorTimeLimit:0} s; pozostali: {string.Join(", ", left)}; w drodze: {root.IncomingEnemies}");
                                    result = "Limit czasu";
                                    root.AbandonRun();
                                }
                                break;
                            case GameScreen.Intermission:
                                rec.FloorEnded(true); // HU: po piętrze od razu postój ze sklepem (bez darmowej nagrody)
                                bot.Intermission();
                                root.NextFloor();
                                yield return null;
                                rec.FloorStarted();
                                floorSnap = RunSnapshot.Capture(root.Run);
                                floorAttempt = 1;
                                replay.Begin(record, floorAttempt);
                                break;
                            case GameScreen.Victory: result = "Zwycięstwo"; break;
                            case GameScreen.Death:
                                if (floorAttempt < AttemptsPerFloor)
                                {
                                    int fi = floorSnap.floorIndex + 1;
                                    record.floorRetries.Add(fi.ToString());
                                    record.retryNotes.Add($"piętro {fi}, próba {floorAttempt}: {record.killer} – {record.killerAttack}");
                                    rec.FloorEnded(false);
                                    floorAttempt++;
                                    Assert.IsTrue(root.StartRunFromSnapshot(floorSnap), "powtórka piętra z zapisu");
                                    bot.ResumeAfterRetry();
                                    yield return null;
                                    rec.FloorStarted();
                                    replay.Begin(record, floorAttempt);
                                    break;
                                }
                                result = "Śmierć";
                                if (challenge == null || floorSnap.floorIndex >= challenge.floorIndex)
                                    challenge = Challenge(floorSnap, cls, p, record, replay.LastHurt());
                                string replayName = replay.Save(dir, result, $"{record.killer} – {record.killerAttack}",
                                    $"{cls.displayName} ({p.name}), podejście {record.run}: piętro {floorSnap.floorIndex + 1} „{record.floorName}”, próba {floorAttempt}/{AttemptsPerFloor}");
                                if (replayName != null) record.replays.Add(replayName);
                                break;
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
                    replay.Dispose();
                    bot.End();
                    series.runs.Add(record);
                    series.generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    Debug.Log($"BOT {classId}/{persona} #{i + 1}: {result}, piętro {record.floorReached} ({record.floorName}), zabici {record.kills}, " +
                              $"czas gry {record.gameTime:0} s, czas rzeczywisty {record.wallTime:0} s" + (result == "Śmierć" ? $", zabójca {record.killer} – {record.killerAttack}" : ""));
                    File.WriteAllText(Path.Combine(dir, $"{classId}_{persona}.json"), JsonUtility.ToJson(series, true));
                    WriteReport(dir, classId);
                    yield return null;
                }
                if (challenge != null) SaveChallenge(dir, challenge, cls, p);
                Assert.AreEqual(runs, series.runs.Count);
            }
            finally
            {
                if (bot != null) bot.End();
                Time.captureFramerate = 0;
                Application.targetFrameRate = -1;
                Time.timeScale = 1f;
                GameRoot.ProfileFileOverride = null;
                LogAssert.ignoreFailingMessages = false;
                string path = Path.Combine(Application.persistentDataPath, profile);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        /// <summary>
        /// Piętro, którego bot nie przeszedł w <see cref="AttemptsPerFloor"/> próbach: zapis do katalogu wyzwań gry (menu główne →
        /// „Wyzwania botów”) i kopia obok raportu. Jedno wyzwanie na klasę, osobowość i piętro – nowsze zastępuje starsze.
        /// </summary>
        static RunSnapshot Challenge(RunSnapshot floorSnap, ClassDefinition cls, PersonaProfile persona, BotRunRecord record, string lastHit)
        {
            int floor = floorSnap.floorIndex + 1;
            string place = record.floorName;
            int dot = place.IndexOf(". ");
            if (dot > 0 && dot < 6) place = place.Substring(dot + 2); // „IX. Królowa Topieli” → „Królowa Topieli”
            var snap = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(floorSnap));
            lastHit = ShortHit(lastHit);
            snap.title = $"Piętro {floor} · {place}";
            snap.note = $"{cls.displayName} · {persona.name} · poległ {AttemptsPerFloor}× (podejście {record.run})" + (string.IsNullOrEmpty(lastHit) ? "" : $" · ostatni cios: {lastHit}");
            record.challenges.Add($"piętro {floor}");
            return snap;
        }

        /// <summary>„Tytan Otchłani: Tytan Otchłani: Combo – Hit” → „Tytan Otchłani: Combo” (bez wyniku i powtórzonej nazwy).</summary>
        static string ShortHit(string hit)
        {
            if (string.IsNullOrEmpty(hit)) return hit;
            int dash = hit.LastIndexOf(" – ");
            if (dash > 0) hit = hit.Substring(0, dash);
            var parts = hit.Split(new[] { ": " }, System.StringSplitOptions.None).ToList();
            for (int i = parts.Count - 1; i > 0; i--) if (parts[i].StartsWith(parts[i - 1])) parts.RemoveAt(i - 1);
            return string.Join(": ", parts);
        }

        static void SaveChallenge(string dir, RunSnapshot snap, ClassDefinition cls, PersonaProfile persona)
        {
            string name = $"{cls.id}_{persona.persona}_pietro{snap.floorIndex + 1:00}.json";
            snap.Save(Path.Combine(RunSnapshot.ChallengeDir, name));
            snap.Save(Path.Combine(dir, "wyzwania", name));
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
