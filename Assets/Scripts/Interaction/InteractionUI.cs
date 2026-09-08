using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject interactParent;
    [SerializeField] private TextMeshProUGUI interactionTMP;

    public static InteractionUI Instance;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        interactionTMP.text = null;
        interactParent.gameObject.SetActive(false);
    }

    public void ShowPrompt(string prompt)
    {
        if(!DialogueManager.Instance.isInDialogue)
        {
            interactionTMP.text = prompt;
            interactParent.gameObject.SetActive(true);
        }
        else
        {
            HidePrompt();
        }
    }

    public void HidePrompt()
    {
        interactionTMP.text = null;
        interactParent.gameObject.SetActive(false);
    }
}
