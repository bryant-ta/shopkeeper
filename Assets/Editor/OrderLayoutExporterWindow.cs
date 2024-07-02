using UnityEditor;
using UnityEngine;

public class OrderLayoutExporterWindow : EditorWindow {
    OrderLayoutExporter orderLayoutExporter;
    string filePath = "Assets/_Main/Resources/OrderLayouts/";

    [MenuItem("Tools/Tilemap Exporter")]
    public static void ShowWindow() { GetWindow<OrderLayoutExporterWindow>("Tilemap Exporter"); }

    void OnGUI() {
        GUILayout.Label("Tilemap Exporter", EditorStyles.boldLabel);

        orderLayoutExporter = (OrderLayoutExporter) EditorGUILayout.ObjectField(
            "Tilemap Exporter", orderLayoutExporter, typeof(OrderLayoutExporter), true
        );

        filePath = EditorGUILayout.TextField("File Path", filePath);
        
        int difficultyRating = EditorGUILayout.IntField("Difficulty", 1);

        if (GUILayout.Button("Export Tilemap to ScriptableObject")) {
            if (orderLayoutExporter != null) {
                orderLayoutExporter.ExportToScriptableObject(filePath, difficultyRating);
            } else {
                Debug.LogError("Tilemap Exporter not assigned.");
            }
        }
    }
}