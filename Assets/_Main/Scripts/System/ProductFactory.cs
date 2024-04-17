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

    public Product CreateProduct(SO_Product productData, ProductTags productTags, ShapeTags shapeTags) {
        GameObject productObj = Instantiate(productBase, Vector3.zero, Quaternion.identity).transform.GetChild(0).gameObject;
        Product product = productObj.GetComponent<Product>();
        product.Init(productData, productTags);
        IGridShape shape = productObj.GetComponent<IGridShape>();
        shape.Init(shapeTags);

        return product;
    }

    public BoxProduct CreateRandomProduct() {
        
        
        return null;
    }

    public Stack CreateStack() {
        GameObject newStackObj = Instantiate(stackBase, Vector3.zero, Quaternion.identity);
        return newStackObj.GetComponent<Stack>();
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