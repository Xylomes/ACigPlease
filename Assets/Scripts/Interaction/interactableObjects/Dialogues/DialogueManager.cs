using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private DialogueAnimator dialogueAnimator;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private string completionFlag;

    public bool isInDialogue { get; private set; }
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(DialogueData pDialogueData, string pCompletionFlag)
    {
        currentDialogue = pDialogueData;
        currentLineIndex = 0;
        isInDialogue = true;
        completionFlag = pCompletionFlag;

        string lSpeakerName = currentDialogue.Lines[0].SpeakerName;
        string lDialogueText = currentDialogue.Lines[0].Text;
        dialogueAnimator.ShowLine(lSpeakerName, lDialogueText);

        PlayerController.Instance.stateMachine.EnterListening();
        InteractionUI.Instance.HidePrompt();
    }
    public void ContinueDialogue()
    {
        
        if (dialogueAnimator.IsTyping)
        {
            dialogueAnimator.CompleteTyping();
            return;
        }
        else
        {
            currentLineIndex++;
            if (currentLineIndex < currentDialogue.Lines.Length)
            {
                string lSpeakerName = currentDialogue.Lines[currentLineIndex].SpeakerName;
                string lDialogueText = currentDialogue.Lines[currentLineIndex].Text;
                dialogueAnimator.ShowLine(lSpeakerName, lDialogueText);
            }
            else
            {
                dialogueAnimator.HideLine();
                PlayerController.Instance.stateMachine.ExitInteracting();
                if (!string.IsNullOrEmpty(completionFlag))
                {
                    GameFlags.SetFlag(completionFlag);
                }
                isInDialogue = false;
            }
        }
    }

}
