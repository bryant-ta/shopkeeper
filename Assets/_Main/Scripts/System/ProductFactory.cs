using System.Collections.Generic;
using MK.Toon;
using UnityEngine;

public class ProductFactory : Singleton<ProductFactory> {
    [SerializeField] GameObject productBase;
    [SerializeField] GameObject productDisplayBase;

    /// <summary>
    /// Creates Product object from SO_Product.
    /// </summary>
    /// <remarks>NOTE: references SO_Product input directly, so input instance will be shared if input multiple times.</remarks>
    public Product CreateProduct(SO_Product productData, Vector3 position) {
        // SO_Product productDataInst = Instantiate(productData);
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

    /// <summary>
    /// Creates SO_Product instance from params.
    /// </summary>
    /// <remarks>NOTE: Automatically creates new instance of ShapeData to prevent Products sharing the same instance.</remarks>
    public SO_Product CreateSOProduct(Color color, Pattern pattern, ShapeData shapeData, List<ShapeTagID> shapeTags = null) {
        SO_Product productData = ScriptableObject.CreateInstance<SO_Product>();

        productData.ID = new ProductID(color, pattern, shapeData);
        productData.ShapeData = new ShapeData(shapeData);
        if (shapeTags != null && shapeTags.Count != 0) {
            productData.ShapeTagIDs = new(shapeTags);
        }

        return productData;
    }
}