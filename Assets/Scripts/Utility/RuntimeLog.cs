using System.Diagnostics;
using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// 빌드 타입에 따라 Unity 로그 출력을 제어하는 런타임 로그 유틸리티입니다.
    /// </summary>
    public static class RuntimeLog
    {
        /// <summary>
        /// 릴리즈 빌드에서는 Unity 기본 로그 출력을 비활성화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.unityLogger.logEnabled = true;
#else
            UnityEngine.Debug.unityLogger.logEnabled = false;
#endif
        }

        /// <summary>
        /// 에디터 또는 Development Build에서만 일반 로그를 출력합니다.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <summary>
        /// 에디터 또는 Development Build에서만 경고 로그를 출력합니다.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        /// <summary>
        /// 에디터 또는 Development Build에서만 에러 로그를 출력합니다.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Error(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        /// <summary>
        /// 로그 비활성화 상태와 관계없이 일반 로그를 강제로 출력합니다.
        /// </summary>
        public static void ForceLog(object message)
        {
            var previous = UnityEngine.Debug.unityLogger.logEnabled;
            UnityEngine.Debug.unityLogger.logEnabled = true;
            UnityEngine.Debug.Log(message);
            UnityEngine.Debug.unityLogger.logEnabled = previous;
        }

        /// <summary>
        /// 로그 비활성화 상태와 관계없이 경고 로그를 강제로 출력합니다.
        /// </summary>
        public static void ForceWarning(object message)
        {
            var previous = UnityEngine.Debug.unityLogger.logEnabled;
            UnityEngine.Debug.unityLogger.logEnabled = true;
            UnityEngine.Debug.LogWarning(message);
            UnityEngine.Debug.unityLogger.logEnabled = previous;
        }

        /// <summary>
        /// 로그 비활성화 상태와 관계없이 에러 로그를 강제로 출력합니다.
        /// </summary>
        public static void ForceError(object message)
        {
            var previous = UnityEngine.Debug.unityLogger.logEnabled;
            UnityEngine.Debug.unityLogger.logEnabled = true;
            UnityEngine.Debug.LogError(message);
            UnityEngine.Debug.unityLogger.logEnabled = previous;
        }
    }
}
