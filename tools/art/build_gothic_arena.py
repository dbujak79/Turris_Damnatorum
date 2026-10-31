"""Moduły dziedzińca gotyckiego anime. Blender 5.2, wyłącznie nowe pliki PoC."""
import bpy
import math
import random
import json
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "ArtSource/GothicAnimePoC/Arena"
GAME = ROOT / "Assets/TurrisDamnatorum/Models/PoC/GothicArena"
OUT.mkdir(parents=True, exist_ok=True)
GAME.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
rng = random.Random(726103)
STONE = (0.49, 0.465, 0.425, 1)
EDGE = (0.56, 0.525, 0.465, 1)
DARK = (0.18, 0.175, 0.17, 1)
GOLD = (0.57, 0.435, 0.235, 1)
IRON = (0.15, 0.16, 0.18, 1)
CLOTH = (0.31, 0.045, 0.055, 1)
material = bpy.data.materials.new("KamienMalowany")
pieces = []
modules = []


def painted(obj, color, variation=0):
    obj.data.materials.append(material)
    attr = obj.data.color_attributes.new(name="Col", type="FLOAT_COLOR", domain="CORNER")
    distant=color[0]<.12 and color[2]>color[0]*1.8
    if distant:
        color=tuple(min(1,c*2.2) for c in color[:3])+(1,)
    for polygon in obj.data.polygons:
        gain = 1 + rng.uniform(-variation, variation)
        if distant:
            gain *= .70+.30*max(0,polygon.normal.dot(Vector((-.6,-.7,.8)).normalized()))
        for loop in polygon.loop_indices:
            attr.data[loop].color_srgb = tuple(min(1, max(0, c * gain)) for c in color[:3]) + (1,)
    pieces.append(obj)
    return obj


def box(pos, size, color=STONE, bevel=0.025, rot=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=pos)
    obj = bpy.context.object
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.rotation_euler.z = rot
    if bevel:
        mod = obj.modifiers.new("KrawedzieCiosuKamienia", "BEVEL")
        mod.width = bevel
        mod.segments = 1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return painted(obj, color, 0.018)


def mesh(name, verts, faces, color=STONE, variation=0.015):
    data = bpy.data.meshes.new(name)
    data.from_pydata(verts, [], faces)
    data.update()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    return painted(obj, color, variation)


def line(points, width, color=DARK):
    for a, b in zip(points, points[1:]):
        d = Vector(b) - Vector(a)
        p = (Vector(a) + Vector(b)) / 2
        obj = box(p, (width, width, d.length), color, 0)
        obj.rotation_euler = d.to_track_quat("Z", "Y").to_euler()


def lathe(profile, sides=24, flutes=0, color=STONE, center=(0, 0, 0)):
    verts, faces = [], []
    for radius, height in profile:
        for i in range(sides):
            a = i * math.tau / sides
            r = radius * (1 - flutes * (0.5 + 0.5 * math.cos(i * math.pi)))
            verts.append((center[0] + r * math.cos(a), center[1] + r * math.sin(a), center[2] + height))
    for j in range(len(profile) - 1):
        for i in range(sides):
            n = (i + 1) % sides
            faces.append((j * sides + i, j * sides + n, (j + 1) * sides + n, (j + 1) * sides + i))
    faces += [tuple(reversed(range(sides))), tuple((len(profile) - 1) * sides + i for i in range(sides))]
    return mesh("ProfilKamienny", verts, faces, color)


def finish(name):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in pieces:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = pieces[0]
    bpy.ops.object.join()
    obj = bpy.context.object
    obj.name = name
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    modules.append(obj)
    pieces.clear()


# Płyty: nieregularny obrys, ukruszone naroża, faza i rzeczywista grubość zamiast pudełka.
for variant in range(10):
    cut = [rng.uniform(0.035, 0.12) for _ in range(4)]
    xy = [(-0.5 + cut[0], -0.5), (0.5 - cut[1], -0.5), (0.5, -0.5 + cut[1]),
          (0.5, 0.5 - cut[2]), (0.5 - cut[2], 0.5), (-0.5 + cut[3], 0.5),
          (-0.5, 0.5 - cut[3]), (-0.5, -0.5 + cut[0])]
    verts = [(x, y, -0.15) for x, y in xy] + [(x, y, -0.018) for x, y in xy]
    verts += [(x * 0.965, y * 0.965, 0) for x, y in xy]
    faces = []
    for row in range(2):
        for i in range(8):
            faces.append((row * 8 + i, row * 8 + (i + 1) % 8, (row + 1) * 8 + (i + 1) % 8, (row + 1) * 8 + i))
    faces.append(tuple(16 + i for i in range(8)))
    gain = rng.uniform(0.92, 1.04)
    color = tuple(c * gain for c in STONE[:3]) + (1,)
    mesh("Plyta", verts, faces, color)
    if variant % 3 == 0:
        x = rng.uniform(-0.3, 0.3)
        path = [(x, -0.495, 0.001)]
        for j in range(1, 5):
            path.append((x + rng.uniform(-0.10, 0.10), -0.5 + j * 0.18, 0.001))
        line(path, 0.006, DARK)
        line([path[2], (path[2][0] + 0.15, path[2][1] + 0.07, 0.001)], 0.004, DARK)
    finish(f"Plyta_{variant:02}")

# Rozłupane płyty z rzeczywistą szczeliną i fazowanymi krawędziami przełamu.
for variant in range(10,14):
    points=[(-.44,-.5),(-.05,-.5),(.04,-.25),(-.025,0),(.035,.27),(-.02,.5),(-.44,.5),(-.5,.43),(-.5,-.43)]
    for side in [-1,1]:
        xy=[(side*x+side*.009,y) for x,y in points]
        if side==-1:
            xy.reverse()
        count=len(xy)
        verts=[(x,y,-.15) for x,y in xy]+[(x,y,-.014) for x,y in xy]+[(x*.98,y*.98,0) for x,y in xy]
        faces=[]
        for row in range(2):
            for i in range(count):
                faces.append((row*count+i,row*count+(i+1)%count,(row+1)*count+(i+1)%count,(row+1)*count+i))
        faces.append(tuple(2*count+i for i in range(count)))
        gain=rng.uniform(.93,1.02)
        obj=mesh("PeknietaPlyta",verts,faces,tuple(c*gain for c in STONE[:3])+(1,))
        obj.rotation_euler.z=(variant-10)*math.pi/2
    finish(f"Plyta_{variant:02}")


def wall(variant):
    width = 4.6
    box((0, 0.16, 2.1), (width, 0.7, 4.2), DARK, 0)
    for row in range(8):
        x = -width / 2 - (0.49 if row % 2 else 0)
        while x < width / 2:
            length = rng.uniform(0.73, 1.2)
            a, b = max(x, -width / 2), min(x + length, width / 2)
            if b - a > 0.12:
                gain = rng.uniform(0.94, 1.06)
                c = tuple(v * gain for v in STONE[:3]) + (1,)
                box(((a + b) / 2, 0, 0.35 + row * 0.475), (b - a - 0.022, 0.82 + rng.uniform(-0.012, 0.012), 0.451), c, 0.014)
            x += length
    for z, depth, height in [(0.12, 1.02, 0.24), (0.32, 0.91, 0.12), (4.12, 0.98, 0.16), (4.28, 1.08, 0.16)]:
        box((0, -0.015, z), (width, depth, height), EDGE, 0.025)
    for side in [-1, 1]:
        x = side * (width / 2 - 0.15)
        box((x, -0.12, 2.27), (0.40, 1.07, 4.54), EDGE, 0.025)
        box((x, -0.12, 4.61), (0.65, 1.22, 0.21), EDGE, 0.035)
        lathe([(0.33, 0), (0.33, 0.12), (0.06, 0.48)], 4, color=EDGE, center=(x, -0.12, 4.71))
    if variant:
        for x, z in [(-1.1, 2.8), (1.5, 1.3), (-0.6, 3.8)]:
            line([(x, -0.425, z), (x + 0.09, -0.425, z - 0.22), (x - 0.03, -0.425, z - 0.48), (x + 0.16, -0.425, z - 0.68)], 0.009)
    finish(f"Mur_{variant:02}")


wall(0)
wall(1)


def column(name, broken=False):
    for z, s, h in [(0.10, 1.5, 0.2), (0.27, 1.3, 0.14), (0.43, 1.13, 0.18)]:
        box((0, 0, z), (s, s, h), EDGE, 0.035)
    lathe([(0.53, 0.52), (0.53, 0.62), (0.43, 0.7), (0.43, 0.84)], 32, color=EDGE)
    height = 1.9 if broken else 8.8
    lathe([(0.43, 0.84), (0.43, 0.95), (0.39, height - 0.2), (0.39, height)], 64, 0.12)
    if broken:
        lathe([(0.39, height), (0.31, height + 0.12), (0.18, height + 0.25)], 11, color=EDGE)
    else:
        lathe([(0.39, height), (0.49, height + 0.12), (0.55, height + 0.26), (0.55, height + 0.4)], 32, color=EDGE)
        box((0, 0, height + 0.49), (1.28, 1.28, 0.18), EDGE, 0.035)
    finish(name)


column("Kolumna_Cala")
column("Kolumna_Zlamana", True)
lathe([(0.38, 0), (0.40, 0.16), (0.40, 1.65), (0.34, 1.84)], 64, 0.12)
finish("Trzon_Powalony")


def bezier(t):
    a, b, c, d = Vector((-2.15, 0)), Vector((-2.15, 1.6)), Vector((-1.0, 2.5)), Vector((0, 3.0))
    return (1-t)**3*a + 3*(1-t)**2*t*b + 3*(1-t)*t*t*c + t**3*d


def arch(broken=False):
    for side in [-1, 1]:
        stem = 2.9 if not broken or side == -1 else 1.6
        x = -side * 2.15
        box((x, 0, stem/2), (0.46, 0.9, stem), EDGE, 0.025)
        for rib in [-0.14, 0.14]:
            box((x+rib, -0.49, stem/2), (0.065, 0.11, stem), EDGE, 0.012)
        box((x, 0, 0.14), (0.85, 1.05, 0.28), EDGE, 0.03)
        if broken and side == 1:
            continue
        count = 5 if broken else 18
        for i in range(count):
            t0, t1 = (i+0.025)/18, (i+0.975)/18
            p0, p1 = bezier(t0), bezier(t1)
            direction = (p1-p0).normalized()
            normal = Vector((-direction.y, direction.x))
            for offset, thickness, depth, front in [(0, 0.34, 0.78, 0), (-0.16, 0.065, 0.12, -0.47), (0.23, 0.08, 0.14, -0.47)]:
                q = [p0+normal*(offset-thickness/2), p1+normal*(offset-thickness/2),
                     p1+normal*(offset+thickness/2), p0+normal*(offset+thickness/2)]
                verts = [(side*v.x, y, v.y+2.9) for y in [front-depth/2, front+depth/2] for v in q]
                faces=[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]
                if side == -1:
                    faces = [tuple(reversed(f)) for f in faces]
                mesh("KlieniecOstroluku",verts,faces,EDGE)
    if not broken:
        lathe([(0.26,0),(0.32,0.26),(0.04,0.59)],4,color=EDGE,center=(0,0,5.9))
    finish("Ostroluk_Zlamany" if broken else "Ostroluk_Caly")


arch(False)
arch(True)

# Sztandar ma rzeczywiste fałdy, złoty haft i postrzępiony brzeg.
nx, nz = 18, 26
verts, faces = [], []
for j in range(nz+1):
    t = j/nz
    for i in range(nx+1):
        u = i/nx
        torn = (0.14 if i%3==0 else -0.04)*max(0,(t-0.9)/0.1)
        verts.append(((u-.5)*1.18, -0.06*math.sin(u*math.pi*5+t*1.3)*(0.3+t), -t*2.5+torn))
for j in range(nz):
    for i in range(nx):
        a=j*(nx+1)+i
        faces += [(a,a+1,a+nx+2,a+nx+1)]
obj=mesh("Tkanina",verts,faces,CLOTH,0.025)
for polygon in obj.data.polygons:
    polygon.use_smooth = True
for side in [-1,1]:
    box((side*.56,-.04,-1.12),(.025,.02,2.22),GOLD,0.003)
box((0,-.08,-.78),(.065,.026,.64),GOLD,.004)
box((0,-.08,-.65),(.36,.026,.065),GOLD,.004)
box((0,0,.045),(1.43,.07,.07),IRON,.012)
for side in [-1,1]:
    lathe([(.07,0),(.07,.12),(0,.2)],8,color=GOLD,center=(side*.71,0,0))
finish("Sztandar_Krzyz")

# Kosz, ramiona i szpica kutego żelaza; płomień dołącza Unity.
box((0,0,0),(.22,.09,.75),IRON,.02)
line([(0,-.05,-.2),(0,-.42,-.04),(0,-.62,.36)],.065,IRON)
lathe([(.09,0),(.12,.12),(.22,.35),(.24,.39)],12,color=IRON,center=(0,-.60,.20))
for i in range(6):
    a=i*math.tau/6
    line([(.18*math.cos(a),-.60+.18*math.sin(a),.35),(.25*math.cos(a),-.60+.25*math.sin(a),.72)],.027,IRON)
finish("Pochodnia_Kosz")

# Wielościenne odłamki z jasnymi przełamami, bez gładkich kul.
for i in range(6):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.5)
    obj=bpy.context.object
    for v in obj.data.vertices:
        v.co *= rng.uniform(.65,1.1)
        v.co.z *= .65
        v.co.z += .15
    painted(obj,STONE,0.08)
    finish(f"Gruz_{i:02}")

# Skalista podstawa oraz zamek to sylwety 3D: wieże, przypory, ostre dachy, arkadowy most.
for i in range(12):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=(rng.uniform(-18,18),rng.uniform(-9,9),-8))
    obj=bpy.context.object
    obj.scale=(rng.uniform(9,18),rng.uniform(8,14),rng.uniform(10,17))
    painted(obj,(.06,.095,.16,1),.08)
for x,y,h,r in [(-14,0,19,2.2),(-8,3,25,2.0),(-3,0,28,2.7),(3,3,23,2.1),(8,0,21,2.0),(13,4,18,1.8)]:
    lathe([(r,0),(r,h),(r*1.12,h+.6),(r*1.12,h+1),(r*.9,h+1.2)],12,color=(.055,.08,.13,1),center=(x,y,-1))
    lathe([(r*1.13,h+1),(0,h+10)],12,color=(.035,.055,.10,1),center=(x,y,-1))
    for angle in range(0,360,90):
        a=math.radians(angle)
        lathe([(.45,0),(.45,3),(0,6)],8,color=(.045,.07,.12,1),center=(x+r*math.cos(a),y+r*math.sin(a),h-1))
    for z in range(5,int(h)-2,4):
        for side in [-1,1]:
            box((x,side*r+y+.015*side,z),(.16,.025,.85),(.016,.025,.045,1),0)
for x in [-10,-5,0,5,10]:
    box((x,2,7),(5.3,6,14),(.05,.075,.12,1),.1)
    for j in [-1.7,-.6,.6,1.7]:
        lathe([(.18,0),(0,2)],4,color=(.035,.055,.095,1),center=(x+j,-1,14))
for x in range(16,43,5):
    box((x,3,1),(1.3,3,13),(.055,.085,.14,1),.05)
    box((x+2.5,3,7.8),(5.3,3,.7),(.06,.09,.145,1),.05)
    path=[(x+2.5+2.25*math.cos(i*math.pi/16),3,5.1+2.25*math.sin(i*math.pi/16)) for i in range(17)]
    line(path,.40,(.06,.09,.145,1))
finish("Zamek_Na_Skale")

bpy.ops.object.select_all(action="SELECT")
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"gothic_arena_modules.blend"))
bpy.ops.export_scene.fbx(filepath=str(GAME/"gothic_arena_modules.fbx"),use_selection=True,object_types={"MESH"},
                         add_leaf_bones=False,bake_anim=False,axis_forward="-Z",axis_up="Y",
                         bake_space_transform=True,mesh_smooth_type="FACE",colors_type="SRGB",path_mode="STRIP")
report={obj.name:{"vertices":len(obj.data.vertices),"triangles":sum(len(p.vertices)-2 for p in obj.data.polygons)} for obj in modules}
(OUT/"modules-report.json").write_text(json.dumps(report,indent=2),encoding="utf-8")
print("GOTHIC_ARENA_RESULT "+json.dumps(report))
