using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UpgradeManagerUI : MonoBehaviour {
    [Tooltip("GameObject to place upgrade entries under, usually \"Content\"")]
    [SerializeField] GameObject contentObj;
    [SerializeField] GameObject upgradeEntryPrefab;

    [SerializeField] List<UpgradeEntryUI> upgradeEntries = new();
    
    UpgradeManager upgradeMngr;

    void Awake() {
        upgradeMngr = Ref.UpgradeMngr;

        upgradeMngr.OnAvailableUpgradeAdded += AddAvailableUpgradeEntry;
        upgradeMngr.OnUpgradePurchased += RemoveAvailableUpgradeEntry;
    }

    void AddAvailableUpgradeEntry(Upgrade upgrade) {
        UpgradeEntryUI upgradeEntry = Instantiate(upgradeEntryPrefab, contentObj.transform).GetComponent<UpgradeEntryUI>();
        upgradeEntry.Init(upgrade);
        
        upgradeEntries.Add(upgradeEntry);
    }

    public void RemoveAvailableUpgradeEntry(Upgrade upgrade) {
        UpgradeEntryUI availableUpgradeEntry = upgradeEntries.Find(upgradeEntry => upgradeEntry.Upgrade == upgrade);
        upgradeEntries.Remove(availableUpgradeEntry);
        Destroy(availableUpgradeEntry.gameObject);
    }

    // TEMP: until making full book UI
    [SerializeField] RectTransform bookClosePos;
    [SerializeField] RectTransform bookOpenPos;
    bool windowIsOpen;
    public void ToggleWindow() {
        windowIsOpen = !windowIsOpen;
        
        transform.DOKill();
        if (windowIsOpen) {
            transform.DOMove(bookOpenPos.position, 0.3f).SetEase(Ease.OutQuad);
        } else {
            transform.DOMove(bookClosePos.position, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}
