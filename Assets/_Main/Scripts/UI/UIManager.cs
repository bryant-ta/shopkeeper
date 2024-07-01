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
    [SerializeField] Image orderPhaseTimerFill;
    [SerializeField] TextMeshProUGUI orderPhaseTimerText;
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

        Ref.DeliveryMngr.OnDeliveriesOpenedCheck += HandleOrderPhaseStartButton;

        Ref.OrderMngr.OrderPhaseTimer.TickEvent += UpdateOrderPhaseTimer;
        Ref.OrderMngr.OrderPhaseTimer.EndEvent += UpdateOrderPhaseTimerEnd;
        Ref.OrderMngr.OnOrderFulfilled += UpdateOrdersFulfilled;

        gameMngr.OnDayEnd += UpdateNextDayPanel;
        gameMngr.OnPause += TogglePauseMenu;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            ToggleOrderPhaseStartButton(false);
            ToggleOrderPhaseTimer(true);
        }
    }
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            ToggleOrderPhaseTimer(false);
        }
    }

    void UpdateMoneyText(DeltaArgs args) { goldCounter.SetValue(args.NewValue); }

    #region OrderPhase

    void ToggleOrderPhaseStartButton(bool enable) { orderPhaseStartPanel.SetActive(enable); }
    public void HandleOrderPhaseStartButton() {
        if (Ref.DeliveryMngr.AllDeliveriesOpened) {
            ToggleOrderPhaseStartButton(true);
        }
    }

    void ToggleOrderPhaseTimer(bool enable) { orderPhasePanel.SetActive(enable); }
    void UpdateOrderPhaseTimer(float time) {
        orderPhaseTimerFill.fillAmount = time;
        orderPhaseTimerText.text = Ref.OrderMngr.OrderPhaseTimer.ToStringMinuteSeconds();
    }
    void UpdateOrderPhaseTimerEnd() {
        orderPhaseTimerText.text = "0:00";
    }

    void UpdateOrdersFulfilled(int curVal, int thresholdVal) {
        ordersFulfilledText.text = $"{curVal}/{thresholdVal}";
        if (curVal >= thresholdVal) {
            ordersFulfilledText.color = Color.green;
        } else {
            ordersFulfilledText.color = Color.black;
        }
    }

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
            metQuotaText.text = $"FINISHED DAY {gameMngr.Day}";
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