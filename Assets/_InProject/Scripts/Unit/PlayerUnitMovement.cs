using UnityEngine;

public enum WeaponAnimationStyle
{
    Unarmed = -1,
    Greatsword = 0,
    TwinDagger = 1
}

[RequireComponent(typeof(InputReader_Player))]
[RequireComponent(typeof(PlayerUnit))]
public class PlayerUnitMovement : MonoBehaviour
{
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int WeaponStyleParameter = Animator.StringToHash("WeaponStyle");
    private static readonly int JumpParameter = Animator.StringToHash("Jump");
    private static readonly int AttackParameter = Animator.StringToHash("Attack");
    private static readonly int AttackVariantParameter = Animator.StringToHash("AttackVariant");

    [SerializeField] private WeaponAnimationStyle weaponStyle = WeaponAnimationStyle.Greatsword;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float jumpSpeed = 7.071f;
    [SerializeField] private float gravity = 15f;
    [SerializeField] private float fallGravityMultiplier = 1.6f;
    [SerializeField] private float maxFallSpeed = 12f;
    [SerializeField] private float animationDampTime = 0.1f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.1f;
    [SerializeField, Min(0.01f)] private float comboResetTime = 1.5f;

    private InputReader_Player _inputReader;
    private PlayerUnit _playerUnit;
    private float _groundHeight;
    private float _verticalSpeed;
    private bool _isGrounded = true;
    private Vector3 _moveTarget;
    private bool _hasMoveTarget;
    private int _attackIndex;
    private float _lastAttackTime = float.NegativeInfinity;

    public WeaponAnimationStyle WeaponStyle => weaponStyle;

    public void SetWeaponStyle(WeaponAnimationStyle style)
    {
        weaponStyle = style;
        _attackIndex = 0;
        _lastAttackTime = float.NegativeInfinity;
        if (_playerUnit != null && _playerUnit.animator != null)
            _playerUnit.animator.SetFloat(WeaponStyleParameter, (float)style);
    }

    private void Awake()
    {
        _inputReader = GetComponent<InputReader_Player>();
        _playerUnit = GetComponent<PlayerUnit>();
        _groundHeight = transform.position.y;
    }

    private void OnEnable()
    {
        _attackIndex = 0;
        _lastAttackTime = float.NegativeInfinity;
        _inputReader.JumpPerformed += OnJump;
        _inputReader.Attack += OnAttack;
        _inputReader.AttackSkill += OnAttackSkill;
        _inputReader.AttackPreview += OnAttackPreview;
        _inputReader.MoveRequested += OnMoveRequested;
        _inputReader.WeaponStyleRequested += OnWeaponStyleRequested;
    }

    private void OnDisable()
    {
        _inputReader.JumpPerformed -= OnJump;
        _inputReader.Attack -= OnAttack;
        _inputReader.AttackSkill -= OnAttackSkill;
        _inputReader.AttackPreview -= OnAttackPreview;
        _inputReader.MoveRequested -= OnMoveRequested;
        _inputReader.WeaponStyleRequested -= OnWeaponStyleRequested;
    }

    private void Update()
    {
        if (_playerUnit.animator == null)
            return;

        bool isRunning = _inputReader.IsRunning;
        Vector2 input = _inputReader.Move;
        Vector3 direction = new Vector3(input.x, 0f, input.y);
        Camera camera = Camera.main;
        if (camera != null)
        {
            Vector3 forward = camera.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = camera.transform.right;
            right.y = 0f;
            right.Normalize();
            direction = right * input.x + forward * input.y;
        }
        float inputAmount = direction.sqrMagnitude > 0f ? (isRunning ? 1f : 0.5f) : 0f;

        if (inputAmount > 0f)
        {
            _hasMoveTarget = false;
        }
        else if (_hasMoveTarget)
        {
            direction = _moveTarget - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= stoppingDistance * stoppingDistance)
            {
                _hasMoveTarget = false;
                direction = Vector3.zero;
            }
            else
            {
                inputAmount = isRunning ? 1f : 0.5f;
            }
        }

        if (inputAmount > 0f)
        {
            float step = (isRunning ? runSpeed : moveSpeed) * Time.deltaTime;
            if (_hasMoveTarget)
                step = Mathf.Min(step, direction.magnitude);

            direction.Normalize();
            transform.position += direction * step;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        UpdateVerticalMovement();
        _playerUnit.animator.SetFloat(WeaponStyleParameter, (float)weaponStyle);
        _playerUnit.animator.SetFloat(
            SpeedParameter,
            inputAmount,
            animationDampTime,
            Time.deltaTime);
    }

    private void OnWeaponStyleRequested(WeaponAnimationStyle style)
    {
        _playerUnit.EquipWeapon(style);
    }

    private void OnMoveRequested(Vector2 screenPosition)
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        Ray ray = camera.ScreenPointToRay(screenPosition);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, _groundHeight, 0f));
        if (!ground.Raycast(ray, out float distance))
            return;

        _moveTarget = ray.GetPoint(distance);
        _moveTarget.y = _groundHeight;
        _hasMoveTarget = true;
    }

    private void OnJump()
    {
        if (!_isGrounded || _playerUnit.animator == null)
            return;

        _isGrounded = false;
        _verticalSpeed = jumpSpeed;
        _playerUnit.animator.SetTrigger(JumpParameter);
    }

    private void OnAttack()
    {
        if (_playerUnit.animator == null || weaponStyle == WeaponAnimationStyle.Unarmed)
            return;

        float now = Time.time;
        if (now - _lastAttackTime >= comboResetTime)
            _attackIndex = 0;

        PlayAttack((weaponStyle == WeaponAnimationStyle.TwinDagger ? 0 : 3) + _attackIndex);
        _attackIndex = (_attackIndex + 1) % 3;
        _lastAttackTime = now;
    }

    private void OnAttackSkill()
    {
        if (_playerUnit.animator == null || weaponStyle != WeaponAnimationStyle.Greatsword)
            return;

        PlayAttack(7);
        _attackIndex = 0;
        _lastAttackTime = float.NegativeInfinity;
    }

    private void OnAttackPreview(int slot)
    {
        if (_playerUnit.animator == null || slot < 1 || slot > 9)
            return;

        int variant = slot <= 5 ? slot + 2 : slot <= 8 ? slot - 6 : 8;
        PlayAttack(variant);
    }

    private void PlayAttack(int variant)
    {
        _playerUnit.animator.SetInteger(AttackVariantParameter, variant);
        _playerUnit.animator.SetTrigger(AttackParameter);
    }

    private void UpdateVerticalMovement()
    {
        if (_isGrounded)
            return;

        float gravityMultiplier = _verticalSpeed < 0f ? fallGravityMultiplier : 1f;
        _verticalSpeed -= gravity * gravityMultiplier * Time.deltaTime;
        _verticalSpeed = Mathf.Max(_verticalSpeed, -maxFallSpeed);
        transform.position += Vector3.up * (_verticalSpeed * Time.deltaTime);

        if (transform.position.y > _groundHeight || _verticalSpeed > 0f)
            return;

        Vector3 position = transform.position;
        position.y = _groundHeight;
        transform.position = position;
        _verticalSpeed = 0f;
        _isGrounded = true;
    }
}