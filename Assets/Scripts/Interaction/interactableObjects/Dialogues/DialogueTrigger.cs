using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt;
    [SerializeField] private List<ConditionalDialogue> dialogues;
    public string InteractionPrompt => interactionPrompt;
    public bool IsInteractable => GetAvailableDialogue() != null;

    public ConditionalDialogue GetAvailableDialogue()
    {
        foreach (var lDialogue in dialogues)
        {
            if (lDialogue.condition.IsConditionMet() == true)
            {
                if (!GameFlags.IsSetFlag(lDialogue.completionFlag))
                {
                    return lDialogue;
                }
                else continue;
            }
            else
            {
                continue; 
            }
        }
        return null;
    }

    public void Interact()
    {
        ConditionalDialogue lAvailableDialogue = GetAvailableDialogue();

        if(lAvailableDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(lAvailableDialogue.data, lAvailableDialogue.completionFlag);
        }
    }

}