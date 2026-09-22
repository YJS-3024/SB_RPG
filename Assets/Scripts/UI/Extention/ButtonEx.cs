using System;
using System.Collections;
using System.Linq;
using DevelopKit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class ButtonEx : Button, IEndDragHandler
{
    private TextMeshProUGUI txtName;
    public TextMeshProUGUI Text
    {
        get => txtName;
        set => txtName = value;
    }

    [Header("하위 오브젝트의 색변경을 무시할 UI들")]
    [SerializeField] private Graphic[] arrIgnoreGraphics;

    [SerializeField] private CanvasGroup[] arrPressActiveGo;

    [Header("지정색으로 하위 오브젝트들 변경")]
    [SerializeField] private Color color_NoInteractable = Color.white;
    [SerializeField] private Color color_Normal = Color.white;
    [SerializeField] private Color color_Selected = Color.white;
    [SerializeField] private Color color_Rollhover = Color.white;

    [Header("지정색으로 하위 오브젝트들 변경_Text")]
    [SerializeField] private bool isUseInChangeText = false;
    [SerializeField] private Color color_Text_NonSelect = Color.white;
    [SerializeField] private Color color_Text_Selected = Color.white;

    private Action _onCancel;
    private Action _onClick;
    private Action _onHover;
    private Action _onPressDown;
    private Action _onPressUp;

    private bool _isPointerInside;
    private bool _isPointerPressed;
    private int _activePointerId;


    public bool IsSelect { private get; set; }
    public bool Interactable
    {
        get => interactable;
        set
        {
            interactable = value;
            UpdateVisualGraphic();
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (txtName == null)
            txtName = GetComponentInChildren<TextMeshProUGUI>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _onClick = null;
        _onHover = null;
        _onCancel = null;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        UpdateVisualGraphic();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        InvokePressUpOnce();
        UpdateVisualGraphic();

        _isPointerInside = false;
    }

    public void SetEvent(Action onClick = null, Action onHover = null, Action onCancel = null, Action onPressDown = null, Action onPressUp = null)
    {
        _onClick = onClick;
        _onHover = onHover;
        _onCancel = onCancel;
        _onPressDown = onPressDown;
        _onPressUp = onPressUp;
    }

    public void SetText(string text, Action onClick = null)
    {
        Text?.SetText(text);
        _onClick = onClick;
        this.SetActive(true);
    }

    /// <summary>
    /// 버튼 상태에 대한 VisualUpdate
    /// </summary>
    public void UpdateVisualGraphic()
    {
        if (transition != Transition.None)
            return;

        var graphics = GetComponentsInChildren<Graphic>(true);
        if (graphics == null)
            return;

        foreach (var g in graphics)
        {
            if (g == null)
                continue;

            if (arrIgnoreGraphics.Contains(g))
                continue;

            if (interactable == false)
            {
                g.color = color_NoInteractable;
            }
            else if (_isPointerInside)
            {
                g.color = color_Rollhover;
            }
            else if (IsSelect)
            {
                g.color = color_Selected;
            }
            else
            {
                g.color = color_Normal;
            }
        }

        if (arrPressActiveGo != null &&
            arrPressActiveGo.Length > 0)
        {
            foreach (var activeGo in arrPressActiveGo)
            {
                activeGo.alpha = interactable && _isPointerInside ? 1 : 0;
            }
        }
        
        if (txtName != null && isUseInChangeText)
        {
            txtName.color = _isPointerInside
                ? color_Text_Selected
                : color_Text_NonSelect;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if (_isPointerInside == false)
            _onHover?.Invoke();

        _isPointerInside = true;

        UpdateVisualGraphic();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        _isPointerInside = false;
        
        _onCancel?.Invoke();

        UpdateVisualGraphic();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        _isPointerPressed = true;
        _activePointerId = eventData.pointerId;
        _onPressDown?.Invoke();

        UpdateVisualGraphic();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        
      if (eventData.pointerId == _activePointerId)
          InvokePressUpOnce();

        UpdateVisualGraphic();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
      if (eventData.pointerId == _activePointerId)
          InvokePressUpOnce();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        _onClick?.Invoke();
        
        UpdateVisualGraphic();
    }

    /// <summary>
    /// 버튼 영역에서 클릭후 영역밖에서 땟을때를 체크
    /// </summary>
    private void InvokePressUpOnce()
    {
        if (!_isPointerPressed)
            return;

        _isPointerPressed = false;
        _activePointerId = int.MinValue;
        _onPressUp?.Invoke();

        UpdateVisualGraphic();
    }
}


#if UNITY_EDITOR
public partial class ButtonEx
{
    private const string ConvertMenuPath = "GameObject/UI/Convert Selected Button To ButtonEx";
    private const string ConvertContextMenuPath = "CONTEXT/Button/Convert To ButtonEx";

    [UnityEditor.MenuItem(ConvertMenuPath, false, 49)]
    private static void ConvertSelectedButtons()
    {
        var convertedCount = 0;

        foreach (var gameObject in UnityEditor.Selection.gameObjects)
        {
            if (gameObject == null)
                continue;

            var button = gameObject.GetComponent<Button>();
            if (button == null || button is ButtonEx)
                continue;

            Convert(button);
            convertedCount++;
        }

        Debug.Log($"Converted {convertedCount} Button component(s) to ButtonEx.");
    }

    [UnityEditor.MenuItem(ConvertMenuPath, true)]
    private static bool CanConvertSelectedButtons()
    {
        foreach (var gameObject in UnityEditor.Selection.gameObjects)
        {
            if (gameObject == null)
                continue;

            var button = gameObject.GetComponent<Button>();
            if (button != null && !(button is ButtonEx))
                return true;
        }

        return false;
    }

    [UnityEditor.MenuItem(ConvertContextMenuPath)]
    private static void ConvertContextButton(UnityEditor.MenuCommand command)
    {
        var button = command.context as Button;
        if (button == null || button is ButtonEx)
            return;

        Convert(button);
    }

    [UnityEditor.MenuItem(ConvertContextMenuPath, true)]
    private static bool CanConvertContextButton(UnityEditor.MenuCommand command)
    {
        var button = command.context as Button;
        return button != null && !(button is ButtonEx);
    }

    private static void Convert(Button button)
    {
        var gameObject = button.gameObject;
        var interactable = button.interactable;
        var transition = button.transition;
        var colors = button.colors;
        var spriteState = button.spriteState;
        var animationTriggers = button.animationTriggers;
        var targetGraphic = button.targetGraphic;
        var navigation = button.navigation;
        var onClick = button.onClick;

        UnityEditor.Undo.RegisterFullObjectHierarchyUndo(gameObject, "Convert Button To ButtonEx");
        UnityEditorInternal.ComponentUtility.CopyComponent(button);

        DestroyImmediate(button, true);
        var buttonEx = UnityEditor.Undo.AddComponent<ButtonEx>(gameObject);

        if (buttonEx != null)
        {
            UnityEditorInternal.ComponentUtility.PasteComponentValues(buttonEx);
            buttonEx.interactable = interactable;
            buttonEx.transition = transition;
            buttonEx.colors = colors;
            buttonEx.spriteState = spriteState;
            buttonEx.animationTriggers = animationTriggers;
            buttonEx.targetGraphic = targetGraphic;
            buttonEx.navigation = navigation;
            buttonEx.onClick = onClick;
        }

        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
}

[UnityEditor.CustomEditor(typeof(ButtonEx), true)]
[UnityEditor.CanEditMultipleObjects]
public class ButtonExEditor : UnityEditor.UI.ButtonEditor
{
    private UnityEditor.SerializedProperty m_Transition;
    private UnityEditor.SerializedProperty color_NoInteractable;
    private UnityEditor.SerializedProperty color_Normal;
    private UnityEditor.SerializedProperty color_Select;
    private UnityEditor.SerializedProperty color_Rollhover;

    private UnityEditor.SerializedProperty isUseInChangeText;
    private UnityEditor.SerializedProperty color_Text_NonSelect;
    private UnityEditor.SerializedProperty color_Text_Selected;

    private UnityEditor.SerializedProperty arrIgnoreGraphics;
    private UnityEditor.SerializedProperty arrPressActiveGo;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_Transition = serializedObject.FindProperty("m_Transition");

        color_NoInteractable = serializedObject.FindProperty("color_NoInteractable");
        color_Normal = serializedObject.FindProperty("color_Normal");
        color_Select = serializedObject.FindProperty("color_Selected");
        color_Rollhover = serializedObject.FindProperty("color_Rollhover");

        isUseInChangeText = serializedObject.FindProperty("isUseInChangeText");
        color_Text_NonSelect = serializedObject.FindProperty("color_Text_NonSelect");
        color_Text_Selected = serializedObject.FindProperty("color_Text_Selected");

        arrIgnoreGraphics = serializedObject.FindProperty("arrIgnoreGraphics");
        arrPressActiveGo = serializedObject.FindProperty("arrPressActiveGo");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("ButtonEx", UnityEditor.EditorStyles.boldLabel);

        serializedObject.Update();
        if (!m_Transition.hasMultipleDifferentValues &&
            m_Transition.enumValueIndex == (int)UnityEngine.UI.Selectable.Transition.None)
        {
            SetPropertyField(color_NoInteractable);
            SetPropertyField(color_Normal);
            SetPropertyField(color_Select);
            SetPropertyField(color_Rollhover);

            SetPropertyField(isUseInChangeText);
            if (isUseInChangeText.boolValue == true)
            {
                SetPropertyField(color_Text_NonSelect);
                SetPropertyField(color_Text_Selected);
            }

            SetPropertyField(arrIgnoreGraphics);
            SetPropertyField(arrPressActiveGo);
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void SetPropertyField(UnityEditor.SerializedProperty property)
    {
        if(property != null)
        {
            UnityEditor.EditorGUILayout.PropertyField(property);
        }
    }
}
#endif
