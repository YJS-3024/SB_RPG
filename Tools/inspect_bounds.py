import bpy
from mathutils import Vector

def bounds(obj):
    points = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    return tuple(min(point[index] for point in points) for index in range(3)), tuple(max(point[index] for point in points) for index in range(3))

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=r"C:\Project\SB_RPG\Assets\See1\SD Chanz Costume Series\BunnyGirl\Models\body_bunnygirl.FBX")
bunny = next(obj for obj in bpy.context.scene.objects if obj.type == 'MESH')
print('BUNNY', bounds(bunny))

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=r"C:\Project\SB_RPG\Assets\Haons SD series Pack\__Haon SD oldVersion (ver3 - 2020year)\Models\_Dummy Model Source\_Humanoid and Add-Bone Mask Base 3d0 (Ver3).fbx")
base = next(obj for obj in bpy.context.scene.objects if obj.name == 'Mesh_Costume_Base_Female')
print('BASE', bounds(base))
