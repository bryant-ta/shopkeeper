using TriInspector;
using UnityEngine;

public class GridVisualizer : MonoBehaviour {
    [SerializeField] Grid grid;

    public Vector2 offset = new Vector2(0.5f, 0.5f);
    [ReadOnly] public Vector3 minBounds = new Vector3(-10f, 0f, -10f);
    [ReadOnly] public Vector3 maxBounds = new Vector3(10f, 0f, 10f);

    void OnValidate() {
        minBounds = new Vector3(grid.MinX, 0, grid.MinZ);
        maxBounds = new Vector3(grid.MaxX, 0, grid.MaxZ);
    }
}