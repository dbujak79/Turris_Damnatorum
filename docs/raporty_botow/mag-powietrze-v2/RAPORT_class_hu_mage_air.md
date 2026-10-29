# Raport botów grających — Mag powietrza

Wygenerowano: 2026-10-29 18:55. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 6 (30%) | 19,3 | 18,5 | 40,6 min | 826,4 | 379988 / 4920 | 8,4 | 4% |
| Gracz H&S / souls-like | 20 | 15 (75%) | 27,2 | 30,0 | 52,6 min | 1401,3 | 700472 / 4230 | 5,5 | 8% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: ★ 14 10 11 10 13 ★ 11 17 20 ⏱20 ★ 11 14 22 13 ★ 20 ★ ★
- Gracz H&S / souls-like: ★ ★ ★ ★ 11 22 ★ ★ 28 16 ★ ★ ★ ★ ★ ★ ★ ★ 16 ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×2, 11. XI. Rzeźnia ×3, 13. XIII. Kamienne Trzewia ×2, 14. XIV. Kamienne Trzewia ×2, 17. XVII. Wieża Burz ×1, 20. XX. Kostnica ×2, 22. XXII. Chór Cieni ×1.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×4; Miotacz głazów – „Miotacz głazów: Rockfall” ×2; Ognisty chochlik – „Pazury” ×1; Gnilec – „Gnilec: Vomit” ×1; Tkaczka – „Tkaczka: Cocoon” ×1; Kultysta topieli – „Kultysta topieli: Zones” ×1. Śmierć ciosem ≥40% maks. HP: 0/13. Zginął mając flaszki: 13/13.
  Ostatnie 3 s przed śmiercią: średnio 48% maks. HP straconych (śmierć z ≥50% HP w 3 s: 6/13). Picie: 500 prób, 167 wypitych (67% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 11. XI. Rzeźnia ×1, 16. XVI. Wieża Burz ×2, 22. XXII. Chór Cieni ×1, 28. XXVIII. Tron Arcydemona ×1.
  Ostatni cios: Miotacz głazów – „Miotacz głazów: Rockfall” ×3; Miotacz głazów – „Miotacz głazów” ×1; Gnilec – „Gnilec” ×1. Śmierć ciosem ≥40% maks. HP: 0/5. Zginął mając flaszki: 5/5.
  Ostatnie 3 s przed śmiercią: średnio 45% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/5). Picie: 410 prób, 109 wypitych (73% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 104 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 114 s | 0 | 0,0 | 40,9 |
| Tester QA | 3. III. Władca Bramy | 22 | 20 | 21 s | 69 | 0,0 | 7,1 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 113 s | 0 | 0,0 | 45,3 |
| Tester QA | 5. V. Psiarnia Piekieł | 20 | 20 | 99 s | 5 | 0,0 | 43,1 |
| Tester QA | 6. VI. Pan Sfory | 21 | 20 | 35 s | 68 | 0,1 | 5,4 |
| Tester QA | 7. VII. Zatopione Krużganki | 20 | 20 | 96 s | 11 | 0,4 | 41,8 |
| Tester QA | 8. VIII. Zatopione Krużganki | 22 | 20 | 129 s | 57 | 0,5 | 51,4 |
| Tester QA | 9. IX. Królowa Topieli | 20 | 20 | 89 s | 104 | 0,4 | 2,7 |
| Tester QA | 10. X. Rzeźnia | 28 | 18 | 115 s | 211 | 0,4 | 50,0 |
| Tester QA | 11. XI. Rzeźnia | 26 | 15 | 129 s | 313 | 0,3 | 48,0 |
| Tester QA | 12. XII. Rzeźnik | 15 | 15 | 52 s | 32 | 0,1 | 4,6 |
| Tester QA | 13. XIII. Kamienne Trzewia | 20 | 13 | 143 s | 329 | 0,6 | 58,6 |
| Tester QA | 14. XIV. Kamienne Trzewia | 21 | 11 | 138 s | 358 | 0,5 | 50,9 |
| Tester QA | 15. XV. Tytan Otchłani | 16 | 11 | 53 s | 537 | 1,3 | 2,1 |
| Tester QA | 16. XVI. Wieża Burz | 16 | 11 | 163 s | 509 | 1,1 | 72,3 |
| Tester QA | 17. XVII. Wieża Burz | 14 | 10 | 165 s | 389 | 0,4 | 77,2 |
| Tester QA | 18. XVIII. Matka Burz | 10 | 10 | 141 s | 259 | 0,7 | 1,8 |
| Tester QA | 19. XIX. Kostnica | 17 | 10 | 165 s | 494 | 0,4 | 81,6 |
| Tester QA | 20. XX. Kostnica | 17 | 7 | 175 s | 575 | 0,8 | 72,3 |
| Tester QA | 21. XXI. Król Kości | 7 | 7 | 79 s | 191 | 0,1 | 7,0 |
| Tester QA | 22. XXII. Chór Cieni | 11 | 6 | 181 s | 566 | 0,8 | 82,8 |
| Tester QA | 23. XXIII. Chór Cieni | 6 | 6 | 179 s | 12 | 0,0 | 105,3 |
| Tester QA | 24. XXIV. Bezimienny Cień | 6 | 6 | 96 s | 0 | 0,2 | 2,5 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 6 | 6 | 180 s | 53 | 0,0 | 106,8 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 6 | 6 | 188 s | 48 | 0,0 | 122,3 |
| Tester QA | 27. XXVII. Kowal Piekieł | 7 | 6 | 65 s | 446 | 0,7 | 1,6 |
| Tester QA | 28. XXVIII. Tron Arcydemona | 6 | 6 | 176 s | 203 | 0,2 | 127,3 |
| Tester QA | 29. XXIX. Tron Arcydemona | 6 | 6 | 199 s | 81 | 0,2 | 135,3 |
| Tester QA | 30. XXX. Arcydiabeł | 8 | 6 | 106 s | 895 | 1,3 | 5,8 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 103 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 20 | 109 s | 0 | 0,0 | 41,6 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 11 s | 5 | 0,0 | 6,4 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 20 | 20 | 112 s | 0 | 0,0 | 45,1 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 20 | 20 | 97 s | 0 | 0,0 | 40,4 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 20 | 20 | 18 s | 16 | 0,0 | 4,8 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 20 | 20 | 92 s | 2 | 0,0 | 41,4 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 21 | 20 | 139 s | 53 | 0,0 | 52,1 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 20 | 20 | 35 s | 26 | 0,0 | 2,7 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 21 | 20 | 112 s | 21 | 0,0 | 60,1 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 26 | 19 | 134 s | 259 | 0,3 | 53,5 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 19 | 19 | 25 s | 4 | 0,0 | 4,2 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 20 | 19 | 135 s | 131 | 0,3 | 64,2 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 19 | 19 | 141 s | 142 | 0,4 | 69,9 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 21 | 19 | 35 s | 338 | 0,7 | 2,6 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 23 | 17 | 153 s | 351 | 0,5 | 74,7 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 18 | 17 | 156 s | 128 | 0,3 | 82,6 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 17 | 17 | 61 s | 81 | 0,0 | 2,3 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 19 | 17 | 160 s | 196 | 0,4 | 85,4 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 21 | 17 | 166 s | 241 | 0,2 | 89,6 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 17 | 17 | 46 s | 114 | 0,0 | 10,3 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 20 | 16 | 173 s | 386 | 0,5 | 102,4 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 16 | 16 | 177 s | 152 | 0,4 | 107,4 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 17 | 16 | 54 s | 11 | 0,0 | 2,4 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 16 | 16 | 188 s | 12 | 0,0 | 115,5 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 17 | 16 | 190 s | 131 | 0,1 | 115,9 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 18 | 16 | 67 s | 499 | 0,8 | 1,6 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 24 | 15 | 194 s | 482 | 0,3 | 112,2 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 15 | 15 | 192 s | 19 | 0,0 | 125,9 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 16 | 15 | 84 s | 421 | 0,4 | 5,9 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 3 ×2, piętro 6 ×1, piętro 8 ×2, piętro 10 ×8, piętro 11 ×8, piętro 13 ×5, piętro 14 ×8, piętro 15 ×5, piętro 16 ×5, piętro 17 ×3, piętro 19 ×7, piętro 20 ×7, piętro 22 ×4, piętro 27 ×1, piętro 30 ×2; wyzwań (3 porażki na piętrze): 4 – piętro 14, piętro 17, piętro 20, piętro 22.
**Gracz H&S / souls-like** – ponowne próby: piętro 8 ×1, piętro 10 ×1, piętro 11 ×6, piętro 13 ×1, piętro 15 ×2, piętro 16 ×4, piętro 17 ×1, piętro 19 ×2, piętro 20 ×4, piętro 22 ×3, piętro 24 ×1, piętro 26 ×1, piętro 27 ×2, piętro 28 ×8, piętro 30 ×1; wyzwań (3 porażki na piętrze): 3 – piętro 11, piętro 22, piętro 28.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 1337 | 2,3 s | 4,8 | 3197 | 64745 | 47 | 10 |
| Tytan Otchłani | 30 | 37,3 s | 160,9 | 359 | 15688 | 523 | 0 |
| Arcydiabeł | 21 | 84,6 s | 553,6 | 633 | 13889 | 661 | 0 |
| Kowal Piekieł | 22 | 61,7 s | 328,0 | 420 | 12111 | 550 | 0 |
| Grzebacz | 1058 | 1,7 s | 5,5 | 850 | 11105 | 10 | 1 |
| Wiedźma wichru | 2079 | 2,4 s | 5,0 | 4274 | 9012 | 4 | 0 |
| Kultysta topieli | 2347 | 1,3 s | 3,1 | 1560 | 7259 | 3 | 1 |
| Ogar otchłani | 3267 | 1,0 s | 3,6 | 1298 | 5229 | 2 | 1 |
| Taranowiec | 2292 | 1,8 s | 5,7 | 372 | 4066 | 2 | 0 |
| Matka Burz | 27 | 86,8 s | 377,0 | 764 | 3972 | 147 | 0 |
| Harpia | 1724 | 1,3 s | 4,8 | 2267 | 3553 | 2 | 0 |
| Król Kości | 24 | 51,8 s | 276,1 | 344 | 3251 | 135 | 0 |
| Cień | 1436 | 1,1 s | 4,8 | 328 | 2894 | 2 | 0 |
| Tkaczka | 729 | 2,5 s | 6,7 | 618 | 2568 | 3 | 1 |
| Królowa Topieli | 40 | 58,1 s | 160,8 | 750 | 2448 | 61 | 0 |
| Kolos | 2212 | 2,2 s | 5,9 | 38 | 2222 | 1 | 0 |
| Kultysta | 3196 | 0,7 s | 2,2 | 425 | 2077 | 1 | 1 |
| Kolos żaru | 1352 | 2,4 s | 9,0 | 65 | 1975 | 1 | 0 |
| Legionista | 2077 | 3,4 s | 10,3 | 231 | 1883 | 1 | 0 |
| Nekromanta | 622 | 3,0 s | 6,9 | 276 | 1852 | 3 | 0 |
| Upiór | 797 | 2,7 s | 6,5 | 1031 | 1421 | 2 | 0 |
| Pan Sfory | 40 | 23,5 s | 57,3 | 204 | 1324 | 33 | 0 |
| Władca Bramy | 40 | 11,4 s | 32,7 | 171 | 1216 | 30 | 0 |
| Ożywiony nietoperz | 1516 | 0,5 s | 1,6 | 162 | 1111 | 1 | 0 |
| Siarkowy pomiot | 2290 | 0,4 s | 2,0 | 24 | 938 | 0 | 0 |
| Pajączek | 482 | 0,3 s | 1,5 | 167 | 814 | 2 | 0 |
| Gargulec | 782 | 2,0 s | 6,9 | 70 | 777 | 1 | 0 |
| Czerw | 7904 | 0,4 s | 1,8 | 217 | 731 | 0 | 0 |
| Rzeźnik | 34 | 33,2 s | 140,1 | 92 | 546 | 16 | 0 |
| Ofiarnik | 740 | 2,6 s | 3,9 | 482 | 521 | 1 | 0 |
| Gnilec | 1239 | 3,1 s | 10,2 | 122 | 517 | 0 | 2 |
| Gnilec (odłamek) | 2536 | 1,3 s | 4,6 | 120 | 412 | 0 | 0 |
| Kamienny chochlik | 925 | 0,4 s | 1,9 | 18 | 219 | 0 | 0 |
| Bezimienny Cień | 22 | 61,7 s | 439,4 | 216 | 186 | 8 | 0 |
| Chochlik wichru | 963 | 1,2 s | 3,5 | 15 | 155 | 0 | 0 |
| Wodny chochlik | 944 | 0,6 s | 2,2 | 20 | 149 | 0 | 0 |
| Ognisty chochlik | 1098 | 0,6 s | 2,2 | 30 | 148 | 0 | 1 |
| Czerw (elita) | 57 | 2,3 s | 5,8 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 66 | 1,6 s | 6,6 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 69 | 4,6 s | 16,1 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 80 | 3,5 s | 7,8 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 90 | 2,4 s | 8,6 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 27 | 5,6 s | 28,0 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 38 | 10,5 s | 41,3 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 34 | 8,3 s | 20,3 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 27 | 5,4 s | 18,4 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 35 | 8,6 s | 46,7 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 70 | 8,2 s | 27,1 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 26 | 7,5 s | 18,8 | ↑ | ↑ | ↑ |  |
| Worek jaj | 358 | 0,9 s | 2,3 | 0 | 0 | 0 | 0 |
| Taranowiec (elita) | 78 | 6,7 s | 25,3 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 45 | 3,4 s | 20,1 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 61 | 7,8 s | 29,0 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 67 | 7,4 s | 20,4 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 88 | 3,6 s | 12,6 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 27 | 6,4 s | 26,0 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 76 | 6,4 s | 12,9 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 23 | 8,2 s | 21,3 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 19 | 15,5 s | 24,9 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3169 | 49% | 0% | 0% | 51% | 0% |
| ciężki | 175 | 45% | 0% | 0% | 55% | 0% |
| nieparowalny | 2397 | 44% | 0% | 0% | 56% | 0% |
| nieblokowalny | 155 | 69% | 0% | 0% | 31% | 0% |
| obszarowy | 5023 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3208 | 29% | 0% | 0% | 71% | 0% |
| ciężki | 153 | 24% | 0% | 0% | 76% | 0% |
| nieparowalny | 2305 | 23% | 0% | 0% | 77% | 0% |
| nieblokowalny | 181 | 60% | 0% | 0% | 40% | 0% |
| obszarowy | 5464 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 3104 | 24% | 17% | 3% | 15% | 41% |
| Tester QA (0,30 s) | ciężki | 499 | 1% | 1% | 5% | 31% | 63% |
| Tester QA (0,30 s) | nieparowalny | 1405 | 7% | 3% | 8% | 33% | 48% |
| Tester QA (0,30 s) | nieblokowalny | 46 | 4% | 7% | 4% | 43% | 41% |
| Tester QA (0,30 s) | obszarowy | 2211 | 1% | 2% | 7% | 43% | 48% |
| Tester QA (0,30 s) | pocisk | 3262 | 3% | 9% | 8% | 13% | 67% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3662 | 15% | 10% | 12% | 7% | 56% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 451 | 1% | 1% | 1% | 26% | 71% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1457 | 6% | 2% | 2% | 14% | 76% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 39 | 0% | 0% | 0% | 3% | 97% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 2725 | 2% | 2% | 7% | 35% | 54% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 3401 | 2% | 9% | 7% | 10% | 72% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 603 |
| Pajączek: Pajączek | 0,31 s | 325 |
| Ognisty chochlik: Pazury | 0,33 s | 111 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 479 |
| Wodny chochlik: Pazury | 0,33 s | 82 |
| Czerw: Ugryzienie | 0,33 s | 865 |
| Chochlik wichru: Pazury | 0,33 s | 100 |
| Kamienny chochlik: Pazury | 0,33 s | 78 |
| Władca Bramy: Cięcie w szale | 0,41 s | 9 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 2199 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 83% (n=175) | 94% (n=17) | 87% (n=168) | 100% (n=2) | 100% (n=117) | 80% (n=100) |
| niezauważony | 77% (n=533) | – | 83% (n=248) | 91% (n=77) | 100% (n=2973) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 89% (n=284) |
| unik | 32% (n=1204) | 20% (n=95) | 22% (n=1115) | 33% (n=43) | 100% (n=84) | 24% (n=384) |
| unik (eksperyment) | 40% (n=246) | 15% (n=13) | 28% (n=205) | 45% (n=20) | 100% (n=224) | 22% (n=64) |
| wyjście z zasięgu | 51% (n=181) | 88% (n=25) | 58% (n=118) | – | 100% (n=1172) | 55% (n=213) |
| wyjście z zasięgu (eksperyment) | 79% (n=256) | 80% (n=25) | 76% (n=238) | 91% (n=11) | 100% (n=186) | 52% (n=103) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 190,3 | reakcja na cios wręcz 175,8, unik przed pociskiem 63,2, wabienie pod czar obszarowy 7,4, wyjście z okrążenia 3,9 | w trakcie uniku 20,5, drgnięcie (trafiony) 18,9, brak wytrzymałości 8,6, picie flaszki 2,7, unieruchomienie 1,0, inna 0,2, umiejętność 0,1 | 68% (n=3516) / 92% (n=1264) |
| Gracz H&S / souls-like | 248,6 | reakcja na cios wręcz 223,4, unik przed pociskiem 78,0, doskok do odsłoniętego celu 23,9, wabienie pod czar obszarowy 12,0, wyjście z okrążenia 2,0 | w trakcie uniku 51,6, drgnięcie (trafiony) 12,5, brak wytrzymałości 10,8, picie flaszki 2,7, unieruchomienie 0,8, inna 0,4, umiejętność 0,1 | 76% (n=4468) / 95% (n=1560) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Powietrze | Nova powietrza | 20 | 6 | 19,3 | 826,4 |
| Gracz H&S / souls-like | Powietrze | Nova powietrza | 20 | 15 | 27,2 | 1401,3 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka powietrza | 5334420 | 70% |  |  |
| Nova powietrza | 821920 | 11% | 2234 | 368 |
| Nawałnica | 755987 | 10% | 1191 | 635 |
| Roztrzaskanie | 274459 | 4% |  |  |
| Ściana ognia | 175482 | 2% | 812 | 216 |
| Odskok widma | 93064 | 1% |  |  |
| Łza lodowca | 55492 | 1% | 24 | 2312 |
| Pocisk arkanów | 44672 | 1% | 565 | 79 |
| Kamienne kolce | 21921 | 0% | 86 | 255 |
| Przyzwanie chochlików | 4417 | 0% | 1 | 4417 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Kojące światło ×3.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka powietrza | 9691893 | 69% |  |  |
| Nawałnica | 1079368 | 8% | 1321 | 817 |
| Płonąca ziemia | 885445 | 6% | 603 | 1468 |
| Nova powietrza | 836474 | 6% | 1324 | 632 |
| Burza | 433005 | 3% | 240 | 1804 |
| Łańcuch potępionych | 207756 | 1% | 244 | 851 |
| Odskok widma | 164813 | 1% |  |  |
| Deszcz strzał | 164154 | 1% | 70 | 2345 |
| Wyrok burzy | 138814 | 1% | 31 | 4478 |
| Nova ognia | 116840 | 1% | 72 | 1623 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Krwawy pakt ×6.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,5, mediana 4,0, najwięcej 6 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 27% | 68,9 | 160 | 0,6 |
| klasa | 24% | 59,6 | 635 | 0,3 |
| sklep | 5% | 12,7 | 393 | 1,6 |
| przedmiot | 44% | 111,7 | 368 | 0,6 |

Oferty sklepu dusz: widziane 1098, kupione 11, odrzucone 0, za mało dusz 355.
Zamiany w slotach: 2,1 na podejście; najczęstsze: Ściana ognia → Nova powietrza ×17, Nova powietrza → Ściana ognia ×11, Pocisk arkanów → Ściana ognia ×2, Nova powietrza → Piekielna kula ×1, Nova powietrza → Łańcuchy potępionych ×1, Nova powietrza → Kojące światło ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Nova powietrza | przedmiot | 20 | 111,7 | 368 | 0,6 |
| Nawałnica | klasa | 20 | 59,6 | 635 | 0,3 |
| Ściana ognia | odblokowanie | 20 | 40,6 | 216 | 0,5 |
| Pocisk arkanów | odblokowanie | 20 | 28,3 | 79 | 0,7 |
| Kamienne kolce | sklep | 1 | 86,0 | 255 | 1,0 |
| Iskra | sklep | 1 | 50,0 | 80 | 2,0 |
| Krok wichru | sklep | 1 | 35,0 | 106 | 1,0 |
| Łza lodowca | sklep | 1 | 24,0 | 2312 | 2,0 |
| Łańcuchy potępionych | sklep | 1 | 18,0 | 231 | 2,0 |
| Tąpnięcie | sklep | 1 | 18,0 | 202 | 4,0 |
| Kula ognia | sklep | 1 | 17,0 | 128 | 0,9 |
| Kojące światło | sklep | 1 | 3,0 | – | 0,7 |
| Piekielna kula | sklep | 1 | 2,0 | 96 | 2,0 |
| Przyzwanie chochlików | sklep | 1 | 1,0 | 4417 | 2,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,9, mediana 5,5, najwięcej 12 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 23% | 49,8 | 1368 | 0,6 |
| klasa | 31% | 66,1 | 817 | 0,2 |
| sklep | 14% | 30,2 | 1438 | 2,0 |
| przedmiot | 31% | 66,2 | 632 | 0,6 |

Oferty sklepu dusz: widziane 1569, kupione 37, odrzucone 6, za mało dusz 480.
Zamiany w slotach: 4,3 na podejście; najczęstsze: Pocisk arkanów → Płonąca ziemia ×17, Kojące światło → Burza ×17, Płonąca ziemia → Salwa ×2, Burza → Wyrok burzy ×2, Płonąca ziemia → Łańcuch potępionych ×2, Burza → Strzała rozszczepu ×2.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Nova powietrza | przedmiot | 20 | 66,2 | 632 | 0,6 |
| Nawałnica | klasa | 20 | 66,1 | 817 | 0,2 |
| Płonąca ziemia | odblokowanie | 19 | 31,7 | 1468 | 0,6 |
| Łańcuch potępionych | sklep | 3 | 81,3 | 851 | 2,0 |
| Burza | odblokowanie | 18 | 13,3 | 1804 | 0,5 |
| Łańcuch błyskawic | odblokowanie | 1 | 84,0 | 411 | 1,0 |
| Nova ognia | sklep | 3 | 24,0 | 1623 | 1,6 |
| Deszcz strzał | sklep | 4 | 17,5 | 2345 | 2,0 |
| Przyzwanie chochlików | sklep | 1 | 41,0 | 1573 | 2,0 |
| Kościany golem | sklep | 2 | 17,0 | 3369 | 2,0 |
| Wyrok burzy | sklep | 2 | 15,5 | 4478 | 2,0 |
| Ściana ognia | odblokowanie | 1 | 23,0 | 184 | 0,4 |
| Kamienne kolce | sklep | 1 | 17,0 | 383 | 1,0 |
| Fala przypływu | sklep | 1 | 17,0 | 688 | 2,0 |
| Fala mocy | sklep | 1 | 14,0 | 372 | 4,0 |
| Pocisk arkanów | odblokowanie | 1 | 10,0 | 76 | 0,8 |
| Strzała rozszczepu | sklep | 1 | 9,0 | 690 | 2,0 |
| Mroźna fala | odblokowanie | 1 | 8,0 | 68 | 0,0 |
| Oko cyklonu | sklep | 1 | 8,0 | 1375 | 2,0 |
| Lodowa włócznia | sklep | 1 | 7,0 | 320 | 4,0 |
| Krwawy pakt | odblokowanie | 1 | 6,0 | – | 0,0 |
| Włócznia potępionych | odblokowanie | 1 | 6,0 | 152 | 0,3 |
| Meteor | sklep | 1 | 6,0 | 630 | 4,0 |
| Lodowy podmuch | odblokowanie | 1 | 5,0 | 207 | 0,0 |
| Rozprysk arkanów | odblokowanie | 1 | 5,0 | 59 | 0,0 |
| Kula ognia | odblokowanie | 1 | 5,0 | 143 | 0,0 |
| Deszcz siarki | sklep | 1 | 5,0 | 902 | 2,0 |
| Strzał przeszywający | sklep | 1 | 5,0 | 390 | 0,4 |
| Tąpnięcie | sklep | 1 | 5,0 | 111 | 2,0 |
| Krąg żaru | sklep | 1 | 5,0 | 467 | 2,0 |
| Nova wody | sklep | 1 | 4,0 | 50 | 1,0 |
| Ognista strzała | sklep | 1 | 3,0 | 303 | 4,0 |
| Mroźna strzała | sklep | 1 | 3,0 | 140 | 2,0 |
| Strzała siarkowa | sklep | 1 | 2,0 | 1253 | 2,0 |
| Salwa | sklep | 1 | 1,0 | 229 | 2,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 1568,1, umiejętność 252,8, unik 190,3, drgnięcie (trafiony) 95,2, czar obszarowy na jednego (test) 51,5, czar obszarowy na jedynego wroga 38,5, flaszka (próba) 25,0, czar obszarowy na grupę 22,0, wabienie bez skutku 21,0, czar obszarowy na jednego (ratunek) 20,0, flaszka many (próba) 12,7, czar obszarowy po zwabieniu 1,6.
Czas w bloku 0 s, w biegu 75 s, bez wytrzymałości (<10%) 126 s na podejście.
Zakupy: ulepszenie: Nova powietrza ×34, dodatkowa flaszka ×29, sklep: Różdżka powietrza: poziom 3 ×20, sklep: Różdżka powietrza: poziom 4 ×20, ulepszenie: Pocisk arkanów ×18, ulepszenie: Ściana ognia ×14, sklep: Nurt ×13, ulepszenie: Nawałnica ×12.
Punkty cech: Intelligence 102, Toughness 91, Dexterity 88, Strength 85.
Pieczęcie na podejście: z daru bossa 5,8, z sakwy do skilli 4,9, kupione 0,2; najczęstsze: Oko Czarnoboga ×20, Deszcz piekieł ×14, Klątwa Mokoszy ×10, Serce otchłani ×10, Pakt z diabłem ×10, Nieskończoność ×8.
Komunikaty gry: „Za mało zdrowia” ×325; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×322; „Portale gasną – dobij pozostałe demony” ×259; „Garda wroga przełamana!” ×166; „OGŁUSZENIE!” ×125; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×121; „POLEGŁEŚ” ×81; „ODKRYCIE: więź Więź: Ognisty wicher” ×48.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 1514,6, unik 248,6, umiejętność 212,2, drgnięcie (trafiony) 75,2, czar obszarowy na jedynego wroga 49,8, wabienie bez skutku 38,9, flaszka (próba) 20,5, czar obszarowy na grupę 18,9, flaszka many (próba) 10,6, czar obszarowy po zwabieniu 2,1, czar obszarowy na jednego (ratunek) 1,8.
Czas w bloku 0 s, w biegu 107 s, bez wytrzymałości (<10%) 100 s na podejście.
Zakupy: sklep: Twarda skóra ×41, ulepszenie: Nova powietrza ×39, sklep: Szybkie ręce ×38, dodatkowa flaszka ×37, sklep: Wicher ×37, sklep: Przeszycie ×35, sklep: Furia ×35, sklep: Skupienie ×31.
Punkty cech: Intelligence 359, Toughness 164.
Pieczęcie na podejście: z daru bossa 8,2, z sakwy do skilli 7,5, kupione 0,4; najczęstsze: Pakt z diabłem ×17, Oko Czarnoboga ×16, Nieskończoność ×15, Klątwa Mokoszy ×15, Wieczny płomień ×14, Serce otchłani ×12.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Kontra, Rozprysk arkanów, Mroźna fala, Krwawy pakt, Mroźna zbroja, Łańcuch błyskawic, Włócznia potępionych, Płonąca ziemia, Burza, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Szata popiołu, Kosa, Wielki topór, Młot burzy, Kojące światło, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×396; „Portale gasną – dobij pozostałe demony” ×362; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×179; „Garda wroga przełamana!” ×168; „OGŁUSZENIE!” ×118; „POLEGŁEŚ” ×43; „Tytan Otchłani: kolejna faza” ×42; „Kupiono: Twarda skóra” ×41.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×1077, uśpiony wróg blokuje koniec piętra ×155, zablokowany ruch ×6, limit czasu piętra ×1.
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 12 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×1442, uśpiony wróg blokuje koniec piętra ×309, zablokowany ruch ×12.
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 15 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 13 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 12 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** 1 podejść przerwanych limitem czasu piętra – możliwy nieosiągalny wróg albo zbyt długie piętro (zob. anomalie).
- Pojedyncze zatrzymania na geometrii (6 razy) – do przejrzenia, nie blokują gry.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 37% ciosów (n=63).
- Unik przed ciężkim: działa – rani tylko 19% ciosów (n=108).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=1358) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 8% pocisków, przed którymi był unik (n=1264).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 41% decyzji zapada, gdy do trafienia zostało <0,1 s (n=3104) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=499).
- Telegraf „nieparowalny” daje czas: tylko 10% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1405).
- Telegraf „nieblokowalny” daje czas: tylko 11% decyzji zapada mniej niż 0,1 s przed trafieniem (n=46).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=2211).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- UX: gra odmawiała akcji: „Za mało zdrowia” ×325 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 163 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 165 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 165 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 175 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 181 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 179 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 180 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVI. Kuźnia Piekieł” trwa średnio 188 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVIII. Tron Arcydemona” trwa średnio 176 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIX. Tron Arcydemona” trwa średnio 199 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,7 ms/klatkę (maks. 155 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 15/20 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Czuję progres: druga połowa podejść średnio piętro 29,5 wobec 26,3 na początku (popiół, odblokowania, wybór zestawu).
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Arcydiabeł (539,2 trafień, 78,0 s), Bezimienny Cień (355,6 trafień, 50,1 s), Kowal Piekieł (309,3 trafień, 62,3 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Ożywiony nietoperz (1,6), Pajączek (1,6), Kamienny chochlik (2,0) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Matka Burz – średnio 57 s i 300 trafień do zabicia.
- Długie starcie: Król Kości – średnio 42 s i 239 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 50 s i 356 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 62 s i 309 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 78 s i 539 trafień do zabicia.
- Mój nemezis: Miotacz głazów – 4 z 5 śmierci (najczęściej „Miotacz głazów: Rockfall”).
- 5/5 razy zginąłem z flaszkami w kieszeni. 2 z tych śmierci to utrata ≥50% HP w 3 s, a 73% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Za dużo ciosów zza pleców (20% trafień). Przy namierzaniu nie widzę reszty grupy – w souls to oznaka złego designu walki z grupą.
- Skille robią 30% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Wyrok burzy”: 4478 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 14%. Różnych skilli na podejście: średnio 5,9.
- Unik to moje główne narzędzie: 248,6 na podejście, w tym 25,9 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 95% przypadków.
- Gra nie wykonała uniku od razu średnio 78,6× na podejście – najczęściej: w trakcie poprzedniego uniku 51,6, drgnięcie (trafiony) 12,5, brak wytrzymałości 10,8. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (75 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

