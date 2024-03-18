using System;
using UnityEngine;

public class DebugPlayerArms : MonoBehaviour {
    // TEMP: until replace placeholder char arms
    [SerializeField] GameObject armsObj;
    Grid holdGrid;

    void Awake() {
        holdGrid = Ref.Player.PlayerInteract.HoldGrid;
    }

    void Update() { armsObj.SetActive(!holdGrid.IsEmpty()); }
}
