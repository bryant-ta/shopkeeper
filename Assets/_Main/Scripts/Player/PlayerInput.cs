using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {
    [Tooltip("Point raycast detects this layer.")]
    [field: SerializeField] public LayerMask PointLayer { get; private set; }

    int playerID; // TEMP: gonna need somewhere to differentiate players in local multiplayer, eventually passed w/ inputs
    Camera mainCam;
    EventSystem eventSystem;
    [SerializeField] GraphicRaycaster graphicRaycaster;

    UnityEngine.InputSystem.PlayerInput playerInput; // TEMP: change for local multiplayer

    void Awake() {
        mainCam = Camera.main;
        eventSystem = FindObjectOfType<EventSystem>();

        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();

        SetActionMap(Constants.ActionMapNamePlayer);
    }

    #region Mouse

    public event Action<ClickInputArgs> InputPrimaryDown;
    public event Action<ClickInputArgs> InputPrimaryUp;
    public event Action<bool> InputPrimaryDownMod;
    public event Action<ClickInputArgs> InputSecondaryDown;
    public event Action<ClickInputArgs> InputSecondaryUp;
    public event Action<ClickInputArgs> InputPoint;

    // uses Action Type "Button"
    public void OnPrimary(InputAction.CallbackContext ctx) {
        ClickInputArgs clickInputArgs = ClickInputArgsRaycast(cursorPosition);
        if (clickInputArgs.TargetObj == null) return;

        if (ctx.performed) {
            InputPrimaryDown?.Invoke(clickInputArgs);
            if (modifierIsHeld) {
                InputPrimaryDownMod?.Invoke(true);
            }
        } else if (ctx.canceled) {
            InputPrimaryUp?.Invoke(clickInputArgs);
        }
    }

    public void OnSecondary(InputAction.CallbackContext ctx) {
        ClickInputArgs clickInputArgs = ClickInputArgsRaycast(cursorPosition);
        if (clickInputArgs.TargetObj == null) return;

        if (ctx.performed) {
            InputSecondaryDown?.Invoke(clickInputArgs);
        } else if (ctx.canceled) {
            InputSecondaryUp?.Invoke(clickInputArgs);
        }
    }

    // Sends collision point from cursor raycast
    Vector2 cursorPosition;
    public void OnPoint(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            cursorPosition = ctx.ReadValue<Vector2>();
        }
    }

    void Update() {
        ClickInputArgs clickInputArgs = ClickInputArgsRaycast(cursorPosition);
        if (clickInputArgs.TargetObj == null) return;
        
        InputPoint?.Invoke(clickInputArgs);
    }

    ClickInputArgs ClickInputArgsRaycast(Vector2 cursorPos) {
        ClickInputArgs clickInputArgs = new();

        // Ignore click if cursor is over UI 
        if (IsPointerOverUIElement(cursorPos)) { return clickInputArgs; }

        Ray ray = mainCam.ScreenPointToRay(cursorPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100.0f, PointLayer, QueryTriggerInteraction.Ignore)) {
            if (hit.collider != null) {
                clickInputArgs.CursorPos = cursorPos;
                clickInputArgs.HitNormal = hit.normal;
                clickInputArgs.HitPoint = hit.point;
                clickInputArgs.TargetObj = hit.collider.gameObject;
            }
        }

        return clickInputArgs;
    }
    bool IsPointerOverUIElement(Vector2 cursorPos) {
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = cursorPos;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, raycastResults);

        return raycastResults.Count > 0;
    }

    #endregion

    #region Tools

    public event Action<float> InputScroll;
    public event Action InputDragTool;
    public event Action InputSliceTool;
    public event Action InputCompactTool;
    bool modifierIsHeld;

    public void OnScroll(InputAction.CallbackContext ctx) {
        float scrollInput = ctx.ReadValue<Vector2>().y;
        scrollInput /= Math.Abs(scrollInput); // normalize scroll value for easier usage later

        if (ctx.performed) {
            InputScroll?.Invoke(scrollInput);
        }
    }
    
    public void OnDragTool(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputDragTool?.Invoke();
        }
    }

    public void OnSliceTool(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputSliceTool?.Invoke();
        }
    }

    public void OnCompactTool(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputCompactTool?.Invoke();
        }
    }
    
    public void OnModifier(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            modifierIsHeld = true;
        } else if (ctx.canceled) {
            modifierIsHeld = false;
        }
    }

    #endregion

    #region Camera

    public event Action<float> InputRotateCameraDown;
    public event Action<float> InputRotateCameraUp;

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
            InputRotateCameraDown?.Invoke(rotateCameraInput);
        } else if (ctx.canceled) {
            InputRotateCameraUp?.Invoke(rotateCameraInput);
        }
    }

    #endregion

    #region Interact

    public event Action InputInteract;
    public event Action<bool> InputRotate;
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

    public void OnRotateCW(InputAction.CallbackContext ctx) {
        // if (ctx.performed || ctx.canceled) { // essentially detecting hold
        if (ctx.performed) {
            InputRotate?.Invoke(true);
        }
    }

    public void OnRotateCCW(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputRotate?.Invoke(false);
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

    public void OnMove(InputAction.CallbackContext ctx) { InputMove?.Invoke(new MoveInputArgs() {MoveInput = ctx.ReadValue<Vector2>()}); }

    public void OnDash(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            InputDash?.Invoke();
        }
    }

    #endregion

    public event Action InputCancel;

    public void OnPause(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            GameManager.Instance.TogglePause();
            playerInput.SwitchCurrentActionMap(GameManager.Instance.IsPaused ? Constants.ActionMapNameUI : Constants.ActionMapNameGlobal);
        }
    }

    #region Helper

    // TEMP: make more robust if more than two action maps
    void SetActionMap(string mapName) {
        if (mapName == Constants.ActionMapNamePlayer) {
            playerInput.actions.FindActionMap(Constants.ActionMapNamePlayer).Enable();
        } else {
            Debug.LogError("Invalid action map name.");
        }
    }

    public void SetAction(string mapName, string actionName, bool enable) {
        if (enable) {
            playerInput.actions.FindAction(mapName + "/" + actionName).Enable();
        } else {
            playerInput.actions.FindAction(mapName + "/" + actionName).Disable();
        }
    }

    #endregion
}