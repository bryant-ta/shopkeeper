using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(OrderManager))]
public class UI_OrderManager : MonoBehaviour {
    [SerializeField] List<UI_OrderBubble> orderBubbles;

    OrderManager orderMngr;

    void Awake() {
        orderMngr = GetComponent<OrderManager>();

        orderMngr.OnNewActiveOrder += UpdateOrderBubble;
    }

    void UpdateOrderBubble(int activeOrderIndex, Order order) {
        if (order == null) { // no new active order, disable the bubble at index
            DOVirtual.Float(1f, 0f, Constants.AnimOrderBubbleFadeDur, alpha => orderBubbles[activeOrderIndex].SetAlpha(alpha));
            return;
        }

        DOVirtual.Float(0f, 1f, Constants.AnimOrderBubbleFadeDur, alpha => orderBubbles[activeOrderIndex].SetAlpha(alpha));
        orderBubbles[activeOrderIndex].DisplayNewOrder(order);
    }
}