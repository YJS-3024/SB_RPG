#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace yjs.DevKit
{
    public class ComponentFinder : EditorWindow
    {
        private const float RowHeight = 22f;
        private const int ViewCount = 40;

        private readonly List<FindComponentInfo> findList = new();
        private string _componentTypeName = string.Empty;
        private Vector2 _scrollPos = Vector2.zero;
        private GUIStyle _numberStyle;

        private void OnGUI()
        {
            DrawToolbar();
            DrawResultList();
        }

        /// <summary>
        /// 현재 씬에서 특정 컴포넌트를 찾는 에디터 창을 엽니다.
        /// </summary>
        [MenuItem("Utility/Tools/Component Finder &c")]
        public static void OpenWindow()
        {
            var window = GetWindow<ComponentFinder>(true, "Component Finder");
            window.minSize = window.maxSize = new Vector2(620f, 600f);
            window.Show();
        }

        /// <summary>
        /// 컴포넌트 타입 입력창, 검색 버튼, 결과 개수를 표시합니다.
        /// </summary>
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                _componentTypeName = GUILayout.TextField(_componentTypeName, EditorStyles.toolbarTextField, GUILayout.MinWidth(180f));

                if (GUILayout.Button("Search", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                    SearchComponents();

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"Found: {findList.Count}", GUILayout.Width(90f));
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 결과가 많아도 에디터 창이 느려지지 않도록 MissingReferenceFinder와 같은 방식으로 목록을 그립니다.
        /// </summary>
        private void DrawResultList()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (findList.Count == 0)
            {
                EditorGUILayout.HelpBox("Enter a component type name and press Search.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            var firstIndex = Mathf.Clamp((int)(_scrollPos.y / RowHeight), 0, Mathf.Max(0, findList.Count - 1));
            GUILayout.Space(firstIndex * RowHeight);

            var count = Mathf.Min(findList.Count, firstIndex + ViewCount);
            for (var i = firstIndex; i < count; i++)
                DrawResultRow(findList[i]);

            GUILayout.Space(Mathf.Max(0, findList.Count - firstIndex - ViewCount) * RowHeight);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 입력된 타입 이름과 일치하는 컴포넌트를 현재 씬의 모든 GameObject에서 검색합니다.
        /// </summary>
        private void SearchComponents()
        {
            findList.Clear();
            _scrollPos = Vector2.zero;

            var componentType = FindComponentType(_componentTypeName);
            if (componentType == null)
            {
                Debug.LogWarning($"Component type not found: {_componentTypeName}");
                return;
            }

            var objects = UnityEngine.Object.FindObjectsByType<GameObject>();
            foreach (var go in objects)
                AddComponents(go, componentType);
        }

        /// <summary>
        /// 입력된 이름과 일치하는 Component 타입을 로드된 어셈블리에서 찾습니다.
        /// </summary>
        private static Type FindComponentType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            typeName = typeName.Trim();
            var directType = Type.GetType(typeName);
            if (IsComponentType(directType))
                return directType;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                foreach (var type in types)
                {
                    if (!IsComponentType(type))
                        continue;

                    if (type.Name == typeName || type.FullName == typeName)
                        return type;
                }
            }

            return null;
        }

        /// <summary>
        /// 타입이 Unity Component를 상속하는지 확인합니다.
        /// </summary>
        private static bool IsComponentType(Type type)
        {
            return type != null && typeof(Component).IsAssignableFrom(type);
        }

        /// <summary>
        /// GameObject에 있는 대상 컴포넌트를 결과 목록에 추가합니다.
        /// </summary>
        private void AddComponents(GameObject go, Type componentType)
        {
            var components = go.GetComponents(componentType);
            foreach (var component in components)
            {
                if (component == null)
                    continue;

                findList.Add(new FindComponentInfo(findList.Count + 1, go, component));
            }
        }

        /// <summary>
        /// 결과 한 줄을 그리고 선택 및 Ping 동작을 제공합니다.
        /// </summary>
        private void DrawResultRow(FindComponentInfo info)
        {
            _numberStyle ??= new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
                richText = true
            };

            EditorGUILayout.BeginHorizontal(GUILayout.Height(RowHeight));
            {
                EditorGUILayout.LabelField($"<color=#ffffff>{info.No}</color>", _numberStyle, GUILayout.Width(36f));
                EditorGUILayout.ObjectField(info.Go, typeof(GameObject), true, GUILayout.Width(220f));
                EditorGUILayout.ObjectField(info.Component, typeof(Component), true, GUILayout.MinWidth(180f));

                if (GUILayout.Button("Select", GUILayout.Width(58f)))
                    Selection.activeObject = info.Component;

                if (GUILayout.Button("Ping", GUILayout.Width(44f)))
                    EditorGUIUtility.PingObject(info.Component);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 창에 표시할 컴포넌트 검색 결과 하나를 저장합니다.
        /// </summary>
        private readonly struct FindComponentInfo
        {
            public readonly int No;
            public readonly GameObject Go;
            public readonly Component Component;

            public FindComponentInfo(int no, GameObject go, Component component)
            {
                No = no;
                Go = go;
                Component = component;
            }
        }
    }
}

#endif