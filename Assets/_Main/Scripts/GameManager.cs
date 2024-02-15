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

    [Header("Coins")]
    [SerializeField] int coins;
    public int Coins => coins;
    
    // Stocked Products
    public static Dictionary<ProductID, List<Product>> StockedProducts => stockedProducts;
    static Dictionary<ProductID, List<Product>> stockedProducts = new();

    void Awake() {
        _worldGrid = worldGrid;
    }

    void Start() {
        if (Debug) { DebugTasks(); }
    }

    void DebugTasks() {
        // Initialize items with any IStackable in transform children
        // In game, stacks containing items should only be created after game start and inited on instantiation
        List<Stack> preMadeStacks = FindObjectsByType<Stack>(FindObjectsSortMode.None).ToList();
        for (int i = 0; i < preMadeStacks.Count; i++) {
            preMadeStacks[i].Init();
        }
    }

    #region Coins
    
    /// <summary>
    /// Applies delta to current coin value. 
    /// </summary>
    /// <param name="delta">(+/-)</param>
    public bool ModifyCoins(int delta) {
        int newCoins = coins + delta;
        if (newCoins < 0) {
            return false;
        }

        coins = newCoins;
        // TODO: modify coins event
        
        return true;
    }
    
    #endregion
    
    #region Stocked Products
    
    public static void AddStockedProduct(Product product) {
        if (stockedProducts.ContainsKey(product.ID)) { stockedProducts[product.ID].Add(product); }
        else { stockedProducts[product.ID] = new List<Product> {product}; }
    }
    public static void RemoveStockedProduct(Product product) {
        if (stockedProducts.ContainsKey(product.ID)) { stockedProducts[product.ID].Remove(product); }

        if (stockedProducts[product.ID].Count == 0) { stockedProducts.Remove(product.ID); }
    }
    public static List<ProductID> GetStockedProductIDs() {
        return stockedProducts.Keys.ToList();
    }
    
    #endregion
}