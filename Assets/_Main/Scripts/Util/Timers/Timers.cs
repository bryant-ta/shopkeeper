using System;
using System.Collections.Generic;
using UnityEngine;

namespace Timers {
public abstract class TimerBase {
    public float Duration { get; protected set; }
    public bool IsTicking { get; protected set; }

    public abstract float TimeElapsedSeconds { get; }
    public abstract float RemainingTimeSeconds { get; }
    public abstract float RemainingTimePercent { get; }

    public event Action EndEvent;

    protected float timer = 0f;

    protected TimerBase(float duration) {
        Duration = duration;
        timer = duration;
    }

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
    }

    public virtual void End() {
        Stop();
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
        base.Start();
        TickEvent?.Invoke(RemainingTimePercent);
    }

    protected override void Tick(float deltaTime) {
        timer -= deltaTime;

        if (timer < 0) timer = 0; // Ensure non-negative percent for TickEvent
        TickEvent?.Invoke(RemainingTimePercent);

        if (timer <= 0f) {
            End();
            return;
        }
    }

    public void Reset() {
        timer = Duration;
        TickEvent?.Invoke(RemainingTimePercent);
        Stop();
    }

    public void AddTime(float value) {
        timer += value;
        TickEvent?.Invoke(RemainingTimePercent);
    }
    public void SetTime(float value) {
        timer = value;
        TickEvent?.Invoke(RemainingTimePercent);
    }

    public string ToStringMinuteSeconds() {
        int t = (int) timer + 1;
        int minutes = t / 60;
        int seconds = t % 60;
        return $"{minutes}:{seconds:D2}";
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
        base.Start();
        timer = 0f;
        TickEvent?.Invoke();
    }

    protected override void Tick(float deltaTime) {
        timer += deltaTime;

        if (curIntervalIndex < intervals.Count && timer >= intervals[curIntervalIndex]) {
            TickEvent?.Invoke();
            curIntervalIndex++;
        }

        if (timer >= Duration) {
            End();
            return;
        }
    }
}

public class ClockTimer : TimerBase {
    public override float TimeElapsedSeconds => timer;
    public override float RemainingTimeSeconds => Duration - timer;
    public override float RemainingTimePercent => 1 - (timer / Duration);

    public DateTime StartClockTime; // Time on clock that the timer will start
    public DateTime EndClockTime;   // Time on clock that the timer will end
    float clockTickDurationSeconds; // Real-time duration until clock moves to next step (seconds)
    int clockTickStepMinutes;       // Increment of time on clock that clock will move after tick duration (minutes)

    public string ClockTime => parsedClockTime.ToString("h:mm tt");
    DateTime parsedClockTime;

    float clockTickTimer = 0f;

    public event Action<string> TickEvent;

    /// <remarks>
    /// duration input is ignored, will be calculated from other inputs
    /// </remarks>
    public ClockTimer(float duration, string startClockTime, string endClockTime, float clockTickDurationSeconds,
        int clockTickStepMinutes) : base(
        CalculateRealTimeDuration(
            startClockTime, endClockTime, clockTickDurationSeconds,
            clockTickStepMinutes
        )
    ) {
        if (!DateTime.TryParse(startClockTime, out StartClockTime)) {
            Debug.LogError("Unable to parse start time string.");
            return;
        }

        if (!DateTime.TryParse(endClockTime, out EndClockTime)) {
            Debug.LogError("Unable to parse end time string.");
            return;
        }

        this.clockTickDurationSeconds = clockTickDurationSeconds;
        this.clockTickStepMinutes = clockTickStepMinutes;

        parsedClockTime = StartClockTime;
    }

    public override void Start() {
        base.Start();
        timer = 0f;
        TickEvent?.Invoke(ClockTime);
    }

    protected override void Tick(float deltaTime) {
        timer += deltaTime;
        clockTickTimer += deltaTime;

        if (clockTickTimer >= clockTickDurationSeconds) {
            clockTickTimer = 0f;
            TickEvent?.Invoke(AddTickStep());
        }

        if (parsedClockTime >= EndClockTime) {
            End();
            return;
        }
    }

    public void Reset() {
        parsedClockTime = StartClockTime;
        timer = 0f;
        clockTickTimer = 0f;
    }

    string AddTickStep() {
        parsedClockTime = parsedClockTime.AddMinutes(clockTickStepMinutes);
        return ClockTime;
    }

    static float CalculateRealTimeDuration(string startClockTime, string endClockTime, float clockTickDurationSeconds,
        int clockTickStepMinutes) {
        DateTime startTime;
        if (!DateTime.TryParse(startClockTime, out startTime)) {
            Debug.LogError("Unable to parse input time string.");
            return 0f;
        }

        DateTime endTime;
        if (!DateTime.TryParse(endClockTime, out endTime)) {
            Debug.LogError("Unable to parse input time string.");
            return 0f;
        }

        TimeSpan totalClockDuration = endTime - startTime;
        if (totalClockDuration < TimeSpan.Zero) { // e.g. 8AM-12AM gives negative totalClockDuration, so convert to actual difference
            totalClockDuration = TimeSpan.FromDays(1) + totalClockDuration;
        }

        int numClockTicks = (int) totalClockDuration.TotalMinutes / clockTickStepMinutes;
        float realTimeDuration = numClockTicks * clockTickDurationSeconds;

        return realTimeDuration;
    }
}
}