using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using MK.Toon;
using Orders;
using UnityEngine;

[RequireComponent(typeof(HoverEvent), typeof(SplineFollower))]
public class Orderer : MonoBehaviour, IDocker {
    public Order Order { get; private set; }
    public Grid Grid { get; private set; }

    public Dock AssignedDock { get; private set; }
    public SplineFollower Docker { get; private set; }

    public List<Product> SubmittedProducts { get; private set; }

    List<TrailRenderer> trailRenderers = new();

    [SerializeField] Transform body;
    [SerializeField] Transform gridFloor;
    [SerializeField] GameObject gridCellObj;

    public event Action<Order> OnOrderStarted;
    public event Action<Order> OnOrderFinished;
    public event Action<ProductID> OnInvalidProductSet;
    public event Action OnInvalidProductUnset;

    void Awake() {
        Grid = gameObject.GetComponentInChildren<Grid>();
        if (Grid != null) { // is a bag orderer
            Grid.IsLocked = true;
            Grid.OnPlaceShapes += DoTryFulfillOrderList;
            Grid.OnRemoveShapes += RemoveFromOrder;
        }

        HoverEvent he = GetComponent<HoverEvent>();
        he.OnHoverEnter += HoverEnter;
        he.OnHoverExit += HoverExit;

        Docker = GetComponent<SplineFollower>();
        SubmittedProducts = new();

        trailRenderers = GetComponentsInChildren<TrailRenderer>(true).ToList();

        Ref.OrderMngr.OrderPhaseTimer.EndEvent += OrderFailed;
    }

    #region Order

    public void StartOrder() {
        if (Order == null) {
            Debug.LogError("Unable to start order: Order is not set.");
            return;
        }

        if (Grid != null) Grid.IsLocked = false;

        Order.OnOrderSucceeded += OrderSucceeded;
        Order.OnOrderFailed += OrderFailed;

        OnOrderStarted?.Invoke(Order);
    }

    void DoTryFulfillOrderList(List<IGridShape> shapes) { TryFulfillOrder(shapes, true); } // checked already in HoverEnter()
    public bool TryFulfillOrder(List<IGridShape> shapes, bool skipCheck = false) {
        List<Product> products = Util.GetProductsFromShapes(shapes);

        if (!skipCheck && !CheckOrderInput(products, out ProductID invalidProductID)) {
            return false;
        }

        foreach (Product product in products) {
            if (Order.Fulfill(product.ID)) {
                SubmittedProducts.Add(product);
                SoundManager.Instance.PlaySound(SoundID.OrderProductFilled);
            } else {
                Debug.LogError("Invalid product given to Order when check should have prevented this!");
            }
        }

        if (Grid == null) { // Destroy fulfilled product for bag orders (ledger removal occurs when orderer actually leaves dock
            foreach (IGridShape shape in shapes) {
                shape.Grid.DestroyShape(shape);
            }
        }

        if (Order.IsFinished()) {
            Order.Succeed();
        }

        return true;
    }
    public void RemoveFromOrder(List<IGridShape> shapes) {
        foreach (IGridShape shape in shapes) {
            if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                SubmittedProducts.Remove(product);
                Order.Remove(product.ID);
            }
        }
    }

    public void HoverEnter() {
        List<IGridShape> heldShapes = Ref.Player.PlayerDrag.DragGrid.AllShapes();
        List<Product> heldProducts = Util.GetProductsFromShapes(heldShapes);
        if (heldProducts == null) return;

        if (!CheckOrderInput(heldProducts, out ProductID invalidProductID)) {
            OnInvalidProductSet?.Invoke(invalidProductID);

            if (Grid != null) Grid.IsLocked = true;
        }
    }
    void HoverExit() {
        OnInvalidProductUnset?.Invoke();

        if (Grid != null) Grid.IsLocked = false;
    }
    public bool CheckOrderInput(List<Product> products, out ProductID invalidProductID) {
        invalidProductID = new ProductID();

        Dictionary<ProductID, int> productCounts = new();
        foreach (Product product in products) {
            Util.DictIntAdd(productCounts, product.ID, 1);
        }

        HashSet<ProductID> keys = productCounts.Keys.ToHashSet();
        foreach (ProductID id in keys) {
            if (!Order.Check(id, productCounts[id])) {
                invalidProductID = id;
                return false;
            }
        }

        return true;
    }

    void OrderSucceeded() {
        Ref.OrderMngr.OrderPhaseTimer.EndEvent -= OrderFailed;
        OnOrderFinished?.Invoke(Order);
        LeaveDock();
    }
    void OrderFailed() {
        // TODO: game effects of failing an order

        OnOrderFinished?.Invoke(Order);
        LeaveDock();
    }

    public void SetOrder(Order order) {
        if (Order != null) {
            Debug.LogError("Unable to set Order: Order is already set.");
            return;
        }

        Order = order;
        
        // Place grid cell objects according to order layout
        MoldOrder moldOrder = order as MoldOrder;
        if (moldOrder != null) {
            foreach (Vector3Int offset in moldOrder.Mold.ShapeData.ShapeOffsets) {
                MeshRenderer mr = Instantiate(gridCellObj, gridFloor).GetComponent<MeshRenderer>();
                mr.transform.localPosition = offset;
                Properties.albedoColor.SetValue(mr.material, moldOrder.Mold.GridColorRequirements[offset]);
            }
        }

        // Set trail colors by Order requirements
        int trailIndex = 0;
        foreach (Requirement req in Order.Requirements) {
            TrailRenderer tr = trailRenderers[trailIndex];
            tr.gameObject.SetActive(true);
            tr.startColor = req.Color;
            tr.endColor = req.Color;

            trailIndex++;
            if (trailIndex == trailRenderers.Count) break;
        }
    }

    #endregion

    #region Dock

    public void OccupyDock(Dock dock) {
        AssignedDock = dock;
        AssignedDock.SetDocker(Docker);
        Docker.OnReachedEnd += StartOrder; // assumes single path from Occupy -> Dock

        // TEMP: Rotate Orderer body to face correct dir based on dock (does not support curved path, fix if needed)
        if (dock.IsXAligned) { body.Rotate(Vector3.up, 90); }

        Docker.StartFollowing();
    }
    public void LeaveDock() {
        Ref.OrderMngr.HandleFinishedOrderer(this);

        if (Grid != null) Grid.IsLocked = true;

        AssignedDock.RemoveDocker();
        AssignedDock = null;

        foreach (Product product in SubmittedProducts) {
            Ledger.RemoveStockedProduct(product);
        }

        // TODO: leaving anim
        Docker.StartFollowing();
    }

    #endregion

    #region Helper

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }

    #endregion
}