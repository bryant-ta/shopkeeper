using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TriInspector;
using UnityEngine;

// TODO: turn this whole thing into state machine... one day
[RequireComponent(typeof(PlayerInteract))]
public class PlayerDrag : MonoBehaviour, IPlayerTool {
    [SerializeField] float hoverHeight;
    [field: SerializeField] public Grid DragGrid { get; private set; }

    [SerializeField] [Tooltip("in units of cells")]
    int multiSelectCapacity;

    [SerializeField] Transform dragPivot;
    Vector3 pivotTargetRotation;

    [Title("Selected Shape Outline")]
    [SerializeField] Color selectedOutlineColor;
    [SerializeField] Color selectedInvalidOutlineColor;

    Vector3Int selectedCellCoord;       // target grid coord in context of current drag state
    Vector3Int selectedShapeCellOffset; // local shape offset from clicked shape's root coord
    Grid targetGrid;

    // Cancel Drag
    Vector3Int previousShapePos; // last grid coord of shape (stack) before dragging
    Grid previousGrid;

    // TEMP: Particles
    [SerializeField] ParticleSystem releaseDraggedPs;

    public event Action<Vector3> OnGrab;
    public event Action<Vector3> OnDrag;
    public event Action OnRelease;

    void Awake() { pivotTargetRotation = dragPivot.rotation.eulerAngles; }

    bool isHolding;
    void GrabRelease(ClickInputArgs clickInputArgs) {
        if (!isHolding) {
            Grab(clickInputArgs);
        }
    }

    Vector2 grabCursorPos;
    void Grab(ClickInputArgs clickInputArgs) {
        if (!DragGrid.IsAllEmpty()) return;

        IGridShape clickedShape = clickInputArgs.TargetObj.GetComponent<IGridShape>();
        if (clickedShape == null) return;
        
        // Try to pick up stack of shapes
        targetGrid = clickedShape.Grid;
        List<IGridShape> stackedShapes = targetGrid.SelectStackedShapes(
            clickedShape.ShapeData.RootCoord, out IGridShape outOfFootprintShape
        );
        if (stackedShapes == null) {
            if (outOfFootprintShape == null) return; // handle case of clicking thru to bottom shape while top shape is still physically moving
            TweenManager.Shake(outOfFootprintShape);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }
        if (stackedShapes.Count == 0) { // keep separate from null check for debugging
            Debug.LogError("Clicked shape not registered in targetGrid. (Did you forget to initialize it with its grid?)");
            return;
        }

        // formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal =
            targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedShapeCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitAntiNormal + new Vector3(0.5f, 0, 0.5f));

        if (clickedShape.ShapeData.IsMultiY) {
            // NOTE: uses root as shortcut to lowest y level offset, assumes root is always on lowest y
            selectedShapeCellCoord = new Vector3Int(selectedShapeCellCoord.x, clickedShape.ShapeData.RootCoord.y, selectedShapeCellCoord.z);
        }

        selectedShapeCellOffset = selectedShapeCellCoord - clickedShape.ShapeData.RootCoord;

        previousShapePos = clickedShape.ShapeData.RootCoord;
        previousGrid = targetGrid;

        // Move dragGrid to shape before shape becomes child of grid - prevents movement anim choppyness
        DragGrid.transform.position = targetGrid.transform.position + clickedShape.ShapeData.RootCoord;

        if (!targetGrid.MoveShapes(DragGrid, Vector3Int.zero, stackedShapes)) {
            TweenManager.Shake(stackedShapes);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            stackedShapes.Clear();
            return;
        }

        for (int i = 0; i < stackedShapes.Count; i++) {
            foreach (Collider col in stackedShapes[i].Colliders) {
                col.enabled = false;
            }

            // Outline selected effect
            stackedShapes[i].SetOutline(selectedOutlineColor);
        }

        SetIsHolding(true);
        numRotations = 0;

        grabCursorPos = clickInputArgs.CursorPos;
        // Cursor.visible = false;

        // Do lift movement
        Vector3 worldPos = targetGrid.transform.TransformPoint(selectedShapeCellCoord - selectedShapeCellOffset);
        string tweenID = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
        DOTween.Kill(tweenID);
        DragGrid.transform.DOMove(worldPos + new Vector3(0, hoverHeight, 0), TweenManager.DragMoveDur).SetId(tweenID)
            .SetEase(Ease.OutQuad);

        SoundManager.Instance.PlaySound(SoundID.ProductPickUp);

        OnGrab?.Invoke(clickInputArgs.HitPoint);
    }

    void Update() { dragPivot.transform.position = DragGrid.transform.position + selectedShapeCellOffset; }

    Vector3Int lastSelectedCellCoord;
    bool isDragging;
    void Drag(ClickInputArgs clickInputArgs) {
        if (DragGrid.IsAllEmpty()) return;
        targetGrid = Ref.Player.SelectTargetGrid(clickInputArgs);

        if (targetGrid == null) {
            // Detect when cursor is not over grid but is over shape in a grid
            if (clickInputArgs.TargetObj.layer == Ref.Player.PlayerInput.PointLayer) {
                if (clickInputArgs.TargetObj.TryGetComponent(out IGridShape shape)) {
                    targetGrid = shape.Grid;
                }
            } else { // Cursor is off a grid
                List<IGridShape> heldShapes = DragGrid.AllShapes();

                // Set invalid outline
                for (int i = 0; i < heldShapes.Count; i++) {
                    heldShapes[i].SetOutline(selectedInvalidOutlineColor);
                }

                // Drag grid follows cursor directly
                string tweenID = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
                DOTween.Kill(tweenID);
                DragGrid.transform.DOMove(
                        clickInputArgs.HitPoint + new Vector3(0, hoverHeight, 0) - selectedShapeCellOffset, TweenManager.DragMoveDur
                    ).SetId(tweenID)
                    .SetEase(Ease.OutQuad);

                lastSelectedCellCoord.y = -1; // reset lastSelectedCellCoord

                OnDrag?.Invoke(clickInputArgs.HitPoint);
                return;
            }
        }

        // Formula for selecting cell adjacent to clicked face normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitNormal = targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(clickInputArgs.HitNormal, 0.1f));
        selectedCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitNormal + new Vector3(0.5f, 0, 0.5f));

        // Prevent shapes immediately jumping from raycast hitting cell behind shape, better dragging experience
        if ((clickInputArgs.CursorPos - grabCursorPos).sqrMagnitude < 100f) return;
        isDragging = true;

        // Get lowest open grid cell
        if (targetGrid.SelectLowestOpenFromCell(selectedCellCoord, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        }

        if (selectedCellCoord != lastSelectedCellCoord) {
            lastSelectedCellCoord = selectedCellCoord;
            MoveDragGrid();
        }

        OnDrag?.Invoke(clickInputArgs.HitPoint);
    }
    void MoveDragGrid() {
        // Tries to find a valid position above baseSelectedCellCoord to fit shapes in drag grid
        List<IGridShape> heldShapes = DragGrid.AllShapes();
        Grid.PlacementValidations validations = targetGrid.ValidateShapesPlacement(selectedCellCoord - selectedShapeCellOffset, heldShapes);

        // Raise selected cell coord until held shapes do not overlap anything
        bool hasOverlap = true;
        while (hasOverlap && selectedCellCoord.y <= targetGrid.MaxY) {
            hasOverlap = false;
            for (int i = 0; i < validations.ValidationList.Count; i++) {
                if (validations.ValidationList[i].HasFlag(Grid.PlacementInvalidFlag.Overlap)) {
                    selectedCellCoord.y++;
                    validations = targetGrid.ValidateShapesPlacement(selectedCellCoord - selectedShapeCellOffset, heldShapes);
                    hasOverlap = true;
                    break;
                }
            }
        }

        // Set outline for shapes that would have invalid placement
        for (int i = 0; i < validations.ValidationList.Count; i++) {
            heldShapes[i].SetOutline(selectedOutlineColor); // Reset selected shape outline
            if (!validations.ValidationList[i].IsValid) {
                heldShapes[i].SetOutline(selectedInvalidOutlineColor);
            }
        }

        // For orders, check grid cell color
        // Vector3Int localCoord = Vector3Int.RoundToInt(targetGrid.transform.InverseTransformPoint(DragGrid.transform.position));
        // if (targetGrid.CompareTag("Order")) {
        //     Orderer orderer = targetGrid.GetComponentInParent<Orderer>();
        //     if (!orderer.CheckOrderInput(heldShapes, selectedCellCoord - selectedShapeCellOffset, out List<IGridShape> invalidShapes)) {
        //         foreach (IGridShape invalidShape in invalidShapes) {
        //             foreach (IGridShape heldShape in heldShapes) {
        //                 if (invalidShape == heldShape) {
        //                     heldShape.SetOutline(selectedInvalidOutlineColor);
        //                     break;
        //                 }
        //             }
        //         }
        //     }
        // }

        // Do drag movement
        // aligned cell coord to world position
        Vector3 worldPos = targetGrid.transform.TransformPoint(selectedCellCoord - selectedShapeCellOffset);
        string tweenID = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
        DOTween.Kill(tweenID);
        DragGrid.transform.DOMove(worldPos + new Vector3(0, hoverHeight, 0), TweenManager.DragMoveDur).SetId(tweenID).SetEase(Ease.OutQuad);
    }

    bool isRotating;
    int numRotations;
    void Rotate(bool clockwise, bool tween = true) {
        if (DragGrid.IsAllEmpty()) return;
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
            shape.ObjTransform.SetParent(dragPivot);
        }

        // Do instant drag grid shift (needs to be here to prevent occasional missed drag grid shift)
        string moveTweenID = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
        DOTween.Kill(moveTweenID);
        Vector3 worldPos = targetGrid ?
            targetGrid.transform.TransformPoint(selectedCellCoord - selectedShapeCellOffset) :
            dragPivot.transform.position;
        DragGrid.transform.position = worldPos + new Vector3(0, hoverHeight, 0);

        dragPivot.rotation = Quaternion.Euler(pivotTargetRotation);
        pivotTargetRotation = dragPivot.rotation.eulerAngles;
        pivotTargetRotation.y += clockwise ? 90f : -90f;

        string rotateTweenID = dragPivot.transform.GetInstanceID() + TweenManager.DragRotateID;
        DOTween.Kill(rotateTweenID);

        if (tween) {
            dragPivot.transform.DORotate(pivotTargetRotation, TweenManager.DragRotateDur).SetId(rotateTweenID).SetEase(Ease.InOutQuad)
                .OnComplete(
                    () => {
                        foreach (IGridShape shape in dragShapes) {
                            shape.ObjTransform.SetParent(DragGrid.transform);
                        }

                        DragGrid.RotateShapes(dragShapes, clockwise);
                        if (targetGrid) MoveDragGrid(); // to validate new rotated shapes' position + raise if needed

                        numRotations += clockwise ? 1 : -1;
                        isRotating = false;
                    }
                );
        } else {
            dragPivot.transform.rotation = Quaternion.Euler(pivotTargetRotation);

            foreach (IGridShape shape in dragShapes) {
                shape.ObjTransform.SetParent(DragGrid.transform);
            }

            DragGrid.RotateShapes(dragShapes, clockwise);

            isRotating = false;
        }
    }
    void RotateByClick(ClickInputArgs clickInputArgs) { Rotate(true); }
    void DoRotate(bool clockwise) { Rotate(true); }

    void Release(ClickInputArgs clickInputArgs) {
        if (DragGrid.IsAllEmpty()) return;
        List<IGridShape> heldShapes = DragGrid.AllShapes();

        // Non-grid releases
        if (clickInputArgs.TargetObj.TryGetComponent(out Trash trash)) {
            if (ShapeTags.CheckTags(heldShapes, ShapeTagID.NoPlaceInTrash)) {
                TweenManager.Shake(heldShapes);
                SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
                return;
            }

            trash.TrashShapes(heldShapes, DragGrid);
            SetIsHolding(false);
            return;
        }

        targetGrid = Ref.Player.SelectTargetGrid(clickInputArgs);
        if (targetGrid == null) {
            return;
        }

        // Orderer release - check is valid placement
        Vector3Int localCoord = Vector3Int.RoundToInt(targetGrid.transform.InverseTransformPoint(DragGrid.transform.position));
        if (targetGrid.CompareTag("Order")) {
            Orderer orderer = targetGrid.GetComponentInParent<Orderer>();
            if (!orderer.OrderInputPrecheck(heldShapes, localCoord, out List<IGridShape> invalidShape)) {
                foreach (IGridShape shape in invalidShape) {
                    TweenManager.Shake(shape);
                }

                SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
                return;
            }
        }

        // Force finish tweens on held shapes before placement
        DOTween.Kill(DragGrid.transform, true);
        DOTween.Kill(dragPivot.transform, true);

        // Try to place held shapes
        if (!DragGrid.MoveShapes(targetGrid, localCoord, heldShapes)) {
            bool outOfHeightBounds = false;
            for (int i = 0; i < heldShapes.Count; i++) {
                // Shake only shapes that are out of height bounds
                if (heldShapes[i].ShapeData.RootCoord.y + DragGrid.transform.position.y >= targetGrid.Height) {
                    outOfHeightBounds = true;
                    TweenManager.Shake(heldShapes[i]);
                    SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
                }
            }

            // Shake whole stack (whole held stack is invalid)
            if (!outOfHeightBounds) {
                TweenManager.Shake(heldShapes);
                SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            }

            return;
        }

        // Trigger falling
        foreach (IGridShape shape in heldShapes) {
            if (shape.ShapeData.RootCoord.y == localCoord.y) {
                targetGrid.TriggerFallOnTarget(shape);
            }
        }

        ReleaseReset(heldShapes);

        // TEMP: play shape placement smoke burst particles
        ParticleSystem.Burst burst = releaseDraggedPs.emission.GetBurst(0);
        burst.count = heldShapes.Count * 2 + 3;
        releaseDraggedPs.emission.SetBurst(0, burst);
        releaseDraggedPs.Play();
    }
    void ReleaseReset(List<IGridShape> heldShapes) {
        for (int i = 0; i < heldShapes.Count; i++) {
            heldShapes[i].ResetOutline();
        }

        SetIsHolding(false);
        numRotations = 0;
        isDragging = false;
        lastMultiSelectShape = null;

        // Cursor.visible = true;

        OnRelease?.Invoke();
    }
    void Cancel() {
        if (DragGrid.IsAllEmpty()) return;
        List<IGridShape> heldShapes = DragGrid.AllShapes();

        // undo rotations during drag... like unwinding lol
        while (numRotations != 0) {
            if (numRotations < 0) {
                Rotate(true, false);
                numRotations++;
            } else {
                Rotate(false, false);
                numRotations--;
            }
        }

        if (!DragGrid.MoveShapes(previousGrid, previousShapePos, heldShapes)) {
            Debug.LogError("Unable to return dragged shapes to original position.");
            return;
        }

        ReleaseReset(heldShapes);
    }

    bool isMultiSelecting;
    Vector3Int grabbedMultiSelectCoord;
    void EnableMultiSelect(ClickInputArgs clickInputArgs) {
        if (isDragging) return;

        isMultiSelecting = true;
        DefaultMode(false);

        Ref.Player.PlayerInput.InputPoint += MultiSelect;
        Ref.Player.PlayerInput.InputPrimaryUp += DisableMultiSelect;

        // TEMP: until custom cursor, render multiselect cell outline
        // formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal =
            targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
        grabbedMultiSelectCoord = Vector3Int.FloorToInt(localHitPoint + localHitAntiNormal + new Vector3(0.5f, 0, 0.5f));
    }
    void DisableMultiSelect(ClickInputArgs clickInputArgs) {
        if (isDragging) return;

        isMultiSelecting = false;
        DefaultMode(true);

        Ref.Player.PlayerInput.InputPoint -= MultiSelect;
        Ref.Player.PlayerInput.InputPrimaryUp -= DisableMultiSelect;

        isDragging = true; // NOTE: needed to prevent reactivating multi select mode after first time before releasing again
    }
    IGridShape lastMultiSelectShape;
    void MultiSelect(ClickInputArgs clickInputArgs) {
        IGridShape hoveredShape = clickInputArgs.TargetObj.GetComponent<IGridShape>();
        if (hoveredShape == null || hoveredShape == lastMultiSelectShape ||
            hoveredShape.ShapeData.RootCoord.y != grabbedMultiSelectCoord.y)
            return;

        lastMultiSelectShape = hoveredShape;

        // Check is not multi Y shape and hoveredShape is same Y
        if (hoveredShape.ShapeData.IsMultiY || hoveredShape.ShapeData.RootCoord.y != Mathf.RoundToInt(DragGrid.transform.position.y)) {
            TweenManager.Shake(hoveredShape);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }

        // Check/Try to pick up stack of shapes
        List<IGridShape> stackedShapes = hoveredShape.Grid.SelectStackedShapes(
            hoveredShape.ShapeData.RootCoord, out IGridShape outOfFootprintShape
        );
        if (stackedShapes == null || stackedShapes.Count == 0) {
            TweenManager.Shake(outOfFootprintShape);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }

        // Check multiselect capacity
        if (multiSelectCapacity != -1) {
            int stackedShapesSize = 0;
            foreach (IGridShape shape in stackedShapes) {
                stackedShapesSize += shape.ShapeData.Size;
            }
            if (DragGrid.AllShapesSize() + stackedShapesSize > multiSelectCapacity) {
                TweenManager.Shake(stackedShapes);
                SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
                return;
            }
        }

        // Move shapes to drag grid, in place
        Vector3Int worldPos = Vector3Int.RoundToInt(targetGrid.transform.TransformPoint(hoveredShape.ShapeData.RootCoord));
        Vector3Int dragGridCoord = Vector3Int.RoundToInt(DragGrid.transform.InverseTransformPoint(worldPos));
        if (!targetGrid.MoveShapes(DragGrid, dragGridCoord, stackedShapes)) {
            TweenManager.Shake(stackedShapes);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            stackedShapes.Clear();
            return;
        }

        // Manage colliders and outlines
        for (int i = 0; i < stackedShapes.Count; i++) {
            foreach (Collider col in stackedShapes[i].Colliders) {
                col.enabled = false;
            }

            stackedShapes[i].SetOutline(selectedOutlineColor);
        }

        grabCursorPos = clickInputArgs.CursorPos;

        SoundManager.Instance.PlaySound(SoundID.ProductPickUp);
    }

    void SetIsHolding(bool enable) {
        if (enable) {
            isHolding = true;
            Ref.Player.PlayerInput.SetAction(Constants.ActionMapNameGlobal, "Pause", false);
            Ref.Player.PlayerInput.SetAction(Constants.ActionMapNamePlayer, "Cancel", true);
        } else {
            isHolding = false;
            Ref.Player.PlayerInput.SetAction(Constants.ActionMapNameGlobal, "Pause", true);
            Ref.Player.PlayerInput.SetAction(Constants.ActionMapNamePlayer, "Cancel", false);
        }
    }

    #region Upgrades

    public void ModifyMaxDragHeight(int delta) { DragGrid.SetMaxHeight(DragGrid.Height + delta); }

    #endregion

    public void Equip() {
        DefaultMode(true);
        Ref.Player.PlayerInput.InputPrimaryDownMod += EnableMultiSelect;

        Ref.Player.PlayerInput.SetAction(Constants.ActionMapNamePlayer, "Cancel", false);
    }
    public bool Unequip() {
        if (isMultiSelecting) return false;

        DefaultMode(false);
        Ref.Player.PlayerInput.InputPrimaryDownMod -= EnableMultiSelect;

        Cancel();
        Ref.Player.PlayerInput.SetAction(Constants.ActionMapNamePlayer, "Cancel", false);

        return true;
    }

    void DefaultMode(bool enable) {
        if (enable) {
            Ref.Player.PlayerInput.InputPrimaryDown += GrabRelease;
            Ref.Player.PlayerInput.InputPrimaryUp += Release;
            Ref.Player.PlayerInput.InputSecondaryDown += RotateByClick;
            Ref.Player.PlayerInput.InputPoint += Drag;
            Ref.Player.PlayerInput.InputRotate += DoRotate;
            Ref.Player.PlayerInput.InputCancel += Cancel;
        } else {
            Ref.Player.PlayerInput.InputPrimaryDown -= GrabRelease;
            Ref.Player.PlayerInput.InputPrimaryUp -= Release;
            Ref.Player.PlayerInput.InputSecondaryDown -= RotateByClick;
            Ref.Player.PlayerInput.InputPoint -= Drag;
            Ref.Player.PlayerInput.InputRotate -= DoRotate;
            Ref.Player.PlayerInput.InputCancel -= Cancel;
        }
    }
}