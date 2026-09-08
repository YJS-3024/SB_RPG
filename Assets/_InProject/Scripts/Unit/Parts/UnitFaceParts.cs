using DevelopKit;
using UnityEngine;

public class UnitFaceParts : BaseParts
{
    public SkinnedMeshRenderer faceSkin;
    public SkinnedMeshRenderer eyeSkin_L;
    public SkinnedMeshRenderer eyeSkin_R;
    public Material[] matFaceSkins;

    public override bool IsValidIndex(int index)
    {
        return index < matFaceSkins.Length;
    }

    public void SetFace(Transform rootHead)
    {
    }
}
