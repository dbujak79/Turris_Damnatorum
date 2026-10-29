# Raport botów grających — Mag

Wygenerowano: 2026-10-29 10:01. Scena `Main`, prawdziwa treść (`GameConfig`), trudność podstawowa, stały krok symulacji 1/60 s. Boty sterują wyłącznie tym, co ma gracz (ruch, garda, bieg, namierzanie, przyciski akcji) i reagują na początek zamachu z opóźnieniem człowieka.

- **Tester QA** (3 podejść): Metodyczny tester: przechodzi wieżę poprawnie, ale ~30% ciosów przyjmuje celowo każdą możliwą reakcją (blok, parowanie, unik, wyjście z kręgu, brak reakcji), rotuje zestawy broni i skille startowe, używa każdej akcji i pilnuje niezmienników reguł. Kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji. Bez meta-progresji (stała linia bazowa).
- **Gracz H&S / souls-like** (3 podejść): Doświadczony gracz Dark Souls i Diablo: namierza, paruje zwykłe ciosy, unika czerwonych, karze odsłonięcia seriami 2–3 ciosów, ripostuje, szaleje skillami przy grupach, pilnuje wytrzymałości. Kupuje oferty sklepu i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie albo dają atak z dystansu; układa sloty według obrażeń na sekundę odnowienia z własnych podejść, nowe skille próbuje z ciekawości. Między podejściami wydaje popiół i wybiera zestaw, który mu najlepiej idzie.

## Wyniki w skrócie

| Osobowość | Podejścia | Zwycięstwa | Śr. piętro | Mediana | Śr. czas gry | Zabójstwa/podejście | Zadane / otrzymane obr. | Flaszki/podejście | Najniższe HP (śr.) |
|---|---|---|---|---|---|---|---|---|---|
| Tester QA | 3 | 0 (0%) | 10,0 | 10,0 | 20,6 min | 301,0 | 70855 / 2679 | 4,3 | 0% |
| Gracz H&S / souls-like | 3 | 0 (0%) | 9,7 | 10,0 | 20,9 min | 294,3 | 69889 / 2486 | 4,7 | 1% |

Przebieg podejść (piętro końcowe; ★ – zwycięstwo, ⏱ – limit czasu):

- Tester QA: 10 10 10
- Gracz H&S / souls-like: 11 8 10

## Gdzie i od czego giną

- **Tester QA** — piętra: 10. X. Rzeźnia ×3.
  Ostatni cios: Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×2; Ofiarnik – „Ofiarnik” ×1. Śmierć ciosem ≥40% maks. HP: 0/3. Zginął mając flaszki: 3/3.
  Ostatnie 3 s przed śmiercią: średnio 42% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/3). Picie: 57 prób, 13 wypitych (77% przerwanych trafieniem).
- **Gracz H&S / souls-like** — piętra: 10. X. Rzeźnia ×1, 11. XI. Rzeźnia ×1, 8. VIII. Zatopione Krużganki ×1.
  Ostatni cios: Wiedźma wichru – „Wiedźma wichru: CycloneEye” ×1; Kolos – „Kolos: Combo” ×1; Harpia – „Harpia” ×1. Śmierć ciosem ≥40% maks. HP: 0/3. Zginął mając flaszki: 3/3.
  Ostatnie 3 s przed śmiercią: średnio 48% maks. HP straconych (śmierć z ≥50% HP w 3 s: 1/3). Picie: 64 prób, 14 wypitych (78% przerwanych trafieniem).

## Piętra

| Osobowość | Piętro | Dotarło | Ukończyło | Śr. czas | Śr. otrzymane obr. | Śr. flaszki | Śr. zabójstwa |
|---|---|---|---|---|---|---|---|
| Tester QA | 1. I. Wrota Otchłani | 3 | 3 | 94 s | 0 | 0,0 | 24,0 |
| Tester QA | 2. II. Kult Bramy | 3 | 3 | 109 s | 0 | 0,0 | 43,3 |
| Tester QA | 3. III. Władca Bramy | 3 | 3 | 34 s | 65 | 0,0 | 7,0 |
| Tester QA | 4. IV. Psiarnia Piekieł | 3 | 3 | 118 s | 0 | 0,0 | 43,0 |
| Tester QA | 5. V. Psiarnia Piekieł | 3 | 3 | 93 s | 3 | 0,0 | 42,0 |
| Tester QA | 6. VI. Pan Sfory | 3 | 3 | 63 s | 248 | 1,3 | 9,0 |
| Tester QA | 7. VII. Zatopione Krużganki | 4 | 3 | 123 s | 83 | 0,0 | 41,5 |
| Tester QA | 8. VIII. Zatopione Krużganki | 4 | 3 | 146 s | 304 | 0,8 | 50,3 |
| Tester QA | 9. IX. Królowa Topieli | 4 | 3 | 118 s | 205 | 0,3 | 2,3 |
| Tester QA | 10. X. Rzeźnia | 9 | 0 | 0 s | 524 | 0,6 | 27,4 |
| Gracz H&S / souls-like | 1. I. Wrota Otchłani | 3 | 3 | 107 s | 0 | 0,0 | 24,0 |
| Gracz H&S / souls-like | 2. II. Kult Bramy | 3 | 3 | 112 s | 0 | 0,0 | 42,3 |
| Gracz H&S / souls-like | 3. III. Władca Bramy | 3 | 3 | 36 s | 3 | 0,0 | 5,0 |
| Gracz H&S / souls-like | 4. IV. Psiarnia Piekieł | 3 | 3 | 117 s | 0 | 0,0 | 42,7 |
| Gracz H&S / souls-like | 5. V. Psiarnia Piekieł | 3 | 3 | 84 s | 0 | 0,0 | 34,3 |
| Gracz H&S / souls-like | 6. VI. Pan Sfory | 3 | 3 | 64 s | 23 | 0,0 | 9,0 |
| Gracz H&S / souls-like | 7. VII. Zatopione Krużganki | 3 | 3 | 147 s | 9 | 0,0 | 44,3 |
| Gracz H&S / souls-like | 8. VIII. Zatopione Krużganki | 6 | 2 | 140 s | 403 | 0,5 | 39,0 |
| Gracz H&S / souls-like | 9. IX. Królowa Topieli | 2 | 2 | 170 s | 472 | 1,5 | 3,0 |
| Gracz H&S / souls-like | 10. X. Rzeźnia | 4 | 1 | 118 s | 396 | 0,3 | 36,0 |
| Gracz H&S / souls-like | 11. XI. Rzeźnia | 3 | 0 | 0 s | 803 | 2,3 | 45,0 |

Po śmierci bot powtarza piętro z zapisu stanu z jego początku (do 3 prób); „Dotarło” liczy każdą próbę.

**Tester QA** – ponowne próby: piętro 7 ×1, piętro 8 ×1, piętro 9 ×1, piętro 10 ×6; wyzwań (3 porażki na piętrze): 3 – piętro 10.
**Gracz H&S / souls-like** – ponowne próby: piętro 8 ×3, piętro 10 ×2, piętro 11 ×2; wyzwań (3 porażki na piętrze): 1 – piętro 11.

## Przeciwnicy (obie osobowości łącznie)

| Wróg | Zabici | Śr. czas zabicia | Śr. trafień do zabicia | Trafienia w bohatera | Obrażenia dla bohatera | Obr. na zabitego | Zabił bohatera |
|---|---|---|---|---|---|---|---|
| Wiedźma wichru | 68 | 2,9 s | 3,3 | 535 | 2321 | 34 | 3 |
| Kultysta topieli | 66 | 2,5 s | 2,7 | 292 | 2156 | 32 | 0 |
| Ogar otchłani | 235 | 1,4 s | 2,1 | 196 | 1627 | 7 | 0 |
| Królowa Topieli | 5 | 134,5 s | 134,4 | 277 | 1533 | 307 | 0 |
| Kolos | 62 | 7,3 s | 5,7 | 34 | 1457 | 23 | 1 |
| Miotacz głazów | 3 | 3,9 s | 3,3 | 68 | 1328 | 443 | 0 |
| Taranowiec | 101 | 3,0 s | 3,5 | 57 | 815 | 8 | 0 |
| Harpia | 57 | 1,6 s | 2,4 | 149 | 593 | 10 | 1 |
| Kultysta | 243 | 1,3 s | 2,0 | 107 | 590 | 2 | 0 |
| Pan Sfory | 6 | 59,0 s | 57,0 | 115 | 567 | 95 | 0 |
| Legionista | 73 | 9,7 s | 8,9 | 62 | 481 | 6 | 0 |
| Czerw | 597 | 0,5 s | 1,4 | 54 | 377 | 1 | 0 |
| Gnilec | 17 | 10,4 s | 7,1 | 21 | 316 | 19 | 0 |
| Siarkowy pomiot | 133 | 0,5 s | 1,7 | 11 | 296 | 2 | 0 |
| Gargulec | 6 | 4,3 s | 5,7 | 11 | 209 | 30 | 0 |
| Władca Bramy | 6 | 31,1 s | 33,2 | 57 | 204 | 34 | 0 |
| Ofiarnik | 17 | 2,1 s | 3,0 | 59 | 191 | 11 | 1 |
| Kamienny chochlik | 108 | 0,9 s | 2,0 | 12 | 146 | 1 | 0 |
| Gnilec (odłamek) | 28 | 3,1 s | 2,5 | 22 | 85 | 3 | 0 |
| Ognisty chochlik | 103 | 1,4 s | 2,8 | 14 | 85 | 1 | 0 |
| Wodny chochlik | 110 | 0,8 s | 2,1 | 7 | 48 | 0 | 0 |
| Tkaczka | 2 | 6,6 s | 2,0 | 15 | 39 | 20 | 0 |
| Chochlik wichru | 126 | 1,0 s | 2,5 | 5 | 19 | 0 | 0 |
| Pajączek | 12 | 0,0 s | 1,0 | 7 | 10 | 1 | 0 |
| Ożywiony nietoperz | 30 | 0,3 s | 1,1 | 3 | 0 | 0 | 0 |
| Ogar otchłani (elita) | 10 | 3,8 s | 7,5 | ↑ | ↑ | ↑ |  |
| Siarkowy pomiot (elita) | 6 | 2,0 s | 4,8 | ↑ | ↑ | ↑ |  |
| Kultysta (elita) | 3 | 3,6 s | 5,3 | ↑ | ↑ | ↑ |  |
| Taranowiec (elita) | 1 | 16,7 s | 7,0 | ↑ | ↑ | ↑ |  |
| Ognisty chochlik (elita) | 4 | 6,8 s | 11,8 | ↑ | ↑ | ↑ |  |
| Kolos (elita) | 2 | 17,6 s | 24,0 | ↑ | ↑ | ↑ |  |
| Legionista (elita) | 2 | 20,0 s | 24,0 | ↑ | ↑ | ↑ |  |
| Czerw (elita) | 2 | 1,3 s | 3,0 | ↑ | ↑ | ↑ |  |
| Gargulec (elita) | 1 | 10,8 s | 10,0 | ↑ | ↑ | ↑ |  |
| Worek jaj | 5 | – | – | 0 | 0 | 0 | 0 |
| Harpia (elita) | 1 | 7,6 s | 9,0 | ↑ | ↑ | ↑ |  |
| Kultysta topieli (elita) | 1 | 26,0 s | 10,0 | ↑ | ↑ | ↑ |  |

## Obrona: nadchodzące ciosy

**Tester QA** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 283 | 57% | 0% | 0% | 43% | 0% |
| ciężki | 31 | 48% | 0% | 0% | 52% | 0% |
| nieparowalny | 245 | 43% | 0% | 0% | 57% | 0% |
| nieblokowalny | 18 | 78% | 0% | 0% | 22% | 0% |
| obszarowy | 385 | 100% | 0% | 0% | 0% | 0% |

**Gracz H&S / souls-like** — wynik ciosu wg koloru telegrafu:

| Telegraf | Razem | trafienie | blok | parowanie | unik | przełamana garda |
|---|---|---|---|---|---|---|
| zwykły | 391 | 37% | 0% | 0% | 63% | 0% |
| ciężki | 35 | 26% | 0% | 0% | 74% | 0% |
| nieparowalny | 207 | 25% | 0% | 0% | 75% | 0% |
| nieblokowalny | 28 | 71% | 0% | 0% | 29% | 0% |
| obszarowy | 567 | 100% | 0% | 0% | 0% | 0% |

**Zapas czasu na obronę** – ile zostało do trafienia, gdy bot (po ludzkim czasie reakcji) zdecydował o reakcji:

| Osobowość (reakcja) | Telegraf | Decyzji | spóźniony (<0 s) | 0–0,1 s | 0,1–0,2 s | 0,2–0,4 s | >0,4 s |
|---|---|---|---|---|---|---|---|
| Tester QA (0,30 s) | zwykły | 299 | 28% | 31% | 3% | 20% | 18% |
| Tester QA (0,30 s) | ciężki | 65 | 0% | 2% | 3% | 12% | 83% |
| Tester QA (0,30 s) | nieparowalny | 146 | 8% | 3% | 14% | 32% | 44% |
| Tester QA (0,30 s) | nieblokowalny | 5 | 0% | 0% | 20% | 0% | 80% |
| Tester QA (0,30 s) | obszarowy | 233 | 1% | 1% | 1% | 50% | 46% |
| Tester QA (0,30 s) | pocisk | 410 | 3% | 13% | 11% | 14% | 59% |
| Gracz H&S / souls-like (0,20 s) | zwykły | 390 | 4% | 26% | 29% | 11% | 30% |
| Gracz H&S / souls-like (0,20 s) | ciężki | 71 | 0% | 1% | 1% | 11% | 86% |
| Gracz H&S / souls-like (0,20 s) | nieparowalny | 111 | 9% | 0% | 5% | 30% | 56% |
| Gracz H&S / souls-like (0,20 s) | nieblokowalny | 6 | 0% | 0% | 0% | 17% | 83% |
| Gracz H&S / souls-like (0,20 s) | obszarowy | 266 | 2% | 1% | 5% | 31% | 62% |
| Gracz H&S / souls-like (0,20 s) | pocisk | 661 | 3% | 11% | 9% | 12% | 65% |

Najkrótsze zamachy wrogów (średni czas od początku zamachu do fazy aktywnej, z tempem i mnożnikami gry):

| Wróg: atak | Śr. zamach | Obserwacji |
|---|---|---|
| Ogar otchłani: Ogar otchłani | 0,28 s | 148 |
| Pajączek: Pajączek | 0,31 s | 20 |
| Wodny chochlik: Pazury | 0,33 s | 25 |
| Czerw: Ugryzienie | 0,33 s | 242 |
| Chochlik wichru: Pazury | 0,33 s | 32 |
| Kamienny chochlik: Pazury | 0,33 s | 32 |
| Ożywiony nietoperz: Ukąszenie | 0,33 s | 16 |
| Ognisty chochlik: Pazury | 0,33 s | 42 |
| Wiedźma wichru: Wiedźma wichru | 0,44 s | 281 |
| Królowa Topieli: Królowa Topieli: WindStep | 0,45 s | 19 |

**Macierz testera** — wybrana reakcja × telegraf → odsetek ciosów, które mimo to zraniły (trafienie lub przełamana garda):

| Reakcja | zwykły | ciężki | nieparowalny | nieblokowalny | obszarowy | pocisk |
|---|---|---|---|---|---|---|
| brak reakcji (eksperyment) | 100% (n=13) | 60% (n=5) | 100% (n=10) | 100% (n=4) | 100% (n=16) | 62% (n=13) |
| niezauważony | 85% (n=62) | – | 91% (n=23) | 100% (n=6) | 100% (n=187) | – |
| strefa/wybuch po pocisku | – | – | – | – | – | 50% (n=8) |
| unik | 47% (n=70) | 35% (n=17) | 15% (n=114) | 0% (n=2) | – | 31% (n=54) |
| unik (eksperyment) | 44% (n=16) | 50% (n=4) | 35% (n=20) | 0% (n=2) | 100% (n=36) | 38% (n=8) |
| wyjście z zasięgu | 52% (n=21) | – | 88% (n=8) | – | 100% (n=115) | 51% (n=41) |
| wyjście z zasięgu (eksperyment) | 75% (n=20) | 80% (n=5) | 94% (n=16) | 100% (n=3) | 100% (n=31) | 58% (n=12) |

## Uniki

Ile razy bot nacisnął unik i dlaczego, ile uników gra faktycznie wykonała, ile odmówiła, oraz **skuteczność**: odsetek naciśnięć, po których bohater nie oberwał tym ciosem/pociskiem (chybienie po przewrocie liczy się jako sukces). Tyknięcia płonącej ziemi po kuli ognia nie są liczone jako nieudany unik.

| Osobowość | Uniki wykonane/podejście | Naciśnięcia wg powodu (na podejście) | Odmowy gry (na podejście) | Skuteczność: cios wręcz / pocisk |
|---|---|---|---|---|
| Tester QA | 145,7 | reakcja na cios wręcz 129,0, unik przed pociskiem 56,7, wyjście z okrążenia 12,3 | drgnięcie (trafiony) 21,7, w trakcie uniku 12,7, brak wytrzymałości 7,3, picie flaszki 3,7 | 72% (n=387) / 88% (n=170) |
| Gracz H&S / souls-like | 199,7 | reakcja na cios wręcz 153,0, unik przed pociskiem 116,7, doskok do odsłoniętego celu 12,7, wyjście z okrążenia 4,3 | w trakcie uniku 39,0, drgnięcie (trafiony) 23,0, brak wytrzymałości 10,3, picie flaszki 2,0, unieruchomienie 0,7, inna 0,7 | 83% (n=459) / 91% (n=350) |

## Zestawy broni i skille startowe

| Osobowość | Zestaw | Skill startowy | Podejścia | Zwycięstwa | Śr. piętro | Śr. zabójstwa |
|---|---|---|---|---|---|---|
| Tester QA | Ogień | Nova ognia | 1 | 0 | 10,0 | 297,0 |
| Tester QA | Woda | Nova wody | 1 | 0 | 10,0 | 273,0 |
| Tester QA | Powietrze | Nova powietrza | 1 | 0 | 10,0 | 333,0 |
| Gracz H&S / souls-like | Ogień | Nova ognia | 1 | 0 | 11,0 | 384,0 |
| Gracz H&S / souls-like | Powietrze | Nova powietrza | 1 | 0 | 10,0 | 275,0 |
| Gracz H&S / souls-like | Woda | Nova wody | 1 | 0 | 8,0 | 224,0 |

## Umiejętności i źródła obrażeń

**Tester QA** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Różdżka wody | 39749 | 19% |  |  |
| Różdżka ognia | 34684 | 16% |  |  |
| Różdżka powietrza | 34430 | 16% |  |  |
| Nova wody | 27007 | 13% | 130 | 208 |
| Nova powietrza | 25245 | 12% | 90 | 281 |
| Ściana ognia | 18472 | 9% | 101 | 183 |
| Nova ognia | 13000 | 6% | 58 | 224 |
| Odskok widma | 11386 | 5% |  |  |
| Pocisk arkanów | 8592 | 4% | 120 | 72 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Przeciążenie ×149, Kojące światło ×24.

**Gracz H&S / souls-like** — źródła obrażeń (top 10):

| Źródło | Obrażenia | Udział | Użycia skilla | Obr./użycie |
|---|---|---|---|---|
| Nova ognia | 44356 | 21% | 85 | 522 |
| Lodowy podmuch | 36084 | 17% | 150 | 241 |
| Różdżka ognia | 35011 | 17% |  |  |
| Różdżka powietrza | 31263 | 15% |  |  |
| Różdżka wody | 31077 | 15% |  |  |
| Odskok widma | 9742 | 5% |  |  |
| Ściana ognia | 6977 | 3% | 22 | 317 |
| Pocisk arkanów | 6679 | 3% | 83 | 80 |
| Nova powietrza | 4638 | 2% | 25 | 186 |
| Przyzwanie chochlików | 1628 | 1% | 5 | 326 |

Użyte bez zapisanych obrażeń bezpośrednich (wsparcie, aury, efekty w czasie): Przeciążenie ×95.

## Kolekcja skilli: różnorodność, sklep, zamiany

Pochodzenie: **startowy** – skill startowy wybrany przed podejściem, **odblokowanie** – kupiony na stałe za popiół, **zestaw** – z zestawu startowego, **klasa** – skill klasy pod osobnym przyciskiem, **nagroda** – wybrany po piętrze, **sklep** – oferta sklepu dusz, **przedmiot** – z broni. „?” – serie sprzed oznaczania skilla klasy.

**Tester QA** — różnych skilli użytych na podejście: średnio 4,3, mediana 4,0, najwięcej 5 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 33% | 73,7 | 122 | 0,3 |
| klasa | 22% | 49,7 | – | 0,1 |
| sklep | 4% | 8,0 | – | 0,7 |
| przedmiot | 41% | 92,7 | 235 | 0,4 |

Oferty sklepu dusz: widziane 81, kupione 1, odrzucone 0, za mało dusz 26.
Zamiany w slotach: 2,0 na podejście; najczęstsze: Ściana ognia → Nova ognia ×2, Nova ognia → Kojące światło ×1, Pocisk arkanów → Nova ognia ×1, Nova ognia → Pocisk arkanów ×1, Nova ognia → Ściana ognia ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Przeciążenie | klasa | 3 | 49,7 | – | 0,1 |
| Nova wody | przedmiot | 1 | 130,0 | 208 | 0,3 |
| Pocisk arkanów | odblokowanie | 3 | 40,0 | 72 | 0,4 |
| Ściana ognia | odblokowanie | 3 | 33,7 | 183 | 0,1 |
| Nova powietrza | przedmiot | 1 | 90,0 | 281 | 0,7 |
| Nova ognia | przedmiot | 1 | 58,0 | 224 | 0,0 |
| Kojące światło | sklep | 1 | 24,0 | – | 0,7 |

**Gracz H&S / souls-like** — różnych skilli użytych na podejście: średnio 5,7, mediana 6,0, najwięcej 7 (3 sloty + skill klasy, zamiany między piętrami dokładają kolejne).

| Pochodzenie | Udział użyć | Użycia/podejście | Obr./użycie | Śr. poziom przy użyciu |
|---|---|---|---|---|
| odblokowanie | 56% | 91,7 | 186 | 0,5 |
| klasa | 19% | 31,7 | – | 0,1 |
| sklep | 1% | 1,7 | 326 | 0,0 |
| przedmiot | 24% | 39,0 | 426 | 0,6 |

Oferty sklepu dusz: widziane 78, kupione 1, odrzucone 1, za mało dusz 24.
Zamiany w slotach: 3,0 na podejście; najczęstsze: Pocisk arkanów → Kula ognia ×2, Szarża → Lodowy podmuch ×1, Nova wody → Pocisk arkanów ×1, Szarża → Mroźna fala ×1, Nova powietrza → Przyzwanie chochlików ×1, Przyzwanie chochlików → Lodowy podmuch ×1.

| Skill | Pochodzenie (najczęstsze) | Podejścia z użyciem | Użycia/podejście z użyciem | Obr./użycie | Śr. poziom |
|---|---|---|---|---|---|
| Lodowy podmuch | odblokowanie | 2 | 75,0 | 241 | 0,5 |
| Przeciążenie | klasa | 3 | 31,7 | – | 0,1 |
| Nova ognia | przedmiot | 1 | 85,0 | 522 | 0,6 |
| Pocisk arkanów | odblokowanie | 3 | 27,7 | 80 | 0,4 |
| Nova powietrza | przedmiot | 1 | 25,0 | 186 | 0,7 |
| Ściana ognia | odblokowanie | 1 | 22,0 | 317 | 0,4 |
| Mroźna fala | odblokowanie | 1 | 8,0 | 86 | 0,0 |
| Szarża | odblokowanie | 1 | 7,0 | 38 | 0,0 |
| Nova wody | przedmiot | 1 | 7,0 | 121 | 0,0 |
| Kula ognia | odblokowanie | 2 | 2,5 | 83 | 0,0 |
| Przyzwanie chochlików | sklep | 1 | 5,0 | 326 | 0,0 |

## Akcje, nagrody, zakupy

**Tester QA** — akcje na podejście: szybki atak 647,0, umiejętność 224,0, unik 145,7, drgnięcie (trafiony) 77,0, flaszka (próba) 19,0, flaszka many (próba) 11,0.
Czas w bloku 0 s, w biegu 45 s, bez wytrzymałości (<10%) 94 s na podejście.
Zakupy: założony przedmiot: Sandały pielgrzyma ×3, dodatkowa flaszka ×3, założony przedmiot: Jedwabne rękawiczki ×2, sklep: Kruszenie ×2, ulepszenie: Pocisk arkanów ×2, sklep: Sandały pielgrzyma ×2, ulepszenie: Nova wody ×2, założony przedmiot: Kolcze nogawice ×2.
Punkty cech: Intelligence 9, Toughness 6, Dexterity 6, Strength 6.
Pieczęcie na podejście: z daru bossa 3,0, z sakwy do skilli 2,0, kupione 0,7; najczęstsze: Pakt z diabłem ×2, Wieczny płomień ×2, Klątwa Mokoszy ×2, Przemiana: woda ×1, Nieskończoność ×1, Wstrząs ×1.
Komunikaty gry: „Sloty przegrzane” ×2113; „PRZECIĄŻENIE” ×143; „WYŁADOWANIE!” ×96; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×29; „Portale gasną – dobij pozostałe demony” ×22; „ZAMROŻENIE!” ×12; „POLEGŁEŚ” ×12; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×9.

**Gracz H&S / souls-like** — akcje na podejście: szybki atak 540,7, unik 199,7, umiejętność 164,0, drgnięcie (trafiony) 58,7, flaszka (próba) 21,3, flaszka many (próba) 10,7.
Czas w bloku 0 s, w biegu 47 s, bez wytrzymałości (<10%) 120 s na podejście.
Zakupy: sklep: Szeroki zasięg ×4, sklep: Pancerz ×3, sklep: Rykoszet ×3, założony przedmiot: Kolczy czepiec ×2, sklep: Skupienie ×2, ulepszenie: Pocisk arkanów ×2, założony przedmiot: Kolcze rękawice ×2, dodatkowa flaszka ×2.
Punkty cech: Intelligence 18, Toughness 8.
Pieczęcie na podejście: z daru bossa 2,7, z sakwy do skilli 1,0, kupione 0,3; najczęstsze: Pakt z diabłem ×2, Nieskończoność ×2, Oko Czarnoboga ×1, Zemsta ×1, Narastanie ×1, Przekaźnik ×1.
Odblokowania za popiół między podejściami: Szarża, Kamienna osłona, Kula ognia, Lodowy podmuch, Płomienne ostrze, Puklerz, Kontra, Mroźna fala.
Komunikaty gry: „Sloty przegrzane” ×421; „PRZECIĄŻENIE” ×90; „WYŁADOWANIE!” ×59; „Portale się otwierają – zabijaj, zanim zrobi się ciasno!” ×28; „Portale gasną – dobij pozostałe demony” ×22; „POLEGŁEŚ” ×10; „OGŁUSZENIE!” ×9; „DAR BOSSA: trzy pieczęcie do wyboru w sklepie” ×8.

## Anomalie i niezmienniki

- **Tester QA**: trafienie z tyłu (poza polem widzenia) ×89, uśpiony wróg blokuje koniec piętra ×1.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść
- **Gracz H&S / souls-like**: trafienie z tyłu (poza polem widzenia) ×123, uśpiony wróg blokuje koniec piętra ×3, zablokowany ruch ×1.
  - piętro 10 „X. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść (w 2 podejściach)
  - piętro 8 „VIII. Zatopione Krużganki”: brak postępu przy (-7,8; 0,0)
  - piętro 11 „XI. Rzeźnia”: Gargulec czeka uśpiony – trzeba do niego podejść

## Wnioski testera

- Log czysty w podejściach testera: ani jednego błędu ani wyjątku w 3 podejściach (błędy z serii gracza – w sekcji anomalii).
- Reguły telegrafów trzymają się: żaden nieblokowalny cios nie został zablokowany, obszarowy – uniknięty, nieparowalny – sparowany.
- Unik przed ciężkim: działa – rani tylko 38% ciosów (n=21).
- Wychodzenie z różowego kręgu: 100% ciosów mimo to rani (n=146) – przy ludzkiej reakcji ok. 0,3 s okno jest zbyt ciasne albo telegraf za krótki.
- Unik przed pociskiem działa: trafia tylko 12% pocisków, przed którymi był unik (n=170).
- **Czytelność (P2):** telegraf „zwykły”: przy reakcji 0,30 s 59% decyzji zapada, gdy do trafienia zostało <0,1 s (n=299) – człowiek nie zdąży zareagować (zob. „Najkrótsze zamachy”).
- Telegraf „ciężki” daje czas: tylko 2% decyzji zapada mniej niż 0,1 s przed trafieniem (n=65).
- Telegraf „nieparowalny” daje czas: tylko 11% decyzji zapada mniej niż 0,1 s przed trafieniem (n=146).
- Telegraf „obszarowy” daje czas: tylko 3% decyzji zapada mniej niż 0,1 s przed trafieniem (n=233).
- **Pokrycie:** nie udało się wykonać: mocny atak, riposta, blok, parowanie – sprawdzić warunki (np. riposta wymaga złamanej postawy).
- UX: gra odmawiała akcji: „Sloty przegrzane” ×2113 – HUD powinien to sygnalizować zanim gracz naciśnie.
- Koszt symulacji w trybie testów: średnio 0,6 ms/klatkę (maks. 79 ms). To pomiar edytora wsadowego, nie buildu gracza.

## Wnioski gracza (hack and slash / souls-like)

- Ani jednego zwycięstwa w 3 podejściach. Średnio dochodzę do piętra 9,7 – na razie to ściana, nie krzywa.
- Prawie nie parowałem – z tą bronią nie ma czym (lub nie było okazji).
- Gąbki na obrażenia: Legionista (9,0 trafień, 11,4 s). W H&S chcę czuć siłę ciosu.
- Mięso armatnie padające od 1–2 ciosów: Pajączek (1,0), Ożywiony nietoperz (1,2), Czerw (1,4) – fajne przy hordach, ale nie uczą niczego.
- Długie starcie: Pan Sfory – średnio 59 s i 50 trafień do zabicia.
- Długie starcie: Królowa Topieli – średnio 166 s i 110 trafień do zabicia.
- 3/3 razy zginąłem z flaszkami w kieszeni. 1 z tych śmierci to utrata ≥50% HP w 3 s, a 78% prób picia przerwał cios (flaszka działa dopiero po wypiciu). W gęstym tłumie nie ma okna na leczenie.
- Skille robią 49% obrażeń – zdrowa proporcja między bronią a umiejętnościami.
- Najmocniej czuć „Nova ognia”: 522 obrażeń na użycie.
- Skill startowy to 0% moich użyć skilli, zdobyte w trakcie podejścia (nagrody, sklep) – 1%. Różnych skilli na podejście: średnio 5,7.
- Najczęściej grałem: Ogień + Nova ognia (1×, śr. piętro 11,0). Inne: Woda + Nova wody 1× (8,0), Powietrze + Nova powietrza 1× (10,0).
- Unik to moje główne narzędzie: 199,7 na podejście, w tym 17,0 z inicjatywy (wyjście z okrążenia, doskok). Po uniku przed pociskiem nie obrywam w 91% przypadków.
- Gra nie wykonała uniku od razu średnio 75,7× na podejście – najczęściej: w trakcie poprzedniego uniku 39,0, drgnięcie (trafiony) 23,0, brak wytrzymałości 10,3. Część z tego to tylko opóźnienie (bufor wejścia), ale w drgnięciu po trafieniu unik nie wychodzi wcale.
- Dużo drgnięć po trafieniu (59 na podejście) – łańcuchy ciosów wrogów potrafią mnie zablokować.

