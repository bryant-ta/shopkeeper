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

    Collider[] heldObjOverlaps = new Collider[32]; // arbitrary limit (OverlapBox ignores number of cols after this number)
    void Drag(Vector3 hitPoint) {
        if (heldObjRb == null) return; // Drag is constantly called from Point input, so only Drag if holding an object
        if (heldObjCol == null) { Debug.LogError("Held object with Rigidbody requires a collider."); }

        // If held out of interactable range, use closest point in range adjusted for hit point height
        Vector3 hoverPoint = hitPoint;
        if (!IsInRange(hitPoint)) {
            Vector3 dir = hitPoint - transform.position;
            hoverPoint = transform.position + Vector3.ClampMagnitude(dir, interactionRange);

            Vector3 castCenter = heldObjCol.transform.position + Vector3.up * 50f;
            if (Physics.BoxCast(castCenter, heldObjCol.bounds.extents, Vector3.down, out RaycastHit hit, Quaternion.identity,
                    100f, LayerMask.GetMask("Point"), QueryTriggerInteraction.Ignore)) {
                if (hit.collider) {
                    hoverPoint += new Vector3(0, hit.collider.bounds.size.y, 0);
                }
            }
            
            // if (Physics.Raycast(heldObjRb.position, Vector3.down, out RaycastHit hit, 100f, 
            //         LayerMask.GetMask("Point"), QueryTriggerInteraction.Ignore)) {
            //     if (hit.collider) {
            //         hoverPoint += new Vector3(0, hit.collider.bounds.extents.y * 2, 0);
            //     }
            // }
            
            // this works but after moving to correct height, no longer overlaps so hoverPoint y offset is lost. flip flops every drag frame.
            // int numCols = Physics.OverlapBoxNonAlloc(heldObjCol.transform.position, heldObjCol.bounds.extents, heldObjOverlaps, Quaternion.identity,
            //     LayerMask.GetMask("Point"));
            // if (numCols > 0) {
            //     float greatestHeight = float.MinValue;
            //     for (int i = 0; i < numCols; i++) {
            //         print(heldObjOverlaps[i].gameObject.name);
            //         float colliderHeight = heldObjOverlaps[i].bounds.size.y;
            //         if (colliderHeight > greatestHeight) {
            //             greatestHeight = colliderHeight;
            //         }
            //     }
            //
            //     hoverPoint += new Vector3(0, greatestHeight, 0);
            // }
        } else {    // In interactable range
            hoverPoint += Vector3.up * (heldObjCol.bounds.extents.y + dragHoverHeight);
        }

        heldObjRb.MovePosition(hoverPoint);
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
