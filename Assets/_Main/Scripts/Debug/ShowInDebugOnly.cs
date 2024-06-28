using UnityEngine;

public class ShowInDebugOnly : MonoBehaviour {
    void Awake() {
        if (!DebugManager.DebugMode) {
            gameObject.SetActive(false);
        }
    }
}