using System;
using UnityEngine;

// TODO: make responsive to start clock time, rather than adjusting curve to fit start clock time
[RequireComponent(typeof(Light))]
public class DayNightLighting : MonoBehaviour {
    [Tooltip("Defines intensity of sunlight over the day.")]
    [SerializeField] AnimationCurve lightIntensityCurve;
    [Tooltip("Defines the rotation of sunlight over the day on x-axis.")]
    [SerializeField] AnimationCurve lightXRotationCurve;
    [Tooltip("Defines the rotation of sunlight over the day on y-axis.")]
    [SerializeField] AnimationCurve lightYRotationCurve;

    Light sunlight;

    float startTimeOffset;

    void Awake() { sunlight = GetComponent<Light>(); }

    void Update() {
        // Get time of day as remaining percent, offset based on day clock start time
        float timeOfDay = (GameManager.Instance.RunTimer.TimeElapsedSeconds / GameManager.Instance.RunTimer.Duration * (1 - startTimeOffset)) + startTimeOffset;
        
        // Update the intensity of the sunlight based on the time of day
        float lightIntensity = lightIntensityCurve.Evaluate(timeOfDay);
        sunlight.intensity = lightIntensity;

        // Rotate the sun around the Y-axis to simulate its movement
        float rotationAngleX = lightXRotationCurve.Evaluate(timeOfDay) * 35f + 10f;
        float rotationAngleY = lightYRotationCurve.Evaluate(timeOfDay) * 180f + -45f;
        sunlight.transform.rotation = Quaternion.Euler(rotationAngleX, rotationAngleY, 0f);
    }
}