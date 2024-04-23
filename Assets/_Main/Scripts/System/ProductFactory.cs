using System.Collections.Generic;
using UnityEngine;

public class ProductFactory : Singleton<ProductFactory> {
    [SerializeField] GameObject productBase;

    public Dictionary<ProductID, SO_Product> ProductLookUp { get; private set; }

    void Awake() {
        ProductLookUp = new();
        LoadProducts();
    }

    public Product CreateProduct(SO_Product productData) {
        GameObject productObj = Instantiate(productBase, Vector3.zero, Quaternion.identity);
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
    public Product CreateProduct(GameObject productObj) {
        Product product = productObj.GetComponentInChildren<Product>();
        if (product == null) {
            Debug.LogError("Unable to find Product component in product prefab.");
            return null;
        }
        // product will self init since productData will be set already.

        return product;
    }

    public Product CreateRandomProduct() {
        
        
        return null;
    }

    void LoadProducts() {
        // TODO: Load recipes created in inspector
        
        // Load recipes from Resources
        SO_Product[] productDatas = Resources.LoadAll<SO_Product>("Products");
        foreach (SO_Product productData in productDatas) {
            ProductLookUp[productData.ProductID] = productData;
        }
    }
}