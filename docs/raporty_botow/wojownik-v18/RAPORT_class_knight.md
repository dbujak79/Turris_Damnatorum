# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 19:37. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 8,2 | 8,0 | 16,1 min | 226,8 | 53316 / 5399 | 19,4 | 1% |
| Gracz H&S / souls-like | 20 | 0 (0%) | 13,0 | 9,0 | 27,6 min | 501,2 | 204895 / 9592 | 27,1 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 8 6 9 9 8 10 7 9 9 ⏱8 5 9 6 6 9 9 8 8 7 14
- Gracz H&S / souls-like: 8 27 5 7 17 9 10 9 30 8 30 15 9 9 11 9 15 9 7 15

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×1, 14. XIV. Kamienne Trzewia ×1, 5. V. Psiarnia Piekieł ×1, 6. VI. Pan Sfory ×3, 7. VII. Zatopione Krużganki ×2, 8. VIII. Zatopione Krużganki ×4, 9. IX. Królowa Topieli ×7.
  Ostatni cios: Królowa Topieli – „Królowa Topieli: CycloneEye” ×3; Pan Sfory – „Pan Sfory: InfernalCharge” ×2; Królowa Topieli – „Królowa Topieli: Combo” ×2; Kultysta topieli – „Kultysta topieli” ×2; Kolos – „Kolos: Combo” ×1; Taranowiec – „Taranowiec” ×1. Śmierć ciosem ≥40% maks. HP: 0/19. Zginął mając flaszki: 9/19.
  Ostatnie 3 s przed śmiercią: średnio 26% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/19). Picie: 464 prób, 388 wypitych (16% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 10. X. Rzeźnia ×1, 11. XI. Rzeźnia ×1, 15. XV. Tytan Otchłani ×3, 17. XVII. Wieża Burz ×1, 27. XXVII. Kowal Piekieł ×1, 30. XXX. Arcydiabeł ×2, 5. V. Psiarnia Piekieł ×1, 7. VII. Zatopione Krużganki ×2, 8. VIII. Zatopione Krużganki ×2, 9. IX. Królowa Topieli ×6.
  Ostatni cios: Królowa Topieli – „Królowa Topieli: Flood” ×4; Kultysta – „Kula ognia” ×2; Harpia – „Harpia” ×2; Czerw – „Ugryzienie” ×1; Kowal Piekieł – „Kowal Piekieł: InfernalOrb” ×1; Kolos żaru – „Kolos żaru: Quake” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 9/20.
  Ostatnie 3 s przed śmiercią: średnio 32% maks. HP straconych (śmierć z ≥50% HP w 3 s: 5/20). Picie: 623 prób, 541 wypitych (13% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 102 s | 9 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 113 s | 87 | 0,1 | 41,0 |
| Tester QA | 3. III. Władca Bramy | 22 | 20 | 43 s | 455 | 1,6 | 7,9 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 108 s | 91 | 0,5 | 45,5 |
| Tester QA | 5. V. Psiarnia Piekieł | 22 | 19 | 103 s | 249 | 0,9 | 39,0 |
| Tester QA | 6. VI. Pan Sfory | 30 | 16 | 40 s | 648 | 2,9 | 4,8 |
| Tester QA | 7. VII. Zatopione Krużganki | 24 | 14 | 108 s | 662 | 2,7 | 36,0 |
| Tester QA | 8. VIII. Zatopione Krużganki | 27 | 9 | 139 s | 763 | 2,6 | 35,7 |
| Tester QA | 9. IX. Królowa Topieli | 23 | 2 | 72 s | 1154 | 3,7 | 0,2 |
| Tester QA | 10. X. Rzeźnia | 4 | 1 | 102 s | 393 | 0,3 | 15,0 |
| Tester QA | 11. XI. Rzeźnia | 2 | 1 | 108 s | 733 | 2,5 | 45,0 |
| Tester QA | 12. XII. Rzeźnik | 1 | 1 | 58 s | 163 | 0,0 | 1,0 |
| Tester QA | 13. XIII. Kamienne Trzewia | 1 | 1 | 113 s | 881 | 4,0 | 63,0 |
| Tester QA | 14. XIV. Kamienne Trzewia | 3 | 0 | 0 s | 729 | 2,0 | 44,3 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 100 s | 4 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 21 | 20 | 111 s | 81 | 0,1 | 41,3 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 22 | 20 | 39 s | 260 | 0,7 | 8,4 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 20 | 20 | 111 s | 131 | 0,6 | 45,9 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 22 | 19 | 103 s | 105 | 0,2 | 38,4 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 20 | 19 | 44 s | 377 | 1,8 | 5,4 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 24 | 17 | 104 s | 456 | 1,8 | 32,8 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 26 | 15 | 127 s | 628 | 2,4 | 48,3 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 29 | 9 | 101 s | 1127 | 3,8 | 1,4 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 12 | 8 | 113 s | 664 | 2,0 | 48,1 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 11 | 7 | 129 s | 603 | 1,3 | 47,8 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 7 | 7 | 90 s | 213 | 1,1 | 6,1 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 7 | 7 | 129 s | 538 | 1,9 | 65,7 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 9 | 7 | 134 s | 848 | 2,9 | 65,3 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 15 | 4 | 69 s | 975 | 3,7 | 1,1 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 4 | 4 | 160 s | 534 | 2,5 | 80,0 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 7 | 3 | 163 s | 916 | 1,7 | 66,6 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 3 | 3 | 89 s | 636 | 2,3 | 3,0 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 4 | 3 | 161 s | 735 | 1,5 | 71,8 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 3 | 3 | 176 s | 1492 | 1,0 | 100,3 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 3 | 3 | 93 s | 469 | 2,3 | 13,0 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 4 | 3 | 176 s | 946 | 0,8 | 73,0 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 3 | 3 | 180 s | 1237 | 0,3 | 108,7 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 3 | 3 | 122 s | 500 | 2,7 | 4,0 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 5 | 3 | 182 s | 1627 | 1,2 | 87,0 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 5 | 3 | 191 s | 1319 | 0,4 | 84,2 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 6 | 2 | 97 s | 948 | 2,8 | 0,8 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 3 | 2 | 213 s | 1553 | 1,0 | 87,3 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 3 | 2 | 205 s | 1945 | 1,3 | 99,7 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 6 | 0 | 0 s | 1771 | 4,5 | 2,7 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 3 ×2, piętro 5 ×2, piętro 6 ×11, piętro 7 ×8, piętro 8 ×13, piętro 9 ×14, piętro 10 ×2, piętro 11 ×1, piętro 14 ×2; wyzwań (3 porażki na piętrze): 5 – piętro 8, piętro 9, piętro 10, piętro 14.
**Gracz H&S / souls-like** – ponowne próby: piętro 2 ×1, piętro 3 ×2, piętro 5 ×2, piętro 6 ×1, piętro 7 ×5, piętro 8 ×9, piętro 9 ×14, piętro 10 ×3, piętro 11 ×3, piętro 14 ×2, piętro 15 ×8, piętro 17 ×3, piętro 19 ×1, piętro 22 ×1, piętro 25 ×2, piętro 26 ×2, piętro 27 ×3, piętro 28 ×1, piętro 29 ×1, piętro 30 ×4; wyzwań (3 porażki na piętrze): 4 – piętro 8, piętro 27, piętro 30.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Królowa Topieli | 11 | 91,9 s | 227,4 | 2545 | 58101 | 5282 | 13 |
| Pan Sfory | 35 | 38,1 s | 49,0 | 1173 | 25580 | 731 | 3 |
| Kultysta topieli | 603 | 1,6 s | 3,4 | 975 | 22266 | 35 | 2 |
| Wiedźma wichru | 523 | 2,2 s | 4,7 | 1550 | 19187 | 36 | 1 |
| Kultysta | 1512 | 0,5 s | 1,5 | 1003 | 17827 | 11 | 2 |
| Miotacz głazów | 184 | 2,0 s | 6,3 | 328 | 16103 | 85 | 2 |
| Władca Bramy | 40 | 37,1 s | 23,4 | 618 | 14714 | 368 | 0 |
| Tytan Otchłani | 4 | 64,9 s | 430,8 | 282 | 14537 | 3634 | 3 |
| Harpia | 476 | 1,2 s | 4,0 | 958 | 12565 | 25 | 2 |
| Ogar otchłani | 1412 | 0,9 s | 2,8 | 1074 | 12290 | 8 | 2 |
| Kolos | 532 | 3,7 s | 6,9 | 234 | 10797 | 20 | 1 |
| Arcydiabeł | 0 | – | – | 241 | 10623 | – | 2 |
| Grzebacz | 143 | 2,1 s | 6,6 | 140 | 7205 | 48 | 0 |
| Taranowiec | 708 | 2,2 s | 4,7 | 442 | 6466 | 9 | 1 |
| Kowal Piekieł | 2 | 93,7 s | 633,0 | 97 | 5685 | 2843 | 1 |
| Czerw | 3706 | 0,5 s | 1,6 | 1187 | 4909 | 1 | 1 |
| Kolos żaru | 189 | 3,1 s | 10,5 | 97 | 4700 | 24 | 1 |
| Siarkowy pomiot | 889 | 0,3 s | 1,6 | 133 | 4466 | 5 | 0 |
| Legionista | 543 | 4,1 s | 8,2 | 540 | 4319 | 8 | 0 |
| Cień | 247 | 1,5 s | 5,8 | 111 | 2501 | 10 | 0 |
| Upiór | 136 | 3,5 s | 9,1 | 242 | 2327 | 17 | 0 |
| Gnilec | 196 | 4,5 s | 12,4 | 131 | 2322 | 11 | 0 |
| Gnilec (odłamek) | 399 | 1,9 s | 5,4 | 125 | 2050 | 5 | 1 |
| Ognisty chochlik | 762 | 0,3 s | 1,5 | 158 | 1854 | 2 | 1 |
| Matka Burz | 3 | 85,8 s | 411,7 | 70 | 1800 | 600 | 0 |
| Tkaczka | 104 | 3,1 s | 7,7 | 109 | 1732 | 16 | 0 |
| Rzeźnik | 8 | 82,4 s | 216,8 | 124 | 1655 | 207 | 0 |
| Bezimienny Cień | 3 | 118,3 s | 561,3 | 99 | 1485 | 495 | 0 |
| Król Kości | 3 | 89,5 s | 460,0 | 67 | 1408 | 469 | 0 |
| Kamienny chochlik | 652 | 0,3 s | 1,4 | 114 | 1390 | 2 | 0 |
| Wodny chochlik | 643 | 0,3 s | 1,4 | 123 | 1358 | 2 | 0 |
| Chochlik wichru | 701 | 0,8 s | 1,9 | 120 | 1279 | 2 | 0 |
| Ożywiony nietoperz | 482 | 0,5 s | 1,6 | 224 | 1169 | 2 | 0 |
| Ofiarnik | 153 | 2,3 s | 4,2 | 161 | 1148 | 7 | 0 |
| Gargulec | 140 | 3,0 s | 8,8 | 45 | 1080 | 8 | 0 |
| Nekromanta | 77 | 4,1 s | 8,0 | 89 | 670 | 8 | 0 |
| Pajączek | 53 | 0,5 s | 2,6 | 35 | 237 | 4 | 0 |
| Ognisty chochlik (elita) | 87 | 3,5 s | 5,6 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 55 | 7,3 s | 8,5 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 30 | 11,7 s | 13,9 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 20 | 6,7 s | 25,3 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 38 | 3,4 s | 4,0 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 37 | 1,7 s | 5,2 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 22 | 12,3 s | 23,7 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 48 | 8,3 s | 13,4 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 17 | 9,3 s | 15,7 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 14 | 21,1 s | 39,1 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 11 | 24,8 s | 34,6 | ↑ | ↑ | ↑ |  |
| Worek jaj | 43 | 1,3 s | 3,2 | 0 | 0 | 0 | 0 |
| Miotacz głazów (elita) | 5 | 11,7 s | 17,8 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 4 | 17,2 s | 32,8 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 3 | 16,6 s | 41,7 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 4 | 12,7 s | 48,5 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 7 | 26,9 s | 66,1 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 6 | 12,0 s | 45,2 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 7 | 16,3 s | 36,3 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 4 | 13,2 s | 32,5 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 5 | 21,3 s | 27,2 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 4 | 10,4 s | 27,8 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2389 | 75% | 3% | 2% | 18% | 2% |
| ciężki | 339 | 58% | 4% | 3% | 33% | 2% |
| nieparowalny | 1645 | 73% | 6% | 0% | 17% | 4% |
| nieblokowalny | 184 | 84% | 0% | 0% | 16% | 0% |
| obszarowy | 1289 | 88% | 8% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3364 | 57% | 5% | 12% | 25% | 1% |
| ciężki | 576 | 34% | 3% | 18% | 45% | 0% |
| nieparowalny | 2626 | 53% | 13% | 0% | 31% | 3% |
| nieblokowalny | 292 | 75% | 0% | 0% | 25% | 0% |
| obszarowy | 3060 | 84% | 14% | 0% | 0% | 2% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2155 | 23% | 40% | 3% | 18% | 15% |
| Tester QA (0,30 s) | ciężki | 365 | 1% | 1% | 3% | 14% | 80% |
| Tester QA (0,30 s) | nieparowalny | 633 | 4% | 7% | 13% | 34% | 42% |
| Tester QA (0,30 s) | nieblokowalny | 67 | 1% | 0% | 4% | 30% | 64% |
| Tester QA (0,30 s) | obszarowy | 889 | 2% | 2% | 2% | 27% | 67% |
| Tester QA (0,30 s) | pocisk | 1449 | 3% | 19% | 15% | 20% | 43% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3253 | 5% | 25% | 28% | 11% | 31% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 689 | 2% | 1% | 3% | 14% | 80% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1141 | 5% | 1% | 3% | 22% | 69% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 72 | 3% | 0% | 1% | 10% | 86% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 1999 | 2% | 1% | 3% | 22% | 72% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2300 | 3% | 20% | 15% | 17% | 44% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 835 |
| Pajączek: Pajączek | 0,31 s | 59 |
| Ognisty chochlik: Pazury | 0,33 s | 262 |
| Czerw: Ugryzienie | 0,33 s | 1967 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 364 |
| Kamienny chochlik: Pazury | 0,33 s | 221 |
| Wodny chochlik: Pazury | 0,33 s | 225 |
| Chochlik wichru: Pazury | 0,33 s | 231 |
| Władca Bramy: Cięcie w szale | 0,41 s | 47 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 716 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 70% (n=156) | 0% (n=7) | 50% (n=139) | 100% (n=7) | 44% (n=165) | 85% (n=247) |
| blok (eksperyment) | 80% (n=86) | 63% (n=19) | 95% (n=55) | 100% (n=2) | 84% (n=61) | 80% (n=55) |
| brak reakcji (eksperyment) | 90% (n=92) | 100% (n=19) | 98% (n=86) | 100% (n=7) | 96% (n=71) | 88% (n=50) |
| niezauważony | 88% (n=422) | – | 98% (n=216) | 100% (n=91) | 99% (n=379) | – |
| parowanie | 71% (n=69) | 54% (n=13) | – | – | – | – |
| parowanie (eksperyment) | 70% (n=46) | 67% (n=9) | 100% (n=18) | 100% (n=2) | 100% (n=11) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 87% (n=54) |
| unik | 57% (n=572) | 37% (n=156) | 39% (n=329) | 46% (n=48) | 100% (n=1) | 72% (n=149) |
| unik (eksperyment) | 72% (n=150) | 56% (n=32) | 61% (n=85) | 57% (n=7) | 100% (n=117) | 80% (n=41) |
| wyjście z zasięgu | 95% (n=424) | 98% (n=66) | 98% (n=266) | 100% (n=14) | 100% (n=436) | 100% (n=14) |
| wyjście z zasięgu (eksperyment) | 92% (n=107) | 100% (n=18) | 98% (n=60) | 100% (n=6) | 98% (n=45) | 76% (n=49) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 63,2 | reakcja na cios wręcz 70,9, unik przed pociskiem 22,7, wyjście z okrążenia 6,3 | drgnięcie (trafiony) 10,7, brak wytrzymałości 7,2, w trakcie uniku 6,6, picie flaszki 5,1, inna 1,2, parowanie 0,5, blok 0,2, umiejętność 0,2, mocny atak 0,1, riposta 0,1, szybki atak 0,1 | 41% (n=1417) / 69% (n=453) |
| Gracz H&S / souls-like | 146,8 | reakcja na cios wręcz 102,2, unik przed pociskiem 52,6, doskok do odsłoniętego celu 35,5, wyjście z okrążenia 11,9 | w trakcie uniku 15,8, drgnięcie (trafiony) 11,3, brak wytrzymałości 5,3, parowanie 3,9, picie flaszki 3,5, inna 1,2, blok 0,4, riposta 0,3, umiejętność 0,3, unieruchomienie 0,3, mocny atak 0,2 | 55% (n=2044) / 76% (n=1052) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 10,0 | 305,4 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 7,8 | 234,2 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 7,6 | 154,4 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 7,4 | 213,2 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 15 | 0 | 14,8 | 598,1 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 3 | 0 | 8,3 | 245,3 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 1 | 0 | 7,0 | 175,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 5,0 | 142,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 166185 | 16% | 792 | 210 |
| Zamach | 165254 | 15% |  |  |
| Rozpłatanie | 125138 | 12% | 671 | 186 |
| Tąpnięcie | 102684 | 10% | 900 | 114 |
| Fala przełamania | 77515 | 7% | 68 | 1140 |
| Upadek kata | 65559 | 6% | 223 | 294 |
| Cięcie | 64281 | 6% |  |  |
| Egzekucja | 62762 | 6% | 13 | 4828 |
| Okrzyk bojowy | 42555 | 4% | 1058 | 40 |
| Potężne cięcie | 30286 | 3% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×70.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 1710146 | 42% | 3422 | 500 |
| Fala przełamania | 990268 | 24% | 1109 | 893 |
| Rozpłatanie | 718437 | 18% | 3699 | 194 |
| Cięcie | 284585 | 7% |  |  |
| Potężne cięcie | 105767 | 3% |  |  |
| Riposta | 68435 | 2% |  |  |
| Szarża | 67831 | 2% | 1117 | 61 |
| Okrzyk bojowy | 49648 | 1% | 782 | 63 |
| Młynek | 46169 | 1% | 187 | 247 |
| Upadek kata | 19453 | 0% | 114 | 171 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×6.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,4, mediana 5,5, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 27% | 68,7 | 202 | 0,7 |
| odblokowanie | 18% | 45,0 | 114 | 0,7 |
| klasa | 21% | 52,9 | 40 | 0,1 |
| nagroda | 13% | 33,2 | 147 | 1,0 |
| sklep | 20% | 51,3 | 203 | 0,7 |

Oferty sklepu dusz: widziane 144, kupione 39, odrzucone 0, za mało dusz 105.
Zamiany w slotach: 2,8 na podejście; najczęstsze: Cięcie wichru → Tąpnięcie ×7, Tąpnięcie → Rozpłatanie ×3, Rozpłatanie → Tąpnięcie ×3, Tąpnięcie → Cięcie wichru ×3, Tąpnięcie → Odtrącenie ×2, Tąpnięcie → Upadek kata ×2.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 20 | 52,9 | 40 | 0,1 |
| Tąpnięcie | odblokowanie | 20 | 45,0 | 114 | 0,7 |
| Cięcie wichru | startowy | 11 | 72,0 | 210 | 0,6 |
| Rozpłatanie | startowy | 11 | 61,0 | 186 | 0,8 |
| Upadek kata | nagroda | 6 | 37,2 | 294 | 0,5 |
| Wypad | sklep | 4 | 52,5 | 12 | 1,0 |
| Krwawe cięcie | nagroda | 4 | 50,0 | 142 | 1,0 |
| Młynek | sklep | 5 | 28,2 | 169 | 0,6 |
| Cięcie z wyskoku | sklep | 4 | 34,0 | 167 | 0,6 |
| Uderzenie tarczą | sklep | 3 | 40,0 | 52 | 0,9 |
| Cięcie krzyżowe | nagroda | 3 | 34,7 | 263 | 0,7 |
| Szarża | nagroda | 4 | 22,5 | 104 | 0,8 |
| Odtrącenie | nagroda | 3 | 23,3 | 5 | 0,7 |
| Okrzyk wojenny | sklep | 2 | 35,0 | – | 1,7 |
| Fala przełamania | sklep | 2 | 34,0 | 1140 | 1,0 |
| Rozdarcie ran | sklep | 2 | 26,5 | 75 | 0,7 |
| Seria cięć | sklep | 1 | 42,0 | 293 | 2,0 |
| Kontra | sklep | 1 | 33,0 | 24 | 2,0 |
| Piekielna szarża | nagroda | 1 | 26,0 | 265 | 1,0 |
| Egzekucja | sklep | 1 | 13,0 | 355 | 2,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,0, mediana 5,0, najwięcej 11 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 37% | 197,1 | 459 | 1,5 |
| odblokowanie | 43% | 227,2 | 158 | 2,3 |
| klasa | 7% | 39,1 | 63 | 1,2 |
| nagroda | 6% | 32,2 | 1002 | 1,6 |
| sklep | 6% | 32,4 | 609 | 1,9 |

Oferty sklepu dusz: widziane 239, kupione 8, odrzucone 108, za mało dusz 123.
Zamiany w slotach: 1,6 na podejście; najczęstsze: Szarża → Fala przełamania ×8, Szarża → Młynek ×5, Rozpłatanie → Tąpnięcie ×2, Tąpnięcie → Fala przełamania ×1, Cięcie wichru → Uderzenie tarczą ×1, Szarża → Krwawe cięcie ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | odblokowanie | 20 | 185,0 | 194 | 2,6 |
| Cięcie wichru | startowy | 16 | 213,9 | 500 | 1,6 |
| Szarża | odblokowanie | 16 | 69,8 | 61 | 1,2 |
| Fala przełamania | sklep | 9 | 123,2 | 893 | 1,8 |
| Okrzyk bojowy | klasa | 20 | 39,1 | 63 | 1,2 |
| Młynek | odblokowanie | 6 | 31,2 | 247 | 1,9 |
| Upadek kata | nagroda | 1 | 114,0 | 171 | 1,4 |
| Tąpnięcie | odblokowanie | 3 | 19,3 | 65 | 0,0 |
| Seria cięć | sklep | 1 | 16,0 | 257 | 4,0 |
| Cięcie krzyżowe | sklep | 1 | 13,0 | 213 | 2,0 |
| Egzekucja | sklep | 1 | 11,0 | 161 | 4,0 |
| Uderzenie tarczą | odblokowanie | 1 | 7,0 | 50 | 0,4 |
| Okrzyk wojenny | sklep | 1 | 6,0 | – | 0,0 |
| Kontra | odblokowanie | 1 | 6,0 | 19 | 0,0 |
| Krwawe cięcie | odblokowanie | 1 | 5,0 | 104 | 0,6 |
| Rozdarcie ran | odblokowanie | 1 | 5,0 | 67 | 0,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 251,0, szybki atak 121,2, drgnięcie (trafiony) 87,8, unik 63,2, mocny atak 44,0, blok 32,7, flaszka (próba) 23,2, przełamana garda 8,8, parowanie 4,5, riposta 1,2.
Czas w bloku 15 s, w biegu 62 s, bez wytrzymałości (<10%) 250 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Żelazna skóra ×6, wzmocnienie: Oddech wojownika ×5, wzmocnienie: Alchemik ×5, ulepszenie: Ulepszenie: Cięcie wichru → +2 ×5, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×5, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×5, wzmocnienie: Hart ducha ×4, wzmocnienie: Czujność ×4.
Zakupy: ulepszenie broni ×19, pieczęć: Żniwiarz ×9, ulepszenie: Tąpnięcie ×9, dodatkowa flaszka ×8, ulepszenie: Rozpłatanie ×7, pieczęć: Pełny obrót ×6, pieczęć: Bumerang wiatru ×6, ulepszenie: Cięcie wichru ×6.
Punkty cech: Strength 42, Toughness 37, Dexterity 35, Intelligence 30.
Pieczęcie na podejście: z daru bossa 2,0, z sakwy do skilli 1,9, kupione 4,0; najczęstsze: Żniwiarz ×9, Pełny obrót ×6, Wieczny płomień ×6, Bumerang wiatru ×6, Krzyż wichru ×5, Deszcz piekieł ×5.
Komunikaty gry: „OKRZYK!” ×1072; „SZAŁ!” ×466; „GARDA PRZEŁAMANA!” ×190; „Garda wroga przełamana!” ×144; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×143; „Portale gasną – dobij pozostałe demony” ×115; „ZAMROŻENIE!” ×105; „POLEGŁEŚ” ×76.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 527,9, szybki atak 197,8, unik 146,8, drgnięcie (trafiony) 138,5, blok 68,3, mocny atak 48,8, parowanie 32,1, flaszka (próba) 31,2, riposta 11,8, przełamana garda 8,8.
Czas w bloku 38 s, w biegu 101 s, bez wytrzymałości (<10%) 213 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +2 ×12, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×10, wzmocnienie: Żelazna skóra ×9, wzmocnienie: Obrońca ×9, wzmocnienie: Oddech wojownika ×9, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×8, ulepszenie: Ulepszenie: Rozpłatanie → +3 ×8, wzmocnienie: Pijawka ×8.
Zakupy: ulepszenie broni ×39, ulepszenie: Okrzyk bojowy ×25, ulepszenie: Rozpłatanie ×22, ulepszenie: Cięcie wichru ×18, dodatkowa flaszka ×16, ulepszenie: Szarża ×12, pieczęć: Krzyż wichru ×10, ulepszenie: Fala przełamania ×9.
Punkty cech: Toughness 87, Strength 152.
Pieczęcie na podejście: z daru bossa 3,5, z sakwy do skilli 2,3, kupione 3,9; najczęstsze: Krzyż wichru ×10, Deszcz piekieł ×8, Oko Czarnoboga ×7, Bumerang wiatru ×6, Serce otchłani ×6, Pakt z diabłem ×6.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Kosa, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×1014; „SZAŁ!” ×616; „PAROWANIE!” ×489; „Za mało zdrowia” ×399; „Postawa przełamana – RIPOSTA!” ×357; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×213; „RIPOSTA!” ×209; „GARDA PRZEŁAMANA!” ×182.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×491, zablokowany ruch ×17, garda nie zatrzymała ciosu ×5, limit czasu piętra ×1, uśpiony wróg blokuje koniec piętra ×1.
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; -0,1) (w 2 podejściach)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 152°)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,5; 1,1)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 163°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,8; 0,6)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; 0,1)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; -0,2)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,2; 1,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; -0,2)
  - piętro 8 „VIII. Zatopione Krużganki” > 600 s; pozostali: Kultysta [Moving] (8; 0; 14), Kolos [Moving] (14; 1; 13), Kultysta topieli [Moving] (12; 0; 8), Czerw [Moving] (14; 0; 15); w drodze: 0
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (6,6; -6,9)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; 7,8)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×1225, zablokowany ruch ×53, parowanie w oknie nie zadziałało ×44, uśpiony wróg blokuje koniec piętra ×43, garda nie zatrzymała ciosu ×9.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; -0,1)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 179°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 146°)
  - Grzebacz: „Grzebacz” trafił w aktywnym oknie parowania (kąt do źródła 130°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 141°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** 1 podejść przerwanych limitem czasu piętra – możliwy nieosiągalny wróg albo zbyt długie piętro (zob. anomalie).
- **Problem (P2):** zablokowany ruch średnio 0,9× na podejście (geometria areny/kolizje). Przykłady: piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; -0,1); piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,5; 1,1); piętro 2 „II. Kult Bramy”: brak postępu przy (-7,8; 0,6).
- **Do sprawdzenia (P3):** 5× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 152°)
- Parowanie zwykłych ciosów: 70% ciosów mimo to rani (n=115) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 47% ciosów mimo to rani (n=55) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 40% ciosów (n=188).
- Blok zwykłych ciosów: 74% ciosów mimo to rani (n=242) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=481) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 31% pocisków, przed którymi był unik (n=453).
- Blok pocisku: 84% ciosów mimo to rani (n=302) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 64% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2155) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=365).
- Telegraf „nieparowalny” daje czas: tylko 12% decyzji zapada mniej niż 0,1 s przed trafieniem (n=633).
- Telegraf „nieblokowalny” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=67).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=889).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 212 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 20 podejściach. Średnio dochodzę do piętra 13,0 – na razie to ściana, nie krzywa.
- Progres między podejściami słabo odczuwalny: 13,0 → 12,9 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 42% udanych (n=899). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (57,0 trafień, 40,3 s), Kolos (elita) (42,7 trafień, 20,3 s), Legionista (elita) (35,0 trafień, 25,4 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kamienny chochlik (1,4), Wodny chochlik (1,6), Czerw (1,7) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Pan Sfory – średnio 40 s i 57 trafień do zabicia.
- Długie starcie: Królowa Topieli – średnio 97 s i 220 trafień do zabicia.
- Długie starcie: Rzeźnik – średnio 87 s i 233 trafień do zabicia.
- Długie starcie: Tytan Otchłani – średnio 65 s i 431 trafień do zabicia.
- Długie starcie: Matka Burz – średnio 86 s i 412 trafień do zabicia.
- Długie starcie: Król Kości – średnio 90 s i 460 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 118 s i 561 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 94 s i 633 trafień do zabicia.
- 9/20 razy zginąłem z flaszkami w kieszeni. 5 z tych śmierci to utrata ≥50% HP w 3 s, a 13% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 88% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 893 obrażeń na użycie.
- Skill startowy to 37% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 12%. Różnych skilli na podejście: średnio 5,0.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (15×, śr. piętro 14,8). Inne: Miecz i tarcza + Rozpłatanie 3× (8,3), Dwuręczny + Rozpłatanie 1× (5,0), Dwuręczny + Cięcie wichru 1× (7,0).
- Unik to moje główne narzędzie: 146,8 na podejście, w tym 47,4 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 76% przypadków.
- Gra nie wykonała uniku od razu średnio 42,2× na podejście – najczęściej: w trakcie poprzedniego uniku 15,8, drgnięcie (trafiony) 11,3, brak wytrzymałości 5,3. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (13% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (138 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

