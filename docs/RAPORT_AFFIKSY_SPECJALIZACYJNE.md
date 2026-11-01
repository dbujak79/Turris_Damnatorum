# Raport: affiksy specjalizacyjne i rzeczywiste dodane żywioły (2026-11-01)

Zadanie: [PROMPT_WDROZENIE_AFFIKSOW_POE2.md](PROMPT_WDROZENIE_AFFIKSOW_POE2.md). Katalog: [AFFIKSY_PRZEDMIOTOW.md](AFFIKSY_PRZEDMIOTOW.md), skrót: [AFFIKSY_SKROT.md](AFFIKSY_SKROT.md).

## 1. Zakres wykonania

| Punkt | Stan |
|---|---|
| 4 dodane żywioły jako rzeczywiste składniki (`fire_add`/`water_add`/`air_add`/`earth_add`) | wdrożone |
| 8 nowych grup × 3 tiery (przebicie ×4, postawa, sługi, blok tarczą, rzucanie) | wdrożone, 155 rekordów / 54 grupy |
| Przebicie liczone osobno dla każdego żywiołu | wdrożone (dotyczy też starych perków przebicia) |
| Nakładka danych odporna na ponowny import HU | wdrożona (`tools/data/affixes_turris.json`) |
| Zapis: id i wersja affiksów, stare zapisy bez zmian | wdrożone |
| Opisy, statystyki, analiza buildu z podziałem obrażeń | wdrożone (bez zrzutów UI – patrz rozdz. 8) |
| Testy EditMode E01–E14, PlayMode P01/P06/P07/P10, pomiary 10.3 | wdrożone – wyniki w rozdz. 7 |

## 2. Źródła danych

- **HU (tylko odczyt):** 131 rekordów w 46 grupach, bez zmian treści i kolejności (scalenie dopisuje rekordy na końcu – diff `affixes.json` to wyłącznie +486 linii).
- **Nakładka Turris:** `tools/data/affixes_turris.json` – 24 rekordy `affix.turris.<grupa>.t1..t3`, polskie nazwy, pole `Filter` (wąski warunek przedmiotu) i `Decimals` (precyzja 3 miejsc w jednostkach HU), pusta lista `overrides`.
- **Scalanie:** `tools/art/turris_affix_overlay.py` (walidacja id/grupy/rodzaju/tieru/progu/wagi/kategorii/filtra/statystyki; konflikt id lub grupy z HU przerywa; rekordy `source: turris` zdejmowane przed scaleniem – idempotentne, `--check` porównuje). Importer `import_hu_economy.py` scala nakładkę po odczycie źródła HU. Sprawdzone na kopii już zaimportowanych danych (źródło HU nie było potrzebne): trzy kolejne scalenia → identyczny plik, `--check` „zgodne”.
- **Dokumentacja:** `tools/art/affix_docs.py` generuje obie listy z danych.

## 3. Kontrakty obliczeń

### 3.1. Dodane żywioły
- Statystyki `AddedWeaponFire/Frost/Lightning/Earth` (nowe wartości na końcu `StatType`); affiksy mapuje `HellEconomy.MapAffix` (5% bazy broni za punkt HU – nominalnie +10–70%, jak dotąd). Przedmioty bazowe i perki HU zostają przy `Map` (płaski żywioł nadal → premia fizyczna; zmiana tylko dla losowych affiksów).
- `B` = obrażenia ataku bez premii fizycznej: `dmg / (1 + PhysicalDamage/100)`, gdzie `dmg` to obrażenia tego ataku tej ręki (skalowanie, poziom broni, wymagania, mnożnik ciosu/techniki/wiązki). `dodatek[e] = B × suma[e] / 100` (`AffixMath.AddedElement`).
- Miejsce: tuż po `HitData.FromAttack`, przed ruchem (mnożnik ciosu ładowanego/biegu wchodzi raz) i przed `PrepareOutgoing` (konwersja ogień → mróz i premie żywiołu działają na dodatek raz, premia fizyczna wcale).
- Ścieżki z dodatkiem: zwykły cios (`SweepWeapon`), podstawowy strzał łuku/różdżki, przeniesiony cios (Taniec bez przerwy), atak drugiej broni (Niezgodna para), techniki Turris (`SkillHitData`) i HU (`HuHit`, nie czar). Bez dodatku: czary, sługi, aury, tyknięcia, reakcje, fala ciosu ładowanego, uniki/szarże cech, trafienia wtórne unikatów, riposta (omija `HitData` – liczona z samych obrażeń broni, bez zmian).
- Reprezentacja: `HitData.split` (`MagicSplit` – struktura wartościowa z udziałami magii bez żywiołu i czterech żywiołów). Nieaktywny = dawny zapis `element/element2` (stare ścieżki bez zmian). `AddMagic` przechodzi na podział tylko, gdy dawny zapis nie wystarcza. `element/element2` są wtedy opisem (dominujący i drugi żywioł: barwa, dźwięk, warunki). Bez alokacji, kopia trafienia ma własny podział. Wszystkie odczyty udziałów (`ShareOf`, `Has`, `Weighted`, `ScaleMagicByElement`) obsługują podział; dawni pisarze `element` (Odpowiedź łotrzyka, Chór strzał, nadpisanie żywiołu unikatu, zaklęcie broni) przy aktywnym podziale używają `AddMagic`/`SetSingleElement`. Czary HU z 3–4 składowymi dostają teraz pełny podział zamiast dwóch najsilniejszych.
- Zaklęcie broni innego żywiołu niż broń jest osobnym składnikiem (wcześniej przejmowało żywioł broni); pozostałe przypadki zaklęcia bez zmian.
- Reakcje (szok termiczny, przewodzenie itd.) nadal działają proporcjonalnie do udziału żywiołu w trafieniu – istniejąca reguła; dodatek sam nie nakłada efektów (statusy pochodzą tylko z listy `statuses`).

### 3.2. Przebicie
- `HitData.penFire/penFrost/penLightning/penEarth` zamiast jednej ważonej wartości `penetration` (usunięta). `DamageResolver.ApplyElementResistances`: dla każdego składnika `x = mnożnik żywiołu celu + osłabienie (Sąd przysięgi, Zbroja runożercy)`, przebicie tylko gdy `x < 1`, najwyżej do 1 (`AffixMath.ResistMultiplier`). Podatność bez zmian. Magia bez żywiołu bez zmian. Dotyczy wszystkich bezpośrednich trafień gracza z `PrepareOutgoing` (także czarów).
- Strefy i aury: trafienie strefy powstaje z `SkillHitData`, więc niesie przebicie jak dotąd (istniejąca reguła, nieruszana). Tyknięcia statusów (podpalenie, krwawienie) nie przechodzą przez `PrepareOutgoing` – bez przebicia. Sługi – bez dziedziczenia (ich trafienia nie dostają nowych pól).

### 3.3. Blok tarczą
- `ShieldBlockCost` (osobna statystyka, nie `BlockCostReduction`). W `BuildCalculator` po dotychczasowej redukcji perka: `stability × (1 − clamp(A/100, 0, 0,5))`, tylko gdy gardę daje założona tarcza. `BuildSnapshot.BlockCost` = ta sama reguła co `DamageResolver`. Blok bierny, okno parowania, ataki nieblokowalne – bez zmian.
- Istniejąca reguła: stary perk `BlockCostReduction` ≥ 100% daje koszt 0 (nie naprawiane – poza zakresem).

### 3.4. Postawa
- `HeavyPoiseDamage`: `poiseDamage × (1 + suma/100)` po mnożnikach ruchu, gdy ręka główna to ciężka broń wręcz (`WeaponFamilies.IsHeavyMelee`: rodzina HeavyWeapon, bez łuku i fokusu), dla zwykłego ciosu, przeniesionego ciosu, techniki wręcz Turris i HU. Bez riposty, fali ciosu ładowanego, drugiej ręki, efektów cech/unikatów, źródeł z zerową postawą. Odporności i regeneracja postawy bossów bez zmian.

### 3.5. Szybkość rzucania
- `PlayerCombat.EffectiveCastTime(def)` = `ScaleStartup(castTime) / (1 + CastSpeed/100)` tylko dla `IsSpell`; używane przy starcie akcji `Cast` i w kolejce Dwóch taktów. Recovery, `cancelAfter`, czas aktywny, odnowienie, koszt – bez zmian. Animacja rzucania jest sterowana postępem fazy akcji, więc klip dochodzi do wyzwolenia razem z rozgrywką; efekt zbierania mocy dostaje efektywny czas. Kopie, echa i wyzwalacze nie przechodzą przez start akcji – bez drugiego skrócenia. Zasady anulowania i zwrotów bez zmian (zmienia się tylko długość fazy).

### 3.6. Sługi
- `MinionDamage`: migawka w `Servant.Spawn` – funkcja trafienia sługi opakowana raz mnożnikiem z chwili przyzwania (`Servant.MinionMultiplier`). Składniki chimery i komendy dyrygenta niosą tę samą funkcję (bez ponownego mnożenia); śmiertelny sługa (Serce szóstego żebra) – ten sam mnożnik, bez zdrowia. Wcielenie używa surowej migawki (`SnapshotHit` bez premii) – atak gracza. Kokon (Grymuar niemego lodowca) i wybuchy unikatów – bez mnożnika sług.

### 3.7. Jakość i zakładanie
- Nowe premie są statystykami/efektami egzemplarza, więc skaluje je `HuQualityFactor` raz przy agregowaniu (T3 rzucania 12% → 18% przy jakości 2 itd.). Poziom broni zwiększa tylko bazę broni. Plecak, sklep i nieaktywny zestaw Arsenału nie dają statystyk (dotychczasowe agregowanie z założonych przedmiotów).

## 4. Kwalifikacja przedmiotów (filtr `Filter`)

| Filtr | Grupy | Przedmiot |
|---|---|---|
| `penetration` | `*_pen` | dowolna broń, grymuar (druga ręka dająca czar), amulet |
| `heavy_melee` | `poise_damage` | ciężka broń wręcz wg `WeaponFamilies` |
| `minion` | `minion_damage` | hełm albo grymuar, którego umiejętność ma rodzaj `Summon` |
| `shield` | `shield_block_cost` | tarcza (`IsShield`) |
| `caster` | `cast_speed` | broń-fokus (różdżka, kostur) albo grymuar |

`RequiredTags` zachowują semantykę „którykolwiek”; filtr jest dodatkowym warunkiem (`HellEconomy.Qualifies`). Nieznany filtr nie dopuszcza.

## 5. Zapis i migracja

- `ItemInstance.affixIds` (trwałe id) i `affixVersion` (2 = pakiet 2026-11-01), zapisywane w `RunSnapshot.Item` (wyposażenie, plecak, Arsenał – jedyne miejsce utrwalania egzemplarzy). Wartości zawsze z zapisanych list `affixes/affixEffects` – jedno źródło; id i wersja tylko identyfikują znaczenie.
- Stary zapis (brak pól → wersja 0): premie bez zmian, bez ponownego losowania i bez zgadywania żywiołu. W opisie przedmiotu dawne płaskie premie fizyczne przedmiotu z „dodanym żywiołem” mają dopisek „Premia obrażeń broni ze starego zapisu”. Ponowny zapis zachowuje wersję 0 (idempotentne).

## 6. Interfejs

- Linie affiksów: „Dodaje obrażenia od ognia równe X% bazowych obrażeń broni”, „Trafienia ignorują X p.p. odporności na ogień”, „Przygotowanie czarów szybsze o X%”, „Obrażenia przyzwanych sług +X% (przy przyzwaniu)”, „Ciosy ciężką bronią wręcz: +X% obrażeń postawy”, „Aktywny blok tarczą kosztuje X% mniej wytrzymałości (razem do 50%)” – wartości po jakości.
- Panel statystyk: składniki lekkiego ciosu („fiz. 120 · ogień 30”), linia „Specjalizacje” (rzucanie osobno od odnowień, sługi – nowe przyzwania, postawa z ostrzeżeniem bez ciężkiej broni, blok z limitem i nadwyżką). Analiza buildu w sklepie pokazuje podział składników zwykłego ataku.

## 7. Testy i pomiary

Wyniki w `TestResults/affiksy-specjalizacyjne/` (katalogi `editmode-koncowe`, `playmode-pelne`, `playmode-powtorka`, `playmode-nowe`, `pojedyncze`, `diag`).

### 7.1. EditMode (końcowy kod)
**307 zaliczonych / 2 niezaliczone / 1 pominięty (310).** Nowe: `AffixSpecializationTests` (17 – E01…E14) i `AffixBalanceMeasurementTests` (1) – wszystkie zielone.
Dwa niezaliczone to znane testy shaderów `AnimeForward` (`GothicArenaTests.Materials_CompileAndSkyTextureIsPresent`, `PaladinAnimeTests.Avatar_MapsToesAndSpine_AndArmorIsMatte`): `Material.FindPass` = −1 w `-batchmode -nographics`; opisane w PROGRESS jako znane („te same 2 znane”), shadery nie były ruszane.
Zmienione oczekiwanie: `HuPortTests.AllAffixes_AreSupported…` sprawdza teraz `HellEconomy.AffixSupported` (affiksy mają własną mapę – nowe statystyki nie istnieją w mapie przedmiotów bazowych i perków).

### 7.2. PlayMode
- Nowe `AffixSpecializationPlayTests` (tryb `window`, osobno i w pełnym przebiegu): **4/4** – P01 (fizyczny miecz + dodany ogień przeciw odpornemu i podatnemu demonowi: składnik ognia = B × 50% × premia ognia × wspólne mnożniki ciosu, obrażenia = fiz. + ogień × mnożnik demona, jedno zdarzenie trafienia), P06 (postawa ×1,25 ciężkim toporem, miecz bez zmian), P07 (rzeczywisty czas fazy przygotowania czaru skrócony 1,5× przy +50%), P10 (migawka sług przed/po zmianie sprzętu, składniki chimery bez ponownego mnożenia, Wcielenie bez premii).
- Pełny PlayMode `window` (kod przed ostatnią poprawką aur/przywołań): **263 zaliczone / 5 niezaliczonych / 58 pominiętych (326)** – pominięte to galerie i boty na żądanie.
  - `FrameCostPlayTests.Floor1Combat_FrameWorkFitsBudget` (19,8 ms przy limicie 16,7) i `HuSkillCasterPlayTests.EveryHuSkillAndForm…` (Krok wichru bez obrażeń) – znane niestabilne/środowiskowe wg PROGRESS.
  - `DemonPlayTests.ParriedCharge…` i `UniqueScenarioPlayTests.Bellringer…` (parowanie) padają też pojedynczo. Diagnoza tymczasowym logowaniem (test przywrócony bez zmian): każde trafienie szarży to czysto fizyczny, stary zapis (`split` nieaktywny, bez żywiołu, `parryable = true`), a w chwili uderzenia akcja gracza jest już w `Parry/Recovery` – okno parowania mija przed trafieniem. Kod okna parowania, czasu szarży i rozstrzygnięcia trafienia fizycznego nie był zmieniany w tym pakiecie; `Bellringer` należy do równoległych, niezacommitowanych prac nad unikatami. Nie dało się uruchomić punktu wyjścia bez moich zmian (te same pliki zawierają cudze niezacommitowane zmiany) – **przyczyna do potwierdzenia przez autora tych prac**.
  - `GamepadPlayTests.FullFlow_WithGamepadOnly`: po 22 naciśnięciach „w dół” postój nie przechodzi do gry – test liczy przyciski kolumny postoju; pakiet nie dodał przycisków (tylko etykiety), brak wyjątków UI w logu. Do sprawdzenia razem z równoległymi zmianami interfejsu.

> **Późniejsza zmiana (audyt buildcraftu, 2026-11-01):** dodane żywioły dają 3,5% bazy broni za punkt zamiast 5%. Maksimum z tabeli poniżej to teraz +367,5% B zamiast +525%; pozostałe liczby w tym rozdziale opisują stan sprzed zmiany.

### 7.3. Pomiary (rozdz. 10.3; deterministycznie, `AffixBalanceMeasurementTests`, plik `TestResults/affiksy-specjalizacyjne/pomiary.md`)

Wartości: brak premii / typowy affiks (T2, jakość 0) / maksymalny legalny zestaw po jakości 2 (×1,5).

#### Blok tarczą (Tarcza HU, rycerz)

| Wariant | Premia | Koszt bloku (obciążenie 20 / 40) | Bloki z pełnej wytrzymałości (40) |
|---|---|---|---|
| brak | −0% | 13,0 / 26,0 | 3 (wytrz. 96) |
| T2 typowy | −14,5% | 11,1 / 22,2 | 4 (wytrz. 96) |
| T3 maks. × jakość 2 | −30% | 9,1 / 18,2 | 5 (wytrz. 96) |

#### Szybkość rzucania (Deszcz siarki, przygotowanie 0,250 s przed tempem gracza)

| Wariant | Premia | Przygotowanie | Rzuty przygotowania w 10 s (bez recovery i odnowienia) |
|---|---|---|---|
| brak | +0% | 0,250 s | 40,0 |
| T2 typowy | +9% | 0,229 s | 43,6 |
| fokus T3 + grymuar T3, jakość 2 | +36% | 0,184 s | 54,4 |

#### Postawa ciężkiej broni (Topór HU: lekki 45, ciężki 80 postawy)

Boss: Władca Bramy, postawa 150, regeneracja 25/s po 2 s.

| Wariant | Premia | Ciężkie ciosy do przełamania | Lekkie ciosy do przełamania |
|---|---|---|---|
| brak | +0% | 2 | 4 |
| T2 typowy | +17% | 2 | 3 |
| T3 maks. × jakość 2 | +37,5% | 2 | 3 |

#### Obrażenia sług (migawka przy przyzwaniu)

| Wariant | Premia | Mnożnik DPS nowego sługi |
|---|---|---|
| brak | +0% | ×1,00 |
| T2 typowy | +22% | ×1,22 |
| hełm T3 + grymuar T3, jakość 2 | +90% | ×1,90 |

#### Dodane żywioły (miecz HU, B lekkiego ciosu = 50,7)

| Wariant | Suma dodanego ognia | Składnik ognia przed premią | Udział w ciosie (bez premii) |
|---|---|---|---|
| brak | +0% | 0,0 | 0% |
| T2 typowy (jeden slot) | +32,5% | 16,5 | 25% |
| T3 × jakość 2 na 5 slotach (broń, rękawice, 2 pierścienie, amulet) | +525% | 266,3 | 84% |

| Przebicie ognia | Odporność 50% | Odporność 25% | Podatność +25% |
|---|---|---|---|
| brak (0 p.p.) | ×0,50 | ×0,75 | ×1,25 |
| T2 typowy (7 p.p.) | ×0,57 | ×0,82 | ×1,25 |
| broń + grymuar + amulet T3, jakość 2 (45 p.p.) | ×0,95 | ×1,00 | ×1,25 |

#### Dostępność nowych grup (500 losowań na komórkę, rzadkość jak w sklepie; to częstość w próbce, nie gwarancja dropu)

| Przedmiot | Piętro | przebicie (dowolne) | postawa | sługi | blok | rzucanie |
|---|---|---|---|---|---|---|
| Topór HU | 2 | 0% | 2,6% | 0% | 0% | 0% |
| Topór HU | 10 | 7,6% | 3,2% | 0% | 0% | 0% |
| Topór HU | 20 | 5% | 2% | 0% | 0% | 0% |
| Kostur HU | 2 | 0% | 0% | 0% | 0% | 13,8% |
| Kostur HU | 10 | 7% | 0% | 0% | 0% | 11% |
| Kostur HU | 20 | 6,2% | 0% | 0% | 0% | 14,4% |
| Tarcza HU | 2 | 0% | 0% | 0% | 6,2% | 0% |
| Tarcza HU | 10 | 0% | 0% | 0% | 5% | 0% |
| Tarcza HU | 20 | 0% | 0% | 0% | 7% | 0% |
| Hełm ciężki | 2 | 0% | 0% | 3,6% | 0% | 0% |
| Hełm ciężki | 10 | 0% | 0% | 5,4% | 0% | 0% |
| Hełm ciężki | 20 | 0% | 0% | 2,8% | 0% | 0% |
| Grymuar przywołań | 2 | 0% | 0% | 4,6% | 0% | 7% |
| Grymuar przywołań | 10 | 5,6% | 0% | 6,6% | 0% | 5,6% |
| Grymuar przywołań | 20 | 6,8% | 0% | 4,6% | 0% | 8,2% |
| Amulet | 2 | 0% | 0% | 0% | 0% | 0% |
| Amulet | 10 | 4% | 0% | 0% | 0% | 0% |
| Amulet | 20 | 2,4% | 0% | 0% | 0% | 0% |

#### Wnioski (oszacowania, nie dane z gry)
- **Brak blokerów:** brak wielokrotnego naliczenia (testy E05/E06/E07, P01), brak darmowego bloku (koszt > 0 przy maksimum, limit nowej części 50%), brak nowej stałej kontroli bossa (P06 i tabela: ciężkie ciosy do przełamania bez zmian, lekkie 4 → 3).
- **Dodane żywioły – ryzyko dominacji:** maksymalny legalny zestaw (ten sam żywioł na 5 slotach, T3, jakość 2) daje +525% B jako ogień – 84% ciosu. Wcześniej te same affiksy dawały tyle samo procent premii fizycznej; teraz dodatek dodatkowo mnoży premia ognia (do 6 slotów `fire_pct`) i omija rozcieńczenie innymi premiami fizycznymi, a ogranicza go odporność demonów. Zakres zgodnie z poleceniem bez zmian – do obserwacji w graniu (kandydat do strojenia: waga lub górny tier `*_add`, nie życie wrogów).
- **Przebicie:** 45 p.p. (broń + grymuar + amulet, T3, jakość 2) praktycznie znosi odporność 50% (×0,95); przy jednym affiksie T2 to +14% obrażeń przeciw odpornemu celowi.
- **Szybkość rzucania** ma mały wpływ przy obecnych czasach przygotowania czarów HU (0,25 s): maksimum skraca o 0,066 s. Uczciwa, ale słaba premia; wartości pozostawiono (hipoteza startowa) – ewentualnie rozważyć objęcie recovery w przyszłym pakiecie.
- **Postawa:** przy postawie bossa 150 i ciosach 45/80 premia działa skokowo (próg liczby ciosów); typowa wartość daje −1 lekki cios do przełamania.
- **Dostępność:** nowe grupy wypadają na właściwych przedmiotach (2–14% przedmiotów w próbce zależnie od grupy i piętra), nigdy na niewłaściwych (np. postawa tylko na toporze, blok tylko na tarczy). Przebicie od piętra 3.


## 8. Niezweryfikowane

- **Zrzuty interfejsu (P12):** tooltipy, porównanie i panel statystyk sprawdzone tylko tekstowo (testy `ItemLines`, opisy statystyk) – ekrany OnGUI nie renderują się w trybie `gfx`, a w trybie `window` nie ma galerii ekwipunku z nowymi affiksami. Czytelność w 720p/1080p i obsługa padem **niesprawdzone wizualnie**.
- **P02–P05, P08, P09, P11 w silniku:** cztery dodatki + zaklęcie broni, techniki HU/Turris, kopie/odpryski/wybuchy, blok tarczą w walce, anulowanie czaru, Dwa takty z premią, chimera w grze – pokryte testami EditMode na prawdziwych funkcjach (E06–E14, `PrepareOutgoing`, `BuildCalculator`), bez osobnych scenariuszy PlayMode.
- **Pełny PlayMode po ostatniej poprawce** (wyłączenie aur i przywołań z dodatku żywiołów i premii postawy) – uruchomione EditMode (całość) i nowe testy PlayMode; pełnego PlayMode nie powtórzono.
- **Import z rzeczywistego źródła HU** – scalanie sprawdzone na kopii zaimportowanych danych (źródło HU nieużywane); funkcja `merge` jest ta sama dla obu ścieżek.
- **Boty** (`HeroBot.Gear`) nie wyceniają nowych statystyk – oceniają przedmioty jak dotąd (serie botów wstrzymane, poza zakresem).
- **Porażki parowania i pada** z rozdz. 7.2 – brak punktu wyjścia do porównania.

