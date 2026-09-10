using UnityEngine;

public class MotorcycleSoundScript : MonoBehaviour
{
    [SerializeField] private AudioClip motorcycleClip;
    [SerializeField] private float triggerTime = 40f;

    private AudioSource audioSource;
    private bool hasTriggered;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        GameManager.OnPhaseChanged += HandlePhaseChanged;
    }

    void OnDisable()
    {
        GameManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.Setup)
        {
            hasTriggered = false;
        }
    }

    void Update()
    {
        if (hasTriggered || GameManager.Instance == null || !GameManager.Instance.IsTimerRunning)
            return;

        if (GameManager.Instance.TimeRemaining <= triggerTime)
        {
            hasTriggered = true;
            PlaySound();
        }
    }

    private void PlaySound()
    {
        if (motorcycleClip != null)
        {
            audioSource.PlayOneShot(motorcycleClip);
        }
    }
}
