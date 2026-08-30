import bpy

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=r"C:\Project\SB_RPG\Assets\Haons SD series Pack\__Haon SD oldVersion (ver3 - 2020year)\_Dummy Model Source\_Humanoid and Add-Bone Mask Base 3d0 (Ver3).fbx")

for obj in bpy.context.scene.objects:
    if obj.type == 'ARMATURE':
        print("ARMATURE", obj.name, "BONES", len(obj.data.bones))
        print("BONE_NAMES", ",".join(bone.name for bone in obj.data.bones))
