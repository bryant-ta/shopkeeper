using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(VolumeSlicer))]
public class DeliveryManager : MonoBehaviour {
    // TODO: reorgnize these params
    [Title("Delivery Size")]
    [SerializeField, Space] bool useWholeGridAsBounds;
    [Tooltip("The lowest coord of the volume in a Deliverer grid to spawn products in. One corner of a cube.")]
    [ShowIf(nameof(useWholeGridAsBounds), false)]
    [SerializeField] Vector3Int minBoundProductSpawn;
    [Tooltip("The highest coord of the volume in a Deliverer grid to spawn products in. The opposite corner of a cube.")]
    [ShowIf(nameof(useWholeGridAsBounds), false)]
    [SerializeField] Vector3Int maxBoundProductSpawn;

    int numProductsInDelivery;

    [Title("Delivery Contents")]
    [SerializeField] ListList<ProductID> possibleProductLists;

    [SerializeField] RollTable<ProductID> basicProductRollTable = new();
    [SerializeField] RollTable<GameObject> specialProductRollTable = new();
    [SerializeField] RollTable<ShapeDataID> basicShapeRollTable = new();

    [Title("Special Delivery")]
    [SerializeField] int bulkQuantityMin;
    [SerializeField] int bulkQuantityMax;

    // [Title("Delivery Scaling")]
    // [SerializeField] int numInitialProductsInDelivery;
    // [SerializeField] int productsPerDayGrowth;
    // [SerializeField] int productsInDeliveryMax;

    [Title("Other")]
    [SerializeField] List<Deliverer> deliverers = new();

    VolumeSlicer vs;

    void Awake() {
        vs = GetComponent<VolumeSlicer>();
        GameManager.Instance.SM_dayPhase.OnStateEnter += StateTrigger;
    }

    void StateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) {
            ScaleDeliveryDifficulty(GameManager.Instance.Day);
            // StartCoroutine(DoDelivery());


            // DEBUG
            Deliver();
        }
    }

    void Deliver() {
        // TODO: basic delivery every day
        // GenerateBasicDelivery(deliverers[0]);

        // TODO: special delivery every 3 days
        GenerateSpecialDelivery(deliverers[1]);
    }

    void GenerateBasicDelivery(Deliverer deliverer) {
        Grid grid = deliverer.Grid;
        Dictionary<Vector3Int, ShapeData> volumeData;
        if (useWholeGridAsBounds) {
            volumeData = vs.Slice(new Vector3Int(grid.MinX, 0, grid.MinZ), new Vector3Int(grid.MaxX, grid.Height - 1, grid.MaxZ));
        } else {
            volumeData = vs.Slice(minBoundProductSpawn, maxBoundProductSpawn);
        }

        // Convert generated shape datas to product game objects and placement
        foreach (KeyValuePair<Vector3Int, ShapeData> kv in volumeData) {
            ShapeDataID curShapeDataID = kv.Value.ID;
            List<SO_Product> possibleProductDatas = ProductFactory.Instance.ShapeDataIDToProductDataLookUp[curShapeDataID];

            SO_Product productData = Instantiate(possibleProductDatas[Random.Range(0, possibleProductDatas.Count)]);
            productData.ShapeData = kv.Value;
            Product product = ProductFactory.Instance.CreateProduct(productData, grid.transform.position);

            grid.PlaceShapeNoValidate(kv.Key, product);

            GameManager.AddStockedProduct(product);
        }
    }

    void GenerateSpecialDelivery(Deliverer deliverer) { BulkDelivery(deliverer); }

    void BulkDelivery(Deliverer deliverer) {
        Grid grid = deliverer.Grid;
        ShapeDataID id = basicShapeRollTable.GetRandom();
        print(id);
        List<SO_Product> possibleProductDatas = ProductFactory.Instance.ShapeDataIDToProductDataLookUp[id];
        SO_Product productData = possibleProductDatas[Random.Range(0, possibleProductDatas.Count)];

        int quantity = Random.Range(bulkQuantityMin, bulkQuantityMax);
        for (int x = grid.MinX; x < grid.MaxX; x++) {
            for (int z = grid.MinZ; z < grid.MaxZ; z++) {
                Vector3Int selectedXZ = new Vector3Int(x, grid.Height, z);

                while (grid.SelectLowestOpenFromCell(selectedXZ, out int y)) {
                    Vector3Int deliveryCoord = new Vector3Int(x, y, z);
                    Product product = ProductFactory.Instance.CreateProduct(productData, selectedXZ);
                    if (!grid.PlaceShape(deliveryCoord, product, true)) {
                        Debug.LogErrorFormat("Unable to place shape at {0} in delivery: Selected cell should have been open.", deliveryCoord);
                        break;
                    }
                    
                    GameManager.AddStockedProduct(product);

                    quantity--;

                    if (quantity == 0) {
                        return;
                    }
                }
            }
        }

        if (quantity > 0) {
            Debug.LogWarning($"Unable to place all products in bulk delivery: {quantity} remaining.");
        }
    }

    void IrregularDelivery() { }

    // IEnumerator DoDelivery() {
    //     // Place products starting from (0, 0, 0) within deliveryZone
    //     // Order of placement is alternating forward/backwards every other row, one product on next open y of (x, z).
    //     int numProductsDelivered = 0;
    //     while (numProductsDelivered < numProductsInDelivery) {
    //         SO_Product productData = null;
    //         int groupQuantity = 0;
    //         int prodsDeliveredLastCycle = numProductsDelivered;
    //
    //         for (int x = 0; x < deliveryZone.Length; x++) {
    //             int startZ = 0;
    //             int endZ = deliveryZone.Width;
    //             int stepZ = 1;
    //             if (x % 2 == 1) { // iterate backwards every other row
    //                 startZ = deliveryZone.Width - 1;
    //                 endZ = -1;
    //                 stepZ = -1;
    //             }
    //
    //             for (int z = startZ; z != endZ; z += stepZ) {
    //                 Vector3Int deliveryCoord;
    //                 Vector3Int selectedCell = new Vector3Int(deliveryZone.RootCoord.x + x, grid.Height, deliveryZone.RootCoord.z + z);
    //                 if (grid.SelectLowestOpenFromCell(selectedCell, out int y)) {
    //                     deliveryCoord = deliveryZone.RootCoord + new Vector3Int(x, y, z);
    //                 } else { // this xz coord has no free cells
    //                     continue;
    //                 }
    //
    //                 if (groupQuantity == 0) { // finished a group, generate new group with random product
    //                     productData = ProductFactory.Instance.ProductDataLookUp[basicProductRollTable.GetRandom()];
    //                     groupQuantity = Random.Range(minGroupQuantity, maxGroupQuantity + 1);
    //                 }
    //
    //                 groupQuantity--;
    //
    //                 Product product = ProductFactory.Instance.CreateProduct(productData);
    //
    //                 if (product.TryGetComponent(out IGridShape shape)) {
    //                     shape.ShapeTransform.position = productSpawnPosition.position;
    //                     if (!grid.PlaceShape(deliveryCoord, shape, true)) {
    //                         Debug.LogErrorFormat("Unable to place shape at {0} in delivery zone", deliveryCoord);
    //                     }
    //
    //                     GameManager.AddStockedProduct(product);
    //
    //                     // Stagger delivery anim
    //                     // TODO: maybe use tween delay for this - but really there is no anim needed bc they come in off screen
    //                     yield return new WaitForSeconds(TweenManager.IndividualDeliveryDelay);
    //                 } else {
    //                     Debug.LogErrorFormat("Unable to deliver product {0}: product has no grid shape.", product.Name);
    //                     yield break;
    //                 }
    //
    //                 numProductsDelivered++;
    //                 if (numProductsDelivered == numProductsInDelivery) yield break;
    //             }
    //         }
    //
    //         // Did not finish delivering target number of products
    //         if (prodsDeliveredLastCycle == numProductsDelivered) {
    //             Debug.LogWarning("Unable to deliver all products: delivery zone is full.");
    //             yield break;
    //         }
    //     }
    // }

    void AddPossibleProduct(ProductID productID) {
        if (!basicProductRollTable.Contains(productID)) {
            basicProductRollTable.Add(productID, 1);
        }
    }

    public List<ProductID> GetDayPossibleProducts(int day) {
        if (day - 1 >= possibleProductLists.outerList.Count) return null;
        return possibleProductLists.outerList[day - 1].innerList;
    }

    // TEMP: pre-crafting difficulty formulas
    void ScaleDeliveryDifficulty(int day) {
        // Scale quantity
        // numProductsInDelivery = productsPerDayGrowth * (day - 1) + numInitialProductsInDelivery;
        // if (numProductsInDelivery > maxProductsInDelivery) {
        //     numProductsInDelivery = maxProductsInDelivery;
        // }

        // Scale variety
        if (day - 1 < possibleProductLists.outerList.Count) {
            foreach (ProductID productID in possibleProductLists.outerList[day - 1].innerList) {
                AddPossibleProduct(productID);
            }
        }
    }

    #region Upgrades

    public void SetMaxGroupQuantity(int value) {
        // maxGroupQuantity = value;
    }

    #endregion
}