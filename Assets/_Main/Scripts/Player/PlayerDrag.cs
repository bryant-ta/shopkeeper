using System;
using System.Collections.Generic;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDrag : MonoBehaviour {
    [SerializeField] float dragHoverHeight;

    Collider bottomObjCol;
    Stack heldStack;

    Stack lastStack;        // heldStack's previous location/stack
    LastStackState lastStackState;

    Player player;

    void Awake() {
        player = GetComponent<Player>();
    }

    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Grab);
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryUp, Release);
        Events.Sub<Vector3>(gameObject, EventID.Point, Drag);
        Events.Sub(gameObject, EventID.Cancel, Cancel);
    }

    void Grab(ClickInputArgs clickInputArgs) {
        GameObject targetObj = clickInputArgs.TargetObj;
        if (!player.IsInRange(targetObj.transform.position)) return;
        IStackable s = targetObj.GetComponent<IStackable>();
        if (s == null) return;

        // TODO: add tag or something for objects that should be moveable
        
        // Save last stack state for drag canceling
        lastStack = s.GetStack();
        lastStackState.DestroyOnEmpty = lastStack.DestroyOnEmpty;
        lastStackState.ColliderEnabled = targetObj.GetComponent<Collider>().enabled;
        
        lastStack.DestroyOnEmpty = false;

        // Remove target obj from its stack
        bottomObjCol = targetObj.GetComponent<Collider>();
        heldStack = lastStack.Take(s);
        heldStack.ModifyStackProperties(stackable => {
            Transform t = stackable.GetTransform();
            t.GetComponent<Collider>().enabled = false;
        });
        
        Drag(clickInputArgs.HitPoint); // One Drag to update held obj position on initial click
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
    
    void Release(ClickInputArgs clickInputArgs) {
        if (heldStack == null) return;

        // Find selected grid cell
        // TEMP: prob replace with tilemap
        Vector3Int selectedCellCoord;
        if (clickInputArgs.TargetObj.CompareTag("PlayArea")) { // Hit floor
            selectedCellCoord = Vector3Int.RoundToInt(clickInputArgs.HitPoint);
        } else { // Hit object
            selectedCellCoord = Vector3Int.RoundToInt(clickInputArgs.TargetObj.transform.position);
        }

        Grid grid = GameManager.WorldGrid;
        if (grid.SelectLowestOpen(selectedCellCoord.x, selectedCellCoord.z, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        } else {
            return;
        }
        
        // Validate + Place/Move heldStack to grid cell
        List<IGridShape> gridShapes = heldStack.GetItemComponents<IGridShape>();
        foreach (IGridShape shape in gridShapes) {
            // TEMP: rootCoord won't work if obj pivot is not regular (fitting in stack like a box)
            if (!grid.ValidateShapePlacement(selectedCellCoord + Vector3Int.RoundToInt(shape.GetTransform().localPosition), shape)) {
                return;
            }
        }
        
        foreach (IGridShape shape in gridShapes) {
            // TEMP: rootCoord won't work if obj pivot is not regular (fitting in stack like a box)
            Vector3Int rootCoord = selectedCellCoord + Vector3Int.RoundToInt(shape.GetTransform().localPosition);
            grid.PlaceShape(rootCoord, shape);
            shape.GetTransform().position = rootCoord;
        }
        
        // Restore lastStack state + do cleanup, no longer need to cancel drag
        lastStack.DestroyOnEmpty = lastStackState.DestroyOnEmpty;
        heldStack.ModifyStackProperties(stackable => {
            Transform t = stackable.GetTransform();
            t.GetComponent<Collider>().enabled = true;
        });

        lastStack.TryDestroyStack();
        
        // Reset PlayerDrag
        bottomObjCol = null;
        lastStack = null;
        heldStack = null;
    }
    
    void Cancel() {
        if (heldStack == null) return;
        
        lastStack.DestroyOnEmpty = lastStackState.DestroyOnEmpty;
        heldStack.ModifyStackProperties(stackable => {
            Transform t = stackable.GetTransform();
            t.GetComponent<Collider>().enabled = lastStackState.ColliderEnabled;
        });

        heldStack.PlaceAll(lastStack);
        
        bottomObjCol = null;
        lastStack = null;
        heldStack = null;
    }
}

struct LastStackState {
    public bool DestroyOnEmpty;
    public bool ColliderEnabled;
}