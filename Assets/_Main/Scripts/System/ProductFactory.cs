using System.Collections.Generic;
using UnityEngine;

public class ProductFactory : Singleton<ProductFactory> {
    [SerializeField] GameObject productBase;

    public Dictionary<ProductID, SO_Product> ProductDataLookUp { get; private set; }
    public Dictionary<ShapeDataID, List<SO_Product>> ShapeDataIDToProductDataLookUp { get; private set; }

    void Awake() {
        ProductDataLookUp = new();
        ShapeDataIDToProductDataLookUp = new();
        LoadProducts();
        LoadShapeDataToProducts();
    }

    public Product CreateProduct(SO_Product productData, Vector3 position) {
        GameObject productObj = Instantiate(productBase, position, Quaternion.identity);
        Product product = productObj.GetComponentInChildren<Product>();
        if (product == null) {
            Debug.LogError("Unable to find Product component in shape base object.");
            return null;
        }
        product.Init(productData);

        return product;
    }
    
    /// <summary>
    /// Instantiates an instance of a product prefab object. The prefab should have a Product component with ShapeData and SO_ProductData set.
    /// </summary>
    // public Product CreateProduct(GameObject productObj) {
    //     Product product = productObj.GetComponentInChildren<Product>();
    //     if (product == null) {
    //         Debug.LogError("Unable to find Product component in product prefab.");
    //         return null;
    //     }
    //     // product will self init since productData will be set already.
    //
    //     return product;
    // }

    public Product CreateRandomProduct() {
        
        
        return null;
    }

    void LoadProducts() {
        // TODO: Load recipes created in inspector
        
        // Load recipes from Resources
        SO_Product[] productDatas = Resources.LoadAll<SO_Product>("Products");
        foreach (SO_Product productData in productDatas) {
            ProductDataLookUp[productData.ProductID] = productData;
        }
    }
    
    void LoadShapeDataToProducts() {
        if (ProductDataLookUp == null || ProductDataLookUp.Count == 0) {
            Debug.LogError("Must load products to ProductDataLookUp before using this function.");
            return;
        }

        foreach (KeyValuePair<ProductID,SO_Product> kv in ProductDataLookUp) {
            ShapeDataID id = kv.Value.ShapeData.ID;
            if (!ShapeDataIDToProductDataLookUp.ContainsKey(id)) {
                ShapeDataIDToProductDataLookUp[id] = new List<SO_Product> {kv.Value};
            } else {
                ShapeDataIDToProductDataLookUp[id].Add(kv.Value);
            }
        }
    }
}