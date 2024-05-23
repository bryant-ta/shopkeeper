using System;
using System.Collections.Generic;
using Orders;
using TMPro;
using UnityEngine;

public class RequirementsDisplayUI : MonoBehaviour {
    [SerializeField] List<Transform> requirementDisplayPoints;
    [SerializeField] List<TextMeshProUGUI> remainingQuantityTexts;

    void Awake() {
        Orderer orderer = GetComponentInParent<Orderer>();
        orderer.OnOrderSet += SetRequirements;
    }

    void SetRequirements(Order order) {
        for (int i = 0; i < order.Requirements.Count; i++) {
            Requirement req = order.Requirements[i];
            if (req.Color == null || req.ShapeDataID == null) {
                continue;
            }
            
            requirementDisplayPoints[i].gameObject.SetActive(true);

            Color c = req.Color ?? Color.clear;
            ShapeDataID s = req.ShapeDataID ?? ShapeDataID.Custom;

            SO_Product productData = ProductFactory.Instance.CreateSOProduct(
                c,
                Pattern.None, // TEMP: until implementing pattern
                ShapeDataLookUp.LookUp[s]
            );
            Product product = ProductFactory.Instance.CreateProduct(productData, Vector3.zero);
            product.ShapeTransform.gameObject.layer = 0;
            product.ColliderTransform.gameObject.layer = 0;
            GameObject productDisplay = product.gameObject;
            Destroy(product);
            productDisplay.transform.SetParent(requirementDisplayPoints[i]);
            productDisplay.transform.localPosition = Vector3.zero;
            productDisplay.transform.localScale *= 0.5f;
            
            UpdateQuantityText(i, req.QuantityUntilTarget());
        }
        
        order.OnProductFulfilled += UpdateQuantityText;
    }

    public void UpdateQuantityText(int index, int remainingQuantity) { remainingQuantityTexts[index].text = remainingQuantity.ToString(); }
}