#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace yjs.DevKit
{
    public class MissingReferenceFinder : EditorWindow
    {
        private const float RowHeight = 22f;
        private const int ViewCount = 40;

        private readonly List<FindGoInfo> findList = new();
        private Vector2 _scrollPos = Vector2.zero;
        private GUIStyle _numberStyle;

        private void OnGUI()
        {
            DrawToolbar();
            DrawResultList();
        }

        /// <summary>
        /// 현재 씬에서 Missing Reference를 찾는 에디터 창을 엽니다.
        /// </summary>
        [MenuItem("Utility/Tools/Missing Reference Finder &m")]
        public static void OpenWindow()
        {
            var window = GetWindow<MissingReferenceFinder>(true, "Missing Reference Finder");
            window.minSize = window.maxSize = new Vector2(520f, 600f);
            window.Show();
        }

        /// <summary>
        /// 검색 버튼과 결과 개수를 표시합니다.
        /// </summary>
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Search", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                    SearchMissingReferences();

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"Found: {findList.Count}", GUILayout.Width(90f));
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 결과가 많아도 에디터 창이 느려지지 않도록 가상화된 결과 목록을 그립니다.
        /// </summary>
        private void DrawResultList()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (findList.Count == 0)
            {
                EditorGUILayout.HelpBox("No missing references found.", MessageType.Info);
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
        /// 현재 씬의 모든 GameObject에서 Missing Reference가 있는 ObjectReference 프로퍼티를 검색합니다.
        /// </summary>
        private void SearchMissingReferences()
        {
            findList.Clear();

            var objects = Object.FindObjectsByType<GameObject>();
            foreach (var go in objects)
                AddMissingReferences(go);
        }

        /// <summary>
        /// GameObject에서 발견한 Missing Reference마다 결과를 하나씩 추가합니다.
        /// </summary>
        private void AddMissingReferences(GameObject go)
        {
            using var serializedObject = new SerializedObject(go);
            var iterator = serializedObject.GetIterator();

            while (iterator.NextVisible(true))
            {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (iterator.objectReferenceValue != null || iterator.objectReferenceEntityIdValue == EntityId.None)
                    continue;

                findList.Add(new FindGoInfo(findList.Count + 1, go, iterator.propertyPath));
            }
        }

        /// <summary>
        /// 결과 한 줄을 그리고 선택 및 Ping 동작을 제공합니다.
        /// </summary>
        private void DrawResultRow(FindGoInfo info)
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
                EditorGUILayout.LabelField(info.PropertyPath, GUILayout.MinWidth(120f));

                if (GUILayout.Button("Select", GUILayout.Width(58f)))
                    Selection.activeGameObject = info.Go;

                if (GUILayout.Button("Ping", GUILayout.Width(44f)))
                    EditorGUIUtility.PingObject(info.Go);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 창에 표시할 Missing Reference 결과 하나를 저장합니다.
        /// </summary>
        private readonly struct FindGoInfo
        {
            public readonly int No;
            public readonly GameObject Go;
            public readonly string PropertyPath;

            public FindGoInfo(int no, GameObject go, string propertyPath)
            {
                No = no;
                Go = go;
                PropertyPath = propertyPath;
            }
        }
    }
}

#endif