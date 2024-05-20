using System;
using System.Collections.Generic;
using System.Linq;
using Orders;
using TriInspector;
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

    [Header("Other")]
    [SerializeField] ListList<ShapeDataID> shapeDifficultyPool;
    [SerializeField] List<Orderer> orderers;

    // true if all orders for the day are fulfilled
    [field: SerializeField, ReadOnly] public bool PerfectOrders { get; private set; }
    [field: SerializeField, ReadOnly] public int NumRemainingOrders { get; private set; }

    Queue<Order> backlogOrders = new();
    Order[] activeOrders;

    Util.ValueRef<bool> isOpenPhase;

    public event Action<ActiveOrderChangedArgs> OnActiveOrderChanged;

    void Awake() {
        activeOrders = new Order[numActiveOrders];
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

    #region Active Orders

    void StartOrders() {
        if (backlogOrders.Count > 0) {
            Debug.LogError("Unable to start new orders: orders remain in backlog orders.");
            return;
        }

        GenerateOrders(numTotalOrders);

        PerfectOrders = true;

        ActivateNextOrder(0); // always immediately activate first order
        for (int i = 1; i < numActiveOrders; i++) {
            ActivateNextOrderDelayed(i, Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }
    }
    void StopOrders() {
        // Commented: letting current active orders finish
        // for (int i = 0; i < activeOrders.Length; i++) {
        //     if (activeOrders[i] != null) {
        //         activeOrders[i].StopOrder();
        //         ResetActiveOrderSlot(i);
        //     }
        // }

        backlogOrders.Clear();
    }

    void ActivateNextOrderDelayed(int activeOrderIndex, float delay, bool lastOrderFulfilled = false) {
        ResetActiveOrderSlot(activeOrderIndex, lastOrderFulfilled);
        Util.DoAfterSeconds(this, delay, () => ActivateNextOrder(activeOrderIndex), isOpenPhase);
    }
    void ActivateNextOrder(int activeOrderIndex) {
        // Prevents delayed active orders from occuring at wrong phase, since ActivateNextOrderDelayed can keep counting after phase end
        if (GameManager.Instance.CurDayPhase != DayPhase.Open) {
            return;
        }

        if (backlogOrders.Count == 0) {
            ResetActiveOrderSlot(activeOrderIndex);
            return;
        }

        Order nextOrder = backlogOrders.Dequeue();

        nextOrder.StartOrder();
        nextOrder.OnOrderFulfilled += FulfillOrder;
        nextOrder.OnOrderFailed += FailOrder;

        activeOrders[activeOrderIndex] = nextOrder;
        nextOrder.ActiveOrderIndex = activeOrderIndex;
        OnActiveOrderChanged?.Invoke(
            new ActiveOrderChangedArgs {
                ActiveOrderIndex = activeOrderIndex,
                NewOrder = nextOrder,
                NumRemainingOrders = NumRemainingOrders
            }
        );
    }

    void ResetActiveOrderSlot(int activeOrderIndex, bool lastOrderFulfilled = false) {
        OnActiveOrderChanged?.Invoke(
            new ActiveOrderChangedArgs {
                ActiveOrderIndex = activeOrderIndex,
                NewOrder = null,
                NumRemainingOrders = NumRemainingOrders,
                LastOrderFulfilled = lastOrderFulfilled
            }
        );
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
                Requirement req = Random.Range(0f, 1f) < chanceReqFromExisting ? CreateOrderFromExisting(availableStock) : CreateOrder();
                order.Add(req);
            }

            backlogOrders.Enqueue(order);
        }
    }

    Requirement CreateOrder() {
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

    Requirement CreateOrderFromExisting(Dictionary<ProductID, int> availableStock) {
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

    #region Order Fulfillment

    public void TryFillOrder(Grid fulfillmentGrid) {
        // Attempt to fulfill active orders with products in input grid
        List<IGridShape> shapes = fulfillmentGrid.AllShapes();
        bool matched = false;
        for (int i = 0; i < shapes.Count; i++) {
            if (shapes[i].ColliderTransform.TryGetComponent(out Product product)) {
                if (MatchOrder(product)) {
                    // Product fulfilled, consume product from its grid
                    matched = true;
                    fulfillmentGrid.DestroyShape(shapes[i]);

                    Ledger.RemoveStockedProduct(product);
                }
            }
        }

        if (matched) { SoundManager.Instance.PlaySound(SoundID.OrderProductFilled); }
    }

    // Returns true if successfully fulfilled an order with product
    bool MatchOrder(Product product) {
        // Prioritize order with least time left
        List<Order> activeOrdersList = activeOrders.ToList();
        for (int i = activeOrdersList.Count - 1; i >= 0; i--) {
            if (activeOrdersList[i] == null || !activeOrdersList[i].Timer.IsTicking) {
                activeOrdersList.Remove(activeOrdersList[i]);
            }
        }

        activeOrdersList.Sort((a, b) => a.Timer.RemainingTimePercent.CompareTo(b.Timer.RemainingTimePercent));

        for (int i = 0; i < activeOrdersList.Count; i++) {
            if (activeOrdersList[i].Submit(product.ID)) {
                return true;
            }
        }

        return false;
    }

    void FulfillOrder(int activeOrderIndex) {
        NumRemainingOrders--;
        GameManager.Instance.ModifyGold(activeOrders[activeOrderIndex].TotalValue());
        ActivateNextOrderDelayed(activeOrderIndex, Random.Range(NextOrderDelay.Min, NextOrderDelay.Max), true);

        SoundManager.Instance.PlaySound(SoundID.OrderFulfilled);
    }
    void FailOrder(int activeOrderIndex) {
        PerfectOrders = false;
        ActivateNextOrderDelayed(activeOrderIndex, Random.Range(NextOrderDelay.Min, NextOrderDelay.Max), false);

        SoundManager.Instance.PlaySound(SoundID.OrderFailed);
    }

    #endregion
}

public struct ActiveOrderChangedArgs {
    public int ActiveOrderIndex;
    public int NumRemainingOrders;
    public Order NewOrder;
    public bool LastOrderFulfilled;
}