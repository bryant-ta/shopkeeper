using System;
using System.Collections.Generic;
using System.Linq;
using Orders;
using Timers;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [Title("Order Queue")]
    [SerializeField] int numActiveDocks;
    [SerializeField] MinMax NextOrderDelay;

    [Title("Order Parameters")]
    [SerializeField] MinMax numReqsPerOrder;
    [SerializeField] int baseOrderTime;
    [SerializeField] int timePerProduct;
    [SerializeField] int baseOrderValue;
    [SerializeField] int valuePerProduct;
    [SerializeField] int perfectOrdersBonus;
    [field: SerializeField] public CountdownTimer OrderPhaseTimer { get; private set; }

    [Title("Requirement Paramenters")]
    [SerializeField] MinMax reqQuantity;
    [Tooltip("Chance to generate a Requirement that pulls from available stock.")]
    [SerializeField, Range(0f, 1f)] float reqChanceFromExisting = 0.5f;
    [SerializeField, Range(0f, 1f)] float reqChanceNeedsColor;
    [SerializeField, Range(0f, 1f)] float reqChanceNeedsShape;
    [Tooltip("Difficulty Table for requested shapes in requirements. Only used in non-fromExisting orders")]
    [SerializeField] List<ShapeDataID> reqVirtualShapePool;

    [Title("Mold Orders")]
    [SerializeField] GameObject moldOrdererObj;
    [SerializeField, Range(0f, 1f)] float moldChance;
    [Tooltip("Difficulty Table for mold shapes.")]
    [SerializeField] List<ShapeDataID> moldShapePool;

    [Title("Orderers")]
    [SerializeField] Transform docksContainer;
    List<Dock> docks;
    [SerializeField] GameObject ordererObj;

    [field: Title("ReadOnly")]
    [field: SerializeField, ReadOnly] public bool PerfectOrders { get; private set; } // true if all orders for the day are fulfilled

    Dictionary<ProductID, int> availableStock = new();  // Count of different products by product ID
    Dictionary<Color, int> availableColorCells = new(); // Count of cells by color

    Util.ValueRef<bool> orderPhaseActive;

    void Awake() {
        orderPhaseActive = new Util.ValueRef<bool>(false);

        OrderPhaseTimer = new CountdownTimer(GameManager.Instance.OrderPhaseDuration);

        docks = docksContainer.GetComponentsInChildren<Dock>().ToList();

        GameManager.Instance.SM_dayPhase.OnStateEnter += EnterStateTrigger;
        GameManager.Instance.SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            DifficultyManager.Instance.ApplyOrderDifficulty();
            orderPhaseActive.Value = true;
            StartOrders();
        }
    }
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            OrderPhaseTimer.Reset();
        }
    }

    void StartOrders() {
        foreach (KeyValuePair<ProductID, List<Product>> kv in Ledger.StockedProducts) {
            availableStock[kv.Key] = kv.Value.Count;
        }
        availableColorCells = new(Ledger.CellCountByColor);

        PerfectOrders = true;

        // Start sending Orderers
        AssignNextOrderer(docks[0]); // always immediately activate first order
        int activeDocks = Math.Min(numActiveDocks, docks.Count);
        for (var i = 1; i < activeDocks; i++) {
            AssignNextOrdererDelayed(docks[i], Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }

        // Start Order Phase timer
        OrderPhaseTimer.Start();
        OrderPhaseTimer.EndEvent += StopOrders;
    }
    void StopOrders() {
        orderPhaseActive.Value = false; // Stops delayed orders and order generation chain
        TryTriggerOrderPhaseEnd();
    }

    void TryTriggerOrderPhaseEnd() {
        if (orderPhaseActive.Value) return;

        // Can end Order phase when all docks no longer have active orderers
        bool docksEmpty = true;
        foreach (Dock dock in docks) {
            if (dock.IsOccupied) {
                docksEmpty = false;
            }
        }
        if (!docksEmpty) return;

        GameManager.Instance.NextPhase();
    }

    #region Orderer Management

    void AssignNextOrdererDelayed(Dock openDock, float delay) {
        Util.DoAfterSeconds(this, delay, () => AssignNextOrderer(openDock), orderPhaseActive);
    }
    void AssignNextOrderer(Dock openDock) {
        if (openDock.IsOccupied) {
            Debug.LogError("Unable to assign next orderer: Dock is occupied.");
            return;
        }

        Order order = GenerateOrder();
        Orderer orderer;

        if (order is MoldOrder moldOrder) {
            // Setup MoldOrder Orderer
            orderer = Instantiate(moldOrdererObj, Ref.Instance.OffScreenSpawnTrs).GetComponent<Orderer>();
            ShapeOutlineRenderer shapeOutlineRenderer = orderer.GetComponentInChildren<ShapeOutlineRenderer>();
            moldOrder.Mold.InitByOrderer(orderer.Grid, shapeOutlineRenderer);
        } else {
            orderer = Instantiate(ordererObj, Ref.Instance.OffScreenSpawnTrs).GetComponent<Orderer>();
        }

        orderer.SetOrder(order);
        orderer.OccupyDock(openDock);
    }

    public void HandleFinishedOrderer(Orderer orderer) {
        if (orderPhaseActive.Value) {
            AssignNextOrdererDelayed(orderer.AssignedDock, Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }

        if (orderer.Order.IsFulfilled) {
            GameManager.Instance.ModifyGold(orderer.Order.TotalValue());
            if (PerfectOrders) GameManager.Instance.ModifyGold(perfectOrdersBonus);

            SoundManager.Instance.PlaySound(SoundID.OrderFulfilled);
        } else {
            PerfectOrders = false;
            SoundManager.Instance.PlaySound(SoundID.OrderFailed);
        }

        orderer.Docker.OnReachedEnd += () => {
            TryTriggerOrderPhaseEnd();
            Destroy(orderer.gameObject);
        };
    }

    #endregion

    #region Order Generation

    // Populates backlog of orders
    Order GenerateOrder() {
        Order order = Random.Range(0f, 1f) <= moldChance ?
            GenerateMoldOrder(availableStock, availableColorCells) :
            GenerateBagOrder(availableStock, availableColorCells);

        string a = "";
        foreach (var kv in availableStock) {
            a += $"{kv.Key} | {kv.Value}";
        }
        print(a);

        return order;
        // TODO: handle when no order can be generated?
    }

    Order GenerateBagOrder(Dictionary<ProductID, int> shapeStock, Dictionary<Color, int> colorStock) {
        Order order = new Order(baseOrderTime, timePerProduct, baseOrderValue, valuePerProduct);
        int numReqs = Random.Range(numReqsPerOrder.Min, numReqsPerOrder.Max);

        for (int j = 0; j < numReqs; j++) {
            Requirement req = Random.Range(0f, 1f) <= reqChanceFromExisting ?
                MakeRequirementFromExisting(shapeStock, colorStock) :
                MakeRequirementFromVirtual(shapeStock, colorStock);

            if (req != null) {
                order.AddRequirement(req);
            }
        }

        if (order.Requirements.Count == 0) return null;
        return order;
    }

    Requirement MakeRequirementFromVirtual(Dictionary<ProductID, int> shapeStock, Dictionary<Color, int> colorStock) {
        ShapeDataID reqShapeDataID = Util.GetRandomFromList(reqVirtualShapePool);

        int randomQuantity = Random.Range(reqQuantity.Min, reqQuantity.Max + 1);
        int quantity = randomQuantity;
        int reqShapeSize = ShapeDataLookUp.LookUp(reqShapeDataID).Size;
        int consumedCount = randomQuantity * reqShapeSize;

        // Find colors with enough cell count to create chosen shape at chosen quantity.
        // If no colors have enough cells, scale back count of shape until some colors can support it.
        List<Color> availableColors = new();
        while (availableColors.Count == 0) {
            if (quantity == 0) { // Not enough cells of any color to create req shape even once. Use requirement from existing as backup.
                print("using backup");
                return MakeRequirementFromExisting(shapeStock, colorStock);
            }

            availableColors = colorStock.Where(kv => kv.Value >= consumedCount).Select(kv => kv.Key)
                .ToList();

            if (availableColors.Count > 0) break;

            consumedCount -= reqShapeSize;
            quantity--;
        }

        Color color = Util.GetRandomFromList(availableColors);
        // req.Pattern = Ledger.Instance.PatternPaletteData.Patterns[Random.Range(0, 2)];

        Requirement req = new Requirement(color, null, reqShapeDataID, quantity);

        // Remove stock until enough cells of a color have been accounted for
        List<ProductID> stockIDsOfColor = shapeStock.Where(kv => kv.Key.Color == req.Color).Select(kv => kv.Key).ToList();
        while (consumedCount > 0) {
            if (stockIDsOfColor.Count == 0) {
                Debug.LogError("Expected stock to remove, but no stock remains.");
                break;
            }

            ProductID productID = Util.GetRandomFromList(stockIDsOfColor);
            shapeStock[productID]--;
            consumedCount -= productID.ShapeData.Size;

            if (shapeStock[productID] <= 0) {
                shapeStock.Remove(productID);
                stockIDsOfColor.Remove(productID);
            }

            colorStock[productID.Color] -= productID.ShapeData.Size;
            if (colorStock[productID.Color] <= 0) {
                colorStock.Remove(productID.Color);
            }
        }

        return req;
    }
    Requirement MakeRequirementFromExisting(Dictionary<ProductID, int> shapeStock, Dictionary<Color, int> colorStock) {
        if (shapeStock.Count == 0) {
            Debug.LogWarning("No available stock to generate orders from!");
            return null;
        }

        ProductID productID = Util.GetRandomFromList(shapeStock.Keys.ToList());

        int randomQuantity = Random.Range(reqQuantity.Min, reqQuantity.Max + 1);
        int quantity = Math.Min(randomQuantity, shapeStock[productID]);
        if (quantity == 0) {
            Debug.LogWarning("No available stock to generate orders from!");
            return null;
        }

        Requirement req = new Requirement(productID.Color, productID.Pattern, productID.ShapeDataID, quantity);

        // Equality check is flipped vs. MakeRequirement bc here we overwrite req values with null instead of filling them out
        if (Random.Range(0f, 1f) > reqChanceNeedsColor) { req.Color = null; }

        // if (Random.Range(0f, 1f) > 1) {
        //     req.Pattern = Ledger.Instance.PatternPaletteData.Patterns[Random.Range(0, 2)];
        // }
        // Guarantee at least ShapeDataID is kept
        if (Random.Range(0f, 1f) > reqChanceNeedsShape && (req.Color != null || req.Pattern != null)) { req.ShapeDataID = null; }

        shapeStock[productID] -= quantity;
        if (shapeStock[productID] == 0) shapeStock.Remove(productID);

        colorStock[productID.Color] -= quantity * productID.ShapeData.Size;
        if (colorStock[productID.Color] <= 0) colorStock.Remove(productID.Color);

        return req;
    }

    // Mold order requirement is only a color
    MoldOrder GenerateMoldOrder(Dictionary<ProductID, int> shapeStock, Dictionary<Color, int> colorStock) {
        MoldOrder moldOrder = new MoldOrder(baseOrderTime, timePerProduct, baseOrderValue, valuePerProduct);

        // Generate mold shape
        ShapeDataID moldShapeDataID = Util.GetRandomFromList(moldShapePool);
        ShapeData moldShapeData = ShapeDataLookUp.LookUp(moldShapeDataID);
        moldOrder.AddMold(new Mold(moldShapeData));

        int numReqs = Random.Range(numReqsPerOrder.Min, numReqsPerOrder.Max);
        // Remove rounding if want some orders to possibly be impossible with available stock
        int minColorCount = Mathf.CeilToInt((float) moldShapeData.Size / numReqs);

        for (int j = 0; j < numReqs; j++) {
            // Find available stock with enough quantity to fill mold
            List<Color> availableColors = colorStock.Where(kv => kv.Value > minColorCount).Select(kv => kv.Key)
                .ToList();
            if (availableColors.Count == 0) {
                Debug.LogWarning("Not enough available stock to generate mold order.");
                return null;
            }

            Color reqColor = Util.GetRandomFromList(availableColors);

            // Add requirement
            Requirement req = new Requirement(reqColor, null, null);
            moldOrder.AddRequirement(req);

            // Remove stock until enough cells of a color have been accounted for
            int consumedCount = minColorCount;
            List<ProductID> stockIDsOfColor = shapeStock.Where(kv => kv.Key.Color == req.Color).Select(kv => kv.Key).ToList();
            while (consumedCount > 0) {
                if (stockIDsOfColor.Count == 0) {
                    Debug.LogError("Expected stock to remove, but no stock remains.");
                    break;
                }

                ProductID productID = Util.GetRandomFromList(stockIDsOfColor);
                shapeStock[productID]--;
                consumedCount -= productID.ShapeData.Size;

                if (shapeStock[productID] <= 0) {
                    shapeStock.Remove(productID);
                    stockIDsOfColor.Remove(productID);
                }
                
                colorStock[productID.Color] -= productID.ShapeData.Size;
                if (colorStock[productID.Color] <= 0) {
                    colorStock.Remove(productID.Color);
                }
            }
        }

        if (moldOrder.Requirements.Count == 0) return null;
        return moldOrder;
    }

    #endregion

    public void SetDifficultyOptions(SO_OrdersDifficultyTable.OrderDifficultyEntry orderDiffEntry) {
        numActiveDocks = orderDiffEntry.numActiveDocks;
        baseOrderTime = orderDiffEntry.baseOrderTime;
        baseOrderValue = orderDiffEntry.baseOrderValue;

        numReqsPerOrder.Max = orderDiffEntry.reqMaxNum;
        reqQuantity.Max = orderDiffEntry.reqMaxQuantity;
        reqChanceFromExisting = orderDiffEntry.reqChanceFromExisting;
        reqChanceNeedsColor = orderDiffEntry.reqChanceNeedsColor;
        reqChanceNeedsShape = orderDiffEntry.reqChanceNeedsShape;
        reqVirtualShapePool = new List<ShapeDataID>(orderDiffEntry.reqVirtualShapePool);

        moldChance = orderDiffEntry.moldChance;
        moldShapePool = new List<ShapeDataID>(orderDiffEntry.moldShapePool);
    }
}

public struct ActiveOrderChangedArgs {
    public int ActiveOrderIndex;
    public int NumRemainingOrders;
    public Order NewOrder;
    public bool LastOrderFulfilled;
}