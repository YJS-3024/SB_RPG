using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Camera))]
public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -2.5f);
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1f, 0f);

    [Header("Orbit")]
    [SerializeField, Min(0f)] private float horizontalSensitivity = 0.15f;
    [SerializeField, Min(0f)] private float verticalSensitivity = 0.15f;
    [SerializeField] private bool invertVerticalInput = true;
    [SerializeField, Range(-89f, 89f)] private float minVerticalAngle = 25f;
    [SerializeField, Range(-89f, 89f)] private float maxVerticalAngle = 85f;

    [Header("Zoom")]
    [SerializeField, Min(0.1f)] private float minDistance = 5f;
    [SerializeField, Min(0.1f)] private float maxDistance = 12f;
    [SerializeField, Min(0f)] private float zoomSensitivity = 0.2f;

    private Transform _target;
    private Vector3 _horizontalOrbitDirection;
    private float _distance;
    private float _followHeight;
    private float _yawOffset;
    private float _verticalAngle;
    private bool _isCursorReleased;

    private void Awake()
    {
        Vector3 initialDirection = offset.sqrMagnitude > 0f
            ? offset.normalized
            : new Vector3(0f, 0.9f, -0.4f).normalized;
        _horizontalOrbitDirection = Vector3.ProjectOnPlane(initialDirection, Vector3.up).normalized;
        if (_horizontalOrbitDirection.sqrMagnitude <= Mathf.Epsilon)
            _horizontalOrbitDirection = Vector3.back;

        float horizontalMagnitude = Vector3.ProjectOnPlane(initialDirection, Vector3.up).magnitude;
        _verticalAngle = Mathf.Atan2(initialDirection.y, horizontalMagnitude) * Mathf.Rad2Deg;
        _verticalAngle = ClampVerticalAngle(_verticalAngle);
        _distance = Mathf.Clamp(
            Mathf.Max(offset.magnitude, minDistance),
            minDistance,
            Mathf.Max(minDistance, maxDistance));
    }

    private void OnEnable()
    {
        SetCursorReleased(false, true);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            SetCursorReleased(IsAltHeld(), true);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        if (_target != null)
            _followHeight = _target.position.y;

        if (_target == null)
            return;

        transform.position = GetDesiredPosition();
        LookAtTarget();
    }

private void LateUpdate()
    {
        if (_target == null)
            return;

        transform.position = GetDesiredPosition();
        LookAtTarget();
    }

    private void UpdateMouseInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        if (!_isCursorReleased && mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            _yawOffset += delta.x * horizontalSensitivity;
            float verticalInput = invertVerticalInput ? -delta.y : delta.y;
            _verticalAngle = ClampVerticalAngle(
                _verticalAngle + verticalInput * verticalSensitivity);
        }

        float scroll = mouse.scroll.ReadValue().y;
        _distance = Mathf.Clamp(
            _distance - scroll * zoomSensitivity,
            minDistance,
            Mathf.Max(minDistance, maxDistance));
    }

    private Vector3 GetDesiredPosition()
    {
        Quaternion cameraYaw = Quaternion.Euler(0f, _yawOffset, 0f);
        float verticalRadians = _verticalAngle * Mathf.Deg2Rad;
        Vector3 horizontalDirection = cameraYaw * _horizontalOrbitDirection;
        Vector3 worldDirection =
            horizontalDirection * Mathf.Cos(verticalRadians) +
            Vector3.up * Mathf.Sin(verticalRadians);
        return GetFollowOrigin() + worldDirection * _distance;
    }

    private float ClampVerticalAngle(float angle)
    {
        float minAngle = Mathf.Min(minVerticalAngle, maxVerticalAngle);
        float maxAngle = Mathf.Max(minVerticalAngle, maxVerticalAngle);
        return Mathf.Clamp(angle, minAngle, maxAngle);
    }
    private Vector3 GetFollowOrigin()
    {
        Vector3 position = _target.position;
        position.y = _followHeight;
        return position;
    }

    private void LookAtTarget()
    {
        transform.LookAt(GetFollowOrigin() + lookOffset);
    }

    private static bool IsAltHeld()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null &&
               (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed);
    }

    private void SetCursorReleased(bool released, bool force = false)
    {
        if (!force && _isCursorReleased == released)
            return;

        _isCursorReleased = released;
        Cursor.lockState = released ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = released;
    }


private void Update()
    {
        SetCursorReleased(IsAltHeld());
        UpdateMouseInput();

        if (_target != null)
        {
            transform.position = GetDesiredPosition();
            LookAtTarget();
        }
    }
}
