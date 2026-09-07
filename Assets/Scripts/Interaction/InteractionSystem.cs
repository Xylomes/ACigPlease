using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactMask = ~0;
    [SerializeField] private LayerMask playerMask;

    public static InteractionSystem Instance;

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
    }

    private IInteractable GetInteractableFromHit(RaycastHit hit)
    {
        IInteractable interactable = null;
        hit.transform.TryGetComponent<IInteractable>(out interactable);
        if (interactable == null) interactable = hit.transform.GetComponentInParent<IInteractable>();
        if (interactable == null) interactable = hit.transform.GetComponentInChildren<IInteractable>();
        return interactable;
    }

    void Update()
    {
        if (!PlayerStateMachine.CanInteract)
        {
            return;
        }

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange, ~playerMask))
        {
            IInteractable interactable = GetInteractableFromHit(hit);

            if (interactable != null)
            {
                if (PlayerStateMachine.IsHoldingItem && !(interactable is BagRecepter))
                {
                    InteractionUI.Instance.HidePrompt();
                    return;
                }

                if (interactable.IsInteractable)
                {
                    InteractionUI.Instance.ShowPrompt(interactable.InteractionPrompt);
                }
                else
                {
                    InteractionUI.Instance.HidePrompt();
                }
            }
            else
            {
                InteractionUI.Instance.HidePrompt();
            }
        }
        else
        {
            InteractionUI.Instance.HidePrompt();
        }
    }

    public void TryInteract()
    {
        if (!PlayerStateMachine.CanInteract)
        {
            return;
        }

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange, ~playerMask))
        {
            IInteractable interactable = GetInteractableFromHit(hit);

            if (interactable != null)
            {
                if (PlayerStateMachine.IsHoldingItem && !(interactable is BagRecepter))
                {
                    return;
                }

                if (interactable.IsInteractable)
                {
                    interactable.Interact();
                }
            }
        }
    }
}
