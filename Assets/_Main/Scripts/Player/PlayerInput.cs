using TriInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour {
    public PlayerMovement playerMovement;
    
    public void OnMove(InputAction.CallbackContext context) {
        playerMovement.moveInput = context.ReadValue<Vector2>();
    }

    // uses Action Type "Button"
    public void OnPrimary(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            print("hi");
        }
    }

    public void OnSecondary(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
        }

        if (ctx.canceled) {
        }
    }
}