using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -2.5f);
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float followSmoothTime = 0.18f;
    [SerializeField, Min(0f)] private float horizontalSensitivity = 0.15f;
    [SerializeField, Min(0.1f)] private float minDistance = 5f;
    [SerializeField, Min(0.1f)] private float maxDistance = 12f;
    [SerializeField, Min(0f)] private float zoomSensitivity = 0.01f;

    private Transform _target;
    private Vector3 _followVelocity;
    private float _distance;
    private float _yaw;
    private bool _cursorFree;

    private void Awake()
    {
        _distance = Mathf.Clamp(offset.magnitude, minDistance, Mathf.Max(minDistance, maxDistance));
    }

    private void OnEnable()
    {
        SetCursorFree(false);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyCursorState();
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        _followVelocity = Vector3.zero;

        if (_target != null)
        {
            transform.position = _target.position + GetOffset();
            transform.LookAt(_target.position + lookOffset);
        }
    }

    private void LateUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null &&
            (keyboard.leftAltKey.wasPressedThisFrame || keyboard.rightAltKey.wasPressedThisFrame))
            SetCursorFree(!_cursorFree);

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            if (!_cursorFree)
                _yaw += mouse.delta.ReadValue().x * horizontalSensitivity;

            float scroll = mouse.scroll.ReadValue().y;
            _distance = Mathf.Clamp(
                _distance - scroll * zoomSensitivity,
                minDistance,
                Mathf.Max(minDistance, maxDistance));
        }

        if (_target == null)
            return;

        Vector3 targetPosition = _target.position + GetOffset();
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _followVelocity,
            followSmoothTime);
        transform.LookAt(_target.position + lookOffset);
    }

    private Vector3 GetOffset()
    {
        return Quaternion.Euler(0f, _yaw, 0f) * offset.normalized * _distance;
    }

    private void SetCursorFree(bool cursorFree)
    {
        _cursorFree = cursorFree;
        ApplyCursorState();
    }

    private void ApplyCursorState()
    {
        Cursor.lockState = _cursorFree ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _cursorFree;
    }
}
