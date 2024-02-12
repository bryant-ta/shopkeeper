using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour {
    [SerializeField] int numProductsInDelivery;
    [SerializeField] Transform deliveryZoneRootCoord; // TEMP: replace when attached to delivery structure
    [SerializeField] Vector3Int deliveryZoneDimensions;

    [SerializeField] List<ProductID> initialPossibleProducts;
    
    Zone deliveryZone;
    Grid grid;
    RollTable<ProductID> productRollTable = new();

    void Start() {
        grid = GameManager.WorldGrid;

        // Create delivery zone
        ZoneProperties deliveryZoneProps = new ZoneProperties() {CanPlace = false, CanTake = true};
        deliveryZone = new Zone(Vector3Int.RoundToInt(deliveryZoneRootCoord.localPosition), 
            deliveryZoneDimensions.x, deliveryZoneDimensions.y, deliveryZoneDimensions.z, deliveryZoneProps);
        grid.AddZone(deliveryZone);

        // Load initial posssible products
        foreach (ProductID productID in initialPossibleProducts) {
            AddPossibleProduct(productID);
        }
        
        DoDelivery();
    }

    public void DoDelivery() {
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
                    } else { // this xz coord has no free cells
                        continue;
                    }

                    SO_Product p = ProductFactory.Instance.ProductLookUp[productRollTable.GetRandom()];
                    Product product = ProductFactory.Instance.CreateProduct(p);
                    
                    if (product.TryGetComponent(out IGridShape shape)) {
                        if (!grid.PlaceShape(deliveryCoord, shape, true)) {
                            Debug.LogErrorFormat("Unable to place shape at {0} in delivery zone", deliveryCoord);
                        }
                        
                        GameManager.AddStockedProduct(product);
                    } else {
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