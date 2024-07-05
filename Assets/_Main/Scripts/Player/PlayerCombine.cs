using System.Collections.Generic;
using System.Linq;
using Timers;
using UnityEngine;

public class PlayerCombine : MonoBehaviour, IPlayerTool {
    [SerializeField] GameObject previewObj; // a obj with ShapeOutlineRenderer component
    ShapeOutlineRenderer previewRenderer;
    
    [SerializeField] float cooldown;
    CountdownTimer cooldownTimer;

    Grid targetGrid;

    Product combinedProduct;
    Product lastSelectedProduct;

    void Awake() {
        previewRenderer = previewObj.GetComponent<ShapeOutlineRenderer>();
        if (previewRenderer == null) {
            Debug.LogError("Combine preview obj is missing ShapeOutlineRenderer component.");
            return;
        }

        cooldownTimer = new CountdownTimer(cooldown);
    }

    void StartCombine(ClickInputArgs clickInputArgs) {
        if (!clickInputArgs.TargetObj.TryGetComponent(out Product startProduct)) {
            return;
        }

        if (startProduct.Grid != GameManager.WorldGrid || startProduct.ShapeTags.Contains(ShapeTagID.NoCombine)) {
            TweenManager.Shake(startProduct);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }

        targetGrid = startProduct.Grid;
        combinedProduct = startProduct;
        lastSelectedProduct = combinedProduct;

        // Draw shape outline for combined shape
        previewRenderer.Render(combinedProduct.ShapeData);
        previewObj.SetActive(true);
        
        cooldownTimer.Start();
    }

    void ContinueCombine(ClickInputArgs clickInputArgs) {
        if (combinedProduct == null || cooldownTimer.IsTicking) return;
        
        if (!clickInputArgs.TargetObj.TryGetComponent(out Product hoveredProduct)) {
            return;
        }

        // Cutoff for not repeating on same shape
        if (hoveredProduct == lastSelectedProduct || hoveredProduct == combinedProduct) return;
        lastSelectedProduct = hoveredProduct;

        // Check adjacent to combine shape
        List<Vector3Int> combinedOffsetsToGridSpace =
            combinedProduct.ShapeData.ShapeOffsets.Select(offset => combinedProduct.ShapeData.RootCoord + offset).ToList();
        if (!hoveredProduct.ShapeData.IsAdjacentToCoords(combinedOffsetsToGridSpace)) {
            return;
        }

        // Check same color as combine shape
        if (hoveredProduct.ID.Color != combinedProduct.ID.Color) {
            TweenManager.Shake(hoveredProduct);
            SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
            return;
        }

        // Create new offsets for combined shape
        List<Vector3Int> newOffsets = new List<Vector3Int>(combinedProduct.ShapeData.ShapeOffsets);
        Vector3Int toProductRoot = hoveredProduct.ShapeData.RootCoord - combinedProduct.ShapeData.RootCoord;
        foreach (Vector3Int offset in hoveredProduct.ShapeData.ShapeOffsets) {
            newOffsets.Add(toProductRoot + offset);
        }

        // Remove original shape from grid
        targetGrid.RemoveShapeCells(hoveredProduct, false);

        // Create new shape keeping original shape root
        ShapeData newShapeData = new ShapeData {RootCoord = combinedProduct.ShapeData.RootCoord, ShapeOffsets = newOffsets};
        newShapeData.ID = ShapeData.DetermineID(newShapeData.ShapeOffsets);

        SO_Product productData = ProductFactory.Instance.CreateSOProduct(
            combinedProduct.ID.Color, combinedProduct.ID.Pattern, newShapeData
        );
        Product newProduct = ProductFactory.Instance.CreateProduct(
            productData, targetGrid.transform.TransformPoint(combinedProduct.ShapeData.RootCoord)
        );

        targetGrid.PlaceShapeNoValidate(newShapeData.RootCoord, newProduct);
        Ledger.AddStockedProduct(newProduct);

        // Destroy original shapes
        Ledger.RemoveStockedProduct(combinedProduct);
        ((IGridShape) combinedProduct).DestroyShape(false);
        Ledger.RemoveStockedProduct(hoveredProduct);
        ((IGridShape) hoveredProduct).DestroyShape(false);

        combinedProduct = newProduct;
        lastSelectedProduct = combinedProduct;

        // Draw shape outline for combined shape
        previewRenderer.Render(combinedProduct.ShapeData);
        previewObj.SetActive(true);
        
        cooldownTimer.Start();
    }

    void StopCombine(ClickInputArgs clickInputArgs) {
        combinedProduct = null;
        lastSelectedProduct = null;
        targetGrid = null;
        
        previewObj.SetActive(false);
        cooldownTimer.Reset();
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