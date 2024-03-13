using EventManager;
using UnityEngine;

public class CartMovement : MonoBehaviour {
    [Header("Basic Movement")]
    [SerializeField] float speed;
    [SerializeField] float turnSpeed;
    Vector2 moveInput;

    Rigidbody rb;

    void Awake() {
        rb = GetComponent<Rigidbody>();

        Events.Sub<MoveInputArgs>(Ref.Instance.Player.gameObject, EventID.Move, SetMoveInput);
    }

    void FixedUpdate() {
        // Forward and backward movement
        Vector3 moveDir = transform.forward * moveInput.y;
        rb.AddForce(moveDir * speed * 1000 * Time.fixedDeltaTime);

        // Turning
        Vector3 turnDir = transform.forward + transform.right * moveInput.x;
        Quaternion targetRotation = Quaternion.LookRotation(turnDir);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
    }
    
    void SetMoveInput(MoveInputArgs moveInputArgs) { moveInput = moveInputArgs.MoveInput; }
}