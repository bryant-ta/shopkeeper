using System;
using EventManager;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    public event Action<ClickInputArgs> InputPrimaryDown;
    public event Action<ClickInputArgs> InputPrimaryUp;
    public event Action<ClickInputArgs> InputSecondaryDown;
    public event Action<ClickInputArgs> InputSecondaryUp;
    public event Action<ClickInputArgs> InputPoint;

    // uses Action Type "Button"
    public void OnPrimary(InputAction.CallbackContext ctx) {
        ClickInputArgs clickInputArgs = ClickInputArgsRaycast(ctx);
        if (clickInputArgs.TargetObj == null) return;

        if (ctx.performed) {
            InputPrimaryDown?.Invoke(clickInputArgs);
        } else if (ctx.canceled) {
            InputPrimaryUp?.Invoke(clickInputArgs);
        }
    }

    public void OnSecondary(InputAction.CallbackContext ctx) {
        ClickInputArgs clickInputArgs = ClickInputArgsRaycast(ctx);
        if (clickInputArgs.TargetObj == null) return;
        
        if (ctx.performed) {
            InputSecondaryDown?.Invoke(clickInputArgs);
        } else if (ctx.canceled) {
            InputSecondaryUp?.Invoke(clickInputArgs);
        }
    }

    ClickInputArgs ClickInputArgsRaycast(InputAction.CallbackContext ctx) {
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

        return clickInputArgs;
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
                InputPoint?.Invoke(clickInputArgs);
            }
        }
    }

    #endregion

    #region Camera

    public event Action<float> InputScroll;
    public event Action<float> InputRotateCamera;
    
    public void OnZoom(InputAction.CallbackContext ctx) {
        float scrollInput = ctx.ReadValue<Vector2>().y;
        scrollInput /= Math.Abs(scrollInput); // normalize scroll value for easier usage later

        if (ctx.performed) {
            InputScroll?.Invoke(scrollInput);
        }
    }
    
    public void OnRotateCamera(InputAction.CallbackContext ctx) {
        float rotateCameraInput = ctx.ReadValue<float>();
        if (ctx.performed) {
            InputRotateCamera?.Invoke(rotateCameraInput);
        }
    }

    #endregion

    #region Interact
    
    public event Action InputInteract;
    public event Action InputCancel;
    public event Action<float> InputRotate;
    public event Action InputDrop;
    
    public void OnInteract(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputInteract?.Invoke();
        }
    }
    
    public void OnCancel(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputCancel?.Invoke();
        }
    }

    public void OnRotate(InputAction.CallbackContext ctx) {
        float rotateInput = ctx.ReadValue<float>();
        if (ctx.performed || ctx.canceled) { // essentially detecting hold
            InputRotate?.Invoke(rotateInput);
        }
    }

    public void OnDrop(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputDrop?.Invoke();
        }
    }
    
    #endregion

    #region Movement
    
    public event Action<MoveInputArgs> InputMove;
    public event Action InputDash;

    public void OnMove(InputAction.CallbackContext ctx) {
        InputMove?.Invoke(new MoveInputArgs() {MoveInput = ctx.ReadValue<Vector2>()});
    }
    
    public void OnDash(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputDash?.Invoke();
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