using UnityEngine;

[RequireComponent(typeof(InputReader_Player))]
[RequireComponent(typeof(PlayerUnit))]
public class PlayerUnitMovement : MonoBehaviour
{
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float animationDampTime = 0.1f;

    private InputReader_Player _inputReader;
    private PlayerUnit _playerUnit;

    private void Awake()
    {
        _inputReader = GetComponent<InputReader_Player>();
        _playerUnit = GetComponent<PlayerUnit>();
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

        _playerUnit.animator.SetFloat(
            SpeedParameter,
            inputAmount,
            animationDampTime,
            Time.deltaTime);
    }
}