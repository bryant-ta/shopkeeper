using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Difficulty/SO_DeliveriesDifficultyTable")]
public class SO_DeliveriesDifficultyTable : SO_DifficultyTableBase<SO_DeliveriesDifficultyTable.DeliveryDifficultyEntry> {
    [Serializable]
    public class DeliveryDifficultyEntry : IDifficultyEntry {
        public int day { get; set; }

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
        public List<ShapeDataID> irregularShapes;
    }
}