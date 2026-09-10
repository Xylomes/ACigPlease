using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private float timeRemaining;
    [SerializeField] private float timeBonusOnFind;

    [Header("Inner Voice")]
    [SerializeField] private InnerVoiceData gameStartVoice;
    [SerializeField] private InnerVoiceData lighterPhaseVoice;
    [SerializeField] private InnerVoiceData gameOverVoice;

    private const float TIMER_START_VALUE = 60f;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Setup;
    public float TimeRemaining => timeRemaining;
    public bool IsTimerRunning { get; private set; }
    public static event Action<GamePhase, GamePhase> OnPhaseChanged;
    public static event Action<float> OnTimerTick;
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

        if (InnerVoiceManager.Instance != null && gameStartVoice != null)
        {
            InnerVoiceManager.Instance.Show(gameStartVoice);
        }
    }

    public void ResetGame()
    {
        IsTimerRunning = false;
        timeRemaining = TIMER_START_VALUE;
        ChangePhase(GamePhase.Setup);

        GameFlags.ResetAllFlags();

        if (HidingSpotManager.Instance != null)
        {
            HidingSpotManager.Instance.ResetAllSpots();
        }

        if (PenaltyManager.Instance != null)
        {
            PenaltyManager.Instance.ClearCurrentPenalty();
        }

        PlayerController playerCtrl = PlayerController.Instance;
        if (playerCtrl != null)
        {
            PickupSystem pickup = playerCtrl.GetComponent<PickupSystem>();
            if (pickup != null)
            {
                pickup.ClearHeldItem();
            }
        }

        PlayerController.IsMovementInverted = false;
        PlayerRotation.IsInputInverted = false;

        PlayerStateMachine.CurrentState = PlayerStateMachine.PlayerState.Idle;
        PlayerStateMachine.CanInteract = false;
        PlayerStateMachine.IsHoldingItem = false;

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
    private void TransitionToLighterPhase()
    {
        HidingSpotManager spotManager = HidingSpotManager.Instance;
        if (spotManager != null)
        {
            spotManager.SetupLighterPhase();
        }

        ChangePhase(GamePhase.SearchingLighter);

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
