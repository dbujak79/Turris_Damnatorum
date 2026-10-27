# Raport botów grających — Wojownik

Wygenerowano: 2026-10-27 20:48. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 10 (50%) | 4,3 | 5,5 | 4,8 min | 94,5 | 13998 / 1120 | 3,3 | 12% |
| Gracz H&S / souls-like | 20 | 13 (65%) | 4,7 | 6,0 | 4,9 min | 98,3 | 15694 / 898 | 2,4 | 21% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 2 2 2 ★ 5 ★ ★ ★ 2 ★ 2 ★ 2 ★ 2 ★ 5 ★ 2 ★
- Gracz H&S / souls-like: 2 ★ 2 ★ ★ ★ 3 2 2 ★ ★ ★ ★ ★ ★ 2 3 ★ ★ ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×8, 5. V. Brama Strażnika ×2.
  Ostatni cios: Kultysta – „Kula ognia” ×6; Czerw – „Ugryzienie” ×2; Szkieletowy łucznik – „Strzał” ×2. Śmierć ciosem ≥40% maks. HP: 0/10. Zginął mając flaszki: 8/10.
  Ostatnie 3 s przed śmiercią: średnio 35% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/10). Picie: 78 prób, 65 wypitych (17% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×5, 3. III. Władca Bramy ×2.
  Ostatni cios: Kultysta – „Kula ognia” ×2; Kamienny chochlik – „Pazury” ×1; Ognisty chochlik – „Pazury” ×1; Władca Bramy – „Piekielne kule” ×1; Czerw – „Ugryzienie” ×1; Władca Bramy – „Zamaszyste cięcie” ×1. Śmierć ciosem ≥40% maks. HP: 0/7. Zginął mając flaszki: 5/7.
  Ostatnie 3 s przed śmiercią: średnio 44% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/7). Picie: 58 prób, 47 wypitych (19% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 66 s | 37 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 12 | 75 s | 407 | 1,3 | 41,1 |
| Tester QA | 3. III. Władca Bramy | 12 | 12 | 48 s | 464 | 1,9 | 8,5 |
| Tester QA | 4. IV. Zbrojownia | 12 | 12 | 90 s | 227 | 0,4 | 15,8 |
| Tester QA | 5. V. Brama Strażnika | 12 | 10 | 97 s | 379 | 0,9 | 13,8 |
| Tester QA | 6. VI. Szczyt Wieży | 10 | 10 | 19 s | 69 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 69 s | 45 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 15 | 79 s | 316 | 1,1 | 44,4 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 15 | 13 | 55 s | 394 | 1,3 | 7,7 |
| Gracz H&S / souls-like | 4. IV. Zbrojownia | 13 | 13 | 69 s | 171 | 0,2 | 13,2 |
| Gracz H&S / souls-like | 5. V. Brama Strażnika | 13 | 13 | 77 s | 190 | 0,3 | 13,8 |
| Gracz H&S / souls-like | 6. VI. Szczyt Wieży | 13 | 13 | 16 s | 10 | 0,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Władca Bramy | 25 | 48,3 s | 32,8 | 412 | 10770 | 431 | 2 |
| Kultysta | 459 | 0,2 s | 1,1 | 648 | 10244 | 22 | 8 |
| Szkieletowy łucznik | 169 | 3,5 s | 2,8 | 475 | 3873 | 23 | 2 |
| Czerw | 719 | 0,5 s | 1,3 | 409 | 1490 | 2 | 3 |
| Ognisty chochlik | 418 | 0,2 s | 1,1 | 146 | 1444 | 3 | 1 |
| Harpia | 25 | 0,9 s | 1,7 | 111 | 1256 | 50 | 0 |
| Chochlik wichru | 420 | 0,6 s | 1,4 | 144 | 1185 | 3 | 0 |
| Kolos | 25 | 2,4 s | 2,9 | 33 | 1123 | 45 | 0 |
| Kamienny chochlik | 421 | 0,1 s | 1,1 | 114 | 991 | 2 | 1 |
| Kasztelan Potępionych | 23 | 13,0 s | 12,7 | 49 | 824 | 36 | 0 |
| Tkaczka | 23 | 2,9 s | 2,7 | 81 | 811 | 35 | 0 |
| Ożywiony nietoperz | 254 | 0,7 s | 1,3 | 169 | 794 | 3 | 0 |
| Wodny chochlik | 400 | 0,3 s | 1,1 | 108 | 755 | 2 | 0 |
| Pajączek | 55 | 1,1 s | 1,3 | 87 | 583 | 11 | 0 |
| Lodowy strażnik | 27 | 8,5 s | 7,6 | 32 | 574 | 21 | 0 |
| Strażnik Bramy | 46 | 7,3 s | 6,9 | 60 | 564 | 12 | 0 |
| Heretyk | 14 | 3,2 s | 2,9 | 31 | 563 | 40 | 0 |
| Upiór | 23 | 1,7 s | 3,1 | 92 | 551 | 24 | 0 |
| Kultysta burzy | 11 | 4,4 s | 3,7 | 22 | 479 | 44 | 0 |
| Kultysta topieli | 25 | 0,2 s | 1,1 | 30 | 443 | 18 | 0 |
| Nekromanta | 25 | 4,2 s | 2,2 | 88 | 342 | 14 | 0 |
| Taranowiec | 25 | 1,5 s | 2,2 | 26 | 263 | 11 | 0 |
| Gargulec | 23 | 2,1 s | 3,3 | 9 | 171 | 7 | 0 |
| Płonący ghul | 22 | 2,1 s | 3,0 | 13 | 127 | 6 | 0 |
| Legionista | 25 | 5,4 s | 5,2 | 22 | 77 | 3 | 0 |
| Ghul Wieży | 28 | 1,3 s | 2,6 | 13 | 53 | 2 | 0 |
| Ogar otchłani | 25 | 0,2 s | 2,0 | 3 | 17 | 1 | 0 |
| Ognisty chochlik (elita) | 57 | 5,8 s | 4,7 | ↑ | ↑ | ↑ |  |
| Worek jaj | 28 | 0,4 s | 1,0 | 0 | 0 | 0 | 0 |
| Czerw (elita) | 6 | 4,2 s | 3,3 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 9 | 10,2 s | 4,4 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 819 | 74% | 8% | 2% | 14% | 1% |
| ciężki | 32 | 41% | 3% | 9% | 38% | 9% |
| nieparowalny | 936 | 65% | 24% | 0% | 9% | 2% |
| nieblokowalny | 33 | 73% | 0% | 3% | 24% | 0% |
| obszarowy | 84 | 80% | 18% | 0% | 0% | 2% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 780 | 46% | 7% | 1% | 46% | 0% |
| ciężki | 36 | 17% | 0% | 0% | 83% | 0% |
| nieparowalny | 604 | 64% | 26% | 0% | 10% | 1% |
| nieblokowalny | 51 | 65% | 0% | 0% | 35% | 0% |
| obszarowy | 52 | 71% | 29% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 948 | 22% | 49% | 3% | 7% | 19% |
| Tester QA (0,30 s) | ciężki | 47 | 0% | 0% | 0% | 36% | 64% |
| Tester QA (0,30 s) | nieparowalny | 1520 | 1% | 2% | 3% | 9% | 86% |
| Tester QA (0,30 s) | nieblokowalny | 49 | 2% | 0% | 2% | 12% | 84% |
| Tester QA (0,30 s) | obszarowy | 45 | 11% | 0% | 2% | 38% | 49% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 992 | 0% | 24% | 51% | 10% | 15% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 52 | 0% | 2% | 0% | 2% | 96% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1240 | 1% | 1% | 4% | 5% | 89% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 61 | 0% | 0% | 2% | 10% | 89% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 53 | 6% | 6% | 8% | 25% | 57% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Pajączek: Pajączek | 0,31 s | 120 |
| Chochlik wichru: Pazury | 0,33 s | 288 |
| Kamienny chochlik: Pazury | 0,33 s | 226 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 267 |
| Ognisty chochlik: Pazury | 0,33 s | 268 |
| Czerw: Ugryzienie | 0,33 s | 589 |
| Wodny chochlik: Pazury | 0,33 s | 204 |
| Władca Bramy: Cięcie w szale | 0,41 s | 30 |
| Płonący ghul: Pazury | 0,42 s | 16 |
| Ghul Wieży: Pazury | 0,42 s | 12 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy |
|---|---|---|---|---|---|
| blok | 58% (n=89) | 0% (n=1) | 31% (n=266) | – | 21% (n=14) |
| blok (eksperyment) | 72% (n=54) | 100% (n=3) | 40% (n=48) | 100% (n=3) | 75% (n=4) |
| brak reakcji (eksperyment) | 77% (n=31) | – | 88% (n=49) | 100% (n=2) | 100% (n=1) |
| niezauważony | 86% (n=126) | – | 86% (n=127) | 100% (n=15) | 95% (n=62) |
| parowanie | 70% (n=30) | 100% (n=1) | – | – | – |
| parowanie (eksperyment) | 70% (n=27) | 25% (n=4) | 93% (n=28) | 0% (n=1) | – |
| unik | 68% (n=210) | 25% (n=12) | 77% (n=194) | 20% (n=10) | – |
| unik (eksperyment) | 68% (n=50) | 50% (n=6) | 86% (n=71) | 100% (n=1) | 100% (n=3) |
| wyjście z zasięgu | 91% (n=159) | 100% (n=4) | 91% (n=119) | – | – |
| wyjście z zasięgu (eksperyment) | 74% (n=43) | 100% (n=1) | 97% (n=34) | 100% (n=1) | – |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 5 | 6,0 | 116,2 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 4 | 5,2 | 109,6 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 3,2 | 81,4 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 1 | 2,8 | 70,6 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 15 | 12 | 5,3 | 107,1 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 2 | 1 | 4,0 | 91,5 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 2 | 0 | 2,0 | 63,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 1 | 0 | 2,0 | 51,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 74286 | 27% | 495 | 150 |
| Tąpnięcie | 47739 | 17% | 413 | 116 |
| Zamach | 44082 | 16% |  |  |
| Rozpłatanie | 40372 | 14% | 238 | 170 |
| Cięcie | 17459 | 6% |  |  |
| Egzekucja | 12762 | 5% | 15 | 851 |
| Okrzyk bojowy | 9938 | 4% | 359 | 28 |
| Trzęsienie | 6872 | 2% | 42 | 164 |
| Potężne cięcie | 6191 | 2% |  |  |
| Cięcie z wyskoku | 4325 | 2% | 30 | 144 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Wypad ×8, Przyzwanie chochlików ×1, Krwawy pakt ×12.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 116330 | 37% | 783 | 149 |
| Rozpłatanie | 84979 | 27% | 505 | 168 |
| Szarża | 45481 | 14% | 577 | 79 |
| Zamach | 25673 | 8% |  |  |
| Egzekucja | 18151 | 6% |  |  |
| Okrzyk bojowy | 5682 | 2% | 86 | 66 |
| Tąpnięcie | 5416 | 2% | 51 | 106 |
| Rąbnięcie | 4643 | 1% |  |  |
| Rozłupanie | 3849 | 1% |  |  |
| Szarża barkiem | 2108 | 1% |  |  |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 85,6, szybki atak 32,4, drgnięcie (trafiony) 28,3, unik 28,3, blok 22,2, mocny atak 10,9, flaszka (próba) 3,9, parowanie 3,7, przełamana garda 1,8, riposta 0,6.
Czas w bloku 21 s, w biegu 19 s, bez wytrzymałości (<10%) 73 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Żelazna skóra ×5, wzmocnienie: Obrońca ×4, ulepszenie: Ulepszenie: Cięcie wichru → +2 ×3, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×2, wzmocnienie: Hart ducha ×2, przedmiot: Gwiazda zaranna +1 ×2, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×2, przedmiot: Skórznia +2 ×2.
Zakupy: ulepszenie: Tąpnięcie ×43, ulepszenie broni ×22, ulepszenie: Cięcie wichru ×14, ulepszenie: Okrzyk bojowy ×14, ulepszenie: Rozpłatanie ×12, dodatkowa flaszka ×7, oferta: Trzęsienie ×6, ulepszenie: Cięcie z wyskoku ×3.
Punkty cech: Strength 30, Toughness 12, Dexterity 12, Intelligence 12.
Komunikaty gry: „OKRZYK!” ×347; „SZAŁ!” ×131; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×40; „Portale gasną – dobij pozostałe demony” ×37; „GARDA PRZEŁAMANA!” ×36; „PAROWANIE!” ×22; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×22; „Postawa przełamana – RIPOSTA!” ×17.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 100,8, unik 39,1, drgnięcie (trafiony) 22,8, szybki atak 22,0, blok 15,9, mocny atak 15,2, flaszka (próba) 2,9, parowanie 0,3, riposta 0,2, przełamana garda 0,2.
Czas w bloku 19 s, w biegu 14 s, bez wytrzymałości (<10%) 76 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Szarża → +3 ×8, wzmocnienie: Zwinność ×6, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×6, wzmocnienie: Pijawka ×5, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×4, ulepszenie: Ulepszenie: Szarża → +4 ×4, wzmocnienie: Żelazna skóra ×4, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×3.
Zakupy: ulepszenie: Rozpłatanie ×44, ulepszenie: Szarża ×34, ulepszenie: Okrzyk bojowy ×33, ulepszenie: Cięcie wichru ×26, ulepszenie broni ×26, dodatkowa flaszka ×14, ulepszenie: Tąpnięcie ×3, ulepszenie: Uderzenie tarczą ×2.
Punkty cech: Toughness 33, Strength 41.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Kontra, Młynek, Egzekucja, Trzęsienie, Uderzenie gromu, Puklerz, Przetrwanie, Pawęż, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „OKRZYK!” ×78; „SZAŁ!” ×42; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×40; „Portale gasną – dobij pozostałe demony” ×40; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×26; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×15; „Władca Bramy wpada w szał!” ×14; „Kasztelan wpada w szał!” ×13.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×122, uśpiony wróg blokuje koniec piętra ×8, zablokowany ruch ×5, garda nie zatrzymała ciosu ×4, skill w slocie nie do użycia ×1, wyjątek w logu ×1.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 8 podejściach)
  - „Pocisk arkanów” w slocie 3: wymaga Inteligencji, klasa nie spełnia
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,8; 1,3)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (6,4; -9,1)
  - Szkieletowy łucznik: „Strzał w nogi” przeszedł przez gardę (kąt 155°)
  - Upiór: „Upiór: Tether” przeszedł przez gardę (kąt 134°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-0,9; -0,7)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,8; 0,3)
  - Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 166°)
  - Szkieletowy łucznik: „Strzał” przeszedł przez gardę (kąt 141°)
  - NullReferenceException: Object reference not set to an instance of an object
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-1,0; 4,5)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×106, uśpiony wróg blokuje koniec piętra ×11, zablokowany ruch ×2, skill w slocie nie do użycia ×1.
  - piętro 5 „V. Brama Strażnika”: Gargulec czeka uśpiony – trzeba do niego podejść (w 11 podejściach)
  - „Lodowa włócznia” w slocie 3: wymaga Inteligencji, klasa nie spełnia
  - piętro 5 „V. Brama Strażnika”: brak postępu przy (-8,0; 9,1)
  - piętro 5 „V. Brama Strażnika”: brak postępu przy (7,9; 6,9)

## Wnioski testera

- **BŁĄD (P1):** 1 wpisów błędów/wyjątków w logu podczas gry, m.in.: NullReferenceException: Object reference not set to an instance of an object.
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (5 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 4× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Szkieletowy łucznik: „Strzał w nogi” przeszedł przez gardę (kąt 155°)
- Parowanie zwykłych ciosów: 70% ciosów mimo to rani (n=57) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 27% ciosów (n=11).
- Unik przed ciężkim: działa – rani tylko 33% ciosów (n=18).
- Blok zwykłych ciosów: 64% ciosów mimo to rani (n=143) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 71% decyzji zapada, gdy do trafienia zostało <0,1 s (n=948) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=47).
- Telegraf „nieparowalny” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1520).
- Telegraf „nieblokowalny” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=49).
- Telegraf „obszarowy” daje czas: tylko 11% decyzji zapada mniej niż 0,1 s przed trafieniem (n=45).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,7 ms/klatkę (maks. 86 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 13/20 – za łatwo. Brakuje mi zagrożenia, które wymusza naukę wzorców.
- Czuję progres: druga połowa podejść średnio piętro 6,1 wobec 4,6 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 40% udanych (n=15). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Władca Bramy (34,7 trafień, 52,0 s), Kasztelan Potępionych (10,1 trafień, 11,7 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta topieli (1,0), Kultysta (1,0), Kamienny chochlik (1,0) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 52 s i 35 trafień do zabicia.
- 5/7 razy zginąłem z flaszkami w kieszeni. 2 z tych śmierci to utrata ≥50% HP w 3 s, a 19% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 82% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Rozpłatanie”: 168 obrażeń na użycie.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (15×, śr. piętro 6,1). Inne: Miecz i tarcza + Rozpłatanie 2× (2,0), Dwuręczny + Cięcie wichru 2× (4,5), Dwuręczny + Rozpłatanie 1× (2,0).
- Wytrzymałość kończy się często (26% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.

