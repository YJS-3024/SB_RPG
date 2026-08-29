using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// 색상 변환과 보간에 사용하는 유틸리티입니다.
    /// </summary>
    public static class ColorUtility
    {
        /// <summary>
        /// HTML 색상 문자열을 Color로 변환합니다. 실패하면 기본값을 반환합니다.
        /// </summary>
        public static Color FromHtml(string html, Color defaultColor = default)
        {
            return UnityEngine.ColorUtility.TryParseHtmlString(html, out Color color) ? color : defaultColor;
        }

        /// <summary>
        /// Color를 HTML RGB 문자열로 변환합니다.
        /// </summary>
        public static string ToHtmlRGB(Color color)
        {
            return UnityEngine.ColorUtility.ToHtmlStringRGB(color);
        }

        /// <summary>
        /// Color를 HTML RGBA 문자열로 변환합니다.
        /// </summary>
        public static string ToHtmlRGBA(Color color)
        {
            return UnityEngine.ColorUtility.ToHtmlStringRGBA(color);
        }

        /// <summary>
        /// 색상의 알파 값만 변경한 새 색상을 반환합니다.
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        /// <summary>
        /// 색상의 RGB 값은 유지하고 알파 값을 곱한 새 색상을 반환합니다.
        /// </summary>
        public static Color MultiplyAlpha(this Color color, float multiplier)
        {
            color.a = Mathf.Clamp01(color.a * multiplier);
            return color;
        }

        /// <summary>
        /// 두 색상을 보간하고 진행률은 0에서 1 사이로 제한합니다.
        /// </summary>
        public static Color LerpClamped(Color from, Color to, float t)
        {
            return Color.Lerp(from, to, Mathf.Clamp01(t));
        }
    }
}
