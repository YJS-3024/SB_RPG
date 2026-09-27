
#region [Enum]

using UnityEngine;

public enum UnitType
{
    None,
    Player,
    Enemy,
    Npc
}

public enum eUI
{
    None = 0,

    Panel_Start,
    Panel_End,

    Popup_Start,
    Popup_End,
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

public abstract class UIBase : MonoBehaviour
{
    public abstract void Init();

    public abstract void Show();

    public abstract void Hide();
}

#endregion [상속클래스]


#region [인터페이스]
public interface iData
{
    public int ID { get; }
}
#endregion [인터페이스]