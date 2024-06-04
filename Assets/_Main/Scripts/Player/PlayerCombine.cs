using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombine : MonoBehaviour, IPlayerTool {
    [SerializeField] GameObject previewObj; // a obj with ShapeOutlineRenderer component
    ShapeOutlineRenderer previewRenderer;

    Grid targetGrid;

    IGridShape selectedShape;
    List<Product> combinedProducts = new();
    ShapeData newShapeData;

    void Awake() {
        previewRenderer = previewObj.GetComponent<ShapeOutlineRenderer>();
        if (previewRenderer == null) {
            Debug.LogError("Combine preview obj is missing ShapeOutlineRenderer component.");
            return;
        }
    }
    
    void Combine(ClickInputArgs clickInputArgs) {
        if (selectedShape == null || newShapeData == null) return;
        
        Product selectedProduct = Util.GetProductFromShape(selectedShape);
        if (selectedProduct == null) return;
        
        // Remove original shapes from grid
        foreach (Product product in combinedProducts) {
            targetGrid.RemoveShapeCells(product, false);
        }
        
        // Create new shape using selected shape root as new root
        SO_Product productData = ProductFactory.Instance.CreateSOProduct(
            selectedProduct.ID.Color, selectedProduct.ID.Pattern, newShapeData
        );
        Product newProduct = ProductFactory.Instance.CreateProduct(productData, targetGrid.transform.TransformPoint(newShapeData.RootCoord));

        targetGrid.PlaceShapeNoValidate(newShapeData.RootCoord, newProduct);
        Ledger.AddStockedProduct(newProduct);
        
        // Destroy original shapes
        foreach (Product product in combinedProducts) {
            Ledger.RemoveStockedProduct(product);
            ((IGridShape)product).DestroyShape();
        }
        selectedShape = null;
        combinedProducts.Clear();
        newShapeData = null;
    }

    // works in target grid local space, positions preview in world space!
    IGridShape lastSelectedShape;
    void CombinePreview(ClickInputArgs clickInputArgs) {
        targetGrid = Ref.Player.SelectTargetGrid(clickInputArgs);
        if (targetGrid == null) {
            previewObj.SetActive(false);
            return;
        }

        // Formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal =
            targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedShapeCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitAntiNormal + new Vector3(0.5f, 0, 0.5f));

        selectedShape = targetGrid.SelectPosition(selectedShapeCellCoord);
        if (selectedShape == null) {
            lastSelectedShape = null;
            previewObj.SetActive(false);
            return;
        }
        
        // Cutoff for not repeating on same shape
        if (selectedShape == lastSelectedShape) return;
        lastSelectedShape = selectedShape;
        
        Vector3Int selectedRoot = selectedShape.ShapeData.RootCoord;
        Product selectedProduct = Util.GetProductFromShape(selectedShape);
        if (selectedProduct == null) return;

        // Find adjacent shapes of same color
        combinedProducts.Clear();
        combinedProducts.Add(selectedProduct);
        foreach (Vector3Int offset in selectedShape.ShapeData.ShapeOffsets) {
            for (int i = 0; i < 4; i++) {
                IGridShape adjacentShape = targetGrid.SelectPosition(selectedRoot + offset + DirectionData.DirectionVectorsInt[i]);

                if (adjacentShape != null && adjacentShape != selectedShape && !combinedProducts.Contains(adjacentShape)) {
                    Product adjacentProduct = Util.GetProductFromShape(adjacentShape);
                    if (adjacentProduct == null) return;

                    if (adjacentProduct.ID.Color == selectedProduct.ID.Color) {
                        combinedProducts.Add(adjacentProduct);
                    }
                }
            }
        }
        
        // Create new offsets for combined shape
        List<Vector3Int> newOffsets = new();
        foreach (Product product in combinedProducts) {
            Vector3Int toProductRoot = product.ShapeData.RootCoord - selectedRoot;
            foreach (Vector3Int offset in product.ShapeData.ShapeOffsets) {
                newOffsets.Add(toProductRoot + offset);
            }
        }
        
        // TODO: possible problem if shape is rotated/ offsets could repeat?
        
        // Draw shape outline for combined shape
        newShapeData = new ShapeData {RootCoord = selectedRoot, ShapeOffsets = newOffsets};
        newShapeData.ID = ShapeData.DetermineID(newShapeData.ShapeOffsets);
        
        previewRenderer.Render(newShapeData);
        previewObj.SetActive(true);
    }

    public void Equip() {
        Ref.Player.PlayerInput.InputPrimaryDown += Combine;
        Ref.Player.PlayerInput.InputPoint += CombinePreview;
    }
    public void Unequip() {
        Ref.Player.PlayerInput.InputPrimaryDown -= Combine;
        Ref.Player.PlayerInput.InputPoint -= CombinePreview;
        previewObj.SetActive(false);
    }
}