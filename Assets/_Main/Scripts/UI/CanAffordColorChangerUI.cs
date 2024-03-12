using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CanAffordColorChangerUI : MonoBehaviour {
    [SerializeField] Color canAffordColor;
    [SerializeField] Color cannotAffordColor;
    TextMeshProUGUI costText;

    void Awake() {
        costText = GetComponent<TextMeshProUGUI>();
        GameManager.Instance.OnModifyMoney += SetCostTextColor;
    }

    void SetCostTextColor(DeltaArgs deltaArgs) {
        int cost = int.Parse(costText.text);
        if (deltaArgs.NewValue >= cost) {
            costText.color = canAffordColor;
        } else {
            costText.color = cannotAffordColor;
        }
    }
}