"""Rebind the package BunnyGirl accessory to the project's aligned BaseRig.

Run: blender --background --python Tools/rebind_bunny_accessory.py
Requires source_geometry.json in Temp/AccessoryRebind exported from Unity:
renderer localToWorld matrix (row arrays) and sharedMesh vertices (XYZ arrays).
Writes a candidate into Temp/AccessoryRebind; does not overwrite source assets.
"""
from pathlib import Path
import json
import bpy
from mathutils import Matrix, Vector, kdtree

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'Assets/See1/SD Chanz Costume Series/BunnyGirl/Models/acc_bunnygirl.FBX'
RIG = ROOT / 'Assets/_InProject/Resources/FBX/BaseRig_HeadAligned.fbx'
OUT = ROOT / 'Temp/AccessoryRebind/acc_BunnyGirl.fbx'
OUT.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(SOURCE), use_anim=False)
meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
if len(meshes) != 1:
    raise ValueError('Expected one accessory mesh')
mesh = meshes[0]
# FBX geometric transforms are represented differently by the two importers.
# Match the source mesh-local data first, then use Unity's renderer transform.
data = json.loads((OUT.parent / 'source_geometry.json').read_text())
unity_vertices = [Vector(v) for v in data['vertices']]
scale = Matrix.Diagonal(Vector((-0.01, 0.01, 0.01, 1.0)))
local = [scale @ v.co for v in mesh.data.vertices]
def midpoint(points):
    return Vector([(min(v[i] for v in points)+max(v[i] for v in points))/2 for i in range(3)])
offset = midpoint(unity_vertices)-midpoint(local)
local_map = Matrix.Translation(offset) @ scale
kd = kdtree.KDTree(len(unity_vertices))
for i,v in enumerate(unity_vertices):
    kd.insert(v,i)
kd.balance()
error = max(kd.find(local_map @ v.co)[2] for v in mesh.data.vertices)
if error > 0.00001:
    raise ValueError(f'Unity/Blender vertex correspondence mismatch: {error}')
unity_to_blender = Matrix(((-1,0,0,0),(0,0,-1,0),(0,1,0,0),(0,0,0,1)))
mesh.data.transform(unity_to_blender @ Matrix(data['matrix']) @ local_map)
mesh.parent = None
mesh.matrix_world = Matrix.Identity(4)
for mod in list(mesh.modifiers):
    if mod.type == 'ARMATURE':
        mesh.modifiers.remove(mod)
for obj in list(bpy.context.scene.objects):
    if obj != mesh:
        bpy.data.objects.remove(obj, do_unlink=True)
mesh.matrix_world = Matrix.Identity(4)
before = [list(v.co) for v in mesh.data.vertices]
existing = set(bpy.context.scene.objects)
bpy.ops.import_scene.fbx(filepath=str(RIG), use_anim=False)
rig_objects = set(bpy.context.scene.objects) - existing
for obj in rig_objects:
    if obj.parent is None and abs(obj.scale.x - 0.01) < 1e-5:
        for child in obj.children_recursive:
            child.location *= 0.01
            if child.type in {'ARMATURE', 'MESH'}:
                child.data.transform(Matrix.Scale(0.01, 4))
        obj.scale *= 100
bpy.context.view_layer.update()
arm = next(o for o in rig_objects if o.type == 'ARMATURE')
if 'Head' not in arm.data.bones:
    raise ValueError('Aligned rig has no Head')
for bone in arm.data.bones:
    bone.use_deform = bone.name == 'Head'
# The accessory loader searches only the shared Head subtree. Keep the Head
# rest matrix, but omit unused ancestor skin entries from the accessory FBX.
bpy.context.view_layer.objects.active = arm
bpy.ops.object.select_all(action='DESELECT')
arm.select_set(True)
bpy.ops.object.mode_set(mode='EDIT')
head = arm.data.edit_bones['Head']
head.parent = None
for bone in list(arm.data.edit_bones):
    if bone.name != 'Head':
        arm.data.edit_bones.remove(bone)
bpy.ops.object.mode_set(mode='OBJECT')
mesh.vertex_groups.clear()
group = mesh.vertex_groups.new(name='Head')
group.add(list(range(len(mesh.data.vertices))), 1.0, 'REPLACE')
mod = mesh.modifiers.new('BaseRig', 'ARMATURE')
mod.object = arm
world = mesh.matrix_world.copy()
mesh.parent = arm
mesh.matrix_world = world
bpy.context.view_layer.update()
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=str(OUT), use_selection=True,
    object_types={'ARMATURE', 'EMPTY', 'MESH'}, global_scale=1.0,
    apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
    axis_forward='-Z', axis_up='Y', add_leaf_bones=False,
    use_armature_deform_only=True, bake_anim=False, use_mesh_modifiers=False,
    path_mode='AUTO', embed_textures=False)
print('ACCESSORY_REBIND=' + json.dumps({
    'vertices':len(before), 'head':list(arm.matrix_world @ arm.data.bones['Head'].head_local),
    'bounds_min':[min(v[i] for v in before) for i in range(3)],
    'bounds_max':[max(v[i] for v in before) for i in range(3)],
    'output':str(OUT)}))
