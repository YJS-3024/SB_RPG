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
    public event Action DodgePerformed;
    public event Action Attack;

    private bool _isMoving = false;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dodgeAction;
    private InputAction _attackAction;

    private void OnEnable()
    {
        InputManager.Instance.Initialize();
        _moveAction = InputManager.Instance.MoveAction;
        _jumpAction = InputManager.Instance.JumpAction;
        _dodgeAction = InputManager.Instance.DodgeAction;
        _attackAction = InputManager.Instance.AttackAction;

        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;

        _jumpAction.performed += OnJump;
        _dodgeAction.performed += OnDodge;

        _attackAction.started += OnAttack;
        _attackAction.performed += OnAttack;
        _attackAction.canceled += OnAttack;
    }

    private void OnDisable()
    {
        if (_moveAction != null)
        {
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
        }

        if (_jumpAction != null)
            _jumpAction.performed -= OnJump;

        if (_dodgeAction != null)
            _dodgeAction.performed -= OnDodge;

        if (_attackAction != null)
        {
            _attackAction.started -= OnAttack;
            _attackAction.performed -= OnAttack;
            _attackAction.canceled -= OnAttack;
        }

        _moveAction = null;
        _jumpAction = null;
        _dodgeAction = null;
        _attackAction = null;
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

    private void OnDodge(InputAction.CallbackContext context)
    {
        DodgePerformed?.Invoke();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        Attack?.Invoke();
    }
}