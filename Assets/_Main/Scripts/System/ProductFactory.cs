using UnityEngine;

public class ProductFactory : Singleton<ProductFactory> {
    [SerializeField] GameObject productBase;

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