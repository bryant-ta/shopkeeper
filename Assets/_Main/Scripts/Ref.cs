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
    
    [SerializeField] DeliveryManager deliveryMngr;
    public static DeliveryManager DeliveryMngr => _deliveryMngr;
    static DeliveryManager _deliveryMngr;
    
    [SerializeField] OrderManager orderMngr;
    public static OrderManager OrderMngr => _orderMngr;
    static OrderManager _orderMngr;
    
    public Trash Trash;

    public Transform OffScreenSpawnTrs;

    void Awake() {
        _player = player;
        _upgradeMngr = upgradeMngr;
        _deliveryMngr = deliveryMngr;
        _orderMngr = orderMngr;
    }
}
