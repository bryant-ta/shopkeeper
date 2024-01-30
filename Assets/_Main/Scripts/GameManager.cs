using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public bool debug;
    
    void Start() {
        if (debug) { DebugTasks(); }
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
