using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the overall game flow: main menu, playing, options, game over, and credits.
/// Uses the EyeBlinkTransition for smooth state changes between phases.
/// The player keeps an FPS camera view at all times; only input and UI change.
/// </summary>
public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    public enum FlowState
    {
        MainMenu,
        Playing,
        Options,
        GameOver,
        WinVideo,
        Credits
    }

    [Header("Blink Transition")]
    [SerializeField] private EyeBlinkTransition eyeBlink;

    [Header("UI Canvases")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private GameObject winVideoCanvas;
    [SerializeField] private GameObject creditsCanvas;
    [SerializeField] private GameObject hudCanvas;

    [Header("Win Video Placeholder")]
    [Tooltip("Duration of the win video placeholder before transitioning to credits.")]
    [SerializeField] private float winVideoPlaceholderDuration = 3f;

    [Header("Credits")]
    [Tooltip("Duration the credits stay on screen before returning to menu.")]
    [SerializeField] private float creditsDuration = 6f;

    [Header("Game Over")]
    [Tooltip("Delay in seconds after a game over before the blink transition to the menu.")]
    [SerializeField] private float gameOverDelay = 3f;

    [Header("Player References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerInput playerInput;

    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;

    [Header("Settings Defaults")]
    [SerializeField] private float defaultAudioVolume = 1f;
    [SerializeField] private float defaultMouseSensitivity = 1f;

    /// <summary>Current flow state.</summary>
    public FlowState CurrentState { get; private set; } = FlowState.MainMenu;

    // Stored settings
    private float audioVolume;
    private float mouseSensitivity;

    // Player start transform (to reset on game over)
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        audioVolume = defaultAudioVolume;
        mouseSensitivity = defaultMouseSensitivity;
        AudioListener.volume = audioVolume;

        // Record the player's starting transform for reset on game over
        if (playerController != null)
        {
            playerStartPosition = playerController.transform.position;
            playerStartRotation = playerController.transform.rotation;
        }

        // Subscribe to game manager events
        GameManager.OnTimerExpired += HandleGameOver;
        GameFlags.OnFlagSet += HandleFlagSet;

        SetState(FlowState.MainMenu);
    }

    private void OnDestroy()
    {
        GameManager.OnTimerExpired -= HandleGameOver;
        GameFlags.OnFlagSet -= HandleFlagSet;
    }

    // ---- Button handlers (called from UI) ----

    /// <summary>Called when the Play button is clicked.</summary>
    public void OnPlayClicked()
    {
        eyeBlink.Blink(() =>
        {
            SetState(FlowState.Playing);
            gameManager.StartGame();
        });
    }

    /// <summary>Called when the Options button is clicked.</summary>
    public void OnOptionsClicked()
    {
        eyeBlink.Blink(() =>
        {
            SetState(FlowState.Options);
        });
    }

    /// <summary>Called when the Quit button is clicked.</summary>
    public void OnQuitClicked()
    {
        eyeBlink.Blink(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    /// <summary>Called when the Back button in Options is clicked.</summary>
    public void OnOptionsBackClicked()
    {
        eyeBlink.Blink(() =>
        {
            SetState(FlowState.MainMenu);
        });
    }

    /// <summary>Called when the audio volume slider changes.</summary>
    public void OnAudioSliderChanged(float value)
    {
        audioVolume = value;
        AudioListener.volume = value;
    }

    /// <summary>Called when the mouse sensitivity slider changes.</summary>
    public void OnSensitivitySliderChanged(float value)
    {
        mouseSensitivity = value;
        if (playerRotation != null)
            playerRotation.SensitivityMultiplier = value;
    }

    // ---- State management ----

    private void SetState(FlowState newState)
    {
        CurrentState = newState;

        bool playerCanMove = newState == FlowState.Playing;
        bool uiInteractable = !playerCanMove;

        // Enable/disable player controls
        if (playerController != null)
            playerController.enabled = playerCanMove;
        if (playerRotation != null)
            playerRotation.enabled = playerCanMove;
        if (playerInput != null)
            playerInput.enabled = playerCanMove;

        // Cursor
        if (playerCanMove)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Toggle UI canvases
        SetCanvasActive(mainMenuCanvas, newState == FlowState.MainMenu);
        SetCanvasActive(optionsCanvas, newState == FlowState.Options);
        SetCanvasActive(winVideoCanvas, newState == FlowState.WinVideo);
        SetCanvasActive(creditsCanvas, newState == FlowState.Credits);
        SetCanvasActive(hudCanvas, newState == FlowState.Playing);

        // Reset head bob effect when leaving gameplay so the menu camera is still
        if (newState != FlowState.Playing)
        {
            HeadMouvementEffect headEffect = playerController != null
                ? playerController.GetComponentInChildren<HeadMouvementEffect>()
                : null;
            if (headEffect != null)
            {
                headEffect.ResetEffect();
            }
        }
    }

    private static void SetCanvasActive(GameObject canvas, bool active)
    {
        if (canvas != null)
            canvas.SetActive(active);
    }

    // ---- Game over / win handling ----

    private void HandleGameOver()
    {
        StartCoroutine(GameOverAfterDelay());
    }

    /// <summary>Wait gameOverDelay seconds, then blink and return to the main menu.</summary>
    private IEnumerator GameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);

        eyeBlink.Blink(() =>
        {
            ResetPlayerToStart();
            gameManager.ResetGame();
            SetState(FlowState.MainMenu);
        });
    }

    private void HandleFlagSet(string flag)
    {
        if (flag == GameFlags.GAME_WON)
        {
            eyeBlink.Blink(() =>
            {
                SetState(FlowState.WinVideo);
                StartCoroutine(WinVideoThenCredits());
            });
        }
    }

    /// <summary>After the win video placeholder plays, transition to credits.</summary>
    private IEnumerator WinVideoThenCredits()
    {
        // Placeholder: wait for the configured duration.
        // When the real video is ready, replace this with VideoPlayer playback logic.
        yield return new WaitForSeconds(winVideoPlaceholderDuration);

        eyeBlink.Blink(() =>
        {
            SetState(FlowState.Credits);
            StartCoroutine(CreditsThenMainMenu());
        });
    }

    /// <summary>After credits finish, return to main menu and reset the game.</summary>
    private IEnumerator CreditsThenMainMenu()
    {
        yield return new WaitForSeconds(creditsDuration);

        eyeBlink.Blink(() =>
        {
            ResetPlayerToStart();
            gameManager.ResetGame();
            SetState(FlowState.MainMenu);
        });
    }

    /// <summary>Teleports the player back to the starting position, rotation, and camera angle.</summary>
    private void ResetPlayerToStart()
    {
        if (playerController != null)
        {
            CharacterController cc = playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerController.transform.position = playerStartPosition;
            playerController.transform.rotation = playerStartRotation;
            if (cc != null) cc.enabled = true;
        }

        // Reset camera local rotation to neutral (looking straight ahead)
        if (playerRotation != null)
        {
            playerRotation.ResetCameraAngle();
        }
    }
}
