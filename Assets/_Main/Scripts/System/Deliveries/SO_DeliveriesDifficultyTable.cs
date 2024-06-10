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

        [Group("Basic Delivery")] [Tooltip("length")]
        public int basicMaxShapeLength;
        [Group("Basic Delivery")] [Tooltip("width")]
        public int basicMaxShapeWidth;
        [Group("Basic Delivery")] [Tooltip("basicChanceShapeExtension")]
        [Range(0f, 1f)] public float basicChanceShapeExtension;
        [Group("Basic Delivery")]
        [Range(0f, 1f)] public float basicOrderliness;

        [Group("Irregular Delivery")]
        [Range(0f, 1f)] public float irregularChance;
        [Group("Irregular Delivery")] [ListDrawerSettings(AlwaysExpanded = true)]
        public List<ShapeDataID> irregularShapePool;
    }
}