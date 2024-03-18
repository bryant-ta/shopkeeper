using System;
using System.Collections.Generic;
using System.Linq;
using Timers;
using TriInspector;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    [Header("Debug")]
    public bool DebugMode;
    [SerializeField] bool useDebugDayClockTimes;
    [SerializeField] DebugDayClockTimes debugDayClockTimes;

    [Header("General")]
    [SerializeField, ReadOnly] bool isPaused;
    public bool IsPaused => isPaused;
    public event Action<bool> OnPause;

    [Header("World Grid")]
    [SerializeField] Grid worldGrid;
    public static Grid WorldGrid => _worldGrid;
    static Grid _worldGrid;

    [Header("Time")]
    [SerializeField] [Tooltip("Time on clock that day starts")]
    string dayStartClockTime;
    [SerializeField] [Tooltip("Time on clock that day ends")]
    string dayEndClockTime;
    [SerializeField] [Tooltip("Real-time duration until clock moves to next step (seconds)")]
    float dayClockTickDurationSeconds;
    [SerializeField] [Tooltip("Increment of time on clock that clock will move after tick duration (minutes)")]
    int dayclockTickStepMinutes;
    [SerializeField] [Tooltip("Time on clock Open Phase starts")]
    string deliveryPhaseClockTime;
    [SerializeField] [Tooltip("Time on clock Open Phase starts")]
    string openPhaseClockTime;
    [SerializeField] [Tooltip("Time on clock Close Phase starts")]
    string closePhaseClockTime;

    [field:SerializeField] public int Day { get; private set; }
    
    public ClockTimer DayTimer { get; private set; }
    public StateMachine<DayPhase> SM_dayPhase { get; private set; }
    public DayPhase CurDayPhase => SM_dayPhase.CurState.ID;
    
    List<string> phaseTriggerTimes = new();

    public event Action OnDayEnd;

    [Header("Gold")]
    [SerializeField] int initialGold;
    [SerializeField] int gold;
    public int Gold => gold;
    [SerializeField] int perfectOrdersGoldBonus;
    
    public event Action<DeltaArgs> OnModifyMoney;

    // Stocked Products
    public static Dictionary<ProductID, List<Product>> StockedProducts => stockedProducts;
    static Dictionary<ProductID, List<Product>> stockedProducts;

    void Awake() {
        if (DebugMode) AwakeDebugTasks();
        
        // Required to reset every Play mode start because static
        _worldGrid = worldGrid;
        stockedProducts = new();

        // Setup Day Cycle
        DayTimer = new ClockTimer(-1, dayStartClockTime, dayEndClockTime, dayClockTickDurationSeconds, dayclockTickStepMinutes);
        DayTimer.TickEvent += DayPhaseTrigger;
        
        SM_dayPhase = new StateMachine<DayPhase>(new DeliveryDayPhaseState());
        SM_dayPhase.OnStateExit += ExitStateTrigger;
        
        phaseTriggerTimes.Add(deliveryPhaseClockTime);
        phaseTriggerTimes.Add(openPhaseClockTime);
        phaseTriggerTimes.Add(closePhaseClockTime);
        phaseTriggerTimes.Add(dayEndClockTime);
    }

    void Start() {
        if (DebugMode) StartDebugTasks();
        
        // need to wait for all scripts' Start to finish before starting main loop
        Util.DoAfterOneFrame(this, () => MainLoop());
    }
    
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Close) DayEndTrigger();
    }

    void AwakeDebugTasks() {
        if (useDebugDayClockTimes) {
            dayStartClockTime = debugDayClockTimes.DayStartClockTime;
            dayEndClockTime = debugDayClockTimes.DayEndClockTime;
            dayClockTickDurationSeconds = debugDayClockTimes.DayClockTickDurationSeconds;
            dayclockTickStepMinutes = debugDayClockTimes.DayclockTickStepMinutes;
            deliveryPhaseClockTime = debugDayClockTimes.DeliveryPhaseClockTime;
            openPhaseClockTime = debugDayClockTimes.OpenPhaseClockTime;
            closePhaseClockTime = debugDayClockTimes.ClosePhaseClockTime;
        }
    }
    void StartDebugTasks() {
    }

    #region Main

    void MainLoop() {
        ModifyGold(initialGold);
        
        DayTimer.Start();
    }

    #endregion

    #region Time

    void DayPhaseTrigger(string clockTime) {
        // TODO: using clockTime mapped directly to phases
        for (int i = 0; i < phaseTriggerTimes.Count; i++) {
            if (Util.CompareTime(clockTime, phaseTriggerTimes[i]) == 0) {
                SM_dayPhase.ExecuteNextState();
                
                //TEMP: sound insert for now
                if (SM_dayPhase.CurState == null) continue;
                if (CurDayPhase == DayPhase.Open) {
                    SoundManager.Instance.PlaySound(SoundID.EnterOpenPhase);
                } else if (CurDayPhase == DayPhase.Close) {
                    SoundManager.Instance.PlaySound(SoundID.EnterClosePhase);
                } 
            }
        }
    }

    // End of day trigger/actions, next day has not started yet
    void DayEndTrigger() {
        if (Ref.Instance.OrderMngr.PerfectOrders) {
            ModifyGold(perfectOrdersGoldBonus);
        }
        
        OnDayEnd?.Invoke();
    }
    
    // actions for starting a new following day
    public void StartNextDay() {
        Day++;
        
        DayTimer.Reset();
        DayTimer.Start();
    }

    #endregion

    #region Control

    public void TogglePause() {
        isPaused = !isPaused;
        if (isPaused) {
            Time.timeScale = 0f;
            GlobalClock.SetTimeScale(0f);
            OnPause?.Invoke(true);
        }
        else {
            Time.timeScale = 1f;
            GlobalClock.SetTimeScale(1f);
            OnPause?.Invoke(false);
        }
    }

    public void SkipToPhaseEnd() {
        GlobalClock.SetTimeScale(10f);

        void ResetTimeScaleDelegate(IState<DayPhase> state) {
            GlobalClock.SetTimeScale(1f);
            SM_dayPhase.OnStateEnter -= ResetTimeScaleDelegate;
        }

        SM_dayPhase.OnStateEnter += ResetTimeScaleDelegate;
    }
    
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

    #region Stocked Products

    public static void AddStockedProduct(Product product) {
        if (stockedProducts.ContainsKey(product.ID)) {
            stockedProducts[product.ID].Add(product);
        }
        else {
            stockedProducts[product.ID] = new List<Product> {product};
        }
    }
    public static void RemoveStockedProduct(Product product) {
        if (stockedProducts.ContainsKey(product.ID)) {
            stockedProducts[product.ID].Remove(product);
        }

        if (stockedProducts[product.ID].Count == 0) {
            stockedProducts.Remove(product.ID);
        }
    }
    public static List<ProductID> GetStockedProductIDs() { return stockedProducts.Keys.ToList(); }
    public static Dictionary<ProductID, List<Product>> GetStockedProductsCopy() {
        Dictionary<ProductID, List<Product>> copy = new();

        // Deep copy
        foreach (KeyValuePair<ProductID, List<Product>> kvp in stockedProducts) {
            List<Product> newList = new List<Product>(kvp.Value);
            copy.Add(kvp.Key, newList);
        }

        return copy;
    }

    #endregion
}