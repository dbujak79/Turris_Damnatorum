"""Konwersja broni z Hell Unleashed (.glb) do FBX dla Unity + tekstury według roli.

Uruchomienie (Blender 5.2):
    blender -b --python tools/art/convert_hu_weapon.py -- <wejście.glb> <folder_wyjściowy>

Zapisuje <nazwa>.fbx oraz tekstury <nazwa>_albedo.png, _mr.png (glTF: G = roughness, B = metallic),
_normal.png i _emission.png (jeśli są). Materiał Standard składa z nich edytor Unity (HeroAssetImport).
Konwencja broni zostaje z Hell Unleashed: początek w środku rękojeści, ostrze wzdłuż +Y (Unity),
krawędzie tnące na ±X. Tarcza: lico w +Y, dół w −Z.
"""
import os
import sys

import bpy


def args():
    a = sys.argv[sys.argv.index("--") + 1:]
    return a[0], a[1]


def save_image(img, path):
    # Obrazy z .glb są spakowane – zapis wprost do pliku (bez przekształceń koloru).
    img.save(filepath=os.path.abspath(path))


def main():
    src, out_dir = args()
    name = os.path.splitext(os.path.basename(src))[0]
    os.makedirs(out_dir, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=src)
    # Pomocnicza kula z eksportu Meshy (2 m) – nie jest częścią broni.
    for o in list(bpy.data.objects):
        if o.name.startswith("Icosphere"):
            bpy.data.objects.remove(o, do_unlink=True)

    done = set()
    for mat in bpy.data.materials:
        if not mat.node_tree:
            continue
        for n in mat.node_tree.nodes:
            if n.type != "TEX_IMAGE" or n.image is None:
                continue
            targets = [l.to_node.type + ":" + l.to_socket.name for l in n.outputs[0].links]
            role = None
            for t in targets:
                if "Base Color" in t:
                    role = "albedo"
                elif t.startswith("SEPARATE_COLOR") or t.startswith("SEPRGB") or "Roughness" in t or "Metallic" in t:
                    role = "mr"
                elif t.startswith("NORMAL_MAP"):
                    role = "normal"
                elif "Emission" in t:
                    role = "emission"
            if role and role not in done:
                save_image(n.image, os.path.join(out_dir, f"{name}_{role}.png"))
                done.add(role)

    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(out_dir, name + ".fbx"), use_selection=False,
        object_types={"ARMATURE", "MESH", "EMPTY"}, add_leaf_bones=False,
        axis_forward="-Z", axis_up="Y", apply_scale_options="FBX_SCALE_ALL", bake_space_transform=True,
        bake_anim=False, path_mode="STRIP", embed_textures=False, mesh_smooth_type="FACE")
    print("ZAPISANO", name, sorted(done))


main()
