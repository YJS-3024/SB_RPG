using System.Collections.Generic;
using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// Resources 폴더 에셋 로드를 보조하는 유틸리티입니다.
    /// </summary>
    public static class ResourceUtility
    {
        private static readonly Dictionary<string, Object> Cache = new();

        /// <summary>
        /// 지정한 경로에서 Resources 에셋을 로드합니다.
        /// </summary>
        public static T Load<T>(string path) where T : Object
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return Resources.Load<T>(NormalizePath(path));
        }

        /// <summary>
        /// 지정한 경로에서 Resources 에셋을 로드하고 실패하면 경고 로그를 출력합니다.
        /// </summary>
        public static T LoadOrWarn<T>(string path) where T : Object
        {
            T asset = Load<T>(path);
            if (asset == null)
                RuntimeLog.Warning($"Resources load failed: {path}");

            return asset;
        }

        /// <summary>
        /// 지정한 경로의 모든 Resources 에셋을 로드합니다.
        /// </summary>
        public static T[] LoadAll<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(NormalizePath(path));
        }

        /// <summary>
        /// 지정한 경로에서 Resources 에셋 로드를 시도합니다.
        /// </summary>
        public static bool TryLoad<T>(string path, out T asset) where T : Object
        {
            asset = Load<T>(path);
            return asset != null;
        }

        /// <summary>
        /// 지정한 경로에서 Resources 에셋을 캐시 기반으로 로드합니다.
        /// </summary>
        public static T LoadCached<T>(string path) where T : Object
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            string normalizedPath = NormalizePath(path);
            string key = typeof(T).FullName + ":" + normalizedPath;

            if (Cache.TryGetValue(key, out Object cached))
                return cached as T;

            T asset = Resources.Load<T>(normalizedPath);
            if (asset != null)
                Cache[key] = asset;

            return asset;
        }

        /// <summary>
        /// Resources 로드 캐시를 비웁니다.
        /// </summary>
        public static void ClearCache()
        {
            Cache.Clear();
        }

        /// <summary>
        /// Resources 경로 조각을 슬래시 기준 경로로 합칩니다.
        /// </summary>
        public static string CombinePath(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            return NormalizePath(string.Join("/", paths));
        }

        /// <summary>
        /// Resources 경로 구분자와 앞뒤 슬래시를 정리합니다.
        /// </summary>
        public static string NormalizePath(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/').Trim('/');
        }
    }
}
