using System;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDrag : MonoBehaviour {
    [SerializeField] float dragHoverHeight;

    Collider bottomObjCol;

    Stack heldStack;

    Player player;

    void Awake() {
        player = GetComponent<Player>();
    }

    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Grab);
        Events.Sub(gameObject, EventID.PrimaryUp, Release);
        Events.Sub<Vector3>(gameObject, EventID.Point, Drag);
    }

    void Grab(ClickInputArgs clickInputArgs) {
        GameObject targetObj = clickInputArgs.TargetObj;
        if (!player.IsInRange(targetObj.transform.position)) return;
        IStackable s = targetObj.GetComponent<IStackable>();
        if (s == null) return;

        // TODO: move to Player
        // if (targetObj.TryGetComponent(out IInteractable interactable)) {
        //     interactable.Interact();
        // }
        
        // Remove target obj from its stack
        heldStack = s.GetStack().Take(s);
        
        // TODO: add tag or something for objects that should be moveable
        if (targetObj.TryGetComponent(out Collider col)) {
            bottomObjCol = col;
            heldStack.ModifyStackProperties(stackable => {
                Transform t = stackable.GetTransform();
                t.GetComponent<Rigidbody>().isKinematic = true;
                t.GetComponent<Collider>().enabled = false;
            });
            
            Drag(heldStack.transform.position); // One Drag to update held obj position on initial click
        }
    }

    void Drag(Vector3 hitPoint) {
        if (heldStack == null) return;
        if (bottomObjCol == null) {
            Debug.LogError("Held object is missing a collider.");
            return;
        }

        // If held out of interactable range, use closest point in range adjusted for hit point height
        Vector3 hoverPoint = hitPoint;
        if (!player.IsInRange(hitPoint)) {
            Vector3 dir = hitPoint - transform.position;
            hoverPoint = transform.position + Vector3.ClampMagnitude(dir, player.InteractionRange);
        }

        // Calculate hoverPoint y from objects underneath held object's footprint + object's height + manual offset
        float yOffset = 0f;
        Vector3 castCenter = new Vector3(bottomObjCol.transform.position.x, 50f, bottomObjCol.transform.position.z); // some high point
        if (Physics.BoxCast(castCenter, bottomObjCol.transform.localScale / 2f, Vector3.down, out RaycastHit hit, Quaternion.identity,
                100f, LayerMask.GetMask("Point"), QueryTriggerInteraction.Ignore)) {
            if (hit.collider) {
                yOffset = hit.point.y;
            }
        }
        yOffset += dragHoverHeight;
        
        heldStack.transform.position = new Vector3(hoverPoint.x, yOffset, hoverPoint.z);
    }
    
    void Release() {
        if (heldStack == null) return;
        
        heldStack.ModifyStackProperties(stackable => {
            Transform t = stackable.GetTransform();
            t.GetComponent<Rigidbody>().isKinematic = false;
            t.GetComponent<Collider>().enabled = true;
        });

        bottomObjCol = null;
        heldStack = null;
    }
}
