using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RainSound : MonoBehaviour
{
    [SerializeField] private AudioSource rainAudio; // Ensure this is set in the Inspector

    private ParticleSystem rainParticleSystem;

    void Start()
    {
        rainParticleSystem = GetComponent<ParticleSystem>();
        if (rainAudio == null)
        {
            rainAudio = GetComponent<AudioSource>();
        }
    }

    void OnEnable()
    {
        // Start the sound when the particle system is enabled
        if (rainAudio != null && !rainAudio.isPlaying)
        {
            rainAudio.Play();
        }
    }

    void OnDisable()
    {
        // Stop the sound when the particle system is disabled
        if (rainAudio != null && rainAudio.isPlaying)
        {
            rainAudio.Stop();
        }
    }
}
