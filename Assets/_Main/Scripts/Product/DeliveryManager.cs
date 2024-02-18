using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour {
    [SerializeField] int numProductsInDelivery;

    [SerializeField] List<ProductID> initialPossibleProducts;

    [Header("Zone")]
    [SerializeField] Vector3Int deliveryZoneDimensions;
    [SerializeField] Zone deliveryZone;
    Grid grid;

    RollTable<ProductID> productRollTable = new();

    void Awake() {
        GameManager.Instance.SM_dayPhase.OnStateEnter += StateTrigger;
    }

    void Start() {
        grid = GameManager.WorldGrid;

        // Create delivery zone
        ZoneProperties deliveryZoneProps = new ZoneProperties() {CanPlace = false, CanTake = true};
        deliveryZone.Setup(Vector3Int.RoundToInt(transform.localPosition), deliveryZoneDimensions, deliveryZoneProps);
        grid.AddZone(deliveryZone);

        // Load initial possible products
        foreach (ProductID productID in initialPossibleProducts) {
            AddPossibleProduct(productID);
        }
    }

    void StateTrigger(IState<DayPhase> state) { if (state.ID == DayPhase.Delivery) DoDelivery(); }
    void DoDelivery() {
        // Place products starting from (0, 0, 0) within deliveryZone
        // Order of placement is one product on next open y of (x, z), then next (x, z)
        int numProductsDelivered = 0;
        while (numProductsDelivered < numProductsInDelivery) {
            int prodsDeliveredLastCycle = numProductsDelivered;

            for (int x = 0; x < deliveryZone.Length; x++) {
                for (int z = 0; z < deliveryZone.Width; z++) {
                    Vector3Int deliveryCoord;
                    if (grid.SelectLowestOpen(deliveryZone.RootCoord.x + x, deliveryZone.RootCoord.z + z, out int y)) {
                        deliveryCoord = deliveryZone.RootCoord + new Vector3Int(x, y, z);
                    }
                    else { // this xz coord has no free cells
                        continue;
                    }

                    SO_Product p = ProductFactory.Instance.ProductLookUp[productRollTable.GetRandom()];
                    Product product = ProductFactory.Instance.CreateProduct(p);

                    if (product.TryGetComponent(out IGridShape shape)) {
                        if (!grid.PlaceShape(deliveryCoord, shape, true)) {
                            Debug.LogErrorFormat("Unable to place shape at {0} in delivery zone", deliveryCoord);
                        }

                        GameManager.AddStockedProduct(product);
                    }
                    else {
                        Debug.LogErrorFormat("Unable to deliver product {0}: product has no grid shape.", product.Name);
                        return;
                    }

                    numProductsDelivered++;
                    if (numProductsDelivered == numProductsInDelivery) return;
                }
            }

            // Did not finish delivering target number of products
            if (prodsDeliveredLastCycle == numProductsDelivered) {
                Debug.LogWarning("Unable to deliver all products: delivery zone is full.");
                return;
            }
        }
    }

    public void AddPossibleProduct(ProductID productID) {
        if (!productRollTable.Contains(productID)) {
            productRollTable.Add(productID, 1);
        }
    }
}