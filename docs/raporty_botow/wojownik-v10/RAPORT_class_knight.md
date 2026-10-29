# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 13:07. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (30 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (30 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 30 | 0 (0%) | 5,1 | 5,0 | 4,5 min | 132,9 | 19115 / 1560 | 5,1 | 2% |
| Gracz H&S / souls-like | 30 | 0 (0%) | 6,5 | 7,0 | 5,6 min | 183,0 | 34433 / 1579 | 4,2 | 2% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 4 5 2 8 2 5 6 4 7 5 7 6 7 6 7 7 2 6 5 5 4 4 2 4 2 6 5 8 5 6
- Gracz H&S / souls-like: 2 7 5 7 2 7 7 8 7 7 7 8 7 8 9 7 7 5 7 8 4 7 8 7 5 8 2 8 8 7

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×5, 4. IV. Psiarnia Piekieł ×5, 5. V. Psiarnia Piekieł ×7, 6. VI. Pan Sfory ×6, 7. VII. Zatopione Krużganki ×5, 8. VIII. Zatopione Krużganki ×2.
  Ostatni cios: Kultysta – „Kula ognia” ×7; Ogar otchłani – „Ogar otchłani” ×6; Siarkowy pomiot – „Siarkowy pomiot” ×5; Pan Sfory – „Pan Sfory: Combo” ×3; Kolos – „Kolos” ×2; Pan Sfory – „Pan Sfory: InfernalCharge” ×2. Śmierć ciosem ≥40% maks. HP: 0/30. Zginął mając flaszki: 23/30.
  Ostatnie 3 s przed śmiercią: średnio 29% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/30). Picie: 174 prób, 153 wypitych (12% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×3, 4. IV. Psiarnia Piekieł ×1, 5. V. Psiarnia Piekieł ×3, 7. VII. Zatopione Krużganki ×14, 8. VIII. Zatopione Krużganki ×8, 9. IX. Królowa Topieli ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×8; Ogar otchłani – „Ogar otchłani” ×4; Kultysta topieli – „Kultysta topieli” ×2; Chochlik wichru – „Pazury” ×2; Siarkowy pomiot – „Siarkowy pomiot” ×2; Wodny chochlik – „Pazury” ×2. Śmierć ciosem ≥40% maks. HP: 0/30. Zginął mając flaszki: 28/30.
  Ostatnie 3 s przed śmiercią: średnio 34% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/30). Picie: 141 prób, 126 wypitych (11% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 30 | 30 | 55 s | 25 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 30 | 25 | 62 s | 231 | 0,7 | 37,4 |
| Tester QA | 3. III. Władca Bramy | 25 | 25 | 34 s | 352 | 1,2 | 7,7 |
| Tester QA | 4. IV. Psiarnia Piekieł | 25 | 20 | 63 s | 285 | 0,9 | 40,9 |
| Tester QA | 5. V. Psiarnia Piekieł | 20 | 13 | 68 s | 426 | 1,4 | 34,7 |
| Tester QA | 6. VI. Pan Sfory | 13 | 7 | 49 s | 729 | 3,2 | 5,2 |
| Tester QA | 7. VII. Zatopione Krużganki | 7 | 2 | 70 s | 445 | 0,6 | 22,3 |
| Tester QA | 8. VIII. Zatopione Krużganki | 2 | 0 | 0 s | 1041 | 3,5 | 6,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 30 | 30 | 56 s | 29 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 30 | 27 | 60 s | 142 | 0,3 | 39,8 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 27 | 27 | 25 s | 196 | 0,4 | 7,7 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 27 | 26 | 61 s | 106 | 0,1 | 45,6 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 26 | 23 | 61 s | 233 | 0,6 | 40,7 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 23 | 23 | 35 s | 343 | 1,7 | 5,8 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 23 | 9 | 69 s | 539 | 1,4 | 30,0 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 9 | 1 | 74 s | 703 | 1,0 | 28,6 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 1 | 0 | 0 s | 1384 | 5,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 1222 | 0,2 s | 1,1 | 1197 | 18774 | 15 | 15 |
| Pan Sfory | 30 | 34,7 s | 25,0 | 755 | 16277 | 543 | 6 |
| Władca Bramy | 52 | 25,8 s | 17,4 | 475 | 12998 | 250 | 0 |
| Ogar otchłani | 777 | 0,4 s | 1,3 | 816 | 9180 | 11 | 11 |
| Czerw | 2848 | 0,4 s | 1,3 | 1256 | 4839 | 2 | 2 |
| Kultysta topieli | 50 | 0,4 s | 1,3 | 220 | 4635 | 93 | 3 |
| Siarkowy pomiot | 519 | 0,1 s | 1,1 | 108 | 3725 | 7 | 7 |
| Harpia | 69 | 0,9 s | 1,8 | 251 | 3497 | 49 | 3 |
| Taranowiec | 232 | 1,6 s | 2,2 | 207 | 3069 | 13 | 2 |
| Ognisty chochlik | 713 | 0,1 s | 1,1 | 243 | 2548 | 3 | 0 |
| Wiedźma wichru | 20 | 2,2 s | 2,1 | 202 | 2317 | 116 | 0 |
| Wodny chochlik | 742 | 0,2 s | 1,1 | 262 | 2261 | 3 | 3 |
| Legionista | 136 | 5,3 s | 4,4 | 245 | 2232 | 16 | 0 |
| Kolos | 92 | 3,7 s | 3,1 | 43 | 1940 | 21 | 3 |
| Kamienny chochlik | 677 | 0,1 s | 1,1 | 212 | 1843 | 3 | 2 |
| Królowa Topieli | 0 | – | – | 56 | 1384 | – | 1 |
| Chochlik wichru | 715 | 0,5 s | 1,4 | 168 | 1341 | 2 | 2 |
| Ożywiony nietoperz | 348 | 0,2 s | 1,1 | 210 | 1100 | 3 | 0 |
| Ofiarnik | 5 | 0,1 s | 1,4 | 19 | 204 | 34 | 0 |
| Ognisty chochlik (elita) | 91 | 3,8 s | 3,8 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 33 | 2,0 s | 2,5 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 46 | 4,5 s | 3,8 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 22 | 5,0 s | 5,3 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 26 | 1,1 s | 2,2 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 6 | 11,3 s | 12,0 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 3 | 33,1 s | 19,0 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 1 | 9,4 s | 9,0 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 2 | 18,8 s | 6,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1779 | 80% | 2% | 2% | 14% | 3% |
| ciężki | 133 | 62% | 2% | 6% | 25% | 5% |
| nieparowalny | 955 | 77% | 5% | 0% | 9% | 9% |
| nieblokowalny | 110 | 82% | 0% | 0% | 18% | 0% |
| obszarowy | 259 | 83% | 12% | 0% | 0% | 5% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1923 | 56% | 3% | 2% | 36% | 3% |
| ciężki | 162 | 49% | 1% | 2% | 46% | 2% |
| nieparowalny | 986 | 61% | 5% | 0% | 22% | 12% |
| nieblokowalny | 119 | 79% | 0% | 0% | 21% | 0% |
| obszarowy | 519 | 81% | 14% | 0% | 0% | 6% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1988 | 28% | 52% | 2% | 10% | 8% |
| Tester QA (0,30 s) | ciężki | 156 | 2% | 1% | 1% | 6% | 90% |
| Tester QA (0,30 s) | nieparowalny | 229 | 5% | 9% | 11% | 30% | 45% |
| Tester QA (0,30 s) | nieblokowalny | 9 | 11% | 0% | 22% | 33% | 33% |
| Tester QA (0,30 s) | obszarowy | 344 | 1% | 2% | 4% | 55% | 38% |
| Tester QA (0,30 s) | pocisk | 1235 | 0% | 11% | 10% | 23% | 56% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2275 | 3% | 31% | 45% | 6% | 15% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 195 | 3% | 2% | 2% | 8% | 86% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 278 | 12% | 3% | 7% | 33% | 46% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 9 | 11% | 0% | 0% | 56% | 33% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 555 | 1% | 1% | 4% | 40% | 54% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 1536 | 1% | 11% | 7% | 16% | 65% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 580 |
| Czerw: Ugryzienie | 0,33 s | 2000 |
| Ognisty chochlik: Pazury | 0,33 s | 417 |
| Wodny chochlik: Pazury | 0,33 s | 440 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 313 |
| Kamienny chochlik: Pazury | 0,33 s | 376 |
| Chochlik wichru: Pazury | 0,33 s | 335 |
| Władca Bramy: Cięcie w szale | 0,41 s | 38 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 81 |
| Władca Bramy: Piekielne kule | 0,46 s | 11 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 74% (n=88) | 0% (n=3) | 62% (n=42) | 100% (n=1) | 31% (n=36) | 89% (n=258) |
| blok (eksperyment) | 89% (n=98) | 100% (n=12) | 94% (n=18) | 100% (n=2) | 54% (n=13) | 75% (n=48) |
| brak reakcji (eksperyment) | 94% (n=86) | 90% (n=10) | 100% (n=13) | 100% (n=2) | 100% (n=10) | 94% (n=49) |
| niezauważony | 92% (n=361) | – | 98% (n=111) | 98% (n=59) | 100% (n=50) | – |
| parowanie | 75% (n=55) | 69% (n=13) | – | – | – | – |
| parowanie (eksperyment) | 78% (n=36) | 40% (n=5) | 100% (n=6) | – | 100% (n=7) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 95% (n=61) |
| unik | 62% (n=373) | 38% (n=42) | 52% (n=97) | 45% (n=29) | – | 87% (n=117) |
| unik (eksperyment) | 71% (n=123) | 22% (n=9) | 50% (n=22) | 40% (n=5) | 100% (n=28) | 95% (n=37) |
| wyjście z zasięgu | 93% (n=448) | 100% (n=31) | 99% (n=71) | 100% (n=7) | 100% (n=103) | – |
| wyjście z zasięgu (eksperyment) | 93% (n=76) | 100% (n=8) | 100% (n=12) | 100% (n=5) | 92% (n=12) | 93% (n=28) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 24,1 | reakcja na cios wręcz 29,6, unik przed pociskiem 11,5, wyjście z okrążenia 2,3 | drgnięcie (trafiony) 4,9, brak wytrzymałości 4,9, w trakcie uniku 2,7, picie flaszki 1,9, parowanie 0,5, inna 0,3, riposta 0,1, umiejętność 0,1, mocny atak 0,1, blok 0,1 | 50% (n=888) / 60% (n=345) |
| Gracz H&S / souls-like | 43,6 | reakcja na cios wręcz 43,0, unik przed pociskiem 18,3, doskok do odsłoniętego celu 3,6, wyjście z okrążenia 1,1 | w trakcie uniku 8,2, drgnięcie (trafiony) 3,1, brak wytrzymałości 2,8, picie flaszki 1,8, parowanie 0,2, inna 0,2, mocny atak 0,2, szybki atak 0,1, umiejętność 0,1, blok 0,1 | 68% (n=1291) / 77% (n=550) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 7 | 0 | 6,0 | 158,3 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 8 | 0 | 5,4 | 146,5 |
| Tester QA | Dwuręczny | Rozpłatanie | 7 | 0 | 4,9 | 131,3 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 8 | 0 | 4,1 | 98,4 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 24 | 0 | 6,9 | 196,0 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 3 | 0 | 5,3 | 141,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 5,0 | 131,0 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 2 | 0 | 4,5 | 116,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 109947 | 19% |  |  |
| Cięcie wichru | 68871 | 12% | 406 | 170 |
| Rozpłatanie | 66885 | 12% | 385 | 174 |
| Tąpnięcie | 63942 | 11% | 387 | 165 |
| Fala przełamania | 42204 | 7% | 134 | 315 |
| Cięcie | 38506 | 7% |  |  |
| Egzekucja | 33939 | 6% |  |  |
| Upadek kata | 33604 | 6% | 144 | 233 |
| Okrzyk bojowy | 24048 | 4% | 529 | 45 |
| Cięcie krzyżowe | 21533 | 4% | 113 | 191 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×63.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 324491 | 31% | 1431 | 227 |
| Rozpłatanie | 316163 | 31% | 1011 | 313 |
| Zamach | 123842 | 12% |  |  |
| Fala przełamania | 91474 | 9% | 218 | 420 |
| Szarża | 82021 | 8% | 550 | 149 |
| Egzekucja | 43212 | 4% |  |  |
| Okrzyk bojowy | 25771 | 2% | 282 | 91 |
| Cięcie | 6988 | 1% |  |  |
| Szarża barkiem | 5658 | 1% |  |  |
| Potężne cięcie | 5430 | 1% |  |  |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,8, mediana 6,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 24% | 22,3 | 177 | 0,8 |
| odblokowanie | 14% | 12,9 | 165 | 0,5 |
| klasa | 19% | 17,6 | 45 | 0,0 |
| nagroda | 6% | 5,2 | 115 | 0,3 |
| sklep | 36% | 33,0 | 145 | 0,6 |

Oferty sklepu dusz: widziane 122, kupione 90, odrzucone 0, za mało dusz 32.
Zamiany w slotach: 3,0 na podejście; najczęstsze: Tąpnięcie → Cięcie wichru ×8, Tąpnięcie → Rozpłatanie ×8, Rozpłatanie → Tąpnięcie ×6, Cięcie wichru → Odtrącenie ×5, Tąpnięcie → Cięcie krzyżowe ×3, Rozpłatanie → Cięcie krzyżowe ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 30 | 17,6 | 45 | 0,0 |
| Cięcie wichru | startowy | 17 | 23,9 | 170 | 0,8 |
| Tąpnięcie | odblokowanie | 30 | 12,9 | 165 | 0,5 |
| Rozpłatanie | startowy | 27 | 14,3 | 174 | 0,6 |
| Upadek kata | sklep | 8 | 18,0 | 233 | 0,7 |
| Fala przełamania | sklep | 6 | 22,3 | 315 | 0,6 |
| Cięcie z wyskoku | sklep | 11 | 10,8 | 174 | 0,5 |
| Cięcie krzyżowe | sklep | 8 | 14,1 | 191 | 0,7 |
| Kontra | sklep | 7 | 15,1 | 30 | 0,3 |
| Odtrącenie | sklep | 9 | 10,8 | 24 | 0,6 |
| Wypad | sklep | 7 | 11,7 | 18 | 0,7 |
| Szarża | nagroda | 5 | 14,0 | 150 | 0,4 |
| Okrzyk wojenny | sklep | 6 | 10,5 | – | 0,9 |
| Krwawe cięcie | sklep | 5 | 12,0 | 103 | 0,3 |
| Uderzenie tarczą | sklep | 3 | 12,0 | 65 | 0,2 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,4, mediana 4,0, najwięcej 5 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 43% | 50,4 | 225 | 1,5 |
| odblokowanie | 39% | 46,4 | 259 | 1,7 |
| klasa | 8% | 9,4 | 91 | 0,9 |
| nagroda | 6% | 6,6 | 362 | 1,7 |
| sklep | 4% | 5,2 | 311 | 1,4 |

Oferty sklepu dusz: widziane 166, kupione 10, odrzucone 106, za mało dusz 50.
Zamiany w slotach: 0,8 na podejście; najczęstsze: Szarża → Fala przełamania ×11, Szarża → Rozpłatanie ×4, Okrzyk wojenny → Kontra ×1, Tąpnięcie → Rozpłatanie ×1, Rozpłatanie → Uderzenie tarczą ×1, Szarża → Kontra ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 28 | 51,1 | 227 | 1,5 |
| Rozpłatanie | nagroda | 34 | 29,7 | 313 | 2,0 |
| Szarża | odblokowanie | 26 | 21,2 | 149 | 1,1 |
| Okrzyk bojowy | klasa | 30 | 9,4 | 91 | 0,9 |
| Fala przełamania | nagroda | 12 | 18,2 | 420 | 1,5 |
| Tąpnięcie | odblokowanie | 2 | 8,5 | 92 | 0,4 |
| Krwawe cięcie | odblokowanie | 2 | 5,5 | 200 | 2,1 |
| Kontra | odblokowanie | 2 | 4,0 | 112 | 0,0 |
| Cięcie krzyżowe | sklep | 1 | 6,0 | 208 | 1,5 |
| Uderzenie tarczą | odblokowanie | 1 | 5,0 | 49 | 0,8 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 91,0, szybki atak 39,2, drgnięcie (trafiony) 30,2, unik 24,1, blok 16,1, mocny atak 13,4, flaszka (próba) 5,8, przełamana garda 5,1, parowanie 2,2, riposta 0,5.
Czas w bloku 6 s, w biegu 18 s, bez wytrzymałości (<10%) 97 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Obrońca ×7, wzmocnienie: Oddech wojownika ×6, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×5, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×4, nowy skill: Szarża ×3, wzmocnienie: Żelazna skóra ×3, wzmocnienie: Ostrze gniewu ×3, nowy skill: Kontra ×3.
Zakupy: ulepszenie broni ×25, ulepszenie: Rozpłatanie ×20, ulepszenie: Tąpnięcie ×17, ulepszenie: Cięcie wichru ×16, oferta: Rozpłatanie ×12, oferta: Cięcie z wyskoku ×10, oferta: Upadek kata ×8, oferta: Odtrącenie ×7.
Punkty cech: Strength 43, Toughness 32, Dexterity 27, Intelligence 20.
Komunikaty gry: „OKRZYK!” ×503; „SZAŁ!” ×256; „GARDA PRZEŁAMANA!” ×155; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×114; „Portale gasną – dobij pozostałe demony” ×100; „Garda wroga przełamana!” ×35; „Okrzyk wojenny: +29% obrażeń” ×35; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×32.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 118,0, unik 43,6, szybki atak 32,8, drgnięcie (trafiony) 31,2, blok 21,6, mocny atak 19,4, przełamana garda 6,7, flaszka (próba) 4,7, parowanie 1,0, riposta 0,3.
Czas w bloku 10 s, w biegu 19 s, bez wytrzymałości (<10%) 104 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +3 ×18, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×12, wzmocnienie: Żelazna skóra ×11, wzmocnienie: Zwinność ×9, wzmocnienie: Oddech wojownika ×9, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×8, ulepszenie: Ulepszenie: Szarża → +1 ×7, nowy skill: Fala przełamania ×7.
Zakupy: ulepszenie: Rozpłatanie ×64, ulepszenie: Cięcie wichru ×44, ulepszenie: Okrzyk bojowy ×41, ulepszenie broni ×33, ulepszenie: Szarża ×18, ulepszenie: Fala przełamania ×17, dodatkowa flaszka ×8, oferta: Fala przełamania ×5.
Punkty cech: Toughness 65, Strength 101.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Puklerz, Krwawe cięcie, Wypad, Kontra, Sztylet parujący, Łuk refleksyjny, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×262; „GARDA PRZEŁAMANA!” ×210; „SZAŁ!” ×167; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×145; „Portale gasną – dobij pozostałe demony” ×125; „Garda wroga przełamana!” ×63; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×50; „POLEGŁEŚ” ×30.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×267, garda nie zatrzymała ciosu ×10, zablokowany ruch ×4, parowanie w oknie nie zadziałało ×1.
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-5,6; 7,9)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; 8,1)
  - Czerw: „Ugryzienie” przeszedł przez gardę (kąt 161°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; -0,2)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 137°)
  - Ogar otchłani: „Ogar otchłani” przeszedł przez gardę (kąt 122°)
  - Kamienny chochlik: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 127°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 128°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (5,6; -0,6)
  - Wiedźma wichru: „Wiedźma wichru: CycloneEye” przeszedł przez gardę (kąt 134°)
  - Wiedźma wichru: „Wiedźma wichru: CycloneEye” przeszedł przez gardę (kąt 137°)
  - Wiedźma wichru: „Wiedźma wichru: CycloneEye” przeszedł przez gardę (kąt 130°)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×297, garda nie zatrzymała ciosu ×5, zablokowany ruch ×1, parowanie w oknie nie zadziałało ×1.
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 164°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (5,6; 8,1)
  - Siarkowy pomiot: „Siarkowy pomiot” przeszedł przez gardę (kąt 143°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 79°)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 164°)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 177°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 174°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 30 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (4 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 1× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Kamienny chochlik: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 127°)
- **Do sprawdzenia (P3):** 10× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Czerw: „Ugryzienie” przeszedł przez gardę (kąt 161°)
- Parowanie zwykłych ciosów: 76% ciosów mimo to rani (n=91) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 44% ciosów mimo to rani (n=34) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 35% ciosów (n=51).
- Blok zwykłych ciosów: 82% ciosów mimo to rani (n=186) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 99% ciosów mimo to rani (n=115) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 40% pocisków, przed którymi był unik (n=345).
- Blok pocisku: 87% ciosów mimo to rani (n=306) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 80% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1988) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=156).
- Telegraf „nieparowalny” daje czas: tylko 14% decyzji zapada mniej niż 0,1 s przed trafieniem (n=229).
- Telegraf „obszarowy” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=344).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Brak broni/tarczy zdolnej do parowania” ×1 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 74 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 30 podejściach. Średnio dochodzę do piętra 6,5 – na razie to ściana, nie krzywa.
- Progres między podejściami słabo odczuwalny: 6,5 → 6,5 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 41% udanych (n=61). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (24,1 trafień, 31,6 s), Władca Bramy (16,3 trafień, 21,6 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta (1,0), Ożywiony nietoperz (1,0), Kamienny chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- 28/30 razy zginąłem z flaszkami w kieszeni. 3 z tych śmierci to utrata ≥50% HP w 3 s, a 11% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 82% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 420 obrażeń na użycie.
- Skill startowy to 43% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 10%. Różnych skilli na podejście: średnio 4,4.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (24×, śr. piętro 6,9). Inne: Miecz i tarcza + Cięcie wichru 3× (5,3), Miecz i tarcza + Rozpłatanie 2× (4,5), Dwuręczny + Rozpłatanie 1× (5,0).
- Unik to moje główne narzędzie: 43,6 na podejście, w tym 4,7 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 77% przypadków.
- Gra nie wykonała uniku od razu średnio 16,8× na podejście – najczęściej: w trakcie poprzedniego uniku 8,2, drgnięcie (trafiony) 3,1, brak wytrzymałości 2,8. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (31% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (31 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

