
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    protected UnitData unitData;

    public abstract void CreateUnit();
    public abstract void DestroyUnit();
}