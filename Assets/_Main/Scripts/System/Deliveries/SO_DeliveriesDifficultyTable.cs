using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Difficulty/SO_DeliveriesDifficultyTable")]
public class SO_DeliveriesDifficultyTable : ScriptableObject {
    [TableList(Draggable = true, HideAddButton = false, HideRemoveButton = false, AlwaysExpanded = true)]
    public List<DeliveryDifficultyEntry> table;

    [Serializable]
    public class DeliveryDifficultyEntry {
        public int day;

        public int numDeliveries;
        public int maxColorIndex;
        [ListDrawerSettings(AlwaysExpanded = true)]
        public List<GameObject> deliveryBoxes;

        [Group("Basic Delivery")]
        public int basicMaxLength;
        [Group("Basic Delivery")]
        public int basicMaxWidth;
        [Group("Basic Delivery")]
        [Range(0f, 1f)] public float basicChanceShapeExtension;
        [Group("Basic Delivery")]
        [Range(0f, 1f)] public float basicOrderliness;

        [Group("Irregular Delivery")]
        [Range(0f, 1f)] public float irregularChance;
        [Group("Irregular Delivery")] [ListDrawerSettings(AlwaysExpanded = true)]
        public List<GameObject> irregularShapes;
    }

    /// <summary>
    /// Returns T of highest valid day less than or equal to Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public T GetHighestByDifficulty<T>(Func<DeliveryDifficultyEntry, T> selector) {
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
    public T GetRandomByDifficulty<T>(Func<DeliveryDifficultyEntry, T> selector) {
        List<T> possible = FilterByDifficulty(selector);
        return possible[Random.Range(0, possible.Count)];
    }

    /// <summary>
    /// Returns list of all Objs from days less than or equal to Difficulty.
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public List<T> FilterByDifficulty<T>(Func<DeliveryDifficultyEntry, T> selector) {
        return table
            .Where(entry => entry.day <= GameManager.Instance.Difficulty)
            .Select(selector)
            .ToList();
    }

    /// <summary>
    /// Returns list of all Objs from days less than or equal to Difficulty (IEnumerable version).
    /// </summary>
    /// <param name="selector">Example: (entry => entry.desiredVar)</param>
    public List<T> FilterByDifficulty<T>(Func<DeliveryDifficultyEntry, IEnumerable<T>> selector) {
        return table
            .Where(entry => entry.day <= GameManager.Instance.Difficulty)
            .SelectMany(selector)
            .ToList();
    }
}