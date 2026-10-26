"""Lokalny audyt i podgląd jednej opłaconej generacji; uruchamiany przez Blender -b --python."""
import json
from collections import Counter
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "ArtSource/GothicAnimePoC/Warrior"


def audit_import(format_name):
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    coords = [obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
    low = Vector(tuple(min(point[axis] for point in coords) for axis in range(3)))
    high = Vector(tuple(max(point[axis] for point in coords) for axis in range(3)))
    images = [image for image in bpy.data.images if image.type == "IMAGE"]
    triangles = 0
    for obj in meshes:
        obj.data.calc_loop_triangles()
        triangles += len(obj.data.loop_triangles)
    return {
        "format": format_name,
        "meshes": len(meshes),
        "vertices": sum(len(obj.data.vertices) for obj in meshes),
        "faces": sum(len(obj.data.polygons) for obj in meshes),
        "triangles": triangles,
        "face_sizes": dict(Counter(len(face.vertices) for obj in meshes for face in obj.data.polygons)),
        "materials": len({material for obj in meshes for material in obj.data.materials if material}),
        "uv_layers": [len(obj.data.uv_layers) for obj in meshes],
        "armatures": len([obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]),
        "actions": len(bpy.data.actions),
        "bounds_min": list(low),
        "bounds_max": list(high),
        "dimensions": list(high - low),
        "textures": [{"name": image.name, "width": image.size[0], "height": image.size[1],
                      "has_pixels": image.has_data, "packed": bool(image.packed_file)} for image in images],
    }, low, high


def aim(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def area(name, location, energy, size, color):
    light = bpy.data.lights.new(name, "AREA")
    light.energy = energy
    light.shape = "DISK"
    light.size = size
    light.color = color
    obj = bpy.data.objects.new(name, light)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    aim(obj, (0, 0, 1))


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(OUTPUT / "warrior-v1.glb"))
glb_audit, low, high = audit_import("glb")
report = {"task_id": "01a16a0a-a6e5-77cc-81be-c97226baf934", "credits_before": 159,
          "credits_after": 129, "credits_spent": 30, "imports": [glb_audit]}

# Tylko scena prezentacyjna ma wysokość 1,8 m. Oryginalne GLB/FBX pozostają bez zmian.
roots = [obj for obj in bpy.context.scene.objects if obj.parent is None]
presentation = bpy.data.objects.new("Skala prezentacji 1,8 m", None)
bpy.context.collection.objects.link(presentation)
for obj in roots:
    obj.parent = presentation
scale = 1.8 / (high.z - low.z)
presentation.scale = (scale,) * 3
presentation.location = (-(low.x + high.x) * 0.5 * scale,
                         -(low.y + high.y) * 0.5 * scale, -low.z * scale)
report["presentation_height_m"] = 1.8
report["presentation_scale"] = scale

bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -0.012))
floor = bpy.context.object
floor.name = "Podłoże podglądu"
material = bpy.data.materials.new("Neutralne podłoże")
material.use_nodes = True
shader = next(node for node in material.node_tree.nodes if node.type == "BSDF_PRINCIPLED")
shader.inputs["Base Color"].default_value = (0.1, 0.12, 0.16, 1)
shader.inputs["Roughness"].default_value = 0.85
floor.data.materials.append(material)

scene = bpy.context.scene
for engine in ("BLENDER_EEVEE_NEXT", "BLENDER_EEVEE", "CYCLES"):
    try:
        scene.render.engine = engine
        break
    except TypeError:
        pass
if scene.render.engine == "CYCLES":
    scene.cycles.samples = 32
scene.render.resolution_x = 1400
scene.render.resolution_y = 1400
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.film_transparent = False
scene.view_settings.view_transform = "AgX"
world = bpy.data.worlds.new("Neutralne studio")
world.use_nodes = True
background = next(node for node in world.node_tree.nodes if node.type == "BACKGROUND")
background.inputs["Color"].default_value = (0.32, 0.36, 0.45, 1)
background.inputs["Strength"].default_value = 0.45
scene.world = world
area("Światło główne", (-3, -4, 5), 450, 4, (1, 0.92, 0.84))
area("Wypełnienie", (4, -1, 3), 280, 4, (0.8, 0.88, 1))
area("Światło tylne", (0, 4, 4), 500, 3, (1, 0.95, 0.88))

camera_data = bpy.data.cameras.new("Kamera podglądu")
camera = bpy.data.objects.new("Kamera podglądu", camera_data)
bpy.context.collection.objects.link(camera)
scene.camera = camera
camera_data.type = "ORTHO"
camera_data.ortho_scale = 2.65
report["renders"] = []
for name, position in [("front", (0, -6, 1.4)),
                       ("three-quarter", (3.5, -6, 2.1)),
                       ("back", (0, 6, 1.4))]:
    camera.location = position
    aim(camera, (0, 0, 0.95))
    scene.render.filepath = str(OUTPUT / ("warrior-v1-" + name + ".png"))
    bpy.ops.render.render(write_still=True)
    report["renders"].append(Path(scene.render.filepath).name)

# Gotowy plik do otwarcia bez połączeń z Meshy lub zależności od zewnętrznych tekstur.
camera.location = (3.5, -6, 2.1)
aim(camera, (0, 0, 0.95))
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT / "warrior-v1-preview.blend"))

# Osobny import FBX sprawdza, czy drugi eksport też jest czytelny i ma teksturę.
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(OUTPUT / "warrior-v1.fbx"))
fbx_audit, _, _ = audit_import("fbx")
report["imports"].append(fbx_audit)
(OUTPUT / "audit.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
print("AUDYT_MODELU " + json.dumps(report, ensure_ascii=True), flush=True)
