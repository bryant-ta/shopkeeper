using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Ledger : Singleton<Ledger> {
    public static Dictionary<ProductID, List<Product>> StockedProducts => stockedProducts;
    static Dictionary<ProductID, List<Product>> stockedProducts;

    public static Dictionary<Color, int> CellCountByColor => cellCountByColor;
    static Dictionary<Color, int> cellCountByColor;

    // TODO: make these auto properties

    public Ledger() {
        stockedProducts = new();
        cellCountByColor = new();
    }

    public static void AddStockedProduct(Product product) {
        if (stockedProducts.ContainsKey(product.ID)) {
            stockedProducts[product.ID].Add(product);
        } else {
            stockedProducts[product.ID] = new List<Product> {product};
        }

        AddColorCellCount(product.Color, product.ShapeData.Size);
    }
    public static void RemoveStockedProduct(Product product) {
        if (stockedProducts.ContainsKey(product.ID)) {
            stockedProducts[product.ID].Remove(product);
        }

        RemoveColorCellCount(product.Color, product.ShapeData.Size);
    }
    public static List<ProductID> GetStockedProductIDs() { return stockedProducts.Keys.ToList(); }
    public static Dictionary<ProductID, List<Product>> GetStockedProductsCopy() {
        Dictionary<ProductID, List<Product>> copy = new();

        // Deep copy
        foreach (KeyValuePair<ProductID, List<Product>> kvp in stockedProducts) {
            List<Product> newList = new List<Product>(kvp.Value);
            copy.Add(kvp.Key, newList);
        }

        return copy;
    }

    static void AddColorCellCount(Color color, int n) {
        if (cellCountByColor.ContainsKey(color)) {
            cellCountByColor[color] += n;
        } else {
            cellCountByColor[color] = n;
        }
    }
    static void RemoveColorCellCount(Color color, int n) {
        if (cellCountByColor.ContainsKey(color)) {
            cellCountByColor[color] -= n;
            if (cellCountByColor[color] < 0) { Debug.LogWarning("Attempted to decrease cell count of color below 0."); }
        } else {
            Debug.LogWarning("Attempted to decrease cell count of color that does not exist in ledger.");
        }
    }

    #region Debug

    public void PrintDictionary() {
        string s = "Cell Count by Color:\n\n";
        foreach (var pair in cellCountByColor) {
            s += $"{pair.Key}: {pair.Value}\n";
        }

        Debug.Log(s);
    }

    #endregion
}