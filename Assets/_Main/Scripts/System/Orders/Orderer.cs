using System;
using System.Collections.Generic;
using Dreamteck.Splines;
using Orders;
using UnityEngine;

[RequireComponent(typeof(HoverEvent), typeof(SplineFollower))]
public class Orderer : MonoBehaviour, IDocker {
    public Order Order { get; private set; }
    public Grid Grid { get; private set; }

    public Dock AssignedDock { get; private set; }
    public SplineFollower Docker { get; private set; }

    public List<Product> SubmittedProducts { get; private set; }

    public event Action<Order> OnOrderStarted;
    public event Action<Order> OnOrderFinished;
    public event Action<Product> OnInvalidProductSet;

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
    }

    #region Order

    public void StartOrder() {
        if (Order == null) {
            Debug.LogError("Unable to start order: Order is not set.");
            return;
        }

        if (Grid != null) Grid.IsLocked = false;

        // Order.StartOrder(); // TEMP: currently no timer
        Order.OnOrderSucceeded += OrderSucceeded;
        Order.OnOrderFailed += OrderFailed;

        OnOrderStarted?.Invoke(Order);
    }

    void DoTryFulfillOrderList(List<IGridShape> shapes) { TryFulfillOrder(shapes, true); } // checked already in HoverEnter()
    public bool TryFulfillOrder(List<IGridShape> shapes, bool skipCheck = false) {
        List<Product> products = Util.GetProductsFromShapes(shapes);

        if (!skipCheck && !CheckOrderInput(products, out Product invalidProduct)) {
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

        if (Grid == null) { // Destroy fulfilled product for bag orders
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
                SubmittedProducts.Remove(product);
                Order.Remove(product.ID);
            }
        }
    }

    public void HoverEnter() {
        List<IGridShape> heldShapes = Ref.Player.PlayerDrag.DragGrid.AllShapes();
        List<Product> heldProducts = Util.GetProductsFromShapes(heldShapes);
        if (heldProducts == null) return;

        if (!CheckOrderInput(heldProducts, out Product invalidProduct)) {
            OnInvalidProductSet?.Invoke(invalidProduct);

            if (Grid != null) Grid.IsLocked = true;
        }
    }
    void HoverExit() {
        OnInvalidProductSet?.Invoke(null);

        if (Grid != null) Grid.IsLocked = false;
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

    void OrderSucceeded() {
        // TODO: some visual for fulfill vs. fail
        OnOrderFinished?.Invoke(Order);
        LeaveDock();
    }
    void OrderFailed() {
        // TODO: game effects of failing an order

        // TODO: some visual for fulfill vs. fail
        OnOrderFinished?.Invoke(Order);
        LeaveDock();
    }

    public void SetOrder(Order order) {
        if (Order != null) {
            Debug.LogError("Unable to set Order: Order is already set.");
            return;
        }

        Order = order;
    }

    #endregion

    #region Dock

    public void OccupyDock(Dock dock) {
        AssignedDock = dock;
        AssignedDock.SetDocker(Docker);
        Docker.OnReachedEnd += StartOrder; // assumes single path from Occupy -> Dock

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