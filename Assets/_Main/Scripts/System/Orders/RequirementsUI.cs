using System.Collections.Generic;
using Orders;
using TMPro;
using UnityEngine;

public class RequirementsUI : MonoBehaviour {
    [SerializeField] List<Transform> requirementDisplayPoints;
    [SerializeField] List<TextMeshProUGUI> remainingQuantityTexts;

    void Awake() {
        Orderer orderer = GetComponentInParent<Orderer>();
        orderer.OnOrderSet += DisplayRequirements;
    }

    void DisplayRequirements(Order order) {
        for (int i = 0; i < order.Requirements.Count; i++) {
            Requirement req = order.Requirements[i];
            if (req.Color == null || req.ShapeDataID == null) {
                
                
                
                
                
                
                continue;
            }

            requirementDisplayPoints[i].gameObject.SetActive(true);

            Color color = req.Color ?? Color.clear;
            Pattern pattern = req.Pattern ?? Pattern.None;
            ShapeDataID shapeDataID = req.ShapeDataID ?? ShapeDataID.Custom;
            // TODO: handle null (default) values

            GameObject productDisplay = ProductFactory.Instance.CreateProductDisplay(color, pattern, ShapeDataLookUp.LookUp(shapeDataID));
            productDisplay.transform.SetParent(requirementDisplayPoints[i]);
            productDisplay.transform.localPosition = Vector3.zero;
            productDisplay.transform.localScale *= 0.5f;

            UpdateQuantityText(i, req.QuantityUntilTarget());
        }

        order.OnProductFulfilled += UpdateQuantityText;
    }

    public void UpdateQuantityText(int index, int remainingQuantity) { remainingQuantityTexts[index].text = remainingQuantity.ToString(); }
}