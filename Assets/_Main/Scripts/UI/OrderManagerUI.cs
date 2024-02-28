using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(OrderManager))]
public class OrderManagerUI : MonoBehaviour {
    [SerializeField] List<OrderBubbleUI> orderBubbles;

    OrderManager orderMngr;

    void Awake() {
        orderMngr = GetComponent<OrderManager>();

        orderMngr.OnNewActiveOrder += UpdateOrderBubble;
    }

    void UpdateOrderBubble(int activeOrderIndex, Order order) {
        if (order == null) { // no new active order, disable the bubble at index
            DOVirtual.Float(
                orderBubbles[activeOrderIndex].Alpha, 0f, TweenManager.OrderBubbleFadeDur,
                alpha => orderBubbles[activeOrderIndex].Alpha = alpha
            );
            return;
        }

        DOVirtual.Float(
            orderBubbles[activeOrderIndex].Alpha, 1f, TweenManager.OrderBubbleFadeDur, alpha => orderBubbles[activeOrderIndex].Alpha = alpha
        );
        orderBubbles[activeOrderIndex].DisplayNewOrder(order);
    }
}