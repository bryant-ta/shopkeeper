using System;
using TriInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour {
    // TEMP: refactor with events
    public PlayerMovement playerMovement;
    public CameraController cameraCtrl;

    int playerID;   // TEMP: gonna need somewhere to differentiate players in local multiplayer, eventually passed w/ inputs
    
    public void OnMove(InputAction.CallbackContext context) {
        playerMovement.moveInput = context.ReadValue<Vector2>();
    }

    // uses Action Type "Button"
    public void OnPrimary(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100.0f)) {
                if (hit.collider != null) {
                    EventManager.Invoke(hit.collider.gameObject, EventID.PrimaryDown);
                }
            }
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