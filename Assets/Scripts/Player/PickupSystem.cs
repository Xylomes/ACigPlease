using UnityEngine;
using UnityEngine.InputSystem;

public class PickupSystem : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform holdPointRight;
    [SerializeField] private Transform holdPointLeft;

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

    private void OnGrabPerformed(InputAction.CallbackContext pContext)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.GameOver)
            return;

        TryGrab();
    }

    private void TryGrab()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit lHit, interactRange, ~playerMask))
        {
            TargetItem lTargetItem = lHit.collider.GetComponent<TargetItem>();
            if (lTargetItem == null)
                return;

            if (lTargetItem.UseLeftHand)
            {
                if (heldObjectLeft != null)
                    return;

                lTargetItem.OnGrabbed(holdPointLeft);
                heldObjectLeft = lHit.collider.gameObject;
            }
            else
            {
                if (heldObjectRight != null)
                    return;

                lTargetItem.OnGrabbed(holdPointRight);
                heldObjectRight = lHit.collider.gameObject;
            }
        }
    }

    public void ReleaseHeldItemRight()
    {
        heldObjectRight = null;
    }
    public void RealeaseHeldLeft()
    {
        heldObjectLeft = null;
    }

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

    private void DropItemToFloor(GameObject pHeldObj, Transform pHoldPoint)
    {
        if (pHeldObj == null || pHoldPoint == null) return;

        pHeldObj.transform.SetParent(null);

        Vector3 lDropPos = pHoldPoint.position + pHoldPoint.forward * 0.5f;
        lDropPos.y = Mathf.Max(lDropPos.y, 0.5f);
        pHeldObj.transform.position = lDropPos;
        pHeldObj.transform.rotation = Random.rotation;

        Rigidbody lRb = pHeldObj.GetComponent<Rigidbody>();
        if (lRb == null)
            lRb = pHeldObj.AddComponent<Rigidbody>();
        lRb.isKinematic = false;
        lRb.useGravity = true;
        lRb.AddForce(pHoldPoint.forward * 2f, ForceMode.Impulse);

        Collider lCol = pHeldObj.GetComponent<Collider>();
        if (lCol is MeshCollider lMeshCol && !lMeshCol.convex)
            lMeshCol.convex = true;
        if (lCol != null)
            lCol.enabled = true;
    }

    public void SetGrabInfos(bool pPressed) { }

    private void OnDestroy()
    {
        grabAction.performed -= OnGrabPerformed;
    }
}
