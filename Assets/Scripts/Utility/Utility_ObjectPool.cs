using System.Collections.Generic;
using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// GameObject 전용 간단한 오브젝트 풀입니다.
    /// </summary>
    public sealed class GameObjectPool
    {
        private readonly GameObject prefab;
        private readonly Transform parent;
        private readonly Queue<GameObject> pool = new();

        /// <summary>
        /// 프리팹과 부모 Transform을 지정해 풀을 생성합니다.
        /// </summary>
        public GameObjectPool(GameObject prefab, Transform parent = null, int preloadCount = 0)
        {
            this.prefab = prefab;
            this.parent = parent;
            Preload(preloadCount);
        }

        /// <summary>
        /// 현재 풀에 보관 중인 비활성 오브젝트 수를 반환합니다.
        /// </summary>
        public int Count => pool.Count;

        /// <summary>
        /// 지정한 개수만큼 오브젝트를 미리 생성해 풀에 넣습니다.
        /// </summary>
        public void Preload(int count)
        {
            if (prefab == null || count <= 0)
                return;

            for (int i = 0; i < count; i++)
                Release(CreateInstance());
        }

        /// <summary>
        /// 풀에서 오브젝트를 가져오고, 없으면 새로 생성합니다.
        /// </summary>
        public GameObject Get(Vector3 position = default, Quaternion rotation = default)
        {
            if (prefab == null)
                return null;

            GameObject instance = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
            instance.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
            instance.SetActive(true);
            return instance;
        }

        /// <summary>
        /// 오브젝트를 비활성화하고 풀에 반환합니다.
        /// </summary>
        public void Release(GameObject instance)
        {
            if (instance == null)
                return;

            instance.SetActive(false);
            if (parent != null)
                instance.transform.SetParent(parent);

            pool.Enqueue(instance);
        }

        /// <summary>
        /// 풀에 보관된 비활성 오브젝트를 모두 제거합니다.
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                GameObject instance = pool.Dequeue();
                if (instance != null)
                    Object.Destroy(instance);
            }
        }

        private GameObject CreateInstance()
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.SetActive(false);
            return instance;
        }
    }
}
