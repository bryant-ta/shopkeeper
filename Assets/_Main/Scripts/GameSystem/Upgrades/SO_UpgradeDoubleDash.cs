using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/DoubleDash")]
public class SO_UpgradeDoubleDash : SO_Upgrade {
    public override void Apply() {
        UpgradeManager.Flags.DoubleDash = true;
    }
}
