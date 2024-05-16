using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ledger : Singleton<Ledger> {
    public static Dictionary<ProductID, List<Product>> StockedProducts => stockedProducts;
    static Dictionary<ProductID, List<Product>> stockedProducts;
    
    public static Dictionary<Color, int> CellCountByColor => cellCountByColor;
    static Dictionary<Color, int> cellCountByColor;

    public Ledger() {
        stockedProducts = new();
    }
    
    public static void AddStockedProduct(Product product) {
        if (stockedProducts.ContainsKey(product.ID)) {
            stockedProducts[product.ID].Add(product);
        }
        else {
            stockedProducts[product.ID] = new List<Product> {product};
        }
    }
    public static void RemoveStockedProduct(Product product) {
        if (stockedProducts.ContainsKey(product.ID)) {
            stockedProducts[product.ID].Remove(product);
        }

        if (stockedProducts[product.ID].Count == 0) {
            stockedProducts.Remove(product.ID);
        }
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
}
