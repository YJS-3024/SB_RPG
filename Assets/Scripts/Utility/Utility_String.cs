using System;
using System.Text;

namespace yjs.DevKit
{
    /// <summary>
    /// 문자열 검사, 변환, 경로 조합에 사용하는 유틸리티입니다.
    /// </summary>
    public static class StringUtility
    {
        /// <summary>
        /// 문자열이 null, 빈 문자열, 공백 문자열인지 확인합니다.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 문자열이 비어 있으면 대체 문자열을 반환합니다.
        /// </summary>
        public static string OrDefault(this string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// 문자열이 지정한 최대 길이를 넘으면 잘라서 반환합니다.
        /// </summary>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || maxLength < 0 || value.Length <= maxLength)
                return value;

            return value[..maxLength];
        }

        /// <summary>
        /// 문자열의 첫 글자를 대문자로 바꿉니다.
        /// </summary>
        public static string ToUpperFirst(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return char.ToUpperInvariant(value[0]) + value[1..];
        }

        /// <summary>
        /// 문자열 앞뒤의 지정 문자열을 제거합니다.
        /// </summary>
        public static string TrimText(this string value, string trimText)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(trimText))
                return value;

            while (value.StartsWith(trimText, StringComparison.Ordinal))
                value = value[trimText.Length..];

            while (value.EndsWith(trimText, StringComparison.Ordinal))
                value = value[..^trimText.Length];

            return value;
        }

        /// <summary>
        /// 경로 조각을 슬래시 기준 경로로 합칩니다.
        /// </summary>
        public static string CombinePath(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            var builder = new StringBuilder();
            for (int i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                    continue;

                if (builder.Length > 0)
                    builder.Append('/');

                builder.Append(paths[i].Replace('\\', '/').Trim('/'));
            }

            return builder.ToString();
        }
    }
}
