using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using MK.Toon;
using Orders;
using UnityEngine;
using Random = UnityEngine.Random;

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
    [SerializeField] Transform gridCellObjContainer;

    [SerializeField] GameObject gridCellObj;

    public event Action<Order> OnOrderStarted;
    public event Action<Order> OnOrderFinished;

    void Awake() {
        Grid = gameObject.GetComponentInChildren<Grid>();
        Grid.IsLocked = true;
        Grid.OnPlaceShapes += DoFulfillOrder;
        Grid.OnRemoveShapes += RemoveFromOrder;

        HoverEvent he = GetComponent<HoverEvent>();
        he.OnHoverEnter += HoverEnter;
        he.OnHoverExit += HoverExit;

        Docker = GetComponent<SplineFollower>();
        SubmittedProducts = new();

        trailRenderers = GetComponentsInChildren<TrailRenderer>(true).ToList();

        GameManager.Instance.RunTimer.EndEvent += OrderFailed;
    }
    
    public void HoverEnter() {
        // TODO: reveal grid floor objects effect
    }
    void HoverExit() { }

    #region Order

    public void StartOrder() {
        if (Order == null) {
            Debug.LogError("Unable to start order: Order is not set.");
            return;
        }

        Grid.IsLocked = false;

        OnOrderStarted?.Invoke(Order);
    }

    void DoFulfillOrder(List<IGridShape> shapes) { FulfillOrder(shapes, true); } // checked already in HoverEnter()
    public void FulfillOrder(List<IGridShape> shapes, bool skipCheck = false) {
        List<Product> products = Util.GetProductsFromShapes(shapes);

        foreach (Product product in products) {
            if (Order.Fulfill(product.ID)) {
                SubmittedProducts.Add(product);
                SoundManager.Instance.PlaySound(SoundID.OrderProductFilled);
            } else {
                Debug.LogError("Invalid product given to Order when check should have prevented this!");
            }
        }

        if (Grid.IsAllFull()) {
            Order.Succeed();
            OrderSucceeded();
        }
    }
    public void RemoveFromOrder(List<IGridShape> shapes) {
        foreach (IGridShape shape in shapes) {
            if (shape.ColliderTransform.TryGetComponent(out Product product)) {
                SubmittedProducts.Remove(product);
            }
        }
    }

    public bool CheckOrderInput(List<IGridShape> shapes, Vector3Int coord, out List<IGridShape> invalidShapes) {
        if (shapes == null || shapes.Count == 0) {
            Debug.LogError("Unable to check order input: shapes input is null or empty.");
            invalidShapes = null;
            return false;
        }

        invalidShapes = new List<IGridShape>();

        if (coord.y > 0) return false;
        foreach (IGridShape shape in shapes) {
            if (shape.ShapeData.RootCoord.y > 0) {
                invalidShapes.Add(shape);
            }
        }
        if (invalidShapes.Count > 0) return false;

        // Check input shape offsets/colors at placed coords in orderer grid
        List<Product> products = Util.GetProductsFromShapes(shapes);
        foreach (Product product in products) {
            foreach (Vector3Int offset in product.ShapeData.ShapeOffsets) {
                Vector3Int curCoord = coord + product.ShapeData.RootCoord + offset;
                if (Order.GridColors.TryGetValue(curCoord, out Color color) && 
                    (product.ID.Color == color || color == Ledger.Instance.WildColor)) {
                    continue;
                }
                invalidShapes.Add(product);
            }
        }
        if (invalidShapes.Count > 0) return false;

        return true;
    }

    void OrderSucceeded() {
        GameManager.Instance.RunTimer.EndEvent -= OrderFailed;
        OnOrderFinished?.Invoke(Order);
        LeaveDock();
    }
    void OrderFailed() {
        // TODO: game effects of failing an order

        GameManager.Instance.RunTimer.EndEvent -= OrderFailed;
        OnOrderFinished?.Invoke(Order);
        LeaveDock();
    }

    public void AssignOrder(Order order) {
        if (Order != null) {
            Debug.LogError("Unable to set Order: Order is already set.");
            return;
        }

        Order = order;
        ShapeData orderShapeData = Order.ShapeData;

        // Rotate order layout randomly
        int cwRandomRotationTimes = Random.Range(0, 4);
        List<Vector3Int> oldShapeOffsets = orderShapeData.ShapeOffsets;
        for (int i = 0; i < cwRandomRotationTimes; i++) {
            orderShapeData.RotateShape(true);
        }
        Dictionary<Vector3Int, Color> rotatedGridColors = new();
        for (int i = 0; i < orderShapeData.ShapeOffsets.Count; i++) {
            rotatedGridColors.Add(orderShapeData.ShapeOffsets[i], Order.GridColors[oldShapeOffsets[i]]);
        }
        Order.GridColors = rotatedGridColors;

        // Place grid cell objects according to order layout
        foreach (Vector3Int offset in Order.ShapeData.ShapeOffsets) {
            MeshRenderer mr = Instantiate(gridCellObj, gridCellObjContainer).GetComponent<MeshRenderer>();
            mr.transform.localPosition = offset;
            Properties.albedoColor.SetValue(mr.material, Order.GridColors[offset]);
        }

        // NOTE: keep ahead of setting shape data root coord!
        Grid.SetGridSize(orderShapeData.Length, orderShapeData.Height, orderShapeData.Width);
        // TEMP: scale orderer floor grid, replaced after orderer prefabs for different sizes
        gridFloor.localScale = new Vector3(orderShapeData.Length, orderShapeData.Width, 1);

        // Remove grid cells to match shape data
        List<Vector2Int> cells = new();
        foreach (Vector3Int offset in orderShapeData.ShapeOffsets) {
            cells.Add(new Vector2Int(orderShapeData.RootCoord.x, orderShapeData.RootCoord.z) + new Vector2Int(offset.x, offset.z));
        }
        List<Vector2Int> invertedcells = Grid.ValidCells.Except(cells).ToList();
        foreach (Vector2Int coord in invertedcells) {
            Grid.RemoveValidCell(coord);
        }

        // Set trail colors by Order requirements
        int trailIndex = 0;
        HashSet<Color> orderColors = Order.GetColors();
        foreach (Color color in orderColors) {
            TrailRenderer tr = trailRenderers[trailIndex];
            tr.gameObject.SetActive(true);
            tr.startColor = color;
            tr.endColor = color;

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
}