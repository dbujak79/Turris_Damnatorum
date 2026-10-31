"""Przekroje nowych siatek potrzebne do lokalnego dopasowania stawów."""
import json
from pathlib import Path
import bpy
import numpy as np

root = Path(__file__).resolve().parents[2]
for key in ("fire", "water"):
    path = root / f"ArtSource/GothicAnimePoC/FirstArenaEnemies/{key}-meshy-v1/imp-{key}-v1.blend"
    bpy.ops.wm.open_mainfile(filepath=str(path))
    obj = next(o for o in bpy.context.scene.objects if o.type == "MESH")
    co = np.empty(len(obj.data.vertices)*3, dtype=np.float32)
    obj.data.vertices.foreach_get("co", co)
    co = co.reshape(-1,3) @ np.array(obj.matrix_world.to_3x3()).T + np.array(obj.matrix_world.translation)
    co[:,2] -= co[:,2].min()
    co /= co[:,2].max()
    print(key, flush=True)
    for name, mask in [
        ("tułów", (abs(co[:,0])<.08)&(co[:,2]>.48)&(co[:,2]<.68)),
        ("ręce", (abs(co[:,0])>.40)&(co[:,2]>.58)&(co[:,2]<.75)&(co[:,1]<0)),
        ("głowa", (abs(co[:,0])<.15)&(co[:,2]>.75)),
        ("kolana", (abs(co[:,0])<.18)&(co[:,2]>.23)&(co[:,2]<.27)),
        ("stopy", (co[:,2]<.07)&(co[:,1]<0)),
    ]:
        p=co[mask]
        if not len(p):
            continue
        print(name, json.dumps({"q10":np.quantile(p,.1,axis=0).tolist(),"q50":np.median(p,axis=0).tolist(),"q90":np.quantile(p,.9,axis=0).tolist()}), flush=True)
