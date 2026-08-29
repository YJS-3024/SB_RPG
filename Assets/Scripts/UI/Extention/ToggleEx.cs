using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleEx : MonoBehaviour
{
    public bool bGrayScale = false;

    public RectTransform Rect => (RectTransform)transform;
    public bool Interactable => _toggle?.interactable ?? true;

    private Toggle _toggle;
    public Toggle Toggle
    {
        get
        {
            if (_toggle is null)
            {
                _toggle = GetComponent<Toggle>();
            }

            return _toggle;
        }
    }

    private Text _text;
    public Text Text
    {
        get
        {
            if (_text is null)
            {
                _text = GetComponentInChildren<Text>();
            }

            return _text;
        }
    }

    private Action<bool> _onChanged = null;
    public Action<bool> OnChanged
    {
        set => _onChanged = value;
    }

    private void OnEnable()
    {
        Toggle.onValueChanged.AddListener((isOn) =>
        {
            _onChanged?.Invoke(isOn);
        });

        if (bGrayScale)
        {
            // var graphics = GetComponentsInChildren<MaskableGraphic>();
            // foreach (var graphic in graphics)
            // {
            //     var material = new Material(graphic.material);
            //     material.color = Interactable
            //         ? Color.gray
            //         : Color.white;
            //     graphic.material = material;
            // }
        }
    }

    private void OnDisable()
    {
        Toggle.onValueChanged.RemoveListener((isOn) =>
        {
            _onChanged?.Invoke(isOn);
        });
    }
}
