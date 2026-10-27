# Raport botów grających — Wojownik

Wygenerowano: 2026-10-27 22:04. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 17 (85%) | 5,4 | 6,0 | 5,2 min | 107,6 | 17227 / 948 | 2,8 | 22% |
| Gracz H&S / souls-like | 20 | 13 (65%) | 4,8 | 6,0 | 4,7 min | 99,1 | 16084 / 859 | 1,8 | 24% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 2 ★ ★ ★ ★ 2 ★ 2 ★
- Gracz H&S / souls-like: ★ ★ 2 ★ 2 ★ 3 ★ ★ ★ 2 ★ 3 ★ ★ 2 ★ ★ ★ 3

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×3.
  Ostatni cios: Chochlik wichru – „Pazury” ×2; Kultysta – „Kula ognia” ×1. Śmierć ciosem ≥40% maks. HP: 0/3. Zginął mając flaszki: 2/3.
  Ostatnie 3 s przed śmiercią: średnio 21% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/3). Picie: 63 prób, 55 wypitych (13% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×4, 3. III. Władca Bramy ×3.
  Ostatni cios: Władca Bramy – „Piekielne kule” ×2; Kultysta – „Kula ognia” ×1; Chochlik wichru – „Pazury” ×1; Władca Bramy – „Cięcie w szale” ×1; Czerw – „Ugryzienie” ×1; Kamienny chochlik – „Pazury” ×1. Śmierć ciosem ≥40% maks. HP: 0/7. Zginął mając flaszki: 6/7.
  Ostatnie 3 s przed śmiercią: średnio 37% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/7). Picie: 42 prób, 36 wypitych (14% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 68 s | 26 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 17 | 71 s | 236 | 0,9 | 46,3 |
| Tester QA | 3. III. Władca Bramy | 17 | 17 | 40 s | 363 | 1,4 | 7,9 |
| Tester QA | 4. IV. Zbrojownia | 17 | 17 | 71 s | 111 | 0,1 | 14,9 |
| Tester QA | 5. V. Brama Strażnika | 17 | 17 | 77 s | 231 | 0,6 | 13,0 |
| Tester QA | 6. VI. Szczyt Wieży | 17 | 17 | 18 s | 104 | 0,1 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 71 s | 54 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 16 | 73 s | 259 | 0,7 | 45,7 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 16 | 13 | 54 s | 392 | 1,1 | 7,6 |
| Gracz H&S / souls-like | 4. IV. Zbrojownia | 13 | 13 | 64 s | 152 | 0,2 | 13,2 |
| Gracz H&S / souls-like | 5. V. Brama Strażnika | 13 | 13 | 69 s | 174 | 0,2 | 12,5 |
| Gracz H&S / souls-like | 6. VI. Szczyt Wieży | 13 | 13 | 16 s | 31 | 0,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Władca Bramy | 30 | 42,4 s | 30,6 | 462 | 11604 | 387 | 3 |
| Kultysta | 465 | 0,2 s | 1,1 | 426 | 6630 | 14 | 2 |
| Szkieletowy łucznik | 210 | 1,8 s | 2,5 | 318 | 3507 | 17 | 0 |
| Kasztelan Potępionych | 30 | 12,8 s | 11,1 | 86 | 2171 | 72 | 0 |
| Czerw | 843 | 0,3 s | 1,2 | 383 | 1604 | 2 | 1 |
| Harpia | 30 | 0,4 s | 1,5 | 138 | 1447 | 48 | 0 |
| Chochlik wichru | 432 | 0,5 s | 1,4 | 130 | 1067 | 2 | 3 |
| Ognisty chochlik | 388 | 0,1 s | 1,1 | 107 | 1052 | 2 | 0 |
| Kolos | 30 | 2,1 s | 3,0 | 37 | 1039 | 35 | 0 |
| Ożywiony nietoperz | 292 | 0,5 s | 1,3 | 167 | 902 | 3 | 0 |
| Strażnik Bramy | 65 | 6,7 s | 6,8 | 67 | 614 | 9 | 0 |
| Kamienny chochlik | 418 | 0,2 s | 1,1 | 77 | 608 | 1 | 1 |
| Wodny chochlik | 417 | 0,2 s | 1,1 | 86 | 532 | 1 | 0 |
| Lodowy strażnik | 25 | 7,3 s | 7,4 | 26 | 530 | 21 | 0 |
| Heretyk | 25 | 2,0 s | 2,6 | 27 | 482 | 19 | 0 |
| Tkaczka | 30 | 1,9 s | 2,2 | 41 | 458 | 15 | 0 |
| Upiór | 30 | 2,7 s | 3,0 | 62 | 450 | 15 | 0 |
| Nekromanta | 30 | 2,6 s | 2,4 | 107 | 394 | 13 | 0 |
| Kultysta topieli | 30 | 0,0 s | 1,0 | 16 | 272 | 9 | 0 |
| Legionista | 30 | 5,1 s | 5,4 | 31 | 187 | 6 | 0 |
| Taranowiec | 30 | 0,9 s | 2,1 | 15 | 155 | 5 | 0 |
| Gargulec | 30 | 1,9 s | 3,3 | 10 | 136 | 5 | 0 |
| Ghul Wieży | 38 | 1,5 s | 2,8 | 18 | 133 | 3 | 0 |
| Pajączek | 30 | 0,5 s | 1,4 | 11 | 90 | 3 | 0 |
| Płonący ghul | 22 | 1,6 s | 2,9 | 10 | 53 | 2 | 0 |
| Kultysta burzy | 5 | 3,2 s | 3,2 | 2 | 26 | 5 | 0 |
| Ogar otchłani | 30 | 0,2 s | 1,9 | 5 | 8 | 0 | 0 |
| Ognisty chochlik (elita) | 53 | 5,0 s | 4,6 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 15 | 8,6 s | 4,7 | ↑ | ↑ | ↑ |  |
| Worek jaj | 23 | 0,1 s | 1,0 | 0 | 0 | 0 | 0 |
| Czerw (elita) | 8 | 3,0 s | 3,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 564 | 78% | 3% | 2% | 14% | 3% |
| ciężki | 27 | 41% | 4% | 15% | 41% | 0% |
| nieparowalny | 627 | 77% | 6% | 0% | 10% | 7% |
| nieblokowalny | 60 | 68% | 0% | 0% | 32% | 0% |
| obszarowy | 108 | 70% | 23% | 0% | 0% | 6% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 761 | 52% | 2% | 1% | 43% | 1% |
| ciężki | 48 | 33% | 0% | 4% | 63% | 0% |
| nieparowalny | 552 | 71% | 2% | 0% | 18% | 10% |
| nieblokowalny | 37 | 49% | 0% | 0% | 51% | 0% |
| obszarowy | 81 | 60% | 37% | 0% | 0% | 2% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 638 | 20% | 47% | 4% | 9% | 20% |
| Tester QA (0,30 s) | ciężki | 49 | 2% | 0% | 0% | 45% | 53% |
| Tester QA (0,30 s) | nieparowalny | 139 | 5% | 4% | 11% | 4% | 76% |
| Tester QA (0,30 s) | nieblokowalny | 69 | 1% | 0% | 1% | 17% | 80% |
| Tester QA (0,30 s) | obszarowy | 61 | 10% | 3% | 3% | 33% | 51% |
| Tester QA (0,30 s) | pocisk | 492 | 3% | 18% | 14% | 26% | 39% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 941 | 1% | 26% | 49% | 11% | 13% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 63 | 0% | 0% | 0% | 2% | 98% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 133 | 5% | 0% | 9% | 9% | 77% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 55 | 2% | 0% | 0% | 7% | 91% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 55 | 5% | 4% | 2% | 22% | 67% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 584 | 3% | 15% | 10% | 20% | 51% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Pajączek: Pajączek | 0,31 s | 25 |
| Chochlik wichru: Pazury | 0,33 s | 250 |
| Czerw: Ugryzienie | 0,33 s | 584 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 261 |
| Wodny chochlik: Pazury | 0,33 s | 183 |
| Kamienny chochlik: Pazury | 0,33 s | 184 |
| Ognisty chochlik: Pazury | 0,33 s | 213 |
| Władca Bramy: Cięcie w szale | 0,41 s | 35 |
| Ghul Wieży: Pazury | 0,42 s | 16 |
| Płonący ghul: Pazury | 0,42 s | 8 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 78% (n=50) | – | 70% (n=56) | – | 6% (n=17) | 84% (n=119) |
| blok (eksperyment) | 88% (n=33) | 50% (n=2) | 100% (n=5) | 100% (n=5) | – | 68% (n=25) |
| brak reakcji (eksperyment) | 100% (n=35) | 100% (n=2) | 100% (n=12) | – | 100% (n=3) | 95% (n=21) |
| niezauważony | 90% (n=83) | – | 97% (n=167) | 100% (n=27) | 100% (n=23) | – |
| parowanie | 60% (n=10) | 0% (n=3) | – | – | – | – |
| parowanie (eksperyment) | 73% (n=15) | – | 100% (n=10) | 100% (n=1) | – | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 89% (n=81) |
| unik | 53% (n=118) | 17% (n=12) | 53% (n=53) | 26% (n=19) | – | 82% (n=51) |
| unik (eksperyment) | 79% (n=53) | 0% (n=1) | 66% (n=29) | 17% (n=6) | – | 79% (n=14) |
| wyjście z zasięgu | 93% (n=134) | 83% (n=6) | 100% (n=23) | 100% (n=1) | 100% (n=6) | – |
| wyjście z zasięgu (eksperyment) | 96% (n=26) | 100% (n=1) | 100% (n=8) | 100% (n=1) | 100% (n=2) | 82% (n=17) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 18,5 | reakcja na cios wręcz 17,6, unik przed pociskiem 8,6, wyjście z okrążenia 0,8 | drgnięcie (trafiony) 2,4, brak wytrzymałości 2,2, w trakcie uniku 1,5, picie flaszki 0,6, inna 0,3, umiejętność 0,2, unieruchomienie 0,1, mocny atak 0,1, parowanie 0,1, szybki atak 0,1 | 54% (n=351) / 69% (n=172) |
| Gracz H&S / souls-like | 35,9 | reakcja na cios wręcz 32,3, unik przed pociskiem 12,1, doskok do odsłoniętego celu 3,6, wyjście z okrążenia 0,8 | w trakcie uniku 5,9, drgnięcie (trafiony) 2,5, brak wytrzymałości 0,8, picie flaszki 0,7, inna 0,4, parowanie 0,1, mocny atak 0,1, umiejętność 0,1, unieruchomienie 0,1 | 77% (n=645) / 73% (n=241) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 5 | 6,0 | 115,0 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 4 | 5,2 | 105,6 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 4 | 5,2 | 105,0 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 4 | 5,2 | 104,8 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 11 | 10 | 5,7 | 109,6 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 3 | 2 | 5,0 | 108,0 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 3 | 1 | 3,3 | 77,7 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 3 | 0 | 2,3 | 73,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 70365 | 20% | 473 | 149 |
| Tąpnięcie | 62076 | 18% | 500 | 124 |
| Zamach | 59081 | 17% |  |  |
| Rozpłatanie | 57454 | 17% | 397 | 145 |
| Cięcie | 25222 | 7% |  |  |
| Egzekucja | 19192 | 6% |  |  |
| Okrzyk bojowy | 11194 | 3% | 409 | 27 |
| Potężne cięcie | 10642 | 3% |  |  |
| Upadek kata | 6820 | 2% | 45 | 152 |
| Ciężki rzut | 4407 | 1% | 29 | 152 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Krwawy pakt ×30, Kamienna osłona ×2.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 103222 | 32% | 673 | 153 |
| Rozpłatanie | 100684 | 31% | 576 | 175 |
| Szarża | 52789 | 16% | 634 | 83 |
| Zamach | 30002 | 9% |  |  |
| Egzekucja | 15054 | 5% |  |  |
| Okrzyk bojowy | 5883 | 2% | 95 | 62 |
| Rąbnięcie | 3771 | 1% |  |  |
| Tąpnięcie | 3716 | 1% | 36 | 103 |
| Rozłupanie | 3121 | 1% |  |  |
| Riposta | 2016 | 1% |  |  |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 102,1, szybki atak 44,7, drgnięcie (trafiony) 28,8, unik 18,5, mocny atak 15,8, blok 11,0, przełamana garda 3,5, flaszka (próba) 3,2, parowanie 1,1, riposta 0,3.
Czas w bloku 5 s, w biegu 23 s, bez wytrzymałości (<10%) 74 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Oddech wojownika ×4, wzmocnienie: Ostrze gniewu ×4, wzmocnienie: Obrońca ×4, wzmocnienie: Siła tytana ×4, ulepszenie: Ulepszenie: Tąpnięcie → +4 ×3, przedmiot: Włócznia +2 ×3, ulepszenie: Ulepszenie: Tąpnięcie → +2 ×3, wzmocnienie: Zwinność ×2.
Zakupy: ulepszenie: Tąpnięcie ×48, ulepszenie broni ×34, ulepszenie: Rozpłatanie ×17, ulepszenie: Okrzyk bojowy ×17, ulepszenie: Cięcie wichru ×16, dodatkowa flaszka ×13, oferta: Trzęsienie ×4, ulepszenie: Krwawy pakt ×4.
Punkty cech: Strength 37, Toughness 17, Dexterity 17, Intelligence 17.
Komunikaty gry: „OKRZYK!” ×398; „SZAŁ!” ×121; „GARDA PRZEŁAMANA!” ×69; „Broń jeszcze w locie” ×51; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×40; „Portale gasną – dobij pozostałe demony” ×38; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×34; „Krwawy pakt: 3 czary bez many” ×30.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 100,8, unik 35,9, szybki atak 26,9, drgnięcie (trafiony) 23,0, mocny atak 15,7, blok 9,8, przełamana garda 3,2, flaszka (próba) 2,1, parowanie 0,6, riposta 0,5.
Czas w bloku 4 s, w biegu 14 s, bez wytrzymałości (<10%) 84 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Szarża → +3 ×9, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×5, wzmocnienie: Siła tytana ×5, ulepszenie: Ulepszenie: Szarża → +1 ×5, ulepszenie: Ulepszenie: Szarża → +4 ×4, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×4, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×4, wzmocnienie: Kolekcjoner flaszek ×4.
Zakupy: ulepszenie: Rozpłatanie ×50, ulepszenie: Szarża ×36, ulepszenie: Okrzyk bojowy ×31, ulepszenie broni ×26, ulepszenie: Cięcie wichru ×22, dodatkowa flaszka ×11, ulepszenie: Tąpnięcie ×3, ulepszenie: Rzut nożami ×1.
Punkty cech: Toughness 33, Strength 42.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Kontra, Młynek, Egzekucja, Trzęsienie, Uderzenie gromu, Puklerz, Pawęż, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×81; „GARDA PRZEŁAMANA!” ×66; „SZAŁ!” ×50; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×40; „Portale gasną – dobij pozostałe demony” ×39; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×26; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×16; „Władca Bramy wpada w szał!” ×15.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×145, uśpiony wróg blokuje koniec piętra ×13, zablokowany ruch ×5, garda nie zatrzymała ciosu ×1.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 13 podejściach)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 0,0)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 8,3)
  - piętro 6 „VI. Szczyt Wieży”: brak postępu przy (0,0; -2,2)
  - piętro 3 „III. Władca Bramy”: brak postępu przy (-3,5; -0,2)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 140°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (6,6; 1,1)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×120, uśpiony wróg blokuje koniec piętra ×11, zablokowany ruch ×3, garda nie zatrzymała ciosu ×1.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - Czerw: „Ugryzienie” przeszedł przez gardę (kąt 131°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,7; 9,1)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-5,6; -7,9)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-5,6; -7,7)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (5 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 1× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta: „Kula ognia” przeszedł przez gardę (kąt 140°)
- Parowanie zwykłych ciosów: 68% ciosów mimo to rani (n=25) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 24% ciosów (n=25).
- Unik przed ciężkim: działa – rani tylko 15% ciosów (n=13).
- Blok zwykłych ciosów: 82% ciosów mimo to rani (n=83) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=8) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 31% pocisków, przed którymi był unik (n=172).
- Blok pocisku: 81% ciosów mimo to rani (n=144) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 67% decyzji zapada, gdy do trafienia zostało <0,1 s (n=638) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=49).
- Telegraf „nieparowalny” daje czas: tylko 9% decyzji zapada mniej niż 0,1 s przed trafieniem (n=139).
- Telegraf „nieblokowalny” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=69).
- Telegraf „obszarowy” daje czas: tylko 13% decyzji zapada mniej niż 0,1 s przed trafieniem (n=61).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,5 ms/klatkę (maks. 73 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 13/20 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Progres między podejściami słabo odczuwalny: 5,6 → 5,2 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie siedzi: 50% udanych na zwykłych ciosach, 0,5 ripost na podejście. To najlepsza część walki.
- Gąbki na obrażenia: Władca Bramy (34,8 trafień, 50,2 s), Kasztelan Potępionych (10,5 trafień, 12,1 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta topieli (1,0), Ognisty chochlik (1,0), Wodny chochlik (1,0) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 50 s i 35 trafień do zabicia.
- Mój nemezis: Władca Bramy – 3 z 7 śmierci (najczęściej „Piekielne kule”).
- 6/7 razy zginąłem z flaszkami w kieszeni. 2 z tych śmierci to utrata ≥50% HP w 3 s, a 14% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 83% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Rozpłatanie”: 175 obrażeń na użycie.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (11×, śr. piętro 6,6). Inne: Miecz i tarcza + Rozpłatanie 3× (3,7), Miecz i tarcza + Cięcie wichru 3× (5,7), Dwuręczny + Rozpłatanie 3× (2,3).
- Unik to moje główne narzędzie: 35,9 na podejście, w tym 4,4 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 73% przypadków.
- Gra nie wykonała uniku od razu średnio 10,6× na podejście – najczęściej: w trakcie poprzedniego uniku 5,9, drgnięcie (trafiony) 2,5, brak wytrzymałości 0,8. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (30% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.

