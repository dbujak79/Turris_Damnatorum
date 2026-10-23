#!/usr/bin/env bash
# Konwersja wszystkich bohaterów Hell Unleashed do Assets/TurrisDamnatorum/Models/Heroes/<nazwa>/.
# Użycie: bash tools/art/convert_hu_heroes.sh ["C:/GIT/Hell Unleashed"]
set -e
HU="${1:-C:/GIT/Hell Unleashed}"
SRC="$HU/src/Game/Assets/Models/Heroes"
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
DST="$ROOT/Assets/TurrisDamnatorum/Models/Heroes"
BLENDER="${BLENDER:-C:/Program Files/Blender Foundation/Blender 5.2/blender.exe}"
HEROES="paladin mage_witch rogue_hooded_meshy hero_warrior hero_warrior_berserker hero_warrior_dual
hero_mage_fire hero_mage_water hero_mage_air hero_mage_earth hero_mage_summoner hero_mage_battle
hero_rogue_archer hero_rogue_assassin hero_rogue_arcane_archer"
for h in $HEROES; do
  mkdir -p "$DST/$h"
  "$BLENDER" -b --python "$ROOT/tools/art/convert_hu_hero.py" -- "$SRC/$h.glb" "$DST/$h/$h.fbx" 2>&1 | grep -E "ZAPISANO|Error" || true
  python "$ROOT/tools/art/hu_textures.py" "$SRC" "$h" "$DST/$h"
  [ -f "$SRC/$h.grips.json" ] && cp "$SRC/$h.grips.json" "$DST/$h/$h.grips.json.txt"
done
