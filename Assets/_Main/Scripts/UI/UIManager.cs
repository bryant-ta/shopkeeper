using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [SerializeField] [Required] GameObject orderPhaseStartPanel;

    [Title("Next Day Panel")] // TEMP: until making better next day screen
    [SerializeField] [Required] GameObject nextDayPanel;
    [SerializeField] [Required] TextMeshProUGUI perfectOrdersText;
    [SerializeField] [Required] TextMeshProUGUI metQuotaText;
    [SerializeField] [Required] Button nextDayButton;

    [Title("Pause Menu")]
    [SerializeField] [Required] GameObject pauseMenuPanel;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GameManager.Instance;

        Ref.DeliveryMngr.OnDeliveryOpened += HandleOrderPhaseStartButton;

        gameMngr.OnPause += TogglePauseMenu;
        // gameMngr.OnModifyMoney += UpdateMoneyText;

        gameMngr.SM_dayPhase.OnStateEnter += EnterStateTrigger;
        gameMngr.SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    // NOTE: calls here should really only toggle panels
    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            ToggleOrderPhaseStartButton(false);
        }
    }
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            UpdateNextDayPanel();
        }
    }

    public void HandleOrderPhaseStartButton() {
        if (Ref.DeliveryMngr.AllDeliveriesOpened) {
            ToggleOrderPhaseStartButton(true);
        }
    }
    void ToggleOrderPhaseStartButton(bool enable) { orderPhaseStartPanel.SetActive(enable); }

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