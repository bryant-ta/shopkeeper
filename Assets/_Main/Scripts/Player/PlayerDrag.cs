using System.Collections.Generic;
using DG.Tweening;
using EventManager;
using UnityEngine;
using UnityEngine.Serialization;

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

        // Move dragGrid to shape before shape becomes child of grid - prevents movement anim choppyness
        dragGrid.transform.position = clickedShape.ShapeTransform.position;

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

    Vector3 lastHitPoint;
    Vector3Int lastSelectedCellCoord;
    void Drag(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count == 0) return;
        if (bottomObjCol == null) {
            Debug.LogError("Held object is missing a collider.");
            return;
        }

        // If held out of interactable range, use closest point in range with adjusted hover height
        Vector3 hitPoint = clickInputArgs.HitPoint;
        if (!player.IsInRange(hitPoint)) {
            if (hitPoint == lastHitPoint) return;
            lastHitPoint = hitPoint;

            Vector3 dir = hitPoint - transform.position;
            Vector3 rangeClampedPoint = transform.position + Vector3.ClampMagnitude(dir, player.InteractionRange);

            // Calculate hoverPoint y from objects underneath held object's footprint + manual offset
            Vector3 castCenter = new Vector3(bottomObjCol.transform.position.x, 50f, bottomObjCol.transform.position.z); // some high point
            if (Physics.BoxCast(
                    castCenter, bottomObjCol.transform.localScale / 2f, Vector3.down, out RaycastHit hit, Quaternion.identity,
                    100f, LayerMask.GetMask("Point"), QueryTriggerInteraction.Ignore
                )) {
                if (hit.collider) {
                    rangeClampedPoint.y = hit.point.y;
                }
            }

            dragGrid.transform.DOKill();
            dragGrid.transform.DOMove(rangeClampedPoint, Constants.AnimDragSnapDur).SetEase(Ease.OutQuad);

            return;
        }

        // formula for selecting cell adjacent to clicked face (when pivot is bottom center) (y ignored)
        Vector3 hitNormal = Vector3.ClampMagnitude(clickInputArgs.HitNormal, 0.1f);
        Vector3Int selectedCellCoord = Vector3Int.FloorToInt(hitPoint + hitNormal + new Vector3(0.5f, 0, 0.5f));

        Grid targetGrid = GameManager.WorldGrid;
        if (targetGrid.SelectLowestOpen(selectedCellCoord.x, selectedCellCoord.z, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        }
        else {
            // TODO: some feedback that this point is occupied/out of bounds
            return;
        }

        if (selectedCellCoord != lastSelectedCellCoord) {
            lastSelectedCellCoord = selectedCellCoord;
            dragGrid.transform.DOKill();
            dragGrid.transform.DOMove(selectedCellCoord, Constants.AnimDragSnapDur).SetEase(Ease.OutQuad);
        }
    }

    Tweener invalidReleaseTween;
    void Release(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count == 0) return;

        // If releasing out of interactable range, fail to place.
        if (!player.IsInRange(clickInputArgs.HitPoint)) {
            invalidReleaseTween.Kill();
            invalidReleaseTween = dragGrid.transform.DOShakePosition(
                Constants.AnimInvalidShake.Duration,
                new Vector3(1, 0, 1) * Constants.AnimInvalidShake.Strength,
                Constants.AnimInvalidShake.Vibrato,
                Constants.AnimInvalidShake.Randomness
            );

            return;
        }

        // Try to place held shapes
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