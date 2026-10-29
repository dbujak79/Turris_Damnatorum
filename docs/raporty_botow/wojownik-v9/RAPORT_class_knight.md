# Raport botów grających — Wojownik

Wygenerowano: 2026-10-28 12:56. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (30 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (30 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 30 | 0 (0%) | 4,9 | 5,0 | 4,3 min | 131,3 | 17964 / 1349 | 4,0 | 2% |
| Gracz H&S / souls-like | 30 | 0 (0%) | 6,0 | 7,0 | 5,3 min | 162,2 | 27931 / 1536 | 4,1 | 3% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 5 5 5 4 6 4 2 4 2 4 2 7 6 5 7 8 5 5 5 2 5 5 5 7 5 5 7 5 4 5
- Gracz H&S / souls-like: 7 5 2 5 7 7 5 7 7 2 8 7 7 6 7 7 6 3 6 8 7 7 2 5 4 7 7 6 8 7

## Gdzie i od czego giną

- **Tester QA** — piętra: 2. II. Kult Bramy ×4, 4. IV. Psiarnia Piekieł ×5, 5. V. Psiarnia Piekieł ×14, 6. VI. Pan Sfory ×2, 7. VII. Zatopione Krużganki ×4, 8. VIII. Zatopione Krużganki ×1.
  Ostatni cios: Kultysta – „Kula ognia” ×10; Czerw – „Ugryzienie” ×3; Siarkowy pomiot – „Siarkowy pomiot” ×3; Taranowiec – „Taranowiec” ×2; Wodny chochlik – „Pazury” ×2; Chochlik wichru – „Pazury” ×2. Śmierć ciosem ≥40% maks. HP: 0/30. Zginął mając flaszki: 26/30.
  Ostatnie 3 s przed śmiercią: średnio 29% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/30). Picie: 133 prób, 119 wypitych (11% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 2. II. Kult Bramy ×3, 3. III. Władca Bramy ×1, 4. IV. Psiarnia Piekieł ×1, 5. V. Psiarnia Piekieł ×4, 6. VI. Pan Sfory ×4, 7. VII. Zatopione Krużganki ×14, 8. VIII. Zatopione Krużganki ×3.
  Ostatni cios: Kultysta – „Kula ognia” ×4; Pan Sfory – „Pan Sfory: Combo” ×4; Ogar otchłani – „Ogar otchłani” ×4; Czerw – „Ugryzienie” ×3; Kultysta topieli – „Kultysta topieli: Zones” ×2; Legionista – „Legionista” ×2. Śmierć ciosem ≥40% maks. HP: 0/30. Zginął mając flaszki: 29/30.
  Ostatnie 3 s przed śmiercią: średnio 39% maks. HP straconych (śmierć z ≥50% HP w 3 s: 4/30). Picie: 147 prób, 124 wypitych (16% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 30 | 30 | 55 s | 27 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 30 | 26 | 62 s | 199 | 0,5 | 39,1 |
| Tester QA | 3. III. Władca Bramy | 26 | 26 | 34 s | 314 | 0,9 | 8,1 |
| Tester QA | 4. IV. Psiarnia Piekieł | 26 | 21 | 62 s | 268 | 0,7 | 41,7 |
| Tester QA | 5. V. Psiarnia Piekieł | 21 | 7 | 65 s | 517 | 1,6 | 28,9 |
| Tester QA | 6. VI. Pan Sfory | 7 | 5 | 40 s | 622 | 3,3 | 4,3 |
| Tester QA | 7. VII. Zatopione Krużganki | 5 | 1 | 61 s | 512 | 1,2 | 21,4 |
| Tester QA | 8. VIII. Zatopione Krużganki | 1 | 0 | 0 s | 803 | 2,0 | 6,0 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 30 | 30 | 54 s | 30 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 30 | 27 | 64 s | 164 | 0,3 | 39,5 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 27 | 26 | 28 s | 175 | 0,2 | 7,4 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 26 | 25 | 61 s | 157 | 0,4 | 45,3 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 25 | 21 | 63 s | 296 | 0,9 | 39,5 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 21 | 17 | 46 s | 514 | 2,2 | 6,0 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 17 | 3 | 93 s | 686 | 1,6 | 23,1 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 3 | 0 | 0 s | 531 | 0,7 | 24,7 |

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Kultysta | 1159 | 0,3 s | 1,1 | 1270 | 20339 | 17 | 14 |
| Pan Sfory | 22 | 40,9 s | 29,7 | 696 | 14166 | 644 | 6 |
| Władca Bramy | 52 | 27,0 s | 18,5 | 489 | 11860 | 228 | 1 |
| Ogar otchłani | 667 | 0,6 s | 1,6 | 692 | 8539 | 12 | 5 |
| Czerw | 2568 | 0,4 s | 1,3 | 1170 | 5005 | 2 | 6 |
| Taranowiec | 223 | 2,7 s | 2,7 | 231 | 3919 | 17 | 4 |
| Kultysta topieli | 20 | 0,4 s | 1,7 | 158 | 3544 | 169 | 3 |
| Siarkowy pomiot | 497 | 0,1 s | 1,1 | 109 | 3182 | 6 | 4 |
| Ognisty chochlik | 703 | 0,2 s | 1,2 | 245 | 2560 | 3 | 3 |
| Legionista | 105 | 8,2 s | 5,2 | 237 | 2502 | 23 | 3 |
| Chochlik wichru | 737 | 0,5 s | 1,4 | 201 | 2219 | 3 | 3 |
| Kamienny chochlik | 691 | 0,2 s | 1,2 | 176 | 1856 | 3 | 1 |
| Wodny chochlik | 697 | 0,2 s | 1,1 | 190 | 1845 | 3 | 2 |
| Kolos | 49 | 4,9 s | 3,3 | 30 | 1715 | 34 | 2 |
| Harpia | 34 | 1,1 s | 2,1 | 94 | 1313 | 38 | 1 |
| Ożywiony nietoperz | 360 | 0,3 s | 1,2 | 211 | 1017 | 3 | 0 |
| Wiedźma wichru | 4 | 0,0 s | 1,0 | 69 | 735 | 184 | 2 |
| Ofiarnik | 6 | 1,2 s | 1,8 | 33 | 244 | 35 | 0 |
| Ognisty chochlik (elita) | 93 | 3,7 s | 4,3 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 31 | 3,2 s | 3,3 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 45 | 5,6 s | 4,1 | ↑ | ↑ | ↑ |  |
| Ogar otchłani (elita) | 20 | 6,3 s | 5,5 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 12 | 1,3 s | 2,5 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 3 | 34,9 s | 20,3 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 3 | 10,8 s | 10,7 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 1 | 6,5 s | 8,0 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 1 | 35,5 s | 24,0 | ↑ | ↑ | ↑ |  |
| Ofiarnik (elita) | 1 | 8,9 s | 7,0 | ↑ | ↑ | ↑ |  |
| Harpia (elita) | 1 | 24,8 s | 12,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1646 | 78% | 3% | 2% | 14% | 2% |
| ciężki | 100 | 54% | 4% | 5% | 34% | 3% |
| nieparowalny | 848 | 72% | 4% | 0% | 12% | 12% |
| nieblokowalny | 65 | 86% | 0% | 0% | 14% | 0% |
| obszarowy | 224 | 81% | 11% | 0% | 0% | 8% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 1771 | 60% | 2% | 4% | 32% | 3% |
| ciężki | 166 | 46% | 0% | 12% | 40% | 2% |
| nieparowalny | 987 | 58% | 6% | 0% | 25% | 11% |
| nieblokowalny | 140 | 69% | 0% | 0% | 31% | 0% |
| obszarowy | 354 | 80% | 17% | 0% | 0% | 3% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 1800 | 30% | 49% | 2% | 9% | 10% |
| Tester QA (0,30 s) | ciężki | 136 | 2% | 1% | 0% | 6% | 91% |
| Tester QA (0,30 s) | nieparowalny | 145 | 4% | 8% | 14% | 23% | 51% |
| Tester QA (0,30 s) | nieblokowalny | 14 | 7% | 0% | 14% | 71% | 7% |
| Tester QA (0,30 s) | obszarowy | 266 | 0% | 2% | 1% | 65% | 33% |
| Tester QA (0,30 s) | pocisk | 1246 | 1% | 10% | 9% | 21% | 59% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 2121 | 2% | 30% | 43% | 7% | 18% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 216 | 2% | 0% | 0% | 8% | 90% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 271 | 6% | 1% | 3% | 39% | 52% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 8 | 0% | 0% | 0% | 50% | 50% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 463 | 1% | 0% | 3% | 48% | 48% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 1746 | 1% | 10% | 8% | 17% | 62% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 465 |
| Wodny chochlik: Pazury | 0,33 s | 355 |
| Kamienny chochlik: Pazury | 0,33 s | 354 |
| Czerw: Ugryzienie | 0,33 s | 1816 |
| Chochlik wichru: Pazury | 0,33 s | 406 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 328 |
| Ognisty chochlik: Pazury | 0,33 s | 393 |
| Władca Bramy: Cięcie w szale | 0,41 s | 30 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 36 |
| Władca Bramy: Piekielne kule | 0,47 s | 13 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| blok | 70% (n=103) | 38% (n=8) | 92% (n=12) | 100% (n=1) | 21% (n=29) | 90% (n=283) |
| blok (eksperyment) | 87% (n=69) | 75% (n=4) | 100% (n=8) | – | 92% (n=13) | 71% (n=48) |
| brak reakcji (eksperyment) | 90% (n=81) | 100% (n=8) | 100% (n=11) | – | 100% (n=10) | 98% (n=45) |
| niezauważony | 86% (n=340) | – | 98% (n=90) | 98% (n=42) | 100% (n=57) | – |
| parowanie | 80% (n=41) | 33% (n=6) | – | – | – | – |
| parowanie (eksperyment) | 80% (n=51) | 100% (n=1) | 100% (n=6) | 100% (n=2) | 100% (n=8) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 87% (n=63) |
| unik | 62% (n=315) | 34% (n=38) | 36% (n=61) | 60% (n=15) | – | 80% (n=119) |
| unik (eksperyment) | 69% (n=98) | 11% (n=9) | 35% (n=20) | 0% (n=2) | 100% (n=16) | 71% (n=35) |
| wyjście z zasięgu | 91% (n=452) | 100% (n=20) | 100% (n=23) | 100% (n=2) | 99% (n=85) | – |
| wyjście z zasięgu (eksperyment) | 88% (n=75) | 100% (n=6) | 100% (n=12) | 100% (n=1) | 100% (n=6) | 94% (n=33) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 22,2 | reakcja na cios wręcz 24,3, unik przed pociskiem 11,0, wyjście z okrążenia 1,8 | brak wytrzymałości 3,7, drgnięcie (trafiony) 3,5, w trakcie uniku 2,6, picie flaszki 1,3, parowanie 0,3, inna 0,3, mocny atak 0,1, umiejętność 0,1, szybki atak 0,1, riposta 0,1, blok 0,0 | 54% (n=729) / 64% (n=331) |
| Gracz H&S / souls-like | 43,1 | reakcja na cios wręcz 39,2, unik przed pociskiem 20,8, doskok do odsłoniętego celu 5,0, wyjście z okrążenia 1,2 | w trakcie uniku 7,3, drgnięcie (trafiony) 3,5, brak wytrzymałości 3,0, picie flaszki 1,7, parowanie 0,9, inna 0,4, blok 0,1, umiejętność 0,1, mocny atak 0,1, szybki atak 0,0, riposta 0,0 | 65% (n=1176) / 78% (n=623) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Dwuręczny | Cięcie wichru | 7 | 0 | 5,3 | 140,1 |
| Tester QA | Miecz i tarcza | Rozpłatanie | 8 | 0 | 4,8 | 129,6 |
| Tester QA | Miecz i tarcza | Cięcie wichru | 8 | 0 | 4,8 | 130,0 |
| Tester QA | Dwuręczny | Rozpłatanie | 7 | 0 | 4,7 | 125,9 |
| Gracz H&S / souls-like | Miecz i tarcza | Cięcie wichru | 7 | 0 | 6,7 | 183,4 |
| Gracz H&S / souls-like | Dwuręczny | Cięcie wichru | 14 | 0 | 6,0 | 166,3 |
| Gracz H&S / souls-like | Miecz i tarcza | Rozpłatanie | 7 | 0 | 5,6 | 146,7 |
| Gracz H&S / souls-like | Dwuręczny | Rozpłatanie | 2 | 0 | 4,5 | 113,5 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Zamach | 91626 | 17% |  |  |
| Cięcie wichru | 79860 | 15% | 451 | 177 |
| Rozpłatanie | 79740 | 15% | 424 | 188 |
| Tąpnięcie | 69734 | 13% | 425 | 164 |
| Cięcie | 43837 | 8% |  |  |
| Fala przełamania | 29288 | 5% | 96 | 305 |
| Egzekucja | 28423 | 5% |  |  |
| Okrzyk bojowy | 21733 | 4% | 499 | 44 |
| Upadek kata | 19761 | 4% | 76 | 260 |
| Potężne cięcie | 16937 | 3% |  |  |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Okrzyk wojenny ×48.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Rozpłatanie | 297168 | 35% | 1336 | 222 |
| Cięcie wichru | 244894 | 29% | 1145 | 214 |
| Zamach | 69311 | 8% |  |  |
| Szarża | 62414 | 7% | 485 | 129 |
| Fala przełamania | 51396 | 6% | 165 | 311 |
| Cięcie | 25795 | 3% |  |  |
| Egzekucja | 24824 | 3% |  |  |
| Okrzyk bojowy | 22572 | 3% | 261 | 86 |
| Potężne cięcie | 11645 | 1% |  |  |
| Riposta | 11515 | 1% |  |  |

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 5,7, mediana 6,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 27% | 23,7 | 170 | 0,9 |
| odblokowanie | 16% | 14,2 | 164 | 0,4 |
| klasa | 19% | 16,6 | 44 | 0,1 |
| nagroda | 3% | 3,0 | 147 | 0,3 |
| sklep | 34% | 29,4 | 143 | 0,5 |

Oferty sklepu dusz: widziane 116, kupione 84, odrzucone 0, za mało dusz 32.
Zamiany w slotach: 2,6 na podejście; najczęstsze: Tąpnięcie → Rozpłatanie ×6, Rozpłatanie → Tąpnięcie ×6, Tąpnięcie → Cięcie wichru ×6, Cięcie wichru → Rozpłatanie ×4, Rozpłatanie → Cięcie wichru ×4, Cięcie wichru → Cięcie z wyskoku ×4.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Okrzyk bojowy | klasa | 30 | 16,6 | 44 | 0,1 |
| Cięcie wichru | startowy | 19 | 23,7 | 177 | 0,8 |
| Tąpnięcie | odblokowanie | 30 | 14,2 | 164 | 0,4 |
| Rozpłatanie | startowy | 24 | 17,7 | 188 | 0,9 |
| Cięcie z wyskoku | sklep | 11 | 10,0 | 146 | 0,7 |
| Wypad | sklep | 6 | 17,3 | 30 | 0,4 |
| Fala przełamania | sklep | 5 | 19,2 | 305 | 0,5 |
| Krwawe cięcie | sklep | 6 | 14,7 | 133 | 0,4 |
| Upadek kata | sklep | 6 | 12,7 | 260 | 0,8 |
| Cięcie krzyżowe | sklep | 7 | 10,6 | 148 | 0,4 |
| Szarża | sklep | 5 | 13,2 | 91 | 0,1 |
| Odtrącenie | sklep | 7 | 9,1 | 13 | 0,1 |
| Okrzyk wojenny | sklep | 8 | 6,0 | – | 0,5 |
| Kontra | sklep | 6 | 8,0 | 29 | 0,6 |
| Uderzenie tarczą | sklep | 2 | 17,5 | 57 | 0,4 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 4,4, mediana 4,0, najwięcej 6 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| startowy | 44% | 50,6 | 205 | 1,5 |
| odblokowanie | 36% | 41,7 | 206 | 1,6 |
| klasa | 8% | 8,7 | 86 | 0,7 |
| nagroda | 5% | 5,3 | 214 | 1,4 |
| sklep | 8% | 9,0 | 235 | 1,5 |

Oferty sklepu dusz: widziane 149, kupione 15, odrzucone 93, za mało dusz 41.
Zamiany w slotach: 1,0 na podejście; najczęstsze: Szarża → Fala przełamania ×8, Szarża → Rozpłatanie ×4, Szarża → Krwawe cięcie ×3, Szarża → Cięcie krzyżowe ×3, Rozpłatanie → Fala przełamania ×2, Szarża → Cięcie wichru ×2.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Rozpłatanie | startowy | 34 | 39,3 | 222 | 1,9 |
| Cięcie wichru | sklep | 25 | 45,8 | 214 | 1,5 |
| Szarża | odblokowanie | 27 | 18,0 | 129 | 1,1 |
| Okrzyk bojowy | klasa | 30 | 8,7 | 86 | 0,7 |
| Fala przełamania | sklep | 11 | 15,0 | 311 | 1,1 |
| Cięcie krzyżowe | sklep | 4 | 6,8 | 189 | 1,6 |
| Krwawe cięcie | odblokowanie | 3 | 7,0 | 147 | 2,0 |
| Tąpnięcie | odblokowanie | 1 | 10,0 | 87 | 0,5 |
| Uderzenie tarczą | nagroda | 1 | 5,0 | 52 | 1,0 |
| Kontra | odblokowanie | 1 | 5,0 | 89 | 0,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: umiejętność 86,9, szybki atak 37,7, drgnięcie (trafiony) 27,7, unik 22,2, blok 16,4, mocny atak 12,8, przełamana garda 5,1, flaszka (próba) 4,4, parowanie 1,8, riposta 0,5.
Czas w bloku 7 s, w biegu 17 s, bez wytrzymałości (<10%) 89 s na podejście.
Najczęściej brane nagrody: wzmocnienie: Oddech wojownika ×6, ulepszenie: Ulepszenie: Tąpnięcie → +1 ×5, nowy skill: Aura niezłomności ×4, wzmocnienie: Ostrze gniewu ×4, wzmocnienie: Moc arkanów ×4, wzmocnienie: Obrońca ×3, wzmocnienie: Jasność umysłu ×3, wzmocnienie: Pijawka ×3.
Zakupy: ulepszenie broni ×26, ulepszenie: Rozpłatanie ×23, ulepszenie: Cięcie wichru ×20, ulepszenie: Tąpnięcie ×16, oferta: Cięcie z wyskoku ×11, oferta: Okrzyk wojenny ×7, oferta: Aura niezłomności ×7, oferta: Cięcie krzyżowe ×7.
Punkty cech: Strength 37, Toughness 31, Dexterity 27, Intelligence 21.
Komunikaty gry: „OKRZYK!” ×467; „SZAŁ!” ×227; „GARDA PRZEŁAMANA!” ×162; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×113; „Portale gasną – dobij pozostałe demony” ×97; „DAR BOSSA: trzy pieczęcie do wyboru na postoju” ×31; „Garda wroga przełamana!” ×31; „PAROWANIE!” ×30.

**Gracz H&S / souls-like** — akcje na podejście: umiejętność 115,3, unik 43,1, drgnięcie (trafiony) 30,2, szybki atak 29,4, blok 21,1, mocny atak 14,4, przełamana garda 5,7, flaszka (próba) 4,9, parowanie 3,9, riposta 1,6.
Czas w bloku 10 s, w biegu 20 s, bez wytrzymałości (<10%) 91 s na podejście.
Najczęściej brane nagrody: ulepszenie: Ulepszenie: Rozpłatanie → +3 ×17, wzmocnienie: Zwinność ×11, ulepszenie: Ulepszenie: Rozpłatanie → +4 ×9, ulepszenie: Ulepszenie: Rozpłatanie → +1 ×7, wzmocnienie: Oddech wojownika ×7, ulepszenie: Ulepszenie: Rozpłatanie → +2 ×6, wzmocnienie: Obrońca ×6, ulepszenie: Ulepszenie: Szarża → +2 ×5.
Zakupy: ulepszenie: Rozpłatanie ×68, ulepszenie: Cięcie wichru ×39, ulepszenie: Okrzyk bojowy ×39, ulepszenie broni ×29, ulepszenie: Szarża ×17, ulepszenie: Fala przełamania ×12, dodatkowa flaszka ×9, oferta: Fala przełamania ×6.
Punkty cech: Toughness 58, Strength 91.
Odblokowania za popiół między podejściami: Rozpłatanie, Szarża, Uderzenie tarczą, Krwawe cięcie, Puklerz, Wypad, Kontra, Sztylet parujący, Łuk refleksyjny, Krwawa riposta, Strażnik zaklęć, Pierścień riposty, Przetrwanie.
Komunikaty gry: „OKRZYK!” ×241; „GARDA PRZEŁAMANA!” ×176; „SZAŁ!” ×148; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×131; „Portale gasną – dobij pozostałe demony” ×121; „PAROWANIE!” ×79; „Postawa przełamana – RIPOSTA!” ×68; „RIPOSTA!” ×47.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×249, garda nie zatrzymała ciosu ×4, zablokowany ruch ×3, parowanie w oknie nie zadziałało ×2.
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 133°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-5,6; 0,5)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,2; 1,1)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 164°)
  - Harpia: „Harpia: WindStep” przeszedł przez gardę (kąt 162°)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-6,3; -9,1)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 99°)
  - Ożywiony nietoperz: „Ukąszenie” przeszedł przez gardę (kąt 163°)
  - Ożywiony nietoperz: „Ukąszenie” trafił w aktywnym oknie parowania (kąt do źródła 132°)
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×242, garda nie zatrzymała ciosu ×7, zablokowany ruch ×2, parowanie w oknie nie zadziałało ×1.
  - Taranowiec: „Taranowiec” przeszedł przez gardę (kąt 145°)
  - Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 142°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 129°)
  - Czerw: „Ugryzienie” przeszedł przez gardę (kąt 131°)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 136°)
  - Wiedźma wichru: „Wiedźma wichru: CycloneEye” przeszedł przez gardę (kąt 127°)
  - Wiedźma wichru: „Wiedźma wichru: CycloneEye” przeszedł przez gardę (kąt 135°)
  - piętro 4 „IV. Psiarnia Piekieł”: brak postępu przy (7,8; -0,6)
  - piętro 2 „II. Kult Bramy”: brak postępu przy (-7,2; 6,9)
  - Kultysta: „Kula ognia” przeszedł przez gardę (kąt 138°)

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 30 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Pojedyncze zatrzymania na geometrii (3 razy) – do przejrzenia, nie blokują gry.
- **Do sprawdzenia (P3):** 2× cios trafił w aktywnym oknie parowania. Najczęściej to cios spoza kąta parowania – czy obrót bohatera do celu przy namierzaniu jest wystarczająco szybki? Przykład: Czerw: „Ugryzienie” trafił w aktywnym oknie parowania (kąt do źródła 99°)
- **Do sprawdzenia (P3):** 4× cios przeszedł przez podniesioną gardę (zwykle z boku/tyłu). Przykład: Kultysta: „Kula ognia” przeszedł przez gardę (kąt 133°)
- Parowanie zwykłych ciosów: 80% ciosów mimo to rani (n=92) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed czerwonym (nieblokowalnym): 53% ciosów mimo to rani (n=17) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed ciężkim: działa – rani tylko 30% ciosów (n=47).
- Blok zwykłych ciosów: 77% ciosów mimo to rani (n=172) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Wychodzenie z różowego kręgu: 99% ciosów mimo to rani (n=91) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 36% pocisków, przed którymi był unik (n=331).
- Blok pocisku: 88% ciosów mimo to rani (n=331) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 79% decyzji zapada, gdy do trafienia zostało <0,1 s (n=1800) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=136).
- Telegraf „nieparowalny” daje czas: tylko 12% decyzji zapada mniej niż 0,1 s przed trafieniem (n=145).
- Telegraf „nieblokowalny” daje czas: tylko 7% decyzji zapada mniej niż 0,1 s przed trafieniem (n=14).
- Telegraf „obszarowy” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=266).
- Pokrycie: wszystkie akcje gracza (ciosy, riposta, blok, parowanie, unik, skille, flaszka) zostały wykonane w prawdziwej walce.
- UX: gra odmawiała akcji: „Brak broni/tarczy zdolnej do parowania” ×2 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 71 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 30 podejściach. Średnio dochodzę do piętra 6,0 – na razie to ściana, nie krzywa.
- Progres między podejściami słabo odczuwalny: 5,9 → 6,0 średnio. Meta-progresja niewiele zmienia w walce.
- Parowanie zawodne: tylko 43% udanych (n=163). Ryzyko większe niż nagroda – zaczynam wolić unik albo blok.
- Gąbki na obrażenia: Pan Sfory (31,2 trafień, 42,6 s), Władca Bramy (17,9 trafień, 23,8 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Siarkowy pomiot (1,0), Wodny chochlik (1,1), Kultysta (1,1) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Pan Sfory – średnio 43 s i 31 trafień do zabicia.
- Długie starcie: Legionista (elita) – średnio 40 s i 22 trafień do zabicia.
- 29/30 razy zginąłem z flaszkami w kieszeni. 4 z tych śmierci to utrata ≥50% HP w 3 s, a 16% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 82% obrażeń – broń jest tłem, walka zamienia się w obracanie cooldownów.
- Najmocniej czuć „Fala przełamania”: 311 obrażeń na użycie.
- Skill startowy to 44% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 12%. Różnych skilli na podejście: średnio 4,4.
- Najczęściej grałem: Dwuręczny + Cięcie wichru (14×, śr. piętro 6,0). Inne: Miecz i tarcza + Rozpłatanie 7× (5,6), Miecz i tarcza + Cięcie wichru 7× (6,7), Dwuręczny + Rozpłatanie 2× (4,5).
- Unik to moje główne narzędzie: 43,1 na podejście, w tym 6,2 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 78% przypadków.
- Gra nie wykonała uniku od razu średnio 17,2× na podejście – najczęściej: w trakcie poprzedniego uniku 7,3, drgnięcie (trafiony) 3,5, brak wytrzymałości 3,0. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Wytrzymałość kończy się często (29% czasu walki poniżej 10%) – zarządzanie staminą to główne ograniczenie, tak jak w Souls.
- Dużo drgnięć po trafieniu (30 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

