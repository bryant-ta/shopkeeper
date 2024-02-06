using System;
using System.Collections.Generic;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDrag : MonoBehaviour {
    [SerializeField] float dragHoverHeight;

    [SerializeField] Grid dragGrid;
    List<IGridShape> heldShapes = new();

    Collider bottomObjCol;

    Player player;

    void Awake() {
        player = GetComponent<Player>();
    }

    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Grab);
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryUp, Release);
        Events.Sub<Vector3>(gameObject, EventID.Point, Drag);
    }

    void Grab(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count > 0) return;
        
        GameObject clickedObj = clickInputArgs.TargetObj;
        if (!player.IsInRange(clickedObj.transform.position)) return;
        IGridShape clickedShape = clickedObj.GetComponent<IGridShape>();
        if (clickedShape == null) return;

        // TODO: add tag or something for objects that should be moveable

        // Try to pick up stack of shapes
        Grid targetGrid = clickedShape.Grid;
        heldShapes = targetGrid.SelectStackedShapes(clickedShape.RootCoord);
        if (heldShapes.Count == 0) {
            Debug.LogError("Clicked shape not registered in targetGrid. (Did you forget to initialize it with its grid?)");
            return;
        }

        if (!targetGrid.MoveShapes(dragGrid, Vector3Int.zero, heldShapes)) {
            Debug.LogFormat("Not enough space in target grid ({0}) to move shapes.", targetGrid.gameObject.name); // TEMP
            heldShapes.Clear();
            return;
        }

        bottomObjCol = clickedObj.GetComponent<Collider>();
        foreach (IGridShape shape in heldShapes) {
            shape.ColliderTransform.GetComponent<Collider>().enabled = false;
        }

        Drag(clickInputArgs.HitPoint); // One Drag to update held obj position on initial click
    }

    void Drag(Vector3 hitPoint) {
        if (heldShapes.Count == 0) return;
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

        dragGrid.transform.position = new Vector3(hoverPoint.x, yOffset, hoverPoint.z);
    }

    void Release(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count == 0) return;

        // Find selected grid cell
        // TEMP: prob replace with tilemap
        // formula for selecting cell adjacent to clicked face (when pivot is bottom center) (y ignored)
        Vector3 hitNormalClamped = Vector3.ClampMagnitude(clickInputArgs.HitNormal, 0.1f);
        Vector3Int selectedCellCoord = Vector3Int.FloorToInt(clickInputArgs.HitPoint + hitNormalClamped + new Vector3(0.5f, 0, 0.5f));

        Grid targetGrid = GameManager.WorldGrid;
        if (targetGrid.SelectLowestOpen(selectedCellCoord.x, selectedCellCoord.z, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        } else {
            return;
        }

        // Try to place held stack of shapes
        if (!dragGrid.MoveShapes(targetGrid, selectedCellCoord, heldShapes)) {
            Debug.LogFormat("Not enough space in target grid ({0}) to move shapes.", targetGrid.gameObject.name); // TEMP
            return;
        }

        foreach (IGridShape shape in heldShapes) {
            shape.ColliderTransform.GetComponent<Collider>().enabled = true;
        }

        // Reset PlayerDrag
        heldShapes.Clear();
        bottomObjCol = null;
    }
}