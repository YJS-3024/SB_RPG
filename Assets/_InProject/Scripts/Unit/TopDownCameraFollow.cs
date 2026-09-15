using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -6f);
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float followSmoothTime = 0.18f;

    private Transform _target;
    private Vector3 _followVelocity;

    private void LateUpdate()
    {
        if (_target == null)
        {
            PlayerUnit player = FindAnyObjectByType<PlayerUnit>();
            if (player == null)
                return;

            _target = player.transform;
            transform.position = _target.position + offset;
        }

        Vector3 targetPosition = _target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _followVelocity,
            followSmoothTime);
        transform.LookAt(_target.position + lookOffset);
    }
}