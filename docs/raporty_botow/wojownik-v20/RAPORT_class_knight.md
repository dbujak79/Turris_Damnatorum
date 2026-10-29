# Raport botów grających — Wojownik

Wygenerowano: 2026-10-29 08:15. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 11,6 | 10,0 | 23,2 min | 394,4 | 147307 / 8222 | 27,1 | 2% |
| Gracz H&S / souls-like | 20 | 2 (10%) | 15,0 | 12,0 | 32,1 min | 609,5 | 305681 / 13194 | 28,2 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 10 13 10 9 10 9 11 9 9 9 8 18 11 10 8 19 13 9 10 26
- Gracz H&S / souls-like: 10 9 10 10 8 27 16 10 8 15 8 14 14 14 28 20 10 ★ ★ 9

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×5, 11. XI. Rzeźnia ×2, 13. XIII. Kamienne Trzewia ×2, 18. XVIII. Matka Burz ×1, 19. XIX. Kostnica ×1, 26. XXVI. Kuźnia Piekieł ×1, 8. VIII. Zatopione Krużganki ×2, 9. IX. Królowa Topieli ×6.
  Ostatni cios: Królowa Topieli – „Królowa Topieli: Combo” ×3; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×3; Kolos – „Kolos: Combo” ×1; Grzebacz – „Grzebacz” ×1; Kolos – „Kolos” ×1; Królowa Topieli – „Królowa Topieli: Flood” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 13/20.
  Ostatnie 3 s przed śmiercią: średnio 33% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/20). Picie: 674 prób, 541 wypitych (20% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 10. X. Rzeźnia ×5, 14. XIV. Kamienne Trzewia ×3, 15. XV. Tytan Otchłani ×1, 16. XVI. Wieża Burz ×1, 20. XX. Kostnica ×1, 27. XXVII. Kowal Piekieł ×1, 28. XXVIII. Tron Arcydemona ×1, 8. VIII. Zatopione Krużganki ×3, 9. IX. Królowa Topieli ×2.
  Ostatni cios: Kultysta – „Kula ognia” ×2; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×2; Królowa Topieli – „Królowa Topieli: CycloneEye” ×1; Ogar otchłani – „Ogar otchłani” ×1; Czerw – „Ugryzienie” ×1; Legionista – „Legionista: ShieldBash” ×1. Śmierć ciosem ≥40% maks. HP: 0/18. Zginął mając flaszki: 16/18.
  Ostatnie 3 s przed śmiercią: średnio 34% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/18). Picie: 680 prób, 563 wypitych (17% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 103 s | 12 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 113 s | 45 | 0,0 | 41,6 |
| Tester QA | 3. III. Władca Bramy | 20 | 20 | 28 s | 224 | 0,5 | 7,6 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 112 s | 37 | 0,4 | 45,7 |
| Tester QA | 5. V. Psiarnia Piekieł | 20 | 20 | 92 s | 78 | 0,5 | 39,6 |
| Tester QA | 6. VI. Pan Sfory | 23 | 20 | 36 s | 379 | 1,7 | 5,3 |
| Tester QA | 7. VII. Zatopione Krużganki | 22 | 20 | 106 s | 306 | 1,6 | 40,5 |
| Tester QA | 8. VIII. Zatopione Krużganki | 34 | 18 | 137 s | 652 | 2,4 | 41,6 |
| Tester QA | 9. IX. Królowa Topieli | 39 | 12 | 70 s | 926 | 2,9 | 1,1 |
| Tester QA | 10. X. Rzeźnia | 26 | 7 | 111 s | 719 | 2,1 | 38,3 |
| Tester QA | 11. XI. Rzeźnia | 14 | 5 | 124 s | 808 | 2,6 | 43,0 |
| Tester QA | 12. XII. Rzeźnik | 5 | 5 | 81 s | 395 | 1,4 | 7,0 |
| Tester QA | 13. XIII. Kamienne Trzewia | 9 | 3 | 126 s | 1111 | 3,8 | 47,6 |
| Tester QA | 14. XIV. Kamienne Trzewia | 4 | 3 | 133 s | 1039 | 3,8 | 62,0 |
| Tester QA | 15. XV. Tytan Otchłani | 5 | 3 | 72 s | 1155 | 3,6 | 2,6 |
| Tester QA | 16. XVI. Wieża Burz | 4 | 3 | 153 s | 584 | 2,5 | 73,5 |
| Tester QA | 17. XVII. Wieża Burz | 3 | 3 | 172 s | 378 | 2,7 | 86,7 |
| Tester QA | 18. XVIII. Matka Burz | 5 | 2 | 91 s | 1914 | 5,0 | 1,4 |
| Tester QA | 19. XIX. Kostnica | 4 | 1 | 167 s | 1114 | 2,0 | 57,8 |
| Tester QA | 20. XX. Kostnica | 1 | 1 | 172 s | 923 | 2,0 | 84,0 |
| Tester QA | 21. XXI. Król Kości | 1 | 1 | 109 s | 1107 | 3,0 | 11,0 |
| Tester QA | 22. XXII. Chór Cieni | 1 | 1 | 165 s | 1133 | 3,0 | 92,0 |
| Tester QA | 23. XXIII. Chór Cieni | 2 | 1 | 185 s | 1162 | 2,5 | 87,0 |
| Tester QA | 24. XXIV. Bezimienny Cień | 1 | 1 | 123 s | 811 | 2,0 | 4,0 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 1 | 1 | 195 s | 861 | 3,0 | 110,0 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 3 | 0 | 0 s | 2075 | 4,3 | 104,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 102 s | 11 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 20 | 113 s | 52 | 0,2 | 41,1 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 27 s | 153 | 0,4 | 7,5 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 20 | 20 | 110 s | 83 | 0,3 | 44,4 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 20 | 20 | 100 s | 43 | 0,1 | 41,6 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 20 | 20 | 38 s | 344 | 1,5 | 6,7 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 23 | 20 | 105 s | 332 | 0,8 | 37,1 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 32 | 17 | 129 s | 711 | 2,2 | 40,1 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 24 | 15 | 73 s | 923 | 3,0 | 1,9 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 29 | 10 | 116 s | 872 | 2,1 | 41,6 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 13 | 10 | 132 s | 779 | 2,2 | 57,5 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 10 | 10 | 94 s | 158 | 0,1 | 6,4 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 16 | 10 | 137 s | 977 | 1,8 | 56,4 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 18 | 7 | 137 s | 1090 | 1,9 | 46,6 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 11 | 6 | 73 s | 1277 | 2,5 | 2,0 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 8 | 5 | 150 s | 1190 | 1,9 | 63,1 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 7 | 5 | 156 s | 1226 | 2,1 | 76,3 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 6 | 5 | 95 s | 1274 | 3,0 | 2,8 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 6 | 5 | 166 s | 1292 | 1,8 | 89,0 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 8 | 4 | 167 s | 1386 | 1,8 | 66,6 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 4 | 4 | 78 s | 768 | 1,5 | 15,0 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 5 | 4 | 190 s | 1048 | 1,8 | 100,4 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 6 | 4 | 178 s | 1514 | 2,3 | 99,8 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 4 | 4 | 87 s | 293 | 0,8 | 4,0 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 6 | 4 | 178 s | 1296 | 1,5 | 98,2 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 7 | 4 | 196 s | 1215 | 1,3 | 95,7 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 7 | 3 | 75 s | 2161 | 4,6 | 1,1 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 6 | 2 | 203 s | 1480 | 1,3 | 94,7 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 3 | 2 | 194 s | 1800 | 2,7 | 103,3 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 2 | 2 | 79 s | 1236 | 1,5 | 9,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 6 ×3, piętro 7 ×2, piętro 8 ×14, piętro 9 ×21, piętro 10 ×14, piętro 11 ×7, piętro 13 ×4, piętro 14 ×1, piętro 15 ×2, piętro 16 ×1, piętro 18 ×2, piętro 19 ×2, piętro 23 ×1, piętro 26 ×2; wyzwań (3 porażki na piętrze): 5 – piętro 10, piętro 13, piętro 18, piętro 19, piętro 26.
**Gracz H&S / souls-like** – ponowne próby: piętro 7 ×3, piętro 8 ×12, piętro 9 ×7, piętro 10 ×14, piętro 11 ×3, piętro 13 ×6, piętro 14 ×8, piętro 15 ×4, piętro 16 ×2, piętro 17 ×2, piętro 18 ×1, piętro 19 ×1, piętro 20 ×3, piętro 22 ×1, piętro 23 ×2, piętro 25 ×2, piętro 26 ×3, piętro 27 ×3, piętro 28 ×3, piętro 29 ×1; wyzwań (3 porażki na piętrze): 5 – piętro 10, piętro 27, piętro 28.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Królowa Topieli | 27 | 67,3 s | 51,9 | 2382 | 54842 | 2031 | 7 |
| Wiedźma wichru | 800 | 2,4 s | 1,8 | 4151 | 43064 | 52 | 5 |
| Miotacz głazów | 381 | 1,7 s | 1,8 | 1136 | 39501 | 101 | 2 |
| Kultysta topieli | 1079 | 0,8 s | 1,3 | 2053 | 38885 | 35 | 3 |
| Harpia | 768 | 0,7 s | 1,7 | 1634 | 20979 | 26 | 2 |
| Kultysta | 1968 | 0,3 s | 1,1 | 1274 | 20859 | 10 | 3 |
| Tytan Otchłani | 9 | 69,0 s | 65,8 | 404 | 19730 | 2192 | 1 |
| Matka Burz | 7 | 90,0 s | 96,6 | 610 | 16529 | 2361 | 1 |
| Kowal Piekieł | 3 | 71,4 s | 136,7 | 369 | 15101 | 5034 | 1 |
| Kolos | 962 | 2,3 s | 2,6 | 419 | 15049 | 15 | 2 |
| Pan Sfory | 40 | 32,8 s | 22,1 | 807 | 14534 | 363 | 0 |
| Grzebacz | 296 | 1,2 s | 2,2 | 403 | 14038 | 46 | 1 |
| Ogar otchłani | 1955 | 0,4 s | 1,4 | 1339 | 12298 | 6 | 1 |
| Kolos żaru | 346 | 2,8 s | 3,3 | 363 | 11699 | 33 | 0 |
| Upiór | 254 | 3,0 s | 2,6 | 1079 | 8878 | 34 | 2 |
| Taranowiec | 1047 | 1,4 s | 2,0 | 595 | 7746 | 7 | 0 |
| Władca Bramy | 40 | 23,4 s | 14,7 | 315 | 6962 | 174 | 0 |
| Legionista | 878 | 3,6 s | 3,8 | 824 | 6201 | 7 | 1 |
| Gnilec | 460 | 3,2 s | 3,4 | 468 | 6012 | 13 | 1 |
| Czerw | 4805 | 0,2 s | 1,2 | 1691 | 6001 | 1 | 1 |
| Gnilec (odłamek) | 906 | 1,0 s | 1,9 | 456 | 5928 | 7 | 1 |
| Siarkowy pomiot | 1202 | 0,1 s | 1,1 | 161 | 5912 | 5 | 0 |
| Cień | 435 | 0,7 s | 1,8 | 356 | 4917 | 11 | 0 |
| Król Kości | 5 | 80,8 s | 113,0 | 246 | 4154 | 831 | 0 |
| Tkaczka | 209 | 2,5 s | 2,3 | 359 | 3572 | 17 | 0 |
| Ofiarnik | 328 | 1,1 s | 1,4 | 491 | 3536 | 10 | 0 |
| Rzeźnik | 15 | 86,0 s | 51,1 | 330 | 3329 | 222 | 0 |
| Arcydiabeł | 2 | 72,8 s | 209,5 | 88 | 2473 | 1236 | 0 |
| Gargulec | 252 | 2,1 s | 2,8 | 179 | 2425 | 9 | 0 |
| Ognisty chochlik | 895 | 0,2 s | 1,2 | 197 | 2142 | 2 | 0 |
| Bezimienny Cień | 5 | 90,5 s | 136,4 | 203 | 1964 | 393 | 0 |
| Ożywiony nietoperz | 602 | 0,1 s | 1,1 | 365 | 1810 | 3 | 0 |
| Nekromanta | 147 | 2,7 s | 2,4 | 362 | 1728 | 12 | 0 |
| Chochlik wichru | 785 | 0,5 s | 1,5 | 145 | 1661 | 2 | 0 |
| Kamienny chochlik | 832 | 0,1 s | 1,1 | 142 | 1564 | 2 | 0 |
| Wodny chochlik | 813 | 0,1 s | 1,2 | 131 | 1230 | 2 | 1 |
| Pajączek | 154 | 0,1 s | 1,0 | 140 | 1074 | 7 | 2 |
| Czerw (elita) | 46 | 1,4 s | 2,1 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 55 | 1,7 s | 2,8 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 27 | 6,4 s | 6,4 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 33 | 3,3 s | 4,0 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 47 | 2,8 s | 2,7 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 29 | 8,8 s | 4,5 | ↑ | ↑ | ↑ |  |
| Worek jaj | 108 | 0,5 s | 1,3 | 0 | 0 | 0 | 0 |
| Harpia (elita) | 25 | 3,2 s | 4,0 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 20 | 12,2 s | 12,5 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 9 | 6,0 s | 6,4 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 23 | 7,7 s | 3,6 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 33 | 0,7 s | 1,7 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 26 | 10,3 s | 9,5 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 14 | 10,4 s | 12,6 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 10 | 5,2 s | 4,6 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 7 | 4,2 s | 8,6 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 1 | 4,7 s | 6,0 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 4 | 7,4 s | 13,5 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 11 | 12,9 s | 10,9 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 6 | 3,4 s | 4,2 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 14 | 4,0 s | 6,4 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 7 | 8,0 s | 9,4 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3140 | 69% | 9% | 2% | 16% | 4% |
| ciężki | 431 | 55% | 7% | 6% | 30% | 2% |
| nieparowalny | 2515 | 64% | 12% | 0% | 18% | 6% |
| nieblokowalny | 204 | 76% | 0% | 0% | 24% | 0% |
| obszarowy | 3607 | 83% | 13% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 6111 | 56% | 11% | 8% | 21% | 4% |
| ciężki | 768 | 40% | 8% | 15% | 36% | 0% |
| nieparowalny | 3877 | 54% | 19% | 0% | 23% | 5% |
| nieblokowalny | 283 | 73% | 0% | 0% | 27% | 0% |
| obszarowy | 5731 | 76% | 20% | 0% | 0% | 4% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2502 | 24% | 33% | 4% | 19% | 20% |
| Tester QA (0,30 s) | ciężki | 526 | 2% | 2% | 6% | 22% | 68% |
| Tester QA (0,30 s) | nieparowalny | 1035 | 4% | 4% | 10% | 31% | 51% |
| Tester QA (0,30 s) | nieblokowalny | 70 | 3% | 1% | 3% | 19% | 74% |
| Tester QA (0,30 s) | obszarowy | 1798 | 2% | 1% | 3% | 23% | 71% |
| Tester QA (0,30 s) | pocisk | 2321 | 5% | 18% | 12% | 20% | 45% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 5267 | 5% | 23% | 31% | 11% | 30% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 992 | 3% | 2% | 3% | 22% | 70% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1739 | 4% | 2% | 4% | 17% | 74% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 84 | 0% | 0% | 0% | 7% | 93% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 3299 | 2% | 1% | 3% | 15% | 78% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 3677 | 2% | 18% | 12% | 16% | 51% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 930 |
| Pajączek: Pajączek | 0,31 s | 221 |
| Ognisty chochlik: Pazury | 0,33 s | 385 |
| Wodny chochlik: Pazury | 0,33 s | 251 |
| Chochlik wichru: Pazury | 0,33 s | 260 |
| Czerw: Ugryzienie | 0,33 s | 2651 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 529 |
| Kamienny chochlik: Pazury | 0,33 s | 233 |
| Władca Bramy: Cięcie w szale | 0,41 s | 21 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 1728 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 48% (n=237) | 29% (n=7) | 53% (n=333) | 100% (n=17) | 53% (n=523) | 80% (n=487) |
| blok (eksperyment) | 82% (n=105) | 89% (n=28) | 83% (n=120) | 100% (n=6) | 75% (n=168) | 70% (n=98) |
| brak reakcji (eksperyment) | 83% (n=107) | 74% (n=34) | 92% (n=71) | 83% (n=6) | 92% (n=154) | 82% (n=90) |
| niezauważony | 85% (n=514) | – | 90% (n=255) | 98% (n=53) | 95% (n=1115) | – |
| parowanie | 63% (n=72) | 35% (n=20) | – | – | 100% (n=1) | – |
| parowanie (eksperyment) | 59% (n=59) | 36% (n=22) | 100% (n=37) | 100% (n=2) | 91% (n=80) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 89% (n=113) |
| unik | 56% (n=622) | 36% (n=174) | 40% (n=562) | 47% (n=85) | 100% (n=14) | 68% (n=253) |
| unik (eksperyment) | 68% (n=188) | 54% (n=41) | 54% (n=136) | 50% (n=2) | 97% (n=216) | 80% (n=66) |
| wyjście z zasięgu | 92% (n=520) | 88% (n=88) | 92% (n=389) | 97% (n=33) | 94% (n=1176) | – |
| wyjście z zasięgu (eksperyment) | 81% (n=111) | 82% (n=17) | 92% (n=96) | – | 91% (n=108) | 71% (n=66) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 86,4 | reakcja na cios wręcz 87,1, unik przed pociskiem 38,0, wyjście z okrążenia 6,0 | drgnięcie (trafiony) 13,8, brak wytrzymałości 11,5, w trakcie uniku 7,5, picie flaszki 5,7, inna 0,9, parowanie 0,7, umiejętność 0,2, szybki atak 0,2, unieruchomienie 0,2, mocny atak 0,1, przełamana garda 0,1, riposta 0,1, blok 0,1 | 35% (n=1741) / 70% (n=759) |
| Gracz H&S / souls-like | 171,2 | reakcja na cios wręcz 146,5, unik przed pociskiem 72,3, doskok do odsłoniętego celu 17,3, wyjście z okrążenia 13,8 | w trakcie uniku 20,9, drgnięcie (trafiony) 18,2, brak wytrzymałości 11,9, picie flaszki 6,3, parowanie 5,8, unieruchomienie 0,8, inna 0,8, blok 0,8, mocny atak 0,4, riposta 0,3, umiejętność 0,2, szybki atak 0,2 | 53% (n=2929) / 75% (n=1445) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 16,2 | 665,6 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 10,6 | 328,6 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 10,0 | 293,6 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 9,4 | 289,6 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 11 | 1 | 17,8 | 762,3 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 3 | 1 | 16,0 | 726,7 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 4 | 0 | 9,5 | 279,5 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 2 | 0 | 9,0 | 253,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 1129733 | 38% | 2367 | 477 |
| Zamach | 479168 | 16% |  |  |
| Rozpłatanie | 413947 | 14% | 1510 | 274 |
| Tąpnięcie | 342326 | 12% | 1650 | 207 |
| Egzekucja | 212275 | 7% |  |  |
| Cięcie | 156442 | 5% |  |  |
| Okrzyk bojowy | 110103 | 4% | 1486 | 74 |
| Potężne cięcie | 82902 | 3% |  |  |
| Riposta | 9788 | 0% |  |  |
| Szarża barkiem | 9456 | 0% |  |  |

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 2863751 | 47% | 3992 | 717 |
| Rozpłatanie | 1330548 | 22% | 3693 | 360 |
| Cięcie | 423257 | 7% |  |  |
| Szarża | 379220 | 6% | 1972 | 192 |
| Egzekucja | 240415 | 4% | 363 | 662 |
| Okrzyk bojowy | 218965 | 4% | 898 | 244 |
| Potężne cięcie | 208352 | 3% |  |  |
| Riposta | 202214 | 3% |  |  |
| Zamach | 183534 | 3% |  |  |
| Szarża barkiem | 40975 | 1% |  |  |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 3,0, mediana 3,0, najwięcej 3 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 55% | 193,9 | 398 | 0,4 |
| odblokowanie | 24% | 82,5 | 207 | 0,0 |
| klasa | 21% | 74,3 | 74 | 0,0 |

Oferty sklepu dusz: widziane 633, kupione 0, odrzucone 0, za mało dusz 211.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 10 | 236,7 | 477 | 0,5 |
| Tąpnięcie | odblokowanie | 20 | 82,5 | 207 | 0,0 |
| Rozpłatanie | startowy | 10 | 151,0 | 274 | 0,2 |
| Okrzyk bojowy | klasa | 20 | 74,3 | 74 | 0,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,1, mediana 4,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 46% | 252,7 | 622 | 0,3 |
| odblokowanie | 46% | 254,4 | 327 | 0,1 |
| klasa | 8% | 44,9 | 244 | 0,0 |

Oferty sklepu dusz: widziane 840, kupione 0, odrzucone 0, za mało dusz 280.
Zamiany w slotach: 1,0 na podejście; najczęstsze: Szarża → Egzekucja ×8, Rozpłatanie → Uderzenie tarczą ×1, Szarża → Kontra ×1, Cięcie wichru → Krwawe cięcie ×1, Krwawe cięcie → Rozpłatanie ×1, Uderzenie tarczą → Cięcie wichru ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 14 | 285,1 | 717 | 0,3 |
| Rozpłatanie | startowy | 20 | 184,7 | 360 | 0,2 |
| Szarża | odblokowanie | 11 | 179,3 | 192 | 0,1 |
| Okrzyk bojowy | klasa | 20 | 44,9 | 244 | 0,0 |
| Egzekucja | odblokowanie | 9 | 40,3 | 582 | 0,0 |
| Tąpnięcie | odblokowanie | 1 | 68,0 | 216 | 0,0 |
| Seria cięć | odblokowanie | 1 | 16,0 | 274 | 0,0 |
| Kontra | odblokowanie | 1 | 13,0 | 31 | 0,0 |
| Rozdarcie ran | odblokowanie | 1 | 7,0 | 96 | 0,0 |
| Uderzenie tarczą | odblokowanie | 1 | 6,0 | 67 | 0,0 |
| Krwawe cięcie | odblokowanie | 1 | 6,0 | 147 | 0,0 |
| Młynek | odblokowanie | 1 | 6,0 | 157 | 0,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 350,7, szybki atak 158,7, drgnięcie (trafiony) 136,1, unik 86,4, blok 70,2, mocny atak 56,5, flaszka (próba) 33,7, przełamana garda 20,6, parowanie 7,1, riposta 1,5.
Czas w bloku 35 s, w biegu 94 s, bez wytrzymałości (<10%) 350 s na podejście.
Zakupy: dodatkowa flaszka ×16, cecha za dusze: Toughness (52) ×11, sklep: Miecz jednoręczny: poziom 3 ×10, sklep: Miecz jednoręczny: poziom 4 ×10, cecha za dusze: Strength (48) ×10, sklep: Miecz dwuręczny: poziom 3 ×10, założony przedmiot: Trzewiki płytowe ×9, cecha za dusze: Strength (45) ×8.
Punkty cech: Strength 34, Toughness 29, Dexterity 26, Intelligence 23.
Pieczęcie na podejście: z daru bossa 3,2, z sakwy do skilli 2,0, kupione 0,1; najczęstsze: Pakt z diabłem ×8, Deszcz piekieł ×7, Bliźniacza dusza ×7, Korona Welesa ×6, Klątwa Mokoszy ×5, Serce otchłani ×5.
Komunikaty gry: „OKRZYK!” ×1448; „SZAŁ!” ×708; „GARDA PRZEŁAMANA!” ×439; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×208; „Garda wroga przełamana!” ×196; „ZAMROŻENIE!” ×181; „Portale gasną – dobij pozostałe demony” ×160; „Za mało zdrowia” ×112.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 552,0, drgnięcie (trafiony) 195,7, unik 171,2, szybki atak 148,4, blok 130,1, mocny atak 60,1, parowanie 39,4, flaszka (próba) 34,0, przełamana garda 31,0, riposta 14,3.
Czas w bloku 69 s, w biegu 123 s, bez wytrzymałości (<10%) 361 s na podejście.
Zakupy: sklep: Furia ×23, dodatkowa flaszka ×22, sklep: Twarda skóra ×22, sklep: Szybkie ręce ×22, sklep: Ostrze ×20, sklep: Hart ciała ×18, sklep: Świszczące ostrze ×17, sklep: Płonące ostrze ×16.
Punkty cech: Toughness 52, Strength 79.
Pieczęcie na podejście: z daru bossa 4,4, z sakwy do skilli 1,9, kupione 0,0; najczęstsze: Deszcz piekieł ×10, Korona Welesa ×9, Oko Czarnoboga ×9, Wieczny płomień ×7, Pakt z diabłem ×6, Bliźniacza dusza ×6.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Krwawa riposta, Pawęż, Przetrwanie, Kosa, Wielki topór, Młot burzy, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „OKRZYK!” ×1591; „SZAŁ!” ×882; „GARDA PRZEŁAMANA!” ×648; „PAROWANIE!” ×579; „Postawa przełamana – RIPOSTA!” ×465; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×273; „RIPOSTA!” ×255; „Portale gasną – dobij pozostałe demony” ×206.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×1023, uśpiony wróg blokuje koniec piętra ×41, zablokowany ruch ×19, garda nie zatrzymała ciosu ×17, parowanie w oknie nie zadziałało ×1.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; 2,1) (w 3 podejściach)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (7,8; 0,0) (w 2 podejściach)
  - Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 136°) (w 2 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - Miotacz głazów: „Miotacz głazów: Rockfall” przeszedł przez gardę (kąt 177°)
  - piętro 12 „XII. Rzeźnik”: brak postępu przy (16,4; 4,7)
  - piętro 7 „VII. Zatopione Krużganki”: brak postępu przy (10,0; -1,6)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×1521, zablokowany ruch ×105, uśpiony wróg blokuje koniec piętra ×99, parowanie w oknie nie zadziałało ×32, garda nie zatrzymała ciosu ×26.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 177°) (w 2 podejściach)
  - Miotacz głazów: „Miotacz głazów” przeszedł przez gardę (kąt 164°) (w 2 podejściach)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; 0,2) (w 2 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** zablokowany ruch średnio 1,0× na podejście (geometria areny/kolizje). Przykłady: piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; 2,1); piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (7,8; 0,0); piętro 12 „XII. Rzeźnik”: brak postępu przy (16,4; 4,7).
- **Do sprawdzenia (P3):** 1× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Ofiarnik: „Ofiarnik: BloodRite” trafił w aktywnym oknie parowania (kąt do źródła 77°)
- **Do sprawdzenia (P3):** 17× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 136°)
- Parowanie zwykłych ciosów: 61% ciosów mimo to rani (n=131) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 47% ciosów mimo to rani (n=87) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 40% ciosów (n=215).
- Blok zwykłych ciosów: 58% ciosów mimo to rani (n=342) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 94% ciosów mimo to rani (n=1284) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 30% pocisków, przed którymi był unik (n=759).
- Blok pocisku: 78% ciosów mimo to rani (n=585) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 57% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2502) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 5% decyzji zapada mniej niż 0,1 s przed trafieniem (n=526).
- Telegraf „nieparowalny” daje czas: tylko 8% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1035).
- Telegraf „nieblokowalny” daje czas: tylko 4% decyzji zapada mniej niż 0,1 s przed trafieniem (n=70).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1798).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Za mało zdrowia” ×112 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 153 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 172 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 167 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 172 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 165 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 185 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 195 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 3453 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 2/20. Trudno, ale do zrobienia – tak ma być w souls-like.
- Czuję progres: druga połowa podejść średnio piętro 17,9 wobec 12,3 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 49% udanych (n=1064). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Królowa Topieli (56,2 trafień, 68,9 s), Rzeźnik (53,4 trafień, 90,6 s), Pan Sfory (22,0 trafień, 33,7 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,1), Siarkowy pomiot (1,1), Ożywiony nietoperz (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Królowa Topieli – średnio 69 s i 56 trafień do zabicia.
- Długie starcie: Rzeźnik – średnio 91 s i 53 trafień do zabicia.
- Długie starcie: Tytan Otchłani – średnio 69 s i 71 trafień do zabicia.
- Długie starcie: Matka Burz – średnio 91 s i 108 trafień do zabicia.
- Długie starcie: Król Kości – średnio 75 s i 113 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 83 s i 136 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 71 s i 137 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 73 s i 210 trafień do zabicia.
- 16/18 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 17% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 83% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Cięcie wichru”: 717 obrażeń na użycie.
- Skill startowy to 46% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 0%. Różnych skilli na podejście: średnio 4,1.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (11×, śr. piętro 17,9). Inne: Miecz i tarcza + Rozpłatanie 4× (9,5), Dwuręczny + Cięcie wichru 3× (16,3), Dwuręczny + Rozpłatanie 2× (9,0).
- Unik to moje główne narzędzie: 171,2 na podejście, w tym 31,1 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 75% przypadków.
- Gra nie wykonała uniku od razu średnio 66,3× na podejście – najczęściej: w trakcie poprzedniego uniku 20,9, drgnięcie (trafiony) 18,2, brak wytrzymałości 11,9. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (19% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (196 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

