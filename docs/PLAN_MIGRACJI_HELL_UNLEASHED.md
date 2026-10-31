# Plan dalszej migracji Hell Unleashed → Turris Damnatorum

Aktualizacja stanu: **2026-10-23**; inwentaryzacja źródła: 2026-10-22.
Źródło: `C:/GIT/Hell Unleashed` (Godot 4.7.2 .NET).
Cel i jedyne miejsce zapisu: `C:/GIT/Gry/Turris_Damnatorum` (Unity 6000.6.3f1, Built-in Render Pipeline).

**Nie zmieniać, nie przenosić i nie usuwać plików w Hell Unleashed.** Import oznacza kopiowanie
zasobów do Turris i pisanie odpowiedników adapterów silnika. Zastane zmiany w obu repozytoriach należą
do wcześniejszych prac i nie należy ich resetować. Ten plan nie jest zgodą na zmianę zasad rozgrywki Turris.

**Realizacja P0/P1 (2026-10-22): etapy 1–3.** Zakres został ograniczony przez użytkownika do tych etapów.
Wdrożenie, testy i świadome różnice: [RAPORT_MIGRACJI_P0_P1.md](RAPORT_MIGRACJI_P0_P1.md).
Etapu 4 nie rozpoczęto. Modele i animacje skopiowano z istniejących zasobów HU i przekonwertowano do FBX.

**Uzupełnienie z 2026-10-23:** poprawiono ustawienie tylnej stopy w spoczynku i bloku oraz zginanie
stalowych nosków paladyna i wojownika. Zmiany dotyczą wyłącznie adaptera i definicji wyglądu w Turris.
Nie zmieniano modeli, klipów ani żadnych plików Hell Unleashed. Ostatnia kontrola potwierdziła
**2335 niezmienionych plików źródła**; kontrola VFX — **38 identycznych tekstur i zgodne receptury**.

| Zakres | Stan na 2026-10-23 |
|---|---|
| Cząsteczki i ich użycie przez 29 przeniesionych skilli | Wdrożone; dalsze siatki i shadery środowiska w etapie 7 |
| Etap 1 — audyt i zgodność (P0) | Wdrożony; 29/61 skilli, 33/53 broni z jawnym mapowaniem i opisem różnic |
| Etap 2 — animacje, broń i sylwetka (P1) | Wdrożony, wraz z korektami stopy i sztywnego noska; pozostaje ręczna ocena |
| Etap 3 — demony i bossowie (P1) | Wdrożony; 34 definicje źródłowe, 37 modeli z wariantami, bestiariusz i próba walki |
| Etap 4 — pozostałe 32 skille i runtime'y | **Wdrożony 2026-10-23**: 61 skilli z komponentów HU, 19 nowych broni z mocami unikatów – [ETAP4_PIECZECIE.md](ETAP4_PIECZECIE.md) |
| Etap 6 — przedmioty, affixy, łupy, ekonomia | **Wdrożony 2026-10-28, uzupełniony 2026-10-29**: wszystkie 46 perków, łup na ziemi, dojrzewanie dusz, cechy za dusze, 3 oferty skilli, sklep zamiast ekranu nagrody, balans HU 2.0 z 26.10 – szczegóły w PROGRESS.md |
| Reżyser ataków HU (commit 2026-10-26) | **Wdrożony 2026-10-28** – `Enemies/AttackDirector.cs` |
| Etap 5 — pieczęcie | **Wdrożony 2026-10-23**: 186 pieczęci, sakwa, łupy, dar bossa, sklep dusz, kodeks w profilu |
| Etapy 6–8 — ekonomia, biomy, UI/audio/postęp | Plan dalszych prac, bez deklaracji pełnej migracji |

Najbliższe zadanie w obecnym zakresie: ręczna ocena pozy i chwytów, nowych pojedynków oraz balansu IV–V.
Rozszerzenie zakresu o etap 4 wymaga nowego zlecenia użytkownika.

## 1. Jak są napisane obie gry

| Obszar | Hell Unleashed | Turris Damnatorum | Konsekwencja migracji |
|---|---|---|---|
| Reguły | `src/Game.Core/`: zwykły C#, bez Godota; osobne testy xUnit | `Scripts/Runtime/{Combat,Stats,Run,Meta}` we wspólnym assembly Unity | Przenosić algorytmy i testy reguł, przepisywać zależności od typów Unity; nie dublować istniejących systemów |
| Treść | `.tres` → adaptery `Scripts/Content` → rekordy domeny | `ScriptableObject`, `DefaultContent`, assety w `Content/` | Potrzebny jawny import danych i porównywanie wartości; sama kopia `.tres` nie działa |
| Świat | `ArenaController`, `CombatWorld`, aktorzy, komponentowe skille i runtime'y | `GameRoot`, `WorldBuilder`, `EnemyBrain`, `PlayerCombat`, osobne komponenty dostaw | Zachować jednego właściciela obrażeń; VFX mają tylko prezentować zdarzenia |
| Modele | GLB, szkielety Generic i Mixamo, warstwy póz | FBX, `CharacterVisual`, Playables, osobny sterownik demonów | Używać istniejących konwerterów i definicji wyglądu |
| Efekty | `GpuParticles3D`, materiały Godota, pule i proceduralne siatki | `ParticleSystem`, shadery ShaderLab, `HellParticles`, `HellFxPool` | Receptury i tekstury są przenośne; symulacja i materiały wymagają adaptera |
| UI | Godot Control, tekstury HUD/menu/sklepu, podglądy 3D | IMGUI i `UINavigator`, własne ekrany | Przenosić wygląd i funkcje do docelowego UI Unity, zachowując działający pad |
| Zapis | `ProgressProfile`, `GameSettings`, stan domenowy | `PlayerProfile`, `MetaService`, wersjonowany profil Turris | Nie kopiować zapisów Godota wprost ani nie zmieniać istniejących identyfikatorów |

## 2. Co już jest przeniesione

- [x] 15 klas/bohaterów, specjalizacje, modele, startowe zestawy broni, klipy i cechy klas.
- [x] Skill klasy, aury, sługi, fale, doskok, impulsy novy, przebijanie/rozszczep/wybuch pocisków.
- [x] Ziemia, zachwianie, ogłuszenie i pęknięcie pancerza; adaptacja pozostałych statusów.
- [x] Cały katalog 34 demonów HU z modelami i klipami, adapterami zdolności i próbą walki w bestiariuszu;
  wybrane nowe archetypy na piętrach IV–V. Wcześniejsze demony i początkowe portale zachowują dotychczasowy balans.
- [x] Chwyty IK, lewa ręka, cięciwa łuku, brakujące modele broni P1, warstwy sylwetki i osobny wybór wyglądu.
- [x] Korekta spoczynkowego rozwarcia stóp i sztywne stalowe noski paladyna/wojownika;
  test kształtu siatki w spoczynku, biegu i ciosie oraz porównawcze zrzuty.
- [x] Tekstury i warstwy cząsteczek HU oraz integracja ze skopiowanymi umiejętnościami —
  zakres, audyt wywołań i ograniczenia w [MIGRACJA_EFEKTOW_HU.md](MIGRACJA_EFEKTOW_HU.md).

Poprzednie decyzje i przeliczenia: [PORT_HELL_UNLEASHED.md](PORT_HELL_UNLEASHED.md).
Nie przywracać listy „do zrobienia” na podstawie starszych fragmentów historii w `PROGRESS.md`.

## 3. Inwentaryzacja danych źródłowych

Liczby policzone z plików `src/Game/Content/<kategoria>/*.tres`, bez dodatkowych projektów `UI_v2`:

| Kategoria HU | Pliki | Stan w Turris |
|---|---:|---|
| Klasy | 15 | Jest komplet typów i specjalizacji; pozostają różnice animacji i wyglądu |
| Umiejętności | 61 | Są **29 assetów `hu_*.asset`** oraz własne skille Turris; liczba wszystkich skilli Turris nie oznacza pełnej migracji HU |
| Bronie | 53 | 33 jawne mapowania, modele P1 i 11 nowych definicji; pozostałe warianty w dalszym katalogu |
| Przedmioty | 71 | Turris ma własną kolekcję, bez pełnego mapowania HU |
| Affixy | 131 | Brak pełnego odpowiednika generatora i katalogu HU |
| Perki/ulepszenia | 46 | Częściowo podobne talenty i pasywy; potrzebne porównanie po funkcji |
| Pieczęcie | 186 | Brak kompletnego systemu form, wsparć, synergii i więzi |
| Efekty statusowe | 7 | Adaptowane; woda/chłód i reakcje mają świadome różnice |
| Wrogowie | 34 | Komplet mapowań i modeli, katalog oraz próba walki; źródłowe reguły zaadaptowane do Turris |
| Fale | 30 | Turris zachowuje strukturę wieży; nie kopiować 30 fal w miejsce pięter |
| Biomy | 8 | Są 3 proceduralne areny Turris; brak pełnych biomów HU |

Źródło jest nadal rozwijane. Przed każdym etapem ponownie porównać aktualne pliki, a nie traktować
tych liczb jako stałego kontraktu. W nowych importerach zapisywać identyfikator źródła, SHA-256 i mapowanie docelowe.

## 4. Kolejność prac

### Etap 1 — zgodność tego, co już działa (P0)

- [x] Utworzyć tabelę `HU id → Turris id → różnica → test` dla 61 skilli i 53 broni.
  Czytać rzeczywiste `.tres`, `DefaultContent.Heroes.cs` **oraz zapisane assety**, bo mogą się różnić.
- [x] Porównać koszty, czasy, progi poziomów, zasięgi, liczbę impulsów, statusy i strefy po wybuchu.
  Utrzymać dotychczasowe ×2,5 obrażeń tylko tam, gdzie pasuje do skali Turris.
- [x] Przejść wszystkie klasy i zestawy startowe na klawiaturze i padzie, sprawdzić anulowanie rzutów,
  utratę celu, śmierć, pauzę i przejście piętra z aktywnym efektem.
- [x] Sprawdzić VFX i sygnały ataków przy kamerze Turris względem danych HU; wykonać galerie i pomiar większej walki.

Wynik: [audyt 61 skilli/53 broni](AUDYT_HU_P0.md), testy wejścia wszystkich klas, cyklu życia i czasu efektów.
Galeria 30 demonów i metryki są w `TestResults/hu-p1`. Subiektywna ocena czytelności w ręcznym graniu pozostaje wskazana.

**Odbiór:** żaden przeniesiony skill nie używa przypadkowego żywiołu, każda różnica jest opisana,
testy istniejących mechanik pozostają zielone. Nie zaczynać kolejnej dużej porcji treści przed usunięciem regresji.

### Etap 2 — animacje, broń i sylwetka (P1)

Źródło: `HeroPoseLayer`, `PalmUpLayer`, `WeaponVisual`, `SwingStyles`, modele i klipy HU.
Cel: `Animation/ClipAnimationDriver`, `WeaponAnimSet`, `CharacterVisual`, `BodyDynamics`, importer modeli.

- [x] IK drugiej ręki na broni dwuręcznej i ręki z tarczą; dostosowanie do Humanoid i Playables.
- [x] Własne pozy sztyletów/różdżek, lustrzane ciosy drugą ręką, ruch cięciwy łuku.
- [x] Brakujące modele broni: warianty korbaczy i morgensternów dwuręcznych oraz kostury;
  wyeksportować tylko używane zasoby, uwzględnić warianty nowych toporów i młotów z bieżącego HU.
- [x] Oddzielny wybór wyglądu od specjalizacji, dostępny w przygotowaniu runu i zapisany w profilu.
- [x] Warstwy materiałów sylwetki HU (cień, przemoczenie, żar, obrys elity) i ich połączenie
  z obecnym hit-flash/tint; cząsteczki są już osobną warstwą, sam materiał sylwetki to inna praca.
- [x] Korekta tylnej nogi w `Idle_Combat`: około 80° → 40° kierunku stopy, z zachowaniem punktu podparcia.
  `ApplyIdleLegPose` obraca nogę wokół osi biodro–kostka, wygasa z udziałem spoczynku i akcji całego ciała.
  Sprawdzono spoczynek i blok sześciu modeli oraz wcześniejsze regresje biegu i kroku podczas ciosu.
- [x] Sztywny stalowy nosek: `CharacterVisualDefinition.rigidBootToes` włączone u paladyna i wojownika,
  także w `HeroAssetImport`. Lokalna poza palców pozostaje zgodna z modelem; kostka i noga nadal się poruszają.
  Zachowano źródłowe siatki, wagi i klipy. Pozostałe modele zachowują animację palców.

Test `SteelBoots_ForefootMeshKeepsItsShape` porównuje kształt ostatnich 4 cm noska z `BakeMesh`,
po odjęciu przesunięcia całego fragmentu. Próba kontrolna z wyłączoną korektą wykrywa deformację
53,638 mm u paladyna i 33,411 mm u wojownika; po korekcie oba wyniki to około 0,001 mm (próg 3 mm).
Obejrzano [porównanie przed/po](../TestResults/boot/comparison.png) i
[galerię sześciu modeli](../TestResults/boot/after/idle_feet.png). Pomiary: `TestResults/boot/shape.tsv`.

**Odbiór:** galeria pozy/ataku każdą bronią, poprawny chwyt i brak przenikania tarczy/łuku;
zachowana zgodność animacji z momentem trafienia. Brak nadpisania istniejących dopracowanych klipów.

### Etap 3 — pozostałe demony i bossowie (P1)

Źródło: `Content/Enemies`, `EnemyActor`, `EnemyBehaviours`, `BossBehaviour`, `EnemySpecials`,
`BossProfile`, modele/klipy demonów. Cel: `EnemyDefinition`, `EnemyBrain.Specials`, `DemonAnimationDriver`.

- [x] Najpierw przeciwnicy o podobnych regułach do istniejących archetypów (ogar, ciężki wręcz,
  szarżujący), potem nekromanta/przywoływacz, artyleria, skrytobójca i przeciwnicy ze strefami.
- [x] Przenosić bossów osobno: zdolność → zapowiedź → obrażenia → kanał → odpoczynek → przerwanie;
  progi faz, kolejność kombinacji, sługi, bariery i śmierć mistrza.
- [x] Przy każdym nowym demonie odtworzyć doczepienia VFX występujące w źródle na kościach.
- [x] Rozszerzyć bestiariusz i wybrane fale pięter IV–V po testach pojedynczych przeciwników.
  Zachować obecne wagi początkowych portali; nowych bossów udostępnić w próbie walki przed doborem do wieży.

Komplet definicji i pochodzenie: [KATALOG_DEMONOW_HU.md](KATALOG_DEMONOW_HU.md).
Adaptacje reguł i pozostałe ograniczenia są jawnie opisane w raporcie P0/P1.

**Odbiór:** testy obrażeń i przerwania każdej zdolności, brak osieroconych stref/sług,
czytelne kolory ataków blokowalnych/parowalnych i niewrażliwych na unik, galeria oraz próba całego piętra.

### Etap 4 — pozostałe umiejętności i runtime'y (P1/P2)

**Poza obecnym zleceniem — użytkownik zatwierdził tylko etapy 1–3.**

Źródło: `SkillCaster`, `CombatWorld`, `SkillShape`, `SkillRuntimes`, `SealRuntimes`, `HostileRuntimes`, `GearPowers`,
`Content/Skills`. Cel: `SpellDefinition`, dostawy `Projectile/DamageZone/DelayedStrike/WaveEffect/Servant`.

- [ ] Dopisać brakujące pozycje katalogu po audycie etapu 1, bez tworzenia drugiej wersji już działającego skilla.
- [ ] Zaimplementować brakujące zachowania wymagane przez treść: ślady/strefy po drodze,
  miny, orbity, upadek z góry, dodatkowe przywołania i różne kształty dostaw — tylko gdy potwierdzi je katalog.
- [ ] Modele i zachowania przyzwanych chochlików, ogarów i golema; obecne sługi są proceduralnymi widmami.
- [ ] Jawne limity instancji i anulowanie runtime'u przy śmierci/przejściu piętra, spójne z zegarem pauzy.

**Odbiór:** dla każdego nowego skilla test skutku mechanicznego, czasu, zasięgu, poziomów i VFX;
efekt nie może sam tworzyć obrażeń. Turris zachowuje trzy sloty i osobny skill klasy.

### Etap 5 — pieczęcie (P2, po etapie 4)

Źródło: `SkillSealData`, `SkillShape.Compose`, `SkillInstance`, `SealCodex`, `BondRules`, katalog `Content/Seals`,
`docs/skill-seals*.md`. Cel: nowy moduł kompilowania konfiguracji skilla i assety definicji pieczęci.

- [ ] Model formy, wsparcia, synergii i więzi między dwoma skillami; zgodność tagów, gniazda i poziomy.
- [ ] Kolejność operacji: dodanie/usunięcie/zamiana komponentu, zmiana żywiołu, parametry i mnożniki.
- [ ] Kompilacja kopii definicji na potrzeby runu, bez modyfikowania współdzielonych ScriptableObject.
- [ ] Rodzina VFX powinna podążać za **wynikowym** żywiołem/kształtem po konwersji, z możliwością
  zachowania osobnego wyglądu. Rozszerzyć `HellSkillFx`, zamiast rozpoznawać barwę efektu.
- [ ] UI montowania/porównania, generator ofert i zapis wyposażonych pieczęci.

**Odbiór:** przeniesione testy kompilatora i reguł, deterministyczny wynik, brak podwójnego naliczania bonusów;
macierz zgodności i testy wybranych synergii/więzi w walce.

### Etap 6 — przedmioty, affixy, łupy i ekonomia (P2)

Źródło: `AffixData`, `ItemData`, `Run/LootTable`, `Run/SealLoot`, `World/LootDrop`, `ShopGenerator`, `LoadoutAnalysis`, `Content/{Items,Affixes,Upgrades}`.
Cel: `ItemDefinition`, `Equipment`, `BuildCalculator`, `RewardGenerator`, `SoulShop`, UI ekwipunku.

- [ ] Rzadkości, pule i wykluczające się grupy affixów, unikatowe moce, wymagania i poziomy przedmiotów.
- [ ] Plecak/łup na ziemi, porównanie przedmiotów i czytelne wyjaśnienie zmian buildu.
- [ ] Oferty i ulepszenia między piętrami, skalowanie cen i wartość nagród dostosowana do długości wieży.
- [ ] Opcjonalnie model dojrzewania dusz (`SoulAbsorptionSystem`) i kary po trafieniu — to zmiana projektu ekonomii,
  nie konieczny warunek skopiowania efektu wizualnego duszy.

**Odbiór:** deterministyczne losowanie na seedzie, poprawne wykluczenia i limity statystyk,
atomowy zakup, brak duplikacji waluty, zgodne podsumowanie buildu i faktyczne obrażenia.

### Etap 7 — biomy, środowisko i oprawa (P2)

Źródło: `Content/Biomes`, budowa aren, `IceSheet`, `WaveFx`, `BossBehaviour.StoneSpikes`, `EmberPulse`,
`OccluderFader`, tekstury środowiska. Cel: `ArenaDefinition`, `ArenaBuilder`, oświetlenie/kamera Unity.

- [ ] Mapowanie 8 biomów na kolejne piętra/odmiany 3 obecnych aren; najpierw dekoracje i materiały, potem geometria.
- [ ] Woda, lodowe płyty i kolce jako siatki/shadery Unity; nie mylić ich z przeniesionymi cząsteczkami.
- [ ] Pełny shader portalu HU i detale ziemi, jeśli obecna oprawa portalu Turris nie wystarczy.
- [ ] Przenikanie zasłaniających ścian, czytelność pod kamerą zza pleców i zagrożeń przy krawędzi areny.

**Odbiór:** poprawne kolizje/nawigacja, brak blokad portali, brak ukrytych zagrożeń;
porównawcze zrzuty, liczby rendererów, cząsteczek, świateł i koszt klatki na docelowej maszynie.

### Etap 8 — UI, audio i postęp (P2/P3)

- [ ] UI: przenieść grafiki HUD, ikon, sklepu i menu z `Assets/Textures/UI`; ustalić docelową technologię
  Unity przed budową ekranów. Zachować nawigację padem, rebinding, tooltips i aktualne sloty Turris.
- [ ] Cząsteczki 2D sklepu (`UI/ShopMood`) i shader świec dopiero z migracją właściwego ekranu;
  obecny sklep IMGUI nie ma warstwy Godot Control.
- [ ] Audio: skopiować wybrane gotowe klipy HU, przypisać je do istniejącego `AudioService/SoundId`;
  porównać miks i limity z obecną syntezą, nie uruchamiać dwóch usług równolegle.
- [ ] Profil: dopisywać nowe pola z wersjonowaniem i wartościami domyślnymi; nie zmieniać id już
  odblokowanych klas/przedmiotów. Import starych zapisów HU traktować jako osobną funkcję.
- [x] Katalog bestiariusza i próba walki bez zapisu/nagród wdrożone w etapie 3.
- [ ] Pozostałe statystyki i oprawa UI bestiariusza z HU.

**Odbiór:** cała gra obsługiwana padem, skalowanie tekstu i ekranów, odczyt starszego profilu,
zachowanie głośności, brak utraty odblokowań; test przejścia pełnej wieży i samodzielnego buildu Windows.

## 5. Reguły realizacji każdego etapu

1. Czytać aktualne źródło HU; zapisy i eksporty kierować wyłącznie do katalogu Turris.
2. Przejść od reguły i danych do adaptera Unity, potem dodać prezentację i integrację z pętlą gry.
3. Importery mają dodawać brakujące assety lub pokazywać różnice. Nie uruchamiać pełnego resetu treści.
4. Testować reguły w EditMode, zachowanie w PlayMode, wygląd renderem. Przy otwartym Unity używać
   MCP lub Test Runnera; `tools/unity_mcp.py` pozwala wywołać testy i zapisać wynik.
   Zamknięcie edytora jest wymagane tylko przed `tools/run_tests.py`, który uruchamia osobny proces Unity.
   Testy wsadowe pomijają część UI pada.
5. Uaktualnić ten plan i `PROGRESS.md`, zapisać wyniki oraz świadome odstępstwa.

Następny zakres do osobnego zatwierdzenia: **etap 4 — pozostałe 32 skille i wymagane runtime'y**.
Przed rozszerzeniem katalogu warto ocenić w graniu chwyty, nowe pojedynki i balans pięter IV–V.
Pieczęcie i ekonomia wymagają największych nowych systemów.

## 6. Weryfikacja i stan przekazania

| Moment / przebieg | Zaliczone | Błędy | Pominięte |
|---|---:|---:|---:|
| P0/P1, 2026-10-22 — pełne EditMode | 156 | 0 | 1 |
| P0/P1, 2026-10-22 — pełne PlayMode window | 99 | 0 | 15 |
| P0/P1, 2026-10-22 — pełne PlayMode gfx | 97 | 0 | 17 |
| P0/P1, 2026-10-22 — galeria migracji | 5 | 0 | 0 |
| Korekta kierunku stopy, 2026-10-23 — pełne EditMode przez MCP | 156 | 0 | 1 |
| Korekta kierunku stopy, 2026-10-23 — pełne PlayMode przez MCP | 100 | 0 | 16 |
| Po korekcie stalowego noska, 2026-10-23 — celowane PlayMode przez MCP | 6 | 0 | 0 |

Pełne przebiegi z 23 października wykonano **przed** późniejszą korektą stalowego noska.
Po niej sprawdzono kształt noska (z próbą kontrolną), spoczynek/blok sześciu modeli, galerię,
kierunek stóp w biegu, krok przy ciosie i blok górnej połowy ciała.
Wyniki lokalne: `TestResults/foot/EditMode-MCP.json`, `TestResults/foot/PlayMode-MCP.json`,
`TestResults/boot/Targeted-MCP.json`. Ostatni zapis MCP zawiera status ukończenia 6/6 bez błędów;
pełna lista wyników nie przetrwała przeładowania domeny. Pomiary siatki i rendery zapisano osobno.

- [ ] Ręcznie ocenić chwyty, sztywność butów i płynność przejść podczas normalnej gry na wybranych wyglądach.
- [ ] Ocenić czytelność nowych demonów i balans pięter IV–V; automatyczne testy i galerie nie zamykają odbioru balansu.
- [ ] Przed etapem 4 ponownie porównać aktualne źródło i audyt brakujących skilli; nie dublować już działających umiejętności.

## 7. Co jeszcze nie zostało zmigrowane (audyt 2026-10-31)

Audyt kodu obu gier w trzech obszarach. **Celowo** pozostawione różnice są oznaczone; reszta to luki do decyzji autora.
Poprawione 2026-10-31 (szczegóły w [PROGRESS.md](PROGRESS.md)): unikatowa broń z łupu, przemoczenie i spowolnienie jako osobne statusy,
reakcje tylko przy bezpośrednim trafieniu z limitem 1 s na cel.

### 7.1 Przedmioty, bronie, affixy, perki, ekonomia

Zrobione 2026-10-31 (szczegóły w [PROGRESS.md](PROGRESS.md)):
- [x] **Statystyki bez odpowiednika** jako efekty pasywne (`HellEconomy.MapEffect`, wspólne dla przedmiotów, affixów, perków i unikatowych broni):
  uchylenie, celność, przebicie (5), blok bierny, tańszy blok, obszar, zasięg wręcz, odrzut, rozpęd, prędkość/przebicie/rykoszet/podział/pozostałość
  pocisków, warstwy krwawienia, życie, mana i dusze za zabójstwo, odbicie. Lekkie i średnie zbroje mają uchylenie, złoty pierścień – dusze.
- [x] **Wszystkie 131 affixów wypadają** (`ItemInstance.affixEffects`, zapis w `RunSnapshot`).
- [x] **Nowe mechaniki:** mnożnik dusz (`SoulGain`), mana za zabójstwo (`ManaOnKill`), prędkość pocisków broni, czarów i skilli HU (`ProjectileSpeed`).
- [x] **Moce 17 unikatów zbroi i biżuterii** (`HellEconomy.PowerEffect`, te same efekty co u unikatowych broni, `PlayerCombat.Gear`).
- [x] **Sprzedaż** (35% wartości, broń +50% za poziom, tylko na postoju, nie broń w rękach) i **plecak na 8 miejsc**
  (łup przepada, zakup zablokowany przy zajętym slocie, zdjęcie zablokowane).
- [x] **Pola broni HU:** odrzut (efekt `MeleeKnockback`, także pocisk zwykłego ataku), rykoszet różdżki powietrza, zasięg ciosu względem broni wzorcowej
  szablonu (0,8–1,25×). Udział wybuchu kostura (1,0) = pełny wybuch, był już zgodny. Łuk ataku – brak odpowiednika (trafienie wręcz to kula, nie łuk).
- [x] Spójne przeliczenia: unikatowa broń daje uchylenie i przebicie powietrza jako efekty (nie Zręczność i nie premię do błyskawic).
- [x] **Poziom tarczy** jak w HU: oferta kolejnego poziomu w sklepie (3 poziomy, 28 × 1,7^poziom).
- [x] **Jakość** skaluje też moce i efekty (bez liczb sztuk i przełączników, `PassiveEffects.ScalesWithQuality`).
- [x] **`DamageLedger`** – księga obrażeń (zadane wg źródła, przyjęte wg demona, uniki/parowania/bloki) na ekranie śmierci i zwycięstwa.
- [x] **`OfferPreview`** – w sklepie przy przedmiocie widać, co jest założone w jego slocie.
- [x] **Unik demonów i celność:** demony HU unikają zwykłego ciosu i pocisku bohatera (0–18%, `EnemyDefinition.evasion`; nie wybuchów, stref,
  efektów i reakcji; unieruchomiony nie unika), celność = Zręczność ponad próg × 2,25% (HU: 1,5 × uchylenie za punkt) + przedmioty.
  Testy wyłączają unik (`EnemyBrain.DebugNoEvasion`, `NoEvasionSetUp`), serie botów grają z nim.
- [x] **`LoadoutAnalysis`:** w sklepie panel „Twój build” (zwykły atak i skille: na trafienie, tempo, DPS), na karcie broni perki, które po jej
  założeniu zasną, na karcie perka ostrzeżenie, gdy nic nie da (same premie czarów bez czaru w slotach).
- [x] **Boty sprzedają** wszystko, co po przymierzeniu zostało w plecaku: gorsze od założonego i bezużyteczne dla nich (bronie, druga ręka);
  ocena buildu bota liczy też uchylenie, blok bierny, moce unikatów, przebicie, dusze i życie/manę za zabójstwo (`HeroBot.EffectScore`).

Zostaje:
- [ ] `BossTrial` (trening bossa z menu, start z 85% oczekiwanych dusz).
- Celowo: darmowy punkt cechy co piętro, brak ekranu darmowej nagrody, dusze z elity ×1,5 (HU ×8 – do przemyślenia),
  `ProgressProfile` HU – Turris ma własną metaprogresję (popiół, odblokowania, bestiariusz).

### 7.2 Skille, pieczęci, statusy, reakcje

Skille (61) i pieczęci (186) przeniesione w pełni – każdy parametr jest czytany (poza wizualnymi `strike_size`, `blades`).
Zrobione 2026-10-31:
- [x] **Para:** woda (mróz) z ciosu bohatera w płonącego wroga gasi ogień i stawia chmurę 2,4 m na 3 s (`SteamCloud`); demon, który w niej
  zaczyna cios, chybia w 50% („PUDŁO”).
- [x] **Wybuch żaru** w obszarze 1,8 m wokół trafionego wroga (`EnemyBrain.EmberBurst`); u bohatera nadal w niego samego.
- [x] **Drugi żywioł** (`HitData.element2/element2Share`, `AttackDefinition.element2`): każda część trafia we własną odporność i dostaje własną premię;
  reakcje widzą oba żywioły. Skille HU (`HuHit`) i ataki demonów HU (`HellEnemyCatalog.SetDamage`, np. Kowal: ogień + ziemia).
- [x] **Pęknięcie pancerza** u wrogów obniża odporność fizyczną o 4% na warstwę (może zejść poniżej 0), u bohatera – jak dotąd mnożnik.
- [x] **Koszt skilli HU** z parametrów poziomu: czar `mana` albo 8 + 2 × odnowienie, technika `stamina` albo 10 + odnowienie, bez zniżki za poziom.
- [x] **Wartości statusów** porównane z HU: zgodne poza podpaleniem – teraz 2 s (było 2,5 s; łączne obrażenia bez zmian).
- [x] Perk „Mokre ostrze” przemacza (`SoakedOnHit`). Rzut wyzwolony pieczęcią kosztuje 50% – było już zgodne (`HuSkillCaster.TriggerCostShare`).

Zostaje (drobne):
- [ ] Krwawienie rośnie w HU proporcjonalnie do prędkości celu (Turris: ×2 przy ruchu). Błoto w HU tylko na wrogach.
- Celowo: Żelazna skóra zamiast Nieśmiertelności (`hu_undying`), 39 łatek `HuBalance.Patches`, nadpisane formy pieczęci, komponent `Wall`.

### 7.3 Walka gracza, wrogowie, bossowie, areny

Brakuje całkowicie:
- [ ] **Biomy** (`BiomeData`, 8 × `Content/biomes`): erupcje (Ember 5,5 s, Quarry 4,5 s, Sanctum 6,5 s), podłoże (lód – poślizg, bagno),
  mnożniki demonów (np. bagno ×0,78, kości ×1,25). Turris ma tylko style aren.
- [ ] **Poise bohatera** (`Defense/PoiseState`): pasek, przełamanie, chwila odporności. W Turris każde trafienie przerywa akcję
  (poza szałem i pancerzem w ataku).
- [x] **Unik demonów** – zrobione z celnością (7.1).
- [ ] **Warianty żywiołowe** (`EnemyVariants`: 15% od fali 4, 60% ataku w żywiole, odporności ±, HP ×1,25, dusze ×1,6, `AlwaysVariant`).
- [ ] **Specjały wg fali i start odnowień** (`EnemySpecials` `wave=`, `FirstReady` 40–100%, `EliteRecharge` ×0,65): np. skok ogara od piętra 1, sfora atakuje naraz.
- [ ] **Moveset broni** (`Combat/Moveset`): mnożniki ciosów serii, cięższy finisher, ładowany ciężki cios, cios z biegu i po przewrocie.

Uproszczone:
- [ ] Flood (bossowie Tide, Archdemon) – wachlarz fal zamiast ścian wody z luką.
- [ ] Pchnięcie w combo – stożek 120–360° zamiast wąskiej linii.
- [ ] Elity – brak prędkości ×1,1, rozmiaru ×1,4, specjałów od startu i szybszego odnawiania (zamiast odnowień ×0,8 jest agresja ×1,25).
- [ ] Limit ataków specjalnych (1 naraz) nie obejmuje specjałów HU (Volley, Cocoon, Tether, CycloneEye…).
- [ ] Petrify / StoneSkin – kamień przyjmuje 20% każdego ciosu w HU (Turris: obrona fizyczna 0,9, żywioły w pełni); brak kruszenia ogniem
  i wybuchem; leczenie jednym tikiem; odłamki wachlarzem zamiast pierścienia; brak budzenia posągu po 25 s.
- [ ] Tether – nie zrywa się przy uniku, pęka na 1,0× zasięgu (HU 1,3×), tik 0,5 s (HU 0,4 s).
- [ ] Vomit – brak kałuży żółci. CycloneEye – brak wyrzucenia na końcu. StoneSpikes – jedna erupcja zamiast dwóch.
- [ ] WindStep – teleport za gracza zamiast smugi wzdłuż doskoku. Whirlwind – boss chyba nie dryfuje do gracza (niepewne).
- [ ] Latch – tik 1 s (HU 0,5 s), spowolnienie bez dolnej granicy 0,55.
- [ ] FrontGuard (legionista) – garda na staminie zamiast tarczy 45% HP z otwarciem 2,5 s (wybuchy i ziemia ją omijają).
- [ ] Frenzy – Turris dodatkowo przyspiesza zamach ×1,4 (w HU nie).
- [ ] Model obrażeń wrogów – jeden żywioł; ataki mieszane (np. Forgemaster ogień + ziemia) tracą drugi; limit odporności demonów w HU 75%.
- [ ] Drobne: stały czas życia pocisku wroga 6 s (HU `ProjectileLifetimeSeconds`); brak limitu aktywnych wrogów (HU 220, niepewne);
  imp, robak, minion i Gatelord to assety Turris, nie import 1:1.

Szczegóły wdrożenia: [raport P0/P1](RAPORT_MIGRACJI_P0_P1.md).
Historia i pułapki techniczne: [PROGRESS.md](PROGRESS.md).
