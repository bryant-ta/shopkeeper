using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMax))]
public class MinMaxDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var minRect = new Rect(position.x, position.y, position.width / 2 - 2, position.height);
        var maxRect = new Rect(position.x + position.width / 2 + 2, position.y, position.width / 2 - 2, position.height);

        SerializedProperty minProp = property.FindPropertyRelative("Min");
        SerializedProperty maxProp = property.FindPropertyRelative("Max");

        EditorGUI.PropertyField(minRect, minProp, GUIContent.none);
        EditorGUI.PropertyField(maxRect, maxProp, GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}