"""Fit the BunnyGirl neck using Temp/NeckSeam/geometry.json exported from Unity.

Pass the unmodified, head-aligned FBX via --source; never use the fitted output.
The geometry export and source must describe the same mesh/bind pose.
"""
import argparse
import sys
import json
import math
import heapq
from collections import Counter, defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--source', type=Path, required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
source = args.source.resolve(strict=True)
if source == (ROOT / 'Temp/NeckSeam/body_BunnyGirl_fitted.fbx').resolve():
    raise ValueError('The fitted output cannot be used as input')
DATA = json.loads((ROOT / 'Temp/NeckSeam/geometry.json').read_text())


def boundaries(vertices, triangles):
    keys = [tuple(round(c, 6) for c in v) for v in vertices]
    edges = Counter()
    for i in range(0, len(triangles), 3):
        tri = [keys[k] for k in triangles[i:i+3]]
        for a, b in zip(tri, tri[1:] + tri[:1]):
            if a != b:
                edges[tuple(sorted((a, b)))] += 1
    adj = defaultdict(set)
    for (a, b), count in edges.items():
        if count == 1:
            adj[a].add(b)
            adj[b].add(a)
    seen, loops = set(), []
    for start in adj:
        if start in seen:
            continue
        pending, component = [start], []
        while pending:
            v = pending.pop()
            if v in seen:
                continue
            seen.add(v)
            component.append(v)
            pending.extend(adj[v] - seen)
        loops.append(component)
    return loops


report = {}
for name in ('body', 'face'):
    loops = boundaries(DATA[name+'Vertices'], DATA[name+'Triangles'])
    report[name] = [{'count': len(loop),
                     'min': [min(v[i] for v in loop) for i in range(3)],
                     'max': [max(v[i] for v in loop) for i in range(3)],
                     'vertices': loop if len(loop) < 40 else []} for loop in loops]
print('BOUNDARY_COUNTS=' + json.dumps({k: [x['count'] for x in v] for k, v in report.items()}))

import bpy
from mathutils import Matrix, Vector, kdtree
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(source), use_anim=False)
for obj in list(bpy.context.scene.objects):
    if obj.parent is None and abs(obj.scale.x - 0.01) < 1e-5:
        for child in obj.children_recursive:
            child.location *= 0.01
            if child.type in {'ARMATURE', 'MESH'}:
                child.data.transform(Matrix.Scale(0.01, 4))
        if obj.type == 'MESH':
            obj.data.transform(Matrix.Scale(0.01, 4))
        obj.scale *= 100
bpy.context.view_layer.update()
mesh = next(o for o in bpy.context.scene.objects if o.type == 'MESH')
body_loops = boundaries(DATA['bodyVertices'], DATA['bodyTriangles'])
face_loops = boundaries(DATA['faceVertices'], DATA['faceTriangles'])
body_rim = next(loop for loop in body_loops if len(loop) == 10 and min(v[2] for v in loop) > 0.64)
face_rim = next(loop for loop in face_loops if len(loop) == 8 and max(v[2] for v in loop) < 0.68)
face_center = sum((Vector(v) for v in face_rim), Vector()) / len(face_rim)
face_rim.sort(key=lambda v: math.atan2(v[1]-face_center.y, v[0]-face_center.x))
face_vectors = [Vector(v) for v in face_rim]
body_mid_y = (max(v[1] for v in body_rim)+min(v[1] for v in body_rim))/2
face_mid_y = (max(v[1] for v in face_rim)+min(v[1] for v in face_rim))/2
mean_shift = Vector((face_center.x, face_mid_y-body_mid_y,
    max(v[2] for v in face_rim)-max(v[2] for v in body_rim)))

def nearest_segment(point):
    best = None
    for i, a in enumerate(face_vectors):
        b = face_vectors[(i+1) % len(face_vectors)]
        t = max(0.0, min(1.0, (point-a).dot(b-a)/(b-a).length_squared))
        q = a.lerp(b,t)
        item = ((point-q).length_squared,q,i,t)
        if best is None or item[0] < best[0]:
            best = item
    return best

kd = kdtree.KDTree(len(mesh.data.vertices))
for v in mesh.data.vertices:
    kd.insert(v.co,v.index)
kd.balance()
rim = {}
for point in body_rim:
    co, index, distance = kd.find(Vector(point))
    if distance > 0.00005:
        raise ValueError(f'Unexpected mesh correspondence error: {distance}')
    rim[index] = nearest_segment(Vector(point)+mean_shift)
if len(rim) != 10:
    raise ValueError('Expected ten distinct body neck boundary vertices')

adj = defaultdict(list)
for e in mesh.data.edges:
    a,b=e.vertices
    length=(mesh.data.vertices[a].co-mesh.data.vertices[b].co).length
    adj[a].append((b,length)); adj[b].append((a,length))
distance={i:0.0 for i in rim}
pending=[(0.0,i) for i in rim]
heapq.heapify(pending)
width=0.04
while pending:
    d,i=heapq.heappop(pending)
    if d != distance[i]:
        continue
    for j,length in adj[i]:
        nd=d+length
        if nd < width and nd < distance.get(j,float('inf')):
            distance[j]=nd; heapq.heappush(pending,(nd,j))

old_normals=[n.vector.copy() for n in mesh.data.corner_normals]
face_normals=[]
for point in face_vectors:
    ids=[i for i,v in enumerate(DATA['faceVertices']) if (Vector(v)-point).length < 0.000005]
    face_normals.append(sum((Vector(DATA['faceNormals'][i]) for i in ids),Vector()).normalized())
head_group=mesh.vertex_groups.get('Head') or mesh.vertex_groups.new(name='Head')
changed={}
for i,d in distance.items():
    v=mesh.data.vertices[i]
    t=1-d/width
    blend=t*t*(3-2*t)
    if i in rim:
        v.co=rim[i][1]
    else:
        v.co += mean_shift*blend
    memberships=[(g.group,g.weight) for g in v.groups]
    old_head=sum(w for g,w in memberships if g==head_group.index)
    for g,w in memberships:
        if g != head_group.index:
            mesh.vertex_groups[g].add([i],w*(1-blend),'REPLACE')
    head_group.add([i],old_head*(1-blend)+blend,'REPLACE')
    changed[i]=blend
mesh.data.update()
new_normals=[]
for loop,n in zip(mesh.data.loops,old_normals):
    i=loop.vertex_index
    if i in rim:
        _,_,seg,t=rim[i]
        n=face_normals[seg].lerp(face_normals[(seg+1)%len(face_normals)],t).normalized()
    new_normals.append(n)
mesh.data.normals_split_custom_set(new_normals)
bpy.context.view_layer.update()
out=ROOT/'Temp/NeckSeam/body_BunnyGirl_fitted.fbx'
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=str(out),use_selection=True,object_types={'ARMATURE','EMPTY','MESH'},
    global_scale=1.0,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',
    axis_forward='-Z',axis_up='Y',add_leaf_bones=False,use_armature_deform_only=False,
    bake_anim=False,use_mesh_modifiers=False,path_mode='AUTO',embed_textures=False)
(ROOT/'Temp/NeckSeam/fit_report.json').write_text(json.dumps({
    'boundary_count':len(rim),'changed_vertices':len(changed),'mean_shift':list(mean_shift),
    'rim_targets':[list(x[1]) for x in rim.values()]}))
print('FIT_RESULT='+json.dumps({'rim':len(rim),'changed':len(changed),'shift':list(mean_shift),'output':str(out)}))
