using System;
using System.Collections.Generic;
using System.Linq;
using Orders;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour {
    [Title("Order Queue")]
    [SerializeField] int numActiveOrders;
    [SerializeField] MinMax NextOrderDelay;
    [SerializeField, Range(0f, 1f)] float chanceMoldOrder;

    [Title("Order Parameters")]
    [SerializeField] MinMax numReqsPerOrder;
    [SerializeField] int minTimePerOrder;
    [SerializeField] int timePerProduct;
    [SerializeField] int goldPerProduct;

    [Title("Requirement Paramenters")]
    [SerializeField] MinMax ReqQuantity;
    [Tooltip("Chance to generate a Requirement that pulls from available stock.")]
    [SerializeField, Range(0.5f, 1f)] float chanceReqFromExisting = 0.5f;
    [SerializeField, Range(0f, 1f)] float chanceReqNeedsColor;
    [SerializeField, Range(0f, 1f)] float chanceReqNeedsShape;

    [Title("Orderers")]
    [SerializeField] Transform docksContainer;
    List<Dock> docks;
    [SerializeField] GameObject ordererObj;
    [SerializeField] GameObject moldOrdererObj;

    [Title("Other")]
    [Tooltip("Difficulty Table for requested shapes in requirements.")]
    [SerializeField] ListList<ShapeDataID> reqShapeDifficultyPool;
    [Tooltip("Difficulty Table for mold shapes.")]
    [SerializeField] ListList<ShapeDataID> moldShapeDifficultyPool;

    [field: Title("ReadOnly")]
    [field: SerializeField, ReadOnly] public bool PerfectOrders { get; private set; } // true if all orders for the day are fulfilled

    [field: SerializeField, ReadOnly] public int NumRemainingOrders { get; private set; }

    Dictionary<ProductID, int> availableStock = new();

    Util.ValueRef<bool> isOpenPhase;
    
    

    void Awake() {
        isOpenPhase = new Util.ValueRef<bool>(false);

        docks = docksContainer.GetComponentsInChildren<Dock>().ToList();

        GameManager.Instance.SM_dayPhase.OnStateEnter += EnterStateTrigger;
        GameManager.Instance.SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Open) {
            isOpenPhase.Value = true;
            ScaleOrderDifficulty(GameManager.Instance.Day);
            StartOrders();
        }
    }
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Open) {
            isOpenPhase.Value = false;
            StopOrders();
        }
    }

    void StartOrders() {
        // Stock is taken out from availableStock as they are added to generated orders, avoids repeats with non-existent stock.
        foreach (KeyValuePair<ProductID, List<Product>> kv in Ledger.StockedProducts) {
            availableStock[kv.Key] = kv.Value.Count;
        }

        PerfectOrders = true;

        AssignNextOrderer(docks[0]); // always immediately activate first order
        int activeOrders = Math.Min(numActiveOrders, docks.Count);
        for (var i = 1; i < activeOrders; i++) {
            AssignNextOrdererDelayed(docks[i], Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        }
    }
    void StopOrders() { }

    #region Orderer Management

    void AssignNextOrdererDelayed(Dock openDock, float delay) {
        Util.DoAfterSeconds(this, delay, () => AssignNextOrderer(openDock), isOpenPhase);
    }
    void AssignNextOrderer(Dock openDock) {
        // Prevents delayed active orders from occuring at wrong phase, since ActivateNextOrderDelayed can keep counting after phase end
        // TODO: fix so this isnt needed
        if (GameManager.Instance.CurDayPhase != DayPhase.Open) {
            return;
        }

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

        // TODO: handle when no orders remain gracefully
    }

    public void HandleFinishedOrderer(Orderer orderer) {
        AssignNextOrdererDelayed(orderer.AssignedDock, Random.Range(NextOrderDelay.Min, NextOrderDelay.Max));
        NumRemainingOrders--;

        if (orderer.Order.IsFulfilled) {
            GameManager.Instance.ModifyGold(orderer.Order.TotalValue());
            SoundManager.Instance.PlaySound(SoundID.OrderFulfilled);
        } else {
            PerfectOrders = false;
            SoundManager.Instance.PlaySound(SoundID.OrderFailed);
        }

        orderer.Docker.OnReachedEnd += () => Destroy(orderer.gameObject);
    }

    #endregion

    #region Order Generation

    // Populates backlog of orders
    Order GenerateOrder() {
        return Random.Range(0f, 1f) <= chanceMoldOrder ? GenerateMoldOrder(availableStock) : GenerateBagOrder(availableStock);
        
        // TODO: handle when no order can be generated?
    }

    Order GenerateBagOrder(Dictionary<ProductID, int> stock) {
        Order order = new Order(minTimePerOrder, timePerProduct, goldPerProduct);
        int numReqs = Random.Range(numReqsPerOrder.Min, numReqsPerOrder.Max);

        for (int j = 0; j < numReqs; j++) {
            Requirement req = Random.Range(0f, 1f) <= chanceReqFromExisting ?
                MakeRequirementFromExisting(stock) :
                MakeRequirement();

            if (req != null) {
                order.AddRequirement(req);
            }
        }

        if (order.Requirements.Count == 0) return null;
        return order;
    }

    Requirement MakeRequirement() {
        int quantity = Random.Range(ReqQuantity.Min, ReqQuantity.Max + 1);
        Requirement req = new Requirement(null, null, null, quantity);

        if (Random.Range(0f, 1f) <= chanceReqNeedsColor) {
            List<Color> c = Ledger.Instance.ColorPaletteData.Colors;
            req.Color = c[Random.Range(0, c.Count)];
        }

        // if (Random.Range(0, 2) <= 1) {
        //     req.Pattern = Ledger.Instance.PatternPaletteData.Patterns[Random.Range(0, 2)];
        // }
        // Guarantee at least ShapeDataID is generated
        if (Random.Range(0f, 1f) <= chanceReqNeedsShape || (req.Color == null && req.Pattern == null)) {
            List<ShapeDataID> s = reqShapeDifficultyPool.outerList[0].innerList;
            req.ShapeDataID = s[Random.Range(0, s.Count)];
        }

        return req;
    }
    Requirement MakeRequirementFromExisting(Dictionary<ProductID, int> stock) {
        if (stock.Count == 0) {
            Debug.LogWarning("No available stock to generate orders from!");
            return null;
        }

        ProductID productID = stock.Keys.ToArray()[Random.Range(0, stock.Count)];

        int randomQuantity = Random.Range(ReqQuantity.Min, ReqQuantity.Max + 1);
        int quantity = Math.Min(randomQuantity, stock[productID]);
        if (quantity == 0) {
            Debug.LogWarning("No available stock to generate orders from!");
            return null;
        }

        Requirement req = new Requirement(productID.Color, productID.Pattern, productID.ShapeDataID, quantity);

        // equality check is flipped vs. MakeRequirement bc here we overwrite req values with null instead of filling them out
        if (Random.Range(0f, 1f) > chanceReqNeedsColor) { req.Color = null; }

        // if (Random.Range(0f, 1f) > 1) {
        //     req.Pattern = Ledger.Instance.PatternPaletteData.Patterns[Random.Range(0, 2)];
        // }
        // Guarantee at least ShapeDataID is kept
        if (Random.Range(0f, 1f) > chanceReqNeedsShape && (req.Color != null || req.Pattern != null)) { req.ShapeDataID = null; }

        stock[productID] -= quantity;
        if (stock[productID] == 0) stock.Remove(productID);

        return req;
    }

    // Mold order requirement is only a color
    MoldOrder GenerateMoldOrder(Dictionary<ProductID, int> stock) {
        MoldOrder moldOrder = new MoldOrder(minTimePerOrder, timePerProduct, goldPerProduct);

        // generate mold shape
        List<ShapeDataID> moldShapeIDs = moldShapeDifficultyPool.outerList[0].innerList;
        ShapeDataID moldShapeDataID = moldShapeIDs[Random.Range(0, moldShapeIDs.Count)];
        ShapeData moldShapeData = ShapeDataLookUp.LookUp(moldShapeDataID);
        moldOrder.AddMold(new Mold(moldShapeData));

        int numReqs = Random.Range(numReqsPerOrder.Min, numReqsPerOrder.Max);
        // remove rounding if want some orders to possibly be impossible with available stock
        int minColorCount = Mathf.CeilToInt((float) moldShapeData.Size / numReqs);

        for (int j = 0; j < numReqs; j++) {
            // find available stock with enough quantity to fill mold
            List<Color> availableColors = Ledger.CellCountByColor.Where(kv => kv.Value > minColorCount).Select(kv => kv.Key)
                .ToList();
            if (availableColors.Count == 0) {
                Debug.LogWarning("Not enough available stock to generate mold order.");
                return null;
            }

            Color reqColor = availableColors[Random.Range(0, availableColors.Count)];

            // add requirement
            Requirement req = new Requirement(reqColor, null, null);
            moldOrder.AddRequirement(req);

            // update count of availableStock - subtracts from quantity of first found productID in availableStock that matches req color
            int consumedCount = minColorCount;
            List<ProductID> availableStockIDs = stock.Keys.ToList();
            for (int i = 0; i < availableStockIDs.Count; i++) {
                ProductID productID = availableStockIDs[i];
                if (productID.Color == reqColor) {
                    int t = stock[productID];
                    stock[productID] -= Math.Min(stock[productID], consumedCount);
                    consumedCount -= t;

                    if (stock[productID] == 0) stock.Remove(productID);
                    if (consumedCount == 0) { break; }
                }
            }
        }

        if (moldOrder.Requirements.Count == 0) return null;
        return moldOrder;
    }

    void ScaleOrderDifficulty(int day) {
        if (day > 10) return;

        // numTotalOrders = day / 2 + 3;
        // NumRemainingOrders = numTotalOrders;

        ReqQuantity.Max++;
    }

    #endregion
}

public struct ActiveOrderChangedArgs {
    public int ActiveOrderIndex;
    public int NumRemainingOrders;
    public Order NewOrder;
    public bool LastOrderFulfilled;
}