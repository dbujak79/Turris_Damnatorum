"""Odczyt istniejących rigów chochlików, bez zmiany assetów źródłowych."""
import json
from pathlib import Path

import bpy
from mathutils import Vector

root = Path(__file__).resolve().parents[2]
reports = []
for key in ("imp", "imp_water"):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.render.fps = 30
    bpy.ops.import_scene.fbx(filepath=str(root / f"Assets/TurrisDamnatorum/Models/Demons/{key}/{key}.fbx"))
    arm = next(obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE")
    arm.animation_data_clear()
    arm.data.pose_position = "REST"
    bpy.context.view_layer.update()
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    pts = [obj.matrix_world @ Vector(c) for obj in meshes for c in obj.bound_box]
    reports.append({"id": key, "armature": arm.name, "armature_matrix": [list(row) for row in arm.matrix_world],
                    "bounds_min": [min(p[i] for p in pts) for i in range(3)],
                    "bounds_max": [max(p[i] for p in pts) for i in range(3)],
                    "bones": [{"name": b.name, "parent": b.parent.name if b.parent else None,
                               "head": list(arm.matrix_world @ b.head_local), "tail": list(arm.matrix_world @ b.tail_local),
                               "matrix_local": [list(row) for row in b.matrix_local], "deform": b.use_deform}
                              for b in arm.data.bones],
                    "actions": [{"name": a.name, "frames": list(a.frame_range)} for a in bpy.data.actions]})
out = root / "ArtSource/GothicAnimePoC/FirstArenaEnemies/old-skeleton-audit.json"
out.write_text(json.dumps(reports, ensure_ascii=False, indent=2), encoding="utf-8")
for report in reports:
    print("RIG " + json.dumps({k: report[k] for k in ("id", "armature", "bounds_min", "bounds_max", "actions")}, ensure_ascii=True), flush=True)
