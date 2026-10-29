# Raport botów grających — Mag

Wygenerowano: 2026-10-29 10:14. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (3 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (3 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 3 | 1 (33%) | 20,0 | 16,0 | 39,1 min | 881,7 | 354200 / 2608 | 6,7 | 2% |
| Gracz H&S / souls-like | 3 | 1 (33%) | 24,3 | 27,0 | 47,6 min | 1185,3 | 521314 / 3926 | 12,3 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 16 14 ★
- Gracz H&S / souls-like: 27 16 ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 14. XIV. Kamienne Trzewia ×1, 16. XVI. Wieża Burz ×1.
  Ostatni cios: Harpia – „Harpia” ×1; Grzebacz – „Grzebacz: StoneSpikes” ×1. Śmierć ciosem ≥40% maks. HP: 0/2. Zginął mając flaszki: 2/2.
  Ostatnie 3 s przed śmiercią: średnio 33% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/2). Picie: 72 prób, 20 wypitych (72% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 16. XVI. Wieża Burz ×1, 27. XXVII. Kowal Piekieł ×1.
  Ostatni cios: Kowal Piekieł – „Kowal Piekieł: StoneSpikes” ×1; Ożywiony nietoperz – „Ukąszenie” ×1. Śmierć ciosem ≥40% maks. HP: 0/2. Zginął mając flaszki: 2/2.
  Ostatnie 3 s przed śmiercią: średnio 50% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/2). Picie: 77 prób, 37 wypitych (52% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 3 | 3 | 103 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 3 | 3 | 118 s | 0 | 0,0 | 40,0 |
| Tester QA | 3. III. Władca Bramy | 3 | 3 | 31 s | 37 | 0,0 | 9,0 |
| Tester QA | 4. IV. Psiarnia Piekieł | 3 | 3 | 99 s | 8 | 0,0 | 46,7 |
| Tester QA | 5. V. Psiarnia Piekieł | 3 | 3 | 110 s | 0 | 0,0 | 37,7 |
| Tester QA | 6. VI. Pan Sfory | 3 | 3 | 51 s | 0 | 0,0 | 4,3 |
| Tester QA | 7. VII. Zatopione Krużganki | 3 | 3 | 94 s | 3 | 0,0 | 37,0 |
| Tester QA | 8. VIII. Zatopione Krużganki | 4 | 3 | 119 s | 223 | 0,5 | 43,0 |
| Tester QA | 9. IX. Królowa Topieli | 3 | 3 | 122 s | 196 | 0,7 | 3,0 |
| Tester QA | 10. X. Rzeźnia | 3 | 3 | 110 s | 0 | 0,0 | 63,0 |
| Tester QA | 11. XI. Rzeźnia | 3 | 3 | 130 s | 126 | 0,3 | 65,7 |
| Tester QA | 12. XII. Rzeźnik | 3 | 3 | 54 s | 0 | 0,0 | 1,0 |
| Tester QA | 13. XIII. Kamienne Trzewia | 3 | 3 | 132 s | 86 | 0,7 | 71,0 |
| Tester QA | 14. XIV. Kamienne Trzewia | 5 | 2 | 133 s | 533 | 1,2 | 45,8 |
| Tester QA | 15. XV. Tytan Otchłani | 2 | 2 | 59 s | 416 | 2,0 | 2,0 |
| Tester QA | 16. XVI. Wieża Burz | 4 | 1 | 143 s | 248 | 0,3 | 49,8 |
| Tester QA | 17. XVII. Wieża Burz | 1 | 1 | 149 s | 0 | 0,0 | 89,0 |
| Tester QA | 18. XVIII. Matka Burz | 1 | 1 | 187 s | 21 | 0,0 | 3,0 |
| Tester QA | 19. XIX. Kostnica | 1 | 1 | 171 s | 171 | 0,0 | 108,0 |
| Tester QA | 20. XX. Kostnica | 1 | 1 | 161 s | 221 | 0,0 | 91,0 |
| Tester QA | 21. XXI. Król Kości | 1 | 1 | 63 s | 0 | 0,0 | 15,0 |
| Tester QA | 22. XXII. Chór Cieni | 1 | 1 | 187 s | 129 | 0,0 | 96,0 |
| Tester QA | 23. XXIII. Chór Cieni | 1 | 1 | 172 s | 75 | 0,0 | 97,0 |
| Tester QA | 24. XXIV. Bezimienny Cień | 1 | 1 | 81 s | 0 | 0,0 | 4,0 |
| Tester QA | 25. XXV. Kuźnia Piekieł | 1 | 1 | 210 s | 25 | 0,0 | 112,0 |
| Tester QA | 26. XXVI. Kuźnia Piekieł | 1 | 1 | 188 s | 31 | 0,0 | 113,0 |
| Tester QA | 27. XXVII. Kowal Piekieł | 1 | 1 | 51 s | 393 | 2,0 | 1,0 |
| Tester QA | 28. XXVIII. Tron Arcydemona | 1 | 1 | 195 s | 0 | 0,0 | 126,0 |
| Tester QA | 29. XXIX. Tron Arcydemona | 1 | 1 | 184 s | 0 | 0,0 | 121,0 |
| Tester QA | 30. XXX. Arcydiabeł | 1 | 1 | 86 s | 16 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 3 | 3 | 101 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 3 | 3 | 115 s | 0 | 0,0 | 40,3 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 3 | 3 | 25 s | 11 | 0,0 | 5,0 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 3 | 3 | 112 s | 0 | 0,0 | 41,0 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 3 | 3 | 95 s | 0 | 0,0 | 43,0 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 3 | 3 | 36 s | 47 | 0,3 | 5,7 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 3 | 3 | 104 s | 0 | 0,0 | 40,0 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 4 | 3 | 133 s | 95 | 0,0 | 50,5 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 3 | 3 | 68 s | 67 | 0,0 | 3,0 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 3 | 3 | 115 s | 7 | 0,0 | 66,3 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 4 | 3 | 134 s | 193 | 0,8 | 56,5 |
| Gracz H&S / souls-like | 12. XII. Rzeźnik | 3 | 3 | 33 s | 0 | 0,0 | 3,0 |
| Gracz H&S / souls-like | 13. XIII. Kamienne Trzewia | 3 | 3 | 125 s | 63 | 0,0 | 64,7 |
| Gracz H&S / souls-like | 14. XIV. Kamienne Trzewia | 3 | 3 | 133 s | 107 | 0,3 | 75,7 |
| Gracz H&S / souls-like | 15. XV. Tytan Otchłani | 3 | 3 | 32 s | 155 | 0,3 | 3,0 |
| Gracz H&S / souls-like | 16. XVI. Wieża Burz | 5 | 2 | 159 s | 509 | 1,2 | 63,0 |
| Gracz H&S / souls-like | 17. XVII. Wieża Burz | 3 | 2 | 143 s | 183 | 0,7 | 59,3 |
| Gracz H&S / souls-like | 18. XVIII. Matka Burz | 2 | 2 | 96 s | 338 | 1,5 | 3,0 |
| Gracz H&S / souls-like | 19. XIX. Kostnica | 2 | 2 | 144 s | 0 | 0,5 | 91,0 |
| Gracz H&S / souls-like | 20. XX. Kostnica | 2 | 2 | 166 s | 0 | 1,0 | 92,5 |
| Gracz H&S / souls-like | 21. XXI. Król Kości | 2 | 2 | 52 s | 352 | 2,0 | 12,0 |
| Gracz H&S / souls-like | 22. XXII. Chór Cieni | 2 | 2 | 166 s | 0 | 0,0 | 99,5 |
| Gracz H&S / souls-like | 23. XXIII. Chór Cieni | 2 | 2 | 168 s | 0 | 0,0 | 106,5 |
| Gracz H&S / souls-like | 24. XXIV. Bezimienny Cień | 2 | 2 | 87 s | 28 | 1,0 | 4,0 |
| Gracz H&S / souls-like | 25. XXV. Kuźnia Piekieł | 2 | 2 | 194 s | 0 | 0,0 | 110,5 |
| Gracz H&S / souls-like | 26. XXVI. Kuźnia Piekieł | 3 | 2 | 213 s | 359 | 0,7 | 92,7 |
| Gracz H&S / souls-like | 27. XXVII. Kowal Piekieł | 4 | 1 | 57 s | 900 | 2,3 | 0,3 |
| Gracz H&S / souls-like | 28. XXVIII. Tron Arcydemona | 1 | 1 | 178 s | 47 | 0,0 | 119,0 |
| Gracz H&S / souls-like | 29. XXIX. Tron Arcydemona | 1 | 1 | 195 s | 0 | 0,0 | 138,0 |
| Gracz H&S / souls-like | 30. XXX. Arcydiabeł | 1 | 1 | 70 s | 0 | 0,0 | 1,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 8 ×1, piętro 14 ×2, piętro 16 ×2; wyzwań (3 porażki na piętrze): 1 – piętro 16.
**Gracz H&S / souls-like** – ponowne próby: piętro 8 ×1, piętro 11 ×1, piętro 16 ×2, piętro 17 ×1, piętro 26 ×1, piętro 27 ×2; wyzwań (3 porażki na piętrze): 1 – piętro 27.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kowal Piekieł | 2 | 49,7 s | 147,0 | 175 | 3993 | 1997 | 1 |
| Miotacz głazów | 174 | 1,9 s | 3,5 | 328 | 3519 | 20 | 0 |
| Wiedźma wichru | 272 | 1,6 s | 3,4 | 492 | 1444 | 5 | 0 |
| Tytan Otchłani | 5 | 38,1 s | 105,8 | 40 | 1296 | 259 | 0 |
| Grzebacz | 138 | 1,6 s | 3,9 | 119 | 1280 | 9 | 1 |
| Kultysta topieli | 326 | 1,1 s | 2,7 | 198 | 898 | 3 | 0 |
| Król Kości | 3 | 51,9 s | 113,0 | 48 | 704 | 235 | 0 |
| Matka Burz | 3 | 122,2 s | 225,7 | 152 | 696 | 232 | 0 |
| Ogar otchłani | 456 | 1,0 s | 2,7 | 191 | 656 | 1 | 0 |
| Królowa Topieli | 6 | 90,7 s | 155,3 | 152 | 642 | 107 | 0 |
| Harpia | 231 | 1,1 s | 3,4 | 303 | 617 | 3 | 1 |
| Taranowiec | 308 | 1,6 s | 4,4 | 44 | 579 | 2 | 0 |
| Legionista | 286 | 3,5 s | 8,9 | 62 | 432 | 1 | 0 |
| Cień | 168 | 1,2 s | 3,1 | 47 | 424 | 2 | 0 |
| Tkaczka | 75 | 2,6 s | 5,1 | 73 | 340 | 4 | 0 |
| Kultysta | 424 | 0,9 s | 2,0 | 68 | 309 | 1 | 0 |
| Siarkowy pomiot | 316 | 0,3 s | 1,8 | 8 | 277 | 1 | 0 |
| Kolos żaru | 165 | 2,7 s | 6,6 | 7 | 251 | 1 | 0 |
| Gnilec (odłamek) | 314 | 1,3 s | 3,5 | 25 | 163 | 1 | 0 |
| Władca Bramy | 6 | 23,3 s | 35,3 | 41 | 144 | 24 | 0 |
| Upiór | 107 | 2,3 s | 4,5 | 77 | 133 | 1 | 0 |
| Ożywiony nietoperz | 139 | 0,6 s | 1,2 | 21 | 131 | 1 | 1 |
| Nekromanta | 63 | 2,2 s | 3,7 | 23 | 120 | 2 | 0 |
| Pan Sfory | 6 | 39,1 s | 64,7 | 75 | 109 | 18 | 0 |
| Kamienny chochlik | 166 | 0,5 s | 2,0 | 10 | 94 | 1 | 0 |
| Gnilec | 156 | 2,7 s | 8,0 | 13 | 76 | 0 | 0 |
| Bezimienny Cień | 3 | 80,3 s | 188,0 | 39 | 57 | 19 | 0 |
| Kolos | 302 | 2,3 s | 5,1 | 5 | 50 | 0 | 0 |
| Ofiarnik | 95 | 1,2 s | 3,1 | 47 | 35 | 0 | 0 |
| Gargulec | 98 | 2,2 s | 4,7 | 11 | 33 | 0 | 0 |
| Ognisty chochlik | 138 | 1,4 s | 3,3 | 8 | 31 | 0 | 0 |
| Arcydiabeł | 2 | 72,3 s | 216,0 | 21 | 16 | 8 | 0 |
| Chochlik wichru | 140 | 1,1 s | 2,7 | 5 | 15 | 0 | 0 |
| Wodny chochlik | 147 | 0,6 s | 2,4 | 3 | 15 | 0 | 0 |
| Pajączek | 45 | 0,4 s | 1,2 | 14 | 13 | 0 | 0 |
| Czerw | 1057 | 0,5 s | 1,6 | 35 | 8 | 0 | 0 |
| Kultysta (elita) | 13 | 4,9 s | 8,8 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 14 | 4,2 s | 9,2 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 6 | 10,8 s | 22,5 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 9 | 6,5 s | 13,8 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 9 | 7,6 s | 10,9 | ↑ | ↑ | ↑ |  |
| Worek jaj | 34 | 0,9 s | 1,7 | 0 | 0 | 0 | 0 |
| Rzeźnik | 6 | 39,1 s | 128,8 | 17 | 0 | 0 | 0 |
| Ogar otchłani (elita) | 12 | 4,7 s | 10,5 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 10 | 3,0 s | 6,4 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 10 | 2,7 s | 9,0 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 8 | 10,6 s | 19,3 | ↑ | ↑ | ↑ |  |
| Miotacz głazów (elita) | 4 | 10,2 s | 8,0 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 7 | 2,9 s | 3,9 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 5 | 3,5 s | 11,2 | ↑ | ↑ | ↑ |  |
| Gnilec (elita) | 3 | 12,6 s | 31,0 | ↑ | ↑ | ↑ |  |
| Tkaczka (elita) | 6 | 3,2 s | 13,3 | ↑ | ↑ | ↑ |  |
| Grzebacz (elita) | 3 | 2,2 s | 10,0 | ↑ | ↑ | ↑ |  |
| Cień (elita) | 7 | 3,7 s | 11,1 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 3 | 4,4 s | 19,0 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 2 | 2,3 s | 3,5 | ↑ | ↑ | ↑ |  |
| Kolos żaru (elita) | 6 | 14,4 s | 30,7 | ↑ | ↑ | ↑ |  |
| Upiór (elita) | 8 | 5,9 s | 16,5 | ↑ | ↑ | ↑ |  |
| Nekromanta (elita) | 1 | 1,2 s | 7,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 423 | 41% | 0% | 0% | 59% | 0% |
| ciężki | 33 | 30% | 0% | 0% | 70% | 0% |
| nieparowalny | 315 | 34% | 0% | 0% | 66% | 0% |
| nieblokowalny | 16 | 44% | 0% | 0% | 56% | 0% |
| obszarowy | 680 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 393 | 40% | 0% | 0% | 60% | 0% |
| ciężki | 40 | 40% | 0% | 0% | 60% | 0% |
| nieparowalny | 317 | 20% | 0% | 0% | 80% | 0% |
| nieblokowalny | 39 | 54% | 0% | 0% | 46% | 0% |
| obszarowy | 741 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 438 | 22% | 17% | 4% | 20% | 37% |
| Tester QA (0,30 s) | ciężki | 83 | 0% | 2% | 4% | 25% | 69% |
| Tester QA (0,30 s) | nieparowalny | 183 | 7% | 3% | 6% | 39% | 44% |
| Tester QA (0,30 s) | nieblokowalny | 8 | 13% | 0% | 0% | 25% | 63% |
| Tester QA (0,30 s) | obszarowy | 346 | 1% | 1% | 5% | 47% | 46% |
| Tester QA (0,30 s) | pocisk | 463 | 3% | 8% | 8% | 15% | 67% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 518 | 10% | 19% | 18% | 11% | 41% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 83 | 2% | 1% | 0% | 34% | 63% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 204 | 5% | 2% | 2% | 19% | 72% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 11 | 0% | 0% | 0% | 9% | 91% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 389 | 3% | 1% | 8% | 34% | 54% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 404 | 2% | 11% | 7% | 11% | 69% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 114 |
| Pajączek: Pajączek | 0,31 s | 32 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 62 |
| Czerw: Ugryzienie | 0,33 s | 181 |
| Ognisty chochlik: Pazury | 0,33 s | 26 |
| Kamienny chochlik: Pazury | 0,33 s | 24 |
| Chochlik wichru: Pazury | 0,33 s | 22 |
| Wodny chochlik: Pazury | 0,33 s | 11 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 239 |
| Matka Burz: Matka Burz: WindStep | 0,45 s | 13 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 88% (n=24) | 100% (n=5) | 100% (n=8) | – | 100% (n=11) | 69% (n=13) |
| niezauważony | 82% (n=55) | – | 78% (n=18) | 100% (n=7) | 100% (n=432) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 76% (n=17) |
| unik | 20% (n=171) | 10% (n=21) | 13% (n=160) | 0% (n=7) | 100% (n=10) | 22% (n=69) |
| unik (eksperyment) | 28% (n=43) | 0% (n=2) | 5% (n=22) | 0% (n=2) | 100% (n=28) | 20% (n=5) |
| wyjście z zasięgu | 60% (n=20) | 50% (n=4) | 50% (n=20) | – | 100% (n=170) | 47% (n=17) |
| wyjście z zasięgu (eksperyment) | 89% (n=36) | 100% (n=1) | 78% (n=40) | – | 100% (n=17) | 50% (n=12) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 208,7 | reakcja na cios wręcz 174,7, unik przed pociskiem 63,7, wyjście z okrążenia 8,3, wabienie pod czar obszarowy 7,0 | w trakcie uniku 18,7, drgnięcie (trafiony) 11,0, brak wytrzymałości 4,7, picie flaszki 3,0, umiejętność 0,3 | 79% (n=524) / 92% (n=191) |
| Gracz H&S / souls-like | 227,0 | reakcja na cios wręcz 205,3, unik przed pociskiem 65,7, doskok do odsłoniętego celu 24,0, wabienie pod czar obszarowy 6,3, wyjście z okrążenia 5,0 | w trakcie uniku 41,0, drgnięcie (trafiony) 11,7, brak wytrzymałości 9,0, picie flaszki 3,0, unieruchomienie 1,0, inna 0,3 | 81% (n=616) / 95% (n=197) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Powietrze | Nova powietrza | 1 | 1 | 30,0 | 1595,0 |
| Tester QA | Ogień | Nova ognia | 1 | 0 | 16,0 | 541,0 |
| Tester QA | Woda | Nova wody | 1 | 0 | 14,0 | 509,0 |
| Gracz H&S / souls-like | Powietrze | Nova powietrza | 1 | 1 | 30,0 | 1593,0 |
| Gracz H&S / souls-like | Ogień | Nova ognia | 1 | 0 | 27,0 | 1353,0 |
| Gracz H&S / souls-like | Woda | Nova wody | 1 | 0 | 16,0 | 610,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka powietrza | 606887 | 57% |  |  |
| Nova powietrza | 122748 | 12% | 157 | 782 |
| Różdżka ognia | 104267 | 10% |  |  |
| Różdżka wody | 86303 | 8% |  |  |
| Ściana ognia | 41826 | 4% | 165 | 253 |
| Nova wody | 36890 | 3% | 73 | 505 |
| Nova ognia | 34234 | 3% | 118 | 290 |
| Odskok widma | 16401 | 2% |  |  |
| Pocisk arkanów | 10710 | 1% | 109 | 98 |
| Roztrzaskanie | 2334 | 0% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Przeciążenie ×255.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka powietrza | 570590 | 36% |  |  |
| Różdżka ognia | 251776 | 16% |  |  |
| Łańcuch potępionych | 185319 | 12% | 191 | 970 |
| Płonąca ziemia | 130414 | 8% | 70 | 1863 |
| Różdżka wody | 108797 | 7% |  |  |
| Burza | 104645 | 7% | 63 | 1661 |
| Nova ognia | 62705 | 4% | 114 | 550 |
| Wyrok burzy | 61466 | 4% | 42 | 1463 |
| Kościany golem | 27575 | 2% | 12 | 2298 |
| Odskok widma | 20492 | 1% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Przeciążenie ×197, Krwawy pakt ×5.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,0, mediana 4,0, najwięcej 4 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 31% | 91,3 | 192 | 0,8 |
| klasa | 29% | 85,0 | – | 0,5 |
| przedmiot | 40% | 116,0 | 557 | 0,8 |

Oferty sklepu dusz: widziane 171, kupione 2, odrzucone 0, za mało dusz 55.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Przeciążenie | klasa | 3 | 85,0 | – | 0,5 |
| Ściana ognia | odblokowanie | 3 | 55,0 | 253 | 0,7 |
| Nova powietrza | przedmiot | 1 | 157,0 | 782 | 0,8 |
| Nova ognia | przedmiot | 1 | 118,0 | 290 | 0,8 |
| Pocisk arkanów | odblokowanie | 3 | 36,3 | 98 | 1,0 |
| Nova wody | przedmiot | 1 | 73,0 | 505 | 0,8 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 8,7, mediana 7,0, najwięcej 12 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 32% | 91,3 | 984 | 0,5 |
| klasa | 23% | 65,7 | – | 0,2 |
| sklep | 30% | 85,7 | 1080 | 2,0 |
| przedmiot | 15% | 41,3 | 521 | 0,3 |

Oferty sklepu dusz: widziane 210, kupione 6, odrzucone 1, za mało dusz 63.
Zamiany w slotach: 6,7 na podejście; najczęstsze: Ściana ognia → Łańcuch potępionych ×1, Pocisk arkanów → Wyrok burzy ×1, Łańcuch potępionych → Kościany golem ×1, Pocisk arkanów → Rozprysk arkanów ×1, Kojące światło → Krwawy pakt ×1, Nova wody → Lodowy podmuch ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Przeciążenie | klasa | 3 | 65,7 | – | 0,2 |
| Łańcuch potępionych | sklep | 1 | 191,0 | 970 | 2,0 |
| Nova ognia | przedmiot | 1 | 114,0 | 550 | 0,3 |
| Płonąca ziemia | odblokowanie | 2 | 35,0 | 1863 | 0,7 |
| Burza | odblokowanie | 2 | 31,5 | 1661 | 0,5 |
| Wyrok burzy | sklep | 1 | 42,0 | 1463 | 2,0 |
| Ściana ognia | odblokowanie | 1 | 33,0 | 247 | 0,0 |
| Lodowy podmuch | odblokowanie | 1 | 28,0 | 257 | 0,9 |
| Łańcuch błyskawic | odblokowanie | 1 | 17,0 | 371 | 0,0 |
| Pocisk arkanów | odblokowanie | 1 | 15,0 | 119 | 0,6 |
| Kula ognia | odblokowanie | 2 | 7,5 | 367 | 0,8 |
| Kościany golem | sklep | 2 | 6,0 | 2298 | 2,0 |
| Mroźna fala | odblokowanie | 1 | 12,0 | 249 | 0,0 |
| Włócznia potępionych | odblokowanie | 1 | 9,0 | 200 | 0,0 |
| Rozprysk arkanów | odblokowanie | 1 | 7,0 | 115 | 1,0 |
| Nova ziemi | sklep | 1 | 7,0 | 257 | 1,0 |
| Nova wody | przedmiot | 1 | 5,0 | 199 | 0,0 |
| Krwawy pakt | odblokowanie | 1 | 5,0 | – | 0,0 |
| Nova powietrza | przedmiot | 1 | 5,0 | 184 | 0,0 |
| Salwa | sklep | 1 | 5,0 | 299 | 4,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 1329,7, czar obszarowy na jednego (ratunek) 293,0, umiejętność 292,3, unik 208,7, drgnięcie (trafiony) 71,7, czar obszarowy po zwabieniu 30,7, flaszka (próba) 24,0, flaszka many (próba) 15,3, wabienie bez skutku 7,0.
Czas w bloku 0 s, w biegu 84 s, bez wytrzymałości (<10%) 106 s na podejście.
Zakupy: dodatkowa flaszka ×5, sklep: Wicher ×4, sklep: Hart ciała ×4, sklep: Przepalenie ×3, sklep: Hart ognia ×3, ulepszenie: Pocisk arkanów ×3, sklep: Szybkie ręce ×3, ulepszenie: Ściana ognia ×3.
Punkty cech: Intelligence 16, Toughness 14, Dexterity 14, Strength 13.
Pieczęcie na podejście: z daru bossa 6,0, z sakwy do skilli 4,0, kupione 0,0; najczęstsze: Deszcz piekieł ×3, Klątwa Mokoszy ×3, Pakt z diabłem ×2, Serce otchłani ×2, Oko Czarnoboga ×2, Wieczny płomień ×2.
Komunikaty gry: „Sloty przegrzane” ×3662; „PRZECIĄŻENIE” ×250; „WYŁADOWANIE!” ×214; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×46; „Portale gasną – dobij pozostałe demony” ×39; „Garda wroga przełamana!” ×21; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×19; „OGŁUSZENIE!” ×11.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 1331,0, umiejętność 284,0, unik 227,0, czar obszarowy na jednego (ratunek) 98,0, drgnięcie (trafiony) 72,3, flaszka (próba) 25,7, czar obszarowy po zwabieniu 20,7, flaszka many (próba) 6,3, wabienie bez skutku 5,3.
Czas w bloku 0 s, w biegu 102 s, bez wytrzymałości (<10%) 98 s na podejście.
Zakupy: dodatkowa flaszka ×6, sklep: Furia ×6, sklep: Szeroki zasięg ×6, sklep: Hart ciała ×5, sklep: Szybkie ręce ×5, sklep: Twarda skóra ×5, sklep: Rykoszet ×5, sklep: Skupienie ×4.
Punkty cech: Intelligence 48, Toughness 22.
Pieczęcie na podejście: z daru bossa 7,3, z sakwy do skilli 5,3, kupione 0,3; najczęstsze: Deszcz piekieł ×3, Pakt z diabłem ×3, Oko Czarnoboga ×3, Nieskończoność ×3, Ofiara krwi ×2, Wieczny płomień ×2.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Kontra, Rozprysk arkanów, Mroźna fala, Krwawy pakt, Mroźna zbroja, Łańcuch błyskawic, Włócznia potępionych, Płonąca ziemia, Burza, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Szata popiołu, Kosa, Wielki topór, Młot burzy, Kojące światło, Pierścień riposty, Przetrwanie, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „Sloty przegrzane” ×1715; „PRZECIĄŻENIE” ×191; „WYŁADOWANIE!” ×152; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×55; „Portale gasną – dobij pozostałe demony” ×48; „Garda wroga przełamana!” ×24; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×23; „POLEGŁEŚ” ×10.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×174, uśpiony wróg blokuje koniec piętra ×19, zablokowany ruch ×2.
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 14 „XIV. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 9 „IX. Królowa Topieli”: brak postępu przy (-5,4; -9,4)
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 28 „XXVIII. Tron Arcydemona”: brak postępu przy (-7,1; -9,1)
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 29 „XXIX. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×246, uśpiony wróg blokuje koniec piętra ×22, zablokowany ruch ×1.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 16 „XVI. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść (w 3 podejściach)
  - piętro 10 „X. Rzeźnia”: brak postępu przy (-7,8; -0,2)
  - piętro 25 „XXV. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 26 „XXVI. Kuźnia Piekieł”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 13 „XIII. Kamienne Trzewia”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 17 „XVII. Wieża Burz”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 22 „XXII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 23 „XXIII. Chór Cieni”: Gargulec czeka uśpiony – trzeba do niego podejść
  - piętro 28 „XXVIII. Tron Arcydemona”: Gargulec czeka uśpiony – trzeba do niego podejść

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 3 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** zablokowany ruch średnio 0,7× na podejście (geometria areny/kolizje). Przykłady: piętro 9 „IX. Królowa Topieli”: brak postępu przy (-5,4; -9,4); piętro 28 „XXVIII. Tron Arcydemona”: brak postępu przy (-7,1; -9,1).
- **Czytelność (P2):** 18% trafień w bohatera przyszło z tyłu (>110° od kierunku patrzenia). W grupach brakuje sygnału ataku spoza kadru.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 0% ciosów (n=9).
- Unik przed ciężkim: działa – rani tylko 9% ciosów (n=23).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=187) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 8% pocisków, przed którymi był unik (n=191).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 39% decyzji zapada, gdy do trafienia zostało <0,1 s (n=438) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=83).
- Telegraf „nieparowalny” daje czas: tylko 10% decyzji zapada mniej niż 0,1 s przed trafieniem (n=183).
- Telegraf „obszarowy” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=346).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- UX: gra odmawiała akcji: „Sloty przegrzane” ×3662 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Tempo: piętro „XVIII. Matka Burz” trwa średnio 187 s – najdłuższy fragment wieży.
- Tempo: piętro „XIX. Kostnica” trwa średnio 171 s – najdłuższy fragment wieży.
- Tempo: piętro „XX. Kostnica” trwa średnio 161 s – najdłuższy fragment wieży.
- Tempo: piętro „XXII. Chór Cieni” trwa średnio 187 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIII. Chór Cieni” trwa średnio 172 s – najdłuższy fragment wieży.
- Tempo: piętro „XXV. Kuźnia Piekieł” trwa średnio 210 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVI. Kuźnia Piekieł” trwa średnio 188 s – najdłuższy fragment wieży.
- Tempo: piętro „XXVIII. Tron Arcydemona” trwa średnio 195 s – najdłuższy fragment wieży.
- Tempo: piętro „XXIX. Tron Arcydemona” trwa średnio 184 s – najdłuższy fragment wieży.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 106 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 1/3. Wyzwanie umiarkowane; po opanowaniu parowania wieża przestaje straszyć.
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Legionista (9,8 trafień, 3,1 s), Gnilec (9,0 trafień, 2,0 s), Kolos żaru (8,7 trafień, 2,1 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,1), Ożywiony nietoperz (1,3), Czerw (1,8) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Królowa Topieli – średnio 64 s i 132 trafień do zabicia.
- Długie starcie: Matka Burz – średnio 92 s i 136 trafień do zabicia.
- Długie starcie: Król Kości – średnio 48 s i 118 trafień do zabicia.
- Długie starcie: Bezimienny Cień – średnio 82 s i 178 trafień do zabicia.
- Długie starcie: Kowal Piekieł – średnio 53 s i 189 trafień do zabicia.
- Długie starcie: Arcydiabeł – średnio 65 s i 246 trafień do zabicia.
- 2/2 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 52% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Za dużo ciosów zza pleców (25% trafień). Przy namierzaniu nie widzę reszty grupy – w souls to oznaka złego designu walki z grupą.
- Skille robią 39% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Kościany golem”: 2298 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 30%. Różnych skilli na podejście: średnio 8,7.
- Najczęściej grałem: Ogień + Nova ognia (1×, śr. piętro 27,0). Inne: Woda + Nova wody 1× (16,0), Powietrze + Nova powietrza 1× (31,0).
- Unik to moje główne narzędzie: 227,0 na podejście, w tym 29,0 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 95% przypadków.
- Gra nie wykonała uniku od razu średnio 66,0× na podejście – najczęściej: w trakcie poprzedniego uniku 41,0, drgnięcie (trafiony) 11,7, brak wytrzymałości 9,0. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (72 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

