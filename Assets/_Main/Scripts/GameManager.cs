using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    [Header("Debug")]
    public bool Debug;

    [Header("World Grid")]
    [SerializeField] Grid worldGrid;
    public static Grid WorldGrid => _worldGrid;
    static Grid _worldGrid;

    [Header("Gold")]
    [SerializeField] int initialGold;
    [SerializeField] int gold;
    public int Gold => gold;
    public Action<DeltaArgs> OnModifyMoney;

    // Stocked Products
    public static Dictionary<ProductID, List<Product>> StockedProducts => stockedProducts;
    static Dictionary<ProductID, List<Product>> stockedProducts;

    void Awake() {
        // Required to reset every Play mode start because static
        _worldGrid = worldGrid;
        stockedProducts = new();
    }

    void Start() {
        if (Debug) { DebugTasks(); }
        
        ModifyGold(initialGold);
    }

    void DebugTasks() {
        // Initialize items with any IStackable in transform children
        // In game, stacks containing items should only be created after game start and inited on instantiation
        List<Stack> preMadeStacks = FindObjectsByType<Stack>(FindObjectsSortMode.None).ToList();
        for (int i = 0; i < preMadeStacks.Count; i++) {
            preMadeStacks[i].Init();
        }
    }

    #region Gold

    /// <summary>
    /// Applies delta to current coin value. 
    /// </summary>
    /// <param name="delta">(+/-)</param>
    public bool ModifyGold(int delta) {
        int newGold = gold + delta;
        if (newGold < 0) {
            return false;
        }

        gold = newGold;
        OnModifyMoney?.Invoke(new DeltaArgs {NewValue = newGold, DeltaValue = delta});

        return true;
    }

    #endregion

    #region Stocked Products

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

    #endregion
}