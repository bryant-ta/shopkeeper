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
        ApplyDeliveryDifficulty();
        ApplyOrderDifficulty();
    }

    void ApplyDeliveryDifficulty() {
        SO_DeliveriesDifficultyTable.DeliveryDifficultyEntry ret = new() {
            numDeliveries = deliveryDiffTable.GetHigh(entry => entry.numDeliveries),
            maxColorIndex = deliveryDiffTable.GetHigh(entry => entry.maxColorIndex),
            deliveryBoxPool = deliveryDiffTable.GetRandomUnder(entry => entry.deliveryBoxPool),
            basicMaxShapeLength = deliveryDiffTable.GetHigh(entry => entry.basicMaxShapeLength),
            basicMaxShapeWidth = deliveryDiffTable.GetHigh(entry => entry.basicMaxShapeWidth),
            basicChanceShapeExtension = deliveryDiffTable.GetHigh(entry => entry.basicChanceShapeExtension),
            basicOrderliness = deliveryDiffTable.GetHigh(entry => entry.basicOrderliness),
            irregularChance = deliveryDiffTable.GetHigh(entry => entry.irregularChance),
            irregularShapePool = deliveryDiffTable.GetRandomUnder(entry => entry.irregularShapePool)
        };

        // Apply overrides if they exist for current Difficulty
        if (deliveryDiffTableOverride.GetExact(entry => entry.numDeliveries, GameManager.Instance.Difficulty, out int o_numDeliveries)) {
            ret.numDeliveries = o_numDeliveries;
        }
        if (deliveryDiffTableOverride.GetExact(entry => entry.maxColorIndex, GameManager.Instance.Difficulty, out int o_maxColorIndex)) {
            ret.maxColorIndex = o_maxColorIndex;
        }
        if (deliveryDiffTableOverride.GetExact(
                entry => entry.deliveryBoxPool, GameManager.Instance.Difficulty, out List<GameObject> o_deliveryBoxPool
            )) {
            ret.deliveryBoxPool = o_deliveryBoxPool;
        }
        if (deliveryDiffTableOverride.GetExact(
                entry => entry.basicMaxShapeLength, GameManager.Instance.Difficulty, out int o_basicMaxShapeLength
            )) {
            ret.basicMaxShapeLength = o_basicMaxShapeLength;
        }
        if (deliveryDiffTableOverride.GetExact(
                entry => entry.basicMaxShapeWidth, GameManager.Instance.Difficulty, out int o_basicMaxShapeWidth
            )) {
            ret.basicMaxShapeWidth = o_basicMaxShapeWidth;
        }
        if (deliveryDiffTableOverride.GetExact(
                entry => entry.basicChanceShapeExtension, GameManager.Instance.Difficulty, out float o_basicChanceShapeExtension
            )) {
            ret.basicChanceShapeExtension = o_basicChanceShapeExtension;
        }
        if (deliveryDiffTableOverride.GetExact(
                entry => entry.basicOrderliness, GameManager.Instance.Difficulty, out float o_basicOrderliness
            )) {
            ret.basicOrderliness = o_basicOrderliness;
        }
        if (deliveryDiffTableOverride.GetExact(
                entry => entry.irregularChance, GameManager.Instance.Difficulty, out float o_irregularChance
            )) {
            ret.irregularChance = o_irregularChance;
        }
        if (deliveryDiffTableOverride.GetExact(
                entry => entry.irregularShapePool, GameManager.Instance.Difficulty, out List<ShapeDataID> o_irregularShapes
            )) {
            ret.irregularShapePool = o_irregularShapes;
        }
        
        Ref.Instance.DeliveryMngr.SetDifficultyOptions(ret);
    }
    void ApplyOrderDifficulty() {
        SO_OrdersDifficultyTable.OrderDifficultyEntry ret = new() {
            numActiveDocks = orderDiffTable.GetHigh(entry => entry.numActiveDocks),
            baseOrderTime = orderDiffTable.GetHigh(entry => entry.baseOrderTime),
            baseOrderValue = orderDiffTable.GetHigh(entry => entry.baseOrderValue),
            numReqs = orderDiffTable.GetHigh(entry => entry.numReqs),
            reqQuantity = orderDiffTable.GetHigh(entry => entry.reqQuantity),
            reqChanceFromExisting = orderDiffTable.GetHigh(entry => entry.reqChanceFromExisting),
            reqChanceNeedsColor = orderDiffTable.GetHigh(entry => entry.reqChanceNeedsColor),
            reqChanceNeedsShape = orderDiffTable.GetHigh(entry => entry.reqChanceNeedsShape),
            reqShapePool = orderDiffTable.GetRandomUnder(entry => entry.reqShapePool),
            moldChance = orderDiffTable.GetHigh(entry => entry.moldChance),
            moldShapePool = orderDiffTable.GetRandomUnder(entry => entry.moldShapePool)
        };

        // Apply overrides if they exist for current Difficulty
        if (orderDiffTableOverride.GetExact(entry => entry.numActiveDocks, GameManager.Instance.Difficulty, out int o_numActiveDocks)) {
            ret.numActiveDocks = o_numActiveDocks;
        }
        if (orderDiffTableOverride.GetExact(entry => entry.baseOrderTime, GameManager.Instance.Difficulty, out int o_baseOrderTime)) {
            ret.baseOrderTime = o_baseOrderTime;
        }
        if (orderDiffTableOverride.GetExact(entry => entry.baseOrderValue, GameManager.Instance.Difficulty, out int o_baseOrderValue)) {
            ret.baseOrderValue = o_baseOrderValue;
        }
        if (orderDiffTableOverride.GetExact(entry => entry.numReqs, GameManager.Instance.Difficulty, out int o_numReqs)) {
            ret.numReqs = o_numReqs;
        }
        if (orderDiffTableOverride.GetExact(entry => entry.reqQuantity, GameManager.Instance.Difficulty, out int o_quantity)) {
            ret.reqQuantity = o_quantity;
        }
        if (orderDiffTableOverride.GetExact(
                entry => entry.reqChanceFromExisting, GameManager.Instance.Difficulty, out float o_chanceFromExisting
            )) {
            ret.reqChanceFromExisting = o_chanceFromExisting;
        }
        if (orderDiffTableOverride.GetExact(
                entry => entry.reqChanceNeedsColor, GameManager.Instance.Difficulty, out float o_chanceNeedsColor
            )) {
            ret.reqChanceNeedsColor = o_chanceNeedsColor;
        }
        if (orderDiffTableOverride.GetExact(
                entry => entry.reqChanceNeedsShape, GameManager.Instance.Difficulty, out float o_chanceNeedsShape
            )) {
            ret.reqChanceNeedsShape = o_chanceNeedsShape;
        }
        if (orderDiffTableOverride.GetExact(entry => entry.reqShapePool, GameManager.Instance.Difficulty, out List<ShapeDataID> o_reqShapes)) {
            ret.reqShapePool = o_reqShapes;
        }
        if (orderDiffTableOverride.GetExact(entry => entry.moldChance, GameManager.Instance.Difficulty, out float o_moldChance)) {
            ret.moldChance = o_moldChance;
        }
        if (orderDiffTableOverride.GetExact(
                entry => entry.moldShapePool, GameManager.Instance.Difficulty, out List<ShapeDataID> o_moldShapes
            )) {
            ret.moldShapePool = o_moldShapes;
        }
        
        Ref.Instance.OrderMngr.SetDifficultyOptions(ret);
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