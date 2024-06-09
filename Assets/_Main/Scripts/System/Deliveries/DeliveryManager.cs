using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(VolumeSlicer))]
public class DeliveryManager : MonoBehaviour {
    [Title("General")]
    [Tooltip("Determines possible color choices for ALL delivery types.")]
    [SerializeField] int maxIndexColorPalette;
    
    [Title("Basic Delivery")]
    [SerializeField] int basicMaxShapeLength;
    [SerializeField] int basicMaxShapeWidth;

    [Tooltip("1 = shapes extended until hitting volume boundary, an existing shape, or reaching max length.")]
    [SerializeField, Range(0f, 1f)] float basicChanceOfShapeExtension;
    [Tooltip("1 = all shapes oriented in same direction")]
    [SerializeField, Range(0f, 1f)] float basicOrderliness;

    VolumeSlicer basicVs;

    [Title("Bulk Delivery")]
    [Tooltip("Frequency of bulk deliveries (i.e. every X days).")]
    [SerializeField] int bulkDayInterval = 3;

    [Title("Irregular Delivery")]
    [SerializeField] DifficultyTable<float> irregularChanceDiffTable;
    [SerializeField] DifficultyTable<ShapeDataID> irregularShapesDiffTable;

    [Header("Deliverers")]
    [SerializeField] Transform docksContainer;
    List<Dock> docks;
    [SerializeField] GameObject delivererObj;
    [SerializeField] DifficultyTable<GameObject> deliveryBoxDiffTable;

    // [Title("Delivery Scaling")]
    // [SerializeField] int numInitialProductsInDelivery;
    // [SerializeField] int productsPerDayGrowth;
    // [SerializeField] int productsInDeliveryMax;


    void Awake() {
        basicVs = GetComponent<VolumeSlicer>();
        basicVs.SetOptions(basicMaxShapeLength, basicMaxShapeWidth, basicChanceOfShapeExtension);

        docks = docksContainer.GetComponentsInChildren<Dock>().ToList();

        GameManager.Instance.SM_dayPhase.OnStateEnter += StateTrigger;
    }

    void StateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) {
            ScaleDeliveryDifficulty(GameManager.Instance.Day);
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
        // Setup deliverer
        Deliverer deliverer = Instantiate(delivererObj, Ref.Instance.OffScreenSpawnTrs).GetComponent<Deliverer>();
        deliverer.OccupyDock(openDock);

        float irregularChance = irregularChanceDiffTable.GetHighestByDifficulty();
        IGridShape cargoShape;
        if (GameManager.Instance.Day % bulkDayInterval == 0) { // Create delivery box for bulk delivery
            GameObject obj = deliveryBoxDiffTable.GetRandomByDifficulty();
            DeliveryBox deliveryBox = Instantiate(obj, deliverer.Grid.transform).GetComponentInChildren<DeliveryBox>();
            deliveryBox.SetDeliveryBoxType(DeliveryBox.DeliveryBoxType.Bulk);
            
            cargoShape = deliveryBox;
        } else if (Random.Range(0f, 1f) <= irregularChance) {   // Create irregular delivery
            cargoShape = GenerateIrregularDelivery();
        } else {                                                // Create delivery box for basic delivery
            GameObject obj = deliveryBoxDiffTable.GetRandomByDifficulty();
            DeliveryBox deliveryBox = Instantiate(obj, deliverer.Grid.transform).GetComponentInChildren<DeliveryBox>();
            deliveryBox.SetDeliveryBoxType(DeliveryBox.DeliveryBoxType.Basic);

            cargoShape = deliveryBox;
        }
        
        // TEMP: scale deliverer floor grid, replaced after deliverer anim
        ShapeData cargoShapeData = cargoShape.ShapeData;
        deliverer.Grid.SetGridSize(cargoShapeData.Length, cargoShapeData.Height, cargoShapeData.Width);
        deliverer.transform.Find("Floor").transform.localScale = new Vector3(
            0.1f * cargoShapeData.Length + 0.05f, 1, 0.1f * cargoShapeData.Width + 0.05f
        );
        
        // centers shape on deliverer grid origin
        Vector3Int targetCoord = new Vector3Int(-cargoShapeData.Length / 2, 0, -cargoShapeData.Width / 2);

        deliverer.Grid.PlaceShapeNoValidate(targetCoord, cargoShape);
    }

    /// <summary>
    /// Uses basic slicing algorithm to generate basic shapes within local grid bounds.
    /// </summary>
    /// <param name="deliveryBox">Delivery box containing this delivery.</param>
    public void BasicDelivery(DeliveryBox deliveryBox) {
        // Generate shape datas of basic delivery
        Vector3Int minBoundCoord = deliveryBox.ShapeData.RootCoord + deliveryBox.ShapeData.MinOffset;
        Vector3Int maxBoundCoord = deliveryBox.ShapeData.RootCoord + deliveryBox.ShapeData.MaxOffset;

        DeliveryOrientation orientation = SelectOrientation(basicOrderliness);
        List<Direction2D> extensionDirs = orientation switch {
            DeliveryOrientation.All => new() {Direction2D.North, Direction2D.East, Direction2D.South, Direction2D.West},
            DeliveryOrientation.NS => new() {Direction2D.North, Direction2D.South},
            DeliveryOrientation.EW => new() {Direction2D.East, Direction2D.West},
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };

        List<ShapeData> volumeData = basicVs.Slice(minBoundCoord, maxBoundCoord, extensionDirs);

        // Convert generated shape datas to product game objects and place them
        foreach (ShapeData shapeData in volumeData) {
            SO_Product productData = ProductFactory.Instance.CreateSOProduct(
                Ledger.Instance.ColorPaletteData.Colors[Random.Range(0, maxIndexColorPalette)],
                Pattern.None, // TEMP: until implementing pattern
                shapeData
            );
            // productData.ID.Pattern = patternPaletteData.Patterns[Random.Range(0, patternPaletteData.Patterns.Count)]; TODO: pattern lookup
            Product product = ProductFactory.Instance.CreateProduct(productData, deliveryBox.Grid.transform.position + shapeData.RootCoord);

            deliveryBox.Grid.PlaceShapeNoValidate(shapeData.RootCoord, product);

            Ledger.AddStockedProduct(product);
        }
    }

    // want either all of y level filled or many neat rows of width 1 shapes to fill, 1 layer deep always
    public void BulkDelivery(DeliveryBox deliveryBox) {
        // Determine shape based on delivery box dimensions
        int length = deliveryBox.ShapeData.Length;
        int width = 1; // TODO: any width that fits evenly into delivery box

        List<Vector3Int> shapeOffsets = new();
        for (int x = 0; x < length; x++) {
            for (int z = 0; z < width; z++) {
                shapeOffsets.Add(new Vector3Int(x, 0, z));
            }
        }

        // Build shape data for bulk
        ShapeDataID id = ShapeData.DetermineID(shapeOffsets);
        ShapeData shapeData = id == ShapeDataID.Custom ?
            new ShapeData(ShapeDataID.Custom, Vector3Int.zero, shapeOffsets) :
            ShapeDataLookUp.LookUp(id);

        // Create shape in bulk to fill delivery box volume
        Grid grid = deliveryBox.Grid;
        Vector3Int minBoundCoord = deliveryBox.ShapeData.RootCoord + deliveryBox.ShapeData.MinOffset;
        Vector3Int maxBoundCoord = deliveryBox.ShapeData.RootCoord + deliveryBox.ShapeData.MaxOffset;
        Color color = Ledger.Instance.ColorPaletteData.Colors[Random.Range(0, maxIndexColorPalette)];

        for (int z = minBoundCoord.z; z <= maxBoundCoord.z; z += width) {
            for (int y = minBoundCoord.y; y <= maxBoundCoord.y; y++) {
                SO_Product productData = ProductFactory.Instance.CreateSOProduct(
                    color,
                    Pattern.None, // TEMP: until implementing pattern
                    shapeData
                );

                Vector3Int deliveryCoord = new Vector3Int(minBoundCoord.x, y, z);
                Product product = ProductFactory.Instance.CreateProduct(productData, grid.transform.position + deliveryCoord);

                deliveryBox.Grid.PlaceShapeNoValidate(deliveryCoord, product);

                Ledger.AddStockedProduct(product);
            }
        }
    }

    Product GenerateIrregularDelivery() {
        ShapeDataID id = irregularShapesDiffTable.GetRandomByDifficulty();
        ShapeData shapeData = ShapeDataLookUp.LookUp(id);
        SO_Product productData = ProductFactory.Instance.CreateSOProduct(
            Ledger.Instance.ColorPaletteData.Colors[Random.Range(0, maxIndexColorPalette)],
            Pattern.None, // TEMP: until implementing pattern
            shapeData
        );

        Product product = ProductFactory.Instance.CreateProduct(productData, Ref.Instance.OffScreenSpawnTrs.position);
        
        Ledger.AddStockedProduct(product);

        return product;
    }

    void ScaleDeliveryDifficulty(int day) {
        // TODO: scale basic, bulk, irregular delivery
    }

    #region Delivery Orientation

    enum DeliveryOrientation {
        All = 0,
        NS = 1, // North South
        EW = 2, // East West
    }

    DeliveryOrientation SelectOrientation(float orderliness) {
        if (orderliness < 0 || 1f < orderliness) {
            Debug.LogError("Unable to determine delivery orientation: orderliness is not between 0 and 1");
            return DeliveryOrientation.All;
        }

        // scaled weights based on orderliness
        float nsWeight = Mathf.Lerp(0, 1, orderliness);
        float ewWeight = Mathf.Lerp(0, 1, orderliness);
        float allWeight = Mathf.Lerp(1, 0, orderliness);

        float totalWeight = nsWeight + ewWeight + allWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < nsWeight) {
            return DeliveryOrientation.NS;
        } else if (randomValue < nsWeight + ewWeight) {
            return DeliveryOrientation.EW;
        } else {
            return DeliveryOrientation.All;
        }
    }

    #endregion

    #region Upgrades

    public void SetMaxGroupQuantity(int value) {
        // maxGroupQuantity = value;
    }

    #endregion
}