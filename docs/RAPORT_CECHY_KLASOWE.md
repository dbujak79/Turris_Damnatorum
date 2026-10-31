# Raport wdrożenia: 30 cech klasowych

Data: 2026-10-31. Źródło zakresu: [PROMPT_WDROZENIE_30_CECH_KLASOWYCH.md](PROMPT_WDROZENIE_30_CECH_KLASOWYCH.md).

## 1. Co działa

- **Wybór:** gracz wybiera jedną z dwóch cech swojej klasy w przygotowaniu podejścia. Dostępne są obie od razu, bez kosztu.
  - Karty A/B pokazują regułę wyłączną, korzyść, koszt/ryzyko, sposób użycia, nietypowy build oraz build startowy i rozwinięty. Liczby w opisach pochodzą z `GameConfig.classFeatures`, a przycisk z aktualnego przypisania (`{G}` → klawisz/przycisk pada).
  - Karty to zwykłe przełączniki IMGUI z nawigacją padem. Fokus nie zmienia wyboru, a wybrana karta ma złote oznaczenie „● WYBRANA”.
  - Na dole ekranu jest podsumowanie startu: klasa, wygląd, cecha, broń, skille i trudność.
- **Przycisk klasy** (G / View, z remapowaniem) wywołuje wybraną cechę. Dawna sygnatura nie powstaje obok niej. Jej definicje zostają w danych (migracja zapisów, poligon).
  - Bazowa właściwość klasy nazywa się teraz w UI „Mechanika klasy”.
- **Trwałość:**
  - plan (`LoadoutPlan.classFeatureId`);
  - profil per klasa (`ProfileData.classFeatures`, wersja 4 z migracją);
  - `RunState.classFeatureId`, snapshot i wyzwania (`RunSnapshot.classFeatureId`, dług many, drugi zestaw Arsenału);
  - restart po śmierci (`RetrySameLoadout` używa tego samego planu) i start z zapisu.
- **Stary zapis** bez pola dostaje wariant A. Stary Berserker z `hu_undying` dostaje Czerwoną linię. Pieczęcie dawnej sygnatury wracają do sakwy, wyposażenie zostaje.
  - Obce, nieznane i puste id → wariant A z diagnostyką (`ClassFeatureCatalog.Resolve`). Obca cecha w stanie podejścia nie instaluje kontrolera.
- **Wyłączność:** cechy nie są `ContentDefinition`, nie trafiają do sklepu, odblokowań, łupu ani pul skilli. Skille startowe wariantów to zwykłe skille z puli sklepu, dostępne też innym klasom.

## 2. Architektura

| Element | Plik |
|---|---|
| Definicje i katalog 30 kart (id, klasa, teksty z parametrów, buildy, skille startowe, reset) | `Scripts/Runtime/ClassFeatures/ClassFeatureDefinition.cs`, `ClassFeatureCatalog.cs` |
| Parametry (hipotezy startowe) | `ClassFeatureTuning.cs` → `GameConfig.classFeatures` |
| Kontroler bazowy (stan HUD, odnowienie, telemetria, hooki) i fabryka | `ClassFeatureController.cs`, `ClassFeatureFactory.cs` |
| 30 kontrolerów | `Features.Warrior.cs`, `Features.Mage.cs`, `Features.Summoner.cs`, `Features.Rogue.cs` |
| Byty świata | `FeatureDecoy.cs`, `PinnedShadow.cs`, `EarthWall.cs`, `FiringPost.cs`, chimera w `Combat/Servant.cs` |
| Wpięcie w walkę gracza | `Player/PlayerCombat.Features.cs` i hooki w `PlayerCombat.cs`, `PlayerCombat.Hero.cs` |
| Strona wroga (wabik, egzekucja, Sąd, zderzenia, ściana, zdarzenia ataków) | `Enemies/EnemyBrain.Features.cs` |
| Stan podejścia (dług, Arsenał, parkowanie skilli przedmiotów) | `Run/RunState.Features.cs` |
| UI (karty, podsumowanie, okienko HUD, pasek rezerwacji, dług, Arsenał w ekwipunku) | `UI/GameUI.Features.cs` |

Kontrolery są lokalne, bez wyjątków rozsianych po całym PlayerCombat. Wspólne operacje mają jedno wykonanie:
- trafienie efektu cechy: `FeatureHit` – efekt potomny bez kradzieży życia, leczenia za zabójstwo i nowych zasobów cech;
- cel: `FeatureTarget` – namierzenie → pomoc w celowaniu → stożek 70°;
- koszt życia: `PayLife`; samoobrażenia: `SelfFire`;
- leczenie ze źródłem: `HealFrom`.

Pochodzenie trafień (rozdz. 7.2):
- `HitData.originId` – id ręcznej akcji, dziedziczone przez kopie, odpryski, przebicia i strefy;
- `basicAttack`, `featureEffect` oraz ładunki cech (`answerCharge`, `spellArrowToken`, `frontsBudget`, `nailShare`, `launchPoint`).

Pula zasobu (`ResourcePool`) dostała trzy rozszerzenia:
- **sufit** (`Ceiling`) – dla Czerwonej linii; `Max` i `Fraction` dalej liczą się względem normalnego maksimum;
- **filtr odnowienia** (`RestoreFilter`) – spłata długu przez wszystkie źródła many;
- **nadwyżkę** (`Overflow`) – nadleczenie do Łaski.

### Kolejność obrony bohatera

1. Odporność na żywioł.
2. Reakcje.
3. Pancerz albo garda (`DamageResolver`).
4. Ogólna redukcja z miękkim limitem.
5. Minimum ciosu.
6. **Czerwona linia**.
7. Furia Berserkera.
8. Szarża.
9. Pancerz w ataku.
10. Arkanowa osłona.
11. **Płaszcz many**.
12. Bariera.
13. Życie.

Koszty życia i rezerwacja nie przechodzą przez ten potok.

**Przykład łączenia warstw** przy 35% PŻ, pełnej furii, pancerzu w ataku i ogólnej redukcji 45%:
(1 − 0,52)(1 − 0,20)(1 − 0,50)(1 − 0,45) = **0,1056** budżetu po tych czterech mnożnikach. Stała jest sprawdzana w `RedLine_CurveTable_IsMonotonic`, a rzeczywisty potok w `RedLine_DamageMatchesCurveFromPreHitLife`.

### Kolejność wyjścia gracza

1. Przemiana w parę.
2. **Hook cechy** (np. budżet frontów).
3. Premie ogólne i przedmiotów.
4. Penetracja.
5. Premia żywiołu.
6. Modyfikatory efektów.
7. **Późny hook cechy** (rodziny podpaleń).

Strzała odpowiedzi rozstrzyga żywioł na końcu, przy kontakcie: po konwersjach, z jednorazową premią wybranego żywiołu, przed odpornością celu.

## 3. Tabela stanu 30 wariantów

Kolumny:
- **D** – dane (definicja, parametry);
- **R** – runtime (prawdziwa mechanika);
- **W** – wybór i zapis;
- **S** – sterowanie (przycisk klasy, druga faza);
- **O** – oprawa;
- **TM** – test matematyki/zasobów (EditMode);
- **TG** – test w grze (PlayMode);
- **Img** – obejrzany zrzut;
- **Bot** – użycie przez bota w scenariuszu;
- **B** – legalny build startowy.

| Cecha | D | R | W | S | O | TM | TG | Img | Bot | B |
|---|---|---|---|---|---|---|---|---|---|---|
| Wojownik A – Walka w gardzie | ✓ | ✓ | ✓ | ✓ | aura | ✓ | – | – | ✓ | miecz+tarcza |
| Wojownik B – Arsenał weterana | ✓ | ✓ | ✓ (zestaw II w zapisie) | ✓ | model broni | ✓ (100 zmian) | – | – | ✓ | oba zestawy startowe |
| Paladyn A – Rozgrzeszenie | ✓ | ✓ | ✓ | ✓ | fala | ✓ | – | – | ✓ | regen. klasy |
| Paladyn B – Sąd przysięgi | ✓ | ✓ (6 aur) | ✓ | ✓ | aura na celu | – | ✓ | – | ✗* | 1 aura startowa |
| Berserker A – Czerwona linia | ✓ | ✓ | ✓ (stary zapis → A) | ✓ | pierścień + HUD | ✓ (tabela, krzywa, zgon) | – | ✓ | ✓ | topór |
| Berserker B – Spalenie furii | ✓ | ✓ | ✓ | ✓ | fala | ✓ | – | – | ✗* | Rozpłatanie |
| Rębajło A – Niezgodna para | ✓ | ✓ | ✓ | ✓ | aura | ✓ (tempo) | ✓ (2 pakiety) | – | ✓ | dwa miecze |
| Rębajło B – Taniec bez przerwy | ✓ | ✓ | ✓ | ✓ | świst | – | ✓ | – | ✓ | dwa miecze |
| Mag A – Dług arkanów | ✓ | ✓ | ✓ (dług w zapisie) | ✓ | HUD długu | ✓ (limit, spłata, piętro) | – | – | ✗* | różdżka+grymuar |
| Mag B – Płaszcz many | ✓ | ✓ | ✓ | ✓ | aura | ✓ (częściowa mana) | – | – | ✗* | różdżka+grymuar |
| Mag ognia A – Żywy piec | ✓ | ✓ | ✓ | ✓ | płomień | – | ✓ | – | ✓ | różdżka+grymuar ognia |
| Mag ognia B – Wielki pożar | ✓ | ✓ | ✓ | ✓ | „płonie ×N” | ✓ | ✓ | – | ✓ | różdżka + nova |
| Mag wody A – Niezmienny szron | ✓ | ✓ | ✓ | ✓ | lód na celu | ✓ | ✓ (boss) | – | ✓ | + Piekielna kula |
| Mag wody B – Prąd głębin | ✓ | ✓ | ✓ | ✓ | błyskawica pola | – | ✓ | – | ✓ | + Krąg żaru |
| Mag powietrza A – Oko ciszy | ✓ | ✓ | ✓ | ✓ (2 fazy) | krąg | – | ✓ | ✓ | ✓ | różdżka powietrza |
| Mag powietrza B – Zderzenie frontów | ✓ | ✓ | ✓ | ✓ | piorun | – | ✓ (ściana, boss) | – | ✓ | różdżka powietrza |
| Mag ziemi A – Bastion ziemi | ✓ | ✓ | ✓ | ✓ | ściana | – | ✓ (pocisk) | ✓ | ✓ (pęknięcia) | różdżka+nova ziemi |
| Mag ziemi B – Krusząca skorupa | ✓ | ✓ | ✓ | ✓ | płyty | ✓ | ✓ | – | ✓ | Arkanowa osłona |
| Przywoływacz A – Wcielenie | ✓ | ✓ | ✓ | ✓ | forma sługi | – | ✓ | ✓ | ✓ | chochliki |
| Przywoływacz B – Chimera legionu | ✓ | ✓ | ✓ | ✓ (2 fazy) | korpus + składniki | – | ✓ | ✓ | ✓ | chochliki |
| Mag bitewny A – Arkana oręża | ✓ | ✓ | ✓ | ✓ | HUD „Arkana oręża” | ✓ (tylko właściciel) | – | – | ✓ | + Piekielna kula |
| Mag bitewny B – Dwa takty | ✓ | ✓ | ✓ | ✓ | zbieranie mocy | ✓ | ✓ | – | ✓ | miecz+grymuar |
| Łotrzyk A – Fałszywy świadek | ✓ | ✓ | ✓ | ✓ | wabik | – | ✓ (AI) | ✓ | ✗* | sztylety |
| Łotrzyk B – Kradzież sztuczki | ✓ | ✓ | ✓ | ✓ (2 fazy) | stożek/pocisk/fala | – | ✓ | – | ✗* | łuk/sztylety |
| Łucznik A – Gwoździe łowcy | ✓ | ✓ | ✓ | ✓ (2 fazy) | krew przy wyrwaniu | – | ✓ | – | ✓ | łuk |
| Łucznik B – Stanowisko strzeleckie | ✓ | ✓ | ✓ | ✓ | pal z krągiem | – | ✓ | ✓ | ✓ | łuk |
| Zabójca A – Przygwożdżony cień | ✓ | ✓ | ✓ | ✓ | sylwetka + nić | – | ✓ (jedna tożsamość) | ✓ | ✓ | sztylet |
| Zabójca B – Otwarta tętnica | ✓ | ✓ | ✓ | ✓ | krew na celu | – | ✓ | – | ✓ | +30% egzekucji |
| Łucznik arkanów A – Strzała zaklęcia | ✓ | ✓ | ✓ | ✓ (slot jako wybór) | czar w miejscu trafienia | ✓ | ✓ | – | ✓ | + Łza lodowca |
| Łucznik arkanów B – Strzała odpowiedzi | ✓ | ✓ | ✓ | ✓ | błysk żywiołu | ✓ | ✓ | – | ✓ | + Sidła |

\* W ostatnim przebiegu scenariusza botów (45 s, piętro 1) bot nie spełnił warunku użycia: brak szału w 45 s, mag trzymający dystans, wabik bez wrogów blisko, kradzież bez obserwowanego zwykłego ataku. Warunki poluzowano po przebiegu (patrz rozdz. 6) i nie powtórzono go jeszcze. To ocena polityki bota, a nie mechaniki. Mechanika tych wariantów ma osobne testy.

## 4. Ważne decyzje (do wiadomości autora)

- **Żywy piec:** własny płomień płonie pod Arkanową osłoną maga i jej nie zdejmuje. Paliwo ogranicza odporność na ogień i bariera.
  - Bez tego osłona (20% życia, stale odrastająca) pochłaniała cały płomień i wariant był martwy na starcie.
  - Odporność na podpalenie dalej zeruje paliwo.
- **Czerwona linia:** odnowienie startowo jak dawna Żelazna skóra (60 s), trans 6 s, linia 35%, R_max 0,8. Bierne leczenie w transie zostaje osłabione o połowę (dawna zasada). Wartości czekają na pomiar w serii botów.
- **Strzała zaklęcia:** ładunek wyzwala się tylko na trafieniu wroga. Pudło (ściana, koniec lotu) traci go bez zwrotu.
  - Działają czary HU (z pieczęciami), które nie są aurą, leczeniem, barierą, przywołaniem ani doskokiem.
  - Moc czaru bez mnożnika łuku (wzorzec ×1).
- **Dwa takty:** w osobnym torze idą czary bez fazy ciała, czyli bez doskoku i młynka. Pozostałe idą zwykłym rzutem.
- **Gwoździe łowcy:** pierwszy cel strzału dostaje pełny grot, kolejne (przebicie, rykoszet) połowę. Groty niewyrwane gasną 5 s po oknie.
- **Wielki pożar:** rodzina = grupa zwykłych ataków albo nazwa skilla. Kopie i strefy dziedziczą trafienie. Po oknie zostaje najsilniejsza rodzina z jej pozostałym czasem.
- **Bastion:** wrogowie omijają ścianę bliższym końcem, a przy zetknięciu ją burzą (boss ×4). Ściana blokuje pociski obu stron.

## 5. Ograniczenia i luki

1. **Animacja:**
   - Przeniesiony cios Tańca i czar w torze Dwóch taktów nie mają własnej warstwy animacji. Ciało pokazuje unik albo atak bronią, a drugą akcję pokazują efekt świstu i zbieranie mocy przy dłoni.
   - Forma Wcielenia to proceduralny model sługi (sylwetka i atak), bez osobnych klipów biegu.
   - Chwytów nowych modeli nie zmieniano: cechy nie dodają modeli trzymających broń.
2. **Dźwięk:** cechy używają istniejących efektów (ryk, blok, ogień, wyładowania). Nie ma nowych klipów.
3. **Zrzuty UI** wykonane w trybie `window` (`ClassFeatureUiShots`, `TestResults/class-features/shots/class-features-ui`) i obejrzane: karty A/B (Berserker A i B, Arsenał, Łucznik arkanów), podsumowanie startu, okienko cechy w HUD, rezerwacja paska życia z redukcją −52% przy 35%, dług many (aktywny i blokada), zestaw II w ekwipunku. Poprawione po obejrzeniu: opis klasy nie wspomina już dawnej sygnatury (cięty w UI, asset bez zmian), napis długu przeniesiony obok paska many. Fokusu pada na karcie nie fotografowano osobno (karty to zwykłe przełączniki nawigatora).
4. **Wielogodzinne serie botów** nie były uruchamiane – zgodnie z obowiązującym wstrzymaniem. Wykonano krótkie scenariusze 30 wariantów (rozdz. 6). Parametry to nadal hipotezy startowe.
5. **Sąd przysięgi z aurą-sztandarem:** środek sztandaru się nie przenosi (odwrócone działanie na celu tak).
6. **Build gracza** (player build) nie był sprawdzany w tej sesji.

## 6. Testy i dowody

- **EditMode:** 275/278.
  - 2 znane, niezwiązane błędy „AnimeForward”, 1 pominięty eksport WAV.
  - Nowe `ClassFeatureTests` (25 testów): katalog i wyłączność, wybór per klasa, profil v3→v4, RunFactory, zapis i stary zapis, tabela krzywej, rezerwacja, leczenie i zgon w transie, żetony furii (zwrot przy anulowaniu), Łaska, dług (limit, blokada, spłata, piętro, zapis), płaszcz, garda (tył trafia), Arsenał (100 zmian), Arkana oręża, rodziny podpaleń, Szron, kolejność odpowiedzi, Strzała zaklęcia, odmowy bez kosztu, skorupa, tor Dwóch taktów, tempo Niezgodnej pary.
  - Stare testy sygnatur przepisano na kontrakt cech albo jawny tryb zgodności (`Fixture.Legacy`).
- **PlayMode `ClassFeaturePlayTests`:** 22/22 w silniku (wabik i AI, cień, tętnica, Oko ciszy, ściana, Prąd głębin, Sąd, Wcielenie, chimera, fronty, groty, stanowisko, Strzała zaklęcia i odpowiedzi, Żywy piec, Wielki pożar, para, taniec, kradzież, Szron na bossie, płyty, Dwa takty).
- **Scenariusze botów** (`TURRIS_FEATURE_BOTS`, `ClassFeatureBotScenarios`): raport w `TestResults/class-features/bots/RAPORT_CECHY_BOTY.md`. W ostatnim przebiegu użyto 24/30 wariantów, z telemetrią mechaniki, np.:
  - Żywy piec: koszt 116 PŻ → 116 Żaru → 61 obrażeń;
  - Bastion: 2 ściany i 2 pęknięcia;
  - chimera: 4 scalenia;
  - Wcielenie: 12 ataków formy;
  - tętnica: 4 egzekucje poza progiem.
- **Galeria:** `ClassFeatureGalleryTests` – zrzuty obejrzane (ściana, wabik, cień, stanowisko, chimera, forma i powrót modelu, trans, Oko ciszy).
- **Pełny PlayMode w trybie `gfx`:** 167 zaliczonych, 1 niezaliczony, 55 pominiętych (na żądanie). Jedyny błąd to `Floor1Combat_FrameWorkFitsBudget` (`WaitForEndOfFrame` w trybie wsadowym, niezwiązany z cechami). Wyniki: `TestResults/class-features/full/`.
- **Pełny PlayMode w trybie `-nographics`:** kończy się awarią URP w niezwiązanym teście renderującym (`AnimeHeroStylePlayTests.Capture`).

## 7. Następne kroki

1. Seria botów (gracz i tester) per wariant po decyzji autora o wznowieniu serii – strojenie liczb z `ClassFeatureTuning`.
3. Warstwa animacji górnej części ciała dla Tańca bez przerwy i Dwóch taktów; dźwięki gotowości i wyzwolenia cech.
4. Kontrola w buildzie gracza.
