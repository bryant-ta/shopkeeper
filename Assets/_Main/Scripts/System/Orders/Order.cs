using System;
using System.Collections.Generic;
using System.Linq;
using Timers;
using UnityEngine;

namespace Orders {
public class Order {
    // TEMP: timer is unused until implementing time mode
    public float TimeToComplete { get; private set; }
    public CountdownTimer Timer { get; private set; }

    public List<Requirement> Requirements { get; private set; }

    int Value;
    
    int timePerProduct;
    int baseOrderValue;
    int valuePerProduct;

    public bool IsFulfilled { get; private set; }

    public event Action<int, int> OnProductFulfilled; // requirement index, quantity remaining until target
    public event Action OnOrderSucceeded;
    public event Action OnOrderFailed;

    public Order(int minTimePerOrder, int timePerProduct, int baseOrderValue, int valuePerProduct) {
        Requirements = new();
        this.timePerProduct = timePerProduct;
        this.baseOrderValue = baseOrderValue;
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
    public void StopOrder() {
        if (Timer != null && Timer.IsTicking) {
            Timer.EndEvent -= Fail;
            Timer.End();
        }
    }

    public bool Fulfill(ProductID productID) {
        // Match submitted productID
        bool acceptedSubmit = false;
        for (int i = 0; i < Requirements.Count; i++) {
            Requirement req = Requirements[i];
            if (req.Match(productID)) {
                req.CurQuantity++;
                OnProductFulfilled?.Invoke(i, req.QuantityUntilTarget());
                acceptedSubmit = true;
                break;
            }
        }

        return acceptedSubmit;
    }

    public void Remove(ProductID productID) {
        for (int i = 0; i < Requirements.Count; i++) {
            Requirement req = Requirements[i];
            if (req.Match(productID)) {
                req.CurQuantity--;
                OnProductFulfilled?.Invoke(i, req.QuantityUntilTarget());
                break;
            }
        }
    }

    // returns true if productID matches at least one requirement
    public bool Check(ProductID productID) {
        for (int i = 0; i < Requirements.Count; i++) {
            Requirement req = Requirements[i];
            if (req.Match(productID)) {
                return true;
            }
        }

        return false;
    }

    #region Submission

    public virtual bool IsFinished() {
        foreach (Requirement req in Requirements) {
            if (!req.IsFulfilled) {
                return false;
            }
        }

        return true;
    }

    public void Succeed() {
        StopOrder();
        IsFulfilled = true;
        OnOrderSucceeded?.Invoke();
    }
    void Fail() { 
        StopOrder();
        OnOrderFailed?.Invoke(); 
    }

    #endregion

    #region Helper

    // requirement input should have everything set, including target quantity
    public void AddRequirement(Requirement requirement) {
        if (Requirements.Contains(requirement)) {
            Debug.LogError("Unable to add requirement: Requirement already exists.");
            return;
        }

        Requirements.Add(requirement);

        TimeToComplete += timePerProduct * requirement.TargetQuantity;
    }

    public int TotalValue() {
        int total = baseOrderValue;
        foreach (Requirement req in Requirements) {
            total += req.TargetQuantity * valuePerProduct;
        }

        return Value;
    }

    public new string ToString() {
        string t = "";

        foreach (Requirement req in Requirements) {
            t += $"{req.Color}_{req.Pattern}_{req.ShapeDataID}x{req.QuantityUntilTarget()}\n";
        }

        if (t.Length > 0) t = t.Remove(t.Length - 1, 1);

        return t;
    }

    #endregion
}

public class MoldOrder : Order {
    public Mold Mold { get; private set; }

    public MoldOrder(int minTimePerOrder, int timePerProduct, int baseOrderValue, int valuePerProduct) :
        base(minTimePerOrder, timePerProduct, baseOrderValue, valuePerProduct) {
    }

    public void AddMold(Mold mold) { Mold = mold; }

    #region Submission

    public override bool IsFinished() {
        if (Mold == null) return true;

        // Mold must be fully occupied
        if (!Mold.IsFullyOccupied()) return false;

        // Mold must hold at least one shape that fits every requirement
        List<IGridShape> shapes = Mold.Grid.AllShapes();
        bool[] reqsPassed = new bool[Requirements.Count];
        for (int i = 0; i < Requirements.Count; i++) {
            foreach (IGridShape shape in shapes) {
                if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                    if (Requirements[i].Match(product.ID)) {
                        reqsPassed[i] = true;
                        break;
                    }
                }
            }
        }

        return reqsPassed.All(x => x);
    }

    #endregion
}
}