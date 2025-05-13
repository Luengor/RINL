using UnityEngine;

public class BalloonPopEffect : MonoBehaviour
{
    public AudioClip[] popSounds;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("AudioSource component not found on the GameObject.");
        
        if (popSounds == null || popSounds.Length == 0)
            Debug.LogError("Pop sounds not assigned in the inspector.");


        audioSource.clip = popSounds[Random.Range(0, popSounds.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
    }
}
