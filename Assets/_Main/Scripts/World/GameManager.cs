using System;
using System.Collections.Generic;
using System.Linq;
using EventManager;
using Timers;
using TriInspector;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    [Header("Debug")]
    public bool DebugMode;

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
    public ClockTimer DayTimer { get; private set; }
    public StateMachine<DayPhase> SM_dayPhase { get; private set; }
    public DayPhase CurDayPhase => SM_dayPhase.CurState.ID;

    public event Action OnDayEnd;

    [Header("Gold")]
    [SerializeField] int initialGold;
    [SerializeField] int gold;
    public int Gold => gold;
    public event Action<DeltaArgs> OnModifyMoney;

    // Stocked Products
    public static Dictionary<ProductID, List<Product>> StockedProducts => stockedProducts;
    static Dictionary<ProductID, List<Product>> stockedProducts;

    void Awake() {
        // Required to reset every Play mode start because static
        _worldGrid = worldGrid;
        stockedProducts = new();

        // Setup Day Cycle
        DayTimer = new ClockTimer(-1, dayStartClockTime, dayEndClockTime, dayClockTickDurationSeconds, dayclockTickStepMinutes);
        DayTimer.TickEvent += DayPhaseTrigger;
        
        SM_dayPhase = new StateMachine<DayPhase>(new DeliveryDayPhaseState());
        SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void Start() {
        if (DebugMode) { DebugTasks(); }

        ModifyGold(initialGold);

        MainLoop();
    }
    
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Close) EndDayTrigger();
    }

    void DebugTasks() {
        // Initialize items with any IStackable in transform children
        // In game, stacks containing items should only be created after game start and inited on instantiation
        List<Stack> preMadeStacks = FindObjectsByType<Stack>(FindObjectsSortMode.None).ToList();
        for (int i = 0; i < preMadeStacks.Count; i++) {
            preMadeStacks[i].Init();
        }
    }

    public void TogglePause() {
        isPaused = !isPaused;
        if (isPaused) {
            Time.timeScale = 0f;
            GlobalClock.TimeScale = 0f;
            OnPause?.Invoke(true);
        }
        else {
            Time.timeScale = 1f;
            GlobalClock.TimeScale = 1f;
            OnPause?.Invoke(false);
        }
    }

    #region Main

    void MainLoop() {
        // need to wait for all scripts' Start to finish before starting day timer
        Util.DoAfterOneFrame(this, () => DayTimer.Start());
    }

    #endregion

    #region Time

    void DayPhaseTrigger(string clockTime) {
        // TODO: using clockTime mapped directly to phases
        if (Util.CompareTime(clockTime, deliveryPhaseClockTime) == 0) {
            SM_dayPhase.ExecuteNextState();
        } else if (Util.CompareTime(clockTime, openPhaseClockTime) == 0) {
            SM_dayPhase.ExecuteNextState();
        } else if (Util.CompareTime(clockTime, closePhaseClockTime) == 0) {
            SM_dayPhase.ExecuteNextState();
        }
    }

    void EndDayTrigger() {
        OnDayEnd?.Invoke();
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