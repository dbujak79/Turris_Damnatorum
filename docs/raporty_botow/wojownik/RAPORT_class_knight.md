# Raport botów grających — Wojownik

Wygenerowano: 2026-10-27 18:40. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 9 (45%) | 4,1 | 5,0 | 4,7 min | 91,2 | 12836 / 1060 | 2,4 | 13% |
| Gracz H&S / souls-like | 20 | 11 (55%) | 4,2 | 6,0 | 4,4 min | 90,8 | 13840 / 838 | 2,2 | 15% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 5 ★ 5 2 ★ ★ 2 2 2 ★ 2 ★ 2 ★ 2 ★ 2 ★ 2 ★
- Gracz H&S / souls-like: 2 ★ 2 ★ ★ 2 ★ ★ 2 ★ 2 2 ★ ★ 2 2 ★ 2 ★ ★

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×9, 5. V. Brama Strażnika ×2.
  Ostatni cios: Kultysta – „Kula ognia” ×6; Tkaczka – „Tkaczka” ×1; Szkieletowy łucznik – „Strzał” ×1; Wodny chochlik – „Pazury” ×1; Czerw – „Ugryzienie” ×1; Kamienny chochlik – „Pazury” ×1. Śmierć ciosem ≥40% maks. HP: 0/11. Zginął mając flaszki: 7/11.
  Ostatnie 3 s przed śmiercią: średnio 33% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/11). Picie: 55 prób, 47 wypitych (15% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×9.
  Ostatni cios: Kultysta – „Kula ognia” ×6; Ognisty chochlik – „Dwa cięcia” ×1; Ognisty chochlik – „Pazury” ×1; Ognisty chochlik – „Drugie cięcie” ×1. Śmierć ciosem ≥40% maks. HP: 0/9. Zginął mając flaszki: 8/9.
  Ostatnie 3 s przed śmiercią: średnio 31% maks. HP straconych (śmierć z ≥50% HP w 3 s: 0/9). Picie: 51 prób, 44 wypitych (14% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 71 s | 40 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 11 | 82 s | 419 | 1,1 | 40,6 |
| Tester QA | 3. III. Władca Bramy | 11 | 11 | 46 s | 375 | 1,1 | 8,1 |
| Tester QA | 4. IV. Zbrojownia | 11 | 11 | 84 s | 164 | 0,0 | 14,3 |
| Tester QA | 5. V. Brama Strażnika | 11 | 9 | 96 s | 496 | 1,2 | 14,2 |
| Tester QA | 6. VI. Szczyt Wieży | 9 | 9 | 21 s | 70 | 0,0 | 1,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 69 s | 48 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 11 | 84 s | 413 | 1,4 | 40,6 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 11 | 11 | 55 s | 305 | 1,0 | 8,4 |
| Gracz H&S / souls-like | 4. IV. Zbrojownia | 11 | 11 | 73 s | 195 | 0,4 | 14,3 |
| Gracz H&S / souls-like | 5. V. Brama Strażnika | 11 | 11 | 74 s | 170 | 0,1 | 13,1 |
| Gracz H&S / souls-like | 6. VI. Szczyt Wieży | 11 | 11 | 15 s | 16 | 0,0 | 1,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 458 | 0,3 s | 1,1 | 759 | 11585 | 25 | 12 |
| Władca Bramy | 22 | 46,0 s | 36,3 | 353 | 7101 | 323 | 0 |
| Szkieletowy łucznik | 148 | 4,1 s | 2,8 | 478 | 4087 | 28 | 1 |
| Ognisty chochlik | 368 | 0,2 s | 1,2 | 168 | 1867 | 4 | 3 |
| Czerw | 721 | 0,3 s | 1,2 | 482 | 1680 | 2 | 1 |
| Chochlik wichru | 393 | 0,7 s | 1,5 | 137 | 1166 | 3 | 0 |
| Tkaczka | 20 | 3,7 s | 2,6 | 106 | 1112 | 56 | 1 |
| Kamienny chochlik | 425 | 0,2 s | 1,2 | 122 | 1101 | 3 | 1 |
| Wodny chochlik | 392 | 0,2 s | 1,1 | 119 | 999 | 3 | 1 |
| Upiór | 20 | 2,1 s | 3,2 | 124 | 922 | 46 | 0 |
| Harpia | 22 | 0,7 s | 1,5 | 85 | 899 | 41 | 0 |
| Kolos | 22 | 2,3 s | 3,2 | 26 | 834 | 38 | 0 |
| Kasztelan Potępionych | 20 | 13,4 s | 13,9 | 41 | 801 | 40 | 0 |
| Pajączek | 66 | 1,4 s | 1,4 | 120 | 790 | 12 | 0 |
| Ożywiony nietoperz | 231 | 0,5 s | 1,3 | 116 | 503 | 2 | 0 |
| Lodowy strażnik | 29 | 8,2 s | 8,5 | 30 | 443 | 15 | 0 |
| Kultysta topieli | 22 | 0,0 s | 1,0 | 27 | 433 | 20 | 0 |
| Nekromanta | 22 | 2,7 s | 2,8 | 83 | 387 | 18 | 0 |
| Kultysta burzy | 10 | 6,8 s | 5,4 | 20 | 381 | 38 | 0 |
| Heretyk | 12 | 3,2 s | 3,2 | 19 | 299 | 25 | 0 |
| Strażnik Bramy | 35 | 6,8 s | 6,7 | 23 | 285 | 8 | 0 |
| Ghul Wieży | 28 | 2,6 s | 3,4 | 26 | 171 | 6 | 0 |
| Płonący ghul | 16 | 2,0 s | 3,4 | 8 | 75 | 5 | 0 |
| Legionista | 22 | 4,8 s | 5,6 | 20 | 22 | 1 | 0 |
| Ogar otchłani | 22 | 0,2 s | 2,0 | 1 | 15 | 1 | 0 |
| Ognisty chochlik (elita) | 53 | 5,9 s | 4,7 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 9 | 8,9 s | 5,6 | ↑ | ↑ | ↑ |  |
| Worek jaj | 26 | 1,3 s | 1,0 | 0 | 0 | 0 | 0 |
| Czerw (elita) | 4 | 4,7 s | 4,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 790 | 73% | 8% | 3% | 14% | 2% |
| ciężki | 23 | 35% | 9% | 9% | 48% | 0% |
| nieparowalny | 983 | 64% | 24% | 0% | 9% | 3% |
| nieblokowalny | 32 | 81% | 0% | 3% | 16% | 0% |
| obszarowy | 51 | 76% | 20% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 850 | 52% | 3% | 1% | 43% | 0% |
| ciężki | 33 | 12% | 0% | 0% | 88% | 0% |
| nieparowalny | 645 | 66% | 21% | 0% | 12% | 0% |
| nieblokowalny | 7 | 100% | 0% | 0% | 0% | 0% |
| obszarowy | 79 | 76% | 23% | 0% | 0% | 1% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 929 | 27% | 41% | 3% | 8% | 21% |
| Tester QA (0,30 s) | ciężki | 34 | 0% | 0% | 0% | 26% | 74% |
| Tester QA (0,30 s) | nieparowalny | 1649 | 0% | 2% | 3% | 8% | 88% |
| Tester QA (0,30 s) | nieblokowalny | 44 | 0% | 0% | 2% | 7% | 91% |
| Tester QA (0,30 s) | obszarowy | 38 | 5% | 5% | 3% | 42% | 45% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 1083 | 1% | 26% | 54% | 8% | 12% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 43 | 0% | 0% | 0% | 0% | 100% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 1400 | 0% | 2% | 4% | 7% | 87% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 48 | 0% | 0% | 0% | 4% | 96% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 40 | 8% | 0% | 3% | 25% | 65% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Pajączek: Pajączek | 0,31 s | 169 |
| Chochlik wichru: Pazury | 0,33 s | 257 |
| Czerw: Ugryzienie | 0,33 s | 718 |
| Kamienny chochlik: Pazury | 0,33 s | 231 |
| Wodny chochlik: Pazury | 0,33 s | 222 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 192 |
| Ghul Wieży: Druga seria | 0,33 s | 8 |
| Ognisty chochlik: Pazury | 0,33 s | 255 |
| Władca Bramy: Cięcie w szale | 0,41 s | 26 |
| Ghul Wieży: Pazury | 0,42 s | 25 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy |
|---|---|---|---|---|---|
| blok | 55% (n=86) | – | 34% (n=293) | – | 29% (n=14) |
| blok (eksperyment) | 77% (n=48) | 0% (n=1) | 52% (n=42) | 100% (n=1) | 100% (n=1) |
| brak reakcji (eksperyment) | 88% (n=33) | 100% (n=2) | 77% (n=57) | 100% (n=3) | – |
| niezauważony | 85% (n=165) | – | 89% (n=146) | 100% (n=16) | 100% (n=32) |
| parowanie | 70% (n=33) | 0% (n=1) | – | 0% (n=1) | – |
| parowanie (eksperyment) | 72% (n=18) | 50% (n=4) | 97% (n=34) | 100% (n=1) | 100% (n=1) |
| unik | 65% (n=178) | 0% (n=8) | 71% (n=175) | 56% (n=9) | – |
| unik (eksperyment) | 79% (n=53) | 0% (n=3) | 90% (n=58) | 0% (n=1) | 100% (n=3) |
| wyjście z zasięgu | 87% (n=145) | 100% (n=3) | 91% (n=134) | – | – |
| wyjście z zasięgu (eksperyment) | 74% (n=31) | 100% (n=1) | 82% (n=44) | – | – |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 5 | 6,0 | 118,8 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 3 | 4,4 | 93,0 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 1 | 3,4 | 82,8 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 2,6 | 70,0 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 9 | 7 | 5,1 | 106,2 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 7 | 4 | 4,3 | 92,1 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 1 | 0 | 2,0 | 58,0 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 3 | 0 | 2,0 | 52,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 69761 | 27% | 489 | 143 |
| Tąpnięcie | 47307 | 18% | 397 | 119 |
| Zamach | 35942 | 14% |  |  |
| Rozpłatanie | 34802 | 14% | 227 | 153 |
| Cięcie | 19548 | 8% |  |  |
| Egzekucja | 11103 | 4% |  |  |
| Okrzyk bojowy | 9547 | 4% | 350 | 27 |
| Potężne cięcie | 7298 | 3% |  |  |
| Przyzwanie chochlików | 4026 | 2% | 13 | 310 |
| Młynek | 3436 | 1% | 57 | 60 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×5.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 107367 | 39% | 670 | 160 |
| Rozpłatanie | 68499 | 25% | 400 | 171 |
| Szarża | 45007 | 16% | 503 | 89 |
| Zamach | 20249 | 7% |  |  |
| Egzekucja | 15147 | 5% |  |  |
| Okrzyk bojowy | 4740 | 2% | 78 | 61 |
| Tąpnięcie | 4327 | 2% | 41 | 106 |
| Rąbnięcie | 3525 | 1% |  |  |
| Rozłupanie | 3398 | 1% |  |  |
| Szarża barkiem | 2107 | 1% |  |  |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 82,0, unik 29,9, szybki atak 29,5, drgnięcie (trafiony) 28,3, blok 22,1, mocny atak 10,3, parowanie 3,7, flaszka (próba) 2,8, przełamana garda 2,5, riposta 0,6.
Czas w bloku 20 s, w biegu 16 s, bez wytrzymałości (<10%) 68 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Pijawka ×4, wzmocnienie: Żelazna skóra ×4, ulepszenie: Ulepszenie: Tąpnięcie → +2 ×3, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×3, wzmocnienie: Czujność ×3, wzmocnienie: Krew przodków ×2, przedmiot: Puginał +3 ×2, przedmiot: Miecz jednoręczny +2 ×1.
Zakupy: ulepszenie: Tąpnięcie ×38, ulepszenie broni ×20, ulepszenie: Cięcie wichru ×17, ulepszenie: Rozpłatanie ×12, ulepszenie: Okrzyk bojowy ×11, dodatkowa flaszka ×5, oferta: Okrzyk wojenny ×3, ulepszenie: Młynek ×3.
Punkty cech: Strength 29, Toughness 11, Dexterity 11, Intelligence 11.
Komunikaty gry: „OKRZYK!” ×341; „SZAŁ!” ×124; „GARDA PRZEŁAMANA!” ×50; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×40; „Portale gasną – dobij pozostałe demony” ×34; „PAROWANIE!” ×23; „Postawa przełamana – RIPOSTA!” ×21; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×20.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 85,6, unik 38,9, drgnięcie (trafiony) 22,3, szybki atak 19,4, blok 13,3, mocny atak 12,9, flaszka (próba) 2,6, parowanie 0,6, riposta 0,4, przełamana garda 0,2.
Czas w bloku 15 s, w biegu 10 s, bez wytrzymałości (<10%) 71 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +1 ×9, wzmocnienie: Ostrze gniewu ×5, wzmocnienie: Żelazna skóra ×5, ulepszenie: Ulepszenie: Szarża → +3 ×4, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×3, wzmocnienie: Jasność umysłu ×3, wzmocnienie: Źródło many ×3, ulepszenie: Ulepszenie: Szarża → +1 ×3.
Zakupy: ulepszenie: Rozpłatanie ×39, ulepszenie: Szarża ×30, ulepszenie: Okrzyk bojowy ×30, ulepszenie: Cięcie wichru ×23, ulepszenie broni ×22, dodatkowa flaszka ×11, ulepszenie: Tąpnięcie ×4, ulepszenie: Krok cienia ×2.
Punkty cech: Toughness 31, Strength 33.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Kontra, Młynek, Egzekucja, Trzęsienie, Uderzenie gromu, Puklerz, Przetrwanie, Pawęż, Wielki topór, Młot burzy, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Amulet salamandry, Amulet przewodnika.
Komunikaty gry: „OKRZYK!” ×65; „SZAŁ!” ×50; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×40; „Portale gasną – dobij pozostałe demony” ×34; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×22; „PAROWANIE!” ×12; „Postawa przełamana – RIPOSTA!” ×11; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×11.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×143, garda nie zatrzymała ciosu ×5, zablokowany ruch ×2, parowanie w oknie nie zadziałało ×1, skill w slocie nie do użycia ×1.
  - Tkaczka: „Tkaczka” przeszedł przez gardę (kąt 129°)
  - Ożywiony nietoperz: „Ukąszenie” przeszedł przez gardę (kąt 139°)
  - Ożywiony nietoperz: „Ukąszenie” przeszedł przez gardę (kąt 171°)
  - piętro 4 „IV. Zbrojownia”: brak postępu przy (-7,8; 0,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (2,1; 0,2)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 136°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 125°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 79°)
  - „Rozprysk arkanów” w slocie 3: wymaga Inteligencji, klasa nie spełnia
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×105, zablokowany ruch ×1.
  - piętro 6 „VI. Szczyt Wieży”: brak postępu przy (0,0; -4,9)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (2 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 1× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 79°)
- **Do sprawdzenia (P3):** 5× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Tkaczka: „Tkaczka” przeszedł przez gardę (kąt 129°)
- Parowanie zwykłych ciosów: 71% ciosów mimo to rani (n=51) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 50% ciosów mimo to rani (n=10) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 0% ciosów (n=11).
- Blok zwykłych ciosów: 63% ciosów mimo to rani (n=134) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 68% decyzji zapada, gdy do trafienia zostało <0,1 s (n=929) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=34).
- Telegraf „nieparowalny” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=1649).
- Telegraf „nieblokowalny” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=44).
- Telegraf „obszarowy” daje czas: tylko 11% decyzji zapada mniej niż 0,1 s przed trafieniem (n=38).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,7 ms/klatkę (maks. 82 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Wygrałem 11/20. Wyzwanie umiarkowane; po opanowaniu parowania wieża przestaje straszyć.
- Progres między podejściami słabo odczuwalny: 5,0 → 4,5 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie siedzi: 55% udanych na zwykłych ciosach, 0,4 ripost na podejście. To najlepsza część walki.
- Gąbki na obrażenia: Władca Bramy (34,4 trafień, 50,7 s), Kasztelan Potępionych (9,9 trafień, 10,7 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kultysta topieli (1,0), Wodny chochlik (1,0), Kultysta (1,0) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Władca Bramy – średnio 51 s i 34 trafień do zabicia.
- Mój nemezis: Kultysta – 6 z 9 śmierci (najczęściej „Kula ognia”).
- 8/9 razy zginąłem z flaszkami w kieszeni. 0 z tych śmierci to utrata ≥50% HP w 3 s, a 14% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 84% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Rozpłatanie”: 171 obrażeń na użycie.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (9×, śr. piętro 5,9). Inne: Miecz i tarcza + Cięcie wichru 7× (4,9), Dwuręczny + Rozpłatanie 3× (2,0), Miecz i tarcza + Rozpłatanie 1× (2,0).
- Wytrzymałość kończy się często (27% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.

