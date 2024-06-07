using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour {
    public class ValueRef<T> where T : struct {
        public T Value { get; set; }
        public ValueRef(T value) { Value = value; }
    }

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
}