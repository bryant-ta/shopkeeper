using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OrderManager))]
public class OrderManagerUI : MonoBehaviour {
    [SerializeField] List<OrderBubbleUI> orderBubbles;
    [SerializeField] List<OrderDisplayUI> orderDisplays;

    OrderManager orderMngr;

    // TEMP: until real book ui framework
    [SerializeField] Transform orderDisplayOpenPos;
    [SerializeField] Transform orderDisplayClosePos;
    
    // TEMP: debug show remaining orders for playtest
    [SerializeField] TextMeshProUGUI numRemainingOrderText;

    void Awake() {
        orderMngr = GetComponent<OrderManager>();

        orderMngr.OnActiveOrderChanged += UpdateActiveOrderChanged;
    }

    void UpdateActiveOrderChanged(ActiveOrderChangedArgs args) {
        UpdateOrderBubble(args.ActiveOrderIndex, args.NewOrder);
        UpdateOrderDisplay(args.ActiveOrderIndex, args.NewOrder, args.LastOrderFulfilled);

        // TEMP: debug show remaining orders for playtest
        if (GameManager.Instance.DebugMode) {
            numRemainingOrderText.text = "Remaining: " + args.NumRemainingOrders;
        }
    }

    void UpdateOrderBubble(int activeOrderIndex, Order order) {
        OrderBubbleUI orderBubble = orderBubbles[activeOrderIndex];
        if (order == null) { // no new active order, disable the bubble at index
            DOVirtual.Float(
                orderBubble.Alpha, 0f, TweenManager.OrderBubbleFadeDur,
                alpha => orderBubble.Alpha = alpha
            );
            return;
        }

        DOVirtual.Float(
            orderBubble.Alpha, 1f, TweenManager.OrderBubbleFadeDur, 
            alpha => orderBubbles[activeOrderIndex].Alpha = alpha
        );
        orderBubble.DisplayNewOrder(order);
    }

    Vector3 targetPos;
    void UpdateOrderDisplay(int activeOrderIndex, Order order, bool lastOrderFulfilled) {
        OrderDisplayUI orderDisplay = orderDisplays[activeOrderIndex];
        if (order == null) { // active order was reset, do end tasks for last active orderlfilled);
            targetPos = orderDisplayClosePos.position;
            targetPos.y = orderDisplay.transform.position.y;
            
            orderDisplay.transform.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() => orderDisplay.DisplayEndStatusStamp(lastOrderFulfilled));
            seq.AppendInterval(1);
            seq.Append(orderDisplay.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutQuad));
            seq.Play();
            
            return;
        }
        
        orderDisplay.HideEndStatusStamp();
        orderDisplay.DisplayNewOrder(order);
        
        targetPos = orderDisplayOpenPos.position;
        targetPos.y = orderDisplay.transform.position.y;
        orderDisplay.transform.DOKill();
        orderDisplay.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutQuad);
    }
}