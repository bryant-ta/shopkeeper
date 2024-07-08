using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Util : MonoBehaviour {
    #region General

    public static int CompareTime(string timeA, string timeB) {
        if (!DateTime.TryParse(timeA, out DateTime t1)) {
            Debug.LogError($"Unable to parse time string {timeA}");
            return 0;
        }

        if (!DateTime.TryParse(timeB, out DateTime t2)) {
            Debug.LogError($"Unable to parse time string {timeB}");
            return 0;
        }

        return DateTime.Compare(t1, t2);
    }

    /// <summary>
    /// Checks if target point is left or right of an object defined with a forward/up
    /// </summary>
    /// <param name="forward">forward vector of reference point</param>
    /// <param name="up">up vector of reference point</param>
    /// <param name="targetDir">test point</param>
    /// <returns>left -> -1, right -> 1, directly in front/behind -> 0</returns>
    public static float IsLeftOrRight(Vector3 forward, Vector3 up, Vector3 targetDir) {
        Vector3 perp = Vector3.Cross(forward, targetDir);
        float dir = Vector3.Dot(perp, up);

        return dir switch {
            > 0f => 1f,
            < 0f => -1f,
            _ => 0f
        };
    }

    public static T GetRandomEnumValue<T>() where T : Enum {
        Array values = Enum.GetValues(typeof(T));
        return (T) values.GetValue(Random.Range(0, values.Length));
    }

    public static T GetRandomFromList<T>(List<T> list) {
        if (list == null || list.Count == 0) {
            Debug.LogError("Unable to return random from list: list is null or empty.");
            return default;
        }

        return list[Random.Range(0, list.Count)];
    }

    public static void DictIntAdd<T>(Dictionary<T, int> dict, T key, int value) {
        if (dict.ContainsKey(key)) {
            dict[key] += value;
        } else {
            dict[key] = value;
        }
    }

    public static bool IsSubsetOf(List<Vector3Int> subset, List<Vector3Int> superset) {
        if (subset.Count > superset.Count) return false;
        
        HashSet<Vector3Int> supersetSet = new HashSet<Vector3Int>(superset);
        for (int i = 0; i < subset.Count; i++) {
            if (!supersetSet.Contains(subset[i])) {
                return false;
            }
        }
        return true;
    }

    public class ValueRef<T> where T : struct {
        public T Value { get; set; }
        public ValueRef(T value) { Value = value; }
    }

    #endregion

    #region Sequencing

    /// <summary>
    /// Executes action exactly one frame later from calling.
    /// </summary>
    /// <param name="obj">Script which will execute the coroutine (usually "this").</param>
    /// <param name="action">Function to execute.</param>
    /// <remarks>
    /// ALWAYS add comment above usage of this function explaining why a one frame delay is needed. Use sparingly...
    /// </remarks>
    public static void DoAfterOneFrame(MonoBehaviour obj, Action action) { obj.StartCoroutine(DoAfterOneFrameCoroutine(action)); }
    static IEnumerator DoAfterOneFrameCoroutine(Action action) {
        yield return null;
        action?.Invoke();
    }

    /// <summary>
    /// Executes action after seconds. Cancels action execution if interrupt is false.
    /// </summary>
    /// <param name="obj">Script which will execute the coroutine (usually "this").</param>
    /// <param name="seconds">Action execution delay.</param>
    /// <param name="interrupt">Condition for canceling action. MUST be passed by ref using a value reference wrapper.</param>
    /// <param name="action">Function to execute.</param>
    public static void DoAfterSeconds(MonoBehaviour obj, float seconds, Action action, ValueRef<bool> interrupt = null) {
        obj.StartCoroutine(DoAfterSecondsCoroutine(seconds, action, interrupt));
    }
    static IEnumerator DoAfterSecondsCoroutine(float seconds, Action onComplete, ValueRef<bool> interrupt = null) {
        float t = 0f;
        while (t < seconds) {
            t += Time.deltaTime * GlobalClock.TimeScale;
            if (interrupt is {Value: false}) {
                yield break;
            }

            yield return null;
        }

        onComplete?.Invoke();
    }

    #endregion

    #region Products

    public static Product GetProductFromShape(IGridShape shape) {
        if (shape == null) {
            Debug.LogError("Unexpected input shape: shape is null.");
            return null;
        }

        return shape.ColliderTransform.TryGetComponent(out Product product) ? product : null;
    }
    public static List<Product> GetProductsFromShapes(List<IGridShape> shapes) {
        List<Product> heldProducts = new();
        foreach (IGridShape shape in shapes) {
            Product p = GetProductFromShape(shape);
            if (p == null) {
                return null;
            }

            heldProducts.Add(p);
        }

        return heldProducts;
    }

    #endregion
}