using System;
using System.Collections.Generic;
using System.Linq;
using Orders;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [Title("Quota")]
    [SerializeField] int quotaBase;
    int quotaTotal => quotaBase;
    [SerializeField] int numFulfilled;
    public bool MetQuota => numFulfilled >= quotaTotal;
    public event Action<int, int> OnOrderFulfilled; // <current number fulfilled, number orders needed for level>

    [Title("Order Queue")]
    [SerializeField] int numActiveDocks;
    [SerializeField] MinMax NextOrderDelay;

    [Title("Order Parameters")]
    [SerializeField] int baseOrderTime;
    [SerializeField] int timePerProduct;
    [SerializeField] int baseOrderValue;
    [SerializeField] int valuePerProduct;

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
    public List<Orderer> ActiveOrderers { get; private set; }

    public event Action<int> OnQuotaUpdated;

    void Awake() {
        LoadOrderLayouts();

        orderPhaseActive = new Util.ValueRef<bool>(false);
        docks = docksContainer.GetComponentsInChildren<Dock>().ToList();
        ActiveOrderers = new();

        GameManager.Instance.RunTimer.EndEvent += StopOrders;
        GameManager.Instance.SM_dayPhase.OnStateEnter += EnterStateTrigger;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) {
            SetDifficultyOptions();
        }
        if (state.ID == DayPhase.Order) {
            orderPhaseActive.Value = true;
            StartOrders();
        }
    }

    void StartOrders() {
        numFulfilled = 0;
        OnOrderFulfilled?.Invoke(0, quotaTotal);
        PerfectOrders = true;

        // Start sending Orderers
        AssignNextOrderer(docks[0]); // always immediately activate first order
        int activeDocks = Math.Min(numActiveDocks, docks.Count);
        for (var i = 1; i < activeDocks; i++) {
            AssignNextOrdererDelayed(docks[i], Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }
    }
    void StopOrders() {
        orderPhaseActive.Value = false; // Stops delayed orders and order generation chain

        foreach (Orderer orderer in ActiveOrderers) {
            if (orderer.Order.State == OrderState.Ready) {
                orderer.Order.Skip();
                orderer.OrderSkipped();
            }
        }

        TryTriggerOrderPhaseEnd();
    }

    void TryTriggerOrderPhaseEnd() {
        if (orderPhaseActive.Value) return;
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

        ActiveOrderers.Add(orderer);
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
        if (orderer.Order.State == OrderState.Fulfilled) {
            // GameManager.Instance.ModifyGold(orderer.Order.TotalValue());
            // if (PerfectOrders) GameManager.Instance.ModifyGold(perfectOrdersBonus);

            numFulfilled++;
            OnOrderFulfilled?.Invoke(numFulfilled, quotaTotal);
            // TEMP: until finalizing level system
            if (MetQuota) {
                StopOrders();
            }

            // TODO: calculate score based on matched color region + global mult
            float scoreMult = 1f;
            foreach (Product product in orderer.SubmittedProducts) {
                if (product.FulfillsColorRegion) {
                    scoreMult += product.ShapeData.Size * 0.1f;
                }
            }
            
            GameManager.Instance.ModifyScore(Mathf.RoundToInt(orderer.Order.TotalValue() * scoreMult));
            GameManager.Instance.AddRunTime(orderer.Order.TotalValue()); // TEMP: until determining calculation
            
            SoundManager.Instance.PlaySound(SoundID.OrderFulfilled);
        } else if (orderer.Order.State == OrderState.Failed) {
            PerfectOrders = false;
            SoundManager.Instance.PlaySound(SoundID.OrderFailed);
        }

        ActiveOrderers.Remove(orderer);

        if (orderPhaseActive.Value) {
            AssignNextOrdererDelayed(orderer.AssignedDock, Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }

        orderer.Docker.OnReachedEnd += () => {
            Destroy(orderer.gameObject);
        };
    }

    #endregion

    public void SetDifficultyOptions() {
        if (DebugManager.DebugMode && !DebugManager.Instance.DoSetDifficulty) return;

        SO_OrdersDifficultyTable.OrderDifficultyEntry orderDiffEntry = DifficultyManager.Instance.ApplyOrderDifficulty();

        layoutDifficulty = orderDiffEntry.layoutDifficulty;
        numActiveDocks = orderDiffEntry.numActiveDocks;

        if (GameManager.Instance.Difficulty % 10 == 0) {
            quotaBase++;
        }
        
        OnQuotaUpdated?.Invoke(quotaTotal);
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