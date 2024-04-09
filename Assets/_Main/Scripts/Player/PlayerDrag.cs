using System.Collections.Generic;
using DG.Tweening;
using EventManager;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(PlayerInteract))]
public class PlayerDrag : MonoBehaviour {
    [field:SerializeField] public Grid DragGrid { get; private set; }

    // TEMP: Particles
    [SerializeField] ParticleSystem releaseDraggedPs;

    PlayerInteract playerInteract;

    void Awake() {
        playerInteract = GetComponent<PlayerInteract>();

        Ref.Player.PlayerInput.InputPrimaryDown += Grab;
        Ref.Player.PlayerInput.InputPrimaryUp += Release;
        Ref.Player.PlayerInput.InputPoint += Drag;
    }

    void Grab(ClickInputArgs clickInputArgs) {
        if (!DragGrid.IsEmpty()) return;

        GameObject clickedObj = clickInputArgs.TargetObj;
        IGridShape clickedShape = clickedObj.GetComponent<IGridShape>();
        if (clickedShape == null) return;

        // TODO: add tag or something for objects that should be moveable

        // Try to pick up stack of shapes
        Grid targetGrid = clickedShape.Grid;
        List<IGridShape> heldShapes = targetGrid.SelectStackedShapes(clickedShape.RootCoord);
        if (heldShapes.Count == 0) {
            Debug.LogError("Clicked shape not registered in targetGrid. (Did you forget to initialize it with its grid?)");
            return;
        }
        
        // formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal = targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
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
        
        SoundManager.Instance.PlaySound(SoundID.ProductPickUp);
        Drag(clickInputArgs); // One Drag to update held obj position on initial click
    }

    Vector3 lastHitPoint;
    Vector3Int lastSelectedCellCoord;
    Vector3Int selectedShapeCellOffset; // local offset from clicked shape's root coord
    Grid targetGrid;
    void Drag(ClickInputArgs clickInputArgs) {
        if (DragGrid.IsEmpty()) return;
        if (!SelectGrid(clickInputArgs)) {
            return;
        }

        // formula for selecting cell adjacent to clicked face normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitNormal = targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitNormal + new Vector3(0.5f, 0, 0.5f));

        // Get lowest open grid cell
        if (targetGrid.SelectLowestOpen(selectedCellCoord.x, selectedCellCoord.z, out int lowestOpenY)) {
            selectedCellCoord.y = lowestOpenY;
        } else {
            // TODO: some feedback that this point is occupied/out of bounds
            return;
        }

        if (selectedCellCoord != lastSelectedCellCoord) {
            lastSelectedCellCoord = selectedCellCoord;
            
            // No drag movement if selected cell would make drag shapes overlap with existing shapes
            if (!targetGrid.ValidateShapesPlacement(selectedCellCoord - selectedShapeCellOffset, DragGrid.AllShapes())) {
                return;
            } 

            // Do drag movement
            Vector3 worldPos = targetGrid.transform.TransformPoint(selectedCellCoord); // cell coord to world position
            worldPos -= selectedShapeCellOffset; // aligns drag grid with clicked shape cell, to drag from point of clicking
            DragGrid.transform.DOKill();
            DragGrid.transform.DOMove(worldPos, TweenManager.DragSnapDur).SetEase(Ease.OutQuad);
            DragGrid.transform.DORotateQuaternion(targetGrid.transform.rotation, 0.15f).SetEase(Ease.OutQuad);
        }
    }

    void Release(ClickInputArgs clickInputArgs) {
        if (DragGrid.IsEmpty()) return;
        if (!SelectGrid(clickInputArgs)) {
            return;
        }
        
        List<IGridShape> heldShapes = DragGrid.SelectStackedShapes(Vector3Int.zero);

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
    }
    
    #region Helper
    
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
    
    #endregion

    #region Upgrades

    public void ModifyMaxDragHeight(int delta) { DragGrid.SetMaxHeight(DragGrid.Height + delta); }

    #endregion
}