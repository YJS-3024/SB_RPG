import bpy
import os

PROJECT = r"C:\Project\SB_RPG"
BASE_FBX = os.path.join(PROJECT, r"Assets\Haons SD series Pack\__Haon SD oldVersion (ver3 - 2020year)\Models\_Dummy Model Source\_Humanoid and Add-Bone Mask Base 3d0 (Ver3).fbx")
BUNNY_FBX = os.path.join(PROJECT, r"Assets\_InProject\Resources\FBX\body_BunnyGirl_Black.fbx")
OUTPUT_FBX = os.path.join(PROJECT, r"Assets\_InProject\Resources\FBX\body_BunnyGirl_Black_BaseRig.fbx")

GROUP_MAP = {
    "Character1_Hips": "Hips",
    "Character1_Spine": "Spine",
    "Character1_Spine1": "Chest",
    "Character1_Spine2": "Upper_Chest",
    "Character1_Neck": "Neck",
    "Character1_Head": "Head",
    "Character1_LeftShoulder": "Clavicle_L",
    "Character1_LeftArm": "Upper_Arm_L",
    "J_L_Arm_00_tw": "Upper_Arm_L_twist_00",
    "Character1_LeftForeArm": "Lower_Arm_L",
    "J_L_ForeArm_00_tw": "Lower_Arm_L_twist_00",
    "J_L_Elbow": "Elbow_L",
    "Character1_LeftHand": "Hand_L",
    "Character1_LeftHandPinky1": "Finger_L_Pinky_Proximal",
    "Character1_LeftHandPinky2": "Finger_L_Pinky_Intermediate",
    "Character1_LeftHandRing1": "Finger_L_Ring_Proximal",
    "Character1_LeftHandRing2": "Finger_L_Ring_Intermediate",
    "Character1_LeftHandMiddle1": "Finger_L_Middle_Proximal",
    "Character1_LeftHandMiddle2": "Finger_L_Middle_Intermediate",
    "Character1_LeftHandIndex1": "Finger_L_Index_Proximal",
    "Character1_LeftHandIndex2": "Finger_L_Index_Intermediate",
    "Character1_LeftHandThumb1": "Finger_L_Thumb_Proximal",
    "Character1_LeftHandThumb2": "Finger_L_Thumb_Intermediate",
    "Character1_LeftHandThumb3": "Finger_L_Thumb_Distal",
    "Character1_RightShoulder": "Clavicle_R",
    "Character1_RightArm": "Upper_Arm_R",
    "J_R_Arm_00_tw": "Upper_Arm_R_twist_00",
    "Character1_RightForeArm": "Lower_Arm_R",
    "J_R_ForeArm_00_tw": "Lower_Arm_R_twist_00",
    "J_R_Elbow": "Elbow_R",
    "Character1_RightHand": "Hand_R",
    "Character1_RightHandPinky1": "Finger_R_Pinky_Proximal",
    "Character1_RightHandPinky2": "Finger_R_Pinky_Intermediate",
    "Character1_RightHandRing1": "Finger_R_Ring_Proximal",
    "Character1_RightHandRing2": "Finger_R_Ring_Intermediate",
    "Character1_RightHandMiddle1": "Finger_R_Middle_Proximal",
    "Character1_RightHandMiddle2": "Finger_R_Middle_Intermediate",
    "Character1_RightHandIndex1": "Finger_R_Index_Proximal",
    "Character1_RightHandIndex2": "Finger_R_Index_Intermediate",
    "Character1_RightHandThumb1": "Finger_R_Thumb_Proximal",
    "Character1_RightHandThumb2": "Finger_R_Thumb_Intermediate",
    "Character1_RightHandThumb3": "Finger_R_Thumb_Distal",
    "Character1_LeftUpLeg": "Upper_Leg_L",
    "J_L_knee": "Knee_L",
    "Character1_LeftLeg": "Lower_Leg_L",
    "Character1_LeftFoot": "Foot_L",
    "Character1_RightUpLeg": "Upper_Leg_R",
    "J_R_knee": "Knee_R",
    "Character1_RightLeg": "Lower_Leg_R",
    "Character1_RightFoot": "Foot_R",
    "Collar": "Upper_Chest",
}

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=BASE_FBX)
base_armature = max((obj for obj in bpy.context.scene.objects if obj.type == 'ARMATURE'), key=lambda obj: len(obj.data.bones))

bpy.ops.import_scene.fbx(filepath=BUNNY_FBX)
bunny_armature = max((obj for obj in bpy.context.scene.objects if obj.type == 'ARMATURE' and obj != base_armature), key=lambda obj: len(obj.data.bones))
bunny_meshes = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH' and obj.parent == bunny_armature]
if not bunny_meshes:
    bunny_meshes = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH' and obj.vertex_groups]
if len(bunny_meshes) != 1:
    raise RuntimeError("Expected one BunnyGirl mesh, found %d" % len(bunny_meshes))

mesh = bunny_meshes[0]
base_bones = {bone.name for bone in base_armature.data.bones}
for group in list(mesh.vertex_groups):
    target_name = GROUP_MAP.get(group.name)
    if target_name is None:
        raise RuntimeError("Unmapped BunnyGirl weight group: %s" % group.name)
    if target_name not in base_bones:
        raise RuntimeError("Missing BaseRig bone: %s" % target_name)
    group.name = target_name

for modifier in list(mesh.modifiers):
    if modifier.type == 'ARMATURE':
        mesh.modifiers.remove(modifier)
modifier = mesh.modifiers.new(name="BaseRig", type='ARMATURE')
modifier.object = base_armature
mesh.parent = base_armature

for obj in list(bpy.context.scene.objects):
    if obj != mesh and obj != base_armature:
        bpy.data.objects.remove(obj, do_unlink=True)

base_armature.name = "BaseRig"
base_armature.data.name = "BaseRig"
mesh.name = "body_BunnyGirl_Black_BaseRig"

bpy.ops.object.select_all(action='DESELECT')
base_armature.select_set(True)
mesh.select_set(True)
bpy.context.view_layer.objects.active = base_armature
bpy.ops.export_scene.fbx(
    filepath=OUTPUT_FBX,
    use_selection=True,
    object_types={'ARMATURE', 'MESH'},
    add_leaf_bones=False,
    bake_anim=False,
    use_armature_deform_only=True,
    mesh_smooth_type='FACE',
    path_mode='AUTO',
)
print("EXPORTED", OUTPUT_FBX)
