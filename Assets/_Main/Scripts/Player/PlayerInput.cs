using System;
using EventManager;
using UnityEngine;
using UnityEngine.InputSystem;

// Should be attached to Player GameObject for movement inputs
[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {
    [Tooltip("Point raycast detects this layer.")]
    [SerializeField] LayerMask pointLayer;

    int playerID; // TEMP: gonna need somewhere to differentiate players in local multiplayer, eventually passed w/ inputs
    Camera mainCam;
    
    UnityEngine.InputSystem.PlayerInput playerInput; // TEMP: change for local multiplayer

    void Awake() {
        mainCam = Camera.main;
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
    }

    #region Mouse

    // uses Action Type "Button"
    public void OnPrimary(InputAction.CallbackContext ctx) {
        ClickInputArgs clickInputArgs = new();
        if (ctx.performed || ctx.canceled) {
            Ray ray = mainCam.ScreenPointToRay(cursorPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100.0f, pointLayer, QueryTriggerInteraction.Ignore)) {
                if (hit.collider != null) {
                    clickInputArgs.HitNormal = hit.normal;
                    clickInputArgs.HitPoint = hit.point;
                    clickInputArgs.TargetObj = hit.collider.gameObject;
                }
            }
        }

        if (clickInputArgs.TargetObj == null) return;

        if (ctx.performed) {
            Events.Invoke(gameObject, EventID.PrimaryDown, clickInputArgs);
        } else if (ctx.canceled) {
            Events.Invoke(gameObject, EventID.PrimaryUp, clickInputArgs);
        }
    }

    public void OnSecondary(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            Ray ray = mainCam.ScreenPointToRay(cursorPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100.0f, pointLayer, QueryTriggerInteraction.Ignore)) {
                if (hit.collider != null) {
                    Events.Invoke(gameObject, EventID.SecondaryDown, new ClickInputArgs {TargetObj = hit.collider.gameObject});
                }
            }
        } else if (ctx.canceled) {
            Events.Invoke(gameObject, EventID.SecondaryDown);
        }
    }

    public void OnZoom(InputAction.CallbackContext ctx) {
        float scrollInput = ctx.ReadValue<Vector2>().y;
        scrollInput /= Math.Abs(scrollInput); // normalize scroll value for easier usage later

        if (ctx.performed) {
            Events.Invoke(mainCam.gameObject, EventID.Scroll, scrollInput);
        }
    }

    // Sends collision point from cursor raycast
    public void OnPoint(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            cursorPosition = ctx.ReadValue<Vector2>();
        }
    }

    Vector2 cursorPosition;
    void Update() {
        Ray ray = mainCam.ScreenPointToRay(cursorPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100.0f, pointLayer, QueryTriggerInteraction.Ignore)) {
            if (hit.collider != null) {
                ClickInputArgs clickInputArgs = new ClickInputArgs {
                    HitNormal = hit.normal,
                    HitPoint = hit.point,
                    TargetObj = hit.collider.gameObject,
                };
                Events.Invoke(gameObject, EventID.Point, clickInputArgs);
            }
        }
    }

    #endregion
    
    public void OnRotateCamera(InputAction.CallbackContext ctx) {
        float rotateCameraInput = ctx.ReadValue<float>();
        if (ctx.performed) {
            Events.Invoke(mainCam.gameObject, EventID.RotateCamera, rotateCameraInput);
        }
    }

    #region Interact
    
    public void OnInteract(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            Events.Invoke(gameObject, EventID.Interact);
        }
    }
    
    public void OnCancel(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            Events.Invoke(gameObject, EventID.Cancel);
        }
    }

    public void OnRotate(InputAction.CallbackContext ctx) {
        float rotateInput = ctx.ReadValue<float>();
        if (ctx.performed || ctx.canceled) { // essentially detecting hold
            Events.Invoke(gameObject, EventID.Rotate, rotateInput);
        }
    }

    public void OnDrop(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            Events.Invoke(gameObject, EventID.Drop);
        }
    }
    
    #endregion

    #region Movement

    public void OnMove(InputAction.CallbackContext ctx) {
        Events.Invoke(gameObject, EventID.Move, new MoveInputArgs() {MoveInput = ctx.ReadValue<Vector2>()});
    }
    
    public void OnDash(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            Events.Invoke(gameObject, EventID.Dash);
        }
    }
    
    #endregion
    
    public void OnPause(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            GameManager.Instance.TogglePause();
            playerInput.SwitchCurrentActionMap(GameManager.Instance.IsPaused ? "UI" : "Player");
        }
    }
}