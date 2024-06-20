using DG.Tweening;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [Title("Delivery Phase")]
    [SerializeField] GameObject deliveryPhasePanel;
    [SerializeField] NumberCounter goldCounter;

    [Title("Order Phase")]
    [SerializeField] GameObject orderPhasePanel;
    [SerializeField] Image orderPhaseTimeFill;
    [SerializeField] TextMeshProUGUI ordersFulfilledText;
    [SerializeField] GameObject orderPhaseStartPanel;
    [SerializeField] Button orderPhaseStartButton;

    [Title("Next Day")] // TEMP: until making better next day screen
    [SerializeField] GameObject nextDayPanel;
    [SerializeField] TextMeshProUGUI perfectOrdersText;
    [SerializeField] TextMeshProUGUI metQuotaText;
    [SerializeField] Button nextDayButton;

    [Title("Pause Menu")]
    [SerializeField] GameObject pauseMenuPanel;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GameManager.Instance;

        // gameMngr.OnModifyMoney += UpdateMoneyText;
        gameMngr.SM_dayPhase.OnStateEnter += EnterStateTrigger;
        gameMngr.SM_dayPhase.OnStateExit += ExitStateTrigger;
        
        Ref.OrderMngr.OrderPhaseTimer.TickEvent += UpdateOrderPhaseTimer;
        Ref.OrderMngr.OnIncOrderFulfilled += UpdateOrdersFulfilled;

        gameMngr.OnDayEnd += UpdateNextDayPanel;
        gameMngr.OnPause += TogglePauseMenu;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) {
            ToggleOrderPhaseStartButton(true);
        } else if (state.ID == DayPhase.Order) {
            ToggleOrderPhaseTimer(true);
        }
    }
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) {
            ToggleOrderPhaseStartButton(false);
        } else if (state.ID == DayPhase.Order) {
            ToggleOrderPhaseTimer(false);
        }
    }

    void UpdateMoneyText(DeltaArgs args) { goldCounter.SetValue(args.NewValue); }

    #region OrderPhase

    void ToggleOrderPhaseStartButton(bool enable) { orderPhaseStartPanel.SetActive(enable); }
    public void HandleOrderPhaseStartButton() {
        if (Ref.DeliveryMngr.AllDeliveriesOpened) {
            gameMngr.NextPhase();
        } else {
            Color origColor = orderPhaseStartButton.image.color;
            DOTween.Kill(orderPhaseStartButton.image);
            orderPhaseStartButton.image.DOColor(Color.red, 0.2f).OnComplete(
                () =>
                    orderPhaseStartButton.image.DOColor(origColor, 0.2f)
            );
        }
    }

    void ToggleOrderPhaseTimer(bool enable) { orderPhasePanel.SetActive(enable); }
    void UpdateOrderPhaseTimer(float time) { orderPhaseTimeFill.fillAmount = time; }
    void UpdateOrdersFulfilled(int curVal, int thresholdVal) { ordersFulfilledText.text = $"{curVal}/{thresholdVal}"; }

    #endregion

    #region NextDay

    void UpdateNextDayPanel() {
        nextDayPanel.SetActive(true);

        if (Ref.OrderMngr.PerfectOrders) {
            perfectOrdersText.text = "PERFECT!\n+100";
        } else {
            perfectOrdersText.text = "GOOD";
        }
        
        if (Ref.OrderMngr.MetQuota) {
            metQuotaText.text = $"PASSED DAY {gameMngr.Day}";
            nextDayButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Start Day {gameMngr.Day + 1}";
        } else {
            metQuotaText.text = "TRY AGAIN D:";
            nextDayButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Start Day {gameMngr.Day}";
        }

        nextDayButton.gameObject.SetActive(true);
    }
    public void NextDayTransition() { nextDayPanel.SetActive(false); }

    #endregion

    #region Pause

    void TogglePauseMenu(bool isPaused) { pauseMenuPanel.SetActive(isPaused); }

    #endregion
}