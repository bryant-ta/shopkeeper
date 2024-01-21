using EventManager;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour {
    [SerializeField] float interactionRange;
    [SerializeField] float dragHoverHeight;
    
    Rigidbody heldObjRb;
    Collider heldObjCol;
    
    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Interact);
        Events.Sub(gameObject, EventID.PrimaryUp, Release);
        Events.Sub<Vector3>(gameObject, EventID.Point, Drag);
    }

    void Interact(ClickInputArgs clickInputArgs) {
        if (!IsInRange(clickInputArgs.TargetObj.transform.position)) return;

        heldObjRb = clickInputArgs.TargetObj.GetComponent<Rigidbody>();
        heldObjCol = heldObjRb.GetComponent<Collider>();
        if (clickInputArgs.TargetObj.TryGetComponent(out Rigidbody rb)) {
            heldObjRb = rb;
            heldObjCol = rb.GetComponent<Collider>();
        }
    }
    
    void Drag(Vector3 hitPoint) {
        if (heldObjRb == null) return; // Drag is constantly called from Point input, so only Drag if holding an object
        if (heldObjCol == null) { Debug.LogError("Held object with Rigidbody requires a collider."); }

        heldObjRb.isKinematic = true;
        heldObjCol.enabled = false;
        heldObjRb.MovePosition(hitPoint + Vector3.up * (heldObjCol.bounds.extents.y + dragHoverHeight));
    }
    void Release() {
        if (heldObjRb == null) return;
        if (heldObjCol == null) { Debug.LogError("Held object with Rigidbody requires a collider."); }
        
        heldObjRb.isKinematic = false;
        heldObjCol.enabled = true;
        heldObjRb = null;
    }
    
    bool IsInRange(Vector3 targetPos) { return (targetPos - transform.position).magnitude < interactionRange; }
}
