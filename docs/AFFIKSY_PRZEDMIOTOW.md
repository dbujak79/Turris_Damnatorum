# Affiksy przedmiotów losowych – przedrostki i przyrostki

Lista wygenerowana (`python tools/art/affix_docs.py`) z `Assets/TurrisDamnatorum/Resources/HellCatalog/affixes.json`: 131 affiksów z Hell Unleashed i 24 z nakładki Turris (`tools/data/affixes_turris.json`, pakiet specjalizacyjny 2026-11-01). Wartości przeliczone jak w grze (`Run/HellEconomy.cs`: `MapAffix`, `MapEffect`, `RollAffixes`) – to, co dostaje bohater w Turris **przed jakością**.

## Zasady losowania

- **Rzadkość → liczba affiksów:** zwykły 0, magiczny 1 przedrostek + 1 przyrostek, rzadki 2 + 2, legendarny 3 + 3 (od piętra 5), unikat – bez losowych affiksów.
- **Grupa raz na przedmiot:** np. nie wypadną dwa affiksy szybkości ataku. Waga grupy = najwyższa waga jej odblokowanych tierów (waga względna w puli przedmiotu, nie procent dropu).
- **Tier:** tylko tiery z progiem piętra ≤ bieżące piętro (piętro = fala HU); tier losowany z wagą równą numerowi (wyższe częściej).
- **Wartość:** min + (max − min) · q, q losowe 0–1; przedmiot legendarny losuje q dwa razy i bierze lepsze.
- **Jakość HU:** wszystkie premie affiksów × (1 + 0,25 · jakość), raz, przy zakładaniu (elita 0–1, boss 1–2 → do ×1,5). Poziom broni zwiększa bazę broni, nie procent affiksu.
- **Slot, tagi i filtr:** affiks musi pasować do slotu (kolumna „Sloty”); dawne tagi HU działają jak „którykolwiek”; nowe grupy mają wąski filtr przedmiotu (kolumna „Wymaga”) – opisuje przedmiot, nie klasę gracza.
- **Przeliczniki HU → Turris:** życie ×2, pancerz ×6, ułamki → %, dodane obrażenia fizyczne → +5%, dodane żywioły → +3,5% bazy broni za punkt. Wartości płaskie z max ≥ 2 (dane HU) są zaokrąglane do całości przed przeliczeniem; nowe rekordy mają zadeklarowaną precyzję (3 miejsca w jednostkach HU).
- **Dodane żywioły (`fire_add`, `water_add`, `air_add`, `earth_add`):** od pakietu 2026-11-01 to **rzeczywisty składnik danego żywiołu**: dodatek = B × suma dodanych % / 100, gdzie B to obrażenia tego ataku bronią bez premii fizycznej. Działa na zwykłe ciosy, podstawowe strzały łuku/różdżki i techniki (Turris i HU), nie na czary, sługi i efekty wtórne. Premia fizyczna go nie zwiększa, premia tego żywiołu – raz; odporność i przebicie liczone osobno dla każdego żywiołu; sam dodatek nie nakłada podpalenia, chłodu ani porażenia. Stare egzemplarze (sprzed pakietu) zachowują zapisaną premię fizyczną – w opisie „Premia obrażeń broni ze starego zapisu”.
- **Znane (bez zmian w tym pakiecie):** zaokrąglanie płaskich wartości HU spłaszcza zakresy – Trolla (`regen` T3) daje zawsze +4/s, Kontemplacji (`mana_regen` T2) tylko +1/s albo +2/s.
- Przedrostki z HU nie mają polskich nazw (w danych `name` = id) – kolumna „Nazwa” pokazuje wtedy „—”. Przyrostki HU i wszystkie rekordy Turris mają nazwy.

## Przedrostki (92 affiksów, 30 grup)

| Grupa | Tier | Nazwa | Statystyka | Zakres | Od piętra | Waga | Sloty | Wymaga |
|---|---|---|---|---|---|---|---|---|
| `air_add` | T1 | — | Dodane błyskawice (% bazy broni B) | +7% – +14% | 1 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `air_add` | T2 | — | Dodane błyskawice (% bazy broni B) | +17,5% – +28% | 8 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `air_add` | T3 | — | Dodane błyskawice (% bazy broni B) | +31,5% – +49% | 16 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `air_pct` | T1 | — | Obrażenia od błyskawic | +8% – +12% | 1 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `air_pct` | T2 | — | Obrażenia od błyskawic | +13% – +18% | 8 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `air_pct` | T3 | — | Obrażenia od błyskawic | +19% – +26% | 16 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `air_pen` *(Turris)* | T1 | Przenikający burzą | Przebicie odporności na błyskawice | +4 p.p. – +5 p.p. | 3 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `air_pen` *(Turris)* | T2 | Przenikający burzą II | Przebicie odporności na błyskawice | +6 p.p. – +8 p.p. | 10 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `air_pen` *(Turris)* | T3 | Przenikający burzą III | Przebicie odporności na błyskawice | +9 p.p. – +10 p.p. | 18 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `area` | T1 | — | Obszar działania | +5% – +8% | 1 | 0,6 | amulet, hełm, druga ręka | — |
| `area` | T2 | — | Obszar działania | +9% – +13% | 10 | 0,6 | amulet, hełm, druga ręka | — |
| `armour` | T1 | — | Pancerz (obrona fizyczna) | +6 – +12 | 1 | 1 | hełm, zbroja, rękawice, buty, druga ręka | zbroja lekka, zbroja średnia, zbroja ciężka, tarcza |
| `armour` | T2 | — | Pancerz (obrona fizyczna) | +18 – +24 | 8 | 1 | hełm, zbroja, rękawice, buty, druga ręka | zbroja lekka, zbroja średnia, zbroja ciężka, tarcza |
| `armour` | T3 | — | Pancerz (obrona fizyczna) | +30 – +42 | 16 | 1 | hełm, zbroja, rękawice, buty, druga ręka | zbroja lekka, zbroja średnia, zbroja ciężka, tarcza |
| `attack_speed` | T1 | — | Szybkość ataku | +4% – +6% | 1 | 0,9 | broń, rękawice, pierścień | — |
| `attack_speed` | T2 | — | Szybkość ataku | +7% – +10% | 8 | 0,9 | broń, rękawice, pierścień | — |
| `attack_speed` | T3 | — | Szybkość ataku | +11% – +15% | 16 | 0,9 | broń, rękawice, pierścień | — |
| `bleed` | T1 | — | Obrażenia krwawienia | +15% – +25% | 1 | 0,7 | broń, rękawice | broń wręcz, zbroja lekka, zbroja średnia, zbroja ciężka |
| `bleed` | T2 | — | Obrażenia krwawienia | +26% – +40% | 8 | 0,7 | broń, rękawice | broń wręcz, zbroja lekka, zbroja średnia, zbroja ciężka |
| `bleed` | T3 | — | Obrażenia krwawienia | +41% – +60% | 16 | 0,7 | broń, rękawice | broń wręcz, zbroja lekka, zbroja średnia, zbroja ciężka |
| `block` | T1 | — | Szansa na bierny blok | +4% – +6% | 1 | 1 | druga ręka | tarcza |
| `block` | T2 | — | Szansa na bierny blok | +7% – +10% | 10 | 1 | druga ręka | tarcza |
| `earth_add` | T1 | — | Dodana ziemia (% bazy broni B) | +7% – +14% | 1 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `earth_add` | T2 | — | Dodana ziemia (% bazy broni B) | +17,5% – +28% | 8 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `earth_add` | T3 | — | Dodana ziemia (% bazy broni B) | +31,5% – +49% | 16 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `earth_pct` | T1 | — | Obrażenia od ziemi | +8% – +12% | 1 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `earth_pct` | T2 | — | Obrażenia od ziemi | +13% – +18% | 8 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `earth_pct` | T3 | — | Obrażenia od ziemi | +19% – +26% | 16 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `earth_pen` *(Turris)* | T1 | Przenikający skałę | Przebicie odporności na ziemię | +4 p.p. – +5 p.p. | 3 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `earth_pen` *(Turris)* | T2 | Przenikający skałę II | Przebicie odporności na ziemię | +6 p.p. – +8 p.p. | 10 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `earth_pen` *(Turris)* | T3 | Przenikający skałę III | Przebicie odporności na ziemię | +9 p.p. – +10 p.p. | 18 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `evasion` | T1 | — | Uchylenie | +2% – +3% | 1 | 0,8 | hełm, zbroja, rękawice, buty, pierścień | — |
| `evasion` | T2 | — | Uchylenie | +3,5% – +5% | 8 | 0,8 | hełm, zbroja, rękawice, buty, pierścień | — |
| `evasion` | T3 | — | Uchylenie | +5,5% – +7% | 16 | 0,8 | hełm, zbroja, rękawice, buty, pierścień | — |
| `execute` | T1 | — | Obrażenia egzekucji (wróg z niskim życiem) | +15% – +25% | 4 | 0,5 | broń | — |
| `execute` | T2 | — | Obrażenia egzekucji (wróg z niskim życiem) | +26% – +40% | 14 | 0,5 | broń | — |
| `fire_add` | T1 | — | Dodany ogień (% bazy broni B) | +7% – +14% | 1 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `fire_add` | T2 | — | Dodany ogień (% bazy broni B) | +17,5% – +28% | 8 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `fire_add` | T3 | — | Dodany ogień (% bazy broni B) | +31,5% – +49% | 16 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `fire_pct` | T1 | — | Obrażenia od ognia | +8% – +12% | 1 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `fire_pct` | T2 | — | Obrażenia od ognia | +13% – +18% | 8 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `fire_pct` | T3 | — | Obrażenia od ognia | +19% – +26% | 16 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `fire_pen` *(Turris)* | T1 | Przenikający żarem | Przebicie odporności na ogień | +4 p.p. – +5 p.p. | 3 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `fire_pen` *(Turris)* | T2 | Przenikający żarem II | Przebicie odporności na ogień | +6 p.p. – +8 p.p. | 10 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `fire_pen` *(Turris)* | T3 | Przenikający żarem III | Przebicie odporności na ogień | +9 p.p. – +10 p.p. | 18 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `global_pct` | T1 | — | Wszystkie obrażenia | +5% – +8% | 1 | 0,7 | broń, amulet | — |
| `global_pct` | T2 | — | Wszystkie obrażenia | +9% – +13% | 8 | 0,7 | broń, amulet | — |
| `global_pct` | T3 | — | Wszystkie obrażenia | +14% – +19% | 16 | 0,7 | broń, amulet | — |
| `health` | T1 | — | Maks. życie | +16 – +28 | 1 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `health` | T2 | — | Maks. życie | +30 – +48 | 8 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `health` | T3 | — | Maks. życie | +50 – +80 | 16 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `leech` | T1 | — | Kradzież życia | +1% – +2% | 4 | 0,5 | broń, pierścień, amulet | — |
| `leech` | T2 | — | Kradzież życia | +2,5% – +3,5% | 14 | 0,5 | broń, pierścień, amulet | — |
| `mana` | T1 | — | Maks. mana | +10 – +16 | 1 | 0,7 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `mana` | T2 | — | Maks. mana | +17 – +26 | 8 | 0,7 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `mana` | T3 | — | Maks. mana | +27 – +40 | 16 | 0,7 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `minion_damage` *(Turris)* | T1 | Władczy | Obrażenia sług (nowe przyzwania) | +15% – +19% | 1 | 0,5 | hełm, druga ręka | hełm albo grymuar przywołań |
| `minion_damage` *(Turris)* | T2 | Władczy II | Obrażenia sług (nowe przyzwania) | +20% – +24% | 8 | 0,5 | hełm, druga ręka | hełm albo grymuar przywołań |
| `minion_damage` *(Turris)* | T3 | Władczy III | Obrażenia sług (nowe przyzwania) | +25% – +30% | 16 | 0,5 | hełm, druga ręka | hełm albo grymuar przywołań |
| `phys_add` | T1 | — | Obrażenia broni (dodane fizyczne) | +10% – +20% | 1 | 1 | broń, rękawice, pierścień | — |
| `phys_add` | T2 | — | Obrażenia broni (dodane fizyczne) | +25% – +40% | 8 | 1 | broń, rękawice, pierścień | — |
| `phys_add` | T3 | — | Obrażenia broni (dodane fizyczne) | +45% – +70% | 16 | 1 | broń, rękawice, pierścień | — |
| `phys_pct` | T1 | — | Obrażenia fizyczne | +6% – +10% | 1 | 1 | broń, rękawice, pierścień, amulet | — |
| `phys_pct` | T2 | — | Obrażenia fizyczne | +11% – +16% | 8 | 1 | broń, rękawice, pierścień, amulet | — |
| `phys_pct` | T3 | — | Obrażenia fizyczne | +17% – +24% | 16 | 1 | broń, rękawice, pierścień, amulet | — |
| `phys_pen` | T1 | — | Przebicie obrony fizycznej | +4% – +6% | 3 | 0,6 | broń | — |
| `phys_pen` | T2 | — | Przebicie obrony fizycznej | +7% – +10% | 10 | 0,6 | broń | — |
| `phys_pen` | T3 | — | Przebicie obrony fizycznej | +11% – +15% | 18 | 0,6 | broń | — |
| `poise_damage` *(Turris)* | T1 | Kruszący | Obrażenia postawy ciężkiej broni | +10% – +14% | 1 | 0,5 | broń | ciężka broń wręcz (WeaponFamilies; nie łuk, nie kostur) |
| `poise_damage` *(Turris)* | T2 | Kruszący II | Obrażenia postawy ciężkiej broni | +15% – +19% | 8 | 0,5 | broń | ciężka broń wręcz (WeaponFamilies; nie łuk, nie kostur) |
| `poise_damage` *(Turris)* | T3 | Kruszący III | Obrażenia postawy ciężkiej broni | +20% – +25% | 16 | 0,5 | broń | ciężka broń wręcz (WeaponFamilies; nie łuk, nie kostur) |
| `reduction` | T1 | — | Redukcja obrażeń | +2% – +3% | 1 | 0,8 | hełm, zbroja, druga ręka | zbroja ciężka, zbroja średnia, tarcza |
| `reduction` | T2 | — | Redukcja obrażeń | +3,5% – +5% | 8 | 0,8 | hełm, zbroja, druga ręka | zbroja ciężka, zbroja średnia, tarcza |
| `reduction` | T3 | — | Redukcja obrażeń | +5,5% – +7% | 16 | 0,8 | hełm, zbroja, druga ręka | zbroja ciężka, zbroja średnia, tarcza |
| `spell_pct` | T1 | — | Moc czarów | +6% – +10% | 1 | 1 | broń, druga ręka | broń czarów |
| `spell_pct` | T1 | — | Moc czarów | +4% – +7% | 1 | 1 | hełm, zbroja, rękawice, buty | szata |
| `spell_pct` | T1 | — | Moc czarów | +4% – +7% | 1 | 0,6 | amulet, pierścień | — |
| `spell_pct` | T2 | — | Moc czarów | +11% – +16% | 8 | 1 | broń, druga ręka | broń czarów |
| `spell_pct` | T2 | — | Moc czarów | +8% – +11% | 8 | 1 | hełm, zbroja, rękawice, buty | szata |
| `spell_pct` | T2 | — | Moc czarów | +8% – +11% | 8 | 0,6 | amulet, pierścień | — |
| `spell_pct` | T3 | — | Moc czarów | +17% – +24% | 16 | 1 | broń, druga ręka | broń czarów |
| `spell_pct` | T3 | — | Moc czarów | +12% – +16% | 16 | 1 | hełm, zbroja, rękawice, buty | szata |
| `spell_pct` | T3 | — | Moc czarów | +12% – +16% | 16 | 0,6 | amulet, pierścień | — |
| `water_add` | T1 | — | Dodany mróz (% bazy broni B) | +7% – +14% | 1 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `water_add` | T2 | — | Dodany mróz (% bazy broni B) | +17,5% – +28% | 8 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `water_add` | T3 | — | Dodany mróz (% bazy broni B) | +31,5% – +49% | 16 | 0,8 | broń, pierścień, amulet, rękawice | — |
| `water_pct` | T1 | — | Obrażenia od mrozu | +8% – +12% | 1 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `water_pct` | T2 | — | Obrażenia od mrozu | +13% – +18% | 8 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `water_pct` | T3 | — | Obrażenia od mrozu | +19% – +26% | 16 | 0,7 | broń, druga ręka, amulet, pierścień, hełm, rękawice | — |
| `water_pen` *(Turris)* | T1 | Przenikający szronem | Przebicie odporności na mróz | +4 p.p. – +5 p.p. | 3 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `water_pen` *(Turris)* | T2 | Przenikający szronem II | Przebicie odporności na mróz | +6 p.p. – +8 p.p. | 10 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |
| `water_pen` *(Turris)* | T3 | Przenikający szronem III | Przebicie odporności na mróz | +9 p.p. – +10 p.p. | 18 | 0,15 | broń, druga ręka, amulet | broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja) |

## Przyrostki (63 affiksów, 24 grup)

| Grupa | Tier | Nazwa | Statystyka | Zakres | Od piętra | Waga | Sloty | Wymaga |
|---|---|---|---|---|---|---|---|---|
| `accuracy` | T1 | Sokoła | Celność | +3% – +5% | 1 | 0,6 | broń, rękawice, hełm, pierścień | — |
| `accuracy` | T2 | Orła | Celność | +6% – +9% | 10 | 0,6 | broń, rękawice, hełm, pierścień | — |
| `air_res` | T1 | Wichru | Odporność na błyskawice (powietrze) | +6% – +10% | 1 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `air_res` | T2 | Burzy | Odporność na błyskawice (powietrze) | +11% – +16% | 8 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `air_res` | T3 | Zawieruchy | Odporność na błyskawice (powietrze) | +17% – +24% | 16 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `all_res` | T1 | Pryzmatu | Odporność na ogień<br>Odporność na mróz (woda)<br>Odporność na błyskawice (powietrze)<br>Odporność na ziemię | +3% – +5%<br>+3% – +5%<br>+3% – +5%<br>+3% – +5% | 6 | 0,5 | amulet, pierścień, zbroja, druga ręka | — |
| `all_res` | T2 | Tęczy | Odporność na ogień<br>Odporność na mróz (woda)<br>Odporność na błyskawice (powietrze)<br>Odporność na ziemię | +6% – +8%<br>+6% – +8%<br>+6% – +8%<br>+6% – +8% | 15 | 0,5 | amulet, pierścień, zbroja, druga ręka | — |
| `cast_speed` *(Turris)* | T1 | Inkantacji | Szybkość rzucania czarów | +5% – +7% | 1 | 0,7 | broń, druga ręka | broń-fokus (różdżka, kostur) albo grymuar |
| `cast_speed` *(Turris)* | T2 | Inkantacji II | Szybkość rzucania czarów | +8% – +10% | 8 | 0,7 | broń, druga ręka | broń-fokus (różdżka, kostur) albo grymuar |
| `cast_speed` *(Turris)* | T3 | Inkantacji III | Szybkość rzucania czarów | +11% – +12% | 16 | 0,7 | broń, druga ręka | broń-fokus (różdżka, kostur) albo grymuar |
| `cooldown` | T1 | Natchnienia | Skrócenie odnowienia umiejętności | +3% – +5% | 1 | 0,7 | hełm, amulet, pierścień, druga ręka | — |
| `cooldown` | T2 | Wizji | Skrócenie odnowienia umiejętności | +6% – +8% | 8 | 0,7 | hełm, amulet, pierścień, druga ręka | — |
| `cooldown` | T3 | Proroctwa | Skrócenie odnowienia umiejętności | +9% – +12% | 16 | 0,7 | hełm, amulet, pierścień, druga ręka | — |
| `earth_res` | T1 | Skały | Odporność na ziemię | +6% – +10% | 1 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `earth_res` | T2 | Góry | Odporność na ziemię | +11% – +16% | 8 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `earth_res` | T3 | Tytana | Odporność na ziemię | +17% – +24% | 16 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `fire_res` | T1 | Popiołu | Odporność na ogień | +6% – +10% | 1 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `fire_res` | T2 | Salamandry | Odporność na ogień | +11% – +16% | 8 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `fire_res` | T3 | Feniksa | Odporność na ogień | +17% – +24% | 16 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `health_pct` | T1 | Niedźwiedzia | Maks. życie | +4% – +6% | 1 | 0,8 | zbroja, hełm, amulet | — |
| `health_pct` | T2 | Tura | Maks. życie | +7% – +10% | 8 | 0,8 | zbroja, hełm, amulet | — |
| `health_pct` | T3 | Smoka | Maks. życie | +11% – +15% | 16 | 0,8 | zbroja, hełm, amulet | — |
| `knockback` | T1 | Taranu | Odrzut ciosów wręcz | +0,3 – +0,5 | 1 | 0,6 | broń | broń wręcz |
| `knockback` | T2 | Lawiny | Odrzut ciosów wręcz | +0,6 – +0,9 | 10 | 0,6 | broń | broń wręcz |
| `life_on_kill` | T1 | Rzeźnika | Życie za zabójstwo | +2 – +4 | 1 | 0,8 | broń, rękawice, pierścień | — |
| `life_on_kill` | T2 | Kata | Życie za zabójstwo | +6 – +8 | 8 | 0,8 | broń, rękawice, pierścień | — |
| `life_on_kill` | T3 | Żniwiarza | Życie za zabójstwo | +10 – +14 | 16 | 0,8 | broń, rękawice, pierścień | — |
| `load` | T1 | Tragarza | Udźwig | +3 – +5 | 1 | 0,7 | zbroja, buty, amulet | — |
| `load` | T2 | Mułu | Udźwig | +6 – +9 | 8 | 0,7 | zbroja, buty, amulet | — |
| `load` | T3 | Atlasa | Udźwig | +10 – +14 | 16 | 0,7 | zbroja, buty, amulet | — |
| `mana_on_kill` | T1 | Pijawki | Mana za zabójstwo | +1 – +2 | 1 | 0,6 | broń, pierścień, hełm | — |
| `mana_on_kill` | T2 | Upiora | Mana za zabójstwo | +3 – +5 | 10 | 0,6 | broń, pierścień, hełm | — |
| `mana_regen` | T1 | Medytacji | Regeneracja many | +0,5/s – +1/s | 1 | 0,7 | hełm, zbroja, amulet, pierścień, druga ręka | — |
| `mana_regen` | T2 | Kontemplacji | Regeneracja many | +1/s – +2/s | 8 | 0,7 | hełm, zbroja, amulet, pierścień, druga ręka | — |
| `mana_regen` | T3 | Ascety | Regeneracja many | +2/s – +3/s | 16 | 0,7 | hełm, zbroja, amulet, pierścień, druga ręka | — |
| `melee_range` | T1 | Zasięgu | Zasięg ataków wręcz | +6% – +10% | 1 | 0,8 | broń | broń wręcz |
| `melee_range` | T2 | Długiego Ramienia | Zasięg ataków wręcz | +11% – +16% | 10 | 0,8 | broń | broń wręcz |
| `move` | T1 | Wędrowca | Szybkość ruchu | +3% – +5% | 1 | 1,2 | buty | — |
| `move` | T2 | Gończego | Szybkość ruchu | +6% – +8% | 8 | 1,2 | buty | — |
| `move` | T3 | Wiatru | Szybkość ruchu | +9% – +12% | 16 | 1,2 | buty | — |
| `phys_res` | T1 | Tarczy | Obrona fizyczna | +3 – +5 | 1 | 0,8 | hełm, zbroja, rękawice, buty, druga ręka, amulet | — |
| `phys_res` | T2 | Bastionu | Obrona fizyczna | +6 – +8 | 8 | 0,8 | hełm, zbroja, rękawice, buty, druga ręka, amulet | — |
| `phys_res` | T3 | Twierdzy | Obrona fizyczna | +9 – +12 | 16 | 0,8 | hełm, zbroja, rękawice, buty, druga ręka, amulet | — |
| `pierce` | T1 | Przeszycia | Przebicie pocisków (liczba celów) | +1 | 6 | 0,4 | broń | broń dystansowa |
| `proj_speed` | T1 | Jastrzębia | Szybkość pocisków | +8% – +12% | 1 | 0,8 | broń | broń dystansowa |
| `proj_speed` | T2 | Błyskawicy | Szybkość pocisków | +13% – +20% | 10 | 0,8 | broń | broń dystansowa |
| `regen` | T1 | Odnowy | Regeneracja życia | +0,8/s – +1,6/s | 1 | 0,9 | hełm, zbroja, rękawice, buty, amulet, pierścień | — |
| `regen` | T2 | Regeneracji | Regeneracja życia | +1,8/s – +3/s | 8 | 0,9 | hełm, zbroja, rękawice, buty, amulet, pierścień | — |
| `regen` | T3 | Trolla | Regeneracja życia | +4/s | 16 | 0,9 | hełm, zbroja, rękawice, buty, amulet, pierścień | — |
| `shield_block_cost` *(Turris)* | T1 | Muru tarcz | Koszt aktywnego bloku tarczą | −10% – −12% | 1 | 0,6 | druga ręka | tylko tarcza |
| `shield_block_cost` *(Turris)* | T2 | Muru tarcz II | Koszt aktywnego bloku tarczą | −13% – −16% | 8 | 0,6 | druga ręka | tylko tarcza |
| `shield_block_cost` *(Turris)* | T3 | Muru tarcz III | Koszt aktywnego bloku tarczą | −17% – −20% | 16 | 0,6 | druga ręka | tylko tarcza |
| `soul` | T1 | Chciwości | Zdobywane dusze | +5% – +8% | 1 | 0,6 | amulet, pierścień, hełm | — |
| `soul` | T2 | Skąpca | Zdobywane dusze | +9% – +13% | 8 | 0,6 | amulet, pierścień, hełm | — |
| `soul` | T3 | Mamony | Zdobywane dusze | +14% – +20% | 16 | 0,6 | amulet, pierścień, hełm | — |
| `stamina` | T1 | Biegacza | Maks. wytrzymałość | +8 – +12 | 1 | 0,8 | hełm, zbroja, rękawice, buty, amulet | — |
| `stamina` | T2 | Atlety | Maks. wytrzymałość | +13 – +20 | 8 | 0,8 | hełm, zbroja, rękawice, buty, amulet | — |
| `stamina` | T3 | Niezmordowania | Maks. wytrzymałość | +21 – +30 | 16 | 0,8 | hełm, zbroja, rękawice, buty, amulet | — |
| `stamina_regen` | T1 | Oddechu | Regeneracja wytrzymałości | +8% – +12% | 1 | 0,7 | buty, zbroja, amulet | — |
| `stamina_regen` | T2 | Wytrwałości | Regeneracja wytrzymałości | +13% – +20% | 10 | 0,7 | buty, zbroja, amulet | — |
| `water_res` | T1 | Przypływu | Odporność na mróz (woda) | +6% – +10% | 1 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `water_res` | T2 | Głębin | Odporność na mróz (woda) | +11% – +16% | 8 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |
| `water_res` | T3 | Lewiatana | Odporność na mróz (woda) | +17% – +24% | 16 | 1 | hełm, zbroja, rękawice, buty, amulet, pierścień, druga ręka | — |

## Działanie nowych grup (pakiet 2026-11-01)

- **Przebicie żywiołu** (`fire_pen`, `water_pen`, `air_pen`, `earth_pen`): odejmuje punkty procentowe od odporności celu na ten żywioł (50% → 40% przy 10 p.p.), tylko dla składnika tego żywiołu, nie schodzi poniżej 0, nie zmienia podatności. Bezpośrednie trafienia gracza, także czary; nie tyknięcia statusów i nie sługi.
- **Kruszący** (`poise_damage`): mnoży obrażenia postawy zwykłych ciosów i technik wręcz wykonanych ciężką bronią; bez riposty, fali ciosu ładowanego, czarów, sług i efektów wtórnych. Zdrowie bez zmian.
- **Władczy** (`minion_damage`): obrażenia sług przyzwanych **po** założeniu (migawka przy przyzwaniu; istniejący sługa się nie zmienia). Chimera dziedziczy migawki składników raz; Wcielenie gracza nie korzysta.
- **Muru tarcz** (`shield_block_cost`): koszt aktywnego bloku założoną tarczą × (1 − suma, najwyżej 50%), raz, po starych perkach; bez wpływu na blok bierny, okno parowania i ciosy nieblokowalne.
- **Inkantacji** (`cast_speed`): przygotowanie ręcznie rzucanego czaru / (1 + suma); bez recovery, odnowienia, kanału, technik, flaszek, parowania i uniku. Animacja przygotowania idzie fazą akcji, więc wyzwala się razem z rozgrywką.
