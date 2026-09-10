using System.Collections;
using UnityEngine;

public class drawer_slide : MonoBehaviour
{
    public enum SlideAxis { X, Y, Z }

    public float slideDistance = 0.5f;

    public SlideAxis slideAxis = SlideAxis.Z;

    public float duration = 1f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine currentCoroutine;

    void Awake()
    {
        closedPosition = transform.localPosition;

        Vector3 lOffset = Vector3.zero;
        switch (slideAxis)
        {
            case SlideAxis.X: lOffset = new Vector3(slideDistance, 0f, 0f); break;
            case SlideAxis.Y: lOffset = new Vector3(0f, slideDistance, 0f); break;
            case SlideAxis.Z: lOffset = new Vector3(0f, 0f, slideDistance); break;
        }
        openPosition = closedPosition + lOffset;
    }

    public void ToggleDoor()
    {
        if (isAnimating) return;

        isOpen = !isOpen;
        Vector3 lTarget = isOpen ? openPosition : closedPosition;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SlideDrawer(lTarget));
    }

    public void ForceClose()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        isOpen = false;
        isAnimating = false;
        transform.localPosition = closedPosition;
    }

    private IEnumerator SlideDrawer(Vector3 pTarget)
    {
        isAnimating = true;
        Vector3 lStartPos = transform.localPosition;
        float lElapsed = 0f;

        while (lElapsed < duration)
        {
            lElapsed += Time.deltaTime;
            float lT = lElapsed / duration;
            transform.localPosition = Vector3.Lerp(lStartPos, pTarget, lT);
            yield return null;
        }

        transform.localPosition = pTarget;
        isAnimating = false;
    }
}
