using EventManager;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;
    
    Vector3 forward;
    Vector3 right;
    Vector2 moveInput;

    Camera mainCam;
    Rigidbody rb;

    void Awake() {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();

        SetMovementAxes();
    }

    void Start() {
        Events.Sub<MoveInputArgs>(gameObject, EventID.Movement, SetMovementInput);
    }

    void FixedUpdate() {
        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

        transform.Translate(moveDir * speed * Time.deltaTime, Space.World);
        if (moveDir != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime * 100);
        }
    }

    void SetMovementInput(MoveInputArgs moveInputArgs) {
        moveInput = moveInputArgs.MoveInput;
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