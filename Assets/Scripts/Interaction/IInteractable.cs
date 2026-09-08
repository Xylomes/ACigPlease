using UnityEngine;

public interface IInteractable
{
    string InteractionPrompt { get; }
    bool IsInteractable { get; }
    public void Interact();
}
