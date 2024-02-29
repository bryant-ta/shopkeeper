using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OrderManager))]
public class OrderManagerUI : MonoBehaviour {
    [SerializeField] List<OrderBubbleUI> orderBubbles;

    [SerializeField] Button skipToClosePhaseButton;

    OrderManager orderMngr;

    void Awake() {
        orderMngr = GetComponent<OrderManager>();

        orderMngr.OnActiveOrderChanged += UpdateActiveOrderChanged;
    }

    void UpdateActiveOrderChanged(ActiveOrderChangedArgs args) {
        UpdateOrderBubble(args.ActiveOrderIndex, args.NewOrder);

        if (args.NumRemainingOrders == 0 && !hideSkipToPhaseEndButton) {
            skipToClosePhaseButton.gameObject.SetActive(true);
        }
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

    // TEMP
    bool hideSkipToPhaseEndButton = false;
    public void SkipToPhaseEnd() {
        GameManager.Instance.SetTimeScale(10);

        void ResetTimeScaleDelegate(IState<DayPhase> state) {
            GameManager.Instance.SetTimeScale(1);
            hideSkipToPhaseEndButton = false;
            GameManager.Instance.SM_dayPhase.OnStateEnter -= ResetTimeScaleDelegate;
        }

        skipToClosePhaseButton.gameObject.SetActive(false);
        hideSkipToPhaseEndButton = true;
        GameManager.Instance.SM_dayPhase.OnStateEnter += ResetTimeScaleDelegate;
    }
}