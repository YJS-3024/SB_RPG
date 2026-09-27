using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerUnit), typeof(PlayerUnitMovement))]
public class PlayerAutoCombat : MonoBehaviour
{
    private static readonly Collider[] DetectionResults = new Collider[16];

    [Header("Detection")]
    [SerializeField, Min(0.1f)] private float detectionRadius = 2.2f;
    [SerializeField, Range(1f, 360f)] private float detectionAngle = 120f;
    [SerializeField, Min(0.02f)] private float detectionInterval = 0.1f;

    [Header("Attack")]
    [SerializeField, Min(0.05f)] private float basicAttackInterval = 0.75f;
    [SerializeField, Min(0f)] private float skillCooldown = 5f;

    private PlayerUnit _playerUnit;
    private PlayerUnitMovement _movement;
    private EnemyUnit _target;
    private float _nextDetectionTime;
    private float _nextAttackTime;
    private float _nextSkillTime;

    public EnemyUnit Target => _target;

    private void Awake()
    {
        _playerUnit = GetComponent<PlayerUnit>();
        _movement = GetComponent<PlayerUnitMovement>();
        _nextSkillTime = Time.time + skillCooldown;
    }

    private void Update()
    {
        if (!_playerUnit.IsAlive || _playerUnit.IsHitReacting)
            return;

        if (Time.time >= _nextDetectionTime)
        {
            _nextDetectionTime = Time.time + detectionInterval;
            _target = FindNearestTarget();
        }

        if (_target == null || !_target.IsAlive || Time.time < _nextAttackTime)
            return;

        FaceTarget(_target.transform.position);

        if (Time.time >= _nextSkillTime && _movement.TryAttackSkill())
        {
            _nextSkillTime = Time.time + skillCooldown;
            _nextAttackTime = Time.time + basicAttackInterval;
            return;
        }

        if (_movement.TryBasicAttack())
            _nextAttackTime = Time.time + basicAttackInterval;
    }

    private EnemyUnit FindNearestTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            DetectionResults,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);

        EnemyUnit nearest = null;
        float nearestDistance = float.PositiveInfinity;
        float minimumDirectionDot = Mathf.Cos(detectionAngle * 0.5f * Mathf.Deg2Rad);
        for (int i = 0; i < count; i++)
        {
            Collider hit = DetectionResults[i];
            DetectionResults[i] = null;
            if (hit == null)
                continue;

            EnemyUnit enemy = hit.GetComponentInParent<EnemyUnit>();
            if (enemy == null || !enemy.IsAlive)
                continue;

            Vector3 direction = enemy.transform.position - transform.position;
            direction.y = 0f;
            float distance = direction.sqrMagnitude;
            if (distance > detectionRadius * detectionRadius || distance >= nearestDistance)
                continue;

            if (distance > Mathf.Epsilon &&
                Vector3.Dot(transform.forward, direction.normalized) < minimumDirectionDot)
                continue;

            nearest = enemy;
            nearestDistance = distance;
        }

        return nearest;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > Mathf.Epsilon)
            transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.6f);
        Vector3 origin = transform.position;
        Vector3 left = Quaternion.AngleAxis(-detectionAngle * 0.5f, Vector3.up) * transform.forward;
        Vector3 right = Quaternion.AngleAxis(detectionAngle * 0.5f, Vector3.up) * transform.forward;
        Gizmos.DrawLine(origin, origin + left * detectionRadius);
        Gizmos.DrawLine(origin, origin + right * detectionRadius);

        const int segmentCount = 20;
        Vector3 previous = origin + left * detectionRadius;
        for (int i = 1; i <= segmentCount; i++)
        {
            float angle = Mathf.Lerp(-detectionAngle * 0.5f, detectionAngle * 0.5f,
                i / (float)segmentCount);
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
            Vector3 current = origin + direction * detectionRadius;
            Gizmos.DrawLine(previous, current);
            previous = current;
        }
    }
}