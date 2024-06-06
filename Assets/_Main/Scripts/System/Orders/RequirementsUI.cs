using System.Collections.Generic;
using MK.Toon;
using Orders;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LookAtOnCameraRotation))]
public class RequirementsUI : MonoBehaviour {
    [SerializeField] List<RequirementDisplayUI> requirementDisplays;

    void Awake() {
        Orderer orderer = GetComponentInParent<Orderer>();
        orderer.OnOrderStarted += Display;
        orderer.OnOrderFinished += Hide;

        // Hide all requirement displays, to be shown as needed
        for (int i = 0; i < requirementDisplays.Count; i++) {
            requirementDisplays[i].gameObject.SetActive(false);
            requirementDisplays[i].RemainingQuantityCanvas.gameObject.SetActive(false);
            requirementDisplays[i].ShapelessDisplayObj.SetActive(false);
        }
    }

    void Display(Order order) {
        for (int i = 0; i < order.Requirements.Count; i++) {
            Requirement req = order.Requirements[i];
            
            // Extract requirement properties
            Color color;
            if (req.Color != null) {
                color = req.Color ?? Color.clear;
            } else {
                color = Color.gray;
            }

            ShapeData shapeData = null;
            if (req.ShapeDataID != null) {
                ShapeDataID shapeDataID = req.ShapeDataID ?? ShapeDataID.Custom;
                shapeData = ShapeDataLookUp.LookUp(shapeDataID);
            }

            RequirementDisplayUI reqDisplay = requirementDisplays[i];
            reqDisplay.gameObject.SetActive(true);

            // TODO: handle pattern
            Pattern pattern = req.Pattern ?? Pattern.None;
            // TODO: handle null (default) values

            // Create display obj
            if (shapeData != null) {
                GameObject productDisplay = ProductFactory.Instance.CreateProductDisplay(color, pattern, shapeData);
                productDisplay.transform.SetParent(reqDisplay.transform);
                productDisplay.transform.localPosition = Vector3.zero;
                productDisplay.transform.localScale *= 0.5f;
                
                reqDisplay.RemainingQuantityCanvas.gameObject.SetActive(true);
            } else {
                Material mat = reqDisplay.ShapelessDisplayObj.GetComponent<MeshRenderer>().material;
                Properties.albedoColor.SetValue(mat, color);
                
                reqDisplay.ShapelessDisplayObj.SetActive(true);
            }

            UpdateQuantityText(i, req.QuantityUntilTarget());
        }
        
        GetComponent<LookAtOnCameraRotation>().RotateWithCamera();

        order.OnProductFulfilled += UpdateQuantityText;
    }

    void Hide(Order order) {
        for (int i = 0; i < requirementDisplays.Count; i++) {
            requirementDisplays[i].gameObject.SetActive(false);
        }
    }

    public void UpdateQuantityText(int index, int remainingQuantity) { requirementDisplays[index].RemainingQuantityText.text = remainingQuantity.ToString(); }
}