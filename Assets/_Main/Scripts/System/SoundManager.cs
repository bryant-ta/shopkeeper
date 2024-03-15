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

    Sound GetSound(SoundID soundID) {
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
    ProductMove
}
