using System;
using System.Collections.Generic;
using UnityEngine;

namespace Timers {
public abstract class TimerBase {
    public float Duration { get; }
    public bool IsTicking { get; protected set; }

    public abstract float RemainingTimePercent { get; }
    public abstract float RemainingTimeSeconds { get; }

    public event Action EndEvent;

    protected float timer = 0f;

    protected TimerBase(float duration) { Duration = duration; }

    public virtual void Start() {
        if (IsTicking) {
            Debug.LogWarning("Timer has already started!");
            return;
        }

        IsTicking = true;
        GlobalClock.OnTick += Tick;
    }

    public virtual void Stop() {
        IsTicking = false;
        GlobalClock.OnTick -= Tick;
        EndEvent?.Invoke();
    }

    protected abstract void Tick(float deltaTime);
}

public class CountdownTimer : TimerBase {
    public override float RemainingTimePercent => timer / Duration;
    public override float RemainingTimeSeconds => timer;

    public event Action<float> TickEvent;

    public CountdownTimer(float duration) : base(duration) { }

    public override void Start() {
        timer = Duration;
        TickEvent?.Invoke(RemainingTimePercent);
        base.Start();
    }

    protected override void Tick(float deltaTime) {
        timer -= deltaTime;

        if (timer < 0) timer = 0; // Ensure non-negative percent for TickEvent
        TickEvent?.Invoke(RemainingTimePercent);

        if (timer <= 0f) {
            Stop();
            return;
        }
    }
}

public class StageTimer : TimerBase {
    public override float RemainingTimePercent => 1 - (timer / Duration);
    public override float RemainingTimeSeconds => Duration - timer;

    public event Action TickEvent;

    List<float> intervals;
    int curIntervalIndex = 0;

    public StageTimer(float duration, List<float> intervals) : base(duration) { this.intervals = intervals; }

    public override void Start() {
        timer = 0f;
        TickEvent?.Invoke();
        base.Start();
    }

    protected override void Tick(float deltaTime) {
        timer += deltaTime;

        if (curIntervalIndex < intervals.Count && timer >= intervals[curIntervalIndex]) {
            TickEvent?.Invoke();
            curIntervalIndex++;
        }

        if (timer >= Duration) {
            Stop();
            return;
        }
    }
}
}