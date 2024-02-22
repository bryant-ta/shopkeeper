using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour {
    [SerializeField] int numProductsInDelivery;
    [SerializeField, Min(1)] int minGroupQuantity = 1;
    [SerializeField, Min(1)] int maxGroupQuantity = 1;

    [SerializeField] List<ProductID> initialPossibleProducts;
    
    [SerializeField] Transform productSpawnPosition; // TEMP

    [Header("Zone")]
    [SerializeField] Vector3Int deliveryZoneDimensions;
    [SerializeField] Zone deliveryZone;
    Grid grid;

    RollTable<ProductID> productRollTable = new();

    void Awake() { GameManager.Instance.SM_dayPhase.OnStateEnter += StateTrigger; }

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

    void StateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) StartCoroutine(DoDelivery());
    }
    IEnumerator DoDelivery() {
        // Place products starting from (0, 0, 0) within deliveryZone
        // Order of placement is one product on next open y of (x, z), then next (x, z)
        int numProductsDelivered = 0;
        while (numProductsDelivered < numProductsInDelivery) {
            SO_Product productData = null;
            int groupQuantity = 0;
            int prodsDeliveredLastCycle = numProductsDelivered;

            for (int x = 0; x < deliveryZone.Length; x++) {
                for (int z = 0; z < deliveryZone.Width; z++) {
                    Vector3Int deliveryCoord;
                    if (grid.SelectLowestOpen(deliveryZone.RootCoord.x + x, deliveryZone.RootCoord.z + z, out int y)) {
                        deliveryCoord = deliveryZone.RootCoord + new Vector3Int(x, y, z);
                    } else { // this xz coord has no free cells
                        continue;
                    }

                    if (groupQuantity == 0) { // finished a group, generate new group with random product
                        productData = ProductFactory.Instance.ProductLookUp[productRollTable.GetRandom()];
                        groupQuantity = Random.Range(minGroupQuantity, maxGroupQuantity + 1);
                    }
                    groupQuantity--;
                    
                    Product product = ProductFactory.Instance.CreateProduct(productData);

                    if (product.TryGetComponent(out IGridShape shape)) {
                        shape.ShapeTransform.position = productSpawnPosition.position;
                        if (!grid.PlaceShape(deliveryCoord, shape, true)) {
                            Debug.LogErrorFormat("Unable to place shape at {0} in delivery zone", deliveryCoord);
                        }

                        GameManager.AddStockedProduct(product);
                        
                        // Stagger delivery anim
                        yield return new WaitForSeconds(Constants.AnimIndividualDeliveryDelay);
                    }
                    else {
                        Debug.LogErrorFormat("Unable to deliver product {0}: product has no grid shape.", product.Name);
                        yield break;
                    }

                    numProductsDelivered++;
                    if (numProductsDelivered == numProductsInDelivery) yield break;
                }
            }

            // Did not finish delivering target number of products
            if (prodsDeliveredLastCycle == numProductsDelivered) {
                Debug.LogWarning("Unable to deliver all products: delivery zone is full.");
                yield break;
            }
        }
    }

    public void AddPossibleProduct(ProductID productID) {
        if (!productRollTable.Contains(productID)) {
            productRollTable.Add(productID, 1);
        }
    }
}