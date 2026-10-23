"""Konwersja demona z Hell Unleashed (.glb ze szkieletem i klipami) do FBX dla Unity (rig Generic).

Uruchomienie (Blender 5.2):
    blender -b --python tools/art/convert_hu_demon.py -- <wejście.glb> <wyjście.fbx>

Demony mają różne szkielety (dwunożne Meshy, chochliki żywiołów ze skrzydłami i ogonem, czerw, nietoperz),
więc nie przechodzą na Humanoid: klipy z HU (Idle, Walk, Attack, Special, Death, Latch…) idą do jednego pliku
bez zmian – ruch siatki jest identyczny jak w Hell Unleashed. Tekstury: tools/art/hu_demon_textures.py.
"""
import sys

import bpy


def args():
    a = sys.argv[sys.argv.index("--") + 1:]
    return a[0], a[1]


def main():
    src_path, out_path = args()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    # Klatki klipów liczone z kl./s sceny w chwili importu – 30 jak w eksporcie, inaczej klipy grają o 25% za szybko.
    bpy.context.scene.render.fps = 30
    bpy.ops.import_scene.gltf(filepath=src_path)

    arm = next(o for o in bpy.data.objects if o.type == "ARMATURE")
    # Śmieci z eksportu (luźne siatki bez szkieletu).
    for o in list(bpy.data.objects):
        if o.type == "MESH" and o.parent != arm:
            bpy.data.objects.remove(o, do_unlink=True)
    # Klipy jako osobne ujęcia FBX: bez ścieżek NLA i bez aktywnej akcji (inaczej pierwszy klip trafia do pozy domyślnej).
    for a in bpy.data.actions:
        a.use_fake_user = True
    if arm.animation_data:
        arm.animation_data.action = None
        for t in list(arm.animation_data.nla_tracks):
            arm.animation_data.nla_tracks.remove(t)

    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(filepath=out_path, use_selection=False, object_types={"ARMATURE", "MESH"}, add_leaf_bones=False,
                             primary_bone_axis="Y", secondary_bone_axis="X", axis_forward="-Z", axis_up="Y",
                             apply_scale_options="FBX_SCALE_ALL", bake_space_transform=False, use_armature_deform_only=False,
                             path_mode="STRIP", embed_textures=False, mesh_smooth_type="FACE",
                             bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                             bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0)
    print("ZAPISANO", out_path, "klipy:", [a.name for a in bpy.data.actions])


main()
