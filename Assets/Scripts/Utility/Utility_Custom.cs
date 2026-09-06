using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DevelopKit
{
    public static class Utility_Custom
    {
        public static Transform FindChildCheck(this Transform parents, Predicate<Transform> onCheck)
        {
            var childs = parents.GetComponentsInChildren<Transform>(true);
            foreach (var childTf in childs)
            {
                if (onCheck(childTf))
                    return childTf;
            }

            return null;
        }

        public static void SetParents(this MonoBehaviour target, Transform parentTf)
        {
            if (target == null || parentTf == null)
                return;

            target.transform.SetParent(parentTf);
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;
        }

        public static void SetActive(this MonoBehaviour mono, bool isActive)
        {
            if (mono != null)
                mono.SetActive(isActive);
        }

        public static void SetActive(this Component component, bool isActive)
        {
            if (component != null)
                component.SetActive(isActive);
        }

        public static void ResistCoroutine(this MonoBehaviour mono, IEnumerator onCallback, ref Coroutine coroutine)
        {
            if(coroutine != null)
            {
                mono.StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = mono.StartCoroutine(onCallback);
        }
    }
}
