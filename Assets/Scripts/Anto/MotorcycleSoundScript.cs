using UnityEngine;

public class MotorcycleSoundScript : MonoBehaviour
{
    [SerializeField] private AudioClip motorcycleClip;
    [SerializeField] private float[] triggerTimes = new float[] { 40f };

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private GamePhase targetPhase = GamePhase.SearchingCigarettes;

    private bool[] hasTriggered;

    void Awake()
    {
        audioSource.playOnAwake = false;
        hasTriggered = new bool[triggerTimes.Length];
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
            for (int i = 0; i < hasTriggered.Length; i++)
            {
                hasTriggered[i] = false;
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsTimerRunning)
            return;

        if (GameManager.Instance.CurrentPhase != targetPhase)
            return;

        float lTimeRemaining = GameManager.Instance.TimeRemaining;

        for (int i = 0; i < triggerTimes.Length; i++)
        {
            if (!hasTriggered[i] && lTimeRemaining <= triggerTimes[i])
            {
                hasTriggered[i] = true;
                PlaySound();
            }
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