import bpy
from mathutils import Matrix

BASE = r"C:\Project\SB_RPG\Assets\Haons SD series Pack\__Haon SD oldVersion (ver3 - 2020year)\Models\_Dummy Model Source\_Humanoid and Add-Bone Mask Base 3d0 (Ver3).fbx"
BUNNY = r"C:\Project\SB_RPG\Assets\See1\SD Chanz Costume Series\BunnyGirl\Models\body_bunnygirl.FBX"
OUT = r"C:\Project\SB_RPG\Assets\_InProject\Resources\CharacterParts\body_BunnyGirl_BaseRig.fbx"

MAP = {
    "Character1_Hips": "Hips", "Character1_Spine": "Spine", "Character1_Spine1": "Chest",
    "Character1_Spine2": "Upper_Chest", "Character1_Neck": "Neck", "Character1_Head": "Head",
    "Character1_RightShoulder": "Clavicle_R", "Character1_LeftShoulder": "Clavicle_L",
    "Character1_RightArm": "Upper_Arm_R", "Character1_LeftArm": "Upper_Arm_L",
    "Character1_RightForeArm": "Lower_Arm_R", "Character1_LeftForeArm": "Lower_Arm_L",
    "J_R_Elbow": "Elbow_R", "J_L_Elbow": "Elbow_L",
    "J_R_Arm_00_tw": "Upper_Arm_R_twist_00", "J_L_Arm_00_tw": "Upper_Arm_L_twist_00",
    "J_R_ForeArm_00_tw": "Lower_Arm_R_twist_00", "J_L_ForeArm_00_tw": "Lower_Arm_L_twist_00",
    "Character1_RightHand": "Hand_R", "Character1_LeftHand": "Hand_L",
    "Character1_RightUpLeg": "Upper_Leg_R", "Character1_LeftUpLeg": "Upper_Leg_L",
    "Character1_RightLeg": "Lower_Leg_R", "Character1_LeftLeg": "Lower_Leg_L",
    "Character1_RightFoot": "Foot_R", "Character1_LeftFoot": "Foot_L",
    "J_R_knee": "Knee_R", "J_L_knee": "Knee_L",
    "Character1_RightHandThumb1": "Finger_R_Thumb_Proximal", "Character1_RightHandThumb2": "Finger_R_Thumb_Intermediate", "Character1_RightHandThumb3": "Finger_R_Thumb_Distal",
    "Character1_LeftHandThumb1": "Finger_L_Thumb_Proximal", "Character1_LeftHandThumb2": "Finger_L_Thumb_Intermediate", "Character1_LeftHandThumb3": "Finger_L_Thumb_Distal",
}
for side, long, short in (("Right", "R", "R"), ("Left", "L", "L")):
    for finger, target in (("Pinky", "Pinky"), ("Ring", "Ring"), ("Middle", "Middle"), ("Index", "Index")):
        MAP[f"Character1_{side}Hand{finger}1"] = f"Finger_{long}_{target}_Proximal"
        MAP[f"Character1_{side}Hand{finger}2"] = f"Finger_{long}_{target}_Intermediate"

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=BASE)
base_arm = next(o for o in bpy.context.scene.objects if o.type == 'ARMATURE')
for obj in list(bpy.context.scene.objects):
    if obj.type == 'MESH':
        bpy.data.objects.remove(obj, do_unlink=True)

bpy.ops.import_scene.fbx(filepath=BUNNY)
bunny_arm = next(o for o in bpy.context.scene.objects if o.type == 'ARMATURE' and o != base_arm)
bunny_mesh = next(o for o in bpy.context.scene.objects if o.type == 'MESH')

# Place the costume's Hips rest transform on the Base Hips rest transform.
base_hips = base_arm.matrix_world @ base_arm.data.bones['Hips'].matrix_local
bunny_hips = bunny_arm.matrix_world @ bunny_arm.data.bones['Character1_Hips'].matrix_local
bunny_mesh.matrix_world = base_hips @ bunny_hips.inverted() @ bunny_mesh.matrix_world
bpy.context.view_layer.objects.active = bunny_mesh
bunny_mesh.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

# Preserve all existing weights; unsupported accessory bones fall back to their nearest mapped ancestor.
source_bones = bunny_arm.data.bones
for group in bunny_mesh.vertex_groups:
    source = source_bones.get(group.name)
    name = group.name
    while name not in MAP and source and source.parent:
        source = source.parent
        name = source.name
    target = MAP.get(name, 'Hips')
    group.name = target

mod = bunny_mesh.modifiers.new(name='Armature', type='ARMATURE')
mod.object = base_arm
bunny_mesh.parent = base_arm
bunny_mesh.matrix_parent_inverse = base_arm.matrix_world.inverted()

bpy.data.objects.remove(bunny_arm, do_unlink=True)
for obj in list(bpy.context.scene.objects):
    if obj.type == 'EMPTY':
        bpy.data.objects.remove(obj, do_unlink=True)

bpy.ops.object.select_all(action='DESELECT')
base_arm.select_set(True)
bunny_mesh.select_set(True)
bpy.context.view_layer.objects.active = base_arm
bpy.ops.export_scene.fbx(filepath=OUT, use_selection=True, object_types={'ARMATURE','MESH'}, add_leaf_bones=False, bake_anim=False, global_scale=1.0, apply_unit_scale=False)
print('WROTE', OUT)

