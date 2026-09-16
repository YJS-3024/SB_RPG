using UnityEngine;

[RequireComponent(typeof(InputReader_Player))]
[RequireComponent(typeof(PlayerUnit))]
public class PlayerUnitMovement : MonoBehaviour
{
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int JumpParameter = Animator.StringToHash("Jump");
    private static readonly int AttackParameter = Animator.StringToHash("Attack");
    private static readonly int AttackVariantParameter = Animator.StringToHash("AttackVariant");

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float jumpSpeed = 7.071f;
    [SerializeField] private float gravity = 15f;
    [SerializeField] private float fallGravityMultiplier = 1.6f;
    [SerializeField] private float maxFallSpeed = 12f;
    [SerializeField] private float animationDampTime = 0.1f;

    private InputReader_Player _inputReader;
    private PlayerUnit _playerUnit;
    private float _groundHeight;
    private float _verticalSpeed;
    private bool _isGrounded = true;
    private int _attackIndex;

    private void Awake()
    {
        _inputReader = GetComponent<InputReader_Player>();
        _playerUnit = GetComponent<PlayerUnit>();
        _groundHeight = transform.position.y;
    }

    private void OnEnable()
    {
        _inputReader.JumpPerformed += OnJump;
        _inputReader.Attack += OnAttack;
        _inputReader.AttackSkill += OnAttackSkill;
    }

    private void OnDisable()
    {
        _inputReader.JumpPerformed -= OnJump;
        _inputReader.Attack -= OnAttack;
        _inputReader.AttackSkill -= OnAttackSkill;
    }

    private void Update()
    {
        if (_playerUnit.animator == null)
            return;

        Vector2 input = _inputReader.Move;
        Vector3 direction = new Vector3(input.x, 0f, input.y);
        float inputAmount = Mathf.Clamp01(direction.magnitude);

        if (inputAmount > 0f)
        {
            direction.Normalize();
            transform.position += direction * (moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        UpdateVerticalMovement();
        _playerUnit.animator.SetFloat(
            SpeedParameter,
            inputAmount,
            animationDampTime,
            Time.deltaTime);
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
        if (_playerUnit.animator == null)
            return;

        PlayAttack(3 + _attackIndex);
        _attackIndex = (_attackIndex + 1) % 4;
    }

    private void OnAttackSkill()
    {
        if (_playerUnit.animator == null)
            return;

        PlayAttack(7);
        _attackIndex = 0;
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