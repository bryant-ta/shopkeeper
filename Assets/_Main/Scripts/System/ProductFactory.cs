using MK.Toon;
using UnityEngine;

public class ProductFactory : Singleton<ProductFactory> {
    [SerializeField] GameObject productBase;
    [SerializeField] GameObject productDisplayBase;

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
    
    // Just creates the mesh of a product, used mainly for displaying a product.
    public GameObject CreateProductDisplay(Color color, Pattern pattern, ShapeData shapeData) {
        GameObject productDisplayObj = Instantiate(productDisplayBase);
        VoxelMeshGenerator.Generate(productDisplayObj, shapeData, false);
        
        Material mat = productDisplayObj.GetComponent<MeshRenderer>().material;
        Properties.albedoColor.SetValue(mat, color);
        // MK.Toon.Properties.sketchMap.SetValue(mat, pattern); // TODO: Pattern lookup

        return productDisplayObj;
    }

    // public Product CreateRandomProduct(Vector3 position) {
    //     Array ids = Enum.GetValues(typeof(ProductID)); // TEMP: until replace ProductID, prob with int id
    //     SO_Product productData = Instantiate(ProductDataLookUp[(ProductID)ids.GetValue(Random.Range(0, ids.Length))]);
    //     GameObject productObj = Instantiate(productBase, position, Quaternion.identity);
    //     Product product = productObj.GetComponentInChildren<Product>();
    //     if (product == null) {
    //         Debug.LogError("Unable to find Product component in shape base object.");
    //         return null;
    //     }
    //     product.Init(productData);
    //
    //     return product;
    // }

    public SO_Product CreateSOProduct(Color color, Pattern pattern, ShapeData shapeData) {
        SO_Product productData = ScriptableObject.CreateInstance<SO_Product>();

        productData.ID = new ProductID(color, pattern, shapeData);
        productData.ShapeData = shapeData;
        productData.MoveTagIDs = new();
        productData.PlaceTagIDs = new();

        return productData;
    }
}