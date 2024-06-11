using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [SerializeField] NumberCounter goldCounter;
    [SerializeField] GameObject orderPhaseTimer;
    [SerializeField] Image orderPhaseTimeFill;

    [Title("Next Day")] // TEMP: until making better next day screen
    [SerializeField] GameObject nextDayPanel;
    [SerializeField] TextMeshProUGUI perfectOrdersText;
    [SerializeField] Button nextDayButton;

    [Title("Pause Menu")]
    [SerializeField] GameObject pauseMenuPanel;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GameManager.Instance;

        gameMngr.OnModifyMoney += UpdateMoneyText;
        gameMngr.SM_dayPhase.OnStateEnter += EnterStateTrigger;
        gameMngr.SM_dayPhase.OnStateExit += ExitStateTrigger;
        Ref.OrderMngr.OrderPhaseTimer.TickEvent += UpdateOrderPhaseTimer;


        gameMngr.OnDayEnd += UpdateNextDayPanel;
        gameMngr.OnPause += TogglePauseMenu;
    }

    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            ToggleOrderPhaseTimer(true);
        }
    }
    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Order) {
            ToggleOrderPhaseTimer(false);
        }
    }

    void UpdateMoneyText(DeltaArgs args) { goldCounter.SetValue(args.NewValue); }

    void UpdateOrderPhaseTimer(float time) { orderPhaseTimeFill.fillAmount = time; }
    void ToggleOrderPhaseTimer(bool enable) { orderPhaseTimer.SetActive(enable); }

    void UpdateNextDayPanel() {
        nextDayPanel.SetActive(true);

        // Perfect Orders
        if (Ref.OrderMngr.PerfectOrders) {
            perfectOrdersText.text = "PERFECT!\n+100";
        } else {
            perfectOrdersText.text = "GOOD";
        }

        // Next Day Button
        nextDayButton.gameObject.SetActive(true);
    }
    public void NextDayTransition() { nextDayPanel.SetActive(false); }

    void TogglePauseMenu(bool isPaused) { pauseMenuPanel.SetActive(isPaused); }
}