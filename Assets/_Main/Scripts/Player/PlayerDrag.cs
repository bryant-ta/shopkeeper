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
        
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Grab);
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryUp, Release);
        Events.Sub<ClickInputArgs>(gameObject, EventID.Point, Drag);
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
            Debug.LogFormat("Unable to move shapes to target grid ({0}).", dragGrid.gameObject.name); // TEMP
            heldShapes.Clear();
            return;
        }

        bottomObjCol = clickedObj.GetComponent<Collider>();
        foreach (IGridShape shape in heldShapes) {
            shape.Collider.enabled = false;
        }

        Drag(clickInputArgs); // One Drag to update held obj position on initial click
    }

    void Drag(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count == 0) return;
        if (bottomObjCol == null) {
            Debug.LogError("Held object is missing a collider.");
            return;
        }

        // If held out of interactable range, use closest point in range adjusted for hit point height
        Vector3 hitPoint = clickInputArgs.HitPoint;
        Vector3 rangeClampedPoint = hitPoint;
        if (!player.IsInRange(hitPoint)) {
            Vector3 dir = hitPoint - transform.position;
            rangeClampedPoint = transform.position + Vector3.ClampMagnitude(dir, player.InteractionRange);
        }
        
        // Calculate grid coord y from hit point + clamped point
        Vector3Int coord = Vector3Int.RoundToInt(new Vector3(rangeClampedPoint.x, hitPoint.y, rangeClampedPoint.z));

        // formula for selecting cell adjacent to clicked face (when pivot is bottom center) (y ignored)
        Vector3 hitNormalClamped = Vector3.ClampMagnitude(clickInputArgs.HitNormal, 0.1f);
        Vector3Int selectedCellCoord = Vector3Int.FloorToInt(hitPoint + hitNormalClamped + new Vector3(0.5f, 0, 0.5f));
        
        Grid targetGrid = GameManager.WorldGrid;
        if (targetGrid.SelectLowestOpen(selectedCellCoord.x, selectedCellCoord.z, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        } else {
            // TODO: some feedback that this point is occupied/out of bounds
            return;
        }
        

        dragGrid.transform.position = selectedCellCoord;
    }

    void Release(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count == 0) return;

        // If releasing out of interactable range, fail to place.
        if (!player.IsInRange(clickInputArgs.HitPoint)) {
            // TODO: some sort of feedback
            return;
        }

        // Try to place held stack of shapes
        Grid targetGrid = GameManager.WorldGrid; // TEMP: change when having vehicle grids/ other grids to drag into
        if (!dragGrid.MoveShapes(targetGrid, Vector3Int.RoundToInt(dragGrid.transform.position), heldShapes)) {
            Debug.LogFormat("Unable to move shapes to target grid ({0}).", targetGrid.gameObject.name); // TEMP
            return;
        }

        foreach (IGridShape shape in heldShapes) {
            shape.Collider.enabled = true;
        }

        // Reset PlayerDrag
        heldShapes.Clear();
        bottomObjCol = null;
    }
}