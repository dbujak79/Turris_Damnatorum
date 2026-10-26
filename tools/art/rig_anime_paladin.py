"""Lokalne rigowanie modelu PoC na kopii szkieletu z Turris; oryginały pozostają bez zmian.

Blender -b --python tools/art/rig_anime_paladin.py
"""
import json
import math
from pathlib import Path

import bpy
from mathutils import Matrix, Vector

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/GothicAnimePoC/Warrior/warrior-v2-grip.blend"
RIG_SOURCE = ROOT / "Assets/TurrisDamnatorum/Models/Heroes/hero_warrior/hero_warrior.fbx"
OUT = ROOT / "ArtSource/GothicAnimePoC/Paladin"
GAME = ROOT / "Assets/TurrisDamnatorum/Models/PoC/PaladinAnime"
OUT.mkdir(parents=True, exist_ok=True)
GAME.mkdir(parents=True, exist_ok=True)


def active(obj):
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj


def smooth(a, b, value):
    t = max(0, min(1, (value - a) / (b - a)))
    return t * t * (3 - 2 * t)


bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
model = next(obj for obj in bpy.context.scene.objects if obj.type == "MESH")
model.name = "PaladynAnime"
model.data.name = "PaladynAnime"
before_import = set(bpy.context.scene.objects)
bpy.ops.import_scene.fbx(filepath=str(RIG_SOURCE))
imported = set(bpy.context.scene.objects) - before_import
arm = next(obj for obj in imported if obj.type == "ARMATURE")
for obj in imported:
    if obj != arm:
        bpy.data.objects.remove(obj, do_unlink=True)
arm.name = "Armature"
arm.animation_data_clear()
for pose_bone in arm.pose.bones:
    pose_bone.rotation_mode = "QUATERNION"
    pose_bone.rotation_quaternion = (1, 0, 0, 0)
    pose_bone.location = (0, 0, 0)
    pose_bone.scale = (1, 1, 1)
arm.data.transform(Matrix.Scale(1.8 / 1.85, 4) @ arm.matrix_world)
arm.matrix_world = Matrix.Identity(4)
arm.data.pose_position = "REST"

# Stawy dopasowane do siatki PoC w pozie wiązania. Zachowujemy nazwy i hierarchię istniejącego rigu.
targets = {
    "Hips": ((0, 0.025, 1.015), (0, 0.025, 1.15)),
    "Spine": ((0, 0.025, 1.13), (0, 0.025, 1.265)),
    "Spine1": ((0, 0.025, 1.265), (0, 0.025, 1.395)),
    "Spine2": ((0, 0.025, 1.395), (0, 0.025, 1.485)),
    "Neck": ((0, 0.025, 1.485), (0, 0.02, 1.555)),
    "Head": ((0, 0.02, 1.555), (0, 0.02, 1.755)),
    "HeadTop_End": ((0, 0.02, 1.755), (0, 0.02, 1.79)),
}
for side, sign in [("Left", 1), ("Right", -1)]:
    def mirrored(p):
        return (p[0] * sign, p[1], p[2])
    for suffix, head, tail in [
        ("Shoulder", (0.04, 0.035, 1.465), (0.215, 0.035, 1.445)),
        ("Arm", (0.215, 0.035, 1.445), (0.435, 0.04, 1.402)),
        ("ForeArm", (0.435, 0.04, 1.402), (0.665, 0.04, 1.37)),
        ("Hand", (0.665, 0.04, 1.37), (0.77, 0.035, 1.32)),
        ("UpLeg", (0.115, 0.025, 0.98), (0.15, -0.005, 0.54)),
        ("Leg", (0.15, -0.005, 0.54), (0.18, 0.025, 0.115)),
        ("Foot", (0.18, 0.025, 0.115), (0.195, -0.085, 0.045)),
        ("ToeBase", (0.195, -0.085, 0.045), (0.21, -0.18, 0.045)),
    ]:
        targets[side + suffix] = (mirrored(head), mirrored(tail))
active(arm)
bpy.ops.object.mode_set(mode="EDIT")
for name, (head, tail) in targets.items():
    bone = arm.data.edit_bones["mixamorig:" + name]
    bone.use_connect = False
    bone.head, bone.tail = head, tail
for sign, slot, parent in [(1, "HandSlot.L", "LeftHand"), (-1, "HandSlot.R", "RightHand")]:
    bone = arm.data.edit_bones[slot]
    bone.use_connect = False
    bone.head = (sign * 0.77, 0.035, 1.32)
    bone.tail = (sign * 0.81, 0.035, 1.30)
    bone.parent = arm.data.edit_bones["mixamorig:" + parent]
    bone.use_deform = False
if "headfront" in arm.data.edit_bones:
    bone = arm.data.edit_bones["headfront"]
    bone.head, bone.tail = (0, 0.02, 1.64), (0, -0.09, 1.64)
    bone.use_deform = False
arm.data.edit_bones["mixamorig:HeadTop_End"].use_deform = False
bpy.ops.object.mode_set(mode="OBJECT")
arm.data.pose_position = "POSE"
bpy.context.view_layer.update()

# Wielowarstwowy pancerz i szwy generatora nie dają stabilnego wyniku bone heat.
# Wagi są jawne, zależne od regionu anatomicznego; środek każdego stalowego segmentu pozostaje sztywny.
groups = {bone.name: model.vertex_groups.new(name=bone.name) for bone in arm.data.bones if bone.use_deform}
model.parent = arm
deform = model.modifiers.new("Szkielet paladyna", "ARMATURE")
deform.object = arm


def replace_weights(vertex, values):
    for membership in list(vertex.groups):
        model.vertex_groups[membership.group].remove([vertex.index])
    total = sum(values.values())
    for name, value in values.items():
        if value > 0.00001:
            groups["mixamorig:" + name].add([vertex.index], value / total, "REPLACE")


rigid_hands = rigid_feet = 0
for vertex in model.data.vertices:
    p = vertex.co
    x = abs(p.x)
    side = "Left" if p.x > 0 else "Right"
    if x > 0.18 and p.z > 1.2:
        if x < 0.29:
            t = smooth(0.18, 0.29, x)
            weights = {"Spine2": 1 - t, side + "Arm": t}
        elif x < 0.385:
            weights = {side + "Arm": 1}
        elif x < 0.485:
            t = smooth(0.385, 0.485, x)
            weights = {side + "Arm": 1 - t, side + "ForeArm": t}
        elif x < 0.63:
            weights = {side + "ForeArm": 1}
        elif x < 0.685:
            t = smooth(0.63, 0.685, x)
            weights = {side + "ForeArm": 1 - t, side + "Hand": t}
        else:
            weights = {side + "Hand": 1}
            rigid_hands += 1
    elif p.z > 1.54:
        weights = {"Head": 1}
    elif p.z > 1.48:
        t = smooth(1.48, 1.54, p.z)
        weights = {"Neck": 1 - t, "Head": t}
    elif p.z > 1.42:
        weights = {"Spine2": 1}
    elif p.z > 1.28:
        t = smooth(1.28, 1.42, p.z)
        weights = {"Spine1": 1 - t, "Spine2": t}
    elif p.z > 1.14:
        t = smooth(1.14, 1.28, p.z)
        weights = {"Spine": 1 - t, "Spine1": t}
    elif p.z > 1.03:
        t = smooth(1.03, 1.14, p.z)
        weights = {"Hips": 1 - t, "Spine": t}
    elif p.z > 0.98:
        weights = {"Hips": 1}
    elif 0.43 < p.z < 0.98 and ((p.y > 0.10 and x < 0.25) or (p.y < -0.10 and x < 0.13)):
        weights = {"Hips": 0.7, side + "UpLeg": 0.3}
    elif p.z > 0.87:
        t = smooth(0.87, 0.98, p.z)
        weights = {side + "UpLeg": 1 - t, "Hips": t}
    elif p.z > 0.615:
        weights = {side + "UpLeg": 1}
    elif p.z > 0.475:
        t = smooth(0.475, 0.615, p.z)
        weights = {side + "Leg": 1 - t, side + "UpLeg": t}
    elif p.z > 0.28:
        weights = {side + "Leg": 1}
    elif p.z > 0.205:
        t = smooth(0.205, 0.28, p.z)
        weights = {side + "Foot": 1 - t, side + "Leg": t}
    else:
        weights = {side + "Foot": 1}
        rigid_feet += 1
    replace_weights(vertex, weights)
unweighted = [vertex.index for vertex in model.data.vertices if not any(group.weight > 0 for group in vertex.groups)]
assert not unweighted, "Nieprzypisane wierzchołki: " + str(len(unweighted))

# Poza T do poprawnego awatara Humanoid: ramiona poziomo, nogi pionowo pod biodrami.
# Zostawienie szerokiej pozy generatora w bind pose przenosiło odwiedzenie nóg do retargetowanego biegu.
feet_rest = {side: arm.pose.bones["mixamorig:" + side + "Foot"].matrix.copy() for side in ["Left", "Right"]}
active(arm)
bpy.ops.object.mode_set(mode="POSE")
for side, sign in [("Left", 1), ("Right", -1)]:
    for suffix in ["Arm", "ForeArm", "Hand"]:
        bone = arm.pose.bones["mixamorig:" + side + suffix]
        bpy.context.view_layer.update()
        matrix = bone.matrix.copy()
        direction = (matrix.to_3x3() @ Vector((0, 1, 0))).normalized()
        rotation = direction.rotation_difference(Vector((sign, 0, 0))).to_matrix().to_4x4()
        new = rotation @ matrix
        new.translation = matrix.translation
        bone.matrix = new
for side in ["Left", "Right"]:
    for suffix in ["UpLeg", "Leg"]:
        bone = arm.pose.bones["mixamorig:" + side + suffix]
        bpy.context.view_layer.update()
        matrix = bone.matrix.copy()
        direction = (matrix.to_3x3() @ Vector((0, 1, 0))).normalized()
        rotation = direction.rotation_difference(Vector((0, 0, -1))).to_matrix().to_4x4()
        new = rotation @ matrix
        new.translation = matrix.translation
        bone.matrix = new
    bpy.context.view_layer.update()
    # Podeszwy pozostają płaskie po wyprostowaniu nóg, palce wskazują przód zamiast szerokiej pozy ekspozycyjnej.
    foot = arm.pose.bones["mixamorig:" + side + "Foot"]
    original = feet_rest[side]
    original_forward = (original.to_3x3() @ Vector((0, 1, 0))).normalized()
    yaw = math.atan2(original_forward.x, -original_forward.y)
    flat = Matrix.Rotation(-yaw, 4, "Z") @ original
    flat.translation = foot.matrix.translation.copy()
    foot.matrix = flat
bpy.context.view_layer.update()
bpy.ops.object.mode_set(mode="OBJECT")
active(model)
bake = model.modifiers.new("Wypieczenie pozy T", "ARMATURE")
bake.object = arm
bpy.ops.object.modifier_move_to_index(modifier=bake.name, index=0)
bpy.ops.object.modifier_apply(modifier=bake.name)
active(arm)
bpy.ops.object.mode_set(mode="POSE")
bpy.ops.pose.select_all(action="SELECT")
bpy.ops.pose.armature_apply(selected=False)
bpy.ops.object.mode_set(mode="OBJECT")
for bone in arm.pose.bones:
    bone.rotation_quaternion = (1, 0, 0, 0)
    bone.location = (0, 0, 0)
    bone.scale = (1, 1, 1)
for action in list(bpy.data.actions):
    bpy.data.actions.remove(action)
for image in bpy.data.images:
    if image.type == "IMAGE" and image.has_data:
        image.pack()
bpy.context.preferences.filepaths.save_version = 0
if "file_preview_type" in bpy.context.preferences.filepaths.bl_rna.properties:
    bpy.context.preferences.filepaths.file_preview_type = "NONE"
scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1
bpy.ops.wm.save_as_mainfile(filepath=str(OUT / "paladin_anime_rigged.blend"))
active(model)
arm.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(GAME / "paladin_anime.fbx"), use_selection=True,
                         object_types={"ARMATURE", "MESH"}, add_leaf_bones=False, bake_anim=False,
                         primary_bone_axis="Y", secondary_bone_axis="X", axis_forward="-Z", axis_up="Y",
                         apply_scale_options="FBX_SCALE_ALL", use_armature_deform_only=False,
                         path_mode="STRIP", embed_textures=False, mesh_smooth_type="FACE")
report = {"source": str(SOURCE.relative_to(ROOT)), "rig_source": str(RIG_SOURCE.relative_to(ROOT)),
          "output": str((GAME / "paladin_anime.fbx").relative_to(ROOT)), "bones": len(arm.data.bones),
          "vertices": len(model.data.vertices), "triangles": len(model.data.polygons),
          "unweighted": len(unweighted), "rigid_hand_vertices": rigid_hands,
          "rigid_boot_vertices": rigid_feet, "paid_operations": 0,
          "weight_method": "anatomiczne regiony, sztywne segmenty, płynne przejścia przy stawach",
          "slots": {name: list(arm.matrix_world @ arm.data.bones[name].head_local) for name in ["HandSlot.R", "HandSlot.L"]}}
(OUT / "rig-report.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
print("RIG_PALADYNA " + json.dumps(report, ensure_ascii=True), flush=True)
