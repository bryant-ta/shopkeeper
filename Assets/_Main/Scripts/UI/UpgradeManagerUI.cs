using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManagerUI : MonoBehaviour {
    [Tooltip("GameObject to place upgrade entries under, usually \"Content\"")]
    [SerializeField] GameObject contentObj;
    [SerializeField] GameObject upgradeEntryPrefab;

    [SerializeField] List<UpgradeEntry> upgradeEntries = new();
    
    UpgradeManager upgradeMngr;

    void Awake() {
        upgradeMngr = Ref.Instance.UpgradeMngr;

        upgradeMngr.OnAvailableUpgradeAdded += AddAvailableUpgradeEntry;
        upgradeMngr.OnUpgradePurchased += RemoveAvailableUpgradeEntry;
    }

    void AddAvailableUpgradeEntry(Upgrade upgrade) {
        UpgradeEntry upgradeEntry = Instantiate(upgradeEntryPrefab, contentObj.transform).GetComponent<UpgradeEntry>();
        upgradeEntry.Init(upgrade);
        
        upgradeEntries.Add(upgradeEntry);
    }

    public void RemoveAvailableUpgradeEntry(Upgrade upgrade) {
        UpgradeEntry availableUpgradeEntry = upgradeEntries.Find(upgradeEntry => upgradeEntry.Upgrade == upgrade);
        upgradeEntries.Remove(availableUpgradeEntry);
        Destroy(availableUpgradeEntry.gameObject);
    }
}
