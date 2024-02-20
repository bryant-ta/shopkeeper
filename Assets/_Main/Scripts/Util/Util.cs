using System;
using System.Collections;
using UnityEngine;

public class Util : Singleton<Util> {
    public class ValueRef<T> where T : struct
    {
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
    public void DoAfterSeconds(MonoBehaviour obj, float seconds, ValueRef<bool> interrupt, Action action) {
        Debug.Log(seconds);
        obj.StartCoroutine(DoAfterSecondsCoroutine(seconds, interrupt, action));
    } 
    IEnumerator DoAfterSecondsCoroutine(float seconds, ValueRef<bool> interrupt, Action onComplete) {
        float t = 0f;
        Debug.Log(seconds);
        while (t < seconds) {
            t += Time.deltaTime * GlobalClock.TimeScale;
            Debug.Log(interrupt.Value);
            if (!interrupt.Value) {
                Debug.Log("canceled!");
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
}