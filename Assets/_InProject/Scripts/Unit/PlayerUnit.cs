using System.Collections.Generic;
using System.Linq;
using DevelopKit;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 공용 골격에 캐릭터 파츠를 결합하고 파츠의 Animator를 비활성화한다.
/// </summary>
public class PlayerUnit : UnitBase
{
    private const string BaseBodyPath = "BaseRig";

    private const string CostumePath = "CharacterParts/body_BunnyGirl";
    private const string FacePath = "CharacterParts/Mesh_Face_00";

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

    private const float HeadForwardOffset = 0.1f;
    

    private readonly List<GameObject> _createdObjects = new();
    private readonly Dictionary<string, Transform> _bonesByName = new();
    private Vector3 _headBaseLocalPosition;
    private Vector3 _headForwardLocalOffset;
    private bool _headOffsetReady;

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
        headAccSkin.SetBones(rootHead, parentsTf: rootHead);

        faceSkin.SetBones(rootHead, parentsTf: rootHead);
        eyeMeshs_L.SetRender(rootEyeL);
        eyeMeshs_R.SetRender(rootEyeR);
        hairSkin_front.SetHair(rootHead);
        hairSkin_base.SetHair(rootHead);

        _headBaseLocalPosition = rootHead.localPosition;
        _headForwardLocalOffset = rootHead.parent.InverseTransformVector(
            animator.transform.forward * HeadForwardOffset);
        _headOffsetReady = true;
    }

    private void LateUpdate()
    {
        if (!_headOffsetReady || rootHead == null)
            return;

        rootHead.localPosition = _headBaseLocalPosition + _headForwardLocalOffset;
    }


    public override void DestroyUnit()
    {

    }
}
