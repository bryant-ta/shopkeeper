using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    public bool debug;

    public static Grid WorldGrid => worldGrid;
    static Grid worldGrid;
    [SerializeField] Grid _worldGrid;
    
    // TEMP
    [SerializeField] Transform deliveryZoneRootCoord;
    Zone deliveryZone;

    void Start() {
        if (debug) { DebugTasks(); }

        worldGrid = _worldGrid;
    }

    void DebugTasks() {
        // Initialize items with any IStackable in transform children
        // In game, stacks containing items should only be created after game start and inited on instantiation
        List<Stack> preMadeStacks = FindObjectsByType<Stack>(FindObjectsSortMode.None).ToList();
        for (int i = 0; i < preMadeStacks.Count; i++) {
            preMadeStacks[i].Init();
        }
        
        // Create zones
        ZoneProperties deliveryZoneProps = new ZoneProperties() {CanPlace = false, CanTake = true};
        deliveryZone = new Zone(Vector3Int.RoundToInt(deliveryZoneRootCoord.position), 5, 5, 5, deliveryZoneProps);
        worldGrid.AddZone(deliveryZone);
    }

    void Delivery() {
        // spawn boxes on xz coords
    }
}