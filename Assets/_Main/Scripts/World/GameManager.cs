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
    [SerializeField] [Tooltip("Length of day in seconds")] float dayDuration;
    [SerializeField] [Tooltip("Time of day Open Phase starts in seconds")] float openPhaseTime;
    [SerializeField] [Tooltip("Time of day Close Phase starts in seconds")] float closePhaseTime;
    public StageTimer DayTimer { get; private set; }
    public StateMachine<DayPhase> SM_dayPhase { get; private set; }
    public DayPhase CurDayPhase => SM_dayPhase.CurState.ID;

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
        if (openPhaseTime > dayDuration || closePhaseTime > dayDuration) {
            Debug.LogError("A phase time is outside the duration of a day.");
        }
        
        List<float> dayPhaseIntervals = new();
        dayPhaseIntervals.Add(openPhaseTime);
        dayPhaseIntervals.Add(closePhaseTime);
        DayTimer = new StageTimer(dayDuration, dayPhaseIntervals);
        
        SM_dayPhase = new StateMachine<DayPhase>(new DeliveryDayPhaseState());
        DayTimer.TickEvent += SM_dayPhase.ExecuteNextState;
    }

    void Start() {
        if (DebugMode) { DebugTasks(); }
        
        ModifyGold(initialGold);
        
        MainLoop();
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
        // need to wait for all scripts' Start to finish
        Util.DoAfterOneFrame(this, () => DayTimer.Start());
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