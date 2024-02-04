using System;
using System.Collections.Generic;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDrag : MonoBehaviour {
    [SerializeField] float dragHoverHeight;

    Collider bottomObjCol;
    [SerializeField] Grid holdGrid;
    IGridShape heldShape; // TEMP

    Player player;

    void Awake() {
        holdGrid.Init(1, 1, 1); // TEMP
        player = GetComponent<Player>();
    }

    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Grab);
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryUp, Release);
        Events.Sub<Vector3>(gameObject, EventID.Point, Drag);
    }

    void Grab(ClickInputArgs clickInputArgs) {
        GameObject heldObj = clickInputArgs.TargetObj;
        if (!player.IsInRange(heldObj.transform.position)) return;
        heldShape = heldObj.GetComponent<IGridShape>();
        if (heldShape == null) return;

        if (!holdGrid.ValidateShapePlacement(Vector3Int.zero, heldShape)) {
            Debug.Log("Object too big to pick up!"); // TEMP
            return;
        }

        // TODO: add tag or something for objects that should be moveable

        // Remove target obj from world grid
        Vector3Int rootCoord = Vector3Int.RoundToInt(heldObj.transform.parent.localPosition);
        GameManager.WorldGrid.RemoveShape(rootCoord, heldShape);
        
        // Pick up target obj
        bottomObjCol = heldObj.GetComponent<Collider>();
        bottomObjCol.enabled = false;
        holdGrid.PlaceShape(Vector3Int.zero, heldShape); // placement should already be validated
        
        // heldStack.ModifyStackProperties(stackable => {
        //     Transform t = stackable.GetTransform();
        //     t.GetComponent<Collider>().enabled = false;
        // });
        
        Drag(clickInputArgs.HitPoint); // One Drag to update held obj position on initial click
    }

    void Drag(Vector3 hitPoint) {
        if (heldShape == null) return;
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
        
        holdGrid.transform.position = new Vector3(hoverPoint.x, yOffset, hoverPoint.z);
    }
    
    void Release(ClickInputArgs clickInputArgs) {
        if (heldShape == null) return;

        // Find selected grid cell
        // TEMP: prob replace with tilemap
        // formula for selecting cell adjacent to clicked face (when pivot is bottom center) (y ignored)
        Vector3Int selectedCellCoord = Vector3Int.FloorToInt(clickInputArgs.HitPoint + clickInputArgs.HitNormal + new Vector3(0.5f, 0, 0.5f));

        Grid targetGrid = GameManager.WorldGrid;
        if (targetGrid.SelectLowestOpen(selectedCellCoord.x, selectedCellCoord.z, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        } else {
            return;
        }
        
        // Validate + Place/Move held obj to grid cell
        heldShape = holdGrid.SelectPosition(Vector3Int.zero);
        targetGrid.ValidateShapePlacement(selectedCellCoord, heldShape);
        
        // List<IGridShape> gridShapes = heldStack.GetItemComponents<IGridShape>();
        // foreach (IGridShape shape in gridShapes) {
        //     // TEMP: rootCoord won't work if obj pivot is not regular (fitting in stack like a box)
        //     if (!targetGrid.ValidateShapePlacement(selectedCellCoord + Vector3Int.RoundToInt(shape.GetTransform().localPosition), shape)) {
        //         return;
        //     }
        // }

        holdGrid.RemoveShape(Vector3Int.zero, heldShape);
        targetGrid.PlaceShape(selectedCellCoord, heldShape);
        
        // foreach (IGridShape shape in gridShapes) {
        //     // TEMP: rootCoord won't work if obj pivot is not regular (fitting in stack like a box)
        //     Vector3Int rootCoord = selectedCellCoord + Vector3Int.RoundToInt(shape.GetTransform().localPosition);
        //     targetGrid.PlaceShape(rootCoord, shape);
        //     shape.GetTransform().position = rootCoord;
        // }

        bottomObjCol.enabled = true;

        // Reset PlayerDrag
        bottomObjCol = null;
        heldShape = null;
    }
}