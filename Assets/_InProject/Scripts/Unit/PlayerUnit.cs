using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    protected UnitData unitData;
    protected UnitRuntimeRoot unitRuntimeRoot;

    public abstract void CreateUnit();
    public abstract void DestroyUnit();
}


public class PlayerUnit : UnitBase
{
    public override void CreateUnit()
    {
    }

    public override void DestroyUnit()
    {
    }
}
