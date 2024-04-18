using System.Collections;
using System.Collections.Generic;
using Tags;
using UnityEngine;

public class DeliveryManager : MonoBehaviour {
    [SerializeField] int numInitialProductsInDelivery;
    [SerializeField] int productsPerDayGrowth;
    [SerializeField] int maxProductsInDelivery;
    [SerializeField, Min(1)] int minGroupQuantity = 1;
    [SerializeField, Min(1)] int maxGroupQuantity = 1;

    [SerializeField] ListList<ProductID> possibleProductLists;

    [SerializeField] Transform productSpawnPosition; // TEMP: until delivery animation/theme chosen

    [Header("Zone")]
    [SerializeField] Vector3Int deliveryZoneDimensions;
    [SerializeField] Zone deliveryZone;
    Grid grid;

    RollTable<ProductID> productRollTable = new();

    int numProductsInDelivery;

    void Awake() { GameManager.Instance.SM_dayPhase.OnStateEnter += StateTrigger; }

    void Start() {
        grid = GameManager.WorldGrid;

        // Create delivery zone
        ZoneProperties deliveryZoneProps = new ZoneProperties() {CanPlace = true, CanTake = true};
        deliveryZone.Setup(Vector3Int.RoundToInt(transform.localPosition), deliveryZoneDimensions, deliveryZoneProps);
        grid.AddZone(deliveryZone);
    }

    void StateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) {
            ScaleDeliveryDifficulty(GameManager.Instance.Day);
            StartCoroutine(DoDelivery());
        }
    }

    void ScaleDeliveryDifficulty(int day) { // TEMP: pre-crafting difficulty formulas
        // Scale quantity
        numProductsInDelivery = productsPerDayGrowth * (day-1) + numInitialProductsInDelivery;
        if (numProductsInDelivery > maxProductsInDelivery) {
            numProductsInDelivery = maxProductsInDelivery;
        }

        // Scale variety
        if (day - 1 < possibleProductLists.outerList.Count) {
            foreach (ProductID productID in possibleProductLists.outerList[day - 1].innerList) {
                AddPossibleProduct(productID);
            }
        }
    }

    IEnumerator DoDelivery() {
        // Place products starting from (0, 0, 0) within deliveryZone
        // Order of placement is alternating forward/backwards every other row, one product on next open y of (x, z).
        int numProductsDelivered = 0;
        while (numProductsDelivered < numProductsInDelivery) {
            SO_Product productData = null;
            int groupQuantity = 0;
            int prodsDeliveredLastCycle = numProductsDelivered;

            for (int x = 0; x < deliveryZone.Length; x++) {
                int startZ = 0;
                int endZ = deliveryZone.Width;
                int stepZ = 1;
                if (x % 2 == 1) { // iterate backwards every other row
                    startZ = deliveryZone.Width - 1;
                    endZ = -1;
                    stepZ = -1;
                }

                for (int z = startZ; z != endZ; z += stepZ) {
                    Vector3Int deliveryCoord;
                    Vector3Int selectedCell = new Vector3Int(deliveryZone.RootCoord.x + x, grid.Height, deliveryZone.RootCoord.z + z);
                    if (grid.SelectLowestOpenFromCell(selectedCell, out int y)) {
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
                        yield return new WaitForSeconds(TweenManager.IndividualDeliveryDelay);
                    } else {
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

    void AddPossibleProduct(ProductID productID) {
        if (!productRollTable.Contains(productID)) {
            productRollTable.Add(productID, 1);
        }
    }

    public List<ProductID> GetDayPossibleProducts(int day) {
        if (day - 1 >= possibleProductLists.outerList.Count) return null;
        return possibleProductLists.outerList[day - 1].innerList;
    }

    #region Upgrades

    public void SetMaxGroupQuantity(int value) { maxGroupQuantity = value; }

    #endregion
}