"""Stały chwyt obu dłoni PoC; lokalna edycja siatki, bez rigowania i wywołań Meshy."""
import bisect
import json
import math
from pathlib import Path

import bpy
from mathutils import Matrix, Vector

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "ArtSource/GothicAnimePoC/Warrior"
DIAGNOSTICS = ROOT / "TestResults/gothic-anime-warrior/hands"
DIAGNOSTICS.mkdir(parents=True, exist_ok=True)


def smooth(a, b, value):
    t = max(0, min(1, (value - a) / (b - a)))
    return t * t * (3 - 2 * t)


def interpolate(value, keys, values):
    index = max(0, min(len(keys) - 2, bisect.bisect_right(keys, value) - 1))
    t = max(0, min(1, (value - keys[index]) / (keys[index + 1] - keys[index])))
    return values[index] * (1 - t) + values[index + 1] * t


def curl(point):
    sign = 1 if point.x > 0 else -1
    p = Vector((abs(point.x), point.y, point.z))
    if p.x < 0.685 or p.z < 1.2:
        return point.copy()
    direction = Vector((1, 0, -0.19)).normalized()
    normal = Vector((0.19, 0, 1)).normalized()
    knuckle_x = interpolate(p.y, [-0.02, 0.025, 0.065, 0.095], [0.779, 0.78, 0.774, 0.765])
    knuckle = Vector((knuckle_x, 0, 1.351 - 0.19 * (knuckle_x - 0.775)))
    relative = p - knuckle
    u, n = relative.dot(direction), relative.dot(normal)

    # Wspólne sklepienie chwytu; różna długość palców daje promienie odpowiadające ich anatomii.
    radius = interpolate(p.y, [-0.02, 0.025, 0.065, 0.095], [0.028, 0.0345, 0.032, 0.032])
    finger = p.copy()
    if u > 0:
        angle = u / radius
        finger = knuckle + direction * ((radius + n) * math.sin(angle))
        finger += normal * ((radius + n) * math.cos(angle) - radius)
        finger.y = p.y + (0.033 - p.y) * 0.12 * smooth(0, 0.08, u)

    # Kciuk leży poniżej płaszczyzny pozostałych palców. Zwijamy go osobnym łukiem na zewnątrz chwytu.
    base = Vector((0.705, 0.004, 1.315))
    original_axis = Vector((0.078, -0.049, -0.061))
    length = original_axis.length
    tangent = original_axis.normalized()
    along = (p - base).dot(tangent)
    t = max(0, min(1, along / length))
    b0, b1 = base, Vector((0.735, -0.024, 1.288))
    b2, b3 = Vector((0.794, -0.024, 1.282)), Vector((0.797, 0.025, 1.311))
    center = (1 - t) ** 3 * b0 + 3 * (1 - t) ** 2 * t * b1 + 3 * (1 - t) * t ** 2 * b2 + t ** 3 * b3
    new_tangent = (3 * (1 - t) ** 2 * (b1 - b0) + 6 * (1 - t) * t * (b2 - b1) + 3 * t ** 2 * (b3 - b2)).normalized()
    residual = p - (base + tangent * along)
    thumb = center + tangent.rotation_difference(new_tangent) @ residual
    if along > length:
        thumb += new_tangent * (along - length)
    thumb_weight = smooth(0.018, 0.038, -n) * (1 - smooth(0.005, 0.028, p.y)) * smooth(0, 0.035, along)
    changed = finger.lerp(thumb, thumb_weight)
    changed.x *= sign
    return changed


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(OUT / "warrior-v1.glb"))
meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
points = [obj.matrix_world @ vertex.co for obj in meshes for vertex in obj.data.vertices]
low = Vector(tuple(min(p[axis] for p in points) for axis in range(3)))
high = Vector(tuple(max(p[axis] for p in points) for axis in range(3)))
scale = 1.8 / (high.z - low.z)
transform = Matrix.Translation(Vector((-(low.x + high.x) * 0.5, -(low.y + high.y) * 0.5, -low.z)) * scale) @ Matrix.Scale(scale, 4)
counts = {"left": 0, "right": 0}
maximum = 0
for obj in meshes:
    obj.data.transform(transform @ obj.matrix_world)
    obj.parent = None
    obj.matrix_world = Matrix.Identity(4)
    mesh = obj.data
    normals = [tuple(item.vector) for item in mesh.corner_normals]
    changed_indices = set()
    for vertex in mesh.vertices:
        original = vertex.co.copy()
        vertex.co = curl(original)
        distance = (vertex.co - original).length
        if distance > 0.000001:
            changed_indices.add(vertex.index)
            counts["left" if original.x > 0 else "right"] += 1
            maximum = max(maximum, distance)
    # Zmodyfikowane dłonie dostają normalne wynikające z nowej geometrii; reszta zachowuje oryginalne normalne.
    for loop in mesh.loops:
        if loop.vertex_index in changed_indices:
            normals[loop.index] = (0, 0, 0)
    for polygon in mesh.polygons:
        if all(index in changed_indices for index in polygon.vertices):
            polygon.use_smooth = True
    mesh.normals_split_custom_set(normals)
    mesh.update()

for obj in list(bpy.context.scene.objects):
    if obj not in meshes:
        bpy.data.objects.remove(obj, do_unlink=True)
# Jawna mapa koloru uniezależnia eksport od tymczasowego katalogu tekstur importera FBX.
color_image = bpy.data.images.load(str(OUT / "warrior-v1_base_color.png"), check_existing=True)
color_image.pack()
for material in bpy.data.materials:
    if material.node_tree:
        for node in material.node_tree.nodes:
            if node.type == "TEX_IMAGE":
                node.image = color_image
for image in list(bpy.data.images):
    if image.type == "IMAGE" and image != color_image and image.users == 0:
        bpy.data.images.remove(image)
scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1
bpy.ops.object.select_all(action="DESELECT")
for obj in meshes:
    obj.select_set(True)
bpy.context.view_layer.objects.active = meshes[0]
bpy.ops.file.pack_all()
bpy.context.preferences.filepaths.save_version = 0
if "file_preview_type" in bpy.context.preferences.filepaths.bl_rna.properties:
    bpy.context.preferences.filepaths.file_preview_type = "NONE"
bpy.ops.wm.save_as_mainfile(filepath=str(OUT / "warrior-v2-grip.blend"))
bpy.ops.export_scene.fbx(filepath=str(OUT / "warrior-v2-grip.fbx"), use_selection=True,
                         object_types={"MESH"}, add_leaf_bones=False, bake_anim=False,
                         path_mode="COPY", embed_textures=True, axis_forward="-Z", axis_up="Y")
bpy.ops.export_scene.gltf(filepath=str(OUT / "warrior-v2-grip.glb"), export_format="GLB", use_selection=True)
report = {"source": "warrior-v1.glb", "output": "warrior-v2-grip", "height_m": 1.8,
          "changed_vertices": counts, "max_vertex_displacement_m": maximum,
          "vertices": sum(len(obj.data.vertices) for obj in meshes),
          "faces": sum(len(obj.data.polygons) for obj in meshes),
          "armatures": 0, "paid_operations": 0,
          "description": "Palce zwinięte w stały chwyt; nowy eksport nadal wymaga szkieletu i wag."}
(OUT / "grip-edit.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
print("CHWYT " + json.dumps(report, ensure_ascii=True), flush=True)

# Oświetlenie bierzemy z istniejącej sceny podglądu, a renderujemy nowy eksport GLB.
bpy.ops.wm.open_mainfile(filepath=str(OUT / "warrior-v1-preview.blend"))
for obj in list(bpy.context.scene.objects):
    if obj.type == "MESH" and obj.name != "Podłoże podglądu":
        bpy.data.objects.remove(obj, do_unlink=True)
bpy.ops.import_scene.gltf(filepath=str(OUT / "warrior-v2-grip.glb"))
scene = bpy.context.scene
scene.render.resolution_x = scene.render.resolution_y = 1400
scene.camera.data.ortho_scale = 2.65
scene.camera.location = (3.5, -6, 2.1)
scene.camera.rotation_euler = (Vector((0, 0, 0.95)) - scene.camera.location).to_track_quat("-Z", "Y").to_euler()
scene.render.filepath = str(OUT / "warrior-v2-grip-preview.png")
bpy.ops.render.render(write_still=True)
# Walce istnieją wyłącznie w zbliżeniach: pokazują prześwit na rękojeść o średnicy 25 mm.
handle_material = bpy.data.materials.new("Próbna rękojeść 25 mm — tylko podgląd")
handle_material.use_nodes = True
handle_shader = next(node for node in handle_material.node_tree.nodes if node.type == "BSDF_PRINCIPLED")
handle_shader.inputs["Base Color"].default_value = (0.095, 0.045, 0.018, 1)
handle_shader.inputs["Roughness"].default_value = 0.75
for sign in [-1, 1]:
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=0.0125, depth=0.17,
                                      location=(sign * 0.77, 0.035, 1.32), rotation=(math.pi / 2, 0, 0))
    bpy.context.object.data.materials.append(handle_material)
scene.render.resolution_x = scene.render.resolution_y = 1100
scene.camera.data.ortho_scale = 0.285
for hand, sign in [("left", 1), ("right", -1)]:
    target = Vector((sign * 0.765, 0.025, 1.325))
    for view, position in [("top", (sign * 0.765, -0.12, 3)),
                           ("front", (sign * 0.9, -2, 1.5)),
                           ("palm", (sign * 0.8, -0.12, 0.6))]:
        scene.camera.location = position
        scene.camera.rotation_euler = (target - scene.camera.location).to_track_quat("-Z", "Y").to_euler()
        scene.render.filepath = str(DIAGNOSTICS / ("grip-" + hand + "-" + view + ".png"))
        bpy.ops.render.render(write_still=True)
        if view == "front":
            bpy.data.images["Render Result"].save_render(str(OUT / ("warrior-v2-grip-" + hand + "-hand.png")), scene=scene)
