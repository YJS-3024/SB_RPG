using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace yjs.DevKit
{
    public class FinderMissingReference : EditorWindow
    {
        private readonly List<FindGoInfo> findList = new();
        private Vector2 _scrollPos = Vector2.zero;

        private void OnGUI()
        {
            Display();
        }

        [MenuItem("Utility/Tools/FinderMissingReference &m")]
        public static void OpenWindow()
        {
            var window = GetWindow<FinderMissingReference>(true, "FinderMissingReferences");
            window.minSize = window.maxSize = new Vector2(400f, 600f);
            window.Show();
        }

        private void Display()
        {
            EditorGUILayout.BeginVertical();
            {
                ReSearch();

                //  ?????? UISprite?? ????
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                if (findList.Count != 0)
                {
                    //  ?????? ????
                    var viewCount = 40;
                    //  ?? ?????? ????
                    var culumnHeight = 20;
                    //  ?? ?????? ??????
                    var firstIndex = (int)_scrollPos.y / culumnHeight;
                    firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, findList.Count - 1));

                    //  ???? ????
                    GUILayout.Space(firstIndex * culumnHeight);

                    //  ???????? ??????
                    var count = Mathf.Min(findList.Count, firstIndex + viewCount);
                    for (var i = firstIndex; i < count; i++) HorizontalColumn(findList[i]);

                    //  ?????? ????
                    GUILayout.Space(Mathf.Max(0, findList.Count - firstIndex - viewCount) * culumnHeight);
                }

                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void ReSearch()
        {
            if (GUILayout.Button("SearchComponent"))
            {
                findList.Clear();

                var arrGo = FindObjectsByType<GameObject>();
                foreach (var go in arrGo)
                {
                    var so = new SerializedObject(go);
                    var it = so.GetIterator();
                    while (it.Next(true))
                    {
                        if (it.propertyType != SerializedPropertyType.ObjectReference)
                            continue;

                        if (it.objectReferenceValue == null && it.objectReferenceEntityIdValue != EntityId.None)
                            findList.Add(new FindGoInfo(findList.Count + 1, go));
                    }
                }
            }
        }


        private void HorizontalColumn(FindGoInfo info)
        {
            EditorGUILayout.BeginHorizontal();
            {
                //  ????
                var style = new GUIStyle();
                style.wordWrap = true;
                style.alignment = TextAnchor.MiddleRight;
                EditorGUILayout.LabelField($" <color=#ffffff>{info.No}</color> ", style,
                    GUILayout.Width(40),
                    GUILayout.ExpandHeight(false));

                //  ?????????? ?????? ???? ????????\
                var arrOptions = new[] { GUILayout.MaxWidth(400), GUILayout.ExpandHeight(false) };
                EditorGUILayout.ObjectField(info.Go, typeof(GameObject), false, arrOptions);
            }
            EditorGUILayout.EndHorizontal();
        }

        public class FindGoInfo
        {
            public readonly GameObject Go;
            public readonly int No;

            public FindGoInfo(int no, GameObject go)
            {
                No = no;
                Go = go;
            }
        }
    }
}

#endif