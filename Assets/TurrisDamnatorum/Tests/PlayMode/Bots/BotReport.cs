using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Raport z serii podejść botów jednej klasy: tabele zbiorcze i wnioski generowane regułami
    /// w „głosie” danej osobowości (tester – błędy, pokrycie, niezmienniki; gracz – odczucia, uczciwość, tempo).
    /// </summary>
    public static class BotReport
    {
        static readonly CultureInfo Pl = CultureInfo.GetCultureInfo("pl-PL");
        static string F(float v, int dec = 1) => v.ToString("F" + dec, Pl);
        static string Pct(float part, float whole) => whole <= 0 ? "–" : (100f * part / whole).ToString("F0", Pl) + "%";
        static float Avg(IEnumerable<float> v) { var l = v.ToList(); return l.Count == 0 ? 0f : l.Average(); }
        static float Median(IEnumerable<float> v)
        {
            var l = v.OrderBy(x => x).ToList();
            if (l.Count == 0) return 0f;
            return l.Count % 2 == 1 ? l[l.Count / 2] : (l[l.Count / 2 - 1] + l[l.Count / 2]) / 2f;
        }

        public static string Build(IList<BotSeries> series)
        {
            var sb = new StringBuilder();
            string cls = series.Count > 0 ? series[0].className : "?";
            sb.AppendLine($"# Raport botów grających — {cls}");
            sb.AppendLine();
            sb.AppendLine($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, " +
                          "stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) " +
                          "i reagują na początek zamachu z opóźnieniem człowieka.");
            sb.AppendLine();
            foreach (var s in series)
                sb.AppendLine($"- **{s.personaName}** ({s.runs.Count} podejść): {s.personaDescription}");
            sb.AppendLine();

            Summary(sb, series);
            Deaths(sb, series);
            Floors(sb, series);
            Enemies(sb, series);
            Defense(sb, series);
            Dodges(sb, series);
            Loadouts(sb, series);
            Features(sb, series);
            NativePerks(sb, series);
            Skills(sb, series);
            SkillCollection(sb, series);
            ActionsAndEconomy(sb, series);
            Anomalies(sb, series);

            foreach (var s in series)
            {
                sb.AppendLine(s.persona == BotPersona.Tester.ToString() ? "## Wnioski testera" : "## Wnioski gracza (hack and slash / souls-like)");
                sb.AppendLine();
                var lines = s.persona == BotPersona.Tester.ToString() ? TesterConclusions(s) : PlayerConclusions(s);
                foreach (var l in lines) sb.AppendLine("- " + l);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ Tabele

        static void Summary(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Wyniki w skrócie");
            sb.AppendLine();
            sb.AppendLine("| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
            foreach (var s in series)
            {
                var r = s.runs;
                if (r.Count == 0) continue;
                sb.AppendLine($"| {s.personaName} | {r.Count} | {r.Count(x => x.Victory)} ({Pct(r.Count(x => x.Victory), r.Count)}) | {F(Avg(r.Select(x => (float)x.floorReached)))} | " +
                              $"{F(Median(r.Select(x => (float)x.floorReached)))} | {F(Avg(r.Select(x => x.gameTime)) / 60f)} min | {F(Avg(r.Select(x => (float)x.kills)))} | " +
                              $"{F(Avg(r.Select(x => x.damageDealt)), 0)} / {F(Avg(r.Select(x => x.damageTaken)), 0)} | {F(Avg(r.Select(x => (float)x.flasksUsed)))} | {Pct(Avg(r.Select(x => x.minHpFraction)), 1f)} |");
            }
            sb.AppendLine();
            sb.AppendLine("Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):");
            sb.AppendLine();
            foreach (var s in series)
                sb.AppendLine($"- {s.personaName}: " + string.Join(" ", s.runs.Select(x => x.Victory ? "★" : x.result == "Limit czasu" ? "⏱" + x.floorReached : x.floorReached.ToString())));
            sb.AppendLine();
        }

        static void Deaths(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Gdzie i od czego giną");
            sb.AppendLine();
            foreach (var s in series)
            {
                var dead = s.runs.Where(x => x.result == "Śmierć").ToList();
                if (dead.Count == 0) { sb.AppendLine($"- {s.personaName}: brak śmierci."); continue; }
                var byFloor = dead.GroupBy(x => $"{x.floorReached}. {x.floorName}").OrderBy(g => g.Key).Select(g => $"{g.Key} ×{g.Count()}");
                var killers = dead.GroupBy(x => $"{x.killer} – „{x.killerAttack}”").OrderByDescending(g => g.Count()).Take(6).Select(g => $"{g.Key} ×{g.Count()}");
                float oneShot = dead.Count(x => x.maxHealth > 0 && x.killerHitDamage >= 0.4f * x.maxHealth);
                sb.AppendLine($"- **{s.personaName}** — piętra: {string.Join(", ", byFloor)}.");
                sb.AppendLine($"  Ostatni cios: {string.Join("; ", killers)}. Śmierć ciosem ≥40% maks. HP: {oneShot}/{dead.Count}. " +
                              $"Zginął mając flaszki: {dead.Count(x => x.flasksLeft > 0)}/{dead.Count}.");
                sb.AppendLine($"  Ostatnie 3 s przed śmiercią: średnio {Pct(Avg(dead.Select(x => x.deathBurst3s)), 1)} maks. HP straconych " +
                              $"(śmierć z ≥50% HP w 3 s: {dead.Count(x => x.deathBurst3s >= 0.5f)}/{dead.Count}). " +
                              $"Picie: {s.runs.Sum(x => x.flaskAttempts)} prób, {s.runs.Sum(x => x.flasksUsed)} wypitych ({Pct(s.runs.Sum(x => x.flaskAttempts) - s.runs.Sum(x => x.flasksUsed), s.runs.Sum(x => x.flaskAttempts))} przerwanych trafieniem).");
                var low = Tally.Merge(s.runs.Select(x => x.lowHpTime));
                if (low.Sum > 0f)
                    sb.AppendLine($"  Niskie życie z flaszką w zapasie: {F(low.Sum / s.runs.Count)} s na podejście – " +
                                  string.Join(", ", low.Items.OrderByDescending(x => x.val).Select(x => $"{x.key} {Pct(x.val, low.Sum)}")) + ".");
            }
            sb.AppendLine();
        }

        static void Floors(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Piętra");
            sb.AppendLine();
            sb.AppendLine("| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|");
            foreach (var s in series)
            {
                var fl = s.runs.SelectMany(r => r.floors).GroupBy(f => f.index).OrderBy(g => g.Key);
                foreach (var g in fl)
                {
                    var cleared = g.Where(f => f.cleared).ToList();
                    sb.AppendLine($"| {s.personaName} | {g.Key + 1}. {g.First().name} | {g.Count()} | {cleared.Count} | {F(Avg(cleared.Select(f => f.time)), 0)} s | " +
                                  $"{F(Avg(g.Select(f => f.damageTaken)), 0)} | {F(Avg(g.Select(f => (float)f.flasks)))} | {F(Avg(g.Select(f => (float)f.kills)))} |");
                }
            }
            sb.AppendLine();
            sb.AppendLine("Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.");
            sb.AppendLine();
            foreach (var s in series)
            {
                var retries = Tally.Merge(s.runs.Select(r => r.floorRetries));
                var ch = s.runs.SelectMany(r => r.challenges).ToList();
                if (retries.Sum > 0)
                    sb.AppendLine($"**{s.personaName}** – ponowne próby: " + string.Join(", ", retries.Items.OrderBy(x => int.Parse(x.key)).Select(x => $"piętro {x.key} ×{x.val}")) +
                                  $"; wyzwań (3 porażki na piętrze): {ch.Count}" + (ch.Count > 0 ? $" – {string.Join(", ", ch.Distinct())}" : "") + ".");
            }
            sb.AppendLine();
        }

        static void Enemies(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Przeciwnicy (obie osobowości łącznie)");
            sb.AppendLine();
            var runs = series.SelectMany(s => s.runs).ToList();
            var kills = Tally.Merge(runs.Select(r => r.enemyKills));
            var ttk = Tally.Merge(runs.Select(r => r.enemyTtkSum));
            var eng = Tally.Merge(runs.Select(r => r.enemyEngagedKills));
            var hits = Tally.Merge(runs.Select(r => r.enemyHitsSum));
            var dmg = Tally.Merge(runs.Select(r => r.enemyDamageToPlayer));
            var onPlayer = Tally.Merge(runs.Select(r => r.enemyHitsOnPlayer));
            var deaths = runs.Where(r => r.result == "Śmierć").GroupBy(r => r.killer).ToDictionary(g => g.Key ?? "?", g => g.Count());
            sb.AppendLine("| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|");
            var names = kills.k.Union(dmg.k).Distinct().OrderByDescending(n => dmg.Get(n));
            foreach (var n in names)
            {
                string baseName = n.Replace(" (elita)", "");
                float k = kills.Get(n);
                float kAll = kills.Get(baseName) + kills.Get(baseName + " (elita)");
                bool elite = n.EndsWith("(elita)");
                // Obrażenia/trafienia od wroga zapisane są po nazwie bazowej – elity wliczone.
                string dmgCell = elite ? "↑" : F(dmg.Get(n), 0);
                string perKill = elite ? "↑" : kAll > 0 ? F(dmg.Get(n) / kAll, 0) : "–";
                sb.AppendLine($"| {n} | {k} | {(eng.Get(n) > 0 ? F(ttk.Get(n) / eng.Get(n)) + " s" : "–")} | {(eng.Get(n) > 0 ? F(hits.Get(n) / eng.Get(n)) : "–")} | {(elite ? "↑" : onPlayer.Get(n).ToString(Pl))} | " +
                              $"{dmgCell} | {perKill} | {(elite ? "" : deaths.TryGetValue(n, out var d) ? d.ToString() : "0")} |");
            }
            sb.AppendLine();
        }

        static void Defense(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Obrona: nadchodzące ciosy");
            sb.AppendLine();
            string[] kinds = { "zwykły", "ciężki", "nieparowalny", "nieblokowalny", "obszarowy" };
            string[] reactKinds = { "zwykły", "ciężki", "nieparowalny", "nieblokowalny", "obszarowy", "pocisk" };
            string[] outs = { "trafienie", "blok", "parowanie", "unik", "przełamana garda" };
            foreach (var s in series)
            {
                var inc = Tally.Merge(s.runs.Select(r => r.incoming));
                sb.AppendLine($"**{s.personaName}** — wynik ciosu wg koloru telegrafu:");
                sb.AppendLine();
                sb.AppendLine("| Telegraf | Razem | " + string.Join(" | ", outs) + " |");
                sb.AppendLine("|---|---|" + string.Join("", outs.Select(_ => "---|")));
                foreach (var kd in kinds)
                {
                    float tot = outs.Sum(o => inc.Get($"{kd}|{o}"));
                    if (tot <= 0) continue;
                    sb.AppendLine($"| {kd} | {tot} | " + string.Join(" | ", outs.Select(o => Pct(inc.Get($"{kd}|{o}"), tot))) + " |");
                }
                sb.AppendLine();
            }

            sb.AppendLine("**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:");
            sb.AppendLine();
            sb.AppendLine("| Osobowość (reakcja) | Telegraf | Decyzji | " + string.Join(" | ", HeroBot.MarginBuckets) + " |");
            sb.AppendLine("|---|---|---|" + string.Join("", HeroBot.MarginBuckets.Select(_ => "---|")));
            foreach (var s in series)
            {
                var m = Tally.Merge(s.runs.Select(r => r.reactionMargin));
                var p = PersonaProfile.For(s.persona == BotPersona.Tester.ToString() ? BotPersona.Tester : BotPersona.SoulsSlasher);
                foreach (var kd in reactKinds)
                {
                    float tot = HeroBot.MarginBuckets.Sum(b => m.Get($"{kd}|{b}"));
                    if (tot <= 0) continue;
                    sb.AppendLine($"| {s.personaName} ({F(p.reaction, 2)} s) | {kd} | {tot} | " + string.Join(" | ", HeroBot.MarginBuckets.Select(b => Pct(m.Get($"{kd}|{b}"), tot))) + " |");
                }
            }
            sb.AppendLine();
            var wsum = Tally.Merge(series.SelectMany(s => s.runs).Select(r => r.windupSum));
            var wn = Tally.Merge(series.SelectMany(s => s.runs).Select(r => r.windupN));
            var fastest = wn.Items.Where(x => x.val >= 5).Select(x => (x.key, wsum.Get(x.key) / x.val, x.val)).OrderBy(x => x.Item2).Take(10).ToList();
            if (fastest.Count > 0)
            {
                sb.AppendLine("Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):");
                sb.AppendLine();
                sb.AppendLine("| Wróg: atak | Śr. zamach | Obserwacji |");
                sb.AppendLine("|---|---|---|");
                foreach (var (k, w, c) in fastest) sb.AppendLine($"| {k} | {F(w, 2)} s | {c} |");
                sb.AppendLine();
            }

            var tester = series.FirstOrDefault(s => s.persona == BotPersona.Tester.ToString());
            if (tester != null)
            {
                var resp = Tally.Merge(tester.runs.Select(r => r.responses));
                sb.AppendLine("**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):");
                sb.AppendLine();
                var respNames = resp.k.Select(x => x.Split('|')[0]).Distinct().OrderBy(x => x).ToList();
                sb.AppendLine("| Reakcja | " + string.Join(" | ", reactKinds) + " |");
                sb.AppendLine("|---|" + string.Join("", reactKinds.Select(_ => "---|")));
                foreach (var rn in respNames)
                {
                    var cells = reactKinds.Select(kd =>
                    {
                        float tot = outs.Sum(o => resp.Get($"{rn}|{kd}|{o}"));
                        if (tot <= 0) return "–";
                        float hurt = resp.Get($"{rn}|{kd}|trafienie") + resp.Get($"{rn}|{kd}|przełamana garda");
                        return $"{Pct(hurt, tot)} (n={tot})";
                    });
                    sb.AppendLine($"| {rn} | " + string.Join(" | ", cells) + " |");
                }
                sb.AppendLine();
            }
        }

        /// <summary>Skuteczność uniku: odsetek ciosów, przed którymi bot zrobił unik i które dotarły, zakończonych wynikiem „unik”.</summary>
        static (float ok, float tot) DodgeRate(Tally resp, string kind)
        {
            float tot = 0, ok = 0;
            foreach (var (k, v) in resp.Items)
            {
                var p = k.Split('|');
                if (p.Length != 3 || !p[0].StartsWith("unik") || (kind != null && p[1] != kind)) continue;
                tot += v;
                if (p[2] == "unik") ok += v;
            }
            return (ok, tot);
        }

        static void Dodges(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Uniki");
            sb.AppendLine();
            sb.AppendLine("Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: " +
                          "odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). " +
                          "Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.");
            sb.AppendLine();
            sb.AppendLine("| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var s in series)
            {
                int n = Math.Max(1, s.runs.Count);
                var acts = Tally.Merge(s.runs.Select(r => r.actions));
                var why = Tally.Merge(s.runs.Select(r => r.dodgeReasons));
                var resp = Tally.Merge(s.runs.Select(r => r.responses));
                string reasons = string.Join(", ", why.Items.Where(x => !x.key.StartsWith("odmowa")).OrderByDescending(x => x.val).Select(x => $"{x.key} {F(x.val / n)}"));
                string refusals = string.Join(", ", why.Items.Where(x => x.key.StartsWith("odmowa")).OrderByDescending(x => x.val)
                    .Select(x => $"{x.key.Replace("odmowa: ", "").Replace("Flask", "picie flaszki").Replace("unik", "w trakcie uniku")} {F(x.val / n)}"));
                float Hurt(bool shot) => resp.Items.Where(x =>
                {
                    var p = x.key.Split('|');
                    return p.Length == 3 && p[0].StartsWith("unik") && (p[1] == "pocisk") == shot && (p[2] == "trafienie" || p[2] == "przełamana garda");
                }).Sum(x => x.val);
                string Eff(float pressed, float hurt) => pressed < 5 ? "–" : $"{Pct(Math.Max(0f, pressed - hurt), pressed)} (n={pressed})";
                sb.AppendLine($"| {s.personaName} | {F(acts.Get("unik") / n)} | {(reasons.Length > 0 ? reasons : "–")} | {(refusals.Length > 0 ? refusals : "0")} | " +
                              $"{Eff(why.Get("reakcja na cios wręcz"), Hurt(false))} / {Eff(why.Get("unik przed pociskiem"), Hurt(true))} |");
            }
            sb.AppendLine();
        }

        static void Loadouts(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Zestawy broni i skille startowe");
            sb.AppendLine();
            sb.AppendLine("| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |");
            sb.AppendLine("|---|---|---|---|---|---|---|");
            foreach (var s in series)
                foreach (var g in s.runs.GroupBy(r => (r.loadout, r.startSkill)).OrderByDescending(g => g.Average(r => r.floorReached)))
                    sb.AppendLine($"| {s.personaName} | {g.Key.loadout} | {g.Key.startSkill} | {g.Count()} | {g.Count(r => r.Victory)} | {F((float)g.Average(r => r.floorReached))} | {F((float)g.Average(r => r.kills))} |");
            sb.AppendLine();
        }

        /// <summary>Cechy klasowe: wynik mechaniki (zasoby i zdarzenia z telemetrii cechy) osobno od jakości polityki (naciśnięcia, odmowy).</summary>
        static void Features(StringBuilder sb, IList<BotSeries> series)
        {
            var runs = series.SelectMany(s => s.runs.Select(r => (s.personaName, r))).Where(x => !string.IsNullOrEmpty(x.r.featureId)).ToList();
            if (runs.Count == 0) return;
            sb.AppendLine("## Cechy klasowe");
            sb.AppendLine();
            sb.AppendLine("| Osobowość | Cecha | Podejścia | Śr. piętro | Naciśnięcia | Użycia | Odmowy (przyczyna) | Mechanika (suma) |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|");
            foreach (var g in runs.GroupBy(x => (x.personaName, x.r.featureId)))
            {
                var all = g.Select(x => x.r).ToList();
                var rej = new Tally(); var mech = new Tally();
                foreach (var r in all) { for (int i = 0; i < r.featureRejections.k.Count; i++) rej.Add(r.featureRejections.k[i], r.featureRejections.v[i]); for (int i = 0; i < r.featureStats.k.Count; i++) mech.Add(r.featureStats.k[i], r.featureStats.v[i]); }
                string rs = string.Join(", ", Enumerable.Range(0, rej.k.Count).Select(i => $"{rej.k[i]} ×{rej.v[i]:0}"));
                string ms = string.Join(", ", Enumerable.Range(0, mech.k.Count).Select(i => $"{mech.k[i]}: {mech.v[i]:0}"));
                sb.AppendLine($"| {g.Key.personaName} | {ClassFeatureCatalog.Get(g.Key.featureId)?.name ?? g.Key.featureId} | {all.Count} | {F((float)all.Average(r => r.floorReached))} | {all.Sum(r => r.featurePresses)} | {all.Sum(r => r.featureUses)} | {rs} | {ms} |");
            }
            sb.AppendLine();
            sb.AppendLine("Brak użyć przy wielu naciśnięciach to sygnał słabej polityki bota albo braku warunku (odmowy z przyczyną), nie dowód słabości cechy.");
            sb.AppendLine();
        }

        /// <summary>Perki specjalizacyjne: zakupy i działanie mechaniki (tempo, okna wyłomu, strefy manifestacji).</summary>
        static void NativePerks(StringBuilder sb, IList<BotSeries> series)
        {
            var names = NativePerkCatalog.Ids.Select(NativePerkCatalog.Name).ToList();
            var rows = series.Select(s =>
            {
                var buys = new Tally(); var mech = new Tally();
                foreach (var r in s.runs)
                {
                    for (int i = 0; i < r.purchases.k.Count; i++) foreach (var n in names) if (r.purchases.k[i] == "sklep: " + n) buys.Add(n, r.purchases.v[i]);
                    for (int i = 0; i < r.perkStats.k.Count; i++) mech.Add(r.perkStats.k[i], r.perkStats.v[i]);
                }
                return (s.personaName, buys, mech);
            }).Where(x => x.buys.k.Count > 0 || x.mech.k.Count > 0).ToList();
            if (rows.Count == 0) return;
            sb.AppendLine("## Perki specjalizacyjne");
            sb.AppendLine();
            sb.AppendLine("| Osobowość | Zakupy | Mechanika (suma) |");
            sb.AppendLine("|---|---|---|");
            foreach (var x in rows)
                sb.AppendLine($"| {x.personaName} | {string.Join(", ", Enumerable.Range(0, x.buys.k.Count).Select(i => $"{x.buys.k[i]} ×{x.buys.v[i]:0}"))} | {string.Join(", ", Enumerable.Range(0, x.mech.k.Count).Select(i => $"{x.mech.k[i]}: {x.mech.v[i]:0.##}"))} |");
            sb.AppendLine();
            // Audyt Gwałtownej manifestacji: ile dałaby kompresja przy tym samym zachowaniu wrogów (strefy zwykłe).
            foreach (var x in rows)
            {
                float Get(string key) { int i = x.mech.k.IndexOf(key); return i < 0 ? 0f : x.mech.v[i]; }
                float hits = Get("strefy zwykłe: trafienia celów"), first = Get("strefy zwykłe: trafienia w 1. połowie"), n = Get("strefy zwykłe: liczba");
                if (n > 0 && hits > 0)
                    sb.AppendLine($"- {x.personaName}: {n:0} kwalifikowanych stref, trafienia celów {hits:0}, w pierwszej połowie {first:0} ({first / hits:P0}) → szacowany zysk Manifestacji ×{Mathf.Min(2f, 2f * first / hits):0.00} realizacji budżetu.");
            }
            sb.AppendLine();
        }

        static void Skills(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Umiejętności i źródła obrażeń");
            sb.AppendLine();
            foreach (var s in series)
            {
                var uses = Tally.Merge(s.runs.Select(r => r.skillUses));
                var src = Tally.Merge(s.runs.Select(r => r.damageBySource));
                float total = src.Sum;
                sb.AppendLine($"**{s.personaName}** — źródła obrażeń (top 10):");
                sb.AppendLine();
                sb.AppendLine("| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |");
                sb.AppendLine("|---|---|---|---|---|");
                foreach (var (k, v) in src.Items.OrderByDescending(x => x.val).Take(10))
                {
                    float u = uses.Get(k);
                    sb.AppendLine($"| {k} | {F(v, 0)} | {Pct(v, total)} | {(u > 0 ? u.ToString(Pl) : "")} | {(u > 0 ? F(v / u, 0) : "")} |");
                }
                var unusedDmg = uses.Items.Where(x => src.Get(x.key) <= 0).Select(x => $"{x.key} ×{x.val}").ToList();
                if (unusedDmg.Count > 0) sb.AppendLine($"\nUżyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): {string.Join(", ", unusedDmg)}.");
                sb.AppendLine();
            }
        }

        static readonly string[] Origins = { "startowy", "odblokowanie", "zestaw", "klasa", "nagroda", "sklep", "przedmiot" };

        /// <summary>
        /// Różnorodność skilli: ile różnych skilli bohater używa, skąd pochodzą użycia (start, odblokowanie, nagroda, sklep),
        /// oferty sklepu, zamiany w slotach i porównanie skuteczności skilli startowych z dokupionymi w trakcie podejścia.
        /// </summary>
        static void SkillCollection(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Kolekcja skilli: różnorodność, sklep, zamiany");
            sb.AppendLine();
            sb.AppendLine("Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, " +
                          "**zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, " +
                          "**sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.");
            sb.AppendLine();
            foreach (var s in series)
            {
                int n = Math.Max(1, s.runs.Count);
                var distinct = s.runs.Select(r => (float)r.skillUses.Items.Count(x => x.val > 0)).ToList();
                // Na podejście: pochodzenie każdego skilla z jego użyć w tym podejściu – obrażenia i poziomy przypisane dokładnie.
                var byOrigin = new Dictionary<string, (float uses, float dmg, float lvl)>();
                var bySkill = new Dictionary<string, (string orig, float runs, float uses, float dmg, float lvl)>();
                foreach (var r in s.runs)
                    foreach (var (key, uses) in r.skillUsesByOrigin.Items)
                    {
                        int bar = key.LastIndexOf('|');
                        string name = key.Substring(0, bar), orig = key.Substring(bar + 1);
                        float share = uses / Math.Max(1f, r.skillUses.Get(name));
                        float dmg = r.damageBySource.Get(name) * share, lvl = r.skillLevelSum.Get(name) * share;
                        byOrigin.TryGetValue(orig, out var o);
                        byOrigin[orig] = (o.uses + uses, o.dmg + dmg, o.lvl + lvl);
                        bySkill.TryGetValue(name, out var k);
                        bySkill[name] = (k.orig == null || k.uses < uses ? orig : k.orig, k.runs + 1, k.uses + uses, k.dmg + dmg, k.lvl + lvl);
                    }
                float allUses = byOrigin.Values.Sum(x => x.uses);
                sb.AppendLine($"**{s.personaName}** — różnych skilli użytych na podejście: średnio {F(Avg(distinct))}, mediana {F(Median(distinct))}, " +
                              $"najwięcej {(distinct.Count > 0 ? distinct.Max() : 0):0} (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).");
                sb.AppendLine();
                sb.AppendLine("| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |");
                sb.AppendLine("|---|---|---|---|---|");
                foreach (var o in Origins.Concat(byOrigin.Keys.Except(Origins)))
                {
                    if (!byOrigin.TryGetValue(o, out var v) || v.uses <= 0) continue;
                    sb.AppendLine($"| {o} | {Pct(v.uses, allUses)} | {F(v.uses / n)} | {(v.dmg > 0 ? F(v.dmg / v.uses, 0) : "–")} | {F(v.lvl / v.uses)} |");
                }
                sb.AppendLine();
                var offers = Tally.Merge(s.runs.Select(r => r.shopOffers));
                if (offers.Sum > 0)
                    sb.AppendLine($"Oferty sklepu dusz: widziane {offers.Get("widziana")}, kupione {offers.Get("kupiona")}, odrzucone {offers.Get("odrzucona")}, " +
                                  $"za mało dusz {offers.Get("za mało dusz")}.");
                var swaps = Tally.Merge(s.runs.Select(r => r.skillSwaps));
                if (swaps.Sum > 0)
                    sb.AppendLine($"Zamiany w slotach: {F(swaps.Sum / n)} na podejście; najczęstsze: " +
                                  string.Join(", ", swaps.Items.OrderByDescending(x => x.val).Take(6).Select(x => $"{x.key} ×{x.val}")) + ".");
                sb.AppendLine();
                sb.AppendLine("| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |");
                sb.AppendLine("|---|---|---|---|---|---|");
                foreach (var (name, v) in bySkill.OrderByDescending(x => x.Value.uses))
                    sb.AppendLine($"| {name} | {v.orig} | {v.runs} | {F(v.uses / Math.Max(1f, v.runs))} | {(v.dmg > 0 ? F(v.dmg / v.uses, 0) : "–")} | {F(v.lvl / Math.Max(1f, v.uses))} |");
                sb.AppendLine();
            }
        }

        static void ActionsAndEconomy(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Akcje, nagrody, zakupy");
            sb.AppendLine();
            foreach (var s in series)
            {
                int n = Math.Max(1, s.runs.Count);
                var acts = Tally.Merge(s.runs.Select(r => r.actions));
                sb.AppendLine($"**{s.personaName}** — akcje na podejście: " + string.Join(", ", acts.Items.OrderByDescending(x => x.val).Select(x => $"{x.key} {F(x.val / n)}")) + ".");
                sb.AppendLine($"Czas w bloku {F(Avg(s.runs.Select(r => r.timeBlocking)), 0)} s, w biegu {F(Avg(s.runs.Select(r => r.timeSprinting)), 0)} s, " +
                              $"bez wytrzymałości (<10%) {F(Avg(s.runs.Select(r => r.timeLowStamina)), 0)} s na podejście.");
                var buy = Tally.Merge(s.runs.Select(r => r.purchases));
                if (buy.k.Count > 0) sb.AppendLine("Zakupy: " + string.Join(", ", buy.Items.OrderByDescending(x => x.val).Take(8).Select(x => $"{x.key} ×{x.val}")) + ".");
                var attr = Tally.Merge(s.runs.Select(r => r.attributes));
                if (attr.k.Count > 0) sb.AppendLine("Punkty cech: " + string.Join(", ", attr.Items.Select(x => $"{x.key} {x.val}")) + ".");
                var seals = Tally.Merge(s.runs.Select(r => r.seals));
                if (seals.k.Count > 0)
                {
                    float Kind(string k) => seals.Items.Where(x => x.key.StartsWith(k)).Sum(x => x.val);
                    sb.AppendLine($"Pieczęcie na podejście: z daru bossa {F(Kind("dar") / n)}, z sakwy do skilli {F(Kind("z sakwy") / n)}, kupione {F(Kind("kupiona") / n)}; " +
                                  "najczęstsze: " + string.Join(", ", seals.Items.OrderByDescending(x => x.val).Take(6).Select(x => $"{x.key.Substring(x.key.IndexOf(':') + 2)} ×{x.val}")) + ".");
                }
                var meta = s.runs.SelectMany(r => r.metaUnlocks).ToList();
                if (meta.Count > 0) sb.AppendLine("Odblokowania za popiół między podejściami: " + string.Join(", ", meta) + ".");
                var msg = Tally.Merge(s.runs.Select(r => r.messages));
                if (msg.k.Count > 0) sb.AppendLine("Komunikaty gry: " + string.Join("; ", msg.Items.OrderByDescending(x => x.val).Take(8).Select(x => $"„{x.key}” ×{x.val}")) + ".");
                sb.AppendLine();
            }
        }

        static void Anomalies(StringBuilder sb, IList<BotSeries> series)
        {
            sb.AppendLine("## Anomalie i niezmienniki");
            sb.AppendLine();
            foreach (var s in series)
            {
                var an = Tally.Merge(s.runs.Select(r => r.anomalies));
                if (an.k.Count == 0) { sb.AppendLine($"- {s.personaName}: brak."); continue; }
                sb.AppendLine($"- **{s.personaName}**: " + string.Join(", ", an.Items.OrderByDescending(x => x.val).Select(x => $"{x.key} ×{x.val}")) + ".");
                foreach (var note in s.runs.SelectMany(r => r.anomalyNotes).GroupBy(x => x).OrderByDescending(g => g.Count()).Take(12))
                    sb.AppendLine($"  - {note.Key}" + (note.Count() > 1 ? $" (w {note.Count()} podejściach)" : ""));
            }
            sb.AppendLine();
        }

        // ------------------------------------------------------------------ Wnioski

        static float HurtRate(Tally t, string kindFilter = null, string respFilter = null)
        {
            float tot = 0, hurt = 0;
            foreach (var (k, v) in t.Items)
            {
                var p = k.Split('|');
                if (respFilter != null && p.Length == 3 && !p[0].StartsWith(respFilter)) continue;
                string kd = p.Length == 3 ? p[1] : p[0], oc = p[p.Length - 1];
                if (kindFilter != null && kd != kindFilter) continue;
                tot += v;
                if (oc == "trafienie" || oc == "przełamana garda") hurt += v;
            }
            return tot <= 0 ? -1f : hurt / tot;
        }

        static float Count(Tally t, string kindFilter, string respFilter)
        {
            float tot = 0;
            foreach (var (k, v) in t.Items)
            {
                var p = k.Split('|');
                if (p.Length == 3 && p[0].StartsWith(respFilter) && p[1] == kindFilter) tot += v;
            }
            return tot;
        }

        public static List<string> TesterConclusions(BotSeries s)
        {
            var l = new List<string>();
            var r = s.runs;
            int n = Math.Max(1, r.Count);
            var an = Tally.Merge(r.Select(x => x.anomalies));
            var notes = r.SelectMany(x => x.anomalyNotes).GroupBy(x => x).OrderByDescending(g => g.Count()).ToList();

            float errors = an.Get("wyjątek w logu") + an.Get("błąd w logu");
            if (errors > 0)
                l.Add($"**BŁĄD (P1):** {errors} wpisów błędów/wyjątków w logu podczas gry, m.in.: " +
                      string.Join("; ", notes.Where(g => r.Any(x => x.anomalyNotes.Contains(g.Key))).Select(g => g.Key).Where(k => !k.Contains("pozycja") && !k.Contains("brak postępu") && !k.Contains("„")).Take(3)) + ".");
            else l.Add("Log czysty w podejściach testera: ani jednego błędu ani wyjątku w " + r.Count + " podejściach (błędy z serii gracza – w sekcji anomalii).");

            foreach (var rule in new[] { "reguła: zablokowano nieblokowalny", "reguła: unik przed obszarowym", "reguła: sparowano nieparowalny", "obrażenia NaN/ujemne", "bohater poza światem" })
                if (an.Get(rule) > 0) l.Add($"**BŁĄD (P1):** naruszenie niezmiennika „{rule}” ×{an.Get(rule)}.");
            if (new[] { "reguła: zablokowano nieblokowalny", "reguła: unik przed obszarowym", "reguła: sparowano nieparowalny" }.All(x => an.Get(x) <= 0))
                l.Add("Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.");

            float timeouts = r.Count(x => x.result == "Limit czasu");
            if (timeouts > 0)
                l.Add($"**Problem (P2):** {timeouts} podejść przerwanych limitem czasu piętra – możliwy nieosiągalny wróg albo zbyt długie piętro (zob. anomalie).");

            float stuck = r.Sum(x => x.stuckEvents);
            if (stuck / n > 0.5f)
            {
                var where = notes.Where(g => g.Key.Contains("brak postępu")).Take(3).Select(g => g.Key);
                l.Add($"**Problem (P2):** zablokowany ruch średnio {F(stuck / n)}× na podejście (geometria areny/kolizje). Przykłady: {string.Join("; ", where)}.");
            }
            else if (stuck > 0) l.Add($"Pojedyncze zatrzymania na geometrii ({stuck} razy) – do przejrzenia, nie blokują gry.");

            float parryMiss = an.Get("parowanie w oknie nie zadziałało");
            if (parryMiss > 0)
                l.Add($"**Do sprawdzenia (P3):** {parryMiss}× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – " +
                      "czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: " + notes.FirstOrDefault(g => g.Key.Contains("oknie parowania"))?.Key);
            float guardMiss = an.Get("garda nie zatrzymała ciosu");
            if (guardMiss > 0)
                l.Add($"**Do sprawdzenia (P3):** {guardMiss}× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: " + notes.FirstOrDefault(g => g.Key.Contains("przez gardę"))?.Key);

            var inc = Tally.Merge(r.Select(x => x.incoming));
            float hitsTotal = inc.Items.Where(x => x.key.EndsWith("|trafienie")).Sum(x => x.val);
            float behind = an.Get("trafienie z tyłu (poza polem widzenia)");
            if (hitsTotal > 0 && behind / hitsTotal > 0.15f)
                l.Add($"**Czytelność (P2):** {Pct(behind, hitsTotal)} trafień w bohatera przyszło z tyłu (>110° od kierunku patrzenia). W grupach brakuje sygnału ataku spoza kadru.");

            var resp = Tally.Merge(r.Select(x => x.responses));
            void Rate(string resp0, string kind, string label, float warnAbove)
            {
                float rate = HurtRate(resp, kind, resp0);
                float cnt = Count(resp, kind, resp0);
                if (rate < 0 || cnt < 5) return;
                l.Add(rate > warnAbove
                    ? $"{label}: {Pct(rate, 1)} ciosów mimo to rani (n={cnt}) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki."
                    : $"{label}: działa – rani tylko {Pct(rate, 1)} ciosów (n={cnt}).");
            }
            Rate("parowanie", "zwykły", "Parowanie zwykłych ciosów", 0.5f);
            Rate("unik", "nieblokowalny", "Unik przed czerwonym (nieblokowalnym)", 0.4f);
            Rate("unik", "ciężki", "Unik przed ciężkim", 0.4f);
            Rate("blok", "zwykły", "Blok zwykłych ciosów", 0.25f);
            Rate("wyjście z zasięgu", "obszarowy", "Wychodzenie z różowego kręgu", 0.4f);
            {
                var why = Tally.Merge(r.Select(x => x.dodgeReasons));
                float pressed = why.Get("unik przed pociskiem");
                float hurt = resp.Items.Where(x => x.key.StartsWith("unik") && x.key.Contains("|pocisk|") && (x.key.EndsWith("|trafienie") || x.key.EndsWith("|przełamana garda"))).Sum(x => x.val);
                if (pressed >= 10)
                    l.Add(hurt / pressed > 0.4f
                        ? $"**Unik przed pociskiem (P2):** po {Pct(hurt, pressed)} uników i tak trafia (n={pressed}) – przy reakcji 0,3 s pocisk jest za szybki albo okno niewrażliwości za krótkie."
                        : $"Unik przed pociskiem działa: trafia tylko {Pct(hurt, pressed)} pocisków, przed którymi był unik (n={pressed}).");
            }
            Rate("blok", "pocisk", "Blok pocisku", 0.3f);
            float noneZw = HurtRate(resp, "zwykły", "brak reakcji");
            if (noneZw >= 0 && noneZw < 0.6f) l.Add($"Kontrola: bez reakcji zwykły cios rani tylko w {Pct(noneZw, 1)} przypadków – część ataków wrogów pudłuje sama (tracking/zasięg), co osłabia karę za brak obrony.");

            var marg = Tally.Merge(r.Select(x => x.reactionMargin));
            foreach (var kd in new[] { "zwykły", "ciężki", "nieparowalny", "nieblokowalny", "obszarowy" })
            {
                float tot = HeroBot.MarginBuckets.Sum(b => marg.Get($"{kd}|{b}"));
                float late = marg.Get($"{kd}|{HeroBot.MarginBuckets[0]}") + marg.Get($"{kd}|{HeroBot.MarginBuckets[1]}");
                if (tot < 10) continue;
                if (late / tot > 0.25f)
                    l.Add($"**Czytelność (P2):** telegraf „{kd}”: przy reakcji {F(PersonaProfile.For(BotPersona.Tester).reaction, 2)} s {Pct(late, tot)} decyzji zapada, gdy do trafienia zostało <0,1 s (n={tot}) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).");
                else l.Add($"Telegraf „{kd}” daje czas: tylko {Pct(late, tot)} decyzji zapada mniej niż 0,1 s przed trafieniem (n={tot}).");
            }

            var acts = Tally.Merge(r.Select(x => x.actions));
            var expected = new[] { "szybki atak", "mocny atak", "riposta", "blok", "parowanie", "unik", "umiejętność", "flaszka (próba)" };
            var missing = expected.Where(e => acts.Get(e) <= 0).ToList();
            l.Add(missing.Count == 0 ? "Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce."
                                     : $"**Pokrycie:** nie udało się wykonać: {string.Join(", ", missing)} – sprawdzić warunki (np. riposta wymaga złamanej postawy).");

            var msg = Tally.Merge(r.Select(x => x.messages));
            var refusals = msg.Items.Where(x => x.key.StartsWith("Za mało") || x.key.Contains("pusty") || x.key.Contains("przegrzane") || x.key.Contains("Brak")).ToList();
            if (refusals.Count > 0)
                l.Add("UX: gra odmawiała akcji: " + string.Join(", ", refusals.Select(x => $"„{x.key}” ×{x.val}")) + " – HUD powinien to sygnalizować zanim gracz naciśnie.");

            var slow = r.SelectMany(x => x.floors).Where(f => f.cleared).GroupBy(f => f.name).Select(g => (g.Key, Avg(g.Select(f => f.time)))).Where(x => x.Item2 > 150f).ToList();
            foreach (var (name, t) in slow) l.Add($"Tempo: piętro „{name}” trwa średnio {F(t, 0)} s – najdłuższy fragment wieży.");

            float oneShots = r.Count(x => x.result == "Śmierć" && x.maxHealth > 0 && x.killerHitDamage >= 0.4f * x.maxHealth);
            if (oneShots > 0) l.Add($"Balans: {oneShots} śmierci od pojedynczego ciosu ≥40% maks. HP: " +
                                    string.Join(", ", r.Where(x => x.result == "Śmierć" && x.maxHealth > 0 && x.killerHitDamage >= 0.4f * x.maxHealth).Select(x => $"{x.killer} „{x.killerAttack}” {F(x.killerHitDamage, 0)}/{F(x.maxHealth, 0)}").Distinct().Take(4)) + ".");

            float ms = Avg(r.Select(x => x.wallMsPerFrameAvg));
            l.Add($"Koszt symulacji w trybie testów: średnio {F(ms)} ms/klatkę (maks. {F(r.Count > 0 ? r.Max(x => x.wallMsPerFrameMax) : 0, 0)} ms). " +
                  "To pomiar edytora wsadowego, nie buildu gracza.");
            return l;
        }

        public static List<string> PlayerConclusions(BotSeries s)
        {
            var l = new List<string>();
            var r = s.runs;
            int n = Math.Max(1, r.Count);
            float wins = r.Count(x => x.Victory);
            float win = wins / n;
            if (win <= 0f) l.Add($"Ani jednego zwycięstwa w {r.Count} podejściach. Średnio dochodzę do piętra {F(Avg(r.Select(x => (float)x.floorReached)))} – na razie to ściana, nie krzywa.");
            else if (win < 0.25f) l.Add($"Wygrałem {wins}/{r.Count}. Trudno, ale do zrobienia – tak ma być w souls-like.");
            else if (win < 0.6f) l.Add($"Wygrałem {wins}/{r.Count}. Wyzwanie umiarkowane; po opanowaniu parowania wieża przestaje straszyć.");
            else l.Add($"Wygrałem {wins}/{r.Count} – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.");

            if (r.Count >= 6)
            {
                int h = r.Count / 2;
                float a = Avg(r.Take(h).Select(x => (float)x.floorReached + (x.Victory ? 1 : 0)));
                float b = Avg(r.Skip(h).Select(x => (float)x.floorReached + (x.Victory ? 1 : 0)));
                l.Add(b > a + 0.4f ? $"Czuję progres: druga połowa podejść średnio piętro {F(b)} wobec {F(a)} na początku (popiół, odblokowania, wybór zestawu)."
                                   : $"Progres między podejściami słabo odczuwalny: {F(a)} → {F(b)} średnio. Meta-progresja niewiele zmienia w walce.");
            }

            var resp = Tally.Merge(r.Select(x => x.responses));
            float parryN = Count(resp, "zwykły", "parowanie");
            float parryHurt = HurtRate(resp, "zwykły", "parowanie");
            var acts = Tally.Merge(r.Select(x => x.actions));
            float rip = acts.Get("riposta");
            var src = Tally.Merge(r.Select(x => x.damageBySource));
            float total = Math.Max(1f, src.Sum);
            if (parryN >= 5)
            {
                float ok = 1f - Math.Max(0f, parryHurt);
                l.Add(ok >= 0.5f
                    ? $"Parowanie siedzi: {Pct(ok, 1)} udanych na zwykłych ciosach, {F(rip / n)} ripost na podejście. To najlepsza część walki."
                    : $"Parowanie zawodne: tylko {Pct(ok, 1)} udanych (n={parryN}). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.");
            }
            else l.Add("Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).");

            var kills = Tally.Merge(r.Select(x => x.enemyKills));
            var hits = Tally.Merge(r.Select(x => x.enemyHitsSum));
            var ttk = Tally.Merge(r.Select(x => x.enemyTtkSum));
            var eng = Tally.Merge(r.Select(x => x.enemyEngagedKills));
            var common = eng.Items.Where(x => x.val >= 10).Select(x => (x.key, x.val, hits.Get(x.key) / x.val, ttk.Get(x.key) / x.val)).ToList();
            var sponges = common.Where(x => x.Item3 > 8f).OrderByDescending(x => x.Item3).Take(3).ToList();
            var paper = common.Where(x => x.Item3 < 2.5f).OrderBy(x => x.Item3).Take(3).ToList();
            if (sponges.Count > 0) l.Add("Gąbki na obrażenia: " + string.Join(", ", sponges.Select(x => $"{x.key} ({F(x.Item3)} trafień, {F(x.Item4)} s)")) + ". W H&S chcę czuć siłę ciosu.");
            if (paper.Count > 0) l.Add("Mięso armatnie padające od 1–2 ciosów: " + string.Join(", ", paper.Select(x => $"{x.key} ({F(x.Item3)})")) + " – fajne przy hordach, ale nie uczą niczego.");
            var bosses = eng.Items.Where(x => x.val >= 1 && ttk.Get(x.key) / x.val > 40f).ToList();
            foreach (var bo in bosses) l.Add($"Długie starcie: {bo.key} – średnio {F(ttk.Get(bo.key) / bo.val, 0)} s i {F(hits.Get(bo.key) / bo.val, 0)} trafień do zabicia.");

            var dead = r.Where(x => x.result == "Śmierć").ToList();
            if (dead.Count > 0)
            {
                var top = dead.GroupBy(x => x.killer).OrderByDescending(g => g.Count()).First();
                if (top.Count() >= Math.Max(2, dead.Count * 0.35f))
                    l.Add($"Mój nemezis: {top.Key} – {top.Count()} z {dead.Count} śmierci (najczęściej „{top.GroupBy(x => x.killerAttack).OrderByDescending(g => g.Count()).First().Key}”).");
                float ticks = dead.Count(x => x.killerAttack == "efekt (tyknięcie)");
                if (ticks > 0) l.Add($"{ticks}× zabiło mnie tyknięcie efektu (krwawienie/podpalenie) – mało czytelna śmierć; brakuje ostrzeżenia przy niskim HP.");
                float withFlasks = dead.Count(x => x.flasksLeft > 0);
                float burst = dead.Count(x => x.deathBurst3s >= 0.5f);
                float att = r.Sum(x => x.flaskAttempts), drank = r.Sum(x => x.flasksUsed);
                if (withFlasks >= dead.Count * 0.4f)
                    l.Add($"{withFlasks}/{dead.Count} razy zginąłem z flaszkami w kieszeni. {burst} z tych śmierci to utrata ≥50% HP w 3 s" +
                          (att > 0 ? $", a {Pct(att - drank, att)} prób picia przerwał cios (flaszka działa dopiero po wypiciu)." : ".") +
                          " W gęstym tłumie nie ma okna na leczenie.");
            }
            var an = Tally.Merge(r.Select(x => x.anomalies));
            var inc = Tally.Merge(r.Select(x => x.incoming));
            float hitsTotal = inc.Items.Where(x => x.key.EndsWith("|trafienie")).Sum(x => x.val);
            float behind = an.Get("trafienie z tyłu (poza polem widzenia)");
            if (hitsTotal > 0 && behind / hitsTotal > 0.2f) l.Add($"Za dużo ciosów zza pleców ({Pct(behind, hitsTotal)} trafień). Przy namierzaniu nie widzę reszty grupy – w souls to oznaka złego designu walki z grupą.");

            var uses = Tally.Merge(r.Select(x => x.skillUses));
            float skillDmg = uses.k.Sum(k => src.Get(k));
            float share = skillDmg / total;
            l.Add(share < 0.15f ? $"Skille dają tylko {Pct(skillDmg, total)} obrażeń – jako fan H&S chcę, żeby to one robiły show; teraz to dodatek do miecza."
                : share > 0.6f ? $"Skille robią {Pct(skillDmg, total)} obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów."
                : $"Skille robią {Pct(skillDmg, total)} obrażeń – zdrowa proporcja między bronią a umiejętnościami.");
            var bestSkill = uses.Items.Where(x => x.val >= 5).Select(x => (x.key, src.Get(x.key) / x.val)).OrderByDescending(x => x.Item2).FirstOrDefault();
            if (bestSkill.key != null) l.Add($"Najmocniej czuć „{bestSkill.key}”: {F(bestSkill.Item2, 0)} obrażeń na użycie.");
            var byOrig = Tally.Merge(r.Select(x => x.skillUsesByOrigin));
            float uStart = byOrig.Items.Where(x => x.key.EndsWith("|startowy")).Sum(x => x.val);
            float uRun = byOrig.Items.Where(x => x.key.EndsWith("|nagroda") || x.key.EndsWith("|sklep")).Sum(x => x.val);
            if (byOrig.Sum > 0)
                l.Add($"Skill startowy to {Pct(uStart, byOrig.Sum)} moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – {Pct(uRun, byOrig.Sum)}. " +
                      $"Różnych skilli na podejście: średnio {F(Avg(r.Select(x => (float)x.skillUses.Items.Count(y => y.val > 0))))}.");

            var combos = r.GroupBy(x => $"{x.loadout} + {x.startSkill}").Select(g => (g.Key, g.Count(), Avg(g.Select(x => (float)x.floorReached + (x.Victory ? 1 : 0))))).OrderByDescending(x => x.Item2).ToList();
            if (combos.Count > 1)
                l.Add($"Najczęściej grałem: {combos[0].Key} ({combos[0].Item2}×, śr. piętro {F(combos[0].Item3)}). " +
                      "Inne: " + string.Join(", ", combos.Skip(1).Select(c => $"{c.Key} {c.Item2}× ({F(c.Item3)})")) + ".");

            var why = Tally.Merge(r.Select(x => x.dodgeReasons));
            float shotPressed = why.Get("unik przed pociskiem");
            float shotHurt = resp.Items.Where(x => x.key.StartsWith("unik") && x.key.Contains("|pocisk|") && (x.key.EndsWith("|trafienie") || x.key.EndsWith("|przełamana garda"))).Sum(x => x.val);
            float dodges = acts.Get("unik") / n;
            float tactical = (why.Get("wyjście z okrążenia") + why.Get("doskok do odsłoniętego celu")) / n;
            l.Add($"Unik to moje główne narzędzie: {F(dodges)} na podejście, w tym {F(tactical)} z inicjatywy (wyjście z okrążenia, doskok)." +
                  (shotPressed >= 5 ? $" Po uniku przed pociskiem nie obrywam w {Pct(Math.Max(0f, shotPressed - shotHurt), shotPressed)} przypadków." : ""));
            var refusedBy = why.Items.Where(x => x.key.StartsWith("odmowa")).OrderByDescending(x => x.val).ToList();
            float refused = refusedBy.Sum(x => x.val) / n;
            if (refused >= 1f)
                l.Add($"Gra nie wykonała uniku od razu średnio {F(refused)}× na podejście – najczęściej: " +
                      string.Join(", ", refusedBy.Take(3).Select(x => $"{x.key.Replace("odmowa: ", "").Replace("Flask", "picie flaszki").Replace("unik", "w trakcie poprzedniego uniku")} {F(x.val / n)}")) +
                      ". Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.");

            float lowSt = Avg(r.Select(x => x.timeLowStamina));
            float gt = Math.Max(1f, Avg(r.Select(x => x.gameTime)));
            if (lowSt / gt > 0.12f) l.Add($"Wytrzymałość kończy się często ({Pct(lowSt, gt)} czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.");
            float flinch = acts.Get("drgnięcie (trafiony)") / n;
            if (flinch > 25f) l.Add($"Dużo drgnięć po trafieniu ({F(flinch, 0)} na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.");
            return l;
        }
    }
}
