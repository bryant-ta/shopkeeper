using System.Collections.Generic;
using DG.Tweening;
using Orders;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(OrderManager))]
public class OrderManagerUI : MonoBehaviour {
    [SerializeField] List<OrderBubbleUI> orderBubbles;
    [SerializeField] List<OrderDisplayUI> orderDisplays;

    // TEMP: until real book ui framework
    [SerializeField] Transform orderDisplayOpenPos;
    [SerializeField] Transform orderDisplayClosePos;

    void UpdateActiveOrderChanged(ActiveOrderChangedArgs args) {
        UpdateOrderBubble(args.ActiveOrderIndex, args.NewOrder);
        UpdateOrderDisplay(args.ActiveOrderIndex, args.NewOrder, args.LastOrderFulfilled);
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
        orderBubble.DisplayOrder(order);
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
            seq.AppendInterval(0.2f);
            seq.Append(orderDisplay.transform.DOMove(targetPos, 1f).SetEase(Ease.InQuad));
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