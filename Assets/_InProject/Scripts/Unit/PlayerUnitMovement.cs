using UnityEngine;

[RequireComponent(typeof(InputReader_Player))]
[RequireComponent(typeof(PlayerUnit))]
public class PlayerUnitMovement : MonoBehaviour
{
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int JumpParameter = Animator.StringToHash("Jump");

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float gravity = 15f;
    [SerializeField] private float animationDampTime = 0.1f;

    private InputReader_Player _inputReader;
    private PlayerUnit _playerUnit;
    private float _groundHeight;
    private float _verticalSpeed;
    private bool _isGrounded = true;

    private void Awake()
    {
        _inputReader = GetComponent<InputReader_Player>();
        _playerUnit = GetComponent<PlayerUnit>();
        _groundHeight = transform.position.y;
    }

    private void OnEnable()
    {
        _inputReader.JumpPerformed += OnJump;
    }

    private void OnDisable()
    {
        _inputReader.JumpPerformed -= OnJump;
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

    private void UpdateVerticalMovement()
    {
        if (_isGrounded)
            return;

        _verticalSpeed -= gravity * Time.deltaTime;
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