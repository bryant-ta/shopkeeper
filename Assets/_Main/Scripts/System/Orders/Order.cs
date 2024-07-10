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

    public OrderState State { get; private set; }

    public event Action OnProductFulfilled; // requirement index, quantity remaining until target
    public event Action OnOrderSucceeded;
    public event Action OnOrderFailed;

    public Order(SO_OrderLayout orderLayoutData, int minTimePerOrder, int timePerProduct, int baseOrderValue, int valuePerProduct) {
        OrderLayoutData = orderLayoutData;
        ShapeData = orderLayoutData.GetColorShapeData();

        List<Color> shuffledColors = new();
        for (int i = 0; i < Ref.DeliveryMngr.MaxColorIndex; i++) {
            shuffledColors.Add(Ledger.Instance.ColorPaletteData.Colors[i]);
        }
        shuffledColors = Util.ShuffleList(shuffledColors);

        Dictionary<Vector3Int, int> d = orderLayoutData.GetTilesDict();
        GridColors = new();
        foreach (KeyValuePair<Vector3Int, int> kv in d) {
            if (kv.Value == 0) {
                GridColors.Add(kv.Key, Ledger.Instance.WildColor);
            } else {
                GridColors.Add(kv.Key, shuffledColors[kv.Value - 1]);
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
        State = OrderState.Fulfilled;
        OnOrderSucceeded?.Invoke();
    }
    public void Fail() {
        StopOrder();
        State = OrderState.Failed;
        OnOrderFailed?.Invoke();
    }
    public void Skip() {
        StopOrder();
        State = OrderState.Skipped;
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

    // Flood-fill algorithm to find coords of a color region (adjacent cells with the same color)
    public HashSet<Vector3Int> IdentifyColorRegion(Vector3Int startCoord) {
        if (!GridColors.ContainsKey(startCoord)) {
            Debug.LogError("Unable to identify color region: start coordinate is outside grid.");
            return null;
        }

        Color color = GridColors[startCoord];
        HashSet<Vector3Int> region = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        queue.Enqueue(startCoord);

        while (queue.Count > 0) {
            Vector3Int curCoord = queue.Dequeue();
            if (region.Contains(curCoord)) continue;

            if (GridColors.TryGetValue(curCoord, out Color currentColor) && currentColor == color) {
                region.Add(curCoord);
                for (int d = 0; d < 4; d++) {
                    queue.Enqueue(curCoord + DirectionData.DirectionVectorsInt[d]);
                }
            }
        }

        return region;
    }

    public new string ToString() { return OrderLayoutData.name; }

    #endregion
}

public enum OrderState {
    Ready = 0,
    Fulfilled = 1,
    Failed = 2,
    Skipped = 3,
}
}