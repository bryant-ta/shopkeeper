using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Cart")]
public class SO_UpgradeCart : SO_Upgrade {
    public override void Apply() {
        Ref.UpgradeMngr.Refs.Cart.gameObject.SetActive(true);
    }
}
