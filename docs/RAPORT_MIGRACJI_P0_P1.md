# Migracja Hell Unleashed — P0 i P1, etapy 1–3

Wdrożenie P0/P1: 2026-10-22; aktualizacja po korektach animacji: **2026-10-23**.
Cel: `C:/GIT/Gry/Turris_Damnatorum`, Unity 6000.6.3f1.
Zgodnie z decyzją użytkownika zakres obejmuje **wyłącznie etapy 1–3** planu.
Etap 4, pozostałe skille gracza, pieczęcie i ekonomia pozostają do osobnego zlecenia.

## Pochodzenie i ochrona źródła

Hell Unleashed (`C:/GIT/Hell Unleashed`) służy wyłącznie do odczytu. Wszystkie nowe pliki,
konwersje i raporty zapisano w Turris. Nie usuwano, nie przenoszono ani nie edytowano źródła.
Nie wykonywano pełnego resetu assetów ani sceny Turris.

**Wykorzystano istniejące modele i animacje gry źródłowej.** Blender konwertuje ich GLB do FBX,
zachowując siatki, szkielety i klipy. Nie generowano nowych potworów, konceptów ani zamienników AI.
Usuwanie pomocniczych obiektów przez konwerter dotyczy jego kopii w pamięci Blendera.

- `Resources/HellCatalog/source_manifest.json`: punkt odniesienia SHA-256 dla 2335 plików kodu,
  danych i modeli HU; kolejne uruchomienie importera go nie zastępuje.
- `Models/hu_p1_manifest.json`: powiązania źródłowych GLB i docelowych FBX nowych modeli.
- `Resources/HellVfx/migration_manifest.json`: pochodzenie 38 tekstur VFX i receptur.
- Weryfikacja bez zapisu: `python tools/art/import_hu_catalog.py --verify`
  oraz `python tools/art/import_hu_vfx.py --verify`.

## Etap 1 — audyt i cykl życia efektów

[AUDYT_HU_P0.md](AUDYT_HU_P0.md) zawiera tabelę **61 skilli i 53 broni**: HU id → Turris id →
parametry źródła i assetu → test. `Resources/HellCatalog/catalog.json` zachowuje źródłowe pola,
trzy scalone poziomy każdego skilla, ścieżki i SHA-256. Audyt porównuje zapisane assety z eksportem
fabryki `DefaultContent`, a nie tylko dwa fragmenty kodu.

Mapowanie obejmuje **29/61 skilli i 33/53 broni źródłowych**. Braki są jawne; podobna nazwa skilla
Turris nie jest uznawana za port. Dwa dodatkowe warianty wizualne kostura używają tej samej
źródłowej definicji `weapon.staff`. Nie dopisywano pozostałych 32 skilli gracza z etapu 4.

Świadome różnice:

- Turris zachowuje trzy sloty i osobny skill klasy, skalowanie atrybutów, gardę, postawę, ładunki
  i lokalne krzywe ulepszeń. Poziomy HU 1–3 odpowiadają Turris +0–+2.
- Woda korzysta z mechaniki mrozu, powietrze z błyskawic. `HitData.visualElement` i `HellSkillFx`
  zachowują wodną/powietrzną oprawę niezależnie od obliczeń obrażeń.
- Trzy rozbieżności fabryka/asset są poprawkami geometrii importu: `hu_greatsword.modelScale=1.3`,
  `hu_sword_broad.modelScale=1.05`, `hu_warhammer.modelScale=0.8` i `modelGripShift=0.24`.
  Dokładne pary wartości zapisano w `Resources/HellCatalog/audit.json`.
- Nowe bronie/demony przeliczają odległości /24, bazowe obrażenia i HP ×2,5. Wcześniejsze definicje
  pozostają przy istniejącym balansie Turris. To adaptacja do walki Turris, wymagająca dalszej oceny w graniu.

Poprawiono frakcję fal przeciwnika, zatrzymanie nowych stref/fal/opóźnionych uderzeń podczas pauzy,
sprzątanie efektów właściciela przy śmierci i zmianie piętra oraz gubienie tyknięć stref w długiej klatce.
Strefa 2 s co 0,5 s trafia cztery razy także w pojedynczym kroku 2 s.
Test wejścia przechodzi przez każdą z 15 klas i wszystkie zestawy startowe: G i Select uruchamiają faktyczny rzut.

## Etap 2 — broń, animacje i sylwetka

- Dodano IK drugiej ręki broni dwuręcznej i tarczy po ocenie Playables, z zachowaniem długości kości.
  Gdy potrzeba, dopasowywana jest też ręka główna. Galeria sprawdza błąd chwytu poniżej 8 cm.
- Dodano pozy spoczynkowe sztyletu/różdżki, odbicie mięśni ramion dla ciosu lewą ręką oraz
  ruch naciągu istniejącego rigowanego łuku. Łuk wybiera `BowDraw` przed ogólną animacją pocisku/czaru.
- Skopiowano i przekonwertowano **7 modeli broni**: korbacz i morgenstern dwuręczny, trzy kostury,
  aktualny topór i młot HU. Starsze eksporty i dopracowane klipy pozostają zachowane.
- Dodano **11 definicji przedmiotów**: korbacze, morgensterny, kostury oraz odmiany topora i młota.
  Kostur strzela ogniem, zamiast wykonywać przypadkowy cios wręcz; korbacz zachowuje dwie warstwy zachwiania.
- Przygotowanie runu pozwala wybrać **Wygląd** niezależnie od klasy, atrybutów i zestawu.
  Opcjonalne `lastAppearanceId` w profilu zachowuje zgodność starszych zapisów.
- `HellBody.shader` i `HellBodySurface` łączą cień, przemoczenie, żar i elitę z dotychczasowym tint/hit-flash.
  Materiały modeli są kopiowane w pamięci, bez zmiany źródłowych assetów materiałów.

Galeria `TestResults/hu-p1/p1_weapons_0..4.png` pokazuje spoczynek, koniec zamachu, trafienie i ciężki cios.
Pliki `.txt` podają identyfikatory wierszy. `p1_surfaces.png`: zwykły, cień, mokry, żar, elita z błyskiem trafienia.

### Uzupełnienie animacji — 2026-10-23

- Poprawiono tylną nogę w `Idle_Combat`: kierunek prawej stopy około 80° → 40°, bez przesuwania kostki.
  Korekta obraca nogę wokół osi biodro–kostka; wygasa podczas akcji całego ciała i przejścia do ruchu.
- Usunięto zginanie stalowego noska paladyna i wojownika przez kości palców.
  `rigidBootToes` w definicjach wyglądu i importerze zachowuje lokalną pozę palców, pozostawiając ruch kostki.
  Nie edytowano źródłowych modeli, wag ani klipów. Flaga nie obejmuje pozostałych modeli.
- Pomiar kształtu ostatnich 4 cm noska w spoczynku, biegu i ciosie: paladyn 53,638 → około 0,001 mm,
  wojownik 33,411 → około 0,001 mm. Wartości „przed” uzyskano na kopii definicji bez korekty,
  test odróżnia deformację kształtu od przesunięcia całego fragmentu.
- Obejrzano [porównanie przed/po](../TestResults/boot/comparison.png) i
  [galerię stóp](../TestResults/boot/after/idle_feet.png); pomiary są w `TestResults/boot/shape.tsv`.

Po korekcie kierunku stóp: pełne EditMode przez MCP 156 zaliczonych / 0 błędów / 1 pominięty,
pełne PlayMode 100 / 0 / 16. Po późniejszej korekcie stalowego noska: **6/6 celowanych PlayMode bez błędów**
(kształt siatki z próbą kontrolną, spoczynek/blok, galeria, bieg, krok ciosu, blok górnej połowy ciała).
Raporty: `TestResults/foot/{EditMode-MCP,PlayMode-MCP}.json`, `TestResults/boot/Targeted-MCP.json`.
Nie przedstawiać wcześniejszego pełnego przebiegu jako ponowionego po korekcie noska.

## Etap 3 — demony, bossowie i próba walki

[KATALOG_DEMONOW_HU.md](KATALOG_DEMONOW_HU.md) mapuje **wszystkie 34 źródłowe definicje**,
modele, wysokości, HP, fazy i opisy zdolności wraz z SHA-256.
Do wcześniejszych pięciu definicji dodano 29 nowych i pomocniczą definicję odłamka rozpadlaka.
Skopiowano 29 brakujących modeli; razem z wcześniejszymi wariantami chochlika są 37 modeli demonów.

Adapter `HellEnemyCatalog` + `EnemyBrain.Hell` obsługuje kombinacje, pociski i salwy,
strefy/opóźnione uderzenia, fale, szarże, teleportacje, przywołania, kanały, osłony i progi faz.
Zachowuje kroki `!` dostępne dopiero po rozwścieczeniu, limity tempa czytelnej zapowiedzi oraz
źródłowe reguły blokowania/parowania/uniku dla rodzaju ataku.

Szczególnie sprawdzono:

- Kamienna osłona pochłania obrażenia; rozbicie wywołuje oszołomienie, a wygaśnięcie wybuch.
- Śmierć/przerwanie zatrzymuje kanał i usuwa pociski, strefy, fale i oczekujące uderzenia właściciela.
  Deszcz skał trwa przez zwykły odpoczynek bossa, kończy się przy jego śmierci.
- Nekromanta ma limit żywych sług i łączny budżet 3 × limit; po wyczerpaniu zbliża się i strzela.
  Sługi giną z mistrzem. Wyklute pająki usamodzielniają się; odłamki rozpadlaka mają 40% HP, 70% rozmiaru i nie dzielą się ponownie.
- Krwawy Rytuał wybiera jednego najmocniejszego sojusznika, preferuje walczących wręcz,
  wyklucza bossa/kapłana/jajo/kamienną postać i ogranicza szał do trzech naraz.
  Wzmacnia dopiero po pełnym kanale; bezpośrednie trafienie kapłana, śmierć celu lub zerwanie zasięgu przerywa rytuał.
  Szał daje źródłowe ×1,4 ruchu/obrażeń/tempa i ×0,7 odstępu między atakami, z zachowaniem minimum zapowiedzi Turris.
- Kokon tkaczki blokuje ruch i unik, pozostawia atak, gardę i czary. Ma limit 3 s, odporność 2 s
  po zakończeniu i połowę czasu przy szybkim powtórzeniu. Unik przepuszcza kokon dalej;
  na końcu zasięgu zostaje pajęczyna spowalniająca o połowę, bez obrażeń.
- Doczepienia cząsteczek odtwarzane są na kościach właściwych demonów.

Bestiariusz pokazuje cały katalog i ma **Próbę walki** również dla niewidzianego demona.
Próba nie daje trwałych nagród, nie zmienia profilu ani odkryć; zwycięstwo wraca do bestiariusza.
Na piętrach IV–V wybrane pozycje fal zastąpiono sprawdzonymi archetypami HU.
Zachowano sześć pięter, układ fal i 36 przeciwników w stałych listach; początkowe portale i finał pozostają dotychczasowe.
Pozostałych bossów można sprawdzać pojedynczo przed włączeniem ich do normalnej wieży.

### Granice zgodności

To adapter do istniejącej walki Unity. Nie przenosi całego `CombatWorld` Godota ani jego kamery.
W szczególności:

- Pięć wcześniejszych demonów zachowuje wcześniejsze adaptery i balans, zamiast otrzymać pełny reset.
- Zdarzenia wejścia w fazę są kolejkowane przez maszynę stanów Turris; nie działają równolegle jak dowolne runtime'y Godota.
  Parametry `wave/near/reach` dopasowano do walki w sześciopiętrowej wieży, bez kopiowania 30 fal źródłowych.
- `HitData` ma jeden magiczny żywioł na trafienie. Wielożywiołowe obrażenia HU sumowane są do części
  magicznej z dominującym żywiołem (np. ogień/ziemia Kuźniarza); ich oddzielne odporności wymagają rozszerzenia resolvera.
- Fale/kolce/woda mają dostawy i cząsteczki Unity. Pełne siatki i shadery środowiska źródłowego należą do etapu 7.
- Ręczna ocena chwytów na wszystkich wyglądach, balansu całej wieży i oprawy w ruchu pozostaje potrzebna.
  Galerie oraz automatyczne testy nie zastępują takiej sesji gry.

## Weryfikacja

| Przebieg | Zaliczone | Błędy | Pominięte |
|---|---:|---:|---:|
| Pełne EditMode | 156 | 0 | 1 (opcjonalny eksport WAV) |
| Pełne PlayMode w oknie, w tym menu pada | 99 | 0 | 15 (galerie bez zmiennej środowiskowej) |
| Pełne PlayMode gfx | 97 | 0 | 17 (galerie i dwa testy OnGUI) |
| Mechaniki migracji | 10 | 0 | 0 |
| Końcowa galeria P1 | 5 | 0 | 0 |

Raporty: `TestResults/EditMode.xml`, `PlayMode-hu-p1-full-window.xml`,
`PlayMode-hu-p1-full-gfx.xml`, `PlayMode-hu-p1-native.xml`, `PlayMode-hu-p1-gallery.xml`.
Zestaw migracji przechodzi 134 zdolności i kroki kombinacji. Kontrole SHA potwierdziły 2335 niezmienionych
plików źródła oraz 38 identycznych tekstur i zgodne receptury VFX.

Pierwszy pełny przebieg okienkowy ujawnił wcześniejszą niestabilność testu reakcji ciała:
próbkowanie raz na klatkę mogło pominąć krótki szczyt sprężyny. Test nadal korzysta z rzeczywistego
trafienia, ale odpowiedź ciała mierzy stałym krokiem 1/120 s, bez obniżania progu. Końcowe pełne gfx i window przeszły.

Testy migracji obejmują każdy nowy rodzaj ataku i krok kombinacji: rzeczywiste wypuszczenie,
obrażenia tam, gdzie atak je zadaje, i przerwanie przed wypuszczeniem. Zestawienie zapisuje
`TestResults/hu-native-coverage.tsv`. Osobne testy sprawdzają osłonę, kanał, rytuał, kokon,
budżet sług, śmierć mistrza, rozpad, pauzę, skok czasu, modele i próbę bez zapisu.

Obejrzano wszystkie pięć arkuszy demonów, pięć arkuszy broni, warstwy sylwetki, powłokę kokonu i scenę 30 demonów.
Pomiar diagnostyczny edytora (RTX 4070, D3D12, kamera 1600×900, 120 klatek po rozgrzaniu):
mediana 10,06 ms, p95 13,37 ms; wywołanie `Camera.Render` po stronie CPU 1,06/1,68 ms.
208 systemów cząsteczek, 1800 cząsteczek, 1273 MiB zaalokowane przez Unity.
To wynik edytora z ręcznym renderem każdej próbki, nie pomiar samodzielnego buildu ani osobny czas GPU.

## Powtarzanie importu

1. `python tools/art/import_hu_catalog.py --verify` — sprawdź niezmienność punktu odniesienia HU.
2. `python tools/art/import_hu_models.py` — konwertuj brakujące modele; potrzebny Blender 5.2.
3. W Unity: **Turris → Hell Unleashed → Import P1 (dodaj brakujące)**.
   Import uzupełnia referencje i brakujące assety, aktualizuje własne nowe definicje o niższej wersji migracji;
   nie uruchamia `RunBatchForce`. Zmianę numeru wersji adaptera należy traktować jako świadomą aktualizację jego definicji.
4. `python tools/art/import_hu_catalog.py` — odśwież raport na podstawie eksportu fabryki i zapisanych assetów.
5. Przy otwartym edytorze: testy i rendery przez Unity MCP / Test Runner;
   klient `tools/unity_mcp.py` może zaczekać na zakończenie i zapisać wynik.
   Alternatywnie **po zamknięciu edytora**: `python tools/run_tests.py EditMode`, `python tools/run_tests.py PlayMode window`.
   Galerie: `TURRIS_SHOT_DIR` oraz `PlayMode gfx Turris.Tests.HellMigrationGalleryTests`.

Następne prace opisuje [plan migracji](PLAN_MIGRACJI_HELL_UNLEASHED.md). Etapu 4 nie rozpoczęto.
