using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float Speed;
    public float RotationSpeed;
    
    Vector3 cameraForward;
    Vector3 cameraRight;
    public Vector2 moveInput;
    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        cameraForward  = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();
        cameraRight  = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();
    }

    void FixedUpdate() {
        Vector3 moveDir = cameraForward * moveInput.y + cameraRight * moveInput.x;

        transform.Translate(moveDir * Speed * Time.deltaTime, Space.World);
        if (moveDir != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, RotationSpeed * Time.deltaTime * 100);
        }
    }
}
