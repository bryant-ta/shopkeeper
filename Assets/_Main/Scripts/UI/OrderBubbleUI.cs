using System;
using Orders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO: update UI to match new order
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

        // UpdateProducts(.OnProductFulfilled += UpdateProducts;
        displayedOrder.Timer.TickEvent += UpdateTimer;
    }

    void UpdateProducts(int quantityRemaining) {
        orderText.text = displayedOrder.ToString();
    }

    void UpdateTimer(float percent) {
        orderTimerBar.fillAmount = percent;
    }
}
