# Efekty cząsteczkowe Hell Unleashed w Turris

2026-10-22. Hell Unleashed jest źródłem **tylko do odczytu**. Wszystkie kopie, kod adapterów i dokumentacja
znajdują się w Turris Damnatorum. Nie uruchamiano generatorów ani edytora w projekcie HU.

## Zasoby i adapter

- 38 oryginalnych PNG z `Hell Unleashed/src/Game/Assets/Textures/Vfx` skopiowano do
  `Assets/TurrisDamnatorum/Resources/HellVfx`. Nie kopiować plików `.import` Godota.
- `migration_manifest.json` zawiera SHA-256 tekstur i źródłowego `ElementFx.cs`;
  `generated_manifest.json` zachowuje informację o pochodzeniu części grafik.
- Powtarzalny import: `python tools/art/import_hu_vfx.py --source "C:/GIT/Hell Unleashed"`.
  Skrypt czyta HU i zapisuje tylko ścieżki pod swoim repozytorium docelowym; zachowuje istniejące GUID-y.
- `HellParticleRecipes.cs` zawiera przepisane automatycznie receptury `ElementFx`: ogień, woda,
  powietrze, ziemia, atak wroga, dusza, popiół, lód i efekt fizyczny; osobno ogień w dłoni.
- `HellParticles` tłumaczy warstwy na `ParticleSystem`: gradient, ruch, grawitację, rozmiar,
  obrót, turbulencję, tłumienie, przestrzeń symulacji i kształt emisji.
- `Amount` HU to liczba jednocześnie żywych cząsteczek; dla emisji ciągłej adapter liczy
  `rateOverTime = Amount / Lifetime`. Wybuch używa bursta. Plansze płomieni mają **8×8 klatek**;
  atlas 4×4/4×2 losuje jeden nieruchomy obraz na cząsteczkę.
- Materiały i tekstury są współdzielone. Shader cząsteczek oraz shader ziemi są w `Resources`,
  więc nie zależą od obecności materiału w otwartej scenie przy tworzeniu buildu.
- `HellFxPool`: maksymalnie 6 wybuchów na rodzinę i 6 krótkich świateł; najstarszy wybuch
  jest czyszczony przed ponownym użyciem. Pozostałe emitery mają właściciela i kończą życie po emisji.

Warstwy trwające używają czasu gry: pauza zatrzymuje symulację i wygaszanie.
Po końcu lotu/wyłączeniu statusu smuga odczepia się od obiektu, przestaje emitować i dogasa.
VFX nie naliczają obrażeń i nie zmieniają parametrów umiejętności.

## Integracja z istniejącą grą

| Miejsce | Przeniesiona oprawa |
|---|---|
| Pociski i różdżki | Jądro/animowana plansza, smuga żywiołu, odpadanie iskier, dogasanie przy trafieniu, wygaśnięciu i zniszczeniu obiektu |
| Strzała żywiołów | Model strzały pozostaje, dochodzi smuga ognia; wybuch i płonąca strefa dziedziczą rodzinę VFX |
| Trafienia, blok, śmierć | Tekstury iskier, krwi, pyłu; warstwy popiołu i żaru HU |
| Statusy | Ogień na sylwetce, mgła/szron, elektryczne iskry, pył/odłamki przy pęknięciu pancerza |
| Strefy i aury | Ciągłe warstwy żywiołu na rzeczywistym obszarze; koniec emisji zgodny z czasem działania |
| Nova i meteor | Wybuch żywiołu, brzeg strefy, wypełniana zapowiedź; pył przy uderzeniu w ziemię |
| Fala | Pas emisji o szerokości skilla, cząsteczki zostają w świecie i dogasają na końcu fali |
| Unik w cień | Ciemny fioletowy dym i iskry, krótka smuga podczas zniknięcia |
| Odskok widma | Iskry zbiegające się przy zniknięciu i rozprysk przy powrocie, jak `BlinkFx.PlaySparks` HU |
| Portale i ogniska | Plansze płomienia, żaru i dymu; dotychczasowy owal i wir portalu Unity pozostają |
| Demony z modelami HU | Ogień dłoni/ogona chochlika, dłoń kultysty, warianty chochlika i żar Władcy Bramy |
| Sługi | Aura duszy, pozostała mechanika i proceduralne modele sług Turris |

## Audyt skopiowanych umiejętności

Sprawdzono **29 assetów `Content/Spells/hu_*.asset`**, fabrykę `DefaultContent.Heroes`,
`PlayerCombat.FireSpell`, `FireHeroSpell`, `NovaPulse`, dostawy pocisków/fal/stref i czas życia aur.

`HellSkillFx` wybiera rodzinę z danych/id skilla, a nie koloru. `HitData.visualElement` przenosi ją
do pocisku, odłamków, wybuchu i strefy; pole nie jest używane przez resolver obrażeń.
Mechanika Woda → Mróz i Powietrze → Błyskawica zostaje. Wodne skille HU zachowują wodne cząsteczki,
a własne lodowe skille Turris korzystają z odłamków lodu i szronu.

| Skill (id) | Dostawa i użycie efektów |
|---|---|
| `hu_warcry` | Fizyczny impuls/pył przy okrzyku; kolor UI nie zamienia go w ogień |
| `hu_undying` | Fizyczny impuls; czerwona aura trwa `duration` i kończy emisję, zamiast pozostać na bohaterze |
| `hu_blade_dance` | Smugi broni podczas młynka, fizyczne zakończenie o `finalRadius` |
| `hu_judgement` | Ognisty impuls po zapowiedzi, zasięg zgodny z novą |
| `hu_overload` | Błysk/impuls duszy; wzmocnienie kolejnego skilla nadal liczy istniejąca mechanika |
| `hu_meteor` | Zapowiedź w punkcie celu → jeden wybuch ognia → strefa po uderzeniu |
| `hu_maelstrom` | Trzy wodne impulsy przy bohaterze; w mechanice nadal chłód i przyciąganie |
| `hu_tempest` | Strefa powietrza i elektrycznych iskier; pioruny przez czas strefy |
| `hu_quake` | Impuls ziemi i pył na promieniu skilla |
| `hu_legion` | Impuls duszy przy przyzwaniu, aury sług; ataki sług korzystają z dostaw pocisków/trafień |
| `hu_storm_cleave` | Stożek powietrza, bez lodowych cząsteczek |
| `hu_blade_fan` | Fizyczne ostrza dookoła, smugi pocisków i krew dopiero przy trafieniach |
| `hu_arrow_storm` | Wachlarz strzał, fizyczne smugi i trafienia; przebicie nie tworzy strefy mrozu |
| `hu_death_mark` | Pył przy starcie doskoku, fizyczny impuls i krew na końcu |
| `hu_elemental_arrow` | Strzała ze smugą ognia → pojedynczy wybuch → płonąca strefa |
| `hu_cleave` | Fizyczny stożek, bez efektu FrostCone |
| `hu_wind_slash` | Pas powietrza o szerokości fali, dogasanie po końcu przelotu |
| `hu_aura_holy_fire` | Ogniste warstwy na dysku, impuls po trafiającym tyknięciu, gaszenie przy wyłączeniu |
| `hu_aura_defiance` | Aura ziemi jako wizualizacja obrony, bez zadawania dodatkowych obrażeń |
| `hu_piercing_shot` | Fizyczna strzała, krótkie trafienia po kolejnych przebiciach |
| `hu_splinter_arrow` | Fizyczna strzała i dwie drzazgi; każda dziedziczy rodzinę VFX oryginału |
| `hu_cross_cut` | Fizyczny stożek zamiast lodowego podmuchu |
| `hu_snare` | Fizyczna strefa o czasie działania skilla; efekty spowolnienia/krwawienia bez zmian |
| `hu_nova_fire` | Zapowiedź i ognisty impuls, podążanie za bohaterem według danych |
| `hu_nova_water` | Zapowiedź i wodny impuls; nie mylić z własną Mroźną falą Turris |
| `hu_nova_air` | Zapowiedź i impuls powietrza |
| `hu_nova_earth` | Zapowiedź i impuls ziemi |
| `hu_summon_imps` | Ogień przy przyzwaniu; pociski chochlików używają oprawy ognia |
| `hu_knife_throw` | Trzy fizyczne noże, smugi lotu i efekt trafienia |

Naprawione podczas audytu:

1. Wspólne wywołanie `FrostCone` przy wszystkich stożkach zastąpiono wyborem rodziny skilla.
2. Wybuchy obszarowe pocisków i meteoru miały dodatkowy generyczny `Impact` nad `Shockwave`;
   oprawa wybuchu powstaje teraz raz, w rzeczywistym promieniu obrażeń.
3. Rodzina efektów jest jawnie przekazywana do stref po wybuchu i pocisków rozszczepionych.
4. Aura nieśmiertelności ma czas życia; anulowanie przygotowania gasi wszystkie jego warstwy.
5. Test Strzału przeszywającego w oknie ujawnił pomijanie kolejnych celów przy długiej klatce:
   pocisk po pierwszym przebiciu przeskakiwał na koniec kroku. `Projectile.Step` przetwarza teraz
   trafienia całego odcinka w kolejności odległości, a regresja sprawdza dwa cele w jednym kroku 0,4 s.

## Zakres i świadome różnice

To port tekstur, receptur i użycia cząsteczek do natywnego ParticleSystem, nie binarne kopiowanie Godota.
Turbulencja, opór i orientacja rozciągniętego billboardu mają odpowiedniki Unity, więc obraz nie jest
identyczny piksel w piksel. Efekty trzeba oceniać pod kamerą Turris.

Nie przeniesiono w tej pracy całego renderera `SpellFx` Godota, jego edytora stylów ani wszystkich
shaderów brył wody/wiatru, lodowych płyt, kolców, obrysów/przemoczenia sylwetek i shaderowego wnętrza portalu.
Tekstury są dostępne, podstawowe warstwy są używane w grze. Te siatki i materiały są ujęte w etapach 2 i 7
[planu dalszej migracji](PLAN_MIGRACJI_HELL_UNLEASHED.md).

Emisja 2D w tle sklepu HU (`ShopMood`) zostaje przy migracji UI. Turris obecnie ma sklep IMGUI.
Nie zmieniono go w tej pracy w ekran Godota ani nie zastąpiono istniejącego systemu audio.

## Weryfikacja

- `HellFxPlayTests`: atlasy i flipbooki, dostępność tekstur/shaderów, ograniczona pula i czyszczenie
  pozycji poprzedniego wybuchu, współdzielenie materiałów, pauza, koniec strefy i dogasanie smugi.
- `HeroSkillPlayTests`: regresja stożków fizycznych/powietrza i wodnego Wiru głębin, obok dotychczasowych
  testów obrażeń fali, przyzwania, strzał, doskoku i aury.
- Galeria: `TURRIS_SHOT_DIR=... python tools/run_tests.py PlayMode gfx Turris.Tests.HellFxPlayTests`.
  Zapisuje ogień, wodę, powietrze, ziemię, lód i krew/cień. Wyniki ostatniej sesji: `PROGRESS.md`.
  W obrazach żywiołów lewa strona pokazuje strefę, środek emiter smugi, prawa strona wybuch.
