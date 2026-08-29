using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// 게임 로직에서 자주 사용하는 수학 계산 유틸리티입니다.
    /// </summary>
    public static class MathUtility
    {
        /// <summary>
        /// 값을 기존 범위에서 새 범위로 변환합니다.
        /// </summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            if (Mathf.Approximately(fromMin, fromMax))
                return toMin;

            float t = (value - fromMin) / (fromMax - fromMin);
            return Mathf.LerpUnclamped(toMin, toMax, t);
        }

        /// <summary>
        /// 값을 기존 범위에서 새 범위로 변환하고 결과를 새 범위 안으로 제한합니다.
        /// </summary>
        public static float RemapClamped(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            if (Mathf.Approximately(fromMin, fromMax))
                return toMin;

            float t = Mathf.Clamp01((value - fromMin) / (fromMax - fromMin));
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// NaN이나 무한대 값을 0으로 처리한 뒤 0에서 1 사이로 제한합니다.
        /// </summary>
        public static float Clamp01Safe(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Clamp01(value);
        }

        /// <summary>
        /// 값이 0에 가까운지 확인합니다.
        /// </summary>
        public static bool ApproximatelyZero(float value, float epsilon = 0.0001f)
        {
            return Mathf.Abs(value) <= epsilon;
        }

        /// <summary>
        /// 값이 최소값과 최대값 사이에 있는지 확인합니다.
        /// </summary>
        public static bool IsBetween(float value, float min, float max, bool inclusive = true)
        {
            return inclusive ? value >= min && value <= max : value > min && value < max;
        }

        /// <summary>
        /// 현재값과 최대값을 기준으로 0에서 1 사이 비율을 반환합니다.
        /// </summary>
        public static float Percent(float current, float max)
        {
            if (max <= 0f)
                return 0f;

            return Mathf.Clamp01(current / max);
        }

        /// <summary>
        /// 값을 지정한 간격에 맞춰 반올림합니다.
        /// </summary>
        public static float Snap(float value, float step)
        {
            if (step <= 0f)
                return value;

            return Mathf.Round(value / step) * step;
        }

        /// <summary>
        /// 인덱스를 0 이상 길이 미만 범위로 순환시킵니다.
        /// </summary>
        public static int LoopIndex(int index, int length)
        {
            if (length <= 0)
                return 0;

            return ((index % length) + length) % length;
        }

        /// <summary>
        /// 시간 값을 0에서 1 사이로 왕복 반복하는 값으로 변환합니다.
        /// </summary>
        public static float PingPong01(float time)
        {
            return Mathf.PingPong(time, 1f);
        }
    }
}
