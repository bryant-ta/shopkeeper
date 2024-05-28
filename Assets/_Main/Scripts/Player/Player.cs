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
}

public interface IPlayerTool {
    public void Equip();
    public void Unequip();
}