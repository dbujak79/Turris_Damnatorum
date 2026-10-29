# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 12:31. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (20 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (20 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 20 | 0 (0%) | 4,6 | 4,5 | 4,5 min | 146,4 | 19452 / 1424 | 4,5 | 1% |
| Gracz H&S / souls-like | 20 | 0 (0%) | 5,0 | 5,0 | 5,1 min | 161,0 | 27573 / 1395 | 4,2 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 2 5 4 3 4 6 4 7 5 5 4 6 5 5 4 4 4 5 4 6
- Gracz H&S / souls-like: 2 7 2 7 2 2 4 7 2 5 5 6 5 7 5 7 7 5 7 5

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×1, 3. III. Władca Bramy ×1, 4. IV. Psiarnia Piekieł ×8, 5. V. Psiarnia Piekieł ×6, 6. VI. Pan Sfory ×3, 7. VII. Zatopione Krużganki ×1.
  Ostatni cios: Czerw – „Ugryzienie” ×4; Kultysta – „Kula ognia” ×3; Ogar otchłani – „Ogar otchłani” ×3; Pan Sfory – „Pan Sfory: Combo” ×2; Wodny chochlik – „Pazury” ×2; Kamienny chochlik – „Pazury” ×2. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 17/20.
  Ostatnie 3 s przed śmiercią: średnio 31% maks. HP straconych (śmierć z ≥50% HP w 3 s: 2/20). Picie: 111 prób, 90 wypitych (19% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×5, 4. IV. Psiarnia Piekieł ×1, 5. V. Psiarnia Piekieł ×6, 6. VI. Pan Sfory ×1, 7. VII. Zatopione Krużganki ×7.
  Ostatni cios: Kultysta – „Kula ognia” ×5; Ogar otchłani – „Ogar otchłani” ×3; Czerw – „Ugryzienie” ×2; Taranowiec – „Taranowiec” ×2; Ognisty chochlik – „Pazury” ×2; Kultysta – „Wachlarz ognia” ×1. Śmierć ciosem ≥40% maks. HP: 0/20. Zginął mając flaszki: 20/20.
  Ostatnie 3 s przed śmiercią: średnio 34% maks. HP straconych (śmierć z ≥50% HP w 3 s: 3/20). Picie: 96 prób, 83 wypitych (14% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 20 | 20 | 68 s | 31 | 0,0 | 30,0 |
| Tester QA | 2. II. Kult Bramy | 20 | 19 | 75 s | 282 | 1,1 | 48,0 |
| Tester QA | 3. III. Władca Bramy | 19 | 18 | 28 s | 288 | 0,7 | 7,4 |
| Tester QA | 4. IV. Psiarnia Piekieł | 18 | 10 | 77 s | 451 | 1,4 | 47,7 |
| Tester QA | 5. V. Psiarnia Piekieł | 10 | 4 | 72 s | 547 | 1,8 | 34,5 |
| Tester QA | 6. VI. Pan Sfory | 4 | 1 | 38 s | 694 | 2,8 | 3,3 |
| Tester QA | 7. VII. Zatopione Krużganki | 1 | 0 | 0 s | 372 | 0,0 | 10,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 20 | 20 | 68 s | 41 | 0,0 | 30,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 20 | 15 | 74 s | 287 | 0,9 | 42,8 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 15 | 15 | 27 s | 225 | 0,9 | 7,2 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 15 | 14 | 77 s | 185 | 0,5 | 56,4 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 14 | 8 | 80 s | 481 | 1,6 | 40,9 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 8 | 7 | 46 s | 493 | 2,0 | 5,9 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 7 | 0 | 0 s | 646 | 1,0 | 27,3 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 836 | 0,4 s | 1,1 | 1024 | 16337 | 19 | 9 |
| Władca Bramy | 33 | 23,6 s | 17,1 | 297 | 8253 | 250 | 1 |
| Pan Sfory | 8 | 40,4 s | 26,9 | 295 | 6426 | 803 | 4 |
| Ogar otchłani | 413 | 0,5 s | 1,5 | 462 | 5716 | 13 | 6 |
| Czerw | 1734 | 0,3 s | 1,2 | 994 | 3807 | 2 | 6 |
| Siarkowy pomiot | 349 | 0,1 s | 1,1 | 76 | 2762 | 8 | 0 |
| Taranowiec | 120 | 2,4 s | 2,6 | 148 | 2589 | 21 | 4 |
| Ognisty chochlik | 562 | 0,1 s | 1,1 | 198 | 2076 | 3 | 2 |
| Kamienny chochlik | 547 | 0,2 s | 1,1 | 147 | 1489 | 3 | 2 |
| Wodny chochlik | 570 | 0,2 s | 1,1 | 156 | 1432 | 3 | 2 |
| Chochlik wichru | 495 | 0,7 s | 1,5 | 139 | 1340 | 3 | 2 |
| Legionista | 60 | 7,0 s | 4,9 | 124 | 1268 | 20 | 0 |
| Harpia | 20 | 1,5 s | 1,8 | 71 | 838 | 38 | 1 |
| Kultysta topieli | 18 | 0,6 s | 1,3 | 42 | 835 | 46 | 0 |
| Ożywiony nietoperz | 216 | 0,2 s | 1,2 | 115 | 604 | 3 | 0 |
| Kolos | 9 | 3,5 s | 3,1 | 12 | 600 | 67 | 1 |
| Ofiarnik | 3 | 0,3 s | 2,0 | 4 | 10 | 3 | 0 |
| Ognisty chochlik (elita) | 76 | 3,6 s | 4,0 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 18 | 2,9 s | 3,0 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 15 | 7,4 s | 5,7 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 19 | 1,7 s | 2,4 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 19 | 7,6 s | 4,1 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 2 | 18,0 s | 11,5 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 3 | 24,5 s | 19,7 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 2 | 4,1 s | 5,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1110 | 80% | 2% | 1% | 14% | 2% |
| ciężki | 42 | 71% | 0% | 2% | 24% | 2% |
| nieparowalny | 657 | 75% | 4% | 0% | 7% | 13% |
| nieblokowalny | 48 | 90% | 0% | 0% | 10% | 0% |
| obszarowy | 120 | 83% | 13% | 0% | 0% | 4% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1349 | 58% | 3% | 2% | 34% | 3% |
| ciężki | 84 | 48% | 1% | 6% | 45% | 0% |
| nieparowalny | 665 | 62% | 3% | 0% | 19% | 15% |
| nieblokowalny | 71 | 80% | 0% | 0% | 20% | 0% |
| obszarowy | 158 | 85% | 10% | 0% | 0% | 5% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1268 | 28% | 53% | 2% | 8% | 9% |
| Tester QA (0,30 s) | ciężki | 57 | 2% | 2% | 0% | 0% | 96% |
| Tester QA (0,30 s) | nieparowalny | 76 | 9% | 5% | 20% | 12% | 54% |
| Tester QA (0,30 s) | nieblokowalny | 8 | 0% | 0% | 25% | 63% | 13% |
| Tester QA (0,30 s) | obszarowy | 179 | 0% | 1% | 7% | 77% | 16% |
| Tester QA (0,30 s) | pocisk | 875 | 1% | 12% | 8% | 25% | 54% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 1605 | 2% | 33% | 48% | 6% | 12% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 110 | 1% | 2% | 1% | 6% | 90% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 126 | 8% | 0% | 5% | 26% | 61% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 7 | 0% | 0% | 14% | 43% | 43% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 260 | 1% | 1% | 3% | 56% | 38% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 1097 | 1% | 9% | 9% | 15% | 65% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 311 |
| Czerw: Ugryzienie | 0,33 s | 1510 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 176 |
| Ognisty chochlik: Pazury | 0,33 s | 337 |
| Wodny chochlik: Pazury | 0,33 s | 313 |
| Chochlik wichru: Pazury | 0,33 s | 272 |
| Kamienny chochlik: Pazury | 0,33 s | 272 |
| Władca Bramy: Cięcie w szale | 0,41 s | 23 |
| Władca Bramy: Drugie cięcie | 0,48 s | 53 |
| Kultysta: Kula ognia | 0,50 s | 2482 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 72% (n=60) | – | 87% (n=15) | 100% (n=2) | 7% (n=15) | 92% (n=228) |
| blok (eksperyment) | 80% (n=59) | 100% (n=1) | 86% (n=7) | 100% (n=1) | 89% (n=9) | 68% (n=28) |
| brak reakcji (eksperyment) | 90% (n=62) | 100% (n=5) | 100% (n=4) | – | 89% (n=9) | 90% (n=39) |
| niezauważony | 89% (n=217) | – | 96% (n=79) | 100% (n=30) | 100% (n=1) | – |
| parowanie | 79% (n=19) | 100% (n=1) | – | – | – | – |
| parowanie (eksperyment) | 85% (n=20) | 0% (n=1) | 100% (n=3) | – | 100% (n=3) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 98% (n=62) |
| unik | 64% (n=224) | 59% (n=22) | 46% (n=26) | 50% (n=4) | – | 77% (n=84) |
| unik (eksperyment) | 79% (n=84) | 50% (n=2) | 87% (n=15) | 67% (n=9) | 100% (n=6) | 92% (n=26) |
| wyjście z zasięgu | 90% (n=306) | 100% (n=10) | 100% (n=14) | – | 100% (n=68) | – |
| wyjście z zasięgu (eksperyment) | 93% (n=59) | – | 83% (n=6) | 100% (n=2) | 100% (n=9) | 100% (n=21) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 22,1 | reakcja na cios wręcz 25,1, unik przed pociskiem 11,9, wyjście z okrążenia 1,8 | brak wytrzymałości 4,8, drgnięcie (trafiony) 4,1, w trakcie uniku 2,8, picie flaszki 1,3, parowanie 0,6, inna 0,4, mocny atak 0,1, przełamana garda 0,1, umiejętność 0,1, riposta 0,1, blok 0,1, szybki atak 0,1 | 48% (n=501) / 63% (n=238) |
| Gracz H&S / souls-like | 40,9 | reakcja na cios wręcz 41,9, unik przed pociskiem 18,9, doskok do odsłoniętego celu 2,8, wyjście z okrążenia 0,9 | w trakcie uniku 7,9, drgnięcie (trafiony) 3,4, brak wytrzymałości 2,9, picie flaszki 1,6, parowanie 1,0, riposta 0,2, mocny atak 0,2, blok 0,1, inna 0,1, szybki atak 0,1, umiejętność 0,1 | 70% (n=838) / 76% (n=378) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Miecz i tarcza | Cięcie wichru | 5 | 0 | 5,2 | 172,0 |
| Tester QA | Dwuręczny | Cięcie wichru | 5 | 0 | 5,2 | 160,0 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 5 | 0 | 4,0 | 129,6 |
| Tester QA | Dwuręczny | Rozpłatanie | 5 | 0 | 4,0 | 123,8 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 12 | 0 | 5,7 | 187,2 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 2 | 0 | 4,5 | 137,5 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 5 | 0 | 4,0 | 129,6 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 1 | 0 | 2,0 | 51,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 73903 | 19% |  |  |
| Tąpnięcie | 56252 | 14% | 345 | 163 |
| Cięcie wichru | 51556 | 13% | 316 | 163 |
| Rozpłatanie | 50130 | 13% | 273 | 184 |
| Cięcie krzyżowe | 36349 | 9% | 162 | 224 |
| Cięcie | 29828 | 8% |  |  |
| Egzekucja | 18616 | 5% |  |  |
| Okrzyk bojowy | 15673 | 4% | 352 | 45 |
| Potężne cięcie | 13888 | 4% |  |  |
| Cięcie z wyskoku | 10109 | 3% | 63 | 160 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×46.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Cięcie wichru | 170081 | 31% | 800 | 213 |
| Rozpłatanie | 167616 | 30% | 617 | 272 |
| Szarża | 81156 | 15% | 523 | 155 |
| Zamach | 65942 | 12% |  |  |
| Egzekucja | 16487 | 3% |  |  |
| Okrzyk bojowy | 15549 | 3% | 171 | 91 |
| Fala przełamania | 8237 | 1% | 27 | 305 |
| Cięcie | 7427 | 1% |  |  |
| Potężne cięcie | 5576 | 1% |  |  |
| Szarża barkiem | 4154 | 1% |  |  |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,4, mediana 5,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 30% | 27,1 | 169 | 0,9 |
| odblokowanie | 19% | 17,3 | 163 | 0,7 |
| klasa | 19% | 17,6 | 45 | 0,1 |
| nagroda | 4% | 3,3 | 220 | 0,4 |
| sklep | 28% | 25,4 | 139 | 0,8 |

Oferty sklepu dusz: widziane 72, kupione 53, odrzucone 0, za mało dusz 19.
Zamiany w slotach: 2,3 na podejście; najczęstsze: Tąpnięcie → Wypad ×7, Rozpłatanie → Tąpnięcie ×6, Cięcie wichru → Tąpnięcie ×5, Tąpnięcie → Cięcie z wyskoku ×4, Tąpnięcie → Okrzyk wojenny ×3, Tąpnięcie → Rozpłatanie ×3.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 20 | 17,6 | 45 | 0,1 |
| Tąpnięcie | odblokowanie | 20 | 17,3 | 163 | 0,7 |
| Cięcie wichru | startowy | 10 | 31,6 | 163 | 1,0 |
| Rozpłatanie | startowy | 16 | 17,1 | 184 | 0,9 |
| Cięcie krzyżowe | sklep | 9 | 18,0 | 224 | 0,8 |
| Wypad | sklep | 8 | 12,9 | 36 | 1,0 |
| Cięcie z wyskoku | nagroda | 9 | 7,0 | 160 | 0,6 |
| Okrzyk wojenny | sklep | 7 | 6,6 | – | 0,7 |
| Szarża | nagroda | 3 | 15,3 | 142 | 0,3 |
| Upadek kata | sklep | 1 | 27,0 | 288 | 1,4 |
| Kontra | sklep | 1 | 25,0 | 27 | 0,0 |
| Fala przełamania | sklep | 2 | 10,0 | 361 | 1,0 |
| Uderzenie tarczą | sklep | 1 | 17,0 | 68 | 0,8 |
| Odtrącenie | sklep | 2 | 6,0 | 41 | 1,5 |
| Krwawe cięcie | sklep | 1 | 4,0 | 84 | 0,0 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,1, mediana 4,0, najwięcej 6 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 41% | 45,6 | 215 | 1,4 |
| odblokowanie | 46% | 50,8 | 211 | 1,7 |
| klasa | 8% | 8,6 | 91 | 0,8 |
| nagroda | 1% | 1,3 | 101 | 1,6 |
| sklep | 4% | 4,2 | 257 | 1,6 |

Oferty sklepu dusz: widziane 79, kupione 8, odrzucone 51, za mało dusz 20.
Zamiany w slotach: 0,5 na podejście; najczęstsze: Szarża → Rozpłatanie ×3, Szarża → Cięcie krzyżowe ×2, Tąpnięcie → Uderzenie tarczą ×1, Fala przełamania → Krwawe cięcie ×1, Uderzenie tarczą → Fala przełamania ×1, Szarża → Kontra ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Cięcie wichru | startowy | 17 | 47,1 | 213 | 1,4 |
| Rozpłatanie | odblokowanie | 22 | 28,0 | 272 | 1,8 |
| Szarża | odblokowanie | 18 | 29,1 | 155 | 1,7 |
| Okrzyk bojowy | klasa | 20 | 8,6 | 91 | 0,8 |
| Fala przełamania | sklep | 1 | 27,0 | 305 | 1,6 |
| Tąpnięcie | odblokowanie | 2 | 11,0 | 92 | 0,5 |
| Krwawe cięcie | nagroda | 1 | 17,0 | 116 | 1,9 |
| Uderzenie tarczą | nagroda | 1 | 9,0 | 71 | 1,0 |
| Cięcie krzyżowe | sklep | 1 | 9,0 | 233 | 1,4 |
| Wypad | sklep | 1 | 7,0 | 50 | 1,0 |
| Kontra | sklep | 1 | 5,0 | 26 | 1,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 90,6, szybki atak 43,5, drgnięcie (trafiony) 31,2, unik 22,1, blok 17,2, mocny atak 14,5, przełamana garda 5,8, flaszka (próba) 5,6, parowanie 1,2, riposta 0,3.
Czas w bloku 7 s, w biegu 19 s, bez wytrzymałości (<10%) 96 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Kolekcjoner flaszek ×3, wzmocnienie: Żelazna skóra ×3, nowy skill: Cięcie z wyskoku ×3, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×3, wzmocnienie: Pijawka ×3, przedmiot: Rękawice zaklinacza ×2, wzmocnienie: Krew przodków ×2, wzmocnienie: Ostrze gniewu ×2.
Zakupy: ulepszenie: Rozpłatanie ×20, ulepszenie: Cięcie wichru ×18, ulepszenie broni ×18, ulepszenie: Tąpnięcie ×16, oferta: Cięcie krzyżowe ×9, oferta: Wypad ×8, ulepszenie: Wypad ×8, oferta: Rozpłatanie ×7.
Punkty cech: Strength 24, Toughness 20, Dexterity 18, Intelligence 10.
Komunikaty gry: „OKRZYK!” ×332; „SZAŁ!” ×165; „GARDA PRZEŁAMANA!” ×117; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×69; „Portale gasną – dobij pozostałe demony” ×61; „Okrzyk wojenny: +29% obrażeń” ×30; „POLEGŁEŚ” ×20; „Władca Bramy otwiera wrota – nadlatują nietoperze!” ×19.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 110,4, unik 40,9, szybki atak 30,6, drgnięcie (trafiony) 29,0, blok 21,2, mocny atak 16,1, przełamana garda 7,3, flaszka (próba) 4,8, parowanie 1,6, riposta 0,7.
Czas w bloku 9 s, w biegu 17 s, bez wytrzymałości (<10%) 101 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +3 ×8, ulepszenie: Ulepszenie: Szarża → +3 ×7, ulepszenie: Ulepszenie: Szarża → +1 ×5, wzmocnienie: Oddech wojownika ×4, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×4, wzmocnienie: Pijawka ×4, ulepszenie: Ulepszenie: Szarża → +2 ×4, wzmocnienie: Siła tytana ×3.
Zakupy: ulepszenie: Rozpłatanie ×46, ulepszenie: Cięcie wichru ×27, ulepszenie: Okrzyk bojowy ×21, ulepszenie: Szarża ×19, ulepszenie broni ×15, dodatkowa flaszka ×3, oferta: Rozpłatanie ×3, ulepszenie: Fala przełamania ×2.
Punkty cech: Toughness 34, Strength 45.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Puklerz, Krwawe cięcie, Wypad, Kontra, Sztylet parujący, Łuk refleksyjny, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×160; „GARDA PRZEŁAMANA!” ×149; „SZAŁ!” ×108; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×76; „Portale gasną – dobij pozostałe demony” ×63; „PAROWANIE!” ×31; „Garda wroga przełamana!” ×30; „Postawa przełamana – RIPOSTA!” ×24.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×151, garda nie zatrzymała ciosu ×5, zablokowany ruch ×2, parowanie w oknie nie zadziałało ×1.
  - Czerw: „Ugryzienie” przeszedł przez gardę (kąt 152°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 143°)
  - Ogar otchłani: „Ogar otchłani: Combo” przeszedł przez gardę (kąt 173°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 129°)
  - Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 94°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 141°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,3; -1,1)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,9; -6,9)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×136, zablokowany ruch ×3, parowanie w oknie nie zadziałało ×2, garda nie zatrzymała ciosu ×2.
  - piętro 2 „II. Kult Bramy”: brak postępu przy (6,2; -1,1)
  - Chochlik wichru: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 110°)
  - Wodny chochlik: „Pazury” trafił w aktywnym oknie parowania (kąt do źródła 129°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (7,8; -0,4)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 122°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 152°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (-7,1; -9,1)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 20 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (2 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 1× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Ogar otchłani: „Ogar otchłani” trafił w aktywnym oknie parowania (kąt do źródła 94°)
- **Do sprawdzenia (P3):** 5× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Czerw: „Ugryzienie” przeszedł przez gardę (kąt 152°)
- Parowanie zwykłych ciosów: 82% ciosów mimo to rani (n=39) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 62% ciosów mimo to rani (n=13) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: 58% ciosów mimo to rani (n=24) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Blok zwykłych ciosów: 76% ciosów mimo to rani (n=119) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=77) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 37% pocisków, przed którymi był unik (n=238).
- Blok pocisku: 89% ciosów mimo to rani (n=256) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 82% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1268) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 4% decyzji zapada mniej niż 0,1 s przed trafieniem (n=57).
- Telegraf „nieparowalny” daje czas: tylko 14% decyzji zapada mniej niż 0,1 s przed trafieniem (n=76).
- Telegraf „obszarowy” daje czas: tylko 1% decyzji zapada mniej niż 0,1 s przed trafieniem (n=179).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Brak broni/tarczy zdolnej do parowania” ×1 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 72 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 20 podejściach. Średnio dochodzę do piętra 5,0 – na razie to ściana, nie krzywa.
- Czuję progres: druga połowa podejść średnio piętro 5,9 wobec 4,0 na początku (popiół, odblokowania, wybór zestawu).
- Parowanie zawodne: tylko 32% udanych (n=62). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Władca Bramy (17,5 trafień, 23,3 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Wodny chochlik (1,1), Kamienny chochlik (1,1), Ognisty chochlik (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Pan Sfory – średnio 41 s i 27 trafień do zabicia.
- 20/20 razy zginąłem z flaszkami w kieszeni. 3 z tych śmierci to utrata ≥50% HP w 3 s, a 14% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 82% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 305 obrażeń na użycie.
- Skill startowy to 41% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 5%. Różnych skilli na podejście: średnio 4,1.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (12×, śr. piętro 5,7). Inne: Miecz i tarcza + Cięcie wichru 5× (4,0), Dwuręczny + Rozpłatanie 2× (4,5), Miecz i tarcza + Rozpłatanie 1× (2,0).
- Unik to moje główne narzędzie: 40,9 na podejście, w tym 3,7 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 76% przypadków.
- Gra nie wykonała uniku od razu średnio 17,3× na podejście – najczęściej: w trakcie poprzedniego uniku 7,9, drgnięcie (trafiony) 3,4, brak wytrzymałości 2,9. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (33% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (29 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

