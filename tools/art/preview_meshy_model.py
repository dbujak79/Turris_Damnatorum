"""Audyt nieredukowanego FBX Meshy i lokalne rendery geometrii z teksturami."""
import argparse
import json
import math
import sys
from collections import Counter
from pathlib import Path

import bpy
import numpy as np
from mathutils import Vector


parser = argparse.ArgumentParser()
parser.add_argument("--source", required=True)
parser.add_argument("--output", required=True)
parser.add_argument("--prefix", default="model-v1")
parser.add_argument("--task-id", required=True)
parser.add_argument("--profile", choices=("character", "demon"), default="character")
args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
source = Path(args.source).resolve()
output = Path(args.output).resolve()
output.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
bpy.ops.import_scene.fbx(filepath=str(source))
meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
if not meshes:
    raise RuntimeError("FBX nie zawiera siatek.")

# Pobieranie może nadawać mapom nazwy z prefiksem; dopasowanie zachowuje obrazy źródłowe.
files = [p for p in output.rglob("*") if p.suffix.lower() in (".png", ".jpg", ".jpeg")]
for image in bpy.data.images:
    if image.source != "FILE":
        continue
    original = Path(bpy.path.abspath(image.filepath))
    candidates = [p for p in files if p.name.lower() == original.name.lower()]
    if not candidates and not original.is_file():
        kind = (image.name + " " + image.filepath).lower()
        keys = next((keys for keys in [
            ("normal",), ("roughness",), ("metallic", "metalness"), ("base_color", "basecolor", "albedo")
        ] if any(key in kind for key in keys)), None)
        if keys:
            candidates = [p for p in files if any(key in p.name.lower() for key in keys)]
        elif original.name.lower() == "texture_0.png":
            candidates = [p for p in files if p.name.lower() == "base_color.png"]
    if candidates and (not original.is_file() or not image.has_data):
        if image.packed_file:
            image.unpack(method="REMOVE")
        image.filepath = str(candidates[0])
        image.reload()

points = [obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
low = Vector(tuple(min(p[i] for p in points) for i in range(3)))
high = Vector(tuple(max(p[i] for p in points) for i in range(3)))
triangles = 0
bad_vertices = 0
degenerate = 0
for obj in meshes:
    obj.data.calc_loop_triangles()
    triangles += len(obj.data.loop_triangles)
    bad_vertices += sum(not all(math.isfinite(c) for c in v.co) for v in obj.data.vertices)
    degenerate += sum(face.area < 1e-12 for face in obj.data.polygons)
materials = list({mat for obj in meshes for mat in obj.data.materials if mat})
report = {
    "task_id": args.task_id, "source": str(source), "geometry_modified": False,
    "meshes": len(meshes), "vertices": sum(len(obj.data.vertices) for obj in meshes),
    "faces": sum(len(obj.data.polygons) for obj in meshes), "triangles": triangles,
    "face_sizes": dict(Counter(len(face.vertices) for obj in meshes for face in obj.data.polygons)),
    "nonfinite_vertices": bad_vertices, "degenerate_faces": degenerate,
    "uv_layers": [len(obj.data.uv_layers) for obj in meshes],
    "materials": len(materials), "armatures": len([o for o in bpy.data.objects if o.type == "ARMATURE"]),
    "bounds_min": list(low), "bounds_max": list(high), "dimensions": list(high - low),
    "textures": [{"name": image.name, "path": image.filepath, "size": list(image.size),
                  "has_pixels": image.has_data} for image in bpy.data.images if image.source == "FILE"],
    "renders": [],
}
world_vertices = []
for obj in meshes:
    vertices = np.empty(len(obj.data.vertices) * 3, dtype=np.float32)
    obj.data.vertices.foreach_get("co", vertices)
    world_vertices.append(vertices.reshape(-1, 3) @ np.array(obj.matrix_world.to_3x3()).T
                          + np.array(obj.matrix_world.translation))
world_vertices = np.concatenate(world_vertices)
if bad_vertices:
    raise RuntimeError("Niefinitywne współrzędne wierzchołków.")
if any(not image["has_pixels"] for image in report["textures"]):
    raise RuntimeError("Brakuje map tekstur; popraw ścieżki przed renderowaniem.")
bpy.ops.file.pack_all()
# Osobny, spakowany plik źródłowy bez lamp i podłoża prezentacyjnego.
bpy.ops.wm.save_as_mainfile(filepath=str(output / (args.prefix + ".blend")))

roots = [obj for obj in bpy.context.scene.objects if obj.parent is None]
presentation = bpy.data.objects.new("Wyłącznie skala podglądu 2 m", None)
bpy.context.collection.objects.link(presentation)
for obj in roots:
    obj.parent = presentation
scale = 2.0 / (high.z - low.z)
presentation.scale = (scale,) * 3
presentation.location = (-(low.x + high.x) * .5 * scale, -(low.y + high.y) * .5 * scale, -low.z * scale)
report["presentation_height_m"] = 2
report["presentation_scale"] = scale

def aim(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def area(name, position, energy, color):
    data = bpy.data.lights.new(name, "AREA")
    data.energy, data.shape, data.size, data.color = energy, "DISK", 4, color
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    obj.location = position
    aim(obj, (0, 0, 1))


scene = bpy.context.scene
scene.render.engine = "CYCLES"
scene.cycles.samples = 24
scene.cycles.use_denoising = True
scene.render.resolution_x, scene.render.resolution_y = 1200, 1400
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.view_settings.view_transform = "AgX"
scene.world = bpy.data.worlds.new("Neutralne studio")
scene.world.use_nodes = True
background = scene.world.node_tree.nodes.get("Background")
background.inputs["Color"].default_value = (.25, .28, .34, 1)
background.inputs["Strength"].default_value = .5
area("Główne", (-3, -4, 5), 500, (1, .95, .9))
area("Wypełnienie", (4, -1, 3), 350, (.86, .92, 1))
area("Kontur", (0, 4, 4), 450, (1, .96, .9))
bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -.01))
floor = bpy.context.object
floor.name = "Podłoże tylko do podglądu"
floor_mat = bpy.data.materials.new("Podłoże")
floor_mat.diffuse_color = (.12, .14, .18, 1)
floor.data.materials.append(floor_mat)
camera_data = bpy.data.cameras.new("Kamera")
camera = bpy.data.objects.new("Kamera", camera_data)
bpy.context.collection.objects.link(camera)
scene.camera = camera
camera_data.type = "ORTHO"
width = (high.x - low.x) * scale
depth = (high.y - low.y) * scale
shots = [
    ("front", (0, -6, 1.25), (0, 0, 1), max(2.5, width * 1.3)),
    ("three-quarter", (3.5, -6, 2), (0, 0, 1), max(2.5, width * 1.3)),
    ("back", (0, 6, 1.25), (0, 0, 1), max(2.5, width * 1.3)),
    ("left", (-6, 0, 1.25), (0, 0, 1), 2.5),
    ("right", (6, 0, 1.25), (0, 0, 1), 2.5),
    ("head", (0, -4, 1.75), (0, 0, 1.63), .85),
    ("robe-book", (-.45, -4, 1.12), (-.25, 0, .85), .95),
]
if args.profile == "demon":
    # Skrzydła nie są dłońmi; skrajne wierzchołki X nie wskazują tu palców.
    shots = shots[:5]
    shots[3] = ("left", (-6, 0, 1.25), (0, 0, 1), max(2.5, depth * 1.3))
    shots[4] = ("right", (6, 0, 1.25), (0, 0, 1), max(2.5, depth * 1.3))
    shots.extend([
        ("head", (0, -4, 1.68), (0, 0, 1.63), 1.0),
        ("torso", (0, -4, 1.2), (0, 0, 1.2), 1.15),
        ("feet", (0, -4, .35), (0, 0, .30), 1.05),
        ("back-detail", (0, 4, 1.3), (0, 0, 1.25), max(1.6, width * 1.3)),
    ])
for name, side in ([] if args.profile == "demon" else [("left", -1), ("right", 1)]):
    edge = low.x + (high.x - low.x) * .10 if side < 0 else high.x - (high.x - low.x) * .10
    selected = world_vertices[world_vertices[:, 0] < edge if side < 0 else world_vertices[:, 0] > edge]
    center = Vector(((selected.min(axis=0) + selected.max(axis=0)) * .5).tolist()) * scale + presentation.location
    shots.append(("hand-" + name, center + Vector((0, -3, .15)), center, .32))
    shots.append(("hand-" + name + "-top", center + Vector((0, -.1, 3)), center, .32))
for name, position, target, framing in shots:
    camera.location = position
    camera_data.ortho_scale = framing
    aim(camera, target)
    scene.render.filepath = str(output / (args.prefix + "-" + name + ".png"))
    bpy.ops.render.render(write_still=True)
    report["renders"].append(Path(scene.render.filepath).name)
camera.location = (3.5, -6, 2)
camera_data.ortho_scale = max(2.5, width * 1.3)
aim(camera, (0, 0, 1))
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=str(output / (args.prefix + "-preview.blend")))
(output / (args.prefix + "-audit.json")).write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
print("AUDYT_MODELU " + json.dumps(report, ensure_ascii=True), flush=True)
