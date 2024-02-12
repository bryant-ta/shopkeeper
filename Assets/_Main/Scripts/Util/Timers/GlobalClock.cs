using System;
using UnityEngine;

/// <summary>
/// GlobalClock is the ticker every other Timer type uses to tick. All Timers can be stopped by stopping GlobalClock.
/// </summary>
public class GlobalClock : Singleton<GlobalClock> {
	public float timeScale = 1f;

	/// <summary>
	/// OnTick sends deltaTime on invoke.
	/// </summary>
	public static Action<float> OnTick;

	void Awake() {
		OnTick = null; // null is same as saying onTick has no subscribers
	}

	void Update() {
		if (timeScale == 0) return; // prevents ticks of 0 delta time
		OnTick?.Invoke(Time.deltaTime * timeScale);
	}
}
