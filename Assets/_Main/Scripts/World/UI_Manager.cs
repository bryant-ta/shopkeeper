using TMPro;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class UI_Manager : MonoBehaviour {
    [SerializeField] TextMeshProUGUI moneyText;
    
    [SerializeField] GameObject pauseMenuPanel;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GetComponent<GameManager>();
        
        gameMngr.OnModifyMoney += UpdateMoneyText;
        gameMngr.OnPause += TogglePauseMenu;
    }

    void UpdateMoneyText(DeltaArgs args) {
        moneyText.text = "Gold: " + args.NewValue.ToString();
    }
    
    void TogglePauseMenu(bool isPaused) {
        pauseMenuPanel.SetActive(isPaused);
    }
}
