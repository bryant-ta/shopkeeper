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

        Grid.OnRemoveShapes += CheckLastShapeMoved;
    }

    void AllowInteraction() {
        Grid.IsLocked = false;
    }
    
    // TEMP: placeholder until/if doing delivery boxes
    IGridShape lastShape;
    void CheckLastShapeMoved(List<IGridShape> shapes) {
        if (!Grid.IsAllEmpty()) return;

        lastShape = shapes[0];

        GameManager.WorldGrid.OnPlaceShapes -= HandleCheckLastShapeMoved;
        GameManager.WorldGrid.OnPlaceShapes += HandleCheckLastShapeMoved;
    }

    void HandleCheckLastShapeMoved(List<IGridShape> shapes) {
        GameManager.WorldGrid.OnPlaceShapes -= HandleCheckLastShapeMoved;
        
        foreach (IGridShape shape in shapes) {
            if (shape == lastShape) {
                LeaveDock();
                break;
            }
        }
    }
    
    #region Dock

    public void OccupyDock(Dock dock) {
        AssignedDock = dock;
        AssignedDock.SetDocker(Docker);
        Docker.OnReachedEnd += AllowInteraction; // assumes single path from Occupy -> Dock

        Docker.StartFollowing();
    }
    public void LeaveDock() {
        Grid.IsLocked = false;
        
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