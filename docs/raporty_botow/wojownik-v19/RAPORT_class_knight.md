# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 20:25. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 2 (10%) | 14,4 | 11,0 | 27,9 min | 575,4 | 325688 / 9546 | 21,8 | 1% |
| Gracz H&S / souls-like | 20 | 5 (25%) | 17,3 | 14,0 | 34,3 min | 749,5 | 573347 / 13172 | 26,1 | 2% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 13 10 3 ★ 9 29 9 17 10 17 10 11 13 8 8 20 7 22 ★ 11
- Gracz H&S / souls-like: ★ 8 8 8 ★ 13 11 17 ★ 13 19 11 14 ★ 20 13 11 ★ 16 14

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×3, 11. XI. Rzeźnia ×2, 13. XIII. Kamienne Trzewia ×2, 17. XVII. Wieża Burz ×2, 20. XX. Kostnica ×1, 22. XXII. Chór Cieni ×1, 29. XXIX. Tron Arcydemona ×1, 3. III. Władca Bramy ×1, 7. VII. Zatopione Krużganki ×1, 8. VIII. Zatopione Krużganki ×2, 9. IX. Królowa Topieli ×2.
  Ostatni cios: Kultysta topieli – „Kultysta topieli: Zones” ×2; Miotacz głazów – „Miotacz głazów” ×2; Siarkowy pomiot – „Siarkowy pomiot” ×2; Miotacz głazów – „Miotacz głazów: Rockfall” ×2; Kultysta – „Wachlarz ognia” ×1; Władca Bramy – „Cięcie w szale” ×1. Śmierć ciosem ≥40% maks. HP: 0/18. Zginął mając flaszki: 13/18.
  Ostatnie 3 s przed śmiercią: średnio 31% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/18). Picie: 526 prób, 435 wypitych (17% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 11. XI. Rzeźnia ×3, 13. XIII. Kamienne Trzewia ×3, 14. XIV. Kamienne Trzewia ×2, 16. XVI. Wieża Burz ×1, 17. XVII. Wieża Burz ×1, 19. XIX. Kostnica ×1, 20. XX. Kostnica ×1, 8. VIII. Zatopione Krużganki ×3.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×4; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×2; Czerw – „Ugryzienie” ×1; Kolos – „Kolos: Combo” ×1; Kolos – „Kolos” ×1; Kultysta – „Kula ognia” ×1. Śmierć ciosem ≥40% maks. HP: 0/15. Zginął mając flaszki: 13/15.
  Ostatnie 3 s przed śmiercią: średnio 29% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/15). Picie: 602 prób, 522 wypitych (13% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 99 s | 9 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 22 | 20 | 115 s | 142 | 0,6 | 39,1 |
| Tester QA | 3. III. Władca Bramy | 22 | 19 | 24 s | 339 | 0,8 | 7,6 |
| Tester QA | 4. IV. Psiarnia Piekieł | 19 | 19 | 109 s | 45 | 0,2 | 46,5 |
| Tester QA | 5. V. Psiarnia Piekieł | 19 | 19 | 99 s | 90 | 0,2 | 40,8 |
| Tester QA | 6. VI. Pan Sfory | 21 | 19 | 32 s | 399 | 1,5 | 5,4 |
| Tester QA | 7. VII. Zatopione Krużganki | 22 | 18 | 106 s | 426 | 1,6 | 38,1 |
| Tester QA | 8. VIII. Zatopione Krużganki | 25 | 16 | 133 s | 692 | 2,3 | 46,4 |
| Tester QA | 9. IX. Królowa Topieli | 28 | 14 | 57 s | 830 | 2,5 | 1,7 |
| Tester QA | 10. X. Rzeźnia | 25 | 11 | 109 s | 618 | 1,5 | 38,5 |
| Tester QA | 11. XI. Rzeźnia | 18 | 9 | 126 s | 716 | 1,4 | 48,6 |
| Tester QA | 12. XII. Rzeźnik | 9 | 9 | 46 s | 273 | 1,0 | 6,3 |
| Tester QA | 13. XIII. Kamienne Trzewia | 13 | 7 | 140 s | 747 | 2,2 | 52,9 |
| Tester QA | 14. XIV. Kamienne Trzewia | 10 | 7 | 136 s | 772 | 1,4 | 60,6 |
| Tester QA | 15. XV. Tytan Otchłani | 8 | 7 | 55 s | 1098 | 2,9 | 2,6 |
| Tester QA | 16. XVI. Wieża Burz | 8 | 7 | 165 s | 787 | 1,1 | 88,3 |
| Tester QA | 17. XVII. Wieża Burz | 11 | 5 | 151 s | 1091 | 1,0 | 64,6 |
| Tester QA | 18. XVIII. Matka Burz | 5 | 5 | 75 s | 946 | 2,2 | 3,0 |
| Tester QA | 19. XIX. Kostnica | 5 | 5 | 160 s | 887 | 1,4 | 91,4 |
| Tester QA | 20. XX. Kostnica | 8 | 4 | 172 s | 1190 | 1,0 | 63,9 |
| Tester QA | 21. XXI. Król Kości | 4 | 4 | 60 s | 388 | 0,0 | 15,0 |
| Tester QA | 22. XXII. Chór Cieni | 6 | 3 | 183 s | 753 | 0,2 | 63,5 |
| Tester QA | 23. XXIII. Chór Cieni | 3 | 3 | 182 s | 682 | 1,0 | 101,7 |
| Tester QA | 24. XXIV. Bezimienny Cień | 3 | 3 | 50 s | 221 | 0,0 | 4,0 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 3 | 3 | 185 s | 240 | 0,7 | 117,7 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 3 | 3 | 180 s | 649 | 0,3 | 108,7 |
| Tester QA | 27. XXVII. Kowal Piekieł | 3 | 3 | 63 s | 959 | 1,3 | 1,3 |
| Tester QA | 28. XXVIII. Tron Arcydemona | 4 | 3 | 200 s | 925 | 1,0 | 112,0 |
| Tester QA | 29. XXIX. Tron Arcydemona | 6 | 2 | 162 s | 880 | 0,8 | 66,0 |
| Tester QA | 30. XXX. Arcydiabeł | 2 | 2 | 82 s | 973 | 0,0 | 5,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 102 s | 17 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 20 | 114 s | 105 | 0,2 | 40,4 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 22 s | 136 | 0,2 | 7,5 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 20 | 20 | 111 s | 71 | 0,1 | 46,0 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 20 | 20 | 93 s | 77 | 0,2 | 40,5 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 22 | 20 | 36 s | 446 | 1,3 | 6,1 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 24 | 20 | 105 s | 416 | 1,0 | 40,2 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 32 | 17 | 137 s | 777 | 1,8 | 39,9 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 17 | 17 | 43 s | 580 | 1,4 | 3,0 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 23 | 17 | 116 s | 692 | 1,5 | 50,1 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 28 | 14 | 132 s | 1108 | 2,5 | 48,5 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 14 | 14 | 39 s | 162 | 0,2 | 7,0 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 29 | 11 | 139 s | 1157 | 2,4 | 44,6 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 16 | 9 | 130 s | 1012 | 2,0 | 60,3 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 9 | 9 | 39 s | 709 | 2,0 | 3,0 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 11 | 8 | 157 s | 1031 | 2,5 | 72,3 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 13 | 7 | 160 s | 1107 | 2,2 | 63,8 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 7 | 7 | 34 s | 474 | 0,6 | 2,7 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 9 | 6 | 164 s | 974 | 1,3 | 73,3 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 10 | 5 | 165 s | 1223 | 1,8 | 78,2 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 5 | 5 | 30 s | 122 | 0,2 | 11,0 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 6 | 5 | 169 s | 1138 | 2,7 | 98,3 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 5 | 5 | 173 s | 968 | 1,6 | 109,0 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 5 | 5 | 26 s | 46 | 0,2 | 2,8 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 7 | 5 | 181 s | 1793 | 1,7 | 105,7 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 5 | 5 | 199 s | 1022 | 0,8 | 123,6 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 5 | 5 | 19 s | 175 | 0,2 | 1,6 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 6 | 5 | 192 s | 1062 | 1,5 | 121,5 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 5 | 5 | 187 s | 1119 | 1,2 | 132,0 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 5 | 5 | 41 s | 475 | 0,2 | 9,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 2 ×2, piętro 3 ×2, piętro 6 ×2, piętro 7 ×3, piętro 8 ×7, piętro 9 ×12, piętro 10 ×11, piętro 11 ×7, piętro 13 ×4, piętro 14 ×3, piętro 15 ×1, piętro 16 ×1, piętro 17 ×4, piętro 20 ×3, piętro 22 ×2, piętro 28 ×1, piętro 29 ×3; wyzwań (3 porażki na piętrze): 2 – piętro 13, piętro 29.
**Gracz H&S / souls-like** – ponowne próby: piętro 6 ×2, piętro 7 ×4, piętro 8 ×12, piętro 10 ×6, piętro 11 ×11, piętro 13 ×15, piętro 14 ×5, piętro 16 ×2, piętro 17 ×5, piętro 19 ×2, piętro 20 ×4, piętro 22 ×1, piętro 25 ×2, piętro 28 ×1; wyzwań (3 porażki na piętrze): 7 – piętro 8, piętro 13, piętro 17, piętro 19, piętro 20.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 614 | 0,7 s | 1,5 | 1749 | 60724 | 96 | 8 |
| Wiedźma wichru | 1170 | 0,9 s | 1,4 | 4672 | 46853 | 38 | 3 |
| Kultysta topieli | 1376 | 0,5 s | 1,3 | 2423 | 46050 | 32 | 3 |
| Królowa Topieli | 31 | 44,6 s | 42,0 | 1113 | 30882 | 996 | 2 |
| Kultysta | 2237 | 0,3 s | 1,2 | 1886 | 30623 | 13 | 2 |
| Harpia | 951 | 0,5 s | 1,5 | 1798 | 22057 | 22 | 2 |
| Kolos | 1285 | 1,3 s | 2,4 | 521 | 19865 | 15 | 2 |
| Grzebacz | 485 | 0,8 s | 2,1 | 548 | 18610 | 37 | 1 |
| Pan Sfory | 39 | 29,5 s | 21,3 | 1032 | 16831 | 432 | 0 |
| Tytan Otchłani | 16 | 41,1 s | 67,5 | 347 | 15152 | 947 | 0 |
| Ogar otchłani | 2356 | 0,3 s | 1,3 | 1705 | 13794 | 6 | 0 |
| Upiór | 384 | 1,4 s | 2,2 | 1648 | 12578 | 31 | 1 |
| Kolos żaru | 544 | 1,2 s | 2,5 | 352 | 12225 | 22 | 0 |
| Władca Bramy | 39 | 19,0 s | 11,9 | 400 | 9408 | 241 | 1 |
| Taranowiec | 1418 | 0,8 s | 1,8 | 794 | 9162 | 6 | 0 |
| Siarkowy pomiot | 1430 | 0,1 s | 1,1 | 222 | 7892 | 5 | 2 |
| Matka Burz | 12 | 46,4 s | 77,0 | 339 | 7639 | 637 | 0 |
| Gnilec | 661 | 1,8 s | 3,1 | 517 | 7544 | 11 | 0 |
| Legionista | 1143 | 2,4 s | 3,4 | 993 | 7072 | 6 | 1 |
| Tkaczka | 341 | 1,0 s | 1,8 | 659 | 7059 | 20 | 0 |
| Czerw | 5763 | 0,1 s | 1,1 | 1928 | 6174 | 1 | 2 |
| Cień | 666 | 0,4 s | 1,5 | 506 | 5417 | 8 | 0 |
| Gnilec (odłamek) | 1333 | 0,7 s | 1,8 | 445 | 4887 | 4 | 1 |
| Rzeźnik | 23 | 36,3 s | 45,0 | 333 | 4347 | 189 | 0 |
| Arcydiabeł | 7 | 45,5 s | 51,0 | 237 | 4298 | 614 | 0 |
| Kowal Piekieł | 8 | 31,6 s | 43,6 | 130 | 3736 | 467 | 0 |
| Ofiarnik | 443 | 0,4 s | 1,3 | 573 | 3489 | 8 | 0 |
| Gargulec | 430 | 1,0 s | 2,5 | 165 | 2561 | 6 | 0 |
| Kamienny chochlik | 841 | 0,1 s | 1,2 | 210 | 2329 | 3 | 0 |
| Ożywiony nietoperz | 745 | 0,1 s | 1,1 | 535 | 2263 | 3 | 0 |
| Ognisty chochlik | 914 | 0,2 s | 1,2 | 227 | 2113 | 2 | 0 |
| Król Kości | 9 | 38,7 s | 95,0 | 252 | 2067 | 230 | 0 |
| Wodny chochlik | 816 | 0,2 s | 1,2 | 225 | 2048 | 3 | 0 |
| Pajączek | 193 | 0,0 s | 1,0 | 330 | 2044 | 11 | 1 |
| Chochlik wichru | 869 | 0,3 s | 1,3 | 209 | 1881 | 2 | 0 |
| Nekromanta | 200 | 1,0 s | 1,7 | 548 | 1877 | 9 | 1 |
| Bezimienny Cień | 8 | 30,6 s | 80,5 | 106 | 796 | 100 | 0 |
| Ognisty chochlik (elita) | 112 | 2,2 s | 3,6 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 89 | 3,0 s | 3,3 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 77 | 2,2 s | 4,0 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 49 | 9,3 s | 13,2 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 41 | 3,7 s | 3,9 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 69 | 0,8 s | 2,5 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 23 | 9,2 s | 16,2 | ↑ | ↑ | ↑ |  |
| Worek jaj | 185 | 0,2 s | 1,2 | 0 | 0 | 0 | 0 |
| Taranowiec (elita) | 49 | 4,7 s | 7,6 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 16 | 7,9 s | 13,9 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 51 | 1,7 s | 2,6 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 44 | 11,8 s | 14,8 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 33 | 2,6 s | 4,3 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 47 | 4,0 s | 4,6 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 20 | 8,7 s | 8,9 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 11 | 5,1 s | 8,5 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 12 | 2,1 s | 3,8 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 18 | 4,1 s | 6,3 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 13 | 3,1 s | 6,2 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 24 | 2,8 s | 5,5 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 15 | 5,5 s | 8,7 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 13 | 5,9 s | 6,4 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 4216 | 70% | 5% | 3% | 19% | 3% |
| ciężki | 508 | 62% | 4% | 5% | 28% | 2% |
| nieparowalny | 2792 | 69% | 10% | 0% | 16% | 5% |
| nieblokowalny | 213 | 79% | 0% | 0% | 21% | 0% |
| obszarowy | 3970 | 85% | 11% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 7628 | 61% | 6% | 11% | 19% | 3% |
| ciężki | 766 | 48% | 7% | 15% | 28% | 1% |
| nieparowalny | 4333 | 59% | 16% | 0% | 20% | 5% |
| nieblokowalny | 406 | 78% | 0% | 0% | 22% | 0% |
| obszarowy | 5845 | 78% | 18% | 0% | 0% | 4% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 3589 | 23% | 32% | 4% | 16% | 24% |
| Tester QA (0,30 s) | ciężki | 701 | 3% | 2% | 7% | 20% | 69% |
| Tester QA (0,30 s) | nieparowalny | 1144 | 5% | 2% | 8% | 27% | 58% |
| Tester QA (0,30 s) | nieblokowalny | 67 | 1% | 0% | 7% | 40% | 51% |
| Tester QA (0,30 s) | obszarowy | 2347 | 2% | 2% | 2% | 20% | 75% |
| Tester QA (0,30 s) | pocisk | 2666 | 4% | 17% | 11% | 19% | 48% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 6739 | 4% | 25% | 31% | 10% | 31% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 1111 | 2% | 1% | 2% | 20% | 75% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1727 | 4% | 2% | 2% | 13% | 79% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 38 | 11% | 3% | 0% | 11% | 76% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 3741 | 2% | 1% | 3% | 14% | 81% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 4896 | 3% | 19% | 13% | 17% | 49% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 1387 |
| Pajączek: Pajączek | 0,31 s | 458 |
| Kamienny chochlik: Pazury | 0,33 s | 389 |
| Chochlik wichru: Pazury | 0,33 s | 386 |
| Czerw: Ugryzienie | 0,33 s | 3105 |
| Wodny chochlik: Pazury | 0,33 s | 412 |
| Ognisty chochlik: Pazury | 0,33 s | 419 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 758 |
| Władca Bramy: Cięcie w szale | 0,41 s | 22 |
| Władca Bramy: Piekielne kule | 0,43 s | 7 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 56% (n=399) | 43% (n=21) | 54% (n=494) | 92% (n=13) | 51% (n=682) | 85% (n=508) |
| blok (eksperyment) | 73% (n=155) | 75% (n=28) | 77% (n=153) | 80% (n=10) | 78% (n=196) | 65% (n=112) |
| brak reakcji (eksperyment) | 85% (n=171) | 88% (n=24) | 93% (n=115) | 89% (n=9) | 95% (n=194) | 80% (n=122) |
| niezauważony | 84% (n=784) | – | 96% (n=261) | 99% (n=92) | 99% (n=1193) | – |
| parowanie | 59% (n=130) | 60% (n=35) | – | – | 100% (n=2) | – |
| parowanie (eksperyment) | 73% (n=71) | 47% (n=15) | 93% (n=72) | 100% (n=2) | 99% (n=98) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 95% (n=359) |
| unik | 56% (n=802) | 48% (n=223) | 50% (n=534) | 26% (n=50) | 100% (n=24) | 67% (n=322) |
| unik (eksperyment) | 66% (n=240) | 46% (n=35) | 67% (n=172) | 89% (n=9) | 98% (n=230) | 76% (n=90) |
| wyjście z zasięgu | 91% (n=636) | 96% (n=101) | 97% (n=276) | 100% (n=26) | 99% (n=880) | 100% (n=4) |
| wyjście z zasięgu (eksperyment) | 88% (n=164) | 92% (n=26) | 90% (n=81) | 50% (n=2) | 96% (n=167) | 81% (n=85) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 108,1 | reakcja na cios wręcz 110,0, unik przed pociskiem 46,8, wyjście z okrążenia 11,2 | drgnięcie (trafiony) 19,1, brak wytrzymałości 13,5, w trakcie uniku 13,2, picie flaszki 4,3, inna 1,6, parowanie 1,3, unieruchomienie 0,7, umiejętność 0,6, blok 0,5, mocny atak 0,2, szybki atak 0,2, riposta 0,2, przełamana garda 0,1 | 37% (n=2200) / 70% (n=936) |
| Gracz H&S / souls-like | 192,3 | reakcja na cios wręcz 139,4, unik przed pociskiem 108,3, wyjście z okrążenia 28,8, doskok do odsłoniętego celu 25,3 | w trakcie uniku 30,2, drgnięcie (trafiony) 25,7, brak wytrzymałości 12,5, parowanie 10,7, picie flaszki 5,8, unieruchomienie 1,9, riposta 1,5, inna 1,4, blok 0,8, mocny atak 0,4, umiejętność 0,3, szybki atak 0,1 | 44% (n=2787) / 72% (n=2166) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 1 | 17,8 | 759,8 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 17,2 | 722,0 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 1 | 12,0 | 504,8 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 10,4 | 314,8 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 17 | 5 | 18,9 | 841,4 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 1 | 0 | 8,0 | 211,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 8,0 | 239,0 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 1 | 0 | 8,0 | 237,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Piekielna szarża | 1353991 | 21% | 470 | 2881 |
| Cięcie wichru | 1276483 | 20% | 1922 | 664 |
| Zamach | 612906 | 9% |  |  |
| Fala przełamania | 600334 | 9% | 265 | 2265 |
| Tąpnięcie | 564406 | 9% | 1864 | 303 |
| Rozpłatanie | 494083 | 8% | 1325 | 373 |
| Egzekucja | 400600 | 6% | 204 | 1964 |
| Cięcie | 238563 | 4% |  |  |
| Upadek kata | 190659 | 3% | 162 | 1177 |
| Okrzyk bojowy | 182084 | 3% | 1917 | 95 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×90.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Rozpłatanie | 4867882 | 42% | 6204 | 785 |
| Fala przełamania | 3893117 | 34% | 802 | 4854 |
| Szarża | 625016 | 5% | 2499 | 250 |
| Okrzyk bojowy | 452872 | 4% | 1273 | 356 |
| Potężne cięcie | 433712 | 4% |  |  |
| Riposta | 418116 | 4% |  |  |
| Cięcie | 407641 | 4% |  |  |
| Cięcie krzyżowe | 184748 | 2% | 234 | 790 |
| Szarża barkiem | 66519 | 1% |  |  |
| Tąpnięcie | 41141 | 0% | 187 | 220 |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,7, mediana 5,0, najwięcej 9 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 32% | 150,4 | 546 | 1,2 |
| odblokowanie | 20% | 93,2 | 303 | 1,6 |
| klasa | 20% | 95,9 | 95 | 0,6 |
| nagroda | 21% | 98,2 | 712 | 1,8 |
| sklep | 7% | 34,6 | 2237 | 1,5 |

Oferty sklepu dusz: widziane 267, kupione 10, odrzucone 0, za mało dusz 257.
Zamiany w slotach: 3,4 na podejście; najczęstsze: Cięcie wichru → Tąpnięcie ×9, Tąpnięcie → Cięcie wichru ×9, Cięcie wichru → Rozpłatanie ×4, Rozdarcie ran → Krwawe cięcie ×3, Rozpłatanie → Cięcie wichru ×3, Fala przełamania → Piekielna szarża ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 12 | 160,2 | 664 | 1,2 |
| Okrzyk bojowy | klasa | 20 | 95,9 | 95 | 0,6 |
| Tąpnięcie | odblokowanie | 20 | 93,2 | 303 | 1,6 |
| Rozpłatanie | startowy | 14 | 94,6 | 373 | 1,3 |
| Piekielna szarża | nagroda | 2 | 235,0 | 2881 | 1,5 |
| Szarża | nagroda | 2 | 150,5 | 431 | 1,2 |
| Fala przełamania | sklep | 3 | 88,3 | 2265 | 1,3 |
| Krwawe cięcie | nagroda | 4 | 56,3 | 401 | 1,5 |
| Wypad | nagroda | 3 | 73,7 | 29 | 0,9 |
| Egzekucja | nagroda | 4 | 51,0 | 1231 | 2,5 |
| Kontra | nagroda | 2 | 91,0 | 138 | 3,0 |
| Rozdarcie ran | nagroda | 2 | 88,0 | 205 | 2,3 |
| Upadek kata | nagroda | 2 | 81,0 | 1177 | 2,0 |
| Okrzyk wojenny | nagroda | 2 | 45,0 | – | 2,4 |
| Młynek | sklep | 1 | 62,0 | 585 | 2,0 |
| Cięcie krzyżowe | nagroda | 2 | 20,0 | 2384 | 2,0 |
| Seria cięć | sklep | 1 | 17,0 | 98 | 0,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,0, mediana 3,0, najwięcej 10 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 53% | 305,1 | 793 | 1,6 |
| odblokowanie | 25% | 141,9 | 244 | 1,6 |
| klasa | 11% | 63,7 | 356 | 0,6 |
| nagroda | 11% | 61,7 | 3316 | 1,6 |
| sklep | 0% | 1,1 | 927 | 1,0 |

Oferty sklepu dusz: widziane 326, kupione 1, odrzucone 16, za mało dusz 309.
Zamiany w slotach: 1,3 na podejście; najczęstsze: Szarża → Fala przełamania ×3, Szarża → Cięcie krzyżowe ×3, Szarża → Tąpnięcie ×2, Krwawe cięcie → Wypad ×1, Tąpnięcie → Cięcie krzyżowe ×1, Wypad → Fala przełamania ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | startowy | 22 | 282,0 | 785 | 1,6 |
| Szarża | odblokowanie | 17 | 147,0 | 250 | 1,7 |
| Okrzyk bojowy | klasa | 20 | 63,7 | 356 | 0,6 |
| Fala przełamania | nagroda | 5 | 160,4 | 4854 | 1,6 |
| Cięcie krzyżowe | nagroda | 4 | 58,5 | 790 | 1,9 |
| Tąpnięcie | odblokowanie | 4 | 46,8 | 220 | 1,1 |
| Cięcie wichru | startowy | 2 | 55,5 | 171 | 0,7 |
| Wypad | nagroda | 1 | 99,0 | 69 | 2,9 |
| Kontra | odblokowanie | 1 | 14,0 | 27 | 0,8 |
| Rozdarcie ran | odblokowanie | 1 | 13,0 | 94 | 0,0 |
| Krwawe cięcie | nagroda | 1 | 8,0 | 154 | 0,0 |
| Uderzenie tarczą | odblokowanie | 1 | 7,0 | 45 | 0,0 |
| Seria cięć | odblokowanie | 1 | 7,0 | 85 | 0,0 |
| Młynek | odblokowanie | 1 | 5,0 | 223 | 0,0 |
| Egzekucja | odblokowanie | 1 | 5,0 | 121 | 0,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 472,2, szybki atak 170,0, drgnięcie (trafiony) 155,1, unik 108,1, blok 81,5, mocny atak 61,4, flaszka (próba) 26,3, przełamana garda 22,1, parowanie 11,6, riposta 2,4.
Czas w bloku 41 s, w biegu 114 s, bez wytrzymałości (<10%) 334 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Tąpnięcie → +2 ×14, wzmocnienie: Zwinność ×8, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×8, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×7, ulepszenie: Ulepszenie: Tąpnięcie → +3 ×6, wzmocnienie: Żelazna skóra ×6, ulepszenie: Ulepszenie: Cięcie wichru → +2 ×5, wzmocnienie: Oddech wojownika ×5.
Zakupy: dodatkowa flaszka ×26, założony przedmiot: Stalowe rękawice ×16, ulepszenie: Okrzyk bojowy ×14, sklep: Twarda skóra ×14, sklep: Zwinność ×13, sklep: Nurt ×11, ulepszenie: Tąpnięcie ×11, sklep: Skupienie ×11.
Punkty cech: Strength 74, Toughness 68, Dexterity 64, Intelligence 61.
Pieczęcie na podejście: z daru bossa 4,2, z sakwy do skilli 3,8, kupione 0,7; najczęstsze: Bliźniacza dusza ×12, Wieczny płomień ×10, Klątwa Mokoszy ×9, Pakt z diabłem ×8, Korona Welesa ×7, Nieskończoność ×6.
Komunikaty gry: „OKRZYK!” ×1918; „SZAŁ!” ×835; „GARDA PRZEŁAMANA!” ×459; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×250; „Garda wroga przełamana!” ×226; „Portale gasną – dobij pozostałe demony” ×191; „OGŁUSZENIE!” ×163; „ZAMROŻENIE!” ×152.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 573,4, drgnięcie (trafiony) 214,0, unik 192,3, blok 161,5, szybki atak 103,4, parowanie 64,8, mocny atak 62,5, przełamana garda 31,8, flaszka (próba) 30,1, riposta 24,1.
Czas w bloku 89 s, w biegu 151 s, bez wytrzymałości (<10%) 276 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +2 ×31, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×22, ulepszenie: Ulepszenie: Szarża → +1 ×15, ulepszenie: Ulepszenie: Szarża → +2 ×13, ulepszenie: Ulepszenie: Rozpłatanie → +3 ×10, wzmocnienie: Żelazna skóra ×10, wzmocnienie: Siła tytana ×9, wzmocnienie: Zwinność ×9.
Zakupy: sklep: Szybkie ręce ×37, sklep: Ostrze ×30, dodatkowa flaszka ×29, sklep: Twarda skóra ×28, sklep: Kat ×26, sklep: Skupienie ×26, sklep: Hart ciała ×25, sklep: Świszczące ostrze ×24.
Punkty cech: Toughness 117, Strength 209.
Pieczęcie na podejście: z daru bossa 5,1, z sakwy do skilli 3,7, kupione 0,4; najczęstsze: Wieczny płomień ×12, Nieskończoność ×12, Bliźniacza dusza ×11, Klątwa Mokoszy ×10, Pakt z diabłem ×7, Oko Czarnoboga ×7.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Kosa, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×1482; „PAROWANIE!” ×962; „SZAŁ!” ×918; „Postawa przełamana – RIPOSTA!” ×782; „GARDA PRZEŁAMANA!” ×662; „RIPOSTA!” ×417; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×309; „OGŁUSZENIE!” ×298.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×1226, uśpiony wróg blokuje koniec piętra ×83, zablokowany ruch ×19, garda nie zatrzymała ciosu ×18, parowanie w oknie nie zadziałało ×10.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×1952, uśpiony wróg blokuje koniec piętra ×108, parowanie w oknie nie zadziałało ×86, zablokowany ruch ×54, garda nie zatrzymała ciosu ×51.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 10 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - Kultysta topieli: „Kultysta topieli: Zones” przeszedł przez gardę (kąt 180°) (w 4 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - Grzebacz: „Grzebacz” trafił w aktywnym oknie parowania (kąt do źródła 180°) (w 3 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** zablokowany ruch średnio 1,0× na podejście (geometria areny/kolizje). Przykłady: piętro 10 „X. Rzeźnia”: brak postępu przy (-7,8; -0,1); piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,0); piętro 12 „XII. Rzeźnik”: brak postępu przy (7,0; 8,0).
- **Do sprawdzenia (P3):** 10× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Taranowiec: „Taranowiec” trafił w aktywnym oknie parowania (kąt do źródła 0°)
- **Do sprawdzenia (P3):** 18× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Miotacz głazów: „Miotacz głazów” przeszedł przez gardę (kąt 122°)
- Parowanie zwykłych ciosów: 64% ciosów mimo to rani (n=201) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 36% ciosów (n=59).
- Unik przed ciężkim: 48% ciosów mimo to rani (n=258) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 61% ciosów mimo to rani (n=554) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 99% ciosów mimo to rani (n=1047) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 30% pocisków, przed którymi był unik (n=936).
- Blok pocisku: 81% ciosów mimo to rani (n=620) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 56% decyzji zapada, gdy do trafienia zostało <0,1 s (n=3589) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 5% decyzji zapada mniej niż 0,1 s przed trafieniem (n=701).
- Telegraf „nieparowalny” daje czas: tylko 7% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1144).
- Telegraf „nieblokowalny” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=67).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=2347).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Za mało zdrowia” ×142 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 165 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 151 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 160 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 172 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 183 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 182 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 185 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVI. Kuźnia Piekieł” trwa średnio 180 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVIII. Tron Arcydemona” trwa średnio 200 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIX. Tron Arcydemona” trwa średnio 162 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 80 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 5/20. Wyzwanie umiarkowane; po opanowaniu parowania wieża przestaje straszyć.
- Czuję progres: druga połowa podejść średnio piętro 18,0 wobec 17,1 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 42% udanych (n=1984). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Rzeźnik (34,1 trafień, 32,9 s), Królowa Topieli (32,2 trafień, 38,2 s), Pan Sfory (19,2 trafień, 30,4 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,0), Ożywiony nietoperz (1,0), Ognisty chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- 13/15 razy zginąłem z flaszkami w kieszeni. 0 z tych śmierci to utrata ≥50% HP w 3 s, a 13% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 88% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 4854 obrażeń na użycie.
- Skill startowy to 53% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 11%. Różnych skilli na podejście: średnio 4,0.
- Najczęściej grałem: Miecz i tarcza + Rozpłatanie (17×, śr. piętro 19,2). Inne: Miecz i tarcza + Cięcie wichru 1× (8,0), Dwuręczny + Rozpłatanie 1× (8,0), Dwuręczny + Cięcie wichru 1× (8,0).
- Unik to moje główne narzędzie: 192,3 na podejście, w tym 54,1 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 72% przypadków.
- Gra nie wykonała uniku od razu średnio 91,2× na podejście – najczęściej: w trakcie poprzedniego uniku 30,2, drgnięcie (trafiony) 25,7, brak wytrzymałości 12,5. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (13% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (214 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

