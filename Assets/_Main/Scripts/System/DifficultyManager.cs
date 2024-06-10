using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class DifficultyManager : MonoBehaviour {
    [SerializeField] SO_DeliveriesDifficultyTable deliveryDiffTable;
    [SerializeField] SO_OrdersDifficultyTable orderDiffTable;
    
    [SerializeField] SO_DeliveriesDifficultyTable deliveryDiffTableOverride;
    [SerializeField] SO_OrdersDifficultyTable orderDiffTableOverride;

    public void ApplyDifficulty() {
        Ref.Instance.DeliveryMngr.SetDifficultyOptions(deliveryDiffTable, deliveryDiffTableOverride);
    }
}

public abstract class SO_DifficultyTableBase<TEntry> : ScriptableObject where TEntry : class, IDifficultyEntry {
    [TableList(Draggable = true, HideAddButton = false, HideRemoveButton = false, AlwaysExpanded = true)]
    public List<TEntry> table;

    /// <summary>
    /// Returns T of highest valid day less than or equal to Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public T GetHigh<T>(Func<TEntry, T> selector) {
        TEntry highestEntry = table
            .Where(entry => entry.day <= GameManager.Instance.Difficulty)
            .OrderByDescending(entry => entry.day)
            .FirstOrDefault();

        return highestEntry != null ? selector(highestEntry) : default;
    }

    /// <summary>
    /// Returns true if exact day entry equal to Difficulty exists. If exists, output contains requested value.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    /// <param name="difficulty">Target day.</param>
    /// <param name="output">If day entry exists, output contains requested value.</param>
    public bool GetExact<T>(Func<TEntry, T> selector, int difficulty, out T output) {
        List<TEntry> entries = table.Where(entry => entry.day == difficulty).ToList();

        if (entries.Count == 0) {
            output = default;
            return false;
        }

        output = selector(entries[0]);
        return true;
    }
    
    /// <summary>
    /// Returns lerped float between existing entry values using current Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public float GetFloatLerp(Func<TEntry, float> selector) {
        // Entry exists for target day, no need to lerp.
        if (GetExact(selector, GameManager.Instance.Difficulty, out float output)) {
            return output;
        }
        
        // Search min bound
        int target = GameManager.Instance.Difficulty;
        float minBound = float.MinValue;
        int minDay;
        for (minDay = target; minDay >= 0; minDay--) {
            if (GetExact(selector, minDay, out float min)) {
                minBound = min;
                break;
            }
        }
        if (Math.Abs(minBound - float.MinValue) < 0.1f) {
            minBound = 0;
        }
        
        // Search max bound
        float maxBound = float.MinValue;
        int maxDay;
        int totalDays = GameManager.Instance.TotalDays;
        for (maxDay = target; maxDay <= totalDays; maxDay++) {
            if (GetExact(selector, maxDay, out float max)) {
                maxBound = max;
                break;
            }
        }
        if (Math.Abs(maxBound - float.MinValue) < 0.1f) {
            maxBound = 1f;
        }
        
        // Determine where target stands relative to existing day entries' values
        return Mathf.Lerp(minBound, maxBound, (float) (target - minDay) / (maxDay - minDay));
    }

    /// <summary>
    /// Returns random T from Objs with day less than or equal to Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public T GetRandomUnder<T>(Func<TEntry, T> selector) {
        List<T> possible = Filter(selector);
        return possible[Random.Range(0, possible.Count)];
    }

    /// <summary>
    /// Returns list of all Objs from days less than or equal to Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public List<T> Filter<T>(Func<TEntry, T> selector) {
        return table
            .Where(entry => entry.day <= GameManager.Instance.Difficulty)
            .Select(selector)
            .ToList();
    }

    /// <summary>
    /// Returns list of all Objs from days less than or equal to Difficulty (IEnumerable version).
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public List<T> Filter<T>(Func<TEntry, IEnumerable<T>> selector) {
        return table
            .Where(entry => entry.day <= GameManager.Instance.Difficulty)
            .SelectMany(selector)
            .ToList();
    }
}

public interface IDifficultyEntry {
    public int day { get; }
}
