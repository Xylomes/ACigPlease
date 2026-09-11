using System.Collections;
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

    [SerializeField] private float grabMoveDuration = 0.3f;

    [SerializeField] private ParticleSystem etincelle;

    [SerializeField] private bool useLeftHand = false;
    public bool UseLeftHand => useLeftHand;

    public event System.Action OnPickedUp;


    public void Init(string pFlagToSetOnPickup)
    {
        targetFlag = pFlagToSetOnPickup;
        hasFlagToSet = !string.IsNullOrEmpty(pFlagToSetOnPickup);
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    public void OnGrabbed(Transform pHoldPoint)
    {
        if (isGrabbed)
            return;

        isGrabbed = true;

        OnPickedUp?.Invoke();

        PenaltyManager.Instance?.ClearCurrentPenalty();

        if (hasFlagToSet)
        {
            GameFlags.SetFlag(targetFlag);
        }
        else
        {
            if (InnerVoiceManager.Instance != null)
            {
                InnerVoiceManager.Instance.Show("<shake>Ce briquet ne marche pas... Il me faut un autre.</shake>");
            }
            StartCoroutine(AutoDropAfterDelay(pHoldPoint));
        }

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        transform.SetParent(pHoldPoint);

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        moveCoroutine = StartCoroutine(MoveToHand());
    }

    private IEnumerator MoveToHand()
    {
        Vector3 lStartPos = transform.localPosition;
        Quaternion lStartRotation = transform.localRotation;

        Vector3 lTargetPos = Vector3.zero;
        Quaternion lTargetRotation = Quaternion.identity;

        float lElapsed = 0;

        while (lElapsed < grabMoveDuration)
        {
            lElapsed += Time.deltaTime;
            float lT = lElapsed / grabMoveDuration;

            transform.localPosition = Vector3.Lerp(lStartPos, lTargetPos, lT);
            transform.localRotation = Quaternion.Slerp(lStartRotation, lTargetRotation, lT);

            yield return null;
        }

        transform.localPosition = lTargetPos;
        transform.localRotation = lTargetRotation;

        if (etincelle != null)
        {
            etincelle.Play();
            SoundManager.Instance.PlaySFX(SoundManager.SoundType.Rat);
        }
    }

    private IEnumerator AutoDropAfterDelay(Transform pHoldPoint)
    {
        yield return new WaitForSeconds(NON_FUNCTIONAL_DROP_DELAY);

        transform.SetParent(null);

        Vector3 lDropPos = pHoldPoint.position + pHoldPoint.forward * 0.5f;
        lDropPos.y = Mathf.Max(lDropPos.y, 0.5f);
        transform.position = lDropPos;
        transform.rotation = Random.rotation;

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(pHoldPoint.forward * 2f, ForceMode.Impulse);

        MeshCollider lMeshCol = itemCollider as MeshCollider;
        if (lMeshCol != null && !lMeshCol.convex)
        {
            lMeshCol.convex = true;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        InnerVoiceManager.Instance?.Hide();

        PickupSystem lPickupSystem = pHoldPoint.GetComponentInParent<PickupSystem>();
        if (lPickupSystem != null)
        {
            lPickupSystem.ReleaseHeldItemRight();
        }

        isGrabbed = false;
    }
}
