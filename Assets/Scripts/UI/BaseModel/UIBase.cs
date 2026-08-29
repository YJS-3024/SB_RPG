using System;
using UnityEngine;

namespace yjs.DevKit.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        protected RectTransform RectTransform => (RectTransform)transform;

        private void Start()
        {
            if (_canvasGroup is null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected void SetPanelAlpha(float value)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = value;
        }

        public void SetActive(bool bActive, GameObject targetGo = null)
        {
            if (targetGo == null)
                targetGo = this.gameObject;

            targetGo?.SetActive(bActive);
        }
        
        /// <summary>
        /// 패널 생성후 Initialize
        /// </summary>
        public abstract void Created();

        /// <summary>
        /// 패널 열때
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// 패널 닫을때
        /// </summary>
        public abstract void Hide();

        /// <summary>
        /// esc 반응
        /// </summary>
        /// <returns></returns>
        public abstract bool IsProcessEscape();
    }
}