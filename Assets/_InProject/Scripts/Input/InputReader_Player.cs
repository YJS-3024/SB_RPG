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
    public event Action AttackSkill;
    public event Action<int> AttackPreview;

    private bool _isMoving = false;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dodgeAction;
    private InputAction _attackAction;
    private InputAction _attackSkillAction;

    private void OnEnable()
    {
        InputManager.Instance.Initialize();
        _moveAction = InputManager.Instance.MoveAction;
        _jumpAction = InputManager.Instance.JumpAction;
        _dodgeAction = InputManager.Instance.DodgeAction;
        _attackAction = InputManager.Instance.AttackAction;
        _attackSkillAction = InputManager.Instance.AttackSkillAction;

        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;

        _jumpAction.performed += OnJump;
        _dodgeAction.performed += OnDodge;

        _attackAction.performed += OnAttack;
        _attackSkillAction.performed += OnAttackSkill;
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
            _attackAction.performed -= OnAttack;

        if (_attackSkillAction != null)
            _attackSkillAction.performed -= OnAttackSkill;

        _moveAction = null;
        _jumpAction = null;
        _dodgeAction = null;
        _attackAction = null;
        _attackSkillAction = null;
        Move = Vector2.zero;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame) AttackPreview?.Invoke(1);
        if (keyboard.digit2Key.wasPressedThisFrame) AttackPreview?.Invoke(2);
        if (keyboard.digit3Key.wasPressedThisFrame) AttackPreview?.Invoke(3);
        if (keyboard.digit4Key.wasPressedThisFrame) AttackPreview?.Invoke(4);
        if (keyboard.digit5Key.wasPressedThisFrame) AttackPreview?.Invoke(5);
        if (keyboard.digit6Key.wasPressedThisFrame) AttackPreview?.Invoke(6);
        if (keyboard.digit7Key.wasPressedThisFrame) AttackPreview?.Invoke(7);
        if (keyboard.digit8Key.wasPressedThisFrame) AttackPreview?.Invoke(8);
        if (keyboard.digit9Key.wasPressedThisFrame) AttackPreview?.Invoke(9);
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

    private void OnAttackSkill(InputAction.CallbackContext context)
    {
        if (context.control.device is Keyboard)
            return;

        AttackSkill?.Invoke();
    }
}
