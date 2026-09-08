using DevelopKit;
using UnityEngine;

public abstract class BaseParts : MonoBehaviour
{
    public eUnitParts PartsType;
    public int SelectPartsIndex;
    public abstract bool IsValidIndex(int index);

    public void AttachTo(Transform parent)
    {
        if (parent != null)
            this.SetParents(parent);
    }
}
