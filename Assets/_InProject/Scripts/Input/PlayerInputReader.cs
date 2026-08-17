using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }

    public event Action JumpPerformed;

    private void OnEnable()
    {
        InputManager.Instance.MoveAction.performed += OnMove;
        InputManager.Instance.MoveAction.canceled += OnMove;
        InputManager.Instance.JumpAction.performed += OnJump;
    }

    private void OnDisable()
    {
        InputManager.Instance.MoveAction.performed -= OnMove;
        InputManager.Instance.MoveAction.canceled -= OnMove;
        InputManager.Instance.JumpAction.performed -= OnJump;
        Move = Vector2.zero;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Move = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        JumpPerformed?.Invoke();
    }
}
