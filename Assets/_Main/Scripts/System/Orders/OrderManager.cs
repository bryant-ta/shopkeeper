using System;
using System.Collections.Generic;
using System.Linq;
using Orders;
using Timers;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [SerializeField] int numNeedOrdersFulfilled;
    [SerializeField] int numOrdersFulfilled;
    public bool MetQuota => numOrdersFulfilled >= numNeedOrdersFulfilled;
    public event Action<int, int> OnOrderFulfilled; // <current number fulfilled, number orders needed for level>

    [Title("Order Queue")]
    [SerializeField] int numActiveDocks;
    [SerializeField] MinMax NextOrderDelay;

    [Title("Order Parameters")]
    [SerializeField] int baseOrderTime;
    [SerializeField] int timePerProduct;
    [SerializeField] int baseOrderValue;
    [SerializeField] int valuePerProduct;
    [SerializeField] int perfectOrdersBonus;
    [field: SerializeField] public CountdownTimer OrderPhaseTimer { get; private set; }

    [Title("Order Layouts")]
    [SerializeField] List<SO_OrderLayout> orderLayouts = new();
    [SerializeField] int layoutDifficulty;

    [Title("Orderers")]
    [SerializeField] Transform docksContainer;
    List<Dock> docks;
    [SerializeField] GameObject ordererObj;

    [field: Title("ReadOnly")]
    [field: SerializeField, ReadOnly] public bool PerfectOrders { get; private set; } // true if all orders for the day are fulfilled

    Util.ValueRef<bool> orderPhaseActive;

    void Awake() {
        LoadOrderLayouts();
        
        orderPhaseActive = new Util.ValueRef<bool>(false);
        OrderPhaseTimer = new CountdownTimer(GameManager.Instance.OrderPhaseDuration);
        docks = docksContainer.GetComponentsInChildren<Dock>().ToList();

        GameManager.Instance.SM_dayPhase.OnStateEnter += EnterStateTrigger;
        GameManager.Instance.SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            DifficultyManager.Instance.ApplyOrderDifficulty();
            orderPhaseActive.Value = true;
            StartOrders();
        }
    }
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            OrderPhaseTimer.Reset();
        }
    }

    void StartOrders() {
        numOrdersFulfilled = 0;
        OnOrderFulfilled?.Invoke(0, numNeedOrdersFulfilled);
        PerfectOrders = true;

        // Start sending Orderers
        AssignNextOrderer(docks[0]); // always immediately activate first order
        int activeDocks = Math.Min(numActiveDocks, docks.Count);
        for (var i = 1; i < activeDocks; i++) {
            AssignNextOrdererDelayed(docks[i], Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }

        // Start Order Phase timer
        OrderPhaseTimer.AddDuration(GameManager.Instance.OrderPhaseDurationGrowth);
        OrderPhaseTimer.Start();
        OrderPhaseTimer.EndEvent += StopOrders;
    }
    void StopOrders() {
        orderPhaseActive.Value = false; // Stops delayed orders and order generation chain
        TryTriggerOrderPhaseEnd();
    }

    void TryTriggerOrderPhaseEnd() {
        if (orderPhaseActive.Value || GameManager.Instance.CurDayPhase != DayPhase.Order) return;

        // Can end Order phase when all docks no longer have active orderers
        bool docksEmpty = true;
        foreach (Dock dock in docks) {
            if (dock.IsOccupied) {
                docksEmpty = false;
            }
        }
        if (!docksEmpty) return;

        GameManager.Instance.NextPhase();
    }

    #region Orderer Management

    void AssignNextOrdererDelayed(Dock openDock, float delay) {
        Util.DoAfterSeconds(this, delay, () => AssignNextOrderer(openDock), orderPhaseActive);
    }
    void AssignNextOrderer(Dock openDock) {
        if (openDock.IsOccupied) {
            Debug.LogError("Unable to assign next orderer: Dock is occupied.");
            return;
        }

        // Generate Order and Orderer
        Order order = GenerateOrder();
        if (order == null) {
            return;
        }

        Orderer orderer = Instantiate(ordererObj, openDock.GetStartPoint(), Quaternion.identity).GetComponent<Orderer>();
        orderer.AssignOrder(order);
        orderer.OccupyDock(openDock);
    }

    Order GenerateOrder() {
        // Filter for valid order layouts
        List<SO_OrderLayout> diffFilteredLayoutData = orderLayouts.Where(entry => entry.DifficultyRating <= layoutDifficulty).ToList();
        if (diffFilteredLayoutData.Count == 0) {
            Debug.LogError("Unable to generate order: out of stock.");
            return null;
        }
        
        // TODO: weighted random nice bell curve favoring current difficulty (but still allowing some of below difficulties)
        SO_OrderLayout selectedOrderLayout = Util.GetRandomFromList(diffFilteredLayoutData).Copy();
        Order order = new Order(selectedOrderLayout, baseOrderTime, timePerProduct, baseOrderValue, valuePerProduct);
        
        return order;
    }

    public void HandleFinishedOrderer(Orderer orderer) {
        if (orderer.Order.IsFulfilled) {
            GameManager.Instance.ModifyGold(orderer.Order.TotalValue());
            if (PerfectOrders) GameManager.Instance.ModifyGold(perfectOrdersBonus);

            SoundManager.Instance.PlaySound(SoundID.OrderFulfilled);

            numOrdersFulfilled++;
            OnOrderFulfilled?.Invoke(numOrdersFulfilled, numNeedOrdersFulfilled);
            // TEMP: until finalizing level system
            if (MetQuota) {
                OrderPhaseTimer.End();
            }
        } else {
            PerfectOrders = false;
            SoundManager.Instance.PlaySound(SoundID.OrderFailed);
        }

        if (orderPhaseActive.Value) {
            AssignNextOrdererDelayed(orderer.AssignedDock, Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }
        
        orderer.Docker.OnReachedEnd += () => {
            Destroy(orderer.gameObject);
            TryTriggerOrderPhaseEnd();
        };
    }

    #endregion

    public void SetDifficultyOptions(SO_OrdersDifficultyTable.OrderDifficultyEntry orderDiffEntry) {
        numNeedOrdersFulfilled = orderDiffEntry.numNeedOrdersFulfilled;
        layoutDifficulty = orderDiffEntry.layoutDifficulty;
        numActiveDocks = orderDiffEntry.numActiveDocks;
        baseOrderTime = orderDiffEntry.baseOrderTime;
        baseOrderValue = orderDiffEntry.baseOrderValue;
    }
    
    void LoadOrderLayouts() {
        if (DebugManager.DebugMode && !DebugManager.Instance.DoSetDifficulty) return;

        orderLayouts.Clear();
        SO_OrderLayout[] loadedLayouts = Resources.LoadAll<SO_OrderLayout>("OrderLayouts/");

        if (loadedLayouts.Length > 0) {
            orderLayouts.AddRange(loadedLayouts);
        } else {
            Debug.LogError("No order layouts found in Resources.");
        }
    }
}