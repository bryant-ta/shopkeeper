using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

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
        [Group("Requirements")] [Tooltip("reqChanceNeedsColor")]
        [Range(0f, 1f)] public float reqChanceNeedsColor;
        [Group("Requirements")] [Tooltip("reqChanceNeedsShape")]
        [Range(0f, 1f)] public float reqChanceNeedsShape;

        [Group("Virtual Reqs")] [Tooltip("reqChanceFromExisting")]
        [Range(0f, 1f)] public float reqChanceFromExisting;
        [Group("Virtual Reqs")] [ListDrawerSettings(AlwaysExpanded = true)] // moved out of group for better UI
        public List<ShapeDataID> reqVirtualShapePool;

        [Group("Mold")]
        [Range(0f, 1f)] public float moldChance;
        [Group("Mold")] [ListDrawerSettings(AlwaysExpanded = true)]
        public List<ShapeDataID> moldShapePool;
    }
}