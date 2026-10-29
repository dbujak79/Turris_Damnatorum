# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 15:20. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 6,9 | 7,0 | 9,2 min | 187,5 | 38097 / 5000 | 12,4 | 1% |
| Gracz H&S / souls-like | 20 | 0 (0%) | 6,9 | 7,0 | 8,8 min | 186,0 | 38050 / 3776 | 9,9 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 7 7 8 9 8 6 5 8 7 7 8 7 5 6 7 7 6 7 8 5
- Gracz H&S / souls-like: 7 7 8 5 2 8 8 7 8 7 7 7 8 7 7 7 7 8 8 5

## Gdzie i od czego giną

- **Tester QA** — piętra: 5. V. Psiarnia Piekieł ×3, 6. VI. Pan Sfory ×3, 7. VII. Zatopione Krużganki ×8, 8. VIII. Zatopione Krużganki ×5, 9. IX. Królowa Topieli ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×5; Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×2; Ogar otchłani – „Ogar otchłani: Combo” ×2; Taranowiec – „Taranowiec” ×2; Pan Sfory – „Pan Sfory: Combo” ×2; Harpia – „Harpia” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 17/20.
  Ostatnie 3 s przed śmiercią: średnio 37% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/20). Picie: 286 prób, 248 wypitych (13% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×1, 5. V. Psiarnia Piekieł ×2, 7. VII. Zatopione Krużganki ×10, 8. VIII. Zatopione Krużganki ×7.
  Ostatni cios: Kultysta – „Kula ognia” ×3; Kultysta topieli – „Kultysta topieli” ×2; Taranowiec – „Taranowiec” ×2; Ogar otchłani – „Ogar otchłani” ×2; Legionista – „Legionista: ShieldBash” ×2; Siarkowy pomiot – „Siarkowy pomiot” ×2. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 19/20.
  Ostatnie 3 s przed śmiercią: średnio 34% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/20). Picie: 245 prób, 197 wypitych (20% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 54 s | 28 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 21 | 20 | 63 s | 175 | 0,5 | 41,0 |
| Tester QA | 3. III. Władca Bramy | 22 | 20 | 38 s | 431 | 1,6 | 7,6 |
| Tester QA | 4. IV. Psiarnia Piekieł | 23 | 20 | 65 s | 269 | 0,4 | 44,3 |
| Tester QA | 5. V. Psiarnia Piekieł | 31 | 17 | 68 s | 497 | 1,3 | 33,0 |
| Tester QA | 6. VI. Pan Sfory | 28 | 14 | 46 s | 704 | 3,2 | 4,6 |
| Tester QA | 7. VII. Zatopione Krużganki | 39 | 6 | 86 s | 685 | 1,1 | 19,9 |
| Tester QA | 8. VIII. Zatopione Krużganki | 18 | 1 | 91 s | 868 | 0,9 | 21,6 |
| Tester QA | 9. IX. Królowa Topieli | 3 | 0 | 0 s | 882 | 2,3 | 0,7 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 55 s | 13 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 25 | 19 | 73 s | 305 | 1,0 | 37,1 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 19 | 19 | 31 s | 153 | 0,2 | 8,1 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 25 | 19 | 62 s | 247 | 0,5 | 41,1 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 27 | 17 | 63 s | 391 | 1,1 | 31,7 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 21 | 17 | 44 s | 514 | 2,5 | 6,0 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 39 | 7 | 88 s | 598 | 1,2 | 22,7 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 21 | 0 | 0 s | 661 | 1,2 | 22,2 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 2 ×1, piętro 3 ×2, piętro 4 ×3, piętro 5 ×11, piętro 6 ×11, piętro 7 ×25, piętro 8 ×12, piętro 9 ×2; wyzwań (3 porażki na piętrze): 20 – class_knight_Tester_pietro07.json, class_knight_Tester_pietro08.json, class_knight_Tester_pietro09.json, class_knight_Tester_pietro06.json, class_knight_Tester_pietro05.json.
**Gracz H&S / souls-like** – ponowne próby: piętro 2 ×5, piętro 4 ×6, piętro 5 ×8, piętro 6 ×4, piętro 7 ×22, piętro 8 ×14; wyzwań (3 porażki na piętrze): 20 – class_knight_SoulsSlasher_pietro07.json, class_knight_SoulsSlasher_pietro08.json, class_knight_SoulsSlasher_pietro05.json, class_knight_SoulsSlasher_pietro02.json.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 1076 | 0,6 s | 1,1 | 2135 | 32912 | 30 | 8 |
| Pan Sfory | 31 | 40,5 s | 27,8 | 1276 | 28211 | 910 | 3 |
| Kultysta topieli | 121 | 1,8 s | 1,5 | 776 | 15948 | 131 | 3 |
| Ogar otchłani | 973 | 0,9 s | 1,5 | 1394 | 15474 | 16 | 5 |
| Władca Bramy | 39 | 30,4 s | 19,6 | 491 | 11326 | 290 | 0 |
| Harpia | 162 | 1,0 s | 1,7 | 659 | 9073 | 55 | 1 |
| Taranowiec | 358 | 3,5 s | 2,6 | 571 | 8643 | 24 | 5 |
| Kolos | 159 | 6,3 s | 3,7 | 177 | 8179 | 50 | 1 |
| Czerw | 3051 | 0,5 s | 1,3 | 1943 | 8001 | 3 | 2 |
| Siarkowy pomiot | 635 | 0,2 s | 1,1 | 192 | 7303 | 11 | 2 |
| Wiedźma wichru | 56 | 2,9 s | 1,8 | 662 | 7207 | 126 | 3 |
| Legionista | 183 | 9,1 s | 5,1 | 695 | 6628 | 35 | 3 |
| Ognisty chochlik | 607 | 0,4 s | 1,2 | 348 | 4101 | 6 | 1 |
| Królowa Topieli | 0 | – | – | 79 | 2646 | – | 1 |
| Kamienny chochlik | 579 | 0,3 s | 1,2 | 221 | 2580 | 4 | 0 |
| Wodny chochlik | 616 | 0,4 s | 1,2 | 259 | 2490 | 4 | 1 |
| Chochlik wichru | 630 | 0,6 s | 1,4 | 245 | 2463 | 4 | 1 |
| Ofiarnik | 25 | 1,1 s | 1,7 | 168 | 1267 | 47 | 0 |
| Ożywiony nietoperz | 283 | 0,8 s | 1,4 | 213 | 1062 | 4 | 0 |
| Ognisty chochlik (elita) | 67 | 4,4 s | 4,3 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 32 | 6,2 s | 4,1 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 27 | 1,7 s | 2,0 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 31 | 4,2 s | 3,2 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 3 | 14,7 s | 11,0 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 4 | 59,4 s | 32,5 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 17 | 5,7 s | 4,8 | ↑ | ↑ | ↑ |  |
| Wiedźma wichru (elita) | 1 | 33,0 s | 9,0 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 2 | 6,5 s | 7,0 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 4 | 41,4 s | 19,3 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 1 | 2,1 s | 4,0 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 2 | 10,4 s | 5,5 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 3358 | 77% | 2% | 2% | 15% | 3% |
| ciężki | 339 | 66% | 0% | 3% | 28% | 4% |
| nieparowalny | 1760 | 73% | 4% | 0% | 14% | 9% |
| nieblokowalny | 173 | 83% | 0% | 0% | 17% | 0% |
| obszarowy | 913 | 89% | 6% | 0% | 0% | 5% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2981 | 62% | 2% | 7% | 26% | 3% |
| ciężki | 294 | 50% | 2% | 15% | 31% | 1% |
| nieparowalny | 1672 | 62% | 6% | 0% | 21% | 11% |
| nieblokowalny | 131 | 76% | 0% | 0% | 24% | 0% |
| obszarowy | 883 | 84% | 11% | 0% | 0% | 4% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 3415 | 27% | 46% | 2% | 13% | 12% |
| Tester QA (0,30 s) | ciężki | 411 | 2% | 1% | 2% | 12% | 83% |
| Tester QA (0,30 s) | nieparowalny | 458 | 7% | 4% | 14% | 38% | 36% |
| Tester QA (0,30 s) | nieblokowalny | 18 | 0% | 0% | 33% | 44% | 22% |
| Tester QA (0,30 s) | obszarowy | 838 | 2% | 1% | 3% | 38% | 55% |
| Tester QA (0,30 s) | pocisk | 2559 | 2% | 11% | 8% | 20% | 59% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3201 | 3% | 31% | 39% | 8% | 19% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 373 | 1% | 1% | 1% | 8% | 89% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 367 | 10% | 1% | 5% | 30% | 53% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 9 | 11% | 0% | 0% | 33% | 56% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 857 | 2% | 1% | 5% | 39% | 54% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 3062 | 1% | 8% | 8% | 17% | 66% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 1078 |
| Ognisty chochlik: Pazury | 0,33 s | 515 |
| Chochlik wichru: Pazury | 0,33 s | 454 |
| Czerw: Ugryzienie | 0,33 s | 2857 |
| Kamienny chochlik: Pazury | 0,33 s | 391 |
| Wodny chochlik: Pazury | 0,33 s | 479 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 337 |
| Władca Bramy: Cięcie w szale | 0,41 s | 44 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 352 |
| Władca Bramy: Drugie cięcie | 0,49 s | 99 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 64% (n=114) | – | 64% (n=61) | 100% (n=6) | 32% (n=63) | 92% (n=522) |
| blok (eksperyment) | 88% (n=122) | 81% (n=21) | 86% (n=49) | – | 85% (n=33) | 76% (n=83) |
| brak reakcji (eksperyment) | 90% (n=165) | 90% (n=21) | 100% (n=51) | 100% (n=6) | 100% (n=36) | 92% (n=102) |
| niezauważony | 86% (n=653) | – | 94% (n=162) | 99% (n=104) | 99% (n=351) | – |
| parowanie | 69% (n=61) | 61% (n=18) | – | – | – | – |
| parowanie (eksperyment) | 73% (n=63) | 64% (n=11) | 95% (n=19) | 100% (n=2) | 100% (n=16) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 87% (n=89) |
| unik | 61% (n=684) | 51% (n=136) | 40% (n=240) | 32% (n=38) | – | 72% (n=191) |
| unik (eksperyment) | 77% (n=245) | 58% (n=43) | 64% (n=61) | 60% (n=5) | 100% (n=42) | 87% (n=75) |
| wyjście z zasięgu | 92% (n=871) | 97% (n=73) | 98% (n=152) | 100% (n=12) | 99% (n=347) | 97% (n=32) |
| wyjście z zasięgu (eksperyment) | 92% (n=158) | 94% (n=16) | 100% (n=30) | – | 100% (n=25) | 87% (n=63) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 68,9 | reakcja na cios wręcz 85,0, unik przed pociskiem 32,9, wyjście z okrążenia 4,9 | brak wytrzymałości 16,2, drgnięcie (trafiony) 11,8, w trakcie uniku 9,7, picie flaszki 3,7, parowanie 0,9, inna 0,7, umiejętność 0,4, riposta 0,2, mocny atak 0,2, szybki atak 0,1, przełamana garda 0,1 | 48% (n=1700) / 69% (n=658) |
| Gracz H&S / souls-like | 84,0 | reakcja na cios wręcz 73,1, unik przed pociskiem 56,8, doskok do odsłoniętego celu 6,2, wyjście z okrążenia 4,7 | w trakcie uniku 16,1, drgnięcie (trafiony) 9,5, brak wytrzymałości 7,4, picie flaszki 4,6, parowanie 4,3, inna 0,9, riposta 0,5, umiejętność 0,3, blok 0,2, szybki atak 0,1, mocny atak 0,1 | 58% (n=1461) / 69% (n=1136) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 7,2 | 196,0 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 7,2 | 194,2 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 6,6 | 180,6 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 6,6 | 179,2 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 14 | 0 | 7,2 | 194,4 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 2 | 0 | 7,0 | 193,0 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 2 | 0 | 6,5 | 174,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 2 | 0 | 5,0 | 132,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 133244 | 17% |  |  |
| Rozpłatanie | 84439 | 11% | 401 | 211 |
| Fala przełamania | 84314 | 11% | 209 | 403 |
| Tąpnięcie | 76898 | 10% | 501 | 153 |
| Cięcie wichru | 72748 | 10% | 403 | 181 |
| Cięcie | 57356 | 8% |  |  |
| Egzekucja | 43430 | 6% |  |  |
| Cięcie krzyżowe | 36558 | 5% | 192 | 190 |
| Okrzyk bojowy | 30729 | 4% | 717 | 43 |
| Cięcie z wyskoku | 29688 | 4% | 165 | 180 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×84.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Rozpłatanie | 357928 | 47% | 1929 | 186 |
| Cięcie wichru | 135570 | 18% | 625 | 217 |
| Szarża | 53784 | 7% | 502 | 107 |
| Cięcie | 45432 | 6% |  |  |
| Fala przełamania | 31560 | 4% | 95 | 332 |
| Riposta | 31516 | 4% |  |  |
| Okrzyk bojowy | 27864 | 4% | 369 | 76 |
| Zamach | 23245 | 3% |  |  |
| Potężne cięcie | 21189 | 3% |  |  |
| Krwawe cięcie | 9041 | 1% | 57 | 159 |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 6,5, mediana 6,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 19% | 32,9 | 188 | 1,1 |
| odblokowanie | 14% | 25,1 | 153 | 0,8 |
| klasa | 20% | 35,9 | 43 | 0,1 |
| nagroda | 6% | 9,8 | 108 | 1,0 |
| sklep | 41% | 72,2 | 161 | 0,7 |

Oferty sklepu dusz: widziane 118, kupione 78, odrzucone 0, za mało dusz 40.
Zamiany w slotach: 4,4 na podejście; najczęstsze: Tąpnięcie → Rozpłatanie ×6, Tąpnięcie → Cięcie wichru ×5, Rozpłatanie → Tąpnięcie ×4, Tąpnięcie → Wypad ×4, Rozpłatanie → Fala przełamania ×3, Rozpłatanie → Cięcie z wyskoku ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 20 | 35,9 | 43 | 0,1 |
| Tąpnięcie | odblokowanie | 20 | 25,1 | 153 | 0,8 |
| Cięcie wichru | startowy | 12 | 33,6 | 181 | 0,9 |
| Rozpłatanie | startowy | 16 | 25,1 | 211 | 1,0 |
| Wypad | nagroda | 6 | 35,5 | 38 | 0,9 |
| Fala przełamania | sklep | 7 | 29,9 | 403 | 0,8 |
| Cięcie krzyżowe | sklep | 8 | 24,0 | 190 | 0,5 |
| Odtrącenie | nagroda | 7 | 25,0 | 24 | 0,7 |
| Krwawe cięcie | sklep | 7 | 23,7 | 136 | 0,8 |
| Cięcie z wyskoku | sklep | 8 | 20,6 | 180 | 1,0 |
| Szarża | sklep | 7 | 16,0 | 81 | 0,5 |
| Okrzyk wojenny | sklep | 5 | 16,8 | – | 0,8 |
| Upadek kata | sklep | 3 | 24,0 | 275 | 0,5 |
| Kontra | sklep | 2 | 27,5 | 46 | 0,8 |
| Uderzenie tarczą | sklep | 2 | 25,5 | 54 | 0,9 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,1, mediana 4,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 55% | 100,0 | 182 | 1,8 |
| odblokowanie | 22% | 39,3 | 146 | 1,6 |
| klasa | 10% | 18,5 | 76 | 0,7 |
| nagroda | 0% | 0,9 | 94 | 0,8 |
| sklep | 13% | 23,7 | 242 | 1,3 |

Oferty sklepu dusz: widziane 118, kupione 19, odrzucone 60, za mało dusz 39.
Zamiany w slotach: 1,1 na podejście; najczęstsze: Szarża → Krwawe cięcie ×4, Szarża → Cięcie wichru ×4, Rozpłatanie → Cięcie wichru ×3, Szarża → Fala przełamania ×3, Tąpnięcie → Fala przełamania ×1, Fala przełamania → Cięcie krzyżowe ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | odblokowanie | 20 | 96,5 | 186 | 1,9 |
| Cięcie wichru | startowy | 12 | 52,1 | 217 | 1,5 |
| Szarża | odblokowanie | 15 | 33,5 | 107 | 1,3 |
| Okrzyk bojowy | klasa | 20 | 18,5 | 76 | 0,7 |
| Fala przełamania | sklep | 5 | 19,0 | 332 | 1,0 |
| Krwawe cięcie | odblokowanie | 4 | 14,3 | 159 | 1,7 |
| Tąpnięcie | odblokowanie | 2 | 16,5 | 76 | 0,8 |
| Cięcie krzyżowe | sklep | 1 | 18,0 | 186 | 1,6 |
| Kontra | odblokowanie | 1 | 10,0 | 22 | 0,6 |
| Uderzenie tarczą | sklep | 1 | 7,0 | 32 | 0,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 175,8, szybki atak 84,9, drgnięcie (trafiony) 80,2, unik 68,9, blok 44,4, mocny atak 29,8, przełamana garda 16,3, flaszka (próba) 14,3, parowanie 4,6, riposta 1,1.
Czas w bloku 17 s, w biegu 28 s, bez wytrzymałości (<10%) 248 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Tąpnięcie → +1 ×10, wzmocnienie: Pijawka ×8, wzmocnienie: Siła tytana ×4, wzmocnienie: Oddech wojownika ×4, wzmocnienie: Obrońca ×4, wzmocnienie: Zwinność ×4, ulepszenie: Ulepszenie: Tąpnięcie → +2 ×3, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×3.
Zakupy: ulepszenie broni ×20, ulepszenie: Rozpłatanie ×17, ulepszenie: Cięcie wichru ×14, oferta: Cięcie z wyskoku ×9, oferta: Fala przełamania ×8, ulepszenie: Tąpnięcie ×8, oferta: Rozpłatanie ×7, oferta: Krwawe cięcie ×7.
Punkty cech: Strength 37, Toughness 34, Dexterity 26, Intelligence 21.
Komunikaty gry: „OKRZYK!” ×665; „SZAŁ!” ×467; „GARDA PRZEŁAMANA!” ×336; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×152; „Portale gasną – dobij pozostałe demony” ×108; „POLEGŁEŚ” ×87; „Garda wroga przełamana!” ×84; „PAROWANIE!” ×71.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 182,3, unik 84,0, drgnięcie (trafiony) 69,4, blok 51,2, szybki atak 44,6, mocny atak 17,4, parowanie 16,0, przełamana garda 15,6, flaszka (próba) 12,3, riposta 6,0.
Czas w bloku 23 s, w biegu 30 s, bez wytrzymałości (<10%) 153 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +4 ×8, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×7, wzmocnienie: Żelazna skóra ×6, ulepszenie: Ulepszenie: Szarża → +2 ×6, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×6, ulepszenie: Ulepszenie: Cięcie wichru → +2 ×5, wzmocnienie: Czujność ×5, wzmocnienie: Bystry umysł ×4.
Zakupy: ulepszenie: Rozpłatanie ×63, ulepszenie: Okrzyk bojowy ×23, ulepszenie broni ×22, ulepszenie: Szarża ×15, ulepszenie: Cięcie wichru ×13, oferta: Cięcie wichru ×8, oferta: Fala przełamania ×6, ulepszenie: Fala przełamania ×5.
Punkty cech: Toughness 46, Strength 72.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Puklerz, Kontra, Sztylet parujący, Łuk refleksyjny, Przetrwanie, Krwawa riposta, Strażnik zaklęć, Pierścień riposty.
Komunikaty gry: „OKRZYK!” ×341; „GARDA PRZEŁAMANA!” ×324; „SZAŁ!” ×289; „PAROWANIE!” ×239; „Postawa przełamana – RIPOSTA!” ×196; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×157; „Portale gasną – dobij pozostałe demony” ×113; „RIPOSTA!” ×112.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×503, garda nie zatrzymała ciosu ×5, zablokowany ruch ×5, parowanie w oknie nie zadziałało ×1.
  - Ofiarnik: „Ofiarnik” przeszedł przez gardę (kąt 156°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,5; -1,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (6,4; -1,1)
  - Harpia: „Harpia” trafił w aktywnym oknie parowania (kąt do źródła 93°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 130°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,0; -1,1)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 144°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 158°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; 7,5)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 143°)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-5,6; 8,1)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×509, parowanie w oknie nie zadziałało ×12, garda nie zatrzymała ciosu ×5, zablokowany ruch ×2.
  - Kultysta topieli: „Kultysta topieli” przeszedł przez gardę (kąt 175°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 141°)
  - Ofiarnik: „Ofiarnik: BloodRite” przeszedł przez gardę (kąt 146°)
  - Ożywiony nietoperz: „Ukąszenie” trafił w aktywnym oknie parowania (kąt do źródła 111°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (5,7; 1,0)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 175°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 169°)
  - Taranowiec: „Taranowiec: Combo” trafił w aktywnym oknie parowania (kąt do źródła 96°)
  - Siarkowy pomiot: „Siarkowy pomiot” trafił w aktywnym oknie parowania (kąt do źródła 80°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 169°)
  - Pan Sfory: „Pan Sfory: Volley” trafił w aktywnym oknie parowania (kąt do źródła 15°)
  - Legionista: „Legionista” trafił w aktywnym oknie parowania (kąt do źródła 105°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (5 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 1× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Harpia: „Harpia” trafił w aktywnym oknie parowania (kąt do źródła 93°)
- **Do sprawdzenia (P3):** 5× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Ofiarnik: „Ofiarnik” przeszedł przez gardę (kąt 156°)
- Parowanie zwykłych ciosów: 71% ciosów mimo to rani (n=124) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 35% ciosów (n=43).
- Unik przed ciężkim: 53% ciosów mimo to rani (n=179) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 76% ciosów mimo to rani (n=236) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 99% ciosów mimo to rani (n=372) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 31% pocisków, przed którymi był unik (n=658).
- Blok pocisku: 90% ciosów mimo to rani (n=605) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 73% decyzji zapada, gdy do trafienia zostało <0,1 s (n=3415) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=411).
- Telegraf „nieparowalny” daje czas: tylko 12% decyzji zapada mniej niż 0,1 s przed trafieniem (n=458).
- Telegraf „nieblokowalny” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=18).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=838).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 77 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 20 podejściach. Średnio dochodzę do piętra 6,9 – na razie to ściana, nie krzywa.
- Czuję progres: druga połowa podejść średnio piętro 7,1 wobec 6,7 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 40% udanych (n=538). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (27,6 trafień, 39,8 s), Władca Bramy (16,9 trafień, 26,9 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta (1,1), Siarkowy pomiot (1,1), Wodny chochlik (1,2) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Legionista (elita) – średnio 70 s i 33 trafień do zabicia.
- Długie starcie: Kolos (elita) – średnio 52 s i 22 trafień do zabicia.
- 19/20 razy zginąłem z flaszkami w kieszeni. 3 z tych śmierci to utrata ≥50% HP w 3 s, a 20% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 82% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 332 obrażeń na użycie.
- Skill startowy to 55% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 13%. Różnych skilli na podejście: średnio 4,1.
- Najczęściej grałem: Miecz i tarcza + Rozpłatanie (14×, śr. piętro 7,2). Inne: Miecz i tarcza + Cięcie wichru 2× (7,0), Dwuręczny + Rozpłatanie 2× (5,0), Dwuręczny + Cięcie wichru 2× (6,5).
- Unik to moje główne narzędzie: 84,0 na podejście, w tym 10,9 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 69% przypadków.
- Gra nie wykonała uniku od razu średnio 43,9× na podejście – najczęściej: w trakcie poprzedniego uniku 16,1, drgnięcie (trafiony) 9,5, brak wytrzymałości 7,4. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (29% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (69 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

