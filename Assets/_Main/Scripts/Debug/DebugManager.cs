using System;
using TriInspector;
using UnityEngine;

public class DebugManager : Singleton<DebugManager> {
    [SerializeField] bool debugMode;
    public static bool DebugMode { get; private set; }

    [Title("Flags")]
    [Tooltip("If true, uses current inspector values of Delivery/Order Manager, doesn't progress difficulty.")]
    public bool DoSetDifficulty;
    public bool DoLevelInitialize;
    public bool DoOrderPhaseImmediately;
    public float PauseTimerAfterSeconds;

    [Title("Values")]
    public bool UseValues;
    public int Day = 1;
    public float OrderPhaseDuration = 1;

    void Awake() {
        DebugMode = debugMode;

        if (DebugMode) {
            if (PauseTimerAfterSeconds > 0) {
                Util.DoAfterSeconds(this, PauseTimerAfterSeconds, () => GlobalClock.SetTimeScale(0f));
            }
            
            Application.targetFrameRate = 60;
        }
    }

    void Start() {
        Util.DoAfterOneFrame(this, AfterOneFrame);
    }

    void AfterOneFrame() {
        if (DebugMode) {
            if (DoOrderPhaseImmediately) {
                GameManager.Instance.NextPhase();
            }
        }
    }
}