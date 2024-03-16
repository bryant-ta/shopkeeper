using System;
using System.Collections.Generic;
using System.Linq;
using Timers;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [Header("Order Queue")]
    [SerializeField] int numTotalOrders;
    [SerializeField] int numActiveOrders;
    [SerializeField] int minNextOrderDelay;
    [SerializeField] int maxNextOrderDelay;
    
    [Header("Order Parameters")]
    [SerializeField] int minTimePerOrder;
    [SerializeField] int timePerProduct;
    [SerializeField] int goldPerProduct;

    [Header("Quantity Order Type")]
    [SerializeField] int quantityOrderTotalMin;
    [SerializeField] int quantityOrderTotalMax;

    [Header("Variety Order Type")]
    [SerializeField] int varietyOrderTotalMin;
    [SerializeField] int varietyOrderTotalMax;
    [SerializeField] int varietyOrderIndividualMax;

    [Header("Zone")]
    [SerializeField] Vector3Int dropOffZoneDimensions;
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

        dropOffZone.OnEnterZone += TryFulfillOrder;

        GameManager.Instance.SM_dayPhase.OnStateEnter += EnterStateTrigger;
        GameManager.Instance.SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void Start() {
        // Create drop off zone
        ZoneProperties dropOffZoneProps = new ZoneProperties() {CanPlace = false};
        dropOffZone.Setup(Vector3Int.RoundToInt(transform.localPosition), dropOffZoneDimensions, dropOffZoneProps);
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

        for (int i = 0; i < numActiveOrders; i++) {
            ActivateNextOrder(i);
        }
    }
    void StopOrders() {
        for (int i = 0; i < activeOrders.Length; i++) {
            if (activeOrders[i] != null) {
                activeOrders[i].StopOrder();
                ResetActiveOrderSlot(i);
            }
        }

        backlogOrders.Clear();
    }

    void ActivateNextOrderDelayed(int activeOrderIndex) {
        ResetActiveOrderSlot(activeOrderIndex);
        Util.DoAfterSeconds(
            this, Random.Range(minNextOrderDelay, maxNextOrderDelay), isOpenPhase, () => ActivateNextOrder(activeOrderIndex)
        );
    }
    void ActivateNextOrder(int activeOrderIndex) {
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

    void ResetActiveOrderSlot(int activeOrderIndex) {
        OnActiveOrderChanged?.Invoke(
            new ActiveOrderChangedArgs {
                ActiveOrderIndex = activeOrderIndex,
                NewOrder = null,
                NumRemainingOrders = NumRemainingOrders
            }
        );
    }

    #endregion

    #region Order Generation

    // Populates backlog of orders
    bool GenerateOrders(int numOrders) {
        // Stock is taken out from availableStock as they are added to generated orders, avoids repeats with non-existent stock.
        Dictionary<ProductID, List<Product>> availableStock = GameManager.GetStockedProductsCopy();

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

        quantityOrderTotalMin++;
        quantityOrderTotalMax++;
        varietyOrderIndividualMax++;
    }

    #endregion

    #region Order Fulfillment

    public void TryFulfillOrder(Grid fulfillmentGrid) {
        // Attempt to fulfill active orders with products in input grid
        List<IGridShape> shapes = fulfillmentGrid.AllShapes();
        for (int i = 0; i < shapes.Count; i++) {
            if (shapes[i].ColliderTransform.TryGetComponent(out Product product)) {
                if (MatchOrder(product)) {
                    // Consume product from its grid
                    fulfillmentGrid.DestroyShape(shapes[i]);

                    GameManager.RemoveStockedProduct(product);
                }
            }
        }
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
        ActivateNextOrderDelayed(activeOrderIndex);
    }
    void FailOrder(int activeOrderIndex) {
        PerfectOrders = false;
        ActivateNextOrderDelayed(activeOrderIndex);
    }

    #endregion
}

public class Order {
    public int Value { get; private set; }

    public float TimeToComplete { get; private set; }
    public CountdownTimer Timer { get; private set; }

    public int ActiveOrderIndex;

    public Dictionary<ProductID, int> Products => products;
    Dictionary<ProductID, int> products;

    public event Action OnProductFulfilled;
    public event Action<int> OnOrderFulfilled;
    public event Action<int> OnOrderFailed;

    int timePerProduct;
    int valuePerProduct;

    public Order(int minTimePerOrder, int timePerProduct, int valuePerProduct) {
        this.timePerProduct = timePerProduct;
        this.valuePerProduct = valuePerProduct;

        products = new();

        TimeToComplete = minTimePerOrder;
    }

    ~Order() { StopOrder(); }

    public void StartOrder() {
        Timer = new CountdownTimer(TimeToComplete);
        Timer.EndEvent += Fail;
        Timer.Start();
    }

    public bool TryFulfill(ProductID productID) {
        if (products.ContainsKey(productID)) { products[productID]--; } else { return false; }

        if (products[productID] == 0) { products.Remove(productID); }

        OnProductFulfilled?.Invoke();

        if (products.Count == 0) {
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
        if (products.ContainsKey(productID)) { products[productID]++; } else { products[productID] = 1; }

        TimeToComplete += timePerProduct;
        Value += valuePerProduct;
    }
    public void Remove(ProductID productID) {
        if (products.ContainsKey(productID)) {
            products[productID]--;
            TimeToComplete -= timePerProduct;
            Value -= valuePerProduct;
        }

        if (products[productID] == 0) {
            products.Remove(productID);
        }
    }

    // Don't need to explicitly cleanup event listeners, as long as all references of this Order are gone.

    public int TotalReward() { return Value + (int) TimeToComplete; }
    public new string ToString() {
        string t = "";

        foreach (KeyValuePair<ProductID, int> order in products) {
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
}