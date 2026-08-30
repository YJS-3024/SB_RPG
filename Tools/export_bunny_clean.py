import bpy

src = r"C:\Project\SB_RPG\Assets\See1\SD Chanz Costume Series\BunnyGirl\Models\body_bunnygirl.FBX"
out = r"C:\Project\SB_RPG\Assets\_InProject\Resources\CharacterParts\body_BunnyGirl_BaseRig.fbx"

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=src)
arm = next(o for o in bpy.context.scene.objects if o.type == 'ARMATURE')
mesh = next(o for o in bpy.context.scene.objects if o.type == 'MESH')
bpy.ops.object.select_all(action='DESELECT')
arm.select_set(True)
mesh.select_set(True)
bpy.context.view_layer.objects.active = arm
bpy.ops.export_scene.fbx(filepath=out, use_selection=True, object_types={'ARMATURE', 'MESH'}, add_leaf_bones=False, bake_anim=False, global_scale=1.0, apply_unit_scale=False)
print('WROTE', out)
