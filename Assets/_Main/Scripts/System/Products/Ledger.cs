using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Ledger : Singleton<Ledger> {
    [field: SerializeField] public SO_ColorPalette ColorPaletteData { get; private set; }
    [field: SerializeField] public SO_PatternPalette PatternPaletteData { get; private set; }

    public static Dictionary<ProductID, List<Product>> StockedProducts { get; private set; }
    public static Dictionary<Color, int> CellCountByColor { get; private set; }

    public Ledger() {
        StockedProducts = new();
        CellCountByColor = new();
    }

    public static void AddStockedProduct(Product product) {
        if (product.ID.ShapeDataID != ShapeDataID.Custom) {
            if (StockedProducts.ContainsKey(product.ID)) {
                StockedProducts[product.ID].Add(product);
            } else {
                StockedProducts[product.ID] = new List<Product> {product};
            }
        }

        AddColorCellCount(product.ID.Color, product.ShapeData.Size);
    }
    public static void RemoveStockedProduct(Product product) {
        if (product.ID.ShapeDataID != ShapeDataID.Custom) {
            if (StockedProducts.ContainsKey(product.ID)) {
                StockedProducts[product.ID].Remove(product);
            }
        }

        RemoveColorCellCount(product.ID.Color, product.ShapeData.Size);
    }
    public static Dictionary<ProductID, List<Product>> GetStockedProductsCopy() {
        Dictionary<ProductID, List<Product>> copy = new();

        // Deep copy
        foreach (KeyValuePair<ProductID, List<Product>> kvp in StockedProducts) {
            List<Product> newList = new List<Product>(kvp.Value);
            copy.Add(kvp.Key, newList);
        }

        return copy;
    }

    static void AddColorCellCount(Color color, int n) { Util.DictIntAdd(CellCountByColor, color, n); }
    static void RemoveColorCellCount(Color color, int n) {
        if (CellCountByColor.ContainsKey(color)) {
            CellCountByColor[color] -= n;
            if (CellCountByColor[color] < 0) {
                Debug.LogWarning("Attempted to decrease cell count of color below 0.");
                CellCountByColor[color] = 0;
            }
        } else {
            Debug.LogWarning("Attempted to decrease cell count of color that does not exist in ledger.");
        }
    }

    #region Debug

    public void PrintDictionary() {
        string s = "Cell Count by Color:\n\n";
        foreach (var pair in CellCountByColor) {
            s += $"{pair.Key}: {pair.Value}\n";
        }

        Debug.Log(s);
    }

    #endregion
}