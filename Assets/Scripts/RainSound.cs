using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainSound : MonoBehaviour
{
    public AudioClip environmentClip;    // Background sound, e.g., ambient rain sound
    public AudioClip triggerClip;        // Triggered sound, e.g., thunder when rain intensifies

    private AudioSource environmentAudioSource;  // Main looping sound
    private AudioSource triggerAudioSource;      // Sound played on specific triggers
    private ParticleSystem particleSystem;       // Reference to the particle system

    private bool isPlayingEnvironmentSound = false;  // Tracks environment sound status
    private bool triggeredSoundPlayed = false;       // Tracks if the trigger sound has been played

    void Start()
    {
        // Initialize the particle system and audio sources
        particleSystem = GetComponent<ParticleSystem>();

        // Set up the audio source for the environment sound
        environmentAudioSource = gameObject.AddComponent<AudioSource>();
        environmentAudioSource.clip = environmentClip;
        environmentAudioSource.loop = true;
        environmentAudioSource.volume = 0.5f;

        // Set up the audio source for the trigger sound
        triggerAudioSource = gameObject.AddComponent<AudioSource>();
        triggerAudioSource.clip = triggerClip;
        triggerAudioSource.loop = false;
        triggerAudioSource.volume = 0.7f;
    }

    void Update()
    {
        // Play the environment sound only when the particle system is actively emitting
        if (particleSystem.isEmitting && !isPlayingEnvironmentSound)
        {
            environmentAudioSource.Play();
            isPlayingEnvironmentSound = true;
        }
        else if (!particleSystem.isEmitting && isPlayingEnvironmentSound)
        {
            environmentAudioSource.Stop();
            isPlayingEnvironmentSound = false;
        }

        // Play the trigger sound based on a custom condition (e.g., particle count or other triggers)
        if (ShouldPlayTriggerSound())
        {
            PlayTriggerSound();
        }
    }

    private bool ShouldPlayTriggerSound()
    {
        // Example condition: play trigger sound when particle count exceeds a threshold
        if (!triggeredSoundPlayed && particleSystem.particleCount > 50)
        {
            return true;
        }
        return false;
    }

    private void PlayTriggerSound()
    {
        triggerAudioSource.Play();
        triggeredSoundPlayed = true;
    }
}
