# Turris Damnatorum — stan prac i plan

### Następna sesja — wdrożenie wojownika gotyckiego anime

- [Konkretny plan integracji](PLAN_WDROZENIA_WOJOWNIKA_ANIME.md): dopasowanie kopii szkieletu, wagi, import Humanoid,
  osobna definicja wyglądu, istniejące animacje/broń/VFX, celowane `class_knight.lookOverride`, testy i zrzuty w Unity.
- **Sprawdzono istniejący rig wojownika z kopii w Turris:** 26 kości, komplet 22 sprawdzonych kości sterownika i gniazda obu dłoni.
  Nadaje się jako baza po dopasowaniu ramion, kolan i stóp; nie ma kości palców dłoni. Raport i nakładki: `ArtSource/GothicAnimePoC/Warrior/rig-*`.
- Na prośbę użytkownika **zwinięto palce obu dłoni w geometrii**. Do dalszej pracy używać `warrior-v2-grip.blend/fbx/glb`
  w [katalogu źródeł](../ArtSource/GothicAnimePoC/Warrior/README.md); oryginalne `warrior-v1.*` zachowane.
  Wersja v2: 1,8 m, stopy na podłożu, 45 832 trójkąty, tekstura 2K, nadal bez szkieletu i wag.
- Oba eksporty v2 ponownie wczytane z teksturą; geometria poza dłońmi bez zmian po normalizacji.
  Obejrzano model oraz zbliżenia obu chwytów, także z próbną rękojeścią 25 mm (walec tylko w podglądach).
- Praca lokalna w Blenderze, **0 kolejnych kredytów Meshy**. Gry ani plików Hell Unleashed nie zmieniano;
  Unity i testy animacji nowego modelu pozostają do wykonania w następnej sesji.

### Ostatnia sesja — 2026-10-23/26: audyt skilli i pieczęci (pomiar na manekinach) i korekty

- **Nowy test pomiarowy** `Tests/PlayMode/HuBenchPlayTests.cs` (port HU `--bench`): każdy skill (poziom III) i forma
  na gromadzie i kolosie, plus 51 wsparć na 10 skillach testowych; jeden rzut, okno 6 s, timeScale 2, manekiny 1e7 HP,
  obrażenia liczone z `CombatEvents.Impact` (trafienia, sługi, strefy, tyknięcia). Uruchamia się tylko, gdy istnieje
  `TestResults/hu-bench` albo `TURRIS_BENCH_DIR`; zapis `bench.csv` (kolumny: rodzaj, skill, pieczęć, cele, obrażenia,
  odnowienie, ładunki, na sekundę cyklu, skala, sprzęt, zasób, koszt, koszt życia [ułamek maks. HP], żywioł, komponenty).
  Bohater: mag (techniki fizyczne liczone bronią maga – porównuj w obrębie rodzaju). Czas: ~10–15 min.
- **Błąd gry znaleziony pomiarem i naprawiony:** `HitQuery.Deliver` używał jednej listy celów, a trafienie (impuls aury
  → efekt „przy trafieniu”) wywoływało zagnieżdżone zapytanie → `InvalidOperationException`. Teraz lista na poziom
  zagnieżdżenia (`Combat/HitSystem.cs`); test regresji `HitQueryTests.NestedQuery_InsideOnHit_KeepsOuterTargets` (EditMode, zaliczony).
- **Przebieg 1** (`TestResults/hu-bench/bench_manekiny_zywiolowe.csv`, 642 wiersze) jest **przekłamany żywiołami**:
  gromada to w 2/3 chochliki (ogień ×0,25, mróz ×1,6), kolos – osiłek (25% pancerza, ziemia ×0,7, błyskawice ×1,3).
  Stąd np. konwersja na ogień ×0,49, na wodę ×1,72. Wstępne obserwacje (do potwierdzenia przebiegiem 2):
  Barrage/Nova/Wave/Rain dominują na gromadzie (Łza lodowca 457/s vs mediana 78/s; na kolosie 40/s); formy: tornado
  wiru ×3,9, sopel ×3,7, topór-deszcz ×3,0, mrugnięcie ×2,7; martwe: kuźnia pierścienia ×0,02, odpychające oko cyklonu
  ×0,16, rój kul ×0,27; wsparcia: Brama piekieł ×2,6, Serce otchłani ×2,0, Wachlarz ×1,8; wyzwalacze ×0,7–0,8 (pomiar
  nie wywołuje uników/zabójstw – nie oceniać), Ofiara krwi pokazuje koszt many 1,0 (kolumna kosztu nie odejmuje zamiany na życie).
- **Przebieg 2 — GOTOWY** (2026-10-26, neutralne manekiny, `TestResults/hu-bench/bench.csv`, 642 wiersze; wyciszony –
  `Tests/PlayMode/MuteDuringTests.cs` wycisza edytor na czas testów PlayMode, `TURRIS_TEST_SOUND=1` włącza dźwięk).
  EditMode po poprawce `HitSystem`: **184/0/1**. Analiza: `python tools/balance/bench_report.py TestResults/hu-bench/bench.csv`.
- **Wstępne wnioski (pomiar, nie granie):**
  - Mediana DPS cyklu: gromada 96/s, kolos 27/s.
  - **Dominacja: Aura świętego ognia** – 300/s gromada, 60/s kolos (2× mediany), bez kosztu rzutu (tylko rezerwacja 20%),
    stale; forma **Sztandar** 615/90 – najlepsza w obu kontekstach. Główny kandydat do korekty.
  - Najlepsze obszarowe: Deszcz strzał 359/51 (dobry też na kolosie – prawie bez słabości), Łza lodowca 320/40,
    Fala przyboju 283/43. Najlepsze na pojedynczy cel: chochliki 95/s (sługi mimo skrócenia), salwa 79, przebijający strzał 64.
  - Słabe (DPS; część to skille użytkowe): Rozpłatanie 19/s (wynik zależny od ustawienia – między przebiegami ×3,
    niewiarygodny), Krzyżowe cięcie 23, Wachlarz ostrzy 19, Legion 24 (CD 28 s), Znak śmierci 28, Rzut toporem 42.
  - Formy silne w obu kontekstach (≥1,6×): Kula błyskawic (łańcuch), Wyrok pojedynczy, Napalm i Lepka bomba, Bumerang
    cięcia wiatru, Huragan. Martwe w obu (≤0,7×): Kuźnia pierścienia ×0,02 (prawdopodobnie użytkowa – działa przez ciosy
    bronią, pomiar ich nie robi), Odpychające oko cyklonu, Smoła sideł, Gwiazda kolców, Mina siarkowa.
  - Wsparcia (mediana): Brama piekieł ×2,6, Korona Welesa ×2,3, Serce otchłani ×2,0, Wachlarz ×1,9, Kaskada ×1,7;
    wyzwalacze ×0,7–0,8 i Totem/Orbita/Wieczny płomień <1 – tego pomiaru nie da się ocenić (brak uników/zabójstw, ruchu).
  - Odporności wrogów: na chochlikach skille ognia tracą ~57% (×0,43), wody zyskują ×1,45 – to celowe (chochliki
    mają warianty żywiołów), ale warto sprawdzić, czy mag ognia nie jest karany na piętrach z ognistymi chochlikami.
  - Ograniczenia: bohater testowy to mag (techniki fizyczne liczone jego bronią – zaniżone), okno 6 s, manekiny stoją.
- **Raport:** [AUDYT_SKILLI_PIECZECI.md](AUDYT_SKILLI_PIECZECI.md). **Korekty wdrożone** (2026-10-26, `Seals/HuBalance.cs`,
  nowe `SealPatches` – korekty pieczęci z opisem): Aura świętego ognia 24 → 14 (kolos 60 → 35/s), Sztandar ×1,25; sługi
  ×0,4/×0,45/×0,6 (pomiar z oknem = odnowienie potwierdził 4–6× mediany); formy dominujące +1,5 s odnowienia; Mina
  siarkowa 3 ładunki, Deszcz meteorów 80%.
- Test pomiarowy: okno = jeden cykl odnowienia (6–20 s), kolumna `okno`; pliki: `bench_okno6s.csv` (stare okno),
  `bench_po_aurze.csv` (po korekcie aury, przed sługami i formami).
- Testy (MCP, edytor bez fokusu): EditMode **187/0/1** (nowe: `HolyFireAura_AndBanner_AreWeakened`,
  `Summons_DamagePerSecond_StaysNearTwiceTheMedian`, `DominantForms_PayWithCooldown`, `HitQueryTests`); PlayMode `HuSkillCasterPlayTests` + `SealFlowPlayTests` + `HeroSkillPlayTests` **18/18**.
- **Ściana ognia** (dawny Ognisty krąg – dublet Novy ognia u maga): skill Turris na komponentach HU
  (`Seals/TurrisSkills.cs`, nowy komponent `Wall` → `HuWall` w `HuRuntimes.cs`), id `spell_firering` bez zmian
  (odblokowania, profile). Ściana 7 m (III: 8 m) w poprzek celowania do 8 m, płonie 4 s (III: 5 s), parzy co 0,5 s
  i podpala; odnowienie 9 s, 22 many. Formy: **Fala ognia** (rusza 3,5 m/s, płonie ¼ krócej), **Forteca ognia**
  (pierścień 4 m wokół bohatera, +3 s odnowienia). Forma Kręgu żaru „Ściana ognia” → „Szereg kręgów”.
  Asset odświeżony celowanym `OverwriteFromDefaults("spell_firering", "unlock_firering")`. EditMode **188/0/1**,
  `HuSkillCasterPlayTests` 6/6, zrzuty `TestResults/hu-seals/spell_firering*.png` obejrzane.
- **Następny krok:** granie (aura, sługi, formy na piętrach 1–6); opcjonalnie pomiar per klasa (techniki z właściwą bronią).

### Poprzednia sesja — 2026-10-23: audyt balansu bohaterów, etap 4 (skille i bronie HU) i pieczęcie

Szczegóły: [ETAP4_PIECZECIE.md](ETAP4_PIECZECIE.md).
- **Audyt balansu bohaterów** (model DPS/EHP z danych): anomalie różdżki ziemi, stałych sług, darmowej aury i dwuręcznej
  broni ×1,45 miecza. Korekty wdrożone razem z importem (niżej). Łotrzyk ze sztyletem nadal najsłabszy – do decyzji.
- **Skille HU wykonywane z komponentów** (port `SkillCaster`/`SkillShape`): 61 skilli (32 nowe), 25 komponentów, jedno
  źródło prawdy w parametrach HU (`Resources/HellCatalog/skills.json`, importer `tools/art/import_hu_catalog.py`).
  Wcześniejsze 29 skilli HU przeszło na ten sam wykonawca (poziomy wg krzywych HU zamiast `powerPerLevel` Turris).
- **Pieczęcie:** 186 (90 form, 51 wsparć z 11 demonicznymi, 22 zespolenia, 23 więzi), bezpieczniki HU (suma mnożników
  ≤ +200%, kopie ≤ 1,5× i ≤ 24 zdarzenia, wyzwalacze co 1,5 s), sakwa, poziomy II–III, łup z elit, dar bossa,
  oferty w sklepie dusz, zakładka „Pieczęcie” w ekwipunku, znaczki w HUD, przełączniki na poligonie, kodeks w profilu.
- **Bronie:** 19 nowych (12 wariantów + 7 unikatów z mocami: ostatnia szansa, wybuch zwłok, aura żaru, ciernie, łańcuch
  błyskawic, pakt krwi, przebicie). Wszystkie 47 broni HU przeliczone zasadą DPS (proporcja do miecza jak w HU; tempo
  z ataków/s) – miecz bez zmian, dwuręczne 100 → 80–90, różdżka ziemi wolniejsza.
- **Korekty balansu (jawne, `Seals/HuBalance.cs`):** sługi żyją ≤ odnowienie; aktywna aura rezerwuje 20% wytrzymałości
  (Pobożność: 20/30% many jak HU); unikaty w nagrodach z wagą 0,35; skille z `MinWave` HU wchodzą od piętra 1/3.
- **Poprawki przy okazji:** doskok po akcji ślizgał bohatera o ~5 m (pęd 25 m/s hamowany zwykłym opóźnieniem) – pęd po
  akcji przycięty do prędkości sprintu; doskok HU przechodzi przez ciała wrogów; uderzenie na końcu doskoku nie ginie
  przy wcześniejszym końcu akcji.
- Assety: `TurrisSetup.SyncContent` (51 nowych), celowane `OverwriteFromDefaults` 28 broni HU, ponowne przypisanie modeli
  (`HeroAssetImport.AssignContentAssets`, `HellMigrationImport.Run`). Bez `RunBatchForce`.
- Testy (przez MCP, edytor bez fokusu): EditMode **183/0/1**; nowe `HuSealTests` (14), `HuContentTests` (13),
  `HuSkillCasterPlayTests` (6, w tym 151 rzutów: każdy skill i forma zadaje obrażenia), `SealFlowPlayTests` (1).
  Pełny PlayMode (bez fokusu): **109 zaliczonych, 4 niezaliczone, 18 pominiętych galerii**. Po poprawce: `FullFlow_WithGamepadOnly`
  i `Sounds_FollowCombatAndFlow…` zaliczone osobno; zostają 2 znane testy zależne od fokusu (`Impact_Hitstop…`, `Demon_Walks…`).
  Galeria: `TestResults/hu-seals` (16 skilli/form, sklep dusz, menedżer pieczęci) – obejrzana.
- **Do oceny w graniu:** balans form i wsparć w Turris, ceny pieczęci, częstość łupów (6 pięter zamiast 30 fal),
  rezerwacja aury, siła unikatów.

### Koncepcje oprawy — 2026-10-23

- Wytypowano [zakres PoC gotyckiego anime](POC_GOTYCKIE_ANIME.md): wojownik, miecz/tarcza,
  trzy skille startowe, dziedziniec pierwszego piętra oraz chochlik z wariantami żywiołów.
  Użytkownik ograniczył próbę do **jednego wojownika**. Wykonano jedną generację Meshy 7.1 (standard, 2K):
  zadanie `01a16a0a-a6e5-77cc-81be-c97226baf934`, `SUCCEEDED`; rzeczywiste saldo API **159 → 129**, koszt **30 kredytów**.
  [Pliki i raport](../ArtSource/GothicAnimePoC/Warrior/README.md): oryginalne GLB/FBX, tekstura, scena `.blend`, audyt JSON i trzy obejrzane rendery.
  Oba formaty wczytane w osobnym Blenderze 5.2.2; tekstura 2048², jeden materiał, ~45,8 tys. trójkątów.
  Brak rigu, wag i animacji; dłonie, szyja i łączenia płyt do czyszczenia, powtórzona klamra z tyłu do korekty.
  Bez integracji z Unity i bez testów gry na tym etapie; bez zmian bieżącej postaci ani plików Hell Unleashed.
  Gotowa [referencja dla Meshy](concepts/2026-10-23/meshy/warrior-reference-v1.png), przygotowana przez image_gen bez kredytów Meshy.
  Osobny Meshy MCP 0.6.5 zainstalowany w lokalnym katalogu Codex; inicjalizacja i lista 28 narzędzi sprawdzone.
  Klucz API użytkownik uzupełnił wyłącznie lokalnie w konfiguracji klienta; połączenie działa.
  Bez płatnego rigowania, animacji i ponowień. Parametry w `warrior-job-v1.json` obok referencji.
  Kolejny krok: [plan wdrożenia](PLAN_WDROZENIA_WOJOWNIKA_ANIME.md); obecny szkielet sprawdzony, lokalna wersja v2 ma zwinięte palce.
- Przygotowano cztery propozycje na podstawie zrzutu gry: [gotyckie anime](concepts/2026-10-23/01-gotyckie_anime.png),
  [malowana makieta](concepts/2026-10-23/02-malowana_makieta.png),
  [mroczna rycina](concepts/2026-10-23/03-mroczna_rycina.png),
  [graficzne low-poly](concepts/2026-10-23/04-graficzne_low_poly.png).
  Wbudowany image_gen; obrazy, zrzut odniesienia i prompty w `docs/concepts/2026-10-23/`.
  Użytkownik wybrał kierunek gotyckiego anime; zmiany wyglądu nie są jeszcze wdrożone w grze.
  Priorytet użytkownika: łatwiejsze tworzenie assetów 3D przy atrakcyjnym wyglądzie; dopuszczona inspiracja anime.

Aktualizacja: 2026-10-23 (migracja renderowania Built-in → URP 17.6 + Linear, gałąź `feature/urp-vfx-migration`). Opis gry, sterowania i architektury jest w [README.md](../README.md). Ten plik służy do przekazania pracy między sesjami.

### Poprzednia sesja — 2026-10-23: poligon umiejętności, smuga ognia, krótsze stopy wojownika

- **Poligon umiejętności** (wzorem `SkillRange.cs` z Hell Unleashed): przycisk w menu głównym pod „Odblokowania”
  (nie wyżej – `GamepadPlayTests.Unlocks…` liczy pozycje menu). Arena bez fal, manekiny bez AI
  (`EnemyBrain.TrainingDummy`), panel po prawej (`GameUI.Range.cs`, do wysokości 740 – niżej lista sterowania HUD):
  rodzina klasy → specjalizacje → „Wspólne”, poziom, cele Gromada/Ciężcy/Kolos/Pusto, Rań/Wychłódź, Pokaz.
  Wybrana umiejętność pod Q, poprzednia pod E; bez odnowień, bohater nieśmiertelny (`PlayerCombat.Immortal`), cele wstają po 1,6 s.
  Klawisze `[ ] - = T B V P`, Alt – kursor do panelu, Esc → „Wyjdź z poligonu”. Kod: `Core/SkillRange.cs`, `Core/GameRoot.Range.cs`.
  Pad w poligonie: tylko zwykła nawigacja menu (panel obsługiwany klawiaturą/myszą). Test `SkillRangePlayTests`.
- **Smuga ognia za ruchomym celem:** `HellParticles.Follow()` (dziedziczenie prędkości emitera) w `StatusFx` – ogon płomienia
  na biegnącym wrogu 3,31 m → 0,18 m (`HellFxPlayTests.StatusFlame_OnMovingTarget_KeepsShortTail`).
- **Stopy wojownika −15%** (uwaga użytkownika): `CharacterVisualDefinition.footLength` (0,85 dla `visual_hero_warrior`),
  `FootMesh.Shorten` ściska wierzchołki stopy/palców w poziomie wzdłuż kostka→palce (kości stopy mają skośne osie, skala kości
  przechyliłaby podeszwę). Nosek 21,2 → 18,0 cm przed kostką, podeszwa bez zmian. Wymaga Read/Write na `hero_warrior.fbx`
  (ustawione; `HeroAssetImport` też to ustawia); bez Read/Write ostrzeżenie i oryginalna stopa. Zrzuty: `TestResults/feet85`.
- Testy (edytor bez fokusu): EditMode 156/0/1; PlayMode 105 zaliczonych, 18 galerii pominiętych, 1 błąd – znany
  `Impact_Hitstop…` (dławienie bez fokusu). Po zmianie stóp: `Run_FeetInStancePointLikeRestPose`, `PlayerRun_PlantedFootDoesNotJump`
  i galerie przechodzą. `import_hu_vfx.py --verify` OK.

### Poprzednia sesja — 2026-10-23: płynność biegu i obrotów

Uwaga użytkownika: nogi w biegu „teleportują się”, jakby cykl miał kilka klatek; obroty za szybkie.
- **Diagnoza pomiarem** (nie klatkaż: praca klatki w walce 3,5 ms mediana, 6,8 ms p95 przy 2560×1248 –
  `FrameCostPlayTests`). Klip biegu nagrany przy 3,25 m/s był grany 1,6× dla 5,2 m/s: cykl 0,4 s (~300 kroków/min),
  noga w wymachu przeskakiwała 18–24 cm na klatkę przy 60 kl./s; stopa podporowa ślizgała się 2–4 cm/kl., przy zwrocie do 15 cm.
- **`ClipAnimationDriver`:** kadencja biegu ograniczona do 1,2× (`RunCadenceMax`, cykl 0,53 s), resztę drogi daje dłuższy
  krok (`ApplyLegIK`: stride warping do 1,4× przez dwukościowe IK, kolano w swojej płaszczyźnie) i **blokada stopy** –
  stopa, która stanęła, zostaje w miejscu świata do oderwania (puszcza powyżej 17 cm lub 35 cm od pozy klipu).
  Stopa w powietrzu idzie za goleniem, na ziemi zostaje płasko (bez skrętu względem kolana).
- **Obrót gracza:** `playerTurnSpeed` 1080 → 540°/s (GameConfig i domyślna wartość), końcówka swobodnego obrotu
  wygaszana wykładniczo (`TurnEase`); garda i śledzenie celu w ciosach bez zmian.
- Wynik (bieg prawdziwego gracza na wirtualnym padzie, `RunFeelPlayTests`): stopa na ziemi w biegu na wprost
  mediana 0,0 cm/kl., p95 3,9 cm (moment lądowania); przy zwrocie max 9 cm. Noga w wymachu nadal ~18 cm/kl. przy 60 kl./s –
  to fizyka biegu 5,2 m/s (stopa ~2× szybciej niż ciało), przy 144 Hz ~7 cm.
- Nowe testy: `RunFeelPlayTests`, `FrameCostPlayTests`, `HeroModelPlayTests.Run_FootMovesEveryFrame_NoHeldPoses`,
  galeria `RunFrames_WhenRequested` (32 kolejne klatki: sterownik vs nagranie).
- Testy (edytor z fokusem): **EditMode 156/0/1, PlayMode 104 zaliczone, 0 błędów, 18 galerii pominiętych**.
  Przy fokusie przechodzą też `Impact_Hitstop…`, `Demon_Walks…` i test muzyki – wcześniejsze błędy wynikały z dławienia
  edytora bez fokusu (~15 kl./s). Nowe testy ładujące `Main` sprzątają scenę (`UnityTearDown`) – bez tego jej GameUI
  rysował HUD zniszczoną kamerą i wywracał 40+ kolejnych testów.
- **Do oceny w grze:** tempo biegu, długość kroku, szybkość obrotu.

### Poprzednia sesja — 2026-10-23: efekty AAA, krok 2 (lód, ziemia, pule, drganie powietrza)

Szczegóły i pomiary: [EFEKTY_AAA.md](EFEKTY_AAA.md).
- Lód: kryształy z siatek (`IceCrystals`, `Turris/FxIce`), szron na ziemi (`Turris/FxFrost`), mgła przy ziemi, zimny błysk;
  podmuch mrozu zostawia kryształy i szron w stożku.
- Ziemia: ściana pyłu (`ShockRing`, `Turris/FxShockRing`), pęknięcia z żarem (`Turris/FxCracks`), kurz.
- Pule: błyski (`HellFxPool.OneShot`), ślady (`FxLibrary.TimedDecal`), `LightningBolt`, `ShockRing`, `IceCrystals` –
  potwierdzone ponowne użycie po drugiej fali.
- Drganie powietrza: Opaque Texture w URP, `Turris/FxDistort` nad ogniem i płonącą ziemią.
- Testy: EditMode 156/0/1; PlayMode 98 zaliczonych, 17 galerii pominiętych, 3 niezaliczone – dwa znane (tempo klatek)
  i `AudioPlayTests.Sounds_FollowCombatAndFlow…`, który pada tylko przy stanie statycznym `AudioService` z poprzedniego
  przebiegu (bez przeładowania domeny); po przeładowaniu przechodzi zarówno na kodzie kroku 1, jak i 2.
  Receptury HU: `import_hu_vfx.py --verify` OK. Koszt najgorszego przypadku (18 wybuchów + 6 błyskawic): +2,3 ms przy 1080p.

### Poprzednia sesja — 2026-10-23: efekty AAA, krok 1 (uber-shader, ogień, błyskawica)

Dziennik etapu: [EFEKTY_AAA.md](EFEKTY_AAA.md). Niezacommitowane (na `feature/urp-vfx-migration` po commicie migracji).
- `Turris/HellParticle` jako uber-shader: jądro HDR, erozja z Custom Data, falowanie UV, płynny flipbook, miękkie
  przecięcie z podłożem; profile per tekstura w `HellVfxMaterials`, strumienie w `ApplyStreams`. Receptury HU bez zmian.
- Naprawa: renderer URP kopiował głębię po przezroczystych (`After Transparents`) – miękkie cząsteczki nie działały; teraz `After Opaques`.
- Ogień: błysk HDR i ślad na ziemi (zwęglenie + stygnący żar, `Turris/FxScorch`). Błyskawica: `LightningBolt`
  (poszarpany kanał, odgałęzienia, rdzeń HDR + niebieska otoczka, ponowne wyładowania, ślad uderzenia z nieba).
- Nowy test-galeria `HellFxPlayTests.BurstSequence_WhenRequested` (sekwencje życia wybuchów i błyskawicy).
- Testy: PlayMode 99 zaliczonych, 17 galerii pominiętych, 2 niezaliczone – te same, zależne od tempa klatek, co po migracji.
  Koszt: +~0,3 ms przy 6 równoczesnych wybuchach ognia (1080p, edytor).

### Poprzednia sesja — 2026-10-23: migracja Built-in → URP + Linear

Pełny opis, inwentarz, wyniki i znane problemy: [MIGRACJA_URP.md](MIGRACJA_URP.md). Gałąź `feature/urp-vfx-migration`
(niezacommitowana); migawka drzewa sprzed migracji: `refs/backup/pre-urp-worktree`.
- URP 17.6.0 (Forward+, HDR, MSAA 2×, Depth Texture), Color Space **Linear**, domyślny Volume: Bloom (próg 1), ACES,
  Color Adjustments (ekspozycja +1), winieta. Assety w `Settings/URP/`.
- Konwerter Unity: 72 materiały Standard → URP Lit (GUID bez zmian). Ręcznie przepisane shadery: `HellBody` (Lit +
  `_HellSurface`), `HellParticle`, `HellGround`, nowe `Turris/FxAdditive` i `Turris/FxAlphaBlended` zamiast Legacy Particles.
- Kod tylko renderujący: `RenderingSetup` (post na kamerach, kompensacja świateł punktowych, kalibracja słońca 2,0 i otoczenia ×1,6),
  `_BaseColor`/`_Smoothness` zamiast `_Color`/`_Glossiness` (w URP Lit te stare nazwy istnieją, ale są ignorowane).
- Wyniki: kompilacja i konsola 0 błędów, EditMode 156/0/1, PlayMode 115/117 – dwa testy zależne od długości klatki
  padają identycznie także w Built-in (edytor bez fokusu ~15 kl./s), do powtórzenia przy aktywnym oknie Unity.
  0 klatek magenta w 94 zrzutach; scena gry ~75% jasności Built-in (celowo ciemniej); render 1080p 4,8 ms.
- Nie rozpoczęto przebudowy efektów żywiołów ani VFX Graph – kolejny etap.

### Poprzednia sesja — 2026-10-23: ustawienie stopy

- Uaktualniono [plan migracji](PLAN_MIGRACJI_HELL_UNLEASHED.md) w projekcie Turris oraz raport P0/P1:
  stan etapów, korekty stopy/noska, wyniki testów z rozróżnieniem kolejności zmian, praca przez MCP i zadania do odbioru.
  Starszy `PORT_HELL_UNLEASHED.md` odsyła do aktualnego planu. Etap 4 pozostaje nierozpoczęty.
- Referencja użytkownika: [nienaturalna stopa](references/2026-10-22-nienaturalna-stopa.png).
  Pomiar wykazał obrót całej tylnej nogi w źródłowym `Idle_Combat`: prawa stopa około **80°** od kierunku postaci,
  przy znacznie mniejszym skręcie samej kostki. Nie zmieniano klipów ani modeli.
- `ClipAnimationDriver.ApplyIdleLegPose` ogranicza rozwarcie stóp w spoczynku do **40°**.
  Obraca nogę wokół osi biodro–kostka, zachowując położenie kostki i długości kości, następnie zachowuje pochylenie podeszwy.
  Korekta jest ważona udziałem spoczynku i wygasa podczas akcji całego ciała; wcześniejsza korekta stóp w biegu pozostaje.
- Nowy `FootPosePlayTests` mierzy spoczynek i blok na sześciu modelach (wojownik, dual, paladyn, berserker, wiedźma, łucznik).
  We wszystkich prawa stopa ma **40°**, lewa około **9°**; skręt kostki pozostaje poniżej 30°
  (wojownik: prawa 18,85° przed → 21,16° po; lewa bez zmian 11,56°).
  Obejrzano galerię przed i po: `TestResults/foot/before/idle_feet.png`, `TestResults/foot/after/idle_feet.png`.
  Ostatnia kolumna to diagnostyczny przekrój z góry z celowo odciętym tułowiem; miarodajny wygląd siatki pokazują kolumny 1–2.
  Pomiary są w `TestResults/foot/{before,after}/ankles.tsv`.
- **Testy przez MCP w otwartym edytorze**, zgodnie z preferencją użytkownika. Wymóg zamknięcia Unity dotyczy
  `tools/run_tests.py`, który uruchamia drugi proces edytora; nie dotyczy Test Runnera ani MCP.
  Dodano pomocniczy `tools/unity_mcp.py` (lokalny klient, lista narzędzi, wywołania z JSON, oczekiwanie i zapis wyniku).
  Przed późniejszą korektą noska pełne EditMode: **156 zaliczonych, 0 błędów, 1 opcjonalny eksport WAV pominięty** (`TestResults/foot/EditMode-MCP.json`).
  Zestaw celowany: **5 zakończonych bez błędów** (pomiar i galeria stóp, bieg, krok przy ciosie, blok).
  Pełne PlayMode: **100 zaliczonych, 0 błędów, 16 opcjonalnych galerii pominiętych**, 185,9 s
  (`TestResults/foot/PlayMode-MCP.json`); testy obsługi menu padem także zaliczone.
- **Dalsza korekta po uwadze użytkownika o opadającym czubku stalowego buta:**
  [referencja](references/2026-10-23-opadajacy-nosek.png). W źródłowej siatce paladyna do kości palców przypisano
  157 lewych i 149 prawych wierzchołków; klip zmienia ich obrót względem modelu nawet o około 40°.
  Zrzut kontrolny z neutralnymi palcami potwierdził przyczynę: stalowy nosek zginał się jak miękkie obuwie.
  `CharacterVisualDefinition.rigidBootToes` włączono tylko u paladyna i wojownika, również w `HeroAssetImport`.
  Sterownik zachowuje lokalną pozę palców (pozycja, obrót, skala), przy swobodnym ruchu stawu skokowego i reszty nogi.
  Nie zmieniano źródłowych modeli, wag ani klipów. Pozostałe modele zachowują dotychczasową animację palców.
- `SteelBoots_ForefootMeshKeepsItsShape` bada ostatnie 4 cm noska z `BakeMesh` w spoczynku, biegu i ciosie.
  Mierzy zmianę kształtu w układzie stopy, po odjęciu przesunięcia całego fragmentu, bez przegubu cholewki ważonego do goleni.
  Próba kontrolna na kopii definicji z wyłączoną korektą wykrywa deformację **53,638 mm / 33,411 mm** (paladyn / wojownik);
  przy właściwym ustawieniu assetów oba wyniki wynoszą około **0,001 mm**, przy progu 3 mm.
  Pomiary: `TestResults/boot/shape.tsv`. Po tej zmianie **6/6 celowanych testów PlayMode przez MCP bez błędów**:
  kształt siatki, spoczynek/blok sześciu modeli, galeria, kierunek stóp w biegu, krok ciosu i blok górnej połowy ciała.
  Wynik: `TestResults/boot/Targeted-MCP.json`. Obejrzano `TestResults/boot/after/idle_feet.png` i
  `TestResults/boot/comparison.png` (paladyn / wojownik, lewa kolumna przed, prawa po).
- Hell Unleashed wyłącznie do odczytu: ponowna weryfikacja **2335 plików bez zmian**, **38 zgodnych tekstur VFX**.
  Zakres migracji nadal obejmuje etapy 1–3. Następnie ręczna ocena pozy w grze, chwytów i nowych walk oraz balans IV–V;
  nie rozpoczynać etapu 4 bez nowego zlecenia.

### Poprzednia sesja: P0/P1 — audyt, animacje i pełny katalog demonów

- **Zakres użytkownika: tylko etapy 1–3. Etapu 4 nie rozpoczynać bez nowego zlecenia. Hell Unleashed wyłącznie do odczytu.**
- Nie generowano nowych potworów: skopiowano istniejące GLB, tekstury i animacje HU, konwersja do FBX wyłącznie w Turris.
  Dodano 29 brakujących modeli demonów (łącznie 34 źródłowe i 3 wcześniejsze warianty chochlika), 7 modeli broni i 11 przedmiotów.
- `import_hu_catalog.py`: rzeczywiste `.tres`, poziomy skilli, SHA, mapowania 61 skilli/53 broni/34 demonów,
  porównanie fabryki i zapisanych assetów. Mapowania: 29 skilli, 33 bronie, wszystkie demony. Braki jawne w audycie.
  Trzy różnice fabryki to celowe poprawki skali/chwytu wcześniejszych modeli.
- IK obu rąk, tarcza, pozy sztyletu/różdżki, lustrzany atak lewej ręki, naciąg łuku.
  `AnimResolve` poprawnie wybiera `BowDraw` przed ogólnym `Cast`; test starego oczekiwania zaktualizowany.
  Dopracowane klipy zachowane. Wygląd wybierany niezależnie od klasy i zapamiętywany w profilu.
- Warstwy cienia/przemoczenia/żaru/elity na materiałach kopii modelu współpracują z błyskiem trafienia.
- `HellEnemyCatalog` i `EnemyBrain.Hell`: brakujące demony i bossowie, kombinacje, sygnały obrony, szał,
  fazy, kanały, osłony, jaja i rozpad. Wersja importu nowych definicji: **4**.
  Rytuał wzmacnia jednego wybranego sojusznika po kanale, trafienie kapłana go przerywa.
  Kokon zatrzymuje ruch/unik bez blokowania ataków/czarów, ma odporność na powtarzanie i pajęczynę bez obrażeń.
  Nekromanta ma budżet 3 × limit przywołań w życiu i zapasowy pocisk.
- Bestiariusz zawiera cały katalog i próbę walki bez zmian profilu; zwycięstwo wraca do bestiariusza.
  W ośmiu miejscach fal IV–V wprowadzono nowe archetypy. Zachowano sześć pięter, liczbę fal i 36 pozycji w listach wieży.
- Fale wroga trafiają właściwą frakcję. Pauza nie wypuszcza nowego trafienia; strefa nadrabia tyknięcia długiej klatki.
  Efekty wroga sprzątają się po śmierci/przejściu piętra, sługi po śmierci mistrza; wyklucie/rozpad bez osieroconego właściciela.
- **Kontrola źródła:** `import_hu_catalog.py --verify` potwierdza 2335 niezmienionych plików,
  `import_hu_vfx.py --verify` — 38 identycznych tekstur i zgodne receptury.
- **Wyniki:** pełne EditMode 156 zaliczonych, 0 błędów, 1 opcjonalny eksport WAV pominięty;
  testy mechanik migracji PlayMode 10/10 (134 zdolności i kroki kombinacji, modele, klasy/oba urządzenia,
  rytuał/kokon, sługi, pauza, strefy, osłona, próba bez zapisu).
  Pełne PlayMode window: **99 zaliczonych, 0 błędów, 15 galerii pominiętych**;
  pełne gfx: **97 zaliczonych, 0 błędów, 17 pominiętych** (galerie i OnGUI). Końcowa galeria P1: **5/5**.
  Raporty `TestResults/PlayMode-hu-p1-full-window.xml`, `PlayMode-hu-p1-full-gfx.xml`, `PlayMode-hu-p1-gallery.xml`.
  Wcześniejszy niestabilny pomiar szczytu reakcji ciała poprawiono na stały krok 1/120 s po rzeczywistym trafieniu;
  próg pozostał bez zmian. Oba końcowe pełne przebiegi zielone.
- Obejrzane arkusze wszystkich modeli i broni oraz warstw sylwetki w `TestResults/hu-p1/`.
  Pomiar 30 demonów w edytorze RTX 4070/D3D12, 1600×900: mediana 10,06 ms / p95 13,37 ms;
  CPU `Camera.Render` 1,06/1,68 ms, 208 systemów / 1800 cząsteczek, 1273 MiB. To diagnostyka edytora, nie benchmark buildu/GPU.
- Szczegółowy zakres i ograniczenia (w tym pojedynczy żywioł magiczny, kolejka faz, zachowany wcześniejszy balans):
  [RAPORT_MIGRACJI_P0_P1.md](RAPORT_MIGRACJI_P0_P1.md), [AUDYT_HU_P0.md](AUDYT_HU_P0.md),
  [KATALOG_DEMONOW_HU.md](KATALOG_DEMONOW_HU.md), [plan](PLAN_MIGRACJI_HELL_UNLEASHED.md).
- Import przyrostowy: menu **Turris → Hell Unleashed → Import P1 (dodaj brakujące)**;
  odświeża tylko własne nowe definicje o starszej wersji i brakujące referencje. Nie używać `RunBatchForce`.
  Przy kolejnej celowej zmianie wersji sprawdzić lokalne poprawki nowych demonów przed nadpisaniem.
- Następny krok: ręczna ocena chwytów i nowych walk, balans IV–V. Pozostałe 32 skille, pieczęcie,
  ekonomia, pełne shadery/siatki biomów i UI są późniejszymi etapami.

### Poprzednia sesja: cząsteczki Hell Unleashed → Unity

- **Hell Unleashed tylko do odczytu.** Nie zmieniano ani nie usuwano tam plików. Wszystkie kopie, nowe skrypty i dokumenty są w Turris.
- Skopiowano **38 PNG**, manifest pochodzenia i sumy SHA-256 do `Resources/HellVfx`.
  `tools/art/import_hu_vfx.py` odtwarza import; `--verify` bez zapisu potwierdza zgodność kopii i receptur ze źródłem.
- `HellParticleRecipes` + `HellParticles`: oryginalne warstwy HU w natywnym ParticleSystem, gradienty, siły,
  tłumienie/turbulencja, flipbooki 8×8 i nieruchome atlasy 4×4/4×2; współdzielone materiały i shadery w Resources.
- Wpięto cząsteczki do pocisków/różdżek, wybuchów, stref i aur, fal, statusów, ognisk/portali, śmierci,
  krwi/pyłu, zniknięcia/odskoku widma i kości już przeniesionych demonów.
- `HellFxPool` ogranicza wybuchy do 6 na rodzinę i krótkie światła do 6. Smugi kończą emisję i dogasają
  również po bezpośrednim zniszczeniu pocisku; strefa jest właścicielem swojej oprawy i sprząta ją przy usunięciu.
- **Audyt 29 skilli HU:** `HellSkillFx` i `HitData.visualElement` oddzielają wygląd od obrażeń.
  Wir głębin i wodna nova zachowują wodne cząsteczki przy mechanice mrozu. Rozpłatanie/Cięcie krzyżowe
  nie wywołują już FrostCone, Cięcie burzy używa powietrza. Poprawiono podwójne wywołania wybuchów
  pocisków/meteoru i bezterminową aurę nieśmiertelności.
- Przy sprawdzaniu skilli znaleziono też pomijanie kolejnych celów przebicia w długiej klatce.
  `Projectile.Step` porządkuje trafienia całego odcinka; test obejmuje dwa cele w jednym kroku 0,4 s.
- Szczegółowy zakres, tabela wszystkich 29 skilli i ograniczenia:
  [MIGRACJA_EFEKTOW_HU.md](MIGRACJA_EFEKTOW_HU.md).
- Plan pozostałych elementów po przeglądzie architektury i danych obu gier:
  [PLAN_MIGRACJI_HELL_UNLEASHED.md](PLAN_MIGRACJI_HELL_UNLEASHED.md).
- Obejrzano 6 renderów galerii `TestResults/hu-vfx/` (ogień, woda, powietrze, ziemia, lód, krew/cień).
  Adapter nie zastępuje całego renderera Godota: osobne shadery brył, materiały sylwetki, siatki kolców/lodu
  i tło sklepu 2D są wyszczególnione w planie. Potrzebna dalsza ocena wyglądu i wydajności w normalnej walce.

## 1. Stan na teraz

Prototyp jest **grywalny od początku do końca**:
- wybór typu bohatera i specjalizacji (15 bohaterów z Hell Unleashed), broni startowej, skilla startowego, trudności i dodatków w budżecie przygotowania;
- 6 pięter: I–III to areny Hell Unleashed (portale z demonami, Władca Bramy), IV–VI – piętra Turris z bossem; nagrody między piętrami, kapliczki;
- śmierć i szybki restart, meta-postęp z zapisem.

Działają mysz z klawiaturą oraz pad, także w menu.

| Obszar | Stan |
|---|---|
| Walka (blok tarczą i bronią, parowanie, unik z oknem niewrażliwości, przełamanie gardy, riposta, flaszki) | gotowe, pokryte testami |
| Umiejętności: 3 sloty pod osobnymi przyciskami, czary i techniki bronią, odnowienia i ładunki, odblokowania na stałe + nagrody tymczasowe | gotowe, pokryte testami (`SkillTests`, test w silniku szarży i rozpłatania) |
| Żywioły (ogień, mróz, błyskawica) i efekty (krwawienie, podpalenie, chłód/zamrożenie, porażenie), 3 reakcje, słabości wrogów; 22 czary i 16 technik | gotowe, pokryte testami (`StatusEffectTests`, `ElementSetTests`, testy w silniku) |
| Zestawy żywiołów: bronie i przedmioty żywiołów/krwawienia, odporności bohatera, warianty wrogów, cechy poziomów umiejętności, sklep dusz | gotowe – szczegóły i dziennik w [PLAN_ROZWOJU.md](PLAN_ROZWOJU.md) |
| Cechy postaci: Siła, Zręczność, Inteligencja, Wytrzymałość; +1 punkt po każdym piętrze | gotowe, pokryte testami (`AttributeTests`) |
| Przemapowanie przycisków (klawiatura/mysz i pad osobno), zapis w profilu | gotowe, pokryte testami (`InputRebindPlayTests`) |
| Modele broni: buława, młot, włócznia, kosa, noże bliźniacze, łuk; ciężki rzut ze smugą | gotowe |
| Bohaterowie z Hell Unleashed: 3 typy (Wojownik, Mag, Łotrzyk – w miejscu dawnych klas, te same id) + 12 specjalizacji; modele FBX (Meshy, T-pose, Humanoid), tekstury 4K, animacje Mixamo i Meshy, 13 modeli broni | gotowe, pokryte testami (`HeroClassTests`, `HeroSkillPlayTests`, `HeroModelPlayTests`) – **do oceny w graniu i balansu** |
| Skill klasy (G / Select), 9 cech klas, 4 style uniku, aury, sługi, fale, przebijanie/rozszczep/wybuch pocisków, doskok, przeciążenie, nieśmiertelność; żywioł Ziemia, zachwianie → ogłuszenie, pęknięcie pancerza | gotowe, pokryte testami |
| Fale wrogów na piętrach (36 wrogów na wieżę), Szkieletowy łucznik, strzały specjalne | gotowe – do oceny długość pięter w ręcznym graniu |
| Wyposażenie, statystyki, nagrody, odblokowania, trudność, zapis z wersjonowaniem | gotowe, pokryte testami |
| AI: przeciwnicy Turris, 14 archetypów HU, bossowie z fazami/kombinacjami, elity, kanały i osłony | wdrożone; nowe demony dostępne w próbie walki, wybrane na IV–V; do oceny balansu |
| Postacie: proceduralne humanoidy (240–345 części), animacja zsynchronizowana z fazami walki; nogi na IK, ruch wtórny, ataki w 5 etapach, peleryna i płaty tabardu na sprężynach, odmiany wrogów | gotowe, pokryte testami (`AnimationPlayTests`) |
| Odczucie ciosu (bez wstrząsów kamery): hitstop lokalny, reakcja ciała, odrzut, błysk, upadek w kierunku ciosu, warstwy dźwięku, wibracje pada | gotowe, pokryte testami (`CombatFeelTests`, `Impact_…`) – **do oceny w graniu** |
| Postacie FBX – wariant B | działa na bohaterach z HU (zestawy animacji broni `WeaponAnimSet`, gniazda dłoni, modele przedmiotów); braki względem HU – patrz [PORT_HELL_UNLEASHED.md](PORT_HELL_UNLEASHED.md) |
| Demony HU: komplet 34 definicji, modele i klipy Generic, 3 dodatkowe warianty chochlika, bestiariusz/próba, portale I–II i nowe archetypy IV–V | testy `DemonPlayTests`, `DemonContentTests`, `HellMigrationPlayTests` — **do oceny w graniu i balansu** |
| Efekty czarów i walki (ParticleSystem, kręgi runiczne, światła, smugi ostrzy) | gotowe |
| Areny: Dziedziniec, Krypta, Szczyt (65–120 tys. wierzchołków) | gotowe |
| Interfejs IMGUI z nawigacją padem | gotowy (prototypowy) |
| Dźwięk: 58 efektów syntezowanych w kodzie, muzyka (menu, walka, boss), tła 3 aren, pogłos, głośność w ustawieniach | gotowe, pokryte testami (`AudioSynthTests`, `AudioPlayTests`) – **do oceny na słuch** |

### Wyniki testów (ostatni przebieg)

- **Po migracji cząsteczek (2026-10-22):** EditMode **150 zaliczonych, 0 błędów**, 1 pominięty eksport WAV.
  Pełny PlayMode `gfx`: **87 zaliczonych, 0 błędów**, 12 pominiętych (10 galerii bez zmiennej środowiskowej i 2 testy menu wymagające OnGUI).
- **Skille HU w oknie Unity po poprawce przebijania:** **11/11** (`PlayMode-hero-skills-window.xml`),
  w tym stożki, wodny Wir głębin, strzała żywiołów, aury, fale i dwa cele przebite w jednej klatce.
  Oba testy `GamepadPlayTests` zaliczone w pełnym przebiegu okienkowym.
- **Uwaga o wcześniejszym przebiegu okienkowym tej sesji:** test reakcji ciała `Impact_HitstopBothSides_KnockbackAndBodyReaction`
  raz nie osiągnął progu odchylenia. Powtórzenie osobno oraz końcowy pełny `gfx` przeszły; nie zmieniano progu ani algorytmu reakcji.
  Drugi wykryty błąd (przebijanie) został poprawiony w kodzie i potwierdzony testem regresji. Raport wcześniejszego przebiegu: `TestResults/PlayMode-full-window.xml`.
- **Grafika/import HU:** obejrzane 6 PNG w `TestResults/hu-vfx/`; `import_hu_vfx.py --verify` potwierdza
  identyczność 38 tekstur i SHA-256 źródłowego `ElementFx.cs`, zgodność wygenerowanych receptur. W kopii nie ma plików `.import` Godota.
- **EditMode (2026-10-22, przez Unity MCP w otwartym edytorze):** 150/150 + eksport WAV pominięty (bez `TURRIS_SOUND_DIR`).
- **PlayMode po sesji „stopa w biegu, postawa HU, bestiariusz” (2026-10-22):** 81/81 + 9 galerii pominiętych; EditMode 150/150.
- **PlayMode po sesji „krok ciosu, bieg, portale” (2026-10-22):** 79/79 + 8 galerii pominiętych. Wszystkie testy w silniku używają bohaterów z modelami FBX (`TestContent.Create()` nadaje klasom wygląd z assetów; także `legacy_*`).
- **PlayMode (2026-10-22, przez Unity MCP):** 78/78 + 5 galerii pominiętych bez `TURRIS_SHOT_DIR` (galeria demonów `demon_*` obejrzana). Testy demonów używają konfiguracji z assetów i Wojownika z modelem FBX. Galerie bohaterów i gry obejrzane (`hero_*`, `0f_loadout_paladin_ui`, `1_floor1_start`, `1f_signature_warcry`). Testy pada bywają niestabilne, gdy okno Unity traci fokus (wirtualny pad nie dostaje wejścia) – ponowne uruchomienie przechodzi.
- **PlayMode w trybie wsadowym:** 2 testy pada są pomijane, bo nie działa tam `OnGUI`. Galerie zrzutów są pomijane bez zmiennej `TURRIS_SHOT_DIR`.

### Metryki szczegółowości (z testu `DetailLevel_ManyParts_FewRenderers`)

Części ciała postaci (bez broni i tarczy) i liczba rendererów:

| Postać | Części | Renderery |
|---|---|---|
| Rycerz | 273 | 22 |
| Mag | 295 | 15 |
| Heretyk | 326 | 15 |
| Ghul | 239 | 18 |
| Strażnik | 291 | 20 |
| Kasztelan | 332 | 21 |

Areny z testu `Arenas_BuildWithSimpleCollidersAndBakedVisuals` (po powiększeniu o ~25% powierzchni: rozmiary 14,6 / 24,6 / 19):

| Arena | Wierzchołki | Światła | Systemy cząsteczek |
|---|---|---|---|
| Dziedziniec | ~128 tys. | 5 | 20 |
| Krypta | ~67 tys. | 12 | 44 |
| Szczyt | ~128 tys. | 4 | 16 |

### Stan repozytorium

- GitHub: `dbujak79/Turris_Damnatorum`, gałąź `main`. Ostatni commit: `83ddcac new class and weapons`.
- **Niezatwierdzone zmiany:** dźwięk (`Scripts/Runtime/Audio/`, wpięcia w walkę, przebieg i UI, głośność w profilu v3), testy dźwięku, ta dokumentacja. Do zatwierdzenia na prośbę użytkownika.

## 2. Historia iteracji (sesja 2026-09-28)

1. **Prototyp od zera.** Pusty katalog, Unity 6000.6.3f1. Wszystkie systemy z zadania, treść w ScriptableObject generowana skryptem `Turris → Setup`, testy EditMode i PlayMode.
2. **Obsługa pada w menu.** `UINavigator`: fokus przestrzenny, A/B, przewijanie list, podpowiedzi przycisków. Test pełnej ścieżki wirtualnym padem.
3. **Pakiety.** Unity przy tworzeniu projektu dopisał szablonowe pakiety. Usunięte zostały przestarzałe `analytics` i `purchasing` oraz niepotrzebne `xr.legacyinputhelpers` i `multiplayer.center`. Jednorazowy `TypeLoadException` (Newtonsoft) przy przeładowaniu po ich usunięciu jest nieszkodliwy.
4. **Humanoidy zamiast kapsuł.** Użytkownik wybrał wariant „oba”: A (proceduralny humanoid z animacją) teraz, a architektura gotowa na B (FBX z Mixamo przez Playables).
5. **Szczegółowość ×5.** Siatki proceduralne (`ProcMesh`), materiały PBR, scalanie części na kość.
6. **Kolejne ×2 postaci + efekty + areny.** `FxLibrary`, `ArenaBuilder`, `DetailPass`/`DetailPass2` w `HumanoidRig`.

Użytkownik ocenił ostatni wygląd jako „jest nieźle”. Kolejne prośby dotyczyły wyłącznie zwiększania szczegółowości.

### Sesja 2026-09-29: tempo i responsywność

Prośba: gra nieco szybsza i płynniejsza, bohater bardziej responsywny, unik ma przerywać atak.
- `ActionRules`: unik przerywa atak/czar w każdej fazie; z uniku po `dodgeCancelAfter` (0,1 s fazy zakończenia) można wykonać kolejną akcję (poza flaszką).
- `PlayerCombat`: czasy akcji gracza skalowane przez `playerActionSpeed` (1,1) i `playerRecoveryScale` (0,8) – metody `ScaleStartup`/`ScaleRecovery`; okno parowania bez zmian. Przerwana inkantacja zwraca manę i gasi efekt skupienia.
- `PlayerController`: płynne przyspieszanie/hamowanie (`moveAcceleration` 45, `moveDeceleration` 60), obrót 1080°/s, unik z prędkością wygasającą liniowo (ten sam dystans), wypad zgodny ze skalowanym zamachem.
- `GameRoot`: `Time.timeScale = balance.gameSpeed` (1,1) tylko w trakcie gry.
- `ProceduralHumanoidAnimator`: przenikanie póz ~0,09 s przy zmianie akcji (poza startem z bezczynności – tam poza startuje z gotowości).
- Wartości w `Content/GameConfig.asset`: `moveSpeed` 4,6 → 5,2, `dodgeTotalDuration` 0,75 → 0,62, `inputBuffer` 0,25 → 0,3. Nowe pola dostają domyślne wartości bez regeneracji.
- Nowe testy: `Dodge_InterruptsAttackInAnyPhase`, `Dodge_InterruptsCast_RefundsManaBeforeRelease`, `Dodge_CanChainIntoAttack_AfterCancelPoint`.
- Poprawka po uwadze „przewrót zbyt szybki, nienaturalny”: nowe pole `dodgeRollDuration` (0,6 s) – przewrót i przemieszczenie trwają tyle samo (wcześniej ~0,38 s); `dodgeTotalDuration` 0,82, `dodgeCancelAfter` 0,24 (akcja możliwa tuż przed końcem przewrotu). Profil prędkości 1 − u² (równe toczenie, łagodne wyhamowanie), obrót zaczyna się po krótkim zgięciu i kończy przed wstaniem. Galeria: `pose_roll_side.png` (7 klatek co 0,1 s).
### Sesja 2026-09-29: umiejętności (3 sloty)

Prośba: szybki atak, mocny atak i trzy umiejętności (w tym czary) przypisywane pod przyciski, spośród dostępnych dla postaci; odblokowanie na stałe albo zdobycie tymczasowo między piętrami. Inspiracja: opis innej gry (sloty, odnowienia, ładunki, komponenty skilli, sklep między falami).
- **Model:** `SpellDefinition` to teraz „umiejętność” (nazwa klasy zostaje ze względu na assety): `category` (czar/technika), `cooldown`, `charges`, `extraChargeAtLevel`, `requiredTags`, parametry technik (`weaponMultiplier`, `arcAngle`, `tickInterval`, `moveMultiplier`). Nowe rodzaje: `Barrier`, `Cleave`, `ShieldBash`, `Charge`, `Whirlwind`, `Quake`.
- **Stan:** `SpellInstance` trzyma ładunki i odnowienie oraz flagę `permanent`. `RunState.skillSlots[3]` zastąpiło `attunedSpells`; `AssignSlot` (z zamianą miejsc), `ClearSlot`, `SlotOf`. `BuildSnapshot.skills[3]`. Usunięte: `maxAttunedSpells`, `CycleSpell`, `CurrentSpell`.
- **Walka:** `PlayerCombat.RequestSkill(slot)`; techniki w `FireSpell`/`SkillActiveTick` (łuk, krąg, kapsuła szarży, cykliczne trafienia młynka), `TechniqueHit` = lekki atak broni × moc. Osłona pochłania obrażenia w `ReceiveHit` (w pełni pochłonięty cios bez drgnięcia). Szarża narzuca prędkość przez `SkillMotion`, młynek pozwala powoli sterować.
- **Animacja:** techniki używają póz ataku (Thrust, Slam, SlashRight) lub parowania (uderzenie tarczą); młynek obraca całe ciało (`CharacterAnimState.spinAngle`).
- **Sterowanie:** Q/E/R i A/X/Y – sloty; parowanie przeniesione na lewy Ctrl i boczny przycisk myszy; flaszki na padzie na D-pad ↑/↓; zmiana celu D-pad ←/→.
- **Meta:** odblokowania typu `Spell` nie zajmują budżetu – trafiają do `LoadoutPlan.permanentSkills` i kolekcji każdego podejścia. Nieodblokowane umiejętności trafiają do puli nagród po dotarciu na ich piętro. Układ slotów w `ProfileData.skillSlots` (nowe pole, zgodne wstecz).
- **Treść:** nowe assety dodane przez `TurrisSetup.SyncContent` (nowa metoda: tylko dodaje brakujące assety i referencje). Odnowienia istniejących czarów dopisane w YAML. Rycerz startuje z Rozpłataniem i Uderzeniem tarczą; Pocisk arkanów jest domyślnie odblokowany, więc każda postać ma go w kolekcji.
- **HUD:** trzy sloty z przyciskiem, kosztem, odliczaniem i zasłoną odnowienia; pasek osłony; pasek bossa przeniesiony na górę ekranu.
- **Zrzuty:** nowe `UiShot` (ScreenCapture, tylko w oknie) – `0b_loadout_ui`, `1e_hud_skills_ui`, `2b_equipment_ui`; `1d_whirlwind` z kamery.
- **Do oceny / dalsze kroki:** balans odnowień i mnożników technik; ewentualne ograniczenie zmiany slotów tylko między piętrami (teraz ekwipunek i tak jest dostępny tylko tam); przemapowanie przycisków (osobno pad i klawiatura) jak w inspiracji; efekty statusów (krwawienie, podpalenie) i żywioły z inspiracji to osobny, większy krok.

### Sesja 2026-09-29: żywioły, efekty, więcej umiejętności

Prośba: więcej czarów i technik bronią oraz efekty żywiołów i krwawienia.
- **Model:** `Element` (None/Fire/Frost/Lightning), `StatusKind`, `StatusApplication` (rodzaj, warstwy, szansa). `AttackDefinition` ma `element`, `statuses`, `explosionRadius`; `HitData` je przenosi (+ `isStatusTick`). Obrażenia żywiołów to część magiczna.
- **Logika:** `Combat/StatusEffects.cs` – wspólna dla gracza i wrogów (warstwy, tyknięcia co 0,5 s, spowolnienie, zamrożenie z odpornością, porażenie, reakcje szok termiczny/przewodzenie, mnożnik kontroli dla bossów/elit). Parametry w `BalanceConfig` („Efekty i żywioły”).
- **Wróg:** słabości/odporności (`EnemyDefinition.fire/frost/lightning/bleedMultiplier`), efekty przy trafieniu, tyknięcia, chłód spowalnia ruch i zamach, zamrożenie przerywa atak (także z pancerzem). **Gracz:** to samo po stronie obrony (zamrożenie = wymuszone drgnięcie, chłód spowalnia akcje), osłona pochłania tyknięcia.
- **Nowe umiejętności:** czary Kula ognia (wybuch, `Projectile.SplashShare` 60%), Lodowy podmuch (`Cone`), Mroźna fala, Łańcuch błyskawic (`Chain`), Płonąca ziemia (`Zone` → `DamageZone`), Płomienne ostrze (zaklęcie z żywiołem i efektem – działa też na techniki); techniki Krwawe cięcie, Wypad, Seria cięć (`Flurry`), Uderzenie gromu (technika z żywiołem – pół na pół), Rzut nożami (technika-pocisk).
- **Efekty wizualne:** `Fx/StatusFx.cs` (ogień, szron, iskry, krew + zabarwienie), `FxLibrary.Lightning/FrostCone/BurningGround`. Liczby tyknięć i reakcje przez nowe `CombatEvents.WorldText` (bez efektów trafienia).
- **Treść w assetach:** `SyncContent` dodaje nowe assety i **uzupełnia nowe pola tylko, gdy są domyślne** (`FillNewDefaults`) – tak trafiło krwawienie do toporów/sztyletu i efekty/słabości do 4 wrogów.
- **Zrzuty:** `fx_elements.png` (płonący+krwawiący, wychłodzony, zamrożony, porażony ghul, błyskawica, mróz, płonąca ziemia).
- **Do oceny:** balans (siła krwawienia ×2 w ruchu, czas zamrożenia, obrażenia strefy); czy efekty wrogów (chłód heretyka) nie są zbyt karzące; ewentualne odporności gracza na żywioły (przedmioty/wzmocnienia) jako kolejny krok.

### Sesja 2026-09-29: plan rozwoju – etapy G, E, F

- **G – cechy:** `StatType.Vigor` → `Toughness` (ta sama liczba w assetach), `Endurance`/`Mind` → `LegacyEndurance`/`LegacyMind` (modyfikatory ze starych assetów przeliczane w `StatCalculator`), pola `BalanceConfig` przemianowane z `FormerlySerializedAs`, `ClassDefinition.toughness` (dawniej `vigor`). Zręczność: `BuildSnapshot.evasionChance`, `attackSpeed`, dłuższy unik. Twarde wymagania czarów (`spellRequirementsAreHard`). `RunState.attributePoints` + panel rozwoju między piętrami.
- **E1 – przemapowanie:** bindingi mają grupy `KeyboardMouse`/`Gamepad`; `PlayerInputReader.ApplyRebind/StartRebind/SaveOverrides/LoadOverrides`; ekran `GameScreen.Controls`; `ProfileData.bindingOverrides`. **Pułapka:** `SaveBindingOverridesAsJson` dopasowuje po id bindingów, które przy mapie tworzonej w kodzie są losowe – dlatego własny zapis po nazwie akcji i urządzeniu.
- **F1/F2:** `WeaponModel.Mace/Hammer/Spear/Scythe` (dopisane na końcu wyliczenia), Włócznia i Kosa, `ThrownWeapon` + `SpellKind.WeaponThrow`, `CharacterVisual.SetWeaponVisible`.
- Pełny dziennik: [PLAN_ROZWOJU.md](PLAN_ROZWOJU.md).

### Sesja 2026-09-29: plan rozwoju – etapy A–D

Plan z etapami i dziennikiem: [PLAN_ROZWOJU.md](PLAN_ROZWOJU.md). Zrobione: fundamenty (roztrzaskanie, odporności i premie żywiołów, modyfikatory efektów, broń z żywiołem, 8 nowych rodzajów umiejętności), treść (8 broni, 9 przedmiotów, 7 czarów, 5 technik, 3 warianty wrogów), cechy poziomów umiejętności, sklep dusz. Do zrobienia: przemapowanie przycisków (E), modele broni (F).

- **Do oceny w ręcznym graniu:** czy tempo nie jest za duże dla wrogów (gameSpeed przyspiesza też AI) i czy unik-przerwanie nie jest zbyt „bezpieczne” (ewentualnie dodać koszt wytrzymałości lub minimalny czas zamachu).

### Sesja 2026-09-29: dźwięk (dark fantasy)

Prośba: dźwięk w klimacie dark fantasy. W projekcie nie było żadnych plików audio, więc wszystko jest syntezowane w kodzie (wariant „oba”: działa od razu, a pliki w `Resources/Audio` podmieniają syntezę bez zmian w kodzie).
- **Synteza (`Audio/Dsp`):** szum przez filtr stanu TPT, oscylatory, rezonatory modalne (metal, szkło, drewno, dzwon z tercją małą), trzaski, chór (piły przez formanty „a/o”), Freeverb. Czysty C# – działa w wątku tła i w EditMode.
- **Efekty (`SoundSynth`, 58 rodzajów, 126 wariantów):** głośność względna, warianty, limit głosów i odstęp w `SoundInfo.For`. Głośności wyrównane pomiarem (K-ważony RMS w oknie 400 ms): trafienia ~−12…−15, świsty ~−19, kroki ~−29, tyknięcia ~−29, UI ~−22…−26.
- **Muzyka (`MusicSynth`):** pętle stereo 24 kHz; zdarzenia zawijają się na początek (`FoldLoop`), pogłos liczony okresowo – szew bez trzasku (test). Generacja ~16 s CPU w tle; efekty są gotowe wcześniej.
- **`AudioService`:** bank statyczny (raz na uruchomienie, klipy `DontUnloadUnusedAsset`), pula 40 źródeł, muzyka i tło z przenikaniem (dwie warstwy), `AudioReverbZone` za kamerą z presetem areny, pauza przez `AudioListener.pause` (muzyka/UI `ignoreListenerPause`), bicie serca < 30% życia. `Sfx.Play/Ui` – statyczne wywołania, bez usługi nic nie robią.
- **Wpięcia:** `CombatFxDirector` (wynik trafienia), `EnemyBrain` (zapowiedzi ciosów dźwiękiem, zamachy, pociski, ryk fazy, śmierć wg sylwetki, zamrożenie, roztrzaskanie, reakcje, tyknięcia), `PlayerCombat` (świst wg broni, łuk, unik, umiejętności wg rodzaju i żywiołu, flaszka, riposta), `Projectile`/`DelayedStrike`/`StormEffect`/`DamageZone`/`ThrownWeapon`, `PlayerController` (kroki), `GameRoot` (dzwon piętra, róg fali, chór, zwycięstwo, śmierć, dusze, sklep), `GameUI` (nawigacja).
- **Ustawienia:** ekran „Sterowanie” → „Ustawienia – dźwięk i sterowanie” (głośność ogólna/muzyka/efekty, krok 10%). `ProfileData` v3: `masterVolume/musicVolume/sfxVolume`, migracja z v2 ustawia domyślne.
- **Weryfikacja:** nie da się tego odsłuchać automatycznie – testy sprawdzają poprawność sygnału (brak NaN i przesterowania, cichy koniec, powtarzalność, różne warianty, szew pętli), a `AudioPlayTests` – odtwarzanie w grze (dzwon piętra, świst, unik, pauza, muzyka menu → walki, tło areny, śmierć wroga, dusza, głośność w profilu). WAV-y do odsłuchu: `TURRIS_SOUND_DIR=<katalog> python tools/run_tests.py EditMode`.
- **Do oceny na słuch:** czy świsty i trafienia nie są zbyt „syntetyczne”, głośność muzyki względem efektów, częstość zapowiedzi ciosów przy wielu wrogach, czy bicie serca nie męczy.

### Sesja 2026-09-29: start klasowy, łuk łotrzyka, umiejętności obszarowe, zachodzenie od tyłu

Uwagi użytkownika po graniu: łotrzyk nie może zacząć z łukiem; po uniku na skos za plecy wróg za szybko się odwraca; przy wielu wrogach każda klasa potrzebuje umiejętności obszarowej; dodatki na start – ogólne albo klasowe (rycerz nie powinien zaczynać z kulą ognia).
- **Start klasowy:** `UnlockDefinition.classes` (puste = ogólne). `MetaService.LoadoutOptionsFor(klasa)`, `PermanentSkillsFor(klasa)`, `SelectClass` (zmiana klasy usuwa cudze dodatki), walidacja odrzuca dodatki innej klasy. Przypisanie w `DefaultContent` (`ForClass`); ogólne: Czujność, Przetrwanie, Kojące światło, pierścień riposty, amulety przewodnika i salamandry. Pocisk arkanów (domyślny) – tylko mag. Nagrody w podejściu bez ograniczeń.
- **Broń startowa:** `ClassDefinition.alternativeWeapons` (łotrzyk: łuk), `LoadoutPlan.startingWeapon/Weapon`, zapamiętywana w profilu (`lastWeaponId`). Łuk nie pojawia się już jako dodatek łotrzyka.
- **Umiejętności obszarowe (odblokowane od razu, klasowe):** Tąpnięcie (`skill_stomp`, Quake z zasięgiem 0), Ognisty krąg (`spell_firering`, Nova z ogniem i podpaleniem), Wachlarz ostrzy (`skill_bladefan`, 10 noży dookoła, krwawienie). Zajmują trzeci slot na start; są też w puli nagród.
- **Obrót wrogów:** `BalanceConfig.enemyTurnDuringDodge` (0,35 w czasie uniku i `enemyTurnDodgeGrace` 0,4 s po nim) i `enemyTurnBehind` (0,6, gdy gracz jest ponad 100° za plecami); działa też na śledzenie w zamachu. `EnemyBrain.TurnMultiplier`, `PlayerCombat.TimeSinceDodge`.
- **UI:** wybór broni startowej w kolumnie klasy, dodatki filtrowane klasą („… klasy”), w „Odblokowaniach” oznaczenie „ogólne” albo nazwa klasy. Zrzuty `0b_loadout_ui`, `0d_loadout_rogue_ui`, `0e_unlocks_ui` obejrzane.
- **Assety:** `SyncContent` dodał 3 umiejętności i 3 odblokowania oraz uzupełnił `classes` w 38 odblokowaniach i `alternativeWeapons` łotrzyka (nowe pola w `FillNewDefaults`). Opisy rycerza i łotrzyka poprawione ręcznie w assetach.
- **Do oceny w graniu:** siła mnożników obrotu (zachodzenie od tyłu nie może być za łatwe), obrażenia i odnowienia nowych umiejętności obszarowych, podział odblokowań między klasy.

### Sesja 2026-09-29: odczucie walki, płynność animacji, szczegółowość modeli (pakiety A–C)

Prośba: płynniejsza gra i lepsze odczucie walki, dokładniejsze modele i więcej animacji. **Bez wstrząsów ekranu** – siłę ciosu oddają reakcja przeciwnika i dźwięk (trwała preferencja użytkownika).

**A – odczucie ciosu** (`Fx/ImpactDirector.cs`, nowe zdarzenie `CombatEvents.Impact` z `HitQuery.Apply` i riposty):
- **Hitstop lokalny** (`IHitStop`): trafiony i atakujący bronią zastygają na 0,05–0,15 s (lekki / ciężki i cios kończący / riposta; blok i parowanie 0,06–0,09 s). Stoją logika (`EnemyBrain`, `PlayerCombat.Tick`, ruch w `PlayerController`) i animacja (`CharacterVisual.Hold`); reszta gry toczy się dalej. Trafiona postać drży wzdłuż kierunku ciosu (sama postać, nie kamera). Techniki wielotrafieniowe nie zatrzymują atakującego (`HitData.weapon` = Auto).
- **Reakcja ciała** (`IImpactReactive`, sprężyny w `ProceduralHumanoidAnimator`): tułów, głowa (z opóźnieniem), ręce, miednica i przechył ciała ustępują w kierunku ciosu i wracają z lekkim przestrzałem – dokładane na każdą pozę, także w trakcie ataku. Biały błysk ~0,1 s (`CharacterVisual.Impact`).
- **Odrzut**: wróg 0,25–0,7 m (boss ×0,2, elita ×0,5, pancerz w zamachu ×0,3), bohater 0,4 m (blok – połowa).
- **Upadek w kierunku ciosu** (`CharacterAnimState.hitDirection/hitWeight`): na plecy, na twarz (z przygięciem w pasie) lub na bok; ciężki cios odrzuca ciało dalej, krótkie odbicie przy uderzeniu o ziemię + kurz i dźwięk.
- **Dźwięk – 4 nowe efekty** (`SoundId` na końcu wyliczenia): `HitBlunt` (obuch: buława, młot, pięści, kostur), `HitArmor` (cios w strażnika/rycerza), `KillBlow` (cios kończący), `BodyFall` (ciało o ziemię). Warstwy nakładane na dotychczasowe trafienie.
- **Wibracje pada** (`Core/Rumble.cs`): impulsy wygasające liniowo, osobne dla zadanego/otrzymanego ciosu, bloku, parowania, przełamania gardy i zabicia; w pauzie stoją. Ustawienie „Wibracje” (0–100%) w ekranie ustawień, `ProfileData.rumbleStrength` (nowe pole, bez zmiany wersji).
- Parametry w `GameConfig → balance` („Odczucie trafień”): `hitstopLight/Heavy/Riposte/Block`, `hitReactStrength`, `knockbackLight/Heavy`, `playerKnockback`.

**B – płynność animacji:**
- **Nogi na IK** (`SolveLegs`): stopa w podporze stoi na podłodze, w wymachu przenosi się łukiem (odbicie palcami, lądowanie na pięcie). Udział podpory maleje z prędkością (bieg ma fazę lotu), biodra obniżają się tyle, by noga sięgnęła. Długość cyklu 1,15–1,75 m (×1,25 w sprincie). Poślizg stopy w podporze: **0,44 m/s zamiast ~6 m/s** przy biegu 5,2 m/s. FK zostaje dla przewrotu, młynka, skoku i śmierci. Kroki dźwiękowe zsynchronizowane (co 0,9 m / 1,03 m w sprincie).
- **Ruch wtórny**: pochylenie przy ruszaniu/hamowaniu i przechył w zakrętach (sprężyna `Lean`), kołysanie miednicy w chodzie z kontrą klatki, przenoszenie ciężaru i rozglądanie się w bezruchu.
- **Ataki w 5 etapach** (`PoseLibrary.SampleAttack`): przygotowanie (ugięcie kolan), zamach z przestrzałem, przyspieszające cięcie, wybrzmienie z przestrzałem (`Pose.LerpUnclamped`), powrót. **Seria**: kolejny cios zaczyna zamach z wybrzmienia poprzedniego (nie z gardy). Skręt tułowia zaczyna się w biodrach, nogi go kompensują.

**C – szczegółowość i tkaniny:**
- **Peleryna z 4 segmentów** (`HumanoidRig.CapeSegments`) – łańcuch sprężyn: fala w dół, łopot w biegu, odchylenie przy ruszaniu i w zakrętach, podrzut przy trafieniu; nie wchodzi w plecy. Obszycie brzegów i szwy.
- **Płaty na biodrach** (`HumanoidRig.HipFlaps`: tabard przód/tył, fartuch strażnika, pasy skórzni): kołyszą się, a kolano w wykroku je odpycha (twarde ograniczenie – koniec przenikania tkaniny przez uda).
- **Odmiany wrogów** (`RigLook.Vary`, w `WorldBuilder.CreateEnemy`): odcień tkanin/zbroi/skóry, tusza, grubość kończyn, garb, u łuczników peleryna; styl, broń i tarcza bez zmian (czytelność walki). Boss bez odmian.
- **Krew na broni**: metalowe części broni (tylko materiały „Metal”) ciemnieją po trafieniach i schną ~30 s.
- Metryki: rycerz 286 części / 26 rendererów, strażnik 291/21, kasztelan 345/23.

- **Nowe testy:** `CombatFeelTests` (EditMode: ciężar ciosu, wibracje, seria z przestrzałem), `HitImpact_BodyGivesWayToBlow_HoldsInHitstop_ThenRecovers`, `Death_FallsInDirectionOfBlow`, `Run_StanceFootBarelySlides_AndBodyLeansWhenStartingAndStopping`, `Cloth_CapeFlowsInRun_TabardFlapsGiveWayToKnees`, `EnemyVariety_SameArchetype_DifferentLooks`, `Impact_HitstopBothSides_KnockbackAndBodyReaction` (w silniku, z krwią na ostrzu).
- **Nowe zrzuty:** `pose_hitreact`, `pose_death_dirs`, `pose_combo`, `pose_run_side`, `pose_cloth_run`, `pose_variety`.
- **Do oceny w graniu:** długość hitstopu (czy nie „lepi” szybkich serii), siła odrzutu przy grupach wrogów, głośność nowych warstw dźwięku, siła wibracji; wygląd biegu (głębokość ugięcia kolan).

### Sesja 2026-09-29: czytelność odblokowań, dodatek-broń na starcie

- **Ekran odblokowań** jako karty: nazwa + klasa + „budżet N”/„na stałe”, jedna linijka opisu (rodzaj · `UnlockDefinition.description`); pełne parametry pod przyciskiem „Szczegóły ▼” (osobno dla każdej karty) albo „Rozwiń wszystkie szczegóły”. Przycisk „Odblokuj – N popiołu” lub powód blokady w osobnej kolumnie. Po wejściu fokus pada na pierwszy możliwy zakup (`UINavigator.FocusOn`).
- **Błąd:** dodatek z przygotowania (np. Topór bojowy) lądował w plecaku, bo `AddItem(autoEquip)` zakłada tylko do pustego slotu. Teraz `RunState.AddAndEquip` – dodatek w ręku, wyparty przedmiot klasy do plecaka. Test `ExtraWeaponAndShield_AreEquippedAtStart_ClassItemsGoToBackpack`.
- **Przełączniki** (`Tog`: klasa, trudność, dodatki, broń startowa) – zaznaczony jest złoty i pogrubiony (wcześniej stan „wł.” był prawie niewidoczny).

### Sesja 2026-10-22: bohaterowie z Hell Unleashed

Prośba: przenieść bohaterów, ich modele, rig, animacje, bronie i startowe skille z gry „Hell Unleashed” (ten sam autor, `C:\GIT\Hell Unleashed`). Decyzje użytkownika i pełny opis: [PORT_HELL_UNLEASHED.md](PORT_HELL_UNLEASHED.md).

- **Zasoby:**
  - 15 bohaterów: Blender → FBX, T-pose, kości Mixamo, gniazda dłoni, osobny plik klipów;
  - 13 broni;
  - animacje Mixamo;
  - materiały Standard z tekstur 4K;
  - 3 zestawy animacji broni z klatkami faz ciosów z HU.

  Narzędzia: `tools/art/convert_hu_*.py`, `mixamo_concat.py`, `hu_textures.py`; edytor `HeroAssetImport`.
- **Wygląd:**
  - `CharacterVisual.BuildModel(def, scale, RigLook)` – gniazda z kości `HandSlot.R/L` (rotacja w T-pose: ostrze do przodu), łuk w lewej ręce, kostur pionowo w spoczynku;
  - `ItemDefinition.model/modelScale/modelGripShift`;
  - `ClipAnimationDriver` z `WeaponAnimSet` (ruch i ciosy z zestawu, reszta z modelu).
- **Klasy:**
  - `ClassDefinition.parent` (specjalizacja), `tagline`, `trait`, `dodgeStyle`, `signatureSkill`, `startingSkillChoices`, `loadouts` (`StartingLoadout`), `classModifiers`, `lookOverride`;
  - `LoadoutPlan.loadoutIndex/startingSkill/OpeningSkill/GearSkill`, `MetaService.SelectLoadout/SelectStartingSkill`, profil `lastLoadoutIndex/lastStartSkillId`;
  - ekran przygotowania: typy, specjalizacje, cecha, unik, skill klasy, zestawy broni, skill startowy.
- **Walka:**
  - `PlayerCombat.Hero.cs` (klasa częściowa) – skill klasy (`SignatureSlot`), cechy, style uniku, aury (`AuraEffect`), sługi (`Servant`), fala (`WaveEffect`), doskok, przeciążenie, nieśmiertelność, impulsy novy;
  - `Projectile` – przebijanie, rozszczep, pełny wybuch, strefa;
  - `DamageZone` w barwach żywiołu;
  - `HitData.knockback/isProjectile`;
  - hak `EnemyBrain.ReceiveHit → PlayerCombat.OutgoingAgainst` (zasadzka, sokole oko, żądza krwi);
  - nowe staty: Ziemia, wszystkie obrażenia, redukcja obrażeń, szybkość ataku, skrócenie odnowień, kradzież życia, egzekucja;
  - druga ręka: `ItemKind.OffHand` (grymuar, sztylet do rzucania – `grantedSkill`, `RunState.SyncGrantedSkills`), druga broń jednoręczna (ciosy na zmianę).
- **Treść:**
  - `DefaultContent.Heroes.cs` – 23 bronie/przedmioty drugiej ręki, 28 skilli, 12 specjalizacji; trzy dawne klasy nadpisane danymi HU;
  - assety: `SyncContent` (65 nowych) + punktowe `TurrisSetup.OverwriteFromDefaults("class_knight", "class_mage", "class_rogue")`;
  - klasyczne zestawy Rycerz/Mag/Łotrzyk zostały tylko w pamięci jako `legacy_*` do testów mechanik.
- **Wejście:**
  - skill klasy: G / Select;
  - pomoc padem: Select tylko poza rozgrywką;
  - nawigacja padem w poziomie: najpierw ten sam wiersz.

### Sesja 2026-10-22: wolniejsi wrogowie (−10%)

- `BalanceConfig` ma nowe globalne mnożniki `enemyMoveSpeedMultiplier` i `enemyAttackSpeedMultiplier`, oba = 0.9. Oba działają dodatkowo do mnożników faz bossa i statusów.
- `EnemyBrain`:
  - chód, strafe i wycofywanie są wolniejsze o 10%;
  - zamach, cios i powrót (`attackT`) też trwają o 10% dłużej, a czas zbierania energii czaru skaluje się razem z nimi;
  - odstępy między atakami (`attackInterval`), uniki wroga i prędkość pocisków zostały bez zmian.
- Testy: EditMode 146/146, PlayMode (okno) 62/62.

### Sesja 2026-10-22: stopa w biegu, postawa jak w HU, bestiariusz

- **Lewa stopa wykręcona w biegu** (uwaga użytkownika): przeniesienie biegu Mixamo na modele Meshy dawało lewej stopie w podporze −48…−57°
  (spoczynek modelu −13°, w nagraniu prosto; kolano proste). `ClipAnimationDriver.MeasureFootYaw` mierzy dla chodu i biegu do przodu kierunek stóp
  w podporze i co klatkę obraca stopę w kostce o różnicę do pozy spoczynkowej (`FootYawFix`). Test `Run_FeetInStancePointLikeRestPose` (4 bohaterów, ≤15°).
- **Ukryty błąd:** pomiary na klipach przy budowie sterownika widziały pozę spoczynkową (ewaluacja grafu jeszcze nie przesuwa kości) – fazy kroków
  wychodziły zerowe od sesji animacji. Teraz pomiary w drugiej klatce (`measureIn`), poza spoczynkowa – przy budowie.
- **Wojownik trzyma miecz jak w HU:** miecz z tarczą stoi we własnym `Idle_Combat` modelu (`WeaponAnimSet.useModelIdle`, jak `UseCalmIdle` w HU),
  a ostrze w spoczynku ustawia się jak `SwingStyles.Stance` z HU (32° w prawo, 34° w górę; `CharacterVisual.PostPose`). Postawa po ciosie (`combatIdle`)
  dla tego zestawu zbędna. Galeria `WarriorIdleGallery_WhenRequested`. **Do poprawy:** tarcza w tej postawie zasłania twarz i wydaje się większa niż w HU
  (HU układa lewą rękę warstwą `HeroPoseLayer.HoldShield`, której Turris nie ma).
- **Bestiariusz** (menu główne → „Bestiariusz”; `GameUI.Bestiary.cs`, `BestiaryStage`, `Meta/Bestiary.cs`): zakładki „Demony” (HU) i „Wrogowie wieży”,
  siatka kart z portretami 3D (render do tekstur), duży obracający się podgląd, opis, piętro, odmiany żywiołów, liczba zabitych, mocne strony
  (odporności, pancerz, ataki specjalne, fazy bossa) i słabe – odkrywane po 20 zabitych (boss – po pierwszym). Niespotkani – czarna sylwetka i „???”.
  Profil: `ProfileData.bestiary` (spotkani + zabici; warianty liczą się do bazowego). Test `Bestiary_ShowsMetEnemies_WeaknessesAfterKills` (zrzut obejrzany).

### Sesja 2026-10-22: krok przy ciosie, bieg na skos, portale, większe areny

Uwagi użytkownika: „atak mieczem powinien prowadzić do małego kroku naprzód, a nie do wycofania nogi”; „wojownik ma krzywo nogę jak biegnie”;
„przestań testować stare modele”; „powiększ arenę o 20% średnicy; w fali co kilka sekund potwór w różnych miejscach, wychodzi z portalu jak town portal w Diablo, tylko czerwony”.

- **Krok z nagrania ciosu:** `ActionClip.rootForward` (RootT.z z klipu, `HeroAssetImport.RootForward`) – bohater idzie naprzód tyle, ile ciało w nagraniu,
  tylko do przodu (`ClipAnimationDriver.ConsumeAdvance`, `PlayerController`); stały `lunge` broni tylko dla postaci bez modelu.
  **Zakotwiczenie stóp** (`AnchorFeet`): stopa na ziemi nie jedzie do tyłu przy wejściu w cios ze spoczynku i przy wygaszaniu akcji – bohater robi wtedy
  krok naprzód, druga noga się dostawia. Ruch nadany przez sterownik nie włącza chodu (`selfSpeed`).
- **Klipy cięć:** miecz z tarczą SlashLeft = „slash (5)” (dawny „slash (3)” cofał wykroczną nogę o 0,7 m); dwuręczny SlashLeft = „slash (4)”, Thrust = „great sword attack”.
- **Postawa bojowa** (`WeaponAnimSet.combatIdle`, miecz z tarczą: „idle (4)”, tarcza w pionie dzięki chwytowi na przedramieniu): 4 s po ciosie/parowaniu –
  kolejny cios bez wykroku (krok 0,3–0,7 m); potem swobodne stanie (tylna noga dostawia się naprzód). Blok nie przełącza postawy.
- Test `SwordAttack_StepsForward_NoFootRetreats`: cofnięcie stóp ≤ 0,11 m (było 0,5 m), krok w walce ≤ 0,8 m (dwuręczny „slash (4)” ~1,3 m – nagranie).
- **Bieg na skos** (błąd z sesji animacji – mieszanie biegu w przód i bokiem krzyżowało kolana): strefy kierunków – na skos jedno nagranie (przód/tył),
  biodra i nogi obrócone w stronę ruchu, kręgosłup i klatka oddają obrót (`LegTwist`, maks. 65°); bokiem tylko blisko 90°. Galeria `RunGallery_WhenRequested`.
- **Testy na modelach FBX:** `Tests/PlayMode/TestContent.cs`; testy riga proceduralnego gracza przepisane na model. Przy okazji: drugi nóż w lewej dłoni
  modelu FBX przy nożach bliźniaczych (brakowało).
- **Areny ×1,2 średnicy:** Dziedziniec 17,5, Krypta 29,5, Szczyt 22,8 (kolumny przesunięte proporcjonalnie) – w kodzie i w assetach.
- **Portale w każdej fali:** fale bez bossa nie pojawiają się naraz – wrogowie wychodzą po jednym co 2,5 s (`GameRoot.StreamInterval`) z portali w losowych
  miejscach (≥ 7 m od bohatera); bossowie wchodzą od razu. **`DemonPortal`**: stojący czerwony owal (wir `FxMaterials.Portal`, jasna obwódka, iskry, światło,
  blask na ziemi), otwiera się od szczeliny, wróg wychodzi krokiem w stronę bohatera, portal się zamyka; elita – złoty. Galeria `PortalGallery_WhenRequested`.

### Sesja 2026-10-22: demony z pierwszych trzech aren Hell Unleashed

Prośba: „przenieś z hell unleashed wszystkie potwory, które pojawiają się na pierwszych trzech arenach”. Decyzje użytkownika:
piętra I–III jak w HU; **małe grupy wychodzą z portali w różnych miejscach przez cały czas piętra – trzeba je szybko zabijać, bo robi się tłoczno**.

- **Skład (HU fale 1–3):** fala 1 – chochlik; fala 2 – chochlik, kultysta, czerw; fala 3 – boss `enemy.boss` (Władca Bramy, model gatelord) i jego sługi (ożywione nietoperze).
- **Modele:** `tools/art/convert_hu_demons.sh` → `Models/Demons/<nazwa>/` (Blender: `convert_hu_demon.py`, rig **Generic** – szkielety różne: skrzydła, ogony, czerw;
  tekstury wprost z .glb: `hu_demon_textures.py`; takty ciosu `<nazwa>.beats.json.txt`). Importer: `DemonAssetImport` (menu „Turris/Demony z Hell Unleashed…”,
  `ApplyContent` = import + `SyncContent` + `OverwriteFromDefaults("tower_main")` + przypisanie wyglądów). Postprocesor modeli: folder `Demons/` = Generic.
- **Animacja:** `DemonAnimationDriver` – zamach prowadzony jak w HU (`DriveAttackClip`: wyhamowane wznoszenie do szczytu, trzymanie, przyspieszenie do klatki ciosu
  w ostatnich 0,07–0,2 s zapowiedzi), potem klip gra sam; przenikanie klipów, odchylenie ciała od ciosu (sprężyna), spłaszczenie przy własnym ciosie.
  `CharacterAnimState.windupSeconds`, `AnimAction.Latched`, `AttackAnim.Special/Latch`.
- **Liczby:** zdrowie i obrażenia HU ×2,5 (chochlik 55 PŻ = 2 cięcia mieczem), odległości/prędkości px/24, czasy bez zmian. Chochlik zawsze w żywiole
  (ogień = model imp, mróz = imp_water, błyskawica = imp_air, ziemia = imp_earth; 60% ciosu w żywiole, odporność własna, słabość przeciwna).
  Złote elity HU: co 20–30 demonów z portalu, ×7 zdrowia, ×1,6 obrażeń, znają specjały (chochlik – dwa cięcia, kultysta – wachlarz 5 kul, czerw – wgryzienie).
- **Portale** (`FloorDefinition.portals`, `GameRoot.Portals.cs`): czas/budżet/odstęp grup/wagi jak fale HU 1–2 (75 s/30, 78 s/44), grupy 1–2 (czerw – stado 4),
  portal ≥7 m od bohatera, demony wychodzą po 1,1 s (`FxLibrary.Portal`), limit 30 żywych; HUD: „Portale: N s” → „Dobij pozostałe demony”.
- **Ataki specjalne** (`EnemySpecial`, `EnemyBrain.Specials.cs`): wgryzienie (czerw jedzie na bohaterze, −20% prędkości, gryzie co 1 s z krwawieniem, strząsa unik; maks. 3),
  szarża po zapowiedzianej linii z płonącym śladem i wybuchem, kręgi ognia, piekielne kule z płonącą ziemią, skok z uderzeniem w krąg, przywołanie sług
  (`EnemyBrain.SummonRequested`, sługi giną z przyzywającym). Władca Bramy: 2500 PŻ, od 50% wezwanie 6 nietoperzy i nowe ataki, od 30% szał (tempo ×1,35);
  krok serii „tylko w szale” pomijany wcześniej (`minPhase` w łańcuchu).
- **Wieża:** I. Wrota Otchłani, II. Kult Bramy, III. Władca Bramy (odpoczynek po nim), IV. Zbrojownia, V. Brama Strażnika, VI. Szczyt Wieży. Dawne pojedynki z ghulem
  i heretykiem zniknęły jako osobne piętra (ci wrogowie są w falach IV–V).
- **Znalezione przy okazji:** `convert_hu_hero.py` ustawia 30 kl./s **po** imporcie glb – klipy Meshy bohaterów (Idle_A, Walking_A, Cast…) mogą grać o 25% za szybko. Nie poprawione.

### Sesja 2026-10-22: animacje walki i ruchu bohaterów, reakcje wrogów

Prośba: lepsze animacje walki i chodu bohaterów, płynne i zgodne z anatomią; widoczna siła ciosu w reakcji wroga;
przejścia między animacjami; „dziwnie stoi nowy wojownik”. Praca na **nowych modelach FBX z HU** (`ClipAnimationDriver`);
wrogowie zostali proceduralni (migracja objęła tylko bohaterów), więc ich reakcje są w `ProceduralHumanoidAnimator`.

- **Przejścia (`ClipAnimationDriver`):** każda warstwa akcji ma mikser z dwoma wejściami (bieżący i zamrożony poprzedni klip) –
  kolejny cios serii, ten sam klip od nowa, unik, drgnięcie i powrót do ruchu przenikają się po krzywej gładkiej.
  Czas przenikania zależy od **odległości póz dłoni** (próbna ewaluacja grafu): średnio do 7 m/s (akcje wymuszone – trafienie,
  unik, parowanie, śmierć – 8 m/s), maks. 0,26 s / 0,18 s. Wcześniej: twarde przełączenie, a przy trafieniu w czasie ciosu
  dłoń przeskakiwała 0,53 m w jednej klatce.
- **Tempo ciosu:** klipy Mixamo są ~2× dłuższe niż ciosy Turris. Faza aktywna obejmuje w klipie tylko przejście ostrza
  (`StrikeEarly` 0,1, `StrikeShare` 0,6 rozpiętości), powrót sięga 75% reszty klipu, dalej przenikanie do gardy (0,24 s).
  Szczyt prędkości dłoni ~25 m/s zamiast ~35, powrót wolniejszy od cięcia. Łuk i pociski czarów – bez przesunięć (wystrzał na granicy faz).
- **Klipy akcji „w miejscu”** (`HeroAssetImport.InPlace`): ciosy, parowanie, czary, drgnięcia – wpieczony krok wysuwał ciało
  do 0,5 m przed postać, a przejście ściągało je z powrotem. Śmierć zostaje z przesunięciem.
- **Ruch:** mieszanie 2D (przód/tył/lewo/prawo, cosinusy) chodu i **biegu w tył i bokiem** (nowe pola `WeaponAnimSet.runBack/runLeft/runRight`:
  miecz z tarczą `run (2)`, `strafe (3)/(4)`; dwuręczny `run`, `strafe (3)/(4)`; łuk `run back/left/right`). Prędkość nagrania każdego klipu
  z ruchu korzenia (`averageSpeed`). **Faza kroków wyrównana**: dla każdego klipu chwila, w której lewa stopa najbardziej wyprzedza prawą
  w kierunku ruchu (pomiar przy budowie sterownika) – nogi nie krzyżują się przy biegu po skosie.
- **Ruch wtórny bohatera (`BodyDynamics`, nowy plik):** reakcja na trafienie (sprężyny tułowia i głowy jak u proceduralnych postaci;
  głowa z bezwładnością), pochylenie przy ruszaniu/hamowaniu i w zakrętach. Zgięcie rozłożone na lędźwie/klatkę/górną klatkę
  (30/35/35%), kąty ograniczone; biodra i nogi z klipu – stopy nie odjeżdżają. `ClipAnimationDriver` implementuje `IImpactReactive`.
- **Spoczynek wojownika:** dawny `sword and shield idle (4)` to szeroki przysiad z tarczą trzymaną płasko. Pozostałe idle z paczki są złe
  (bez numeru – obrót o 90°, (2)/(3) – broń odrywa się od dłoni, `Idle_Combat` z modelu – przysiad). Teraz `Pro Longbow Pack/unarmed idle 01`
  (swobodne stanie); miecz przy opuszczonej ręce opada sztychem w dół (`CharacterVisual.PostPose`, tylko poza walką).
- **Tarcza na przedramieniu:** `WeaponAnimSet.hasShieldGrip/shieldGripPosition/shieldGripEuler` (wyliczone z pozy bloku, jednakowe dla
  wszystkich bohaterów – gniazdo wyrównane w T-pose), węzeł `ShieldGrip`. Przy przedramieniu uniesionym – lico do przodu, góra do góry;
  przy ręce opuszczonej tarcza płynnie przechodzi do zawieszenia pionowo przy boku (`CarryShield`). Wcześniej tarcza obracała się jak ostrze
  (płasko „jak taca” w gardzie i biegu). **Poprawka po uwadze użytkownika:** tarcza jest teraz liczona co klatkę z kierunku przedramienia
  (`CarryShield`), a nie ze stałego kąta względem dłoni – przedramię leży na tarczy, a przy przedramieniu poziomym tarcza stoi w pionie
  (zgięcie nadgarstka jej nie przechyla); test `Shield_StrappedToForearm_UprightWhenForearmLevel`. Kostur: poprawka liczona od chwytu z dłoni (wcześniej kumulowała się między klatkami).
- **Wrogowie – reakcja zależna od ciosu:** `PoseLibrary.Stagger` zastępuje symetryczne `Flinch`: tułów odchyla się od napastnika
  (z boku – skręt i przechył), szybkie narastanie (~0,1 s), krótkie łapanie równowagi, gładki powrót; ciężar ciosu pogłębia zgięcie,
  ugina kolana, poszerza postawę; ramiona z bezwładnością; głowa – broda do piersi (bez „limbo”, tułów najwyżej ~30°).
  Nogi kroczą razem z odrzutem. `EnemyBrain`: ciężar = `ImpactDirector.Weight` (ten sam co odczucie ciosu), przełamanie postawy ≥ 0,8,
  przełamanie gardy ≥ 0,9, odbity cios (parowanie) – pchnięcie do tyłu. Przenikanie póz przy przerwaniu akcji – gładka krzywa.
- **Nowe testy:** `Transitions_Crossfade_NoPoseJump`, `Strike_HandSpeed_HumanlikeProfile`, `Locomotion_DirectionalRunClips`,
  `HeroBody_GivesWayToBlow_ThenRecovers` (HeroModelPlayTests), `Stagger_FollowsBlowDirectionAndWeight_LegsStepWithKnockback` (AnimationPlayTests);
  `ClipDriver_ScrubsActionClipByCombatPhase` dostosowany do nowego rozkładu czasu.
- **Weryfikacja wzrokowa:** zrzuty renderowane w edytorze przez Unity MCP (wojownik: spoczynek, chód, bieg, bieg bokiem/w tył, blok, seria ciosów,
  trafienie; wróg: zachwianie lekkie/ciężkie/z boku). Assety `Content/Visuals/anim_*.asset` przebudowane `HeroAssetImport.Run()`.
- **Blok – noga do tyłu (uwaga użytkownika):** w miejscu stopy stoją (7 cm za biodrami); wyrzut nogi brał się z cofania w bloku
  (55% prędkości ≈ 2,9 m/s), gdzie dołączał klip biegu w tył (stopa 53 cm za biodrami). W gardzie (blok, picie) nie ma biegu: chód
  szybciej (do 2,7×), a powyżej nagranej prędkości chodu do 20% domieszki postawy w miejscu z proporcjonalnie szybszą kadencją –
  krótsze kroki bez ślizgania (35 cm). Testy `Block_Backpedal_ShortSteps`, `Block_UpperBody_FeetStayWhereLocomotionPutsThem`.
  Parowanie (pełny klip `sword and shield block`) ma szeroki wykrok – 69 cm – bez zmian, do oceny.
- **Blok z boku (uwaga użytkownika):** w bloku bohater obraca się szybko (`balance.guardTurnSpeed` 1800°/s) do wroga, który się
  zamachuje lub uderza (najbliższy w zasięgu 10 m, faza aktywna przed zamachem – `PlayerController.IncomingThreat`, `EnemyBrain.AttackPhase`).
  Cios w blok z kąta do `balance.guardAutoTurnAngle` (120°) od przodu jest blokowany i zleca obrót do napastnika (`PlayerCombat.GuardFace`);
  cios w plecy przechodzi. Testy `Block_SideHit_TurnsToAttackerAndBlocks_BackHitStillLands` (EditMode), `Block_TurnsQuicklyToAttackerFromSide_AndBlocks`
  (w silniku, ghul z boku: w chwili ciosu bohater stoi przodem, cios zablokowany).
- **Do oceny w graniu:** czy spokojny spoczynek pasuje (alternatywa: postawa bojowa w czasie walki), długość przejść między ciosami,
  siła reakcji wrogów; klipy dwuręczne i łuku – postawa w spoczynku nie była zmieniana.

## 3. Jak pracować z projektem (ważne dla kolejnej sesji)

- **Unity MCP (edytor otwarty):** kompilacja (`refresh_unity`), testy (`run_tests` – EditMode i PlayMode), kod w edytorze (`execute_code`, tylko C# 6 – CodeDom)
  i zrzuty renderowane kamerą do PNG. Zrzut `manage_camera screenshot` zapisuje plik w `Assets/Screenshots` – usuwać po obejrzeniu.
- **Testy i kompilacja z CLI wymagają zamkniętego edytora.** Gdy projekt jest otwarty, Unity kończy się od razu z kodem 1. Trzeba poprosić użytkownika o zamknięcie edytora. Otwartość można sprawdzić przez `tasklist | grep -i unity.exe`.
- **Kompilacja = uruchomienie testów EditMode** (`python tools/run_tests.py EditMode`). Błędy `error CS` są wypisywane z logu.
- **Weryfikacja wyglądu:** `TURRIS_SHOT_DIR=<katalog> python tools/run_tests.py PlayMode gfx` zapisuje PNG. Galerie:
  - `pose_*` — pozy i zbliżenia postaci;
  - `arena_*` — areny;
  - `fx_*` — efekty;
  - `0_menu` … `3_boss` — gra.

  Zrzuty trzeba obejrzeć przed ogłoszeniem efektu. Kilka błędów wizualnych wyszło dopiero na nich.
- **Dźwięk do odsłuchu:** `TURRIS_SOUND_DIR=<katalog> python tools/run_tests.py EditMode` zapisuje wszystkie efekty i utwory jako WAV. Głośność względną warto mierzyć (np. numpy/scipy: RMS K-ważony w oknie 400 ms), bo normalizacja jest szczytowa.
- **`OnGUI` nie działa w trybie `-batchmode`**, także z grafiką. Testy interfejsu (`GamepadPlayTests`) uruchamiaj w trybie `window`.
- **Regeneracja treści:**
  - `-executeMethod Turris.EditorTools.TurrisSetup.RunBatch` tworzy brakujące assety, scenę i materiały efektów;
  - `RunBatchForce` **nadpisuje wszystkie assety treści** wartościami z `DefaultContent`, a zmiany użytkownika w inspektorze przepadają. Używaj tylko świadomie.

  Nowe pola w SO dodawaj z wartością domyślną `Auto`, żeby istniejące assety działały bez regeneracji.
- **Długie skrypty edycyjne:** w narzędziu Bash długi heredoc z Pythonem potrafi się wysypać („unexpected EOF”). Lepiej zapisać skrypt `.py` w katalogu tymczasowym i go uruchomić. Uwaga na `\b` w stringach Pythona: bez prefiksu `r` to znak backspace, a nie granica słowa w regex.
- **Klasy `ScriptableObject` i `MonoBehaviour` serializowane w assetach** muszą mieć plik o tej samej nazwie co klasa.
- **Bohaterowie z Hell Unleashed** (zasoby i treść):
  1. `bash tools/art/convert_hu_heroes.sh` – Blender, ok. 1 min.
  2. `Unity -batchmode -executeMethod Turris.EditorTools.HeroAssetImport.RunBatch` – materiały, zestawy animacji, wyglądy.
  3. `HeroAssetImport.ApplyContentBatch` – SyncContent + nadpisanie trzech klas + przypisanie wyglądu i modeli.

  Pierwszy import tekstur 4K trwa kilka minut.

## 4. Pułapki techniczne już rozwiązane

- `InputAction.expectedControlType` ustawia się właściwością, bo `AddAction` nie ma takiego parametru w Input System 1.20.
- `CharacterController.minMoveDistance = 0`, inaczej przy wysokim FPS postacie stoją w miejscu.
- `FindFirstObjectByType` i `FindObjectsByType(SortMode)` są przestarzałe w 6.6; w projekcie używane są rejestry statyczne oraz `FindAnyObjectByType`/`FindObjectsByType(FindObjectsInactive)`.
- Zmiana stanu w IMGUI w trakcie przebiegu psuje GUILayout, dlatego akcje przycisków są kolejkowane i wykonywane na końcu `OnGUI` (`UINavigator.Enqueue`).
- `renderer.material` w EditMode tworzy wycieki i ostrzeżenia. Podświetlenia działają przez `MaterialPropertyBlock` (`TintSet`), a efekty poza trybem gry zwracają `null`.
- Wybór „słońca” w `GameRoot` obejmuje tylko `LightType.Directional`, bo światła pochodni są niszczone przy zmianie areny.
- Otwarte bryły obrotowe (szaty, rękawy, kaptur) są dwustronne (`ProcMesh.Lathe(doubleSided)`), inaczej od środka znikają.
- **`WaitForEndOfFrame` nigdy nie nadchodzi w `-batchmode`** – test czekający na niego wisi bez końca (tak zawiesił się przebieg `gfx`). Zrzuty z interfejsem (`UiShot`, `ScreenCapture`) robi się tylko w trybie `window`; w trybie wsadowym są pomijane.
- `GetComponent<T>() ?? AddComponent<T>()` nie działa w edytorze (brakujący komponent to „fałszywy null”) – trzeba jawnego `if (x == null)`.
- Kolejna gra (np. następny test) tworzy nowy `AudioService`, zanim poprzedni zostanie zniszczony – nowa instancja przejmuje `Instance` zamiast niszczyć siebie. Bank klipów jest statyczny (synteza raz na uruchomienie).
- Scalanie części (`PartBuilder.BakeAll`) łączy tylko bezpośrednie dzieci z `RigPart`. Obiekty przełączane (flaszka) i przestawiane (gniazda broni) muszą być osobnymi węzłami.
- **Po testach PlayMode w oknie edytor zostaje z pustą sceną tymczasową** i przy następnym starcie otwiera „Untitled” – Play pokazuje wtedy samo niebo. `MainSceneAutoOpen` (edytor) otwiera wtedy `Scenes/Main.unity` (nie w trybie wsadowym ani z `-runTests`).
- **Klipy ruchu Mixamo nie są „w miejscu”** – biodra idą do przodu (bieg ok. 2,7 m na cykl). Z „Bake Into Pose” XZ (`lockRootPositionXZ`) ciało odjeżdża od postaci i co pętlę wraca skokiem (szarpanie biegu). Pętle i uniki importuje się z `lockRootPositionXZ = false` (`HeroAssetImport.Loop/Roll`) – ruch korzenia jest wyjmowany i odrzucany, bo `applyRootMotion = false`. Modele Meshy mają tylko chód; bieg bez zestawu broni (mag) pochodzi z paczki miecza i tarczy. `ClipAnimationDriver` prowadzi klipy ruchu wspólną fazą kroków; test `HeroModelPlayTests.Run_BodyStaysOverCharacter_LegsStride`.
- **Eksporter FBX Blendera z animacjami zapisuje jako pozę domyślną klatkę pierwszego klipu** (u bohaterów – pozę bloku). Unity buduje z niej awatar Humanoid i wszystkie klipy dostają przesunięte ręce. Dlatego model jest eksportowany bez animacji (czysty T-pose), a klipy w osobnym `<nazwa>_clips.fbx` z awatarem skopiowanym z modelu (`CopyFromOther`).
- **Animator z domyślnym `cullingMode` nie nakłada pozy z ręcznie ewaluowanego grafu Playables, gdy postaci nie widzi kamera** (testy, postać poza kadrem). Pozostaje wtedy poza z pliku. `CharacterVisual.BuildModel` ustawia `AnimatorCullingMode.AlwaysAnimate`. Wyłączony Animator w ogóle nie przyjmuje pozy z grafu.
- **Klipy akcji Mixamo z wpieczonym ruchem korzenia** (`lockRootPositionXZ`) przesuwają ciało względem postaci – ciosy i drgnięcia importuje się „w miejscu” (`InPlace`).
- **`sword and shield idle (2)/(3)` odrywają broń od dłoni** (gniazda `HandSlot` nie nadążają) – nie używać jako spoczynku.
- Zapis spakowanego obrazu z `.glb` w Blenderze: `img.save(filepath=...)` (sam `filepath_raw` + `save()` nic nie zapisuje). Eksport Meshy zawiera pomocniczą kulę „Icosphere” (2 m) – trzeba ją usunąć.

## 5. Kolejne kroki (propozycja priorytetów)

0. **Demony z HU – ocena w graniu (użytkownik):** tłok na piętrach I–II (budżet, odstęp grup, limit żywych), zdrowie Władcy Bramy (2500), czytelność portali;
   brakuje dźwięków własnych demonów (używane są dźwięki Turris) i efektów ciała z HU (`DemonFx` – krople, całun, żar).
   Rozważyć poprawkę kl./s w `convert_hu_hero.py` (klipy Meshy bohaterów).
0. **Bohaterowie z HU – ocena w graniu (użytkownik).** Do sprawdzenia:
   - balans: obrażenia skilli ×2,5, zdrowie klas, mistrzostwa;
   - czy wygląda chwyt broni dwuręcznej i tarczy;
   - tempo ciosów Mixamo względem faz walki Turris.

   Braki względem HU: IK drugiej ręki na drzewcu (chwyt tarczy na przedramieniu jest – sesja animacji), cięciwa łuku, lustrzane ciosy lewą ręką, wybór wyglądu niezależny od specjalizacji, rezerwa many Pobożności, poziomy III skilli (druga fala Cięcia wichru, Podpalenie aury świętego ognia).

1. **Sesja ręcznego grania** (użytkownik, pad i klawiatura). Do oceny: odczucie ciosu (hitstop, odrzut, wibracje, nowe dźwięki), sterowanie i kamera, czytelność telegrafów i efektów, balans pięter. Poprawki parametrów w `GameConfig → balance` i assetach wrogów.
2. **Pomiar wydajności.** Profiler, FPS w krypcie (12 świateł, 44 systemy cząsteczek). W razie potrzeby:
   - ustawienie jakości (liczba świateł i cząsteczek);
   - łączenie świec w jedno światło;
   - LOD dla panoramy szczytu.
3. **Dźwięk – ocena na słuch** (użytkownik): poprawki barw i głośności w `SoundSynth`/`SoundInfo`; w razie potrzeby podmiana wybranych efektów na nagrania w `Resources/Audio/`.
4. **Wariant B na prawdziwych plikach z Mixamo.** Sprawdzić:
   - kreator `CharacterModelTools`;
   - maskę górnej połowy ciała;
   - położenie broni w dłoni;
   - granice faz `windupEnd` i `activeEnd`.
5. **Treść:** podejście do docelowych 20–30 min (więcej pięter i aren), więcej wrogów i ataków, przeciwnicy parujący, NavMesh albo omijanie filarów.
6. **Interfejs docelowy** (UI Toolkit), zmiana przypisań przycisków, wibracje pada.
7. **Przed wydaniem:** usunąć deweloperskie F9 (+100 popiołu), sprawdzić build (materiały efektów z `Resources`, shadery `Turris/*` i URP Lit z `Shader.Find`).

## 6. Preferencje użytkownika

- Komunikacja i dokumentacja **po polsku**.
- Hell Unleashed jest **wyłącznie źródłem do odczytu**. Migracja ma dodawać/zmieniać pliki tylko w Turris, bez modyfikacji ani usuwania plików HU.
- Treści z Hell Unleashed (jego własnej gry) przenosi „tak jak tam jest”: oryginalne tekstury 4K, typy bohatera + specjalizacje, Turris ma dostosować się do mechanik HU.
- Oczekuje wysokiej szczegółowości wizualnej; kolejne rundy prosiły o „×4–5”, a potem „×2”. Mierzalne liczby (części, renderery) i zrzuty pomagają pokazać postęp.
- Chce pełnej obsługi pada.
- Przy decyzjach architektonicznych wybrał podejście łączące: szybki efekt teraz, a jednocześnie gotowość na lepsze rozwiązanie później.
