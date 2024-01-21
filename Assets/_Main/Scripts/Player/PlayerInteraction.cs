using System.Numerics;
using EventManager;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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
        GameObject targetObj = clickInputArgs.TargetObj;

        if (targetObj.TryGetComponent(out IInteractable interactable)) {
            interactable.Interact();
        }
        
        // TODO: add tag or something for objects that should be moveable
        if (targetObj.TryGetComponent(out Rigidbody rb)) {
            heldObjRb = rb;
            heldObjCol = rb.GetComponent<Collider>();
            
            heldObjRb.isKinematic = true;
            heldObjCol.enabled = false;
            Drag(heldObjRb.transform.position);
        }
    }

    void Drag(Vector3 hitPoint) {
        if (heldObjRb == null) return; // Drag is constantly called from Point input, so only Drag if holding an object
        if (heldObjCol == null) { Debug.LogError("Held object with Rigidbody requires a collider."); }

        // If held out of interactable range, use closest point in range adjusted for hit point height
        Vector3 hoverPoint = hitPoint;
        if (!IsInRange(hitPoint)) {
            Vector3 dir = hitPoint - transform.position;
            hoverPoint = transform.position + Vector3.ClampMagnitude(dir, interactionRange);
        }

        // Calculate hoverPoint y from objects underneath held object's footprint + object's height + manual offset
        float yOffset = 0f;
        Vector3 castCenter = new Vector3(heldObjCol.transform.position.x, 50f, heldObjCol.transform.position.z);
        if (Physics.BoxCast(castCenter, heldObjRb.transform.localScale / 2f, Vector3.down, out RaycastHit hit, Quaternion.identity,
                100f, LayerMask.GetMask("Point"), QueryTriggerInteraction.Ignore)) {
            if (hit.collider) {
                yOffset = hit.point.y;
            }
        }
        yOffset += heldObjCol.bounds.extents.y + dragHoverHeight;

        heldObjRb.MovePosition(new Vector3(hoverPoint.x, yOffset, hoverPoint.z));
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
