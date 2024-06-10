using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Difficulty/SO_OrdersDifficultyTable")]
public class SO_OrdersDifficultyTable : ScriptableObject {
    [TableList(Draggable = true, HideAddButton = false, HideRemoveButton = false, AlwaysExpanded = true)]
    public List<OrderDifficultyEntry> table;

    [Serializable]
    public class OrderDifficultyEntry {
        public int day;

        public int numActiveDocks;
        [Range(0f, 1f)] public float baseOrderTime;
        [Range(0f, 1f)] public float baseOrderValue;

        [Group("Requirements")]
        public int numReqs;
        [Group("Requirements")]
        public int quantity;
        [Group("Requirements")]
        [Range(0f, 1f)] public float chanceFromExisting;
        [Group("Requirements")]
        [Range(0f, 1f)] public float chanceNeedsColor;
        [Group("Requirements")]
        [Range(0f, 1f)] public float chanceNeedsShape;
        
        [ListDrawerSettings(AlwaysExpanded = true)] // moved out of group for better UI
        public List<ShapeDataID> reqShapes;

        [Group("Mold")]
        [Range(0f, 1f)] public float moldChance;
        [Group("Mold")] [ListDrawerSettings(AlwaysExpanded = true)]
        public List<ShapeDataID> moldShapes;
    }

    /// <summary>
    /// Returns T of highest valid day less than or equal to Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public T GetHighestByDifficulty<T>(Func<OrderDifficultyEntry, T> selector) {
        return selector(
            table
                .Where(entry => entry.day <= GameManager.Instance.Difficulty)
                .OrderByDescending(entry => entry.day)
                .FirstOrDefault()
        );
    }

    /// <summary>
    /// Returns random T from Objs with day less than or equal to Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public T GetRandomByDifficulty<T>(Func<OrderDifficultyEntry, T> selector) {
        List<T> possible = FilterByDifficulty(selector);
        return possible[Random.Range(0, possible.Count)];
    }

    /// <summary>
    /// Returns list of all Objs from days less than or equal to Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public List<T> FilterByDifficulty<T>(Func<OrderDifficultyEntry, T> selector) {
        return table
            .Where(entry => entry.day <= GameManager.Instance.Difficulty)
            .Select(selector)
            .ToList();
    }

    /// <summary>
    /// Returns list of all Objs from days less than or equal to Difficulty (IEnumerable version).
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public List<T> FilterByDifficulty<T>(Func<OrderDifficultyEntry, IEnumerable<T>> selector) {
        return table
            .Where(entry => entry.day <= GameManager.Instance.Difficulty)
            .SelectMany(selector)
            .ToList();
    }
}