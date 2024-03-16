using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;

public class UpgradeManager : MonoBehaviour {
    [Title("Presets")]
    [SerializeField] List<SO_Upgrade> InitialAvailableUpgrades = new();
    [SerializeField] List<SO_Upgrade> InitialPurchasedUpgrades = new();
    
    [Title("Runtime")]
    public List<Upgrade> AvailableUpgrades = new();
    public List<Upgrade> PurchasedUpgrades = new();

    public UpgradeRefs Refs; // questionable design pattern? needed because need way to ref scene objs from SO_Upgrades
    
    public static UpgradeFlags Flags;

    public event Action<Upgrade> OnAvailableUpgradeAdded;
    public event Action<Upgrade> OnUpgradePurchased;

    void Awake() {
        Flags = new UpgradeFlags();
    }

    void Start() {
        // Activate initial available upgrades (UI)
        for (int i = 0; i < InitialAvailableUpgrades.Count; i++) {
            AddAvailableUpgrade(InitialAvailableUpgrades[i]);
        }
        
        // Activate initial purchased upgrades
        for (int i = 0; i < InitialPurchasedUpgrades.Count; i++) {
            Upgrade initialPurchasedUpgrade = AvailableUpgrades.Find(upgrade => upgrade.ID == InitialPurchasedUpgrades[i].ID);
            PurchaseUpgrade(initialPurchasedUpgrade, true);
        }
    }

    public bool PurchaseUpgrade(Upgrade upgrade, bool ignoreCost = false) {
        if (!ignoreCost && !GameManager.Instance.ModifyGold(-upgrade.Cost)) {
            return false;
        }
        
        // Apply upgrade, update internal tracking lists
        AvailableUpgrades.Remove(upgrade);
        if (!PurchasedUpgrades.Contains(upgrade)) {
            PurchasedUpgrades.Add(upgrade);
        }
        upgrade.Apply();
            
        OnUpgradePurchased?.Invoke(upgrade);

        // Load next upgrade in upgrade chain, if any
        AddNextUpgrade(upgrade);

        return true;
    }

    void AddAvailableUpgrade(SO_Upgrade upgradeData) {
        Upgrade upgrade = new Upgrade(upgradeData);
        AvailableUpgrades.Add(upgrade);
        OnAvailableUpgradeAdded?.Invoke(upgrade);
    }
    
    void AddNextUpgrade(Upgrade origUpgrade) {
        if (origUpgrade.NextUpgradeData == null) return;    // No next upgrade
        
        Upgrade nextUpgrade = new Upgrade(origUpgrade.NextUpgradeData);

        if (origUpgrade.IsRepeating) {
            nextUpgrade.RepeatsRemaining = origUpgrade.RepeatsRemaining - 1;
            if (nextUpgrade.RepeatsRemaining == 0) return;   // No repeats remaining for repeating upgrade
        }

        AvailableUpgrades.Add(nextUpgrade);
        OnAvailableUpgradeAdded?.Invoke(nextUpgrade);
    }

    public bool HasUpgrade(UpgradeID id) {
        return PurchasedUpgrades.Any(upgrade => upgrade.ID == id);
    }
}

[Serializable]
public struct UpgradeRefs {
    public ShopExpansionManager ShopExpansionMngr;
    public Cart Cart;
}