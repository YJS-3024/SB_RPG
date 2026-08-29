using System;
using System.Collections.Generic;
using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// 런타임에서 Unity 로그를 화면에 표시하고 터치 제스처로 열 수 있는 로그 뷰어입니다.
    /// </summary>
    public sealed class RuntimeLogViewer : MonoBehaviour
    {
        private const int WindowId = 24681357;
        private const int MaxLogCount = 300;
        private const float TriggerAreaSize = 96f;
        private const float MultiTapSeconds = 1.2f;
        private const int RequiredTapCount = 3;
        private const float GestureTimeoutSeconds = 3f;
        private const float MinGestureWidth = 120f;
        private const float MinGestureHeight = 80f;
        private const float MinGestureLength = 240f;
        private const float MinSegmentDeltaY = 24f;

        private static RuntimeLogViewer _instance;

        private readonly List<LogEntry> _logs = new();
        private readonly List<Vector2> _gesturePoints = new();
        private Vector2 _scrollPos;
        private Rect _windowRect = new(20f, 20f, 760f, 520f);
        private bool _isVisible;
        private bool _showLog = true;
        private bool _showWarning = true;
        private bool _showError = true;
        private bool _isWaitingGesture;
        private int _tapCount;
        private float _lastTapTime;
        private float _gestureStartTime;
        private bool _previousLoggerEnabled;

        /// <summary>
        /// 씬 로드 후 로그 뷰어 오브젝트를 자동으로 생성합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
                return;

            var go = new GameObject(nameof(RuntimeLogViewer));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<RuntimeLogViewer>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F12))
                ToggleVisible();

            UpdateTouchTrigger();
        }

        private void OnGUI()
        {
            if (!_isVisible)
                return;

            _windowRect.width = Mathf.Min(_windowRect.width, Screen.width - 40f);
            _windowRect.height = Mathf.Min(_windowRect.height, Screen.height - 40f);
            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, "Runtime Log Viewer");
        }

        /// <summary>
        /// 왼쪽 위 영역을 세 번 터치한 뒤 W 제스처를 그리면 로그 뷰어를 열거나 닫습니다.
        /// </summary>
        private void UpdateTouchTrigger()
        {
            if (_isWaitingGesture)
            {
                UpdateWGesture();
                return;
            }

            if (Input.touchCount <= 0)
                return;

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended)
                return;

            if (!IsInTriggerArea(touch.position))
                return;

            if (Time.unscaledTime - _lastTapTime > MultiTapSeconds)
                _tapCount = 0;

            _lastTapTime = Time.unscaledTime;
            _tapCount++;

            if (_tapCount < RequiredTapCount)
                return;

            _tapCount = 0;
            StartGestureWait();
        }

        /// <summary>
        /// 터치 위치가 숨겨진 트리거 영역 안에 있는지 확인합니다.
        /// </summary>
        private static bool IsInTriggerArea(Vector2 position)
        {
            return position.x <= TriggerAreaSize && position.y >= Screen.height - TriggerAreaSize;
        }

        /// <summary>
        /// W 제스처 입력 대기 상태를 시작합니다.
        /// </summary>
        private void StartGestureWait()
        {
            _isWaitingGesture = true;
            _gestureStartTime = Time.unscaledTime;
            _gesturePoints.Clear();
        }

        /// <summary>
        /// 제한 시간 안에 입력된 터치 궤적이 W 제스처인지 검사합니다.
        /// </summary>
        private void UpdateWGesture()
        {
            if (Time.unscaledTime - _gestureStartTime > GestureTimeoutSeconds)
            {
                CancelGestureWait();
                return;
            }

            if (Input.touchCount <= 0)
                return;

            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                _gesturePoints.Clear();

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                AddGesturePoint(touch.position);

            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                return;

            AddGesturePoint(touch.position);

            if (IsWGesture(_gesturePoints))
                ToggleVisible();

            CancelGestureWait();
        }

        /// <summary>
        /// 너무 가까운 점은 제외하고 제스처 포인트를 기록합니다.
        /// </summary>
        private void AddGesturePoint(Vector2 position)
        {
            if (_gesturePoints.Count > 0 && Vector2.Distance(_gesturePoints[^1], position) < 8f)
                return;

            _gesturePoints.Add(position);
        }

        /// <summary>
        /// 제스처 입력 대기 상태를 종료합니다.
        /// </summary>
        private void CancelGestureWait()
        {
            _isWaitingGesture = false;
            _gesturePoints.Clear();
        }

        /// <summary>
        /// 입력된 궤적이 아래-위-아래-위 방향 전환을 가진 W 모양인지 확인합니다.
        /// </summary>
        private static bool IsWGesture(List<Vector2> points)
        {
            if (points.Count < 8)
                return false;

            var bounds = GetBounds(points);
            if (bounds.width < MinGestureWidth || bounds.height < MinGestureHeight)
                return false;

            if (GetPathLength(points) < MinGestureLength)
                return false;

            var directions = GetVerticalDirections(points);
            if (directions.Count < 4)
                return false;

            return directions[0] < 0 && directions[1] > 0 && directions[2] < 0 && directions[3] > 0;
        }

        /// <summary>
        /// 제스처 포인트 전체를 포함하는 영역을 계산합니다.
        /// </summary>
        private static Rect GetBounds(List<Vector2> points)
        {
            var min = points[0];
            var max = points[0];

            for (var i = 1; i < points.Count; i++)
            {
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        /// <summary>
        /// 제스처 전체 이동 거리를 계산합니다.
        /// </summary>
        private static float GetPathLength(List<Vector2> points)
        {
            var length = 0f;
            for (var i = 1; i < points.Count; i++)
                length += Vector2.Distance(points[i - 1], points[i]);

            return length;
        }

        /// <summary>
        /// 큰 수직 이동 방향만 모아 W 판정에 사용할 방향 목록을 만듭니다.
        /// </summary>
        private static List<int> GetVerticalDirections(List<Vector2> points)
        {
            var directions = new List<int>();
            var accumulatedY = 0f;

            for (var i = 1; i < points.Count; i++)
            {
                accumulatedY += points[i].y - points[i - 1].y;
                if (Mathf.Abs(accumulatedY) < MinSegmentDeltaY)
                    continue;

                var direction = accumulatedY > 0f ? 1 : -1;
                if (directions.Count == 0 || directions[^1] != direction)
                    directions.Add(direction);

                accumulatedY = 0f;
            }

            return directions;
        }

        /// <summary>
        /// 뷰어 표시 상태를 변경하고 표시 중에는 Unity 로그 출력을 임시로 활성화합니다.
        /// </summary>
        private void ToggleVisible()
        {
            _isVisible = !_isVisible;

            if (_isVisible)
            {
                _previousLoggerEnabled = UnityEngine.Debug.unityLogger.logEnabled;
                UnityEngine.Debug.unityLogger.logEnabled = true;
                return;
            }

            UnityEngine.Debug.unityLogger.logEnabled = _previousLoggerEnabled;
        }

        /// <summary>
        /// Unity 로그 콜백으로 들어온 로그를 화면 표시용 버퍼에 저장합니다.
        /// </summary>
        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (_logs.Count >= MaxLogCount)
                _logs.RemoveAt(0);

            _logs.Add(new LogEntry(DateTime.Now, condition, stackTrace, type));

            if (_isVisible)
                _scrollPos.y = float.MaxValue;
        }

        /// <summary>
        /// 로그 창 전체 UI를 그립니다.
        /// </summary>
        private void DrawWindow(int windowId)
        {
            DrawToolbar();
            DrawLogList();
            GUI.DragWindow(new Rect(0f, 0f, _windowRect.width, 24f));
        }

        /// <summary>
        /// 로그 필터와 Clear, Close 버튼을 그립니다.
        /// </summary>
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();
            _showLog = GUILayout.Toggle(_showLog, "Log", GUILayout.Width(60f));
            _showWarning = GUILayout.Toggle(_showWarning, "Warning", GUILayout.Width(90f));
            _showError = GUILayout.Toggle(_showError, "Error", GUILayout.Width(70f));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Count: {_logs.Count}", GUILayout.Width(90f));

            if (GUILayout.Button("Clear", GUILayout.Width(70f)))
                _logs.Clear();

            if (GUILayout.Button("Close", GUILayout.Width(70f)))
                ToggleVisible();

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 필터 조건에 맞는 로그 목록을 스크롤 영역에 표시합니다.
        /// </summary>
        private void DrawLogList()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUI.skin.box);

            foreach (var log in _logs)
            {
                if (!CanShow(log.Type))
                    continue;

                var previousColor = GUI.contentColor;
                GUI.contentColor = GetColor(log.Type);
                GUILayout.Label($"[{log.Time:HH:mm:ss}] [{log.Type}] {log.Condition}");
                GUI.contentColor = previousColor;

                if (!string.IsNullOrEmpty(log.StackTrace) && log.Type != LogType.Log)
                    GUILayout.TextArea(log.StackTrace);
            }

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// 로그 타입이 현재 필터 조건에 포함되는지 확인합니다.
        /// </summary>
        private bool CanShow(LogType type)
        {
            return type switch
            {
                LogType.Warning => _showWarning,
                LogType.Error or LogType.Exception or LogType.Assert => _showError,
                _ => _showLog
            };
        }

        /// <summary>
        /// 로그 타입에 맞는 표시 색상을 반환합니다.
        /// </summary>
        private static Color GetColor(LogType type)
        {
            return type switch
            {
                LogType.Warning => Color.yellow,
                LogType.Error or LogType.Exception or LogType.Assert => Color.red,
                _ => Color.white
            };
        }

        private readonly struct LogEntry
        {
            public readonly DateTime Time;
            public readonly string Condition;
            public readonly string StackTrace;
            public readonly LogType Type;

            public LogEntry(DateTime time, string condition, string stackTrace, LogType type)
            {
                Time = time;
                Condition = condition;
                StackTrace = stackTrace;
                Type = type;
            }
        }
    }
}
