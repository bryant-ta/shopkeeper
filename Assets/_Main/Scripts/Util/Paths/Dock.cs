using System;
using Dreamteck.Splines;
using UnityEngine;

public class Dock : MonoBehaviour {
    // Dock arrival callback assumes single path in and single out!
    [SerializeField] SplineComputer pathIn; // TODO: look into Spline Nodes, they seem to handle this
    [SerializeField] SplineComputer pathOut;
    
    public SplineFollower Docker { get; private set; }
    public bool IsOccupied => Docker != null;

    public event Action OnDockerArrived;

    public bool SetDocker(SplineFollower docker) {
        if (Docker != null) {
            Debug.LogWarning("Unable to set orderer: dock is occupied.");
            return false;
        }
        
        Docker = docker;
        
        // Start listening for when docker arrives at dock
        Docker.OnReachedEnd += HandleArrival;
        
        Docker.Spline = pathIn;

        return true;
    }
    public void RemoveDocker() {
        Docker.OnReachedEnd -= HandleArrival;
        Docker = null;
    }

    void HandleArrival() {
        Docker.OnReachedEnd -= HandleArrival;
        
        Docker.Spline = pathOut;
        Docker.RebuildImmediate();
        Docker.Restart();
        Docker.IsFollowing = false;
        
        OnDockerArrived?.Invoke();
    }

    public Vector3 GetDockingPoint() {
        return transform.position;
    }
}
