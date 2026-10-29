# Raport botów grających — Wojownik

Wygenerowano: 2026-10-29 08:45. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 10,0 | 10,0 | 19,2 min | 306,9 | 91041 / 6988 | 21,8 | 1% |
| Gracz H&S / souls-like | 20 | 1 (5%) | 16,2 | 14,0 | 32,3 min | 655,8 | 390284 / 12046 | 33,3 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 8 10 11 9 8 9 10 9 11 9 10 9 10 10 9 10 11 17 10 9
- Gracz H&S / souls-like: 8 13 10 25 13 14 ★ 26 14 25 22 17 14 10 13 14 13 25 8 10

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×7, 11. XI. Rzeźnia ×3, 17. XVII. Wieża Burz ×1, 8. VIII. Zatopione Krużganki ×2, 9. IX. Królowa Topieli ×7.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×4; Królowa Topieli – „Królowa Topieli: TidalWave” ×2; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×2; Królowa Topieli – „Królowa Topieli: WindStep” ×2; Królowa Topieli – „Królowa Topieli: Combo” ×2; Ognisty chochlik – „Dwa cięcia” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 15/20.
  Ostatnie 3 s przed śmiercią: średnio 32% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/20). Picie: 519 prób, 436 wypitych (16% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 10. X. Rzeźnia ×3, 13. XIII. Kamienne Trzewia ×4, 14. XIV. Kamienne Trzewia ×4, 17. XVII. Wieża Burz ×1, 22. XXII. Chór Cieni ×1, 25. XXV. Kuźnia Piekieł ×3, 26. XXVI. Kuźnia Piekieł ×1, 8. VIII. Zatopione Krużganki ×2.
  Ostatni cios: Taranowiec – „Taranowiec: Combo” ×3; Miotacz głazów – „Miotacz głazów” ×2; Pajączek – „Pajączek” ×2; Grzebacz – „Grzebacz: StoneSpikes” ×2; Kolos – „Kolos: Combo” ×2; Czerw – „Ugryzienie” ×1. Śmierć ciosem ≥40% maks. HP: 0/19. Zginął mając flaszki: 16/19.
  Ostatnie 3 s przed śmiercią: średnio 34% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/19). Picie: 778 prób, 666 wypitych (14% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 106 s | 6 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 110 s | 38 | 0,0 | 41,8 |
| Tester QA | 3. III. Władca Bramy | 20 | 20 | 28 s | 247 | 0,7 | 8,1 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 112 s | 56 | 0,4 | 45,6 |
| Tester QA | 5. V. Psiarnia Piekieł | 20 | 20 | 96 s | 93 | 0,2 | 41,6 |
| Tester QA | 6. VI. Pan Sfory | 21 | 20 | 39 s | 481 | 2,3 | 5,6 |
| Tester QA | 7. VII. Zatopione Krużganki | 26 | 20 | 107 s | 401 | 1,7 | 38,7 |
| Tester QA | 8. VIII. Zatopione Krużganki | 32 | 18 | 133 s | 679 | 2,6 | 43,2 |
| Tester QA | 9. IX. Królowa Topieli | 38 | 11 | 72 s | 1124 | 3,6 | 1,2 |
| Tester QA | 10. X. Rzeźnia | 27 | 4 | 109 s | 924 | 2,4 | 36,9 |
| Tester QA | 11. XI. Rzeźnia | 10 | 1 | 129 s | 856 | 1,8 | 34,4 |
| Tester QA | 12. XII. Rzeźnik | 1 | 1 | 78 s | 535 | 2,0 | 7,0 |
| Tester QA | 13. XIII. Kamienne Trzewia | 1 | 1 | 147 s | 517 | 1,0 | 70,0 |
| Tester QA | 14. XIV. Kamienne Trzewia | 1 | 1 | 123 s | 871 | 1,0 | 67,0 |
| Tester QA | 15. XV. Tytan Otchłani | 1 | 1 | 107 s | 2000 | 5,0 | 3,0 |
| Tester QA | 16. XVI. Wieża Burz | 2 | 1 | 157 s | 1889 | 2,0 | 60,0 |
| Tester QA | 17. XVII. Wieża Burz | 3 | 0 | 0 s | 1608 | 1,0 | 54,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 100 s | 9 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 20 | 113 s | 27 | 0,0 | 40,7 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 23 s | 130 | 0,2 | 7,6 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 21 | 20 | 113 s | 84 | 0,1 | 44,1 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 20 | 20 | 96 s | 42 | 0,2 | 37,6 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 20 | 20 | 31 s | 291 | 1,3 | 5,9 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 20 | 20 | 101 s | 264 | 0,8 | 42,2 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 28 | 18 | 130 s | 673 | 2,1 | 43,0 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 22 | 18 | 57 s | 942 | 3,0 | 2,5 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 30 | 15 | 113 s | 697 | 1,7 | 45,3 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 22 | 15 | 133 s | 797 | 2,0 | 50,8 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 15 | 15 | 47 s | 191 | 0,4 | 7,0 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 30 | 11 | 136 s | 942 | 2,1 | 46,8 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 20 | 7 | 134 s | 1025 | 2,6 | 49,1 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 7 | 7 | 44 s | 709 | 2,3 | 3,0 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 8 | 7 | 146 s | 727 | 2,8 | 75,4 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 10 | 6 | 160 s | 946 | 2,4 | 73,0 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 6 | 6 | 66 s | 1483 | 4,5 | 3,0 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 8 | 6 | 165 s | 777 | 3,8 | 76,4 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 7 | 6 | 184 s | 1017 | 3,4 | 94,6 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 6 | 6 | 61 s | 597 | 1,7 | 17,7 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 8 | 5 | 181 s | 922 | 3,5 | 90,3 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 8 | 5 | 185 s | 1244 | 2,8 | 94,6 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 5 | 5 | 66 s | 350 | 0,8 | 3,4 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 11 | 2 | 173 s | 1581 | 2,7 | 69,5 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 4 | 1 | 179 s | 1812 | 5,5 | 94,3 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 1 | 1 | 48 s | 821 | 2,0 | 2,0 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 1 | 1 | 196 s | 886 | 6,0 | 132,0 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 1 | 1 | 189 s | 817 | 3,0 | 128,0 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 2 | 1 | 70 s | 998 | 1,5 | 0,5 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 6 ×1, piętro 7 ×6, piętro 8 ×12, piętro 9 ×20, piętro 10 ×16, piętro 11 ×6, piętro 16 ×1, piętro 17 ×2; wyzwań (3 porażki na piętrze): 6 – piętro 8, piętro 10, piętro 11, piętro 17.
**Gracz H&S / souls-like** – ponowne próby: piętro 4 ×1, piętro 8 ×8, piętro 9 ×4, piętro 10 ×12, piętro 11 ×7, piętro 13 ×15, piętro 14 ×9, piętro 16 ×1, piętro 17 ×3, piętro 19 ×2, piętro 20 ×1, piętro 22 ×2, piętro 23 ×3, piętro 25 ×6, piętro 26 ×2, piętro 30 ×1; wyzwań (3 porażki na piętrze): 4 – piętro 8, piętro 13, piętro 25, piętro 26.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Królowa Topieli | 29 | 58,4 s | 37,2 | 2407 | 60504 | 2086 | 7 |
| Wiedźma wichru | 857 | 1,3 s | 1,4 | 3756 | 42033 | 48 | 4 |
| Miotacz głazów | 360 | 0,6 s | 1,2 | 1187 | 41040 | 112 | 6 |
| Kultysta topieli | 983 | 0,2 s | 1,1 | 1784 | 34567 | 34 | 2 |
| Kultysta | 1957 | 0,2 s | 1,1 | 1245 | 20965 | 10 | 1 |
| Harpia | 745 | 0,6 s | 1,4 | 1463 | 19396 | 25 | 0 |
| Kolos | 929 | 1,7 s | 2,0 | 361 | 14693 | 15 | 4 |
| Pan Sfory | 40 | 30,7 s | 20,3 | 713 | 14179 | 354 | 0 |
| Ogar otchłani | 1904 | 0,3 s | 1,2 | 1418 | 14164 | 7 | 0 |
| Grzebacz | 270 | 0,4 s | 1,3 | 288 | 10087 | 37 | 3 |
| Matka Burz | 6 | 62,3 s | 38,7 | 309 | 8248 | 1375 | 0 |
| Taranowiec | 1041 | 1,0 s | 1,6 | 504 | 7291 | 7 | 3 |
| Siarkowy pomiot | 1193 | 0,1 s | 1,1 | 189 | 7071 | 6 | 2 |
| Władca Bramy | 40 | 21,5 s | 12,9 | 289 | 7047 | 176 | 0 |
| Tytan Otchłani | 8 | 48,2 s | 30,8 | 157 | 6962 | 870 | 0 |
| Czerw | 4860 | 0,2 s | 1,1 | 1916 | 6729 | 1 | 1 |
| Kolos żaru | 316 | 1,6 s | 1,9 | 184 | 6631 | 20 | 0 |
| Legionista | 860 | 3,5 s | 3,2 | 737 | 6498 | 7 | 1 |
| Upiór | 194 | 0,9 s | 1,3 | 709 | 6195 | 31 | 0 |
| Gnilec | 365 | 2,9 s | 2,4 | 340 | 5403 | 14 | 0 |
| Tkaczka | 208 | 1,4 s | 1,4 | 409 | 4921 | 23 | 0 |
| Gnilec (odłamek) | 719 | 0,5 s | 1,3 | 304 | 4199 | 6 | 0 |
| Ofiarnik | 312 | 0,5 s | 1,1 | 481 | 3631 | 11 | 1 |
| Król Kości | 6 | 57,7 s | 42,7 | 135 | 3436 | 573 | 0 |
| Rzeźnik | 16 | 44,7 s | 28,7 | 277 | 3329 | 208 | 0 |
| Cień | 376 | 0,3 s | 1,2 | 225 | 2924 | 8 | 0 |
| Pajączek | 174 | 0,0 s | 1,0 | 310 | 2408 | 14 | 2 |
| Ognisty chochlik | 876 | 0,1 s | 1,1 | 212 | 2229 | 2 | 1 |
| Gargulec | 204 | 1,3 s | 1,9 | 127 | 2115 | 10 | 0 |
| Arcydiabeł | 1 | 65,0 s | 93,0 | 65 | 1995 | 1995 | 0 |
| Bezimienny Cień | 5 | 62,4 s | 50,8 | 149 | 1718 | 344 | 0 |
| Chochlik wichru | 770 | 0,5 s | 1,4 | 157 | 1673 | 2 | 0 |
| Nekromanta | 130 | 0,4 s | 1,3 | 277 | 1570 | 12 | 1 |
| Ożywiony nietoperz | 587 | 0,0 s | 1,0 | 316 | 1525 | 3 | 0 |
| Wodny chochlik | 790 | 0,1 s | 1,1 | 159 | 1328 | 2 | 0 |
| Kamienny chochlik | 794 | 0,1 s | 1,1 | 110 | 1172 | 1 | 0 |
| Kowal Piekieł | 1 | 44,1 s | 68,0 | 19 | 821 | 821 | 0 |
| Czerw (elita) | 43 | 1,3 s | 1,8 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 47 | 0,8 s | 1,8 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 56 | 1,9 s | 2,3 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 24 | 2,6 s | 3,0 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 40 | 2,2 s | 2,3 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 23 | 9,9 s | 8,1 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 50 | 2,5 s | 3,1 | ↑ | ↑ | ↑ |  |
| Worek jaj | 142 | 0,1 s | 1,1 | 0 | 0 | 0 | 0 |
| Ofiarnik (elita) | 11 | 4,1 s | 3,0 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 34 | 6,0 s | 4,5 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 24 | 6,4 s | 2,3 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 14 | 15,3 s | 3,6 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 23 | 8,0 s | 6,0 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 6 | 13,5 s | 4,5 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 4 | 4,1 s | 4,3 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 9 | 2,4 s | 2,8 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 8 | 6,0 s | 5,3 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 5 | 5,9 s | 3,2 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 9 | 4,5 s | 5,1 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 6 | 5,2 s | 3,2 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 3 | 17,9 s | 3,3 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 1 | 1,9 s | 3,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3099 | 69% | 10% | 2% | 14% | 5% |
| ciężki | 342 | 58% | 6% | 6% | 26% | 4% |
| nieparowalny | 2225 | 68% | 12% | 0% | 15% | 5% |
| nieblokowalny | 143 | 79% | 0% | 0% | 21% | 0% |
| obszarowy | 2869 | 80% | 15% | 0% | 0% | 6% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 6045 | 60% | 5% | 1% | 30% | 4% |
| ciężki | 596 | 47% | 2% | 3% | 47% | 1% |
| nieparowalny | 3228 | 60% | 11% | 0% | 21% | 7% |
| nieblokowalny | 233 | 80% | 0% | 0% | 20% | 0% |
| obszarowy | 4908 | 83% | 12% | 0% | 0% | 5% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2547 | 25% | 38% | 3% | 17% | 18% |
| Tester QA (0,30 s) | ciężki | 439 | 1% | 0% | 2% | 17% | 80% |
| Tester QA (0,30 s) | nieparowalny | 799 | 6% | 4% | 14% | 36% | 40% |
| Tester QA (0,30 s) | nieblokowalny | 55 | 4% | 2% | 2% | 22% | 71% |
| Tester QA (0,30 s) | obszarowy | 1488 | 2% | 2% | 3% | 24% | 69% |
| Tester QA (0,30 s) | pocisk | 2195 | 5% | 17% | 12% | 20% | 46% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 5522 | 4% | 24% | 33% | 9% | 30% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 856 | 4% | 3% | 2% | 25% | 67% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1495 | 6% | 2% | 3% | 15% | 73% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 55 | 2% | 0% | 0% | 7% | 91% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 2773 | 2% | 1% | 3% | 18% | 75% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 3845 | 3% | 16% | 12% | 15% | 54% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 982 |
| Pajączek: Pajączek | 0,31 s | 417 |
| Ognisty chochlik: Pazury | 0,33 s | 317 |
| Wodny chochlik: Pazury | 0,33 s | 266 |
| Chochlik wichru: Pazury | 0,33 s | 259 |
| Czerw: Ugryzienie | 0,33 s | 2896 |
| Kamienny chochlik: Pazury | 0,33 s | 201 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 524 |
| Władca Bramy: Piekielne kule | 0,41 s | 9 |
| Władca Bramy: Cięcie w szale | 0,41 s | 24 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 53% (n=219) | 0% (n=5) | 55% (n=200) | 100% (n=12) | 42% (n=397) | 83% (n=470) |
| blok (eksperyment) | 78% (n=125) | 79% (n=29) | 84% (n=102) | 100% (n=10) | 88% (n=177) | 57% (n=103) |
| brak reakcji (eksperyment) | 85% (n=127) | 90% (n=21) | 90% (n=80) | 100% (n=2) | 86% (n=139) | 74% (n=87) |
| niezauważony | 81% (n=595) | – | 90% (n=242) | 100% (n=44) | 92% (n=891) | – |
| parowanie | 64% (n=77) | 47% (n=36) | – | – | – | – |
| parowanie (eksperyment) | 68% (n=80) | 45% (n=11) | 90% (n=72) | – | 87% (n=63) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 80% (n=80) |
| unik | 58% (n=553) | 45% (n=140) | 42% (n=424) | 50% (n=56) | 67% (n=3) | 67% (n=226) |
| unik (eksperyment) | 63% (n=153) | 52% (n=31) | 65% (n=145) | 78% (n=9) | 96% (n=212) | 76% (n=79) |
| wyjście z zasięgu | 89% (n=526) | 96% (n=57) | 94% (n=300) | 100% (n=9) | 94% (n=883) | – |
| wyjście z zasięgu (eksperyment) | 89% (n=107) | 92% (n=12) | 91% (n=103) | 100% (n=1) | 90% (n=84) | 78% (n=69) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 74,6 | reakcja na cios wręcz 76,5, unik przed pociskiem 34,8, wyjście z okrążenia 6,0 | drgnięcie (trafiony) 12,2, brak wytrzymałości 11,6, w trakcie uniku 7,0, picie flaszki 5,2, parowanie 1,2, inna 1,0, unieruchomienie 0,3, blok 0,2, przełamana garda 0,1, mocny atak 0,1, umiejętność 0,1, riposta 0,1, szybki atak 0,1 | 34% (n=1529) / 70% (n=696) |
| Gracz H&S / souls-like | 194,3 | reakcja na cios wręcz 190,5, unik przed pociskiem 80,9, doskok do odsłoniętego celu 19,7, wyjście z okrążenia 6,8 | w trakcie uniku 35,5, drgnięcie (trafiony) 21,1, brak wytrzymałości 13,3, picie flaszki 9,2, unieruchomienie 2,0, inna 1,1, mocny atak 0,7, parowanie 0,6, blok 0,3, szybki atak 0,2, riposta 0,1, umiejętność 0,1 | 54% (n=3810) / 77% (n=1618) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 11,0 | 357,2 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 10,0 | 302,4 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 9,6 | 299,8 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 9,2 | 268,0 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 15 | 1 | 18,0 | 765,5 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 2 | 0 | 11,5 | 369,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 2 | 0 | 11,5 | 350,5 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 1 | 0 | 8,0 | 193,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 463681 | 25% | 1569 | 296 |
| Rozpłatanie | 339344 | 19% | 1282 | 265 |
| Zamach | 275797 | 15% |  |  |
| Tąpnięcie | 271346 | 15% | 1436 | 189 |
| Cięcie | 175252 | 10% |  |  |
| Egzekucja | 96306 | 5% |  |  |
| Okrzyk bojowy | 94466 | 5% | 1389 | 68 |
| Potężne cięcie | 86786 | 5% |  |  |
| Riposta | 9432 | 1% |  |  |
| Szarża barkiem | 8289 | 0% |  |  |

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 4115780 | 53% | 5237 | 786 |
| Rozpłatanie | 1360687 | 17% | 2221 | 613 |
| Egzekucja | 1058400 | 14% | 540 | 1960 |
| Zamach | 907856 | 12% |  |  |
| Okrzyk bojowy | 235606 | 3% | 820 | 287 |
| Szarża barkiem | 52920 | 1% |  |  |
| Cięcie | 25021 | 0% |  |  |
| Potężne cięcie | 17684 | 0% |  |  |
| Riposta | 13408 | 0% |  |  |
| Krwawe cięcie | 8621 | 0% | 52 | 166 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Szarża ×9.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 3,0, mediana 3,0, najwięcej 3 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 50% | 142,6 | 282 | 0,2 |
| odblokowanie | 25% | 71,8 | 189 | 0,0 |
| klasa | 24% | 69,5 | 68 | 0,0 |

Oferty sklepu dusz: widziane 537, kupione 0, odrzucone 0, za mało dusz 179.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 10 | 156,9 | 296 | 0,2 |
| Tąpnięcie | odblokowanie | 20 | 71,8 | 189 | 0,0 |
| Okrzyk bojowy | klasa | 20 | 69,5 | 68 | 0,0 |
| Rozpłatanie | startowy | 10 | 128,2 | 265 | 0,2 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,2, mediana 4,0, najwięcej 6 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 62% | 278,6 | 761 | 0,2 |
| odblokowanie | 29% | 128,1 | 902 | 0,1 |
| klasa | 9% | 41,0 | 287 | 0,0 |
| sklep | 0% | 0,3 | 446 | 1,0 |

Oferty sklepu dusz: widziane 912, kupione 1, odrzucone 0, za mało dusz 303.
Zamiany w slotach: 1,5 na podejście; najczęstsze: Szarża → Egzekucja ×17, Cięcie wichru → Uderzenie tarczą ×1, Szarża → Krwawe cięcie ×1, Uderzenie tarczą → Cięcie wichru ×1, Rozpłatanie → Młynek ×1, Rozpłatanie → Rozdarcie ran ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 17 | 308,1 | 786 | 0,3 |
| Rozpłatanie | odblokowanie | 20 | 111,1 | 613 | 0,1 |
| Okrzyk bojowy | klasa | 20 | 41,0 | 287 | 0,0 |
| Egzekucja | odblokowanie | 18 | 30,0 | 1960 | 0,0 |
| Krwawe cięcie | odblokowanie | 1 | 52,0 | 166 | 0,0 |
| Tąpnięcie | odblokowanie | 1 | 46,0 | 107 | 0,0 |
| Szarża | odblokowanie | 1 | 9,0 | – | 0,0 |
| Kontra | odblokowanie | 1 | 9,0 | 19 | 0,0 |
| Uderzenie tarczą | odblokowanie | 1 | 5,0 | 94 | 0,0 |
| Rozdarcie ran | odblokowanie | 1 | 5,0 | 81 | 0,0 |
| Młynek | odblokowanie | 1 | 5,0 | 157 | 0,0 |
| Seria cięć | odblokowanie | 1 | 5,0 | 148 | 0,0 |
| Fala przełamania | sklep | 1 | 5,0 | 446 | 1,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 283,8, szybki atak 130,6, drgnięcie (trafiony) 115,8, unik 74,6, blok 61,9, mocny atak 44,5, flaszka (próba) 26,0, przełamana garda 20,3, parowanie 9,3, riposta 1,6.
Czas w bloku 30 s, w biegu 78 s, bez wytrzymałości (<10%) 291 s na podejście.
Zakupy: dodatkowa flaszka ×16, cecha za dusze: Toughness (52) ×14, cecha za dusze: Dexterity (68) ×11, sklep: Miecz jednoręczny: poziom 3 ×10, sklep: Miecz jednoręczny: poziom 4 ×10, sklep: Miecz dwuręczny: poziom 3 ×10, cecha za dusze: Strength (45) ×9, założony przedmiot: Trzewiki płytowe ×9.
Punkty cech: Strength 36, Toughness 33, Dexterity 26, Intelligence 22.
Pieczęcie na podejście: z daru bossa 2,7, z sakwy do skilli 1,6, kupione 0,2; najczęstsze: Serce otchłani ×6, Nieskończoność ×5, Pakt z diabłem ×5, Oko Czarnoboga ×4, Wieczny płomień ×4, Bliźniacza dusza ×4.
Komunikaty gry: „OKRZYK!” ×1363; „SZAŁ!” ×627; „GARDA PRZEŁAMANA!” ×429; „Za mało zdrowia” ×223; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×182; „Garda wroga przełamana!” ×168; „ZAMROŻENIE!” ×155; „Portale gasną – dobij pozostałe demony” ×141.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 448,0, unik 194,3, drgnięcie (trafiony) 186,6, szybki atak 125,9, blok 124,2, mocny atak 90,6, flaszka (próba) 38,9, przełamana garda 36,0, parowanie 4,1, riposta 1,8.
Czas w bloku 67 s, w biegu 127 s, bez wytrzymałości (<10%) 371 s na podejście.
Zakupy: dodatkowa flaszka ×31, sklep: Kat ×24, sklep: Twarda skóra ×24, sklep: Ostrze ×23, sklep: Szybkie ręce ×23, sklep: Kamienne ostrze ×22, sklep: Hart ciała ×20, sklep: Płonące ostrze ×19.
Punkty cech: Toughness 56, Strength 93.
Pieczęcie na podejście: z daru bossa 4,9, z sakwy do skilli 2,0, kupione 0,0; najczęstsze: Nieskończoność ×15, Pakt z diabłem ×12, Klątwa Mokoszy ×10, Korona Welesa ×8, Bliźniacza dusza ×7, Oko Czarnoboga ×6.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Kosa, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×1480; „SZAŁ!” ×821; „GARDA PRZEŁAMANA!” ×760; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×297; „OGŁUSZENIE!” ×248; „Portale gasną – dobij pozostałe demony” ×220; „Za mało zdrowia” ×201; „Garda wroga przełamana!” ×150.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×800, garda nie zatrzymała ciosu ×21, zablokowany ruch ×20, uśpiony wróg blokuje koniec piętra ×14, parowanie w oknie nie zadziałało ×6.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 169°) (w 2 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 171°)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (-16,6; -1,1)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; 0,2)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; -0,1)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; 0,1)
  - Ognisty chochlik: „Pazury” przeszedł przez gardę (kąt 124°)
  - Ofiarnik: „Ofiarnik: BloodRite” przeszedł przez gardę (kąt 148°)
  - Gnilec (odłamek): „Gnilec: Vomit” przeszedł przez gardę (kąt 148°)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; 2,1)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×1358, uśpiony wróg blokuje koniec piętra ×113, zablokowany ruch ×28, garda nie zatrzymała ciosu ×25, parowanie w oknie nie zadziałało ×1.
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - Miotacz głazów: „Miotacz głazów” przeszedł przez gardę (kąt 180°) (w 2 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** zablokowany ruch średnio 1,0× na podejście (geometria areny/kolizje). Przykłady: piętro 9 „IX. Królowa Topieli”: brak postępu przy (-16,6; -1,1); piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; 0,2); piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; -0,1).
- **Do sprawdzenia (P3):** 6× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Siarkowy pomiot: „Siarkowy pomiot” trafił w aktywnym oknie parowania (kąt do źródła 144°)
- **Do sprawdzenia (P3):** 21× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 169°)
- Parowanie zwykłych ciosów: 66% ciosów mimo to rani (n=157) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 54% ciosów mimo to rani (n=65) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: 46% ciosów mimo to rani (n=171) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 62% ciosów mimo to rani (n=344) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 94% ciosów mimo to rani (n=967) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 30% pocisków, przed którymi był unik (n=696).
- Blok pocisku: 78% ciosów mimo to rani (n=573) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 63% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2547) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=439).
- Telegraf „nieparowalny” daje czas: tylko 10% decyzji zapada mniej niż 0,1 s przed trafieniem (n=799).
- Telegraf „nieblokowalny” daje czas: tylko 5% decyzji zapada mniej niż 0,1 s przed trafieniem (n=55).
- Telegraf „obszarowy” daje czas: tylko 4% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1488).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Za mało zdrowia” ×223 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 157 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 73 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 1/20. Trudno, ale do zrobienia – tak ma być w souls-like.
- Progres między podejściami słabo odczuwalny: 17,9 → 14,6 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 48% udanych (n=122). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Królowa Topieli (30,4 trafień, 53,0 s), Rzeźnik (27,3 trafień, 42,7 s), Pan Sfory (16,4 trafień, 26,5 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,0), Ożywiony nietoperz (1,0), Siarkowy pomiot (1,0) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Królowa Topieli – średnio 53 s i 30 trafień do zabicia.
- Długie starcie: Rzeźnik – średnio 43 s i 27 trafień do zabicia.
- Długie starcie: Tytan Otchłani – średnio 40 s i 27 trafień do zabicia.
- Długie starcie: Matka Burz – średnio 62 s i 39 trafień do zabicia.
- Długie starcie: Król Kości – średnio 58 s i 43 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 62 s i 51 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 44 s i 68 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 65 s i 93 trafień do zabicia.
- 16/19 razy zginąłem z flaszkami w kieszeni. 3 z tych śmierci to utrata ≥50% HP w 3 s, a 14% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 87% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Egzekucja”: 1960 obrażeń na użycie.
- Skill startowy to 62% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 0%. Różnych skilli na podejście: średnio 4,2.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (15×, śr. piętro 18,1). Inne: Miecz i tarcza + Cięcie wichru 2× (11,5), Dwuręczny + Rozpłatanie 2× (11,5), Miecz i tarcza + Rozpłatanie 1× (8,0).
- Unik to moje główne narzędzie: 194,3 na podejście, w tym 26,5 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 77% przypadków.
- Gra nie wykonała uniku od razu średnio 83,9× na podejście – najczęściej: w trakcie poprzedniego uniku 35,5, drgnięcie (trafiony) 21,1, brak wytrzymałości 13,3. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (19% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (187 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

