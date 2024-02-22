using System;
using System.Collections;
using System.Collections.Generic;
using EventManager;
using Timers;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Header("Basic Movement")]
    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;
    Vector3 forward;
    Vector3 right;
    Vector2 moveInput;
    float rotateInput;

    [Header("Dash")]
    [SerializeField] float dashForce;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCooldown;
    CountdownTimer dashTimer;

    Camera mainCam;
    Rigidbody rb;

    void Awake() {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();

        dashTimer = new CountdownTimer(dashCooldown);

        SetMovementAxes();

        Events.Sub<MoveInputArgs>(gameObject, EventID.Move, SetMoveInput);
        Events.Sub<float>(gameObject, EventID.Rotate, SetRotateInput);
        Events.Sub(gameObject, EventID.Dash, Dash);

        mainCam.GetComponent<CameraController>().OnCameraRotate += SetMovementAxes;
    }

    void FixedUpdate() {
        if (moveInput.sqrMagnitude != 0) {
            // Translation
            Vector3 moveDir = forward * moveInput.y + right * moveInput.x;
            rb.AddForce(moveDir * speed * 1000 * Time.fixedDeltaTime);

            // Rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    void Dash() {
        if (!dashTimer.IsTicking && moveInput.sqrMagnitude != 0f) {
            dashTimer.Start();
            StartCoroutine(DashForceSmooth());
        }
    }
    IEnumerator DashForceSmooth() {
        float t = 0f;
        while (t < dashDuration) {
            Vector3 moveDir = forward * moveInput.y + right * moveInput.x;
            rb.AddForce(moveDir * dashForce);
            t += Time.deltaTime * GlobalClock.TimeScale;
            
            yield return null;
        }
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