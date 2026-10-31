# Etap 4 i 5: pozostałe skille, bronie i pieczęcie z Hell Unleashed

Stan: 2026-10-23, gałąź `feature/urp-vfx-migration`. Źródło HU tylko do odczytu; dane kopiuje importer
`tools/art/import_hu_catalog.py` do `Resources/HellCatalog/{skills,seals,weapons}.json`.

## Co przeniesiono

| Zakres | Liczba | Gdzie |
|---|---:|---|
| Skille HU wykonywane z komponentów | 61 (29 wcześniejszych + 32 nowe) | `Seals/HuSkillCaster.cs`, `HuRuntimes.cs`, `HuShot.cs`, `HuWorld.cs`, `HuCast.cs` |
| Komponenty | 25 (Projectile, Explosion, Zone, Rain, Barrage, Chain, Nova, Shove, Wave, Vortex, Line, Cone, Trail, Dash, Spin, Barrier, Warcry, Overload, Aura, Heal, Undying, Summon, Fall, Mine, Infuse) | jw. |
| Pieczęcie | 186: 90 form, 51 wsparć (w tym 11 demonicznych), 22 zespolenia, 23 więzi | `Seals/HuSeal.cs`, `HuSkillShape.cs`, `SealSet.cs` |
| Bronie | 19 nowych (12 wariantów, 7 unikatów z mocami) + przeliczenie 28 istniejących | `Core/DefaultContent.HellWeapons.cs`, `Core/HellWeaponCatalog.cs` |
| Moce unikatów | ostatnia szansa, wybuch zwłok, aura żaru, ciernie, łańcuch błyskawic, mroźna osłona, pakt krwi, żniwo dusz, przebicie strzał | `Player/PlayerCombat.Gear.cs` |
| Formy aur | sztandar, krwawy zapał, kolce, wielki płomień, oczyszczenie, pożeracz życia, burzliwy zapał | `Combat/AuraEffect.Hu.cs` |

## Architektura

- **Jedno źródło prawdy dla skilli HU:** parametry poziomów z katalogu HU (komponenty, tagi, celowanie, żywioł).
  Asset `SpellDefinition` wskazuje skill przez `HuData` (po id) i dokłada koszt rzutu, animację, barwę i opis.
  32 nowe definicje generuje `DefaultContent.HellSkills.cs` z katalogu.
- **Pieczęcie** to czyste dane (port `SkillShape.Compose`): forma → wsparcia po kolei → zespolenia → więzi.
  Kopia definicji nigdy nie jest modyfikowana; `SealSet` przy `SpellInstance` trzyma formę, gniazda i kształt.
- **Wykonanie:** `PlayerCombat.FireHu` → `HuSkillCaster.CastByHand` → kopie, echo, przekaźnik, słupy → komponenty.
  Trafienie budowane w jednym miejscu (`PlayerCombat.HuHit`): obrażenia HU × 2,5 × (Int/moc czarów albo premia
  fizyczna) × moc rzutu × udział. Poziom jest w parametrach HU (bez `powerPerLevel` Turris).
- **Efekty HU → Turris:** podpalenie → podpalenie, przemoczenie i spowolnienie → chłód, woda → mróz, powietrze →
  błyskawice; premia „przeciw przemoczonym” → „przeciw wychłodzonym”.
- **Bezpieczniki z HU:** mnożniki wsparć sumują się (limit +200%, podłoga −80%), poziomy pieczęci najwyżej +32%,
  kopie jednego rzutu razem ≤ 1,5× i ≤ 24 zdarzenia, wyzwalacz raz na 1,5 s i za połowę kosztu, odnowienie ≥ 40%
  bazowego, najwyżej 40 stref skilli naraz, Żelazna skóra (dawniej Nieśmiertelność) nie przyjmuje wyzwalaczy ani przyspieszeń.

## Korekty Turris (z audytu balansu)

| Korekta | Powód | Test |
|---|---|---|
| Zasada DPS broni: DPS w Turris w tej samej proporcji do miecza co w HU; tempo z ataków/s względem wzorca szablonu | broń dwuręczna 1,45× miecza zamiast 1,25×; różdżka ziemi bez wolniejszego tempa (P1-a) | `HuContentTests.EveryHuWeapon_KeepsHuDpsRatioToTheSword` |
| Sługi żyją najwyżej tyle, ile trwa odnowienie (chochliki 9/10 s, ogary III 12 s, golem III 18 s) | stałe sługi z jednego slotu (P1-b) | `Summons_DoNotOutliveTheirCooldown` |
| Aktywna aura rezerwuje zasób: Pobożność 20%/30% many (jak HU), inne klasy 20% wytrzymałości | darmowe stałe obrażenia obszarowe (P1-c) | `Aura_ReservesStamina_AndPaysBackWhenOff` |
| Unikaty w nagrodach z wagą 0,35 | w HU tylko z łupów | `UniqueWeapons_CarryTheirHuPower` |
| Aura świętego ognia 8/11/14, Sztandar ×1,25 | audyt skilli P1-a: 2,2× mediany bez kosztu akcji | `HolyFireAura_AndBanner_AreWeakened` |
| Obrażenia sług ×0,4/×0,45/×0,6 | audyt skilli P1-b: 4–6× mediany na pojedynczy cel | `Summons_DamagePerSecond_StaysNearTwiceTheMedian` |
| Formy dominujące +1,5 s odnowienia, dwie słabe wzmocnione | audyt skilli P2-b/c ([AUDYT_SKILLI_PIECZECI.md](AUDYT_SKILLI_PIECZECI.md)) | `DominantForms_PayWithCooldown` |

Korekty skilli i pieczęci są jawne w `Seals/HuBalance.cs` (dane HU zostają nietknięte).

## Skille Turris na komponentach HU

- `Seals/TurrisSkills.cs` dopisuje do katalogu własne skille (katalog HU generuje importer, więc nie w JSON-ie).
- **Ściana ognia** (`spell_firering`, komponent `Wall`): ściana ognia w poprzek celowania; formy **Fala ognia**
  (`wall_speed`) i **Forteca ognia** (`wall_ring`, celowanie na siebie). Przyjmuje zwykłe wsparcia HU.

## Pieczęcie w podejściu

- **Łup:** zwykły wróg 0,5%, elita 35% (25% demonicznych), boss – dar trzech do wyboru (70% demonicznych). Łup trafia
  do sakwy; kopia posiadanej podnosi jej poziom (II, III: +8% obrażeń dla ofensywnych, +12,5% siły dla użytkowych).
- **Sklep dusz na postoju:** trzy oferty (zawsze forma dla posiadanego skilla), budzące zespolenie/więź 3× częściej,
  ceny HU × 2,5 × inflacja, przelosowanie 38 dusz + 25 za każde kolejne. Sprzedaż z sakwy: 35% ceny, demoniczna 100.
- **Ekwipunek → Pieczęcie:** gniazda skilli w ręce, uśpione pieczęcie szare, sakwa z „Włóż”/„Sprzedaj”, aktywne więzi.
- **HUD:** znaczki pieczęci pod kartą skilla. **Poligon:** przełączniki wszystkich pieczęci wybranego skilla.
- **Kodeks:** obudzone zespolenia i więzi zapisywane w profilu (`discoveredSeals`), komunikat „ODKRYCIE”.

## Ograniczenia i różnice

- Słup ofiarny i sztandar to proste bryły; deszcz strzał rysuje spadające i wbite strzały (`HuFallingArrow`).
- Doskok HU przechodzi przez ciała wrogów (zatrzymują go ściany i `stop_on_enemy`); pęd po każdej akcji przycięty do
  prędkości sprintu (wcześniej doskok 25 m/s ślizgał bohatera ~5 m – dotyczyło też szarży Turris).
- Turris nie ma łupu na ziemi: pieczęć trafia od razu do sakwy z komunikatem.
- Kostium HU „ice=1” rysowany rodziną lodu; reakcja Para (woda + ogień) odpowiada szokowi termicznemu Turris.
- Nie ma kodeksu jako osobnego ekranu (tylko zapis w profilu).
- Balans form i wsparć zgodny z pomiarami HU na manekinach; w Turris wymaga ręcznego grania.

## Testy

- EditMode: `HuSealTests` (port `ShippedSealTests`: parsowanie, każda forma zmienia skill, każde zespolenie i więź da się
  obudzić, żadne wsparcie nie psuje skilla, sumowanie mnożników), `HuContentTests` (zasada DPS, sługi, komplet definicji,
  sakwa, poziomy, więzi, zakup atomowy, oferty, dar bossa, unikaty).
- PlayMode: `HuSkillCasterPlayTests` – 151 rzutów (każdy skill na poziomie III i każda forma zadaje obrażenia manekinom,
  bez wyjątków, czyszczenie kończy runtime'y), budżet kopii, wyzwalacz uniku, rezerwacja aury, dystans doskoku, galeria;
  `SealFlowPlayTests` – piętro → nagroda → sklep z pieczęciami i darem → zakup → sakwa → menedżer pieczęci.
