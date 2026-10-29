# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 13:53. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (30 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (30 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 30 | 0 (0%) | 5,2 | 5,0 | 4,6 min | 145,2 | 19400 / 1486 | 4,6 | 2% |
| Gracz H&S / souls-like | 30 | 0 (0%) | 5,8 | 7,0 | 5,1 min | 157,6 | 23586 / 1435 | 3,8 | 3% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 5 5 5 5 6 2 7 7 5 6 5 5 5 6 7 4 5 6 7 5 6 6 2 5 4 5 5 5 6 5
- Gracz H&S / souls-like: 8 5 2 7 7 7 7 7 8 5 7 7 8 5 8 7 5 6 7 2 2 2 6 5 4 7 7 7 7 2

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×2, 4. IV. Psiarnia Piekieł ×2, 5. V. Psiarnia Piekieł ×15, 6. VI. Pan Sfory ×7, 7. VII. Zatopione Krużganki ×4.
  Ostatni cios: Kultysta – „Kula ognia” ×6; Ogar otchłani – „Ogar otchłani” ×4; Pan Sfory – „Pan Sfory: Combo” ×3; Wodny chochlik – „Pazury” ×3; Pan Sfory – „Pan Sfory: InfernalCharge” ×2; Taranowiec – „Taranowiec: Combo” ×2. Śmierć ciosem ≥40% maks. HP: 0/30. Zginął mając flaszki: 28/30.
  Ostatnie 3 s przed śmiercią: średnio 33% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/30). Picie: 169 prób, 139 wypitych (18% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×5, 4. IV. Psiarnia Piekieł ×1, 5. V. Psiarnia Piekieł ×5, 6. VI. Pan Sfory ×2, 7. VII. Zatopione Krużganki ×13, 8. VIII. Zatopione Krużganki ×4.
  Ostatni cios: Kultysta – „Kula ognia” ×7; Kolos – „Kolos: Combo” ×4; Chochlik wichru – „Pazury” ×3; Czerw – „Ugryzienie” ×3; Siarkowy pomiot – „Siarkowy pomiot” ×2; Kamienny chochlik – „Pazury” ×2. Śmierć ciosem ≥40% maks. HP: 0/30. Zginął mając flaszki: 30/30.
  Ostatnie 3 s przed śmiercią: średnio 35% maks. HP straconych (śmierć z ≥50% HP w 3 s: 5/30). Picie: 138 prób, 113 wypitych (18% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 30 | 30 | 55 s | 26 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 30 | 28 | 63 s | 179 | 0,4 | 41,1 |
| Tester QA | 3. III. Władca Bramy | 28 | 28 | 33 s | 343 | 1,3 | 8,3 |
| Tester QA | 4. IV. Psiarnia Piekieł | 28 | 26 | 62 s | 241 | 0,8 | 43,5 |
| Tester QA | 5. V. Psiarnia Piekieł | 26 | 11 | 66 s | 491 | 1,3 | 32,7 |
| Tester QA | 6. VI. Pan Sfory | 11 | 4 | 39 s | 657 | 2,7 | 3,5 |
| Tester QA | 7. VII. Zatopione Krużganki | 4 | 0 | 0 s | 517 | 0,8 | 15,5 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 30 | 30 | 56 s | 21 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 30 | 25 | 64 s | 195 | 0,3 | 38,0 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 25 | 25 | 28 s | 166 | 0,4 | 7,9 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 25 | 24 | 62 s | 158 | 0,4 | 45,4 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 24 | 19 | 62 s | 289 | 0,8 | 37,3 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 19 | 17 | 41 s | 439 | 2,0 | 6,1 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 17 | 4 | 95 s | 588 | 1,2 | 26,1 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 4 | 0 | 0 s | 796 | 1,3 | 21,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 1131 | 0,6 s | 1,1 | 1379 | 20681 | 17 | 13 |
| Pan Sfory | 21 | 36,1 s | 26,3 | 755 | 14639 | 697 | 8 |
| Władca Bramy | 53 | 26,7 s | 17,7 | 484 | 12443 | 235 | 0 |
| Ogar otchłani | 747 | 0,6 s | 1,5 | 807 | 8991 | 12 | 5 |
| Czerw | 2737 | 0,4 s | 1,3 | 1180 | 4495 | 2 | 4 |
| Taranowiec | 205 | 2,5 s | 2,4 | 245 | 3628 | 17 | 4 |
| Siarkowy pomiot | 536 | 0,1 s | 1,1 | 110 | 3477 | 6 | 4 |
| Kultysta topieli | 32 | 5,7 s | 1,8 | 166 | 3134 | 95 | 2 |
| Ognisty chochlik | 719 | 0,1 s | 1,1 | 237 | 2486 | 3 | 3 |
| Kamienny chochlik | 718 | 0,2 s | 1,1 | 213 | 2340 | 3 | 3 |
| Kolos | 35 | 5,3 s | 3,5 | 47 | 2256 | 63 | 4 |
| Legionista | 112 | 7,2 s | 4,8 | 267 | 2042 | 18 | 1 |
| Wodny chochlik | 672 | 0,2 s | 1,1 | 202 | 1967 | 3 | 4 |
| Chochlik wichru | 727 | 0,4 s | 1,3 | 181 | 1695 | 2 | 4 |
| Harpia | 32 | 1,4 s | 1,8 | 127 | 1554 | 47 | 1 |
| Ożywiony nietoperz | 377 | 0,6 s | 1,3 | 257 | 1311 | 3 | 0 |
| Wiedźma wichru | 8 | 0,2 s | 1,5 | 46 | 441 | 55 | 0 |
| Ofiarnik | 6 | 0,3 s | 1,3 | 9 | 60 | 10 | 0 |
| Ognisty chochlik (elita) | 88 | 3,6 s | 4,2 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 53 | 5,5 s | 4,0 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 29 | 2,3 s | 2,9 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 15 | 1,4 s | 2,1 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 22 | 4,9 s | 5,0 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 5 | 16,5 s | 12,8 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 1 | 16,8 s | 7,0 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 1 | 55,6 s | 21,0 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 1 | 53,0 s | 36,0 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 1 | 5,6 s | 6,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1835 | 79% | 3% | 2% | 13% | 3% |
| ciężki | 119 | 66% | 2% | 6% | 25% | 2% |
| nieparowalny | 911 | 72% | 6% | 0% | 10% | 12% |
| nieblokowalny | 97 | 89% | 0% | 0% | 11% | 0% |
| obszarowy | 224 | 83% | 13% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1731 | 58% | 3% | 10% | 28% | 2% |
| ciężki | 164 | 36% | 7% | 14% | 43% | 0% |
| nieparowalny | 1057 | 62% | 6% | 0% | 24% | 8% |
| nieblokowalny | 175 | 75% | 0% | 0% | 25% | 0% |
| obszarowy | 399 | 79% | 17% | 0% | 0% | 4% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2024 | 30% | 49% | 2% | 9% | 10% |
| Tester QA (0,30 s) | ciężki | 150 | 1% | 1% | 2% | 5% | 91% |
| Tester QA (0,30 s) | nieparowalny | 187 | 6% | 9% | 12% | 21% | 51% |
| Tester QA (0,30 s) | nieblokowalny | 12 | 0% | 0% | 8% | 75% | 17% |
| Tester QA (0,30 s) | obszarowy | 326 | 0% | 0% | 7% | 64% | 29% |
| Tester QA (0,30 s) | pocisk | 1268 | 1% | 11% | 11% | 19% | 59% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2005 | 2% | 29% | 45% | 7% | 17% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 196 | 1% | 2% | 1% | 6% | 91% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 254 | 6% | 3% | 8% | 27% | 56% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 16 | 19% | 0% | 0% | 44% | 38% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 523 | 1% | 1% | 3% | 47% | 49% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 1711 | 1% | 12% | 11% | 19% | 58% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 521 |
| Chochlik wichru: Pazury | 0,33 s | 392 |
| Ognisty chochlik: Pazury | 0,33 s | 393 |
| Czerw: Ugryzienie | 0,33 s | 1859 |
| Kamienny chochlik: Pazury | 0,33 s | 397 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 422 |
| Wodny chochlik: Pazury | 0,33 s | 384 |
| Władca Bramy: Cięcie w szale | 0,41 s | 38 |
| Władca Bramy: Piekielne kule | 0,44 s | 5 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 14 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 64% (n=102) | 0% (n=2) | 65% (n=46) | 100% (n=6) | 19% (n=31) | 90% (n=272) |
| blok (eksperyment) | 88% (n=92) | 100% (n=2) | 92% (n=13) | – | 75% (n=12) | 62% (n=53) |
| brak reakcji (eksperyment) | 90% (n=97) | 85% (n=13) | 100% (n=9) | – | 100% (n=16) | 100% (n=44) |
| niezauważony | 87% (n=390) | – | 100% (n=104) | 100% (n=57) | 100% (n=19) | – |
| parowanie | 65% (n=37) | 43% (n=7) | – | – | – | – |
| parowanie (eksperyment) | 84% (n=51) | 50% (n=4) | 100% (n=10) | 100% (n=3) | 100% (n=4) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 91% (n=46) |
| unik | 68% (n=389) | 53% (n=53) | 40% (n=67) | 50% (n=20) | – | 78% (n=113) |
| unik (eksperyment) | 71% (n=147) | 57% (n=7) | 50% (n=16) | 0% (n=1) | 100% (n=23) | 89% (n=38) |
| wyjście z zasięgu | 94% (n=437) | 96% (n=24) | 98% (n=48) | 100% (n=9) | 100% (n=114) | 100% (n=1) |
| wyjście z zasięgu (eksperyment) | 92% (n=91) | 100% (n=7) | 100% (n=10) | 100% (n=1) | 100% (n=5) | 100% (n=23) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 23,8 | reakcja na cios wręcz 29,1, unik przed pociskiem 11,4, wyjście z okrążenia 2,5 | brak wytrzymałości 5,2, drgnięcie (trafiony) 4,9, w trakcie uniku 2,5, picie flaszki 2,2, parowanie 0,3, blok 0,1, inna 0,1, umiejętność 0,1, mocny atak 0,1, riposta 0,1, przełamana garda 0,0 | 46% (n=874) / 64% (n=343) |
| Gracz H&S / souls-like | 37,7 | reakcja na cios wręcz 32,7, unik przed pociskiem 22,4, doskok do odsłoniętego celu 3,5, wyjście z okrążenia 2,1 | w trakcie uniku 6,8, drgnięcie (trafiony) 4,5, picie flaszki 1,7, brak wytrzymałości 1,6, parowanie 1,6, inna 0,3, umiejętność 0,1, mocny atak 0,1, riposta 0,1, szybki atak 0,1, blok 0,0 | 62% (n=980) / 74% (n=673) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Rozpłatanie | 7 | 0 | 5,4 | 149,9 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 8 | 0 | 5,3 | 139,5 |
| Tester QA | Dwuręczny | Cięcie wichru | 7 | 0 | 5,1 | 145,9 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 8 | 0 | 5,1 | 146,1 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 20 | 0 | 6,1 | 165,3 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 2 | 0 | 6,0 | 162,5 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 5 | 0 | 5,8 | 159,6 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 3 | 0 | 3,7 | 100,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 103481 | 18% |  |  |
| Cięcie wichru | 84346 | 14% | 458 | 184 |
| Rozpłatanie | 73568 | 13% | 435 | 169 |
| Tąpnięcie | 64553 | 11% | 440 | 147 |
| Cięcie | 48069 | 8% |  |  |
| Fala przełamania | 34496 | 6% | 99 | 348 |
| Cięcie z wyskoku | 27795 | 5% | 139 | 200 |
| Egzekucja | 26721 | 5% |  |  |
| Potężne cięcie | 22713 | 4% |  |  |
| Okrzyk bojowy | 22692 | 4% | 528 | 43 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×62.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Rozpłatanie | 306366 | 43% | 1714 | 179 |
| Cięcie wichru | 115059 | 16% | 562 | 205 |
| Fala przełamania | 71674 | 10% | 242 | 296 |
| Szarża | 50655 | 7% | 522 | 97 |
| Cięcie | 43629 | 6% |  |  |
| Zamach | 32377 | 5% |  |  |
| Okrzyk bojowy | 23085 | 3% | 275 | 84 |
| Potężne cięcie | 21316 | 3% |  |  |
| Riposta | 20547 | 3% |  |  |
| Egzekucja | 9839 | 1% |  |  |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,8, mediana 6,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 27% | 25,5 | 178 | 0,9 |
| odblokowanie | 16% | 14,7 | 147 | 0,5 |
| klasa | 19% | 17,6 | 43 | 0,0 |
| nagroda | 3% | 3,0 | 113 | 0,4 |
| sklep | 35% | 32,5 | 145 | 0,6 |

Oferty sklepu dusz: widziane 127, kupione 96, odrzucone 0, za mało dusz 31.
Zamiany w slotach: 2,9 na podejście; najczęstsze: Tąpnięcie → Rozpłatanie ×8, Tąpnięcie → Cięcie wichru ×8, Cięcie wichru → Tąpnięcie ×7, Tąpnięcie → Upadek kata ×4, Rozpłatanie → Cięcie z wyskoku ×4, Rozpłatanie → Tąpnięcie ×4.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 30 | 17,6 | 43 | 0,0 |
| Cięcie wichru | startowy | 19 | 24,1 | 184 | 0,8 |
| Tąpnięcie | odblokowanie | 30 | 14,7 | 147 | 0,5 |
| Rozpłatanie | startowy | 20 | 21,8 | 169 | 0,9 |
| Cięcie z wyskoku | sklep | 9 | 15,4 | 200 | 0,7 |
| Odtrącenie | sklep | 7 | 16,0 | 17 | 0,6 |
| Wypad | sklep | 7 | 15,7 | 21 | 0,6 |
| Krwawe cięcie | sklep | 11 | 9,4 | 185 | 0,7 |
| Fala przełamania | sklep | 8 | 12,4 | 348 | 0,5 |
| Cięcie krzyżowe | sklep | 8 | 11,0 | 194 | 0,2 |
| Upadek kata | sklep | 6 | 12,0 | 200 | 0,3 |
| Szarża | sklep | 4 | 16,8 | 131 | 0,8 |
| Okrzyk wojenny | sklep | 9 | 6,9 | – | 0,5 |
| Uderzenie tarczą | sklep | 4 | 14,5 | 59 | 1,0 |
| Kontra | nagroda | 3 | 9,0 | 13 | 0,5 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,0, mediana 4,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 53% | 60,2 | 176 | 1,6 |
| odblokowanie | 23% | 25,7 | 141 | 1,2 |
| klasa | 8% | 9,2 | 84 | 0,6 |
| nagroda | 2% | 2,5 | 234 | 1,5 |
| sklep | 13% | 14,9 | 238 | 1,2 |

Oferty sklepu dusz: widziane 144, kupione 17, odrzucone 81, za mało dusz 46.
Zamiany w slotach: 0,9 na podejście; najczęstsze: Szarża → Fala przełamania ×8, Szarża → Cięcie wichru ×7, Szarża → Krwawe cięcie ×3, Rozpłatanie → Cięcie wichru ×2, Tąpnięcie → Fala przełamania ×1, Rozpłatanie → Uderzenie tarczą ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | startowy | 31 | 55,3 | 179 | 1,6 |
| Cięcie wichru | startowy | 16 | 35,1 | 205 | 1,4 |
| Szarża | odblokowanie | 27 | 19,3 | 97 | 0,9 |
| Okrzyk bojowy | klasa | 29 | 9,5 | 84 | 0,6 |
| Fala przełamania | sklep | 10 | 24,2 | 296 | 1,3 |
| Krwawe cięcie | odblokowanie | 4 | 6,3 | 175 | 2,0 |
| Tąpnięcie | odblokowanie | 1 | 11,0 | 97 | 0,5 |
| Kontra | odblokowanie | 1 | 11,0 | 91 | 0,0 |
| Cięcie krzyżowe | sklep | 1 | 7,0 | 107 | 0,0 |
| Uderzenie tarczą | odblokowanie | 1 | 5,0 | 42 | 0,4 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 93,3, szybki atak 41,7, drgnięcie (trafiony) 30,8, unik 23,8, blok 17,0, mocny atak 14,5, flaszka (próba) 5,6, przełamana garda 5,4, parowanie 2,0, riposta 0,5.
Czas w bloku 7 s, w biegu 18 s, bez wytrzymałości (<10%) 99 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Tąpnięcie → +1 ×8, wzmocnienie: Obrońca ×7, wzmocnienie: Czujność ×5, wzmocnienie: Ostrze gniewu ×4, wzmocnienie: Mistrz parowania ×4, wzmocnienie: Siła tytana ×3, nowy skill: Wypad ×3, wzmocnienie: Oddech wojownika ×3.
Zakupy: ulepszenie broni ×28, ulepszenie: Cięcie wichru ×24, ulepszenie: Rozpłatanie ×22, ulepszenie: Tąpnięcie ×15, oferta: Krwawe cięcie ×10, oferta: Rozpłatanie ×10, oferta: Cięcie z wyskoku ×10, oferta: Okrzyk wojenny ×8.
Punkty cech: Strength 41, Toughness 32, Dexterity 28, Intelligence 26.
Komunikaty gry: „OKRZYK!” ×500; „SZAŁ!” ×264; „GARDA PRZEŁAMANA!” ×165; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×118; „Portale gasną – dobij pozostałe demony” ×106; „PAROWANIE!” ×45; „Garda wroga przełamana!” ×38; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×32.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 112,5, unik 37,7, drgnięcie (trafiony) 31,7, szybki atak 29,0, blok 20,5, mocny atak 13,9, parowanie 7,2, flaszka (próba) 4,6, przełamana garda 4,2, riposta 2,9.
Czas w bloku 10 s, w biegu 22 s, bez wytrzymałości (<10%) 72 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +2 ×10, ulepszenie: Ulepszenie: Rozpłatanie → +3 ×10, wzmocnienie: Oddech wojownika ×8, ulepszenie: Ulepszenie: Szarża → +1 ×8, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×7, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×6, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×5, wzmocnienie: Siła tytana ×4.
Zakupy: ulepszenie: Rozpłatanie ×85, ulepszenie broni ×28, ulepszenie: Okrzyk bojowy ×26, ulepszenie: Cięcie wichru ×21, ulepszenie: Szarża ×19, ulepszenie: Fala przełamania ×10, dodatkowa flaszka ×9, oferta: Fala przełamania ×8.
Punkty cech: Toughness 58, Strength 86.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Puklerz, Kontra, Sztylet parujący, Łuk refleksyjny, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×263; „PAROWANIE!” ×182; „SZAŁ!” ×160; „Postawa przełamana – RIPOSTA!” ×141; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×130; „GARDA PRZEŁAMANA!” ×128; „Portale gasną – dobij pozostałe demony” ×114; „RIPOSTA!” ×81.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×226, zablokowany ruch ×3, garda nie zatrzymała ciosu ×1.
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-5,6; -0,3)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 162°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; 8,0)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 0,0)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×283, parowanie w oknie nie zadziałało ×11, garda nie zatrzymała ciosu ×4, zablokowany ruch ×2.
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 125°) (w 2 podejściach)
  - Chochlik wichru: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 119°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 168°)
  - Kultysta topieli: „Kultysta topieli” przeszedł przez gardę (kąt 139°)
  - Kultysta topieli: „Kultysta topieli” przeszedł przez gardę (kąt 150°)
  - Ofiarnik: „Ofiarnik: BloodRite” trafił w aktywnym oknie parowania (kąt do źródła 101°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 174°)
  - Chochlik wichru: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 173°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 160°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 156°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (6,2; 1,1)
  - Legionista: „Legionista” trafił w aktywnym oknie parowania (kąt do źródła 158°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 30 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (3 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 1× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta: „Kula ognia” przeszedł przez gardę (kąt 162°)
- Parowanie zwykłych ciosów: 76% ciosów mimo to rani (n=88) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 48% ciosów mimo to rani (n=21) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: 53% ciosów mimo to rani (n=60) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 75% ciosów mimo to rani (n=194) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=119) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 36% pocisków, przed którymi był unik (n=343).
- Blok pocisku: 85% ciosów mimo to rani (n=325) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 79% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2024) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=150).
- Telegraf „nieparowalny” daje czas: tylko 15% decyzji zapada mniej niż 0,1 s przed trafieniem (n=187).
- Telegraf „nieblokowalny” daje czas: tylko 0% decyzji zapada mniej niż 0,1 s przed trafieniem (n=12).
- Telegraf „obszarowy” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=326).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,7 ms/klatkę (maks. 76 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 30 podejściach. Średnio dochodzę do piętra 5,8 – na razie to ściana, nie krzywa.
- Progres między podejściami słabo odczuwalny: 6,5 → 5,1 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 44% udanych (n=350). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (27,4 trafień, 36,5 s), Władca Bramy (16,6 trafień, 23,9 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kamienny chochlik (1,1), Wodny chochlik (1,1), Ognisty chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Kolos (elita) – średnio 56 s i 21 trafień do zabicia.
- Długie starcie: Legionista (elita) – średnio 53 s i 36 trafień do zabicia.
- 30/30 razy zginąłem z flaszkami w kieszeni. 5 z tych śmierci to utrata ≥50% HP w 3 s, a 18% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 81% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 296 obrażeń na użycie.
- Skill startowy to 53% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 15%. Różnych skilli na podejście: średnio 4,0.
- Najczęściej grałem: Miecz i tarcza + Rozpłatanie (20×, śr. piętro 6,1). Inne: Dwuręczny + Cięcie wichru 5× (5,8), Dwuręczny + Rozpłatanie 3× (3,7), Miecz i tarcza + Cięcie wichru 2× (6,0).
- Unik to moje główne narzędzie: 37,7 na podejście, w tym 5,6 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 74% przypadków.
- Gra nie wykonała uniku od razu średnio 16,9× na podejście – najczęściej: w trakcie poprzedniego uniku 6,8, drgnięcie (trafiony) 4,5, picie flaszki 1,7. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (23% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (32 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

