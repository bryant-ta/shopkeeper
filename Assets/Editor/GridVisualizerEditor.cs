using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridVisualizer))]
public class GridVisualizerEditor : Editor {
    void OnSceneGUI() {
        GridVisualizer gridVisualizer = (GridVisualizer) target;

        if (!gridVisualizer.enabled) return;

        Handles.color = Color.magenta;

        Vector3 cellSize = new Vector3(1f, 1f, 0f);
        Vector3 offset = new Vector3(gridVisualizer.offset.x, 0f, gridVisualizer.offset.y);

        // Draw thinner lines at increments of 1
        for (float x = gridVisualizer.minBounds.x; x <= gridVisualizer.maxBounds.x; x++) {
            if (Mathf.Approximately(x % 5, 0)) {
                Handles.DrawAAPolyLine(5f, new Vector3[] {
                    new Vector3(x, 0f, gridVisualizer.minBounds.z) + offset,
                    new Vector3(x, 0f, gridVisualizer.maxBounds.z) + offset
                });
                Handles.Label(new Vector3(x, 0f, gridVisualizer.minBounds.z - 0.2f) + offset, x.ToString());
            }
            else {
                Handles.DrawLine(new Vector3(x, 0f, gridVisualizer.minBounds.z) + offset,
                    new Vector3(x, 0f, gridVisualizer.maxBounds.z) + offset);
            }
        }

        for (float z = gridVisualizer.minBounds.z; z <= gridVisualizer.maxBounds.z; z++) {
            if (Mathf.Approximately(z % 5, 0)) {
                Handles.DrawAAPolyLine(5f, new Vector3[] {
                    new Vector3(gridVisualizer.minBounds.x, 0f, z) + offset,
                    new Vector3(gridVisualizer.maxBounds.x, 0f, z) + offset
                });
                Handles.Label(new Vector3(gridVisualizer.minBounds.x - 0.2f, 0f, z) + offset, z.ToString());
            }
            else {
                Handles.DrawLine(new Vector3(gridVisualizer.minBounds.x, 0f, z) + offset,
                    new Vector3(gridVisualizer.maxBounds.x, 0f, z) + offset);
            }
        }
    }
}