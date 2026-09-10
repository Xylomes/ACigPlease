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

        if (heldObjectLeft != null)
        {
            Destroy(heldObjectLeft);
            heldObjectLeft = null;
        }
    }

    /// <summary>Drop both held items to the floor with physics, like defective lighters. Used for the "throw away" choice.</summary>
    public void DropAllItemsToFloor()
    {
        if (heldObjectRight != null)
        {
            DropItemToFloor(heldObjectRight, holdPointRight);
            heldObjectRight = null;
        }

        if (heldObjectLeft != null)
        {
            DropItemToFloor(heldObjectLeft, holdPointLeft);
            heldObjectLeft = null;
        }
    }

    private void DropItemToFloor(GameObject heldObj, Transform holdPoint)
    {
        if (heldObj == null || holdPoint == null) return;

        heldObj.transform.SetParent(null);

        Vector3 dropPos = holdPoint.position + holdPoint.forward * 0.5f;
        dropPos.y = Mathf.Max(dropPos.y, 0.5f);
        heldObj.transform.position = dropPos;
        heldObj.transform.rotation = Random.rotation;

        Rigidbody rb = heldObj.GetComponent<Rigidbody>();
        if (rb == null)
            rb = heldObj.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(holdPoint.forward * 2f, ForceMode.Impulse);

        Collider col = heldObj.GetComponent<Collider>();
        if (col is MeshCollider meshCol && !meshCol.convex)
            meshCol.convex = true;
        if (col != null)
            col.enabled = true;
    }

    public void SetGrabInfos(bool pressed) { }

    private void OnDestroy()
    {
        grabAction.performed -= OnGrabPerformed;
    }
}
