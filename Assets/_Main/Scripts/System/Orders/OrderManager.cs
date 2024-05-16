using System;
using System.Collections.Generic;
using System.Linq;
using Timers;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [Title("Order Queue")]
    [SerializeField] int numTotalOrders;
    [SerializeField] int numActiveOrders;
    [SerializeField] int minNextOrderDelay;
    [SerializeField] int maxNextOrderDelay;

    [Title("Order Parameters")]
    [SerializeField] int minTimePerOrder;
    [SerializeField] int timePerProduct;
    [SerializeField] int goldPerProduct;

    [Title("Quantity Order Type")]
    [SerializeField] int quantityOrderTotalMin;
    [SerializeField] int quantityOrderTotalMax;

    [Title("Variety Order Type")]
    [SerializeField] int varietyOrderTotalMin;
    [SerializeField] int varietyOrderTotalMax;
    [SerializeField] int varietyOrderIndividualMax;

    [Title("Zone")]
    [SerializeField] Zone dropOffZone;

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

    void Start() {
        // Create drop off zone
        ZoneProperties dropOffZoneProps = new ZoneProperties() {CanPlace = false};
        // dropOffZone.Setup(Vector3Int.RoundToInt(transform.localPosition), dropOffZoneDimensions, dropOffZoneProps);
        GameManager.WorldGrid.AddZone(dropOffZone);
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

        if (!GenerateOrders(numTotalOrders)) {
            Debug.LogError("Unable to generate orders.");
            return;
        }

        PerfectOrders = true;

        ActivateNextOrder(0); // always immediately activate first order
        for (int i = 1; i < numActiveOrders; i++) {
            ActivateNextOrderDelayed(i, Random.Range(minNextOrderDelay, maxNextOrderDelay));
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
    bool GenerateOrders(int numOrders) {
        // Stock is taken out from availableStock as they are added to generated orders, avoids repeats with non-existent stock.
        Dictionary<ProductID, List<Product>> availableStock = Ledger.GetStockedProductsCopy();

        for (int i = 0; i < numOrders; i++) {
            int orderType = Random.Range(0, 2);
            Order order;
            switch (orderType) {
                case 0: // Quantity
                    order = GenerateQuantityOrder(availableStock);
                    break;
                case 1: // Variety
                    order = GenerateVarietyOrder(availableStock);
                    break;
                default:
                    Debug.LogError("Unexpected orderType.");
                    return false;
            }

            if (order != null) backlogOrders.Enqueue(order);
        }

        return true;
    }

    Order GenerateQuantityOrder(Dictionary<ProductID, List<Product>> availableStock) {
        if (availableStock.Count == 0) {
            Debug.LogWarning("No available stock to generate orders from!");
            return null;
        }

        int randomQuantity = Random.Range(quantityOrderTotalMin, quantityOrderTotalMax + 1);

        Order order = new Order(minTimePerOrder, timePerProduct, goldPerProduct);
        ProductID requestedProductID = availableStock.Keys.ToArray()[Random.Range(0, availableStock.Count)];

        int quantity = Math.Min(randomQuantity, availableStock[requestedProductID].Count);
        for (int i = 0; i < quantity; i++) {
            order.Add(requestedProductID);
            availableStock[requestedProductID].Remove(availableStock[requestedProductID].Last());
            if (availableStock[requestedProductID].Count == 0) availableStock.Remove(requestedProductID);
        }

        return order;
    }

    Order GenerateVarietyOrder(Dictionary<ProductID, List<Product>> availableStock) {
        int orderTotal = Random.Range(varietyOrderTotalMin, varietyOrderTotalMax + 1);

        Order order = new Order(minTimePerOrder, timePerProduct, goldPerProduct);
        for (int i = 0; i < orderTotal; i++) {
            if (availableStock.Count == 0) {
                Debug.LogWarning("No available stock to generate orders from!");
                return null;
            }

            ProductID requestedProductID = availableStock.Keys.ToArray()[Random.Range(0, availableStock.Count)];

            int randomQuantity = Random.Range(1, varietyOrderIndividualMax + 1);
            int quantity = Math.Min(randomQuantity, availableStock[requestedProductID].Count);

            for (int j = 0; j < quantity; j++) {
                order.Add(requestedProductID);
                availableStock[requestedProductID].Remove(availableStock[requestedProductID].Last());
                if (availableStock[requestedProductID].Count == 0) availableStock.Remove(requestedProductID);
            }
        }

        return order;
    }

    void ScaleOrderDifficulty(int day) {
        if (day > 10) return;

        numTotalOrders = day / 2 + 3;
        NumRemainingOrders = numTotalOrders;

        quantityOrderTotalMax++;
        varietyOrderIndividualMax++;
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
            if (activeOrdersList[i].TryFulfill(product.ID)) {
                return true;
            }
        }

        return false;
    }

    void FulfillOrder(int activeOrderIndex) {
        NumRemainingOrders--;
        GameManager.Instance.ModifyGold(activeOrders[activeOrderIndex].TotalReward());
        ActivateNextOrderDelayed(activeOrderIndex, Random.Range(minNextOrderDelay, maxNextOrderDelay), true);

        SoundManager.Instance.PlaySound(SoundID.OrderFulfilled);
    }
    void FailOrder(int activeOrderIndex) {
        PerfectOrders = false;
        ActivateNextOrderDelayed(activeOrderIndex, Random.Range(minNextOrderDelay, maxNextOrderDelay), false);

        SoundManager.Instance.PlaySound(SoundID.OrderFailed);
    }

    #endregion
}

public class Order {
    int Value;

    public float TimeToComplete { get; private set; }
    public CountdownTimer Timer { get; private set; }

    public int ActiveOrderIndex;

    public Dictionary<ProductID, int> Products { get; private set; }

    public event Action OnProductFulfilled;
    public event Action<int> OnOrderFulfilled;
    public event Action<int> OnOrderFailed;

    int timePerProduct;
    int valuePerProduct;

    public Order(int minTimePerOrder, int timePerProduct, int valuePerProduct) {
        this.timePerProduct = timePerProduct;
        this.valuePerProduct = valuePerProduct;

        Products = new();

        TimeToComplete = minTimePerOrder;
    }

    ~Order() { StopOrder(); }

    public void StartOrder() {
        Timer = new CountdownTimer(TimeToComplete);
        Timer.EndEvent += Fail;
        Timer.Start();
    }

    public bool TryFulfill(ProductID productID) {
        if (Products.ContainsKey(productID)) { Products[productID]--; } else { return false; }

        if (Products[productID] == 0) { Products.Remove(productID); }

        OnProductFulfilled?.Invoke();

        if (Products.Count == 0) {
            StopOrder();
            OnOrderFulfilled?.Invoke(ActiveOrderIndex);
        }

        return true;
    }
    void Fail() { OnOrderFailed?.Invoke(ActiveOrderIndex); }

    public void StopOrder() {
        if (Timer.IsTicking) {
            Timer.EndEvent -= Fail;
            Timer.End();
        }
    }

    public void Add(ProductID productID) {
        if (Products.ContainsKey(productID)) { Products[productID]++; } else { Products[productID] = 1; }

        TimeToComplete += timePerProduct;
        Value += valuePerProduct;
    }
    public void Remove(ProductID productID) {
        if (Products.ContainsKey(productID)) {
            Products[productID]--;
            TimeToComplete -= timePerProduct;
            Value -= valuePerProduct;
        }

        if (Products[productID] == 0) {
            Products.Remove(productID);
        }
    }

    // Don't need to explicitly cleanup event listeners, as long as all references of this Order are gone.

    public int TotalReward() { return Value + (int) Timer.RemainingTimeSeconds; }
    public new string ToString() {
        string t = "";

        foreach (KeyValuePair<ProductID, int> order in Products) {
            t += $"<sprite name={order.Key}> {order.Value}\n"; // TEMP: -1 from how productID is setup currently
        }

        if (t.Length > 0) t = t.Remove(t.Length - 1, 1);

        return t;
    }
}

public struct ActiveOrderChangedArgs {
    public int ActiveOrderIndex;
    public int NumRemainingOrders;
    public Order NewOrder;
    public bool LastOrderFulfilled;
}