# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 15:07. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 6,8 | 7,0 | 8,3 min | 185,8 | 36284 / 4173 | 11,5 | 1% |
| Gracz H&S / souls-like | 20 | 0 (0%) | 7,2 | 7,0 | 9,2 min | 190,9 | 37393 / 3958 | 8,7 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 7 7 8 7 9 7 8 5 7 5 9 7 5 5 8 6 8 5 7 6
- Gracz H&S / souls-like: 7 7 6 5 8 7 8 7 7 7 7 7 7 7 8 7 7 7 7 10

## Gdzie i od czego giną

- **Tester QA** — piętra: 5. V. Psiarnia Piekieł ×5, 6. VI. Pan Sfory ×2, 7. VII. Zatopione Krużganki ×7, 8. VIII. Zatopione Krużganki ×4, 9. IX. Królowa Topieli ×2.
  Ostatni cios: Kolos – „Kolos” ×3; Taranowiec – „Taranowiec” ×3; Kultysta topieli – „Kultysta topieli” ×2; Kolos – „Kolos: Combo” ×2; Królowa Topieli – „Królowa Topieli: Combo” ×2; Kultysta – „Kula ognia” ×2. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 18/20.
  Ostatnie 3 s przed śmiercią: średnio 38% maks. HP straconych (śmierć z ≥50% HP w 3 s: 5/20). Picie: 276 prób, 229 wypitych (17% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 10. X. Rzeźnia ×1, 5. V. Psiarnia Piekieł ×1, 6. VI. Pan Sfory ×1, 7. VII. Zatopione Krużganki ×14, 8. VIII. Zatopione Krużganki ×3.
  Ostatni cios: Kultysta topieli – „Kultysta topieli” ×4; Czerw – „Ugryzienie” ×3; Siarkowy pomiot – „Siarkowy pomiot” ×2; Kultysta – „Kula ognia” ×2; Wodny chochlik – „Pazury” ×2; Pan Sfory – „Pan Sfory: Volley” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 18/20.
  Ostatnie 3 s przed śmiercią: średnio 30% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/20). Picie: 214 prób, 174 wypitych (19% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 56 s | 19 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 21 | 20 | 61 s | 162 | 0,4 | 40,4 |
| Tester QA | 3. III. Władca Bramy | 20 | 20 | 33 s | 327 | 1,2 | 7,9 |
| Tester QA | 4. IV. Psiarnia Piekieł | 22 | 20 | 62 s | 224 | 0,7 | 43,0 |
| Tester QA | 5. V. Psiarnia Piekieł | 35 | 15 | 63 s | 446 | 1,2 | 28,9 |
| Tester QA | 6. VI. Pan Sfory | 27 | 13 | 43 s | 591 | 2,4 | 3,6 |
| Tester QA | 7. VII. Zatopione Krużganki | 32 | 6 | 80 s | 586 | 1,0 | 19,6 |
| Tester QA | 8. VIII. Zatopione Krużganki | 15 | 2 | 90 s | 808 | 1,7 | 25,5 |
| Tester QA | 9. IX. Królowa Topieli | 6 | 0 | 0 s | 963 | 2,7 | 0,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 54 s | 30 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 25 | 20 | 66 s | 245 | 0,7 | 37,6 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 29 s | 157 | 0,2 | 7,6 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 22 | 20 | 60 s | 181 | 0,4 | 43,8 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 29 | 19 | 67 s | 388 | 0,8 | 33,6 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 24 | 18 | 47 s | 482 | 2,0 | 6,0 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 47 | 4 | 85 s | 638 | 1,1 | 21,1 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 11 | 1 | 104 s | 899 | 1,5 | 21,6 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 1 | 1 | 115 s | 1250 | 5,0 | 3,0 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 3 | 0 | 0 s | 457 | 0,0 | 13,3 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 2 ×1, piętro 4 ×2, piętro 5 ×15, piętro 6 ×12, piętro 7 ×19, piętro 8 ×9, piętro 9 ×4; wyzwań (3 porażki na piętrze): 20 – class_knight_Tester_pietro07.json, class_knight_Tester_pietro08.json, class_knight_Tester_pietro09.json, class_knight_Tester_pietro05.json, class_knight_Tester_pietro06.json.
**Gracz H&S / souls-like** – ponowne próby: piętro 2 ×5, piętro 4 ×2, piętro 5 ×9, piętro 6 ×5, piętro 7 ×29, piętro 8 ×7, piętro 10 ×2; wyzwań (3 porażki na piętrze): 20 – class_knight_SoulsSlasher_pietro07.json, class_knight_SoulsSlasher_pietro06.json, class_knight_SoulsSlasher_pietro05.json, class_knight_SoulsSlasher_pietro08.json, class_knight_SoulsSlasher_pietro10.json.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 1080 | 0,4 s | 1,1 | 1919 | 29782 | 26 | 4 |
| Pan Sfory | 32 | 40,4 s | 27,6 | 1383 | 26058 | 814 | 3 |
| Ogar otchłani | 1007 | 0,8 s | 1,5 | 1403 | 15603 | 15 | 1 |
| Kultysta topieli | 112 | 2,1 s | 1,6 | 738 | 14945 | 131 | 7 |
| Władca Bramy | 40 | 26,7 s | 17,3 | 393 | 8833 | 221 | 0 |
| Harpia | 153 | 1,1 s | 1,7 | 640 | 8522 | 56 | 1 |
| Taranowiec | 348 | 3,6 s | 2,6 | 544 | 8300 | 24 | 4 |
| Czerw | 2776 | 0,6 s | 1,3 | 1777 | 7121 | 3 | 5 |
| Królowa Topieli | 1 | 111,8 s | 81,0 | 297 | 6993 | 6993 | 2 |
| Kolos | 147 | 5,9 s | 3,8 | 141 | 6291 | 43 | 6 |
| Siarkowy pomiot | 652 | 0,2 s | 1,1 | 184 | 6140 | 9 | 3 |
| Wiedźma wichru | 36 | 0,6 s | 1,4 | 526 | 5983 | 153 | 0 |
| Legionista | 175 | 7,6 s | 5,1 | 626 | 5629 | 32 | 1 |
| Ognisty chochlik | 594 | 0,4 s | 1,2 | 246 | 3003 | 5 | 0 |
| Wodny chochlik | 644 | 0,2 s | 1,2 | 277 | 2777 | 4 | 2 |
| Chochlik wichru | 601 | 0,6 s | 1,3 | 244 | 2507 | 4 | 0 |
| Kamienny chochlik | 584 | 0,2 s | 1,1 | 211 | 2339 | 4 | 1 |
| Ofiarnik | 27 | 2,8 s | 1,5 | 112 | 884 | 33 | 0 |
| Ożywiony nietoperz | 270 | 0,5 s | 1,2 | 182 | 852 | 3 | 0 |
| Gnilec (odłamek) | 1 | 0,9 s | 3,0 | 1 | 29 | 29 | 0 |
| Gnilec | 1 | 9,0 s | 5,0 | 2 | 29 | 29 | 0 |
| Czerw (elita) | 19 | 2,2 s | 2,9 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 62 | 3,6 s | 4,2 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 48 | 7,1 s | 4,0 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 24 | 6,5 s | 4,8 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 27 | 1,1 s | 1,6 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 5 | 23,4 s | 11,0 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 3 | 18,5 s | 8,3 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 2 | 25,0 s | 6,5 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 2 | 41,8 s | 24,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2668 | 78% | 3% | 3% | 12% | 4% |
| ciężki | 287 | 67% | 2% | 2% | 23% | 6% |
| nieparowalny | 1472 | 74% | 5% | 0% | 12% | 9% |
| nieblokowalny | 139 | 83% | 0% | 0% | 17% | 0% |
| obszarowy | 862 | 88% | 7% | 0% | 0% | 5% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3114 | 62% | 2% | 9% | 25% | 2% |
| ciężki | 336 | 50% | 5% | 15% | 29% | 1% |
| nieparowalny | 1845 | 58% | 6% | 0% | 27% | 9% |
| nieblokowalny | 252 | 72% | 0% | 0% | 28% | 0% |
| obszarowy | 871 | 84% | 12% | 0% | 0% | 4% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2590 | 27% | 46% | 2% | 13% | 12% |
| Tester QA (0,30 s) | ciężki | 309 | 1% | 0% | 2% | 10% | 86% |
| Tester QA (0,30 s) | nieparowalny | 393 | 7% | 3% | 16% | 38% | 36% |
| Tester QA (0,30 s) | nieblokowalny | 21 | 5% | 0% | 14% | 14% | 67% |
| Tester QA (0,30 s) | obszarowy | 763 | 1% | 2% | 5% | 45% | 47% |
| Tester QA (0,30 s) | pocisk | 1895 | 1% | 11% | 11% | 17% | 60% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3446 | 3% | 32% | 42% | 7% | 17% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 446 | 2% | 0% | 0% | 8% | 89% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 438 | 9% | 2% | 4% | 30% | 55% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 13 | 31% | 0% | 0% | 15% | 54% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 940 | 2% | 1% | 4% | 38% | 55% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2952 | 1% | 9% | 9% | 16% | 64% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 1020 |
| Wodny chochlik: Pazury | 0,33 s | 449 |
| Ognisty chochlik: Pazury | 0,33 s | 421 |
| Kamienny chochlik: Pazury | 0,33 s | 370 |
| Czerw: Ugryzienie | 0,33 s | 2677 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 281 |
| Chochlik wichru: Pazury | 0,33 s | 427 |
| Władca Bramy: Cięcie w szale | 0,41 s | 23 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 216 |
| Królowa Topieli: Królowa Topieli: WindStep | 0,45 s | 18 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 66% (n=114) | 0% (n=3) | 55% (n=77) | 100% (n=6) | 37% (n=71) | 91% (n=479) |
| blok (eksperyment) | 80% (n=119) | 92% (n=25) | 85% (n=48) | – | 78% (n=49) | 75% (n=75) |
| brak reakcji (eksperyment) | 93% (n=105) | 94% (n=17) | 100% (n=44) | 100% (n=5) | 100% (n=26) | 97% (n=68) |
| niezauważony | 91% (n=496) | – | 97% (n=176) | 99% (n=72) | 100% (n=288) | – |
| parowanie | 64% (n=53) | 79% (n=19) | – | – | – | – |
| parowanie (eksperyment) | 76% (n=62) | 80% (n=10) | 92% (n=13) | 100% (n=2) | 100% (n=15) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 86% (n=59) |
| unik | 67% (n=491) | 48% (n=118) | 36% (n=160) | 47% (n=30) | – | 73% (n=154) |
| unik (eksperyment) | 79% (n=184) | 55% (n=11) | 47% (n=53) | 36% (n=11) | 98% (n=66) | 86% (n=49) |
| wyjście z zasięgu | 92% (n=711) | 100% (n=74) | 99% (n=175) | 100% (n=9) | 100% (n=325) | – |
| wyjście z zasięgu (eksperyment) | 90% (n=117) | 100% (n=10) | 100% (n=23) | 100% (n=4) | 90% (n=21) | 86% (n=36) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 50,1 | reakcja na cios wręcz 61,5, unik przed pociskiem 24,2, wyjście z okrążenia 3,8 | brak wytrzymałości 11,5, drgnięcie (trafiony) 9,2, w trakcie uniku 5,1, picie flaszki 3,6, inna 0,9, parowanie 0,8, riposta 0,4, mocny atak 0,3, umiejętność 0,3, szybki atak 0,1 | 43% (n=1230) / 68% (n=483) |
| Gracz H&S / souls-like | 91,3 | reakcja na cios wręcz 82,4, unik przed pociskiem 54,5, doskok do odsłoniętego celu 6,8, wyjście z okrążenia 6,0 | w trakcie uniku 15,7, drgnięcie (trafiony) 9,6, brak wytrzymałości 7,4, parowanie 5,3, picie flaszki 3,8, riposta 1,0, inna 0,5, mocny atak 0,2, szybki atak 0,2, blok 0,1 | 58% (n=1648) / 73% (n=1089) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 8,0 | 224,2 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 7,2 | 197,6 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 6,2 | 167,4 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 5,8 | 153,8 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 17 | 0 | 7,4 | 197,6 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 1 | 0 | 7,0 | 180,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 6,0 | 154,0 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 1 | 0 | 5,0 | 124,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 129727 | 18% |  |  |
| Rozpłatanie | 101253 | 14% | 489 | 207 |
| Cięcie wichru | 86200 | 12% | 459 | 188 |
| Tąpnięcie | 79525 | 11% | 468 | 170 |
| Cięcie | 52350 | 7% |  |  |
| Egzekucja | 48065 | 7% |  |  |
| Fala przełamania | 46908 | 6% | 126 | 372 |
| Cięcie krzyżowe | 30370 | 4% | 163 | 186 |
| Okrzyk bojowy | 29662 | 4% | 634 | 47 |
| Potężne cięcie | 23647 | 3% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×95.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Rozpłatanie | 388238 | 52% | 2180 | 178 |
| Cięcie wichru | 72132 | 10% | 380 | 190 |
| Fala przełamania | 60177 | 8% | 185 | 325 |
| Cięcie | 51569 | 7% |  |  |
| Szarża | 48412 | 6% | 524 | 92 |
| Riposta | 37548 | 5% |  |  |
| Okrzyk bojowy | 28762 | 4% | 385 | 75 |
| Potężne cięcie | 20684 | 3% |  |  |
| Cięcie krzyżowe | 11058 | 1% | 68 | 163 |
| Zamach | 8866 | 1% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Kontra ×11.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 6,3, mediana 6,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 19% | 32,5 | 190 | 1,1 |
| odblokowanie | 14% | 23,4 | 170 | 0,6 |
| klasa | 19% | 31,7 | 47 | 0,1 |
| nagroda | 5% | 8,7 | 125 | 0,5 |
| sklep | 42% | 70,5 | 148 | 0,8 |

Oferty sklepu dusz: widziane 116, kupione 82, odrzucone 0, za mało dusz 34.
Zamiany w slotach: 4,4 na podejście; najczęstsze: Tąpnięcie → Cięcie wichru ×6, Tąpnięcie → Rozpłatanie ×5, Rozpłatanie → Tąpnięcie ×5, Rozpłatanie → Rozpłatanie ×4, Cięcie wichru → Cięcie krzyżowe ×3, Tąpnięcie → Upadek kata ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 20 | 31,7 | 47 | 0,1 |
| Rozpłatanie | startowy | 18 | 27,2 | 207 | 1,0 |
| Tąpnięcie | odblokowanie | 20 | 23,4 | 170 | 0,6 |
| Cięcie wichru | sklep | 16 | 28,7 | 188 | 0,8 |
| Krwawe cięcie | sklep | 6 | 38,3 | 95 | 0,7 |
| Szarża | sklep | 9 | 19,6 | 124 | 0,9 |
| Cięcie krzyżowe | sklep | 6 | 27,2 | 186 | 0,9 |
| Wypad | sklep | 6 | 25,8 | 28 | 1,1 |
| Fala przełamania | sklep | 3 | 42,0 | 372 | 1,3 |
| Okrzyk wojenny | sklep | 6 | 15,8 | – | 0,6 |
| Cięcie z wyskoku | sklep | 6 | 15,8 | 116 | 0,4 |
| Upadek kata | nagroda | 5 | 16,0 | 287 | 0,4 |
| Kontra | sklep | 3 | 24,7 | 48 | 1,0 |
| Odtrącenie | nagroda | 3 | 20,3 | 9 | 0,5 |
| Uderzenie tarczą | sklep | 1 | 22,0 | 58 | 0,0 |
| Piekielna szarża | nagroda | 2 | 4,5 | 164 | 1,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,3, mediana 4,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 58% | 110,2 | 178 | 1,8 |
| odblokowanie | 17% | 33,4 | 98 | 1,3 |
| klasa | 10% | 19,3 | 75 | 0,7 |
| nagroda | 1% | 2,0 | 283 | 0,4 |
| sklep | 14% | 26,7 | 217 | 1,4 |

Oferty sklepu dusz: widziane 123, kupione 20, odrzucone 63, za mało dusz 40.
Zamiany w slotach: 1,3 na podejście; najczęstsze: Szarża → Fala przełamania ×8, Szarża → Cięcie wichru ×4, Szarża → Cięcie krzyżowe ×3, Rozpłatanie → Tąpnięcie ×2, Aura niezłomności → Upadek kata ×1, Tąpnięcie → Krwawe cięcie ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | startowy | 20 | 109,0 | 178 | 1,8 |
| Szarża | odblokowanie | 19 | 27,6 | 92 | 1,3 |
| Okrzyk bojowy | klasa | 20 | 19,3 | 75 | 0,7 |
| Cięcie wichru | startowy | 7 | 54,3 | 190 | 1,5 |
| Fala przełamania | nagroda | 9 | 20,6 | 325 | 1,1 |
| Tąpnięcie | odblokowanie | 3 | 23,0 | 49 | 0,4 |
| Cięcie krzyżowe | sklep | 3 | 22,7 | 163 | 1,4 |
| Upadek kata | sklep | 1 | 11,0 | 141 | 0,4 |
| Kontra | sklep | 1 | 11,0 | – | 0,0 |
| Uderzenie tarczą | sklep | 1 | 11,0 | 53 | 1,0 |
| Krwawe cięcie | sklep | 1 | 7,0 | 96 | 0,6 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 166,8, szybki atak 73,4, drgnięcie (trafiony) 65,7, unik 50,1, blok 40,1, mocny atak 24,7, przełamana garda 14,0, flaszka (próba) 13,8, parowanie 4,1, riposta 0,9.
Czas w bloku 16 s, w biegu 28 s, bez wytrzymałości (<10%) 210 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Czujność ×7, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×6, wzmocnienie: Oddech wojownika ×5, wzmocnienie: Siła tytana ×4, wzmocnienie: Żelazna skóra ×4, wzmocnienie: Jasność umysłu ×3, wzmocnienie: Alchemik ×3, wzmocnienie: Zwinność ×3.
Zakupy: ulepszenie broni ×20, ulepszenie: Rozpłatanie ×19, ulepszenie: Cięcie wichru ×16, oferta: Rozpłatanie ×10, ulepszenie: Tąpnięcie ×9, oferta: Aura świętego ognia ×7, oferta: Aura niezłomności ×7, oferta: Szarża ×7.
Punkty cech: Strength 35, Toughness 33, Dexterity 26, Intelligence 22.
Komunikaty gry: „OKRZYK!” ×608; „SZAŁ!” ×403; „GARDA PRZEŁAMANA!” ×293; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×145; „Portale gasną – dobij pozostałe demony” ×103; „POLEGŁEŚ” ×82; „ZAMROŻENIE!” ×63; „PAROWANIE!” ×55.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 191,6, unik 91,3, drgnięcie (trafiony) 71,9, blok 51,8, szybki atak 46,7, parowanie 18,9, mocny atak 16,4, przełamana garda 12,7, flaszka (próba) 10,7, riposta 7,6.
Czas w bloku 23 s, w biegu 31 s, bez wytrzymałości (<10%) 156 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +3 ×9, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×7, ulepszenie: Ulepszenie: Szarża → +1 ×7, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×5, wzmocnienie: Pijawka ×5, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×5, wzmocnienie: Żelazna skóra ×4, wzmocnienie: Mistrz parowania ×4.
Zakupy: ulepszenie: Rozpłatanie ×65, ulepszenie: Okrzyk bojowy ×25, ulepszenie broni ×21, ulepszenie: Szarża ×14, ulepszenie: Cięcie wichru ×10, dodatkowa flaszka ×10, ulepszenie: Fala przełamania ×9, oferta: Fala przełamania ×7.
Punkty cech: Toughness 44, Strength 79.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Puklerz, Kontra, Sztylet parujący, Łuk refleksyjny, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×352; „PAROWANIE!” ×331; „SZAŁ!” ×305; „Postawa przełamana – RIPOSTA!” ×276; „GARDA PRZEŁAMANA!” ×258; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×157; „RIPOSTA!” ×137; „Portale gasną – dobij pozostałe demony” ×120.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×488, garda nie zatrzymała ciosu ×13, parowanie w oknie nie zadziałało ×12, zablokowany ruch ×5.
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 129°) (w 2 podejściach)
  - Kultysta topieli: „Kultysta topieli” przeszedł przez gardę (kąt 138°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; -0,3)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 96°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 147°)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 157°)
  - Kamienny chochlik: „Pazury” przeszedł przez gardę (kąt 144°)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,8; 8,1)
  - Czerw: „Wgryzienie” trafił w aktywnym oknie parowania (kąt do źródła 97°)
  - Ognisty chochlik: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 81°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 109°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 130°)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×582, parowanie w oknie nie zadziałało ×44, garda nie zatrzymała ciosu ×18, zablokowany ruch ×4.
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 93°) (w 2 podejściach)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 85°) (w 2 podejściach)
  - piętro 5 „V. Psiarnia Piekieł”: brak postępu przy (7,9; 9,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-5,6; 7,8)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 120°)
  - Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 171°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 156°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 158°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 85°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 113°)
  - Kamienny chochlik: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 142°)
  - Kamienny chochlik: „Pazury” przeszedł przez gardę (kąt 159°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (5 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 12× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 96°)
- **Do sprawdzenia (P3):** 13× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta: „Kula ognia” przeszedł przez gardę (kąt 129°)
- Parowanie zwykłych ciosów: 70% ciosów mimo to rani (n=115) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 44% ciosów mimo to rani (n=41) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: 49% ciosów mimo to rani (n=129) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 73% ciosów mimo to rani (n=233) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 99% ciosów mimo to rani (n=346) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 32% pocisków, przed którymi był unik (n=483).
- Blok pocisku: 89% ciosów mimo to rani (n=554) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 72% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2590) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=309).
- Telegraf „nieparowalny” daje czas: tylko 10% decyzji zapada mniej niż 0,1 s przed trafieniem (n=393).
- Telegraf „nieblokowalny” daje czas: tylko 5% decyzji zapada mniej niż 0,1 s przed trafieniem (n=21).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=763).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Brak broni/tarczy zdolnej do parowania” ×1 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 71 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 20 podejściach. Średnio dochodzę do piętra 7,2 – na razie to ściana, nie krzywa.
- Czuję progres: druga połowa podejść średnio piętro 7,4 wobec 6,9 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 40% udanych (n=749). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (28,2 trafień, 41,9 s), Władca Bramy (15,6 trafień, 24,1 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Siarkowy pomiot (1,1), Kultysta (1,1), Kamienny chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Pan Sfory – średnio 42 s i 28 trafień do zabicia.
- Długie starcie: Królowa Topieli – średnio 112 s i 81 trafień do zabicia.
- 18/20 razy zginąłem z flaszkami w kieszeni. 2 z tych śmierci to utrata ≥50% HP w 3 s, a 19% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 82% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 325 obrażeń na użycie.
- Skill startowy to 58% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 15%. Różnych skilli na podejście: średnio 4,3.
- Najczęściej grałem: Miecz i tarcza + Rozpłatanie (17×, śr. piętro 7,4). Inne: Miecz i tarcza + Cięcie wichru 1× (7,0), Dwuręczny + Rozpłatanie 1× (6,0), Dwuręczny + Cięcie wichru 1× (5,0).
- Unik to moje główne narzędzie: 91,3 na podejście, w tym 12,8 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 73% przypadków.
- Gra nie wykonała uniku od razu średnio 43,4× na podejście – najczęściej: w trakcie poprzedniego uniku 15,7, drgnięcie (trafiony) 9,6, brak wytrzymałości 7,4. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (28% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (72 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

