import bpy
bpy.ops.wm.read_factory_settings(use_empty=True)
src=r'C:\Project\SB_RPG\Assets\See1\SD Chanz Costume Series\BunnyGirl\Models\body_bunnygirl.FBX'
out=r'C:\Project\SB_RPG\Assets\_InProject\Resources\CharacterParts\body_BunnyGirl_BaseRig.fbx'
bpy.ops.import_scene.fbx(filepath=src)
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
mp={'Character1_Hips':'Hips','Character1_Spine':'Spine','Character1_Spine1':'Chest','Character1_Spine2':'Upper_Chest','Character1_Neck':'Neck','Character1_Head':'Head','Character1_RightShoulder':'Clavicle_R','Character1_LeftShoulder':'Clavicle_L','Character1_RightArm':'Upper_Arm_R','Character1_LeftArm':'Upper_Arm_L','Character1_RightForeArm':'Lower_Arm_R','Character1_LeftForeArm':'Lower_Arm_L','Character1_RightHand':'Hand_R','Character1_LeftHand':'Hand_L','Character1_RightUpLeg':'Upper_Leg_R','Character1_LeftUpLeg':'Upper_Leg_L','Character1_RightLeg':'Lower_Leg_R','Character1_LeftLeg':'Lower_Leg_L','Character1_RightFoot':'Foot_R','Character1_LeftFoot':'Foot_L'}
bpy.context.view_layer.objects.active=arm;bpy.ops.object.mode_set(mode='EDIT')
for b in arm.data.edit_bones:
    if b.name in mp:b.name=mp[b.name]
bpy.ops.object.mode_set(mode='OBJECT')
mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH')
for vg in mesh.vertex_groups:
    if vg.name in mp:vg.name=mp[vg.name]
for o in list(bpy.context.scene.objects):
    if o not in (mesh,arm):bpy.data.objects.remove(o,do_unlink=True)
bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True);arm.select_set(True);bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=out,use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,global_scale=1.0,apply_unit_scale=False)
print('WROTE',out)