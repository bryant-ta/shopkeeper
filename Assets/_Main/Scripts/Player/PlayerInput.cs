using System;
using TriInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour {
    // TEMP: refactor
    public PlayerMovement playerMovement;
    public CameraController cameraCtrl;
    
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
    
    public void OnZoom(InputAction.CallbackContext ctx) {
        float scrollInput = ctx.ReadValue<Vector2>().y;
        scrollInput = scrollInput / Math.Abs(scrollInput);  // normalize scroll value for easier usage later
        
        if (ctx.performed) {
            cameraCtrl.ZoomView(scrollInput);
        }
    }
}