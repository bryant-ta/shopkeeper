using System;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDrag : MonoBehaviour {
    [SerializeField] float dragHoverHeight;

    Rigidbody heldObjRb;
    Collider heldObjCol;

    Player player;

    void Awake() {
        player = GetComponent<Player>();
    }

    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Interact);
        Events.Sub(gameObject, EventID.PrimaryUp, ReleaseHeld);
        Events.Sub<Vector3>(gameObject, EventID.Point, DragHeld);
    }

    void Interact(ClickInputArgs clickInputArgs) {
        if (!player.IsInRange(clickInputArgs.TargetObj.transform.position)) return;
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
            DragHeld(heldObjRb.transform.position);
        }
    }

    void DragHeld(Vector3 hitPoint) {
        if (heldObjRb == null) return; // Drag is constantly called from Point input, so only Drag if holding an object
        if (heldObjCol == null) { Debug.LogError("Held object with Rigidbody requires a collider."); }

        // If held out of interactable range, use closest point in range adjusted for hit point height
        Vector3 hoverPoint = hitPoint;
        if (!player.IsInRange(hitPoint)) {
            Vector3 dir = hitPoint - transform.position;
            hoverPoint = transform.position + Vector3.ClampMagnitude(dir, player.InteractionRange);
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
    void ReleaseHeld() {
        if (heldObjRb == null) return;
        if (heldObjCol == null) { Debug.LogError("Held object with Rigidbody requires a collider."); }
        
        heldObjRb.isKinematic = false;
        heldObjCol.enabled = true;
        heldObjRb = null;
    }
}
