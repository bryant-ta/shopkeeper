using System;
using EventManager;
using UnityEngine;
using UnityEngine.InputSystem;

// Should be attached to Player GameObject for movement inputs
[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {
    int playerID; // TEMP: gonna need somewhere to differentiate players in local multiplayer, eventually passed w/ inputs
    Camera mainCam;

    void Awake() { mainCam = Camera.main; }

    // uses Action Type "Button"
    public void OnPrimary(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100.0f)) {
                if (hit.collider != null) {
                    Events.Invoke(gameObject, EventID.PrimaryDown, new ClickInputArgs{TargetObj = hit.collider.gameObject});
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
        scrollInput /= Math.Abs(scrollInput); // normalize scroll value for easier usage later

        if (ctx.performed) {
            Events.Invoke(mainCam.gameObject, EventID.Scroll, scrollInput);
        }
    }

    public void OnPoint(InputAction.CallbackContext ctx) {
        print(ctx.ReadValue<Vector2>());
        if (ctx.performed) {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100.0f)) {
                if (hit.collider != null) {
                    Events.Invoke(gameObject, EventID.Point, hit.point);
                }
            }
        }
    }

    public void OnMove(InputAction.CallbackContext ctx) {
        Events.Invoke(gameObject, EventID.Movement, new MoveInputArgs() {MoveInput = ctx.ReadValue<Vector2>()});
    }
}