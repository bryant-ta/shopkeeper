using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade")]
public class SO_Upgrade : ScriptableObject {
    public UpgradeID ID { get; private set; }
    public string Name { get; private set; }
    public int Cost { get; private set; }
    public string Description { get; private set; }

    public virtual void Apply() {
        
    }
}

public enum UpgradeID {
    Dash = 1,
    DoubleDash = 2,
}

public class SO_UpgradeDash : SO_Upgrade {
    public override void Apply() {
        UpgradeManager.UpgradeFlags.Dash = true;
    }
}