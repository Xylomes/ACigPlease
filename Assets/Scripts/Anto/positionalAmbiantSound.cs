using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class positionalAmbientSound : MonoBehaviour
{
    [SerializeField] private SoundManager.SoundType soundType;
    [SerializeField] private AudioMixerGroup mixerGroup;

    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;

        if (mixerGroup != null)
            audioSource.outputAudioMixerGroup = mixerGroup;
    }

    void Start()
    {
        AudioClip lClip = SoundManager.Instance.GetClip(soundType);
        if (lClip != null)
        {
            audioSource.clip = lClip;
            audioSource.Play();
        }
    }
}
