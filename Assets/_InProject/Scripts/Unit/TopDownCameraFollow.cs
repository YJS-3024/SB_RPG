using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -2.5f);
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float followSmoothTime = 0.18f;
    [SerializeField, Min(0.1f)] private float minDistance = 5f;
    [SerializeField, Min(0.1f)] private float maxDistance = 12f;
    [SerializeField, Min(0f)] private float zoomSensitivity = 0.01f;

    private Transform _target;
    private Vector3 _followVelocity;
    private float _distance;

    private void Awake()
    {
        _distance = Mathf.Clamp(offset.magnitude, minDistance, Mathf.Max(minDistance, maxDistance));
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        _followVelocity = Vector3.zero;

        if (_target != null)
        {
            transform.position = _target.position + offset.normalized * _distance;
            transform.LookAt(_target.position + lookOffset);
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            _distance = Mathf.Clamp(
                _distance - scroll * zoomSensitivity,
                minDistance,
                Mathf.Max(minDistance, maxDistance));
        }

        Vector3 targetPosition = _target.position + offset.normalized * _distance;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _followVelocity,
            followSmoothTime);
        transform.LookAt(_target.position + lookOffset);
    }
}