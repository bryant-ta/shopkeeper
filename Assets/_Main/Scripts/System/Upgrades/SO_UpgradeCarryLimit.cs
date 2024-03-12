using TriInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/CarryLimit")]
public class SO_UpgradeCarryLimit : SO_Upgrade {
    [Title("UpgradeCarryLimit")]
    [SerializeField] int increaseAmt;
    
    public override void Apply() {
        Player player = Ref.Instance.Player;
        PlayerDrag playerDrag = player.GetComponent<PlayerDrag>();
        
        player.ModifyMaxHoldHeight(increaseAmt);
        playerDrag.ModifyMaxDragHeight(increaseAmt);
    }
}
