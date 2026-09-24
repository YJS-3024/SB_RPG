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
    private static readonly int DashParameter = Animator.StringToHash("Dash");
    private static readonly int DashLeftParameter = Animator.StringToHash("DashLeft");
    private static readonly int DashRightParameter = Animator.StringToHash("DashRight");
    private static readonly int AttackParameter = Animator.StringToHash("Attack");
    private static readonly int AttackVariantParameter = Animator.StringToHash("AttackVariant");
    private static readonly int JumpState = Animator.StringToHash("Jump");
    private static readonly int DashState = Animator.StringToHash("Dash");
    private static readonly int DashLeftState = Animator.StringToHash("Dash_Left");
    private static readonly int DashRightState = Animator.StringToHash("Dash_Right");

    [SerializeField] private WeaponAnimationStyle weaponStyle = WeaponAnimationStyle.Greatsword;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField, Min(0f)] private float dashSpeed = 11f;
    [SerializeField, Min(0f)] private float sideDashSpeed = 11.25f;
    [SerializeField, Min(0.01f)] private float dashDuration = 0.3f;
    [SerializeField, Min(0.01f)] private float sideDashDuration = 0.267f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float jumpSpeed = 3.886f;
    [SerializeField, Min(0f)] private float jumpAcceleration = 20f;
    [SerializeField, Min(0f)] private float jumpAccelerationDuration = 0.11f;
    [SerializeField] private float gravity = 15f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float maxFallSpeed = 12f;
    [SerializeField] private float animationDampTime = 0.1f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.1f;
    [SerializeField, Min(0.01f)] private float comboResetTime = 1.5f;
    [SerializeField, Min(0f)] private float basicAttackCancelDelay = 0.45f;
    [SerializeField, Min(0f)] private float thirdAttackCancelDelay = 0.9f;
    [Header("Combat Judgement")]
    [SerializeField, Min(0f)] private float attackHitDelay = 0.22f;
    [SerializeField, Min(0.1f)] private float attackReach = 1.35f;
    [SerializeField, Min(0.1f)] private float attackRadius = 0.8f;
    [SerializeField, Min(1)] private int attackDamage = 20;

    private InputReader_Player _inputReader;
    private PlayerUnit _playerUnit;
    private float _groundHeight;
    private float _verticalSpeed;
    private float _jumpAccelerationRemaining;
    private bool _isGrounded = true;
    private Vector3 _moveTarget;
    private bool _hasMoveTarget;
    private int _previousDashInputDirection;
    private bool _dodgeRequested;
    private float _backDodgeReadyTime = float.NegativeInfinity;
    private float _dashEndTime = float.NegativeInfinity;
    private Vector3 _dashDirection;
    private float _currentDashSpeed;
    private int _attackIndex;
    private float _lastAttackTime = float.NegativeInfinity;
    private float _attackCancelUnlockTime = float.NegativeInfinity;
    private bool _hasQueuedBasicAttack;
    private bool _attackHitPending;
    private float _attackHitTime;

    public WeaponAnimationStyle WeaponStyle => weaponStyle;

public void SetWeaponStyle(WeaponAnimationStyle style)
    {
        weaponStyle = style;
        _attackIndex = 0;
        _lastAttackTime = float.NegativeInfinity;
        _attackCancelUnlockTime = float.NegativeInfinity;
        _hasQueuedBasicAttack = false;
        if (_playerUnit != null && _playerUnit.animator != null)
            UpdateActionWeaponState();
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
        _attackCancelUnlockTime = float.NegativeInfinity;
        _hasQueuedBasicAttack = false;
        _attackHitPending = false;
        _previousDashInputDirection = 0;
        _dodgeRequested = false;
        _backDodgeReadyTime = float.NegativeInfinity;
        _dashEndTime = float.NegativeInfinity;
        _inputReader.JumpPerformed += OnJump;
        _inputReader.DodgePerformed += OnDodge;
        _inputReader.Attack += OnAttack;
        _inputReader.AttackSkill += OnAttackSkill;
        _inputReader.AttackPreview += OnAttackPreview;
        _inputReader.MoveRequested += OnMoveRequested;
        _inputReader.WeaponStyleRequested += OnWeaponStyleRequested;
    }

    private void OnDisable()
    {
        _inputReader.JumpPerformed -= OnJump;
        _inputReader.DodgePerformed -= OnDodge;
        _inputReader.Attack -= OnAttack;
        _inputReader.AttackSkill -= OnAttackSkill;
        _inputReader.AttackPreview -= OnAttackPreview;
        _inputReader.MoveRequested -= OnMoveRequested;
        _inputReader.WeaponStyleRequested -= OnWeaponStyleRequested;
        _playerUnit.SetWeaponsVisible(true);
    }

private void Update()
    {
        if (_playerUnit.animator == null)
            return;

        UpdateAttackHit();

        if (_playerUnit.IsDead || _playerUnit.IsHitReacting)
        {
            _hasQueuedBasicAttack = false;
            _attackHitPending = false;
            _hasMoveTarget = false;
            _dodgeRequested = false;
            _dashEndTime = float.NegativeInfinity;
            _backDodgeReadyTime = float.NegativeInfinity;
            UpdateVerticalMovement();
            _playerUnit.SetWeaponsVisible(true);
            _playerUnit.animator.SetFloat(WeaponStyleParameter, (float)weaponStyle);
            _playerUnit.animator.SetFloat(SpeedParameter, 0f);
            return;
        }

        UpdateQueuedBasicAttack();
        if (UpdateDash())
        {
            UpdateActionWeaponState();
            return;
        }

        bool isAttackLocked = IsBasicAttackCancelLocked();
        bool isRunning = _inputReader.IsRunning;
        Vector2 input = _inputReader.Move;
        Vector3 direction = GetKeyboardMoveDirection(input);
        float inputAmount = direction.sqrMagnitude > 0f ? (isRunning ? 1f : 0.5f) : 0f;

        if (inputAmount > 0f)
        {
            _hasMoveTarget = false;
        }
        else if (_hasMoveTarget && !isAttackLocked)
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

            if (_hasMoveTarget)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }

        UpdateVerticalMovement();
        UpdateActionWeaponState();
        float animationSpeed = inputAmount;
        if (!_hasMoveTarget && weaponStyle != WeaponAnimationStyle.Unarmed && input.y < -0.1f)
            animationSpeed = -inputAmount;

        _playerUnit.animator.SetFloat(
            SpeedParameter,
            animationSpeed,
            animationDampTime,
            Time.deltaTime);
    }


    private void FaceCameraForward()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 cameraForward = camera.transform.forward;
        cameraForward.y = 0f;
        if (cameraForward.sqrMagnitude <= Mathf.Epsilon)
            return;

        transform.rotation = Quaternion.LookRotation(cameraForward.normalized);
    }

private Vector3 GetKeyboardMoveDirection(Vector2 input)
    {
        Camera camera = Camera.main;
        if (camera == null)
            return transform.right * input.x + transform.forward * input.y;

        Vector3 cameraForward = camera.transform.forward;
        cameraForward.y = 0f;
        if (cameraForward.sqrMagnitude <= Mathf.Epsilon)
            return transform.right * input.x + transform.forward * input.y;

        cameraForward.Normalize();
        if (input.sqrMagnitude > Mathf.Epsilon)
        {
            bool usesBackwardWalk = weaponStyle != WeaponAnimationStyle.Unarmed &&
                input.y < -Mathf.Epsilon;
            transform.rotation = Quaternion.LookRotation(
                usesBackwardWalk || input.y >= -Mathf.Epsilon ? cameraForward : -cameraForward);
        }

        Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
        return cameraRight * input.x + cameraForward * input.y;
    }

private void OnWeaponStyleRequested(WeaponAnimationStyle style)
    {
        if (_playerUnit.IsDead || _playerUnit.IsHitReacting || IsBasicAttackCancelLocked() || IsDashing())
            return;

        _playerUnit.EquipWeapon(style);
    }

    private void OnMoveRequested(Vector2 screenPosition)
    {
        if (_playerUnit.IsDead || _playerUnit.IsHitReacting || IsDashing())
            return;

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
        if (!_isGrounded || _playerUnit.animator == null || _playerUnit.IsDead ||
            _playerUnit.IsHitReacting || IsBasicAttackCancelLocked() || IsDashing())
            return;

        _isGrounded = false;
        _verticalSpeed = jumpSpeed;
        _jumpAccelerationRemaining = jumpAccelerationDuration;
        _playerUnit.animator.SetTrigger(JumpParameter);
        UpdateActionWeaponState();
    }

private void OnDodge()
    {
        if (_playerUnit.IsDead || _playerUnit.IsHitReacting)
            return;

        _dodgeRequested = true;
    }


private void OnAttack()
    {
        if (_playerUnit.animator == null || _playerUnit.IsDead || _playerUnit.IsHitReacting ||
            weaponStyle == WeaponAnimationStyle.Unarmed || IsDashing())
            return;

        if (IsBasicAttackCancelLocked())
        {
            _hasQueuedBasicAttack = true;
            return;
        }

        ExecuteBasicAttack();
    }

private void ExecuteBasicAttack()
    {
        float now = Time.time;
        if (now - _lastAttackTime >= comboResetTime)
            _attackIndex = 0;

        int currentAttackIndex = _attackIndex;
        PlayAttack((weaponStyle == WeaponAnimationStyle.TwinDagger ? 0 : 3) + currentAttackIndex);
        _attackIndex = (currentAttackIndex + 1) % 3;
        _lastAttackTime = now;
        float cancelDelay = currentAttackIndex == 2
            ? Mathf.Max(basicAttackCancelDelay, thirdAttackCancelDelay)
            : basicAttackCancelDelay;
        _attackCancelUnlockTime = now + cancelDelay;
        _hasQueuedBasicAttack = false;
    }

    private void UpdateQueuedBasicAttack()
    {
        if (!_hasQueuedBasicAttack || IsBasicAttackCancelLocked())
            return;

        ExecuteBasicAttack();
    }

    private bool IsBasicAttackCancelLocked()
    {
        return Time.time < _attackCancelUnlockTime;
    }


private void OnAttackSkill()
    {
        if (_playerUnit.animator == null || _playerUnit.IsDead || _playerUnit.IsHitReacting ||
            weaponStyle != WeaponAnimationStyle.Greatsword ||
            IsBasicAttackCancelLocked() || IsDashing())
            return;

        PlayAttack(7);
        _attackIndex = 0;
        _lastAttackTime = float.NegativeInfinity;
    }

private void OnAttackPreview(int slot)
    {
        if (_playerUnit.animator == null || _playerUnit.IsDead || _playerUnit.IsHitReacting ||
            slot < 1 ||
            slot > 9 ||
            IsBasicAttackCancelLocked() || IsDashing())
            return;

        int variant = slot <= 5 ? slot + 2 : slot <= 8 ? slot - 6 : 8;
        PlayAttack(variant);
    }

    private void PlayAttack(int variant)
    {
        _playerUnit.animator.SetInteger(AttackVariantParameter, variant);
        _playerUnit.animator.SetTrigger(AttackParameter);
        _attackHitPending = true;
        _attackHitTime = Time.time + attackHitDelay;
    }

    private void UpdateAttackHit()
    {
        if (!_attackHitPending || Time.time < _attackHitTime)
            return;

        _attackHitPending = false;
        if (_playerUnit.IsDead || _playerUnit.IsHitReacting)
            return;

        Vector3 center = transform.position + Vector3.up + transform.forward * attackReach;
        CombatHitUtility.DamageSphere(gameObject, center, attackRadius, attackDamage);
    }

private void UpdateVerticalMovement()
    {
        if (_isGrounded)
            return;

        float deltaTime = Time.deltaTime;
        float poweredTime = _verticalSpeed > 0f ? Mathf.Min(_jumpAccelerationRemaining, deltaTime) : 0f;
        _jumpAccelerationRemaining -= poweredTime;
        float gravityMultiplier = _verticalSpeed < 0f ? fallGravityMultiplier : 1f;
        _verticalSpeed += jumpAcceleration * poweredTime;
        _verticalSpeed -= gravity * gravityMultiplier * deltaTime;
        _verticalSpeed = Mathf.Max(_verticalSpeed, -maxFallSpeed);
        transform.position += Vector3.up * (_verticalSpeed * deltaTime);

        if (transform.position.y > _groundHeight || _verticalSpeed > 0f)
            return;

        Vector3 position = transform.position;
        position.y = _groundHeight;
        transform.position = position;
        _verticalSpeed = 0f;
        _jumpAccelerationRemaining = 0f;
        _isGrounded = true;
    }


private bool UpdateDash()
    {
        Vector2 move = _inputReader.Move;
        bool dodgeRequested = _dodgeRequested;
        _dodgeRequested = false;
        int dashInput = 0;
        if (_inputReader.IsRunning || dodgeRequested)
        {
            if (move.y > 0.5f && Mathf.Abs(move.x) < 0.5f)
                dashInput = 1;
            else if (move.y < -0.5f && Mathf.Abs(move.x) < 0.5f)
                dashInput = 4;
            else if (move.x < -0.5f && Mathf.Abs(move.y) < 0.5f)
                dashInput = 2;
            else if (move.x > 0.5f && Mathf.Abs(move.y) < 0.5f)
                dashInput = 3;
        }

        if (dodgeRequested && move.sqrMagnitude < 0.25f)
            dashInput = 4;

        bool shouldStart = dashInput != 0 &&
            (dashInput != _previousDashInputDirection || dodgeRequested);
        _previousDashInputDirection = dashInput;

        if (shouldStart && _isGrounded && !IsBasicAttackCancelLocked() && !IsDashing())
        {
            if (dashInput == 4)
            {
                if (Time.time >= _backDodgeReadyTime)
                    return PerformBackDodge(move.y < -0.5f);
            }
            else
            {
                if (dashInput == 1)
                    FaceCameraForward();

                _dashDirection = dashInput == 1
                    ? transform.forward
                    : GetKeyboardMoveDirection(new Vector2(dashInput == 2 ? -1f : 1f, 0f));
                _hasMoveTarget = false;
                _dashEndTime = Time.time + (dashInput == 1 ? dashDuration : sideDashDuration);
                _currentDashSpeed = dashInput == 1 ? dashSpeed : sideDashSpeed;
                int trigger = dashInput == 1 ? DashParameter
                    : dashInput == 2 ? DashLeftParameter : DashRightParameter;
                _playerUnit.animator.SetTrigger(trigger);
            }
        }

        if (!IsDashing())
            return false;

        transform.position += _dashDirection * (_currentDashSpeed * Time.deltaTime);
        _playerUnit.animator.SetFloat(SpeedParameter, 1f);
        return true;
    }

private bool PerformBackDodge(bool useCameraBackward)
    {
        Vector3 backward = -transform.forward;
        if (useCameraBackward)
        {
            Camera camera = Camera.main;
            if (camera != null)
                backward = -camera.transform.forward;
        }

        backward.y = 0f;
        if (backward.sqrMagnitude <= Mathf.Epsilon)
            return false;

        transform.position += backward.normalized * (sideDashSpeed * sideDashDuration);
        _hasMoveTarget = false;
        _backDodgeReadyTime = Time.time + sideDashDuration;
        _playerUnit.animator.SetFloat(SpeedParameter, 0f);
        return true;
    }


    private void UpdateActionWeaponState()
    {
        if (_playerUnit.animator == null)
            return;

        bool actionActive = !_isGrounded || IsDashing() ||
            Time.time < _backDodgeReadyTime || IsActionAnimationPlaying();
        _playerUnit.SetWeaponsVisible(!actionActive);
        _playerUnit.animator.SetFloat(
            WeaponStyleParameter,
            actionActive ? (float)WeaponAnimationStyle.Unarmed : (float)weaponStyle);
    }

    private bool IsActionAnimationPlaying()
    {
        Animator animator = _playerUnit.animator;
        if (animator.runtimeAnimatorController == null)
            return false;

        if (IsJumpOrDodgeState(animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            return true;

        return animator.IsInTransition(0) &&
            IsJumpOrDodgeState(animator.GetNextAnimatorStateInfo(0).shortNameHash);
    }

    private static bool IsJumpOrDodgeState(int stateHash)
    {
        return stateHash == JumpState || stateHash == DashState ||
            stateHash == DashLeftState || stateHash == DashRightState;
    }

    private bool IsDashing()
    {
        return Time.time < _dashEndTime;
    }
}