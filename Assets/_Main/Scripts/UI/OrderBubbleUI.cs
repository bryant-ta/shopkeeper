using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderBubbleUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI orderText;
    [SerializeField] Image orderTimerBar;

    public float Alpha {
        get => canvasGroup.alpha;
        set => canvasGroup.alpha = value;
    }

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
}
