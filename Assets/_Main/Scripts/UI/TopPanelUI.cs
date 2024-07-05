using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

public class TopPanelUI : MonoBehaviour {
    [SerializeField] [Required] Image runTimerFill;
    [SerializeField] [Required] TextMeshProUGUI runTimerText;
    
    [SerializeField] [Required] GameObject ordersTargetPanel;
    [SerializeField] [Required] TextMeshProUGUI ordersTargetText;
    
    [SerializeField] [Required] GameObject ordersFulfilledPanel;
    [SerializeField] [Required] TextMeshProUGUI ordersFulfilledText;
    
    [SerializeField] [Required] NumberCounter scoreCounter;
    
    [SerializeField] [Required] TextMeshProUGUI roundText;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GameManager.Instance;

        Ref.OrderMngr.OnOrderFulfilled += UpdateOrdersFulfilledText;
        Ref.OrderMngr.OnQuotaUpdated += UpdateOrdersTargetText;

        gameMngr.RunTimer.TickEvent += UpdateOrderPhaseTimer;
        gameMngr.RunTimer.EndEvent += UpdateOrderPhaseTimerEnd;
        
        gameMngr.OnDayEnd += UpdateRoundText;

        gameMngr.OnModifyScore += UpdateScoreCounter;
        
        gameMngr.SM_dayPhase.OnStateEnter += EnterStateTrigger;
    }

    // NOTE: calls here should really only toggle panels
    void EnterStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Delivery) {
            ToggleOrdersTargetPanel(true);
            ToggleOrdersFulfilledPanel(false);
        } else if (state.ID == DayPhase.Order) {
            ToggleOrdersTargetPanel(false);
            ToggleOrdersFulfilledPanel(true);
        }
    }

    void UpdateOrderPhaseTimer(float time) {
        runTimerFill.fillAmount = time;
        runTimerText.text = gameMngr.RunTimer.ToStringMinuteSeconds();
    }
    void UpdateOrderPhaseTimerEnd() { runTimerText.text = "0:00"; }

    void ToggleOrdersTargetPanel(bool enable) { ordersTargetPanel.gameObject.SetActive(enable); }
    void UpdateOrdersTargetText(int val) { ordersTargetText.text = val.ToString(); }
    
    void ToggleOrdersFulfilledPanel(bool enable) { ordersFulfilledPanel.gameObject.SetActive(enable); }
    void UpdateOrdersFulfilledText(int curVal, int thresholdVal) {
        ordersFulfilledText.text = $"{curVal}/{thresholdVal}";
        if (curVal >= thresholdVal) {
            ordersFulfilledText.color = Color.green;
        } else {
            ordersFulfilledText.color = Color.white;
        }
    }
    
    void UpdateScoreCounter(DeltaArgs args) { scoreCounter.SetValue(args.NewValue); }

    void UpdateRoundText(int val) { roundText.text = $"Round {val}"; }
}