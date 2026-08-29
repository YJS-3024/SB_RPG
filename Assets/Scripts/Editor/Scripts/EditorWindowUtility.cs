using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR


namespace yjs.DevKit
{
    public static class EditorWindowUtility
    {
        public static void VerticalLayout(Action content)
        {
            EditorGUILayout.BeginVertical();
            {
                content?.Invoke();
            }
            EditorGUILayout.EndVertical();
        }

        public static void HorizontalLayout(Action content)
        {
            EditorGUILayout.BeginHorizontal();
            {
                content?.Invoke();
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void LabelField(this string str)
        {
            EditorGUILayout.LabelField(str);
        }

        public static void HelpBox(string message, MessageType messageType = MessageType.Info)
        {
            EditorGUILayout.HelpBox(message, messageType);
        }

        // ... existing code ...
        public static T ObjectField<T>(this Object obj, string labelText, bool isAllow = false) where T : Object
        {
            var tObj = EditorGUILayout.ObjectField(labelText, obj, typeof(T), isAllow, GUILayout.ExpandWidth(true));
            return tObj as T;
        }

        public static void Button(string buttonText, Action onClick)
        {
            if (GUILayout.Button(buttonText))
                onClick?.Invoke();
        }

        public static void ToggleButton(string buttonText, bool isOn, Action onClick)
        {
            GUIStyle onStyle = new GUIStyle(GUI.skin.button);
            onStyle.normal.textColor = isOn ? Color.yellow : Color.white;
            if (GUILayout.Button(buttonText, onStyle))
                onClick?.Invoke();
        }

        public static void Toggle(string buttonText, bool isOn, Action<bool> onClick)
        {
            isOn = GUILayout.Toggle(isOn, buttonText);
            onClick?.Invoke(isOn);
        }

        public static Vector2 Scroll(Vector2 pos, Action onClick)
        {
            pos = EditorGUILayout.BeginScrollView(pos);
            {
                onClick?.Invoke();
            }
            EditorGUILayout.EndScrollView();
            return pos;
        }
    }
}

#endif