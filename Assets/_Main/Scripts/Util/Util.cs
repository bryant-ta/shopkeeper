using System;
using System.Collections;
using UnityEngine;

public class Util : MonoBehaviour {
    /// <summary>
    /// Executes action exactly one frame later from calling.
    /// </summary>
    /// <param name="obj">Script which will execute the coroutine (usually "this").</param>
    /// <param name="action">Function to execute</param>
    /// <remarks>
    /// ALWAYS add comment above usage of this function explaining why a one frame delay is needed. Use sparingly...
    /// </remarks>
    public static void DoAfterOneFrame(MonoBehaviour obj, Action action) { obj.StartCoroutine(DoAfterOneFrameCoroutine(action)); }
    static IEnumerator DoAfterOneFrameCoroutine(Action action) {
        yield return null;
        action?.Invoke();
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
}