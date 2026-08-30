import bpy

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=r"C:\Project\SB_RPG\Assets\Haons SD series Pack\__Haon SD oldVersion (ver3 - 2020year)\Models\_Dummy Model Source\_Humanoid and Add-Bone Mask Base 3d0 (Ver3).fbx")
arm = next(obj for obj in bpy.context.scene.objects if obj.type == 'ARMATURE')
for name in ('Hips', 'Head', 'Foot_L', 'Foot_R', 'Hand_L', 'Hand_R'):
    print(name, arm.matrix_world @ arm.data.bones[name].head)
