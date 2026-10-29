# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 07:17. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (50 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (50 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 50 | 45 (90%) | 5,7 | 6,0 | 6,0 min | 113,1 | 18245 / 1209 | 3,4 | 20% |
| Gracz H&S / souls-like | 50 | 22 (44%) | 3,8 | 2,0 | 4,3 min | 83,3 | 12292 / 875 | 2,3 | 13% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: ★ ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 3 ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ 5 ★
- Gracz H&S / souls-like: 2 ★ 2 ★ ★ ★ 2 2 ★ 2 2 2 2 2 2 2 ★ ★ ★ ★ 2 2 ★ 2 ★ 2 2 ★ ★ ★ 2 2 ★ 2 ★ ★ 2 2 ★ 2 2 2 3 ★ 2 ★ ★ 2 ★ 2

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×3, 3. III. Władca Bramy ×1, 5. V. Brama Strażnika ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×2; Władca Bramy – „Cięcie w szale” ×1; Kamienny chochlik – „Pazury” ×1; Tkaczka – „Tkaczka” ×1. Śmierć ciosem ≥40% maks. HP: 0/5. Zginął mając flaszki: 3/5.
  Ostatnie 3 s przed śmiercią: średnio 26% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/5). Picie: 192 prób, 171 wypitych (11% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×27, 3. III. Władca Bramy ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×14; Czerw – „Ugryzienie” ×4; Ognisty chochlik – „Pazury” ×3; Chochlik wichru – „Pazury” ×2; Kultysta – „Wachlarz ognia” ×2; Wodny chochlik – „Pazury” ×1. Śmierć ciosem ≥40% maks. HP: 0/28. Zginął mając flaszki: 28/28.
  Ostatnie 3 s przed śmiercią: średnio 33% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/28). Picie: 128 prób, 116 wypitych (9% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 50 | 50 | 68 s | 32 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 50 | 47 | 72 s | 220 | 0,7 | 47,9 |
| Tester QA | 3. III. Władca Bramy | 47 | 46 | 44 s | 414 | 1,6 | 8,2 |
| Tester QA | 4. IV. Zbrojownia | 46 | 46 | 81 s | 188 | 0,3 | 14,9 |
| Tester QA | 5. V. Brama Strażnika | 46 | 45 | 90 s | 352 | 1,0 | 14,0 |
| Tester QA | 6. VI. Szczyt Wieży | 45 | 45 | 21 s | 78 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 50 | 50 | 70 s | 50 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 50 | 23 | 81 s | 383 | 1,0 | 35,9 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 23 | 22 | 74 s | 425 | 1,8 | 8,2 |
| Gracz H&S / souls-like | 4. IV. Zbrojownia | 22 | 22 | 78 s | 225 | 0,3 | 15,6 |
| Gracz H&S / souls-like | 5. V. Brama Strażnika | 22 | 22 | 92 s | 304 | 0,9 | 14,4 |
| Gracz H&S / souls-like | 6. VI. Szczyt Wieży | 22 | 22 | 19 s | 33 | 0,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Władca Bramy | 68 | 49,5 s | 33,9 | 1178 | 27848 | 410 | 2 |
| Kultysta | 1078 | 0,4 s | 1,2 | 1358 | 21120 | 19 | 18 |
| Szkieletowy łucznik | 473 | 2,4 s | 2,6 | 1152 | 13076 | 28 | 0 |
| Kasztelan Potępionych | 67 | 15,5 s | 16,3 | 216 | 4247 | 63 | 0 |
| Czerw | 1838 | 0,5 s | 1,3 | 892 | 3518 | 2 | 4 |
| Harpia | 68 | 0,6 s | 2,0 | 282 | 3220 | 47 | 0 |
| Ognisty chochlik | 988 | 0,1 s | 1,2 | 313 | 2895 | 3 | 4 |
| Upiór | 68 | 6,3 s | 3,5 | 371 | 2650 | 39 | 0 |
| Kamienny chochlik | 1036 | 0,1 s | 1,1 | 297 | 2496 | 2 | 1 |
| Chochlik wichru | 1036 | 0,7 s | 1,5 | 315 | 2323 | 2 | 2 |
| Tkaczka | 67 | 3,2 s | 2,7 | 198 | 2163 | 32 | 1 |
| Kolos | 68 | 2,1 s | 3,0 | 70 | 2079 | 31 | 0 |
| Lodowy strażnik | 79 | 10,5 s | 9,4 | 138 | 2071 | 26 | 0 |
| Strażnik Bramy | 124 | 8,6 s | 8,9 | 162 | 2058 | 17 | 0 |
| Wodny chochlik | 1031 | 0,2 s | 1,2 | 288 | 1916 | 2 | 1 |
| Ożywiony nietoperz | 717 | 0,4 s | 1,3 | 362 | 1740 | 2 | 0 |
| Heretyk | 43 | 2,7 s | 2,6 | 86 | 1629 | 38 | 0 |
| Pajączek | 150 | 1,1 s | 1,3 | 197 | 1358 | 9 | 0 |
| Nekromanta | 68 | 2,9 s | 2,4 | 341 | 1343 | 20 | 0 |
| Płonący ghul | 56 | 3,1 s | 3,4 | 78 | 854 | 15 | 0 |
| Kultysta topieli | 68 | 0,1 s | 1,1 | 56 | 839 | 12 | 0 |
| Taranowiec | 68 | 1,9 s | 2,2 | 48 | 644 | 9 | 0 |
| Kultysta burzy | 25 | 2,7 s | 3,2 | 29 | 629 | 25 | 0 |
| Ghul Wieży | 80 | 1,9 s | 2,9 | 68 | 576 | 7 | 0 |
| Gargulec | 67 | 2,8 s | 4,1 | 34 | 514 | 8 | 0 |
| Legionista | 68 | 3,6 s | 5,2 | 27 | 318 | 5 | 0 |
| Ogar otchłani | 68 | 0,4 s | 1,8 | 29 | 79 | 1 | 0 |
| Ognisty chochlik (elita) | 136 | 4,9 s | 4,8 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 30 | 9,0 s | 5,1 | ↑ | ↑ | ↑ |  |
| Worek jaj | 70 | 0,5 s | 1,2 | 0 | 0 | 0 | 0 |
| Czerw (elita) | 15 | 3,1 s | 2,9 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1720 | 75% | 3% | 3% | 18% | 2% |
| ciężki | 130 | 52% | 5% | 4% | 37% | 2% |
| nieparowalny | 2117 | 77% | 8% | 0% | 9% | 7% |
| nieblokowalny | 182 | 66% | 0% | 2% | 32% | 0% |
| obszarowy | 355 | 74% | 19% | 0% | 0% | 7% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2096 | 52% | 2% | 1% | 44% | 2% |
| ciężki | 75 | 19% | 0% | 3% | 77% | 1% |
| nieparowalny | 1653 | 66% | 3% | 0% | 16% | 15% |
| nieblokowalny | 87 | 59% | 0% | 0% | 41% | 0% |
| obszarowy | 170 | 72% | 24% | 0% | 0% | 5% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1919 | 21% | 39% | 5% | 11% | 23% |
| Tester QA (0,30 s) | ciężki | 201 | 0% | 0% | 0% | 39% | 61% |
| Tester QA (0,30 s) | nieparowalny | 444 | 5% | 5% | 7% | 9% | 75% |
| Tester QA (0,30 s) | nieblokowalny | 186 | 2% | 0% | 3% | 12% | 83% |
| Tester QA (0,30 s) | obszarowy | 194 | 4% | 3% | 5% | 34% | 55% |
| Tester QA (0,30 s) | pocisk | 1447 | 2% | 24% | 15% | 22% | 36% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2733 | 1% | 26% | 54% | 8% | 11% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 106 | 0% | 0% | 0% | 2% | 98% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 291 | 4% | 0% | 7% | 12% | 77% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 106 | 1% | 1% | 0% | 8% | 91% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 100 | 8% | 5% | 2% | 23% | 62% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2334 | 1% | 15% | 10% | 18% | 56% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Pajączek: Pajączek | 0,31 s | 300 |
| Ognisty chochlik: Pazury | 0,33 s | 556 |
| Czerw: Ugryzienie | 0,33 s | 1344 |
| Wodny chochlik: Pazury | 0,33 s | 541 |
| Chochlik wichru: Pazury | 0,33 s | 624 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 597 |
| Ghul Wieży: Druga seria | 0,33 s | 9 |
| Płonący ghul: Druga seria | 0,33 s | 15 |
| Kamienny chochlik: Pazury | 0,33 s | 554 |
| Władca Bramy: Cięcie w szale | 0,41 s | 83 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 72% (n=133) | 25% (n=4) | 63% (n=236) | 100% (n=3) | 7% (n=60) | 81% (n=355) |
| blok (eksperyment) | 86% (n=80) | 64% (n=11) | 81% (n=36) | 100% (n=9) | 38% (n=8) | 71% (n=72) |
| brak reakcji (eksperyment) | 94% (n=68) | 100% (n=5) | 94% (n=63) | 100% (n=8) | 100% (n=1) | 95% (n=97) |
| niezauważony | 90% (n=284) | – | 98% (n=665) | 99% (n=72) | 100% (n=46) | – |
| parowanie | 68% (n=62) | 70% (n=10) | – | 67% (n=3) | – | – |
| parowanie (eksperyment) | 74% (n=46) | 60% (n=5) | 93% (n=30) | 71% (n=7) | 100% (n=3) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 98% (n=260) |
| unik | 47% (n=384) | 28% (n=58) | 46% (n=169) | 25% (n=72) | – | 80% (n=164) |
| unik (eksperyment) | 58% (n=110) | 40% (n=10) | 58% (n=43) | 20% (n=5) | 100% (n=5) | 86% (n=51) |
| wyjście z zasięgu | 95% (n=419) | 100% (n=23) | 98% (n=46) | – | 95% (n=39) | – |
| wyjście z zasięgu (eksperyment) | 95% (n=76) | 100% (n=4) | 100% (n=28) | 100% (n=3) | 100% (n=9) | 91% (n=44) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 22,8 | reakcja na cios wręcz 21,7, unik przed pociskiem 9,4, wyjście z okrążenia 1,0 | drgnięcie (trafiony) 3,0, brak wytrzymałości 2,0, w trakcie uniku 1,5, picie flaszki 0,8, inna 0,5, unieruchomienie 0,1, mocny atak 0,1, umiejętność 0,1, parowanie 0,1, szybki atak 0,0, blok 0,0, riposta 0,0 | 64% (n=1085) / 63% (n=469) |
| Gracz H&S / souls-like | 42,9 | reakcja na cios wręcz 34,6, unik przed pociskiem 18,2, doskok do odsłoniętego celu 4,2, wyjście z okrążenia 0,4 | w trakcie uniku 6,2, drgnięcie (trafiony) 2,1, brak wytrzymałości 1,3, picie flaszki 1,0, inna 0,3, mocny atak 0,2, szybki atak 0,2, unieruchomienie 0,2, parowanie 0,1, riposta 0,0, blok 0,0, umiejętność 0,0 | 82% (n=1730) / 72% (n=911) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Cięcie wichru | 13 | 13 | 6,0 | 116,8 |
| Tester QA | Dwuręczny | Cięcie wichru | 12 | 12 | 6,0 | 116,0 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 13 | 11 | 5,7 | 116,2 |
| Tester QA | Dwuręczny | Rozpłatanie | 12 | 9 | 5,0 | 102,7 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 41 | 20 | 4,0 | 87,1 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 3 | 1 | 3,3 | 70,7 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 5 | 1 | 2,8 | 63,2 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 1 | 0 | 2,0 | 64,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 162669 | 18% |  |  |
| Rozpłatanie | 122917 | 13% | 735 | 167 |
| Cięcie wichru | 120731 | 13% | 793 | 152 |
| Tąpnięcie | 101667 | 11% | 788 | 129 |
| Cięcie | 93798 | 10% |  |  |
| Egzekucja | 69053 | 8% | 21 | 3288 |
| Potężne cięcie | 40331 | 4% |  |  |
| Okrzyk bojowy | 28570 | 3% | 1155 | 25 |
| Fala przełamania | 20982 | 2% | 86 | 244 |
| Trzęsienie | 16988 | 2% | 128 | 133 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Kamienna osłona ×7, Okrzyk wojenny ×55, Krwawy pakt ×15, Kojące światło ×6.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 210168 | 34% | 1405 | 150 |
| Egzekucja | 117655 | 19% | 345 | 341 |
| Rozpłatanie | 109013 | 18% | 684 | 159 |
| Zamach | 91908 | 15% |  |  |
| Okrzyk bojowy | 14501 | 2% | 238 | 61 |
| Tąpnięcie | 7067 | 1% | 45 | 157 |
| Nova ognia | 6932 | 1% | 21 | 330 |
| Cięcie krzyżowe | 6623 | 1% | 32 | 207 |
| Rąbnięcie | 5869 | 1% |  |  |
| Szarża barkiem | 3675 | 1% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Krwawy pakt ×7.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 6,1, mediana 6,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 29% | 29,3 | 161 | 0,9 |
| odblokowanie | 16% | 15,8 | 129 | 1,2 |
| nagroda | 1% | 0,8 | 151 | 0,9 |
| sklep | 31% | 31,3 | 113 | 1,1 |
| ? | 23% | 23,1 | 25 | 0,4 |

Oferty sklepu dusz: widziane 234, kupione 169, odrzucone 0, za mało dusz 65.
Zamiany w slotach: 3,6 na podejście; najczęstsze: Tąpnięcie → Cięcie wichru ×12, Rozpłatanie → Tąpnięcie ×11, Cięcie wichru → Tąpnięcie ×6, Tąpnięcie → Trzęsienie ×6, Tąpnięcie → Rozpłatanie ×4, Cięcie wichru → Młynek ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | ? | 50 | 23,1 | 25 | 0,4 |
| Cięcie wichru | startowy | 27 | 29,4 | 152 | 0,9 |
| Tąpnięcie | odblokowanie | 50 | 15,8 | 129 | 1,2 |
| Rozpłatanie | startowy | 29 | 25,3 | 167 | 1,0 |
| Trzęsienie | sklep | 10 | 12,8 | 133 | 1,1 |
| Cięcie krzyżowe | sklep | 6 | 17,3 | 109 | 1,5 |
| Kontra | sklep | 6 | 15,7 | 20 | 1,1 |
| Fala przełamania | sklep | 5 | 17,2 | 244 | 1,4 |
| Piekielna szarża | sklep | 6 | 13,5 | 153 | 1,1 |
| Młynek | sklep | 6 | 12,7 | 64 | 0,9 |
| Rozdarcie ran | sklep | 4 | 14,8 | 34 | 0,4 |
| Seria cięć | sklep | 4 | 14,5 | 91 | 1,2 |
| Wypad | sklep | 2 | 29,0 | 20 | 0,8 |
| Okrzyk wojenny | sklep | 6 | 9,2 | – | 1,2 |
| Cięcie z wyskoku | sklep | 4 | 13,3 | 76 | 0,6 |
| Nova powietrza | sklep | 3 | 16,3 | 103 | 1,4 |
| Ciężki rzut | sklep | 3 | 16,0 | 146 | 1,3 |
| Łańcuch potępionych | sklep | 4 | 11,3 | 127 | 1,5 |
| Upadek kata | sklep | 2 | 21,5 | 167 | 0,7 |
| Szarża | sklep | 2 | 19,0 | 78 | 1,7 |
| Bomba siarkowa | sklep | 4 | 8,3 | 103 | 1,0 |
| Krok cienia | sklep | 2 | 16,0 | 73 | 1,3 |
| Sidła | sklep | 3 | 10,7 | 84 | 1,3 |
| Nova ziemi | sklep | 3 | 10,0 | 107 | 1,3 |
| Uderzenie gromu | sklep | 2 | 12,5 | 169 | 1,0 |
| Strzał przeszywający | sklep | 4 | 6,0 | 151 | 1,4 |
| Rzut nożami | sklep | 7 | 3,3 | 86 | 1,0 |
| Krwawe cięcie | sklep | 2 | 11,5 | 118 | 1,1 |
| Egzekucja | sklep | 2 | 10,5 | 470 | 1,4 |
| Przyzwanie chochlików | sklep | 3 | 6,3 | 247 | 1,2 |
| Piekielna kula | sklep | 4 | 4,5 | 412 | 1,3 |
| Nova wody | sklep | 2 | 8,5 | 59 | 1,2 |
| Garść cierni | sklep | 5 | 3,2 | 239 | 0,7 |
| Nova ognia | sklep | 1 | 16,0 | 87 | 1,0 |
| Lodowy odłamek | sklep | 4 | 4,0 | 71 | 0,8 |
| Krwawy pakt | sklep | 2 | 7,5 | – | 1,3 |
| Krąg żaru | sklep | 3 | 4,3 | 59 | 1,5 |
| Oko cyklonu | sklep | 4 | 3,3 | 179 | 1,1 |
| Żużel | sklep | 2 | 5,5 | 142 | 1,0 |
| Krok wichru | sklep | 2 | 5,5 | 56 | 1,1 |
| Salwa | sklep | 2 | 5,0 | 132 | 1,0 |
| Strzała siarkowa | sklep | 1 | 9,0 | 133 | 1,2 |
| Sfora | sklep | 2 | 4,0 | 271 | 1,0 |
| Kamienna osłona | sklep | 4 | 1,8 | – | 0,9 |
| Rzut toporem | sklep | 2 | 3,0 | 182 | 1,0 |
| Kojące światło | nagroda | 1 | 6,0 | – | 1,0 |
| Łza lodowca | sklep | 2 | 2,0 | 148 | 0,5 |
| Kościany golem | sklep | 1 | 3,0 | 342 | 1,0 |
| Wachlarz ostrzy | sklep | 1 | 3,0 | 91 | 0,0 |
| Deszcz siarki | sklep | 1 | 2,0 | 11 | 1,0 |
| Łańcuchy potępionych | nagroda | 1 | 2,0 | 39 | 1,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,2, mediana 5,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 49% | 29,7 | 151 | 1,0 |
| odblokowanie | 34% | 20,5 | 219 | 1,3 |
| klasa | 8% | 4,8 | 61 | 0,9 |
| nagroda | 1% | 0,7 | 179 | 1,1 |
| sklep | 7% | 4,5 | 172 | 1,2 |

Oferty sklepu dusz: widziane 139, kupione 56, odrzucone 59, za mało dusz 24.
Zamiany w slotach: 2,5 na podejście; najczęstsze: Szarża → Egzekucja ×45, Rozpłatanie → Nova ognia ×6, Rozpłatanie → Cięcie krzyżowe ×4, Szarża → Trzęsienie ×2, Rozpłatanie → Salwa ×2, Cięcie wichru → Rzut toporem ×2.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 46 | 30,5 | 150 | 1,0 |
| Rozpłatanie | odblokowanie | 48 | 14,3 | 159 | 1,1 |
| Egzekucja | odblokowanie | 48 | 7,2 | 341 | 1,6 |
| Okrzyk bojowy | klasa | 50 | 4,8 | 61 | 0,9 |
| Tąpnięcie | odblokowanie | 2 | 22,5 | 157 | 2,1 |
| Cięcie krzyżowe | sklep | 4 | 8,0 | 207 | 1,3 |
| Nova ognia | sklep | 5 | 4,2 | 330 | 1,3 |
| Szarża | sklep | 1 | 19,0 | 48 | 1,6 |
| Trzęsienie | odblokowanie | 3 | 6,0 | 194 | 0,2 |
| Nova ziemi | sklep | 2 | 6,0 | 186 | 1,4 |
| Strzała rozszczepu | sklep | 2 | 5,5 | 106 | 1,0 |
| Nova wody | sklep | 2 | 5,0 | 259 | 1,4 |
| Strzała siarkowa | sklep | 2 | 4,5 | 185 | 1,0 |
| Krąg żaru | sklep | 2 | 4,0 | 150 | 1,5 |
| Krwawe cięcie | nagroda | 3 | 2,7 | 131 | 0,3 |
| Przyzwanie chochlików | sklep | 1 | 8,0 | 181 | 0,5 |
| Uderzenie gromu | odblokowanie | 1 | 7,0 | 126 | 0,0 |
| Piekielna kula | sklep | 4 | 1,8 | 321 | 1,3 |
| Deszcz strzał | sklep | 1 | 7,0 | 200 | 1,4 |
| Ciężki rzut | sklep | 1 | 7,0 | 266 | 1,0 |
| Bomba siarkowa | nagroda | 2 | 3,5 | 300 | 1,0 |
| Krwawy pakt | nagroda | 2 | 3,5 | – | 1,4 |
| Wachlarz ostrzy | sklep | 2 | 3,5 | 87 | 2,0 |
| Młynek | odblokowanie | 1 | 6,0 | 119 | 0,3 |
| Salwa | nagroda | 2 | 3,0 | 135 | 1,0 |
| Łza lodowca | sklep | 1 | 6,0 | 132 | 1,0 |
| Seria cięć | sklep | 1 | 6,0 | 81 | 1,0 |
| Rzut toporem | nagroda | 3 | 2,0 | 132 | 1,0 |
| Garść cierni | sklep | 2 | 3,0 | 207 | 1,0 |
| Fala przypływu | nagroda | 2 | 3,0 | 160 | 1,0 |
| Rozdarcie ran | sklep | 1 | 6,0 | 32 | 1,7 |
| Sidła | sklep | 2 | 2,5 | 168 | 0,6 |
| Kontra | sklep | 1 | 5,0 | 27 | 0,0 |
| Lodowy odłamek | sklep | 2 | 2,5 | 119 | 1,0 |
| Kościany golem | sklep | 2 | 2,5 | 198 | 0,8 |
| Rzut nożami | sklep | 2 | 2,5 | 43 | 1,2 |
| Oko cyklonu | sklep | 1 | 4,0 | 68 | 1,0 |
| Łańcuch potępionych | sklep | 1 | 3,0 | 329 | 1,0 |
| Kamienne kolce | sklep | 1 | 3,0 | 254 | 1,0 |
| Sfora | sklep | 1 | 3,0 | 268 | 1,0 |
| Żużel | sklep | 1 | 2,0 | 186 | 1,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 100,3, szybki atak 59,6, drgnięcie (trafiony) 35,1, unik 22,8, mocny atak 21,4, blok 13,3, przełamana garda 3,9, flaszka (próba) 3,8, parowanie 1,9, riposta 0,5.
Czas w bloku 7 s, w biegu 24 s, bez wytrzymałości (<10%) 95 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Oddech wojownika ×10, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×9, wzmocnienie: Czujność ×8, wzmocnienie: Żelazna skóra ×7, wzmocnienie: Ostrze gniewu ×6, wzmocnienie: Jasność umysłu ×6, ulepszenie: Ulepszenie: Tąpnięcie → +2 ×6, wzmocnienie: Obrońca ×6.
Zakupy: ulepszenie broni ×91, ulepszenie: Tąpnięcie ×76, ulepszenie: Rozpłatanie ×42, ulepszenie: Okrzyk bojowy ×42, ulepszenie: Cięcie wichru ×35, dodatkowa flaszka ×33, ulepszenie: Trzęsienie ×10, oferta: Trzęsienie ×9.
Punkty cech: Strength 95, Toughness 47, Dexterity 46, Intelligence 46.
Komunikaty gry: „OKRZYK!” ×1104; „SZAŁ!” ×420; „GARDA PRZEŁAMANA!” ×199; „Broń jeszcze w locie” ×104; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×100; „Portale gasną – dobij pozostałe demony” ×99; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×91; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×47.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 60,2, unik 42,9, szybki atak 31,7, drgnięcie (trafiony) 22,9, mocny atak 19,1, blok 14,4, przełamana garda 5,8, flaszka (próba) 2,6, parowanie 0,3, riposta 0,2.
Czas w bloku 7 s, w biegu 10 s, bez wytrzymałości (<10%) 78 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +1 ×11, ulepszenie: Ulepszenie: Egzekucja → +1 ×11, ulepszenie: Ulepszenie: Egzekucja → +3 ×11, ulepszenie: Ulepszenie: Egzekucja → +2 ×8, wzmocnienie: Oddech wojownika ×6, ulepszenie: Ulepszenie: Cięcie wichru → +2 ×5, wzmocnienie: Żelazna skóra ×5, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×5.
Zakupy: ulepszenie: Egzekucja ×59, ulepszenie: Okrzyk bojowy ×49, ulepszenie: Cięcie wichru ×47, ulepszenie broni ×44, ulepszenie: Rozpłatanie ×41, dodatkowa flaszka ×16, ulepszenie: Nova ognia ×9, ulepszenie: Cięcie krzyżowe ×6.
Punkty cech: Toughness 72, Strength 67.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Kontra, Młynek, Egzekucja, Trzęsienie, Uderzenie gromu, Puklerz, Przetrwanie, Pawęż, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „GARDA PRZEŁAMANA!” ×292; „OKRZYK!” ×219; „SZAŁ!” ×166; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×100; „Portale gasną – dobij pozostałe demony” ×87; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×44; „POLEGŁEŚ” ×28; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×23.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×459, uśpiony wróg blokuje koniec piętra ×32, zablokowany ruch ×16, garda nie zatrzymała ciosu ×5, parowanie w oknie nie zadziałało ×2.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 32 podejściach)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,8; -0,1) (w 2 podejściach)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; -0,4)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-6,1; 1,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-5,6; -8,5)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 8,0)
  - Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 173°)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-2,1; -6,5)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,2; 1,1)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (5,6; -8,3)
  - Tkaczka: „Tkaczka: Cocoon” trafił w aktywnym oknie parowania (kąt do źródła 90°)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-6,5; 6,9)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×247, uśpiony wróg blokuje koniec piętra ×15, zablokowany ruch ×5, garda nie zatrzymała ciosu ×3.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 15 podejściach)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 0,0) (w 2 podejściach)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,8; -7,4)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 122°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 128°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 7,5)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 0,1)
  - Kultysta: „Wachlarz ognia” przeszedł przez gardę (kąt 132°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 50 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (16 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 2× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Tkaczka: „Tkaczka: Cocoon” trafił w aktywnym oknie parowania (kąt do źródła 90°)
- **Do sprawdzenia (P3):** 5× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 173°)
- Parowanie zwykłych ciosów: 70% ciosów mimo to rani (n=108) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 25% ciosów (n=77).
- Unik przed ciężkim: działa – rani tylko 29% ciosów (n=68).
- Blok zwykłych ciosów: 77% ciosów mimo to rani (n=213) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 96% ciosów mimo to rani (n=48) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 37% pocisków, przed którymi był unik (n=469).
- Blok pocisku: 80% ciosów mimo to rani (n=427) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 60% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1919) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=201).
- Telegraf „nieparowalny” daje czas: tylko 9% decyzji zapada mniej niż 0,1 s przed trafieniem (n=444).
- Telegraf „nieblokowalny” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=186).
- Telegraf „obszarowy” daje czas: tylko 7% decyzji zapada mniej niż 0,1 s przed trafieniem (n=194).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 68 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 22/50. Wyzwanie umiarkowane; po opanowaniu parowania wieża przestaje straszyć.
- Progres między podejściami słabo odczuwalny: 4,2 → 4,2 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 39% udanych (n=28). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Władca Bramy (39,5 trafień, 69,9 s), Kasztelan Potępionych (12,6 trafień, 14,7 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta topieli (1,0), Kamienny chochlik (1,1), Ognisty chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 70 s i 39 trafień do zabicia.
- Mój nemezis: Kultysta – 16 z 28 śmierci (najczęściej „Kula ognia”).
- 28/28 razy zginąłem z flaszkami w kieszeni. 3 z tych śmierci to utrata ≥50% HP w 3 s, a 9% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 83% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Egzekucja”: 341 obrażeń na użycie.
- Skill startowy to 49% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 9%. Różnych skilli na podejście: średnio 5,2.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (41×, śr. piętro 4,5). Inne: Dwuręczny + Cięcie wichru 5× (3,0), Dwuręczny + Rozpłatanie 3× (3,7), Miecz i tarcza + Rozpłatanie 1× (2,0).
- Unik to moje główne narzędzie: 42,9 na podejście, w tym 4,5 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 72% przypadków.
- Gra nie wykonała uniku od razu średnio 11,5× na podejście – najczęściej: w trakcie poprzedniego uniku 6,2, drgnięcie (trafiony) 2,1, brak wytrzymałości 1,3. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (30% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.

