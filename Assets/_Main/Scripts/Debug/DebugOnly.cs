using UnityEngine;

public class DebugOnly : MonoBehaviour {
    void Awake() {
        if (!DebugManager.DebugMode) {
            gameObject.SetActive(false);
        }
    }
}