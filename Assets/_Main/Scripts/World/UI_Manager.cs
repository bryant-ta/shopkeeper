using TMPro;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class UI_Manager : MonoBehaviour {
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI phaseText;
    
    [SerializeField] TextMeshProUGUI nextDayText; // TEMP: until making better next day screen
    
    [SerializeField] GameObject pauseMenuPanel;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GetComponent<GameManager>();
        
        gameMngr.OnModifyMoney += UpdateMoneyText;
        gameMngr.DayTimer.TickEvent += UpdateTimeText;
        gameMngr.SM_dayPhase.OnStateEnter += UpdatePhaseText;

        gameMngr.OnDayEnd += UpdateNextDayText;
        
        gameMngr.OnPause += TogglePauseMenu;
    }

    void UpdateMoneyText(DeltaArgs args) {
        moneyText.text = "Gold: " + args.NewValue.ToString();
    }
    
    void UpdateTimeText(string time) {
        timeText.text = time;
    }

    void UpdatePhaseText(IState<DayPhase> phase) {
        phaseText.text = phase.ID.ToString();
    }
    
    void UpdateNextDayText() {
        // TODO
    }

    void TogglePauseMenu(bool isPaused) {
        pauseMenuPanel.SetActive(isPaused);
    }
}
