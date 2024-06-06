using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

[RequireComponent(typeof(SplineFollower))]
public class Deliverer : MonoBehaviour, IDocker {
    public Grid Grid { get; private set; }
    
    public Dock AssignedDock { get; private set; }
    public SplineFollower Docker { get; private set; }

    void Awake() {
        Grid = gameObject.GetComponentInChildren<Grid>();
        Grid.IsLocked = true;
        
        Docker = GetComponent<SplineFollower>();

        Grid.OnRemoveShapes += LeaveOnEmpty;
    }

    void AllowInteraction() {
        Grid.IsLocked = false;
    }
    
    // TEMP: placeholder until/if doing delivery boxes
    void LeaveOnEmpty(List<IGridShape> shapes) {
        if (!Grid.IsEmpty()) return;
        LeaveDock();
    }
    
    #region Dock

    public void OccupyDock(Dock dock) {
        AssignedDock = dock;
        AssignedDock.SetDocker(Docker);
        Docker.OnReachedEnd += AllowInteraction; // assumes single path from Occupy -> Dock

        Docker.StartFollowing();
    }
    public void LeaveDock() {
        AssignedDock.RemoveDocker();
        AssignedDock = null;

        // TEMP: until leaving anim
        Docker.StartFollowing();
        Docker.OnReachedEnd += () => Destroy(gameObject);
    }

    #endregion

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }
}