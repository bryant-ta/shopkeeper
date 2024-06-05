using Dreamteck.Splines;

/// <summary>
/// Interface for interacting with docks, such as arriving and leaving
/// </summary>
public interface IDocker {
    public Dock AssignedDock { get; }
    public SplineFollower Docker { get; }
    
    public void OccupyDock(Dock dock);
    public void LeaveDock();
}