# Raport botów grających — Wojownik

Wygenerowano: 2026-10-29 09:42. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 14,1 | 12,0 | 28,5 min | 531,2 | 213704 / 11068 | 33,0 | 1% |
| Gracz H&S / souls-like | 20 | 11 (55%) | 22,8 | 30,0 | 44,9 min | 1098,7 | 702284 / 12865 | 34,0 | 4% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 26 14 10 22 23 9 11 10 9 25 10 7 14 16 22 9 9 13 9 13
- Gracz H&S / souls-like: 8 ★ 8 9 ★ ★ ★ 8 19 17 27 19 10 ★ ★ ★ ★ ★ ★ ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×3, 11. XI. Rzeźnia ×1, 13. XIII. Kamienne Trzewia ×2, 14. XIV. Kamienne Trzewia ×2, 16. XVI. Wieża Burz ×1, 22. XXII. Chór Cieni ×2, 23. XXIII. Chór Cieni ×1, 25. XXV. Kuźnia Piekieł ×1, 26. XXVI. Kuźnia Piekieł ×1, 7. VII. Zatopione Krużganki ×1, 9. IX. Królowa Topieli ×5.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×6; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×3; Królowa Topieli – „Królowa Topieli: Combo” ×3; Grzebacz – „Grzebacz” ×1; Wodny chochlik – „Pazury” ×1; Królowa Topieli – „Królowa Topieli: TidalWave” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 12/20.
  Ostatnie 3 s przed śmiercią: średnio 30% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/20). Picie: 787 prób, 659 wypitych (16% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 10. X. Rzeźnia ×1, 17. XVII. Wieża Burz ×1, 19. XIX. Kostnica ×2, 27. XXVII. Kowal Piekieł ×1, 8. VIII. Zatopione Krużganki ×3, 9. IX. Królowa Topieli ×1.
  Ostatni cios: Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×1; Kultysta – „Kula ognia” ×1; Królowa Topieli – „Królowa Topieli: Volley” ×1; Legionista – „Legionista” ×1; Czerw – „Ugryzienie” ×1; Kultysta topieli – „Kultysta topieli” ×1. Śmierć ciosem ≥40% maks. HP: 0/9. Zginął mając flaszki: 6/9.
  Ostatnie 3 s przed śmiercią: średnio 26% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/9). Picie: 756 prób, 679 wypitych (10% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 103 s | 12 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 110 s | 63 | 0,1 | 41,5 |
| Tester QA | 3. III. Władca Bramy | 20 | 20 | 28 s | 210 | 0,5 | 7,6 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 112 s | 71 | 0,5 | 45,8 |
| Tester QA | 5. V. Psiarnia Piekieł | 20 | 20 | 92 s | 75 | 0,2 | 40,2 |
| Tester QA | 6. VI. Pan Sfory | 20 | 20 | 34 s | 402 | 2,0 | 5,8 |
| Tester QA | 7. VII. Zatopione Krużganki | 23 | 19 | 100 s | 287 | 1,0 | 38,0 |
| Tester QA | 8. VIII. Zatopione Krużganki | 26 | 19 | 129 s | 544 | 2,3 | 48,3 |
| Tester QA | 9. IX. Królowa Topieli | 36 | 14 | 69 s | 1108 | 3,5 | 1,3 |
| Tester QA | 10. X. Rzeźnia | 24 | 11 | 112 s | 752 | 2,6 | 46,3 |
| Tester QA | 11. XI. Rzeźnia | 21 | 10 | 137 s | 919 | 2,3 | 48,4 |
| Tester QA | 12. XII. Rzeźnik | 10 | 10 | 65 s | 291 | 1,0 | 6,4 |
| Tester QA | 13. XIII. Kamienne Trzewia | 14 | 8 | 140 s | 1048 | 3,4 | 56,1 |
| Tester QA | 14. XIV. Kamienne Trzewia | 14 | 6 | 137 s | 998 | 2,9 | 47,1 |
| Tester QA | 15. XV. Tytan Otchłani | 11 | 6 | 66 s | 976 | 2,3 | 1,8 |
| Tester QA | 16. XVI. Wieża Burz | 8 | 5 | 144 s | 1026 | 2,5 | 71,8 |
| Tester QA | 17. XVII. Wieża Burz | 8 | 5 | 158 s | 1005 | 2,6 | 77,5 |
| Tester QA | 18. XVIII. Matka Burz | 5 | 5 | 72 s | 645 | 2,2 | 2,6 |
| Tester QA | 19. XIX. Kostnica | 6 | 5 | 156 s | 1192 | 3,7 | 96,0 |
| Tester QA | 20. XX. Kostnica | 5 | 5 | 175 s | 1012 | 3,8 | 98,8 |
| Tester QA | 21. XXI. Król Kości | 7 | 5 | 78 s | 760 | 2,6 | 14,1 |
| Tester QA | 22. XXII. Chór Cieni | 12 | 3 | 184 s | 1204 | 2,4 | 66,5 |
| Tester QA | 23. XXIII. Chór Cieni | 5 | 2 | 175 s | 723 | 0,2 | 46,6 |
| Tester QA | 24. XXIV. Bezimienny Cień | 2 | 2 | 104 s | 659 | 2,5 | 4,0 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 4 | 1 | 171 s | 903 | 0,3 | 48,5 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 3 | 0 | 0 s | 1483 | 0,7 | 49,7 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 101 s | 6 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 20 | 111 s | 24 | 0,1 | 41,1 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 27 s | 154 | 0,3 | 7,6 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 20 | 20 | 108 s | 55 | 0,5 | 43,3 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 20 | 20 | 98 s | 58 | 0,5 | 39,7 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 20 | 20 | 30 s | 268 | 1,1 | 6,1 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 22 | 20 | 107 s | 330 | 1,0 | 38,3 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 27 | 17 | 125 s | 616 | 1,7 | 42,4 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 19 | 16 | 63 s | 777 | 2,5 | 2,5 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 19 | 15 | 108 s | 396 | 1,3 | 51,0 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 18 | 15 | 129 s | 539 | 2,1 | 60,1 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 15 | 15 | 54 s | 143 | 0,3 | 5,8 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 16 | 15 | 130 s | 382 | 1,6 | 64,1 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 17 | 15 | 133 s | 415 | 1,8 | 62,8 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 17 | 15 | 50 s | 942 | 2,8 | 2,8 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 19 | 15 | 150 s | 605 | 2,3 | 71,5 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 23 | 14 | 151 s | 971 | 2,9 | 70,0 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 14 | 14 | 64 s | 847 | 2,2 | 2,9 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 19 | 12 | 156 s | 814 | 1,6 | 72,3 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 13 | 12 | 168 s | 516 | 0,8 | 93,6 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 12 | 12 | 53 s | 314 | 0,4 | 10,7 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 12 | 12 | 175 s | 532 | 0,9 | 101,8 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 14 | 12 | 179 s | 1109 | 2,1 | 102,2 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 12 | 12 | 59 s | 266 | 0,4 | 3,8 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 14 | 12 | 185 s | 785 | 1,4 | 106,1 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 12 | 12 | 180 s | 711 | 1,7 | 114,7 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 14 | 11 | 40 s | 771 | 1,6 | 1,6 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 14 | 11 | 186 s | 816 | 1,6 | 115,4 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 14 | 11 | 195 s | 887 | 1,6 | 116,6 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 11 | 11 | 62 s | 717 | 0,8 | 6,1 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 7 ×3, piętro 8 ×7, piętro 9 ×17, piętro 10 ×10, piętro 11 ×10, piętro 13 ×4, piętro 14 ×6, piętro 15 ×5, piętro 16 ×2, piętro 17 ×3, piętro 19 ×1, piętro 21 ×2, piętro 22 ×7, piętro 23 ×2, piętro 25 ×2, piętro 26 ×2; wyzwań (3 porażki na piętrze): 1 – piętro 26.
**Gracz H&S / souls-like** – ponowne próby: piętro 7 ×2, piętro 8 ×7, piętro 9 ×2, piętro 10 ×3, piętro 11 ×3, piętro 13 ×1, piętro 14 ×2, piętro 15 ×2, piętro 16 ×4, piętro 17 ×8, piętro 19 ×5, piętro 20 ×1, piętro 23 ×2, piętro 25 ×2, piętro 27 ×2, piętro 28 ×3, piętro 29 ×3; wyzwań (3 porażki na piętrze): 5 – piętro 8, piętro 9, piętro 19, piętro 27.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 889 | 0,7 s | 1,6 | 1470 | 54030 | 58 | 7 |
| Królowa Topieli | 30 | 61,6 s | 51,8 | 2095 | 52442 | 1748 | 6 |
| Wiedźma wichru | 1505 | 1,1 s | 1,8 | 3719 | 41365 | 27 | 4 |
| Kultysta topieli | 1732 | 0,5 s | 1,3 | 1818 | 37422 | 21 | 2 |
| Tytan Otchłani | 21 | 50,2 s | 74,1 | 533 | 26651 | 1269 | 0 |
| Kultysta | 2561 | 0,3 s | 1,2 | 1293 | 22794 | 9 | 1 |
| Harpia | 1204 | 0,5 s | 1,7 | 1630 | 21256 | 17 | 2 |
| Grzebacz | 689 | 0,7 s | 2,0 | 553 | 20218 | 29 | 1 |
| Kolos | 1651 | 1,5 s | 2,5 | 500 | 18867 | 11 | 0 |
| Kolos żaru | 930 | 1,5 s | 3,0 | 497 | 18094 | 19 | 0 |
| Matka Burz | 19 | 61,9 s | 94,0 | 594 | 14455 | 761 | 0 |
| Pan Sfory | 40 | 27,8 s | 22,4 | 645 | 12649 | 316 | 0 |
| Upiór | 596 | 1,3 s | 2,2 | 1382 | 12104 | 20 | 1 |
| Ogar otchłani | 2622 | 0,3 s | 1,4 | 1238 | 11484 | 4 | 0 |
| Kowal Piekieł | 11 | 36,5 s | 125,8 | 309 | 10757 | 978 | 1 |
| Król Kości | 17 | 56,5 s | 103,9 | 537 | 8875 | 522 | 0 |
| Siarkowy pomiot | 1802 | 0,1 s | 1,1 | 203 | 8091 | 4 | 0 |
| Arcydiabeł | 11 | 56,9 s | 178,9 | 319 | 7852 | 714 | 0 |
| Gnilec | 835 | 1,8 s | 3,4 | 457 | 7149 | 8 | 0 |
| Taranowiec | 1688 | 0,8 s | 2,0 | 560 | 7145 | 4 | 0 |
| Władca Bramy | 40 | 23,7 s | 14,0 | 328 | 6797 | 170 | 0 |
| Gnilec (odłamek) | 1693 | 0,6 s | 1,9 | 482 | 6363 | 4 | 0 |
| Legionista | 1445 | 2,4 s | 3,6 | 834 | 6327 | 4 | 1 |
| Tkaczka | 448 | 1,3 s | 2,2 | 492 | 6269 | 14 | 0 |
| Cień | 1036 | 0,4 s | 1,6 | 470 | 6266 | 6 | 0 |
| Czerw | 6216 | 0,2 s | 1,2 | 1373 | 5211 | 1 | 1 |
| Rzeźnik | 25 | 54,3 s | 56,1 | 341 | 4801 | 192 | 0 |
| Bezimienny Cień | 14 | 61,6 s | 145,1 | 397 | 4369 | 312 | 0 |
| Gargulec | 544 | 1,2 s | 2,6 | 196 | 3821 | 7 | 0 |
| Nekromanta | 399 | 0,9 s | 2,2 | 611 | 2915 | 7 | 0 |
| Ofiarnik | 531 | 0,5 s | 1,4 | 424 | 2883 | 5 | 0 |
| Ożywiony nietoperz | 967 | 0,1 s | 1,1 | 435 | 2053 | 2 | 0 |
| Ognisty chochlik | 1033 | 0,2 s | 1,2 | 167 | 1714 | 2 | 0 |
| Pajączek | 204 | 0,3 s | 1,1 | 187 | 1473 | 7 | 1 |
| Wodny chochlik | 885 | 0,2 s | 1,2 | 129 | 1330 | 2 | 1 |
| Kamienny chochlik | 897 | 0,1 s | 1,1 | 106 | 1263 | 1 | 0 |
| Chochlik wichru | 863 | 0,5 s | 1,5 | 116 | 1102 | 1 | 0 |
| Siarkowy pomiot (elita) | 59 | 0,7 s | 2,4 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 67 | 1,6 s | 3,1 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 80 | 2,1 s | 2,7 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 45 | 5,5 s | 5,1 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 48 | 6,9 s | 8,6 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 12 | 3,7 s | 7,8 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 14 | 5,7 s | 6,2 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 19 | 3,5 s | 4,4 | ↑ | ↑ | ↑ |  |
| Worek jaj | 175 | 0,6 s | 1,2 | 0 | 0 | 0 | 0 |
| Legionista (elita) | 46 | 9,8 s | 11,5 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 37 | 4,1 s | 5,3 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 49 | 3,8 s | 6,6 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 57 | 3,5 s | 3,4 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 66 | 2,2 s | 4,1 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 17 | 7,6 s | 8,6 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 32 | 1,0 s | 2,1 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 35 | 2,0 s | 4,2 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 23 | 4,9 s | 12,4 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 30 | 2,4 s | 5,0 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 23 | 6,8 s | 14,8 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 17 | 4,1 s | 6,6 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 12 | 7,5 s | 9,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 4464 | 67% | 11% | 2% | 17% | 4% |
| ciężki | 482 | 55% | 9% | 6% | 26% | 4% |
| nieparowalny | 3276 | 63% | 15% | 0% | 17% | 5% |
| nieblokowalny | 205 | 78% | 0% | 0% | 22% | 0% |
| obszarowy | 4559 | 81% | 16% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 4759 | 54% | 13% | 10% | 20% | 2% |
| ciężki | 787 | 40% | 10% | 14% | 35% | 1% |
| nieparowalny | 3369 | 49% | 28% | 0% | 19% | 4% |
| nieblokowalny | 309 | 77% | 0% | 0% | 23% | 0% |
| obszarowy | 5230 | 70% | 27% | 0% | 0% | 3% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 3673 | 24% | 30% | 4% | 16% | 26% |
| Tester QA (0,30 s) | ciężki | 683 | 3% | 2% | 4% | 23% | 68% |
| Tester QA (0,30 s) | nieparowalny | 1316 | 4% | 4% | 7% | 27% | 57% |
| Tester QA (0,30 s) | nieblokowalny | 84 | 0% | 1% | 7% | 19% | 73% |
| Tester QA (0,30 s) | obszarowy | 2439 | 2% | 2% | 2% | 22% | 72% |
| Tester QA (0,30 s) | pocisk | 2940 | 4% | 18% | 13% | 19% | 46% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 4594 | 5% | 18% | 27% | 14% | 37% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 1099 | 3% | 2% | 3% | 26% | 66% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1613 | 3% | 2% | 3% | 14% | 79% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 73 | 0% | 0% | 0% | 7% | 93% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 3343 | 1% | 1% | 2% | 15% | 80% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2966 | 3% | 22% | 13% | 16% | 46% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 879 |
| Pajączek: Pajączek | 0,31 s | 305 |
| Ognisty chochlik: Pazury | 0,33 s | 277 |
| Wodny chochlik: Pazury | 0,33 s | 254 |
| Chochlik wichru: Pazury | 0,33 s | 238 |
| Czerw: Ugryzienie | 0,33 s | 2450 |
| Kamienny chochlik: Pazury | 0,33 s | 213 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 675 |
| Władca Bramy: Cięcie w szale | 0,41 s | 26 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 1586 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 53% (n=387) | 13% (n=16) | 42% (n=491) | 100% (n=7) | 45% (n=710) | 78% (n=582) |
| blok (eksperyment) | 71% (n=197) | 63% (n=38) | 71% (n=160) | 100% (n=3) | 74% (n=134) | 64% (n=138) |
| brak reakcji (eksperyment) | 82% (n=177) | 88% (n=34) | 81% (n=191) | 100% (n=11) | 86% (n=169) | 80% (n=118) |
| niezauważony | 80% (n=734) | – | 89% (n=358) | 100% (n=51) | 93% (n=1638) | – |
| parowanie | 61% (n=149) | 62% (n=55) | – | – | 0% (n=2) | – |
| parowanie (eksperyment) | 72% (n=122) | 62% (n=21) | 91% (n=118) | 100% (n=6) | 88% (n=77) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 89% (n=347) |
| unik | 56% (n=836) | 38% (n=177) | 42% (n=649) | 48% (n=82) | 91% (n=23) | 61% (n=328) |
| unik (eksperyment) | 65% (n=250) | 54% (n=24) | 63% (n=159) | 60% (n=5) | 96% (n=274) | 76% (n=91) |
| wyjście z zasięgu | 87% (n=675) | 87% (n=91) | 90% (n=439) | 97% (n=34) | 93% (n=1109) | – |
| wyjście z zasięgu (eksperyment) | 78% (n=175) | 85% (n=26) | 89% (n=97) | 100% (n=6) | 91% (n=123) | 81% (n=72) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 112,3 | reakcja na cios wręcz 112,1, unik przed pociskiem 47,7, wyjście z okrążenia 11,5 | drgnięcie (trafiony) 16,9, brak wytrzymałości 13,0, w trakcie uniku 11,0, picie flaszki 6,3, parowanie 1,6, inna 1,2, unieruchomienie 0,7, blok 0,6, umiejętność 0,5, szybki atak 0,4, mocny atak 0,2, riposta 0,2, przełamana garda 0,1 | 37% (n=2241) / 72% (n=953) |
| Gracz H&S / souls-like | 157,5 | reakcja na cios wręcz 115,2, unik przed pociskiem 64,2, doskok do odsłoniętego celu 26,3, wyjście z okrążenia 15,8 | w trakcie uniku 20,7, drgnięcie (trafiony) 14,4, brak wytrzymałości 7,7, parowanie 4,5, picie flaszki 4,2, unieruchomienie 0,7, inna 0,5, blok 0,3, mocny atak 0,3, riposta 0,2, szybki atak 0,1, umiejętność 0,1 | 56% (n=2304) / 76% (n=1283) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 16,2 | 682,4 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 15,4 | 579,2 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 12,4 | 427,0 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 12,2 | 436,2 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 14 | 10 | 26,6 | 1339,1 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 3 | 1 | 15,3 | 667,7 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 2 | 0 | 14,0 | 499,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 8,0 | 225,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 1158396 | 27% | 2232 | 519 |
| Rozpłatanie | 1034966 | 24% | 1701 | 608 |
| Tąpnięcie | 582856 | 14% | 2339 | 249 |
| Zamach | 422393 | 10% |  |  |
| Cięcie | 339061 | 8% |  |  |
| Okrzyk bojowy | 207026 | 5% | 2058 | 101 |
| Egzekucja | 189604 | 4% | 115 | 1649 |
| Potężne cięcie | 165074 | 4% |  |  |
| Młynek | 101690 | 2% | 276 | 368 |
| Riposta | 20801 | 0% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×35.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 8285339 | 59% | 6834 | 1212 |
| Rozpłatanie | 2095864 | 15% | 3746 | 559 |
| Fala przełamania | 1075217 | 8% | 588 | 1829 |
| Cięcie | 688500 | 5% |  |  |
| Szarża | 583926 | 4% | 2406 | 243 |
| Potężne cięcie | 420163 | 3% |  |  |
| Okrzyk bojowy | 301229 | 2% | 1037 | 290 |
| Riposta | 242829 | 2% |  |  |
| Egzekucja | 140443 | 1% | 239 | 588 |
| Zamach | 80397 | 1% |  |  |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 3,8, mediana 4,0, najwięcej 5 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 41% | 187,1 | 540 | 0,9 |
| odblokowanie | 26% | 117,0 | 249 | 0,7 |
| klasa | 23% | 102,9 | 101 | 0,5 |
| sklep | 10% | 46,0 | 361 | 2,0 |

Oferty sklepu dusz: widziane 783, kupione 19, odrzucone 0, za mało dusz 242.
Zamiany w slotach: 0,5 na podejście; najczęstsze: Cięcie wichru → Tąpnięcie ×2, Rozpłatanie → Uderzenie tarczą ×1, Tąpnięcie → Odtrącenie ×1, Rozpłatanie → Tąpnięcie ×1, Rozpłatanie → Kontra ×1, Tąpnięcie → Okrzyk wojenny ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Tąpnięcie | odblokowanie | 20 | 117,0 | 249 | 0,7 |
| Cięcie wichru | startowy | 10 | 223,2 | 519 | 0,8 |
| Okrzyk bojowy | klasa | 20 | 102,9 | 101 | 0,5 |
| Rozpłatanie | startowy | 11 | 154,6 | 608 | 1,2 |
| Młynek | sklep | 3 | 92,0 | 368 | 2,4 |
| Odtrącenie | sklep | 2 | 66,5 | 43 | 2,0 |
| Egzekucja | sklep | 2 | 57,5 | 139 | 1,4 |
| Cięcie krzyżowe | sklep | 3 | 33,0 | 178 | 1,2 |
| Kontra | sklep | 1 | 41,0 | 7 | 2,0 |
| Okrzyk wojenny | sklep | 2 | 17,5 | – | 2,5 |
| Uderzenie tarczą | sklep | 1 | 22,0 | 735 | 4,0 |
| Upadek kata | sklep | 1 | 7,0 | 100 | 1,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,8, mediana 5,0, najwięcej 10 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 49% | 370,2 | 1112 | 1,2 |
| odblokowanie | 38% | 283,7 | 405 | 0,8 |
| klasa | 7% | 51,9 | 290 | 0,5 |
| sklep | 7% | 50,7 | 1700 | 1,3 |

Oferty sklepu dusz: widziane 1305, kupione 14, odrzucone 48, za mało dusz 373.
Zamiany w slotach: 1,4 na podejście; najczęstsze: Szarża → Egzekucja ×7, Szarża → Fala przełamania ×4, Rozpłatanie → Fala przełamania ×2, Szarża → Cięcie krzyżowe ×2, Cięcie wichru → Uderzenie tarczą ×1, Szarża → Krwawe cięcie ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 17 | 402,0 | 1212 | 1,2 |
| Rozpłatanie | startowy | 20 | 187,3 | 559 | 0,9 |
| Szarża | odblokowanie | 13 | 185,1 | 243 | 0,6 |
| Okrzyk bojowy | klasa | 20 | 51,9 | 290 | 0,5 |
| Fala przełamania | sklep | 8 | 73,5 | 1829 | 1,0 |
| Egzekucja | odblokowanie | 8 | 29,9 | 588 | 0,8 |
| Cięcie krzyżowe | sklep | 2 | 69,0 | 330 | 2,0 |
| Tąpnięcie | odblokowanie | 1 | 45,0 | 150 | 0,0 |
| Seria cięć | sklep | 1 | 42,0 | 514 | 4,0 |
| Rozdarcie ran | sklep | 1 | 17,0 | 180 | 2,0 |
| Kontra | odblokowanie | 1 | 16,0 | 42 | 0,0 |
| Krwawe cięcie | odblokowanie | 1 | 7,0 | 116 | 0,0 |
| Młynek | odblokowanie | 1 | 7,0 | 103 | 0,0 |
| Uderzenie tarczą | odblokowanie | 1 | 6,0 | 87 | 1,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 452,9, szybki atak 199,5, drgnięcie (trafiony) 169,5, unik 112,3, blok 95,8, mocny atak 71,1, flaszka (próba) 39,4, przełamana garda 24,8, parowanie 14,5, riposta 2,8.
Czas w bloku 51 s, w biegu 108 s, bez wytrzymałości (<10%) 448 s na podejście.
Zakupy: dodatkowa flaszka ×26, ulepszenie: Tąpnięcie ×16, ulepszenie: Okrzyk bojowy ×14, sklep: Miecz jednoręczny: poziom 3 ×10, sklep: Miecz jednoręczny: poziom 4 ×10, ulepszenie: Rozpłatanie ×10, sklep: Miecz dwuręczny: poziom 3 ×10, sklep: Szeroki zasięg ×8.
Punkty cech: Strength 73, Toughness 65, Dexterity 62, Intelligence 61.
Pieczęcie na podejście: z daru bossa 4,1, z sakwy do skilli 2,4, kupione 1,0; najczęstsze: Wieczny płomień ×7, Korona Welesa ×7, Klątwa Mokoszy ×7, Oko Czarnoboga ×7, Serce otchłani ×7, Deszcz piekieł ×6.
Komunikaty gry: „OKRZYK!” ×2058; „SZAŁ!” ×976; „GARDA PRZEŁAMANA!” ×516; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×253; „Garda wroga przełamana!” ×203; „Portale gasną – dobij pozostałe demony” ×195; „ZAMROŻENIE!” ×180; „OGŁUSZENIE!” ×164.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 756,4, drgnięcie (trafiony) 183,2, unik 157,5, szybki atak 151,5, blok 138,1, mocny atak 67,9, parowanie 45,9, flaszka (próba) 37,8, przełamana garda 20,3, riposta 14,8.
Czas w bloku 83 s, w biegu 195 s, bez wytrzymałości (<10%) 258 s na podejście.
Zakupy: sklep: Ostrze ×37, dodatkowa flaszka ×35, ulepszenie: Rozpłatanie ×30, sklep: Skupienie ×30, sklep: Kat ×29, sklep: Szybkie ręce ×28, sklep: Twarda skóra ×27, ulepszenie: Cięcie wichru ×26.
Punkty cech: Toughness 152, Strength 283.
Pieczęcie na podejście: z daru bossa 6,8, z sakwy do skilli 3,1, kupione 1,1; najczęstsze: Oko Czarnoboga ×14, Nieskończoność ×13, Wieczny płomień ×12, Pakt z diabłem ×11, Korona Welesa ×10, Klątwa Mokoszy ×10.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Kosa, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×1745; „SZAŁ!” ×734; „PAROWANIE!” ×602; „Postawa przełamana – RIPOSTA!” ×477; „GARDA PRZEŁAMANA!” ×422; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×353; „Za mało zdrowia” ×327; „Portale gasną – dobij pozostałe demony” ×305.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×1341, uśpiony wróg blokuje koniec piętra ×70, garda nie zatrzymała ciosu ×33, zablokowany ruch ×31, parowanie w oknie nie zadziałało ×13.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 12 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,0) (w 2 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,8; 0,1) (w 2 podejściach)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; 2,0) (w 2 podejściach)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×1413, uśpiony wróg blokuje koniec piętra ×210, zablokowany ruch ×91, garda nie zatrzymała ciosu ×25, parowanie w oknie nie zadziałało ×22.
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 12 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 12 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** zablokowany ruch średnio 1,6× na podejście (geometria areny/kolizje). Przykłady: piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,0); piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,8; 0,1); piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; 2,0).
- **Do sprawdzenia (P3):** 13× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Kultysta topieli: „Kultysta topieli” trafił w aktywnym oknie parowania (kąt do źródła 106°)
- **Do sprawdzenia (P3):** 33× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 166°)
- Parowanie zwykłych ciosów: 66% ciosów mimo to rani (n=271) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 48% ciosów mimo to rani (n=87) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 40% ciosów (n=201).
- Blok zwykłych ciosów: 59% ciosów mimo to rani (n=584) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 93% ciosów mimo to rani (n=1232) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 28% pocisków, przed którymi był unik (n=953).
- Blok pocisku: 75% ciosów mimo to rani (n=720) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 54% decyzji zapada, gdy do trafienia zostało <0,1 s (n=3673) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 5% decyzji zapada mniej niż 0,1 s przed trafieniem (n=683).
- Telegraf „nieparowalny” daje czas: tylko 8% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1316).
- Telegraf „nieblokowalny” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=84).
- Telegraf „obszarowy” daje czas: tylko 4% decyzji zapada mniej niż 0,1 s przed trafieniem (n=2439).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Za mało zdrowia” ×114 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 158 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 156 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 175 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 184 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 175 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 171 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 160 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 11/20. Wyzwanie umiarkowane; po opanowaniu parowania wieża przestaje straszyć.
- Czuję progres: druga połowa podejść średnio piętro 27,3 wobec 19,3 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 46% udanych (n=1191). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Arcydiabeł (178,9 trafień, 56,9 s), Bezimienny Cień (151,8 trafień, 55,3 s), Kowal Piekieł (125,8 trafień, 36,5 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kamienny chochlik (1,0), Ożywiony nietoperz (1,0), Siarkowy pomiot (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Królowa Topieli – średnio 59 s i 46 trafień do zabicia.
- Długie starcie: Rzeźnik – średnio 50 s i 47 trafień do zabicia.
- Długie starcie: Tytan Otchłani – średnio 46 s i 64 trafień do zabicia.
- Długie starcie: Matka Burz – średnio 60 s i 99 trafień do zabicia.
- Długie starcie: Król Kości – średnio 49 s i 102 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 55 s i 152 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 57 s i 179 trafień do zabicia.
- 6/9 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 10% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 89% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 1829 obrażeń na użycie.
- Skill startowy to 49% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 7%. Różnych skilli na podejście: średnio 4,8.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (14×, śr. piętro 27,4). Inne: Miecz i tarcza + Rozpłatanie 3× (15,7), Dwuręczny + Cięcie wichru 2× (14,0), Dwuręczny + Rozpłatanie 1× (8,0).
- Unik to moje główne narzędzie: 157,5 na podejście, w tym 42,1 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 76% przypadków.
- Gra nie wykonała uniku od razu średnio 53,5× na podejście – najczęściej: w trakcie poprzedniego uniku 20,7, drgnięcie (trafiony) 14,4, brak wytrzymałości 7,7. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (183 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

