# Turris Damnatorum — prototyp

Gra 3D dla jednego gracza, w której walka w stylu souls-like łączy się ze strukturą rogue-lite. Gracz wspina się na wieżę: pięć pięter, na końcu boss. Po śmierci traci wszystko, co zdobył w danym podejściu, ale zachowuje odblokowania i popiół.

- **Silnik:** Unity **6000.6.3f1**, jedyna wersja zainstalowana na tej maszynie. Kod nie używa API specyficznego dla 6.6, więc powinien działać także na Unity 6 LTS (6000.3), ale tej wersji nie sprawdzałem.
- **Pakiety:** `com.unity.inputsystem` 1.20.0 i `com.unity.test-framework` 1.8.0, do tego moduły wbudowane. Nie ma zależności płatnych ani zewnętrznych.
- **Grafika:** wbudowany pipeline renderowania. Wszystko jest generowane w kodzie, bez zewnętrznych assetów:
  - postacie to proceduralne humanoidy z animacją;
  - areny są zbudowane z tysięcy części;
  - efekty to systemy cząsteczek z teksturami tworzonymi w kodzie.

  Opcjonalnie można podpiąć modele FBX (np. z Mixamo).
- **Stan prac i plan na kolejną sesję:** [`docs/PROGRESS.md`](docs/PROGRESS.md).

## Uruchomienie

1. Otwórz folder `C:\GIT\Turris_Damnatorum` w Unity Hub (Add → wskaż folder) w wersji 6000.6.3f1.
2. Otwórz scenę `Assets/TurrisDamnatorum/Scenes/Main.unity` i naciśnij **Play**.
3. Assety treści i scena są już wygenerowane. Gdyby ich brakowało, użyj menu **Turris → Setup (utwórz brakujące assety i scenę)**. Pełne przywrócenie wartości domyślnych daje **Turris → Odtwórz domyślną treść**, ale ta opcja nadpisuje wprowadzone zmiany.
4. Profil gracza zapisuje się w `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Turris Damnatorum\turris_profile.json`. Pełna ścieżka jest też widoczna w menu głównym. Usuwa go opcja **Turris → Usuń profil gracza**.
5. Gdy scena nie ma przypisanego `GameConfig`, gra startuje z treścią domyślną zdefiniowaną w kodzie (`DefaultContent`) i wypisuje ostrzeżenie.

Budowanie pliku wykonywalnego: *File → Build Profiles*. Scena `Main` jest już dodana do listy.

### Testy

W edytorze: *Window → General → Test Runner*, zakładki EditMode i PlayMode.

Z wiersza poleceń najwygodniej uruchomić skrypt z repozytorium. Wymaga Pythona 3, a projekt **nie może być otwarty w edytorze**:

```
python tools/run_tests.py EditMode           # tryb wsadowy, bez grafiki
python tools/run_tests.py PlayMode gfx       # tryb wsadowy z grafiką (zrzuty kamery działają, OnGUI nie)
python tools/run_tests.py PlayMode window    # okno edytora – jedyny tryb, w którym przechodzą testy menu padem
python tools/run_tests.py PlayMode window Turris.Tests.GamepadPlayTests   # filtr
```

Wyniki i log trafiają do `TestResults/`. Zmienna środowiskowa `TURRIS_SHOT_DIR=<katalog>` włącza galerie zrzutów (pozy postaci, areny, efekty, gra).

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
| Flaszka życia / many | 1 / 2 | D-pad ↑ / D-pad ↓ |
| Namierzanie | Tab lub środkowy przycisk myszy | R3 |
| Zmiana celu | Z / C lub szybki ruch myszą | D-pad ← / → lub ruch prawej gałki |
| Pauza | Esc | Start |
| Pomoc w HUD | F1 | Select |
| Wybór nagrody | 1 / 2 / 3 | D-pad / gałka + A |
| Szybki restart po śmierci | R | Y |
| (dev) +100 popiołu w menu głównym | F9 | — |

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

## Umiejętności

Postać ma **szybki atak, mocny atak i trzy sloty umiejętności**, każdy pod własnym przyciskiem (Q/E/R, na padzie A/X/Y).

- **Umiejętność** to czar albo technika bronią:
  - **czar** kosztuje manę i skaluje z Inteligencją (pocisk, fala mocy, leczenie, zaklęte ostrze, kamienna osłona…);
  - **technika** kosztuje wytrzymałość, a jej obrażenia to lekki atak aktualnej broni × mnożnik, więc rośnie razem z bronią (rozpłatanie, uderzenie tarczą, szarża, młynek, trzęsienie).
- Każda umiejętność ma **odnowienie**, niektóre także **ładunki** (np. szarża od poziomu 3 ma dwa). Odnowienie należy do umiejętności, nie do slotu, liczy je zegar gry (pauza je zatrzymuje), a na nowym piętrze ładunki są pełne.
- Część umiejętności wymaga wyposażenia – uderzenie tarczą działa tylko z tarczą. Wymagania atrybutów są miękkie (obniżona skuteczność), jak przy broni.
- **Unik przerywa umiejętność** tak jak atak. Przerwana przed wyzwoleniem oddaje manę i ładunek (wytrzymałość przepada).

Skąd się biorą umiejętności:

| Źródło | Trwałość |
|---|---|
| Klasa (rycerz: Rozpłatanie, Uderzenie tarczą; mag: Pocisk arkanów, Fala mocy) | w każdym podejściu tą klasą |
| Odblokowanie za popiół (menu „Odblokowania”) | **na stałe** – w kolekcji każdego podejścia, bez kosztu w budżecie przygotowania |
| Nagroda między piętrami | **tymczasowo** – tylko w tym podejściu |

Nagrody proponują też umiejętności jeszcze nieodblokowane (gdy gracz dotarł na piętro wymagane do ich odblokowania) – można je wypróbować przed zakupem. Nie są proponowane umiejętności, których obecne wyposażenie nie pozwala użyć.

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
- **Nagrody z pięter** przyznają rosnąco 3/6/10/15/30 popiołu, plus 40 za zwycięstwo, a całość jest mnożona przez trudność. Pierwsze ukończenie danego piętra na danej trudności daje jednorazowo +50%. Wielokrotne kończenie tylko pierwszego piętra daje więc najmniej.

## Zakres prototypu

- **Klasy:** Rycerz (miecz, tarcza herbowa, kolczuga) i Mag (kostur, szata, Pocisk arkanów, Fala mocy). Klasa ustala tylko atrybuty, wyposażenie startowe, czary i liczbę flaszek.
- **Przedmioty:** 6 broni, w tym Wielki topór (dwuręczny) i Sztylet parujący, 3 tarcze oraz pancerz i akcesoria do każdego ze slotów: głowa, korpus, rękawice, pas, buty, 2 pierścienie, amulet.
- **Czary:** pocisk, fala, włócznia, rozprysk, leczenie w czasie, zaklęte ostrze (dla buildów hybrydowych).
- **Przeciwnicy (3 archetypy i boss):**
  - Ghul — szybki, walczy wręcz;
  - Heretyk — atakuje z dystansu, odpycha i odskakuje;
  - Strażnik Bramy — opancerzony, zasłania się tarczą, ma ciosy z odpornością na przerwanie;
  - Kasztelan Potępionych (boss) — 5 ataków, a poniżej 50% życia druga faza: szybszy, z falą obszarową i salwą pocisków.
- **Piętra:** I pojedynek (Ghul), II pojedynek (Heretyk), III grupa (2 Ghule i Heretyk), IV pojedynek (Strażnik), V boss.
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
    UI/       GameUI (IMGUI), UINavigator (fokus i nawigacja padem/strzałkami)
  Scripts/Editor/  TurrisSetup (assety, scena, materiały efektów), CharacterModelTools (import FBX, kreator definicji wyglądu)
  Content/         wygenerowane assety (balans edytowalny w inspektorze)
  Resources/       materiały efektów (dla buildu)
  Models/          miejsce na modele FBX (wariant B), obecnie puste
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

**EditMode: 105/105 zaliczonych.** Testy sprawdzają kryteria ukończenia na prawdziwym komponencie `PlayerCombat`:

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

- **Postacie proceduralne są stylizowane.** To bryły na szkielecie, bez skinningu i bez motion capture. Pełną jakość da dopiero wariant B z modelami FBX.
- **Wariant B nie był uruchamiany na prawdziwym modelu Humanoid.** Mapowanie faz na czas klipu jest przetestowane, ale położenie broni w dłoni, maska górnej połowy ciała i dopasowanie nazw trzeba sprawdzić na pobranych plikach.
- **Balans jest wstępny.** Wartości ustaliłem na podstawie obliczeń, bez testów z graczami. Wszystkie są w assetach `Content/` i w `GameConfig → balance`.
- **Podejście trwa krócej niż docelowe 20–30 minut.** Prototyp ma 5 pięter.
- **Przeciwnicy nie parują** (`PlayerCombat.OnAttackParried` jest przygotowane). Nie ma też nawigacji NavMesh: areny są płaskie, więc AI porusza się bezpośrednio w stronę gracza i może utknąć za filarem.
- **IMGUI** jest wystarczające dla prototypu, ale nie nadaje się do wersji docelowej. Nie ma wibracji pada ani zmiany przypisań przycisków z poziomu gry; przypisania są w `PlayerInputReader.cs`.
- **Poziom jakości przedmiotu** skaluje modyfikatory i obrażenia, ale nie parametry gardy.
- **Wydajność nie była mierzona na słabszym sprzęcie.** Na scenie jest kilka–kilkanaście dynamicznych świateł (w krypcie 12), kilkadziesiąt systemów cząsteczek i 65–120 tys. wierzchołków areny. `GameRoot` podnosi `QualitySettings.pixelLightCount` do 8.
- **Brak dźwięku.**
- **F9 w menu** to narzędzie deweloperskie do testowania odblokowań. Przed wydaniem trzeba je usunąć.
