using System;
using System.Collections.Generic;
using Orders;
using UnityEngine;

[RequireComponent(typeof(HoverEvent))]
public class Orderer : MonoBehaviour {
    public Order Order { get; private set; }
    public Dock AssignedDock { get; private set; }

    Grid grid;

    List<Product> submittedProducts = new();

    public event Action<Order> OnOrderSet;

    void Awake() {
        HoverEvent he = GetComponent<HoverEvent>();
        he.OnHoverEnter += HoverEnter;
        he.OnHoverExit += HoverExit;

        grid = gameObject.GetComponentInChildren<Grid>();
        if (grid != null) {
            grid.OnPlaceShapes += DoTryFulfillOrderList;
            grid.OnRemoveShapes += RemoveFromOrder;
        }
    }

    #region Order

    public void StartOrder() {
        // Order.StartOrder(); // TEMP: currently no timer
        Order.OnOrderSucceeded += OrderSucceeded;
        Order.OnOrderFailed += OrderFailed;
    }

    void DoTryFulfillOrderList(List<IGridShape> shapes) { TryFulfillOrder(shapes, true); } // checked already in HoverEnter()
    public bool TryFulfillOrder(List<IGridShape> shapes, bool skipCheck = false) {
        List<Product> products = Util.GetProductsFromShapes(shapes);

        if (!skipCheck && !CheckOrderInput(products, out Product invalidProduct)) {
            return false;
        }

        foreach (Product product in products) {
            if (Order.Fulfill(product.ID)) {
                submittedProducts.Add(product);
                SoundManager.Instance.PlaySound(SoundID.OrderProductFilled);
            } else {
                Debug.LogError("Invalid product given to Order when check should have prevented this!");
            }
        }

        if (grid != null) { // Destroy fulfilled product for bag orders
            Ref.Instance.Trash.TrashShapes(shapes, Ref.Player.PlayerDrag.DragGrid);
        }

        if (Order.IsFinished()) {
            Order.Succeed();
        }

        return true;
    }
    public void RemoveFromOrder(List<IGridShape> shapes) {
        foreach (IGridShape shape in shapes) {
            if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                submittedProducts.Remove(product);
                Order.Remove(product.ID);
            }
        }
    }

    public void HoverEnter() {
        List<IGridShape> heldShapes = Ref.Player.PlayerDrag.DragGrid.AllShapes();
        List<Product> heldProducts = Util.GetProductsFromShapes(heldShapes);

        if (!CheckOrderInput(heldProducts, out Product invalidProduct)) {
            // TODO: display invalid product that is currently in held shapes

            if (grid != null) {
                grid.IsLocked = true;
            }
        }
    }
    public bool CheckOrderInput(List<Product> products, out Product invalidProduct) {
        invalidProduct = null;
        foreach (Product product in products) {
            if (!Order.Check(product.ID)) {
                invalidProduct = product;
                return false;
            }
        }

        return true;
    }
    void HoverExit() {
        // TODO: undisplay invalid product

        if (grid != null) {
            grid.IsLocked = false;
        }
    }

    void OrderSucceeded() {
        // TODO: some visual for fulfill vs. fail
        LeaveDock();
    }
    void OrderFailed() {
        // TODO: game effects of failing an order

        // TODO: some visual for fulfill vs. fail
        LeaveDock();
    }

    public void SetOrder(Order order) {
        if (Order != null) {
            Debug.LogError("Unable to set Order: Order is already set.");
            return;
        }

        Order = order;
        OnOrderSet?.Invoke(Order);
    }

    #endregion

    #region Dock

    public void OccupyDock(Dock dock) {
        AssignedDock = dock; // do not unset, OrderManager uses ref
        AssignedDock.SetOrderer(this);
        transform.position = dock.GetDockingPoint(); // TEMP: until anim
        StartOrder();
    }
    void LeaveDock() {
        AssignedDock.RemoveOrderer();

        foreach (Product product in submittedProducts) {
            Ledger.RemoveStockedProduct(product);
        }

        // TODO: throwing away bad submissions

        // TODO: leaving anim

        Ref.Instance.OrderMngr.HandleFinishedOrderer(this);
    }

    #endregion

    #region Helper

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }

    #endregion
}