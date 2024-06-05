using System.Collections.Generic;
using Paths;
using UnityEngine;

[RequireComponent(typeof(PathActor))]
public class Deliverer : MonoBehaviour, IDocker {
    public Grid Grid { get; private set; }
    
    public Dock AssignedDock { get; private set; }
    public PathActor Docker { get; private set; }

    void Awake() {
        Grid = gameObject.GetComponentInChildren<Grid>();
        Grid.IsLocked = true;
        
        Docker = GetComponent<PathActor>();

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
        Docker.OnPathEnd += AllowInteraction; // assumes single path from Occupy -> Dock

        Docker.StartPath(0);
    }
    public void LeaveDock() {
        AssignedDock.RemoveDocker();
        AssignedDock = null;

        // TEMP: until leaving anim
        Destroy(gameObject);
    }

    #endregion

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }
}