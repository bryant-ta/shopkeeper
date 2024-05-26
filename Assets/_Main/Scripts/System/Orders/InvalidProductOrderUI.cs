using UnityEngine;

public class InvalidProductOrderUI : MonoBehaviour {
    GameObject productDisplay;
    
    void Awake() {
        Orderer orderer = GetComponentInParent<Orderer>();
        orderer.OnInvalidProductSet += DisplayInvalidProduct;
    }

    // TODO: make this more efficient than destroying/creating gameObjects
    void DisplayInvalidProduct(Product product) {
        if (product) {
            productDisplay = ProductFactory.Instance.CreateProductDisplay(product.ID.Color, product.ID.Pattern, product.ShapeData);
            productDisplay.transform.SetParent(transform);
            productDisplay.transform.localPosition = Vector3.zero;
            productDisplay.transform.localScale *= 0.5f;
        } else {
            // Destroy(productDisplay);
        }
    }
}