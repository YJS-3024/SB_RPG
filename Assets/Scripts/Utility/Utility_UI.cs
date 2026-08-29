using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace yjs.DevKit
{
    /// <summary>
    /// CanvasGroup, RectTransform, UI 입력 처리에 사용하는 유틸리티입니다.
    /// </summary>
    public static class UIUtility
    {
        /// <summary>
        /// CanvasGroup의 표시, 입력, Raycast 차단 상태를 함께 설정합니다.
        /// </summary>
        public static void SetVisible(this CanvasGroup canvasGroup, bool visible)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        /// <summary>
        /// CanvasGroup의 alpha만 변경합니다.
        /// </summary>
        public static void SetAlpha(this CanvasGroup canvasGroup, float alpha)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        /// <summary>
        /// Button의 상호작용 가능 상태를 안전하게 변경합니다.
        /// </summary>
        public static void SetInteractableSafe(this Button button, bool interactable)
        {
            if (button != null && button.interactable != interactable)
                button.interactable = interactable;
        }

        /// <summary>
        /// RectTransform의 anchoredPosition을 설정합니다.
        /// </summary>
        public static void SetAnchoredPosition(this RectTransform rectTransform, float x, float y)
        {
            if (rectTransform == null)
                return;

            rectTransform.anchoredPosition = new Vector2(x, y);
        }

        /// <summary>
        /// RectTransform의 sizeDelta를 설정합니다.
        /// </summary>
        public static void SetSize(this RectTransform rectTransform, float width, float height)
        {
            if (rectTransform == null)
                return;

            rectTransform.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// 화면 좌표가 RectTransform 내부에 있는지 확인합니다.
        /// </summary>
        public static bool ContainsScreenPoint(this RectTransform rectTransform, Vector2 screenPoint, Camera camera = null)
        {
            return rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, camera);
        }

        /// <summary>
        /// 현재 포인터가 UI 위에 있는지 확인합니다.
        /// </summary>
        public static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
