import bpy, sys, os

bunny = r'C:\Project\SB_RPG\Assets\See1\SD Chanz Costume Series\BunnyGirl\Models\body_bunnygirl.FBX'
base = r'C:\Project\SB_RPG\Assets\Haons SD series Pack\__Haon SD oldVersion (ver3 - 2020year)\Models\_Dummy Model Source\_Humanoid and Add-Bone Mask Base 3d0 (Ver3).fbx'
out = r'C:\Project\SB_RPG\Assets\_InProject\Resources\CharacterParts\body_BunnyGirl_BaseRig.fbx'

bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=bunny)
bunny_objs=list(bpy.context.scene.objects)
bunny_mesh=[o for o in bunny_objs if o.type=='MESH'][0]
bpy.ops.import_scene.fbx(filepath=base)
base_objs=[o for o in bpy.context.scene.objects if o not in bunny_objs]
arm=[o for o in base_objs if o.type=='ARMATURE'][0]
for o in base_objs:
    o.select_set(False)
for o in bunny_objs:
    o.select_set(True)
for o in base_objs:
    o.hide_set(True)
bunny_mesh.select_set(True); arm.hide_set(False); arm.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.object.parent_set(type='ARMATURE_AUTO')
# keep only the Bunny mesh and Base armature, with generated weights
for o in list(bpy.context.scene.objects):
    if o not in (bunny_mesh, arm):
        bpy.data.objects.remove(o, do_unlink=True)
bpy.ops.object.select_all(action='DESELECT'); bunny_mesh.select_set(True); arm.select_set(True); bpy.context.view_layer.objects.active=arm
for obj in (bunny_mesh, arm):
    obj.scale = obj.scale * 3.0
    bpy.context.view_layer.objects.active=obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.select_set(False)
bpy.ops.object.select_all(action='DESELECT')
bunny_mesh.select_set(True)
arm.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=out, use_selection=True, object_types={'ARMATURE','MESH'}, add_leaf_bones=False, bake_anim=False, global_scale=0.00001, apply_unit_scale=True)
print('WROTE', out)
