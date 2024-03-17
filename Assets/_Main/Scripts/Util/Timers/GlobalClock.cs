using System;
using UnityEngine;

/// <summary>
/// GlobalClock is the ticker every other Timer type uses to tick. All Timers can be stopped by stopping GlobalClock.
/// - Script Execution Order: Place above GameManager (and any script which subs to OnTick)
/// </summary>
public class GlobalClock : Singleton<GlobalClock> {
	public static float TimeScale => timeScale;
	static float timeScale = 1f;

	/// <summary>
	/// OnTick sends deltaTime on invoke.
	/// </summary>
	public static event Action<float> OnTick;

	void Awake() {
		// Required to reset subscribers every Play mode start because static
		OnTick = null; // null is same as saying onTick has no subscribers
		timeScale = 1f;
	}

	void Update() {
		if (TimeScale == 0) return; // prevents ticks of 0 delta time
		OnTick?.Invoke(Time.deltaTime * TimeScale);
	}

	public static void SetTimeScale(float value) {
		timeScale = value;
	}
}
