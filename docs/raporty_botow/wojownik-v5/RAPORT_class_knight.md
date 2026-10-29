# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 07:10. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (50 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (50 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 50 | 45 (90%) | 5,7 | 6,0 | 6,0 min | 113,1 | 18245 / 1209 | 3,4 | 20% |
| Gracz H&S / souls-like | 50 | 20 (40%) | 3,6 | 2,0 | 4,1 min | 82,3 | 11829 / 849 | 2,3 | 11% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: ★ ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 3 ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ 5 ★
- Gracz H&S / souls-like: ★ 2 2 ★ 2 2 2 2 2 ★ 2 2 ★ 2 2 ★ ★ ★ ★ 2 ★ 2 2 ★ ★ 2 ★ 2 2 ★ ★ 2 2 2 2 2 2 2 ★ 2 ★ ★ 3 2 ★ ★ ★ 2 2 2

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×3, 3. III. Władca Bramy ×1, 5. V. Brama Strażnika ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×2; Władca Bramy – „Cięcie w szale” ×1; Kamienny chochlik – „Pazury” ×1; Tkaczka – „Tkaczka” ×1. Śmierć ciosem ≥40% maks. HP: 0/5. Zginął mając flaszki: 3/5.
  Ostatnie 3 s przed śmiercią: średnio 26% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/5). Picie: 192 prób, 171 wypitych (11% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×29, 3. III. Władca Bramy ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×18; Czerw – „Ugryzienie” ×6; Ognisty chochlik – „Pazury” ×2; Chochlik wichru – „Pazury” ×1; Kamienny chochlik – „Pazury” ×1; Wodny chochlik – „Pazury” ×1. Śmierć ciosem ≥40% maks. HP: 0/30. Zginął mając flaszki: 28/30.
  Ostatnie 3 s przed śmiercią: średnio 28% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/30). Picie: 128 prób, 116 wypitych (9% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 50 | 50 | 68 s | 32 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 50 | 47 | 72 s | 220 | 0,7 | 47,9 |
| Tester QA | 3. III. Władca Bramy | 47 | 46 | 44 s | 414 | 1,6 | 8,2 |
| Tester QA | 4. IV. Zbrojownia | 46 | 46 | 81 s | 188 | 0,3 | 14,9 |
| Tester QA | 5. V. Brama Strażnika | 46 | 45 | 90 s | 352 | 1,0 | 14,0 |
| Tester QA | 6. VI. Szczyt Wieży | 45 | 45 | 21 s | 78 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 50 | 50 | 71 s | 51 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 50 | 21 | 82 s | 414 | 1,2 | 35,6 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 21 | 20 | 66 s | 382 | 1,4 | 9,1 |
| Gracz H&S / souls-like | 4. IV. Zbrojownia | 20 | 20 | 85 s | 231 | 0,4 | 16,7 |
| Gracz H&S / souls-like | 5. V. Brama Strażnika | 20 | 20 | 89 s | 311 | 1,0 | 14,4 |
| Gracz H&S / souls-like | 6. VI. Szczyt Wieży | 20 | 20 | 19 s | 17 | 0,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Władca Bramy | 66 | 46,3 s | 33,6 | 1056 | 26279 | 398 | 2 |
| Kultysta | 1121 | 0,5 s | 1,2 | 1461 | 22136 | 19 | 20 |
| Szkieletowy łucznik | 459 | 2,3 s | 2,6 | 1121 | 12609 | 27 | 0 |
| Czerw | 1811 | 0,6 s | 1,3 | 1046 | 3927 | 2 | 6 |
| Kasztelan Potępionych | 65 | 15,3 s | 16,6 | 208 | 3861 | 59 | 0 |
| Ognisty chochlik | 961 | 0,2 s | 1,2 | 366 | 3307 | 3 | 2 |
| Harpia | 66 | 0,6 s | 2,0 | 267 | 2922 | 44 | 0 |
| Upiór | 66 | 6,3 s | 3,7 | 386 | 2677 | 41 | 0 |
| Kamienny chochlik | 1039 | 0,2 s | 1,2 | 309 | 2505 | 2 | 2 |
| Chochlik wichru | 1046 | 0,7 s | 1,5 | 307 | 2311 | 2 | 1 |
| Strażnik Bramy | 119 | 9,1 s | 8,9 | 175 | 2196 | 18 | 0 |
| Lodowy strażnik | 78 | 10,2 s | 9,5 | 132 | 2125 | 27 | 0 |
| Tkaczka | 65 | 3,0 s | 2,7 | 185 | 2006 | 31 | 1 |
| Wodny chochlik | 1016 | 0,2 s | 1,2 | 270 | 1719 | 2 | 1 |
| Heretyk | 45 | 2,4 s | 2,5 | 89 | 1662 | 37 | 0 |
| Kolos | 66 | 2,0 s | 3,2 | 54 | 1604 | 24 | 0 |
| Ożywiony nietoperz | 735 | 0,4 s | 1,3 | 336 | 1537 | 2 | 0 |
| Nekromanta | 66 | 3,6 s | 2,6 | 382 | 1428 | 22 | 0 |
| Pajączek | 147 | 1,2 s | 1,3 | 206 | 1398 | 10 | 0 |
| Płonący ghul | 53 | 3,4 s | 3,2 | 82 | 863 | 16 | 0 |
| Kultysta topieli | 66 | 0,1 s | 1,1 | 54 | 811 | 12 | 0 |
| Gargulec | 65 | 2,9 s | 3,9 | 39 | 682 | 10 | 0 |
| Taranowiec | 66 | 2,0 s | 2,4 | 53 | 682 | 10 | 0 |
| Ghul Wieży | 79 | 2,1 s | 2,9 | 76 | 681 | 9 | 0 |
| Kultysta burzy | 21 | 2,4 s | 3,3 | 26 | 544 | 26 | 0 |
| Legionista | 66 | 3,8 s | 5,3 | 29 | 337 | 5 | 0 |
| Ogar otchłani | 66 | 0,4 s | 1,8 | 33 | 90 | 1 | 0 |
| Ognisty chochlik (elita) | 133 | 5,3 s | 4,9 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 29 | 9,9 s | 5,2 | ↑ | ↑ | ↑ |  |
| Worek jaj | 67 | 0,4 s | 1,5 | 0 | 0 | 0 | 0 |
| Czerw (elita) | 18 | 3,5 s | 3,2 | ↑ | ↑ | ↑ |  |

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
| zwykły | 2194 | 51% | 2% | 0% | 44% | 2% |
| ciężki | 64 | 28% | 2% | 3% | 66% | 2% |
| nieparowalny | 1717 | 65% | 3% | 0% | 16% | 15% |
| nieblokowalny | 75 | 48% | 0% | 1% | 51% | 0% |
| obszarowy | 194 | 70% | 17% | 0% | 0% | 13% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1919 | 21% | 39% | 5% | 11% | 23% |
| Tester QA (0,30 s) | ciężki | 201 | 0% | 0% | 0% | 39% | 61% |
| Tester QA (0,30 s) | nieparowalny | 444 | 5% | 5% | 7% | 9% | 75% |
| Tester QA (0,30 s) | nieblokowalny | 186 | 2% | 0% | 3% | 12% | 83% |
| Tester QA (0,30 s) | obszarowy | 194 | 4% | 3% | 5% | 34% | 55% |
| Tester QA (0,30 s) | pocisk | 1447 | 2% | 24% | 15% | 22% | 36% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2922 | 1% | 27% | 56% | 8% | 9% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 93 | 0% | 0% | 0% | 3% | 97% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 257 | 4% | 0% | 5% | 11% | 81% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 94 | 0% | 1% | 0% | 6% | 93% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 90 | 6% | 2% | 6% | 21% | 66% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2700 | 1% | 12% | 8% | 17% | 62% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Pajączek: Pajączek | 0,31 s | 300 |
| Wodny chochlik: Pazury | 0,33 s | 532 |
| Czerw: Ugryzienie | 0,33 s | 1550 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 573 |
| Ghul Wieży: Druga seria | 0,33 s | 13 |
| Płonący ghul: Druga seria | 0,33 s | 18 |
| Ognisty chochlik: Pazury | 0,33 s | 600 |
| Kamienny chochlik: Pazury | 0,33 s | 590 |
| Chochlik wichru: Pazury | 0,33 s | 611 |
| Władca Bramy: Cięcie w szale | 0,41 s | 68 |

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
| Gracz H&S / souls-like | 43,5 | reakcja na cios wręcz 34,6, unik przed pociskiem 18,9, doskok do odsłoniętego celu 3,9, wyjście z okrążenia 0,3 | w trakcie uniku 6,1, drgnięcie (trafiony) 2,0, brak wytrzymałości 1,2, picie flaszki 0,9, inna 0,3, mocny atak 0,2, parowanie 0,0, umiejętność 0,0, unieruchomienie 0,0, riposta 0,0, szybki atak 0,0 | 82% (n=1731) / 72% (n=945) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Cięcie wichru | 13 | 13 | 6,0 | 116,8 |
| Tester QA | Dwuręczny | Cięcie wichru | 12 | 12 | 6,0 | 116,0 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 13 | 11 | 5,7 | 116,2 |
| Tester QA | Dwuręczny | Rozpłatanie | 12 | 9 | 5,0 | 102,7 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 40 | 17 | 3,7 | 85,0 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 6 | 2 | 3,3 | 74,3 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 3 | 1 | 3,3 | 76,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 2,0 | 41,0 |

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
| Cięcie wichru | 185878 | 31% | 1256 | 148 |
| Rozpłatanie | 133340 | 23% | 811 | 164 |
| Egzekucja | 111159 | 19% | 328 | 339 |
| Zamach | 80804 | 14% |  |  |
| Okrzyk bojowy | 17379 | 3% | 246 | 71 |
| Rąbnięcie | 5309 | 1% |  |  |
| Sfora | 4949 | 1% | 12 | 412 |
| Bomba siarkowa | 4209 | 1% | 18 | 234 |
| Nova ognia | 4165 | 1% | 14 | 298 |
| Szarża barkiem | 3687 | 1% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Kontra ×6, Krwawy pakt ×5.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. Skill klasy (osobny przycisk) liczony jako „odblokowanie”/„zestaw”.

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

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,1, mediana 5,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 49% | 29,0 | 151 | 0,9 |
| odblokowanie | 34% | 20,2 | 216 | 1,3 |
| nagroda | 1% | 0,8 | 173 | 1,2 |
| sklep | 7% | 4,3 | 166 | 1,2 |
| ? | 8% | 4,9 | 71 | 0,7 |

Oferty sklepu dusz: widziane 131, kupione 52, odrzucone 57, za mało dusz 22.
Zamiany w slotach: 2,4 na podejście; najczęstsze: Szarża → Egzekucja ×44, Szarża → Trzęsienie ×3, Cięcie wichru → Oko cyklonu ×2, Rozpłatanie → Cięcie krzyżowe ×2, Rozpłatanie → Kamienne kolce ×2, Rozpłatanie → Nova powietrza ×2.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 43 | 29,2 | 148 | 0,9 |
| Rozpłatanie | startowy | 49 | 16,6 | 164 | 1,2 |
| Egzekucja | odblokowanie | 45 | 7,3 | 336 | 1,5 |
| Okrzyk bojowy | ? | 50 | 4,9 | 71 | 0,7 |
| Tąpnięcie | odblokowanie | 1 | 21,0 | 125 | 1,0 |
| Wypad | sklep | 1 | 18,0 | 9 | 1,0 |
| Bomba siarkowa | sklep | 2 | 9,0 | 234 | 1,8 |
| Trzęsienie | odblokowanie | 4 | 3,5 | 173 | 0,0 |
| Nova ognia | nagroda | 2 | 7,0 | 298 | 1,6 |
| Sfora | sklep | 1 | 12,0 | 412 | 1,8 |
| Cięcie krzyżowe | sklep | 2 | 5,0 | 135 | 1,5 |
| Garść cierni | nagroda | 3 | 3,3 | 208 | 1,0 |
| Oko cyklonu | sklep | 2 | 4,5 | 110 | 1,0 |
| Lodowy odłamek | sklep | 2 | 4,0 | 136 | 1,3 |
| Nova powietrza | sklep | 3 | 2,7 | 123 | 1,3 |
| Piekielna szarża | sklep | 1 | 8,0 | 137 | 1,0 |
| Szarża | odblokowanie | 1 | 7,0 | 25 | 0,0 |
| Krok wichru | sklep | 2 | 3,5 | 17 | 1,0 |
| Seria cięć | sklep | 2 | 3,5 | 101 | 1,0 |
| Piekielna kula | sklep | 3 | 2,3 | 441 | 0,9 |
| Nova wody | sklep | 1 | 7,0 | 212 | 1,4 |
| Kontra | odblokowanie | 1 | 6,0 | – | 1,0 |
| Młynek | odblokowanie | 1 | 6,0 | 172 | 0,3 |
| Uderzenie gromu | odblokowanie | 1 | 6,0 | 133 | 0,0 |
| Łańcuch potępionych | sklep | 1 | 6,0 | 125 | 1,0 |
| Kamienne kolce | sklep | 2 | 3,0 | 97 | 1,0 |
| Łza lodowca | sklep | 2 | 3,0 | 319 | 1,0 |
| Rzut toporem | sklep | 1 | 6,0 | 264 | 1,5 |
| Cięcie z wyskoku | sklep | 1 | 6,0 | 63 | 1,0 |
| Strzała rozszczepu | sklep | 1 | 6,0 | 79 | 0,0 |
| Salwa | nagroda | 1 | 6,0 | 200 | 1,2 |
| Przyzwanie chochlików | sklep | 1 | 6,0 | 292 | 1,0 |
| Krwawy pakt | sklep | 2 | 2,5 | – | 1,0 |
| Wyrok burzy | sklep | 1 | 5,0 | 202 | 1,2 |
| Strzała siarkowa | sklep | 2 | 2,5 | 187 | 0,8 |
| Krwawe cięcie | nagroda | 1 | 5,0 | 73 | 1,0 |
| Ciężki rzut | sklep | 1 | 5,0 | 175 | 1,0 |
| Krąg żaru | sklep | 2 | 2,5 | 100 | 1,2 |
| Sidła | sklep | 2 | 2,5 | 58 | 1,0 |
| Rzut nożami | nagroda | 1 | 5,0 | 99 | 1,6 |
| Rozdarcie ran | sklep | 1 | 5,0 | 59 | 1,0 |
| Strzał przeszywający | nagroda | 1 | 5,0 | 129 | 1,6 |
| Uderzenie tarczą | odblokowanie | 1 | 4,0 | 61 | 1,0 |
| Fala przypływu | nagroda | 2 | 2,0 | 248 | 1,0 |
| Deszcz siarki | sklep | 1 | 4,0 | 67 | 1,3 |
| Deszcz strzał | sklep | 1 | 4,0 | 128 | 1,0 |
| Wachlarz ostrzy | sklep | 1 | 1,0 | 84 | 1,0 |
| Nova ziemi | sklep | 1 | 1,0 | 161 | 1,0 |
| Kościany golem | sklep | 1 | 1,0 | 358 | 1,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 100,3, szybki atak 59,6, drgnięcie (trafiony) 35,1, unik 22,8, mocny atak 21,4, blok 13,3, przełamana garda 3,9, flaszka (próba) 3,8, parowanie 1,9, riposta 0,5.
Czas w bloku 7 s, w biegu 24 s, bez wytrzymałości (<10%) 95 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Oddech wojownika ×10, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×9, wzmocnienie: Czujność ×8, wzmocnienie: Żelazna skóra ×7, wzmocnienie: Ostrze gniewu ×6, wzmocnienie: Jasność umysłu ×6, ulepszenie: Ulepszenie: Tąpnięcie → +2 ×6, wzmocnienie: Obrońca ×6.
Zakupy: ulepszenie broni ×91, ulepszenie: Tąpnięcie ×76, ulepszenie: Rozpłatanie ×42, ulepszenie: Okrzyk bojowy ×42, ulepszenie: Cięcie wichru ×35, dodatkowa flaszka ×33, ulepszenie: Trzęsienie ×10, oferta: Trzęsienie ×9.
Punkty cech: Strength 95, Toughness 47, Dexterity 46, Intelligence 46.
Komunikaty gry: „OKRZYK!” ×1104; „SZAŁ!” ×420; „GARDA PRZEŁAMANA!” ×199; „Broń jeszcze w locie” ×104; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×100; „Portale gasną – dobij pozostałe demony” ×99; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×91; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×47.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 59,2, unik 43,5, szybki atak 27,6, drgnięcie (trafiony) 22,4, mocny atak 18,5, blok 17,4, przełamana garda 6,7, flaszka (próba) 2,6, parowanie 0,2, riposta 0,1.
Czas w bloku 8 s, w biegu 9 s, bez wytrzymałości (<10%) 79 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Egzekucja → +3 ×11, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×10, ulepszenie: Ulepszenie: Egzekucja → +1 ×9, wzmocnienie: Żelazna skóra ×7, ulepszenie: Ulepszenie: Egzekucja → +2 ×6, wzmocnienie: Oddech wojownika ×6, wzmocnienie: Pijawka ×5, ulepszenie: Ulepszenie: Egzekucja → +4 ×5.
Zakupy: ulepszenie: Egzekucja ×58, ulepszenie: Rozpłatanie ×54, ulepszenie: Okrzyk bojowy ×46, ulepszenie: Cięcie wichru ×45, ulepszenie broni ×40, dodatkowa flaszka ×17, ulepszenie: Lodowy odłamek ×3, ulepszenie: Bomba siarkowa ×3.
Punkty cech: Toughness 70, Strength 61.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Kontra, Młynek, Egzekucja, Trzęsienie, Uderzenie gromu, Puklerz, Przetrwanie, Pawęż, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „GARDA PRZEŁAMANA!” ×346; „OKRZYK!” ×235; „SZAŁ!” ×177; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×100; „Portale gasną – dobij pozostałe demony” ×87; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×40; „POLEGŁEŚ” ×30; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×21.

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
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×225, uśpiony wróg blokuje koniec piętra ×13, garda nie zatrzymała ciosu ×13, zablokowany ruch ×3.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 13 podejściach)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 143°) (w 2 podejściach)
  - Ognisty chochlik: „Pazury” przeszedł przez gardę (kąt 124°)
  - Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 159°)
  - Ognisty chochlik: „Pazury” przeszedł przez gardę (kąt 153°)
  - Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 150°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 145°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 168°)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,1; 6,9)
  - piętro 5 „V. Brama Strażnika”: brak postępu przy (-9,1; -8,3)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 120°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 130°)

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

- Wygrałem 20/50. Wyzwanie umiarkowane; po opanowaniu parowania wieża przestaje straszyć.
- Progres między podejściami słabo odczuwalny: 4,2 → 3,8 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 27% udanych (n=15). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Władca Bramy (38,8 trafień, 61,6 s), Kasztelan Potępionych (13,1 trafień, 14,0 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta topieli (1,1), Wodny chochlik (1,1), Kamienny chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 62 s i 39 trafień do zabicia.
- Mój nemezis: Kultysta – 18 z 30 śmierci (najczęściej „Kula ognia”).
- 28/30 razy zginąłem z flaszkami w kieszeni. 2 z tych śmierci to utrata ≥50% HP w 3 s, a 9% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 84% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Piekielna kula”: 441 obrażeń na użycie.
- Skill startowy to 49% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 9%. Różnych skilli na podejście: średnio 5,1.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (40×, śr. piętro 4,2). Inne: Miecz i tarcza + Rozpłatanie 6× (3,7), Miecz i tarcza + Cięcie wichru 3× (3,7), Dwuręczny + Rozpłatanie 1× (2,0).
- Unik to moje główne narzędzie: 43,5 na podejście, w tym 4,2 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 72% przypadków.
- Gra nie wykonała uniku od razu średnio 10,9× na podejście – najczęściej: w trakcie poprzedniego uniku 6,1, drgnięcie (trafiony) 2,0, brak wytrzymałości 1,2. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (32% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.

