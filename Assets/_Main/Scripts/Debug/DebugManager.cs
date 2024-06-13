using System;
using TriInspector;
using UnityEngine;

public class DebugManager : Singleton<DebugManager> {
    [SerializeField] bool debugMode;
    public static bool DebugMode { get; private set; }

    [Title("Flags")]
    [Tooltip("If true, uses current inspector values of Delivery/Order Manager, doesn't progress difficulty.")]
    public bool DoSetDifficulty;

    [Title("Values")]
    public int Day = 1;

    void Awake() {
        DebugMode = debugMode;
    }
}