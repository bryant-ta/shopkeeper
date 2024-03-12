using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Zoom")]
public class SO_UpgradeZoom : SO_Upgrade {
    public override void Apply() {
        UpgradeManager.Flags.Zoom = true;
    }
}