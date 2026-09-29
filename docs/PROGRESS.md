# Turris Damnatorum — stan prac i plan

Aktualizacja: 2026-09-29. Opis gry, sterowania i architektury jest w [README.md](../README.md). Ten plik służy do przekazania pracy między sesjami.

## 1. Stan na teraz

Prototyp jest **grywalny od początku do końca**:
- wybór klasy, trudności i dodatków w budżecie przygotowania;
- 5 pięter z bossem, nagrody między piętrami, kapliczki;
- śmierć i szybki restart, meta-postęp z zapisem.

Działają mysz z klawiaturą oraz pad, także w menu.

| Obszar | Stan |
|---|---|
| Walka (blok tarczą i bronią, parowanie, unik z oknem niewrażliwości, przełamanie gardy, riposta, flaszki) | gotowe, pokryte testami |
| Umiejętności: 3 sloty pod osobnymi przyciskami, czary i techniki bronią, odnowienia i ładunki, odblokowania na stałe + nagrody tymczasowe | gotowe, pokryte testami (`SkillTests`, test w silniku szarży i rozpłatania) |
| Wyposażenie, statystyki, nagrody, odblokowania, trudność, zapis z wersjonowaniem | gotowe, pokryte testami |
| AI: 3 archetypy + boss z 2 fazami, elity, dodatkowe zachowania bossa | gotowe |
| Postacie: proceduralne humanoidy (240–330 części), animacja zsynchronizowana z fazami walki | gotowe |
| Postacie FBX (Mixamo) – wariant B | kod i narzędzia gotowe, **nieprzetestowane na prawdziwym modelu** |
| Efekty czarów i walki (ParticleSystem, kręgi runiczne, światła, smugi ostrzy) | gotowe |
| Areny: Dziedziniec, Krypta, Szczyt (65–120 tys. wierzchołków) | gotowe |
| Interfejs IMGUI z nawigacją padem | gotowy (prototypowy) |
| Dźwięk | **brak** |

### Wyniki testów (ostatni przebieg)

- **EditMode:** 75/75.
- **PlayMode:** 21/21 + 3 galerie pominięte (bez `TURRIS_SHOT_DIR`) w oknie edytora; z `TURRIS_SHOT_DIR` w oknie 24/24 (`python tools/run_tests.py PlayMode window`).
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

Areny z testu `Arenas_BuildWithSimpleCollidersAndBakedVisuals`:

| Arena | Wierzchołki | Światła | Systemy cząsteczek |
|---|---|---|---|
| Dziedziniec | ~120 tys. | 5 | 20 |
| Krypta | ~66 tys. | 12 | 44 |
| Szczyt | ~121 tys. | 4 | 16 |

### Stan repozytorium

- GitHub: `dbujak79/Turris_Damnatorum`, gałąź `main`. Commity: `init`, `graphics improvement`.
- **Niezatwierdzone zmiany** z ostatniej iteracji: efekty (`Scripts/Runtime/Fx/`), `ArenaBuilder`, druga warstwa detali postaci, materiały w `Resources/`, `tools/run_tests.py`, ta dokumentacja. Do zatwierdzenia na prośbę użytkownika.

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

- **Do oceny w ręcznym graniu:** czy tempo nie jest za duże dla wrogów (gameSpeed przyspiesza też AI) i czy unik-przerwanie nie jest zbyt „bezpieczne” (ewentualnie dodać koszt wytrzymałości lub minimalny czas zamachu).

## 3. Jak pracować z projektem (ważne dla kolejnej sesji)

- **Testy i kompilacja z CLI wymagają zamkniętego edytora.** Gdy projekt jest otwarty, Unity kończy się od razu z kodem 1. Trzeba poprosić użytkownika o zamknięcie edytora. Otwartość można sprawdzić przez `tasklist | grep -i unity.exe`.
- **Kompilacja = uruchomienie testów EditMode** (`python tools/run_tests.py EditMode`). Błędy `error CS` są wypisywane z logu.
- **Weryfikacja wyglądu:** `TURRIS_SHOT_DIR=<katalog> python tools/run_tests.py PlayMode gfx` zapisuje PNG. Galerie:
  - `pose_*` — pozy i zbliżenia postaci;
  - `arena_*` — areny;
  - `fx_*` — efekty;
  - `0_menu` … `3_boss` — gra.

  Zrzuty trzeba obejrzeć przed ogłoszeniem efektu. Kilka błędów wizualnych wyszło dopiero na nich.
- **`OnGUI` nie działa w trybie `-batchmode`**, także z grafiką. Testy interfejsu (`GamepadPlayTests`) uruchamiaj w trybie `window`.
- **Regeneracja treści:**
  - `-executeMethod Turris.EditorTools.TurrisSetup.RunBatch` tworzy brakujące assety, scenę i materiały efektów;
  - `RunBatchForce` **nadpisuje wszystkie assety treści** wartościami z `DefaultContent`, a zmiany użytkownika w inspektorze przepadają. Używaj tylko świadomie.

  Nowe pola w SO dodawaj z wartością domyślną `Auto`, żeby istniejące assety działały bez regeneracji.
- **Długie skrypty edycyjne:** w narzędziu Bash długi heredoc z Pythonem potrafi się wysypać („unexpected EOF”). Lepiej zapisać skrypt `.py` w katalogu tymczasowym i go uruchomić. Uwaga na `\b` w stringach Pythona: bez prefiksu `r` to znak backspace, a nie granica słowa w regex.
- **Klasy `ScriptableObject` i `MonoBehaviour` serializowane w assetach** muszą mieć plik o tej samej nazwie co klasa.

## 4. Pułapki techniczne już rozwiązane

- `InputAction.expectedControlType` ustawia się właściwością, bo `AddAction` nie ma takiego parametru w Input System 1.20.
- `CharacterController.minMoveDistance = 0`, inaczej przy wysokim FPS postacie stoją w miejscu.
- `FindFirstObjectByType` i `FindObjectsByType(SortMode)` są przestarzałe w 6.6; w projekcie używane są rejestry statyczne oraz `FindAnyObjectByType`/`FindObjectsByType(FindObjectsInactive)`.
- Zmiana stanu w IMGUI w trakcie przebiegu psuje GUILayout, dlatego akcje przycisków są kolejkowane i wykonywane na końcu `OnGUI` (`UINavigator.Enqueue`).
- `renderer.material` w EditMode tworzy wycieki i ostrzeżenia. Podświetlenia działają przez `MaterialPropertyBlock` (`TintSet`), a efekty poza trybem gry zwracają `null`.
- Wybór „słońca” w `GameRoot` obejmuje tylko `LightType.Directional`, bo światła pochodni są niszczone przy zmianie areny.
- Otwarte bryły obrotowe (szaty, rękawy, kaptur) są dwustronne (`ProcMesh.Lathe(doubleSided)`), inaczej od środka znikają.
- Scalanie części (`PartBuilder.BakeAll`) łączy tylko bezpośrednie dzieci z `RigPart`. Obiekty przełączane (flaszka) i przestawiane (gniazda broni) muszą być osobnymi węzłami.

## 5. Kolejne kroki (propozycja priorytetów)

1. **Sesja ręcznego grania** (użytkownik, pad i klawiatura). Do oceny: odczucie sterowania i kamery, czytelność telegrafów i efektów, balans pięter. Poprawki parametrów w `GameConfig → balance` i assetach wrogów.
2. **Pomiar wydajności.** Profiler, FPS w krypcie (12 świateł, 44 systemy cząsteczek). W razie potrzeby:
   - ustawienie jakości (liczba świateł i cząsteczek);
   - łączenie świec w jedno światło;
   - LOD dla panoramy szczytu.
3. **Dźwięk:** uderzenia, blok, parowanie, czary, kroki, muzyka bossa. Obecnie brak.
4. **Wariant B na prawdziwych plikach z Mixamo.** Sprawdzić:
   - kreator `CharacterModelTools`;
   - maskę górnej połowy ciała;
   - położenie broni w dłoni;
   - granice faz `windupEnd` i `activeEnd`.
5. **Treść:** podejście do docelowych 20–30 min (więcej pięter i aren), więcej wrogów i ataków, przeciwnicy parujący, NavMesh albo omijanie filarów.
6. **Interfejs docelowy** (UI Toolkit), zmiana przypisań przycisków, wibracje pada.
7. **Przed wydaniem:** usunąć deweloperskie F9 (+100 popiołu), sprawdzić build (materiały efektów z `Resources`, shadery „Legacy Shaders/Particles”).

## 6. Preferencje użytkownika

- Komunikacja i dokumentacja **po polsku**.
- Oczekuje wysokiej szczegółowości wizualnej; kolejne rundy prosiły o „×4–5”, a potem „×2”. Mierzalne liczby (części, renderery) i zrzuty pomagają pokazać postęp.
- Chce pełnej obsługi pada.
- Przy decyzjach architektonicznych wybrał podejście łączące: szybki efekt teraz, a jednocześnie gotowość na lepsze rozwiązanie później.
