using System.Collections.Generic;
using DG.Tweening;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(PlayerInteract))]
public class PlayerDrag : MonoBehaviour {
    [field:SerializeField] public Grid DragGrid { get; private set; }

    // TEMP: Particles
    [SerializeField] ParticleSystem releaseDraggedPs;

    List<IGridShape> heldShapes = new();
    Collider bottomObjCol;

    PlayerInteract playerInteract;

    void Awake() {
        playerInteract = GetComponent<PlayerInteract>();

        Ref.Player.PlayerInput.InputPrimaryDown += Grab;
        Ref.Player.PlayerInput.InputPrimaryUp += Release;
        Ref.Player.PlayerInput.InputPoint += Drag;
    }

    void Grab(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count > 0) return;

        GameObject clickedObj = clickInputArgs.TargetObj;
        if (!playerInteract.IsInRange(clickedObj.transform.position)) return;
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
        DragGrid.transform.position = clickedShape.ShapeTransform.position;

        if (!targetGrid.MoveShapes(DragGrid, Vector3Int.zero, heldShapes)) {
            TweenManager.Shake(heldShapes);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            heldShapes.Clear();
            return;
        }

        bottomObjCol = clickedObj.GetComponent<Collider>();
        foreach (IGridShape shape in heldShapes) {
            shape.Collider.enabled = false;
        }
        
        SoundManager.Instance.PlaySound(SoundID.ProductPickUp);

        Drag(clickInputArgs); // One Drag to update held obj position on initial click
    }

    Vector3 lastHitPoint;
    Vector3Int lastSelectedCellCoord;
    Grid targetGrid;
    void Drag(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count == 0) return;
        if (bottomObjCol == null) {
            Debug.LogError("Held object is missing a collider.");
            return;
        }

        // If held out of interactable range, use closest point in range with adjusted hover height
        Vector3 hitPoint = clickInputArgs.HitPoint;
        if (!playerInteract.IsInRange(hitPoint)) {
            if (hitPoint == lastHitPoint) return;
            lastHitPoint = hitPoint;

            Vector3 dir = hitPoint - transform.position;
            Vector3 rangeClampedPoint = transform.position + Vector3.ClampMagnitude(dir, playerInteract.InteractRange);

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

            DragGrid.transform.DOKill();
            DragGrid.transform.DOMove(rangeClampedPoint, TweenManager.DragSnapDur).SetEase(Ease.OutQuad);

            return;
        }

        if (!SelectGrid(clickInputArgs)) {
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
            
            DragGrid.transform.DOKill();
            DragGrid.transform.DOMove(worldPos, TweenManager.DragSnapDur).SetEase(Ease.OutQuad);
            DragGrid.transform.DORotateQuaternion(targetGrid.transform.rotation, 0.15f).SetEase(Ease.OutQuad);
        }
    }

    void Release(ClickInputArgs clickInputArgs) {
        if (heldShapes.Count == 0) return;
        // If releasing out of interactable range, fail to place.
        if (!playerInteract.IsInRange(clickInputArgs.HitPoint)) {
            TweenManager.Shake(heldShapes);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }

        if (!SelectGrid(clickInputArgs)) {
            return;
        }
        
        // Try to place held shapes
        Vector3Int localCoord = Vector3Int.RoundToInt(targetGrid.transform.InverseTransformPoint(DragGrid.transform.position));
        if (!DragGrid.MoveShapes(targetGrid, localCoord, heldShapes)) {
            bool outOfHeightBounds = false;
            for (int i = 0; i < heldShapes.Count; i++) {
                if (heldShapes[i].RootCoord.y + DragGrid.transform.position.y >= targetGrid.Height) {
                    outOfHeightBounds = true;
                    TweenManager.Shake(heldShapes[i]);
                    SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
                }
            }

            if (!outOfHeightBounds) {
                TweenManager.Shake(heldShapes);
                SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            }

            return;
        }

        foreach (IGridShape shape in heldShapes) {
            shape.Collider.enabled = true;
        }

        // TEMP: play shape placement smoke burst particles
        ParticleSystem.Burst burst = releaseDraggedPs.emission.GetBurst(0);
        burst.count = heldShapes.Count * 2 + 3;
        releaseDraggedPs.emission.SetBurst(0, burst);
        releaseDraggedPs.Play();
        
        SoundManager.Instance.PlaySound(SoundID.ProductPlace);

        // Reset PlayerDrag
        heldShapes.Clear();
        bottomObjCol = null;
    }
    
    // Select grid that is currently dragged over, caches last selected
    // Returns false if targetGrid is not set
    GameObject lastHitObj;
    bool SelectGrid(ClickInputArgs clickInputArgs) {
        if (clickInputArgs.TargetObj != lastHitObj) {
            lastHitObj = clickInputArgs.TargetObj;
            if (clickInputArgs.TargetObj.TryGetComponent(out GridFloorHelper gridFloor)) {
                targetGrid = gridFloor.Grid;
            } else if (clickInputArgs.TargetObj.TryGetComponent(out IGridShape shape)) {
                targetGrid = shape.Grid;
            }
        }

        return targetGrid != null;
    }

    #region Upgrades

    public void ModifyMaxDragHeight(int delta) { DragGrid.SetMaxHeight(DragGrid.Height + delta); }

    #endregion
}