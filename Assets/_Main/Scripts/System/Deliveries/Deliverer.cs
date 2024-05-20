using System;
using UnityEngine;

public class Deliverer : MonoBehaviour {
    public Grid Grid { get; private set; }
    [SerializeField] Zone deliveryZone;
    [SerializeField] Vector3 deliveryZoneRootCoord;

    void Awake() {
        Grid = gameObject.GetComponentInChildren<Grid>();
        
        Grid.OnShapeStackMove += OnShapeStackMove;
    }

    void Start() {
        deliveryZone.Setup(Vector3Int.RoundToInt(deliveryZoneRootCoord));
        Grid.AddZone(deliveryZone);
    }
    
    // TEMP: placeholder until doing anims/theme for basic delivery
    void OnShapeStackMove() {
        if (!Grid.IsEmpty()) return;
        Disable();
    }

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }
}