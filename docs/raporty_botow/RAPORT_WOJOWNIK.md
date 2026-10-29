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

## Piąta seria: oferty sklepu, zamiana skilli, 2 × 50 podejść (2026-10-28)

Gra bez zmian, zmieniły się boty:
- **Tester** kupuje każdą ofertę sklepu dusz i co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji.
- **Gracz** kupuje oferty i wybiera nowe skille w nagrodach, jeśli zapowiadają się lepiej niż najsłabszy skill w slocie
  albo dają atak z dystansu. Sloty układa według obrażeń na sekundę odnowienia, które zmierzył w swoich podejściach,
  a nieznane skille próbuje z ciekawości.

Raport zawiera nową sekcję „Kolekcja skilli”. Dane: [wojownik-v5b/RAPORT_class_knight.md](wojownik-v5b/RAPORT_class_knight.md)
(tester z serii v5 i gracz po poprawce wyceny skilli ruchu). Pierwszy przebieg gracza jest w `wojownik-v5`.

| | Seria 4 (20) | Seria 5 (50) |
|---|---|---|
| Tester – zwycięstwa | 17/20 | **45/50** |
| Gracz – zwycięstwa | 13/20 | 22/50 (pierwszy przebieg: 20/50) |
| Różnych skilli użytych na podejście (tester / gracz) | 3,9 / 3,8 | **6,1 / 5,2** |
| Udział użyć: skille ze sklepu (tester / gracz) | – | 31% / 7% |
| Udział użyć: skill startowy (tester / gracz) | – | 29% / 49% |

### Skille startowe są za mocne (potwierdzone)

Obrażenia na sekundę odnowienia (dane testera, skille użyte co najmniej 15 razy, odnowienie z poziomu 1):

| Skill wojownika | Obr./użycie | Odnowienie | Obr./s odnowienia |
|---|---|---|---|
| **Rozpłatanie** (startowy) | 167 | 4 s | **41,8** |
| Fala przełamania | 244 | 7 s | 34,9 |
| **Cięcie wichru** (startowy) | 152 | 5 s | **30,4** |
| Piekielna szarża | 153 | 8 s | 19,1 |
| Upadek kata | 167 | 9 s | 18,6 |
| Trzęsienie | 133 | 8 s | 16,6 |
| Młynek | 64 | 9 s | 7,1 |

Wnioski:
- Oba skille startowe są **1,5–2,5× mocniejsze** od pozostałych skilli wojownika z puli sklepu i nagród.
  Rozpłatanie jest najmocniejsze dopiero po korektach z serii 2–4 (+15% obrażeń, odnowienie 5 → 4 s), więc te korekty poszły za daleko.
- Gracz, który ocenia skille po wynikach, prawie zawsze zostawia oba startowe w slotach. Odrzucił 59 ze 139 ofert,
  a skille ze sklepu dają tylko 7% jego użyć. Nowe skille przegrywają z ulepszonymi startowymi także dlatego,
  że ulepszenia trafiają tylko do skilli w slotach.
- Skille z puli innych klas (Piekielna kula, Strzał przeszywający, novy) mieszczą się w przedziale 15–70 obr./s odnowienia,
  więc to skille klasowe wojownika odstają w dół.
- **Zastrzeżenie:** skille nie są dziś przypisane do broni ani klasy. Prawie żaden skill HU nie ma `requiredTags` ani wymagań cech:
  wojownik z mieczem rzuca Piekielną kulę i novy maga, a Cięcie wichru działa z każdą bronią. Wymaganie broni mają tylko
  strzały Turris (łuk) i Uderzenie tarczą (tarcza). Pole `classId` w katalogu HU nie filtruje puli sklepu i nagród.
  Część 31% użyć ze sklepu u testera to skille, które po przypisaniu do broni byłyby niedostępne.
- Tester z rotacją i zakupami wygrywa 45/50, czyli więcej niż w serii 4. Pełna kolekcja skilli mu nie szkodzi, a raczej pomaga.

### Gracz i piętro II

27 z 28 śmierci gracza padło na piętrze II, najczęściej od kultysty (16). Oferty sklepu niewiele tu zmieniają, bo przed piętrem II
jest tylko jedna przerwa. Gracz na starcie każdego podejścia zamienia Szarżę na Egzekucję (45×), bo Egzekucja ma 341 obrażeń na użycie.
Spadek z 13/20 do 22/50 może więc wynikać z utraty doskoku do strzelców, ale może to być też szum: przy 20 podejściach rozrzut wynosi ok. ±11 p.p.
Do sprawdzenia: seria gracza z Szarżą zablokowaną w slocie.

### Propozycja balansu (do decyzji)

1. **Rozpłatanie:** cofnąć część korekt z serii 2–4, np. odnowienie 4 → 5 s i obrażenia 39 → 34. Daje to ok. 29 obr./s odnowienia,
   czyli poziom Cięcia wichru. Powalenie zostaje jako cecha skilla.
2. **Cięcie wichru:** −10% obrażeń albo odnowienie 5 → 5,5 s (ok. 27 obr./s odnowienia).
3. **Pozostałe skille wojownika:** podnieść do 22–28 obr./s odnowienia. Trzęsienie, Upadek kata i Piekielna szarża
   +20–30% obrażeń albo krótsze odnowienie. Młynek wymaga osobnej analizy (7 obr./s odnowienia; możliwe, że jego obrażenia nie trafiają w pełni do telemetrii).
4. **Zmiana skilla nie może zerować inwestycji:** obecnie ulepszenia zostają w starym skillu. Można np. zwracać część dusz przy wymianie
   albo dawać nowemu skillowi poziom startowy równy połowie poziomu wymienianego.

## Szósta seria: skille przypisane do broni, 2 × 50 podejść (2026-10-28)

Skille wymagają rodzaju broni (ostrze, ciężka broń, sztylet, łuk, fokus), a ich obrażenia zależą od siły broni.
Kupione odblokowania skilli i broni są dla każdej klasy. Dane: [wojownik-v6/RAPORT_class_knight.md](wojownik-v6/RAPORT_class_knight.md).

| | Seria 5 | Seria 6 |
|---|---|---|
| Tester – zwycięstwa | 45/50 | 45/50 |
| Gracz – zwycięstwa | 22/50 | **39/50** |
| Różnych skilli na podejście (tester / gracz) | 6,1 / 5,2 | 5,9 / 4,1 |
| Udział użyć: sklep (tester / gracz) | 31% / 7% | 36% / 1% |
| Gracz: oferty kupione / odrzucone | 56 / 59 | 9 / 157 |

Wnioski:
- Sklep proponuje teraz tylko skille pasujące do broni, więc wojownik nie dostaje już Piekielnej kuli ani nov.
  Tester kupuje wszystko i dalej wygrywa 45/50.
- Gracz wygrywa 39/50, bo od początku gra zestawem Cięcie wichru + Rozpłatanie (odblokowanie) + Szarża i wymienia skille tylko
  wyjątkowo. **Cel „korzystanie z wielu skilli” nie jest spełniony**: pozostałe skille wojownika przegrywają z dwoma startowymi
  (patrz seria 5: 30–42 wobec 7–19 obr./s odnowienia), więc gracz odrzuca 157 z 207 ofert.
- Egzekucja u testera: 810 obrażeń na użycie, czyli wartość odstająca. Do sprawdzenia przy balansie.
- Pierwszy przebieg gracza (14/50) był błędny. Bot brał na start dodatek z bronią innego rodzaju (np. Młot burzy),
  przez co jego skill startowy przestawał działać. To samo grozi graczowi: **ekran przygotowania powinien ostrzegać**,
  gdy broń z dodatków nie pasuje do skilla startowego.

## Siódma seria: balans skilli wojownika, ostrzeżenie przy dodatkach, 2 × 50 podejść (2026-10-28)

Cel balansu: 24–28 obrażeń na sekundę odnowienia dla wszystkich skilli wojownika (`HuBalance`, Egzekucja w treści Turris).

| Skill | Zmiana | Obr./s odnowienia: seria 5 → 7 (tester) |
|---|---|---|
| Rozpłatanie | 39/44/44 → 32/36/36, odnowienie 4 → 5 s | 41,8 → 28,4 |
| Cięcie wichru | 32 → 28/30/30 | 30,4 → 32,3 |
| Fala przełamania | 34/40/46 → 30/35/40, odnowienie 7 → 8 s | 34,9 → 26,2 |
| Upadek kata | 42/50/50 → 46/54/54, odnowienie 9 → 7 s | 18,6 → 23,8 |
| Piekielna szarża | 38 → 42, odnowienie 8 → 6,5 s | 19,1 → 28,6 |
| Trzęsienie | 34 → 40, odnowienie 8 → 6 s | 16,6 → za mało danych |
| Młynek | 11 → 23 na obrót, odnowienie 9 → 6 s | 7,1 → 15,2 |
| Egzekucja | mnożnik 1,6 → 1,2, premia +100% → +60%, odnowienie 7 → 9 s | 90 (810/użycie) → 41,7 |

Ekran przygotowania ostrzega teraz, gdy broń z dodatków nie pozwala użyć skilla startowego (np. kostur zamiast miecza przy Cięciu wichru).
Dane: [wojownik-v7/RAPORT_class_knight.md](wojownik-v7/RAPORT_class_knight.md).

| | Seria 6 | Seria 7 |
|---|---|---|
| Tester – zwycięstwa | 45/50 | 37/50 |
| Gracz – zwycięstwa | 39/50 | 34/50 |
| Różnych skilli na podejście (tester / gracz) | 5,9 / 4,1 | 5,5 / 4,2 |
| Gracz: udział użyć ze sklepu, oferty kupione / odrzucone | 1%, 9 / 157 | 7%, 28 / 123 |

Wnioski:
- Rozrzut skilli wojownika zmalał z 7–42 do 24–32 obrażeń na sekundę odnowienia (bez Młynka). Wieża jest trochę trudniejsza: −8 zwycięstw testera, −5 gracza.
- Gracz kupuje 3× więcej ofert i rotuje trzeci slot (Szarża → Egzekucja, Fala przełamania, Cięcie krzyżowe). Rozpłatanie i Cięcie wichru
  dalej zostają, bo są teraz porównywalne z resztą, a do tego ulepszone (ulepszenia nie przechodzą na nowy skill) i dają atak z dystansu
  oraz powalenie. Różnorodność wzrosła tylko nieznacznie (4,1 → 4,2 skilla na podejście).
- Na dalszą różnorodność bardziej niż liczby wpłyną zmiany w systemie: przenoszenie części ulepszeń przy wymianie skilla, więcej ofert
  skilli ostrzy i ciężkiej broni w sklepie, ewentualnie czwarty slot.
- Do dalszej korekty: Młynek (15,2) i Egzekucja u testera (41,7; u gracza 24,6, bo gracz rzadziej trafia nią w powalone cele).

## Ósma seria: wieża 30 pięter (fale Hell Unleashed), 2 × 20 podejść (2026-10-28)

Wieża jest wiernym portem 30 fal HU. Do tego skille zdobyte później przychodzą na wyższym poziomie (progi HU).
Dane: [wojownik-v8/RAPORT_class_knight.md](wojownik-v8/RAPORT_class_knight.md).

| | Tester | Gracz |
|---|---|---|
| Zwycięstwa | 0/20 | 0/20 |
| Średnie piętro | 4,6 | 5,0 |
| Najdalej | VII | VII |
| Ukończyło piętro IV / V / VI | 10 / 4 / 1 | 14 / 8 / 7 |
| Ukończyło piętro VII | 0 | 0 (7 dotarło) |

Wnioski:
- **Nowa ściana to fale 4–7.** Fala 4 to ogary i pomioty przy budżecie 57 (dawne piętro IV było ręczne, ze skalą 0,95).
  Fala 7 dokłada osiłków, czarowników mrozu, harpie i ofiarników przy życiu ×1,47. Nikt jej nie przeszedł.
- Przyczyną jest **krzywa siły bohatera, nie liczby demonów**. W HU bohater rośnie co falę dzięki sklepowi: perki, poziomy broni (+20%/poziom),
  przedmioty losowane na poziom fali z rzadkością i affixami, pieczęcie, atrybuty. W Turris rośnie wolniej:
  - nagroda za piętro;
  - jeden punkt cechy na piętro;
  - ulepszenie broni tylko w kapliczce po bossie;
  - brak przedmiotów skalowanych falą i brak perków.

  Demony mają liczby HU, bohater ich nie dogania.
- Wniosek dla migracji: wierna wieża HU wymaga **etapu 6 planu migracji** (przedmioty, affixy, rzadkości i ekonomia),
  czyli sklepu co piętro z poziomami broni i perkami jak w HU.

## Seria 9–10: o 20% mniej demonów niż w HU, łagodzenie pięter IV–VI (2026-10-28)

Założenie projektu: w Turris ma być nieco mniej demonów niż w Hell Unleashed, na początek o 20% (`HellTower.MonsterCountScale` = 0,8,
budżet portali każdej fali). Seria 10 dokłada łagodzenie pięter IV–VI: życie ×0,85, obrażenia ×0,9. Ma ono przywrócić przejście
pierwszych sześciu pięter z dawnej wieży, bo bohater Turris rośnie wolniej niż w HU.
Dane: [wojownik-v9](wojownik-v9/RAPORT_class_knight.md), [wojownik-v10](wojownik-v10/RAPORT_class_knight.md) (po 30 podejść).

Odsetek podejść, które przeszły piętro VI:

| | Stara wieża (seria 7) | 30 pięter, demony jak w HU | −20% demonów | −20% demonów + łagodzenie IV–VI |
|---|---|---|---|---|
| Gracz | ok. 68% | 35% | 57% | **77%** |
| Tester | ok. 74% | 5% | 17% | 23% |

Ukończone piętra w serii 10:
- gracz: I 30, II 27, III 27, IV 26, V 23, VI 23, **VII 9**, VIII 1;
- tester: I 30, II 25, III 25, IV 20, V 13, VI 7, VII 2.

Wnioski:
- Liczba demonów była częścią problemu: −20% podniosło przejście VI u gracza z 35% do 57%.
- Gracz przechodzi teraz pierwsze sześć pięter tak jak dawną wieżę. Tester (30% celowo złych reakcji, bez meta-progresji)
  ginie częściej niż dawniej na piętrze V i na bossie VI (Pan Sfory).
- **Kolejną ścianą jest piętro VII**, gdzie zaczyna się pełna krzywa HU (osiłki, harpie, czarownicy mrozu, życie ×1,47). Dalsze piętra
  wymagają progresji bohatera jak w HU (etap 6 planu migracji). Łagodzenie samych liczb tylko przesuwa ścianę.

## Serie 11–16: przedmioty botów, korekty piętra VII, ponowne próby, powtórki, wolniejsze portale (2026-10-28)

- **Seria 11:** boty zakładają przedmioty. Gracz bierze przedmiot z nagrody, gdy wypełni pusty slot albo poprawi build.
  Wyniki prawie bez zmian (gracz przechodzi piętro VII w 4 z 17 podejść).
- **Seria 12–13:** na piętrze VII o 1, potem o 8 wrogów mniej, zawsze najsłabszych (budżet maleje o koszt najsłabszego demona składu).
  Bez efektu: gracz przechodzi 1/20 i 3/27. Boty giną po 35–65 s, mając 10–30 zabitych, więc nie decyduje liczba wrogów.
- **Seria 14:** ponowne próby. Po śmierci bot wraca na początek piętra z zapisu stanu (do 3 prób), a po trzeciej porażce zapisuje
  wyzwanie dla gracza (menu główne → „Wyzwania botów”). Średnie piętro: tester 6,8, gracz 7,2.
- **Seria 15:** powtórki ostatecznych porażek, odtwarzacz HTML z widokiem z góry (`wojownik-v15/powtorki/`).
- **Seria 16: tempo wychodzenia z portali −50%** (obserwacja gracza z powtórki: potwory wychodziły za szybko). Odstęp grup i czas
  otwarcia portali ×2, liczba demonów bez zmian. Dane: [wojownik-v16](wojownik-v16/RAPORT_class_knight.md).

| Seria 16 (po 20 podejść, 3 próby na piętro) | Tester | Gracz |
|---|---|---|
| Średnie piętro (poprzednio 6,9 / 6,9) | **7,7** | **9,1** |
| Najdalej | XI | XIV |
| Piętro VII: ukończone / próby | 12/18 | 16/21 |
| Ściana | VI Pan Sfory (12/43), IX Królowa Topieli (1/22) | IX Królowa Topieli (6/40), XI (1/16) |

Wnioski:
- Wolniejsze portale dały największy skok ze wszystkich korekt. Piętro VII przestało być ścianą (gracz 76% prób),
  a gracz dochodzi średnio o dwa piętra dalej.
- Kolejne ściany to bossowie: Królowa Topieli na IX, a u testera Pan Sfory na VI. Do tego piętro XI (Rzeźnia).
- Korekta „−8 najsłabszych na piętrze VII” zostaje do decyzji. Przy wolniejszych portalach może być zbędna.

## Ograniczenia botów (uczciwie)

- Bot widzi początek zamachu z danych gry, a nie z animacji. Opóźnienie reakcji to model człowieka, nie percepcja obrazu.
  Nie ma kamery, więc „ciosy z tyłu” liczone są względem kierunku postaci.
- Nawigacja jest prosta (prosto do celu plus omijanie zablokowań), nie ma planowania pozycji względem strzelców ani priorytetu zabijania kultystów.
  Człowiek prawdopodobnie przeszedłby piętro 2 częściej.
- Prędkość pocisków jest szacowana stałą (14 m/s), a skille idą przy każdej okazji. Udział skilli w obrażeniach jest przez to zawyżony.
- Jedna trudność i 20 podejść na bota (seria 5: 50): wnioski ilościowe mają dokładność rzędu ±10 p.p. (±7 p.p. przy 50).
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
