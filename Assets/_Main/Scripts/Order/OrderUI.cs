using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(OrderManager))]
public class OrderUI : MonoBehaviour {
    [SerializeField] List<TextMeshProUGUI> orderTexts;

    OrderManager orderMngr;

    void Awake() { orderMngr = GetComponent<OrderManager>(); }

    void Start() { orderMngr.OnNewActiveOrder += UpdateOrderBubble; }

    void UpdateOrderBubble(int activeOrderIndex, Order order) {
        // TODO: adding more order bubble UIs with more active orders
        // while (activeOrderIndex > orderTexts.Count - 1) {
        //     break;
        // }
        if (activeOrderIndex > orderTexts.Count - 1) {
            Debug.LogError("Unable to update order bubble: active order index is greater than number of existing bubbles");
            return;
        }

        orderTexts[activeOrderIndex].text = order.ToString();
    }
}