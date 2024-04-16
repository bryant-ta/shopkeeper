using TriInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/ShopExpansion")]
public class SO_UpgradeShopExpansion : SO_Upgrade {
    [Title("SO_UpgradeShopExpansion")]
    [SerializeField] int expansionIndex;
    
    public override void Apply() {
        Ref.UpgradeMngr.Refs.ShopExpansionMngr.DoShopExpansion(expansionIndex);
    }
}