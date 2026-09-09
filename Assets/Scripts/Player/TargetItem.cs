using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TargetItem : MonoBehaviour
{
    private const float NON_FUNCTIONAL_DROP_DELAY = 3f;

    private string targetFlag;
    private bool hasFlagToSet;
    private Rigidbody rb;
    private bool isGrabbed;
    private Collider itemCollider;
    private Coroutine moveCoroutine;

    [Header("Animation de grab")]
    [SerializeField] private float grabMoveDuration = 0.3f;

    [SerializeField] private ParticleSystem etincelle;

    [Header("Main utilisé")]
    [SerializeField] private bool useLeftHand = false;
    public bool UseLeftHand => useLeftHand;

    public event System.Action OnPickedUp;


    public void Init(string flagToSetOnPickup)
    {
        targetFlag = flagToSetOnPickup;
        hasFlagToSet = !string.IsNullOrEmpty(flagToSetOnPickup);
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    public void OnGrabbed(Transform holdPoint)
    {
        if (isGrabbed)
            return;

        isGrabbed = true;

        OnPickedUp?.Invoke();

        // Picking up a valid item clears any active penalty
        PenaltyManager.Instance?.ClearCurrentPenalty();

        if (hasFlagToSet)
        {
            GameFlags.SetFlag(targetFlag);
        }
        else
        {
            // Non-functional item: warn the player and auto-drop after a delay
            if (InnerVoiceManager.Instance != null)
            {
                InnerVoiceManager.Instance.Show("<shake>Ce briquet ne marche pas... Il me faut un autre.</shake>");
            }
            StartCoroutine(AutoDropAfterDelay(holdPoint));
        }

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        transform.SetParent(holdPoint);
        //transform.localPosition = Vector3.zero;
        //transform.localRotation = Quaternion.identity;

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        moveCoroutine = StartCoroutine(MoveToHand());
    }

    private IEnumerator MoveToHand()
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRotation = transform.localRotation;

        Vector3 targetPos = Vector3.zero;
        Quaternion targetRotation = Quaternion.identity;

        float elapsed = 0;

        while (elapsed < grabMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / grabMoveDuration;

            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRotation;

        if (etincelle != null)
        {
            etincelle.Play();
        }
    }

    /// <summary>Drop a non-functional item to the floor so the player can grab another one.</summary>
    private IEnumerator AutoDropAfterDelay(Transform holdPoint)
    {
        yield return new WaitForSeconds(NON_FUNCTIONAL_DROP_DELAY);

        // Detach from the player
        transform.SetParent(null);

        // Place in front of the player at a reasonable drop height
        Vector3 dropPos = holdPoint.position + holdPoint.forward * 0.5f;
        dropPos.y = Mathf.Max(dropPos.y, 0.5f);
        transform.position = dropPos;
        transform.rotation = Random.rotation;

        // Enable physics so the item falls to the floor
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(holdPoint.forward * 2f, ForceMode.Impulse);

        // Non-convex MeshColliders are incompatible with non-kinematic Rigidbodies
        MeshCollider meshCol = itemCollider as MeshCollider;
        if (meshCol != null && !meshCol.convex)
        {
            meshCol.convex = true;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        // Hide the "doesn't work" inner voice now that the item is dropped
        InnerVoiceManager.Instance?.Hide();

        // Let the player grab another item
        PickupSystem pickupSystem = holdPoint.GetComponentInParent<PickupSystem>();
        if (pickupSystem != null)
        {
            pickupSystem.ReleaseHeldItemRight();
        }

        // Allow this item to be picked up again
        isGrabbed = false;
    }
}
