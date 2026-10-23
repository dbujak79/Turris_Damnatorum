# Zasoby zewnętrzne i ich pochodzenie

Wszystko, czego nie wygenerował kod Turris, z pochodzeniem i licencją. Zasoby bohaterów pochodzą z gry *Hell Unleashed*
tego samego autora (`C:\GIT\Hell Unleashed`, plik `THIRD_PARTY_NOTICES.md` tamtego repozytorium). Poniżej skrót dla Turris.

## Modele bohaterów i broni – Meshy

`Assets/TurrisDamnatorum/Models/Heroes/*` (15 bohaterów) oraz część broni w `Models/Weapons/`:

- **Bronie:** `knight_sword`, `greataxe`, `warhammer`, `mage_staff_*`.
- **Pochodzenie:** wygenerowane 2026-10-17 w Meshy na koncie właściciela projektu.
  - Ścieżka: koncept → image-to-3D (meshy-7.1, PBR) → auto-rig → animacje z biblioteki Meshy.
  - Złożone skryptami Hell Unleashed (`tools/art/blender/assemble_meshy_hero.py`).
  - Przekonwertowane do FBX skryptami Turris (`tools/art/convert_hu_*.py`).
- **Licencja:** prawo do użycia komercyjnego wynika z warunków Meshy dla płatnego planu – **sprawdzić przed wydaniem**.

## Modele broni zbudowane w Blenderze (Hell Unleashed)

- **Pliki:** `knight_shield`, `mage_grimoire`, `rogue_bow_rigged`, `rogue_dagger`, `flail`, `morningstar`.
- **Import P1:** także `flail_2h`, `morningstar_2h`; trzy istniejące kostury i aktualne topory/młoty HU zachowują pochodzenie swoich źródłowych modeli.
- **Pochodzenie:** zbudowane skryptami Blendera Hell Unleashed na potrzeby tamtej gry, bez materiału źródłowego.

## Animacje – Mixamo (Adobe)

- **Pliki:** `Assets/TurrisDamnatorum/Models/Animations/Mixamo/`.
  - Paczki: „Great Sword Pack”, „Pro Sword and Shield Pack”, „Pro Longbow Pack”, „Longbow Locomotion Pack”.
  - Pojedyncze klipy: „Falling To Roll”, „Standing Melee Attack Downward”, „Standing Melee Attack 360 High”, „Standing Melee Combo Attack Ver. 3” i inne z folderu `Loose/`.
- **Pochodzenie:** pobrane przez właściciela projektu z jego konta Mixamo.
- **Licencja:** Mixamo pozwala używać animacji bez opłat, także w grach komercyjnych, ale bez rozpowszechniania samych plików Mixamo jako osobnych zasobów.

## Głowy bohaterów – MakeHuman

Siatka bazowa i targety MakeHuman (**CC0 1.0**) zostały użyte w Hell Unleashed przy części modeli. Atrybucja nie jest wymagana.

## Tekstury efektów Hell Unleashed (2026-10-22)

- **Pliki:** 38 PNG w `Assets/TurrisDamnatorum/Resources/HellVfx/`.
- **Źródło:** kopie z `Hell Unleashed/src/Game/Assets/Textures/Vfx`, projektu tego samego autora.
- **Pochodzenie części grafik:** oryginalny `generated_manifest.json` opisuje generowanie i wybór
  wariantów tekstur; jego kopia pozostaje przy plikach. Pozostałe tekstury i receptury pochodzą z oprawy HU.
- **Identyfikacja kopii:** `migration_manifest.json` zawiera SHA-256 każdej tekstury i pliku receptur.
  Ten import nie zmienia praw/licencji źródłowych ani nie stanowi ponownej deklaracji ich warunków.

## Demony Hell Unleashed — pełny katalog P1 (2026-10-22)

- `Models/Demons/`: 34 źródłowe modele i trzy wcześniejsze warianty chochlika, siatki, szkielety, tekstury i klipy z gry tego samego autora.
- 29 brakujących modeli skopiowano i przekonwertowano GLB → FBX; wcześniejsze eksporty zachowano.
  Nie generowano nowych modeli. Pochodzenie i SHA źródłowych GLB: `Models/hu_p1_manifest.json`.
- Prawa i pierwotne pochodzenie wynikają z `THIRD_PARTY_NOTICES.md` Hell Unleashed i zapisów autora;
  konwersja formatu nie zmienia licencji.
