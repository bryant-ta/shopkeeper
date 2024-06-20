using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Difficulty/SO_DeliveriesDifficultyTable")]
public class SO_DeliveriesDifficultyTable : SO_DifficultyTableBase<SO_DeliveriesDifficultyTable.DeliveryDifficultyEntry> {
    [Serializable]
    public class DeliveryDifficultyEntry : IDifficultyEntry {
        [field: SerializeField] public int day { get; set; }

        public int numDeliveries;
        public int maxColorIndex;
        [ListDrawerSettings(AlwaysExpanded = true)]
        public List<GameObject> deliveryBoxPool;

        [Group("Basic Delivery")] [Tooltip("First extension max length")]
        public int basicFirstDimensionMax;
        [Group("Basic Delivery")] [Tooltip("Second extension (orthagonal) max length")]
        public int basicSecondDimensionMax;
        [Group("Basic Delivery")] [Tooltip("basicChanceShapeExtension")]
        [Range(0f, 1f)] public float basicChanceShapeExtension;

        [Group("Irregular Delivery")]
        [Range(0f, 1f)] public float irregularChance;
        [Group("Irregular Delivery")] [ListDrawerSettings(AlwaysExpanded = true)]
        public List<ShapeDataID> irregularShapePool;
    }

    // Overrides
    // TEMP: think of a better way to do overrides
    [SerializeField] bool useOverride;
    [ShowIf(nameof(useOverride)), ListDrawerSettings(AlwaysExpanded = true)]
    public List<OverrideEntry> overrides;

    [Serializable]
    public struct OverrideEntry {
        public int day;
        public float irregularChance;
        public List<ShapeDataID> irregularShapePool;
    }

    public void UseOverrides(DeliveryDifficultyEntry diffEntry, int difficulty) {
        if (!useOverride) return;

        for (var i = 0; i < overrides.Count; i++) {
            OverrideEntry overrideEntry = overrides[i];
            if (overrideEntry.day == difficulty) {
                diffEntry.irregularChance = overrideEntry.irregularChance;
                diffEntry.irregularShapePool = overrideEntry.irregularShapePool;
            }
        }
    }
}