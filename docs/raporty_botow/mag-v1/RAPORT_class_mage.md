# Raport botów grających — Mag

Wygenerowano: 2026-10-29 11:03. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 2 (10%) | 15,0 | 12,5 | 31,5 min | 588,3 | 250174 / 4301 | 9,8 | 2% |
| Gracz H&S / souls-like | 20 | 4 (20%) | 20,0 | 17,5 | 40,5 min | 883,9 | 404970 / 4428 | 9,1 | 5% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: ★ 11 10 ★ 10 11 8 30 22 10 16 15 14 15 10 14 10 10 10 14
- Gracz H&S / souls-like: 14 11 16 16 ⏱10 13 11 ★ 15 ★ 14 19 13 20 ★ 27 28 ★ 27 26

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×7, 11. XI. Rzeźnia ×2, 14. XIV. Kamienne Trzewia ×3, 15. XV. Tytan Otchłani ×2, 16. XVI. Wieża Burz ×1, 22. XXII. Chór Cieni ×1, 30. XXX. Arcydiabeł ×1, 8. VIII. Zatopione Krużganki ×1.
  Ostatni cios: Miotacz głazów – „Miotacz głazów: Rockfall” ×4; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×2; Kultysta topieli – „Kultysta topieli” ×2; Kultysta topieli – „Kultysta topieli: Zones” ×1; Kolos – „Kolos: Combo” ×1; Arcydiabeł – „Arcydiabeł: Combo” ×1. Śmierć ciosem ≥40% maks. HP: 0/18. Zginął mając flaszki: 16/18.
  Ostatnie 3 s przed śmiercią: średnio 48% maks. HP straconych (śmierć z ≥50% HP w 3 s: 7/18). Picie: 506 prób, 195 wypitych (61% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 11. XI. Rzeźnia ×2, 13. XIII. Kamienne Trzewia ×2, 14. XIV. Kamienne Trzewia ×2, 15. XV. Tytan Otchłani ×1, 16. XVI. Wieża Burz ×2, 19. XIX. Kostnica ×1, 20. XX. Kostnica ×1, 26. XXVI. Kuźnia Piekieł ×1, 27. XXVII. Kowal Piekieł ×2, 28. XXVIII. Tron Arcydemona ×1.
  Ostatni cios: Miotacz głazów – „Miotacz głazów” ×3; Miotacz głazów – „Miotacz głazów: Rockfall” ×2; Kowal Piekieł – „Kowal Piekieł: InfernalOrb” ×2; Kolos – „Kolos: Combo” ×1; Legionista – „Legionista: ShieldBash” ×1; Ogar otchłani – „Ogar otchłani: Combo” ×1. Śmierć ciosem ≥40% maks. HP: 0/15. Zginął mając flaszki: 13/15.
  Ostatnie 3 s przed śmiercią: średnio 46% maks. HP straconych (śmierć z ≥50% HP w 3 s: 7/15). Picie: 463 prób, 181 wypitych (61% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 104 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 20 | 117 s | 0 | 0,0 | 40,7 |
| Tester QA | 3. III. Władca Bramy | 20 | 20 | 32 s | 48 | 0,0 | 7,3 |
| Tester QA | 4. IV. Psiarnia Piekieł | 20 | 20 | 114 s | 2 | 0,0 | 44,8 |
| Tester QA | 5. V. Psiarnia Piekieł | 20 | 20 | 101 s | 4 | 0,1 | 40,3 |
| Tester QA | 6. VI. Pan Sfory | 22 | 20 | 50 s | 111 | 0,5 | 5,1 |
| Tester QA | 7. VII. Zatopione Krużganki | 20 | 20 | 104 s | 31 | 0,2 | 40,5 |
| Tester QA | 8. VIII. Zatopione Krużganki | 24 | 19 | 135 s | 155 | 0,5 | 49,9 |
| Tester QA | 9. IX. Królowa Topieli | 20 | 19 | 108 s | 346 | 1,7 | 2,9 |
| Tester QA | 10. X. Rzeźnia | 39 | 12 | 118 s | 421 | 0,8 | 39,8 |
| Tester QA | 11. XI. Rzeźnia | 17 | 10 | 130 s | 271 | 0,6 | 48,9 |
| Tester QA | 12. XII. Rzeźnik | 10 | 10 | 58 s | 54 | 0,3 | 5,8 |
| Tester QA | 13. XIII. Kamienne Trzewia | 12 | 10 | 136 s | 240 | 0,7 | 58,9 |
| Tester QA | 14. XIV. Kamienne Trzewia | 18 | 7 | 149 s | 448 | 0,5 | 47,8 |
| Tester QA | 15. XV. Tytan Otchłani | 16 | 5 | 67 s | 870 | 2,6 | 1,3 |
| Tester QA | 16. XVI. Wieża Burz | 9 | 4 | 162 s | 494 | 0,4 | 56,6 |
| Tester QA | 17. XVII. Wieża Burz | 4 | 4 | 155 s | 52 | 0,0 | 79,8 |
| Tester QA | 18. XVIII. Matka Burz | 4 | 4 | 88 s | 347 | 1,0 | 3,0 |
| Tester QA | 19. XIX. Kostnica | 4 | 4 | 164 s | 40 | 0,0 | 95,0 |
| Tester QA | 20. XX. Kostnica | 4 | 4 | 169 s | 238 | 0,5 | 100,0 |
| Tester QA | 21. XXI. Król Kości | 4 | 4 | 53 s | 61 | 0,0 | 8,0 |
| Tester QA | 22. XXII. Chór Cieni | 6 | 3 | 188 s | 485 | 0,5 | 80,7 |
| Tester QA | 23. XXIII. Chór Cieni | 3 | 3 | 182 s | 17 | 0,0 | 108,7 |
| Tester QA | 24. XXIV. Bezimienny Cień | 3 | 3 | 79 s | 130 | 0,0 | 3,0 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 4 | 3 | 164 s | 416 | 0,8 | 103,3 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 3 | 3 | 187 s | 351 | 0,0 | 117,0 |
| Tester QA | 27. XXVII. Kowal Piekieł | 5 | 3 | 89 s | 606 | 1,2 | 1,2 |
| Tester QA | 28. XXVIII. Tron Arcydemona | 6 | 3 | 196 s | 595 | 0,8 | 108,5 |
| Tester QA | 29. XXIX. Tron Arcydemona | 3 | 3 | 179 s | 107 | 0,0 | 126,7 |
| Tester QA | 30. XXX. Arcydiabeł | 7 | 2 | 109 s | 631 | 0,6 | 4,9 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 101 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 20 | 115 s | 0 | 0,0 | 41,2 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 20 | 20 | 15 s | 1 | 0,0 | 6,7 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 20 | 20 | 114 s | 0 | 0,1 | 44,3 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 20 | 20 | 95 s | 3 | 0,0 | 40,6 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 20 | 20 | 35 s | 33 | 0,1 | 6,3 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 20 | 20 | 103 s | 8 | 0,0 | 40,6 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 22 | 20 | 132 s | 118 | 0,2 | 50,9 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 20 | 20 | 44 s | 49 | 0,1 | 2,9 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 26 | 19 | 111 s | 123 | 0,1 | 55,7 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 28 | 17 | 130 s | 393 | 0,7 | 50,0 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 17 | 17 | 34 s | 1 | 0,0 | 5,2 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 26 | 15 | 146 s | 405 | 0,7 | 58,8 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 21 | 13 | 142 s | 325 | 0,8 | 59,5 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 18 | 12 | 49 s | 629 | 2,3 | 2,4 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 20 | 10 | 155 s | 321 | 0,4 | 63,9 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 12 | 10 | 164 s | 225 | 0,5 | 82,1 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 10 | 10 | 62 s | 137 | 0,6 | 2,6 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 15 | 9 | 161 s | 256 | 0,3 | 82,3 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 12 | 8 | 164 s | 421 | 0,5 | 83,9 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 8 | 8 | 47 s | 144 | 0,4 | 15,0 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 9 | 8 | 167 s | 115 | 0,1 | 105,3 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 8 | 8 | 177 s | 54 | 0,1 | 103,3 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 8 | 8 | 85 s | 0 | 0,3 | 4,0 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 9 | 8 | 183 s | 131 | 0,1 | 106,8 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 11 | 7 | 191 s | 269 | 0,3 | 101,2 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 13 | 5 | 71 s | 732 | 1,8 | 0,9 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 7 | 4 | 186 s | 427 | 0,4 | 118,0 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 4 | 4 | 171 s | 61 | 0,3 | 121,3 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 4 | 4 | 78 s | 567 | 1,0 | 9,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 6 ×2, piętro 8 ×4, piętro 9 ×1, piętro 10 ×20, piętro 11 ×5, piętro 13 ×2, piętro 14 ×8, piętro 15 ×9, piętro 16 ×4, piętro 22 ×2, piętro 25 ×1, piętro 27 ×2, piętro 28 ×3, piętro 30 ×4; wyzwań (3 porażki na piętrze): 3 – piętro 11, piętro 30.
**Gracz H&S / souls-like** – ponowne próby: piętro 8 ×2, piętro 10 ×6, piętro 11 ×9, piętro 13 ×9, piętro 14 ×6, piętro 15 ×5, piętro 16 ×8, piętro 17 ×2, piętro 19 ×5, piętro 20 ×3, piętro 22 ×1, piętro 25 ×1, piętro 26 ×3, piętro 27 ×6, piętro 28 ×2; wyzwań (3 porażki na piętrze): 7 – piętro 14, piętro 16, piętro 19, piętro 20, piętro 27, piętro 28.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Miotacz głazów | 718 | 2,5 s | 5,5 | 2364 | 31176 | 42 | 9 |
| Tytan Otchłani | 17 | 49,5 s | 183,4 | 611 | 25237 | 1485 | 3 |
| Wiedźma wichru | 1353 | 1,3 s | 3,2 | 3665 | 12939 | 9 | 2 |
| Kowal Piekieł | 8 | 72,9 s | 352,1 | 521 | 12434 | 1554 | 2 |
| Kultysta topieli | 1552 | 1,4 s | 2,9 | 1738 | 10093 | 6 | 4 |
| Grzebacz | 574 | 2,3 s | 7,2 | 625 | 8151 | 14 | 0 |
| Królowa Topieli | 39 | 69,8 s | 95,9 | 1139 | 7577 | 194 | 0 |
| Ogar otchłani | 2474 | 0,8 s | 2,6 | 1397 | 7332 | 3 | 1 |
| Arcydiabeł | 6 | 81,6 s | 350,3 | 444 | 6686 | 1114 | 1 |
| Harpia | 1104 | 1,1 s | 3,4 | 1597 | 5270 | 5 | 2 |
| Kolos | 1485 | 3,4 s | 8,3 | 114 | 4831 | 3 | 2 |
| Legionista | 1373 | 4,9 s | 11,4 | 556 | 4250 | 3 | 2 |
| Taranowiec | 1517 | 1,9 s | 5,3 | 373 | 3776 | 2 | 2 |
| Kultysta | 2539 | 1,0 s | 2,4 | 643 | 3421 | 1 | 1 |
| Cień | 793 | 1,0 s | 3,4 | 291 | 3184 | 4 | 1 |
| Kolos żaru | 685 | 3,8 s | 15,6 | 94 | 2763 | 4 | 0 |
| Matka Burz | 14 | 64,2 s | 199,9 | 392 | 2762 | 197 | 0 |
| Pan Sfory | 40 | 37,6 s | 47,0 | 474 | 2428 | 61 | 0 |
| Gnilec | 814 | 3,7 s | 10,7 | 240 | 1991 | 2 | 0 |
| Czerw | 6190 | 0,4 s | 1,5 | 431 | 1873 | 0 | 0 |
| Upiór | 423 | 2,3 s | 7,1 | 635 | 1823 | 4 | 0 |
| Gnilec (odłamek) | 1614 | 1,6 s | 4,1 | 275 | 1783 | 1 | 0 |
| Siarkowy pomiot | 1700 | 0,5 s | 2,3 | 46 | 1648 | 1 | 0 |
| Tkaczka | 402 | 2,2 s | 5,7 | 464 | 1633 | 4 | 0 |
| Król Kości | 12 | 44,3 s | 190,2 | 132 | 1397 | 116 | 0 |
| Nekromanta | 276 | 2,0 s | 7,3 | 156 | 1209 | 4 | 0 |
| Gargulec | 476 | 2,9 s | 9,1 | 120 | 980 | 2 | 0 |
| Władca Bramy | 40 | 18,8 s | 27,7 | 226 | 960 | 24 | 0 |
| Pajączek | 331 | 0,3 s | 1,2 | 174 | 928 | 3 | 1 |
| Ognisty chochlik | 1033 | 1,3 s | 3,6 | 71 | 812 | 1 | 0 |
| Ofiarnik | 469 | 3,3 s | 4,6 | 557 | 760 | 2 | 0 |
| Rzeźnik | 27 | 38,2 s | 96,6 | 135 | 557 | 21 | 0 |
| Ożywiony nietoperz | 828 | 0,2 s | 1,4 | 84 | 521 | 1 | 0 |
| Bezimienny Cień | 11 | 78,6 s | 315,8 | 231 | 390 | 35 | 0 |
| Chochlik wichru | 853 | 0,8 s | 2,4 | 40 | 376 | 0 | 0 |
| Kamienny chochlik | 916 | 0,8 s | 2,4 | 47 | 321 | 0 | 0 |
| Wodny chochlik | 898 | 0,7 s | 2,2 | 37 | 303 | 0 | 0 |
| Ognisty chochlik (elita) | 66 | 4,9 s | 13,6 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 69 | 3,8 s | 8,1 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 60 | 2,6 s | 7,2 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 45 | 14,7 s | 29,7 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 56 | 10,8 s | 37,4 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 19 | 4,3 s | 14,1 | ↑ | ↑ | ↑ |  |
| Worek jaj | 232 | 0,8 s | 1,7 | 0 | 0 | 0 | 0 |
| Kultysta topieli (elita) | 44 | 6,6 s | 9,7 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 43 | 2,2 s | 4,2 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 19 | 6,9 s | 30,1 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 11 | 11,6 s | 16,7 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 44 | 6,9 s | 13,5 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 47 | 5,6 s | 20,8 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 28 | 4,9 s | 10,5 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 24 | 10,5 s | 32,9 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 17 | 6,4 s | 25,5 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 23 | 9,0 s | 21,7 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 50 | 5,4 s | 9,7 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 12 | 8,6 s | 34,5 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 21 | 17,1 s | 82,9 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 7 | 5,0 s | 21,4 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 9 | 8,3 s | 29,7 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2757 | 53% | 0% | 0% | 47% | 0% |
| ciężki | 312 | 39% | 0% | 0% | 61% | 0% |
| nieparowalny | 2100 | 44% | 0% | 0% | 56% | 0% |
| nieblokowalny | 195 | 68% | 0% | 0% | 32% | 0% |
| obszarowy | 3922 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3775 | 34% | 0% | 0% | 66% | 0% |
| ciężki | 354 | 28% | 0% | 0% | 72% | 0% |
| nieparowalny | 2169 | 28% | 0% | 0% | 72% | 0% |
| nieblokowalny | 190 | 48% | 0% | 0% | 52% | 0% |
| obszarowy | 5365 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2889 | 25% | 24% | 4% | 20% | 26% |
| Tester QA (0,30 s) | ciężki | 622 | 0% | 1% | 4% | 21% | 73% |
| Tester QA (0,30 s) | nieparowalny | 1289 | 4% | 3% | 9% | 33% | 51% |
| Tester QA (0,30 s) | nieblokowalny | 54 | 4% | 0% | 4% | 24% | 69% |
| Tester QA (0,30 s) | obszarowy | 1975 | 2% | 1% | 4% | 39% | 54% |
| Tester QA (0,30 s) | pocisk | 3168 | 2% | 10% | 7% | 14% | 67% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 4575 | 9% | 16% | 20% | 9% | 46% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 700 | 1% | 1% | 1% | 23% | 74% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1381 | 6% | 2% | 2% | 15% | 74% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 64 | 2% | 0% | 0% | 6% | 92% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 2793 | 2% | 2% | 5% | 31% | 60% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 4377 | 2% | 7% | 7% | 11% | 73% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 875 |
| Pajączek: Pajączek | 0,31 s | 364 |
| Kamienny chochlik: Pazury | 0,33 s | 174 |
| Ognisty chochlik: Pazury | 0,33 s | 253 |
| Chochlik wichru: Pazury | 0,33 s | 161 |
| Czerw: Ugryzienie | 0,33 s | 1963 |
| Wodny chochlik: Pazury | 0,33 s | 154 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 304 |
| Władca Bramy: Cięcie w szale | 0,41 s | 13 |
| Ognisty chochlik: Drugie cięcie | 0,43 s | 6 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 83% (n=144) | 82% (n=39) | 91% (n=132) | 100% (n=4) | 100% (n=92) | 66% (n=115) |
| niezauważony | 85% (n=435) | – | 86% (n=154) | 94% (n=77) | 100% (n=2118) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 81% (n=181) |
| unik | 36% (n=920) | 23% (n=169) | 18% (n=895) | 34% (n=70) | 100% (n=45) | 26% (n=378) |
| unik (eksperyment) | 37% (n=207) | 8% (n=39) | 28% (n=173) | 0% (n=9) | 100% (n=185) | 32% (n=74) |
| wyjście z zasięgu | 65% (n=188) | 64% (n=36) | 63% (n=136) | 80% (n=5) | 100% (n=1146) | 48% (n=248) |
| wyjście z zasięgu (eksperyment) | 84% (n=262) | 86% (n=29) | 89% (n=240) | 93% (n=30) | 100% (n=180) | 57% (n=131) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 190,8 | reakcja na cios wręcz 171,8, unik przed pociskiem 61,2, wyjście z okrążenia 8,7, wabienie pod czar obszarowy 6,3 | drgnięcie (trafiony) 18,3, w trakcie uniku 17,7, brak wytrzymałości 7,9, picie flaszki 2,8, unieruchomienie 0,6, inna 0,1 | 73% (n=3436) / 90% (n=1223) |
| Gracz H&S / souls-like | 276,2 | reakcja na cios wręcz 261,1, unik przed pociskiem 96,6, doskok do odsłoniętego celu 17,9, wabienie pod czar obszarowy 5,5, wyjście z okrążenia 4,7 | w trakcie uniku 50,4, drgnięcie (trafiony) 19,3, brak wytrzymałości 16,2, picie flaszki 3,5, unieruchomienie 1,2, inna 0,2, szybki atak 0,1 | 78% (n=5222) / 93% (n=1932) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Ziemia | Nova ziemi | 5 | 1 | 20,6 | 939,0 |
| Tester QA | Ogień | Nova ognia | 5 | 1 | 17,2 | 721,4 |
| Tester QA | Woda | Nova wody | 5 | 0 | 11,4 | 360,4 |
| Tester QA | Powietrze | Nova powietrza | 5 | 0 | 10,8 | 332,4 |
| Gracz H&S / souls-like | Ogień | Nova ognia | 12 | 4 | 23,5 | 1124,5 |
| Gracz H&S / souls-like | Woda | Nova wody | 3 | 0 | 17,3 | 735,3 |
| Gracz H&S / souls-like | Ziemia | Nova ziemi | 3 | 0 | 13,3 | 465,3 |
| Gracz H&S / souls-like | Powietrze | Nova powietrza | 2 | 0 | 13,0 | 290,5 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka ziemi | 2046958 | 41% |  |  |
| Różdżka ognia | 1353843 | 27% |  |  |
| Różdżka wody | 275102 | 5% |  |  |
| Różdżka powietrza | 246497 | 5% |  |  |
| Ściana ognia | 201009 | 4% | 758 | 265 |
| Nova ziemi | 189652 | 4% | 559 | 339 |
| Nova powietrza | 163461 | 3% | 506 | 323 |
| Odskok widma | 139039 | 3% |  |  |
| Nova ognia | 138010 | 3% | 410 | 337 |
| Nova wody | 102337 | 2% | 366 | 280 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Przeciążenie ×1512, Kamienna osłona ×82, Kojące światło ×6, Krwawy pakt ×34, Płomienne ostrze ×53.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka ognia | 3899763 | 48% |  |  |
| Płonąca ziemia | 1713305 | 21% | 1123 | 1526 |
| Nova ognia | 775202 | 10% | 1034 | 750 |
| Różdżka wody | 732404 | 9% |  |  |
| Różdżka ziemi | 216631 | 3% |  |  |
| Odskok widma | 201850 | 2% |  |  |
| Różdżka powietrza | 174838 | 2% |  |  |
| Lodowy podmuch | 124210 | 2% | 554 | 224 |
| Salwa | 52284 | 1% | 3 | 17428 |
| Kula ognia | 42910 | 1% | 165 | 260 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Przeciążenie ×1165, Krwawy pakt ×5.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,4, mediana 4,0, najwięcej 6 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 30% | 79,4 | 186 | 0,5 |
| klasa | 29% | 75,6 | – | 0,2 |
| sklep | 6% | 16,2 | 166 | 0,8 |
| przedmiot | 35% | 92,1 | 322 | 0,6 |

Oferty sklepu dusz: widziane 840, kupione 9, odrzucone 0, za mało dusz 271.
Zamiany w slotach: 4,2 na podejście; najczęstsze: Ściana ognia → Pocisk arkanów ×25, Pocisk arkanów → Ściana ognia ×20, Ściana ognia → Nova ognia ×6, Nova ognia → Ściana ognia ×6, Nova ognia → Kamienna osłona ×3, Pocisk arkanów → Nova ognia ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Przeciążenie | klasa | 20 | 75,6 | – | 0,2 |
| Pocisk arkanów | odblokowanie | 20 | 41,5 | 113 | 0,6 |
| Ściana ognia | odblokowanie | 20 | 37,9 | 265 | 0,4 |
| Nova ziemi | przedmiot | 5 | 111,8 | 339 | 0,6 |
| Nova powietrza | przedmiot | 5 | 101,2 | 323 | 0,6 |
| Nova ognia | przedmiot | 5 | 82,0 | 337 | 0,5 |
| Nova wody | przedmiot | 5 | 73,2 | 280 | 0,7 |
| Meteor | sklep | 1 | 105,0 | 461 | 0,6 |
| Kamienna osłona | sklep | 1 | 82,0 | – | 0,6 |
| Płomienne ostrze | sklep | 1 | 53,0 | – | 0,5 |
| Iskra | sklep | 2 | 19,5 | 102 | 1,5 |
| Krwawy pakt | sklep | 1 | 34,0 | – | 0,6 |
| Kojące światło | sklep | 1 | 6,0 | – | 4,0 |
| Piekielna kula | sklep | 1 | 4,0 | 312 | 2,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,2, mediana 5,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 47% | 105,3 | 912 | 0,5 |
| klasa | 26% | 58,3 | – | 0,2 |
| sklep | 3% | 7,6 | 1155 | 2,0 |
| przedmiot | 24% | 53,4 | 730 | 0,4 |

Oferty sklepu dusz: widziane 1140, kupione 19, odrzucone 3, za mało dusz 358.
Zamiany w slotach: 3,7 na podejście; najczęstsze: Kojące światło → Kula ognia ×12, Pocisk arkanów → Płonąca ziemia ×10, Pocisk arkanów → Kula ognia ×4, Nova ziemi → Płonąca ziemia ×3, Kula ognia → Salwa ×3, Kojące światło → Lodowy podmuch ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Przeciążenie | klasa | 20 | 58,3 | – | 0,2 |
| Płonąca ziemia | odblokowanie | 17 | 66,1 | 1526 | 0,6 |
| Nova ognia | przedmiot | 12 | 86,2 | 750 | 0,4 |
| Lodowy podmuch | odblokowanie | 7 | 79,1 | 224 | 0,3 |
| Kula ognia | odblokowanie | 16 | 10,3 | 260 | 0,4 |
| Pocisk arkanów | odblokowanie | 4 | 40,0 | 99 | 0,6 |
| Ściana ognia | odblokowanie | 1 | 42,0 | 292 | 0,0 |
| Sfora | sklep | 1 | 27,0 | 915 | 1,0 |
| Iskra | sklep | 1 | 22,0 | 354 | 4,0 |
| Włócznia potępionych | odblokowanie | 1 | 21,0 | 191 | 0,9 |
| Nova powietrza | przedmiot | 1 | 17,0 | 133 | 0,4 |
| Lodowa włócznia | sklep | 1 | 16,0 | 176 | 2,0 |
| Wyrok burzy | sklep | 1 | 14,0 | 2621 | 2,0 |
| Deszcz strzał | sklep | 1 | 14,0 | 1142 | 2,0 |
| Łańcuch błyskawic | odblokowanie | 1 | 13,0 | 107 | 0,0 |
| Przyzwanie chochlików | sklep | 1 | 13,0 | 588 | 1,0 |
| Oko cyklonu | sklep | 1 | 10,0 | 303 | 1,0 |
| Nova ziemi | przedmiot | 1 | 10,0 | 62 | 0,8 |
| Łańcuchy potępionych | sklep | 1 | 10,0 | 212 | 2,0 |
| Lodowy odłamek | sklep | 1 | 9,0 | 856 | 1,0 |
| Meteor | sklep | 1 | 7,0 | 1248 | 4,0 |
| Nova wody | przedmiot | 1 | 6,0 | 77 | 0,0 |
| Szarża | odblokowanie | 1 | 6,0 | 43 | 0,0 |
| Burza | odblokowanie | 1 | 6,0 | 641 | 0,3 |
| Piekielna kula | sklep | 3 | 2,0 | 898 | 1,7 |
| Krwawy pakt | odblokowanie | 1 | 5,0 | – | 0,0 |
| Mroźna fala | odblokowanie | 1 | 5,0 | 243 | 0,0 |
| Rozprysk arkanów | odblokowanie | 1 | 5,0 | 111 | 0,0 |
| Salwa | sklep | 2 | 1,5 | 17428 | 0,7 |
| Strzał przeszywający | sklep | 1 | 1,0 | 694 | 2,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 884,8, czar obszarowy na jedynego wroga 586,3, umiejętność 263,2, unik 190,8, czar obszarowy na grupę 185,4, drgnięcie (trafiony) 92,3, czar obszarowy na jednego (test) 38,6, flaszka (próba) 25,3, czar obszarowy na jednego (ratunek) 15,1, flaszka many (próba) 9,9, wabienie bez skutku 8,6, czar obszarowy po zwabieniu 3,5.
Czas w bloku 0 s, w biegu 66 s, bez wytrzymałości (<10%) 112 s na podejście.
Zakupy: dodatkowa flaszka ×24, ulepszenie: Ściana ognia ×15, ulepszenie: Pocisk arkanów ×15, sklep: Jedwabne rękawiczki ×11, sklep: Świszczące ostrze ×10, sklep: Nurt ×9, ulepszenie: Nova ognia ×9, sklep: Sandały pielgrzyma ×9.
Punkty cech: Intelligence 83, Toughness 69, Dexterity 65, Strength 63.
Pieczęcie na podejście: z daru bossa 4,4, z sakwy do skilli 3,0, kupione 0,3; najczęstsze: Deszcz piekieł ×12, Serce otchłani ×11, Klątwa Mokoszy ×11, Pakt z diabłem ×10, Nieskończoność ×9, Wieczny płomień ×8.
Komunikaty gry: „Sloty przegrzane” ×21723; „PRZECIĄŻENIE” ×1467; „WYŁADOWANIE!” ×1127; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×256; „Portale gasną – dobij pozostałe demony” ×202; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×90; „OGŁUSZENIE!” ×87; „POLEGŁEŚ” ×85.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 1227,0, unik 276,2, umiejętność 224,5, czar obszarowy na jedynego wroga 184,7, drgnięcie (trafiony) 80,2, czar obszarowy na grupę 43,4, flaszka (próba) 23,2, wabienie bez skutku 11,9, flaszka many (próba) 7,6, czar obszarowy po zwabieniu 2,9, czar obszarowy na jednego (ratunek) 2,8.
Czas w bloku 0 s, w biegu 83 s, bez wytrzymałości (<10%) 149 s na podejście.
Zakupy: dodatkowa flaszka ×32, sklep: Szybkie ręce ×30, sklep: Twarda skóra ×29, sklep: Przebicie ×28, sklep: Szeroki zasięg ×26, sklep: Skupienie ×24, sklep: Refleks ×23, sklep: Furia ×22.
Punkty cech: Intelligence 260, Toughness 120.
Pieczęcie na podejście: z daru bossa 6,0, z sakwy do skilli 3,0, kupione 0,1; najczęstsze: Pakt z diabłem ×24, Wieczny płomień ×12, Serce otchłani ×11, Oko Czarnoboga ×11, Deszcz piekieł ×9, Klątwa Mokoszy ×7.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Kontra, Mroźna fala, Krwawy pakt, Puklerz, Mroźna zbroja, Łańcuch błyskawic, Sztylet parujący, Rozprysk arkanów, Płonąca ziemia, Burza, Łuk refleksyjny, Kostur burzy, Pawęż, Przetrwanie, Szata popiołu, Kosa, Wielki topór, Młot burzy, Kojące światło, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Włócznia potępionych.
Komunikaty gry: „Sloty przegrzane” ×7210; „PRZECIĄŻENIE” ×1116; „WYŁADOWANIE!” ×846; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×330; „Portale gasną – dobij pozostałe demony” ×273; „Garda wroga przełamana!” ×190; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×124; „OGŁUSZENIE!” ×123.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×1031, uśpiony wróg blokuje koniec piętra ×101, zablokowany ruch ×12.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 13 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 7 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 4 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×1411, uśpiony wróg blokuje koniec piętra ×162, zablokowany ruch ×7, limit czasu piętra ×1.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 12 podejściach)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - piętro 19 „XIX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 20 „XX. Kostnica”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść (w 6 podejściach)
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść (w 5 podejściach)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** zablokowany ruch średnio 0,6× na podejście (geometria areny/kolizje). Przykłady: piętro 9 „IX. Królowa Topieli”: brak postępu przy (0,0; -0,7); piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (7,8; -0,6); piętro 22 „XXII. Chór Cieni”: brak postępu przy (-6,3; 1,1).
- **Czytelność (P2):** 16% trafień w bohatera przyszło z tyłu (>110° od kierunku patrzenia). W grupach brakuje sygnału ataku spoza kadru.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 30% ciosów (n=79).
- Unik przed ciężkim: działa – rani tylko 20% ciosów (n=208).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=1326) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 10% pocisków, przed którymi był unik (n=1223).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 49% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2889) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=622).
- Telegraf „nieparowalny” daje czas: tylko 7% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1289).
- Telegraf „nieblokowalny” daje czas: tylko 4% decyzji zapada mniej niż 0,1 s przed trafieniem (n=54).
- Telegraf „obszarowy” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1975).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- UX: gra odmawiała akcji: „Sloty przegrzane” ×21723, „Za mało zdrowia” ×6 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XVI. Wieża Burz” trwa średnio 162 s – najdłuższy fragment wieży.
- Tempo: piętro „XVII. Wieża Burz” trwa średnio 155 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 164 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 169 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 188 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 182 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 164 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVI. Kuźnia Piekieł” trwa średnio 187 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVIII. Tron Arcydemona” trwa średnio 196 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIX. Tron Arcydemona” trwa średnio 179 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 133 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 4/20. Trudno, ale do zrobienia – tak ma być w souls-like.
- Czuję progres: druga połowa podejść średnio piętro 23,6 wobec 16,8 na początku (popiół, odblokowania, wybór zestawu).
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Matka Burz (224,8 trafień, 56,6 s), Tytan Otchłani (204,3 trafień, 44,2 s), Rzeźnik (102,3 trafień, 29,3 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,2), Ożywiony nietoperz (1,6), Czerw (1,6) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Tytan Otchłani – średnio 44 s i 204 trafień do zabicia.
- Długie starcie: Matka Burz – średnio 57 s i 225 trafień do zabicia.
- Długie starcie: Król Kości – średnio 42 s i 197 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 80 s i 344 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 66 s i 394 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 72 s i 421 trafień do zabicia.
- 13/15 razy zginąłem z flaszkami w kieszeni. 7 z tych śmierci to utrata ≥50% HP w 3 s, a 61% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 35% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Wyrok burzy”: 2621 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 3%. Różnych skilli na podejście: średnio 5,2.
- Najczęściej grałem: Ogień + Nova ognia (12×, śr. piętro 23,8). Inne: Woda + Nova wody 3× (17,3), Ziemia + Nova ziemi 3× (13,3), Powietrze + Nova powietrza 2× (13,0).
- Unik to moje główne narzędzie: 276,2 na podejście, w tym 22,6 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 93% przypadków.
- Gra nie wykonała uniku od razu średnio 90,8× na podejście – najczęściej: w trakcie poprzedniego uniku 50,4, drgnięcie (trafiony) 19,3, brak wytrzymałości 16,2. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (80 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

