# Naprawa animacji berserkera z toporem – notatki (co, jak, dlaczego)

Stan na 2026-10-31. Szczegółowe liczby, wyniki testów i historia wersji są w [PROGRESS.md](PROGRESS.md)
(sekcja „Naprawa animacji berserkera”). Ten plik zbiera wnioski przydatne przy kolejnych naprawach.

## 1. Punkt wyjścia

- Berserker (`class_hu_berserker`, wygląd `visual_berserker_anime`, model `Models/PoC/BerserkerAnime/berserker_anime.fbx`)
  nie ma własnych klipów. Z toporem i młotem gra wspólny zestaw `anim_greatsword` (Mixamo „Great Sword Pack”, Humanoid).
  Każda zmiana klipu dotyczy więc wszystkich wyglądów z bronią dwuręczną.
- `ClipAnimationDriver` gra klipy przez Playables **bez root motion z Animatora**. Ruch postaci pochodzi z krzywej `rootForward`.
- Na klip nakładane są warstwy proceduralne, w tej kolejności:
  1. `GreatSwingPose`: profil ciosu, postawa topora, prostowanie tułowia, stopy;
  2. `HellEquipmentPose`: IK lewej dłoni na trzonku, rozstaw dłoni;
  3. poprawki stóp.
  Przy każdym problemie najpierw trzeba ustalić, **która warstwa** go powoduje. Audyt `BerserkerStanceAuditTests` mierzy pozę po każdej warstwie.
- Zasada użytkownika: usterki mierzalne (dłoń poza trzonkiem, broń w podłodze, wykręcony staw) poprawiamy od razu. Zmianę charakteru
  zaakceptowanego ruchu (np. usunięcie obrotu) najpierw pokazujemy i pytamy. Lekcja: „ciężki 2 był dobry, a ty go zmasakrowałeś” –
  wrócił do oryginału.

## 2. Narzędzia (do ponownego użycia)

| Narzędzie | Do czego |
|---|---|
| `ArtSource/AnimationRepair/lib/repair_swing.py` | Biblioteka napraw w Blenderze (bake z IK): `time_map`, `root_offset`, `foot_target`, `grip_span/slide/reach`, `feet_follow`, `arms_from`, `hands_ik`, `torso_roll`, `head_counter`… |
| `ArtSource/AnimationRepair/lib/export_clip.py` | Eksport FBX: poza T na klatce −10, ujęcie `mixamo.com` |
| `ArtSource/AnimationRepair/<etap>/spec_*.py` | Specyfikacja każdego naprawionego klipu (do powtórzenia) |
| `ArtSource/AnimationRepair/review_viewer.py` | Podgląd przed/po w przeglądarce (arkusze z testu, tempo gry) |
| `Scripts/Editor/AnimationRepairImport.cs` | Batch importu klipów: `ApplyLightSlashesBatch`, `ApplyHeavyChopBatch`, `ApplyShoulderChargeBatch`, `ApplyFollowAndRunningBatch`, `ApplyReactionsBatch` |
| `BerserkerAnimeImport.RefreshSkeletonBatch` | Odświeża szkielet awatara Humanoid po zmianie proporcji modelu (patrz pułapka 4.1) |
| `Tests/PlayMode/BerserkerAnimeReviewTests.cs` | Arkusze klatek + CSV (`TURRIS_SHOT_DIR`, `TURRIS_REVIEW_ONLY`, `TURRIS_REVIEW_COLS`, `TURRIS_REVIEW_AUTHORED`) |
| `Tests/PlayMode/BerserkerStanceAuditTests.cs` | Postawa: pomiary po warstwach, zrzuty całej postaci, nóg, dłoni (`dlonie_`, `dlonP/L_`, `chwyt_` z bronią i bez), zgięcie/skręt nadgarstków, położenie broni w gnieździe |

Uruchamianie batchy: `Unity.exe -batchmode -nographics -projectPath . -executeMethod Turris.EditorTools.<Klasa>.<Metoda> -logFile …`
(edytor musi być zamknięty).

## 3. Etapy: co, jak, dlaczego

### 3.1 Lekkie cięcia
- **Co:** naprawiony „great sword slash”. Jako lekki 2 użyty naprawiony „slash (3)” (przekrok) zamiast „slash (4)”, który ma obrót o 360°.
- **Jak:** Blender (IK, czas, stopy), flaga `ActionClip.authoredSwing` wyłącza profil `GreatSwingPose` dla klipów autorskich.
- **Dlaczego:** profil proceduralny nakładał się na zamach wbudowany w klip. Spłycenie przysiadu wymusił test
  `TwoHandCombo_DoesNotDropIntoCrouch` (biodra mogą opaść najwyżej 18 cm).

### 3.2 Postawa i stopy
- **Co:**
  - `GreatSwingPose.Upright`: korekta przechyłu w bok;
  - `KneeHint`: kolano nad palcami;
  - buty: wagi cholewki przeniesione na podudzie.
- **Dlaczego:** retarget klipu spoczynku przechylał tułów o ~11° w bok, kolano uciekało do środka, a cholewka deformowała się razem ze stopą.

### 3.3 Ciężki 1 i atak ładowany
- **Co:** nowy klip `Composed/great axe chop.fbx`: cofnięcie w tył i w dół, ostrze w dół pod kątem, skręt tułowia tylko 8°.
  Drżenie przy ładowaniu: pole `CharacterAnimState.charge` i warstwa `ChargeTremble` (1°→3,2°, 13 Hz). `FloorGuard` pilnuje,
  żeby broń nie wchodziła w podłogę.
- **Dlaczego:** życzenie użytkownika (wycofanie topora, drżenie, potężne uderzenie); topór wcześniej wchodził w ziemię.

### 3.4 Unik = szarża barkiem
- **Co:** akcje `ChargeForward/Back/Left/Right` i 8 klipów zbudowanych z biegów (topór oburącz i tarcza) dla berserkera, wojownika i paladyna HU.
- **Dlaczego:** decyzja projektowa (styl uniku ShoulderCharge). Unik musi przerywać każdą fazę ataku.

### 3.5 Ataki z biegu, ciężki 2
- **Co:** `great axe leap chop` (cios z wyskoku) i `great axe running sweep` (zamach w biegu). Ciężki 2 to **oryginalny**
  „high spin attack”.
- **Dlaczego:** wersja bez obrotu zepsuła ruch, który użytkownik akceptował. Ten błąd dał regułę: najpierw pytać.

### 3.6 Trafienie i śmierć
- **Co:** reakcje zależne od siły i kierunku ciosu (`FlinchHeavy`, `FlinchBack`, `FlinchBackHeavy`), `DeathForward`.
  `CharacterVisual.DropWeaponOnDeath`: broń wypada i kładzie się płasko, korbacz zostaje w dłoni.
- **Dlaczego:** topór wchodził 74 cm w podłogę przy śmierci, a jedna reakcja na wszystkie trafienia była zbyt uboga.

### 3.7 Postawa stojąca z toporem (najdłuższa iteracja)
Kolejne uwagi użytkownika i odpowiedzi:

1. *„Wygięty do tyłu, topór uniesiony i przylepiony, ręce w tułowiu”*.
   Przyczyna: `GuardLean` −16° wyginał postać do tyłu, a dłoń była za blisko tułowia.
   Zmiana: `GuardLean` −4°, chwyt przed tułowiem.
2. **Wzorzec z Elden Ring:** topór nisko oburącz, trzonek poziomo przed pasem, głowica z prawej ostrzem w dół.
   Zmiana: niska garda (`GuardHand`, `GuardAim`, `GuardBlade`).
3. *„Ręce proporcjonalne jak u człowieka”*. Siatka Meshy miała rozpiętość ramion 1,43 m przy wzroście 1,80 m.
   - W Blenderze wydłużono kości i wierzchołki według wag kości ręki: przedramię ×1,40, dłoń ×1,20.
   - Ramię najpierw ×1,22. Uwaga *„biceps/triceps za długi”* – ostatecznie ×1,10.
4. *„Dłonie źle zaciśnięte”*. Trzy niezależne przyczyny, każda znaleziona pomiarem albo zbliżeniem:
   - **kciuk był w 84% przypięty do przedramienia** (podział wag po osi X). Przy obrocie dłoni zostawał z przedramieniem
     i sterczał, a skrypt zginania palców go pomijał. Poprawka: przypięcie do dłoni i podgięcie o 40°;
   - **lewa dłoń trzymała sam koniec trzonka**: rozstaw 34 cm przy trzonku sięgającym 33,6 cm za prawą dłonią;
   - **wykręcone nadgarstki** (P ~120° skrętu, L zgięcie 137° i skręt ~155°), przez co siatka dłoni robiła się płaska.
     Ostrze w dół ustawiano obrotem *dłoni* wokół trzonka.
     Poprawka: `HellEquipmentPose.NaturalWrist`. Dłoń obraca się wokół trzonka do ułożenia najbliższego pozie wiązania,
     resztę skrętu przejmuje przedramię, a ostrze ustawia obrót *broni* wokół trzonka (`GreatSwingPose.GuardSpin`).
5. *„Trzymasz topór za końcówkę”*. Według wzorca prawa dłoń jest wyżej na trzonku, a lewa przy końcu.
   - W gardzie broń przesuwa się w dłoni o `GuardSlide` 0,32 m, rozstaw `GuardGripSpan` 0,56 m.
   - Łokcie są w dół i do tyłu.
   - Wszystko jest ważone udziałem gardy, więc w atakach zostaje dawny chwyt.

Wynik zaakceptowany przez użytkownika („jest ok”).

6. *„W biegu topór ucieka na dół i w bok”* – w biegu działało dawne „niesienie” (`CarryHand/CarryAim`): broń przy prawym
   boku, skośnie w dół i do tyłu, bez naturalnego nadgarstka i bez chwytu z gardy. Teraz bieg używa tego samego chwytu oburącz
   przed sobą (przesunięcie broni, rozstaw, obrót ostrza, łokcie w dół), tylko wyżej i dalej w przód (`CarryHand` (0,28; 1,02; 0,38),
   `CarryAim` (0,95; 0,04; 0,30)), bo tułów w biegu się pochyla. Podgląd `TestResults/berserker-anim-review/podglad_bieg.html`.

## 4. Pułapki (warto pamiętać)

1. **Humanoid trzyma długości kości w `.meta`** (`humanDescription.skeleton`). Po podmianie FBX z innymi proporcjami postać
   w grze dalej ma stare długości. Trzeba uruchomić `BerserkerAnimeImport.RefreshSkeletonBatch`
   (menu Turris → PoC → Berserker anime → Odśwież szkielet awatara). Nie używać do tego `Apply`, bo nadpisuje materiał i wygląd.
2. **Eksport z Blendera** zapisuje bieżącą klatkę jako pozę domyślną. Eksport robić z klatki z pozą T (klucz na −10).
3. **Liczby nie wystarczą przy dłoniach.** Pomiar „lewa dłoń na osi trzonka, 0 cm” był prawdziwy, a dłoń i tak wyglądała źle
   (koniec trzonka, wykręcony nadgarstek). Zawsze trzeba obejrzeć zbliżenie z bronią i bez.
4. **Obrót dłoni zamiast broni** wokół trzonka niszczy nadgarstek. Broń można obracać i przesuwać w dłoni swobodnie, nadgarstek nie.
5. **Model Meshy dzieli wagi skóry po osi X.** Wszystko, co wystaje w poprzek osi ręki (kciuk), może trafić do złej kości.
   Sprawdzać wagi w pozie z obróconą kością.
6. Siatka berserkera ma ~1,27 mln wierzchołków (FBX ok. 111 MB, LFS). Zapisy plików bywają chwilowo blokowane (Errno 22) – wystarczy ponowić.
7. `FrameCostPlayTests` w trybie `gfx` zawsze pada (`WaitForEndOfFrame` w batchmode). W trybie okna ostatnio 19,35 ms przy limicie
   16,7 ms (rycerz, otwarty Blender) – do sprawdzenia na czystej maszynie.

## 5. Pliki źródłowe modelu

- `ArtSource/GothicAnimePoC/Heroes/Berserker/rigged/berserker_anime_rigged.blend` – bieżący. Kopie zapasowe:
  - `_v2_krotkie-rece`: przed wydłużeniem;
  - `_v3_dlugie-ramie`: ramię ×1,22.
- Eksporty: `berserker_anime_v2_krotkie-rece.fbx`, `_v3_dlugie-rece.fbx`, `_v4_krotsze-ramie.fbx` (bieżący, skopiowany do Assets).
- `tools/art/rig_anime_berserker.py` zawiera wydłużenie rąk i poprawkę kciuka. Całego skryptu nie uruchamiano ponownie po tych zmianach.
- Folder `ArtSource/GothicAnimePoC/Heroes/` (~1,9 GB) **nie jest w repozytorium**.

## 6. Otwarte

- Przegląd ataków toporem i młotem z nowym modelem (dłuższe ręce, rozstaw 28 cm) – testy chwytu zielone, wizualnie nie oglądane.
- Kciuk jest długi i prosty. Jeśli będzie przeszkadzał, można go dodatkowo zgiąć w Blenderze.
- Pozostałe wyglądy z mieczem dwuręcznym grają te same klipy – galerii nie oglądano.
