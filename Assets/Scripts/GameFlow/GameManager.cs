using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private float timeRemaining = 30f;
    [SerializeField] private float timeBonusOnFind = 30f;

    [Header("Inner Voice")]
    [SerializeField] private InnerVoiceData gameStartVoice;
    [SerializeField] private InnerVoiceData lighterPhaseVoice;
    [SerializeField] private InnerVoiceData gameOverVoice;

    private const float TIMER_START_VALUE = 30f;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Setup;
    public float TimeRemaining => timeRemaining;
    public bool IsTimerRunning { get; private set; }

    /// <summary>Fired when the game phase changes. Passes old and new phase.</summary>
    public static event Action<GamePhase, GamePhase> OnPhaseChanged;

    /// <summary>Fired every frame the timer is running. Passes remaining seconds.</summary>
    public static event Action<float> OnTimerTick;

    /// <summary>Fired when the timer reaches zero.</summary>
    public static event Action OnTimerExpired;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        GameFlags.OnFlagSet += HandleFlagSet;
    }

    private void OnDisable()
    {
        GameFlags.OnFlagSet -= HandleFlagSet;
    }

    private void Start()
    {
        // Game is started by GameFlowController when the player clicks Play.
        // Do not auto-start here.
    }

    /// <summary>Reset all flags and start a new game session.</summary>
    public void StartGame()
    {
        GameFlags.ResetAllFlags();
        timeRemaining = TIMER_START_VALUE;
        ChangePhase(GamePhase.Setup);

        HidingSpotManager spotManager = HidingSpotManager.Instance;
        if (spotManager != null)
        {
            spotManager.SetupCigarettePhase();
        }

        GameFlags.SetFlag(GameFlags.GAME_SETUP_DONE);
        ChangePhase(GamePhase.SearchingCigarettes);
        IsTimerRunning = true;

        // Show inner voice at game start
        if (InnerVoiceManager.Instance != null && gameStartVoice != null)
        {
            InnerVoiceManager.Instance.Show(gameStartVoice);
        }
    }

    /// <summary>
    /// Fully reset the game state without starting a new session.
    /// Closes all hiding spots, resets flags, timer, penalties, and player modifiers.
    /// Called when returning to the main menu after a game over or win.
    /// </summary>
    public void ResetGame()
    {
        IsTimerRunning = false;
        timeRemaining = TIMER_START_VALUE;
        ChangePhase(GamePhase.Setup);

        // Reset all flags
        GameFlags.ResetAllFlags();

        // Close all hiding spots and destroy spawned items
        if (HidingSpotManager.Instance != null)
        {
            HidingSpotManager.Instance.CloseAllSpots();
        }

        // Clear any active penalty
        if (PenaltyManager.Instance != null)
        {
            PenaltyManager.Instance.ClearCurrentPenalty();
        }

        // Reset player input modifiers
        PlayerController.IsMovementInverted = false;
        PlayerRotation.IsInputInverted = false;

        // Reset timer UI
        OnTimerTick?.Invoke(TIMER_START_VALUE);
    }

    private void Update()
    {
        if (!IsTimerRunning)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            IsTimerRunning = false;
            OnTimerExpired?.Invoke();
            HandleTimerExpired();
        }

        OnTimerTick?.Invoke(timeRemaining);
    }

    private void HandleFlagSet(string flag)
    {
        switch (flag)
        {
            case GameFlags.CIGARETTES_FOUND:
                AddTime(timeBonusOnFind);
                TransitionToLighterPhase();
                break;

            case GameFlags.WORKING_LIGHTER_FOUND:
                AddTime(timeBonusOnFind);
                HandleGameWon();
                break;
        }
    }

    /// <summary>Transition from cigarette search to lighter search.</summary>
    private void TransitionToLighterPhase()
    {
        HidingSpotManager spotManager = HidingSpotManager.Instance;
        if (spotManager != null)
        {
            spotManager.SetupLighterPhase();
        }

        ChangePhase(GamePhase.SearchingLighter);

        // Show inner voice when transitioning to lighter phase
        if (InnerVoiceManager.Instance != null && lighterPhaseVoice != null)
        {
            InnerVoiceManager.Instance.Show(lighterPhaseVoice);
        }
    }

    private void HandleGameWon()
    {
        IsTimerRunning = false;
        GameFlags.SetFlag(GameFlags.GAME_WON);
        GameFlags.SetFlag(GameFlags.GAME_OVER);
        ChangePhase(GamePhase.GameOver);
    }

    private void HandleTimerExpired()
    {
        GameFlags.SetFlag(GameFlags.GAME_OVER);
        ChangePhase(GamePhase.GameOver);

        if (InnerVoiceManager.Instance != null && gameOverVoice != null)
        {
            InnerVoiceManager.Instance.Show(gameOverVoice);
        }
    }

    /// <summary>Add bonus time to the timer (clamped to a max of 30s).</summary>
    public void AddTime(float seconds)
    {
        timeRemaining = Mathf.Min(timeRemaining + seconds, TIMER_START_VALUE);
    }

    private void ChangePhase(GamePhase newPhase)
    {
        GamePhase oldPhase = CurrentPhase;
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(oldPhase, newPhase);
    }
}
