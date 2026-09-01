using System.Collections.Generic;
using System.Linq;
using DevelopKit;
using UnityEngine;

/// <summary>
/// 공용 골격에 캐릭터 파츠를 결합하고 파츠의 Animator를 비활성화한다.
/// </summary>
public class PlayerUnit : UnitBase
{
    private const string BaseBodyPath = "BaseRig";
    private const string AnimatorPath = "Anim/SD_AnimController";

    private const string CostumePath = "CharacterParts/body_BunnyGirl";
    private const string FacePath = "CharacterParts/Mesh_Face_00";

    public Animator animator;
    public SkinnedMeshRenderer skinnedMesh_Body;
    public SkinnedMeshRenderer skinMeshRen_Face;
    public Transform root;
    public Transform hips;
    public Transform costumes;

    private readonly List<GameObject> _createdObjects = new();
    private readonly Dictionary<string, Transform> _bonesByName = new();

    public override void CreateUnit()
    {
        var body = Resources.Load<GameObject>(BaseBodyPath);
        if (body == null)
        {
            Debug.LogError($"PlayerUnit: 공용 골격을 찾을 수 없습니다. Resources/{BaseBodyPath}", this);
            return;
        }

        var goBody = Instantiate(body, transform);
        goBody.name = $"Body_{0}";
        goBody.SetActive(false);
 
        if (animator == null)
        {
            animator = goBody.GetComponent<Animator>();
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(AnimatorPath);
            animator.ApplyBuiltinRootMotion();
        }

        root = goBody.transform.FindChildCheck(x => x.name == "Root");
        hips = root.FindChildCheck(x => x.name.Contains("Hips"));

        var costume = Resources.Load<CostumeSetting>(CostumePath);
        if (costume != null)
        {
            costume = Instantiate(costume, goBody.transform);
            costume.SetRootBone(root);
            costume.SetSkinMaterial(2);
        }

        goBody.SetActive(true);

        _createdObjects.Add(goBody);
    }

    public override void DestroyUnit()
    {

    }

    public void ReMappingBone(Transform targetRoot, SkinnedMeshRenderer skin)
    {
        if (targetRoot == null || skin == null)
        {
            Debug.LogError("ReMappingBone: targetRoot 또는 skin이 null입니다.", this);
            return;
        }

        var targetBones = targetRoot.GetComponentsInChildren<Transform>(true);
        var boneMap = targetBones
            .Where(x => x != null)
            .GroupBy(x => x.name)
            .ToDictionary(x => x.Key, x => x.First());

        skin.bones = skin.bones
            .Select(sourceBone =>
            {
                if (sourceBone == null)
                {
                    Debug.LogError($"코스튬 bone이 null입니다: {skin.name}", skin);
                    return null;
                }

                return boneMap.TryGetValue(sourceBone.name, out var targetBone)
                    ? targetBone
                    : null;
            })
            .ToArray();

        if (skin.rootBone != null &&
            boneMap.TryGetValue(skin.rootBone.name, out var targetRootBone))
        {
            skin.rootBone = targetRootBone;
        }
        else if (boneMap.TryGetValue("Hips", out var hipsBone))
        {
            skin.rootBone = hipsBone;
        }
        else
        {
            Debug.LogError($"코스튬 rootBone을 찾지 못했습니다: {skin.name}", skin);
        }
    }
}
