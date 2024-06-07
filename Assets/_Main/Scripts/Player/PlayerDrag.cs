using System;
using System.Collections.Generic;
using DG.Tweening;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(PlayerInteract))]
public class PlayerDrag : MonoBehaviour, IPlayerTool {
    [SerializeField] float hoverHeight;
    [field: SerializeField] public Grid DragGrid { get; private set; }

    [SerializeField] Transform rotationPivot;
    Vector3 pivotTargetRotation;

    [Title("Selected Shape Outline")]
    [SerializeField] Color selectedOutlineColor;
    [SerializeField] Color selectedInvalidOutlineColor;
    [SerializeField] float selectedOutlineWidth;

    Vector3Int selectedCellCoord;
    Vector3Int selectedShapeCellOffset; // local offset from clicked shape's root coord
    Grid targetGrid;

    // TEMP: Particles
    [SerializeField] ParticleSystem releaseDraggedPs;

    public event Action<Vector3> OnGrab;
    public event Action<Vector3> OnDrag;
    public event Action OnRelease;

    void Awake() { pivotTargetRotation = rotationPivot.rotation.eulerAngles; }

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
        List<IGridShape> heldShapes = targetGrid.SelectStackedShapes(clickedShape.ShapeData.RootCoord, out IGridShape outOfFootprintShape);
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

        if (clickedShape.ShapeData.IsMultiY) {
            // NOTE: uses root as shortcut to lowest y level offset, assumes root is always on lowest y
            selectedShapeCellCoord = new Vector3Int(selectedShapeCellCoord.x, clickedShape.ShapeData.RootCoord.y, selectedShapeCellCoord.z);
        }

        selectedShapeCellOffset = selectedShapeCellCoord - clickedShape.ShapeData.RootCoord;

        // Move dragGrid to shape before shape becomes child of grid - prevents movement anim choppyness
        DragGrid.transform.position = clickedShape.ObjTransform.position;

        if (!targetGrid.MoveShapes(DragGrid, Vector3Int.zero, heldShapes)) {
            TweenManager.Shake(heldShapes);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            heldShapes.Clear();
            return;
        }

        for (int i = 0; i < heldShapes.Count; i++) {
            foreach (Collider col in heldShapes[i].Colliders) {
                col.enabled = false;
            }

            // Outline selected effect
            heldShapes[i].SetOutline(selectedOutlineColor, selectedOutlineWidth);
        }

        isHolding = true;

        // Cursor.visible = false;

        SoundManager.Instance.PlaySound(SoundID.ProductPickUp);

        OnGrab?.Invoke(clickInputArgs.HitPoint);
    }

    void Update() { rotationPivot.transform.position = DragGrid.transform.position + selectedShapeCellOffset; }

    Vector3Int lastSelectedCellCoord;
    void Drag(ClickInputArgs clickInputArgs) {
        if (DragGrid.IsEmpty()) return;
        targetGrid = Ref.Player.SelectTargetGrid(clickInputArgs);
        if (targetGrid == null) {
            List<IGridShape> heldShapes = DragGrid.AllShapes();
            if (clickInputArgs.TargetObj.TryGetComponent(out OrderBag bag)) { // Set valid outline when over an orderer bag
                for (int i = 0; i < heldShapes.Count; i++) {
                    heldShapes[i].SetOutline(selectedOutlineColor, selectedOutlineWidth);
                }
            } else { // Set invalid outline
                for (int i = 0; i < heldShapes.Count; i++) {
                    heldShapes[i].SetOutline(selectedInvalidOutlineColor, selectedOutlineWidth);
                }
            }

            // Drag grid follows cursor directly
            string tweenID = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
            DOTween.Kill(tweenID);
            DragGrid.transform.DOMove(clickInputArgs.HitPoint + new Vector3(0, hoverHeight, 0), TweenManager.DragMoveDur).SetId(tweenID)
                .SetEase(Ease.OutQuad);

            OnDrag?.Invoke(clickInputArgs.HitPoint);
            return;
        }

        // Formula for selecting cell adjacent to clicked face normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitNormal = targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(clickInputArgs.HitNormal, 0.1f));
        selectedCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitNormal + new Vector3(0.5f, 0, 0.5f));

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
            heldShapes[i].SetOutline(selectedOutlineColor, selectedOutlineWidth); // Reset selected shape outline
            if (!validations.ValidationList[i].IsValid) {
                heldShapes[i].SetOutline(selectedInvalidOutlineColor, selectedOutlineWidth);
            }
        }

        // Do drag movement
        Vector3 worldPos = targetGrid.transform.TransformPoint(selectedCellCoord); // cell coord to world position
        worldPos -= selectedShapeCellOffset; // aligns drag grid with clicked shape cell, to drag from point of clicking
        string tweenID = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
        DOTween.Kill(tweenID);
        DragGrid.transform.DOMove(worldPos, TweenManager.DragMoveDur).SetId(tweenID).SetEase(Ease.OutQuad);
        // DragGrid.transform.DORotateQuaternion(targetGrid.transform.rotation, 0.15f).SetEase(Ease.OutQuad);
    }

    bool isRotating = false;
    void Rotate(bool clockwise) {
        if (DragGrid.IsEmpty() || targetGrid == null) return;
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
            shape.ObjTransform.SetParent(rotationPivot);
        }

        // Do instant drag grid shift (needs to be here to prevent occasional missed drag grid shift)
        string moveTweenID = DragGrid.transform.GetInstanceID() + TweenManager.DragMoveID;
        DOTween.Kill(moveTweenID);
        Vector3 worldPos = targetGrid.transform.TransformPoint(selectedCellCoord);
        worldPos -= selectedShapeCellOffset; // aligns drag grid with new pos of clicked shape cell
        DragGrid.transform.position = worldPos;

        rotationPivot.rotation = Quaternion.Euler(pivotTargetRotation);
        pivotTargetRotation = rotationPivot.rotation.eulerAngles;
        pivotTargetRotation.y += 90f;

        string rotateTweenID = rotationPivot.transform.GetInstanceID() + TweenManager.DragRotateID;
        DOTween.Kill(rotateTweenID);
        rotationPivot.transform.DORotate(pivotTargetRotation, TweenManager.DragRotateDur).SetId(rotateTweenID).SetEase(Ease.OutQuad)
            .OnComplete(
                () => {
                    foreach (IGridShape shape in dragShapes) {
                        shape.ObjTransform.SetParent(DragGrid.transform);
                    }

                    DragGrid.RotateShapes(dragShapes, clockwise);
                    MoveDragGrid(); // to validate new rotated shapes' position + raise if needed

                    isRotating = false;
                }
            );
    }

    void Release(ClickInputArgs clickInputArgs) {
        if (DragGrid.IsEmpty()) return;
        List<IGridShape> heldShapes = DragGrid.AllShapes();

        // Non-grid releases
        if (clickInputArgs.TargetObj.TryGetComponent(out OrderBag bag)) {
            if (ShapeTags.CheckTags(heldShapes, ShapeTagID.NoPlaceInOrder)) {
                TweenManager.Shake(heldShapes);
                SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
                return;
            }

            if (bag.orderer.TryFulfillOrder(heldShapes)) {
                isHolding = false;
            } else {
                TweenManager.Shake(heldShapes);
                SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            }

            return;
        } else if (clickInputArgs.TargetObj.TryGetComponent(out Trash trash)) {
            if (ShapeTags.CheckTags(heldShapes, ShapeTagID.NoPlaceInTrash)) {
                TweenManager.Shake(heldShapes);
                SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
                return;
            }
            
            trash.TrashShapes(heldShapes, DragGrid);
            isHolding = false;
            return;
        }

        targetGrid = Ref.Player.SelectTargetGrid(clickInputArgs);
        if (targetGrid == null) {
            return;
        }

        // Try to place held shapes
        Vector3Int localCoord = Vector3Int.RoundToInt(targetGrid.transform.InverseTransformPoint(DragGrid.transform.position));
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

        for (int i = 0; i < heldShapes.Count; i++) {
            foreach (Collider col in heldShapes[i].Colliders) {
                col.enabled = true;
            }

            heldShapes[i].ResetOutline();
        }

        isHolding = false;

        // Cursor.visible = true;

        // TEMP: play shape placement smoke burst particles
        ParticleSystem.Burst burst = releaseDraggedPs.emission.GetBurst(0);
        burst.count = heldShapes.Count * 2 + 3;
        releaseDraggedPs.emission.SetBurst(0, burst);
        releaseDraggedPs.Play();

        OnRelease?.Invoke();
    }

    #region Upgrades

    public void ModifyMaxDragHeight(int delta) { DragGrid.SetMaxHeight(DragGrid.Height + delta); }

    #endregion

    public void Equip() {
        Ref.Player.PlayerInput.InputPrimaryDown += GrabRelease;
        Ref.Player.PlayerInput.InputPrimaryUp += Release;
        Ref.Player.PlayerInput.InputPoint += Drag;
        Ref.Player.PlayerInput.InputRotate += Rotate;
    }
    public void Unequip() {
        Ref.Player.PlayerInput.InputPrimaryDown -= GrabRelease;
        Ref.Player.PlayerInput.InputPrimaryUp -= Release;
        Ref.Player.PlayerInput.InputPoint -= Drag;
        Ref.Player.PlayerInput.InputRotate -= Rotate;
    }
}