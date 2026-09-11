using UnityEngine;

public class BreathingSoundScript : MonoBehaviour
{
    [SerializeField] private float triggerTime = 8f;

    private bool hasTriggeredPhase1;
    private bool hasTriggeredPhase2;
    private AudioSource breathingSource;

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
            hasTriggeredPhase1 = false;
            hasTriggeredPhase2 = false;
            StopBreathing();
        }

        if (newPhase == GamePhase.GameOver)
        {
            StopBreathing();
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsTimerRunning)
            return;

        GamePhase currentPhase = GameManager.Instance.CurrentPhase;
        float timeRemaining = GameManager.Instance.TimeRemaining;

        if (currentPhase == GamePhase.SearchingCigarettes && !hasTriggeredPhase1 && timeRemaining <= triggerTime)
        {
            hasTriggeredPhase1 = true;
            StartBreathing(SoundManager.SoundType.GameMusic);
        }
        else if (currentPhase == GamePhase.SearchingLighter && !hasTriggeredPhase2 && timeRemaining <= triggerTime)
        {
            hasTriggeredPhase2 = true;
            StartBreathing(SoundManager.SoundType.MenuMusic);
        }
    }

    private void StartBreathing(SoundManager.SoundType soundType)
    {
        StopBreathing();

        if (SoundManager.Instance != null)
        {
            breathingSource = SoundManager.Instance.PlayMusic(soundType, true);
        }
    }

    private void StopBreathing()
    {
        if (breathingSource != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopMusic(breathingSource);
            breathingSource = null;
        }
    }
}