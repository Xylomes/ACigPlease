using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    public enum FlowState
    {
        MainMenu,
        Playing,
        Choice,
        Options,
        Rules,
        GameOver,
        WinVideo,
        Credits
    }

    [SerializeField] private EyeBlinkTransition eyeBlink;

    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private GameObject rulesCanvas;
    [SerializeField] private GameObject winVideoCanvas;
    [SerializeField] private GameObject creditsCanvas;
    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private GameObject choiceCanvas;

    [SerializeField] private float winVideoPlaceholderDuration = 3f;
    [SerializeField] private float creditsDuration = 6f;
    [SerializeField] private float gameOverDelay = 3f;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private GameManager gameManager;

    [SerializeField] private float defaultAudioVolume = 1f;
    [SerializeField] private float defaultMouseSensitivity = 1f;

    public FlowState CurrentState { get; private set; } = FlowState.MainMenu;

    private float audioVolume;
    private float mouseSensitivity;

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

        if (playerController != null)
        {
            playerStartPosition = playerController.transform.position;
            playerStartRotation = playerController.transform.rotation;
        }

        GameManager.OnTimerExpired += HandleGameOver;
        GameFlags.OnFlagSet += HandleFlagSet;

        if (InnerVoiceManager.Instance != null)
        {
            InnerVoiceManager.Instance.OnAllLinesTyped += HandleVoiceComplete;
        }

        SetState(FlowState.MainMenu);
    }

    private void OnDestroy()
    {
        GameManager.OnTimerExpired -= HandleGameOver;
        GameFlags.OnFlagSet -= HandleFlagSet;

        if (InnerVoiceManager.Instance != null)
        {
            InnerVoiceManager.Instance.OnAllLinesTyped -= HandleVoiceComplete;
        }
    }

    public void OnPlayClicked()
    {
        eyeBlink.Blink(() =>
        {
            SetState(FlowState.Playing);
            gameManager.StartGame();
        });
    }

    public void OnOptionsClicked()
    {
        eyeBlink.Blink(() =>
        {
            SetState(FlowState.Options);
        });
    }

    public void OnRulesClicked()
    {
        eyeBlink.Blink(() =>
        {
            SetState(FlowState.Rules);
        });
    }

    public void OnRulesBackClicked()
    {
        eyeBlink.Blink(() =>
        {
            SetState(FlowState.MainMenu);
        });
    }

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

    public void OnOptionsBackClicked()
    {
        eyeBlink.Blink(() =>
        {
            SetState(FlowState.MainMenu);
        });
    }

    public void OnAudioSliderChanged(float pValue)
    {
        audioVolume = pValue;
        AudioListener.volume = pValue;
    }

    public void OnSensitivitySliderChanged(float pValue)
    {
        mouseSensitivity = pValue;
        if (playerRotation != null)
            playerRotation.SensitivityMultiplier = pValue;
    }

    private void SetState(FlowState pNewState)
    {
        CurrentState = pNewState;

        bool lPlayerCanMove = pNewState == FlowState.Playing;

        if (playerController != null)
            playerController.enabled = lPlayerCanMove;
        if (playerRotation != null)
            playerRotation.enabled = lPlayerCanMove;
        if (playerInput != null)
            playerInput.enabled = lPlayerCanMove;

        if (lPlayerCanMove)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        SetCanvasActive(mainMenuCanvas, pNewState == FlowState.MainMenu);
        SetCanvasActive(optionsCanvas, pNewState == FlowState.Options);
        SetCanvasActive(rulesCanvas, pNewState == FlowState.Rules);
        SetCanvasActive(winVideoCanvas, pNewState == FlowState.WinVideo);
        SetCanvasActive(creditsCanvas, pNewState == FlowState.Credits);
        SetCanvasActive(hudCanvas, pNewState == FlowState.Playing || pNewState == FlowState.Choice);
        SetCanvasActive(choiceCanvas, pNewState == FlowState.Choice);

        if (pNewState != FlowState.Playing)
        {
            HeadMouvementEffect lHeadEffect = playerController != null
                ? playerController.GetComponentInChildren<HeadMouvementEffect>()
                : null;
            if (lHeadEffect != null)
            {
                lHeadEffect.ResetEffect();
            }
        }
    }

    private static void SetCanvasActive(GameObject pCanvas, bool pActive)
    {
        if (pCanvas != null)
            pCanvas.SetActive(pActive);
    }

    private void HandleGameOver()
    {
        StartCoroutine(GameOverAfterDelay());
    }

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

    private void HandleFlagSet(string pFlag)
    {
        if (pFlag == GameFlags.CHOICE_PROMPT)
        {
            SetState(FlowState.Choice);
        }
    }

    private IEnumerator WinVideoThenCredits()
    {
        VideoPlayer lPlayer = winVideoCanvas != null
            ? winVideoCanvas.GetComponentInChildren<VideoPlayer>()
            : null;

        if (lPlayer != null && lPlayer.clip != null)
        {
            lPlayer.time = 0;
            lPlayer.Play();

            while (!lPlayer.isPlaying)
                yield return null;

            bool lFinished = false;
            VideoPlayer lCaptured = lPlayer;
            lPlayer.loopPointReached += _ => lFinished = true;

            while (!lFinished)
                yield return null;

            lCaptured.Stop();
        }
        else
        {
            yield return new WaitForSeconds(winVideoPlaceholderDuration);
        }

        eyeBlink.Blink(() =>
        {
            SetState(FlowState.Credits);
            StartCoroutine(CreditsThenMainMenu());
        });
    }

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

    private bool voiceComplete;

    private void HandleVoiceComplete()
    {
        voiceComplete = true;
    }

    public void OnSmokeClicked()
    {
        SetCanvasActive(choiceCanvas, false);
        gameManager.EndGame();

        voiceComplete = false;
        if (InnerVoiceManager.Instance != null && gameManager.SmokeVoice != null)
        {
            InnerVoiceManager.Instance.Show(gameManager.SmokeVoice);
            StartCoroutine(SmokeEndingRoutine());
        }
    }

    public void OnThrowAwayClicked()
    {
        SetCanvasActive(choiceCanvas, false);

        PlayerController lPlayerCtrl = PlayerController.Instance;
        if (lPlayerCtrl != null)
        {
            PickupSystem lPickup = lPlayerCtrl.GetComponent<PickupSystem>();
            if (lPickup != null)
            {
                lPickup.DropAllItemsToFloor();
            }
        }

        gameManager.EndGame();

        voiceComplete = false;
        if (InnerVoiceManager.Instance != null && gameManager.ThrowAwayVoice != null)
        {
            InnerVoiceManager.Instance.Show(gameManager.ThrowAwayVoice);
            StartCoroutine(ThrowAwayEndingRoutine());
        }
    }

    private IEnumerator SmokeEndingRoutine()
    {
        yield return new WaitUntil(() => voiceComplete);
        yield return new WaitForSeconds(2f);

        InnerVoiceManager.Instance?.Hide();

        eyeBlink.Blink(() =>
        {
            SetState(FlowState.WinVideo);
            StartCoroutine(WinVideoThenCredits());
        });
    }

    private IEnumerator ThrowAwayEndingRoutine()
    {
        yield return new WaitUntil(() => voiceComplete);
        yield return new WaitForSeconds(2f);

        InnerVoiceManager.Instance?.Hide();

        eyeBlink.Blink(() =>
        {
            SetState(FlowState.Credits);
            StartCoroutine(CreditsThenMainMenu());
        });
    }

    private void ResetPlayerToStart()
    {
        if (playerController != null)
        {
            CharacterController lCc = playerController.GetComponent<CharacterController>();
            if (lCc != null) lCc.enabled = false;
            playerController.transform.position = playerStartPosition;
            playerController.transform.rotation = playerStartRotation;
            if (lCc != null) lCc.enabled = true;
        }

        if (playerRotation != null)
        {
            playerRotation.ResetCameraAngle();
        }
    }
}
