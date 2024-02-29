using UnityEngine;

public class Ref : Singleton<Ref> {
    // References to commonly used objects in a scene.
    public DeliveryManager DeliveryMngr;
    public OrderManager OrderMngr;
}
