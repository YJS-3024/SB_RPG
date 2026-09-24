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
public class PlayerUnit : UnitBase, IDamageable
{
    private const string BaseRigName = "BaseRig";
    private const string BodyCostumeName = "body_costume_BunnyGirl";
    private const string HeadAccessoryName = "head_acc_BunnyGirl";
    private const string FaceName = "face_Skin";
    private const string EyeLeftName = "face_Eye_L";
    private const string EyeRightName = "face_Eye_R";
    private const string HairFrontName = "hair_front";
    private const string HairBackName = "hair_back";
    private const string GreatswordName = "prf_Prop_R_GreatSword1";
    private const string RightDaggerName = "prf_Prop_R_TwinDagger1";
    private const string LeftDaggerName = "prf_Prop_L_TwinDagger1";
    private static readonly int HitParameter = Animator.StringToHash("Hit");
    private static readonly int DieParameter = Animator.StringToHash("Die");
    private static readonly int HitState = Animator.StringToHash("Hit");

    private GameObject _rigInstance;
    private Transform _rightHand;
    private Transform _leftHand;
    private GameObject _greatsword;
    private GameObject _rightDagger;
    private GameObject _leftDagger;
    private WeaponAnimationStyle _equippedWeaponStyle;
    private bool _weaponsVisible = true;
    [SerializeField, Min(1)] private int maxHealth = 100;

    private bool _isCreated;
    private bool _isDead;
    private int _currentHealth;

    public bool IsDead => _isDead;
    public bool IsAlive => _isCreated && !_isDead && _currentHealth > 0;
    public int CurrentHealth => _currentHealth;
    public bool IsHitReacting
    {
        get
        {
            if (_isDead || animator == null || animator.runtimeAnimatorController == null)
                return false;

            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HitState)
                return true;

            return animator.IsInTransition(0) &&
                animator.GetNextAnimatorStateInfo(0).shortNameHash == HitState;
        }
    }

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
        _isDead = false;
        _currentHealth = maxHealth;

        EnsureMovementComponents();

        if (!LoadRig())
        {
            DestroyUnit();
            return;
        }

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

        if (!LoadWeapons())
        {
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
        _rightHand = null;
        _leftHand = null;
        _greatsword = null;
        _rightDagger = null;
        _leftDagger = null;
        _weaponsVisible = true;
        animator = null;
        _isCreated = false;
        _isDead = false;
        _currentHealth = 0;
    }

    private void EnsureMovementComponents()
    {
        if (GetComponent<InputReader_Player>() == null)
            gameObject.AddComponent<InputReader_Player>();

        if (GetComponent<PlayerUnitMovement>() == null)
            gameObject.AddComponent<PlayerUnitMovement>();

        CapsuleCollider hitCollider = GetComponent<CapsuleCollider>();
        if (hitCollider == null)
            hitCollider = gameObject.AddComponent<CapsuleCollider>();

        hitCollider.center = new Vector3(0f, 1f, 0f);
        hitCollider.height = 2f;
        hitCollider.radius = 0.45f;
        hitCollider.isTrigger = true;
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
        _rightHand = FindChild(_rigInstance.transform, "Hand_R");
        _leftHand = FindChild(_rigInstance.transform, "Hand_L");

        if (animator != null && rootBone != null && rootHead != null && rootEyeL != null && rootEyeR != null && _rightHand != null && _leftHand != null)
            return true;

        RuntimeLog.Error("BaseRig is missing Animator or required bones.");
        DestroyUnit();
        return false;
    }

    private bool LoadWeapons()
    {
        _greatsword = CreateWeapon(GreatswordName, _rightHand);
        _rightDagger = CreateWeapon(RightDaggerName, _rightHand);
        _leftDagger = CreateWeapon(LeftDaggerName, _leftHand);
        if (_greatsword == null || _rightDagger == null || _leftDagger == null)
            return false;

        return EquipWeapon(GetComponent<PlayerUnitMovement>().WeaponStyle);
    }

    public bool EquipWeapon(WeaponAnimationStyle style)
    {
        if (_greatsword == null || _rightDagger == null || _leftDagger == null ||
            style < WeaponAnimationStyle.Unarmed || style > WeaponAnimationStyle.TwinDagger)
            return false;

        _equippedWeaponStyle = style;
        ApplyWeaponVisibility();
        GetComponent<PlayerUnitMovement>().SetWeaponStyle(style);
        return true;
    }

    public void SetWeaponsVisible(bool visible)
    {
        if (_weaponsVisible == visible)
            return;

        _weaponsVisible = visible;
        ApplyWeaponVisibility();
    }

    private void ApplyWeaponVisibility()
    {
        if (_greatsword != null)
            _greatsword.SetActive(_weaponsVisible && _equippedWeaponStyle == WeaponAnimationStyle.Greatsword);
        if (_rightDagger != null)
            _rightDagger.SetActive(_weaponsVisible && _equippedWeaponStyle == WeaponAnimationStyle.TwinDagger);
        if (_leftDagger != null)
            _leftDagger.SetActive(_weaponsVisible && _equippedWeaponStyle == WeaponAnimationStyle.TwinDagger);
    }

    private GameObject CreateWeapon(string assetName, Transform hand)
    {
        GameObject prefab = ResourceManager.Instance.LoadCached<GameObject>(
            eResourceType.Prefab_Unit_Weapon, assetName);
        if (prefab == null)
        {
            RuntimeLog.Error("PlayerUnit weapon load failed: " + assetName);
            return null;
        }

        return Instantiate(prefab, hand, false);
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


public bool ReceiveDamage(int damage, GameObject attacker, Vector3 hitPoint)
    {
        if (_isDead || damage <= 0)
            return false;

        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        if (_currentHealth == 0)
            return PlayDeath();

        return PlayHitReaction();
    }

public bool PlayHitReaction()
    {
        if (_isDead || animator == null)
            return false;

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Dash");
        animator.ResetTrigger("DashLeft");
        animator.ResetTrigger("DashRight");
        animator.SetTrigger(HitParameter);
        return true;
    }

    public bool PlayDeath()
    {
        if (_isDead || animator == null)
            return false;

        _isDead = true;
        animator.ResetTrigger(HitParameter);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Dash");
        animator.ResetTrigger("DashLeft");
        animator.ResetTrigger("DashRight");
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger(DieParameter);
        return true;
    }
}