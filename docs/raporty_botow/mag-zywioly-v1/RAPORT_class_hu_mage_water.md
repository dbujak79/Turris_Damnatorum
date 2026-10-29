# Raport botów grających — Mag wody

Wygenerowano: 2026-10-29 12:31. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (10 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (10 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 10 | 4 (40%) | 22,4 | 24,0 | 48,3 min | 1066,5 | 524413 / 5754 | 11,2 | 5% |
| Gracz H&S / souls-like | 10 | 8 (80%) | 28,5 | 30,0 | 56,2 min | 1477,7 | 803154 / 3424 | 2,8 | 18% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 28 ★ 14 20 11 17 ★ 14 ★ ★
- Gracz H&S / souls-like: ★ ★ ★ ★ ★ ★ ★ 29 16 ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 11. XI. Rzeźnia ×1, 14. XIV. Kamienne Trzewia ×2, 17. XVII. Wieża Burz ×1, 20. XX. Kostnica ×1, 28. XXVIII. Tron Arcydemona ×1.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×2; Harpia – „Harpia: WindStep” ×1; Ogar otchłani – „Ogar otchłani” ×1; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×1; Kultysta topieli – „Kultysta topieli” ×1. Śmierć ciosem ≥40% maks. HP: 0/6. Zginął mając flaszki: 6/6.
  Ostatnie 3 s przed śmiercią: średnio 56% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/6). Picie: 287 prób, 112 wypitych (61% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 16. XVI. Wieża Burz ×1, 29. XXIX. Tron Arcydemona ×1.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×1; Harpia – „Harpia” ×1. Śmierć ciosem ≥40% maks. HP: 0/2. Zginął mając flaszki: 2/2.
  Ostatnie 3 s przed śmiercią: średnio 48% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/2). Picie: 155 prób, 28 wypitych (82% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 10 | 10 | 102 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 10 | 10 | 112 s | 0 | 0,0 | 41,8 |
| Tester QA | 3. III. Władca Bramy | 10 | 10 | 21 s | 34 | 0,1 | 7,3 |
| Tester QA | 4. IV. Psiarnia Piekieł | 10 | 10 | 116 s | 0 | 0,0 | 46,7 |
| Tester QA | 5. V. Psiarnia Piekieł | 10 | 10 | 93 s | 3 | 0,0 | 40,3 |
| Tester QA | 6. VI. Pan Sfory | 10 | 10 | 30 s | 11 | 0,0 | 6,0 |
| Tester QA | 7. VII. Zatopione Krużganki | 10 | 10 | 100 s | 18 | 0,0 | 39,3 |
| Tester QA | 8. VIII. Zatopione Krużganki | 10 | 10 | 120 s | 19 | 0,0 | 51,0 |
| Tester QA | 9. IX. Królowa Topieli | 10 | 10 | 96 s | 125 | 0,8 | 2,6 |
| Tester QA | 10. X. Rzeźnia | 11 | 10 | 118 s | 94 | 0,5 | 56,9 |
| Tester QA | 11. XI. Rzeźnia | 20 | 9 | 143 s | 422 | 0,7 | 46,3 |
| Tester QA | 12. XII. Rzeźnik | 9 | 9 | 45 s | 25 | 0,3 | 5,0 |
| Tester QA | 13. XIII. Kamienne Trzewia | 10 | 9 | 143 s | 272 | 1,1 | 68,3 |
| Tester QA | 14. XIV. Kamienne Trzewia | 17 | 7 | 152 s | 550 | 1,2 | 56,8 |
| Tester QA | 15. XV. Tytan Otchłani | 10 | 7 | 59 s | 269 | 0,6 | 1,5 |
| Tester QA | 16. XVI. Wieża Burz | 12 | 7 | 150 s | 457 | 0,6 | 67,0 |
| Tester QA | 17. XVII. Wieża Burz | 11 | 6 | 154 s | 484 | 0,5 | 63,4 |
| Tester QA | 18. XVIII. Matka Burz | 6 | 6 | 60 s | 0 | 0,2 | 1,7 |
| Tester QA | 19. XIX. Kostnica | 7 | 6 | 157 s | 94 | 0,7 | 85,6 |
| Tester QA | 20. XX. Kostnica | 10 | 5 | 187 s | 554 | 1,5 | 78,4 |
| Tester QA | 21. XXI. Król Kości | 5 | 5 | 68 s | 42 | 0,0 | 5,0 |
| Tester QA | 22. XXII. Chór Cieni | 6 | 5 | 184 s | 372 | 0,0 | 100,5 |
| Tester QA | 23. XXIII. Chór Cieni | 6 | 5 | 174 s | 335 | 0,2 | 96,2 |
| Tester QA | 24. XXIV. Bezimienny Cień | 5 | 5 | 84 s | 0 | 0,0 | 2,8 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 5 | 5 | 184 s | 77 | 0,0 | 113,2 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 7 | 5 | 194 s | 405 | 0,1 | 118,7 |
| Tester QA | 27. XXVII. Kowal Piekieł | 5 | 5 | 56 s | 228 | 0,4 | 1,4 |
| Tester QA | 28. XXVIII. Tron Arcydemona | 7 | 4 | 193 s | 559 | 0,1 | 110,0 |
| Tester QA | 29. XXIX. Tron Arcydemona | 4 | 4 | 183 s | 25 | 0,0 | 124,8 |
| Tester QA | 30. XXX. Arcydiabeł | 4 | 4 | 101 s | 287 | 1,0 | 5,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 10 | 10 | 100 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 10 | 10 | 117 s | 0 | 0,0 | 41,2 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 10 | 10 | 12 s | 23 | 0,1 | 5,8 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 10 | 10 | 106 s | 0 | 0,0 | 46,9 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 10 | 10 | 88 s | 0 | 0,0 | 39,3 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 10 | 10 | 14 s | 13 | 0,0 | 3,8 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 10 | 10 | 105 s | 2 | 0,0 | 41,0 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 10 | 10 | 132 s | 60 | 0,0 | 50,9 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 10 | 10 | 53 s | 20 | 0,0 | 2,6 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 11 | 10 | 107 s | 115 | 0,2 | 55,6 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 10 | 10 | 131 s | 17 | 0,0 | 61,3 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 10 | 10 | 26 s | 0 | 0,0 | 3,4 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 10 | 10 | 134 s | 66 | 0,1 | 66,2 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 12 | 10 | 136 s | 187 | 0,2 | 62,7 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 10 | 10 | 32 s | 163 | 0,5 | 2,8 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 15 | 9 | 165 s | 362 | 0,4 | 70,8 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 9 | 9 | 155 s | 39 | 0,0 | 88,1 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 9 | 9 | 43 s | 37 | 0,0 | 2,1 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 11 | 9 | 160 s | 202 | 0,0 | 81,5 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 11 | 9 | 164 s | 217 | 0,1 | 89,3 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 9 | 9 | 43 s | 6 | 0,0 | 8,6 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 10 | 9 | 176 s | 164 | 0,1 | 104,9 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 11 | 9 | 180 s | 203 | 0,1 | 96,5 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 9 | 9 | 53 s | 0 | 0,0 | 2,3 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 10 | 9 | 185 s | 108 | 0,0 | 112,5 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 10 | 9 | 190 s | 209 | 0,3 | 118,2 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 10 | 9 | 50 s | 137 | 0,0 | 1,4 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 12 | 9 | 195 s | 342 | 0,3 | 120,1 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 11 | 8 | 185 s | 256 | 0,1 | 110,6 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 8 | 8 | 71 s | 122 | 0,1 | 6,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 10 ×1, piętro 11 ×10, piętro 13 ×1, piętro 14 ×8, piętro 15 ×3, piętro 16 ×5, piętro 17 ×4, piętro 19 ×1, piętro 20 ×4, piętro 22 ×1, piętro 23 ×1, piętro 26 ×2, piętro 28 ×2; wyzwań (3 porażki na piętrze): 1 – piętro 28.
**Gracz H&S / souls-like** – ponowne próby: piętro 10 ×1, piętro 14 ×2, piętro 16 ×5, piętro 19 ×2, piętro 20 ×2, piętro 22 ×1, piętro 23 ×2, piętro 25 ×1, piętro 26 ×1, piętro 27 ×1, piętro 28 ×3, piętro 29 ×2; wyzwań (3 porażki na piętrze): 1 – piętro 29.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 802 | 2,1 s | 3,6 | 2259 | 33929 | 41 | 3 |
| Wiedźma wichru | 1228 | 1,4 s | 2,8 | 2609 | 7865 | 6 | 1 |
| Grzebacz | 641 | 2,0 s | 4,9 | 597 | 6729 | 10 | 0 |
| Tytan Otchłani | 17 | 38,9 s | 128,9 | 116 | 4326 | 254 | 0 |
| Kultysta topieli | 1391 | 2,3 s | 4,5 | 1144 | 3812 | 3 | 1 |
| Harpia | 966 | 1,0 s | 3,2 | 1368 | 3452 | 3 | 2 |
| Ogar otchłani | 1813 | 0,7 s | 2,6 | 676 | 3372 | 2 | 1 |
| Taranowiec | 1370 | 1,9 s | 5,3 | 223 | 2625 | 2 | 0 |
| Kowal Piekieł | 14 | 48,2 s | 219,4 | 101 | 2508 | 179 | 0 |
| Cień | 884 | 1,2 s | 3,8 | 255 | 2380 | 3 | 0 |
| Nekromanta | 412 | 4,0 s | 7,3 | 275 | 2272 | 5 | 0 |
| Kolos | 1361 | 2,9 s | 6,5 | 43 | 2229 | 2 | 0 |
| Arcydiabeł | 12 | 76,3 s | 485,5 | 97 | 2128 | 177 | 0 |
| Upiór | 498 | 4,0 s | 8,8 | 779 | 1574 | 3 | 0 |
| Kultysta | 1644 | 0,7 s | 2,0 | 273 | 1456 | 1 | 0 |
| Królowa Topieli | 20 | 69,4 s | 145,6 | 317 | 1361 | 68 | 0 |
| Legionista | 1177 | 3,2 s | 9,3 | 169 | 1141 | 1 | 0 |
| Kolos żaru | 836 | 1,9 s | 5,9 | 33 | 1060 | 1 | 0 |
| Tkaczka | 445 | 2,8 s | 6,1 | 384 | 986 | 2 | 0 |
| Ożywiony nietoperz | 1034 | 0,3 s | 1,3 | 143 | 968 | 1 | 0 |
| Siarkowy pomiot | 1269 | 0,2 s | 1,4 | 20 | 953 | 1 | 0 |
| Gnilec | 767 | 4,8 s | 13,3 | 78 | 686 | 1 | 0 |
| Czerw | 4360 | 0,4 s | 1,5 | 137 | 555 | 0 | 0 |
| Gnilec (odłamek) | 1545 | 1,9 s | 5,0 | 109 | 506 | 0 | 0 |
| Władca Bramy | 20 | 12,6 s | 24,5 | 59 | 457 | 23 | 0 |
| Pajączek | 317 | 0,1 s | 1,1 | 96 | 403 | 1 | 0 |
| Ofiarnik | 487 | 1,9 s | 3,1 | 288 | 398 | 1 | 0 |
| Matka Burz | 15 | 45,7 s | 185,3 | 107 | 332 | 22 | 0 |
| Król Kości | 14 | 47,9 s | 226,6 | 39 | 261 | 19 | 0 |
| Gargulec | 523 | 3,0 s | 7,9 | 30 | 248 | 0 | 0 |
| Rzeźnik | 19 | 30,4 s | 101,9 | 29 | 221 | 12 | 0 |
| Pan Sfory | 20 | 18,1 s | 34,1 | 64 | 195 | 10 | 0 |
| Chochlik wichru | 489 | 0,4 s | 1,7 | 11 | 143 | 0 | 0 |
| Ognisty chochlik | 561 | 0,2 s | 1,4 | 6 | 91 | 0 | 0 |
| Wodny chochlik | 515 | 0,6 s | 2,2 | 13 | 84 | 0 | 0 |
| Kamienny chochlik | 530 | 0,4 s | 1,8 | 7 | 66 | 0 | 0 |
| Ognisty chochlik (elita) | 34 | 1,9 s | 4,3 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 48 | 2,8 s | 9,8 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 53 | 7,0 s | 18,6 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 47 | 7,6 s | 17,9 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 35 | 11,3 s | 29,9 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 48 | 4,5 s | 5,7 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 16 | 17,6 s | 43,1 | ↑ | ↑ | ↑ |  |
| Worek jaj | 243 | 0,9 s | 1,6 | 0 | 0 | 0 | 0 |
| Harpia (elita) | 33 | 3,2 s | 8,8 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 36 | 1,2 s | 5,3 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 8 | 13,2 s | 20,3 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 8 | 6,8 s | 23,5 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 33 | 4,8 s | 16,1 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 23 | 4,9 s | 13,0 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 15 | 9,4 s | 24,6 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 29 | 8,2 s | 15,6 | ↑ | ↑ | ↑ |  |
| Bezimienny Cień | 14 | 59,8 s | 292,7 | 80 | 0 | 0 | 0 |
| Wiedźma wichru (elita) | 44 | 6,0 s | 11,8 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 30 | 7,5 s | 29,4 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 31 | 1,3 s | 4,4 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 7 | 21,1 s | 20,6 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 36 | 9,5 s | 24,6 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 13 | 7,0 s | 16,1 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2152 | 47% | 0% | 0% | 53% | 0% |
| ciężki | 84 | 50% | 0% | 0% | 50% | 0% |
| nieparowalny | 1473 | 48% | 0% | 0% | 52% | 0% |
| nieblokowalny | 39 | 77% | 0% | 0% | 23% | 0% |
| obszarowy | 2941 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1990 | 32% | 0% | 0% | 68% | 0% |
| ciężki | 104 | 22% | 0% | 0% | 78% | 0% |
| nieparowalny | 1254 | 25% | 0% | 0% | 75% | 0% |
| nieblokowalny | 73 | 70% | 0% | 0% | 30% | 0% |
| obszarowy | 2924 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2066 | 23% | 14% | 3% | 12% | 48% |
| Tester QA (0,30 s) | ciężki | 225 | 2% | 0% | 5% | 24% | 69% |
| Tester QA (0,30 s) | nieparowalny | 831 | 7% | 3% | 7% | 29% | 53% |
| Tester QA (0,30 s) | nieblokowalny | 5 | 0% | 0% | 0% | 40% | 60% |
| Tester QA (0,30 s) | obszarowy | 1291 | 1% | 1% | 7% | 42% | 48% |
| Tester QA (0,30 s) | pocisk | 2144 | 3% | 8% | 6% | 12% | 71% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2300 | 13% | 11% | 14% | 6% | 55% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 244 | 1% | 0% | 1% | 24% | 75% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 815 | 6% | 1% | 3% | 11% | 79% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 15 | 7% | 0% | 0% | 7% | 87% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 1331 | 1% | 3% | 8% | 38% | 50% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2164 | 2% | 8% | 8% | 8% | 74% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 254 |
| Pajączek: Pajączek | 0,31 s | 229 |
| Chochlik wichru: Pazury | 0,33 s | 33 |
| Czerw: Ugryzienie | 0,33 s | 581 |
| Wodny chochlik: Pazury | 0,33 s | 50 |
| Ognisty chochlik: Pazury | 0,33 s | 39 |
| Kamienny chochlik: Pazury | 0,33 s | 24 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 428 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 1189 |
| Królowa Topieli: Królowa Topieli: WindStep | 0,45 s | 33 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 79% (n=105) | 80% (n=5) | 83% (n=80) | 100% (n=2) | 100% (n=48) | 69% (n=68) |
| niezauważony | 78% (n=303) | 100% (n=2) | 85% (n=112) | 100% (n=20) | 100% (n=1934) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 85% (n=158) |
| unik | 33% (n=858) | 30% (n=43) | 29% (n=733) | 47% (n=17) | 100% (n=71) | 28% (n=246) |
| unik (eksperyment) | 33% (n=165) | 43% (n=14) | 37% (n=124) | – | 100% (n=63) | 19% (n=37) |
| wyjście z zasięgu | 55% (n=137) | 82% (n=11) | 59% (n=34) | – | 100% (n=577) | 37% (n=133) |
| wyjście z zasięgu (eksperyment) | 81% (n=195) | 89% (n=9) | 86% (n=218) | – | 100% (n=103) | 48% (n=64) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 226,4 | reakcja na cios wręcz 213,9, unik przed pociskiem 82,8, wabienie pod czar obszarowy 5,8, wyjście z okrążenia 1,6 | drgnięcie (trafiony) 26,2, w trakcie uniku 24,2, brak wytrzymałości 12,6, picie flaszki 3,6, unieruchomienie 0,7, inna 0,5 | 65% (n=2139) / 91% (n=828) |
| Gracz H&S / souls-like | 276,9 | reakcja na cios wręcz 254,3, unik przed pociskiem 103,4, doskok do odsłoniętego celu 21,1, wabienie pod czar obszarowy 2,7, wyjście z okrążenia 0,7 | w trakcie uniku 53,8, drgnięcie (trafiony) 17,1, brak wytrzymałości 13,7, picie flaszki 1,5, unieruchomienie 1,1, inna 0,1, szybki atak 0,1 | 74% (n=2543) / 95% (n=1034) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Woda | Nova wody | 10 | 4 | 22,4 | 1066,5 |
| Gracz H&S / souls-like | Woda | Nova wody | 10 | 8 | 28,5 | 1477,7 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka wody | 3591253 | 68% |  |  |
| Wir głębin | 1094993 | 21% | 1563 | 701 |
| Nova wody | 254475 | 5% | 967 | 263 |
| Ściana ognia | 180052 | 3% | 490 | 367 |
| Odskok widma | 73388 | 1% |  |  |
| Pocisk arkanów | 23818 | 0% | 270 | 88 |
| Łańcuchy potępionych | 18955 | 0% | 49 | 387 |
| Nova powietrza | 7212 | 0% | 9 | 801 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Kamienna osłona ×13, Zaklęte ostrze ×83, Odtrącenie ×2.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka wody | 5535690 | 69% |  |  |
| Wir głębin | 1159692 | 14% | 1062 | 1092 |
| Płonąca ziemia | 681877 | 8% | 372 | 1833 |
| Lodowy podmuch | 306166 | 4% | 926 | 331 |
| Burza | 117221 | 1% | 81 | 1447 |
| Odskok widma | 105039 | 1% |  |  |
| Nova wody | 27838 | 0% | 100 | 278 |
| Nova ziemi | 19921 | 0% | 28 | 711 |
| Wyrok burzy | 16444 | 0% | 10 | 1644 |
| Piekielna kula | 8328 | 0% | 4 | 2082 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Krwawy pakt ×7, Strzała rozszczepu ×1.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,5, mediana 4,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 22% | 76,0 | 268 | 0,5 |
| klasa | 45% | 156,3 | 701 | 0,4 |
| sklep | 5% | 15,6 | 168 | 2,5 |
| przedmiot | 28% | 96,7 | 263 | 0,5 |

Oferty sklepu dusz: widziane 642, kupione 6, odrzucone 0, za mało dusz 208.
Zamiany w slotach: 4,3 na podejście; najczęstsze: Ściana ognia → Nova wody ×13, Nova wody → Ściana ognia ×11, Pocisk arkanów → Nova wody ×6, Nova wody → Pocisk arkanów ×6, Nova wody → Kamienna osłona ×1, Nova wody → Zaklęte ostrze ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Wir głębin | klasa | 10 | 156,3 | 701 | 0,4 |
| Nova wody | przedmiot | 10 | 96,7 | 263 | 0,5 |
| Ściana ognia | odblokowanie | 10 | 49,0 | 367 | 0,4 |
| Pocisk arkanów | odblokowanie | 10 | 27,0 | 88 | 0,7 |
| Zaklęte ostrze | sklep | 1 | 83,0 | – | 2,0 |
| Łańcuchy potępionych | sklep | 1 | 49,0 | 387 | 4,0 |
| Kamienna osłona | sklep | 1 | 13,0 | – | 1,0 |
| Nova powietrza | sklep | 1 | 9,0 | 801 | 2,0 |
| Odtrącenie | sklep | 1 | 2,0 | – | 2,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 6,2, mediana 6,0, najwięcej 9 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 54% | 146,4 | 765 | 0,9 |
| klasa | 39% | 106,2 | 1092 | 0,2 |
| sklep | 4% | 10,2 | 816 | 2,2 |
| przedmiot | 4% | 10,0 | 278 | 1,1 |

Oferty sklepu dusz: widziane 825, kupione 18, odrzucone 10, za mało dusz 247.
Zamiany w slotach: 5,4 na podejście; najczęstsze: Pocisk arkanów → Lodowy podmuch ×8, Nova wody → Płonąca ziemia ×7, Kojące światło → Burza ×7, Ściana ognia → Strzał przeszywający ×1, Nova wody → Lodowy podmuch ×1, Kojące światło → Krwawy pakt ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Wir głębin | klasa | 10 | 106,2 | 1092 | 0,2 |
| Lodowy podmuch | odblokowanie | 9 | 102,9 | 331 | 0,8 |
| Płonąca ziemia | odblokowanie | 8 | 46,5 | 1833 | 1,3 |
| Nova wody | przedmiot | 1 | 100,0 | 278 | 1,1 |
| Burza | odblokowanie | 8 | 10,1 | 1447 | 0,2 |
| Ściana ognia | odblokowanie | 1 | 29,0 | 191 | 0,4 |
| Nova ziemi | sklep | 1 | 28,0 | 711 | 2,0 |
| Łańcuch błyskawic | odblokowanie | 1 | 20,0 | 253 | 1,0 |
| Nova ognia | sklep | 1 | 13,0 | 419 | 2,0 |
| Łańcuchy potępionych | sklep | 1 | 11,0 | 418 | 2,0 |
| Wyrok burzy | sklep | 2 | 5,0 | 1644 | 2,0 |
| Krwawy pakt | odblokowanie | 1 | 7,0 | – | 0,0 |
| Mroźna fala | odblokowanie | 1 | 7,0 | 154 | 0,4 |
| Tąpnięcie | sklep | 1 | 7,0 | 289 | 4,0 |
| Pocisk arkanów | odblokowanie | 2 | 3,0 | 61 | 0,3 |
| Kula ognia | odblokowanie | 2 | 3,0 | 181 | 1,0 |
| Salwa | sklep | 1 | 6,0 | 1006 | 2,0 |
| Rozprysk arkanów | odblokowanie | 1 | 5,0 | 118 | 0,0 |
| Włócznia potępionych | odblokowanie | 2 | 2,5 | 116 | 0,0 |
| Mroźna strzała | sklep | 1 | 5,0 | 365 | 4,0 |
| Kamienne kolce | sklep | 1 | 5,0 | 559 | 2,0 |
| Przyzwanie chochlików | sklep | 1 | 5,0 | 611 | 2,0 |
| Piekielna kula | sklep | 1 | 4,0 | 2082 | 1,0 |
| Krąg żaru | sklep | 1 | 4,0 | 1381 | 2,0 |
| Kościany golem | sklep | 1 | 2,0 | 3106 | 2,0 |
| Strzała rozszczepu | sklep | 1 | 1,0 | – | 1,0 |
| Strzała siarkowa | sklep | 1 | 1,0 | 980 | 2,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 1711,5, umiejętność 344,6, unik 226,4, drgnięcie (trafiony) 120,5, czar obszarowy na jednego (test) 46,1, wabienie bez skutku 45,1, czar obszarowy na jedynego wroga 29,4, flaszka (próba) 28,7, czar obszarowy na grupę 16,9, flaszka many (próba) 10,7, czar obszarowy na jednego (ratunek) 4,3, czar obszarowy po zwabieniu 1,1.
Czas w bloku 0 s, w biegu 80 s, bez wytrzymałości (<10%) 161 s na podejście.
Zakupy: ulepszenie: Nova wody ×21, dodatkowa flaszka ×19, założony przedmiot: Szata adepta ×11, sklep: Różdżka wody: poziom 3 ×10, sklep: Różdżka wody: poziom 4 ×10, sklep: Przepalenie ×9, ulepszenie: Pocisk arkanów ×9, założony przedmiot: Żelazny pierścień ×9.
Punkty cech: Intelligence 59, Toughness 53, Dexterity 52, Strength 50.
Pieczęcie na podejście: z daru bossa 6,7, z sakwy do skilli 4,4, kupione 0,4; najczęstsze: Klątwa Mokoszy ×9, Nieskończoność ×9, Oko Czarnoboga ×9, Deszcz piekieł ×8, Pakt z diabłem ×6, Serce otchłani ×6.
Komunikaty gry: „Za mało zdrowia” ×287; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×193; „Portale gasną – dobij pozostałe demony” ×152; „OGŁUSZENIE!” ×109; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×71; „Garda wroga przełamana!” ×55; „POLEGŁEŚ” ×49; „ODKRYCIE: więź Więź: Para” ×40.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 1845,9, unik 276,9, umiejętność 272,8, drgnięcie (trafiony) 69,1, wabienie bez skutku 16,0, flaszka (próba) 15,5, czar obszarowy na jedynego wroga 13,3, flaszka many (próba) 8,2, czar obszarowy na grupę 2,2, czar obszarowy po zwabieniu 0,5.
Czas w bloku 0 s, w biegu 101 s, bez wytrzymałości (<10%) 122 s na podejście.
Zakupy: sklep: Twarda skóra ×20, sklep: Furia ×19, dodatkowa flaszka ×19, sklep: Nurt ×17, sklep: Przebicie ×17, sklep: Szybkie ręce ×17, założony przedmiot: Żelazny pierścień ×16, sklep: Rozmycie ×15.
Punkty cech: Intelligence 189, Toughness 86.
Pieczęcie na podejście: z daru bossa 8,6, z sakwy do skilli 4,3, kupione 0,0; najczęstsze: Nieskończoność ×14, Klątwa Mokoszy ×13, Wieczny płomień ×10, Pakt z diabłem ×7, Oko Czarnoboga ×7, Serce otchłani ×6.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Kontra, Rozprysk arkanów, Mroźna fala, Krwawy pakt, Mroźna zbroja, Łańcuch błyskawic, Włócznia potępionych, Płonąca ziemia, Burza, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Szata popiołu, Kosa, Wielki topór, Młot burzy, Kojące światło, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×213; „Portale gasną – dobij pozostałe demony” ×190; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×94; „OGŁUSZENIE!” ×61; „Garda wroga przełamana!” ×45; „POLEGŁEŚ” ×25; „Kupiono: Twarda skóra” ×20; „Królowa Topieli: kolejna faza” ×20.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×703, uśpiony wróg blokuje koniec piętra ×158, zablokowany ruch ×8.
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×685, uśpiony wróg blokuje koniec piętra ×167, zablokowany ruch ×8.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 10 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** zablokowany ruch średnio 0,8× na podejście (geometria areny/kolizje). Przykłady: piętro 22 „XXII. Chór Cieni”: brak postępu przy (-7,8; -0,5); piętro 25 „XXV. Kuźnia Piekieł”: brak postępu przy (8,2; -9,1); piętro 26 „XXVI. Kuźnia Piekieł”: brak postępu przy (-6,3; -1,1).
- Unik przed czerwonym (nieblokowalnym): 47% ciosów mimo to rani (n=17) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 33% ciosów (n=57).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=680) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 9% pocisków, przed którymi był unik (n=828).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 38% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2066) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=225).
- Telegraf „nieparowalny” daje czas: tylko 10% decyzji zapada mniej niż 0,1 s przed trafieniem (n=831).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1291).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- UX: gra odmawiała akcji: „Za mało zdrowia” ×287 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XIV. Kamienne Trzewia” trwa średnio 152 s – najdłuższy fragment wieży.
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 150 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 154 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 157 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 187 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 184 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 174 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 184 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVI. Kuźnia Piekieł” trwa średnio 194 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVIII. Tron Arcydemona” trwa średnio 193 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIX. Tron Arcydemona” trwa średnio 183 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,8 ms/klatkę (maks. 465 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 8/10 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Progres między podejściami słabo odczuwalny: 31,0 → 27,6 średnio. Meta-progresja niewiele zmienia w walce.
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Tytan Otchłani (112,9 trafień, 27,7 s), Królowa Topieli (105,5 trafień, 47,1 s), Rzeźnik (94,1 trafień, 20,9 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,1), Ożywiony nietoperz (1,4), Ognisty chochlik (1,4) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Królowa Topieli – średnio 47 s i 106 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 48 s i 323 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 46 s i 254 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 67 s i 552 trafień do zabicia.
- 2/2 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 82% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 30% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Płonąca ziemia”: 1833 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 4%. Różnych skilli na podejście: średnio 6,2.
- Unik to moje główne narzędzie: 276,9 na podejście, w tym 21,8 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 95% przypadków.
- Gra nie wykonała uniku od razu średnio 87,4× na podejście – najczęściej: w trakcie poprzedniego uniku 53,8, drgnięcie (trafiony) 17,1, brak wytrzymałości 13,7. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (69 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

