using UnityEngine;

public enum EnemyActionType
{
    Idle,
    TimedAttack
}

public class EnemyUnit : UnitBase, IDamageable
{
    [Header("Action")]
    [SerializeField] private EnemyActionType actionType = EnemyActionType.TimedAttack;
    [SerializeField, Min(0.1f)] private float attackInterval = 3f;
    [SerializeField, Min(0f)] private float attackWindup = 0.65f;
    [SerializeField, Min(0.1f)] private float attackReach = 1.2f;
    [SerializeField, Min(0.1f)] private float attackRadius = 0.75f;
    [SerializeField, Min(1)] private int attackDamage = 10;

    [Header("Hit")]
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private bool infiniteHealth = true;
    [SerializeField, Min(0f)] private float hitFlashDuration = 0.15f;

    private int _currentHealth;
    private float _nextActionTime;
    private float _attackResolveTime;
    private float _hitFlashEndTime;
    private bool _isInitialized;
    private bool _isPreparingAttack;
    private PlayerUnit _target;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _propertyBlock;
    private ParticleSystem _hitSpark;

    public bool IsAlive => _isInitialized && _currentHealth > 0;
    public int CurrentHealth => _currentHealth;
    public EnemyActionType ActionType => actionType;

    private void Start()
    {
        CreateUnit();
    }

    private void Update()
    {
        UpdateVisualState();
        if (!IsAlive)
            return;

        switch (actionType)
        {
            case EnemyActionType.Idle:
                CancelPreparedAttack();
                break;
            case EnemyActionType.TimedAttack:
                UpdateTimedAttack();
                break;
        }
    }

    public override void CreateUnit()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        _currentHealth = maxHealth;
        _nextActionTime = Time.time + attackInterval;
        _renderers = GetComponentsInChildren<Renderer>(true);
        _propertyBlock = new MaterialPropertyBlock();
        CreateHitSpark();

        CapsuleCollider hitCollider = GetComponent<CapsuleCollider>();
        if (hitCollider == null)
            hitCollider = gameObject.AddComponent<CapsuleCollider>();

        hitCollider.center = new Vector3(0f, 1.2f, 0f);
        hitCollider.height = 2.4f;
        hitCollider.radius = 0.5f;
        hitCollider.isTrigger = true;
    }

    public override void DestroyUnit()
    {
        _isInitialized = false;
        _currentHealth = 0;
        CancelPreparedAttack();
    }

    public void SetActionType(EnemyActionType type)
    {
        if (actionType == type)
            return;

        actionType = type;
        CancelPreparedAttack();
        _nextActionTime = Time.time + attackInterval;
    }

    public bool ReceiveDamage(int damage, GameObject attacker, Vector3 hitPoint)
    {
        if (!IsAlive || damage <= 0)
            return false;

        _hitFlashEndTime = Time.time + hitFlashDuration;
        PlayHitSpark(attacker, hitPoint);
        if (infiniteHealth)
            return true;

        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        if (_currentHealth == 0)
        {
            CancelPreparedAttack();
            SetVisualColor(new Color(0.2f, 0.2f, 0.2f, 1f));
        }

        return true;
    }

    private void CreateHitSpark()
    {
        GameObject effectObject = new GameObject("HitSparkEffect");
        effectObject.transform.SetParent(transform, false);
        effectObject.transform.localPosition = Vector3.up;

        _hitSpark = effectObject.AddComponent<ParticleSystem>();
        _hitSpark.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = _hitSpark.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.2f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.35f, 0.03f, 1f),
            new Color(1f, 0.95f, 0.25f, 1f));
        main.gravityModifier = 0.35f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 24;

        ParticleSystem.EmissionModule emission = _hitSpark.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = _hitSpark.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.05f;

        ParticleSystemRenderer particleRenderer =
            effectObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        particleRenderer.lengthScale = 0.25f;
        particleRenderer.velocityScale = 0.08f;
    }

    private void PlayHitSpark(GameObject attacker, Vector3 hitPoint)
    {
        if (_hitSpark == null)
            return;

        Vector3 direction = attacker != null
            ? attacker.transform.position - hitPoint
            : -transform.forward;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            direction = -transform.forward;

        Transform effectTransform = _hitSpark.transform;
        effectTransform.position = hitPoint;
        effectTransform.rotation = Quaternion.LookRotation(direction.normalized);
        _hitSpark.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _hitSpark.Emit(14);
    }
    private void UpdateTimedAttack()
    {
        if (_isPreparingAttack)
        {
            if (Time.time >= _attackResolveTime)
                ResolveAttack();
            return;
        }

        if (Time.time < _nextActionTime)
            return;

        _target = FindAnyObjectByType<PlayerUnit>();
        if (_target == null || !_target.IsAlive)
        {
            _nextActionTime = Time.time + 0.5f;
            return;
        }

        Vector3 direction = _target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > Mathf.Pow(attackReach + attackRadius, 2f))
        {
            _nextActionTime = Time.time + 0.25f;
            return;
        }

        if (direction.sqrMagnitude > Mathf.Epsilon)
            transform.rotation = Quaternion.LookRotation(direction.normalized);

        _isPreparingAttack = true;
        _attackResolveTime = Time.time + attackWindup;
    }

    private void ResolveAttack()
    {
        _isPreparingAttack = false;
        _nextActionTime = Time.time + attackInterval;

        if (_target == null || !_target.IsAlive)
            return;

        Vector3 direction = _target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > Mathf.Epsilon)
            transform.rotation = Quaternion.LookRotation(direction.normalized);

        Vector3 center = transform.position + Vector3.up + transform.forward * attackReach;
        CombatHitUtility.DamageSphere(gameObject, center, attackRadius, attackDamage);
    }

    private void CancelPreparedAttack()
    {
        _isPreparingAttack = false;
        _target = null;
    }

    private void UpdateVisualState()
    {
        if (!IsAlive)
            return;

        if (Time.time < _hitFlashEndTime)
            SetVisualColor(Color.yellow);
        else if (_isPreparingAttack)
            SetVisualColor(new Color(1f, 0.25f, 0.15f, 1f));
        else
            ClearVisualColor();
    }

    private void SetVisualColor(Color color)
    {
        if (_renderers == null || _propertyBlock == null)
            return;

        _propertyBlock.Clear();
        _propertyBlock.SetColor("_BaseColor", color);
        _propertyBlock.SetColor("_Color", color);
        foreach (Renderer targetRenderer in _renderers)
            targetRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void ClearVisualColor()
    {
        if (_renderers == null)
            return;

        foreach (Renderer targetRenderer in _renderers)
            targetRenderer.SetPropertyBlock(null);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + Vector3.up + transform.forward * attackReach;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}