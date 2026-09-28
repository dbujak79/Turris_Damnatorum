# Turris Damnatorum — prototyp

Gra 3D dla jednego gracza, w której walka w stylu souls-like łączy się ze strukturą rogue-lite. Gracz wspina się na wieżę: pięć pięter, na końcu boss. Po śmierci traci wszystko, co zdobył w danym podejściu, ale zachowuje odblokowania i popiół.

- **Silnik:** Unity **6000.6.3f1**, jedyna wersja zainstalowana na tej maszynie. Kod nie używa API specyficznego dla 6.6, więc powinien działać także na Unity 6 LTS (6000.3), ale tej wersji nie sprawdzałem.
- **Pakiety:** `com.unity.inputsystem` 1.20.0 i `com.unity.test-framework` 1.8.0, do tego moduły wbudowane. Nie ma zależności płatnych ani zewnętrznych.
- **Grafika:** wbudowany pipeline renderowania. Postacie i areny to prymitywy (placeholdery).

## Uruchomienie

1. Otwórz folder `C:\GIT\Turris_Damnatorum` w Unity Hub (Add → wskaż folder) w wersji 6000.6.3f1.
2. Otwórz scenę `Assets/TurrisDamnatorum/Scenes/Main.unity` i naciśnij **Play**.
3. Assety treści i scena są już wygenerowane. Gdyby ich brakowało, użyj menu **Turris → Setup (utwórz brakujące assety i scenę)**. Pełne przywrócenie wartości domyślnych daje **Turris → Odtwórz domyślną treść**, ale ta opcja nadpisuje wprowadzone zmiany.
4. Profil gracza zapisuje się w `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Turris Damnatorum\turris_profile.json`. Pełna ścieżka jest też widoczna w menu głównym. Usuwa go opcja **Turris → Usuń profil gracza**.
5. Gdy scena nie ma przypisanego `GameConfig`, gra startuje z treścią domyślną zdefiniowaną w kodzie (`DefaultContent`) i wypisuje ostrzeżenie.

Budowanie pliku wykonywalnego: *File → Build Profiles*. Scena `Main` jest już dodana do listy.

### Testy

W edytorze: *Window → General → Test Runner*, zakładki EditMode i PlayMode. Z wiersza poleceń:

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
| Lekki atak / riposta | LPM | RB |
| Ciężki atak | F | RT |
| Blok (trzymaj) | PPM | LB |
| Parowanie | Q | LT |
| Unik | Spacja | B |
| Rzuć czar / zmień czar | R / X | A / D-pad → |
| Flaszka życia / many | 1 / 2 | X / Y |
| Namierzanie | Tab lub środkowy przycisk myszy | R3 |
| Zmiana celu | Z / C lub szybki ruch myszą | D-pad ← / ruch prawej gałki |
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

## Postacie i animacja

Postacie są humanoidami z pełną animacją ciała, dostępnymi w dwóch wariantach. Kod walki i AI nie wie, który z nich jest używany: udostępnia tylko stan (`CharacterAnimState`: akcja, faza walki, postęp fazy).

### Wariant A: proceduralny humanoid (domyślny, działa bez żadnych plików)

- **Budowa:** szkielet z 17 kości obłożony 100–160 częściami ciała na postać, do tego 20–50 elementów broni i tarczy.
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

## Przyjęte założenia

- **Kamera:** własny `CameraRig` zamiast Cinemachine. Namierzanie, zmiana celu i kolizje kamery wymagały niewielkiej ilości kodu, a unikamy dodatkowego pakietu i konfiguracji. To dopuszczalne, bo zadanie wymagało Cinemachine tylko „jeśli pasuje”.
- **Interfejs:** IMGUI (`OnGUI`), bez prefabów i Canvasów. Prototyp działa od razu po otwarciu sceny. Nawigację padem zapewnia `UINavigator`: każda kontrolka rejestruje swój prostokąt, a akcje z kliknięć i przycisków pada są kolejkowane i wykonywane na końcu przebiegu `OnGUI`, żeby nie rozspójnić układu GUILayout.
- **Areny:** trzy gotowe areny (Dziedziniec, Krypta, Szczyt) zapisane jako dane (`ArenaDefinition`) i budowane z prymitywów. Geometria nie jest generowana proceduralnie.
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
    Core/     GameRoot (przepływ gry), WorldBuilder (areny i postacie z prymitywów), DefaultContent
    Animation/ CharacterVisual, HumanoidRig + RigLook (proceduralny humanoid), PoseLibrary + ProceduralHumanoidAnimator (pozy, IK, chód),
              CharacterVisualDefinition + ClipAnimationDriver (modele FBX, Playables), GearBuilder (broń i tarcze)
    UI/       GameUI (IMGUI), UINavigator (fokus i nawigacja padem/strzałkami)
  Scripts/Editor/  TurrisSetup – generuje assety i scenę
  Content/         wygenerowane assety (balans edytowalny w inspektorze)
  Tests/EditMode, Tests/PlayMode
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
  - parowania, uniku, flaszki, riposty i reakcji na trafienie nie da się przerwać;
  - atak i czar można przerwać dopiero po punkcie `cancelAfter` w fazie zakończenia;
  - z bloku nie da się wypić flaszki;
  - bufor wejścia wynosi 0,25 s.

## Wykonane sprawdzenia

Wszystkie testy uruchomiłem w Unity 6000.6.3f1 w trybie wsadowym.

**EditMode: 61/61 zaliczonych.** Testy sprawdzają kryteria ukończenia na prawdziwym komponencie `PlayerCombat`:

- mag zakłada topór i nim atakuje: ataki pochodzą z topora, zużywają wytrzymałość i można blokować toporem;
- rycerz uczy się czaru i rzuca go, zużywając manę; bez many czar nie zostaje rzucony;
- tarcza zatrzymuje 100% zwykłych obrażeń fizycznych i zużywa wytrzymałość, a obrażenia magiczne redukuje tylko częściowo (osobny parametr);
- blok bronią przepuszcza część obrażeń, ale mniej niż bez bloku;
- parowanie w aktywnym oknie zatrzymuje atak, który da się sparować; atak oznaczony jako nie do sparowania trafia mimo aktywnego okna;
- spóźnione parowanie odsłania gracza, nie zamienia się w blok i nie pozwala na unik w trakcie zakończenia;
- unik chroni tylko w oknie niewrażliwości; ataki obszarowe bez możliwości uniku trafiają zawsze;
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

**PlayMode: 18/18 zaliczonych w edytorze z oknem**, plus dwa testy zrzutów ekranu celowo pominięte (uruchamiają się tylko ze zmienną `TURRIS_SHOT_DIR`). W trybie wsadowym zalicza się 16 testów, a oba testy pada są pomijane, bo Unity nie wywołuje wtedy `OnGUI`. Testy działają w silniku, z prawdziwą fizyką i AI:

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
- galeria póz i zrzuty z gry, które obejrzałem i na tej podstawie poprawiłem wygląd (tarcza, szata, peleryna, wypad).

Testy wykryły i pomogły usunąć dwa błędy:

- premie procentowe do statystyk o wartości bazowej 0, takich jak moc czarów, nie działały;
- przy bardzo wysokim FPS postacie nie mogły się ruszać, bo `CharacterController.minMoveDistance` odrzucał za małe przesunięcia.

**Czego nie sprawdziłem.** Nie grałem ręcznie ani z klawiatury i myszy, ani z fizycznego pada. Bindingi pada zweryfikowałem wirtualnym urządzeniem Input System, ale nie sprawdzałem: martwych stref i czułości prawej gałki na prawdziwym sprzęcie, zmiany celu ruchem gałki, odczucia sterowania i kamery, czytelności HUD ani balansu trudności. Wymaga to sesji testowej w edytorze.

## Najważniejsze ryzyka i kompromisy / znane braki

- **Postacie proceduralne są stylizowane.** To bryły na szkielecie, bez skinningu i bez motion capture. Pełną jakość da dopiero wariant B z modelami FBX.
- **Wariant B nie był uruchamiany na prawdziwym modelu Humanoid.** Mapowanie faz na czas klipu jest przetestowane, ale położenie broni w dłoni, maska górnej połowy ciała i dopasowanie nazw trzeba sprawdzić na pobranych plikach.
- **Balans jest wstępny.** Wartości ustaliłem na podstawie obliczeń, bez testów z graczami. Wszystkie są w assetach `Content/` i w `GameConfig → balance`.
- **Podejście trwa krócej niż docelowe 20–30 minut.** Prototyp ma 5 pięter.
- **Przeciwnicy nie parują** (`PlayerCombat.OnAttackParried` jest przygotowane). Nie ma też nawigacji NavMesh: areny są płaskie, więc AI porusza się bezpośrednio w stronę gracza i może utknąć za filarem.
- **IMGUI** jest wystarczające dla prototypu, ale nie nadaje się do wersji docelowej. Nie ma wibracji pada ani zmiany przypisań przycisków z poziomu gry; przypisania są w `PlayerInputReader.cs`.
- **Poziom jakości przedmiotu** skaluje modyfikatory i obrażenia, ale nie parametry gardy.
- **Brak dźwięku.**
- **F9 w menu** to narzędzie deweloperskie do testowania odblokowań. Przed wydaniem trzeba je usunąć.
