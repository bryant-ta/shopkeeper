using System;
using System.Collections.Generic;
using Timers;
using UnityEngine;

namespace Orders {
public class Order {
    // TEMP: timer is unused until implementing time mode
    public float TimeToComplete { get; private set; }
    public CountdownTimer Timer { get; private set; }

    public int ActiveOrderIndex;

    // some fields of productID may not be set
    public List<Requirement> Requirements { get; private set; }

    int Value;
    int timePerProduct;
    int valuePerProduct;

    public event Action<int> OnProductFulfilled; // passes quantity remaining until target
    public event Action<int> OnOrderFulfilled;
    public event Action<int> OnOrderFailed;

    public Order(int minTimePerOrder, int timePerProduct, int valuePerProduct) {
        Requirements = new();
        this.timePerProduct = timePerProduct;
        this.valuePerProduct = valuePerProduct;

        TimeToComplete = minTimePerOrder;
    }

    // Don't need to explicitly cleanup event listeners, as long as all references of this Order are gone.
    ~Order() { StopOrder(); }

    public void StartOrder() {
        Timer = new CountdownTimer(TimeToComplete);
        Timer.EndEvent += Fail;
        Timer.Start();
    }

    public bool Submit(ProductID productID) {
        bool acceptedSubmit = false;
        foreach (Requirement req in Requirements) {
            if (req.Match(productID)) {
                req.CurQuantity++;
                OnProductFulfilled?.Invoke(req.QuantityUntilTarget());
                acceptedSubmit = true;
                break;
            }
        }

        bool orderIsFulfilled = true;
        foreach (Requirement req in Requirements) {
            if (!req.IsFulfilled) {
                orderIsFulfilled = false;
                break;
            }
        }

        if (orderIsFulfilled) {
            StopOrder();
            OnOrderFulfilled?.Invoke(ActiveOrderIndex);
        }

        return acceptedSubmit;
    }

    public void Remove(ProductID productID) {
        foreach (Requirement req in Requirements) {
            if (req.Match(productID)) {
                req.CurQuantity--;
                OnProductFulfilled?.Invoke(req.QuantityUntilTarget());
                break;
            }
        }
    }

    void Fail() { OnOrderFailed?.Invoke(ActiveOrderIndex); }

    public void StopOrder() {
        if (Timer.IsTicking) {
            Timer.EndEvent -= Fail;
            Timer.End();
        }
    }

    // requirement input should have everything set, including target quantity
    public void Add(Requirement requirement) {
        if (Requirements.Contains(requirement)) {
            Debug.LogError("Unable to add requirement: Requirement already exists.");
            return;
        }

        Requirements.Add(requirement);

        TimeToComplete += timePerProduct;
        Value += valuePerProduct;
    }

    // TODO: possibly just calculate directly from requirements
    public int TotalReward() { return Value + (int) Timer.RemainingTimeSeconds; }

    public new string ToString() {
        string t = "";

        foreach (Requirement req in Requirements) {
            t += $"{req.Color}_{req.Pattern}_{req.ShapeDataID}x{req.QuantityUntilTarget()}\n";
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
    public bool IsFulfilled => QuantityUntilTarget() == 0;

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
}