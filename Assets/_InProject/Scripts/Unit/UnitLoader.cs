using UnityEngine;

public class UnitLoader
{
    public UnitBase CreateUnit()
    {
        var goUnit = new GameObject();
        goUnit.name = "Unit_1";
        goUnit.layer = LayerMask.NameToLayer("Player");
        goUnit.transform.localPosition = Vector3.zero;
        goUnit.transform.localRotation = Quaternion.identity;
        goUnit.transform.localScale = Vector3.one;
        
        var unit = goUnit.AddComponent<PlayerUnit>();
        unit.CreateUnit();

        return unit;
    }
}
