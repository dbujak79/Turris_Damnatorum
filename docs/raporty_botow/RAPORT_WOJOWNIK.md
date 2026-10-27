# Boty grające — raport: Wojownik (2026-10-27)

Dwa boty dedykowane klasie **Wojownik** (`class_knight`) rozegrały po **20 pełnych podejść** na prawdziwej scenie `Main`
(trudność podstawowa, treść z `GameConfig`, stały krok 1/60 s). Grają wyłącznie tym, co ma gracz: ruch, garda, bieg,
namierzanie i przyciski akcji. Na początek zamachu wroga reagują z opóźnieniem i błędem wyczucia człowieka.

| Bot | Kim jest | Reakcja |
|---|---|---|
| **Tester QA** | Gra poprawnie, ale ~30% ciosów przyjmuje celowo każdą reakcją (blok, parowanie, unik, wyjście z zasięgu, brak reakcji). Rotuje 2 zestawy × 2 skille startowe po 5 podejść, sprawdza niezmienniki reguł, szuka błędów w logu i zablokowań na geometrii. Bez meta-progresji. | 0,30 s ± 0,08, błąd wyczucia σ = 60 ms |
| **Gracz H&S / souls-like** | Namierza, paruje zwykłe ciosy, robi uniki przed czerwonymi i karze odsłonięcia seriami 2–3 ciosów. Przy grupach używa skilli, pilnuje wytrzymałości i ripostuje. Między podejściami wydaje popiół, a zestaw dobiera według dotychczasowych wyników. | 0,20 s ± 0,06, σ = 45 ms |

Pełne tabele wygenerowane przez boty (wraz z ich własnymi wnioskami): [wojownik/RAPORT_class_knight.md](wojownik/RAPORT_class_knight.md),
surowe dane: `wojownik/class_knight_*.json`.

## Wyniki

| | Zwycięstwa | Śr. piętro | Śmierci na piętrze 2 | Śr. czas zwycięskiego podejścia |
|---|---|---|---|---|
| Tester QA | **9/20** (45%) | 4,1 | 9 z 11 | ok. 6 min gry |
| Gracz H&S / souls | **11/20** (55%) | 4,2 | 9 z 9 | ok. 5 min gry |

Przebieg podejść (piętro śmierci, ★ – zwycięstwo):
- Tester: 5 ★ 5 2 ★ ★ 2 2 2 ★ 2 ★ 2 ★ 2 ★ 2 ★ 2 ★
- Gracz: 2 ★ 2 ★ ★ 2 ★ ★ 2 ★ 2 2 ★ ★ 2 2 ★ 2 ★ ★

Przy 20 podejściach różnica między botami (45% wobec 55%) mieści się w szumie (±11 p.p.). Wzorzec obu serii jest taki sam.

## Najważniejsze wnioski

### 1. Wieża ma jedną ścianę: piętro 2 „Kult Bramy”, a finał jest za łatwy (balans, P1)
- **18 z 20 śmierci** przypada na piętro 2. Po przejściu piętra 2 boty wygrywają **20 z 22** podejść.
- Ostatni boss jest za słaby: **Kasztelan Potępionych** pada średnio po **13 s** i ok. 14 trafieniach. Finałowe piętro trwa 15–21 s
  i zabiera bohaterowi 16–70 HP.
- **Władca Bramy** (boss piętra 3) jest trzy razy wytrzymalszy od finałowego bossa: 46 s i 36 trafień. Kolejność napięcia jest odwrócona.
- Piętro 1 to praktycznie samouczek: 30 zabitych, średnio **40 HP** otrzymanych obrażeń, zero flaszek.

### 2. Kultysta z „Kulą ognia” zabija najczęściej, a skill startowy decyduje o całym podejściu (balans, P1)
- **Kultysta** zadał **31%** wszystkich obrażeń otrzymanych przez bohatera i zadał ostatni cios w **12 z 20** śmierci.
  Sam pada od 1,1 trafienia, ale na piętrze 2 wychodzi z portali w dużej liczbie i ostrzeliwuje bohatera z dystansu.
- Skill startowy przesądza o wyniku:

  | Skill startowy | Zwycięstwa (obie serie) |
  |---|---|
  | **Cięcie wichru** (fala dystansowa) | **19/26 (73%)** |
  | **Rozpłatanie** (stożek wręcz) | **1/14 (7%)** |

  Zestaw broni ma znaczenie drugorzędne. Tester z mieczem, tarczą i Cięciem wichru wygrał 5/5, z mieczem, tarczą i Rozpłataniem — 1/5.
  Rozpłatanie nie daje odpowiedzi na strzelców, więc ten wybór jest dziś pułapką dla nowego gracza.
- Tarcza nie rozwiązuje problemu kul ognia: zablokowane kule nadal ranią w 34% przypadków (garda z boku, przełamanie, kilku strzelców naraz).

### 3. Zwykłe ciosy hordy są za szybkie na parowanie (czytelność, P2)
- Zamachy pomniejszych wrogów (pazury chochlików, ugryzienie czerwia, nietoperz, pajączek) trwają **0,31–0,33 s**
  (z tempem gry 1,1). Przy reakcji 0,3 s **68%** decyzji o obronie zapada, gdy do trafienia zostało mniej niż 0,1 s.
  Nawet doświadczony gracz (0,2 s) w 81% przypadków ma na to najwyżej 0,2 s, a parowanie potrzebuje ok. 0,17 s.
- Skutek: parowanie zwykłych ciosów udaje się graczowi souls w 55% prób, testerowi w 30%. Riposty zdarzają się
  **0,4–0,6 razy na podejście**, choć „złamana postawa” pojawia się kilkanaście razy na serię.
- Telegrafy ciężkie, czerwone i fioletowe działają bardzo dobrze: w ~90% przypadków zostaje >0,4 s. Unik przed ciężkim ciosem rani w 0% przypadków.
  Rdzeń souls-like („czytaj i karz”) działa więc na bossach i elitach, a nie działa na hordzie.

### 4. Leczenie: boty giną z flaszkami w kieszeni (P2)
- 15 z 20 śmierci nastąpiło z niewypitą flaszką. To nie są nagłe zgony: żadna śmierć nie poprzedziła utrata ≥50% HP w 3 s,
  średnio w ostatnich 3 s ginie ok. 31% HP. To powolne wykrwawianie pod ciągłym ostrzałem.
- Sprawdziliśmy to eksperymentalnie, zmieniając zachowanie bota:
  - bot pijący tylko w bezpiecznym oknie dłuższym niż czas picia (0,55 s) na piętrze 2 **nie pił prawie wcale** i wygrał 0/20;
  - bot pijący jak człowiek, czyli z ryzykiem, gdy najbliższy cios jest dalej niż 0,3 s, wygrał 11/20.

  Pod ostrzałem kultystów nie ma więc okna na 0,55 s picia. Dodatkowo 14–15% prób picia przerywa trafienie.

### 5. Obrażenia pochodzą głównie ze skilli (projekt, P3, z zastrzeżeniem)
- Skille dały **70% (tester) – 84% (gracz)** obrażeń. Cięcie wichru i Rozpłatanie zadają 143–171 obrażeń na użycie,
  zwykły cios bronią średnio 70–100. Boty używają skilli przy każdej okazji, więc część tej proporcji wynika z ich stylu.
  Mimo to gracz H&S dostaje tu to, czego chce, a gracz souls czuje, że broń jest tłem.
- Wytrzymałość jest stale na wyczerpaniu: **27–29%** czasu walki poniżej 10%.

### 6. Błędy i usterki znalezione przez testera
- **Wyjątek (P1):** `NullReferenceException` w `EnemyBrain.SpecialActive` (`EnemyBrain.Specials.cs:119`, gałąź `EnemySpecial.Charge`).
  Trafienie w trakcie szarży jest rozstrzygane w callbacku `HitQuery.Capsule`. Jeśli szarża zostanie sparowana,
  `OnAttackParried` zeruje `current` (`EnemyBrain.cs:751`), po czym kod odczytuje `current.specialDuration` i `current.specialCount`.
  Błąd wystąpił w przebiegu próbnym; w końcowych 40 podejściach się nie powtórzył, więc jest rzadki. Proponowana poprawka:
  po `HitQuery.Capsule` sprawdzić `if (current == null || CurrentState != State.Attacking) return Vector3.zero;`.
- **UX (P2):** czary wymagające Inteligencji (Kojące światło, Łańcuch błyskawic, Rozprysk arkanów) trafiają do slotów Wojownika
  (Int 7): automatycznie z odblokowania stałego albo z nagrody/oferty. Gra proponuje też ich **ulepszenia**. Przed poprawką bota
  gracz 364 razy usłyszał „za mało Inteligencji”. Odblokowanie „Kojące światło” jest ogólne i zajmuje slot na starcie każdego podejścia.
- **Geometria (P3):** zablokowanie ruchu w powtarzalnych miejscach. „IV. Zbrojownia” (−7,8; 0,1) wystąpiło w dwóch przebiegach,
  do tego „II. Kult Bramy” (2,1; 0,2) i „VI. Szczyt Wieży” (0,0; −4,9).
- **Kąt gardy (P3):** 10–14% trafień przychodzi z tyłu (>110°). Pięć razy cios przeszedł przez podniesioną gardę pod kątem 125–180°
  (łucznik, harpia, nietoperz, kula kultysty). Reguła jest poprawna, ale w grupie brakuje sygnału ataku spoza kadru.
- **Niezmienniki trzymają się:** żaden nieblokowalny cios nie został zablokowany, żaden obszarowy nie został uniknięty, żaden nieparowalny
  nie został sparowany. Nie było wartości NaN ani wyjścia poza świat. W 40 końcowych podejściach log był bez błędów.

### 7. Meta-progresja słabo odczuwalna
Gracz kupił za popiół 20 odblokowań, a jego średni wynik w drugiej połowie serii nie wzrósł (5,0 → 4,5 piętra).
Większość zakupów to bronie i skille, których nie miał w slotach. Przynajmniej z perspektywy bota odblokowania dodają opcje, a nie siłę.

## Rekomendacje (kolejność wg wpływu)

1. **Piętro 2:** zmniejszyć liczbę kultystów jednocześnie aktywnych w portalach albo wydłużyć przerwę między kulami ognia.
   Alternatywnie dać Rozpłataniu odpowiedź na dystans (np. falę przy +2).
2. **Finałowy boss:** podnieść zdrowie i postawę Kasztelana tak, żeby walka trwała dłużej niż z Władcą Bramy (cel ok. 60–90 s).
   Rozważyć zamianę kolejności trudności pięter 3–6.
3. **Zamachy hordy:** 0,33 s → ok. 0,45 s albo krótszy start parowania na zwykłe ciosy. Inna opcja to otwarta decyzja projektowa,
   że hordę się wycina, a paruje tylko elity.
4. **Flaszka:** krótszy czas picia (0,55 → 0,4 s) albo krótka odporność na drgnięcie w trakcie picia.
5. **Poprawić NRE** w szarży (jedna linia, opis wyżej).
6. **Sloty:** nie wkładać automatycznie i nie proponować czarów, których klasa nie może rzucić (twarde wymaganie Inteligencji).

## Po zmianach balansu (druga seria, 2026-10-27)

Wprowadzone: piętro II średnio o dwóch kultystów mniej (budżet portali 44 → 42, waga kultysty 4,5 → 4),
Rozpłatanie +15% (39/44/44), dodatkowi wrogowie charakterystyczni: Taranowiec w 2. fali „IV. Zbrojowni”, Gargulec w ostatniej
fali „V. Bramy Strażnika”, pomoc w celowaniu z Hell Unleashed (boty i tak grają z namierzaniem, więc ich nie zmienia).
Te same boty, 2 × 20 podejść; dane: [wojownik-v2/RAPORT_class_knight.md](wojownik-v2/RAPORT_class_knight.md).

| | Przed | Po |
|---|---|---|
| Tester – zwycięstwa | 9/20 | 10/20 |
| Gracz – zwycięstwa | 11/20 | 13/20 |
| Śmierci na piętrze II | 18 z 20 | 13 z 17 |
| Rozpłatanie jako skill startowy | 1/14 | 1/13 |
| Cięcie wichru jako skill startowy | 19/26 | 22/27 |

- Piętro II nadal jest główną ścianą, ale łagodniejszą – przy 20 podejściach różnica jest na granicy szumu.
- **+15% dla Rozpłatania nie wystarczyło:** problemem jest brak odpowiedzi na strzelców, nie same obrażenia.
- Taranowiec (śr. 2,2 trafienia, 1,5 s) i Gargulec (3,3 trafienia, 2,1 s) są dla wojownika lekkim dodatkiem, nie zagrożeniem.
- **Gargulec jest „posągiem”** budzonym z 5,4 m lub uderzeniem (mechanika HU). Piętro V nie kończy się, dopóki ktoś do niego
  nie podejdzie – boty (po poprawce) musiały go szukać w 19 podejściach. Warto rozważyć znacznik na HUD dla uśpionych wrogów.
- **Błąd szarży naprawiony:** Taranowiec szarżuje, więc NRE po sparowaniu szarży (`EnemyBrain.Specials.cs:119`) pojawiał się częściej –
  poprawka w `EnemyBrain.SpecialActive`/`TickAttack` i test `DemonPlayTests.ParriedCharge_EndsAttack_WithoutException`
  (oblewa bez poprawki, przechodzi z nią).

## Trzecia seria: −1 kultysta, szybsze Rozpłatanie, lepsze uniki botów (2026-10-27)

Zmiany w grze: piętro II – budżet 41, waga kultysty 3,7 (średnio 13,9 kultysty, łącznie o trzech mniej niż na starcie);
Rozpłatanie – odnowienie 5 → 3,5 s (−30%). Zmiany w botach: unik przed pociskiem liczony z prawdziwego pocisku i chwili **styku**
(wcześniej bot szacował lot i reagował ok. 0,1 s za późno – skuteczność 7–20%), uniki z inicjatywy (wyjście z okrążenia, doskok),
telemetria uników. Dane: [wojownik-v3/RAPORT_class_knight.md](wojownik-v3/RAPORT_class_knight.md).

| | Seria 1 | Seria 2 | Seria 3 |
|---|---|---|---|
| Tester – zwycięstwa | 9/20 | 10/20 | **17/20** |
| Gracz – zwycięstwa | 11/20 | 13/20 | **14/20** |
| Śmierci na piętrze II | 18 z 20 | 13 z 17 | 7 z 9 |
| Rozpłatanie jako skill startowy | 1/14 | 1/13 | **12/16** |
| Cięcie wichru jako skill startowy | 19/26 | 22/27 | 19/24 |

Seria 3 łączy zmiany gry i lepsze uniki botów, więc jej skok nie jest w całości zasługą balansu.

**Uniki (pytanie użytkownika: czy boty aktywnie z nich korzystają?)** – tak, to ich główna obrona:

| | Tester | Gracz souls |
|---|---|---|
| Wykonane uniki / podejście | 17,9 | 39,6 |
| Reakcja na cios wręcz / na pocisk | 15,9 / 8,7 | 33,8 / 13,8 |
| Z inicjatywy (okrążenie, doskok) | 0,5 | 4,2 |
| Skuteczność wręcz / pocisk (bez obrażeń po uniku) | 66% / 65% | 82% / 78% |
| Gra nie wykonała uniku od razu | 6,4 | 9,8 |

- Unik działa dobrze przed ciężkimi i czerwonymi ciosami (rani 14–28%) oraz – po poprawce bota – przed kulami ognia.
- Odmowy to głównie **drugi unik w trakcie pierwszego** (5,6× u gracza; część wykonuje się z bufora 0,3 s) i **drgnięcie po trafieniu**
  (ok. 2×; w drgnięciu unik nie wychodzi wcale – zasada souls, ale przy ostrzale z kilku stron to łańcuch trafień bez wyjścia).
  Brak wytrzymałości tylko ok. 1×.
- Blok pocisku jest słaby: 80% zablokowanych kul nadal rani (garda pod kątem 120–140°, kilku strzelców) – przeciw kultystom unik
  jest wyraźnie lepszy od tarczy.
- Gracz souls po tej serii: „14/20 – za łatwo”. Wieża z dobrym unikaniem przestaje być wyzwaniem; ewentualne dalsze ruchy
  balansu warto mierzyć tą wersją botów.

## Czwarta seria: Rozpłatanie z powaleniem, odnowienie 4 s (2026-10-27)

Rozpłatanie powala każdego zranionego wroga na 1 s (`down_seconds`, ten sam mechanizm co Odtrącenie; bossy odporne, elity ×0,7),
odnowienie 3,5 → 4 s. Dane: [wojownik-v4/RAPORT_class_knight.md](wojownik-v4/RAPORT_class_knight.md).

| | Seria 3 | Seria 4 |
|---|---|---|
| Tester – zwycięstwa | 17/20 | 17/20 |
| Gracz – zwycięstwa | 14/20 | 13/20 |
| Rozpłatanie jako skill startowy | 12/16 | 9/16 (tester 8/10, gracz 1/6) |
| Cięcie wichru jako skill startowy | 19/24 | 21/24 |
| Rozpłatanie: obrażenia na użycie (tester / gracz) | 162 | 145 / 175 |

Przy 16 podejściach różnica 12 → 9 zwycięstw mieści się w szumie. Powalenie nie zastępuje odpowiedzi na strzelców:
gracz z Rozpłataniem nadal ginie na piętrze II od kul ognia. Boty nie wykorzystują jeszcze celowo okna po powaleniu
(np. mocny cios w leżącego), więc zysk z efektu jest w tych liczbach raczej zaniżony.

## Ograniczenia botów (uczciwie)

- Bot widzi początek zamachu z danych gry, a nie z animacji. Opóźnienie reakcji to model człowieka, nie percepcja obrazu.
  Nie ma kamery, więc „ciosy z tyłu” liczone są względem kierunku postaci.
- Nawigacja jest prosta (prosto do celu plus omijanie zablokowań), nie ma planowania pozycji względem strzelców ani priorytetu zabijania kultystów.
  Człowiek prawdopodobnie przeszedłby piętro 2 częściej.
- Prędkość pocisków jest szacowana stałą (14 m/s), a skille idą przy każdej okazji. Udział skilli w obrażeniach jest przez to zawyżony.
- Jedna trudność i 20 podejść na bota: wnioski ilościowe mają dokładność rzędu ±10 p.p.
- Strojenie botów w trakcie prac zmieniało wyniki: wersja bez odpowiedniego leczenia wygrała 0/20, a ta sama gra z poprawionym botem 11/20.
  Wnioski z punktów 1–3 były stabilne we wszystkich czterech wersjach serii.

## Jak uruchomić

```
# 20 podejść na bota (ok. 5 min dla pary Wojownika w trybie wsadowym, edytor zamknięty):
TURRIS_BOT_DIR=<katalog> TURRIS_BOT_RUNS=20 python tools/run_tests.py PlayMode batch Turris.Tests.Bots.WarriorBotPlaytests
# Przegenerowanie raportu z zapisanych JSON-ów (po zmianie reguł raportu):
TURRIS_BOT_DIR=<katalog> python tools/run_tests.py PlayMode batch Turris.Tests.Bots.BotReportRegenerate
```

Kod: `Assets/TurrisDamnatorum/Tests/PlayMode/Bots/`. `HeroBot` to wspólny mózg (percepcja, obrona, atak, leczenie, decyzje między piętrami),
`WarriorBot` to część klasowa, `BotPersona` opisuje osobowości, `BotTelemetry` zbiera dane, a `BotReport` generuje tabele i wnioski.
`BotPlaytestRunner` uruchamia serię. Bez `TURRIS_BOT_DIR` testy są pomijane i nie spowalniają zwykłego PlayMode.
Kolejne klasy wymagają podklasy `HeroBot` (zestawy, cechy, skille) i pary testów.
