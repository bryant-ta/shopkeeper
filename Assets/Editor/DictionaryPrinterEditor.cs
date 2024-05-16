using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a button to print contents of dictionary on any MonoBehaviour, useful during runtime.
/// The script must implement: public void PrintDictionary().
/// </summary>
[CustomEditor(typeof(MonoBehaviour), true)]
public class DictionaryPrinterEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        MonoBehaviour targetScript = (MonoBehaviour) target;

        var methodInfo = targetScript.GetType().GetMethod("PrintDictionary");
        if (methodInfo != null && methodInfo.DeclaringType == targetScript.GetType()) {
            if (GUILayout.Button("Print Dictionary")) {
                methodInfo.Invoke(targetScript, null);
            }
        }
    }
}