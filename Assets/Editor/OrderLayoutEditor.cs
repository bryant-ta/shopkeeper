using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SO_OrderLayout))]
public class OrderLayoutEditor : Editor {
    Color[] colors = new Color[] {
        Color.white, Color.red, Color.blue, Color.green, Color.yellow, Color.magenta
    };

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        SO_OrderLayout layout = (SO_OrderLayout)target;

        GUILayout.Space(10);
        GUILayout.Label("Visual", EditorStyles.boldLabel);

        if (layout.Tiles == null || layout.Tiles.Length == 0) {
            GUILayout.Label("No tiles to display.");
            return;
        }

        // Find grid dimensions
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var tile in layout.Tiles) {
            if (tile.coord.x < minX) minX = tile.coord.x;
            if (tile.coord.x > maxX) maxX = tile.coord.x;
            if (tile.coord.y < minY) minY = tile.coord.y;
            if (tile.coord.y > maxY) maxY = tile.coord.y;
        }

        int gridWidth = maxX - minX + 1;
        int gridHeight = maxY - minY + 1;

        float cellSize = 20f;
        float gap = 2f;
        float borderGap = 4f;
        float totalWidth = gridWidth * cellSize + 2 * (gap + borderGap);
        float totalHeight = gridHeight * cellSize + 2 * (gap + borderGap);

        Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);

        // Draw the border
        Rect borderRect = new Rect(gridRect.x + gap, gridRect.y + gap, gridWidth * cellSize + 2 * borderGap, gridHeight * cellSize + 2 * borderGap);
        EditorGUI.DrawRect(new Rect(borderRect.x - 1, borderRect.y - 1, borderRect.width + 2, 1), Color.white); // Top border
        EditorGUI.DrawRect(new Rect(borderRect.x - 1, borderRect.y + borderRect.height, borderRect.width + 2, 1), Color.white); // Bottom border
        EditorGUI.DrawRect(new Rect(borderRect.x - 1, borderRect.y - 1, 1, borderRect.height + 2), Color.white); // Left border
        EditorGUI.DrawRect(new Rect(borderRect.x + borderRect.width, borderRect.y - 1, 1, borderRect.height + 2), Color.white); // Right border

        // Draw the grid using Tiles' coordinates
        Rect innerGridRect = new Rect(borderRect.x + borderGap, borderRect.y + borderGap, gridWidth * cellSize, gridHeight * cellSize);
        foreach (var tile in layout.Tiles) {
            Color color = colors[tile.colorID % colors.Length];
            int x = tile.coord.x - minX;
            int y = tile.coord.y - minY;
            Rect cellRect = new Rect(innerGridRect.x + x * cellSize, innerGridRect.y + (gridHeight - y - 1) * cellSize, cellSize, cellSize);
            EditorGUI.DrawRect(cellRect, color);
        }
    }
}
