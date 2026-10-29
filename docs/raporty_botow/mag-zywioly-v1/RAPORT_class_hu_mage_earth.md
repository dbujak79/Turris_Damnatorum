# Raport botów grających — Mag ziemi

Wygenerowano: 2026-10-29 12:56. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (10 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (10 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 10 | 4 (40%) | 22,8 | 21,5 | 47,9 min | 1039,9 | 561919 / 4679 | 11,7 | 7% |
| Gracz H&S / souls-like | 10 | 8 (80%) | 29,2 | 30,0 | 57,9 min | 1542,7 | 892264 / 4232 | 9,1 | 14% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 15 ★ ★ ⏱14 ★ ★ 20 20 23 16
- Gracz H&S / souls-like: ★ ★ ★ ★ ★ 25 ★ ★ 27 ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 15. XV. Tytan Otchłani ×1, 16. XVI. Wieża Burz ×1, 20. XX. Kostnica ×2, 23. XXIII. Chór Cieni ×1.
  Ostatni cios: Tytan Otchłani – „Tytan Otchłani: StoneSpikes” ×1; Cień – „Cień: Combo” ×1; Miotacz głazów – „Miotacz głazów” ×1; Harpia – „Harpia: WindStep” ×1; Legionista – „Legionista” ×1. Śmierć ciosem ≥40% maks. HP: 0/5. Zginął mając flaszki: 4/5.
  Ostatnie 3 s przed śmiercią: średnio 38% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/5). Picie: 359 prób, 117 wypitych (67% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 25. XXV. Kuźnia Piekieł ×1, 27. XXVII. Kowal Piekieł ×1.
  Ostatni cios: Kolos żaru – „Kolos żaru: Quake” ×1; Kowal Piekieł – „Kowal Piekieł: InfernalOrb” ×1. Śmierć ciosem ≥40% maks. HP: 0/2. Zginął mając flaszki: 2/2.
  Ostatnie 3 s przed śmiercią: średnio 62% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/2). Picie: 250 prób, 91 wypitych (64% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 10 | 10 | 102 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 10 | 10 | 112 s | 0 | 0,0 | 41,5 |
| Tester QA | 3. III. Władca Bramy | 10 | 10 | 15 s | 15 | 0,0 | 6,1 |
| Tester QA | 4. IV. Psiarnia Piekieł | 10 | 10 | 112 s | 0 | 0,0 | 44,1 |
| Tester QA | 5. V. Psiarnia Piekieł | 10 | 10 | 101 s | 2 | 0,0 | 42,8 |
| Tester QA | 6. VI. Pan Sfory | 10 | 10 | 27 s | 29 | 0,0 | 4,4 |
| Tester QA | 7. VII. Zatopione Krużganki | 10 | 10 | 103 s | 1 | 0,0 | 43,2 |
| Tester QA | 8. VIII. Zatopione Krużganki | 10 | 10 | 128 s | 41 | 0,1 | 54,6 |
| Tester QA | 9. IX. Królowa Topieli | 10 | 10 | 44 s | 28 | 0,2 | 2,6 |
| Tester QA | 10. X. Rzeźnia | 12 | 10 | 139 s | 161 | 0,8 | 58,0 |
| Tester QA | 11. XI. Rzeźnia | 12 | 10 | 135 s | 210 | 0,8 | 58,3 |
| Tester QA | 12. XII. Rzeźnik | 10 | 10 | 46 s | 13 | 0,0 | 6,4 |
| Tester QA | 13. XIII. Kamienne Trzewia | 12 | 10 | 147 s | 224 | 0,5 | 64,0 |
| Tester QA | 14. XIV. Kamienne Trzewia | 13 | 9 | 145 s | 313 | 0,8 | 66,2 |
| Tester QA | 15. XV. Tytan Otchłani | 12 | 8 | 80 s | 703 | 2,1 | 1,4 |
| Tester QA | 16. XVI. Wieża Burz | 13 | 7 | 163 s | 509 | 0,6 | 72,0 |
| Tester QA | 17. XVII. Wieża Burz | 7 | 7 | 154 s | 264 | 0,4 | 87,6 |
| Tester QA | 18. XVIII. Matka Burz | 7 | 7 | 59 s | 230 | 0,6 | 3,0 |
| Tester QA | 19. XIX. Kostnica | 7 | 7 | 158 s | 21 | 0,0 | 89,0 |
| Tester QA | 20. XX. Kostnica | 13 | 5 | 171 s | 511 | 0,4 | 78,1 |
| Tester QA | 21. XXI. Król Kości | 5 | 5 | 79 s | 190 | 0,8 | 12,2 |
| Tester QA | 22. XXII. Chór Cieni | 6 | 5 | 182 s | 174 | 0,7 | 100,7 |
| Tester QA | 23. XXIII. Chór Cieni | 7 | 4 | 181 s | 229 | 0,7 | 94,1 |
| Tester QA | 24. XXIV. Bezimienny Cień | 4 | 4 | 58 s | 4 | 0,3 | 1,8 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 4 | 4 | 183 s | 0 | 0,3 | 110,5 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 4 | 4 | 186 s | 58 | 0,8 | 112,0 |
| Tester QA | 27. XXVII. Kowal Piekieł | 5 | 4 | 61 s | 324 | 1,4 | 1,4 |
| Tester QA | 28. XXVIII. Tron Arcydemona | 5 | 4 | 189 s | 351 | 1,4 | 115,4 |
| Tester QA | 29. XXIX. Tron Arcydemona | 5 | 4 | 186 s | 134 | 0,4 | 107,0 |
| Tester QA | 30. XXX. Arcydiabeł | 5 | 4 | 73 s | 224 | 0,0 | 8,8 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 10 | 10 | 102 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 10 | 10 | 105 s | 0 | 0,0 | 42,2 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 10 | 10 | 13 s | 2 | 0,0 | 6,4 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 10 | 10 | 116 s | 0 | 0,0 | 44,4 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 10 | 10 | 91 s | 4 | 0,1 | 37,5 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 10 | 10 | 19 s | 0 | 0,0 | 3,0 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 10 | 10 | 105 s | 0 | 0,0 | 45,5 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 11 | 10 | 126 s | 101 | 0,3 | 50,9 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 10 | 10 | 36 s | 82 | 0,0 | 3,0 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 11 | 10 | 110 s | 20 | 0,1 | 55,1 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 10 | 10 | 131 s | 41 | 0,1 | 66,2 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 10 | 10 | 28 s | 0 | 0,0 | 2,2 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 13 | 10 | 132 s | 178 | 0,3 | 61,2 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 10 | 10 | 133 s | 163 | 0,6 | 69,4 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 10 | 10 | 44 s | 170 | 0,2 | 2,2 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 12 | 10 | 183 s | 421 | 0,7 | 83,0 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 11 | 10 | 161 s | 140 | 0,4 | 84,2 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 11 | 10 | 42 s | 153 | 0,3 | 1,9 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 11 | 10 | 163 s | 201 | 0,9 | 90,1 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 14 | 10 | 174 s | 238 | 0,5 | 91,4 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 11 | 10 | 41 s | 90 | 0,0 | 6,0 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 12 | 10 | 177 s | 346 | 0,9 | 105,5 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 12 | 10 | 183 s | 169 | 0,3 | 102,0 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 10 | 10 | 56 s | 19 | 0,2 | 2,5 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 13 | 9 | 211 s | 486 | 1,0 | 108,9 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 9 | 9 | 187 s | 61 | 0,4 | 116,2 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 11 | 8 | 51 s | 269 | 0,2 | 1,5 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 8 | 8 | 190 s | 165 | 0,1 | 124,5 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 8 | 8 | 178 s | 32 | 0,1 | 126,3 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 8 | 8 | 58 s | 184 | 0,4 | 6,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 10 ×2, piętro 11 ×2, piętro 13 ×2, piętro 14 ×3, piętro 15 ×3, piętro 16 ×5, piętro 20 ×6, piętro 22 ×1, piętro 23 ×2, piętro 27 ×1, piętro 28 ×1, piętro 29 ×1, piętro 30 ×1; wyzwań (3 porażki na piętrze): 4 – piętro 15, piętro 20, piętro 23.
**Gracz H&S / souls-like** – ponowne próby: piętro 8 ×1, piętro 10 ×1, piętro 13 ×3, piętro 16 ×2, piętro 17 ×1, piętro 18 ×1, piętro 19 ×1, piętro 20 ×4, piętro 21 ×1, piętro 22 ×2, piętro 23 ×2, piętro 25 ×3, piętro 27 ×2; wyzwań (3 porażki na piętrze): 2 – piętro 25, piętro 27.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 794 | 3,1 s | 3,5 | 2561 | 26021 | 32 | 1 |
| Tytan Otchłani | 18 | 55,5 s | 148,7 | 350 | 10135 | 563 | 1 |
| Wiedźma wichru | 1271 | 0,6 s | 1,5 | 2497 | 9160 | 7 | 0 |
| Kowal Piekieł | 12 | 49,9 s | 229,2 | 187 | 4580 | 382 | 1 |
| Grzebacz | 671 | 2,2 s | 4,6 | 767 | 4272 | 6 | 0 |
| Harpia | 1010 | 0,5 s | 1,7 | 1480 | 3864 | 4 | 1 |
| Kultysta topieli | 1442 | 0,8 s | 1,7 | 1018 | 3791 | 3 | 0 |
| Matka Burz | 17 | 44,7 s | 113,4 | 223 | 3273 | 193 | 0 |
| Arcydiabeł | 12 | 57,6 s | 274,8 | 145 | 2587 | 216 | 0 |
| Ogar otchłani | 1830 | 0,3 s | 1,4 | 634 | 2159 | 1 | 0 |
| Król Kości | 15 | 48,9 s | 141,6 | 166 | 1931 | 129 | 0 |
| Taranowiec | 1341 | 1,6 s | 3,3 | 251 | 1778 | 1 | 0 |
| Upiór | 566 | 1,8 s | 3,1 | 774 | 1709 | 3 | 0 |
| Cień | 907 | 0,9 s | 2,3 | 297 | 1682 | 2 | 1 |
| Legionista | 1145 | 3,9 s | 7,9 | 199 | 1378 | 1 | 1 |
| Nekromanta | 355 | 3,8 s | 4,7 | 230 | 1221 | 3 | 0 |
| Kultysta | 1726 | 0,4 s | 1,3 | 281 | 1132 | 1 | 0 |
| Kolos żaru | 806 | 2,1 s | 4,2 | 44 | 1119 | 1 | 1 |
| Kolos | 1346 | 3,2 s | 5,4 | 37 | 1110 | 1 | 0 |
| Królowa Topieli | 20 | 35,0 s | 61,4 | 203 | 989 | 49 | 0 |
| Tkaczka | 464 | 3,2 s | 4,7 | 490 | 967 | 2 | 0 |
| Gnilec (odłamek) | 1492 | 1,1 s | 2,7 | 131 | 667 | 0 | 0 |
| Ożywiony nietoperz | 1070 | 0,2 s | 1,2 | 93 | 523 | 0 | 0 |
| Czerw | 4340 | 0,2 s | 1,3 | 142 | 501 | 0 | 0 |
| Siarkowy pomiot | 1249 | 0,1 s | 1,2 | 15 | 441 | 0 | 0 |
| Pajączek | 351 | 0,1 s | 1,1 | 125 | 383 | 1 | 0 |
| Gnilec | 733 | 2,9 s | 6,1 | 71 | 306 | 0 | 0 |
| Ofiarnik | 497 | 1,6 s | 2,2 | 332 | 290 | 1 | 0 |
| Gargulec | 503 | 2,8 s | 6,5 | 47 | 260 | 1 | 0 |
| Bezimienny Cień | 14 | 52,5 s | 166,8 | 139 | 202 | 14 | 0 |
| Władca Bramy | 20 | 9,5 s | 18,2 | 57 | 159 | 8 | 0 |
| Pan Sfory | 20 | 18,5 s | 30,5 | 62 | 136 | 7 | 0 |
| Rzeźnik | 20 | 32,4 s | 79,0 | 48 | 130 | 7 | 0 |
| Chochlik wichru | 534 | 0,1 s | 1,1 | 10 | 122 | 0 | 0 |
| Ognisty chochlik | 563 | 0,3 s | 1,3 | 12 | 49 | 0 | 0 |
| Kamienny chochlik | 506 | 0,8 s | 2,2 | 7 | 47 | 0 | 0 |
| Wodny chochlik | 487 | 0,1 s | 1,2 | 5 | 36 | 0 | 0 |
| Ognisty chochlik (elita) | 43 | 1,8 s | 4,5 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 48 | 1,4 s | 3,6 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 63 | 1,8 s | 4,0 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 28 | 1,7 s | 2,8 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 42 | 5,4 s | 11,5 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 41 | 4,1 s | 4,2 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 16 | 8,0 s | 18,0 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 39 | 3,1 s | 3,9 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 40 | 12,5 s | 21,7 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 34 | 9,5 s | 24,9 | ↑ | ↑ | ↑ |  |
| Worek jaj | 268 | 0,5 s | 1,3 | 0 | 0 | 0 | 0 |
| Upiór (elita) | 21 | 8,8 s | 10,2 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 13 | 16,3 s | 15,3 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 36 | 8,9 s | 21,9 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 30 | 9,5 s | 12,4 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 14 | 6,6 s | 8,7 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 31 | 3,3 s | 5,5 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 9 | 11,0 s | 21,1 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 21 | 10,3 s | 29,6 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 32 | 3,4 s | 7,8 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 25 | 2,1 s | 5,0 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 17 | 9,8 s | 18,6 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1990 | 46% | 0% | 0% | 54% | 0% |
| ciężki | 96 | 49% | 0% | 0% | 51% | 0% |
| nieparowalny | 1273 | 44% | 0% | 0% | 56% | 0% |
| nieblokowalny | 31 | 61% | 0% | 0% | 39% | 0% |
| obszarowy | 3046 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2516 | 33% | 0% | 0% | 67% | 0% |
| ciężki | 152 | 24% | 0% | 0% | 76% | 0% |
| nieparowalny | 1638 | 25% | 0% | 0% | 75% | 0% |
| nieblokowalny | 74 | 54% | 0% | 0% | 46% | 0% |
| obszarowy | 3314 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1979 | 23% | 14% | 3% | 15% | 45% |
| Tester QA (0,30 s) | ciężki | 318 | 1% | 1% | 7% | 20% | 71% |
| Tester QA (0,30 s) | nieparowalny | 727 | 7% | 2% | 4% | 28% | 58% |
| Tester QA (0,30 s) | nieblokowalny | 20 | 0% | 0% | 0% | 45% | 55% |
| Tester QA (0,30 s) | obszarowy | 1204 | 1% | 1% | 4% | 43% | 50% |
| Tester QA (0,30 s) | pocisk | 1524 | 2% | 7% | 5% | 12% | 74% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3028 | 14% | 10% | 14% | 8% | 54% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 420 | 1% | 1% | 1% | 23% | 74% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 995 | 7% | 2% | 2% | 11% | 78% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 27 | 0% | 0% | 0% | 15% | 85% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 1749 | 2% | 1% | 4% | 38% | 55% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2500 | 2% | 7% | 6% | 9% | 77% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 217 |
| Pajączek: Pajączek | 0,31 s | 286 |
| Czerw: Ugryzienie | 0,33 s | 807 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 328 |
| Kamienny chochlik: Pazury | 0,33 s | 51 |
| Wodny chochlik: Pazury | 0,33 s | 32 |
| Chochlik wichru: Pazury | 0,33 s | 37 |
| Ognisty chochlik: Pazury | 0,33 s | 65 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 1043 |
| Królowa Topieli: Królowa Topieli: WindStep | 0,45 s | 18 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 83% (n=107) | 75% (n=8) | 81% (n=83) | – | 100% (n=55) | 78% (n=37) |
| niezauważony | 80% (n=242) | 100% (n=1) | 85% (n=103) | 100% (n=10) | 100% (n=2002) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 91% (n=137) |
| unik | 29% (n=928) | 36% (n=61) | 27% (n=669) | 40% (n=20) | 100% (n=80) | 22% (n=201) |
| unik (eksperyment) | 32% (n=149) | 55% (n=11) | 32% (n=121) | – | 100% (n=79) | 32% (n=31) |
| wyjście z zasięgu | 56% (n=90) | 57% (n=7) | 50% (n=40) | – | 100% (n=580) | 57% (n=76) |
| wyjście z zasięgu (eksperyment) | 80% (n=199) | 100% (n=8) | 88% (n=125) | 100% (n=1) | 100% (n=111) | 69% (n=64) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 221,7 | reakcja na cios wręcz 214,1, unik przed pociskiem 63,2, wyjście z okrążenia 7,6, wabienie pod czar obszarowy 6,3 | w trakcie uniku 28,8, drgnięcie (trafiony) 19,2, brak wytrzymałości 7,8, picie flaszki 3,4, unieruchomienie 0,8, szybki atak 0,1 | 66% (n=2141) / 91% (n=632) |
| Gracz H&S / souls-like | 358,2 | reakcja na cios wręcz 336,7, unik przed pociskiem 112,9, doskok do odsłoniętego celu 33,0, wabienie pod czar obszarowy 6,5, wyjście z okrążenia 4,6 | w trakcie uniku 78,2, drgnięcie (trafiony) 19,0, brak wytrzymałości 16,5, picie flaszki 3,1, unieruchomienie 0,7, inna 0,3 | 76% (n=3367) / 95% (n=1129) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Ziemia | Nova ziemi | 10 | 4 | 22,8 | 1039,9 |
| Gracz H&S / souls-like | Ziemia | Nova ziemi | 10 | 8 | 29,2 | 1542,7 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka ziemi | 4205521 | 75% |  |  |
| Trzęsienie ziemi | 774834 | 14% | 1369 | 566 |
| Nova ziemi | 300267 | 5% | 1166 | 258 |
| Ściana ognia | 139450 | 2% | 418 | 334 |
| Odskok widma | 105712 | 2% |  |  |
| Deszcz siarki | 40488 | 1% | 35 | 1157 |
| Burza | 25511 | 0% | 8 | 3189 |
| Pocisk arkanów | 20857 | 0% | 260 | 80 |
| Piekielna kula | 6553 | 0% | 9 | 728 |

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka ziemi | 5909645 | 66% |  |  |
| Trzęsienie ziemi | 1436824 | 16% | 1147 | 1253 |
| Płonąca ziemia | 762091 | 9% | 493 | 1546 |
| Nova ziemi | 208871 | 2% | 614 | 340 |
| Odskok widma | 205919 | 2% |  |  |
| Łza lodowca | 103153 | 1% | 33 | 3126 |
| Strzał przeszywający | 69138 | 1% | 19 | 3639 |
| Burza | 47284 | 1% | 29 | 1630 |
| Nova ognia | 33835 | 0% | 21 | 1611 |
| Kula ognia | 25954 | 0% | 89 | 292 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Krwawy pakt ×5.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,3, mediana 4,0, najwięcej 5 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 21% | 67,8 | 236 | 0,8 |
| klasa | 42% | 136,9 | 566 | 0,4 |
| sklep | 2% | 5,2 | 1395 | 2,3 |
| przedmiot | 36% | 116,6 | 258 | 0,6 |

Oferty sklepu dusz: widziane 654, kupione 4, odrzucone 0, za mało dusz 214.
Zamiany w slotach: 2,4 na podejście; najczęstsze: Ściana ognia → Nova ziemi ×9, Nova ziemi → Ściana ognia ×6, Pocisk arkanów → Ściana ognia ×3, Ściana ognia → Pocisk arkanów ×2, Nova ziemi → Burza ×1, Nova ziemi → Deszcz siarki ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Trzęsienie ziemi | klasa | 10 | 136,9 | 566 | 0,4 |
| Nova ziemi | przedmiot | 10 | 116,6 | 258 | 0,6 |
| Ściana ognia | odblokowanie | 10 | 41,8 | 334 | 0,7 |
| Pocisk arkanów | odblokowanie | 10 | 26,0 | 80 | 0,9 |
| Deszcz siarki | sklep | 1 | 35,0 | 1157 | 2,0 |
| Piekielna kula | sklep | 1 | 9,0 | 728 | 2,0 |
| Burza | sklep | 1 | 8,0 | 3189 | 4,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 6,8, mediana 6,0, najwięcej 14 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 26% | 68,6 | 1234 | 0,7 |
| klasa | 43% | 114,7 | 1253 | 0,3 |
| sklep | 8% | 21,9 | 1439 | 2,3 |
| przedmiot | 23% | 61,4 | 340 | 0,6 |

Oferty sklepu dusz: widziane 846, kupione 22, odrzucone 10, za mało dusz 250.
Zamiany w slotach: 5,5 na podejście; najczęstsze: Pocisk arkanów → Płonąca ziemia ×7, Kojące światło → Kula ognia ×6, Kojące światło → Burza ×2, Ściana ognia → Krwawy pakt ×1, Nova ziemi → Lodowy podmuch ×1, Kojące światło → Szarża ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Trzęsienie ziemi | klasa | 10 | 114,7 | 1253 | 0,3 |
| Nova ziemi | przedmiot | 8 | 76,8 | 340 | 0,6 |
| Płonąca ziemia | odblokowanie | 9 | 54,8 | 1546 | 0,8 |
| Kula ognia | odblokowanie | 9 | 9,9 | 292 | 0,7 |
| Fala przypływu | sklep | 1 | 46,0 | 550 | 2,0 |
| Łza lodowca | sklep | 1 | 33,0 | 3126 | 2,0 |
| Burza | odblokowanie | 3 | 9,7 | 1630 | 0,1 |
| Ściana ognia | odblokowanie | 1 | 26,0 | 219 | 0,2 |
| Nova ognia | sklep | 2 | 10,5 | 1611 | 2,0 |
| Strzał przeszywający | sklep | 1 | 19,0 | 3639 | 2,0 |
| Łańcuchy potępionych | sklep | 1 | 14,0 | 539 | 4,0 |
| Ognista strzała | sklep | 1 | 13,0 | 561 | 4,0 |
| Strzała rozszczepu | sklep | 2 | 6,0 | 563 | 2,0 |
| Mroźna fala | odblokowanie | 1 | 9,0 | 66 | 0,0 |
| Łańcuch błyskawic | odblokowanie | 1 | 8,0 | 254 | 0,0 |
| Szarża | odblokowanie | 1 | 7,0 | 27 | 0,0 |
| Lodowy podmuch | odblokowanie | 1 | 7,0 | 68 | 0,0 |
| Włócznia potępionych | odblokowanie | 1 | 7,0 | 124 | 0,6 |
| Przyzwanie chochlików | sklep | 1 | 7,0 | 1253 | 2,0 |
| Łańcuch potępionych | sklep | 1 | 7,0 | 232 | 2,0 |
| Oko cyklonu | sklep | 1 | 7,0 | 784 | 2,0 |
| Wyrok burzy | sklep | 1 | 7,0 | 2413 | 2,0 |
| Pocisk arkanów | odblokowanie | 2 | 3,0 | 67 | 0,0 |
| Nova powietrza | sklep | 1 | 6,0 | 141 | 1,0 |
| Sfora | sklep | 1 | 6,0 | 566 | 1,0 |
| Lodowy odłamek | sklep | 1 | 6,0 | 3641 | 2,0 |
| Krwawy pakt | sklep | 1 | 5,0 | – | 4,0 |
| Rozprysk arkanów | odblokowanie | 2 | 2,5 | 137 | 1,0 |
| Kamienne kolce | sklep | 1 | 5,0 | 404 | 1,0 |
| Tąpnięcie | sklep | 1 | 5,0 | 235 | 4,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 1123,3, umiejętność 326,5, unik 221,7, drgnięcie (trafiony) 105,7, czar obszarowy na jedynego wroga 51,7, flaszka (próba) 35,9, wabienie bez skutku 33,8, czar obszarowy na jednego (test) 31,6, czar obszarowy na grupę 26,6, flaszka many (próba) 18,8, czar obszarowy na jednego (ratunek) 4,7, czar obszarowy po zwabieniu 2,3.
Czas w bloku 0 s, w biegu 92 s, bez wytrzymałości (<10%) 100 s na podejście.
Zakupy: ulepszenie: Nova ziemi ×21, dodatkowa flaszka ×19, sklep: Różdżka ziemi: poziom 3 ×10, sklep: Różdżka ziemi: poziom 4 ×10, ulepszenie: Pocisk arkanów ×10, ulepszenie: Ściana ognia ×10, sklep: Pancerz ×9, założony przedmiot: Szata adepta ×9.
Punkty cech: Intelligence 60, Toughness 55, Dexterity 53, Strength 50.
Pieczęcie na podejście: z daru bossa 6,8, z sakwy do skilli 5,0, kupione 0,5; najczęstsze: Oko Czarnoboga ×11, Nieskończoność ×9, Pakt z diabłem ×8, Klątwa Mokoszy ×7, Deszcz piekieł ×7, Serce otchłani ×6.
Komunikaty gry: „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×180; „Portale gasną – dobij pozostałe demony” ×155; „OGŁUSZENIE!” ×76; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×72; „POLEGŁEŚ” ×35; „Garda wroga przełamana!” ×34; „ODKRYCIE: więź Więź: Magma” ×28; „Tytan Otchłani: kolejna faza” ×24.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 1268,6, unik 358,2, umiejętność 266,6, drgnięcie (trafiony) 86,9, czar obszarowy na jedynego wroga 40,5, wabienie bez skutku 32,0, flaszka (próba) 25,0, czar obszarowy na grupę 22,1, flaszka many (próba) 11,2, czar obszarowy po zwabieniu 2,1, czar obszarowy na jednego (ratunek) 1,4.
Czas w bloku 0 s, w biegu 119 s, bez wytrzymałości (<10%) 131 s na podejście.
Zakupy: sklep: Szybkie ręce ×23, sklep: Twarda skóra ×22, dodatkowa flaszka ×20, sklep: Furia ×20, sklep: Skała ×19, sklep: Szeroki zasięg ×18, sklep: Hart ciała ×16, sklep: Pancerz ×16.
Punkty cech: Intelligence 194, Toughness 88.
Pieczęcie na podejście: z daru bossa 8,8, z sakwy do skilli 7,0, kupione 0,2; najczęstsze: Serce otchłani ×12, Pakt z diabłem ×12, Oko Czarnoboga ×12, Nieskończoność ×12, Wieczny płomień ×8, Klątwa Mokoszy ×6.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Kontra, Rozprysk arkanów, Mroźna fala, Krwawy pakt, Mroźna zbroja, Łańcuch błyskawic, Włócznia potępionych, Płonąca ziemia, Burza, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Szata popiołu, Kosa, Wielki topór, Młot burzy, Kojące światło, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×215; „Portale gasną – dobij pozostałe demony” ×200; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×96; „OGŁUSZENIE!” ×83; „Garda wroga przełamana!” ×67; „POLEGŁEŚ” ×26; „Kupiono: Szybkie ręce” ×23; „Matka Burz: kolejna faza” ×22.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×688, uśpiony wróg blokuje koniec piętra ×152, zablokowany ruch ×3, limit czasu piętra ×1.
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×808, uśpiony wróg blokuje koniec piętra ×180, zablokowany ruch ×5.
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 10 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** 1 podejść przerwanych limitem czasu piętra – możliwy nieosiągalny wróg albo zbyt długie piętro (zob. anomalie).
- Pojedyncze zatrzymania na geometrii (3 razy) – do przejrzenia, nie blokują gry.
- **Czytelność (P2):** 15% trafień w bohatera przyszło z tyłu (>110° od kierunku patrzenia). W grupach brakuje sygnału ataku spoza kadru.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 40% ciosów (n=20).
- Unik przed ciężkim: działa – rani tylko 39% ciosów (n=72).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=691) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 9% pocisków, przed którymi był unik (n=632).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 37% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1979) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=318).
- Telegraf „nieparowalny” daje czas: tylko 9% decyzji zapada mniej niż 0,1 s przed trafieniem (n=727).
- Telegraf „nieblokowalny” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=20).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1204).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 163 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 154 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 158 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 171 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 182 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 181 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 183 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVI. Kuźnia Piekieł” trwa średnio 186 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVIII. Tron Arcydemona” trwa średnio 189 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIX. Tron Arcydemona” trwa średnio 186 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,9 ms/klatkę (maks. 601 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 8/10 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Progres między podejściami słabo odczuwalny: 31,0 → 29,0 średnio. Meta-progresja niewiele zmienia w walce.
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Bezimienny Cień (173,5 trafień, 51,7 s), Tytan Otchłani (141,3 trafień, 39,2 s), Król Kości (139,1 trafień, 36,1 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,1), Chochlik wichru (1,2), Ożywiony nietoperz (1,2) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Bezimienny Cień – średnio 52 s i 174 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 46 s i 282 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 52 s i 318 trafień do zabicia.
- 2/2 razy zginąłem z flaszkami w kieszeni. 2 z tych śmierci to utrata ≥50% HP w 3 s, a 64% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 31% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Lodowy odłamek”: 3641 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 8%. Różnych skilli na podejście: średnio 6,8.
- Unik to moje główne narzędzie: 358,2 na podejście, w tym 37,6 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 95% przypadków.
- Gra nie wykonała uniku od razu średnio 117,8× na podejście – najczęściej: w trakcie poprzedniego uniku 78,2, drgnięcie (trafiony) 19,0, brak wytrzymałości 16,5. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (87 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

