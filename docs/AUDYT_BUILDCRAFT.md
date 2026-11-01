# Audyt buildcraftu: perki specjalizacyjne, unikaty, całość

Trzy etapy: (1) cztery perki specjalizacyjne, (2) 75 unikatów klasowych, (3) przenikanie cech, unikatów, perków i affiksów.
Oznaczenia: **[K]** – odczytane z kodu/danych, **[Z]** – zmierzone w silniku, **[S]** – szacunek z liczb, **[H]** – hipoteza do pomiaru.

## Etap 1 – perki specjalizacyjne (2026-11-01)

### Dane porównawcze

- Ceny = baza × 2,5 przy inflacji 1 [K]. Bezwarunkowe alternatywy HU: **Ostrze** +15% obr. fizycznych – 55 dusz; **Furia** +8% wszystkich obrażeń (także sług – migawka sługi przechodzi przez `PrepareOutgoing`) – 65; **Szybkie ręce** +8% szybkości ataku – 60; kolejne poziomy ×1,5 ceny [K].
- Postawa [K]: wrogowie HU – zwykli 35 (elita ×1,5 = 52,5), ciężcy 80, bossowie 150; Turris 18–30, strażnik 80, boss 120. Miecz: lekki 18, mocny 40; parowanie 65–85. Przełamanie resetuje postawę do pełna.
- Przywołania [K]: Legion (skill klasy Przywoływacza, ogary) – czas 16 s, odnowienie 28 s → 57% czasu; Chochliki (od piętra 1) – 12 s / 10 s → do 100%; Golem od piętra 16.

> **Korekta (etap 3):** rachuba Sfory poniżej zakłada Legion jako drugi rodzaj sług. To błąd: cecha klasowa zastępuje skill klasy (`MetaService`: `signatureSkill` tylko bez cechy), więc Legion jest w grze niedostępny. Właściwe liczby w etapie 3.

### Karty perków

**Pojedynkowicz** (70 dusz). Fantazja: szybki szermierz bez tarczy. Decyzja: druga ręka (tarcza: blok bierny 15%+, garda; druga broń) albo tempo.
- Siła [Z/S]: zmierzone 0,380 → 0,335 s, czyli suma tempa ≈ 1,12 → **+13% ciosów** na starcie. Dodatek do sumy sam się ogranicza: przy Szybkich rękach V (+40%) ≈ +10%, z pełnym Rozpędem III ≈ +7%.
- Względem Szybkich rąk I: ok. 1,6× więcej tempa za duszę, ale z realnym kosztem (bez tarczy) i bez kolejnych poziomów. Wytrzymałość na sekundę rośnie o te same 13% – naturalny hamulec.
- Ocena: **zdrowy**. Ryzyko pętli brak. Do obserwacji: czy bez tarczy da się przeżyć bossów (sam miecz paruje).

**Wykorzystaj wyłom** (75 dusz, od piętra 3). Fantazja: wykorzystanie odsłony twardego wroga.
- Siła [S]: zwykły wróg HU (35) pęka od 2 lekkich albo 1 mocnego cięcia miecza. Seria lekkich → co drugi cios ×1,25 (≈ **+12%**); seria mocnych → każdy po pierwszym ×1,25 (≈ **+25%**). Boss (150): ok. 4 mocne na przełamanie (≈ +6%), parowanie 2× → riposta (×2,5–4 obrażeń broni) ×1,25.
- **Problem: odwrócona fantazja.** Najwięcej daje na zwykłych wrogach, którzy i tak giną szybko, a najmniej na elicie i bossie, dla których powstał. Na zwykłych to praktycznie stałe +12–25% bez wysiłku (ciosy i tak łamią postawę).
- Względem Ostrza (+15% bezwarunkowo, 55): porównywalny na zwykłych, słabszy na bossie.

**Różnorodna sfora** (80 dusz). Fantazja: zróżnicowana armia.
- Siła [S]: Przywoływacz ma 2 rodzaje od piętra 1 (Legion + Chochliki) → +10%, ale Legion stoi tylko 57% czasu → **efektywnie ok. +6% obrażeń sług**. Trzeci rodzaj (Golem) dopiero od piętra 16 i kosztem trzeciego slotu → +20% (efektywnie ok. +12–15%).
- **Problem: martwy wybór przy 2 rodzajach.** Furia I (+8% wszystkiego, w tym sług, 65 dusz) jest tańsza i lepsza. Bot słusznie by go nie kupił.

**Gwałtowna manifestacja** (75 dusz). Fantazja: gwałtowny wybuch zamiast długiej kontroli pola.
- Siła [K/S]: budżet i liczba tyknięć bez zmian, czas ×0,5. Na stojącym celu **0% DPS w stanie ustalonym** (odnowienie się nie zmienia). Zysk tylko z realizacji budżetu: wrogowie, którzy wychodzą ze strefy albo giną przed końcem, mniej „marnują”. Koszt: połowa czasu kontroli pola (spowolnienia, blokada przejścia).
- **Problem: wartość niewidoczna i niemierzona.** Nie wiemy, jaka część budżetu stref dziś ginie (wyjście ze strefy). Bez tego nie wiadomo, czy to +0% czy +40%. Statusy nakładane 2× częściej przez połowę czasu – wpływ na podpalenie niezmierzony [H].

### Wspólne

- Pętle sprzężenia: brak (żaden perk nie zwraca zasobu ani nie wyzwala kolejnych zdarzeń). Wyłom ma zabezpieczenia: raz na cel, nowa akcja, bez sług/stref/kopii.
- Kumulacja mnożników na ripoście: broń ×2,5–4 × Krwawa riposta +30% × Wyłom ×1,25 × Egzekucja (przy przełamanej postawie) – największy pojedynczy cios gry, ale wymaga parowania; to zamierzony „power spike”, nie błąd [S].
- Kwalifikacja Sfory w sklepie nie widzi skilla klasy (Legion) – sam Legion i tak daje 1 rodzaj (0%), więc w praktyce bez skutków [K]. Poprawiony analogiczny błąd w ocenie bota.

### Decyzje etapu 1 (autor, 2026-11-01) i wdrożenie

- **Wyłom:** okno otwiera tylko wróg z maks. postawą ≥ 50 (po mnożniku elity: elity, ciężcy, bossowie), premia ×1,5 (było ×1,25, bez progu). `NativePerkTuning.breachMinPoise`, telemetria „wyłom: pominięte (słaba postawa)”.
- **Sfora:** +15% za rodzaj ponad pierwszy (do +30%), cena bez zmian.
- **Manifestacja:** najpierw pomiar. Kwalifikowane strefy czaru (z perkiem i bez) zliczają trafienia celów w całym czasie i w pierwszej połowie (`DamageZone.MeasuredBy`, `PlayerCombat.ZoneMeasured`); raport serii botów podaje szacowany zysk „2 × pierwsza połowa / całość” przy tym samym zachowaniu wrogów.
- Testy: EditMode 315/318 (2 znane błędy wyglądu), PlayMode perków + boty + scenariusze unikatów 14/14 (w tym nowy przypadek: wróg o postawie 20 łamie się bez okna).

### Pomiar Manifestacji [Z] (mała próba)

Serie maga po 2 podejścia na osobowość (`TestResults/audyt/strefy`), jeden bot Gracza sam kupił perk w trakcie:
- Bez perka: 14 kwalifikowanych stref, 98 tyknięć, 54 trafienia celów (0,55 na tyknięcie), z tego 65% w pierwszej połowie czasu → szacowany zysk realizacji budżetu **×1,30**.
- Z perkiem: 7 stref, 70 tyknięć, 72 trafienia celów (1,03 na tyknięcie) → surowo **×1,87** względem stref bez perka, ale to inny zestaw stref i walk, więc liczba nie jest bezpośrednio porównywalna.
- Tester nie użył kwalifikowanych stref, więc cała próba pochodzi od bota Gracza.
- Wniosek: perk nie jest martwy; realny zysk realizacji to rząd +30% lub więcej, przy połowie czasu kontroli pola. **Bez zmian liczb.** Telemetria zostaje w raporcie serii.

## Etap 2 – 75 unikatów klasowych

### Co jest dobrze [K]

- Każda karta ma jawny koszt i kompromis, a reguły mają klauzule bezpieczeństwa („jedna konsumpcja na rzut”, „bez łańcucha”, „widma nie tworzą żetonów”, „rana znika z ofiary – nie jest klonowana”). Trafienia mocy mają znacznik `uniqueEffect`, więc nie odpalają cech, Wyłomu ani innych mocy – **nie znalazłem pętli zdarzeń** (zabójstwo → rzut → zabójstwo, trafienie → kopia → trafienie).
- Moce skalują się udziałem obrażeń broni/czaru (np. 40% lekkiego ciosu), więc rosną razem z buildem tymi samymi warstwami, bez osobnego toru mnożników.

### Bloker jakości [K]

- **24 z 75 mocy nie ma żadnego dowodu działania w silniku** (36 nie zadziałało w dymie, ale 12 z nich ma osobny test rozstrzygający). Lista braków niżej.
- **Boty nie widzą różnic między unikatami:** każda działająca moc ma w ocenie wyposażenia stałe 25 pkt. Serie botów nie powiedzą więc, który unikat jest za mocny albo martwy.

### Kandydaci na dominację (szacunki do pomiaru) [S]

| Kod | Unikat | Szacunek | Dlaczego ryzyko |
|---|---|---|---|
| B02 | Różaniec gniewnych duchów | 3 widma × 40% lekkiego co 1,1 s ≈ +1,1 lekkiego ciosu/s; topór dwuręczny robi ok. 0,7 lekkiego/s | możliwe **+150% DPS** przy pełnych widmach; zależy od tempa żetonów |
| R03 | Pas żonglera ostrzami | każdy mocny atak → orbita 4 s, 10 tyknięć × 50% lekkiego drugiej broni na każdego wroga w 2,2 m | ok. 5 lekkich ciosów AoE na 4 s, praktycznie stała obecność |
| R05 | Amulet zbłąkanej klingi | wir 4 s, tyknięcie 0,35 s × 45% lekkiego | ok. 5 lekkich ciosów AoE na każdy Młynek, ponad sam Młynek |
| B05 | Pierścień nierozładowanego szału | ładowanie 1,35 s → mocny + 3 × 55% mocnego | ≈ 2,65 mocnego ciosu za jeden ładowany; koszt: szał bez szybkości |
| S03 + P03 | Dyrygent + Habit | komendy skracają odstęp ataku sług do 45% (≈ 2,2× ataków), aury na sługach 70% | mnożenie osi częstotliwości i obrażeń sług – patrz etap 3 (Sfora +30%) |

### Kandydaci na martwy wybór [H]

- Moce, które zabierają podstawową zaletę broni: K03 Łuk grabarza (łuk w zwarciu), T05 Grymuar złamanej różdżki (różdżka jako ostrze) – wymagają pomiaru, czy zysk pokrywa utratę dystansu.
- L04 Sztylet niedokończonej ucieczki i W01 Pawęż – zysk zależny od wyczucia gracza, a boty ich nie używają celowo.

### Czego brakuje 24 mocom bez dowodu [K]

Scenariusz dymny to stała, ślepa sekwencja (co 0,45 s, ok. 7 s) z bohaterem `DebugInvulnerable`, wrogami 4000 PŻ i parowaniem niezgranym z ciosem wroga. Przycisk cechy jest naciskany (slot skilla klasy), ale warunek cechy zwykle nie zachodzi.

| Brak w scenariuszu | Moce |
|---|---|
| Warunek cechy nie zachodzi (brak szału, oszukanych, sztuczki, grotów, reakcji) | B02 (od tego czasu zmierzona), L01, L02, K01, Q02 |
| Udana obrona zgrana z ciosem wroga | W04, P04, T04 |
| Prawdziwe przyjęcie ciosu (bohater jest nietykalny) | M02, E04 |
| Śmierć podpalonego wroga (wrogowie nie giną) | F02 |
| Kolejność albo geometria: strefa przed kulą, wróg odrzucony na pocisk, pocisk jako węzeł, cios w plecy, dwóch krwawiących w Cięciu, pułapka na trasie noża, unik w trakcie ataku | F01, A02, A04, Z04, Z05, L05, R02 |
| Czas: naturalny koniec sługi (15 s > długość scenariusza) | S05 |
| Niepełny build: brak perka Rozszczepienie i skilli trzech żywiołów | Q03, Q05 |
| Moc bierna bez zdarzenia w telemetrii – trzeba mierzyć skutek (tempo sług, odporność celu) | P03, T03 |
| **Niewyjaśnione: warunki wyglądają na spełnione** (fokus z grymuaru, sługa w zasięgu) | S04 |

### Pomiar kandydatów na dominację [Z]

`UniqueMeasurePlayTests` (`TURRIS_MEASURE=<katalog>`): ten sam build i polityka, moc włączona/wyłączona (statystyki unikatu zostają), 20 s, wrogowie stoją i nie atakują, miara: stracone życie wrogów. Jedno uruchomienie, bez wariancji; kontekst obronny niemierzony. Wyniki: `TestResults/audyt/unikaty/`.

| Kod | Grupa 4 (postawa 35) | Twardy cel (postawa 150) | Szacunek sprzed pomiaru |
|---|---:|---:|---|
| B02 Różaniec | ×1,20 | ×1,39 | „do +150%” – **mocno zawyżony** (widma działają tylko 6 s po szale) |
| R03 Pas żonglera | ×1,15 | ×0,99 | moc obszarowa, na jednym celu bez zysku – zdrowo |
| R05 Amulet klingi | ×0,71 | ×0,80 | „ok. 5 ciosów AoE” – **w prostej grze moc szkodzi**: zastępuje Młynek i zabiera główną broń na 4 s |
| B05 Pierścień | ×1,06 | ×1,03 | „2,65 mocnego” – **zawyżony**: tylko 2 cięcia w 20 s (działa w szale) |
| S03 Dyrygent | ×1,09 | ×1,18 | umiarkowany; „2,2× ataków” to górna granica, nie średnia |

Wniosek: **żaden z kandydatów nie dominuje**; w tym zestawie ryzykiem jest raczej słaba moc (R05) niż za mocna.

### Błędy znalezione przy pomiarach (naprawione)

1. **Wyjątek w grze:** `UniquePowerSet.Sync` sortował listę mocy przy każdym odświeżeniu buildu; odświeżenie w trakcie `Tick` mocy (np. początek/koniec szału) wywracało pętlę. Sortowanie tylko przy zmianie listy, pętle cyklu życia po indeksie.
2. **R05 niezgodny z kartą:** podczas wiru ciosy szły na zmianę pięść i lewa broń, a karta obiecuje, że „lewa walczy sama”. Teraz lekkie ciosy zadaje sama lewa broń (`OffhandFightsAlone`), test `WanderingBlade_LeftHandFightsAloneDuringVortex_NoFists`. Zysk mocy prawie bez zmian (×0,71 / ×0,80).

### Boty

`HeroBot.UniqueScore`: 5 zmierzonych mocy wycenianych jako DPS × (zysk − 1), reszta stałe 25 pkt do czasu pomiaru.

### Decyzje etapu 2 (autor, 2026-11-01)

R05: wir 70% lekkiego ciosu (było 45%), pomiar do powtórzenia. Napisać testy dowodowe dla wszystkich 24 mocy. Resztę mocy zmierzyć po testach dowodowych.

### Scenariusze dowodowe [Z]

`UniqueProofPlayTests` – 24/24, trzy przebiegi z rzędu. Warunek każdej mocy powstaje prawdziwą ścieżką gry (parowanie, przycisk cechy, rzut skilla, potok trafienia), a test sprawdza zdarzenie mocy i skutek w świecie. S04 wyjaśniony: moc działa, w dymie rzut wypadał w trakcie ataku.

**Wady znalezione dzięki scenariuszom – moce, które nie mogły zadziałać w grze (naprawione):**

| Kod | Wada | Naprawa |
|---|---|---|
| Q02 Kołczan skradzionych żywiołów | `BasicShot` nadpisywał tożsamość strzału, więc odłożony ładunek nigdy nie dochodził i wygasał | usunięte nadpisanie |
| Z05 Rękawice karminowego węzła | Cięcie trafia w trakcie rzutu, a zbieranie ofiar startowało dopiero po nim – węzeł nigdy nie powstawał | zbieranie od pierwszego trafienia tej akcji |
| F02 Mankiety popielnych marionetek | `Die()` czyści statusy przed zdarzeniem śmierci – marionetka nie miała czego nieść | nowe zdarzenie `EnemyBrain.Dying` przed czyszczeniem |
| L01 Maska fałszywego króla | kolider wabika wypychał ciało celu: **król uciekał przez arenę ok. 3 m/s** (widoczny błąd), a oszukani go nie dosięgali | wabik ignoruje kolizję z ciałem śledzonego celu |

**Karty obiecujące rdzeń, który nie mógł działać (poprawione opisy):**

| Kod | Było | Jest | Dlaczego |
|---|---|---|---|
| A02 | Oko cyklonu | Piekielna kula | Oko to wir, nie pocisk; jedyny wolny pocisk maga to kula (prędkość 9) |
| L05 | Sidła | Sidła z pieczęcią Wnyki | sama strefa Sideł nie jest pułapką |
| T03 | `hu_morningstar` (pęknięcie) | `hu_warhammer` | buława nie nakłada pęknięcia |
| T04 | miecz i tarcza | miecz i grymuar | czar wymaga fokusu, a tarcza go nie daje |

Wniosek: **4 z 24 mocy bez dowodu były w grze martwe, a kolejne 4 miały błędny opis rdzenia** – obawa o brak dowodów była uzasadniona. Pozostałe działały, tylko scenariusz dymny nie tworzył ich warunku.

### Pomiar wszystkich 75 mocy [Z]

`AllPowers_GenericRotation` (`TURRIS_MEASURE_ALL=<katalog>`, wynik: `TestResults/audyt/wszystkie/pomiar_wszystkich.md`): ta sama rotacja dla każdej mocy (cecha co 6 s, gotowe skille, co trzeci cios mocny), rdzeń z testu dymnego, jeden przebieg na wariant.

**Ta metoda jest grubym sitem, nie wyceną:**
- **Szum ok. ±15%:** moce z 0 zdarzeń nic nie zrobiły, a pokazują ×0,86–×1,04 (W01 ×1,04/×0,89, B01 ×0,86/×0,91, B05 ×0,89/×0,89).
- **Około 1/3 mocy nie zadziałała w rotacji** (0 zdarzeń) – to brak warunku, nie słabość.
- **Artefakty rotacji bez ruchu:** W05 ×0,02 (odwrót od celu, bez powrotu), B04 ×0,03 w grupie (rzut broni), K04 ×0,53 – bohater nie podchodzi z powrotem.
- Mierzy same obrażenia: moce obronne i użytkowe (W01, P03, M02, E01…) nie mają tu jak wykazać wartości.

Trzy wyniki wystające ponad szum sprawdzone osobno (3 powtórzenia, `TestResults/audyt/unikaty/pomiar_P02_F04_T01.md`):

| Kod | Ogólna rotacja (grupa/twardy) | Po powtórzeniach | Wniosek |
|---|---|---|---|
| P02 Mitra | ×1,69 / ×1,12 | ×0,81 / ×1,02 | szum – wariant bez mocy waha się 5041–8668 |
| F04 Szata pogorzeliska | ×1,13 / ×1,67 | ×0,94 / ×1,07 | neutralna |
| T01 Bandaże zapaśnika | ×2,21 / ×1,63 (wobec pięści) | **×0,51 / ×0,45 (wobec miecza)** | pierwotna liczba mylna: porównanie z gołymi pięściami; wobec broni traci połowę |

R05 po wzmocnieniu wiru do 70% (pomiar celowany): ×0,97 / ×1,13 – już nie szkodzi.

**Wniosek: w żadnym pomiarze nie potwierdziła się dominacja obrażeń.** Największy potwierdzony zysk to B02 (×1,20–1,39). Ryzykiem są raczej moce słabe: T01 ×0,5.

### Boty

Wycena z pomiaru tylko dla mocy zmierzonych celowanie (B02, R03, R05, B05, S03, P02, F04, T01); wyniki ogólnej rotacji **celowo nie trafiły do wyceny**, bo przy szumie ±15% i artefaktach bot odrzucałby dobre przedmioty. Reszta: stałe 25 pkt.

### Regresje po etapie 2

- EditMode 315/318 (2 znane błędy wyglądu „AnimeForward”, 1 pominięty eksport WAV).
- PlayMode: pełny zestaw (unikaty, dowody, perki, boty, cechy, skille, przepływ, HU, pieczęcie, affiksy) 195/199 – 3 błędy, 1 pominięta galeria: `CollidingFronts_…`, `Meteor_…`, `Storm_…`.
- **Trzy błędy to wina mojego testu:** `NativePerkBotTests` zostawiał po sobie scenę `Main` z działającym podejściem, a alfabetycznie szedł przed `ClassFeature` i `GameFlow`. Te same 3 błędy powtórzyły się w pełnym zestawie; po sprzątaniu sceny zestaw podejrzany przechodzi 50/50. Wcześniejsze dwie hipotezy (zakłócanie przez `AffixSpecializationPlayTests`, sporadyczność) były błędne. Pełny zestaw po poprawce: 198/200 (1 pominięta galeria, 1 błąd `Bellringer_…` – test parował za wcześnie, zgrany z końcem zamachu; B01 zależał od losowego uchylenia i odrzutu, teraz ciosy co 0,2 s przez ten sam potok). Pojedynczy błąd `Archer_ShootsArrowsAtPlayer_…` z innego przebiegu pozostaje niewyjaśniony.

## Etap 3 – przenikanie cech, unikatów, perków i affiksów

Stan affiksów: praca drugiej sesji wg `PROGRESS.md` zakończona, ale niezacommitowana. **Liczby tego etapu to wyprowadzenia z kodu i danych [K/S], nie pomiary w grze**; etap 2 pokazał, że szacunki z parametrów potrafią być mocno zawyżone, gdy zależą od częstotliwości. Wzory na pojedyncze trafienie są pewniejsze (potwierdza je pomiar `AffixBalanceMeasurementTests`).

### Warstwy obrażeń [K]

Kolejność dla ciosu bronią (`PlayerCombat.PrepareOutgoing`, `EnemyBrain`):

1. Baza broni × (1 + premie fizyczne) – **jeden wspólny kubeł**: `phys_add`, `phys_pct`, perk Ostrze sumują się.
2. Dodane żywioły: osobne składniki, B × suma% / 100.
3. Cecha i moce unikatów.
4. Premia ogólna (Furia, `global_pct`) – osobny mnożnik całości.
5. Premia danego żywiołu – mnoży tylko jego składnik.
6. Bufy czasowe, płonięcie; na celu: Wyłom, odporności, przebicie, reakcje, pancerz.

### Znaleziska

**1. Droga żywiołowa wygrywa z fizyczną przy pełnej inwestycji [S].** W affiksie Flat wartość jest mnożona ×5 (`HellEconomy.MapAffix`), więc T3 daje średnio +57,5% wobec +20,5% `phys_pct`. Kubeł fizyczny jest wspólny i addytywny, a żywioł dostaje osobny mnożnik.

| Inwestycja (T3, średnie rzuty, bez jakości) | Rachunek | Mnożnik ciosu |
|---|---|---:|
| 6 affiksów: 4 × `phys_add` + 2 × `phys_pct` | 1 + 2,30 + 0,41 | ≈ 3,7 |
| 6 affiksów: 4 × `fire_add` + 2 × `fire_pct` | 1 + 2,30 × 1,45 | ≈ 4,3 |
| Wszystkie sloty: 4 × `phys_add` + 5 × `phys_pct` | 1 + 2,30 + 1,03 | ≈ 4,3 |
| Wszystkie sloty: 5 × `fire_add` + 6 × `fire_pct` | 1 + 2,88 × 2,35 | ≈ 7,8 |

Przy 6 affiksach żywioł prowadzi nieznacznie (ok. 1,2×), przy pełnej inwestycji ok. 1,8×: ma więcej slotów (`fire_add` 5 wobec `phys_add` 4, `fire_pct` 6 wobec `phys_pct` 5) i osobny mnożnik. Przeciwwaga: odporności demonów i konieczność trzymania się jednego żywiołu. Wcześniej zgłoszone przez raport affiksów (+525% B = 84% ciosu po jakości 2). Bez pomiaru na prawdziwych odpornościach to hipoteza.

**2. Riposta omija cały potok [K].** `ReceiveRiposte` bierze tylko obrażenia fizyczne broni × riposta: bez premii ogólnej, dodanych żywiołów, egzekucji i pancerza.
- Furia obiecuje „+8% obrażeń wszystkich typów”, a riposty nie wzmacnia – **opis mija się z działaniem**.
- W buildzie żywiołowym riposta traci sens: przy mnożniku 7,8 zwykły lekki cios zadaje więcej niż riposta ×3–4 obrażeń fizycznych. Kara za styl gry, który gra chce nagradzać (Wyłom, W03, T04).

**3. Moce oparte na broni nie widzą dodanych żywiołów [K].** `UniqueWeaponHit` (B02, R02, R03, R05, Z…) liczy z obrażeń fizycznych; dodatek jest celowo tylko dla ręcznych ciosów („efekty wtórne” w raporcie affiksów). Skutek: im silniejszy build żywiołowy, tym słabszy unikat oparty na broni. Spójne ze specyfikacją affiksów, ale to antysynergia, o której gracz się nie dowie. Czary i sługi też dodatku nie dostają.

**4. Sfora po korekcie [K].** Chochliki od piętra 1 (12 s / odnowienie 10 s), ogary od 7 (12/12), golem od 16 (18/18) – **wszystkie mogą działać bez przerwy**, więc +15% od piętra 7 i +30% od 16 działa w pełni (przy 3 slotach na sługi). Wcześniejsza obawa o lukę Legionu była bezpodstawna. Ale: **perk jest oferowany od piętra 1, a przed piętrem 7 daje 0%**, chyba że przedmiot daje drugie przywołanie.

**5. ~~Opisy klas mylą~~ – fałszywy alarm [K].** Assety mają „Skill klasy: …”, ale `GameUI.ClassDescription` usuwa to zdanie, gdy klasa ma cechy. Bez zmian.

**6. Warstwy sług są umiarkowane [S].** Maksimum: `minion_damage` 2 sloty (+90% po jakości 2) × Sfora 1,3 × premia ogólna (Furia V + 2 × `global_pct`, ok. 1,97) × Habit z Aurą męstwa 1,08 ≈ 5,3×. Dyrygent w pomiarze ×1,09–1,18. Mniej niż droga żywiołowa, a kosztuje wszystkie sloty skilli. Bez pętli.

**7. Obrona: bez nieśmiertelności [K].** Limity: redukcja 45%, uchylenie 50%, odporność 65%; razem 0,55 × 0,5 ≈ 27% obrażeń fizycznych, do tego pancerz, blok i bariery. Kradzież życia ma wagi (obszar 0,25, tyknięcia 0) i okno leczenia biernego, a moce unikatów jej nie dają. Pętli przeżywalności nie znalazłem; nie mierzyłem jej w grze.

**8. Pokrycie [K].**
- Każda z 30 cech ma co najmniej jeden unikat, żadna nie jest osierocona.
- Affiksy specjalizacyjne: sługi, czary, ciężka broń, tarcza, przebicie; broń dystansowa ma szybkość i przebicie pocisków.
- **Luka:** perki klasowe są tylko dla rodzin Wojownika (8) i Maga (7). Łucznicy korzystają z perków pocisków zapisanych pod maga (waga ×0,35), a łotrzyk wręcz nie ma nic dla zasadzki, krwawienia i uników. Projekt niżej.

**9. Boty nie wyceniają nowych statystyk [K].** `GearScore` nie liczy dodanych żywiołów ani obrażeń sług, więc serie botów nie zmierzą dróg z punktów 1 i 6.

**10. T01 Bandaże zapaśnika są słabe także przy grze pod moc [Z].** Polityka: Arkana włączone, czary z karty, lekkie ciosy; ten sam zestaw czarów z mieczem: ×0,51 w grupie, ×0,42 na twardym celu (3 powtórzenia). Moc działa (4 rzuty w 20 s, 14 trafionych). Przyczyny: pięść ma bazę 16 wobec 40 miecza, a Arkana trwają 6 s na 16 s odnowienia, więc poza oknem nie ma ani broni, ani fokusu. Uwaga: polityka włączała Arkana raz na walkę, w obu wariantach, więc wynik nie uwzględnia drugiego okna.

### Czego etap 3 nie sprawdził

Bez pomiaru w grze: droga żywiołowa na prawdziwych odpornościach, przeżywalność, ekonomia dusz (`soul` do +90% po jakości 2 na 3 slotach), maksymalne warstwy sług. Punkty 1 i 6 to rachunek z danych.

### Decyzje etapu 3 (autor, 2026-11-01) i wdrożenie

- **Riposta:** dostaje premię ogólną i dodane żywioły (z premią żywiołu, przebiciem i odpornością celu), bez egzekucji i bufów ciosu. Test `Riposte_GetsGlobalBonusAndAddedElements`. Fala W03 liczy z sumy składników, bez odporności na ogień u trafionych obok.
- **T01:** pięści ×2 (`fist_mult`). Pomiar z 3 powtórzeniami w trybie graficznym: grupa ×1,03, twardy cel ×0,71 (przed zmianą ×0,51 / ×0,42 w `batch`).
- **Sfora:** w sklepie od piętra 7 (`packFloor`).
- **Boty:** `GearScore` liczy dodane żywioły i obrażenia sług.

**Uwaga do metody [Z]:** tryb `batch` zawyża walkę z grupą: ten sam miecz w grupie 7610 w `batch`, 4733 w trybie graficznym; na jednym celu tryby się zgadzają (2962 / 2858). Wyniki grupowe z `AllPowers_GenericRotation` (batch) są więc dodatkowo niepewne; celowane pomiary z etapu 2 szły w trybie graficznym.

### Projekt: perki rodziny Łotrzyka (propozycja, bez wdrożenia)

Wszystkie jako natywne, jednopoziomowe (`NativePerkCatalog`), oferowane tylko klasom z danym stylem. Liczby to hipotezy do pomiaru.

| Perk | Fantazja | Reguła | Koszt / decyzja | Kontra | Test |
|---|---|---|---|---|---|
| **Długi oddech** | cierpliwy zabójca | Okno Zasadzki po uniku 1,5 → 2,5 s; premia po uniku bez zmian, w plecy +60% → +50% | gorsze ciosy w plecy za łatwiejsze otwarcia | wrogowie obszarowi karzą przeciąganie | udział trafień z Zasadzki, zmiana obrażeń na bossie |
| **Otwarta rana** | krwawienie jako przygotowanie | Bezpośredni ręczny cios w krwawiący cel +4% za warstwę, najwyżej +12%; rany bez zmian | trzeba najpierw nałożyć krwawienie; nic nie daje buildom bez ran | odporni na krwawienie; krótka walka | zysk na bossie i tłumie, kumulacja z `bleed` i Z05 |
| **Lekki krok** | taniec między ciosami | Pierwszy lekki cios po uniku kosztuje 50% wytrzymałości (raz na unik) | tylko unik, nie gwarda; działa wyłącznie przy grze blisko | ciosy obszarowe po uniku | wytrzymałość na minutę, liczba uników, spam unik + cios |
| **Daleki strzał** | łucznik trzymający dystans | Strzał w cel powyżej 8 m +12%, poniżej 4 m −12% (zwykły atak łukiem, obok Sokolego oka) | kara w zwarciu; wymaga miejsca | wrogowie szybcy, skoczki, ciasne areny | średni dystans strzału, przeżywalność, kumulacja z Sokolim okiem |

Rekomendacja na pierwszy test: **Otwarta rana** i **Długi oddech** – dotykają braku u łotrzyka wręcz i nie mają ryzyka pętli. Największe ryzyko nadużycia: Lekki krok (spam).

### Pomiar dróg obrażeń [Z]

`Routes_PhysicalVsElemental_RealDemons` (`TURRIS_MEASURE_ROUTES=<katalog>`, wynik: `TestResults/audyt/drogi/drogi_obrazen.md`): miecz, 6 s lekkich ciosów, 34 demony z katalogu z prawdziwymi odpornościami, jeden cel, bez statusów i reakcji.

| Inwestycja | Fizyczna | Ogień | Mróz | Żywioł / fizyczna |
|---|---:|---:|---:|---:|
| 6 affiksów | 2568 | 3024 | 3056 | ×1,18–1,19 |
| Wszystkie sloty | 3007 | 5414 | 5466 | **×1,80** |

Rachunek z tabeli wyżej się potwierdził. Odporności hamują słabo: żywioł przegrywa tylko na odpornych na niego demonach (ogień: 2010 wobec 3415 na Ognistym chochliku), a na bossach wygrywa (Władca Bramy: mróz 5100, ogień 4170, fizyczna 2745). To stan przy średnich rzutach T3 we wszystkich slotach, czyli górny pułap, nie typowa gra. 
**Decyzja autora:** dodany żywioł ×3,5 zamiast ×5 punktu affiksu (`HellEconomy.AddedElementScale`); dodane obrażenia fizyczne bez zmian. Działa przy losowaniu: przedmioty z zapisu trwającego podejścia zachowują wylosowane wartości.

| Inwestycja, po zmianie | Fizyczna | Ogień | Mróz | Żywioł / fizyczna |
|---|---:|---:|---:|---:|
| 6 affiksów | 2566 | 2335 | 2341 | ×0,91 |
| Wszystkie sloty | 2978 | 4000 | 4036 | **×1,35** |

Pełna specjalizacja w żywiole nadal się opłaca, a przy częściowej droga fizyczna jest nieco lepsza.
