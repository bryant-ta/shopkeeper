using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(OrderManager))]
public class OrderManagerUI : MonoBehaviour {
    [SerializeField] List<OrderBubbleUI> orderBubbles;
    
    [Header("Animation")]
    [SerializeField] float animOrderBubbleFadeDur; // anim duration of order bubble fade in/out

    OrderManager orderMngr;

    void Awake() {
        orderMngr = GetComponent<OrderManager>();

        orderMngr.OnNewActiveOrder += UpdateOrderBubble;
    }

    void UpdateOrderBubble(int activeOrderIndex, Order order) {
        if (order == null) { // no new active order, disable the bubble at index
            DOVirtual.Float(1f, 0f, animOrderBubbleFadeDur, alpha => orderBubbles[activeOrderIndex].SetAlpha(alpha));
            return;
        }

        DOVirtual.Float(0f, 1f, animOrderBubbleFadeDur, alpha => orderBubbles[activeOrderIndex].SetAlpha(alpha));
        orderBubbles[activeOrderIndex].DisplayNewOrder(order);
    }
}