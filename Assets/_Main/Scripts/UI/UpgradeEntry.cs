using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeEntry : MonoBehaviour {
    public Upgrade Upgrade;
    
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Image icon;
    
    public void Init(Upgrade upgrade) {
        Upgrade = upgrade;
        
        nameText.text = upgrade.Name;
        descriptionText.text = upgrade.Description;
        costText.text = upgrade.Cost.ToString();
        icon.sprite = upgrade.Icon;
    }

    public void OnClickUpgradeEntry() {
        if (!Ref.Instance.UpgradeMngr.PurchaseUpgrade(Upgrade)) {
            // TODO: feedback for not enough money to purchase
        }
    }
}
