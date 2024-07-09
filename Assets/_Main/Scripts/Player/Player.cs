using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour {
    [field: SerializeField] public PlayerInput PlayerInput { get; private set; }
    [field: SerializeField] public PlayerInteract PlayerInteract { get; private set; }

    [field: SerializeField] public PlayerDrag PlayerDrag { get; private set; }
    [field: SerializeField] public PlayerSlice PlayerSlice { get; private set; }
    [field: SerializeField] public PlayerCombine PlayerCombine { get; private set; }

    int curToolIndex;
    List<IPlayerTool> tools;
    IPlayerTool curTool => tools[curToolIndex];

    public event Action<int> OnToolSwitch;

    void Awake() {
        // PlayerInput.InputScroll += SwitchTool; // TEMP: until making scroll to switch tools an option
        PlayerInput.InputDragTool += SelectDragTool;
        PlayerInput.InputSliceToolDown += SelectSliceTool;
        PlayerInput.InputSliceToolUp += SelectDragTool;
        PlayerInput.InputCompactToolDown += SelectCombineTool;
        PlayerInput.InputCompactToolUp += SelectDragTool;
        
        tools = new List<IPlayerTool> {PlayerDrag, PlayerSlice, PlayerCombine};
        
        // Default tool mode: Drag
        curToolIndex = 0;
        if (PlayerDrag != null) {
            PlayerDrag.Equip();
        }
    }

    void SwitchTool(float scrollInput) {
        if (!curTool.Unequip()) return;
        
        if (scrollInput > 0) {
            curToolIndex = (curToolIndex - 1 + tools.Count) % tools.Count;
        } else if (scrollInput < 0) {
            curToolIndex = (curToolIndex + 1) % tools.Count;
        }
        
        curTool.Equip();
        OnToolSwitch?.Invoke(curToolIndex);
    }

    public void SelectDragTool() {
        if (!curTool.Unequip()) return;
        curToolIndex = 0;
        curTool.Equip();
        OnToolSwitch?.Invoke(curToolIndex);
    }
    public void SelectSliceTool() {
        if (!curTool.Unequip()) return;
        curToolIndex = 1;
        curTool.Equip();
        OnToolSwitch?.Invoke(curToolIndex);
    }
    public void SelectCombineTool() {
        if (!curTool.Unequip()) return;
        curToolIndex = 2;
        curTool.Equip();
        OnToolSwitch?.Invoke(curToolIndex);
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
    public bool Unequip();
}