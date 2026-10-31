"""Zbliżenia nieredukowanych chochlików Meshy z gotowej sceny podglądowej."""
import argparse
import json
import sys
from pathlib import Path

import bpy
from mathutils import Vector

parser = argparse.ArgumentParser()
parser.add_argument("--preview", required=True)
parser.add_argument("--prefix", required=True)
parser.add_argument("--verify-only", action="store_true")
args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
preview = Path(args.preview).resolve()
bpy.ops.wm.open_mainfile(filepath=str(preview))
scene = bpy.context.scene
camera = scene.camera
maps = [image for image in bpy.data.images if image.source == "FILE"]
for image in maps:
    # Blender ładuje spakowane piksele leniwie; sam has_data tuż po open_mainfile nie wystarcza.
    _ = image.pixels[0]
if len(maps) != 4 or not all(image.packed_file and image.has_data for image in maps):
    raise RuntimeError("Niepełne spakowanie lub odczyt map po ponownym otwarciu sceny.")
if args.verify_only:
    print("SPAKOWANE_MAPY_OK " + str(preview) + " 4/4", flush=True)
    raise SystemExit(0)

shots = [("feet", (0, -4, .35), (0, 0, .3), 1.4),
         ("wing-roots", (0, 4, 1.4), (0, .2, 1.4), 1.4)]
for name, sign in [("left", -1), ("right", 1)]:
    target = Vector((sign * .94, -.07, 1.30))
    shots.append(("hand-" + name, target + Vector((0, -3, .15)), target, .65))
    shots.append(("hand-" + name + "-top", target + Vector((0, -.1, 3)), target, .65))

rendered = []
for name, position, target, framing in shots:
    camera.location = position
    camera.rotation_euler = (Vector(target) - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.ortho_scale = framing
    scene.render.filepath = str(preview.parent / (args.prefix + "-" + name + ".png"))
    bpy.ops.render.render(write_still=True)
    rendered.append(Path(scene.render.filepath).name)

report = {"source_preview": str(preview), "packed_maps_loaded": len(maps),
          "geometry_modified": False, "renders": rendered}
(preview.parent / (args.prefix + "-detail-audit.json")).write_text(
    json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
print("KONTROLA_DETALI " + json.dumps(report, ensure_ascii=True), flush=True)
