# Przeniesienie bohaterów z Hell Unleashed

Następne etapy: [plan całej dalszej migracji](PLAN_MIGRACJI_HELL_UNLEASHED.md).
Efekty cząsteczkowe i sprawdzenie użycia przez 29 skilli HU:
[MIGRACJA_EFEKTOW_HU.md](MIGRACJA_EFEKTOW_HU.md).
Repozytorium Hell Unleashed jest wyłącznie źródłem do odczytu; wszystkie zmiany powstają w Turris.

Aktualizacja P0/P1: wdrożono etapy 1–3, opisane w [raporcie](RAPORT_MIGRACJI_P0_P1.md).
Aktualizacja 2026-10-23: poprawiono kierunek stóp i sztywność stalowych nosków paladyna/wojownika.
Aktualny zakres, wyniki testów i pozostałe zadania są w [planie migracji](PLAN_MIGRACJI_HELL_UNLEASHED.md).
Etap 4 nadal nie został rozpoczęty; poniższy opis zachowuje historię wcześniejszego portu bohaterów.

Źródło: `C:\GIT\Hell Unleashed` (Godot 4.7 C#, ten sam autor). Cel: 3 typy bohatera (Wojownik, Mag, Łotrzyk) z 12 specjalizacjami,
ich modele 3D, rig, animacje, bronie startowe i startowe skille – „tak jak to tam jest”.

## Decyzje użytkownika (2026-10-22)

| Pytanie | Decyzja |
|---|---|
| Struktura | jak w HU: 3 typy bohatera + specjalizacje (bazowy = „Bez specjalizacji”) |
| Obecne klasy Turris | **zastąpione danymi HU**: Rycerz → Wojownik, Mag → Mag, Łotrzyk → Łotrzyk; id assetów (`class_knight`, `class_mage`, `class_rogue`) zostają, więc profil i odblokowania działają |
| Tekstury | **oryginalne 4K** (bez zmniejszania) |
| Żywioły | Woda → Mróz, Powietrze → Błyskawica, **nowa Ziemia** (Zachwianie → Ogłuszenie) |
| Skill klasy, cechy, uniki | **wszystko**: skill klasy na 4. przycisku (obok 3 slotów Turris), 9 cech, 4 style uniku |
| Skille | Turris dostosowany do mechaniki skilli HU (aury, przywołania, fale, przebijanie, rozszczep, doskok…) |

## Przeliczenie liczb

- Obrażenia HU × **2,5** (miecz HU 16 ↔ miecz długi Turris 40; demony HU 20–120 PŻ ↔ wrogowie Turris 220–400).
- Promienie, zasięgi, czasy, odnowienia, prędkości pocisków – bez zmian (HU już w metrach; długości broni HU w pikselach / 24).
- Zdrowie bohatera: z cech Turris (`baseHealth + healthPerToughness × Wytrzymałość`); cechy HU (Siła, Zręczność, Inteligencja,
  Wytrzymałość, Kondycja) → Turris: Siła, Zręczność, Inteligencja, Wytrzymałość = średnia (Wytrzymałość, Kondycja).

## Zasoby (zrobione)

| Co | Gdzie | Jak |
|---|---|---|
| 15 bohaterów (FBX, T-pose, kości Mixamo, gniazda `HandSlot.R/L`, klipy Meshy) | `Assets/TurrisDamnatorum/Models/Heroes/<nazwa>/` | `tools/art/convert_hu_heroes.sh` (Blender 5.2: `convert_hu_hero.py`, tekstury `hu_textures.py`) |
| 13 broni (FBX + tekstury) | `Models/Weapons/<nazwa>/` | `tools/art/convert_hu_weapon.py` |
| Animacje Mixamo (miecz i tarcza, miecz dwuręczny, łuk, luźne) | `Models/Animations/Mixamo/` | kopia z `Hell Unleashed/src/Game/Assets/Downloads/Mixamo/extracted`; strzał z łuku sklejony `tools/art/mixamo_concat.py` |
| Materiały, zestawy animacji broni, definicje wyglądu | `Content/Visuals/` | menu „Turris/Bohaterowie z Hell Unleashed…” albo `HeroAssetImport.RunBatch` |

Konwerter ustawia pozę spoczynkową w T-pose (Unity traktuje ją jako T-pose awatara; modele Meshy stały w A-pose)
i przepieka klipy kopiowaniem przekształceń świata z oryginału – ruch siatki jest identyczny jak w HU (sprawdzone renderem).

## Stan (2026-10-22)

| Etap | Stan |
|---|---|
| 1. Wygląd: modele, gniazda broni, zestawy animacji, modele przedmiotów | gotowe (`HeroModelPlayTests`, galeria `hero_*`) |
| 2. Klasy i specjalizacje: cechy, style uniku, skill klasy, wybór w menu | gotowe (`HeroClassTests`, `ClassLoadoutTests`, test pada) |
| 3. Ziemia i efekty: zachwianie → ogłuszenie, pęknięcie pancerza, wybuch żaru, błoto | gotowe (`HeroClassTests`) |
| 4. Mechaniki skilli: aury, sługi, fala, przebijanie, rozszczep, wybuch, doskok, impulsy novy, przeciążenie, nieśmiertelność | gotowe (`HeroSkillPlayTests` w silniku) |
| 5. Bronie: druga ręka (grymuar, sztylet do rzucania), dwie bronie na zmianę, różdżki z pociskiem żywiołu | gotowe |
| 6. Treść w assetach | gotowe: `HeroAssetImport.ApplyContentBatch` |
| 7. Testy i dokumentacja | gotowe: EditMode 146/146, PlayMode 61/61 |

## Mapowanie

- **Cechy HU → Turris:** Siła, Zręczność i Inteligencja bez zmian; Wytrzymałość = średnia Wytrzymałości i Kondycji.
- **Statystyki klasy HU → `classModifiers`:**
  - zdrowie bazowe: (HU − 110) × 2;
  - pancerz × 6 + odporność fizyczna × 100 → Obrona fizyczna;
  - redukcja i premia do obrażeń → Redukcja obrażeń, Wszystkie obrażenia;
  - odporności: woda → mróz, powietrze → błyskawice, ziemia → Odporność na ziemię;
  - szybkość ruchu: px / 112;
  - mistrzostwa HU 1:1, gdzie Turris ma odpowiednik; penetracja → +10% obrażeń żywiołu.
- **Efekty:** Soaked/Slow → Chłód; Ignite → Podpalenie; Stagger/Stun/Sunder – nowe.
- **Ziemia:** reakcje Mud i Ember burst jako *błoto* i *wybuch żaru*. Para (woda w płonący cel) nie została przeniesiona – w Turris działa szok termiczny.
- **Poziomy skilli:** HU I–III → Turris +0…+2: moc +12% za poziom, a cechy poziomów (`LevelFeature`) odpowiadają zmianom z HU (więcej warstw, szersza fala, przebicie, drugi pocisk).

## Świadome różnice i braki względem HU

- **Poza bohatera:** adapter Unity `HellEquipmentPose` dodaje IK drugiej ręki i tarczy, pozy sztyletu/różdżki,
  ruch cięciwy oraz lustrzane ramiona w ciosie lewą ręką. Zachowuje klipy i ich synchronizację z walką.
  Dalsza ręczna ocena chwytów na wszystkich wyglądach pozostaje potrzebna.
- **Wygląd** można wybrać niezależnie od klasy w przygotowaniu runu; domyślnie pozostaje zgodny ze specjalizacją.
- **Pobożność** nie rezerwuje many.
- **Sługi** to proceduralne widma Turris (zielona poświata), a nie modele demonów HU. Golem nie jest używany przez żaden startowy skill.
- **Turris zachowuje swoje mechaniki:** flaszki, parowanie, ripostę, 3 sloty, ekwipunek i meta-postęp. Skill klasy jest dodatkowym, czwartym przyciskiem.
- Domyślne odblokowania Turris (Tąpnięcie, Ognisty krąg, Wachlarz ostrzy, Pocisk arkanów) nadal dochodzą typom bohatera i ich specjalizacjom.
- **Zasoby:**
  - 13 nieużywanych w HU modeli bohaterów (KayKit/Blender) nie zostało przeniesionych;
  - z broni: modele startowe, korbacz i morgenstern dwuręczny, kostury, aktualne warianty topora/młota HU;
  - dane: mapowanie 33 z 53 broni, pozostałe warianty opisuje audyt P0;
  - demony: wszystkie 34 źródłowe modele/definicje i wcześniejsze warianty chochlika, z istniejących GLB/klipów.

## Narzędzia

| Plik | Do czego |
|---|---|
| `tools/art/convert_hu_hero.py` | jeden bohater: .glb → `<nazwa>.fbx` (T-pose) + `<nazwa>_clips.fbx` |
| `tools/art/convert_hu_heroes.sh` | wszyscy bohaterowie + tekstury (`hu_textures.py`) + plik chwytów |
| `tools/art/convert_hu_weapon.py` | broń: .glb → .fbx + tekstury według roli |
| `tools/art/mixamo_concat.py` | sklejanie klipów Mixamo (strzał z łuku = naciągnięcie + odrzut) |
| `Scripts/Editor/HeroAssetImport.cs` | materiały, zestawy animacji, wyglądy, treść (`RunBatch`, `ApplyContentBatch`) |

## Plan (etapy, wykonany)

1. **Wygląd** – modele, gniazda broni, zestawy animacji (Mixamo: miecz z tarczą, dwuręczny, łuk; Meshy: czary, śmierć, trafienie),
   modele broni przedmiotów (`ItemDefinition.model`), galeria zrzutów (IK drugiej ręki i tarczy – do zrobienia).
2. **Klasy i specjalizacje** – `ClassDefinition.parent`, cechy HU, style uniku, skill klasy, wybór specjalizacji i broni startowej w menu.
3. **Ziemia i efekty** – żywioł Ziemia, Zachwianie/Ogłuszenie, Spowolnienie, Pęknięcie pancerza.
4. **Mechaniki skilli** – aury, przywołania (sługi), fala przenikająca, przebijanie i rozszczep pocisków, doskok z wybuchem,
   impulsy novy, przeciążenie, nieśmiertelność, okrzyk z redukcją obrażeń.
5. **Bronie** – druga ręka: grymuar (daje novę), sztylet do rzucania (daje rzut nożami), druga broń (dwie bronie na zmianę).
6. **Treść** – 15 klas, bronie, skille w `DefaultContent`, dopisanie przez `SyncContentBatch` (bez nadpisywania).
7. **Testy i dokumentacja.**
