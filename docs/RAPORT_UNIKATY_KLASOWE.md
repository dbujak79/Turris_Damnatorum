# Raport wdrożenia 75 unikatów klasowych

Specyfikacja: [PROMPT_WDROZENIE_75_UNIKATOW_KLASOWYCH.md](PROMPT_WDROZENIE_75_UNIKATOW_KLASOWYCH.md). Dokument roboczy – aktualizowany w trakcie prac.

## 1. Audyt wyjściowy (2026-11-01)

- **30 cech klasowych istnieje w kodzie** (`Scripts/Runtime/ClassFeatures/`, raport [RAPORT_CECHY_KLASOWE.md](RAPORT_CECHY_KLASOWE.md)); kontrolery mają haki potoku walki w `PlayerCombat`. Unikaty korzystają z tych kontrolerów, niczego nie przyznają.
- Łup: `GameRoot.ItemDrop` (4% zwykły, 60% elita, 100% boss) → `LootPickup` → plecak. Unikat rozpoznawał wyłącznie `HellEconomy.IsUnique` (pochodzenie HU). `FloorShop` wyklucza unikaty.
- Treść: assety `Content/` (GameConfig) albo `DefaultContent` w testach. **Nie użyto `RunBatchForce`** – unikaty Turris powstają w kodzie (rejestr runtime) na bazie istniejących broni z puli, bez zmian w assetach.
- Zapis: `RunSnapshot` (id definicji przez `Index`), profil bez przedmiotów.

## 2. Architektura

- `Scripts/Runtime/Uniques/UniqueCatalog.cs` – 75 kart (id, kod W01…, klasa, slot, baza broni, warunek, statystyki, parametry mocy, teksty, łup).
- `UniqueRegistry` – buduje `ItemDefinition` (ScriptableObject w pamięci) na konfigurację; `nativeUnique = true`, `uniquePowerId`. Nie trafiają do `baseItemPool` (sklep/nagrody ich nie widzą), ale do `RunSnapshot.Index` i puli unikatów łupu.
- `UniquePower` (kontroler mocy na właściciela i definicję) + `UniquePowerSet` w `PlayerCombat` – haki potoku jak u cech. Duplikat definicji (dwa pierścienie) = jedna moc.
- Jakość/poziom skaluje wyłącznie statystyki; parametry mocy są strukturalne.

## 3. Tabela postępu

Legenda: D – definicja, R – runtime, L – łup, U – opis/UI, Z – zapis, B – legalny build, K – test kontraktu, S – test silnika, O – oprawa, P – dowód (obraz).

Stan roboczy 2026-11-01 (etap kodu): **wszystkie 75 kontrolerów mocy napisane i kompilują się** (`Uniques/Powers.*.cs`, fabryka 75/75).
Dalej: UI/HUD, testy kontraktowe i w silniku, boty, zrzuty.

Pliki: `Uniques/UniqueCard.cs`, `UniqueCatalog*.cs` (75 kart), `UniqueRegistry.cs`, `UniqueLoot.cs`, `UniquePower.cs`, `UniquePowerFactory.cs`,
`UniqueGeo.cs`, `UniqueMines.cs`, `Powers.Warrior/Paladin/Berserker/Dual/Mage/Elements/Summoner/Rogue.cs`, `Player/PlayerCombat.Uniques.cs`,
`Run/RunState.Uniques.cs`. Ikony: `Resources/UniqueIcons/<kod>.png` (128 px z ArtSource/Unikaty/ProsteIkony_v1).

Zmiany w istniejącym kodzie (haki, bez zmiany zachowania bez unikatu): punkty `UniquesFeatureEvent` w cechach (absolution.release,
judgement.split, fury.tokens, dance.carried, pair.wand, cloak.paid, furnace.attack, frost.press, current.glide, eye.release,
bastion.press, shell.plate, incarnation.heavy, chimera.spawned, arcana.fists, witness.press, trick.execute, nails.rip, post.shot,
spellarrow.release); Sąd przysięgi jako lista powiązań aura→cel; Servant: śmiertelność, kokony, komendy, dziedziczenie, zdarzenie
naturalnego końca; StatusEffects: tempo ran, odroczone podpalenie, transfer krwawienia/podpaleń, magiczne pęknięcie; HuShot/Projectile:
rejestr, przekierowanie, zdarzenia końca lotu; HuMine: przesuwanie, wstrzymanie kontaktu; EarthWall: płyty prowadzone; FeatureDecoy:
wabik doczepiony do celu; PinnedShadow: podążanie; ThrownWeapon: zakotwiczenie; EquipmentSet: lokalny wyjątek slotu (różdżka w lewej).


### Tabela (2026-11-01)

D – definicja, R – runtime, L – łup, U – opis/UI, Z – zapis. „Dym 75” – `UniqueSmokePlayTests` (legalny rdzeń, walka z AI, brak wyjątków, moc nieuśpiona).
„Moc w dymie” – zdarzenie mocy w telemetrii podczas ogólnego scenariusza. „Test ukierunkowany” – EditMode/PlayMode sprawdzający rozstrzygający przypadek.
Brak: B (rozwinięty build w grze botów), O/P (oprawa i obraz) poza 8 mocami z galerii (M03, E01, R03, B02, F05, K04, R05, B04).

| Kod | Id | Nazwa | D | R | L | U | Z | Dym 75 | Moc w dymie | Test ukierunkowany |
|---|---|---|---|---|---|---|---|---|---|---|
| W01 | warrior.last_pavise | Pawęż ostatniego zakonnika | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| W02 | warrior.two_oaths | Pas dwóch przysiąg | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| W03 | warrior.bellringer | Hełm dzwonnika | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| W04 | warrior.left_watch | Karwasze pozostawionej warty | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| W05 | warrior.empty_front | Medalion pustego frontu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| P01 | paladin.hungry_reliquary | Relikwiarz głodnego świętego | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| P02 | paladin.divided_litany | Mitra rozdwojonej litanii | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| P03 | paladin.empty_monastery | Habit pustego klasztoru | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| P04 | paladin.penitent_bells | Karwasze dzwonów pokutnych | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| P05 | paladin.long_penance | Pastorał długiej pokuty | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| B01 | berserker.hungry_scars | Kolczuga głodnych blizn | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| B02 | berserker.angry_spirits | Różaniec gniewnych duchów | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| B03 | berserker.walking_slaughter | Topór kroczącej rzezi | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| B04 | berserker.chained_butcher | Karwasze przykutego rzeźnika | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| B05 | berserker.held_rage | Pierścień nierozładowanego szału | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| R01 | dual.double_secret | Rękawice podwójnego sekretu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| R02 | dual.unfinished_step | Buty niedokończonego kroku | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| R03 | dual.blade_juggler | Pas żonglera ostrzami | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| R04 | dual.reverse_wounds | Szabla wspak pisanych ran | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| R05 | dual.wandering_blade | Amulet zbłąkanej klingi | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| M01 | mage.moneylenders_note | Weksel lichwiarza | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| M02 | mage.sealed_source | Szata zamkniętego źródła | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| M03 | mage.sleeping_circles | Grymuar uśpionych kręgów | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| M04 | mage.bound_spells | Rękawice związanych zaklęć | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| M05 | mage.late_presence | Buty spóźnionej obecności | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| F01 | fire.pyre_devourer | Miech pożeracza stosów | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| F02 | fire.ash_puppets | Mankiety popielnych marionetek | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| F03 | fire.borrowed_sun | Młot pożyczonego słońca | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| F04 | fire.deferred_wildfire | Szata odroczonego pogorzeliska | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| F05 | fire.sky_furnace | Diadem pieca pod niebem | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| H01 | water.drowned_king | Korona zatopionego króla | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| H02 | water.wandering_river | Płaszcz wędrującej rzeki | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| H03 | water.silent_glacier | Grymuar niemego lodowca | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| H04 | water.eternal_ebb | Obrączka wiecznego odpływu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| H05 | water.salt_mirror | Różdżka słonego lustra | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| A01 | air.last_breath | Flakon ostatniego oddechu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| A02 | air.storm_smith | Rękawice burzowego kowala | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| A03 | air.windless_night | Peleryna bezwietrznej nocy | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| A04 | air.empty_nodes | Różdżka pustych węzłów | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| A05 | air.last_gust | Buty ostatniego podmuchu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| E01 | earth.moving_bastion | Buty ruchomego bastionu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| E02 | earth.grave_mortar | Pas grobowego moździerza | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| E03 | earth.undug_grave | Młot niewykopanego grobu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| E04 | earth.living_quarry | Napierśnik żywego kamieniołomu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| E05 | earth.wound_underfoot | Amulet rany pod stopami | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| S01 | summoner.third_hunger | Maska trzeciego głodu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| S02 | summoner.sixth_rib | Serce szóstego żebra | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| S03 | summoner.bone_conductor | Rękawice kościanego dyrygenta | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | ✓ |
| S04 | summoner.open_maws | Grymuar otwartych pysków | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| S05 | summoner.inherited_beast | Pas dziedziczonej bestii | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| T01 | battlemage.runic_wrestler | Bandaże runicznego zapaśnika | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| T02 | battlemage.unfinished_rune | Miecz niedokończonej runy | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| T03 | battlemage.rune_eater | Zbroja runożercy | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| T04 | battlemage.parrying_rune | Obrączka parującej runy | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| T05 | battlemage.broken_wand | Grymuar złamanej różdżki | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| L01 | rogue.false_king | Maska fałszywego króla | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| L02 | rogue.professional_cheat | Rękawice zawodowego oszusta | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| L03 | rogue.duds | Pas niewypałów | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| L04 | rogue.unfinished_escape | Sztylet niedokończonej ucieczki | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| L05 | rogue.abyss_signal | Medalion sygnału z otchłani | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| K01 | archer.bowstring_weaver | Rękawice tkacza cięciw | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| K02 | archer.exiled_shooter | Buty wygnanego strzelca | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| K03 | archer.gravediggers_bow | Łuk grabarza | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| K04 | archer.unfinished_volley | Pas niedokończonej salwy | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| K05 | archer.reversed_sight | Kaptur odwróconego celownika | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| Z01 | assassin.own_funeral | Całun własnego pogrzebu | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| Z02 | assassin.deferred_verdict | Pierścień zwleczonego wyroku | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| Z03 | assassin.grave_needle | Igła grobowej szwaczki | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Z04 | assassin.silent_witness | Maska milczącego świadka | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| Z05 | assassin.crimson_knot | Rękawice karminowego węzła | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| Q01 | arcane_archer.seventh_seal | Łuk siódmej pieczęci | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| Q02 | arcane_archer.stolen_elements | Kołczan skradzionych żywiołów | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| Q03 | arcane_archer.heretic_choir | Łuk heretyckiego chóru | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |
| Q04 | arcane_archer.unfinished_incantation | Amulet niedokończonej inkantacji | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – |
| Q05 | arcane_archer.between_worlds | Rękawice kołczanu między światami | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | – | – |

## 4. Wyniki testów

- EditMode `UniqueItemTests` 14/14; pełny EditMode 289/292 (2 znane „AnimeForward”, 1 pominięty eksport WAV).
- PlayMode (gfx): `UniquePlayTests` 12/12, `UniqueScenarioPlayTests` 7/7, `UniqueSmokePlayTests` 75/75; window: `UniqueGalleryTests` 2/2.
- Symulacja łupu (seed 11, 4000 losowań bossa, Wojownik, piętro 10): własna klasa 32,1%, poszukiwany 16,2% (bez wskazania 4,4%).
- Zrzuty obejrzane: `TestResults/unique-items/shots/unique-items/` (8 mocy w świecie, HUD, ekwipunek, katalog). Wykryty i naprawiony wyciek bytów mocy po zniszczeniu postaci; zakotwiczona broń wysunięta z ciała ofiary.
- Błędy wykryte testami w silniku: nieosiągalny build P05 (buława jednoręczna nie może Rozpłatania → Odtrącenie `hu_repel`).

## 5. Decyzje i ograniczenia (do wiadomości autora)

- Jakość/poziom skaluje tylko statystyki; parametry mocy strukturalne. Dwie kopie: statystyki obu, jedna legenda (pierwszy slot).
- M02: runy wzmacniają tę samą novę (moc rzutu, więc i promień) zamiast osobnego wyładowania – dzięki temu z M03 zapisują się w jednej minie.
- B04 działa tylko z Ciężkim rzutem `skill_throw` (rzut prawdziwą bronią); Rzut toporem HU nie jest bronią w locie.
- P04/P02 wymagają włączonych aur; R01 – wyjątek slotu wyłącznie przy rękawicach (zdjęcie odsyła różdżkę do plecaka).
- S02: śmiertelna chimera przyciąga wrogów wabikiem doczepionym do niej; czar leczenia leczy też chimerę w 8 m (25% jej maks.).
- Liczby są hipotezami startowymi, bez strojenia na seriach botów (wstrzymanie serii z PROGRESS obowiązuje).

## 6. Czego nie zrobiono

- ~~Testy rozstrzygające dla mocy bez dowodu działania~~ – zrobione: `UniqueProofPlayTests` 24/24; wyniki i znalezione wady w [audycie](AUDYT_BUILDCRAFT.md).
- Polityki botów dla nowych pętli (przygotowanie/wyzwolenie); boty tylko oceniają, czy moc działa.
- Pomiary DPS/czasu czyszczenia na seedach, koszt klatki przy 100 aktywacjach, dźwięki mocy, osobne modele 3D unikatowych broni (używają modeli baz).
- Regresje PlayMode cech klasowych po zmianach haków – zablokowane przez równoległą, niedokończoną edycję `DamageResolver.cs` (`MagicSplit`).
