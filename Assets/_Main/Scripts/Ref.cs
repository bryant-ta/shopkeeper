using UnityEngine;

public class Ref : Singleton<Ref> {
    // References to commonly used objects in a scene.
    public Player Player;
    
    public UpgradeManager UpgradeMngr;
    
    public DeliveryManager DeliveryMngr;
    public OrderManager OrderMngr;
}
