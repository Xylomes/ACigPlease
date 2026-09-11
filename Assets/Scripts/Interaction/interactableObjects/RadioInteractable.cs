using UnityEngine;

public class RadioInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string onPrompt = "Eteindre";
    [SerializeField] private string offPrompt = "Allumer";

    private bool isOn;
    private bool isPaused;
    private bool hasEverBeenPlayed;
    public static RadioInteractable Instance { get; private set; }

    public string InteractionPrompt => isOn ? onPrompt : offPrompt;

    public bool IsInteractable
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.GameOver)
                return false;

            return true;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Interact()
    {
        if (!IsInteractable)
            return;

        if (isOn)
        {
            Pause();
            isOn = false;
        }
        else
        {
            isOn = true;
            isPaused = false;

            if (audioSource != null)
            {
                audioSource.loop = true;

                if (!hasEverBeenPlayed)
                {
                    audioSource.time = 0f;
                    audioSource.Play();
                    hasEverBeenPlayed = true;
                }
                else
                {
                    audioSource.UnPause();
                }
            }
        }
    }

    public void Pause()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            isPaused = true;
        }
    }

    public void Resume()
    {
        if (isPaused && isOn && audioSource != null)
        {
            audioSource.UnPause();
            isPaused = false;
        }
    }
}

