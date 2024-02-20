using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GameManager))]
public class UI_Manager : MonoBehaviour {
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI phaseText;

    [SerializeField] GameObject nextDayPanel;
    [SerializeField] TextMeshProUGUI nextDayText; // TEMP: until making better next day screen
    [SerializeField] Button nextDayButton; // TEMP: until making better next day screen

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

    void UpdateMoneyText(DeltaArgs args) { moneyText.text = "Gold: " + args.NewValue.ToString(); }

    void UpdateTimeText(string time) { timeText.text = time; }

    void UpdatePhaseText(IState<DayPhase> phase) { phaseText.text = phase.ID.ToString(); }

    void UpdateNextDayPanel() {
        nextDayPanel.SetActive(true);
        nextDayButton.gameObject.SetActive(true);
    }
    public void NextDayTransition() {
        nextDayButton.gameObject.SetActive(false);
        nextDayText.DOFade(0f, 2f).OnComplete(() => {
            nextDayText.alpha = 1f;
            nextDayPanel.SetActive(false);
        });
    }

    void TogglePauseMenu(bool isPaused) { pauseMenuPanel.SetActive(isPaused); }
}