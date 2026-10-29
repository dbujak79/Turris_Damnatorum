# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 14:11. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (40 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (40 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 40 | 0 (0%) | 5,1 | 5,0 | 4,7 min | 134,5 | 19313 / 1621 | 5,2 | 3% |
| Gracz H&S / souls-like | 40 | 0 (0%) | 5,5 | 6,5 | 4,9 min | 145,0 | 21177 / 1357 | 3,8 | 3% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 2 4 6 8 4 5 4 7 2 4 6 2 4 6 7 6 5 5 7 7 5 7 7 7 7 6 5 4 5 5 7 3 2 5 6 4 5 5 2 7
- Gracz H&S / souls-like: 2 6 7 2 7 4 5 5 2 7 7 6 7 7 6 6 6 2 2 7 7 7 7 7 7 8 2 5 7 7 4 7 2 7 2 7 7 7 6 3

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×5, 3. III. Władca Bramy ×1, 4. IV. Psiarnia Piekieł ×7, 5. V. Psiarnia Piekieł ×10, 6. VI. Pan Sfory ×6, 7. VII. Zatopione Krużganki ×10, 8. VIII. Zatopione Krużganki ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×9; Pan Sfory – „Pan Sfory: Combo” ×6; Ogar otchłani – „Ogar otchłani” ×4; Siarkowy pomiot – „Siarkowy pomiot” ×3; Kolos – „Kolos: Combo” ×3; Taranowiec – „Taranowiec: Combo” ×3. Śmierć ciosem ≥40% maks. HP: 0/40. Zginął mając flaszki: 31/40.
  Ostatnie 3 s przed śmiercią: średnio 33% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/40). Picie: 232 prób, 206 wypitych (11% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×8, 3. III. Władca Bramy ×1, 4. IV. Psiarnia Piekieł ×2, 5. V. Psiarnia Piekieł ×3, 6. VI. Pan Sfory ×6, 7. VII. Zatopione Krużganki ×19, 8. VIII. Zatopione Krużganki ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×11; Pan Sfory – „Pan Sfory: Combo” ×6; Kamienny chochlik – „Pazury” ×3; Taranowiec – „Taranowiec” ×3; Ognisty chochlik – „Pazury” ×2; Ogar otchłani – „Ogar otchłani” ×2. Śmierć ciosem ≥40% maks. HP: 0/40. Zginął mając flaszki: 38/40.
  Ostatnie 3 s przed śmiercią: średnio 39% maks. HP straconych (śmierć z ≥50% HP w 3 s: 7/40). Picie: 181 prób, 153 wypitych (15% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 40 | 40 | 55 s | 30 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 40 | 35 | 68 s | 223 | 0,6 | 38,8 |
| Tester QA | 3. III. Władca Bramy | 35 | 34 | 32 s | 334 | 1,0 | 7,4 |
| Tester QA | 4. IV. Psiarnia Piekieł | 34 | 27 | 64 s | 271 | 0,8 | 40,6 |
| Tester QA | 5. V. Psiarnia Piekieł | 27 | 17 | 71 s | 502 | 1,8 | 33,0 |
| Tester QA | 6. VI. Pan Sfory | 17 | 11 | 42 s | 661 | 3,1 | 5,3 |
| Tester QA | 7. VII. Zatopione Krużganki | 11 | 1 | 71 s | 765 | 1,5 | 20,5 |
| Tester QA | 8. VIII. Zatopione Krużganki | 1 | 0 | 0 s | 648 | 1,0 | 18,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 40 | 40 | 57 s | 30 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 40 | 32 | 65 s | 211 | 0,6 | 36,8 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 32 | 31 | 33 s | 187 | 0,5 | 7,8 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 31 | 29 | 60 s | 151 | 0,3 | 44,8 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 29 | 26 | 63 s | 246 | 0,7 | 40,4 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 26 | 20 | 51 s | 522 | 2,4 | 6,1 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 20 | 1 | 71 s | 631 | 1,1 | 19,5 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 1 | 0 | 0 s | 655 | 0,0 | 9,0 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 1501 | 0,4 s | 1,1 | 1869 | 28003 | 18 | 21 |
| Pan Sfory | 31 | 43,6 s | 30,9 | 1199 | 23687 | 764 | 12 |
| Władca Bramy | 65 | 28,7 s | 20,2 | 677 | 16409 | 252 | 2 |
| Ogar otchłani | 924 | 0,7 s | 1,5 | 975 | 10398 | 11 | 7 |
| Czerw | 3236 | 0,5 s | 1,3 | 1608 | 6154 | 2 | 4 |
| Taranowiec | 283 | 2,4 s | 2,6 | 323 | 4523 | 16 | 9 |
| Siarkowy pomiot | 573 | 0,1 s | 1,1 | 123 | 4393 | 7 | 4 |
| Kolos | 34 | 5,8 s | 4,0 | 81 | 3967 | 117 | 4 |
| Ognisty chochlik | 872 | 0,2 s | 1,1 | 299 | 3564 | 4 | 3 |
| Kultysta topieli | 21 | 0,7 s | 1,6 | 157 | 3355 | 160 | 3 |
| Legionista | 125 | 9,6 s | 5,5 | 365 | 3092 | 24 | 2 |
| Chochlik wichru | 926 | 0,6 s | 1,5 | 264 | 2546 | 3 | 0 |
| Kamienny chochlik | 904 | 0,2 s | 1,1 | 230 | 2480 | 3 | 5 |
| Harpia | 41 | 1,8 s | 2,2 | 185 | 2444 | 58 | 2 |
| Wodny chochlik | 920 | 0,2 s | 1,1 | 263 | 2393 | 3 | 2 |
| Ożywiony nietoperz | 445 | 0,5 s | 1,2 | 236 | 1260 | 3 | 0 |
| Ofiarnik | 8 | 0,5 s | 2,0 | 42 | 232 | 29 | 0 |
| Wiedźma wichru | 3 | 0,4 s | 1,7 | 13 | 217 | 72 | 0 |
| Czerw (elita) | 31 | 4,8 s | 3,4 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 116 | 4,3 s | 4,6 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 56 | 6,4 s | 4,4 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 28 | 1,3 s | 2,1 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 25 | 7,7 s | 5,8 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 3 | 45,2 s | 22,3 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 4 | 16,8 s | 14,3 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 1 | 10,0 s | 11,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2421 | 79% | 2% | 3% | 14% | 2% |
| ciężki | 208 | 60% | 0% | 2% | 33% | 5% |
| nieparowalny | 1423 | 72% | 5% | 0% | 12% | 12% |
| nieblokowalny | 151 | 82% | 0% | 0% | 18% | 0% |
| obszarowy | 321 | 86% | 10% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2290 | 58% | 3% | 9% | 28% | 2% |
| ciężki | 216 | 47% | 1% | 18% | 33% | 1% |
| nieparowalny | 1335 | 61% | 4% | 0% | 26% | 10% |
| nieblokowalny | 193 | 75% | 0% | 0% | 25% | 0% |
| obszarowy | 351 | 76% | 19% | 0% | 0% | 5% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2697 | 27% | 49% | 2% | 12% | 11% |
| Tester QA (0,30 s) | ciężki | 234 | 1% | 0% | 3% | 11% | 84% |
| Tester QA (0,30 s) | nieparowalny | 293 | 6% | 6% | 16% | 26% | 46% |
| Tester QA (0,30 s) | nieblokowalny | 20 | 20% | 0% | 20% | 55% | 5% |
| Tester QA (0,30 s) | obszarowy | 433 | 0% | 2% | 6% | 47% | 45% |
| Tester QA (0,30 s) | pocisk | 1917 | 1% | 10% | 9% | 23% | 57% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2629 | 2% | 31% | 45% | 8% | 15% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 284 | 2% | 0% | 3% | 10% | 85% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 370 | 5% | 1% | 5% | 35% | 54% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 19 | 11% | 0% | 0% | 63% | 26% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 604 | 1% | 0% | 3% | 45% | 51% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2002 | 1% | 11% | 9% | 17% | 63% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 721 |
| Czerw: Ugryzienie | 0,33 s | 2508 |
| Wodny chochlik: Pazury | 0,33 s | 488 |
| Kamienny chochlik: Pazury | 0,33 s | 457 |
| Chochlik wichru: Pazury | 0,33 s | 493 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 449 |
| Ognisty chochlik: Pazury | 0,33 s | 504 |
| Władca Bramy: Cięcie w szale | 0,41 s | 48 |
| Wiedźma wichru: Wiedźma wichru | 0,41 s | 12 |
| Władca Bramy: Piekielne kule | 0,47 s | 10 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 79% (n=123) | – | 60% (n=40) | 100% (n=4) | 25% (n=40) | 91% (n=393) |
| blok (eksperyment) | 83% (n=100) | 78% (n=18) | 89% (n=38) | 100% (n=4) | 67% (n=9) | 73% (n=74) |
| brak reakcji (eksperyment) | 89% (n=123) | 100% (n=15) | 95% (n=20) | 100% (n=2) | 100% (n=16) | 93% (n=73) |
| niezauważony | 89% (n=466) | – | 97% (n=156) | 98% (n=100) | 100% (n=61) | – |
| parowanie | 65% (n=72) | 57% (n=14) | – | – | – | – |
| parowanie (eksperyment) | 67% (n=63) | 0% (n=1) | 100% (n=9) | – | 100% (n=7) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 89% (n=94) |
| unik | 70% (n=575) | 43% (n=81) | 40% (n=110) | 17% (n=24) | – | 77% (n=183) |
| unik (eksperyment) | 78% (n=189) | 44% (n=18) | 63% (n=46) | 50% (n=10) | 100% (n=22) | 73% (n=51) |
| wyjście z zasięgu | 89% (n=554) | 94% (n=52) | 100% (n=74) | 100% (n=4) | 100% (n=152) | 92% (n=13) |
| wyjście z zasięgu (eksperyment) | 85% (n=127) | 67% (n=9) | 100% (n=23) | 100% (n=3) | 100% (n=14) | 95% (n=55) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 26,5 | reakcja na cios wręcz 32,7, unik przed pociskiem 13,7, wyjście z okrążenia 2,2 | brak wytrzymałości 5,4, drgnięcie (trafiony) 5,4, w trakcie uniku 3,0, picie flaszki 2,0, parowanie 0,4, inna 0,4, umiejętność 0,1, riposta 0,1, mocny atak 0,1, blok 0,1, szybki atak 0,0, przełamana garda 0,0 | 47% (n=1306) / 68% (n=549) |
| Gracz H&S / souls-like | 37,5 | reakcja na cios wręcz 31,2, unik przed pociskiem 17,9, doskok do odsłoniętego celu 3,7, wyjście z okrążenia 2,1 | w trakcie uniku 5,0, drgnięcie (trafiony) 3,3, brak wytrzymałości 2,1, picie flaszki 1,5, parowanie 1,4, inna 0,3, riposta 0,1, mocny atak 0,1, umiejętność 0,1, szybki atak 0,0, blok 0,0 | 64% (n=1247) / 76% (n=715) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Rozpłatanie | 10 | 0 | 5,7 | 149,8 |
| Tester QA | Dwuręczny | Cięcie wichru | 10 | 0 | 5,5 | 145,1 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 10 | 0 | 5,2 | 138,7 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 10 | 0 | 4,1 | 104,2 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 30 | 0 | 5,7 | 155,3 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 3 | 0 | 5,3 | 128,0 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 2 | 0 | 4,5 | 112,5 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 5 | 0 | 4,4 | 105,8 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 146355 | 19% |  |  |
| Rozpłatanie | 101519 | 13% | 551 | 184 |
| Cięcie wichru | 87054 | 11% | 543 | 160 |
| Tąpnięcie | 79055 | 10% | 557 | 142 |
| Fala przełamania | 63962 | 8% | 176 | 363 |
| Cięcie | 53977 | 7% |  |  |
| Egzekucja | 43747 | 6% |  |  |
| Upadek kata | 36595 | 5% | 171 | 214 |
| Cięcie krzyżowe | 32509 | 4% | 160 | 203 |
| Okrzyk bojowy | 32310 | 4% | 730 | 44 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×90.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Rozpłatanie | 275478 | 33% | 1551 | 178 |
| Cięcie wichru | 268346 | 32% | 1431 | 188 |
| Szarża | 87261 | 10% | 1029 | 85 |
| Cięcie | 69820 | 8% |  |  |
| Riposta | 28511 | 3% |  |  |
| Potężne cięcie | 27023 | 3% |  |  |
| Okrzyk bojowy | 25083 | 3% | 328 | 76 |
| Zamach | 22943 | 3% |  |  |
| Fala przełamania | 12891 | 2% | 46 | 280 |
| Cięcie krzyżowe | 11700 | 1% | 57 | 205 |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,8, mediana 6,0, najwięcej 8 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 25% | 23,1 | 165 | 0,8 |
| odblokowanie | 15% | 13,9 | 142 | 0,5 |
| klasa | 20% | 18,3 | 44 | 0,1 |
| nagroda | 4% | 3,5 | 91 | 0,1 |
| sklep | 37% | 33,9 | 163 | 0,6 |

Oferty sklepu dusz: widziane 165, kupione 120, odrzucone 0, za mało dusz 45.
Zamiany w slotach: 2,9 na podejście; najczęstsze: Tąpnięcie → Cięcie wichru ×11, Rozpłatanie → Tąpnięcie ×9, Tąpnięcie → Rozpłatanie ×8, Cięcie wichru → Tąpnięcie ×7, Cięcie wichru → Rozpłatanie ×5, Cięcie wichru → Fala przełamania ×4.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 40 | 18,3 | 44 | 0,1 |
| Tąpnięcie | odblokowanie | 40 | 13,9 | 142 | 0,5 |
| Rozpłatanie | startowy | 33 | 16,7 | 184 | 0,8 |
| Cięcie wichru | startowy | 24 | 22,6 | 160 | 0,7 |
| Wypad | sklep | 12 | 18,6 | 30 | 0,5 |
| Fala przełamania | sklep | 11 | 16,0 | 363 | 0,8 |
| Upadek kata | sklep | 12 | 14,3 | 214 | 0,5 |
| Cięcie krzyżowe | sklep | 10 | 16,0 | 203 | 0,6 |
| Odtrącenie | sklep | 9 | 14,1 | 28 | 0,7 |
| Krwawe cięcie | sklep | 10 | 9,5 | 202 | 0,3 |
| Okrzyk wojenny | sklep | 11 | 8,2 | – | 0,9 |
| Cięcie z wyskoku | sklep | 4 | 20,0 | 252 | 0,5 |
| Szarża | sklep | 8 | 9,0 | 100 | 0,6 |
| Kontra | sklep | 5 | 13,2 | 47 | 0,4 |
| Uderzenie tarczą | sklep | 6 | 10,5 | 64 | 0,1 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,0, mediana 4,0, najwięcej 5 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 40% | 44,5 | 193 | 1,4 |
| odblokowanie | 49% | 55,3 | 128 | 1,5 |
| klasa | 7% | 8,2 | 76 | 0,6 |
| nagroda | 1% | 1,1 | 126 | 1,6 |
| sklep | 3% | 3,0 | 225 | 1,2 |

Oferty sklepu dusz: widziane 179, kupione 18, odrzucone 111, za mało dusz 50.
Zamiany w slotach: 0,4 na podejście; najczęstsze: Szarża → Cięcie krzyżowe ×4, Szarża → Krwawe cięcie ×2, Szarża → Fala przełamania ×2, Tąpnięcie → Szarża ×1, Rozpłatanie → Cięcie krzyżowe ×1, Rozpłatanie → Uderzenie tarczą ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | startowy | 42 | 36,9 | 178 | 1,7 |
| Cięcie wichru | startowy | 32 | 44,7 | 188 | 1,4 |
| Szarża | odblokowanie | 36 | 28,6 | 85 | 1,2 |
| Okrzyk bojowy | klasa | 40 | 8,2 | 76 | 0,6 |
| Cięcie krzyżowe | sklep | 5 | 11,4 | 205 | 1,4 |
| Fala przełamania | sklep | 2 | 23,0 | 280 | 1,3 |
| Tąpnięcie | odblokowanie | 2 | 9,0 | 84 | 0,4 |
| Krwawe cięcie | odblokowanie | 1 | 10,0 | 190 | 2,2 |
| Uderzenie tarczą | odblokowanie | 1 | 7,0 | 62 | 1,4 |
| Kontra | odblokowanie | 1 | 6,0 | 130 | 0,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 92,6, szybki atak 41,9, drgnięcie (trafiony) 32,4, unik 26,5, blok 17,7, mocny atak 14,5, przełamana garda 5,8, flaszka (próba) 5,8, parowanie 2,0, riposta 0,5.
Czas w bloku 7 s, w biegu 18 s, bez wytrzymałości (<10%) 99 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Żelazna skóra ×7, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×7, wzmocnienie: Oddech wojownika ×6, wzmocnienie: Pijawka ×5, nowy skill: Rozpłatanie ×5, wzmocnienie: Zwinność ×5, ulepszenie: Ulepszenie: Cięcie wichru → +1 ×4, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×4.
Zakupy: ulepszenie broni ×34, ulepszenie: Rozpłatanie ×32, ulepszenie: Cięcie wichru ×25, ulepszenie: Tąpnięcie ×19, oferta: Upadek kata ×12, oferta: Fala przełamania ×12, oferta: Rozpłatanie ×12, oferta: Okrzyk wojenny ×11.
Punkty cech: Strength 57, Toughness 46, Dexterity 35, Intelligence 27.
Komunikaty gry: „OKRZYK!” ×699; „SZAŁ!” ×368; „GARDA PRZEŁAMANA!” ×242; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×153; „Portale gasną – dobij pozostałe demony” ×136; „Garda wroga przełamana!” ×59; „PAROWANIE!” ×54; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×45.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 112,1, unik 37,5, szybki atak 31,8, drgnięcie (trafiony) 28,6, blok 18,3, mocny atak 11,6, parowanie 6,4, przełamana garda 4,8, flaszka (próba) 4,5, riposta 2,7.
Czas w bloku 8 s, w biegu 19 s, bez wytrzymałości (<10%) 76 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +3 ×12, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×10, ulepszenie: Ulepszenie: Szarża → +2 ×8, ulepszenie: Ulepszenie: Szarża → +3 ×8, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×8, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×8, wzmocnienie: Siła tytana ×7, ulepszenie: Ulepszenie: Cięcie wichru → +2 ×6.
Zakupy: ulepszenie: Rozpłatanie ×74, ulepszenie: Cięcie wichru ×50, ulepszenie: Okrzyk bojowy ×42, ulepszenie: Szarża ×39, ulepszenie broni ×32, dodatkowa flaszka ×9, oferta: Fala przełamania ×8, oferta: Cięcie krzyżowe ×7.
Punkty cech: Toughness 70, Strength 109.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Wypad, Puklerz, Kontra, Sztylet parujący, Łuk refleksyjny, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×301; „PAROWANIE!” ×229; „SZAŁ!” ×209; „GARDA PRZEŁAMANA!” ×196; „Postawa przełamana – RIPOSTA!” ×169; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×161; „Portale gasną – dobij pozostałe demony” ×143; „RIPOSTA!” ×104.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×375, garda nie zatrzymała ciosu ×10, zablokowany ruch ×3.
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 174°)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 134°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; -7,8)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 135°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 179°)
  - Siarkowy pomiot: „Siarkowy pomiot” przeszedł przez gardę (kąt 166°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 121°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-6,9; -1,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,4; 9,1)
  - Ognisty chochlik: „Pazury” przeszedł przez gardę (kąt 144°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 146°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 155°)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×304, parowanie w oknie nie zadziałało ×9, zablokowany ruch ×3, garda nie zatrzymała ciosu ×2.
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 161°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 130°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 157°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 84°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (6,5; 6,9)
  - Ofiarnik: „Ofiarnik” trafił w aktywnym oknie parowania (kąt do źródła 90°)
  - Ofiarnik: „Ofiarnik: BloodRite” trafił w aktywnym oknie parowania (kąt do źródła 87°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 170°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 168°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 176°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,1; 6,9)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,8; 0,4)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 40 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (3 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 10× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta: „Kula ognia” przeszedł przez gardę (kąt 174°)
- Parowanie zwykłych ciosów: 66% ciosów mimo to rani (n=135) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): działa – rani tylko 26% ciosów (n=34).
- Unik przed ciężkim: 43% ciosów mimo to rani (n=99) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 81% ciosów mimo to rani (n=223) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=166) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 32% pocisków, przed którymi był unik (n=549).
- Blok pocisku: 88% ciosów mimo to rani (n=467) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 76% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2697) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=234).
- Telegraf „nieparowalny” daje czas: tylko 13% decyzji zapada mniej niż 0,1 s przed trafieniem (n=293).
- Telegraf „nieblokowalny” daje czas: tylko 20% decyzji zapada mniej niż 0,1 s przed trafieniem (n=20).
- Telegraf „obszarowy” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=433).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Brak broni/tarczy zdolnej do parowania” ×2 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 69 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 40 podejściach. Średnio dochodzę do piętra 5,5 – na razie to ściana, nie krzywa.
- Czuję progres: druga połowa podejść średnio piętro 5,8 wobec 5,2 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 41% udanych (n=497). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (34,5 trafień, 47,3 s), Władca Bramy (22,3 trafień, 29,4 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Kamienny chochlik (1,1), Siarkowy pomiot (1,1), Ognisty chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Pan Sfory – średnio 47 s i 34 trafień do zabicia.
- 38/40 razy zginąłem z flaszkami w kieszeni. 7 z tych śmierci to utrata ≥50% HP w 3 s, a 15% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 81% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 280 obrażeń na użycie.
- Skill startowy to 40% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 4%. Różnych skilli na podejście: średnio 4,0.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (30×, śr. piętro 5,7). Inne: Dwuręczny + Rozpłatanie 5× (4,4), Miecz i tarcza + Rozpłatanie 3× (5,3), Dwuręczny + Cięcie wichru 2× (4,5).
- Unik to moje główne narzędzie: 37,5 na podejście, w tym 5,8 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 76% przypadków.
- Gra nie wykonała uniku od razu średnio 13,7× na podejście – najczęściej: w trakcie poprzedniego uniku 5,0, drgnięcie (trafiony) 3,3, brak wytrzymałości 2,1. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (26% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (29 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

