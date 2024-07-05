using System;
using System.Collections.Generic;
using Timers;
using UnityEngine;

namespace Orders {
public class Order {
    public SO_OrderLayout OrderLayoutData { get; private set; }

    public ShapeData ShapeData { get; private set; }
    public Dictionary<Vector3Int, Color> GridColors;

    // TEMP: timer is unused until implementing time mode
    public float TimeToComplete { get; private set; }
    public CountdownTimer Timer { get; private set; }

    int timePerProduct;
    int baseOrderValue;
    int valuePerProduct;

    public bool IsFulfilled { get; private set; }

    public event Action OnProductFulfilled; // requirement index, quantity remaining until target
    public event Action OnOrderSucceeded;
    public event Action OnOrderFailed;

    public Order(SO_OrderLayout orderLayoutData, int minTimePerOrder, int timePerProduct, int baseOrderValue, int valuePerProduct) {
        OrderLayoutData = orderLayoutData;
        ShapeData = orderLayoutData.GetColorShapeData();

        Dictionary<Vector3Int, int> d = orderLayoutData.GetTilesDict();
        GridColors = new();
        foreach (KeyValuePair<Vector3Int, int> kv in d) {
            if (kv.Value == 0) {
                GridColors.Add(kv.Key, Ledger.Instance.WildColor);
            } else {
                GridColors.Add(kv.Key, Ledger.Instance.ColorPaletteData.Colors[kv.Value - 1]);
            }
        }

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
        // TODO: determine if more needs to be done here... input should always be check before this call? or during it too
        OnProductFulfilled?.Invoke();
        return true;
    }

    #region Submission

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

    public int TotalValue() {
        // TODO: formula for order value

        return ShapeData.Size;
    }

    public HashSet<Color> GetColors() {
        HashSet<Color> colors = new();
        foreach (Color color in GridColors.Values) {
            colors.Add(color);
        }
        return colors;
    }

    public new string ToString() { return OrderLayoutData.name; }

    #endregion
}
}