# Raport botów grających — Mag ognia

Wygenerowano: 2026-10-29 12:27. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (10 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (10 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 10 | 5 (50%) | 26,7 | 30,0 | 50,0 min | 1353,0 | 748797 / 4188 | 5,1 | 11% |
| Gracz H&S / souls-like | 10 | 8 (80%) | 27,5 | 30,0 | 51,5 min | 1403,6 | 734518 / 3724 | 5,3 | 17% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 14 26 ★ ★ ★ 30 ★ ★ 20 27
- Gracz H&S / souls-like: 19 ★ ★ ★ ★ ★ ★ ★ 16 ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 14. XIV. Kamienne Trzewia ×1, 20. XX. Kostnica ×1, 26. XXVI. Kuźnia Piekieł ×1, 27. XXVII. Kowal Piekieł ×1, 30. XXX. Arcydiabeł ×1.
  Ostatni cios: Grzebacz – „Grzebacz” ×1; Miotacz głazów – „Miotacz głazów” ×1; Arcydiabeł – „Arcydiabeł: Rockfall” ×1; Grzebacz – „Grzebacz: StoneSpikes” ×1; Kowal Piekieł – „Kowal Piekieł: Quake” ×1. Śmierć ciosem ≥40% maks. HP: 0/5. Zginął mając flaszki: 5/5.
  Ostatnie 3 s przed śmiercią: średnio 39% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/5). Picie: 137 prób, 51 wypitych (63% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 16. XVI. Wieża Burz ×1, 19. XIX. Kostnica ×1.
  Ostatni cios: Harpia – „Harpia: WindStep” ×1; Gnilec (odłamek) – „Gnilec” ×1. Śmierć ciosem ≥40% maks. HP: 0/2. Zginął mając flaszki: 2/2.
  Ostatnie 3 s przed śmiercią: średnio 54% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/2). Picie: 135 prób, 53 wypitych (61% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 10 | 10 | 106 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 10 | 10 | 112 s | 0 | 0,0 | 40,3 |
| Tester QA | 3. III. Władca Bramy | 10 | 10 | 17 s | 9 | 0,0 | 7,0 |
| Tester QA | 4. IV. Psiarnia Piekieł | 10 | 10 | 112 s | 0 | 0,0 | 47,8 |
| Tester QA | 5. V. Psiarnia Piekieł | 10 | 10 | 90 s | 0 | 0,0 | 37,5 |
| Tester QA | 6. VI. Pan Sfory | 10 | 10 | 30 s | 7 | 0,0 | 4,6 |
| Tester QA | 7. VII. Zatopione Krużganki | 10 | 10 | 94 s | 9 | 0,0 | 40,1 |
| Tester QA | 8. VIII. Zatopione Krużganki | 10 | 10 | 118 s | 34 | 0,0 | 52,9 |
| Tester QA | 9. IX. Królowa Topieli | 10 | 10 | 29 s | 132 | 0,3 | 2,8 |
| Tester QA | 10. X. Rzeźnia | 10 | 10 | 102 s | 44 | 0,0 | 58,6 |
| Tester QA | 11. XI. Rzeźnia | 10 | 10 | 123 s | 37 | 0,0 | 60,6 |
| Tester QA | 12. XII. Rzeźnik | 10 | 10 | 31 s | 8 | 0,0 | 5,2 |
| Tester QA | 13. XIII. Kamienne Trzewia | 10 | 10 | 131 s | 168 | 0,4 | 69,1 |
| Tester QA | 14. XIV. Kamienne Trzewia | 12 | 9 | 138 s | 391 | 0,7 | 61,5 |
| Tester QA | 15. XV. Tytan Otchłani | 9 | 9 | 36 s | 120 | 0,2 | 1,9 |
| Tester QA | 16. XVI. Wieża Burz | 11 | 9 | 154 s | 474 | 0,6 | 74,9 |
| Tester QA | 17. XVII. Wieża Burz | 9 | 9 | 162 s | 142 | 0,1 | 86,3 |
| Tester QA | 18. XVIII. Matka Burz | 9 | 9 | 47 s | 130 | 0,0 | 2,6 |
| Tester QA | 19. XIX. Kostnica | 9 | 9 | 164 s | 116 | 0,1 | 89,0 |
| Tester QA | 20. XX. Kostnica | 11 | 8 | 170 s | 241 | 0,2 | 80,6 |
| Tester QA | 21. XXI. Król Kości | 8 | 8 | 37 s | 179 | 0,1 | 10,3 |
| Tester QA | 22. XXII. Chór Cieni | 8 | 8 | 174 s | 71 | 0,3 | 105,6 |
| Tester QA | 23. XXIII. Chór Cieni | 8 | 8 | 175 s | 32 | 0,0 | 109,0 |
| Tester QA | 24. XXIV. Bezimienny Cień | 8 | 8 | 49 s | 11 | 0,0 | 2,9 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 9 | 8 | 177 s | 111 | 0,1 | 111,1 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 11 | 7 | 193 s | 539 | 0,9 | 109,3 |
| Tester QA | 27. XXVII. Kowal Piekieł | 11 | 6 | 69 s | 475 | 0,3 | 1,3 |
| Tester QA | 28. XXVIII. Tron Arcydemona | 7 | 6 | 191 s | 150 | 0,0 | 115,7 |
| Tester QA | 29. XXIX. Tron Arcydemona | 6 | 6 | 183 s | 13 | 0,0 | 123,5 |
| Tester QA | 30. XXX. Arcydiabeł | 8 | 5 | 74 s | 584 | 0,8 | 4,6 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 10 | 10 | 100 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 10 | 10 | 121 s | 0 | 0,0 | 39,7 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 10 | 10 | 12 s | 0 | 0,0 | 7,0 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 10 | 10 | 115 s | 0 | 0,0 | 46,2 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 10 | 10 | 101 s | 3 | 0,0 | 42,3 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 10 | 10 | 20 s | 0 | 0,0 | 4,0 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 10 | 10 | 97 s | 0 | 0,0 | 42,9 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 10 | 10 | 133 s | 41 | 0,1 | 56,3 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 10 | 10 | 23 s | 23 | 0,0 | 2,4 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 10 | 10 | 107 s | 77 | 0,3 | 60,6 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 11 | 10 | 128 s | 133 | 0,3 | 56,6 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 10 | 10 | 22 s | 0 | 0,0 | 5,2 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 13 | 10 | 130 s | 246 | 0,5 | 58,7 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 12 | 10 | 136 s | 165 | 0,2 | 63,3 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 10 | 10 | 30 s | 116 | 0,2 | 2,2 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 13 | 9 | 156 s | 315 | 0,4 | 68,3 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 10 | 9 | 155 s | 111 | 0,2 | 82,7 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 9 | 9 | 39 s | 96 | 0,2 | 2,6 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 11 | 8 | 158 s | 295 | 0,2 | 72,0 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 9 | 8 | 165 s | 132 | 0,0 | 88,0 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 8 | 8 | 35 s | 76 | 0,3 | 10,3 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 10 | 8 | 176 s | 276 | 0,3 | 96,6 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 8 | 8 | 172 s | 88 | 0,0 | 103,9 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 8 | 8 | 46 s | 0 | 0,1 | 2,9 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 8 | 8 | 186 s | 92 | 0,1 | 111,6 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 10 | 8 | 186 s | 443 | 0,7 | 107,6 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 10 | 8 | 59 s | 370 | 0,9 | 1,7 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 9 | 8 | 194 s | 311 | 0,2 | 122,2 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 9 | 8 | 184 s | 91 | 0,0 | 120,0 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 8 | 8 | 51 s | 113 | 0,0 | 7,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 14 ×2, piętro 16 ×2, piętro 20 ×2, piętro 25 ×1, piętro 26 ×3, piętro 27 ×4, piętro 28 ×1, piętro 30 ×2; wyzwań (3 porażki na piętrze): 3 – piętro 14, piętro 26, piętro 30.
**Gracz H&S / souls-like** – ponowne próby: piętro 11 ×1, piętro 13 ×3, piętro 14 ×2, piętro 16 ×3, piętro 17 ×1, piętro 19 ×2, piętro 20 ×1, piętro 22 ×2, piętro 26 ×2, piętro 27 ×2, piętro 28 ×1, piętro 29 ×1; wyzwań (3 porażki na piętrze): 1 – piętro 19.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 838 | 1,7 s | 3,1 | 1683 | 24174 | 28 | 1 |
| Kowal Piekieł | 14 | 58,7 s | 252,6 | 415 | 8925 | 638 | 1 |
| Arcydiabeł | 13 | 54,8 s | 323,9 | 333 | 5577 | 429 | 1 |
| Grzebacz | 712 | 1,6 s | 4,0 | 594 | 5414 | 8 | 2 |
| Kultysta topieli | 1458 | 0,4 s | 1,5 | 717 | 5115 | 3 | 0 |
| Wiedźma wichru | 1263 | 0,8 s | 2,1 | 1643 | 4166 | 3 | 0 |
| Harpia | 1021 | 0,6 s | 2,4 | 1219 | 3076 | 3 | 1 |
| Ogar otchłani | 1818 | 0,5 s | 2,1 | 576 | 2658 | 1 | 0 |
| Tytan Otchłani | 19 | 28,4 s | 96,0 | 108 | 2211 | 116 | 0 |
| Matka Burz | 18 | 39,0 s | 114,9 | 279 | 1998 | 111 | 0 |
| Król Kości | 16 | 31,9 s | 115,9 | 138 | 1986 | 124 | 0 |
| Taranowiec | 1459 | 1,0 s | 3,3 | 154 | 1920 | 1 | 0 |
| Upiór | 579 | 1,5 s | 3,3 | 607 | 1459 | 2 | 0 |
| Królowa Topieli | 20 | 21,8 s | 57,7 | 145 | 1269 | 63 | 0 |
| Nekromanta | 428 | 2,0 s | 3,8 | 115 | 1160 | 3 | 0 |
| Cień | 968 | 0,6 s | 2,4 | 217 | 1160 | 1 | 0 |
| Legionista | 1205 | 3,3 s | 8,2 | 135 | 1024 | 1 | 0 |
| Kolos żaru | 884 | 2,9 s | 8,9 | 43 | 948 | 1 | 0 |
| Kolos | 1316 | 2,0 s | 4,6 | 19 | 864 | 1 | 0 |
| Kultysta | 1668 | 0,6 s | 1,9 | 212 | 701 | 0 | 0 |
| Siarkowy pomiot | 1341 | 0,5 s | 2,1 | 16 | 473 | 0 | 0 |
| Gnilec | 750 | 1,6 s | 4,7 | 59 | 413 | 1 | 0 |
| Gnilec (odłamek) | 1545 | 0,7 s | 2,4 | 61 | 409 | 0 | 1 |
| Tkaczka | 448 | 1,5 s | 3,0 | 263 | 403 | 1 | 0 |
| Czerw | 4251 | 0,1 s | 1,2 | 92 | 390 | 0 | 0 |
| Ożywiony nietoperz | 823 | 0,1 s | 1,1 | 51 | 366 | 0 | 0 |
| Ofiarnik | 470 | 1,8 s | 3,2 | 256 | 168 | 0 | 0 |
| Pajączek | 194 | 0,0 s | 1,1 | 57 | 134 | 1 | 0 |
| Władca Bramy | 20 | 9,9 s | 24,4 | 81 | 90 | 4 | 0 |
| Gargulec | 520 | 1,5 s | 4,7 | 27 | 87 | 0 | 0 |
| Bezimienny Cień | 16 | 43,9 s | 172,4 | 135 | 85 | 5 | 0 |
| Rzeźnik | 20 | 22,4 s | 71,3 | 35 | 81 | 4 | 0 |
| Wodny chochlik | 501 | 0,3 s | 1,6 | 8 | 71 | 0 | 0 |
| Pan Sfory | 20 | 20,7 s | 39,9 | 132 | 42 | 2 | 0 |
| Kamienny chochlik | 502 | 0,4 s | 1,9 | 6 | 41 | 0 | 0 |
| Chochlik wichru | 536 | 0,5 s | 1,9 | 7 | 40 | 0 | 0 |
| Ognisty chochlik | 594 | 1,6 s | 3,8 | 12 | 18 | 0 | 0 |
| Ogar otchłani (elita) | 55 | 2,4 s | 6,1 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 42 | 3,7 s | 9,7 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 46 | 2,2 s | 6,4 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 50 | 4,7 s | 10,2 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 44 | 6,4 s | 16,3 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 51 | 3,4 s | 6,4 | ↑ | ↑ | ↑ |  |
| Worek jaj | 178 | 0,4 s | 1,7 | 0 | 0 | 0 | 0 |
| Gnilec (elita) | 26 | 4,2 s | 17,2 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 28 | 1,2 s | 2,7 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 38 | 2,5 s | 6,9 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 54 | 4,0 s | 7,3 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 53 | 2,6 s | 4,3 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 40 | 8,0 s | 19,4 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 26 | 2,0 s | 7,3 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 13 | 3,4 s | 11,6 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 8 | 5,5 s | 13,3 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 22 | 9,3 s | 31,3 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 31 | 5,1 s | 11,9 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 10 | 5,8 s | 9,0 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 12 | 6,7 s | 14,1 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 15 | 4,9 s | 14,9 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 8 | 2,7 s | 11,3 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1397 | 48% | 0% | 0% | 52% | 0% |
| ciężki | 88 | 35% | 0% | 0% | 65% | 0% |
| nieparowalny | 1165 | 40% | 0% | 0% | 60% | 0% |
| nieblokowalny | 99 | 75% | 0% | 0% | 25% | 0% |
| obszarowy | 2478 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1624 | 32% | 0% | 0% | 68% | 0% |
| ciężki | 110 | 18% | 0% | 0% | 82% | 0% |
| nieparowalny | 1183 | 24% | 0% | 0% | 76% | 0% |
| nieblokowalny | 91 | 57% | 0% | 0% | 43% | 0% |
| obszarowy | 2415 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1524 | 25% | 13% | 3% | 15% | 44% |
| Tester QA (0,30 s) | ciężki | 239 | 1% | 2% | 8% | 24% | 66% |
| Tester QA (0,30 s) | nieparowalny | 681 | 6% | 2% | 6% | 33% | 54% |
| Tester QA (0,30 s) | nieblokowalny | 17 | 0% | 0% | 6% | 35% | 59% |
| Tester QA (0,30 s) | obszarowy | 1223 | 1% | 1% | 5% | 43% | 50% |
| Tester QA (0,30 s) | pocisk | 1371 | 3% | 8% | 6% | 13% | 72% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2026 | 14% | 10% | 13% | 8% | 54% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 265 | 1% | 2% | 3% | 27% | 68% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 738 | 7% | 1% | 2% | 11% | 78% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 17 | 0% | 0% | 0% | 0% | 100% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 1383 | 2% | 1% | 7% | 35% | 55% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 1720 | 2% | 7% | 6% | 10% | 75% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 215 |
| Pajączek: Pajączek | 0,31 s | 136 |
| Ognisty chochlik: Pazury | 0,33 s | 65 |
| Chochlik wichru: Pazury | 0,33 s | 39 |
| Wodny chochlik: Pazury | 0,33 s | 43 |
| Czerw: Ugryzienie | 0,33 s | 557 |
| Kamienny chochlik: Pazury | 0,33 s | 35 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 208 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 837 |
| Królowa Topieli: Królowa Topieli: WindStep | 0,45 s | 10 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 79% (n=84) | 100% (n=12) | 78% (n=76) | 86% (n=7) | 100% (n=80) | 80% (n=40) |
| niezauważony | 87% (n=182) | – | 86% (n=85) | 97% (n=38) | 100% (n=1410) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 95% (n=204) |
| unik | 33% (n=638) | 14% (n=42) | 22% (n=605) | 52% (n=42) | 100% (n=46) | 28% (n=153) |
| unik (eksperyment) | 32% (n=124) | 13% (n=15) | 17% (n=111) | 0% (n=3) | 100% (n=102) | 37% (n=30) |
| wyjście z zasięgu | 41% (n=66) | 73% (n=11) | 68% (n=28) | – | 100% (n=511) | 55% (n=66) |
| wyjście z zasięgu (eksperyment) | 85% (n=128) | 38% (n=8) | 89% (n=122) | 100% (n=9) | 100% (n=112) | 51% (n=37) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 192,8 | reakcja na cios wręcz 175,7, unik przed pociskiem 54,5, wabienie pod czar obszarowy 5,4, wyjście z okrążenia 3,3 | w trakcie uniku 19,9, drgnięcie (trafiony) 12,1, brak wytrzymałości 5,6, picie flaszki 2,2, unieruchomienie 0,4, inna 0,2 | 67% (n=1757) / 90% (n=545) |
| Gracz H&S / souls-like | 257,0 | reakcja na cios wręcz 240,5, unik przed pociskiem 81,1, doskok do odsłoniętego celu 19,5, wabienie pod czar obszarowy 9,3, wyjście z okrążenia 2,0 | w trakcie uniku 51,7, drgnięcie (trafiony) 15,2, brak wytrzymałości 11,1, picie flaszki 2,5, unieruchomienie 0,9, inna 0,1 | 75% (n=2405) / 95% (n=811) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Ogień | Nova ognia | 10 | 5 | 26,7 | 1353,0 |
| Gracz H&S / souls-like | Ogień | Nova ognia | 10 | 8 | 27,5 | 1403,6 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka ognia | 5534446 | 74% |  |  |
| Meteor | 1281467 | 17% | 640 | 2002 |
| Nova ognia | 401297 | 5% | 1005 | 399 |
| Ściana ognia | 120510 | 2% | 367 | 328 |
| Odskok widma | 97035 | 1% |  |  |
| Przyzwanie chochlików | 33983 | 0% | 55 | 618 |
| Pocisk arkanów | 12804 | 0% | 157 | 82 |
| Fala mocy | 2886 | 0% | 13 | 222 |
| Strzała rozszczepu | 1897 | 0% | 4 | 474 |
| Strzał przeszywający | 1652 | 0% | 2 | 826 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Kamienna osłona ×1, Iskra ×1.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka ognia | 4491101 | 61% |  |  |
| Meteor | 1523517 | 21% | 689 | 2211 |
| Płonąca ziemia | 573143 | 8% | 255 | 2248 |
| Nova ognia | 333277 | 5% | 643 | 518 |
| Fala przypływu | 193849 | 3% | 23 | 8428 |
| Odskok widma | 111281 | 2% |  |  |
| Przyzwanie chochlików | 28880 | 0% | 17 | 1699 |
| Sfora | 24189 | 0% | 19 | 1273 |
| Ściana ognia | 20948 | 0% | 27 | 776 |
| Łańcuch błyskawic | 10955 | 0% | 21 | 522 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Krwawy pakt ×6.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,6, mediana 5,0, najwięcej 5 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 23% | 52,4 | 254 | 0,8 |
| klasa | 29% | 64,0 | 2002 | 0,6 |
| sklep | 3% | 7,6 | 532 | 1,6 |
| przedmiot | 45% | 100,5 | 399 | 0,9 |

Oferty sklepu dusz: widziane 771, kupione 10, odrzucone 0, za mało dusz 247.
Zamiany w slotach: 4,8 na podejście; najczęstsze: Ściana ognia → Nova ognia ×20, Nova ognia → Ściana ognia ×19, Nova ognia → Strzał przeszywający ×1, Nova ognia → Kamienna osłona ×1, Nova ognia → Iskra ×1, Nova ognia → Przyzwanie chochlików ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Nova ognia | przedmiot | 10 | 100,5 | 399 | 0,9 |
| Meteor | klasa | 10 | 64,0 | 2002 | 0,6 |
| Ściana ognia | odblokowanie | 10 | 36,7 | 328 | 0,8 |
| Pocisk arkanów | odblokowanie | 10 | 15,7 | 82 | 0,9 |
| Przyzwanie chochlików | sklep | 1 | 55,0 | 618 | 1,0 |
| Fala mocy | sklep | 1 | 13,0 | 222 | 4,0 |
| Strzała rozszczepu | sklep | 1 | 4,0 | 474 | 2,0 |
| Strzał przeszywający | sklep | 1 | 2,0 | 826 | 2,0 |
| Kamienna osłona | sklep | 1 | 1,0 | – | 2,0 |
| Iskra | sklep | 1 | 1,0 | – | 4,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,1, mediana 4,5, najwięcej 12 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 27% | 48,3 | 1812 | 0,6 |
| klasa | 31% | 56,0 | 2263 | 0,4 |
| sklep | 6% | 10,6 | 2523 | 2,3 |
| przedmiot | 36% | 64,3 | 518 | 0,8 |

Oferty sklepu dusz: widziane 795, kupione 13, odrzucone 7, za mało dusz 245.
Zamiany w slotach: 4,2 na podejście; najczęstsze: Pocisk arkanów → Płonąca ziemia ×7, Kojące światło → Meteor ×7, Meteor → Ognista strzała ×2, Nova ognia → Lodowy podmuch ×1, Pocisk arkanów → Krwawy pakt ×1, Lodowy podmuch → Mroźna fala ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Meteor | klasa | 18 | 38,3 | 2211 | 0,5 |
| Nova ognia | przedmiot | 10 | 64,3 | 518 | 0,8 |
| Płonąca ziemia | odblokowanie | 9 | 28,3 | 2248 | 0,7 |
| Ściana ognia | odblokowanie | 2 | 13,5 | 776 | 0,4 |
| Fala przypływu | sklep | 1 | 23,0 | 8428 | 2,0 |
| Łańcuch błyskawic | odblokowanie | 1 | 21,0 | 522 | 0,0 |
| Sfora | sklep | 1 | 19,0 | 1273 | 2,0 |
| Przyzwanie chochlików | sklep | 1 | 17,0 | 1699 | 2,0 |
| Pocisk arkanów | odblokowanie | 1 | 12,0 | 107 | 0,9 |
| Kamienne kolce | sklep | 1 | 10,0 | 504 | 2,0 |
| Łańcuchy potępionych | sklep | 1 | 9,0 | 417 | 4,0 |
| Mroźna fala | odblokowanie | 1 | 7,0 | 64 | 0,6 |
| Fala mocy | sklep | 2 | 3,5 | 376 | 4,0 |
| Krwawy pakt | odblokowanie | 1 | 6,0 | – | 0,0 |
| Łańcuch potępionych | sklep | 1 | 6,0 | 239 | 2,0 |
| Burza | odblokowanie | 1 | 6,0 | 1456 | 0,8 |
| Nova wody | sklep | 1 | 6,0 | 595 | 2,0 |
| Lodowy podmuch | odblokowanie | 1 | 5,0 | 117 | 0,0 |
| Rozprysk arkanów | odblokowanie | 1 | 5,0 | 141 | 0,0 |
| Kula ognia | odblokowanie | 1 | 5,0 | 217 | 1,0 |
| Włócznia potępionych | odblokowanie | 1 | 5,0 | 134 | 1,6 |
| Nova powietrza | sklep | 1 | 5,0 | 216 | 2,0 |
| Strzała siarkowa | sklep | 1 | 4,0 | 759 | 1,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 1831,3, umiejętność 224,5, unik 192,8, drgnięcie (trafiony) 88,0, czar obszarowy na jednego (test) 47,1, czar obszarowy na jedynego wroga 39,3, wabienie bez skutku 27,1, flaszka (próba) 13,7, czar obszarowy na grupę 12,5, flaszka many (próba) 3,4, czar obszarowy na jednego (ratunek) 2,3, czar obszarowy po zwabieniu 0,9.
Czas w bloku 0 s, w biegu 97 s, bez wytrzymałości (<10%) 95 s na podejście.
Zakupy: dodatkowa flaszka ×20, ulepszenie: Nova ognia ×14, sklep: Skała ×11, sklep: Różdżka ognia: poziom 3 ×10, sklep: Różdżka ognia: poziom 4 ×10, ulepszenie: Pocisk arkanów ×10, ulepszenie: Ściana ognia ×10, ulepszenie: Meteor ×10.
Punkty cech: Intelligence 71, Toughness 63, Dexterity 62, Strength 61.
Pieczęcie na podejście: z daru bossa 8,0, z sakwy do skilli 5,1, kupione 0,7; najczęstsze: Pakt z diabłem ×8, Klątwa Mokoszy ×8, Wrota piekieł ×8, Oko Czarnoboga ×7, Serce otchłani ×7, Nieskończoność ×7.
Komunikaty gry: „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×191; „Portale gasną – dobij pozostałe demony” ×177; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×85; „OGŁUSZENIE!” ×53; „Garda wroga przełamana!” ×45; „POLEGŁEŚ” ×22; „Kowal Piekieł: kolejna faza” ×22; „Królowa Topieli: kolejna faza” ×20.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 1620,9, unik 257,0, umiejętność 179,2, drgnięcie (trafiony) 70,1, wabienie bez skutku 43,8, czar obszarowy na jedynego wroga 43,4, czar obszarowy na grupę 19,7, flaszka (próba) 13,5, flaszka many (próba) 4,3, czar obszarowy po zwabieniu 2,4, czar obszarowy na jednego (ratunek) 2,4.
Czas w bloku 0 s, w biegu 99 s, bez wytrzymałości (<10%) 101 s na podejście.
Zakupy: sklep: Szybkie ręce ×22, sklep: Furia ×21, dodatkowa flaszka ×20, ulepszenie: Nova ognia ×17, sklep: Szeroki zasięg ×17, sklep: Żar ×17, sklep: Skupienie ×16, sklep: Przepalenie ×16.
Punkty cech: Intelligence 182, Toughness 83.
Pieczęcie na podejście: z daru bossa 8,3, z sakwy do skilli 6,0, kupione 0,6; najczęstsze: Nieskończoność ×14, Wrota piekieł ×10, Supernowa ×8, Pakt z diabłem ×8, Wieczny płomień ×8, Oko Czarnoboga ×7.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Kontra, Rozprysk arkanów, Mroźna fala, Krwawy pakt, Mroźna zbroja, Łańcuch błyskawic, Płonąca ziemia, Puklerz, Sztylet parujący, Włócznia potępionych, Burza, Meteor, Łuk refleksyjny, Kostur burzy, Pawęż, Szata popiołu, Kosa, Wielki topór, Młot burzy, Kojące światło, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×203; „Portale gasną – dobij pozostałe demony” ×183; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×91; „Garda wroga przełamana!” ×78; „OGŁUSZENIE!” ×66; „POLEGŁEŚ” ×23; „Kupiono: Szybkie ręce” ×22; „Kupiono: Furia” ×21.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×543, uśpiony wróg blokuje koniec piętra ×153, zablokowany ruch ×5.
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×609, uśpiony wróg blokuje koniec piętra ×143, zablokowany ruch ×5.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 10 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (5 razy) – do przejrzenia, nie blokują gry.
- Unik przed czerwonym (nieblokowalnym): 49% ciosów mimo to rani (n=45) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 14% ciosów (n=57).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=623) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 10% pocisków, przed którymi był unik (n=545).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 38% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1524) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=239).
- Telegraf „nieparowalny” daje czas: tylko 8% decyzji zapada mniej niż 0,1 s przed trafieniem (n=681).
- Telegraf „nieblokowalny” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=17).
- Telegraf „obszarowy” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1223).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 154 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 162 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 164 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 170 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 174 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 175 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 177 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVI. Kuźnia Piekieł” trwa średnio 193 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVIII. Tron Arcydemona” trwa średnio 191 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIX. Tron Arcydemona” trwa średnio 183 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,9 ms/klatkę (maks. 496 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 8/10 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Progres między podejściami słabo odczuwalny: 28,6 → 28,0 średnio. Meta-progresja niewiele zmienia w walce.
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Tytan Otchłani (89,9 trafień, 25,6 s), Rzeźnik (64,3 trafień, 17,4 s), Królowa Topieli (56,8 trafień, 18,9 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Ożywiony nietoperz (1,1), Pajączek (1,1), Czerw (1,2) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Bezimienny Cień – średnio 42 s i 191 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 54 s i 256 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 46 s i 292 trafień do zabicia.
- 2/2 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 61% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 37% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Fala przypływu”: 8428 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 6%. Różnych skilli na podejście: średnio 5,1.
- Unik to moje główne narzędzie: 257,0 na podejście, w tym 21,5 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 95% przypadków.
- Gra nie wykonała uniku od razu średnio 81,5× na podejście – najczęściej: w trakcie poprzedniego uniku 51,7, drgnięcie (trafiony) 15,2, brak wytrzymałości 11,1. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (70 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

