using UnityEngine;

public class RadioInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string onPrompt = "Eteindre";
    [SerializeField] private string offPrompt = "Allumer";

    private bool isOn;

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

    public void Interact()
    {
        if (!IsInteractable)
            return;

        isOn = !isOn;

        if (isOn)
        {
            if (audioSource != null)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }
}
