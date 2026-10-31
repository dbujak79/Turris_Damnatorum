"""Berserker lub --mage na kopii dotychczasowego rigu Humanoid, bez zmiany źródeł."""
import json
import math
import sys
from pathlib import Path

import bpy
import numpy as np
from mathutils import Matrix, Vector

ROOT = Path(__file__).resolve().parents[2]
MAGE = '--mage' in sys.argv
KEY = 'mage' if MAGE else 'berserker'
OUT = ROOT/('ArtSource/GothicAnimePoC/Mage/rigged' if MAGE else 'ArtSource/GothicAnimePoC/Heroes/Berserker/rigged')
GAME = ROOT/('Assets/TurrisDamnatorum/Models/PoC/MageAnime' if MAGE else 'Assets/TurrisDamnatorum/Models/PoC/BerserkerAnime')
SOURCE = OUT.parent/('meshy-v1/mage-v1.blend' if MAGE else 'meshy-v1/berserker-v1.blend')
OLD = ROOT/('Assets/TurrisDamnatorum/Models/Heroes/mage_witch/mage_witch.fbx' if MAGE else 'Assets/TurrisDamnatorum/Models/Heroes/hero_warrior_berserker/hero_warrior_berserker.fbx')
OUT.mkdir(parents=True,exist_ok=True)
GAME.mkdir(parents=True,exist_ok=True)

def active(obj):
    if bpy.context.object and bpy.context.object.mode != 'OBJECT':
        bpy.ops.object.mode_set(mode='OBJECT')
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active=obj

def smooth(a,b,x):
    t=np.clip((x-a)/(b-a),0,1)
    return t*t*(3-2*t)

bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
model=next(o for o in bpy.context.scene.objects if o.type=='MESH')
model.data.transform(model.matrix_world)
model.matrix_world=Matrix.Identity(4)
coords=np.empty(len(model.data.vertices)*3,dtype=np.float32)
model.data.vertices.foreach_get('co',coords)
points=coords.reshape(-1,3)
low,high=points.min(axis=0),points.max(axis=0)
points[:,0]-=(low[0]+high[0])*.5
points[:,1]-=(low[1]+high[1])*.5
points[:,2]-=low[2]
height=2.05 if MAGE else 1.8
points*=height/(high[2]-low[2])
model.data.vertices.foreach_set('co',points.ravel())
model.name=model.data.name='BerserkerAnime'
before=set(bpy.context.scene.objects)
bpy.ops.import_scene.fbx(filepath=str(OLD))
imported=set(bpy.context.scene.objects)-before
arm=next(o for o in imported if o.type=='ARMATURE')
for obj in imported:
    if obj!=arm:
        bpy.data.objects.remove(obj,do_unlink=True)
arm.name='Armature'
arm.animation_data_clear()
arm.data.transform(arm.matrix_world)
arm.matrix_world=Matrix.Identity(4)
arm.data.pose_position='REST'
original_names=[b.name for b in arm.data.bones]

targets={
    'Hips':((0,.015,.94),(0,.015,1.05)),
    'Spine':((0,.015,1.05),(0,.015,1.18)),
    'Spine1':((0,.015,1.18),(0,.015,1.31)),
    'Spine2':((0,.015,1.31),(0,.015,1.43)),
    'Neck':((0,.015,1.43),(0,.005,1.52)),
    'Head':((0,.005,1.52),(0,.005,1.75)),
    'HeadTop_End':((0,.005,1.75),(0,.005,1.8))}
for side,sign in [('Left',1),('Right',-1)]:
    def mirror(p):return (sign*p[0],p[1],p[2])
    for name,a,b in [
        ('Shoulder',(.045,.01,1.425),(.25,.01,1.405)),
        ('Arm',(.25,.01,1.405),(.405,-.005,1.28)),
        ('ForeArm',(.405,-.005,1.28),(.535,-.025,1.175)),
        ('Hand',(.535,-.025,1.175),(.605,-.035,1.115)),
        ('UpLeg',(.115,.015,.94),(.125,-.005,.52)),
        ('Leg',(.125,-.005,.52),(.135,.015,.12)),
        ('Foot',(.135,.015,.12),(.135,-.095,.055)),
        ('ToeBase',(.135,-.095,.055),(.135,-.21,.055))]:
        targets[side+name]=(mirror(a),mirror(b))
    # Rzeczywiste przekroje modelu wyznaczają płaszczyznę łokcia i nadgarstka.
    if MAGE:
        joint_points=[(.20,1.56),(.43,1.50),(.64,1.46),(.73,1.43)]
        for suffix,(a,b) in zip(['Arm','ForeArm','Hand'],zip(joint_points,joint_points[1:])):
            targets[side+suffix]=((sign*a[0],-.025,a[1]),(sign*b[0],-.025,b[1]))
        targets[side+'Shoulder']=((sign*.04,.015,1.58),(sign*.20,-.025,1.56))
    else:
        joints=[]
        for cx,cz in [(.25,1.405),(.405,1.28),(.535,1.175),(.605,1.115)]:
            section=points[(points[:,0]*sign>cx-.014)&(points[:,0]*sign<cx+.014)&(points[:,2]>cz-.10)&(points[:,2]<cz+.10)]
            median=np.median(section,axis=0)
            joints.append((sign*cx,float(median[1]),float(median[2])))
        joints[0]=(sign*.25,.015,1.405)
        for suffix,pair in zip(['Arm','ForeArm','Hand'],zip(joints,joints[1:])):
            targets[side+suffix]=pair
        targets[side+'Shoulder']=((sign*.045,.015,1.425),joints[0])
# Wspólny środek stawów obu stron zapobiega wpływowi futra i naramiennika na długość ręki.
for suffix in ['Arm','ForeArm','Hand']:
    left,right=targets['Left'+suffix],targets['Right'+suffix]
    for joint in range(2):
        a,b=left[joint],right[joint]
        for side,sign in [('Left',1),('Right',-1)]:
            pair=list(targets[side+suffix])
            pair[joint]=(sign*(abs(a[0])+abs(b[0]))*.5,(a[1]+b[1])*.5,(a[2]+b[2])*.5)
            targets[side+suffix]=tuple(pair)
if MAGE:
    targets.update({'Spine2':((0,.015,1.31),(0,.015,1.56)),
        'Neck':((0,.015,1.56),(0,.005,1.65)),
        'Head':((0,.005,1.65),(0,.005,1.84)),
        'HeadTop_End':((0,.005,1.84),(0,.005,2.04))})
active(arm)
bpy.ops.object.mode_set(mode='EDIT')
for name,(a,b) in targets.items():
    bone=arm.data.edit_bones['mixamorig:'+name]
    bone.use_connect=False
    bone.head=a
    bone.tail=b
for sign,slot,side in [(1,'HandSlot.L','Left'),(-1,'HandSlot.R','Right')]:
    bone=arm.data.edit_bones[slot]
    bone.use_connect=False
    bone.parent=arm.data.edit_bones['mixamorig:'+side+'Hand']
    bone.head=(sign*.572,-.035,1.14)
    bone.tail=(sign*.592,-.035,1.12)
    bone.use_deform=False
if 'headfront' in arm.data.edit_bones:
    bone=arm.data.edit_bones['headfront']
    bone.head=(0,0,1.64)
    bone.tail=(0,-.1,1.64)
    bone.use_deform=False
arm.data.edit_bones['mixamorig:HeadTop_End'].use_deform=False
bpy.ops.object.mode_set(mode='OBJECT')
arm.data.pose_position='POSE'
names=[b.name for b in arm.data.bones if b.use_deform]
groups={n:model.vertex_groups.new(name=n) for n in names}
weights=np.zeros((len(points),len(names)),dtype=np.float32)
def put(name,value):weights[:,names.index('mixamorig:'+name)]+=value
x,y,z=np.abs(points[:,0]),points[:,1],points[:,2]
# Regiony ramion uwzględniają pochylenie A, szerokość klatki i płaszcz za plecami.
arm_mask=smooth(.22,.31,x)*smooth(.93,1.02,z)*(1-smooth(.08,.17,y)*(1-smooth(.30,.41,x)))
head_mask=smooth(1.44,1.55,z)*(1-smooth(.13,.24,x))
arm_mask*=1-head_mask
if MAGE:
    arm_mask=smooth(.20,.29,x)*smooth(.94,1.08,z)
    head_mask=smooth(1.54,1.65,z)*(1-smooth(.24,.34,x))
    arm_mask*=1-head_mask
body=1-arm_mask
for side,sign in [('Left',1),('Right',-1)]:
    side_mask=(points[:,0]*sign>=0).astype(np.float32)
    mask=arm_mask*side_mask
    elbow=smooth(.39,.47,x) if MAGE else smooth(.365,.445,x)
    wrist=smooth(.60,.655,x) if MAGE else smooth(.505,.55,x)
    put(side+'Arm',mask*(1-elbow))
    put(side+'ForeArm',mask*elbow*(1-wrist))
    put(side+'Hand',mask*wrist)
    # Buty skórzane zachowują bryłę; przejście jest powyżej cholewki.
    pelvis=smooth(.86,.98,z)
    knee=smooth(.46,.59,z)
    ankle=smooth(.24,.34,z)
    leg=body*side_mask*(1-smooth(.98,1.04,z))
    # Wiszące części z przodu/tyłu podążają za biodrem i udem, nie za kostką.
    cloth=smooth(.12,.19,np.abs(y))*smooth(.31,.44,z)*(1-smooth(.84,.94,z))
    if MAGE:
        cloth=np.maximum(cloth,smooth(.15,.25,x)*smooth(.18,.35,z))
    hip_cloth=cloth*.48
    put('Hips',leg*(pelvis+(1-pelvis)*hip_cloth))
    remainder=leg*(1-pelvis)*(1-hip_cloth)
    put(side+'UpLeg',remainder*np.maximum(knee,cloth*.9))
    lower=remainder*(1-np.maximum(knee,cloth*.9))
    put(side+'Leg',lower*ankle)
    put(side+'Foot',lower*(1-ankle))
upper=body*smooth(.98,1.04,z)
chest=smooth(1.14,1.29,z)
head=smooth(1.54,1.65,z)*(1-smooth(.24,.34,x)) if MAGE else smooth(1.44,1.55,z)*(1-smooth(.14,.23,x))
neck=(smooth(1.51,1.59,z) if MAGE else smooth(1.40,1.48,z))*(1-head)*(1-smooth(.12,.23,x))
put('Head',upper*head)
put('Neck',upper*neck)
trunk=upper*(1-head-neck)
put('Spine',trunk*(1-chest))
put('Spine1',trunk*chest*(1-smooth(1.27,1.38,z)))
put('Spine2',trunk*chest*smooth(1.27,1.38,z))
if MAGE:
    # Cała szata jest ciągłym okryciem. Jednolite wagi bioder nie rozrywają
    # malowanych warstw ani obrzeży na granicach koloru lub na środku między nogami.
    weights[:]=0
    head=smooth(1.55,1.68,z)
    arms=smooth(.22,.33,x)*smooth(.92,1.08,z)*(1-head)
    body=1-arms-head
    put('Head',head)
    lower=1-smooth(1.02,1.12,z)
    boots=(1-smooth(.25,.34,z))*smooth(.075,.105,x)*(1-smooth(.24,.30,x))*(1-smooth(.08,.14,y))
    put('Hips',body*lower*(1-boots))
    for side,sign in [('Left',1),('Right',-1)]:
        half=smooth(-.045,.045,points[:,0]*sign)
        elbow=smooth(.39,.47,x);wrist=smooth(.60,.655,x)
        put(side+'Arm',arms*half*(1-elbow))
        put(side+'ForeArm',arms*half*elbow*(1-wrist))
        put(side+'Hand',arms*half*wrist)
        put(side+'Foot',body*lower*boots*half*(1-smooth(.18,.28,z)))
        put(side+'Leg',body*lower*boots*half*smooth(.18,.28,z))
    torso=body*(1-lower)
    spine=smooth(1.11,1.22,z);chest=smooth(1.27,1.40,z)
    put('Hips',torso*(1-spine))
    put('Spine',torso*spine*(1-chest))
    put('Spine1',torso*spine*chest*(1-smooth(1.40,1.53,z)))
    put('Spine2',torso*spine*chest*smooth(1.40,1.53,z))
weights/=weights.sum(axis=1)[:,None]
# Maksymalnie cztery wpływy i pełna precyzja, bez progowania do sztywnych pasów.
order=np.argsort(weights,axis=1)[:,-4:]
limited=np.zeros_like(weights)
np.put_along_axis(limited,order,np.take_along_axis(weights,order,axis=1),axis=1)
weights=limited/limited.sum(axis=1)[:,None]
for column,name in enumerate(names):
    indices=np.flatnonzero(weights[:,column]>.00001)
    for index in indices:
        groups[name].add([int(index)],float(weights[index,column]),'REPLACE')
model.parent=arm
modifier=model.modifiers.new('Dotychczasowy szkielet berserkera','ARMATURE')
modifier.object=arm
active(arm)
bpy.ops.object.mode_set(mode='POSE')
for side,sign in [('Left',1),('Right',-1)]:
    for suffix in ['Arm','ForeArm','Hand']:
        bone=arm.pose.bones['mixamorig:'+side+suffix]
        bpy.context.view_layer.update()
        matrix=bone.matrix.copy()
        direction=(matrix.to_3x3()@Vector((0,1,0))).normalized()
        rotated=direction.rotation_difference(Vector((sign,0,0))).to_matrix().to_4x4()@matrix
        rotated.translation=matrix.translation
        bone.matrix=rotated
bpy.context.view_layer.update()
bpy.ops.object.mode_set(mode='OBJECT')
active(model)
bpy.ops.object.modifier_apply(modifier=modifier.name)
active(arm)
bpy.ops.object.mode_set(mode='POSE')
bpy.ops.pose.select_all(action='SELECT')
bpy.ops.pose.armature_apply(selected=False)
bpy.ops.object.mode_set(mode='OBJECT')
for bone in arm.pose.bones:
    bone.rotation_mode='QUATERNION'
    bone.rotation_quaternion=(1,0,0,0)
    bone.location=(0,0,0)
    bone.scale=(1,1,1)
modifier=model.modifiers.new('Szkielet berserkera','ARMATURE')
modifier.object=arm

# Palce i kciuk obejmują drzewce. Kości starego modelu nie mają osobnych paliczków,
# więc stały chwyt przygotowujemy w siatce, w układzie każdej dłoni.
grips={}
for side,sign in [('Left',1),('Right',-1)]:
    hand=arm.data.bones['mixamorig:'+side+'Hand']
    inverse=hand.matrix_local.inverted()
    group=model.vertex_groups['mixamorig:'+side+'Hand'].index
    selected=[v for v in model.data.vertices if any(m.group==group and m.weight>.99 for m in v.groups)]
    local=np.array([list(inverse@v.co) for v in selected])
    grips[side]={'min':local.min(axis=0).tolist(),'max':local.max(axis=0).tolist(),
                 'count':len(selected),'matrix':list(map(list,hand.matrix_local))}
    # Cieńszy uchwyt różdżki; obie dłonie mają zamknięty, naturalny chwyt.
    vertices=np.array([list(v.co) for v in selected])
    wrist=float(hand.head_local.x)*sign
    center_y=float(np.median(vertices[:,1]))
    knuckle_x=wrist+(.045 if MAGE else .055)
    section=vertices[(vertices[:,0]*sign>knuckle_x-.012)&(vertices[:,0]*sign<knuckle_x+.012)]
    knuckle_z=float(np.median(section[:,2]))
    radius=.020 if MAGE else .025
    changed=0
    thumb_changed=0
    for vertex in selected:
        p=vertex.co.copy()
        u=p.x*sign-knuckle_x
        original=p.copy()
        if u>0:
            angle=min(u/radius,3.1)
            normal=p.z-knuckle_z
            p.x=sign*(knuckle_x+(radius+normal)*math.sin(angle))
            p.z=knuckle_z+(radius+normal)*math.cos(angle)-radius
            vertex.co=p
            changed+=1
        # Kciuk domyka chwyt od przodu i nakłada opuszkę na palec wskazujący.
        thumb=float(smooth(center_y-.014,center_y-.052,original.y))
        thumb*=float(1-smooth(knuckle_x+.002,knuckle_x+.035,original.x*sign))
        thumb*=float(smooth(knuckle_z-.003,knuckle_z-.022,original.z))
        if thumb>0:
            factor=.75 if MAGE else 1.0
            vertex.co+=Vector((sign*.030,.022,.026))*(thumb*factor)
            thumb_changed+=1
    # Gniazdo znajduje się w otworze zaciśniętych palców, nie przy nadgarstku.
    active(arm)
    bpy.ops.object.mode_set(mode='EDIT')
    slot=arm.data.edit_bones['HandSlot.L' if sign>0 else 'HandSlot.R']
    slot.head=(sign*(knuckle_x+.006),center_y,knuckle_z-radius)
    slot.tail=slot.head+Vector((0,-.035,0))
    bpy.ops.object.mode_set(mode='OBJECT')
    grips[side].update(changed_vertices=changed,thumb_vertices=thumb_changed,grip_center=[sign*(knuckle_x+.006),center_y,knuckle_z-radius],radius=radius)
(OUT/'hand-coordinates.json').write_text(json.dumps(grips,indent=2),encoding='utf-8')
for action in list(bpy.data.actions):bpy.data.actions.remove(action)
for image in bpy.data.images:
    if image.has_data:image.pack()
bpy.context.preferences.filepaths.save_version=0
bpy.context.scene.unit_settings.system='METRIC'
bpy.context.scene.unit_settings.scale_length=1
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/(KEY+'_anime_rigged.blend')))
active(model)
arm.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(GAME/(KEY+'_anime.fbx')),use_selection=True,
    object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,
    primary_bone_axis='Y',secondary_bone_axis='X',axis_forward='-Z',axis_up='Y',
    apply_scale_options='FBX_SCALE_ALL',use_armature_deform_only=False,path_mode='STRIP',
    embed_textures=False,mesh_smooth_type='FACE')
report=dict(source=str(SOURCE.relative_to(ROOT)),rig_source=str(OLD.relative_to(ROOT)),
    bones=len(arm.data.bones),same_bone_names=original_names==[b.name for b in arm.data.bones],
    vertices=len(model.data.vertices),triangles=len(model.data.polygons),
    unweighted=int(np.count_nonzero(weights.sum(axis=1)<.999)),max_influences=4,
    height=height,paid_operations=0,pose='T po wypieczeniu pozy A',closed_grip=grips)
(OUT/'rig-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('RIG_BERSERKERA '+json.dumps(report,ensure_ascii=True),flush=True)
