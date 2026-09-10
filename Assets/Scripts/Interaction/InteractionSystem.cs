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

    private IInteractable GetInteractableFromHit(RaycastHit pHit)
    {
        IInteractable lInteractable = null;
        pHit.transform.TryGetComponent<IInteractable>(out lInteractable);
        if (lInteractable == null) lInteractable = pHit.transform.GetComponentInParent<IInteractable>();
        if (lInteractable == null) lInteractable = pHit.transform.GetComponentInChildren<IInteractable>();
        return lInteractable;
    }

    void Update()
    {
        if (!PlayerStateMachine.CanInteract)
        {
            return;
        }

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit lHit, interactRange, ~playerMask))
        {
            IInteractable lInteractable = GetInteractableFromHit(lHit);

            if (lInteractable != null)
            {
                if (PlayerStateMachine.IsHoldingItem && !(lInteractable is BagRecepter) && !(lInteractable is HidingSpot lHs && lHs.IsOpen))
                {
                    InteractionUI.Instance.HidePrompt();
                    return;
                }

                if (lInteractable.IsInteractable)
                {
                    InteractionUI.Instance.ShowPrompt(lInteractable.InteractionPrompt);
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

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit lHit, interactRange, ~playerMask))
        {
            IInteractable lInteractable = GetInteractableFromHit(lHit);

            if (lInteractable != null)
            {
                if (PlayerStateMachine.IsHoldingItem && !(lInteractable is BagRecepter) && !(lInteractable is HidingSpot lHs && lHs.IsOpen))
                {
                    return;
                }

                if (lInteractable.IsInteractable)
                {
                    lInteractable.Interact();
                }
            }
        }
    }
}
