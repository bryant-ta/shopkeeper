using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Difficulty/SO_OrdersDifficultyTable")]
public class SO_OrdersDifficultyTable : SO_DifficultyTableBase<SO_OrdersDifficultyTable.OrderDifficultyEntry> {
    [Serializable]
    public class OrderDifficultyEntry : IDifficultyEntry {
        [field: SerializeField] public int day { get; set; }

        public int layoutDifficulty;
        public int numNeedOrdersFulfilled;
        public int numActiveDocks;
        public int baseOrderTime;
        public int baseOrderValue;
    }
}