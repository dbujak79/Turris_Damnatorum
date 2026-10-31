"""Pełne siatki Meshy na kopiach istniejących rigów; bez zmiany oryginałów i redukcji.

Blender -b --python tools/art/rig_anime_imps.py -- fire|water
"""
import argparse
import json
import sys
from pathlib import Path

import bpy
import numpy as np
from mathutils import Matrix, Vector

parser = argparse.ArgumentParser()
parser.add_argument("kind", choices=("fire", "water"))
args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "ArtSource/GothicAnimePoC/FirstArenaEnemies"
key = "imp" if args.kind == "fire" else "imp_water"
height = 1.3 if args.kind == "fire" else 1.43
out = ART / "rigged"
game = ROOT / "Assets/TurrisDamnatorum/Models/PoC/AnimeImps"
out.mkdir(exist_ok=True)
game.mkdir(parents=True, exist_ok=True)
source = ART / f"{args.kind}-meshy-v1/imp-{args.kind}-v1.blend"
bpy.ops.wm.open_mainfile(filepath=str(source))
model = next(o for o in bpy.context.scene.objects if o.type == "MESH")
model.data.transform(model.matrix_world)
model.matrix_world = Matrix.Identity(4)
co = np.empty(len(model.data.vertices)*3, dtype=np.float32)
model.data.vertices.foreach_get("co", co)
co = co.reshape(-1,3).astype(np.float64)
low, high = co.min(axis=0), co.max(axis=0)
co[:,2] -= low[2]
co[:,0] -= (low[0]+high[0])*.5
co *= height / (high[2]-low[2])
p = co / height
model.name = "ChochlikAnime_"+args.kind

before = set(bpy.context.scene.objects)
bpy.ops.import_scene.fbx(filepath=str(ROOT / f"Assets/TurrisDamnatorum/Models/Demons/{key}/{key}.fbx"))
imported = set(bpy.context.scene.objects)-before
arm = next(o for o in imported if o.type == "ARMATURE")
for obj in imported:
    if obj != arm:
        bpy.data.objects.remove(obj, do_unlink=True)
arm.animation_data_clear()
for action in list(bpy.data.actions):
    bpy.data.actions.remove(action)
arm.data.transform(arm.matrix_world)
arm.matrix_world = Matrix.Identity(4)
arm.data.pose_position = "REST"
old = {b.name:b.matrix_local.copy() for b in arm.data.bones}
old_heads = {b.name:b.head_local.copy() for b in arm.data.bones}
old_tails = {b.name:b.tail_local.copy() for b in arm.data.bones}
body_y = -.045 if args.kind == "fire" else -.105
arm_y = -.12 if args.kind == "fire" else -.16
leg_y = -.08 if args.kind == "fire" else -.145

def point(x,y,z):
    return Vector((x*height,y*height,z*height))

canonical = {"root":point(0,body_y,.44), "spine":point(0,body_y,.665 if args.kind=="fire" else .59),
             "neck":point(0,body_y-.035,.715), "head":point(0,body_y-.06,.765)}
if args.kind == "fire":
    canonical.update({"Spine02":point(0,body_y,.51), "Spine01":point(0,body_y,.58),
                      "head_end":point(0,body_y-.065,.78), "headfront":point(0,body_y-.16,.78)})
else:
    canonical["neutral_bone"] = Vector((0,0,0))
for side,sign in [("L",1),("R",-1)]:
    for name,x,y,z in [("shoulder",.03,arm_y,.69), ("upperarm",.145,arm_y,.69),
                       ("forearm",.285,arm_y,.678), ("hand",.415,arm_y,.65),
                       ("thigh",.075,leg_y+.025,.43), ("shin",.11,leg_y-.025,.255),
                       ("foot",.12,leg_y+.015,.065), ("toe",.14,leg_y-.08,.03)]:
        if name+"."+side in old:
            canonical[name+"."+side] = point(sign*x,y,z)
    if args.kind == "water":
        canonical["wing."+side] = point(sign*.075,-.015,.69)
        canonical["wingtip."+side] = point(sign*.235,.055,.91)

# Trzy segmenty istniejącego ogona wodnego; u ognia dodatkowy łańcuch na kopii rigu.
tail_points = [point(0,.025,.44), point(-.025,.19,.26), point(-.18,.22,.105), point(-.275,.17,.32)]
if args.kind == "fire":
    bpy.context.view_layer.objects.active = arm
    arm.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    for i in range(3):
        bone = arm.data.edit_bones.new("tail"+str(i+1))
        bone.head, bone.tail = tail_points[i], tail_points[i+1]
        bone.parent = arm.data.edit_bones["root" if i==0 else "tail"+str(i)]
    bpy.ops.object.mode_set(mode="OBJECT")
    for bone in arm.data.bones:
        if bone.name not in old:
            old[bone.name]=bone.matrix_local.copy()
            old_heads[bone.name]=bone.head_local.copy()
            old_tails[bone.name]=bone.tail_local.copy()
for i in range(3):
    canonical["tail"+str(i+1)] = tail_points[i]

desired = {name:v.copy() for name,v in canonical.items()}
canonical_rotation = {name:m.to_quaternion() for name,m in old.items()}
# Korekta otwartej pozy generatora do pozy starego rigu. Końcowe osie kości pozostają źródłowe.
for side in ("L","R"):
    for chain in [("upperarm","forearm","hand"),("thigh","shin","foot"),("wing","wingtip")]:
        if chain[0]+"."+side not in canonical:
            continue
        for parent,child in zip(chain,chain[1:]):
            a,b = parent+"."+side,child+"."+side
            direction = (old_heads[b]-old_heads[a]).normalized()
            segment = canonical[b]-canonical[a]
            canonical_rotation[a] = direction.rotation_difference(segment.normalized()) @ old[a].to_quaternion()
            desired[b] = desired[a] + direction*segment.length
        last = chain[-1]+"."+side
        # Dłonie i stopy zachowują zwartą geometrię, bez rozciągania pojedynczych pazurów.
        canonical_rotation[last] = canonical_rotation[chain[-2]+"."+side] if chain[-1]=="hand" else old[last].to_quaternion()

names = [b.name for b in arm.data.bones]
index = {name:i for i,name in enumerate(names)}
weights = np.zeros((len(co),len(names)),dtype=np.float32)
tail = (p[:,1]>.045)&(p[:,2]<.48)
wing = np.zeros(len(co),dtype=bool)
if args.kind == "water":
    wing = (p[:,1]>-.015)&(abs(p[:,0])>.105)&(p[:,2]>.36)
    tail &= ~wing
head = (((p[:,2]>.80)&(abs(p[:,0])<.28))|((p[:,2]>.725)&(abs(p[:,0])<.15)&(p[:,1]<(-.12 if args.kind=="fire" else -.17))))&~wing
arms = (abs(p[:,0])>.13)&(p[:,2]>.54)&~head&~wing&~tail
legs = (p[:,2]<.43)&(abs(p[:,0])>.025)&~tail&~wing
body = ~(tail|wing|head|arms|legs)

def blend(mask, a, b, t):
    t=np.clip(t,0,1)
    weights[mask,index[a]]=1-t[mask]
    weights[mask,index[b]]=t[mask]

weights[head,index["head"]]=1
centers = [("root",.44),("Spine02",.51),("Spine01",.58),("spine",.665),("neck",.715),("head",.765)] if args.kind=="fire" else [("root",.44),("spine",.59),("neck",.715),("head",.765)]
weights[body,index["root"]]=1
for (a,za),(b,zb) in zip(centers,centers[1:]):
    mask=body&(p[:,2]>=za)&(p[:,2]<zb)
    weights[mask]=0
    blend(mask,a,b,(p[:,2]-za)/(zb-za))

# Ciągłe pola anatomiczne zastępują ostre progi, które na gęstej siatce rozciągają pojedyncze kolce.
def smooth_field(a,b,v):
    t=np.clip((v-a)/(b-a),0,1)
    return t*t*(3-2*t)

body_weights=np.zeros_like(weights)
body_weights[:,index["root"]]=1
for (a,za),(b,zb) in zip(centers,centers[1:]):
    mask=(p[:,2]>=za)&(p[:,2]<zb)
    body_weights[mask]=0
    t=np.clip((p[mask,2]-za)/(zb-za),0,1)
    body_weights[mask,index[a]]=1-t;body_weights[mask,index[b]]=t
body_weights[p[:,2]>=centers[-1][1]]=0
body_weights[p[:,2]>=centers[-1][1],index["head"]]=1
weights[:]=body_weights
arm_weights=np.zeros_like(weights)
leg_weights=np.zeros_like(weights)
for side,sign in [("L",1),("R",-1)]:
    mask=(p[:,0]*sign>0)
    x=abs(p[:,0])
    a=mask&(x<.255); b=mask&(x>=.255)&(x<.305); c=mask&(x>=.305)&(x<.39); d=mask&(x>=.39)
    arm_weights[a,index["upperarm."+side]]=1
    t=np.clip((x-.255)/.05,0,1)
    arm_weights[b,index["upperarm."+side]]=1-t[b];arm_weights[b,index["forearm."+side]]=t[b]
    arm_weights[c,index["forearm."+side]]=1
    t=np.clip((x-.39)/.04,0,1)
    arm_weights[d,index["forearm."+side]]=1-t[d];arm_weights[d,index["hand."+side]]=t[d]
    z=p[:,2]
    a=mask&(z>.285); b=mask&(z<=.285)&(z>.225); c=mask&(z<=.225)&(z>.10); d=mask&(z<=.10)
    leg_weights[a,index["thigh."+side]]=1
    t=np.clip((z-.225)/.06,0,1)
    leg_weights[b,index["shin."+side]]=1-t[b];leg_weights[b,index["thigh."+side]]=t[b]
    leg_weights[c,index["shin."+side]]=1
    t=np.clip((z-.065)/.035,0,1)
    leg_weights[d,index["foot."+side]]=1-t[d];leg_weights[d,index["shin."+side]]=t[d]
arm_mix=smooth_field(.105,.23,abs(p[:,0]))*smooth_field(.50,.57,p[:,2])
arm_mix[head]=0
arm_support=body_weights.copy()
shoulder_zone=(abs(p[:,0])>.105)&(p[:,2]<.79)&~head
arm_support[shoulder_zone]=0;arm_support[shoulder_zone,index["spine"]]=1
weights=weights*(1-arm_mix[:,None])+arm_weights*arm_mix[:,None]
# Klatka przechodzi w bark po szerokim pasie; nie przypinamy kolców barku do głowy.
support_mix=(1-arm_mix)*smooth_field(.10,.16,abs(p[:,0]))*smooth_field(.52,.60,p[:,2])*(1-smooth_field(.75,.81,p[:,2]))
support_mix[head]=0
weights=weights*(1-support_mix[:,None])+arm_support*support_mix[:,None]
leg_mix=(1-smooth_field(.35,.47,p[:,2]))*smooth_field(.012,.07,abs(p[:,0]))
weights=weights*(1-leg_mix[:,None])+leg_weights*leg_mix[:,None]
wing_weights=np.zeros_like(weights)
for side,sign in [("L",1),("R",-1)]:
    if args.kind=="water":
        mask=p[:,0]*sign>0
        t=smooth_field(.16,.32,abs(p[:,0]))
        wing_weights[mask,index["wing."+side]]=1-t[mask]
        wing_weights[mask,index["wingtip."+side]]=t[mask]
if args.kind=="water":
    wing_mix=smooth_field(-.045,.025,p[:,1])*smooth_field(.06,.16,abs(p[:,0]))*smooth_field(.28,.38,p[:,2])
    weights=weights*(1-wing_mix[:,None])+wing_weights*wing_mix[:,None]

# Najbliższy odcinek krzywej ogona daje ciągłe wagi także dla zakrzywionej końcówki.
distances=[]
for i in range(3):
    a,b=np.array(tail_points[i]),np.array(tail_points[i+1]); delta=b-a
    t=np.clip(((co-a)@delta)/(delta@delta),0,1)
    distances.append(np.linalg.norm(co-(a+t[:,None]*delta),axis=1))
ds=np.stack(distances,axis=1)
tw=1/np.maximum(ds,.008*height)**4
tw/=tw.sum(axis=1,keepdims=True)
tail_weights=np.zeros_like(weights)
for i in range(3):
    tail_weights[:,index["tail"+str(i+1)]]=tw[:,i]
tail_mix=smooth_field(.025,.105,p[:,1])*(1-smooth_field(.40,.50,p[:,2]))
if args.kind=="water":
    tail_mix*=1-smooth_field(.34,.45,p[:,2])*smooth_field(.10,.25,abs(p[:,0]))
weights=weights*(1-tail_mix[:,None])+tail_weights*tail_mix[:,None]

# Łagodne przejścia po rzeczywistych krawędziach nie mieszają bliskiej dłoni ze skrzydłem za nią.
edge_array=np.empty(len(model.data.edges)*2,dtype=np.int32)
model.data.edges.foreach_get("vertices",edge_array)
edges=edge_array.reshape(-1,2)
degree=np.bincount(edge_array,minlength=len(co)).clip(1)
seed=weights.copy()
for iteration in range(4):
    averaged=np.empty_like(weights)
    for j in range(len(names)):
        averaged[:,j]=(np.bincount(edges[:,0],weights=weights[edges[:,1],j],minlength=len(co))
                       +np.bincount(edges[:,1],weights=weights[edges[:,0],j],minlength=len(co)))/degree
    weights=seed*.35+averaged*.65
strong=np.argpartition(weights,-4,axis=1)[:,-4:]
keep=np.zeros_like(weights,dtype=bool)
np.put_along_axis(keep,strong,True,axis=1)
weights[~keep]=0
# Pełna precyzja wag: gruba kwantyzacja tworzy widoczne uskoki na gęstej siatce przy stawach.
weights[weights<.00001]=0
sums=weights.sum(axis=1)
assert np.all(sums>0), "Wierzchołki bez przypisania"
weights/=sums[:,None]
assert np.max(np.count_nonzero(weights,axis=1))<=4
posed=np.zeros_like(co)
matrices={}
for name,i in index.items():
    can=Matrix.LocRotScale(canonical[name],canonical_rotation[name],Vector((1,1,1)))
    dst=old[name].copy();dst.translation=desired[name]
    matrix=dst@can.inverted()
    matrices[name]=dst
    rows=np.flatnonzero(weights[:,i]>0)
    if len(rows):
        posed[rows]+=(co[rows]@np.array(matrix.to_3x3()).T+np.array(matrix.translation))*weights[rows,i,None]
ground=-float(posed[:,2].min())
posed[:,2]+=ground
model.data.vertices.foreach_set("co",posed.astype(np.float32).ravel())
model.data.update()
bpy.context.view_layer.objects.active=arm
bpy.ops.object.mode_set(mode="EDIT")
for bone in arm.data.edit_bones:
    bone.use_connect=False
    matrix=matrices[bone.name].copy();matrix.translation.z+=ground
    bone.matrix=matrix
bpy.ops.object.mode_set(mode="OBJECT")
arm.data.pose_position="POSE"
for b in arm.pose.bones:
    b.matrix_basis=Matrix.Identity(4)
for name,i in index.items():
    group=model.vertex_groups.new(name=name)
    values=weights[:,i]
    rigid=np.flatnonzero(values>.99999)
    if len(rigid):
        group.add(rigid.tolist(),1.0,"REPLACE")
    for row in np.flatnonzero((values>0)&(values<=.99999)):
        group.add([int(row)],float(values[row]),"REPLACE")
model.parent=arm
modifier=model.modifiers.new("Istniejący szkielet chochlika","ARMATURE")
modifier.object=arm
used_images={node.image for mat in model.data.materials if mat and mat.use_nodes
             for node in mat.node_tree.nodes if node.type=="TEX_IMAGE" and node.image}
for image in list(bpy.data.images):
    if image in used_images:
        _=image.pixels[0]
        image.pack()
    elif image.source=="FILE":
        bpy.data.images.remove(image)
bpy.context.preferences.filepaths.save_version=0
bpy.context.scene.render.fps=30
bpy.ops.wm.save_as_mainfile(filepath=str(out/f"imp-{args.kind}-rigged.blend"))
bpy.ops.object.select_all(action="DESELECT")
arm.select_set(True);model.select_set(True)
bpy.context.view_layer.objects.active=arm
fbx=game/f"imp_{args.kind}_anime.fbx"
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={"ARMATURE","MESH"},
                        add_leaf_bones=False,primary_bone_axis="Y",secondary_bone_axis="X",
                        axis_forward="-Z",axis_up="Y",apply_scale_options="FBX_SCALE_ALL",
                        bake_space_transform=False,use_armature_deform_only=False,
                        path_mode="STRIP",embed_textures=False,mesh_smooth_type="FACE",bake_anim=False)
report={"kind":args.kind,"source":str(source),"rig_source":f"Models/Demons/{key}/{key}.fbx",
        "vertices":len(model.data.vertices),"triangles":len(model.data.polygons),"bones":len(names),
        "unweighted":int(np.count_nonzero(sums==0)),"max_influences":int(np.max(np.count_nonzero(weights,axis=1))),
        "geometry_reduced":False,"ground_offset":ground,"height":height,"fbx":str(fbx),
        "regions":{name:int(np.count_nonzero(mask)) for name,mask in [("body",body),("head",head),("arms",arms),("legs",legs),("wings",wing),("tail",tail)]}}
(out/f"imp-{args.kind}-rig-report.json").write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
print("RIG_GOTOWY "+json.dumps(report,ensure_ascii=True),flush=True)
