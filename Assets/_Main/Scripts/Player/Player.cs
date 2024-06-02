using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour {
    [field: SerializeField] public PlayerInput PlayerInput { get; private set; }
    [field: SerializeField] public PlayerInteract PlayerInteract { get; private set; }

    [field: SerializeField] public PlayerDrag PlayerDrag { get; private set; }
    [field: SerializeField] public PlayerSlice PlayerSlice { get; private set; }
    [field: SerializeField] public PlayerCompact PlayerCompact { get; private set; }

    IPlayerTool curTool;

    void Awake() {
        PlayerInput.InputDragTool += SelectDragTool;
        PlayerInput.InputSliceTool += SelectSliceTool;
        PlayerInput.InputCompactTool += SelectCompactTool;
        
        // Default tool mode: Drag
        curTool = PlayerDrag;
        PlayerDrag.Equip();
    }

    void SelectDragTool() {
        curTool.Unequip();
        curTool = PlayerDrag;
        PlayerDrag.Equip();
    }
    void SelectSliceTool() {
        curTool.Unequip();
        curTool = PlayerSlice;
        PlayerSlice.Equip();
    }
    void SelectCompactTool() {
        curTool.Unequip();
        curTool = PlayerCompact;
        PlayerCompact.Equip();
    }

    #region Helper

    // Select grid that is currently dragged over, caches last selected
    // Returns false if targetGrid is not set
    GameObject lastHitObj;
    Grid lastTargetedGrid;
    public Grid SelectTargetGrid(ClickInputArgs clickInputArgs) {
        if (clickInputArgs.TargetObj != lastHitObj) {
            lastHitObj = clickInputArgs.TargetObj;
            if (clickInputArgs.TargetObj.TryGetComponent(out GridFloorHelper gridFloor)) {
                lastTargetedGrid = gridFloor.Grid;
            } else if (clickInputArgs.TargetObj.TryGetComponent(out IGridShape shape)) {
                lastTargetedGrid = shape.Grid;
            } else {
                lastTargetedGrid = null;
            }
        }

        return lastTargetedGrid;
    }

    #endregion
}

public interface IPlayerTool {
    public void Equip();
    public void Unequip();
}