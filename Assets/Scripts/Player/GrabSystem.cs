using UnityEngine;

public class GrabSystem : MonoBehaviour
{
    public static GrabSystem Instance;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private GameObject handPosition;

    private const float GRAB_RANGE = 3f;
    private const string BAG_TAG = "Bag";
    private const string HELD_LAYER_NAME = "Ignore Raycast";
    private const int DEFAULT_LAYER = 0;

    private bool isTryingToGrab = false;
    private bool isGrabing = false;
    private Transform heldBag;
    private Rigidbody heldBagRigidbody;
    private Bag heldBagComponent;

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

    public void SetGrabInfos(bool grabPressed)
    {
        isTryingToGrab = grabPressed;
    }

    void Update()
    {
        if (isGrabing && heldBag == null)
        {
            ResetGrabState();
            return;
        }

        if (isTryingToGrab)
        {
            if (heldBag == null || !isGrabing)
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, GRAB_RANGE, ~playerMask))
                {
                    if (hit.transform.CompareTag(BAG_TAG))
                    {
                        Grab(hit.transform);
                    }
                }
            }
        }
        else if (!isTryingToGrab && heldBag != null)
        {
            Release();
        }
    }

    private void LateUpdate()
    {
        if (heldBag != null && isGrabing)
        {
            if (heldBagComponent != null && heldBagComponent.IsPouring)
            {
                return;
            }

            heldBag.position = handPosition.transform.position;
            heldBag.rotation = handPosition.transform.rotation;
        }
    }
    private void Grab(Transform bagTransform)
    {
        isGrabing = true;
        heldBag = bagTransform;
        heldBagRigidbody = heldBag.GetComponent<Rigidbody>();
        heldBagComponent = heldBag.GetComponent<Bag>();

        heldBagRigidbody.isKinematic = true;
        heldBagRigidbody.interpolation = RigidbodyInterpolation.None;
        heldBag.gameObject.layer = LayerMask.NameToLayer(HELD_LAYER_NAME);

        if (heldBagComponent != null)
        {
            heldBagComponent.OnGrab();
        }

        PlayerStateMachine.IsHoldingItem = true;
    }

    private void Release()
    {
        if (heldBag == null)
        {
            ResetGrabState();
            return;
        }

        if (heldBagComponent != null)
        {
            heldBagComponent.OnRelease();
        }

        heldBagRigidbody.isKinematic = false;
        heldBagRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        heldBag.gameObject.layer = DEFAULT_LAYER;

        ResetGrabState();
    }

    private void ResetGrabState()
    {
        isGrabing = false;
        heldBag = null;
        heldBagRigidbody = null;
        heldBagComponent = null;
        PlayerStateMachine.IsHoldingItem = false;
    }

    /// <summary>Release the currently held bag without dropping it. Used during game reset.</summary>
    public void ReleaseHeldItem()
    {
        if (heldBag != null)
        {
            if (heldBagComponent != null)
            {
                heldBagComponent.OnRelease();
            }
            if (heldBagRigidbody != null)
            {
                heldBagRigidbody.isKinematic = false;
                heldBagRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
            heldBag.gameObject.layer = DEFAULT_LAYER;
        }
        ResetGrabState();
    }
}
