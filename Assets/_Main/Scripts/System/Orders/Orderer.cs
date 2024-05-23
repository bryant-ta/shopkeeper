using System;
using System.Collections.Generic;
using Orders;
using UnityEngine;

public class Orderer : MonoBehaviour {
    public Grid Grid { get; private set; }
    public Order Order { get; private set; }
    public Dock AssignedDock { get; private set; }

    public event Action<Order> OnOrderSet;

    void Awake() {
        Grid = gameObject.GetComponentInChildren<Grid>();

        Grid.OnPlaceShapes += SubmitToOrder;
        Grid.OnRemoveShapes += RemoveFromOrder;
    }

    #region Order

    public void StartOrder() {
        // Order.StartOrder(); // TEMP: currently no timer
        Order.OnOrderFulfilled += OrderFulfilled;
        Order.OnOrderFailed += OrderFailed;
    }

    void SubmitToOrder(List<IGridShape> shapes) {
        foreach (IGridShape shape in shapes) {
            if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                if (Order.Submit(product.ID)) {
                    SoundManager.Instance.PlaySound(SoundID.OrderProductFilled);
                }
            }
        }
    }
    void RemoveFromOrder(List<IGridShape> shapes) {
        foreach (IGridShape shape in shapes) {
            if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                Order.Remove(product.ID);
            }
        }
    }

    void OrderFulfilled() {
        // TODO: some visual for fulfill vs. fail
        LeaveDock();
    }
    void OrderFailed() {
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
        
        List<IGridShape> shapes = Grid.AllShapes();
        foreach (IGridShape shape in shapes) {
            if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                Ledger.RemoveStockedProduct(product);
            }
        }
        
        // TODO: leaving anim
        
        Ref.Instance.OrderMngr.HandleFinishedOrderer(this);
    }

    #endregion

    #region Helper

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }

    #endregion
}