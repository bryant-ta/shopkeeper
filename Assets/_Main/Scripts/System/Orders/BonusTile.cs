using System;
using System.Collections.Generic;
using Timers;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(CellOutlineRenderer))]
public class BonusTile : MonoBehaviour {
    [field: SerializeField] public Vector3Int Coord { get; private set; }
    [SerializeField] Color color;
    [SerializeField, ReadOnly] float duration;
    [SerializeField, ReadOnly] float mult;

    CellOutlineRenderer cor;

    public CountdownTimer LifetimeTimer { get; private set; }

    public event Action<BonusTile> OnDurationReached;

    void Awake() {
        cor = GetComponent<CellOutlineRenderer>();
    }

    public void Init(Vector3Int coord, float duration, float mult) {
        Coord = coord;
        this.duration = duration;
        this.mult = mult;

        transform.localPosition = coord;

        LifetimeTimer = new CountdownTimer(duration);
        LifetimeTimer.EndEvent += TriggerEndOfLifetime;
        LifetimeTimer.Start();
        
        cor.Render(new ShapeData{RootCoord = Vector3Int.zero, ShapeOffsets = new List<Vector3Int>{Vector3Int.zero}}, color);
    }

    void TriggerEndOfLifetime() {
        OnDurationReached?.Invoke(this);
    }
}
