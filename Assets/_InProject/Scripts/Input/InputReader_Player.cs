using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 액션별 콜백과 입력 상태 보관의 책임
/// </summary>
public class InputReader_Player : MonoBehaviour
{
    public Vector2 Move { get; private set; }

    public event Action JumpPerformed;
    public event Action Attack;

    private bool _isMoving = false;

    private void OnEnable()
    {
        InputManager.Instance.MoveAction.performed += OnMove;
        InputManager.Instance.MoveAction.canceled += OnMove;

        InputManager.Instance.JumpAction.performed += OnJump;

        InputManager.Instance.AttackAction.started += OnAttack;
        InputManager.Instance.AttackAction.performed += OnAttack;
        InputManager.Instance.AttackAction.canceled += OnAttack;
    }

    private void OnDisable()
    {
        InputManager.Instance.MoveAction.performed -= OnMove;
        InputManager.Instance.MoveAction.canceled -= OnMove;

        InputManager.Instance.JumpAction.performed -= OnJump;

        InputManager.Instance.AttackAction.started -= OnAttack;
        InputManager.Instance.AttackAction.performed -= OnAttack;
        InputManager.Instance.AttackAction.canceled -= OnAttack;

        Move = Vector2.zero;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Move = context.ReadValue<Vector2>();
        _isMoving = Move != Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        JumpPerformed?.Invoke();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        Attack?.Invoke();
    }
}
