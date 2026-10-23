# Efekty żywiołów – etap AAA (dziennik)

Cel: czytelne, warstwowe efekty dark fantasy na poziomie Path of Exile 2. Podstawa: URP 17.6 + Linear + HDR/Bloom/ACES
([MIGRACJA_URP.md](MIGRACJA_URP.md)). Receptury cząsteczek HU (`HellParticleRecipes.cs`) są generowane i weryfikowane
przez `tools/art/import_hu_vfx.py --verify` – **nie edytujemy ich**; ulepszenia idą przez adapter silnika
(`HellParticles`, `HellVfxMaterials`) i warstwy Turris nad efektami HU.

## Narzędzie oceny

`HellFxPlayTests.BurstSequence_WhenRequested` (z `TURRIS_SHOT_DIR`): kamera zbliżona do rozgrywki, podłoga z koliderem,
oświetlenie jak w grze; dla Fire/Ice/Air/Earth wybuch `HellFx.Impact` (promień 2,2) w chwilach 40 ms … 3,5 s
(`seq_{żywioł}_{ms}.png`) oraz błyskawica z nieba + łańcuch (`seq_Bolt_*`, 30 … 700 ms).

## 2026-10-23 – krok 1: uber-shader cząsteczek, ogień, błyskawica

**Uber-shader `Turris/HellParticle`** (ten sam plik i GUID; przy zerowych parametrach wygląd jak dawniej):
- jądro HDR – `_HotBoost`/`_HotPower` wzmacniają najjaśniejsze miejsca tekstury ponad 1 (łapie je bloom);
  wzmocnienia trzymane nisko (szczyt ~1,5–3): przy wyższych ACES wybielał jądro ognia do kremowej bieli;
- erozja – Custom1.x (krzywa: cała do 45% życia, potem rozpad) × `_Erosion`, próg szumu `noise_tile`:
  płomienie, dym i mgła rozpadają się na strzępy zamiast równo blednąć;
- falowanie UV szumem (`_Flow`), płynny flipbook (UV2 + AnimBlend), losowe przesunięcie (StableRandomX);
- miękkie przecięcie z podłożem (`_SoftFade`, metry, z `_CameraDepthTexture`).
- Kontrakt strumieni: `HellVfxMaterials.ApplyStreams` (Position, Color, UV, Custom1XY, UV2, AnimBlend, StableRandomX) –
  wywoływany w `HellParticles.Build` i `FxLibrary.Particles` dla każdego renderera z tym shaderem.
- Profile per tekstura w `HellVfxMaterials.looks` (`fireball_sheet`, `flame_sheet`, `fire_puffs_atlas`, `embers_atlas`,
  `spark`, `smoke`, `glow`, `frost_mist`, `frost_rime`, `electric_sparks`, `lightning_arcs`, wariant `glow_hot`);
  `HellParticleLayer.Look` wybiera wariant bez nowej tekstury. Eksplozje flipbookowe zwalniają pod koniec.

**Naprawa URP:** renderer miał domyślnie `Copy Depth Mode = After Transparents`, więc przezroczyste shadery
nie widziały głębi (miękkie cząsteczki z migracji i `_SoftFade` nie działały). Ustawiono `After Opaques`;
pomiar: profil jasności quadu przy podłodze 0 → 1 (ostro) zmienił się na 0,61 → 0,91 → 1.

**Ogień (`HellFx.FireAccent`, wywoływany z `HellFx.Burst` dla `HellFxElement.Fire`):**
- błysk HDR w punkcie uderzenia (0,16 s, `glow_hot`, ciepła barwa);
- ślad na ziemi od promienia 0,6: zwęglenie ~2 promienie (6 s) i żarzące się pęknięcia ~1,2 promienia (1,6 s),
  stygnące od żółtopomarańczowej do ciemnoczerwonej (`Turris/FxScorch`, `HellVfxMaterials.Scorch`);
  podłoże = najniższe trafienie raycastu w dół (pomija kolidery postaci).
- Sekwencja: 40–250 ms błysk i płomienie, 500 ms żar w pęknięciach, 1 s stygnięcie, 2–3,5 s ciemny ślad.
  Dolna krawędź wybuchu przy podłodze miękka (wcześniej ostre cięcie).

**Błyskawica (`LightningBolt`, przez `FxLibrary.Lightning` – łańcuch, uderzenia z nieba, wiązki wrogów, burza):**
- kanał z przemieszczeniem punktów środkowych (5 poziomów, ≤33 punkty), 0–3 odgałęzienia;
- biały rdzeń HDR (0,085 m) + niebieska otoczka (0,42 m) na `HellVfxMaterials.Beam` (profil wiązki, uber-shader);
- ponowne wyładowania co ~45 ms przez 60% życia (0,28 s), gaśnięcie z migotaniem;
- uderzenie z nieba (różnica wysokości > 4 m) zostawia biało-niebieski żar stygnący do fioletu i ciemny ślad.

**Wydajność** (edytor, scena piętra I, 1920×1080, 40 renderów, rundy 2–3): bez efektów 2,2 ms, z 6 równoczesnymi
wybuchami ognia (~900 cząsteczek, 12 śladów) 2,4–2,5 ms → ~+0,3 ms.

## 2026-10-23 – krok 2: lód, ziemia, pule, drganie powietrza

**Lód (`HellFx.IceAccent`, z `HellFx.Burst` dla `HellFxElement.Ice`):**
- zimny błysk (`glow_hot`, błękit);
- wieniec kryształów z siatek (`IceCrystals.Ring`: 4–12 sześciobocznych igieł, płaskie fasety): wzrost 0,12 s
  z przestrzeleniem, utrzymanie ~0,8 s, rozpad 0,38 s z jasną krawędzią i lekkim zapadaniem (`Turris/FxIce`:
  fasety oświetlone słońcem, ciemny granat w cieniu, fresnel, odblask, wewnętrzna poświata);
- szron rozrastający się po ziemi do ~2,2 promienia (`Turris/FxFrost`: gęstszy front, cienkie żyłki, drobne iskrzenie;
  druga próbka szumu obrócona, bez widocznego kafla) i mgła płożąca się na boki (`HellFx.FrostMist`);
- podmuch mrozu (`FxLibrary.FrostCone`): kryształy rosnące z odległością w stożku (`IceCrystals.Arc`) i szron.
- Pierwsza wersja kryształów była przejaśniona (słońce 2 × jasne fasety + ACES = blady cyjan) – kontrast podniesiony.

**Ziemia (`HellFx.EarthAccent`, od promienia 1):**
- ściana pyłu (`ShockRing`: pochylony walec, rozchodzi się z wyhamowaniem do 1,15 promienia, `Turris/FxShockRing`);
  drobne próbki szumu dawały regularny „splot” – zostawiono jedną modulację niskiej częstotliwości;
- pęknięcia promieniste z kraterem i stygnącym żarem w szczelinach (`Turris/FxCracks`; nierówna liczba promieni,
  ziarno z pozycji obiektu – każde uderzenie inne); kurz `SlamDust` (bez podwójnego wywołania w `Shockwave`).

**Pule:** błyski (`HellFxPool.OneShot`, bufor cykliczny 6 per klucz), ślady (`FxLibrary.TimedDecal` – tylko decale
bez referencji u wywołującego), `LightningBolt`, `ShockRing`, `IceCrystals` (stosy, ponowne użycie dzieci).
Pomiar: po drugiej fali 18 wybuchów + 6 błyskawic liczba obiektów bez zmian (36 decali, 6/6/6), wszystkie ponownie użyte.

**Drganie powietrza:** Opaque Texture włączona w `TurrisURP` (2× bilinear); `Turris/FxDistort` (kolejka 2998)
przesuwa próbkę obrazu szumem w masce dymu; wariant `heat` w `HellVfxMaterials.Sprite`. Nad wybuchem ognia
(`FireHeat`) i stale nad płonącą ziemią (`HU_GroundHeat`). Pomiar na szachownicy: 198/25600 pikseli zmienionych,
średnia różnica 0,063 (na jednolitej podłodze w galeriach efekt jest niewidoczny – ocenić w grze).

**Testy:** EditMode 156/0/1; PlayMode 98 + 17 galerii pominiętych; niezaliczone dwa znane testy zależne od tempa
klatek oraz `AudioPlayTests.Sounds_FollowCombatAndFlow…` przy stanie statycznym audio z poprzedniego przebiegu
(po przeładowaniu domeny przechodzi – sprawdzone na kodzie kroku 1 i 2). `import_hu_vfx.py --verify`: OK.

**Wydajność** (edytor, piętro I, 1080p): bez efektów 2,35 ms; 18 równoczesnych wybuchów (6 × ogień, lód, ziemia)
+ 6 błyskawic, ~1860 cząsteczek: 4,65 ms (+2,3 ms, najgorszy przypadek).

**Do zrobienia w kolejnych krokach:**
- ocena w grze przy wielu wrogach (czytelność, ilość bloomu, drganie powietrza na teksturowanym tle);
- statusy na postaciach (podpalenie, zamrożenie – lodowa skorupa z FxIce, porażenie) i pociski (głowa HDR, smugi);
- telegrafy czarów wrogów w nowym stylu, VFX Graph dla gęstych pól (burza, płonąca ziemia na dużym obszarze);
- kompresja tekstur VFX (BC7/BC4, 512²) i rozgrzewanie pul przy wczytaniu piętra.
