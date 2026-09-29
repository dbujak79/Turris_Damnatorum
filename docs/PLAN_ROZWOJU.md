# Plan rozwoju – zestawy żywiołów, umiejętności, bronie, przedmioty

Utworzony: 2026-09-29. Plik jest żywy: po każdym zadaniu aktualizuję znaczniki i dziennik na dole.
Stan ogólny i pułapki techniczne są w [PROGRESS.md](PROGRESS.md), opis systemów w [README.md](../README.md).

Legenda: `[ ]` do zrobienia · `[~]` w toku · `[x]` zrobione (z testem) · 🟢 dane · 🟡 trochę kodu · 🔴 nowy system

## Cel

Rozbudować to, co już działa (żywioły, reakcje, krwawienie, postawa, riposta), tak żeby nowe elementy tworzyły
**wyraźne zestawy (buildy)**: ogień, mróz, błyskawica, krwawienie, a także ciężka broń i obrona. Każdy zestaw ma mieć
co najmniej: broń, 1–2 przedmioty, czar lub technikę oraz wroga, który go „testuje”.

## Etap A – fundamenty w kodzie (potrzebne treści z etapu B)

- [x] **A1. Reakcja „Roztrzaskanie”** 🟡 – błyskawica w zamrożony cel: wybuch obszarowy (część obrażeń trafienia) wokół celu, zamrożenie znika. Uzupełnia trójkąt reakcji (ogień+mróz, błyskawica+krew, błyskawica+mróz).
- [x] **A2. Odporności bohatera na żywioły** 🟡 – statystyki `FireResist`, `FrostResist`, `LightningResist` (% redukcji części żywiołu, limit 75%), widoczne w statystykach; pancerze i pierścienie mogą je dawać.
- [x] **A3. Premie żywiołów i efektów po stronie atakującego** 🟡 – statystyki `FireDamage`, `FrostDamage`, `LightningDamage` (%), `BleedDamage` (%) oraz modyfikatory efektów przenoszone w trafieniu: dłuższe podpalenie, więcej warstw krwawienia, mocniejsze krwawienie w ruchu, mniej warstw chłodu do zamrożenia, silniejsze przewodzenie (i skok na kolejnego wroga).
- [x] **A4. Broń z żywiołem i premie sytuacyjne** 🟡 – `AttackDefinition.elementShare` (część obrażeń fizycznych zamieniona na żywioł), `bonusVsFrozen` (mnożnik na zamrożonych, rozbija lód), `bonusVsBleeding`, `executeBonus` (cel < 30% życia lub z przełamaną postawą).
- [x] **A5. Nowe rodzaje umiejętności** 🟡 – `Warcry` (wzmocnienie obrażeń + wytrzymałość), `Meteor` (zapowiedziany krąg, wybuch z opóźnieniem), `Storm` (pioruny w losowych wrogów przez czas), `BloodPact` (życie za darmowe czary), `FrostArmor` (osłona z odwetem chłodu), `Pull` (przyciąganie), `Counter` (postawa kontry → automatyczna riposta), `Rupture` (rozdarcie ran).
- [x] **A6. Pasywne efekty przedmiotów** 🟡 – `DodgeShockCharge` (po uniku następny cios poraża), `BurnImmunity` + `BurningDamageBonus` (płonąc, zadajesz więcej).

## Etap B – treść (głównie dane)

- [x] **B1. Bronie** 🟢 – Płonący miecz, Mroźny topór, Młot burzy (żywioł na ciężkim ataku), Ząbkowany miecz (krwawienie na każdym lekkim), Buława (postawa, ×1,5 na zamrożonych), kostury: Ognia, Lodu, Burzy (premia do żywiołu).
- [x] **B2. Przedmioty** 🟢 – Pierścień rozżarzenia, Amulet przewodnika, Rękawice rzeźnika, Pierścień szronu w żyłach, Buty burzy, Płaszcz popiołu; zbroje/hełmy z odpornościami na żywioły.
- [x] **B3. Czary** 🟢 – Iskra, Lodowa włócznia, Meteor, Burza, Krwawy pakt, Mroźna zbroja, Łańcuchy potępionych.
- [x] **B4. Techniki** 🟢 – Kontra, Cięcie z wyskoku, Egzekucja, Okrzyk wojenny, Rozdarcie ran.
- [x] **B5. Wrogowie żywiołów** 🟡 – Płonący ghul (podpala, słaby na mróz), Lodowy strażnik (chłód, odporny na mróz, słaby na ogień), Kultysta burzy (poraża, słaby na krwawienie); piętra losują wariant żywiołu zamiast zwykłego wroga.
- [x] **B6. Wpięcie w meta-postęp** 🟢 – pule nagród i odblokowania za popiół dla nowych broni, przedmiotów i umiejętności; synchronizacja assetów (`SyncContent`).

## Etap C – poziomy umiejętności z nowymi cechami 🟡

- [x] **C1.** Model: `SpellDefinition.levelFeatures` – poziom II i III dodaje cechę (np. Kula ognia III zostawia płonącą ziemię, Łańcuch III – 6 celów, Szarża III – drugi ładunek, Młynek III – końcowe cięcie).
- [x] **C2.** Opis ulepszenia w nagrodzie pokazuje, co realnie dochodzi.

## Etap D – kapliczka jako sklep 🟡

- [x] **D1.** Za dusze: ulepszenie umiejętności, przerzucenie nagród (raz na piętro), zakup losowej umiejętności na to podejście. Sklep jest między **każdym** piętrem; kapliczka dalej daje ulepszenie broni i flaszkę.
- [x] **D2.** Ceny rosnące z numerem piętra (inflacja jak w inspiracji).

## Etap E – przemapowanie przycisków 🔴

- [x] **E1.** Ekran ustawień sterowania (osobno pad i klawiatura), zapis w profilu, podpowiedzi w HUD z bieżącego układu.

## Etap F – nowe modele i trudniejsze mechaniki 🔴

- [x] **F1.** Modele broni: włócznia (przebija w linii), kosa (leczy na krwawiących), młot, buława – `WeaponModel` + geometria w `GearBuilder`.
- [x] **F2.** Ciężki rzut (broń wraca po chwili, w tym czasie pięści).

## Następne kroki

0. ~~G – cechy postaci~~ – zrobione.
1. ~~E1 – przemapowanie przycisków~~ – zrobione. (osobno pad i klawiatura): ekran ustawień, `PerformInteractiveRebinding`, zapis nadpisań w profilu, podpowiedzi w HUD z bieżącego układu.
2. **F1 – modele broni**: buława, młot i włócznia mają dziś zastępcze modele (topór/wielki topór); własna geometria w `GearBuilder` + zrzuty.
3. **Balans w ręcznym graniu**: ceny sklepu, siła roztrzaskania (80%), częstość wariantów (40%), meteor/burza.

## Etap G – cechy postaci (prośba z 2026-09-29) 🟡

Cztery cechy zamiast sześciu atrybutów. Klasa ustawia wartości startowe, po każdym piętrze gracz rozwija jedną cechę o 1.

| Cecha | Działanie |
|---|---|
| **Siła** | wymagania cięższych broni (bez niej broń słabsza *i wolniejsza*), obrażenia fizyczne przez skalowanie broni |
| **Zręczność** | szansa na uchylenie (od 13 pkt, 1,5%/pkt, limit 25%), dłuższy unik (+2%/pkt ponad 10, limit +25%), szybkość ataku (+1%/pkt ponad 10, limit +20%), wymagania lekkich broni |
| **Inteligencja** | mana i moc czarów; **twarde** wymagania czarów (bez niej czaru nie da się rzucić) |
| **Wytrzymałość** | życie, udźwig i pula wytrzymałości |

- [x] **G1.** Model: `Vigor` → `Toughness` (ta sama wartość w zapisie), Kondycja i Umysł wycofane – stare modyfikatory w assetach przeliczane na Wytrzymałość/Inteligencję; parametry w `BalanceConfig` (z `FormerlySerializedAs`).
- [x] **G2.** Działanie cech: uchylenie, długość uniku, szybkość ataku, kara za niespełnione wymagania broni, twarde wymagania czarów.
- [x] **G3.** Punkt cechy po każdym piętrze (panel między piętrami), wartości startowe klas.
- [x] **G4.** UI: cechy i pochodne w przygotowaniu, ekwipunku i opisach; testy.

## Kolejność dalszej realizacji

G (cechy) → E1 (przemapowanie przycisków) → F1 (modele broni) → F2 (ciężki rzut) – zrealizowane w tej kolejności.

## Zasady realizacji

- Po każdym zadaniu: testy (EditMode; PlayMode w oknie dla zmian w walce), znacznik w tym pliku, wpis w dzienniku.
- Nowa treść trafia do assetów przez `TurrisSetup.SyncContentBatch` (tylko dodaje i uzupełnia pola domyślne).
- Efekty wizualne sprawdzam zrzutami (`TURRIS_SHOT_DIR`, tryb `window`) przed ogłoszeniem.
- Wartości balansu – w `BalanceConfig`/assetach, nie w kodzie.

## Dziennik postępów

| Data | Zadanie | Wynik |
|---|---|---|
| 2026-09-29 | Plan utworzony | – |
| 2026-09-29 | A1–A6: roztrzaskanie, odporności i premie żywiołów, modyfikatory efektów w trafieniu, broń z żywiołem (`elementShare`), premie przeciw zamrożonym/krwawiącym, egzekucja, 8 nowych rodzajów umiejętności (`Warcry`, `Meteor`, `Storm`, `BloodPact`, `FrostArmor`, `Pull`, `Counter`, `Rupture`), `DelayedStrike`/`StormEffect` | EditMode 100/100 |
| 2026-09-29 | B1–B6: 8 broni, 9 przedmiotów, 7 czarów, 5 technik, 3 warianty wrogów żywiołów (losowane na piętrach, 40%), pule i 13 odblokowań; 45 nowych assetów przez `SyncContent` | PlayMode 32/32 (w tym roztrzaskanie, kontra, meteor, burza, łańcuchy) |
| 2026-09-29 | Wygląd wariantów: aura żywiołu, poświata broni i odcień; wróg bez celu (podgląd) też odświeża wygląd | zrzut `fx_sets.png` obejrzany – warianty rozpoznawalne |
| 2026-09-29 | C1–C2: `LevelFeature` (dodatkowe cele, promień, czas, warstwy efektu, płonąca ziemia po wybuchu, końcowe cięcie młynka) dla 18 umiejętności; opis pokazuje cechy (✓ / od +N), nagroda-ulepszenie mówi „nowa cecha” | EditMode 102/102, PlayMode 30/30 (łańcuch +4 → 6 celów, kula +4 → strefa) |
| 2026-09-29 | D1–D2: `SoulShop` (czysta logika, zakupy atomowe), sklep obok panelu między piętrami, przerzucenie nagród na ekranie nagród; ceny ×(1 + 0,15 × (piętra − 1)) | EditMode 105/105, PlayMode 33/33; zrzuty `1f_reward_ui`, `2a_intermission_shop_ui` obejrzane |
| 2026-09-29 | Poprawka: panel sklepu rysowany po panelu głównym – inaczej przejmował pierwszy fokus pada (wykrył to `FullFlow_WithGamepadOnly`) | – |
| 2026-09-29 | G1–G4: cztery cechy (Siła, Zręczność, Inteligencja, Wytrzymałość); `Vigor`→`Toughness`, Kondycja/Umysł wycofane i przeliczane ze starych assetów; uchylenie (od 13 Zr., limit 25%), dłuższy unik i szybszy atak (limity), wolniejsza broń bez wymagań, twarde wymagania czarów; punkt cechy po każdym piętrze (panel między piętrami, obsługa padem) | EditMode 113/113, PlayMode 30/30 (`FullFlow_WithGamepadOnly` rozszerzony o wydanie punktu) |
| 2026-09-29 | Poprawki po zrzutach: za długi opis Zręczności nachodził na przycisk; sklep nie proponuje już czarów, których postać nie może rzucić | EditMode 114/114 |
| 2026-09-29 | E1: ekran „Sterowanie” (menu i pauza), interaktywne przypisanie osobno dla klawiatury/myszy i pada, zamiana przy kolizji, „Przywróć domyślne”, zapis w profilu (`bindingOverrides`), podpowiedzi w HUD i pomocy z bieżącego układu, polskie nazwy przycisków | PlayMode 34/34 (`InputRebindPlayTests`: zamiana, zapis/odczyt, nowy klawisz działa, przypisanie wirtualnym padem) |
| 2026-09-29 | Poprawka: wbudowany zapis Input System dopasowuje po losowych id bindingów (mapa tworzona w kodzie) – układ nie wróciłby po restarcie; własny zapis po nazwie akcji i urządzeniu | wykryte testem |
| 2026-09-29 | F1: modele buławy, młota, włóczni i kosy (`GearBuilder`), dwuręczne pozy; nowe bronie Włócznia (zasięg 3,2–3,6 m, pchnięcia przez linię) i Kosa (szeroki łuk, krwawienie, leczy na krwawiących – efekt `BleedingHitHeal`) | PlayMode 38/38 (`AllWeaponModels_Build_WithTipAwayFromHand`), zrzut `pose_weapons.png` obejrzany |
| 2026-09-29 | Poprawki po zrzutach: pióra buławy prześwietlały się w białą „miotełkę” (ciemniejszy metal, węższe); segmenty ostrza kosy obrócone w złą stronę (rozsypany łuk) | zrzut po poprawce – czytelne |
| 2026-09-29 | F2: technika „Ciężki rzut” (`ThrownWeapon`): broń leci i wraca, trafia w obie strony, w tym czasie pięści, brak bloku bronią i technik | PlayMode 39/39 (`HeavyThrow_HitsOutAndBack_FistsMeanwhile`) |
