# Raport: cztery perki specjalizacyjne

Specyfikacja: [PROMPT_WDROZENIE_PERKOW_SPECJALIZACYJNYCH.md](PROMPT_WDROZENIE_PERKOW_SPECJALIZACYJNYCH.md). Stan na 2026-11-01.

## 1. Stan

| Perk | Zaimplementowane | Przetestowane (E/P) | Zbalansowane |
|---|---|---|---|
| Pojedynkowicz `perk.turris.duelist` | tak | E04, E05, P02 | nie – hipotezy startowe |
| Wykorzystaj wyłom `perk.turris.exploit_breach` | tak (cios, parowanie, riposta, W03) | P04, P05, P06 | nie |
| Różnorodna sfora `perk.turris.diverse_pack` | tak | E09, E10, P07 | nie |
| Gwałtowna manifestacja `perk.turris.violent_manifestation` | tak (Płonąca ziemia, strefy HU, Meteor) | E11, E12, P09 | nie |

Wspólne: E01–E03, E14, P01 (prawdziwy `GameConfig` ze sceny Main, bez regeneracji).

## 2. Katalog i ceny

| Id | Poziomy | Działanie | Baza | Cena (PriceScale 2,5, inflacja 1) | Waga | Od piętra |
|---|---:|---|---:|---:|---:|---:|
| `perk.turris.duelist` | 1 | +0,15 do sumy tempa kwalifikowanego ataku | 28 | 70 | 0,70 | 1 |
| `perk.turris.exploit_breach` | 1 | ×1,25 bezpośredniego trafienia, okno 3 s | 30 | 75 | 0,80 | 3 |
| `perk.turris.diverse_pack` | 1 | +10% za inny rodzaj (maks. +20%) | 32 | 80 | 0,65 | 1 |
| `perk.turris.violent_manifestation` | 1 | czas i odstępy tyknięć ×0,5 | 30 | 75 | 0,65 | 1 |

Jedno źródło liczb: `GameConfig.nativePerks` (`NativePerkTuning`, walidacja NaN/zakresów w `Valid()`); stary asset bez pola dostaje wartości domyślne.

## 3. Mapa kodu

- `Run/NativePerks.cs` – strojenie, katalog `NativePerkCatalog` (definicje `BoonDefinition` w pamięci per konfiguracja, idempotentnie), kwalifikacja (`Usable`/`UsableWith`), ceny, teksty.
- `Run/FloorShop.cs` – natywne perki we wspólnej puli czterech ofert (bez kary obcej klasy, od swojego piętra, tylko gdy build je wykorzysta); ponowna kwalifikacja przy zakupie.
- `Run/PerkMechanics.cs` – aktywność natywnego perka = jego kwalifikacja (uśpiony zostaje kupiony).
- `Run/RunSnapshot.cs` – `Index` zna natywne perki (zapis/odczyt bez wcześniejszego sklepu).
- `Player/PlayerCombat.Perks.cs` – Pojedynkowicz, okna Wyłomu, mnożnik Sfory, skala czasu stref, telemetria, HUD.
- Wpięcia: `PlayerCombat.StartAttack` (tempo), `PerformRiposte` (nowe pochodzenie + wyłom), `OnHitResolved` (zużycie), `PlayerCombat.Hero.OutgoingAgainst` (premia przed obroną), `EnemyBrain` (zdarzenie `PoiseBroke` przy rzeczywistym przełamaniu i parowaniu), `HitQuery.Apply` (jawny obrońca parowania `EnemyBrain.ParryDefender`), `Servant.Attack` (warstwa sfory i `servantHit`), `DamageZone` (lokalny zegar), `HuCast.zoneTimeScale` + `HuSkillCaster.SpawnZone`, `FireSpell` (Zone, Meteor), `DelayedStrike`, `HitData.servantHit/copyHit`.
- UI: karta sklepu (korzyść, warunek, koszt alternatywny, powód uśpienia), ostrzeżenie „Po założeniu przestaną działać” także dla natywnych perków – broń i przedmioty do drugiej ręki (`LoadoutAnalysis.NativePerksLosingEffect`), linia HUD (rodzaje sfory i premia, liczba okien wyłomu), napis „WYŁOM” nad celem.

## 4. Zasady interakcji

- **Pojedynkowicz:** jednoręczna broń wręcz w głównej ręce (bez pięści, łuków, różdżek, kosturów, noży bliźniaczych/pazurów) i pusta druga ręka. Dodatek do sumy `Build.attackSpeed + rozpęd` tylko dla zwykłego lekkiego/mocnego ataku wręcz główną ręką; nie wpływa na statystykę szybkości, techniki, czary, parowanie, unik, ripostę ani Wcielenie (to przechwytuje atak wcześniej). Broń w locie (pięści) – bez premii.
- **Wyłom:** okno tworzy jedno zdarzenie przy rzeczywistym przełamaniu (`Staggered` po wyczerpaniu postawy bez hiperzbroi albo `PoiseBroken` po parowaniu). Kwalifikują się bezpośrednie trafienia ręcznej akcji; słudzy (`servantHit`), kopie/wyzwalacze (`copyHit`), strefy, statusy, reakcje i efekty cech/unikatów – nie. Premia ×1,25 do fizycznej i magicznej części (podział `MagicSplit` jest proporcjonalny), bez postawy i statusów, w nowej akcji (inne `originId`), raz na cel, zużycie po obrażeniach życia. Riposta dostaje nowe pochodzenie; Hełm dzwonnika – premia tylko na głównym celu fali; Obrączka parującej runy – czar trafia zwykłą ścieżką.
- **Sfora:** K – maska rodzajów własnych sług (chimera: działające wkłady, śmiertelna – wszystkie wkłady). Warstwa `1 + 0,1·max(0,K−1)` w `Servant.Attack` w chwili ataku, obok `ServantDamageMult`; nie w migawce (`makeHit`), nie w `Deliver` (Wcielenie, Maska trzeciego głodu), nie w kokonach.
- **Manifestacja:** ręczny ofensywny czar (`IsSpell`, bez aur i wyzwalaczy); kopie dziedziczą profil z chwili tworzenia rzutu. Budżet tyknięcia liczony z bazowego `share × tick`, potem kompresja przez lokalny zegar strefy. Prąd głębin/Płaszcz wędrującej rzeki przenoszą profil bez ponownej kompresji. Techniki (Sidła, Deszcz strzał…) bez zmian.

Tabela stref: Płonąca ziemia – tak; Krąg żaru – tak; strefa novy ognia III – tak (sama strefa, fala bez zmian); strefa Meteoru – tak (uderzenie i opóźnienie bez zmian); `hu_arrow_rain`, `hu_snare`, `hu_thorn_scatter`, `hu_slag_spray` – nie (techniki); aury, przywołania – nie. Skille sygnatur nie wróciły do puli.

## 4a. Boty

`Tests/PlayMode/Bots/HeroBot.Perks.cs`:
- **Sklep:** `NativePerkScore` – uśpiony albo już posiadany perk 0; Pojedynkowicz 3; Wyłom 2,8 (mag 2); Sfora 3 tylko przy co najmniej dwóch rodzajach sług w slotach (inaczej 1 – nie kupuje); Manifestacja 2,8. Próg zakupu bez zmian (2,5). Tester kupuje losowo jak dotąd.
- **Wyposażenie:** działający natywny perk ma w `GearScore` wartość 30 (unikat 25), a `GearWouldHelp` porównuje uczciwie także pusty slot, gdy przedmiot uśpiłby perk – rycerz z Pojedynkowiczem nie zakłada tarczy do pustej ręki (ocena 233,6 bez tarczy).
- **Walka:** cel z otwartym wyłomem w promieniu 6 m ma pierwszeństwo i dostaje mocny cios (premia raz – na największy cios); przy Sforze bot wstrzymuje przywołanie rodzaju, który już stoi, gdy inny gotowy slot przywołuje brakujący.
- **Raport serii:** sekcja „Perki specjalizacyjne” (zakupy, `PerkTelemetry` z rekordu `perkStats`).
- Test: `Turris.Tests.Bots.NativePerkBotTests` 1/1. Krótki przebieg (po 1 podejściu, Wojownik i Mag × Tester i Gracz, `TestResults/perki-specjalizacyjne/boty-seria`): 4/4 bez wyjątków; gracz – Wojownik piętro 8, Mag 15; tester – Wojownik 6, Mag 30 (zwycięstwo). Żaden bot nie kupił natywnego perka (rycerz startuje z tarczą, więc Pojedynkowicz śpi; Wyłomu nie było w ofercie albo przegrał z innymi zakupami) – na działanie perków u botów potrzeba dłuższej serii.

## 5. Poprawka regresji (oddzielnie od perków)

`DamageZone.Step` sumował kroki czasu i przy kroku 0,01 s dokładał dziewiąte tyknięcie na końcu strefy 4 s/0,5 s (dryf zmiennoprzecinkowy – liczba trafień zależała od FPS). Teraz chwile tyknięć to `k × interwał` (pierwsze w 0, koniec wykluczony). Bez perka strefy działają jak w zamierzeniu, ale przy wysokim FPS mogą mieć jedno tyknięcie mniej niż dotąd.

## 6. Wyniki testów

- EditMode `NativePerkTests` 8/8 (E01–E06, E09–E12, E14); pełny EditMode 315/318 (2 znane błędy z „AnimeForward”: `GothicArenaTests.Materials_CompileAndSkyTextureIsPresent`, `PaladinAnimeTests.Avatar_MapsToesAndSpine_AndArmorIsMatte`; 1 pominięty eksport WAV).
- PlayMode `NativePerkPlayTests` 6/6 (P01, P02, P04, P05, P06+P07, P09).
- Pomiary (gfx, seed testów): zamach + faza aktywna miecza 0,380 → 0,335 s, wytrzymałość na cios 16,0 bez zmian; atak chochlika 10,00 → 12,00 przy trzech rodzajach; Krąg żaru 10 tyknięć w 4,97 s → 10 w 2,50 s, Sidła 8/4,00 s bez zmian; sklep na prawdziwym GameConfig dla Wojownika bez drugiej ręki oferował Pojedynkowicza i Wyłom, bez Sfory.
- Regresje PlayMode (gfx): ClassFeature, Unique, UniqueScenario, UniqueSmoke, HeroSkill, GameFlow – 154/155; jedyny błąd `Bellringer_RiposteBecomesWave_HitsSecondEnemyOnce` („parowanie przełamało postawę” – test zależny od wyczucia parowania) przeszedł przy ponownym uruchomieniu. Drugi przebieg: UniqueScenario, NativePerk, HuSkillCaster, SealFlow, AffixSpecialization – 24/25, 1 pominięta galeria na żądanie. Wyniki: `TestResults/perki-specjalizacyjne/regresje*`, `edit2`.

## 7. Nierozwiązane / nie zrobiono

- Brak zrzutów UI sklepu/HUD (tryb window) i ciągu klatek tempa/stref; brak testów E07–E08, E13, E15–E16 i P03, P08, P10 jako osobnych przypadków (część pokrywają testy E04/E05/P04–P07).
- Brak porównań balansu na seedach (rozdz. 12) i alternatywnego zakupu; liczby to hipotezy.
- Zmiana kolejności losowania sklepu: gdy w puli są natywne perki, sekwencja losowań HU dla tego samego seeda jest inna niż wcześniej.
- Przy Manifestacji czas statusów i reakcji nie jest kompresowany (zgodnie ze specyfikacją) – szybsze nakładanie zmienia wynik po statusach; nie zmierzono.
- Test W03 (Hełm dzwonnika) bywa niestabilny przez wyczucie parowania – kandydat do deterministycznego ustawienia.
