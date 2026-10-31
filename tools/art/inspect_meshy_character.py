"""Dodatkowe detale postaci Meshy i kontrola map po ponownym otwarciu Blender."""
import argparse
import json
import sys
from pathlib import Path

import bpy
from mathutils import Vector

parser = argparse.ArgumentParser()
parser.add_argument('--blend', required=True)
parser.add_argument('--prefix', required=True)
parser.add_argument('--verify-only', action='store_true')
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
path = Path(args.blend).resolve()
bpy.ops.wm.open_mainfile(filepath=str(path))
maps = [image for image in bpy.data.images if image.source == 'FILE']
for image in maps:
    _ = image.pixels[0]
if len(maps) != 4 or not all(image.packed_file and image.has_data for image in maps):
    raise RuntimeError('Brak odczytu kompletu czterech spakowanych map.')
report = dict(source_blend=str(path), packed_maps_loaded=len(maps), geometry_modified=False,
              textures=[dict(name=image.name,size=list(image.size),packed=True) for image in maps], renders=[])
if not args.verify_only:
    scene = bpy.context.scene
    camera = scene.camera
    if camera is None:
        raise RuntimeError('Detale wymagają sceny podglądu z kamerą.')
    shots = [('face-beard', (0,-4,1.85),(0,0,1.74),.65),
             ('chest-shoulders',(0,-4,1.53),(0,0,1.45),1.3),
             ('belt-skirt',(0,-4,.95),(0,0,.85),1.1),
             ('feet-detail',(0,-4,.28),(0,0,.24),.8),
             ('back-fur',(0,4,1.55),(0,0,1.47),1.4)]
    for name, position, target, scale in shots:
        camera.location = position
        camera.rotation_euler = (Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler()
        camera.data.ortho_scale = scale
        scene.render.filepath = str(path.parent / (args.prefix+'-'+name+'.png'))
        bpy.ops.render.render(write_still=True)
        report['renders'].append(Path(scene.render.filepath).name)
(path.parent/(args.prefix+'-'+('packed-audit' if args.verify_only else 'detail-audit')+'.json')).write_text(
    json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('AUDYT_POSTACI '+json.dumps(report,ensure_ascii=True),flush=True)
