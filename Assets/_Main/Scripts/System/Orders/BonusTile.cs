using System;
using System.Collections.Generic;
using Timers;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(CellOutlineRenderer))]
public class BonusTile : MonoBehaviour {
    [field: SerializeField] public BonusTileType Type { get; private set; }
    [field: SerializeField] public Vector3Int Coord { get; private set; }
    [SerializeField] int value;
    [SerializeField] Color color;
    [SerializeField, ReadOnly] float duration;

    CellOutlineRenderer cor;

    public CountdownTimer LifetimeTimer { get; private set; }

    public event Action<BonusTile> OnDurationReached;

    void Awake() {
        cor = GetComponent<CellOutlineRenderer>();
    }

    public void Init(Vector3Int coord, float duration) {
        Coord = coord;
        this.duration = duration;

        transform.localPosition = coord;

        LifetimeTimer = new CountdownTimer(duration);
        LifetimeTimer.TickEvent += cor.ScaleX;
        LifetimeTimer.EndEvent += TriggerEndOfLifetime;
        
        LifetimeTimer.Start();
        
        cor.Render(new ShapeData{RootCoord = Vector3Int.zero, ShapeOffsets = new List<Vector3Int>{Vector3Int.zero}}, color);
    }

    public void Execute() {
        switch (Type) {
            case BonusTileType.Score:
                GameManager.Instance.ModifyGlobalScoreMult(value);
                break;
            case BonusTileType.Time:
                GameManager.Instance.AddRunTime(value);
                break;
        }
    }

    void TriggerEndOfLifetime() {
        OnDurationReached?.Invoke(this);
    }

    public enum BonusTileType {
        Score = 0,
        Time = 1,
    }
}
