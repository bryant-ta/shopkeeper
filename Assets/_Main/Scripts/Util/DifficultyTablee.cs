using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class DifficultyTablee<T> {
    [Serializable]
    public struct Entry {
        [Range(0f,1f)] public float Difficulty;
        public T Obj;
    }

    [SerializeField] List<Entry> entries;

    /// <summary>
    /// Returns random T from Objs with difficulty threshold under Difficulty.
    /// </summary>
    public T GetRandomByDifficulty() {
        List<T> possible = FilterByDifficulty();
        return possible[Random.Range(0, possible.Count)];
    }

    /// <summary>
    /// Returns T for highest valid difficulty threshold.
    /// </summary>
    public T GetHighestByDifficulty() {
        return entries
            .Where(entry => entry.Difficulty <= GameManager.Instance.Difficulty)
            .OrderByDescending(entry => entry.Difficulty)
            .FirstOrDefault().Obj;
    }

    public List<T> FilterByDifficulty() {
        return entries
            .Where(entry => entry.Difficulty <= GameManager.Instance.Difficulty)
            .Select(entry => entry.Obj)
            .ToList();
    }
}
