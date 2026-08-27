using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skeleton → Face/Eye → Hair/Costume/Accessory → Secondary Motion → Animator Enable
/// </summary>
public class PlayerUnit : UnitBase
{
    private const string SkeletonPath = "CharacterParts/Skeleton_SD";
    private const string CostumePath = "CharacterParts/Costume_01 (Ver3)";
    private const string FacePath = "CharacterParts/Mesh_Face_00";

    public Animator animator;
    public SkinnedMeshRenderer skinnedMesh_Body;
    public SkinnedMeshRenderer skinMeshRen_Face;
    public Transform Root;

    private readonly List<GameObject> _createdObjects = new();
    private readonly Dictionary<string, Transform> _bonesByName = new();

    public override void CreateUnit()
    {
        DestroyUnit();

        GameObject skeletonPrefab = Resources.Load<GameObject>(SkeletonPath);
        if (skeletonPrefab == null)
        {
            Debug.LogError($"PlayerUnit: 공용 골격을 찾을 수 없습니다. Resources/{SkeletonPath}", this);
            return;
        }

        GameObject skeleton = Instantiate(skeletonPrefab, transform);
        skeleton.name = "Skeleton_SD";
        _createdObjects.Add(skeleton);

        animator = skeleton.GetComponentInChildren<Animator>(true);
        Root = FindTransformByName(skeleton.transform, "Root");
        if (Root == null)
        {
            Debug.LogError("PlayerUnit: Skeleton_SD에 Root 본이 없습니다.", this);
            return;
        }

        CacheBones(Root);
        skinnedMesh_Body = CreateAndBindPart(CostumePath, "Costume_01");
        skinMeshRen_Face = CreateAndBindPart(FacePath, "Face_00");
    }

    public override void DestroyUnit()
    {
        foreach (GameObject createdObject in _createdObjects)
        {
            if (createdObject != null)
            {
                Destroy(createdObject);
            }
        }

        _createdObjects.Clear();
        _bonesByName.Clear();
        animator = null;
        skinnedMesh_Body = null;
        skinMeshRen_Face = null;
        Root = null;
    }

    private SkinnedMeshRenderer CreateAndBindPart(string resourcePath, string instanceName)
    {
        GameObject partPrefab = Resources.Load<GameObject>(resourcePath);
        if (partPrefab == null)
        {
            Debug.LogError($"PlayerUnit: 파츠를 찾을 수 없습니다. Resources/{resourcePath}", this);
            return null;
        }

        GameObject part = Instantiate(partPrefab, transform);
        part.name = instanceName;
        _createdObjects.Add(part);

        foreach (Animator partAnimator in part.GetComponentsInChildren<Animator>(true))
        {
            partAnimator.enabled = false;
        }

        SkinnedMeshRenderer firstRenderer = null;
        foreach (SkinnedMeshRenderer renderer in part.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (BindToSkeleton(renderer))
            {
                firstRenderer ??= renderer;
            }
        }

        return firstRenderer;
    }

    private bool BindToSkeleton(SkinnedMeshRenderer renderer)
    {
        Transform[] sourceBones = renderer.bones;
        Transform[] mappedBones = new Transform[sourceBones.Length];

        for (int i = 0; i < sourceBones.Length; i++)
        {
            Transform sourceBone = sourceBones[i];
            if (sourceBone == null || !_bonesByName.TryGetValue(sourceBone.name, out mappedBones[i]))
            {
                renderer.enabled = false;
                Debug.LogWarning($"PlayerUnit: {renderer.name}의 본 '{sourceBone?.name}'을 Skeleton_SD에서 찾지 못했습니다.", renderer);
                return false;
            }
        }

        Transform mappedRootBone = null;
        if (renderer.rootBone != null && !_bonesByName.TryGetValue(renderer.rootBone.name, out mappedRootBone))
        {
            renderer.enabled = false;
            Debug.LogWarning($"PlayerUnit: {renderer.name}의 Root Bone '{renderer.rootBone.name}'을 찾지 못했습니다.", renderer);
            return false;
        }

        renderer.bones = mappedBones;
        renderer.rootBone = mappedRootBone;
        return true;
    }

    private void CacheBones(Transform root)
    {
        foreach (Transform bone in root.GetComponentsInChildren<Transform>(true))
        {
            if (!_bonesByName.ContainsKey(bone.name))
            {
                _bonesByName.Add(bone.name, bone);
            }
        }
    }

    private static Transform FindTransformByName(Transform root, string targetName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
            {
                return child;
            }
        }

        return null;
    }
}
