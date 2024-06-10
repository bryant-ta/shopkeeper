using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Difficulty/SO_OrdersDifficultyTable")]
public class SO_OrdersDifficultyTable : SO_DifficultyTableBase<SO_OrdersDifficultyTable.OrderDifficultyEntry> {
    [Serializable]
    public class OrderDifficultyEntry : IDifficultyEntry {
        [field: SerializeField] public int day { get; set; }

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
}