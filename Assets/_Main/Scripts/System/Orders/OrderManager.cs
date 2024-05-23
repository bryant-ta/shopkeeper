using System;
using System.Collections.Generic;
using System.Linq;
using Orders;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [Header("Order Queue")]
    [SerializeField] int numTotalOrders;
    [SerializeField] int numActiveOrders;
    [SerializeField] MinMax NextOrderDelay;

    [Header("Order Parameters")]
    [SerializeField] MinMax numReqsPerOrder;
    [SerializeField] int minTimePerOrder;
    [SerializeField] int timePerProduct;
    [SerializeField] int goldPerProduct;

    [Header("Requirement Paramenters")]
    [SerializeField] MinMax ReqQuantity;
    [Tooltip("Starting chance generate a Requirement that pulls from available stock. Decreases as difficulty increases.")]
    [SerializeField, Range(0.5f, 1f)] float chanceReqFromExisting = 0.5f;
    [SerializeField, Range(0f, 1f)] float chanceReqNeedsColor;
    [SerializeField, Range(0f, 1f)] float chanceReqNeedsShape;

    [Header("Orderers")]
    [SerializeField] List<Dock> docks; // TEMP: using just as spawn points until orderer anims
    [SerializeField] GameObject ordererObj;
    [SerializeField] Transform ordererSpawnPoint;

    [Header("Other")]
    [SerializeField] ListList<ShapeDataID> shapeDifficultyPool;

    // true if all orders for the day are fulfilled
    [field: SerializeField, ReadOnly] public bool PerfectOrders { get; private set; }
    [field: SerializeField, ReadOnly] public int NumRemainingOrders { get; private set; }

    Queue<Order> orderBacklog = new();
    // Order[] activeOrders;

    Util.ValueRef<bool> isOpenPhase;

    void Awake() {
        isOpenPhase = new Util.ValueRef<bool>(false);

        GameManager.Instance.SM_dayPhase.OnStateEnter += EnterStateTrigger;
        GameManager.Instance.SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Open) {
            isOpenPhase.Value = true;
            ScaleOrderDifficulty(GameManager.Instance.Day);
            StartOrders();
        }
    }
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Open) {
            isOpenPhase.Value = false;
            StopOrders();
        }
    }

    void StartOrders() {
        if (orderBacklog.Count > 0) {
            Debug.LogError("Unable to start new orders: orders remain in backlog orders.");
            return;
        }

        GenerateOrders(numTotalOrders);

        PerfectOrders = true;

        AssignNextOrderer(docks[0]); // always immediately activate first order
        int activeOrders = Math.Min(numActiveOrders, docks.Count);
        for (var i = 1; i < activeOrders; i++) {
            AssignNextOrdererDelayed(docks[i], Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }
    }
    void StopOrders() { orderBacklog.Clear(); }

    #region Orderer Management

    public void AssignNextOrdererDelayed(Dock openDock, float delay) {
        Util.DoAfterSeconds(this, delay, () => AssignNextOrderer(openDock), isOpenPhase);
    }
    public void AssignNextOrderer(Dock openDock) {
        // Prevents delayed active orders from occuring at wrong phase, since ActivateNextOrderDelayed can keep counting after phase end
        // TODO: fix so this isnt needed
        if (GameManager.Instance.CurDayPhase != DayPhase.Open) {
            return;
        }

        if (orderBacklog.Count == 0) {
            return;
        }

        if (openDock.IsOccupied) {
            Debug.LogError("Unable to assign next orderer: Dock is occupied.");
            return;
        }

        Orderer orderer = Instantiate(ordererObj, ordererSpawnPoint).GetComponent<Orderer>();
        orderer.SetOrder(orderBacklog.Dequeue());
        orderer.OccupyDock(openDock);
    }

    public void HandleFinishedOrderer(Orderer orderer) {
        AssignNextOrdererDelayed(orderer.AssignedDock, Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        NumRemainingOrders--;

        if (orderer.Order.IsFulfilled) {
            GameManager.Instance.ModifyGold(orderer.Order.TotalValue());
            SoundManager.Instance.PlaySound(SoundID.OrderFulfilled);
        } else {
            PerfectOrders = false;
            SoundManager.Instance.PlaySound(SoundID.OrderFailed);
        }

        Destroy(orderer.gameObject); // TEMP: until anim
    }
    
    #endregion

    #region Order Generation

    // Populates backlog of orders
    void GenerateOrders(int numOrders) {
        // Stock is taken out from availableStock as they are added to generated orders, avoids repeats with non-existent stock.
        Dictionary<ProductID, int> availableStock = new();
        foreach (KeyValuePair<ProductID, List<Product>> kv in Ledger.StockedProducts) {
            availableStock[kv.Key] = kv.Value.Count;
        }

        for (int i = 0; i < numOrders; i++) {
            Order order = new Order(minTimePerOrder, timePerProduct, goldPerProduct);
            int numReqs = Random.Range(numReqsPerOrder.Min, numReqsPerOrder.Max);

            for (int j = 0; j < numReqs; j++) {
                Requirement req = Random.Range(0f, 1f) < chanceReqFromExisting ?
                    MakeRequirementFromExisting(availableStock) :
                    MakeRequirement();
                order.Add(req);
            }

            orderBacklog.Enqueue(order);
        }
    }

    Requirement MakeRequirement() {
        int quantity = Random.Range(ReqQuantity.Min, ReqQuantity.Max + 1);
        Requirement req = new Requirement(null, null, null, quantity);

        if (Random.Range(0f, 1f) <= chanceReqNeedsColor) {
            List<Color> c = Ledger.Instance.ColorPaletteData.Colors;
            req.Color = c[Random.Range(0, c.Count)];
        }

        // if (Random.Range(0, 2) < 1) {
        //     req.Pattern = Ledger.Instance.PatternPaletteData.Patterns[Random.Range(0, 2)];
        // }
        if (Random.Range(0f, 1f) <= chanceReqNeedsShape) {
            List<ShapeDataID> s = shapeDifficultyPool.outerList[0].innerList;
            req.ShapeDataID = s[Random.Range(0, s.Count)];
        }

        return req;
    }

    Requirement MakeRequirementFromExisting(Dictionary<ProductID, int> availableStock) {
        if (availableStock.Count == 0) {
            Debug.LogWarning("No available stock to generate orders from!");
            return null;
        }

        ProductID productID = availableStock.Keys.ToArray()[Random.Range(0, availableStock.Count)];
        int randomQuantity = Random.Range(ReqQuantity.Min, ReqQuantity.Max + 1);
        int quantity = Math.Min(randomQuantity, availableStock[productID]);
        Requirement req = new Requirement(productID.Color, productID.Pattern, productID.ShapeDataID, quantity);

        if (Random.Range(0f, 1f) > chanceReqNeedsColor) { req.Color = null; }

        // if (Random.Range(0f, 1f) < 1) {
        //     req.Pattern = Ledger.Instance.PatternPaletteData.Patterns[Random.Range(0, 2)];
        // }
        if (Random.Range(0f, 1f) > chanceReqNeedsShape) { req.ShapeDataID = null; }

        availableStock[productID] -= quantity;
        if (availableStock[productID] == 0) availableStock.Remove(productID);

        return req;
    }

    void ScaleOrderDifficulty(int day) {
        if (day > 10) return;

        numTotalOrders = day / 2 + 3;
        NumRemainingOrders = numTotalOrders;

        ReqQuantity.Max++;
    }

    #endregion
}

public struct ActiveOrderChangedArgs {
    public int ActiveOrderIndex;
    public int NumRemainingOrders;
    public Order NewOrder;
    public bool LastOrderFulfilled;
}