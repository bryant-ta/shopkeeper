using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

// Usage: weights are relative to each other, actual value does not matter, only ratio
//   - 2 items, 50% chance each | 1,1
//   - 3 items (20%, 30%, 50%) | 2,3,5
// Supports weights of any sum (not just 100). Order does NOT matter.
// TODO: add seed setting
[Serializable]
public class RollTable<T> {
    [Serializable]
    struct Entry {
        public T item;
        public double weight;
    }
    
    [SerializeField, HideInPlayMode] List<Entry> initEntries = new();
    [SerializeField, HideInEditMode] List<Entry> entries = new();
    double accumulatedWeight;

    bool init = false;

    public void Add(T item, double weight) {
        // Difference between accumulatedWeight and last accumulatedWeight always equal to item's weight, no matter order of entries
        accumulatedWeight += weight;
        entries.Add(new Entry {item = item, weight = accumulatedWeight});
    }

    public T GetRandom() {
        if (!init) {
            for (int i = 0; i < initEntries.Count; i++) {
                Add(initEntries[i].item, initEntries[i].weight);
            }
            init = true;
        }
        
        double r = Random.value * accumulatedWeight;

        foreach (Entry entry in entries) {
            if (entry.weight >= r) {
                return entry.item;
            }
        }

        Debug.LogError($"Unable to get random entry for {r}.");
        return default(T);
    }

    public bool Contains(T item) {
        for (int i = 0; i < entries.Count; i++) {
            if (entries[i].item.Equals(item)) return true;
        }

        return false;
    }
}