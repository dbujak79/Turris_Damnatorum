#!/usr/bin/env bash
# Konwersja demonów z pierwszych trzech aren Hell Unleashed do Assets/TurrisDamnatorum/Models/Demons/<nazwa>/.
# Użycie: bash tools/art/convert_hu_demons.sh ["C:/GIT/Hell Unleashed"]
set -e
HU="${1:-C:/GIT/Hell Unleashed}"
SRC="$HU/src/Game/Assets/Models/Demons"
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
DST="$ROOT/Assets/TurrisDamnatorum/Models/Demons"
BLENDER="${BLENDER:-C:/Program Files/Blender Foundation/Blender 5.2/blender.exe}"
DEMONS="imp imp_air imp_earth imp_water cultist worm gatelord minion"
for d in $DEMONS; do
  mkdir -p "$DST/$d"
  "$BLENDER" -b --python "$ROOT/tools/art/convert_hu_demon.py" -- "$SRC/$d.glb" "$DST/$d/$d.fbx" 2>&1 | grep -E "ZAPISANO|Error" || true
  python "$ROOT/tools/art/hu_demon_textures.py" "$SRC/$d.glb" "$d" "$DST/$d"
  [ -f "$SRC/$d.beats.json" ] && cp "$SRC/$d.beats.json" "$DST/$d/$d.beats.json.txt"
done
