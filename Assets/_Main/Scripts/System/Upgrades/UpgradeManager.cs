using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour {
    public List<SO_Upgrade> AvailableUpgrades = new List<SO_Upgrade>();
    public List<SO_Upgrade> PurchasedUpgrades = new List<SO_Upgrade>();

    public static UpgradeFlags Flags;

    public event Action<SO_Upgrade> OnAvailableUpgradeAdded;
    public event Action<SO_Upgrade> OnUpgradePurchased;

    void Awake() {
        Flags = new UpgradeFlags();
    }

    void Start() {
        // Activate initial available upgrades (UI)
        for (int i = 0; i < AvailableUpgrades.Count; i++) {
            OnAvailableUpgradeAdded?.Invoke(AvailableUpgrades[i]);
        }
        
        // Activate initial purchased upgrades
        for (int i = 0; i < PurchasedUpgrades.Count; i++) {
            PurchaseUpgrade(PurchasedUpgrades[i], true);
        }
    }

    public bool PurchaseUpgrade(SO_Upgrade upgrade, bool ignoreCost = false) {
        if (!ignoreCost && !GameManager.Instance.ModifyGold(-upgrade.Cost)) {
            return false;
        }
        
        AvailableUpgrades.Remove(upgrade);
        if (!PurchasedUpgrades.Contains(upgrade)) {
            PurchasedUpgrades.Add(upgrade);
        }
        upgrade.Apply();
            
        OnUpgradePurchased?.Invoke(upgrade);

        if (upgrade.NextUpgrade != null) {
            AddAvailableUpgrade(upgrade.NextUpgrade);
        }

        return true;
    }

    public void AddAvailableUpgrade(SO_Upgrade upgrade) {
        AvailableUpgrades.Add(upgrade);
        OnAvailableUpgradeAdded?.Invoke(upgrade);
    }

    public bool HasUpgrade(UpgradeID id) {
        return PurchasedUpgrades.Any(upgrade => upgrade.ID == id);
    }
}