using System;
using Paths;
using UnityEngine;

public class Dock : MonoBehaviour {
    // Dock arrival callback assumes single path in and single out!
    [SerializeField] Path pathIn;
    [SerializeField] Path pathOut;
    
    public PathActor Docker { get; private set; }
    public bool IsOccupied => Docker != null;

    public event Action OnDockerArrived;

    public bool SetDocker(PathActor docker) {
        if (Docker != null) {
            Debug.LogWarning("Unable to set orderer: dock is occupied.");
            return false;
        }
        
        Docker = docker;
        
        // Start listening for when docker arrives at dock
        Docker.OnPathEnd += HandleArrival;

        // Setup docker to use dock's paths
        Docker.AddPath(pathIn);
        Docker.AddPath(pathOut);
        
        return true;
    }
    public void RemoveDocker() {
        Docker.OnPathEnd -= HandleArrival;
        Docker = null;
    }

    void HandleArrival() {
        Docker.OnPathEnd -= HandleArrival;
        OnDockerArrived?.Invoke();
    }

    public Vector3 GetDockingPoint() {
        return transform.position;
    }
}
