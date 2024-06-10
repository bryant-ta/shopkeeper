using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Difficulty/SO_OrdersDifficultyTable")]
public class SO_OrdersDifficultyTable : SO_DifficultyTableBase<SO_OrdersDifficultyTable.OrderDifficultyEntry> {
    [Serializable]
    public class OrderDifficultyEntry : IDifficultyEntry {
        [field: SerializeField] public int day { get; set; }

        public int numActiveDocks;
        public int baseOrderTime;
        public int baseOrderValue;

        [Group("Requirements")]
        public int reqMaxNum;
        [Group("Requirements")]
        public int reqMaxQuantity;
        [Group("Requirements")] [Tooltip("reqChanceFromExisting")]
        [Range(0f, 1f)] public float reqChanceFromExisting;
        [Group("Requirements")] [Tooltip("reqChanceNeedsColor")]
        [Range(0f, 1f)] public float reqChanceNeedsColor;
        [Group("Requirements")] [Tooltip("reqChanceNeedsShape")]
        [Range(0f, 1f)] public float reqChanceNeedsShape;

        [ListDrawerSettings(AlwaysExpanded = true)] // moved out of group for better UI
        public List<ShapeDataID> reqShapePool;

        [Group("Mold")]
        [Range(0f, 1f)] public float moldChance;
        [Group("Mold")] [ListDrawerSettings(AlwaysExpanded = true)]
        public List<ShapeDataID> moldShapePool;
    }
}