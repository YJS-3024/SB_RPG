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

public class UnitHairParts : MonoBehaviour
{
    public MultiHairParts[] multiHairParts; 

    public void SetHair(Transform parentsTf, int index = 0)
    {
        if (parentsTf != null)
            this.SetParents(parentsTf);

        if (multiHairParts == null || multiHairParts.Length == 0)
            return;

        foreach (var hair in multiHairParts)
            hair.hairPartRoot.gameObject.SetActive(false);

        multiHairParts[index].hairPartRoot.gameObject.SetActive(true);
    }
}
