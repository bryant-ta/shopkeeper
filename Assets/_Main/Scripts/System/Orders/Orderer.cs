using System;
using System.Collections.Generic;
using Orders;
using Paths;
using UnityEngine;

[RequireComponent(typeof(HoverEvent), typeof(PathActor))]
public class Orderer : MonoBehaviour, IDocker {
    public Order Order { get; private set; }
    public Grid Grid { get; private set; }
    
    public Dock AssignedDock { get; private set; }
    public PathActor Docker { get; private set; }

    List<Product> submittedProducts = new();
    
    public event Action<Order> OnOrderStart;
    public event Action<Product> OnInvalidProductSet;

    void Awake() {
        HoverEvent he = GetComponent<HoverEvent>();
        he.OnHoverEnter += HoverEnter;
        he.OnHoverExit += HoverExit;

        Grid = gameObject.GetComponentInChildren<Grid>();
        if (Grid != null) {
            Grid.OnPlaceShapes += DoTryFulfillOrderList;
            Grid.OnRemoveShapes += RemoveFromOrder;
        }

        Docker = GetComponent<PathActor>();
    }

    #region Order

    public void StartOrder() {
        if (Order == null) {
            Debug.LogError("Unable to start order: Order is not set.");
            return;
        }
        
        // Order.StartOrder(); // TEMP: currently no timer
        Order.OnOrderSucceeded += OrderSucceeded;
        Order.OnOrderFailed += OrderFailed;
        
        OnOrderStart?.Invoke(Order);
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
                submittedProducts.Remove(product);
                Order.Remove(product.ID);
            }
        }
    }

    public void HoverEnter() {
        List<IGridShape> heldShapes = Ref.Player.PlayerDrag.DragGrid.AllShapes();
        List<Product> heldProducts = Util.GetProductsFromShapes(heldShapes);

        if (!CheckOrderInput(heldProducts, out Product invalidProduct)) {
            OnInvalidProductSet?.Invoke(invalidProduct);

            if (Grid != null) {
                Grid.IsLocked = true;
            }
        }
    }
    void HoverExit() {
        OnInvalidProductSet?.Invoke(null);

        if (Grid != null) {
            Grid.IsLocked = false;
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
    }

    #endregion

    #region Dock

    public void OccupyDock(Dock dock) {
        AssignedDock = dock; // do not unset, OrderManager uses ref
        AssignedDock.SetDocker(Docker);
        Docker.OnPathEnd += StartOrder; // assumes single path from Occupy -> Dock

        Docker.StartPath(0);
    }
    public void LeaveDock() {
        AssignedDock.RemoveDocker();

        foreach (Product product in submittedProducts) {
            Ledger.RemoveStockedProduct(product);
        }

        // TODO: leaving anim
        Docker.StartNextPath();

        Ref.Instance.OrderMngr.HandleFinishedOrderer(this);
    }

    #endregion

    #region Helper

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }

    #endregion
}