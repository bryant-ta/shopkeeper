using System;
using EventManager;
using Timers;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Header("Basic Movement")]
    [SerializeField] float initialMoveSpeed;
    [SerializeField] float rotationSpeed;
    float moveSpeed;
    Vector3 forward;
    Vector3 right;
    Vector2 moveInput;
    float rotateInput; // TEMP: unused for now, for rotating products in hand

    public bool CanMove { get; private set; }

    [Header("Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashCooldown;
    [SerializeField] float dashDuration;
    CountdownTimer dashCooldownTimer;
    CountdownTimer dashDurationTimer;
    bool hasSecondDash = true;

    [SerializeField] ParticleSystem dashPs;

    public bool CanDash { get; private set; }
    
    /***********************************************************/
    
    Camera mainCam;
    Rigidbody rb;

    public event Action OnMovement;

    void Awake() {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();

        dashCooldownTimer = new CountdownTimer(dashCooldown);
        dashCooldownTimer.EndEvent += EndDashCooldown;
        dashDurationTimer = new CountdownTimer(dashDuration);
        dashDurationTimer.EndEvent += EndDashDuration;

        moveSpeed = initialMoveSpeed;
        SetMovementAxes();

        EnableMovement();
        Ref.Player.PlayerInput.InputRotate += SetRotateInput;
        mainCam.GetComponent<CameraController>().OnCameraRotate += SetMovementAxes;
    }

    public void EnableMovement() {
        rb.isKinematic = false;

        CanMove = true;
        CanDash = true;

        // Prevent duplicate subs
        Ref.Player.PlayerInput.InputMove -= SetMoveInput;
        Ref.Player.PlayerInput.InputDash -= Dash;

        Ref.Player.PlayerInput.InputMove += SetMoveInput;
        Ref.Player.PlayerInput.InputDash += Dash;
    }
    public void DisableMovement() {
        rb.isKinematic = true;

        CanMove = false;
        CanDash = true;

        Ref.Player.PlayerInput.InputMove -= SetMoveInput;
        Ref.Player.PlayerInput.InputDash -= Dash;
    }

    void FixedUpdate() {
        if (!CanMove) return;
        if (moveInput.sqrMagnitude != 0) {
            // Translation
            Vector3 moveDir = forward * moveInput.y + right * moveInput.x;
            rb.AddForce(moveDir * moveSpeed * 1000 * Time.fixedDeltaTime);
            
            OnMovement?.Invoke();

            // Rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    void Dash() {
        if (!UpgradeManager.Flags.Dash) { return; }

        if (!CanDash) return;

        if ((!dashCooldownTimer.IsTicking || (UpgradeManager.Flags.DoubleDash && hasSecondDash)) && moveInput.sqrMagnitude != 0f) {
            moveSpeed = dashSpeed;
            dashPs.Play();
            
            if (dashCooldownTimer.IsTicking && hasSecondDash) { // this time is the second dash
                hasSecondDash = false;
                dashDurationTimer.Reset();
                dashDurationTimer.Start();
                return;
            }

            dashDurationTimer.Start();
            dashCooldownTimer.Start();
        }
    }
    void EndDashCooldown() {
        hasSecondDash = true;
    }
    void EndDashDuration() {
        moveSpeed = initialMoveSpeed;
        dashPs.Stop();
    }

    void SetMoveInput(MoveInputArgs moveInputArgs) { moveInput = moveInputArgs.MoveInput; }
    void SetRotateInput(float val) { rotateInput = val; }

    void SetMovementAxes() {
        forward = mainCam.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        right = mainCam.transform.right;
        right.y = 0f;
        right.Normalize();
    }
}