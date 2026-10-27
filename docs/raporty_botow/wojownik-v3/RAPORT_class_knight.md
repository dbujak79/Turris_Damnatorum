# Raport botów grających — Wojownik

Wygenerowano: 2026-10-27 21:32. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 17 (85%) | 5,5 | 6,0 | 5,2 min | 108,6 | 17217 / 908 | 2,3 | 25% |
| Gracz H&S / souls-like | 20 | 14 (70%) | 4,9 | 6,0 | 4,8 min | 99,6 | 15698 / 876 | 2,1 | 22% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: ★ ★ 2 ★ ★ ★ ★ ★ ★ ★ ★ ★ ★ 3 2 ★ ★ ★ ★ ★
- Gracz H&S / souls-like: ★ 2 ★ ★ 2 ★ ★ 2 ★ ★ 2 ★ ★ 3 ★ 2 ★ ★ ★ ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×2, 3. III. Władca Bramy ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×2; Władca Bramy – „Piekielne kule” ×1. Śmierć ciosem ≥40% maks. HP: 0/3. Zginął mając flaszki: 3/3.
  Ostatnie 3 s przed śmiercią: średnio 29% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/3). Picie: 50 prób, 46 wypitych (8% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×5, 3. III. Władca Bramy ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×3; Czerw – „Ugryzienie” ×1; Chochlik wichru – „Pazury” ×1; Władca Bramy – „Piekielne kule” ×1. Śmierć ciosem ≥40% maks. HP: 0/6. Zginął mając flaszki: 6/6.
  Ostatnie 3 s przed śmiercią: średnio 35% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/6). Picie: 47 prób, 42 wypitych (11% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 69 s | 31 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 18 | 72 s | 238 | 1,0 | 47,1 |
| Tester QA | 3. III. Władca Bramy | 18 | 17 | 36 s | 343 | 1,2 | 7,8 |
| Tester QA | 4. IV. Zbrojownia | 17 | 17 | 69 s | 112 | 0,0 | 14,0 |
| Tester QA | 5. V. Brama Strażnika | 17 | 17 | 76 s | 209 | 0,3 | 13,8 |
| Tester QA | 6. VI. Szczyt Wieży | 17 | 17 | 17 s | 67 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 70 s | 35 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 15 | 77 s | 269 | 0,8 | 44,0 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 15 | 14 | 54 s | 379 | 1,3 | 8,1 |
| Gracz H&S / souls-like | 4. IV. Zbrojownia | 14 | 14 | 64 s | 174 | 0,1 | 14,0 |
| Gracz H&S / souls-like | 5. V. Brama Strażnika | 14 | 14 | 72 s | 208 | 0,4 | 12,9 |
| Gracz H&S / souls-like | 6. VI. Szczyt Wieży | 14 | 14 | 15 s | 29 | 0,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Władca Bramy | 31 | 40,3 s | 33,8 | 441 | 11191 | 361 | 2 |
| Kultysta | 505 | 0,2 s | 1,2 | 484 | 7455 | 14 | 5 |
| Szkieletowy łucznik | 217 | 1,9 s | 2,5 | 323 | 3727 | 17 | 0 |
| Kasztelan Potępionych | 31 | 11,4 s | 13,2 | 69 | 1546 | 50 | 0 |
| Harpia | 31 | 0,6 s | 1,8 | 106 | 1349 | 44 | 0 |
| Kolos | 31 | 2,1 s | 3,2 | 35 | 1087 | 35 | 0 |
| Ognisty chochlik | 447 | 0,1 s | 1,1 | 127 | 1046 | 2 | 0 |
| Czerw | 771 | 0,3 s | 1,3 | 261 | 951 | 1 | 1 |
| Lodowy strażnik | 39 | 7,5 s | 7,9 | 48 | 872 | 22 | 0 |
| Kamienny chochlik | 414 | 0,2 s | 1,2 | 100 | 751 | 2 | 0 |
| Ożywiony nietoperz | 293 | 0,4 s | 1,3 | 155 | 724 | 2 | 0 |
| Chochlik wichru | 407 | 0,5 s | 1,4 | 103 | 705 | 2 | 1 |
| Wodny chochlik | 400 | 0,1 s | 1,1 | 90 | 556 | 1 | 0 |
| Upiór | 31 | 2,0 s | 3,2 | 74 | 498 | 16 | 0 |
| Strażnik Bramy | 54 | 6,0 s | 7,3 | 52 | 458 | 8 | 0 |
| Tkaczka | 31 | 2,0 s | 2,3 | 50 | 401 | 13 | 0 |
| Kultysta burzy | 13 | 1,0 s | 3,2 | 20 | 343 | 26 | 0 |
| Kultysta topieli | 31 | 0,0 s | 1,2 | 18 | 329 | 11 | 0 |
| Heretyk | 18 | 2,5 s | 3,1 | 16 | 307 | 17 | 0 |
| Gargulec | 31 | 2,2 s | 3,5 | 18 | 305 | 10 | 0 |
| Płonący ghul | 24 | 1,7 s | 3,3 | 20 | 263 | 11 | 0 |
| Pajączek | 51 | 0,7 s | 1,4 | 26 | 206 | 4 | 0 |
| Nekromanta | 31 | 2,0 s | 2,6 | 51 | 174 | 6 | 0 |
| Legionista | 31 | 3,7 s | 5,0 | 20 | 151 | 5 | 0 |
| Ghul Wieży | 38 | 1,6 s | 3,0 | 17 | 148 | 4 | 0 |
| Taranowiec | 31 | 1,1 s | 2,4 | 19 | 120 | 4 | 0 |
| Ogar otchłani | 31 | 0,2 s | 1,9 | 7 | 19 | 1 | 0 |
| Ognisty chochlik (elita) | 58 | 5,3 s | 4,8 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 15 | 6,4 s | 4,5 | ↑ | ↑ | ↑ |  |
| Worek jaj | 23 | 0,0 s | 1,0 | 0 | 0 | 0 | 0 |
| Czerw (elita) | 4 | 3,3 s | 3,3 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 514 | 76% | 4% | 2% | 17% | 2% |
| ciężki | 31 | 42% | 6% | 3% | 39% | 10% |
| nieparowalny | 614 | 73% | 10% | 0% | 9% | 8% |
| nieblokowalny | 48 | 56% | 0% | 6% | 38% | 0% |
| obszarowy | 60 | 65% | 30% | 0% | 0% | 5% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 716 | 44% | 2% | 0% | 51% | 2% |
| ciężki | 44 | 32% | 2% | 7% | 59% | 0% |
| nieparowalny | 615 | 68% | 3% | 0% | 16% | 13% |
| nieblokowalny | 56 | 59% | 0% | 0% | 41% | 0% |
| obszarowy | 52 | 54% | 38% | 0% | 0% | 8% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 587 | 23% | 44% | 4% | 11% | 18% |
| Tester QA (0,30 s) | ciężki | 50 | 0% | 0% | 2% | 48% | 50% |
| Tester QA (0,30 s) | nieparowalny | 140 | 9% | 6% | 9% | 5% | 71% |
| Tester QA (0,30 s) | nieblokowalny | 67 | 0% | 0% | 1% | 4% | 94% |
| Tester QA (0,30 s) | obszarowy | 57 | 5% | 9% | 2% | 37% | 47% |
| Tester QA (0,30 s) | pocisk | 478 | 1% | 21% | 16% | 24% | 38% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 919 | 1% | 24% | 52% | 10% | 13% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 55 | 0% | 0% | 0% | 2% | 98% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 131 | 5% | 0% | 8% | 11% | 76% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 65 | 3% | 2% | 0% | 5% | 91% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 55 | 11% | 0% | 7% | 24% | 58% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 714 | 3% | 13% | 9% | 20% | 55% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Pajączek: Pajączek | 0,31 s | 52 |
| Ognisty chochlik: Pazury | 0,33 s | 239 |
| Czerw: Ugryzienie | 0,33 s | 506 |
| Kamienny chochlik: Pazury | 0,33 s | 207 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 241 |
| Wodny chochlik: Pazury | 0,33 s | 199 |
| Chochlik wichru: Pazury | 0,33 s | 223 |
| Władca Bramy: Cięcie w szale | 0,41 s | 33 |
| Płonący ghul: Pazury | 0,42 s | 8 |
| Ghul Wieży: Pazury | 0,42 s | 13 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 60% (n=42) | 0% (n=2) | 68% (n=78) | – | 0% (n=14) | 80% (n=114) |
| blok (eksperyment) | 93% (n=28) | 100% (n=3) | 67% (n=12) | 100% (n=3) | 50% (n=6) | 79% (n=28) |
| brak reakcji (eksperyment) | 100% (n=31) | 100% (n=3) | 100% (n=6) | 100% (n=4) | 100% (n=3) | 95% (n=19) |
| niezauważony | 96% (n=82) | – | 99% (n=157) | 100% (n=9) | 100% (n=14) | – |
| parowanie | 56% (n=16) | 100% (n=3) | – | 0% (n=2) | – | – |
| parowanie (eksperyment) | 86% (n=14) | 0% (n=1) | 100% (n=1) | 67% (n=3) | – | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 97% (n=38) |
| unik | 43% (n=103) | 9% (n=11) | 47% (n=47) | 25% (n=24) | – | 71% (n=63) |
| unik (eksperyment) | 57% (n=44) | 33% (n=3) | 22% (n=9) | 100% (n=1) | 100% (n=5) | 80% (n=20) |
| wyjście z zasięgu | 98% (n=124) | 100% (n=4) | 100% (n=21) | – | – | – |
| wyjście z zasięgu (eksperyment) | 85% (n=26) | 100% (n=1) | 100% (n=2) | 100% (n=2) | 100% (n=1) | 90% (n=20) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 17,9 | reakcja na cios wręcz 15,9, unik przed pociskiem 8,7, wyjście z okrążenia 0,5 | drgnięcie (trafiony) 2,2, brak wytrzymałości 1,6, w trakcie uniku 1,2, inna 0,4, picie flaszki 0,4, parowanie 0,2, riposta 0,2, mocny atak 0,2 | 66% (n=318) / 65% (n=173) |
| Gracz H&S / souls-like | 39,6 | reakcja na cios wręcz 33,8, unik przed pociskiem 13,8, doskok do odsłoniętego celu 3,6, wyjście z okrążenia 0,6 | w trakcie uniku 5,6, drgnięcie (trafiony) 2,0, brak wytrzymałości 1,0, picie flaszki 0,8, inna 0,3, unieruchomienie 0,2, blok 0,1, mocny atak 0,1 | 82% (n=676) / 78% (n=275) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 5 | 6,0 | 113,0 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 5 | 6,0 | 116,4 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 4 | 5,4 | 107,6 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 3 | 4,4 | 97,2 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 12 | 9 | 5,1 | 102,9 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 3 | 2 | 4,7 | 96,7 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 3 | 2 | 4,7 | 94,7 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 2 | 1 | 4,0 | 91,5 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 68698 | 20% | 469 | 146 |
| Tąpnięcie | 61356 | 18% | 493 | 124 |
| Rozpłatanie | 60019 | 17% | 435 | 138 |
| Zamach | 57229 | 17% |  |  |
| Cięcie | 24212 | 7% |  |  |
| Egzekucja | 20311 | 6% |  |  |
| Potężne cięcie | 11008 | 3% |  |  |
| Okrzyk bojowy | 10202 | 3% | 414 | 25 |
| Przyzwanie chochlików | 5292 | 2% | 23 | 230 |
| Fala przełamania | 5109 | 1% | 25 | 204 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Krwawy pakt ×16.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Rozpłatanie | 105865 | 34% | 654 | 162 |
| Cięcie wichru | 89262 | 28% | 603 | 148 |
| Szarża | 52852 | 17% | 634 | 83 |
| Zamach | 30605 | 10% |  |  |
| Egzekucja | 15006 | 5% |  |  |
| Okrzyk bojowy | 5982 | 2% | 95 | 63 |
| Rąbnięcie | 4535 | 1% |  |  |
| Tąpnięcie | 4082 | 1% | 31 | 132 |
| Rozłupanie | 2629 | 1% |  |  |
| Szarża barkiem | 1817 | 1% |  |  |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 101,8, szybki atak 45,2, drgnięcie (trafiony) 25,9, unik 17,9, mocny atak 15,9, blok 10,9, przełamana garda 3,0, flaszka (próba) 2,5, parowanie 1,3, riposta 0,6.
Czas w bloku 5 s, w biegu 24 s, bez wytrzymałości (<10%) 74 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Czujność ×5, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×4, wzmocnienie: Oddech wojownika ×3, wzmocnienie: Obrońca ×3, wzmocnienie: Bystry umysł ×3, wzmocnienie: Żelazna skóra ×3, ulepszenie: Ulepszenie: Tąpnięcie → +3 ×3, ulepszenie: Ulepszenie: Tąpnięcie → +4 ×2.
Zakupy: ulepszenie: Tąpnięcie ×47, ulepszenie broni ×34, ulepszenie: Cięcie wichru ×19, ulepszenie: Rozpłatanie ×17, ulepszenie: Okrzyk bojowy ×15, dodatkowa flaszka ×14, oferta: Okrzyk wojenny ×6, oferta: Nova ziemi ×4.
Punkty cech: Strength 37, Toughness 18, Dexterity 17, Intelligence 17.
Komunikaty gry: „OKRZYK!” ×393; „SZAŁ!” ×107; „GARDA PRZEŁAMANA!” ×61; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×40; „Portale gasną – dobij pozostałe demony” ×40; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×34; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×18; „Władca Bramy wpada w szał!” ×17.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 101,1, unik 39,6, szybki atak 27,8, drgnięcie (trafiony) 23,5, mocny atak 16,5, blok 12,9, przełamana garda 4,9, flaszka (próba) 2,4, parowanie 0,4, riposta 0,3.
Czas w bloku 7 s, w biegu 15 s, bez wytrzymałości (<10%) 90 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +4 ×9, ulepszenie: Ulepszenie: Szarża → +3 ×7, wzmocnienie: Pijawka ×5, wzmocnienie: Żelazna skóra ×4, wzmocnienie: Oddech wojownika ×4, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×4, ulepszenie: Ulepszenie: Szarża → +2 ×4, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×4.
Zakupy: ulepszenie: Rozpłatanie ×50, ulepszenie: Szarża ×38, ulepszenie: Okrzyk bojowy ×32, ulepszenie broni ×28, ulepszenie: Cięcie wichru ×20, dodatkowa flaszka ×12, ulepszenie: Tąpnięcie ×2, ulepszenie: Uderzenie tarczą ×2.
Punkty cech: Toughness 34, Strength 43.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Kontra, Młynek, Egzekucja, Trzęsienie, Uderzenie gromu, Puklerz, Przetrwanie, Pawęż, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „GARDA PRZEŁAMANA!” ×97; „OKRZYK!” ×88; „SZAŁ!” ×48; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×40; „Portale gasną – dobij pozostałe demony” ×38; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×28; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×15; „Władca Bramy wpada w szał!” ×15.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×102, uśpiony wróg blokuje koniec piętra ×14, zablokowany ruch ×6, garda nie zatrzymała ciosu ×2, skill w slocie nie do użycia ×1.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 14 podejściach)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,1; -1,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,6)
  - Szkieletowy łucznik: „Strzał w nogi” przeszedł przez gardę (kąt 163°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (5,6; -8,0)
  - piętro 5 „V. Brama Strażnika”: brak postępu przy (9,1; -7,9)
  - piętro 6 „VI. Szczyt Wieży”: brak postępu przy (0,0; -2,0)
  - „Ściana ognia” w slocie 3: wymaga Inteligencji, klasa nie spełnia
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 137°)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (7,8; 0,1)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×88, uśpiony wróg blokuje koniec piętra ×9, garda nie zatrzymała ciosu ×3, zablokowany ruch ×2.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 9 podejściach)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 138°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (6,3; -9,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,2; -7,0)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 123°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 143°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (6 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 2× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Szkieletowy łucznik: „Strzał w nogi” przeszedł przez gardę (kąt 163°)
- Parowanie zwykłych ciosów: 70% ciosów mimo to rani (n=30) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 28% ciosów (n=25).
- Unik przed ciężkim: działa – rani tylko 14% ciosów (n=14).
- Blok zwykłych ciosów: 73% ciosów mimo to rani (n=70) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 35% pocisków, przed którymi był unik (n=173).
- Blok pocisku: 80% ciosów mimo to rani (n=142) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 67% decyzji zapada, gdy do trafienia zostało <0,1 s (n=587) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=50).
- Telegraf „nieparowalny” daje czas: tylko 15% decyzji zapada mniej niż 0,1 s przed trafieniem (n=140).
- Telegraf „nieblokowalny” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=67).
- Telegraf „obszarowy” daje czas: tylko 14% decyzji zapada mniej niż 0,1 s przed trafieniem (n=57).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 79 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 14/20 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Progres między podejściami słabo odczuwalny: 5,5 → 5,6 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 16% udanych (n=19). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Władca Bramy (31,6 trafień, 49,9 s), Kasztelan Potępionych (9,2 trafień, 10,6 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta topieli (1,0), Wodny chochlik (1,0), Ognisty chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 50 s i 32 trafień do zabicia.
- Mój nemezis: Kultysta – 3 z 6 śmierci (najczęściej „Kula ognia”).
- 6/6 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 11% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 82% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Rozpłatanie”: 162 obrażeń na użycie.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (12×, śr. piętro 5,8). Inne: Miecz i tarcza + Rozpłatanie 3× (5,3), Dwuręczny + Rozpłatanie 3× (5,3), Miecz i tarcza + Cięcie wichru 2× (4,5).
- Unik to moje główne narzędzie: 39,6 na podejście, w tym 4,1 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 78% przypadków.
- Gra nie wykonała uniku od razu średnio 9,8× na podejście – najczęściej: w trakcie poprzedniego uniku 5,6, drgnięcie (trafiony) 2,0, brak wytrzymałości 1,0. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (31% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.

