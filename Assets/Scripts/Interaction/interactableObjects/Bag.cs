using System.Collections;
using UnityEngine;

public class Bag : MonoBehaviour
{
    private const int DEFAULT_QUANTITY = 3;
    private const float POUR_INTERVAL = 1f;
    private float pourElapsed = 0f;
    private float stopOffset = 0.8f;

    [SerializeField] private int quantity = DEFAULT_QUANTITY;
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float tiltAngle = 80f;
    [SerializeField] private float maxTiltAngle = 80f;
    [SerializeField] private float moveDuration = 1.5f;

    private Coroutine pouringCoroutine;

    public bool IsHeld { get; private set; }
    public bool IsPouring { get; private set; }

    public void OnGrab()
    {
        IsHeld = true;
    }

    public void OnRelease()
    {
        IsHeld = false;
        StopPouring();
    }
    public void StartPouring(BagRecepter pRecepter)
    {
        if (IsPouring) return;
        IsPouring = true;
        pouringCoroutine = StartCoroutine(PourCoroutine(pRecepter));
    }

    public void StopPouring()
    {
        if (pouringCoroutine != null)
        {
            StopCoroutine(pouringCoroutine);
            pouringCoroutine = null;
        }
        IsPouring = false;
    }

    private IEnumerator PourCoroutine(BagRecepter pRecepter)
    {
        Transform lPourPoint = pRecepter.PourPoint;
        Vector3 lStartPos = transform.position;
        float lProgress = 0f;
        int lInitialQuantity = quantity;
        float lTimer = 0f;
        Vector3 lPourDirection = (lPourPoint.position - transform.position).normalized;
        Vector3 lStopPosition = lPourPoint.position - lPourDirection * stopOffset;


        if (lPourPoint == null)
        {
            IsPouring = false;
            yield break;
        }

        while (lProgress < 1f)
        {
            lProgress += Time.deltaTime / moveDuration;
            float lSin = 1f - Mathf.Cos(lProgress * Mathf.PI * 0.5f);
            transform.position = Vector3.Lerp(lStartPos, lPourPoint.position,lSin);
            yield return null;
        }

        Quaternion lArrivalRotation = transform.rotation;
        Vector3 lTiltAxis = Vector3.Cross(Vector3.up, lPourDirection).normalized;

        while (quantity > 0 && IsPouring)
        {
            lTimer += Time.deltaTime;
            pourElapsed += Time.deltaTime;

            float lTotalPourTime = lInitialQuantity * POUR_INTERVAL;
            float lTiltProgress = Mathf.Clamp01(pourElapsed / lTotalPourTime);
            float lCurrentTilt = Mathf.Lerp(0f, maxTiltAngle, lTiltProgress);
            transform.rotation = Quaternion.AngleAxis(lCurrentTilt, lTiltAxis) * lArrivalRotation;


            if (lTimer >= POUR_INTERVAL)
            {
                lTimer = 0f;
                pRecepter.ReceiveContent(1);
                quantity--;
            }

            yield return null;
        }

        if (quantity <= 0)
        {
            IsPouring = false;
            IsHeld = false;
            Destroy(gameObject);
        }
        else
        {
            IsPouring = false;
        }
    }
}
