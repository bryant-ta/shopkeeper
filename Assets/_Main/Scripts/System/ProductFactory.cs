using System.Collections.Generic;
using Tags;
using UnityEngine;

public class ProductFactory : Singleton<ProductFactory> {
    [SerializeField] GameObject productBase;
    [SerializeField] GameObject stackBase;

    public Dictionary<ProductID, SO_Product> ProductLookUp { get; private set; }

    void Awake() {
        ProductLookUp = new();
        LoadProducts();
    }

    public Product CreateProduct(SO_Product productData) {
        GameObject productObj = Instantiate(productBase, Vector3.zero, Quaternion.identity).transform.GetChild(0).gameObject;
        Product product = productObj.GetComponent<Product>();
        product.Init(productData);

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
            ProductLookUp[productData.ID] = productData;
        }
    }
}