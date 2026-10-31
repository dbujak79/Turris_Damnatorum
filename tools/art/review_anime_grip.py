"""Rzeczywiste dłonie siatki i wzorcowe drzewce, bez zapisu assetów gry."""
import sys
from pathlib import Path
import bpy
from mathutils import Vector

root=Path(__file__).resolve().parents[2]
mage='--mage' in sys.argv
key='mage' if mage else 'berserker'
folder=root/('ArtSource/GothicAnimePoC/Mage/rigged' if mage else 'ArtSource/GothicAnimePoC/Heroes/Berserker/rigged')
bpy.ops.wm.open_mainfile(filepath=str(folder/(key+'_anime_rigged.blend')))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True
scene.render.resolution_x=900;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Studio chwytu');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.35,.4,.5,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.6
data=bpy.data.cameras.new('Kamera');camera=bpy.data.objects.new('Kamera',data);scene.collection.objects.link(camera)
data.type='ORTHO';data.ortho_scale=.25;scene.camera=camera
lamp=bpy.data.lights.new('Światło','AREA');lamp.energy=80;lamp.size=1
light=bpy.data.objects.new('Światło',lamp);scene.collection.objects.link(light)
output=root/('TestResults/'+key+'-anime/grip-source');output.mkdir(parents=True,exist_ok=True)
for side,slot in [('left','HandSlot.L'),('right','HandSlot.R')]:
    center=arm.matrix_world@arm.data.bones[slot].head_local
    bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=.02,depth=.32,location=center)
    rod=bpy.context.object;rod.rotation_euler=(1.57079632679,0,0)
    material=bpy.data.materials.new('Drzewce wzorcowe');material.diffuse_color=(.25,.10,.035,1);rod.data.materials.append(material)
    for view,delta in [('top',(.16,-.28,.18)),('palm',(.15,-.28,-.18))]:
        camera.location=center+Vector(delta);camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler()
        light.location=center+Vector((.3,-.5,.6));light.rotation_euler=(center-light.location).to_track_quat('-Z','Y').to_euler()
        scene.render.filepath=str(output/(side+'-'+view+'.png'));bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(rod,do_unlink=True)
print('CHWYT_OBEJRZANY_RENDERY '+str(output))
