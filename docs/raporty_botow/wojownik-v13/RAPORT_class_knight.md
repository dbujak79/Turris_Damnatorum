# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 14:26. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (40 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (40 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 40 | 0 (0%) | 4,9 | 5,0 | 4,4 min | 126,5 | 18172 / 1399 | 3,9 | 3% |
| Gracz H&S / souls-like | 40 | 0 (0%) | 6,2 | 7,0 | 5,8 min | 160,3 | 24412 / 1812 | 5,5 | 3% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 2 6 5 5 6 5 7 2 6 4 4 7 5 5 2 ⏱7 3 6 4 7 5 4 7 6 2 5 5 4 5 6 2 6 6 7 2 6 4 5 5 7
- Gracz H&S / souls-like: 5 7 7 2 7 7 7 2 7 4 7 8 7 7 7 7 7 6 7 7 7 6 7 5 7 7 8 ⏱7 7 7 5 7 2 6 7 5 8 7 5 2

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×6, 3. III. Władca Bramy ×1, 4. IV. Psiarnia Piekieł ×6, 5. V. Psiarnia Piekieł ×11, 6. VI. Pan Sfory ×9, 7. VII. Zatopione Krużganki ×6.
  Ostatni cios: Pan Sfory – „Pan Sfory: Combo” ×7; Kultysta – „Kula ognia” ×6; Czerw – „Ugryzienie” ×4; Taranowiec – „Taranowiec” ×3; Ogar otchłani – „Ogar otchłani” ×3; Taranowiec – „Taranowiec: Combo” ×3. Śmierć ciosem ≥40% maks. HP: 0/39. Zginął mając flaszki: 35/39.
  Ostatnie 3 s przed śmiercią: średnio 36% maks. HP straconych (śmierć z ≥50% HP w 3 s: 7/39). Picie: 180 prób, 156 wypitych (13% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×4, 4. IV. Psiarnia Piekieł ×1, 5. V. Psiarnia Piekieł ×5, 6. VI. Pan Sfory ×3, 7. VII. Zatopione Krużganki ×23, 8. VIII. Zatopione Krużganki ×3.
  Ostatni cios: Kultysta – „Kula ognia” ×6; Kolos – „Kolos: Combo” ×4; Czerw – „Ugryzienie” ×4; Ogar otchłani – „Ogar otchłani” ×3; Pan Sfory – „Pan Sfory: Combo” ×3; Harpia – „Harpia” ×3. Śmierć ciosem ≥40% maks. HP: 0/39. Zginął mając flaszki: 36/39.
  Ostatnie 3 s przed śmiercią: średnio 39% maks. HP straconych (śmierć z ≥50% HP w 3 s: 7/39). Picie: 248 prób, 219 wypitych (12% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 40 | 40 | 56 s | 25 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 40 | 34 | 64 s | 257 | 0,9 | 38,3 |
| Tester QA | 3. III. Władca Bramy | 34 | 33 | 31 s | 274 | 0,7 | 8,0 |
| Tester QA | 4. IV. Psiarnia Piekieł | 33 | 27 | 64 s | 274 | 0,7 | 41,1 |
| Tester QA | 5. V. Psiarnia Piekieł | 27 | 16 | 66 s | 414 | 1,0 | 33,8 |
| Tester QA | 6. VI. Pan Sfory | 16 | 7 | 40 s | 617 | 2,3 | 4,1 |
| Tester QA | 7. VII. Zatopione Krużganki | 7 | 0 | 0 s | 749 | 1,4 | 23,7 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 40 | 40 | 55 s | 27 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 40 | 36 | 66 s | 193 | 0,5 | 39,0 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 36 | 36 | 34 s | 237 | 0,7 | 7,8 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 36 | 35 | 61 s | 152 | 0,4 | 45,3 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 35 | 30 | 66 s | 340 | 0,9 | 37,2 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 30 | 27 | 57 s | 573 | 2,9 | 7,6 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 27 | 3 | 71 s | 700 | 1,5 | 21,8 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 3 | 0 | 0 s | 578 | 0,7 | 9,7 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 1527 | 0,3 s | 1,1 | 1947 | 30241 | 19 | 12 |
| Pan Sfory | 34 | 49,1 s | 34,4 | 1278 | 25155 | 740 | 12 |
| Władca Bramy | 69 | 28,7 s | 21,0 | 672 | 16644 | 241 | 1 |
| Ogar otchłani | 992 | 0,7 s | 1,5 | 1049 | 10655 | 10 | 8 |
| Taranowiec | 293 | 2,5 s | 2,7 | 434 | 7349 | 24 | 7 |
| Czerw | 3424 | 0,5 s | 1,2 | 1785 | 6809 | 2 | 8 |
| Siarkowy pomiot | 640 | 0,1 s | 1,1 | 136 | 4393 | 7 | 3 |
| Kultysta topieli | 39 | 0,4 s | 1,7 | 204 | 4321 | 108 | 3 |
| Harpia | 54 | 2,3 s | 2,4 | 282 | 4128 | 75 | 3 |
| Ognisty chochlik | 940 | 0,2 s | 1,1 | 335 | 3359 | 3 | 4 |
| Legionista | 149 | 7,5 s | 5,6 | 359 | 3225 | 21 | 4 |
| Kolos | 61 | 6,4 s | 3,9 | 61 | 2726 | 45 | 6 |
| Kamienny chochlik | 958 | 0,2 s | 1,1 | 247 | 2687 | 3 | 2 |
| Chochlik wichru | 928 | 0,6 s | 1,5 | 258 | 2585 | 3 | 2 |
| Wodny chochlik | 956 | 0,2 s | 1,1 | 253 | 2341 | 2 | 1 |
| Ożywiony nietoperz | 485 | 0,4 s | 1,2 | 246 | 1198 | 2 | 0 |
| Wiedźma wichru | 3 | 0,8 s | 2,3 | 49 | 494 | 165 | 2 |
| Ofiarnik | 11 | 1,5 s | 2,0 | 28 | 122 | 11 | 0 |
| Ognisty chochlik (elita) | 118 | 4,5 s | 4,8 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 29 | 3,7 s | 3,4 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 63 | 6,1 s | 4,3 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 26 | 6,9 s | 5,9 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 30 | 1,3 s | 2,4 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 1 | 2,8 s | 4,0 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 8 | 26,7 s | 16,5 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 3 | 44,0 s | 31,7 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 1 | 10,7 s | 4,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2138 | 81% | 3% | 2% | 12% | 2% |
| ciężki | 149 | 64% | 3% | 4% | 26% | 4% |
| nieparowalny | 1323 | 73% | 6% | 0% | 9% | 12% |
| nieblokowalny | 75 | 95% | 0% | 0% | 5% | 0% |
| obszarowy | 274 | 81% | 12% | 0% | 0% | 7% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 2922 | 59% | 3% | 9% | 28% | 2% |
| ciężki | 311 | 52% | 2% | 15% | 31% | 0% |
| nieparowalny | 1723 | 61% | 6% | 0% | 25% | 8% |
| nieblokowalny | 277 | 70% | 0% | 0% | 30% | 0% |
| obszarowy | 431 | 82% | 15% | 0% | 0% | 3% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 2344 | 29% | 52% | 2% | 9% | 8% |
| Tester QA (0,30 s) | ciężki | 188 | 3% | 1% | 1% | 8% | 88% |
| Tester QA (0,30 s) | nieparowalny | 235 | 6% | 6% | 14% | 35% | 39% |
| Tester QA (0,30 s) | nieblokowalny | 14 | 7% | 7% | 14% | 64% | 7% |
| Tester QA (0,30 s) | obszarowy | 343 | 0% | 1% | 5% | 60% | 35% |
| Tester QA (0,30 s) | pocisk | 1774 | 1% | 11% | 9% | 22% | 58% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 3430 | 2% | 31% | 43% | 7% | 17% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 343 | 1% | 2% | 1% | 8% | 87% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 525 | 8% | 2% | 3% | 34% | 54% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 22 | 5% | 0% | 5% | 36% | 55% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 691 | 1% | 1% | 3% | 43% | 51% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 2276 | 1% | 11% | 10% | 16% | 62% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 803 |
| Kamienny chochlik: Pazury | 0,33 s | 492 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 423 |
| Czerw: Ugryzienie | 0,33 s | 2782 |
| Wodny chochlik: Pazury | 0,33 s | 487 |
| Ognisty chochlik: Pazury | 0,33 s | 574 |
| Chochlik wichru: Pazury | 0,33 s | 494 |
| Władca Bramy: Cięcie w szale | 0,41 s | 44 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 21 |
| Władca Bramy: Piekielne kule | 0,46 s | 14 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 71% (n=136) | – | 69% (n=32) | 100% (n=10) | 22% (n=32) | 91% (n=397) |
| blok (eksperyment) | 90% (n=101) | 70% (n=10) | 87% (n=15) | – | 59% (n=17) | 68% (n=87) |
| brak reakcji (eksperyment) | 92% (n=104) | 100% (n=6) | 100% (n=19) | – | 92% (n=13) | 95% (n=75) |
| niezauważony | 92% (n=405) | – | 98% (n=142) | 100% (n=49) | 100% (n=58) | – |
| parowanie | 65% (n=37) | 67% (n=9) | – | – | – | – |
| parowanie (eksperyment) | 67% (n=49) | 25% (n=4) | 100% (n=14) | – | 100% (n=6) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 83% (n=65) |
| unik | 67% (n=431) | 50% (n=64) | 52% (n=98) | 57% (n=7) | – | 81% (n=154) |
| unik (eksperyment) | 75% (n=150) | 65% (n=17) | 71% (n=35) | 0% (n=1) | 100% (n=19) | 84% (n=61) |
| wyjście z zasięgu | 94% (n=604) | 100% (n=35) | 93% (n=87) | 100% (n=8) | 100% (n=112) | – |
| wyjście z zasięgu (eksperyment) | 92% (n=98) | 75% (n=4) | 100% (n=13) | – | 100% (n=15) | 96% (n=54) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 21,2 | reakcja na cios wręcz 25,1, unik przed pociskiem 11,3, wyjście z okrążenia 1,6 | brak wytrzymałości 5,0, drgnięcie (trafiony) 4,0, w trakcie uniku 2,4, picie flaszki 1,5, inna 0,4, mocny atak 0,2, riposta 0,1, umiejętność 0,1, parowanie 0,1, szybki atak 0,1 | 46% (n=1004) / 61% (n=450) |
| Gracz H&S / souls-like | 44,4 | reakcja na cios wręcz 40,2, unik przed pociskiem 21,2, doskok do odsłoniętego celu 2,8, wyjście z okrążenia 2,5 | w trakcie uniku 6,1, drgnięcie (trafiony) 3,9, brak wytrzymałości 2,8, parowanie 2,2, picie flaszki 1,8, riposta 0,3, inna 0,2, umiejętność 0,1, mocny atak 0,1, blok 0,1, szybki atak 0,0 | 64% (n=1607) / 72% (n=848) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 10 | 0 | 5,7 | 131,6 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 10 | 0 | 5,3 | 143,1 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 10 | 0 | 4,4 | 114,5 |
| Tester QA | Dwuręczny | Rozpłatanie | 10 | 0 | 4,3 | 116,6 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 34 | 0 | 6,3 | 162,4 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 3 | 0 | 6,0 | 168,0 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 1 | 0 | 5,0 | 135,0 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 2 | 0 | 5,0 | 125,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 125412 | 17% |  |  |
| Rozpłatanie | 100889 | 14% | 578 | 175 |
| Cięcie wichru | 97940 | 13% | 584 | 168 |
| Tąpnięcie | 84227 | 12% | 587 | 143 |
| Cięcie | 54209 | 7% |  |  |
| Egzekucja | 41588 | 6% |  |  |
| Fala przełamania | 40200 | 6% | 99 | 406 |
| Upadek kata | 30316 | 4% | 101 | 300 |
| Cięcie z wyskoku | 28324 | 4% | 139 | 204 |
| Okrzyk bojowy | 27225 | 4% | 669 | 41 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×86.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 341013 | 35% | 1780 | 192 |
| Rozpłatanie | 252297 | 26% | 1545 | 163 |
| Szarża | 124941 | 13% | 1354 | 92 |
| Cięcie | 83881 | 9% |  |  |
| Riposta | 38697 | 4% |  |  |
| Potężne cięcie | 30939 | 3% |  |  |
| Zamach | 30318 | 3% |  |  |
| Okrzyk bojowy | 29538 | 3% | 401 | 74 |
| Krwawe cięcie | 16555 | 2% | 72 | 230 |
| Cięcie krzyżowe | 11202 | 1% | 58 | 193 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Kontra ×9.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,5, mediana 5,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 28% | 24,2 | 169 | 0,9 |
| odblokowanie | 17% | 14,7 | 143 | 0,4 |
| klasa | 19% | 16,7 | 41 | 0,0 |
| nagroda | 5% | 4,6 | 125 | 0,4 |
| sklep | 31% | 26,9 | 165 | 0,6 |

Oferty sklepu dusz: widziane 157, kupione 116, odrzucone 0, za mało dusz 41.
Zamiany w slotach: 2,7 na podejście; najczęstsze: Tąpnięcie → Rozpłatanie ×7, Tąpnięcie → Cięcie wichru ×5, Rozpłatanie → Krwawe cięcie ×5, Tąpnięcie → Okrzyk wojenny ×4, Tąpnięcie → Szarża ×4, Rozpłatanie → Tąpnięcie ×4.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 40 | 16,7 | 41 | 0,0 |
| Tąpnięcie | odblokowanie | 40 | 14,7 | 143 | 0,4 |
| Cięcie wichru | startowy | 22 | 26,5 | 168 | 0,9 |
| Rozpłatanie | startowy | 32 | 18,1 | 175 | 0,8 |
| Cięcie z wyskoku | sklep | 10 | 13,9 | 204 | 0,5 |
| Cięcie krzyżowe | sklep | 7 | 17,1 | 217 | 0,8 |
| Odtrącenie | sklep | 9 | 12,7 | 19 | 0,4 |
| Krwawe cięcie | sklep | 11 | 9,8 | 176 | 0,9 |
| Szarża | sklep | 11 | 9,7 | 129 | 0,4 |
| Upadek kata | sklep | 7 | 14,4 | 300 | 0,6 |
| Fala przełamania | sklep | 8 | 12,4 | 406 | 0,7 |
| Okrzyk wojenny | nagroda | 13 | 6,6 | – | 0,4 |
| Wypad | sklep | 6 | 13,5 | 24 | 0,3 |
| Kontra | nagroda | 6 | 10,3 | 20 | 0,6 |
| Uderzenie tarczą | nagroda | 3 | 16,7 | 62 | 0,9 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,2, mediana 4,0, najwięcej 6 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 36% | 47,7 | 193 | 1,5 |
| odblokowanie | 54% | 71,5 | 128 | 1,8 |
| klasa | 8% | 10,0 | 74 | 0,7 |
| nagroda | 0% | 0,5 | 81 | 1,0 |
| sklep | 2% | 2,0 | 189 | 1,4 |

Oferty sklepu dusz: widziane 207, kupione 6, odrzucone 138, za mało dusz 63.
Zamiany w slotach: 0,4 na podejście; najczęstsze: Rozpłatanie → Krwawe cięcie ×2, Rozpłatanie → Cięcie krzyżowe ×2, Tąpnięcie → Cięcie krzyżowe ×1, Odtrącenie → Upadek kata ×1, Szarża → Fala przełamania ×1, Fala przełamania → Kontra ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 37 | 48,1 | 192 | 1,5 |
| Rozpłatanie | odblokowanie | 38 | 40,7 | 163 | 1,9 |
| Szarża | odblokowanie | 39 | 34,7 | 92 | 1,6 |
| Okrzyk bojowy | klasa | 39 | 10,3 | 74 | 0,7 |
| Krwawe cięcie | odblokowanie | 3 | 24,0 | 230 | 2,1 |
| Cięcie krzyżowe | sklep | 3 | 19,3 | 193 | 1,7 |
| Tąpnięcie | odblokowanie | 2 | 7,0 | 88 | 0,7 |
| Odtrącenie | nagroda | 1 | 13,0 | 9 | 1,5 |
| Fala przełamania | sklep | 1 | 13,0 | 301 | 0,4 |
| Uderzenie tarczą | odblokowanie | 1 | 10,0 | 62 | 0,6 |
| Kontra | sklep | 1 | 9,0 | – | 1,0 |
| Upadek kata | nagroda | 1 | 2,0 | 376 | 1,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 87,1, szybki atak 37,6, drgnięcie (trafiony) 28,7, unik 21,2, blok 16,2, mocny atak 12,9, przełamana garda 5,7, flaszka (próba) 4,5, parowanie 1,4, riposta 0,3.
Czas w bloku 7 s, w biegu 18 s, bez wytrzymałości (<10%) 88 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Żelazna skóra ×10, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×7, wzmocnienie: Czujność ×7, wzmocnienie: Oddech wojownika ×6, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×5, wzmocnienie: Zwinność ×4, wzmocnienie: Pijawka ×4, wzmocnienie: Alchemik ×4.
Zakupy: ulepszenie broni ×33, ulepszenie: Rozpłatanie ×30, ulepszenie: Cięcie wichru ×26, ulepszenie: Tąpnięcie ×21, oferta: Rozpłatanie ×15, oferta: Krwawe cięcie ×11, oferta: Okrzyk wojenny ×11, oferta: Aura niezłomności ×11.
Punkty cech: Strength 56, Toughness 41, Dexterity 33, Intelligence 27.
Komunikaty gry: „OKRZYK!” ×626; „SZAŁ!” ×312; „GARDA PRZEŁAMANA!” ×234; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×147; „Portale gasną – dobij pozostałe demony” ×133; „Okrzyk wojenny: +25% obrażeń” ×47; „POLEGŁEŚ” ×41; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×40.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 131,8, unik 44,4, szybki atak 38,6, drgnięcie (trafiony) 35,2, blok 21,9, mocny atak 12,0, parowanie 9,4, flaszka (próba) 6,2, przełamana garda 4,9, riposta 3,8.
Czas w bloku 10 s, w biegu 21 s, bez wytrzymałości (<10%) 94 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +3 ×14, ulepszenie: Ulepszenie: Szarża → +3 ×14, ulepszenie: Ulepszenie: Szarża → +2 ×12, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×11, ulepszenie: Ulepszenie: Szarża → +1 ×8, wzmocnienie: Oddech wojownika ×8, wzmocnienie: Obrońca ×8, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×7.
Zakupy: ulepszenie: Rozpłatanie ×70, ulepszenie: Cięcie wichru ×59, ulepszenie: Szarża ×57, ulepszenie: Okrzyk bojowy ×48, ulepszenie broni ×43, dodatkowa flaszka ×12, ulepszenie: Cięcie krzyżowe ×5, ulepszenie: Krwawe cięcie ×4.
Punkty cech: Toughness 78, Strength 129.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Puklerz, Wypad, Kontra, Sztylet parujący, Łuk refleksyjny, Krwawa riposta, Przetrwanie, Strażnik zaklęć, Pierścień riposty.
Komunikaty gry: „OKRZYK!” ×361; „PAROWANIE!” ×298; „SZAŁ!” ×276; „Postawa przełamana – RIPOSTA!” ×236; „GARDA PRZEŁAMANA!” ×200; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×181; „Portale gasną – dobij pozostałe demony” ×162; „RIPOSTA!” ×143.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×321, zablokowany ruch ×9, garda nie zatrzymała ciosu ×7, limit czasu piętra ×1.
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,8; -0,4)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 177°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 132°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (5,6; 8,2)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 157°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (6,3; 1,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,3; 1,1)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-5,6; -8,6)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; 0,6)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 121°)
  - piętro 7 „VII. Zatopione Krużganki” > 360 s; pozostali: Kolos [Moving] (-13; 0; -9), Kultysta topieli [Moving] (-12; 0; -8), Legionista [Moving] (-11; 0; -13), Kolos [Moving] (-10; 0; -7); w drodze: 0
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,8; -8,5)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×433, parowanie w oknie nie zadziałało ×13, garda nie zatrzymała ciosu ×13, zablokowany ruch ×5, limit czasu piętra ×1.
  - Taranowiec: „Taranowiec: Combo” trafił w aktywnym oknie parowania (kąt do źródła 86°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (6,2; -1,1)
  - Wodny chochlik: „Pazury” przeszedł przez gardę (kąt 148°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 110°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 156°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 176°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,1; -9,1)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 37°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 177°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 131°)
  - Kultysta topieli: „Kultysta topieli” przeszedł przez gardę (kąt 134°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; 8,7)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 40 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- **Problem (P2):** 1 podejść przerwanych limitem czasu piętra – możliwy nieosiągalny wróg albo zbyt długie piętro (zob. anomalie).
- Pojedyncze zatrzymania na geometrii (9 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 7× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta: „Kula ognia” przeszedł przez gardę (kąt 177°)
- Parowanie zwykłych ciosów: 66% ciosów mimo to rani (n=86) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 50% ciosów mimo to rani (n=8) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: 53% ciosów mimo to rani (n=81) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 79% ciosów mimo to rani (n=237) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=127) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 39% pocisków, przed którymi był unik (n=450).
- Blok pocisku: 87% ciosów mimo to rani (n=484) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 81% decyzji zapada, gdy do trafienia zostało <0,1 s (n=2344) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=188).
- Telegraf „nieparowalny” daje czas: tylko 12% decyzji zapada mniej niż 0,1 s przed trafieniem (n=235).
- Telegraf „nieblokowalny” daje czas: tylko 14% decyzji zapada mniej niż 0,1 s przed trafieniem (n=14).
- Telegraf „obszarowy” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=343).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 87 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 40 podejściach. Średnio dochodzę do piętra 6,2 – na razie to ściana, nie krzywa.
- Progres między podejściami słabo odczuwalny: 6,3 → 6,1 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 38% udanych (n=737). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (37,6 trafień, 52,8 s), Władca Bramy (23,5 trafień, 30,6 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Siarkowy pomiot (1,1), Kamienny chochlik (1,1), Wodny chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Pan Sfory – średnio 53 s i 38 trafień do zabicia.
- Długie starcie: Legionista (elita) – średnio 44 s i 32 trafień do zabicia.
- 36/39 razy zginąłem z flaszkami w kieszeni. 7 z tych śmierci to utrata ≥50% HP w 3 s, a 12% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 80% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 301 obrażeń na użycie.
- Skill startowy to 36% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 2%. Różnych skilli na podejście: średnio 4,2.
- Najczęściej grałem: Miecz i tarcza + Cięcie wichru (34×, śr. piętro 6,3). Inne: Dwuręczny + Rozpłatanie 3× (6,0), Dwuręczny + Cięcie wichru 2× (5,0), Miecz i tarcza + Rozpłatanie 1× (5,0).
- Unik to moje główne narzędzie: 44,4 na podejście, w tym 5,3 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 72% przypadków.
- Gra nie wykonała uniku od razu średnio 17,4× na podejście – najczęściej: w trakcie poprzedniego uniku 6,1, drgnięcie (trafiony) 3,9, brak wytrzymałości 2,8. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (27% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (35 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

