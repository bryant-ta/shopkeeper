using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Difficulty/SO_OrdersDifficultyTable")]
public class SO_OrdersDifficultyTable : SO_DifficultyTableBase<SO_OrdersDifficultyTable.OrderDifficultyEntry> {
    [Serializable]
    public class OrderDifficultyEntry : IDifficultyEntry {
        [field: SerializeField] public int day { get; set; }

        public int numNeedOrdersFulfilled;
        public int numActiveDocks;
        public int baseOrderTime;
        public int baseOrderValue;

        [Group("Requirements")]
        public int reqMaxNum;
        [Group("Requirements")]
        public int reqMaxQuantity;
        
        [Group("Virtual Reqs")] [ListDrawerSettings(AlwaysExpanded = true)] // moved out of group for better UI
        public List<ShapeDataID> reqVirtualShapePool;

        [Group("Mold")]
        [Range(0f, 1f)] public float moldChance;
        [Group("Mold")] [ListDrawerSettings(AlwaysExpanded = true)]
        public List<ShapeDataID> moldShapePool;
    }
}