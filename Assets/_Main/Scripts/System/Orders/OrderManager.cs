using System;
using System.Collections.Generic;
using System.Linq;
using Orders;
using Timers;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [SerializeField] int numNeedOrdersFulfilled;
    [SerializeField] int numOrdersFulfilled;
    public bool MetQuota => numOrdersFulfilled >= numNeedOrdersFulfilled;
    public event Action<int, int> OnIncOrderFulfilled;

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

    Dictionary<Color, int> availableColorStock = new(); // Count of cells by color

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
        availableColorStock = new(Ledger.CellCountByColor);

        numOrdersFulfilled = 0;
        OnIncOrderFulfilled?.Invoke(0, numNeedOrdersFulfilled);

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
        if (orderPhaseActive.Value || GameManager.Instance.CurDayPhase != DayPhase.Order) return;

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
        if (order == null) {
            return;
        }

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
            // Catch up stock for color cell based orders, now that actual fulfillment shape is determined
            // Resolve difference between actual submitted vs. reserved color cells for mold orders
            if (orderer.Order is MoldOrder moldOrder) {
                Dictionary<Color, int> orderColorStock = new();
                foreach (Product product in orderer.SubmittedProducts) {
                    Color c = product.ID.Color;
                    ShapeDataID s = product.ID.ShapeDataID;
                    Util.DictIntAdd(orderColorStock, c, ShapeDataLookUp.LookUp(s).Size);
                }

                foreach (Color color in orderColorStock.Keys.ToList()) {
                    Util.DictIntAdd(
                        availableColorStock, color,
                        CalculateMoldMinColorCount(moldOrder.Mold.ShapeData.Size, moldOrder.Requirements.Count) - orderColorStock[color]
                    );
                }
            }

            GameManager.Instance.ModifyGold(orderer.Order.TotalValue());

            if (PerfectOrders) GameManager.Instance.ModifyGold(perfectOrdersBonus);

            numOrdersFulfilled++;
            OnIncOrderFulfilled?.Invoke(numOrdersFulfilled, numNeedOrdersFulfilled);

            SoundManager.Instance.PlaySound(SoundID.OrderFulfilled);
        } else {
            // Release color cells since order was not fulfilled
            if (orderer.Order is MoldOrder moldOrder) {
                foreach (Requirement req in moldOrder.Requirements) {
                    if (req.Color == null) {
                        Debug.LogError("Unable to release color stock: Expected mold order requirement to have color.");
                        continue;
                    }

                    Color c = req.Color;
                    Util.DictIntAdd(
                        availableColorStock, c, CalculateMoldMinColorCount(moldOrder.Mold.ShapeData.Size, moldOrder.Requirements.Count)
                    );
                }
            } else {
                foreach (Requirement req in orderer.Order.Requirements) {
                    // Req had no color, so color stock was not reserved before, nothing to do.
                    if (req.Color == null || req.ShapeDataID == null) continue;
                    Color c = req.Color;
                    ShapeDataID s = req.ShapeDataID ?? ShapeDataID.Custom;

                    Util.DictIntAdd(availableColorStock, c, req.TargetQuantity * ShapeDataLookUp.LookUp(s).Size);
                }
            }

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

    Order GenerateOrder() {
        Order order = Random.Range(0f, 1f) <= moldChance ? GenerateMoldOrder() : GenerateBagOrder();
        if (order == null) {
            Debug.Log("Did not generate order.");
        }

        return order;
    }

    Order GenerateBagOrder() {
        Order order = new Order(baseOrderTime, timePerProduct, baseOrderValue, valuePerProduct);
        int numReqs = Random.Range(numReqsPerOrder.Min, numReqsPerOrder.Max);

        for (int j = 0; j < numReqs; j++) {
            Requirement req = MakeRequirementFromVirtual(order.GetColors());
            if (req != null) {
                order.AddRequirement(req);
            }
        }

        if (order.Requirements.Count == 0) return null;
        return order;
    }

    Requirement MakeRequirementFromVirtual(List<Color> excludedColors) {
        ShapeDataID reqShapeDataID = Util.GetRandomFromList(reqVirtualShapePool);

        int randomQuantity = Random.Range(reqQuantity.Min, reqQuantity.Max + 1);
        int quantity = randomQuantity;
        int reqShapeSize = ShapeDataLookUp.LookUp(reqShapeDataID).Size;
        int consumedCount = randomQuantity * reqShapeSize;

        // Remove excluded colors
        HashSet<Color> filteredColors = availableColorStock.Keys.Except(excludedColors).ToHashSet();
        Dictionary<Color, int> colorStock = availableColorStock.Where(kv => filteredColors.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        // Find colors with enough cell count to create chosen shape at chosen quantity.
        // If no colors have enough cells, scale back count of shape until some colors can support it.
        List<Color> availableColors = new();
        while (availableColors.Count == 0) {
            if (quantity == 0) { // Not enough cells of any color to create req shape even once. Use 1x1 as backup.
                if (colorStock.Keys.Count == 0) {
                    Debug.LogWarning("Unable to generate virtual requirement: out of stock.");
                    return null;
                }

                Color c = Util.GetRandomFromList(colorStock.Keys.ToList());
                quantity = Math.Min(colorStock[c], randomQuantity);

                availableColorStock[c] -= quantity;
                if (availableColorStock[c] <= 0) { availableColorStock.Remove(c); }

                return new Requirement(c, null, ShapeDataID.O1, quantity);
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

        availableColorStock[color] -= consumedCount;
        if (availableColorStock[color] <= 0) { availableColorStock.Remove(color); }

        return req;
    }

    // Mold order requirement is only a color
    MoldOrder GenerateMoldOrder() {
        MoldOrder moldOrder = new MoldOrder(baseOrderTime, timePerProduct, baseOrderValue, valuePerProduct);

        // Generate mold shape
        ShapeDataID moldShapeDataID = Util.GetRandomFromList(moldShapePool);
        ShapeData moldShapeData = ShapeDataLookUp.LookUp(moldShapeDataID);
        moldOrder.AddMold(new Mold(moldShapeData));

        int numReqs = Random.Range(numReqsPerOrder.Min, numReqsPerOrder.Max);
        // Remove rounding if want some orders to possibly be impossible with available stock
        int minColorCount = CalculateMoldMinColorCount(moldShapeData.Size, numReqs);
        
        // Find available stock with enough quantity to fill mold
        List<Color> availableColors = availableColorStock.Where(kv => kv.Value > minColorCount).Select(kv => kv.Key)
            .ToList();
        
        for (int j = 0; j < numReqs; j++) {
            if (availableColors.Count == 0) break;

            Color color = Util.GetRandomFromList(availableColors);

            // Add requirement
            Requirement req = new Requirement(color, null, null);
            moldOrder.AddRequirement(req);

            availableColors.Remove(color);
            
            availableColorStock[color] -= minColorCount;
            if (availableColorStock[color] <= 0) { availableColorStock.Remove(color); }
        }

        if (moldOrder.Requirements.Count == 0) {
            Debug.LogWarning("Not enough available stock to generate mold order.");
            return null;
        }
        return moldOrder;
    }
    int CalculateMoldMinColorCount(float moldSize, int numReqs) { return Mathf.CeilToInt(moldSize / numReqs); }

    #endregion

    public void SetDifficultyOptions(SO_OrdersDifficultyTable.OrderDifficultyEntry orderDiffEntry) {
        numNeedOrdersFulfilled = orderDiffEntry.numNeedOrdersFulfilled;
        numActiveDocks = orderDiffEntry.numActiveDocks;
        baseOrderTime = orderDiffEntry.baseOrderTime;
        baseOrderValue = orderDiffEntry.baseOrderValue;

        numReqsPerOrder.Max = orderDiffEntry.reqMaxNum;
        reqQuantity.Max = orderDiffEntry.reqMaxQuantity;
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