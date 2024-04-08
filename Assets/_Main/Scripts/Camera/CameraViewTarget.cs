using EventManager;
using TriInspector;
using UnityEngine;

public class CameraViewTarget : MonoBehaviour {
    [Title("Move")]
    [SerializeField] float moveSpeed;
    Vector3 forward;
    Vector3 right;
    Vector2 moveInput;

    public bool CanMove { get; private set; }

    Camera mainCam;

    void Awake() {
        mainCam = Camera.main;

        SetMovementAxes();
        EnableMovement();
    }

    void Update() { MoveView(); }

    void MoveView() {
        if (!CanMove) return;
        if (moveInput.sqrMagnitude != 0) {
            Vector3 moveDir = forward * moveInput.y + right * moveInput.x;
            Vector3 targetPosition = new Vector3(
                transform.position.x + moveDir.x,
                transform.position.y,
                transform.position.z + moveDir.z
            );
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    public void EnableMovement() {
        CanMove = true;
        // Prevent duplicate subs
        Ref.Player.PlayerInput.InputMove -= SetMoveInput;
        Ref.Player.PlayerInput.InputMove += SetMoveInput;
    }
    public void DisableMovement() {
        CanMove = false;
        Ref.Player.PlayerInput.InputMove -= SetMoveInput;
    }

    void SetMoveInput(MoveInputArgs moveInputArgs) { moveInput = moveInputArgs.MoveInput; }

    void SetMovementAxes() {
        forward = mainCam.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        right = mainCam.transform.right;
        right.y = 0f;
        right.Normalize();
    }
}