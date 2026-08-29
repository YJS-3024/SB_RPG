using System;
using System.Collections.Generic;

namespace yjs.DevKit
{
    /// <summary>
    /// enum 파싱, 값 조회, 플래그 검사를 보조하는 유틸리티입니다.
    /// </summary>
    public static class EnumUtility
    {
        private static readonly Dictionary<Type, Array> ValuesCache = new();

        /// <summary>
        /// enum 타입의 모든 값을 캐시해서 반환합니다.
        /// </summary>
        public static T[] GetValues<T>() where T : struct, Enum
        {
            Type type = typeof(T);
            if (!ValuesCache.TryGetValue(type, out Array values))
            {
                values = Enum.GetValues(type);
                ValuesCache[type] = values;
            }

            var result = new T[values.Length];
            Array.Copy(values, result, values.Length);
            return result;
        }

        /// <summary>
        /// 문자열을 enum 값으로 변환합니다.
        /// </summary>
        public static bool TryParse<T>(string value, out T result, bool ignoreCase = true) where T : struct, Enum
        {
            return Enum.TryParse(value, ignoreCase, out result);
        }

        /// <summary>
        /// 문자열을 enum 값으로 변환하고 실패하면 기본값을 반환합니다.
        /// </summary>
        public static T ParseOrDefault<T>(string value, T defaultValue = default, bool ignoreCase = true) where T : struct, Enum
        {
            return TryParse(value, out T result, ignoreCase) ? result : defaultValue;
        }

        /// <summary>
        /// enum 값 중 하나를 무작위로 반환합니다.
        /// </summary>
        public static T RandomValue<T>() where T : struct, Enum
        {
            T[] values = GetValues<T>();
            return values.Length == 0 ? default : values[UnityEngine.Random.Range(0, values.Length)];
        }

        /// <summary>
        /// 플래그 enum 값에 지정한 플래그가 포함되어 있는지 확인합니다.
        /// </summary>
        public static bool HasFlagFast<T>(this T value, T flag) where T : struct, Enum
        {
            long valueNumber = Convert.ToInt64(value);
            long flagNumber = Convert.ToInt64(flag);
            return (valueNumber & flagNumber) == flagNumber;
        }

        /// <summary>
        /// enum 값 캐시를 비웁니다.
        /// </summary>
        public static void ClearCache()
        {
            ValuesCache.Clear();
        }
    }
}
