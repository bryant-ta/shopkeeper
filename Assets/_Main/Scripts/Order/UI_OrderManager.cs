using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OrderManager))]
public class UI_OrderManager : MonoBehaviour {
    [SerializeField] List<UI_OrderBubble> orderBubbles;

    OrderManager orderMngr;

    void Awake() { orderMngr = GetComponent<OrderManager>(); }

    void Start() {
        orderMngr.OnNewActiveOrder += UpdateOrderBubble;
    }

    void UpdateOrderBubble(int activeOrderIndex, Order order) {
        if (order == null) { // no new active order, disable the bubble at index
            orderBubbles[activeOrderIndex].gameObject.SetActive(false);
            return;
        }
        
        orderBubbles[activeOrderIndex].gameObject.SetActive(true);
        orderBubbles[activeOrderIndex].DisplayNewOrder(order);
    }
}