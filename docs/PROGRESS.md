# Turris Damnatorum — stan prac i plan

### Naprawa animacji berserkera, etap 1: przegląd i lekkie cięcia — 2026-10-31

- Na polecenie użytkownika („kompleksowo naprawiamy Berserker, do przeglądu wszystkie animacje”) – skill `repair-souls-animation`, Blender 5.2.2 przez MCP.
- **Ustalenia:** berserker nie ma własnych klipów – z toporem i młotem gra wspólny `anim_greatsword` (Mixamo, Humanoid, 18 wyglądów); `.blend` berserkera to tylko rig i siatka. Ruch w ataku daje skrypt (`rootForward`), nie root motion; klipy zmieniają warstwy `GreatSwingPose` i `HellEquipmentPose`.
- **Przegląd stanu wyjściowego:** nowy test opt-in `BerserkerAnimeReviewTests` (TURRIS_SHOT_DIR; TURRIS_REVIEW_ONLY, TURRIS_REVIEW_COLS, TURRIS_REVIEW_AUTHORED) – 31 sekwencji topora, 19 młota: ruch, ataki z czasami gry, ładowanie, serie, przerwania unikiem, unik, blok, parowanie, trafienie, śmierć, skille; arkusze trzech widoków i CSV pozycji co klatkę. Wyniki i lista usterek: `TestResults/berserker-anim-review/` (m.in. błądzący zamach ciężkiego 1, lekki 1 bez przejścia ostrza przed postacią, lekki 2 z obrotem 360°, unik-przewrót zamiast szarży barkiem, brak klipów powalenia).
- **Decyzje użytkownika:** naprawa wspólnych klipów; naprawa w Blenderze i bez profilu `GreatSwingPose` dla naprawionych klipów; unik berserkera – szarża barkiem (później); lekki 2 – naprawiony „slash (3)” zamiast „slash (4)”; wyniki pokazywać w przeglądarce.
- **Naprawione** (`ArtSource/AnimationRepair/greatsword-light-slashes/`, oryginały w `original/`, skrypty `repair_swing.py`, `spec_*.py`, `export_clip.py`):
  - „great sword slash” (lekki 1, Cleave/SlashRight): zwinięcie tułowia w zamachu, rozkręcenie, biodra niżej, dociągnięcie nisko w lewo z krótkim zatrzymaniem; stopy z nagrania;
  - „great sword slash (3)” (lekki 2, CleaveBack/SlashLeft): cięcie z lewej na prawą; zamiast cofnięcia nogi przekrok w przód (stopa podporowa stoi, lewa robi krok po łuku), garda ~1 m dalej;
  - rozstaw dłoni na trzonku do 30 cm (uwaga użytkownika „nieco większy”), ograniczony zasięgiem ramienia; w zamachu za karkiem dłonie zsuwają się.
- Kod: `ActionClip.authoredSwing` (klip autorski – `GreatSwingPose` bez profilu ciosu), `AnimationRepairImport` (menu **Turris → Naprawa animacji → Lekkie cięcia miecza dwuręcznego**, przelicza `rootForward`), `HeroAssetImport` wskazuje nowe klipy.
- **Pułapka eksportu** (jak przy modelach): Blender zapisuje bieżącą klatkę jako pozę domyślną – eksport z klatki z pozą T (klucz na −10), klatki od 0, ujęcie `mixamo.com`; ponowny import zgodny co do 0,08 mm.
- Pomiary w grze (topór): lekki 1 – ostrze przechodzi przed postacią i kończy nisko po lewej; lekki 2 – czubek max 103 → 38 m/s, skok pozy 18,7 → 9,2 m/s; seria lekka – skok pozy 35,2 → 16,6 m/s; chwyt 0–1,8 cm.
- Podgląd: `TestResults/berserker-anim-review/podglad_lekkie_ciecia.html` (generator `ArtSource/AnimationRepair/review_viewer.py`).
- Testy: PlayMode (Berserker, GreatWeaponMoveset, Animation) **20/20**, 6 pominiętych galerii; EditMode **275/278** (te same 2 znane, 1 pominięty).
- **Postawa topora** (uwaga użytkownika: „dziwnie przechylony, coś nie tak z prawą nogą”): nowy audyt opt-in `BerserkerStanceAuditTests` (pomiary po każdej warstwie `PoseAudit`, zrzuty 4 stron: poza wiązania, sam klip, stan gry). Rig w pozie wiązania symetryczny – przyczyny poza rigiem:
  - przechył tułowia w bok 10,9° i prawy bark niżej (−21°) z klipu „great sword idle (3)” – `GreatSwingPose.Upright` w staniu prostuje kręgosłup o zmierzony przechył (wynik 2,7°, bark −5,5°);
  - lewe kolano zgięte do środka i do tyłu (118° od kierunku stopy): `Reach` przy prostej nodze z klipu brał płaszczyznę zgięcia z samego przesunięcia stopy (`PlaceFeet`). Nowy opcjonalny `kneeHint` (kolano nad palcami, 8° na zewnątrz) – tylko w wywołaniach `GreatSwingPose`; IK kroków biegu bez zmian. Wynik: kolana −9° / +15° przy stopach 6° / 16°.
  - **Buty** (uwaga: „nogi pod kolanami są dziwne”): w rigu cholewka do 0,24–0,34 m szła w całości za kością stopy – przy ugiętym kolanie i płaskiej stopie łydka łamała się w kształt S. Wagi podudzie/stopa przeliczone z przejściem w kostce (0,12–0,20 m) w `berserker_anime_rigged.blend`, ponowny eksport `berserker_anime.fbx` (ustawienia skryptu), `tools/art/rig_anime_berserker.py` (gałąź berserkera; mag bez zmian). Kopie poprzednich: `ArtSource/.../rigged/*_v1_cholewka-na-stopie.*`. Testy berserkera 4/4, stopy/modele/animacje 34/34 (13 galerii pominiętych).
  - Głowa pozostaje lekko przechylona jak w klipie. Podgląd: `TestResults/berserker-anim-review/podglad_postawa.html`. Testy stóp/animacji/modeli bohaterów: 37/37 (13 galerii pominiętych).
- **Ciężki cios i ładowany** (życzenie: „ładowanie – postać wycofuje topór do tyłu i na dół, w trakcie lekkie drganie, po zakończeniu potężne uderzenie”):
  - nowy klip `Models/Animations/Mixamo/Composed/great axe chop.fbx` (na bazie „Standing Melee Attack Downward”, który zostaje dla miecza z tarczą i rąbnięcia po uniku); źródła `ArtSource/AnimationRepair/heavy-chop/`, wspólna biblioteka `ArtSource/AnimationRepair/lib/` (przemapowanie czasu, pełna orientacja broni);
  - przygotowanie rozciągnięte: topór schodzi z gardy w tył i w dół – pozycja ładowania (czubek ~0,35 m za postacią) wypada w punkcie zatrzymania gry (80% zamachu); po puszczeniu szybki łuk górą za plecami i ostrze w ziemi przed postacią (skłon 30°, biodra −23 cm), krótkie zatrzymanie, powrót; znaczniki 24/29 i długość bez zmian; ostrze prowadzi ruch (kalibracja osi ostrza z gry – nowa kolumna `blade` w CSV przeglądu);
  - drżenie: `CharacterAnimState.charge` (PlayerCombat w trakcie trzymania), `ClipAnimationDriver.ChargeTremble` – tułów, głowa, ramiona, 1°→3,2° przy 13 Hz; w trzymaniu czubek ±5 cm, dłonie ±1,5 cm; znika przy puszczeniu;
  - `GreatSwingPose.FloorGuard`: dla klipów autorskich broń nie wchodzi w podłogę (inne proporcje postaci); test `GreatSwing_ChopReachesTheGround…` mierzy przysiad wysokością bioder (0,23 m), ostrze 0,02 m nad podłogą;
  - kalibracja: oś trzonka w dłoni Mixamo różni się między klipami (do 21°, rozkład skrętu nadgarstka w retargecie) – kalibrować per klip;
  - poprawka po uwadze („ostrze w dół, pod kątem; pozycja przesadzona; postać ma ręce, tułów nie musi się tak skręcać”): w ładowaniu ostrze w dół, odchylone 35° na zewnątrz; skręt tułowia 25° → 8°, miednicy 10° → 3°, przysiad 10 → 6 cm, dłonie dalej w tył przy prawym biodrze; obrót ostrza do przodu rozłożony na łuk górą (kl. 20–25); czubek przy cofaniu min. 25 cm nad podłogą; testy 34/34 (11 galerii pominiętych);
  - podgląd `TestResults/berserker-anim-review/podglad_ciezki.html`; testy PlayMode 49/49 (13 galerii pominiętych), EditMode 275/278 (te same 2 znane).
- **Unik – szarża barkiem** (decyzje: warianty kierunkowe, wszystkie trzy klasy ze stylem ShoulderCharge – berserker, wojownik, paladyn HU):
  - nowe `AnimAction.ChargeForward/Back/Left/Right` (na końcu enuma); `AnimResolve.ChargeFor` wybiera wariant z kierunku uniku względem postaci (±50° przód, >130° tył), `PlayerCombat.GetAnimState` dla stylu ShoulderCharge; sterownik traktuje je jak unik (`AnimResolve.IsDodge`), bez klipu w zestawie – zwykły unik;
  - 8 klipów `Composed/shoulder charge <kierunek> (twohand|shield).fbx` na bazie „Pro Longbow Pack / standing dodge …” (`ArtSource/AnimationRepair/shoulder-charge/`): przód – lewy bark prowadzi, ciało bokiem, głowa schowana; boki – ciało przodem, prowadzi bark po stronie ruchu, przechył w ruch; tył – odskok z wypchnięciem; ręce: broń dwuręczna niesiona nisko przy prawej piersi (bieg z mieczem dwuręcznym), tarcza wysunięta (blok tarczą); stopy obracają się z miednicą;
  - czas nagrania przemapowany pod ruch z gry (3,7 m w 0,6 s, profil 1−u²): odbicie ~0,06 s, lot do ~0,42 s – ślizg stóp przy podłodze 2,0–2,6 m → 0,5–1,2 m na stopę;
  - zestawy: `anim_greatsword` (dwuręczna), `anim_sword_shield` i `anim_paladin_anime_onehand` (tarcza) – menu **Turris → Naprawa animacji → Szarża barkiem**, także w `HeroAssetImport`;
  - **wersja 2** (uwaga: „szarża to raczej skręt tułowia, żeby bark był z przodu, i prawie normalny bieg, a nie bieg bokiem”): baza zmieniona na biegi z zestawów (`spec_charge_run.py`) – przód: bieg, tułów skręcony 40° w prawo (lewy bark z przodu), pochylenie 14°, głowa kontruje skręt; boki: bieg bokiem z przechyłem; tył: bieg w tył z wypchnięciem broni/tarczy; czas nagrania prowadzony drogą z gry (zapętlony bieg, sprint i wyhamowanie w 0,6 s); 25 klatek (0,82 s); biblioteka: `wrap`, `head_counter`, `torso_roll`; testy PlayMode 59/59;
  - w grze (topór): dłonie na trzonku przez cały unik (przewrót: lewa puszczała 78 cm), skok pozy 7–9 m/s (przewrót 14–16); podgląd `TestResults/berserker-anim-review/podglad_szarza.html`.
  - Regresja wykryta testem paladyna (`TwoHandCombo_DoesNotDropIntoCrouch`, miecz dwuręczny): lekkie cięcia opuszczały biodra o ~22 cm (limit 18) – spłycone (lekki 1 bez dodatkowego przysiadu, skłon 6°; lekki 2 – wykrok nagrania 10 cm płycej). PlayMode 59/59 (12 galerii pominiętych), EditMode 275/278 (te same 2 znane).
- **Ciężki 2 i ataki z biegu** (bez obrotów ciała; źródła `ArtSource/AnimationRepair/heavy-chop/spec_heavy2.py`, `spec_leap.py`, `spec_sweep.py`; menu **Turris → Naprawa animacji → Ciężki 2 i ataki z biegu**, także `HeroAssetImport.FollowAndRunningMoves`):
  - ciężki 2 (ChopFollow): wersja bez obrotu (krok dostawny, `ArtSource/.../heavy-chop/spec_heavy2.py`) **odrzucona przez użytkownika** („atk_heavy2 był dobry, a ty go zmasakrowałeś”) – przywrócony oryginał „great sword high spin attack” (0/30/37/56, profil GreatSwingPose); pomiary w grze identyczne ze stanem wyjściowym;
  - ciężki z biegu (LeapChop) `Composed/great axe leap chop.fbx` (dawniej styl bazowy Leap – skok z obrotem): przysiad, lot (biodra +30 cm, nogi podkurczone, 50 cm w przód), lądowanie z rąbnięciem;
  - lekki z biegu (RunningSweep) `Composed/great axe running sweep.fbx` (dawniej „slash (2)” z zamachem za głową i obrotem miednicy −121°): z niesienia w biegu poziome cięcie na wysokości bioder przez przód i dociągnięcie nisko w lewo, krok lewą z nagrania, prawa zwykły krok ~75 cm, miednica ±40°;
  - kontakt w grze: ciężki 2 i skok – ostrze ~26 cm nad ziemią (ciężki 1 – 15 cm; inny rozkład skrętu nadgarstka w retargecie), lekki z biegu – skok pozy 28 m/s na starcie ciosu po biegu (do obejrzenia); podgląd `TestResults/berserker-anim-review/podglad_ciezki2_bieg.html`; PlayMode 59/59 (12 galerii pominiętych), EditMode 275/278 (te same 2 znane).
- **Trafienie i śmierć** (decyzje: topór wypada z dłoni; trafienie zależne od siły ciosu):
  - sterownik klipów ignorował kierunek ciosu (jeden klip trafienia – „great sword impact”, zaczynający się w przysiadzie; jedna śmierć – upadek na plecy). `AnimResolve.ReactionClip` wybiera wariant tylko przy szukaniu klipu (stan akcji zostaje Flinch/Death): nowe `AnimAction.FlinchHeavy/FlinchBack/FlinchBackHeavy/DeathForward` (na końcu enuma), bez klipu w zestawie – zwykły;
  - `anim_greatsword` (`HeroAssetImport.Reactions`, menu **Turris → Naprawa animacji → Trafienie i śmierć**): przód lekki – „sword and shield impact (3)”, przód ciężki – „great sword impact (3)”, tył lekki – „sword and shield impact (2)”, tył ciężki – „great sword impact (2)” (zgięcie w pół), śmierć od ciosu z tyłu – „two handed sword death (2)” (upadek do przodu); ciężkie z ruchem poziomym w pozie (import „w miejscu” zjadał odrzut);
  - w grze: odrzut głowy lekki/ciężki 20/35 cm (przód), tył – głowa do 1,30/1,03 m; śmierć z przodu na plecy, z tyłu do przodu;
  - `CharacterVisual.DropWeaponOnDeath`: przy śmierci broń wypada (gdy dotyka podłogi albo dłoń nisko, najpóźniej 1,8 s), kładzie się płasko na ziemi w 0,28 s (bez fizyki); korbacze zostają; po wskrzeszeniu wraca do gniazda; dawniej topór wchodził 74 cm w podłogę – teraz 1 cm nad nią;
  - podgląd `TestResults/berserker-anim-review/podglad_trafienie_smierc.html`; PlayMode (berserker, topór, animacje, modele, paladyn, skille, stopy, przebieg gry) 85/86 + 12 galerii – `PaladinAnimePlayTests.MatteArmor…` pada tylko po `GameFlowPlayTests` (stan renderowania, osobno 11/11); EditMode 275/278 (te same 2 znane).
- **Postawa z toporem** (uwaga: „wygięty do tyłu, topór nienaturalnie uniesiony i przylepiony, ręce jakby w tułowiu – powinny być przed nim”):
  - przyczyny (`GreatSwingPose` sprzed tej pracy): odchylenie tułowia o 16° do tyłu, prawa dłoń 0,2 m przed postacią przy przodzie tułowia 0,22–0,26 m przed osią kręgosłupa, topór prawie pionowo; krótkie ramiona berserkera (ramię + przedramię ≈ 0,39 m) – lewa dłoń nie sięgała trzonka (rozstaw 0,34 m), więc `HellEquipmentPose` cofał prawą dłoń razem z bronią do ciała;
  - nowa postawa: `GuardLean` −16° → −4° (zostaje ~6° naturalnego pochylenia w przód), `GuardHeadLift` −16° → −10°, `GuardHand` (0,10; 1,32; 0,52), `GuardAim` (0,45; 0,55; 0,70) – lewa dłoń przed klatką (najniżej, gdzie sięga przed tułów), prawa 0,34 m wyżej wzdłuż trzonka, ostrze w przód i w prawo na wysokości głowy; `GuardShoulderForward` 22° – obojczyki w przód (więcej zasięgu); dotyczy spoczynku i chodu z toporem/młotem;
  - audyt postawy mierzy dłonie przed klatką; test `GreatSwing_ChopReachesTheGround…` sprawdza nową postawę (ostrze w górę i w przód, chwyt przed tułowiem); PlayMode 59/59 (12 galerii pominiętych); podgląd `TestResults/berserker-anim-review/podglad_postawa_toporu.html`.
  - **zastąpione niską gardą** (wzór użytkownika z Elden Ring: topór oburącz nisko, trzonek poziomo przed pasem, głowica z prawej ostrzem w dół): `GuardLean` −4°, `GuardHeadLift` −10°, `GuardHand` (0,30; 0,96; 0,34), `GuardAim` (0,97; −0,08; 0,12), nowe `GuardBlade` (0; −0,9; 0,4) – obrót topora wokół trzonka (ostrze w dół), `GuardShoulderForward` 6°; w grze chwyt P (0,28; 0,98; 0,34), lewa dłoń na trzonku 34 cm dalej (0 cm od osi); test sprawdza teraz: trzonek prawie poziomo (|y| < 0,3), głowica w prawo (x > 0,8), dłonie na wysokości bioder (0,85–1,10 m), chwyt > 0,25 m przed postacią.
- **Proporcjonalne ręce berserkera** (uwaga: „ma mieć ręce proporcjonalne jak człowiek, a nie jakieś krótkie”): siatka miała rozpiętość 1,43 m przy wzroście 1,80 m. W `berserker_anime_rigged.blend` ramię ×1,22, przedramię ×1,40, dłoń ×1,20 (wierzchołki według wag kości ręki) – rozpiętość 1,69 m; to samo dopisane w `tools/art/rig_anime_berserker.py` (gałąź nie-mag, całego skryptu nie uruchamiano ponownie). Kopie: `berserker_anime_rigged_v2_krotkie-rece.blend`, `berserker_anime_v2_krotkie-rece.fbx`; eksport `berserker_anime_v3_dlugie-rece.fbx` → `Models/PoC/BerserkerAnime/berserker_anime.fbx`.
  - **Pułapka:** Humanoid ustawia kości według szkieletu zapisanego w `.meta` (`humanDescription.skeleton`), więc po samej podmianie FBX ręce w grze dalej miały stare długości (22,4/16,3 cm). Nowa metoda `BerserkerAnimeImport.RefreshSkeletonBatch` (menu **Turris → PoC → Berserker anime → Odśwież szkielet awatara**) przepisuje tylko szkielet awatara z zaimportowanego modelu – bez nadpisywania materiału i wyglądu jak `Apply`. Po niej ramię 27,3 cm, przedramię 22,8 cm.
  - wyniki: PlayMode 167/168 (58 pominiętych: galerie i boty) – jedyna porażka `FrameCostPlayTests.Floor1Combat_FrameWorkFitsBudget`: w `gfx` znany problem `WaitForEndOfFrame`, w trybie `window` mediana 19,35 ms przy limicie 16,7 (rycerz, ekran 2560×1248, otwarty Blender; brak wcześniejszego pomiaru do porównania – do sprawdzenia na czystej maszynie); `BerserkerAnimePlayTests` (błąd chwytu < 2,5 cm) zielone z dłuższymi rękami; EditMode 275/278 (te same 2 znane). Podgląd: `TestResults/berserker-anim-review/podglad_postawa.html` (oryginał / niska garda z krótkimi rękami / teraz).
  - **Korekta** (uwaga: „skróć odrobinę część z bicepsem i tricepsem… dłonie mają się zaciskać na trzonku”): ramię ×1,10 zamiast ×1,22 (24,6 cm; przedramię bez zmian 22,8 cm; rozpiętość 1,64 m); kopia poprzedniej wersji `berserker_anime_rigged_v3_dlugie-ramie.blend`, eksport `berserker_anime_v4_krotsze-ramie.fbx`.
  - **Kciuk** był w ~84% przypięty do przedramienia (podział wag po osi X; pętla zginania palców w `rig_anime_berserker.py` go więc pomijała) – przy obrocie dłoni do chwytu zostawał z przedramieniem i sterczał jak otwarta dłoń. Przypięty do dłoni i podgięty o 40° pod pięść (obie dłonie); dopisane w skrypcie rigu.
  - **Rozstaw dłoni** `HellEquipmentPose.HeavyGripSpan` 0,34 → 0,28 m: trzonek topora kończy się 33,6 cm za gniazdem prawej dłoni, więc lewa pięść obejmowała sam koniec (pół w powietrzu). Dotyczy też ataków toporem/młotem (pomiar rozstawu w audycie postawy).
  - **Chwyt jak na wzorcu** (uwaga: „dłonie nadal źle zaciśnięte… trzymasz topór za końcówkę”): audyt zmierzył nadgarstki – P skręt ~120° od neutralnego, L zgięcie 137° i skręt ~155° (siatka dłoni zapadała się w płaski kształt). Przyczyna: ostrze w dół ustawiano obrotem dłoni wokół trzonka, a lewa dłoń dostawała obrót z gniazda broni. Teraz `HellEquipmentPose.NaturalWrist` obraca dłoń wokół trzonka do ułożenia najbliższego pozie wiązania (z bindposes), resztę skrętu (do ±80°) bierze przedramię; ostrze w dół – obrót broni wokół trzonka (`GreatSwingPose.GuardSpin`, stosuje `CharacterVisual.GuardGrip`). W gardzie broń przesuwa się w dłoni o `GuardSlide` 0,32 m (prawa dłoń wyżej na trzonku), rozstaw `HellEquipmentPose.GuardGripSpan` 0,56 m (lewa przy końcu), łokcie w dół i do tyłu, `GuardHand` (0,30; 0,95; 0,32), `GuardAim` (0,97; −0,03; 0,14). Wynik: nadgarstek P zgięcie 19° / 31° od neutralnego skrętu, L 16° / 0°. Wszystko ważone udziałem gardy – w atakach chwyt jak dawniej.
  - **Bieg** (uwaga: „w trakcie biegu powinien trzymać topór przed sobą w dwóch rękach… teraz ucieka na dół i w bok”): zamiast niesienia przy prawym boku (ostrze w dół i do tyłu) bieg ma chwyt z gardy – oburącz, trzonek poziomo przed pasem, głowica w prawo; `CarryHand` (0,28; 1,02; 0,38), `CarryAim` (0,95; 0,04; 0,30); podgląd `TestResults/berserker-anim-review/podglad_bieg.html`.
  - audyt postawy robi teraz też zbliżenie dłoni (`dlonie_*`, `dlonP/L_*`, `chwyt_*` z bronią i bez), mierzy zgięcie/skręt nadgarstków i rozmiar broni w gnieździe; i mierzy zasięg broni wzdłuż trzonka; PlayMode 167/168 (jedyna porażka – znany `FrameCostPlayTests` w `gfx`), EditMode 275/278 (te same 2 znane).
- Zostaje: przegląd wizualny innych wyglądów z mieczem dwuręcznym (klipy wspólne – liczby i testy zielone, galerii nie oglądano); ciężkie 1–2 i ładowany, cios po uniku, ciosy z biegu, szarża barkiem jako unik, przejścia atak → unik, trafienie/śmierć/powalenie, skille. Pętle „slash (3)” (topór za karkiem w zamachu, oparty o bark w powrocie) zostawione jako cecha nagrania – do oceny użytkownika.
- Notatki „co, jak, dlaczego”: [NOTATKI_NAPRAWA_ANIMACJI_BERSERKERA.md](NOTATKI_NAPRAWA_ANIMACJI_BERSERKERA.md).

### Wdrożenie 30 cech klasowych — 2026-10-31

- Według [promptu](PROMPT_WDROZENIE_30_CECH_KLASOWYCH.md) – pełny raport, tabela stanu 30 wariantów, decyzje i luki: [RAPORT_CECHY_KLASOWE.md](RAPORT_CECHY_KLASOWE.md).
- **Wszystkie 30 wariantów ma prawdziwą mechanikę** (kontrolery w `Scripts/Runtime/ClassFeatures/`), wybór A/B w przygotowaniu (karty, mysz/pad, podsumowanie), okienko cechy w HUD, pasek rezerwacji Czerwonej linii, dług many w HUD, zestaw II Arsenału w ekwipunku.
- Przycisk klasy (G / View) obsługuje cechę; **dawna sygnatura nie powstaje obok niej** (definicje zostają dla migracji). Profil v4 pamięta cechę per klasa; snapshot zapisuje cechę, dług i zestaw II; stary zapis → wariant A (Berserker z `hu_undying` → Czerwona linia), pieczęcie sygnatury wracają do sakwy.
- Rdzeń walki: sufit życia, filtr spłaty i nadwyżka leczenia w `ResourcePool`; pochodzenie trafień (`originId`, `featureEffect`…); alias trafienia (cień); rodziny podpaleń i znacznik Szronu w `StatusEffects`; po stronie wroga wabik, wspólny predykat egzekucji, odwrócone aury, zderzenia przy odrzucie, ściana z obchodzeniem; chimera w `Servant`.
- Decyzja do wiadomości: **własny płomień Żywego pieca nie zdejmuje Arkanowej osłony** (inaczej osłona pochłaniała cały koszt i wariant był martwy); paliwo ograniczają odporność na ogień i bariera.
- Testy: EditMode **275/278** (2 znane „AnimeForward”, 1 pominięty), nowe `ClassFeatureTests` 25; PlayMode `ClassFeaturePlayTests` **22/22**; galeria `ClassFeatureGalleryTests` (zrzuty obejrzane); scenariusze botów `ClassFeatureBotScenarios` (`TURRIS_FEATURE_BOTS`) – 24/30 wariantów użytych przez boty w 45 s, potem poluzowano warunki pozostałych.
- Pełny PlayMode w trybie `gfx`: **167 zaliczonych, 1 niezaliczony, 55 pominiętych** (galerie/pomiary na żądanie) – jedyny błąd `Floor1Combat_FrameWorkFitsBudget` czeka na `WaitForEndOfFrame`, który nie działa w trybie wsadowym (znana pułapka). W trybie `-nographics` przebieg kończy się awarią URP w niezwiązanym teście renderującym (`AnimeHeroStylePlayTests`).
- Zrzuty interfejsu w trybie `window` (`ClassFeatureUiShots`) obejrzane: karty A/B, podsumowanie, HUD cechy, rezerwacja Czerwonej linii, dług many, zestaw II Arsenału; poprawiono opis klasy (bez dawnej sygnatury) i położenie napisu długu.
- Zostaje: warstwa animacji dla Tańca bez przerwy i Dwóch taktów, dźwięki cech, serie botów po wznowieniu (strojenie `ClassFeatureTuning`), build gracza.

### Hell Unleashed 7.1 i 7.2: przedmioty, ekonomia, żywioły, reakcje — 2026-10-31

- Pełna lista i stan: [plan migracji, rozdz. 7.1–7.2](PLAN_MIGRACJI_HELL_UNLEASHED.md#71-przedmioty-bronie-affixy-perki-ekonomia).
- **Przedmioty:** statystyki HU bez odpowiednika jako efekty pasywne (`HellEconomy.MapEffect`) – uchylenie lekkich/średnich zbroi, dusze, życie/mana za zabójstwo, przebicie…; wszystkie 131 affixów (`ItemInstance.affixEffects`); moce 17 unikatów zbroi i biżuterii (`HellEconomy.PowerEffect`); jakość skaluje moce.
- Nowe efekty: `SoulGain`, `ManaOnKill`, `ProjectileSpeed`, `Accuracy` (czeka na unik demonów), `SoakedOnHit` (na końcu enuma).
- **Ekonomia:** sprzedaż za 35% wartości (na postoju, nie broń w rękach), plecak 8 miejsc (łup przepada, zakup/zdjęcie blokowane), oferta poziomu tarczy (3 poziomy), podgląd „Teraz:” w sklepie, księga obrażeń (`Run/DamageLedger.cs`) na ekranie śmierci i zwycięstwa.
- **Bronie HU:** odrzut (także pocisk zwykłego ataku), rykoszet różdżki powietrza, zasięg ciosu względem broni wzorcowej szablonu (0,8–1,25×).
- **Żywioły i reakcje:** drugi żywioł w trafieniu (`HitData.element2/element2Share`; skille HU i demony HU), Para (`Combat/SteamCloud.cs`: 50% chybień demonów, tylko od ciosów bohatera), wybuch żaru w obszarze 1,8 m, pęknięcie pancerza obniża odporność fizyczną wrogów, koszt skilli HU z parametrów poziomu, podpalenie 2 s.
- Assety: menu „Turris → Hell Unleashed → Przedmioty, perki i żywioły (7.1, 7.2)” (`HellMigrationImport.MigrateItemsAndElements`) – 70 assetów, tylko pola z portu. `GameConfig.asset`: `burnDuration` 2.
- Testy: EditMode 248/251 (2 znane „AnimeForward”, 1 pominięty), nowe `HuPortTests` (10). PlayMode wsadowo (HeroSkill, HuSkillCaster, GameFlow, SealFlow, Demon): 52/57, 3 pominięte; `BestiaryPreview…` – RenderTexture w trybie bez grafiki, `ParriedCharge…` – niestabilny w serii, osobno 2/2 zielony.
- Boty sprzedają po przymierzeniu wszystko, co zostało w plecaku (gorsze od założonego, bronie i druga ręka), raport: „sprzedany (…): nazwa”; ocena buildu bota liczy efekty HU (`HeroBot.EffectScore`).
- **Unik demonów i celność** (HU): demony HU unikają zwykłego ciosu i pocisku bohatera (0–18%), celność ze Zręczności (2,25%/pkt ponad próg) i przedmiotów; testy bez uniku (`NoEvasionSetUp`), boty z unikiem. Assety: ta sama migracja (19 demonów).
- **`LoadoutAnalysis`** w sklepie: panel „Twój build” (DPS zwykłego ataku i skilli), ostrzeżenie o perkach zasypiających po zmianie broni i o perku bez działania. Zrzut sklepu sprawdzony.
- Testy po tym etapie: EditMode 250/253 (2 znane „AnimeForward”, 1 pominięty), PlayMode wsadowo 52/57 (te same 2 niezwiązane).
- Zostaje: `BossTrial`. Następne: seria botów (gracz i tester) po zmianach ekonomii i żywiołów.
### Kontynuacja ikon 75 unikatów — 2026-10-31

- Po przerwie autor polecił kontynuować. Obowiązuje uproszczony styl ikon i zapis plików w projekcie bez publikowania kolejnych podglądów w czacie.
- [Katalog grafik](../ArtSource/Unikaty/ProsteIkony_v1/README.md) ma plan wszystkich **75 kart**, osobne prompty, id klas i przedmiotów oraz jednoznaczne nazwy PNG. Aktualny postęp zapisany w manifest.json i rejestr_generowania/KOD.json; brak PNG jest jawnie oznaczony jako do_wygenerowania.
- Dodano narzędzia katalogowania i kontroli: tools/art/katalog_ikon_unikatow.py (w tym --wymagaj-kompletu) oraz tools/art/podglad_ikon_unikatow.py. Źródłowe PNG pozostają bez zmian. Lokalna galeria HTML i plansze TestResults/unique-icons-v1 pokazują skalę 120/64/32 px.
- Pierwsza kontrola małej skali: sylwetki pierwszej partii są czytelne przy 64 px; 32 px zachowuje główne bryły, ale traci detale. To kontrola planszy, bez integracji z właściwym UI gry. Testów Unity nie uruchamiano, kod gry i modele 3D bez zmian w tym zadaniu.
- W chwili wznowienia partii zbiorczej zapisano **11/75** źródeł; kolejne generowane i rejestrowane partiami. Pełny wynik i dalsze uwagi należy odczytać z manifestu, nie zakładać ukończenia z samej obecności planu.

### Grafiki unikatów: trzy proste ikony źródłowe — 2026-10-31

- Po obejrzeniu zapisanych zrzutów broni, Berserkera i sklepu przygotowano próbki grafik ekwipunku: **W01 Pawęż ostatniego zakonnika, P01 Relikwiarz głodnego świętego, B03 Topór kroczącej rzezi**.
- Aktualna dyspozycja autora: **małe, proste ikony**, bez dużej szczegółowości; pierwsza szczegółowa pawęż odrzucona. Kolejnych grafik **nie wyświetlać w czacie**, zapisywać w odpowiednim folderze z jednoznacznymi nazwami.
- Pliki: [ArtSource/Unikaty/ProsteIkony_v1](../ArtSource/Unikaty/ProsteIkony_v1/README.md). README opisuje styl i stan, manifest JSON wiąże PNG z kartami i id przedmiotów, PROMPTY.md zawiera faktyczne prompty. Generowanie wbudowanym image_gen.
- **3 próbki z 75, bez wdrożenia w UI i bez modeli 3D.** Źródła PNG RGBA 1254 × 1254 z kanałem alfa 0–255; obejrzane, sprawdzone parametry plików. Docelowy rozmiar wyświetlania 32–64 px; małych eksportów i oceny w docelowym UI jeszcze nie wykonano.
- Obecne ItemDefinition nie ma pola Sprite ikony, ekwipunek jest głównie tekstowy. Unity MCP na porcie 8099 bez połączenia, więc bazą oceny były zapisane zrzuty, nie bieżący edytor. Testów Unity nie uruchamiano; kod i istniejące assety gry bez zmian w tym zadaniu.
- Następne kroki: dalsze ikony w uproszczonym stylu według kart 75 unikatów, eksport/import docelowy i sprawdzenie małej skali; osobno integracja danych/UI oraz ewentualne modele broni 3D.

### Prompt wdrożenia 75 unikatów — 2026-10-31

- Na polecenie autora rozszerzono zaakceptowane 45 pomysłów o **dwa nowe unikaty dla każdej z 15 klas**:
  łącznie **75 przedmiotów, po pięć na klasę**. Wcześniejsze nazwy i główne moce zachowane; 30 nowych kart oznaczono NOWY.
- Gotowy [szczegółowy prompt wdrożeniowy](PROMPT_WDROZENIE_75_UNIKATOW_KLASOWYCH.md): id, sloty, statystyki kierunkowe,
  moce, koszty, synergie i testy każdej karty; natywne Unique, łup/ekonomia, zapis, sterowanie, oprawa, boty,
  jawna kompozycja przemian, konflikty wyposażenia i kryteria odbioru.
- Powiązana zależność: poprzedni prompt 30 cech klasowych. Unikaty nie przyznają drugiej ani cudzej cechy.
  Wykonawca ma sprawdzić stan ich rzeczywistego wdrożenia, zamiast zakładać, że opis oznacza istniejący kod.
- Kontrola dokumentu: **75 unikalnych id, 75 pełnych kart, 15 grup po 5, 30 nowych, wszystkie 45 wcześniejszych nazw,
  zgodność 15 id klas z istniejącymi assetami**, poprawne bloki i brak błędów formatowania tekstu.
- W tej pracy przygotowano **wyłącznie prompt, bez implementacji mocy i nowych assetów gry**. Testów Unity nie uruchamiano.
  Następny krok: wykonanie promptu wraz z zależnościami; wstrzymanie wielogodzinnych serii botów pozostaje zachowane.

### Audyt HU → Turris i trzy poprawki: unikat z łupu, przemoczenie/spowolnienie, limit reakcji — 2026-10-31

- **Audyt** (skille/pieczęci, przedmioty/ekonomia, walka/wrogowie). Skille i pieczęci przeniesione w pełni. Pełna lista braków: [plan migracji, rozdz. 7](PLAN_MIGRACJI_HELL_UNLEASHED.md#7-co-jeszcze-nie-zostało-zmigrowane-audyt-2026-10-31). Główne luki do decyzji:
  - `HellEconomy.Map` gubi statystyki bez odpowiednika (uchylenie, przebicie odporności, dusze, życie/mana za zabójstwo, odrzut, pociski…). Lekkie i średnie zbroje bez uchylenia, `item.ring.gold` pusty, 27/131 affixów nie wypada.
  - 17 unikatów zbroi i biżuterii bez mocy (`ItemPower`); brak sprzedaży przedmiotów i limitu plecaka; cechy broni HU (odrzut, rykoszet, łuk, zasięg) nieczytane.
  - Brak Pary (chmura, 50% chybień demonów); wybuch żaru tylko w jeden cel (HU: obszar 1,8 m).
  - Wrogowie: brak biomów, poise bohatera, uniku demonów, wariantów żywiołowych, odblokowania specjałów wg fali i losowego pierwszego odnowienia, movesetu broni; uproszczone m.in. Flood, Petrify, Tether, FrontGuard, elity.
- **Unikatowa broń z łupu** traciła status (`HellEconomy.Roll` rozpoznawał unikat tylko w items.json): teraz `HellEconomy.IsUnique` (także `.unique.` z katalogu broni), wspólne dla łupu i sklepu.
- **Przemoczenie i spowolnienie HU** nie są już chłodem (3 warstwy → zamrożenie; strefy wody, Sidła, Deszcz strzał zamrażały po ~1,5 s):
  - nowe `StatusKind.Soaked` (znacznik 4 s) i `StatusKind.Slow` (ruch ×0,65, 2 s, bez narastania); `BalanceConfig.soakedDuration/soakedAirBonus/slowMoveMultiplier/slowDuration`;
  - błyskawica w przemoczony cel +35% (PRZEWODNICTWO, HU SoakedAir); błoto także na przemoczonym; `bonus_vs_soaked` działa na przemoczonych;
  - skille HU (`PlayerCombat.HuStatuses`), demony i bronie HU (`HellEnemyCatalog.StatusFor`). Assety: menu „Turris → Hell Unleashed → Przemoczenie i spowolnienie w assetach” (`HellMigrationImport.MigrateSoakedSlow`) – 28 wpisów w 6 demonach HU i 2 broniach wody, reszta bez zmian.
  - Bez zmian: chłód skilli mrozu Turris; perk „Mokre ostrze” nadal daje chłód (`PassiveEffectType.ChillOnHit`).
- **Reakcje jak w HU:** tylko bezpośrednie trafienie – tyknięcia stref i aur (`HitData.isGroundHazard`, `DamageZone`, `AuraEffect`) nakładają efekty, ale nie wywołują reakcji; ta sama reakcja na celu najwyżej raz na `reactionCooldown` = 1 s (dotyczy też przewodzenia i roztrzaskania Turris).
- Testy: EditMode 238/241 (2 znane „AnimeForward”, 1 pominięty), nowe: 5 w `StatusEffectTests`, 1 w `EconomyTests`. PlayMode (HeroSkill, HuSkillCaster, GameFlow, SealFlow, okno): 42/47 – znane niestabilne `Impact_Hitstop…`, `EveryHuSkillAndForm` (Krok wichru); `Charge_…ShieldBash…` i `Dash_…` padają w oknie przez skoki klatki, osobno w trybie wsadowym/pojedynczo zielone.
- Następne: seria botów maga wody/powietrza (spadek kontroli tłumu), potem luki z audytu wg decyzji autora.

### Prompt wdrożenia 30 wybieranych cech klasowych — 2026-10-30

- Autor zaakceptował **wszystkie 30 wariantów**: po dwie wyjątkowe cechy dla każdej z 15 klas; gracz wybiera jedną
  przed podejściem. Wybór zajmuje dotychczasowy przycisk klasy, zachowuje trzy zwykłe sloty i bazowe mechaniki klas.
- Gotowy, samodzielny [prompt wdrożeniowy](PROMPT_WDROZENIE_30_CECH_KLASOWYCH.md): pełne karty, wyłączność,
  przygotowanie i pad, migracja profili/snapshotów, kontrakty zasobów i trafień, oprawa, boty, telemetria,
  30 szczególnych przypadków testowych, etapy wykonania i kryteria odbioru.
- Doprecyzowanie autora: **Czerwona linia Berserkera połączona z Żelazną skórą — im mniej życia, tym większa ochrona**.
  W promptcie roboczo rezerwacja do 35% normalnego maksimum i podczas transu redukcja 0,8 × (1 − PŻ / normalne maks. PŻ);
  przy 35% życia 52%, przy 10% 72%. Zastępuje dawną stałą redukcję, bez dodawania drugiego mnożnika 80% i bez ochrony przed śmiercią.
  Liczby są hipotezami do strojenia, a nie wynikiem pomiarów.
- **W tej pracy przygotowano tylko prompt, bez wdrożenia mechanik w kodzie i assetach.** Kontrola dokumentu:
  30 kart, 15 par A/B, 30 unikalnych id i zgodność 15 id klas z istniejącymi assetami; poprawne bloki i format tekstu.
  Testów Unity nie uruchamiano — nie zmieniono wykonania gry.
- Następny krok: wdrożenie według promptu; aktualne wstrzymanie wielogodzinnych serii botów pozostaje zachowane.

### Zestaw ruchów topora i młota dwuręcznego (wzór: Battle Axe z Dark Souls 3) — 2026-10-30

- **Źródło:** film „Battle Axe Moveset (Dark Souls 3)”, sekcja dwuręczna 0:38–1:23. Pobrane klatki (5 kl./s) rozpisane na ruchy: lekkie, ciężki, ładowany, po odskoku i przewrocie, lekki i ciężki z biegu.
- **Gra** (`PlayerCombat.Moveset.cs`, tylko `WeaponModel.GreatAxe` i `Hammer`, bez drugiej broni):
  - ciężki po ciężkim to seria (`ActionController` liczy serię także dla ciężkich): `Chop`, czyli rąbnięcie z góry z wykrokiem, na przemian z `ChopFollow`, czyli rąbnięciem zza pleców z obrotem;
  - przytrzymany mocny atak (`PlayerInputReader.HeavyHeld` → `PlayerCombat.HeavyHeld`): zamach stoi w 80% z bronią nad głową, najwyżej 0,8 s. Od 0,25 s cios jest naładowany: do +60% obrażeń i +50% postawy, a na początku powrotu wyzwala falę (50% obrażeń, 2,4 m, trafia tylko tych, których ostrze ominęło);
  - z biegu (do 0,2 s po biegu):
    - lekki `RunningSweep`: pierwsze cięcie serii „great sword slash (2)”, klatki 0–44, +10%, rozpęd 3,5 m/s;
    - ciężki `Leap`: „jump attack”, +20%, +30% postawy, rozpęd 4,5 m/s i fala;
  - lekki w końcówce uniku albo do 0,35 s po nim to `DodgeChop`, czyli rąbnięcie z góry z zamachem ×0,8.
- **Animacje:** nowe style `AttackAnim` są dopisane na końcu enuma (`Chop`, `ChopFollow`, `RunningSweep`, `DodgeChop`). Bez własnego klipu używają stylu bazowego (`AnimResolve.Base`: rąbnięcia → `Overhead`, cięcie z biegu → `SlashRight`). Dotyczy to też proceduralnego humanoida.
  - Klipy dopisuje `HeroAssetImport.AddGreatWeaponMoves` (menu „Turris/Zestaw ruchów topora i młota”) do `anim_greatsword`, bez przebudowy wyglądów; to samo jest w `GreatswordSet()`.
  - Klatki ciosu wyznaczone z prędkości dłoni (`GreatWeaponMovesetTests.ClipHandSpeed_WhenRequested`). „slash (5)” odrzucony: dłoń najwyżej 3,3 m/s, przy cięciach ok. 8 m/s.
- **Weryfikacja:**
  - galeria `GreatWeaponMovesetTests.Moveset_WhenRequested` (8 ruchów, bok i zza pleców, czasy topora z gry) porównana klatka w klatkę z filmem;
  - zgodne: ładowany (topór nad głową, potem rąbnięcie), ciężki z biegu (skok z rąbnięciem), lekki z biegu (niskie cięcie z pochyleniem), lekkie (szerokie poziome cięcia), ciężki (rąbnięcie z wykrokiem);
  - różnice: drugi ciężki ma obrót, którego w DS3 nie ma (płytszy koniec ciężkiego poprawiony niżej).
- **Testy:**
  - `GreatWeaponMovesetEditTests` 5/5;
  - EditMode 232/235: 2 znane porażki drugiego agenta (`GothicArenaTests.Materials…`, `PaladinAnimeTests.Avatar…`), 1 pominięty;
  - PlayMode 166/196: 29 pominiętych (boty i galerie bez zmiennych), 1 porażka `FrameCostPlayTests.Floor1Combat_FrameWorkFitsBudget`, bo `WaitForEndOfFrame` nie działa w `-batchmode`; to środowisko, nie zmiana.
- **Poprawka „ciosy urwane w połowie”** (uwaga użytkownika: „wygląda jakby wojownik bał się uderzyć”):
  - przyczyna: klipy Mixamo z paczki miecza mają krótki łuk. Rąbnięcie kończyło się z ostrzem przy biodrze (najniższy punkt topora 0,69 m nad ziemią), cięcie przed ciałem, a postać od razu wracała do gardy. Do tego wspólny profil faz grał samo przejście ostrza wolniej niż nagranie, a resztę cięcia ściskał w pośpieszny powrót;
  - `GreatSwingPose` (po `BodyDynamics` w `ClipAnimationDriver`) dla ciosów topora i młota:
    - głębszy zamach (odchylenie albo skręt od celu), potem tułów i biodra idą za ciosem;
    - rąbnięcia i skok z biegu: skłon 30–34°, przysiad 0,18–0,22 m (nogi IK do stóp z klipu), prawa dłoń IK przed postacią, trzonek z gniazda broni skierowany w dół i w przód. Ostrze dochodzi do ziemi (0,02 m) i nie wchodzi w podłogę;
    - cięcia: skręt przez cel na drugą stronę, topór kończy nisko po lewej (−0,66 m), drugie cięcie po prawej (+0,47 m);
    - poza końca trwa przez 35% powrotu, potem łagodnie wraca do klipu (wygładzenie 0,045 s, bez skoków w serii);
  - nowe style: `Cleave`/`CleaveBack` (lekkie cięcia topora; miecz dwuręczny zostaje przy `SlashRight`/`SlashLeft`) i `LeapChop` (ciężki z biegu). Klipy ruchów topora mają `strikeShare = 1`, więc całe przejście ostrza mieści się w fazie aktywnej, w tempie nagrania;
  - test `GreatWeaponMovesetTests.GreatSwing_ChopReachesTheGround_CleaveCarriesThrough`; zestawienie z filmem w galerii `Moveset_WhenRequested`. Trafienia liczy logika (kapsuła), więc zasięg ciosów się nie zmienił.
- **Własna postawa topora i młota** (uwaga użytkownika: „siekiera czy młot to nie miecz, zasługują na inne animacje”):
  - wcześniej stały w spoczynku miecza dwuręcznego (`great sword idle`): broń w poprzek piersi, postać zgarbiona, stopy razem;
  - `WeaponAnimSet.greatWeaponIdle` = początkowo „sword and shield idle (4)” (szeroki rozkrok, wyprostowana pierś; później zmieniony – niżej), wybrany z 8 kandydatów (`GreatWeaponMovesetTests.IdleCandidates_WhenRequested`). `ClipAnimationDriver(…, greatWeapon)` bierze go zamiast `idle`; zestaw `anim_greatsword` zostaje ten sam (miecz bez zmian);
  - `GreatSwingPose.Guard`: w spoczynku i chodzie obie dłonie na trzonku przed prawym biodrem, broń pionowo ostrzem w górę obok głowy (pion 0,93, 0,29 m w prawo); w biegu broń niesiona przy prawym boku, ostrzem do tyłu (wcześniej sterczała poziomo w bok z klipu miecza). Cios i unik prowadzi klip, przejście 0,12 s;
  - galeria `GuardInMotion_WhenRequested` (spoczynek, chód, bieg); asercje postawy w `GreatSwing_ChopReachesTheGround_CleaveCarriesThrough`.
  - poprawka chwytu (uwaga: „dłonie obok siebie, powinien być odstęp”; dłonie przy kroczu wyglądały źle): `HellEquipmentPose.HeavyGripSpan = 0,34 m` (wcześniej 0,16 jak rękojeść miecza) dla topora i młota we wszystkich ruchach; w postawie prawa dłoń przy prawej piersi (1,18 m), lewa przy pasie, broń odsunięta w prawo i na zewnątrz, żeby trzonek nie zasłaniał twarzy (pion 0,94, 0,35 m w prawo). PlayMode animacji/chwytu 45/45.
  - nogi w spoczynku (uwaga: „dziwne rzeczy niezgodne z anatomią w nogach”): „sword and shield idle (4)” miał szermierczy wykrok – miednica obrócona o 56°, stopy niemal w jednej linii (0,27 m w poprzek, 0,86 m przód–tył), prawa pięta uniesiona; z przodu nogi wyglądały na skrzyżowane. Teraz `greatWeaponIdle` = „great sword idle (3)” (miednica 4°), a `GreatSwingPose` w spoczynku: stopy na szerokość barków (z rigu), lewa 0,14 m z przodu, biodra w górę o tyle, by nogi zachowały wyprost z klipu (`PlaceFeet`); podeszwa poziomo wg punktów buta z siatki (`LevelSole`, oś kości palców tego riga jest krzywa); uniesiona stopa schodzi do poziomu drugiej (`LevelFeet`); plecy prostowane o 16°, głowa uniesiona o 16° (klip garbi się i patrzy w ziemię) – tylko w staniu, chód ma własną postawę. Broń odsunięta dalej w prawo (twarz odsłonięta). Galeria `IdleCandidates_WhenRequested` loguje położenie i wysokość stóp, obrót miednicy; `TURRIS_IDLE_LEGS=1` – zbliżenie na nogi. Uwaga: w klipach spoczynku spód buta berserkera jest ok. 6–8 cm pod poziomem postaci (nie ruszane – postać nie podskakuje między spoczynkiem a chodem). EditMode 232/235 (2 znane porażki drugiego agenta), PlayMode animacji/modeli/ruchów 46/47 (1 pominięty diagnostyczny).
- **Nie zrobione:** odpowiednik Okrzyku wojennego z DS3 (wzmocnienie i specjalne ciężkie ciosy) – mamy osobny skill Okrzyk. Boty nie ładują ciosów (nie trzymają przycisku); z biegu atakują same, bo mają bieg.

### Naprawione kostki 14 bohaterów — 2026-10-29

- **Wdrożone w grze**, po doprecyzowaniu, że celem jest także usunięcie wykrytych problemów:
  [naprawa z porównaniami przed/po](NAPRAWA_KOSTEK_BOHATEROW.md). Poprzedni raport audytu zachowuje stan sprzed zmiany.
- `AnatomicalFootPose` + flaga `CharacterVisualDefinition.anatomicalFeet`: zawias nogi w osiach danego rigu,
  stałe długości kości, orientacja stopy względem bieżącego kolana/goleni i kontrola podeszwy z kalibracji BakeMesh.
  BakeMesh jest wykonywany przy budowie modelu; w pętli nie ma kopiowania jego siatki.
  Zgięcie buta wyznacza ruch goleni — pierwsza wersja przejmowała skok retargetu u Zabójcy, wykryty nowym testem i poprawiony.
- Flaga zapisana przez Unity API do **14 aktywnych wyglądów**. Importery anime berserkera/maga i paladyna utrzymują ją
  przy imporcie. Zachowano poziomy tor kroków, pracę tułowia, chwyt broni i pełne akcje. Paladyn utrzymuje poprzednie prowadzenie
  nóg do przodu, z dodatkową kontrolą podparcia. Nie zmieniono globalnych mnożników yaw.
- **Wojownik bez zmian:** 19/19 hashy jego plików i wspólnych zestawów animacji zgodnych, 480 porównawczych próbek pozy
  bez różnicy. Modele/tekstury nie były generowane ponownie. Hell Unleashed nie był modyfikowany.
- Pełny przebieg po naprawie: **6/6 zaliczone**, 15 wyglądów (14 poprawionych + wojownik kontrolny), 117 profili,
  2106 przypadków ruchu i wszystkie 27 rzeczywistych zestawów startowych. Dane: `TestResults/hero-feet-repair/after`.
  Regresja modeli, postawy maga, ataków i kostek: **21 zaliczonych / 0 błędów / 5 opcjonalnych galerii pominiętych**.
  Dodatkowy przebieg zawiasów: **1/1**. Łącznie 28 udanych końcowych wykonań testów.
- Nowy `RetargetFootRepairPlayTests.B_AktywneRigi_SkretICiaglosc` działa również w zwykłym PlayMode:
  14 rigów, obie nogi, 30/60 kl./s, prawdziwe próbki wymachu i ciągłość obrotu; brak próbek powoduje błąd.
  Zebranie 56 wyników, min. 29 próbek nogi, maks. skręt ~11°. Diagnostyczne porównanie A jest opt-in.
- Domyślny bieg: P95 asymetrii wszystkich poprawionych wyglądów <2,4°; berserker ~35,7° → ~0,01°.
  Niezależny BakeMesh: badane punkty regionu stóp wszystkich 14 <7 mm zapadania (berserker do ~4,3 mm, wcześniej do ~10,2 cm).
  Ślizg to osobny wynik: np. paladyn P do ~3,7 cm, a część rigów nie ma stabilnych okien pomiarowych; nie deklarować idealnego
  zablokowania każdej podeszwy. Poziomy tor kroków i prędkości klipów pozostały zachowane.
- Obejrzano plansze wszystkich modeli i ujęcia z areny. Utrwalone obrazy, GIF-y, CSV i zbiorczy JSON:
  `docs/qa/hero-feet-repair-2026-10-29`. Unity pozostawione w trybie edycji; brak commita/pusha.

### Audyt anatomii 15 bohaterów — 2026-10-29

- Po poleceniu „działaj” wykonano [audyt kostek i podparcia](RAPORT_WERYFIKACJI_ANATOMII_BOHATEROW.md),
  zgodnie z wcześniejszym [planem](PLAN_WERYFIKACJI_ANATOMII_BOHATEROW.md). **Bez napraw modeli/animacji**,
  bez zmian wojownika i bez zapisu do Hell Unleashed.
- 15 aktywnych `ClassDefinition.Visual`, 27 natywnych zestawów + 90 dodatkowych profili;
  2106 przypadków ruchu, 352 665 próbek czasu po rozgrzaniu, minimum trzy pełne cykle;
  oba boki, przód/tył, szkielety i sekwencje 30/60 kl./s. Osobno 18 porównań aktywnych/starych rigów,
  BakeMesh co klatkę dla 15 modeli i rzeczywisty `PlayerCombat` na wszystkich 27 zestawach
  (36 690 próbek, po trzy lekkie uderzenia/strzały). Wybrane dowody zachowane w `docs/qa/hero-anatomy-2026-10-29`.
- Berserker: usterka odtworzona z toporem i młotem, także na starym rigu z tym samym zestawem.
  Asymetria wymachu P95 ~35,7° przy 60 kl./s, podobna przy 30. W aktywnym rigu `ApplyFootYaw`
  zmniejsza skręt L, lecz wprowadza skręt P; następne warstwy go nie usuwają. Nie jest to dowód wady samej siatki.
  Pomiar BakeMesh sygnalizuje osobno zapadanie badanych punktów butów do ~5,7/10,2 cm (L/P) na płaszczyźnie.
- Pozostałe rigi wymagają przeglądu według tabeli raportu. Paladyn utrzymuje poprawę skrętu w biegu do przodu
  (P95 asymetrii ~2,3°), ale ma osobne podejrzenie kontaktu podeszwy. Brak stabilnego okna podparcia
  oraz geometria zasłonięta szatą są **brakiem danych**, nigdy wynikiem 0 ani automatycznym zaliczeniem.
- Walidacja w otwartym głównym Unity: 6 różnych testów, **7 zakończonych wykonań / 0 błędów**
  (3 główne + 1 źródła + 2 podparcie/akcje + powtórzenie serii berserkera). Wyniki w `TestResults/hero-anatomy`.
  Testy sprawdzają zebranie danych, nie zdrowe kostki. Zachowane źródła: **8/8 hashy zgodnych**.
- Narzędzia: `HeroAnatomyAuditPlayTests` + `HeroAnatomyLiveRunner` (menu **Turris → Audyt anatomii**,
  opt-in), `review_hero_anatomy.py`, `summarize_hero_anatomy.py`. Obserwator kolejnych warstw w
  `ClipAnimationDriver` istnieje tylko w edytorze, z domyślnie pustym callbackiem; nie zmienia pozy.
- Pułapka MCP: domyślny klient może trafić do kopii bota; rejestr potrafią nadpisać Asset Import Workers.
  Sprawdzać `dataPath` przed wywołaniami, port właściciela głównego GUI i kompilację. Tutaj główny GUI miał port 8108.
  Stare połączenie klienta MCP zwolniono bez zamykania Unity i kopii botów. Helper `unity_karnel_mcp.py`
  odmawia pracy przy niezgodnym projekcie.
- Następne kroki po osobnym zleceniu: poprawa retargetu/referencji i korekty wymachu berserkera;
  testy bez luki „brak próbek = 0”, następnie pozostałe rigi osobno. Nie zmieniać globalnych wzmocnień stóp.

### Dusze bez premii, bot maga — 2026-10-29
- Premia +10% dusz cofnięta (seria v24 z darmowym punktem cechy wypadła bardzo dobrze: gracz 11/20).
- **Bot maga** (`Tests/PlayMode/Bots/MageBot.cs`, `MageBotPlaytests`, klasa `class_mage`):
  - walka na dystans: trzyma 4,5–9 m od celu, strzela różdżką w pasie dystansu, w tłumie albo za blisko odchodzi bokiem i ku środkowi areny,
    przyparty strzela z bliska; nova także na jednego wroga tuż obok;
  - Przeciążenie przed mocnym czarem (grupa ≥3 w 6 m albo boss, mana > 40%), potem pierwszy czar w zasięgu; flaszka many przy < 20% many w spokojnej chwili;
  - cechy: Inteligencja, co trzeci punkt Wytrzymałość (tester rotuje wszystkie); żywioły jak zestawy wojownika (tester po kolei, gracz epsilon-zachłannie);
  - sklep i przedmioty pod moc czarów, własny żywioł, mechanikę pocisków różdżki i przebicie własnego żywiołu; uśpione perki pomija.
  - **czary obszarowe wokół siebie** (nova, trzęsienie, młynek) – przy kilku wrogach w 10 m tylko na co najmniej dwóch w promieniu; na jednego wyłącznie
    w ratunku (życie < 35% albo < 55% i cios zaraz dojdzie) albo gdy w pobliżu nie ma nikogo innego. Inaczej wabi: przestaje uciekać, a gdy przy nim stoi jeden,
    a reszta dalej – mrugnięcie od środka grupy (nie pod ścianę); po 5 s bez skutku 3 s zwykłej walki. Telemetria: „czar obszarowy po zwabieniu / na grupę /
    na jednego (ratunek)”, „wabienie bez skutku”, powód uniku „wabienie pod czar obszarowy”. Próba 2 × 3: piętra 27, 16, 30 (zwycięstwo) i 16, 14, 30.
- Wspólne części przeniesione do `HeroBot`: `WantsSkill`, przełączanie aur (`OnFight`), wirtualne `Offense`, `PerkScore`, `GearScore`.
- Próba (`mag-proba`, 2 × 3, przed zasadą wabienia): piętra 8–11, giną głównie od Wiedźmy wichru. Seria `mag-v1` (2 × 20, z zasadą wabienia):
  - gracz 4/20 zwycięstw, średnio piętro 20,0, mediana 19; najlepszy ogień (12 podejść, śr. 23,5), reszta 13–17; zabójcy: Miotacz głazów 5, Taranowiec 2, Kowal Piekieł 2;
  - tester 2/20, średnio 15,0, mediana 14; ziemia śr. 20,6, ogień 17,2, woda 11,4, powietrze 10,8;
  - czary obszarowe na podejście (gracz): na jedynego wroga w 10 m 185, na grupę 43, po zwabieniu 3, w ratunku 3; wabienie bez skutku 12, mrugnięcia wabiące 5,5.
  - Decyzja: „kilku przeciwników” = wszyscy żywi na arenie (zamiast promienia 10 m) – nova na jednego tylko, gdy na arenie jest jeden wróg albo w ratunku.
- Porównanie żywiołów: `MageElementBotPlaytests` – specjalizacje `class_hu_mage_fire/water/air/earth` (skille klasy: Meteor, Wir głębin i Trzęsienie ziemi – nova, więc z zasadą wabienia –, Nawałnica).
  `mag-v2` (wabienie liczone na całej arenie): gracz 3/20, średnio 14,8, mediana 13 (13 podejść wodą – ogień wypadł słabo w próbach); tester 0/20, 14,2.
  Gracz na podejście: nova na grupę 22 (v1: 43), po zwabieniu 1,1, wabienie bez skutku 6 – wabienie rzadko się udaje i zabiera czas. `mag-zywioly-v1` (specjalizacje, 10 podejść, równolegle 4 kopie, 55 min): gracz/tester zwycięstwa – ogień 8/5, woda 8/4, ziemia 8/4, powietrze 6/3 (śr. piętro gracza 27,5 / 28,5 / 29,2 / 24,8). Specjalizacje są dużo silniejsze od bazowego maga (większe premie w klasie, np. +35% i dodatkowa statystyka), więc wynik `mag-v2` to głównie słabość `class_mage`, nie samego bota. **Mag powietrza – powalenie** (`mag-powietrze-v2`, 20 podejść): powalenie jest teraz ogólnym parametrem trafienia (`down_seconds` → `HitData.knockdown`, nakłada je `EnemyBrain` przy trafieniu; bossy odporne, potem 2 s odporności na ogłuszenie; strefy wtórne, ściany i aury go zerują). Nawałnica powala na 0,5 s, Nova powietrza na 0,6 s (`HuBalance`); nowa pieczęć wsparcia **Grzmot** (`seal.turris.thunder`, `TurrisSkills`): +0,7 s powalenia, 85% obrażeń, nie do aur, leczenia ani osłon. Wynik: gracz 15/20 (75%, śr. piętro 27,2; było 6/10 i 24,8), tester 6/20 (30%, bez zmian). Gracz przerywa picie rzadziej (73% zamiast 85%), nadal ginie głównie od Miotacza głazów. Potem `mag-powietrze-v3` (Nawałnica i Nova −10%): gracz 10/20 – za mocne cięcie; `v4` (Nova bez cięcia, Nawałnica −10%): gracz 13/20, tester 3/20 – zostaje.
- **Specjalizacje wojownika (29–30.10):** `wojownik-specjalizacje-v1` (preset `wojownik-specjalizacje`): gracz/tester zwycięstwa na 20 – Berserker 1/0, Rębajło 3/0, Paladyn 0/0; kontrola `wojownik-v25-kontrola`: wojownik bazowy 8/2 (wygrywa tylko miecz z tarczą + Cięcie wichru). Przyczyna: ataki obszarowe zwykłych wrogów (archetyp Ciężki i zdolności HU bez uniku) – bez tarczy rani 60–100% z nich, wyjście z zasięgu prawie nigdy się nie udaje. Zmiany po v1: bot omija znaczniki `DelayedStrike` i strefy `DamageZone` wrogów (`HeroBot.AvoidHazards`), bez tarczy unika zamiast blokować bronią (`wojownik-specjalizacje-v2-bot`: Paladyn 2/20, Rębajło 3/12 – przerwane brakiem pamięci, Berserker bez zmian); Nieśmiertelność → **Żelazna skóra** (3 s −80% obrażeń poza limitem, nie chroni przed śmiercią; `IronSkinTime`) – Berserker nadal 1/20 (`berserker-zelazna-skora`); broń ciężka tylko dwuręczna (jednoręczny korbacz/morgenstern bez rodzaju – Paladyn traci Rozpłatanie, Uderzenie gromu i skille broni ciężkiej, do przemyślenia); unik +10% (4,62 m), tarcza ×0,9 (`shieldDodgeDistanceMult`); **furia Berserkera**: do 20% mniej obrażeń (osobny mnożnik poza limitem) i do 10 życia/s, w szale jak pełna. **`wojownik-specjalizacje-v3`** (wszystkie zmiany powyżej, 20 podejść, 2 kopie, 75 min) – gracz/tester zwycięstwa (śr. piętro gracza): Berserker 2/0 (14,7; v1 1/0, 15,4), Rębajło 4/0 (17,5; v1 3/0, 15,2), Paladyn 2/0 (15,7; v1 0/0, 12,9); kontrola wojownika bazowego nadal lepsza (8/2, 21,0). Berserker zużywa o połowę mniej flaszek (11 zamiast 26 na podejście) i przyjmuje mniej obrażeń, ale ginie w seriach (29–36% HP w ostatnich 3 s, zawsze z flaszkami, ~20% picia przerwane) na piętrach 10–16 – furia nie przesuwa ściany. Tester Paladyna ginie na piętrze 8 (11/20, głównie Wiedźma wichru – „CycloneEye”). Najczęstsi zabójcy wszystkich: Miotacz głazów, Wiedźma wichru (CycloneEye), Kolos. **v4** (30.10): specjalizacje wojownika +10% szybkości ataku i +10% szybkości odnowień (`DefaultContent.Heroes` + assety klas) – gracz/tester (śr. piętro gracza/testera): Berserker 2/1 (16,2/15,0; v3 14,7/12,2), Paladyn 1/1 (11,7/11,3; v3 15,7/9,4 – v3 miał dwa podejścia do piętra 28, mediana 13→10), **Rębajło 10/0** (23,2/11,0; v3 17,5/15,5). Wszyscy giną z flaszkami; Paladyn ma 55% picia przerwanego trafieniem (Berserker ~20%) – kandydat na poprawkę: ochrona przy piciu. **Szarża** (`hu_charge`, `TurrisSkills.Charge`): 8 m w 0,4 s na wroga, w trakcie 50% mniej obrażeń (`dash_reduction`, osobny mnożnik poza limitem), uderzenie z szansą powalenia 40/50/60% (`down_chance` → `HitData.knockdownChance`); wymaga broni dwuręcznej albo tarczy (`BuildTag.TwoHanded`, `WeaponFamilies.Meets`), z tarczą kończy ją uderzenie tarczą (`shield_bash_down`: zawsze powala na 1,5 s). Asset zrobiony ręcznie (bez `RunBatchForce`). **`wojownik-specjalizacje-v5-szarza`**: gracz/tester (śr. piętro gracza) – Berserker 2/0 (18,7), Paladyn 2/0 (14,4), Rębajło 1/1 (11,4). Nowej Szarży boty prawie nie używały: w raportach „Szarża” to głównie stara technika `skill_charge` (ta sama nazwa, odblokowanie za popiół, dowolna broń – także jednoręczna bez tarczy). Spadek Rębajły (10/20 → 1/20) to zależność od ścieżki bota: w v4 gracz utknął przy „Dwa miecze + Cięcie wichru” (9/15 zwycięstw), w v5 przy „Korbacz i miecz + Rozpłatanie” (1/15). Potem (decyzja użytkownika): stara Szarża (`skill_charge`) usunięta – `unlock_charge` daje `hu_charge` (asset `skill_charge` skasowany, testy przepięte na `hu_charge`/`skill_lunge`/`skill_leapslash`); broń dwuręczna do walki wręcz (`BuildTag.TwoHanded`, `BuildSnapshot.twoHandedMelee`) +10% szybkości ataku (`BuildCalculator.TwoHandedAttackSpeed`) i **pancerz w ataku** (`PlayerCombat.HyperArmor`: lekki/ciężki atak i riposta w fazie zamachu i uderzenia – bez drgnięcia i −50% obrażeń, osobny mnożnik). EditMode 227/229 (2 znane testy grafiki), PlayMode `HuSkillCasterPlayTests` + zryw w `GameFlowPlayTests` zielone. **`wojownik-specjalizacje-v6-dwureczne`** (86 min, 2 kopie) – gracz/tester zwycięstwa (śr. piętro gracza/testera): **Berserker 6/0** (23,2/15,0; v5 2/0), **Paladyn 11/0** (24,2/10,4; dziewięć zwycięstw z rzędu), Rębajło 4/4 (14,3/14,3). Nowa Szarża daje 33–34% obrażeń gracza Berserkera i Paladyna (odblokowana w 19/20 podejść); Rębajło jej nie używa (jednoręczne bez tarczy – zgodnie z wymogiem). Nadal wszyscy giną z flaszkami (Paladyn-tester 54% picia przerwane, ginie na piętrach 8–10). **Picie tylko w bezpiecznej chwili** (zasada użytkownika dla wszystkich botów, obecnych i przyszłych): `HeroBot.SafeToDrink` – nikt nie zdąży trafić przed końcem picia + 0,3 s (`ThreatTime`: trwający zamach albo dojście na zasięg najszybszego ataku + zapowiedź, pocisk – zapowiedź + lot; ogłuszony wróg liczy `StateTimer`), żaden pocisk nie doleci, bohater poza strefami i znacznikami; inaczej `RetreatToSafety` – z 16 kierunków ten, gdzie wrogowie najdłużej nie dosięgną (bez ściany areny i stref), uniki co 0,45 s przy wytrzymałości >30%, potem bieg; po 5 s bez skutku 2,5 s walki. Usunięte awaryjne picie poniżej 12% HP. Mag: flaszka many też przez `SafeToDrink`. Serie kontrolne testerów (20 podejść; śr. piętro, flaszki/podejście, picie przerwane): v6 stare picie – Paladyn 10,4 / 6,5 / 54%, Rębajło 14,3 / 21 / 34%; `picie-bezpieczne-v1` (zapas 0,3 s, wrogowie ×1,3 szybsi, zamach ×0,8) – Paladyn 8,6 / 0,9 / 14% (za ostre – prawie nie pije); `-v2` (prawdziwe prędkości i zamachy, zapas 0,15 s) – Paladyn 11,3 / 2,2 / 0%, Rębajło 10,3 / 7,8 / 3% (15× ginie na Królowej Topieli); `-v3` (+ okno po ciosie wroga: `EnemyBrain.AttackRecoveryRemaining`, tylko odczyt) – Paladyn 9,4 / 3,7 / 5%, Rębajło 11,9 / 13,5 / 5%. Picie pod ciosem praktycznie zniknęło; wyniki testerów w granicach rozrzutu, ale nadal giną z flaszkami (Czerw wgryziony = nigdy bezpiecznie; boss stale naciska). Raport zapisuje przyczynę braku bezpiecznej chwili (`brak bezpiecznego miejsca na picie: wróg wręcz/strzelec/pocisk/strefa`) – prawie zawsze wróg wręcz. Dalej: `-v4` strząsanie Czerwia unikiem (`TryShakeOffLatch`), odwrót przy HP <25% do 15 s, ocena kierunku na 1 i 2 uniki – Paladyn 9,1, Rębajło 10,4; `-v5` tester nie przyjmuje ciosów celowo poniżej progu flaszki (`Experimenting`) – Paladyn 10,2, Rębajło 8,9; `-v6-diag` nowa telemetria `lowHpTime` (raport: „Niskie życie z flaszką w zapasie: … s – odwrót X%, pije Y%…”): przy niskim HP bot 48–56% czasu ucieka, 18–32% wychodzi z zasięgu, pije 5–12%, blokuje ≤1% – bezpiecznej chwili w walce prawie nie ma; `-v7` łyk w spokojnej chwili (brakuje ≥ tyle, ile leczy flaszka; flaszki wracają co piętro) – Paladyn 10,5 / 7,6 flaszki / zginął z flaszkami 19/20, Rębajło 13,0 / 25,5 / 14/19, przerwane picie 6%. Testerzy nadal giną z flaszkami głównie podczas odwrotu; serie botów wstrzymane na prośbę użytkownika. **`wojownik-specjalizacje-v7-picie`** (pełna seria z graczem, 70 min, 2 kopie) – gracz/tester zwycięstwa na 20 (śr. piętro gracza/testera): Berserker 6/0 (23,2/15,0; v6 6/0 23,2/15,0 – inne przebiegi, ta sama średnia), Paladyn 8/0 (20,6/9,1; v6 11/0 24,2/10,4), Rębajło 5/1 (17,4/15,5; v6 4/4 14,3/14,3). Picie przerwane trafieniem 4–9% (v6 ~22–54%), ale gracz nadal ginie z flaszkami w każdym podejściu (B 14/14, P 12/12, R 15/15); przy niskim HP 38–63 s na podejście, z czego pije 3–10%, reszta to odwrót i wyjście z zasięgu. Bezpieczne picie nie poprawiło wyników gracza (w granicach rozrzutu, Paladyn słabiej); tester Paladyna odpada zawsze na piętrze 8 lub 10. Nieśmiertelność przy uniku – odrzucone przez użytkownika. Serie puszczać na 2 kopiach (`--copies 2`, `--first-copy`) – 4 kopie + edytor wyczerpały pamięć.
- **Równoległe serie botów** (`tools/run_bots_parallel.py`): kopie projektu w `C:/GIT/Gry/Turris_Damnatorum_kopie/kopiaN` (robocopy /MIR Assets, Packages,
  ProjectSettings przed każdym uruchomieniem; Library kopiowany raz), zadania (filtry testów) rozdzielane na wolne kopie, wyniki do wspólnego `--out`
  (Unity: `--out/_unity/*.xml|log`). Zestawy: `--preset mag-zywioly | mag-powietrze | mag | wojownik | wojownik-specjalizacje`; `--runs`, `--copies` (domyślnie 4), `--realtime`, `--sync-only`.
  Pierwsze kopiowanie Library – przy zamkniętym projekcie głównym. Zasoby: ok. 7 GB dysku i 6–7 GB RAM na kopię. Telemetria powodów czarów obszarowych liczy na razie tylko skille ze slotów, nie skill klasy.

### Reszta Hell Unleashed: wszystkie perki, łup na ziemi, dojrzewanie dusz, cechy za dusze, balans 2.0 — 2026-10-29
- **Perki (46/46):** `DefaultContent.PerkContent` przenosi statystyki, konwersje i efekty trafienia; mechaniki to nowe `PassiveEffectType` (na końcu enuma).
  - Uśpienie jak w HU (`Run/PerkMechanics.Active`): perk bez pasującego wyposażenia (tagi slotu, `RequiresAny`: `perk:` / `tag:`) nic nie daje (`RunState.AllModifiers/AllEffects`).
  - Szeroki zasięg: promienie skilli Turris (`Radius`), skilli HU (`HuCast.area`) i wybuchów pocisków broni.
  - Refleks: +4% uchylenia ponad limit Zręczności, razem do 50%.
  - Tarcza: blok bierny 15% + 2% za poziom tarczy + perk; zatrzymuje cały bezpośredni cios (nie w gardzie), ładunek 1 / 0,92 / 0,85 s. Lekka tarcza: −15% kosztu bloku za poziom.
  - Zwykły cios wręcz: Długie ostrze (+10% zasięgu), Ciężkie uderzenia (odrzut 0,5 m, boss ×0,25, elita ×0,5), Rozpęd (+4% tempa za warstwę, do 5, pudło albo 1,5 s – zero),
    Rozżarzona stal (50% fizycznych → ogień), Głębokie rany (krwawienie, +1 warstwa, +25%). Pęknięcie pancerza – wszystkie ciosy wręcz, także technik.
  - Przebicie odporności (5 żywiołów): odejmowane od odporności wroga, nie poniżej 0 (`HitData.penetration`, `EnemyBrain.ReceiveHit`).
  - Pociski broni (`FireArrows`, `Projectile`): przebicie, rykoszet (7 m, bez spadku obrażeń), Rozszczepienie (łącznie 2/3/5/8, boczne 40%, od 8 pierścień), podział (2 × 40%, z wybuchem),
    Niestabilne pociski (wybuch 30/45/60% w każdym trafieniu, min. 1,5 m), Pozostałość (strefa 2 s, tylko z Niestabilnymi albo bronią wybuchową).
  - Odbicie: sparowany pocisk wroga wraca ×1,4 szybciej za 75% × poziom obrażeń. Przemiana w parę: ogień → mróz. Płonące / Mokre / Kamienne ostrze: podpalenie / chłód / zachwianie zwykłym atakiem.
- **Dusze dojrzewają** (`GameRoot.Souls.cs`): 3 s od zabójstwa; bezpośredni cios, który zabrał życie, niszczy 10% każdej porcji (raz na 1 s; tyknięcia, unik, blok, parowanie – nie).
  Koniec piętra dojrzewa wszystko. HUD: „(+N dojrzewa)”.
- **Łup na ziemi** (`Combat/LootPickup.cs`): przedmioty, pieczęci i dar bossa leżą, bohater zbiera je w promieniu 1,6 m; koniec piętra zbiera resztę.
- **Cechy:** zostaje darmowy punkt za piętro (decyzja użytkownika 2026-10-29) – kupowanie cech za dusze z HU wycofane; za dusze kupuje się resztę (broń, perki, przedmioty, skille, pieczęcie).
- **Sklep:** 3 oferty skilli naraz (`SoulShop.PickOffers`). Po piętrze od razu postój ze sklepem.
- **Darmowa nagroda usunięta** (decyzja użytkownika): bez ekranu nagrody, `RewardGenerator`, `RewardOption`, `RewardKind`, przerzucania nagród
  (`rewardRerollSoulCost`, `rewardExplorationChance`, `learnNewSpellChance`) i `GameScreen.Reward`. Zostały `RewardPools` (pula sklepu) i `Describe`
  (nowe `Describe.Upgrade`). Testy generatora nagród usunięte, pozostałe przepisane na sklep dusz.
- **Balans HU 2.0 (26.10):** unik 18 → 21,6; elity HP ×4,5, obr. ×1,45, co 28–35 demonów od fali 2; odporności pełne do 40%, dalej połowa, limit 65%;
  redukcja obrażeń (wszystkie źródła razem) pełna do 25%, dalej połowa, limit 45%; obrona fizyczna zdejmuje najwyżej 55%; bezpośredni cios – min. 15% surowych obrażeń;
  kradzież życia + życie za zabójstwo – najwyżej 8% maks. życia na sekundę (nieśmiertelność ×0,5). Broń dwuręczna +18% była już w danych (Flamberg 61).
- **Boty:** wybierają z 3 ofert skilli.
- **Testy:** `HuPerkTests` (6); testy przebiegu oczekują postoju zamiast ekranu nagrody.
  EditMode 225/228 (2 × „AnimeForward” z cudzej pracy). PlayMode pełny: 138 zaliczonych; po poprawkach (blok bierny ustępuje parowaniu i unikowi, `DebugNoPassiveBlock` w testach demonów,
  pad: przyciski cech pionowo, test pieczęci zwalnia skill przed wstawieniem – oferty zależą od losowania) zostają znane niestabilne: `Techniques_Charge`, `Floor1Combat_FrameWorkFitsBudget`,
  `EveryHuSkillAndForm` (Krok wichru, czasem jedna inna para przy ×3 czasu).
- Assety: „Dodaj nową treść” utworzyła 23 perki `hu_perk_*` i uzupełniła efekty trafienia w 3 (`TurrisSetup.FillNewDefaults` obsługuje teraz wzmocnienia). Zrzut postoju: `TestResults/shots_hu/seal_shop.png`.
- EditMode po usunięciu nagrody: 223/226 (2 × „AnimeForward”). PlayMode przebiegu (GameFlow, MainScene, Gamepad, SealFlow, Demon): 39/43, 3 pominięte,
  1 niestabilny czasowo: `Impact_HitstopBothSides` – krew na ostrzu zasycha w 30 s, a test w oknie trwał ok. 2 s.
- Seria `wojownik-v20` (wszystkie zmiany, bez darmowej nagrody): gracz 2/20 zwycięstw, średnio piętro 15,0 (v19: 5/20, 17,3); tester 0/20, 11,6 (v19: 2/20, 14,3).
  Gracz kupował ok. 6–7 punktów cech na podejście (wcześniej ok. 15 darmowych). v22 (dusze +10%): gracz 1/20, 16,2; tester 0/20, 9,9.
  Potem przywrócony darmowy punkt cechy, kupowanie cech wycofane.
- Seria `wojownik-v24-darmowa-cecha` (bez darmowej nagrody, darmowy punkt cechy, dusze +10%): **gracz 11/20 zwycięstw**, średnio piętro 22,8, mediana 30;
  tester 0/20, średnio 14,1, mediana 13 (zabójcy: Miotacz głazów 6, Królowa Topieli 5). W toku: `wojownik-v25-darmowa-cecha-czas-rzeczywisty` (2 × 5, normalny czas).
- **Dusze +10%** (`BalanceConfig.soulRewardMultiplier` = 1,1). Serie w kolejce: `wojownik-v22-dusze110` (przyspieszona, 2 × 20) i
  `wojownik-v23-dusze110-czas-rzeczywisty` (`TURRIS_BOT_REALTIME=1`, 2 × 5: bez stałego kroku 1/60 s, czas gry = czas zegara; limit testu 48 h).

### Etap 6 Hell Unleashed (ekonomia bohatera) i reżyser ataków — 2026-10-28
- **Dane:** `tools/art/import_hu_economy.py` kopiuje do `Resources/HellCatalog/` pliki `items.json` (71), `affixes.json` (131) i `upgrades.json` (46).
- **`Run/HellEconomy.cs`:**
  - przeliczenie statystyk HU na Turris: życie ×2, pancerz ×6, ułamki → %, dodane obrażenia płaskie → 5% obrażeń broni za punkt; statystyki bez odpowiednika są pomijane;
  - rzadkość wg źródła (tabele HU, legendarne od piętra 5), jakość (elita 0–1, boss 1–2), affixy (przedrostek i przyrostek: magiczny 1+1, rzadki 2+2, legendarny 3+3;
    grupy wykluczające, tiery od fal 1/8/16, nowsze tiery częściej);
  - wartość = cena HU × rzadkość × jakość × 2,5.
- **`ItemInstance`:** `rarity`, `huQuality`, `affixes` i `affixNames`, wliczane w premie. Zapis stanu podejścia je przechowuje.
- **Poziomy broni:** +20% za poziom (było +10%), do 5 poziomów jak w HU (tarcze i grymuary 3), cena HU × 1,6^poziom × 2,5 × inflacja.
  Kupowane w sklepie po każdym piętrze; kapliczka używa tej samej ceny.
- **Treść** (`DefaultContent.HellEconomy.cs`, assety przez „Dodaj nową treść”):
  - 71 przedmiotów `hu_item_*` w puli;
  - 23 perki HU ze statystykami z odpowiednikiem jako wielopoziomowe wzmocnienia `hu_perk_*` (poziom = warstwa).
  - Perki zmieniające mechanikę – przeniesione 2026-10-29 (wyżej).
- **Sklep po piętrze** (`Run/FloorShop.cs`, `GameRoot.Economy.cs`, `GameUI.Economy.cs`):
  - 3 bronie (zawsze kolejny poziom trzymanej);
  - 4 perki aktywne dla wyposażenia;
  - 6 przedmiotów: 3 w pancerzu klasy i 3 dowolne, co najmniej jeden magiczny i jeden rzadki, od piętra 5 także legendarny; bez unikatów.
  - Treść innej klasy ma wagę ×0,35. Ceny ×2,5 × inflacja 15%/piętro.
- **Łupy:** zwykły wróg 4%, elita 60%, boss 100%. 30% to broń, rzut na unikat daje unikat. Przedmiot trafia od razu do ekwipunku (pusty slot) albo do plecaka.
- **Reżyser ataków** (`Enemies/AttackDirector.cs`, wierny port HU): limit demonów naraz w zamachu lub ciosie.
  - Limity wręcz / dystans / specjalne: akty 1–3 to 3/1/1, 4/2/1 i 5/2/1, sługi obok bossa 2/1/1. Bossowie bez limitu.
  - Demon bez miejsca krąży i ponawia próbę po 0,2–0,5 s. Miejsce zwalnia się w odzyskaniu, przy przerwaniu i przy śmierci.
- **Boty:**
  - korzystają z pieczęci (`HeroBot.Seals.cs`): dar bossa, sakwa i sklep;
  - korzystają ze sklepu po piętrze (`HeroBot.Shop.cs`): poziom broni, perki, przedmioty poprawiające build.
- **Testy:** `EconomyTests` (5), `AttackDirectorTests` (2). EditMode 218/221 (2 × „AnimeForward” z cudzej pracy). PlayMode przebiegu gry: 37/40, 3 pominięte.
- „Dodaj nową treść” uzupełniła przy okazji krwawienie w atakach Bezimiennego Cienia (`enemy_hu_boss_shadowlord`), zgodnie z treścią domyślną.
- **Niezmienione względem HU:** (uzupełnione 2026-10-29 – patrz wyżej).
- Seria z pieczęciami bez etapu 6 (`wojownik-v18`): gracz średnio piętro 12,9, dwa razy do XXX (Arcydiabeł). Tester 8,2.
- **Seria z etapem 6 i reżyserem ataków** (`wojownik-v19`, 2 × 20, 3 próby na piętro): **pierwsze zwycięstwa**.
  - Gracz: 5/20 zwycięstw, średnio piętro 17,3, ok. 17 zakupów w sklepie na podejście (poziom broni, Szybkie ręce, Ostrze, Twarda skóra, Kat).
  - Tester: 2/20 zwycięstw, średnio piętro 14,3.
  - Do obserwacji: „uśpiony wróg blokuje koniec piętra” (13–17 razy na serię, gargulec) i „parowanie w oknie nie zadziałało” (17 u gracza).

### Plan weryfikacji kostek i nóg pozostałych bohaterów — 2026-10-28

- Na wyraźne polecenie użytkownika przygotowano **wyłącznie [plan weryfikacji](PLAN_WERYFIKACJI_ANATOMII_BOHATEROW.md)**.
  Status: **nieuruchomiony; czeka na sygnał użytkownika**. Berserker pierwszy — użytkownik zgłasza u niego
  podobne wykręcanie stopy jak wcześniej u paladyna. Pozostałe postacie wymagają dopiero oceny.
- Zakres: 14 pozostałych aktywnych wyglądów, paladyn jako kontrola regresji; wojownik tylko do odczytu.
  Plan obejmuje obie nogi, pełne cykle podparcia i wymachu, rzeczywiste zestawy broni, aktualne `lookOverride`,
  siatkę buta oraz rozdzielenie klipu, retargetowania, korekt IK i wag modelu.
- Z odczytu kodu: obecna kontrola skrętu obejmuje cztery starsze definicje z mieczem i ma wyjątek dla łucznika;
  testy nowego berserkera nie rozstrzygają anatomicznej poprawności kostek. To luka pokrycia, nie nowy wynik audytu.
- W tej pracy tylko dokumentacja i odczyt danych: **bez uruchamiania Unity/MCP, testów, renderów, instrumentacji,
  zmiany modeli lub animacji i napraw**. Start weryfikacji dopiero po kolejnym poleceniu. Bez commita i pusha.

### Mag — naturalniejsza postawa, bez wypchniętego tyłu — 2026-10-28

- Poprawka tylko dla `visual_mage_anime`: `balancedMageStance`, warstwa `MageStancePose` po retargetowaniu/IK,
  przed `BodyDynamics`. Ograniczenie nadmiernego pochylenia na trzech kręgach, łagodne podniesienie za niskich
  bioder w spoczynku, zachowanie stóp przez IK i ich obrotów. W ruchu zostają naturalny przechył i praca barków;
  korekta wygasa przy czarze, ataku, uniku i pozostałych akcjach. Źródłowe klipy i rig bez zmian.
- [Obejrzane porównania i raport](../ArtSource/GothicAnimePoC/Mage/posture-v1/README.md): spoczynek **15,85° → 5,51°**,
  wysokość miednicy nad środkiem kostek **0,683 → 0,758 m**, badana faza biegu **32,85° → 13,69°**.
  Kość bioder w spoczynku była już blisko środka stóp; wrażenie „tyłu uciekającego do tyłu” wzmacniały niska miednica,
  zgięty tułów i długa szata. Weryfikacja obejmuje bok/przód/tył i prawdziwy zrzut z gry.
- **PlayMode w otwartym edytorze: 20 zaliczonych / 0 błędów / 5 pominiętych opcjonalnych galerii**:
  nowy `MagePosturePlayTests` + regresja obu modeli i `HeroModelPlayTests`. Własna galeria postawy zapisana niezależnie
  od pominiętych testów. Pozycje stóp w badanym spoczynku zgodne <6 mm, końcowe pozy czaru/ataku/uniku zgodne <2 mm.
  XML: `TestResults/mage-posture/posture-live.xml`, `regression-live.xml`; tabela i PNG obok.
- Po utracie połączenia MCP użytkownik włączył serwer; bieżący proces Unity miał aktywny most na **8102**, a martwy
  nasłuch na 8099. Wykorzystano świeżą sesję zainstalowanego serwera przez `tools/unity_karnel_mcp.py`.
  Wtyczka `runTests` miała błąd konstruktora `ExecutionSettings` (Test Framework 1.8), więc
  `MagePostureLiveRunner` uruchamia testy z menu prawidłowym API i zapisuje wyniki, bez drugiej instancji Unity.
- Osiem chronionych źródeł z korekty stylu nadal ma zgodne SHA256; materiały, model FBX, mapy i pozostałe postacie
  nietknięte w tej poprawce. Wojownik i Hell Unleashed bez zmian; brak commita i pusha.

### Boty: przedmioty, ponowne próby, wyzwania, powtórki; wolniejsze portale — 2026-10-28
- **Boty:**
  - zakładają przedmioty z plecaka (`HeroBot.Gear.cs`, ocena buildu: DPS, życie, obrona) i biorą przedmioty z nagród, gdy pomagają;
  - po śmierci powtarzają piętro z zapisu stanu (do 3 prób, `BotPlaytestRunner.AttemptsPerFloor`).
- **Zapis stanu podejścia** (`Run/RunSnapshot.cs`): klasa, cechy, ekwipunek, plecak, skille z poziomami, slotami i pieczęciami, wzmocnienia, sakwa, dusze, flaszki.
  `GameRoot.StartRunFromSnapshot` uruchamia z niego grę. Test: `RunSnapshotTests`.
- **Wyzwania botów:** piętro przegrane trzy razy zapisuje się w `persistentDataPath/Wyzwania`. Menu główne → „Wyzwania botów” startuje to piętro z buildem bota.
  Przycisk niesprawdzony w uruchomionej grze.
- **Powtórki:** `BotReplay.cs` zapisuje co 0,1 s bohatera, wrogów i pociski oraz zdarzenia walki. Szablon `tools/replay/powtorka_szablon.html` daje odtwarzacz
  z widokiem z góry, zapisywany w `<katalog serii>/powtorki/` dla każdej ostatecznej porażki.
- **Korekty wieży:**
  - `HellTower.FewerEnemies` zdejmuje najsłabszych wrogów z piętra (budżet minus koszt najsłabszego demona); na piętrze VII 8 wrogów mniej, do decyzji;
  - **`SpawnTempoScale` = 0,5**: tempo wychodzenia z portali o połowę wolniejsze, czas otwarcia ×2, ta sama liczba demonów;
  - menu edytora: Turris → „Przebuduj wieżę (fale Hell Unleashed)”.
- **Seria 16** (`docs/raporty_botow/wojownik-v16`): średnie piętro tester 7,7, gracz 9,1 (wcześniej 6,9), najdalej XIV. Piętro VII przestało być ścianą.
  Nowe ściany: boss IX Królowa Topieli, piętro XI, u testera boss VI Pan Sfory.

### Wieża 30 pięter (fale Hell Unleashed), poziomy skilli z postępu, szybszy PlayMode — 2026-10-28
- **Wieża**: wierny port 30 fal HU (`Core/HellTower.cs`, dane `Resources/HellCatalog/waves.json` z nowego `tools/art/import_hu_waves.py`).
  - Zwykłe piętra to portale: czas, budżet, grupy i skład demonów fali.
  - Co trzecie piętro to boss HU z kapliczką po nim. Finał na piętrze XXX to Arcydiabeł.
  - 10 aktów z nazwami. Piętro I zachowuje gotycki dziedziniec drugiego agenta.
  - `FloorDefinition`: nowe pola `damageScale`, `speedScale`, `soulScale` jak w falach HU. Życie zwykłych fal łagodzone od fali 10 (×0,75 na 29).
  - Korekta piętra II (budżet 41) zostaje.
  - Asset przebudowuje wąska metoda `TurrisSetup.RebuildTowerBatch`, która nadpisuje tylko `tower_main`. `HellEncounters` usunięty, bo ręcznych pięter IV–V już nie ma.
- **Popiół**: 3 + numer piętra, finał 50.
- **Progi treści**:
  - skille i bronie HU: `MinFloorFor = fala − 1`;
  - progi treści Turris i `requiredBestFloor` odblokowań: ×5 (`LegacyFloorScale`), w kodzie i w assetach.
- **Poziom startowy skilla** (`Run/SkillTiers.cs`, progi HU): skill z nagrody lub sklepu od piętra 7 przychodzi na poziomie II, od 16 na III.
  Skille Turris dostają +2 / +4. Tytuł nagrody i oferta sklepu pokazują poziom.
- **Jawne przypisanie każdego skilla do broni** (`WeaponFamilies`, test `EverySkill_HasExplicitWeaponAssignment`):
  - czary do fokusu;
  - aury, szarże, Odtrącenie, Tąpnięcie i okrzyki do dowolnej broni;
  - skille klas: jedynym wymogiem jest klasa.
- **Migracja HU**: wszystkie 61 skilli i 186 pieczęci były już przeniesione (etap 4). Poza pulą jest 15 skilli klas, celowo.
- **PlayMode**: benchmark i galerie pieczęci włączały się samą obecnością `TestResults/hu-bench` i `TestResults/hu-seals`.
  Teraz włącza je tylko zmienna środowiskowa. Pełny PlayMode w oknie trwa ok. 9 min zamiast ponad 40.
- **Testy**:
  - EditMode 208/211 + 2 nowe (`SkillTiersTests`). Nie przechodzą 2 testy „AnimeForward” z cudzej pracy.
  - PlayMode po poprawkach: niestabilne `Floor1Combat_FrameWorkFitsBudget` (wydajność) i `Techniques_ChargeMovesAndHits` (czas)
    oraz **nowy problem**: `EveryHuSkillAndForm` – Krok wichru nie zadaje obrażeń w poligonie, bo doskok skraca się do zera. Przyczyna nieznana.
- Boty na 30 piętrach (`docs/raporty_botow/wojownik-v8`, 2 × 20): 0 zwycięstw. Tester dochodzi średnio do piętra 4,6, gracz do 5,0, najdalej VII.
  Ścianą są fale 4–7. Bohater nie dogania krzywej demonów HU, bo brakuje progresji HU co falę: sklep z perkami, poziomy broni, przedmioty z affixami.
- O 20% mniej demonów niż w HU (`HellTower.MonsterCountScale`, budżet portali ×0,8) i łagodzenie pięter IV–VI (życie ×0,85, obrażenia ×0,9).
  Boty (`wojownik-v10`): gracz przechodzi piętro VI w 77% podejść (stara wieża ok. 68%), tester w 23%. Nową ścianą jest piętro VII (9/23 gracza).
- Następne kroki: etap 6 planu migracji (przedmioty, affixy, rzadkości, sklep co piętro jak w HU) dla pięter VII+, Krok wichru w poligonie.

### Balans skilli wojownika i ostrzeżenie przy dodatkach — 2026-10-28
- `HuBalance`: Rozpłatanie 32/36/36 przy odnowieniu 5 s. Cięcie wichru 28/30/30, Fala przełamania −12% przy odnowieniu 8 s, Upadek kata przy 7 s,
  Piekielna szarża przy 6,5 s, Trzęsienie przy 6 s, Młynek 23 na obrót przy 6 s. Egzekucja: mnożnik 1,2, premia +60%, odnowienie 9 s (`DefaultContent` i asset).
  Cel to 24–28 obrażeń na sekundę odnowienia.
- `LoadoutPlan.StartingHands` i `SkillsBlockedByExtras()`: ekran przygotowania ostrzega, gdy broń z dodatków wyłącza skill startowy.
  Napis w GUI nie jest sprawdzony zrzutem; logikę sprawdza test.
- Testy: EditMode 206/209 (2 × „AnimeForward” z cudzej pracy). Nowe testy: `WarriorSkills_DamagePerCooldown_InOneBand`,
  `PrepScreen_WarnsWhenExtraWeaponBlocksStartingSkill`. Testy skilli w PlayMode przechodzą.
- Seria botów (`docs/raporty_botow/wojownik-v7`): tester 37/50, gracz 34/50. Rozrzut skilli zmalał z 7–42 do 24–32 obr./s odnowienia.
  Gracz kupuje 28 ofert zamiast 9, ale dalej trzyma Rozpłatanie i Cięcie wichru (4,2 skilla na podejście).
- Następne kroki: przenoszenie części ulepszeń przy wymianie skilla, więcej ofert skilli ostrzy i ciężkiej broni, korekta Młynka i Egzekucji.

### Skille należą do broni, nie do klas (wzór: Path of Exile 2) — 2026-10-28
- `Stats/WeaponFamilies.cs`: rodzaje broni w `BuildTag` (Blade, HeavyWeapon, Dagger, Focus + istniejące Ranged, Shield).
  - Rodzaje broni HU wynikają z tagów katalogu HU, a bronie Turris z tabeli. Broń może należeć do kilku rodzajów (sztylet, szabla, kosa).
  - Tagi buildu liczą obie ręce: grymuar i sztylet do rzucania też się liczą.
- Jedna tabela przypisań skill → rodzaj broni, generowana do assetów (`requiredTags`). Rodzaje broni w wymaganiu działają jako „którykolwiek”
  (Rozpłatanie: ostrze lub ciężka broń), a pozostałe tagi jako „wszystkie” (`WeaponFamilies.Meets`). Czary wymagają fokusu.
  Skille klas pod osobnym przyciskiem i Tąpnięcie działają z każdą bronią.
- Obrażenia skilli HU i czarów są mnożone przez siłę broni. Liczy się DPS broni względem wzorca rodzaju (miecz, sztylet, łuk myśliwski, różdżka żywiołu)
  i poziom ulepszenia broni (+10%/poziom). Ostrze na sztylecie daje ×0,69, miecz dwuręczny ×1,25, a startowe bronie klas ×1.
  Techniki Turris biją bronią jak dotąd. Mnożnik jest zapisany w `SpellRuntime.weaponScale`.
- Kupione odblokowania skilli i broni są dostępne dla każdej klasy (`UnlockDefinition.ClassFree`). Domyślny zestaw klasy i pozostałe dodatki zostają przy klasie.
  Skill startowy do wyboru musi pasować do broni zestawu, a puste sloty na start wypełniają najpierw skille, których broń pozwala użyć.
- Testy:
  - nowe `WeaponFamiliesTests` (5), w tym zgodność assetów z tabelą;
  - zaktualizowane testy rycerza rzucającego czar (dostaje różdżkę) oraz testy odblokowań klasowych;
  - w PlayMode skill zakłada najpierw pasującą broń (`TestWeapons`).
- Wyniki: EditMode 204/207 (2 × „AnimeForward” z cudzej pracy, 1 pominięty). PlayMode, testy skilli i przebiegu: 56/59.
  `Techniques_ChargeMovesAndHits` nie przechodzi tylko w długim przebiegu (szarża bez trafienia), osobno przechodzi. To niestabilność czasowa.
- Seria botów 2 × 50 (`docs/raporty_botow/wojownik-v6`): tester 45/50, gracz 39/50. Gracz zostaje przy skillach startowych i odrzuca 157 z 207 ofert,
  więc cel „wiele skilli” wymaga balansu: osłabić skille startowe, wzmocnić pozostałe skille wojownika. Egzekucja ma 810 obrażeń na użycie, czyli odstaje.
- Ryzyko UX: dodatek z bronią innego rodzaju na ekranie przygotowania wyłącza skill startowy, a ostrzeżenia brak.
- Następne kroki: balans skilli (propozycja w `RAPORT_WOJOWNIK.md`, seria 5), ostrzeżenie na ekranie przygotowania, boty pozostałych klas.

### Boty: oferty sklepu i zamiana skilli, seria 2 × 50 — 2026-10-28
- Boty (`Tests/PlayMode/Bots/HeroBot.Skills.cs`):
  - tester kupuje każdą ofertę sklepu dusz i co piętro rotuje najmniej sprawdzony skill do slotu;
  - gracz kupuje oferty i nowe skille, gdy zapowiadają się lepiej niż najsłabszy w slocie, a sloty układa według obrażeń na sekundę odnowienia z pamięci serii.
  - Skille ruchu (Szarża) mają ocenę co najmniej 90% mediany.
- Telemetria: poziom skilla przy użyciu, pochodzenie użyć (startowy, odblokowanie, klasa, nagroda, sklep, przedmiot), zamiany slotów, oferty.
  Nowa sekcja raportu „Kolekcja skilli”.
- Wyniki (`docs/raporty_botow/wojownik-v5b`): tester 45/50, gracz 22/50. Różnych skilli na podejście: tester 6,1, gracz 5,2.
- **Skille startowe są za mocne:** Rozpłatanie ma 41,8 obr./s odnowienia, Cięcie wichru 30,4, pozostałe skille wojownika 7–19,
  z wyjątkiem Fali przełamania (34,9). Propozycja balansu jest w `RAPORT_WOJOWNIK.md`, seria 5 (czeka na decyzję).
- **Luka w treści:** skille HU nie są przypisane do broni ani klasy (brak `requiredTags`, `classId` nie filtruje puli). Wojownik z mieczem rzuca Piekielną kulę.
- Następne kroki: przypisanie skilli do rodzajów broni; balans skilli startowych i pozostałych skilli wojownika; seria gracza z Szarżą zablokowaną w slocie;
  przenoszenie części ulepszeń przy wymianie skilla.

### Korekta wykończenia maga i berserkera — 2026-10-28

- Na polecenie użytkownika wdrożono [korektę v2](../ArtSource/GothicAnimePoC/StyleCorrections/README.md),
  według zatwierdzonej planszy i referencji dziedzińca. Osobny shader `Turris/AnimeHeroPainterly`,
  prywatne `mage_anime_painterly_v2.mat` / `berserker_anime_painterly_v2.mat` i menu przywrócenia dawnych materiałów.
- Pigment korygowany przy renderowaniu z **oryginalnych map 4096²**: granat/fiolet maga, ograniczenie drobnych
  namalowanych refleksów, matowa paleta naramiennika berserkera. Po obejrzeniu zbliżeń wycofano zbyt mocne
  globalne wygładzanie. `AnimeHeroStyleMasks` dodaje techniczne RGBA8 do importowanych siatek:
  ochrona detalu głowy, górne warstwy maga i stal przy prawym ramieniu berserkera. Maski podążają za animacją.
  Dodatkowy strumień kolorów ok. 13,3 MiB; pozycje, UV, normalne, trójkąty, wagi i kości bez zmian.
- **PlayMode gfx 6/6**: galeria A/B oraz pięć testów obu bohaterów (prawdziwy start, bieg, broń, seria,
  czar grymuaru, geometria i chwyt). Po doprecyzowaniu próby światła powtórzona **galeria 1/1**:
  kamera widzi wyłącznie ciało, więc różnica podłogi/broni nie zalicza testu. Średnia zmiana RGB 0–1
  po dodaniu światła: mag 0,00424, berserker 0,00534. Cztery własne przebiegi shadera × dwa etapy
  skompilowane dla D3D / StandaloneWindows64 bez błędów. Unity 6000.6.3f1, URP 17.6, Linear / D3D12.
- Pary 1600×1200: kamera gry, przód/tył/głowa na arenie i przód/tył neutralny. Stabilizacja trzech renderów
  w tym samym render target, kontrola stałego tła, brak różowych pikseli w ocenionych planszach; obrazy obejrzane.
  Trwałe JPG w `ArtSource/GothicAnimePoC/StyleCorrections/unity-*-v2.jpg`;
  pełne PNG, `regression-6-tests.xml/.log`, powtórna galeria i metryki w `TestResults/hero-style-v2`.
- SHA256 **8/8 chronionych plików zgodne**: FBX, źródłowe tekstury, dawne materiały, shader demonów i klasa wojownika.
  `Models/Heroes` bez różnic Git. Paladyn i Hell Unleashed nietknięte w tej korekcie. Brak nowych operacji Meshy,
  wymiany animacji, forced setup, commita i pusha. Nie wykonywano benchmarku/builda gracza.
- Skrypt testów obsługuje `TURRIS_TEST_RESULT_DIR`, aby osobne zadania nie nadpisywały sobie XML/logów.
  Specyfikacja stylu 1.2 wskazuje planszę korekty jako wzorzec wykończenia, zachowując tożsamości klas.

### Rozpłatanie: powalenie i odnowienie 4 s — 2026-10-27

- Gra miała powalenie (`StatusEffects.KnockDown`, `EnemyBrain.HuThrowDown`, dotąd tylko komponent HU „Odtrącenie”).
  Komponent `Cone` czyta teraz `down_seconds`: każdy zraniony wróg (trafienie lub przełamana garda) leży tyle sekund; bossy odporne,
  elity krócej (mnożnik kontroli), potem standardowa odporność na ogłuszenie.
- Rozpłatanie (`HuBalance`): `down_seconds=1`, odnowienie 3,5 → **4 s**, obrażenia 39/44/44 bez zmian. Opis skilla w assecie
  i `DefaultContent`.
- Testy: `TowerBalanceTests` (EditMode 31/31 w zestawie), nowy `HeroSkillPlayTests.Cleave_KnocksDownEnemies_ButNotBosses`;
  PlayMode skille bohaterów + HU 18/18. Boty seria 4: tester 17/20, gracz 13/20, Rozpłatanie 9/16 (w szumie wobec 12/16).
  Bez commita.

### Piętro II −1 kultysta, Rozpłatanie szybsze, uniki botów — 2026-10-27

- Piętro II: budżet portali 41, waga kultysty 3,7 – średnio 13,9 kultysty (start: 16,9), chochliki i czerwie bez zmian; asset,
  `DefaultContent`, testy `DemonContentTests`/`TowerBalanceTests`.
- Rozpłatanie: odnowienie 5 → 3,5 s (`HuBalance`, wszystkie poziomy); obrażenia +15% bez zmian.
- `Projectile`: gettery tylko do odczytu (prędkość, strona, właściciel, promień, dodgeable/blockable) dla botów.
- Boty: unik przed pociskiem z prawdziwego toru i chwili styku (wcześniej ok. 0,1 s za późno), uniki taktyczne (okrążenie, doskok),
  telemetria uników: powód, odmowy gry z akcją, skuteczność; sekcja „Uniki” w raporcie. Trafienia płonącej ziemi po kuli ognia
  nie są liczone jako nieudany unik.
- Seria 3 (2 × 20): tester 17/20, gracz 14/20; uniki 18 / 40 na podejście, skuteczność wręcz 66% / 82%, pocisk 65% / 78%.
  Rozpłatanie 12/16 zwycięstw (wcześniej 1/13). Raport: `docs/raporty_botow/RAPORT_WOJOWNIK.md`, dane `wojownik-v3/`.
- Testy: EditMode treść wieży/HU 23/23. Bez commita.

### Pomoc w celowaniu z HU, korekty po raporcie botów — 2026-10-27

- **Pomoc w celowaniu** (port `HellUnleashed.Core.Combat.AimAssist` → `Combat/AimAssist.cs`): przy starcie ciosu najbliższy wróg
  w stożku 28° wokół kierunku ruchu (bez ruchu – dowolny kierunek), zasięg broń + 1,5 m (min. 5 m); skille kierunkowe/na punkt –
  stożek 35°, 12 m; skille „na siebie” bez zmian. Cel trzymany do końca akcji, obrót 1440°/s. `PlayerController.AssistTarget`,
  `PlayerCombat.AssistTarget` (skille na punkt). Namierzanie ma pierwszeństwo i śledzi cel przez cały zamach. Przełącznik
  w Ustawieniach, `PlayerProfile.aimAssist` (domyślnie wł.).
- **Piętro II:** budżet portali 44 → 42, waga kultysty 4,5 → 4 – średnio 14,9 zamiast 16,9 kultysty, chochliki i czerwie bez zmian
  (asset `tower_main` i `DefaultContent`).
- **Piętra IV–V (bez bossów):** `HellEncounters` dodaje Taranowca (IV, fala 2) i Gargulca (V, fala 3); idempotentne; asset uzupełniony.
- **Rozpłatanie +15%:** `HuBalance` – `skill.warrior.cleave` 39/44/44.
- **Naprawiony NRE** po sparowaniu szarży (`EnemyBrain.Specials.cs`, `EnemyBrain.TickAttack`) – test regresji w `DemonPlayTests`.
- Boty: szukają uśpionych wrogów (gargulec-posąg), telemetria odporna na przerwane podejście. Druga seria 2 × 20:
  tester 10/20, gracz 13/20 (przed: 9 i 11) – `docs/raporty_botow/RAPORT_WOJOWNIK.md`, dane `wojownik-v2/`.
- Testy: nowe `TowerBalanceTests` (EditMode 3/3), `AimAssistPlayTests` (4/4); PlayMode gfx: przepływ gry, czucie, scena Main, zasięg skilli,
  przypisania zaliczone (przebieg 57 testów; jedyny błąd – EchoAndFan, naprawiony pomiarem), po poprawce celowanie + skille
  bohaterów i HU **21/21**, demony i migracja **18/18** (2 pominięte galerie).
  `HuSkillCasterPlayTests.Face()` mierzy z wyłączoną pomocą (porównanie rzutów w stałym kierunku). EditMode 200/203:
  te same 2 „AnimeForward” co wcześniej + 1 pominięty WAV. Bez commita.

### Propozycja korekty stylu postaci — 2026-10-27

- Po porównaniu modeli z docelowym dziedzińcem przygotowano [planszę korekty maga i berserkera](../ArtSource/GothicAnimePoC/StyleCorrections/README.md).
  Jedna ilustracja wbudowanym `image_gen`, dokładny prompt zachowany, wynik obejrzany.
- Kierunek: czytelniejsze warstwy granatu/fioletu maga, większe malarskie płaszczyzny skóry i pancerza berserkera,
  zgrupowane włosy/futro, kontrolowany detal i wspólne chłodne cienie. Konstrukcja i bogactwo strojów zachowane.
- **Propozycja do oceny, bez wdrożenia w Unity**. Zalecane dalsze prace: osobne wersje tekstur i materiałów,
  najpierw mag jako wzorzec, potem berserker, porównanie jednakowych kadrów w grze.
  Bez zmiany modeli, rigów, animacji, dawnych koncepcji i ZIP; bez Meshy, testów Unity, commita i pusha.

### Gęstszy ogień ściany i fali ognia — 2026-10-27

- Na prośbę użytkownika ok. **+30% ognia** w „Ścianie ognia” i jej formach (Fala ognia, Forteca ognia): płomienie co **0,73 m**
  zamiast 0,95 m (`HuWall.FlameSpacing`), żar na ziemi ×1,3 (`GroundDensity`). Nowe `HellParticles.ScaleAmount` zagęszcza emisję
  ponad limit `SetAmount` i podnosi `maxParticles`, żeby nadmiar nie przepadał. Inne efekty bez zmian.
- Galeria `HuSkillCasterPlayTests.Gallery_SkillsAndForms_WhenRequested` (gfx) przed/po obejrzana:
  `TestResults/firewall/before|after/spell_firering*.png` – ciągła zasłona płomieni zamiast osobnych kęp. Bez commita.

### Boty grające — 2026-10-27: Wojownik, tester i gracz H&S/souls, 2 × 20 podejść

- Nowe testy PlayMode `Tests/PlayMode/Bots/`: wspólny mózg `HeroBot` (percepcja zamachów z ludzkim czasem reakcji,
  blok/parowanie/unik/wyjście z kręgu, ataki, riposta, leczenie, nagrody, cechy, kapliczka, sklep), bot klasowy `WarriorBot`,
  osobowości `BotPersona` (tester QA, gracz H&S/souls-like z meta-progresją), telemetria i raport z wnioskami generowanymi regułami.
  Uruchamianie wyłącznie z `TURRIS_BOT_DIR` (bez niej pomijane); `TURRIS_BOT_RUNS` – liczba podejść (domyślnie 20).
- Zmiana w grze: `PlayerController.Driver` (`IPlayerDriver`) – zewnętrzne źródło ruchu/gardy/biegu – oraz `LockOn(EnemyBrain)`.
  Bez sterownika zachowanie bez zmian.
- Wyniki (scena `Main`, trudność 0, ok. 5 min wsadowo dla 40 podejść): tester 9/20, gracz 11/20 zwycięstw.
  18/20 śmierci na piętrze 2 „Kult Bramy” (Kultysta – 31% otrzymanych obrażeń, 12/20 ostatnich ciosów);
  Cięcie wichru 19/26 zwycięstw wobec Rozpłatania 1/14; finałowy Kasztelan pada w ~13 s (Władca Bramy 46 s);
  zamachy hordy 0,31–0,33 s – za krótkie na parowanie; 15/20 śmierci z flaszką (brak okna 0,55 s pod ostrzałem).
- Znaleziony błąd (nie poprawiony): NRE w `EnemyBrain.Specials.cs:119` po sparowaniu szarży (`current` zerowane w callbacku trafienia).
  UX: czary bez wymaganej Inteligencji trafiają do slotów wojownika i są proponowane do ulepszenia.
- Regresja: PlayMode (przepływ gry, odczucie biegu, scena Main, przypisania, zasięg skilli, skille bohaterów) **46/46**, 1 pominięty.
  EditMode 197/200: 2 niezaliczone („AnimeForward” – materiały gotyckiej areny i awatar paladyna; shadery anime zmieniane równolegle,
  niezwiązane z botami), 1 pominięty WAV. Pełny PlayMode `batch` (-nographics) kończy się natywnym crashem renderera cząsteczek
  w `AnimeImpRigPlayTests.Shot` – test zrzutów wymaga trybu `gfx`/`window`.
- Raport: [raporty_botow/RAPORT_WOJOWNIK.md](raporty_botow/RAPORT_WOJOWNIK.md), dane i tabele botów w `raporty_botow/wojownik/`.
- Następne kroki: pary botów dla pozostałych klas (podklasa `HeroBot` + test), ewentualne poprawki balansu z rekomendacji.
  Bez commita.

### Berserker i mag anime — 2026-10-27: wdrożenie i zamknięty chwyt

- Na polecenie użytkownika [oba modele wdrożone](WDROZENIE_BOHATEROW_ANIME.md):
  `class_hu_berserker.lookOverride` → `visual_berserker_anime`, `class_mage.lookOverride` → `visual_mage_anime`.
  Każdy na kopii swoich dotychczasowych **26 kości**, własnym poprawnym awatarze Humanoid,
  ze starymi klipami i zestawami broni. Topór/młot berserkera, różdżka/kostur i grymuar maga zachowane.
- Pełne **2 543 857 / 3 619 624 trójkąty**, bez redukcji. `rig_anime_berserker.py` obsługuje także `--mage`.
  Ciągłe wagi anatomiczne, sztywne chwyty; dolna szata maga wiązana do bioder jako jedno okrycie,
  bez rozrywania jej w poziome pasy przy ruchu nóg. Brak symulacji Cloth.
- Palce zwinięte w siatce, kciuki domykane, gniazda wewnątrz chwytu. Opcja `anatomicalGripSlots`
  dopasowuje środek i orientację dłoni do drzewca przy IK. Domyślnie wyłączona dla dawnych modeli,
  włączona tylko dla obu nowych. Prywatne matowe materiały 4K, bez zmiany wspólnego shadera.
- Stały wymóg zaciskania dłoni dla **wszystkich przyszłych modeli** zapisano w `AGENTS.md`,
  specyfikacji stylu **1.1** i szablonie promptu. Broń pozostaje oddzielnym assetem.
- Końcowe **PlayMode gfx: 19 zaliczonych / 0 błędów / 5 pominiętych opcjonalnych galerii**:
  pięć nowych testów oraz regresja `HeroModelPlayTests`. Własne galerie nowych modeli zostały zapisane
  niezależnie od tych pięciu pominiętych galerii. Wybrane **EditMode 35/35** (klasy, animacje, migracja/IK).
  XML/logi i obejrzane zrzuty: `TestResults/berserker-anime`, `TestResults/mage-anime` (`review-final.jpg`).
  P99 rozciągania próbkowanych krawędzi maga maks. **2,574×** względem ustabilizowanego idle;
  pojedyncze drobne krawędzie w rzucaniu czaru osiągają większe współczynniki i pozostają do ręcznego dopracowania.
- Import i testy wykonano w zamkniętym edytorze; po problemach IPC licencji/Burst w sandboxie uruchomiono
  Unity poza nim. Nie uruchamiano `TurrisSetup.RunBatchForce`. Menu obu modeli pozwala przywrócić oryginały.
- Oryginalne 13 FBX bohaterów, źródła Meshy i ZIP koncepcji zachowane; wojownik i Hell Unleashed bez zmian.
  Brak dodatkowych operacji Meshy/kredytów, commita i pusha. LOD i benchmark builda gracza poza tym zakresem.

### Berserker Meshy Ultra — 2026-10-27: źródło wygenerowane i sprawdzone

- Po potwierdzonym doładowaniu saldo **1024**. Wykonano jedno wcześniej zatwierdzone zlecenie
  **Meshy 7.1 Ultra, geometria 4K, kolor 4K/PBR, FBX, bez redukcji**. Zadanie
  `01a17e6e-3dc0-7049-ba3e-d9c7e9d6ec49` **SUCCEEDED**, rzeczywisty koszt **35**, saldo **989**.
- [Pełne źródło berserkera](../ArtSource/GothicAnimePoC/Heroes/Berserker/meshy-v1/README.md):
  FBX 147 708 380 B, **1 270 871 wierzchołków / 2 543 857 trójkątów**. Jedna siatka/materiał/UV,
  brak szkieletu, 0 niefinitywnych współrzędnych i 0 zdegenerowanych ścian w progu 1e−12.
  Kolor/normalne 4096², metaliczność/szorstkość 2048². Zachowane oryginalne źródła i SHA256.
- Dwa pliki Blender: sam model z mapami oraz studio. **16 rzeczywistych renderów obejrzanych**:
  przód/tył/boki/¾, twarz, dłonie z dwóch stron, futro, pas, strój i buty.
  Ponowne otwarcie obu plików potwierdziło odczyt i spakowanie **4/4 map**.
- Wymaga dalszej obróbki: lokalny mostek między palcami, chwyt broni, dopasowanie istniejącego rigu,
  materiał matowego anime i testy deformacji. Meshy pozostawiło pozę **A**, mimo żądania **T**.
  PBR ze studia jest jaśniejsze i mniej malarskie od koncepcji. Nie oznaczono jako gotowego do gry.
- Bez kolejnych płatnych operacji. W tej pracy nie zmieniano modeli/definicji Unity, wojownika ani Hell Unleashed;
  berserker w grze nadal oryginalny. Archiwum koncepcji zachowane. Brak testów Unity, commita i pusha.

### Archiwum koncepcji i zlecenie berserkera — 2026-10-27

- Zachowano oryginalne PNG. Utworzono [dedykowany ZIP](../ArtSource/Archives/Turris_Damnatorum_concept_arty_bohaterow_2026-10-27_v1.zip):
  13 nowych plansz + wcześniejsze koncepcje paladyna/maga, referencje, prompty, galeria i specyfikacja stylu.
  41 plików, 44 038 750 B. Kontrola CRC i porównanie SHA256 wszystkich spakowanych plików zaliczone;
  osobny plik `.zip.sha256`, odtwarzanie `tools/art/archive_hero_concepts.py` bez nadpisywania archiwum.
- Użytkownik zatwierdził **berserkera Meshy 7.1 Ultra za 35 kredytów**, geometria 4K, kolor 4K/PBR,
  FBX, bez redukcji. Przygotowano i obejrzano [pojedynczą referencję bez topora](../ArtSource/GothicAnimePoC/Heroes/Berserker/meshy-reference-apose-v1.png)
  oraz manifest dokładnego żądania. Obraz wejściowy ma pozę A; wyjście zamówione w T.
- Początkowo dwa odczyty Meshy pokazały **24 kredyty, brakowało 11**; zapisano przygotowane żądanie
  i oczekiwano na doładowanie. Nie obniżono jakości. Zlecenie po doładowaniu wykonano — wynik w nowszym wpisie powyżej.
- Bez zmian assetów gry, Hell Unleashed, testów Unity, commita i pusha.

### Koncepcje pozostałych klas — 2026-10-27

- Przygotowano [13 osobnych plansz bohaterów](../ArtSource/GothicAnimePoC/Heroes/README.md),
  zgodnie ze specyfikacją gotyckiego anime. Pominięto istniejące koncepcje paladyna i bazowego maga.
  Wojownik, berserker, rębajło, łotrzyk, łucznik, zabójca, łucznik arkanów, mag bitewny,
  magowie ognia/wody/powietrza/ziemi i przywoływacz mają pełną sylwetkę, tył i detal głowy.
- Najpierw odczytano 15 rzeczywistych definicji klas oraz ich visual/FBX; wykonano i obejrzano
  13 neutralnych podglądów modeli. Zachowano tożsamość, płeć, wiek, charakterystyczne fryzury i wyposażenie.
  Styl oparto na referencjach paladyna/maga oraz `STYL_GRAFICZNY_GOTYCKIE_ANIME.md`.
- `ArtSource/GothicAnimePoC/Heroes/`: PNG, lokalna `galeria.html`, `classes.json`, dokładne prompty,
  manifest wymiarów/SHA256 i skrypty odtwarzające referencje/katalog. Wszystkie 13 wyników obejrzane,
  zweryfikowane jako PNG; drobne różnice konstrukcyjne między widokami wymagają ujednolicenia przed 3D.
- Wbudowany imagegen; bez operacji Meshy i kredytów. Wyłącznie koncepcje: bez podmiany wojownika,
  zmian assetów Unity, Hell Unleashed, testów gry, commita lub pusha. Następny etap po wyborze:
  pojedyncza referencja A/T, osobne wyposażenie, modelowanie i wykorzystanie istniejącego rigu.

### Specyfikacja stylu i przyszłych promptów — 2026-10-27

- Utworzono [STYL_GRAFICZNY_GOTYCKIE_ANIME.md](STYL_GRAFICZNY_GOTYCKIE_ANIME.md): referencje i ich role,
  zasady formy, detalu, matowych materiałów, palety i światła; wymagania dla bohaterów, potworów, areny,
  wyposażenia i VFX. Rozdzielono docelową koncepcję od bieżących niedociągnięć PoC.
- Stały blok stylu i pięć szablonów promptów: adaptacja postaci, arena, pojedynczy obiekt, tekstura, VFX.
  Jawne niezmienniki tożsamości, przeciwdziałanie futurystycznemu połyskowi, czarnym cieniom i uproszczeniom,
  osobny format referencji do Meshy, lista kryteriów oceny. Orientacyjne HEX oznaczone jako propozycje palety.
- Dodano odnośnik w README. Wyłącznie dokumentacja; bez zmian assetów, generacji, testów gry i commita.

### Chochliki anime — 2026-10-27: stare rigi, animacje i wdrożenie

- Na polecenie użytkownika [nowe siatki ognia i wody podpięte pod istniejące szkielety](../ArtSource/GothicAnimePoC/FirstArenaEnemies/rigged/README.md).
  Ogień: stare 24 kości + 3 segmenty ogona; woda: istniejące 26, ze skrzydłami i ogonem.
  Pełne 949 966 / 1 271 242 trójkąty zachowane; 0 nieprzypisanych wierzchołków, maks. 4 wpływy.
- `rig_anime_imps.py`: korekta pozy generatora, ciągłe regiony wag, wygładzanie po krawędziach, bez kwantyzacji
  powodującej uskoki. `AnimeImpImport`: dziesięć kopii starych klipów, retarget pozycji względem bindposes,
  zachowane czasy i granice trafienia. Prefaby Generic bez awatara blokującego część jawnych wiązań Transform.
- Prywatny shader `AnimeCreatureMatte`, zachowane VFX ognia/wody. Podparcie ognia skorygowane o +7,8 cm.
  `enemy_imp` i `enemy_imp_frost` mają nowe przypisania; ich balans i pozostałe definicje bez zmian.
  Menu **Turris → PoC → Chochliki anime → Przywróć oryginalne modele** zapewnia powrót.
- Końcowe **PlayMode 3/3** (`e2914e71f84f48439f1258483a3f5e08`): sterownik, ruch, atak, śmierć,
  ogony/skrzydła, 45 próbek klipów/model, wagi, oświetlenie pierwszej areny i doczepienia VFX.
  Regresja demonów: `bd1fcc8092724e028fab2c9cfc7b7799`, **succeeded**, 12 zakończonych, 0 błędów
  (dwie galerie warunkowe bez żądania). Pełne **EditMode 199 zaliczonych / 1 pominięty eksport WAV**.
  Szersze przebiegi poprzedzają końcową korektę materiału; końcowy stan sprawdzony wymienionymi 3 testami.
- Poprawiony stary test kultysty: wyłączony automatyczny LateUpdate przy ręcznym Tick, bez osłabiania asercji.
  `TestResults/anime-imps/`: raporty MCP, obejrzane galerie rzeczywistego BakeMesh i zrzuty z areny.
  Geometria nie eksploduje; lokalne rozciąganie drobnych krawędzi pozostaje do ręcznego dopracowania w zbliżeniach.
- To pełna geometria PoC; **bez pomiaru maksymalnej hordy w buildzie gracza**, LOD/siatka do licznych grup pozostają.
  Stare modele i pełne źródła Meshy mają zgodne SHA256. Hell Unleashed i bohaterowie bez zmian.
  Rigowanie bez kolejnych operacji Meshy/kredytów; bez commita i pusha.

### Potwory pierwszej areny — 2026-10-27: ogień i woda z Meshy gotowe

- Użytkownik zatwierdził **dwa modele za 70 kredytów**. Meshy 7.1 Ultra, geometria 4K, kolor 4K/PBR,
  FBX, poza T, bez remeshu/redukcji. Oba zadania **SUCCEEDED**, rzeczywisty koszt 35 + 35,
  saldo ponownie odczytane **94 → 24**. Bez dodatkowego rigowania lub płatnych poprawek.
- [Ogień](../ArtSource/GothicAnimePoC/FirstArenaEnemies/fire-meshy-v1/README.md): zadanie
  `01a17d06-fb7c-779f-9ebe-eb015c8d7357`, **474 951 wierzchołków / 949 966 trójkątów**.
- [Woda/lód](../ArtSource/GothicAnimePoC/FirstArenaEnemies/water-meshy-v1/README.md): zadanie
  `01a17d07-5187-7363-b08e-ee48623b9235`, **635 607 wierzchołków / 1 271 242 trójkąty**.
- Oryginalne FBX zachowane, osobne pliki Blender z modelem/mapami i studiem, 14 PNG na model.
  Kolor/normalne 4K, metaliczność/szorstkość 2K. Jedna siatka, materiał i UV na model; brak szkieletu.
  Audyt: 0 niefinitywnych wierzchołków, 0 zdegenerowanych ścian w progu 1e−12. Ponowne otwarcie
  obu czystych źródeł i studiów potwierdziło odczyt 4/4 spakowanych map. Hashe FBX/Blend/map zapisane.
- Obejrzano sylwetki z pięciu kierunków oraz detale głów, stóp, dłoni i skrzydeł. Woda zachowuje
  odstęp dłoni od skrzydeł, ogień jest bezskrzydły; ogony i pazury obecne. Statyczna ocena nie
  potwierdza jeszcze deformacji ani zgodności z animacjami. PBR jest jaśniejszy i bardziej połyskliwy
  niż koncepcje: podczas integracji dopracować matowy shader i paletę. Pełne źródła wymagają
  siatki do gry, rigowania, testów ataków, ogonów/skrzydeł i wydajności grupy.
- `preview_meshy_model.py --profile demon` oraz `inspect_imp_details.py` odtwarzają podglądy/audyt.
  Wiatr i ziemia nadal tylko koncepcje, bez dodatkowych zleceń. Modele/definicje Unity oraz Hell Unleashed
  bez zmian. Bez testów gry, commita i pusha; nie wdrażano jeszcze tych modeli.

### Potwory pierwszej areny — 2026-10-27: koncepcje i wejścia do Meshy

- Sprawdzono rzeczywistą kompozycję „I. Wrota Otchłani”: ognisty chochlik `enemy_imp`, warianty
  `enemy_imp_frost`, `enemy_imp_storm`, `enemy_imp_earth`. Elita korzysta z bazowego ognia; nie jest osobnym gatunkiem.
- Wykonano osiem podglądów źródłowych z Unity i [cztery osobne koncepcje gotyckiego anime](../ArtSource/GothicAnimePoC/FirstArenaEnemies/README.md).
  Zachowany brak skrzydeł u ognia i skrzydła u pozostałych. Matowe malowane powierzchnie, palety żywiołów,
  naturalne rogi, pazury, ogony i detal. Wszystkie grafiki obejrzane; prompty i mapowanie źródeł zapisane.
- Wbudowany imagegen; **bez płatnej generacji Meshy**. Saldo odczytane: **94**. Przygotowane propozycje
  Meshy 7.1 Ultra 4K/PBR/FBX bez remeshu: 35/model, cztery 140 (brakuje 46), dwa 70 (pozostaje 24).
  Manifest `meshy-jobs-v1.pending.json` bez tasków. Następny krok: wybór zakresu i zatwierdzenie kosztu.
- To referencje, nie wdrożone modele. Oryginalni przeciwnicy w Unity i Hell Unleashed bez zmian.
  Bez ponawiania testów gry, commita i pusha.

### Nowy model maga z Meshy — 2026-10-27

- Użytkownik zlecił nowy model z Meshy i podkreślił zachowanie jakości, bez upraszczania.
  Przygotowana [pojedyncza referencja w pozie T](../ArtSource/GothicAnimePoC/Mage/meshy-reference-tpose-v1.png)
  na podstawie zaakceptowanej koncepcji, z zachowaniem warstw szaty, postrzępień, kapelusza, włosów, pasa i księgi.
  Kostur będzie oddzielnym dotychczasowym assetem, aby nie został połączony z dłonią i ubraniem.
- Odczytane saldo Meshy: **129 kredytów**. Przygotowane parametry Meshy 7.1 Ultra, geometria 4K,
  tekstury 4K/PBR, FBX, T-pose, bez remeshu i zmiany stylu wejścia: **35 kredytów** (30 + 5 Ultra).
  `meshy-job-v1.pending.json` oraz prompt referencji zapisane w `ArtSource/GothicAnimePoC/Mage/`.
- Użytkownik zatwierdził Meshy 7.1 Ultra za 35 kredytów. Jedno zadanie
  01a17c7e-1b4a-7543-95d9-ef055499ba4c zakończone **SUCCEEDED**, rzeczywisty koszt **35**, saldo **129 → 94**.
- [Pełny model źródłowy i raport](../ArtSource/GothicAnimePoC/Mage/meshy-v1/README.md):
  **3 619 624 trójkąty / 1 802 038 wierzchołków**, bez remeshu i lokalnej redukcji.
  Oryginalny FBX 175,19 MiB, plik Blender z samą siatką/spakowanymi teksturami i osobna scena studyjna.
  Kolor i normalne 4K; szorstkość/metaliczność 2K. Jeden materiał, UV, brak szkieletu i wag.
- Obejrzano rzeczywiste rendery przodu, tyłu, boków, ¾ i zbliżenia głowy, księgi, dłoni z góry.
  Zachowane warstwy i postrzępienia szaty, kapelusz, włosy, pas i księga; palce są rozdzielone.
  Audyt: brak niefinitywnych współrzędnych i zdegenerowanych powierzchni w przyjętym progu.
  Osobne otwarcie zapisanego pliku Blender potwierdziło spakowanie i odczyt **4/4 map**.
- To pełne wysokopoligonowe źródło do dalszego rigowania i przygotowania wersji do gry, nie ukończona
  siatka animowana. PBR w neutralnym studiu daje jaśniejszy wygląd niż koncepcja; shader/paleta Unity
  i deformacje długiej szaty wymagają oceny przy integracji. Oryginalny FBX pozostaje zachowany.
  tools/art/preview_meshy_model.py odtwarza audyt i podglądy bez redukcji geometrii.
- Obecny mag w Unity i Hell Unleashed bez zmian. Bez ponownych testów Unity, commita i pusha.

### Koncepcja maga — 2026-10-27: zachowanie obecnego charakteru

- Na prośbę użytkownika przygotowana [koncepcja gotyckiego anime](../ArtSource/GothicAnimePoC/Mage/mage-gothic-anime-v1.png)
  na bazie aktualnej klasy **„Mag” (`class_mage`, `mage_witch`)**. Użytkownik lubi istniejący model;
  kierunek jest zachowawczy: ten sam kapelusz, jasne włosy, ciemna poszarpana szata, kaptur, rękawice,
  księga przy pasie i kostur ognia. Zmiany dotyczą głównie malarskich płaszczyzn, włosów i matowego cieniowania.
- Dwa widoki pełnej sylwetki i detal głowy na jednej planszy. Bazą były obejrzane rendery modelu Unity,
  w tym neutralne podglądy albedo z oryginalnym wyposażeniem. Ilustracja paladyna użyta wyłącznie jako wzorzec stylu.
  [Źródła i pełny prompt](../ArtSource/GothicAnimePoC/Mage/README.md) zachowane w projekcie.
- Wbudowany `imagegen`; bez operacji Meshy/ElevenLabs. Model i materiały maga, jego animacje oraz przypisanie
  klasy **bez zmian**, potwierdzone pustym diffem właściwych assetów. Podglądy korzystały z tymczasowych kopii
  w scenie Preview, bez zapisu do sceny gry. Nie jest to wdrożenie nowego wyglądu; testów gry nie ponawiano.
  Bez commita i pusha. Kolejny krok: ocena koncepcji przez użytkownika przed ewentualnymi zmianami w modelu.

### Dłuższy naturalny krok — 2026-10-27: kolejne +30%

- Na prośbę użytkownika przesunięcie zwiększone **54 → 70,2 cm na cios (+30%)**, łącznie **1,404 m**
  po dwóch uderzeniach. Uniesienie buta **15 → 17 cm**; zmierzone maksimum **16,92 cm**.
- Postęp kroku wynika teraz z rzeczywistego czasu zamachu i trafienia, zamiast stałego podziału 55/45.
  Usunięte dodatkowe przyspieszenie nogi na granicy krótszej fazy aktywnej. Pomimo wydłużenia kroku,
  największa zmiana stopy w próbie spadła z 10,25 do **9,20 cm/kl.** przy 60 kl./s.
- Dostosowane dopuszczalne obniżenie miednicy do zasięgu nóg (maks. **7,5 cm**). Nowa kontrola pionowa
  wykryła wcześniej 5,2 mm oderwania podpory; po korekcie odchylenie poziome i pionowe <0,01 mm w pomiarze.
  Bez rozciągania kości; zachowane naprzemienne prowadzenie nóg, ruch buta i wcześniejsze ograniczenia przysiadu.
- **PlayMode 5/5**: miecz, prawdziwy korbacz, pełna seria w `Main`, regresja przysiadu i ciągłość przejścia.
  Galerie z boku i skosu obejrzane. Raport `TestResults/paladin-attack-steps/step-30-playmode-final.json`;
  poprzednia wersja zachowana w `before-30-percent/`. Bez ponawiania EditMode ani buildu gracza.
  Dotyczy paladyna; wojownik i Hell Unleashed bez zmian. Bez commita i pusha.

### Pełniejszy krok — 2026-10-27: kolejne +50% i wyraźne przeniesienie nogi

- Na prośbę użytkownika przesunięcie zwiększone o kolejne **50%: 36 → 54 cm na cios**, czyli **1,08 m**
  przy dwóch uderzeniach. But podnoszony na **15 cm** zamiast 9 cm. Krok zaczyna się wcześniej w zamachu;
  obrót buta pokazuje wybicie z palców, uniesienie noska i lądowanie przed płaskim podparciem.
- Miednica uwzględnia anatomiczny zasięg obu nóg: gdy wymaga tego dłuższa podstawa, obniża się o brakującą
  wysokość (maks. 6 cm), zamiast rozciągać goleń albo przesuwać stopę podporową. Zachowane wcześniejsze
  ograniczenia przysiadu, kolejność prawa/lewa i czas uderzeń.
- Miecz i korbacz: prowadzenie nogi **54 cm**, uniesienie **15 cm**, przesunięcie **1,080 m**;
  odchylenie podpory <0,01 mm w próbie obu kroków. **PlayMode 5/5**, w tym pełna seria w `Main`,
  regresja przysiadu i ciągłość drugiego zamachu. Bez ponawiania EditMode ani buildu gracza.
- Obejrzane galerie z przodu/skosu oraz nowy profil boczny: `TestResults/paladin-attack-steps/*-side.png`.
  Wariant 36 cm / 9 cm zachowany w `before-full-step/`; nowy wynik `full-step-playmode-final.json`.
  Dotyczy prywatnego zestawu paladyna. Hell Unleashed i wojownik bez zmian; bez commita i pusha.

### Korekta wykroku — 2026-10-27: większe przesunięcie i wyższy but

- Na prośbę użytkownika przesunięcie kontrolera zwiększone **3×: 12 → 36 cm na cios**, czyli **72 cm**
  po dwóch uderzeniach. Podniesienie buta zwiększone **2×: 4,5 → 9 cm**. Dopasowana długość wykroku;
  noga prowadząca kończy teraz 36 cm przed podporową. Zachowana naprzemienna kolejność i dotychczasowe tempo rąk.
- Pomiar miecza i prawdziwego korbacza: przesunięcie **0,720 m**, uniesienie **0,090 m**, odchylenie
  podpory maks. **1,09 mm**. Bez powrotu głębokiego przysiadu ani przeskoku przy drugim zamachu.
- Celowane **PlayMode 5/5**: dwa scenariusze wykroku, pełna seria w `Main`, regresja przysiadu i ciągłość
  przejścia korbacza. Galerie obu broni i TSV w `TestResults/paladin-attack-steps/`; poprzedni krótki wariant
  zachowany w `before-longer/`, nowy raport `longer-playmode-final.json`. Bez ponawiania EditMode ani buildu gracza.
  Opcja nadal dotyczy tylko prywatnego zestawu paladyna. Bez zmian Hell Unleashed, wojownika, commita i pusha.

### Ostatnia poprawka — 2026-10-27: naprzemienne wykroki w serii paladyna

- Zgodnie z opisem użytkownika: pozycja wyjściowa z lewą nogą z przodu, pierwszy cios — mały wykrok prawej,
  drugi cios z lewej na prawą — wykrok lewej. Prywatny zestaw jednoręczny paladyna ma teraz
  `alternatingLightSteps`; pozostałe zestawy, wojownik i współdzielone pliki animacji bez zmian.
- `AlternatingAttackSteps` dodaje niskie, zsynchronizowane z zamachem/trafieniem kroki po ewaluacji klipów.
  Stopa podporowa trzyma punkt w świecie; kolana prowadzone w przód, stałe długości nóg i sztywny but.
  Miednica podąża za skróconą podstawą podparcia, zachowując obrót, pochylenie i wysokość z klipu.
  Krótki krok zastępuje ruch korzenia tego ciosu, zamiast się z nim sumować. Utrzymany wypracowany wykrok
  podczas powrotu rąk do gardy; ruch/inne akcje wygaszają warstwę, większy zwrot i teleport zwalniają podparcie.
  Powtarzanie tego samego cięcia nie powiększa rozkroku bez końca; pchnięcie serii korzysta z tej samej podstawy.
- Próba przed zmianą odtworzyła problem: po pierwszym ciosie prawa stopa pozostawała **83,4 cm za lewą**,
  a dwa ciosy przenosiły kontroler o **1,68 m**. Po poprawce, miecz i prawdziwy korbacz paladyna:
  **prawa/lewa noga prowadzi kolejno o 12 cm**, podniesienie buta **4,5 cm**, łącznie **24 cm** przesunięcia sylwetki.
  Błąd położenia podpory <0,01 mm w pomiarze obu wykroków; największa zmiana stopy **3,63 cm/kl.** przy 60 kl./s.
  Osobno sprawdzone utrzymanie lewej nogi po powrocie do gardy i reset przy teleportacji.
- Celowane **PlayMode 10/10** przez otwarty edytor/MCP: dwa nowe scenariusze kroków, seria jednoręczna/dwuręczna,
  przejście korbacza, pełna seria w `Main`, oba zestawy biegu, sztywność butów i swobodny łańcuch.
  W rzeczywistej grze potwierdzone naprzemienne prowadzenie nóg. Tempo drugiego ciosu nadal **1,00×**,
  dłoń nie pozostaje nad barkiem; brak powrotu głębokiego przysiadu. Pełne **EditMode 199/0/1**
  (pominięty warunkowy eksport WAV). Bez pełnego PlayMode i buildu gracza.
- Galerie miecza/korbacza obejrzane. Raporty, osiem faz obu serii, TSV i pomiary:
  `TestResults/paladin-attack-steps/`; porównanie `sword-before.png`, `sword-before.tsv`, `baseline.json`,
  wyniki końcowe `playmode-final.json`, `editmode-final.json`. FBX i współdzielone klipy bez zmian;
  w prywatnym zestawie assetu zmieniła się wyłącznie flaga. Hell Unleashed bez zmian, bez commita i pusha.
  Kolejny krok: ocena wyczucia małych wykroków w walce z aktywnymi przeciwnikami.

### Ostatnia poprawka — 2026-10-27: paladyn na tle koncepcji areny

- Porównanie użytkownika pokazało istotną różnicę artystyczną mimo wcześniejszych zaliczonych prób matowości:
  zbyt jasne płyty, czarne plamy w cieniu i słabo czytelne płaszczyzny pancerza. Atlas Meshy zawiera już
  namalowane światła i cienie; kolejne mocne cieniowanie pogłębiało ten problem.
- `AnimeMatte` odwzorowuje neutralne fragmenty atlasu na przygaszoną stal i osobno stare złoto,
  zachowując rozpiętość namalowanych płaszczyzn, burgund i istniejące UV. Wypełnienie głównego cienia
  jest teraz neutralne/chłodne, bez dodatkowego mnożnika 0,35. Ograniczona suma świateł chroni kolor
  także przy kilku pochodniach. Łagodniejsze przejścia pasm, bez dodawania połysku lub odbić.
- Zmieniono wyłącznie materiał nowego paladyna i jego shader. `PaladinAnimeImport.UpdateMaterial()` / menu
  **Turris → PoC → Odśwież malowany materiał paladyna** pozwala odtworzyć ustawienia bez importu rigowania i klipów.
  Oryginalny atlas i FBX zachowane bajtowo; wygląd wojownika, animacje i Hell Unleashed bez zmian.
- Celowane **PlayMode 5/5** przez otwarty edytor/MCP: materiał, galeria paladyna na nowej arenie,
  start gry z zachowaniem wojownika, sztywność butów/galeria animacji oraz regresja mgły.
  **EditMode 3/3**: przypisanie wyłącznie paladyna, poprawność siatki/awataru i cztery przebiegi materiału.
  Shader Direct3D12 bez komunikatów kompilatora. Bez pełnych zestawów ani buildu gracza.
- Wzmocniona próba obrazu sprawdza zachowanie koloru płyty w cieniu oraz limit jasności przy świetle 8×;
  nadal wykrywa połysk PBR kontrolą dodatnią. Kamera pomiarowa rozgrzewana przed odczytem pikseli.
  Obrazy sprzed poprawki i po niej obejrzane: `TestResults/paladin-look/before/` i `current/`,
  przód, tył, bok i normalna kamera gry; raporty `playmode-final.json` i `editmode-final.json`.
- **Pozostała różnica względem koncepcji:** poszarpane detale atlasu generatora oraz niespójne stylistycznie
  dotychczasowe bronie/tarcza. Korekta materiału nie jest przemalowaniem atlasu i nie oznacza osiągnięcia
  docelowej jakości ilustracji. Kolejna praca artystyczna: oczyszczenie krawędzi i podziałów tekstury,
  opracowanie detali płyt oraz dopasowanie oręża do tego samego języka graficznego.
  Bez operacji Meshy/ElevenLabs i bez nowego commita.

### Ostatnia sesja — 2026-10-26: nowy zestaw areny gotyckiego anime

- Zlecony commit wcześniejszych prac wykonany: **`568e119`** na `codex/paladin-gothic-anime` — paladyn,
  animacje, korbacz, materiały, źródła PoC i dokumentacja. Bez pusha. Poniższe zmiany areny są osobną pracą,
  pozostawioną w drzewie roboczym.
- [Nowy zestaw](../ArtSource/GothicAnimePoC/Arena/README.md) według [koncepcji użytkownika](../ArtSource/GothicAnimePoC/Arena/reference-courtyard.png):
  **30 modułów 3D**, w tym 14 płyt z ukruszeniami i rozłupaniem, murowane przęsła, profile i klińce ostrołuków,
  żłobione całe/złamane kolumny, powalone trzony, fałdowane sztandary z haftem, kute kosze pochodni,
  sześć odłamków oraz zamek z arkadowym mostem na skale. Nowe siatki, prefabrykaty i materiały zapisane w
  `Models/PoC/GothicArena`; źródło `.blend`, skrypt modelowania i raport geometrii w katalogach ArtSource/tools.
- Malowane nocne niebo z księżycem i tekstura wapienia powstały przez wbudowany `imagegen`.
  Prompty i pliki źródłowe są zapisane; gra korzysta wyłącznie z kopii w projekcie.
  Niebo jest tłem, a architektura, posadzka, ozdoby i zamek — geometrią 3D. **Bez Meshy i ElevenLabs; 0 ich kredytów**.
- `GothicStone`: paleta wierzchołków, mapowanie trójpłaszczyznowe detalu kamienia, matowe światło pasmowe,
  chłodne cienie, obsługa świateł dodatkowych i dwustronnych sztandarów z wiatrem.
  `GothicNightSky`: panorama z pojedynczym księżycem. Pochodnie korzystają z dotychczasowych efektów HU w Unity.
  Poprawiono pomylenie widoczności z ilością mgły; oddzielny test obrazu wykrywa tę regresję.
- Nowa definicja `arena_courtyard_gothic.asset` jest przypisana **tylko do pierwszego piętra**.
  Oryginalne trzy definicje aren, ich meta i `ArenaBuilder.cs`: **7/7 plików identycznych bajtowo**
  (`ArtSource/GothicAnimePoC/Arena/legacy-sha256.json`). Piętra III/V nadal korzystają ze starego dziedzińca.
  Zachowane rozmiar areny, pozycje kolumn, płaska kolizja na y=0 i zasady walki/portali.
- Przywrócenie: zatrzymać Play, **Turris → PoC → Arena gotycka → Przywróć poprzedni dziedziniec**.
  Polecenie sprawdzone przez MCP: pierwsze piętro wróciło do `arena_courtyard`; następnie ponownie włączono nowy zestaw.
  Przełącznik zachowuje nowe assety. Parametry oświetlenia i nieba dobierane są na zmianie areny.
- Budowa: **33 renderery siatek, 144 910 trójkątów, 30 prostych kolizji**, bez MeshColliderów.
  Moduły statyczne scalone przestrzennie; siatki instancji zwalniane przy zniszczeniu areny.
  Oba nowe shadery sprawdzone na Direct3D12; kamień ma przebiegi Forward/ShadowCaster/DepthOnly/DepthNormals.
- Pełne **EditMode: 199 zaliczonych, 0 błędów, 1 pominięty** (warunkowy eksport WAV).
  **18 różnych scenariuszy PlayMode zaliczonych**: 17 w przebiegu regresji i ponowiony test galerii po poprawce
  rozgrzewki renderowania do stałego RenderTexture. Obejmuje nową arenę, mgłę, kolizje, płomienie oraz regresje
  paladyna, biegu i korbacza. Bez pełnego PlayMode ani buildu gracza.
- Wyniki i obejrzane kadry: `TestResults/gothic-arena/*-final.json`, `01-gameplay.png`, `02-concept-angle.png`,
  `03-architecture.png`, `04-floor-detail.png`, `05-torch-detail.png`. Zrzut rozgrzewający poprzedza właściwą galerię;
  ustabilizowane płomienie są wstępnie symulowane, a kamera galerii nie reaguje na ruch myszy na pulpicie.
- Hell Unleashed i wygląd wojownika bez zmian. Kolejna ocena artystyczna dotyczy zgodności kompozycji,
  światła, skali ruin i materiałów z ilustracją; nie utożsamiać zielonych testów z identycznością ilustracji.

### Poprzednia poprawka — 2026-10-26: ciągłość serii i paleta gotyckiego paladyna

- Odtworzony nienaturalny przeskok pomiędzy pierwszym a drugim ciosem: poprzednia poza była zamrażana,
  przejście do odległej pozy trwało około 0,11 s, po czym krótki klip czekał w przygotowaniu. W nowych pomiarach
  przy różnych chwilach przerwania pierwszego ciosu dłoń przemieszczała się do **28,19 cm/kl.**, barki do **28,42°/kl.**
- Wyłącznie prywatne drugie cięcie paladyna ma `preparationBlendShare=1`: wejście rozłożone na zapowiedź,
  poprzedni klip przez chwilę kontynuuje ruch z wygaszaną prędkością, a krótki wycinek łagodnie zaczyna zamach.
  Pozostałe klipy zachowują wcześniejsze przenikanie. Wycinek nadal **18–24**, wczesny powrót do gardy,
  tempo aktywnego cięcia **1,00×**; parametry walki i chwila trafienia bez zmian.
- Nowa próba `FlailCombo_SecondHitHasContinuousPreparation` porównuje trzy momenty przejścia (25/55/90% regeneracji)
  i mierzy dłoń, barki, biodra. Po poprawce: **10,07 cm/kl.**, zmiana kroku dłoni maks. **2,87 cm/kl.**,
  barki **9,17°/kl.**, biodra **0,85 cm/kl.**; także sprawdzenie pełnej zapowiedzi w scenie `Main`.
- Zbroja przybliżona do [wzoru użytkownika](../ArtSource/GothicAnimePoC/Paladin/reference-palette.png):
  przygaszona ciepła stal, stare złoto obrzeży, ciepłe cienie zamiast błękitnych. Korekta palety w `AnimeMatte`
  korzysta z istniejącej tekstury i UV, osobno rozpoznaje neutralne płyty i beżowe okucia, zachowuje burgund tkaniny.
  Niższy udział światła otoczenia i limit światła głównego zapobiegają wybielaniu płyt na arenie. Materiał nadal matowy,
  bez połysku i odbić; shader ma cztery przebiegi, bez błędów Direct3D12. Zrzuty z galerii i normalnej kamery gry obejrzane.
- Próby obrazu potwierdzają zachowaną matowość, światła dodatkowe, ciepłą stal/złoto i ciemniejszy cień.
  Poprzedni wygląd i pomiary w `TestResults/paladin-anime/before-transition-palette/`;
  bieżący tor w `combo-transition.tsv`, galeria `combo-transition.png`, wygląd w `paladin-idle.png` i `paladin-game.png`.
- Celowane **PlayMode 15/15** i pełne **EditMode 196/0/1** (pominięty warunkowy eksport WAV) przez MCP.
  Wyniki `TestResults/paladin-anime/transition-palette-*.json`; bez pełnego PlayMode i buildu gracza.
- Sumy SHA256 nadal **16/16 plików wojownika i 336/336 współdzielonych plików animacji bez zmian**.
  Hell Unleashed bez zmian, bez operacji Meshy. Commit na prośbę użytkownika na `codex/paladin-gothic-anime`, bez pusha.
  Następny krok: ocena wyczucia przejścia i palety w zwykłej grze.

### Poprzednia poprawka — 2026-10-26: krótszy drugi cios i swobodny korbacz

- Drugi lekki cios nowego paladyna kończy się wcześniej: prywatny `paladin_slash_left.fbx` obejmuje teraz
  **klatki 18–24** zamiast 18–33. Wycięto końcówkę z unoszeniem ręki. Po końcu klipu warstwa akcji płynnie
  wygasa do gardy, również przed zakończeniem logicznej regeneracji. Obrażenia, koszty i czasy akcji bez zmian.
  Pomiar retargetowanego modelu w serii: dłoń maks. **1,285 m** (bark w gardzie 1,261 m), pod koniec wraca do gardy.
- Odtworzone zwolnienie krótkiego cięcia przy wolniejszym korbaczu: około **0,70×** nagrania w próbie bazowej.
  Mapowanie czasu korzysta z faktycznych czasów zapowiedzi/trafienia i nie rozciąga krótkiego cięcia.
  Obecnie **1,00×**, także w serii uruchomionej przez `PlayerCombat` w prawdziwej scenie `Main`.
  Pozostałe klipy zachowują dotychczasowe ustawienia; czasy faz przekazywane wyłącznie dla lekkich/mocnych ataków.
- `HellFlailChain` oparty na HU `FlailChain`: 14 ogniw, Verlet w świecie, grawitacja 9,8, masa kuli 12,
  12 iteracji ograniczeń i podkroki do 1/120 s. Aktualizacja po końcowej pozie dłoni/broni, także podczas hitstopu;
  pauza zatrzymuje symulację. Bezwładność, reset przy teleportacji, ograniczenie długiej klatki i stałe długości kości.
  Kula porusza istniejącą siatką poprzez obroty kości; rozszerzone granice renderera i `updateWhenOffscreen`.
  `WeaponTip` podąża za kulą. Bez zapisu do modeli źródłowych ani Hell Unleashed.
- Celowane **PlayMode 14/14** przez MCP: seria i wysokość dłoni, tempo korbacza, oba zestawy biegu,
  matowy materiał, buty, niezależny ruch kuli i deformacja siatki, start w `Main` oraz cztery regresje wcześniejszych modeli.
  Seria jednoręczna: biodra min. **0,764 m**, zmiana na klatkę maks. **2,8 cm**, cofnięcie podparcia maks. **1,8 cm**.
  Niezależny ruch kuli **0,952 m**; galeria `combo-onehand.png`, `flail-chain-cycle.png` i `TestResults/flail/flail-*.png` obejrzana.
  Wyniki: `TestResults/paladin-anime/flail-final-playmode.json`; pełnego PlayMode ani buildu gracza nie wykonywano.
- Pełne **EditMode: 196 zaliczonych, 0 błędów, 1 pominięty** (warunkowy eksport WAV), w tym oba korbacze,
  ciągłość długości łańcucha, bezwładność, pauza, długa klatka i teleport. Wynik w `flail-final-editmode.json`.
- Sumy SHA256: **336/336 współdzielonych plików animacji i 16/16 plików wojownika bez zmian**.
  Bez operacji Meshy, commita i pusha. Kolejny krok: ocena wyczucia krótszego ciosu podczas zwykłej walki.

### Poprzednia poprawka — 2026-10-26: tempo korbacza i ruch kuli

- Przy krótkim `paladin_slash_left` (0,5 s) wolniejszy korbacz rozciągał odcinek trafienia do około **0,77×**
  tempa nagrania. `PlayerCombat.GetAnimState` przekazuje teraz rzeczywiste czasy zapowiedzi i okna trafienia.
  `ClipAnimationDriver.NormalizedTime` dla krótkich wycinków z jawnym `strikeShare` zachowuje co najmniej tempo
  nagrania: nadmiar zapowiedzi jest przygotowaniem, przejście przez cel ciągłe na granicy faz. Wynik **1,00×**.
  Obrażenia, koszty i czasy akcji bez zmian. Pozostałe klipy zachowują dotychczasowe mapowanie.
- Sprawdzony istniejący, niezacommitowany port `HellFlailChain` względem HU `FlailChain`: 14 ogniw,
  symulacja Verlet w świecie, grawitacja, bezwładność ciężkiej kuli, ograniczenia długości, obroty kości.
  Siatka reaguje na kości. Włączone `updateWhenOffscreen` na siatkach korbacza, aby przeliczać skórę i obrys
  również poza obrysem spoczynkowym. Kod i pliki Hell Unleashed bez zmian.
- `FlailPlayTests`: tempo i ciągłość krótkiego cięcia, korbacz w dłoni nowego paladyna, ugięcie siatki,
  stała długość łańcucha i pauza. Galeria `TestResults/flail/flail-*.png` obejrzana.
  **4/4 celowane PlayMode** (w tym regresje obu serii paladyna); pełny **EditMode 191/0/1** przez MCP.
  To próby sterownika i uzbrojonego modelu; pełnego PlayMode ani buildu gracza nie wykonywano.

### Poprzednia poprawka — 2026-10-26: przysiad podczas serii ataków paladyna

- **Odtworzone:** w drugim lekkim ciosie zestawu jednoręcznego (`sword and shield slash (5)`) biodra paladyna
  opadały z **0,888 do 0,496 m** — spadek **39,2 cm**, do **7,7 cm w klatce**. Głęboki przysiad pochodzi z klipu,
  nie z korekty biegu ani kumulowania przesunięcia modelu. Seria dwuręczna bez tego problemu.
- `PaladinAnimeImport.StandingComboSet`: osobny **`anim_paladin_anime_onehand.asset`** tylko dla nowego paladyna.
  Drugie cięcie korzysta z lokalnej kopii `sword and shield slash (3)`, **klatki 18–33** (drugi, stojący zamach,
  koniec przed cofnięciem nogi), zapisanej jako `Models/PoC/PaladinAnime/paladin_slash_left.fbx`.
  Zsynchronizowane granice zamachu/trafienia i krzywa kroku z `RootT.z`. Parametry walki, obrażenia i czasy akcji bez zmian.
  Pozostałe akcje, bieg/chód i zestaw dwuręczny zachowują dotychczasowe klipy.
- Nowe próby `OneHandCombo_DoesNotDropIntoCrouch` i `TwoHandCombo_DoesNotDropIntoCrouch`:
  seria różnych ciosów i powtarzanie tego samego cięcia, wysokość bioder, płynność przejść i cofnięcie podparcia.
  Po zmianie seria jednoręczna min. **0,764 m**, spadek **12,5 cm**, zmiana na klatkę max **3,4 cm**;
  dwuręczna spadek **14,9 cm** (bez podmiany). Arkusze `combo-onehand.png` i `combo-twohand.png`, pomiary TSV;
  poprzednie wyniki i zrzuty w `TestResults/paladin-anime/before-combo-fix/`.
- Pełne EditMode **191/0/1** przez MCP. Sumy SHA256: **336/336 współdzielonych plików animacji oraz 16/16 plików wojownika bez zmian**.
  Bez zmian Hell Unleashed, operacji Meshy, commita i pusha.

### Poprzednia poprawka — 2026-10-26: węższy tor biegu i ruch tułowia paladyna

- Uwaga użytkownika po poprzedniej poprawce: bieg nadal sztywny, nogi zbyt szeroko, tułów nie pracuje.
  Poprzednie pomiary dotyczyły kostki **względem biodra** i kości buta; nie oceniały szerokości względem osi ruchu ani pracy siatki tułowia.
- **Tor stóp odniesiony teraz do osi ruchu postaci.** W biegu cel ±5,5 cm zamiast prowadzenia pod biodrami,
  w chodzie ±8 cm; zachowana mała część bocznej trajektorii z klipu. W próbie podpora od osi maks. około **6,3 cm**
  (wcześniej **12,86 cm** miecz/tarcza i **10,27 cm** broń dwuręczna).
- Poluzowana korekta nóg: zachowana ograniczona praca płaszczyzny zgięcia kolana (do 3,6° z nagrania),
  mała zmienność kierunku buta (do 4°) i łagodniejsze pochylenie stopy w wymachu. Stałe długości kości i sztywne buty zachowane.
- **Przyczyna sztywnego tułowia:** `Spine` i `Spine1` miały łączną wagę **0**; siatka brzucha i klatki wisiała na `Spine2`.
  Szkielet już niósł ruch, ale widoczna bryła pozostawała jednym blokiem. `rig_anime_paladin.py` rozkłada teraz wagi
  na biodra, lędźwie i obie części klatki, z płynnymi przejściami. Praca siatki brzucha względem klatki w cyklu
  **9,23° / 7,46°** dla obu zestawów. Nie dodawano sztucznego kołysania całej postaci ani zmieniano klipów współdzielonych.
- Rozszerzone próby `PaladinAnimePlayTests`: tor stopy od środka postaci, obrót miednicy/barków,
  rzeczywista praca **siatki** brzucha względem klatki i arkusze ośmiu faz cyklu z przodu oraz boku.
  `PaladinAnimeTests` wykrywa teraz również nieużywane kości tułowia. Pełne EditMode **191/0/1** przez MCP.
- Celowane PlayMode **7/7**: oba zestawy przy 5,2 i 8,06 m/s, rzeczywisty bieg w `Main`, galeria ośmiu stanów,
  matowość i regresje dotychczasowych modeli. Tor podparcia maks. **6,28 / 6,16 cm** od osi; kolano od płaszczyzny
  kroku maks. **2,29 / 2,15 cm**. Buty zachowują wymiary (<0,001 mm w galerii), kontrolowany kierunek do 4°;
  długości ud i goleni stałe (<0,01 mm). Podpora w grze: przesunięcie na klatkę p95 **2,01 cm** przy 60 kl./s.
  Arkusze cyklu obu zestawów oraz widok z normalnej kamery gry obejrzane. Wynik dotyczy edytora; bez buildu gracza i pełnego PlayMode.
- Wyniki i obrazy: `TestResults/paladin-anime/fluid-*.json`, `run-feet-cycle*.png`, `run-twohand-cycle*.png`;
  poprzedni tor i pomiary zachowane w `before-fluid-run/`. Wojownik: **16/16 plików nadal identycznych**, bez operacji Meshy,
  zmian Hell Unleashed, commita i pusha. Matowy materiał pozostaje jak po poprzedniej poprawce.

### Poprzednia poprawka — 2026-10-26: anatomia biegu paladyna anime i matowa zbroja

- **Poprawiona przyczyna szerokiego kroku w modelu:** poza T obejmuje teraz pionowe nogi pod biodrami oraz płaskie,
  skierowane do przodu buty. Importer aktualizuje zapamiętaną w meta pozę szkieletu z aktualnego FBX.
  Jawne mapowanie Humanoid obejmuje **22 kości** (wcześniej automatyczne: 16, brak palców, barków i części kręgosłupa).
  Brak `Left/RightToes` wyłączał wcześniej `CaptureRestFeet` i korekty stóp mimo formalnie poprawnego awatara.
- **Błąd źródłowego retargetu potwierdzony pomiarem:** skręt lewej stopy w klipie biegu miecz/tarcza
  `−2,71…−0,97` jednostki mięśnia Humanoid, skręt goleni `0,68…1,25`; zapisane cele IK odtwarzały wadliwą pozę
  także po edycji samych krzywych mięśni. Sam test sztywności siatki buta nie wykrywał tego problemu.
- `ClipAnimationDriver.AlignArmoredRunFeet`, **opcja `sagittalRunFeet` włączona wyłącznie w `visual_paladin_anime`**:
  kostki pod biodrami, kolana zginają się do przodu w płaszczyźnie kroku, usunięty boczny skręt butów,
  ograniczone zgięcie kostki; stałe długości segmentów zamiast rozciągania goleni przez Humanoid.
  Zachowane cele wykroku/wysokości (w granicach zasięgu nóg), kadencja i dotychczasowe klipy; korekta wygasa przy akcjach całego ciała.
  Bieg na skos uwzględnia obrót bioder; osobne klipy ruchu w tył/bokiem zachowują swoje prowadzenie.
- **Matowa zbroja:** nowy `Shaders/AnimeMatte.shader`, przypisany tylko do materiału nowego paladyna.
  Oświetlenie rozproszone w pasmach, cienie i światła dodatkowe Forward+, bez metalicznej BRDF, połysku i odbić otoczenia.
  `TintSet` i statusy HU pozostają obsługiwane; moknięcie zmienia barwę, nie przywraca połysku.
- **Weryfikacja MCP, Unity 6000.6.3f1 / URP / Linear / Direct3D12:** pełne EditMode **191/0/1**;
  celowane PlayMode **7/7**: pięć prób paladyna oraz dwie regresje biegu dotychczasowych modeli/wojownika.
  Shader wspierany, 0 komunikatów błędów kompilacji; przebiegi Forward, ShadowCaster, DepthOnly i DepthNormals.
  Bieg **5,2 i 8,06 m/s**, oba zestawy broni, po 360 analizowanych klatek na zestaw:
  kostka od biodra w bok maks. **1,60 cm / 1,39 cm**, kolano poza płaszczyzną <0,01 cm,
  boczne odchylenie butów <0,01°, długości kości stałe (błąd numeryczny <0,01 mm).
  Test materiału: różnica koloru przy zmianie widoku **0**, kontrola PBR **0,06275**; światło punktowe odbierane.
  Rzeczywisty bieg paladyna sterowanego wirtualnym padem w `Main`: przesunięcie stopy podporowej na klatkę p95 **2,01 cm** przy 60 kl./s.
- Obejrzane fazy z przodu i boku dla obu zestawów broni oraz `TestResults/paladin-anime/paladin-game-run.png`
  z normalnej kamery gry. Raporty `final-editmode.json`, `final-playmode.json`, przebiegi `run-feet.tsv` i `run-twohand.tsv`.
  **16 plików wojownika nadal identycznych bajtowo.** Hell Unleashed bez zmian; 0 operacji Meshy, bez commita/pusha.
- Zakres dowodu: edytor i wskazane scenariusze; bez pełnego PlayMode, buildu gracza i pomiaru GPU.
  Dalsza ocena: bieganie po nierównych płytach i przejścia ruch–walka w zwykłym graniu; przenikanie tabardu/naramienników w skrajnych pozach.

### Wdrożenie — 2026-10-26: model gotyckiego anime jako paladyn

- Branch **`codex/paladin-gothic-anime`**, utworzony z `main`; bez commita/pusha.
  Aktualna decyzja użytkownika: **nowy model zastępuje wygląd paladyna, wojownika nie zmieniamy**.
  [Raport, pliki i odtworzenie](../ArtSource/GothicAnimePoC/Paladin/README.md), [stan planu](PLAN_WDROZENIA_WOJOWNIKA_ANIME.md).
- `tools/art/rig_anime_paladin.py`: źródło `warrior-v2-grip.blend`, kopia 26-kościowego szkieletu z modelu wojownika już w Turris,
  dopasowanie stawów, wagi, wypieczenie pozy T. Źródłowe modele i Hell Unleashed bez zmian; 0 nowych kredytów Meshy.
  Bone heat nie znalazł rozwiązania na siatce generatora; zastosowano jawne wagi regionów anatomicznych, ze sztywnym chwytem i butami.
- Nowy FBX, mapa 2K i materiał: `Models/PoC/PaladinAnime/`; 45 832 trójkąty, jeden materiał ciała, Humanoid z własnym awatarem.
  Plik roboczy: `ArtSource/GothicAnimePoC/Paladin/paladin_anime_rigged.blend`. Brak nieważonych wierzchołków.
- `PaladinAnimeImport.Run` (menu **Turris → PoC → Włącz model anime dla paladyna**) tworzy `visual_paladin_anime.asset`
  na wzorcu paladyna: obecne klipy i zestawy broni, `scale=1`, `footLength=1`, sztywne noski.
  Zapisuje wyłącznie **`class_hu_paladin.lookOverride`**; oryginalne `visual_paladin` i `class_hu_paladin.visual` zachowane.
  Broń, cechy, umiejętności, runtime animacji i balansu bez zmian. Powrót: wyczyścić lookOverride tylko paladyna.
- **Weryfikacja przez MCP w otwartym Unity 6000.6.3f1 / URP / Linear:** EditMode **190 zaliczonych / 0 błędów / 1 pominięty**;
  celowane PlayMode **2/2** (`PaladinAnimePlayTests`: start rzeczywistego podejścia paladynem i galeria ośmiu stanów).
  Nowe `PaladinAnimeTests`: przypisanie tylko paladyna, zachowanie klipów, awatar, wagi, tekstura.
  Spoczynek, bieg, blok, cięcia, unik, gest czaru i śmierci: maks. deformacja punktów buta względem kości ≈ **0,001 mm**;
  chwyt i gniazdo również pozostają przy kości dłoni (wynik `TestResults/paladin-anime/grip.tsv`).
- Galeria **`TestResults/paladin-anime` obejrzana**, także ujęcie z normalnej kamery pierwszej areny.
  Pułapka: po ręcznym próbkowaniu animacji przed `Camera.Render` trzeba odczekać klatki aktualizacji skinningu;
  inaczej obraz łączy poprzednią siatkę z nowym położeniem broni. Poprawiono galerię, nie było potrzeby zmieniać broni.
- **Wojownik: 16 sprawdzonych plików identycznych bajtowo** (klasa, definicja, model i pliki towarzyszące), raporty sum w `TestResults/paladin-anime/warrior-*.json`.
  Unity pozostawione w trybie edycji, scena `Main`, bez trwających testów. Nie wykonywano pełnego PlayMode ani buildu gracza.
- Do oceny w graniu: spasowanie naramienników/tabardu przy skrajnych pozach i estetyka jasnej zbroi przy pochodniach;
  dalsze malowanie/toon i pozostałe assety PoC to osobny etap. Wojownik nadal korzysta z dotychczasowego wyglądu.

### Poprzednia sesja — 2026-10-23/26: audyt skilli i pieczęci (pomiar na manekinach) i korekty

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

- **Git LFS (2026-10-31):** po scaleniu/commicie bez smudge LFS wszystkie `*.fbx`/`*.blend` zostały w katalogu roboczym jako 132-bajtowe wskaźniki – Unity loguje „Couldn't read file … None of the registered readers”, a w grze nie ma bohatera ani demonów (arena była widoczna). Naprawa: `git lfs pull` przy zamkniętym edytorze; kontrola: `git lfs ls-files` ma pokazywać `*`, nie `-`.

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
