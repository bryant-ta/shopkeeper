using System;
using UnityEngine;

public class DebugPlayerArms : MonoBehaviour {
    // TEMP: until replace placeholder char arms
    [SerializeField] GameObject armsObj;
    Grid holdGrid;

    void Awake() {
        if (!GameManager.Instance.DebugMode) this.enabled = false;
        holdGrid = Ref.Player.PlayerInteract.HoldGrid;
    }

    void Update() { armsObj.SetActive(!holdGrid.IsEmpty()); }
}
