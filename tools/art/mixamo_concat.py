"""Sklejenie kilku klipów Mixamo (FBX) w jeden klip, np. strzał z łuku = naciągnięcie + odrzut.

Uruchomienie (Blender 5.2):
    blender -b --python tools/art/mixamo_concat.py -- <wyjście.fbx> <wejście1.fbx>[:od-do] <wejście2.fbx>[:od-do] ...
Zakresy klatek jak w Blenderze (od 1). Kolejne fragmenty są doklejane jeden za drugim (30 kl./s).
"""
import sys

import bpy


def parse(spec):
    if ":" in spec[3:]:
        path, rng = spec.rsplit(":", 1)
        a, b = rng.split("-")
        return path, int(a), int(b)
    return spec, None, None


def main():
    a = sys.argv[sys.argv.index("--") + 1:]
    out, parts = a[0], [parse(p) for p in a[1:]]
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.render.fps = 30

    target = None
    sources = []
    for path, f0, f1 in parts:
        before = set(bpy.data.objects)
        bpy.ops.import_scene.fbx(filepath=path)
        arm = next(o for o in bpy.data.objects if o not in before and o.type == "ARMATURE")
        act = arm.animation_data.action
        if f0 is None:
            f0, f1 = int(act.frame_range[0]), int(act.frame_range[1])
        sources.append((arm, f0, f1))
        if target is None:
            target = arm

    # Klatka po klatce: poza kości z kolejnych źródeł kopiowana do celu i kluczowana.
    frame = 1
    keyed = []
    for arm, f0, f1 in sources:
        for f in range(f0, f1 + 1):
            scene.frame_set(f)
            pose = {pb.name: (pb.location.copy(), pb.rotation_quaternion.copy()) for pb in arm.pose.bones}
            keyed.append((frame, pose))
            frame += 1
    target.animation_data.action = None
    for o in list(bpy.data.objects):
        if o is not target and (o.type == "ARMATURE" or o.parent in [s[0] for s in sources if s[0] is not target]):
            bpy.data.objects.remove(o, do_unlink=True)
    for f, pose in keyed:
        for pb in target.pose.bones:
            if pb.name not in pose:
                continue
            pb.rotation_mode = "QUATERNION"
            pb.location, pb.rotation_quaternion = pose[pb.name]
            pb.keyframe_insert("location", frame=f)
            pb.keyframe_insert("rotation_quaternion", frame=f)
    target.animation_data.action.name = "mixamo.com"
    scene.frame_start, scene.frame_end = 1, frame - 1
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(filepath=out, use_selection=False, object_types={"ARMATURE", "MESH"},
                             add_leaf_bones=False, bake_anim=True, bake_anim_use_all_actions=False,
                             bake_anim_use_nla_strips=False, bake_anim_simplify_factor=0.0)
    print("ZAPISANO", out, "klatek:", frame - 1)


main()
