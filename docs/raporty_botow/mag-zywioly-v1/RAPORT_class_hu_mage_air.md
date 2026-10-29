# Raport botów grających — Mag powietrza

Wygenerowano: 2026-10-29 12:53. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (10 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (10 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 10 | 3 (30%) | 19,8 | 16,0 | 39,6 min | 874,4 | 363370 / 4094 | 9,5 | 5% |
| Gracz H&S / souls-like | 10 | 6 (60%) | 24,8 | 30,0 | 49,4 min | 1213,8 | 597659 / 3541 | 5,2 | 7% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 16 ★ 13 15 16 14 14 20 ★ ★
- Gracz H&S / souls-like: 20 ★ ★ 13 ★ 16 ★ ★ ★ 19

## Gdzie i od czego giną

- **Tester QA** — piętra: 13. XIII. Kamienne Trzewia ×1, 14. XIV. Kamienne Trzewia ×2, 15. XV. Tytan Otchłani ×1, 16. XVI. Wieża Burz ×2, 20. XX. Kostnica ×1.
  Ostatni cios: Miotacz głazów – „Miotacz głazów: Rockfall” ×2; Cień – „Cień: Combo” ×2; Tytan Otchłani – „Tytan Otchłani: StoneSpikes” ×1; Tkaczka – „Tkaczka: Cocoon” ×1; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×1. Śmierć ciosem ≥40% maks. HP: 0/7. Zginął mając flaszki: 7/7.
  Ostatnie 3 s przed śmiercią: średnio 36% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/7). Picie: 285 prób, 95 wypitych (67% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 13. XIII. Kamienne Trzewia ×1, 16. XVI. Wieża Burz ×1, 19. XIX. Kostnica ×1, 20. XX. Kostnica ×1.
  Ostatni cios: Kolos – „Kolos” ×1; Miotacz głazów – „Miotacz głazów” ×1; Gnilec – „Gnilec: Vomit” ×1; Legionista – „Legionista: ShieldBash” ×1. Śmierć ciosem ≥40% maks. HP: 0/4. Zginął mając flaszki: 4/4.
  Ostatnie 3 s przed śmiercią: średnio 49% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/4). Picie: 354 prób, 52 wypitych (85% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 10 | 10 | 107 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 10 | 10 | 111 s | 0 | 0,0 | 41,4 |
| Tester QA | 3. III. Władca Bramy | 10 | 10 | 20 s | 52 | 0,0 | 7,0 |
| Tester QA | 4. IV. Psiarnia Piekieł | 10 | 10 | 112 s | 0 | 0,0 | 44,7 |
| Tester QA | 5. V. Psiarnia Piekieł | 10 | 10 | 95 s | 2 | 0,0 | 41,3 |
| Tester QA | 6. VI. Pan Sfory | 10 | 10 | 31 s | 42 | 0,2 | 4,6 |
| Tester QA | 7. VII. Zatopione Krużganki | 10 | 10 | 104 s | 9 | 0,0 | 42,2 |
| Tester QA | 8. VIII. Zatopione Krużganki | 10 | 10 | 129 s | 24 | 0,2 | 52,0 |
| Tester QA | 9. IX. Królowa Topieli | 13 | 10 | 75 s | 192 | 0,8 | 2,2 |
| Tester QA | 10. X. Rzeźnia | 12 | 10 | 111 s | 132 | 0,4 | 54,3 |
| Tester QA | 11. XI. Rzeźnia | 17 | 10 | 135 s | 378 | 0,9 | 50,2 |
| Tester QA | 12. XII. Rzeźnik | 10 | 10 | 51 s | 124 | 0,5 | 5,8 |
| Tester QA | 13. XIII. Kamienne Trzewia | 13 | 9 | 142 s | 214 | 0,2 | 58,2 |
| Tester QA | 14. XIV. Kamienne Trzewia | 14 | 7 | 140 s | 401 | 0,5 | 50,9 |
| Tester QA | 15. XV. Tytan Otchłani | 11 | 6 | 46 s | 583 | 1,9 | 1,6 |
| Tester QA | 16. XVI. Wieża Burz | 11 | 4 | 165 s | 415 | 0,4 | 67,5 |
| Tester QA | 17. XVII. Wieża Burz | 5 | 4 | 155 s | 274 | 1,0 | 74,8 |
| Tester QA | 18. XVIII. Matka Burz | 4 | 4 | 121 s | 119 | 1,3 | 2,0 |
| Tester QA | 19. XIX. Kostnica | 4 | 4 | 155 s | 5 | 0,0 | 95,8 |
| Tester QA | 20. XX. Kostnica | 6 | 3 | 186 s | 552 | 1,3 | 71,7 |
| Tester QA | 21. XXI. Król Kości | 3 | 3 | 57 s | 12 | 0,0 | 8,3 |
| Tester QA | 22. XXII. Chór Cieni | 3 | 3 | 161 s | 139 | 0,0 | 102,3 |
| Tester QA | 23. XXIII. Chór Cieni | 3 | 3 | 168 s | 126 | 0,0 | 105,3 |
| Tester QA | 24. XXIV. Bezimienny Cień | 3 | 3 | 82 s | 0 | 0,0 | 2,0 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 3 | 3 | 179 s | 0 | 0,0 | 104,3 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 3 | 3 | 187 s | 128 | 0,0 | 116,7 |
| Tester QA | 27. XXVII. Kowal Piekieł | 3 | 3 | 72 s | 308 | 0,7 | 2,0 |
| Tester QA | 28. XXVIII. Tron Arcydemona | 3 | 3 | 195 s | 190 | 0,0 | 137,3 |
| Tester QA | 29. XXIX. Tron Arcydemona | 3 | 3 | 175 s | 10 | 0,0 | 129,0 |
| Tester QA | 30. XXX. Arcydiabeł | 3 | 3 | 114 s | 202 | 0,3 | 3,7 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 10 | 10 | 102 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 10 | 10 | 116 s | 0 | 0,0 | 40,8 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 10 | 10 | 15 s | 8 | 0,0 | 7,0 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 10 | 10 | 100 s | 0 | 0,0 | 45,4 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 10 | 10 | 96 s | 0 | 0,0 | 40,4 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 10 | 10 | 22 s | 24 | 0,1 | 5,0 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 10 | 10 | 102 s | 2 | 0,0 | 40,7 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 10 | 10 | 126 s | 2 | 0,0 | 52,9 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 10 | 10 | 61 s | 129 | 0,3 | 2,6 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 11 | 10 | 105 s | 64 | 0,3 | 54,9 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 13 | 10 | 127 s | 285 | 0,9 | 56,5 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 10 | 10 | 32 s | 3 | 0,0 | 4,6 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 12 | 9 | 136 s | 162 | 0,6 | 58,4 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 11 | 9 | 149 s | 171 | 0,4 | 64,9 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 9 | 9 | 35 s | 189 | 0,4 | 2,6 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 12 | 8 | 157 s | 269 | 0,4 | 70,7 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 9 | 8 | 153 s | 98 | 0,0 | 82,6 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 8 | 8 | 109 s | 124 | 0,1 | 2,3 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 10 | 7 | 156 s | 276 | 0,5 | 81,3 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 12 | 6 | 179 s | 351 | 0,2 | 78,5 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 6 | 6 | 73 s | 112 | 0,0 | 11,3 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 6 | 6 | 175 s | 175 | 0,2 | 108,0 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 7 | 6 | 181 s | 145 | 0,0 | 104,3 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 6 | 6 | 73 s | 0 | 0,0 | 2,0 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 6 | 6 | 186 s | 105 | 0,2 | 113,5 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 7 | 6 | 194 s | 337 | 0,0 | 121,1 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 7 | 6 | 71 s | 446 | 0,1 | 1,7 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 7 | 6 | 185 s | 173 | 0,0 | 119,9 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 6 | 6 | 190 s | 0 | 0,0 | 126,0 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 6 | 6 | 86 s | 278 | 0,3 | 5,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 9 ×3, piętro 10 ×2, piętro 11 ×7, piętro 13 ×3, piętro 14 ×5, piętro 15 ×4, piętro 16 ×5, piętro 17 ×1, piętro 20 ×2; wyzwań (3 porażki na piętrze): 3 – piętro 16, piętro 20.
**Gracz H&S / souls-like** – ponowne próby: piętro 10 ×1, piętro 11 ×3, piętro 13 ×2, piętro 14 ×2, piętro 16 ×3, piętro 17 ×1, piętro 19 ×2, piętro 20 ×5, piętro 23 ×1, piętro 26 ×1, piętro 27 ×1, piętro 28 ×1; wyzwań (3 porażki na piętrze): 1 – piętro 20.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 555 | 2,5 s | 4,7 | 1336 | 25323 | 44 | 3 |
| Tytan Otchłani | 15 | 35,4 s | 207,6 | 166 | 8112 | 541 | 1 |
| Grzebacz | 487 | 1,9 s | 5,6 | 429 | 5564 | 11 | 0 |
| Kowal Piekieł | 9 | 67,2 s | 598,6 | 189 | 4050 | 450 | 0 |
| Królowa Topieli | 20 | 64,7 s | 178,9 | 583 | 3463 | 173 | 0 |
| Wiedźma wichru | 922 | 2,8 s | 5,2 | 1953 | 3447 | 4 | 1 |
| Ogar otchłani | 1519 | 1,0 s | 3,4 | 657 | 2663 | 2 | 0 |
| Kultysta topieli | 1085 | 1,2 s | 3,1 | 657 | 2651 | 2 | 0 |
| Arcydiabeł | 9 | 89,4 s | 1077,1 | 136 | 2276 | 253 | 0 |
| Kolos | 1093 | 2,4 s | 6,0 | 38 | 1879 | 2 | 1 |
| Harpia | 777 | 1,4 s | 5,1 | 1078 | 1731 | 2 | 0 |
| Matka Burz | 12 | 109,1 s | 566,2 | 325 | 1470 | 122 | 0 |
| Cień | 639 | 1,3 s | 5,2 | 195 | 1455 | 2 | 2 |
| Tkaczka | 339 | 2,8 s | 6,5 | 327 | 1327 | 4 | 1 |
| Rzeźnik | 20 | 37,7 s | 160,9 | 107 | 1272 | 64 | 0 |
| Taranowiec | 1139 | 2,0 s | 5,7 | 164 | 1081 | 1 | 0 |
| Legionista | 922 | 3,6 s | 10,3 | 139 | 1046 | 1 | 1 |
| Kultysta | 1540 | 0,6 s | 2,1 | 209 | 926 | 1 | 0 |
| Upiór | 398 | 2,6 s | 6,8 | 551 | 756 | 2 | 0 |
| Król Kości | 9 | 64,0 s | 445,0 | 108 | 705 | 78 | 0 |
| Kolos żaru | 588 | 2,9 s | 9,5 | 32 | 591 | 1 | 0 |
| Władca Bramy | 20 | 13,0 s | 34,5 | 85 | 562 | 28 | 0 |
| Pan Sfory | 20 | 23,6 s | 61,7 | 130 | 542 | 27 | 0 |
| Nekromanta | 261 | 2,7 s | 7,7 | 107 | 514 | 2 | 0 |
| Pajączek | 238 | 0,3 s | 1,7 | 77 | 467 | 2 | 0 |
| Czerw | 3678 | 0,4 s | 1,8 | 95 | 408 | 0 | 0 |
| Gnilec (odłamek) | 1119 | 1,5 s | 5,1 | 89 | 345 | 0 | 0 |
| Ofiarnik | 351 | 2,0 s | 3,8 | 213 | 328 | 1 | 0 |
| Gargulec | 389 | 2,2 s | 6,6 | 47 | 313 | 1 | 0 |
| Siarkowy pomiot | 1137 | 0,3 s | 1,9 | 9 | 304 | 0 | 0 |
| Gnilec | 547 | 3,5 s | 11,3 | 75 | 290 | 1 | 1 |
| Ożywiony nietoperz | 570 | 0,4 s | 1,8 | 40 | 182 | 0 | 0 |
| Kamienny chochlik | 491 | 0,3 s | 1,7 | 10 | 122 | 0 | 0 |
| Wodny chochlik | 477 | 0,5 s | 2,1 | 11 | 80 | 0 | 0 |
| Chochlik wichru | 467 | 1,1 s | 3,2 | 13 | 55 | 0 | 0 |
| Ognisty chochlik | 564 | 0,5 s | 2,1 | 12 | 54 | 0 | 0 |
| Siarkowy pomiot (elita) | 39 | 1,4 s | 5,5 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 49 | 4,2 s | 11,5 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 32 | 6,5 s | 14,2 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 26 | 4,4 s | 18,6 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 46 | 3,7 s | 7,4 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 30 | 2,1 s | 6,0 | ↑ | ↑ | ↑ |  |
| Worek jaj | 174 | 1,3 s | 1,7 | 0 | 0 | 0 | 0 |
| Gargulec (elita) | 10 | 6,7 s | 30,8 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 19 | 11,2 s | 57,0 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 43 | 3,0 s | 8,2 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 36 | 6,5 s | 20,0 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 24 | 6,7 s | 22,8 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 18 | 5,1 s | 19,5 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 33 | 9,1 s | 30,1 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 22 | 8,6 s | 32,2 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 16 | 6,2 s | 30,3 | ↑ | ↑ | ↑ |  |
| Bezimienny Cień | 9 | 72,9 s | 643,9 | 78 | 0 | 0 | 0 |
| Gnilec (elita) | 18 | 11,5 s | 59,8 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 13 | 4,3 s | 18,2 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 8 | 14,2 s | 31,9 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 11 | 8,6 s | 36,7 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 6 | 17,8 s | 9,7 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 9 | 8,6 s | 26,9 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1318 | 47% | 0% | 0% | 53% | 0% |
| ciężki | 100 | 49% | 0% | 0% | 51% | 0% |
| nieparowalny | 928 | 43% | 0% | 0% | 57% | 0% |
| nieblokowalny | 51 | 80% | 0% | 0% | 20% | 0% |
| obszarowy | 2085 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1719 | 33% | 0% | 0% | 67% | 0% |
| ciężki | 117 | 21% | 0% | 0% | 79% | 0% |
| nieparowalny | 1451 | 26% | 0% | 0% | 74% | 0% |
| nieblokowalny | 72 | 56% | 0% | 0% | 44% | 0% |
| obszarowy | 2629 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1295 | 26% | 15% | 4% | 16% | 38% |
| Tester QA (0,30 s) | ciężki | 226 | 1% | 1% | 6% | 24% | 68% |
| Tester QA (0,30 s) | nieparowalny | 586 | 8% | 3% | 8% | 29% | 52% |
| Tester QA (0,30 s) | nieblokowalny | 15 | 7% | 0% | 20% | 13% | 60% |
| Tester QA (0,30 s) | obszarowy | 1016 | 1% | 2% | 6% | 45% | 46% |
| Tester QA (0,30 s) | pocisk | 1301 | 3% | 11% | 7% | 12% | 66% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2052 | 15% | 12% | 14% | 10% | 49% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 271 | 1% | 1% | 2% | 21% | 75% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 831 | 6% | 2% | 2% | 15% | 75% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 29 | 0% | 0% | 3% | 3% | 93% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 1430 | 1% | 2% | 7% | 29% | 61% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 1700 | 2% | 9% | 8% | 11% | 70% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,27 s | 343 |
| Pajączek: Pajączek | 0,31 s | 198 |
| Czerw: Ugryzienie | 0,33 s | 541 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 171 |
| Ognisty chochlik: Pazury | 0,33 s | 57 |
| Kamienny chochlik: Pazury | 0,33 s | 29 |
| Wodny chochlik: Pazury | 0,33 s | 52 |
| Chochlik wichru: Pazury | 0,33 s | 49 |
| Władca Bramy: Cięcie w szale | 0,41 s | 7 |
| Władca Bramy: Drugie cięcie | 0,43 s | 9 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 89% (n=64) | 100% (n=14) | 73% (n=51) | 100% (n=4) | 100% (n=68) | 69% (n=36) |
| niezauważony | 79% (n=199) | – | 87% (n=99) | 100% (n=21) | 100% (n=1260) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 58% (n=31) |
| unik | 32% (n=517) | 23% (n=52) | 22% (n=465) | 47% (n=19) | 100% (n=33) | 22% (n=165) |
| unik (eksperyment) | 27% (n=106) | 0% (n=7) | 38% (n=81) | – | 100% (n=116) | 26% (n=27) |
| wyjście z zasięgu | 61% (n=62) | 75% (n=12) | 57% (n=23) | – | 100% (n=462) | 46% (n=67) |
| wyjście z zasięgu (eksperyment) | 80% (n=138) | 93% (n=15) | 88% (n=84) | 100% (n=7) | 100% (n=128) | 55% (n=49) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 170,7 | reakcja na cios wręcz 151,1, unik przed pociskiem 55,3, wabienie pod czar obszarowy 6,8, wyjście z okrążenia 3,1 | w trakcie uniku 16,2, drgnięcie (trafiony) 13,6, brak wytrzymałości 7,0, picie flaszki 3,5, unieruchomienie 0,5, inna 0,3, umiejętność 0,1 | 67% (n=1511) / 92% (n=553) |
| Gracz H&S / souls-like | 268,1 | reakcja na cios wręcz 249,3, unik przed pociskiem 80,8, doskok do odsłoniętego celu 22,2, wabienie pod czar obszarowy 12,3, wyjście z okrążenia 1,4 | w trakcie uniku 54,1, brak wytrzymałości 15,9, drgnięcie (trafiony) 12,6, picie flaszki 2,8, unieruchomienie 0,4 | 76% (n=2493) / 94% (n=808) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Powietrze | Nova powietrza | 10 | 3 | 19,8 | 874,4 |
| Gracz H&S / souls-like | Powietrze | Nova powietrza | 10 | 6 | 24,8 | 1213,8 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka powietrza | 2555828 | 70% |  |  |
| Nova powietrza | 390853 | 11% | 1122 | 348 |
| Nawałnica | 310464 | 9% | 575 | 540 |
| Roztrzaskanie | 165454 | 5% |  |  |
| Ściana ognia | 120874 | 3% | 346 | 349 |
| Odskok widma | 35260 | 1% |  |  |
| Kamienne kolce | 24462 | 1% | 45 | 544 |
| Nova ognia | 17819 | 0% | 60 | 297 |
| Pocisk arkanów | 11366 | 0% | 134 | 85 |
| Fala mocy | 1308 | 0% | 4 | 327 |

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka powietrza | 3768924 | 63% |  |  |
| Łańcuch błyskawic | 653934 | 11% | 1644 | 398 |
| Nawałnica | 493970 | 8% | 620 | 797 |
| Nova powietrza | 457558 | 8% | 846 | 541 |
| Burza | 304449 | 5% | 176 | 1730 |
| Przyzwanie chochlików | 99849 | 2% | 68 | 1468 |
| Odskok widma | 95025 | 2% |  |  |
| Nova wody | 49931 | 1% | 43 | 1161 |
| Nova ziemi | 9476 | 0% | 17 | 557 |
| Deszcz strzał | 7219 | 0% | 9 | 802 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Krwawy pakt ×8.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,3, mediana 4,0, najwięcej 5 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 21% | 48,0 | 276 | 0,7 |
| klasa | 25% | 57,5 | 540 | 0,3 |
| sklep | 5% | 10,9 | 400 | 1,1 |
| przedmiot | 49% | 112,2 | 348 | 0,6 |

Oferty sklepu dusz: widziane 564, kupione 3, odrzucone 0, za mało dusz 185.
Zamiany w slotach: 3,0 na podejście; najczęstsze: Ściana ognia → Nova powietrza ×15, Nova powietrza → Ściana ognia ×12, Nova powietrza → Kamienne kolce ×1, Nova powietrza → Nova ognia ×1, Nova powietrza → Fala mocy ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Nova powietrza | przedmiot | 10 | 112,2 | 348 | 0,6 |
| Nawałnica | klasa | 10 | 57,5 | 540 | 0,3 |
| Ściana ognia | odblokowanie | 10 | 34,6 | 349 | 0,7 |
| Pocisk arkanów | odblokowanie | 10 | 13,4 | 85 | 0,9 |
| Nova ognia | sklep | 1 | 60,0 | 297 | 0,9 |
| Kamienne kolce | sklep | 1 | 45,0 | 544 | 1,0 |
| Fala mocy | sklep | 1 | 4,0 | 327 | 4,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 6,1, mediana 6,0, najwięcej 10 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 53% | 190,2 | 511 | 0,4 |
| klasa | 17% | 62,0 | 797 | 0,3 |
| sklep | 5% | 19,3 | 975 | 1,9 |
| przedmiot | 24% | 84,6 | 541 | 0,8 |

Oferty sklepu dusz: widziane 714, kupione 17, odrzucone 11, za mało dusz 210.
Zamiany w slotach: 4,7 na podejście; najczęstsze: Pocisk arkanów → Burza ×7, Kojące światło → Łańcuch błyskawic ×7, Burza → Salwa ×2, Łańcuch błyskawic → Nova wody ×2, Nova powietrza → Lodowy podmuch ×1, Pocisk arkanów → Krwawy pakt ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Łańcuch błyskawic | odblokowanie | 9 | 182,7 | 398 | 0,4 |
| Nova powietrza | przedmiot | 10 | 84,6 | 541 | 0,8 |
| Nawałnica | klasa | 10 | 62,0 | 797 | 0,3 |
| Burza | odblokowanie | 8 | 22,0 | 1730 | 0,6 |
| Przyzwanie chochlików | sklep | 1 | 68,0 | 1468 | 1,3 |
| Nova wody | sklep | 2 | 21,5 | 1161 | 1,0 |
| Ściana ognia | odblokowanie | 1 | 22,0 | 237 | 0,0 |
| Nova ziemi | sklep | 1 | 17,0 | 557 | 2,0 |
| Pocisk arkanów | odblokowanie | 1 | 14,0 | 61 | 0,0 |
| Meteor | sklep | 1 | 11,0 | 594 | 4,0 |
| Łańcuchy potępionych | sklep | 1 | 10,0 | 306 | 2,0 |
| Tąpnięcie | sklep | 2 | 5,0 | 150 | 3,4 |
| Kula ognia | odblokowanie | 1 | 9,0 | 158 | 0,7 |
| Lodowa włócznia | sklep | 1 | 9,0 | 303 | 4,0 |
| Deszcz strzał | sklep | 1 | 9,0 | 802 | 2,0 |
| Krwawy pakt | odblokowanie | 1 | 8,0 | – | 0,0 |
| Mroźna fala | odblokowanie | 1 | 7,0 | 97 | 0,0 |
| Rozprysk arkanów | odblokowanie | 2 | 3,0 | 65 | 0,3 |
| Płonąca ziemia | odblokowanie | 1 | 6,0 | 473 | 0,0 |
| Strzał przeszywający | sklep | 1 | 6,0 | 417 | 1,0 |
| Lodowy podmuch | odblokowanie | 1 | 5,0 | 220 | 0,0 |
| Włócznia potępionych | odblokowanie | 1 | 5,0 | 202 | 0,2 |
| Mroźna strzała | sklep | 1 | 5,0 | 209 | 4,0 |
| Lodowy odłamek | sklep | 1 | 3,0 | 1387 | 2,0 |
| Salwa | sklep | 1 | 2,0 | 92 | 4,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 1557,2, umiejętność 228,6, unik 170,7, drgnięcie (trafiony) 83,2, czar obszarowy na jedynego wroga 50,6, czar obszarowy na jednego (test) 49,7, flaszka (próba) 28,5, wabienie bez skutku 23,4, czar obszarowy na grupę 15,9, flaszka many (próba) 13,3, czar obszarowy na jednego (ratunek) 5,5, czar obszarowy po zwabieniu 1,6.
Czas w bloku 0 s, w biegu 72 s, bez wytrzymałości (<10%) 101 s na podejście.
Zakupy: dodatkowa flaszka ×18, ulepszenie: Nova powietrza ×17, sklep: Różdżka powietrza: poziom 3 ×10, sklep: Różdżka powietrza: poziom 4 ×10, ulepszenie: Pocisk arkanów ×10, założony przedmiot: Kolczuga ×7, ulepszenie: Ściana ognia ×7, sklep: Szybkie ręce ×7.
Punkty cech: Intelligence 52, Toughness 47, Dexterity 46, Strength 43.
Pieczęcie na podejście: z daru bossa 5,9, z sakwy do skilli 4,1, kupione 0,4; najczęstsze: Serce otchłani ×10, Oko Czarnoboga ×7, Klątwa Mokoszy ×7, Deszcz piekieł ×6, Nieskończoność ×5, Wieczny płomień ×5.
Komunikaty gry: „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×160; „Portale gasną – dobij pozostałe demony” ×131; „Garda wroga przełamana!” ×67; „OGŁUSZENIE!” ×65; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×62; „ODKRYCIE: więź Więź: Ognisty wicher” ×40; „POLEGŁEŚ” ×39; „Królowa Topieli: kolejna faza” ×26.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 1264,0, umiejętność 356,1, unik 268,1, drgnięcie (trafiony) 65,2, czar obszarowy na jedynego wroga 62,2, wabienie bez skutku 43,8, flaszka (próba) 35,4, flaszka many (próba) 27,0, czar obszarowy na grupę 23,9, czar obszarowy po zwabieniu 3,5, czar obszarowy na jednego (ratunek) 3,2.
Czas w bloku 0 s, w biegu 100 s, bez wytrzymałości (<10%) 136 s na podejście.
Zakupy: ulepszenie: Nova powietrza ×22, dodatkowa flaszka ×18, sklep: Przebicie ×16, sklep: Szeroki zasięg ×16, sklep: Przeszycie ×15, sklep: Rykoszet ×15, sklep: Skupienie ×15, sklep: Pancerz ×14.
Punkty cech: Intelligence 163, Toughness 75.
Pieczęcie na podejście: z daru bossa 7,5, z sakwy do skilli 5,8, kupione 0,2; najczęstsze: Pakt z diabłem ×11, Serce otchłani ×10, Nieskończoność ×9, Deszcz piekieł ×7, Wieczny płomień ×7, Oko Czarnoboga ×6.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Kontra, Rozprysk arkanów, Mroźna fala, Krwawy pakt, Mroźna zbroja, Łańcuch błyskawic, Włócznia potępionych, Płonąca ziemia, Puklerz, Sztylet parujący, Burza, Łuk refleksyjny, Kostur burzy, Pawęż, Szata popiołu, Kosa, Wielki topór, Młot burzy, Kojące światło, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×189; „Portale gasną – dobij pozostałe demony” ×166; „Garda wroga przełamana!” ×89; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×81; „OGŁUSZENIE!” ×60; „POLEGŁEŚ” ×27; „Królowa Topieli: kolejna faza” ×20; „Tytan Otchłani: kolejna faza” ×18.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×400, uśpiony wróg blokuje koniec piętra ×80, zablokowany ruch ×2.
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×655, uśpiony wróg blokuje koniec piętra ×113, zablokowany ruch ×3.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 10 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (2 razy) – do przejrzenia, nie blokują gry.
- Unik przed czerwonym (nieblokowalnym): 47% ciosów mimo to rani (n=19) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 20% ciosów (n=59).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=590) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 8% pocisków, przed którymi był unik (n=553).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 41% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1295) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=226).
- Telegraf „nieparowalny” daje czas: tylko 11% decyzji zapada mniej niż 0,1 s przed trafieniem (n=586).
- Telegraf „nieblokowalny” daje czas: tylko 7% decyzji zapada mniej niż 0,1 s przed trafieniem (n=15).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1016).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 165 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 155 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 155 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 186 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 161 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 168 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 179 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVI. Kuźnia Piekieł” trwa średnio 187 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVIII. Tron Arcydemona” trwa średnio 195 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIX. Tron Arcydemona” trwa średnio 175 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 1,0 ms/klatkę (maks. 567 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 6/10 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Progres między podejściami słabo odczuwalny: 25,2 → 25,6 średnio. Meta-progresja niewiele zmienia w walce.
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Królowa Topieli (156,6 trafień, 58,3 s), Rzeźnik (140,8 trafień, 28,0 s), Gnilec (elita) (61,7 trafień, 12,3 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kamienny chochlik (1,5), Worek jaj (1,8), Czerw (1,8) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Królowa Topieli – średnio 58 s i 157 trafień do zabicia.
- Długie starcie: Matka Burz – średnio 105 s i 569 trafień do zabicia.
- Długie starcie: Król Kości – średnio 69 s i 448 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 70 s i 650 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 67 s i 627 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 80 s i 1172 trafień do zabicia.
- Długie starcie: Nekromanta (elita) – średnio 43 s i 62 trafień do zabicia.
- 4/4 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 85% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 35% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Burza”: 1730 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 5%. Różnych skilli na podejście: średnio 6,1.
- Unik to moje główne narzędzie: 268,1 na podejście, w tym 23,6 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 94% przypadków.
- Gra nie wykonała uniku od razu średnio 85,8× na podejście – najczęściej: w trakcie poprzedniego uniku 54,1, brak wytrzymałości 15,9, drgnięcie (trafiony) 12,6. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (65 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

