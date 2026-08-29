using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// 코루틴 대기, 지연 실행, 안전 중지를 보조하는 비동기 유틸리티입니다.
    /// </summary>
    public static class AsyncUtility
    {
        private static readonly Dictionary<float, WaitForSeconds> WaitCache = new();
        private static readonly Dictionary<float, WaitForSecondsRealtime> RealtimeWaitCache = new();
        private static readonly WaitForEndOfFrame EndOfFrame = new();
        private static readonly WaitForFixedUpdate FixedUpdate = new();

        /// <summary>
        /// 지정 시간만큼 대기하는 WaitForSeconds를 캐시해서 반환합니다.
        /// </summary>
        public static WaitForSeconds Wait(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            if (!WaitCache.TryGetValue(seconds, out WaitForSeconds wait))
            {
                wait = new WaitForSeconds(seconds);
                WaitCache[seconds] = wait;
            }

            return wait;
        }

        /// <summary>
        /// 시간 스케일 영향을 받지 않는 WaitForSecondsRealtime을 캐시해서 반환합니다.
        /// </summary>
        public static WaitForSecondsRealtime WaitRealtime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            if (!RealtimeWaitCache.TryGetValue(seconds, out WaitForSecondsRealtime wait))
            {
                wait = new WaitForSecondsRealtime(seconds);
                RealtimeWaitCache[seconds] = wait;
            }

            return wait;
        }

        /// <summary>
        /// 프레임 끝까지 대기하는 객체를 반환합니다.
        /// </summary>
        public static WaitForEndOfFrame WaitEndOfFrame()
        {
            return EndOfFrame;
        }

        /// <summary>
        /// 다음 고정 업데이트까지 대기하는 객체를 반환합니다.
        /// </summary>
        public static WaitForFixedUpdate WaitFixedUpdate()
        {
            return FixedUpdate;
        }

        /// <summary>
        /// 지정한 MonoBehaviour에서 코루틴을 안전하게 시작합니다.
        /// </summary>
        public static Coroutine StartSafe(this MonoBehaviour owner, IEnumerator routine)
        {
            return owner != null && routine != null ? owner.StartCoroutine(routine) : null;
        }

        /// <summary>
        /// 지정한 MonoBehaviour에서 코루틴을 안전하게 중지합니다.
        /// </summary>
        public static void StopSafe(this MonoBehaviour owner, Coroutine coroutine)
        {
            if (owner != null && coroutine != null)
                owner.StopCoroutine(coroutine);
        }

        /// <summary>
        /// 지정 시간 뒤에 동작을 실행하는 코루틴을 반환합니다.
        /// </summary>
        public static IEnumerator Delay(float seconds, Action action, bool useUnscaledTime = false)
        {
            if (useUnscaledTime)
                yield return WaitRealtime(seconds);
            else
                yield return Wait(seconds);

            action?.Invoke();
        }

        /// <summary>
        /// 조건이 참이 될 때까지 대기하는 코루틴을 반환합니다.
        /// </summary>
        public static IEnumerator WaitUntil(Func<bool> predicate)
        {
            if (predicate == null)
                yield break;

            while (!predicate())
                yield return null;
        }

        /// <summary>
        /// 조건이 거짓이 될 때까지 대기하는 코루틴을 반환합니다.
        /// </summary>
        public static IEnumerator WaitWhile(Func<bool> predicate)
        {
            if (predicate == null)
                yield break;

            while (predicate())
                yield return null;
        }

        /// <summary>
        /// WaitForSeconds 캐시를 비웁니다.
        /// </summary>
        public static void ClearCache()
        {
            WaitCache.Clear();
            RealtimeWaitCache.Clear();
        }
    }
}
