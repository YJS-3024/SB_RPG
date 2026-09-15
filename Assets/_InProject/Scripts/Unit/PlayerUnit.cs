using UnityEngine;
using yjs.DevKit;

public enum eUnitParts
{
    None = 0,

    Body = 1,
    RootBone,
    BodyCostume,

    Head = 11,
    Face,
    FaceEyeL,
    FaceEyeR,
    Hair_Front,
    Hair_Back,

    Max,
}

/// <summary>
/// 공용 골격에 캐릭터 파츠를 결합하고 파츠의 Animator를 비활성화한다.
/// </summary>
public class PlayerUnit : UnitBase
{
    private const string BaseRigName = "BaseRig";
    private const string BodyCostumeName = "body_costume_BunnyGirl";
    private const string HeadAccessoryName = "head_acc_BunnyGirl";
    private const string FaceName = "face_Skin";
    private const string EyeLeftName = "face_Eye_L";
    private const string EyeRightName = "face_Eye_R";
    private const string HairFrontName = "hair_front";
    private const string HairBackName = "hair_back";

    private GameObject _rigInstance;
    private bool _isCreated;

    public Animator animator;
    public Transform rootBone;
    public Transform rootHead;
    public Transform rootEyeL;
    public Transform rootEyeR;

    public UnitSkinParts bodySkin;
    public UnitSkinParts faceSkin;
    public UnitMeshParts eyeMeshs_L;
    public UnitMeshParts eyeMeshs_R;

    public UnitSkinParts headAccSkin;
    public UnitHairParts hairSkin_front;
    public UnitHairParts hairSkin_base;

    private void Start()
    {
        CreateUnit();
    }

    public override void CreateUnit()
    {
        if (_isCreated)
            return;

        _isCreated = true;

        EnsureMovementComponents();

        if (!LoadRig())
            return;

        bodySkin = LoadPart<UnitSkinParts>(eResourceType.Prefab_Unit_Costume, BodyCostumeName);
        headAccSkin = LoadPart<UnitSkinParts>(eResourceType.Prefab_Unit_Accessory, HeadAccessoryName);
        faceSkin = LoadPart<UnitSkinParts>(eResourceType.Prefab_Unit_Skin, FaceName);
        eyeMeshs_L = LoadPart<UnitMeshParts>(eResourceType.Prefab_Unit_Skin, EyeLeftName);
        eyeMeshs_R = LoadPart<UnitMeshParts>(eResourceType.Prefab_Unit_Skin, EyeRightName);
        hairSkin_front = LoadPart<UnitHairParts>(eResourceType.Prefab_Unit_Skin, HairFrontName);
        hairSkin_base = LoadPart<UnitHairParts>(eResourceType.Prefab_Unit_Skin, HairBackName);

        if (bodySkin == null || headAccSkin == null || faceSkin == null ||
            eyeMeshs_L == null || eyeMeshs_R == null ||
            hairSkin_front == null || hairSkin_base == null)
        {
            RuntimeLog.Error("PlayerUnit parts load failed.");
            DestroyUnit();
            return;
        }

        animator.transform.localPosition = Vector3.zero;
        animator.transform.localScale = Vector3.one;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.applyRootMotion = false;

        bodySkin.SetBones(rootBone, parentsTf: animator.transform);
        headAccSkin.SetBones(rootHead, parentsTf: rootBone);
        faceSkin.SetBones(rootHead, parentsTf: rootHead);
        eyeMeshs_L.SetRender(rootEyeL);
        eyeMeshs_R.SetRender(rootEyeR);
        hairSkin_front.SetHair(rootHead);
        hairSkin_base.SetHair(rootHead);
    }

    public override void DestroyUnit()
    {
        if (_rigInstance != null)
            Destroy(_rigInstance);

        _rigInstance = null;
        _isCreated = false;
    }

    private void EnsureMovementComponents()
    {
        if (GetComponent<InputReader_Player>() == null)
            gameObject.AddComponent<InputReader_Player>();

        if (GetComponent<PlayerUnitMovement>() == null)
            gameObject.AddComponent<PlayerUnitMovement>();
    }

    private bool LoadRig()
    {
        GameObject prefab = ResourceManager.Instance.LoadCached<GameObject>(eResourceType.Prefab_Unit_Rig, BaseRigName);
        if (prefab == null)
            return false;

        _rigInstance = Instantiate(prefab, transform);
        animator = _rigInstance.GetComponent<Animator>();
        rootBone = FindChild(_rigInstance.transform, "Root");
        rootHead = FindChild(_rigInstance.transform, "Head");
        rootEyeL = FindChild(_rigInstance.transform, "Eye_L");
        rootEyeR = FindChild(_rigInstance.transform, "Eye_R");

        if (animator != null && rootBone != null && rootHead != null && rootEyeL != null && rootEyeR != null)
            return true;

        RuntimeLog.Error("BaseRig is missing Animator or required bones.");
        DestroyUnit();
        return false;
    }

    private T LoadPart<T>(eResourceType type, string assetName) where T : Component
    {
        GameObject prefab = ResourceManager.Instance.LoadCached<GameObject>(type, assetName);
        if (prefab == null)
            return null;

        GameObject instance = Instantiate(prefab, _rigInstance.transform);
        T part = instance.GetComponent<T>();
        if (part != null)
            return part;

        RuntimeLog.Error($"Resource has no {typeof(T).Name}: {assetName}");
        Destroy(instance);
        return null;
    }

    private static Transform FindChild(Transform root, string childName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}