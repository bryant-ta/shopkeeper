using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    public static T Instance {
        get {
            if (instance == null) {
                T[] instances = FindObjectsByType<T>(FindObjectsSortMode.None);
                if (instances == null || instances.Length == 0) {
                    Debug.LogError("Missing instance of singleton " + typeof(T) + ".");
                    return null;
                } else if (instances.Length > 1) {
                    Debug.LogError("Multiple instances of singleton " + typeof(T) + ".");
                    return null;
                }

                instance = instances[0];
            }

            return instance;
        }
    }

    static T instance;
}