using System.Collections.Generic;
using DevelopKit;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    private List<UIPanel> panels = new List<UIPanel>();
    private List<UIPopup> popups = new List<UIPopup>();

    private List<UIPanel> _panelStack = new List<UIPanel>();
    private Stack<UIPopup> _popupStack = new Stack<UIPopup>();

    public override bool Initialize()
    {
        return true;
    }

    protected override void Destroy()
    {

    }

    public UIPanel ChangePanel(eUI uiType, params object[] p)
    {
        if (eUI.Panel_Start <= uiType || uiType <= eUI.Panel_End)
            return null;

        return null;
    }

    public UIPopup ShowPopup(eUI uiType, params object[] p)
    {
        if (eUI.Popup_Start <= uiType || uiType <= eUI.Popup_End)
            return null;

        return null;
    }
    
    public void HidePopup()
    {
        if (_popupStack.Count == 0)
            return;
    }
}

public abstract class UIPanel : UIBase
{
}

public abstract class UIPopup : UIBase
{
    public Image imgBackground;
    public GameObject goPopup;
}