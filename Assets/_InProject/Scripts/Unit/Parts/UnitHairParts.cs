using DevelopKit;
using Unity.Animations.SpringBones;
using UnityEngine;

[System.Serializable]
public struct MultiHairParts
{
    public SpringManager hairPartRoot;
    public SkinnedMeshRenderer hairSkin;
    public Material[] mainMaterials;
    public Material[] subMaterials;
}

public class UnitHairParts : BaseParts
{
    public MultiHairParts[] multiHairParts;

    public override bool IsValidIndex(int index)
    {
        return index < multiHairParts.Length;
    }

    public void SetHair(Transform parentsTf, int index = 0)
    {
        AttachTo(parentsTf);

        if (multiHairParts == null || multiHairParts.Length == 0)
            return;

        foreach (var hair in multiHairParts)
            hair.hairPartRoot.gameObject.SetActive(false);

        multiHairParts[index].hairPartRoot.gameObject.SetActive(true);
    }
}
