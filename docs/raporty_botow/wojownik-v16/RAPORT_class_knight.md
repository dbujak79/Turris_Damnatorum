# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 18:06. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 7,7 | 8,0 | 14,6 min | 219,2 | 48247 / 5355 | 17,3 | 1% |
| Gracz H&S / souls-like | 20 | 0 (0%) | 9,1 | 9,0 | 19,4 min | 276,1 | 74253 / 6955 | 20,9 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 8 8 9 6 8 6 9 9 6 9 6 8 6 6 9 9 6 6 11 9
- Gracz H&S / souls-like: 9 9 5 11 9 11 6 5 9 9 11 7 9 9 14 9 9 9 11 11

## Gdzie i od czego giną

- **Tester QA** — piętra: 11. XI. Rzeźnia ×1, 6. VI. Pan Sfory ×8, 8. VIII. Zatopione Krużganki ×4, 9. IX. Królowa Topieli ×7.
  Ostatni cios: Pan Sfory – „Pan Sfory: Combo” ×5; Pan Sfory – „Pan Sfory: InfernalCharge” ×3; Królowa Topieli – „Królowa Topieli: Combo” ×2; Królowa Topieli – „Królowa Topieli: WindStep” ×2; Siarkowy pomiot – „Siarkowy pomiot” ×1; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 12/20.
  Ostatnie 3 s przed śmiercią: średnio 34% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/20). Picie: 418 prób, 345 wypitych (17% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 11. XI. Rzeźnia ×5, 14. XIV. Kamienne Trzewia ×1, 5. V. Psiarnia Piekieł ×2, 6. VI. Pan Sfory ×1, 7. VII. Zatopione Krużganki ×1, 9. IX. Królowa Topieli ×10.
  Ostatni cios: Królowa Topieli – „Królowa Topieli: CycloneEye” ×4; Miotacz głazów – „Miotacz głazów” ×3; Kultysta – „Kula ognia” ×2; Królowa Topieli – „Królowa Topieli: Flood” ×2; Królowa Topieli – „Królowa Topieli: TidalWave” ×1; Pan Sfory – „Pan Sfory: Volley” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 10/20.
  Ostatnie 3 s przed śmiercią: średnio 32% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/20). Picie: 503 prób, 418 wypitych (17% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 100 s | 10 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 114 s | 66 | 0,1 | 40,6 |
| Tester QA | 3. III. Władca Bramy | 22 | 20 | 40 s | 344 | 1,2 | 7,4 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 113 s | 77 | 0,1 | 45,4 |
| Tester QA | 5. V. Psiarnia Piekieł | 23 | 20 | 101 s | 291 | 0,5 | 37,7 |
| Tester QA | 6. VI. Pan Sfory | 43 | 12 | 46 s | 728 | 3,1 | 4,1 |
| Tester QA | 7. VII. Zatopione Krużganki | 18 | 12 | 113 s | 718 | 2,1 | 34,5 |
| Tester QA | 8. VIII. Zatopione Krużganki | 26 | 8 | 132 s | 727 | 1,8 | 32,9 |
| Tester QA | 9. IX. Królowa Topieli | 22 | 1 | 66 s | 1062 | 3,4 | 0,3 |
| Tester QA | 10. X. Rzeźnia | 1 | 1 | 121 s | 671 | 4,0 | 67,0 |
| Tester QA | 11. XI. Rzeźnia | 3 | 0 | 0 s | 877 | 2,3 | 16,7 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 99 s | 18 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 21 | 20 | 116 s | 86 | 0,3 | 39,3 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 48 s | 334 | 1,0 | 7,6 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 22 | 20 | 110 s | 164 | 0,6 | 44,9 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 25 | 18 | 102 s | 303 | 0,8 | 33,3 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 22 | 17 | 50 s | 552 | 2,6 | 5,4 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 21 | 16 | 102 s | 468 | 1,1 | 36,4 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 24 | 16 | 134 s | 700 | 2,0 | 46,5 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 40 | 6 | 104 s | 1297 | 4,0 | 0,9 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 9 | 6 | 117 s | 539 | 1,6 | 51,9 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 16 | 1 | 119 s | 1028 | 2,6 | 33,5 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 1 | 1 | 94 s | 285 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 3 | 1 | 147 s | 986 | 2,3 | 37,3 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 3 | 0 | 0 s | 1292 | 2,3 | 51,3 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 3 ×2, piętro 5 ×3, piętro 6 ×23, piętro 7 ×6, piętro 8 ×14, piętro 9 ×14, piętro 11 ×2; wyzwań (3 porażki na piętrze): 20 – class_knight_Tester_pietro08.json, class_knight_Tester_pietro09.json, class_knight_Tester_pietro06.json, class_knight_Tester_pietro11.json.
**Gracz H&S / souls-like** – ponowne próby: piętro 2 ×1, piętro 4 ×2, piętro 5 ×5, piętro 6 ×4, piętro 7 ×4, piętro 8 ×8, piętro 9 ×24, piętro 10 ×3, piętro 11 ×10, piętro 13 ×2, piętro 14 ×2; wyzwań (3 porażki na piętrze): 20 – class_knight_SoulsSlasher_pietro09.json, class_knight_SoulsSlasher_pietro05.json, class_knight_SoulsSlasher_pietro11.json, class_knight_SoulsSlasher_pietro06.json, class_knight_SoulsSlasher_pietro07.json, class_knight_SoulsSlasher_pietro14.json.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Królowa Topieli | 7 | 94,9 s | 68,9 | 3123 | 73305 | 10472 | 16 |
| Pan Sfory | 29 | 43,6 s | 29,0 | 2060 | 40945 | 1412 | 9 |
| Kultysta | 1316 | 0,6 s | 1,2 | 1072 | 19076 | 14 | 2 |
| Kultysta topieli | 320 | 1,3 s | 1,6 | 805 | 16857 | 51 | 1 |
| Władca Bramy | 40 | 39,4 s | 23,2 | 562 | 13485 | 337 | 0 |
| Wiedźma wichru | 241 | 2,6 s | 2,0 | 1099 | 13171 | 52 | 1 |
| Ogar otchłani | 1116 | 1,1 s | 1,7 | 1098 | 11604 | 10 | 0 |
| Miotacz głazów | 34 | 3,7 s | 2,7 | 244 | 8942 | 255 | 4 |
| Kolos | 271 | 4,1 s | 3,7 | 193 | 8097 | 29 | 1 |
| Harpia | 256 | 1,4 s | 2,1 | 571 | 7422 | 28 | 1 |
| Taranowiec | 453 | 3,3 s | 2,9 | 417 | 6203 | 13 | 2 |
| Legionista | 350 | 7,7 s | 5,6 | 673 | 5559 | 15 | 1 |
| Siarkowy pomiot | 695 | 0,2 s | 1,1 | 128 | 4521 | 6 | 1 |
| Czerw | 3064 | 0,5 s | 1,3 | 1012 | 4030 | 1 | 1 |
| Ognisty chochlik | 622 | 0,2 s | 1,2 | 187 | 1715 | 2 | 0 |
| Wodny chochlik | 664 | 0,2 s | 1,2 | 151 | 1544 | 2 | 0 |
| Gnilec | 55 | 5,8 s | 5,4 | 89 | 1508 | 27 | 0 |
| Kamienny chochlik | 633 | 0,4 s | 1,2 | 141 | 1333 | 2 | 0 |
| Chochlik wichru | 607 | 0,8 s | 1,7 | 120 | 1153 | 2 | 0 |
| Gnilec (odłamek) | 98 | 2,4 s | 2,5 | 82 | 1008 | 10 | 0 |
| Ofiarnik | 70 | 1,6 s | 1,9 | 86 | 820 | 11 | 0 |
| Tkaczka | 17 | 3,4 s | 2,8 | 61 | 780 | 46 | 0 |
| Ożywiony nietoperz | 274 | 0,6 s | 1,2 | 163 | 755 | 3 | 0 |
| Grzebacz | 8 | 2,5 s | 3,1 | 15 | 561 | 70 | 0 |
| Pajączek | 15 | 0,2 s | 1,1 | 61 | 514 | 34 | 0 |
| Upiór | 6 | 11,0 s | 4,3 | 44 | 327 | 54 | 0 |
| Cień | 8 | 1,5 s | 2,4 | 27 | 307 | 38 | 0 |
| Rzeźnik | 1 | 91,0 s | 94,0 | 43 | 285 | 285 | 0 |
| Kolos żaru | 9 | 4,7 s | 5,0 | 6 | 209 | 23 | 0 |
| Gargulec | 21 | 2,4 s | 3,8 | 8 | 151 | 7 | 0 |
| Ognisty chochlik (elita) | 71 | 3,7 s | 4,6 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 60 | 6,0 s | 4,4 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 28 | 1,5 s | 3,0 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 28 | 5,7 s | 6,0 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 8 | 19,1 s | 7,6 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 17 | 4,3 s | 3,6 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 9 | 35,6 s | 25,7 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 3 | 9,2 s | 8,3 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 17 | 16,2 s | 13,4 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 11 | 13,3 s | 7,5 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 7 | 9,2 s | 8,6 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 7 | 25,8 s | 19,6 | ↑ | ↑ | ↑ |  |
| Worek jaj | 18 | 0,1 s | 1,3 | 0 | 0 | 0 | 0 |
| Miotacz głazów (elita) | 1 | 28,1 s | 15,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2280 | 76% | 3% | 5% | 14% | 3% |
| ciężki | 337 | 63% | 3% | 4% | 28% | 3% |
| nieparowalny | 1626 | 75% | 6% | 0% | 14% | 4% |
| nieblokowalny | 350 | 79% | 0% | 0% | 21% | 0% |
| obszarowy | 1069 | 85% | 10% | 0% | 0% | 5% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3144 | 55% | 3% | 8% | 32% | 2% |
| ciężki | 612 | 37% | 3% | 12% | 48% | 0% |
| nieparowalny | 2400 | 54% | 7% | 0% | 36% | 4% |
| nieblokowalny | 320 | 68% | 0% | 0% | 32% | 0% |
| obszarowy | 2203 | 86% | 11% | 0% | 0% | 3% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1974 | 24% | 38% | 2% | 20% | 16% |
| Tester QA (0,30 s) | ciężki | 373 | 2% | 1% | 2% | 13% | 83% |
| Tester QA (0,30 s) | nieparowalny | 644 | 5% | 5% | 13% | 38% | 39% |
| Tester QA (0,30 s) | nieblokowalny | 50 | 2% | 0% | 10% | 4% | 84% |
| Tester QA (0,30 s) | obszarowy | 724 | 1% | 1% | 2% | 28% | 67% |
| Tester QA (0,30 s) | pocisk | 1347 | 5% | 22% | 16% | 17% | 39% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3047 | 2% | 27% | 32% | 11% | 27% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 653 | 1% | 1% | 0% | 9% | 88% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1006 | 2% | 1% | 4% | 30% | 62% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 111 | 2% | 0% | 1% | 7% | 90% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 1294 | 2% | 1% | 4% | 25% | 68% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2515 | 3% | 17% | 12% | 17% | 52% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 924 |
| Pajączek: Pajączek | 0,31 s | 88 |
| Wodny chochlik: Pazury | 0,33 s | 263 |
| Czerw: Ugryzienie | 0,33 s | 1609 |
| Ognisty chochlik: Pazury | 0,33 s | 277 |
| Chochlik wichru: Pazury | 0,33 s | 224 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 287 |
| Kamienny chochlik: Pazury | 0,33 s | 267 |
| Władca Bramy: Cięcie w szale | 0,41 s | 41 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 500 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 70% (n=185) | 33% (n=6) | 57% (n=138) | 100% (n=6) | 46% (n=164) | 87% (n=227) |
| blok (eksperyment) | 87% (n=98) | 67% (n=24) | 84% (n=82) | 100% (n=9) | 89% (n=72) | 76% (n=38) |
| brak reakcji (eksperyment) | 92% (n=132) | 92% (n=25) | 98% (n=64) | 100% (n=4) | 96% (n=23) | 92% (n=50) |
| niezauważony | 87% (n=434) | 100% (n=1) | 98% (n=201) | 99% (n=159) | 99% (n=306) | – |
| parowanie | 63% (n=68) | 52% (n=25) | – | – | – | – |
| parowanie (eksperyment) | 67% (n=52) | 69% (n=13) | 96% (n=54) | 100% (n=10) | 100% (n=17) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 89% (n=54) |
| unik | 56% (n=409) | 43% (n=122) | 40% (n=272) | 40% (n=101) | – | 77% (n=124) |
| unik (eksperyment) | 73% (n=154) | 38% (n=26) | 54% (n=78) | 48% (n=21) | 100% (n=109) | 85% (n=39) |
| wyjście z zasięgu | 95% (n=486) | 99% (n=80) | 99% (n=292) | 100% (n=34) | 100% (n=358) | – |
| wyjście z zasięgu (eksperyment) | 90% (n=90) | 100% (n=15) | 98% (n=60) | 100% (n=6) | 100% (n=18) | 85% (n=27) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 51,6 | reakcja na cios wręcz 57,0, unik przed pociskiem 21,6, wyjście z okrążenia 6,1 | drgnięcie (trafiony) 9,9, brak wytrzymałości 7,5, w trakcie uniku 6,0, picie flaszki 3,4, inna 0,7, parowanie 0,4, blok 0,2, szybki atak 0,2, unieruchomienie 0,1, umiejętność 0,1, riposta 0,1, mocny atak 0,1 | 37% (n=1140) / 70% (n=432) |
| Gracz H&S / souls-like | 139,6 | reakcja na cios wręcz 120,4, unik przed pociskiem 53,7, doskok do odsłoniętego celu 21,7, wyjście z okrążenia 6,1 | w trakcie uniku 20,4, drgnięcie (trafiony) 10,2, brak wytrzymałości 7,4, picie flaszki 4,6, parowanie 3,4, inna 1,1, riposta 0,6, mocny atak 0,4, szybki atak 0,1, blok 0,1, unieruchomienie 0,1, umiejętność 0,1 | 60% (n=2408) / 78% (n=1074) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 8,8 | 253,2 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 8,2 | 239,0 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 7,0 | 199,6 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 6,8 | 184,8 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 8 | 0 | 9,9 | 308,1 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 3 | 0 | 9,0 | 271,3 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 4 | 0 | 8,5 | 251,5 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 5 | 0 | 8,4 | 247,4 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 179507 | 19% |  |  |
| Fala przełamania | 107387 | 11% | 346 | 310 |
| Cięcie wichru | 98579 | 10% | 589 | 167 |
| Upadek kata | 81151 | 8% | 268 | 303 |
| Rozpłatanie | 79656 | 8% | 471 | 169 |
| Tąpnięcie | 70343 | 7% | 465 | 151 |
| Egzekucja | 69659 | 7% |  |  |
| Cięcie | 56360 | 6% |  |  |
| Krwawe cięcie | 49076 | 5% | 314 | 156 |
| Okrzyk bojowy | 37083 | 4% | 992 | 37 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×115.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 485966 | 33% | 2312 | 210 |
| Rozpłatanie | 444008 | 30% | 2403 | 185 |
| Fala przełamania | 136421 | 9% | 406 | 336 |
| Egzekucja | 102381 | 7% | 336 | 305 |
| Cięcie | 72971 | 5% |  |  |
| Zamach | 65202 | 4% |  |  |
| Okrzyk bojowy | 33026 | 2% | 581 | 57 |
| Riposta | 32817 | 2% |  |  |
| Potężne cięcie | 32706 | 2% |  |  |
| Szarża | 31532 | 2% | 511 | 62 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×11.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 7,1, mediana 7,0, najwięcej 9 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 17% | 43,5 | 170 | 0,8 |
| odblokowanie | 9% | 23,3 | 151 | 0,9 |
| klasa | 19% | 49,6 | 37 | 0,4 |
| nagroda | 5% | 12,8 | 161 | 0,6 |
| sklep | 51% | 133,8 | 122 | 1,0 |

Oferty sklepu dusz: widziane 134, kupione 97, odrzucone 0, za mało dusz 37.
Zamiany w slotach: 5,5 na podejście; najczęstsze: Tąpnięcie → Cięcie wichru ×7, Rozpłatanie → Rozpłatanie ×6, Tąpnięcie → Rozpłatanie ×5, Tąpnięcie → Krwawe cięcie ×5, Rozpłatanie → Kontra ×4, Cięcie wichru → Fala przełamania ×4.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 20 | 49,6 | 37 | 0,4 |
| Cięcie wichru | startowy | 13 | 45,3 | 167 | 0,8 |
| Rozpłatanie | startowy | 20 | 23,6 | 169 | 0,7 |
| Tąpnięcie | odblokowanie | 20 | 23,3 | 151 | 0,9 |
| Kontra | sklep | 8 | 46,4 | 27 | 1,0 |
| Fala przełamania | sklep | 7 | 49,4 | 310 | 1,2 |
| Szarża | sklep | 10 | 33,4 | 102 | 0,8 |
| Krwawe cięcie | sklep | 9 | 34,9 | 156 | 1,1 |
| Odtrącenie | nagroda | 6 | 51,3 | 20 | 0,9 |
| Wypad | sklep | 8 | 36,3 | 29 | 0,9 |
| Upadek kata | sklep | 7 | 38,3 | 303 | 1,1 |
| Uderzenie tarczą | sklep | 6 | 29,3 | 52 | 0,6 |
| Cięcie krzyżowe | nagroda | 2 | 58,5 | 125 | 0,4 |
| Okrzyk wojenny | sklep | 6 | 19,2 | – | 1,1 |
| Cięcie z wyskoku | sklep | 4 | 25,3 | 170 | 1,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,7, mediana 4,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 45% | 157,3 | 199 | 1,9 |
| odblokowanie | 33% | 115,9 | 148 | 2,8 |
| klasa | 8% | 29,1 | 57 | 1,3 |
| nagroda | 4% | 14,0 | 343 | 1,9 |
| sklep | 10% | 33,6 | 206 | 1,6 |

Oferty sklepu dusz: widziane 162, kupione 12, odrzucone 103, za mało dusz 47.
Zamiany w slotach: 2,2 na podejście; najczęstsze: Szarża → Egzekucja ×12, Cięcie wichru → Szarża ×3, Rozpłatanie → Tąpnięcie ×3, Egzekucja → Fala przełamania ×2, Szarża → Fala przełamania ×2, Aura niezłomności → Cięcie wichru ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | startowy | 19 | 126,5 | 185 | 2,8 |
| Cięcie wichru | startowy | 16 | 144,5 | 210 | 1,7 |
| Okrzyk bojowy | klasa | 20 | 29,1 | 57 | 1,3 |
| Szarża | sklep | 8 | 63,9 | 62 | 2,3 |
| Fala przełamania | sklep | 5 | 81,2 | 336 | 1,9 |
| Egzekucja | odblokowanie | 10 | 33,6 | 235 | 2,8 |
| Tąpnięcie | odblokowanie | 4 | 57,0 | 46 | 1,3 |
| Odtrącenie | sklep | 1 | 50,0 | 39 | 0,7 |
| Uderzenie tarczą | odblokowanie | 2 | 23,0 | 31 | 0,5 |
| Seria cięć | odblokowanie | 1 | 39,0 | 190 | 2,4 |
| Kontra | odblokowanie | 2 | 15,0 | 5 | 0,6 |
| Cięcie krzyżowe | sklep | 1 | 23,0 | 155 | 1,7 |
| Okrzyk wojenny | sklep | 1 | 11,0 | – | 0,0 |
| Krwawe cięcie | nagroda | 1 | 9,0 | 80 | 2,0 |
| Rozdarcie ran | sklep | 2 | 3,0 | 81 | 1,3 |
| Młynek | odblokowanie | 1 | 5,0 | 195 | 1,6 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 262,9, szybki atak 99,8, drgnięcie (trafiony) 85,1, unik 51,6, mocny atak 34,7, blok 33,3, flaszka (próba) 20,9, przełamana garda 9,9, parowanie 5,7, riposta 1,4.
Czas w bloku 14 s, w biegu 60 s, bez wytrzymałości (<10%) 241 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Tąpnięcie → +1 ×6, wzmocnienie: Oddech wojownika ×6, wzmocnienie: Siła tytana ×6, wzmocnienie: Ostrze gniewu ×5, wzmocnienie: Żelazna skóra ×4, wzmocnienie: Pijawka ×4, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×4, wzmocnienie: Mistrz parowania ×3.
Zakupy: ulepszenie broni ×20, ulepszenie: Cięcie wichru ×14, oferta: Rozpłatanie ×13, ulepszenie: Rozpłatanie ×12, ulepszenie: Tąpnięcie ×12, oferta: Szarża ×10, ulepszenie: Okrzyk bojowy ×10, oferta: Kontra ×8.
Punkty cech: Strength 41, Toughness 33, Dexterity 32, Intelligence 28.
Komunikaty gry: „OKRZYK!” ×952; „SZAŁ!” ×442; „GARDA PRZEŁAMANA!” ×209; „Garda wroga przełamana!” ×149; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×131; „Portale gasną – dobij pozostałe demony” ×105; „ZAMROŻENIE!” ×84; „POLEGŁEŚ” ×84.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 349,8, unik 139,6, drgnięcie (trafiony) 119,4, szybki atak 106,3, blok 47,4, mocny atak 41,6, flaszka (próba) 25,2, parowanie 16,9, przełamana garda 9,1, riposta 7,6.
Czas w bloku 24 s, w biegu 74 s, bez wytrzymałości (<10%) 216 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +4 ×10, wzmocnienie: Oddech wojownika ×9, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×6, wzmocnienie: Zwinność ×6, ulepszenie: Ulepszenie: Rozpłatanie → +3 ×6, wzmocnienie: Obrońca ×5, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×4, wzmocnienie: Siła tytana ×4.
Zakupy: ulepszenie: Rozpłatanie ×46, ulepszenie: Okrzyk bojowy ×34, ulepszenie broni ×26, ulepszenie: Cięcie wichru ×23, ulepszenie: Egzekucja ×19, ulepszenie: Szarża ×8, dodatkowa flaszka ×7, ulepszenie: Fala przełamania ×7.
Punkty cech: Toughness 63, Strength 99.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Puklerz, Sztylet parujący, Łuk refleksyjny, Przetrwanie, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Kostur burzy, Krwawa riposta, Pawęż, Kosa, Strażnik zaklęć, Pierścień riposty, Amulet przewodnika.
Komunikaty gry: „OKRZYK!” ×534; „SZAŁ!” ×448; „PAROWANIE!” ×311; „Postawa przełamana – RIPOSTA!” ×228; „GARDA PRZEŁAMANA!” ×198; „Garda wroga przełamana!” ×187; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×164; „RIPOSTA!” ×131.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×433, zablokowany ruch ×19, garda nie zatrzymała ciosu ×2, parowanie w oknie nie zadziałało ×1.
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (7,8; 0,1) (w 2 podejściach)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,1) (w 2 podejściach)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; 0,0) (w 2 podejściach)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 136°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-5,6; -0,5)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-6,9; -9,1)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,2; -9,1)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (6,7; -12,8)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-5,6; -7,9)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; 0,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,2)
  - Wiedźma wichru: „Wiedźma wichru: CycloneEye” przeszedł przez gardę (kąt 128°)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×879, parowanie w oknie nie zadziałało ×27, uśpiony wróg blokuje koniec piętra ×17, zablokowany ruch ×15, garda nie zatrzymała ciosu ×8.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,8; -0,1) (w 2 podejściach)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 107°) (w 2 podejściach)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; 2,2) (w 2 podejściach)
  - Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 129°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 161°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 147°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 135°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 78°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 94°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 79°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** zablokowany ruch średnio 1,0× na podejście (geometria areny/kolizje). Przykłady: piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (7,8; 0,1); piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,1); piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; 0,0).
- **Do sprawdzenia (P3):** 1× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Pan Sfory: „Pan Sfory: Combo” trafił w aktywnym oknie parowania (kąt do źródła 0°)
- **Do sprawdzenia (P3):** 2× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 136°)
- Parowanie zwykłych ciosów: 65% ciosów mimo to rani (n=120) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 41% ciosów mimo to rani (n=122) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: 42% ciosów mimo to rani (n=148) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 76% ciosów mimo to rani (n=283) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=376) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 30% pocisków, przed którymi był unik (n=432).
- Blok pocisku: 85% ciosów mimo to rani (n=265) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 62% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1974) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=373).
- Telegraf „nieparowalny” daje czas: tylko 10% decyzji zapada mniej niż 0,1 s przed trafieniem (n=644).
- Telegraf „nieblokowalny” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=50).
- Telegraf „obszarowy” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=724).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 70 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 20 podejściach. Średnio dochodzę do piętra 9,1 – na razie to ściana, nie krzywa.
- Czuję progres: druga połowa podejść średnio piętro 9,9 wobec 8,3 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 42% udanych (n=512). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (30,7 trafień, 45,6 s), Władca Bramy (25,9 trafień, 43,3 s), Taranowiec (elita) (13,3 trafień, 13,1 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,1), Siarkowy pomiot (1,1), Kultysta (1,2) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 43 s i 26 trafień do zabicia.
- Długie starcie: Pan Sfory – średnio 46 s i 31 trafień do zabicia.
- Długie starcie: Królowa Topieli – średnio 100 s i 73 trafień do zabicia.
- Długie starcie: Rzeźnik – średnio 91 s i 94 trafień do zabicia.
- Mój nemezis: Królowa Topieli – 9 z 20 śmierci (najczęściej „Królowa Topieli: CycloneEye”).
- 10/20 razy zginąłem z flaszkami w kieszeni. 3 z tych śmierci to utrata ≥50% HP w 3 s, a 17% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 85% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 336 obrażeń na użycie.
- Skill startowy to 45% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 14%. Różnych skilli na podejście: średnio 4,7.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (8×, śr. piętro 9,9). Inne: Dwuręczny + Cięcie wichru 5× (8,4), Miecz i tarcza + Rozpłatanie 4× (8,5), Dwuręczny + Rozpłatanie 3× (9,0).
- Unik to moje główne narzędzie: 139,6 na podejście, w tym 27,7 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 78% przypadków.
- Gra nie wykonała uniku od razu średnio 48,2× na podejście – najczęściej: w trakcie poprzedniego uniku 20,4, drgnięcie (trafiony) 10,2, brak wytrzymałości 7,4. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (18% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (119 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

