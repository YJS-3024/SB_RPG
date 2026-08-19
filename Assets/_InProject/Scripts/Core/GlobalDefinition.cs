
#region [Enum]

using UnityEngine;

public enum UnitType
{
    None,
    Player,
    Enemy,
    Npc
}

#endregion [Enum]


#region [상수]
#endregion [상수]


#region [상속클래스]
public abstract class UnitData : iData
{
    protected int UnitID;
    public int ID => UnitID;
}
#endregion [상속클래스]


#region [인터페이스]
public interface iData
{
    public int ID { get; }
}
#endregion [인터페이스]