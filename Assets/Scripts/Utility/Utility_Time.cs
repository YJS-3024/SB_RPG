using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// Unity 시간 값, 쿨다운, 진행률, 시간 표기를 다루는 유틸리티입니다.
    /// </summary>
    public static class TimeUtility
    {
        /// <summary>
        /// 현재 시간을 반환합니다. useUnscaledTime이 true면 Time.unscaledTime을 사용합니다.
        /// </summary>
        public static float Now(bool useUnscaledTime = false)
        {
            return useUnscaledTime ? Time.unscaledTime : Time.time;
        }

        /// <summary>
        /// 프레임 간 시간을 반환합니다. useUnscaledTime이 true면 Time.unscaledDeltaTime을 사용합니다.
        /// </summary>
        public static float DeltaTime(bool useUnscaledTime = false)
        {
            return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        /// <summary>
        /// 마지막 사용 시각과 쿨다운을 기준으로 다시 사용할 수 있는지 확인합니다.
        /// </summary>
        public static bool IsCooldownReady(float lastUsedTime, float cooldown, bool useUnscaledTime = false)
        {
            return Now(useUnscaledTime) >= lastUsedTime + Mathf.Max(0f, cooldown);
        }

        /// <summary>
        /// 마지막 사용 시각과 쿨다운을 기준으로 남은 쿨다운 시간을 반환합니다.
        /// </summary>
        public static float GetCooldownRemaining(float lastUsedTime, float cooldown, bool useUnscaledTime = false)
        {
            return Mathf.Max(0f, lastUsedTime + Mathf.Max(0f, cooldown) - Now(useUnscaledTime));
        }

        /// <summary>
        /// 시작 시각에서 지정한 시간이 지났는지 확인합니다.
        /// </summary>
        public static bool HasElapsed(float startTime, float duration, bool useUnscaledTime = false)
        {
            return Now(useUnscaledTime) >= startTime + Mathf.Max(0f, duration);
        }

        /// <summary>
        /// 시작 시각부터 현재까지 지난 시간을 반환합니다.
        /// </summary>
        public static float GetElapsed(float startTime, bool useUnscaledTime = false)
        {
            return Mathf.Max(0f, Now(useUnscaledTime) - startTime);
        }

        /// <summary>
        /// 시작 시각과 전체 시간을 기준으로 0에서 1 사이 진행률을 반환합니다.
        /// </summary>
        public static float GetProgress01(float startTime, float duration, bool useUnscaledTime = false)
        {
            if (duration <= 0f)
                return 1f;

            return Mathf.Clamp01((Now(useUnscaledTime) - startTime) / duration);
        }

        /// <summary>
        /// 누적 시간이 지정한 간격에 도달했는지 확인합니다.
        /// </summary>
        public static bool IsIntervalTick(float elapsedTime, float interval)
        {
            if (interval <= 0f)
                return true;

            return elapsedTime >= interval;
        }

        /// <summary>
        /// 초 단위 시간을 MM:SS 문자열로 변환합니다.
        /// </summary>
        public static string FormatSeconds(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainSeconds = totalSeconds % 60;

            return $"{minutes:00}:{remainSeconds:00}";
        }
    }

    /// <summary>
    /// 쿨다운 상태를 간단히 보관하고 갱신하는 타이머입니다.
    /// </summary>
    public struct CooldownTimer
    {
        private readonly float cooldown;
        private readonly bool useUnscaledTime;
        private float lastUsedTime;

        /// <summary>
        /// 쿨다운 시간과 unscaled 시간 사용 여부를 지정해 타이머를 생성합니다.
        /// </summary>
        public CooldownTimer(float cooldown, bool useUnscaledTime = false)
        {
            this.cooldown = Mathf.Max(0f, cooldown);
            this.useUnscaledTime = useUnscaledTime;
            lastUsedTime = -this.cooldown;
        }

        /// <summary>
        /// 쿨다운이 끝나 다시 사용할 수 있는지 반환합니다.
        /// </summary>
        public bool IsReady => TimeUtility.IsCooldownReady(lastUsedTime, cooldown, useUnscaledTime);

        /// <summary>
        /// 남은 쿨다운 시간을 반환합니다.
        /// </summary>
        public float Remaining => TimeUtility.GetCooldownRemaining(lastUsedTime, cooldown, useUnscaledTime);

        /// <summary>
        /// 쿨다운 진행률을 0에서 1 사이 값으로 반환합니다.
        /// </summary>
        public float Progress01 => 1f - Mathf.Clamp01(Remaining / Mathf.Max(cooldown, float.Epsilon));

        /// <summary>
        /// 현재 시각을 마지막 사용 시각으로 기록해 쿨다운을 시작합니다.
        /// </summary>
        public void Reset()
        {
            lastUsedTime = TimeUtility.Now(useUnscaledTime);
        }

        /// <summary>
        /// 쿨다운이 즉시 완료된 상태로 만듭니다.
        /// </summary>
        public void Complete()
        {
            lastUsedTime = TimeUtility.Now(useUnscaledTime) - cooldown;
        }
    }
}
