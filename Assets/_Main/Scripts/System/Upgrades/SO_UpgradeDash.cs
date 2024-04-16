using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Dash")]
public class SO_UpgradeDash : SO_Upgrade {
    public override void Apply() {
        UpgradeManager.Flags.Dash = true;
    }
}