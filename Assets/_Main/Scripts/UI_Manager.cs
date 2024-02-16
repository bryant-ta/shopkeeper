using System;
using EventManager;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class UI_Manager : MonoBehaviour {
    [SerializeField] TextMeshProUGUI moneyText;

    GameManager gameMngr;

    void Awake() {
        gameMngr = GetComponent<GameManager>();
        
        gameMngr.OnModifyMoney += UpdateMoneyText;
    }

    void UpdateMoneyText(DeltaArgs args) {
        moneyText.text = "Gold: " + args.NewValue.ToString();
    }
}
