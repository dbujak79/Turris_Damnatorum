"""Konwersja bohatera z Hell Unleashed (.glb, szkielet Meshy) do FBX dla Unity (rig Humanoid).

Uruchomienie (Blender 5.2):
    blender -b --python tools/art/convert_hu_hero.py -- <wejście.glb> <wyjście.fbx>

Co robi:
- usuwa śmieci z eksportu Meshy (luźne siatki bez szkieletu, np. „Icosphere”);
- zmienia nazwy kości na nazewnictwo Mixamo (mixamorig:Hips…), które Unity mapuje na Humanoid automatycznie;
- ustawia pozę spoczynkową w T-pose (ręce poziomo, nogi pionowo) – Unity traktuje pozę spoczynkową jako
  T-pose awatara, a modele Meshy stoją w A-pose, więc bez tego klipy Mixamo opuszczałyby ręce o ~45°;
- przepieka klipy z pliku (Idle_A, Walking_A, Interact…) na nowy szkielet: każda kość kopiuje
  przekształcenie świata z kopii oryginału, więc ruch siatki jest identyczny jak w Hell Unleashed;
- eksportuje FBX bez tekstur (materiały składa Unity z plików obok, patrz TurrisSetup / HeroImport).
"""
import json
import os
import sys

import bpy
from mathutils import Vector

RENAME = {
    "root": "Hips", "Spine02": "Spine", "Spine01": "Spine1", "spine": "Spine2",
    "neck": "Neck", "head": "Head", "head_end": "HeadTop_End",
    "shoulder.L": "LeftShoulder", "upperarm.L": "LeftArm", "forearm.L": "LeftForeArm", "hand.L": "LeftHand",
    "shoulder.R": "RightShoulder", "upperarm.R": "RightArm", "forearm.R": "RightForeArm", "hand.R": "RightHand",
    "thigh.L": "LeftUpLeg", "shin.L": "LeftLeg", "foot.L": "LeftFoot", "toe.L": "LeftToeBase",
    "thigh.R": "RightUpLeg", "shin.R": "RightLeg", "foot.R": "RightFoot", "toe.R": "RightToeBase",
}

# Kierunek kości w T-pose (przestrzeń szkieletu; postać patrzy w −Y, lewa strona to +X).
TPOSE = {
    "upperarm.L": Vector((1, 0, 0)), "forearm.L": Vector((1, 0, 0)), "hand.L": Vector((1, 0, 0)),
    "upperarm.R": Vector((-1, 0, 0)), "forearm.R": Vector((-1, 0, 0)), "hand.R": Vector((-1, 0, 0)),
    "thigh.L": Vector((0, 0, -1)), "shin.L": Vector((0, 0, -1)),
    "thigh.R": Vector((0, 0, -1)), "shin.R": Vector((0, 0, -1)),
}


def args():
    a = sys.argv[sys.argv.index("--") + 1:]
    return a[0], a[1]


def set_active(obj):
    bpy.ops.object.mode_set(mode="OBJECT") if bpy.context.object and bpy.context.object.mode != "OBJECT" else None
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj


def main():
    src_path, out_path = args()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=src_path)

    arm = next(o for o in bpy.data.objects if o.type == "ARMATURE")
    for o in list(bpy.data.objects):
        if o.type == "MESH" and o.parent != arm:
            bpy.data.objects.remove(o, do_unlink=True)
    meshes = [o for o in bpy.data.objects if o.type == "MESH"]
    actions = [a for a in bpy.data.actions]
    scene = bpy.context.scene
    scene.render.fps = 30

    # Kopia oryginalnego szkieletu – źródło ruchu przy przepiekaniu klipów.
    src = arm.copy()
    src.data = arm.data.copy()
    src.name = "Source"
    scene.collection.objects.link(src)
    # Ścieżki NLA trzeba usunąć: zmiana nazwy kości przepisuje kanały we wszystkich klipach podpiętych
    # pod szkielet, a oryginalne klipy mają zostać przy starych nazwach dla kopii-źródła.
    for o in (src, arm):
        if o.animation_data:
            o.animation_data.action = None
            for t in list(o.animation_data.nla_tracks):
                o.animation_data.nla_tracks.remove(t)
    for a in actions:
        a.use_fake_user = True

    # Gniazda broni: kości HandSlot.R/L w środku pięści (model_centre z <model>.grips.json, układ glTF:
    # x, y w górę, z do przodu → Blender x, −z, y). Dodane przed T-pose, więc przesuwają się razem z dłonią.
    grips_path = os.path.splitext(src_path)[0] + ".grips.json"
    grips = json.load(open(grips_path, encoding="utf-8")) if os.path.exists(grips_path) else {}
    set_active(arm)
    bpy.ops.object.mode_set(mode="EDIT")
    for hand, slot in (("hand.R", "HandSlot.R"), ("hand.L", "HandSlot.L")):
        hb = arm.data.edit_bones[hand]
        if hand in grips:
            c = grips[hand]["model_centre"]
            head = Vector((c[0], -c[2], c[1]))
        else:
            head = hb.head + (hb.tail - hb.head).normalized() * 0.09
        b = arm.data.edit_bones.new(slot)
        b.head = head
        b.tail = head + (hb.tail - hb.head).normalized() * 0.05
        b.roll = hb.roll
        b.parent = hb
        b.use_deform = False
    bpy.ops.object.mode_set(mode="OBJECT")

    # T-pose: obrót kości od rodzica do dziecka, w przestrzeni szkieletu.
    set_active(arm)
    bpy.ops.object.mode_set(mode="POSE")
    for pb in arm.pose.bones:
        pb.rotation_mode = "QUATERNION"
        pb.rotation_quaternion = (1, 0, 0, 0)
        pb.location = (0, 0, 0)
        pb.scale = (1, 1, 1)
    bpy.context.view_layer.update()
    for name in ["upperarm.L", "forearm.L", "hand.L", "upperarm.R", "forearm.R", "hand.R",
                 "thigh.L", "shin.L", "thigh.R", "shin.R"]:
        pb = arm.pose.bones.get(name)
        if pb is None:
            continue
        bpy.context.view_layer.update()
        m = pb.matrix.copy()
        cur = (m.to_3x3() @ Vector((0, 1, 0))).normalized()
        rot = cur.rotation_difference(TPOSE[name]).to_matrix().to_4x4()
        head = m.translation.copy()
        nm = rot @ m
        nm.translation = head
        pb.matrix = nm
    bpy.context.view_layer.update()
    bpy.ops.object.mode_set(mode="OBJECT")

    # Wypieczenie pozy w siatkach i ustawienie jej jako spoczynkowej.
    for mesh in meshes:
        set_active(mesh)
        mod = next(m for m in mesh.modifiers if m.type == "ARMATURE")
        if mesh.data.shape_keys:
            bpy.ops.object.shape_key_remove(all=True)
        copy = mesh.modifiers.new("ArmatureBake", "ARMATURE")
        copy.object = arm
        bpy.ops.object.modifier_move_to_index(modifier=copy.name, index=0)
        bpy.ops.object.modifier_apply(modifier=copy.name)
        mod.object = arm
    set_active(arm)
    bpy.ops.object.mode_set(mode="POSE")
    bpy.ops.pose.select_all(action="SELECT")
    bpy.ops.pose.armature_apply(selected=False)
    bpy.ops.object.mode_set(mode="OBJECT")

    # Nazwy Mixamo – przed przepieczeniem, żeby nowe klipy od razu wskazywały nowe kości.
    for b in arm.data.bones:
        if b.name in RENAME:
            b.name = "mixamorig:" + RENAME[b.name]
    original = {"mixamorig:" + v: k for k, v in RENAME.items()}

    # Przepieczenie klipów: kość kopiuje przekształcenie świata z oryginału.
    for pb in arm.pose.bones:
        if pb.name.startswith("HandSlot"):
            continue
        c = pb.constraints.new("COPY_TRANSFORMS")
        c.target = src
        c.subtarget = original.get(pb.name, pb.name)
    baked = []
    for act in actions:
        src.animation_data_create()
        src.animation_data.action = act
        if hasattr(src.animation_data, "action_slot") and act.slots:
            src.animation_data.action_slot = act.slots[0]
        f0, f1 = int(act.frame_range[0]), int(act.frame_range[1])
        set_active(arm)
        bpy.ops.object.mode_set(mode="POSE")
        bpy.ops.pose.select_all(action="SELECT")
        if arm.animation_data:
            arm.animation_data.action = None
        bpy.ops.nla.bake(frame_start=f0, frame_end=f1, only_selected=False, visual_keying=True,
                         clear_constraints=False, use_current_action=False, bake_types={"POSE"})
        bpy.ops.object.mode_set(mode="OBJECT")
        new = arm.animation_data.action
        name = act.name
        act.name = name + "_src"
        new.name = name
        new.use_fake_user = True
        baked.append(new)
        arm.animation_data.action = None
    for pb in arm.pose.bones:
        for c in list(pb.constraints):
            pb.constraints.remove(c)
    bpy.data.objects.remove(src, do_unlink=True)
    for act in actions:
        bpy.data.actions.remove(act)

    # Pozycja spoczynkowa na czas eksportu.
    for pb in arm.pose.bones:
        pb.rotation_quaternion = (1, 0, 0, 0)
        pb.location = (0, 0, 0)
    # Dwa pliki: model w czystym T-pose (z niego Unity buduje awatar – eksporter FBX z animacjami zapisuje jako
    # pozę domyślną klatkę klipu, przez co awatar dostawał pozę bloku) oraz same klipy (<nazwa>_clips.fbx),
    # które w Unity korzystają z awatara modelu.
    common = dict(use_selection=False, object_types={"ARMATURE", "MESH"}, add_leaf_bones=False,
                  primary_bone_axis="Y", secondary_bone_axis="X", axis_forward="-Z", axis_up="Y",
                  apply_scale_options="FBX_SCALE_ALL", bake_space_transform=False, use_armature_deform_only=False,
                  path_mode="STRIP", embed_textures=False, mesh_smooth_type="FACE")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(filepath=out_path, bake_anim=False, **common)
    clips_path = os.path.splitext(out_path)[0] + "_clips.fbx"
    bpy.ops.export_scene.fbx(filepath=clips_path, bake_anim=True, bake_anim_use_all_actions=True,
                             bake_anim_use_nla_strips=False, bake_anim_force_startend_keying=True,
                             bake_anim_simplify_factor=0.0, **common)
    print("ZAPISANO", out_path, "klipy:", [a.name for a in baked])


main()
