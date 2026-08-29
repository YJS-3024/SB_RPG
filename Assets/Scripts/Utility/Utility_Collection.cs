using System.Collections.Generic;
using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// 배열과 리스트에서 자주 사용하는 컬렉션 유틸리티입니다.
    /// </summary>
    public static class CollectionUtility
    {
        /// <summary>
        /// 리스트가 null이 아니고 하나 이상의 요소를 가지고 있는지 확인합니다.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// 인덱스가 유효하면 요소를 반환하고, 아니면 기본값을 반환합니다.
        /// </summary>
        public static T GetOrDefault<T>(this IList<T> list, int index, T defaultValue = default)
        {
            return list != null && index >= 0 && index < list.Count ? list[index] : defaultValue;
        }

        /// <summary>
        /// 리스트에서 랜덤 요소를 반환합니다. 비어 있으면 기본값을 반환합니다.
        /// </summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            return list.IsNullOrEmpty() ? default : list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// 리스트에서 랜덤 요소를 꺼내고 제거합니다. 비어 있으면 기본값을 반환합니다.
        /// </summary>
        public static T PopRandom<T>(this IList<T> list)
        {
            if (list.IsNullOrEmpty())
                return default;

            int index = Random.Range(0, list.Count);
            T value = list[index];
            list.RemoveAt(index);
            return value;
        }

        /// <summary>
        /// Fisher-Yates 방식으로 리스트 순서를 무작위로 섞습니다.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null)
                return;

            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        /// <summary>
        /// 리스트에서 null 참조 요소를 제거합니다.
        /// </summary>
        public static void RemoveNulls<T>(this IList<T> list) where T : class
        {
            if (list == null)
                return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                    list.RemoveAt(i);
            }
        }

        /// <summary>
        /// 조건에 맞는 첫 요소의 인덱스를 반환하고, 없으면 -1을 반환합니다.
        /// </summary>
        public static int IndexOf<T>(this IList<T> list, System.Predicate<T> match)
        {
            if (list == null || match == null)
                return -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                    return i;
            }

            return -1;
        }
    }
}
