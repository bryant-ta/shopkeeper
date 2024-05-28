using UnityEngine;

public class InvalidProductOrderUI : MonoBehaviour {
    GameObject productDisplay;
    Canvas displayCanvas;
    
    void Awake() {
        Orderer orderer = GetComponentInParent<Orderer>();
        orderer.OnInvalidProductSet += DisplayInvalidProduct;

        displayCanvas = GetComponentInChildren<Canvas>(true);
    }

    // TODO: make this more efficient than destroying/creating gameObjects
    void DisplayInvalidProduct(Product product) {
        if (product) {
            productDisplay = ProductFactory.Instance.CreateProductDisplay(product.ID.Color, product.ID.Pattern, product.ShapeData);
            productDisplay.transform.SetParent(transform);
            productDisplay.transform.localPosition = Vector3.zero;
            productDisplay.transform.localScale *= 0.5f;
            
            displayCanvas.gameObject.SetActive(true);
        } else {
            Destroy(productDisplay);
            displayCanvas.gameObject.SetActive(false);
        }
    }
}