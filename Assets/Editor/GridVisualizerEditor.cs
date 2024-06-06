using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridVisualizer))]
public class GridVisualizerEditor : Editor {
    void OnSceneGUI() {
        GridVisualizer gridVisualizer = (GridVisualizer) target;

        if (!gridVisualizer.enabled) return;

        Handles.color = Color.magenta;

        // Calculate rotation matrix based on the grid object's rotation
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(gridVisualizer.transform.position, gridVisualizer.transform.rotation, Vector3.one);

        // Calculate centered offset based on the grid object's position
        Vector3 centerOffset = gridVisualizer.transform.localPosition - Vector3.Scale(
            (gridVisualizer.maxBounds + gridVisualizer.minBounds) * 0.5f, gridVisualizer.transform.lossyScale
        );

        // Apply offset and rotation to the grid visualization
        Handles.matrix = rotationMatrix;
        Vector3 offset = new Vector3(gridVisualizer.offset.x, 0f, gridVisualizer.offset.y) + centerOffset;

        // Determine the cell size
        float cellSizeX = 1;
        float cellSizeZ = 1;

        // Draw grid lines
        for (float x = gridVisualizer.minBounds.x; x <= gridVisualizer.maxBounds.x; x += cellSizeX) {
            if (Mathf.Approximately(x % 5, 0)) {
                Handles.DrawAAPolyLine(
                    5f, new Vector3[] {
                        new Vector3(x, 0f, gridVisualizer.minBounds.z) + offset,
                        new Vector3(x, 0f, gridVisualizer.maxBounds.z) + offset
                    }
                );
                Handles.Label(new Vector3(x - cellSizeZ / 2, 0f, gridVisualizer.minBounds.z - 0.2f) + offset, x.ToString());
            } else {
                Handles.DrawLine(
                    new Vector3(x, 0f, gridVisualizer.minBounds.z) + offset,
                    new Vector3(x, 0f, gridVisualizer.maxBounds.z) + offset
                );
            }
        }

        for (float z = gridVisualizer.minBounds.z; z <= gridVisualizer.maxBounds.z; z += cellSizeZ) {
            if (Mathf.Approximately(z % 5, 0)) {
                Handles.DrawAAPolyLine(
                    5f, new Vector3[] {
                        new Vector3(gridVisualizer.minBounds.x, 0f, z) + offset,
                        new Vector3(gridVisualizer.maxBounds.x, 0f, z) + offset
                    }
                );
                Handles.Label(new Vector3(gridVisualizer.minBounds.x - 0.2f, 0f, z - cellSizeX / 2) + offset, z.ToString());
            } else {
                Handles.DrawLine(
                    new Vector3(gridVisualizer.minBounds.x, 0f, z) + offset,
                    new Vector3(gridVisualizer.maxBounds.x, 0f, z) + offset
                );
            }
        }

        Handles.matrix = Matrix4x4.identity; // Reset matrix to avoid affecting other Handles
    }
}