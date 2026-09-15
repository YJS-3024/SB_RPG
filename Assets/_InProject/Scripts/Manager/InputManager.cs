using DevelopKit;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input Actions 생성, 활성화, 폐기 책임
/// </summary>
public class InputManager : MonoSingleton<InputManager>
{
    private InputActions_RPG_SandBox bindingInput;
    private bool _isInitialized;

    public InputAction MoveAction => bindingInput.Gameplay.Move;
    public InputAction JumpAction => bindingInput.Gameplay.Jump;
    public InputAction AttackAction => bindingInput.Gameplay.Attack;

    private bool _isMoveDir;
    private Vector2 _moveVector = Vector2.zero;

    public override bool Initialize()
    {
        if (_isInitialized)
            return true;

        bindingInput = new InputActions_RPG_SandBox();
        bindingInput.Gameplay.Enable();

        MoveAction.started += OnMoveStart;
        MoveAction.canceled += OnMoveCancel;
        MoveAction.performed += OnMovePerformed;

        _isInitialized = true;
        return true;
    }

    private void OnMoveStart(InputAction.CallbackContext context)
    {
        if (context.ReadValue<Vector2>() != Vector2.zero)
            _isMoveDir = true;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveVector = context.ReadValue<Vector2>();
    }

    private void OnMoveCancel(InputAction.CallbackContext context)
    {
        _moveVector = Vector2.zero;
        _isMoveDir = false;
    }

    protected override void Destroy()
    {
        if (bindingInput != null)
        {
            MoveAction.started -= OnMoveStart;
            MoveAction.canceled -= OnMoveCancel;
            MoveAction.performed -= OnMovePerformed;
            bindingInput.Dispose();
        }

        bindingInput = null;
        _isInitialized = false;
    }
}