using System;
using System.Collections.Generic;
using System.Linq;
using Timers;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [SerializeField] int numTotalOrders;
    [SerializeField] int numActiveOrders;

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

    Queue<Order> backlogOrders = new();
    Order[] activeOrders;

    public Action<int, Order> OnNewActiveOrder;

    void Awake() {
        activeOrders = new Order[numActiveOrders];
    }

    void Start() {
        // Create drop off zone
        ZoneProperties dropOffZoneProps = new ZoneProperties() {CanPlace = false};
        dropOffZone.Setup(Vector3Int.RoundToInt(transform.localPosition), dropOffZoneDimensions, dropOffZoneProps);
        GameManager.WorldGrid.AddZone(dropOffZone);

        dropOffZone.OnEnterZone += TryFulfillOrder;
        
        StartOrders();
    }

    #region Order Generation
    
    public void StartOrders() {
        if (backlogOrders.Count > 0) {
            Debug.LogError("Unable to start new orders: orders remain in backlog orders.");
            return;
        }
        
        if (!GenerateOrders()) {
            Debug.LogError("Unable to generate orders.");
            return;
        }

        for (int i = 0; i < numActiveOrders; i++) {
            ActivateNextOrder(i);
        }
    }

    void ActivateNextOrder(int activeOrderIndex) {
        if (backlogOrders.Count == 0) {
            activeOrders[activeOrderIndex] = null;
            OnNewActiveOrder?.Invoke(activeOrderIndex, null);
            return;
        }
        
        Order nextOrder = backlogOrders.Dequeue();
        nextOrder.Start();

        activeOrders[activeOrderIndex] = nextOrder;
        OnNewActiveOrder?.Invoke(activeOrderIndex, nextOrder);
    }

    // Populates backlog of orders
    bool GenerateOrders() {
        for (int i = 0; i < numTotalOrders; i++) {
            int orderType = Random.Range(0, 2);
            Order order;
            switch (orderType) {
                case 0: // Quantity
                    order = GenerateQuantityOrder();
                    break;
                case 1: // Variety
                    order = GenerateVarietyOrder();
                    break;
                default:
                    Debug.LogError("Unexpected orderType.");
                    return false;
            }
            
            backlogOrders.Enqueue(order);
        }

        return true;
    }

    Order GenerateQuantityOrder() {
        int orderTotal = Random.Range(quantityOrderTotalMin, quantityOrderTotalMax + 1);

        Order order = new Order();
        List<ProductID> availableStock = GameManager.GetStockedProductIDs();
        ProductID requestedProductID = availableStock[Random.Range(0, availableStock.Count)];
        for (int i = 0; i < orderTotal; i++) {
            order.Add(requestedProductID);
        }

        return order;
    }

    Order GenerateVarietyOrder() {
        int orderTotal = Random.Range(varietyOrderTotalMin, varietyOrderTotalMax + 1);

        Order order = new Order();
        for (int i = 0; i < orderTotal; i++) {
            List<ProductID> availableStock = GameManager.GetStockedProductIDs();
            ProductID requestedProductID = availableStock[Random.Range(0, availableStock.Count)];
            int individualCount = Random.Range(1, varietyOrderIndividualMax + 1);
            for (int j = 0; j < individualCount; j++) {
                order.Add(requestedProductID);
            }
        }

        return order;
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
                }
            }
        }
        
        // TODO: something about remaining products falling down in place of consumed ones
        
        // Finished fully fulfilled orders
        for (int i = 0; i < activeOrders.Length; i++) {
            if (activeOrders[i].IsComplete()) {
                GameManager.Instance.ModifyCoins(activeOrders[i].TotalReward());
        
                ActivateNextOrder(i);
            }
        }
    }
    
    // Returns true if successfully fulfilled an order with product
    bool MatchOrder(Product product) {
        // Prioritize order with least time left
        List<Order> sortedActiveOrders = activeOrders.ToList();
        sortedActiveOrders.Sort((a, b) => a.Timer.RemainingTimePercent.CompareTo(b.Timer.RemainingTimePercent));
        
        for (int i = 0; i < sortedActiveOrders.Count; i++) {
            if (sortedActiveOrders[i].TryFulfill(product.ID)) {
                return true;
            }
        }

        return false;
    }

    #endregion
}

public class Order {
    public int Value { get; private set; }
    
    public float TimeToComplete { get; private set; }
    public CountdownTimer Timer { get; private set; }

    Dictionary<ProductID, int> orders;

    public Action OnProductFulfilled;

    public Order() { orders = new(); }

    public void Start() {
        Timer = new CountdownTimer(TimeToComplete);
        Timer.Start();
    }
    public bool TryFulfill(ProductID productID) {
        if (orders.ContainsKey(productID)) { orders[productID]--; }
        else { return false; }
        
        if (orders[productID] == 0) { orders.Remove(productID); }
        
        OnProductFulfilled?.Invoke();
        
        return true;
    }

    public void Add(ProductID productID) {
        if (orders.ContainsKey(productID)) { orders[productID]++; }
        else { orders[productID] = 1; }

        TimeToComplete += 10f;
        Value += 10;
    }
    public void Remove(ProductID productID) {
        if (orders.ContainsKey(productID)) {
            orders[productID]--;
            TimeToComplete -= 10f;
            Value -= 10;
        }

        if (orders[productID] == 0) {
            orders.Remove(productID);
        }
    }
    
    // Don't need to explicitly cleanup event listeners, as long as all references of this Order are gone.

    public int TotalReward() { return Value + (int) TimeToComplete; }
    public bool IsComplete() { return orders.Count == 0; }
    public new string ToString() {
        string t = "";
        
        foreach (KeyValuePair<ProductID, int> order in orders) {
            t += $"<sprite name={order.Key}> {order.Value}\n"; // TEMP: -1 from how productID is setup currently
        }
        
        if (t.Length > 0) t = t.Remove(t.Length - 1, 1);

        return t;
    }
}