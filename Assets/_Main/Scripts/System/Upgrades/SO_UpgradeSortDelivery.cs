using TriInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/SortDelivery")]
public class SO_UpgradeSortDelivery : SO_Upgrade {
    [Title("UpgradeSortDelivery")]
    [SerializeField] int maxGroupQuantity;
    
    public override void Apply() {
        Ref.DeliveryMngr.SetMaxGroupQuantity(maxGroupQuantity);
    }
}