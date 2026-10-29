# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 09:19. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (50 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (50 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 50 | 45 (90%) | 5,6 | 6,0 | 6,0 min | 111,6 | 18428 / 1253 | 3,6 | 20% |
| Gracz H&S / souls-like | 50 | 39 (78%) | 5,1 | 6,0 | 5,1 min | 102,8 | 16610 / 797 | 1,5 | 30% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 3 ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ 2 2 ★ ★ ★ ★ ★ ★ ★
- Gracz H&S / souls-like: 2 ★ 2 2 ★ ★ ★ ★ ★ ★ ★ 2 ★ 2 ★ ★ 2 ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ 2 ★ 3 ★ ★ 2 ★ ★ ★ ★ ★ ★ ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×4, 3. III. Władca Bramy ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×3; Władca Bramy – „Zamaszyste cięcie” ×1; Czerw – „Ugryzienie” ×1. Śmierć ciosem ≥40% maks. HP: 0/5. Zginął mając flaszki: 3/5.
  Ostatnie 3 s przed śmiercią: średnio 32% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/5). Picie: 203 prób, 181 wypitych (11% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×10, 3. III. Władca Bramy ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×4; Kamienny chochlik – „Pazury” ×3; Chochlik wichru – „Pazury” ×1; Czerw – „Ugryzienie” ×1; Kultysta – „Wachlarz ognia” ×1; Władca Bramy – „Piekielne kule” ×1. Śmierć ciosem ≥40% maks. HP: 0/11. Zginął mając flaszki: 11/11.
  Ostatnie 3 s przed śmiercią: średnio 32% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/11). Picie: 79 prób, 75 wypitych (5% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 50 | 50 | 69 s | 29 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 50 | 46 | 74 s | 218 | 0,8 | 46,5 |
| Tester QA | 3. III. Władca Bramy | 46 | 45 | 44 s | 448 | 1,7 | 7,8 |
| Tester QA | 4. IV. Zbrojownia | 45 | 45 | 82 s | 204 | 0,3 | 15,9 |
| Tester QA | 5. V. Brama Strażnika | 45 | 45 | 93 s | 375 | 1,1 | 14,2 |
| Tester QA | 6. VI. Szczyt Wieży | 45 | 45 | 19 s | 82 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 50 | 50 | 69 s | 28 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 50 | 40 | 72 s | 224 | 0,6 | 44,5 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 40 | 39 | 51 s | 329 | 1,0 | 8,1 |
| Gracz H&S / souls-like | 4. IV. Zbrojownia | 39 | 39 | 71 s | 174 | 0,1 | 14,1 |
| Gracz H&S / souls-like | 5. V. Brama Strażnika | 39 | 39 | 74 s | 160 | 0,1 | 12,9 |
| Gracz H&S / souls-like | 6. VI. Szczyt Wieży | 39 | 39 | 19 s | 27 | 0,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Władca Bramy | 84 | 43,1 s | 30,1 | 1239 | 31415 | 374 | 2 |
| Kultysta | 1251 | 0,3 s | 1,1 | 1061 | 16291 | 13 | 8 |
| Szkieletowy łucznik | 588 | 2,4 s | 2,5 | 1218 | 13812 | 23 | 0 |
| Kasztelan Potępionych | 84 | 15,1 s | 13,1 | 294 | 4735 | 56 | 0 |
| Harpia | 84 | 0,7 s | 1,7 | 345 | 3608 | 43 | 0 |
| Kolos | 84 | 2,4 s | 3,2 | 104 | 2948 | 35 | 0 |
| Ożywiony nietoperz | 857 | 0,5 s | 1,3 | 580 | 2782 | 3 | 0 |
| Lodowy strażnik | 96 | 10,2 s | 9,0 | 161 | 2691 | 28 | 0 |
| Strażnik Bramy | 156 | 8,1 s | 7,8 | 234 | 2622 | 17 | 0 |
| Czerw | 1907 | 0,3 s | 1,2 | 557 | 2259 | 1 | 2 |
| Upiór | 84 | 4,3 s | 3,2 | 284 | 2033 | 24 | 0 |
| Tkaczka | 84 | 2,1 s | 2,2 | 187 | 2005 | 24 | 0 |
| Kamienny chochlik | 1077 | 0,1 s | 1,1 | 187 | 1971 | 2 | 3 |
| Ognisty chochlik | 997 | 0,1 s | 1,1 | 163 | 1747 | 2 | 0 |
| Nekromanta | 84 | 4,3 s | 2,4 | 411 | 1673 | 20 | 0 |
| Kultysta topieli | 84 | 0,0 s | 1,0 | 89 | 1469 | 17 | 0 |
| Chochlik wichru | 1100 | 0,5 s | 1,4 | 152 | 1420 | 1 | 1 |
| Wodny chochlik | 1025 | 0,1 s | 1,1 | 127 | 1252 | 1 | 0 |
| Heretyk | 49 | 2,4 s | 2,6 | 72 | 1164 | 24 | 0 |
| Kultysta burzy | 35 | 3,7 s | 3,3 | 47 | 945 | 27 | 0 |
| Pajączek | 138 | 0,5 s | 1,3 | 113 | 795 | 6 | 0 |
| Gargulec | 84 | 2,7 s | 3,7 | 48 | 684 | 8 | 0 |
| Taranowiec | 84 | 1,6 s | 2,3 | 61 | 644 | 8 | 0 |
| Ghul Wieży | 104 | 2,0 s | 2,8 | 78 | 536 | 5 | 0 |
| Płonący ghul | 64 | 2,3 s | 3,0 | 50 | 499 | 8 | 0 |
| Legionista | 84 | 5,5 s | 5,8 | 100 | 396 | 5 | 0 |
| Ogar otchłani | 84 | 0,2 s | 1,7 | 15 | 99 | 1 | 0 |
| Ognisty chochlik (elita) | 138 | 4,3 s | 4,9 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 36 | 4,9 s | 4,2 | ↑ | ↑ | ↑ |  |
| Worek jaj | 78 | 0,3 s | 1,1 | 0 | 0 | 0 | 0 |
| Czerw (elita) | 15 | 3,9 s | 3,2 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1688 | 74% | 3% | 4% | 18% | 2% |
| ciężki | 108 | 39% | 11% | 7% | 41% | 2% |
| nieparowalny | 2232 | 77% | 7% | 0% | 9% | 7% |
| nieblokowalny | 192 | 63% | 0% | 2% | 35% | 0% |
| obszarowy | 389 | 83% | 10% | 0% | 0% | 7% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1523 | 53% | 4% | 12% | 30% | 1% |
| ciężki | 129 | 17% | 5% | 26% | 53% | 0% |
| nieparowalny | 1384 | 68% | 5% | 0% | 18% | 8% |
| nieblokowalny | 109 | 50% | 0% | 9% | 40% | 0% |
| obszarowy | 223 | 65% | 32% | 0% | 0% | 4% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1927 | 19% | 41% | 5% | 11% | 24% |
| Tester QA (0,30 s) | ciężki | 178 | 0% | 0% | 0% | 40% | 60% |
| Tester QA (0,30 s) | nieparowalny | 483 | 4% | 5% | 4% | 9% | 77% |
| Tester QA (0,30 s) | nieblokowalny | 186 | 3% | 0% | 4% | 12% | 81% |
| Tester QA (0,30 s) | obszarowy | 201 | 6% | 1% | 4% | 42% | 46% |
| Tester QA (0,30 s) | pocisk | 1490 | 2% | 24% | 15% | 24% | 34% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 1847 | 1% | 22% | 43% | 12% | 21% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 159 | 0% | 1% | 0% | 1% | 98% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 380 | 4% | 0% | 6% | 10% | 80% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 170 | 0% | 0% | 1% | 4% | 95% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 165 | 7% | 2% | 5% | 26% | 59% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 1424 | 1% | 17% | 12% | 19% | 51% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Pajączek: Pajączek | 0,31 s | 200 |
| Czerw: Ugryzienie | 0,33 s | 944 |
| Płonący ghul: Druga seria | 0,33 s | 11 |
| Ognisty chochlik: Pazury | 0,33 s | 344 |
| Ghul Wieży: Druga seria | 0,33 s | 14 |
| Chochlik wichru: Pazury | 0,33 s | 389 |
| Kamienny chochlik: Pazury | 0,33 s | 414 |
| Wodny chochlik: Pazury | 0,33 s | 361 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 913 |
| Władca Bramy: Cięcie w szale | 0,41 s | 79 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 67% (n=104) | 0% (n=7) | 64% (n=256) | – | 35% (n=43) | 87% (n=357) |
| blok (eksperyment) | 82% (n=97) | 33% (n=9) | 86% (n=21) | 100% (n=12) | 71% (n=7) | 62% (n=78) |
| brak reakcji (eksperyment) | 95% (n=85) | 100% (n=6) | 100% (n=41) | 100% (n=5) | 100% (n=16) | 96% (n=77) |
| niezauważony | 88% (n=250) | – | 96% (n=690) | 100% (n=68) | 100% (n=65) | – |
| parowanie | 74% (n=53) | 25% (n=8) | – | 0% (n=3) | – | – |
| parowanie (eksperyment) | 61% (n=54) | 60% (n=5) | 94% (n=17) | 75% (n=4) | 100% (n=4) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 99% (n=270) |
| unik | 43% (n=416) | 8% (n=38) | 48% (n=168) | 26% (n=86) | – | 78% (n=182) |
| unik (eksperyment) | 74% (n=109) | 20% (n=10) | 57% (n=86) | 33% (n=6) | 100% (n=10) | 87% (n=55) |
| wyjście z zasięgu | 95% (n=384) | 100% (n=21) | 100% (n=89) | 100% (n=3) | 92% (n=51) | – |
| wyjście z zasięgu (eksperyment) | 93% (n=82) | 100% (n=4) | 100% (n=26) | 100% (n=5) | 100% (n=8) | 95% (n=58) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 23,1 | reakcja na cios wręcz 22,0, unik przed pociskiem 10,2, wyjście z okrążenia 0,7 | drgnięcie (trafiony) 2,9, brak wytrzymałości 2,3, w trakcie uniku 1,4, picie flaszki 1,0, inna 0,5, parowanie 0,2, unieruchomienie 0,2, umiejętność 0,1, szybki atak 0,1, mocny atak 0,1, riposta 0,0 | 61% (n=1101) / 63% (n=511) |
| Gracz H&S / souls-like | 31,3 | reakcja na cios wręcz 21,3, unik przed pociskiem 10,6, doskok do odsłoniętego celu 6,9, wyjście z okrążenia 0,8 | w trakcie uniku 3,3, drgnięcie (trafiony) 1,8, brak wytrzymałości 0,9, parowanie 0,6, picie flaszki 0,4, inna 0,3, umiejętność 0,1, mocny atak 0,1, szybki atak 0,0, riposta 0,0, blok 0,0, unieruchomienie 0,0 | 80% (n=1066) / 69% (n=528) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 12 | 12 | 6,0 | 116,8 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 13 | 12 | 5,7 | 111,9 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 13 | 11 | 5,5 | 112,9 |
| Tester QA | Dwuręczny | Rozpłatanie | 12 | 10 | 5,3 | 104,7 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 45 | 38 | 5,4 | 106,4 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 3 | 1 | 3,3 | 71,7 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 2,0 | 71,0 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 1 | 0 | 2,0 | 66,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 143185 | 16% | 865 | 166 |
| Rozpłatanie | 140318 | 15% | 821 | 171 |
| Zamach | 140188 | 15% |  |  |
| Tąpnięcie | 107041 | 12% | 865 | 124 |
| Cięcie | 77314 | 8% |  |  |
| Egzekucja | 59139 | 6% | 73 | 810 |
| Potężne cięcie | 33480 | 4% |  |  |
| Upadek kata | 33332 | 4% | 223 | 149 |
| Okrzyk bojowy | 31241 | 3% | 1157 | 27 |
| Fala przełamania | 29121 | 3% | 114 | 255 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×77.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 326432 | 39% | 2142 | 152 |
| Rozpłatanie | 222337 | 27% | 1798 | 124 |
| Szarża | 87615 | 11% | 1615 | 54 |
| Cięcie | 78773 | 9% |  |  |
| Potężne cięcie | 41551 | 5% |  |  |
| Riposta | 39319 | 5% |  |  |
| Okrzyk bojowy | 14084 | 2% | 212 | 66 |
| Młynek | 3756 | 0% | 19 | 198 |
| Szarża barkiem | 3677 | 0% |  |  |
| Tąpnięcie | 2639 | 0% | 21 | 126 |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,9, mediana 6,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 26% | 30,9 | 171 | 1,0 |
| odblokowanie | 14% | 17,3 | 124 | 1,3 |
| klasa | 19% | 23,1 | 27 | 0,5 |
| nagroda | 4% | 5,0 | 79 | 1,0 |
| sklep | 36% | 43,2 | 89 | 1,2 |

Oferty sklepu dusz: widziane 231, kupione 167, odrzucone 0, za mało dusz 64.
Zamiany w slotach: 3,4 na podejście; najczęstsze: Rozpłatanie → Tąpnięcie ×14, Tąpnięcie → Cięcie wichru ×11, Cięcie wichru → Tąpnięcie ×8, Tąpnięcie → Cięcie z wyskoku ×7, Tąpnięcie → Rozpłatanie ×7, Tąpnięcie → Kontra ×5.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 50 | 23,1 | 27 | 0,5 |
| Tąpnięcie | odblokowanie | 50 | 17,3 | 124 | 1,3 |
| Cięcie wichru | startowy | 27 | 32,0 | 166 | 0,9 |
| Rozpłatanie | startowy | 34 | 24,1 | 171 | 1,0 |
| Odtrącenie | sklep | 12 | 20,2 | 6 | 1,0 |
| Upadek kata | sklep | 15 | 14,9 | 149 | 1,2 |
| Kontra | sklep | 9 | 21,3 | 16 | 0,8 |
| Cięcie z wyskoku | nagroda | 14 | 13,2 | 99 | 1,1 |
| Młynek | sklep | 11 | 15,7 | 77 | 1,2 |
| Cięcie krzyżowe | sklep | 11 | 15,4 | 117 | 1,1 |
| Szarża | sklep | 7 | 24,1 | 76 | 1,3 |
| Uderzenie tarczą | sklep | 5 | 30,2 | 51 | 1,7 |
| Krwawe cięcie | sklep | 8 | 16,9 | 103 | 1,3 |
| Wypad | sklep | 5 | 26,4 | 13 | 1,2 |
| Fala przełamania | sklep | 8 | 14,3 | 255 | 1,1 |
| Rozdarcie ran | sklep | 6 | 13,2 | 49 | 1,2 |
| Seria cięć | sklep | 6 | 12,8 | 65 | 1,1 |
| Okrzyk wojenny | sklep | 7 | 11,0 | – | 1,1 |
| Piekielna szarża | sklep | 6 | 12,3 | 150 | 1,1 |
| Egzekucja | sklep | 5 | 14,6 | 232 | 1,1 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,1, mediana 4,0, najwięcej 9 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 39% | 45,3 | 153 | 1,3 |
| odblokowanie | 56% | 66,1 | 89 | 1,9 |
| klasa | 4% | 4,2 | 66 | 1,1 |
| nagroda | 0% | 0,3 | 112 | 1,1 |
| sklep | 1% | 1,4 | 111 | 1,6 |

Oferty sklepu dusz: widziane 207, kupione 9, odrzucone 157, za mało dusz 41.
Zamiany w slotach: 0,3 na podejście; najczęstsze: Tąpnięcie → Krwawe cięcie ×1, Tąpnięcie → Cięcie krzyżowe ×1, Cięcie krzyżowe → Seria cięć ×1, Rozpłatanie → Młynek ×1, Rozpłatanie → Rozdarcie ran ×1, Rozpłatanie → Uderzenie tarczą ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 46 | 46,6 | 152 | 1,3 |
| Rozpłatanie | sklep | 49 | 36,7 | 124 | 2,0 |
| Szarża | odblokowanie | 47 | 34,4 | 54 | 1,8 |
| Okrzyk bojowy | klasa | 50 | 4,2 | 66 | 1,1 |
| Tąpnięcie | odblokowanie | 2 | 10,5 | 126 | 0,8 |
| Młynek | odblokowanie | 3 | 6,3 | 198 | 0,8 |
| Kontra | odblokowanie | 2 | 5,5 | 93 | 0,4 |
| Krwawe cięcie | odblokowanie | 2 | 5,0 | 85 | 1,0 |
| Rozdarcie ran | odblokowanie | 1 | 9,0 | 118 | 1,1 |
| Cięcie krzyżowe | sklep | 1 | 8,0 | 112 | 1,6 |
| Seria cięć | nagroda | 2 | 3,0 | 127 | 1,0 |
| Egzekucja | odblokowanie | 1 | 6,0 | 128 | 1,0 |
| Fala przełamania | nagroda | 1 | 6,0 | 177 | 1,0 |
| Uderzenie tarczą | odblokowanie | 1 | 5,0 | 54 | 0,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 119,5, szybki atak 50,4, drgnięcie (trafiony) 37,1, unik 23,1, mocny atak 17,3, blok 13,1, przełamana garda 4,1, flaszka (próba) 4,1, parowanie 2,3, riposta 0,7.
Czas w bloku 7 s, w biegu 25 s, bez wytrzymałości (<10%) 98 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Oddech wojownika ×13, wzmocnienie: Żelazna skóra ×9, ulepszenie: Ulepszenie: Tąpnięcie → +3 ×8, wzmocnienie: Ostrze gniewu ×7, wzmocnienie: Siła tytana ×7, wzmocnienie: Zwinność ×6, ulepszenie: Ulepszenie: Tąpnięcie → +2 ×6, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×5.
Zakupy: ulepszenie broni ×90, ulepszenie: Tąpnięcie ×80, ulepszenie: Okrzyk bojowy ×49, ulepszenie: Rozpłatanie ×48, ulepszenie: Cięcie wichru ×36, dodatkowa flaszka ×26, ulepszenie: Upadek kata ×20, oferta: Upadek kata ×13.
Punkty cech: Strength 95, Toughness 46, Dexterity 45, Intelligence 45.
Komunikaty gry: „OKRZYK!” ×1110; „SZAŁ!” ×374; „GARDA PRZEŁAMANA!” ×207; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×100; „Portale gasną – dobij pozostałe demony” ×97; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×90; „PAROWANIE!” ×52; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×46.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 117,4, unik 31,3, szybki atak 30,5, drgnięcie (trafiony) 23,6, mocny atak 14,3, blok 11,3, parowanie 5,2, riposta 3,0, przełamana garda 2,8, flaszka (próba) 1,6.
Czas w bloku 5 s, w biegu 21 s, bez wytrzymałości (<10%) 57 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Szarża → +3 ×23, ulepszenie: Ulepszenie: Szarża → +4 ×17, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×16, wzmocnienie: Strażnik zaklęć ×13, ulepszenie: Ulepszenie: Szarża → +1 ×12, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×12, wzmocnienie: Zwinność ×11, ulepszenie: Ulepszenie: Szarża → +2 ×11.
Zakupy: ulepszenie: Rozpłatanie ×120, ulepszenie: Okrzyk bojowy ×87, ulepszenie: Szarża ×80, ulepszenie broni ×78, ulepszenie: Cięcie wichru ×77, dodatkowa flaszka ×36, ulepszenie: Młynek ×4, oferta: Fala przełamania ×4.
Punkty cech: Toughness 89, Strength 118.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Kosa, Przetrwanie, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „PAROWANIE!” ×211; „OKRZYK!” ×186; „Postawa przełamana – RIPOSTA!” ×172; „RIPOSTA!” ×147; „GARDA PRZEŁAMANA!” ×143; „SZAŁ!” ×115; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×100; „Portale gasną – dobij pozostałe demony” ×95.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×464, uśpiony wróg blokuje koniec piętra ×34, garda nie zatrzymała ciosu ×17, zablokowany ruch ×15, parowanie w oknie nie zadziałało ×1.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 34 podejściach)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 142°)
  - Wodny chochlik: „Pazury” przeszedł przez gardę (kąt 131°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 131°)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (5,6; -0,4)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,8; 0,1)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,8; 0,6)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 161°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,3; -6,9)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (5,6; 0,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,2)
  - Szkieletowy łucznik: „Strzał w nogi” przeszedł przez gardę (kąt 153°)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×237, uśpiony wróg blokuje koniec piętra ×28, zablokowany ruch ×11, parowanie w oknie nie zadziałało ×5.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 28 podejściach)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 0,2)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; -0,1)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,8; -0,1)
  - Taranowiec: „Taranowiec” trafił w aktywnym oknie parowania (kąt do źródła 114°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,2; -1,1)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,3; 6,9)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 8,9)
  - Kamienny chochlik: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 114°)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-5,6; 8,0)
  - piętro 5 „V. Brama Strażnika”: brak postępu przy (7,6; -7,0)
  - Kasztelan Potępionych: „Szeroki zamach” trafił w aktywnym oknie parowania (kąt do źródła 0°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 50 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (15 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 1× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 88°)
- **Do sprawdzenia (P3):** 17× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta: „Kula ognia” przeszedł przez gardę (kąt 142°)
- Parowanie zwykłych ciosów: 67% ciosów mimo to rani (n=107) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 26% ciosów (n=92).
- Unik przed ciężkim: działa – rani tylko 10% ciosów (n=48).
- Blok zwykłych ciosów: 75% ciosów mimo to rani (n=201) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 93% ciosów mimo to rani (n=59) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 37% pocisków, przed którymi był unik (n=511).
- Blok pocisku: 82% ciosów mimo to rani (n=435) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 60% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1927) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=178).
- Telegraf „nieparowalny” daje czas: tylko 9% decyzji zapada mniej niż 0,1 s przed trafieniem (n=483).
- Telegraf „nieblokowalny” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=186).
- Telegraf „obszarowy” daje czas: tylko 7% decyzji zapada mniej niż 0,1 s przed trafieniem (n=201).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 82 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 39/50 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Czuję progres: druga połowa podejść średnio piętro 6,2 wobec 5,6 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 48% udanych (n=380). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Władca Bramy (35,2 trafień, 46,9 s), Kasztelan Potępionych (14,1 trafień, 15,5 s), Lodowy strażnik (8,5 trafień, 8,7 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta topieli (1,0), Kamienny chochlik (1,0), Wodny chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 47 s i 35 trafień do zabicia.
- Mój nemezis: Kultysta – 5 z 11 śmierci (najczęściej „Kula ognia”).
- 11/11 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 5% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 80% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Egzekucja”: 319 obrażeń na użycie.
- Skill startowy to 39% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 1%. Różnych skilli na podejście: średnio 4,1.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (45×, śr. piętro 6,2). Inne: Miecz i tarcza + Rozpłatanie 3× (3,7), Dwuręczny + Rozpłatanie 1× (2,0), Dwuręczny + Cięcie wichru 1× (2,0).
- Unik to moje główne narzędzie: 31,3 na podejście, w tym 7,7 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 69% przypadków.
- Gra nie wykonała uniku od razu średnio 7,5× na podejście – najczęściej: w trakcie poprzedniego uniku 3,3, drgnięcie (trafiony) 1,8, brak wytrzymałości 0,9. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (18% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.

