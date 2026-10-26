"""Porównanie istniejącego szkieletu wojownika z modelem PoC, bez zmiany assetów gry."""
import json
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "ArtSource/GothicAnimePoC/Warrior"
SOURCE = ROOT / "Assets/TurrisDamnatorum/Models/Heroes/hero_warrior/hero_warrior.fbx"

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(SOURCE))
armatures = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
assert len(armatures) == 1, "Oczekiwany jeden szkielet istniejącego wojownika"
arm = armatures[0]
arm.data.pose_position = "REST"
bpy.context.view_layer.update()
scale = 1.8 / 1.85
bones = {bone.name: {"parent": bone.parent.name if bone.parent else None,
                     "head": list((arm.matrix_world @ bone.head_local) * scale),
                     "tail": list((arm.matrix_world @ bone.tail_local) * scale),
                     "deform": bone.use_deform} for bone in arm.data.bones}
required = ["Hips", "Spine", "Spine1", "Spine2", "Neck", "Head"]
for side in ["Left", "Right"]:
    required += [side + suffix for suffix in ["Shoulder", "Arm", "ForeArm", "Hand", "UpLeg", "Leg", "Foot", "ToeBase"]]
missing = [name for name in required if "mixamorig:" + name not in bones]
points = [obj.matrix_world @ vertex.co * scale for obj in bpy.context.scene.objects
          if obj.type == "MESH" for vertex in obj.data.vertices]
low = [min(point[axis] for point in points) for axis in range(3)]
high = [max(point[axis] for point in points) for axis in range(3)]
report = {"source": str(SOURCE.relative_to(ROOT)), "source_game_scale": scale,
          "armatures": len(armatures), "bone_count": len(bones), "missing_required_bones": missing,
          "hand_slots": {name: bones.get(name) for name in ["HandSlot.R", "HandSlot.L"]},
          "existing_mesh_bounds_at_game_scale": {"min": low, "max": high},
          "bones_at_game_scale": bones,
          "comparison": "Istniejący rig w skali gry; model PoC w wysokości prezentacyjnej 1,8 m. Bez dopasowania kości i wag."}
(OUTPUT / "rig-audit.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
print("SZKIELET " + json.dumps({key: value for key, value in report.items() if key != "bones_at_game_scale"}, ensure_ascii=True), flush=True)

# Nakładka pokazuje rzeczywiste położenie stawów starego modelu, bez pozorowania ukończonego rigowania.
bpy.ops.wm.open_mainfile(filepath=str(OUTPUT / "warrior-v1-preview.blend"))
scene = bpy.context.scene
scene.render.resolution_x = 1200
scene.render.resolution_y = 1200
material = bpy.data.materials.new("Diagnostyka szkieletu — turkus")
material.use_nodes = True
shader = next(node for node in material.node_tree.nodes if node.type == "BSDF_PRINCIPLED")
shader.inputs["Base Color"].default_value = (0, 0.8, 0.65, 1)
shader.inputs["Emission Color"].default_value = (0, 0.8, 0.65, 1)
shader.inputs["Emission Strength"].default_value = 1


def line(a, b):
    delta = b - a
    if delta.length < 0.0001:
        return
    bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=0.0035, depth=delta.length, location=(a + b) / 2)
    obj = bpy.context.object
    obj.rotation_euler = delta.to_track_quat("Z", "Y").to_euler()
    obj.data.materials.append(material)


for view, camera_position, front_axis, front_value in [
    ("front", (0, -6, 1.4), 1, -0.6), ("side", (6, 0, 1.4), 0, 1.2)
]:
    before = set(bpy.context.scene.objects)
    for name in ["mixamorig:" + name for name in required] + ["HandSlot.R", "HandSlot.L"]:
        if name not in bones:
            continue
        a = Vector(bones[name]["head"])
        b = Vector(bones[name]["tail"])
        a[front_axis] = b[front_axis] = front_value
        line(a, b)
        bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=4, radius=0.008, location=a)
        bpy.context.object.data.materials.append(material)
    scene.camera.location = camera_position
    scene.camera.rotation_euler = (Vector((0, 0, 0.95)) - scene.camera.location).to_track_quat("-Z", "Y").to_euler()
    scene.render.filepath = str(OUTPUT / ("rig-fit-" + view + ".png"))
    bpy.ops.render.render(write_still=True)
    for obj in set(bpy.context.scene.objects) - before:
        bpy.data.objects.remove(obj, do_unlink=True)
