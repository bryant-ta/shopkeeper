using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Grid))]
public class DictionaryInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        Grid targetObject = (Grid) target;

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Cells");

        foreach (KeyValuePair<Vector3Int, Cell> cell in targetObject.Cells) {
            EditorGUILayout.LabelField(cell.Key.ToString(), cell.Value.Shape.Name);
        }
    }
}