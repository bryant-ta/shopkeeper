using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [SerializeField] NumberCounter goldCounter;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI phaseText;

    [Title("Next Day")] // TEMP: until making better next day screen
    [SerializeField] GameObject nextDayPanel;
    [SerializeField] TextMeshProUGUI perfectOrdersText;
    [SerializeField] TextMeshProUGUI productsUnlockedText;
    [SerializeField] Button nextDayButton;

    [Title("Pause Menu")]
    [SerializeField] GameObject pauseMenuPanel;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GameManager.Instance;

        gameMngr.OnModifyMoney += UpdateMoneyText;
        gameMngr.DayTimer.TickEvent += UpdateTimeText;
        gameMngr.SM_dayPhase.OnStateEnter += UpdatePhaseText;

        gameMngr.OnDayEnd += UpdateNextDayPanel;

        gameMngr.OnPause += TogglePauseMenu;
    }

    void UpdateMoneyText(DeltaArgs args) { goldCounter.SetValue(args.NewValue); }

    void UpdateTimeText(string time) { timeText.text = time; }

    void UpdatePhaseText(IState<DayPhase> phase) { phaseText.text = phase.ID.ToString().ToUpper(); }

    void UpdateNextDayPanel() {
        nextDayPanel.SetActive(true);

        // Perfect Orders
        if (Ref.Instance.OrderMngr.PerfectOrders) {
            perfectOrdersText.text = "PERFECT!\n+100";
        } else {
            perfectOrdersText.text = "GOOD";
        }
        
        // Products Unlocked
        List<ProductID> nextDayProductsUnlocked = Ref.Instance.DeliveryMngr.GetDayPossibleProducts(gameMngr.Day + 1);
        string productsUnlockedString = "";
        for (int i = 0; i < nextDayProductsUnlocked.Count; i++) {
            productsUnlockedString += $"<sprite name=\"{nextDayProductsUnlocked[i].ToString()}\"> ";
        }
        productsUnlockedText.text = productsUnlockedString.TrimEnd();
        
        // Next Day Button
        nextDayButton.gameObject.SetActive(true);
    }
    public void NextDayTransition() {
        nextDayPanel.SetActive(false);
    }

    void TogglePauseMenu(bool isPaused) { pauseMenuPanel.SetActive(isPaused); }
}