using System;
using System.Collections.Generic;
using System.Linq;
using Timers;
using UnityEngine;

public class Order {
    public float TimeToComplete { get; private set; }
    public CountdownTimer Timer { get; private set; }

    public int ActiveOrderIndex;

    // some fields of productID may not be set
    public List<Requirement> Requirements { get; private set; }

    int Value;
    int timePerProduct;
    int valuePerProduct;

    public event Action OnProductFulfilled;
    public event Action<int> OnOrderFulfilled;
    public event Action<int> OnOrderFailed;

    public Order(int minTimePerOrder, int timePerProduct, int valuePerProduct) {
        this.timePerProduct = timePerProduct;
        this.valuePerProduct = valuePerProduct;

        Requirements = new();

        TimeToComplete = minTimePerOrder;
    }

    // Don't need to explicitly cleanup event listeners, as long as all references of this Order are gone.
    ~Order() { StopOrder(); }

    public void StartOrder() {
        Timer = new CountdownTimer(TimeToComplete);
        Timer.EndEvent += Fail;
        Timer.Start();
    }
    
    // should trigger ui counter + keep logic in sync
    public void Submit(ProductID productID) {
        foreach (Requirement requirement in Requirements) {
            if (requirement.Match(productID)) {
                requirement.CurQuantity++;
                break;
            }
        }

        
        
        
        

        if (Requirements.ContainsKey(productID)) { Requirements[productID]--; } else { return; }

        if (Requirements[productID] == 0) { Requirements.Remove(productID); }

        OnProductFulfilled?.Invoke();

        if (Requirements.Count == 0) {
            StopOrder();
            OnOrderFulfilled?.Invoke(ActiveOrderIndex);
        }

        return;
    }

    // Call when product is removed from order grid (only when order has mold)
    public void Remove() {
        
    }
    
    
    
    
    
    
    void Fail() { OnOrderFailed?.Invoke(ActiveOrderIndex); }

    public void StopOrder() {
        if (Timer.IsTicking) {
            Timer.EndEvent -= Fail;
            Timer.End();
        }
    }

    // requirement should come in with everything set exactly, including targetquantity
    public void Add(Requirement requirement) {
        if (Requirements.Contains(requirement)) {
            Debug.LogError("Unable to add requirement: Requirement already exists.");
            return;
        }
        
        Requirements.Add(requirement);

        TimeToComplete += timePerProduct;
        Value += valuePerProduct;
    }

    public int TotalReward() { return Value + (int) Timer.RemainingTimeSeconds; } // TODO: possibly just calculate directly from requirements

    public new string ToString() {
        string t = "";

        foreach (KeyValuePair<ProductID, int> order in Requirements) {
            t += $"<sprite name={order.Key}> {order.Value}\n"; // TEMP: -1 from how productID is setup currently
        }

        if (t.Length > 0) t = t.Remove(t.Length - 1, 1);

        return t;
    }
}

// TODO: namespace this?
public class Requirement {
    public Color? Color;
    public Pattern? Pattern;
    public ShapeDataID? ShapeDataID;
    public int TargetQuantity;
    public int CurQuantity;

    public Requirement(Color? color, Pattern? pattern, ShapeDataID? shapeDataID, int targetQuantity) {
        Color = color;
        Pattern = pattern;
        ShapeDataID = shapeDataID;
        TargetQuantity = targetQuantity;
    }

    public int QuantityUntilTarget() {
        int r = TargetQuantity - CurQuantity;
        if (r < 0) r = 0;
        return r;
    }

    public bool Match(ProductID productID) {
        return (Color == null || Color == productID.Color) &&
               (Pattern == null || Pattern == productID.Pattern) &&
               (ShapeDataID == null || ShapeDataID == productID.ShapeDataID);
    }
    
    public bool Match(Requirement requirement) {
        return Color == requirement.Color && Pattern == requirement.Pattern && ShapeDataID == requirement.ShapeDataID;
    }
}