using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : Singleton<SoundManager> {
    [SerializeField] Sound[] sounds;

    AudioSource audioSource;
    
    void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(SoundID soundID) {
        Sound sound = GetSound(soundID);
        audioSource.PlayOneShot(sound.AudioClip);
    }

    public void PlaySound(SoundID soundID, float pitch) {
        // TODO: play sound with modified pitch from audiosource pool
    }
    
    public void PlaySound(SoundID soundID, float pitchLowLimit, float pitchHighLimit) {
        // TODO: play sound with modified random pitch in range from audiosource pool
    }

    public Sound GetSound(SoundID soundID) {
        return sounds.Single(sound => sound.ID == soundID);
    }
}

[Serializable]
public class Sound {
    [field:SerializeField] public SoundID ID { get; private set; }
    [field:SerializeField] public AudioClip AudioClip { get; private set; }
}

public enum SoundID {
    Blank = 0,
    ProductHold = 20,
    ProductPickUp = 21,
    ProductPlace = 22,
    ProductInvalidShake = 23,
    OrderFulfilled = 40,
    OrderFailed = 41,
    OrderProductFilled = 42,
    EnterOrderPhase = 50,
    EnterDeliveryPhase = 51,
    // Footstep = 200,  // reserved
    CartMove = 210,
}
