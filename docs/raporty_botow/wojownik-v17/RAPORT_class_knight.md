# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 18:39. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 7,6 | 8,0 | 14,0 min | 209,9 | 44526 / 4752 | 15,7 | 1% |
| Gracz H&S / souls-like | 20 | 0 (0%) | 8,8 | 9,0 | 17,7 min | 264,1 | 73124 / 6080 | 17,1 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 9 8 9 9 7 6 8 9 9 7 6 8 5 6 7 8 6 6 9 9
- Gracz H&S / souls-like: 7 8 4 15 13 5 9 9 8 5 11 11 9 8 11 11 4 5 13 9

## Gdzie i od czego giną

- **Tester QA** — piętra: 5. V. Psiarnia Piekieł ×1, 6. VI. Pan Sfory ×5, 7. VII. Zatopione Krużganki ×3, 8. VIII. Zatopione Krużganki ×4, 9. IX. Królowa Topieli ×7.
  Ostatni cios: Pan Sfory – „Pan Sfory: Combo” ×4; Królowa Topieli – „Królowa Topieli: Volley” ×2; Królowa Topieli – „Królowa Topieli: WindStep” ×2; Legionista – „Legionista: ShieldBash” ×2; Kultysta – „Kula ognia” ×1; Królowa Topieli – „Królowa Topieli: Flood” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 9/20.
  Ostatnie 3 s przed śmiercią: średnio 35% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/20). Picie: 384 prób, 313 wypitych (18% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 11. XI. Rzeźnia ×4, 13. XIII. Kamienne Trzewia ×2, 15. XV. Tytan Otchłani ×1, 4. IV. Psiarnia Piekieł ×2, 5. V. Psiarnia Piekieł ×3, 7. VII. Zatopione Krużganki ×1, 8. VIII. Zatopione Krużganki ×3, 9. IX. Królowa Topieli ×4.
  Ostatni cios: Kultysta topieli – „Kultysta topieli” ×3; Kultysta – „Kula ognia” ×3; Kolos – „Kolos: Combo” ×2; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×2; Kamienny chochlik – „Pazury” ×1; Tytan Otchłani – „Tytan Otchłani: Combo” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 16/20.
  Ostatnie 3 s przed śmiercią: średnio 39% maks. HP straconych (śmierć z ≥50% HP w 3 s: 6/20). Picie: 428 prób, 342 wypitych (20% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 106 s | 10 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 111 s | 65 | 0,1 | 40,9 |
| Tester QA | 3. III. Władca Bramy | 21 | 20 | 33 s | 316 | 0,9 | 7,4 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 113 s | 106 | 0,2 | 44,4 |
| Tester QA | 5. V. Psiarnia Piekieł | 24 | 19 | 101 s | 316 | 1,0 | 36,6 |
| Tester QA | 6. VI. Pan Sfory | 35 | 14 | 45 s | 685 | 2,9 | 4,7 |
| Tester QA | 7. VII. Zatopione Krużganki | 25 | 11 | 110 s | 604 | 2,0 | 31,1 |
| Tester QA | 8. VIII. Zatopione Krużganki | 20 | 7 | 133 s | 878 | 2,8 | 33,8 |
| Tester QA | 9. IX. Królowa Topieli | 21 | 0 | 0 s | 980 | 3,0 | 0,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 104 s | 8 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 22 | 20 | 111 s | 142 | 0,6 | 39,7 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 21 | 20 | 45 s | 298 | 0,8 | 8,8 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 25 | 18 | 107 s | 253 | 0,8 | 40,5 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 24 | 15 | 95 s | 357 | 0,9 | 29,9 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 15 | 15 | 42 s | 469 | 2,6 | 6,7 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 18 | 14 | 100 s | 461 | 1,4 | 35,8 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 24 | 11 | 141 s | 728 | 1,8 | 38,4 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 26 | 7 | 101 s | 1124 | 3,6 | 1,1 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 10 | 7 | 125 s | 669 | 1,4 | 54,7 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 17 | 3 | 129 s | 821 | 1,6 | 29,7 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 3 | 3 | 133 s | 282 | 0,7 | 7,0 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 8 | 1 | 156 s | 1172 | 1,9 | 28,5 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 1 | 1 | 131 s | 296 | 0,0 | 66,0 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 3 | 0 | 0 s | 1315 | 3,7 | 0,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 3 ×1, piętro 5 ×4, piętro 6 ×16, piętro 7 ×11, piętro 8 ×9, piętro 9 ×14; wyzwań (3 porażki na piętrze): 7 – piętro 9.
**Gracz H&S / souls-like** – ponowne próby: piętro 2 ×2, piętro 3 ×1, piętro 4 ×5, piętro 5 ×6, piętro 7 ×3, piętro 8 ×10, piętro 9 ×15, piętro 10 ×3, piętro 11 ×10, piętro 13 ×5, piętro 15 ×2; wyzwań (3 porażki na piętrze): 3 – piętro 7, piętro 8, piętro 15.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Królowa Topieli | 7 | 97,0 s | 66,3 | 1764 | 48438 | 6920 | 9 |
| Pan Sfory | 29 | 38,8 s | 25,8 | 1273 | 28326 | 977 | 5 |
| Kultysta | 1188 | 0,7 s | 1,2 | 1185 | 19563 | 16 | 4 |
| Wiedźma wichru | 215 | 2,4 s | 2,0 | 1245 | 14910 | 68 | 3 |
| Ogar otchłani | 1137 | 1,2 s | 1,7 | 1409 | 14366 | 12 | 0 |
| Kolos | 283 | 3,9 s | 3,5 | 272 | 12961 | 45 | 3 |
| Kultysta topieli | 263 | 1,0 s | 1,4 | 636 | 12758 | 48 | 3 |
| Władca Bramy | 40 | 34,9 s | 20,9 | 557 | 11951 | 299 | 0 |
| Miotacz głazów | 36 | 3,6 s | 2,7 | 158 | 6994 | 184 | 1 |
| Legionista | 319 | 6,5 s | 5,1 | 731 | 6768 | 21 | 3 |
| Harpia | 217 | 1,3 s | 2,2 | 449 | 5767 | 25 | 1 |
| Taranowiec | 420 | 3,1 s | 2,7 | 390 | 5554 | 13 | 1 |
| Czerw | 2963 | 0,6 s | 1,4 | 1357 | 4966 | 2 | 2 |
| Tytan Otchłani | 0 | – | – | 62 | 3945 | – | 1 |
| Siarkowy pomiot | 678 | 0,2 s | 1,2 | 126 | 3804 | 5 | 0 |
| Ognisty chochlik | 634 | 0,6 s | 1,3 | 222 | 2342 | 3 | 1 |
| Wodny chochlik | 599 | 0,6 s | 1,3 | 194 | 1711 | 3 | 0 |
| Kamienny chochlik | 659 | 0,4 s | 1,2 | 152 | 1421 | 2 | 1 |
| Grzebacz | 13 | 2,0 s | 2,7 | 34 | 1403 | 108 | 0 |
| Gnilec | 51 | 5,5 s | 4,7 | 74 | 1389 | 26 | 0 |
| Chochlik wichru | 608 | 1,0 s | 1,7 | 155 | 1371 | 2 | 1 |
| Ożywiony nietoperz | 300 | 0,4 s | 1,1 | 189 | 935 | 3 | 0 |
| Rzeźnik | 3 | 129,2 s | 62,0 | 77 | 832 | 277 | 0 |
| Gnilec (odłamek) | 99 | 2,6 s | 2,4 | 70 | 800 | 8 | 0 |
| Tkaczka | 18 | 4,3 s | 2,9 | 51 | 794 | 44 | 0 |
| Ofiarnik | 70 | 0,9 s | 1,8 | 97 | 711 | 10 | 0 |
| Gargulec | 29 | 2,4 s | 3,4 | 20 | 625 | 21 | 0 |
| Pajączek | 15 | 2,6 s | 2,0 | 51 | 410 | 27 | 0 |
| Kolos żaru | 6 | 3,6 s | 4,2 | 7 | 335 | 56 | 0 |
| Cień | 7 | 0,9 s | 2,4 | 13 | 271 | 34 | 1 |
| Upiór | 2 | 2,3 s | 3,5 | 26 | 217 | 108 | 0 |
| Ognisty chochlik (elita) | 68 | 3,9 s | 4,4 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 42 | 4,9 s | 4,7 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 28 | 5,0 s | 3,8 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 23 | 1,8 s | 2,9 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 13 | 11,0 s | 10,8 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 10 | 5,5 s | 5,9 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 4 | 8,7 s | 4,3 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 28 | 8,4 s | 5,9 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 5 | 26,2 s | 17,6 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 8 | 37,5 s | 26,0 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 5 | 10,0 s | 6,0 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 2 | 45,8 s | 26,5 | ↑ | ↑ | ↑ |  |
| Worek jaj | 17 | 0,0 s | 1,0 | 0 | 0 | 0 | 0 |
| Gargulec (elita) | 1 | 22,8 s | 19,0 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 2 | 15,2 s | 10,5 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 1 | 10,9 s | 9,0 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 3 | 5,0 s | 5,7 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2338 | 74% | 4% | 4% | 16% | 2% |
| ciężki | 365 | 61% | 1% | 5% | 29% | 5% |
| nieparowalny | 1474 | 71% | 6% | 0% | 17% | 5% |
| nieblokowalny | 248 | 80% | 0% | 0% | 20% | 0% |
| obszarowy | 1078 | 86% | 10% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3388 | 57% | 2% | 2% | 38% | 2% |
| ciężki | 456 | 52% | 1% | 1% | 46% | 0% |
| nieparowalny | 1773 | 67% | 5% | 0% | 25% | 3% |
| nieblokowalny | 185 | 74% | 0% | 0% | 26% | 0% |
| obszarowy | 1741 | 86% | 10% | 0% | 0% | 4% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2189 | 26% | 38% | 2% | 19% | 14% |
| Tester QA (0,30 s) | ciężki | 394 | 1% | 0% | 2% | 10% | 87% |
| Tester QA (0,30 s) | nieparowalny | 530 | 5% | 5% | 13% | 36% | 41% |
| Tester QA (0,30 s) | nieblokowalny | 37 | 5% | 0% | 5% | 19% | 70% |
| Tester QA (0,30 s) | obszarowy | 850 | 2% | 3% | 3% | 26% | 66% |
| Tester QA (0,30 s) | pocisk | 1410 | 4% | 17% | 14% | 20% | 45% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3758 | 2% | 33% | 38% | 9% | 18% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 506 | 1% | 2% | 2% | 7% | 88% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 664 | 3% | 1% | 5% | 23% | 67% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 80 | 3% | 0% | 1% | 13% | 84% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 1179 | 1% | 1% | 4% | 25% | 68% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2569 | 2% | 12% | 10% | 16% | 60% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 1290 |
| Pajączek: Pajączek | 0,31 s | 78 |
| Wodny chochlik: Pazury | 0,33 s | 340 |
| Chochlik wichru: Pazury | 0,33 s | 266 |
| Ognisty chochlik: Pazury | 0,33 s | 339 |
| Czerw: Ugryzienie | 0,33 s | 2176 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 303 |
| Kamienny chochlik: Pazury | 0,33 s | 265 |
| Władca Bramy: Cięcie w szale | 0,41 s | 44 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 606 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 63% (n=188) | 0% (n=2) | 62% (n=161) | 100% (n=9) | 50% (n=182) | 83% (n=260) |
| blok (eksperyment) | 74% (n=116) | 83% (n=30) | 87% (n=53) | 100% (n=11) | 70% (n=43) | 64% (n=56) |
| brak reakcji (eksperyment) | 90% (n=115) | 100% (n=14) | 96% (n=57) | 100% (n=5) | 98% (n=43) | 83% (n=48) |
| niezauważony | 88% (n=460) | – | 98% (n=170) | 100% (n=117) | 100% (n=253) | – |
| parowanie | 69% (n=68) | 40% (n=15) | – | – | – | – |
| parowanie (eksperyment) | 74% (n=62) | 53% (n=17) | 100% (n=21) | – | 100% (n=22) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 90% (n=69) |
| unik | 57% (n=453) | 52% (n=166) | 34% (n=237) | 44% (n=81) | – | 69% (n=150) |
| unik (eksperyment) | 63% (n=159) | 45% (n=40) | 47% (n=62) | 43% (n=7) | 100% (n=73) | 88% (n=41) |
| wyjście z zasięgu | 93% (n=419) | 99% (n=69) | 99% (n=192) | 100% (n=17) | 99% (n=404) | – |
| wyjście z zasięgu (eksperyment) | 92% (n=123) | 100% (n=12) | 100% (n=50) | 100% (n=1) | 98% (n=54) | 88% (n=26) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 58,8 | reakcja na cios wręcz 63,8, unik przed pociskiem 25,5, wyjście z okrążenia 5,6 | brak wytrzymałości 9,2, drgnięcie (trafiony) 9,1, w trakcie uniku 6,4, picie flaszki 3,6, parowanie 0,9, inna 0,6, umiejętność 0,4, riposta 0,2, mocny atak 0,2, szybki atak 0,2, blok 0,1, przełamana garda 0,1 | 46% (n=1275) / 73% (n=510) |
| Gracz H&S / souls-like | 138,9 | reakcja na cios wręcz 141,1, unik przed pociskiem 49,3, doskok do odsłoniętego celu 21,0, wyjście z okrążenia 4,4 | w trakcie uniku 27,7, drgnięcie (trafiony) 12,1, brak wytrzymałości 9,3, picie flaszki 5,6, inna 0,9, parowanie 0,8, umiejętność 0,2, mocny atak 0,2, blok 0,2, szybki atak 0,1, unieruchomienie 0,1, riposta 0,1 | 62% (n=2821) / 78% (n=986) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 8,6 | 242,4 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 7,8 | 220,4 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 7,2 | 196,0 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 6,6 | 180,8 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 15 | 0 | 9,1 | 274,7 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 3 | 0 | 8,7 | 272,0 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 1 | 0 | 8,0 | 231,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 4,0 | 113,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 183674 | 21% |  |  |
| Cięcie wichru | 80531 | 9% | 505 | 159 |
| Tąpnięcie | 76921 | 9% | 449 | 171 |
| Rozpłatanie | 74617 | 8% | 460 | 162 |
| Egzekucja | 65688 | 7% |  |  |
| Cięcie | 60211 | 7% |  |  |
| Krwawe cięcie | 59297 | 7% | 367 | 162 |
| Fala przełamania | 57499 | 6% | 232 | 248 |
| Upadek kata | 43936 | 5% | 162 | 271 |
| Cięcie z wyskoku | 40630 | 5% | 318 | 128 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×195.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 523532 | 36% | 2150 | 244 |
| Rozpłatanie | 374274 | 26% | 1454 | 257 |
| Fala przełamania | 151508 | 10% | 392 | 387 |
| Egzekucja | 149421 | 10% | 274 | 545 |
| Zamach | 124763 | 9% |  |  |
| Okrzyk bojowy | 34774 | 2% | 518 | 67 |
| Tąpnięcie | 19872 | 1% | 360 | 55 |
| Cięcie | 16023 | 1% |  |  |
| Szarża | 14637 | 1% | 308 | 48 |
| Cios pięścią | 13363 | 1% |  |  |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 6,9, mediana 7,0, najwięcej 9 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 17% | 41,7 | 163 | 0,8 |
| odblokowanie | 9% | 22,5 | 171 | 1,0 |
| klasa | 20% | 49,4 | 39 | 0,3 |
| nagroda | 8% | 19,6 | 95 | 0,7 |
| sklep | 47% | 118,6 | 106 | 0,9 |

Oferty sklepu dusz: widziane 131, kupione 90, odrzucone 0, za mało dusz 41.
Zamiany w slotach: 5,4 na podejście; najczęstsze: Tąpnięcie → Rozpłatanie ×8, Tąpnięcie → Cięcie wichru ×6, Rozpłatanie → Tąpnięcie ×6, Cięcie wichru → Tąpnięcie ×5, Tąpnięcie → Cięcie z wyskoku ×4, Cięcie wichru → Okrzyk wojenny ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 20 | 49,4 | 39 | 0,3 |
| Cięcie wichru | startowy | 12 | 42,1 | 159 | 0,8 |
| Rozpłatanie | startowy | 14 | 32,9 | 162 | 0,9 |
| Tąpnięcie | odblokowanie | 20 | 22,5 | 171 | 1,0 |
| Krwawe cięcie | sklep | 9 | 40,8 | 162 | 1,1 |
| Wypad | sklep | 6 | 54,7 | 20 | 0,7 |
| Cięcie z wyskoku | sklep | 9 | 35,3 | 128 | 0,7 |
| Odtrącenie | sklep | 9 | 32,6 | 23 | 0,8 |
| Kontra | sklep | 5 | 47,8 | 25 | 0,9 |
| Uderzenie tarczą | sklep | 6 | 38,8 | 60 | 1,0 |
| Fala przełamania | sklep | 5 | 46,4 | 248 | 0,8 |
| Cięcie krzyżowe | sklep | 8 | 28,1 | 140 | 0,8 |
| Okrzyk wojenny | sklep | 6 | 32,5 | – | 0,7 |
| Upadek kata | sklep | 4 | 40,5 | 271 | 1,1 |
| Szarża | nagroda | 2 | 10,0 | 49 | 2,0 |
| Piekielna szarża | sklep | 2 | 10,0 | 148 | 1,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,1, mediana 5,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 50% | 139,7 | 227 | 1,8 |
| odblokowanie | 31% | 86,5 | 243 | 2,3 |
| klasa | 9% | 25,9 | 67 | 1,2 |
| nagroda | 1% | 3,9 | 265 | 1,7 |
| sklep | 8% | 23,6 | 304 | 1,7 |

Oferty sklepu dusz: widziane 155, kupione 10, odrzucone 99, za mało dusz 46.
Zamiany w slotach: 2,6 na podejście; najczęstsze: Szarża → Egzekucja ×15, Cięcie wichru → Szarża ×7, Rozpłatanie → Tąpnięcie ×6, Szarża → Krwawe cięcie ×2, Rozpłatanie → Cięcie krzyżowe ×2, Egzekucja → Fala przełamania ×2.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 17 | 126,5 | 244 | 1,6 |
| Rozpłatanie | startowy | 19 | 76,5 | 257 | 2,6 |
| Okrzyk bojowy | klasa | 20 | 25,9 | 67 | 1,2 |
| Fala przełamania | sklep | 5 | 78,4 | 387 | 1,7 |
| Tąpnięcie | odblokowanie | 9 | 40,0 | 55 | 1,2 |
| Szarża | sklep | 10 | 30,8 | 48 | 1,4 |
| Egzekucja | odblokowanie | 12 | 22,8 | 445 | 2,9 |
| Odtrącenie | sklep | 1 | 38,0 | 34 | 1,4 |
| Kontra | sklep | 1 | 34,0 | 10 | 2,3 |
| Cięcie krzyżowe | nagroda | 2 | 12,5 | 197 | 1,1 |
| Krwawe cięcie | odblokowanie | 1 | 16,0 | 206 | 3,0 |
| Uderzenie tarczą | odblokowanie | 1 | 6,0 | 52 | 1,7 |
| Rozdarcie ran | odblokowanie | 1 | 5,0 | 74 | 0,6 |
| Młynek | odblokowanie | 1 | 5,0 | 273 | 1,6 |
| Seria cięć | odblokowanie | 1 | 5,0 | 111 | 1,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 251,7, szybki atak 100,9, drgnięcie (trafiony) 79,6, unik 58,8, mocny atak 35,7, blok 34,7, flaszka (próba) 19,2, przełamana garda 9,4, parowanie 5,4, riposta 1,5.
Czas w bloku 16 s, w biegu 56 s, bez wytrzymałości (<10%) 229 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Oddech wojownika ×9, wzmocnienie: Żelazna skóra ×8, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×5, wzmocnienie: Zwinność ×5, wzmocnienie: Czujność ×5, wzmocnienie: Siła tytana ×4, ulepszenie: Ulepszenie: Tąpnięcie → +2 ×4, nowy skill: Okrzyk wojenny ×3.
Zakupy: ulepszenie broni ×20, ulepszenie: Rozpłatanie ×16, ulepszenie: Tąpnięcie ×15, ulepszenie: Cięcie wichru ×11, oferta: Rozpłatanie ×10, ulepszenie: Okrzyk bojowy ×9, oferta: Krwawe cięcie ×8, ulepszenie: Cięcie z wyskoku ×8.
Punkty cech: Strength 39, Toughness 34, Dexterity 31, Intelligence 27.
Komunikaty gry: „OKRZYK!” ×934; „SZAŁ!” ×438; „GARDA PRZEŁAMANA!” ×197; „Garda wroga przełamana!” ×136; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×129; „Portale gasną – dobij pozostałe demony” ×109; „Okrzyk wojenny: +29% obrażeń” ×104; „POLEGŁEŚ” ×75.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 279,5, unik 138,9, drgnięcie (trafiony) 101,2, szybki atak 87,9, mocny atak 39,0, blok 33,1, flaszka (próba) 21,4, przełamana garda 8,1, parowanie 4,2, riposta 1,7.
Czas w bloku 16 s, w biegu 67 s, bez wytrzymałości (<10%) 209 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +4 ×9, ulepszenie: Ulepszenie: Egzekucja → +3 ×8, ulepszenie: Ulepszenie: Rozpłatanie → +3 ×7, ulepszenie: Ulepszenie: Egzekucja → +2 ×7, wzmocnienie: Oddech wojownika ×6, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×5, wzmocnienie: Żelazna skóra ×5, ulepszenie: Ulepszenie: Egzekucja → +4 ×5.
Zakupy: ulepszenie: Rozpłatanie ×31, ulepszenie: Okrzyk bojowy ×29, ulepszenie broni ×28, ulepszenie: Cięcie wichru ×27, ulepszenie: Egzekucja ×18, ulepszenie: Szarża ×12, dodatkowa flaszka ×12, ulepszenie: Tąpnięcie ×7.
Punkty cech: Toughness 60, Strength 95.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Puklerz, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Kosa, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Przetrwanie, Amulet przewodnika.
Komunikaty gry: „OKRZYK!” ×463; „SZAŁ!” ×406; „Garda wroga przełamana!” ×194; „GARDA PRZEŁAMANA!” ×177; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×169; „Portale gasną – dobij pozostałe demony” ×121; „ZAMROŻENIE!” ×85; „POLEGŁEŚ” ×82.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×391, zablokowany ruch ×10, garda nie zatrzymała ciosu ×3, parowanie w oknie nie zadziałało ×1.
  - piętro 2 „II. Kult Bramy”: brak postępu przy (5,6; 0,3)
  - Wodny chochlik: „Pazury” przeszedł przez gardę (kąt 174°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 139°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,8; 0,0)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,8; -8,1)
  - piętro 7 „VII. Zatopione Krużganki”: brak postępu przy (7,8; -9,1)
  - Kultysta topieli: „Kultysta topieli” trafił w aktywnym oknie parowania (kąt do źródła 129°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,8; 0,1)
  - Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 143°)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-6,7; -6,9)
  - piętro 3 „III. Władca Bramy”: brak postępu przy (0,0; 0,8)
  - piętro 5 „V. Psiarnia Piekieł”: brak postępu przy (9,1; 8,3)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×798, zablokowany ruch ×41, uśpiony wróg blokuje koniec piętra ×13, garda nie zatrzymała ciosu ×4, parowanie w oknie nie zadziałało ×4.
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; 2,2) (w 3 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 156°)
  - Wiedźma wichru: „Wiedźma wichru: CycloneEye” przeszedł przez gardę (kąt 124°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,1)
  - Miotacz głazów: „Miotacz głazów” przeszedł przez gardę (kąt 166°)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,1; 2,4)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-5,6; -0,1)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,8; 7,4)
  - Ogar otchłani: „Ogar otchłani: Combo” przeszedł przez gardę (kąt 177°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 81°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (10 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 1× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Kultysta topieli: „Kultysta topieli” trafił w aktywnym oknie parowania (kąt do źródła 129°)
- **Do sprawdzenia (P3):** 3× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Wodny chochlik: „Pazury” przeszedł przez gardę (kąt 174°)
- Parowanie zwykłych ciosów: 72% ciosów mimo to rani (n=130) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 44% ciosów mimo to rani (n=88) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: 50% ciosów mimo to rani (n=206) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 67% ciosów mimo to rani (n=304) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 99% ciosów mimo to rani (n=458) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 27% pocisków, przed którymi był unik (n=510).
- Blok pocisku: 80% ciosów mimo to rani (n=316) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 65% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2189) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=394).
- Telegraf „nieparowalny” daje czas: tylko 10% decyzji zapada mniej niż 0,1 s przed trafieniem (n=530).
- Telegraf „nieblokowalny” daje czas: tylko 5% decyzji zapada mniej niż 0,1 s przed trafieniem (n=37).
- Telegraf „obszarowy” daje czas: tylko 5% decyzji zapada mniej niż 0,1 s przed trafieniem (n=850).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 74 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 20 podejściach. Średnio dochodzę do piętra 8,8 – na razie to ściana, nie krzywa.
- Czuję progres: druga połowa podejść średnio piętro 9,2 wobec 8,3 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 41% udanych (n=162). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (25,5 trafień, 37,9 s), Władca Bramy (24,6 trafień, 41,5 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Siarkowy pomiot (1,2), Ożywiony nietoperz (1,2), Kamienny chochlik (1,2) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 41 s i 25 trafień do zabicia.
- Długie starcie: Królowa Topieli – średnio 97 s i 66 trafień do zabicia.
- Długie starcie: Gnilec (elita) – średnio 46 s i 27 trafień do zabicia.
- Długie starcie: Rzeźnik – średnio 129 s i 62 trafień do zabicia.
- 16/20 razy zginąłem z flaszkami w kieszeni. 6 z tych śmierci to utrata ≥50% HP w 3 s, a 20% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 88% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Egzekucja”: 545 obrażeń na użycie.
- Skill startowy to 50% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 10%. Różnych skilli na podejście: średnio 5,1.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (15×, śr. piętro 9,1). Inne: Miecz i tarcza + Rozpłatanie 3× (8,7), Miecz i tarcza + Cięcie wichru 1× (8,0), Dwuręczny + Rozpłatanie 1× (4,0).
- Unik to moje główne narzędzie: 138,9 na podejście, w tym 25,3 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 78% przypadków.
- Gra nie wykonała uniku od razu średnio 57,0× na podejście – najczęściej: w trakcie poprzedniego uniku 27,7, drgnięcie (trafiony) 12,1, brak wytrzymałości 9,3. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (20% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (101 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

