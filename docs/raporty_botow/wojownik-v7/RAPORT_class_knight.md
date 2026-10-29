# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 10:12. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (50 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (50 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 50 | 37 (74%) | 5,1 | 6,0 | 5,4 min | 104,4 | 16267 / 1177 | 3,4 | 18% |
| Gracz H&S / souls-like | 50 | 34 (68%) | 4,7 | 6,0 | 4,7 min | 98,1 | 15160 / 739 | 1,5 | 27% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: ★ ★ 2 ★ ★ ★ ★ 3 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 2 ★ 2 2 ★ 2 2 3 ★ 3 ★ ★ 2 ★ 2 ★ ★ 4 ★ ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★
- Gracz H&S / souls-like: ★ 2 2 ★ 2 2 2 2 ★ 2 ★ ★ 2 2 2 2 2 ★ ★ ★ 2 ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ 3 ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×9, 3. III. Władca Bramy ×3, 4. IV. Zbrojownia ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×6; Władca Bramy – „Piekielne kule” ×3; Czerw – „Ugryzienie” ×3; Płonący ghul – „Druga seria” ×1. Śmierć ciosem ≥40% maks. HP: 0/13. Zginął mając flaszki: 12/13.
  Ostatnie 3 s przed śmiercią: średnio 31% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/13). Picie: 177 prób, 168 wypitych (5% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×15, 3. III. Władca Bramy ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×6; Czerw – „Ugryzienie” ×3; Ognisty chochlik – „Pazury” ×2; Wodny chochlik – „Pazury” ×2; Chochlik wichru – „Pazury” ×1; Kamienny chochlik – „Pazury” ×1. Śmierć ciosem ≥40% maks. HP: 0/16. Zginął mając flaszki: 15/16.
  Ostatnie 3 s przed śmiercią: średnio 34% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/16). Picie: 79 prób, 74 wypitych (6% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 50 | 50 | 68 s | 27 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 50 | 41 | 72 s | 271 | 0,8 | 44,4 |
| Tester QA | 3. III. Władca Bramy | 41 | 38 | 46 s | 481 | 1,9 | 8,2 |
| Tester QA | 4. IV. Zbrojownia | 38 | 37 | 80 s | 191 | 0,2 | 15,9 |
| Tester QA | 5. V. Brama Strażnika | 37 | 37 | 95 s | 372 | 1,1 | 14,1 |
| Tester QA | 6. VI. Szczyt Wieży | 37 | 37 | 21 s | 86 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 50 | 50 | 68 s | 24 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 50 | 35 | 71 s | 257 | 0,6 | 42,4 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 35 | 34 | 45 s | 302 | 0,9 | 8,2 |
| Gracz H&S / souls-like | 4. IV. Zbrojownia | 34 | 34 | 74 s | 177 | 0,1 | 14,9 |
| Gracz H&S / souls-like | 5. V. Brama Strażnika | 34 | 34 | 76 s | 176 | 0,2 | 13,4 |
| Gracz H&S / souls-like | 6. VI. Szczyt Wieży | 34 | 34 | 16 s | 9 | 0,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Władca Bramy | 72 | 41,8 s | 30,1 | 1093 | 28643 | 398 | 4 |
| Kultysta | 1178 | 0,4 s | 1,1 | 1287 | 18848 | 16 | 12 |
| Szkieletowy łucznik | 499 | 2,3 s | 2,5 | 1070 | 12144 | 24 | 0 |
| Kasztelan Potępionych | 71 | 14,4 s | 12,8 | 216 | 3488 | 49 | 0 |
| Harpia | 71 | 0,7 s | 1,8 | 282 | 3142 | 44 | 0 |
| Czerw | 1778 | 0,3 s | 1,3 | 658 | 2483 | 1 | 6 |
| Ognisty chochlik | 1071 | 0,1 s | 1,1 | 217 | 2264 | 2 | 2 |
| Kolos | 72 | 2,4 s | 3,1 | 79 | 2256 | 31 | 0 |
| Ożywiony nietoperz | 806 | 0,5 s | 1,3 | 467 | 2194 | 3 | 0 |
| Strażnik Bramy | 133 | 8,4 s | 8,1 | 177 | 2009 | 15 | 0 |
| Tkaczka | 71 | 2,8 s | 2,4 | 177 | 1920 | 27 | 0 |
| Kamienny chochlik | 1075 | 0,1 s | 1,1 | 185 | 1915 | 2 | 1 |
| Chochlik wichru | 1023 | 0,5 s | 1,4 | 176 | 1783 | 2 | 1 |
| Upiór | 71 | 3,8 s | 3,0 | 263 | 1771 | 25 | 0 |
| Wodny chochlik | 1033 | 0,1 s | 1,1 | 185 | 1737 | 2 | 2 |
| Lodowy strażnik | 80 | 8,6 s | 8,2 | 100 | 1429 | 18 | 0 |
| Nekromanta | 71 | 3,6 s | 2,5 | 336 | 1231 | 17 | 0 |
| Kultysta topieli | 72 | 0,1 s | 1,1 | 66 | 1226 | 17 | 0 |
| Heretyk | 42 | 2,9 s | 3,0 | 56 | 932 | 22 | 0 |
| Kultysta burzy | 30 | 3,6 s | 3,5 | 40 | 889 | 30 | 0 |
| Taranowiec | 71 | 2,2 s | 2,4 | 65 | 840 | 12 | 0 |
| Pajączek | 129 | 0,4 s | 1,2 | 105 | 759 | 6 | 0 |
| Ghul Wieży | 94 | 2,5 s | 3,0 | 84 | 614 | 7 | 0 |
| Płonący ghul | 49 | 2,0 s | 3,0 | 47 | 486 | 10 | 1 |
| Gargulec | 71 | 2,3 s | 3,5 | 26 | 377 | 5 | 0 |
| Legionista | 71 | 4,6 s | 5,4 | 57 | 359 | 5 | 0 |
| Ogar otchłani | 72 | 0,2 s | 1,7 | 20 | 53 | 1 | 0 |
| Ognisty chochlik (elita) | 139 | 4,0 s | 4,7 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 31 | 4,6 s | 4,4 | ↑ | ↑ | ↑ |  |
| Worek jaj | 64 | 0,1 s | 1,1 | 0 | 0 | 0 | 0 |
| Czerw (elita) | 15 | 3,0 s | 3,2 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1655 | 76% | 3% | 2% | 16% | 3% |
| ciężki | 112 | 51% | 5% | 12% | 28% | 4% |
| nieparowalny | 2071 | 77% | 7% | 0% | 9% | 7% |
| nieblokowalny | 144 | 59% | 0% | 3% | 38% | 0% |
| obszarowy | 274 | 80% | 15% | 0% | 0% | 6% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1443 | 54% | 3% | 11% | 31% | 2% |
| ciężki | 96 | 26% | 9% | 16% | 49% | 0% |
| nieparowalny | 1419 | 66% | 9% | 0% | 16% | 9% |
| nieblokowalny | 98 | 43% | 0% | 10% | 47% | 0% |
| obszarowy | 222 | 63% | 33% | 0% | 0% | 5% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1886 | 20% | 45% | 5% | 10% | 21% |
| Tester QA (0,30 s) | ciężki | 164 | 1% | 0% | 0% | 40% | 60% |
| Tester QA (0,30 s) | nieparowalny | 421 | 6% | 4% | 6% | 7% | 77% |
| Tester QA (0,30 s) | nieblokowalny | 147 | 0% | 1% | 3% | 12% | 83% |
| Tester QA (0,30 s) | obszarowy | 159 | 9% | 4% | 4% | 39% | 43% |
| Tester QA (0,30 s) | pocisk | 1574 | 2% | 20% | 14% | 22% | 42% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 1755 | 1% | 26% | 45% | 10% | 18% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 149 | 0% | 0% | 0% | 2% | 98% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 276 | 6% | 0% | 7% | 7% | 80% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 153 | 1% | 1% | 0% | 7% | 92% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 155 | 3% | 3% | 3% | 29% | 61% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 1718 | 2% | 14% | 12% | 21% | 51% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Pajączek: Pajączek | 0,31 s | 186 |
| Ognisty chochlik: Pazury | 0,33 s | 438 |
| Czerw: Ugryzienie | 0,33 s | 1024 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 758 |
| Ghul Wieży: Druga seria | 0,33 s | 14 |
| Wodny chochlik: Pazury | 0,33 s | 428 |
| Kamienny chochlik: Pazury | 0,33 s | 425 |
| Chochlik wichru: Pazury | 0,33 s | 416 |
| Płonący ghul: Druga seria | 0,33 s | 13 |
| Władca Bramy: Cięcie w szale | 0,41 s | 60 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 72% (n=127) | 0% (n=4) | 62% (n=212) | 100% (n=3) | 6% (n=33) | 88% (n=406) |
| blok (eksperyment) | 85% (n=81) | 67% (n=9) | 77% (n=39) | 100% (n=2) | 85% (n=13) | 76% (n=80) |
| brak reakcji (eksperyment) | 94% (n=84) | 100% (n=14) | 100% (n=26) | 100% (n=4) | 100% (n=3) | 95% (n=76) |
| niezauważony | 91% (n=232) | – | 99% (n=606) | 100% (n=49) | 100% (n=37) | – |
| parowanie | 74% (n=43) | 27% (n=15) | – | 0% (n=3) | – | – |
| parowanie (eksperyment) | 70% (n=50) | 33% (n=3) | 100% (n=19) | 67% (n=6) | 100% (n=8) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 94% (n=215) |
| unik | 50% (n=383) | 32% (n=38) | 52% (n=164) | 21% (n=57) | 100% (n=1) | 76% (n=171) |
| unik (eksperyment) | 73% (n=112) | 50% (n=6) | 52% (n=50) | 25% (n=12) | 100% (n=6) | 92% (n=59) |
| wyjście z zasięgu | 95% (n=424) | 95% (n=22) | 100% (n=60) | – | 96% (n=25) | – |
| wyjście z zasięgu (eksperyment) | 89% (n=75) | 100% (n=1) | 92% (n=26) | 100% (n=8) | 100% (n=2) | 100% (n=52) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 20,5 | reakcja na cios wręcz 20,1, unik przed pociskiem 9,2, wyjście z okrążenia 1,0 | drgnięcie (trafiony) 3,1, brak wytrzymałości 2,4, w trakcie uniku 1,5, picie flaszki 0,8, inna 0,4, mocny atak 0,2, parowanie 0,1, umiejętność 0,1, unieruchomienie 0,0, blok 0,0, szybki atak 0,0, riposta 0,0 | 58% (n=1003) / 60% (n=461) |
| Gracz H&S / souls-like | 31,5 | reakcja na cios wręcz 19,3, unik przed pociskiem 13,9, doskok do odsłoniętego celu 7,8, wyjście z okrążenia 0,7 | w trakcie uniku 4,3, drgnięcie (trafiony) 1,9, brak wytrzymałości 1,0, parowanie 0,5, picie flaszki 0,4, inna 0,3, mocny atak 0,1, blok 0,1, szybki atak 0,0, unieruchomienie 0,0, umiejętność 0,0 | 82% (n=965) / 75% (n=696) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Rozpłatanie | 13 | 11 | 5,4 | 107,5 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 13 | 10 | 5,3 | 111,8 |
| Tester QA | Dwuręczny | Cięcie wichru | 12 | 9 | 5,2 | 105,4 |
| Tester QA | Dwuręczny | Rozpłatanie | 12 | 7 | 4,3 | 92,0 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 30 | 27 | 5,6 | 110,9 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 4 | 2 | 4,0 | 90,8 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 13 | 5 | 3,5 | 80,4 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 3 | 0 | 2,0 | 56,7 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 129401 | 16% | 801 | 162 |
| Zamach | 122601 | 15% |  |  |
| Rozpłatanie | 109082 | 13% | 769 | 142 |
| Tąpnięcie | 98604 | 12% | 808 | 122 |
| Cięcie | 80878 | 10% |  |  |
| Egzekucja | 52903 | 7% | 141 | 375 |
| Potężne cięcie | 35935 | 4% |  |  |
| Okrzyk bojowy | 30112 | 4% | 1041 | 29 |
| Upadek kata | 26472 | 3% | 159 | 166 |
| Cięcie z wyskoku | 22073 | 3% | 170 | 130 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×39.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Rozpłatanie | 252981 | 33% | 1953 | 130 |
| Cięcie wichru | 244673 | 32% | 1682 | 145 |
| Cięcie | 57301 | 8% |  |  |
| Potężne cięcie | 41808 | 6% |  |  |
| Szarża | 34795 | 5% | 644 | 54 |
| Riposta | 28454 | 4% |  |  |
| Fala przełamania | 28304 | 4% | 136 | 208 |
| Zamach | 19601 | 3% |  |  |
| Egzekucja | 15936 | 2% | 72 | 221 |
| Okrzyk bojowy | 14596 | 2% | 202 | 72 |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,5, mediana 6,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 28% | 29,5 | 153 | 1,0 |
| odblokowanie | 15% | 16,2 | 122 | 1,2 |
| klasa | 20% | 20,8 | 29 | 0,5 |
| nagroda | 4% | 4,3 | 103 | 1,2 |
| sklep | 34% | 36,0 | 85 | 1,2 |

Oferty sklepu dusz: widziane 203, kupione 153, odrzucone 0, za mało dusz 50.
Zamiany w slotach: 2,8 na podejście; najczęstsze: Rozpłatanie → Tąpnięcie ×12, Tąpnięcie → Rozpłatanie ×10, Tąpnięcie → Odtrącenie ×4, Cięcie wichru → Tąpnięcie ×4, Tąpnięcie → Cięcie wichru ×4, Rozpłatanie → Szarża ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 50 | 20,8 | 29 | 0,5 |
| Tąpnięcie | odblokowanie | 50 | 16,2 | 122 | 1,2 |
| Cięcie wichru | startowy | 26 | 30,8 | 162 | 0,9 |
| Rozpłatanie | startowy | 34 | 22,6 | 142 | 1,1 |
| Wypad | sklep | 10 | 25,8 | 13 | 1,2 |
| Szarża | nagroda | 8 | 22,3 | 50 | 1,5 |
| Młynek | sklep | 9 | 19,8 | 91 | 1,1 |
| Cięcie z wyskoku | sklep | 9 | 18,9 | 130 | 1,2 |
| Upadek kata | sklep | 8 | 19,9 | 166 | 1,0 |
| Odtrącenie | sklep | 10 | 14,3 | 8 | 1,0 |
| Egzekucja | nagroda | 10 | 14,1 | 112 | 1,0 |
| Krwawe cięcie | sklep | 6 | 18,2 | 91 | 2,0 |
| Uderzenie tarczą | sklep | 5 | 18,0 | 42 | 1,1 |
| Rozdarcie ran | sklep | 7 | 12,7 | 46 | 1,2 |
| Cięcie krzyżowe | sklep | 8 | 10,3 | 131 | 1,3 |
| Piekielna szarża | sklep | 4 | 19,8 | 186 | 1,4 |
| Kontra | sklep | 5 | 15,2 | 22 | 0,4 |
| Fala przełamania | sklep | 5 | 15,0 | 210 | 0,9 |
| Seria cięć | sklep | 9 | 5,7 | 137 | 1,0 |
| Okrzyk wojenny | sklep | 5 | 7,8 | – | 1,2 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,2, mediana 4,0, najwięcej 6 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 46% | 45,0 | 143 | 1,2 |
| odblokowanie | 40% | 38,4 | 103 | 1,7 |
| klasa | 4% | 4,0 | 72 | 0,9 |
| nagroda | 2% | 2,4 | 165 | 1,5 |
| sklep | 7% | 7,1 | 141 | 1,5 |

Oferty sklepu dusz: widziane 187, kupione 28, odrzucone 123, za mało dusz 36.
Zamiany w slotach: 1,1 na podejście; najczęstsze: Szarża → Egzekucja ×17, Szarża → Fala przełamania ×9, Szarża → Cięcie krzyżowe ×7, Szarża → Rozpłatanie ×6, Egzekucja → Fala przełamania ×2, Tąpnięcie → Młynek ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | startowy | 55 | 35,5 | 130 | 1,6 |
| Cięcie wichru | startowy | 36 | 46,7 | 145 | 1,4 |
| Szarża | odblokowanie | 31 | 20,8 | 54 | 1,4 |
| Okrzyk bojowy | klasa | 50 | 4,0 | 72 | 0,9 |
| Fala przełamania | nagroda | 12 | 11,3 | 208 | 1,4 |
| Cięcie krzyżowe | sklep | 8 | 10,3 | 131 | 1,6 |
| Egzekucja | odblokowanie | 14 | 5,1 | 165 | 1,6 |
| Tąpnięcie | odblokowanie | 1 | 21,0 | 105 | 1,2 |
| Kontra | sklep | 1 | 14,0 | 9 | 0,0 |
| Krwawe cięcie | sklep | 1 | 8,0 | 65 | 0,5 |
| Młynek | sklep | 1 | 7,0 | 111 | 1,3 |
| Rozdarcie ran | odblokowanie | 1 | 7,0 | 38 | 0,0 |
| Uderzenie tarczą | odblokowanie | 1 | 7,0 | 51 | 0,0 |
| Seria cięć | odblokowanie | 2 | 3,0 | 125 | 0,7 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 106,7, szybki atak 48,3, drgnięcie (trafiony) 33,8, unik 20,5, mocny atak 16,8, blok 13,4, przełamana garda 4,3, flaszka (próba) 3,5, parowanie 1,8, riposta 0,4.
Czas w bloku 6 s, w biegu 21 s, bez wytrzymałości (<10%) 96 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Żelazna skóra ×8, wzmocnienie: Siła tytana ×7, ulepszenie: Ulepszenie: Tąpnięcie → +2 ×6, wzmocnienie: Zwinność ×6, wzmocnienie: Czujność ×5, wzmocnienie: Oddech wojownika ×5, wzmocnienie: Ostrze gniewu ×5, wzmocnienie: Hart ducha ×4.
Zakupy: ulepszenie: Tąpnięcie ×75, ulepszenie broni ×75, ulepszenie: Rozpłatanie ×43, ulepszenie: Okrzyk bojowy ×40, ulepszenie: Cięcie wichru ×39, dodatkowa flaszka ×17, ulepszenie: Szarża ×14, ulepszenie: Krwawe cięcie ×14.
Punkty cech: Strength 87, Toughness 41, Dexterity 38, Intelligence 37.
Komunikaty gry: „OKRZYK!” ×985; „SZAŁ!” ×355; „GARDA PRZEŁAMANA!” ×222; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×100; „Portale gasną – dobij pozostałe demony” ×95; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×75; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×41; „PAROWANIE!” ×39.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 96,8, unik 31,5, szybki atak 24,1, drgnięcie (trafiony) 21,7, mocny atak 16,7, blok 13,2, parowanie 4,6, przełamana garda 3,1, riposta 2,1, flaszka (próba) 1,6.
Czas w bloku 6 s, w biegu 21 s, bez wytrzymałości (<10%) 45 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Oddech wojownika ×14, ulepszenie: Ulepszenie: Rozpłatanie → +3 ×13, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×10, wzmocnienie: Obrońca ×9, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×9, wzmocnienie: Czujność ×8, ulepszenie: Ulepszenie: Szarża → +1 ×8, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×8.
Zakupy: ulepszenie: Rozpłatanie ×127, ulepszenie: Okrzyk bojowy ×75, ulepszenie broni ×68, ulepszenie: Cięcie wichru ×58, dodatkowa flaszka ×30, ulepszenie: Szarża ×29, ulepszenie: Egzekucja ×20, ulepszenie: Cięcie krzyżowe ×14.
Punkty cech: Toughness 84, Strength 103.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Kontra, Młynek, Seria cięć, Egzekucja, Rozdarcie ran, Puklerz, Sztylet parujący, Łuk refleksyjny, Kostur burzy, Pawęż, Kosa, Przetrwanie, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „PAROWANIE!” ×177; „OKRZYK!” ×173; „GARDA PRZEŁAMANA!” ×159; „Postawa przełamana – RIPOSTA!” ×146; „SZAŁ!” ×118; „RIPOSTA!” ×103; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×100; „Portale gasną – dobij pozostałe demony” ×95.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×407, uśpiony wróg blokuje koniec piętra ×31, zablokowany ruch ×16, garda nie zatrzymała ciosu ×8.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 31 podejściach)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; -0,1) (w 2 podejściach)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,0)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (6,9; 1,1)
  - Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 120°)
  - Szkieletowy łucznik: „Salwa” przeszedł przez gardę (kąt 169°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 155°)
  - piętro 5 „V. Brama Strażnika”: brak postępu przy (-8,2; -6,9)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,8; 0,1)
  - piętro 5 „V. Brama Strażnika”: brak postępu przy (-9,1; -8,7)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-6,8; -1,1)
  - Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 153°)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×271, uśpiony wróg blokuje koniec piętra ×25, zablokowany ruch ×9, garda nie zatrzymała ciosu ×5, parowanie w oknie nie zadziałało ×4.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 25 podejściach)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 0,1) (w 2 podejściach)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 0,0) (w 2 podejściach)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 174°)
  - Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 138°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 173°)
  - Ognisty chochlik: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 179°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 91°)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-6,1; -6,9)
  - Gargulec: „Gargulec” trafił w aktywnym oknie parowania (kąt do źródła 134°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 158°)
  - Ożywiony nietoperz: „Ukąszenie” trafił w aktywnym oknie parowania (kąt do źródła 85°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 50 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (16 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 8× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 120°)
- Parowanie zwykłych ciosów: 72% ciosów mimo to rani (n=93) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 22% ciosów (n=69).
- Unik przed ciężkim: działa – rani tylko 34% ciosów (n=44).
- Blok zwykłych ciosów: 77% ciosów mimo to rani (n=208) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 96% ciosów mimo to rani (n=27) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 40% pocisków, przed którymi był unik (n=461).
- Blok pocisku: 86% ciosów mimo to rani (n=486) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 65% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1886) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=164).
- Telegraf „nieparowalny” daje czas: tylko 10% decyzji zapada mniej niż 0,1 s przed trafieniem (n=421).
- Telegraf „nieblokowalny” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=147).
- Telegraf „obszarowy” daje czas: tylko 14% decyzji zapada mniej niż 0,1 s przed trafieniem (n=159).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Brak broni/tarczy zdolnej do parowania” ×1 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 74 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 34/50 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Czuję progres: druga połowa podejść średnio piętro 6,6 wobec 4,2 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie siedzi: 50% udanych na zwykłych ciosach, 2,1 ripost na podejście. To najlepsza część walki.
- Gąbki na obrażenia: Władca Bramy (31,4 trafień, 41,5 s), Kasztelan Potępionych (11,8 trafień, 11,9 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Wodny chochlik (1,0), Kultysta (1,1), Ognisty chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 41 s i 31 trafień do zabicia.
- Mój nemezis: Kultysta – 6 z 16 śmierci (najczęściej „Kula ognia”).
- 15/16 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 6% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 80% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Egzekucja”: 221 obrażeń na użycie.
- Skill startowy to 46% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 10%. Różnych skilli na podejście: średnio 4,2.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (30×, śr. piętro 6,5). Inne: Miecz i tarcza + Rozpłatanie 13× (3,9), Dwuręczny + Cięcie wichru 4× (4,5), Dwuręczny + Rozpłatanie 3× (2,0).
- Unik to moje główne narzędzie: 31,5 na podejście, w tym 8,5 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 75% przypadków.
- Gra nie wykonała uniku od razu średnio 8,6× na podejście – najczęściej: w trakcie poprzedniego uniku 4,3, drgnięcie (trafiony) 1,9, brak wytrzymałości 1,0. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (16% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.

