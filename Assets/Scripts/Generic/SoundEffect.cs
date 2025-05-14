using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    public AudioClip[] effects;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("AudioSource component not found on the GameObject.");
        
        if (effects == null || effects.Length == 0)
            Debug.LogError("Pop sounds not assigned in the inspector.");


        audioSource.clip = effects[Random.Range(0, effects.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.Play();

        Destroy(gameObject, audioSource.clip.length);
    }
}
