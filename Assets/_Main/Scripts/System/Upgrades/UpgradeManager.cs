using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager> {
    public List<SO_Upgrade> AvailableUpgrades = new List<SO_Upgrade>();
    public List<SO_Upgrade> PurchasedUpgrades = new List<SO_Upgrade>();

    public static UpgradeFlags UpgradeFlags;

    public void AddUpgrade(SO_Upgrade upgrade) {
        PurchasedUpgrades.Add(upgrade);
        upgrade.Apply();
    }

    public bool HasUpgrade(UpgradeID id) {
        return PurchasedUpgrades.Any(upgrade => upgrade.ID == id);
    }
}

public class UpgradeFlags {
    public bool Dash;
}