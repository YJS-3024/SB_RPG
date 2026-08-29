using System;
using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// GameObject, Component, Transform에서 자주 사용하는 확장 메서드 모음입니다.
    /// </summary>
    public static class ComponentUtility
    {
        /// <summary>
        /// GameObject에서 컴포넌트를 가져오고, 없을 때 선택적으로 추가합니다.
        /// </summary>
        public static T GetComponent<T>(this GameObject go, bool addIfMissing) where T : UnityEngine.Component
        {
            if (go == null)
                return null;

            if (go.TryGetComponent<T>(out var component))
                return component;

            return addIfMissing ? go.AddComponent<T>() : null;
        }

        /// <summary>
        /// GameObject에서 컴포넌트를 가져오고, 없으면 새로 추가합니다.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
        {
            return go.GetComponent<T>(true);
        }

        /// <summary>
        /// Component가 붙어 있는 GameObject에서 컴포넌트를 가져오고, 없으면 새로 추가합니다.
        /// </summary>
        public static T GetOrAddComponent<T>(this UnityEngine.Component component) where T : UnityEngine.Component
        {
            return component == null ? null : component.gameObject.GetOrAddComponent<T>();
        }

        /// <summary>
        /// 컴포넌트를 가져오거나 추가한 뒤 반환값과 out 매개변수로 함께 전달합니다.
        /// </summary>
        public static T TryGetOrAddComponent<T>(this GameObject go, out T component) where T : UnityEngine.Component
        {
            component = go.GetOrAddComponent<T>();
            return component;
        }

        /// <summary>
        /// GameObject에 지정한 컴포넌트가 있는지 확인합니다.
        /// </summary>
        public static bool HasComponent<T>(this GameObject go) where T : UnityEngine.Component
        {
            return go != null && go.TryGetComponent<T>(out _);
        }

        /// <summary>
        /// GameObject의 레이어가 지정한 LayerMask에 포함되는지 확인합니다.
        /// </summary>
        public static bool IsInLayerMask(this GameObject go, LayerMask layerMask)
        {
            return go != null && (layerMask.value & (1 << go.layer)) != 0;
        }

        /// <summary>
        /// Transform 아래의 모든 직접 자식 GameObject를 제거합니다.
        /// </summary>
        public static void DestroyChildren(this Transform parent)
        {
            if (parent == null)
                return;

            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(child.gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        /// <summary>
        /// Transform의 로컬 위치, 회전, 크기를 기본값으로 초기화합니다.
        /// </summary>
        public static void ResetLocal(this Transform transform)
        {
            if (transform == null)
                return;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Transform의 직접 자식들을 순회하며 지정한 동작을 실행합니다.
        /// </summary>
        public static void ForEachChild(this Transform parent, Action<Transform> action)
        {
            if (parent == null || action == null)
                return;

            for (var i = 0; i < parent.childCount; i++)
                action.Invoke(parent.GetChild(i));
        }

        /// <summary>
        /// GameObject와 모든 하위 자식에게 레이어를 재귀적으로 적용합니다.
        /// </summary>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            if (go == null)
                return;

            go.layer = layer;

            foreach (Transform child in go.transform)
                child.gameObject.SetLayerRecursively(layer);
        }

        /// <summary>
        /// GameObject가 null이 아니고 상태가 다를 때만 활성 상태를 변경합니다.
        /// </summary>
        public static void SetActiveSafe(this GameObject go, bool active)
        {
            if (go != null && go.activeSelf != active)
                go.SetActive(active);
        }

        /// <summary>
        /// Component가 연결된 GameObject의 활성 상태를 안전하게 변경합니다.
        /// </summary>
        public static void SetActiveSafe(this UnityEngine.Component component, bool active)
        {
            if (component != null)
                component.gameObject.SetActiveSafe(active);
        }

        /// <summary>
        /// MonoBehaviour의 GameObject 활성 상태와 enabled 상태를 함께 설정합니다.
        /// </summary>
        public static void SetActive(this MonoBehaviour mono, bool isActive, bool isEnabled = true)
        {
            if (mono == null)
                return;

            mono.gameObject.SetActiveSafe(isActive);

            if (mono.enabled != isEnabled)
                mono.enabled = isEnabled;
        }
    }
}
