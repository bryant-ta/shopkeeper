using System.Collections.Generic;
using UnityEngine;

// Supports weights of any sum (not just 100). Order does NOT matter.
public class RollTable<T> {
    struct Entry {
        public T item;
        public double weight;
    }
    
    List<Entry> _entries = new();
    double _accumulatedWeight;

    public void Add(T item, double weight) {
        // Difference between accumulatedWeight and last accumulatedWeight always equal to item's weight, no matter order of entries
        _accumulatedWeight += weight;
        _entries.Add(new Entry {item = item, weight = _accumulatedWeight});
    }

    System.Random _rand = new();
    public T GetRandom() {
        double r = _rand.NextDouble() * _accumulatedWeight;

        foreach (Entry entry in _entries) {
            if (entry.weight >= r) {
                return entry.item;
            }
        }

        Debug.LogError($"Unable to get random entry for {r}.");
        return default(T);
    }

    public bool Contains(T item) {
        for (int i = 0; i < _entries.Count; i++) {
            if (_entries[i].item.Equals(item)) return true;
        }

        return false;
    }
}