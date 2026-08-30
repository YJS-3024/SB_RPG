import bpy

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=r"C:\Project\SB_RPG\Assets\_InProject\Resources\FBX\body_BunnyGirl_Black.fbx")

for obj in bpy.context.scene.objects:
    if obj.type == 'ARMATURE':
        print("ARMATURE", obj.name, "BONES", len(obj.data.bones))
    elif obj.type == 'MESH':
        print("MESH", obj.name, "GROUPS", len(obj.vertex_groups))
        print("GROUP_NAMES", ",".join(group.name for group in obj.vertex_groups))
