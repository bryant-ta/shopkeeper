using System;
using System.Collections.Generic;
using Orders;
using UnityEngine;

public class Orderer : MonoBehaviour {
    public Order Order { get; private set; }
    public Dock AssignedDock { get; private set; }

    [SerializeField] PhysicalButton submitButton;
    [SerializeField] PhysicalButton rejectButton;

    Grid grid;

    List<Product> submittedProducts = new();

    public event Action<Order> OnOrderSet;

    void Awake() {
        submitButton.OnInteract += SubmitOrder;
        rejectButton.OnInteract += RejectOrder;
        
        grid = gameObject.GetComponentInChildren<Grid>();
        if (grid != null) {
            grid.OnPlaceShapes += TryFulfillOrder;
            grid.OnRemoveShapes += RemoveFromOrder;
        }
    }

    #region Order

    public void StartOrder() {
        // Order.StartOrder(); // TEMP: currently no timer
        Order.OnOrderSucceeded += OrderSucceeded;
        Order.OnOrderFailed += OrderFailed;
    }

    public void TryFulfillOrder(List<IGridShape> shapes) {
        foreach (IGridShape shape in shapes) {
            if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                if (Order.TryFulfill(product.ID)) {
                    SoundManager.Instance.PlaySound(SoundID.OrderProductFilled);
                }

                submittedProducts.Add(product);
            }
        }
    }
    public void RemoveFromOrder(List<IGridShape> shapes) {
        foreach (IGridShape shape in shapes) {
            if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                submittedProducts.Remove(product);
                Order.Remove(product.ID);
            }
        }
    }

    void SubmitOrder() { Order.Submit(); }
    void RejectOrder() { Order.Reject(); }

    void OrderSucceeded() {
        // TODO: some visual for fulfill vs. fail
        LeaveDock();
    }
    void OrderFailed() { LeaveDock(); }

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