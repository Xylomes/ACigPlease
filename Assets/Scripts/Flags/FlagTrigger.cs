using UnityEngine;
using UnityEngine.Events;

public class FlagTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private FlagTriggerType triggerType;
    [SerializeField] private string flagToSet;
    [SerializeField] private string interactionPrompt;
    [SerializeField] private UnityEvent onFlagTriggered;
    [SerializeField] private bool consumeOnTrigger;

    public string InteractionPrompt { get { return interactionPrompt; } }
    public bool IsInteractable
    {
        get
        {
            if (triggerType != FlagTriggerType.OnInteract)
            {
                return false;
            }
            if (GameFlags.IsSetFlag(flagToSet))
            {
                return false;
            }
            return true;
        }
    }
    public bool Trigger()
    {
        if (GameFlags.IsSetFlag(flagToSet))
        {
            return true;
        }
        else
        {
            GameFlags.SetFlag(flagToSet);
            onFlagTriggered?.Invoke();
            if (consumeOnTrigger)
            {
                gameObject.SetActive(false);
            }
            return true;
        }
        
    }
    public void Interact()
    {
        if (triggerType == FlagTriggerType.OnInteract)
        {
            Trigger();
        }
    }

    public void OnTriggerEnter(Collider pOther)
    {
        if (triggerType == FlagTriggerType.OnTriggerEnter)
        {
            if(pOther.TryGetComponent<PlayerController>(out PlayerController lPlayer))
            {
                Trigger();
            }
        }
    }

}
