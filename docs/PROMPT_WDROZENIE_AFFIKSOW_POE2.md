# Prompt wdrożenia affiksów specjalizacyjnych i rzeczywistych obrażeń żywiołowych

Data przygotowania: 2026-11-01. Dokument jest zadaniem dla agenta wdrażającego; samo jego utworzenie nie oznacza wdrożenia mechanik.

## 1. Zadanie i zakres

Pracujesz w `C:\GIT\Gry\Turris_Damnatorum`. Wdróż poniższy pakiet w danych, kodzie, interfejsie i zapisach gry. Doprowadź pracę do sprawdzonego wyniku, nie kończ na planie ani samych definicjach affiksów.

Cel: przedmioty mają wspierać różne sposoby gry — specjalistę żywiołu, wojownika z tarczą, ciężką broń przełamującą postawę, maga rzucającego szybciej i przywoływacza. Zachowaj znaczenie animacji, wyboru momentu ataku, zasobów i uniku.

Zakres obowiązkowy:

1. Napraw cztery grupy dodawanych obrażeń żywiołowych: ogień, mróz, błyskawice, ziemia. Mają dodawać rzeczywisty składnik danego żywiołu zamiast premii do `PhysicalDamage`.
2. Dodaj pięć rodzin affiksów: przebicie wybranego żywiołu, oszczędniejszy aktywny blok tarczą, obrażenia postawy ciężkiej broni, szybkość rzucania czarów, obrażenia sług. Przebicie to cztery odrębne grupy; cały pakiet obejmuje osiem nowych grup.
3. Zapewnij zgodność losowania, jakości przedmiotów, obu rąk, technik broni, czarów HU/Turris, cech klasowych, unikatów i snapshotów.
4. Uzupełnij polskie opisy, porównywanie wyposażenia, dokumentację affiksów oraz raport z weryfikacji.

Poza tym zadaniem pozostają: efektywność kosztu many, skracanie negatywnych statusów, wydłużanie stref/sług, nowe pociski i łańcuchy, krytyki, automatyczne czary, nowe unikaty, nowy HUD, zmiany modeli i klipów źródłowych. Nie rozszerzaj wdrożenia o całą wcześniejszą listę inspiracji.

## 2. Zasady pracy

- Najpierw przeczytaj aktualne `docs/PROGRESS.md`, potem `README.md` i obowiązujące `AGENTS.md`. Sprawdź bieżący kod; podane dalej lokalizacje są mapą z dnia przygotowania promptu.
- Zastosuj skill `hack-and-slash-design`. Jeżeli rzeczywiście będziesz sterować Unity lub zmieniać jego efekty, zastosuj też właściwe instrukcje Unity/VFX, uwzględniając Windows i faktyczną wersję silnika.
- Unity: `6000.6.3f1`, `C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe`.
- Kod, UI, komentarze i dokumentacja po polsku; identyfikatory techniczne mogą zachować konwencję kodu.
- W drzewie są inne, rozpoczęte zmiany, m.in. cechy klasowe i unikaty. Zrób przegląd różnic przed edycją. Nie resetuj, nie nadpisuj i nie usuwaj cudzej pracy. Nie wykonuj commit/push bez prośby.
- Nie uruchamiaj `TurrisSetup.RunBatchForce` bez osobnej zgody. Nie zmieniaj repozytorium Hell Unleashed, z którego importujemy dane.
- Testy przez `tools/run_tests.py` wymagają zamkniętego edytora — poproś użytkownika o zamknięcie przed ich uruchomieniem. Nie zamykaj sam jego edytora. Kontynuuj pozostałe prace, kiedy czekasz.
- Wielogodzinne serie botów są wstrzymane. Ten prompt ich nie wznawia. Krótkie, deterministyczne testy scenariuszy są częścią wdrożenia.
- Parametry nowych affiksów są **hipotezami startowymi**. Możesz je lokalnie dostroić po pomiarach, zachowując zakres funkcjonalny; zapisz wartości przed/po i przyczynę. Nie kompensuj problemów podnoszeniem życia/obrażeń wszystkich przeciwników.

## 3. Potwierdzony stan wyjściowy i mapa kodu

| Obszar | Miejsce i uwagi |
|---|---|
| Lista skrócona i pełna | `docs/AFFIKSY_SKROT.md`, `docs/AFFIKSY_PRZEDMIOTOW.md`: 131 rekordów w 46 grupach przed tym pakietem. |
| Dane i importer | `Assets/TurrisDamnatorum/Resources/HellCatalog/affixes.json`, `tools/art/import_hu_economy.py`; importer odtwarza dane z HU. |
| Mapowanie i losowanie | `Scripts/Runtime/Run/HellEconomy.cs`: `Map`, `MapEffect`, `Supported`, `RollAffixes`, `Category`, `TagsOf`. Wszystkie dalsze ścieżki `Scripts/…` są względem `Assets/TurrisDamnatorum/`. |
| Statystyki i efekty | `Scripts/Runtime/Data/Enums.cs`, `CombatData.cs`, `Names.cs`; `Scripts/Runtime/Stats/Stats.cs`, `Equipment.cs`, `BuildSnapshot.cs`. |
| Trafienie i obrona | `Scripts/Runtime/Combat/DamageResolver.cs`: `HitData`, `DamageResolver`; `Enemies/EnemyBrain.cs`, `Combat/StatusEffects.cs`. |
| Tworzenie trafień gracza | `Scripts/Runtime/Player/PlayerCombat.cs`: `SkillHitData`, `PrepareOutgoing`, ataki obu rąk; pliki `PlayerCombat.Moveset.cs`, `.Hero.cs`, `.Seals.cs`, `.Features.cs`, `.Uniques.cs`. |
| Czary HU | `Scripts/Runtime/Seals/HuCast.cs`, `HuSkillCaster.cs`, `HuRuntimes.cs`, `HuShot.cs`, `HuWorld.cs`. |
| Sługi | `Scripts/Runtime/Combat/Servant.cs`, `ClassFeatures/Features.Summoner.cs`, `Uniques/Powers.Summoner.cs`. |
| Zapis | `Scripts/Runtime/Run/RunSnapshot.cs`, `Stats/Equipment.cs`; prześledź także klonowanie, profil i przechowywanie przedmiotów. |
| Interfejs i testy | `Scripts/Runtime/UI/GameUI.Economy.cs`, `GameUI.cs`; testy `EconomyTests`, `RunSnapshotTests`, `HeroSkillPlayTests`, `HuSkillCasterPlayTests`, testy cech i unikatów. |

Ważne fakty do zachowania w projekcie:

- `Map` mapuje obecnie płaskie `FireDamage/WaterDamage/AirDamage/EarthDamage` na `PhysicalDamage`, po 5 punktów procentowych na punkt HU. Ich procentowe odmiany są osobnymi premiami do żywiołów.
- `HitData` ma `physical`, `magic`, `element`, `element2`, `element2Share`; reprezentuje najwyżej dwa różne żywioły części magicznej. To za mało dla dowolnej kombinacji czterech affiksów i zaklętej broni.
- Penetracja poszczególnych żywiołów już istnieje jako efekty pasywne, lecz `PrepareOutgoing` sprowadza ją do jednej ważonej wartości `penetration`. Dla mieszanych obrażeń może ona działać na niewłaściwy składnik. Nowy pakiet wymaga obliczenia osobno dla każdego żywiołu.
- Istnieje `BlockCostReduction`, wykorzystywany do zmiany stabilności gardy w `BuildCalculator`. Ponowne mnożenie tej samej redukcji w odbiorze trafienia byłoby błędem.
- Jakość HU skaluje affiksy przez `1 + 0,25 × huQuality`. Typowo jakość bossa 2 oznacza ×1,5. Poziom broni i jakość HU to dwa różne mechanizmy.
- Snapshot zapisuje wynikowe `affixes`, `affixEffects`, `affixNames`, nie pełną historię losowania z pewnym id i wartością każdego affiksu. Nie wolno zakładać, że stare obrażenia fizyczne można bezbłędnie rozbić na żywioły.
- Sługi zwykle znikają po czasie i nie mają zdrowia. Unikat `Serce szóstego żebra` wprowadza śmiertelnych sług. Nie dodawaj wszystkim życia ani odporności przy okazji tego pakietu.

## 4. Katalog nowych affiksów

### 4.1. Parametry startowe

W tabeli są **wartości przed jakością HU**, w jednostkach widocznych w Turris. Procent premii i punkt procentowy przebicia to różne jednostki. Dane importowe mogą przechowywać ułamki, ale adapter ma przeliczać je dokładnie raz.

| Grupa | Rodzaj | T1 / piętro | T2 / piętro | T3 / piętro | Waga grupy |
|---|---|---|---|---|---|
| `fire_pen` | przedrostek | 4–5 p.p. / 3 | 6–8 p.p. / 10 | 9–10 p.p. / 18 | 0,15 |
| `water_pen` | przedrostek | 4–5 p.p. / 3 | 6–8 p.p. / 10 | 9–10 p.p. / 18 | 0,15 |
| `air_pen` | przedrostek | 4–5 p.p. / 3 | 6–8 p.p. / 10 | 9–10 p.p. / 18 | 0,15 |
| `earth_pen` | przedrostek | 4–5 p.p. / 3 | 6–8 p.p. / 10 | 9–10 p.p. / 18 | 0,15 |
| `poise_damage` | przedrostek | +10–14% / 1 | +15–19% / 8 | +20–25% / 16 | 0,50 |
| `minion_damage` | przedrostek | +15–19% / 1 | +20–24% / 8 | +25–30% / 16 | 0,50 |
| `shield_block_cost` | przyrostek | −10–12% / 1 | −13–16% / 8 | −17–20% / 16 | 0,60 |
| `cast_speed` | przyrostek | +5–7% / 1 | +8–10% / 8 | +11–12% / 16 | 0,70 |

Przebicie czterech żywiołów ma łącznie wagę 0,60, aby dodanie czterech wariantów nie zwiększało czterokrotnie częstości całej rodziny. To nadal waga względna w aktualnej puli przedmiotu, nie obietnica 60% szansy.

Każda grupa ma trzy rekordy o trwałych, nowych id, np. `affix.turris.fire_pen.t1`. Dopasuj identyfikatory do konwencji repo, zachowując brak kolizji i niezmienność po wydaniu. Oczekiwany przyrost: 24 rekordy, 8 grup; przy niezmienionym stanie wyjściowym 155 rekordów, 54 grupy, w tym 92 przedrostki w 30 grupach i 63 przyrostki w 24 grupach. To kontrola kompletności, nie powód do usuwania rekordów dodanych równolegle.

### 4.2. Dopuszczalne przedmioty i działanie

- Przebicie: broń, fokus/grymuar w drugiej ręce, amulet. Wariant jest niezależny od wrodzonego żywiołu broni — fizyczny miecz może później dostać ogień. Nie losuj na tarczach, nożach drugiej ręki, zbrojach ani pierścieniach. Poszczególne żywioły mogą współistnieć na jednym przedmiocie, kosztem osobnych przedrostków.
- Postawa: tylko broń z rodziny ciężkiej broni wręcz według `WeaponFamilies`. Nie traktuj wszystkich broni dwuręcznych jako ciężkich: łuk i kostur nie kwalifikują się automatycznie.
- Sługi: hełm lub grymuar do przywołań. Grymuar rozpoznawaj po rzeczywistej umiejętności/rodzinie, nie po przetłumaczonej nazwie. Bonus na hełmie może wypaść dowolnej klasie; brak aktualnego sługi nie blokuje losowania.
- Blok: wyłącznie tarcza, nie dowolny przedmiot kategorii `Offhand`.
- Rzucanie: broń będąca fokusem albo grymuar w drugiej ręce. Nie miecz żywiołu, nie tarcza, nie nóż do rzucania.

Wymagania opisują **przedmiot**, nie klasę gracza. Każda klasa może zmienić build. Uwaga: obecne `RequiredTags.Any(...)` oznacza OR. Nie próbuj wyrażać koniunkcji przez dopisanie dwóch tagów do tej listy. Dodaj wąski, testowalny filtr kwalifikacji albo jawne wymagania z zachowaniem dotychczasowej semantyki starych danych. Kategoria `Offhand` i enum `OffHand` mają inną pisownię — użyj prawdziwych wartości.

Nadaj wszystkim nowym rekordom polskie nazwy. Przykładowe rodziny nazw: „Przenikający żarem”, „Przenikający szronem”, „Przenikający burzą”, „Przenikający skałę”, „Kruszący”, „Władczy”, „Bastionu”, „Inkantacji”. Odmień je lub użyj neutralnej prezentacji w tooltipie, żeby nie tworzyć błędnych gramatycznie nazw przedmiotów. Nie lokalizuj przy tej okazji całego katalogu niezwiązanego ze zmianą.

### 4.3. Trwałe źródło danych

Nie dopisuj wyłącznie rekordów do wygenerowanego `affixes.json`: ponowny import HU nie może usuwać zmian.

Przygotuj lokalną nakładkę danych Turris, np. plik w `tools/data/`, i deterministyczne scalanie jej przez importer. Nakładka ma zawierać nowe rekordy, polskie nazwy i ewentualne jawne nadpisania istniejących rekordów. Waliduj id, grupę, rodzaj, tier, zakres, wagę, kategorię i obsługę statystyki. Nie nadpisuj błędnie kolidującego id po cichu.

Źródło Hell Unleashed pozostaje tylko do odczytu. Gdy nie jest dostępne, sprawdź scalanie na fixture lub kopii już zaimportowanych danych; nie zapisuj niepełnego katalogu. Dwa scalenia dają ten sam wynik, bez duplikatów. Zmiana puli nie musi zachowywać identycznego wyniku starych seedów, ale po zmianie identyczny seed i dane mają dawać identyczne losowanie.

## 5. Rzeczywiste dodane obrażenia żywiołowe

### 5.1. Kontrakt wartości i zakres

- Zachowaj na start istniejące tiery, progi, wagi i zakresy `fire_add/water_add/air_add/earth_add`: nominalnie +10–70% przed jakością, zgodnie z aktualnym katalogiem. Zmienia się ich znaczenie mechaniczne. To nie jest +10–70 płaskich punktów obrażeń.
- Grupa `phys_add` oraz procentowe `fire_pct/water_pct/air_pct/earth_pct` pozostają osobnymi mechanikami. Nie zmieniaj procentowej premii żywiołu w dodane obrażenia.
- Wprowadź osobne statystyki dodawania żywiołów do obrażeń broni, np. `AddedWeaponFirePercent` itd. Nowe wartości enuma dopisuj na końcu. Nie używaj `FireDamage` równocześnie jako wielkości dodatku i premii mnożącej ten dodatek.
- Premie z wyposażenia są globalne dla własnych ataków bronią, zgodnie z obecnym agregowaniem statystyk; atak każdej ręki ma jednak własną bazę. Nie mnoż wartości z drugiej ręki przez bazę pierwszej.
- Obejmij zwykłe ataki wręcz, podstawowe strzały łuku/różdżki i techniki oparte na obrażeniach broni. Nie dokładaj dodatku jako osobnej paczki do czarów, sług, statusów, reakcji ani wtórnych efektów cech/unikatów.
- Techniki HU i Turris mają odmienne ścieżki skali. Wyznacz raz udział obrażeń broni, tak aby dodatek nie wszedł jednocześnie do skali DPS oraz drugi raz jako dodatkowa paczka. Czar może nadal korzystać z dotychczasowej skali fokusu, ale nie otrzymuje osobno dodatku przeznaczonego dla broni.

### 5.2. Kolejność obliczeń

Wyznacz `B`: bazę obrażeń danego ataku broni po skalowaniu atrybutów, poziomu broni, wymagań i właściwych mnożników broni, lecz przed `PhysicalDamage`, premiami żywiołowymi, globalnymi i warunkowymi. Współczynnik lekkiego/ciężkiego ataku albo techniki wchodzi do `B` dokładnie raz.

Dla każdego żywiołu `e`:

`dodatek[e] = B × suma_dodanych_procentów[e] / 100`

Następnie:

1. Utwórz dotychczasowe bazowe składniki broni i dodaj nowe składniki. Nie zmieniaj niepowiązanych zasad bazowego podziału fizyczne/magia.
2. Wykonaj jawne konwersje i zaklęcie broni we właściwym miejscu istniejącego pipeline. Konwersja ogień → mróz obejmuje też dodany ogień, zachowuje sumę przed premiami i nie pozostawia kopii ognia.
3. Premia fizyczna zwiększa część fizyczną zgodnie z przyjętą regułą bazowego ciosu. **Nie zwiększa nowego dodatku żywiołowego.** Premia do żywiołu zwiększa odpowiadający składnik, w tym nowy dodatek, raz.
4. Premie globalne i przeciw konkretnemu celowi działają w istniejących punktach, raz. Nie nakładaj ponownie całego `PrepareOutgoing` na gotową kopię trafienia.
5. Przebicie i odporność oblicz osobno dla każdego składnika; zachowaj ogólną obronę magiczną i reguły gardy.

Wyodrębnij obliczenia do wspólnych funkcji, zamiast kopiować formułę do lekkiego ataku, ciężkiego ataku, techniki i tooltipa. Jeśli obecne `WeaponDamage` nie ujawnia `B`, rozdziel bazę i premie jawnie, z testem regresji dla ataków bez nowych dodatków.

Przykład referencyjny bez konwersji i innych premii: `B=100`, broń czysto fizyczna, premia fizyczna +20%, dodany ogień +20%, premia ognia +50% → fizyczne 120, ogień 30, suma 150. Ogień nie wynosi 36. Przy jakości HU 2 i affiksie dodatku nominalnie +20% efektywny dodatek wynosi +30%; nie skaluj go jakością drugi raz.

### 5.3. Wiele żywiołów i jedno zdarzenie trafienia

Zaprojektuj kanoniczną reprezentację składników obsługującą fizyczne, magię bez żywiołu i wszystkie cztery żywioły naraz, np. niewielką strukturę wartościową. Nie ograniczaj kombinacji do dwóch żywiołów i nie odrzucaj najsłabszego. Nie rozbijaj jednego ciosu na pięć osobnych wywołań `HitQuery.Apply`.

Jedno trafienie pozostaje jednym trafieniem dla: uniknięcia, parowania, bloku, postawy, hitstopu, reakcji, kradzieży życia, zliczania zabójstwa, pochodzenia i haków cech/unikatów. Składniki służą poprawnemu liczeniu obrażeń. Same dodatki nie nakładają podpalenia/chłodu/porażenia/zachwiania.

Przejrzyj wszystkie odczyty i zapisy `physical`, `magic`, `element`, `element2`, `Total`, `Weighted`, `ScaleMagicByElement`, konwersje, wybuchy, odpryski i kopie. Jeśli utrzymujesz adapter do starego formatu, ma mieć jednoznaczną synchronizację; nie utrzymuj dwóch rozchodzących się wersji tych samych obrażeń. Nie współdziel mutowalnej tablicy składników między kopiami trafienia. Nie dokładaj alokacji list/słowników dla każdego składnika każdego ciosu.

## 6. Kontrakty pięciu rodzin affiksów

### 6.1. Przebicie żywiołu

- Wykorzystaj istniejące typy `PenetrationFire/Frost/Lightning/Earth` i ich agregowanie, ale przenieś wartość każdego żywiołu aż do obliczenia jego odporności.
- Przebicie 10 p.p. oznacza odporność 50% → 40%, a nie 50% → 45%.
- Ogień z przebiciem ognia nie korzysta z przebicia mrozu. Nie uśredniaj przebicia między żywiołami, nawet jeśli potem ważysz obrażenia.
- Uwzględnij istniejące osłabienia odporności, np. `FeatureResistCut` i `SunderElementCut`, oraz wrodzone słabości demonów. Penetracja nie tworzy podatności poniżej zera; istniejąca podatność nie znika ani nie dostaje dodatkowego mnożnika od penetracji.
- Dotyczy bezpośrednich trafień gracza, w tym czarów; nie przenoś automatycznie na krwawienie, podpalenie i inne tyknięcia statusów. Zachowaj odrębne istniejące reguły stref i aur; opisz je w raporcie. Nie wprowadzaj nowego dziedziczenia penetracji przez sługi.
- Test liczbowy: ogień 20, mróz 30, odporności 50% i 20%, przebicie tylko ognia 10 p.p. → 12 ognia + 24 mrozu = 36 przed ogólną obroną magiczną i gardą. Dla podatności ognia +25% oraz dowolnego przebicia ognia składnik 20 pozostaje 25.

### 6.2. Oszczędniejszy aktywny blok tarczą

- Affiks działa tylko, gdy właśnie wyposażona tarcza dostarcza aktywną gardę. Nie zwiększa szansy biernego bloku, redukcji zdrowia, okna parowania ani ochrony przed atakiem nieblokowalnym. Nie wzmacnia bloku samą bronią po zdjęciu tarczy.
- Zachowaj istniejącą mechanikę kosztu `guardLoad × stabilityMultiplier`. Wspólny helper ma obliczać końcowy koszt przed rozstrzygnięciem `GuardBroken`, a UI ma używać tej samej reguły.
- Nową część affiksową zastosuj **raz** do kosztu wynikającego z dotychczasowych reguł: `kosztNowy = kosztLegacy × (1 − clamp(A/100, 0, 0,50))`, gdzie `A` jest sumą efektywnych wartości nowych affiksów po jakości. Limit 50% dotyczy tylko tej nowej części; nie zmienia starych perków ani ich limitów. To roboczy limit do testów.
- Nie dodawaj `A` równocześnie do starego `BlockCostReduction` i do powyższego mnożnika. Rozróżnij pochodzenie nowej premii osobnym polem/statystyką albo równoważnym pewnym mechanizmem. Wykorzystaj istniejący kod gardy, nie twórz drugiego systemu bloku.
- Przykład: bazowy koszt 40, stary perk −20% → 32, nowy affiks −20% → 25,6. Po zdjęciu tarczy znika wyłącznie nowa część. Jeśli legacy samo daje koszt zero, raportuj to jako istniejącą regułę; ten pakiet nie ma jej po cichu naprawiać.

### 6.3. Obrażenia postawy ciężkiej broni

- Nowa premia procentowa mnoży `HitData.poiseDamage`, nie obrażenia zdrowia i nie liczbę warstw `Stagger`.
- Działa dla własnego zwykłego ataku ciężką bronią wręcz oraz techniki rzeczywiście wykonanej ciężką bronią. Przy dwóch broniach liczy się broń wykonująca dane uderzenie. Nie wystarczy, że ciężka broń jest schowana w Arsenale.
- Nie wpływa na parowanie, ripostę, czary, sługi, tyknięcia, reakcje i wtórne trafienia cech/unikatów. Nie dodaje postawy do źródła, które miało jej zero. Fala ładowanego ataku jest efektem wtórnym i w tym pakiecie nie otrzymuje tej premii.
- Sumuj kwalifikujące się premie addytywnie, potem zastosuj jeden mnożnik. Zachowaj dotychczasowe mnożniki ruchu/ładowania. Przykład: postawa 40, ładowanie ×1,5, affiks +25% → 75; zdrowie bez zmian.
- Nie skracaj odporności bossa na kontrolę i nie zmieniaj jego regeneracji postawy. Sprawdź czas do przełamania i możliwość utrzymania ciągłej kontroli dla maksymalnego legalnego zestawu.

### 6.4. Szybkość rzucania czarów

- Nowa statystyka dotyczy przygotowania **ręcznie uruchomionego czaru** (`SpellDefinition.IsSpell`), niezależnie od klasy. Nie dotyczy technik, ataku podstawowego różdżką, flaszek, parowania, uniku i cechy pod G.
- `czasPrzygotowania = dotychczasowyCzasPoSkalach / (1 + C/100)`, gdzie `C` jest sumą premii po jakości. Czas zero zostaje zerem. Premia +10% daje 0,6/1,1 ≈ 0,54545 s, a nie 0,54 s.
- W pierwszym pakiecie nie skracaj recovery, `cancelAfter`, aktywnego czasu umiejętności, kanału, odstępów impulsów, lotu pocisku, czasu strefy ani odnowienia. Nie obniżaj kosztu zasobu.
- Skorzystaj ze wspólnej funkcji efektywnego przygotowania; nie zmieniaj globalnego `ScaleStartup`, bo używają go inne akcje.
- Obsłuż czary HU i Turris, ręczny rzut po zmianie formy/pieczęci, dwie kolejki `TwoBeatsFeature` oraz ponowne przeliczenie buildu. Kopie, echo, wyzwalacze i gotowy ładunek zaklęcia nie dostają drugiego skrócenia i nie stają się dodatkowymi ręcznymi rzutami.
- Zgodność animacji: klip ma osiągnąć moment wyzwolenia wraz z gameplayem. Dopasuj fazę przygotowania do rzeczywistego zegara akcji; nie przyspieszaj całego świata/efektu. Efekt zbierania mocy korzysta z efektywnego czasu.
- Anulowanie unikiem przed wyzwoleniem zachowuje istniejące zasady zwrotu many/ładunku i wydanej wytrzymałości. Po wyzwoleniu nie można otrzymać darmowego zwrotu. Dług many i paragony kosztu nadal działają jak przed zmianą.

### 6.5. Obrażenia sług

- Nowa premia zwiększa obrażenia własnych sług: chochlików, ogarów, golemów i składników chimery. Nie zwiększa ich liczby, czasu, zdrowia, pancerza, szybkości ani aury redukcji obrażeń właściciela.
- W tym pakiecie użyj **migawki przy przyzwaniu**: bieżąca premia trafia do budżetu obrażeń nowego sługi raz. Zmiana wyposażenia nie zmienia istniejącego sługi, zmienia kolejne przyzwania. UI ma to wyjaśnić. Jeżeli aktualna architektura została już zmieniona na inną politykę, potwierdź konflikt w raporcie przed utrwaleniem równoległych reguł.
- Mnożnik nakładaj przy tworzeniu własnego budżetu ataku sługi, po istniejącym skalowaniu, raz. Obrażenia pochodne dziedziczą ten budżet, nie naliczają premii ponownie. Nie zmieniaj zasad przenoszenia jego obrażeń na statusy.
- Chimera zachowuje osobne migawki składników; scalenie i rozdzielenie nie mnoży ich ponownie. Komenda dyrygenta nie jest nowym przyzwaniem. Sługa śmiertelny z unikatu dostaje ten sam mnożnik obrażeń co zwykły, bez premii do zdrowia.
- Wcielenie gracza w sługę pozostaje atakiem gracza i nie dostaje tego affiksu. Wybuchy/wyładowania unikatów nie otrzymują osobnego mnożnika „obrażeń sług”; jeśli liczą się jako część ich własnego budżetu, dziedziczą go raz. Rozpisz te przypadki dla istniejących mocy.
- Do przyszłych danych dopisz statystykę na końcu enuma i popraw `StatCalculator`/agregowanie/formatowanie. Nie dodawaj premii wyłącznie do tekstu lub bezpośrednio do jednej ścieżki `Summon`.

## 7. Jakość, sumowanie i losowanie

- Wszystkie nowe ciągłe premie skalują się jakością HU dokładnie raz. T3 szybkości +12% przy jakości 2 daje +18%; postawa +25% daje +37,5%; sługi +30% dają +45%; przebicie 10 p.p. daje 15 p.p.; blok 20% daje 30% przed limitem nowej części.
- Poziom ulepszenia broni nadal zwiększa jej bazę według istniejącej reguły. Nie mnoży drugi raz samej procentowej wartości affiksu.
- Unikalność grupy dotyczy przedmiotu. Nie powielaj T1 i T3 tej samej grupy. Zachowaj regułę rzadkości, progi legendarnych, wagę grupy i lepsze `q` na legendarnych.
- Zachowaj istniejące zaokrąglanie niezwiązanych statystyk. Dla nowych danych zadeklaruj precyzję, żeby wynik nie tracił działania po imporcie. Nie naprawiaj globalnie przy okazji znanego zaokrąglania regeneracji.
- Testuj wartości nominalne i efektywne, pojedynczy przedmiot i cały maksymalny legalny zestaw. W szczególności dodatki żywiołów z kilku slotów mogą dać wysoki łączny budżet; nie zakładaj, że 70% jest globalnym limitem.
- Zdejmowanie/przekładanie sprzętu i zmiana Arsenału nie zostawia premii. Przedmiot trzymany w plecaku, sklepie lub nieaktywnym zestawie nie dostarcza statystyk.

## 8. Zapis, migracja i stare przedmioty

1. Dopisuj wartości enumów, nie przestawiaj starych. Dodaj wersję semantyki affiksów/egzemplarza, jeżeli jest potrzebna do jednoznacznego odczytu.
2. Nowy zapis ma zachować wynik losowania bez ponownego losowania po wczytaniu. Dodaj trwały zapis id/tieru i wylosowanych wartości nowych affiksów albo równoważną reprezentację zapewniającą pewną identyfikację i tooltip. Wersjonuj ją i używaj jednego źródła wartości; nie licz jednocześnie z metadanych i starych list wynikowych.
3. **Stare przedmioty bez wiarygodnych metadanych zachowują zapisane premie.** Nie odejmuj fizycznego bonusu na podstawie tłumaczonej nazwy, nie zgaduj elementu i nie przerzucaj kości ponownie. Wyjaśnij stary opis jako „Premia obrażeń broni ze starego zapisu”, jeśli sugerował nieistniejący żywioł.
4. Nowo wylosowane przedmioty używają poprawionej semantyki. Zwykłe ponowne zapisanie starego przedmiotu nie może udawać, że został już przeliczony. Polityka ma być idempotentna.
5. Obejmij wyposażenie, plecak, Arsenał, kopie i wszystkie faktyczne miejsca utrwalania egzemplarzy. Zachowaj uid, rzadkość, jakość, poziom, `powerSlot`, wybór cechy, dług i pozostałe dane runu.
6. Brak nowych pól/list w starym JSON ma działać. Nie wymagaj resetu profilu i nie usuwaj istniejących wyzwań/snapshotów. Przy wadliwych nowych metadanych nie przeliczaj po cichu losowej wartości.

## 9. Interfejs i prezentacja

- `Names.Stat`, `Names.Effect` i rzeczywiste tooltipy muszą obsłużyć wszystkie nowe typy. Surowe enumy, id affiksu i nazwy HU nie mogą zastępować opisu efektu.
- Pokazuj efektywną wartość po jakości, np. „Dodaje obrażenia od ognia równe 30% bazowych obrażeń broni”, „Trafienia ignorują 15 p.p. odporności na ogień”, „Przygotowanie czarów szybsze o 18%”, „Obrażenia przyzwanych sług +45% (przy przyzwaniu)”.
- Opis postawy wskazuje ciężką broń, opis bloku aktywną gardę tarczą. Wyświetl limit nowej części, kiedy faktycznie ogranicza efekt; porównanie przedmiotów nie może pokazywać zysku już zjedzonego limitem.
- Porównywanie nie może sprowadzić ognia i mrozu do identycznej liczby „obrażenia fizyczne”. Pokaż podział obrażeń broni i istniejące przydatne parametry bez przebudowy całego UI.
- W statystykach szybkość rzucania i odnowienie są osobnymi wartościami. Statystyka sług dotyczy nowych przyzwań, nie zmiany bieżącej migawki istniejącego sługi.
- Nie dodawaj automatycznych statusów ani rozbudowanych VFX tylko po to, by wyrazić typ dodatku. Jeśli używasz barwy istniejącego trafienia, niech nie sugeruje wyłączności jednego żywiołu w ciosie mieszanym.
- Sprawdź czytelność długiego tooltipa po polsku w 720p/1080p i obsługę padem. Nowe nazwy nie mogą zasłaniać porównania.

## 10. Testy obowiązkowe

Rozszerz istniejące zestawy i dodaj skupione testy dla nowych kontraktów. Poniższe punkty są scenariuszami, nie wymogiem jednego osobnego testu na każdy wiersz. Używaj stałych seedów i prawdziwych obliczeń gry, nie kopii implementacji w asercjach.

### 10.1. EditMode — dane, matematyka, zapis

| ID | Sprawdzenie |
|---|---|
| E01 | Każdy nowy rekord ma unikalne id, poprawną grupę/tier/próg/wagę/zakres oraz obsługiwaną statystykę; 8 grup × 3 tiery. |
| E02 | Macierz kwalifikacji: ciężka broń/łuk/kostur/miecz żywiołu/tarcza/grymuar przywołań/inny grymuar/nóż/hełm/amulet/pierścień. |
| E03 | Progi 1/3/8/10/16/18, liczba przedrostków/przyrostków i brak duplikatu grupy. Nie myl odblokowania tieru z gwarancją jego wylosowania. |
| E04 | Powtarzalne losowanie, scalenie importu bez duplikacji, brak zmiany źródeł HU; walidator odrzuca konflikt id. |
| E05 | Nowe ciągłe premie po jakości 0/1/2; poziom broni nie zwiększa dwukrotnie procentu affiksu. |
| E06 | Przykład B=100 z rozdziału 5, zero dodatku, cztery dodatki naraz, magia bez żywiołu, brak statusów z samego dodatku. |
| E07 | Konwersja ogień → mróz zachowuje sumę przed premiami; nowy mróz dostaje premię mrozu raz, nie również premię ognia. |
| E08 | Nierówne odporności i przebicia czterech żywiołów, przykład wyniku 36, podatność, brak zejścia penetracją poniżej 0. |
| E09 | Nowy blok i stary perk: wynik 25,6, próg przełamania, limit nowej części, brak efektu bez aktywnej tarczy. |
| E10 | Postawa 40 × 1,5 × 1,25 = 75, zdrowie bez zmian; źródła wyłączone i zero postawy pozostają bez nowej premii. |
| E11 | Rzucanie 0,6/1,1; technika, recovery, CD, impuls i czas zero bez zmian; suma kilku źródeł. |
| E12 | Nowy/stary snapshot, brak pól, ponowny zapis, migrowanie dwa razy, przedmioty w plecaku/Arsenale, uid i dług. |
| E13 | Zakładanie/zdejmowanie i obie ręce; dwie różne bazy broni, brak statystyk z nieaktywnego zestawu. |
| E14 | Jedno trafienie mieszane powoduje jeden wynik obrony, jedną obsługę postawy i jedno zdarzenie trafienia; brak lawiny proców. |

### 10.2. PlayMode — rzeczywiste ścieżki

| ID | Sprawdzenie |
|---|---|
| P01 | Fizyczny miecz z dodatkiem ognia kontra odporny i podatny demon; rzeczywiste zdrowie, blok i liczba zdarzeń. |
| P02 | Wszystkie cztery dodatki + wrodzony żywioł/zaklęcie broni: nic nie ginie, poprawne sumy po obronie. |
| P03 | Lekki/ciężki/ładowany atak, bieg i obie ręce; technika HU i Turris dostają dodatek raz, czar nie dostaje osobnej paczki. |
| P04 | Kopia, przebicie, odprysk i wybuch dziedziczą właściwy udział składników, bez ponownego naliczenia dodatków i globalnych premii. |
| P05 | Tarcza z affiksem i perk redukcji bloku, okolice progu przełamania, blok bronią po zdjęciu tarczy, atak nieblokowalny i bierny blok. |
| P06 | Ciężka broń przełamuje postawę zgodnie z premią; łuk/kostur/czar/riposta/fala wtórna nie korzystają. Boss zachowuje dotychczasowe reguły kontroli. |
| P07 | Czar Turris i HU z premią czasu, niski/wysoki framerate i pauza; gameplay i animacja wyzwalają w zgodnej fazie. |
| P08 | Anulowanie czaru tuż przed/po wyzwoleniu: właściwa liczba trafień, zwrot many/ładunku i wytrzymałość; brak darmowego rzutu. |
| P09 | Dwa takty, echo/kopia/wyzwalacz i dług many: poprawny czas każdej ręcznej kolejki, bez drugiego skrócenia. |
| P10 | Każdy rodzaj sługi: obrażenia z affiksem, brak wpływu na atak gracza, migawka przed/po zmianie sprzętu. |
| P11 | Chimera i jej składniki, scalenie/rozdzielenie, komenda, śmiertelny sługa i Wcielenie; mnożnik występuje raz w zadeklarowanych źródłach. |
| P12 | Tooltipy i porównanie, nowy/stary przedmiot, jakość i limit bloku; zrzuty obejrzane, nie tylko zapisane. |

Zadbaj o regresję istniejących broni bez nowych dodatków, skilli, pieczęci, statusów, cech i unikatów. Zmiana oczekiwania testu wymaga wyjaśnienia nowej reguły; nie usuwaj asercji, żeby uzyskać zielony wynik.

### 10.3. Krótkie porównanie balansu

Przygotuj małe scenariusze na tych samych seedach i identycznym sprzęcie poza badaną premią. Minimum: grupa zwykłych przeciwników, elita, boss, silne zagrożenie defensywne, niedobór many/wytrzymałości. Nie uruchamiaj pełnych serii runów botów.

Porównaj zestaw bez premii, pojedynczy typowy affiks oraz maksymalny legalny zestaw po jakości. Zapisz:

- faktyczne obrażenia według składników i czas walki;
- liczbę przełamań/czas do pierwszego przełamania i udział czasu kontroli bossa;
- wytrzymałość za blok oraz liczbę bloków przed przełamaniem gardy;
- rzeczywisty czas do wyzwolenia czaru, koszt i liczbę rzutów w stałym oknie;
- DPS i czas obecności sług, oddzielnie przed i po zmianie wyposażenia;
- dostępność nowych grup w deterministycznej próbce losowań na wczesnym/środkowym/późnym piętrze, bez przedstawiania wag jako procentu dropu.

Nie ma z góry przyjętego wymogu identycznego DPS tych buildów. Blokerem jest: wielokrotne naliczenie, martwy affiks w deklarowanym zastosowaniu, nowy darmowy blok niezależny od legacy, darmowe zasoby przez anulowanie, utrata składnika obrażeń albo nowa możliwość stałej kontroli bossa wynikająca z błędu. Wyraźną dominację po poprawnym naliczeniu opisz i dostrój wąsko na podstawie danych.

## 11. Wykonanie i weryfikacja w Unity

Kolejność pracy:

1. Audyt aktualnego drzewa i punktu wyjścia; krótka mapa rzeczywistych ścieżek obrażeń/animacji/zapisu.
2. Kontrakty danych, jednostki, nakładka importu i testy matematyki/migracji.
3. Reprezentacja składników, rzeczywiste dodatki i poprawne przebicie; regresja starych trafień.
4. Nowy blok, postawa, szybkość rzucania oraz sługi, ze sprawdzeniem cech i unikatów.
5. Katalog, losowanie, polskie opisy, porównywanie i zgodność zapisu.
6. Ukierunkowane testy, krótka macierz pomiarów, zrzuty i wymagane pełne sprawdzenia.
7. Aktualizacja dokumentacji i samokontrola różnic.

Po zamknięciu edytora przez użytkownika uruchom:

```powershell
python tools/run_tests.py EditMode
python tools/run_tests.py PlayMode window
```

Do kontroli wizualnej użyj osobnego katalogu, np.:

```powershell
$env:TURRIS_SHOT_DIR = 'TestResults/affiksy-specjalizacyjne/zrzuty'
python tools/run_tests.py PlayMode gfx
Remove-Item Env:TURRIS_SHOT_DIR
```

Możesz najpierw użyć filtra odpowiednich galerii/testów. Obejrzyj PNG: podział obrażeń w tooltipie, porównanie, przygotowanie czaru przed/po i chwilę wyzwolenia. Sam zrzut końcowej eksplozji nie dowodzi zgodności czasu. Zachowaj osobne logi/wyniki tego zadania, najlepiej przez `TURRIS_TEST_RESULT_DIR`.

Jeżeli znany test zawiedzie, porównaj z aktualnym punktem wyjścia i podaj dowód. Nie deklaruj starych porażek wyłącznie na podstawie wpisu w `PROGRESS.md`. Jeśli środowisko uniemożliwia weryfikację, oznacz konkretną część jako niesprawdzoną i dostarcz gotową resztę; nie podawaj wyniku testów, których nie uruchomiono.

## 12. Dokumentacja i kryteria odbioru

Zaktualizuj:

- `docs/AFFIKSY_PRZEDMIOTOW.md`: rzeczywisty nowy katalog, nazwy, tiery, progi, wagi, sloty, tagi/filtry, jednostki i jakość;
- `docs/AFFIKSY_SKROT.md`: nowe grupy i poprawne znaczenie dodatków, z rozróżnieniem starych zapisów;
- `README.md`: tylko reguły, które zmieniły się dla gracza;
- `docs/PROGRESS.md`: co wdrożono, testy, metryki, ryzyka i następne kroki, bez kasowania poprzednich wpisów;
- nowy `docs/RAPORT_AFFIKSY_SPECJALIZACYJNE.md`: źródła danych, kontrakty sumowania, polityka migracji, zrzuty, wyniki przed/po i niezweryfikowane części.

Pakiet jest gotowy, kiedy:

1. Nowe grupy realnie wypadają na właściwych przedmiotach i nie wymagają konkretnej klasy.
2. Cztery dodatki dają cztery rzeczywiste, prawidłowo bronione składniki; jeden cios nadal ma jedno rozstrzygnięcie trafienia.
3. Przebicie działa wyłącznie na właściwy żywioł i we właściwej jednostce.
4. Blok, postawa, przygotowanie czaru i sługi realizują wszystkie zadeklarowane ograniczenia, także po jakości i przy cechach/unikatach.
5. Przedmiot zachowuje swoje wartości po zapisie/wczytaniu, stare zapisy nie są losowane ponownie i nie wymagają resetu profilu.
6. Ponowny import nie usuwa pakietu ani nie modyfikuje źródła HU.
7. UI pokazuje działanie rzeczywistego buildu, a wymagane sprawdzenia i obejrzane zrzuty mają wskazane wyniki.

Końcowa odpowiedź agenta ma krótko podać zakres wykonania, istotne decyzje techniczne, wyniki testów/pomiarów, odnośnik do raportu i dokładnie to, czego jeszcze nie udało się zweryfikować.

## 13. Inspiracje i granice ich użycia

Projekt jest adaptacją do Turris, nie wiernym przeniesieniem systemu PoE. Poniższe źródła potwierdzają rodzaje mechanik, nie proponowane tutaj liczby, sloty i reguły:

- [PoE2DB — ogłuszenie i narastanie ogłuszenia](https://poe2db.tw/us/Stun).
- [PoE2DB — szybkość rzucania](https://poe2db.tw/us/Cast_speed).
- [PoE2DB — sługi i ich modyfikatory](https://poe2db.tw/us/Minions).
- [PoE2DB — przebicie odporności](https://poe2db.tw/us/Resistance_penetration).
- [Adres podany przez autora — archiwalna wiki PoE](https://pathofexile-archive.fandom.com/wiki/Item_Prefixes_and_Suffixes); nie udało się bezpośrednio odczytać tej strony podczas analizy. Nie przedstawiaj jej jako aktualnej listy affiksów PoE 2.

Oszczędniejszy aktywny blok i premia do naszej postawy są lokalnymi rozwiązaniami dopasowanymi do walki w Turris. Dalsze odczytanie internetu nie jest potrzebne do wykonania tego promptu; źródłem prawdy dla wdrożenia jest bieżące repozytorium i powyższe kontrakty.
