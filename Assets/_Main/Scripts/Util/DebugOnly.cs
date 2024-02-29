using UnityEngine;

public class DebugOnly : MonoBehaviour {
    void Awake() {
        if (!GameManager.Instance.DebugMode) {
            gameObject.SetActive(false);
        }
    }
}
