using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelInitializer : MonoBehaviour {
    [SerializeField] int numStacks;
    [SerializeField] MinMax numCountPerStack;

    int maxColorIndex;

    void Awake() { maxColorIndex = DifficultyManager.Instance.GetInitialMaxColorIndex(); }

    public void InitializeLevel() {
        if (DebugManager.DebugMode && !DebugManager.Instance.DoLevelInitialize) return;
        
        // Place 1x1s randomly at start of level
        Grid grid = GameManager.WorldGrid;
        for (int i = 0; i < numStacks; i++) {
            Vector3Int stackPos = new Vector3Int(Random.Range(grid.MinX, grid.MaxX), grid.MinY, Random.Range(grid.MinZ, grid.MaxZ));
            int stackCount = Math.Min(Random.Range(numCountPerStack.Min, numCountPerStack.Max + 1), grid.Height);

            for (int y = 0; y < stackCount; y++) {
                SO_Product productData = ProductFactory.Instance.CreateSOProduct(
                    Ledger.Instance.ColorPaletteData.Colors[Random.Range(0, maxColorIndex)],
                    Pattern.None, // TEMP: until implementing pattern
                    ShapeDataLookUp.LookUp(ShapeDataID.O1)
                );

                Product product = ProductFactory.Instance.CreateProduct(productData, stackPos + new Vector3Int(0, y, 0));

                for (int tries = 3; tries > 0; tries--) {
                    if (grid.PlaceShape(stackPos + new Vector3Int(0, y, 0), product)) {
                        Ledger.AddStockedProduct(product);
                        break;
                    }

                    stackPos = new Vector3Int(Random.Range(grid.MinX, grid.MaxX), grid.MinY, Random.Range(grid.MinZ, grid.MaxZ));
                }
            }
        }
    }
}