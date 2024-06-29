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
    [SerializeField] int maxColorIndex = 1;

    [Title("Basic Delivery")]
    [SerializeField] int basicFirstDimensionMax;
    [SerializeField] int basicSecondDimensionMax;

    [Tooltip("1 = shapes extended until hitting volume boundary, an existing shape, or reaching max length.")]
    [SerializeField, Range(0f, 1f)] float basicChanceShapeExtension;
    [Tooltip("1 = all shapes oriented in same direction")]
    [SerializeField, Range(0f, 1f)] float basicOrderliness;

    VolumeSlicer basicVs;

    [Title("Bulk Delivery")]
    [Tooltip("Frequency of bulk deliveries (i.e. every X days).")]
    [SerializeField] int bulkDayInterval = 3;

    [Title("Irregular Delivery")]
    [SerializeField, Range(0f, 1f)] float irregularChance;
    [SerializeField] List<ShapeDataID> irregularShapePool;

    [Header("Deliverers")]
    [SerializeField] Transform docksContainer;
    List<Dock> docks;
    [SerializeField] GameObject delivererObj;
    [SerializeField] List<GameObject> deliveries;
    List<DeliveryBox> curDeliveryBoxes = new();
    public bool AllDeliveriesOpened => curDeliveryBoxes.Count == 0 && curDeliveryIndex == deliveries.Count;
    public event Action OnDeliveriesOpenedCheck;

    int curDeliveryIndex;

    void Awake() {
        basicVs = GetComponent<VolumeSlicer>();

        bulkDayInterval = GameManager.Instance.BulkDayInterval;

        docks = docksContainer.GetComponentsInChildren<Dock>().ToList();

        GameManager.Instance.SM_dayPhase.OnStateEnter += EnterStateTrigger;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) {
            DifficultyManager.Instance.ApplyDeliveryDifficulty();

            // separate from SetDifficultyOptions for setting debug mode values
            basicVs.SetOptions(basicFirstDimensionMax, basicSecondDimensionMax, basicChanceShapeExtension);

            Deliver();
        }
    }

    void Deliver() {
        curDeliveryBoxes.Clear();

        // TEMP: cannot handle more deliveries than number of docks. Until deciding multiple deliveries behavior.
        // TODO: deliverer queue
        int count = Math.Min(deliveries.Count, docks.Count);
        curDeliveryIndex = count;
        for (int i = 0; i < count; i++) {
            CreateDeliverer(docks[i], deliveries[i]);
        }
    }

    void CreateDeliverer(Dock openDock, GameObject deliveryBoxObj) {
        if (deliveries == null || deliveries.Count == 0) {
            Debug.LogError("Deliveries list is empty.");
            return;
        }

        // Setup deliverer
        Deliverer deliverer = Instantiate(delivererObj, Ref.Instance.OffScreenSpawnTrs).GetComponent<Deliverer>();
        deliverer.OccupyDock(openDock);

        IGridShape cargoShape;
        if (GameManager.Instance.Day % bulkDayInterval == 0) { // Create delivery box for bulk delivery
            DeliveryBox deliveryBox = Instantiate(deliveryBoxObj, deliverer.Grid.transform).GetComponentInChildren<DeliveryBox>();
            deliveryBox.SetDeliveryBoxType(DeliveryBox.DeliveryBoxType.Bulk);

            curDeliveryBoxes.Add(deliveryBox);
            cargoShape = deliveryBox;
        } else if (Random.Range(0f, 1f) <= irregularChance) { // Create irregular delivery
            cargoShape = GenerateIrregularDelivery();
        } else { // Create delivery box for basic delivery
            DeliveryBox deliveryBox = Instantiate(deliveryBoxObj, deliverer.Grid.transform).GetComponentInChildren<DeliveryBox>();
            deliveryBox.SetDeliveryBoxType(DeliveryBox.DeliveryBoxType.Basic);

            curDeliveryBoxes.Add(deliveryBox);
            cargoShape = deliveryBox;
        }

        // TEMP: scale deliverer floor grid, replaced after deliverer anim
        ShapeData cargoShapeData = cargoShape.ShapeData;
        deliverer.Grid.SetGridSize(cargoShapeData.Length, cargoShapeData.Height, cargoShapeData.Width);
        deliverer.transform.Find("Floor").transform.localScale = new Vector3(0.1f * cargoShapeData.Length, 1, 0.1f * cargoShapeData.Width);

        // centers shape on deliverer grid origin by moving delivery box root to a corner
        Vector3Int targetCoord = new Vector3Int(-cargoShapeData.Length / 2, 0, -cargoShapeData.Width / 2);

        deliverer.Grid.PlaceShapeNoValidate(targetCoord, cargoShape);
    }

    public void HandleFinishedDeliverer(Deliverer deliverer) {
        if (curDeliveryIndex < deliveries.Count) {
            CreateDeliverer(deliverer.AssignedDock, deliveries[curDeliveryIndex]);
            curDeliveryIndex++;
        }

        deliverer.Docker.OnReachedEnd += () => {
            Destroy(deliverer.gameObject);
            OnDeliveriesOpenedCheck?.Invoke();
        };
    }

    /// <summary>
    /// Uses basic slicing algorithm to generate basic shapes within local grid bounds.
    /// </summary>
    /// <param name="deliveryBox">Delivery box containing this delivery.</param>
    public void BasicDelivery(DeliveryBox deliveryBox) {
        // Generate shape datas of basic delivery
        Vector3Int minBoundCoord = deliveryBox.ShapeData.RootCoord + deliveryBox.ShapeData.MinOffset;
        Vector3Int maxBoundCoord = deliveryBox.ShapeData.RootCoord + deliveryBox.ShapeData.MaxOffset;

        DeliveryOrientation orientation = OrderlinessToOrientation(basicOrderliness);
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
                Ledger.Instance.ColorPaletteData.Colors[Random.Range(0, maxColorIndex)],
                Pattern.None, // TEMP: until implementing pattern
                shapeData
            );
            // productData.ID.Pattern = patternPaletteData.Patterns[Random.Range(0, patternPaletteData.Patterns.Count)]; TODO: pattern lookup
            Product product = ProductFactory.Instance.CreateProduct(productData, deliveryBox.Grid.transform.position + shapeData.RootCoord);

            deliveryBox.Grid.PlaceShapeNoValidate(shapeData.RootCoord, product);

            Ledger.AddStockedProduct(product);
        }

        curDeliveryBoxes.Remove(deliveryBox);
        OnDeliveriesOpenedCheck?.Invoke();
    }

    /// <summary>
    /// Fills delivery box using one basic shape type all aligned.
    /// </summary>
    /// <remarks>NOTE: assumes delivery box is only rectangular shape.</remarks>
    /// <param name="deliveryBox"></param>
    public void BulkDelivery(DeliveryBox deliveryBox) {
        // Determine shape based on delivery box dimensions
        // TODO: any length/width that fits evenly into delivery box
        int length = 1;
        int width = deliveryBox.ShapeData.Width;

        // Build shape data for bulk
        List<Vector3Int> shapeOffsets = new();
        for (int x = 0; x < length; x++) {
            for (int z = 0; z < width; z++) {
                shapeOffsets.Add(new Vector3Int(x, 0, z));
            }
        }

        ShapeDataID id = ShapeData.DetermineID(shapeOffsets);
        ShapeData shapeData = new ShapeData(id, Vector3Int.zero, shapeOffsets);

        // Create shape in bulk to fill delivery box volume
        Grid grid = deliveryBox.Grid;
        Vector3Int minBoundCoord = deliveryBox.ShapeData.RootCoord + deliveryBox.ShapeData.MinOffset;
        Vector3Int maxBoundCoord = deliveryBox.ShapeData.RootCoord + deliveryBox.ShapeData.MaxOffset;
        Color color = Ledger.Instance.ColorPaletteData.Colors[Random.Range(0, maxColorIndex)];

        for (int x = minBoundCoord.x; x <= maxBoundCoord.x; x += length) {
            for (int y = minBoundCoord.y; y <= maxBoundCoord.y; y++) {
                SO_Product productData = ProductFactory.Instance.CreateSOProduct(
                    color,
                    Pattern.None, // TEMP: until implementing pattern
                    shapeData
                );

                Vector3Int deliveryCoord = new Vector3Int(x, y, minBoundCoord.z);
                Product product = ProductFactory.Instance.CreateProduct(productData, grid.transform.position + deliveryCoord);

                deliveryBox.Grid.PlaceShapeNoValidate(deliveryCoord, product);

                Ledger.AddStockedProduct(product);
            }
        }

        curDeliveryBoxes.Remove(deliveryBox);
        OnDeliveriesOpenedCheck?.Invoke();
    }

    Product GenerateIrregularDelivery() {
        ShapeDataID id = Util.GetRandomFromList(irregularShapePool);
        ShapeData shapeData = ShapeDataLookUp.LookUp(id);
        List<ShapeTagID> shapeTags = new List<ShapeTagID> {ShapeTagID.NoCombine, ShapeTagID.NoSlice, ShapeTagID.NoPlaceInTrash};
        SO_Product productData = ProductFactory.Instance.CreateSOProduct(
            Ledger.Instance.ColorPaletteData.Colors[Random.Range(0, maxColorIndex)],
            Pattern.None, // TEMP: until implementing pattern
            shapeData,
            shapeTags
        );

        Product product = ProductFactory.Instance.CreateProduct(productData, Ref.Instance.OffScreenSpawnTrs.position);

        Ledger.AddStockedProduct(product);

        return product;
    }

    #region Delivery Orientation

    enum DeliveryOrientation {
        All = 0,
        NS = 1, // North South
        EW = 2, // East West
    }

    DeliveryOrientation OrderlinessToOrientation(float orderliness) {
        if (orderliness < 0 || 1f < orderliness) {
            Debug.LogError("Unable to determine delivery orientation: orderliness is not between 0 and 1");
            return DeliveryOrientation.All;
        }

        // scaled weights based on orderliness (if length == width, then favors NS)
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

    public void SetDifficultyOptions(SO_DeliveriesDifficultyTable.DeliveryDifficultyEntry deliveryDiffEntry) {
        maxColorIndex = deliveryDiffEntry.maxColorIndex;
        deliveries = new List<GameObject>(deliveryDiffEntry.deliveries);
        basicFirstDimensionMax = deliveryDiffEntry.basicFirstDimensionMax;
        basicSecondDimensionMax = deliveryDiffEntry.basicSecondDimensionMax;
        basicChanceShapeExtension = deliveryDiffEntry.basicChanceShapeExtension;
        irregularChance = deliveryDiffEntry.irregularChance;
        irregularShapePool = new List<ShapeDataID>(deliveryDiffEntry.irregularShapePool);
    }
}