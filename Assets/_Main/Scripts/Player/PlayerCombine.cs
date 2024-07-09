using System.Collections.Generic;
using System.Linq;
using Timers;
using UnityEngine;

public class PlayerCombine : MonoBehaviour, IPlayerTool {
    [SerializeField] GameObject previewObj; // a obj with ShapeOutlineRenderer component
    ShapeOutlineRenderer previewRenderer;

    Grid targetGrid;

    Product startProduct;
    Product lastSelectedProduct;

    ShapeData combinedShapeData;
    List<Product> oldProducts = new();

    void Awake() {
        previewRenderer = previewObj.GetComponent<ShapeOutlineRenderer>();
        if (previewRenderer == null) {
            Debug.LogError("Combine preview obj is missing ShapeOutlineRenderer component.");
            return;
        }
    }

    void StartCombine(ClickInputArgs clickInputArgs) {
        if (!clickInputArgs.TargetObj.TryGetComponent(out Product product)) {
            return;
        }

        if (product.Grid != GameManager.WorldGrid || product.ShapeTags.Contains(ShapeTagID.NoCombine)) {
            TweenManager.Shake(product);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }

        targetGrid = product.Grid;
        startProduct = product;
        lastSelectedProduct = startProduct;
        
        combinedShapeData = new ShapeData(startProduct.ShapeData);
        oldProducts.Add(startProduct);

        // Draw shape outline for combined shape
        previewRenderer.Render(combinedShapeData);
        previewObj.SetActive(true);
    }

    void ContinueCombine(ClickInputArgs clickInputArgs) {
        if (startProduct == null) return;
        
        if (!clickInputArgs.TargetObj.TryGetComponent(out Product hoveredProduct)) {
            return;
        }

        // Cutoff for not repeating on same shape
        if (hoveredProduct == lastSelectedProduct || hoveredProduct == startProduct) return;
        lastSelectedProduct = hoveredProduct;

        // Check adjacent to combine shape
        List<Vector3Int> combinedOffsetsToGridSpace = 
            combinedShapeData.ShapeOffsets.Select(offset => combinedShapeData.RootCoord + offset).ToList();
        if (!hoveredProduct.ShapeData.IsAdjacentToCoords(combinedOffsetsToGridSpace)) {
            return;
        }

        // Check same color as combine shape
        if (hoveredProduct.ID.Color != startProduct.ID.Color) {
            TweenManager.Shake(hoveredProduct);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }
        
        // Create new offsets for combined shape
        List<Vector3Int> newOffsets = new List<Vector3Int>(combinedShapeData.ShapeOffsets);
        Vector3Int toProductRoot = hoveredProduct.ShapeData.RootCoord - combinedShapeData.RootCoord;
        foreach (Vector3Int offset in hoveredProduct.ShapeData.ShapeOffsets) {
            newOffsets.Add(toProductRoot + offset);
        }
        
        // Create new ShapeData keeping original shape root
        combinedShapeData = new ShapeData {RootCoord = combinedShapeData.RootCoord, ShapeOffsets = newOffsets};
        oldProducts.Add(hoveredProduct);
        
        // Draw shape outline for combined shape
        previewRenderer.Render(combinedShapeData);
        previewObj.SetActive(true);
    }

    void StopCombine(ClickInputArgs clickInputArgs) {
        // Remove original shapes from grid
        foreach (Product product in oldProducts) {
            targetGrid.RemoveShapeCells(product, false);
        }

        combinedShapeData.ID = ShapeData.DetermineID(combinedShapeData.ShapeOffsets);

        SO_Product productData = ProductFactory.Instance.CreateSOProduct(
            startProduct.ID.Color, startProduct.ID.Pattern, combinedShapeData
        );
        Product newProduct = ProductFactory.Instance.CreateProduct(
            productData, targetGrid.transform.TransformPoint(startProduct.ShapeData.RootCoord)
        );

        targetGrid.PlaceShapeNoValidate(combinedShapeData.RootCoord, newProduct);
        Ledger.AddStockedProduct(newProduct);

        // Destroy original shapes
        foreach (Product product in oldProducts) {
            Ledger.RemoveStockedProduct(product);
            ((IGridShape) product).DestroyShape(false);
        }

        startProduct = null;
        lastSelectedProduct = null;
        targetGrid = null;
        oldProducts.Clear();

        previewObj.SetActive(false);
    }

    public void Equip() {
        Ref.Player.PlayerInput.InputPrimaryDown += StartCombine;
        Ref.Player.PlayerInput.InputPoint += ContinueCombine;
        Ref.Player.PlayerInput.InputPrimaryUp += StopCombine;
    }
    public bool Unequip() {
        Ref.Player.PlayerInput.InputPrimaryDown -= StartCombine;
        Ref.Player.PlayerInput.InputPoint -= ContinueCombine;
        Ref.Player.PlayerInput.InputPrimaryUp -= StopCombine;
        previewObj.SetActive(false);
        return true;
    }
}