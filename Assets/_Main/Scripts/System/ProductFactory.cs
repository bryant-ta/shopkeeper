using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProductFactory : Singleton<ProductFactory> {
    [SerializeField] GameObject productBase;

    public Dictionary<ProductID, SO_Product> ProductLookUp { get; private set; }
    public Dictionary<ShapeDataID, GameObject> ShapeLookUp { get; private set; }

    void Awake() {
        ProductLookUp = new();
        LoadProducts();
    }

    public Product CreateProduct(SO_Product productData, ShapeDataID shapeDataID) {
        GameObject productObj = Instantiate(ShapeLookUp[shapeDataID], Vector3.zero, Quaternion.identity);
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
            ProductLookUp[productData.ID] = productData;
        }
    }
    
    void LoadShapes() {
        // Load shapes from Resources
        GameObject[] shapeObjs = Resources.LoadAll<GameObject>("Shapes");
        foreach (GameObject shapeObj in shapeObjs) {
            ShapeDataID id = shapeObj.GetComponentInChildren<IGridShape>().ShapeDataID;
            ShapeLookUp[id] = shapeObj;
        }
    }
}