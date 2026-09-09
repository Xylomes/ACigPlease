using UnityEngine;
using UnityEngine.InputSystem;

public class PickupSystem : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform holdPointRight;
    [SerializeField] private Transform holdPointLeft;

    [Header("Paramètres")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask playerMask;

    private GameObject heldObjectRight;
    private GameObject heldObjectLeft;
    private PlayerInput playerInput;
    private InputAction grabAction;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        grabAction = playerInput.actions["Grab"];
        grabAction.performed += OnGrabPerformed;
    }

    private void OnGrabPerformed(InputAction.CallbackContext context)
    {
        //if (heldObjectRight != null)
        //    return;

        // Empêche le grab si la partie est terminée
        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.GameOver)
            return;

        TryGrab();
    }

    private void TryGrab()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange, ~playerMask))
        {
            TargetItem targetItem = hit.collider.GetComponent<TargetItem>();
            if (targetItem == null)
                return;

            if (targetItem.UseLeftHand)
            {
                if (heldObjectLeft != null)
                    return; // main gauche déjà occupée

                targetItem.OnGrabbed(holdPointLeft);
                heldObjectLeft = hit.collider.gameObject;
            }
            else
            {
                if (heldObjectRight != null)
                    return; // main droite déjà occupée

                targetItem.OnGrabbed(holdPointRight);
                heldObjectRight = hit.collider.gameObject;
            }
        }
    }

    /// <summary>Release the currently held object so the player can grab another item.</summary>
    public void ReleaseHeldItemRight()
    {
        heldObjectRight = null;
    }
    public void RealeaseHeldLeft()
    {
        heldObjectLeft = null;
    }

    /// <summary>Destroy the currently held object and clear all grab state. Used during game reset.</summary>
    public void ClearHeldItem()
    {
        if (heldObjectRight != null)
        {
            Destroy(heldObjectRight);
            heldObjectRight = null;
        }

        //if (heldObjectLeft != null)
        //{
        //    Destroy(heldObjectLeft);
        //    heldObjectLeft = null;
        //}
    }

    public void SetGrabInfos(bool pressed) { }

    private void OnDestroy()
    {
        grabAction.performed -= OnGrabPerformed;
    }
}
