using DevelopKit;
using UnityEngine;

[System.Serializable]
public struct MultiSkinParts
{
    public SkinnedMeshRenderer targetSkin;
    public Material[] matVariations;
}

public class UnitSkinParts : MonoBehaviour
{
    [SerializeField] private MultiSkinParts[] skinParts;
    private Material[] matVariations;
    private SkinnedMeshRenderer curSkin;
    private Material curMaterial;

    public int MatVariationCount => matVariations.Length;

    public void SetBones(Transform rootBone, int index = 0, Transform parentsTf = null)
    {
        if (parentsTf != null)
            this.SetParents(parentsTf);

        if (skinParts.Length == 0)
            return;

        foreach(var skin in skinParts)
            skin.targetSkin.gameObject.SetActive(false);

        curSkin = index == 0
            ? skinParts[0].targetSkin
            : skinParts[index].targetSkin;

        curSkin.rootBone = rootBone;
        curSkin.gameObject.SetActive(true);

        Transform[] childrens = rootBone.GetComponentsInChildren<Transform>(true);

        // sort bones.
        Transform[] bones = new Transform[curSkin.bones.Length];
        for (int boneOrder = 0; boneOrder < curSkin.bones.Length; boneOrder++)
        {
            bones[boneOrder] = System.Array.Find<Transform>(childrens, c => c.name == curSkin.bones[boneOrder].name);
        }
        curSkin.bones = bones;
    }
}
