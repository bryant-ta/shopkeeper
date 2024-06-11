using System;
using TriInspector;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    [Title("Debug")]
    public bool DebugMode;

    [Title("General")]
    [field: SerializeField] public AnimationCurve DifficultyCurve { get; private set; }

    public int Difficulty => Day;
    [SerializeField, ReadOnly] bool isPaused;

    public bool IsPaused => isPaused;
    public event Action<bool> OnPause;

    [Title("Time")]
    [field: SerializeField] public int Day { get; private set; }

    [field: SerializeField] public int TotalDays { get; private set; }

    [Tooltip("Duration of Orders Phase")]
    [field: SerializeField] public float OrderPhaseDuration { get; private set; }

    public StateMachine<DayPhase> SM_dayPhase { get; private set; }
    public DayPhase CurDayPhase => SM_dayPhase.CurState.ID;

    public event Action OnDayEnd;

    [Title("World Grid")]
    [SerializeField] Grid worldGrid;
    public static Grid WorldGrid => _worldGrid;
    static Grid _worldGrid;

    [SerializeField] int globalGridHeight;
    public int GlobalGridHeight => globalGridHeight;

    [Title("Gold")]
    [SerializeField] int initialGold;
    [SerializeField] int gold;
    public int Gold => gold;
    [SerializeField] int perfectOrdersGoldBonus;

    public event Action<DeltaArgs> OnModifyMoney;

    void Awake() {
        if (DebugMode) AwakeDebugTasks();

        _worldGrid = worldGrid; // Required to reset every Play mode start because static

        SM_dayPhase = new StateMachine<DayPhase>(new DeliveryDayPhaseState());
        SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void Start() {
        if (DebugMode) StartDebugTasks();

        // need to wait for all scripts' Start to finish before starting main loop
        Util.DoAfterOneFrame(this, () => MainLoop());
    }

    void ExitStateTrigger(IState<DayPhase> state) { HandleLastState(state); }

    void AwakeDebugTasks() {
        // Util.DoAfterSeconds(this, 30, () => { GlobalClock.SetTimeScale(0f); });
    }
    void StartDebugTasks() { }

    #region Main

    void MainLoop() {
        ModifyGold(initialGold);

        SM_dayPhase.ExecuteNextState();
    }

    #endregion

    #region Time

    void HandleLastState(IState<DayPhase> lastState) {
        if (lastState.ID == DayPhase.Delivery) {
            SoundManager.Instance.PlaySound(SoundID.EnterOrderPhase);
        } else if (lastState.ID == DayPhase.Order) {
            OnDayEnd?.Invoke();
        } else if (lastState.ID == DayPhase.Close) {
            // Start next day
            Day++;
            SoundManager.Instance.PlaySound(SoundID.EnterDeliveryPhase);
        }
    }

    #endregion

    #region Control

    public void TogglePause() {
        isPaused = !isPaused;
        if (isPaused) {
            Time.timeScale = 0f;
            GlobalClock.SetTimeScale(0f);
            OnPause?.Invoke(true);
        } else {
            Time.timeScale = 1f;
            GlobalClock.SetTimeScale(1f);
            OnPause?.Invoke(false);
        }
    }

    public void NextPhase() { SM_dayPhase.ExecuteNextState(); }

    // TODO: create func for skipping to Order Phase end, ideally with time skip like below
    // public void SkipToPhaseEnd() {
    //     GlobalClock.SetTimeScale(10f);
    //
    //     void ResetTimeScaleDelegate(IState<DayPhase> state) {
    //         GlobalClock.SetTimeScale(1f);
    //         SM_dayPhase.OnStateEnter -= ResetTimeScaleDelegate;
    //     }
    //
    //     SM_dayPhase.OnStateEnter += ResetTimeScaleDelegate;
    // }

    #endregion

    #region Gold

    /// <summary>
    /// Applies delta to current coin value. 
    /// </summary>
    /// <param name="delta">(+/-)</param>
    public bool ModifyGold(int delta) {
        int newGold = gold + delta;
        if (newGold < 0) {
            return false;
        }

        gold = newGold;
        OnModifyMoney?.Invoke(new DeltaArgs {NewValue = newGold, DeltaValue = delta});

        return true;
    }

    #endregion
}