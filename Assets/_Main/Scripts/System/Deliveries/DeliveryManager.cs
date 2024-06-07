using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(VolumeSlicer))]
public class DeliveryManager : MonoBehaviour {
    [Title("Special Delivery")]
    [SerializeField] int bulkQuantityMin;
    [SerializeField] int bulkQuantityMax;
    [SerializeField] RollTable<ShapeDataID> bulkDeliveryRollTable = new();
    [SerializeField] int irregularQuantityMin;
    [SerializeField] int irregularQuantityMax;
    [SerializeField] List<Deliverer> specialDeliverers = new();
    
    [Header("Deliverers")]
    [SerializeField] Transform docksContainer;
    List<Dock> docks;
    [SerializeField] GameObject delivererObj;
    [SerializeField] List<ObjDifficulty> deliveryBoxObjs;

    [Title("Other")]
    [SerializeField] ListList<ProductID> possibleProductLists; // currently unused, its just looking up shape -> valid product
    int numProductsInDelivery;

    // [Title("Delivery Scaling")]
    // [SerializeField] int numInitialProductsInDelivery;
    // [SerializeField] int productsPerDayGrowth;
    // [SerializeField] int productsInDeliveryMax;

    VolumeSlicer vs;

    void Awake() {
        vs = GetComponent<VolumeSlicer>();
        
        docks = docksContainer.GetComponentsInChildren<Dock>().ToList();

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
        // TEMP: cannot handle more deliveries than number of docks. Until deciding multiple deliveries behavior.
        // currently just one dock
        CreateDeliverer(docks[0]);

        // for (int i = 0; i < specialDeliverers.Count; i++) {
        //     GenerateSpecialDelivery(specialDeliverers[i]);
        // }
    }

    void CreateDeliverer(Dock openDock) {
        Deliverer deliverer = Instantiate(delivererObj, Ref.Instance.OffScreenSpawnTrs).GetComponent<Deliverer>();
        deliverer.OccupyDock(openDock);

        // Select delivery box based on current difficulty
        List<GameObject> possibleDelBoxes = GameManager.Instance.FilterByDifficulty(deliveryBoxObjs);
        GameObject delboxObj = possibleDelBoxes[Random.Range(0, possibleDelBoxes.Count)];
        
        DeliveryBox deliveryBox = Instantiate(delboxObj, deliverer.Grid.transform).GetComponentInChildren<DeliveryBox>();
        Vector3Int targetCoord = new Vector3Int(-deliveryBox.ShapeData.Length / 2, 0, -deliveryBox.ShapeData.Width / 2); // centers shape on grid origin
        
        // TEMP: scale deliverer floor grid, replaced after deliverer anim
        deliverer.Grid.SetGridSize(deliveryBox.ShapeData.Length, deliveryBox.ShapeData.Height, deliveryBox.ShapeData.Width);
        deliverer.transform.Find("Floor").transform.localScale = new Vector3(
            0.1f * deliveryBox.ShapeData.Length + 0.05f, 1, 0.1f * deliveryBox.ShapeData.Width + 0.05f
        );
        
        deliverer.Grid.PlaceShapeNoValidate(targetCoord, deliveryBox);
    }

    /// <summary>
    /// Uses basic slicing algorithm to generate basic shapes within local grid bounds.
    /// </summary>
    /// <param name="deliveryBoxShapeData">Shape data of delivery box containing this delivery.</param>
    /// <param name="grid">Grid to place shapes on.</param>
    public void GenerateBasicDelivery(ShapeData deliveryBoxShapeData, Grid grid) {
        // Generate shape datas of basic delivery
        Vector3Int minBoundCoord = deliveryBoxShapeData.RootCoord + deliveryBoxShapeData.MinOffset;
        Vector3Int maxBoundCoord = deliveryBoxShapeData.RootCoord + deliveryBoxShapeData.MaxOffset;
        List<ShapeData> volumeData = vs.Slice(minBoundCoord, maxBoundCoord);

        // Convert generated shape datas to product game objects and place them
        foreach (ShapeData shapeData in volumeData) {
            SO_Product productData = ProductFactory.Instance.CreateSOProduct(
                Ledger.Instance.ColorPaletteData.Colors[Random.Range(0, Ledger.Instance.ColorPaletteData.Colors.Count)],
                Pattern.None, // TEMP: until implementing pattern
                shapeData
            );
            // productData.ID.Pattern = patternPaletteData.Patterns[Random.Range(0, patternPaletteData.Patterns.Count)]; TODO: pattern lookup
            Product product = ProductFactory.Instance.CreateProduct(productData, shapeData.RootCoord);

            grid.PlaceShapeNoValidate(shapeData.RootCoord, product);

            Ledger.AddStockedProduct(product);
        }
    }

    // void GenerateSpecialDelivery(Deliverer deliverer) {
    //     // BulkDelivery(deliverer);
    //     IrregularDelivery(deliverer);
    // }
    //
    // void BulkDelivery(Deliverer deliverer) {
    //     Grid grid = deliverer.Grid;
    //     ShapeDataID id = bulkDeliveryRollTable.GetRandom();
    //     List<SO_Product> possibleProductDatas = ProductFactory.Instance.ShapeDataIDToProductDataLookUp[id];
    //     SO_Product productData = possibleProductDatas[Random.Range(0, possibleProductDatas.Count)];
    //
    //     int quantity = Random.Range(bulkQuantityMin, bulkQuantityMax);
    //     for (int x = grid.MinX; x < grid.MaxX; x++) {
    //         for (int z = grid.MinZ; z < grid.MaxZ; z++) {
    //             Vector3Int selectedXZ = new Vector3Int(x, grid.Height, z);
    //
    //             while (grid.SelectLowestOpenFromCell(selectedXZ, out int y)) {
    //                 Vector3Int deliveryCoord = new Vector3Int(x, y, z);
    //                 Product product = ProductFactory.Instance.CreateProduct(productData, grid.transform.position + deliveryCoord);
    //                 if (!grid.PlaceShape(deliveryCoord, product, true)) {
    //                     Debug.LogErrorFormat(
    //                         "Unable to place shape at {0} in delivery: Selected cell should have been open.", deliveryCoord
    //                     );
    //                     return;
    //                 }
    //
    //                 Ledger.AddStockedProduct(product);
    //
    //                 quantity--;
    //
    //                 if (quantity == 0) {
    //                     return;
    //                 }
    //             }
    //         }
    //     }
    //
    //     if (quantity > 0) {
    //         Debug.LogWarning($"Unable to place all products in bulk delivery: {quantity} remaining.");
    //     }
    // }
    //
    // // TEMP: selects one from all products
    // // TODO: fix for new ProductID
    // void IrregularDelivery(Deliverer deliverer) {
    //     Grid grid = deliverer.Grid;
    //     List<ProductID> possibleProductIDs = ProductFactory.Instance.ProductDataLookUp.Keys.ToList();
    //     ProductID id = possibleProductIDs[Random.Range(0, possibleProductIDs.Count)];
    //     SO_Product productData = ProductFactory.Instance.ProductDataLookUp[id];
    //
    //     int quantity = Random.Range(irregularQuantityMin, irregularQuantityMax);
    //     for (int x = grid.MinX; x < grid.MaxX; x++) {
    //         for (int z = grid.MinZ; z < grid.MaxZ; z++) {
    //             Vector3Int selectedXZ = new Vector3Int(x, grid.Height, z);
    //
    //             while (grid.SelectLowestOpenFromCell(selectedXZ, out int y)) {
    //                 Vector3Int deliveryCoord = new Vector3Int(x, y, z);
    //                 Product product = ProductFactory.Instance.CreateProduct(productData, grid.transform.position + deliveryCoord);
    //                 if (!grid.PlaceShape(deliveryCoord, product, true)) {
    //                     Debug.LogErrorFormat(
    //                         "Unable to place shape at {0} in delivery: Selected cell should have been open.", deliveryCoord
    //                     );
    //                     return;
    //                 }
    //
    //                 Ledger.AddStockedProduct(product);
    //
    //                 quantity--;
    //
    //                 if (quantity == 0) {
    //                     return;
    //                 }
    //             }
    //         }
    //     }
    //
    //     if (quantity > 0) {
    //         Debug.LogWarning($"Unable to place all products in irregular delivery: {quantity} remaining.");
    //     }
    // }

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
        // if (!basicProductRollTable.Contains(productID)) {
        //     basicProductRollTable.Add(productID, 1);
        // }
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