using System;
using System.Collections.Generic;
using UnityEngine;

namespace Timers {
public abstract class TimerBase {
    public float Duration { get; }
    public bool IsTicking { get; protected set; }

    public abstract float TimeElapsedSeconds { get; }
    public abstract float RemainingTimeSeconds { get; }
    public abstract float RemainingTimePercent { get; }

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
    public override float TimeElapsedSeconds => Duration - timer;
    public override float RemainingTimeSeconds => timer;
    public override float RemainingTimePercent => timer / Duration;

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
    public override float TimeElapsedSeconds => timer;
    public override float RemainingTimeSeconds => Duration - timer;
    public override float RemainingTimePercent => 1 - (timer / Duration);

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

public class ClockTimer : TimerBase {
    public override float TimeElapsedSeconds => timer;
    public override float RemainingTimeSeconds => Duration - timer;
    public override float RemainingTimePercent => 1 - (timer / Duration);

    string startClockTime;        // Time on clock that the timer will start
    string endClockTime;          // Time on clock that the timer will end
    float clockTickDurationSeconds; // Real-time duration until clock moves to next step (seconds)
    int clockTickStepMinutes;     // Increment of time on clock that clock will move after tick duration (minutes)

    public string ClockTime { get; private set; }

    public event Action<string> TickEvent;

    public ClockTimer(float duration, string startClockTime, string endClockTime, float clockTickDurationSeconds,
        int clockTickStepMinutes) : base(duration) {
        this.startClockTime = startClockTime;
        this.endClockTime = endClockTime;
        this.clockTickDurationSeconds = clockTickDurationSeconds;
        this.clockTickStepMinutes = clockTickStepMinutes;

        if (!DateTime.TryParse(startClockTime, out DateTime parsedTime)) {
            Debug.LogError("Unable to parse input time string.");
            return;
        }

        ClockTime = parsedTime.ToString("h:mm tt");
    }

    public override void Start() {
        timer = 0f;
        TickEvent?.Invoke(ClockTime);
        base.Start();
    }

    float clockTickTimer = 0f;
    protected override void Tick(float deltaTime) {
        timer += deltaTime;
        clockTickTimer += deltaTime;

        if (clockTickTimer >= clockTickDurationSeconds) {
            clockTickTimer = 0f;
            TickEvent?.Invoke(AddTickStep());
        }

        if ((int)Duration != -1 && timer >= Duration) {
            Stop();
            return;
        }
    }

    string AddTickStep() {
        if (!DateTime.TryParse(ClockTime, out DateTime parsedTime)) {
            Debug.LogError("Unable to parse input time string.");
            return ClockTime;
        }

        parsedTime = parsedTime.AddMinutes(clockTickStepMinutes);
        ClockTime = parsedTime.ToString("h:mm tt");

        return ClockTime;
    }
}
}