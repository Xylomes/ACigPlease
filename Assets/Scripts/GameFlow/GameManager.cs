using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float timeRemaining;
    [SerializeField] private float timeBonusOnFind;

    [SerializeField] private InnerVoiceData gameStartVoice;
    [SerializeField] private InnerVoiceData lighterPhaseVoice;
    [SerializeField] private InnerVoiceData gameOverVoice;
    [SerializeField] private InnerVoiceData smokeVoice;
    [SerializeField] private InnerVoiceData throwAwayVoice;

    public InnerVoiceData SmokeVoice => smokeVoice;
    public InnerVoiceData ThrowAwayVoice => throwAwayVoice;

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

        HidingSpotManager lSpotManager = HidingSpotManager.Instance;
        if (lSpotManager != null)
        {
            lSpotManager.SetupCigarettePhase();
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

        PlayerController lPlayerCtrl = PlayerController.Instance;
        if (lPlayerCtrl != null)
        {
            PickupSystem lPickup = lPlayerCtrl.GetComponent<PickupSystem>();
            if (lPickup != null)
            {
                lPickup.ClearHeldItem();
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

    private void HandleFlagSet(string pFlag)
    {
        switch (pFlag)
        {
            case GameFlags.CIGARETTES_FOUND:
                AddTime(timeBonusOnFind);
                TransitionToLighterPhase();
                break;

            case GameFlags.WORKING_LIGHTER_FOUND:
                AddTime(timeBonusOnFind);
                HandleChoicePrompt();
                break;
        }
    }
    private void TransitionToLighterPhase()
    {
        HidingSpotManager lSpotManager = HidingSpotManager.Instance;
        if (lSpotManager != null)
        {
            lSpotManager.SetupLighterPhase();
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

    private void HandleChoicePrompt()
    {
        IsTimerRunning = false;
        ChangePhase(GamePhase.Choice);
        GameFlags.SetFlag(GameFlags.CHOICE_PROMPT);
    }

    public void EndGame()
    {
        IsTimerRunning = false;
        ChangePhase(GamePhase.GameOver);
        GameFlags.SetFlag(GameFlags.GAME_OVER);
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

    public void AddTime(float pSeconds)
    {
        timeRemaining = Mathf.Min(timeRemaining + pSeconds, TIMER_START_VALUE);
    }

    private void ChangePhase(GamePhase pNewPhase)
    {
        GamePhase lOldPhase = CurrentPhase;
        CurrentPhase = pNewPhase;
        OnPhaseChanged?.Invoke(lOldPhase, pNewPhase);
    }
}
