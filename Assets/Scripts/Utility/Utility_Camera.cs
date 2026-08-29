using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// 카메라 좌표 변환과 화면 영역 계산에 사용하는 유틸리티입니다.
    /// </summary>
    public static class CameraUtility
    {
        /// <summary>
        /// 카메라가 null이면 Camera.main을 반환합니다.
        /// </summary>
        public static Camera OrMain(this Camera camera)
        {
            return camera != null ? camera : Camera.main;
        }

        /// <summary>
        /// 화면 좌표를 월드 좌표로 변환합니다.
        /// </summary>
        public static Vector3 ScreenToWorld(Camera camera, Vector3 screenPosition)
        {
            camera = camera.OrMain();
            return camera == null ? Vector3.zero : camera.ScreenToWorldPoint(screenPosition);
        }

        /// <summary>
        /// 월드 좌표를 화면 좌표로 변환합니다.
        /// </summary>
        public static Vector3 WorldToScreen(Camera camera, Vector3 worldPosition)
        {
            camera = camera.OrMain();
            return camera == null ? Vector3.zero : camera.WorldToScreenPoint(worldPosition);
        }

        /// <summary>
        /// 현재 마우스 위치를 지정 평면 위의 월드 좌표로 변환합니다.
        /// </summary>
        public static bool TryGetMouseWorldPosition(Camera camera, Plane plane, out Vector3 worldPosition)
        {
            camera = camera.OrMain();
            if (camera == null)
            {
                worldPosition = Vector3.zero;
                return false;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (!plane.Raycast(ray, out float enter))
            {
                worldPosition = Vector3.zero;
                return false;
            }

            worldPosition = ray.GetPoint(enter);
            return true;
        }

        /// <summary>
        /// 카메라가 보는 월드 영역을 지정 거리 기준 Bounds로 계산합니다.
        /// </summary>
        public static Bounds GetViewportBounds(Camera camera, float distance)
        {
            camera = camera.OrMain();
            if (camera == null)
                return new Bounds(Vector3.zero, Vector3.zero);

            Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0f, 0f, distance));
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1f, 1f, distance));
            var bounds = new Bounds();
            bounds.SetMinMax(Vector3.Min(bottomLeft, topRight), Vector3.Max(bottomLeft, topRight));
            return bounds;
        }

        /// <summary>
        /// 월드 좌표가 카메라 화면 안에 있는지 확인합니다.
        /// </summary>
        public static bool IsWorldPointVisible(Camera camera, Vector3 worldPosition)
        {
            camera = camera.OrMain();
            if (camera == null)
                return false;

            Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
            return viewport.z >= 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
        }
    }
}
