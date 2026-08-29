using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// Vector2, Vector3, 그리드 좌표 변환에 자주 사용하는 유틸리티입니다.
    /// </summary>
    public static class VectorUtility
    {
        /// <summary>
        /// 두 Vector3 사이의 제곱 거리가 지정 거리 안인지 확인합니다.
        /// </summary>
        public static bool IsWithinDistanceSqr(Vector3 a, Vector3 b, float distance)
        {
            return (a - b).sqrMagnitude <= distance * distance;
        }

        /// <summary>
        /// 두 Vector2 사이의 제곱 거리가 지정 거리 안인지 확인합니다.
        /// </summary>
        public static bool IsWithinDistanceSqr(Vector2 a, Vector2 b, float distance)
        {
            return (a - b).sqrMagnitude <= distance * distance;
        }

        /// <summary>
        /// from에서 to를 향하는 정규화된 방향 벡터를 반환합니다.
        /// </summary>
        public static Vector3 DirectionTo(Vector3 from, Vector3 to)
        {
            var direction = to - from;
            return direction.sqrMagnitude <= Mathf.Epsilon ? Vector3.zero : direction.normalized;
        }

        /// <summary>
        /// from에서 to를 향하는 정규화된 Vector2 방향을 반환합니다.
        /// </summary>
        public static Vector2 DirectionTo(Vector2 from, Vector2 to)
        {
            var direction = to - from;
            return direction.sqrMagnitude <= Mathf.Epsilon ? Vector2.zero : direction.normalized;
        }

        /// <summary>
        /// Vector3의 XZ 평면 거리만 계산합니다.
        /// </summary>
        public static float DistanceXZ(Vector3 a, Vector3 b)
        {
            float x = a.x - b.x;
            float z = a.z - b.z;
            return Mathf.Sqrt(x * x + z * z);
        }

        /// <summary>
        /// 월드 좌표를 셀 크기 기준의 Vector2Int 그리드 좌표로 변환합니다.
        /// </summary>
        public static Vector2Int ToGridPosition(Vector3 worldPosition, float cellSize = 1f)
        {
            cellSize = Mathf.Max(cellSize, Mathf.Epsilon);
            return new Vector2Int(Mathf.FloorToInt(worldPosition.x / cellSize), Mathf.FloorToInt(worldPosition.z / cellSize));
        }

        /// <summary>
        /// Vector2Int 그리드 좌표를 XZ 평면의 월드 좌표로 변환합니다.
        /// </summary>
        public static Vector3 ToWorldPosition(Vector2Int gridPosition, float cellSize = 1f, float y = 0f)
        {
            return new Vector3(gridPosition.x * cellSize, y, gridPosition.y * cellSize);
        }

        /// <summary>
        /// Vector3의 x 값만 바꾼 새 벡터를 반환합니다.
        /// </summary>
        public static Vector3 WithX(this Vector3 value, float x)
        {
            value.x = x;
            return value;
        }

        /// <summary>
        /// Vector3의 y 값만 바꾼 새 벡터를 반환합니다.
        /// </summary>
        public static Vector3 WithY(this Vector3 value, float y)
        {
            value.y = y;
            return value;
        }

        /// <summary>
        /// Vector3의 z 값만 바꾼 새 벡터를 반환합니다.
        /// </summary>
        public static Vector3 WithZ(this Vector3 value, float z)
        {
            value.z = z;
            return value;
        }

        /// <summary>
        /// 중심과 반지름을 기준으로 XZ 평면의 랜덤 위치를 반환합니다.
        /// </summary>
        public static Vector3 RandomPointInCircleXZ(Vector3 center, float radius)
        {
            var point = Random.insideUnitCircle * Mathf.Max(0f, radius);
            return new Vector3(center.x + point.x, center.y, center.z + point.y);
        }

        /// <summary>
        /// Vector2Int 방향을 4방향 축 방향으로 정규화합니다.
        /// </summary>
        public static Vector2Int NormalizeTo4Direction(Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
                return Vector2Int.zero;

            return Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                ? new Vector2Int((int)Mathf.Sign(direction.x), 0)
                : new Vector2Int(0, (int)Mathf.Sign(direction.y));
        }
    }
}
