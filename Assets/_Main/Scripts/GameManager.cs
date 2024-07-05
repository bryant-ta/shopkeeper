using System;
using Timers;
using TriInspector;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    // [field: Title("General")]
    // [field: SerializeField] public AnimationCurve DifficultyCurve { get; private set; }

    public int Difficulty => Day;
    [SerializeField, ReadOnly] bool isPaused;

    public bool IsPaused => isPaused;
    public event Action<bool> OnPause;

    [field: Title("Time")]
    [field: SerializeField] public int Day { get; private set; }

    [field: SerializeField] public int TotalDays { get; private set; }

    public StateMachine<DayPhase> SM_dayPhase { get; private set; }
    public DayPhase CurDayPhase => SM_dayPhase.CurState.ID;

    [SerializeField] float runTimerInitDur;
    [SerializeField] float runTimerMaxDur;
    [SerializeField] float runTimerRecoverDur;
    public CountdownTimer RunTimer { get; private set; }

    public event Action<int> OnDayEnd;

    [field: Title("Systems")]
    [Tooltip("Sends only bulk delivery every X days.")]
    [field: SerializeField] public int BulkDayInterval { get; private set; }

    LevelInitializer levelInit;

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
        if (DebugManager.DebugMode) AwakeDebugTasks();

        _worldGrid = worldGrid; // Required to reset every Play mode start because static
        levelInit = GetComponentInChildren<LevelInitializer>();

        RunTimer = new CountdownTimer(runTimerMaxDur - 0.1f); // offset for showing time UI correctly
        RunTimer.EndEvent += Lose;

        SM_dayPhase = new StateMachine<DayPhase>(new DeliveryDayPhaseState());
        SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void Start() {
        if (DebugManager.DebugMode) StartDebugTasks();

        RunTimer.SetTime(runTimerInitDur - 0.1f);
        OnDayEnd?.Invoke(Day);

        // need to wait for all scripts' Start to finish before starting main loop
        Util.DoAfterOneFrame(this, () => MainLoop());
    }

    void ExitStateTrigger(IState<DayPhase> state) { HandleLastState(state); }

    void AwakeDebugTasks() {
        if (DebugManager.Instance.UseValues) {
            Day = DebugManager.Instance.Day;
            runTimerInitDur = DebugManager.Instance.OrderPhaseDuration;
        }
    }
    void StartDebugTasks() { }

    #region Main

    void MainLoop() {
        levelInit.InitializeLevel();
        ModifyGold(initialGold);

        SM_dayPhase.ExecuteNextState();
    }

    void Lose() { print("u lose"); }

    #endregion

    #region Time

    void HandleLastState(IState<DayPhase> lastState) {
        switch (lastState.ID) {
            case DayPhase.Delivery:
                RunTimer.Start();
                SoundManager.Instance.PlaySound(SoundID.EnterOrderPhase);
                break;
            case DayPhase.Order:
                RunTimer.Stop();
                break;
            case DayPhase.Close: { // Start next day
                float timeAdd = runTimerRecoverDur - 0.1f;
                if (RunTimer.RemainingTimeSeconds + timeAdd > runTimerMaxDur)
                    timeAdd = runTimerMaxDur - RunTimer.RemainingTimeSeconds;
                RunTimer.AddTime(timeAdd);

                Day++;
                OnDayEnd?.Invoke(Day);
                SoundManager.Instance.PlaySound(SoundID.EnterDeliveryPhase);
                break;
            }
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