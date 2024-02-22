using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_OrderBubble : MonoBehaviour {
    [SerializeField] TextMeshProUGUI orderText;
    [SerializeField] Image orderTimerBar;

    Order displayedOrder;

    CanvasGroup canvasGroup;

    void Awake() { canvasGroup = GetComponent<CanvasGroup>(); }

    public void DisplayNewOrder(Order order) {
        if (displayedOrder != null && displayedOrder != order) {
            displayedOrder.Timer.TickEvent -= UpdateTimer;
            displayedOrder.OnProductFulfilled -= UpdateProducts;
        }

        displayedOrder = order;

        UpdateProducts();
        
        displayedOrder.OnProductFulfilled += UpdateProducts;
        displayedOrder.Timer.TickEvent += UpdateTimer;
    }

    void UpdateProducts() {
        orderText.text = displayedOrder.ToString();
    }

    void UpdateTimer(float percent) {
        orderTimerBar.fillAmount = percent;
    }

    public void SetAlpha(float value) {
        canvasGroup.alpha = value;
    }
}
