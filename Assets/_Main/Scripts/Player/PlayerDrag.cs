using System.Collections.Generic;
using DG.Tweening;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDrag : MonoBehaviour {
    [SerializeField] float dragHoverHeight;
    [SerializeField] Grid dragGrid;

    // TEMP: Particles
    [SerializeField] ParticleSystem releaseDraggedPs;

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
            TweenManager.Shake(heldShapes);
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
    GameObject lastHitObj;
    Grid targetGrid;
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
            dragGrid.transform.DOMove(rangeClampedPoint, TweenManager.DragSnapDur).SetEase(Ease.OutQuad);

            return;
        }

        // Select grid that is currently dragged over, caches last selected
        if (clickInputArgs.TargetObj != lastHitObj) {
            if (clickInputArgs.TargetObj.TryGetComponent(out GridPlaneHelper gridPlane)) {
                targetGrid = gridPlane.Grid;
                lastHitObj = clickInputArgs.TargetObj;
            }
        }
        if (targetGrid == null) {
            return;
        }

        // formula for selecting cell adjacent to clicked face (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(hitPoint);
        Vector3 localHitNormal = targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitNormal + new Vector3(0.5f, 0, 0.5f));

        // Get lowest open grid cell
        if (targetGrid.SelectLowestOpen(selectedCellCoord.x, selectedCellCoord.z, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        } else {
            // TODO: some feedback that this point is occupied/out of bounds
            return;
        }

        // Do drag movement
        if (selectedCellCoord != lastSelectedCellCoord) {
            lastSelectedCellCoord = selectedCellCoord;
            Vector3 worldPos = targetGrid.transform.TransformPoint(selectedCellCoord); // cell coord to world position
            
            dragGrid.transform.DOKill();
            dragGrid.transform.DOMove(worldPos, TweenManager.DragSnapDur).SetEase(Ease.OutQuad);
            dragGrid.transform.DORotateQuaternion(targetGrid.transform.rotation, 0.15f).SetEase(Ease.OutQuad);
        }
    }

    void Release(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count == 0) return;
        // If releasing out of interactable range, fail to place.
        if (!player.IsInRange(clickInputArgs.HitPoint)) {
            TweenManager.Shake(heldShapes);
            return;
        }

        // Select grid that is currently dragged over, caches last selected
        if (clickInputArgs.TargetObj != lastHitObj) {
            if (clickInputArgs.TargetObj.TryGetComponent(out GridPlaneHelper gridPlane)) {
                targetGrid = gridPlane.Grid;
                lastHitObj = clickInputArgs.TargetObj;
            }
        }
        if (targetGrid == null) {
            return;
        }
        
        // Try to place held shapes
        Vector3Int localCoord = Vector3Int.RoundToInt(targetGrid.transform.InverseTransformPoint(dragGrid.transform.position));
        if (!dragGrid.MoveShapes(targetGrid, localCoord, heldShapes)) {
            bool outOfHeightBounds = false;
            for (int i = 0; i < heldShapes.Count; i++) {
                if (heldShapes[i].RootCoord.y + dragGrid.transform.position.y >= targetGrid.Height) {
                    outOfHeightBounds = true;
                    TweenManager.Shake(heldShapes[i]);
                }
            }

            if (!outOfHeightBounds) {
                TweenManager.Shake(heldShapes);
            }

            return;
        }

        // TEMP: play shape placement smoke burst particles
        ParticleSystem.Burst burst = releaseDraggedPs.emission.GetBurst(0);
        burst.count = heldShapes.Count * 2 + 3;
        releaseDraggedPs.emission.SetBurst(0, burst);
        releaseDraggedPs.Play();

        foreach (IGridShape shape in heldShapes) {
            shape.Collider.enabled = true;
        }

        // Reset PlayerDrag
        heldShapes.Clear();
        bottomObjCol = null;
    }

    #region Upgrades

    public void ModifyMaxDragHeight(int delta) { dragGrid.SetMaxHeight(dragGrid.Height + delta); }

    #endregion
}