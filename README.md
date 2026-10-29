# Turris Damnatorum — prototyp

Gra 3D dla jednego gracza, w której walka w stylu souls-like łączy się ze strukturą rogue-lite. Gracz wspina się na wieżę: **30 pięter – wierny port 30 fal Hell Unleashed**, boss co trzecie piętro, na końcu Arcydiabeł. Po śmierci traci wszystko, co zdobył w danym podejściu, ale zachowuje odblokowania i popiół.

- **Silnik:** Unity **6000.6.3f1**, jedyna wersja zainstalowana na tej maszynie. Kod nie używa API specyficznego dla 6.6, więc powinien działać także na Unity 6 LTS (6000.3), ale tej wersji nie sprawdzałem.
- **Pakiety:** `com.unity.inputsystem` 1.20.0 i `com.unity.test-framework` 1.8.0, do tego moduły wbudowane. Nie ma zależności płatnych ani zewnętrznych.
- **Grafika:** URP (`TurrisURP`), przestrzeń Linear.
  - **Bohaterowie** to modele 3D z gry *Hell Unleashed* tego samego autora: 15 postaci, ich bronie oraz animacje Mixamo. Szczegóły w sekcji [Bohaterowie](#bohaterowie-z-hell-unleashed).
  - Wrogowie korzystają z kompletu 34 modeli/definicji demonów Hell Unleashed, ich wariantów i klipów oraz z wcześniejszych proceduralnych przeciwników Turris.
  - Pierwszy dziedziniec korzysta z [nowego zestawu gotyckiego anime](ArtSource/GothicAnimePoC/Arena/README.md):
    30 modułów 3D, malowany kamień, nocne niebo, ostrołuki i zamek w tle.
    Oryginalne areny i generator zachowano; przełącznik przywrócenia jest w menu **Turris → PoC → Arena gotycka**.
    Pozostałe areny są zbudowane w kodzie z tysięcy części.
  - Efekty korzystają z natywnych systemów cząsteczek Unity oraz tekstur i warstw przeniesionych z Hell Unleashed.
- **Stan prac i plan na kolejną sesję:** [`docs/PROGRESS.md`](docs/PROGRESS.md).
- **Styl graficzny i szablony przyszłych promptów:** [specyfikacja gotyckiego anime](docs/STYL_GRAFICZNY_GOTYCKIE_ANIME.md).
- **Koncepcje bohaterów:** [13 pozostałych klas w nowym stylu](ArtSource/GothicAnimePoC/Heroes/README.md) — osobne plansze, referencje i lokalna galeria.
- **Modele anime:** [berserker i bazowy mag wdrożeni na istniejących rigach i animacjach](docs/WDROZENIE_BOHATEROW_ANIME.md), ze złożonym chwytem broni i możliwością przywrócenia oryginałów.
  [Korekta wykończenia v2](ArtSource/GothicAnimePoC/StyleCorrections/README.md) zawiera rzeczywiste porównania Unity oraz przełącznik dawnych materiałów.
- **Migracja Hell Unleashed:** [plan pozostałych elementów](docs/PLAN_MIGRACJI_HELL_UNLEASHED.md),
  [cząsteczki i audyt 29 przeniesionych skilli](docs/MIGRACJA_EFEKTOW_HU.md),
  [wykonanie P0/P1 — etapy 1–3](docs/RAPORT_MIGRACJI_P0_P1.md).
  Przygotowanie runu pozwala wybrać wygląd niezależnie od klasy. Bestiariusz udostępnia próbę walki z każdym demonem bez nagród i zmian profilu.

## Uruchomienie

1. Otwórz folder `C:\GIT\Gry\Turris_Damnatorum` w Unity Hub (Add → wskaż folder) w wersji 6000.6.3f1.
2. Otwórz scenę `Assets/TurrisDamnatorum/Scenes/Main.unity` i naciśnij **Play**.
3. Assety treści i scena są już wygenerowane. Gdyby ich brakowało, użyj menu **Turris → Setup (utwórz brakujące assety i scenę)**. Pełne przywrócenie wartości domyślnych daje **Turris → Odtwórz domyślną treść**, ale ta opcja nadpisuje wprowadzone zmiany.
4. Profil gracza zapisuje się w `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Turris Damnatorum\turris_profile.json`. Pełna ścieżka jest też widoczna w menu głównym. Usuwa go opcja **Turris → Usuń profil gracza**.
5. Gdy scena nie ma przypisanego `GameConfig`, gra startuje z treścią domyślną zdefiniowaną w kodzie (`DefaultContent`) i wypisuje ostrzeżenie.

Budowanie pliku wykonywalnego: *File → Build Profiles*. Scena `Main` jest już dodana do listy.

### Testy

W edytorze: *Window → General → Test Runner*, zakładki EditMode i PlayMode.

Przez Unity MCP można uruchamiać testy i rendery w **już otwartym edytorze**. Pomocniczy klient
`tools/unity_mcp.py` wymaga pakietu Python `mcp` i działającego serwera Unity MCP (domyślnie
`http://127.0.0.1:8080/mcp`). `--list` pokazuje narzędzia, `--request <plik.json>` wykonuje listę
wywołań MCP, a `--wait --result <wynik.json>` czeka na zakończenie zleconych testów i zapisuje wynik.
Przykładowa lista wywołań: `[{"name":"run_tests","arguments":{"mode":"PlayMode","include_details":true}}]`.
Opcjonalne `--instance <identyfikator>` wskazuje konkretny edytor. Zamykanie Unity dotyczy wyłącznie
poniższego skryptu, który otwiera osobny proces edytora.

Z wiersza poleceń najwygodniej uruchomić skrypt z repozytorium. Wymaga Pythona 3, a projekt **nie może być otwarty w edytorze**:

```
python tools/run_tests.py EditMode           # tryb wsadowy, bez grafiki
python tools/run_tests.py PlayMode gfx       # tryb wsadowy z grafiką (zrzuty kamery działają, OnGUI nie)
python tools/run_tests.py PlayMode window    # okno edytora – jedyny tryb, w którym przechodzą testy menu padem
python tools/run_tests.py PlayMode window Turris.Tests.GamepadPlayTests   # filtr
```

Ciężkie pomiary uruchamia się tylko na żądanie zmienną środowiskową: benchmark skilli i pieczęci `TURRIS_BENCH_DIR=<katalog>` (do 90 min), galerie zrzutów `TURRIS_SHOT_DIR=<katalog>`. Sama obecność katalogów w `TestResults/` ich już nie włącza (pełny PlayMode w oknie: ok. 9 min zamiast ponad 40).

Wyniki i log trafiają do `TestResults/`. Zmienna środowiskowa `TURRIS_SHOT_DIR=<katalog>` włącza galerie zrzutów (pozy postaci, areny, efekty, gra), a `TURRIS_SOUND_DIR=<katalog>` przy testach EditMode zapisuje wszystkie dźwięki i utwory jako pliki WAV do odsłuchu.
`TURRIS_TEST_RESULT_DIR=<katalog>` kieruje XML i logi do osobnego folderu, zachowując wyniki innych zadań.

**Boty grające** (`Tests/PlayMode/Bots/`) rozgrywają pełne podejścia na scenie `Main` jako tester QA i gracz H&S/souls-like, po czym piszą raport z wnioskami: `TURRIS_BOT_DIR=<katalog> TURRIS_BOT_RUNS=20 python tools/run_tests.py PlayMode batch Turris.Tests.Bots.WarriorBotPlaytests`. Bez `TURRIS_BOT_DIR` testy są pomijane. Raport dla Wojownika: [docs/raporty_botow/RAPORT_WOJOWNIK.md](docs/raporty_botow/RAPORT_WOJOWNIK.md).

Bezpośrednio przez Unity:

```
Unity.exe -batchmode -projectPath <projekt> -runTests -testPlatform EditMode -testResults wyniki.xml
Unity.exe -batchmode -projectPath <projekt> -runTests -testPlatform PlayMode -testResults wyniki.xml
```

W trybie `-batchmode` Unity nie wywołuje `OnGUI`, więc testy obsługi menu padem (`GamepadPlayTests`) są wtedy oznaczane jako pominięte. Pełny wynik daje Test Runner w edytorze albo to samo polecenie bez `-batchmode` (otworzy się okno edytora).

## Sterowanie

| Akcja | Klawiatura / mysz | Pad |
|---|---|---|
| Ruch (względem kamery) | WASD | lewa gałka |
| Kamera | mysz | prawa gałka |
| Bieg (zużywa wytrzymałość) | Shift | L3 |
| Szybki atak / riposta | LPM | RB |
| Mocny atak | F | RT |
| Blok (trzymaj) | PPM | LB |
| Parowanie | Lewy Ctrl lub boczny przycisk myszy („wstecz”) | LT |
| Unik (przerywa atak i umiejętność) | Spacja | B |
| Umiejętność 1 / 2 / 3 | Q / E / R | A / X / Y |
| Skill klasy (aura: włącz/wyłącz) | G | Select (View) |
| Flaszka życia / many | 1 / 2 | D-pad ↑ / D-pad ↓ |
| Namierzanie | Tab lub środkowy przycisk myszy | R3 |
| Zmiana celu | Z / C lub szybki ruch myszą | D-pad ← / → lub ruch prawej gałki |
| Pauza | Esc | Start |
| Pomoc w HUD | F1 | Select (poza rozgrywką: menu, pauza) |
| Wybór nagrody | 1 / 2 / 3 | D-pad / gałka + A |
| Szybki restart po śmierci | R | Y |
| Zmiana przypisań i głośność | menu główne lub pauza → **Ustawienia** | to samo |

**Pomoc w celowaniu** (jak w Hell Unleashed, domyślnie włączona; przełącznik w **Ustawieniach**): na początku ciosu postać przyciąga się do najbliższego wroga w stożku 28° wokół kierunku ruchu, a bez ruchu – do najbliższego w dowolną stronę (zasięg broni + 1,5 m, min. 5 m). Skille kierunkowe i na punkt zaginają się w stożku 35° (bez ruchu: najbliższy wróg do 12 m); skille „na siebie” nie. Z włączonym **namierzaniem** cios i skill zawsze idą w namierzonego wroga – przez cały zamach.
| (dev) +100 popiołu w menu głównym | F9 | — |

**Przypisania przycisków** można zmienić w menu **Ustawienia** (z menu głównego i z pauzy), osobno dla klawiatury z myszą i dla pada. Wybierz przycisk akcji i naciśnij nowy klawisz (Esc – anuluj); jeśli inna akcja miała ten przycisk, zamienią się miejscami. Układ zapisuje się w profilu, a podpowiedzi w HUD i pomocy pokazują bieżące przypisania.

**Menu i ekrany między piętrami** obsługuje się myszą, padem albo strzałkami:

| Akcja w menu | Pad | Klawiatura |
|---|---|---|
| Przesuń fokus | D-pad lub lewa gałka (przytrzymanie powtarza) | strzałki |
| Zatwierdź | A | Enter |
| Wstecz (odblokowania, przygotowanie, ekwipunek, pauza, zwycięstwo) | B | Backspace |
| Przewiń panel z samym tekstem (odblokowania, statystyki) | prawa gałka | kółko myszy |

Fokus to złota ramka wokół przycisku. Nawigacja jest przestrzenna: fokus przechodzi do najbliższego przycisku w wybranym kierunku, także między kolumnami, a lista przewija się sama do przycisku z fokusem. Gdy używasz myszy, pierwsze naciśnięcie A lub strzałki tylko pokazuje fokus, niczego nie zatwierdza. Po przejściu z gry padem fokus jest widoczny od razu. Podpowiedzi przycisków w HUD i na dole ekranu same zmieniają się na pad albo klawiaturę, zależnie od ostatnio użytego urządzenia. Opisy przycisków (A/B/X/Y, RB, LT itd.) są w konwencji Xboxa; na padzie PlayStation A = ✕, B = ○, X = □, Y = △.

Kolory sygnalizacji ataków przeciwników (przeciwnik świeci podczas zamachu, nad jego głową pojawia się napis):

- **biały** — zwykły atak;
- **pomarańczowy** — ciężki atak, mocno obciąża gardę;
- **fioletowy** — atak nie do sparowania, ale da się go zablokować;
- **czerwony** — atak nie do zablokowania. Trzeba zrobić unik; niektóre z tych ataków da się też sparować, np. pchnięcie bossa;
- **różowy z kręgiem na ziemi** — atak obszarowy, przed którym nie chroni niewrażliwość uniku. Trzeba wyjść z kręgu albo zablokować.

## Bohaterowie (z Hell Unleashed)

Bohaterowie, ich modele, rig, animacje, bronie startowe i startowe skille pochodzą z gry *Hell Unleashed* (`C:\GIT\Hell Unleashed`), której autorem jest autor Turris. Są trzy **typy bohatera** z czterema lub sześcioma **specjalizacjami** albo bez specjalizacji. Każda klasa ma:

- własny model;
- zestawy broni na start;
- **skill klasy** pod osobnym przyciskiem (G / Select);
- **cechę**;
- **styl uniku**;
- **skill startowy**: z broni (grymuar, sztylet do rzucania) albo jeden z dwóch do wyboru.

Typy bohatera zastąpiły dawnego Rycerza, Maga i Łotrzyka i mają te same id, więc profil i odblokowania nadal działają.

| Typ | Specjalizacja | Cecha | Unik | Skill klasy | Broń na start |
|---|---|---|---|---|---|
| **Wojownik** | – | Furia | szarża barkiem | Okrzyk bojowy | miecz + tarcza / miecz dwuręczny |
| | Paladyn | Pobożność (2 aury) | szarża barkiem | Boski osąd | korbacz + tarcza / miecz + tarcza |
| | Berserker | Żądza krwi | szarża barkiem | Nieśmiertelność | topór dwuręczny / młot bojowy |
| | Rębajło | Dwie klingi | przewrót | Taniec ostrzy | dwa miecze / korbacz + miecz |
| **Mag** | – | Arkanowa osłona | odskok widma | Przeciążenie | różdżka + grymuar żywiołu (4 do wyboru) |
| | Mag ognia / wody / powietrza / ziemi | Arkanowa osłona | odskok widma | Meteor / Wir głębin / Nawałnica / Trzęsienie ziemi | różdżka + grymuar żywiołu |
| | Przywoływacz | Więź dusz | odskok widma | Legion (ogary) | różdżka + grymuar przywołań (chochliki) |
| | Mag bitewny | Klinga zaklęć | odskok widma | Cięcie burzy | miecz żywiołu + grymuar |
| **Łotrzyk** | – | Zasadzka | przewrót w cień | Pierścień ostrzy | łuk / sztylet + nóż do rzucania |
| | Łucznik | Sokole oko | przewrót w cień | Burza strzał | łuk |
| | Zabójca | Zasadzka | przewrót w cień | Znak śmierci | sztylet + nóż / dwa sztylety |
| | Łucznik arkanów | Klinga zaklęć | odskok widma | Strzała żywiołów | łuk |

**Cechy klas:**

- **Furia:** ciosy i trafienia ładują pasek; pełny daje szał na 6 s (+35% szybkości ataku, +15% redukcji obrażeń, bez drgnięć).
- **Żądza krwi:** jak Furia, a im mniej życia, tym mocniejsze ciosy (do +60%).
- **Arkanowa osłona:** osłona za 20% maks. życia pochłania ciosy przed życiem i odrasta po 4 s.
- **Zasadzka:** cios w plecy +60%; pierwszy cios do 1,5 s po uniku +35%.
- **Pobożność:** dwie aury naraz, pierwsza ×1,2, druga ×0,75.
- **Dwie klingi:** ciosy na zmianę z prawej i lewej, +20% szybkości ataku.
- **Klinga zaklęć:** moc czarów działa na ciosy bronią, a każde trafienie oddaje manę.
- **Sokole oko:** pocisk do +45% mocniejszy od 11 m.
- **Więź dusz:** +3% redukcji obrażeń za każdego sługę (do 15%).

**Nowe rodzaje umiejętności:**

- aura (włącz/wyłącz);
- sługi (chochliki, ogary): nie mają życia i znikają po czasie;
- fala przenikająca wrogów;
- pocisk przebijający, rozszczepiający się albo wybuchający całymi obrażeniami;
- doskok z wybuchem;
- nova z telegrafem, impulsami i przyciąganiem;
- przeciążenie: następny skill +60%, potem sloty przegrzane;
- nieśmiertelność.

**Nowe efekty:**

- **zachwianie:** trzecia warstwa ogłusza;
- **ogłuszenie**;
- **pęknięcie pancerza:** więcej obrażeń fizycznych.

**Żywioły:** woda z HU to w Turris **mróz**, powietrze to **błyskawica**, a **ziemia** jest nowym żywiołem. Ziemia w płonący cel daje *wybuch żaru*, a w wychłodzony *błoto*, które ogłusza.

Obrażenia z HU są przeliczone ×2,5 (skala Turris). Zasięgi, czasy i odnowienia zostały bez zmian. Pełne dane i decyzje są w [docs/PORT_HELL_UNLEASHED.md](docs/PORT_HELL_UNLEASHED.md).

## Cechy postaci

| Cecha | Działanie |
|---|---|
| **Siła** | wymagania cięższych broni (bez niej broń jest słabsza i wolniejsza), obrażenia broni przez skalowanie |
| **Zręczność** | uchylenie (od 13 pkt, 1,5%/pkt, limit 25% – trafienie chybia, bez efektów), dłuższy unik (+2%/pkt ponad 10, limit +25%), szybkość ataku (+1%/pkt ponad 10, limit +20%), wymagania lekkich broni |
| **Inteligencja** | mana i moc czarów; wymagania czarów są twarde – bez Inteligencji czaru nie da się rzucić |
| **Wytrzymałość** | życie, udźwig i pula wytrzymałości |

Klasa ustala wartości startowe: Siła, Zręczność i Inteligencja jak w Hell Unleashed, a Wytrzymałość to średnia Wytrzymałości i Kondycji z HU (np. Wojownik 14/10/7/12, Mag 7/11/15/10, Łotrzyk 9/15/8/11). **Po każdym piętrze** gracz dostaje 1 punkt i na ekranie między piętrami rozwija wybraną cechę. Stare atrybuty (Witalność, Kondycja, Umysł) zostały scalone – modyfikatory w starych assetach liczą się jako Wytrzymałość lub Inteligencja.

## Umiejętności

Postać ma **szybki atak, mocny atak i trzy sloty umiejętności**, każdy pod własnym przyciskiem (Q/E/R, na padzie A/X/Y).

- **Umiejętność** to czar albo technika bronią:
  - **czar** kosztuje manę i skaluje z Inteligencją (pocisk, fala mocy, leczenie, zaklęte ostrze, kamienna osłona…);
  - **technika** kosztuje wytrzymałość, a jej obrażenia to lekki atak aktualnej broni × mnożnik, więc rośnie razem z bronią (rozpłatanie, uderzenie tarczą, szarża, młynek, trzęsienie).
- Każda umiejętność ma **odnowienie**, niektóre także **ładunki** (np. szarża od poziomu 3 ma dwa). Odnowienie należy do umiejętności, nie do slotu, liczy je zegar gry (pauza je zatrzymuje), a na nowym piętrze ładunki są pełne.
- Część umiejętności wymaga wyposażenia – uderzenie tarczą działa tylko z tarczą. Wymagania atrybutów są miękkie (obniżona skuteczność), jak przy broni.
- **Skille należą do broni, nie do klas** (wzór: Path of Exile 2). Każdy skill wymaga rodzaju broni, a bronie mają wymagania atrybutów, więc dowolna klasa użyje dowolnego skilla, jeśli trzyma pasującą broń. Rodzaje broni (`WeaponFamilies.cs`, jedna tabela dla wszystkich skilli):

  | Rodzaj | Bronie | Przykładowe skille |
  |---|---|---|
  | ostrze | miecze 1h/2h, pałasz, szabla, sztylety | Cięcie wichru, Cięcie krzyżowe, Fala przełamania, Seria cięć |
  | ciężka broń | topory, młoty, buławy, gwiazdy zaranne, korbacze, cepy | Trzęsienie, Tąpnięcie, Rzut toporem, Uderzenie gromu |
  | ostrze **lub** ciężka broń | – | Rozpłatanie, Młynek, Upadek kata, Cięcie z wyskoku |
  | sztylet | sztylety, szabla, sztylet do rzucania | Rzut nożami, Bomba siarkowa, Sidła, Krok cienia |
  | łuk | łuki | Strzał przeszywający, Salwa, Deszcz strzał |
  | fokus | różdżka, kostur, grymuar (także w drugiej ręce) | wszystkie czary |
  | dowolna broń | – | Szarża, Odtrącenie, okrzyki, aury, skille klas |

  Broń może należeć do kilku rodzajów (sztylet: ostrze i sztylet, kosa: ostrze i ciężka broń). Druga ręka też się liczy: miecz z grymuarem rzuca czary, a tarcza pozwala na Uderzenie tarczą.
- **Obrażenia skilla zależą od siły broni**: mnożnik = DPS broni / DPS wzorca rodzaju, przez który broń spełnia wymaganie (ostrze i ciężka broń – miecz jednoręczny, sztylet – sztylet, łuk – łuk myśliwski, czar – podstawowa różdżka żywiołu fokusu), × poziom ulepszenia broni (+10% za poziom). Cięcie wichru na sztylecie bije ×0,7, na mieczu ×1, na mieczu dwuręcznym ×1,25. Startowe bronie klas mają ×1. Techniki Turris biją bronią same (lekki atak × mnożnik).
- Skill startowy do wyboru musi pasować do broni zestawu (niepasujące są wyszarzone); na start puste sloty wypełniają najpierw skille, których broń zestawu pozwala użyć.
- **Unik przerywa umiejętność** tak jak atak. Przerwana przed wyzwoleniem oddaje manę i ładunek (wytrzymałość przepada).

Skąd się biorą umiejętności:

| Źródło | Trwałość |
|---|---|
| Klasa (rycerz: Rozpłatanie, Uderzenie tarczą; mag: Pocisk arkanów, Fala mocy; łotrzyk: Wypad, Rzut nożami) | w każdym podejściu tą klasą |
| Umiejętność obszarowa klasy (rycerz: **Tąpnięcie** – krąg wokół siebie, łamie postawę; mag: **Ognisty krąg** – podpala wszystkich w pobliżu; łotrzyk: **Wachlarz ostrzy** – 10 noży dookoła z krwawieniem) | odblokowana od razu, trzeci slot na start |
| Odblokowanie za popiół (menu „Odblokowania”) | **na stałe** – na start każdego podejścia klasy, dla której jest przeznaczone (albo każdej, jeśli ogólne), bez kosztu w budżecie przygotowania |
| Nagroda między piętrami | **tymczasowo** – tylko w tym podejściu |

**Start podejścia.** Odblokowania skilli i broni są dostępne dla każdej klasy – o użyciu decydują broń i jej wymagania atrybutów (rycerz z różdżką albo grymuarem rzuci kulę ognia). Przypisanie do klasy dotyczy tylko pozostałych dodatków (szaty, rękawice, wzmocnienia). Przypisanie widać w menu „Odblokowania”, a w trakcie podejścia nagrody nie są ograniczone klasą. **Łotrzyk wybiera broń startową** – noże bliźniacze albo łuk – bez kosztu w budżecie.

Nagrody proponują też umiejętności jeszcze nieodblokowane (gdy gracz dotarł na piętro wymagane do ich odblokowania) – można je wypróbować przed zakupem. **Skill zdobyty później przychodzi na wyższym poziomie** (progi Hell Unleashed, `Run/SkillTiers.cs`): od piętra 7 poziom II, od piętra 16 poziom III (skille Turris z pięcioma poziomami: +2 / +4). Skille i bronie HU pojawiają się od piętra równego ich fali (np. Młynek od 16.), progi treści Turris i odblokowań są ×5 względem dawnej wieży 6-piętrowej. Nie są proponowane umiejętności, których obecne wyposażenie nie pozwala użyć.

Sloty ustawia się na ekranie przygotowania (klasowe + odblokowane) i między piętrami w ekwipunku (cała kolekcja podejścia). Ponowne kliknięcie przypisanego przycisku zdejmuje umiejętność ze slotu. Ostatni układ zapisuje się w profilu i obowiązuje w kolejnych podejściach.

### Lista umiejętności

| Czary (mana) | Działanie |
|---|---|
| Pocisk arkanów, Włócznia potępionych, Rozprysk arkanów | pociski magiczne |
| Fala mocy | fala wokół postaci, łamie postawę |
| **Kula ognia** | pocisk wybuchający przy trafieniu (sąsiedzi 60%), podpala |
| **Lodowy podmuch** | stożek mrozu, chłód |
| **Mroźna fala** | fala mrozu wokół postaci, dwie warstwy chłodu |
| **Łańcuch błyskawic** | skacze do 4 wrogów (każdy skok −20%), poraża |
| **Płonąca ziemia** | strefa ognia na namierzonym wrogu, pali co 0,5 s |
| Zaklęte ostrze / **Płomienne ostrze** | broń zadaje dodatkowe obrażenia; płomienne – ogień i szansa podpalenia (także dla technik) |
| Kojące światło, Kamienna osłona | leczenie w czasie; osłona pochłaniająca obrażenia (także tyknięcia efektów) |

| Techniki (wytrzymałość, obrażenia z broni) | Działanie |
|---|---|
| Rozpłatanie, **Krwawe cięcie** | łuk przed sobą; krwawe – dwie warstwy krwawienia |
| Uderzenie tarczą | wymaga tarczy, mocno łamie postawę |
| Szarża, **Wypad** | zryw trafiający wszystko na drodze; wypad krótszy, otwiera ranę |
| Młynek, **Seria cięć** | wirowanie wokół / cztery cięcia przed sobą (szansa krwawienia) |
| Trzęsienie, **Uderzenie gromu** | krąg przed sobą; grom – połowa obrażeń to błyskawica, poraża |
| **Rzut nożami** | trzy noże w wachlarzu, szansa krwawienia – atak z dystansu |

### Nowe w zestawach (plan rozwoju, etapy A–D)

- **Czary:** Iskra (tania błyskawica), Lodowa włócznia (×2 w zamrożonych), Meteor (zapowiedziany krąg, potem płonąca ziemia), Burza (pioruny w losowych wrogów), Krwawy pakt (życie za 3 czary bez many), Mroźna zbroja (osłona + chłód dla napastnika), Łańcuchy potępionych (przyciąga wrogów).
- **Techniki:** Kontra (zatrzymuje cios i oddaje), Cięcie z wyskoku, Egzekucja (+100% w osłabionych), Okrzyk wojenny (+25% obrażeń), Rozdarcie ran (całe krwawienie od razu ×1,5).
- **Poziomy umiejętności dodają cechy** od +2 i +4 (np. Kula ognia +4 zostawia płonącą ziemię, Łańcuch +4 skacze do 6 celów, Młynek +4 kończy się mocnym cięciem). Opis pokazuje cechy odblokowane (✓) i następne.
- **Sklep dusz** między każdym piętrem: ulepszenie umiejętności, losowa nowa umiejętność na to podejście, przerzucenie nagród (raz na piętro). Ceny rosną o 15% za każde piętro.

## Żywioły i efekty

Żywioły: **ogień, mróz, błyskawica**. Obrażenia żywiołów są magiczne (redukuje je obrona magiczna). Sam żywioł niczego nie nakłada – efekt daje tylko źródło, które ma go wpisanego (broń, umiejętność, atak wroga). Efekty nakłada wyłącznie trafienie, które doszło do celu: zablokowane, sparowane, uniknięte albo w pełni pochłonięte przez osłonę nie nakłada nic.

| Efekt | Czas | Warstwy | Działanie |
|---|---|---|---|
| Krwawienie | 3 s | do 5, każda osobno | 35% wylądowanych obrażeń fizycznych na warstwę; ×2, gdy cel się rusza |
| Podpalenie | 2,5 s | 1 (silniejsze wygrywa) | 80% wylądowanych obrażeń ognia |
| Chłód | 3 s | wspólny czas | −15% ruchu i szybkości akcji na warstwę; **trzecia warstwa = zamrożenie** |
| Zamrożenie | 1,2 s | – | brak ruchu i akcji, przerywa atak wroga; potem 2,5 s odporności |
| Porażenie | 4 s | 1 | cel otrzymuje +20% obrażeń |

Reakcje (raz na trafienie, tyknięcia i obrażenia reakcji ich nie wywołują):

- **Szok termiczny** – ogień w wychłodzony cel: +50% części ognia, chłód znika.
- **Przewodzenie** – błyskawica w krwawiący cel: reszta krwawienia zadana od razu, warstwy znikają.
- **Roztrzaskanie** – błyskawica w zamrożony cel: lód pęka, wybuch mrozu (80% obrażeń trafienia) rani innych wrogów w promieniu 3 m.

**Bronie i przedmioty zestawów:** Płonący miecz, Mroźny topór, Młot burzy (część obrażeń to żywioł, ciężki cios nakłada efekt), Ząbkowany miecz (każde cięcie krwawi), Buława (+50% w zamrożonych, rozbija lód), kostury ognia/lodu/burzy (+25% obrażeń żywiołu); Pierścień rozżarzenia (dłuższe podpalenie), Pierścień szronu w żyłach (zamrożenie po 2 warstwach), Amulet przewodnika (przewodzenie przeskakuje), Rękawice rzeźnika (+1 warstwa, ×3 w ruchu), Buty burzy (po uniku cios poraża), Szata popiołu (płonąc zadajesz +25%), Amulet salamandry (nie płoniesz), Hełm z futrem i Uziemiona kolczuga (odporności). Bohater ma **odporności na żywioły** (limit 75%) i premie do obrażeń żywiołów – widoczne w statystykach ekwipunku.

**Noże bliźniacze** (ostrze w każdej dłoni, seria 4 bardzo szybkich cięć, krwawienie, parowanie bez bloku) i **Łuk refleksyjny** (atak szybki i mocny to strzały; z namierzeniem strzały lekko się naprowadzają). Przy strzale postać naciąga cięciwę (żywa cięciwa sięga dłoni, strzała leży na cięciwie) i puszcza ją w chwili wystrzału – mocny strzał naciąga się dłużej. Łotrzyk nosi własny strój: ćwiekowaną skórznię z pasami na noże, karwasze i pelerynę. Za strzałami i rzuconą bronią widać smugę.

**Nowe modele i bronie:** buława, młot, włócznia (najdłuższy zasięg, pchnięcia przebijają linię) i kosa (szeroki łuk, krwawienie, leczy przy trafieniu krwawiących). Technika **Ciężki rzut**: broń leci i wraca, raniąc w obie strony; do jej powrotu walczysz pięściami i nie blokujesz bronią.

**Wieża 30 pięter (fale Hell Unleashed, `Core/HellTower.cs`, dane `Resources/HellCatalog/waves.json` z `tools/art/import_hu_waves.py`):** piętro N = fala N HU. Zwykłe piętro to portale z czasem, budżetem, grupami i składem demonów fali; co trzecie piętro to pojedynek z bossem HU, a po nim kapliczka. Życie, obrażenia, prędkość i dusze demonów rosną osobnymi mnożnikami fali (życie zwykłych fal łagodzone od fali 10 do ×0,75 na fali 29, jak w HU). Korekta Turris: piętro II ma budżet 41 i mniej kultystów (raport botów). Mechanika fal po kolei (`extraWaves`) zostaje dla treści, która jej używa. **Szkieletowy łucznik** trzyma dystans, ucieka przed zwarciem i strzela z łuku (salwy, strzał w nogi spowalnia). Postacie z łukiem noszą kołczan. **Strzały specjalne** (techniki, wymagają łuku): Ognista strzała, Mroźna strzała, Salwa.

**Warianty wrogów** (piętro losuje z szansą 40% zamiast zwykłego wroga): Płonący ghul, Lodowy strażnik, Kultysta burzy – z aurą i poświatą broni w kolorze żywiołu.

Bossowie dostają kontrolę (chłód, zamrożenie) na połowę czasu, elity na 0,7. Tyknięcia są liczone z obrażeń, które już przeszły przez pancerz, więc nic nie liczy się podwójnie; osłona pochłania także tyknięcia.

Broń: topory i sztylet mogą wywołać krwawienie (wielki topór – dwie warstwy przy ciężkim ataku). Wrogowie:

| Wróg | Nakłada | Słaby na | Odporny na |
|---|---|---|---|
| Ghul | krwawienie (pazury, 50%) | ogień | – |
| Heretyk | chłód (pociski mrozu) | błyskawicę | mróz |
| Strażnik | krwawienie (halabarda, 35%) | błyskawicę | krwawienie (½) |
| Kasztelan | podpalenie (fala, salwa), krwawienie | mróz | ogień |

Słabości i odporności widać przy pasku bossa, a trafienie w słabość pokazuje napis „SŁABOŚĆ”. Aktywne efekty są wypisane pod paskiem wroga i pod paskami bohatera; na postaci widać płomienie, szron, iskry i krople krwi. Parametry: `GameConfig → balance → Efekty i żywioły`, mnożniki wrogów: `EnemyDefinition → Żywioły`.

Nowe umiejętności dodaje się w `DefaultContent` (sekcje „UMIEJĘTNOŚCI”), a do istniejących assetów trafiają przez **Turris → Dodaj nową treść (bez nadpisywania)** albo `-executeMethod Turris.EditorTools.TurrisSetup.SyncContentBatch`. Ta operacja tylko dodaje brakujące assety i referencje (pule nagród, odblokowania, umiejętności klas) oraz uzupełnia nowe pola (żywioł, efekty, wybuch, słabości wrogów) tam, gdzie mają jeszcze wartość domyślną; niczego ustawionego ręcznie nie nadpisuje i nie rusza sceny.

## Postacie i animacja

Postacie są humanoidami z pełną animacją ciała, dostępnymi w dwóch wariantach. Kod walki i AI nie wie, który z nich jest używany: udostępnia tylko stan (`CharacterAnimState`: akcja, faza walki, postęp fazy).

### Wariant A: proceduralny humanoid (domyślny, działa bez żadnych plików)

- **Budowa:** szkielet z 17 kości obłożony 240–330 częściami ciała na postać, do tego 20–50 elementów broni i tarczy.
  - Kształty: zwężające się kończyny, szaty w kształcie dzwonu, kopuły hełmów, ostrza z przekrojem rombowym i zbroczem, tarcze wycinane z obrysu, pierścienie, stożki (`ProcMesh`).
  - Materiały PBR: metal i złoto odbijają światło, a kolczuga, tkanina, skóra, drewno i kość są matowe.
  - Części każdej kości są scalane w jedną siatkę (`PartBuilder`), więc postać to ok. 15–22 renderery. Podświetlenia idą przez MaterialPropertyBlock, bez kopiowania materiałów.
  - Detale:
    - twarz: nos, brwi, szczęka, uszy;
    - hełm garnczkowy: szczeliny wizjera, otwory oddechowe, nity, grzebień, czepiec kolczy;
    - zbroja płytowa: napierśnik z grzbietem, obojczyk, fartuch płytowy, taszki, naramienniki z lamami, nałokietniki, nagolenniki, trzewiki;
    - kolczuga: tabard z lamówką i herbem, pendent, pas z klamrą i sakwami, pochwa i sztylet;
    - buty z cholewą, podeszwą i obcasem, dłonie z palcami i kciukiem;
    - szata maga: fałdy, haft, stuła, frędzle, koraliki, fiolki, tuba na zwoje, broda w kapturze;
    - ghul: żebra, kręgi, zęby, pazury.
- **Wygląd gracza** wynika z założonego sprzętu:
  - kolczuga lub zbroja płytowa daje tabard i pelerynę, która kołysze się w ruchu;
  - hełm, kaptur albo kaptur kolczy zależnie od głowy;
  - szata maga zakrywa nogi;
  - miecz, topór, wielki topór, sztylet, kostur albo pięści;
  - tarcza herbowa, puklerz albo pawęż.
- **Wrogowie mają własne sylwetki:**
  - ghul — przygarbiony, z pazurami i świecącymi oczami;
  - heretyk — w szacie i kapturze, z kosturem;
  - strażnik — masywna zbroja, pawęż i halabarda;
  - Kasztelan (boss) — rogaty hełm, peleryna i wielki miecz.
- **Animacje:**
  - postawa bojowa z oddechem, chód i bieg z pracą nóg, ruch bokiem przy namierzaniu;
  - cięcia z prawej i lewej na zmianę w serii, cios z góry, pchnięcie, uderzenie w ziemię, fala magii, czar, pazury, skok;
  - blok tarczą lub bronią, parowanie (tarcza wychodzi w aktywnym oknie), przewrót całego ciała;
  - picie flaszki (flaszka w dłoni), drgnięcie po trafieniu, zachwianie po przełamaniu gardy, klęczenie w oknie riposty, upadek przy śmierci.
- **Technika:** ręce prowadzi analityczne IK dwóch kości (`ProceduralHumanoidAnimator`), a broń i tarcza mają orientację zapisaną w pozach (`PoseLibrary`).
- **Synchronizacja z walką:** każdy atak ma trzy pozy — szczyt zamachu, trafienie, wybrzmienie. Są one próbkowane wprost z postępu faz walki, więc ostrze przecina powietrze dokładnie w fazie aktywnej. Pod koniec zamachu poza na chwilę zamiera, co daje czytelny sygnał, że cios zaraz padnie. Broń wroga świeci kolorem rodzaju ataku.

To stylizowany „manekin”, a nie model z gry AAA.

### Wariant B: model FBX z animacjami (np. Mixamo)

**Bohaterowie z Hell Unleashed korzystają z tego wariantu.** Przygotowanie zasobów:

1. Modele `.glb` z HU konwertuje `bash tools/art/convert_hu_heroes.sh` (Blender 5.2). Konwerter:
   - ustawia T-pose i nazwy kości Mixamo;
   - dodaje kości gniazd `HandSlot.R/L` w środku pięści;
   - zapisuje `<nazwa>.fbx` (model) i `<nazwa>_clips.fbx` (klipy Meshy, z awatarem modelu).
2. Bronie konwertuje `tools/art/convert_hu_weapon.py`.
3. Animacje Mixamo leżą w `Models/Animations/Mixamo/`.
4. W Unity uruchom menu **Turris → Bohaterowie z Hell Unleashed…** albo batch `HeroAssetImport.RunBatch`. Tworzy ono:
   - materiały Standard z tekstur 4K;
   - zestawy animacji broni `Content/Visuals/anim_*` (miecz z tarczą, miecz dwuręczny, łuk, z klatkami faz ciosów z HU);
   - definicje wyglądu `visual_*`.
5. Batch `HeroAssetImport.ApplyContentBatch`:
   - dopisuje treść bohaterów;
   - nadpisuje trzy dawne klasy danymi z HU;
   - przypisuje wygląd klasom i modele 3D przedmiotom (`ItemDefinition.model`).

Zestaw animacji (`WeaponAnimSet`) wybierany jest według broni. Klipy, których zestaw nie ma (czary, śmierć, trafienie), pochodzą z modelu.

Własny model z Mixamo:

1. Pobierz z [mixamo.com](https://www.mixamo.com) (darmowe konto Adobe) postać w formacie **FBX for Unity, With Skin**. Dobierz do niej animacje, każdą jako **FBX, Without Skin, In Place**:
   - ruch: *Sword And Shield Idle*, *Walk*, *Run*, *Strafe Left/Right*, *Walk Back*;
   - ataki: *Sword And Shield Slash* (najlepiej dwa różne), *Attack Downward*, *Stab/Thrust*;
   - obrona i reakcje: *Block Idle*, *Roll*, *Impact*;
   - pozostałe: *Death*, *Drinking*, *Magic Attack*.
2. Wrzuć wszystkie pliki do jednego podfolderu w `Assets/TurrisDamnatorum/Models/`, np. `Models/Knight/`. Importer sam ustawi rig **Humanoid**, zapętli klipy ruchu i „wypiecze” ruch korzenia w pozę. Ruch postaci prowadzi logika gry.
3. Zaznacz ten folder w oknie Project i wybierz **Turris → Utwórz definicję wyglądu z zaznaczonego folderu (FBX)**. Kreator:
   - znajdzie model z siatką;
   - dopasuje klipy po nazwach (idle/walk/run/strafe, cięcia na zmianę prawe i lewe, cios z góry, pchnięcie, blok, przewrót, trafienie, śmierć, picie, czar);
   - utworzy asset `CharacterVisual_…` i wypisze, co przypisał.
4. W assecie sprawdź przy każdym ataku `windupEnd` i `activeEnd`, czyli moment w klipie, gdy ostrze rusza do ciosu i gdy cios się kończy. Gra ustawia czas klipu według faz walki, więc te dwie wartości synchronizują animację z oknami trafienia. Jeśli trzeba, popraw też położenie broni względem dłoni (`weaponPosition`/`weaponRotation`).
5. Przypisz asset w polu **visual** klasy (`Content/Classes/class_knight`) albo przeciwnika (`Content/Enemies/...`). Puste pole oznacza wariant A.

Sterownik klipów (`ClipAnimationDriver`) działa na Playables, bez Animator Controllera:
- miesza ruch według prędkości i kierunku;
- akcje całego ciała odtwarza na osobnej warstwie;
- blok, picie i czar nakłada tylko na górną połowę ciała (maska Humanoid), więc można się przy nich poruszać.

### Efekty czarów i umiejętności

Efekty są zbudowane z systemów cząsteczek Unity (ParticleSystem) z addytywnym świeceniem, kręgów runicznych na ziemi i dynamicznych świateł (`Fx/FxLibrary`). Tekstury (miękka poświata, dym, pierścień, krąg runiczny z heksagramem) są generowane w kodzie. Materiały leżą w `Resources`, więc trafiają do buildu (tworzy je **Turris → Setup**).

- **Czary:**
  - rzucanie: energia zbiera się w dłoni;
  - pocisk: jądro, halo, smuga, krążące iskry i światło, a przy trafieniu wybuch;
  - fala mocy: krąg runiczny, pierścień uderzeniowy, słup iskier i kurz;
  - leczenie: krąg i spiralne drobinki;
  - zaklęte ostrze: poświata broni i drobinki unoszące się z klingi.
- **Walka:**
  - smuga za ostrzem w fazie aktywnej ciosu (u wrogów w kolorze rodzaju ataku);
  - trafienie: krew;
  - blok: iskry;
  - parowanie: złota gwiazda z falą;
  - przełamanie gardy: odłamki;
  - unik: kurz;
  - picie flaszki: drobinki;
  - riposta: rozbłysk;
  - śmierć wroga: ciało rozsypuje się w popiół i żar;
  - druga faza bossa: fala.
- **Telegraf ataku obszarowego:** obracający się krąg runiczny, który pulsuje coraz szybciej, im bliżej ciosu.

### Areny

Areny buduje `ArenaBuilder`: 65–120 tys. wierzchołków scalonych w kilkadziesiąt rendererów. Kolizje zostają proste i niewidoczne (podłoga, pierścień murów, kolumny), więc dekoracje nie wpływają na rozgrywkę ani na kamerę.

- **Dziedziniec:**
  - posadzka z pojedynczych płyt z fugami, pęknięciami i brakującymi płytami;
  - mozaika w centrum;
  - mur z bloków w wiązaniu, z przyporami i blankami;
  - pochodnie z ogniem i migoczącym światłem, sztandary z herbem;
  - brama z łukiem i kratą;
  - kolumny z bazą, żłobkowanym trzonem i głowicą (część złamana, z bębnami na ziemi);
  - posągi klęczących rycerzy, gruz, czaszki i kości.
- **Krypta:** płyty nagrobne, nisze z łukami, sarkofagi z wyrzeźbioną postacią i świecami, łańcuchy, kandelabry, pajęczyny.
- **Szczyt:** niski parapet, koksowniki, obeliski z żarzącymi się runami, kamienni strażnicy, obracający się krąg rytualny, panorama iglic z oświetlonymi oknami i księżyc.

## Dźwięk

Klimat dark fantasy: stal, kość, kamień, żar, dzwony wieży i chór. Wszystko powstaje w kodzie (jak siatki i tekstury), w wątku tła przy starcie gry – nie ma żadnych plików audio.

- **Efekty (58 rodzajów, 1–6 wariantów każdy):**
  - broń i ruch: świst lekki i ciężki (zależny od broni i ataku), cięciwa, trafienie strzały, kroki, unik;
  - trafienia: ciało (lekkie i ciężkie), bohater, blok, parowanie (długo dzwoniące ostrze), przełamanie gardy, chybienie, riposta;
  - magia: narastanie mocy, pociski i wybuchy żywiołów, zamrożenie, roztrzaskanie, pioruny, leczenie, osłona, okrzyk, krwawy pakt, łańcuchy;
  - wrogowie: zapowiedź ciężkiego ciosu (niski gong) i ciosu nie do zablokowania (dysonansowy dzwon z trytonem), warczenie ghula, ryk bossa, osobne dźwięki śmierci (ghul, szkielet, zbroja, boss);
  - przebieg gry: dzwon na wejście na piętro, róg przed kolejną falą, chór po oczyszczeniu piętra, zwycięstwo, śmierć, wchłonięta dusza, bicie serca poniżej 30% życia;
  - interfejs: ruch fokusu, zatwierdzenie, powrót, zakup, nagroda, punkt cechy.
- **Muzyka (pętle bez szwu, D-moll):** menu i kapliczka (organowy pedał, chór, dalekie dzwony), walka (bębny wojenne, niskie smyczki, frygijskie półtony – gra tylko, gdy żyją wrogowie), boss (cięższe bębny, dęciaki, akord z trytonem). Przejścia z przenikaniem.
- **Tła aren:** dziedziniec (wiatr, pochodnie, daleki dzwon), krypta (pomruk grobowca, jęk korytarzy, kapiąca woda, łańcuch), szczyt (porywisty wiatr, świst, dalekie grzmoty). Każda arena ma też własny pogłos (`AudioReverbZone`: korytarz kamienny, jaskinia, góry).
- **Miks:** pula 40 źródeł, limit jednoczesnych odtworzeń i minimalny odstęp dla każdego efektu (36 wrogów nie zagłusza gry), losowy wariant (nigdy ten sam dwa razy z rzędu) i lekkie odchylenie wysokości. Pauza wstrzymuje efekty gry, muzyka i interfejs grają dalej.
- **Ustawienia:** menu → **Ustawienia**: głośność ogólna, muzyki i efektów (krok 10%, zapis w profilu).
- **Podmiana na nagrania:** plik `Resources/Audio/<SoundId>.wav` (albo folder `Resources/Audio/<SoundId>/` z wariantami) i `Resources/Audio/Music/<MusicId>.wav` mają pierwszeństwo przed syntezą. Nazwy – wyliczenia `SoundId` i `MusicId` w `Audio/SoundId.cs`.

## Przyjęte założenia

- **Kamera:** własny `CameraRig` zamiast Cinemachine. Namierzanie, zmiana celu i kolizje kamery wymagały niewielkiej ilości kodu, a unikamy dodatkowego pakietu i konfiguracji. To dopuszczalne, bo zadanie wymagało Cinemachine tylko „jeśli pasuje”.
- **Interfejs:** IMGUI (`OnGUI`), bez prefabów i Canvasów. Prototyp działa od razu po otwarciu sceny. Nawigację padem zapewnia `UINavigator`: każda kontrolka rejestruje swój prostokąt, a akcje z kliknięć i przycisków pada są kolejkowane i wykonywane na końcu przebiegu `OnGUI`, żeby nie rozspójnić układu GUILayout.
- **Areny:** trzy gotowe areny (Dziedziniec, Krypta, Szczyt) zapisane jako dane (`ArenaDefinition`: rozmiar, kształt, kolumny, kolory, styl). `ArenaBuilder` składa je z części według stylu. Układ nie jest losowy: ziarno generatora wynika z id areny, więc arena zawsze wygląda tak samo.
- **Wymagania atrybutów** nigdy nie blokują przedmiotu ani czaru. Jeśli nie są spełnione, skuteczność spada do 60% (parametr `unmetRequirementEffectiveness`). Dzięki temu możliwe są buildy hybrydowe.
- **Czary nie wymagają katalizatora.** Kostur, kaptur i amulety tylko zwiększają moc czarów. Rycerz może rzucać czary z mieczem i tarczą w rękach.
- **Pięści** są bronią zastępczą, gdy główna ręka jest pusta. Każda postać ma więc zawsze słaby atak, który nie zużywa many. Kostur maga również ma atak wręcz bez many.
- **Przełamanie gardy** działa według jawnej reguły, tej samej dla tarczy i broni. Jeśli koszt przyjęcia ciosu (`obciążenie ataku × stabilność gardy`) jest większy niż pozostała wytrzymałość, to:
  - wytrzymałość spada do 0;
  - obrażenia wynoszą `max(obrażenia po bloku, obrażenia bez bloku × 0,5)`;
  - postać jest ogłuszona na 1,1 s.
- **Flaszka:** ładunek zużywa się dopiero w chwili wypicia, po 0,55 s. Trafienie wcześniej przerywa picie bez utraty ładunku. Ryzykiem jest odsłonięcie się podczas animacji. Efekt działa rozłożony w czasie (1,2 s).
- **Życie, mana i flaszki** odnawiają się po każdym piętrze. Na trudności „Zatracony” dzieje się to tylko w kapliczkach, czyli po piętrach II i IV.
- **Waluty:**
  - *dusze* — waluta podejścia, przepada po śmierci, wydaje się je w kapliczce na ulepszenie broni i dodatkowy ładunek flaszki;
  - *popiół* — waluta trwała, przyznawana za ukończenie piętra i zapisywana od razu.
- **Nagrody z pięter** przyznają rosnąco 3 + numer piętra popiołu (finał 50), plus 40 za zwycięstwo, a całość jest mnożona przez trudność. Pierwsze ukończenie danego piętra na danej trudności daje jednorazowo +50%. Wielokrotne kończenie tylko pierwszego piętra daje więc najmniej.

## Zakres prototypu

- **Klasy:** 15 typów/specjalizacji HU z atrybutami, wyposażeniem, skillem klasy i cechami; wygląd wybierany osobno.
- **Przedmioty:** bronie Turris i 33 odpowiedniki definicji HU, dodatkowe warianty wizualne, tarcze, pancerz i akcesoria.
- **Umiejętności:** czary i techniki Turris oraz 29 skilli HU; pełne mapowanie 61 źródłowych skilli w audycie P0.
- **Przeciwnicy:** katalog 34 demonów HU z próbą walki oraz wcześniejsi wrogowie Turris:
  - Ghul — szybki, walczy wręcz;
  - Heretyk — atakuje z dystansu, odpycha i odskakuje;
  - Strażnik Bramy — opancerzony, zasłania się tarczą, ma ciosy z odpornością na przerwanie;
  - Kasztelan Potępionych (boss) — 5 ataków, a poniżej 50% życia druga faza: szybszy, z falą obszarową i salwą pocisków.
- **Piętra:** I–II portale HU, III Władca Bramy, IV–V fale z przeciwnikami Turris i nowymi demonami, VI finałowy boss.
- **Trudność:** 4 poziomy (Pielgrzym, Potępiony, Przeklęty, Zatracony). Wyższe poziomy wprowadzają:
  - elity z dodatkowymi atakami;
  - mniej flaszek;
  - dodatkowe zachowanie bossa;
  - odnawianie tylko w kapliczkach;
  - umiarkowane skalowanie statystyk.
- **Premie za trudność:** mnożnik popiołu (×1 / 1,6 / 2,3 / 3,2), więcej dusz, lepsza jakość przedmiotów. Opisy utrudnień i premii widać na ekranie przygotowania.
- **Odblokowania (za popiół):** przedmioty startowe, czary, talenty i poziomy trudności. Część z nich wymaga najpierw postępu w wieży. Na start dostępne są m.in. Topór bojowy i Pocisk arkanów, więc od razu da się sprawdzić maga z toporem albo rycerza z czarem.
- **Budżet przygotowania:** 4 punkty na dodatki wybierane przed startem. Starcza na dwa elementy, nie na wszystkie.

## Podział systemów

```
Assets/TurrisDamnatorum/
  Scripts/Runtime/
    Data/     ScriptableObject: Item, Spell, Boon, Class, Enemy, Arena, Tower, Difficulty, Unlock, GameConfig (+BalanceConfig)
    Stats/    StatCalculator (statystyki liczone od zera), EquipmentSet, ItemInstance, BuildSnapshot/BuildCalculator
    Combat/   DamageResolver (czysta logika trafień), ActionController + ActionRules (maszyna stanów akcji),
              HitSystem (IHitReceiver, HitTracker – jedno trafienie na zamach), Projectile, efekty wizualne
    Player/   PlayerInputReader (Input System w kodzie), PlayerCombat, PlayerController, CameraRig, PlayerVisuals
    Enemies/  EnemyBrain – jedno AI sterowane danymi (bez podklas), fazy bossa, telegrafy, okno riposty
    Run/      RunState (stan podejścia), RewardGenerator (nagrody ważone buildem, nie klasą)
    Meta/     ProfileData + ProfileSerializer (JSON z wersją formatu i migracją), MetaService, RunFactory
    Core/     GameRoot (przepływ gry), WorldBuilder (postacie), ArenaBuilder (areny w 3 stylach), DefaultContent
    Animation/ CharacterVisual, HumanoidRig + RigLook (proceduralny humanoid), PoseLibrary + ProceduralHumanoidAnimator (pozy, IK, chód),
              CharacterVisualDefinition + ClipAnimationDriver (modele FBX, Playables), GearBuilder (broń i tarcze),
              ProcMesh (siatki proceduralne), PartBuilder + MaterialLibrary + TintSet (scalanie części, materiały, podświetlenia)
    Fx/       FxLibrary (efekty), FxMaterials (tekstury i materiały w kodzie), FxDecal/FxLight/FxFlicker,
              SwingTrail (smuga ostrza), CombatFxDirector (efekty trafień z CombatEvents)
    Audio/    Dsp (synteza: szum, filtry, rezonatory, dzwony, chór, pogłos), SoundSynth (przepisy efektów), MusicSynth (muzyka i tła),
              AudioService + Sfx (bank w tle, pula źródeł, muzyka, pogłos areny), SoundId/MusicId/SoundInfo (lista i miks)
    UI/       GameUI (IMGUI), UINavigator (fokus i nawigacja padem/strzałkami)
  Scripts/Editor/  TurrisSetup (assety, scena, materiały efektów), CharacterModelTools (import FBX, kreator definicji wyglądu),
                   HeroAssetImport (bohaterowie z Hell Unleashed: materiały, zestawy animacji, wyglądy, treść)
  Content/         wygenerowane assety (balans edytowalny w inspektorze)
  Resources/       materiały efektów (dla buildu)
  Models/          Heroes/ (15 bohaterów HU), Demons/ (37 modeli z wariantami), Weapons/ (20 modeli), Animations/Mixamo/
  Tests/EditMode, Tests/PlayMode
tools/run_tests.py  uruchamianie testów z wiersza poleceń
docs/PROGRESS.md    stan prac, historia, plan
```

Kluczowe decyzje architektoniczne:

- **Akcje wynikają z wyposażenia, nie z klasy.** `BuildCalculator` wylicza, co postać potrafi:
  - broń albo pięści;
  - gardę: tarcza ma pierwszeństwo, w przeciwnym razie broń, o ile potrafi blokować;
  - parowanie;
  - unik zależny od obciążenia;
  - przygotowane czary.

  W kodzie walki nie ma żadnego warunku „jeśli mag” ani „jeśli rycerz”.
- **Statystyki nie mogą się kumulować po zmianie sprzętu.** Po każdej zmianie wyposażenia są liczone od nowa ze źródeł: atrybuty bazowe, założone przedmioty i wzmocnienia.
- **Definicja przedmiotu jest oddzielona od egzemplarza.** `ItemDefinition` to niezmienny ScriptableObject, a `ItemInstance` przechowuje poziom jakości i id egzemplarza. Broń dwuręczna jest obsłużona: zajmuje obie ręce i wypiera tarczę, a założenie tarczy wypiera ją.
- **Każdy atak ma niezależne parametry:** czy da się go zablokować, sparować, uniknąć niewrażliwością, typ i wartość obrażeń, obciążenie gardy i obrażenia postawy. Rodzaj sygnalizacji wynika bezpośrednio z tych parametrów, więc sygnał i zachowanie nie mogą się rozjechać.
- **Tabela dozwolonych przejść między akcjami** jest opisana w `ActionController.cs`. Najważniejsze zasady:
  - **unik przerywa atak i czar w każdej fazie**, także w trakcie zamachu; przerwana inkantacja (przed wypuszczeniem czaru) zwraca manę;
  - pozostałe akcje mogą przerwać atak i czar dopiero po punkcie `cancelAfter` w fazie zakończenia;
  - z końcówki uniku (po `dodgeCancelAfter` w fazie zakończenia) można od razu atakować, blokować, parować, rzucać czar lub zrobić kolejny unik;
  - parowania, flaszki, riposty i reakcji na trafienie nie da się przerwać;
  - z bloku nie da się wypić flaszki;
  - bufor wejścia wynosi 0,3 s.
- **Tempo gry** ustawia się w `GameConfig → balance → Tempo`:
  - `gameSpeed` (1,1) – `Time.timeScale` w trakcie rozgrywki, przyspiesza cały świat;
  - `playerActionSpeed` (1,1) i `playerRecoveryScale` (0,8) – szybsze akcje bohatera i krótsze fazy zakończenia (okno parowania się nie skraca);
  - `moveAcceleration`/`moveDeceleration` – płynne ruszanie i hamowanie; unik i wypad narzucają prędkość wprost, a po nich ruch przejmuje pęd;
  - `playerTurnSpeed` (1080°/s).
  - `dodgeRollDuration` (0,6 s) – czas przewrotu i przemieszczenia uniku; `dodgeCancelAfter` warto dobrać tak, by kolejna akcja była możliwa tuż przed końcem przewrotu.
  Animacja proceduralna przenika pozy przez ~0,09 s przy zmianie akcji (np. unik przerywający zamach).

## Wykonane sprawdzenia

Testy uruchamiałem w Unity 6000.6.3f1: EditMode w trybie wsadowym, PlayMode w oknie edytora (`tools/run_tests.py PlayMode window`).

**EditMode: 146/146 zaliczonych** (2026-10-22, w tym `HeroClassTests`; PlayMode w oknie 61/61, w tym `HeroSkillPlayTests` i `HeroModelPlayTests`). Testy sprawdzają kryteria ukończenia na prawdziwym komponencie `PlayerCombat`:

- mag zakłada topór i nim atakuje: ataki pochodzą z topora, zużywają wytrzymałość i można blokować toporem;
- rycerz uczy się czaru i rzuca go, zużywając manę; bez many czar nie zostaje rzucony;
- tarcza zatrzymuje 100% zwykłych obrażeń fizycznych i zużywa wytrzymałość, a obrażenia magiczne redukuje tylko częściowo (osobny parametr);
- blok bronią przepuszcza część obrażeń, ale mniej niż bez bloku;
- parowanie w aktywnym oknie zatrzymuje atak, który da się sparować; atak oznaczony jako nie do sparowania trafia mimo aktywnego okna;
- spóźnione parowanie odsłania gracza, nie zamienia się w blok i nie pozwala na unik w trakcie zakończenia;
- unik chroni tylko w oknie niewrażliwości; ataki obszarowe bez możliwości uniku trafiają zawsze;
- unik przerywa lekki i ciężki atak w połowie zamachu, przerywa czar i zwraca manę; z końcówki uniku można od razu zaatakować;
- wyczerpanie wytrzymałości podczas bloku przełamuje gardę;
- flaszki życia i many działają, a picie da się przerwać;
- regeneracja pochodzi z wyposażenia;
- akcje wzajemnie się wykluczają;
- klasa nie ogranicza dostępnych akcji.

Poza tym testy sprawdzają:

- tabelę przejść między akcjami;
- `HitTracker` (jeden cel trafiony raz na zamach);
- brak kumulowania premii przy zakładaniu i zdejmowaniu przedmiotów;
- zgodność przedmiotów ze slotami;
- broń dwuręczną;
- wpływ obciążenia na unik;
- sumowanie premii procentowych;
- generator nagród: 3 kategorie i brak filtrowania po klasie;
- zapis profilu: odczyt po zapisie, migracja z wersji 1, uszkodzony plik, plik z nowszej wersji;
- meta-postęp: popiół rośnie z piętrem i trudnością, budżet przygotowania, odblokowanie trudności, trwałość odblokowań;
- nawigacja przestrzenna w menu: ruch w kolumnie, między kolumnami, do przycisku na dole ekranu;
- wybór stylu animacji z danych ataku (cięcia na zmianę w serii, ciężki = z góry, pocisk = czar, szarża = pchnięcie, pazury ghula);
- dopasowanie nazw klipów Mixamo do akcji gry (15 przypadków, w tym pułapka „stable” ≠ „stab”).

**PlayMode: 23/23 zaliczonych w edytorze z oknem**, łącznie z galeriami zrzutów (uruchamianymi ze zmienną `TURRIS_SHOT_DIR`; bez niej są pomijane). W trybie wsadowym oba testy pada są pomijane, bo Unity nie wywołuje wtedy `OnGUI`. Testy działają w silniku, z prawdziwą fizyką i AI:

- pełne podejście przez 5 pięter kończy się zwycięstwem, a popiół zostaje odczytany z pliku przez nowy serwis, co symuluje ponowne uruchomienie gry;
- śmierć kończy się ekranem śmierci; szybki restart zaczyna od piętra I z zerową liczbą dusz i bez tymczasowych nagród, a popiół zostaje zachowany;
- wyższa trudność dodaje elity i daje więcej popiołu za ten sam postęp;
- zamach bronią trafia manekin dokładnie raz, mimo że faza aktywna trwa wiele klatek;
- AI Ghula atakuje blokującego rycerza, a tarcza przyjmuje ciosy;
- sparowanie prawdziwego ataku Ghula przełamuje jego postawę i riposta zadaje zwielokrotnione obrażenia;
- boss wchodzi w drugą fazę poniżej 50% życia;
- test dymny sceny `Main.unity` z assetami: podejście magiem, czary zużywają manę;
- opcjonalne zrzuty ekranu z kamery (zmienna środowiskowa `TURRIS_SHOT_DIR`);
- **wyłącznie wirtualnym padem** (urządzenie Input System, bez wywoływania metod gry): menu → przygotowanie (B wraca, nawigacja do „Wejdź do wieży”) → start → RB wykonuje lekki atak → Start pauzuje, B wznawia → A wybiera nagrodę → ekwipunek otwierany A i zamykany B → wejście na kolejne piętro → śmierć → Y restartuje podejście;
- zakup odblokowania padem i powrót przyciskiem B;
- animacja:
  - tor ostrza w cięciu prawym (za prawym barkiem → przed postacią → po lewej) i w ciosie z góry (nad głową → nisko z przodu);
  - IK doprowadza dłoń do celu, bez wartości NaN w kościach;
  - przewrót obniża całe ciało, a po śmierci postać leży na ziemi;
  - `PlayerCombat` steruje animacją zgodnie z fazami, a zmiana broni przebudowuje wygląd;
  - każdy wróg ma właściwą sylwetkę;
  - sterownik klipów FBX ustawia czas klipu dokładnie według faz walki (sprawdzone na klipach wygenerowanych w kodzie, bo w projekcie nie ma modelu Humanoid);
- galeria póz i zrzuty z gry, które obejrzałem i na tej podstawie poprawiłem wygląd (tarcza, szata, peleryna, wypad);
- szczegółowość: liczba części na postać (240–330) przy 15–22 rendererach;
- areny: każda z trzech buduje się bez błędów, ma ponad 50 tys. wierzchołków, poniżej 80 rendererów, a kolizje nie mają rendererów;
- efekty: wszystkie powstają bez błędów i same się sprzątają;
- galerie aren i efektów, obejrzane i poprawione (zaprawa murów, jasność i wielkość efektów, zbyt duże rozbłyski).

Testy i przegląd zrzutów wykryły i pomogły usunąć m.in.:

- premie procentowe do statystyk o wartości bazowej 0, takich jak moc czarów, nie działały;
- przy bardzo wysokim FPS postacie nie mogły się ruszać, bo `CharacterController.minMoveDistance` odrzucał za małe przesunięcia;
- `GameRoot` mógł uznać światło pochodni za słońce i rzucić wyjątek po jego zniszczeniu (teraz wybiera tylko światło kierunkowe);
- kaptur maga zasłaniał twarz, a otwarte części szat były niewidoczne od środka (teraz są dwustronne).

**Czego nie sprawdziłem.** Nie grałem ręcznie ani z klawiatury i myszy, ani z fizycznego pada. Bindingi pada zweryfikowałem wirtualnym urządzeniem Input System, ale nie sprawdzałem: martwych stref i czułości prawej gałki na prawdziwym sprzęcie, zmiany celu ruchem gałki, odczucia sterowania i kamery, czytelności HUD ani balansu trudności. Wymaga to sesji testowej w edytorze.

## Najważniejsze ryzyka i kompromisy / znane braki

- **Demony HU używają istniejących siatek, szkieletów i klipów źródłowych.** Wcześniejsi przeciwnicy Turris pozostają proceduralni.
- **Warstwa póz FBX jest adapterem Unity:** IK chwytów/tarczy, cięciwa, lewa ręka, sztylet/różdżka i reakcja ciała działają;
  chwyt i przenikanie wyposażenia na każdym wyglądzie wymagają dalszej ręcznej oceny. Różnice względem HU opisuje raport P0/P1.
- **Balans jest wstępny.** Wartości ustaliłem na podstawie obliczeń, bez testów z graczami. Wszystkie są w assetach `Content/` i w `GameConfig → balance`.
- **Podejście trwa krócej niż docelowe 20–30 minut.** Prototyp ma sześć pięter.
- **Przeciwnicy nie parują** (`PlayerCombat.OnAttackParried` jest przygotowane). Nie ma też nawigacji NavMesh: areny są płaskie, więc AI porusza się bezpośrednio w stronę gracza i może utknąć za filarem.
- **IMGUI** pozostaje interfejsem prototypu. Nawigacja padem, wibracje i zmiana przypisań w grze działają;
  docelowa oprawa UI jest dalszym etapem migracji.
- **Poziom jakości przedmiotu** skaluje modyfikatory i obrażenia, ale nie parametry gardy.
- **Wydajność nie była mierzona na słabszym sprzęcie.** Na scenie jest kilka–kilkanaście dynamicznych świateł (w krypcie 12), kilkadziesiąt systemów cząsteczek i 65–120 tys. wierzchołków areny. `GameRoot` podnosi `QualitySettings.pixelLightCount` do 8.
- **Dźwięk jest syntezowany w kodzie.** Brzmi spójnie i nie wymaga plików, ale nie dorówna nagraniom foley – docelowe pliki można podłożyć w `Resources/Audio` bez zmian w kodzie.
- **F9 w menu** to narzędzie deweloperskie do testowania odblokowań. Przed wydaniem trzeba je usunąć.
