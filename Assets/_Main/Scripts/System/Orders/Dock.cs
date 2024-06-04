using PathCreation;
using UnityEngine;

public class Dock : MonoBehaviour {
    [SerializeField] Path pathIn;
    [SerializeField] Path pathOut;
    
    public PathActor Docker { get; private set; }
    
    public Orderer Orderer { get; private set; } // TEMP: generalize for deliveries too
    public bool IsOccupied => Orderer != null;

    public bool SetOrderer(Orderer orderer) {
        if (Orderer != null) {
            Debug.LogWarning("Unable to set orderer: dock is occupied.");
            return false;
        }
        
        Orderer = orderer;
        return true;
    }
    public void RemoveOrderer() {
        Orderer = null;
    }

    public Vector3 GetDockingPoint() {
        return transform.position;
    }
}
