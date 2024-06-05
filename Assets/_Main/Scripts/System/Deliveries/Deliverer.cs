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

        Grid.OnRemoveShapes += DisableOnEmpty;
    }
    
    #region Dock

    public void OccupyDock(Dock dock) {
        AssignedDock = dock; // do not unset, OrderManager uses ref
        AssignedDock.SetDocker(Docker);
        Docker.OnPathEnd += StartOrder; // assumes single path from Occupy -> Dock

        Docker.StartPath(0);
    }
    void LeaveDock() {
        AssignedDock.RemoveDocker();

        // TODO: leaving anim
        Docker.StartNextPath();
    }

    #endregion

    // TEMP: placeholder until doing anims/theme for basic delivery
    void DisableOnEmpty(List<IGridShape> shapes) {
        if (!Grid.IsEmpty()) return;
        Disable();
    }

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }
}