using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GameManager))]
public class UIManager : MonoBehaviour {
    [SerializeField] NumberCounter goldCounter;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI phaseText;

    [Header("Next Day")]
    [SerializeField] GameObject nextDayPanel;
    [SerializeField] Button nextDayButton;        // TEMP: until making better next day screen
    [SerializeField] TextMeshProUGUI perfectOrdersText; // TEMP: until making better next day screen

    [Header("Pause Menu")]
    [SerializeField] GameObject pauseMenuPanel;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GetComponent<GameManager>();

        gameMngr.OnModifyMoney += UpdateMoneyText;
        gameMngr.DayTimer.TickEvent += UpdateTimeText;
        gameMngr.SM_dayPhase.OnStateEnter += UpdatePhaseText;

        gameMngr.OnDayEnd += UpdateNextDayPanel;

        gameMngr.OnPause += TogglePauseMenu;
    }

    void UpdateMoneyText(DeltaArgs args) { goldCounter.SetValue(args.NewValue); }

    void UpdateTimeText(string time) { timeText.text = time; }

    void UpdatePhaseText(IState<DayPhase> phase) { phaseText.text = phase.ID.ToString(); }

    void UpdateNextDayPanel() {
        nextDayPanel.SetActive(true);
        nextDayButton.gameObject.SetActive(true);

        if (Ref.Instance.OrderMngr.PerfectOrders) {
            perfectOrdersText.text = "PERFECT!\n+100";
        } else {
            perfectOrdersText.text = "GOOD";
        }
    }
    public void NextDayTransition() {
        nextDayPanel.SetActive(false);
    }

    void TogglePauseMenu(bool isPaused) { pauseMenuPanel.SetActive(isPaused); }
}