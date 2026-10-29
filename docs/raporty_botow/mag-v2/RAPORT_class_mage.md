# Raport botów grających — Mag

Wygenerowano: 2026-10-29 12:00. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 14,3 | 13,5 | 31,0 min | 496,7 | 164103 / 4147 | 8,0 | 2% |
| Gracz H&S / souls-like | 20 | 3 (15%) | 14,9 | 12,0 | 32,0 min | 587,6 | 243144 / 3620 | 5,9 | 3% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 15 10 22 20 11 10 26 10 15 16 11 16 16 11 15 ⏱10 13 11 14 13
- Gracz H&S / souls-like: 15 ★ 13 10 10 11 26 10 8 17 8 11 14 ⏱10 8 14 ★ ★ 14 8

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×3, 11. XI. Rzeźnia ×4, 13. XIII. Kamienne Trzewia ×2, 14. XIV. Kamienne Trzewia ×1, 15. XV. Tytan Otchłani ×3, 16. XVI. Wieża Burz ×3, 20. XX. Kostnica ×1, 22. XXII. Chór Cieni ×1, 26. XXVI. Kuźnia Piekieł ×1.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×3; Harpia – „Harpia” ×2; Tytan Otchłani – „Tytan Otchłani: StoneSpikes” ×1; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×1; Cień – „Cień” ×1; Gargulec – „Gargulec” ×1. Śmierć ciosem ≥40% maks. HP: 0/19. Zginął mając flaszki: 16/19.
  Ostatnie 3 s przed śmiercią: średnio 42% maks. HP straconych (śmierć z ≥50% HP w 3 s: 5/19). Picie: 609 prób, 159 wypitych (74% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 10. X. Rzeźnia ×3, 11. XI. Rzeźnia ×2, 13. XIII. Kamienne Trzewia ×1, 14. XIV. Kamienne Trzewia ×3, 15. XV. Tytan Otchłani ×1, 17. XVII. Wieża Burz ×1, 26. XXVI. Kuźnia Piekieł ×1, 8. VIII. Zatopione Krużganki ×4.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×4; Tytan Otchłani – „Tytan Otchłani: Rockfall” ×1; Taranowiec – „Taranowiec: Combo” ×1; Taranowiec – „Taranowiec” ×1; Miotacz głazów – „Miotacz głazów: Rockfall” ×1; Kolos żaru – „Kolos żaru” ×1. Śmierć ciosem ≥40% maks. HP: 0/16. Zginął mając flaszki: 16/16.
  Ostatnie 3 s przed śmiercią: średnio 47% maks. HP straconych (śmierć z ≥50% HP w 3 s: 8/16). Picie: 309 prób, 118 wypitych (62% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 101 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 113 s | 0 | 0,0 | 41,5 |
| Tester QA | 3. III. Władca Bramy | 20 | 20 | 30 s | 33 | 0,1 | 7,3 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 112 s | 1 | 0,0 | 44,0 |
| Tester QA | 5. V. Psiarnia Piekieł | 20 | 20 | 97 s | 4 | 0,0 | 40,0 |
| Tester QA | 6. VI. Pan Sfory | 20 | 20 | 52 s | 58 | 0,2 | 6,2 |
| Tester QA | 7. VII. Zatopione Krużganki | 20 | 20 | 104 s | 34 | 0,2 | 41,2 |
| Tester QA | 8. VIII. Zatopione Krużganki | 24 | 20 | 136 s | 210 | 0,6 | 47,5 |
| Tester QA | 9. IX. Królowa Topieli | 24 | 20 | 127 s | 269 | 0,6 | 2,3 |
| Tester QA | 10. X. Rzeźnia | 30 | 16 | 126 s | 301 | 0,5 | 46,0 |
| Tester QA | 11. XI. Rzeźnia | 26 | 12 | 174 s | 398 | 0,6 | 44,8 |
| Tester QA | 12. XII. Rzeźnik | 13 | 12 | 68 s | 51 | 0,1 | 3,2 |
| Tester QA | 13. XIII. Kamienne Trzewia | 18 | 10 | 141 s | 349 | 1,0 | 55,1 |
| Tester QA | 14. XIV. Kamienne Trzewia | 20 | 9 | 150 s | 417 | 0,6 | 53,2 |
| Tester QA | 15. XV. Tytan Otchłani | 17 | 6 | 82 s | 683 | 1,9 | 1,5 |
| Tester QA | 16. XVI. Wieża Burz | 14 | 3 | 165 s | 623 | 0,9 | 47,9 |
| Tester QA | 17. XVII. Wieża Burz | 4 | 3 | 172 s | 265 | 0,0 | 82,3 |
| Tester QA | 18. XVIII. Matka Burz | 3 | 3 | 140 s | 330 | 1,0 | 3,0 |
| Tester QA | 19. XIX. Kostnica | 3 | 3 | 180 s | 154 | 0,0 | 97,7 |
| Tester QA | 20. XX. Kostnica | 5 | 2 | 189 s | 702 | 0,8 | 76,0 |
| Tester QA | 21. XXI. Król Kości | 3 | 2 | 115 s | 762 | 1,3 | 10,0 |
| Tester QA | 22. XXII. Chór Cieni | 4 | 1 | 177 s | 676 | 0,3 | 61,8 |
| Tester QA | 23. XXIII. Chór Cieni | 1 | 1 | 180 s | 0 | 0,0 | 112,0 |
| Tester QA | 24. XXIV. Bezimienny Cień | 1 | 1 | 167 s | 62 | 0,0 | 1,0 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 1 | 1 | 194 s | 10 | 0,0 | 117,0 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 3 | 0 | 0 s | 914 | 0,0 | 17,7 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 103 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 20 | 114 s | 0 | 0,0 | 41,5 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 20 s | 18 | 0,1 | 7,0 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 20 | 20 | 113 s | 1 | 0,0 | 44,9 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 20 | 20 | 101 s | 4 | 0,0 | 39,3 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 20 | 20 | 41 s | 15 | 0,0 | 6,3 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 21 | 20 | 112 s | 39 | 0,1 | 43,0 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 38 | 16 | 153 s | 431 | 0,7 | 40,0 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 17 | 16 | 84 s | 115 | 0,3 | 2,6 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 26 | 12 | 122 s | 316 | 0,3 | 43,1 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 22 | 10 | 135 s | 426 | 0,6 | 48,6 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 10 | 10 | 48 s | 16 | 0,1 | 5,8 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 15 | 9 | 149 s | 407 | 0,7 | 53,5 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 18 | 6 | 137 s | 457 | 0,6 | 41,5 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 11 | 5 | 56 s | 867 | 2,3 | 1,1 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 9 | 5 | 165 s | 386 | 0,6 | 68,3 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 8 | 4 | 162 s | 400 | 0,4 | 67,8 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 4 | 4 | 82 s | 43 | 0,5 | 1,5 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 4 | 4 | 170 s | 32 | 0,0 | 91,8 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 4 | 4 | 171 s | 16 | 0,0 | 98,5 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 4 | 4 | 71 s | 0 | 0,3 | 6,0 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 5 | 4 | 176 s | 289 | 0,6 | 90,6 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 4 | 4 | 188 s | 10 | 0,0 | 107,5 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 4 | 4 | 88 s | 0 | 0,5 | 2,5 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 4 | 4 | 191 s | 0 | 0,0 | 116,3 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 6 | 3 | 198 s | 266 | 0,2 | 112,2 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 3 | 3 | 48 s | 122 | 0,0 | 1,7 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 3 | 3 | 187 s | 82 | 0,0 | 126,3 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 3 | 3 | 192 s | 0 | 0,0 | 125,7 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 3 | 3 | 84 s | 56 | 0,0 | 3,7 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 8 ×4, piętro 9 ×4, piętro 10 ×10, piętro 11 ×10, piętro 12 ×1, piętro 13 ×6, piętro 14 ×10, piętro 15 ×8, piętro 16 ×8, piętro 17 ×1, piętro 20 ×2, piętro 21 ×1, piętro 22 ×2, piętro 26 ×2; wyzwań (3 porażki na piętrze): 3 – piętro 15, piętro 22, piętro 26.
**Gracz H&S / souls-like** – ponowne próby: piętro 7 ×1, piętro 8 ×18, piętro 9 ×1, piętro 10 ×10, piętro 11 ×10, piętro 13 ×5, piętro 14 ×9, piętro 15 ×5, piętro 16 ×4, piętro 17 ×3, piętro 22 ×1, piętro 26 ×2; wyzwań (3 porażki na piętrze): 2 – piętro 15, piętro 26.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 410 | 2,9 s | 4,4 | 1670 | 27688 | 66 | 8 |
| Tytan Otchłani | 11 | 65,6 s | 183,8 | 587 | 21139 | 1922 | 4 |
| Wiedźma wichru | 952 | 2,7 s | 3,3 | 4004 | 15738 | 16 | 1 |
| Kultysta topieli | 1121 | 3,2 s | 3,6 | 2022 | 12033 | 10 | 3 |
| Grzebacz | 336 | 2,7 s | 5,6 | 466 | 8323 | 24 | 1 |
| Królowa Topieli | 36 | 103,3 s | 147,6 | 1405 | 7711 | 214 | 0 |
| Ogar otchłani | 2113 | 1,1 s | 2,6 | 1291 | 7409 | 3 | 0 |
| Taranowiec | 1233 | 2,5 s | 5,0 | 442 | 6094 | 5 | 3 |
| Legionista | 1000 | 7,0 s | 10,1 | 685 | 5283 | 5 | 3 |
| Kolos | 1028 | 5,4 s | 7,2 | 128 | 5218 | 5 | 0 |
| Harpia | 881 | 1,5 s | 3,3 | 1629 | 4978 | 6 | 2 |
| Kultysta | 2056 | 1,4 s | 2,0 | 872 | 4945 | 2 | 2 |
| Kolos żaru | 350 | 2,8 s | 8,5 | 66 | 2497 | 7 | 1 |
| Król Kości | 6 | 81,2 s | 375,7 | 169 | 2287 | 381 | 0 |
| Tkaczka | 242 | 2,9 s | 6,6 | 430 | 2127 | 9 | 1 |
| Czerw | 5385 | 0,6 s | 1,6 | 457 | 2075 | 0 | 0 |
| Upiór | 234 | 3,4 s | 7,9 | 560 | 1982 | 8 | 1 |
| Siarkowy pomiot | 1355 | 0,5 s | 1,6 | 60 | 1853 | 1 | 0 |
| Gnilec | 522 | 5,8 s | 13,3 | 212 | 1710 | 3 | 2 |
| Cień | 450 | 1,6 s | 4,0 | 185 | 1420 | 3 | 1 |
| Gnilec (odłamek) | 1003 | 2,6 s | 5,0 | 230 | 1418 | 1 | 0 |
| Pajączek | 265 | 0,3 s | 1,3 | 259 | 1390 | 5 | 0 |
| Matka Burz | 7 | 102,6 s | 255,0 | 218 | 1163 | 166 | 0 |
| Ofiarnik | 324 | 3,0 s | 3,4 | 495 | 1148 | 3 | 0 |
| Ognisty chochlik | 914 | 1,1 s | 2,5 | 89 | 1108 | 1 | 0 |
| Pan Sfory | 40 | 42,2 s | 52,3 | 454 | 1088 | 27 | 0 |
| Gargulec | 296 | 3,8 s | 8,5 | 115 | 1082 | 4 | 1 |
| Władca Bramy | 40 | 20,7 s | 27,8 | 251 | 835 | 21 | 0 |
| Rzeźnik | 22 | 54,7 s | 120,3 | 148 | 820 | 37 | 0 |
| Ożywiony nietoperz | 503 | 0,4 s | 1,4 | 98 | 633 | 1 | 0 |
| Nekromanta | 120 | 3,9 s | 6,8 | 79 | 536 | 4 | 0 |
| Kamienny chochlik | 864 | 0,9 s | 2,1 | 54 | 437 | 1 | 0 |
| Kowal Piekieł | 3 | 42,9 s | 347,0 | 16 | 366 | 122 | 0 |
| Wodny chochlik | 836 | 1,0 s | 2,3 | 45 | 296 | 0 | 1 |
| Chochlik wichru | 832 | 0,8 s | 2,2 | 43 | 281 | 0 | 0 |
| Arcydiabeł | 3 | 76,5 s | 591,3 | 40 | 169 | 56 | 0 |
| Bezimienny Cień | 5 | 99,0 s | 518,6 | 71 | 62 | 12 | 0 |
| Czerw (elita) | 27 | 4,1 s | 4,1 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 72 | 4,1 s | 8,1 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 40 | 8,4 s | 21,3 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 32 | 14,5 s | 32,3 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 28 | 9,6 s | 14,3 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 51 | 5,1 s | 6,6 | ↑ | ↑ | ↑ |  |
| Worek jaj | 166 | 1,2 s | 2,3 | 0 | 0 | 0 | 0 |
| Miotacz głazów (elita) | 11 | 7,4 s | 18,8 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 60 | 4,7 s | 7,7 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 10 | 11,3 s | 10,5 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 28 | 18,0 s | 37,1 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 6 | 21,9 s | 51,8 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 39 | 2,5 s | 4,8 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 5 | 8,4 s | 18,0 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 12 | 13,1 s | 29,8 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 6 | 22,4 s | 73,7 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 21 | 7,0 s | 12,1 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 26 | 8,0 s | 13,6 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 10 | 5,1 s | 21,8 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 4 | 8,1 s | 30,0 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 7 | 14,4 s | 31,4 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 7 | 9,7 s | 39,1 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3162 | 53% | 0% | 0% | 47% | 0% |
| ciężki | 343 | 31% | 0% | 0% | 69% | 0% |
| nieparowalny | 2367 | 44% | 0% | 0% | 56% | 0% |
| nieblokowalny | 130 | 46% | 0% | 0% | 54% | 0% |
| obszarowy | 4177 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3367 | 37% | 0% | 0% | 63% | 0% |
| ciężki | 334 | 25% | 0% | 0% | 75% | 0% |
| nieparowalny | 1935 | 29% | 0% | 0% | 71% | 0% |
| nieblokowalny | 107 | 47% | 0% | 0% | 53% | 0% |
| obszarowy | 4123 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 3142 | 23% | 24% | 4% | 20% | 29% |
| Tester QA (0,30 s) | ciężki | 670 | 1% | 1% | 5% | 19% | 74% |
| Tester QA (0,30 s) | nieparowalny | 1391 | 7% | 2% | 10% | 31% | 50% |
| Tester QA (0,30 s) | nieblokowalny | 58 | 7% | 0% | 3% | 24% | 66% |
| Tester QA (0,30 s) | obszarowy | 2033 | 2% | 2% | 5% | 38% | 53% |
| Tester QA (0,30 s) | pocisk | 3449 | 2% | 9% | 8% | 13% | 67% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3973 | 7% | 22% | 24% | 11% | 36% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 619 | 1% | 0% | 1% | 17% | 80% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1060 | 7% | 2% | 3% | 21% | 68% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 45 | 2% | 0% | 0% | 11% | 87% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 2304 | 2% | 1% | 6% | 29% | 62% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 4918 | 2% | 8% | 8% | 13% | 69% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 929 |
| Pajączek: Pajączek | 0,31 s | 470 |
| Wodny chochlik: Pazury | 0,33 s | 172 |
| Ognisty chochlik: Pazury | 0,33 s | 272 |
| Chochlik wichru: Pazury | 0,33 s | 160 |
| Kamienny chochlik: Pazury | 0,33 s | 207 |
| Czerw: Ugryzienie | 0,33 s | 1983 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 257 |
| Władca Bramy: Cięcie w szale | 0,41 s | 18 |
| Ognisty chochlik: Drugie cięcie | 0,43 s | 8 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 84% (n=173) | 77% (n=30) | 85% (n=168) | 100% (n=1) | 100% (n=161) | 77% (n=118) |
| niezauważony | 82% (n=524) | – | 82% (n=197) | 100% (n=41) | 100% (n=2270) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 79% (n=143) |
| unik | 38% (n=1081) | 18% (n=193) | 22% (n=1074) | 10% (n=67) | 100% (n=51) | 32% (n=453) |
| unik (eksperyment) | 39% (n=212) | 10% (n=48) | 27% (n=213) | 0% (n=9) | 100% (n=181) | 25% (n=73) |
| wyjście z zasięgu | 57% (n=243) | 43% (n=37) | 49% (n=82) | – | 100% (n=1226) | 48% (n=216) |
| wyjście z zasięgu (eksperyment) | 79% (n=301) | 80% (n=35) | 84% (n=232) | 92% (n=12) | 100% (n=182) | 54% (n=132) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 205,4 | reakcja na cios wręcz 190,3, unik przed pociskiem 68,9, wyjście z okrążenia 7,9, wabienie pod czar obszarowy 7,7 | w trakcie uniku 22,2, drgnięcie (trafiony) 21,6, brak wytrzymałości 9,4, picie flaszki 2,7, unieruchomienie 1,0, inna 0,2, szybki atak 0,1, umiejętność 0,1 | 72% (n=3805) / 88% (n=1377) |
| Gracz H&S / souls-like | 239,5 | reakcja na cios wręcz 212,9, unik przed pociskiem 111,1, doskok do odsłoniętego celu 15,0, wyjście z okrążenia 3,7, wabienie pod czar obszarowy 1,5 | w trakcie uniku 47,7, drgnięcie (trafiony) 19,9, brak wytrzymałości 16,1, picie flaszki 3,5, unieruchomienie 1,1, inna 0,1 | 81% (n=4257) / 92% (n=2221) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Powietrze | Nova powietrza | 5 | 0 | 17,6 | 703,4 |
| Tester QA | Ogień | Nova ognia | 5 | 0 | 14,0 | 473,2 |
| Tester QA | Ziemia | Nova ziemi | 5 | 0 | 13,8 | 440,4 |
| Tester QA | Woda | Nova wody | 5 | 0 | 11,6 | 369,8 |
| Gracz H&S / souls-like | Woda | Nova wody | 13 | 3 | 16,7 | 705,3 |
| Gracz H&S / souls-like | Ogień | Nova ognia | 3 | 0 | 12,3 | 410,3 |
| Gracz H&S / souls-like | Powietrze | Nova powietrza | 2 | 0 | 11,5 | 374,5 |
| Gracz H&S / souls-like | Ziemia | Nova ziemi | 2 | 0 | 10,0 | 301,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka powietrza | 867298 | 26% |  |  |
| Różdżka ziemi | 599881 | 18% |  |  |
| Różdżka ognia | 444133 | 14% |  |  |
| Różdżka wody | 284548 | 9% |  |  |
| Nova powietrza | 242255 | 7% | 612 | 396 |
| Ściana ognia | 193108 | 6% | 928 | 208 |
| Nova ognia | 158505 | 5% | 580 | 273 |
| Odskok widma | 126068 | 4% |  |  |
| Pocisk arkanów | 115037 | 4% | 1056 | 109 |
| Nova wody | 113326 | 3% | 408 | 278 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Przeciążenie ×1475, Okrzyk wojenny ×33.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka wody | 2908597 | 60% |  |  |
| Płonąca ziemia | 365766 | 8% | 287 | 1274 |
| Fala przypływu | 235715 | 5% | 48 | 4911 |
| Kula ognia | 230934 | 5% | 791 | 292 |
| Różdżka ognia | 201959 | 4% |  |  |
| Odskok widma | 155403 | 3% |  |  |
| Łańcuch potępionych | 141044 | 3% | 226 | 624 |
| Łańcuchy potępionych | 125919 | 3% | 199 | 633 |
| Różdżka powietrza | 99095 | 2% |  |  |
| Różdżka ziemi | 83227 | 2% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Przeciążenie ×991, Krwawy pakt ×5, Lodowa włócznia ×1.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,2, mediana 4,0, najwięcej 6 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 35% | 99,2 | 155 | 0,6 |
| klasa | 26% | 73,8 | – | 0,2 |
| sklep | 3% | 8,0 | 177 | 3,3 |
| przedmiot | 36% | 100,7 | 310 | 0,6 |

Oferty sklepu dusz: widziane 795, kupione 3, odrzucone 0, za mało dusz 262.
Zamiany w slotach: 0,7 na podejście; najczęstsze: Ściana ognia → Pocisk arkanów ×5, Pocisk arkanów → Ściana ognia ×4, Ściana ognia → Szarża ×1, Pocisk arkanów → Salwa ×1, Nova powietrza → Pocisk arkanów ×1, Pocisk arkanów → Okrzyk wojenny ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Przeciążenie | klasa | 20 | 73,8 | – | 0,2 |
| Pocisk arkanów | odblokowanie | 20 | 52,8 | 109 | 0,7 |
| Ściana ognia | odblokowanie | 20 | 46,4 | 208 | 0,5 |
| Nova powietrza | przedmiot | 5 | 122,4 | 396 | 0,7 |
| Nova ognia | przedmiot | 5 | 116,0 | 273 | 0,5 |
| Nova ziemi | przedmiot | 5 | 82,8 | 265 | 0,6 |
| Nova wody | przedmiot | 5 | 81,6 | 278 | 0,6 |
| Szarża | sklep | 1 | 125,0 | 220 | 4,0 |
| Okrzyk wojenny | sklep | 1 | 33,0 | – | 0,4 |
| Salwa | sklep | 1 | 1,0 | 637 | 4,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,0, mediana 4,0, najwięcej 9 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 48% | 84,7 | 419 | 0,5 |
| klasa | 28% | 49,6 | – | 0,2 |
| sklep | 19% | 33,2 | 940 | 2,0 |
| przedmiot | 6% | 10,9 | 342 | 0,5 |

Oferty sklepu dusz: widziane 831, kupione 15, odrzucone 4, za mało dusz 258.
Zamiany w slotach: 3,3 na podejście; najczęstsze: Nova wody → Kula ognia ×11, Kojące światło → Płonąca ziemia ×10, Kojące światło → Kula ognia ×4, Kojące światło → Burza ×3, Ściana ognia → Mroźna strzała ×1, Pocisk arkanów → Krwawy pakt ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Przeciążenie | klasa | 20 | 49,6 | – | 0,2 |
| Kula ognia | odblokowanie | 15 | 52,7 | 292 | 0,6 |
| Pocisk arkanów | odblokowanie | 17 | 24,5 | 107 | 0,4 |
| Płonąca ziemia | odblokowanie | 12 | 23,9 | 1274 | 0,2 |
| Łańcuch potępionych | sklep | 1 | 226,0 | 624 | 1,0 |
| Nova ognia | przedmiot | 4 | 55,0 | 335 | 0,5 |
| Łańcuchy potępionych | sklep | 1 | 199,0 | 633 | 4,0 |
| Rozprysk arkanów | odblokowanie | 3 | 34,3 | 136 | 0,7 |
| Przyzwanie chochlików | sklep | 1 | 71,0 | 825 | 2,0 |
| Fala przypływu | sklep | 1 | 48,0 | 4911 | 2,0 |
| Burza | odblokowanie | 4 | 8,3 | 1195 | 0,0 |
| Kamienne kolce | sklep | 1 | 32,0 | 359 | 1,0 |
| Ściana ognia | odblokowanie | 1 | 23,0 | 289 | 0,5 |
| Nova powietrza | przedmiot | 1 | 22,0 | 186 | 0,0 |
| Krąg żaru | sklep | 1 | 22,0 | 953 | 1,0 |
| Łańcuch błyskawic | odblokowanie | 1 | 14,0 | 290 | 0,0 |
| Nova wody | przedmiot | 1 | 8,0 | 186 | 0,0 |
| Lodowy podmuch | odblokowanie | 1 | 7,0 | 191 | 0,0 |
| Mroźna fala | odblokowanie | 1 | 7,0 | 214 | 0,0 |
| Włócznia potępionych | odblokowanie | 3 | 2,3 | 97 | 0,0 |
| Iskra | sklep | 2 | 3,5 | 67 | 2,0 |
| Tąpnięcie | sklep | 1 | 6,0 | 107 | 0,0 |
| Kościany golem | sklep | 1 | 6,0 | 2876 | 2,0 |
| Krwawy pakt | odblokowanie | 1 | 5,0 | – | 0,0 |
| Nova ziemi | przedmiot | 1 | 5,0 | 125 | 0,0 |
| Oko cyklonu | sklep | 1 | 5,0 | 263 | 1,0 |
| Piekielna kula | sklep | 1 | 3,0 | 1816 | 2,0 |
| Lodowa włócznia | sklep | 1 | 1,0 | – | 2,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 973,8, czar obszarowy na jedynego wroga 547,1, umiejętność 281,6, unik 205,4, czar obszarowy na grupę 173,0, drgnięcie (trafiony) 89,5, czar obszarowy na jednego (test) 52,2, flaszka (próba) 30,5, wabienie bez skutku 20,2, flaszka many (próba) 18,0, czar obszarowy na jednego (ratunek) 12,7, czar obszarowy po zwabieniu 4,1.
Czas w bloku 0 s, w biegu 63 s, bez wytrzymałości (<10%) 129 s na podejście.
Zakupy: dodatkowa flaszka ×23, ulepszenie: Pocisk arkanów ×17, ulepszenie: Ściana ognia ×14, sklep: Pancerz ×10, założony przedmiot: Kolczuga ×10, ulepszenie: Nova powietrza ×10, ulepszenie: Nova ziemi ×10, ulepszenie: Nova ognia ×9.
Punkty cech: Intelligence 76, Toughness 69, Dexterity 62, Strength 58.
Pieczęcie na podejście: z daru bossa 4,2, z sakwy do skilli 2,9, kupione 0,4; najczęstsze: Serce otchłani ×12, Deszcz piekieł ×9, Wieczny płomień ×9, Klątwa Mokoszy ×9, Pakt z diabłem ×7, Oko Czarnoboga ×6.
Komunikaty gry: „Sloty przegrzane” ×21549; „PRZECIĄŻENIE” ×1416; „WYŁADOWANIE!” ×1127; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×253; „Portale gasną – dobij pozostałe demony” ×191; „OGŁUSZENIE!” ×110; „POLEGŁEŚ” ×88; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×84.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 893,5, unik 239,5, umiejętność 178,3, drgnięcie (trafiony) 70,0, czar obszarowy na jedynego wroga 64,8, czar obszarowy na grupę 22,1, flaszka (próba) 15,5, wabienie bez skutku 6,1, flaszka many (próba) 4,5, czar obszarowy na jednego (ratunek) 1,2, czar obszarowy po zwabieniu 1,1.
Czas w bloku 0 s, w biegu 64 s, bez wytrzymałości (<10%) 149 s na podejście.
Zakupy: sklep: Twarda skóra ×29, sklep: Szybkie ręce ×24, dodatkowa flaszka ×23, sklep: Nurt ×20, sklep: Furia ×18, sklep: Przebicie ×17, sklep: Hart ciała ×17, sklep: Rykoszet ×16.
Punkty cech: Intelligence 191, Toughness 86.
Pieczęcie na podejście: z daru bossa 4,3, z sakwy do skilli 1,4, kupione 0,0; najczęstsze: Pakt z diabłem ×15, Serce otchłani ×11, Deszcz piekieł ×9, Oko Czarnoboga ×9, Nieskończoność ×7, Klątwa Mokoszy ×6.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Kontra, Rozprysk arkanów, Mroźna fala, Krwawy pakt, Mroźna zbroja, Łańcuch błyskawic, Włócznia potępionych, Płonąca ziemia, Burza, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Szata popiołu, Kosa, Wielki topór, Młot burzy, Kojące światło, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „Sloty przegrzane” ×5569; „PRZECIĄŻENIE” ×954; „WYŁADOWANIE!” ×699; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×270; „Portale gasną – dobij pozostałe demony” ×206; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×89; „POLEGŁEŚ” ×85; „OGŁUSZENIE!” ×78.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×1007, uśpiony wróg blokuje koniec piętra ×96, zablokowany ruch ×11, limit czasu piętra ×1.
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; -0,7) (w 2 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (12,0; -11,3)
  - piętro 18 „XVIII. Matka Burz”: brak postępu przy (0,0; -1,4)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,2; 1,1)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×996, uśpiony wróg blokuje koniec piętra ×86, zablokowany ruch ×5, limit czasu piętra ×1.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** 1 podejść przerwanych limitem czasu piętra – możliwy nieosiągalny wróg albo zbyt długie piętro (zob. anomalie).
- **Problem (P2):** zablokowany ruch średnio 0,6× na podejście (geometria areny/kolizje). Przykłady: piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; -0,7); piętro 9 „IX. Królowa Topieli”: brak postępu przy (12,0; -11,3); piętro 18 „XVIII. Matka Burz”: brak postępu przy (0,0; -1,4).
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 9% ciosów (n=76).
- Unik przed ciężkim: działa – rani tylko 17% ciosów (n=241).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=1408) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 12% pocisków, przed którymi był unik (n=1377).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 47% decyzji zapada, gdy do trafienia zostało <0,1 s (n=3142) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=670).
- Telegraf „nieparowalny” daje czas: tylko 9% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1391).
- Telegraf „nieblokowalny” daje czas: tylko 7% decyzji zapada mniej niż 0,1 s przed trafieniem (n=58).
- Telegraf „obszarowy” daje czas: tylko 4% decyzji zapada mniej niż 0,1 s przed trafieniem (n=2033).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- UX: gra odmawiała akcji: „Sloty przegrzane” ×21549, „Za mało zdrowia” ×80 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XI. Rzeźnia” trwa średnio 174 s – najdłuższy fragment wieży.
- Tempo: piętro „XIV. Kamienne Trzewia” trwa średnio 150 s – najdłuższy fragment wieży.
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 165 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 172 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 180 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 189 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 177 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 180 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIV. Bezimienny Cień” trwa średnio 167 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 194 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 93 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 3/20. Trudno, ale do zrobienia – tak ma być w souls-like.
- Progres między podejściami słabo odczuwalny: 15,1 → 14,9 średnio. Meta-progresja niewiele zmienia w walce.
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Rzeźnik (122,0 trafień, 43,9 s), Królowa Topieli (112,5 trafień, 79,4 s), Pan Sfory (47,9 trafień, 36,5 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,2), Ożywiony nietoperz (1,4), Siarkowy pomiot (1,6) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Królowa Topieli – średnio 79 s i 113 trafień do zabicia.
- Długie starcie: Rzeźnik – średnio 44 s i 122 trafień do zabicia.
- Długie starcie: Tytan Otchłani – średnio 51 s i 175 trafień do zabicia.
- Długie starcie: Matka Burz – średnio 78 s i 246 trafień do zabicia.
- Długie starcie: Król Kości – średnio 66 s i 354 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 83 s i 441 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 43 s i 347 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 77 s i 591 trafień do zabicia.
- 16/16 razy zginąłem z flaszkami w kieszeni. 8 z tych śmierci to utrata ≥50% HP w 3 s, a 62% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 29% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Fala przypływu”: 4911 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 19%. Różnych skilli na podejście: średnio 5,0.
- Najczęściej grałem: Woda + Nova wody (13×, śr. piętro 16,9). Inne: Ogień + Nova ognia 3× (12,3), Powietrze + Nova powietrza 2× (11,5), Ziemia + Nova ziemi 2× (10,0).
- Unik to moje główne narzędzie: 239,5 na podejście, w tym 18,7 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 92% przypadków.
- Gra nie wykonała uniku od razu średnio 88,3× na podejście – najczęściej: w trakcie poprzedniego uniku 47,7, drgnięcie (trafiony) 19,9, brak wytrzymałości 16,1. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (70 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

