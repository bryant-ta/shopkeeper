using UnityEngine;

public class InvalidProductOrderUI : MonoBehaviour {
    GameObject productDisplay;
    Canvas displayCanvas;

    void Awake() {
        Orderer orderer = GetComponentInParent<Orderer>();
        orderer.OnInvalidProductSet += DisplayInvalidProduct;
        orderer.OnInvalidProductUnset += HideInvalidProduct;
        
        displayCanvas = GetComponentInChildren<Canvas>(true);
    }

    // TODO: make this more efficient than destroying/creating gameObjects
    void DisplayInvalidProduct(ProductID productID) {
        productDisplay = ProductFactory.Instance.CreateProductDisplay(productID.Color, productID.Pattern, productID.ShapeData);
        productDisplay.transform.SetParent(transform);
        productDisplay.transform.localPosition = Vector3.zero;
        productDisplay.transform.localScale *= 0.5f;

        displayCanvas.gameObject.SetActive(true);
    }
    void HideInvalidProduct() {
        Destroy(productDisplay);
        displayCanvas.gameObject.SetActive(false);
    }
}