using System;
using System.Collections.Generic;
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

    Queue<Order> backlogOrders = new();
    Order[] activeOrders;

    public Action<int, Order> OnNewActiveOrder;

    void Awake() {
        activeOrders = new Order[numActiveOrders];
    }

    void Start() { DoOrders(); }

    public void DoOrders() {
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
        Order nextOrder = backlogOrders.Dequeue();
        nextOrder.Start();

        activeOrders[activeOrderIndex] = nextOrder;
        OnNewActiveOrder.Invoke(activeOrderIndex, nextOrder);
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
}

public class Order {
    public int Value { get; private set; }
    public float TimeToComplete { get; private set; }
    public CountdownTimer OrderTimer { get; private set; }

    Dictionary<ProductID, int> orders;

    public Order() { orders = new(); }

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

    public void Start() {
        OrderTimer = new CountdownTimer(TimeToComplete);
        OrderTimer.Start();
    }

    public new string ToString() {
        string t = "";
        
        foreach (KeyValuePair<ProductID, int> order in orders) {
            t += $"<sprite index={(int)order.Key - 1}> {order.Value}\n"; // TEMP: -1 from how productID is setup currently
        }
        t = t.Remove(t.Length - 1, 1);

        return t;
    }
    public int TotalReward() { return Value + (int) TimeToComplete; }
}