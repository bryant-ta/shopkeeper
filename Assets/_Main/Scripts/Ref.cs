using System;
using UnityEngine;

// References to commonly used objects in a scene.
public class Ref : Singleton<Ref> {
    [SerializeField] Player player;
    public static Player Player => _player;
    static Player _player;
    
    public UpgradeManager UpgradeMngr;
    
    public DeliveryManager DeliveryMngr;
    public OrderManager OrderMngr;

    void Awake() {
        _player = player;
    }
}
