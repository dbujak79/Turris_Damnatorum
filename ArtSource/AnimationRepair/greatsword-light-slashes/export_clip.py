"""Eksport naprawionego klipu do FBX zgodnego z importem Unity (klipy Mixamo, awatar z własnego pliku).

- Poza domyślna = poza spoczynkowa (T): eksporter Blendera zapisuje bieżącą klatkę jako pozę domyślną
  (pułapka z docs/PROGRESS.md) – dlatego klucz pozy T na klatce −10 i eksport z tej klatki.
- Klatki animacji przesunięte 1..N → 0..N−1 (jak w oryginale, czas FBX od 0).
- Ujęcie (AnimStack) nazwane jak w oryginale: „mixamo.com” (nazwa sceny eksportu).
Po eksporcie: ponowny import i porównanie z obiektem źródłowym (pozycje/obroty wszystkich kości w każdej klatce).
"""
import bpy, math, os
from bpy_extras import anim_utils


def export_clip(rig_name, out_path, take="mixamo.com", obj_name="Armature"):
    sc0 = bpy.context.window.scene
    src = bpy.data.objects[rig_name]
    act0 = src.animation_data.action
    f0, f1 = int(act0.frame_range[0]), int(act0.frame_range[1])
    sc = bpy.data.scenes.get(take) or bpy.data.scenes.new(take)
    for o in list(sc.collection.objects):
        sc.collection.objects.unlink(o)
    sc.render.fps = sc0.render.fps; sc.render.fps_base = sc0.render.fps_base
    exp = src.copy(); exp.data = src.data.copy(); exp.name = obj_name
    for c in list(exp.children):
        pass
    sc.collection.objects.link(exp)
    act = act0.copy(); act.name = take
    exp.animation_data.action = act
    exp.animation_data.action_slot = act.slots[0]
    cb = anim_utils.action_get_channelbag_for_slot(act, act.slots[0])
    for fc in cb.fcurves:
        for k in fc.keyframe_points:
            k.co.x -= f0; k.handle_left.x -= f0; k.handle_right.x -= f0
    for pb in exp.pose.bones:
        pb.rotation_mode = 'QUATERNION'
        pb.location = (0, 0, 0); pb.rotation_quaternion = (1, 0, 0, 0); pb.scale = (1, 1, 1)
        for path in ("location", "rotation_quaternion", "scale"):
            pb.keyframe_insert(path, frame=-10)
    for fc in cb.fcurves:   # klucz T nie może zmieniać interpolacji klatek 0..N
        for k in fc.keyframe_points:
            k.interpolation = 'LINEAR'
    win = bpy.context.window
    win.scene = sc
    try:
        sc.frame_start, sc.frame_end = 0, f1 - f0
        sc.frame_set(-10)
        bpy.context.view_layer.objects.active = exp
        for o in sc.objects: o.select_set(o == exp)
        with bpy.context.temp_override(window=win, scene=sc, active_object=exp, selected_objects=[exp]):
            bpy.ops.export_scene.fbx(filepath=out_path, use_selection=True, object_types={'ARMATURE'}, add_leaf_bones=False,
                primary_bone_axis='Y', secondary_bone_axis='X', armature_nodetype='NULL', apply_unit_scale=True,
                apply_scale_options='FBX_SCALE_NONE', bake_anim=True, bake_anim_use_all_bones=True,
                bake_anim_use_nla_strips=False, bake_anim_use_all_actions=False, bake_anim_force_startend_keying=True,
                bake_anim_step=1.0, bake_anim_simplify_factor=0.0, axis_forward='-Z', axis_up='Y')
    finally:
        win.scene = sc0
        bpy.data.objects.remove(exp)
        bpy.data.actions.remove(act)
        bpy.data.scenes.remove(sc)
    return verify_export(rig_name, out_path)


def verify_export(rig_name, path):
    sc = bpy.context.scene
    src = bpy.data.objects[rig_name]
    before = set(bpy.data.objects)
    with bpy.context.temp_override(window=bpy.context.window_manager.windows[0]):
        bpy.ops.import_scene.fbx(filepath=path, automatic_bone_orientation=False, ignore_leaf_bones=False)
    new = [o for o in bpy.data.objects if o not in before]
    rt = next(o for o in new if o.type == 'ARMATURE')
    ra = rt.animation_data.action
    a0 = src.animation_data.action
    off = ra.frame_range[0] - a0.frame_range[0]
    rest = max(((src.matrix_world @ b.matrix_local).translation - (rt.matrix_world @ rt.data.bones[b.name].matrix_local).translation).length
               for b in src.data.bones)
    maxp = maxr = 0.0
    for f in range(int(a0.frame_range[0]), int(a0.frame_range[1]) + 1):
        sc.frame_set(f); P1 = {b.name: src.matrix_world @ b.matrix for b in src.pose.bones}
        sc.frame_set(int(f + off))
        for b in rt.pose.bones:
            m2 = rt.matrix_world @ b.matrix; m1 = P1[b.name]
            maxp = max(maxp, (m1.translation - m2.translation).length)
            q = m1.to_quaternion().rotation_difference(m2.to_quaternion())
            maxr = max(maxr, math.degrees(min(q.angle, 2 * math.pi - q.angle)))
    info = dict(bones=len(rt.data.bones), range=tuple(ra.frame_range), rest_diff_m=rest, anim_pos_m=maxp, anim_rot_deg=maxr,
                take=ra.name, objects=[o.name for o in new])
    for o in new:
        bpy.data.objects.remove(o)
    bpy.data.actions.remove(ra)   # bez pozostałości – kolejny import nie dostaje nazwy „.001”
    sc.frame_set(1)
    return info
