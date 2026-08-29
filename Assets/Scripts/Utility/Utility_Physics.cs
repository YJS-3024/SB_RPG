using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// Physics 쿼리와 LayerMask 처리를 단순화하는 유틸리티입니다.
    /// </summary>
    public static class PhysicsUtility
    {
        /// <summary>
        /// 지정한 레이어가 LayerMask에 포함되는지 확인합니다.
        /// </summary>
        public static bool ContainsLayer(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        /// <summary>
        /// GameObject의 레이어가 LayerMask에 포함되는지 확인합니다.
        /// </summary>
        public static bool Contains(this LayerMask mask, GameObject go)
        {
            return go != null && mask.ContainsLayer(go.layer);
        }

        /// <summary>
        /// 구체 범위 안의 Collider를 NonAlloc 방식으로 검색합니다.
        /// </summary>
        public static int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, LayerMask layerMask, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (results == null)
                return 0;

            return Physics.OverlapSphereNonAlloc(position, Mathf.Max(0f, radius), results, layerMask, triggerInteraction);
        }

        /// <summary>
        /// Raycast를 실행하고 지정한 LayerMask에 맞는 대상만 검사합니다.
        /// </summary>
        public static bool Raycast(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, out RaycastHit hit, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                hit = default;
                return false;
            }

            return Physics.Raycast(origin, direction.normalized, out hit, Mathf.Max(0f, distance), layerMask, triggerInteraction);
        }

        /// <summary>
        /// origin에서 target까지 장애물 없이 볼 수 있는지 확인합니다.
        /// </summary>
        public static bool HasLineOfSight(Vector3 origin, Vector3 target, LayerMask obstacleMask, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            Vector3 direction = target - origin;
            float distance = direction.magnitude;

            if (distance <= Mathf.Epsilon)
                return true;

            return !Physics.Raycast(origin, direction / distance, distance, obstacleMask, triggerInteraction);
        }

        /// <summary>
        /// 지정 위치 아래로 Raycast를 쏴서 지면이 있는지 확인합니다.
        /// </summary>
        public static bool CheckGround(Vector3 origin, float distance, LayerMask groundMask, out RaycastHit hit, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return Physics.Raycast(origin, Vector3.down, out hit, Mathf.Max(0f, distance), groundMask, triggerInteraction);
        }
    }
}
