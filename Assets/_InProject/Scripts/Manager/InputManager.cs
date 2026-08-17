using System;
using UnityEngine;
using UnityEngine.InputSystem;
using yjs.DevKit;

public class InputManager : MonoSingleton<InputManager>
{
    private InputActions_RPG_SandBox bindingInput;

    public InputAction MoveAction => bindingInput.Gameplay.Move;
    public InputAction JumpAction => bindingInput.Gameplay.Jump;

    //  TODO :: PlayerInputReader 책임으로 옮길것
    private bool _isMoveDir = false;
    private Vector2 _moveVector = Vector2.zero;

    public override bool Initialize()
    {
        bindingInput = new InputActions_RPG_SandBox();
        bindingInput.Gameplay.Enable();

        //  TODO :: PlayerInputReader 책임으로 옮길것
        MoveAction.started += OnMoveStart;
        MoveAction.canceled += OnMoveCancel;
        MoveAction.performed += OnMovePerformed;

        return true;
    }

    private void OnMoveStart(InputAction.CallbackContext context)
    {
        if (context.ReadValue<Vector2>() != Vector2.zero)
        {
            _isMoveDir = true;
        }
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
        bindingInput?.Dispose();
    }
}
