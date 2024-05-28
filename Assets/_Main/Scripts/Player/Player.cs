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
    }

    void SelectDragTool() {
        curTool.Unequip();
        PlayerDrag.Equip();
    }
    void SelectSliceTool() {
        curTool.Unequip();
        PlayerSlice.Equip();
    }
    void SelectCompactTool() {
        curTool.Unequip();
        PlayerCompact.Equip();
    }
}

public interface IPlayerTool {
    public void Equip();
    public void Unequip();
}