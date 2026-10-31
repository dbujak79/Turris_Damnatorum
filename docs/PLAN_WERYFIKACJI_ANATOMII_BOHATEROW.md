# Plan weryfikacji nóg, kostek i postawy bohaterów

Data planu: 2026-10-28. **Realizacja audytu: 2026-10-29, po poleceniu „działaj”.**
Wyniki, zakres rzeczywiście zebranych danych, ograniczenia i priorytety znajdują się w
[raporcie weryfikacji](RAPORT_WERYFIKACJI_ANATOMII_BOHATEROW.md).
Sprawdzono 15 aktywnych wyglądów i 27 zestawów startowych, bez napraw ani zmian modeli.
Nierozstrzygnięte pomiary oznaczono w raporcie; zaliczenie przebiegów nie oznacza poprawności wszystkich kostek.

**Dalsza realizacja:** po doprecyzowaniu celu [wdrożono i sprawdzono naprawę 14 wyglądów](NAPRAWA_KOSTEK_BOHATEROW.md).
Wojownik pozostał kontrolą bez zmian; dane audytu sprzed naprawy zachowano.

## 1. Cel i granice

Sprawdzić, czy pozostałe postacie mają problem podobny do wcześniej zgłoszonej lewej kostki
paladyna: stopa obraca się nienaturalnie względem goleni, pięta ucieka na bok lub kolano
prowadzi nogę w innym kierunku niż widoczny but. Zbadać obie nogi, podparcie i wymach,
powiązanie z biodrami oraz ciągłość ruchu.

- **Berserker jest pierwszy** — użytkownik zgłosił, że widzi ten problem także u niego.
  Zgłoszenie traktujemy jako punkt wyjścia; odtworzenie i pomiary pozostają do wykonania.
- Zakres: **14 pozostałych postaci oraz paladyn jako kontrola regresji**, łącznie 15 aktywnych wyglądów.
  Mag również wymaga kontroli kostek: korekta jego postawy nie zastępuje takiego badania.
- Wojownik podlega weryfikacji tylko do odczytu. Nadal obowiązuje wcześniejsze polecenie
  zachowania jego modelu i zachowania; wykryte problemy należy zapisać do osobnej decyzji.
- Hell Unleashed, zachowane źródłowe modele, animacje i materiały pozostają nienaruszone.
- Wynikiem tego zadania będzie raport i propozycje napraw. Plan nie upoważnia do automatycznego
  nakładania korekty paladyna na inne postacie ani do wykonywania napraw podczas audytu.

## 2. Lista postaci i kolejność

Mapowanie poniżej pochodzi z aktualnych assetów klas, z uwzględnieniem pierwszeństwa
`lookOverride → visual → parent.Visual`. Przed przyszłym uruchomieniem należy je odświeżyć,
ponieważ w projekcie mogą pojawić się kolejne podmiany. Nazwy klas i modeli nie są zamienne.

| Kolejność | Postać / ID klasy | Aktualny wygląd | Pierwsze zestawy do sprawdzenia |
|---|---|---|---|
| 1 | Berserker `class_hu_berserker` | `visual_berserker_anime` | Topór dwuręczny i młot; obie nogi w całym biegu |
| 2 | Rębajło `class_hu_dual` | `visual_hero_warrior_dual` | Dwa miecze, korbacz i miecz; naprzemienne ataki |
| 3 | Mag bitewny `class_hu_battlemage` | `visual_hero_mage_battle` | Miecz i grymuar; ruch oraz przejścia cios–czar |
| 4 | Łotrzyk `class_rogue` | `visual_rogue_hooded_meshy` | Łuk oraz sztylety; niska postawa i unik |
| 5 | Łucznik `class_hu_archer` | `visual_hero_rogue_archer` | Łuk; prawa i lewa stopa, szczególnie wymach |
| 6 | Łucznik arkanów `class_hu_arcane_archer` | `visual_hero_rogue_arcane_archer` | Łuk; strzał i czar podczas ruchu |
| 7 | Zabójca `class_hu_assassin` | `visual_hero_rogue_assassin` | Sztylet i noże, dwa sztylety; zwroty i uniki |
| 8 | Mag ognia `class_hu_mage_fire` | `visual_hero_mage_fire` | Fokus/kostur i grymuar; stopy pod szatą |
| 9 | Mag wody `class_hu_mage_water` | `visual_hero_mage_water` | Jak wyżej, własny rig i zestaw klasy |
| 10 | Mag powietrza `class_hu_mage_air` | `visual_hero_mage_air` | Jak wyżej; szybki ruch i zmiany kierunku |
| 11 | Mag ziemi `class_hu_mage_earth` | `visual_hero_mage_earth` | Jak wyżej; cięższa sylwetka i oparcie stóp |
| 12 | Przywoływacz `class_hu_summoner` | `visual_hero_mage_summoner` | Fokus/kostur i czar przywołania |
| 13 | Mag `class_mage` | `visual_mage_anime` | Rzeczywisty zestaw klasy i kostur; także wpływ korekty postawy |
| 14 | Wojownik `class_knight` | `visual_hero_warrior` | Miecz i tarcza oraz broń dwuręczna; kontrola bez poprawek |
| Kontrola | Paladyn `class_hu_paladin` | `visual_paladin_anime` | Zachowanie wcześniejszej poprawki przy mieczu, tarczy i korbaczu |

Powyższa kolejność określa priorytet oglądania, nie oznacza potwierdzenia błędów u pozostałych postaci.
Zachowane stare modele berserkera, maga i paladyna służą jako dodatkowe porównanie, nie zastępują
kontroli aktualnie używanego wariantu anime.

## 3. Dlaczego dotychczasowe zaliczone testy nie rozstrzygają problemu

Przegląd kodu wykazał następujące ograniczenia:

- `HeroModelPlayTests.Run_FeetInStancePointLikeRestPose` obejmuje cztery starsze definicje:
  wojownika, paladyna, dawnego maga i łucznika. Buduje je wszystkie z zestawem miecza.
  Nie bada nowego berserkera z jego rzeczywistym wyglądem i zestawami broni.
- W tym teście łucznik jest wyłączony z warunku asymetrii wymachu; komentarz opisuje
  nietypowy skręt prawej stopy. Ten przypadek wymaga oceny, nie automatycznego zaliczenia.
- `Run_FootMovesEveryFrame_NoHeldPoses` sprawdza płynność lewej stopy wojownika,
  a nie anatomiczną poprawność obu nóg wszystkich modeli.
- `BerserkerAnimePlayTests` kontrolują m.in. uruchomienie, geometrię, animację i chwyty.
  Nie mają osobnego kryterium skrętu kostki w kolejnych fazach kroku.
- Zgodność z pozą spoczynkową nie wystarcza, jeśli sama poza lub mapowanie awatara są błędne.
  Poprawne kości także nie gwarantują poprawnego ustawienia widocznego buta po skinningu.

Podstawa przyszłego audytu to faktyczny `ClassDefinition.Visual` i wygląd wybrany w grze,
nie tylko oryginalny prefab z katalogu `Models/Heroes`.

## 4. Przebieg przyszłej weryfikacji

### Etap A — inwentaryzacja i utrwalenie stanu

1. Zapisać dla każdego wariantu: klasę, aktywny visual, FBX, Avatar, mapowanie kości nóg/stóp/palców,
   pozę referencyjną, skalę oraz rzeczywiście wybrane klipy i zestawy animacji broni.
2. Zapisać aktywne flagi i warstwy korekt: m.in. `rigidBootToes`, `sagittalRunFeet`, `footLength`,
   `balancedMageStance`, korektę kierunku stopy, IK, blokadę podparcia i kroki ataków.
3. Utrwalić SHA256 źródeł i konfigurację pomiaru. Oryginały zachować jako materiał porównawczy.
4. Zweryfikować, czy każdy pomiar ma sens: kość palców istnieje, odcinki mają długość,
   graf i jednostki FBX są ustabilizowane. Brak danych oznaczyć jako brak pomiaru, nie wynik poprawny.

### Etap B — berserker, odtworzenie i wzorcowy raport

1. Uruchomić aktualnego berserkera z toporem, następnie z młotem, na płaskim podłożu kontrolnym.
2. Zarejestrować pełny cykl obu nóg w spoczynku, chodzie i biegu; osobno podparcie, oderwanie,
   przeniesienie nogi, maksymalne zgięcie kolana i ponowne postawienie stopy.
3. Po ustabilizowaniu animacji zebrać **co najmniej trzy pełne cykle**, dane co klatkę przy 60 kl./s.
   Powtórzyć podejrzane fragmenty przy 30 kl./s i tej samej fazie animacji.
4. Obejrzeć przód, tył, oba boki oraz zbliżenia obu kostek w zwolnieniu i zwykłej prędkości.
   Sprawdzić model z siatką i pomocniczym szkieletem oraz bez nakładek.
5. Powtórzyć na rzeczywistej arenie z kamerą gry. Oddzielić problem nogi od różnicy wysokości
   wizualnej posadzki i collidera podłoża.
6. Zapisać pierwszy kompletny przypadek jako wzorzec formatu dla pozostałych postaci.

### Etap C — wspólna macierz dla wszystkich postaci

| Grupa | Obowiązkowe sytuacje |
|---|---|
| Spoczynek | Zwykły i bojowy po ciosie; przejście do ruchu i zatrzymanie |
| Ruch | Chód i bieg przy prędkościach danej postaci; przód, tył, oba boki i skosy |
| Obrót | Zwroty 90°/180°, okrążanie namierzonego celu, zmiana kierunku w trakcie kroku |
| Walka | Pierwszy/drugi cios i cała seria, ciężki atak, gardowanie/parowanie, wyjście z akcji do biegu |
| Inne akcje | Czar, strzał, unik/skok w tył, reakcja na trafienie oraz powrót do spoczynku |
| Wyposażenie | Wszystkie zestawy startowe oraz każdy odmienny profil animacji dostępnej broni |

Klasy mogą obecnie korzystać z różnych rodzajów broni. Po zestawach startowych trzeba objąć
każdy unikalny zestaw klipów i korekt dla danego modelu: broń jednoręczną, dwuręczną, łuk,
fokus/kostur oraz warianty z tarczą i dwiema broniami. Identyczne kombinacje można grupować,
ale nie wolno przenosić wyniku między różnymi rigami tylko dlatego, że używają tego samego klipu.
W raporcie wymienić rzeczywiście sprawdzone kombinacje i pozostałe luki.

### Etap D — pomiary i ocena anatomiczna

- Kierunek pięta–palce względem kierunku ruchu, goleni i płaszczyzny zgięcia kolana.
- Skręt, przechył boczny i pochylenie stopy; ustawienie kolana nad podparciem oraz różnice L/P.
- Porównanie obu nóg w odpowiadających sobie fazach cyklu po odbiciu lustrzanym,
  zamiast zestawiania dwóch różnych faz z tej samej klatki.
- Rozstaw i krzyżowanie stóp, wysokość uniesienia, kontakt pięty/przodostopia, ślizg punktu podparcia
  w przestrzeni świata; uwzględnić naturalne przetaczanie stopy i zamierzony wykrok ataku.
- Widoczny but i stopa po `BakeMesh`, nie tylko osie kości. Osobno zbadać sztywne stalowe noski
  oraz miękkie buty. Szata nie może ukrywać braku oceny stóp — potrzebny dodatkowy widok diagnostyczny.
- Zmiana kąta między klatkami i ciągłość ruchu; biodra/tułów nie powinny kompensować błędu kostki.

Proponowane **progi sygnalizujące potrzebę obejrzenia**, nie uniwersalne normy anatomiczne:

| Sygnał | Wstępny próg do raportowania |
|---|---|
| Odchylenie stopy w podporze od zweryfikowanej pozy referencyjnej | >15° — zgodnie z dotychczasowym testem; nie zastępuje oceny goleni/kolana |
| Różnica skrętu L/P w odpowiadających fazach wymachu | >12° — dotychczasowy próg, bez automatycznego wyjątku dla łucznika |
| Ślizg rzeczywistego punktu podparcia na płaskim podłożu | >2 cm w stabilnym oknie podporu |
| Widoczne zapadanie/unoszenie kontaktującej części podeszwy | >2 cm względem podłoża kontrolnego |

Progi należy odnieść do skali postaci, rodzaju obuwia i animacji przed oznaczeniem błędu.
Przy prawie prostym kolanie płaszczyzna zgięcia jest niestabilna: taki pomiar skrętu wymaga
oznaczenia braku wiarygodności. Należy podawać liczbę próbek, maksimum, P95 i fazę zdarzenia;
sama średnia może ukryć krótkie, wyraźne wykręcenie kostki.

### Etap E — rozdzielenie możliwych przyczyn

Dla problematycznej pary model–klip porównać na tymczasowych kopiach:

1. Model źródłowy używany wcześniej w Turris i jego nagranie referencyjne.
2. Aktywny model i ten sam klip po retargetowaniu, przed dodatkowymi korektami.
3. Kolejne warstwy w rzeczywistej kolejności: palce/sztywny nosek, obrót stóp, postawa nóg,
   skręt nóg, IK, postawa maga, `BodyDynamics`, ustawienie opancerzonych nóg, kroki ciosów i blokada podparcia.
4. Wynik końcowy `CharacterVisual`, wraz z postawą wyposażenia, oraz zdeformowaną siatkę buta.

Rozróżnić: wadę klipu, mapowanie/pozę awatara, osie kości, wzajemne działanie korekt,
nieodpowiednie wagi siatki, ukształtowanie samego buta i problem podłoża/kamery.
Bez zmiany globalnych parametrów stóp i bez kopiowania ustawień paladyna na cały katalog.

## 5. Materiały wynikowe i warunek zakończenia

Planowane po uruchomieniu:

- `docs/RAPORT_WERYFIKACJI_ANATOMII_BOHATEROW.md` — tabela wszystkich postaci i kombinacji,
  lista błędów, ich priorytet, warunki odtworzenia oraz zalecane miejsce ewentualnej naprawy.
- `TestResults/hero-anatomy/<classId>/<visual>/<zestaw>/<ruch>/` — surowe dane CSV/JSON,
  sekwencje klatek lub krótkie nagrania oraz plansze z fazami i nakładką szkieletu.
- Dla każdego zgłoszenia: model, Avatar, klip, prędkość, kierunek, faza i czas, obie nogi,
  ustawienia korekt oraz zrzut z bliska i z kamery gry.
- Statusy per przypadek: **poprawny / podejrzenie / błąd odtworzony / brak wystarczających danych**.
  Podejrzenie lub brak danych nie może zostać przedstawiony jako zaliczona kontrola.

Audyt kończy się dopiero po przejściu całej listy 15 wyglądów, wyszczególnieniu pokrycia
zestawów animacji i obejrzeniu dowodów dla wszystkich podejrzanych przypadków.
Zaliczone testy ogólne są regresją pomocniczą; nie stanowią samodzielnego dowodu poprawności kostek.
Jeśli później zostaną zlecone naprawy, każda powinna mieć własne porównanie przed/po
oraz kontrolę zachowania pozostałych postaci.

## 6. Sposób uruchomienia po zgodzie użytkownika

Preferowane testy i przechwytywanie w już otwartym Unity przez MCP, bez drugiego edytora.
Przed startem potwierdzić właściwy projekt, aktywny port mostu, kompilację i działanie Test Runnera.
Numer portu z poprzedniej sesji nie jest gwarantowany; wcześniejsze obejście błędu
`ExecutionSettings` opisano przy [korekcie postawy maga](../ArtSource/GothicAnimePoC/Mage/posture-v1/README.md).

Jeżeli zostanie wybrany tryb wsadowy, zamknięcie edytora uzgodnić z użytkownikiem zgodnie z `AGENTS.md`.
Wyniki kierować do osobnego katalogu. Nie uruchamiać forced setup, nie zmieniać profilu gracza
i nie wykonywać commita/pusha bez polecenia.

Pierwotne przygotowanie planu nie obejmowało implementacji ani uruchamiania testów.
Po poleceniu użytkownika wykonano audyt, a następnie naprawę: powstały pomiary, sekwencje,
utrwalone plansze dowodowe oraz test regresji obu kostek aktywnych rigów.
