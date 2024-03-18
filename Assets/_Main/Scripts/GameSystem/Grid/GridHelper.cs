using System;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class GridHelper : MonoBehaviour {
    Grid grid;
    PlayerDrag playerDrag;

    void Awake() {
        grid = GetComponent<Grid>();
        playerDrag = Ref.Player.PlayerDrag;
    }
    
    void OnTriggerEnter(Collider col) {
        if (col.TryGetComponent(out Zone zone)) {
            if (zone.transform.parent != null && zone.transform.parent.TryGetComponent(out OrderManager orderMngr)) {
                orderMngr.TryFillOrder(grid);
            }
        }
    }
}
