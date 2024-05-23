using System;
using System.Collections.Generic;
using Orders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderBubbleUI : MonoBehaviour {
    [SerializeField] Transform shapeDisplay;
    [SerializeField] TextMeshProUGUI remainingQuantity;
    Dictionary<Requirement, TextMeshProUGUI> quantityTextByRequirement;

    public float Alpha {
        get => canvasGroup.alpha;
        set => canvasGroup.alpha = value;
    }

    Order displayedOrder;

    CanvasGroup canvasGroup;

    void Awake() { canvasGroup = GetComponent<CanvasGroup>(); }

    public void DisplayOrder(Order order) {
        displayedOrder = order;

        // order.OnProductFulfilled += UpdateRequirement;
    }

    void UpdateRequirement(int quantityRemaining) {
        // orderText.text = displayedOrder.ToString();
    }
}
