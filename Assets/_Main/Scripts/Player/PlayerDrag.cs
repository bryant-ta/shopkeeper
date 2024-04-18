using System.Collections.Generic;
using DG.Tweening;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(PlayerInteract))]
public class PlayerDrag : MonoBehaviour {
    [field: SerializeField] public Grid DragGrid { get; private set; }

    [SerializeField] Transform rotationPivot;
    Vector3 pivotTargetRotation;

    Vector3Int selectedCellCoord;
    Vector3Int selectedShapeCellOffset; // local offset from clicked shape's root coord
    Grid targetGrid;

    // TEMP: Particles
    [SerializeField] ParticleSystem releaseDraggedPs;

    void Awake() {
        pivotTargetRotation = rotationPivot.rotation.eulerAngles;
        
        Ref.Player.PlayerInput.InputPrimaryDown += GrabRelease;
        Ref.Player.PlayerInput.InputPrimaryUp += Release;
        Ref.Player.PlayerInput.InputPoint += Drag;
        Ref.Player.PlayerInput.InputRotate += Rotate;
    }

    bool isHolding = false;
    void GrabRelease(ClickInputArgs clickInputArgs) {
        if (isHolding) {
            Release(clickInputArgs);
        } else {
            Grab(clickInputArgs);
        }
    }

    void Grab(ClickInputArgs clickInputArgs) {
        if (!DragGrid.IsEmpty()) return;

        GameObject clickedObj = clickInputArgs.TargetObj;
        IGridShape clickedShape = clickedObj.GetComponent<IGridShape>();
        if (clickedShape == null) return;

        // TODO: add tag or something for objects that should be moveable

        // Try to pick up stack of shapes
        Grid targetGrid = clickedShape.Grid;
        List<IGridShape> heldShapes = targetGrid.SelectStackedShapes(clickedShape.RootCoord, out IGridShape outOfFootprintShape);
        if (heldShapes == null) {
            TweenManager.Shake(outOfFootprintShape);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }

        if (heldShapes.Count == 0) { // keep separate from null check for debugging
            Debug.LogError("Clicked shape not registered in targetGrid. (Did you forget to initialize it with its grid?)");
            return;
        }

        // formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal =
            targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedShapeCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitAntiNormal + new Vector3(0.5f, 0, 0.5f));

        selectedShapeCellOffset = selectedShapeCellCoord - clickedShape.RootCoord;

        // Move dragGrid to shape before shape becomes child of grid - prevents movement anim choppyness
        DragGrid.transform.position = clickedShape.ShapeTransform.position;

        if (!targetGrid.MoveShapes(DragGrid, Vector3Int.zero, heldShapes)) {
            TweenManager.Shake(heldShapes);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            heldShapes.Clear();
            return;
        }

        foreach (IGridShape shape in heldShapes) {
            shape.Collider.enabled = false;
        }

        isHolding = true;

        SoundManager.Instance.PlaySound(SoundID.ProductPickUp);
    }

    void Update() {
        rotationPivot.transform.position = DragGrid.transform.position + selectedShapeCellOffset;
    }

    Vector3Int lastSelectedCellCoord;
    void Drag(ClickInputArgs clickInputArgs) {
        if (DragGrid.IsEmpty()) return;
        if (!SelectTargetGrid(clickInputArgs)) {
            return;
        }

        // formula for selecting cell adjacent to clicked face normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitNormal = targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(clickInputArgs.HitNormal, 0.1f));
        selectedCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitNormal + new Vector3(0.5f, 0, 0.5f));

        // Get lowest open grid cell
        if (targetGrid.SelectLowestOpenFromCell(selectedCellCoord, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        } else {
            // TODO: some feedback that this point is occupied/out of bounds
            return;
        }

        if (selectedCellCoord != lastSelectedCellCoord) {
            lastSelectedCellCoord = selectedCellCoord;

            // No drag movement if selected cell would make drag shapes overlap with existing
            // if (!targetGrid.ValidateShapesPlacement(selectedCellCoord - selectedShapeCellOffset, DragGrid.AllShapes())) {
            //     return;
            // }

            // Do drag movement
            Vector3 worldPos = targetGrid.transform.TransformPoint(selectedCellCoord); // cell coord to world position
            worldPos -= selectedShapeCellOffset; // aligns drag grid with clicked shape cell, to drag from point of clicking
            string tweenID = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
            DOTween.Kill(tweenID);
            DragGrid.transform.DOMove(worldPos, TweenManager.DragMoveDur).SetId(tweenID).SetEase(Ease.OutQuad);
            // DragGrid.transform.DORotateQuaternion(targetGrid.transform.rotation, 0.15f).SetEase(Ease.OutQuad);
        }
    }

    bool isRotating = false;
    void Rotate(bool clockwise) {
        if (DragGrid.IsEmpty()) return;
        if (isRotating) return;

        isRotating = true;
        List<IGridShape> dragShapes = DragGrid.AllShapes();

        // Update offset of drag grid so that selected shape cell stays under cursor
        selectedShapeCellOffset = new Vector3Int(selectedShapeCellOffset.z, selectedShapeCellOffset.y, -selectedShapeCellOffset.x);

        /*
         * Order of Operations for the *Illusion* of physical rotation while doing logical rotation a different way:
         *   parent shapes to rotation pivot → shift drag grid → do physical rotation around pivot → parent to drag grid →
         *   do logical rotation around root coord + new placement
         *   (which will do a physical move with no effect bc shape will already be in correct position)
         *
         * The simple non-illusion way (no tweening) just requires logical rotation + instant physical rotation -> instant physical shift
         */
        foreach (IGridShape shape in dragShapes) {
            shape.ShapeTransform.SetParent(rotationPivot);
        }
        
        // Do instant drag grid shift (needs to be here to prevent occasional missed drag grid shift)
        string tweenIDd = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
        DOTween.Kill(tweenIDd);
        Vector3 worldPos = targetGrid.transform.TransformPoint(selectedCellCoord);
        worldPos -= selectedShapeCellOffset; // aligns drag grid with new pos of clicked shape cell
        DragGrid.transform.position = worldPos;
        
        rotationPivot.rotation = Quaternion.Euler(pivotTargetRotation);
        pivotTargetRotation = rotationPivot.rotation.eulerAngles;
        pivotTargetRotation.y += 90f;
        
        string tweenID = rotationPivot.transform.GetInstanceID() + TweenManager.DragRotateID;
        DOTween.Kill(tweenID);
        rotationPivot.transform.DORotate(pivotTargetRotation, TweenManager.DragRotateDur).SetId(tweenID).SetEase(Ease.OutQuad)
            .OnComplete(
                () => {
                    foreach (IGridShape shape in dragShapes) {
                        shape.ShapeTransform.SetParent(DragGrid.transform);
                    }

                    DragGrid.RotateShapes(dragShapes, clockwise);
                    isRotating = false;
                }
            );
    }

    void Release(ClickInputArgs clickInputArgs) {
        if (DragGrid.IsEmpty()) return;
        if (!SelectTargetGrid(clickInputArgs)) {
            return;
        }

        List<IGridShape> heldShapes = DragGrid.SelectStackedShapes(Vector3Int.zero, out IGridShape shapeOutOfFootprint);

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

        isHolding = false;

        // TEMP: play shape placement smoke burst particles
        ParticleSystem.Burst burst = releaseDraggedPs.emission.GetBurst(0);
        burst.count = heldShapes.Count * 2 + 3;
        releaseDraggedPs.emission.SetBurst(0, burst);
        releaseDraggedPs.Play();
    }

    #region Helper

    // Select grid that is currently dragged over, caches last selected
    // Returns false if targetGrid is not set
    GameObject lastHitObj;
    bool SelectTargetGrid(ClickInputArgs clickInputArgs) {
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

    #endregion

    #region Upgrades

    public void ModifyMaxDragHeight(int delta) { DragGrid.SetMaxHeight(DragGrid.Height + delta); }

    #endregion
}