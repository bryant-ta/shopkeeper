using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class SO_Upgrade : ScriptableObject {
    public UpgradeID ID;
    public string Name;
    public int Cost;
    public string Description;
    public Sprite Icon;
    
    [Title("Upgrade Chains")]
    public SO_Upgrade NextUpgrade;
    public bool IsRepeating;
    [Tooltip("Number of times the upgrade can be repeated. NextUpgrade should usually reference this upgrade (itself).")]
    public int RepeatsRemaining;

    public virtual void Apply() { }
}

[Serializable]
public class Upgrade {
    public SO_Upgrade UpgradeData { get; private set; }
    
    public UpgradeID ID;
    public string Name;
    public int Cost;
    public string Description;
    public Sprite Icon;
    
    public SO_Upgrade NextUpgradeData;
    public bool IsRepeating;
    public int RepeatsRemaining;
    
    public Upgrade(SO_Upgrade upgradeData) {
        UpgradeData = upgradeData;
        
        ID = upgradeData.ID;
        Name = upgradeData.Name;
        Cost = upgradeData.Cost;
        Description = upgradeData.Description;
        Icon = upgradeData.Icon;
        
        NextUpgradeData = upgradeData.NextUpgrade;
        IsRepeating = upgradeData.IsRepeating;
        RepeatsRemaining = upgradeData.RepeatsRemaining;
    }

    public void Apply() {
        UpgradeData.Apply();
    }
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