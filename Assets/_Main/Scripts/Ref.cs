using System;
using UnityEngine;

// References to commonly used objects in a scene.
public class Ref : Singleton<Ref> {
    [SerializeField] Player player;
    public static Player Player => _player;
    static Player _player;

    [SerializeField] UpgradeManager upgradeMngr;
    public static UpgradeManager UpgradeMngr => _upgradeMngr;
    static UpgradeManager _upgradeMngr;
    
    public DeliveryManager DeliveryMngr;
    public OrderManager OrderMngr;
    public Trash Trash;

    void Awake() {
        _player = player;
        _upgradeMngr = upgradeMngr;
    }
}
