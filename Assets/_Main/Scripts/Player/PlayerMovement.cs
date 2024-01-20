using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float Speed;
    public float RotationSpeed;
    
    Vector3 forward;
    Vector3 right;
    public Vector2 moveInput;
    Rigidbody rb;

    void Awake() {
        rb = GetComponent<Rigidbody>();

        SetMovementAxes();
    }

    void Start() {
        // EventManager.Subscribe(gameObject, );
    }

    void FixedUpdate() {
        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

        transform.Translate(moveDir * Speed * Time.deltaTime, Space.World);
        if (moveDir != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, RotationSpeed * Time.deltaTime * 100);
        }
    }

    void SetMovementAxes() {
        forward  = Camera.main.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        right  = Camera.main.transform.right;
        right.y = 0f;
        right.Normalize();
    }
}
