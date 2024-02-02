using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    public bool debug;

    public static Grid WorldGrid => worldGrid;
    static Grid worldGrid;

    void Start() {
        if (debug) { DebugTasks(); }

        worldGrid = new Grid(20, 5, 20);
    }

    void DebugTasks() {
        // Initialize items with any IStackable in transform children
        // In game, stacks containing items should only be created after game start and inited on instantiation
        List<Stack> preMadeStacks = FindObjectsByType<Stack>(FindObjectsSortMode.None).ToList();
        for (int i = 0; i < preMadeStacks.Count; i++) {
            preMadeStacks[i].Init();
        }
    }
}