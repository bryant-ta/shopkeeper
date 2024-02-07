using UnityEngine;

public class DeliveryManager : MonoBehaviour {
    [SerializeField] int numProductsInDelivery;
    [SerializeField] Transform deliveryZoneRootCoord; // TEMP: replace when attached to delivery structure

    Zone deliveryZone;
    Grid grid;

    void Start() {
        grid = GameManager.WorldGrid;

        // Create delivery zone
        ZoneProperties deliveryZoneProps = new ZoneProperties() {CanPlace = false, CanTake = true};
        deliveryZone = new Zone(Vector3Int.RoundToInt(deliveryZoneRootCoord.localPosition), 5, 5, 5, deliveryZoneProps);
        grid.AddZone(deliveryZone);

        DoDelivery();
    }

    public void DoDelivery() {
        // Place products starting from (0, 0, 0) within deliveryZone
        // Order of placement is one product on next open y of (x, z), then next (x, z)
        int numProductsDelivered = 0;
        while (numProductsDelivered < numProductsInDelivery) {
            int prodsDeliveredSoFar = numProductsDelivered;

            for (int x = 0; x < deliveryZone.Length; x++) {
                for (int z = 0; z < deliveryZone.Width; z++) {
                    Vector3Int deliveryCoord;
                    if (grid.SelectLowestOpen(x, z, out int y)) {
                        deliveryCoord = deliveryZone.RootCoord + new Vector3Int(x, y, z);
                    } else { // this xz coord has no free cells
                        continue;
                    }

                    SO_Product p = ProductFactory.Instance.ProductLookUp[ProductID.Blank]; // TEMP
                    Product product = ProductFactory.Instance.CreateProduct(p);
                    if (product.TryGetComponent(out IGridShape shape)) {
                        if (!grid.PlaceShape(deliveryCoord, shape, true)) {
                            Debug.LogErrorFormat("Unable to place shape at {0} in delivery zone", deliveryCoord);
                        }
                    } else {
                        Debug.LogErrorFormat("Unable to deliver product {0}: product has no grid shape.", product.Name);
                        return;
                    }

                    numProductsDelivered++;
                    if (numProductsDelivered == numProductsInDelivery) return;
                }
            }

            // Did not finish delivering target number of products
            if (prodsDeliveredSoFar == numProductsDelivered) {
                Debug.LogWarning("Unable to deliver all products: delivery zone is full.");
                return;
            }
        }
    }
}