using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace yjs.DevKit.UI
{
    public class ButtonEx : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Graphic[] arrIgnoreGraphics;
        public GameObject[] arrPressActiveGo;

        [Header("Click")]
        // 싱글클릭과 더블클릭을 구분하기 위해 두 번째 클릭을 기다리는 시간
        [SerializeField, Min(0.1f)] private float doubleClickInterval = 0.3f;
        // 첫 클릭 이후 다음 프레스에서 드래그를 허용할지 여부
        [SerializeField] private bool useClickAfterPressDrag = true;

        private Color color_NotInteractable = Color.white;
        private Color color_Normal = Color.white;
        private Color color_Selected = Color.white;
        private Color color_Rollhover = Color.white;

        private Action _onClick;
        private Action _onDoubleClick;
        private Action<PointerEventData> _onBeginDrag;
        private Action<PointerEventData> _onDrag;
        private Action<PointerEventData> _onEndDrag;
        private Action _onSelect;

        private bool _isPointerInside = false;

        // 첫 클릭이 발생하여 싱글클릭 확정을 대기 중인지 여부
        private bool _isWaitingForSecondClick;

        // 첫 클릭이 완료되어 다음 프레스에서 드래그할 수 있는 상태
        private bool _isClickArmed;

        // 현재 프레스가 클릭 이후에 발생한 드래그 후보인지 여부
        private bool _isArmedPress;

        private bool _isDragging;
        private float _secondClickDeadline;
        private Coroutine _singleClickCoroutine;

        public bool IsSelect { private set; get; }
        public bool IsDragging => _isDragging;

        public Action OnClick { set => _onClick = value; }
        public Action OnDoubleClick { set => _onDoubleClick = value; }
        public Action<PointerEventData> OnBeginPressDrag { set => _onBeginDrag = value; }
        public Action<PointerEventData> OnPressDrag { set => _onDrag = value; }
        public Action<PointerEventData> OnEndPressDrag { set => _onEndDrag = value; }

        private Text _btnText = null;

        public Text Text => _btnText != null
            ? _btnText
            : _btnText = GetComponentInChildren<Text>();

        public string ButtonString
        {
            get => Text.text;
            set => Text.text = value;
        }

        public void UpdateVisualGrapghic()
        {
            if (transition == Transition.None)
                return;

            var graphics = GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                if (arrIgnoreGraphics.Contains(g))
                    continue;

                if (_isPointerInside)
                {
                    g.color = color_Rollhover;
                }
                else if (IsSelect)
                {
                    g.color = color_Selected;
                }
                else
                {
                    if (interactable)
                    {
                        g.color = color_Normal;
                    }
                    else
                    {
                        g.color = color_NotInteractable;
                    }
                }
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            _isPointerInside = true;
            UpdateVisualGrapghic();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            _isPointerInside = false;
            UpdateVisualGrapghic();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            // Button과 동일하게 활성화된 좌클릭만 처리한다.
            if (eventData.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable())
                return;

            // 제한 시간 내 두 번째 클릭이면 대기 중인 싱글클릭을 취소하고
            // 더블클릭 이벤트만 호출한다.
            if (_isWaitingForSecondClick && Time.unscaledTime <= _secondClickDeadline)
            {
                CancelPendingSingleClick();
                _isClickArmed = false;
                _onDoubleClick?.Invoke();
                return;
            }

            // 이전 대기 상태가 남아 있다면 정리한 후 새로운 첫 클릭을 등록한다.
            CancelPendingSingleClick();

            _isWaitingForSecondClick = true;
            _isClickArmed = true;
            _secondClickDeadline = Time.unscaledTime + doubleClickInterval;
            _singleClickCoroutine = StartCoroutine(InvokeSingleClickAfterDelay());
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            // 첫 클릭이 완료된 상태에서 발생한 다음 좌측 프레스만
            // 클릭 이후 드래그의 시작 후보로 인정한다.
            _isArmedPress = useClickAfterPressDrag &&
                            _isClickArmed &&
                            eventData.button == PointerEventData.InputButton.Left &&
                            IsActive() &&
                            IsInteractable();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            // 드래그로 전환되지 않은 일반 프레스는 후보 상태만 해제한다.
            if (!_isDragging)
                _isArmedPress = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // EventSystem의 드래그 임계값을 넘었더라도 클릭 이후 프레스가 아니면 무시한다.
            if (!_isArmedPress || eventData.button != PointerEventData.InputButton.Left)
                return;

            _isDragging = true;
            // 더블클릭 대기 중 드래그가 시작되면 첫 클릭을 싱글클릭으로 확정한다.
            ResolvePendingSingleClick();
            _onBeginDrag?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
                _onDrag?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            _onEndDrag?.Invoke(eventData);

            // 한 번의 클릭 이후 드래그가 끝났으므로 다시 첫 클릭부터 받아야 한다.
            _isDragging = false;
            _isArmedPress = false;
            _isClickArmed = false;
        }

        protected override void OnDisable()
        {
            // 비활성화된 동안 지연 이벤트가 실행되거나 이전 상태가 남지 않도록 정리한다.
            CancelPendingSingleClick();
            _isClickArmed = false;
            _isArmedPress = false;
            _isDragging = false;

            base.OnDisable();
        }

        private IEnumerator InvokeSingleClickAfterDelay()
        {
            // 게임의 timeScale과 관계없이 UI 입력 시간을 측정한다.
            while (Time.unscaledTime < _secondClickDeadline)
                yield return null;

            _singleClickCoroutine = null;
            ResolvePendingSingleClick();
        }

        private void ResolvePendingSingleClick()
        {
            if (!_isWaitingForSecondClick)
                return;

            CancelPendingSingleClick();

            // 기존 Button 인스펙터 이벤트와 ButtonEx 코드 이벤트를 함께 지원한다.
            onClick.Invoke();
            _onClick?.Invoke();
        }

        private void CancelPendingSingleClick()
        {
            _isWaitingForSecondClick = false;

            if (_singleClickCoroutine == null)
                return;

            StopCoroutine(_singleClickCoroutine);
            _singleClickCoroutine = null;
        }
    }
}
