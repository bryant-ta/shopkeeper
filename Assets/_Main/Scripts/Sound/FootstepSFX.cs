using Timers;
using UnityEngine;

// TEMP: current setup is simple triggered by player movement only. When have char animations,
// hook up to animation events and make generic for all characters with footsteps.
[RequireComponent(typeof(AudioSource))]
public class FootstepSFX : MonoBehaviour {
    [SerializeField] float stepRate;
    CountdownTimer footstepTimer;

    AudioSource footstepAs;

    void Awake() {
        footstepAs = GetComponent<AudioSource>();

        footstepTimer = new CountdownTimer(stepRate);
        footstepTimer.Start();

        // Ref.Player.PlayerMovement.OnMovement += Footstep;
    }

    void Footstep() {
        if (!footstepTimer.IsTicking) {
            footstepAs.pitch = 1f + Random.Range(-0.1f, 0.1f);
            footstepAs.Play();
            
            footstepTimer.Reset();
            footstepTimer.Start();
        }
    }
}