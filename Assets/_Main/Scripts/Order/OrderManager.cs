using System.Collections.Generic;
using Timers;
using UnityEngine;

public class OrderManager : MonoBehaviour {
    [SerializeField] int numOrders;
    
    [Header("Quantity Order Type")]
    [SerializeField] int quantityOrderTotalMin;
    [SerializeField] int quantityOrderTotalMax;
    
    [Header("Variety Order Type")]
    [SerializeField] int varietyOrderTotalMin;
    [SerializeField] int varietyOrderTotalMax;

    public void DoOrders() {
        for (int i = 0; i < numOrders; i++) {
            Order order = GenerateOrder();
            CountdownTimer orderTimer = new CountdownTimer(order.TimeToComplete);
            orderTimer.Start();
            orderTimer.EndEvent += DebugOrderDone;
        }
    }

    void DebugOrderDone() {  // TEMP
        print("Order is over!");
    }
    
    public Order GenerateOrder() {
        int orderType = Random.Range(0, 2);

        switch (orderType) {
            case 0: // Quantity
                return GenerateQuantityOrder();
            case 1: // Variety
                return GenerateVarietyOrder();
            default:
                Debug.LogError("Unexpected orderType");
                return null;
        }
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
            order.Add(requestedProductID);
        }

        return order;
    }
}

public class Order {
    public float TimeToComplete = 1f;
    public int Value = 0;
    
    Dictionary<ProductID, int> orders;

    public Order() {
        orders = new();
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

    public int TotalReward() {
        return Value + (int)TimeToComplete;
    }
}