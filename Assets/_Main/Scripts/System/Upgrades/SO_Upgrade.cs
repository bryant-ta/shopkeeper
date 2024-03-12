using TriInspector;
using UnityEngine;

public class SO_Upgrade : ScriptableObject {
    public UpgradeID ID;
    public string Name;
    public int Cost;
    public string Description;
    public Sprite Icon;
    
    [Title("Next Upgrade")]
    public SO_Upgrade NextUpgrade;

    public virtual void Apply() { }
}

public enum UpgradeID {
    Dash = 1,
    DoubleDash = 2,
    CarryLimit = 3,
    Zoom = 4,
    SortDelivery = 5,
    ShopExpansion1 = 6,
    ShopExpansion2 = 7,
}

public class UpgradeFlags {
    public bool Dash;
    public bool DoubleDash;
    public bool Zoom;
}