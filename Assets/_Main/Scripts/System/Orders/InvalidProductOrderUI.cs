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
            GameObject o = product.ShapeTransform.gameObject;
            productDisplay = Instantiate(o, transform.position, o.transform.rotation);
            Product p = productDisplay.GetComponentInChildren<Product>();
            p.ShapeTransform.gameObject.layer = 0;
            p.ColliderTransform.gameObject.layer = 0;
            Destroy(p);
            productDisplay.transform.SetParent(transform);
            productDisplay.transform.localPosition = Vector3.zero;
            productDisplay.transform.localScale *= 0.5f;
        } else {
            Destroy(productDisplay);
        }
    }
}