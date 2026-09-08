using System.Collections.Generic;
using UnityEngine;

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
    private List<KeyValuePair<eUnitParts, string>> PartsPathList = new()
    {
        new KeyValuePair<eUnitParts, string>(eUnitParts.RootBone, "BaseRig"),
        new KeyValuePair<eUnitParts, string>(eUnitParts.BodyCostume, "CharacterParts/body_BunnyGirl"),
        new KeyValuePair<eUnitParts, string>(eUnitParts.Face, "CharacterParts/face_Skin"),
        new KeyValuePair<eUnitParts, string>(eUnitParts.FaceEyeL, "CharacterParts/face_Eye_L"),
        new KeyValuePair<eUnitParts, string>(eUnitParts.FaceEyeR, "CharacterParts/face_Eye_R"),
        new KeyValuePair<eUnitParts, string>(eUnitParts.Hair_Front, "CharacterParts/hair_front"),
        new KeyValuePair<eUnitParts, string>(eUnitParts.Hair_Back, "CharacterParts/hair_back")
    };

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
        animator.transform.localPosition = Vector3.zero;
        animator.transform.localScale = Vector3.one;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        bodySkin.SetBones(rootBone,parentsTf: animator.transform);
        headAccSkin.SetBones(rootHead, parentsTf: rootBone);

        faceSkin.SetBones(rootHead, parentsTf: rootHead);
        eyeMeshs_L.SetRender(rootEyeL);
        eyeMeshs_R.SetRender(rootEyeR);
        hairSkin_front.SetHair(rootHead);
        hairSkin_base.SetHair(rootHead);

    }


    public override void DestroyUnit()
    {

    }
}
