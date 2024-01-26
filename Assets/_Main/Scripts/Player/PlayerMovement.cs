using System;
using EventManager;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;

    Vector3 forward;
    Vector3 right;
    Vector2 moveInput;
    float rotateInput;

    Camera mainCam;
    Rigidbody rb;

    void Awake() {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();

        SetMovementAxes();
    }

    void Start() {
        Events.Sub<MoveInputArgs>(gameObject, EventID.Move, SetMoveInput);
        Events.Sub<float>(gameObject, EventID.Rotate, SetRotateInput);
    }

    void FixedUpdate() {
        // Translation
        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;
        rb.AddForce(moveDir * speed * 1000 * Time.fixedDeltaTime);
        
        // Rotation
        rb.AddTorque(transform.up * rotateInput * rotationSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    void SetMoveInput(MoveInputArgs moveInputArgs) {
        moveInput = moveInputArgs.MoveInput;
    }
    void SetRotateInput(float val) {
        rotateInput = val;
    }

    void SetMovementAxes() {
        forward  = mainCam.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        right  = mainCam.transform.right;
        right.y = 0f;
        right.Normalize();
    }
}