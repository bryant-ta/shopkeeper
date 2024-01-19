using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    public static T Instance {
        get {
            if (instance == null) {
                instance = (T) FindObjectOfType(typeof(T));

                if (instance == null) {
                    Debug.LogError("Missing instance of singleton " + typeof(T) + ".");
                }
            }

            return instance;
        }
    }
    protected static T instance;
}